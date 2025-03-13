﻿using System;
using ProtoBuf;
using Quasar.Common.Messages.other;

namespace Quasar.Common.Messages.Monitoring.HVNC
{
    [ProtoContract]
    public class StartHVNCProcess : IMessage
    {
        [ProtoMember(1)]
        public string Application { get; set; }
    }
}