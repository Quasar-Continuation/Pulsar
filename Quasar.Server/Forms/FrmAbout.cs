using Quasar.Server.Forms.DarkMode;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace Quasar.Server.Forms
{
    public partial class FrmAbout : Form
    {
        private readonly string _repositoryUrl = @"https://github.com/Quasar-Continuation/Quasar-Modded";

        public FrmAbout()
        {
            InitializeComponent();

            DarkModeManager.ApplyDarkMode(this);

            lblVersion.Text = $"v{Application.ProductVersion}";
            rtxtContent.Text = Properties.Resources.License;
            LoadContributors();

            lnkGithubPage.Links.Add(new LinkLabel.Link { LinkData = _repositoryUrl });
            lnkCredits.Links.Add(new LinkLabel.Link { LinkData = "https://github.com/quasar/Quasar/tree/master/Licenses" });
        }

        private async void LoadContributors()
        {
            try
            {
                string contributors = await FetchContributors();
                DisplayContributors(contributors);
            }
            catch (Exception ex)
            {
                DisplayContributors("Error fetching contributors: " + ex.Message);
            }
        }

        private async Task<string> FetchContributors()
        {
            using (WebClient client = new WebClient())
            {
                client.Headers.Add("User-Agent", "Quasar-Modded");
                string apiUrl = "https://api.github.com/repos/Quasar-Continuation/Quasar-Modded/contributors";
                
                string response = await client.DownloadStringTaskAsync(new Uri(apiUrl));
                var contributorData = JsonConvert.DeserializeObject<List<Contributor>>(response);
                
                StringBuilder sb = new StringBuilder();
                foreach (var contributor in contributorData)
                {
                    sb.AppendLine($"- {contributor.Login}");
                }
                
                return sb.ToString();
            }
        }

        private void DisplayContributors(string contributors)
        {
            StringBuilder contributorsText = new StringBuilder();
            contributorsText.AppendLine("Thanks to the contributors below for making this project possible:");
            contributorsText.AppendLine();
            contributorsText.Append(contributors);
            
            cntTxtContent.Text = contributorsText.ToString();
        }

        private void lnkGithubPage_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            lnkGithubPage.LinkVisited = true;
            Process.Start(e.Link.LinkData.ToString());
        }

        private void lnkCredits_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            lnkCredits.LinkVisited = true;
            Process.Start(e.Link.LinkData.ToString());
        }

        private void btnOkay_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }

    public class Contributor
    {
        [JsonProperty("login")]
        public string Login { get; set; }
    }
}
