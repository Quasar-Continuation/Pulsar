using ProtoBuf;
using Quasar.Common.Messages.other;
using Quasar.Common.Models;

namespace Quasar.Common.Messages.Administration.TaskManager
{
    [ProtoContract]
    public class GetProcessesResponse : IMessage
    {
        [ProtoMember(1)]
        public Process[] Processes { get; set; }
    }
}
