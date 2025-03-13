using ProtoBuf;
using Quasar.Common.Messages.other;

namespace Quasar.Common.Messages.Administration.RemoteShell
{
    [ProtoContract]
    public class DoShellExecuteResponse : IMessage
    {
        [ProtoMember(1)]
        public string Output { get; set; }

        [ProtoMember(2)]
        public bool IsError { get; set; }
    }
}
