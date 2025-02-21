using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Gma.System.MouseKeyHook;
using Quasar.Common.Messages;
using Quasar.Server.Controls;
using Quasar.Server.Helper;
using Quasar.Server.Messages;
using Quasar.Server.Networking;
using Quasar.Server.Properties;
using Quasar.Server.Utilities;

namespace Quasar.Server.Forms
{
    // Token: 0x02000036 RID: 54
    public class FrmHvnc : Form
    {
        // Token: 0x0600032B RID: 811 RVA: 0x000120FC File Offset: 0x000102FC
        public static FrmHvnc CreateNewOrGetExisting(Client client)
        {
            if (FrmHvnc.OpenedForms.ContainsKey(client))
            {
                return FrmHvnc.OpenedForms[client];
            }
            FrmHvnc frmHvnc = new FrmHvnc(client);
            frmHvnc.Disposed += delegate (object sender, EventArgs args)
            {
                FrmHvnc.OpenedForms.Remove(client);
            };
            FrmHvnc.OpenedForms.Add(client, frmHvnc);
            return frmHvnc;
        }

        // Token: 0x0600032C RID: 812 RVA: 0x00012169 File Offset: 0x00010369
        public FrmHvnc(Client client)
        {
            this._connectClient = client;
            this._remoteDesktopHandler = new FrmHvnHandler(client);
            this._keysPressed = new List<Keys>();
            this.RegisterMessageHandler();
            this.InitializeComponent();
        }

        // Token: 0x0600032D RID: 813 RVA: 0x0001219B File Offset: 0x0001039B
        private void ClientDisconnected(Client client, bool connected)
        {
            if (!connected)
            {
                base.Invoke(new MethodInvoker(base.Close));
            }
        }

        // Token: 0x0600032E RID: 814 RVA: 0x000121B3 File Offset: 0x000103B3
        private void RegisterMessageHandler()
        {
            this._connectClient.ClientState += this.ClientDisconnected;
            this._remoteDesktopHandler.ProgressChanged += this.UpdateImage;
            MessageHandler.Register(this._remoteDesktopHandler);
        }

        // Token: 0x0600032F RID: 815 RVA: 0x000121EE File Offset: 0x000103EE
        private void UnregisterMessageHandler()
        {
            MessageHandler.Unregister(this._remoteDesktopHandler);
            this._remoteDesktopHandler.ProgressChanged -= this.UpdateImage;
            this._connectClient.ClientState -= this.ClientDisconnected;
        }

        // Token: 0x06000330 RID: 816 RVA: 0x0001222C File Offset: 0x0001042C
        private void StartStream()
        {
            this.ToggleConfigurationControls(true);
            this.picDesktop.Start();
            this.picDesktop.SetFrameUpdatedEvent(new FrameUpdatedEventHandler(this.frameCounter_FrameUpdated));
            this.picDesktop.SetRemoteDesktopHandler(this._remoteDesktopHandler);
            base.ActiveControl = this.picDesktop;
            this._remoteDesktopHandler.BeginReceiveFrames(this.barQuality.Value);
        }

        // Token: 0x06000331 RID: 817 RVA: 0x00012298 File Offset: 0x00010498
        private void StopStream()
        {
            this.ToggleConfigurationControls(false);
            this.picDesktop.Stop();
            this.picDesktop.UnsetFrameUpdatedEvent(new FrameUpdatedEventHandler(this.frameCounter_FrameUpdated));
            base.ActiveControl = this.picDesktop;
            this._remoteDesktopHandler.EndReceiveFrames();
        }

        // Token: 0x06000332 RID: 818 RVA: 0x000122E5 File Offset: 0x000104E5
        private void ToggleConfigurationControls(bool started)
        {
            this.btnStart.Enabled = !started;
            this.btnStop.Enabled = started;
            this.barQuality.Enabled = !started;
        }

        // Token: 0x06000333 RID: 819 RVA: 0x00012311 File Offset: 0x00010511
        private void TogglePanelVisibility(bool visible)
        {
            this.panelTop.Visible = visible;
            this.btnShow.Visible = !visible;
            base.ActiveControl = this.picDesktop;
        }

        // Token: 0x06000334 RID: 820 RVA: 0x0001233A File Offset: 0x0001053A
        private void UpdateImage(object sender, Bitmap bmp)
        {
            this.picDesktop.UpdateImage(bmp, false);
        }

        // Token: 0x06000335 RID: 821 RVA: 0x00012349 File Offset: 0x00010549
        private void FrmHvnc_Load(object sender, EventArgs e)
        {
            this.Text = WindowHelper.GetWindowTitle("Remote Desktop", this._connectClient);
            this.OnResize(EventArgs.Empty);
            this._remoteDesktopHandler.RefreshDisplays();
        }

        // Token: 0x06000336 RID: 822 RVA: 0x00012378 File Offset: 0x00010578
        private void frameCounter_FrameUpdated(FrameUpdatedEventArgs e)
        {
            this.Text = string.Format("{0} - FPS: {1}", WindowHelper.GetWindowTitle("Remote Desktop", this._connectClient), e.CurrentFramesPerSecond.ToString("0.00"));
        }

        // Token: 0x06000337 RID: 823 RVA: 0x000123B8 File Offset: 0x000105B8
        private void FrmHvnc_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this._remoteDesktopHandler.IsStarted)
            {
                this.StopStream();
            }
            this.UnregisterMessageHandler();
            this._remoteDesktopHandler.Dispose();
            Image image = this.picDesktop.Image;
            if (image == null)
            {
                return;
            }
            image.Dispose();
        }

        // Token: 0x06000338 RID: 824 RVA: 0x000123F4 File Offset: 0x000105F4
        private void FrmHvnc_Resize(object sender, EventArgs e)
        {
            if (base.WindowState == FormWindowState.Minimized)
            {
                return;
            }
            this._remoteDesktopHandler.LocalResolution = this.picDesktop.Size;
            this.panelTop.Left = (base.Width - this.panelTop.Width) / 2;
            this.btnShow.Left = (base.Width - this.btnShow.Width) / 2;
            this.btnHide.Left = (this.panelTop.Width - this.btnHide.Width) / 2;
        }

        // Token: 0x06000339 RID: 825 RVA: 0x00012483 File Offset: 0x00010683
        private void btnStart_Click(object sender, EventArgs e)
        {
            this.StartStream();
        }

        // Token: 0x0600033A RID: 826 RVA: 0x0001248B File Offset: 0x0001068B
        private void btnStop_Click(object sender, EventArgs e)
        {
            this.StopStream();
        }

        // Token: 0x0600033B RID: 827 RVA: 0x00012494 File Offset: 0x00010694
        private void barQuality_Scroll(object sender, EventArgs e)
        {
            int value = this.barQuality.Value;
            this.lblQualityShow.Text = value.ToString();
            if (value < 25)
            {
                Label label = this.lblQualityShow;
                label.Text += " (low)";
            }
            else if (value >= 85)
            {
                Label label2 = this.lblQualityShow;
                label2.Text += " (best)";
            }
            else if (value >= 75)
            {
                Label label3 = this.lblQualityShow;
                label3.Text += " (high)";
            }
            else if (value >= 25)
            {
                Label label4 = this.lblQualityShow;
                label4.Text += " (mid)";
            }
            base.ActiveControl = this.picDesktop;
        }

        // Token: 0x0600033C RID: 828 RVA: 0x00012554 File Offset: 0x00010754
        private void btnKeyboard_Click(object sender, EventArgs e)
        {
            if (this._enableKeyboardInput)
            {
                this.picDesktop.Cursor = Cursors.Default;
                this.btnKeyboard.Image = Resources.keyboard_delete;
                this.toolTipButtons.SetToolTip(this.btnKeyboard, "Enable keyboard input.");
                this._enableKeyboardInput = false;
            }
            else
            {
                this.picDesktop.Cursor = Cursors.Hand;
                this.btnKeyboard.Image = Resources.keyboard_add;
                this.toolTipButtons.SetToolTip(this.btnKeyboard, "Disable keyboard input.");
                this._enableKeyboardInput = true;
            }
            base.ActiveControl = this.picDesktop;
        }

        // Token: 0x0600033D RID: 829 RVA: 0x000125F1 File Offset: 0x000107F1
        private void btnHide_Click(object sender, EventArgs e)
        {
            this.TogglePanelVisibility(false);
        }

        // Token: 0x0600033E RID: 830 RVA: 0x000125FA File Offset: 0x000107FA
        private void btnShow_Click(object sender, EventArgs e)
        {
            this.TogglePanelVisibility(true);
        }

        // Token: 0x0600033F RID: 831 RVA: 0x00012603 File Offset: 0x00010803
        private void button1_Click(object sender, EventArgs e)
        {
            this._connectClient.Send<HvncApplication>(new HvncApplication
            {
                name = this.cbAppication.Text
            });
        }

        // Token: 0x06000340 RID: 832 RVA: 0x00012626 File Offset: 0x00010826
        protected override void Dispose(bool disposing)
        {
            if (disposing && this.components != null)
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        // Token: 0x06000341 RID: 833 RVA: 0x00012648 File Offset: 0x00010848
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.barQuality = new System.Windows.Forms.TrackBar();
            this.lblQuality = new System.Windows.Forms.Label();
            this.lblQualityShow = new System.Windows.Forms.Label();
            this.panelTop = new System.Windows.Forms.Panel();
            this.button1 = new System.Windows.Forms.Button();
            this.btnKeyboard = new System.Windows.Forms.Button();
            this.cbAppication = new System.Windows.Forms.ComboBox();
            this.btnHide = new System.Windows.Forms.Button();
            this.btnShow = new System.Windows.Forms.Button();
            this.toolTipButtons = new System.Windows.Forms.ToolTip(this.components);
            this.picDesktop = new Quasar.Server.Controls.RapidPictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.barQuality)).BeginInit();
            this.panelTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picDesktop)).BeginInit();
            this.SuspendLayout();
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(15, 5);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(68, 23);
            this.btnStart.TabIndex = 1;
            this.btnStart.TabStop = false;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click_1);
            // 
            // btnStop
            // 
            this.btnStop.Enabled = false;
            this.btnStop.Location = new System.Drawing.Point(96, 5);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(68, 23);
            this.btnStop.TabIndex = 2;
            this.btnStop.TabStop = false;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            // 
            // barQuality
            // 
            this.barQuality.Location = new System.Drawing.Point(206, -1);
            this.barQuality.Maximum = 100;
            this.barQuality.Minimum = 1;
            this.barQuality.Name = "barQuality";
            this.barQuality.Size = new System.Drawing.Size(76, 45);
            this.barQuality.TabIndex = 3;
            this.barQuality.TabStop = false;
            this.barQuality.Value = 75;
            // 
            // lblQuality
            // 
            this.lblQuality.AutoSize = true;
            this.lblQuality.Location = new System.Drawing.Point(167, 5);
            this.lblQuality.Name = "lblQuality";
            this.lblQuality.Size = new System.Drawing.Size(46, 13);
            this.lblQuality.TabIndex = 4;
            this.lblQuality.Text = "Quality:";
            // 
            // lblQualityShow
            // 
            this.lblQualityShow.AutoSize = true;
            this.lblQualityShow.Location = new System.Drawing.Point(220, 26);
            this.lblQualityShow.Name = "lblQualityShow";
            this.lblQualityShow.Size = new System.Drawing.Size(52, 13);
            this.lblQualityShow.TabIndex = 5;
            this.lblQualityShow.Text = "75 (high)";
            // 
            // panelTop
            // 
            this.panelTop.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelTop.Controls.Add(this.button1);
            this.panelTop.Controls.Add(this.btnKeyboard);
            this.panelTop.Controls.Add(this.cbAppication);
            this.panelTop.Controls.Add(this.btnHide);
            this.panelTop.Controls.Add(this.lblQualityShow);
            this.panelTop.Controls.Add(this.btnStart);
            this.panelTop.Controls.Add(this.btnStop);
            this.panelTop.Controls.Add(this.lblQuality);
            this.panelTop.Controls.Add(this.barQuality);
            this.panelTop.Location = new System.Drawing.Point(86, -1);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(608, 75);
            this.panelTop.TabIndex = 7;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(416, 35);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(68, 23);
            this.button1.TabIndex = 10;
            this.button1.TabStop = false;
            this.button1.Text = "Run";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // btnKeyboard
            // 
            this.btnKeyboard.Image = global::Quasar.Server.Properties.Resources.keyboard_delete;
            this.btnKeyboard.Location = new System.Drawing.Point(336, 5);
            this.btnKeyboard.Name = "btnKeyboard";
            this.btnKeyboard.Size = new System.Drawing.Size(28, 28);
            this.btnKeyboard.TabIndex = 9;
            this.btnKeyboard.TabStop = false;
            this.toolTipButtons.SetToolTip(this.btnKeyboard, "Enable keyboard input.");
            this.btnKeyboard.UseVisualStyleBackColor = true;
            // 
            // cbAppication
            // 
            this.cbAppication.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbAppication.FormattingEnabled = true;
            this.cbAppication.Items.AddRange(new object[] {
            "Explorer",
            "Chrome",
            "Mozilla",
            "Edge",
            "Cmd"});
            this.cbAppication.Location = new System.Drawing.Point(392, 7);
            this.cbAppication.Name = "cbAppication";
            this.cbAppication.Size = new System.Drawing.Size(149, 21);
            this.cbAppication.TabIndex = 8;
            this.cbAppication.TabStop = false;
            // 
            // btnHide
            // 
            this.btnHide.Location = new System.Drawing.Point(196, 51);
            this.btnHide.Name = "btnHide";
            this.btnHide.Size = new System.Drawing.Size(54, 19);
            this.btnHide.TabIndex = 7;
            this.btnHide.TabStop = false;
            this.btnHide.Text = "Hide";
            this.btnHide.UseVisualStyleBackColor = true;
            // 
            // btnShow
            // 
            this.btnShow.Location = new System.Drawing.Point(0, 0);
            this.btnShow.Name = "btnShow";
            this.btnShow.Size = new System.Drawing.Size(54, 19);
            this.btnShow.TabIndex = 8;
            this.btnShow.TabStop = false;
            this.btnShow.Text = "Show";
            this.btnShow.UseVisualStyleBackColor = true;
            this.btnShow.Visible = false;
            // 
            // picDesktop
            // 
            this.picDesktop.BackColor = System.Drawing.Color.Black;
            this.picDesktop.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picDesktop.Cursor = System.Windows.Forms.Cursors.Default;
            this.picDesktop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picDesktop.GetImageSafe = null;
            this.picDesktop.Location = new System.Drawing.Point(0, 0);
            this.picDesktop.Name = "picDesktop";
            this.picDesktop.Running = false;
            this.picDesktop.Size = new System.Drawing.Size(784, 562);
            this.picDesktop.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picDesktop.TabIndex = 0;
            this.picDesktop.TabStop = false;
            // 
            // FrmHvnc
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(784, 562);
            this.Controls.Add(this.btnShow);
            this.Controls.Add(this.panelTop);
            this.Controls.Add(this.picDesktop);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(640, 480);
            this.Name = "FrmHvnc";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "HVNC []";
            this.Load += new System.EventHandler(this.FrmHvnc_Load_1);
            ((System.ComponentModel.ISupportInitialize)(this.barQuality)).EndInit();
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picDesktop)).EndInit();
            this.ResumeLayout(false);

        }

        // Token: 0x040001B4 RID: 436
        private bool _enableMouseInput;

        // Token: 0x040001B5 RID: 437
        private bool _enableKeyboardInput;

        // Token: 0x040001B6 RID: 438
        private IKeyboardMouseEvents _keyboardHook;

        // Token: 0x040001B7 RID: 439
        private IKeyboardMouseEvents _mouseHook;

        // Token: 0x040001B8 RID: 440
        private readonly List<Keys> _keysPressed;

        // Token: 0x040001B9 RID: 441
        private readonly Client _connectClient;

        // Token: 0x040001BA RID: 442
        private readonly FrmHvnHandler _remoteDesktopHandler;

        // Token: 0x040001BB RID: 443
        private static readonly Dictionary<Client, FrmHvnc> OpenedForms = new Dictionary<Client, FrmHvnc>();

        // Token: 0x040001BC RID: 444
        private IContainer components;

        // Token: 0x040001BD RID: 445
        private Button btnStart;

        // Token: 0x040001BE RID: 446
        private Button btnStop;

        // Token: 0x040001BF RID: 447
        private TrackBar barQuality;

        // Token: 0x040001C0 RID: 448
        private Label lblQuality;

        // Token: 0x040001C1 RID: 449
        private Label lblQualityShow;

        // Token: 0x040001C2 RID: 450
        private Panel panelTop;

        // Token: 0x040001C3 RID: 451
        private Button btnHide;

        // Token: 0x040001C4 RID: 452
        private Button btnShow;

        // Token: 0x040001C5 RID: 453
        private ComboBox cbAppication;

        // Token: 0x040001C6 RID: 454
        private Button btnKeyboard;

        // Token: 0x040001C7 RID: 455
        private ToolTip toolTipButtons;

        // Token: 0x040001C8 RID: 456
        private RapidPictureBox picDesktop;

        // Token: 0x040001C9 RID: 457
        private Button button1;

        private void FrmHvnc_Load_1(object sender, EventArgs e)
        {

        }

        private void btnStart_Click_1(object sender, EventArgs e)
        {

        }
    }
}
