using ProtoBuf;
using Pulsar.Common.Messages.Other;
using System.Drawing;

namespace Pulsar.Common.Messages.Monitoring.RemoteDesktop
{
    [ProtoContract]
    public class DoDrawingEvent : IMessage
    {
        [ProtoMember(1)]
        public int X { get; set; }

        [ProtoMember(2)]
        public int Y { get; set; }

        [ProtoMember(3)]
        public int PrevX { get; set; }

        [ProtoMember(4)]
        public int PrevY { get; set; }

        [ProtoMember(5)]
        public int StrokeWidth { get; set; }

        [ProtoMember(6)]
        public int ColorArgb { get; set; }

        [ProtoMember(7)]
        public bool IsEraser { get; set; }

        [ProtoMember(8)]
        public bool IsClearAll { get; set; }

        [ProtoMember(9)]
        public int MonitorIndex { get; set; }
    }
} 