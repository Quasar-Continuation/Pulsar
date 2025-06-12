using Pulsar.Plugin.Common;
using Pulsar.Plugin.Common.Attributes;
using Pulsar.Plugin.Common.Messages;
using Pulsar.Plugin.Common.Exceptions;
using ProtoBuf;
using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Text;

namespace Pulsar.Plugin.Client
{    
    /// <summary>
    /// Example client plugin that gathers system information.
    /// This plugin demonstrates how to create a client plugin that collects system data.
    /// </summary>
    [PluginInfo("SystemInfo", "1.0.0", "Collects system information from the client machine", "Pulsar Team")]
    public class SystemInfoPlugin : IClientPlugin
    {
        public string Name => "SystemInfo";
        public string Version => "1.0.0";        
        
        public void Initialize()
        {
            // Plugin initialization logic
            Console.WriteLine("[SystemInfo Plugin] Initialized successfully");
        }
          public byte[] Execute(byte[] input)
        {
            try
            {
                Console.WriteLine("[SystemInfo Plugin] Starting execution");
                
                SystemInfoRequest request;
                using (var stream = new MemoryStream(input))
                {
                    request = Serializer.Deserialize<SystemInfoRequest>(stream);
                }

                var response = new SystemInfoResponse
                {
                    ComputerName = Environment.MachineName,
                    UserName = Environment.UserName,
                    OSVersion = Environment.OSVersion.ToString(),
                    ProcessorCount = Environment.ProcessorCount,
                    WorkingSet = Environment.WorkingSet,
                    InstalledSoftware = request.IncludeSoftware ? GetInstalledSoftware() : new string[0],
                    RunningProcesses = request.IncludeProcesses ? GetRunningProcesses() : new string[0],
                    SystemUptime = request.IncludeUptime ? GetSystemUptime() : TimeSpan.Zero,
                    Timestamp = DateTime.UtcNow
                };

                using (var stream = new MemoryStream())
                {
                    Serializer.Serialize(stream, response);
                    var result = stream.ToArray();
                    Console.WriteLine($"[SystemInfo Plugin] Execution completed, returning {result.Length} bytes");
                    return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SystemInfo Plugin] Error during execution: {ex.Message}");
                throw new PluginExecutionException(Name, "N/A", $"Failed to gather system information: {ex.Message}", ex);
            }
        }

        private string[] GetInstalledSoftware()
        {
            try
            {
                var software = new System.Collections.Generic.List<string>();
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Product"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        var name = obj["Name"]?.ToString();
                        if (!string.IsNullOrEmpty(name))
                        {
                            software.Add(name);
                        }
                    }
                }
                return software.ToArray();
            }
            catch
            {
                return new[] { "Error retrieving installed software" };
            }
        }

        private string[] GetRunningProcesses()
        {
            try
            {
                var processes = Process.GetProcesses();
                var processNames = new string[processes.Length];
                for (int i = 0; i < processes.Length; i++)
                {
                    processNames[i] = $"{processes[i].ProcessName} (PID: {processes[i].Id})";
                }
                return processNames;
            }
            catch (Exception ex)
            {
                return new[] { $"Error retrieving processes: {ex.Message}" };
            }
        }

        private TimeSpan GetSystemUptime()
        {
            try
            {
                return TimeSpan.FromMilliseconds(Environment.TickCount);
            }
            catch
            {
                return TimeSpan.Zero;
            }
        }
        
        public void Cleanup()
        {
            // Plugin cleanup logic
        }
    }
}
