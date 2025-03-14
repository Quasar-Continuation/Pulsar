using System;
using System.Windows.Forms;
using DiscordRPC;

namespace Quasar.Server.Forms.DiscordRPC
{
    internal class DiscordRPC
    {
        private readonly Form _form;
        private bool _enabled;
        private DiscordRpcClient _client;
        private readonly string _applicationId = "1349912775698153562";

        public DiscordRPC(Form form)
        {
            _form = form;
            _enabled = false;
            _client = new DiscordRpcClient(_applicationId);
        }

        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                _enabled = value;
                if (_enabled)
                {
                    if (_client == null || _client.IsDisposed)
                    {
                        _client = new DiscordRpcClient(_applicationId);
                        Console.WriteLine("Discord RPC Client recreated");
                    }
                    if (!_client.IsInitialized)
                    {
                        try
                        {
                            _client.Initialize();
                            Console.WriteLine("Discord RPC Client Initialized");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Failed to initialize Discord RPC: " + ex.Message);
                            return;
                        }
                    }
                    try
                    {
                        _client.OnReady += (sender, e) =>
                        {
                            Console.WriteLine("Discord RPC Ready for " + _form.Text);
                        };
                        SetPresence();
                        Timer updateTimer = new Timer();
                        updateTimer.Interval = 5000; // 5 seconds
                        updateTimer.Tick += (s, e) => SetPresence();
                        updateTimer.Start();
                        Console.WriteLine("Discord RPC Enabled for " + _form.Text);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Failed to set Discord presence: " + ex.Message);
                    }
                }
                else
                {
                    if (_client != null && _client.IsInitialized)
                    {
                        try
                        {
                            _client.ClearPresence();
                            _client.Deinitialize();
                            Console.WriteLine("Discord RPC Disabled for " + _form.Text);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Failed to disable Discord RPC: " + ex.Message);
                        }
                    }
                }
            }
        }

        private int GetConnectedClientsCount()
        {
            try
            {
                string title = _form.Text;
                int startIndex = title.IndexOf("Connected: ") + "Connected: ".Length;
                int endIndex = title.IndexOf(" ", startIndex) > 0 ? title.IndexOf(" ", startIndex) : title.Length;
                string countStr = title.Substring(startIndex, endIndex - startIndex);
                return int.Parse(countStr);
            }
            catch
            {
                return 0;
            }
        }

        private void SetPresence()
        {
            int connectedClients = GetConnectedClientsCount();
            _client.SetPresence(new RichPresence
            {
                Details = "Modded by KDot227",
                State = $"Connected Clients: {connectedClients}",
                Assets = new Assets
                {
                    LargeImageKey = "default",
                    LargeImageText = "Quasar RAT - Modded by Kdot227"
                },
                Timestamps = new Timestamps { Start = DateTime.UtcNow }
            });
        }
    }
}