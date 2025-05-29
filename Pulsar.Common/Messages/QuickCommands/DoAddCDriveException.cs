using ProtoBuf;
using Pulsar.Common.Messages.Other;

namespace Pulsar.Common.Messages.QuickCommands
{
    [ProtoContract]
    public class DoSendQuickCommand : IMessage
    {
        [ProtoMember(1)]
        public string Command { get; set; }
        [ProtoMember(2)]
        public string Host { get; set; }
    }
}
