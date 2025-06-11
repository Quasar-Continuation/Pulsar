using ProtoBuf;
using System;

namespace Pulsar.Plugin.Common.Messages
{
    /// <summary>
    /// ProtoBuf message for system information requests.
    /// </summary>
    [ProtoContract]
    public class SystemInfoRequest
    {
        [ProtoMember(1)]
        public bool IncludeSoftware { get; set; } = true;

        [ProtoMember(2)]
        public bool IncludeProcesses { get; set; } = true;

        [ProtoMember(3)]
        public bool IncludeUptime { get; set; } = true;
    }

    /// <summary>
    /// ProtoBuf message for system information responses.
    /// </summary>
    [ProtoContract]
    public class SystemInfoResponse
    {
        [ProtoMember(1)]
        public string ComputerName { get; set; }

        [ProtoMember(2)]
        public string UserName { get; set; }

        [ProtoMember(3)]
        public string OSVersion { get; set; }

        [ProtoMember(4)]
        public int ProcessorCount { get; set; }

        [ProtoMember(5)]
        public long WorkingSet { get; set; }

        [ProtoMember(6)]
        public string[] InstalledSoftware { get; set; }

        [ProtoMember(7)]
        public string[] RunningProcesses { get; set; }

        [ProtoMember(8)]
        public long SystemUpticksTicks { get; set; }

        [ProtoMember(9)]
        public long TimestampTicks { get; set; }

        /// <summary>
        /// Helper property to get/set SystemUptime as TimeSpan.
        /// </summary>
        [ProtoIgnore]
        public TimeSpan SystemUptime
        {
            get => new TimeSpan(SystemUpticksTicks);
            set => SystemUpticksTicks = value.Ticks;
        }

        /// <summary>
        /// Helper property to get/set Timestamp as DateTime.
        /// </summary>
        [ProtoIgnore]
        public DateTime Timestamp
        {
            get => new DateTime(TimestampTicks, DateTimeKind.Utc);
            set => TimestampTicks = value.ToUniversalTime().Ticks;
        }
    }

    /// <summary>
    /// ProtoBuf message for plugin error responses.
    /// </summary>
    [ProtoContract]
    public class PluginErrorResponse
    {
        [ProtoMember(1)]
        public string Error { get; set; }

        [ProtoMember(2)]
        public string StackTrace { get; set; }
    }
}
