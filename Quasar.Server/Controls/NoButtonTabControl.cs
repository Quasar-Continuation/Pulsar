<<<<<<< HEAD
﻿using System;
using System.Windows.Forms;

namespace Quasar.Server.Controls
{
    public class NoButtonTabControl : TabControl
    {
        protected override void WndProc(ref Message m)
        {
            // Message 0x1328 is related to tab header drawing, we suppress it here.
            if (m.Msg == 0x1328 && !DesignMode)
            {
                // Suppress the header (tab) drawing
                m.Result = (IntPtr)1;
            }
            else
            {
                // Process other messages as usual
                base.WndProc(ref m);
            }
        }
    }
=======
﻿using System;
using System.Windows.Forms;

namespace Quasar.Server.Controls
{
    public class NoButtonTabControl : TabControl
    {
        protected override void WndProc(ref Message m)
        {
            // Message 0x1328 is related to tab header drawing, we suppress it here.
            if (m.Msg == 0x1328 && !DesignMode)
            {
                // Suppress the header (tab) drawing
                m.Result = (IntPtr)1;
            }
            else
            {
                // Process other messages as usual
                base.WndProc(ref m);
            }
        }
    }
>>>>>>> d1562c487ffbb93b7d062a71485785771a87ce11
}