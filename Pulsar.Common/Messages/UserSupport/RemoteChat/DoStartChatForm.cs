using ProtoBuf;
using Pulsar.Common.Messages.other;

namespace Pulsar.Common.Messages.UserSupport.RemoteChat
{
    [ProtoContract]
    public class DoStartChatForm : IMessage
    {
        [ProtoMember(1)]
        public string Title { get; set; }

        [ProtoMember(2)]
        public string WelcomeMessage { get; set; }
        [ProtoMember(3)]
        public bool TopMost { get; set; }
        [ProtoMember(4)]
        public bool DisableClose { get; set; }
        [ProtoMember(5)]
        public bool DisableType { get; set; }
    }
}
