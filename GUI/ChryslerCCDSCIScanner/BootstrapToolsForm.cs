using System;
using System.Drawing;
using System.Windows.Forms;

namespace ChryslerCCDSCIScanner
{
    public partial class BootstrapToolsForm : Form
    {
        public MainForm OriginalForm;

        public BootstrapToolsForm(MainForm IncomingForm)
        {
            OriginalForm = IncomingForm;
            InitializeComponent();
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            MainForm.Packet.PacketReceived += PacketReceivedHandler; // subscribe to the OnPacketReceived event
            BootloaderComboBox.SelectedIndex = 0;
            WorkerFunctionComboBox.SelectedIndex = 0;
            ActiveControl = BootstrapButton;
        }

        private void SCIBusPCMBootstrapButton_Click(object sender, EventArgs e)
        {
            if (OriginalForm.PCM.speed != "62500 baud")
            {
                OriginalForm.SelectSCIBusPCMHSMode();
            }
            
            MainForm.Packet.tx.source = (byte)Packet.Source.device;
            MainForm.Packet.tx.target = (byte)Packet.Target.device;
            MainForm.Packet.tx.command = (byte)Packet.Command.debug;
            MainForm.Packet.tx.mode = (byte)Packet.DebugMode.initBootstrapMode;
            MainForm.Packet.tx.payload = new byte[1] { (byte)BootloaderComboBox.SelectedIndex };
            MainForm.Packet.GeneratePacket();
            OriginalForm.TransmitUSBPacket("[<-TX] Init PCM bootstrap mode:");

            switch ((byte)BootloaderComboBox.SelectedIndex)
            {
                case 0:
                    OriginalForm.UpdateUSBTextBox("[INFO] Bootloader: SBEC3 (128k).");
                    break;
                case 1:
                    OriginalForm.UpdateUSBTextBox("[INFO] Bootloader: SBEC3 (256k).");
                    break;
                default:
                    OriginalForm.UpdateUSBTextBox("[INFO] Bootloader: SBEC3 (256k).");
                    break;
            }
        }

        private void UploadButton_Click(object sender, EventArgs e)
        {
            MainForm.Packet.tx.source = (byte)Packet.Source.device;
            MainForm.Packet.tx.target = (byte)Packet.Target.device;
            MainForm.Packet.tx.command = (byte)Packet.Command.debug;
            MainForm.Packet.tx.mode = (byte)Packet.DebugMode.writeWorkerFunction;
            MainForm.Packet.tx.payload = new byte[1] { (byte)WorkerFunctionComboBox.SelectedIndex };
            MainForm.Packet.GeneratePacket();
            OriginalForm.TransmitUSBPacket("[<-TX] Upload worker function:");

            switch ((byte)WorkerFunctionComboBox.SelectedIndex)
            {
                case 0:
                    OriginalForm.UpdateUSBTextBox("[INFO] Worker function: flash read.");
                    break;
                default:
                    OriginalForm.UpdateUSBTextBox("[INFO] Worker function: flash read.");
                    break;
            }
        }

        private void PacketReceivedHandler(object sender, EventArgs e)
        {
            // Empty.
        }

        private void BootstrapToolsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            MainForm.Packet.PacketReceived -= PacketReceivedHandler; // unsubscribe from the OnPacketReceived event
        }
    }
}
