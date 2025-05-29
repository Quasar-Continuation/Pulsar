using ProtoBuf;
using Pulsar.Common.Messages.Other;
using Pulsar.Common.Models;
using System.Collections.Generic;

namespace Pulsar.Common.Messages.Administration.StartupManager
{
    [ProtoContract]
    public class GetStartupItemsResponse : IMessage
    {
        [ProtoMember(1)]
        public List<StartupItem> StartupItems { get; set; }
    }
}
