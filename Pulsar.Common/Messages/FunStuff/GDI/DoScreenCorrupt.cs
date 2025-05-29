using ProtoBuf;
using Pulsar.Common.Messages.Other;
using Pulsar.Common.Models;

namespace Pulsar.Common.Messages.FunStuff.GDI
{
    [ProtoContract]
    public class DoScreenCorrupt : IMessage
    {
        [ProtoMember(1)]
        public string Message { get; set; }
    }
}