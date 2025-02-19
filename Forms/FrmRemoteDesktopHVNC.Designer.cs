namespace Quasar.Server.Forms
{
    partial class FrmRemoteDesktopHVNC
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
            this.picDesktop = new Quasar.Server.Controls.RapidPictureBox();
            this.lblQualityShow = new System.Windows.Forms.Label();
            this.btnMouse = new System.Windows.Forms.Button();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.lblQuality = new System.Windows.Forms.Label();
            this.barQuality = new System.Windows.Forms.TrackBar();
            this.panelTop = new System.Windows.Forms.Panel();
            this.btnKeyboard = new System.Windows.Forms.Button();
            this.cbMonitors = new System.Windows.Forms.ComboBox();
            this.btnHide = new System.Windows.Forms.Button();
            this.btnShow = new System.Windows.Forms.Button();
            this.toolTipButtons = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.picDesktop)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.barQuality)).BeginInit();
            this.panelTop.SuspendLayout();
            this.SuspendLayout();
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
            this.picDesktop.TabIndex = 9;
            this.picDesktop.TabStop = false;
            // 
            // lblQualityShow
            // 
            this.lblQualityShow.AutoSize = true;
            this.lblQualityShow.Location = new System.Drawing.Point(230, 31);
            this.lblQualityShow.Name = "lblQualityShow";
            this.lblQualityShow.Size = new System.Drawing.Size(48, 13);
            this.lblQualityShow.TabIndex = 5;
            this.lblQualityShow.Text = "75 (high)";
            // 
            // btnMouse
            // 
            this.btnMouse.Image = global::Quasar.Server.Properties.Resources.mouse_delete;
            this.btnMouse.Location = new System.Drawing.Point(302, 5);
            this.btnMouse.Name = "btnMouse";
            this.btnMouse.Size = new System.Drawing.Size(28, 28);
            this.btnMouse.TabIndex = 6;
            this.btnMouse.TabStop = false;
            this.toolTipButtons.SetToolTip(this.btnMouse, "Enable mouse input.");
            this.btnMouse.UseVisualStyleBackColor = true;
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
            // lblQuality
            // 
            this.lblQuality.AutoSize = true;
            this.lblQuality.Location = new System.Drawing.Point(178, 10);
            this.lblQuality.Name = "lblQuality";
            this.lblQuality.Size = new System.Drawing.Size(42, 13);
            this.lblQuality.TabIndex = 4;
            this.lblQuality.Text = "Quality:";
            // 
            // barQuality
            // 
            this.barQuality.Location = new System.Drawing.Point(219, -1);
            this.barQuality.Maximum = 100;
            this.barQuality.Minimum = 1;
            this.barQuality.Name = "barQuality";
            this.barQuality.Size = new System.Drawing.Size(76, 45);
            this.barQuality.TabIndex = 3;
            this.barQuality.TabStop = false;
            this.barQuality.Value = 75;
            // 
            // panelTop
            // 
            this.panelTop.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelTop.Controls.Add(this.btnKeyboard);
            this.panelTop.Controls.Add(this.cbMonitors);
            this.panelTop.Controls.Add(this.btnHide);
            this.panelTop.Controls.Add(this.lblQualityShow);
            this.panelTop.Controls.Add(this.btnMouse);
            this.panelTop.Controls.Add(this.btnStart);
            this.panelTop.Controls.Add(this.btnStop);
            this.panelTop.Controls.Add(this.lblQuality);
            this.panelTop.Controls.Add(this.barQuality);
            this.panelTop.Location = new System.Drawing.Point(192, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(384, 59);
            this.panelTop.TabIndex = 10;
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
            // cbMonitors
            // 
            this.cbMonitors.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbMonitors.FormattingEnabled = true;
            this.cbMonitors.Location = new System.Drawing.Point(15, 30);
            this.cbMonitors.Name = "cbMonitors";
            this.cbMonitors.Size = new System.Drawing.Size(149, 21);
            this.cbMonitors.TabIndex = 8;
            this.cbMonitors.TabStop = false;
            // 
            // btnHide
            // 
            this.btnHide.Location = new System.Drawing.Point(170, 30);
            this.btnHide.Name = "btnHide";
            this.btnHide.Size = new System.Drawing.Size(54, 22);
            this.btnHide.TabIndex = 7;
            this.btnHide.TabStop = false;
            this.btnHide.Text = "Hide";
            this.btnHide.UseVisualStyleBackColor = true;
            // 
            // btnShow
            // 
            this.btnShow.Location = new System.Drawing.Point(0, -4);
            this.btnShow.Name = "btnShow";
            this.btnShow.Size = new System.Drawing.Size(54, 28);
            this.btnShow.TabIndex = 11;
            this.btnShow.TabStop = false;
            this.btnShow.Text = "Show";
            this.btnShow.UseVisualStyleBackColor = true;
            this.btnShow.Visible = false;
            // 
            // FrmRemoteDesktopHVNC
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 562);
            this.Controls.Add(this.panelTop);
            this.Controls.Add(this.btnShow);
            this.Controls.Add(this.picDesktop);
            this.MinimumSize = new System.Drawing.Size(640, 480);
            this.Name = "FrmRemoteDesktopHVNC";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FrmRemoteDesktopHVNC";
            ((System.ComponentModel.ISupportInitialize)(this.picDesktop)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.barQuality)).EndInit();
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
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
    }
}