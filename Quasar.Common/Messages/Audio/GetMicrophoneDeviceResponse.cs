using ProtoBuf;
using Quasar.Common.Messages.other;
using System;
using System.Collections.Generic;

namespace Quasar.Common.Messages.Audio
{
    [ProtoContract]
    public class GetMicrophoneDeviceResponse : IMessage
    {
        [ProtoMember(1)]
        public List<Tuple<int, string>> DeviceInfos { get; set; }
    }
}
