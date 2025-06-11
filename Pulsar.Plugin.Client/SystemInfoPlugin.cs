using Pulsar.Plugin.Common;
using Pulsar.Plugin.Common.Messages;
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
    /// </summary>
    public class SystemInfoPlugin : IClientPlugin
    {
        public string Name => "SystemInfo";
        public string Version => "1.0.0";

        public void Initialize()
        {
            // Plugin initialization logic
        }
        
        public byte[] Execute(byte[] input)
        {
            try
            {
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
                    return stream.ToArray();
                }
            }
            catch (Exception ex)
            {
                var error = new PluginErrorResponse 
                { 
                    Error = ex.Message, 
                    StackTrace = ex.StackTrace 
                };
                
                using (var stream = new MemoryStream())
                {
                    Serializer.Serialize(stream, error);
                    return stream.ToArray();
                }
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
