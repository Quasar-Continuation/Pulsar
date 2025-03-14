using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;
using Quasar.Server.Forms.DarkMode;
using Quasar.Server.Helper;
using Quasar.Server.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Quasar.Server.Forms
{
    public partial class FrmCertificate : Form
    {
        private Org.BouncyCastle.X509.X509Certificate _certificate;
        private AsymmetricKeyParameter _privateKey;


        public FrmCertificate()
        {
            InitializeComponent();
            DarkModeManager.ApplyDarkMode(this);
        }
        private void SetCertificate(CertificateWithPrivateKey certWithKey)
        {
            if (certWithKey == null || certWithKey.Certificate == null)
            {
                throw new ArgumentNullException(nameof(certWithKey), "Certificate cannot be null.");
            }

            _certificate = certWithKey.Certificate; // Store the certificate
            _privateKey = certWithKey.PrivateKey; // Store the private key
            txtDetails.Text = _certificate.ToString();
            btnSave.Enabled = true;
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            try
            {
                // Create the certificate authority and get the certificate with the private key
                var certWithKey = CertificateHelper.CreateCertificateAuthority("Mod Server", 4096);

                // Pass the entire CertificateWithPrivateKey object to the SetCertificate method
                SetCertificate(certWithKey);
            }
            catch (Exception ex)
            {
                // Handle exceptions and provide feedback to the user
                MessageBox.Show(this, $"Error creating certificate: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }




        private void btnImport_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.CheckFileExists = true;
                ofd.Filter = "PKCS#12 Files (*.p12)|*.p12";
                ofd.Multiselect = false;
                ofd.InitialDirectory = Application.StartupPath;

                if (ofd.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        // Load the PKCS#12 file
                        using (var stream = File.OpenRead(ofd.FileName))
                        {
                            var pkcs12Store = new Pkcs12Store();
                            pkcs12Store.Load(stream, null); // You can provide a password if needed

                            // Assuming you want to get the first certificate
                            foreach (string alias in pkcs12Store.Aliases)
                            {
                                if (pkcs12Store.IsKeyEntry(alias))
                                {
                                    var keyEntry = pkcs12Store.GetKey(alias);
                                    var certificateEntry = pkcs12Store.GetCertificate(alias);
                                    var certificate = certificateEntry.Certificate;

                                    // Check if the private key exists
                                    if (keyEntry.Key != null)
                                    {
                                        // Set the certificate
                                        SetCertificate((CertificateWithPrivateKey)certificate);
                                        MessageBox.Show("Certificate imported successfully and has a private key.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    }
                                    else
                                    {
                                        MessageBox.Show("Certificate imported successfully but does not have a private key.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    }
                                    break; // Exit after the first certificate
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, $"Error importing the certificate:\n{ex.Message}", "Import Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (_certificate == null)
                    throw new ArgumentNullException(nameof(_certificate), "Certificate cannot be null.");

                // Use the private key that was set in SetCertificate
                if (_privateKey == null)
                    throw new ArgumentException("The certificate does not have an associated private key.");

                // Create a new PKCS#12 store
                var pkcs12Store = new Pkcs12Store();
                string alias = "QuasarServerCert";

                // Add the private key and certificate to the store
                pkcs12Store.SetKeyEntry(alias, new AsymmetricKeyEntry(_privateKey), new[] { new X509CertificateEntry(_certificate) });

                // Save the PKCS#12 file
                using (var stream = File.Create(Settings.CertificatePath))
                {
                    // You can set a password for the PKCS#12 file
                    char[] password = null;
                    var secureRandom = new Org.BouncyCastle.Security.SecureRandom(); // Create a new SecureRandom instance

                    pkcs12Store.Save(stream, password, secureRandom);
                }

                MessageBox.Show(this,
                    "Please backup the certificate now. Loss of the certificate results in losing all clients!",
                    "Certificate backup", MessageBoxButtons.OK, MessageBoxIcon.Information);

                string argument = "/select, \"" + Settings.CertificatePath + "\"";
                Process.Start("explorer.exe", argument);

                this.DialogResult = DialogResult.OK;
            }
            catch (ArgumentNullException ex)
            {
                MessageBox.Show(this, ex.Message, "Save error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(this,
                    ex.Message,
                    "Save error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception)
            {
                MessageBox.Show(this,
                    "There was an error saving the certificate, please make sure you have write access to the Quasar directory.",
                    "Save error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
