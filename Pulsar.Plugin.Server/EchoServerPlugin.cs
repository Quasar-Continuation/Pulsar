using Pulsar.Plugin.Common;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;

namespace Pulsar.Plugin.Server
{
    /// <summary>
    /// Simple demo server plugin that handles echo responses.
    /// </summary>
    public class EchoServerPlugin : IServerPlugin
    {
        private readonly ConcurrentDictionary<string, string> _clientResponses;
        private readonly string _logDirectory;

        public string Name => "EchoServer";
        public string Version => "1.0.0";

        public EchoServerPlugin()
        {
            _clientResponses = new ConcurrentDictionary<string, string>();
            _logDirectory = Path.Combine(Environment.CurrentDirectory, "PluginLogs");
            Directory.CreateDirectory(_logDirectory);
        }

        public void Initialize()
        {
            LogMessage("EchoServerPlugin initialized");
        }

        public void ProcessResponse(string clientId, string workId, byte[] response)
        {
            try
            {
                var responseString = Encoding.UTF8.GetString(response);
                _clientResponses[clientId] = responseString;

                var logMessage = $"Response from {clientId} (Work ID: {workId}): {responseString}";
                LogMessage(logMessage);
                
                Console.WriteLine($"[ECHO SERVER] {logMessage}");
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
                var errorString = Encoding.UTF8.GetString(error);
                var logMessage = $"Error from client {clientId} (Work ID: {workId}): {errorString}";
                LogMessage(logMessage);
                
                Console.WriteLine($"[ECHO SERVER ERROR] {logMessage}");
            }
            catch (Exception ex)
            {
                LogMessage($"Error processing error message from {clientId}: {ex.Message}");
            }
        }

        private void LogMessage(string message)
        {
            try
            {
                var logFile = Path.Combine(_logDirectory, $"echo_plugin_{DateTime.Now:yyyyMMdd}.log");
                var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n";
                File.AppendAllText(logFile, logEntry);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to write log: {ex.Message}");
            }
        }

        public void Cleanup()
        {
            LogMessage("EchoServerPlugin cleaned up");
        }
    }
}
