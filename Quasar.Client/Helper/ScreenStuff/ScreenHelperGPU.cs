using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;
using Resource = SharpDX.Direct3D11.Resource;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using Quasar.Client.Config;

namespace Quasar.Client.Helper
{
    public static class ScreenHelperGPU
    {
        private static Device _device;
        private static OutputDuplication[] _duplicatedOutputs;
        private static Texture2D _stagingTexture;
        private static readonly object _lockObject = new object();
        private static bool _initialized = false;

        private const int CURSOR_SHOWING = 0x00000001;
        private static readonly int CursorInfoSize = Marshal.SizeOf(typeof(CURSORINFO));

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CURSORINFO
        {
            public int cbSize;
            public int flags;
            public IntPtr hCursor;
            public POINT ScreenPosition;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorInfo(out CURSORINFO pci);

        [DllImport("user32.dll")]
        private static extern bool DrawIcon(IntPtr hDC, int X, int Y, IntPtr hIcon);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetThreadDesktop(IntPtr hDesktop);

        /// <summary>
        /// Initializes the GPU-based screen capture system.
        /// </summary>
        private static void Initialize()
        {
            try
            {
                _device = new Device(SharpDX.Direct3D.DriverType.Hardware,
                    DeviceCreationFlags.BgraSupport | DeviceCreationFlags.SingleThreaded);

                using (var adapter = _device.QueryInterface<SharpDX.DXGI.Device>().GetParent<Adapter>())
                using (var factory = adapter.GetParent<Factory1>())
                {
                    // get all monitors
                    var outputs = new System.Collections.Generic.List<Output>();
                    var adapters = new System.Collections.Generic.List<Adapter1>();

                    // go through all monitors
                    for (int i = 0; i < factory.GetAdapterCount(); i++)
                    {
                        using (var adapter1 = factory.GetAdapter1(i))
                        {
                            adapters.Add(adapter1);

                            // get all outputs for them
                            for (int j = 0; j < adapter1.GetOutputCount(); j++)
                            {
                                Output tempOutput = adapter1.GetOutput(j);
                                outputs.Add(tempOutput);
                            }
                        }
                    }

                    // duplication bs
                    _duplicatedOutputs = new OutputDuplication[outputs.Count];

                    for (int i = 0; i < outputs.Count; i++)
                    {
                        Output1 output1 = outputs[i].QueryInterface<Output1>();
                        _duplicatedOutputs[i] = output1.DuplicateOutput(_device);
                    }

                    // clean
                    foreach (var output in outputs)
                    {
                        output.Dispose();
                    }

                    foreach (var adapterItem in adapters)
                    {
                        adapterItem.Dispose();
                    }
                }

                _initialized = true;
                Debug.WriteLine("GPU Screen Helper initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to initialize GPU Screen Helper: {ex.Message}");
                CleanupResources();
                throw;
            }
        }

        /// <summary>
        /// Disposes and cleans up DirectX resources
        /// </summary>
        public static void CleanupResources()
        {
            lock (_lockObject)
            {
                _stagingTexture?.Dispose();
                _stagingTexture = null;

                if (_duplicatedOutputs != null)
                {
                    foreach (var output in _duplicatedOutputs)
                    {
                        output?.Dispose();
                    }
                    _duplicatedOutputs = null;
                }

                _device?.Dispose();
                _device = null;

                _initialized = false;
            }
        }

        /// <summary>
        /// Captures the screen using GPU acceleration
        /// </summary>
        /// <param name="screenNumber">The index of the screen to capture</param>
        /// <param name="setThreadPointer">Whether to set the thread desktop pointer</param>
        /// <returns>A bitmap containing the captured screen</returns>
        public static Bitmap CaptureScreen(int screenNumber, bool setThreadPointer = false)
        {
            if (setThreadPointer)
            {
                SetThreadDesktop(Settings.OriginalDesktopPointer);
            }

            // imma be so fr claude cooked with everything under here idk nothing about DirectX

            lock (_lockObject)
            {
                try
                {
                    if (!_initialized)
                    {
                        Initialize();
                    }

                    // make sure we got the right screen number
                    if (_duplicatedOutputs == null || screenNumber >= _duplicatedOutputs.Length)
                    {
                        throw new ArgumentOutOfRangeException(nameof(screenNumber));
                    }

                    Rectangle bounds = GetBounds(screenNumber);

                    // do cpu if screen is in landscape mode
                    if (bounds.Height > bounds.Width)
                    {
                        //Debug.WriteLine("Screen is in landscape mode, using CPU capture");
                        return ScreenHelperCPU.CaptureScreen(screenNumber, false);
                    }

                    Bitmap resultBitmap = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);

                    OutputDuplication outputDuplication = _duplicatedOutputs[screenNumber];

                    SharpDX.DXGI.Resource desktopResource = null;
                    OutputDuplicateFrameInformation frameInfo;

                    try
                    {
                        // try to get the desktop image
                        // 0 timeout means return immediately even if no new frame
                        outputDuplication.AcquireNextFrame(100, out frameInfo, out desktopResource);

                        using (var texture2D = desktopResource.QueryInterface<Texture2D>())
                        {
                            // Create staging texture if needed or if size has changed
                            if (_stagingTexture == null ||
                                _stagingTexture.Description.Width != texture2D.Description.Width ||
                                _stagingTexture.Description.Height != texture2D.Description.Height)
                            {
                                _stagingTexture?.Dispose();

                                // Create a staging texture for CPU access
                                var desc = new Texture2DDescription
                                {
                                    CpuAccessFlags = CpuAccessFlags.Read,
                                    BindFlags = BindFlags.None,
                                    Format = texture2D.Description.Format,
                                    Width = texture2D.Description.Width,
                                    Height = texture2D.Description.Height,
                                    OptionFlags = ResourceOptionFlags.None,
                                    MipLevels = 1,
                                    ArraySize = 1,
                                    SampleDescription = { Count = 1, Quality = 0 },
                                    Usage = ResourceUsage.Staging
                                };

                                _stagingTexture = new Texture2D(_device, desc);
                            }

                            // Copy from GPU to staging texture
                            _device.ImmediateContext.CopyResource(texture2D, _stagingTexture);

                            // Map the staging texture to get access from CPU
                            var dataBox = _device.ImmediateContext.MapSubresource(
                                _stagingTexture,
                                0,
                                MapMode.Read,
                                MapFlags.None,
                                out DataStream dataStream);

                            // Copy data to bitmap
                            BitmapData bmpData = resultBitmap.LockBits(
                                new Rectangle(0, 0, resultBitmap.Width, resultBitmap.Height),
                                ImageLockMode.WriteOnly,
                                resultBitmap.PixelFormat);

                            // Copy only the region we need if capturing a specific area
                            int sourceStride = dataBox.RowPitch;
                            int destStride = bmpData.Stride;
                            IntPtr sourcePtr = dataStream.DataPointer;
                            IntPtr destPtr = bmpData.Scan0;

                            int xOffset = bounds.X;
                            int yOffset = bounds.Y;

                            for (int y = 0; y < bounds.Height; y++)
                            {
                                // Calculate source and destination addresses
                                IntPtr sourceRow = IntPtr.Add(sourcePtr, (y + yOffset) * sourceStride + xOffset * 4);
                                IntPtr destRow = IntPtr.Add(destPtr, y * destStride);

                                // Copy memory
                                memcpy(destRow, sourceRow, destStride);
                            }

                            // Unmap
                            _device.ImmediateContext.UnmapSubresource(_stagingTexture, 0);
                            resultBitmap.UnlockBits(bmpData);

                            // Draw cursor manually (DXGI doesn't capture cursors automatically)
                            using (Graphics g = Graphics.FromImage(resultBitmap))
                            {
                                IntPtr hdc = g.GetHdc();
                                DrawCursor(hdc, bounds);
                                g.ReleaseHdc(hdc);
                            }
                        }
                    }
                    finally
                    {
                        // Always release frame and resource
                        desktopResource?.Dispose();
                        outputDuplication?.ReleaseFrame();
                    }

                    return resultBitmap;
                }
                catch (SharpDXException ex) when (ex.ResultCode.Code == SharpDX.DXGI.ResultCode.WaitTimeout.Result.Code)
                {
                    Debug.WriteLine("GPU capture timed out, falling back to CPU capture");
                    return ScreenHelperCPU.CaptureScreen(screenNumber, false);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error capturing screen with GPU: {ex.Message}");
                    CleanupResources();
                    return ScreenHelperCPU.CaptureScreen(screenNumber, false);
                }
            }
        }

        private static void DrawCursor(IntPtr destDeviceContext, Rectangle bounds)
        {
            var cursorInfo = new CURSORINFO { cbSize = CursorInfoSize };
            if (GetCursorInfo(out cursorInfo) && cursorInfo.flags == CURSOR_SHOWING)
            {
                DrawIcon(destDeviceContext, cursorInfo.ScreenPosition.X - bounds.X, cursorInfo.ScreenPosition.Y - bounds.Y, cursorInfo.hCursor);
            }
        }

        /// <summary>
        /// Gets the bounds of the specified screen
        /// </summary>
        public static Rectangle GetBounds(int screenNumber)
        {
            return System.Windows.Forms.Screen.AllScreens[screenNumber].Bounds;
        }

        [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        private static extern IntPtr memcpy(IntPtr dest, IntPtr src, int count);
    }
}