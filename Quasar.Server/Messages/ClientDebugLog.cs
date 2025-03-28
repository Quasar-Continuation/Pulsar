using Quasar.Common.Messages;
using Quasar.Common.Messages.other;
using Quasar.Common.Networking;
using Quasar.Server.Forms;
using Quasar.Server.Networking;
using System.Windows.Forms;

namespace Quasar.Server.Messages
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
            }
        }
    }
}