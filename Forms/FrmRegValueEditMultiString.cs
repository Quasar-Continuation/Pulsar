using System;
using System.Windows.Forms;
using DarkModeForms;
using Quasar.Common.Models;
using Quasar.Common.Utilities;

namespace Quasar.Server.Forms
{
    public partial class FrmRegValueEditMultiString : Form
    {
        private readonly RegValueData _value;

        private readonly DarkModeCS dm = null;

        public FrmRegValueEditMultiString(RegValueData value)
        {
            _value = value;

            InitializeComponent();

            dm = new DarkModeCS(this)
            {
                //[Optional] Choose your preferred color mode here:
                ColorMode = DarkModeCS.DisplayMode.SystemDefault,
                ColorizeIcons = false
            };

            this.valueNameTxtBox.Text = value.Name;
            this.valueDataTxtBox.Text = string.Join("\r\n", ByteConverter.ToStringArray(value.Data));
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            _value.Data = ByteConverter.GetBytes(valueDataTxtBox.Text.Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries));
            this.Tag = _value;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
