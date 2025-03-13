using ProtoBuf;
using Quasar.Common.Messages.other;

namespace Quasar.Common.Messages
{
    [ProtoContract]
    public class SetStatus : IMessage
    {
        [ProtoMember(1)]
        public string Message { get; set; }
    }
}
