using Pulsar.Common.Messages;
using Pulsar.Common.Messages.Other;
using Pulsar.Common.Messages.UserSupport.MessageBox;
using Pulsar.Common.Messages.UserSupport.RemoteChat;
using Pulsar.Server.Forms.DarkMode;
using Pulsar.Server.Helper;
using Pulsar.Server.Messages;
using Pulsar.Server.Networking;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Pulsar.Server.Forms
{
    public partial class FrmRemoteChat : Form
    {
        private readonly Client _connectClient;

        private readonly RemoteChatHandler chatHandler;
        private static readonly Dictionary<Client, FrmRemoteChat> OpenedForms = new Dictionary<Client, FrmRemoteChat>();

        public static FrmRemoteChat CreateNewOrGetExisting(Client client)
        {
            if (OpenedForms.ContainsKey(client))
            {
                return OpenedForms[client];
            }
            FrmRemoteChat r = new FrmRemoteChat(client);
            r.Disposed += (sender, args) => OpenedForms.Remove(client);
            OpenedForms.Add(client, r);
            return r;
        }

        private void ClientDisconnected(Client client, bool connected)
        {
            if (!connected)
            {
                this.Invoke((MethodInvoker)this.Close);
            }
        }
        public FrmRemoteChat(Client client)
        {
            DarkModeManager.ApplyDarkMode(this);
            _connectClient = client;
            chatHandler = new RemoteChatHandler(client);
            RegisterMessageHandler();
            InitializeComponent();
            message.KeyDown += new KeyEventHandler(message_KeyDown); // Subscribe to the KeyDown event
        }


        private void RegisterMessageHandler()
        {
            _connectClient.ClientState += ClientDisconnected;
            chatHandler.PacketsRetrieved += AddMessageClient;
            MessageHandler.Register(chatHandler);
        }

        private void UnregisterMessageHandler()
        {
            MessageHandler.Unregister(chatHandler);
            chatHandler.PacketsRetrieved -= AddMessageClient;
            _connectClient.ClientState -= ClientDisconnected;
        }

        private void FrmRemoteChat_Load(object sender, EventArgs e)
        {

            this.Text = WindowHelper.GetWindowTitle("Remote Chat | ", _connectClient);
        }

        private void FrmRemoteChat_FormClosing(object sender, FormClosingEventArgs e)
        {
            chatHandler.KillForm();
        }
        public void AddMessageClient(object sender, string message)
        {
            if (this.IsHandleCreated && !this.Disposing && !this.IsDisposed)
            {
                Chatlog.Invoke((MethodInvoker)delegate
                {
                    Chatlog.AppendText(string.Format("{0} {1}: {2}{3}", DateTime.Now.ToString("HH:mm:ss"), "Victim", message, Environment.NewLine));
                });
            }
        }
        public void AddMessageServer(object sender, string message)
        {
            Chatlog.Invoke((MethodInvoker)delegate
            {
                Chatlog.AppendText(string.Format("{0} {1}: {2}{3}", DateTime.Now.ToString("HH:mm:ss"), sender, message, Environment.NewLine));
            });
        }

        private void Sendpacket_Click(object sender, EventArgs e)
        {
            if (message.Text.Trim() != "")
            {

                chatHandler.SendMessageClient(NameTB.Text, message.Text.Trim());

                AddMessageServer(NameTB.Text, message.Text.Trim());
                message.Text = "";
            }
        }

        private void SendBTN_Click(object sender, EventArgs e)
        {
            if (message.Text.Trim() != "")
            {

                chatHandler.SendMessageClient(NameTB.Text, message.Text.Trim());

                AddMessageServer(NameTB.Text, message.Text.Trim());
                message.Text = "";
            }
        }

        private void message_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; 
                Sendpacket_Click(this, new EventArgs()); 
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            chatHandler.StartForm(ChatTitleTB.Text, WelcomeMsgRB.Text, TopMostChk.Checked, DisableCloseChk.Checked, DisableTypeChk.Checked);
            noButtonTabControl1.SelectedIndex = 1;
            NameTB.Text = textBox1.Text;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            chatHandler.KillForm();
            noButtonTabControl1.SelectedIndex = 0;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _connectClient.Send(new DoChatAction());
            Chatlog.Clear();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.DefaultExt = "txt";
            saveFileDialog.Filter = "Text files (*.txt)|*.txt";
            saveFileDialog.FileName = DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = saveFileDialog.FileName;
                System.IO.File.WriteAllText(filePath, Chatlog.Text);
            }
        }

    }
}
