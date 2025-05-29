using ProtoBuf;
using Pulsar.Common.Messages.Other;
using Pulsar.Common.Models.Query.Browsers;
using System.Collections.Generic;

namespace Pulsar.Common.Messages.Monitoring.Query.Browsers
{
    [ProtoContract]
    public class GetBrowsersResponse : IMessage
    {
        [ProtoMember(1)]
        public List<QueryBrowsers> QueryBrowsers { get; set; }
    }
}
