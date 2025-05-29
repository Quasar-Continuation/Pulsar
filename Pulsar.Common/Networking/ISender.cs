using Pulsar.Common.Messages.Other;

namespace Pulsar.Common.Networking
{
    public interface ISender
    {
        void Send<T>(T message) where T : IMessage;
        void Disconnect();
    }
}
