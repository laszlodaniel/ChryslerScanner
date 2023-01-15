using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
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
        private List<byte[]> WordRequestFilter = new List<byte[]>();
        private byte WordRequestCount = 0;
        private List<byte[]> DWordRequestFilter = new List<byte[]>();
        private byte DWordRequestCount = 0;

        private enum SCI_ID
        {
            SCIHiSpeed = 0x12,
            ActuatorTest = 0x13,
            DiagnosticData = 0x14,
            ResetMemory = 0x23,
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
            OriginalForm.ChangeLanguage();

            CHTComboBox.SelectedIndex = OriginalForm.PCM.ControllerHardwareType;
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

            // SCI: FX PP QQ
            WordRequestFilter.Add(new byte[] { 0x00 }); // F0 RAM table
            WordRequestFilter.Add(new byte[] { 0x00 }); // F1 RAM table
            WordRequestFilter.Add(new byte[] { 0x00 }); // F2 RAM table
            WordRequestFilter.Add(new byte[] { 0x00 }); // F3 RAM table
            WordRequestFilter.Add(new byte[] { 0x0A, 0x0C, 0x27, 0x29, 0x35, 0x3C, 0x4B, 0x7A }); // F4 RAM table
            WordRequestFilter.Add(new byte[] { 0x1E, 0x47, 0x49, 0x4B, 0x4D, 0xCB }); // F5 RAM table
            WordRequestFilter.Add(new byte[] { 0x00 }); // F6 RAM table
            WordRequestFilter.Add(new byte[] { 0x00 }); // F7 RAM table
            WordRequestFilter.Add(new byte[] { 0x06, 0x08, 0x0C, 0x0E, 0x10, 0x12, 0x14, 0x1F, 0x21, 0x23, 0x25, 0x29, 0x2D, 0x31, 0x38, 0x3A, 0x3E, 0x40, 0x42, 0x44, 0x46, 0x51, 0x53, 0x55, 0x57, 0x5B, 0x5F, 0x63 }); // F8 RAM table (2003+ CUMMINS, pre-2003 is loaded once Year is received)
            WordRequestFilter.Add(new byte[] { 0x00 }); // F9 RAM table
            WordRequestFilter.Add(new byte[] { 0x00 }); // FA RAM table
            WordRequestFilter.Add(new byte[] { 0x01, 0x03, 0x05, 0x07, 0x0B, 0x0F, 0x11, 0x13, 0x15, 0x17, 0x19 }); // FB RAM table (2003+ CUMMINS, pre-2003 is loaded once Year is received)
            WordRequestFilter.Add(new byte[] { 0x29 }); // FC RAM table (2003+ CUMMINS only)
            WordRequestFilter.Add(new byte[] { 0x21, 0x23, 0x27, 0x29, 0x80, 0x82, 0x84, 0x86, 0x88, 0x8A, 0x8C, 0x8E, 0x90, 0x92, 0x94, 0x96, 0xA0, 0xA4, 0xA6 }); // FD RAM table (2003+ CUMMINS only)

            // SCI: FX PP QQ RR SS
            DWordRequestFilter.Add(new byte[] { 0x00 }); // F0 RAM table
            DWordRequestFilter.Add(new byte[] { 0x00 }); // F1 RAM table
            DWordRequestFilter.Add(new byte[] { 0x00 }); // F2 RAM table
            DWordRequestFilter.Add(new byte[] { 0x00 }); // F3 RAM table
            DWordRequestFilter.Add(new byte[] { 0x00 }); // F4 RAM table
            DWordRequestFilter.Add(new byte[] { 0x00 }); // F5 RAM table
            DWordRequestFilter.Add(new byte[] { 0x00 }); // F6 RAM table
            DWordRequestFilter.Add(new byte[] { 0x00 }); // F7 RAM table
            DWordRequestFilter.Add(new byte[] { 0x00 }); // F8 RAM table
            DWordRequestFilter.Add(new byte[] { 0x00 }); // F9 RAM table
            DWordRequestFilter.Add(new byte[] { 0x00 }); // FA RAM table
            DWordRequestFilter.Add(new byte[] { 0x00 }); // FB RAM table
            DWordRequestFilter.Add(new byte[] { 0x01, 0x05, 0x09, 0x0D, 0x11, 0x15, 0x19, 0x1D, 0x21, 0x25, 0x2B, 0x2F }); // FC RAM table
            DWordRequestFilter.Add(new byte[] { 0x7C }); // FD RAM table

            ActiveControl = CHTDetectButton;
        }

        private void EngineToolsForm_Load(object sender, EventArgs e)
        {
            UpdateCHTGroup();
            UpdateStatusBar();
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
            switch (CHTComboBox.SelectedIndex)
            {
                case 9: // CUMMINS
                case 21:
                case 22:
                case 23:
                case 24:
                case 29:
                    RAMTableComboBox.SelectedIndex = 11; // FB
                    OriginalForm.PCM.CumminsSelected = true;
                    break;
                default:
                    RAMTableComboBox.SelectedIndex = 4; // F4
                    OriginalForm.PCM.CumminsSelected = false;
                    break;
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

                if (OriginalForm.PCM.CumminsSelected)
                {                                           // cnt   len   msg   len   msg
                    MainForm.Packet.tx.payload = new byte[5] { 0x02, 0x01, 0x32, 0x01, 0x33 }; // request stored and one-trip fault codes
                }
                else
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

                if (OriginalForm.PCM.CumminsSelected)
                {
                    MainForm.Packet.tx.payload = new byte[25] { 0x08, 0x02, 0xFB, 0xBB, 0x02, 0xFB, 0xBC, 0x02, 0xFB, 0xBD, 0x02, 0xFB, 0xBE, 0x02, 0xFB, 0xBF, 0x02, 0xFB, 0xC0, 0x02, 0xFB, 0xC1, 0x02, 0xFB, 0xC2 };
                }
                else
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

                if (OriginalForm.PCM.CumminsSelected)
                {
                    MainForm.Packet.tx.payload = new byte[2] { 0x25, 0x01 };
                }
                else
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
            if (OriginalForm.PCM.CumminsSelected)
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
                    if (WordRequestFilter[RAMTableComboBox.SelectedIndex].Contains((byte)DiagnosticDataListBox.SelectedIndex))
                    {
                        MainForm.Packet.tx.payload = new byte[3] { (byte)(0xF0 + RAMTableComboBox.SelectedIndex), (byte)DiagnosticDataListBox.SelectedIndex, (byte)(DiagnosticDataListBox.SelectedIndex + 1) };
                    }
                    else if (DWordRequestFilter[RAMTableComboBox.SelectedIndex].Contains((byte)DiagnosticDataListBox.SelectedIndex))
                    {
                        MainForm.Packet.tx.payload = new byte[5] { (byte)(0xF0 + RAMTableComboBox.SelectedIndex), (byte)DiagnosticDataListBox.SelectedIndex, (byte)(DiagnosticDataListBox.SelectedIndex + 1), (byte)(DiagnosticDataListBox.SelectedIndex + 2), (byte)(DiagnosticDataListBox.SelectedIndex + 3) };
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
                        if (WordRequestFilter[RAMTableComboBox.SelectedIndex].Contains((byte)DiagnosticDataListBox.SelectedIndices[i]))
                        {
                            WordRequestCount++;
                        }
                        else if (DWordRequestFilter[RAMTableComboBox.SelectedIndex].Contains((byte)DiagnosticDataListBox.SelectedIndices[i]))
                        {
                            DWordRequestCount++;
                        }
                    }

                    int PayloadLength = 1 + ((count - WordRequestCount - DWordRequestCount) * 3) + (WordRequestCount * 4) + (WordRequestCount * 6);

                    MainForm.Packet.tx.payload = new byte[PayloadLength];
                    MainForm.Packet.tx.payload[0] = count;

                    ushort bp = 1; // buffer pointer

                    for (int i = 0; i < count; i++)
                    {
                        if (WordRequestFilter[RAMTableComboBox.SelectedIndex].Contains((byte)DiagnosticDataListBox.SelectedIndices[i]))
                        {
                            MainForm.Packet.tx.payload[bp] = 3;
                            MainForm.Packet.tx.payload[bp + 1] = (byte)(0xF0 + RAMTableComboBox.SelectedIndex);
                            MainForm.Packet.tx.payload[bp + 2] = (byte)DiagnosticDataListBox.SelectedIndices[i];
                            MainForm.Packet.tx.payload[bp + 3] = (byte)(DiagnosticDataListBox.SelectedIndices[i] + 1);
                            bp += 4;
                        }
                        else if (DWordRequestFilter[RAMTableComboBox.SelectedIndex].Contains((byte)DiagnosticDataListBox.SelectedIndices[i]))
                        {
                            MainForm.Packet.tx.payload[bp] = 5;
                            MainForm.Packet.tx.payload[bp + 1] = (byte)(0xF0 + RAMTableComboBox.SelectedIndex);
                            MainForm.Packet.tx.payload[bp + 2] = (byte)DiagnosticDataListBox.SelectedIndices[i];
                            MainForm.Packet.tx.payload[bp + 3] = (byte)(DiagnosticDataListBox.SelectedIndices[i] + 1);
                            MainForm.Packet.tx.payload[bp + 4] = (byte)(DiagnosticDataListBox.SelectedIndices[i] + 2);
                            MainForm.Packet.tx.payload[bp + 5] = (byte)(DiagnosticDataListBox.SelectedIndices[i] + 3);
                            bp += 6;
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
                        if (WordRequestFilter[RAMTableComboBox.SelectedIndex].Contains((byte)DiagnosticDataListBox.SelectedIndices[i]))
                        {
                            DiagnosticItems.Add(new byte[3] { (byte)(0xF0 + RAMTableComboBox.SelectedIndex), (byte)DiagnosticDataListBox.SelectedIndices[i], (byte)(DiagnosticDataListBox.SelectedIndices[i] + 1) });
                        }
                        else if (DWordRequestFilter[RAMTableComboBox.SelectedIndex].Contains((byte)DiagnosticDataListBox.SelectedIndices[i]))
                        {
                            DiagnosticItems.Add(new byte[5] { (byte)(0xF0 + RAMTableComboBox.SelectedIndex), (byte)DiagnosticDataListBox.SelectedIndices[i], (byte)(DiagnosticDataListBox.SelectedIndices[i] + 1), (byte)(DiagnosticDataListBox.SelectedIndices[i] + 2), (byte)(DiagnosticDataListBox.SelectedIndices[i] + 3) });
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
            FirstDiagnosticItemID = 0;
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
            Invoke((MethodInvoker)delegate
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
                                case (byte)SCI_ID.ActuatorTest:
                                    if (SCIBusResponseBytes.Length < 3) break;

                                    if ((SCIBusResponseBytes.Length == 4) && (SCIBusResponseBytes[2] == 0))
                                    {
                                        ActuatorTestStatusLabel.Text = "Status: mode not available";
                                        break;
                                    }

                                    if ((SCIBusResponseBytes[1] == 0) && (SCIBusResponseBytes[2] == 0))
                                    {
                                        ActuatorTestStatusLabel.Text = "Status: stopped";
                                        break;
                                    }

                                    ActuatorTestStatusLabel.Text = "Status: running";
                                    break;
                                case (byte)SCI_ID.ResetMemory:
                                    if (SCIBusResponseBytes.Length < 3) break;

                                    switch (SCIBusResponseBytes[2])
                                    {
                                        case 0x00:
                                            ResetMemoryStatusLabel.Text = "Status: stop engine";
                                            break;
                                        case 0x01:
                                            ResetMemoryStatusLabel.Text = "Status: mode not available";
                                            break;
                                        case 0x02:
                                            ResetMemoryStatusLabel.Text = "Status: denied (module busy)";
                                            break;
                                        case 0x03:
                                            ResetMemoryStatusLabel.Text = "Status: denied (security level)";
                                            break;
                                        case 0xF0:
                                            ResetMemoryStatusLabel.Text = "Status: ok";
                                            break;
                                        default:
                                            ResetMemoryStatusLabel.Text = "Status: unknown";
                                            break;
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
                                            CSVLine += ",";

                                            if ((item.Length >= 5) && WordRequestFilter[item[0] - 0xF0].Contains(item[1]))
                                            {
                                                CSVLine += ((item[2] << 8) + item[4]).ToString();
                                            }
                                            else if ((item.Length >= 9) && DWordRequestFilter[item[0] - 0xF0].Contains(item[1]))
                                            {
                                                CSVLine += ((uint)(item[2] << 24 | item[4] << 16 | item[6] << 8 | item[8])).ToString();
                                            }
                                            else
                                            {
                                                CSVLine += item[2].ToString();
                                            }
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
            });
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
                    if ((OriginalForm.PCM.Year < 2003) && OriginalForm.PCM.CumminsSelected)
                    {
                        DiagnosticDataListBox.Items.AddRange(new object[] {
                        "F800 | CUMMINS DTC FREEZE FRAMES",
                        "F801 |",
                        "F802 |",
                        "F803 |",
                        "F804 |",
                        "F805 |",
                        "F806 |",
                        "F80708 | Vehicle speed",
                        "F808 |",
                        "F8090A | Engine speed",
                        "F80A |",
                        "F80B | Switch state 1",
                        "F80C |",
                        "F80D0E | Engine load",
                        "F80E |",
                        "F80F10 | APP sensor percent",
                        "F810 |",
                        "F81112 | Boost pressure",
                        "F812 |",
                        "F81314 | Engine coolant temperature",
                        "F814 |",
                        "F81516 | Intake air temperature",
                        "F816 |",
                        "F81718 | Oil pressure",
                        "F818 |",
                        "F819 | Switch state 2",
                        "F81A |",
                        "F81B |",
                        "F81C |",
                        "F81D |",
                        "F81E |",
                        "F81F |",
                        "F820 | Final fuel state",
                        "F82122 | Battery voltage",
                        "F822 |",
                        "F82324 | Calculated fuel",
                        "F824 |",
                        "F82526 | Calculated timing",
                        "F826 |",
                        "F82728 | Regulator valve current",
                        "F828 |",
                        "F8292A | Injector pump fuel temperature",
                        "F82A |",
                        "F82B2C | Injector pump engine speed",
                        "F82C |",
                        "F82D |",
                        "F82E |",
                        "F82F30 | Freeze frame DTC 1",
                        "F830 |",
                        "F831 |",
                        "F832 |",
                        "F833 |",
                        "F834 |",
                        "F835 |",
                        "F836 |",
                        "F83738 | Vehicle speed",
                        "F838 |",
                        "F8393A | Engine speed",
                        "F83A |",
                        "F83B | Switch state 1",
                        "F83C |",
                        "F83D3E | Engine load",
                        "F83E |",
                        "F83F40 | APP sensor percent",
                        "F840 |",
                        "F84142 | Boost pressure",
                        "F842 |",
                        "F84344 | Engine coolant temperature",
                        "F844 |",
                        "F84546 | Intake air temperature",
                        "F846 |",
                        "F84748 | Oil pressure",
                        "F808 |",
                        "F849 | Switch state 2",
                        "F84A |",
                        "F84B |",
                        "F84C |",
                        "F84D |",
                        "F84E |",
                        "F84F |",
                        "F850 | Final fuel state",
                        "F85152 | Battery voltage",
                        "F852 |",
                        "F85354 | Calculated fuel",
                        "F854 |",
                        "F85556 | Calculated timing",
                        "F856 |",
                        "F85758 | Regulator valve current",
                        "F858 |",
                        "F8595A | Injector pump fuel temperature",
                        "F85A |",
                        "F85B5C | Injector pump engine speed",
                        "F85C |",
                        "F85D |",
                        "F85E |",
                        "F85F60 | Freeze frame DTC 2",
                        "F860 |",
                        "F861 |",
                        "F862 |",
                        "F863 |",
                        "F864 |",
                        "F865 |",
                        "F866 |",
                        "F867 |",
                        "F868 |",
                        "F869 |",
                        "F86A |",
                        "F86B |",
                        "F86C |",
                        "F86D |",
                        "F86E |",
                        "F86F |",
                        "F870 |",
                        "F871 |",
                        "F872 |",
                        "F873 |",
                        "F874 |",
                        "F875 |",
                        "F876 |",
                        "F877 |",
                        "F878 |",
                        "F879 |",
                        "F87A |",
                        "F87B |",
                        "F87C |",
                        "F87D |",
                        "F87E |",
                        "F87F |",
                        "F880 |",
                        "F881 |",
                        "F882 |",
                        "F883 |",
                        "F884 |",
                        "F885 |",
                        "F886 |",
                        "F887 |",
                        "F888 |",
                        "F889 |",
                        "F88A |",
                        "F88B |",
                        "F88C |",
                        "F88D |",
                        "F88E |",
                        "F88F |",
                        "F890 |",
                        "F891 |",
                        "F892 |",
                        "F893 |",
                        "F894 |",
                        "F895 |",
                        "F896 |",
                        "F897 |",
                        "F898 |",
                        "F899 |",
                        "F89A |",
                        "F89B |",
                        "F89C |",
                        "F89D |",
                        "F89E |",
                        "F89F |",
                        "F8A0 |",
                        "F8A1 |",
                        "F8A2 |",
                        "F8A3 |",
                        "F8A4 |",
                        "F8A5 |",
                        "F8A6 |",
                        "F8A7 |",
                        "F8A8 |",
                        "F8A9 |",
                        "F8AA |",
                        "F8AB |",
                        "F8AC |",
                        "F8AD |",
                        "F8AE |",
                        "F8AF |",
                        "F8B0 |",
                        "F8B1 |",
                        "F8B2 |",
                        "F8B3 |",
                        "F8B4 |",
                        "F8B5 |",
                        "F8B6 |",
                        "F8B7 |",
                        "F8B8 |",
                        "F8B9 |",
                        "F8BA |",
                        "F8BB |",
                        "F8BC |",
                        "F8BD |",
                        "F8BE |",
                        "F8BF |",
                        "F8C0 |",
                        "F8C1 |",
                        "F8C2 |",
                        "F8C3 |",
                        "F8C4 |",
                        "F8C5 |",
                        "F8C6 |",
                        "F8C7 |",
                        "F8C8 |",
                        "F8C9 |",
                        "F8CA |",
                        "F8CB |",
                        "F8CC |",
                        "F8CD |",
                        "F8CE |",
                        "F8CF |",
                        "F8D0 |",
                        "F8D1 |",
                        "F8D2 |",
                        "F8D3 |",
                        "F8D4 |",
                        "F8D5 |",
                        "F8D6 |",
                        "F8D7 |",
                        "F8D8 |",
                        "F8D9 |",
                        "F8DA |",
                        "F8DB |",
                        "F8DC |",
                        "F8DD |",
                        "F8DE |",
                        "F8DF |",
                        "F8E0 |",
                        "F8E1 |",
                        "F8E2 |",
                        "F8E3 |",
                        "F8E4 |",
                        "F8E5 |",
                        "F8E6 |",
                        "F8E7 |",
                        "F8E8 |",
                        "F8E9 |",
                        "F8EA |",
                        "F8EB |",
                        "F8EC |",
                        "F8ED |",
                        "F8EE |",
                        "F8EF |"});
                    }
                    else if ((OriginalForm.PCM.Year >= 2003) && OriginalForm.PCM.CumminsSelected)
                    {
                        DiagnosticDataListBox.Items.AddRange(new object[] {
                        "F800 | CUMMINS DTC FREEZE FRAMES",
                        "F801 |",
                        "F802 |",
                        "F803 |",
                        "F804 |",
                        "F805 |",
                        "F80607 | Vehicle speed",
                        "F807 |",
                        "F80809 | Engine speed",
                        "F809 |",
                        "F80A | Switch state 1",
                        "F80B |",
                        "F80C0D | Engine load",
                        "F80D |",
                        "F80E0F | APP sensor percent",
                        "F80F |",
                        "F81011 | Boost pressure",
                        "F811 |",
                        "F81213 | Engine coolant temperature",
                        "F813 |",
                        "F81415 | Intake air temperature",
                        "F815 |",
                        "F816 |",
                        "F817 |",
                        "F818 |",
                        "F819 | Switch state 2",
                        "F81A |",
                        "F81B |",
                        "F81C |",
                        "F81D |",
                        "F81E | Final fuel state",
                        "F81F20 | Battery voltage",
                        "F820 |",
                        "F82122 | Calculated fuel",
                        "F822 |",
                        "F82324 | Calculated timing",
                        "F824 |",
                        "F82526 | Regulator valve current",
                        "F826 |",
                        "F827 | Defect status",
                        "F828 | Fuel pressure status",
                        "F8292A | Fuel pressure volts",
                        "F82A |",
                        "F82B |",
                        "F82C |",
                        "F82D2E | Fuel level percent",
                        "F82E |",
                        "F82F |",
                        "F830 |",
                        "F83132 | Freeze frame DTC 1",
                        "F832 |",
                        "F833 |",
                        "F834 |",
                        "F835 |",
                        "F836 |",
                        "F837 |",
                        "F83839 | Vehicle speed",
                        "F839 |",
                        "F83A3B | Engine speed",
                        "F83B |",
                        "F83C | Switch state 1",
                        "F83D |",
                        "F83E3F | Engine load",
                        "F83F |",
                        "F84041 | APP sensor percent",
                        "F841 |",
                        "F84243 | Boost pressure",
                        "F843 |",
                        "F84445 | Engine coolant temperature",
                        "F845 |",
                        "F84647 | Intake air temperature",
                        "F847 |",
                        "F848 |",
                        "F849 | Switch state 2",
                        "F84A |",
                        "F84B |",
                        "F84C |",
                        "F84D |",
                        "F84E |",
                        "F84F |",
                        "F850 | Final fuel state",
                        "F85152 | Battery voltage",
                        "F852 |",
                        "F85354 | Calculated fuel",
                        "F854 |",
                        "F85556 | Calculated timing",
                        "F856 |",
                        "F85758 | Regulator valve current",
                        "F858 |",
                        "F8595A | Injector pump fuel temperature",
                        "F85A |",
                        "F85B5C | Injector pump engine speed",
                        "F85C |",
                        "F85D |",
                        "F85E |",
                        "F85F60 | Freeze frame DTC 2",
                        "F860 |",
                        "F861 |",
                        "F862 |",
                        "F863 |",
                        "F864 |",
                        "F865 |",
                        "F866 |",
                        "F867 |",
                        "F868 |",
                        "F869 |",
                        "F86A |",
                        "F86B |",
                        "F86C |",
                        "F86D |",
                        "F86E |",
                        "F86F |",
                        "F870 |",
                        "F871 |",
                        "F872 |",
                        "F873 |",
                        "F874 |",
                        "F875 |",
                        "F876 |",
                        "F877 |",
                        "F878 |",
                        "F879 |",
                        "F87A |",
                        "F87B |",
                        "F87C |",
                        "F87D |",
                        "F87E |",
                        "F87F |",
                        "F880 |",
                        "F881 |",
                        "F882 |",
                        "F883 |",
                        "F884 |",
                        "F885 |",
                        "F886 |",
                        "F887 |",
                        "F888 |",
                        "F889 |",
                        "F88A |",
                        "F88B |",
                        "F88C |",
                        "F88D |",
                        "F88E |",
                        "F88F |",
                        "F890 |",
                        "F891 |",
                        "F892 |",
                        "F893 |",
                        "F894 |",
                        "F895 |",
                        "F896 |",
                        "F897 |",
                        "F898 |",
                        "F899 |",
                        "F89A |",
                        "F89B |",
                        "F89C |",
                        "F89D |",
                        "F89E |",
                        "F89F |",
                        "F8A0 |",
                        "F8A1 |",
                        "F8A2 |",
                        "F8A3 |",
                        "F8A4 |",
                        "F8A5 |",
                        "F8A6 |",
                        "F8A7 |",
                        "F8A8 |",
                        "F8A9 |",
                        "F8AA |",
                        "F8AB |",
                        "F8AC |",
                        "F8AD |",
                        "F8AE |",
                        "F8AF |",
                        "F8B0 |",
                        "F8B1 |",
                        "F8B2 |",
                        "F8B3 |",
                        "F8B4 |",
                        "F8B5 |",
                        "F8B6 |",
                        "F8B7 |",
                        "F8B8 |",
                        "F8B9 |",
                        "F8BA |",
                        "F8BB |",
                        "F8BC |",
                        "F8BD |",
                        "F8BE |",
                        "F8BF |",
                        "F8C0 |",
                        "F8C1 |",
                        "F8C2 |",
                        "F8C3 |",
                        "F8C4 |",
                        "F8C5 |",
                        "F8C6 |",
                        "F8C7 |",
                        "F8C8 |",
                        "F8C9 |",
                        "F8CA |",
                        "F8CB |",
                        "F8CC |",
                        "F8CD |",
                        "F8CE |",
                        "F8CF |",
                        "F8D0 |",
                        "F8D1 |",
                        "F8D2 |",
                        "F8D3 |",
                        "F8D4 |",
                        "F8D5 |",
                        "F8D6 |",
                        "F8D7 |",
                        "F8D8 |",
                        "F8D9 |",
                        "F8DA |",
                        "F8DB |",
                        "F8DC |",
                        "F8DD |",
                        "F8DE |",
                        "F8DF |",
                        "F8E0 |",
                        "F8E1 |",
                        "F8E2 |",
                        "F8E3 |",
                        "F8E4 |",
                        "F8E5 |",
                        "F8E6 |",
                        "F8E7 |",
                        "F8E8 |",
                        "F8E9 |",
                        "F8EA |",
                        "F8EB |",
                        "F8EC |",
                        "F8ED |",
                        "F8EE |",
                        "F8EF |"});
                    }
                    else
                    {
                        DiagnosticDataListBox.Items.AddRange(new object[] {
                        "F800 |" });
                    }
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
                    if ((OriginalForm.PCM.Year < 2003) && OriginalForm.PCM.CumminsSelected)
                    {
                        DiagnosticDataListBox.Items.AddRange(new object[] {
                        "FB00 | CUMMINS SENSORS",
                        "FB0102 | Engine speed",
                        "FB02 |",
                        "FB0304 | Transmission temperature",
                        "FB04 |",
                        "FB0506 | Vehicle speed",
                        "FB06 | ",
                        "FB0708 | APP sensor percent",
                        "FB08 |",
                        "FB09 |",
                        "FB0A |",
                        "FB0B0C | Trans temp sensor volts",
                        "FB0C |",
                        "FB0D |",
                        "FB0E |",
                        "FB0F10 | Engine coolant temperature",
                        "FB10 |",
                        "FB1112 | Engine clnt tmp sensor volts",
                        "FB12 |",
                        "FB1314 | Boost pressure",
                        "FB14 |",
                        "FB1516 | Intake air temperature",
                        "FB16 |",
                        "FB1718 | Intake air temp sensor volts",
                        "FB18 |",
                        "FB191A | Battery voltage",
                        "FB1A |",
                        "FB1B1C | Injector pump battery voltage",
                        "FB1C |",
                        "FB1D |",
                        "FB1E |",
                        "FB1F20 | Injector pump fuel temperature",
                        "FB20 |",
                        "FB21 |",
                        "FB22 |",
                        "FB23 |",
                        "FB24 |",
                        "FB25 |",
                        "FB26 |",
                        "FB27 |",
                        "FB28 |",
                        "FB29 |",
                        "FB2A |",
                        "FB2B |",
                        "FB2C |",
                        "FB2D |",
                        "FB2E |",
                        "FB2F |",
                        "FB30 |",
                        "FB31 |",
                        "FB32 |",
                        "FB33 |",
                        "FB34 |",
                        "FB35 |",
                        "FB36 |",
                        "FB37 |",
                        "FB38 |",
                        "FB39 |",
                        "FB3A |",
                        "FB3B |",
                        "FB3C |",
                        "FB3D |",
                        "FB3E |",
                        "FB3F |",
                        "FB40 |",
                        "FB41 |",
                        "FB42 |",
                        "FB43 |",
                        "FB44 |",
                        "FB45 |",
                        "FB46 |",
                        "FB47 | Switch status",
                        "FB48 |",
                        "FB49 |",
                        "FB4A | Final fuel state",
                        "FB4B |",
                        "FB4C |",
                        "FB4D |",
                        "FB4E |",
                        "FB4F |",
                        "FB50 |",
                        "FB5152 | Boost volts",
                        "FB52 |",
                        "FB53 |",
                        "FB54 |",
                        "FB5556 | Water in fuel volts",
                        "FB56 |",
                        "FB5758 | Engine load",
                        "FB58 |",
                        "FB59 |",
                        "FB5A |",
                        "FB5B |",
                        "FB5C |",
                        "FB5D |",
                        "FB5E |",
                        "FB5F |",
                        "FB60 |",
                        "FB61 |",
                        "FB62 |",
                        "FB63 |",
                        "FB64 |",
                        "FB65 |",
                        "FB66 |",
                        "FB67 |",
                        "FB68 |",
                        "FB69 |",
                        "FB6A |",
                        "FB6B |",
                        "FB6C |",
                        "FB6D |",
                        "FB6E |",
                        "FB6F |",
                        "FB70 |",
                        "FB71 |",
                        "FB72 |",
                        "FB73 |",
                        "FB74 |",
                        "FB75 |",
                        "FB76 |",
                        "FB77 |",
                        "FB78 |",
                        "FB79 |",
                        "FB7A |",
                        "FB7B |",
                        "FB7C |",
                        "FB7D |",
                        "FB7E |",
                        "FB7F |",
                        "FB80 |",
                        "FB81 |",
                        "FB82 |",
                        "FB83 |",
                        "FB84 |",
                        "FB85 |",
                        "FB86 |",
                        "FB87 |",
                        "FB88 |",
                        "FB89 |",
                        "FB8A |",
                        "FB8B |",
                        "FB8C |",
                        "FB8D |",
                        "FB8E |",
                        "FB8F |",
                        "FB90 |",
                        "FB91 |",
                        "FB92 |",
                        "FB93 |",
                        "FB94 |",
                        "FB95 |",
                        "FB96 |",
                        "FB97 |",
                        "FB98 |",
                        "FB99 |",
                        "FB9A |",
                        "FB9B |",
                        "FB9C |",
                        "FB9D |",
                        "FB9E |",
                        "FB9F |",
                        "FBA0 |",
                        "FBA1 |",
                        "FBA2 |",
                        "FBA3 |",
                        "FBA4 |",
                        "FBA5 |",
                        "FBA6 |",
                        "FBA7 |",
                        "FBA8 |",
                        "FBA9 |",
                        "FBAA |",
                        "FBAB |",
                        "FBAC |",
                        "FBAD |",
                        "FBAE |",
                        "FBAF |",
                        "FBB0 |",
                        "FBB1 |",
                        "FBB2 |",
                        "FBB3 |",
                        "FBB4 |",
                        "FBB5 |",
                        "FBB6 |",
                        "FBB7 |",
                        "FBB8 |",
                        "FBB9 | Battery temperature",
                        "FBBA |",
                        "FBBB |",
                        "FBBC |",
                        "FBBD |",
                        "FBBE |",
                        "FBBF |",
                        "FBC0 |",
                        "FBC1 |",
                        "FBC2 |",
                        "FBC3 |",
                        "FBC4 |",
                        "FBC5 |",
                        "FBC6 |",
                        "FBC7 |",
                        "FBC8 |",
                        "FBC9 |",
                        "FBCA |",
                        "FBCBCC | Key-on counter",
                        "FBCC |",
                        "FBCDCE | Engine speed CKD sensor",
                        "FBCE |",
                        "FBCFD0 | Engine speed CMP sensor",
                        "FBD0 |",
                        "FBD1 |",
                        "FBD2 |",
                        "FBD3 |",
                        "FBD4 |",
                        "FBD5 |",
                        "FBD6 |",
                        "FBD7D8 | APP sensor volts",
                        "FBD8 |",
                        "FBD9 |",
                        "FBDA |",
                        "FBDB |",
                        "FBDC | Cruise control denied reason",
                        "FBDD |",
                        "FBDE | Cruise control last cutout reason",
                        "FBDF |",
                        "FBE0 |",
                        "FBE1 |",
                        "FBE2 | Cruise indicator lamp",
                        "FBE3 |",
                        "FBE4 | Cruise button pressed",
                        "FBE5E6 | Cruise set speed",
                        "FBE6 |",
                        "FBE7E8 | Cruise switch volts",
                        "FBE8 |",
                        "FBE9 |",
                        "FBEA |",
                        "FBEB |",
                        "FBEC |",
                        "FBED |",
                        "FBEE |",
                        "FBEF |"});
                    }
                    else if ((OriginalForm.PCM.Year >= 2003) && OriginalForm.PCM.CumminsSelected)
                    {
                        DiagnosticDataListBox.Items.AddRange(new object[] {
                        "FB00 | CUMMINS SENSORS",
                        "FB0102 | Engine speed",
                        "FB02 |",
                        "FB0304 | Transmission temperature",
                        "FB04 |",
                        "FB0506 | Vehicle speed",
                        "FB06 |",
                        "FB0708 | APP sensor percent",
                        "FB08 |",
                        "FB09 |",
                        "FB0A |",
                        "FB0B0C | Trans temp sensor volts",
                        "FB0C |",
                        "FB0D |",
                        "FB0E |",
                        "FB0F10 | Engine coolant temperature",
                        "FB10 |",
                        "FB1112 | Engine clnt tmp sensor volts",
                        "FB12 |",
                        "FB1314 | Boost pressure",
                        "FB14 |",
                        "FB1516 | Intake air temperature",
                        "FB16 |",
                        "FB1718 | Intake air temp sensor volts",
                        "FB18 |",
                        "FB191A | Battery voltage",
                        "FB1A |",
                        "FB1B1C | Output shaft speed",
                        "FB1C |",
                        "FB1D1E | Water in fuel counter",
                        "FB1E |",
                        "FB1F20 | Transmission PWM duty cycle",
                        "FB20 |",
                        "FB21 |",
                        "FB22 |",
                        "FB23 | Present drive gear",
                        "FB24 |",
                        "FB25 |",
                        "FB26 |",
                        "FB27 |",
                        "FB28 |",
                        "FB29 |",
                        "FB2A |",
                        "FB2B |",
                        "FB2C |",
                        "FB2D |",
                        "FB2E |",
                        "FB2F30 | Target governor pressure",
                        "FB30 |",
                        "FB3132 | PPS 1 sensor percent",
                        "FB32 |",
                        "FB3334 | PPS 1 sensor volts",
                        "FB34 |",
                        "FB35 |",
                        "FB36 |",
                        "FB37 |",
                        "FB38 |",
                        "FB39 |",
                        "FB3A |",
                        "FB3B |",
                        "FB3C |",
                        "FB3D |",
                        "FB3E |",
                        "FB3F40 | PPS 2 sensor percent",
                        "FB40 |",
                        "FB4142 | PPS 2 sensor volts",
                        "FB42 |",
                        "FB43 |",
                        "FB44 |",
                        "FB45 | Idle switch status",
                        "FB46 | Brake switch pressed",
                        "FB47 | Switch status",
                        "FB48 | Desired TC clutch status",
                        "FB49 |",
                        "FB4A | Final fuel state",
                        "FB4B |",
                        "FB4C |",
                        "FB4D |",
                        "FB4E |",
                        "FB4F50 | Wastegate duty cycle",
                        "FB50 |",
                        "FB5152 | Boots volts",
                        "FB52 |",
                        "FB53 |",
                        "FB54 |",
                        "FB5556 | Water in fuel volts",
                        "FB56 |",
                        "FB5758 | Engine load",
                        "FB58 |",
                        "FB59 |",
                        "FB5A |",
                        "FB5B |",
                        "FB5C |",
                        "FB5D |",
                        "FB5E |",
                        "FB5F |",
                        "FB60 |",
                        "FB61 |",
                        "FB62 |",
                        "FB63 |",
                        "FB64 |",
                        "FB65 |",
                        "FB66 |",
                        "FB67 |",
                        "FB68 |",
                        "FB69 |",
                        "FB6A |",
                        "FB6B |",
                        "FB6C |",
                        "FB6D |",
                        "FB6E |",
                        "FB6F |",
                        "FB70 |",
                        "FB71 |",
                        "FB72 |",
                        "FB73 |",
                        "FB74 |",
                        "FB75 |",
                        "FB76 |",
                        "FB77 |",
                        "FB78 |",
                        "FB79 |",
                        "FB7A |",
                        "FB7B |",
                        "FB7C |",
                        "FB7D |",
                        "FB7E |",
                        "FB7F |",
                        "FB80 |",
                        "FB81 |",
                        "FB82 |",
                        "FB83 |",
                        "FB84 |",
                        "FB85 |",
                        "FB86 |",
                        "FB87 |",
                        "FB88 |",
                        "FB89 |",
                        "FB8A |",
                        "FB8B |",
                        "FB8C |",
                        "FB8D |",
                        "FB8E |",
                        "FB8F |",
                        "FB90 |",
                        "FB91 |",
                        "FB92 |",
                        "FB93 |",
                        "FB94 |",
                        "FB95 |",
                        "FB96 |",
                        "FB97 |",
                        "FB98 |",
                        "FB99 |",
                        "FB9A |",
                        "FB9B |",
                        "FB9C |",
                        "FB9D |",
                        "FB9E |",
                        "FB9F |",
                        "FBA0 |",
                        "FBA1 |",
                        "FBA2 |",
                        "FBA3 |",
                        "FBA4 |",
                        "FBA5 |",
                        "FBA6 |",
                        "FBA7 |",
                        "FBA8 |",
                        "FBA9 |",
                        "FBAA |",
                        "FBAB |",
                        "FBAC |",
                        "FBAD |",
                        "FBAE |",
                        "FBAF |",
                        "FBB0 |",
                        "FBB1 |",
                        "FBB2 |",
                        "FBB3 |",
                        "FBB4 |",
                        "FBB5 |",
                        "FBB6 |",
                        "FBB7 |",
                        "FBB8 |",
                        "FBB9BA | Battery temperature",
                        "FBBA |",
                        "FBBB |",
                        "FBBC |",
                        "FBBD |",
                        "FBBE |",
                        "FBBF |",
                        "FBC0 |",
                        "FBC1 |",
                        "FBC2 |",
                        "FBC3 |",
                        "FBC4 |",
                        "FBC5 |",
                        "FBC6 |",
                        "FBC7 |",
                        "FBC8 |",
                        "FBC9 |",
                        "FBCA |",
                        "FBCBCC | Key-on counter",
                        "FBCC |",
                        "FBCDCE | Engine speed CKD sensor",
                        "FBCE |",
                        "FBCFD0 | Engine speed CMP sensor",
                        "FBD0 |",
                        "FBD1 |",
                        "FBD2 | Relay status",
                        "FBD3 |",
                        "FBD4 |",
                        "FBD5 |",
                        "FBD6 |",
                        "FBD7D8 | APP sensor volts",
                        "FBD8 |",
                        "FBD9 |",
                        "FBDA |",
                        "FBDB |",
                        "FBDC | Cruise control denied reason",
                        "FBDD |",
                        "FBDE | Cruise control last cutout reason",
                        "FBDF |",
                        "FBE0 |",
                        "FBE1 |",
                        "FBE2 | Cruise indicator lamp",
                        "FBE3 |",
                        "FBE4 | Cruise button pressed",
                        "FBE5E6 | Cruise set speed",
                        "FBE6 |",
                        "FBE7E8 | Cruise switch volts",
                        "FBE8 |",
                        "FBE9 |",
                        "FBEA |",
                        "FBEBEC | Injectors disabled vehicle speed",
                        "FBEC |",
                        "FBED |",
                        "FBEE |",
                        "FBEF |"});
                    }
                    else
                    {
                        DiagnosticDataListBox.Items.AddRange(new object[] {
                        "FB00 |"});
                    }
                    break;
                case 0xFC:
                    DiagnosticDataListBox.Items.AddRange(new object[] {
                        "FC00 | CUMMINS STATISTICS 1",
                        "FC0104 | Total fuel used",
                        "FC02 |",
                        "FC03 |",
                        "FC04 |",
                        "FC0508 | Trip fuel used",
                        "FC06 |",
                        "FC07 |",
                        "FC08 |",
                        "FC090C | Total time",
                        "FC0A |",
                        "FC0B |",
                        "FC0C |",
                        "FC0D10 | Trip time",
                        "FC0E |",
                        "FC0F |",
                        "FC10 |",
                        "FC1114 | Total idle fuel",
                        "FC12 |",
                        "FC13 |",
                        "FC14 |",
                        "FC1518 | Trip idle fuel",
                        "FC16 |",
                        "FC17 |",
                        "FC18 |",
                        "FC191C | Total idle time",
                        "FC1A |",
                        "FC1B |",
                        "FC1C |",
                        "FC1D20 | Trip idle time",
                        "FC1E |",
                        "FC1F |",
                        "FC20 |",
                        "FC2124 | Total distance",
                        "FC22 |",
                        "FC23 |",
                        "FC24 |",
                        "FC2528 | Trip distance",
                        "FC26 |",
                        "FC27 |",
                        "FC28 |",
                        "FC292A | Trip average fuel",
                        "FC2A |",
                        "FC2B2E | ECM run time",
                        "FC2C |",
                        "FC2D |",
                        "FC2E |",
                        "FC2F32 | Engine run time",
                        "FC30 |",
                        "FC31 |",
                        "FC32 |",
                        "FC33 |",
                        "FC34 |",
                        "FC35 |",
                        "FC36 |",
                        "FC37 |",
                        "FC38 |",
                        "FC39 |",
                        "FC3A |",
                        "FC3B |",
                        "FC3C |",
                        "FC3D |",
                        "FC3E |",
                        "FC3F |",
                        "FC40 |",
                        "FC41 |",
                        "FC42 |",
                        "FC43 |",
                        "FC44 |",
                        "FC45 |",
                        "FC46 |",
                        "FC47 |",
                        "FC48 |",
                        "FC49 |",
                        "FC4A |",
                        "FC4B |",
                        "FC4C |",
                        "FC4D |",
                        "FC4E |",
                        "FC4F |",
                        "FC50 |",
                        "FC51 |",
                        "FC52 |",
                        "FC53 |",
                        "FC54 |",
                        "FC55 |",
                        "FC56 |",
                        "FC57 |",
                        "FC58 |",
                        "FC59 |",
                        "FC5A |",
                        "FC5B |",
                        "FC5C |",
                        "FC5D |",
                        "FC5E |",
                        "FC5F |",
                        "FC60 |",
                        "FC61 |",
                        "FC62 |",
                        "FC63 |",
                        "FC64 |",
                        "FC65 |",
                        "FC66 |",
                        "FC67 |",
                        "FC68 |",
                        "FC69 |",
                        "FC6A |",
                        "FC6B |",
                        "FC6C |",
                        "FC6D |",
                        "FC6E |",
                        "FC6F |",
                        "FC70 |",
                        "FC71 |",
                        "FC72 |",
                        "FC73 |",
                        "FC74 |",
                        "FC75 |",
                        "FC76 |",
                        "FC77 |",
                        "FC78 |",
                        "FC79 |",
                        "FC7A |",
                        "FC7B |",
                        "FC7C |",
                        "FC7D |",
                        "FC7E |",
                        "FC7F |",
                        "FC80 |",
                        "FC81 |",
                        "FC82 |",
                        "FC83 |",
                        "FC84 |",
                        "FC85 |",
                        "FC86 |",
                        "FC87 |",
                        "FC88 |",
                        "FC89 |",
                        "FC8A |",
                        "FC8B |",
                        "FC8C |",
                        "FC8D |",
                        "FC8E |",
                        "FC8F |",
                        "FC90 |",
                        "FC91 |",
                        "FC92 |",
                        "FC93 |",
                        "FC94 |",
                        "FC95 |",
                        "FC96 |",
                        "FC97 |",
                        "FC98 |",
                        "FC99 |",
                        "FC9A |",
                        "FC9B |",
                        "FC9C |",
                        "FC9D |",
                        "FC9E |",
                        "FC9F |",
                        "FCA0 |",
                        "FCA1 |",
                        "FCA2 |",
                        "FCA3 |",
                        "FCA4 |",
                        "FCA5 |",
                        "FCA6 |",
                        "FCA7 |",
                        "FCA8 |",
                        "FCA9 |",
                        "FCAA |",
                        "FCAB |",
                        "FCAC |",
                        "FCAD |",
                        "FCAE |",
                        "FCAF |",
                        "FCB0 |",
                        "FCB1 |",
                        "FCB2 |",
                        "FCB3 |",
                        "FCB4 |",
                        "FCB5 |",
                        "FCB6 |",
                        "FCB7 |",
                        "FCB8 |",
                        "FCB9 |",
                        "FCBA |",
                        "FCBB |",
                        "FCBC |",
                        "FCBD |",
                        "FCBE |",
                        "FCBF |",
                        "FCC0 |",
                        "FCC1 |",
                        "FCC2 |",
                        "FCC3 |",
                        "FCC4 |",
                        "FCC5 |",
                        "FCC6 |",
                        "FCC7 |",
                        "FCC8 |",
                        "FCC9 |",
                        "FCCA |",
                        "FCCB |",
                        "FCCC |",
                        "FCCD |",
                        "FCCE |",
                        "FCCF |",
                        "FCD0 |",
                        "FCD1 |",
                        "FCD2 |",
                        "FCD3 |",
                        "FCD4 |",
                        "FCD5 |",
                        "FCD6 |",
                        "FCD7 |",
                        "FCD8 |",
                        "FCD9 |",
                        "FCDA |",
                        "FCDB |",
                        "FCDC |",
                        "FCDD |",
                        "FCDE |",
                        "FCDF |",
                        "FCE0 |",
                        "FCE1 |",
                        "FCE2 |",
                        "FCE3 |",
                        "FCE4 |",
                        "FCE5 |",
                        "FCE6 |",
                        "FCE7 |",
                        "FCE8 |",
                        "FCE9 |",
                        "FCEA |",
                        "FCEB |",
                        "FCEC |",
                        "FCED |",
                        "FCEE |",
                        "FCEF |"});
                    break;
                case 0xFD:
                    DiagnosticDataListBox.Items.AddRange(new object[] {
                        "FD00 | CUMMINS STATISTICS 2",
                        "FD01 |",
                        "FD02 |",
                        "FD03 |",
                        "FD04 |",
                        "FD05 |",
                        "FD06 |",
                        "FD07 |",
                        "FD08 |",
                        "FD09 |",
                        "FD0A |",
                        "FD0B |",
                        "FD0C |",
                        "FD0D |",
                        "FD0E |",
                        "FD0F |",
                        "FD10 |",
                        "FD11 |",
                        "FD12 |",
                        "FD13 |",
                        "FD14 |",
                        "FD15 |",
                        "FD16 |",
                        "FD17 |",
                        "FD18 |",
                        "FD19 |",
                        "FD1A |",
                        "FD1B |",
                        "FD1C |",
                        "FD1D |",
                        "FD1E |",
                        "FD1F |",
                        "FD20 |",
                        "FD2122 | Fuel pressure regulator output",
                        "FD22 |",
                        "FD2324 | Fuel pressure",
                        "FD24 |",
                        "FD25 |",
                        "FD26 |",
                        "FD278 | Fuel pressure setpoint",
                        "FD28 |",
                        "FD292A | Fuel pressure sensor volts",
                        "FD2A |",
                        "FD2B |",
                        "FD2C |",
                        "FD2D |",
                        "FD2E |",
                        "FD2F |",
                        "FD30 |",
                        "FD31 |",
                        "FD32 |",
                        "FD33 |",
                        "FD34 |",
                        "FD35 |",
                        "FD36 |",
                        "FD37 |",
                        "FD38 |",
                        "FD39 |",
                        "FD3A |",
                        "FD3B |",
                        "FD3C |",
                        "FD3D |",
                        "FD3E |",
                        "FD3F |",
                        "FD40 |",
                        "FD41 |",
                        "FD42 |",
                        "FD43 |",
                        "FD44 |",
                        "FD45 |",
                        "FD46 |",
                        "FD47 |",
                        "FD48 |",
                        "FD49 |",
                        "FD4A |",
                        "FD4B |",
                        "FD4C |",
                        "FD4D |",
                        "FD4E |",
                        "FD4F |",
                        "FD50 |",
                        "FD51 |",
                        "FD52 |",
                        "FD53 |",
                        "FD54 |",
                        "FD55 |",
                        "FD56 |",
                        "FD57 |",
                        "FD58 |",
                        "FD59 |",
                        "FD5A |",
                        "FD5B |",
                        "FD5C |",
                        "FD5D |",
                        "FD5E |",
                        "FD5F |",
                        "FD60 |",
                        "FD61 |",
                        "FD62 |",
                        "FD63 |",
                        "FD64 |",
                        "FD65 |",
                        "FD66 |",
                        "FD67 |",
                        "FD68 |",
                        "FD69 |",
                        "FD6A |",
                        "FD6B |",
                        "FD6C |",
                        "FD6D |",
                        "FD6E |",
                        "FD6F |",
                        "FD70 |",
                        "FD71 |",
                        "FD72 |",
                        "FD73 |",
                        "FD74 |",
                        "FD75 |",
                        "FD76 |",
                        "FD77 |",
                        "FD78 |",
                        "FD79 |",
                        "FD7A |",
                        "FD7B |",
                        "FD7C7F | CVN",
                        "FD7D |",
                        "FD7E |",
                        "FD7F |",
                        "FD8081 | Radiator fan speed",
                        "FD81 |",
                        "FD8283 | Desired radiator fan PWM",
                        "FD83 |",
                        "FD8485 | % of time @  0-10% LOAD",
                        "FD85 |",
                        "FD8687 | % of time @ 11-20% LOAD",
                        "FD87 |",
                        "FD8889 | % of time @ 21-30% LOAD",
                        "FD89 |",
                        "FD8A8B | % of time @ 31-40% LOAD",
                        "FD8B |",
                        "FD8C8D | % of time @ 41-50% LOAD",
                        "FD8D |",
                        "FD8E8F | % of time @ 51-60% LOAD",
                        "FD8F |",
                        "FD9091 | % of time @ 61-70% LOAD",
                        "FD91 |",
                        "FD9293 | % of time @ 71-80% LOAD",
                        "FD93 |",
                        "FD9495 | % of time @ 81-90% LOAD",
                        "FD95 |",
                        "FD9697 | % of time @ 91-100% LOAD",
                        "FD97 |",
                        "FD98 |",
                        "FD99 |",
                        "FD9A |",
                        "FD9B |",
                        "FD9C |",
                        "FD9D |",
                        "FD9E |",
                        "FD9F |",
                        "FDA0A1 | Barometric pressure",
                        "FDA1 |",
                        "FDA2 |",
                        "FDA3 |",
                        "FDA4A5 | Ambient air temperature",
                        "FDA5 |",
                        "FDA6A7 | Ambient air temp sensor volts",
                        "FDA7 |",
                        "FDA8 |",
                        "FDA9 |",
                        "FDAA |",
                        "FDAB |",
                        "FDAC |",
                        "FDAD |",
                        "FDAE |",
                        "FDAF |",
                        "FDB0 |",
                        "FDB1 |",
                        "FDB2 |",
                        "FDB3 |",
                        "FDB4 |",
                        "FDB5 |",
                        "FDB6 |",
                        "FDB7 |",
                        "FDB8 |",
                        "FDB9 |",
                        "FDBA |",
                        "FDBB |",
                        "FDBC |",
                        "FDBD |",
                        "FDBE |",
                        "FDBF |",
                        "FDC0 |",
                        "FDC1 |",
                        "FDC2 |",
                        "FDC3 |",
                        "FDC4 |",
                        "FDC5 |",
                        "FDC6 |",
                        "FDC7 |",
                        "FDC8 |",
                        "FDC9 |",
                        "FDCA |",
                        "FDCB |",
                        "FDCC |",
                        "FDCD |",
                        "FDCE |",
                        "FDCF |",
                        "FDD0 |",
                        "FDD1 |",
                        "FDD2 |",
                        "FDD3 |",
                        "FDD4 |",
                        "FDD5 |",
                        "FDD6 |",
                        "FDD7 |",
                        "FDD8 |",
                        "FDD9 |",
                        "FDDA |",
                        "FDDB |",
                        "FDDC |",
                        "FDDD |",
                        "FDDE |",
                        "FDDF |",
                        "FDE0 |",
                        "FDE1 | Cylinder 1 contribution",
                        "FDE2 | Cylinder 5 contribution",
                        "FDE3 | Cylinder 3 contribution",
                        "FDE4 | Cylinder 6 contribution",
                        "FDE5 | Cylinder 2 contribution",
                        "FDE6 | Cylinder 4 contribution",
                        "FDE7 | Cylinder 1-3 contribution",
                        "FDE8 | Cylinder 4-6 contribution",
                        "FDE9 |",
                        "FDEA | Engine speed",
                        "FDEB | Cylinder test status",
                        "FDEC | FPO test status",
                        "FDED |",
                        "FDEE |",
                        "FDEF |"});
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
            Invoke((MethodInvoker)delegate
            {
                CHTComboBox.SelectedIndex = OriginalForm.PCM.ControllerHardwareType;
            });
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
