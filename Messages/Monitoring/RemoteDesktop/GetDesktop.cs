using ProtoBuf;
using Quasar.Common.Enums;

namespace Quasar.Common.Messages.Monitoring.RemoteDesktop
{
    [ProtoContract]
    public class GetDesktop : IMessage
    {
        [ProtoMember(1)]
        public bool CreateNew { get; set; }

        [ProtoMember(2)]
        public int Quality { get; set; }

        [ProtoMember(3)]
        public int DisplayIndex { get; set; }

        [ProtoMember(4)]
        public RemoteDesktopStatus Status { get; set; }
    }

    public class GetDesktopHVNC : IMessage
    {
        [ProtoMember(1)]
        public bool CreateNew { get; set; }

        [ProtoMember(2)]
        public int Quality { get; set; }

        [ProtoMember(3)]
        public int DisplayIndex { get; set; }

        [ProtoMember(4)]
        public HVNCStatus Status { get; set; }
    }
}
