﻿using ProtoBuf;
using Quasar.Common.Messages.other;
using Quasar.Common.Video;

namespace Quasar.Common.Messages.Webcam
{
    [ProtoContract]
    public class GetWebcamResponse : IMessage
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
        public long Timestamp { get; set; }

        [ProtoMember(6)]
        public bool IsLastRequestedFrame { get; set; }
    }
}
