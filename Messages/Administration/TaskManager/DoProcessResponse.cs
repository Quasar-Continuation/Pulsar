using ProtoBuf;
using Quasar.Common.Enums;
using Quasar.Common.Messages.other;

namespace Quasar.Common.Messages.Administration.TaskManager
{
    [ProtoContract]
    public class DoProcessResponse : IMessage
    {
        [ProtoMember(1)]
        public ProcessAction Action { get; set; }

        [ProtoMember(2)]
        public bool Result { get; set; }
    }
}
