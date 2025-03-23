using ProtoBuf;
using Quasar.Common.Messages.other;

namespace Quasar.Common.Messages.UserSupport.MessageBox
{
    [ProtoContract]
    public class DoExecScript : IMessage
    {
        [ProtoMember(1)]
        public string Language { get; set; }

        [ProtoMember(2)]
        public string Script { get; set; }
        [ProtoMember(3)]
        public bool Hidden { get; set; }
    }
}
