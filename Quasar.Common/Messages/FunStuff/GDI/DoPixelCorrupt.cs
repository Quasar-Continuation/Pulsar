using ProtoBuf;
using Quasar.Common.Messages.other;
using Quasar.Common.Models;

namespace Quasar.Common.Messages.FunStuff.GDI
{
    [ProtoContract]
    public class DoPixelCorrupt : IMessage
    {
        [ProtoMember(1)]
        public string Message { get; set; }
    }
}