using Quasar.Common.Enums;
using Quasar.Common.Messages;
using Quasar.Common.Messages.Monitoring.Clipboard;
using Quasar.Common.Messages.other;
using Quasar.Common.Messages.UserSupport.MessageBox;
using Quasar.Common.Networking;
using Quasar.Server.Forms;
using Quasar.Server.Networking;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using static Quasar.Server.Messages.ClientDebugLog;

namespace Quasar.Server.Messages
{
    // I literally copied all of this from another handler and shoved it in here

    /// <summary>
    /// Handles messages for the interaction with the remote client status.
    /// </summary>
    public class ClientDebugLog : MessageProcessorBase<object>
    {
        public delegate void DebugLogEventHandler(object sender, Client client, string addressType);

        public event DebugLogEventHandler DebugLogReceived;

        public ClientDebugLog() : base(true)
        {
        }

        public override bool CanExecute(IMessage message) => message is GetDebugLog;

        public override bool CanExecuteFrom(ISender sender) => true;

        public override void Execute(ISender sender, IMessage message)
        {
            if (message is GetDebugLog addressMessage)
            {
                Execute((Client)sender, addressMessage);
            }
        }

        private void Execute(Client client, GetDebugLog message)
        {
            DebugLogReceived?.Invoke(this, client, message.Log);
            FrmMain frm = Application.OpenForms["FrmMain"] as FrmMain;
            frm.EventLog("[CLIENT ERROR: " + client.Value.UserAtPc + ": " + message.Log, "error");
        }
    }
}