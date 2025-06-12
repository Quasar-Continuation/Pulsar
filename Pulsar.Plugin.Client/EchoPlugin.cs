using Pulsar.Plugin.Common;
using System;
using System.IO;
using System.Text;
using System.Threading;

namespace Pulsar.Plugin.Client
{
    /// <summary>
    /// Ping-pong client plugin that creates an infinite ping-pong loop with the server.
    /// </summary>
    public class EchoPlugin : IClientPlugin
    {
        public string Name => "Pulsar.Plugin.Client";

        public string Version => "1.0.0";
        
        public void Initialize()
        {
            var logMessage = $"PingPong Client Plugin initialized at {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            File.WriteAllText("PingPongPlugin.log", logMessage + "\n");
            Console.WriteLine($"[PINGPONG CLIENT] {logMessage}");
        }

        public byte[] Execute(byte[] input)
        {
            try
            {
                var inputString = Encoding.UTF8.GetString(input);
                var logMessage = $"Client received: '{inputString}' (Length: {input.Length})";
                File.AppendAllText("PingPongPlugin.log", $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - {logMessage}\n");
                Console.WriteLine($"[PINGPONG CLIENT] {logMessage}");

                if (inputString.ToUpper().Contains("START") || inputString.ToUpper().Contains("HELLO"))
                {
                    var startMessage = "PING_PONG_START";
                    var responseLog = $"Client sending start request: {startMessage}";
                    File.AppendAllText("PingPongPlugin.log", $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - {responseLog}\n");
                    Console.WriteLine($"[PINGPONG CLIENT] {responseLog}");
                    return Encoding.UTF8.GetBytes(startMessage);
                }

                if (TryParsePingPongMessage(inputString, out string messageType, out int counter))
                {
                    string responseType;
                    int responseCounter = counter + 1;

                    if (messageType == "PING")
                    {
                        responseType = "PONG";
                    }
                    else if (messageType == "PONG")
                    {
                        responseType = "PING";
                    }
                    else
                    {
                        responseType = "PING";
                        responseCounter = 1;
                    }
                    Thread.Sleep(500);

                    var response = $"{responseType} {responseCounter}";

                    var responseLog = $"Client sending: {response}";
                    File.AppendAllText("PingPongPlugin.log", $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - {responseLog}\n");
                    Console.WriteLine($"[PINGPONG CLIENT] {responseLog}");

                    return Encoding.UTF8.GetBytes(response);
                }
                else
                {
                    var startMessage = "PING_PONG_START";
                    var responseLog = $"Client sending start request for unknown input: {startMessage}";
                    File.AppendAllText("PingPongPlugin.log", $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - {responseLog}\n");
                    Console.WriteLine($"[PINGPONG CLIENT] {responseLog}");
                    return Encoding.UTF8.GetBytes(startMessage);
                }
            }
            catch (Exception ex)
            {
                var error = $"CLIENT_ERROR: {ex.Message}";
                var errorLog = $"Client error: {error}";
                File.AppendAllText("PingPongPlugin.log", $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - {errorLog}\n");
                Console.WriteLine($"[PINGPONG CLIENT ERROR] {errorLog}");
                return Encoding.UTF8.GetBytes(error);
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

        public void Cleanup()
        {
            File.AppendAllText("PingPongPlugin.log", $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - PingPong Client Plugin cleaned up\n");
            Console.WriteLine("PingPong Client Plugin cleaned up");
        }
    }
}