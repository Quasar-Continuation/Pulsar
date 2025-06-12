using Pulsar.Plugin.Common;
using Pulsar.Plugin.Common.Attributes;
using Pulsar.Plugin.Common.Messages;
using Pulsar.Plugin.Common.Exceptions;
using ProtoBuf;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Pulsar.Plugin.Server
{    /// <summary>
    /// Example server plugin that handles system information responses.
    /// This plugin demonstrates how to create a server plugin that processes client data.
    /// </summary>
    [PluginInfo("SystemInfoServer", "1.0.0", "Processes system information responses from clients", "Pulsar Team")]
    public class SystemInfoServerPlugin : IServerPlugin
    {
        private readonly ConcurrentDictionary<string, SystemInfoData> _clientData;
        private readonly string _logDirectory;

        public string Name => "SystemInfoServer";
        public string Version => "1.0.0";

        public SystemInfoServerPlugin()
        {
            _clientData = new ConcurrentDictionary<string, SystemInfoData>();
            _logDirectory = Path.Combine(Environment.CurrentDirectory, "PluginLogs");
            Directory.CreateDirectory(_logDirectory);
        }

        public void Initialize()
        {
            LogMessage("SystemInfoServerPlugin initialized");
        }
        
        public void ProcessResponse(string clientId, string workId, byte[] response)
        {
            try
            {
                SystemInfoResponse systemInfo;
                using (var stream = new MemoryStream(response))
                {
                    systemInfo = Serializer.Deserialize<SystemInfoResponse>(stream);
                }

                var data = new SystemInfoData
                {
                    ClientId = clientId,
                    WorkId = workId,
                    SystemInfo = systemInfo,
                    ReceivedAt = DateTime.UtcNow
                };

                _clientData[clientId] = data;

                LogSystemInfo(data);

                var summary = $"Received system info from {clientId}:\n" +
                             $"Computer: {systemInfo.ComputerName}\n" +
                             $"User: {systemInfo.UserName}\n" +
                             $"OS: {systemInfo.OSVersion}\n" +
                             $"Processes: {systemInfo.RunningProcesses?.Length ?? 0}\n" +
                             $"Software: {systemInfo.InstalledSoftware?.Length ?? 0}";

                MessageBox.Show(summary, "Plugin Response Received", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                LogMessage($"Error processing response from {clientId}: {ex.Message}");
            }
        }
        
        public void ProcessError(string clientId, string workId, byte[] error)
        {
            try
            {
                PluginErrorResponse errorResponse;
                using (var stream = new MemoryStream(error))
                {
                    errorResponse = Serializer.Deserialize<PluginErrorResponse>(stream);
                }

                LogMessage($"Error from client {clientId} (Work ID: {workId}): {errorResponse.Error}");

                MessageBox.Show($"Plugin error from {clientId}:\n{errorResponse.Error}\n\nStack Trace:\n{errorResponse.StackTrace}", 
                              "Plugin Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                try
                {
                    var errorString = Encoding.UTF8.GetString(error);
                    LogMessage($"Error from client {clientId} (Work ID: {workId}): {errorString}");
                    MessageBox.Show($"Plugin error from {clientId}:\n{errorString}", 
                                  "Plugin Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                catch
                {
                    LogMessage($"Error processing error message from {clientId}: {ex.Message}");
                }
            }
        }

        private void LogSystemInfo(SystemInfoData data)
        {
            try
            {
                var logFile = Path.Combine(_logDirectory, $"systeminfo_{data.ClientId}_{DateTime.Now:yyyyMMdd}.log");
                var logEntry = $"[{data.ReceivedAt:yyyy-MM-dd HH:mm:ss}] Work ID: {data.WorkId}\n" +
                              $"Computer: {data.SystemInfo.ComputerName}\n" +
                              $"User: {data.SystemInfo.UserName}\n" +
                              $"OS: {data.SystemInfo.OSVersion}\n" +
                              $"CPU Count: {data.SystemInfo.ProcessorCount}\n" +
                              $"Working Set: {data.SystemInfo.WorkingSet:N0} bytes\n" +
                              $"Uptime: {data.SystemInfo.SystemUptime}\n" +
                              $"Processes Count: {data.SystemInfo.RunningProcesses?.Length ?? 0}\n" +
                              $"Software Count: {data.SystemInfo.InstalledSoftware?.Length ?? 0}\n" +
                              new string('-', 50) + "\n";

                File.AppendAllText(logFile, logEntry);
            }
            catch (Exception ex)
            {
                LogMessage($"Error writing system info log: {ex.Message}");
            }
        }

        private void LogMessage(string message)
        {
            try
            {
                var logFile = Path.Combine(_logDirectory, $"plugin_{DateTime.Now:yyyyMMdd}.log");
                var logEntry = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] {message}\n";
                File.AppendAllText(logFile, logEntry);
            }
            catch
            {
                // Ignore logging errors
            }
        }

        public void Cleanup()
        {
            LogMessage("SystemInfoServerPlugin cleaned up");
        }

        /// <summary>
        /// Gets system information data for a specific client.
        /// </summary>
        /// <param name="clientId">Client ID.</param>
        /// <returns>System info data or null if not found.</returns>
        public SystemInfoData GetClientData(string clientId)
        {
            _clientData.TryGetValue(clientId, out SystemInfoData data);
            return data;
        }
    }
    
    public class SystemInfoData
    {
        public string ClientId { get; set; }
        public string WorkId { get; set; }
        public SystemInfoResponse SystemInfo { get; set; }
        public DateTime ReceivedAt { get; set; }
    }
}
