using System;
using ProtoBuf;
using Quasar.Common.Messages.other;

namespace Quasar.Common.Messages
{
    // Token: 0x02000064 RID: 100
    [ProtoContract]
    public class PluginStarted : IMessage
    {
        // Token: 0x170000CA RID: 202
        // (get) Token: 0x06000237 RID: 567 RVA: 0x000042D0 File Offset: 0x000024D0
        // (set) Token: 0x06000238 RID: 568 RVA: 0x000042D8 File Offset: 0x000024D8
        [ProtoMember(1)]
        public string Message { get; set; }
    }
}
