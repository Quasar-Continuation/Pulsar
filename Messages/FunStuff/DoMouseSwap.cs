using ProtoBuf;
using Quasar.Common.Messages.other;

namespace Quasar.Common.Messages.FunStuff
{
    [ProtoContract]
    public class DoSwapMouseButtons : IMessage
    {
        [ProtoMember(1)]
        public string Message { get; set; }
    }
}
