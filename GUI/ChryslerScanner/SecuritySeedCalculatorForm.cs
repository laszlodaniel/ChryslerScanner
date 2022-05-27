using System;
using System.Drawing;
using System.Windows.Forms;

namespace ChryslerScanner
{
    public partial class SecuritySeedCalculatorForm : Form
    {
        public MainForm OriginalForm;

        private const ushort PCMUnlockKeyNormal = 0x9018;
        private const ushort PCMUnlockKeyBootstrap = 0x247C;

        public enum SCI_ID
        {
            GetSecuritySeedNormal = 0x2B,
            SendSecuritySeedNormal = 0x2C,
            SecuritySeedProgrammerBootstrap = 0x24,
            SecuritySeedSBECBootstrap = 0x26
        }

        public SecuritySeedCalculatorForm(MainForm IncomingForm)
        {
            OriginalForm = IncomingForm;
            InitializeComponent();
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            NormalModeSeedMessageTextBox_TextChanged(this, EventArgs.Empty);
            BootstrapModeSeedMessageTextBox_TextChanged(this, EventArgs.Empty);
            ActiveControl = NormalModeSeedSolveButton;
        }

        private void NormalModeSeedMessageTextBox_TextChanged(object sender, EventArgs e)
        {
            if (NormalModeSeedMessageTextBox.Text != string.Empty)
            {
                byte[] bytes = Util.HexStringToByte(NormalModeSeedMessageTextBox.Text);

                if ((bytes != null) && (bytes.Length == 4) && (bytes[0] == (byte)SCI_ID.GetSecuritySeedNormal))
                {
                    NormalModeSeedSolveButton.Enabled = true;
                }
                else
                {
                    NormalModeSeedSolveButton.Enabled = false;
                }
            }
            else
            {
                NormalModeSeedSolveButton.Enabled = false;
            }
        }

        private void NormalModeSeedSolveButton_Click(object sender, EventArgs e)
        {
            byte[] bytes = Util.HexStringToByte(NormalModeSeedMessageTextBox.Text);
            byte checksum = (byte)(bytes[0] + bytes[1] + bytes[2]);

            if (checksum == bytes[3]) // checksum ok
            {

                ushort seed = (ushort)((bytes[1] << 8) | bytes[2]);
                ushort solution = (ushort)((seed << 2) + PCMUnlockKeyNormal);
                byte solutionHB = (byte)(solution >> 8);
                byte solutionLB = (byte)(solution);
                byte solutionChecksum = (byte)((byte)SCI_ID.SendSecuritySeedNormal + solutionHB + solutionLB);
                byte[] solutionArray = { (byte)SCI_ID.SendSecuritySeedNormal, solutionHB, solutionLB, solutionChecksum };

                NormalModeSeedSolutionTextBox.Text = Util.ByteToHexString(solutionArray, 0, solutionArray.Length);
            }
            else // checksum error
            {
                NormalModeSeedSolutionTextBox.Text = "checksum er";
            }
        }

        private void NormalModeSeedMessageTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                e.Handled = true;

                if (NormalModeSeedSolveButton.Enabled) NormalModeSeedSolveButton_Click(this, EventArgs.Empty);
            }
        }

        private void NormalModeSeedSolutionTextBox_TextChanged(object sender, EventArgs e)
        {
            if (NormalModeSeedSolutionTextBox.Text != string.Empty)
            {
                byte[] bytes = Util.HexStringToByte(NormalModeSeedSolutionTextBox.Text);

                if ((bytes != null) && (bytes.Length == 4) && (bytes[0] == (byte)SCI_ID.SendSecuritySeedNormal))
                {
                    NormalModeSeedSolutionCopyButton.Enabled = true;
                }
                else
                {
                    NormalModeSeedSolutionCopyButton.Enabled = false;
                }
            }
            else
            {
                NormalModeSeedSolutionCopyButton.Enabled = false;
            }
        }

        private void NormalModeSeedSolutionCopyButton_Click(object sender, EventArgs e)
        {
            Clipboard.Clear();
            Clipboard.SetText(NormalModeSeedSolutionTextBox.Text);
        }

        private void BootstrapModeSeedMessageTextBox_TextChanged(object sender, EventArgs e)
        {
            if (BootstrapModeSeedMessageTextBox.Text != string.Empty)
            {
                byte[] bytes = Util.HexStringToByte(BootstrapModeSeedMessageTextBox.Text);

                if ((bytes != null) && (bytes.Length == 7) && (bytes[0] == (byte)SCI_ID.SecuritySeedSBECBootstrap))
                {
                    BootstrapModeSeedSolveButton.Enabled = true;
                }
                else
                {
                    BootstrapModeSeedSolveButton.Enabled = false;
                }
            }
            else
            {
                BootstrapModeSeedSolveButton.Enabled = false;
            }
        }

        private void BootstrapModeSeedSolveButton_Click(object sender, EventArgs e)
        {
            byte[] bytes = Util.HexStringToByte(BootstrapModeSeedMessageTextBox.Text);
            byte checksum = (byte)(bytes[0] + bytes[1] + bytes[2] + bytes[3] + bytes[4] + bytes[5]);

            if (checksum == bytes[6]) // checksum ok
            {
                ushort seed = (ushort)((bytes[4] << 8) | bytes[5]);
                ushort buff = (ushort)((seed + PCMUnlockKeyBootstrap) | 5);
                byte rotatecount = (byte)(buff & 0x0F);
                buff = RotateRightBits(buff, rotatecount);
                ushort solution = (ushort)(buff | PCMUnlockKeyBootstrap);
                byte solutionHB = (byte)(solution >> 8);
                byte solutionLB = (byte)(solution);
                byte solutionChecksum = (byte)((byte)SCI_ID.SecuritySeedProgrammerBootstrap + 0xD0 + 0x27 + 0xC2 + solutionHB + solutionLB);
                byte[] solutionArray = { (byte)SCI_ID.SecuritySeedProgrammerBootstrap, 0xD0, 0x27, 0xC2, solutionHB, solutionLB, solutionChecksum };

                BootstrapModeSeedSolutionTextBox.Text = Util.ByteToHexString(solutionArray, 0, solutionArray.Length);
            }
            else // checksum error
            {
                BootstrapModeSeedSolutionTextBox.Text = "checksum error";
            }
        }

        private void BootstrapModeSeedMessageTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                e.Handled = true;

                if (BootstrapModeSeedSolveButton.Enabled) BootstrapModeSeedSolveButton_Click(this, EventArgs.Empty);
            }
        }

        private void BootstrapModeSeedSolutionTextBox_TextChanged(object sender, EventArgs e)
        {
            if (BootstrapModeSeedSolutionTextBox.Text != string.Empty)
            {
                byte[] bytes = Util.HexStringToByte(BootstrapModeSeedSolutionTextBox.Text);

                if ((bytes != null) && (bytes.Length == 7) && (bytes[0] == (byte)SCI_ID.SecuritySeedProgrammerBootstrap))
                {
                    BootstrapModeSeedSolutionCopyButton.Enabled = true;
                }
                else
                {
                    BootstrapModeSeedSolutionCopyButton.Enabled = false;
                }
            }
            else
            {
                BootstrapModeSeedSolutionCopyButton.Enabled = false;
            }
        }

        private void BootstrapModeSeedSolutionCopyButton_Click(object sender, EventArgs e)
        {
            Clipboard.Clear();
            Clipboard.SetText(BootstrapModeSeedSolutionTextBox.Text);
        }

        private ushort RotateLeftBits(ushort input, ushort n)
        {
            return (ushort)((input << n) | (input >> ((sizeof(ushort) * 8) - n)));
        }

        private ushort RotateRightBits(ushort input, ushort n)
        {
            return (ushort)((input >> n) | (input << ((sizeof(ushort) * 8) - n)));
        }
    }
}
