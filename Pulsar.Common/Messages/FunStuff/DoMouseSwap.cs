using ProtoBuf;
using Pulsar.Common.Messages.Other;

namespace Pulsar.Common.Messages.FunStuff
{
    [ProtoContract]
    public class DoSwapMouseButtons : IMessage
    {
        [ProtoMember(1)]
        public string Message { get; set; }
    }
}
