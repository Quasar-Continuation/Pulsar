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
using System.Windows.Forms;
using static Quasar.Server.Messages.FileManagerHandler;

namespace Quasar.Server.Messages
{
    public class MemoryDumpHandler : MessageProcessorBase<string>, IDisposable
    {
        /// <summary>
        /// Raised when a dump transfer updated.
        /// </summary>
        /// <remarks>
        /// Handlers registered with this event will be invoked on the 
        /// <see cref="System.Threading.SynchronizationContext"/> chosen when the instance was constructed.
        /// </remarks>
        public event FileTransferUpdatedEventHandler FileTransferUpdated;

        /// <summary>
        /// The client which is associated with this memory dump handler.
        /// </summary>
        private readonly Client _client;

        private readonly FileManagerHandler _fileManagerHandler;

        private readonly DoProcessDumpResponse _activeDump;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryDumpHandler"/> class using the given client.
        /// </summary>
        /// <param name="client">The associated client.</param>
        /// <param name="response">The process dump this handler tracks.</param>
        public MemoryDumpHandler(Client client, DoProcessDumpResponse response) : base(true)
        {
            _client = client;
            _activeDump = response;
            _fileManagerHandler = new FileManagerHandler(client);
            _fileManagerHandler.FileTransferUpdated += FileTransferUpdated;
            MessageHandler.Register(_fileManagerHandler);
        }

        /// <inheritdoc />
        public override bool CanExecute(IMessage message) => message is FileTransferComplete;

        /// <inheritdoc />
        public override bool CanExecuteFrom(ISender sender) => _client.Equals(sender);

        /// <inheritdoc />
        public override void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case FileTransferComplete complete:
                    Execute(sender, complete);
                    break;
            }
        }

        private void Execute(ISender sender, FileTransferComplete complete)
        {
            if (complete.FilePath == _activeDump.DumpPath)
            {
                MessageBox.Show("Transfer Completed!");
            }
        }

        public void BeginDumpDownload(DoProcessDumpResponse response)
        {
            string fileName = $"{response.UnixTime}_{response.Pid}_{response.ProcessName}.dmp";
            this._fileManagerHandler.BeginDownloadFile(response.DumpPath, fileName, true);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            this._fileManagerHandler.FileTransferUpdated -= FileTransferUpdated;
            MessageHandler.Unregister(this._fileManagerHandler);
            this._fileManagerHandler.Dispose();
        }
    }
}
