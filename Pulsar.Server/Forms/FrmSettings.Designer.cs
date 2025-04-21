namespace Pulsar.Server.Forms
{
    partial class FrmSettings
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmSettings));
            this.btnSave = new System.Windows.Forms.Button();
            this.lblPort = new System.Windows.Forms.Label();
            this.ncPort = new System.Windows.Forms.NumericUpDown();
            this.chkAutoListen = new System.Windows.Forms.CheckBox();
            this.chkPopup = new System.Windows.Forms.CheckBox();
            this.btnListen = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.chkUseUpnp = new System.Windows.Forms.CheckBox();
            this.chkShowTooltip = new System.Windows.Forms.CheckBox();
            this.chkNoIPIntegration = new System.Windows.Forms.CheckBox();
            this.lblHost = new System.Windows.Forms.Label();
            this.lblPass = new System.Windows.Forms.Label();
            this.lblUser = new System.Windows.Forms.Label();
            this.txtNoIPPass = new System.Windows.Forms.TextBox();
            this.txtNoIPUser = new System.Windows.Forms.TextBox();
            this.txtNoIPHost = new System.Windows.Forms.TextBox();
            this.chkShowPassword = new System.Windows.Forms.CheckBox();
            this.chkIPv6Support = new System.Windows.Forms.CheckBox();
            this.chkDarkMode = new System.Windows.Forms.CheckBox();
            this.chkEventLog = new System.Windows.Forms.CheckBox();
            this.chkDiscordRPC = new System.Windows.Forms.CheckBox();
            this.chkTelegramNotis = new System.Windows.Forms.CheckBox();
            this.txtTelegramToken = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtTelegramChatID = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.BlockedRichTB = new System.Windows.Forms.RichTextBox();
            this.hideFromScreenCapture = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.ncPort)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Location = new System.Drawing.Point(227, 617);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 19;
            this.btnSave.Text = "&Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // lblPort
            // 
            this.lblPort.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblPort.AutoSize = true;
            this.lblPort.Location = new System.Drawing.Point(12, 11);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(93, 13);
            this.lblPort.TabIndex = 0;
            this.lblPort.Text = "Port to listen on:";
            // 
            // ncPort
            // 
            this.ncPort.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ncPort.Location = new System.Drawing.Point(111, 7);
            this.ncPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.ncPort.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.ncPort.Name = "ncPort";
            this.ncPort.Size = new System.Drawing.Size(75, 22);
            this.ncPort.TabIndex = 1;
            this.ncPort.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // chkAutoListen
            // 
            this.chkAutoListen.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkAutoListen.AutoSize = true;
            this.chkAutoListen.Location = new System.Drawing.Point(12, 128);
            this.chkAutoListen.Name = "chkAutoListen";
            this.chkAutoListen.Size = new System.Drawing.Size(222, 17);
            this.chkAutoListen.TabIndex = 6;
            this.chkAutoListen.Text = "Listen for new connections on startup";
            this.chkAutoListen.UseVisualStyleBackColor = true;
            // 
            // chkPopup
            // 
            this.chkPopup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkPopup.AutoSize = true;
            this.chkPopup.Location = new System.Drawing.Point(12, 151);
            this.chkPopup.Name = "chkPopup";
            this.chkPopup.Size = new System.Drawing.Size(259, 17);
            this.chkPopup.TabIndex = 7;
            this.chkPopup.Text = "Show popup notification on new connection";
            this.chkPopup.UseVisualStyleBackColor = true;
            // 
            // btnListen
            // 
            this.btnListen.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnListen.Location = new System.Drawing.Point(192, 6);
            this.btnListen.Name = "btnListen";
            this.btnListen.Size = new System.Drawing.Size(110, 23);
            this.btnListen.TabIndex = 2;
            this.btnListen.Text = "Start listening";
            this.btnListen.UseVisualStyleBackColor = true;
            this.btnListen.Click += new System.EventHandler(this.btnListen_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Location = new System.Drawing.Point(146, 617);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 18;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // chkUseUpnp
            // 
            this.chkUseUpnp.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkUseUpnp.AutoSize = true;
            this.chkUseUpnp.Location = new System.Drawing.Point(12, 174);
            this.chkUseUpnp.Name = "chkUseUpnp";
            this.chkUseUpnp.Size = new System.Drawing.Size(249, 17);
            this.chkUseUpnp.TabIndex = 8;
            this.chkUseUpnp.Text = "Try to automatically forward the port (UPnP)";
            this.chkUseUpnp.UseVisualStyleBackColor = true;
            // 
            // chkShowTooltip
            // 
            this.chkShowTooltip.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkShowTooltip.AutoSize = true;
            this.chkShowTooltip.Location = new System.Drawing.Point(12, 197);
            this.chkShowTooltip.Name = "chkShowTooltip";
            this.chkShowTooltip.Size = new System.Drawing.Size(268, 17);
            this.chkShowTooltip.TabIndex = 9;
            this.chkShowTooltip.Text = "Show tooltip on client with system information";
            this.chkShowTooltip.UseVisualStyleBackColor = true;
            // 
            // chkNoIPIntegration
            // 
            this.chkNoIPIntegration.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkNoIPIntegration.AutoSize = true;
            this.chkNoIPIntegration.Location = new System.Drawing.Point(12, 509);
            this.chkNoIPIntegration.Name = "chkNoIPIntegration";
            this.chkNoIPIntegration.Size = new System.Drawing.Size(187, 17);
            this.chkNoIPIntegration.TabIndex = 10;
            this.chkNoIPIntegration.Text = "Enable No-Ip.com DNS Updater";
            this.chkNoIPIntegration.UseVisualStyleBackColor = true;
            this.chkNoIPIntegration.CheckedChanged += new System.EventHandler(this.chkNoIPIntegration_CheckedChanged);
            // 
            // lblHost
            // 
            this.lblHost.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblHost.AutoSize = true;
            this.lblHost.Enabled = false;
            this.lblHost.Location = new System.Drawing.Point(30, 535);
            this.lblHost.Name = "lblHost";
            this.lblHost.Size = new System.Drawing.Size(34, 13);
            this.lblHost.TabIndex = 11;
            this.lblHost.Text = "Host:";
            // 
            // lblPass
            // 
            this.lblPass.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblPass.AutoSize = true;
            this.lblPass.Enabled = false;
            this.lblPass.Location = new System.Drawing.Point(167, 563);
            this.lblPass.Name = "lblPass";
            this.lblPass.Size = new System.Drawing.Size(32, 13);
            this.lblPass.TabIndex = 15;
            this.lblPass.Text = "Pass:";
            // 
            // lblUser
            // 
            this.lblUser.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblUser.AutoSize = true;
            this.lblUser.Enabled = false;
            this.lblUser.Location = new System.Drawing.Point(30, 563);
            this.lblUser.Name = "lblUser";
            this.lblUser.Size = new System.Drawing.Size(32, 13);
            this.lblUser.TabIndex = 13;
            this.lblUser.Text = "Mail:";
            // 
            // txtNoIPPass
            // 
            this.txtNoIPPass.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtNoIPPass.Enabled = false;
            this.txtNoIPPass.Location = new System.Drawing.Point(199, 560);
            this.txtNoIPPass.Name = "txtNoIPPass";
            this.txtNoIPPass.Size = new System.Drawing.Size(63, 22);
            this.txtNoIPPass.TabIndex = 16;
            // 
            // txtNoIPUser
            // 
            this.txtNoIPUser.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtNoIPUser.Enabled = false;
            this.txtNoIPUser.Location = new System.Drawing.Point(70, 560);
            this.txtNoIPUser.Name = "txtNoIPUser";
            this.txtNoIPUser.Size = new System.Drawing.Size(91, 22);
            this.txtNoIPUser.TabIndex = 14;
            // 
            // txtNoIPHost
            // 
            this.txtNoIPHost.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtNoIPHost.Enabled = false;
            this.txtNoIPHost.Location = new System.Drawing.Point(70, 532);
            this.txtNoIPHost.Name = "txtNoIPHost";
            this.txtNoIPHost.Size = new System.Drawing.Size(192, 22);
            this.txtNoIPHost.TabIndex = 12;
            this.txtNoIPHost.TextChanged += new System.EventHandler(this.txtNoIPHost_TextChanged);
            // 
            // chkShowPassword
            // 
            this.chkShowPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkShowPassword.AutoSize = true;
            this.chkShowPassword.Enabled = false;
            this.chkShowPassword.Location = new System.Drawing.Point(192, 588);
            this.chkShowPassword.Name = "chkShowPassword";
            this.chkShowPassword.Size = new System.Drawing.Size(107, 17);
            this.chkShowPassword.TabIndex = 17;
            this.chkShowPassword.Text = "Show Password";
            this.chkShowPassword.UseVisualStyleBackColor = true;
            this.chkShowPassword.CheckedChanged += new System.EventHandler(this.chkShowPassword_CheckedChanged);
            // 
            // chkIPv6Support
            // 
            this.chkIPv6Support.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkIPv6Support.AutoSize = true;
            this.chkIPv6Support.Location = new System.Drawing.Point(12, 105);
            this.chkIPv6Support.Name = "chkIPv6Support";
            this.chkIPv6Support.Size = new System.Drawing.Size(128, 17);
            this.chkIPv6Support.TabIndex = 5;
            this.chkIPv6Support.Text = "Enable IPv6 support";
            this.chkIPv6Support.UseVisualStyleBackColor = true;
            // 
            // chkDarkMode
            // 
            this.chkDarkMode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkDarkMode.AutoSize = true;
            this.chkDarkMode.Checked = true;
            this.chkDarkMode.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDarkMode.Location = new System.Drawing.Point(12, 59);
            this.chkDarkMode.Name = "chkDarkMode";
            this.chkDarkMode.Size = new System.Drawing.Size(83, 17);
            this.chkDarkMode.TabIndex = 20;
            this.chkDarkMode.Text = "Dark Mode";
            this.chkDarkMode.UseVisualStyleBackColor = true;
            // 
            // chkEventLog
            // 
            this.chkEventLog.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkEventLog.AutoSize = true;
            this.chkEventLog.Location = new System.Drawing.Point(12, 220);
            this.chkEventLog.Name = "chkEventLog";
            this.chkEventLog.Size = new System.Drawing.Size(186, 17);
            this.chkEventLog.TabIndex = 21;
            this.chkEventLog.Text = "Show event log and debug log";
            this.chkEventLog.UseVisualStyleBackColor = true;
            // 
            // chkDiscordRPC
            // 
            this.chkDiscordRPC.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkDiscordRPC.AutoSize = true;
            this.chkDiscordRPC.Checked = true;
            this.chkDiscordRPC.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDiscordRPC.Location = new System.Drawing.Point(12, 36);
            this.chkDiscordRPC.Name = "chkDiscordRPC";
            this.chkDiscordRPC.Size = new System.Drawing.Size(88, 17);
            this.chkDiscordRPC.TabIndex = 22;
            this.chkDiscordRPC.Text = "Discord RPC";
            this.chkDiscordRPC.UseVisualStyleBackColor = true;
            this.chkDiscordRPC.CheckedChanged += new System.EventHandler(this.chkDiscordRPC_CheckedChanged);
            // 
            // chkTelegramNotis
            // 
            this.chkTelegramNotis.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkTelegramNotis.AutoSize = true;
            this.chkTelegramNotis.Location = new System.Drawing.Point(12, 251);
            this.chkTelegramNotis.Name = "chkTelegramNotis";
            this.chkTelegramNotis.Size = new System.Drawing.Size(178, 17);
            this.chkTelegramNotis.TabIndex = 23;
            this.chkTelegramNotis.Text = "Enable Telegram Notifications";
            this.chkTelegramNotis.UseVisualStyleBackColor = true;
            this.chkTelegramNotis.CheckedChanged += new System.EventHandler(this.chkTelegramNotis_CheckedChanged);
            // 
            // txtTelegramToken
            // 
            this.txtTelegramToken.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTelegramToken.Enabled = false;
            this.txtTelegramToken.Location = new System.Drawing.Point(70, 274);
            this.txtTelegramToken.Name = "txtTelegramToken";
            this.txtTelegramToken.Size = new System.Drawing.Size(232, 22);
            this.txtTelegramToken.TabIndex = 24;
            this.txtTelegramToken.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Enabled = false;
            this.label1.Location = new System.Drawing.Point(20, 277);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(43, 13);
            this.label1.TabIndex = 25;
            this.label1.Text = "Token: ";
            // 
            // txtTelegramChatID
            // 
            this.txtTelegramChatID.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTelegramChatID.Enabled = false;
            this.txtTelegramChatID.Location = new System.Drawing.Point(70, 302);
            this.txtTelegramChatID.Name = "txtTelegramChatID";
            this.txtTelegramChatID.Size = new System.Drawing.Size(232, 22);
            this.txtTelegramChatID.TabIndex = 26;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Enabled = false;
            this.label2.Location = new System.Drawing.Point(19, 305);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(45, 13);
            this.label2.TabIndex = 27;
            this.label2.Text = "ChatID:";
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(192, 330);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(110, 23);
            this.button1.TabIndex = 28;
            this.button1.Text = "Test";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.groupBox1.Controls.Add(this.BlockedRichTB);
            this.groupBox1.Location = new System.Drawing.Point(12, 355);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(290, 137);
            this.groupBox1.TabIndex = 30;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Blocked IP\'s";
            // 
            // BlockedRichTB
            // 
            this.BlockedRichTB.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.BlockedRichTB.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.BlockedRichTB.Location = new System.Drawing.Point(3, 18);
            this.BlockedRichTB.Name = "BlockedRichTB";
            this.BlockedRichTB.Size = new System.Drawing.Size(284, 116);
            this.BlockedRichTB.TabIndex = 0;
            this.BlockedRichTB.Text = "";
            // 
            // checkBox1
            // 
            this.hideFromScreenCapture.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.hideFromScreenCapture.AutoSize = true;
            this.hideFromScreenCapture.Location = new System.Drawing.Point(12, 82);
            this.hideFromScreenCapture.Name = "checkBox1";
            this.hideFromScreenCapture.Size = new System.Drawing.Size(189, 17);
            this.hideFromScreenCapture.TabIndex = 31;
            this.hideFromScreenCapture.Text = "Hide Pulsar from screen capture";
            this.hideFromScreenCapture.UseVisualStyleBackColor = true;
            this.hideFromScreenCapture.CheckedChanged += new System.EventHandler(this.hideFromScreenCapture_CheckedChanged);
            // 
            // FrmSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(314, 652);
            this.Controls.Add(this.hideFromScreenCapture);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtTelegramChatID);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtTelegramToken);
            this.Controls.Add(this.chkTelegramNotis);
            this.Controls.Add(this.chkDiscordRPC);
            this.Controls.Add(this.chkEventLog);
            this.Controls.Add(this.chkDarkMode);
            this.Controls.Add(this.chkIPv6Support);
            this.Controls.Add(this.chkShowPassword);
            this.Controls.Add(this.txtNoIPHost);
            this.Controls.Add(this.txtNoIPUser);
            this.Controls.Add(this.txtNoIPPass);
            this.Controls.Add(this.lblUser);
            this.Controls.Add(this.lblPass);
            this.Controls.Add(this.lblHost);
            this.Controls.Add(this.chkNoIPIntegration);
            this.Controls.Add(this.chkShowTooltip);
            this.Controls.Add(this.chkUseUpnp);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnListen);
            this.Controls.Add(this.chkPopup);
            this.Controls.Add(this.chkAutoListen);
            this.Controls.Add(this.ncPort);
            this.Controls.Add(this.lblPort);
            this.Controls.Add(this.btnSave);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmSettings";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Settings";
            this.Load += new System.EventHandler(this.FrmSettings_Load);
            ((System.ComponentModel.ISupportInitialize)(this.ncPort)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Label lblPort;
        private System.Windows.Forms.NumericUpDown ncPort;
        private System.Windows.Forms.CheckBox chkAutoListen;
        private System.Windows.Forms.CheckBox chkPopup;
        private System.Windows.Forms.Button btnListen;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.CheckBox chkUseUpnp;
        private System.Windows.Forms.CheckBox chkShowTooltip;
        private System.Windows.Forms.CheckBox chkNoIPIntegration;
        private System.Windows.Forms.Label lblHost;
        private System.Windows.Forms.Label lblPass;
        private System.Windows.Forms.Label lblUser;
        private System.Windows.Forms.TextBox txtNoIPPass;
        private System.Windows.Forms.TextBox txtNoIPUser;
        private System.Windows.Forms.TextBox txtNoIPHost;
        private System.Windows.Forms.CheckBox chkShowPassword;
        private System.Windows.Forms.CheckBox chkIPv6Support;
        private System.Windows.Forms.CheckBox chkDarkMode;
        private System.Windows.Forms.CheckBox chkEventLog;
        private System.Windows.Forms.CheckBox chkDiscordRPC;
        private System.Windows.Forms.CheckBox chkTelegramNotis;
        private System.Windows.Forms.TextBox txtTelegramToken;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtTelegramChatID;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RichTextBox BlockedRichTB;
        private System.Windows.Forms.CheckBox hideFromScreenCapture;
    }
}