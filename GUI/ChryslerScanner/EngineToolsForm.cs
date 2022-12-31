using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChryslerScanner
{
    public partial class EngineToolsForm : Form
    {
        private MainForm OriginalForm;

        private string CSVFilename = string.Empty;
        private string CSVHeader = string.Empty;
        private string CSVLine = string.Empty;
        private byte DiagnosticItemCount = 0;
        private List<byte[]> DiagnosticItems = new List<byte[]>();
        private byte FirstDiagnosticItemID = 0;
        private byte[] WordRequestFilter = new byte[8] { 0x0A, 0x0C, 0x27, 0x29, 0x35, 0x3C, 0x4B, 0x7A };
        private byte[] DWordRequestFilter = new byte[1] { 0x00 };
        private byte WordRequestCount = 0;

        private enum SCI_ID
        {
            SCIHiSpeed = 0x12,
            DiagnosticData = 0x14,
            ConfigData = 0x2A,
            GetSecuritySeedLegacy = 0x2B,
            GetSecuritySeed = 0x35,
            SendSecurityKey = 0x2C,
            SCILoSpeed = 0xFE
        }

        public EngineToolsForm(MainForm IncomingForm)
        {
            OriginalForm = IncomingForm;
            InitializeComponent();
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            MainForm.Packet.PacketReceived += PacketReceivedHandler; // subscribe to the OnPacketReceived event

            CHTComboBox.SelectedIndex = 0;
            RAMTableComboBox.SelectedIndex = 4;
            ActuatorTestComboBox.SelectedIndex = 1;
            ResetMemoryComboBox.SelectedIndex = 1;
            SecurityLevelComboBox.SelectedIndex = 0;
            ConfigurationComboBox.SelectedIndex = 1;

            if (OriginalForm.PCM.speed == "7812.5 baud")
            {
                LowSpeedLayout();
                ReadFaultCodeFreezeFrameButton.Enabled = false;
            }
            else if (OriginalForm.PCM.speed == "62500 baud")
            {
                HighSpeedLayout();
                ReadFaultCodeFreezeFrameButton.Enabled = true;
                AddHighSpeedDiagnosticData((byte)(0xF0 + RAMTableComboBox.SelectedIndex));
            }

            UpdateCHTGroup();
            UpdateStatusBar();

            ActiveControl = CHTDetectButton;
        }

        private void CHTDetectButton_Click(object sender, EventArgs e)
        {
            if (OriginalForm.PCM.speed != "7812.5 baud")
            {
                MessageBox.Show("Detector works in low-speed mode only!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ConfigurationGetAllButton_Click(this, EventArgs.Empty);
        }

        private void CHTComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CHTComboBox.SelectedIndex == 9) // CUMMINS
            {
                RAMTableComboBox.SelectedIndex = 11; // FB
            }
            else
            {
                RAMTableComboBox.SelectedIndex = 4; // F4
            }

            OriginalForm.PCM.ControllerHardwareType = (byte)CHTComboBox.SelectedIndex;
        }

        private void ReadFaultCodesButton_Click(object sender, EventArgs e)
        {
            if (OriginalForm.PCM.speed == "7812.5 baud")
            {
                MainForm.Packet.tx.bus = (byte)Packet.Bus.pcm;
                MainForm.Packet.tx.command = (byte)Packet.Command.msgTx;
                MainForm.Packet.tx.mode = (byte)Packet.MsgTxMode.list;

                if (CHTComboBox.SelectedIndex == 9) // CUMMINS
                {                                           // cnt   len   msg   len   msg
                    MainForm.Packet.tx.payload = new byte[5] { 0x02, 0x01, 0x32, 0x01, 0x33 }; // request stored and one-trip fault codes
                }
                else // SBEC
                {                                           // cnt   len   msg   len   msg   len   msg
                    MainForm.Packet.tx.payload = new byte[7] { 0x03, 0x01, 0x10, 0x01, 0x11, 0x01, 0x2E }; // request stored, pending and one-trip fault codes
                }
            }
            else if (OriginalForm.PCM.speed == "62500 baud")
            {
                bool success = int.TryParse(DiagnosticDataRepeatIntervalTextBox.Text, out int repeatInterval);

                if (!success || (repeatInterval == 0))
                {
                    repeatInterval = 50;
                    DiagnosticDataRepeatIntervalTextBox.Text = "50";
                }

                List<byte> payloadList = new List<byte>();
                byte[] repeatIntervalArray = new byte[2];

                repeatIntervalArray[0] = (byte)((repeatInterval >> 8) & 0xFF);
                repeatIntervalArray[1] = (byte)(repeatInterval & 0xFF);

                payloadList.Add((byte)Packet.Bus.pcm);
                payloadList.AddRange(repeatIntervalArray);

                MainForm.Packet.tx.bus = (byte)Packet.Bus.usb;
                MainForm.Packet.tx.command = (byte)Packet.Command.settings;
                MainForm.Packet.tx.mode = (byte)Packet.SettingsMode.setRepeatBehavior;
                MainForm.Packet.tx.payload = payloadList.ToArray();
                MainForm.Packet.GeneratePacket();
                OriginalForm.TransmitUSBPacket("[<-TX] Set SCI-bus (PCM) message repeat:");

                MainForm.Packet.tx.bus = (byte)Packet.Bus.pcm;
                MainForm.Packet.tx.command = (byte)Packet.Command.msgTx;
                MainForm.Packet.tx.mode = (byte)Packet.MsgTxMode.list;

                if (CHTComboBox.SelectedIndex == 9) // CUMMINS
                {
                    MainForm.Packet.tx.payload = new byte[25] { 0x08, 0x02, 0xFB, 0xBB, 0x02, 0xFB, 0xBC, 0x02, 0xFB, 0xBD, 0x02, 0xFB, 0xBE, 0x02, 0xFB, 0xBF, 0x02, 0xFB, 0xC0, 0x02, 0xFB, 0xC1, 0x02, 0xFB, 0xC2 };
                }
                else // SBEC
                {
                    MainForm.Packet.tx.payload = new byte[25] { 0x08, 0x02, 0xF4, 0x01, 0x02, 0xF4, 0x02, 0x02, 0xF4, 0x74, 0x02, 0xF4, 0x75, 0x02, 0xF4, 0x76, 0x02, 0xF4, 0x77, 0x02, 0xF4, 0x78, 0x02, 0xF4, 0x79 };
                }
            }
            else
            {
                MainForm.Packet.tx.bus = (byte)Packet.Bus.pcm;
                MainForm.Packet.tx.command = (byte)Packet.Command.msgTx;
                MainForm.Packet.tx.mode = (byte)Packet.MsgTxMode.single;
                MainForm.Packet.tx.payload = new byte[1] { 0x10 };
            }

            MainForm.Packet.GeneratePacket();
            OriginalForm.TransmitUSBPacket("[<-TX] PCM fault code list request:");
        }

        private void EraseFaultCodesButton_Click(object sender, EventArgs e)
        {
            if (OriginalForm.PCM.speed == "7812.5 baud")
            {
                MainForm.Packet.tx.bus = (byte)Packet.Bus.pcm;
                MainForm.Packet.tx.command = (byte)Packet.Command.msgTx;
                MainForm.Packet.tx.mode = (byte)Packet.MsgTxMode.single;

                if (CHTComboBox.SelectedIndex == 9) // CUMMINS
                {
                    MainForm.Packet.tx.payload = new byte[2] { 0x25, 0x01 };
                }
                else // SBEC
                {
                    MainForm.Packet.tx.payload = new byte[1] { 0x17 }; // 23 01 works as well
                }

                MainForm.Packet.GeneratePacket();
                OriginalForm.TransmitUSBPacket("[<-TX] Erase PCM fault code(s) request:");
            }
            else
            {
                MessageBox.Show("Fault codes can only be erased in low-speed mode.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ReadFaultCodeFreezeFrameButton_Click(object sender, EventArgs e)
        {
            if (CHTComboBox.SelectedIndex == 9) // CUMMINS
            {
                MessageBox.Show("This feature is not supported yet.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            bool success = int.TryParse(DiagnosticDataRepeatIntervalTextBox.Text, out int repeatInterval);

            if (!success || (repeatInterval == 0))
            {
                repeatInterval = 50;
                DiagnosticDataRepeatIntervalTextBox.Text = "50";
            }

            List<byte> payloadList = new List<byte>();
            byte[] repeatIntervalArray = new byte[2];

            repeatIntervalArray[0] = (byte)((repeatInterval >> 8) & 0xFF);
            repeatIntervalArray[1] = (byte)(repeatInterval & 0xFF);

            payloadList.Add((byte)Packet.Bus.pcm);
            payloadList.AddRange(repeatIntervalArray);

            MainForm.Packet.tx.bus = (byte)Packet.Bus.usb;
            MainForm.Packet.tx.command = (byte)Packet.Command.settings;
            MainForm.Packet.tx.mode = (byte)Packet.SettingsMode.setRepeatBehavior;
            MainForm.Packet.tx.payload = payloadList.ToArray();
            MainForm.Packet.GeneratePacket();
            OriginalForm.TransmitUSBPacket("[<-TX] Set SCI-bus (PCM) message repeat:");

            MainForm.Packet.tx.bus = (byte)Packet.Bus.pcm;
            MainForm.Packet.tx.command = (byte)Packet.Command.msgTx;
            MainForm.Packet.tx.mode = (byte)Packet.MsgTxMode.list;
            MainForm.Packet.tx.payload = new byte[47] { 0x0F, 0x03, 0xF5, 0xCB, 0xCC, 0x02, 0xF4, 0xA7, 0x02, 0xF4, 0xA8, 0x02, 0xF4, 0xA9, 0x02, 0xF4, 0xAA, 0x02, 0xF4, 0xAB, 0x02, 0xF4, 0xAC, 0x02, 0xF4, 0xAD, 0x02, 0xF4, 0xAE, 0x02, 0xF4, 0xAF, 0x02, 0xF4, 0xB0, 0x02, 0xF4, 0xB1, 0x02, 0xF4, 0xB2, 0x02, 0xF4, 0xB3, 0x02, 0xF4, 0xB4 };

            MainForm.Packet.GeneratePacket();
            OriginalForm.TransmitUSBPacket("[<-TX] PCM freeze frame request:");
        }

        private void Baud7812Button_Click(object sender, EventArgs e)
        {
            MainForm.Packet.tx.bus = (byte)Packet.Bus.pcm;
            MainForm.Packet.tx.command = (byte)Packet.Command.msgTx;
            MainForm.Packet.tx.mode = (byte)Packet.MsgTxMode.single;
            MainForm.Packet.tx.payload = new byte[1] { 0xFE };
            MainForm.Packet.GeneratePacket();
            OriginalForm.TransmitUSBPacket("[<-TX] Select SCI-bus low-speed mode:");
        }

        private void Baud62500Button_Click(object sender, EventArgs e)
        {
            if (OriginalForm.PCM.speed == "7812.5 baud")
            {
                MainForm.Packet.tx.bus = (byte)Packet.Bus.pcm;
                MainForm.Packet.tx.command = (byte)Packet.Command.msgTx;
                MainForm.Packet.tx.mode = (byte)Packet.MsgTxMode.single;
                MainForm.Packet.tx.payload = new byte[1] { 0x12 };
                MainForm.Packet.GeneratePacket();
                OriginalForm.TransmitUSBPacket("[<-TX] Select SCI-bus high-speed mode:");
            }
        }

        private void RAMTableSelectButton_Click(object sender, EventArgs e)
        {
            AddHighSpeedDiagnosticData((byte)(0xF0 + RAMTableComboBox.SelectedIndex));
        }

        private void ActuatorTestStartButton_Click(object sender, EventArgs e)
        {
            MainForm.Packet.tx.bus = (byte)Packet.Bus.pcm;
            MainForm.Packet.tx.command = (byte)Packet.Command.msgTx;
            MainForm.Packet.tx.mode = (byte)Packet.MsgTxMode.single;
            MainForm.Packet.tx.payload = new byte[2] { 0x13, (byte)ActuatorTestComboBox.SelectedIndex };
            MainForm.Packet.GeneratePacket();
            OriginalForm.TransmitUSBPacket("[<-TX] Start PCM actuator test:");
        }

        private void ActuatorTestStopButton_Click(object sender, EventArgs e)
        {
            MainForm.Packet.tx.bus = (byte)Packet.Bus.pcm;
            MainForm.Packet.tx.command = (byte)Packet.Command.msgTx;
            MainForm.Packet.tx.mode = (byte)Packet.MsgTxMode.single;
            MainForm.Packet.tx.payload = new byte[2] { 0x13, 0x00 };
            MainForm.Packet.GeneratePacket();
            OriginalForm.TransmitUSBPacket("[<-TX] Stop PCM actuator test:");
        }

        private void DiagnosticDataReadButton_Click(object sender, EventArgs e)
        {
            byte count = (byte)DiagnosticDataListBox.SelectedIndices.Count;

            if (count == 1)
            {
                if (DiagnosticDataRepeatIntervalCheckBox.Checked)
                {
                    SetupRepeat();
                    MainForm.Packet.tx.mode = (byte)Packet.MsgTxMode.repeatedSingle;
                }
                else
                {
                    MainForm.Packet.tx.mode = (byte)Packet.MsgTxMode.single;
                }

                if (OriginalForm.PCM.speed == "7812.5 baud")
                {
                    MainForm.Packet.tx.bus = (byte)Packet.Bus.pcm;
                    MainForm.Packet.tx.command = (byte)Packet.Command.msgTx;
                    MainForm.Packet.tx.payload = new byte[2] { 0x14, (byte)DiagnosticDataListBox.SelectedIndex };
                    MainForm.Packet.GeneratePacket();
                    OriginalForm.TransmitUSBPacket("[<-TX] Request diagnostic data:");
                }
                else if (OriginalForm.PCM.speed == "62500 baud")
                {
                    if (WordRequestFilter.Contains((byte)DiagnosticDataListBox.SelectedIndex))
                    {
                        MainForm.Packet.tx.payload = new byte[3] { (byte)(0xF0 + RAMTableComboBox.SelectedIndex), (byte)DiagnosticDataListBox.SelectedIndex, (byte)(DiagnosticDataListBox.SelectedIndex + 1) };
                    }
                    else
                    {
                        MainForm.Packet.tx.payload = new byte[2] { (byte)(0xF0 + RAMTableComboBox.SelectedIndex), (byte)DiagnosticDataListBox.SelectedIndex };
                    }

                    MainForm.Packet.tx.bus = (byte)Packet.Bus.pcm;
                    MainForm.Packet.tx.command = (byte)Packet.Command.msgTx;                    
                    MainForm.Packet.GeneratePacket();
                    OriginalForm.TransmitUSBPacket("[<-TX] Request diagnostic data:");
                }
            }
            else if (count > 1)
            {
                SetupRepeat();

                if (DiagnosticDataRepeatIntervalCheckBox.Checked)
                {
                    MainForm.Packet.tx.mode = (byte)Packet.MsgTxMode.repeatedList;
                }
                else
                {
                    MainForm.Packet.tx.mode = (byte)Packet.MsgTxMode.list;
                }

                if (OriginalForm.PCM.speed == "7812.5 baud")
                {
                    MainForm.Packet.tx.payload = new byte[(3 * count) + 1];
                    MainForm.Packet.tx.payload[0] = count;

                    ushort bp = 1; // buffer pointer

                    for (int i = 0; i < count; i++)
                    {
                        MainForm.Packet.tx.payload[bp] = 2; // message length
                        MainForm.Packet.tx.payload[bp + 1] = 0x14; // request ID
                        MainForm.Packet.tx.payload[bp + 2] = (byte)DiagnosticDataListBox.SelectedIndices[i]; // request parameter
                        bp += 3;
                    }

                    MainForm.Packet.tx.bus = (byte)Packet.Bus.pcm;
                    MainForm.Packet.tx.command = (byte)Packet.Command.msgTx;
                    MainForm.Packet.GeneratePacket();
                    OriginalForm.TransmitUSBPacket("[<-TX] Request diagnostic data:");
                }
                else if (OriginalForm.PCM.speed == "62500 baud")
                {
                    for (int i = 0; i < count; i++)
                    {
                        if (WordRequestFilter.Contains((byte)DiagnosticDataListBox.SelectedIndices[i]))
                        {
                            WordRequestCount++;
                        }
                    }

                    MainForm.Packet.tx.payload = new byte[1 + ((count - WordRequestCount) * 3) + WordRequestCount * 4];
                    MainForm.Packet.tx.payload[0] = count;

                    ushort bp = 1; // buffer pointer

                    for (int i = 0; i < count; i++)
                    {
                        if (WordRequestFilter.Contains((byte)DiagnosticDataListBox.SelectedIndices[i]))
                        {
                            MainForm.Packet.tx.payload[bp] = 3;
                            MainForm.Packet.tx.payload[bp + 1] = (byte)(0xF0 + RAMTableComboBox.SelectedIndex);
                            MainForm.Packet.tx.payload[bp + 2] = (byte)DiagnosticDataListBox.SelectedIndices[i];
                            MainForm.Packet.tx.payload[bp + 3] = (byte)(DiagnosticDataListBox.SelectedIndices[i] + 1); // [i + 1] would be good too but only if next line is selected
                            bp += 4;
                        }
                        else
                        {
                            MainForm.Packet.tx.payload[bp] = 2;
                            MainForm.Packet.tx.payload[bp + 1] = (byte)(0xF0 + RAMTableComboBox.SelectedIndex);
                            MainForm.Packet.tx.payload[bp + 2] = (byte)DiagnosticDataListBox.SelectedIndices[i];
                            bp += 3;
                        }
                    }

                    MainForm.Packet.tx.bus = (byte)Packet.Bus.pcm;
                    MainForm.Packet.tx.command = (byte)Packet.Command.msgTx;
                    MainForm.Packet.GeneratePacket();
                    OriginalForm.TransmitUSBPacket("[<-TX] Request diagnostic data:");
                }
            }

            if (DiagnosticDataCSVCheckBox.Checked && (count > 0))
            {
                DiagnosticItemCount = count;
                DiagnosticItems.Clear();

                if (OriginalForm.PCM.speed == "7812.5 baud")
                {
                    for (int i = 0; i < count; i++)
                    {
                        DiagnosticItems.Add(new byte[2] { 0x14, (byte)DiagnosticDataListBox.SelectedIndices[i] });
                    }
                }
                else if (OriginalForm.PCM.speed == "62500 baud")
                {
                    for (int i = 0; i < count; i++)
                    {
                        if (WordRequestFilter.Contains((byte)DiagnosticDataListBox.SelectedIndices[i]))
                        {
                            DiagnosticItems.Add(new byte[3] { (byte)(0xF0 + RAMTableComboBox.SelectedIndex), (byte)DiagnosticDataListBox.SelectedIndices[i], (byte)(DiagnosticDataListBox.SelectedIndices[i] + 1) });
                        }
                        else
                        {
                            DiagnosticItems.Add(new byte[2] { (byte)(0xF0 + RAMTableComboBox.SelectedIndex), (byte)DiagnosticDataListBox.SelectedIndices[i] });
                        }
                    }
                }

                string Header = "Milliseconds";

                foreach (var item in DiagnosticItems)
                {
                    Header += "," + Util.ByteToHexStringSimple(item);
                }

                FirstDiagnosticItemID = DiagnosticItems[0][1]; // remember first diagnostic data ID
                DiagnosticItems.Clear();

                Header = Header.Replace(" ", ""); // remove whitespaces

                if (Header != CSVHeader) // header changed, create new file
                {
                    string DateTimeNow = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    CSVFilename = @"LOG/PCM/pcmlog_" + DateTimeNow + ".csv";
                    CSVHeader = Header;
                    File.AppendAllText(CSVFilename, CSVHeader);
                }
            }
        }

        private void DiagnosticDataStopButton_Click(object sender, EventArgs e)
        {
            DiagnosticItemCount = 0;
            DiagnosticItems.Clear();

            MainForm.Packet.tx.bus = (byte)Packet.Bus.pcm;
            MainForm.Packet.tx.command = (byte)Packet.Command.msgTx;
            MainForm.Packet.tx.mode = (byte)Packet.MsgTxMode.stop;
            MainForm.Packet.tx.payload = null;
            MainForm.Packet.GeneratePacket();
            OriginalForm.TransmitUSBPacket("[<-TX] Stop diagnostic data request:");
        }

        private void DiagnosticDataClearButton_Click(object sender, EventArgs e)
        {
            DiagnosticDataListBox.ClearSelected();
        }

        private void SetIdleSpeedSetButton_Click(object sender, EventArgs e)
        {
            List<byte> payloadList = new List<byte>();
            int repeatInterval = 500;
            byte[] repeatIntervalArray = new byte[2];
            repeatIntervalArray[0] = (byte)((repeatInterval >> 8) & 0xFF);
            repeatIntervalArray[1] = (byte)(repeatInterval & 0xFF);

            payloadList.Add((byte)Packet.Bus.pcm);
            payloadList.AddRange(repeatIntervalArray);

            MainForm.Packet.tx.bus = (byte)Packet.Bus.usb;
            MainForm.Packet.tx.command = (byte)Packet.Command.settings;
            MainForm.Packet.tx.mode = (byte)Packet.SettingsMode.setRepeatBehavior;
            MainForm.Packet.tx.payload = payloadList.ToArray();
            MainForm.Packet.GeneratePacket();
            OriginalForm.TransmitUSBPacket("[<-TX] Set SCI-bus (PCM) message repeat:");

            bool success = int.TryParse(SetIdleSpeedTextBox.Text, out int SetRPM);

            if (!success || (SetRPM == 0))
            {
                SetRPM = 1500;
                SetIdleSpeedTextBox.Text = "1500";
                SetIdleSpeedTrackBar.Value = 1500;
            }

            if (SetRPM > 2000)
            {
                SetRPM = 2000;
                SetIdleSpeedTextBox.Text = "2000";
                SetIdleSpeedTrackBar.Value = 2000;
            }

            MainForm.Packet.tx.bus = (byte)Packet.Bus.pcm;
            MainForm.Packet.tx.command = (byte)Packet.Command.msgTx;
            MainForm.Packet.tx.mode = (byte)Packet.MsgTxMode.repeatedSingle;
            MainForm.Packet.tx.payload = new byte[2] { 0x19, (byte)(SetRPM / 7.85) };
            MainForm.Packet.GeneratePacket();
            OriginalForm.TransmitUSBPacket("[<-TX] Set idle speed:");
        }

        private void SetIdleSpeedStopButton_Click(object sender, EventArgs e)
        {
            MainForm.Packet.tx.bus = (byte)Packet.Bus.pcm;
            MainForm.Packet.tx.command = (byte)Packet.Command.msgTx;
            MainForm.Packet.tx.mode = (byte)Packet.MsgTxMode.stop;
            MainForm.Packet.tx.payload = null;
            MainForm.Packet.GeneratePacket();
            OriginalForm.TransmitUSBPacket("[<-TX] Restore default idle speed:");
        }

        private void SetIdleSpeedTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                e.Handled = true;

                SetIdleSpeedSetButton_Click(this, EventArgs.Empty);

                bool success = int.TryParse(SetIdleSpeedTextBox.Text, out int SetRPM);

                if (success)
                {
                    SetIdleSpeedTrackBar.Value = SetRPM;
                }
            }
        }

        private void SetIdleSpeedTrackBar_Scroll(object sender, EventArgs e)
        {
            SetIdleSpeedTextBox.Text = SetIdleSpeedTrackBar.Value.ToString("0");
        }

        private void ResetMemoryOKButton_Click(object sender, EventArgs e)
        {
            MainForm.Packet.tx.bus = (byte)Packet.Bus.pcm;
            MainForm.Packet.tx.command = (byte)Packet.Command.msgTx;
            MainForm.Packet.tx.mode = (byte)Packet.MsgTxMode.single;
            MainForm.Packet.tx.payload = new byte[2] { 0x23, (byte)ResetMemoryComboBox.SelectedIndex };
            MainForm.Packet.GeneratePacket();
            OriginalForm.TransmitUSBPacket("[<-TX] Reset memory value:");
        }

        private void SecurityUnlockButton_Click(object sender, EventArgs e)
        {
            if (SecurityLevelComboBox.SelectedIndex == 0)
            {
                MainForm.Packet.tx.bus = (byte)Packet.Bus.pcm;
                MainForm.Packet.tx.command = (byte)Packet.Command.msgTx;
                MainForm.Packet.tx.mode = (byte)Packet.MsgTxMode.single;

                if (LegacySecurityCheckBox.Checked)
                {
                    MainForm.Packet.tx.payload = new byte[1] { 0x2B };
                }
                else
                {
                    MainForm.Packet.tx.payload = new byte[2] { 0x35, 0x01 };
                }

                MainForm.Packet.GeneratePacket();
                OriginalForm.TransmitUSBPacket("[<-TX] Request level 1 security seed:");
            }
            else if (SecurityLevelComboBox.SelectedIndex == 1)
            {
                MainForm.Packet.tx.bus = (byte)Packet.Bus.pcm;
                MainForm.Packet.tx.command = (byte)Packet.Command.msgTx;
                MainForm.Packet.tx.mode = (byte)Packet.MsgTxMode.single;
                MainForm.Packet.tx.payload = new byte[2] { 0x35, 0x02 };
                MainForm.Packet.GeneratePacket();
                OriginalForm.TransmitUSBPacket("[<-TX] Request level 2 security seed:");
            }
        }

        private void LegacySecurityCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (LegacySecurityCheckBox.Checked)
            {
                SecurityLevelComboBox.Items[0] = "2B | Lvl 1";
            }
            else
            {
                SecurityLevelComboBox.Items[0] = "3501 | Lvl 1";
            }
        }

        private void ConfigurationGetButton_Click(object sender, EventArgs e)
        {
            MainForm.Packet.tx.bus = (byte)Packet.Bus.pcm;
            MainForm.Packet.tx.command = (byte)Packet.Command.msgTx;
            MainForm.Packet.tx.mode = (byte)Packet.MsgTxMode.single;
            MainForm.Packet.tx.payload = new byte[3] { 0x2A, (byte)ConfigurationComboBox.SelectedIndex, 0xFE };
            MainForm.Packet.GeneratePacket();
            OriginalForm.TransmitUSBPacket("[<-TX] Information request:");
        }

        private void ConfigurationGetPartNumberButton_Click(object sender, EventArgs e)
        {
            bool success = int.TryParse(DiagnosticDataRepeatIntervalTextBox.Text, out int repeatInterval);

            if (!success || (repeatInterval == 0))
            {
                repeatInterval = 50;
                DiagnosticDataRepeatIntervalTextBox.Text = "50";
            }

            List<byte> payloadList = new List<byte>();
            byte[] repeatIntervalArray = new byte[2];

            repeatIntervalArray[0] = (byte)((repeatInterval >> 8) & 0xFF);
            repeatIntervalArray[1] = (byte)(repeatInterval & 0xFF);

            payloadList.Add((byte)Packet.Bus.pcm);
            payloadList.AddRange(repeatIntervalArray);

            MainForm.Packet.tx.bus = (byte)Packet.Bus.usb;
            MainForm.Packet.tx.command = (byte)Packet.Command.settings;
            MainForm.Packet.tx.mode = (byte)Packet.SettingsMode.setRepeatBehavior;
            MainForm.Packet.tx.payload = payloadList.ToArray();
            MainForm.Packet.GeneratePacket();
            OriginalForm.TransmitUSBPacket("[<-TX] Set SCI-bus (PCM) message repeat:");

            const byte infoCount = 6; // part number is stored in 6 bytes

            MainForm.Packet.tx.bus = (byte)Packet.Bus.pcm;
            MainForm.Packet.tx.command = (byte)Packet.Command.msgTx;
            MainForm.Packet.tx.mode = (byte)Packet.MsgTxMode.list;
            MainForm.Packet.tx.payload = new byte[(4 * infoCount) + 1] { infoCount, 0x03, 0x2A, 0x01, 0xFE, 0x03, 0x2A, 0x02, 0xFE, 0x03, 0x2A, 0x03, 0xFE, 0x03, 0x2A, 0x04, 0xFE, 0x03, 0x2A, 0x17, 0xFE, 0x03, 0x2A, 0x18, 0xFE };
            MainForm.Packet.GeneratePacket();
            OriginalForm.TransmitUSBPacket("[<-TX] PCM part number request:");
        }

        private void ConfigurationGetAllButton_Click(object sender, EventArgs e)
        {
            bool success = int.TryParse(DiagnosticDataRepeatIntervalTextBox.Text, out int repeatInterval);

            if (!success || (repeatInterval == 0))
            {
                repeatInterval = 50;
                DiagnosticDataRepeatIntervalTextBox.Text = "50";
            }

            List<byte> payloadList = new List<byte>();
            byte[] repeatIntervalArray = new byte[2];

            repeatIntervalArray[0] = (byte)((repeatInterval >> 8) & 0xFF);
            repeatIntervalArray[1] = (byte)(repeatInterval & 0xFF);

            payloadList.Add((byte)Packet.Bus.pcm);
            payloadList.AddRange(repeatIntervalArray);

            MainForm.Packet.tx.bus = (byte)Packet.Bus.usb;
            MainForm.Packet.tx.command = (byte)Packet.Command.settings;
            MainForm.Packet.tx.mode = (byte)Packet.SettingsMode.setRepeatBehavior;
            MainForm.Packet.tx.payload = payloadList.ToArray();
            MainForm.Packet.GeneratePacket();
            OriginalForm.TransmitUSBPacket("[<-TX] Set SCI-bus (PCM) message repeat:");

            const byte infoCount = 31; // there are fixed 31 information records available

            MainForm.Packet.tx.bus = (byte)Packet.Bus.pcm;
            MainForm.Packet.tx.command = (byte)Packet.Command.msgTx;
            MainForm.Packet.tx.mode = (byte)Packet.MsgTxMode.list;
            MainForm.Packet.tx.payload = new byte[(4 * infoCount) + 1];
            MainForm.Packet.tx.payload[0] = infoCount;

            for (int i = 0; i < infoCount; i++)
            {
                MainForm.Packet.tx.payload[1 + (i * 4)] = 3; // message length
                MainForm.Packet.tx.payload[1 + (i * 4) + 1] = 0x2A; // request ID
                MainForm.Packet.tx.payload[1 + (i * 4) + 1 + 1] = (byte)(i + 1); // request parameter
                MainForm.Packet.tx.payload[1 + (i * 4) + 1 + 1 + 1] = 0xFE; // termination command, some earlier SBEC3 won't stop streaming response byte without it
            }

            MainForm.Packet.GeneratePacket();
            OriginalForm.TransmitUSBPacket("[<-TX] Information request:");
        }

        private void PacketReceivedHandler(object sender, EventArgs e)
        {
            switch (MainForm.Packet.rx.bus)
            {
                case (byte)Packet.Bus.pcm:
                case (byte)Packet.Bus.tcm:
                    byte[] SCIBusResponseBytes = MainForm.Packet.rx.payload.Skip(4).ToArray(); // skip 4 timestamp bytes

                    if (SCIBusResponseBytes.Length == 0) break;

                    if (OriginalForm.PCM.speed == "7812.5 baud")
                    {
                        switch (SCIBusResponseBytes[0])
                        {
                            case (byte)SCI_ID.SCIHiSpeed:
                                HighSpeedLayout();
                                AddHighSpeedDiagnosticData((byte)(0xF0 + RAMTableComboBox.SelectedIndex));
                                break;
                            case (byte)SCI_ID.DiagnosticData:
                                if (SCIBusResponseBytes.Length < 3)
                                {
                                    if (DiagnosticDataCSVCheckBox.Checked)
                                    {
                                        DiagnosticItems.Add(new byte[3] { 0x14, 0x00, 0x00 }); // an invalid response would break the CSV-file format, add an empty response instead
                                    }
                                    break;
                                }

                                if (DiagnosticDataCSVCheckBox.Checked && (DiagnosticItemCount > 0))
                                {
                                    DiagnosticItems.Add(SCIBusResponseBytes);

                                    if ((DiagnosticItems.Count == 1) && (SCIBusResponseBytes[1] != FirstDiagnosticItemID)) // keep columns ordered
                                    {
                                        DiagnosticItems.Clear();
                                    }
                                }

                                if ((DiagnosticItems.Count == DiagnosticItemCount) && (DiagnosticItemCount > 0))
                                {
                                    uint Millis = (uint)((MainForm.Packet.rx.payload[0] << 24) + (MainForm.Packet.rx.payload[1] << 16) + (MainForm.Packet.rx.payload[2] << 8) + MainForm.Packet.rx.payload[3]);
                                    CSVLine = Millis.ToString();

                                    foreach (var item in DiagnosticItems)
                                    {
                                        CSVLine += "," + item[2].ToString();
                                    }

                                    File.AppendAllText(CSVFilename, Environment.NewLine + CSVLine);
                                    DiagnosticItems.Clear();
                                }
                                break;
                            case (byte)SCI_ID.ConfigData:
                                if (SCIBusResponseBytes.Length < 3) break;

                                UpdateCHTGroup();
                                UpdateStatusBar();
                                break;
                            case (byte)SCI_ID.GetSecuritySeedLegacy:
                                if (SCIBusResponseBytes.Length < 4) break;

                                byte ChecksumA = (byte)(SCIBusResponseBytes[0] + SCIBusResponseBytes[1] + SCIBusResponseBytes[2]);

                                if (SCIBusResponseBytes[3] != ChecksumA) break;

                                ushort SeedA = (ushort)((SCIBusResponseBytes[1] << 8) + SCIBusResponseBytes[2]);

                                if (SeedA == 0) break;

                                ushort KeyA = (ushort)((SeedA << 2) + 0x9018);
                                byte KeyHBA = (byte)(KeyA >> 8);
                                byte KeyLBA = (byte)(KeyA);
                                byte KeyChecksumA = (byte)(0x2C + KeyHBA + KeyLBA);
                                byte[] KeyArrayA = { (byte)SCI_ID.SendSecurityKey, KeyHBA, KeyLBA, KeyChecksumA };

                                MainForm.Packet.tx.bus = (byte)Packet.Bus.pcm;
                                MainForm.Packet.tx.command = (byte)Packet.Command.msgTx;
                                MainForm.Packet.tx.mode = (byte)Packet.MsgTxMode.single;
                                MainForm.Packet.tx.payload = KeyArrayA;
                                MainForm.Packet.GeneratePacket();
                                OriginalForm.TransmitUSBPacket("[<-TX] Send level 1 security key:");
                                break;
                            case (byte)SCI_ID.GetSecuritySeed:
                                if (SCIBusResponseBytes.Length < 5) break;

                                if (SCIBusResponseBytes[1] == 0x01)
                                {
                                    byte ChecksumB = (byte)(SCIBusResponseBytes[0] + SCIBusResponseBytes[1] + SCIBusResponseBytes[2] + SCIBusResponseBytes[3]);

                                    if (SCIBusResponseBytes[4] != ChecksumB) break;

                                    ushort SeedB = (ushort)((SCIBusResponseBytes[2] << 8) + SCIBusResponseBytes[3]);

                                    if (SeedB == 0) break;

                                    ushort KeyB = (ushort)((SeedB << 2) + 0x9018);
                                    byte KeyHBB = (byte)(KeyB >> 8);
                                    byte KeyLBB = (byte)(KeyB);
                                    byte KeyChecksumB = (byte)(0x2C + KeyHBB + KeyLBB);
                                    byte[] KeyArrayB = { (byte)SCI_ID.SendSecurityKey, KeyHBB, KeyLBB, KeyChecksumB };

                                    MainForm.Packet.tx.bus = (byte)Packet.Bus.pcm;
                                    MainForm.Packet.tx.command = (byte)Packet.Command.msgTx;
                                    MainForm.Packet.tx.mode = (byte)Packet.MsgTxMode.single;
                                    MainForm.Packet.tx.payload = KeyArrayB;
                                    MainForm.Packet.GeneratePacket();
                                    OriginalForm.TransmitUSBPacket("[<-TX] Send level 1 security key:");
                                }
                                else if (SCIBusResponseBytes[1] == 0x02)
                                {
                                    byte ChecksumC = (byte)(SCIBusResponseBytes[0] + SCIBusResponseBytes[1] + SCIBusResponseBytes[2] + SCIBusResponseBytes[3]);

                                    if (SCIBusResponseBytes[4] != ChecksumC) break;

                                    ushort SeedC = (ushort)((SCIBusResponseBytes[2] << 8) + SCIBusResponseBytes[3]);

                                    if (SeedC == 0) break;

                                    ushort KeyC = (ushort)(SeedC & 0xFF00);
                                    KeyC |= (ushort)(KeyC >> 8);

                                    ushort mask = (ushort)(SeedC & 0xFF);
                                    mask |= (ushort)(mask << 8);
                                    KeyC ^= 0x9340; // polinom
                                    KeyC += 0x1010;
                                    KeyC ^= mask;
                                    KeyC += 0x1911;
                                    uint tmp = (uint)((KeyC << 16) | KeyC);
                                    KeyC += (ushort)(tmp >> 3);
                                    byte KeyHBC = (byte)(KeyC >> 8);
                                    byte KeyLBC = (byte)(KeyC);
                                    byte KeyChecksumC = (byte)(0x2C + KeyHBC + KeyLBC);
                                    byte[] KeyArrayC = { 0x2C, KeyHBC, KeyLBC, KeyChecksumC };

                                    MainForm.Packet.tx.bus = (byte)Packet.Bus.pcm;
                                    MainForm.Packet.tx.command = (byte)Packet.Command.msgTx;
                                    MainForm.Packet.tx.mode = (byte)Packet.MsgTxMode.single;
                                    MainForm.Packet.tx.payload = KeyArrayC;
                                    MainForm.Packet.GeneratePacket();
                                    OriginalForm.TransmitUSBPacket("[<-TX] Send level 2 security key:");
                                }
                                break;
                        }
                    }
                    else if (OriginalForm.PCM.speed == "62500 baud")
                    {
                        if ((SCIBusResponseBytes[0] >= 0xF0) && (SCIBusResponseBytes[0] <= 0xFD))
                        {
                            if (SCIBusResponseBytes.Length >= 3)
                            {
                                if (DiagnosticDataCSVCheckBox.Checked && (DiagnosticItemCount > 0))
                                {
                                    DiagnosticItems.Add(SCIBusResponseBytes);

                                    if ((DiagnosticItems.Count == 1) && (SCIBusResponseBytes[1] != FirstDiagnosticItemID)) // keep columns ordered
                                    {
                                        DiagnosticItems.Clear();
                                    }
                                }

                                if ((DiagnosticItems.Count == DiagnosticItemCount) && (DiagnosticItemCount > 0))
                                {
                                    uint Millis = (uint)((MainForm.Packet.rx.payload[0] << 24) + (MainForm.Packet.rx.payload[1] << 16) + (MainForm.Packet.rx.payload[2] << 8) + MainForm.Packet.rx.payload[3]);
                                    CSVLine = Millis.ToString();

                                    foreach (var item in DiagnosticItems)
                                    {
                                        CSVLine += "," + item[2].ToString();
                                    }

                                    File.AppendAllText(CSVFilename, Environment.NewLine + CSVLine);
                                    DiagnosticItems.Clear();
                                }
                            }
                            else if (SCIBusResponseBytes.Length < 3)
                            {
                                if (DiagnosticDataCSVCheckBox.Checked)
                                {
                                    DiagnosticItems.Add(new byte[3] { (byte)(0xF0 + RAMTableComboBox.SelectedIndex), 0x00, 0x00 }); // an invalid response would break the CSV-file format, add an empty response instead
                                }
                            }
                        }
                        else if (SCIBusResponseBytes[0] == (byte)SCI_ID.SCILoSpeed)
                        {
                            LowSpeedLayout();
                            AddLowSpeedDiagnosticData();
                        }
                    }
                    break;
            }
        }

        private void LowSpeedLayout()
        {
            CHTGroupBox.Enabled = true;
            EraseFaultCodesButton.Enabled = true;
            ReadFaultCodeFreezeFrameButton.Enabled = false;
            Baud7812Button.BackColor = Color.RoyalBlue;
            Baud7812Button.ForeColor = Color.White;
            Baud7812Button.FlatAppearance.BorderColor = Color.RoyalBlue;
            Baud7812Button.FlatAppearance.BorderSize = 1;
            Baud62500Button.BackColor = SystemColors.ControlLight;
            Baud62500Button.ForeColor = SystemColors.ControlText;
            Baud62500Button.FlatAppearance.BorderColor = SystemColors.ActiveBorder;
            Baud62500Button.FlatAppearance.BorderSize = 1;
            RAMTableGroupBox.Enabled = false;
            ActuatorTestGroupBox.Enabled = true;
            DiagnosticDataGroupBox.Enabled = true;
            SetIdleSpeedGroupBox.Enabled = true;
            ResetMemoryGroupBox.Enabled = true;
            ConfigurationGroupBox.Enabled = true;
            SecurityGroupBox.Enabled = true;
        }

        private void AddLowSpeedDiagnosticData()
        {
            DiagnosticDataListBox.Items.Clear();
            DiagnosticDataListBox.Items.AddRange(new object[] {
            "1400 |",
            "1401 | Battery temperature sensor voltage",
            "1402 | Upstream O2 1/1 sensor voltage",
            "1403 |",
            "1404 |",
            "1405 | Engine coolant temperature",
            "1406 | Engine coolant temperature sensor voltage",
            "1407 | Throttle position sensor voltage",
            "1408 | Minimum throttle position sensor voltage",
            "1409 | Knock sensor 1 voltage",
            "140A | Battery voltage",
            "140B | Intake manifold absolute pressure (MAP)",
            "140C | Target IAC stepper motor position",
            "140D |",
            "140E | Long term fuel trim 1",
            "140F | Barometric pressure",
            "1410 | Minimum air flow test",
            "1411 | Engine speed",
            "1412 | Cam/Crank sync sense",
            "1413 | Key-on cycles error 1",
            "1414 |",
            "1415 | Spark advance",
            "1416 | Cylinder 1 retard",
            "1417 | Cylinder 2 retard",
            "1418 | Cylinder 3 retard",
            "1419 | Cylinder 4 retard",
            "141A | Target boost",
            "141B | Intake air temperature",
            "141C | Intake air temperature sensor voltage",
            "141D | Cruise set speed",
            "141E | Key-on cycles error 2",
            "141F | Key-on cycles error 3",
            "1420 | Cruise control status 1",
            "1421 | Cylinder 1 retard",
            "1422 |",
            "1423 |",
            "1424 | Battery charging voltage",
            "1425 | Over 5 psi boost timer",
            "1426 |",
            "1427 | Vehicle theft alarm status",
            "1428 | Wastegate duty cycle",
            "1429 | Read fuel setting",
            "142A | Read set sync",
            "142B |",
            "142C | Cruise switch voltage sense",
            "142D | Ambient/Battery temperature",
            "142E | Fuel factor (not LH)",
            "142F | Upstream O2 2/1 sensor voltage",
            "1430 | Knock sensor 2 voltage",
            "1431 | Long term fuel trim 2",
            "1432 | A/C high-side pressure sensor voltage",
            "1433 | A/C high-side pressure",
            "1434 | Flex fuel sensor voltage",
            "1435 | Flex fuel info 1",
            "1436 |",
            "1437 |",
            "1438 |",
            "1439 |",
            "143A |",
            "143B | Fuel system status 1",
            "143C |",
            "143D |",
            "143E | Calpot voltage",
            "143F | Downstream O2 1/2 sensor voltage",
            "1440 | MAP sensor voltage",
            "1441 | Vehicle speed",
            "1442 | Upstream O2 1/1 sensor level",
            "1443 |",
            "1444 |",
            "1445 | MAP vacuum",
            "1446 | Throttle position relative",
            "1447 | Spark advance",
            "1448 | Upstream O2 2/1 sensor level",
            "1449 | Downstream O2 2/2 sensor voltage",
            "144A | Downstream O2 1/2 sensor level",
            "144B | Downstream O2 2/2 sensor level",
            "144C |",
            "144D |",
            "144E | Fuel level sensor voltage",
            "144F | Fuel level",
            "1450 |",
            "1451 |",
            "1452 |",
            "1453 |",
            "1454 |",
            "1455 |",
            "1456 |",
            "1457 | Fuel system status 2",
            "1458 | Cruise control status 1",
            "1459 | Cruise control status 2",
            "145A | Output shaft speed",
            "145B | Governor pressure duty cycle",
            "145C | Engine load",
            "145D |",
            "145E |",
            "145F | EGR position sensor voltage",
            "1460 | EGR Zref update D.C.",
            "1461 |",
            "1462 |",
            "1463 |",
            "1464 | Actual purge current",
            "1465 | Catalyst temperature sensor voltage",
            "1466 | Catalyst temperature",
            "1467 |",
            "1468 |",
            "1469 | Ambient temperature sensor voltage",
            "146A |",
            "146B |",
            "146C |",
            "146D | T-case switch voltage",
            "146E |",
            "146F |",
            "1470 |",
            "1471 |",
            "1472 |",
            "1473 |",
            "1474 |",
            "1475 |",
            "1476 |",
            "1477 |",
            "1478 |",
            "1479 |",
            "147A | FCA current",
            "147B |",
            "147C | Oil temperature sensor voltage",
            "147D | Oil temperature",
            "147E |",
            "147F |"});
        }

        private void HighSpeedLayout()
        {
            CHTGroupBox.Enabled = false;
            EraseFaultCodesButton.Enabled = false;
            ReadFaultCodeFreezeFrameButton.Enabled = true;
            Baud7812Button.BackColor = SystemColors.ControlLight;
            Baud7812Button.ForeColor = SystemColors.ControlText;
            Baud7812Button.FlatAppearance.BorderColor = SystemColors.ActiveBorder;
            Baud7812Button.FlatAppearance.BorderSize = 1;
            Baud62500Button.BackColor = Color.IndianRed;
            Baud62500Button.ForeColor = Color.White;
            Baud62500Button.FlatAppearance.BorderColor = Color.IndianRed;
            Baud62500Button.FlatAppearance.BorderSize = 1;
            RAMTableGroupBox.Enabled = true;
            ActuatorTestGroupBox.Enabled = false;
            DiagnosticDataGroupBox.Enabled = true;
            SetIdleSpeedGroupBox.Enabled = false;
            ResetMemoryGroupBox.Enabled = false;
            ConfigurationGroupBox.Enabled = false;
            SecurityGroupBox.Enabled = false;
        }

        private void AddHighSpeedDiagnosticData(byte RAMTable)
        {
            DiagnosticDataListBox.Items.Clear();

            switch (RAMTable)
            {
                case 0xF0:
                    DiagnosticDataListBox.Items.AddRange(new object[] {
                    "F000 |"});
                    break;
                case 0xF1:
                    DiagnosticDataListBox.Items.AddRange(new object[] {
                    "F100 |"});
                    break;
                case 0xF2:
                    DiagnosticDataListBox.Items.AddRange(new object[] {
                    "F200 |"});
                    break;
                case 0xF3:
                    DiagnosticDataListBox.Items.AddRange(new object[] {
                    "F300 |"});
                    break;
                case 0xF4:
                    DiagnosticDataListBox.Items.AddRange(new object[] {
                    "F400 |",
                    "F401 | DTC 1",
                    "F402 | DTC 8",
                    "F403 | Key-on cycles error 1",
                    "F404 | Key-on cycles error 2",
                    "F405 | Key-on cycles error 3",
                    "F406 | DTC counter 1",
                    "F407 | DTC counter 2",
                    "F408 | DTC counter 3",
                    "F409 | DTC counter 4",
                    "F40A0B | Engine speed",
                    "F40B |",
                    "F40C0D | Vehicle speed",
                    "F40D |",
                    "F40E | Cruise button pressed",
                    "F40F | Battery voltage",
                    "F410 | Ambient temperature sensor voltage",
                    "F411 | Ambient temperature",
                    "F412 | Thorttle position sensor (TPS) voltage",
                    "F413 | Minimum TPS voltage",
                    "F414 | Calculated TPS voltage",
                    "F415 | Engine coolant temperature sensor voltage",
                    "F416 | Engine coolant temperature",
                    "F417 | Intake MAP sensor voltage",
                    "F418 | Intake manifold absolute pressure",
                    "F419 | Barometric pressure",
                    "F41A | MAP vacuum",
                    "F41B | Upstream O2 1/1 sensor voltage",
                    "F41C | Upstream O2 2/1 sensor voltage",
                    "F41D | Intake air temperature sensor voltage",
                    "F41E | Intake air temperature",
                    "F41F | Knock sensor 1 voltage",
                    "F420 | Knock sensor 2 voltage",
                    "F421 | Cruise switch voltage",
                    "F422 | Battery temperature sensor voltage",
                    "F423 | Flex fuel sensor voltage",
                    "F424 | Flex fuel ethanol percentage",
                    "F425 | A/C high-side pressure sensor voltage",
                    "F426 | A/C high-side pressure",
                    "F42728 | Injector pulse width 1",
                    "F428 |",
                    "F4292A | Injector pulse width 2",
                    "F42A |",
                    "F42B | Long term fuel trim 1",
                    "F42C | Long term fuel trim 2",
                    "F42D | Engine coolant temperature 2",
                    "F42E | Engine coolant temperature 3",
                    "F42F | Spark advance",
                    "F430 | Total knock retard",
                    "F431 | Cylinder 1 retard",
                    "F432 | Cylinder 2 retard",
                    "F433 | Cylinder 3 retard",
                    "F434 | Cylinder 4 retard",
                    "F43536 | Target idle speed",
                    "F436 |",
                    "F437 | Target idle air control motor steps",
                    "F438 |",
                    "F439 |",
                    "F43A | Charging voltage",
                    "F43B | Cruise set speed",
                    "F43C3D | Bit state 5",
                    "F43D | ",
                    "F43E | Idle air control motor steps",
                    "F43F | Cruise control status 1",
                    "F440 | Vehicle theft alarm status",
                    "F441 | Crankshaft/Camshaft sync state",
                    "F442 | Fuel system status 1",
                    "F443 | Current adaptive cell ID",
                    "F444 | Short term fuel trim 1",
                    "F445 | Short term fuel trim 2",
                    "F446 | Emission settings 1",
                    "F447 | Emission settings 2",
                    "F448 | Downstream O2 1/2 sensor voltage",
                    "F449 | Downstream O2 2/2 sensor voltage",
                    "F44A | Closed loop timer",
                    "F44B4C | Time from start/run",
                    "F44C |",
                    "F44D | Runtime at stall",
                    "F44E | Current fuel shutoff",
                    "F44F | History of fuel shutoff",
                    "F450 |",
                    "F451 | Adaptive numerator 1",
                    "F452 |",
                    "F453 |",
                    "F454 |",
                    "F455 |",
                    "F456 |",
                    "F457 | RPM/VSS ratio",
                    "F458 | Transmission selected gear 2",
                    "F459 |",
                    "F45A | Dwell coil 1 (cylinders 1 & 4)",
                    "F45B | Dwell coil 2 (cylinders 2 & 3)",
                    "F45C | Dwell coil 3 (cylinders 3 & 6)",
                    "F45D | Fan duty cycle",
                    "F45E |",
                    "F45F |",
                    "F460 | A/C relay state",
                    "F461 | Distance traveled up to 4.2 miles",
                    "F462 |",
                    "F463 |",
                    "F464 |",
                    "F465 |",
                    "F466 |",
                    "F467 |",
                    "F468 |",
                    "F469 |",
                    "F46A |",
                    "F46B |",
                    "F46C |",
                    "F46D |",
                    "F46E |",
                    "F46F |",
                    "F470 |",
                    "F471 |",
                    "F472 |",
                    "F473 | Limp-in status",
                    "F474 | DTC 2",
                    "F475 | DTC 3",
                    "F476 | DTC 4",
                    "F477 | DTC 5",
                    "F478 | DTC 6",
                    "F479 | DTC 7",
                    "F47A7B | SPI transfer result",
                    "F47B |",
                    "F47C |",
                    "F47D |",
                    "F47E |",
                    "F47F |",
                    "F480 |",
                    "F481 |",
                    "F482 |",
                    "F483 |",
                    "F484 |",
                    "F485 |",
                    "F486 |",
                    "F487 |",
                    "F488 |",
                    "F489 |",
                    "F48A |",
                    "F48B |",
                    "F48C |",
                    "F48D |",
                    "F48E | Bit states 7",
                    "F48F |",
                    "F490 |",
                    "F491 |",
                    "F492 |",
                    "F493 |",
                    "F494 | EGR Zref update D.C.",
                    "F495 | EGR position sensor voltage",
                    "F496 | Actual purge current",
                    "F497 |",
                    "F498 | TPS intermittent counter",
                    "F499 |",
                    "F49A |",
                    "F49B | Transmission temperature",
                    "F49C |",
                    "F49D |",
                    "F49E |",
                    "F49F |",
                    "F4A0 |",
                    "F4A1 |",
                    "F4A2 |",
                    "F4A3 | Cam timing position",
                    "F4A4 | Engine good trip counter",
                    "F4A5 | Engine warm-up cycle counter",
                    "F4A6 | OBD2 monitor test results 1",
                    "F4A7 | Freeze frame priority level",
                    "F4A8 | Freeze frame DTC",
                    "F4A9 | Fuel system status 1",
                    "F4AA | Fuel system status 2",
                    "F4AB | Engine load",
                    "F4AC | Engine coolant temperature",
                    "F4AD | Short term fuel trim 1",
                    "F4AE | Long term fuel trim 1",
                    "F4AF | Short term fuel trim 2",
                    "F4B0 | Long term fuel trum 2",
                    "F4B1 | Intake manifold absolute pressure",
                    "F4B2 | Engine speed",
                    "F4B3 | Vehicle speed",
                    "F4B4 | MAP vacuum",
                    "F4B5 | Last emission related DTC stored",
                    "F4B6 | Catalyst temperature sensor voltage",
                    "F4B7 | Purge duty cycle",
                    "F4B8 |",
                    "F4B9 |",
                    "F4BA |",
                    "F4BB |",
                    "F4BC |",
                    "F4BD | Brake switch monitor result 0",
                    "F4BE | Brake switch monitor result 1",
                    "F4BF | Brake switch monitor result 2",
                    "F4C0 | Catalyst temperature",
                    "F4C1 | Fuel level status 1",
                    "F4C2 | Fuel level sensor voltage 3",
                    "F4C3 |",
                    "F4C4 |",
                    "F4C5 |",
                    "F4C6 |",
                    "F4C7 |",
                    "F4C8 |",
                    "F4C9 |",
                    "F4CA |",
                    "F4CB |",
                    "F4CC |",
                    "F4CD |",
                    "F4CE |",
                    "F4CF |",
                    "F4D0 |",
                    "F4D1 |",
                    "F4D2 | Sensor rationality result 0",
                    "F4D3 | Sensor rationality result 1",
                    "F4D4 | O2 sensor rationality result 0",
                    "F4D5 | O2 sensor rationality result 1",
                    "F4D6 | Torque converter clutch monitor",
                    "F4D7 | Sensor rationality result 2",
                    "F4D8 | Sensor rationality result 3",
                    "F4D9 |",
                    "F4DA | P_PCM_NOC_STRDCAMTIME",
                    "F4DB | Fuel level sensor voltage 2",
                    "F4DC |",
                    "F4DD | Configuration 1",
                    "F4DE | Fuel level sensor voltage 1",
                    "F4DF | Fuel level",
                    "F4E0 | Fuel used",
                    "F4E1 |",
                    "F4E2 |",
                    "F4E3 | Engine load",
                    "F4E4 | OBD2 monitor test results 2",
                    "F4E5 | Configuration 2",
                    "F4E6 |",
                    "F4E7 | Cell #1 - idle cell",
                    "F4E8 | Cell #2 - 1st off idle cell",
                    "F4E9 | Cell #3 - 2nd off idle cell",
                    "F4EA |",
                    "F4EB |",
                    "F4EC | Fuel system status 2",
                    "F4ED | Cruise control status 1",
                    "F4EE | Cruise control satuts 2",
                    "F4EF | Calculated TPS voltage"});
                    break;
                case 0xF5:
                    DiagnosticDataListBox.Items.AddRange(new object[] {
                    "F500 |"});
                    break;
                case 0xF6:
                    DiagnosticDataListBox.Items.AddRange(new object[] {
                    "F600 |"});
                    break;
                case 0xF7:
                    DiagnosticDataListBox.Items.AddRange(new object[] {
                    "F700 |"});
                    break;
                case 0xF8:
                    DiagnosticDataListBox.Items.AddRange(new object[] {
                    "F800 | CUMMINS DTC FREEZE FRAMES"});
                    break;
                case 0xF9:
                    DiagnosticDataListBox.Items.AddRange(new object[] {
                    "F900 |"});
                    break;
                case 0xFA:
                    DiagnosticDataListBox.Items.AddRange(new object[] {
                    "FA00 |"});
                    break;
                case 0xFB:
                    DiagnosticDataListBox.Items.AddRange(new object[] {
                    "FB00 |"});
                    break;
                case 0xFC:
                    DiagnosticDataListBox.Items.AddRange(new object[] {
                    "FC00 |"});
                    break;
                case 0xFD:
                    DiagnosticDataListBox.Items.AddRange(new object[] {
                    "FD00 |"});
                    break;
                default:
                    break;

            }
        }

        private void SetupRepeat()
        {
            bool success = int.TryParse(DiagnosticDataRepeatIntervalTextBox.Text, out int repeatInterval);

            if (!success || (repeatInterval == 0))
            {
                repeatInterval = 50;
                DiagnosticDataRepeatIntervalTextBox.Text = "50";
            }

            List<byte> payloadList = new List<byte>();
            byte[] repeatIntervalArray = new byte[2];

            repeatIntervalArray[0] = (byte)((repeatInterval >> 8) & 0xFF);
            repeatIntervalArray[1] = (byte)(repeatInterval & 0xFF);

            payloadList.Add((byte)Packet.Bus.pcm);
            payloadList.AddRange(repeatIntervalArray);

            MainForm.Packet.tx.bus = (byte)Packet.Bus.usb;
            MainForm.Packet.tx.command = (byte)Packet.Command.settings;
            MainForm.Packet.tx.mode = (byte)Packet.SettingsMode.setRepeatBehavior;
            MainForm.Packet.tx.payload = payloadList.ToArray();
            MainForm.Packet.GeneratePacket();
            OriginalForm.TransmitUSBPacket("[<-TX] Set SCI-bus (PCM) message repeat:");
        }

        private void UpdateCHTGroup()
        {
            CHTComboBox.SelectedIndex = OriginalForm.PCM.ControllerHardwareType;
        }

        private void UpdateStatusBar()
        {
            if (OriginalForm.PCM.PartNumberChars[0] == 0)
            {
                EnginePropertiesLabel.Text = "P/N | Year | Body | Manufacturer | Engine | Fuel | Injection" + Environment.NewLine + "Emission | Aspiration | Fans | Chassis ";
                return;
            }
            
            string StatusLabelText = string.Empty;
            byte SearchByte = 0;

            if (Array.IndexOf(OriginalForm.PCM.PartNumberChars.Take(4).ToArray(), SearchByte) == -1) // first 4 bytes of part number is ready
            {
                StatusLabelText += " P/N " + Util.ByteToHexString(OriginalForm.PCM.PartNumberChars, 0, 4).Replace(" ", ""); // show part number

                if ((OriginalForm.PCM.PartNumberChars[4] >= 0x41) && (OriginalForm.PCM.PartNumberChars[4] <= 0x5A) && (OriginalForm.PCM.PartNumberChars[5] >= 0x41) && (OriginalForm.PCM.PartNumberChars[5] <= 0x5A))
                {
                    StatusLabelText += Encoding.ASCII.GetString(OriginalForm.PCM.PartNumberChars.Skip(4).ToArray()); // show revision characters if available
                }
            }

            StatusLabelText += " | "; // add separator

            // Fill first line.
            for (int i = 0; i < 6; i++)
            {
                StatusLabelText += OriginalForm.PCM.EngineToolsStatusBarTextItems[i] + " | ";
            }

            StatusLabelText = StatusLabelText.Remove(StatusLabelText.Length - 3).TrimEnd(); // remove last "|" character
            StatusLabelText += Environment.NewLine + " ";

            // Fill second line.
            for (int i = 6; i < 11; i++)
            {
                StatusLabelText += OriginalForm.PCM.EngineToolsStatusBarTextItems[i] + " | ";
            }

            StatusLabelText = StatusLabelText.Remove(StatusLabelText.Length - 3).TrimEnd(); // remove last "|" character
            EnginePropertiesLabel.Text = StatusLabelText;
        }

        private void EngineToolsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            MainForm.Packet.PacketReceived -= PacketReceivedHandler; // unsubscribe from the OnPacketReceived event
        }
    }
}
