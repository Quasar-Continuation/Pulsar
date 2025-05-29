using ProtoBuf;
using Pulsar.Common.Messages.Other;

namespace Pulsar.Common.Messages.Webcam
{
    [ProtoContract]
    public class GetAvailableWebcamsResponse : IMessage
    {
        [ProtoMember(1)]
        public string[] Webcams { get; set; }
    }
}
