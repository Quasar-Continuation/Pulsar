using System;
using System.Windows.Forms;
using DarkModeForms;
using Quasar.Common.Models;
using Quasar.Common.Utilities;
using Quasar.Server.Registry;

namespace Quasar.Server.Forms
{
    public partial class FrmRegValueEditString : Form
    {
        private readonly DarkModeCS dm = null;

        private readonly RegValueData _value;

        public FrmRegValueEditString(RegValueData value)
        {
            _value = value;

            InitializeComponent();

            dm = new DarkModeCS(this)
            {
                //[Optional] Choose your preferred color mode here:
                ColorMode = DarkModeCS.DisplayMode.SystemDefault,
                ColorizeIcons = false
            };

            this.valueNameTxtBox.Text = RegValueHelper.GetName(value.Name);
            this.valueDataTxtBox.Text = ByteConverter.ToString(value.Data);
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            _value.Data = ByteConverter.GetBytes(valueDataTxtBox.Text);
            this.Tag = _value;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
