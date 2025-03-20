﻿using Quasar.Server.DiscordRPC;
using Quasar.Server.Forms;
using System;
using System.Windows.Forms;

namespace Quasar.Server
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using (FrmMain mainForm = new FrmMain())
            {
                DiscordRPCManager.Initialize(mainForm);
                Application.Run(mainForm);
                DiscordRPCManager.Shutdown();
            }
        }
    }
}