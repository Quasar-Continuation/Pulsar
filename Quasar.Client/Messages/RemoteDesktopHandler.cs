using Quasar.Client.Helper;
using Quasar.Common.Enums;
using Quasar.Common.Messages;
using Quasar.Common.Networking;
using Quasar.Common.Video;
using Quasar.Common.Video.Codecs;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using Quasar.Common.Messages.Monitoring.RemoteDesktop;
using Quasar.Common.Messages.other;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Quasar.Client.Config;
using System.Collections.Generic;
using Quasar.Client.Utilities;
using System.Linq;

namespace Quasar.Client.Messages
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct WNDCLASS
    {
        public uint style;
        public WndProcDelegate lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public IntPtr hInstance;
        public IntPtr hIcon;
        public IntPtr hCursor;
        public IntPtr hbrBackground;
        [MarshalAs(UnmanagedType.LPStr)]
        public string lpszMenuName;
        [MarshalAs(UnmanagedType.LPStr)]
        public string lpszClassName;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BLENDFUNCTION
    {
        public byte BlendOp;
        public byte BlendFlags;
        public byte SourceConstantAlpha;
        public byte AlphaFormat;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SIZE
    {
        public int cx;
        public int cy;
    }

    public delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    public class RemoteDesktopHandler : NotificationMessageProcessor, IDisposable
    {
        #region P/Invoke
        
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetThreadDesktop(IntPtr hDesktop);
        
        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern bool ReleaseDC(IntPtr hwnd, IntPtr hdc);

        [DllImport("gdi32.dll")]
        static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

        [DllImport("gdi32.dll")]
        static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("gdi32.dll")]
        static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll")]
        static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop);

        [DllImport("user32.dll")]
        static extern bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, ref POINT pptDst, ref SIZE psize, IntPtr hdcSrc, ref POINT pptSrc, uint crKey, ref BLENDFUNCTION pblend, uint dwFlags);

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern bool DestroyWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr CreateWindowEx(uint dwExStyle, string lpClassName, string lpWindowName, uint dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        static extern System.UInt16 RegisterClassEx([In] ref WNDCLASSEX lpwcx);

        [DllImport("user32.dll")]
        static extern IntPtr GetDesktopWindow();

        [StructLayout(LayoutKind.Sequential)]
        struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct SIZE
        {
            public int cx;
            public int cy;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct BLENDFUNCTION
        {
            public byte BlendOp;
            public byte BlendFlags;
            public byte SourceConstantAlpha;
            public byte AlphaFormat;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct WNDCLASSEX
        {
            [MarshalAs(UnmanagedType.U4)]
            public int cbSize;
            [MarshalAs(UnmanagedType.U4)]
            public int style;
            public IntPtr lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            public string lpszMenuName;
            public string lpszClassName;
            public IntPtr hIconSm;
        }

        // Constants
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_LAYERED = 0x80000;
        private const int WS_EX_TRANSPARENT = 0x20;
        private const int WS_EX_TOPMOST = 0x8;
        private const int WS_EX_TOOLWINDOW = 0x80;
        private const uint WS_VISIBLE = 0x10000000;
        private const uint WS_POPUP = 0x80000000;
        private const int SW_SHOWNOACTIVATE = 4;
        private const int HWND_TOPMOST = -1;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint SWP_SHOWWINDOW = 0x0040;
        private const uint ULW_ALPHA = 0x00000002;
        private const byte AC_SRC_OVER = 0x00;
        private const byte AC_SRC_ALPHA = 0x01;
        private const uint SRCCOPY = 0x00CC0020;
        private const int LWA_COLORKEY = 0x1;
        private const int LWA_ALPHA = 0x2;
        private const uint WM_DESTROY = 0x0002;
        
        #endregion

        private UnsafeStreamCodec _streamCodec;
        private BitmapData _desktopData = null;
        private Bitmap _desktop = null;
        private int _displayIndex = 0;
        private ISender _clientMain;
        private Thread _captureThread;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _delegateInitialized = false;

        private ScreenOverlay _screenOverlay = new ScreenOverlay();
        private WndProcDelegate _wndProcDelegate;

        private bool _useGPU = false;

        // frame control variables
        private readonly ConcurrentQueue<byte[]> _frameBuffer = new ConcurrentQueue<byte[]>();
        private readonly AutoResetEvent _frameRequestEvent = new AutoResetEvent(false);
        private int _pendingFrameRequests = 0;

        private const int MAX_BUFFER_SIZE = 10;

        private readonly Stopwatch _stopwatch = new Stopwatch();
        private int _frameCount = 0;

        // drawing commands
        private readonly ConcurrentQueue<Action> _drawingQueue = new ConcurrentQueue<Action>();
        private readonly ManualResetEvent _drawingSignal = new ManualResetEvent(false);
        private Thread _drawingThread;
        private bool _drawingThreadRunning = false;

        public override bool CanExecute(IMessage message) => message is GetDesktop ||
                                                             message is DoMouseEvent ||
                                                             message is DoKeyboardEvent ||
                                                             message is DoDrawingEvent ||
                                                             message is GetMonitors;

        public override bool CanExecuteFrom(ISender sender) => true;

        public override void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case GetDesktop msg:
                    Execute(sender, msg);
                    break;
                case DoMouseEvent msg:
                    Execute(sender, msg);
                    break;
                case DoKeyboardEvent msg:
                    Execute(sender, msg);
                    break;
                case DoDrawingEvent msg:
                    _drawingQueue.Enqueue(() => Execute(sender, msg));
                    _drawingSignal.Set();
                    EnsureDrawingThreadIsRunning();
                    break;
                case GetMonitors msg:
                    Execute(sender, msg);
                    break;
            }
        }

        private void EnsureDrawingThreadIsRunning()
        {
            if (!_drawingThreadRunning)
            {
                _drawingThreadRunning = true;
                _drawingThread = new Thread(DrawingThreadProc);
                _drawingThread.IsBackground = true;
                _drawingThread.Start();
            }
        }

        private void DrawingThreadProc()
        {
            try
            {
                while (_drawingThreadRunning)
                {
                    _drawingSignal.WaitOne();

                    while (_drawingQueue.TryDequeue(out Action drawAction))
                    {
                        try
                        {
                            drawAction();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error processing drawing action: {ex.Message}");
                        }
                    }

                    _drawingSignal.Reset();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in drawing thread: {ex.Message}");
            }
            finally
            {
                _drawingThreadRunning = false;
            }
        }

        private void Execute(ISender client, GetDesktop message)
        {
            if (message.Status == RemoteDesktopStatus.Stop)
            {
                StopScreenStreaming();
            }
            else if (message.Status == RemoteDesktopStatus.Start)
            {
                StartScreenStreaming(client, message);
            }
            else if (message.Status == RemoteDesktopStatus.Continue)
            {
                // server is requesting more frames
                Interlocked.Add(ref _pendingFrameRequests, message.FramesRequested);
                _frameRequestEvent.Set();

                Debug.WriteLine($"Server requested {message.FramesRequested} more frames. Total pending: {_pendingFrameRequests}");
            }
        }

        private void StartScreenStreaming(ISender client, GetDesktop message)
        {
            Debug.WriteLine("Starting remote desktop session");

            var monitorBounds = ScreenHelperCPU.GetBounds(message.DisplayIndex);
            var resolution = new Resolution { Height = monitorBounds.Height, Width = monitorBounds.Width };

            if (_streamCodec == null)
                _streamCodec = new UnsafeStreamCodec(message.Quality, message.DisplayIndex, resolution);

            if (message.CreateNew)
            {
                _streamCodec?.Dispose();
                _streamCodec = new UnsafeStreamCodec(message.Quality, message.DisplayIndex, resolution);
                OnReport("Remote desktop session started");
            }

            if (_streamCodec.ImageQuality != message.Quality || _streamCodec.Monitor != message.DisplayIndex || _streamCodec.Resolution != resolution)
            {
                _streamCodec?.Dispose();
                _streamCodec = new UnsafeStreamCodec(message.Quality, message.DisplayIndex, resolution);
            }

            _displayIndex = message.DisplayIndex;
            _clientMain = client;

            _useGPU = message.UseGPU;
            Debug.WriteLine("Use GPU: " + _useGPU);

            // clear any pending frame requests and existing frames
            ClearFrameBuffer();
            Interlocked.Exchange(ref _pendingFrameRequests, message.FramesRequested);

            if (_captureThread == null || !_captureThread.IsAlive)
            {
                _cancellationTokenSource = new CancellationTokenSource();
                _captureThread = new Thread(() => BufferedCaptureLoop(_cancellationTokenSource.Token));
                _captureThread.Start();
            }
        }

        private void StopScreenStreaming()
        {
            Debug.WriteLine("Stopping remote desktop session");

            _cancellationTokenSource?.Cancel();

            if (_captureThread != null && _captureThread.IsAlive)
            {
                _frameRequestEvent.Set(); // wake up thread
                _captureThread.Join();
                _captureThread = null;
            }

            if (_desktop != null)
            {
                if (_desktopData != null)
                {
                    try
                    {
                        _desktop.UnlockBits(_desktopData);
                    }
                    catch
                    {
                    }
                    _desktopData = null;
                }
                _desktop.Dispose();
                _desktop = null;
            }

            if (_streamCodec != null)
            {
                _streamCodec.Dispose();
                _streamCodec = null;
            }

            // clean up gpu bs
            if (_useGPU)
            {
                ScreenHelperGPU.CleanupResources();
            }

            // clear the buff
            ClearFrameBuffer();
            Interlocked.Exchange(ref _pendingFrameRequests, 0);
        }

        private void BufferedCaptureLoop(CancellationToken cancellationToken)
        {
            Debug.WriteLine("Starting buffered capture loop");
            _stopwatch.Start();

            bool success = SetThreadDesktop(Settings.OriginalDesktopPointer);
            if (!success)
            {
                Debug.WriteLine($"Failed to set capture thread to original desktop: {Marshal.GetLastWin32Error()}");
                return;
            }

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // wait for frame requests if the buffer is full or no frames are requested
                    if (_frameBuffer.Count >= MAX_BUFFER_SIZE || _pendingFrameRequests <= 0)
                    {
                        Debug.WriteLine($"Waiting for frame requests. Buffer size: {_frameBuffer.Count}, Pending requests: {_pendingFrameRequests}");
                        _frameRequestEvent.WaitOne(500);

                        // if cancellation was requested during the wait
                        if (cancellationToken.IsCancellationRequested)
                            break;

                        continue;
                    }

                    // capture frame and add to buffer
                    byte[] frameData = CaptureFrame();
                    if (frameData != null)
                    {
                        _frameBuffer.Enqueue(frameData);

                        // increment frame counter for statistics
                        _frameCount++;
                        if (_stopwatch.ElapsedMilliseconds >= 1000)
                        {
                            Debug.WriteLine($"Capture FPS: {_frameCount}, Buffer size: {_frameBuffer.Count}, Pending requests: {_pendingFrameRequests}");
                            _frameCount = 0;
                            _stopwatch.Restart();
                        }
                    }

                    // send frames if we have pending requests
                    while (_pendingFrameRequests > 0 && _frameBuffer.TryDequeue(out byte[] frameToSend))
                    {
                        SendFrameToServer(frameToSend, Interlocked.Decrement(ref _pendingFrameRequests) == 0);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error in buffered capture loop: {ex.Message}");
                    Thread.Sleep(100); // Avoid tight loop in case of repeated errors
                }
            }

            Debug.WriteLine("Buffered capture loop ended");
        }

        private byte[] CaptureFrame()
        {
            try
            {
                if (_useGPU)
                    _desktop = ScreenHelperGPU.CaptureScreen(_displayIndex);
                else
                _desktop = ScreenHelperCPU.CaptureScreen(_displayIndex);

                if (_desktop == null)
                {
                    Debug.WriteLine("Error capturing screen: Bitmap is null");
                    return null;
                }

                _desktopData = _desktop.LockBits(new Rectangle(0, 0, _desktop.Width, _desktop.Height),
                    ImageLockMode.ReadWrite, _desktop.PixelFormat);

                using (MemoryStream stream = new MemoryStream())
                {
                    if (_streamCodec == null) throw new Exception("StreamCodec can not be null.");
                    _streamCodec.CodeImage(_desktopData.Scan0,
                        new Rectangle(0, 0, _desktop.Width, _desktop.Height),
                        new Size(_desktop.Width, _desktop.Height),
                        _desktop.PixelFormat, stream);

                    return stream.ToArray();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error capturing frame: {ex.Message}");
                return null;
            }
            finally
            {
                if (_desktopData != null)
                {
                    _desktop.UnlockBits(_desktopData);
                    _desktopData = null;
                }
                _desktop?.Dispose();
                _desktop = null;
            }
        }

        private void SendFrameToServer(byte[] frameData, bool isLastRequestedFrame)
        {
            if (frameData == null || _clientMain == null) return;

            try
            {
                _clientMain.Send(new GetDesktopResponse
                {
                    Image = frameData,
                    Quality = _streamCodec.ImageQuality,
                    Monitor = _streamCodec.Monitor,
                    Resolution = _streamCodec.Resolution,
                    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    IsLastRequestedFrame = isLastRequestedFrame
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error sending frame to server: {ex.Message}");
            }
        }

        private void ClearFrameBuffer()
        {
            while (_frameBuffer.TryDequeue(out _)) { }
        }

        private void Execute(ISender sender, DoMouseEvent message)
        {
            try
            {
                Screen[] allScreens = Screen.AllScreens;
                int offsetX = allScreens[message.MonitorIndex].Bounds.X;
                int offsetY = allScreens[message.MonitorIndex].Bounds.Y;
                Point p = new Point(message.X + offsetX, message.Y + offsetY);

                // Disable screensaver if active before input
                if (NativeMethodsHelper.IsScreensaverActive())
                    NativeMethodsHelper.DisableScreensaver();

                switch (message.Action)
                {
                    case MouseAction.LeftDown:
                    case MouseAction.LeftUp:
                        NativeMethodsHelper.DoMouseLeftClick(p, message.IsMouseDown);
                        break;
                    case MouseAction.RightDown:
                    case MouseAction.RightUp:
                        NativeMethodsHelper.DoMouseRightClick(p, message.IsMouseDown);
                        break;
                    case MouseAction.MoveCursor:
                        NativeMethodsHelper.DoMouseMove(p);
                        break;
                    case MouseAction.ScrollDown:
                        NativeMethodsHelper.DoMouseScroll(p, true);
                        break;
                    case MouseAction.ScrollUp:
                        NativeMethodsHelper.DoMouseScroll(p, false);
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error executing mouse event: {ex.Message}");
            }
        }

        private void Execute(ISender sender, DoKeyboardEvent message)
        {
            try
            {
                if (NativeMethodsHelper.IsScreensaverActive())
                    NativeMethodsHelper.DisableScreensaver();

                NativeMethodsHelper.DoKeyPress(message.Key, message.KeyDown);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error executing keyboard event: {ex.Message}");
            }
        }
        
        private void Execute(ISender sender, DoDrawingEvent message)
        {
            try
            {
                if (message.IsClearAll)
                {
                    _screenOverlay.ClearDrawings(message.MonitorIndex);
                }
                else if (message.IsEraser)
                {
                    _screenOverlay.DrawEraser(
                        message.PrevX, message.PrevY,
                        message.X, message.Y,
                        message.StrokeWidth, message.MonitorIndex);
                }
                else
                {
                    _screenOverlay.Draw(
                        message.PrevX, message.PrevY,
                        message.X, message.Y,
                        message.StrokeWidth, message.ColorArgb, message.MonitorIndex);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error processing drawing event: {ex.Message}");
            }
        }

        private void Execute(ISender client, GetMonitors message)
        {
            int screenCountTotal = DisplayManager.GetDisplayCount();

            Debug.WriteLine(screenCountTotal);

            client.Send(new GetMonitorsResponse { Number = screenCountTotal });
        }

        /// <summary>
        /// Disposes all managed and unmanaged resources associated with this message processor.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                StopScreenStreaming();
                _streamCodec?.Dispose();
                _cancellationTokenSource?.Dispose();
                _frameRequestEvent?.Dispose();
                
                _drawingThreadRunning = false;
                _drawingSignal.Set();
                if (_drawingThread != null && _drawingThread.IsAlive)
                {
                    _drawingThread.Join(1000);
                }
                _drawingSignal.Dispose();
                _screenOverlay?.Dispose();

                // Clean up GPU resources
                if (_useGPU)
                {
                    ScreenHelperGPU.CleanupResources();
                }
            }
        }
    }
}