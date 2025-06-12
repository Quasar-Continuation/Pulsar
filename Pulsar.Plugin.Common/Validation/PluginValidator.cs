using Pulsar.Plugin.Common.Exceptions;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;

namespace Pulsar.Plugin.Common.Validation
{
    /// <summary>
    /// Provides validation capabilities for plugin assemblies before loading.
    /// </summary>
    public static class PluginValidator
    {
        /// <summary>
        /// Validates a plugin assembly from a file path.
        /// </summary>
        /// <param name="pluginPath">The path to the plugin assembly.</param>
        /// <returns>A validation result indicating success or failure.</returns>
        public static PluginValidationResult ValidatePluginFile(string pluginPath)
        {
            if (string.IsNullOrEmpty(pluginPath))
                return PluginValidationResult.Failure("Plugin path cannot be null or empty");

            if (!File.Exists(pluginPath))
                return PluginValidationResult.Failure($"Plugin file does not exist: {pluginPath}");

            try
            {
                var bytes = File.ReadAllBytes(pluginPath);
                return ValidatePluginBytes(bytes, Path.GetFileNameWithoutExtension(pluginPath));
            }
            catch (Exception ex)
            {
                return PluginValidationResult.Failure($"Failed to read plugin file: {ex.Message}");
            }
        }

        /// <summary>
        /// Validates a plugin assembly from byte array.
        /// </summary>
        /// <param name="pluginBytes">The plugin assembly bytes.</param>
        /// <param name="pluginName">The name of the plugin for error reporting.</param>
        /// <returns>A validation result indicating success or failure.</returns>
        public static PluginValidationResult ValidatePluginBytes(byte[] pluginBytes, string pluginName)
        {
            if (pluginBytes == null || pluginBytes.Length == 0)
                return PluginValidationResult.Failure("Plugin bytes cannot be null or empty");

            try
            {
                if (!IsValidPEFile(pluginBytes))
                    return PluginValidationResult.Failure("File is not a valid PE (Portable Executable) file");

                Assembly assembly;
                try
                {
                    assembly = Assembly.Load(pluginBytes);
                }
                catch (BadImageFormatException)
                {
                    return PluginValidationResult.Failure("File is not a valid .NET assembly");
                }
                catch (Exception ex)
                {
                    return PluginValidationResult.Failure($"Failed to load assembly: {ex.Message}");
                }

                var hasClientPlugin = HasInterface(assembly, typeof(IClientPlugin));
                var hasServerPlugin = HasInterface(assembly, typeof(IServerPlugin));

                if (!hasClientPlugin && !hasServerPlugin)
                {
                    return PluginValidationResult.Failure("Assembly does not contain any valid plugin implementations (IClientPlugin or IServerPlugin)");
                }
                var securityIssues = CheckForDangerousApis(assembly);
                if (securityIssues.Length > 0)
                {
                    return PluginValidationResult.Warning($"Plugin uses potentially dangerous APIs: {string.Join(", ", securityIssues)}");
                }

                return PluginValidationResult.Success();
            }
            catch (Exception ex)
            {
                return PluginValidationResult.Failure($"Validation failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Checks if the byte array represents a valid PE file.
        /// </summary>
        private static bool IsValidPEFile(byte[] bytes)
        {
            if (bytes.Length < 64) return false;

            if (bytes[0] != 0x4D || bytes[1] != 0x5A)
                return false;

            var peOffset = BitConverter.ToInt32(bytes, 60);
            if (peOffset < 0 || peOffset >= bytes.Length - 4)
                return false;

            if (peOffset + 4 > bytes.Length)
                return false;

            return bytes[peOffset] == 0x50 && bytes[peOffset + 1] == 0x45 && 
                   bytes[peOffset + 2] == 0x00 && bytes[peOffset + 3] == 0x00;
        }

        /// <summary>
        /// Checks if the assembly implements the specified interface.
        /// </summary>
        private static bool HasInterface(Assembly assembly, Type interfaceType)
        {
            try
            {
                return assembly.GetTypes()
                    .Any(t => !t.IsInterface && !t.IsAbstract && interfaceType.IsAssignableFrom(t));
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks for usage of potentially dangerous APIs.
        /// </summary>
        private static string[] CheckForDangerousApis(Assembly assembly)
        {
            var dangerousApis = new[]
            {
                "System.IO.File",
                "System.Diagnostics.Process",
                "System.Net.WebClient",
                "System.Net.HttpClient",
                "Microsoft.Win32.Registry"
            };

            var foundApis = new System.Collections.Generic.List<string>();

            try
            {
                var referencedTypes = assembly.GetReferencedAssemblies()
                    .SelectMany(name => 
                    {
                        try
                        {
                            return Assembly.Load(name).GetTypes();
                        }
                        catch
                        {
                            return new Type[0];
                        }
                    })
                    .Select(t => t.FullName)
                    .ToArray();

                foreach (var api in dangerousApis)
                {
                    if (referencedTypes.Any(t => t?.StartsWith(api) == true))
                    {
                        foundApis.Add(api);
                    }
                }
            }
            catch
            {
            }

            return foundApis.ToArray();
        }
    }

    /// <summary>
    /// Represents the result of plugin validation.
    /// </summary>
    public class PluginValidationResult
    {
        /// <summary>
        /// Gets a value indicating whether the validation was successful.
        /// </summary>
        public bool IsValid { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the validation resulted in warnings.
        /// </summary>
        public bool HasWarnings { get; private set; }

        /// <summary>
        /// Gets the validation message.
        /// </summary>
        public string Message { get; private set; }

        private PluginValidationResult(bool isValid, bool hasWarnings, string message)
        {
            IsValid = isValid;
            HasWarnings = hasWarnings;
            Message = message ?? "";
        }

        /// <summary>
        /// Creates a successful validation result.
        /// </summary>
        public static PluginValidationResult Success() => new PluginValidationResult(true, false, "Validation successful");

        /// <summary>
        /// Creates a warning validation result.
        /// </summary>
        public static PluginValidationResult Warning(string message) => new PluginValidationResult(true, true, message);

        /// <summary>
        /// Creates a failed validation result.
        /// </summary>
        public static PluginValidationResult Failure(string message) => new PluginValidationResult(false, false, message);
    }
}
