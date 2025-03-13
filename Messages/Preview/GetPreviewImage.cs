using ProtoBuf;
using Quasar.Common.Messages.other;

namespace Quasar.Common.Messages.Preview
{
    [ProtoContract]
    public class GetPreviewImage : IMessage
    {
        [ProtoMember(2)]
        public int Quality { get; set; }

        [ProtoMember(3)]
        public int DisplayIndex { get; set; }
    }
}
