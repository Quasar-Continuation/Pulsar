using ProtoBuf;
using Pulsar.Common.Messages.Other;
using Pulsar.Common.Models;

namespace Pulsar.Common.Messages.Plugin
{
    [ProtoContract]
    public class DoPluginDistributionChunk : IMessage
    {
        [ProtoMember(1)]
        public string PluginName { get; set; }

        [ProtoMember(2)]
        public long TotalSize { get; set; }

        [ProtoMember(3)]
        public FileChunk Chunk { get; set; }

        [ProtoMember(4)]
        public bool IsFirstChunk { get; set; }

        [ProtoMember(5)]
        public bool IsLastChunk { get; set; }

        [ProtoMember(6)]
        public int ChunkIndex { get; set; }

        [ProtoMember(7)]
        public int TotalChunks { get; set; }
    }
}
