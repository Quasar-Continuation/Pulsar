using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Quasar.Obfuscator.Utils.Injection
{
    public class StringEncryption
    {
        public static string Encrypt(string input, byte[] key, byte[] iv)
        {
            AesManaged aes = new AesManaged();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            ICryptoTransform encryptor = aes.CreateEncryptor(key, iv);
            byte[] encrypted = encryptor.TransformFinalBlock(Encoding.UTF8.GetBytes(input), 0, input.Length);
            encryptor.Dispose();
            aes.Dispose();
            return Convert.ToBase64String(encrypted);
        }

        public static string Decrypt(string sInput, string key, string iv)
        {
            byte[] input = Convert.FromBase64String(sInput); 
            AesManaged aes = new AesManaged();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            ICryptoTransform decryptor = aes.CreateDecryptor(Convert.FromBase64String(key), Convert.FromBase64String(iv));
            byte[] decrypted = decryptor.TransformFinalBlock(input, 0, input.Length);
            decryptor.Dispose();
            aes.Dispose();
            return Encoding.UTF8.GetString(decrypted);
        }



    }
}
