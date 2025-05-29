using System;
using ProtoBuf;
using Pulsar.Common.Messages.Other;

namespace Pulsar.Common.Messages.Monitoring.HVNC
{
    [ProtoContract]
    public class StartHVNCProcess : IMessage
    {
        [ProtoMember(1)]
        public string Application { get; set; }
    }
}