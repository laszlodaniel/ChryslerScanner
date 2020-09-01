using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace ChryslerCCDSCIScanner
{
    public partial class AboutForm : Form
    {
        public MainForm originalForm;

        public AboutForm(MainForm incomingForm)
        {
            originalForm = incomingForm;
            InitializeComponent();
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            GUIFWVersionLabel.Text = "GUI v" + Assembly.GetExecutingAssembly().GetName().Version.ToString(3) + "   |   FW ";

            if (originalForm.FWVersion != String.Empty) GUIFWVersionLabel.Text += originalForm.FWVersion;
            else GUIFWVersionLabel.Text += "N/A";
        }

        private void BlogLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://chryslerccdsci.wordpress.com/");
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            GC.Collect();
            this.Close();
        }

        private void AboutForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            GC.Collect();
        }
    }
}
