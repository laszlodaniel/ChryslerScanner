using System;
using System.Drawing;
using System.Windows.Forms;

namespace ChryslerCCDSCIScanner
{
    public partial class CheatSheetForm : Form
    {
        MainForm originalForm;

        public CheatSheetForm(MainForm incomingForm)
        {
            originalForm = incomingForm;
            InitializeComponent();
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            this.CenterToParent();
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void CheatSheetForm_Load(object sender, EventArgs e)
        {
            CheatSheetRichTextBox.AppendText("DRBDBReader usage:" + Environment.NewLine + Environment.NewLine);
            CheatSheetRichTextBox.AppendText("readdb: load \"database.mem\" file into RAM" + Environment.NewLine);
            CheatSheetRichTextBox.AppendText("modlist: list all supported modules" + Environment.NewLine);
            CheatSheetRichTextBox.AppendText("modsearch <name>: search module by <name>" + Environment.NewLine);
            CheatSheetRichTextBox.AppendText("modtxlist <id>: list all commands of a module <id>" + Environment.NewLine);
            CheatSheetRichTextBox.AppendText("dumpconverter <id>: list properties of a command <id>" + Environment.NewLine);
            CheatSheetRichTextBox.AppendText("txrunconverter <id> <value>: imperial conversion of <value> using <id> properties" + Environment.NewLine);
            CheatSheetRichTextBox.AppendText("txrunconvertermetric <id> <value>: metric conversion of <value> using <id> properties" + Environment.NewLine);
            CheatSheetRichTextBox.AppendText("txsearch <str1> && <str2> && ... && <strn>: search commands among all modules" + Environment.NewLine + Environment.NewLine);
            CheatSheetRichTextBox.AppendText("Example:" + Environment.NewLine + Environment.NewLine);
            CheatSheetRichTextBox.AppendText("> readdb" + Environment.NewLine);
            CheatSheetRichTextBox.AppendText("Loading database, please wait... done!" + Environment.NewLine + Environment.NewLine);
            CheatSheetRichTextBox.AppendText("> modlist" + Environment.NewLine);
            CheatSheetRichTextBox.AppendText("[...]" + Environment.NewLine);
            CheatSheetRichTextBox.AppendText("BCM JA BASE; sc: Body; 0x1010" + Environment.NewLine);
            CheatSheetRichTextBox.AppendText("[...]" + Environment.NewLine + Environment.NewLine);
            CheatSheetRichTextBox.AppendText("> modtxlist 0x1010" + Environment.NewLine);
            CheatSheetRichTextBox.AppendText("[...]" + Environment.NewLine);
            CheatSheetRichTextBox.AppendText("ENGINE RPM: CCD; xmit: E4-00-FF; sc: Body; 0x80002bd9" + Environment.NewLine);
            CheatSheetRichTextBox.AppendText("[...]" + Environment.NewLine + Environment.NewLine);
            CheatSheetRichTextBox.AppendText("> dumpconverter 0x80002bd9" + Environment.NewLine);
            CheatSheetRichTextBox.AppendText("TYPE:  NUMERIC" + Environment.NewLine);
            CheatSheetRichTextBox.AppendText("REC:   11-11-4E-54-02-08" + Environment.NewLine);
            CheatSheetRichTextBox.AppendText("DSREC: 4E-54-00-00-3F-80-00-00-3E-29-00-00-00-00-00-05-3E-29-20-44" + Environment.NewLine + Environment.NewLine);
            CheatSheetRichTextBox.AppendText("UNIT: RPM" + Environment.NewLine);
            CheatSheetRichTextBox.AppendText("SLOPE:  32" + Environment.NewLine);
            CheatSheetRichTextBox.AppendText("OFFSET: 0" + Environment.NewLine);
            CheatSheetRichTextBox.AppendText("SLCONV: 1" + Environment.NewLine);
            CheatSheetRichTextBox.AppendText("OFCONV: 0" + Environment.NewLine + Environment.NewLine);
            CheatSheetRichTextBox.AppendText("Manual calculation: multiply the raw hexadeximal number by the SLOPE value then add the OFFSET value. When metric conversion is available then this imperial result has to be multiplied by SLCONV again then OFCONV added." + Environment.NewLine);
            CheatSheetRichTextBox.AppendText("Pay attention to what number formatting you use. The software makes a difference between hexadecimal (0x16) and decimal (22) number formats." + Environment.NewLine + Environment.NewLine);
            CheatSheetRichTextBox.AppendText("Example CCD-bus message: E4 4A 00 2E (hexadecimal format implied)" + Environment.NewLine);
            CheatSheetRichTextBox.AppendText("Here 0x4A is the raw engine speed." + Environment.NewLine + Environment.NewLine);
            CheatSheetRichTextBox.AppendText("> txrunconverter 0x80002bd9 0x4A" + Environment.NewLine);
            CheatSheetRichTextBox.AppendText("2368 RPM" + Environment.NewLine + Environment.NewLine);
            CheatSheetRichTextBox.AppendText("Manual calculation: 0x4A = 74 -> (74 × SLOPE) + OFFSET = (74 × 32) + 0 = 2368 RPM" + Environment.NewLine + Environment.NewLine);
            CheatSheetRichTextBox.AppendText("To search a specific ID-byte on the CCD-bus you can do the following:" + Environment.NewLine + Environment.NewLine);
            CheatSheetRichTextBox.AppendText("> txsearch ccd && xmit: 24" + Environment.NewLine);
            CheatSheetRichTextBox.AppendText("VEHICLE SPEED: CCD; xmit: 24-00-FF; sc: Air Temp Control; 0x80003883" + Environment.NewLine);
            CheatSheetRichTextBox.AppendText("VEHICLE SPEED: CCD; xmit: 24-00-FF; sc: Vehicle Theft Security; 0x80003b61" + Environment.NewLine);
            CheatSheetRichTextBox.AppendText("VEHICLE SPEED: CCD; xmit: 24-00-FF; sc: Transmission; 0x80004c19" + Environment.NewLine);
            CheatSheetRichTextBox.AppendText("VEHICLE SPEED: CCD; xmit: 24-00-FF; sc: Transmission; 0x80004c1a" + Environment.NewLine);
            CheatSheetRichTextBox.AppendText("VEHICLE SPEED SENSOR: CCD; xmit: 24-00-FF; sc: Body; 0x80004ca1" + Environment.NewLine);
            CheatSheetRichTextBox.AppendText("VEHICLE SPEED: CCD; xmit: 24-00-FF; sc: MIC; 0x80004d64" + Environment.NewLine);
            CheatSheetRichTextBox.AppendText("VEHICLE SPEED SENSOR: CCD; xmit: 24 - 00 - FF; sc: Compass Mini-Trip; 0x80004e81" + Environment.NewLine + Environment.NewLine);
            CheatSheetRichTextBox.AppendText("Now it's a trial and error to find out which record to use but usually they contain the same scaling rules among different modules." + Environment.NewLine + Environment.NewLine);
            CheatSheetRichTextBox.AppendText("> dumpconverter 0x80004ca1" + Environment.NewLine);
            CheatSheetRichTextBox.AppendText("TYPE:  NUMERIC" + Environment.NewLine);
            CheatSheetRichTextBox.AppendText("REC:   11-11-0C-22-01-03" + Environment.NewLine);
            CheatSheetRichTextBox.AppendText("DSREC: 0C-22-00-00-3F-CD-FE-FC-3E-30-00-00-00-00-00-03-3E-31-20-44" + Environment.NewLine + Environment.NewLine);
            CheatSheetRichTextBox.AppendText("UNIT (DFLT/MTRC): MPH/KPH" + Environment.NewLine);
            CheatSheetRichTextBox.AppendText("SLOPE:  1" + Environment.NewLine);
            CheatSheetRichTextBox.AppendText("OFFSET: 0" + Environment.NewLine);
            CheatSheetRichTextBox.AppendText("SLCONV: 1.609344" + Environment.NewLine);
            CheatSheetRichTextBox.AppendText("OFCONV: 0" + Environment.NewLine + Environment.NewLine);
            CheatSheetRichTextBox.AppendText("Notice that the SLOPE value is 1 and OFFSET is 0. This means that the raw byte value is meant to be used as is and you only need to append the appropriate UNIT." + Environment.NewLine);
            CheatSheetRichTextBox.AppendText("Example CCD-bus message: 24 16 24 5E (again hexadecimal format implied)." + Environment.NewLine);
            CheatSheetRichTextBox.AppendText("This format is interesting because it makes use of the two payload bytes to avoid conversion between imperial and metric values." + Environment.NewLine);
            CheatSheetRichTextBox.AppendText("See the first payload byte is 0x16 = 22 MPH and the second byte 0x24 = 36 KM/H happens to be equal to 22 × 1.609344 (= 35.4 ~ 36) as per the metric conversion rule." + Environment.NewLine);
        }
    }
}
