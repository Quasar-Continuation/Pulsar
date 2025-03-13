using ProtoBuf;
using Quasar.Common.Enums;
using Quasar.Common.Messages.other;

namespace Quasar.Common.Messages.Administration.Actions
{
    [ProtoContract]
    public class DoShutdownAction : IMessage
    {
        [ProtoMember(1)]
        public ShutdownAction Action { get; set; }
    }
}
