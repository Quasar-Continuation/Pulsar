using ProtoBuf;
using Pulsar.Common.Messages.Other;

namespace Pulsar.Common.Messages.Plugin
{
    [ProtoContract]
    public class DoPluginDistribution : IMessage
    {
        [ProtoMember(1)]
        public string PluginName { get; set; }

        [ProtoMember(2)]
        public byte[] PluginContent { get; set; }
    }
}
