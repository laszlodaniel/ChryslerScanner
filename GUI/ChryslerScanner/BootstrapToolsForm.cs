using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ChryslerScanner
{
    public partial class BootstrapToolsForm : Form
    {
        public MainForm OriginalForm;

        private byte CurrentWorkerFunctionIndex = 0;
        private byte CurrentFlashChipIndex = 0;
        private string SCIBusBootstrapLogFilename;

        private enum SCI_ID
        {
            WorkerFunctionResult = 0x21,
            FlashBlockProgramResult = 0x31
        }

        private enum Bootloader
        {
            Empty = 0x00,
            SBEC3_128k = 0x01,
            SBEC3_256k = 0x02,
            SBEC3_custom = 0x03
        }

        private enum WorkerFunction
        {
            Empty = 0x00,
            PartNumberRead = 0x01,
            FlashRead = 0x02,
            FlashID = 0x03,
            FlashErase = 0x04,
            FlashProgram = 0x05
        }

        private enum FlashMemoryManufacturer
        {
            STMicroelectronics = 0x20,
            CATALYST = 0x31,
            Intel = 0x89
        }

        private enum FlashMemoryType
        {
            M28F102 = 0x50,
            CAT28F102 = 0x51,
            N28F010 = 0xB4,
            N28F020 = 0xBD,
            M28F210 = 0xE0,
            M28F220 = 0xE6
        }

        private enum BootloaderError
        {
            OK = 0x00,
            NoResponseToMagicByte = 0x01,
            UnexpectedResponseToMagicByte = 0x02,
            SecuritySeedResponseTimeout = 0x03,
            SecuritySeedChecksumError = 0x04,
            SecurityKeyStatusTimeout = 0x05,
            SecurityKeyNotAccepted = 0x06,
            StartBootloaderTimeout = 0x07,
            UnexpectedBootloaderStatusByte = 0x08
        }

        private enum WorkerFunctionError
        {
            OK = 0x00,
            NoResponseToPing = 0x01,
            UploadFinishedStatusByteNotReceived = 0x02,
            UnexpectedUploadFinishedStatus = 0x03
        }

        public BootstrapToolsForm(MainForm IncomingForm)
        {
            OriginalForm = IncomingForm;
            InitializeComponent();
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            MainForm.Packet.PacketReceived += PacketReceivedHandler; // subscribe to the OnPacketReceived event
            BootloaderComboBox.SelectedIndex = 1;
            WorkerFunctionComboBox.SelectedIndex = 1;
            FlashChipComboBox.SelectedIndex = 0;
            ActiveControl = BootstrapButton;
            SCIBusBootstrapLogFilename = @"LOG/SCI/scibootstraplog_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt";
        }

        private void BootstrapButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Turn key to OFF position." + Environment.NewLine + "Click OK when done.", "Information", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) == DialogResult.OK)
            {
                if (OriginalForm.PCM.speed != "62500 baud")
                {
                    OriginalForm.SelectSCIBusPCMHSMode();
                }

                MainForm.Packet.tx.bus = (byte)Packet.Bus.usb;
                MainForm.Packet.tx.command = (byte)Packet.Command.settings;
                MainForm.Packet.tx.mode = (byte)Packet.SettingsMode.setProgVolt;
                MainForm.Packet.tx.payload = new byte[1] { 0x80 };
                MainForm.Packet.GeneratePacket();
                OriginalForm.TransmitUSBPacket("[<-TX] Apply VBB to SCI-RX pin:");

                if (MessageBox.Show("Turn key to RUN position." + Environment.NewLine + "Click OK when done.", "Information", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) == DialogResult.OK)
                {
                    MainForm.Packet.tx.bus = (byte)Packet.Bus.usb;
                    MainForm.Packet.tx.command = (byte)Packet.Command.settings;
                    MainForm.Packet.tx.mode = (byte)Packet.SettingsMode.setProgVolt;
                    MainForm.Packet.tx.payload = new byte[1] { 0x00 };
                    MainForm.Packet.GeneratePacket();
                    OriginalForm.TransmitUSBPacket("[<-TX] Remove VBB from SCI-RX pin:");

                    MainForm.Packet.tx.bus = (byte)Packet.Bus.usb;
                    MainForm.Packet.tx.command = (byte)Packet.Command.debug;
                    MainForm.Packet.tx.mode = (byte)Packet.DebugMode.initBootstrapMode;
                    MainForm.Packet.tx.payload = new byte[2] { (byte)BootloaderComboBox.SelectedIndex, (byte)FlashChipComboBox.SelectedIndex };
                    MainForm.Packet.GeneratePacket();
                    OriginalForm.TransmitUSBPacket("[<-TX] Init PCM bootstrap mode:");

                    switch ((byte)BootloaderComboBox.SelectedIndex)
                    {
                        case (byte)Bootloader.Empty:
                            OriginalForm.UpdateUSBTextBox("[INFO] Bootloader: empty.");
                            break;
                        case (byte)Bootloader.SBEC3_128k:
                            OriginalForm.UpdateUSBTextBox("[INFO] Bootloader: SBEC3 (128k).");
                            break;
                        case (byte)Bootloader.SBEC3_256k:
                            OriginalForm.UpdateUSBTextBox("[INFO] Bootloader: SBEC3 (256k).");
                            break;
                        case (byte)Bootloader.SBEC3_custom:
                            OriginalForm.UpdateUSBTextBox("[INFO] Bootloader: SBEC3 (custom).");
                            break;
                        default:
                            OriginalForm.UpdateUSBTextBox("[INFO] Bootloader: unknown.");
                            break;
                    }
                }
                else
                {
                    MainForm.Packet.tx.bus = (byte)Packet.Bus.usb;
                    MainForm.Packet.tx.command = (byte)Packet.Command.settings;
                    MainForm.Packet.tx.mode = (byte)Packet.SettingsMode.setProgVolt;
                    MainForm.Packet.tx.payload = new byte[1] { 0x00 };
                    MainForm.Packet.GeneratePacket();
                    OriginalForm.TransmitUSBPacket("[<-TX] Remove VBB from SCI-RX pin:");
                }
            }
        }

        private void ExecuteButton_Click(object sender, EventArgs e)
        {
            CurrentWorkerFunctionIndex = (byte)WorkerFunctionComboBox.SelectedIndex;
            CurrentFlashChipIndex = (byte)FlashChipComboBox.SelectedIndex;
            MainForm.Packet.tx.bus = (byte)Packet.Bus.usb;
            MainForm.Packet.tx.command = (byte)Packet.Command.debug;
            MainForm.Packet.tx.mode = (byte)Packet.DebugMode.writeWorkerFunction;
            MainForm.Packet.tx.payload = new byte[2] { CurrentWorkerFunctionIndex, CurrentFlashChipIndex };
            MainForm.Packet.GeneratePacket();
            OriginalForm.TransmitUSBPacket("[<-TX] Upload worker function:");

            switch (CurrentWorkerFunctionIndex)
            {
                case (byte)WorkerFunction.Empty:
                    OriginalForm.UpdateUSBTextBox("[INFO] Worker function: empty.");
                    break;
                case (byte)WorkerFunction.PartNumberRead:
                    OriginalForm.UpdateUSBTextBox("[INFO] Worker function: part number read.");
                    break;
                case (byte)WorkerFunction.FlashRead:
                    OriginalForm.UpdateUSBTextBox("[INFO] Worker function: flash read.");
                    break;
                case (byte)WorkerFunction.FlashID:
                    OriginalForm.UpdateUSBTextBox("[INFO] Worker function: flash ID.");
                    break;
                case (byte)WorkerFunction.FlashErase:
                    OriginalForm.UpdateUSBTextBox("[INFO] Worker function: flash erase.");
                    break;
                case (byte)WorkerFunction.FlashProgram:
                    OriginalForm.UpdateUSBTextBox("[INFO] Worker function: flash program.");
                    break;
                default:
                    OriginalForm.UpdateUSBTextBox("[INFO] Worker function: unknown.");
                    break;
            }
        }

        private void FlashChipDetectButton_Click(object sender, EventArgs e)
        {
            WorkerFunctionComboBox.SelectedIndex = (byte)WorkerFunction.FlashID;
            ExecuteButton_Click(this, EventArgs.Empty);
        }

        private void PacketReceivedHandler(object sender, EventArgs e)
        {
            if ((MainForm.Packet.rx.bus == (byte)Packet.Bus.pcm) | (MainForm.Packet.rx.bus == (byte)Packet.Bus.tcm))
            {
                byte[] SCIBusResponseBytes = MainForm.Packet.rx.payload.Skip(4).ToArray(); // skip 4 timestamp bytes

                if (SCIBusResponseBytes.Length > 0)
                {
                    switch (SCIBusResponseBytes[0])
                    {
                        case (byte)SCI_ID.WorkerFunctionResult:
                            switch (CurrentWorkerFunctionIndex)
                            {
                                case (byte)WorkerFunction.FlashID:
                                    if (SCIBusResponseBytes.Length >= 3)
                                    {
                                        switch (SCIBusResponseBytes[1])
                                        {
                                            case (byte)FlashMemoryManufacturer.STMicroelectronics:
                                                UpdateTextBox(SCIBusBootstrapInfoTextBox, "Flash memory: STMicroelectronics ");
                                                break;
                                            case (byte)FlashMemoryManufacturer.CATALYST:
                                                UpdateTextBox(SCIBusBootstrapInfoTextBox, "Flash memory: CATALYST ");
                                                break;
                                            case (byte)FlashMemoryManufacturer.Intel:
                                                UpdateTextBox(SCIBusBootstrapInfoTextBox, "Flash memory: Intel ");
                                                break;
                                            default:
                                                UpdateTextBox(SCIBusBootstrapInfoTextBox, "Flash memory: unknown");
                                                break;
                                        }

                                        switch (SCIBusResponseBytes[2])
                                        {
                                            case (byte)FlashMemoryType.M28F102:
                                                UpdateTextBox(SCIBusBootstrapInfoTextBox, "M28F102 (128 kB)");
                                                FlashChipComboBox.SelectedIndex = 1;
                                                break;
                                            case (byte)FlashMemoryType.CAT28F102:
                                                UpdateTextBox(SCIBusBootstrapInfoTextBox, "CAT28F102 (128 kB)");
                                                FlashChipComboBox.SelectedIndex = 2;
                                                break;
                                            case (byte)FlashMemoryType.N28F010:
                                                UpdateTextBox(SCIBusBootstrapInfoTextBox, "N28F010 (128 kB)");
                                                FlashChipComboBox.SelectedIndex = 3;
                                                break;
                                            case (byte)FlashMemoryType.N28F020:
                                                UpdateTextBox(SCIBusBootstrapInfoTextBox, "N28F020 (256 kB)");
                                                FlashChipComboBox.SelectedIndex = 4;
                                                break;
                                            case (byte)FlashMemoryType.M28F210:
                                                UpdateTextBox(SCIBusBootstrapInfoTextBox, "M28F210 (256 kB)");
                                                FlashChipComboBox.SelectedIndex = 5;
                                                break;
                                            case (byte)FlashMemoryType.M28F220:
                                                UpdateTextBox(SCIBusBootstrapInfoTextBox, "M28F220 (256 kB)");
                                                FlashChipComboBox.SelectedIndex = 6;
                                                break;
                                            default:
                                                FlashChipComboBox.SelectedIndex = 0;
                                                break;
                                        }
                                    }
                                    break;
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void UpdateTextBox(TextBox textBox, string text)
        {
            if (!textBox.IsDisposed)
            {
                if (textBox.TextLength + text.Length > textBox.MaxLength)
                {
                    textBox.Clear();
                    GC.Collect();
                }

                textBox.AppendText(text);

                if ((textBox.Name == "SCIBusBootstrapInfoTextBox") && (SCIBusBootstrapInfoTextBox != null)) File.AppendAllText(SCIBusBootstrapLogFilename, text);
            }
        }

        private void BootstrapToolsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            MainForm.Packet.PacketReceived -= PacketReceivedHandler; // unsubscribe from the OnPacketReceived event
        }
    }
}
