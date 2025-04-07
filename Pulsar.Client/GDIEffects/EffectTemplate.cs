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
    /// Template for creating custom screen effects using DX and ScreenOverlay
    /// To use: Copy this file, rename the class, and customize the shader code and effect behavior
    /// </summary>
    public class EffectTemplate : IDisposable
    {
        // Reference to the ScreenOverlay that manages rendering
        private readonly ScreenOverlay _overlay;
        
        // The custom shader for this effect
        private readonly CustomEffectShader _effectShader;
        
        // Effect state tracking
        private bool _effectActive = false;
        
        // Animation task management
        private CancellationTokenSource _cancellationTokenSource;
        private Task _animationTask;
        
        private readonly object _lockObject = new object();

        /// <summary>
        /// Creates a new screen effect instance
        /// </summary>
        /// <param name="overlay">The ScreenOverlay to use for rendering</param>
        public EffectTemplate(ScreenOverlay overlay)
        {
            _overlay = overlay ?? throw new ArgumentNullException(nameof(overlay));
            _effectShader = new CustomEffectShader(_overlay);
        }

        /// <summary>
        /// Gets whether the effect is currently active
        /// </summary>
        public bool IsActive => _effectActive;

        /// <summary>
        /// Starts the effect on all monitors
        /// </summary>
        public void Start()
        {
            if (_effectActive)
                return;

            Debug.WriteLine("Setting up shader and renderable for each monitor");
            
            try
            {
                _overlay.ClearDrawings(0);
                
                // Register shader and renderable with overlay for all monitors
                for (int i = 0; i < System.Windows.Forms.Screen.AllScreens.Length; i++)
                {
                    Debug.WriteLine($"Setting up effect for monitor {i}");
                    
                    // Register the shader for this monitor
                    bool shaderRegistered = _overlay.RegisterCustomShader(_effectShader, i);
                    Debug.WriteLine($"Shader registered: {shaderRegistered}");
                    
                    // Register the renderable for this monitor
                    var renderable = _effectShader.GetRenderableForMonitor(i);
                    bool renderableRegistered = _overlay.RegisterCustomRenderable(renderable);
                    Debug.WriteLine($"Renderable registered: {renderableRegistered}");
                    
                    // Mark this monitor to be updated
                    _overlay.UpdateCustomRenderable(renderable.RenderableId, i);
                    
                    var bounds = System.Windows.Forms.Screen.AllScreens[i].Bounds;
                    int white = System.Drawing.Color.FromArgb(0, 255, 255, 255).ToArgb();
                    
                    _overlay.Draw(bounds.X, bounds.Y, bounds.X, bounds.Y, 2, white, i); // Top left
                    _overlay.Draw(bounds.Right, bounds.Y, bounds.Right, bounds.Y, 2, white, i); // Top right
                    _overlay.Draw(bounds.X, bounds.Bottom, bounds.X, bounds.Bottom, 2, white, i); // Bottom left
                    _overlay.Draw(bounds.Right, bounds.Bottom, bounds.Right, bounds.Bottom, 2, white, i); // Bottom right
                }
                
                // Set up animation task for updating effect parameters over time
                _cancellationTokenSource = new CancellationTokenSource();
                CancellationToken token = _cancellationTokenSource.Token;

                _animationTask = Task.Run(async () =>
                {
                    Stopwatch timer = Stopwatch.StartNew();
                    
                    while (!token.IsCancellationRequested)
                    {
                        try
                        {
                            // Update time parameter for shader effect
                            float time = (float)timer.Elapsed.TotalSeconds;
                            _effectShader.UpdateTime(time);
                            
                            // Update all monitors to show new effect
                            for (int i = 0; i < System.Windows.Forms.Screen.AllScreens.Length; i++)
                            {
                                var renderable = _effectShader.GetRenderableForMonitor(i);
                                renderable.NeedsUpdate = true;
                                _overlay.UpdateCustomRenderable(renderable.RenderableId, i);
                            }
                            
                            Debug.WriteLine($"Updated effect with time {time}");
                            
                            await Task.Delay(16, token); // ~60fps
                        }
                        catch (OperationCanceledException)
                        {
                            break;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error in effect animation: {ex.Message}");
                        }
                    }
                    
                    timer.Stop();
                }, token);

                _effectActive = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error starting effect: {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// Stops the effect and cleans up resources
        /// </summary>
        public void Stop()
        {
            if (!_effectActive)
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

            // Unregister shader and renderable from all monitors
            for (int i = 0; i < System.Windows.Forms.Screen.AllScreens.Length; i++)
            {
                try
                {
                    // Get the renderable for this monitor and unregister it
                    var renderable = _effectShader.GetRenderableForMonitor(i);
                    _overlay.UnregisterCustomRenderable(renderable.RenderableId, i);
                    
                    // Unregister the shader
                    _overlay.UnregisterCustomShader(_effectShader.ShaderId, i);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error unregistering effect for monitor {i}: {ex.Message}");
                }
            }

            _effectActive = false;
        }

        /// <summary>
        /// Toggles the effect on/off
        /// </summary>
        /// <returns>True if the effect is active after toggling, false otherwise</returns>
        public bool Toggle()
        {
            lock (_lockObject)
            {
                if (!_effectActive)
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
            _effectShader?.Dispose();
        }

        // Implementation of the custom effect shader
        private class CustomEffectShader : ScreenOverlay.ICustomShader
        {
            // REPLACE THIS WITH YOUR OWN SHADER
            // REPLACE THIS WITH YOUR OWN SHADER
            // REPLACE THIS WITH YOUR OWN SHADER
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
    // CUSTOMIZE THIS: This is where you implement your custom effect
    // Default implementation: transparent with some captured screen blocks
    
    // Sample original screen content
    float4 originalColor = sceneTexture.Sample(samplerState, texCoord);
    
    // Create some blocks based on time and position
    float2 block = floor(texCoord * 10.0) * 0.1;
    float showBlock = step(0.8, rand(block + time * 0.1));
    
    // Default to transparent
    float4 result = float4(0, 0, 0, 0);
    
    // Show some blocks with screen content
    if (showBlock > 0.5) {
        result = originalColor;
    }
    
    return result;
}";

            // Default vertex shader for rendering fullscreen quad
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

            // Vertex structure for the fullscreen quad
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

            // DX shader resources
            private PixelShader _pixelShader;
            private Buffer _timeBuffer;
            private SamplerState _samplerState;
            private float _time;
            private readonly object _timeLock = new object();
            
            // Resources for screen capture and rendering
            private RenderTargetView _renderTargetView;
            private ShaderResourceView _screenTextureView;
            private Texture2D _screenCapture;
            private Texture2D _renderTarget;
            private VertexShader _fullscreenVS;
            private InputLayout _inputLayout;
            private Buffer _vertexBuffer;
            
            private ScreenOverlay _overlay;
            
            // Dictionary of renderables (one per monitor)
            private Dictionary<int, EffectRenderable> _renderables = new Dictionary<int, EffectRenderable>();

            /// <summary>
            /// Creates a new effect shader
            /// </summary>
            public CustomEffectShader(ScreenOverlay overlay)
            {
                _overlay = overlay;
            }

            /// <summary>
            /// Gets the unique identifier for this shader
            /// CUSTOMIZE THIS: Change the shader ID to be unique for your effect
            /// </summary>
            public string ShaderId => "CustomEffectShader";

            /// <summary>
            /// Initializes shader resources
            /// </summary>
            public bool Initialize(Device device)
            {
                try
                {
                    // Compile the pixel shader
                    using (var bytecode = ShaderBytecode.Compile(
                        ShaderCode,
                        "main",
                        "ps_5_0",
                        ShaderFlags.Debug))
                    {
                        _pixelShader = new PixelShader(device, bytecode);
                    }
                    
                    // Compile the fullscreen quad vertex shader
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

                    // Create constant buffer for time value
                    int bufferSize = Marshal.SizeOf<EffectParameterData>();
                    _timeBuffer = new Buffer(device, bufferSize, ResourceUsage.Dynamic, 
                        BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);

                    // Create sampler state
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
                    
                    // Create fullscreen quad vertices
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
                        new SimpleVertex(positions[0], texCoords[0]), // Bottom left
                        new SimpleVertex(positions[1], texCoords[1]), // Top left
                        new SimpleVertex(positions[2], texCoords[2]), // Bottom right
                        new SimpleVertex(positions[3], texCoords[3])  // Top right
                    };

                    // Create vertex buffer
                    _vertexBuffer = Buffer.Create(device, BindFlags.VertexBuffer, vertices);

                    _time = 0.0f;

                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error initializing CustomEffectShader: {ex.Message}");
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
                    // Update time buffer
                    float currentTime;
                    lock (_timeLock)
                    {
                        currentTime = _time;
                    }

                    // Create parameter data
                    var paramData = new EffectParameterData { 
                        Time = currentTime,
                        Intensity = 1.0f 
                    };

                    // Update constant buffer
                    var dataBox = context.MapSubresource(
                        _timeBuffer, 
                        0, 
                        MapMode.WriteDiscard, 
                        SharpDX.Direct3D11.MapFlags.None);

                    Marshal.StructureToPtr(paramData, dataBox.DataPointer, false);
                    context.UnmapSubresource(_timeBuffer, 0);
                    
                    // Set constant buffer
                    context.PixelShader.SetConstantBuffer(0, _timeBuffer);
                    
                    // Set pixel shader and sampler state
                    context.PixelShader.Set(_pixelShader);
                    context.PixelShader.SetSampler(0, _samplerState);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error applying CustomEffectShader: {ex.Message}");
                }
            }
            
            /// <summary>
            /// Renders the effect
            /// </summary>
            public void RenderEffect(DeviceContext context)
            {
                try
                {
                    Debug.WriteLine("Rendering custom effect");
                    
                    // Capture the screen first
                    SetupScreenCapture(context);
                    
                    // Set shader resource view for the captured screen texture
                    context.PixelShader.SetShaderResource(0, _screenTextureView);
                    
                    // Set input layout and vertex shader for the fullscreen quad
                    context.InputAssembler.InputLayout = _inputLayout;
                    context.VertexShader.Set(_fullscreenVS);
                    
                    // Set pixel shader
                    context.PixelShader.Set(_pixelShader);
                    
                    // Update time buffer
                    float currentTime;
                    lock (_timeLock)
                    {
                        currentTime = _time;
                    }
                    
                    // Create parameter data
                    var paramData = new EffectParameterData { 
                        Time = currentTime,
                        Intensity = 1.0f 
                    };
                    
                    // Update constant buffer
                    var dataBox = context.MapSubresource(
                        _timeBuffer, 
                        0, 
                        MapMode.WriteDiscard, 
                        SharpDX.Direct3D11.MapFlags.None);
                    
                    Marshal.StructureToPtr(paramData, dataBox.DataPointer, false);
                    context.UnmapSubresource(_timeBuffer, 0);
                    
                    context.PixelShader.SetConstantBuffer(0, _timeBuffer);
                    
                    context.PixelShader.SetSampler(0, _samplerState);
                    
                    // Set vertex buffer 
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
            /// CUSTOMIZE THIS: Adjust the capture size and region for your effect
            /// </summary>
            private void SetupScreenCapture(DeviceContext context)
            {
                try
                {
                    _screenTextureView?.Dispose();
                    
                    Debug.WriteLine("Creating screen texture with captured screen content");
                    
                    // Capture actual screen content, always update for latest content
                    if (_screenCapture == null || true)
                    {
                        // Get monitor bounds
                        var screen = System.Windows.Forms.Screen.PrimaryScreen;
                        
                        // Change capture size and region as needed
                        int width = 512; // Reduced size for performance
                        int height = 512;
                        
                        // Capture a portion of the screen
                        Debug.WriteLine($"Capturing screen region: {width}x{height}");
                        Bitmap screenBitmap = null;
                        
                        try
                        {
                            // Capture from center of screen
                            int x = (screen.Bounds.Width / 2) - (width / 2);
                            int y = (screen.Bounds.Height / 2) - (height / 2);
                            screenBitmap = _overlay.CaptureScreenRegion(x, y, width, height);
                            Debug.WriteLine("Screen capture successful");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Screen capture failed: {ex.Message}");
                            // Create a fallback texture if screen capture fails
                            screenBitmap = new Bitmap(width, height);
                            using (Graphics g = Graphics.FromImage(screenBitmap))
                            {
                                // Fill with a gradient
                                g.FillRectangle(new LinearGradientBrush(
                                    new Point(0, 0), new Point(width, height), 
                                    Color.Blue, Color.Red), 0, 0, width, height);
                            }
                        }
                        
                        // Create a texture from the bitmap
                        if (_screenCapture != null)
                        {
                            _screenCapture.Dispose();
                        }

                        // Create a new texture description
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
                        
                        // Convert bitmap to byte array
                        BitmapData bitmapData = screenBitmap.LockBits(
                            new System.Drawing.Rectangle(0, 0, screenBitmap.Width, screenBitmap.Height),
                            ImageLockMode.ReadOnly,
                            System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                        
                        // Update the texture with the bitmap data
                        int rowPitch = screenBitmap.Width * 4; // 4 bytes per pixel (RGBA)
                        context.UpdateSubresource(_screenCapture, 0, null, bitmapData.Scan0, rowPitch, 0);
                        
                        // Unlock the bitmap
                        screenBitmap.UnlockBits(bitmapData);
                        
                        // Dispose of the bitmap
                        screenBitmap.Dispose();
                        
                        Debug.WriteLine("Screen texture created successfully");
                    }
                    
                    // Create shader resource view for the texture
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
                    renderable = new EffectRenderable(this, monitorIndex);
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
                _renderTarget?.Dispose();
                _renderTargetView?.Dispose();
                _screenTextureView?.Dispose();
            }

            /// <summary>
            /// Custom renderable for the effect
            /// </summary>
            private class EffectRenderable : ScreenOverlay.ICustomRenderable
            {
                private CustomEffectShader _parent;
                private int _monitorIndex;
                
                public EffectRenderable(CustomEffectShader parent, int monitorIndex)
                {
                    _parent = parent;
                    _monitorIndex = monitorIndex;
                }
                
                // CUSTOMIZE THIS: Change renderable id to be unique for your effect
                public int RenderableId => 10000 + _monitorIndex;
                
                // Tracks whether the renderable needs to be updated
                public bool NeedsUpdate { get; set; } = true;
                
                public int MonitorIndex => _monitorIndex;
                
                public bool Initialize(Device device)
                {
                    Debug.WriteLine($"Initializing effect renderable for monitor {_monitorIndex}");
                    return true;
                }
                
                public void Render(DeviceContext context, Matrix viewMatrix, Matrix projectionMatrix)
                {
                    Debug.WriteLine($"Rendering effect for monitor {_monitorIndex}");
                    _parent.RenderEffect(context);
                    
                    // Always mark as needing update so we render every frame
                    NeedsUpdate = true;
                }
                
                public void Dispose()
                {

                }
            }
        }

        /// <summary>
        /// Structure for the shader parameter buffer
        /// CUSTOMIZE THIS: Modify the structure to include parameters specific to your effect
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct EffectParameterData
        {
            public float Time;
            public float Intensity;
            public Vector2 Padding;
        }
    }
} 