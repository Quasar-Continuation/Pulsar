using Pulsar.Server.Controls;
using Pulsar.Server.Images.Helpers;

namespace Pulsar.Server.Forms
{
    partial class FrmRemoteDesktop
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmRemoteDesktop));
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.barQuality = new System.Windows.Forms.TrackBar();
            this.lblQuality = new System.Windows.Forms.Label();
            this.lblQualityShow = new System.Windows.Forms.Label();
            this.btnMouse = new System.Windows.Forms.Button();
            this.panelTop = new System.Windows.Forms.Panel();
            this.btnStartProgramOnDisplay = new System.Windows.Forms.Button();
            this.btnShowDrawingTools = new System.Windows.Forms.Button();
            this.sizeLabelCounter = new System.Windows.Forms.Label();
            this.enableGPU = new System.Windows.Forms.Button();
            this.btnKeyboard = new System.Windows.Forms.Button();
            this.cbMonitors = new System.Windows.Forms.ComboBox();
            this.btnHide = new System.Windows.Forms.Button();
            this.panelDrawingTools = new System.Windows.Forms.Panel();
            this.colorPicker = new System.Windows.Forms.Button();
            this.strokeWidthTrackBar = new System.Windows.Forms.TrackBar();
            this.btnDrawing = new System.Windows.Forms.Button();
            this.btnEraser = new System.Windows.Forms.Button();
            this.btnClearDrawing = new System.Windows.Forms.Button();
            this.btnShow = new System.Windows.Forms.Button();
            this.toolTipButtons = new System.Windows.Forms.ToolTip(this.components);
            this.picDesktop = new Pulsar.Server.Controls.RapidPictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.barQuality)).BeginInit();
            this.panelTop.SuspendLayout();
            this.panelDrawingTools.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.strokeWidthTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picDesktop)).BeginInit();
            this.SuspendLayout();
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(11, 3);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(68, 28);
            this.btnStart.TabIndex = 1;
            this.btnStart.TabStop = false;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnStop
            // 
            this.btnStop.Enabled = false;
            this.btnStop.Location = new System.Drawing.Point(85, 3);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(68, 28);
            this.btnStop.TabIndex = 2;
            this.btnStop.TabStop = false;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // barQuality
            // 
            this.barQuality.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.barQuality.Location = new System.Drawing.Point(515, 3);
            this.barQuality.Maximum = 100;
            this.barQuality.Minimum = 1;
            this.barQuality.Name = "barQuality";
            this.barQuality.Size = new System.Drawing.Size(76, 45);
            this.barQuality.TabIndex = 3;
            this.barQuality.TabStop = false;
            this.barQuality.Value = 75;
            this.barQuality.Scroll += new System.EventHandler(this.barQuality_Scroll);
            // 
            // lblQuality
            // 
            this.lblQuality.AutoSize = true;
            this.lblQuality.Location = new System.Drawing.Point(463, 5);
            this.lblQuality.Name = "lblQuality";
            this.lblQuality.Size = new System.Drawing.Size(46, 13);
            this.lblQuality.TabIndex = 4;
            this.lblQuality.Text = "Quality:";
            // 
            // lblQualityShow
            // 
            this.lblQualityShow.AutoSize = true;
            this.lblQualityShow.Location = new System.Drawing.Point(463, 18);
            this.lblQualityShow.Name = "lblQualityShow";
            this.lblQualityShow.Size = new System.Drawing.Size(52, 13);
            this.lblQualityShow.TabIndex = 5;
            this.lblQualityShow.Text = "75 (high)";
            // 
            // btnMouse
            // 
            this.btnMouse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMouse.Image = global::Pulsar.Server.Properties.Resources.mouse_delete;
            this.btnMouse.Location = new System.Drawing.Point(626, 3);
            this.btnMouse.Name = "btnMouse";
            this.btnMouse.Size = new System.Drawing.Size(28, 28);
            this.btnMouse.TabIndex = 6;
            this.btnMouse.TabStop = false;
            this.toolTipButtons.SetToolTip(this.btnMouse, "Enable mouse input.");
            this.btnMouse.UseVisualStyleBackColor = true;
            this.btnMouse.Click += new System.EventHandler(this.btnMouse_Click);
            // 
            // panelTop
            // 
            this.panelTop.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelTop.Controls.Add(this.btnStartProgramOnDisplay);
            this.panelTop.Controls.Add(this.btnShowDrawingTools);
            this.panelTop.Controls.Add(this.sizeLabelCounter);
            this.panelTop.Controls.Add(this.enableGPU);
            this.panelTop.Controls.Add(this.btnKeyboard);
            this.panelTop.Controls.Add(this.cbMonitors);
            this.panelTop.Controls.Add(this.btnHide);
            this.panelTop.Controls.Add(this.lblQualityShow);
            this.panelTop.Controls.Add(this.btnMouse);
            this.panelTop.Controls.Add(this.btnStart);
            this.panelTop.Controls.Add(this.btnStop);
            this.panelTop.Controls.Add(this.lblQuality);
            this.panelTop.Controls.Add(this.barQuality);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(784, 36);
            this.panelTop.TabIndex = 7;
            // 
            // btnStartProgramOnDisplay
            // 
            this.btnStartProgramOnDisplay.Image = global::Pulsar.Server.Properties.Resources.application_add;
            this.btnStartProgramOnDisplay.Location = new System.Drawing.Point(350, 3);
            this.btnStartProgramOnDisplay.Name = "btnStartProgramOnDisplay";
            this.btnStartProgramOnDisplay.Size = new System.Drawing.Size(47, 28);
            this.btnStartProgramOnDisplay.TabIndex = 18;
            this.btnStartProgramOnDisplay.UseVisualStyleBackColor = true;
            this.btnStartProgramOnDisplay.Click += new System.EventHandler(this.btnStartProgramOnDisplay_Click);
            // 
            // btnShowDrawingTools
            // 
            this.btnShowDrawingTools.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnShowDrawingTools.Image = global::Pulsar.Server.Properties.Resources.arrow_up;
            this.btnShowDrawingTools.Location = new System.Drawing.Point(694, 3);
            this.btnShowDrawingTools.Name = "btnShowDrawingTools";
            this.btnShowDrawingTools.Size = new System.Drawing.Size(28, 28);
            this.btnShowDrawingTools.TabIndex = 17;
            this.btnShowDrawingTools.TabStop = false;
            this.toolTipButtons.SetToolTip(this.btnShowDrawingTools, "Show drawing tools");
            this.btnShowDrawingTools.UseVisualStyleBackColor = true;
            this.btnShowDrawingTools.Click += new System.EventHandler(this.btnShowDrawingTools_Click);
            // 
            // sizeLabelCounter
            // 
            this.sizeLabelCounter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.sizeLabelCounter.Location = new System.Drawing.Point(694, 11);
            this.sizeLabelCounter.Name = "sizeLabelCounter";
            this.sizeLabelCounter.Size = new System.Drawing.Size(77, 15);
            this.sizeLabelCounter.TabIndex = 11;
            this.sizeLabelCounter.Text = "Size: ";
            // 
            // enableGPU
            // 
            this.enableGPU.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.enableGPU.Image = global::Pulsar.Server.Properties.Resources.computer_error;
            this.enableGPU.Location = new System.Drawing.Point(592, 4);
            this.enableGPU.Name = "enableGPU";
            this.enableGPU.Size = new System.Drawing.Size(28, 28);
            this.enableGPU.TabIndex = 10;
            this.enableGPU.TabStop = false;
            this.toolTipButtons.SetToolTip(this.enableGPU, "Enable mouse input.");
            this.enableGPU.UseVisualStyleBackColor = true;
            this.enableGPU.Click += new System.EventHandler(this.enableGPU_Click);
            // 
            // btnKeyboard
            // 
            this.btnKeyboard.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnKeyboard.Image = global::Pulsar.Server.Properties.Resources.keyboard_delete;
            this.btnKeyboard.Location = new System.Drawing.Point(660, 3);
            this.btnKeyboard.Name = "btnKeyboard";
            this.btnKeyboard.Size = new System.Drawing.Size(28, 28);
            this.btnKeyboard.TabIndex = 9;
            this.btnKeyboard.TabStop = false;
            this.toolTipButtons.SetToolTip(this.btnKeyboard, "Enable keyboard input.");
            this.btnKeyboard.UseVisualStyleBackColor = true;
            this.btnKeyboard.Click += new System.EventHandler(this.btnKeyboard_Click);
            // 
            // cbMonitors
            // 
            this.cbMonitors.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbMonitors.FormattingEnabled = true;
            this.cbMonitors.Location = new System.Drawing.Point(159, 5);
            this.cbMonitors.Name = "cbMonitors";
            this.cbMonitors.Size = new System.Drawing.Size(185, 21);
            this.cbMonitors.TabIndex = 8;
            this.cbMonitors.TabStop = false;
            // 
            // btnHide
            // 
            this.btnHide.Location = new System.Drawing.Point(403, 3);
            this.btnHide.Name = "btnHide";
            this.btnHide.Size = new System.Drawing.Size(54, 28);
            this.btnHide.TabIndex = 7;
            this.btnHide.TabStop = false;
            this.btnHide.Text = "Hide";
            this.btnHide.UseVisualStyleBackColor = true;
            this.btnHide.Click += new System.EventHandler(this.btnHide_Click);
            // 
            // panelDrawingTools
            // 
            this.panelDrawingTools.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelDrawingTools.Controls.Add(this.colorPicker);
            this.panelDrawingTools.Controls.Add(this.strokeWidthTrackBar);
            this.panelDrawingTools.Controls.Add(this.btnDrawing);
            this.panelDrawingTools.Controls.Add(this.btnEraser);
            this.panelDrawingTools.Controls.Add(this.btnClearDrawing);
            this.panelDrawingTools.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelDrawingTools.Location = new System.Drawing.Point(0, 36);
            this.panelDrawingTools.Name = "panelDrawingTools";
            this.panelDrawingTools.Size = new System.Drawing.Size(784, 36);
            this.panelDrawingTools.TabIndex = 8;
            this.panelDrawingTools.Visible = false;
            // 
            // colorPicker
            // 
            this.colorPicker.Location = new System.Drawing.Point(11, 3);
            this.colorPicker.Name = "colorPicker";
            this.colorPicker.Size = new System.Drawing.Size(60, 28);
            this.colorPicker.TabIndex = 12;
            this.colorPicker.TabStop = false;
            this.colorPicker.Text = "Color";
            this.toolTipButtons.SetToolTip(this.colorPicker, "Select drawing color");
            this.colorPicker.UseVisualStyleBackColor = true;
            // 
            // strokeWidthTrackBar
            // 
            this.strokeWidthTrackBar.Location = new System.Drawing.Point(275, 3);
            this.strokeWidthTrackBar.Minimum = 1;
            this.strokeWidthTrackBar.Name = "strokeWidthTrackBar";
            this.strokeWidthTrackBar.Size = new System.Drawing.Size(100, 45);
            this.strokeWidthTrackBar.TabIndex = 13;
            this.strokeWidthTrackBar.TabStop = false;
            this.toolTipButtons.SetToolTip(this.strokeWidthTrackBar, "Adjust stroke width");
            this.strokeWidthTrackBar.Value = 5;
            // 
            // btnDrawing
            // 
            this.btnDrawing.BackgroundImage = global::Pulsar.Server.Properties.Resources.pencil;
            this.btnDrawing.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnDrawing.Location = new System.Drawing.Point(77, 3);
            this.btnDrawing.Name = "btnDrawing";
            this.btnDrawing.Size = new System.Drawing.Size(60, 28);
            this.btnDrawing.TabIndex = 14;
            this.btnDrawing.TabStop = false;
            this.toolTipButtons.SetToolTip(this.btnDrawing, "Enable drawing");
            this.btnDrawing.UseVisualStyleBackColor = false;
            this.btnDrawing.Click += new System.EventHandler(this.btnDrawing_Click);
            // 
            // btnEraser
            // 
            this.btnEraser.BackgroundImage = global::Pulsar.Server.Properties.Resources.eraser;
            this.btnEraser.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnEraser.Location = new System.Drawing.Point(143, 3);
            this.btnEraser.Name = "btnEraser";
            this.btnEraser.Size = new System.Drawing.Size(60, 28);
            this.btnEraser.TabIndex = 15;
            this.btnEraser.TabStop = false;
            this.toolTipButtons.SetToolTip(this.btnEraser, "Enable eraser");
            this.btnEraser.UseVisualStyleBackColor = false;
            this.btnEraser.Click += new System.EventHandler(this.btnEraser_Click);
            // 
            // btnClearDrawing
            // 
            this.btnClearDrawing.BackgroundImage = global::Pulsar.Server.Properties.Resources.clear;
            this.btnClearDrawing.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnClearDrawing.Location = new System.Drawing.Point(209, 3);
            this.btnClearDrawing.Name = "btnClearDrawing";
            this.btnClearDrawing.Size = new System.Drawing.Size(60, 28);
            this.btnClearDrawing.TabIndex = 16;
            this.btnClearDrawing.TabStop = false;
            this.toolTipButtons.SetToolTip(this.btnClearDrawing, "Clear drawing");
            this.btnClearDrawing.UseVisualStyleBackColor = false;
            this.btnClearDrawing.Click += new System.EventHandler(this.btnClearDrawing_Click);
            // 
            // btnShow
            // 
            this.btnShow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnShow.Location = new System.Drawing.Point(730, 534);
            this.btnShow.Name = "btnShow";
            this.btnShow.Size = new System.Drawing.Size(54, 28);
            this.btnShow.TabIndex = 8;
            this.btnShow.TabStop = false;
            this.btnShow.Text = "Show";
            this.btnShow.UseVisualStyleBackColor = true;
            this.btnShow.Visible = false;
            this.btnShow.Click += new System.EventHandler(this.btnShow_Click);
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
            this.picDesktop.MouseDown += new System.Windows.Forms.MouseEventHandler(this.picDesktop_MouseDown);
            this.picDesktop.MouseMove += new System.Windows.Forms.MouseEventHandler(this.picDesktop_MouseMove);
            this.picDesktop.MouseUp += new System.Windows.Forms.MouseEventHandler(this.picDesktop_MouseUp);
            // 
            // FrmRemoteDesktop
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(784, 562);
            this.Controls.Add(this.btnShow);
            this.Controls.Add(this.panelDrawingTools);
            this.Controls.Add(this.panelTop);
            this.Controls.Add(this.picDesktop);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(640, 480);
            this.Name = "FrmRemoteDesktop";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Remote Desktop []";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmRemoteDesktop_FormClosing);
            this.Load += new System.EventHandler(this.FrmRemoteDesktop_Load);
            this.Resize += new System.EventHandler(this.FrmRemoteDesktop_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.barQuality)).EndInit();
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.panelDrawingTools.ResumeLayout(false);
            this.panelDrawingTools.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.strokeWidthTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picDesktop)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.TrackBar barQuality;
        private System.Windows.Forms.Label lblQuality;
        private System.Windows.Forms.Label lblQualityShow;
        private System.Windows.Forms.Button btnMouse;
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Button btnHide;
        private System.Windows.Forms.Button btnShow;
        private System.Windows.Forms.ComboBox cbMonitors;
        private System.Windows.Forms.Button btnKeyboard;
        private System.Windows.Forms.ToolTip toolTipButtons;
        private Controls.RapidPictureBox picDesktop;
        private System.Windows.Forms.Button enableGPU;
        private System.Windows.Forms.Label sizeLabelCounter;
        private System.Windows.Forms.Button colorPicker;
        private System.Windows.Forms.TrackBar strokeWidthTrackBar;
        private System.Windows.Forms.Button btnDrawing;
        private System.Windows.Forms.Button btnEraser;
        private System.Windows.Forms.Button btnClearDrawing;
        private System.Windows.Forms.Button btnShowDrawingTools;
        private System.Windows.Forms.Panel panelDrawingTools;
        private System.Windows.Forms.Button btnStartProgramOnDisplay;
    }
}