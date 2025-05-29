using System;
using ProtoBuf;
using Pulsar.Common.Messages.Other;

namespace Pulsar.Common.Messages.Other
{
    [ProtoContract]
    public class StartProcessOnMonitor : IMessage
    {
        [ProtoMember(1)]
        public string Application { get; set; }

        [ProtoMember(2)]
        public int MonitorID { get; set; }
    }
}