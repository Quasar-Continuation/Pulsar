using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Quasar.Common.DNS
{
    public class HostsConverter
    {
        public List<Host> RawHostsToList(string rawHosts, bool server = false)
        {
            List<Host> hostsList = new List<Host>();

            if (string.IsNullOrEmpty(rawHosts)) return hostsList;

            var hosts = rawHosts.Split(';');

            foreach (var host in hosts)
            {
                if (string.IsNullOrEmpty(host) || !host.Contains(':')) continue; // invalid host, ignore

                if (host.StartsWith("https://pastebin") && !server)
                {
                    var hostWithoutPort = host.Substring(0, host.LastIndexOf(':'));
                    try
                    {
                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(hostWithoutPort);
                        request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:76.0) Gecko/20100101 Firefox/76.0";
                        request.Proxy = null;
                        request.Timeout = 10000;

                        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                        {
                            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                            {
                                string pastebinContent = reader.ReadToEnd().Trim();
                                if (!string.IsNullOrEmpty(pastebinContent) && pastebinContent.Contains(':'))
                                {
                                    if (ushort.TryParse(pastebinContent.Substring(pastebinContent.LastIndexOf(':') + 1), out ushort port))
                                    {
                                        hostsList.Add(new Host { Hostname = pastebinContent.Substring(0, pastebinContent.LastIndexOf(':')), Port = port });
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("Failed to get host from pastebin.");
                        Debug.WriteLine(e);
                        continue; // if there's an error, ignore this host
                    }
                }
                else
                {
                    if (ushort.TryParse(host.Substring(host.LastIndexOf(':') + 1), out ushort port))
                    {
                        hostsList.Add(new Host { Hostname = host.Substring(0, host.LastIndexOf(':')), Port = port });
                    }
                    else if (server && host.StartsWith("https://pastebin"))
                    {
                        hostsList.Add(new Host { Hostname = host.Substring(0, host.LastIndexOf(':')) });
                    }
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
