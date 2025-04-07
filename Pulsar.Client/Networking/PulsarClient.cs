using Pulsar.Client.Config;
using Pulsar.Client.Helper;
using Pulsar.Client.IO;
using Pulsar.Client.IpGeoLocation;
using Pulsar.Client.User;
using Pulsar.Common.DNS;
using Pulsar.Common.Helpers;
using Pulsar.Common.Messages;
using Pulsar.Common.Messages.other;
using Pulsar.Common.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;



namespace Pulsar.Client.Networking
{
    public class PulsarClient : Client, IDisposable
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

        // Store the last resolved host (IP and Port)
        private Host _lastResolvedHost;

        /// <summary>
        /// Initializes a new instance of the <see cref="PulsarClient"/> class.
        /// </summary>
        /// <param name="hostsManager">The hosts manager which contains the available hosts to connect to.</param>
        /// <param name="serverCertificate">The server certificate.</param>
        public PulsarClient(HostsManager hostsManager, X509Certificate2 serverCertificate)
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

                        if (!Connected)
                        {
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Connection error: {ex.Message}");
                        continue;
                    }
                }

                while (Connected && !_token.IsCancellationRequested)
                {
                    try
                    {
                        _token.WaitHandle.WaitOne(250);
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
            }
        }

        /// <summary>
        /// Keeps only one ResolveHost method with logging capabilities
        /// </summary>
        private Host ResolveHost(string hostString)
        {
            try
            {
                if (Uri.TryCreate(hostString, UriKind.Absolute, out Uri uri))
                {
                    try
                    {
                        WebRequest request = WebRequest.Create(uri);
                        request.Timeout = 5000;
                        using (WebResponse response = request.GetResponse())
                        using (Stream stream = response.GetResponseStream())
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            string content = reader.ReadToEnd().Trim();
                            Debug.WriteLine($"Fetched from URI: '{content}'");

                            if (Uri.TryCreate(content, UriKind.Absolute, out Uri nestedUri))
                            {
                                return ResolveHost(content); 
                            }

                            string[] parts = content.Split(':');
                            if (parts.Length >= 2 &&
                                IPAddress.TryParse(parts[0].Trim(), out IPAddress ip) &&
                                ushort.TryParse(parts[1].Trim(), out ushort port))
                            {
                                Debug.WriteLine($"Parsed IP: {ip}, Port: {port}");
                                _lastResolvedHost = new Host { IpAddress = ip, Port = port };
                                return _lastResolvedHost;
                            }
                            else
                            {
                                Debug.WriteLine($"Failed to parse URI response: '{content}'");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"URI fetch error: {ex.Message}");
                    }
                }
                else
                {
                    string[] parts = hostString.Split(':');
                    if (parts.Length >= 2 &&
                        IPAddress.TryParse(parts[0].Trim(), out IPAddress ip) &&
                        ushort.TryParse(parts[1].Trim(), out ushort port))
                    {
                        Debug.WriteLine($"Parsed direct IP: {ip}, Port: {port}");
                        _lastResolvedHost = new Host { IpAddress = ip, Port = port };
                        return _lastResolvedHost;
                    }
                    else
                    {
                        Debug.WriteLine($"Invalid hostString format: '{hostString}'");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Host resolution error: {ex.Message}");
            }
            if (_lastResolvedHost != null)
            {
                Debug.WriteLine($"Using last resolved host: {_lastResolvedHost.IpAddress}:{_lastResolvedHost.Port}");
                return _lastResolvedHost;
            }
            return null;
        }


        private void OnClientRead(Client client, IMessage message, int messageLength)
        {
            if (!_identified)
            {
                if (message.GetType() == typeof(ClientIdentificationResult))
                {
                    var reply = (ClientIdentificationResult)message;
                    _identified = reply.Result;
                }
                return;
            }

            MessageHandler.Process(client, message);
        }

        private void OnClientFail(Client client, Exception ex)
        {
            Debug.WriteLine("Client Failed: " + ex.Message);
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
                    Signature = Convert.FromBase64String(Settings.SERVERSIGNATURE)
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
