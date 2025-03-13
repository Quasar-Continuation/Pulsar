using ProtoBuf;
using Quasar.Common.Messages.other;

namespace Quasar.Common.Messages.Monitoring.RemoteDesktop
{
    [ProtoContract]
    public class DoKeyboardEvent : IMessage
    {
        [ProtoMember(1)]
        public byte Key { get; set; }

        [ProtoMember(2)]
        public bool KeyDown { get; set; }
    }
}
