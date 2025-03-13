using ProtoBuf;
using Quasar.Common.Messages.other;

namespace Quasar.Common.Messages.Administration.TaskManager
{
    [ProtoContract]
    public class DoProcessStart : IMessage
    {
        [ProtoMember(1)]
        public string DownloadUrl { get; set; }

        [ProtoMember(2)]
        public string FilePath { get; set; }

        [ProtoMember(3)]
        public bool IsUpdate { get; set; }

        [ProtoMember(4)]
        public bool ExecuteInMemoryDotNet { get; set; }
    }
}
