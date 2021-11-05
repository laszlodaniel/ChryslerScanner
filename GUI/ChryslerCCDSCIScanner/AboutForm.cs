using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace ChryslerCCDSCIScanner
{
    public partial class AboutForm : Form
    {
        public MainForm OriginalForm;

        public AboutForm(MainForm IncomingForm)
        {
            OriginalForm = IncomingForm;
            InitializeComponent();
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            GUIFWHWVersionLabel.Text = "GUI v" + Assembly.GetExecutingAssembly().GetName().Version.ToString(3) + "   |   FW ";

            if (OriginalForm.FWVersion != string.Empty) GUIFWHWVersionLabel.Text += OriginalForm.FWVersion;
            else GUIFWHWVersionLabel.Text += "N/A";

            GUIFWHWVersionLabel.Text += "   |   HW ";

            if (OriginalForm.HWVersion != string.Empty) GUIFWHWVersionLabel.Text += OriginalForm.HWVersion;
            else GUIFWHWVersionLabel.Text += "N/A";
        }

        private void BlogLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://chryslerccdsci.wordpress.com/");
        }
    }
}
