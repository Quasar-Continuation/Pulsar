using Pulsar.Client.Utilities;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.D3DCompiler;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Point = System.Drawing.Point;
using Matrix = SharpDX.Matrix;
using Color = System.Drawing.Color;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
namespace Pulsar.Client.GDIEffects
{
    /// <summary>
    /// Implements a screen effect using SharpDX with captured screen content
    /// </summary>
    public class ScreenCorruption : IDisposable
    {
        private readonly ScreenOverlay _overlay;
        private readonly ScreenCorruptionShader _corruptionShader;
        private bool _corruptionActive = false;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _animationTask;
        private readonly object _lockObject = new object();
        /// <summary>
        /// Creates a new screen effect
        /// </summary>
        /// <param name="overlay">The ScreenOverlay to use for rendering</param>
        public ScreenCorruption(ScreenOverlay overlay)
        {
            _overlay = overlay ?? throw new ArgumentNullException(nameof(overlay));
            _corruptionShader = new ScreenCorruptionShader(_overlay);
        }
        /// <summary>
        /// Gets whether the effect is currently active
        /// </summary>
        public bool IsActive => _corruptionActive;
        /// <summary>
        /// Starts the effect on all monitors
        /// </summary>
        public void Start()
        {
            if (_corruptionActive)
                return;

            try
            {

                _overlay.ClearDrawings(0);


                for (int i = 0; i < System.Windows.Forms.Screen.AllScreens.Length; i++)
                {
                    Debug.WriteLine($"Setting up screen corruption for monitor {i}");


                    bool shaderRegistered = _overlay.RegisterCustomShader(_corruptionShader, i);
                    Debug.WriteLine($"Shader {shaderRegistered} registered");


                    var renderable = _corruptionShader.GetRenderableForMonitor(i);
                    bool renderableRegistered = _overlay.RegisterCustomRenderable(renderable);
                    Debug.WriteLine($"Renderable {renderableRegistered} registered");


                    _overlay.UpdateCustomRenderable(renderable.RenderableId, i);


                    var bounds = System.Windows.Forms.Screen.AllScreens[i].Bounds;
                    int centerX = bounds.X + bounds.Width / 2;
                    int centerY = bounds.Y + bounds.Height / 2;
                    int white = System.Drawing.Color.FromArgb(0, 255, 255, 255).ToArgb();


                    _overlay.Draw(bounds.X, bounds.Y, bounds.X, bounds.Y, 2, white, i); 
                    _overlay.Draw(bounds.Right, bounds.Y, bounds.Right, bounds.Y, 2, white, i); 
                    _overlay.Draw(bounds.X, bounds.Bottom, bounds.X, bounds.Bottom, 2, white, i); 
                    _overlay.Draw(bounds.Right, bounds.Bottom, bounds.Right, bounds.Bottom, 2, white, i); 
                }


                _cancellationTokenSource = new CancellationTokenSource();
                CancellationToken token = _cancellationTokenSource.Token;
                _animationTask = Task.Run(async () =>
                {
                    Stopwatch timer = Stopwatch.StartNew();

                    while (!token.IsCancellationRequested)
                    {
                        try
                        {

                            float time = (float)timer.Elapsed.TotalSeconds;
                            _corruptionShader.UpdateTime(time);


                            for (int i = 0; i < System.Windows.Forms.Screen.AllScreens.Length; i++)
                            {
                                var renderable = _corruptionShader.GetRenderableForMonitor(i);
                                renderable.NeedsUpdate = true;
                                _overlay.UpdateCustomRenderable(renderable.RenderableId, i);
                            }

                            Debug.WriteLine($"Updated effect with time {time}");


                            await Task.Delay(16, token); 
                        }
                        catch (OperationCanceledException)
                        {
                            break;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error in corruption animation: {ex.Message}");
                        }
                    }

                    timer.Stop();
                }, token);
                _corruptionActive = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error starting corruption: {ex.Message}\n{ex.StackTrace}");
            }
        }
        /// <summary>
        /// Stops the effect and cleans up resources
        /// </summary>
        public void Stop()
        {
            if (!_corruptionActive)
                return;

            _cancellationTokenSource?.Cancel();
            try
            {

                if (_animationTask != null && !_animationTask.IsCompleted)
                {
                    _animationTask.Wait(1000);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error waiting for animation task: {ex.Message}");
            }

            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
            _animationTask = null;

            for (int i = 0; i < System.Windows.Forms.Screen.AllScreens.Length; i++)
            {
                try
                {

                    var renderable = _corruptionShader.GetRenderableForMonitor(i);
                    _overlay.UnregisterCustomRenderable(renderable.RenderableId, i);


                    _overlay.UnregisterCustomShader(_corruptionShader.ShaderId, i);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error unregistering effect for monitor {i}: {ex.Message}");
                }
            }
            _corruptionActive = false;
        }
        /// <summary>
        /// Toggles the effect on/off
        /// </summary>
        /// <returns>True if the effect is active after toggling, false otherwise</returns>
        public bool Toggle()
        {
            lock (_lockObject)
            {
                if (!_corruptionActive)
                {
                    Start();
                    return true;
                }
                else
                {
                    Stop();
                    return false;
                }
            }
        }
        /// <summary>
        /// Disposes of resources used by this effect
        /// </summary>
        public void Dispose()
        {
            Stop();
            _corruptionShader?.Dispose();
        }

        private class ScreenCorruptionShader : ScreenOverlay.ICustomShader
        {

            private const string ShaderCode = @"
Texture2D sceneTexture : register(t0);
SamplerState samplerState : register(s0);
cbuffer TimeBuffer : register(b0)
{
    float time;
    float intensity;
    float2 padding;
};
float rand(float2 co) {
    return frac(sin(dot(co, float2(12.9898, 78.233))) * 43758.5453);
}
float4 main(float4 position : SV_POSITION, float2 texCoord : TEXCOORD) : SV_Target
{
    float2 largeBlock = floor(texCoord * 5.0) * 0.2;
    float blockValue = rand(largeBlock + time * 0.1);
    float showBlock = step(0.7, blockValue);

    float2 offset1 = float2(sin(time + largeBlock.x) * 0.1, cos(time * 0.7 + largeBlock.y) * 0.1);
    float2 offset2 = float2(cos(time * 1.3) * 0.05, sin(time * 0.9) * 0.05);

    float4 shiftedColor1 = sceneTexture.Sample(samplerState, texCoord + offset1);
    float4 shiftedColor2 = sceneTexture.Sample(samplerState, texCoord + offset2);

    float4 corruptedColor = float4(
        shiftedColor1.r,
        shiftedColor2.g,
        shiftedColor1.b,
        1.0
    );

    float4 colorTint = float4(
        sin(time + texCoord.x * 5.0) * 0.3 + 0.7,  // Red tint
        cos(time * 1.3 + texCoord.y * 5.0) * 0.3 + 0.7, // Green tint
        sin(time * 0.7 + texCoord.x * texCoord.y * 5.0) * 0.3 + 0.7, // Blue tint
        1.0
    );

    float horizontalGlitch = step(0.97, frac(texCoord.y * 20.0 + time * 3.0));
    float verticalGlitch = step(0.98, frac(texCoord.x * 30.0 - time * 2.0));
    float glitchCombined = max(horizontalGlitch, verticalGlitch);

    float4 result = float4(0, 0, 0, 0);

    if (showBlock > 0.5) {
        result = corruptedColor * colorTint;
    }

    if (glitchCombined > 0.5) {
        result = float4(1.0 - shiftedColor2.rgb, 1.0);
    }

    return result;
}";

            private const string FullscreenVSCode = @"
struct VS_INPUT
{
    float4 Pos : POSITION;
    float2 Tex : TEXCOORD;
};
struct VS_OUTPUT
{
    float4 Pos : SV_POSITION;
    float2 Tex : TEXCOORD;
};
VS_OUTPUT main(VS_INPUT input)
{
    VS_OUTPUT output;
    output.Pos = input.Pos;
    output.Tex = input.Tex;
    return output;
}";

            [StructLayout(LayoutKind.Sequential)]
            private struct SimpleVertex
            {
                public Vector4 Position;
                public Vector2 TexCoord;

                public SimpleVertex(Vector4 position, Vector2 texCoord)
                {
                    Position = position;
                    TexCoord = texCoord;
                }
            }

            private PixelShader _pixelShader;
            private Buffer _timeBuffer;
            private SamplerState _samplerState;
            private float _time;
            private readonly object _timeLock = new object();


            //private readonly RenderTargetView _renderTargetView;
            private ShaderResourceView _screenTextureView;
            private Texture2D _screenCapture;
            //private readonly Texture2D _renderTarget;
            private VertexShader _fullscreenVS;
            private InputLayout _inputLayout;
            private Buffer _vertexBuffer;


            private ScreenOverlay _overlay;


            private Dictionary<int, CorruptionRenderable> _renderables = new Dictionary<int, CorruptionRenderable>();
            /// <summary>
            /// Creates a new Screen Corruption shader
            /// </summary>
            public ScreenCorruptionShader(ScreenOverlay overlay)
            {
                _overlay = overlay;
            }
            /// <summary>
            /// Gets the unique identifier for this shader
            /// </summary>
            public string ShaderId => "ScreenCorruptionShader";
            /// <summary>
            /// Initializes shader resources
            /// </summary>
            public bool Initialize(Device device)
            {
                try
                {

                    using (var bytecode = ShaderBytecode.Compile(
                        ShaderCode,
                        "main",
                        "ps_5_0",
                        ShaderFlags.Debug))
                    {
                        _pixelShader = new PixelShader(device, bytecode);
                    }


                    using (var bytecode = ShaderBytecode.Compile(
                        FullscreenVSCode,
                        "main",
                        "vs_5_0",
                        ShaderFlags.Debug))
                    {
                        _fullscreenVS = new VertexShader(device, bytecode);


                        var inputElements = new InputElement[]
                        {

                            new InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32A32_Float, 0, 0),

                            new InputElement("TEXCOORD", 0, SharpDX.DXGI.Format.R32G32_Float, 16, 0)
                        };

                        _inputLayout = new InputLayout(device, bytecode, inputElements);
                    }

                    int bufferSize = Marshal.SizeOf<TimeBufferData>();
                    _timeBuffer = new Buffer(device, bufferSize, ResourceUsage.Dynamic, 
                        BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);

                    _samplerState = new SamplerState(device, new SamplerStateDescription
                    {
                        Filter = Filter.MinMagMipLinear,
                        AddressU = TextureAddressMode.Clamp,
                        AddressV = TextureAddressMode.Clamp,
                        AddressW = TextureAddressMode.Clamp,
                        ComparisonFunction = Comparison.Never,
                        MinimumLod = 0,
                        MaximumLod = float.MaxValue,
                        MipLodBias = 0,
                        MaximumAnisotropy = 16
                    });


                    Vector4[] positions = new Vector4[]
                    {
                        new Vector4(-1.0f, -1.0f, 0.0f, 1.0f),
                        new Vector4(-1.0f, 1.0f, 0.0f, 1.0f),
                        new Vector4(1.0f, -1.0f, 0.0f, 1.0f),
                        new Vector4(1.0f, 1.0f, 0.0f, 1.0f)
                    };

                    Vector2[] texCoords = new Vector2[]
                    {
                        new Vector2(0.0f, 1.0f),
                        new Vector2(0.0f, 0.0f),
                        new Vector2(1.0f, 1.0f),
                        new Vector2(1.0f, 0.0f)
                    };

                    SimpleVertex[] vertices = new SimpleVertex[4]
                    {
                        new SimpleVertex(positions[0], texCoords[0]), 
                        new SimpleVertex(positions[1], texCoords[1]), 
                        new SimpleVertex(positions[2], texCoords[2]), 
                        new SimpleVertex(positions[3], texCoords[3])  
                    };

                    _vertexBuffer = Buffer.Create(device, BindFlags.VertexBuffer, vertices);

                    _time = 0.0f;
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error initializing ScreenCorruptionShader: {ex.Message}");
                    return false;
                }
            }
            /// <summary>
            /// Applies the shader to the rendering pipeline
            /// </summary>
            public void Apply(DeviceContext context)
            {
                try
                {

                    float currentTime;
                    lock (_timeLock)
                    {
                        currentTime = _time;
                    }

                    var timeData = new TimeBufferData { 
                        Time = currentTime,
                        Intensity = 1.0f 
                    };

                    var timeDataBox = context.MapSubresource(
                        _timeBuffer, 
                        0, 
                        MapMode.WriteDiscard, 
                        SharpDX.Direct3D11.MapFlags.None);
                    Marshal.StructureToPtr(timeData, timeDataBox.DataPointer, false);
                    context.UnmapSubresource(_timeBuffer, 0);


                    context.PixelShader.SetConstantBuffer(0, _timeBuffer);


                    context.PixelShader.Set(_pixelShader);
                    context.PixelShader.SetSampler(0, _samplerState);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error applying ScreenCorruptionShader: {ex.Message}");
                }
            }

            /// <summary>
            /// Renders the effect
            /// </summary>
            public void RenderEffect(DeviceContext context)
            {
                try
                {
                    Debug.WriteLine("Rendering effect");


                    SetupScreenCapture(context);


                    context.PixelShader.SetShaderResource(0, _screenTextureView);


                    context.InputAssembler.InputLayout = _inputLayout;
                    context.VertexShader.Set(_fullscreenVS);


                    context.PixelShader.Set(_pixelShader);


                    float currentTime;
                    lock (_timeLock)
                    {
                        currentTime = _time;
                    }


                    var timeData = new TimeBufferData { 
                        Time = currentTime,
                        Intensity = 1.0f 
                    };


                    var timeDataBox = context.MapSubresource(
                        _timeBuffer, 
                        0, 
                        MapMode.WriteDiscard, 
                        SharpDX.Direct3D11.MapFlags.None);

                    Marshal.StructureToPtr(timeData, timeDataBox.DataPointer, false);
                    context.UnmapSubresource(_timeBuffer, 0);


                    context.PixelShader.SetConstantBuffer(0, _timeBuffer);


                    context.PixelShader.SetSampler(0, _samplerState);


                    int stride = Marshal.SizeOf<SimpleVertex>();
                    context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(_vertexBuffer, stride, 0));
                    context.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleStrip;


                    context.Draw(4, 0);
                    Debug.WriteLine("Custom effect rendered");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error rendering effect: {ex.Message}");
                }
            }
            /// <summary>
            /// Sets up screen capture for the shader
            /// </summary>
            private void SetupScreenCapture(DeviceContext context)
            {
                try
                {

                    _screenTextureView?.Dispose();

                    Debug.WriteLine("Creating screen texture with actual screen content");


                    if (_screenCapture == null || true) 
                    {

                        var screen = System.Windows.Forms.Screen.PrimaryScreen;
                        int width = 512; 
                        int height = 512;


                        Debug.WriteLine($"Capturing screen region: {width}x{height}");
                        Bitmap screenBitmap = null;

                        try
                        {

                            int x = (screen.Bounds.Width / 2) - (width / 2);
                            int y = (screen.Bounds.Height / 2) - (height / 2);
                            screenBitmap = _overlay.CaptureScreenRegion(x, y, width, height);
                            Debug.WriteLine("Screen capture successful");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Screen capture failed: {ex.Message}");

                            screenBitmap = new Bitmap(width, height);
                            using (Graphics g = Graphics.FromImage(screenBitmap))
                            {

                                g.FillRectangle(new LinearGradientBrush(
                                    new Point(0, 0), new Point(width, height), 
                                    Color.Blue, Color.Red), 0, 0, width, height);
                            }
                        }


                        if (_screenCapture != null)
                        {
                            _screenCapture.Dispose();
                        }

                        var textureDesc = new Texture2DDescription
                        {
                            Width = screenBitmap.Width,
                            Height = screenBitmap.Height,
                            MipLevels = 1,
                            ArraySize = 1,
                            Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm,
                            SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                            Usage = ResourceUsage.Default,
                            BindFlags = BindFlags.ShaderResource,
                            CpuAccessFlags = CpuAccessFlags.None,
                            OptionFlags = ResourceOptionFlags.None
                        };


                        _screenCapture = new Texture2D(context.Device, textureDesc);


                        BitmapData bitmapData = screenBitmap.LockBits(
                            new System.Drawing.Rectangle(0, 0, screenBitmap.Width, screenBitmap.Height),
                            ImageLockMode.ReadOnly,
                            System.Drawing.Imaging.PixelFormat.Format32bppArgb);


                        int rowPitch = screenBitmap.Width * 4; 
                        context.UpdateSubresource(_screenCapture, 0, null, bitmapData.Scan0, rowPitch, 0);


                        screenBitmap.UnlockBits(bitmapData);


                        screenBitmap.Dispose();

                        Debug.WriteLine("Screen texture created successfully");
                    }


                    _screenTextureView = new ShaderResourceView(context.Device, _screenCapture);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error setting up screen capture: {ex.Message}");
                }
            }
            /// <summary>
            /// Updates the time parameter for the shader
            /// </summary>
            public void UpdateTime(float time)
            {
                lock (_timeLock)
                {
                    _time = time;
                }
            }

            /// <summary>
            /// Gets a renderable for the specified monitor
            /// </summary>
            public ScreenOverlay.ICustomRenderable GetRenderableForMonitor(int monitorIndex)
            {
                if (!_renderables.TryGetValue(monitorIndex, out var renderable))
                {
                    renderable = new CorruptionRenderable(this, monitorIndex);
                    _renderables[monitorIndex] = renderable;
                }

                return renderable;
            }
            /// <summary>
            /// Disposes of resources
            /// </summary>
            public void Dispose()
            {
                _pixelShader?.Dispose();
                _timeBuffer?.Dispose();
                _samplerState?.Dispose();
                _fullscreenVS?.Dispose();
                _inputLayout?.Dispose();
                _vertexBuffer?.Dispose();
                _screenCapture?.Dispose();
                //_renderTarget?.Dispose();
                //_renderTargetView?.Dispose();
                _screenTextureView?.Dispose();
            }
            /// <summary>
            /// Custom renderable for the effect
            /// </summary>
            private class CorruptionRenderable : ScreenOverlay.ICustomRenderable
            {
                private ScreenCorruptionShader _parent;
                private int _monitorIndex;

                public CorruptionRenderable(ScreenCorruptionShader parent, int monitorIndex)
                {
                    _parent = parent;
                    _monitorIndex = monitorIndex;
                }

                public int RenderableId => 9999 + _monitorIndex; 


                public bool NeedsUpdate { get; set; } = true;

                public int MonitorIndex => _monitorIndex;

                public bool Initialize(Device device)
                {
                    Debug.WriteLine($"Initializing corruption renderable for monitor {_monitorIndex}");
                    return true; 
                }

                public void Render(DeviceContext context, Matrix viewMatrix, Matrix projectionMatrix)
                {
                    Debug.WriteLine($"Rendering corruption for monitor {_monitorIndex}");
                    _parent.RenderEffect(context);


                    NeedsUpdate = true;
                }

                public void Dispose()
                {
                    Debug.WriteLine($"Disposing corruption renderable for monitor {_monitorIndex}");

                }
            }
        }
        /// <summary>
        /// Structure for the time buffer
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct TimeBufferData
        {
            public float Time;
            public float Intensity;
            public Vector2 Padding;
        }
    }
}
