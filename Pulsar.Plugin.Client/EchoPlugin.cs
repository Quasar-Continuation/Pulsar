using Pulsar.Plugin.Common;
using System;
using System.IO;
using System.Text;

namespace Pulsar.Plugin.Client
{
    /// <summary>
    /// Simple demo client plugin that echoes input with a prefix.
    /// </summary>
    public class EchoPlugin : IClientPlugin
    {
        public string Name => "Echo";
        public string Version => "1.0.0";

        public void Initialize()
        {
            File.WriteAllText("EchoPlugin.log", $"EchoPlugin initialized at {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n");
        }

        public byte[] Execute(byte[] input)
        {
            try
            {
                var inputString = Encoding.UTF8.GetString(input);
                var output = $"[ECHO PLUGIN] Received: {inputString} at {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                return Encoding.UTF8.GetBytes(output);
            }
            catch (Exception ex)
            {
                var error = $"[ECHO PLUGIN ERROR] {ex.Message}";
                return Encoding.UTF8.GetBytes(error);
            }
        }

        public void Cleanup()
        {
            Console.WriteLine("EchoPlugin cleaned up");
        }
    }
}
