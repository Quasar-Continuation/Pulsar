using Pulsar.Plugin.Common;
using Pulsar.Plugin.Common.Attributes;
using Pulsar.Plugin.Common.Exceptions;
using ProtoBuf;
using System;
using System.IO;
using System.Text;

namespace Pulsar.Plugin.Test
{
    /// <summary>
    /// Simple request message for ping pong test
    /// </summary>
    [ProtoContract]
    public class PingRequest
    {
        [ProtoMember(1)]
        public string Message { get; set; }
        
        [ProtoMember(2)]
        public DateTime Timestamp { get; set; }
        
        [ProtoMember(3)]
        public int PingNumber { get; set; }
    }

    /// <summary>
    /// Response message for ping pong test
    /// </summary>
    [ProtoContract]
    public class PongResponse
    {
        [ProtoMember(1)]
        public string Message { get; set; }
        
        [ProtoMember(2)]
        public DateTime ClientTimestamp { get; set; }
        
        [ProtoMember(3)]
        public DateTime ResponseTimestamp { get; set; }
        
        [ProtoMember(4)]
        public int PongNumber { get; set; }
        
        [ProtoMember(5)]
        public string ComputerName { get; set; }
    }

    /// <summary>
    /// Simple ping pong client plugin for testing the plugin system.
    /// This plugin receives a ping request and responds with a pong.
    /// </summary>
    [PluginInfo("PingPong", "1.0.0", "Simple ping pong plugin for testing", "Pulsar Team")]
    public class PingPongClientPlugin : IClientPlugin
    {
        public string Name => "PingPong";
        public string Version => "1.0.0";

        public void Initialize()
        {
            Console.WriteLine("[PingPong Client] Plugin initialized successfully");
        }        
        
        public byte[] Execute(byte[] input)
        {
            try
            {
                Console.WriteLine("[PingPong Client] Received ping request");
                
                // Try to handle as string first (for simple START messages)
                var inputString = Encoding.UTF8.GetString(input);
                Console.WriteLine($"[PingPong Client] Input string: '{inputString}' (Length: {input.Length})");
                
                // If it's a simple START message, send back a simple acknowledgment
                if (inputString.Trim().ToUpper() == "START" || inputString.ToUpper().Contains("START"))
                {
                    Console.WriteLine("[PingPong Client] Received START command, sending acknowledgment");
                    var ackMessage = "PingPong plugin is ready";
                    return Encoding.UTF8.GetBytes(ackMessage);
                }
                
                // Check if it's a simple ping/pong message format (e.g., "PING 1", "PONG 2")
                if (TryParseSimplePingPong(inputString, out string messageType, out int counter))
                {
                    Console.WriteLine($"[PingPong Client] Received {messageType} {counter}");
                    
                    // Respond with opposite message type and incremented counter
                    string responseType = messageType == "PING" ? "PONG" : "PING";
                    string pingPongResponse = $"{responseType} {counter + 1}";
                    
                    Console.WriteLine($"[PingPong Client] Sending {pingPongResponse}");
                    return Encoding.UTF8.GetBytes(pingPongResponse);
                }
                
                // Try to deserialize as a protobuf PingRequest
                PingRequest request;
                try
                {
                    using (var stream = new MemoryStream(input))
                    {
                        request = Serializer.Deserialize<PingRequest>(stream);
                    }
                }
                catch (Exception)
                {
                    // If protobuf deserialization fails, treat as a simple string message
                    Console.WriteLine("[PingPong Client] Input is not protobuf, treating as simple string");
                    var simpleResponse = $"Received: {inputString} at {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                    return Encoding.UTF8.GetBytes(simpleResponse);
                }

                Console.WriteLine($"[PingPong Client] Ping message: {request.Message}");
                Console.WriteLine($"[PingPong Client] Ping number: {request.PingNumber}");
                Console.WriteLine($"[PingPong Client] Original timestamp: {request.Timestamp}");

                // Create pong response
                var response = new PongResponse
                {
                    Message = $"Pong! Received: {request.Message}",
                    ClientTimestamp = request.Timestamp,
                    ResponseTimestamp = DateTime.Now,
                    PongNumber = request.PingNumber,
                    ComputerName = Environment.MachineName
                };

                // Serialize the response
                using (var stream = new MemoryStream())
                {
                    Serializer.Serialize(stream, response);
                    var responseBytes = stream.ToArray();
                    
                    Console.WriteLine($"[PingPong Client] Sending pong response ({responseBytes.Length} bytes)");
                    return responseBytes;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PingPong Client] Error: {ex.Message}");
                throw new PluginExecutionException(Name, "unknown", $"Failed to process ping: {ex.Message}", ex);
            }
        }

        private bool TryParseSimplePingPong(string message, out string messageType, out int counter)
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
            Console.WriteLine("[PingPong Client] Plugin cleaned up");
        }
    }
}
