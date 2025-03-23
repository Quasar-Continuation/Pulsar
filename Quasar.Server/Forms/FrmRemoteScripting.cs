using Quasar.Server.Forms.DarkMode;
using Quasar.Server.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Quasar.Server.Forms
{
    public partial class FrmRemoteScripting : Form
    { 
        private readonly int _selectedClients;

    public string Lang { get; set; }
    public string Script { get; set; }
        public bool Hidden { get; set; }
        public FrmRemoteScripting(int selected)
        {
            _selectedClients = selected;

            InitializeComponent();

            DarkModeManager.ApplyDarkMode(this);
        }

        private void ExecBtn_Click(object sender, EventArgs e)
        {
            if (dotNetBarTabControl1.SelectedTab == tabPage1)
            {
                Lang = "Powershell";
                Script = PSEdit.Text;
                Hidden = HidCheckBox.Checked;
            }
            else if (dotNetBarTabControl1.SelectedTab == tabPage2)
            {
                Lang = "Batch";
                Script = BATEdit.Text;
                Hidden = HidCheckBox.Checked;
            }
            else if (dotNetBarTabControl1.SelectedTab == tabPage3)
            {
                Lang = "VBScript";
                Script = VBSEdit.Text;
                Hidden = HidCheckBox.Checked;
            }
            else if (dotNetBarTabControl1.SelectedTab == tabPage4)
            {
                Lang = "JavaScript";
                Script = JSEdit.Text;
                Hidden = HidCheckBox.Checked;
            }


            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void FrmRemoteScripting_Load(object sender, EventArgs e)
        {
            this.Text = WindowHelper.GetWindowTitle("Remote Scripting", _selectedClients);
        }
    }
}
