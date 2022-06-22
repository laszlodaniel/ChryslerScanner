using System;
using System.Drawing;
using System.Windows.Forms;

namespace ChryslerScanner
{
    public partial class SecurityKeyCalculatorForm : Form
    {
        private MainForm OriginalForm;

        private const ushort PCMUnlockKeyNormal = 0x9018;
        private const ushort PCMUnlockKeyBootstrap = 0x247C;

        public enum SCI_ID
        {
            GetSecuritySeedNormalLegacy = 0x2B,
            GetSecuritySeedNormal = 0x35,
            SendSecurityKeyNormal = 0x2C,
            SecuritySeedProgrammerBootstrap = 0x24,
            SecurityKeySBECBootstrap = 0x26
        }

        public SecurityKeyCalculatorForm(MainForm IncomingForm)
        {
            OriginalForm = IncomingForm;
            InitializeComponent();
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            NormalModeSeedTextBox_TextChanged(this, EventArgs.Empty);
            BootstrapModeSeedTextBox_TextChanged(this, EventArgs.Empty);
            ActiveControl = NormalModeGetKeyButton;
        }

        private void NormalModeSeedTextBox_TextChanged(object sender, EventArgs e)
        {
            if (NormalModeSeedTextBox.Text != string.Empty)
            {
                byte[] bytes = Util.HexStringToByte(NormalModeSeedTextBox.Text);

                if ((bytes != null) && ((bytes.Length == 4) && (bytes[0] == (byte)SCI_ID.GetSecuritySeedNormalLegacy)) || ((bytes.Length == 5) && (bytes[0] == (byte)SCI_ID.GetSecuritySeedNormal) && ((bytes[1] == 0x01 ) || (bytes[1] == 0x02))))
                {
                    NormalModeGetKeyButton.Enabled = true;
                }
                else
                {
                    NormalModeGetKeyButton.Enabled = false;
                }
            }
            else
            {
                NormalModeGetKeyButton.Enabled = false;
            }
        }

        private void NormalModeGetKeyButton_Click(object sender, EventArgs e)
        {
            byte[] bytes = Util.HexStringToByte(NormalModeSeedTextBox.Text);

            if (bytes[0] == (byte)SCI_ID.GetSecuritySeedNormalLegacy)
            {
                byte checksum = (byte)(bytes[0] + bytes[1] + bytes[2]);

                if (checksum == bytes[3]) // checksum ok
                {
                    ushort seed = (ushort)((bytes[1] << 8) | bytes[2]);

                    if (seed != 0)
                    {
                        ushort key = (ushort)((seed << 2) + PCMUnlockKeyNormal);
                        byte keyHB = (byte)(key >> 8);
                        byte keyLB = (byte)(key);
                        byte keyChecksum = (byte)((byte)SCI_ID.SendSecurityKeyNormal + keyHB + keyLB);
                        byte[] keyArray = { (byte)SCI_ID.SendSecurityKeyNormal, keyHB, keyLB, keyChecksum };

                        NormalModeKeyTextBox.Text = Util.ByteToHexString(keyArray, 0, keyArray.Length);
                    }
                    else
                    {
                        NormalModeKeyTextBox.Text = "already unlocked";
                    }
                }
                else // checksum error
                {
                    NormalModeKeyTextBox.Text = "checksum error";
                }
            }
            else if (bytes[0] == (byte)SCI_ID.GetSecuritySeedNormal)
            {
                byte checksum = (byte)(bytes[0] + bytes[1] + bytes[2] + bytes[3]);

                if (checksum == bytes[4]) // checksum ok)
                {
                    ushort seed = (ushort)((bytes[2] << 8) | bytes[3]);

                    if (seed != 0)
                    {
                        if (bytes[1] == 0x01) // Level 1 (same as legacy key)
                        {
                            ushort key = (ushort)((seed << 2) + PCMUnlockKeyNormal);
                            byte keyHB = (byte)(key >> 8);
                            byte keyLB = (byte)(key);
                            byte keyChecksum = (byte)((byte)SCI_ID.SendSecurityKeyNormal + keyHB + keyLB);
                            byte[] keyArray = { (byte)SCI_ID.SendSecurityKeyNormal, keyHB, keyLB, keyChecksum };

                            NormalModeKeyTextBox.Text = Util.ByteToHexString(keyArray, 0, keyArray.Length);
                        }
                        else if (bytes[1] == 0x02) // Level 2
                        {
                            ushort key = (ushort)(seed & 0xFF00);
                            key |= (ushort)(key >> 8);

                            ushort mask = (ushort)(seed & 0xFF);
                            mask |= (ushort)(mask << 8);
                            key ^= 0x9340; // polinom
                            key += 0x1010;
                            key ^= mask;
                            key += 0x1911;
                            uint tmp = (uint)((key << 16) | key);
                            key += (ushort)(tmp >> 3);
                            byte keyHB = (byte)(key >> 8);
                            byte keyLB = (byte)(key);
                            byte keyChecksum = (byte)(0x2C + keyHB + keyLB);
                            byte[] keyArray = { 0x2C, keyHB, keyLB, keyChecksum };

                            NormalModeKeyTextBox.Text = Util.ByteToHexString(keyArray, 0, keyArray.Length);
                        }
                    }
                    else
                    {
                        NormalModeKeyTextBox.Text = "already unlocked";
                    }
                }
                else // checksum error
                {
                    NormalModeKeyTextBox.Text = "checksum error";
                }
            }
        }

        private void NormalModeSeedTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                e.Handled = true;

                if (NormalModeGetKeyButton.Enabled) NormalModeGetKeyButton_Click(this, EventArgs.Empty);
            }
        }

        private void NormalModeKeyTextBox_TextChanged(object sender, EventArgs e)
        {
            if (NormalModeKeyTextBox.Text != string.Empty)
            {
                byte[] bytes = Util.HexStringToByte(NormalModeKeyTextBox.Text);

                if ((bytes != null) && (bytes.Length == 4) && (bytes[0] == (byte)SCI_ID.SendSecurityKeyNormal))
                {
                    NormalModeKeyCopyButton.Enabled = true;
                }
                else
                {
                    NormalModeKeyCopyButton.Enabled = false;
                }
            }
            else
            {
                NormalModeKeyCopyButton.Enabled = false;
            }
        }

        private void NormalModeKeyCopyButton_Click(object sender, EventArgs e)
        {
            Clipboard.Clear();
            Clipboard.SetText(NormalModeKeyTextBox.Text);
        }

        private void BootstrapModeSeedTextBox_TextChanged(object sender, EventArgs e)
        {
            if (BootstrapModeSeedTextBox.Text != string.Empty)
            {
                byte[] bytes = Util.HexStringToByte(BootstrapModeSeedTextBox.Text);

                if ((bytes != null) && (bytes.Length == 7) && (bytes[0] == (byte)SCI_ID.SecurityKeySBECBootstrap))
                {
                    BootstrapModeGetKeyButton.Enabled = true;
                }
                else
                {
                    BootstrapModeGetKeyButton.Enabled = false;
                }
            }
            else
            {
                BootstrapModeGetKeyButton.Enabled = false;
            }
        }

        private void BootstrapModeGetKeyButton_Click(object sender, EventArgs e)
        {
            byte[] bytes = Util.HexStringToByte(BootstrapModeSeedTextBox.Text);
            byte checksum = (byte)(bytes[0] + bytes[1] + bytes[2] + bytes[3] + bytes[4] + bytes[5]);

            if (checksum == bytes[6]) // checksum ok
            {
                ushort seed = (ushort)((bytes[4] << 8) | bytes[5]);
                ushort buff = (ushort)((seed + PCMUnlockKeyBootstrap) | 5);
                byte rotatecount = (byte)(buff & 0x0F);
                buff = RotateRightBits(buff, rotatecount);
                ushort key = (ushort)(buff | PCMUnlockKeyBootstrap);
                byte keyHB = (byte)(key >> 8);
                byte keyLB = (byte)(key);
                byte keyChecksum = (byte)((byte)SCI_ID.SecuritySeedProgrammerBootstrap + 0xD0 + 0x27 + 0xC2 + keyHB + keyLB);
                byte[] keyArray = { (byte)SCI_ID.SecuritySeedProgrammerBootstrap, 0xD0, 0x27, 0xC2, keyHB, keyLB, keyChecksum };

                BootstrapModeKeyTextBox.Text = Util.ByteToHexString(keyArray, 0, keyArray.Length);
            }
            else // checksum error
            {
                BootstrapModeKeyTextBox.Text = "checksum error";
            }
        }

        private void BootstrapModeSeedTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                e.Handled = true;

                if (BootstrapModeGetKeyButton.Enabled) BootstrapModeGetKeyButton_Click(this, EventArgs.Empty);
            }
        }

        private void BootstrapModeKeyTextBox_TextChanged(object sender, EventArgs e)
        {
            if (BootstrapModeKeyTextBox.Text != string.Empty)
            {
                byte[] bytes = Util.HexStringToByte(BootstrapModeKeyTextBox.Text);

                if ((bytes != null) && (bytes.Length == 7) && (bytes[0] == (byte)SCI_ID.SecuritySeedProgrammerBootstrap))
                {
                    BootstrapModeKeyCopyButton.Enabled = true;
                }
                else
                {
                    BootstrapModeKeyCopyButton.Enabled = false;
                }
            }
            else
            {
                BootstrapModeKeyCopyButton.Enabled = false;
            }
        }

        private void BootstrapModeKeyCopyButton_Click(object sender, EventArgs e)
        {
            Clipboard.Clear();
            Clipboard.SetText(BootstrapModeKeyTextBox.Text);
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
