using ProtoBuf;
using Quasar.Common.Messages.other;

namespace Quasar.Common.Messages
{
    [ProtoContract]
    public class GetDebugLog : IMessage
    {
        [ProtoMember(1)]
        public string Log { get; set; }
    }
}
