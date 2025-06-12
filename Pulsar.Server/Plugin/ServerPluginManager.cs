using Pulsar.Plugin.Common;
using Pulsar.Plugin.Common.Validation;
using Pulsar.Plugin.Common.Exceptions;
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
    public delegate void ExecutePluginDelegate(string clientId, string pluginName, string workId, byte[] input);    /// <summary>
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
        }        /// <summary>
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
                    LoadPluginSafely(pluginFile);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SERVER PLUGIN MANAGER] Error loading plugin {pluginFile}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Safely loads a plugin with validation and error handling.
        /// </summary>
        /// <param name="pluginPath">Path to the plugin file</param>
        private void LoadPluginSafely(string pluginPath)
        {
            var pluginName = Path.GetFileNameWithoutExtension(pluginPath);
            
            try
            {
                Console.WriteLine($"[SERVER PLUGIN MANAGER] Validating plugin: {pluginName}");
                
                // Validate plugin before loading
                var validationResult = PluginValidator.ValidatePluginFile(pluginPath);
                if (!validationResult.IsValid)
                {
                    Console.WriteLine($"[SERVER PLUGIN MANAGER] Plugin validation failed for {pluginName}: {validationResult.Message}");
                    return;
                }

                if (validationResult.HasWarnings)
                {
                    Console.WriteLine($"[SERVER PLUGIN MANAGER] Plugin validation warning for {pluginName}: {validationResult.Message}");
                }

                // Load the plugin
                LoadPlugin(pluginPath);
            }
            catch (PluginException pex)
            {
                Console.WriteLine($"[SERVER PLUGIN MANAGER] Plugin-specific error loading {pluginName}: {pex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SERVER PLUGIN MANAGER] Unexpected error loading plugin {pluginName}: {ex.Message}");
                Console.WriteLine($"[SERVER PLUGIN MANAGER] Stack trace: {ex.StackTrace}");
            }
        }        /// <summary>
        /// Loads a specific plugin from a file.
        /// </summary>
        /// <param name="pluginPath">Path to the plugin file</param>
        private void LoadPlugin(string pluginPath)
        {
            var pluginName = Path.GetFileNameWithoutExtension(pluginPath);
            
            try
            {
                Console.WriteLine($"[SERVER PLUGIN MANAGER] Attempting to load assembly: {Path.GetFileName(pluginPath)}");
                
                // Load assembly with error handling
                Assembly assembly;
                try
                {
                    assembly = Assembly.LoadFrom(pluginPath);
                }
                catch (BadImageFormatException)
                {
                    throw new PluginLoadException(pluginName, "Invalid assembly format - not a valid .NET assembly");
                }
                catch (FileLoadException ex)
                {
                    throw new PluginLoadException(pluginName, $"Failed to load assembly: {ex.Message}", ex);
                }

                Console.WriteLine($"[SERVER PLUGIN MANAGER] Assembly loaded successfully: {assembly.FullName}");

                Type[] allTypes;
                try
                {
                    allTypes = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    // Handle partial type loading
                    allTypes = ex.Types.Where(t => t != null).ToArray();
                    Console.WriteLine($"[SERVER PLUGIN MANAGER] Warning: Some types could not be loaded from {pluginName}");
                }

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
                    try
                    {
                        LoadPluginType(pluginType, pluginPath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[SERVER PLUGIN MANAGER] Error loading plugin type {pluginType.FullName}: {ex.Message}");
                    }
                }

                if (pluginTypes.Count == 0)
                {
                    Console.WriteLine($"[SERVER PLUGIN MANAGER] No IServerPlugin implementations found in {Path.GetFileName(pluginPath)}");
                }
            }
            catch (PluginException)
            {
                throw; // Re-throw plugin-specific exceptions
            }
            catch (Exception ex)
            {
                throw new PluginLoadException(pluginName, $"Unexpected error loading plugin: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Loads a specific plugin type with error handling.
        /// </summary>
        private void LoadPluginType(Type pluginType, string pluginPath)
        {
            var pluginName = Path.GetFileNameWithoutExtension(pluginPath);
            
            try
            {
                Console.WriteLine($"[SERVER PLUGIN MANAGER] Creating instance of plugin type: {pluginType.FullName}");
                
                IServerPlugin plugin;
                try
                {
                    plugin = Activator.CreateInstance(pluginType) as IServerPlugin;
                }
                catch (Exception ex)
                {
                    throw new PluginLoadException(pluginName, $"Failed to create instance of {pluginType.FullName}: {ex.Message}", ex);
                }

                if (plugin == null)
                {
                    throw new PluginLoadException(pluginName, $"Failed to create instance of {pluginType.FullName} - returned null");
                }

                // Initialize plugin with error handling
                try
                {
                    plugin.Initialize();
                }
                catch (Exception ex)
                {
                    throw new PluginLoadException(pluginName, $"Plugin initialization failed: {ex.Message}", ex);
                }

                // Check for duplicate plugin names
                if (_loadedPlugins.ContainsKey(plugin.Name))
                {
                    Console.WriteLine($"[SERVER PLUGIN MANAGER] Warning: Plugin with name '{plugin.Name}' already loaded. Skipping duplicate.");
                    return;
                }

                _loadedPlugins[plugin.Name] = plugin;
                Console.WriteLine($"[SERVER PLUGIN MANAGER] Loaded server plugin: {plugin.Name} v{plugin.Version} from {Path.GetFileName(pluginPath)}");
            }
            catch (PluginException)
            {
                throw; // Re-throw plugin-specific exceptions
            }
            catch (Exception ex)
            {
                throw new PluginLoadException(pluginName, $"Unexpected error loading plugin type {pluginType.FullName}: {ex.Message}", ex);
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
                PluginContext.SetExecutor((pName, wId, data) =>
                {
                    clientExecutor("*", pName, wId, data);
                });

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
                {                    string currentClientId = clientId;
                    PluginContext.SetExecutor((pName, wId, data) =>
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