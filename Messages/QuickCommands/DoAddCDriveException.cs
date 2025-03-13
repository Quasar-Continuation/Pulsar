using ProtoBuf;
using Quasar.Common.Messages.other;

namespace Quasar.Common.Messages.QuickCommands
{
    [ProtoContract]
    public class DoSendQuickCommand : IMessage
    {
        [ProtoMember(1)]
        public string Command { get; set; }
    }
}
