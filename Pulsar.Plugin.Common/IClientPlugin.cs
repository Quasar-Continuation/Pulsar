using System;

namespace Pulsar.Plugin.Common
{
    /// <summary>
    /// Interface that all client plugins must implement.
    /// Client plugins run on the client side and process requests from server plugins.
    /// </summary>
    public interface IClientPlugin
    {
        /// <summary>
        /// Gets the plugin name. This should be unique and descriptive.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the plugin version using semantic versioning (e.g., "1.0.0").
        /// </summary>
        string Version { get; }

        /// <summary>
        /// Executes the plugin with the provided input data.
        /// This method should be thread-safe and handle errors gracefully.
        /// </summary>
        /// <param name="input">Input data for the plugin. Can be null or empty.</param>
        /// <returns>Output data from the plugin execution. Return null to indicate failure.</returns>
        /// <exception cref="PluginExecutionException">Thrown when plugin execution fails.</exception>
        byte[] Execute(byte[] input);

        /// <summary>
        /// Initializes the plugin. This method is called once when the plugin is loaded.
        /// Use this method to set up any required resources or configuration.
        /// </summary>
        /// <exception cref="PluginLoadException">Thrown when plugin initialization fails.</exception>
        void Initialize();

        /// <summary>
        /// Cleans up plugin resources. This method is called when the plugin is unloaded.
        /// Ensure all resources are properly disposed of in this method.
        /// </summary>
        void Cleanup();
    }
}
