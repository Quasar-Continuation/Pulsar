using ProtoBuf;
using Quasar.Common.Messages.other;

namespace Quasar.Common.Messages
{
    [ProtoContract]
    public class GetMonitorsResponse : IMessage
    {
        [ProtoMember(1)]
        public int Number { get; set; }
    }
}
