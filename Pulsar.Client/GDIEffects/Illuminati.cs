using Pulsar.Client.Utilities;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.D3DCompiler;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Matrix = SharpDX.Matrix;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using FillMode = SharpDX.Direct3D11.FillMode;
using Color4 = SharpDX.Color4;
using Rectangle = System.Drawing.Rectangle;
namespace Pulsar.Client.GDIEffects
{
    /// <summary>
    /// illuminati 3D model with a rotating effect
    /// </summary>
    public class Illuminati : IDisposable
    {

        private readonly ScreenOverlay _overlay;


        private readonly illuminatiShader _illuminatiShader;


        private bool _effectActive = false;


        private CancellationTokenSource _cancellationTokenSource;
        private Task _animationTask;


        private readonly object _lockObject = new object();
        /// <summary>
        /// Creates a new illuminati effect instance
        /// </summary>
        /// <param name="overlay">The ScreenOverlay to use for rendering</param>
        public Illuminati(ScreenOverlay overlay)
        {
            _overlay = overlay ?? throw new ArgumentNullException(nameof(overlay));
            _illuminatiShader = new illuminatiShader(_overlay);
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

            try
            {

                _overlay.ClearDrawings(0);


                for (int i = 0; i < System.Windows.Forms.Screen.AllScreens.Length; i++)
                {
                    Debug.WriteLine($"Setting up effect for monitor {i}");


                    bool shaderRegistered = _overlay.RegisterCustomShader(_illuminatiShader, i);
                    Debug.WriteLine($"Shader registered: {shaderRegistered}");


                    var renderable = _illuminatiShader.GetRenderableForMonitor(i);
                    bool renderableRegistered = _overlay.RegisterCustomRenderable(renderable);
                    Debug.WriteLine($"Renderable registered: {renderableRegistered}");


                    _overlay.UpdateCustomRenderable(renderable.RenderableId, i);


                    var bounds = System.Windows.Forms.Screen.AllScreens[i].Bounds;
                    int white = System.Drawing.Color.FromArgb(255, 255, 255, 255).ToArgb();


                    _overlay.Draw(bounds.X + 10, bounds.Y + 10, bounds.X + 10, bounds.Y + 10, 5, white, i); 
                    _overlay.Draw(bounds.Right - 10, bounds.Y + 10, bounds.Right - 10, bounds.Y + 10, 5, white, i); 
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
                            _illuminatiShader.UpdateTime(time);


                            for (int i = 0; i < System.Windows.Forms.Screen.AllScreens.Length; i++)
                            {
                                var renderable = _illuminatiShader.GetRenderableForMonitor(i);
                                renderable.NeedsUpdate = true;
                                _overlay.UpdateCustomRenderable(renderable.RenderableId, i);
                            }



                            await Task.Delay(16, token); 
                        }
                        catch (OperationCanceledException)
                        {
                            break;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error in illuminati animation: {ex.Message}");
                        }
                    }

                    timer.Stop();
                }, token);
                _effectActive = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error starting illuminati effect: {ex.Message}\n{ex.StackTrace}");
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

            for (int i = 0; i < System.Windows.Forms.Screen.AllScreens.Length; i++)
            {
                try
                {

                    var renderable = _illuminatiShader.GetRenderableForMonitor(i);
                    _overlay.UnregisterCustomRenderable(renderable.RenderableId, i);


                    _overlay.UnregisterCustomShader(_illuminatiShader.ShaderId, i);
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
            _illuminatiShader?.Dispose();
        }

        private class illuminatiShader : ScreenOverlay.ICustomShader
        {

            private const string VertexShaderCode = @"
cbuffer MatrixBuffer : register(b0)
{
    matrix worldMatrix;
    matrix viewMatrix;
    matrix projectionMatrix;
};
cbuffer TimeBuffer : register(b1)
{
    float time;
    float3 padding;
};
struct VS_INPUT
{
    float3 position : POSITION;
    float2 texcoord : TEXCOORD0;
    float3 normal : NORMAL;
};
struct PS_INPUT
{
    float4 position : SV_POSITION;
    float2 texcoord : TEXCOORD0;
    float3 normal : NORMAL;
    float3 worldPos : TEXCOORD1;
    float time : TEXCOORD2;
};
PS_INPUT main(VS_INPUT input)
{
    PS_INPUT output;

    float rotationX = sin(time * 0.3) * 0.2;  // Gentle tilt
    float rotationY = time * 0.3;             // Slower rotation

    float3x3 rotX;
    rotX[0] = float3(1, 0, 0);
    rotX[1] = float3(0, cos(rotationX), -sin(rotationX));
    rotX[2] = float3(0, sin(rotationX), cos(rotationX));

    float3x3 rotY;
    rotY[0] = float3(cos(rotationY), 0, sin(rotationY));
    rotY[1] = float3(0, 1, 0);
    rotY[2] = float3(-sin(rotationY), 0, cos(rotationY));

    float3 rotatedPos = mul(input.position, rotX);
    rotatedPos = mul(rotatedPos, rotY);

    float3 rotatedNormal = mul(input.normal, rotX);
    rotatedNormal = mul(rotatedNormal, rotY);

    float4 worldPosition = mul(float4(rotatedPos, 1.0f), worldMatrix);
    output.worldPos = worldPosition.xyz;
    output.position = mul(worldPosition, viewMatrix);
    output.position = mul(output.position, projectionMatrix);

    output.texcoord = input.texcoord;

    output.normal = normalize(mul(rotatedNormal, (float3x3)worldMatrix));

    output.time = time;

    return output;
}";

            private const string PixelShaderCode = @"
Texture2D modelTexture : register(t0);
SamplerState textureSampler : register(s0);
struct PS_INPUT
{
    float4 position : SV_POSITION;
    float2 texcoord : TEXCOORD0;
    float3 normal : NORMAL;
    float3 worldPos : TEXCOORD1;
    float time : TEXCOORD2;
};
float4 main(PS_INPUT input) : SV_TARGET
{
    float4 textureColor = modelTexture.Sample(textureSampler, input.texcoord);

    float3 lightDir = normalize(float3(0.5, -0.5, 1.0));
    float lightIntensity = max(0.3, dot(input.normal, lightDir));
    float ambient = 0.3;
    float lighting = lightIntensity + ambient;

    float pulse = (sin(input.time * 2.0) * 0.5 + 0.5) * 0.3;
    float3 glow = float3(1.0, 0.9, 0.5) * pulse;

    float4 finalColor = textureColor * lighting;
    finalColor.rgb += glow;

    finalColor.a = 1.0;

    return finalColor;
}";

            [StructLayout(LayoutKind.Sequential)]
            private struct ModelVertex
            {
                public Vector3 Position;
                public Vector2 TexCoord;
                public Vector3 Normal;

                public ModelVertex(Vector3 position, Vector2 texCoord, Vector3 normal)
                {
                    Position = position;
                    TexCoord = texCoord;
                    Normal = normal;
                }
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct TimeData
            {
                public float Time;
                public Vector3 Padding;
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct MatrixBuffer
            {
                public Matrix World;
                public Matrix View;
                public Matrix Projection;
            }

            private VertexShader _vertexShader;
            private PixelShader _pixelShader;
            private InputLayout _inputLayout;
            private Buffer _vertexBuffer;
            private Buffer _indexBuffer;
            private Buffer _matrixBuffer;
            private Buffer _timeBuffer;
            private ShaderResourceView _textureView;
            private SamplerState _samplerState;
            private BlendState _blendState;
            private RasterizerState _rasterizerState;
            private DepthStencilState _depthStencilState;
            private float _time;
            private readonly object _timeLock = new object();
            private int _indexCount;


            private readonly ScreenOverlay _overlay;


            private Dictionary<int, illuminatiRenderable> _renderables = new Dictionary<int, illuminatiRenderable>();
            /// <summary>
            /// Creates a new illuminati shader
            /// </summary>
            public illuminatiShader(ScreenOverlay overlay)
            {
                _overlay = overlay;
            }
            /// <summary>
            /// Gets the unique identifier for this shader
            /// </summary>
            public string ShaderId => "illuminatiShader";
            /// <summary>
            /// Initializes the shader resources
            /// </summary>
            public bool Initialize(Device device)
            {
                try
                {

                    using (var vertexShaderBytecode = ShaderBytecode.Compile(
                        VertexShaderCode,
                        "main",
                        "vs_5_0",
                        ShaderFlags.Debug))
                    {
                        _vertexShader = new VertexShader(device, vertexShaderBytecode);

                        var inputElements = new[]
                        {
                            new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                            new InputElement("TEXCOORD", 0, Format.R32G32_Float, 12, 0),
                            new InputElement("NORMAL", 0, Format.R32G32B32_Float, 20, 0)
                        };
                        _inputLayout = new InputLayout(device, vertexShaderBytecode, inputElements);
                    }

                    using (var pixelShaderBytecode = ShaderBytecode.Compile(
                        PixelShaderCode,
                        "main",
                        "ps_5_0",
                        ShaderFlags.Debug))
                    {
                        _pixelShader = new PixelShader(device, pixelShaderBytecode);
                    }

                    LoadilluminatiModel(device);

                    _matrixBuffer = new Buffer(device, Marshal.SizeOf<MatrixBuffer>(), ResourceUsage.Dynamic,
                        BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);

                    _timeBuffer = new Buffer(device, Marshal.SizeOf<TimeData>(), ResourceUsage.Dynamic,
                        BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);

                    _samplerState = new SamplerState(device, new SamplerStateDescription
                    {
                        Filter = Filter.MinMagMipLinear,
                        AddressU = TextureAddressMode.Wrap,
                        AddressV = TextureAddressMode.Wrap,
                        AddressW = TextureAddressMode.Wrap,
                        ComparisonFunction = Comparison.Never,
                        MinimumLod = 0,
                        MaximumLod = float.MaxValue,
                        MipLodBias = 0,
                        MaximumAnisotropy = 16
                    });


                    BlendStateDescription blendDesc = new BlendStateDescription();
                    blendDesc.AlphaToCoverageEnable = false;
                    blendDesc.IndependentBlendEnable = false;

                    blendDesc.RenderTarget[0].IsBlendEnabled = true;
                    blendDesc.RenderTarget[0].SourceBlend = BlendOption.SourceAlpha;
                    blendDesc.RenderTarget[0].DestinationBlend = BlendOption.InverseSourceAlpha;
                    blendDesc.RenderTarget[0].BlendOperation = BlendOperation.Add;
                    blendDesc.RenderTarget[0].SourceAlphaBlend = BlendOption.One;
                    blendDesc.RenderTarget[0].DestinationAlphaBlend = BlendOption.Zero;
                    blendDesc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
                    blendDesc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;
                    _blendState = new BlendState(device, blendDesc);

                    var depthStencilDesc = new DepthStencilStateDescription
                    {
                        IsDepthEnabled = false, 
                        DepthWriteMask = DepthWriteMask.Zero,
                        IsStencilEnabled = false
                    };
                    _depthStencilState = new DepthStencilState(device, depthStencilDesc);

                    var rasterizerDesc = new RasterizerStateDescription
                    {
                        CullMode = CullMode.Back,
                        FillMode = FillMode.Solid,
                        IsDepthClipEnabled = false,
                        IsFrontCounterClockwise = false,
                    };
                    _rasterizerState = new RasterizerState(device, rasterizerDesc);
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error initializing illuminati shader: {ex.Message}");
                    return false;
                }
            }
            /// <summary>
            /// Loads the illuminati 3D model from OBJ file
            /// </summary>
            private void LoadilluminatiModel(Device device)
            {
                try
                {

                    string objFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Pulsar.Client", "GDIEffects", "illuminati", "illuminati.obj");
                    string texturePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Pulsar.Client", "GDIEffects", "illuminati", "texture.jpg");

                    Debug.WriteLine($"Loading OBJ model from: {objFilePath}");


                    var modelData = ObjModelLoader.LoadObj(objFilePath);



                    List<ModelVertex> vertices = new List<ModelVertex>();


                    if (modelData.Indices != null && modelData.Indices.Length > 0)
                    {

                        for (int i = 0; i < modelData.Indices.Length; i++)
                        {
                            int index = modelData.Indices[i];


                            Vector3 position = (index < modelData.Positions.Length) 
                                ? modelData.Positions[index] 
                                : Vector3.Zero;

                            Vector2 texCoord = (index < modelData.TexCoords.Length) 
                                ? modelData.TexCoords[index] 
                                : Vector2.Zero;

                            Vector3 normal = (index < modelData.Normals.Length) 
                                ? modelData.Normals[index] 
                                : Vector3.UnitY;

                            vertices.Add(new ModelVertex(position, texCoord, normal));
                        }


                        int[] sequentialIndices = new int[vertices.Count];
                        for (int i = 0; i < vertices.Count; i++)
                        {
                            sequentialIndices[i] = i;
                        }


                        _vertexBuffer = Buffer.Create(device, BindFlags.VertexBuffer, vertices.ToArray());
                        _indexBuffer = Buffer.Create(device, BindFlags.IndexBuffer, sequentialIndices);
                        _indexCount = sequentialIndices.Length;
                    }
                    else
                    {
                        Debug.WriteLine("No indices found in obj file");


                        int maxLength = Math.Min(
                            Math.Min(modelData.Positions.Length, modelData.TexCoords.Length),
                            modelData.Normals.Length);

                        for (int i = 0; i < maxLength; i++)
                        {
                            vertices.Add(new ModelVertex(
                                modelData.Positions[i],
                                modelData.TexCoords[i],
                                modelData.Normals[i]
                            ));
                        }


                        int[] indices = new int[vertices.Count];
                        for (int i = 0; i < vertices.Count; i++)
                            indices[i] = i;

                        _vertexBuffer = Buffer.Create(device, BindFlags.VertexBuffer, vertices.ToArray());
                        _indexBuffer = Buffer.Create(device, BindFlags.IndexBuffer, indices);
                        _indexCount = indices.Length;
                    }


                    if (File.Exists(texturePath))
                    {
                        using (var bitmap = new Bitmap(texturePath))
                        {

                            var textureDesc = new Texture2DDescription
                            {
                                Width = bitmap.Width,
                                Height = bitmap.Height,
                                MipLevels = 1,
                                ArraySize = 1,
                                Format = Format.R8G8B8A8_UNorm,
                                SampleDescription = new SampleDescription(1, 0),
                                Usage = ResourceUsage.Default,
                                BindFlags = BindFlags.ShaderResource,
                                CpuAccessFlags = CpuAccessFlags.None,
                                OptionFlags = ResourceOptionFlags.None
                            };


                            BitmapData bitmapData = bitmap.LockBits(
                                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                                ImageLockMode.ReadOnly,
                                System.Drawing.Imaging.PixelFormat.Format32bppArgb);


                            using (var texture = new Texture2D(device, textureDesc))
                            {

                                device.ImmediateContext.UpdateSubresource(
                                    texture,
                                    0,
                                    null,
                                    bitmapData.Scan0,
                                    bitmapData.Stride,
                                    0);


                                _textureView = new ShaderResourceView(device, texture);
                            }


                            bitmap.UnlockBits(bitmapData);
                        }
                    }

                    Debug.WriteLine($"Loaded model with {vertices.Count} vertices and {_indexCount} indices");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error loading illuminati model: {ex.Message}");


                    CreateFallbackModel(device);
                }
            }

            /// <summary>
            /// Creates a simple quad as a fallback if model loading fails (basically no chance of happening but why not)
            /// </summary>
            private void CreateFallbackModel(Device device)
            {
                Debug.WriteLine("Creating fallback model");


                ModelVertex[] vertices = new ModelVertex[]
                {
                    new ModelVertex(new Vector3(-1.0f, -1.0f, 0.0f), new Vector2(0.0f, 1.0f), new Vector3(0.0f, 0.0f, 1.0f)),
                    new ModelVertex(new Vector3(-1.0f, 1.0f, 0.0f), new Vector2(0.0f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f)),
                    new ModelVertex(new Vector3(1.0f, -1.0f, 0.0f), new Vector2(1.0f, 1.0f), new Vector3(0.0f, 0.0f, 1.0f)),
                    new ModelVertex(new Vector3(1.0f, 1.0f, 0.0f), new Vector2(1.0f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f)),
                };


                _vertexBuffer = Buffer.Create(device, BindFlags.VertexBuffer, vertices);


                int[] indices = new int[] { 0, 1, 2, 2, 1, 3 };


                _indexBuffer = Buffer.Create(device, BindFlags.IndexBuffer, indices);
                _indexCount = indices.Length;


                var textureDesc = new Texture2DDescription
                {
                    Width = 1,
                    Height = 1,
                    MipLevels = 1,
                    ArraySize = 1,
                    Format = Format.R8G8B8A8_UNorm,
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = ResourceUsage.Default,
                    BindFlags = BindFlags.ShaderResource,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.None
                };


                byte[] pixelData = new byte[] { 255, 255, 255, 255 };


                using (var texture = new Texture2D(device, textureDesc))
                {

                    DataStream stream = new DataStream(pixelData.Length, true, true);
                    stream.Write(pixelData, 0, pixelData.Length);
                    stream.Position = 0;

                    DataRectangle dataRectangle = new DataRectangle(stream.DataPointer, 4); 
                    device.ImmediateContext.UpdateSubresource(
                        ref dataRectangle,
                        texture,
                        0);


                    _textureView = new ShaderResourceView(device, texture);

                    stream.Dispose();
                }
            }
            /// <summary>
            /// Applies the shader to the rendering pipeline
            /// </summary>
            public void Apply(DeviceContext context)
            {
                try
                {

                    Color4 blendFactor = new Color4(0.0f, 0.0f, 0.0f, 0.0f);
                    context.OutputMerger.SetBlendState(_blendState, blendFactor);


                    context.OutputMerger.SetDepthStencilState(_depthStencilState, 0);


                    context.Rasterizer.State = _rasterizerState;


                    context.InputAssembler.InputLayout = _inputLayout;
                    context.VertexShader.Set(_vertexShader);
                    context.PixelShader.Set(_pixelShader);


                    context.PixelShader.SetShaderResource(0, _textureView);
                    context.PixelShader.SetSampler(0, _samplerState);

                    int stride = Marshal.SizeOf<ModelVertex>();
                    int offset = 0;
                    context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(_vertexBuffer, stride, offset));
                    context.InputAssembler.SetIndexBuffer(_indexBuffer, Format.R32_UInt, 0);
                    context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;


                    float currentTime;
                    lock (_timeLock)
                    {
                        currentTime = _time;
                    }

                    var timeData = new TimeData
                    {
                        Time = currentTime,
                        Padding = Vector3.Zero
                    };

                    var timeDataBox = context.MapSubresource(
                        _timeBuffer, 
                        0, 
                        MapMode.WriteDiscard, 
                        MapFlags.None);

                    Marshal.StructureToPtr(timeData, timeDataBox.DataPointer, false);
                    context.UnmapSubresource(_timeBuffer, 0);


                    context.VertexShader.SetConstantBuffer(0, _matrixBuffer);
                    context.VertexShader.SetConstantBuffer(1, _timeBuffer);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error applying illuminati shader: {ex.Message}");
                }
            }
            /// <summary>
            /// Updates the model-view-projection matrices for 3D rendering
            /// </summary>
            private void UpdateMatrices(DeviceContext context, Matrix viewMatrix, Matrix projectionMatrix)
            {
                try
                {


                    Matrix worldMatrix = Matrix.Scaling(30.0f) * 
                                        Matrix.Translation(new Vector3(0, -50, 300));


                    var matrixData = new MatrixBuffer
                    {
                        World = Matrix.Transpose(worldMatrix),
                        View = Matrix.Transpose(viewMatrix),
                        Projection = Matrix.Transpose(projectionMatrix)
                    };

                    var matrixDataBox = context.MapSubresource(
                        _matrixBuffer,
                        0, 
                        MapMode.WriteDiscard, 
                        MapFlags.None);
                    Marshal.StructureToPtr(matrixData, matrixDataBox.DataPointer, false);
                    context.UnmapSubresource(_matrixBuffer, 0);

                    Debug.WriteLine("Updated matrix buffer with new world transform");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error updating matrices: {ex.Message}");
                }
            }
            /// <summary>
            /// Renders the illuminati model
            /// </summary>
            public void Renderilluminati(DeviceContext context, Matrix viewMatrix, Matrix projectionMatrix)
            {
                try
                {
                    Debug.WriteLine("Rendering illuminati model");


                    UpdateMatrices(context, viewMatrix, projectionMatrix);


                    Apply(context);


                    context.DrawIndexed(_indexCount, 0, 0);

                    Debug.WriteLine("Done rendering illuminati model");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error rendering illuminati: {ex.Message}");
                }
            }
            /// <summary>
            /// Updates the time parameter for animation
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
                    renderable = new illuminatiRenderable(this, monitorIndex);
                    _renderables[monitorIndex] = renderable;
                }

                return renderable;
            }
            /// <summary>
            /// Disposes of resources
            /// </summary>
            public void Dispose()
            {
                _vertexShader?.Dispose();
                _pixelShader?.Dispose();
                _inputLayout?.Dispose();
                _vertexBuffer?.Dispose();
                _indexBuffer?.Dispose();
                _matrixBuffer?.Dispose();
                _timeBuffer?.Dispose();
                _textureView?.Dispose();
                _samplerState?.Dispose();
                _blendState?.Dispose();
                _rasterizerState?.Dispose();
                _depthStencilState?.Dispose();
            }
            /// <summary>
            /// Custom renderable for the illuminati model
            /// </summary>
            private class illuminatiRenderable : ScreenOverlay.ICustomRenderable
            {
                private readonly illuminatiShader _parent;
                private readonly int _monitorIndex;

                public illuminatiRenderable(illuminatiShader parent, int monitorIndex)
                {
                    _parent = parent;
                    _monitorIndex = monitorIndex;
                }

                public int RenderableId => 12345 + _monitorIndex;

                public bool NeedsUpdate { get; set; } = true;

                public int MonitorIndex => _monitorIndex;

                public bool Initialize(Device device)
                {
                    Debug.WriteLine($"Initializing illuminati renderable for monitor {_monitorIndex}");
                    return true; 
                }

                public void Render(DeviceContext context, Matrix viewMatrix, Matrix projectionMatrix)
                {
                    Debug.WriteLine($"Rendering illuminati for monitor {_monitorIndex}");


                    _parent.Renderilluminati(context, viewMatrix, projectionMatrix);


                    NeedsUpdate = true;
                }

                public void Dispose()
                {

                }
            }
        }
    }
} 
