using System;

namespace Pulsar.Plugin.Common.Exceptions
{
    /// <summary>
    /// Base exception class for all plugin-related exceptions.
    /// </summary>
    public class PluginException : Exception
    {
        /// <summary>
        /// Gets the name of the plugin that caused the exception.
        /// </summary>
        public string PluginName { get; }

        /// <summary>
        /// Initializes a new instance of the PluginException class.
        /// </summary>
        /// <param name="pluginName">The name of the plugin that caused the exception.</param>
        /// <param name="message">The exception message.</param>
        public PluginException(string pluginName, string message) : base(message)
        {
            PluginName = pluginName;
        }

        /// <summary>
        /// Initializes a new instance of the PluginException class.
        /// </summary>
        /// <param name="pluginName">The name of the plugin that caused the exception.</param>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The inner exception.</param>
        public PluginException(string pluginName, string message, Exception innerException) : base(message, innerException)
        {
            PluginName = pluginName;
        }
    }

    /// <summary>
    /// Exception thrown when a plugin fails to load.
    /// </summary>
    public class PluginLoadException : PluginException
    {
        /// <summary>
        /// Initializes a new instance of the PluginLoadException class.
        /// </summary>
        /// <param name="pluginName">The name of the plugin that failed to load.</param>
        /// <param name="message">The exception message.</param>
        public PluginLoadException(string pluginName, string message) : base(pluginName, message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the PluginLoadException class.
        /// </summary>
        /// <param name="pluginName">The name of the plugin that failed to load.</param>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The inner exception.</param>
        public PluginLoadException(string pluginName, string message, Exception innerException) : base(pluginName, message, innerException)
        {
        }
    }

    /// <summary>
    /// Exception thrown when a plugin fails to execute.
    /// </summary>
    public class PluginExecutionException : PluginException
    {
        /// <summary>
        /// Gets the work ID associated with the failed execution.
        /// </summary>
        public string WorkId { get; }

        /// <summary>
        /// Initializes a new instance of the PluginExecutionException class.
        /// </summary>
        /// <param name="pluginName">The name of the plugin that failed to execute.</param>
        /// <param name="workId">The work ID associated with the failed execution.</param>
        /// <param name="message">The exception message.</param>
        public PluginExecutionException(string pluginName, string workId, string message) : base(pluginName, message)
        {
            WorkId = workId;
        }

        /// <summary>
        /// Initializes a new instance of the PluginExecutionException class.
        /// </summary>
        /// <param name="pluginName">The name of the plugin that failed to execute.</param>
        /// <param name="workId">The work ID associated with the failed execution.</param>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The inner exception.</param>
        public PluginExecutionException(string pluginName, string workId, string message, Exception innerException) : base(pluginName, message, innerException)
        {
            WorkId = workId;
        }
    }

    /// <summary>
    /// Exception thrown when a plugin validation fails.
    /// </summary>
    public class PluginValidationException : PluginException
    {
        /// <summary>
        /// Initializes a new instance of the PluginValidationException class.
        /// </summary>
        /// <param name="pluginName">The name of the plugin that failed validation.</param>
        /// <param name="message">The exception message.</param>
        public PluginValidationException(string pluginName, string message) : base(pluginName, message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the PluginValidationException class.
        /// </summary>
        /// <param name="pluginName">The name of the plugin that failed validation.</param>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The inner exception.</param>
        public PluginValidationException(string pluginName, string message, Exception innerException) : base(pluginName, message, innerException)
        {
        }
    }
}
