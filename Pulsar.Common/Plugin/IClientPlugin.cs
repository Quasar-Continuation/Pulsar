using System;

namespace Pulsar.Common.Plugin
{
    /// <summary>
    /// Interface that all client plugins must implement.
    /// </summary>
    public interface IClientPlugin
    {
        /// <summary>
        /// Gets the plugin name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the plugin version.
        /// </summary>
        string Version { get; }

        /// <summary>
        /// Executes the plugin with the provided input data.
        /// </summary>
        /// <param name="input">Input data for the plugin.</param>
        /// <returns>Output data from the plugin execution.</returns>
        byte[] Execute(byte[] input);

        /// <summary>
        /// Initializes the plugin.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Cleans up plugin resources.
        /// </summary>
        void Cleanup();
    }
}
