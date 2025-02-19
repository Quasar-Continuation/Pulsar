using ProtoBuf;
using Quasar.Common.Enums;

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
    }
}
