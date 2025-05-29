using ProtoBuf;
using Pulsar.Common.Messages.Other;
using System;
using System.Collections.Generic;

namespace Pulsar.Common.Messages.Audio
{
    [ProtoContract]
    public class GetOutputDeviceResponse : IMessage
    {
        [ProtoMember(1)]
        public List<Tuple<int, string>> DeviceInfos { get; set; }
    }
}
