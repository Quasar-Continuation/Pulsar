using System;
using Quasar.Common.Messages;
using Quasar.Common.Messages.other;
using Quasar.Common.Networking;
using Quasar.Server.Networking;

namespace Quasar.Server.Messages
{
    // Token: 0x02000020 RID: 32
    public class PluginHandler : MessageProcessorBase<object>
    {
        // Token: 0x14000018 RID: 24
        // (add) Token: 0x06000226 RID: 550 RVA: 0x00007EE4 File Offset: 0x000060E4
        // (remove) Token: 0x06000227 RID: 551 RVA: 0x00007F1C File Offset: 0x0000611C
        public event PluginHandler.StatusUpdatedEventHandler StatusUpdated;

        // Token: 0x06000228 RID: 552 RVA: 0x00007F54 File Offset: 0x00006154
        private void OnStatusUpdated(Client client, string statusMessage, bool FromHandler)
        {
            this.SynchronizationContext.Post(delegate (object c)
            {
                PluginHandler.StatusUpdatedEventHandler statusUpdated = this.StatusUpdated;
                if (statusUpdated == null)
                {
                    return;
                }
                statusUpdated(this, (Client)c, statusMessage, true);
            }, client);
        }

        // Token: 0x06000229 RID: 553 RVA: 0x00007F8D File Offset: 0x0000618D
        public PluginHandler() : base(true)
        {
        }

        // Token: 0x0600022A RID: 554 RVA: 0x00007F96 File Offset: 0x00006196
        public override bool CanExecute(IMessage message)
        {
            return message is PluginStarted;
        }

        // Token: 0x0600022B RID: 555 RVA: 0x00007FA1 File Offset: 0x000061A1
        public override bool CanExecuteFrom(ISender sender)
        {
            return true;
        }

        // Token: 0x0600022C RID: 556 RVA: 0x00007FA4 File Offset: 0x000061A4
        public override void Execute(ISender sender, IMessage message)
        {
            PluginStarted pluginStarted = message as PluginStarted;
            if (pluginStarted != null)
            {
                this.Execute((Client)sender, pluginStarted);
                return;
            }
            this.Execute((Client)sender);
        }

        private void Execute(Client sender) { }


        // Token: 0x0600022D RID: 557 RVA: 0x00007FE1 File Offset: 0x000061E1
        private void Execute(Client client, PluginStarted message)
        {
            this.OnStatusUpdated(client, message.Message, true);
        }

        // Token: 0x0200008A RID: 138
        // (Invoke) Token: 0x0600062A RID: 1578
        public delegate void StatusUpdatedEventHandler(object sender, Client client, string statusMessage, bool FromHandler);
    }
}
