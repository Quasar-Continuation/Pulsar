using Quasar.Server.Controls;

namespace Quasar.Server.Forms
{
    partial class FrmRemoteMic
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmRemoteMic));
            this.cbDevices = new System.Windows.Forms.ComboBox();
            this.toolTipButtons = new System.Windows.Forms.ToolTip(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.lblQuality = new System.Windows.Forms.Label();
            this.lblQualityShow = new System.Windows.Forms.Label();
            this.barQuality = new System.Windows.Forms.TrackBar();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.barQuality)).BeginInit();
            this.SuspendLayout();
            // 
            // cbDevices
            // 
            this.cbDevices.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbDevices.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbDevices.FormattingEnabled = true;
            this.cbDevices.Location = new System.Drawing.Point(9, 110);
            this.cbDevices.Name = "cbDevices";
            this.cbDevices.Size = new System.Drawing.Size(263, 21);
            this.cbDevices.TabIndex = 8;
            this.cbDevices.TabStop = false;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 94);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(82, 13);
            this.label1.TabIndex = 12;
            this.label1.Text = "Audio Devices:";
            // 
            // lblQuality
            // 
            this.lblQuality.AutoSize = true;
            this.lblQuality.Location = new System.Drawing.Point(111, 12);
            this.lblQuality.Name = "lblQuality";
            this.lblQuality.Size = new System.Drawing.Size(44, 13);
            this.lblQuality.TabIndex = 4;
            this.lblQuality.Text = "Bitrate:";
            // 
            // lblQualityShow
            // 
            this.lblQualityShow.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblQualityShow.AutoSize = true;
            this.lblQualityShow.Location = new System.Drawing.Point(175, 60);
            this.lblQualityShow.Name = "lblQualityShow";
            this.lblQualityShow.Size = new System.Drawing.Size(68, 13);
            this.lblQualityShow.TabIndex = 5;
            this.lblQualityShow.Text = "44100 (best)";
            // 
            // barQuality
            // 
            this.barQuality.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.barQuality.Location = new System.Drawing.Point(114, 28);
            this.barQuality.Maximum = 12;
            this.barQuality.Minimum = 1;
            this.barQuality.Name = "barQuality";
            this.barQuality.Size = new System.Drawing.Size(159, 45);
            this.barQuality.TabIndex = 3;
            this.barQuality.TabStop = false;
            this.barQuality.Value = 8;
            this.barQuality.Scroll += new System.EventHandler(this.barQuality_Scroll);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 13;
            this.button1.Text = "Start";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Enabled = false;
            this.button2.Location = new System.Drawing.Point(12, 41);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 14;
            this.button2.Text = "Stop";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // FrmRemoteMic
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(285, 145);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblQuality);
            this.Controls.Add(this.lblQualityShow);
            this.Controls.Add(this.barQuality);
            this.Controls.Add(this.cbDevices);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Name = "FrmRemoteMic";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Audio";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmAudio_FormClosing);
            this.Load += new System.EventHandler(this.FrmRemoteAudio_Load);
            this.Resize += new System.EventHandler(this.FrmAudio_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.barQuality)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ComboBox cbDevices;
        private System.Windows.Forms.ToolTip toolTipButtons;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblQuality;
        private System.Windows.Forms.Label lblQualityShow;
        private System.Windows.Forms.TrackBar barQuality;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
    }
}