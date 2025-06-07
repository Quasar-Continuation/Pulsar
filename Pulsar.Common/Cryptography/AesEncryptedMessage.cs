using Pulsar.Common.Messages;
using Pulsar.Common.Messages.Other;
using Pulsar.Common.Networking;
using System;
using System.IO;
using System.Security.Cryptography;

namespace Pulsar.Common.Cryptography
{
    /// <summary>
    /// Wraps messages with AES encryption for pseudo End-to-End encryption
    /// </summary>
    [Serializable]
    public class AesEncryptedMessage : IMessage
    {
        /// <summary>
        /// The encrypted message data
        /// </summary>
        public byte[] EncryptedData { get; set; }

        /// <summary>
        /// Timestamp when the message was encrypted (for replay attack prevention)
        /// </summary>
        public long Timestamp { get; set; }

        /// <summary>
        /// Creates an empty encrypted message (for deserialization)
        /// </summary>
        public AesEncryptedMessage()
        {
        }

        /// <summary>
        /// Creates an encrypted message from the original message
        /// </summary>
        /// <param name="originalMessage">The message to encrypt</param>
        /// <param name="aes">The AES encryption instance</param>
        public AesEncryptedMessage(IMessage originalMessage, Aes256 aes)
        {
            if (originalMessage == null)
                throw new ArgumentNullException(nameof(originalMessage));
            if (aes == null)
                throw new ArgumentNullException(nameof(aes));

            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            
            byte[] serializedMessage = SerializeMessage(originalMessage);
            
            EncryptedData = aes.Encrypt(serializedMessage);
        }

        /// <summary>
        /// Decrypts and deserializes the original message
        /// </summary>
        /// <param name="aes">The AES decryption instance</param>
        /// <returns>The original decrypted message</returns>
        public IMessage DecryptMessage(Aes256 aes)
        {
            if (aes == null)
                throw new ArgumentNullException(nameof(aes));
            if (EncryptedData == null)
                throw new InvalidOperationException("No encrypted data available");

            try
            {
                byte[] decryptedData = aes.Decrypt(EncryptedData);
                
                return DeserializeMessage(decryptedData);
            }
            catch (Exception ex)
            {
                throw new CryptographicException($"Failed to decrypt message: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Validates if the message timestamp is within acceptable range (prevents replay attacks)
        /// </summary>
        /// <param name="maxAgeSeconds">Maximum age in seconds (default: 300 = 5 minutes)</param>
        /// <returns>True if timestamp is valid</returns>
        public bool IsTimestampValid(int maxAgeSeconds = 300)
        {
            long currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            long ageMilliseconds = currentTimestamp - Timestamp;
            return ageMilliseconds >= 0 && ageMilliseconds <= (maxAgeSeconds * 1000);
        }

        /// <summary>
        /// Serializes a message to byte array
        /// </summary>
        private byte[] SerializeMessage(IMessage message)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var payloadWriter = new PayloadWriter(memoryStream, false))
                {
                    payloadWriter.WriteMessage(message);
                    return memoryStream.ToArray();
                }
            }
        }

        /// <summary>
        /// Deserializes a message from byte array
        /// </summary>
        private IMessage DeserializeMessage(byte[] data)
        {
            using (var memoryStream = new MemoryStream(data))
            {
                using (var payloadReader = new PayloadReader(data, data.Length, false))
                {
                    return payloadReader.ReadMessage();
                }
            }
        }
    }
}
