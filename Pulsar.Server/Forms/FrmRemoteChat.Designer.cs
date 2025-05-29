namespace Pulsar.Server.Forms
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
            this.noButtonTabControl1 = new Pulsar.Server.Controls.NoButtonTabControl();
            this.Settings = new System.Windows.Forms.TabPage();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.WelcomeMsgRB = new System.Windows.Forms.RichTextBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.DisableTypeChk = new System.Windows.Forms.CheckBox();
            this.DisableCloseChk = new System.Windows.Forms.CheckBox();
            this.TopMostChk = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.ChatTitleTB = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.Chat = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.NameTB = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.SendBTN = new System.Windows.Forms.Button();
            this.message = new System.Windows.Forms.TextBox();
            this.Chatlog = new System.Windows.Forms.RichTextBox();
            this.noButtonTabControl1.SuspendLayout();
            this.Settings.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.Chat.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // noButtonTabControl1
            // 
            this.noButtonTabControl1.Controls.Add(this.Settings);
            this.noButtonTabControl1.Controls.Add(this.Chat);
            this.noButtonTabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.noButtonTabControl1.Location = new System.Drawing.Point(0, 0);
            this.noButtonTabControl1.Name = "noButtonTabControl1";
            this.noButtonTabControl1.SelectedIndex = 0;
            this.noButtonTabControl1.Size = new System.Drawing.Size(364, 431);
            this.noButtonTabControl1.TabIndex = 1;
            // 
            // Settings
            // 
            this.Settings.Controls.Add(this.groupBox3);
            this.Settings.Controls.Add(this.groupBox4);
            this.Settings.Controls.Add(this.groupBox2);
            this.Settings.Controls.Add(this.groupBox1);
            this.Settings.Controls.Add(this.button1);
            this.Settings.Location = new System.Drawing.Point(4, 22);
            this.Settings.Name = "Settings";
            this.Settings.Padding = new System.Windows.Forms.Padding(3);
            this.Settings.Size = new System.Drawing.Size(356, 405);
            this.Settings.TabIndex = 0;
            this.Settings.Text = "Settings";
            this.Settings.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.WelcomeMsgRB);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox3.Location = new System.Drawing.Point(3, 87);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(350, 205);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Welcome Message";
            // 
            // WelcomeMsgRB
            // 
            this.WelcomeMsgRB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.WelcomeMsgRB.Location = new System.Drawing.Point(3, 16);
            this.WelcomeMsgRB.Name = "WelcomeMsgRB";
            this.WelcomeMsgRB.Size = new System.Drawing.Size(344, 186);
            this.WelcomeMsgRB.TabIndex = 0;
            this.WelcomeMsgRB.Text = "Hello!\nYou are currently speaking with the administrator.\nPlease do feel free to " +
    "ask for any support questions.";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.DisableTypeChk);
            this.groupBox4.Controls.Add(this.DisableCloseChk);
            this.groupBox4.Controls.Add(this.TopMostChk);
            this.groupBox4.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.groupBox4.Location = new System.Drawing.Point(3, 292);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(350, 87);
            this.groupBox4.TabIndex = 3;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Extra Options";
            // 
            // DisableTypeChk
            // 
            this.DisableTypeChk.AutoSize = true;
            this.DisableTypeChk.Checked = true;
            this.DisableTypeChk.CheckState = System.Windows.Forms.CheckState.Checked;
            this.DisableTypeChk.Location = new System.Drawing.Point(6, 42);
            this.DisableTypeChk.Name = "DisableTypeChk";
            this.DisableTypeChk.Size = new System.Drawing.Size(94, 17);
            this.DisableTypeChk.TabIndex = 2;
            this.DisableTypeChk.Text = "Enable Typing";
            this.DisableTypeChk.UseVisualStyleBackColor = true;
            // 
            // DisableCloseChk
            // 
            this.DisableCloseChk.AutoSize = true;
            this.DisableCloseChk.Location = new System.Drawing.Point(6, 65);
            this.DisableCloseChk.Name = "DisableCloseChk";
            this.DisableCloseChk.Size = new System.Drawing.Size(123, 17);
            this.DisableCloseChk.TabIndex = 1;
            this.DisableCloseChk.Text = "Disable Closing Chat";
            this.DisableCloseChk.UseVisualStyleBackColor = true;
            // 
            // TopMostChk
            // 
            this.TopMostChk.AutoSize = true;
            this.TopMostChk.Checked = true;
            this.TopMostChk.CheckState = System.Windows.Forms.CheckState.Checked;
            this.TopMostChk.Location = new System.Drawing.Point(6, 19);
            this.TopMostChk.Name = "TopMostChk";
            this.TopMostChk.Size = new System.Drawing.Size(71, 17);
            this.TopMostChk.TabIndex = 0;
            this.TopMostChk.Text = "Top Most";
            this.TopMostChk.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.ChatTitleTB);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox2.Location = new System.Drawing.Point(3, 45);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(350, 42);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Client Window Title";
            // 
            // ChatTitleTB
            // 
            this.ChatTitleTB.Dock = System.Windows.Forms.DockStyle.Top;
            this.ChatTitleTB.Location = new System.Drawing.Point(3, 16);
            this.ChatTitleTB.Name = "ChatTitleTB";
            this.ChatTitleTB.Size = new System.Drawing.Size(344, 20);
            this.ChatTitleTB.TabIndex = 0;
            this.ChatTitleTB.Text = "Currently chatting with support";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.textBox1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(350, 42);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Your Name";
            // 
            // textBox1
            // 
            this.textBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.textBox1.Location = new System.Drawing.Point(3, 16);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(344, 20);
            this.textBox1.TabIndex = 0;
            this.textBox1.Text = "Administrator";
            // 
            // button1
            // 
            this.button1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.button1.Location = new System.Drawing.Point(3, 379);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(350, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "Start Chat";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Chat
            // 
            this.Chat.Controls.Add(this.tableLayoutPanel1);
            this.Chat.Location = new System.Drawing.Point(4, 22);
            this.Chat.Margin = new System.Windows.Forms.Padding(0);
            this.Chat.Name = "Chat";
            this.Chat.Size = new System.Drawing.Size(356, 405);
            this.Chat.TabIndex = 1;
            this.Chat.Text = "Chat";
            this.Chat.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.Chatlog, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(356, 405);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.button2);
            this.flowLayoutPanel1.Controls.Add(this.button3);
            this.flowLayoutPanel1.Controls.Add(this.button4);
            this.flowLayoutPanel1.Controls.Add(this.NameTB);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 346);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(356, 30);
            this.flowLayoutPanel1.TabIndex = 1;
            // 
            // button2
            // 
            this.button2.Image = global::Pulsar.Server.Properties.Resources.broom;
            this.button2.Location = new System.Drawing.Point(3, 3);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(23, 23);
            this.button2.TabIndex = 0;
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Image = global::Pulsar.Server.Properties.Resources.save;
            this.button3.Location = new System.Drawing.Point(32, 3);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(23, 23);
            this.button3.TabIndex = 1;
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.Image = global::Pulsar.Server.Properties.Resources.shutdown;
            this.button4.Location = new System.Drawing.Point(61, 3);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(23, 23);
            this.button4.TabIndex = 2;
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // NameTB
            // 
            this.NameTB.Location = new System.Drawing.Point(90, 3);
            this.NameTB.Name = "NameTB";
            this.NameTB.Size = new System.Drawing.Size(263, 20);
            this.NameTB.TabIndex = 4;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 76F));
            this.tableLayoutPanel2.Controls.Add(this.SendBTN, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.message, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 376);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(356, 29);
            this.tableLayoutPanel2.TabIndex = 2;
            // 
            // SendBTN
            // 
            this.SendBTN.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SendBTN.Location = new System.Drawing.Point(283, 3);
            this.SendBTN.Name = "SendBTN";
            this.SendBTN.Size = new System.Drawing.Size(70, 23);
            this.SendBTN.TabIndex = 0;
            this.SendBTN.Text = "Send";
            this.SendBTN.UseVisualStyleBackColor = true;
            this.SendBTN.Click += new System.EventHandler(this.SendBTN_Click);
            // 
            // message
            // 
            this.message.Dock = System.Windows.Forms.DockStyle.Fill;
            this.message.Location = new System.Drawing.Point(3, 3);
            this.message.Name = "message";
            this.message.Size = new System.Drawing.Size(274, 20);
            this.message.TabIndex = 1;
            this.message.KeyDown += new System.Windows.Forms.KeyEventHandler(this.message_KeyDown);
            // 
            // Chatlog
            // 
            this.Chatlog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Chatlog.Location = new System.Drawing.Point(3, 3);
            this.Chatlog.Name = "Chatlog";
            this.Chatlog.ReadOnly = true;
            this.Chatlog.Size = new System.Drawing.Size(350, 340);
            this.Chatlog.TabIndex = 3;
            this.Chatlog.Text = "";
            // 
            // FrmRemoteChat
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(364, 431);
            this.Controls.Add(this.noButtonTabControl1);
            this.Name = "FrmRemoteChat";
            this.Text = "FrmRemoteChat";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmRemoteChat_FormClosing);
            this.Load += new System.EventHandler(this.FrmRemoteChat_Load);
            this.noButtonTabControl1.ResumeLayout(false);
            this.Settings.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.Chat.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.NoButtonTabControl noButtonTabControl1;
        private System.Windows.Forms.TabPage Settings;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.RichTextBox WelcomeMsgRB;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.CheckBox DisableTypeChk;
        private System.Windows.Forms.CheckBox DisableCloseChk;
        private System.Windows.Forms.CheckBox TopMostChk;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox ChatTitleTB;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TabPage Chat;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.TextBox NameTB;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Button SendBTN;
        private System.Windows.Forms.TextBox message;
        private System.Windows.Forms.RichTextBox Chatlog;
    }
}