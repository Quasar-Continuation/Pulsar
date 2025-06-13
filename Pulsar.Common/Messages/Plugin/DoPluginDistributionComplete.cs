using ProtoBuf;
using Pulsar.Common.Messages.Other;

namespace Pulsar.Common.Messages.Plugin
{
    [ProtoContract]
    public class DoPluginDistributionComplete : IMessage
    {
        [ProtoMember(1)]
        public string PluginName { get; set; }

        [ProtoMember(2)]
        public bool Success { get; set; }

        [ProtoMember(3)]
        public string ErrorMessage { get; set; }
    }
}
