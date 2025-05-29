using ProtoBuf;
using Pulsar.Common.Messages.Other;
using System;
using System.Collections.Generic;

namespace Pulsar.Common.Messages
{
    [ProtoContract]
    public class GetSystemInfoResponse : IMessage
    {
        [ProtoMember(1)]
        public List<Tuple<string, string>> SystemInfos { get; set; }
    }
}
