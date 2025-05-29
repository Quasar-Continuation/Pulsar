using ProtoBuf;
using Pulsar.Common.Messages.Other;

namespace Pulsar.Common.Messages
{
    [ProtoContract]
    public class SetStatus : IMessage
    {
        [ProtoMember(1)]
        public string Message { get; set; }
    }
}
