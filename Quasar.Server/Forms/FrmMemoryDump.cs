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
using System.Threading;
using System.Windows.Forms;
using System.Threading.Tasks;

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
        public static FrmMemoryDump CreateNewOrGetExisting(Client client, DoProcessDumpResponse dump) // Check this
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
            InitializeComponent();

            _connectClient = client;
            _dumpHandler = new MemoryDumpHandler(client, dump);
            _dumpedProcess = dump;
            RegisterMessageHandler();
            progressDownload.Maximum = (int)dump.Length;
            progressDownload.Minimum = 0;

            DarkModeManager.ApplyDarkMode(this);
        }

        private void RegisterMessageHandler()
        {
            _connectClient.ClientState += ClientDisconnected;
            _dumpHandler.FileTransferUpdated += FileTransferUpdated;
            MessageHandler.Register(_dumpHandler);
        }

        private void UnregisterMessageHandler()
        {
            MessageHandler.Unregister(_dumpHandler);
            _dumpHandler.FileTransferUpdated -= FileTransferUpdated;
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

        private void FileTransferUpdated(object sender, FileTransfer transfer)
        {
            if (transfer.RemotePath == _dumpedProcess.DumpPath)
            {
                if (transfer.Status == "Completed")
                {
                    MessageBox.Show("Dump Transfer Complete!");
                    this.Close();
                }
                if (progressDownload.InvokeRequired)
                {
                    progressDownload.BeginInvoke((MethodInvoker)delegate
                    {
                        progressDownload.Value = (int)transfer.TransferredSize;
                    });
                }
                else
                {
                    progressDownload.Value = (int)transfer.TransferredSize;
                }
                if (labelProgress.InvokeRequired)
                {
                    labelProgress.BeginInvoke((MethodInvoker)delegate
                    {
                        labelProgress.Text = $"Progress: {Math.Round((double)(transfer.TransferredSize / this._dumpedProcess.Length), 2)}%";
                    });
                }
                else
                {
                    labelProgress.Text = $"Progress: {Math.Round((double)(transfer.TransferredSize / this._dumpedProcess.Length), 2)}%";
                }
                if (labelValue.InvokeRequired)
                {
                    labelValue.BeginInvoke((MethodInvoker)delegate
                    {
                        labelValue.Text = transfer.Status;
                    });
                }
                else
                {
                    labelValue.Text = transfer.Status;
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            // Fix this later
            _connectClient.Send(new FileTransferCancel { Id = 0, Reason = "User Requested" });
            this.Close();
        }

        private void FrmMemoryDump_Load(object sender, EventArgs e)
        {
            this.Text = WindowHelper.GetWindowTitle("Memory Dump", _connectClient) + $" of {_dumpedProcess.Pid} : {_dumpedProcess.ProcessName}";
        }

        private void FrmMemoryDump_FormClosing(object sender, FormClosingEventArgs e)
        {
            UnregisterMessageHandler();
            _dumpHandler.Dispose();
        }

        private void FrmMemoryDump_Shown(object sender, EventArgs e)
        {
            MessageBox.Show("Starting Download...");
            Task.Run(() =>
            {
                _dumpHandler.BeginDumpDownload(_dumpedProcess);
            });
        }
    }
}
