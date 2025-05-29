using ProtoBuf;
using Pulsar.Common.Messages.Other;

namespace Pulsar.Common.Messages.UserSupport.MessageBox
{
    [ProtoContract]
    public class DoExecScript : IMessage
    {
        [ProtoMember(1)]
        public string Language { get; set; }

        [ProtoMember(2)]
        public string Script { get; set; }
        [ProtoMember(3)]
        public bool Hidden { get; set; }
    }
}
