using System;

namespace Pulsar.Plugin.Common
{
    /// <summary>
    /// A simplified PluginContext that provides access to client execution functionality for plugins.
    /// This is a bridge between the server's PluginContext and the plugins.
    /// </summary>
    public static class PluginContext
    {
        private static Action<string, string, byte[]> _executePluginAction;

        /// <summary>
        /// Sets the executor action.
        /// </summary>
        /// <param name="executeAction">The action used to execute plugins</param>
        public static void SetExecutor(Action<string, string, byte[]> executeAction)
        {
            _executePluginAction = executeAction;
        }

        /// <summary>
        /// Executes a plugin with the given parameters.
        /// </summary>
        /// <param name="pluginName">Name of the plugin to execute</param>
        /// <param name="workId">Work ID for tracking</param>
        /// <param name="input">Input data for the plugin</param>
        public static void ExecutePlugin(string pluginName, string workId, byte[] input)
        {
            Console.WriteLine($"[PLUGIN COMMON CONTEXT] ExecutePlugin called - Plugin: {pluginName}, WorkId: {workId}");
            
            if (_executePluginAction != null)
            {
                Console.WriteLine("[PLUGIN COMMON CONTEXT] Executor is available, forwarding call");
                _executePluginAction(pluginName, workId, input);
                Console.WriteLine("[PLUGIN COMMON CONTEXT] Executor call completed");
            }
            else
            {
                Console.WriteLine("[PLUGIN COMMON CONTEXT] No executor available - ping-pong chain broken");
            }
        }
    }
}
