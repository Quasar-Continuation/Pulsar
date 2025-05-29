using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pulsar.Common.DNS
{
    public class HostsConverter
    {

        public List<Host> RawHostsToList(string rawHosts, bool server = false)
        {
            List<Host> hostsList = new List<Host>();

            if (string.IsNullOrEmpty(rawHosts)) return hostsList;

            if ((rawHosts.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || 
                 rawHosts.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) &&
                !rawHosts.Contains(";"))
            {
                hostsList.Add(new Host { Hostname = rawHosts });
                return hostsList;
            }

            var hosts = rawHosts.Split(';');

            foreach (var host in hosts)
            {
                if (string.IsNullOrEmpty(host)) continue;

                if (Uri.TryCreate(host, UriKind.Absolute, out Uri uri) &&
                    (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
                {
                    hostsList.Add(new Host { Hostname = host });
                }
                else if (host.Contains(':'))
                {
                    if (ushort.TryParse(host.Split(':').Last(), out ushort port))
                    {
                        hostsList.Add(new Host
                        {
                            Hostname = host.Substring(0, host.LastIndexOf(':')),
                            Port = port
                        });
                    }
                }
                else
                {
                    hostsList.Add(new Host { Hostname = host });
                }
            }

            return hostsList;
        }

        public string ListToRawHosts(IList<Host> hosts)
        {
            StringBuilder rawHosts = new StringBuilder();

            foreach (var host in hosts)
                rawHosts.Append(host + ";");

            return rawHosts.ToString();
        }
    }
}
