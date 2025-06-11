using ProtoBuf;
using Pulsar.Common.Messages.Other;

namespace Pulsar.Common.Messages.Plugin
{
    [ProtoContract]
    public class DoPluginExecution : IMessage
    {
        [ProtoMember(1)]
        public string PluginName { get; set; }

        [ProtoMember(2)]
        public string WorkId { get; set; }

        [ProtoMember(3)]
        public PluginOperationType Type { get; set; }

        [ProtoMember(4)]
        public byte[] Output { get; set; }
    }

    public enum PluginOperationType
    {
        Execute = 0,
        Response = 1,
        Error = 2
    }
}
