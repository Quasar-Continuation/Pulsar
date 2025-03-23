<<<<<<< HEAD
﻿namespace Quasar.Server.Forms
{
    partial class FrmKeywords
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
            this.NotiRichTextBox = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SaveNoti = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // NotiRichTextBox
            // 
            this.NotiRichTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.NotiRichTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.NotiRichTextBox.Location = new System.Drawing.Point(12, 25);
            this.NotiRichTextBox.Name = "NotiRichTextBox";
            this.NotiRichTextBox.Size = new System.Drawing.Size(445, 261);
            this.NotiRichTextBox.TabIndex = 0;
            this.NotiRichTextBox.Text = "";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(74, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Key-words list.";
            // 
            // SaveNoti
            // 
            this.SaveNoti.Location = new System.Drawing.Point(382, 292);
            this.SaveNoti.Name = "SaveNoti";
            this.SaveNoti.Size = new System.Drawing.Size(75, 23);
            this.SaveNoti.TabIndex = 2;
            this.SaveNoti.Text = "Save";
            this.SaveNoti.UseVisualStyleBackColor = true;
            this.SaveNoti.Click += new System.EventHandler(this.SaveNoti_Click);
            // 
            // FrmKeywords
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(469, 322);
            this.Controls.Add(this.SaveNoti);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.NotiRichTextBox);
            this.Name = "FrmKeywords";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Notification Centre | Key-words";
            this.Load += new System.EventHandler(this.FrmKeywords_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox NotiRichTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button SaveNoti;
    }
=======
﻿namespace Quasar.Server.Forms
{
    partial class FrmKeywords
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
            this.NotiRichTextBox = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SaveNoti = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // NotiRichTextBox
            // 
            this.NotiRichTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.NotiRichTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.NotiRichTextBox.Location = new System.Drawing.Point(12, 25);
            this.NotiRichTextBox.Name = "NotiRichTextBox";
            this.NotiRichTextBox.Size = new System.Drawing.Size(445, 261);
            this.NotiRichTextBox.TabIndex = 0;
            this.NotiRichTextBox.Text = "";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(74, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Key-words list.";
            // 
            // SaveNoti
            // 
            this.SaveNoti.Location = new System.Drawing.Point(382, 292);
            this.SaveNoti.Name = "SaveNoti";
            this.SaveNoti.Size = new System.Drawing.Size(75, 23);
            this.SaveNoti.TabIndex = 2;
            this.SaveNoti.Text = "Save";
            this.SaveNoti.UseVisualStyleBackColor = true;
            this.SaveNoti.Click += new System.EventHandler(this.SaveNoti_Click);
            // 
            // FrmKeywords
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(469, 322);
            this.Controls.Add(this.SaveNoti);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.NotiRichTextBox);
            this.Name = "FrmKeywords";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Notification Centre | Key-words";
            this.Load += new System.EventHandler(this.FrmKeywords_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox NotiRichTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button SaveNoti;
    }
>>>>>>> d1562c487ffbb93b7d062a71485785771a87ce11
}