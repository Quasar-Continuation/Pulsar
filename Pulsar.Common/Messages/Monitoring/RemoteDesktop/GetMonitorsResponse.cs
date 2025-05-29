using ProtoBuf;
using Pulsar.Common.Messages.Other;

namespace Pulsar.Common.Messages
{
    [ProtoContract]
    public class GetMonitorsResponse : IMessage
    {
        [ProtoMember(1)]
        public int Number { get; set; }
    }
}
