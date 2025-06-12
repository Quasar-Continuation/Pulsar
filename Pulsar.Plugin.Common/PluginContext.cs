using System;
using System.Threading;

namespace Pulsar.Plugin.Common
{
    /// <summary>
    /// Provides context and capabilities for plugin execution.
    /// This class serves as a bridge between plugins and the Pulsar environment.
    /// </summary>
    public static class PluginContext
    {
        private static readonly ThreadLocal<Action<string, string, byte[]>> _currentExecutor = new ThreadLocal<Action<string, string, byte[]>>();        /// <summary>
        /// Sets the executor for the current thread.
        /// This is used by the plugin manager to provide plugin execution capabilities.
        /// </summary>
        /// <param name="executor">The executor delegate.</param>
        public static void SetExecutor(Action<string, string, byte[]> executor)
        {
            _currentExecutor.Value = executor;
        }

        /// <summary>
        /// Executes a plugin on the client with the specified parameters.
        /// This method can be called from server plugins to trigger client plugin execution.
        /// </summary>
        /// <param name="pluginName">The name of the plugin to execute.</param>
        /// <param name="workId">A unique identifier for tracking this operation.</param>
        /// <param name="input">The input data to send to the plugin.</param>
        /// <exception cref="InvalidOperationException">Thrown when no executor is available.</exception>
        /// <exception cref="ArgumentNullException">Thrown when pluginName or workId is null.</exception>
        public static void ExecuteClientPlugin(string pluginName, string workId, byte[] input)
        {
            if (string.IsNullOrEmpty(pluginName))
                throw new ArgumentNullException(nameof(pluginName));

            if (string.IsNullOrEmpty(workId))
                throw new ArgumentNullException(nameof(workId));

            var executor = _currentExecutor.Value;
            if (executor == null)
            {
                throw new InvalidOperationException("No plugin executor available. This method can only be called from within a server plugin context.");
            }

            executor(pluginName, workId, input ?? new byte[0]);
        }

        /// <summary>
        /// Legacy method for backwards compatibility.
        /// Use ExecuteClientPlugin instead.
        /// </summary>
        /// <param name="pluginName">Name of the plugin to execute</param>
        /// <param name="workId">Work ID for tracking</param>
        /// <param name="input">Input data for the plugin</param>
        [Obsolete("Use ExecuteClientPlugin instead")]
        public static void ExecutePlugin(string pluginName, string workId, byte[] input)
        {
            ExecuteClientPlugin(pluginName, workId, input);
        }

        /// <summary>
        /// Gets a value indicating whether a plugin executor is available in the current context.
        /// </summary>
        public static bool IsExecutorAvailable => _currentExecutor.Value != null;
    }
}
