using Pulsar.Common.Enums;
using Pulsar.Common.Messages;
using Pulsar.Common.Messages.Monitoring.RemoteDesktop;
using Pulsar.Common.Messages.Other;
using Pulsar.Common.Networking;
using Pulsar.Common.Video.Codecs;
using Pulsar.Server.Networking;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Pulsar.Server.Messages
{
    /// <summary>
    /// Handles messages for the interaction with the remote desktop.
    /// </summary>
    public class RemoteDesktopHandler : MessageProcessorBase<Bitmap>, IDisposable
    {
        /// <summary>
        /// States if the client is currently streaming desktop frames.
        /// </summary>
        public bool IsStarted { get; set; }

        /// <summary>
        /// Gets or sets whether the remote desktop is using buffered mode.
        /// </summary>
        public bool IsBufferedMode { get; set; } = true;

        /// <summary>
        /// Used in lock statements to synchronize access to <see cref="_codec"/> between UI thread and thread pool.
        /// </summary>
        private readonly object _syncLock = new object();

        /// <summary>
        /// Used in lock statements to synchronize access to <see cref="LocalResolution"/> between UI thread and thread pool.
        /// </summary>
        private readonly object _sizeLock = new object();

        /// <summary>
        /// The local resolution, see <seealso cref="LocalResolution"/>.
        /// </summary>
        private Size _localResolution;

        /// <summary>
        /// The local resolution in width x height. It indicates to which resolution the received frame should be resized.
        /// </summary>
        /// <remarks>
        /// This property is thread-safe.
        /// </remarks>
        public Size LocalResolution
        {
            get
            {
                lock (_sizeLock)
                {
                    return _localResolution;
                }
            }
            set
            {
                lock (_sizeLock)
                {
                    _localResolution = value;
                }
            }
        }

        /// <summary>
        /// Represents the method that will handle display changes.
        /// </summary>
        /// <param name="sender">The message processor which raised the event.</param>
        /// <param name="value">All currently available displays.</param>
        public delegate void DisplaysChangedEventHandler(object sender, int value);

        /// <summary>
        /// Raised when a display changed.
        /// </summary>
        /// <remarks>
        /// Handlers registered with this event will be invoked on the 
        /// <see cref="System.Threading.SynchronizationContext"/> chosen when the instance was constructed.
        /// </remarks>
        public event DisplaysChangedEventHandler DisplaysChanged;

        /// <summary>
        /// Reports changed displays.
        /// </summary>
        /// <param name="value">All currently available displays.</param>
        private void OnDisplaysChanged(int value)
        {
            SynchronizationContext.Post(val =>
            {
                var handler = DisplaysChanged;
                handler?.Invoke(this, (int)val);
            }, value);
        }

        /// <summary>
        /// The client which is associated with this remote desktop handler.
        /// </summary>
        private readonly Client _client;

        /// <summary>
        /// The video stream codec used to decode received frames.
        /// </summary>
        private UnsafeStreamCodec _codec;        // buffer parameters
        private readonly int _initialFramesRequested = 5; // request 5 frames initially
        private readonly int _defaultFrameRequestBatch = 3; // request 3 frames at a time now on
        private int _pendingFrames = 0;
        private readonly SemaphoreSlim _frameRequestSemaphore = new SemaphoreSlim(1, 1);
        private readonly Stopwatch _frameReceiptStopwatch = new Stopwatch();
        private readonly ConcurrentQueue<long> _frameTimestamps = new ConcurrentQueue<long>();
        private readonly int _fpsCalculationWindow = 10; // calculate FPS based on last 10 frames

        private readonly Stopwatch _performanceMonitor = new Stopwatch();
        private int _framesReceived = 0;
        private double _estimatedFps = 0;

        /// <summary>
        /// Stores the last FPS reported by the client.
        /// </summary>
        private float _lastReportedFps = -1f;

        /// <summary>
        /// Shows the last FPS reported by the client, or estimated FPS if not available.
        /// </summary>
        public float CurrentFps => _lastReportedFps > 0 ? _lastReportedFps : (float)_estimatedFps;

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteDesktopHandler"/> class using the given client.
        /// </summary>
        /// <param name="client">The associated client.</param>
        public RemoteDesktopHandler(Client client) : base(true)
        {
            _client = client;
            _performanceMonitor.Start();
        }

        /// <inheritdoc />
        public override bool CanExecute(IMessage message) => message is GetDesktopResponse || message is GetMonitorsResponse;

        /// <inheritdoc />
        public override bool CanExecuteFrom(ISender sender) => _client.Equals(sender);

        /// <inheritdoc />
        public override void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case GetDesktopResponse d:
                    Execute(sender, d);
                    break;
                case GetMonitorsResponse m:
                    Execute(sender, m);
                    break;
            }
        }

        private void ClearTimeStamps()
        {
            while (_frameTimestamps.TryDequeue(out _)) { }
        }

        /// <summary>
        /// Begins receiving frames from the client using the specified quality and display.
        /// </summary>
        /// <param name="quality">The quality of the remote desktop frames.</param>
        /// <param name="display">The display to receive frames from.</param>
        /// <param name="useGPU">Whether to use GPU for screen capture.</param>
        public void BeginReceiveFrames(int quality, int display, bool useGPU)
        {
            lock (_syncLock)
            {
                IsStarted = true;
                _codec?.Dispose();
                _codec = null;

                // Reset buffering counters
                _pendingFrames = _initialFramesRequested;
                ClearTimeStamps();
                _framesReceived = 0;
                _frameReceiptStopwatch.Restart();

                // Start in buffered mode
                _client.Send(new GetDesktop
                {
                    CreateNew = true,
                    Quality = quality,
                    DisplayIndex = display,
                    Status = RemoteDesktopStatus.Start,
                    UseGPU = useGPU,
                    IsBufferedMode = IsBufferedMode,
                    FramesRequested = _initialFramesRequested
                });
            }
        }

        /// <summary>
        /// Ends receiving frames from the client.
        /// </summary>
        public void EndReceiveFrames()
        {
            lock (_syncLock)
            {
                IsStarted = false;
            }

            Debug.WriteLine("Remote desktop session stopped");

            _client.Send(new GetDesktop { Status = RemoteDesktopStatus.Stop });
        }

        /// <summary>
        /// Refreshes the available displays of the client.
        /// </summary>
        public void RefreshDisplays()
        {
            Debug.WriteLine("Refreshing displays");
            _client.Send(new GetMonitors());
        }

        /// <summary>
        /// Sends a mouse event to the specified display of the client.
        /// </summary>
        /// <param name="mouseAction">The mouse action to send.</param>
        /// <param name="isMouseDown">Indicates whether it's a mousedown or mouseup event.</param>
        /// <param name="x">The X-coordinate inside the <see cref="LocalResolution"/>.</param>
        /// <param name="y">The Y-coordinate inside the <see cref="LocalResolution"/>.</param>
        /// <param name="displayIndex">The display to execute the mouse event on.</param>
        public void SendMouseEvent(MouseAction mouseAction, bool isMouseDown, int x, int y, int displayIndex)
        {
            lock (_syncLock)
            {
                if (_codec == null) return;

                _client.Send(new DoMouseEvent
                {
                    Action = mouseAction,
                    IsMouseDown = isMouseDown,
                    // calculate remote width & height
                    X = x * _codec.Resolution.Width / LocalResolution.Width,
                    Y = y * _codec.Resolution.Height / LocalResolution.Height,
                    MonitorIndex = displayIndex
                });
            }
        }

        /// <summary>
        /// Sends a keyboard event to the client.
        /// </summary>
        /// <param name="keyCode">The pressed key.</param>
        /// <param name="keyDown">Indicates whether it's a keydown or keyup event.</param>
        public void SendKeyboardEvent(byte keyCode, bool keyDown)
        {
            _client.Send(new DoKeyboardEvent { Key = keyCode, KeyDown = keyDown });
        }

        /// <summary>
        /// Sends a drawing event to the client.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="prevX">The previous X coordinate.</param>
        /// <param name="prevY">The previous Y coordinate.</param>
        /// <param name="strokeWidth">The width of the stroke.</param>
        /// <param name="colorArgb">The color in ARGB format.</param>
        /// <param name="isEraser">True if using eraser, false for drawing.</param>
        /// <param name="isClearAll">True to clear all drawings.</param>
        /// <param name="displayIndex">The display index to draw on.</param>
        public void SendDrawingEvent(int x, int y, int prevX, int prevY, int strokeWidth, int colorArgb, bool isEraser, bool isClearAll, int displayIndex)
        {
            lock (_syncLock)
            {
                if (_codec == null || !IsStarted) return;

                int remoteX = x * _codec.Resolution.Width / LocalResolution.Width;
                int remoteY = y * _codec.Resolution.Height / LocalResolution.Height;
                int remotePrevX = prevX * _codec.Resolution.Width / LocalResolution.Width;
                int remotePrevY = prevY * _codec.Resolution.Height / LocalResolution.Height;

                _client.Send(new DoDrawingEvent
                {
                    X = remoteX,
                    Y = remoteY,
                    PrevX = remotePrevX,
                    PrevY = remotePrevY,
                    StrokeWidth = strokeWidth,
                    ColorArgb = colorArgb,
                    IsEraser = isEraser,
                    IsClearAll = isClearAll,
                    MonitorIndex = displayIndex
                });
            }
        }        private async void Execute(ISender client, GetDesktopResponse message)
        {
            _framesReceived++;

            // Capture the FPS reported by the client
            if (message.FrameRate > 0 && message.FrameRate != _lastReportedFps)
            {
                _lastReportedFps = message.FrameRate;
                Debug.WriteLine($"Client-reported FPS updated: {_lastReportedFps}");
            }

            if (_performanceMonitor.ElapsedMilliseconds >= 1000)
            {
                _estimatedFps = _framesReceived / (_performanceMonitor.ElapsedMilliseconds / 1000.0);
                Debug.WriteLine($"Estimated FPS: {_estimatedFps:F1}, Client-reported FPS: {(_lastReportedFps > 0 ? _lastReportedFps.ToString("F1") : "N/A")}, Frames received: {_framesReceived}");
                _framesReceived = 0;
                _performanceMonitor.Restart();
            }

            lock (_syncLock)
            {
                if (!IsStarted)
                    return;

                if (_codec == null || _codec.ImageQuality != message.Quality || _codec.Monitor != message.Monitor || _codec.Resolution != message.Resolution)
                {
                    _codec?.Dispose();
                    _codec = new UnsafeStreamCodec(message.Quality, message.Monitor, message.Resolution);
                }

                using (MemoryStream ms = new MemoryStream(message.Image))
                {
                    try
                    {
                        // create deep copy & resize bitmap to local resolution
                        OnReport(new Bitmap(_codec.DecodeData(ms), LocalResolution));
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error decoding frame: {ex.Message}");
                    }                }                message.Image = null;

                long serverTimestamp = Stopwatch.GetTimestamp();
                _frameTimestamps.Enqueue(serverTimestamp);
                
                _framesReceived++;
                if (_framesReceived % _fpsCalculationWindow == 0)
                {
                    double elapsedSeconds = _performanceMonitor.Elapsed.TotalSeconds;
                    _estimatedFps = _framesReceived / elapsedSeconds;
                    
                    if (_framesReceived >= 100)
                    {
                        _framesReceived = 0;
                        _performanceMonitor.Restart();
                    }
                }
                
                while (_frameTimestamps.Count > _fpsCalculationWindow && _frameTimestamps.TryDequeue(out _)) { }

                Interlocked.Decrement(ref _pendingFrames);
            }

            if (IsBufferedMode && (message.IsLastRequestedFrame || _pendingFrames <= 1))
            {
                await RequestMoreFramesAsync();
            }
        }

        private async Task RequestMoreFramesAsync()
        {
            if (!await _frameRequestSemaphore.WaitAsync(0))
                return;

            try
            {
                int batchSize = _defaultFrameRequestBatch;

                if (_estimatedFps > 25)
                    batchSize = 5;
                else if (_estimatedFps < 10)
                    batchSize = 2;

                Debug.WriteLine($"Requesting {batchSize} more frames");
                Interlocked.Add(ref _pendingFrames, batchSize);

                _client.Send(new GetDesktop
                {
                    CreateNew = false,
                    Quality = _codec?.ImageQuality ?? 75,
                    DisplayIndex = _codec?.Monitor ?? 0,
                    Status = RemoteDesktopStatus.Continue,
                    IsBufferedMode = true,
                    FramesRequested = batchSize
                });
            }
            finally
            {
                _frameRequestSemaphore.Release();
            }
        }

        private void Execute(ISender client, GetMonitorsResponse message)
        {
            OnDisplaysChanged(message.Number);
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
                lock (_syncLock)
                {
                    _codec?.Dispose();
                    _frameRequestSemaphore?.Dispose();
                    IsStarted = false;
                }
            }
        }
    }
}