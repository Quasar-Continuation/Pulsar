using Pulsar.Plugin.Common;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;

namespace Pulsar.Plugin.Server
{
    /// <summary>
    /// Ping-pong server plugin that creates an infinite ping-pong loop with clients.
    /// </summary>
    public class EchoServerPlugin : IServerPlugin
    {
        private readonly ConcurrentDictionary<string, bool> _activePingPongs;
        private readonly string _logDirectory;

        public string Name => "Pulsar.Plugin.Server";
        public string Version => "1.0.0";

        public EchoServerPlugin()
        {
            _activePingPongs = new ConcurrentDictionary<string, bool>();
            _logDirectory = Path.Combine(Environment.CurrentDirectory, "PluginLogs");
            Directory.CreateDirectory(_logDirectory);
        }

        public void Initialize()
        {
            LogMessage("PingPongServerPlugin initialized");
        }
        
        public void ProcessResponse(string clientId, string workId, byte[] response)
        {
            Console.WriteLine($"[PING PONG SERVER] ProcessResponse called! ClientId: {clientId}, WorkId: {workId}");
            
            try
            {
                var responseString = Encoding.UTF8.GetString(response);
                LogMessage($"Response from {clientId} (Work ID: {workId}): {responseString}");
                
                Console.WriteLine($"[PING PONG SERVER] Received from {clientId}: {responseString}");

                if (TryParsePingPongMessage(responseString, out string messageType, out int counter))
                {
                    string responseType;
                    if (messageType == "PING")
                    {
                        responseType = "PONG";
                        LogMessage($"Client sent PING, server responding with PONG");
                    }
                    else if (messageType == "PONG")
                    {
                        responseType = "PING";
                        LogMessage($"Client sent PONG, server responding with PING");
                    }
                    else
                    {
                        LogMessage($"Unknown message type from {clientId}: {messageType}");
                        return;
                    }
                    string responseMessage = $"{responseType} {counter + 1}";
                    
                    LogMessage($"Sending response to {clientId}: {responseMessage}");
                    Console.WriteLine($"[PING PONG SERVER] Sending to {clientId}: {responseMessage}");
                    
                    try
                    {
                        var messageBytes = Encoding.UTF8.GetBytes(responseMessage);
                        
                        Console.WriteLine($"[PING PONG SERVER] About to call Common PluginContext.ExecutePlugin for client {clientId}");
                        
                        PluginContext.ExecuteClientPlugin("Pulsar.Plugin.Client", Guid.NewGuid().ToString(), messageBytes);
                        
                        LogMessage($"Successfully sent response to {clientId}: {responseMessage}");
                        Console.WriteLine($"[PING PONG SERVER] Successfully sent response to {clientId}: {responseMessage}");
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"Error sending response to {clientId}: {ex.Message}");
                        Console.WriteLine($"[PING PONG SERVER] Error sending response: {ex.Message}");
                    }
                }                else if (responseString.StartsWith("PING_PONG_START") || responseString.Contains("plugin is ready"))
                {
                    _activePingPongs[clientId] = true;
                    LogMessage($"Starting ping-pong with {clientId}");
                    Console.WriteLine($"[PING PONG SERVER] Starting ping-pong with {clientId}");
                    
                    try
                    {
                        string initialMessage = "PING 1";
                        var messageBytes = Encoding.UTF8.GetBytes(initialMessage);
                        
                        Pulsar.Plugin.Common.PluginContext.ExecuteClientPlugin("Pulsar.Plugin.Client", Guid.NewGuid().ToString(), messageBytes);
                        
                        LogMessage($"Sent initial PING to {clientId}: {initialMessage}");
                        Console.WriteLine($"[PING PONG SERVER] Sent initial PING to {clientId}: {initialMessage}");
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"Error sending initial PING to {clientId}: {ex.Message}");
                        Console.WriteLine($"[PING PONG SERVER] Error sending initial PING: {ex.Message}");
                    }
                }
                else
                {
                    LogMessage($"Non-ping-pong message from {clientId}: {responseString}");
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Error processing response from {clientId}: {ex.Message}");
            }
        }

        private bool TryParsePingPongMessage(string message, out string messageType, out int counter)
        {
            messageType = null;
            counter = 0;

            if (string.IsNullOrWhiteSpace(message))
                return false;

            var parts = message.Trim().Split(' ');
            if (parts.Length != 2)
                return false;

            messageType = parts[0];
            return int.TryParse(parts[1], out counter) && 
                   (messageType == "PING" || messageType == "PONG");
        }

        public void ProcessError(string clientId, string workId, byte[] error)
        {
            try
            {
                var errorString = Encoding.UTF8.GetString(error);
                var logMessage = $"Error from client {clientId} (Work ID: {workId}): {errorString}";
                LogMessage(logMessage);
                
                Console.WriteLine($"[PING PONG SERVER ERROR] {logMessage}");
                
                _activePingPongs.TryRemove(clientId, out _);
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
                var logFile = Path.Combine(_logDirectory, $"pingpong_server_{DateTime.Now:yyyyMMdd}.log");
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
            LogMessage("PingPongServerPlugin cleaned up");
            _activePingPongs.Clear();
        }
    }
}
