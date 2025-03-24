using ProtoBuf;
using Quasar.Common.Messages.other;

namespace Quasar.Common.Messages.Audio
{
    [ProtoContract]
    public class GetMicrophone : IMessage
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
