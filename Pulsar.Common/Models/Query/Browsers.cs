using ProtoBuf;

namespace Pulsar.Common.Models.Query.Browsers
{
    [ProtoContract]
    public class QueryBrowsers
    {
        [ProtoMember(1)]
        public string Location { get; set; }

        [ProtoMember(2)]
        public string Browser { get; set; }
    }
}
