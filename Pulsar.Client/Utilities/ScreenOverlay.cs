using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace Pulsar.Client.Utilities
{
    /// <summary>
    /// This class provides functions for creating drawing overlays on the screen.
    /// 
    /// Usage:
    /// 1. Create an instance of ScreenOverlay
    ///    ScreenOverlay overlay = new ScreenOverlay();
    /// 
    /// 2. Draw lines on the screen:
    ///    // Draw a line from the coordinates (100, 100) to (200, 200) with 5px width and red color on monitor 0
    ///    overlay.Draw(100, 100, 200, 200, 5, Color.Red.ToArgb(), 0);
    ///    
    ///    // Previous coordinates are used to create connected lines
    ///    // This creates a line from the last point
    ///    overlay.Draw(200, 200, 300, 150, 5, Color.Red.ToArgb(), 0);
    /// 
    /// 3. Erase parts of the drawing:
    ///    // Erase area from (150, 150) to (250, 250) with 20px eraser on monitor 0
    ///    overlay.DrawEraser(150, 150, 250, 250, 20, 0);
    /// 
    /// 4. Clear all drawings on a monitor:
    ///    overlay.ClearDrawings(0);
    /// 
    /// 5. Properly dispose when done:
    ///    overlay.Dispose();
    /// 
    /// All drawing operations are performed asynchronously on a background thread for performance.
    /// The overlay automatically creates transparent windows that appear on top of everything else except the cursor,
    /// making it suitable for drawing visual elements on any part of the screen.
    /// 
    /// Monitor indices correspond to the indices in Screen.AllScreens[], use 0 for primary monitor.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct WNDCLASS
    {
        public uint style;
        public WndProcDelegate lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public IntPtr hInstance;
        public IntPtr hIcon;
        public IntPtr hCursor;
        public IntPtr hbrBackground;
        [MarshalAs(UnmanagedType.LPStr)]
        public string lpszMenuName;
        [MarshalAs(UnmanagedType.LPStr)]
        public string lpszClassName;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BLENDFUNCTION
    {
        public byte BlendOp;
        public byte BlendFlags;
        public byte SourceConstantAlpha;
        public byte AlphaFormat;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SIZE
    {
        public int cx;
        public int cy;
    }

    public delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    /// <summary>
    /// For creating transparent overlays on the screen
    /// </summary>
    public class ScreenOverlay : IDisposable
    {
        // handles for monitors
        private Dictionary<int, IntPtr> _overlayWindows = new Dictionary<int, IntPtr>();
        private Dictionary<int, IntPtr> _overlayBitmaps = new Dictionary<int, IntPtr>();
        private Dictionary<int, Bitmap> _overlayCache = new Dictionary<int, Bitmap>();
        
        // drawing layers for monitors
        private Dictionary<int, Bitmap> _drawingLayers = new Dictionary<int, Bitmap>();
        private readonly object _drawingLock = new object();
        
        // delegate for handling window messages
        private WndProcDelegate _wndProcDelegate;
        private bool _delegateInitialized = false;
        
        // threading-related
        private ConcurrentQueue<Action> _drawingQueue = new ConcurrentQueue<Action>();
        private ManualResetEvent _drawingSignal = new ManualResetEvent(false);
        private bool _drawingThreadRunning = false;
        private Thread _drawingThread;
        
        // performance
        private int _pendingDrawingUpdates = 0;
        private const int MAX_UPDATE_RATE_MS = 16; // 60 fps
        private bool _needsUpdate = false;

        /// <summary>
        /// Creates a new ScreenOverlay instance
        /// </summary>
        public ScreenOverlay()
        {
            drawThreadRunning();
        }

        /// <summary>
        /// Checks if drawing thread is running
        /// </summary>
        private void drawThreadRunning()
        {
            if (!_drawingThreadRunning)
            {
                lock (_drawingLock)
                {
                    if (!_drawingThreadRunning)
                    {
                        _drawingThreadRunning = true;
                        _drawingThread = new Thread(DrawingThreadProc);
                        _drawingThread.IsBackground = true;
                        _drawingThread.Start();
                    }
                }
            }
        }

        /// <summary>
        /// Thread proc for handling drawing operations asynchronously
        /// </summary>
        private void DrawingThreadProc()
        {
            try
            {
                Stopwatch updateTimer = new Stopwatch();
                updateTimer.Start();
                
                while (_drawingThreadRunning)
                {
                    try
                    {
                        _drawingSignal.WaitOne(5);
                        
                        bool didWork = false;
                        int maxOperationsPerBatch = 20;
                        int processedOps = 0;
                        
                        while (processedOps < maxOperationsPerBatch && _drawingQueue.TryDequeue(out Action drawAction))
                        {
                            try
                            {
                                drawAction();
                                didWork = true;
                                processedOps++;
                            }
                            catch (Exception)
                            {

                            }
                        }
                        
                        long elapsedMs = updateTimer.ElapsedMilliseconds;
                        if (_needsUpdate && elapsedMs >= MAX_UPDATE_RATE_MS)
                        {
                            updateTimer.Restart();
                            _needsUpdate = false;
                            
                            UpdateActiveOverlays();
                        }
                        
                        if (!didWork && !_needsUpdate)
                        {
                            Thread.Sleep(1);
                        }
                        
                        if (_drawingQueue.IsEmpty)
                        {
                            _drawingSignal.Reset();
                        }
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(100);
                    }
                }
            }
            catch (Exception)
            {
                _drawingThreadRunning = false;
            }
        }

        /// <summary>
        /// Queues a drawing operation to be async executed
        /// </summary>
        public void QueueDrawingAction(Action drawAction)
        {
            if (drawAction == null)
                return;
                
            drawThreadRunning();
            _drawingQueue.Enqueue(drawAction);
            _drawingSignal.Set();
        }
        
        /// <summary>
        /// Draws a line on a monitor
        /// </summary>
        /// <param name="prevX">Previous X coordinate</param>
        /// <param name="prevY">Previous Y coordinate</param>
        /// <param name="x">Current X coordinate</param>
        /// <param name="y">Current Y coordinate</param>
        /// <param name="strokeWidth">Width of the stroke</param>
        /// <param name="colorArgb">ARGB color value</param>
        /// <param name="monitorIndex">Monitor index to draw on</param>
        public void Draw(int prevX, int prevY, int x, int y, int strokeWidth, int colorArgb, int monitorIndex)
        {
            QueueDrawingAction(() => DrawLine(monitorIndex, prevX, prevY, x, y, colorArgb, strokeWidth));
        }
        
        /// <summary>
        /// Erases at the specified coordinates on a monitor
        /// </summary>
        /// <param name="prevX">Previous X coordinate</param>
        /// <param name="prevY">Previous Y coordinate</param>
        /// <param name="x">Current X coordinate</param>
        /// <param name="y">Current Y coordinate</param>
        /// <param name="strokeWidth">Width of the eraser</param>
        /// <param name="monitorIndex">Monitor index to erase on</param>
        public void DrawEraser(int prevX, int prevY, int x, int y, int strokeWidth, int monitorIndex)
        {
            QueueDrawingAction(() => Erase(monitorIndex, prevX, prevY, x, y, strokeWidth));
        }
        
        /// <summary>
        /// Draws a line on a monitor
        /// </summary>
        /// <param name="monitorIndex">Monitor index</param>
        /// <param name="prevX">Previous X coordinate</param>
        /// <param name="prevY">Previous Y coordinate</param>
        /// <param name="x">Current X coordinate</param>
        /// <param name="y">Current Y coordinate</param>
        /// <param name="colorArgb">Color in ARGB format</param>
        /// <param name="strokeWidth">Width of the line</param>
        public void DrawLine(int monitorIndex, int prevX, int prevY, int x, int y, int colorArgb, float strokeWidth)
        {
            QueueDrawingAction(() =>
            {
                try
                {
                    Rectangle bounds;
                    try
                    {
                        bounds = Screen.AllScreens[monitorIndex].Bounds;
                    }
                    catch (IndexOutOfRangeException)
                    {
                        return;
                    }

                    lock (_drawingLock)
                    {
                        if (!_drawingLayers.TryGetValue(monitorIndex, out Bitmap drawingLayer))
                        {
                            drawingLayer = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);
                            using (Graphics g = Graphics.FromImage(drawingLayer))
                            {
                                g.Clear(Color.Transparent);
                            }
                            _drawingLayers[monitorIndex] = drawingLayer;
                        }

                        if (prevX < 0 || prevY < 0 || x < 0 || y < 0 ||
                            prevX >= bounds.Width || prevY >= bounds.Height ||
                            x >= bounds.Width || y >= bounds.Height)
                        {
                            return;
                        }

                        using (Graphics g = Graphics.FromImage(drawingLayer))
                        {
                            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                            using (Pen pen = new Pen(Color.FromArgb(colorArgb), strokeWidth))
                            {

                                pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                                pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                                
                                g.DrawLine(pen, prevX, prevY, x, y);
                            }
                        }
                        
                        _needsUpdate = true;
                    }
                }
                catch (Exception)
                {

                }
            });
        }
        
        /// <summary>
        /// Erases in the specified area on a monitor
        /// </summary>
        public void Erase(int monitorIndex, int prevX, int prevY, int x, int y, float strokeWidth)
        {
            QueueDrawingAction(() =>
            {
                try
                {
                    Rectangle bounds;
                    try
                    {
                        bounds = Screen.AllScreens[monitorIndex].Bounds;
                    }
                    catch (IndexOutOfRangeException)
                    {
                        return;
                    }

                    lock (_drawingLock)
                    {
                        if (!_drawingLayers.TryGetValue(monitorIndex, out Bitmap drawingLayer))
                        {
                            drawingLayer = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);
                            using (Graphics g = Graphics.FromImage(drawingLayer))
                            {
                                g.Clear(Color.Transparent);
                            }
                            _drawingLayers[monitorIndex] = drawingLayer;
                        }

                        if (prevX < 0 || prevY < 0 || x < 0 || y < 0 ||
                            prevX >= bounds.Width || prevY >= bounds.Height ||
                            x >= bounds.Width || y >= bounds.Height)
                        {
                            return;
                        }

                        using (Graphics g = Graphics.FromImage(drawingLayer))
                        {
                            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                            
                            using (SolidBrush transparentBrush = new SolidBrush(Color.Transparent))
                            {
                                int width = (int)Math.Ceiling(strokeWidth);
                                int rectX = Math.Min(prevX, x) - width/2;
                                int rectY = Math.Min(prevY, y) - width/2;
                                int rectW = Math.Abs(x - prevX) + width;
                                int rectH = Math.Abs(y - prevY) + width;
                                
                                if (rectW < width) rectW = width;
                                if (rectH < width) rectH = width;
                                
                                g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                                g.FillRectangle(transparentBrush, rectX, rectY, rectW, rectH);
                                g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
                            }
                        }
                        
                        _needsUpdate = true;
                    }
                }
                catch (Exception)
                {

                }
            });
        }
        
        /// <summary>
        /// Clears all drawings on a monitor
        /// </summary>
        public void ClearDrawings(int monitorIndex)
        {
            QueueDrawingAction(() =>
            {
                try
                {
                    lock (_drawingLock)
                    {
                        if (!_drawingLayers.TryGetValue(monitorIndex, out Bitmap drawingLayer))
                            return;
                            
                        using (Graphics g = Graphics.FromImage(drawingLayer))
                        {
                            g.Clear(Color.Transparent);
                        }
                        
                        _needsUpdate = true;
                    }
                }
                catch (Exception)
                {

                }
            });
        }
        
        /// <summary>
        /// Gets a copy of the current drawing layer for a monitor
        /// </summary>
        public Bitmap GetDrawingLayer(int monitorIndex)
        {
            lock (_drawingLock)
            {
                if (!_drawingLayers.TryGetValue(monitorIndex, out Bitmap drawingLayer))
                    return null;
                    
                return new Bitmap(drawingLayer);
            }
        }

        /// <summary>
        /// Update all active overlays
        /// </summary>
        private void UpdateActiveOverlays()
        {
            HashSet<int> monitorsToUpdate = new HashSet<int>();
            
            lock (_drawingLock)
            {
                foreach (var monitorIndex in _drawingLayers.Keys)
                {
                    monitorsToUpdate.Add(monitorIndex);
                }
            }

            foreach (var monitorIndex in monitorsToUpdate)
            {
                try
                {
                    ShowDrawingOverlay(monitorIndex);
                }
                catch (Exception)
                {

                }
            }
        }

        /// <summary>
        /// Shows the drawing overlay on a monitor
        /// </summary>
        private void ShowDrawingOverlay(int monitorIndex)
        {
            IntPtr hdcWindow = IntPtr.Zero;
            IntPtr hdcMemory = IntPtr.Zero;
            IntPtr hBitmap = IntPtr.Zero;
            IntPtr oldBitmap = IntPtr.Zero;
            Bitmap drawingLayerCopy = null;
            
            try
            {
                lock (_drawingLock)
                {
                    if (!_drawingLayers.TryGetValue(monitorIndex, out Bitmap drawingLayer))
                        return;
                    
                    drawingLayerCopy = new Bitmap(drawingLayer);
                }

                var bounds = Screen.AllScreens[monitorIndex].Bounds;

                if (!_delegateInitialized)
                {
                    _wndProcDelegate = DXOverlayWndProc;
                    _delegateInitialized = true;
                }

                if (!_overlayWindows.TryGetValue(monitorIndex, out IntPtr hwnd) || hwnd == IntPtr.Zero)
                {
                    string className = monitorIndex.ToString();
                    
                    WNDCLASS wndClass = new WNDCLASS();
                    wndClass.lpfnWndProc = _wndProcDelegate;
                    wndClass.hInstance = GdiNativeMethods.GetModuleHandle(null);
                    wndClass.lpszClassName = className;
                    wndClass.hbrBackground = GdiNativeMethods.GetStockObject(GdiNativeMethods.NULL_BRUSH);
                    
                    try
                    {
                        GdiNativeMethods.RegisterClass(ref wndClass);
                    }
                    catch (Exception)
                    {
                    }
                    
                    hwnd = GdiNativeMethods.CreateWindowEx(
                        GdiNativeMethods.WS_EX_LAYERED | GdiNativeMethods.WS_EX_TOOLWINDOW | GdiNativeMethods.WS_EX_TRANSPARENT,
                        className,
                        "",
                        GdiNativeMethods.WS_POPUP,
                        bounds.X, bounds.Y, bounds.Width, bounds.Height,
                        IntPtr.Zero, IntPtr.Zero, GdiNativeMethods.GetModuleHandle(null), IntPtr.Zero);
                        
                    if (hwnd == IntPtr.Zero)
                    {
                        return;
                    }
                    
                    _overlayWindows[monitorIndex] = hwnd;

                    GdiNativeMethods.ShowWindow(hwnd, GdiNativeMethods.SW_SHOWNA);

                    GdiNativeMethods.SetWindowPos(
                        hwnd, 
                        GdiNativeMethods.HWND_TOPMOST,
                        bounds.X, bounds.Y, bounds.Width, bounds.Height,
                        (uint)(GdiNativeMethods.SWP_NOACTIVATE | GdiNativeMethods.SWP_SHOWWINDOW));
                        
                    IntPtr taskbarHwnd = GdiNativeMethods.FindWindow("Shell_TrayWnd", null);
                    if (taskbarHwnd != IntPtr.Zero)
                    {
                        GdiNativeMethods.SetWindowPos(
                            taskbarHwnd,
                            GdiNativeMethods.HWND_TOPMOST,
                            0, 0, 0, 0,
                            GdiNativeMethods.SWP_NOMOVE | GdiNativeMethods.SWP_NOSIZE | GdiNativeMethods.SWP_NOACTIVATE);
                            
                        GdiNativeMethods.SetWindowPos(
                            taskbarHwnd,
                            GdiNativeMethods.HWND_NOTOPMOST,
                            0, 0, 0, 0,
                            GdiNativeMethods.SWP_NOMOVE | GdiNativeMethods.SWP_NOSIZE | GdiNativeMethods.SWP_NOACTIVATE);
                            
                        GdiNativeMethods.SetWindowPos(
                            taskbarHwnd,
                            GdiNativeMethods.HWND_TOPMOST,
                            0, 0, 0, 0,
                            GdiNativeMethods.SWP_NOMOVE | GdiNativeMethods.SWP_NOSIZE | GdiNativeMethods.SWP_NOACTIVATE);
                    }
                }
                
                hdcWindow = GdiNativeMethods.GetDC(hwnd);
                if (hdcWindow == IntPtr.Zero)
                {
                    return;
                }
                
                hdcMemory = GdiNativeMethods.CreateCompatibleDC(hdcWindow);
                if (hdcMemory == IntPtr.Zero)
                {
                    GdiNativeMethods.ReleaseDC(hwnd, hdcWindow);
                    hdcWindow = IntPtr.Zero;
                    return;
                }
                
                bool createNewBitmap = false;
                if (!_overlayBitmaps.TryGetValue(monitorIndex, out hBitmap) || hBitmap == IntPtr.Zero)
                {
                    createNewBitmap = true;
                }
                else
                {
                    if (!_overlayCache.TryGetValue(monitorIndex, out Bitmap cachedBitmap) ||
                        cachedBitmap == null || 
                        cachedBitmap.Width != bounds.Width ||
                        cachedBitmap.Height != bounds.Height)
                    {
                        createNewBitmap = true;
                        
                        if (hBitmap != IntPtr.Zero)
                        {
                            GdiNativeMethods.DeleteObject(hBitmap);
                            hBitmap = IntPtr.Zero;
                            _overlayBitmaps.Remove(monitorIndex);
                        }
                        
                        if (cachedBitmap != null)
                        {
                            cachedBitmap.Dispose();
                            _overlayCache.Remove(monitorIndex);
                        }
                    }
                }
                
                if (createNewBitmap)
                {
                    try
                    {
                        Bitmap newBitmap = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);
                        using (Graphics g = Graphics.FromImage(newBitmap))
                        {
                            g.Clear(Color.FromArgb(0, 0, 0, 0));
                        }

                        _overlayCache[monitorIndex] = newBitmap;
                        
                        hBitmap = newBitmap.GetHbitmap(Color.FromArgb(0));
                        if (hBitmap == IntPtr.Zero)
                        {
                            return;
                        }
                        
                        _overlayBitmaps[monitorIndex] = hBitmap;
                    }
                    catch (Exception)
                    {
                        return;
                    }
                }
                else
                {
                    hBitmap = _overlayBitmaps[monitorIndex];
                }

                oldBitmap = GdiNativeMethods.SelectObject(hdcMemory, hBitmap);
                if (oldBitmap == IntPtr.Zero)
                {
                    return;
                }
                
                using (Graphics g = Graphics.FromHdc(hdcMemory))
                {
                    g.Clear(Color.FromArgb(0, 0, 0, 0));
                    g.DrawImage(drawingLayerCopy, 0, 0, bounds.Width, bounds.Height);
                }
                
                BLENDFUNCTION blend = new BLENDFUNCTION();
                blend.BlendOp = GdiNativeMethods.AC_SRC_OVER;
                blend.BlendFlags = 0;
                blend.SourceConstantAlpha = 255;
                blend.AlphaFormat = GdiNativeMethods.AC_SRC_ALPHA;

                POINT ptSrc = new POINT { x = 0, y = 0 };
                POINT ptDst = new POINT { x = bounds.X, y = bounds.Y };
                SIZE size = new SIZE { cx = bounds.Width, cy = bounds.Height };

                bool result = GdiNativeMethods.UpdateLayeredWindow(
                    hwnd,
                    hdcWindow,
                    ref ptDst,
                    ref size,
                    hdcMemory,
                    ref ptSrc,
                    0,
                    ref blend,
                    (uint)GdiNativeMethods.ULW_ALPHA);
                    
                if (!result)
                {
                    int error = Marshal.GetLastWin32Error();
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                try
                {
                    if (oldBitmap != IntPtr.Zero && hdcMemory != IntPtr.Zero)
                    {
                        GdiNativeMethods.SelectObject(hdcMemory, oldBitmap);
                    }
                    
                    if (hdcMemory != IntPtr.Zero)
                    {
                        GdiNativeMethods.DeleteDC(hdcMemory);
                    }
                    
                    if (hdcWindow != IntPtr.Zero && _overlayWindows.TryGetValue(monitorIndex, out IntPtr hwnd))
                    {
                        GdiNativeMethods.ReleaseDC(hwnd, hdcWindow);
                    }

                    drawingLayerCopy?.Dispose();
                }
                catch (Exception)
                {
                }
            }
        }

        private IntPtr DXOverlayWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            switch (msg)
            {
                case GdiNativeMethods.WM_DESTROY:
                    try
                    {
                        var toRemove = new List<int>();
                        
                        foreach (var pair in _overlayWindows)
                        {
                            if (pair.Value == hWnd)
                            {
                                toRemove.Add(pair.Key);
                                
                                if (_overlayBitmaps.TryGetValue(pair.Key, out IntPtr hBitmap) && hBitmap != IntPtr.Zero)
                                {
                                    GdiNativeMethods.DeleteObject(hBitmap);
                                }
                                
                                if (_overlayCache.TryGetValue(pair.Key, out Bitmap bitmap) && bitmap != null)
                                {
                                    bitmap.Dispose();
                                }
                            }
                        }
                        
                        foreach (var key in toRemove)
                        {
                            _overlayWindows.Remove(key);
                            _overlayBitmaps.Remove(key);
                            _overlayCache.Remove(key);
                        }
                    }
                    catch (Exception)
                    {
                    }
                    break;
            }
            
            return GdiNativeMethods.DefWindowProc(hWnd, msg, wParam, lParam);
        }

        /// <summary>
        /// Disposes of all resources used by screen overlay
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _drawingThreadRunning = false;
                _drawingSignal.Set();
                if (_drawingThread != null && _drawingThread.IsAlive)
                {
                    try
                    {
                        _drawingThread.Join(1000);
                    }
                    catch { }
                }
                _drawingSignal.Dispose();
                
                lock (_drawingLock)
                {
                    foreach (var layer in _drawingLayers.Values)
                    {
                        layer?.Dispose();
                    }
                    _drawingLayers.Clear();
                }
                
                List<IntPtr> windowsToDestroy = new List<IntPtr>();
                foreach (var handle in _overlayWindows.Values)
                {
                    if (handle != IntPtr.Zero)
                        windowsToDestroy.Add(handle);
                }
                
                foreach (var handle in windowsToDestroy)
                {
                    GdiNativeMethods.DestroyWindow(handle);
                }
                _overlayWindows.Clear();
                
                foreach (var handle in _overlayBitmaps.Values)
                {
                    if (handle != IntPtr.Zero)
                        GdiNativeMethods.DeleteObject(handle);
                }
                _overlayBitmaps.Clear();
                
                foreach (var bitmap in _overlayCache.Values)
                {
                    bitmap?.Dispose();
                }
                _overlayCache.Clear();
            }
        }
        
        /// <summary>
        /// Native methods for window + drawing operations
        /// </summary>
        public static class GdiNativeMethods
        {
            public const int GWL_EXSTYLE = -20;
            public const int WS_EX_LAYERED = 0x80000;
            public const int WS_EX_TRANSPARENT = 0x20;
            public const int WS_EX_TOPMOST = 0x8;
            public const int WS_EX_TOOLWINDOW = 0x80;
            public const uint WS_POPUP = 0x80000000;
            public const int SWP_NOMOVE = 0x2;
            public const int SWP_NOSIZE = 0x1;
            public const int SWP_NOACTIVATE = 0x10;
            public const int SWP_SHOWWINDOW = 0x40;
            public const int HWND_TOPMOST = -1;
            public const int HWND_NOTOPMOST = -2;
            public const int LWA_COLORKEY = 0x1;
            public const int LWA_ALPHA = 0x2;
            public const int SW_SHOW = 5;
            public const int SW_SHOWNA = 8;
            public const int ULW_COLORKEY = 0x1;
            public const int ULW_ALPHA = 0x2;
            public const int ULW_OPAQUE = 0x4;
            public const byte AC_SRC_OVER = 0;
            public const byte AC_SRC_ALPHA = 1;
            
            public const int BLACK_BRUSH = 4;
            public const int NULL_BRUSH = 5;
            
            public const uint WM_DESTROY = 0x0002;
            public const uint WM_PAINT = 0x000F;
            
            public const int TRANSPARENT = 1;
            public const int OPAQUE = 2;

            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern IntPtr GetModuleHandle(string lpModuleName);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

            [DllImport("user32.dll")]
            public static extern IntPtr GetDC(IntPtr hWnd);

            [DllImport("user32.dll")]
            public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);

            [DllImport("user32.dll")]
            public static extern IntPtr DefWindowProc(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern IntPtr CreateWindowEx(uint dwExStyle, string lpClassName, string lpWindowName, uint dwStyle, int X, int Y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

            [DllImport("user32.dll")]
            public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern bool RegisterClass(ref WNDCLASS lpWndClass);
            
            [DllImport("user32.dll", SetLastError = true)]
            public static extern ushort RegisterClassEx(ref WNDCLASS lpWndClass);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern bool DestroyWindow(IntPtr hWnd);
            
            [DllImport("user32.dll", SetLastError = true)]
            public static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

            [DllImport("gdi32.dll", SetLastError = true)]
            public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

            [DllImport("gdi32.dll", SetLastError = true)]
            public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hObject);

            [DllImport("gdi32.dll", SetLastError = true)]
            public static extern IntPtr DeleteObject(IntPtr hObject);
            
            [DllImport("gdi32.dll", SetLastError = true)]
            public static extern bool DeleteDC(IntPtr hdc);
            
            [DllImport("gdi32.dll", SetLastError = true)]
            public static extern IntPtr GetStockObject(int fnObject);
            
            [DllImport("gdi32.dll", SetLastError = true)]
            public static extern int SetBkMode(IntPtr hdc, int iBkMode);
            
            [DllImport("gdi32.dll", SetLastError = true)]
            public static extern uint SetTextColor(IntPtr hdc, uint crColor);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, ref POINT pptDst, ref SIZE psize, IntPtr hdcSrc, ref POINT pptSrc, uint crKey, ref BLENDFUNCTION pblend, uint dwFlags);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        }
    }
} 