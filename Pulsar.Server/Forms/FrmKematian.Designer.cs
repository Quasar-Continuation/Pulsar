namespace Pulsar.Server.Forms
{
    partial class FrmKematian
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
            Pulsar.Server.Utilities.ListViewColumnSorter listViewColumnSorter1 = new Pulsar.Server.Utilities.ListViewColumnSorter();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmKematian));
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.statusLabel = new System.Windows.Forms.Label();
            this.dotNetBarTabControl1 = new Pulsar.Server.Controls.DotNetBarTabControl();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.aeroListView1 = new Pulsar.Server.Controls.AeroListView();
            this.progressBar1 = new Pulsar.Server.Controls.BetterProgressBar();
            this.dotNetBarTabControl1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Omnes", 20F);
            this.label1.Location = new System.Drawing.Point(89, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(237, 38);
            this.label1.TabIndex = 0;
            this.label1.Text = "Kematian Grabbing";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 79);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "Start";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.start_Click);
            // 
            // button2
            // 
            this.button2.Enabled = false;
            this.button2.Location = new System.Drawing.Point(93, 79);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 2;
            this.button2.Text = "Cancel";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.cancel_Click);
            // 
            // statusLabel
            // 
            this.statusLabel.AutoSize = true;
            this.statusLabel.Font = new System.Drawing.Font("Omnes", 9F);
            this.statusLabel.Location = new System.Drawing.Point(13, 105);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(44, 17);
            this.statusLabel.TabIndex = 5;
            this.statusLabel.Text = "Status: ";
            // 
            // dotNetBarTabControl1
            // 
            this.dotNetBarTabControl1.Controls.Add(this.tabPage2);
            this.dotNetBarTabControl1.Location = new System.Drawing.Point(12, 311);
            this.dotNetBarTabControl1.Name = "dotNetBarTabControl1";
            this.dotNetBarTabControl1.SelectedIndex = 0;
            this.dotNetBarTabControl1.Size = new System.Drawing.Size(394, 150);
            this.dotNetBarTabControl1.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.dotNetBarTabControl1.TabIndex = 4;
            // 
            // tabPage2
            // 
            this.tabPage2.Location = new System.Drawing.Point(4, 25);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(386, 121);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // aeroListView1
            // 
            this.aeroListView1.FullRowSelect = true;
            this.aeroListView1.HideSelection = false;
            this.aeroListView1.Location = new System.Drawing.Point(12, 127);
            listViewColumnSorter1.NeedNumberCompare = false;
            listViewColumnSorter1.Order = System.Windows.Forms.SortOrder.None;
            listViewColumnSorter1.SortColumn = 0;
            this.aeroListView1.LvwColumnSorter = listViewColumnSorter1;
            this.aeroListView1.Name = "aeroListView1";
            this.aeroListView1.Size = new System.Drawing.Size(394, 178);
            this.aeroListView1.TabIndex = 3;
            this.aeroListView1.UseCompatibleStateImageBehavior = false;
            this.aeroListView1.View = System.Windows.Forms.View.Details;
            // 
            // progressBar1
            // 
            this.progressBar1.BackColor = System.Drawing.SystemColors.Control;
            this.progressBar1.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.progressBar1.Location = new System.Drawing.Point(12, 50);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.ProgressColor = System.Drawing.Color.FromArgb(((int)(((byte)(104)))), ((int)(((byte)(104)))), ((int)(((byte)(255)))));
            this.progressBar1.Size = new System.Drawing.Size(394, 23);
            this.progressBar1.TabIndex = 1;
            // 
            // FrmKematian
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(418, 473);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.dotNetBarTabControl1);
            this.Controls.Add(this.aeroListView1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FrmKematian";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Kematian";
            this.dotNetBarTabControl1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private Controls.BetterProgressBar progressBar1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private Controls.AeroListView aeroListView1;
        private System.Windows.Forms.TabPage tabPage2;
        private Controls.DotNetBarTabControl dotNetBarTabControl1;
        private System.Windows.Forms.Label statusLabel;
    }
}