using Pulsar.Common.Enums;
using Pulsar.Common.Helpers;
using Pulsar.Common.Models;
using Pulsar.Server.Forms.DarkMode;
using System;
using System.IO;
using System.Windows.Forms;

namespace Pulsar.Server.Forms
{
    public partial class FrmStartupAdd : Form
    {
        public StartupItem StartupItem { get; set; }

        public FrmStartupAdd()
        {
            InitializeComponent();

            DarkModeManager.ApplyDarkMode(this);
			ScreenCaptureHider.ScreenCaptureHider.Apply(this);

            AddTypes();
        }

        public FrmStartupAdd(string startupPath)
        {
            InitializeComponent();
            AddTypes();

            txtName.Text = Path.GetFileNameWithoutExtension(startupPath);
            txtPath.Text = startupPath;
        }

        /// <summary>
        /// Adds all supported startup types to ComboBox groups.
        /// </summary>
        /// <remarks>
        /// Must be in same order as <see cref="StartupType"/>.
        /// </remarks>
        private void AddTypes()
        {
            // must be in same order as StartupType
            cmbType.Items.Add("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run");
            cmbType.Items.Add("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\RunOnce");
            cmbType.Items.Add("HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run");
            cmbType.Items.Add("HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\RunOnce");
            cmbType.Items.Add("%APPDATA%\\Microsoft\\Windows\\Start Menu\\Programs\\Startup");
            cmbType.SelectedIndex = 0;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            StartupItem = new StartupItem
            { Name = txtName.Text, Path = txtPath.Text, Type = (StartupType)cmbType.SelectedIndex };

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void txtName_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = ((e.KeyChar == '\\' || FileHelper.HasIllegalCharacters(e.KeyChar.ToString())) &&
                         !char.IsControl(e.KeyChar));
        }

        private void txtPath_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = ((e.KeyChar == '\\' || FileHelper.HasIllegalCharacters(e.KeyChar.ToString())) &&
                         !char.IsControl(e.KeyChar));
        }
    }
}