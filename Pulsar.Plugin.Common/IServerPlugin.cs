using System;

namespace Pulsar.Plugin.Common
{
    /// <summary>
    /// Interface that all server plugins must implement.
    /// Server plugins run on the server side and handle responses from client plugins.
    /// </summary>
    public interface IServerPlugin
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
        /// Processes plugin response from client.
        /// This method should be thread-safe and handle errors gracefully.
        /// </summary>
        /// <param name="clientId">ID of the client that sent the response.</param>
        /// <param name="workId">Work ID associated with this operation.</param>
        /// <param name="response">Response data from the client. Can be null or empty.</param>
        /// <exception cref="PluginExecutionException">Thrown when response processing fails.</exception>
        void ProcessResponse(string clientId, string workId, byte[] response);

        /// <summary>
        /// Handles plugin errors from client.
        /// This method is called when a client plugin reports an error.
        /// </summary>
        /// <param name="clientId">ID of the client that sent the error.</param>
        /// <param name="workId">Work ID associated with this operation.</param>
        /// <param name="error">Error data from the client. Can be null or empty.</param>
        void ProcessError(string clientId, string workId, byte[] error);

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
