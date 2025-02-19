using ProtoBuf;

namespace Quasar.Common.Messages.Webcam
{
    [ProtoContract]
    public class GetAvailableWebcamsResponse : IMessage
    {
        [ProtoMember(1)]
        public string[] Webcams { get; set; }
    }
}
