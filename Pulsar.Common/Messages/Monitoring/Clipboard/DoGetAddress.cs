using ProtoBuf;
using Pulsar.Common.Messages.Other;

namespace Pulsar.Common.Messages.Monitoring.Clipboard
{
    [ProtoContract]
    public class DoGetAddress : IMessage
    {
        [ProtoMember(1)]
        public string Type { get; set; }
    }
}
