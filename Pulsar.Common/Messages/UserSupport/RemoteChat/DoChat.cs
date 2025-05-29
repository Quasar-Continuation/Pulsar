using ProtoBuf;
using Pulsar.Common.Messages.Other;

namespace Pulsar.Common.Messages.UserSupport.RemoteChat
{
    [ProtoContract]
    public class DoChat : IMessage
    {
        [ProtoMember(1)]
        public string PacketDms { get; set; }
        [ProtoMember(2)]
        public string User { get; set; }
    }
}
