using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Pulsar.Common.Cryptography
{
    public static class AesKeyManager
    {
        private const string KeyFileName = "KEY.txt";
        
        public static string GetKeyFilePath(string baseDirectory = null)
        {
            string directory = baseDirectory ?? AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(directory, KeyFileName);
        }

        public static string GenerateAndSaveKey(string baseDirectory = null)
        {
            string key;
            
#if DEBUG
            key = "1111111111111111111111111111111111111111111111111111111111111111";
#else
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] keyBytes = new byte[32];
                rng.GetBytes(keyBytes);
                key = Convert.ToBase64String(keyBytes);
            }
#endif

            string keyFilePath = GetKeyFilePath(baseDirectory);
            File.WriteAllText(keyFilePath, key, Encoding.UTF8);
            return key;
        }

        public static string LoadKey(string baseDirectory = null)
        {
#if DEBUG
            return "1111111111111111111111111111111111111111111111111111111111111111";
#else
            string keyFilePath = GetKeyFilePath(baseDirectory);
            if (!File.Exists(keyFilePath))
                throw new FileNotFoundException("AES key file not found");
            return File.ReadAllText(keyFilePath, Encoding.UTF8).Trim();
#endif
        }

        public static bool KeyFileExists(string baseDirectory = null)
        {
#if DEBUG
            return true;
#else
            return File.Exists(GetKeyFilePath(baseDirectory));
#endif
        }

        public static string EnsureKeyExists(string baseDirectory = null)
        {
#if DEBUG
            return "1111111111111111111111111111111111111111111111111111111111111111";
#else
            return KeyFileExists(baseDirectory) ? LoadKey(baseDirectory) : GenerateAndSaveKey(baseDirectory);
#endif
        }

        public static Aes256 CreateAes256Instance(string baseDirectory = null)
        {
            string key = EnsureKeyExists(baseDirectory);
            return new Aes256(key);
        }
    }
}
