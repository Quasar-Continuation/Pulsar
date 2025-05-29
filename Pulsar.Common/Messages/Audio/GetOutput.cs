using ProtoBuf;
using Pulsar.Common.Messages.Other;

namespace Pulsar.Common.Messages.Audio
{
    [ProtoContract]
    public class GetOutput : IMessage
    {
        [ProtoMember(1)]
        public bool CreateNew { get; set; }

        [ProtoMember(2)]
        public int DeviceIndex { get; set; }

        [ProtoMember(3)]
        public int Bitrate { get; set; }

        [ProtoMember(4)]
        public bool Destroy { get; set; }
    }
}
