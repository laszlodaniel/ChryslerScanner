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
            GUIFWHWVersionLabel.Text = "GUI v" + Assembly.GetExecutingAssembly().GetName().Version.ToString(3) + "   |   FW ";

            if (originalForm.FWVersion != String.Empty) GUIFWHWVersionLabel.Text += originalForm.FWVersion;
            else GUIFWHWVersionLabel.Text += "N/A";

            GUIFWHWVersionLabel.Text += "   |   HW ";

            if (originalForm.HWVersion != String.Empty) GUIFWHWVersionLabel.Text += originalForm.HWVersion;
            else GUIFWHWVersionLabel.Text += "N/A";
        }

        private void BlogLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://chryslerccdsci.wordpress.com/");
        }
    }
}
