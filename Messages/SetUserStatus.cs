using ProtoBuf;
using Quasar.Common.Enums;
using Quasar.Common.Messages.other;

namespace Quasar.Common.Messages
{
    [ProtoContract]
    public class SetUserStatus : IMessage
    {
        [ProtoMember(1)]
        public UserStatus Message { get; set; }
    }
}
