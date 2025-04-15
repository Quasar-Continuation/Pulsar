namespace Pulsar.Server.Forms
{
    partial class FrmLoot
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
            Pulsar.Server.Utilities.ListViewColumnSorter listViewColumnSorter2 = new Pulsar.Server.Utilities.ListViewColumnSorter();
            Pulsar.Server.Utilities.ListViewColumnSorter listViewColumnSorter3 = new Pulsar.Server.Utilities.ListViewColumnSorter();
            Pulsar.Server.Utilities.ListViewColumnSorter listViewColumnSorter4 = new Pulsar.Server.Utilities.ListViewColumnSorter();
            this.dotNetBarTabControl1 = new Pulsar.Server.Controls.DotNetBarTabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.aeroListView1 = new Pulsar.Server.Controls.AeroListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.aeroListView2 = new Pulsar.Server.Controls.AeroListView();
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.aeroListView3 = new Pulsar.Server.Controls.AeroListView();
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.aeroListView4 = new Pulsar.Server.Controls.AeroListView();
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.dotNetBarTabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.SuspendLayout();
            // 
            // dotNetBarTabControl1
            // 
            this.dotNetBarTabControl1.Alignment = System.Windows.Forms.TabAlignment.Left;
            this.dotNetBarTabControl1.Controls.Add(this.tabPage1);
            this.dotNetBarTabControl1.Controls.Add(this.tabPage2);
            this.dotNetBarTabControl1.Controls.Add(this.tabPage3);
            this.dotNetBarTabControl1.Controls.Add(this.tabPage4);
            this.dotNetBarTabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dotNetBarTabControl1.ItemSize = new System.Drawing.Size(44, 136);
            this.dotNetBarTabControl1.Location = new System.Drawing.Point(0, 0);
            this.dotNetBarTabControl1.Multiline = true;
            this.dotNetBarTabControl1.Name = "dotNetBarTabControl1";
            this.dotNetBarTabControl1.SelectedIndex = 0;
            this.dotNetBarTabControl1.Size = new System.Drawing.Size(757, 396);
            this.dotNetBarTabControl1.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.dotNetBarTabControl1.TabIndex = 0;
            this.dotNetBarTabControl1.ShowCloseButtons = false;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.aeroListView1);
            this.tabPage1.Location = new System.Drawing.Point(140, 4);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(613, 388);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Browsers";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // aeroListView1
            // 
            this.aeroListView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.aeroListView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.aeroListView1.FullRowSelect = true;
            this.aeroListView1.HideSelection = false;
            this.aeroListView1.Location = new System.Drawing.Point(3, 3);
            listViewColumnSorter1.NeedNumberCompare = false;
            listViewColumnSorter1.Order = System.Windows.Forms.SortOrder.None;
            listViewColumnSorter1.SortColumn = 0;
            this.aeroListView1.LvwColumnSorter = listViewColumnSorter1;
            this.aeroListView1.Name = "aeroListView1";
            this.aeroListView1.Size = new System.Drawing.Size(607, 382);
            this.aeroListView1.TabIndex = 0;
            this.aeroListView1.UseCompatibleStateImageBehavior = false;
            this.aeroListView1.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Browser";
            this.columnHeader1.Width = 590;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.aeroListView2);
            this.tabPage2.Location = new System.Drawing.Point(140, 4);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(613, 388);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Communication";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // aeroListView2
            // 
            this.aeroListView2.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader2});
            this.aeroListView2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.aeroListView2.FullRowSelect = true;
            this.aeroListView2.HideSelection = false;
            this.aeroListView2.Location = new System.Drawing.Point(3, 3);
            listViewColumnSorter2.NeedNumberCompare = false;
            listViewColumnSorter2.Order = System.Windows.Forms.SortOrder.None;
            listViewColumnSorter2.SortColumn = 0;
            this.aeroListView2.LvwColumnSorter = listViewColumnSorter2;
            this.aeroListView2.Name = "aeroListView2";
            this.aeroListView2.Size = new System.Drawing.Size(607, 382);
            this.aeroListView2.TabIndex = 0;
            this.aeroListView2.UseCompatibleStateImageBehavior = false;
            this.aeroListView2.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Communicator";
            this.columnHeader2.Width = 603;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.aeroListView3);
            this.tabPage3.Location = new System.Drawing.Point(140, 4);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(613, 388);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Games";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // aeroListView3
            // 
            this.aeroListView3.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader3});
            this.aeroListView3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.aeroListView3.FullRowSelect = true;
            this.aeroListView3.HideSelection = false;
            this.aeroListView3.Location = new System.Drawing.Point(3, 3);
            listViewColumnSorter3.NeedNumberCompare = false;
            listViewColumnSorter3.Order = System.Windows.Forms.SortOrder.None;
            listViewColumnSorter3.SortColumn = 0;
            this.aeroListView3.LvwColumnSorter = listViewColumnSorter3;
            this.aeroListView3.Name = "aeroListView3";
            this.aeroListView3.Size = new System.Drawing.Size(607, 382);
            this.aeroListView3.TabIndex = 0;
            this.aeroListView3.UseCompatibleStateImageBehavior = false;
            this.aeroListView3.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Games";
            this.columnHeader3.Width = 603;
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.aeroListView4);
            this.tabPage4.Location = new System.Drawing.Point(140, 4);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(613, 388);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Other Applications";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // aeroListView4
            // 
            this.aeroListView4.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4});
            this.aeroListView4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.aeroListView4.FullRowSelect = true;
            this.aeroListView4.HideSelection = false;
            this.aeroListView4.Location = new System.Drawing.Point(3, 3);
            listViewColumnSorter4.NeedNumberCompare = false;
            listViewColumnSorter4.Order = System.Windows.Forms.SortOrder.None;
            listViewColumnSorter4.SortColumn = 0;
            this.aeroListView4.LvwColumnSorter = listViewColumnSorter4;
            this.aeroListView4.Name = "aeroListView4";
            this.aeroListView4.Size = new System.Drawing.Size(607, 382);
            this.aeroListView4.TabIndex = 0;
            this.aeroListView4.UseCompatibleStateImageBehavior = false;
            this.aeroListView4.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Application";
            this.columnHeader4.Width = 603;
            // 
            // FrmLoot
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(757, 396);
            this.Controls.Add(this.dotNetBarTabControl1);
            this.Name = "FrmLoot";
            this.Text = "FrmLoot";
            this.dotNetBarTabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tabPage4.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.DotNetBarTabControl dotNetBarTabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private Controls.AeroListView aeroListView1;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.TabPage tabPage2;
        private Controls.AeroListView aeroListView2;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.TabPage tabPage3;
        private Controls.AeroListView aeroListView3;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.TabPage tabPage4;
        private Controls.AeroListView aeroListView4;
        private System.Windows.Forms.ColumnHeader columnHeader4;
    }
}