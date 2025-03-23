namespace Quasar.Server.Forms
{
    partial class FrmRemoteScripting
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
            this.ExecBtn = new System.Windows.Forms.Button();
            this.TestBtn = new System.Windows.Forms.Button();
            this.dotNetBarTabControl1 = new Quasar.Server.Controls.DotNetBarTabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.PSEdit = new System.Windows.Forms.RichTextBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.BATEdit = new System.Windows.Forms.RichTextBox();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.VBSEdit = new System.Windows.Forms.RichTextBox();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.JSEdit = new System.Windows.Forms.RichTextBox();
            this.HidCheckBox = new System.Windows.Forms.CheckBox();
            this.dotNetBarTabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.SuspendLayout();
            // 
            // ExecBtn
            // 
            this.ExecBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ExecBtn.Location = new System.Drawing.Point(597, 399);
            this.ExecBtn.Name = "ExecBtn";
            this.ExecBtn.Size = new System.Drawing.Size(75, 23);
            this.ExecBtn.TabIndex = 1;
            this.ExecBtn.Text = "Execute";
            this.ExecBtn.UseVisualStyleBackColor = true;
            this.ExecBtn.Click += new System.EventHandler(this.ExecBtn_Click);
            // 
            // TestBtn
            // 
            this.TestBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.TestBtn.Location = new System.Drawing.Point(516, 399);
            this.TestBtn.Name = "TestBtn";
            this.TestBtn.Size = new System.Drawing.Size(75, 23);
            this.TestBtn.TabIndex = 2;
            this.TestBtn.Text = "Test";
            this.TestBtn.UseVisualStyleBackColor = true;
            this.TestBtn.Click += new System.EventHandler(this.TestBtn_Click);
            // 
            // dotNetBarTabControl1
            // 
            this.dotNetBarTabControl1.Alignment = System.Windows.Forms.TabAlignment.Left;
            this.dotNetBarTabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dotNetBarTabControl1.Controls.Add(this.tabPage1);
            this.dotNetBarTabControl1.Controls.Add(this.tabPage2);
            this.dotNetBarTabControl1.Controls.Add(this.tabPage3);
            this.dotNetBarTabControl1.Controls.Add(this.tabPage4);
            this.dotNetBarTabControl1.ItemSize = new System.Drawing.Size(44, 136);
            this.dotNetBarTabControl1.Location = new System.Drawing.Point(0, 0);
            this.dotNetBarTabControl1.Multiline = true;
            this.dotNetBarTabControl1.Name = "dotNetBarTabControl1";
            this.dotNetBarTabControl1.SelectedIndex = 0;
            this.dotNetBarTabControl1.Size = new System.Drawing.Size(684, 393);
            this.dotNetBarTabControl1.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.dotNetBarTabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.PSEdit);
            this.tabPage1.Location = new System.Drawing.Point(140, 4);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(540, 385);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Powershell";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // PSEdit
            // 
            this.PSEdit.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.PSEdit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PSEdit.Location = new System.Drawing.Point(3, 3);
            this.PSEdit.Name = "PSEdit";
            this.PSEdit.Size = new System.Drawing.Size(534, 379);
            this.PSEdit.TabIndex = 0;
            this.PSEdit.Text = "Write-Host \"Hello you are being administrated using Quasar Continuation\"\nPause";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.BATEdit);
            this.tabPage2.Location = new System.Drawing.Point(140, 4);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(540, 385);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Batch";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // BATEdit
            // 
            this.BATEdit.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.BATEdit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BATEdit.Location = new System.Drawing.Point(3, 3);
            this.BATEdit.Name = "BATEdit";
            this.BATEdit.Size = new System.Drawing.Size(534, 379);
            this.BATEdit.TabIndex = 1;
            this.BATEdit.Text = "@echo off\necho Hello you are being administrated using Quasar Continuation\npause";
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.VBSEdit);
            this.tabPage3.Location = new System.Drawing.Point(140, 4);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(540, 385);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "VBScript";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // VBSEdit
            // 
            this.VBSEdit.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.VBSEdit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.VBSEdit.Location = new System.Drawing.Point(3, 3);
            this.VBSEdit.Name = "VBSEdit";
            this.VBSEdit.Size = new System.Drawing.Size(534, 379);
            this.VBSEdit.TabIndex = 1;
            this.VBSEdit.Text = "MsgBox \"Hello you are being administrated using Quasar Continuation\"";
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.JSEdit);
            this.tabPage4.Location = new System.Drawing.Point(140, 4);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(540, 385);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "JavaScript";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // JSEdit
            // 
            this.JSEdit.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.JSEdit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.JSEdit.Location = new System.Drawing.Point(3, 3);
            this.JSEdit.Name = "JSEdit";
            this.JSEdit.Size = new System.Drawing.Size(534, 379);
            this.JSEdit.TabIndex = 1;
            this.JSEdit.Text = "alert(\"Hello you are being administrated using Quasar Continuation\");";
            // 
            // HidCheckBox
            // 
            this.HidCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.HidCheckBox.AutoSize = true;
            this.HidCheckBox.Location = new System.Drawing.Point(450, 403);
            this.HidCheckBox.Name = "HidCheckBox";
            this.HidCheckBox.Size = new System.Drawing.Size(60, 17);
            this.HidCheckBox.TabIndex = 3;
            this.HidCheckBox.Text = "Hidden";
            this.HidCheckBox.UseVisualStyleBackColor = true;
            // 
            // FrmRemoteScripting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(684, 431);
            this.Controls.Add(this.HidCheckBox);
            this.Controls.Add(this.TestBtn);
            this.Controls.Add(this.ExecBtn);
            this.Controls.Add(this.dotNetBarTabControl1);
            this.Name = "FrmRemoteScripting";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FrmRemoteScripting";
            this.Load += new System.EventHandler(this.FrmRemoteScripting_Load);
            this.dotNetBarTabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tabPage4.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Controls.DotNetBarTabControl dotNetBarTabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.RichTextBox PSEdit;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.RichTextBox BATEdit;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.RichTextBox VBSEdit;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.RichTextBox JSEdit;
        private System.Windows.Forms.Button ExecBtn;
        private System.Windows.Forms.Button TestBtn;
        private System.Windows.Forms.CheckBox HidCheckBox;
    }
}