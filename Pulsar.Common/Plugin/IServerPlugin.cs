using System;

namespace Pulsar.Common.Plugin
{
    /// <summary>
    /// Interface that all server plugins must implement.
    /// </summary>
    public interface IServerPlugin
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
        /// Processes plugin response from client.
        /// </summary>
        /// <param name="clientId">ID of the client that sent the response.</param>
        /// <param name="workId">Work ID associated with this operation.</param>
        /// <param name="response">Response data from the client.</param>
        void ProcessResponse(string clientId, string workId, byte[] response);

        /// <summary>
        /// Handles plugin errors from client.
        /// </summary>
        /// <param name="clientId">ID of the client that sent the error.</param>
        /// <param name="workId">Work ID associated with this operation.</param>
        /// <param name="error">Error data from the client.</param>
        void ProcessError(string clientId, string workId, byte[] error);

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
