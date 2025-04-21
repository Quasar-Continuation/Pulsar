using Pulsar.Common.Models;
using Pulsar.Common.Utilities;
using Pulsar.Server.Forms.DarkMode;
using System;
using System.Windows.Forms;

namespace Pulsar.Server.Forms
{
    public partial class FrmRegValueEditMultiString : Form
    {
        private readonly RegValueData _value;

        public FrmRegValueEditMultiString(RegValueData value)
        {
            _value = value;

            InitializeComponent();

            DarkModeManager.ApplyDarkMode(this);
			ScreenCaptureHider.ScreenCaptureHider.Apply(this.Handle);

            this.valueNameTxtBox.Text = value.Name;
            this.valueDataTxtBox.Text = string.Join("\r\n", ByteConverter.ToStringArray(value.Data));
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            _value.Data = ByteConverter.GetBytes(valueDataTxtBox.Text.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries));
            this.Tag = _value;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
