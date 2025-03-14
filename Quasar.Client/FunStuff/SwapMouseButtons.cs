using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Quasar.Client.FunStuff
{
    public static class SwapMouseButtons
    {
        [DllImport("user32.dll")]
        public static extern bool SwapMouseButton(bool swap);

        [DllImport("user32.dll")]
        public static extern int GetSystemMetrics(int nIndex);

        private const int SM_SWAPBUTTON = 23;

        public static void SwapMouse()
        {
            bool isSwapped = GetSystemMetrics(SM_SWAPBUTTON) != 0;

            SwapMouseButton(!isSwapped);
        }
    }
}
