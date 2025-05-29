using ProtoBuf;
using Pulsar.Common.Messages.Other;

namespace Pulsar.Common.Messages
{
    [ProtoContract]
    public class SetUserActiveWindowStatus : IMessage
    {
        [ProtoMember(1)]
        public string WindowTitle { get; set; }
    }
}