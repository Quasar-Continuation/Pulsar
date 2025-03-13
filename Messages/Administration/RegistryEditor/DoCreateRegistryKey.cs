using ProtoBuf;
using Quasar.Common.Messages.other;

namespace Quasar.Common.Messages.Administration.RegistryEditor
{
    [ProtoContract]
    public class DoCreateRegistryKey : IMessage
    {
        [ProtoMember(1)]
        public string ParentPath { get; set; }
    }
}
