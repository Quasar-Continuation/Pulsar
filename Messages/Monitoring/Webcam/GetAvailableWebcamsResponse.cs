using ProtoBuf;
using Quasar.Common.Messages.other;

namespace Quasar.Common.Messages.Webcam
{
    [ProtoContract]
    public class GetAvailableWebcamsResponse : IMessage
    {
        [ProtoMember(1)]
        public string[] Webcams { get; set; }
    }
}
