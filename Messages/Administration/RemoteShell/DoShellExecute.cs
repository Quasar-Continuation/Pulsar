using ProtoBuf;
using Quasar.Common.Messages.other;

namespace Quasar.Common.Messages.Administration.RemoteShell
{
    [ProtoContract]
    public class DoShellExecute : IMessage
    {
        [ProtoMember(1)]
        public string Command { get; set; }
    }
}
