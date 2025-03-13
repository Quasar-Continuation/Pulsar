using ProtoBuf;
using Quasar.Common.Messages.other;

namespace Quasar.Common.Messages.Administration.TaskManager
{
    [ProtoContract]
    public class DoProcessEnd : IMessage
    {
        [ProtoMember(1)]
        public int Pid { get; set; }
    }
}
