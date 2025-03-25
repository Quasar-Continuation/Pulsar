using ProtoBuf;
using Quasar.Common.Messages.other;

namespace Quasar.Common.Messages.Administration.FileManager
{
    [ProtoContract]
    public class DoZipFolder : IMessage
    {
        [ProtoMember(1)]
        public string SourcePath { get; set; }

        [ProtoMember(2)]
        public string DestinationPath { get; set; }

        [ProtoMember(3)]
        public int CompressionLevel { get; set; } = (int)System.IO.Compression.CompressionLevel.Optimal;
    }
}
