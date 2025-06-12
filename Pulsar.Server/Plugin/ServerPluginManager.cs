using Pulsar.Plugin.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Pulsar.Server.Plugin
{
    /// <summary>
    /// Delegate for executing plugins on clients.
    /// </summary>
    /// <param name="clientId">ID of the client to execute on</param>
    /// <param name="pluginName">Name of the plugin to execute</param>
    /// <param name="workId">Work ID for tracking</param>
    /// <param name="input">Input data for the plugin</param>
    public delegate void ExecutePluginDelegate(string clientId, string pluginName, string workId, byte[] input);

    /// <summary>
    /// Context for plugin execution - provides access to client execution functionality.
    /// </summary>
    public static class PluginContext
    {
        private static readonly ThreadLocal<ExecutePluginDelegate> _currentExecutor = new ThreadLocal<ExecutePluginDelegate>();

        /// <summary>
        /// Sets the executor for the current thread.
        /// </summary>
        internal static void SetExecutor(ExecutePluginDelegate executor)
        {
            _currentExecutor.Value = executor;
        }

        /// <summary>
        /// Executes a plugin on all connected clients.
        /// </summary>
        /// <param name="pluginName">Name of the plugin to execute</param>
        /// <param name="workId">Work ID for tracking</param>
        /// <param name="input">Input data for the plugin</param>
        public static void ExecutePlugin(string pluginName, string workId, byte[] input)
        {
            var executor = _currentExecutor.Value;
            if (executor != null)
            {
                executor("*", pluginName, workId, input);
            }
            else
            {
                Console.WriteLine("[PLUGIN CONTEXT] No executor available for current thread");
            }
        }
    }

    /// <summary>
    /// Manages server-side plugins and provides communication mechanisms with client plugins.
    /// </summary>
    public class ServerPluginManager
    {
        private readonly ConcurrentDictionary<string, IServerPlugin> _loadedPlugins = new ConcurrentDictionary<string, IServerPlugin>();
        private readonly ConcurrentDictionary<string, ExecutePluginDelegate> _clientExecutors = new ConcurrentDictionary<string, ExecutePluginDelegate>();
        private readonly string _pluginDirectory;
        private readonly string _clientPluginDirectory;

        public ServerPluginManager()
        {
            _pluginDirectory = Path.Combine(Environment.CurrentDirectory, "Plugins", "Server");
            _clientPluginDirectory = Path.Combine(Environment.CurrentDirectory, "Plugins", "Client");

            Console.WriteLine($"[SERVER PLUGIN MANAGER] Initialized with directories:");
            Console.WriteLine($"  Server plugins: {_pluginDirectory}");
            Console.WriteLine($"  Client plugins: {_clientPluginDirectory}");
        }

        /// <summary>
        /// Registers a client executor for plugin communication.
        /// </summary>
        /// <param name="clientId">The client ID</param>
        /// <param name="executor">The executor delegate</param>
        public void RegisterClientExecutor(string clientId, ExecutePluginDelegate executor)
        {
            _clientExecutors[clientId] = executor;
            Console.WriteLine($"[SERVER PLUGIN MANAGER] Registered client executor for client: {clientId}");
        }

        /// <summary>
        /// Unregisters a client executor.
        /// </summary>
        /// <param name="clientId">The client ID</param>
        public void UnregisterClientExecutor(string clientId)
        {
            if (_clientExecutors.TryRemove(clientId, out _))
            {
                Console.WriteLine($"[SERVER PLUGIN MANAGER] Unregistered client executor for client: {clientId}");
            }
        }

        /// <summary>
        /// Executes a plugin on a specific client or all clients.
        /// </summary>
        /// <param name="clientId">Client ID or "*" for all clients</param>
        /// <param name="pluginName">Name of the plugin to execute</param>
        /// <param name="workId">Work ID for tracking</param>
        /// <param name="input">Input data for the plugin</param>
        public void ExecutePluginOnClient(string clientId, string pluginName, string workId, byte[] input)
        {
            Console.WriteLine($"[SERVER PLUGIN MANAGER] ExecutePluginOnClient called - Client: {clientId}, Plugin: {pluginName}, WorkId: {workId}");

            if (clientId == "*")
            {
                foreach (var executor in _clientExecutors.Values)
                {
                    try
                    {
                        executor(clientId, pluginName, workId, input);
                        Console.WriteLine($"[SERVER PLUGIN MANAGER] Executed plugin {pluginName} on client via executor");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[SERVER PLUGIN MANAGER] Error executing plugin on client: {ex.Message}");
                    }
                }
            }
            else
            {
                if (_clientExecutors.TryGetValue(clientId, out var executor))
                {
                    try
                    {
                        executor(clientId, pluginName, workId, input);
                        Console.WriteLine($"[SERVER PLUGIN MANAGER] Executed plugin {pluginName} on client {clientId}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[SERVER PLUGIN MANAGER] Error executing plugin on client {clientId}: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($"[SERVER PLUGIN MANAGER] No executor found for client: {clientId}");
                }
            }
        }

        /// <summary>
        /// Loads all plugins from the server plugin directory.
        /// </summary>
        public void LoadPluginsFromDirectory()
        {
            if (!Directory.Exists(_pluginDirectory))
            {
                Console.WriteLine($"[SERVER PLUGIN MANAGER] Plugin directory does not exist: {_pluginDirectory}");
                return;
            }

            var pluginFiles = Directory.GetFiles(_pluginDirectory, "*.dll");
            Console.WriteLine($"[SERVER PLUGIN MANAGER] Found {pluginFiles.Length} plugin files");

            foreach (var pluginFile in pluginFiles)
            {
                try
                {
                    LoadPlugin(pluginFile);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SERVER PLUGIN MANAGER] Error loading plugin {pluginFile}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Loads a specific plugin from a file.
        /// </summary>
        /// <param name="pluginPath">Path to the plugin file</param>
        private void LoadPlugin(string pluginPath)
        {
            try
            {
                Console.WriteLine($"[SERVER PLUGIN MANAGER] Attempting to load assembly: {Path.GetFileName(pluginPath)}");
                var assembly = Assembly.LoadFrom(pluginPath);
                Console.WriteLine($"[SERVER PLUGIN MANAGER] Assembly loaded successfully: {assembly.FullName}");

                var allTypes = assembly.GetTypes();
                Console.WriteLine($"[SERVER PLUGIN MANAGER] Found {allTypes.Length} types in assembly");

                var pluginTypes = allTypes
                    .Where(t => typeof(IServerPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                    .ToList();

                if (pluginTypes.Count == 0)
                {
                    Console.WriteLine($"[SERVER PLUGIN MANAGER] No plugins found with direct interface check, trying by name...");

                    var potentialPluginTypes = allTypes
                        .Where(t => !t.IsInterface && !t.IsAbstract &&
                                  (t.Name.EndsWith("ServerPlugin") || t.Name.EndsWith("Plugin")) &&
                                  t.GetInterfaces().Any(i => i.Name == "IServerPlugin"))
                        .ToList();

                    if (potentialPluginTypes.Count > 0)
                    {
                        Console.WriteLine($"[SERVER PLUGIN MANAGER] Found {potentialPluginTypes.Count} potential plugin types by name");
                        pluginTypes = potentialPluginTypes;
                    }
                }

                Console.WriteLine($"[SERVER PLUGIN MANAGER] Found {pluginTypes.Count} server plugin types");

                foreach (var pluginType in pluginTypes)
                {
                    Console.WriteLine($"[SERVER PLUGIN MANAGER] Creating instance of plugin type: {pluginType.FullName}");
                    var plugin = Activator.CreateInstance(pluginType) as IServerPlugin;
                    if (plugin != null)
                    {
                        plugin.Initialize();
                        _loadedPlugins[plugin.Name] = plugin;
                        Console.WriteLine($"[SERVER PLUGIN MANAGER] Loaded server plugin: {plugin.Name} from {Path.GetFileName(pluginPath)}");
                    }
                    else
                    {
                        Console.WriteLine($"[SERVER PLUGIN MANAGER] Failed to create instance of {pluginType.FullName}");
                    }
                }

                if (pluginTypes.Count == 0)
                {
                    Console.WriteLine($"[SERVER PLUGIN MANAGER] No IServerPlugin implementations found in {Path.GetFileName(pluginPath)}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SERVER PLUGIN MANAGER] Error loading plugin from {pluginPath}: {ex.Message}");
                Console.WriteLine($"[SERVER PLUGIN MANAGER] Stack trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Gets the names of all loaded plugins.
        /// </summary>
        /// <returns>Collection of plugin names</returns>
        public IEnumerable<string> GetLoadedPlugins()
        {
            return _loadedPlugins.Keys;
        }

        /// <summary>
        /// Executes a server plugin with the given parameters.
        /// </summary>
        /// <param name="pluginName">Name of the plugin to execute</param>
        /// <param name="workId">Work ID for tracking</param>
        /// <param name="input">Input data for the plugin</param>
        /// <param name="clientExecutor">Executor for communicating with clients</param>
        /// <returns>Plugin execution result</returns>
        public byte[] ExecutePlugin(string pluginName, string workId, byte[] input, ExecutePluginDelegate clientExecutor)
        {
            Console.WriteLine($"[SERVER PLUGIN MANAGER] ExecutePlugin called - Plugin: {pluginName}, WorkId: {workId}");

            if (!_loadedPlugins.TryGetValue(pluginName, out var plugin))
            {
                Console.WriteLine($"[SERVER PLUGIN MANAGER] Plugin not found: {pluginName}");
                return null;
            }

            try
            {
                PluginContext.SetExecutor(clientExecutor);

                Console.WriteLine($"[SERVER PLUGIN MANAGER] Executing server plugin: {pluginName}");
                plugin.ProcessResponse("*", workId, input);
                Console.WriteLine($"[SERVER PLUGIN MANAGER] Server plugin {pluginName} executed successfully");

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SERVER PLUGIN MANAGER] Error executing server plugin {pluginName}: {ex.Message}");
                Console.WriteLine($"[SERVER PLUGIN MANAGER] Stack trace: {ex.StackTrace}");
                return null;
            }
        }

        /// <summary>
        /// Handles responses from client plugins and routes them to appropriate server plugins.
        /// </summary>
        /// <param name="clientId">ID of the client that sent the response</param>
        /// <param name="pluginName">Name of the plugin that sent the response</param>
        /// <param name="workId">Work ID for tracking</param>
        /// <param name="responseData">Response data from the client plugin</param>
        public void HandleClientPluginResponse(string clientId, string pluginName, string workId, byte[] responseData)
        {
            Console.WriteLine($"[SERVER PLUGIN MANAGER] HandleClientPluginResponse - Client: {clientId}, Plugin: {pluginName}, WorkId: {workId}");

            string targetServerPlugin = pluginName;
            bool foundMatchingServerPlugin = false;

            if (_loadedPlugins.ContainsKey(pluginName))
            {
                foundMatchingServerPlugin = true;
                Console.WriteLine($"[SERVER PLUGIN MANAGER] Using direct match for plugin: {pluginName}");
            }

            else if (pluginName.EndsWith(".Client"))
            {
                string possibleServerName = pluginName.Replace(".Client", ".Server");
                if (_loadedPlugins.ContainsKey(possibleServerName))
                {
                    targetServerPlugin = possibleServerName;
                    foundMatchingServerPlugin = true;
                    Console.WriteLine($"[SERVER PLUGIN MANAGER] Routing from {pluginName} to {targetServerPlugin}");
                }
            }

            else if (pluginName == "Pulsar.Plugin.Client")
            {
                targetServerPlugin = "Pulsar.Plugin.Server";
                if (_loadedPlugins.ContainsKey(targetServerPlugin))
                {
                    foundMatchingServerPlugin = true;
                    Console.WriteLine($"[SERVER PLUGIN MANAGER] Routing from {pluginName} to {targetServerPlugin}");
                }
            }

            if (!foundMatchingServerPlugin)
            {
                Console.WriteLine($"[SERVER PLUGIN MANAGER] No direct match found, looking for alternative server plugins for {pluginName}");

                foreach (var serverPluginName in _loadedPlugins.Keys)
                {
                    Console.WriteLine($"[SERVER PLUGIN MANAGER] Checking against loaded server plugin: {serverPluginName}");

                    if (serverPluginName.EndsWith(".Server"))
                    {
                        string baseServerName = serverPluginName.Substring(0, serverPluginName.Length - ".Server".Length);

                        if (pluginName.StartsWith(baseServerName))
                        {
                            targetServerPlugin = serverPluginName;
                            foundMatchingServerPlugin = true;
                            Console.WriteLine($"[SERVER PLUGIN MANAGER] Found matching server plugin using base name: routing from {pluginName} to {targetServerPlugin}");
                            break;
                        }
                    }
                }
            }

            if (foundMatchingServerPlugin && _loadedPlugins.TryGetValue(targetServerPlugin, out var serverPlugin))
            {
                try
                {
                    ExecutePluginDelegate executor = (cId, pName, wId, data) =>
                    {
                        ExecutePluginOnClient(cId, pName, wId, data);
                    };                    
                    
                    PluginContext.SetExecutor(executor);

                    string currentClientId = clientId;
                    Pulsar.Plugin.Common.PluginContext.SetExecutor((pName, wId, data) =>
                    {
                        ExecutePluginOnClient(currentClientId, pName, wId, data);
                    });

                    Console.WriteLine($"[SERVER PLUGIN MANAGER] Forwarding response to server plugin: {targetServerPlugin}");
                    serverPlugin.ProcessResponse(clientId, workId, responseData);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SERVER PLUGIN MANAGER] Error handling client plugin response for {targetServerPlugin}: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine($"[SERVER PLUGIN MANAGER] No server plugin found to handle response from {pluginName}");
                Console.WriteLine($"[SERVER PLUGIN MANAGER] Available server plugins: {string.Join(", ", _loadedPlugins.Keys)}");
                Console.WriteLine($"[SERVER PLUGIN MANAGER] Please ensure your server plugin is named '{pluginName.Replace(".Client", ".Server")}' or similar");
                Console.WriteLine($"[SERVER PLUGIN MANAGER] Plugin should be placed in: {_pluginDirectory}");
            }
        }

        /// <summary>
        /// Gets the bytes of a client plugin for distribution.
        /// </summary>
        /// <param name="pluginName">Name of the client plugin</param>
        /// <returns>Plugin bytes or null if not found</returns>
        public byte[] GetClientPluginBytes(string pluginName)
        {
            try
            {
                var pluginPath = Path.Combine(_clientPluginDirectory, $"{pluginName}.dll");

                if (File.Exists(pluginPath))
                {
                    var bytes = File.ReadAllBytes(pluginPath);
                    Console.WriteLine($"[SERVER PLUGIN MANAGER] Retrieved {bytes.Length} bytes for client plugin: {pluginName}");
                    return bytes;
                }
                else
                {
                    Console.WriteLine($"[SERVER PLUGIN MANAGER] Client plugin file not found: {pluginPath}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SERVER PLUGIN MANAGER] Error getting client plugin bytes for {pluginName}: {ex.Message}");
                return null;
            }
        }
    }
}