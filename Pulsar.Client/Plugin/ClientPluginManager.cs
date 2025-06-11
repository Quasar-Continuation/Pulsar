using Pulsar.Plugin.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Pulsar.Client.Plugin
{
    /// <summary>
    /// Manages client-side plugins including loading, execution, and lifecycle.
    /// </summary>
    public class ClientPluginManager
    {
        private readonly ConcurrentDictionary<string, IClientPlugin> _loadedPlugins;
        private readonly ConcurrentDictionary<string, Assembly> _pluginAssemblies;
        private readonly string _pluginDirectory;

        public ClientPluginManager()
        {
            _loadedPlugins = new ConcurrentDictionary<string, IClientPlugin>();
            _pluginAssemblies = new ConcurrentDictionary<string, Assembly>();
            _pluginDirectory = Path.Combine(Path.GetTempPath(), "PulsarPlugins");
            
            // Ensure plugin directory exists
            Directory.CreateDirectory(_pluginDirectory);
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
                // Load assembly from memory
                Assembly assembly = Assembly.Load(pluginBytes);
                
                // Find types implementing IClientPlugin
                var pluginTypes = assembly.GetTypes()
                    .Where(t => t.GetInterfaces().Contains(typeof(IClientPlugin)) && !t.IsAbstract)
                    .ToList();

                if (pluginTypes.Count == 0)
                {
                    return false;
                }

                // Create instance of the first plugin type found
                var pluginType = pluginTypes.First();
                var plugin = (IClientPlugin)Activator.CreateInstance(pluginType);
                
                plugin.Initialize();
                
                _pluginAssemblies[pluginName] = assembly;
                _loadedPlugins[pluginName] = plugin;
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load plugin {pluginName}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Executes a plugin with the provided input.
        /// </summary>
        /// <param name="pluginName">Name of the plugin to execute.</param>
        /// <param name="input">Input data for the plugin.</param>
        /// <returns>Output from plugin execution, or null if execution failed.</returns>
        public byte[] ExecutePlugin(string pluginName, byte[] input)
        {
            if (_loadedPlugins.TryGetValue(pluginName, out IClientPlugin plugin))
            {
                try
                {
                    return plugin.Execute(input);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error executing plugin {pluginName}: {ex.Message}");
                    return null;
                }
            }
            
            return null;
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
        /// Unloads a plugin.
        /// </summary>
        /// <param name="pluginName">Name of the plugin to unload.</param>
        /// <returns>True if unloaded successfully.</returns>
        public bool UnloadPlugin(string pluginName)
        {
            try
            {
                if (_loadedPlugins.TryRemove(pluginName, out IClientPlugin plugin))
                {
                    plugin.Cleanup();
                }
                
                _pluginAssemblies.TryRemove(pluginName, out _);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error unloading plugin {pluginName}: {ex.Message}");
                return false;
            }
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
        }
    }
}
