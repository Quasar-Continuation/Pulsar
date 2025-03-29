namespace Quasar.Server.Forms
{
    partial class FrmRemoteSystemAudio
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
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.lblQuality = new System.Windows.Forms.Label();
            this.lblQualityShow = new System.Windows.Forms.Label();
            this.barQuality = new System.Windows.Forms.TrackBar();
            this.cbDevices = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.barQuality)).BeginInit();
            this.SuspendLayout();
            // 
            // button2
            // 
            this.button2.Enabled = false;
            this.button2.Location = new System.Drawing.Point(13, 42);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 21;
            this.button2.Text = "Stop";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(13, 13);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 20;
            this.button1.Text = "Start";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 95);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(79, 13);
            this.label1.TabIndex = 19;
            this.label1.Text = "Audio Devices:";
            // 
            // lblQuality
            // 
            this.lblQuality.AutoSize = true;
            this.lblQuality.Location = new System.Drawing.Point(112, 13);
            this.lblQuality.Name = "lblQuality";
            this.lblQuality.Size = new System.Drawing.Size(40, 13);
            this.lblQuality.TabIndex = 16;
            this.lblQuality.Text = "Bitrate:";
            // 
            // lblQualityShow
            // 
            this.lblQualityShow.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblQualityShow.AutoSize = true;
            this.lblQualityShow.Location = new System.Drawing.Point(176, 61);
            this.lblQualityShow.Name = "lblQualityShow";
            this.lblQualityShow.Size = new System.Drawing.Size(66, 13);
            this.lblQualityShow.TabIndex = 17;
            this.lblQualityShow.Text = "44100 (best)";
            // 
            // barQuality
            // 
            this.barQuality.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.barQuality.Location = new System.Drawing.Point(115, 29);
            this.barQuality.Maximum = 12;
            this.barQuality.Minimum = 1;
            this.barQuality.Name = "barQuality";
            this.barQuality.Size = new System.Drawing.Size(159, 45);
            this.barQuality.TabIndex = 15;
            this.barQuality.TabStop = false;
            this.barQuality.Value = 8;
            this.barQuality.Scroll += new System.EventHandler(this.barQuality_Scroll_1);
            // 
            // cbDevices
            // 
            this.cbDevices.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbDevices.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbDevices.FormattingEnabled = true;
            this.cbDevices.Location = new System.Drawing.Point(10, 111);
            this.cbDevices.Name = "cbDevices";
            this.cbDevices.Size = new System.Drawing.Size(263, 21);
            this.cbDevices.TabIndex = 18;
            this.cbDevices.TabStop = false;
            // 
            // FrmRemoteSystemAudio
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(285, 145);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblQuality);
            this.Controls.Add(this.lblQualityShow);
            this.Controls.Add(this.barQuality);
            this.Controls.Add(this.cbDevices);
            this.Name = "FrmRemoteSystemAudio";
            this.Text = "FrmRemoteSystemAudio";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmRemoteSystemAudio_FormClosing);
            this.Load += new System.EventHandler(this.FrmRemoteSystemAudio_Load);
            ((System.ComponentModel.ISupportInitialize)(this.barQuality)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblQuality;
        private System.Windows.Forms.Label lblQualityShow;
        private System.Windows.Forms.TrackBar barQuality;
        private System.Windows.Forms.ComboBox cbDevices;
    }
}