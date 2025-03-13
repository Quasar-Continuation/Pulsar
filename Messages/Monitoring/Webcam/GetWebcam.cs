using ProtoBuf;
using Quasar.Common.Enums;
using Quasar.Common.Messages.other;

namespace Quasar.Common.Messages.Webcam
{
    [ProtoContract]
    public class GetWebcam : IMessage
    {
        [ProtoMember(1)]
        public bool CreateNew { get; set; }

        [ProtoMember(2)]
        public int Quality { get; set; }

        [ProtoMember(3)]
        public int DisplayIndex { get; set; }

        [ProtoMember(4)]
        public RemoteWebcamStatus Status { get; set; }

        [ProtoMember(5)]
        public bool UseGPU { get; set; }

        [ProtoMember(6)]
        public int FramesRequested { get; set; } = 1;

        [ProtoMember(7)]
        public bool IsBufferedMode { get; set; } = true;
    }
}
