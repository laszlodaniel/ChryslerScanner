using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChryslerCCDSCIScanner
{
    public partial class AboutForm : Form
    {
        MainForm originalForm;

        public AboutForm(MainForm incomingForm)
        {
            originalForm = incomingForm;
            InitializeComponent();
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
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
