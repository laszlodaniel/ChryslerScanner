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

        private enum SCI_ID
        {
            DiagnosticData = 0x14,
            GetSecuritySeedLegacy = 0x2B,
            GetSecuritySeed = 0x35,
            SendSecurityKey = 0x2C
        }

        public EngineToolsForm(MainForm IncomingForm)
        {
            OriginalForm = IncomingForm;
            InitializeComponent();
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            MainForm.Packet.PacketReceived += PacketReceivedHandler; // subscribe to the OnPacketReceived event

            ActuatorTestComboBox.SelectedIndex = 1;
            ResetMemoryComboBox.SelectedIndex = 1;
            SecurityLevelComboBox.SelectedIndex = 0;

            ActiveControl = ReadFaultCodesButton;
        }

        private void ReadFaultCodesButton_Click(object sender, EventArgs e)
        {
            MainForm.Packet.tx.bus = (byte)Packet.Bus.pcm;
            MainForm.Packet.tx.command = (byte)Packet.Command.msgTx;
            MainForm.Packet.tx.mode = (byte)Packet.MsgTxMode.single;
            MainForm.Packet.tx.payload = new byte[1] { 0x10 };
            MainForm.Packet.GeneratePacket();
            OriginalForm.TransmitUSBPacket("[<-TX] PCM fault code list request:");
        }

        private void EraseFaultCodesButton_Click(object sender, EventArgs e)
        {
            MainForm.Packet.tx.bus = (byte)Packet.Bus.pcm;
            MainForm.Packet.tx.command = (byte)Packet.Command.msgTx;
            MainForm.Packet.tx.mode = (byte)Packet.MsgTxMode.single;
            MainForm.Packet.tx.payload = new byte[1] { 0x17 };
            MainForm.Packet.GeneratePacket();
            OriginalForm.TransmitUSBPacket("[<-TX] Erase PCM fault code(s) request:");
        }

        private void Baud976Button_Click(object sender, EventArgs e)
        {
            // TODO
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
            MainForm.Packet.tx.bus = (byte)Packet.Bus.pcm;
            MainForm.Packet.tx.command = (byte)Packet.Command.msgTx;
            MainForm.Packet.tx.mode = (byte)Packet.MsgTxMode.single;
            MainForm.Packet.tx.payload = new byte[1] { 0x12 };
            MainForm.Packet.GeneratePacket();
            OriginalForm.TransmitUSBPacket("[<-TX] Select SCI-bus high-speed mode:");
        }

        private void Baud125000Button_Click(object sender, EventArgs e)
        {
            // TODO
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
                    OriginalForm.TransmitUSBPacket("[<-TX] Set SCI-bus (PCM) message repeat behavior:");

                    MainForm.Packet.tx.mode = (byte)Packet.MsgTxMode.repeatedSingle;
                }
                else
                {
                    MainForm.Packet.tx.mode = (byte)Packet.MsgTxMode.single;
                }

                MainForm.Packet.tx.bus = (byte)Packet.Bus.pcm;
                MainForm.Packet.tx.command = (byte)Packet.Command.msgTx;
                MainForm.Packet.tx.payload = new byte[2] { 0x14, (byte)DiagnosticDataListBox.SelectedIndex };
                MainForm.Packet.GeneratePacket();
                OriginalForm.TransmitUSBPacket("[<-TX] Request diagnostic data:");
            }
            else if (count > 1)
            {
                if (DiagnosticDataRepeatIntervalCheckBox.Checked)
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
                    OriginalForm.TransmitUSBPacket("[<-TX] Set SCI-bus (PCM) message repeat behavior:");

                    MainForm.Packet.tx.mode = (byte)Packet.MsgTxMode.repeatedList;
                }
                else
                {
                    MainForm.Packet.tx.mode = (byte)Packet.MsgTxMode.list;
                }

                MainForm.Packet.tx.payload = new byte[(3 * count) + 1];
                MainForm.Packet.tx.payload[0] = count;

                for (int i = 0; i < count; i++)
                {
                    MainForm.Packet.tx.payload[1 + (i * 3)] = 2; // message length
                    MainForm.Packet.tx.payload[1 + (i * 3) + 1] = 0x14; // request ID
                    MainForm.Packet.tx.payload[1 + (i * 3) + 1 + 1] = (byte)DiagnosticDataListBox.SelectedIndices[i]; // request parameter
                }

                MainForm.Packet.tx.bus = (byte)Packet.Bus.pcm;
                MainForm.Packet.tx.command = (byte)Packet.Command.msgTx;
                MainForm.Packet.GeneratePacket();
                OriginalForm.TransmitUSBPacket("[<-TX] Request diagnostic data:");
            }

            if (DiagnosticDataCSVCheckBox.Checked && (count > 0))
            {
                DiagnosticItemCount = count;
                DiagnosticItems.Clear();
                
                for (int i = 0; i < count; i++)
                {
                    DiagnosticItems.Add(new byte[2] { 0x14, (byte)DiagnosticDataListBox.SelectedIndices[i] });
                }

                string Header = "Milliseconds";

                foreach (var item in DiagnosticItems)
                {
                    Header += "," + Util.ByteToHexStringSimple(item);
                }

                FirstDiagnosticItemID = DiagnosticItems[0][1]; // remember first diagnostic data ID
                DiagnosticItems.Clear();

                Header = Header.Replace(" ", ""); // remove whitespaces

                if (Header != CSVHeader)
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

        private void DiagnosticDataRepeatIntervalCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (DiagnosticDataRepeatIntervalCheckBox.Checked)
            {
                DiagnosticDataRepeatIntervalTextBox.Enabled = true;
                MillisecondsLabel01.Enabled = true;
            }
            else
            {
                DiagnosticDataRepeatIntervalTextBox.Enabled = false;
                MillisecondsLabel01.Enabled = false;
            }
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
            OriginalForm.TransmitUSBPacket("[<-TX] Set SCI-bus (PCM) message repeat behavior:");

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
            MainForm.Packet.tx.payload = new byte[2] { 0x19, (byte)((double)SetRPM / 7.85) };
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
                if (LegacySecurityCheckBox.Checked)
                {
                    MainForm.Packet.tx.bus = (byte)Packet.Bus.pcm;
                    MainForm.Packet.tx.command = (byte)Packet.Command.msgTx;
                    MainForm.Packet.tx.mode = (byte)Packet.MsgTxMode.single;
                    MainForm.Packet.tx.payload = new byte[1] { 0x2B };
                    MainForm.Packet.GeneratePacket();
                    OriginalForm.TransmitUSBPacket("[<-TX] Request level 1 security seed:");
                }
                else
                {
                    MainForm.Packet.tx.bus = (byte)Packet.Bus.pcm;
                    MainForm.Packet.tx.command = (byte)Packet.Command.msgTx;
                    MainForm.Packet.tx.mode = (byte)Packet.MsgTxMode.single;
                    MainForm.Packet.tx.payload = new byte[2] { 0x35, 0x01 };
                    MainForm.Packet.GeneratePacket();
                    OriginalForm.TransmitUSBPacket("[<-TX] Request level 1 security seed:");
                }
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

        private void SecurityLevelComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SecurityLevelComboBox.SelectedIndex == 0)
            {
                LegacySecurityCheckBox.Enabled = true;
            }
            else
            {
                LegacySecurityCheckBox.Enabled = false;
            }
        }

        private void PacketReceivedHandler(object sender, EventArgs e)
        {
            switch (MainForm.Packet.rx.bus)
            {
                case (byte)Packet.Bus.pcm:
                case (byte)Packet.Bus.tcm:
                    byte[] SCIBusResponseBytes = MainForm.Packet.rx.payload.Skip(4).ToArray(); // skip 4 timestamp bytes

                    if (SCIBusResponseBytes.Length > 0)
                    {
                        switch (SCIBusResponseBytes[0])
                        {
                            case (byte)SCI_ID.DiagnosticData:
                                if (SCIBusResponseBytes.Length >= 3)
                                {
                                    if (DiagnosticDataCSVCheckBox.Checked && (DiagnosticItemCount > 0))
                                    {
                                        DiagnosticItems.Add(SCIBusResponseBytes);

                                        if ((DiagnosticItems.Count == 1) && (SCIBusResponseBytes[1] != DiagnosticItems[0][1])) // keep columns ordered
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
                                break;
                            case (byte)SCI_ID.GetSecuritySeedLegacy:
                                if (SCIBusResponseBytes.Length >= 4)
                                {
                                    byte checksum = (byte)(SCIBusResponseBytes[0] + SCIBusResponseBytes[1] + SCIBusResponseBytes[2]);

                                    if (SCIBusResponseBytes[3] == checksum)
                                    {
                                        ushort seed = (ushort)((SCIBusResponseBytes[1] << 8) + SCIBusResponseBytes[2]);

                                        if (seed != 0)
                                        {
                                            ushort key = (ushort)((seed << 2) + 0x9018);
                                            byte keyHB = (byte)(key >> 8);
                                            byte keyLB = (byte)(key);
                                            byte keyChecksum = (byte)(0x2C + keyHB + keyLB);
                                            byte[] keyArray = { (byte)SCI_ID.SendSecurityKey, keyHB, keyLB, keyChecksum };

                                            MainForm.Packet.tx.bus = (byte)Packet.Bus.pcm;
                                            MainForm.Packet.tx.command = (byte)Packet.Command.msgTx;
                                            MainForm.Packet.tx.mode = (byte)Packet.MsgTxMode.single;
                                            MainForm.Packet.tx.payload = keyArray;
                                            MainForm.Packet.GeneratePacket();
                                            OriginalForm.TransmitUSBPacket("[<-TX] Send level 1 security key:");
                                        }
                                    }
                                }
                                break;
                            case (byte)SCI_ID.GetSecuritySeed:
                                if (SCIBusResponseBytes.Length >= 5)
                                {
                                    if (SCIBusResponseBytes[1] == 0x01)
                                    {
                                        byte checksum = (byte)(SCIBusResponseBytes[0] + SCIBusResponseBytes[1] + SCIBusResponseBytes[2] + SCIBusResponseBytes[3]);

                                        if (SCIBusResponseBytes[4] == checksum)
                                        {
                                            ushort seed = (ushort)((SCIBusResponseBytes[2] << 8) + SCIBusResponseBytes[3]);

                                            if (seed != 0)
                                            {
                                                ushort key = (ushort)((seed << 2) + 0x9018);
                                                byte keyHB = (byte)(key >> 8);
                                                byte keyLB = (byte)(key);
                                                byte keyChecksum = (byte)(0x2C + keyHB + keyLB);
                                                byte[] keyArray = { (byte)SCI_ID.SendSecurityKey, keyHB, keyLB, keyChecksum };

                                                MainForm.Packet.tx.bus = (byte)Packet.Bus.pcm;
                                                MainForm.Packet.tx.command = (byte)Packet.Command.msgTx;
                                                MainForm.Packet.tx.mode = (byte)Packet.MsgTxMode.single;
                                                MainForm.Packet.tx.payload = keyArray;
                                                MainForm.Packet.GeneratePacket();
                                                OriginalForm.TransmitUSBPacket("[<-TX] Send level 1 security key:");
                                            }
                                        }
                                    }
                                    else if (SCIBusResponseBytes[1] == 0x02)
                                    {
                                        byte checksum = (byte)(SCIBusResponseBytes[0] + SCIBusResponseBytes[1] + SCIBusResponseBytes[2] + SCIBusResponseBytes[3]);

                                        if (SCIBusResponseBytes[4] == checksum)
                                        {
                                            ushort seed = (ushort)((SCIBusResponseBytes[2] << 8) + SCIBusResponseBytes[3]);

                                            if (seed != 0)
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

                                                MainForm.Packet.tx.bus = (byte)Packet.Bus.pcm;
                                                MainForm.Packet.tx.command = (byte)Packet.Command.msgTx;
                                                MainForm.Packet.tx.mode = (byte)Packet.MsgTxMode.single;
                                                MainForm.Packet.tx.payload = keyArray;
                                                MainForm.Packet.GeneratePacket();
                                                OriginalForm.TransmitUSBPacket("[<-TX] Send level 2 security key:");
                                            }
                                        }
                                    }
                                }
                                break;
                        }
                    }
                    break;
            }
        }

        private void EngineToolsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            MainForm.Packet.PacketReceived -= PacketReceivedHandler; // unsubscribe from the OnPacketReceived event
        }
    }
}
