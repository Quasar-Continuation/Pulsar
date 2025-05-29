using Pulsar.Common.Enums;
using Pulsar.Common.Networking;
using ProtoBuf;
using Pulsar.Common.Messages.Other;

namespace Pulsar.Common.Messages.Monitoring.Kematian
{
    [ProtoContract]
    public class SetKematianStatus : IMessage
    {
        [ProtoMember(1)]
        public KematianStatus Status { get; set; }
        
        [ProtoMember(2)]
        public int Percentage { get; set; }
    }
}