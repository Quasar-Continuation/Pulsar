using ProtoBuf;

namespace Pulsar.Common.Messages.Other
{
    [ProtoContract]
    public class ClientIdentificationResult : IMessage
    {
        [ProtoMember(1)]
        public bool Result { get; set; }
    }
}
