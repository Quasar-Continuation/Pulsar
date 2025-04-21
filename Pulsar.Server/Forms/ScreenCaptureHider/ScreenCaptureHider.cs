using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pulsar.Server.Forms.ScreenCaptureHider
{
    public class ScreenCaptureHider
    {
        public static bool FormsHiddenFromScreenCapture = false;

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        static extern bool SetWindowDisplayAffinity(IntPtr hwnd, uint affinity);

        [DllImport("user32.dll")]
        static extern bool GetWindowDisplayAffinity(IntPtr hwnd, out uint affinity);

        public static ulong GetWindowLong(IntPtr hWnd, int nIndex)
        {
            if (IntPtr.Size == 4)
            {
                return (ulong)GetWindowLong32(hWnd, nIndex);
            }
            return (ulong)GetWindowLongPtr64(hWnd, nIndex);
        }


        [DllImport("user32.dll", EntryPoint = "GetWindowLong", CharSet = CharSet.Auto)]
        private static extern IntPtr GetWindowLong32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", CharSet = CharSet.Auto)]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        public static void Refresh()
        {
            uint currentProcessId = (uint)Process.GetCurrentProcess().Id;

            EnumWindows(delegate (IntPtr hWnd, IntPtr lParam) {
                GetWindowThreadProcessId(hWnd, out uint windowPid);
                if (currentProcessId == windowPid) Apply(hWnd);
                return true;
            }, IntPtr.Zero);
        }

        public static void Apply(IntPtr hWnd)
        {
            ulong windowStyle = GetWindowLong(hWnd, -16);
            if ((windowStyle & 0x10000000) != 0 && (windowStyle & 0x00080000) != 0)
            {
                GetWindowDisplayAffinity(hWnd, out uint displayAffinity);
                if ((displayAffinity & 0x00000011) == (FormsHiddenFromScreenCapture ? 0x00000000 : 0x00000011))
                    displayAffinity ^= 0x00000011;
                SetWindowDisplayAffinity(hWnd, displayAffinity);
            }
        }
    }
}