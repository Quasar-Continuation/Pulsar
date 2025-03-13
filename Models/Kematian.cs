using Quasar.Common.Messages.other;

namespace Quasar.Common.Models
{
    public class KematianZipMessage : IMessage
    {
        public byte[] ZipFile { get; set; }
    }
}