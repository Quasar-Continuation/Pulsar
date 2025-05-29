namespace Pulsar.Client
{
    partial class FrmRemoteChat
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
            this.txtMessage = new System.Windows.Forms.TextBox();
            this.Sendpacket = new System.Windows.Forms.Button();
            this.txtMessages = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // txtMessage
            // 
            this.txtMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtMessage.Location = new System.Drawing.Point(12, 357);
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.Size = new System.Drawing.Size(349, 20);
            this.txtMessage.TabIndex = 0;
            this.txtMessage.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtMessage_KeyDown);
            // 
            // Sendpacket
            // 
            this.Sendpacket.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Sendpacket.Location = new System.Drawing.Point(367, 354);
            this.Sendpacket.Name = "Sendpacket";
            this.Sendpacket.Size = new System.Drawing.Size(75, 23);
            this.Sendpacket.TabIndex = 1;
            this.Sendpacket.Text = "Send";
            this.Sendpacket.UseVisualStyleBackColor = true;
            this.Sendpacket.Click += new System.EventHandler(this.Sendpacket_Click);
            // 
            // txtMessages
            // 
            this.txtMessages.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtMessages.Location = new System.Drawing.Point(12, 12);
            this.txtMessages.Name = "txtMessages";
            this.txtMessages.ReadOnly = true;
            this.txtMessages.Size = new System.Drawing.Size(430, 339);
            this.txtMessages.TabIndex = 2;
            this.txtMessages.Text = "";
            // 
            // FrmRemoteChat
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(454, 389);
            this.Controls.Add(this.txtMessages);
            this.Controls.Add(this.Sendpacket);
            this.Controls.Add(this.txtMessage);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmRemoteChat";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "FrmRemoteChat";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FrmRemoteChat_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.TextBox txtMessage;
        private System.Windows.Forms.Button Sendpacket;
        public System.Windows.Forms.RichTextBox txtMessages;
    }
}