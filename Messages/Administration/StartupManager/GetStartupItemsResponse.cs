using ProtoBuf;
using Quasar.Common.Messages.other;
using Quasar.Common.Models;
using System.Collections.Generic;

namespace Quasar.Common.Messages.Administration.StartupManager
{
    [ProtoContract]
    public class GetStartupItemsResponse : IMessage
    {
        [ProtoMember(1)]
        public List<StartupItem> StartupItems { get; set; }
    }
}
