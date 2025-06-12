using Pulsar.Plugin.Common;
using Pulsar.Plugin.Common.Validation;
using Pulsar.Plugin.Common.Exceptions;
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
        /// Loads a plugin from byte array with validation and error handling.
        /// </summary>
        /// <param name="pluginName">Name of the plugin.</param>
        /// <param name="pluginBytes">Plugin assembly bytes.</param>
        /// <returns>True if loaded successfully, false otherwise.</returns>
        public bool LoadPlugin(string pluginName, byte[] pluginBytes)
        {
            try
            {
                // Validate plugin before loading
                var validationResult = PluginValidator.ValidatePluginBytes(pluginBytes, pluginName);
                if (!validationResult.IsValid)
                {
                    Console.WriteLine($"[CLIENT PLUGIN MANAGER] Plugin validation failed for {pluginName}: {validationResult.Message}");
                    return false;
                }

                if (validationResult.HasWarnings)
                {
                    Console.WriteLine($"[CLIENT PLUGIN MANAGER] Plugin validation warning for {pluginName}: {validationResult.Message}");
                }

                // Load assembly from memory with error handling
                Assembly assembly;
                try
                {
                    assembly = Assembly.Load(pluginBytes);
                }
                catch (BadImageFormatException)
                {
                    Console.WriteLine($"[CLIENT PLUGIN MANAGER] Invalid assembly format for plugin {pluginName}");
                    return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[CLIENT PLUGIN MANAGER] Failed to load assembly for plugin {pluginName}: {ex.Message}");
                    return false;
                }
                
                // Find types implementing IClientPlugin with error handling
                List<Type> pluginTypes;
                try
                {
                    pluginTypes = assembly.GetTypes()
                        .Where(t => t.GetInterfaces().Contains(typeof(IClientPlugin)) && !t.IsAbstract)
                        .ToList();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    // Handle partial type loading
                    pluginTypes = ex.Types.Where(t => t != null && t.GetInterfaces().Contains(typeof(IClientPlugin)) && !t.IsAbstract)
                        .ToList();
                    Console.WriteLine($"[CLIENT PLUGIN MANAGER] Warning: Some types could not be loaded from {pluginName}");
                }

                if (pluginTypes.Count == 0)
                {
                    Console.WriteLine($"[CLIENT PLUGIN MANAGER] No IClientPlugin implementations found in {pluginName}");
                    return false;
                }

                // Create instance of the first plugin type found with error handling
                var pluginType = pluginTypes.First();
                IClientPlugin plugin;
                try
                {
                    plugin = (IClientPlugin)Activator.CreateInstance(pluginType);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[CLIENT PLUGIN MANAGER] Failed to create instance of plugin {pluginName}: {ex.Message}");
                    return false;
                }

                if (plugin == null)
                {
                    Console.WriteLine($"[CLIENT PLUGIN MANAGER] Plugin instance is null for {pluginName}");
                    return false;
                }

                // Initialize plugin with error handling
                try
                {
                    plugin.Initialize();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[CLIENT PLUGIN MANAGER] Plugin initialization failed for {pluginName}: {ex.Message}");
                    return false;
                }
                
                // Check for duplicate plugin names
                if (_loadedPlugins.ContainsKey(pluginName))
                {
                    Console.WriteLine($"[CLIENT PLUGIN MANAGER] Warning: Plugin with name '{pluginName}' already loaded. Unloading previous version.");
                    UnloadPlugin(pluginName);
                }

                _pluginAssemblies[pluginName] = assembly;
                _loadedPlugins[pluginName] = plugin;
                
                Console.WriteLine($"[CLIENT PLUGIN MANAGER] Successfully loaded plugin: {plugin.Name} v{plugin.Version}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CLIENT PLUGIN MANAGER] Unexpected error loading plugin {pluginName}: {ex.Message}");
                return false;
            }
        }        
        
        /// <summary>
        /// Executes a plugin with the provided input, with enhanced error handling.
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
                    Console.WriteLine($"[CLIENT PLUGIN MANAGER] Executing plugin: {pluginName}");
                    var result = plugin.Execute(input);
                    Console.WriteLine($"[CLIENT PLUGIN MANAGER] Plugin {pluginName} executed successfully");
                    return result;
                }
                catch (PluginExecutionException pex)
                {
                    Console.WriteLine($"[CLIENT PLUGIN MANAGER] Plugin execution error in {pluginName}: {pex.Message}");
                    return null;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[CLIENT PLUGIN MANAGER] Unexpected error executing plugin {pluginName}: {ex.Message}");
                    Console.WriteLine($"[CLIENT PLUGIN MANAGER] Stack trace: {ex.StackTrace}");
                    return null;
                }
            }
            else
            {
                Console.WriteLine($"[CLIENT PLUGIN MANAGER] Plugin not found: {pluginName}");
                return null;
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
