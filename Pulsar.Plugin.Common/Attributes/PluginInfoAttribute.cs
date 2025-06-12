using System;

namespace Pulsar.Plugin.Common.Attributes
{
    /// <summary>
    /// Attribute to provide metadata about a plugin.
    /// This attribute should be applied to plugin classes to provide discoverable information.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class PluginInfoAttribute : Attribute
    {
        /// <summary>
        /// Gets the plugin name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the plugin version.
        /// </summary>
        public string Version { get; }

        /// <summary>
        /// Gets the plugin description.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the plugin author.
        /// </summary>
        public string Author { get; }

        /// <summary>
        /// Gets the minimum Pulsar version required.
        /// </summary>
        public string MinimumPulsarVersion { get; }

        /// <summary>
        /// Initializes a new instance of the PluginInfoAttribute class.
        /// </summary>
        /// <param name="name">The plugin name.</param>
        /// <param name="version">The plugin version.</param>
        /// <param name="description">The plugin description.</param>
        /// <param name="author">The plugin author.</param>
        /// <param name="minimumPulsarVersion">The minimum Pulsar version required.</param>
        public PluginInfoAttribute(string name, string version, string description = "", string author = "", string minimumPulsarVersion = "1.0.0")
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Version = version ?? throw new ArgumentNullException(nameof(version));
            Description = description ?? "";
            Author = author ?? "";
            MinimumPulsarVersion = minimumPulsarVersion ?? "1.0.0";
        }
    }
}
