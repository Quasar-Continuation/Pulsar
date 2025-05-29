using ProtoBuf;
using Pulsar.Common.Messages.Other;

namespace Pulsar.Common.Messages.UserSupport.RemoteChat
{
    [ProtoContract]
    public class GetChat : IMessage
    {
        [ProtoMember(1)]
        public string Message { get; set; }
    }
}
