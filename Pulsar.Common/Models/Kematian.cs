using Pulsar.Common.Messages.Other;

namespace Pulsar.Common.Models
{
    public class KematianZipMessage : IMessage
    {
        public byte[] ZipFile { get; set; }
    }
}