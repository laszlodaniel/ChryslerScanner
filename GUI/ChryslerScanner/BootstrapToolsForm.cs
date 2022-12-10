using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows.Forms;

namespace ChryslerScanner
{
    public partial class BootstrapToolsForm : Form
    {
        private MainForm OriginalForm;

        private string SCIBusBootstrapLogFilename;
        private string SCIBusFlashReadFilename;
        private string SCIBusEEPROMReadFilename;
        private byte[] FlashFileBuffer = null;
        private byte[] EEPROMFileBuffer = null;
        private uint FlashChipSize = 0;

        private bool SCIBusResponse = false;
        private bool SCIBusNextRequest = false;
        private byte SCIBusRxRetryCount = 0;
        private byte SCIBusTxRetryCount = 0;
        private bool SCIBusBootstrapFinished = false;
        private bool SCIBusRxTimeout = false;
        private bool SCIBusTxTimeout = false;
        private Task CurrentTask = Task.None;

        private uint SCIBusCurrentMemoryOffset = 0;
        private byte[] SCIBusTxPayload = null;

        private const double MinBattVolts = 11.5; // V
        private const double MinBootVolts = 11.5; // V
        private const double MinProgVolts = 19.5; // V

        private const ushort FlashReadBlockSize = 512;
        private const ushort FlashWriteBlockSize = 512;
        private const ushort EEPROMReadBlockSize = 512;
        private const ushort EEPROMWriteBlockSize = 512;

        private string PartNumberString = string.Empty;
        private string NewPartNumberString = string.Empty;

        private bool SwitchBackToLSWhenExit = false;

        private System.Timers.Timer SCIBusNextRequestTimer = new System.Timers.Timer();
        private System.Timers.Timer SCIBusRxTimeoutTimer = new System.Timers.Timer();
        private System.Timers.Timer SCIBusTxTimeoutTimer = new System.Timers.Timer();
        private BackgroundWorker SCIBusBootstrapWorker = new BackgroundWorker();

        private enum SCI_ID
        {
            WriteError = 0x01,
            BootstrapBaudrateSet = 0x06,
            UploadWorkerFunctionResult = 0x11,
            StartWorkerFunction = 0x21,
            ExitWorkerFunction = 0x22,
            BootstrapSeedKeyRequest = 0x24,
            BootstrapSeedKeyResponse = 0x26,
            FlashBlockWrite = 0x31,
            FlashBlockRead = 0x34,
            EEPROMBlockWrite = 0x37,
            EEPROMBlockRead = 0x3A,
            StartBootloader = 0x47,
            UploadBootloader = 0x4C,
            BlockSizeError = 0x80,
            EraseError_81 = 0x81,
            EraseError_82 = 0x82,
            EraseError_83 = 0x83,
            OffsetError = 0x84,
            BootstrapModeNotProtected = 0xDB
        }

        private enum Bootloader
        {
            Empty = 0x00,
            SBEC3_128k = 0x01,
            SBEC3_256k = 0x02,
            SBEC3_256k_custom = 0x03,
            JTEC_256k = 0x04
        }

        private enum WorkerFunction
        {
            Empty = 0x00,
            PartNumberRead = 0x01,
            FlashID = 0x02,
            FlashRead = 0x03,
            FlashErase = 0x04,
            FlashWrite = 0x05,
            VerifyFlashChecksum = 0x06,
            EEPROMReadSPI = 0x07,
            EEPROMWriteSPI = 0x08,
            EEPROMReadParallel = 0x09,
            EEPROMWriteParallel = 0x0A
        }

        private enum FlashMemoryManufacturer
        {
            STMicroelectronics = 0x20,
            CATALYST = 0x31,
            Intel = 0x89,
            TexasInstruments = 0x97
        }

        private enum FlashMemoryType
        {
            M28F102 = 0x50,
            CAT28F102 = 0x51,
            N28F010 = 0xB4,
            N28F020 = 0xBD,
            M28F210 = 0xE0,
            M28F220 = 0xE6,
            M28F200T = 0x74,
            M28F200B = 0x75,
            TMS28F210 = 0xE5
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
            UploadInterrupted = 0x02,
            UnexpectedUploadResult = 0x03
        }

        private enum Task
        {
            None,
            CheckVoltages,
            ReadPartNumber,
            DetectFlashMemoryType,
            BackupFlashMemory,
            ReadFlashMemory,
            BackupEEPROM,
            EraseFlashMemory,
            WriteFlashMemory,
            VerifyFlashChecksum,
            UpdateEEPROM,
            ReadEEPROM,
            WriteEEPROM,
            FinishFlashRead,
            FinishFlashWrite,
            FinishEEPROMRead,
            FinishEEPROMWrite
        }

        public BootstrapToolsForm(MainForm IncomingForm)
        {
            OriginalForm = IncomingForm;
            InitializeComponent();
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            MainForm.Packet.PacketReceived += PacketReceivedHandler; // subscribe to the OnPacketReceived event
            BootloaderComboBox.SelectedIndex = 2;
            WorkerFunctionComboBox.SelectedIndex = 0;
            FlashChipComboBox.SelectedIndex = 0;
            SCIBusBootstrapLogFilename = @"LOG/SCI/scibootstraplog_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt";

            SCIBusNextRequestTimer.Elapsed += new ElapsedEventHandler(SCIBusNextRequestHandler);
            SCIBusNextRequestTimer.Interval = 25; // ms
            SCIBusNextRequestTimer.AutoReset = false;
            SCIBusNextRequestTimer.Enabled = true;
            SCIBusNextRequestTimer.Start();

            SCIBusRxTimeoutTimer.Elapsed += new ElapsedEventHandler(SCIBusRxTimeoutHandler);
            SCIBusRxTimeoutTimer.Interval = 2000; // ms
            SCIBusRxTimeoutTimer.AutoReset = false;
            SCIBusRxTimeoutTimer.Enabled = true;
            SCIBusRxTimeoutTimer.Stop();

            SCIBusTxTimeoutTimer.Elapsed += new ElapsedEventHandler(SCIBusTxTimeoutHandler);
            SCIBusTxTimeoutTimer.Interval = 2000; // ms
            SCIBusTxTimeoutTimer.AutoReset = false;
            SCIBusTxTimeoutTimer.Enabled = true;
            SCIBusTxTimeoutTimer.Stop();

            SCIBusBootstrapWorker.WorkerReportsProgress = true;
            SCIBusBootstrapWorker.WorkerSupportsCancellation = true;
            SCIBusBootstrapWorker.DoWork += new DoWorkEventHandler(SCIBusBootstrap_DoWork);
            SCIBusBootstrapWorker.ProgressChanged += new ProgressChangedEventHandler(SCIBusBootstrap_ProgressChanged);
            SCIBusBootstrapWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(SCIBusBootstrap_RunWorkerCompleted);

            UpdateTextBox(SCIBusBootstrapInfoTextBox, "Begin by bootstrapping ECU with selected bootloader.");

            ActiveControl = BootstrapButton;
        }

        private void SCIBusNextRequestHandler(object source, ElapsedEventArgs e) => SCIBusNextRequest = true;

        private void SCIBusRxTimeoutHandler(object source, ElapsedEventArgs e) => SCIBusRxTimeout = true;

        private void SCIBusTxTimeoutHandler(object source, ElapsedEventArgs e) => SCIBusTxTimeout = true;

        private void SCIBusBootstrap_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!SCIBusBootstrapFinished && !SCIBusBootstrapWorker.CancellationPending)
            {
                Thread.Sleep(1);

                while (!SCIBusNextRequest && !SCIBusBootstrapWorker.CancellationPending) // wait for next request message
                {
                    Thread.Sleep(1);
                }

                if (SCIBusBootstrapWorker.CancellationPending) break;

                SCIBusResponse = false;
                SCIBusNextRequest = false;

                SCIBusBootstrapWorker.ReportProgress(0); // request message is sent in the ProgressChanged event handler method

                while (!SCIBusResponse && !SCIBusRxTimeout && !SCIBusBootstrapWorker.CancellationPending)
                {
                    Thread.Sleep(1);
                }

                if (SCIBusBootstrapWorker.CancellationPending) break;

                if (SCIBusRxTimeout)
                {
                    SCIBusRxRetryCount++;

                    if (SCIBusRxRetryCount > 9)
                    {
                        SCIBusRxRetryCount = 0;
                        e.Cancel = true;
                        break;
                    }

                    SCIBusNextRequest = true;
                    SCIBusRxTimeout = false;
                    SCIBusRxTimeoutTimer.Stop();
                    SCIBusRxTimeoutTimer.Start();
                }

                if (SCIBusResponse)
                {
                    SCIBusRxRetryCount = 0;
                }
            }

            if (SCIBusBootstrapWorker.CancellationPending)
            {
                e.Cancel = true;
            }
        }

        private void SCIBusBootstrap_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 0)
            {
                switch (CurrentTask)
                {
                    case Task.CheckVoltages:
                        UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "Step 1. Check voltages.");
                        MainForm.Packet.tx.bus = (byte)Packet.Bus.usb;
                        MainForm.Packet.tx.command = (byte)Packet.Command.request;
                        MainForm.Packet.tx.mode = (byte)Packet.RequestMode.AllVolts;
                        MainForm.Packet.tx.payload = null;
                        MainForm.Packet.GeneratePacket();
                        OriginalForm.TransmitUSBPacket("[<-TX] Request voltage measurements:");
                        break;
                    case Task.ReadPartNumber:
                        UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "Step 2. Read part number.");
                        WorkerFunctionComboBox.SelectedIndex = (byte)WorkerFunction.PartNumberRead;
                        UploadButton_Click(this, EventArgs.Empty);
                        break;
                    case Task.DetectFlashMemoryType:
                        UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "Step 3. Detect flash memory type.");
                        WorkerFunctionComboBox.SelectedIndex = (byte)WorkerFunction.FlashID;
                        UploadButton_Click(this, EventArgs.Empty);
                        break;
                    case Task.BackupFlashMemory:
                        if (WorkerFunctionComboBox.SelectedIndex != (byte)WorkerFunction.FlashRead)
                        {
                            UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "Step 4. Backup flash memory.");
                            WorkerFunctionComboBox.SelectedIndex = (byte)WorkerFunction.FlashRead;
                            UploadButton_Click(this, EventArgs.Empty);
                            SCIBusFlashReadFilename = @"ROMs/PCM/pcm_flash_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".bin";
                        }
                        else
                        {
                            SCIBusTxPayload[1] = (byte)((SCIBusCurrentMemoryOffset >> 16) & 0xFF);
                            SCIBusTxPayload[2] = (byte)((SCIBusCurrentMemoryOffset >> 8) & 0xFF);
                            SCIBusTxPayload[3] = (byte)(SCIBusCurrentMemoryOffset & 0xFF);

                            MainForm.Packet.tx.bus = (byte)Packet.Bus.pcm;
                            MainForm.Packet.tx.command = (byte)Packet.Command.msgTx;
                            MainForm.Packet.tx.mode = (byte)Packet.MsgTxMode.single;
                            MainForm.Packet.tx.payload = SCIBusTxPayload;
                            MainForm.Packet.GeneratePacket();

                            OriginalForm.TransmitUSBPacket("[<-TX] Send an SCI-bus (PCM) message once:");
                            SCIBusRxTimeout = false;
                            SCIBusRxTimeoutTimer.Stop();
                            SCIBusRxTimeoutTimer.Start();
                        }
                        break;
                    case Task.ReadFlashMemory:
                        if (WorkerFunctionComboBox.SelectedIndex != (byte)WorkerFunction.FlashRead)
                        {
                            WorkerFunctionComboBox.SelectedIndex = (byte)WorkerFunction.FlashRead;
                            UploadButton_Click(this, EventArgs.Empty);

                            SCIBusCurrentMemoryOffset = 0;
                            SCIBusBootstrapToolsProgressLabel.Text = "Progress: " + (byte)(Math.Round((double)SCIBusCurrentMemoryOffset / (double)FlashChipSize * 100.0)) + "% (" + SCIBusCurrentMemoryOffset.ToString() + "/" + FlashChipSize.ToString() + " bytes)";
                            SCIBusTxPayload = new byte[6];
                            SCIBusTxPayload[0] = 0x33;
                            SCIBusTxPayload[1] = (byte)((SCIBusCurrentMemoryOffset >> 16) & 0xFF);
                            SCIBusTxPayload[2] = (byte)((SCIBusCurrentMemoryOffset >> 8) & 0xFF);
                            SCIBusTxPayload[3] = (byte)(SCIBusCurrentMemoryOffset & 0xFF);
                            SCIBusTxPayload[4] = (byte)((FlashReadBlockSize >> 8) & 0xFF);
                            SCIBusTxPayload[5] = (byte)(FlashReadBlockSize & 0xFF);
                        }
                        else
                        {
                            SCIBusTxPayload[1] = (byte)((SCIBusCurrentMemoryOffset >> 16) & 0xFF);
                            SCIBusTxPayload[2] = (byte)((SCIBusCurrentMemoryOffset >> 8) & 0xFF);
                            SCIBusTxPayload[3] = (byte)(SCIBusCurrentMemoryOffset & 0xFF);

                            MainForm.Packet.tx.bus = (byte)Packet.Bus.pcm;
                            MainForm.Packet.tx.command = (byte)Packet.Command.msgTx;
                            MainForm.Packet.tx.mode = (byte)Packet.MsgTxMode.single;
                            MainForm.Packet.tx.payload = SCIBusTxPayload;
                            MainForm.Packet.GeneratePacket();

                            OriginalForm.TransmitUSBPacket("[<-TX] Send an SCI-bus (PCM) message once:");
                            SCIBusRxTimeout = false;
                            SCIBusRxTimeoutTimer.Stop();
                            SCIBusRxTimeoutTimer.Start();
                        }
                        break;
                    case Task.BackupEEPROM:
                        if (WorkerFunctionComboBox.SelectedIndex != (byte)WorkerFunction.EEPROMReadSPI)
                        {
                            UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "Step 5. Backup EEPROM.");

                            switch (BootloaderComboBox.SelectedIndex)
                            {
                                case (byte)Bootloader.JTEC_256k:
                                    WorkerFunctionComboBox.SelectedIndex = (byte)WorkerFunction.EEPROMReadParallel;
                                    break;
                                default:
                                    WorkerFunctionComboBox.SelectedIndex = (byte)WorkerFunction.EEPROMReadSPI;
                                    break;
                            }
                            
                            SCIBusCurrentMemoryOffset = 0;
                            SCIBusBootstrapToolsProgressLabel.Text = "Progress: " + (byte)(Math.Round((double)SCIBusCurrentMemoryOffset / 512.0 * 100.0)) + "% (" + SCIBusCurrentMemoryOffset.ToString() + "/512 bytes)";
                            SCIBusTxPayload = new byte[5] { 0x39, 0x00, 0x00, 0x02, 0x00 };
                            UploadButton_Click(this, EventArgs.Empty);
                            SCIBusEEPROMReadFilename = @"ROMs/PCM/pcm_eeprom_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".bin";
                        }
                        else
                        {
                            MainForm.Packet.tx.bus = (byte)Packet.Bus.pcm;
                            MainForm.Packet.tx.command = (byte)Packet.Command.msgTx;
                            MainForm.Packet.tx.mode = (byte)Packet.MsgTxMode.single;
                            MainForm.Packet.tx.payload = SCIBusTxPayload;
                            MainForm.Packet.GeneratePacket();

                            OriginalForm.TransmitUSBPacket("[<-TX] Send an SCI-bus (PCM) message once:");
                            SCIBusRxTimeout = false;
                            SCIBusRxTimeoutTimer.Stop();
                            SCIBusRxTimeoutTimer.Start();
                        }
                        break;
                    case Task.EraseFlashMemory:
                        UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "Step 6. Erase flash memory.");
                        WorkerFunctionComboBox.SelectedIndex = (byte)WorkerFunction.FlashErase;
                        UploadButton_Click(this, EventArgs.Empty);
                        break;
                    case Task.WriteFlashMemory:
                        if (WorkerFunctionComboBox.SelectedIndex != (byte)WorkerFunction.FlashWrite)
                        {
                            UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "Step 7. Write flash memory.");
                            WorkerFunctionComboBox.SelectedIndex = (byte)WorkerFunction.FlashWrite;
                            UploadButton_Click(this, EventArgs.Empty);

                            SCIBusCurrentMemoryOffset = 0;
                            SCIBusBootstrapToolsProgressLabel.Text = "Progress: " + (byte)(Math.Round((double)SCIBusCurrentMemoryOffset / (double)FlashChipSize * 100.0)) + "% (" + SCIBusCurrentMemoryOffset.ToString() + "/" + FlashChipSize.ToString() + " bytes)";
                        }
                        else
                        {
                            SCIBusTxPayload[1] = (byte)((SCIBusCurrentMemoryOffset >> 16) & 0xFF);
                            SCIBusTxPayload[2] = (byte)((SCIBusCurrentMemoryOffset >> 8) & 0xFF);
                            SCIBusTxPayload[3] = (byte)(SCIBusCurrentMemoryOffset & 0xFF);
                            Array.Copy(FlashFileBuffer, SCIBusCurrentMemoryOffset, SCIBusTxPayload, 6, FlashWriteBlockSize);

                            MainForm.Packet.tx.bus = (byte)Packet.Bus.pcm;
                            MainForm.Packet.tx.command = (byte)Packet.Command.msgTx;
                            MainForm.Packet.tx.mode = (byte)Packet.MsgTxMode.singleVPP;
                            MainForm.Packet.tx.payload = SCIBusTxPayload;
                            MainForm.Packet.GeneratePacket();

                            OriginalForm.TransmitUSBPacket("[<-TX] Send an SCI-bus (PCM) message once:");
                            SCIBusRxTimeout = false;
                            SCIBusRxTimeoutTimer.Stop();
                            SCIBusRxTimeoutTimer.Start();
                        }
                        break;
                    case Task.VerifyFlashChecksum:
                        if (WorkerFunctionComboBox.SelectedIndex != (byte)WorkerFunction.VerifyFlashChecksum)
                        {
                            UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "Step 8. Verify flash checksum.");
                            WorkerFunctionComboBox.SelectedIndex = (byte)WorkerFunction.VerifyFlashChecksum;
                            UploadButton_Click(this, EventArgs.Empty);
                        }
                        break;
                    case Task.UpdateEEPROM:
                        if (WorkerFunctionComboBox.SelectedIndex != (byte)WorkerFunction.EEPROMWriteSPI)
                        {
                            UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "Step 9. Update EEPROM.");

                            switch (BootloaderComboBox.SelectedIndex)
                            {
                                case (byte)Bootloader.JTEC_256k:
                                    //WorkerFunctionComboBox.SelectedIndex = (byte)WorkerFunction.EEPROMWriteParallel;
                                    WorkerFunctionComboBox.SelectedIndex = (byte)WorkerFunction.EEPROMWriteSPI;
                                    break;
                                default:
                                    WorkerFunctionComboBox.SelectedIndex = (byte)WorkerFunction.EEPROMWriteSPI;
                                    break;
                            }

                            SCIBusTxPayload = new byte[6] { 0x36, 0x00, 0x00, 0x00, 0x01, 0xFF };
                            UploadButton_Click(this, EventArgs.Empty);
                        }
                        else
                        {
                            //MainForm.Packet.tx.bus = (byte)Packet.Bus.pcm;
                            //MainForm.Packet.tx.command = (byte)Packet.Command.msgTx;
                            //MainForm.Packet.tx.mode = (byte)Packet.MsgTxMode.single;
                            //MainForm.Packet.tx.payload = SCIBusTxPayload;
                            //MainForm.Packet.GeneratePacket();

                            //OriginalForm.TransmitUSBPacket("[<-TX] Send an SCI-bus (PCM) message once:");
                            //SCIBusRxTimeout = false;
                            //SCIBusRxTimeoutTimer.Stop();
                            //SCIBusRxTimeoutTimer.Start();

                            ExitButton_Click(this, EventArgs.Empty);
                            SCIBusResponse = true;
                        }
                        break;
                    case Task.ReadEEPROM:
                        if (WorkerFunctionComboBox.SelectedIndex != (byte)WorkerFunction.EEPROMReadSPI)
                        {
                            switch (BootloaderComboBox.SelectedIndex)
                            {
                                case (byte)Bootloader.JTEC_256k:
                                    //WorkerFunctionComboBox.SelectedIndex = (byte)WorkerFunction.EEPROMReadParallel;
                                    WorkerFunctionComboBox.SelectedIndex = (byte)WorkerFunction.EEPROMReadSPI;
                                    break;
                                default:
                                    WorkerFunctionComboBox.SelectedIndex = (byte)WorkerFunction.EEPROMReadSPI;
                                    break;
                            }

                            SCIBusCurrentMemoryOffset = 0;
                            SCIBusBootstrapToolsProgressLabel.Text = "Progress: " + (byte)(Math.Round((double)SCIBusCurrentMemoryOffset / 512.0 * 100.0)) + "% (" + SCIBusCurrentMemoryOffset.ToString() + "/512 bytes)";
                            SCIBusTxPayload = new byte[5];
                            SCIBusTxPayload[0] = 0x39;
                            SCIBusTxPayload[1] = (byte)((SCIBusCurrentMemoryOffset >> 8) & 0xFF);
                            SCIBusTxPayload[2] = (byte)(SCIBusCurrentMemoryOffset & 0xFF);
                            SCIBusTxPayload[3] = (byte)((EEPROMReadBlockSize >> 8) & 0xFF);
                            SCIBusTxPayload[4] = (byte)(EEPROMReadBlockSize & 0xFF);
                            UploadButton_Click(this, EventArgs.Empty);
                        }
                        else
                        {
                            MainForm.Packet.tx.bus = (byte)Packet.Bus.pcm;
                            MainForm.Packet.tx.command = (byte)Packet.Command.msgTx;
                            MainForm.Packet.tx.mode = (byte)Packet.MsgTxMode.single;
                            MainForm.Packet.tx.payload = SCIBusTxPayload;
                            MainForm.Packet.GeneratePacket();

                            OriginalForm.TransmitUSBPacket("[<-TX] Send an SCI-bus (PCM) message once:");
                            SCIBusRxTimeout = false;
                            SCIBusRxTimeoutTimer.Stop();
                            SCIBusRxTimeoutTimer.Start();
                        }
                        break;
                    case Task.WriteEEPROM:
                        if (WorkerFunctionComboBox.SelectedIndex != (byte)WorkerFunction.EEPROMWriteSPI)
                        {
                            switch (BootloaderComboBox.SelectedIndex)
                            {
                                case (byte)Bootloader.JTEC_256k:
                                    //WorkerFunctionComboBox.SelectedIndex = (byte)WorkerFunction.EEPROMWriteParallel;
                                    WorkerFunctionComboBox.SelectedIndex = (byte)WorkerFunction.EEPROMWriteSPI;
                                    break;
                                default:
                                    WorkerFunctionComboBox.SelectedIndex = (byte)WorkerFunction.EEPROMWriteSPI;
                                    break;
                            }

                            SCIBusCurrentMemoryOffset = 0;
                            SCIBusBootstrapToolsProgressLabel.Text = "Progress: " + (byte)(Math.Round((double)SCIBusCurrentMemoryOffset / 512.0 * 100.0)) + "% (" + SCIBusCurrentMemoryOffset.ToString() + "/512 bytes)";
                            SCIBusTxPayload = new byte[5 + EEPROMWriteBlockSize];
                            SCIBusTxPayload[0] = 0x36;
                            SCIBusTxPayload[1] = (byte)((SCIBusCurrentMemoryOffset >> 8) & 0xFF);
                            SCIBusTxPayload[2] = (byte)(SCIBusCurrentMemoryOffset & 0xFF);
                            SCIBusTxPayload[3] = (byte)((EEPROMWriteBlockSize >> 8) & 0xFF);
                            SCIBusTxPayload[4] = (byte)(EEPROMWriteBlockSize & 0xFF);
                            Array.Copy(EEPROMFileBuffer, SCIBusCurrentMemoryOffset, SCIBusTxPayload, 5, EEPROMWriteBlockSize);
                            UploadButton_Click(this, EventArgs.Empty);
                        }
                        else
                        {
                            MainForm.Packet.tx.bus = (byte)Packet.Bus.pcm;
                            MainForm.Packet.tx.command = (byte)Packet.Command.msgTx;
                            MainForm.Packet.tx.mode = (byte)Packet.MsgTxMode.single;
                            MainForm.Packet.tx.payload = SCIBusTxPayload;
                            MainForm.Packet.GeneratePacket();

                            OriginalForm.TransmitUSBPacket("[<-TX] Send an SCI-bus (PCM) message once:");
                            SCIBusRxTimeout = false;
                            SCIBusRxTimeoutTimer.Stop();
                            SCIBusRxTimeoutTimer.Start();
                        }
                        break;
                    case Task.FinishFlashRead:
                        UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "Flash memory reading session finished successfully.");
                        SCIBusBootstrapFinished = true;
                        SCIBusResponse = true;
                        break;
                    case Task.FinishFlashWrite:
                        UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "Flash memory writing session finished successfully.");
                        SCIBusBootstrapFinished = true;
                        SCIBusResponse = true;
                        UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "User is instructed to turn key to OFF position.");
                        MessageBox.Show("Turn key to OFF position." + Environment.NewLine + "Wait for 5 seconds before starting the engine.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;
                    case Task.FinishEEPROMRead:
                        UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "EEPROM reading session finished successfully.");
                        SCIBusBootstrapFinished = true;
                        SCIBusResponse = true;
                        break;
                    case Task.FinishEEPROMWrite:
                        UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "EEPROM writing session finished successfully.");
                        SCIBusBootstrapFinished = true;
                        SCIBusResponse = true;
                        UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "User is instructed to turn key to OFF position.");
                        MessageBox.Show("Turn key to OFF position." + Environment.NewLine + "Wait for 5 seconds before starting the engine.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;
                    default:
                        break;
                }

                SCIBusRxTimeout = false;
                SCIBusRxTimeoutTimer.Stop();

                if (CurrentTask == Task.EraseFlashMemory)
                {
                    SCIBusRxTimeoutTimer.Interval = 10000; // ms, erasing takes some time, wait more
                }
                else if (CurrentTask == Task.WriteEEPROM)
                {
                    SCIBusRxTimeoutTimer.Interval = 5000; // ms, EEPROM write takes some time, wait more
                }
                else
                {
                    SCIBusRxTimeoutTimer.Interval = 2000; // ms, return to original timeout
                }

                SCIBusRxTimeoutTimer.Start();
            }
        }

        private void SCIBusBootstrap_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled == true)
            {
                switch (CurrentTask)
                {
                    case Task.BackupFlashMemory:
                    case Task.ReadFlashMemory:
                    case Task.WriteFlashMemory:
                    case Task.BackupEEPROM:
                    case Task.UpdateEEPROM:
                    case Task.ReadEEPROM:
                    case Task.WriteEEPROM:
                        ExitButton_Click(this, EventArgs.Empty);
                        break;
                    default:
                        break;
                }

                UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "Current task is cancelled.");
            }

            SCIBusRxTimeout = false;
            SCIBusRxTimeoutTimer.Stop();
            SCIBusTxTimeout = false;
            SCIBusTxTimeoutTimer.Stop();
            SCIBusTxPayload = null;
            EEPROMStopButton.Enabled = true;
            EEPROMReadButton.Enabled = true;
            EEPROMWriteButton.Enabled = true;
            EEPROMBrowseButton.Enabled = true;
            FlashStopButton.Enabled = true;
            FlashReadButton.Enabled = true;
            FlashWriteButton.Enabled = true;
            FlashBrowseButton.Enabled = true;
            FlashChipDetectButton.Enabled = true;
            FlashChipComboBox.Enabled = true;
            ExitButton.Enabled = true;
            StartButton.Enabled = true;
            UploadButton.Enabled = true;
            WorkerFunctionComboBox.Enabled = true;
            BootstrapButton.Enabled = true;
            BootloaderComboBox.Enabled = true;
            WorkerFunctionComboBox.SelectedIndex = (byte)WorkerFunction.Empty;
            CurrentTask = Task.None;
        }

        private void BootstrapButton_Click(object sender, EventArgs e)
        {
            string LastPCMSpeed = OriginalForm.PCM.speed;

            UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "User is instructed to turn key to OFF position.");

            if (MessageBox.Show("Turn key to OFF position and wait at least 10 seconds." + Environment.NewLine + "Click OK when done.", "Information", MessageBoxButtons.OKCancel, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2) == DialogResult.OK)
            {
                if (OriginalForm.PCM.speed != "62500 baud")
                {
                    UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Scanner SCI-bus speed is set to 62500 baud.");
                    OriginalForm.SelectSCIBusPCMHSMode();
                }

                MainForm.Packet.tx.bus = (byte)Packet.Bus.usb;
                MainForm.Packet.tx.command = (byte)Packet.Command.settings;
                MainForm.Packet.tx.mode = (byte)Packet.SettingsMode.setProgVolt;
                MainForm.Packet.tx.payload = new byte[1] { 0x80 };
                MainForm.Packet.GeneratePacket();
                OriginalForm.TransmitUSBPacket("[<-TX] Apply VBB to SCI-RX pin:");

                UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "User is instructed to turn key to RUN position.");

                if (MessageBox.Show("Turn key to RUN position." + Environment.NewLine + "Click OK when done.", "Information", MessageBoxButtons.OKCancel, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2) == DialogResult.OK)
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
                    //MainForm.Packet.tx.payload = new byte[2] { 0x02, (byte)FlashChipComboBox.SelectedIndex }; // JTEC test
                    MainForm.Packet.GeneratePacket();
                    OriginalForm.TransmitUSBPacket("[<-TX] Init PCM bootstrap mode:");

                    switch ((byte)BootloaderComboBox.SelectedIndex)
                    {
                        case (byte)Bootloader.SBEC3_128k:
                            OriginalForm.UpdateUSBTextBox("[INFO] Bootloader: SBEC3 (128k).");
                            break;
                        case (byte)Bootloader.SBEC3_256k:
                            OriginalForm.UpdateUSBTextBox("[INFO] Bootloader: SBEC3 (256k).");
                            break;
                        case (byte)Bootloader.SBEC3_256k_custom:
                            OriginalForm.UpdateUSBTextBox("[INFO] Bootloader: SBEC3 custom (256k).");
                            break;
                        case (byte)Bootloader.JTEC_256k:
                            OriginalForm.UpdateUSBTextBox("[INFO] Bootloader: JTEC (256k).");
                            break;
                        case (byte)Bootloader.Empty:
                        default:
                            OriginalForm.UpdateUSBTextBox("[INFO] Bootloader: empty.");
                            break;
                    }

                    SwitchBackToLSWhenExit = true;
                }
                else
                {
                    MainForm.Packet.tx.bus = (byte)Packet.Bus.usb;
                    MainForm.Packet.tx.command = (byte)Packet.Command.settings;
                    MainForm.Packet.tx.mode = (byte)Packet.SettingsMode.setProgVolt;
                    MainForm.Packet.tx.payload = new byte[1] { 0x00 };
                    MainForm.Packet.GeneratePacket();
                    OriginalForm.TransmitUSBPacket("[<-TX] Remove VBB from SCI-RX pin:");

                    UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "ECU bootstrapping is cancelled.");

                    if (LastPCMSpeed != "62500 baud")
                    {
                        UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "Scanner SCI-bus speed is set to 7812.5 baud.");
                        OriginalForm.SelectSCIBusPCMLSMode();
                    }
                }
            }
            else
            {
                UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "ECU bootstrapping is cancelled.");

                if (LastPCMSpeed != "62500 baud")
                {
                    UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "Scanner SCI-bus speed is set to 7812.5 baud.");
                    OriginalForm.SelectSCIBusPCMLSMode();
                }
            }
        }

        private void UploadButton_Click(object sender, EventArgs e)
        {
            MainForm.Packet.tx.bus = (byte)Packet.Bus.usb;
            MainForm.Packet.tx.command = (byte)Packet.Command.debug;
            MainForm.Packet.tx.mode = (byte)Packet.DebugMode.uploadWorkerFunction;
            MainForm.Packet.tx.payload = new byte[2] { (byte)WorkerFunctionComboBox.SelectedIndex, (byte)FlashChipComboBox.SelectedIndex };
            MainForm.Packet.GeneratePacket();
            OriginalForm.TransmitUSBPacket("[<-TX] Upload worker function:");

            switch ((byte)WorkerFunctionComboBox.SelectedIndex)
            {
                case (byte)WorkerFunction.PartNumberRead:
                    OriginalForm.UpdateUSBTextBox("[INFO] Worker function: part number read.");
                    break;
                case (byte)WorkerFunction.FlashID:
                    OriginalForm.UpdateUSBTextBox("[INFO] Worker function: flash ID.");
                    break;
                case (byte)WorkerFunction.FlashRead:
                    OriginalForm.UpdateUSBTextBox("[INFO] Worker function: flash read.");
                    break;
                case (byte)WorkerFunction.FlashErase:
                    OriginalForm.UpdateUSBTextBox("[INFO] Worker function: flash erase.");
                    break;
                case (byte)WorkerFunction.FlashWrite:
                    OriginalForm.UpdateUSBTextBox("[INFO] Worker function: flash write.");
                    break;
                case (byte)WorkerFunction.VerifyFlashChecksum:
                    OriginalForm.UpdateUSBTextBox("[INFO] Worker function: verify flash checksum.");
                    break;
                case (byte)WorkerFunction.EEPROMReadSPI:
                    OriginalForm.UpdateUSBTextBox("[INFO] Worker function: EEPROM read (SPI).");
                    break;
                case (byte)WorkerFunction.EEPROMWriteSPI:
                    OriginalForm.UpdateUSBTextBox("[INFO] Worker function: EEPROM write (SPI).");
                    break;
                case (byte)WorkerFunction.EEPROMReadParallel:
                    OriginalForm.UpdateUSBTextBox("[INFO] Worker function: EEPROM read (Parallel).");
                    break;
                case (byte)WorkerFunction.EEPROMWriteParallel:
                    OriginalForm.UpdateUSBTextBox("[INFO] Worker function: EEPROM write (Parallel).");
                    break;
                case (byte)WorkerFunction.Empty:
                default:
                    OriginalForm.UpdateUSBTextBox("[INFO] Worker function: empty.");
                    break;
            }
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            MainForm.Packet.tx.bus = (byte)Packet.Bus.usb;
            MainForm.Packet.tx.command = (byte)Packet.Command.debug;
            MainForm.Packet.tx.mode = (byte)Packet.DebugMode.startWorkerFunction;
            MainForm.Packet.tx.payload = new byte[2] { (byte)WorkerFunctionComboBox.SelectedIndex, (byte)FlashChipComboBox.SelectedIndex };
            MainForm.Packet.GeneratePacket();
            OriginalForm.TransmitUSBPacket("[<-TX] Start worker function:");
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            MainForm.Packet.tx.bus = (byte)Packet.Bus.usb;
            MainForm.Packet.tx.command = (byte)Packet.Command.debug;
            MainForm.Packet.tx.mode = (byte)Packet.DebugMode.exitWorkerFunction;
            MainForm.Packet.tx.payload = new byte[2] { (byte)WorkerFunctionComboBox.SelectedIndex, (byte)FlashChipComboBox.SelectedIndex };
            MainForm.Packet.GeneratePacket();
            OriginalForm.TransmitUSBPacket("[<-TX] Exit worker function:");
        }

        private void FlashChipDetectButton_Click(object sender, EventArgs e)
        {
            WorkerFunctionComboBox.SelectedIndex = (byte)WorkerFunction.FlashID;
            UploadButton_Click(this, EventArgs.Empty);
            StartButton_Click(this, EventArgs.Empty);
        }

        private void FlashBrowseButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog OpenFlashFileDialog = new OpenFileDialog())
            {
                OpenFlashFileDialog.InitialDirectory = Path.Combine(Application.StartupPath, @"ROMs\PCM");
                OpenFlashFileDialog.Filter = "Binary files (*.bin)|*.bin|All files (*.*)|*.*";
                OpenFlashFileDialog.FilterIndex = 2;
                OpenFlashFileDialog.RestoreDirectory = false;

                if (OpenFlashFileDialog.ShowDialog() == DialogResult.OK)
                {
                    using (var FileStream = File.Open(OpenFlashFileDialog.FileName, FileMode.Open))
                    {
                        using (var MemoryStream = new MemoryStream())
                        {
                            FileStream.CopyTo(MemoryStream);
                            FlashFileBuffer = MemoryStream.ToArray();

                            if (Path.GetFileName(OpenFlashFileDialog.FileName).Length > 29)
                            {
                                FlashFileNameLabel.Text = Path.GetFileName(OpenFlashFileDialog.FileName).Remove(29);
                            }
                            else
                            {
                                FlashFileNameLabel.Text = Path.GetFileName(OpenFlashFileDialog.FileName);
                            }

                            UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "Flash file is loaded.");
                            UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Name: " + Path.GetFileName(OpenFlashFileDialog.FileName));
                            UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Size: " + FlashFileBuffer.Length + " bytes = " + ((double)FlashFileBuffer.Length / 1024.0).ToString("0.00") + " kilobytes.");
                        }
                    }
                }
            }
        }

        private void FlashWriteButton_Click(object sender, EventArgs e)
        {
            WorkerFunctionComboBox.SelectedIndex = 0;

            UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "Start flash memory writing session.");

            if (FlashFileBuffer == null)
            {
                FlashBrowseButton_Click(this, EventArgs.Empty);
            }

            if (FlashFileBuffer == null)
            {
                UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "Flash memory writing is cancelled.");
                return;
            }

            if (!SCIBusBootstrapWorker.IsBusy)
            {
                CurrentTask = Task.CheckVoltages;
                //CurrentTask = Task.VerifyFlashChecksum; // debug, skip time consuming stuff
                SCIBusBootstrapFinished = false;
                SCIBusNextRequest = true;
                EEPROMStopButton.Enabled = false;
                EEPROMReadButton.Enabled = false;
                EEPROMWriteButton.Enabled = false;
                EEPROMBrowseButton.Enabled = false;
                FlashReadButton.Enabled = false;
                FlashWriteButton.Enabled = false;
                FlashBrowseButton.Enabled = false;
                FlashChipDetectButton.Enabled = false;
                FlashChipComboBox.Enabled = false;
                ExitButton.Enabled = false;
                StartButton.Enabled = false;
                UploadButton.Enabled = false;
                WorkerFunctionComboBox.Enabled = false;
                BootstrapButton.Enabled = false;
                BootloaderComboBox.Enabled = false;
                SCIBusCurrentMemoryOffset = 0;
                SCIBusBootstrapToolsProgressLabel.Text = "Progress: " + (byte)(Math.Round((double)SCIBusCurrentMemoryOffset / (double)FlashChipSize * 100.0)) + "% (" + SCIBusCurrentMemoryOffset.ToString() + "/" + FlashChipSize.ToString() + " bytes)";
                SCIBusBootstrapWorker.RunWorkerAsync();
            }
        }

        private void FlashReadButton_Click(object sender, EventArgs e)
        {
            WorkerFunctionComboBox.SelectedIndex = 0;
            
            if (FlashChipComboBox.SelectedIndex == 0)
            {
                MessageBox.Show("Detect flash memory chip first.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "Start flash memory reading session.");

            using (SaveFileDialog SaveFlashFileDialog = new SaveFileDialog())
            {
                SaveFlashFileDialog.InitialDirectory = Path.Combine(Application.StartupPath, @"ROMs\PCM");
                SaveFlashFileDialog.Filter = "Binary files (*.bin)|*.bin|All files (*.*)|*.*";
                SaveFlashFileDialog.FilterIndex = 2;
                SaveFlashFileDialog.RestoreDirectory = false;

                if (SaveFlashFileDialog.ShowDialog() == DialogResult.OK)
                {
                    SCIBusFlashReadFilename = SaveFlashFileDialog.FileName;

                    if (File.Exists(SaveFlashFileDialog.FileName)) File.Delete(SaveFlashFileDialog.FileName);

                    if (Path.GetFileName(SaveFlashFileDialog.FileName).Length > 29)
                    {
                        FlashFileNameLabel.Text = Path.GetFileName(SaveFlashFileDialog.FileName).Remove(29);
                    }
                    else
                    {
                        FlashFileNameLabel.Text = Path.GetFileName(SaveFlashFileDialog.FileName);
                    }

                    CurrentTask = Task.ReadFlashMemory;
                    SCIBusBootstrapFinished = false;
                    SCIBusNextRequest = true;
                    EEPROMStopButton.Enabled = false;
                    EEPROMReadButton.Enabled = false;
                    EEPROMWriteButton.Enabled = false;
                    EEPROMBrowseButton.Enabled = false;
                    FlashReadButton.Enabled = false;
                    FlashWriteButton.Enabled = false;
                    FlashBrowseButton.Enabled = false;
                    FlashChipDetectButton.Enabled = false;
                    FlashChipComboBox.Enabled = false;
                    ExitButton.Enabled = false;
                    StartButton.Enabled = false;
                    UploadButton.Enabled = false;
                    WorkerFunctionComboBox.Enabled = false;
                    BootstrapButton.Enabled = false;
                    BootloaderComboBox.Enabled = false;
                    SCIBusCurrentMemoryOffset = 0;
                    SCIBusBootstrapToolsProgressLabel.Text = "Progress: " + (byte)(Math.Round((double)SCIBusCurrentMemoryOffset / (double)FlashChipSize * 100.0)) + "% (" + SCIBusCurrentMemoryOffset.ToString() + "/" + FlashChipSize.ToString() + " bytes)";
                    SCIBusBootstrapWorker.RunWorkerAsync();
                }
                else
                {
                    UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "Flash memory reading is cancelled.");
                }
            }
        }

        private void FlashStopButton_Click(object sender, EventArgs e)
        {
            if (SCIBusBootstrapWorker.IsBusy && SCIBusBootstrapWorker.WorkerSupportsCancellation)
            {
                SCIBusBootstrapWorker.CancelAsync();
            }
        }

        private void FlashMemoryBackupCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!FlashMemoryBackupCheckBox.Checked)
            {
                if (MessageBox.Show("Skipping flash memory backup could lead to data loss." + Environment.NewLine + "Do you really want to continue without backup?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.No)
                {
                    FlashMemoryBackupCheckBox.Checked = true;
                }
            }
        }

        private void EEPROMBrowseButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog OpenEEPROMFileDialog = new OpenFileDialog())
            {
                OpenEEPROMFileDialog.InitialDirectory = Path.Combine(Application.StartupPath, @"ROMs\PCM");
                OpenEEPROMFileDialog.Filter = "Binary files (*.bin)|*.bin|All files (*.*)|*.*";
                OpenEEPROMFileDialog.FilterIndex = 2;
                OpenEEPROMFileDialog.RestoreDirectory = false;

                if (OpenEEPROMFileDialog.ShowDialog() == DialogResult.OK)
                {
                    using (var FileStream = File.Open(OpenEEPROMFileDialog.FileName, FileMode.Open))
                    {
                        using (var MemoryStream = new MemoryStream())
                        {
                            FileStream.CopyTo(MemoryStream);
                            EEPROMFileBuffer = MemoryStream.ToArray();

                            if (EEPROMFileBuffer.Length == 512)
                            {
                                if (Path.GetFileName(OpenEEPROMFileDialog.FileName).Length > 29)
                                {
                                    EEPROMFileNameLabel.Text = Path.GetFileName(OpenEEPROMFileDialog.FileName).Remove(29);
                                }
                                else
                                {
                                    EEPROMFileNameLabel.Text = Path.GetFileName(OpenEEPROMFileDialog.FileName);
                                }

                                UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "EEPROM file is loaded.");
                                UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Name: " + Path.GetFileName(OpenEEPROMFileDialog.FileName));
                                UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Size: " + EEPROMFileBuffer.Length + " bytes = " + ((double)EEPROMFileBuffer.Length / 1024.0).ToString("0.00") + " kilobytes.");
                            }
                            else
                            {
                                UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "Invalid EEPROM file size (" + EEPROMFileBuffer.Length + " bytes)." + Environment.NewLine + "Valid EEPROM size is 512 bytes.");
                            }
                        }
                    }
                }
            }
        }

        private void EEPROMWriteButton_Click(object sender, EventArgs e)
        {
            if (BootloaderComboBox.SelectedIndex == (byte)Bootloader.JTEC_256k)
            {
                MessageBox.Show("JTEC EEPROM writing is not supported yet.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            WorkerFunctionComboBox.SelectedIndex = 0;

            UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "Start EEPROM writing session.");

            if (EEPROMFileBuffer == null)
            {
                EEPROMBrowseButton_Click(this, EventArgs.Empty);
            }

            if (EEPROMFileBuffer == null)
            {
                UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "EEPROM writing is cancelled.");
                return;
            }

            if (!SCIBusBootstrapWorker.IsBusy && (EEPROMFileBuffer.Length == 512))
            {
                CurrentTask = Task.WriteEEPROM;
                SCIBusBootstrapFinished = false;
                SCIBusNextRequest = true;
                EEPROMReadButton.Enabled = false;
                EEPROMWriteButton.Enabled = false;
                EEPROMBrowseButton.Enabled = false;
                FlashStopButton.Enabled = false;
                FlashReadButton.Enabled = false;
                FlashWriteButton.Enabled = false;
                FlashBrowseButton.Enabled = false;
                FlashChipDetectButton.Enabled = false;
                FlashChipComboBox.Enabled = false;
                ExitButton.Enabled = false;
                StartButton.Enabled = false;
                UploadButton.Enabled = false;
                WorkerFunctionComboBox.Enabled = false;
                BootstrapButton.Enabled = false;
                BootloaderComboBox.Enabled = false;
                SCIBusCurrentMemoryOffset = 0;
                SCIBusBootstrapToolsProgressLabel.Text = "Progress: " + (byte)(Math.Round((double)SCIBusCurrentMemoryOffset / 512.0 * 100.0)) + "% (" + SCIBusCurrentMemoryOffset.ToString() + "/512 bytes)";
                SCIBusBootstrapWorker.RunWorkerAsync();
            }
        }

        private void EEPROMReadButton_Click(object sender, EventArgs e)
        {
            if (BootloaderComboBox.SelectedIndex == (byte)Bootloader.JTEC_256k)
            {
                MessageBox.Show("JTEC EEPROM reading is not supported yet.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            WorkerFunctionComboBox.SelectedIndex = 0;

            UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "Start EEPROM reading session.");

            using (SaveFileDialog SaveEEPROMFileDialog = new SaveFileDialog())
            {
                SaveEEPROMFileDialog.InitialDirectory = Path.Combine(Application.StartupPath, @"ROMs\PCM");
                SaveEEPROMFileDialog.Filter = "Binary files (*.bin)|*.bin|All files (*.*)|*.*";
                SaveEEPROMFileDialog.FilterIndex = 2;
                SaveEEPROMFileDialog.RestoreDirectory = false;

                if (SaveEEPROMFileDialog.ShowDialog() == DialogResult.OK)
                {
                    SCIBusEEPROMReadFilename = SaveEEPROMFileDialog.FileName;

                    if (File.Exists(SaveEEPROMFileDialog.FileName)) File.Delete(SaveEEPROMFileDialog.FileName);

                    if (Path.GetFileName(SaveEEPROMFileDialog.FileName).Length > 29)
                    {
                        EEPROMFileNameLabel.Text = Path.GetFileName(SaveEEPROMFileDialog.FileName).Remove(29);
                    }
                    else
                    {
                        EEPROMFileNameLabel.Text = Path.GetFileName(SaveEEPROMFileDialog.FileName);
                    }

                    CurrentTask = Task.ReadEEPROM;
                    SCIBusBootstrapFinished = false;
                    SCIBusNextRequest = true;
                    EEPROMReadButton.Enabled = false;
                    EEPROMWriteButton.Enabled = false;
                    EEPROMBrowseButton.Enabled = false;
                    FlashStopButton.Enabled = false;
                    FlashReadButton.Enabled = false;
                    FlashWriteButton.Enabled = false;
                    FlashBrowseButton.Enabled = false;
                    FlashChipDetectButton.Enabled = false;
                    FlashChipComboBox.Enabled = false;
                    ExitButton.Enabled = false;
                    StartButton.Enabled = false;
                    UploadButton.Enabled = false;
                    WorkerFunctionComboBox.Enabled = false;
                    BootstrapButton.Enabled = false;
                    BootloaderComboBox.Enabled = false;
                    SCIBusCurrentMemoryOffset = 0;
                    SCIBusBootstrapToolsProgressLabel.Text = "Progress: " + (byte)(Math.Round((double)SCIBusCurrentMemoryOffset / 512.0 * 100.0)) + "% (" + SCIBusCurrentMemoryOffset.ToString() + "/512 bytes)";
                    SCIBusBootstrapWorker.RunWorkerAsync();
                }
                else
                {
                    UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "EEPROM reading is cancelled.");
                }
            }
        }

        private void EEPROMStopButton_Click(object sender, EventArgs e)
        {
            if (SCIBusBootstrapWorker.IsBusy && SCIBusBootstrapWorker.WorkerSupportsCancellation)
            {
                SCIBusBootstrapWorker.CancelAsync();
            }
        }

        private void EEPROMBackupCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!EEPROMBackupCheckBox.Checked)
            {
                if (MessageBox.Show("Skipping EEPROM backup could lead to data loss." + Environment.NewLine + "Do you really want to continue without backup?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.No)
                {
                    EEPROMBackupCheckBox.Checked = true;
                }
            }
        }

        private void WorkerFunctionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (WorkerFunctionComboBox.SelectedIndex)
            {
                case (byte)WorkerFunction.Empty:
                case (byte)WorkerFunction.PartNumberRead:
                case (byte)WorkerFunction.FlashID:
                case (byte)WorkerFunction.FlashErase:
                case (byte)WorkerFunction.VerifyFlashChecksum:
                    ExitButton.Enabled = false;
                    break;
                case (byte)WorkerFunction.FlashRead:
                case (byte)WorkerFunction.FlashWrite:
                case (byte)WorkerFunction.EEPROMReadSPI:
                case (byte)WorkerFunction.EEPROMWriteSPI:
                    if (!SCIBusBootstrapWorker.IsBusy) ExitButton.Enabled = true;
                    break;
            }
        }

        private void PacketReceivedHandler(object sender, EventArgs e)
        {
            switch (MainForm.Packet.rx.bus)
            {
                case (byte)Packet.Bus.usb:
                    switch (MainForm.Packet.rx.command)
                    {
                        case (byte)Packet.Command.settings:
                            switch (MainForm.Packet.rx.mode)
                            {
                                case (byte)Packet.SettingsMode.setProgVolt:
                                    if (MainForm.Packet.rx.payload.Length == 0) break;

                                    if (MainForm.Packet.rx.payload[0] == 0)
                                    {
                                        UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "VBB/VPP removed from SCI-RX pin.");

                                        if (SCIBusBootstrapWorker.IsBusy)
                                        {
                                            switch (CurrentTask)
                                            {
                                                case Task.DetectFlashMemoryType:
                                                    if (FlashMemoryBackupCheckBox.Checked)
                                                    {
                                                        CurrentTask = Task.BackupFlashMemory;
                                                        SCIBusCurrentMemoryOffset = 0;
                                                        SCIBusBootstrapToolsProgressLabel.Text = "Progress: " + (byte)(Math.Round((double)SCIBusCurrentMemoryOffset / (double)FlashChipSize * 100.0)) + "% (" + SCIBusCurrentMemoryOffset.ToString() + "/" + FlashChipSize.ToString() + " bytes)";
                                                        SCIBusTxPayload = new byte[6];
                                                        SCIBusTxPayload[0] = 0x33;
                                                        SCIBusTxPayload[1] = (byte)((SCIBusCurrentMemoryOffset >> 16) & 0xFF);
                                                        SCIBusTxPayload[2] = (byte)((SCIBusCurrentMemoryOffset >> 8) & 0xFF);
                                                        SCIBusTxPayload[3] = (byte)(SCIBusCurrentMemoryOffset & 0xFF);
                                                        SCIBusTxPayload[4] = (byte)((FlashReadBlockSize >> 8) & 0xFF);
                                                        SCIBusTxPayload[5] = (byte)(FlashReadBlockSize & 0xFF);
                                                    }
                                                    else
                                                    {
                                                        UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "Step 4. Backup flash memory." + Environment.NewLine + Environment.NewLine + "Skip flash memory backup.");

                                                        if (EEPROMBackupCheckBox.Checked)
                                                        {
                                                            switch (BootloaderComboBox.SelectedIndex)
                                                            {
                                                                case (byte)Bootloader.JTEC_256k:
                                                                    UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "Step 5. Backup EEPROM." + Environment.NewLine + Environment.NewLine + "Skip EEPROM backup.");
                                                                    CurrentTask = Task.EraseFlashMemory;
                                                                    break;
                                                                default:
                                                                    CurrentTask = Task.BackupEEPROM;
                                                                    break;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "Step 5. Backup EEPROM." + Environment.NewLine + Environment.NewLine + "Skip EEPROM backup.");

                                                            CurrentTask = Task.EraseFlashMemory;
                                                        }
                                                    }
                                                    SCIBusResponse = true;
                                                    break;
                                                case Task.EraseFlashMemory:
                                                    CurrentTask = Task.WriteFlashMemory;
                                                    SCIBusCurrentMemoryOffset = 0;
                                                    SCIBusBootstrapToolsProgressLabel.Text = "Progress: " + (byte)(Math.Round((double)SCIBusCurrentMemoryOffset / (double)FlashChipSize * 100.0)) + "% (" + SCIBusCurrentMemoryOffset.ToString() + "/" + FlashChipSize.ToString() + " bytes)";
                                                    SCIBusTxPayload = new byte[6 + FlashWriteBlockSize];
                                                    SCIBusTxPayload[0] = 0x30;
                                                    SCIBusTxPayload[1] = (byte)((SCIBusCurrentMemoryOffset >> 16) & 0xFF);
                                                    SCIBusTxPayload[2] = (byte)((SCIBusCurrentMemoryOffset >> 8) & 0xFF);
                                                    SCIBusTxPayload[3] = (byte)(SCIBusCurrentMemoryOffset & 0xFF);
                                                    SCIBusTxPayload[4] = (byte)((FlashWriteBlockSize >> 8) & 0xFF);
                                                    SCIBusTxPayload[5] = (byte)(FlashWriteBlockSize & 0xFF);
                                                    Array.Copy(FlashFileBuffer, SCIBusCurrentMemoryOffset, SCIBusTxPayload, 6, FlashWriteBlockSize);
                                                    SCIBusResponse = true;
                                                    break;
                                                default:
                                                    break;
                                            }
                                        }
                                    }
                                    else if (Util.IsBitSet(MainForm.Packet.rx.payload[0], 7))
                                    {
                                        UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Apply VBB (12V) to SCI-RX pin.");
                                    }
                                    else if (Util.IsBitSet(MainForm.Packet.rx.payload[0], 6))
                                    {
                                        UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Apply VPP (20V) to SCI-RX pin.");
                                    }
                                    else
                                    {
                                        // TODO
                                    }
                                    break;
                            }
                            break;
                        case (byte)Packet.Command.response:
                            switch (MainForm.Packet.rx.mode)
                            {
                                case (byte)Packet.ResponseMode.AllVolts:
                                    double BatteryVoltage = ((MainForm.Packet.rx.payload[0] << 8) + MainForm.Packet.rx.payload[1]) / 1000.00;
                                    double BootstrapVoltage = ((MainForm.Packet.rx.payload[2] << 8) + MainForm.Packet.rx.payload[3]) / 1000.00;
                                    double ProgrammingVoltage = ((MainForm.Packet.rx.payload[4] << 8) + MainForm.Packet.rx.payload[5]) / 1000.00;
                                    string BatteryVoltageString = BatteryVoltage.ToString("0.000").Replace(",", ".") + " V";
                                    string BootstrapVoltageString = BootstrapVoltage.ToString("0.000").Replace(",", ".") + " V";
                                    string ProgrammingVoltageString = ProgrammingVoltage.ToString("0.000").Replace(",", ".") + " V";

                                    UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "Battery voltage: " + BatteryVoltageString + Environment.NewLine + "Bootstrap voltage: " + BootstrapVoltageString + Environment.NewLine + "Programming voltage: " + ProgrammingVoltageString);

                                    if ((BatteryVoltage >= MinBattVolts) && (BootstrapVoltage >= MinBootVolts) && (ProgrammingVoltage >= MinProgVolts))
                                    {
                                        UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "All voltages are nominal.");

                                        if (SCIBusBootstrapWorker.IsBusy)
                                        {
                                            switch (BootloaderComboBox.SelectedIndex)
                                            {
                                                case (byte)Bootloader.JTEC_256k:
                                                    UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "Step 2. Read part number." + Environment.NewLine + Environment.NewLine + "Skip part number read.");
                                                    CurrentTask = Task.DetectFlashMemoryType;
                                                    break;
                                                default:
                                                    CurrentTask = Task.ReadPartNumber;
                                                    break;
                                            }

                                            SCIBusResponse = true;
                                        }
                                    }
                                    else
                                    {
                                        if (BatteryVoltage < MinBattVolts)
                                        {
                                            UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Battery voltage must be above " + MinBattVolts.ToString("0.0").Replace(",", ".") + "V.");
                                        }
                                        if (BootstrapVoltage < MinBootVolts)
                                        {
                                            UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Bootstrap voltage must be above " + MinBootVolts.ToString("0.0").Replace(",", ".") + "V.");
                                        }
                                        if (ProgrammingVoltage < MinProgVolts)
                                        {
                                            UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Programming voltage must be above " + MinProgVolts.ToString("0.0").Replace(",", ".") + "V.");
                                        }

                                        if (SCIBusBootstrapWorker.IsBusy)
                                        {
                                            SCIBusBootstrapWorker.CancelAsync();
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case (byte)Packet.Command.debug:
                            switch (MainForm.Packet.rx.mode)
                            {
                                case (byte)Packet.DebugMode.initBootstrapMode:
                                    switch (MainForm.Packet.rx.payload[0])
                                    {
                                        case (byte)BootloaderError.OK:
                                            UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Bootstrap mode initialized successfully.");
                                            break;
                                        case (byte)BootloaderError.NoResponseToMagicByte:
                                            UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Bootstrap status: no response to magic byte.");
                                            break;
                                        case (byte)BootloaderError.UnexpectedResponseToMagicByte:
                                            UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Bootstrap status: unexpected response to magic byte.");
                                            break;
                                        case (byte)BootloaderError.SecuritySeedResponseTimeout:
                                            UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Bootstrap status: security seed response timeout.");
                                            break;
                                        case (byte)BootloaderError.SecuritySeedChecksumError:
                                            UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Bootstrap status: security seed checksum error.");
                                            break;
                                        case (byte)BootloaderError.SecurityKeyStatusTimeout:
                                            UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Bootstrap status: security key status timeout.");
                                            break;
                                        case (byte)BootloaderError.SecurityKeyNotAccepted:
                                            UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Bootstrap status: security key not accepted.");
                                            break;
                                        case (byte)BootloaderError.StartBootloaderTimeout:
                                            UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Bootstrap status: start bootloader timeout.");
                                            break;
                                        case (byte)BootloaderError.UnexpectedBootloaderStatusByte:
                                            UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Bootstrap status: unexpected bootloader status byte.");
                                            break;
                                        default:
                                            UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Bootstrap status: unknown.");
                                            break;
                                    }
                                    break;
                                case (byte)Packet.DebugMode.uploadWorkerFunction:
                                    switch (MainForm.Packet.rx.payload[0])
                                    {
                                        case (byte)WorkerFunctionError.OK:
                                            UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Worker function uploaded successfully.");

                                            if (SCIBusBootstrapWorker.IsBusy)
                                            {
                                                switch (CurrentTask)
                                                {
                                                    case Task.ReadPartNumber:
                                                    case Task.DetectFlashMemoryType:
                                                    case Task.BackupFlashMemory:
                                                    case Task.ReadFlashMemory:
                                                    case Task.BackupEEPROM:
                                                    case Task.EraseFlashMemory:
                                                    case Task.WriteFlashMemory:
                                                    case Task.VerifyFlashChecksum:
                                                    case Task.UpdateEEPROM:
                                                    case Task.ReadEEPROM:
                                                    case Task.WriteEEPROM:
                                                        StartButton_Click(this, EventArgs.Empty);
                                                        break;
                                                    default:
                                                        break;
                                                }
                                            }
                                            break;
                                        case (byte)WorkerFunctionError.NoResponseToPing:
                                            UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Worker function status: no response to ping.");

                                            if (SCIBusBootstrapWorker.IsBusy)
                                            {
                                                SCIBusBootstrapWorker.CancelAsync();
                                            }
                                            break;
                                        case (byte)WorkerFunctionError.UploadInterrupted:
                                            UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Worker function status: upload interrupted.");

                                            if (SCIBusBootstrapWorker.IsBusy)
                                            {
                                                SCIBusBootstrapWorker.CancelAsync();
                                            }
                                            break;
                                        case (byte)WorkerFunctionError.UnexpectedUploadResult:
                                            UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Worker function status: unexptected upload result.");

                                            if (SCIBusBootstrapWorker.IsBusy)
                                            {
                                                SCIBusBootstrapWorker.CancelAsync();
                                            }
                                            break;
                                    }
                                    break;
                            }
                            break;
                        default:
                            break;
                    }
                    break;
                case (byte)Packet.Bus.pcm:
                case (byte)Packet.Bus.tcm:
                    byte[] SCIBusResponseBytes = MainForm.Packet.rx.payload.Skip(4).ToArray(); // skip 4 timestamp bytes

                    if (SCIBusResponseBytes.Length > 0)
                    {
                        switch (SCIBusResponseBytes[0])
                        {
                            case (byte)SCI_ID.BootstrapBaudrateSet:
                                UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Set bootstrap baudrate to 62500 baud. OK.");
                                break;
                            case (byte)SCI_ID.UploadWorkerFunctionResult:
                                if (SCIBusResponseBytes.Length > 1)
                                {
                                    UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "Upload worker function: ");

                                    switch ((byte)WorkerFunctionComboBox.SelectedIndex)
                                    {
                                        case (byte)WorkerFunction.PartNumberRead:
                                            UpdateTextBox(SCIBusBootstrapInfoTextBox, "part number read.");
                                            break;
                                        case (byte)WorkerFunction.FlashID:
                                            UpdateTextBox(SCIBusBootstrapInfoTextBox, "flash ID.");
                                            break;
                                        case (byte)WorkerFunction.FlashRead:
                                            UpdateTextBox(SCIBusBootstrapInfoTextBox, "flash read.");
                                            break;
                                        case (byte)WorkerFunction.FlashErase:
                                            UpdateTextBox(SCIBusBootstrapInfoTextBox, "flash erase.");
                                            UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Erase algorithm used: " + FlashChipComboBox.Items[(byte)FlashChipComboBox.SelectedIndex].ToString() + ".");
                                            break;
                                        case (byte)WorkerFunction.FlashWrite:
                                            UpdateTextBox(SCIBusBootstrapInfoTextBox, "flash write.");
                                            UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Write algorithm used: " + FlashChipComboBox.Items[(byte)FlashChipComboBox.SelectedIndex].ToString() + ".");
                                            break;
                                        case (byte)WorkerFunction.VerifyFlashChecksum:
                                            UpdateTextBox(SCIBusBootstrapInfoTextBox, "verify flash checksum.");
                                            break;
                                        case (byte)WorkerFunction.EEPROMReadSPI:
                                            UpdateTextBox(SCIBusBootstrapInfoTextBox, "EEPROM read (SPI).");
                                            break;
                                        case (byte)WorkerFunction.EEPROMWriteSPI:
                                            UpdateTextBox(SCIBusBootstrapInfoTextBox, "EEPROM write (SPI).");
                                            break;
                                        case (byte)WorkerFunction.EEPROMReadParallel:
                                            UpdateTextBox(SCIBusBootstrapInfoTextBox, "EEPROM read (Parallel).");
                                            break;
                                        case (byte)WorkerFunction.EEPROMWriteParallel:
                                            UpdateTextBox(SCIBusBootstrapInfoTextBox, "EEPROM write (Parallel).");
                                            break;
                                        case (byte)WorkerFunction.Empty:
                                        default:
                                            UpdateTextBox(SCIBusBootstrapInfoTextBox, "empty.");
                                            break;
                                    }
                                }
                                break;
                            case (byte)SCI_ID.StartWorkerFunction:
                                switch ((byte)WorkerFunctionComboBox.SelectedIndex)
                                {
                                    case (byte)WorkerFunction.PartNumberRead:
                                        if (SCIBusResponseBytes.Length >= 30)
                                        {
                                            if (SCIBusResponseBytes[1] != 0xFF)
                                            {
                                                PartNumberString = Util.ByteToHexString(SCIBusResponseBytes, 1, 4).Replace(" ", "");

                                                if ((SCIBusResponseBytes[5] >= 0x41) && (SCIBusResponseBytes[5] <= 0x5A) && (SCIBusResponseBytes[6] >= 0x41) && (SCIBusResponseBytes[6] <= 0x5A))
                                                {
                                                    PartNumberString += Encoding.ASCII.GetString(SCIBusResponseBytes, 5, 2);
                                                }
                                                else // no revision label available, append 99 by default
                                                {
                                                    PartNumberString += "99";
                                                }
                                            }
                                            else if (SCIBusResponseBytes[21] != 0xFF)
                                            {
                                                PartNumberString = Util.ByteToHexString(SCIBusResponseBytes, 21, 4).Replace(" ", "");

                                                if ((SCIBusResponseBytes[25] >= 0x41) && (SCIBusResponseBytes[25] <= 0x5A) && (SCIBusResponseBytes[26] >= 0x41) && (SCIBusResponseBytes[26] <= 0x5A))
                                                {
                                                    PartNumberString += Encoding.ASCII.GetString(SCIBusResponseBytes, 25, 2);
                                                }
                                                else // no revision label available, append 99 by default
                                                {
                                                    PartNumberString += "99";
                                                }
                                            }
                                            else
                                            {
                                                PartNumberString = string.Empty;
                                            }

                                            if (PartNumberString != string.Empty)
                                            {
                                                UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Part number: " + PartNumberString);
                                            }
                                            else
                                            {
                                                UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Part number: unknown.");
                                            }
                                        }
                                        else
                                        {
                                            UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Part number: unknown.");
                                        }

                                        if (SCIBusBootstrapWorker.IsBusy)
                                        {
                                            // TODO: verify part number

                                            CurrentTask = Task.DetectFlashMemoryType;
                                            SCIBusResponse = true;
                                        }
                                        break;
                                    case (byte)WorkerFunction.FlashID:
                                        if (SCIBusResponseBytes.Length >= 3)
                                        {
                                            bool ManufacturerKnown = true;
                                            bool ChipTypeKnown = true;

                                            switch (SCIBusResponseBytes[1])
                                            {
                                                case (byte)FlashMemoryManufacturer.STMicroelectronics:
                                                    UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Flash memory: ST ");
                                                    break;
                                                case (byte)FlashMemoryManufacturer.CATALYST:
                                                    UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Flash memory: CATALYST ");
                                                    break;
                                                case (byte)FlashMemoryManufacturer.Intel:
                                                    UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Flash memory: Intel ");
                                                    break;
                                                case (byte)FlashMemoryManufacturer.TexasInstruments:
                                                    UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Flash memory: Texas Instruments ");
                                                    break;
                                                default:
                                                    UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Flash memory: unknown ");
                                                    ManufacturerKnown = false;
                                                    break;
                                            }

                                            if (ManufacturerKnown)
                                            {
                                                switch (SCIBusResponseBytes[2])
                                                {
                                                    case (byte)FlashMemoryType.M28F102:
                                                        UpdateTextBox(SCIBusBootstrapInfoTextBox, "M28F102 (128 kB).");
                                                        FlashChipComboBox.SelectedIndex = 1;
                                                        FlashChipSize = 131072; // bytes
                                                        break;
                                                    case (byte)FlashMemoryType.CAT28F102:
                                                        UpdateTextBox(SCIBusBootstrapInfoTextBox, "CAT28F102 (128 kB).");
                                                        FlashChipComboBox.SelectedIndex = 2;
                                                        FlashChipSize = 131072; // bytes
                                                        break;
                                                    case (byte)FlashMemoryType.N28F010:
                                                        UpdateTextBox(SCIBusBootstrapInfoTextBox, "N28F010 (128 kB).");
                                                        FlashChipComboBox.SelectedIndex = 3;
                                                        FlashChipSize = 131072; // bytes
                                                        break;
                                                    case (byte)FlashMemoryType.N28F020:
                                                        UpdateTextBox(SCIBusBootstrapInfoTextBox, "N28F020 (256 kB).");
                                                        FlashChipComboBox.SelectedIndex = 4;
                                                        FlashChipSize = 262144; // bytes
                                                        break;
                                                    case (byte)FlashMemoryType.M28F210:
                                                        UpdateTextBox(SCIBusBootstrapInfoTextBox, "M28F210 (256 kB).");
                                                        FlashChipComboBox.SelectedIndex = 5;
                                                        FlashChipSize = 262144; // bytes
                                                        break;
                                                    case (byte)FlashMemoryType.M28F220:
                                                        UpdateTextBox(SCIBusBootstrapInfoTextBox, "M28F220 (256 kB).");
                                                        FlashChipComboBox.SelectedIndex = 6;
                                                        FlashChipSize = 262144; // bytes
                                                        break;
                                                    case (byte)FlashMemoryType.M28F200T:
                                                    case (byte)FlashMemoryType.M28F200B:
                                                        UpdateTextBox(SCIBusBootstrapInfoTextBox, "M28F200 (256 kB).");
                                                        FlashChipComboBox.SelectedIndex = 7;
                                                        FlashChipSize = 262144; // bytes
                                                        break;
                                                    case (byte)FlashMemoryType.TMS28F210:
                                                        UpdateTextBox(SCIBusBootstrapInfoTextBox, "TMS28F210 (256 kB).");
                                                        FlashChipComboBox.SelectedIndex = 8;
                                                        FlashChipSize = 262144; // bytes
                                                        break;
                                                    default:
                                                        UpdateTextBox(SCIBusBootstrapInfoTextBox, "(" + Util.ByteToHexString(SCIBusResponseBytes, 1, 2) + ").");
                                                        FlashChipComboBox.SelectedIndex = 0;
                                                        FlashChipSize = 0; // bytes
                                                        ChipTypeKnown = false;
                                                        break;
                                                }
                                            }
                                            else
                                            {
                                                UpdateTextBox(SCIBusBootstrapInfoTextBox, "(" + Util.ByteToHexString(SCIBusResponseBytes, 1, 2) + ").");
                                                FlashChipComboBox.SelectedIndex = 0;
                                                ChipTypeKnown = false;
                                            }

                                            if (!ManufacturerKnown || !ChipTypeKnown)
                                            {
                                                FlashChipComboBox.SelectedIndex = 0;
                                                UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Flash memory type could not be determined.");
                                                UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "Add request for flash memory chip support at:");
                                                UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "https://github.com/laszlodaniel/ChryslerScanner/discussions/8" + Environment.NewLine);

                                                if (SCIBusBootstrapWorker.IsBusy)
                                                {
                                                    SCIBusBootstrapWorker.CancelAsync();
                                                }
                                            }

                                            if ((FlashFileBuffer != null) && (FlashFileBuffer.Length > 0) && (FlashFileBuffer.Length != FlashChipSize))
                                            {
                                                UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Flash file size (" + FlashFileBuffer.Length.ToString() + " bytes) must be equal to the flash memory chip size (" + FlashChipSize.ToString() + " bytes)!");

                                                if (SCIBusBootstrapWorker.IsBusy)
                                                {
                                                    SCIBusBootstrapWorker.CancelAsync();
                                                }
                                            }
                                        }
                                        break;
                                    
                                    case (byte)WorkerFunction.FlashRead:
                                        UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Start flash reading.");

                                        if (SCIBusBootstrapWorker.IsBusy)
                                        {
                                            SCIBusResponse = true;
                                        }
                                        break;
                                    case (byte)WorkerFunction.FlashErase:
                                        UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Start flash erasing.");
                                        break;
                                    case (byte)WorkerFunction.FlashWrite:
                                        UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Start flash writing.");

                                        if (SCIBusBootstrapWorker.IsBusy)
                                        {
                                            SCIBusResponse = true;
                                        }
                                        break;
                                    case (byte)WorkerFunction.VerifyFlashChecksum:
                                        UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Skip flash checksum verification.");

                                        if (SCIBusBootstrapWorker.IsBusy)
                                        {
                                            CurrentTask = Task.UpdateEEPROM;
                                            SCIBusResponse = true;
                                        }
                                        break;
                                    case (byte)WorkerFunction.EEPROMReadSPI:
                                    case (byte)WorkerFunction.EEPROMReadParallel:
                                        UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Start EEPROM reading.");

                                        if (SCIBusBootstrapWorker.IsBusy)
                                        {
                                            SCIBusResponse = true;
                                        }
                                        break;
                                    case (byte)WorkerFunction.EEPROMWriteSPI:
                                    case (byte)WorkerFunction.EEPROMWriteParallel:
                                        UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Start EEPROM writing.");

                                        if (CurrentTask == Task.UpdateEEPROM)
                                        {
                                            UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Skip EEPROM update.");
                                        }

                                        if (SCIBusBootstrapWorker.IsBusy)
                                        {
                                            SCIBusResponse = true;
                                        }
                                        break;
                                    case (byte)WorkerFunction.Empty:
                                    default:
                                        UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Start worker function.");
                                        break;
                                }
                                break;
                            case (byte)SCI_ID.ExitWorkerFunction:
                                switch ((byte)WorkerFunctionComboBox.SelectedIndex)
                                {
                                    case (byte)WorkerFunction.FlashRead:
                                        UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Exit flash reading.");

                                        if (CurrentTask == Task.BackupFlashMemory)
                                        {
                                            if (EEPROMBackupCheckBox.Checked)
                                            {
                                                switch (BootloaderComboBox.SelectedIndex)
                                                {
                                                    case (byte)Bootloader.JTEC_256k:
                                                        UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "Step 5. Backup EEPROM." + Environment.NewLine + Environment.NewLine + "Skip EEPROM backup.");
                                                        CurrentTask = Task.EraseFlashMemory;
                                                        break;
                                                    default:
                                                        CurrentTask = Task.BackupEEPROM;
                                                        break;
                                                }
                                            }
                                            else
                                            {
                                                UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "Step 5. Backup EEPROM." + Environment.NewLine + Environment.NewLine + "Skip EEPROM backup.");

                                                CurrentTask = Task.EraseFlashMemory;
                                            }
                                        }
                                        else if (CurrentTask == Task.ReadFlashMemory)
                                        {
                                            CurrentTask = Task.FinishFlashRead;
                                        }
                                        
                                        SCIBusResponse = true;
                                        break;
                                    case (byte)WorkerFunction.FlashErase:
                                        UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Flash erased successfully.");
                                        break;
                                    case (byte)WorkerFunction.FlashWrite:
                                        UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Exit flash writing.");
                                        CurrentTask = Task.VerifyFlashChecksum;
                                        SCIBusResponse = true;
                                        break;
                                    case (byte)WorkerFunction.VerifyFlashChecksum:
                                        UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Exit flash checksum verification.");

                                        switch (BootloaderComboBox.SelectedIndex)
                                        {
                                            case (byte)Bootloader.JTEC_256k:
                                                CurrentTask = Task.FinishFlashWrite;
                                                break;
                                            default:
                                                CurrentTask = Task.UpdateEEPROM;
                                                break;
                                        }

                                        SCIBusResponse = true;
                                        break;
                                    case (byte)WorkerFunction.EEPROMReadSPI:
                                    case (byte)WorkerFunction.EEPROMReadParallel:
                                        UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Exit EEPROM reading.");

                                        if (CurrentTask == Task.BackupEEPROM)
                                        {
                                            CurrentTask = Task.EraseFlashMemory;
                                        }
                                        else if (CurrentTask == Task.ReadEEPROM)
                                        {
                                            CurrentTask = Task.FinishEEPROMRead;
                                        }
                                        
                                        SCIBusResponse = true;
                                        break;
                                    case (byte)WorkerFunction.EEPROMWriteSPI:
                                    case (byte)WorkerFunction.EEPROMWriteParallel:
                                        UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Exit EEPROM writing.");

                                        if (CurrentTask == Task.UpdateEEPROM)
                                        {
                                            CurrentTask = Task.FinishFlashWrite;
                                        }
                                        else if (CurrentTask == Task.WriteEEPROM)
                                        {
                                            CurrentTask = Task.FinishEEPROMWrite;
                                        }
                                        
                                        //SCIBusBootstrapFinished = true;
                                        SCIBusResponse = true;
                                        break;
                                    case (byte)WorkerFunction.Empty:
                                    default:
                                        UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Exit worker function.");
                                        break;
                                }
                                
                                break;
                            case (byte)SCI_ID.BootstrapSeedKeyRequest:
                                break;
                            case (byte)SCI_ID.BootstrapSeedKeyResponse:
                                if ((SCIBusResponseBytes.Length == 5) && (SCIBusResponseBytes[1] == 0xD0) && (SCIBusResponseBytes[2] == 0x67) && (SCIBusResponseBytes[3] == 0xC2) && (SCIBusResponseBytes[4] == 0x1F))
                                {
                                    UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Unlock bootstrap mode security. OK.");
                                }
                                break;
                            case (byte)SCI_ID.FlashBlockWrite:
                                if (SCIBusResponseBytes.Length > 6)
                                {
                                    List<byte> offset = new List<byte>();
                                    List<byte> length = new List<byte>();
                                    List<byte> values = new List<byte>();
                                    offset.Clear();
                                    length.Clear();
                                    values.Clear();
                                    offset.AddRange(SCIBusResponseBytes.Skip(1).Take(3));
                                    length.AddRange(SCIBusResponseBytes.Skip(4).Take(2));
                                    values.AddRange(SCIBusResponseBytes.Skip(6));

                                    ushort blockSize = (ushort)((SCIBusResponseBytes[4] << 8) + SCIBusResponseBytes[5]);
                                    ushort echoCount = (ushort)(SCIBusResponseBytes.Length - 6);

                                    UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Write offset: " + Util.ByteToHexStringSimple(offset.ToArray()) + ". Size: " + Util.ByteToHexStringSimple(length.ToArray()) + ". ");

                                    if (echoCount == blockSize)
                                    {
                                        UpdateTextBox(SCIBusBootstrapInfoTextBox, "OK.");

                                        if (SCIBusBootstrapWorker.IsBusy)
                                        {
                                            if ((SCIBusResponseBytes[1] == SCIBusTxPayload[1]) && (SCIBusResponseBytes[2] == SCIBusTxPayload[2]) && (SCIBusResponseBytes[3] == SCIBusTxPayload[3]))
                                            {
                                                SCIBusCurrentMemoryOffset += FlashWriteBlockSize;

                                                if ((FlashChipComboBox.SelectedIndex < 4) && (FlashChipComboBox.SelectedIndex != 0)) // 128 kB
                                                {
                                                    if (SCIBusCurrentMemoryOffset >= 0x20000)
                                                    {
                                                        ExitButton_Click(this, EventArgs.Empty);
                                                    }
                                                }
                                                else // 256 kB
                                                {
                                                    if (SCIBusCurrentMemoryOffset >= 0x40000)
                                                    {
                                                        ExitButton_Click(this, EventArgs.Empty);
                                                    }
                                                }

                                                SCIBusBootstrapToolsProgressLabel.Text = "Progress: " + (byte)(Math.Round((double)SCIBusCurrentMemoryOffset / (double)FlashChipSize * 100.0)) + "% (" + SCIBusCurrentMemoryOffset.ToString() + "/" + FlashChipSize.ToString() + " bytes)";
                                                SCIBusResponse = true;
                                                SCIBusRxTimeoutTimer.Stop();
                                            }
                                        }
                                    }
                                    else
                                    {
                                        switch (SCIBusResponseBytes[SCIBusResponseBytes.Length - 1]) // last payload byte stores error status
                                        {
                                            case (byte)SCI_ID.WriteError:
                                                UpdateTextBox(SCIBusBootstrapInfoTextBox, "Write error.");
                                                break;
                                            case (byte)SCI_ID.BlockSizeError:
                                                UpdateTextBox(SCIBusBootstrapInfoTextBox, "Invalid block size.");
                                                break;
                                            default:
                                                UpdateTextBox(SCIBusBootstrapInfoTextBox, "Unknown error.");
                                                break;
                                        }

                                        UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Exit flash writing.");

                                        if (SCIBusBootstrapWorker.IsBusy)
                                        {
                                            SCIBusBootstrapWorker.CancelAsync();
                                        }
                                    }
                                }
                                break;
                            case (byte)SCI_ID.FlashBlockRead:
                                if (SCIBusResponseBytes.Length > 6)
                                {
                                    List<byte> offset = new List<byte>();
                                    List<byte> length = new List<byte>();
                                    List<byte> values = new List<byte>();
                                    offset.Clear();
                                    length.Clear();
                                    values.Clear();
                                    offset.AddRange(SCIBusResponseBytes.Skip(1).Take(3));
                                    length.AddRange(SCIBusResponseBytes.Skip(4).Take(2));
                                    values.AddRange(SCIBusResponseBytes.Skip(6));

                                    ushort blockSize = (ushort)((SCIBusResponseBytes[4] << 8) + SCIBusResponseBytes[5]);
                                    ushort echoCount = (ushort)(SCIBusResponseBytes.Length - 6);

                                    UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Read offset: " + Util.ByteToHexStringSimple(offset.ToArray()) + ". Size: " + Util.ByteToHexStringSimple(length.ToArray()) + ". ");

                                    if (echoCount == blockSize)
                                    {
                                        UpdateTextBox(SCIBusBootstrapInfoTextBox, "OK.");

                                        if (SCIBusBootstrapWorker.IsBusy)
                                        {
                                            using (BinaryWriter writer = new BinaryWriter(File.Open(SCIBusFlashReadFilename, FileMode.Append)))
                                            {
                                                writer.Write(values.ToArray());
                                                writer.Close();
                                            }

                                            SCIBusCurrentMemoryOffset += FlashReadBlockSize;
                                            SCIBusBootstrapToolsProgressLabel.Text = "Progress: " + (byte)(Math.Round((double)SCIBusCurrentMemoryOffset / (double)FlashChipSize * 100.0)) + "% (" + SCIBusCurrentMemoryOffset.ToString() + "/" + FlashChipSize.ToString() + " bytes)";

                                            if ((FlashChipComboBox.SelectedIndex < 4) && (FlashChipComboBox.SelectedIndex != 0)) // 128 kB
                                            {
                                                if (SCIBusCurrentMemoryOffset >= 0x20000)
                                                {
                                                    UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Flash memory content saved to:" + Environment.NewLine + SCIBusFlashReadFilename);
                                                    ExitButton_Click(this, EventArgs.Empty);
                                                }
                                            }
                                            else // 256 kB
                                            {
                                                if (SCIBusCurrentMemoryOffset >= 0x40000)
                                                {
                                                    UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Flash memory content saved to:" + Environment.NewLine + SCIBusFlashReadFilename);
                                                    ExitButton_Click(this, EventArgs.Empty);
                                                }
                                            }

                                            SCIBusResponse = true;
                                            SCIBusRxTimeoutTimer.Stop();
                                        }
                                    }
                                    else
                                    {
                                        switch (SCIBusResponseBytes[SCIBusResponseBytes.Length - 1]) // last payload byte stores error status
                                        {
                                            case (byte)SCI_ID.BlockSizeError:
                                                UpdateTextBox(SCIBusBootstrapInfoTextBox, "Invalid block size.");
                                                break;
                                            default:
                                                UpdateTextBox(SCIBusBootstrapInfoTextBox, "Unknown error.");
                                                break;
                                        }

                                        UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Exit flash reading.");

                                        if (SCIBusBootstrapWorker.IsBusy)
                                        {
                                            SCIBusBootstrapWorker.CancelAsync();
                                        }
                                    }
                                }
                                break;
                            case (byte)SCI_ID.EEPROMBlockWrite:
                                if (SCIBusResponseBytes.Length > 5)
                                {
                                    List<byte> offset = new List<byte>();
                                    List<byte> length = new List<byte>();
                                    List<byte> values = new List<byte>();
                                    offset.Clear();
                                    length.Clear();
                                    values.Clear();
                                    offset.AddRange(SCIBusResponseBytes.Skip(1).Take(2));
                                    length.AddRange(SCIBusResponseBytes.Skip(3).Take(2));
                                    values.AddRange(SCIBusResponseBytes.Skip(5));

                                    ushort blockSize = (ushort)((SCIBusResponseBytes[3] << 8) + SCIBusResponseBytes[4]);
                                    ushort echoCount = (ushort)(SCIBusResponseBytes.Length - 5);

                                    UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Write offset: " + Util.ByteToHexStringSimple(offset.ToArray()) + ". Size: " + Util.ByteToHexStringSimple(length.ToArray()) + ". ");

                                    if ((echoCount == blockSize) && (offset[0] < 2))
                                    {
                                        UpdateTextBox(SCIBusBootstrapInfoTextBox, "OK.");

                                        if (SCIBusBootstrapWorker.IsBusy)
                                        {
                                            if ((SCIBusResponseBytes[1] == SCIBusTxPayload[1]) && (SCIBusResponseBytes[2] == SCIBusTxPayload[2]))
                                            {
                                                SCIBusCurrentMemoryOffset += EEPROMWriteBlockSize;
                                                SCIBusBootstrapToolsProgressLabel.Text = "Progress: " + (byte)(Math.Round((double)SCIBusCurrentMemoryOffset / 512.0 * 100.0)) + "% (" + SCIBusCurrentMemoryOffset.ToString() + "/512 bytes)";
                                                ExitButton_Click(this, EventArgs.Empty);
                                                SCIBusResponse = true;
                                                SCIBusRxTimeoutTimer.Stop();
                                            }
                                        }
                                    }
                                    else
                                    {
                                        switch (SCIBusResponseBytes[SCIBusResponseBytes.Length - 1]) // last payload byte stores error status
                                        {
                                            case (byte)SCI_ID.WriteError:
                                                UpdateTextBox(SCIBusBootstrapInfoTextBox, "Write error.");
                                                break;
                                            case (byte)SCI_ID.BlockSizeError:
                                                UpdateTextBox(SCIBusBootstrapInfoTextBox, "Invalid block size.");
                                                break;
                                            case (byte)SCI_ID.OffsetError:
                                                UpdateTextBox(SCIBusBootstrapInfoTextBox, "Invalid offset.");
                                                break;
                                            default:
                                                UpdateTextBox(SCIBusBootstrapInfoTextBox, "Unknown error.");
                                                break;
                                        }

                                        UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Exit EEPROM writing.");

                                        if (SCIBusBootstrapWorker.IsBusy)
                                        {
                                            SCIBusBootstrapWorker.CancelAsync();
                                        }
                                    }
                                }
                                break;
                            case (byte)SCI_ID.EEPROMBlockRead:
                                if (SCIBusResponseBytes.Length > 5)
                                {
                                    List<byte> offset = new List<byte>();
                                    List<byte> length = new List<byte>();
                                    List<byte> values = new List<byte>();
                                    offset.Clear();
                                    length.Clear();
                                    values.Clear();
                                    offset.AddRange(SCIBusResponseBytes.Skip(1).Take(2));
                                    length.AddRange(SCIBusResponseBytes.Skip(3).Take(2));
                                    values.AddRange(SCIBusResponseBytes.Skip(5));

                                    ushort blockSize = (ushort)((SCIBusResponseBytes[3] << 8) + SCIBusResponseBytes[4]);
                                    ushort echoCount = (ushort)(SCIBusResponseBytes.Length - 5);

                                    UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Read offset: " + Util.ByteToHexStringSimple(offset.ToArray()) + ". Size: " + Util.ByteToHexStringSimple(length.ToArray()) + ". ");

                                    if ((echoCount == blockSize) && (offset[0] < 2))
                                    {
                                        UpdateTextBox(SCIBusBootstrapInfoTextBox, "OK.");

                                        if (blockSize == 512)
                                        {
                                            using (BinaryWriter writer = new BinaryWriter(File.Open(SCIBusEEPROMReadFilename, FileMode.Append)))
                                            {
                                                writer.Write(values.ToArray());
                                                writer.Close();
                                            }

                                            UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "EEPROM content saved to:" + Environment.NewLine + SCIBusEEPROMReadFilename);

                                            if (SCIBusBootstrapWorker.IsBusy)
                                            {
                                                if ((SCIBusResponseBytes[1] == SCIBusTxPayload[1]) && (SCIBusResponseBytes[2] == SCIBusTxPayload[2]))
                                                {
                                                    SCIBusCurrentMemoryOffset += EEPROMReadBlockSize;
                                                    SCIBusBootstrapToolsProgressLabel.Text = "Progress: " + (byte)(Math.Round((double)SCIBusCurrentMemoryOffset / 512.0 * 100.0)) + "% (" + SCIBusCurrentMemoryOffset.ToString() + "/512 bytes)";
                                                    ExitButton_Click(this, EventArgs.Empty);
                                                    SCIBusResponse = true;
                                                    SCIBusRxTimeoutTimer.Stop();
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        switch (SCIBusResponseBytes[SCIBusResponseBytes.Length - 1]) // last payload byte stores error status
                                        {
                                            case (byte)SCI_ID.BlockSizeError:
                                                UpdateTextBox(SCIBusBootstrapInfoTextBox, "Invalid block size.");
                                                break;
                                            case (byte)SCI_ID.OffsetError:
                                                UpdateTextBox(SCIBusBootstrapInfoTextBox, "Invalid offset.");
                                                break;
                                            default:
                                                UpdateTextBox(SCIBusBootstrapInfoTextBox, "Unknown error.");
                                                break;
                                        }

                                        UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Exit EEPROM reading.");

                                        if (SCIBusBootstrapWorker.IsBusy)
                                        {
                                            SCIBusBootstrapWorker.CancelAsync();
                                        }
                                    }
                                }
                                break;
                            case (byte)SCI_ID.StartBootloader:
                                if ((SCIBusResponseBytes.Length == 4) && (SCIBusResponseBytes[3] == 0x22))
                                {
                                    UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Start bootloader. OK.");
                                }
                                else if(SCIBusResponseBytes.Length == 3)
                                {
                                    UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Start bootloader. Error.");
                                }
                                break;
                            case (byte)SCI_ID.UploadBootloader:
                                UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Upload bootloader: ");

                                switch ((byte)BootloaderComboBox.SelectedIndex)
                                {
                                    case (byte)Bootloader.SBEC3_128k:
                                        UpdateTextBox(SCIBusBootstrapInfoTextBox, "SBEC3 (128k). ");
                                        break;
                                    case (byte)Bootloader.SBEC3_256k:
                                        UpdateTextBox(SCIBusBootstrapInfoTextBox, "SBEC3 (256k). ");
                                        break;
                                    case (byte)Bootloader.SBEC3_256k_custom:
                                        UpdateTextBox(SCIBusBootstrapInfoTextBox, "SBEC3 custom (256k). ");
                                        break;
                                    case (byte)Bootloader.JTEC_256k:
                                        UpdateTextBox(SCIBusBootstrapInfoTextBox, "JTEC (256k). ");
                                        break;
                                    case (byte)Bootloader.Empty:
                                    default:
                                        UpdateTextBox(SCIBusBootstrapInfoTextBox, "empty. ");
                                        break;
                                }

                                ushort start = (ushort)((SCIBusResponseBytes[1] << 8) + SCIBusResponseBytes[2]);
                                ushort end = (ushort)((SCIBusResponseBytes[3] << 8) + SCIBusResponseBytes[4]);

                                if ((end - start + 1) == (SCIBusResponseBytes.Length - 5))
                                {
                                    UpdateTextBox(SCIBusBootstrapInfoTextBox, "OK.");
                                }
                                else
                                {
                                    UpdateTextBox(SCIBusBootstrapInfoTextBox, "Error.");
                                }
                                break;
                            case (byte)SCI_ID.BlockSizeError:
                                switch ((byte)WorkerFunctionComboBox.SelectedIndex)
                                {
                                    case (byte)WorkerFunction.FlashRead:
                                    case (byte)WorkerFunction.FlashWrite:
                                        UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Flash block size error.");
                                        break;
                                    case (byte)WorkerFunction.EEPROMReadSPI:
                                    case (byte)WorkerFunction.EEPROMWriteSPI:
                                        UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "EEPROM block size error.");
                                        break;
                                    default:
                                        UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Block size error.");
                                        break;
                                }

                                if (SCIBusBootstrapWorker.IsBusy)
                                {
                                    SCIBusBootstrapWorker.CancelAsync();
                                }
                                break;
                            case (byte)SCI_ID.EraseError_81:
                                switch ((byte)WorkerFunctionComboBox.SelectedIndex)
                                {
                                    case (byte)WorkerFunction.FlashErase:
                                        UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Flash erase error 0x81.");
                                        break;
                                    default:
                                        UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Error 0x81.");
                                        break;
                                }

                                if (SCIBusBootstrapWorker.IsBusy)
                                {
                                    SCIBusBootstrapWorker.CancelAsync();
                                }
                                break;
                            case (byte)SCI_ID.EraseError_82:
                                switch ((byte)WorkerFunctionComboBox.SelectedIndex)
                                {
                                    case (byte)WorkerFunction.FlashErase:
                                        UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Flash erase error 0x82.");
                                        break;
                                    default:
                                        UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Error 0x82.");
                                        break;
                                }

                                if (SCIBusBootstrapWorker.IsBusy)
                                {
                                    SCIBusBootstrapWorker.CancelAsync();
                                }
                                break;
                            case (byte)SCI_ID.EraseError_83:
                                switch ((byte)WorkerFunctionComboBox.SelectedIndex)
                                {
                                    case (byte)WorkerFunction.FlashErase:
                                        UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Flash erase error 0x83.");
                                        break;
                                    default:
                                        UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Error 0x83.");
                                        break;
                                }

                                if (SCIBusBootstrapWorker.IsBusy)
                                {
                                    SCIBusBootstrapWorker.CancelAsync();
                                }
                                break;
                            case (byte)SCI_ID.OffsetError:
                                switch ((byte)WorkerFunctionComboBox.SelectedIndex)
                                {
                                    case (byte)WorkerFunction.EEPROMReadSPI:
                                    case (byte)WorkerFunction.EEPROMWriteSPI:
                                        UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "EEPROM offset error.");
                                        break;
                                    default:
                                        UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Error 0x84.");
                                        break;
                                }

                                if (SCIBusBootstrapWorker.IsBusy)
                                {
                                    SCIBusBootstrapWorker.CancelAsync();
                                }
                                break;
                            case (byte)SCI_ID.BootstrapModeNotProtected:
                                if ((SCIBusResponseBytes.Length == 5) && (SCIBusResponseBytes[1] == 0x2F) && (SCIBusResponseBytes[2] == 0xD8) && (SCIBusResponseBytes[3] == 0x3E) && (SCIBusResponseBytes[4] == 0x23))
                                {
                                    UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Bootstrap mode is not protected.");
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    break;
            }

            SCIBusNextRequest = false;
            SCIBusNextRequestTimer.Stop();
            SCIBusNextRequestTimer.Start();
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
            if (SCIBusBootstrapWorker.IsBusy) SCIBusBootstrapWorker.CancelAsync();

            if (SwitchBackToLSWhenExit && (OriginalForm.PCM.speed == "62500 baud"))
            {
                UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "Scanner SCI-bus speed is set to 7812.5 baud.");
                OriginalForm.SelectSCIBusPCMLSMode();
            }

            MainForm.Packet.PacketReceived -= PacketReceivedHandler; // unsubscribe from the OnPacketReceived event
        }
    }
}
