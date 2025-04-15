using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Threading.Tasks;
using Buffer = SharpDX.Direct3D11.Buffer;
using Color = System.Drawing.Color;
using Device = SharpDX.Direct3D11.Device;
using Rectangle = System.Drawing.Rectangle;
using System.Drawing;
using System.Drawing.Imaging;
namespace Pulsar.Client.Utilities
{
    /// <summary>
    /// This class provides functions for creating drawing overlays on the screen using Direct3D 11.
    /// 
    /// Usage:
    /// 1. Create an instance of ScreenOverlay
    ///    ScreenOverlay overlay = new ScreenOverlay();
    /// 
    /// 2. Draw points on the screen:
    ///    // Draw a point at coordinates (100, 100) with 5px width and red color on monitor 0
    ///    overlay.Draw(100, 100, 200, 200, 5, Color.Red.ToArgb(), 0);
    ///    
    ///    // Each Draw call creates a new point without connecting lines
    ///    // This creates another point at the specified location
    ///    overlay.Draw(200, 200, 300, 150, 5, Color.Red.ToArgb(), 0);
    /// 
    /// 3. Erase parts of the drawing:
    ///    // Erase area from (150, 150) to (250, 250) with 20px eraser on monitor 0
    ///    overlay.DrawEraser(150, 150, 250, 250, 20, 0);
    /// 
    /// 4. Clear all drawings on a monitor:
    ///    overlay.ClearDrawings(0);
    /// 
    /// 5. Create, move, and rotate 3D cubes:
    ///    // Create a 3D cube with different colors for each face
    ///    overlay.Render3DCube(1, Color.Red.ToArgb(), Color.Blue.ToArgb(), Color.Green.ToArgb(),
    ///                          Color.Yellow.ToArgb(), Color.Purple.ToArgb(), Color.Orange.ToArgb(),
    ///                          100, 100, 0, 50, 50);
    ///                          
    ///    // Move the cube smoothly to a new position
    ///    overlay.Move3DCube(1, true, 200, 200, 0);
    ///    
    ///    // Rotate the cube (in radians)
    ///    overlay.Rotate3DCube(1, true, 0.5f, 1.0f, 0.25f);
    ///    
    ///    // Remove the cube when done
    ///    overlay.Remove3DCube(1);
    ///
    /// 6. Capture screen information:
    ///    // Get pixel color at specific coordinates
    ///    PixelData pixel = overlay.GetPixelCoords(100, 100);
    ///    
    ///    // Get a random pixel from the screen
    ///    PixelData randomPixel = overlay.GetRandomPixel();
    ///    
    ///    // Capture a region of the screen
    ///    Bitmap screenshot = overlay.CaptureScreenRegion(50, 50, 200, 150);
    ///
    /// 7. Custom Rendering:
    ///    // Create and register custom shaders and objects that get rendered on the screen
    ///    // Implement ICustomShader and ICustomRenderable interfaces
    ///    // Then register them with the overlay:
    ///    overlay.RegisterCustomShader(myShader);
    ///    overlay.RegisterCustomRenderable(myRenderable);
    ///    
    ///    // Get DirectX device for more advanced operations
    ///    Device device = overlay.GetDeviceForMonitor(0);
    /// 
    /// 8. Properly dispose when done:
    ///    overlay.Dispose();
    /// 
    /// All drawing operations are performed asynchronously on a background thread for performance.
    /// The overlay automatically creates transparent windows that appear on top of everything else except the cursor,
    /// making it suitable for drawing visual elements on any part of the screen.
    /// 
    /// This implementation uses Direct3D 11. Points are rendered as quads.
    /// 
    /// Monitor indices correspond to the indices in Screen.AllScreens[], use 0 for primary monitor.
    /// </summary>
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
    [StructLayout(LayoutKind.Sequential)]
    public struct PixelData
    {
        public Color Color;
        public int X;
        public int Y;
        public int MonitorIndex;
        public PixelData(Color color, int x, int y, int monitorIndex)
        {
            Color = color;
            X = x;
            Y = y;
            MonitorIndex = monitorIndex;
        }
    }
    public delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
    /// <summary>
    /// For creating transparent DX overlays on the screen
    /// </summary>
    public class ScreenOverlay : IDisposable
    {
        private Dictionary<int, IntPtr> _overlayWindows = new Dictionary<int, IntPtr>();
        private Dictionary<int, Device> _devices = new Dictionary<int, Device>();
        private Dictionary<int, DeviceContext> _deviceContexts = new Dictionary<int, DeviceContext>();
        private Dictionary<int, SwapChain> _swapChains = new Dictionary<int, SwapChain>();
        private Dictionary<int, RenderTargetView> _renderTargetViews = new Dictionary<int, RenderTargetView>();
        private Dictionary<int, ShaderResourceView> _shaderResourceViews = new Dictionary<int, ShaderResourceView>();
        private Dictionary<int, Texture2D> _renderTextures = new Dictionary<int, Texture2D>();
        private Dictionary<int, bool> _needsUpdate = new Dictionary<int, bool>();
        private Dictionary<int, VertexShader> _vertexShaders = new Dictionary<int, VertexShader>();
        private Dictionary<int, PixelShader> _pixelShaders = new Dictionary<int, PixelShader>();
        private Dictionary<int, InputLayout> _inputLayouts = new Dictionary<int, InputLayout>();
        private Dictionary<int, Buffer> _vertexBuffers = new Dictionary<int, Buffer>();
        private Dictionary<int, VertexShader> _vertex3DShaders = new Dictionary<int, VertexShader>();
        private Dictionary<int, PixelShader> _pixel3DShaders = new Dictionary<int, PixelShader>();
        private Dictionary<int, InputLayout> _input3DLayouts = new Dictionary<int, InputLayout>();
        private Dictionary<int, Buffer> _matrixBuffers = new Dictionary<int, Buffer>();
        private Dictionary<int, Dictionary<int, Cube3DData>> _cubes = new Dictionary<int, Dictionary<int, Cube3DData>>();
        private WndProcDelegate _wndProcDelegate;
        private bool _delegateInit = false;
        private ConcurrentQueue<Action> _drawingQueue = new ConcurrentQueue<Action>();
        private ManualResetEvent _drawingSignal = new ManualResetEvent(false);
        private bool _drawingThreadRunning = false;
        private Thread _drawingThread;
        private const int MAX_UPDATE_RATE_MS = 16;
        private const string VertexShaderCode = @"
            struct VS_INPUT
            {
                float2 pos : POSITION;
                float4 color : COLOR;
                float size : SIZE;
                uint vertexID : SV_VertexID;
            };
            struct VS_OUTPUT
            {
                float4 pos : SV_POSITION;
                float4 color : COLOR;
            };
            VS_OUTPUT main(VS_INPUT input)
            {
                VS_OUTPUT output;
                float size = input.size * 0.5f;
                float2 offset = float2(0, 0);
                if (input.vertexID % 4 == 0) offset = float2(-size, -size);
                else if (input.vertexID % 4 == 1) offset = float2(size, -size);
                else if (input.vertexID % 4 == 2) offset = float2(-size, size);
                else if (input.vertexID % 4 == 3) offset = float2(size, size);
                output.pos = float4(input.pos + offset, 0.0f, 1.0f);
                output.color = input.color;
                return output;
            }";
        private const string PixelShaderCode = @"
            struct PS_INPUT
            {
                float4 pos : SV_POSITION;
                float4 color : COLOR;
            };
            float4 main(PS_INPUT input) : SV_TARGET
            {
                return input.color;
            }";
        private const string Vertex3DShaderCode = @"
cbuffer MatrixBuffer : register(b0)
{
    matrix worldMatrix;
    matrix viewMatrix;
    matrix projectionMatrix;
};
struct VS_INPUT
{
    float3 position : POSITION;
    float4 color : COLOR;
};
struct VS_OUTPUT
{
    float4 position : SV_POSITION;
    float4 color : COLOR;
};
VS_OUTPUT main(VS_INPUT input)
{
    VS_OUTPUT output;
    float4 pos = float4(input.position, 1.0f);
    pos = mul(pos, worldMatrix);
    pos = mul(pos, viewMatrix);
    pos = mul(pos, projectionMatrix);
    output.position = pos;
    output.color = input.color;
    return output;
}";
        private const string Pixel3DShaderCode = @"
struct PS_INPUT
{
    float4 position : SV_POSITION;
    float4 color : COLOR;
};
float4 main(PS_INPUT input) : SV_TARGET
{
    return input.color;
}";
        [StructLayout(LayoutKind.Sequential)]
        private struct VertexPositionColor
        {
            public Vector2 Position;
            public Color4 Color;
            public float Size;
            public VertexPositionColor(Vector2 position, Color4 color, float size)
            {
                Position = position;
                Color = color;
                Size = size;
            }
            public static int SizeInBytes => Marshal.SizeOf<VertexPositionColor>();
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct VertexPosition3DColor
        {
            public Vector3 Position;
            public Color4 Color;
            public VertexPosition3DColor(Vector3 position, Color4 color)
            {
                Position = position;
                Color = color;
            }
            public static int SizeInBytes => Marshal.SizeOf<VertexPosition3DColor>();
        }
        private class MonitorDrawingData
        {
            public List<VertexPositionColor> Points { get; private set; } = new List<VertexPositionColor>();
            public int[] Indices { get; private set; } = new int[0];
            public int PointCount => Points.Count;
            private int _bufferCapacity;
            private readonly int _initialCapacity;
            public MonitorDrawingData(int initialCapacity = 1000)
            {
                _initialCapacity = initialCapacity;
                _bufferCapacity = initialCapacity;
                Indices = new int[initialCapacity * 6];
            }
            public void AddPoint(VertexPositionColor point)
            {
                Points.Add(point);
                if (Points.Count > _bufferCapacity)
                {
                    _bufferCapacity = Points.Count * 2;
                    Debug.WriteLine($"Growing buffer capacity to {_bufferCapacity} points");
                }
                RebuildIndices();
            }
            public void Clear()
            {
                Points.Clear();
                _bufferCapacity = _initialCapacity;
            }
            public void RebuildIndices()
            {
                int pointCount = Points.Count;
                if (Indices.Length < pointCount * 6)
                {
                    Indices = new int[pointCount * 6];
                }
                for (int i = 0; i < pointCount; i++)
                {
                    int baseIndex = i * 4;
                    int indexOffset = i * 6;
                    Indices[indexOffset] = baseIndex;
                    Indices[indexOffset + 1] = baseIndex + 1;
                    Indices[indexOffset + 2] = baseIndex + 2;
                    Indices[indexOffset + 3] = baseIndex + 1;
                    Indices[indexOffset + 4] = baseIndex + 3;
                    Indices[indexOffset + 5] = baseIndex + 2;
                }
            }
            public int GetUsedIndexCount() => Math.Min(Points.Count * 6, Indices.Length);
        }
        private Dictionary<int, MonitorDrawingData> _drawingData = new Dictionary<int, MonitorDrawingData>();
        private Dictionary<int, Buffer> _indexBuffers = new Dictionary<int, Buffer>();
        private Dictionary<int, int> _bufferSizes = new Dictionary<int, int>();
        private const int INITIAL_BUFFER_SIZE = 1000;
        private Vector3 _cameraPosition = new Vector3(0, 0, -5);
        private Vector3 _cameraTarget = new Vector3(0, 0, 0);
        private Vector3 _cameraUp = new Vector3(0, 1, 0);
        private float _fieldOfView = (float)Math.PI / 4.0f;
        private float _aspectRatio = 1.0f;
        private float _nearPlane = 0.1f;
        private float _farPlane = 1000.0f;
        private Dictionary<int, Dictionary<string, ICustomShader>> _customShaders = new Dictionary<int, Dictionary<string, ICustomShader>>();
        private Dictionary<int, Dictionary<int, ICustomRenderable>> _customRenderables = new Dictionary<int, Dictionary<int, ICustomRenderable>>();
        /// <summary>
        /// Creates a new ScreenOverlay instance
        /// </summary>
        public ScreenOverlay()
        {
            drawThreadRunning();
        }
        /// <summary>
        /// Checks if drawing thread is running
        /// </summary>
        private void drawThreadRunning()
        {
            if (!_drawingThreadRunning)
            {
                lock (_drawingQueue)
                {
                    if (!_drawingThreadRunning)
                    {
                        _drawingThreadRunning = true;
                        _drawingThread = new Thread(DrawingThreadProc);
                        _drawingThread.IsBackground = true;
                        _drawingThread.Start();
                    }
                }
            }
        }
        /// <summary>
        /// Thread proc for handling drawing operations asynchronously
        /// </summary>
        private void DrawingThreadProc()
        {
            try
            {
                Stopwatch updateTimer = new Stopwatch();
                updateTimer.Start();
                while (_drawingThreadRunning)
                {
                    try
                    {
                        _drawingSignal.WaitOne(5);
                        
                        bool didWork = false;
                        int maxOperationsPerBatch = 20;
                        int processedOps = 0;
                        
                        while (processedOps < maxOperationsPerBatch && _drawingQueue.TryDequeue(out Action drawAction))
                        {
                            try
                            {
                                drawAction();
                                didWork = true;
                                processedOps++;
                            }
                            catch (Exception)
                            {

                            }
                        }
                        long elapsedMs = updateTimer.ElapsedMilliseconds;
                        if (elapsedMs >= MAX_UPDATE_RATE_MS)
                        {
                            updateTimer.Restart();
                            UpdateActiveOverlays();
                        }
                        if (!didWork)
                        {
                            Thread.Sleep(1);
                        }
                        
                        if (_drawingQueue.IsEmpty)
                        {
                            _drawingSignal.Reset();
                        }
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(100);
                    }
                }
            }
            catch (Exception)
            {
                _drawingThreadRunning = false;
            }
        }
        /// <summary>
        /// Queues a drawing operation to be async executed
        /// </summary>
        public void QueueDrawingAction(Action drawAction)
        {
            if (drawAction == null)
                return;
            drawThreadRunning();
            _drawingQueue.Enqueue(drawAction);
            _drawingSignal.Set();
        }
        /// <summary>
        /// Draws a point on a monitor. The strokeWidth parameter controls the size of the point.
        /// </summary>
        /// <param name="prevX">Previous X coordinate (ignored in Direct3D implementation which only x,y are used)</param>
        /// <param name="prevY">Previous Y coordinate (ignored in Direct3D implementation which only x,y are used)</param>
        /// <param name="x">X coordinate to draw at</param>
        /// <param name="y">Y coordinate to draw at</param>
        /// <param name="strokeWidth">Width/size of the point</param>
        /// <param name="colorArgb">ARGB color value</param>
        /// <param name="monitorIndex">Monitor index to draw on</param>
        public void Draw(int prevX, int prevY, int x, int y, int strokeWidth, int colorArgb, int monitorIndex)
        {
            QueueDrawingAction(() => DrawPoint(monitorIndex, x, y, colorArgb, strokeWidth));
        }
        /// <summary>
        /// Erases at the specified coordinates on a monitor by removing any points in the radius determined by strokeWidth
        /// </summary>
        /// <param name="prevX">Previous X coordinate (ignored in Direct3D implementation which only x,y are used)</param>
        /// <param name="prevY">Previous Y coordinate (ignored in Direct3D implementation which only x,y are used)</param>
        /// <param name="x">X coordinate to erase at</param>
        /// <param name="y">Y coordinate to erase at</param>
        /// <param name="strokeWidth">Width of the eraser (radius)</param>
        /// <param name="monitorIndex">Monitor index to erase on</param>
        public void DrawEraser(int prevX, int prevY, int x, int y, int strokeWidth, int monitorIndex)
        {
            QueueDrawingAction(() => Erase(monitorIndex, x, y, strokeWidth));
        }
        /// <summary>
        /// Draws a point on the screen at the specified coordinates
        /// </summary>
        private void DrawPoint(int monitorIndex, int x, int y, int colorArgb, float strokeWidth)
        {
            try
            {
                Rectangle bounds;
                try
                {
                    bounds = Screen.AllScreens[monitorIndex].Bounds;
                }
                catch (IndexOutOfRangeException)
                {
                    return;
                }
                ShowDrawingOverlay(monitorIndex, bounds);
                if (x < 0 || y < 0 || x >= bounds.Width || y >= bounds.Height)
                {
                    return;
                }
                float ndcX = (x / (float)bounds.Width) * 2.0f - 1.0f;
                float ndcY = 1.0f - (y / (float)bounds.Height) * 2.0f;
                Color argbColor = Color.FromArgb(colorArgb);
                var color4 = new Color4(argbColor.R / 255.0f, argbColor.G / 255.0f, argbColor.B / 255.0f, argbColor.A / 255.0f);
                float scaledWidth = (strokeWidth / (float)bounds.Width) * 2.0f;
                float pointSize = Math.Max(0.01f, scaledWidth * 5.0f);
                var vertex = new VertexPositionColor(
                    new Vector2(ndcX, ndcY),
                    color4,
                    pointSize
                );
                if (!_drawingData.TryGetValue(monitorIndex, out MonitorDrawingData data))
                {
                    data = new MonitorDrawingData(INITIAL_BUFFER_SIZE);
                    _drawingData[monitorIndex] = data;
                }
                data.AddPoint(vertex);
                _needsUpdate[monitorIndex] = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DrawPoint error: {ex.Message}");
            }
        }
        /// <summary>
        /// Erases at the specified coordinates
        /// </summary>
        private void Erase(int monitorIndex, int x, int y, float strokeWidth)
        {
            try
            {
                Rectangle bounds;
                try
                {
                    bounds = Screen.AllScreens[monitorIndex].Bounds;
                }
                catch (IndexOutOfRangeException)
                {
                    return;
                }
                bool shouldUpdate = false;
                bool isRemovingLastPoint = false;
                if (_drawingData.TryGetValue(monitorIndex, out MonitorDrawingData data))
                {
                    shouldUpdate = true;
                    if (data.Points.Count > 0)
                    {
                        float ndcX = (x / (float)bounds.Width) * 2.0f - 1.0f;
                        float ndcY = 1.0f - (y / (float)bounds.Height) * 2.0f;
                        float eraseRadiusX = (strokeWidth / (float)bounds.Width) * 2.0f * 1.5f;
                        float eraseRadiusY = (strokeWidth / (float)bounds.Height) * 2.0f * 1.5f;
                        List<VertexPositionColor> remainingPoints = new List<VertexPositionColor>(data.Points.Count);
                        float radiusSquared = eraseRadiusX * eraseRadiusX;
                        for (int i = 0; i < data.Points.Count; i++)
                        {
                            var point = data.Points[i];
                            float dx = point.Position.X - ndcX;
                            float dy = point.Position.Y - ndcY;
                            float distanceSquared = dx * dx + dy * dy;
                            float pointRadius = point.Size / (float)Math.Max(bounds.Width, bounds.Height);
                            if (distanceSquared > radiusSquared + pointRadius * pointRadius)
                            {
                                remainingPoints.Add(point);
                            }
                        }
                        if (remainingPoints.Count == 0 && data.Points.Count > 0)
                        {
                            isRemovingLastPoint = true;
                        }
                        if (remainingPoints.Count < data.Points.Count)
                        {
                            data.Points.Clear();
                            data.Points.AddRange(remainingPoints);
                            data.RebuildIndices();
                        }
                    }
                }
                if (shouldUpdate)
                {
                    _needsUpdate[monitorIndex] = true;
                    if (isRemovingLastPoint)
                    {
                        ForceImmediateUpdate(monitorIndex);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erase error: {ex.Message}");
            }
        }
        private void ForceImmediateUpdate(int monitorIndex)
        {
            try
            {
                if (_deviceContexts.TryGetValue(monitorIndex, out DeviceContext context) &&
                    _renderTargetViews.TryGetValue(monitorIndex, out RenderTargetView renderTargetView) &&
                    _swapChains.TryGetValue(monitorIndex, out SwapChain swapChain))
                {
                    context.ClearRenderTargetView(renderTargetView, new Color4(0, 0, 0, 0));
                    context.OutputMerger.SetRenderTargets(renderTargetView);
                    swapChain.Present(0, PresentFlags.None);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ForceImmediateUpdate error: {ex.Message}");
            }
        }
        /// <summary>
        /// Clears all drawings on a monitor
        /// </summary>
        public void ClearDrawings(int monitorIndex)
        {
            QueueDrawingAction(() =>
            {
                try
                {
                    if (_drawingData.TryGetValue(monitorIndex, out MonitorDrawingData data))
                    {
                        data.Clear();
                        _needsUpdate[monitorIndex] = true;
                    }
                    if (_deviceContexts.TryGetValue(monitorIndex, out DeviceContext context) &&
                        _renderTargetViews.TryGetValue(monitorIndex, out RenderTargetView renderTargetView))
                    {
                        context.ClearRenderTargetView(renderTargetView, new Color4(0, 0, 0, 0));
                        if (_swapChains.TryGetValue(monitorIndex, out SwapChain swapChain))
                        {
                            swapChain.Present(0, PresentFlags.None);
                        }
                    }
                    RenderOverlay(monitorIndex);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"ClearDrawings error: {ex.Message}");
                }
            });
        }
        /// <summary>
        /// Creates the overlay window and DirectX resources
        /// </summary>
        private void ShowDrawingOverlay(int monitorIndex, Rectangle bounds)
        {
            if (_overlayWindows.TryGetValue(monitorIndex, out IntPtr hwnd) && hwnd != IntPtr.Zero)
            {
                return;
            }
            if (!_delegateInit)
            {
                _wndProcDelegate = DXOverlayWndProc;
                _delegateInit = true;
            }
            string className = $"{monitorIndex}";
            WNDCLASS wndClass = new WNDCLASS();
            wndClass.lpfnWndProc = _wndProcDelegate;
            wndClass.hInstance = NativeMethods.GetModuleHandle(null);
            wndClass.lpszClassName = className;
            wndClass.hbrBackground = IntPtr.Zero;
            try
            {
                NativeMethods.RegisterClass(ref wndClass);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"RegisterClass error: {ex.Message}");
            }
            hwnd = NativeMethods.CreateWindowEx(
                NativeMethods.WS_EX_LAYERED | NativeMethods.WS_EX_TOOLWINDOW | NativeMethods.WS_EX_TRANSPARENT,
                        className,
                        "",
                NativeMethods.WS_POPUP,
                        bounds.X, bounds.Y, bounds.Width, bounds.Height,
                IntPtr.Zero, IntPtr.Zero, NativeMethods.GetModuleHandle(null), IntPtr.Zero);
            if (hwnd == IntPtr.Zero)
            {
                Debug.WriteLine("Failed to create overlay window");
                return;
            }
            _overlayWindows[monitorIndex] = hwnd;
            NativeMethods.SetLayeredWindowAttributes(hwnd, 0, 255, NativeMethods.LWA_COLORKEY | NativeMethods.LWA_ALPHA);
            NativeMethods.ShowWindow(hwnd, NativeMethods.SW_SHOWNA);
            NativeMethods.SetWindowPos(
                hwnd,
                NativeMethods.HWND_TOPMOST,
                0, 0, 0, 0,
                NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOACTIVATE
            );
            CreateDXResources(monitorIndex, hwnd, bounds);
        }
        /// <summary>
        /// Create DirectX resources for a specific monitor overlay including swap chain, buffers, and shaders
        /// </summary>
        private void CreateDXResources(int monitorIndex, IntPtr windowHandle, Rectangle bounds)
        {
            try
            {
                var swapChainDesc = new SwapChainDescription
                {
                    BufferCount = 2,
                    Usage = Usage.RenderTargetOutput,
                    OutputHandle = windowHandle,
                    IsWindowed = true,
                    ModeDescription = new ModeDescription(
                        bounds.Width,
                        bounds.Height,
                        new Rational(60, 1),
                        Format.B8G8R8A8_UNorm
                    ),
                    SampleDescription = new SampleDescription(1, 0),
                    Flags = SwapChainFlags.AllowModeSwitch,
                    SwapEffect = SwapEffect.Discard
                };
                Device device;
                SwapChain swapChain;
                try
                {
                    Device.CreateWithSwapChain(
                        DriverType.Hardware,
                        DeviceCreationFlags.BgraSupport,
                        swapChainDesc,
                        out device,
                        out swapChain
                    );
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"failed to create device & swap chain: {ex.Message}");
                        return;
                }
                _devices[monitorIndex] = device;
                _swapChains[monitorIndex] = swapChain;
                var context = device.ImmediateContext;
                _deviceContexts[monitorIndex] = context;
                try
                {
                    using (var backBuffer = swapChain.GetBackBuffer<Texture2D>(0))
                    {
                        var renderTargetView = new RenderTargetView(device, backBuffer);
                        _renderTargetViews[monitorIndex] = renderTargetView;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"failed to create render target view: {ex.Message}");
                    return;
                }
                context.Rasterizer.SetViewport(0, 0, bounds.Width, bounds.Height);
                try
                {
                    var blendStateDesc = new BlendStateDescription();
                    blendStateDesc.AlphaToCoverageEnable = false;
                    blendStateDesc.IndependentBlendEnable = false;
                    for (int i = 0; i < 8; i++)
                    {
                        blendStateDesc.RenderTarget[i].IsBlendEnabled = true;
                        blendStateDesc.RenderTarget[i].SourceBlend = BlendOption.SourceAlpha;
                        blendStateDesc.RenderTarget[i].DestinationBlend = BlendOption.InverseSourceAlpha;
                        blendStateDesc.RenderTarget[i].BlendOperation = BlendOperation.Add;
                        blendStateDesc.RenderTarget[i].SourceAlphaBlend = BlendOption.One;
                        blendStateDesc.RenderTarget[i].DestinationAlphaBlend = BlendOption.Zero;
                        blendStateDesc.RenderTarget[i].AlphaBlendOperation = BlendOperation.Add;
                        blendStateDesc.RenderTarget[i].RenderTargetWriteMask = ColorWriteMaskFlags.All;
                    }
                    var blendState = new BlendState(device, blendStateDesc);
                    context.OutputMerger.SetBlendState(blendState);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"failed to set blend state: {ex.Message}");
                }
                if (!CompileShaders(device, monitorIndex))
                {
                    return;
                }
                try
                {
                    var vertexBufferDesc = new BufferDescription
                    {
                        SizeInBytes = VertexPositionColor.SizeInBytes * INITIAL_BUFFER_SIZE * 4,
                        Usage = ResourceUsage.Dynamic,
                        BindFlags = BindFlags.VertexBuffer,
                        CpuAccessFlags = CpuAccessFlags.Write,
                        OptionFlags = ResourceOptionFlags.None
                    };
                    var vertexBuffer = new Buffer(device, vertexBufferDesc);
                    _vertexBuffers[monitorIndex] = vertexBuffer;
                    var indexBufferDesc = new BufferDescription
                    {
                        SizeInBytes = sizeof(int) * INITIAL_BUFFER_SIZE * 6,
                        Usage = ResourceUsage.Dynamic,
                        BindFlags = BindFlags.IndexBuffer,
                        CpuAccessFlags = CpuAccessFlags.Write,
                        OptionFlags = ResourceOptionFlags.None
                    };
                    var indexBuffer = new Buffer(device, indexBufferDesc);
                    _indexBuffers[monitorIndex] = indexBuffer;
                    _bufferSizes[monitorIndex] = INITIAL_BUFFER_SIZE;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to create buffers: {ex.Message}");
                    return;
                }
                _drawingData[monitorIndex] = new MonitorDrawingData(INITIAL_BUFFER_SIZE);
                _needsUpdate[monitorIndex] = true;
                try
                {
                    var rasterizerDesc = new RasterizerStateDescription
                    {
                        CullMode = CullMode.None,
                        FillMode = FillMode.Solid,
                        IsAntialiasedLineEnabled = true,
                        IsMultisampleEnabled = true,
                        IsFrontCounterClockwise = false,
                        IsScissorEnabled = false,
                        IsDepthClipEnabled = false
                    };
                    var rasterState = new RasterizerState(device, rasterizerDesc);
                    context.Rasterizer.State = rasterState;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"failed to set rasterizer state: {ex.Message}");
                }
                Setup3DResources(monitorIndex);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CreateDXResources error: {ex.Message}");
            }
        }
        /// <summary>
        /// Sets up the 3D resources for the overlay
        /// </summary>
        private void Setup3DResources(int monitorIndex)
        {
            if (!_devices.TryGetValue(monitorIndex, out Device device))
            {
                Debug.WriteLine($"No device available for monitor {monitorIndex}");
                return;
            }
            var depthDesc = new Texture2DDescription
            {
                Width = Screen.AllScreens[monitorIndex].Bounds.Width,
                Height = Screen.AllScreens[monitorIndex].Bounds.Height,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.D24_UNorm_S8_UInt,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            };
            var depthTexture = new Texture2D(device, depthDesc);
            var depthStencilView = new DepthStencilView(device, depthTexture);
            _deviceContexts[monitorIndex].OutputMerger.SetDepthStencilState(
                new DepthStencilState(device, new DepthStencilStateDescription
                {
                    IsDepthEnabled = true,
                    DepthWriteMask = DepthWriteMask.All,
                    DepthComparison = Comparison.Less,
                    IsStencilEnabled = false
                })
            );
            _deviceContexts[monitorIndex].OutputMerger.SetRenderTargets(depthStencilView, _renderTargetViews[monitorIndex]);
            _aspectRatio = (float)Screen.AllScreens[monitorIndex].Bounds.Width / Screen.AllScreens[monitorIndex].Bounds.Height;
            if (!_vertex3DShaders.ContainsKey(monitorIndex))
            {
                try
                {
                    using (var vertexShaderBytecode = SharpDX.D3DCompiler.ShaderBytecode.Compile(
                        Vertex3DShaderCode,
                        "main",
                        "vs_4_0",
                        SharpDX.D3DCompiler.ShaderFlags.Debug))
                    {
                        _vertex3DShaders[monitorIndex] = new VertexShader(device, vertexShaderBytecode);
                        var inputElements = new[]
                        {
                            new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                            new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 12, 0)
                        };
                        _input3DLayouts[monitorIndex] = new InputLayout(device, vertexShaderBytecode, inputElements);
                    }
                    using (var pixelShaderBytecode = SharpDX.D3DCompiler.ShaderBytecode.Compile(
                        Pixel3DShaderCode,
                        "main",
                        "ps_4_0",
                        SharpDX.D3DCompiler.ShaderFlags.Debug))
                    {
                        _pixel3DShaders[monitorIndex] = new PixelShader(device, pixelShaderBytecode);
                    }
                    var matrixBufferDesc = new BufferDescription
                    {
                        Usage = ResourceUsage.Dynamic,
                        SizeInBytes = SharpDX.Utilities.SizeOf<Matrix>() * 3,
                        BindFlags = BindFlags.ConstantBuffer,
                        CpuAccessFlags = CpuAccessFlags.Write,
                        OptionFlags = ResourceOptionFlags.None,
                        StructureByteStride = 0
                    };
                    _matrixBuffers[monitorIndex] = new Buffer(device, matrixBufferDesc);
                    _cubes[monitorIndex] = new Dictionary<int, Cube3DData>();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error setting up 3D resources: {ex.Message}");
                }
            }
        }
        /// <summary>
        /// Creates a cube mesh with the specified colors for each face
        /// </summary>
        private void CreateCubeMesh(Device device, Cube3DData cube)
        {
            VertexPosition3DColor[] vertices = new VertexPosition3DColor[24];
            float halfSize = 0.5f;
            vertices[0] = new VertexPosition3DColor(new Vector3(-halfSize, -halfSize, halfSize), cube.FrontColor);
            vertices[1] = new VertexPosition3DColor(new Vector3(-halfSize, halfSize, halfSize), cube.FrontColor);
            vertices[2] = new VertexPosition3DColor(new Vector3(halfSize, halfSize, halfSize), cube.FrontColor);
            vertices[3] = new VertexPosition3DColor(new Vector3(halfSize, -halfSize, halfSize), cube.FrontColor);
            vertices[4] = new VertexPosition3DColor(new Vector3(halfSize, -halfSize, -halfSize), cube.BackColor);
            vertices[5] = new VertexPosition3DColor(new Vector3(halfSize, halfSize, -halfSize), cube.BackColor);
            vertices[6] = new VertexPosition3DColor(new Vector3(-halfSize, halfSize, -halfSize), cube.BackColor);
            vertices[7] = new VertexPosition3DColor(new Vector3(-halfSize, -halfSize, -halfSize), cube.BackColor);
            vertices[8] = new VertexPosition3DColor(new Vector3(-halfSize, halfSize, halfSize), cube.TopColor);
            vertices[9] = new VertexPosition3DColor(new Vector3(-halfSize, halfSize, -halfSize), cube.TopColor);
            vertices[10] = new VertexPosition3DColor(new Vector3(halfSize, halfSize, -halfSize), cube.TopColor);
            vertices[11] = new VertexPosition3DColor(new Vector3(halfSize, halfSize, halfSize), cube.TopColor);
            vertices[12] = new VertexPosition3DColor(new Vector3(-halfSize, -halfSize, -halfSize), cube.BottomColor);
            vertices[13] = new VertexPosition3DColor(new Vector3(-halfSize, -halfSize, halfSize), cube.BottomColor);
            vertices[14] = new VertexPosition3DColor(new Vector3(halfSize, -halfSize, halfSize), cube.BottomColor);
            vertices[15] = new VertexPosition3DColor(new Vector3(halfSize, -halfSize, -halfSize), cube.BottomColor);
            vertices[16] = new VertexPosition3DColor(new Vector3(-halfSize, -halfSize, -halfSize), cube.LeftColor);
            vertices[17] = new VertexPosition3DColor(new Vector3(-halfSize, halfSize, -halfSize), cube.LeftColor);
            vertices[18] = new VertexPosition3DColor(new Vector3(-halfSize, halfSize, halfSize), cube.LeftColor);
            vertices[19] = new VertexPosition3DColor(new Vector3(-halfSize, -halfSize, halfSize), cube.LeftColor);
            vertices[20] = new VertexPosition3DColor(new Vector3(halfSize, -halfSize, halfSize), cube.RightColor);
            vertices[21] = new VertexPosition3DColor(new Vector3(halfSize, halfSize, halfSize), cube.RightColor);
            vertices[22] = new VertexPosition3DColor(new Vector3(halfSize, halfSize, -halfSize), cube.RightColor);
            vertices[23] = new VertexPosition3DColor(new Vector3(halfSize, -halfSize, -halfSize), cube.RightColor);
            var vertexBufferDesc = new BufferDescription
            {
                SizeInBytes = VertexPosition3DColor.SizeInBytes * vertices.Length,
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.VertexBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            };
            cube.VertexBuffer = Buffer.Create(device, vertices, vertexBufferDesc);
            cube.VertexCount = vertices.Length;
            int[] indices = new int[36];
            indices[0] = 0; indices[1] = 1; indices[2] = 2;
            indices[3] = 0; indices[4] = 2; indices[5] = 3;
            indices[6] = 4; indices[7] = 5; indices[8] = 6;
            indices[9] = 4; indices[10] = 6; indices[11] = 7;
            indices[12] = 8; indices[13] = 9; indices[14] = 10;
            indices[15] = 8; indices[16] = 10; indices[17] = 11;
            indices[18] = 12; indices[19] = 13; indices[20] = 14;
            indices[21] = 12; indices[22] = 14; indices[23] = 15;
            indices[24] = 16; indices[25] = 17; indices[26] = 18;
            indices[27] = 16; indices[28] = 18; indices[29] = 19;
            indices[30] = 20; indices[31] = 21; indices[32] = 22;
            indices[33] = 20; indices[34] = 22; indices[35] = 23;
            var indexBufferDesc = new BufferDescription
            {
                SizeInBytes = sizeof(int) * indices.Length,
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.IndexBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            };
            cube.IndexBuffer = Buffer.Create(device, indices, indexBufferDesc);
            cube.IndexCount = indices.Length;
        }
        /// <summary>
        /// Compile vertex and pixel shaders at runtime
        /// </summary>
        private bool CompileShaders(Device device, int monitorIndex)
        {
            try
            {
                using (var vertexShaderBytecode = SharpDX.D3DCompiler.ShaderBytecode.Compile(
                    VertexShaderCode,
                    "main",
                    "vs_4_0",
                    SharpDX.D3DCompiler.ShaderFlags.Debug))
                {
                    var vertexShader = new VertexShader(device, vertexShaderBytecode);
                    _vertexShaders[monitorIndex] = vertexShader;
                    var inputElements = new[]
                    {
                        new InputElement("POSITION", 0, Format.R32G32_Float, 0, 0),
                        new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 8, 0),
                        new InputElement("SIZE", 0, Format.R32_Float, 24, 0)
                    };
                    var inputLayout = new InputLayout(device, vertexShaderBytecode, inputElements);
                    _inputLayouts[monitorIndex] = inputLayout;
                }
                using (var pixelShaderBytecode = SharpDX.D3DCompiler.ShaderBytecode.Compile(
                    PixelShaderCode,
                    "main",
                    "ps_4_0",
                    SharpDX.D3DCompiler.ShaderFlags.Debug))
                {
                    var pixelShader = new PixelShader(device, pixelShaderBytecode);
                    _pixelShaders[monitorIndex] = pixelShader;
                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CompileShaders error: {ex.Message}");
                return false;
            }
        }
        /// <summary>
        /// Update all active overlays
        /// </summary>
        private void UpdateActiveOverlays()
        {
            UpdateAnimatedCubes();
            HashSet<int> monitorsToUpdate = new HashSet<int>();
            foreach (var kvp in _needsUpdate)
            {
                if (kvp.Value)
                {
                    monitorsToUpdate.Add(kvp.Key);
                }
            }
            foreach (var monitorIndex in monitorsToUpdate)
            {
                try
                {
                    RenderOverlay(monitorIndex);
                    _needsUpdate[monitorIndex] = false;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"UpdateActiveOverlays error: {ex.Message}");
                }
            }
        }
        /// <summary>
        /// Renders the overlay for a monitor
        /// </summary>
        private void RenderOverlay(int monitorIndex)
                {
                    try
                    {
                if (!_deviceContexts.TryGetValue(monitorIndex, out DeviceContext context) ||
                    !_renderTargetViews.TryGetValue(monitorIndex, out RenderTargetView renderTargetView) ||
                    !_vertexShaders.TryGetValue(monitorIndex, out VertexShader vertexShader) ||
                    !_pixelShaders.TryGetValue(monitorIndex, out PixelShader pixelShader) ||
                    !_inputLayouts.TryGetValue(monitorIndex, out InputLayout inputLayout) ||
                    !_vertexBuffers.TryGetValue(monitorIndex, out Buffer vertexBuffer) ||
                    !_indexBuffers.TryGetValue(monitorIndex, out Buffer indexBuffer) ||
                    !_swapChains.TryGetValue(monitorIndex, out SwapChain swapChain) ||
                    !_drawingData.TryGetValue(monitorIndex, out MonitorDrawingData drawingData))
                {
                    Debug.WriteLine($"Missing DirectX resources for monitor {monitorIndex}");
                        return;
                    }
                if (drawingData.Points.Count == 0)
                {
                    context.ClearRenderTargetView(renderTargetView, new Color4(0, 0, 0, 0));
                    bool hasContent = false;
                    if (_cubes.TryGetValue(monitorIndex, out Dictionary<int, Cube3DData> cubes) && cubes.Count > 0)
                    {
                        Render3DCubes(monitorIndex, context, cubes.Values.ToList());
                        hasContent = true;
                    }
                    if (_customRenderables.TryGetValue(monitorIndex, out Dictionary<int, ICustomRenderable> renderables) && renderables.Count > 0)
                    {
                        RenderCustomRenderables(monitorIndex, context, renderables.Values.ToList());
                        hasContent = true;
                    }
                    if (hasContent)
                    {
                        swapChain.Present(1, PresentFlags.DoNotWait);
                    }
                        return;
                    }
                int pointCount = drawingData.Points.Count;
                int vertexCount = pointCount * 4;
                int usedIndices = pointCount * 6;
                if (!_bufferSizes.TryGetValue(monitorIndex, out int currentBufferSize) ||
                    vertexCount > currentBufferSize * 4)
                {
                    int newBufferSize = Math.Max(pointCount * 2, INITIAL_BUFFER_SIZE);
                    try
                    {
                        Debug.WriteLine($"resizing buffers for monitor {monitorIndex}: " +
                                        $"points={pointCount}, new capacity={newBufferSize}");
                        var newVertexBufferDesc = new BufferDescription
                        {
                            SizeInBytes = VertexPositionColor.SizeInBytes * newBufferSize * 4,
                            Usage = ResourceUsage.Dynamic,
                            BindFlags = BindFlags.VertexBuffer,
                            CpuAccessFlags = CpuAccessFlags.Write,
                            OptionFlags = ResourceOptionFlags.None
                        };
                        var newVertexBuffer = new Buffer(context.Device, newVertexBufferDesc);
                        var newIndexBufferDesc = new BufferDescription
                        {
                            SizeInBytes = sizeof(int) * newBufferSize * 6,
                            Usage = ResourceUsage.Dynamic,
                            BindFlags = BindFlags.IndexBuffer,
                            CpuAccessFlags = CpuAccessFlags.Write,
                            OptionFlags = ResourceOptionFlags.None
                        };
                        var newIndexBuffer = new Buffer(context.Device, newIndexBufferDesc);
                        vertexBuffer.Dispose();
                        indexBuffer.Dispose();
                        _vertexBuffers[monitorIndex] = newVertexBuffer;
                        _indexBuffers[monitorIndex] = newIndexBuffer;
                        _bufferSizes[monitorIndex] = newBufferSize;
                        vertexBuffer = newVertexBuffer;
                        indexBuffer = newIndexBuffer;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Failed to resize buffers: {ex.Message}");
                    }
                }
                try
                {
                    var quadVertices = new VertexPositionColor[vertexCount];
                    for (int i = 0; i < pointCount; i++)
                    {
                        var point = drawingData.Points[i];
                        float halfSize = point.Size * 0.5f;
                        quadVertices[i * 4] = new VertexPositionColor(
                            new Vector2(point.Position.X - halfSize, point.Position.Y - halfSize),
                            point.Color,
                            point.Size
                        );
                        quadVertices[i * 4 + 1] = new VertexPositionColor(
                            new Vector2(point.Position.X + halfSize, point.Position.Y - halfSize),
                            point.Color,
                            point.Size
                        );
                        quadVertices[i * 4 + 2] = new VertexPositionColor(
                            new Vector2(point.Position.X - halfSize, point.Position.Y + halfSize),
                            point.Color,
                            point.Size
                        );
                        quadVertices[i * 4 + 3] = new VertexPositionColor(
                            new Vector2(point.Position.X + halfSize, point.Position.Y + halfSize),
                            point.Color,
                            point.Size
                        );
                    }
                    try
                    {
                        var vertexDataBox = context.MapSubresource(
                            vertexBuffer,
                            0,
                            MapMode.WriteDiscard,
                            SharpDX.Direct3D11.MapFlags.None);
                        SharpDX.Utilities.Write(vertexDataBox.DataPointer, quadVertices, 0, vertexCount);
                        context.UnmapSubresource(vertexBuffer, 0);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error updating vertex buffer: {ex.Message}");
                    return;
                }
                    try
                    {
                        var indexDataBox = context.MapSubresource(
                            indexBuffer,
                            0,
                            MapMode.WriteDiscard,
                            SharpDX.Direct3D11.MapFlags.None);
                        SharpDX.Utilities.Write(indexDataBox.DataPointer, drawingData.Indices, 0, usedIndices);
                        context.UnmapSubresource(indexBuffer, 0);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error updating index buffer: {ex.Message}");
                        return;
                    }
                    context.ClearRenderTargetView(renderTargetView, new Color4(0, 0, 0, 0));
                    context.OutputMerger.SetRenderTargets(renderTargetView);
                    context.VertexShader.Set(vertexShader);
                    context.PixelShader.Set(pixelShader);
                    context.InputAssembler.InputLayout = inputLayout;
                    context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
                    context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertexBuffer, VertexPositionColor.SizeInBytes, 0));
                    context.InputAssembler.SetIndexBuffer(indexBuffer, Format.R32_UInt, 0);
                    context.DrawIndexed(usedIndices, 0, 0);
                    if (_cubes.TryGetValue(monitorIndex, out Dictionary<int, Cube3DData> cubes) && cubes.Count > 0)
                    {
                        Render3DCubes(monitorIndex, context, cubes.Values.ToList());
                    }
                    if (_customRenderables.TryGetValue(monitorIndex, out Dictionary<int, ICustomRenderable> renderables) && renderables.Count > 0)
                    {
                        RenderCustomRenderables(monitorIndex, context, renderables.Values.ToList());
                    }
                    swapChain.Present(1, PresentFlags.DoNotWait);
                }
                catch (SharpDXException dx)
                {
                    Debug.WriteLine($"DirectX error in RenderOverlay: {dx.Message}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"RenderOverlay error: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"RenderOverlay outer error: {ex.Message}");
            }
        }
        /// <summary>
        /// Renders the 3D cubes for a monitor
        /// </summary>
        private void Render3DCubes(int monitorIndex, DeviceContext context, List<Cube3DData> cubes)
            {
                try
                {
                if (!_vertex3DShaders.TryGetValue(monitorIndex, out VertexShader vertexShader) ||
                    !_pixel3DShaders.TryGetValue(monitorIndex, out PixelShader pixelShader) ||
                    !_input3DLayouts.TryGetValue(monitorIndex, out InputLayout inputLayout) ||
                    !_matrixBuffers.TryGetValue(monitorIndex, out Buffer matrixBuffer))
                {
                    Setup3DResources(monitorIndex);
                    if (!_vertex3DShaders.TryGetValue(monitorIndex, out vertexShader) ||
                        !_pixel3DShaders.TryGetValue(monitorIndex, out pixelShader) ||
                        !_input3DLayouts.TryGetValue(monitorIndex, out inputLayout) ||
                        !_matrixBuffers.TryGetValue(monitorIndex, out matrixBuffer))
                    {
                        return;
                    }
                }
                context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
                context.VertexShader.Set(vertexShader);
                context.PixelShader.Set(pixelShader);
                context.InputAssembler.InputLayout = inputLayout;
                Matrix viewMatrix = Matrix.LookAtLH(_cameraPosition, _cameraTarget, _cameraUp);
                Matrix projectionMatrix = Matrix.PerspectiveFovLH(
                    _fieldOfView,
                    _aspectRatio,
                    _nearPlane,
                    _farPlane
                );
                foreach (var cube in cubes)
                {
                    DataStream mappedResource;
                    context.MapSubresource(matrixBuffer, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None, out mappedResource);
                    mappedResource.Write(cube.WorldMatrix);
                    mappedResource.Write(viewMatrix);
                    mappedResource.Write(projectionMatrix);
                    context.UnmapSubresource(matrixBuffer, 0);
                    context.VertexShader.SetConstantBuffer(0, matrixBuffer);
                    context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(cube.VertexBuffer, VertexPosition3DColor.SizeInBytes, 0));
                    context.InputAssembler.SetIndexBuffer(cube.IndexBuffer, Format.R32_UInt, 0);
                    context.DrawIndexed(cube.IndexCount, 0, 0);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Render3DCubes error: {ex.Message}");
            }
        }
        /// <summary>
        /// Renders custom renderables for a monitor
        /// </summary>
        private void RenderCustomRenderables(int monitorIndex, DeviceContext context, List<ICustomRenderable> renderables)
            {
                try
                {
                Matrix viewMatrix = Matrix.LookAtLH(_cameraPosition, _cameraTarget, _cameraUp);
                Matrix projectionMatrix = Matrix.PerspectiveFovLH(
                    _fieldOfView,
                    _aspectRatio,
                    _nearPlane,
                    _farPlane
                );
                bool customShaderApplied = false;
                if (_customShaders.TryGetValue(monitorIndex, out Dictionary<string, ICustomShader> shaders) && shaders.Count > 0)
                {
                    foreach (var shader in shaders.Values)
                    {
                        try
                        {
                            shader.Apply(context);
                            customShaderApplied = true;
                            break; 
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error applying custom shader {shader.ShaderId}: {ex.Message}");
                        }
                    }
                }
                foreach (var renderable in renderables)
                {
                    try
                    {
                        renderable.Render(context, viewMatrix, projectionMatrix);
                        renderable.NeedsUpdate = false;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error rendering custom renderable {renderable.RenderableId}: {ex.Message}");
                    }
                }
                if (customShaderApplied && 
                    _vertexShaders.TryGetValue(monitorIndex, out VertexShader defaultVS) &&
                    _pixelShaders.TryGetValue(monitorIndex, out PixelShader defaultPS))
                {
                    context.VertexShader.Set(defaultVS);
                    context.PixelShader.Set(defaultPS);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"RenderCustomRenderables error: {ex.Message}");
            }
        }
        private IntPtr DXOverlayWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            switch (msg)
            {
                case NativeMethods.WM_DESTROY:
                    try
                    {
                        var toRemove = new List<int>();
                        foreach (var pair in _overlayWindows)
                        {
                            if (pair.Value == hWnd)
                            {
                                toRemove.Add(pair.Key);
                                CleanupDirectXResources(pair.Key);
                            }
                        }
                        foreach (var key in toRemove)
                        {
                            _overlayWindows.Remove(key);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"WndProc error: {ex.Message}");
                    }
                    break;
            }
            return NativeMethods.DefWindowProc(hWnd, msg, wParam, lParam);
        }
        /// <summary>
        /// Clean up DirectX resources for a monitor
        /// </summary>
        private void CleanupDirectXResources(int monitorIndex)
        {
            try
            {
                if (_renderTargetViews.TryGetValue(monitorIndex, out RenderTargetView renderTargetView))
                {
                    renderTargetView.Dispose();
                    _renderTargetViews.Remove(monitorIndex);
                }
                if (_vertexBuffers.TryGetValue(monitorIndex, out Buffer vertexBuffer))
                {
                    vertexBuffer.Dispose();
                    _vertexBuffers.Remove(monitorIndex);
                }
                if (_inputLayouts.TryGetValue(monitorIndex, out InputLayout inputLayout))
                {
                    inputLayout.Dispose();
                    _inputLayouts.Remove(monitorIndex);
                }
                if (_vertexShaders.TryGetValue(monitorIndex, out VertexShader vertexShader))
                {
                    vertexShader.Dispose();
                    _vertexShaders.Remove(monitorIndex);
                }
                if (_pixelShaders.TryGetValue(monitorIndex, out PixelShader pixelShader))
                {
                    pixelShader.Dispose();
                    _pixelShaders.Remove(monitorIndex);
                }
                if (_swapChains.TryGetValue(monitorIndex, out SwapChain swapChain))
                {
                    swapChain.Dispose();
                    _swapChains.Remove(monitorIndex);
                }
                if (_deviceContexts.TryGetValue(monitorIndex, out DeviceContext context))
                {
                    context.Dispose();
                    _deviceContexts.Remove(monitorIndex);
                }
                if (_devices.TryGetValue(monitorIndex, out Device device))
                {
                    device.Dispose();
                    _devices.Remove(monitorIndex);
                }
                _needsUpdate.Remove(monitorIndex);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CleanupDirectXResources error: {ex.Message}");
            }
        }
        /// <summary>
        /// Disposes of all resources used by screen overlay
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
                _drawingThreadRunning = false;
                _drawingSignal.Set();
                if (_drawingThread != null && _drawingThread.IsAlive)
                {
                    try
                    {
                        _drawingThread.Join(1000);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Thread join error: {ex.Message}");
                    }
                }
                _drawingSignal.Dispose();
                List<IntPtr> windowsToDestroy = new List<IntPtr>();
                foreach (var handle in _overlayWindows.Values)
                {
                    if (handle != IntPtr.Zero)
                        windowsToDestroy.Add(handle);
                }
                foreach (var monitorIndex in _devices.Keys.ToList())
                {
                    if (_customRenderables.TryGetValue(monitorIndex, out var renderables))
                    {
                        foreach (var renderable in renderables.Values)
                        {
                            try
                            {
                                renderable.Dispose();
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Error disposing custom renderable: {ex.Message}");
                            }
                        }
                        renderables.Clear();
                    }
                    if (_customShaders.TryGetValue(monitorIndex, out var shaders))
                    {
                        foreach (var shader in shaders.Values)
                        {
                            try
                            {
                                shader.Dispose();
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Error disposing custom shader: {ex.Message}");
                            }
                        }
                        shaders.Clear();
                    }
                    CleanupDirectXResources(monitorIndex);
                }
                foreach (var handle in windowsToDestroy)
                {
                    NativeMethods.DestroyWindow(handle);
                }
                _overlayWindows.Clear();
            }
        }
        /// <summary>
        /// Native methods for window operations
        /// </summary>
        public static class NativeMethods
        {
            public const int GWL_EXSTYLE = -20;
            public const int WS_EX_LAYERED = 0x80000;
            public const int WS_EX_TRANSPARENT = 0x20;
            public const int WS_EX_TOPMOST = 0x8;
            public const int WS_EX_TOOLWINDOW = 0x80;
            public const uint WS_POPUP = 0x80000000;
            public const int SWP_NOMOVE = 0x2;
            public const int SWP_NOSIZE = 0x1;
            public const int SWP_NOACTIVATE = 0x10;
            public const int SWP_SHOWWINDOW = 0x40;
            public const int SWP_NOOWNERZORDER = 0x0200;
            public const int HWND_TOPMOST = -1;
            public const int SWP_NOZORDER = 4;
            public const int HWND_NOTOPMOST = -2;
            public const int LWA_COLORKEY = 0x00000001;
            public const int LWA_ALPHA = 0x00000002;
            public const int SW_SHOW = 5;
            public const int SW_SHOWNA = 8;
            public const int BLACK_BRUSH = 4;
            public const int NULL_BRUSH = 5;
            public const uint WM_DESTROY = 0x0002;
            public const uint WM_PAINT = 0x000F;
            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern IntPtr GetModuleHandle(string lpModuleName);
            [DllImport("user32.dll", SetLastError = true)]
            public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
            [DllImport("user32.dll", SetLastError = true)]
            public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
            [DllImport("user32.dll", SetLastError = true)]
            public static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
            [DllImport("user32.dll")]
            public static extern IntPtr DefWindowProc(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
            [DllImport("user32.dll", SetLastError = true)]
            public static extern IntPtr CreateWindowEx(uint dwExStyle, string lpClassName, string lpWindowName, uint dwStyle, int X, int Y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);
            [DllImport("user32.dll")]
            public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
            [DllImport("user32.dll", SetLastError = true)]
            public static extern bool RegisterClass(ref WNDCLASS lpWndClass);
            [DllImport("user32.dll", SetLastError = true)]
            public static extern bool DestroyWindow(IntPtr hWnd);
            [DllImport("user32.dll", SetLastError = true)]
            public static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);
            [DllImport("gdi32.dll", SetLastError = true)]
            public static extern IntPtr GetStockObject(int fnObject);
            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        }
        /// <summary>
        /// Gets data about the pixel (color etc.) from behind the overlay at specific coordinates
        /// </summary>
        /// <param name="x">X coordinate on the screen</param>
        /// <param name="y">Y coordinate on the screen</param>
        /// <param name="monitorIndex">Monitor index (defaults to primary monitor)</param>
        /// <returns>Pixel data including color and position</returns>
        /// <remarks>
        /// This method captures a single pixel from the screen beneath the overlay.
        /// It uses GDI+ to take a screenshot of a 1x1 area at the specified coordinates.
        /// 
        /// Example:
        /// <code>
        /// PixelData pixel = overlay.GetPixelCoords(100, 100);
        /// Console.WriteLine($"Pixel color: R={pixel.Color.R}, G={pixel.Color.G}, B={pixel.Color.B}");
        /// Console.WriteLine($"Pixel position: X={pixel.X}, Y={pixel.Y}");
        /// </code>
        /// </remarks>
        public PixelData GetPixelCoords(int x, int y, int monitorIndex = 0)
        {
            if (monitorIndex < 0 || monitorIndex >= Screen.AllScreens.Length)
            {
                monitorIndex = 0; 
            }
            Rectangle bounds = Screen.AllScreens[monitorIndex].Bounds;
            int screenX = bounds.Left + x;
            int screenY = bounds.Top + y;
            if (x < 0 || x >= bounds.Width || y < 0 || y >= bounds.Height)
            {
                return new PixelData(Color.Black, x, y, monitorIndex);
            }
            try
            {
                using (Bitmap bitmap = new Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                {
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        g.CopyFromScreen(screenX, screenY, 0, 0, new Size(1, 1), CopyPixelOperation.SourceCopy);
                    }
                    Color pixelColor = bitmap.GetPixel(0, 0);
                    return new PixelData(pixelColor, x, y, monitorIndex);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error capturing pixel: {ex.Message}");
                return new PixelData(Color.Black, x, y, monitorIndex);
            }
        }
        /// <summary>
        /// Gets data about a random pixel from the screen behind the overlay
        /// </summary>
        /// <param name="monitorIndex">Monitor index (defaults to primary monitor)</param>
        /// <returns>Pixel data including color and random position</returns>
        /// <remarks>
        /// This method selects random coordinates within the specified monitor's bounds
        /// and returns the pixel data at that location. Useful for creating custom effects.
        /// 
        /// Example:
        /// <code>
        /// PixelData randomPixel = overlay.GetRandomPixel();
        /// Console.WriteLine($"Random pixel at X={randomPixel.X}, Y={randomPixel.Y} has color {randomPixel.Color}");
        /// </code>
        /// </remarks>
        public PixelData GetRandomPixel(int monitorIndex = 0)
        {
            if (monitorIndex < 0 || monitorIndex >= Screen.AllScreens.Length)
            {
                monitorIndex = 0; 
            }
            Rectangle bounds = Screen.AllScreens[monitorIndex].Bounds;
            Random random = new Random();
            int x = random.Next(0, bounds.Width);
            int y = random.Next(0, bounds.Height);
            return GetPixelCoords(x, y, monitorIndex);
        }
        /// <summary>
        /// Captures a region of the screen
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="width">Width of the region</param>
        /// <param name="height">Height of the region</param>
        /// <param name="monitorIndex">Monitor index</param>
        /// <returns>Bitmap containing the captured region</returns>
        /// <remarks>
        /// This method captures a rectangular region of the screen from beneath the overlay and returns it as a Bitmap object.
        /// Useful for creating effects.
        ///
        /// Example:
        /// <code>
        /// // Capture a 200x150 region starting at coordinates (50, 50)
        /// Bitmap screenshot = overlay.CaptureScreenRegion(50, 50, 200, 150);
        /// 
        /// // Save the screenshot to a file
        /// screenshot.Save("screenshot.png");
        /// 
        /// // Don't forget to dispose when done
        /// screenshot.Dispose();
        /// </code>
        /// </remarks>
        public Bitmap CaptureScreenRegion(int x, int y, int width, int height, int monitorIndex = 0)
        {
            if (monitorIndex < 0 || monitorIndex >= Screen.AllScreens.Length)
            {
                monitorIndex = 0; 
            }
            Rectangle bounds = Screen.AllScreens[monitorIndex].Bounds;
            int screenX = bounds.Left + x;
            int screenY = bounds.Top + y;
            width = Math.Max(1, width);
            height = Math.Max(1, height);
            if (x < 0) { width += x; x = 0; }
            if (y < 0) { height += y; y = 0; }
            if (x + width > bounds.Width) width = bounds.Width - x;
            if (y + height > bounds.Height) height = bounds.Height - y;
            if (width <= 0 || height <= 0)
            {
                return new Bitmap(1, 1); 
            }
            try
            {
                Bitmap bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(screenX, screenY, 0, 0, new Size(width, height), CopyPixelOperation.SourceCopy);
                }
                return bitmap;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error capturing screen region: {ex.Message}");
                return new Bitmap(1, 1); 
            }
        }
        private class Cube3DData
        {
            public int Id { get; set; }
            public Vector3 Position { get; set; }
            public Vector3 Rotation { get; set; }
            public Vector3 Scale { get; set; }
            public Color4 FrontColor { get; set; }
            public Color4 BackColor { get; set; }
            public Color4 TopColor { get; set; }
            public Color4 BottomColor { get; set; }
            public Color4 LeftColor { get; set; }
            public Color4 RightColor { get; set; }
            public bool IsMoving { get; set; }
            public Vector3 TargetPosition { get; set; }
            public float MoveSpeed { get; set; }
            public bool IsRotating { get; set; }
            public Vector3 TargetRotation { get; set; }
            public float RotationSpeed { get; set; }
            public Buffer VertexBuffer { get; set; }
            public Buffer IndexBuffer { get; set; }
            public int VertexCount { get; set; }
            public int IndexCount { get; set; }
            public Matrix WorldMatrix { get; private set; }
            public Cube3DData(int id, Color4 frontColor, Color4 backColor, Color4 topColor, 
                              Color4 bottomColor, Color4 leftColor, Color4 rightColor, 
                              float x, float y, float z, float width, float height, float depth = 0)
            {
                Id = id;
                Position = new Vector3(x, y, z);
                Rotation = Vector3.Zero;
                Scale = new Vector3(width, height, depth > 0 ? depth : width);
                FrontColor = frontColor;
                BackColor = backColor;
                TopColor = topColor;
                BottomColor = bottomColor;
                LeftColor = leftColor;
                RightColor = rightColor;
                IsMoving = false;
                IsRotating = false;
                TargetPosition = Position;
                TargetRotation = Rotation;
                MoveSpeed = 5.0f;
                RotationSpeed = 3.0f;
                UpdateWorldMatrix();
            }
            public void UpdateWorldMatrix()
            {
                Matrix rotationX = Matrix.RotationX(Rotation.X);
                Matrix rotationY = Matrix.RotationY(Rotation.Y);
                Matrix rotationZ = Matrix.RotationZ(Rotation.Z);
                Matrix rotationMatrix = rotationX * rotationY * rotationZ;
                Matrix scaleMatrix = Matrix.Scaling(Scale);
                Matrix translationMatrix = Matrix.Translation(Position);
                WorldMatrix = scaleMatrix * rotationMatrix * translationMatrix;
            }
        }
        private void UpdateAnimatedCubes()
        {
            float deltaTime = 0.016f; 
            foreach (var monitorCubes in _cubes)
            {
                bool cubeUpdated = false;
                foreach (var cube in monitorCubes.Value.Values.ToList())
                {
                    bool updated = false;
                    if (cube.IsMoving)
                    {
                        Vector3 direction = cube.TargetPosition - cube.Position;
                        float distance = direction.Length();
                        if (distance > 0.01f)
                        {
                            direction.Normalize();
                            float moveAmount = cube.MoveSpeed * deltaTime;
                            if (distance <= moveAmount)
                            {
                                cube.Position = cube.TargetPosition;
                                cube.IsMoving = false;
                            }
                            else
                            {
                                cube.Position += direction * moveAmount;
                            }
                            updated = true;
                        }
                        else
                        {
                            cube.IsMoving = false;
                        }
                    }
                    if (cube.IsRotating)
                    {
                        Vector3 rotationDelta = cube.TargetRotation - cube.Rotation;
                        float rotationDistance = rotationDelta.Length();
                        if (rotationDistance > 0.01f)
                        {
                            float rotationAmount = cube.RotationSpeed * deltaTime;
                            if (rotationDistance <= rotationAmount)
                            {
                                cube.Rotation = cube.TargetRotation;
                                cube.IsRotating = false;
                            }
                            else
                            {
                                Vector3 rotationDir = new Vector3(
                                    rotationDelta.X != 0 ? Math.Sign(rotationDelta.X) : 0,
                                    rotationDelta.Y != 0 ? Math.Sign(rotationDelta.Y) : 0,
                                    rotationDelta.Z != 0 ? Math.Sign(rotationDelta.Z) : 0
                                );
                                cube.Rotation += rotationDir * rotationAmount;
                            }
                            updated = true;
                        }
                        else
                        {
                            cube.IsRotating = false;
                        }
                    }
                    if (updated)
                    {
                        cube.UpdateWorldMatrix();
                        cubeUpdated = true;
                    }
                }
                if (cubeUpdated)
                {
                    _needsUpdate[monitorCubes.Key] = true;
                }
            }
        }
        /// <summary>
        /// Renders a 3D cube on screen with the specified parameters
        /// </summary>
        /// <param name="id">Unique identifier for the cube</param>
        /// <param name="frontColor">Color for the front face</param>
        /// <param name="backColor">Color for the back face</param>
        /// <param name="topColor">Color for the top face</param>
        /// <param name="bottomColor">Color for the bottom face</param>
        /// <param name="leftColor">Color for the left face</param>
        /// <param name="rightColor">Color for the right face</param>
        /// <param name="x">X position in screen coordinates</param>
        /// <param name="y">Y position in screen coordinates</param>
        /// <param name="z">Z position (depth)</param>
        /// <param name="width">Width of the cube</param>
        /// <param name="height">Height of the cube</param>
        /// <param name="monitorIndex">Monitor index to display the cube on</param>
        /// <remarks>
        /// Creates a 3D cube with customizable colors for each face. The cube is positioned at the specified coordinates
        /// with the given dimensions. Each cube has a unique ID that can be used to manipulate it later.
        /// 
        /// Example:
        /// <code>
        /// // Create a red/blue/green/yellow/purple/orange cube at position (100, 100) with size 50x50
        /// overlay.Render3DCube(1, Color.Red.ToArgb(), Color.Blue.ToArgb(), Color.Green.ToArgb(),
        ///                      Color.Yellow.ToArgb(), Color.Purple.ToArgb(), Color.Orange.ToArgb(),
        ///                      100, 100, 0, 50, 50);
        /// </code>
        /// </remarks>
        public void Render3DCube(int id, int frontColor, int backColor, int topColor, int bottomColor, 
                                 int leftColor, int rightColor, float x, float y, float z, 
                                 float width, float height, int monitorIndex = 0)
        {
            QueueDrawingAction(() =>
            {
                try
                {
                    if (monitorIndex < 0 || monitorIndex >= Screen.AllScreens.Length)
                    {
                        monitorIndex = 0;
                    }
                    Rectangle bounds = Screen.AllScreens[monitorIndex].Bounds;
                    ShowDrawingOverlay(monitorIndex, bounds);
                    if (!_vertex3DShaders.ContainsKey(monitorIndex))
                    {
                        Setup3DResources(monitorIndex);
                    }
                    if (!_cubes.ContainsKey(monitorIndex))
                    {
                        _cubes[monitorIndex] = new Dictionary<int, Cube3DData>();
                    }
                    if (_cubes[monitorIndex].ContainsKey(id))
                    {
                        Remove3DCube(id, monitorIndex);
                    }
                    float ndcX = (x / (float)bounds.Width) * 2.0f - 1.0f;
                    float ndcY = 1.0f - (y / (float)bounds.Height) * 2.0f;
                    float ndcWidth = width / (float)bounds.Width * 2.0f;
                    float ndcHeight = height / (float)bounds.Height * 2.0f;
                    Color4 front = ARGBToColor4(frontColor);
                    Color4 back = ARGBToColor4(backColor);
                    Color4 top = ARGBToColor4(topColor);
                    Color4 bottom = ARGBToColor4(bottomColor);
                    Color4 left = ARGBToColor4(leftColor);
                    Color4 right = ARGBToColor4(rightColor);
                    var cube = new Cube3DData(id, front, back, top, bottom, left, right,
                                              ndcX, ndcY, z, ndcWidth, ndcHeight);
                    CreateCubeMesh(_devices[monitorIndex], cube);
                    _cubes[monitorIndex][id] = cube;
                    _needsUpdate[monitorIndex] = true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Render3DCube error: {ex.Message}");
                }
            });
        }
        /// <summary>
        /// Moves a 3D cube to the specified position
        /// </summary>
        /// <param name="id">ID of the cube to move</param>
        /// <param name="smooth">If true, animate the movement; otherwise move immediately</param>
        /// <param name="x">X position in screen coordinates</param>
        /// <param name="y">Y position in screen coordinates</param>
        /// <param name="z">Z position (depth)</param>
        /// <param name="monitorIndex">Monitor index the cube is on</param>
        /// <remarks>
        /// Moves an existing 3D cube to a new position. If smooth is true, the cube will
        /// animate smoothly to the new position over several frames. If false, the cube
        /// will instantly teleport to the new position.
        /// 
        /// Example:
        /// <code>
        /// // Move cube with ID 1 smoothly to position (200, 200)
        /// overlay.Move3DCube(1, true, 200, 200, 0);
        /// 
        /// // Move cube with ID 1 instantly to position (300, 300)
        /// overlay.Move3DCube(1, false, 300, 300, 0);
        /// </code>
        /// </remarks>
        public void Move3DCube(int id, bool smooth, float x, float y, float z, int monitorIndex = 0)
        {
            QueueDrawingAction(() =>
            {
                try
                {
                    if (monitorIndex < 0 || monitorIndex >= Screen.AllScreens.Length)
                    {
                        monitorIndex = 0;
                    }
                    if (!_cubes.ContainsKey(monitorIndex) || !_cubes[monitorIndex].ContainsKey(id))
                    {
                        Debug.WriteLine($"Cube with ID {id} not found on monitor {monitorIndex}");
                        return;
                    }
                    var cube = _cubes[monitorIndex][id];
                    Rectangle bounds = Screen.AllScreens[monitorIndex].Bounds;
                    float ndcX = (x / (float)bounds.Width) * 2.0f - 1.0f;
                    float ndcY = 1.0f - (y / (float)bounds.Height) * 2.0f;
                    if (smooth)
                    {
                        cube.TargetPosition = new Vector3(ndcX, ndcY, z);
                        cube.IsMoving = true;
                    }
                    else
                    {
                        cube.Position = new Vector3(ndcX, ndcY, z);
                        cube.UpdateWorldMatrix();
                    }
                    _needsUpdate[monitorIndex] = true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Move3DCube error: {ex.Message}");
                }
            });
        }
        /// <summary>
        /// Rotates a 3D cube to the specified orientation
        /// </summary>
        /// <param name="id">ID of the cube to rotate</param>
        /// <param name="smooth">If true, animate the rotation; otherwise rotate immediately</param>
        /// <param name="x">X rotation in radians</param>
        /// <param name="y">Y rotation in radians</param>
        /// <param name="z">Z rotation in radians</param>
        /// <param name="monitorIndex">Monitor index the cube is on</param>
        /// <remarks>
        /// Rotates an existing 3D cube to a new orientation. If smooth is true, the cube will
        /// animate smoothly to the new orientation over several frames. If false, the cube
        /// will instantly rotate to the new orientation.
        /// 
        /// Example:
        /// <code>
        /// // Rotate cube with ID 1 smoothly to a new orientation
        /// overlay.Rotate3DCube(1, true, 0.5f, 1.0f, 0.25f);
        /// 
        /// // Rotate cube with ID 1 instantly to a specific orientation
        /// overlay.Rotate3DCube(1, false, 0, (float)Math.PI/2, 0);
        /// </code>
        /// </remarks>
        public void Rotate3DCube(int id, bool smooth, float x, float y, float z, int monitorIndex = 0)
        {
            QueueDrawingAction(() =>
            {
                try
                {
                    if (monitorIndex < 0 || monitorIndex >= Screen.AllScreens.Length)
                    {
                        monitorIndex = 0;
                    }
                    if (!_cubes.ContainsKey(monitorIndex) || !_cubes[monitorIndex].ContainsKey(id))
                    {
                        Debug.WriteLine($"Cube with ID {id} not found on monitor {monitorIndex}");
                        return;
                    }
                    var cube = _cubes[monitorIndex][id];
                    if (smooth)
                    {
                        cube.TargetRotation = new Vector3(x, y, z);
                        cube.IsRotating = true;
                    }
                    else
                    {
                        cube.Rotation = new Vector3(x, y, z);
                        cube.UpdateWorldMatrix();
                    }
                    _needsUpdate[monitorIndex] = true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Rotate3DCube error: {ex.Message}");
                }
            });
        }
        /// <summary>
        /// Removes a 3D cube from the overlay
        /// </summary>
        /// <param name="id">ID of the cube to remove</param>
        /// <param name="monitorIndex">Monitor index the cube is on</param>
        /// <remarks>
        /// Completely removes a 3D cube from the overlay and disposes its resources.
        /// 
        /// Example:
        /// <code>
        /// // Remove cube with ID 1
        /// overlay.Remove3DCube(1);
        /// </code>
        /// </remarks>
        public void Remove3DCube(int id, int monitorIndex = 0)
        {
            QueueDrawingAction(() =>
            {
                try
                {
                    if (monitorIndex < 0 || monitorIndex >= Screen.AllScreens.Length)
                    {
                        monitorIndex = 0;
                    }
                    if (!_cubes.ContainsKey(monitorIndex) || !_cubes[monitorIndex].ContainsKey(id))
                    {
                        return;
                    }
                    var cube = _cubes[monitorIndex][id];
                    if (cube.VertexBuffer != null)
                    {
                        cube.VertexBuffer.Dispose();
                    }
                    if (cube.IndexBuffer != null)
                    {
                        cube.IndexBuffer.Dispose();
                    }
                    _cubes[monitorIndex].Remove(id);
                    _needsUpdate[monitorIndex] = true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Remove3DCube error: {ex.Message}");
                }
            });
        }
        /// <summary>
        /// Converts an ARGB color integer to a Direct3D Color4
        /// </summary>
        private Color4 ARGBToColor4(int colorArgb)
        {
            Color argbColor = Color.FromArgb(colorArgb);
            return new Color4(
                argbColor.R / 255.0f,
                argbColor.G / 255.0f,
                argbColor.B / 255.0f,
                argbColor.A / 255.0f
            );
        }
        /// <summary>
        /// Interface for custom shader implementations
        /// </summary>
        /// <remarks>
        /// Use this interface to create custom shaders that can be registered with the ScreenOverlay.
        /// Custom shaders allow you to apply custom rendering effects to the overlay.
        /// 
        /// Implementation example:
        /// <code>
        /// public class MyCustomShader : ICustomShader
        /// {
        ///     private string _id = "MyShader";
        ///     private Device _device;
        ///     private VertexShader _vertexShader;
        ///     private PixelShader _pixelShader;
        ///     private InputLayout _inputLayout;
        ///     
        ///     public string ShaderId => _id;
        ///     
        ///     public bool Initialize(Device device)
        ///     {
        ///         _device = device;
        ///         
        ///         // Compile shaders
        ///         using (var vertexShaderBytecode = SharpDX.D3DCompiler.ShaderBytecode.Compile(
        ///             vertexShaderCode, "main", "vs_4_0", SharpDX.D3DCompiler.ShaderFlags.Debug))
        ///         {
        ///             _vertexShader = new VertexShader(device, vertexShaderBytecode);
        ///             
        ///             // Create input layout
        ///             _inputLayout = new InputLayout(device, vertexShaderBytecode, inputElements);
        ///         }
        ///         
        ///         using (var pixelShaderBytecode = SharpDX.D3DCompiler.ShaderBytecode.Compile(
        ///             pixelShaderCode, "main", "ps_4_0", SharpDX.D3DCompiler.ShaderFlags.Debug))
        ///         {
        ///             _pixelShader = new PixelShader(device, pixelShaderBytecode);
        ///         }
        ///         
        ///         return true;
        ///     }
        ///     
        ///     public void Apply(DeviceContext context)
        ///     {
        ///         context.VertexShader.Set(_vertexShader);
        ///         context.PixelShader.Set(_pixelShader);
        ///         context.InputAssembler.InputLayout = _inputLayout;
        ///     }
        ///     
        ///     public void Dispose()
        ///     {
        ///         _vertexShader?.Dispose();
        ///         _pixelShader?.Dispose();
        ///         _inputLayout?.Dispose();
        ///     }
        /// }
        /// </code>
        /// </remarks>
        public interface ICustomShader : IDisposable
        {
            /// <summary>
            /// Initializes the shader with a device
            /// </summary>
            /// <param name="device">The Direct3D device</param>
            /// <returns>True if initialization succeeded</returns>
            bool Initialize(Device device);
            /// <summary>
            /// Applies the shader to the rendering pipeline
            /// </summary>
            /// <param name="context">The Direct3D device context</param>
            void Apply(DeviceContext context);
            /// <summary>
            /// Gets the unique identifier for this shader
            /// </summary>
            string ShaderId { get; }
        }
        /// <summary>
        /// Interface for custom renderable objects
        /// </summary>
        /// <remarks>
        /// Use this interface to create custom 3D objects that can be rendered by the ScreenOverlay.
        /// This allows you to extend the ScreenOverlay with your own rendering logic.
        /// 
        /// Implementation example:
        /// <code>
        /// public class MyCube : ICustomRenderable
        /// {
        ///     private int _id;
        ///     private int _monitorIndex;
        ///     private Vector3 _position;
        ///     private Vector3 _rotation;
        ///     private Vector3 _scale;
        ///     private Color4 _color;
        ///     
        ///     private Buffer _vertexBuffer;
        ///     private Buffer _indexBuffer;
        ///     private Buffer _constantBuffer;
        ///     private int _vertexCount;
        ///     private int _indexCount;
        ///     
        ///     public MyCube(int id, int monitorIndex, Vector3 position, Color4 color)
        ///     {
        ///         _id = id;
        ///         _monitorIndex = monitorIndex;
        ///         _position = position;
        ///         _color = color;
        ///         _rotation = Vector3.Zero;
        ///         _scale = new Vector3(1, 1, 1);
        ///     }
        ///     
        ///     public int RenderableId => _id;
        ///     public bool NeedsUpdate { get; set; } = true;
        ///     public int MonitorIndex => _monitorIndex;
        ///     
        ///     public bool Initialize(Device device)
        ///     {
        ///         // Create vertex and index buffers
        ///         // Create geometry
        ///         // Create constant buffer for transformations
        ///         return true;
        ///     }
        ///     
        ///     public void Render(DeviceContext context, Matrix viewMatrix, Matrix projectionMatrix)
        ///     {
        ///         // Create world matrix from position, rotation, scale
        ///         Matrix worldMatrix = Matrix.Scaling(_scale) *
        ///                            Matrix.RotationYawPitchRoll(_rotation.Y, _rotation.X, _rotation.Z) *
        ///                            Matrix.Translation(_position);
        ///         
        ///         // Update constant buffer with matrices
        ///         // Set vertex and index buffers
        ///         // Draw the object
        ///         context.DrawIndexed(_indexCount, 0, 0);
        ///     }
        ///     
        ///     public void Dispose()
        ///     {
        ///         _vertexBuffer?.Dispose();
        ///         _indexBuffer?.Dispose();
        ///         _constantBuffer?.Dispose();
        ///     }
        /// }
        /// </code>
        /// 
        /// Register your renderable with the ScreenOverlay:
        /// <code>
        /// var cube = new MyCube(1, 0, new Vector3(0, 0, 0), new Color4(1, 0, 0, 1));
        /// overlay.RegisterCustomRenderable(cube);
        /// </code>
        /// </remarks>
        public interface ICustomRenderable : IDisposable
        {
            /// <summary>
            /// Initializes resources for the renderable object
            /// </summary>
            /// <param name="device">The Direct3D device</param>
            /// <returns>True if initialization succeeded</returns>
            bool Initialize(Device device);
            /// <summary>
            /// Renders the object using the specified context
            /// </summary>
            /// <param name="context">The Direct3D device context</param>
            /// <param name="viewMatrix">The view matrix</param>
            /// <param name="projectionMatrix">The projection matrix</param>
            void Render(DeviceContext context, Matrix viewMatrix, Matrix projectionMatrix);
            /// <summary>
            /// Gets the unique identifier for this renderable
            /// </summary>
            int RenderableId { get; }
            /// <summary>
            /// Gets or sets whether this object needs to be updated
            /// </summary>
            bool NeedsUpdate { get; set; }
            /// <summary>
            /// Gets the monitor index this renderable belongs to
            /// </summary>
            int MonitorIndex { get; }
        }
        /// <summary>
        /// Registers a custom shader for use with this overlay
        /// </summary>
        /// <param name="shader">The custom shader to register</param>
        /// <param name="monitorIndex">The monitor index to register the shader with</param>
        /// <returns>True if registration succeeded</returns>
        /// <remarks>
        /// Register a custom shader that implements the ICustomShader interface.
        /// Custom shaders allow you to apply custom rendering effects to the overlay.
        /// 
        /// Example:
        /// <code>
        /// // Create and register a custom shader
        /// var shader = new MyCustomShader();
        /// bool success = overlay.RegisterCustomShader(shader);
        /// </code>
        /// </remarks>
        public bool RegisterCustomShader(ICustomShader shader, int monitorIndex = 0)
        {
            return QueueDrawingActionWithResult(() =>
            {
                try
                {
                    if (monitorIndex < 0 || monitorIndex >= Screen.AllScreens.Length)
                    {
                        monitorIndex = 0;
                    }
                    Rectangle bounds = Screen.AllScreens[monitorIndex].Bounds;
                    ShowDrawingOverlay(monitorIndex, bounds);
                    if (!_customShaders.ContainsKey(monitorIndex))
                    {
                        _customShaders[monitorIndex] = new Dictionary<string, ICustomShader>();
                    }
                    if (_customShaders[monitorIndex].ContainsKey(shader.ShaderId))
                    {
                        _customShaders[monitorIndex][shader.ShaderId].Dispose();
                    }
                    if (!shader.Initialize(_devices[monitorIndex]))
                    {
                        Debug.WriteLine($"Failed to initialize custom shader {shader.ShaderId}");
                        return false;
                    }
                    _customShaders[monitorIndex][shader.ShaderId] = shader;
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"RegisterCustomShader error: {ex.Message}");
                    return false;
                }
            });
        }
        /// <summary>
        /// Unregisters a custom shader
        /// </summary>
        /// <param name="shaderId">The ID of the shader to unregister</param>
        /// <param name="monitorIndex">The monitor index the shader is registered with</param>
        /// <returns>True if unregistration succeeded</returns>
        /// <remarks>
        /// Removes a previously registered custom shader and disposes its resources.
        /// 
        /// Example:
        /// <code>
        /// // Unregister a shader with ID "MyShader"
        /// overlay.UnregisterCustomShader("MyShader");
        /// </code>
        /// </remarks>
        public bool UnregisterCustomShader(string shaderId, int monitorIndex = 0)
        {
            return QueueDrawingActionWithResult(() =>
            {
                try
                {
                    if (monitorIndex < 0 || monitorIndex >= Screen.AllScreens.Length)
                    {
                        monitorIndex = 0;
                    }
                    if (!_customShaders.ContainsKey(monitorIndex) || 
                        !_customShaders[monitorIndex].ContainsKey(shaderId))
                    {
                        return false;
                    }
                    _customShaders[monitorIndex][shaderId].Dispose();
                    _customShaders[monitorIndex].Remove(shaderId);
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"UnregisterCustomShader error: {ex.Message}");
                    return false;
                }
            });
        }
        /// <summary>
        /// Registers a custom renderable object for rendering with this overlay
        /// </summary>
        /// <param name="renderable">The custom renderable to register</param>
        /// <returns>True if registration succeeded</returns>
        /// <remarks>
        /// Register a custom renderable object that implements the ICustomRenderable interface.
        /// This allows you to create your own 3D objects and have them rendered by the overlay.
        /// 
        /// Example:
        /// <code>
        /// // Create and register a custom 3D object
        /// var mySphere = new CustomSphere(1, 0, new Vector3(0, 0, 0), 1.0f);
        /// bool success = overlay.RegisterCustomRenderable(mySphere);
        /// </code>
        /// </remarks>
        public bool RegisterCustomRenderable(ICustomRenderable renderable)
        {
            return QueueDrawingActionWithResult(() =>
            {
                try
                {
                    int monitorIndex = renderable.MonitorIndex;
                    if (monitorIndex < 0 || monitorIndex >= Screen.AllScreens.Length)
                    {
                        monitorIndex = 0;
                    }
                    Rectangle bounds = Screen.AllScreens[monitorIndex].Bounds;
                    ShowDrawingOverlay(monitorIndex, bounds);
                    if (!_customRenderables.ContainsKey(monitorIndex))
                    {
                        _customRenderables[monitorIndex] = new Dictionary<int, ICustomRenderable>();
                    }
                    if (_customRenderables[monitorIndex].ContainsKey(renderable.RenderableId))
                    {
                        _customRenderables[monitorIndex][renderable.RenderableId].Dispose();
                    }
                    if (!renderable.Initialize(_devices[monitorIndex]))
                    {
                        Debug.WriteLine($"Failed to initialize custom renderable {renderable.RenderableId}");
                        return false;
                    }
                    _customRenderables[monitorIndex][renderable.RenderableId] = renderable;
                    _needsUpdate[monitorIndex] = true;
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"RegisterCustomRenderable error: {ex.Message}");
                    return false;
                }
            });
        }
        /// <summary>
        /// Unregisters a custom renderable
        /// </summary>
        /// <param name="renderableId">The ID of the renderable to unregister</param>
        /// <param name="monitorIndex">The monitor index the renderable is registered with</param>
        /// <returns>True if unregistration succeeded</returns>
        /// <remarks>
        /// Removes a previously registered custom renderable object and disposes its resources.
        /// 
        /// Example:
        /// <code>
        /// // Unregister a renderable with ID 1
        /// overlay.UnregisterCustomRenderable(1);
        /// </code>
        /// </remarks>
        public bool UnregisterCustomRenderable(int renderableId, int monitorIndex = 0)
        {
            return QueueDrawingActionWithResult(() =>
            {
                try
                {
                    if (monitorIndex < 0 || monitorIndex >= Screen.AllScreens.Length)
                    {
                        monitorIndex = 0;
                    }
                    if (!_customRenderables.ContainsKey(monitorIndex) || 
                        !_customRenderables[monitorIndex].ContainsKey(renderableId))
                    {
                        return false;
                    }
                    _customRenderables[monitorIndex][renderableId].Dispose();
                    _customRenderables[monitorIndex].Remove(renderableId);
                    _needsUpdate[monitorIndex] = true;
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"UnregisterCustomRenderable error: {ex.Message}");
                    return false;
                }
            });
        }
        /// <summary>
        /// Updates a custom renderable to trigger a re-render
        /// </summary>
        /// <param name="renderableId">The ID of the renderable to update</param>
        /// <param name="monitorIndex">The monitor index the renderable is on</param>
        /// <remarks>
        /// Marks a custom renderable as needing an update, which will trigger a re-render
        /// on the next frame. Call this after modifying properties of your custom renderable
        /// to ensure the changes are displayed.
        /// 
        /// Example:
        /// <code>
        /// // Update position of custom sphere in our code
        /// mySphere.Position = new Vector3(100, 100, 0);
        /// 
        /// // Tell the overlay to update the rendering of this object
        /// overlay.UpdateCustomRenderable(mySphere.RenderableId);
        /// </code>
        /// </remarks>
        public void UpdateCustomRenderable(int renderableId, int monitorIndex = 0)
        {
            QueueDrawingAction(() =>
            {
                try
                {
                    if (monitorIndex < 0 || monitorIndex >= Screen.AllScreens.Length)
                    {
                        monitorIndex = 0;
                    }
                    if (!_customRenderables.ContainsKey(monitorIndex) || 
                        !_customRenderables[monitorIndex].ContainsKey(renderableId))
                    {
                        return;
                    }
                    _customRenderables[monitorIndex][renderableId].NeedsUpdate = true;
                    _needsUpdate[monitorIndex] = true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"UpdateCustomRenderable error: {ex.Message}");
                }
            });
        }
        /// <summary>
        /// Gets a custom shader by ID
        /// </summary>
        /// <param name="shaderId">The ID of the shader to retrieve</param>
        /// <param name="monitorIndex">The monitor index the shader is registered with</param>
        /// <returns>The custom shader, or null if not found</returns>
        /// <remarks>
        /// Retrieves a previously registered custom shader by its ID.
        /// 
        /// Example:
        /// <code>
        /// // Get a registered shader
        /// ICustomShader shader = overlay.GetCustomShader("MyShader");
        /// if (shader != null)
        /// {
        ///     // Use shader...
        /// }
        /// </code>
        /// </remarks>
        public ICustomShader GetCustomShader(string shaderId, int monitorIndex = 0)
        {
            try
            {
                if (monitorIndex < 0 || monitorIndex >= Screen.AllScreens.Length)
                {
                    monitorIndex = 0;
                }
                if (!_customShaders.ContainsKey(monitorIndex) || 
                    !_customShaders[monitorIndex].ContainsKey(shaderId))
                {
                    return null;
                }
                return _customShaders[monitorIndex][shaderId];
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetCustomShader error: {ex.Message}");
                return null;
            }
        }
        /// <summary>
        /// Queues a drawing action that returns a result
        /// </summary>
        private T QueueDrawingActionWithResult<T>(Func<T> action)
        {
            if (action == null)
                return default;
            ManualResetEvent waitEvent = new ManualResetEvent(false);
            T result = default;
            Exception exception = null;
            QueueDrawingAction(() =>
            {
                try
                {
                    result = action();
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
                finally
                {
                    waitEvent.Set();
                }
            });
            waitEvent.WaitOne();
            if (exception != null)
            {
                Debug.WriteLine($"QueueDrawingActionWithResult error: {exception.Message}");
            }
            return result;
        }
        /// <summary>
        /// Gets the Direct3D device for a specific monitor
        /// </summary>
        /// <param name="monitorIndex">The monitor index</param>
        /// <returns>The Direct3D device, or null if not available</returns>
        /// <remarks>
        /// Provides access to the Direct3D device for a specific monitor.
        /// This is useful for advanced rendering scenarios or when creating
        /// custom renderables that need direct access to the device.
        /// 
        /// Example:
        /// <code>
        /// // Get the device for the primary monitor
        /// Device device = overlay.GetDeviceForMonitor(0);
        /// if (device != null)
        /// {
        ///     // Use device for advanced DirectX operations
        /// }
        /// </code>
        /// </remarks>
        public Device GetDeviceForMonitor(int monitorIndex = 0)
        {
            return QueueDrawingActionWithResult(() =>
            {
                try
                {
                    if (monitorIndex < 0 || monitorIndex >= Screen.AllScreens.Length)
                    {
                        monitorIndex = 0;
                    }
                    if (!_devices.ContainsKey(monitorIndex))
                    {
                        Rectangle bounds = Screen.AllScreens[monitorIndex].Bounds;
                        ShowDrawingOverlay(monitorIndex, bounds);
                    }
                    if (_devices.TryGetValue(monitorIndex, out Device device))
                    {
                        return device;
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"GetDeviceForMonitor error: {ex.Message}");
                    return null;
                }
            });
        }
    }
} 