using ProtoBuf;
using Quasar.Common.Messages.other;

namespace Quasar.Common.Messages.Administration.RegistryEditor
{
    [ProtoContract]
    public class DoRenameRegistryValue : IMessage
    {
        [ProtoMember(1)]
        public string KeyPath { get; set; }

        [ProtoMember(2)]
        public string OldValueName { get; set; }

        [ProtoMember(3)]
        public string NewValueName { get; set; }
    }
}
