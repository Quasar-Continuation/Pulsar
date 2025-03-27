namespace Quasar.Server.Forms
{
    partial class FrmMemoryDump
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
            this.btnCancel = new System.Windows.Forms.Button();
            this.progressDownload = new System.Windows.Forms.ProgressBar();
            this.labelProgress = new System.Windows.Forms.Label();
            this.labelStatus = new System.Windows.Forms.Label();
            this.labelValue = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(653, 14);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 28);
            this.btnCancel.TabIndex = 0;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // progressDownload
            // 
            this.progressDownload.Location = new System.Drawing.Point(17, 14);
            this.progressDownload.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.progressDownload.Name = "progressDownload";
            this.progressDownload.Size = new System.Drawing.Size(483, 28);
            this.progressDownload.TabIndex = 1;
            // 
            // labelProgress
            // 
            this.labelProgress.AutoSize = true;
            this.labelProgress.Location = new System.Drawing.Point(508, 20);
            this.labelProgress.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelProgress.Name = "labelProgress";
            this.labelProgress.Size = new System.Drawing.Size(87, 16);
            this.labelProgress.TabIndex = 2;
            this.labelProgress.Text = "Progress: 0%";
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new System.Drawing.Point(17, 50);
            this.labelStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(47, 16);
            this.labelStatus.TabIndex = 3;
            this.labelStatus.Text = "Status:";
            // 
            // labelValue
            // 
            this.labelValue.AutoSize = true;
            this.labelValue.Location = new System.Drawing.Point(79, 50);
            this.labelValue.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelValue.Name = "labelValue";
            this.labelValue.Size = new System.Drawing.Size(66, 16);
            this.labelValue.TabIndex = 4;
            this.labelValue.Text = "Pending...";
            // 
            // FrmMemoryDump
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(769, 84);
            this.Controls.Add(this.labelValue);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.labelProgress);
            this.Controls.Add(this.progressDownload);
            this.Controls.Add(this.btnCancel);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "FrmMemoryDump";
            this.Text = "Memory Dump []";
            this.Load += new System.EventHandler(this.FrmMemoryDump_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.ProgressBar progressDownload;
        private System.Windows.Forms.Label labelProgress;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.Label labelValue;
    }
}