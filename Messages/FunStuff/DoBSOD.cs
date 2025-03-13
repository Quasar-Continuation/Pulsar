using ProtoBuf;
using Quasar.Common.Messages.other;
using Quasar.Common.Models;

namespace Quasar.Common.Messages.FunStuff
{
    [ProtoContract]
    public class DoBSOD : IMessage
    {
        [ProtoMember(1)]
        public string Message { get; set; }
    }
}
