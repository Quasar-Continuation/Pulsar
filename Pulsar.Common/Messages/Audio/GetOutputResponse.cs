using ProtoBuf;
using Pulsar.Common.Messages.Other;

namespace Pulsar.Common.Messages.Audio
{
    [ProtoContract]
    public class GetOutputResponse : IMessage
    {
        [ProtoMember(1)]
        public byte[] Audio { get; set; }

        [ProtoMember(2)]
        public int Device { get; set; }
    }
}
