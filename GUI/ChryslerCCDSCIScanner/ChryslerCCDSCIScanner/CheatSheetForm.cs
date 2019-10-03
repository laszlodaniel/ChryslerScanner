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
    public partial class CheatSheetForm : Form
    {
        MainForm originalForm;

        public CheatSheetForm(MainForm incomingForm)
        {
            this.CenterToParent();
            originalForm = incomingForm;

            InitializeComponent();

            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void CheatSheetForm_Load(object sender, EventArgs e)
        {
            //CheatSheetRichTextBox.Font = new Font("Corier New", 9f);
            CheatSheetRichTextBox.AppendText("Hello world!" + Environment.NewLine);
            CheatSheetRichTextBox.AppendText("This is another line.");
        }
    }
}
