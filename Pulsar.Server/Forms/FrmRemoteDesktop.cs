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
using Pulsar.Server.Forms.RemoteDesktopPopUp;

namespace Pulsar.Server.Forms
{
    public partial class FrmRemoteDesktop : Form
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
        /// States whether drawing mode is enabled.
        /// </summary>
        private bool _enableDrawingMode;

        /// <summary>
        /// States whether eraser mode is enabled.
        /// </summary>
        private bool _enableEraserMode;

        /// <summary>
        /// The current stroke width for drawing.
        /// </summary>
        private int _strokeWidth = 5;

        /// <summary>
        /// The current drawing color.
        /// </summary>
        private Color _drawingColor = Color.Red;

        /// <summary>
        /// Keeps track of the previous mouse position for drawing.
        /// </summary>
        private Point _previousMousePosition = Point.Empty;

        /// <summary>
        /// Holds the state of the local keyboard hooks.
        /// </summary>
        private IKeyboardMouseEvents _keyboardHook;

        /// <summary>
        /// Holds the state of the local mouse hooks.
        /// </summary>
        private IKeyboardMouseEvents _mouseHook;

        /// <summary>
        /// A list of pressed keys for synchronization between key down & -up events.
        /// </summary>
        private readonly List<Keys> _keysPressed;

        /// <summary>
        /// The client which can be used for the remote desktop.
        /// </summary>
        private readonly Client _connectClient;

        /// <summary>
        /// The message handler for handling the communication with the client.
        /// </summary>
        private readonly RemoteDesktopHandler _remoteDesktopHandler;

        /// <summary>
        /// Holds the opened remote desktop form for each client.
        /// </summary>
        private static readonly Dictionary<Client, FrmRemoteDesktop> OpenedForms = new Dictionary<Client, FrmRemoteDesktop>();

        private bool _useGPU = false;
        private const int UpdateInterval = 10;

        /// <summary>
        /// Stopwatch used to suppress FPS display during the initial seconds of the stream,
        /// preventing unstable or misleading values from being shown.
        /// </summary>
        private readonly Stopwatch _fpsDisplayStopwatch = Stopwatch.StartNew();

        /// <summary>
        /// Last frames per second value to show in the title bar.
        /// </summary>
        private float _lastFps = -1f;

        private int _framesReceived = 0;

        /// <summary>
        /// Creates a new remote desktop form for the client or gets the current open form, if there exists one already.
        /// </summary>
        /// <param name="client">The client used for the remote desktop form.</param>
        /// <returns>
        /// Returns a new remote desktop form for the client if there is none currently open, otherwise creates a new one.
        /// </returns>
        public static FrmRemoteDesktop CreateNewOrGetExisting(Client client)
        {
            if (OpenedForms.ContainsKey(client))
            {
                return OpenedForms[client];
            }
            FrmRemoteDesktop r = new FrmRemoteDesktop(client);
            r.Disposed += (sender, args) => OpenedForms.Remove(client);
            OpenedForms.Add(client, r);
            return r;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FrmRemoteDesktop"/> class using the given client.
        /// </summary>
        /// <param name="client">The client used for the remote desktop form.</param>
        public FrmRemoteDesktop(Client client)
        {
            _connectClient = client;
            _remoteDesktopHandler = new RemoteDesktopHandler(client);
            _keysPressed = new List<Keys>();

            RegisterMessageHandler();
            InitializeComponent();

            DarkModeManager.ApplyDarkMode(this);
			ScreenCaptureHider.ScreenCaptureHider.Apply(this.Handle);
            
            colorPicker.BackColor = _drawingColor;
            colorPicker.FlatStyle = FlatStyle.Flat;
            colorPicker.FlatAppearance.BorderColor = Color.White;
            colorPicker.Text = "Color";
            colorPicker.ForeColor = Color.White;
            
            ConfigureDrawingButtons();
            
            btnShowDrawingTools.Enabled = false;
            
            strokeWidthTrackBar.Value = _strokeWidth;
            strokeWidthTrackBar.ValueChanged += strokeWidthTrackBar_ValueChanged;
            colorPicker.Click += colorPicker_Click;
        }

        /// <summary>
        /// Configures the drawing tool buttons with the correct styling and icons
        /// </summary>
        private void ConfigureDrawingButtons()
        {
            // pencil
            btnDrawing.Size = new Size(60, 28);
            btnDrawing.FlatStyle = FlatStyle.Flat;
            btnDrawing.FlatAppearance.BorderSize = 1;
            btnDrawing.BackgroundImage = Properties.Resources.pencil;
            btnDrawing.BackgroundImageLayout = ImageLayout.Zoom;
            btnDrawing.Text = string.Empty;
            btnDrawing.BackColor = SystemColors.Control;
            btnDrawing.UseVisualStyleBackColor = false;
            toolTipButtons.SetToolTip(btnDrawing, "Enable drawing");

            // eraser
            btnEraser.Size = new Size(60, 28);
            btnEraser.FlatStyle = FlatStyle.Flat;
            btnEraser.FlatAppearance.BorderSize = 1;
            btnEraser.BackgroundImage = Properties.Resources.eraser;
            btnEraser.BackgroundImageLayout = ImageLayout.Zoom;
            btnEraser.Text = string.Empty;
            btnEraser.BackColor = SystemColors.Control;
            btnEraser.UseVisualStyleBackColor = false;
            toolTipButtons.SetToolTip(btnEraser, "Enable eraser");

            // clear
            btnClearDrawing.Size = new Size(60, 28);
            btnClearDrawing.FlatStyle = FlatStyle.Flat;
            btnClearDrawing.FlatAppearance.BorderSize = 1;
            btnClearDrawing.BackgroundImage = Properties.Resources.clear;
            btnClearDrawing.BackgroundImageLayout = ImageLayout.Zoom;
            btnClearDrawing.Text = string.Empty;
            btnClearDrawing.BackColor = SystemColors.Control;
            btnClearDrawing.UseVisualStyleBackColor = false;
            toolTipButtons.SetToolTip(btnClearDrawing, "Clear drawing");
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
        /// Registers the remote desktop message handler for client communication.
        /// </summary>
        private void RegisterMessageHandler()
        {
            _connectClient.ClientState += ClientDisconnected;
            _remoteDesktopHandler.DisplaysChanged += DisplaysChanged;
            _remoteDesktopHandler.ProgressChanged += UpdateImage;
            MessageHandler.Register(_remoteDesktopHandler);
        }

        /// <summary>
        /// Unregisters the remote desktop message handler.
        /// </summary>
        private void UnregisterMessageHandler()
        {
            MessageHandler.Unregister(_remoteDesktopHandler);
            _remoteDesktopHandler.DisplaysChanged -= DisplaysChanged;
            _remoteDesktopHandler.ProgressChanged -= UpdateImage;
            _connectClient.ClientState -= ClientDisconnected;
        }

        /// <summary>
        /// Subscribes to local mouse and keyboard events for remote desktop input.
        /// </summary>
        private void SubscribeEvents()
        {
            // TODO: Check Hook.GlobalEvents vs Hook.AppEvents below
            // TODO: Maybe replace library with .NET events like on Linux
            if (PlatformHelper.RunningOnMono) // Mono/Linux
            {
                this.KeyDown += OnKeyDown;
                this.KeyUp += OnKeyUp;
            }
            else // Windows
            {
                _keyboardHook = Hook.GlobalEvents();
                _keyboardHook.KeyDown += OnKeyDown;
                _keyboardHook.KeyUp += OnKeyUp;

                _mouseHook = Hook.AppEvents();
                _mouseHook.MouseWheel += OnMouseWheelMove;
            }
        }

        /// <summary>
        /// Unsubscribes from local mouse and keyboard events.
        /// </summary>
        private void UnsubscribeEvents()
        {
            if (PlatformHelper.RunningOnMono) // Mono/Linux
            {
                this.KeyDown -= OnKeyDown;
                this.KeyUp -= OnKeyUp;
            }
            else // Windows
            {
                if (_keyboardHook != null)
                {
                    _keyboardHook.KeyDown -= OnKeyDown;
                    _keyboardHook.KeyUp -= OnKeyUp;
                    _keyboardHook.Dispose();
                }
                if (_mouseHook != null)
                {
                    _mouseHook.MouseWheel -= OnMouseWheelMove;
                    _mouseHook.Dispose();
                }
            }
        }

        /// <summary>
        /// Starts the remote desktop stream and begin to receive desktop frames.
        /// </summary>
        private void StartStream(bool useGpu)
        {
            ToggleConfigurationControls(true);

            picDesktop.Start();
            // Subscribe to the new frame counter.
            picDesktop.SetFrameUpdatedEvent(frameCounter_FrameUpdated);

            this.ActiveControl = picDesktop;

            _remoteDesktopHandler.BeginReceiveFrames(barQuality.Value, cbMonitors.SelectedIndex, useGpu);
            
            btnShowDrawingTools.Enabled = true;
        }

        /// <summary>
        /// Stops the remote desktop stream.
        /// </summary>
        private void StopStream()
        {
            ToggleConfigurationControls(false);

            picDesktop.Stop();
            // Unsubscribe from the frame counter. It will be re-created when starting again.
            picDesktop.UnsetFrameUpdatedEvent(frameCounter_FrameUpdated);

            this.ActiveControl = picDesktop;

            _remoteDesktopHandler.EndReceiveFrames();
            
            btnShowDrawingTools.Enabled = false;
            
            if (panelDrawingTools.Visible)
            {
                panelDrawingTools.Visible = false;
                btnShowDrawingTools.Image = Properties.Resources.arrow_up;
                toolTipButtons.SetToolTip(btnShowDrawingTools, "Show drawing tools");
            }
            
            _enableDrawingMode = false;
            _enableEraserMode = false;
            
            ConfigureDrawingButtons();
            
            picDesktop.Cursor = Cursors.Default;
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

        private void UpdateImage(object sender, Bitmap bmp)
        {
            _framesReceived++;

            if (_framesReceived >= 30)
            {
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

                _framesReceived = 0;
            }

            picDesktop.UpdateImage(bmp, false);
        }

        private void FrmRemoteDesktop_Load(object sender, EventArgs e)
        {
            this.Text = WindowHelper.GetWindowTitle("Remote Desktop", _connectClient);

            OnResize(EventArgs.Empty); // trigger resize event to align controls 

            panelDrawingTools.Visible = false;
            btnShowDrawingTools.Image = Properties.Resources.arrow_up;
            toolTipButtons.SetToolTip(btnShowDrawingTools, "Show drawing tools");

            _enableDrawingMode = false;
            _enableEraserMode = false;
            
            ConfigureDrawingButtons();

            _remoteDesktopHandler.RefreshDisplays();
        }
        
        /// <summary>
        /// Updates the title with the current frames per second.
        /// </summary>
        /// <param name="e">The new frames per second.</param>
        private void frameCounter_FrameUpdated(FrameUpdatedEventArgs e)
        {
            float fpsToShow = _remoteDesktopHandler.CurrentFps > 0 ? _remoteDesktopHandler.CurrentFps : e.CurrentFramesPerSecond;
            this.Text = string.Format("{0} - FPS: {1}", WindowHelper.GetWindowTitle("Remote Desktop", _connectClient), fpsToShow.ToString("0.00"));
        }

        private void FrmRemoteDesktop_FormClosing(object sender, FormClosingEventArgs e)
        {
            // all cleanup logic goes here
            UnsubscribeEvents();
            if (_remoteDesktopHandler.IsStarted) StopStream();
            UnregisterMessageHandler();
            _remoteDesktopHandler.Dispose();
            picDesktop.Image?.Dispose();
        }

        private void FrmRemoteDesktop_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
                return;

            _remoteDesktopHandler.LocalResolution = picDesktop.Size;
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

        #region Remote Desktop Input

        private void picDesktop_MouseDown(object sender, MouseEventArgs e)
        {
            if (picDesktop.Image != null && this.ContainsFocus)
            {
                if ((_enableDrawingMode || _enableEraserMode) && e.Button == MouseButtons.Left)
                {
                    _previousMousePosition = e.Location;
                }
                else if (_enableMouseInput && !(_enableDrawingMode || _enableEraserMode))
                {
                    MouseAction action = MouseAction.None;

                    if (e.Button == MouseButtons.Left)
                        action = MouseAction.LeftDown;
                    if (e.Button == MouseButtons.Right)
                        action = MouseAction.RightDown;

                    int selectedDisplayIndex = cbMonitors.SelectedIndex;

                    _remoteDesktopHandler.SendMouseEvent(action, true, e.X, e.Y, selectedDisplayIndex);
                }
            }
        }

        private void picDesktop_MouseMove(object sender, MouseEventArgs e)
        {
            if (picDesktop.Image != null && this.ContainsFocus)
            {
                if ((_enableDrawingMode || _enableEraserMode) && e.Button == MouseButtons.Left)
                {
                    if (_previousMousePosition != Point.Empty && 
                        (_previousMousePosition.X != e.X || _previousMousePosition.Y != e.Y))
                    {
                        int selectedDisplayIndex = cbMonitors.SelectedIndex;
                        
                        bool useEraser = _enableEraserMode;
                        
                        _remoteDesktopHandler.SendDrawingEvent(
                            e.X, e.Y,
                            _previousMousePosition.X, _previousMousePosition.Y,
                            _strokeWidth, 
                            _drawingColor.ToArgb(),
                            useEraser,
                            false,
                            selectedDisplayIndex);
                        
                        _previousMousePosition = e.Location;
                    }
                }
                else if (_enableMouseInput && !(_enableDrawingMode || _enableEraserMode))
                {
                    int selectedDisplayIndex = cbMonitors.SelectedIndex;

                    _remoteDesktopHandler.SendMouseEvent(MouseAction.MoveCursor, false, e.X, e.Y, selectedDisplayIndex);
                }
            }
        }

        private void picDesktop_MouseUp(object sender, MouseEventArgs e)
        {
            if (picDesktop.Image != null && this.ContainsFocus)
            {
                if ((_enableDrawingMode || _enableEraserMode) && e.Button == MouseButtons.Left)
                {
                    _previousMousePosition = Point.Empty;
                }
                else if (_enableMouseInput && !(_enableDrawingMode || _enableEraserMode))
                {
                    MouseAction action = MouseAction.None;

                    if (e.Button == MouseButtons.Left)
                        action = MouseAction.LeftUp;
                    if (e.Button == MouseButtons.Right)
                        action = MouseAction.RightUp;

                    int selectedDisplayIndex = cbMonitors.SelectedIndex;

                    _remoteDesktopHandler.SendMouseEvent(action, false, e.X, e.Y, selectedDisplayIndex);
                }
            }
        }

        private void OnMouseWheelMove(object sender, MouseEventArgs e)
        {
            if (picDesktop.Image != null && _enableMouseInput && this.ContainsFocus)
            {
                _remoteDesktopHandler.SendMouseEvent(e.Delta == 120 ? MouseAction.ScrollUp : MouseAction.ScrollDown,
                    false, 0, 0, cbMonitors.SelectedIndex);
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (picDesktop.Image != null && _enableKeyboardInput && this.ContainsFocus)
            {
                if (!IsLockKey(e.KeyCode))
                    e.Handled = true;

                if (_keysPressed.Contains(e.KeyCode))
                    return;

                _keysPressed.Add(e.KeyCode);

                _remoteDesktopHandler.SendKeyboardEvent((byte)e.KeyCode, true);
            }
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (picDesktop.Image != null && _enableKeyboardInput && this.ContainsFocus)
            {
                if (!IsLockKey(e.KeyCode))
                    e.Handled = true;

                _keysPressed.Remove(e.KeyCode);

                _remoteDesktopHandler.SendKeyboardEvent((byte)e.KeyCode, false);
            }
        }

        private bool IsLockKey(Keys key)
        {
            return ((key & Keys.CapsLock) == Keys.CapsLock)
                   || ((key & Keys.NumLock) == Keys.NumLock)
                   || ((key & Keys.Scroll) == Keys.Scroll);
        }

        #endregion

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

            UpdateInputButtonsVisualState();
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

            UpdateInputButtonsVisualState();
            this.ActiveControl = picDesktop;
        }

        private void enableGPU_Click(object sender, EventArgs e)
        {
            _useGPU = !_useGPU;
            if (_useGPU)
            {
                enableGPU.Image = Properties.Resources.computer_go; // enable GPU
                toolTipButtons.SetToolTip(enableGPU, "Disable GPU.");
            }
            else
            {
                enableGPU.Image = Properties.Resources.computer_error; // disable GPU
                toolTipButtons.SetToolTip(enableGPU, "Enable GPU.");
            }
        }

        private void btnDrawing_Click(object sender, EventArgs e)
        {
            if (!_remoteDesktopHandler.IsStarted)
            {
                MessageBox.Show("Drawing is only available when Remote Desktop is started", 
                    "Drawing unavailable", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            _enableDrawingMode = !_enableDrawingMode;
            
            if (_enableDrawingMode)
            {
                _enableEraserMode = false;
                
                btnEraser.BackColor = SystemColors.Control;
                toolTipButtons.SetToolTip(btnEraser, "Enable eraser");
                
                btnDrawing.BackColor = Color.FromArgb(120, 170, 120);
                toolTipButtons.SetToolTip(btnDrawing, "Disable drawing");
                picDesktop.Cursor = Cursors.Cross;
            }
            else
            {
                btnDrawing.BackColor = SystemColors.Control;
                toolTipButtons.SetToolTip(btnDrawing, "Enable drawing");
                picDesktop.Cursor = Cursors.Default;
            }
            
            this.ActiveControl = picDesktop;
        }

        private void btnEraser_Click(object sender, EventArgs e)
        {
            if (!_remoteDesktopHandler.IsStarted)
            {
                MessageBox.Show("Eraser is only available when Remote Desktop is started", 
                    "Eraser unavailable", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            _enableEraserMode = !_enableEraserMode;
            
            if (_enableEraserMode)
            {
                _enableDrawingMode = false;
                
                btnDrawing.BackColor = SystemColors.Control;
                toolTipButtons.SetToolTip(btnDrawing, "Enable drawing");
                
                btnEraser.BackColor = Color.FromArgb(120, 170, 120);
                toolTipButtons.SetToolTip(btnEraser, "Disable eraser");
                picDesktop.Cursor = Cursors.Hand;
            }
            else
            {
                btnEraser.BackColor = SystemColors.Control;
                toolTipButtons.SetToolTip(btnEraser, "Enable eraser");
                picDesktop.Cursor = Cursors.Default;
            }
            
            this.ActiveControl = picDesktop;
        }

        private void btnShowDrawingTools_Click(object sender, EventArgs e)
        {
            ToggleDrawingPanelVisibility(!panelDrawingTools.Visible);
        }

        private void ToggleDrawingPanelVisibility(bool visible)
        {
            if (visible && !_remoteDesktopHandler.IsStarted)
            {
                MessageBox.Show("Drawing tools are only available when Remote Desktop is started", 
                    "Drawing unavailable", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            panelDrawingTools.Visible = visible;
            btnShowDrawingTools.Image = visible ? 
                Properties.Resources.arrow_down : 
                Properties.Resources.arrow_up;
            toolTipButtons.SetToolTip(btnShowDrawingTools, 
                visible ? "Hide drawing tools" : "Show drawing tools");
            this.ActiveControl = picDesktop;
        }

        private void btnClearDrawing_Click(object sender, EventArgs e)
        {
            if (!_remoteDesktopHandler.IsStarted)
            {
                MessageBox.Show("Clear drawing is only available when Remote Desktop is started", 
                    "Clear unavailable", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            int displayIndex = cbMonitors.SelectedIndex;
            _remoteDesktopHandler.SendDrawingEvent(0, 0, 0, 0, 0, 0, false, true, displayIndex);
        }
        
        private void colorPicker_Click(object sender, EventArgs e)
        {
            if (!_remoteDesktopHandler.IsStarted)
            {
                MessageBox.Show("Color selection is only available when Remote Desktop is started", 
                    "Color selection unavailable", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            ColorDialog colorDialog = new ColorDialog();
            colorDialog.Color = _drawingColor;
            colorDialog.AllowFullOpen = true;
            colorDialog.AnyColor = true;
            
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                _drawingColor = colorDialog.Color;
                colorPicker.BackColor = _drawingColor;
                
                colorPicker.ForeColor = GetContrastColor(_drawingColor);
            }
        }
        
        private void strokeWidthTrackBar_ValueChanged(object sender, EventArgs e)
        {
            _strokeWidth = strokeWidthTrackBar.Value;
        }
        
        private Color GetContrastColor(Color color)
        {
            int brightness = (int)Math.Sqrt(
                color.R * color.R * 0.299 +
                color.G * color.G * 0.587 +
                color.B * color.B * 0.114);
                
            return brightness > 130 ? Color.Black : Color.White;
        }

        #endregion

        /// <summary>
        /// Updates the visual state of the input buttons based on current input settings
        /// </summary>
        private void UpdateInputButtonsVisualState()
        {
            if (_enableMouseInput)
            {
                btnMouse.BackColor = System.Drawing.Color.FromArgb(0, 120, 0); // Dark green
                btnMouse.FlatAppearance.BorderColor = System.Drawing.Color.LimeGreen;
            }
            else
            {
                btnMouse.BackColor = System.Drawing.Color.FromArgb(40, 40, 40); // Default dark
                btnMouse.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
            }

            if (_enableKeyboardInput)
            {
                btnKeyboard.BackColor = System.Drawing.Color.FromArgb(0, 120, 0); // Dark green
                btnKeyboard.FlatAppearance.BorderColor = System.Drawing.Color.LimeGreen;
            }
            else
            {
                btnKeyboard.BackColor = System.Drawing.Color.FromArgb(40, 40, 40); // Default dark
                btnKeyboard.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
            }
        }

        private void btnHide_Click(object sender, EventArgs e)
        {
            TogglePanelVisibility(false);
        }

        private void btnShow_Click(object sender, EventArgs e)
        {
            TogglePanelVisibility(true);
        }

        private void btnStartProgramOnDisplay_Click(object sender, EventArgs e)
        {
            int currentDisplayIndex = cbMonitors.SelectedIndex;
            FrmOpenApplicationOnMonitor frm = new FrmOpenApplicationOnMonitor(_connectClient, currentDisplayIndex);
            frm.ShowDialog(this);
        }
    }
}
