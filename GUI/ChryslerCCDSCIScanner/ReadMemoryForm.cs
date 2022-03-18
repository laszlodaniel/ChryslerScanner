using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Timers;
using System.Windows.Forms;

namespace ChryslerCCDSCIScanner
{
    public partial class ReadMemoryForm : Form
    {
        public MainForm OriginalForm;

        public bool CCDBusAlive = false;
        public bool CCDBusEcho = false;
        public bool CCDBusResponse = false;
        public bool CCDBusNextRequest = false;
        public byte CCDBusRxRetryCount = 0;
        public byte CCDBusTxRetryCount = 0;
        public bool CCDBusReadMemoryFinished = false;
        public bool CCDBusRxTimeout = false;
        public bool CCDBusTxTimeout = false;
        public byte CCDBusModule = 0;
        public byte CCDBusReadMemoryCommand = 0;
        public byte[] CCDBusMemoryOffsetStartBytes = new byte[2];
        public uint CCDBusMemoryOffsetStart = 0;
        public byte[] CCDBusMemoryOffsetEndBytes = new byte[2];
        public uint CCDBusMemoryOffsetEnd = 0;
        public byte[] CCDBusIncrementBytes = new byte[2];
        public uint CCDBusIncrement = 0;
        public byte[] CCDBusCurrentMemoryOffsetBytes = new byte[2];
        public uint CCDBusCurrentMemoryOffset = 0;
        public byte[] CCDBusMemoryValueBytes = new byte[2];
        public uint CCDBusBytesReadCount = 0;
        public uint CCDBusTotalBytes = 0;
        public byte[] CCDBusTxPayload = new byte[6] { 0xB2, 0x20, 0x22, 0x00, 0x00, 0xF4 };

        public string CCDBusMemoryBinaryFilename;
        public string CCDBusMemoryTextFilename;

        public System.Timers.Timer CCDBusAliveTimer = new System.Timers.Timer();
        public System.Timers.Timer CCDBusNextRequestTimer = new System.Timers.Timer();
        public System.Timers.Timer CCDBusRxTimeoutTimer = new System.Timers.Timer();
        public System.Timers.Timer CCDBusTxTimeoutTimer = new System.Timers.Timer();
        public BackgroundWorker CCDBusReadMemoryWorker = new BackgroundWorker();


        public bool SCIBusPCMResponse = false;
        public bool SCIBusPCMNextRequest = false;
        public byte SCIBusPCMRxRetryCount = 0;
        public byte SCIBusPCMTxRetryCount = 0;
        public bool SCIBusPCMReadMemoryFinished = false;
        public bool SCIBusPCMRxTimeout = false;
        public bool SCIBusPCMTxTimeout = false;
        public byte SCIBusPCMReadMemoryCommand = 0;
        public byte SCIBusPCMMemoryOffsetWidth = 24; // bits
        public byte[] SCIBusPCMMemoryOffsetStartBytes = new byte[3];
        public uint SCIBusPCMMemoryOffsetStart = 0;
        public byte[] SCIBusPCMMemoryOffsetEndBytes = new byte[3];
        public uint SCIBusPCMMemoryOffsetEnd = 0;
        public byte[] SCIBusPCMIncrementBytes = new byte[3];
        public uint SCIBusPCMIncrement = 0;
        public byte[] SCIBusPCMCurrentMemoryOffsetBytes = new byte[3];
        public uint SCIBusPCMCurrentMemoryOffset = 0;
        public byte SCIBusPCMMemoryValue = 0;
        public byte[] SCIBusPCMMemoryArray;
        public uint SCIBusPCMBytesReadCount = 0;
        public uint SCIBusPCMTotalBytes = 0;
        public byte[] SCIBusPCMTxPayload = new byte[4] { 0x26, 0x00, 0x00, 0x00 };

        public string SCIBusPCMMemoryBinaryFilename;
        public string SCIBusPCMMemoryTextFilename;

        public System.Timers.Timer SCIBusPCMNextRequestTimer = new System.Timers.Timer();
        public System.Timers.Timer SCIBusPCMRxTimeoutTimer = new System.Timers.Timer();
        public System.Timers.Timer SCIBusPCMTxTimeoutTimer = new System.Timers.Timer();
        public BackgroundWorker SCIBusPCMReadMemoryWorker = new BackgroundWorker();

        public TimeSpan ElapsedMillis;
        public DateTime Timestamp;
        public string TimestampString;

        public ReadMemoryForm(MainForm IncomingForm)
        {
            OriginalForm = IncomingForm;
            InitializeComponent();
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            MainForm.Packet.PacketReceived += PacketReceivedHandler; // subscribe to the OnPacketReceived event

            CCDBusAliveTimer.Elapsed += new ElapsedEventHandler(CCDBusAliveHandler);
            CCDBusAliveTimer.Interval = 1000; // ms
            CCDBusAliveTimer.AutoReset = true;
            CCDBusAliveTimer.Enabled = true;
            CCDBusAliveTimer.Start();

            CCDBusNextRequestTimer.Elapsed += new ElapsedEventHandler(CCDBusNextRequestHandler);
            CCDBusNextRequestTimer.Interval = 150; // ms
            CCDBusNextRequestTimer.AutoReset = false;
            CCDBusNextRequestTimer.Enabled = true;
            CCDBusNextRequestTimer.Start();

            CCDBusRxTimeoutTimer.Elapsed += new ElapsedEventHandler(CCDBusRxTimeoutHandler);
            CCDBusRxTimeoutTimer.Interval = 2000; // ms
            CCDBusRxTimeoutTimer.AutoReset = false;
            CCDBusRxTimeoutTimer.Enabled = true;
            CCDBusRxTimeoutTimer.Stop();

            CCDBusTxTimeoutTimer.Elapsed += new ElapsedEventHandler(CCDBusTxTimeoutHandler);
            CCDBusTxTimeoutTimer.Interval = 2000; // ms
            CCDBusTxTimeoutTimer.AutoReset = false;
            CCDBusTxTimeoutTimer.Enabled = true;
            CCDBusTxTimeoutTimer.Stop();

            CCDBusReadMemoryWorker.WorkerReportsProgress = true;
            CCDBusReadMemoryWorker.WorkerSupportsCancellation = true;
            CCDBusReadMemoryWorker.DoWork += new DoWorkEventHandler(CCDBusReadMemory_DoWork);
            CCDBusReadMemoryWorker.ProgressChanged += new ProgressChangedEventHandler(CCDBusReadMemory_ProgressChanged);
            CCDBusReadMemoryWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(CCDBusReadMemory_RunWorkerCompleted);

            SCIBusPCMNextRequestTimer.Elapsed += new ElapsedEventHandler(SCIBusPCMNextRequestHandler);
            SCIBusPCMNextRequestTimer.Interval = 25; // ms
            SCIBusPCMNextRequestTimer.AutoReset = false;
            SCIBusPCMNextRequestTimer.Enabled = true;
            SCIBusPCMNextRequestTimer.Start();

            SCIBusPCMRxTimeoutTimer.Elapsed += new ElapsedEventHandler(SCIBusPCMRxTimeoutHandler);
            SCIBusPCMRxTimeoutTimer.Interval = 2000; // ms
            SCIBusPCMRxTimeoutTimer.AutoReset = false;
            SCIBusPCMRxTimeoutTimer.Enabled = true;
            SCIBusPCMRxTimeoutTimer.Stop();

            SCIBusPCMTxTimeoutTimer.Elapsed += new ElapsedEventHandler(SCIBusPCMTxTimeoutHandler);
            SCIBusPCMTxTimeoutTimer.Interval = 2000; // ms
            SCIBusPCMTxTimeoutTimer.AutoReset = false;
            SCIBusPCMTxTimeoutTimer.Enabled = true;
            SCIBusPCMTxTimeoutTimer.Stop();

            SCIBusPCMReadMemoryWorker.WorkerReportsProgress = true;
            SCIBusPCMReadMemoryWorker.WorkerSupportsCancellation = true;
            SCIBusPCMReadMemoryWorker.DoWork += new DoWorkEventHandler(SCIBusPCMReadMemory_DoWork);
            SCIBusPCMReadMemoryWorker.ProgressChanged += new ProgressChangedEventHandler(SCIBusPCMReadMemory_ProgressChanged);
            SCIBusPCMReadMemoryWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(SCIBusPCMReadMemory_RunWorkerCompleted);

            SCIBusPCMReadMemoryPresetComboBox.SelectedIndex = 2;

            ActiveControl = CCDBusReadMemoryInitializeSessionButton;
        }

        #region CCD-bus

        private void CCDBusAliveHandler(object source, ElapsedEventArgs e) => CCDBusAlive = false;

        private void CCDBusNextRequestHandler(object source, ElapsedEventArgs e) => CCDBusNextRequest = true;

        private void CCDBusRxTimeoutHandler(object source, ElapsedEventArgs e) => CCDBusRxTimeout = true;

        private void CCDBusTxTimeoutHandler(object source, ElapsedEventArgs e) => CCDBusTxTimeout = true;

        private void CCDBusReadMemoryInitializeSessionButton_Click(object sender, EventArgs e)
        {
            bool success = false;

            byte[] a = Util.HexStringToByte(CCDBusReadMemoryModuleTextBox.Text);

            if (a.Length == 1)
            {
                success = byte.TryParse(a[0].ToString(), out CCDBusModule);
                if (!success) CCDBusModule = 0x20;
            }
            else CCDBusModule = 0x20;

            CCDBusReadMemoryModuleTextBox.Text = Util.ByteToHexStringSimple(new byte[1] { CCDBusModule });
            CCDBusReadMemoryModuleTextBox.SelectionLength = 0;

            a = Util.HexStringToByte(CCDBusReadMemoryCommandTextBox.Text);

            if (a.Length == 1)
            {
                success = byte.TryParse(a[0].ToString(), out CCDBusReadMemoryCommand);
                if (!success) CCDBusReadMemoryCommand = 0x22;
            }
            else CCDBusReadMemoryCommand = 0x22;

            CCDBusReadMemoryCommandTextBox.Text = Util.ByteToHexStringSimple(new byte[1] { CCDBusReadMemoryCommand });
            CCDBusReadMemoryCommandTextBox.SelectionLength = 0;

            CCDBusMemoryOffsetStartBytes = Util.HexStringToByte(CCDBusReadMemoryStartOffsetTextBox.Text);

            if (CCDBusMemoryOffsetStartBytes.Length == 2)
            {
                success = uint.TryParse(((CCDBusMemoryOffsetStartBytes[0] << 8) | CCDBusMemoryOffsetStartBytes[1]).ToString(), out CCDBusMemoryOffsetStart);

                if (!success)
                {
                    CCDBusMemoryOffsetStart = 0;
                    CCDBusMemoryOffsetStartBytes = new byte[2];
                    CCDBusMemoryOffsetStartBytes[0] = 0x00;
                    CCDBusMemoryOffsetStartBytes[1] = 0x00;
                }
            }
            else
            {
                CCDBusMemoryOffsetStart = 0;
                CCDBusMemoryOffsetStartBytes = new byte[2];
                CCDBusMemoryOffsetStartBytes[0] = 0x00;
                CCDBusMemoryOffsetStartBytes[1] = 0x00;
            }

            CCDBusReadMemoryStartOffsetTextBox.Text = Util.ByteToHexStringSimple(CCDBusMemoryOffsetStartBytes);
            CCDBusReadMemoryStartOffsetTextBox.SelectionLength = 0;

            CCDBusMemoryOffsetEndBytes = Util.HexStringToByte(CCDBusReadMemoryEndOffsetTextBox.Text);

            if (CCDBusMemoryOffsetEndBytes.Length == 2)
            {
                success = uint.TryParse(((CCDBusMemoryOffsetEndBytes[0] << 8) | CCDBusMemoryOffsetEndBytes[1]).ToString(), out CCDBusMemoryOffsetEnd);

                if (!success)
                {
                    CCDBusMemoryOffsetEnd = 0xFFFE;
                    CCDBusMemoryOffsetEndBytes = new byte[2];
                    CCDBusMemoryOffsetEndBytes[0] = 0xFF;
                    CCDBusMemoryOffsetEndBytes[1] = 0xFE;
                }
            }
            else
            {
                CCDBusMemoryOffsetEnd = 0xFFFE;
                CCDBusMemoryOffsetEndBytes = new byte[2];
                CCDBusMemoryOffsetEndBytes[0] = 0xFF;
                CCDBusMemoryOffsetEndBytes[1] = 0xFE;
            }

            CCDBusReadMemoryEndOffsetTextBox.Text = Util.ByteToHexStringSimple(CCDBusMemoryOffsetEndBytes);
            CCDBusReadMemoryEndOffsetTextBox.SelectionLength = 0;

            CCDBusIncrementBytes = Util.HexStringToByte(CCDBusReadMemoryIncrementTextBox.Text);

            if (CCDBusIncrementBytes.Length == 2)
            {
                success = uint.TryParse(((CCDBusIncrementBytes[0] << 8) | CCDBusIncrementBytes[1]).ToString(), out CCDBusIncrement);

                if (!success)
                {
                    CCDBusIncrement = 2;
                    CCDBusIncrementBytes = new byte[2];
                    CCDBusIncrementBytes[0] = 0x00;
                    CCDBusIncrementBytes[1] = 0x02;
                }
            }
            else
            {
                CCDBusIncrement = 2;
                CCDBusIncrementBytes = new byte[2];
                CCDBusIncrementBytes[0] = 0x00;
                CCDBusIncrementBytes[1] = 0x02;
            }

            CCDBusReadMemoryIncrementTextBox.Text = Util.ByteToHexStringSimple(CCDBusIncrementBytes);
            CCDBusReadMemoryIncrementTextBox.SelectionLength = 0;

            CCDBusCurrentMemoryOffsetBytes = CCDBusMemoryOffsetStartBytes.ToArray();
            CCDBusCurrentMemoryOffset = (ushort)(CCDBusCurrentMemoryOffsetBytes[0] << 8 | CCDBusCurrentMemoryOffsetBytes[1]);
            CCDBusReadMemoryCurrentOffsetTextBox.Text = Util.ByteToHexStringSimple(CCDBusCurrentMemoryOffsetBytes);
            CCDBusReadMemoryCurrentOffsetTextBox.SelectionLength = 0;

            if (CCDBusIncrement == 1) CCDBusReadMemoryValuesTextBox.Text = "00";
            else CCDBusReadMemoryValuesTextBox.Text = "00 00";

            string ModuleName;

            switch (CCDBusModule)
            {
                case 0x10:
                    ModuleName = "VIC – Vehicle Info Center";
                    break;
                case 0x18:
                case 0x1B:
                    ModuleName = "VTS – Vehicle Theft Security";
                    break;
                case 0x19:
                    ModuleName = "CMT – Compass Mini-Trip";
                    break;
                case 0x1E:
                    ModuleName = "ACM – Airbag Control Module";
                    break;
                case 0x20:
                    ModuleName = "BCM – Body Control Module";
                    break;
                case 0x22:
                case 0x60:
                    ModuleName = "MIC – Mechanical Instrument Cluster";
                    break;
                case 0x41:
                case 0x42:
                    ModuleName = "TCM – Transmission Control Module";
                    break;
                case 0x43:
                    ModuleName = "ABS – Antilock Brake System";
                    break;
                case 0x50:
                    ModuleName = "HVAC - Heat Vent Air Conditioning";
                    break;
                case 0x80:
                    ModuleName = "DDM - Driver Door Module";
                    break;
                case 0x81:
                    ModuleName = "PDM - Passenger Door Module";
                    break;
                case 0x82:
                    ModuleName = "MSM - Memory Seat Module";
                    break;
                case 0x96:
                    ModuleName = "ASM - Audio System Module";
                    break;
                case 0xC0:
                    ModuleName = "SKIM - Sentry Key Immobilizer Module";
                    break;
                default:
                    ModuleName = "unknown";
                    break;
            }

            string DateTimeNow = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            CCDBusMemoryBinaryFilename = @"ROMs/CCD/ccd_eprom_" + DateTimeNow + ".bin";
            CCDBusMemoryTextFilename = @"ROMs/CCD/ccd_eprom_" + DateTimeNow + ".txt";

            if (CCDBusIncrement == 2) CCDBusTotalBytes = CCDBusMemoryOffsetEnd - CCDBusMemoryOffsetStart + CCDBusIncrement;
            else CCDBusTotalBytes = (CCDBusMemoryOffsetEnd - CCDBusMemoryOffsetStart + CCDBusIncrement) / CCDBusIncrement;

            CCDBusBytesReadCount = 0;
            CCDBusReadMemoryProgressLabel.Text = "Progress: " + (byte)(Math.Round((double)CCDBusBytesReadCount / (double)CCDBusTotalBytes * 100.0)) + "% (" + CCDBusBytesReadCount.ToString() + "/" + CCDBusTotalBytes.ToString() + " bytes)";

            CCDBusReadMemoryInfoTextBox.Clear();
            UpdateTextBox(CCDBusReadMemoryInfoTextBox, "Initialize memory reading session.");
            UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "Module: " + Util.ByteToHexStringSimple(new byte[] { CCDBusModule }) + " (" + ModuleName + ")");
            UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "Command: " + Util.ByteToHexStringSimple(new byte[] { CCDBusReadMemoryCommand }));
            UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "Start offset: " + Util.ByteToHexStringSimple(CCDBusMemoryOffsetStartBytes));
            UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "End offset: " + Util.ByteToHexStringSimple(CCDBusMemoryOffsetEndBytes));
            UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "Increment: " + Util.ByteToHexStringSimple(CCDBusIncrementBytes));
            UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "Output: " + CCDBusMemoryBinaryFilename.ToString());
            UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "Binary size: " + CCDBusTotalBytes.ToString() + " bytes = " + ((double)CCDBusTotalBytes / 1024.0).ToString("0.00") + " kilobytes.");
            UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "Check CCD-bus status: ");

            if (CCDBusAlive)
            {
                UpdateTextBox(CCDBusReadMemoryInfoTextBox, "alive");

                UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "Memory reading session is ready to start.");
            }
            else UpdateTextBox(CCDBusReadMemoryInfoTextBox, "no activity");

            if (!CCDBusReadMemoryWorker.IsBusy && CCDBusAlive)
            {
                CCDBusReadMemoryInitializeSessionButton.Enabled = true;
                CCDBusReadMemoryStartButton.Enabled = true;
                CCDBusReadMemoryStopButton.Enabled = false;
                CCDBusEcho = false;
                CCDBusResponse = false;
                CCDBusRxTimeout = false;
                CCDBusTxTimeout = false;
                CCDBusRxRetryCount = 0;
                CCDBusTxRetryCount = 0;

                CCDBusTxPayload = new byte[6] { 0xB2, CCDBusModule, CCDBusReadMemoryCommand, CCDBusCurrentMemoryOffsetBytes[0], CCDBusCurrentMemoryOffsetBytes[1], 0x00 };
                byte checksum = 0;
                for (int i = 0; i < 5; i++) checksum += CCDBusTxPayload[i];
                CCDBusTxPayload[5] = checksum;
            }
            else
            {
                CCDBusReadMemoryInitializeSessionButton.Enabled = true;
                CCDBusReadMemoryStartButton.Enabled = false;
                CCDBusReadMemoryStopButton.Enabled = false;
                UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "Failed to initialize session.");
            }

            CCDBusReadMemoryHelpButton.Enabled = true;
        }

        private void CCDBusReadMemoryStartButton_Click(object sender, EventArgs e)
        {
            if (CCDBusAlive)
            {
                CCDBusReadMemoryCurrentOffsetTextBox.Text = Util.ByteToHexStringSimple(CCDBusCurrentMemoryOffsetBytes);
                CCDBusReadMemoryCurrentOffsetTextBox.SelectionLength = 0;

                CCDBusTxPayload = new byte[6] { 0xB2, CCDBusModule, CCDBusReadMemoryCommand, CCDBusCurrentMemoryOffsetBytes[0], CCDBusCurrentMemoryOffsetBytes[1], 0x00 };
                byte checksum = 0;
                for (int i = 0; i < 5; i++) checksum += CCDBusTxPayload[i];
                CCDBusTxPayload[5] = checksum;

                CCDBusReadMemoryInitializeSessionButton.Enabled = false;
                CCDBusReadMemoryStartButton.Enabled = false;
                CCDBusReadMemoryStopButton.Enabled = true;
                CCDBusEcho = false;
                CCDBusResponse = false;
                CCDBusReadMemoryFinished = false;
                CCDBusNextRequest = true;
                CCDBusRxRetryCount = 0;
                CCDBusTxRetryCount = 0;
                UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "Start memory reading session.");
                CCDBusReadMemoryWorker.RunWorkerAsync();
            }
            else
            {
                CCDBusReadMemoryStartButton.Enabled = false;
                CCDBusReadMemoryStopButton.Enabled = false;
                UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "No CCD-bus activity detected.");
            }
        }

        private void CCDBusReadMemoryStopButton_Click(object sender, EventArgs e)
        {
            if (CCDBusReadMemoryWorker.IsBusy && CCDBusReadMemoryWorker.WorkerSupportsCancellation)
            {
                CCDBusReadMemoryWorker.CancelAsync();
            }

            CCDBusEcho = false;
            CCDBusResponse = false;
            CCDBusReadMemoryFinished = true;
        }

        private void CCDBusReadMemoryHelpButton_Click(object sender, EventArgs e)
        {
            if (CCDBusReadMemoryInfoTextBox.Text != "") UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + Environment.NewLine);

            UpdateTextBox(CCDBusReadMemoryInfoTextBox, "----------------------HELP---------------------");
            UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "Module: hex address of the target module to be interrogated.");
            UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "Known module addresses:");
            UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "10:    VIC - Vehicle Info Center");
            UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "18,1B: VTS - Vehicle Theft Security");
            UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "19:    CMT - Compass Mini-Trip");
            UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "1E:    ACM - Airbag Control Module");
            UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "20:    BCM - Body Control Module");
            UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "22,60: MIC - Mechanical Instrument Cluster");
            UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "41,42: TCM - Transmission Control Module");
            UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "43:    ABS - Antilock Brake System");
            UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "50:   HVAC - Heat Vent Air Conditioning");
            UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "80:    DDM - Driver Door Module");
            UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "81:    PDM - Passenger Door Module");
            UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "82:    MSM - Memory Seat Module");
            UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "96:    ASM - Audio System Module");
            UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "C0:   SKIM - Sentry Key Immobilizer Module");
            UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Command: hex value of the \"read memory\" command recognized by the target module. 22 is known to be working with BCM. Not all modules may support memory reading.");
            UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Start offset: this 16-bit value signifies the starting address of the memory space. Zero is a good place to start.");
            UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "End offset: this 16-bit value signifies the ending address of the memory space. With a usual 64 kB of program space FF FE is a good place to end the session.");
            UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Increment: memory values are read using multiple request messages and this value gets added to the requested memory offset. Due to the format of the B2/F2 diagnostic messages 2 payload bytes are returned. During memory reading the first payload byte of the response message contains the requested memory value at the given offset, the second payload byte contains the next memory value. Therefore it's enough to request every other memory offset. When in doubt use an increment value of 1 and change the end offset value to FF FF.");
            UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Current offset/Value(s): these show the current status of the session.");
            UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Initialize session: prepares session, assigns output filename and checks if CCD-bus is alive.");
            UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Start: begins to read the target module's memory using the given instructions. Multiple timeout measures are in place to ensure data integrity and to avoid hiccups. The reading session ends automatically once the ending offset is reached.");
            UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Stop: cancels the current session with the possibility of resuming where the session was left hanging.");
            UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Progress: shows current session status in percentage done and how much memory bytes have been read out of the total bytes count.");
            UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Help: shows this text.");
            UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Remarks: the binary output file and text log file are being updated during the session in real time. Modules with program space bigger than 64 kB may have multiple \"read memory\" commands. ROM/RAM/EEPROM areas are usually reachable between a single offset interval. Check the target module's microcontroller datasheet to discover where different memory areas are located.");
            UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "-----------------------------------------------" + Environment.NewLine);
        }

        private void CCDBusReadMemory_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!CCDBusReadMemoryFinished)
            {
                Thread.Sleep(1);

                if (CCDBusReadMemoryWorker.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }

                while (!CCDBusNextRequest) // wait for next request message
                {
                    Thread.Sleep(1);

                    if (CCDBusReadMemoryWorker.CancellationPending)
                    {
                        e.Cancel = true;
                        break;
                    }
                }

                CCDBusEcho = false;
                CCDBusResponse = false;
                CCDBusNextRequest = false;

                CCDBusReadMemoryWorker.ReportProgress(0); // new messages are sent in the ProgressChanged event handler method

                while (!CCDBusEcho && !CCDBusTxTimeout)
                {
                    Thread.Sleep(1);

                    if (CCDBusReadMemoryWorker.CancellationPending)
                    {
                        e.Cancel = true;
                        break;
                    }
                }

                if (CCDBusTxTimeout)
                {
                    CCDBusTxRetryCount++;

                    if (CCDBusTxRetryCount > 9)
                    {
                        CCDBusTxRetryCount = 0;
                        e.Cancel = true;
                        break;
                    }

                    CCDBusNextRequest = true;
                    CCDBusTxTimeout = false;
                    CCDBusTxTimeoutTimer.Stop();
                    CCDBusTxTimeoutTimer.Start();
                }

                if (CCDBusEcho)
                {
                    CCDBusTxRetryCount = 0;

                    while (!CCDBusResponse && !CCDBusRxTimeout)
                    {
                        Thread.Sleep(1);

                        if (CCDBusReadMemoryWorker.CancellationPending)
                        {
                            e.Cancel = true;
                            break;
                        }
                    }

                    if (CCDBusResponse)
                    {
                        CCDBusRxRetryCount = 0;
                    }

                    if (CCDBusRxTimeout)
                    {
                        CCDBusRxRetryCount++;

                        BeginInvoke((MethodInvoker)delegate
                        {
                            UpdateTextBox(CCDBusReadMemoryInfoTextBox, " | no response");
                        });

                        if (CCDBusRxRetryCount > 9)
                        {
                            CCDBusRxRetryCount = 0;
                            e.Cancel = true;
                            break;
                        }

                        CCDBusNextRequest = true;
                        CCDBusRxTimeout = false;
                        CCDBusRxTimeoutTimer.Stop();
                        CCDBusRxTimeoutTimer.Start();
                    }
                }
            }
        }

        private void CCDBusReadMemory_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 0)
            {
                // Fill Packet class fields with data.
                MainForm.Packet.tx.source = (byte)Packet.Source.device;
                MainForm.Packet.tx.target = (byte)Packet.Target.ccd;
                MainForm.Packet.tx.command = (byte)Packet.Command.msgTx;
                MainForm.Packet.tx.mode = (byte)Packet.MsgTxMode.single;
                MainForm.Packet.tx.payload = CCDBusTxPayload;
                MainForm.Packet.GeneratePacket();

                OriginalForm.TransmitUSBPacket("[<-TX] Send a CCD-bus message once:");

                CCDBusTxTimeout = false;
                CCDBusTxTimeoutTimer.Stop();
                CCDBusTxTimeoutTimer.Start();
            }
        }

        private void CCDBusReadMemory_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled == true)
            {
                if (CCDBusRxTimeout) UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "Memory reading session timeout (RX).");
                if (CCDBusTxTimeout) UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "Memory reading session timeout (TX).");
                UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "Memory reading session cancelled.");

                CCDBusReadMemoryInitializeSessionButton.Enabled = true;
                CCDBusReadMemoryStartButton.Enabled = true;
                CCDBusReadMemoryStopButton.Enabled = false;
            }
            else
            {
                UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "Output: " + CCDBusMemoryBinaryFilename.ToString());
                UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "Memory reading session finished.");

                CCDBusReadMemoryInitializeSessionButton.Enabled = true;
                CCDBusReadMemoryStartButton.Enabled = false;
                CCDBusReadMemoryStopButton.Enabled = false;
            }

            CCDBusRxTimeout = false;
            CCDBusRxTimeoutTimer.Stop();
            CCDBusTxTimeout = false;
            CCDBusTxTimeoutTimer.Stop();
            //CCDBusNextRequest = false;
            //CCDBusNextRequestTimer.Stop();
        }

        #endregion

        #region SCI-bus (PCM)

        private void SCIBusPCMNextRequestHandler(object source, ElapsedEventArgs e) => SCIBusPCMNextRequest = true;

        private void SCIBusPCMRxTimeoutHandler(object source, ElapsedEventArgs e) => SCIBusPCMRxTimeout = true;

        private void SCIBusPCMTxTimeoutHandler(object source, ElapsedEventArgs e) => SCIBusPCMTxTimeout = true;

        private void SCIBusPCMReadMemoryInitializeSessionButton_Click(object sender, EventArgs e)
        {
            bool success = false;

            byte[] a = Util.HexStringToByte(SCIBusPCMReadMemoryCommandTextBox.Text);

            if (a.Length == 1)
            {
                success = byte.TryParse(a[0].ToString(), out SCIBusPCMReadMemoryCommand);
                if (!success) SCIBusPCMReadMemoryCommand = 0x26;
            }
            else SCIBusPCMReadMemoryCommand = 0x26;

            SCIBusPCMReadMemoryCommandTextBox.Text = Util.ByteToHexStringSimple(new byte[1] { SCIBusPCMReadMemoryCommand });
            SCIBusPCMReadMemoryCommandTextBox.SelectionLength = 0;

            SCIBusPCMMemoryOffsetStartBytes = Util.HexStringToByte(SCIBusPCMReadMemoryStartOffsetTextBox.Text);

            if (SCIBusPCMMemoryOffsetStartBytes.Length == 2)
            {
                SCIBusPCMMemoryOffsetWidth = 16; // bits

                success = uint.TryParse(((SCIBusPCMMemoryOffsetStartBytes[0] << 8) | SCIBusPCMMemoryOffsetStartBytes[1]).ToString(), out SCIBusPCMMemoryOffsetStart);

                if (!success)
                {
                    SCIBusPCMMemoryOffsetStart = 0;
                    SCIBusPCMMemoryOffsetStartBytes = new byte[2];
                    SCIBusPCMMemoryOffsetStartBytes[0] = 0x00;
                    SCIBusPCMMemoryOffsetStartBytes[1] = 0x00;
                }
            }
            else if (SCIBusPCMMemoryOffsetStartBytes.Length == 3)
            {
                SCIBusPCMMemoryOffsetWidth = 24; // bits

                success = uint.TryParse(((SCIBusPCMMemoryOffsetStartBytes[0] << 16) | (SCIBusPCMMemoryOffsetStartBytes[1] << 8) | SCIBusPCMMemoryOffsetStartBytes[2]).ToString(), out SCIBusPCMMemoryOffsetStart);

                if (!success)
                {
                    SCIBusPCMMemoryOffsetStart = 0;
                    SCIBusPCMMemoryOffsetStartBytes = new byte[3];
                    SCIBusPCMMemoryOffsetStartBytes[0] = 0x00;
                    SCIBusPCMMemoryOffsetStartBytes[1] = 0x00;
                    SCIBusPCMMemoryOffsetStartBytes[2] = 0x00;
                }
            }
            else
            {
                SCIBusPCMMemoryOffsetWidth = 24; // bits
                SCIBusPCMMemoryOffsetStart = 0;
                SCIBusPCMMemoryOffsetStartBytes = new byte[3];
                SCIBusPCMMemoryOffsetStartBytes[0] = 0x00;
                SCIBusPCMMemoryOffsetStartBytes[1] = 0x00;
                SCIBusPCMMemoryOffsetStartBytes[2] = 0x00;
            }

            SCIBusPCMReadMemoryStartOffsetTextBox.Text = Util.ByteToHexStringSimple(SCIBusPCMMemoryOffsetStartBytes);
            SCIBusPCMReadMemoryStartOffsetTextBox.SelectionLength = 0;

            SCIBusPCMMemoryOffsetEndBytes = Util.HexStringToByte(SCIBusPCMReadMemoryEndOffsetTextBox.Text);

            if (SCIBusPCMMemoryOffsetWidth == 16)
            {
                if (SCIBusPCMMemoryOffsetEndBytes.Length == 2)
                {
                    success = uint.TryParse(((SCIBusPCMMemoryOffsetEndBytes[0] << 8) | SCIBusPCMMemoryOffsetEndBytes[1]).ToString(), out SCIBusPCMMemoryOffsetEnd);

                    if (!success)
                    {
                        SCIBusPCMMemoryOffsetEnd = 0xFFFF;
                        SCIBusPCMMemoryOffsetEndBytes = new byte[2];
                        SCIBusPCMMemoryOffsetEndBytes[0] = 0xFF;
                        SCIBusPCMMemoryOffsetEndBytes[1] = 0xFF;
                    }
                }
                else
                {
                    SCIBusPCMMemoryOffsetEnd = 0xFFFF;
                    SCIBusPCMMemoryOffsetEndBytes = new byte[2];
                    SCIBusPCMMemoryOffsetEndBytes[0] = 0xFF;
                    SCIBusPCMMemoryOffsetEndBytes[1] = 0xFF;
                }
            }
            
            if (SCIBusPCMMemoryOffsetWidth == 24)
            {
                if (SCIBusPCMMemoryOffsetEndBytes.Length == 3)
                {
                    success = uint.TryParse(((SCIBusPCMMemoryOffsetEndBytes[0] << 16) | (SCIBusPCMMemoryOffsetEndBytes[1] << 8) | SCIBusPCMMemoryOffsetEndBytes[2]).ToString(), out SCIBusPCMMemoryOffsetEnd);

                    if (!success)
                    {
                        SCIBusPCMMemoryOffsetEnd = 0x01FFFF;
                        SCIBusPCMMemoryOffsetEndBytes = new byte[3];
                        SCIBusPCMMemoryOffsetEndBytes[0] = 0x01;
                        SCIBusPCMMemoryOffsetEndBytes[1] = 0xFF;
                        SCIBusPCMMemoryOffsetEndBytes[2] = 0xFF;
                    }
                }
                else
                {
                    SCIBusPCMMemoryOffsetEnd = 0x01FFFF;
                    SCIBusPCMMemoryOffsetEndBytes = new byte[3];
                    SCIBusPCMMemoryOffsetEndBytes[0] = 0x01;
                    SCIBusPCMMemoryOffsetEndBytes[1] = 0xFF;
                    SCIBusPCMMemoryOffsetEndBytes[2] = 0xFF;
                }
            }

            SCIBusPCMReadMemoryEndOffsetTextBox.Text = Util.ByteToHexStringSimple(SCIBusPCMMemoryOffsetEndBytes);
            SCIBusPCMReadMemoryEndOffsetTextBox.SelectionLength = 0;

            SCIBusPCMIncrementBytes = Util.HexStringToByte(SCIBusPCMReadMemoryIncrementTextBox.Text);

            if (SCIBusPCMMemoryOffsetWidth == 16)
            {
                if (SCIBusPCMIncrementBytes.Length == 2)
                {
                    success = uint.TryParse(((SCIBusPCMIncrementBytes[0] << 8) | SCIBusPCMIncrementBytes[1]).ToString(), out SCIBusPCMIncrement);

                    if (!success)
                    {
                        SCIBusPCMIncrement = 1;
                        SCIBusPCMIncrementBytes = new byte[2];
                        SCIBusPCMIncrementBytes[0] = 0x00;
                        SCIBusPCMIncrementBytes[1] = 0x01;
                    }
                }
                else
                {
                    SCIBusPCMIncrement = 1;
                    SCIBusPCMIncrementBytes = new byte[2];
                    SCIBusPCMIncrementBytes[0] = 0x00;
                    SCIBusPCMIncrementBytes[1] = 0x01;
                }
            }

            if (SCIBusPCMMemoryOffsetWidth == 24)
            {
                if (SCIBusPCMIncrementBytes.Length == 3)
                {
                    success = uint.TryParse(((SCIBusPCMIncrementBytes[0] << 16) | (SCIBusPCMIncrementBytes[1] << 8) | SCIBusPCMIncrementBytes[2]).ToString(), out SCIBusPCMIncrement);

                    if (!success)
                    {
                        SCIBusPCMIncrement = 1;
                        SCIBusPCMIncrementBytes = new byte[3];
                        SCIBusPCMIncrementBytes[0] = 0x00;
                        SCIBusPCMIncrementBytes[1] = 0x00;
                        SCIBusPCMIncrementBytes[2] = 0x01;
                    }
                }
                else
                {
                    SCIBusPCMIncrement = 1;
                    SCIBusPCMIncrementBytes = new byte[3];
                    SCIBusPCMIncrementBytes[0] = 0x00;
                    SCIBusPCMIncrementBytes[1] = 0x00;
                    SCIBusPCMIncrementBytes[2] = 0x01;
                }
            }

            SCIBusPCMReadMemoryIncrementTextBox.Text = Util.ByteToHexStringSimple(SCIBusPCMIncrementBytes);
            SCIBusPCMReadMemoryIncrementTextBox.SelectionLength = 0;

            SCIBusPCMCurrentMemoryOffsetBytes = SCIBusPCMMemoryOffsetStartBytes.ToArray();

            if (SCIBusPCMMemoryOffsetWidth == 16) SCIBusPCMCurrentMemoryOffset = (uint)(SCIBusPCMCurrentMemoryOffsetBytes[0] << 8 | SCIBusPCMCurrentMemoryOffsetBytes[1]);

            if (SCIBusPCMMemoryOffsetWidth == 24) SCIBusPCMCurrentMemoryOffset = (uint)((SCIBusPCMCurrentMemoryOffsetBytes[0] << 16) | (SCIBusPCMCurrentMemoryOffsetBytes[1] << 8) | SCIBusPCMCurrentMemoryOffsetBytes[2]);

            SCIBusPCMReadMemoryCurrentOffsetTextBox.Text = Util.ByteToHexStringSimple(SCIBusPCMCurrentMemoryOffsetBytes);
            SCIBusPCMReadMemoryCurrentOffsetTextBox.SelectionLength = 0;

            SCIBusPCMReadMemoryValueTextBox.Text = "00";

            SCIBusPCMTotalBytes = SCIBusPCMMemoryOffsetEnd - SCIBusPCMMemoryOffsetStart + 1;
            SCIBusPCMBytesReadCount = 0;
            SCIBusPCMReadMemoryProgressLabel.Text = "Progress: " + (byte)(Math.Round((double)SCIBusPCMBytesReadCount / (double)SCIBusPCMTotalBytes * 100.0)) + "% (" + SCIBusPCMBytesReadCount.ToString() + "/" + SCIBusPCMTotalBytes.ToString() + " bytes)";

            SCIBusPCMReadMemoryInfoTextBox.Clear();

            string DateTimeNow = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            switch (SCIBusPCMReadMemoryPresetComboBox.SelectedIndex)
            {
                case 0: // ROM/32kB
                    SCIBusPCMMemoryBinaryFilename = @"ROMs/PCM/pcm_eprom_" + DateTimeNow + ".bin";
                    SCIBusPCMMemoryTextFilename = @"ROMs/PCM/pcm_eprom_" + DateTimeNow + ".txt";
                    UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, "Initialize memory reading session.");
                    UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + "Preset: ");
                    UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, "ROM/32kB");
                    break;
                case 1: // ROM/64kB
                    SCIBusPCMMemoryBinaryFilename = @"ROMs/PCM/pcm_eprom_" + DateTimeNow + ".bin";
                    SCIBusPCMMemoryTextFilename = @"ROMs/PCM/pcm_eprom_" + DateTimeNow + ".txt";
                    UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, "Initialize memory reading session.");
                    UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + "Preset: ");
                    UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, "ROM/64kB");
                    break;
                case 2: // ROM/128kB
                    SCIBusPCMMemoryBinaryFilename = @"ROMs/PCM/pcm_eprom_" + DateTimeNow + ".bin";
                    SCIBusPCMMemoryTextFilename = @"ROMs/PCM/pcm_eprom_" + DateTimeNow + ".txt";
                    UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, "Initialize memory reading session.");
                    UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + "Preset: ");
                    UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, "ROM/128kB");
                    break;
                case 3: // ROM/256kB
                    SCIBusPCMMemoryBinaryFilename = @"ROMs/PCM/pcm_eprom_" + DateTimeNow + ".bin";
                    SCIBusPCMMemoryTextFilename = @"ROMs/PCM/pcm_eprom_" + DateTimeNow + ".txt";
                    UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, "Initialize memory reading session.");
                    UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + "Preset: ");
                    UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, "ROM/256kB");
                    break;
                case 4: // EEPROM (512B)
                    SCIBusPCMMemoryBinaryFilename = @"ROMs/PCM/pcm_eeprom_" + DateTimeNow + ".bin";
                    SCIBusPCMMemoryTextFilename = @"ROMs/PCM/pcm_eeprom_" + DateTimeNow + ".txt";
                    UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, "Initialize memory reading session.");
                    UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + "Preset: ");
                    UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, "EEPROM");
                    break;
                case 5: // RAM (6kB)
                    SCIBusPCMMemoryBinaryFilename = @"ROMs/PCM/pcm_ram_" + DateTimeNow + ".bin";
                    SCIBusPCMMemoryTextFilename = @"ROMs/PCM/pcm_ram_" + DateTimeNow + ".txt";
                    UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, "Initialize memory reading session.");
                    UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + "Preset: ");
                    UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, "RAM");
                    break;
            }

            UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + "Command: " + Util.ByteToHexStringSimple(new byte[] { SCIBusPCMReadMemoryCommand }));
            UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + "Start offset: " + Util.ByteToHexStringSimple(SCIBusPCMMemoryOffsetStartBytes));
            UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + "End offset: " + Util.ByteToHexStringSimple(SCIBusPCMMemoryOffsetEndBytes));
            UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + "Increment: " + Util.ByteToHexStringSimple(SCIBusPCMIncrementBytes));
            UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + "Output: " + SCIBusPCMMemoryBinaryFilename.ToString());
            UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + "Binary size: " + SCIBusPCMTotalBytes.ToString() + " bytes = " + ((double)SCIBusPCMTotalBytes / 1024.0).ToString("0.00") + " kilobytes.");
            UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + "Memory reading session is ready to start.");

            if (!SCIBusPCMReadMemoryWorker.IsBusy)
            {
                SCIBusPCMReadMemoryInitializeSessionButton.Enabled = true;
                SCIBusPCMReadMemoryStartButton.Enabled = true;
                SCIBusPCMReadMemoryStopButton.Enabled = false;
                SCIBusPCMResponse = false;
                SCIBusPCMRxTimeout = false;
                SCIBusPCMTxTimeout = false;
                SCIBusPCMRxRetryCount = 0;
                SCIBusPCMTxRetryCount = 0;

                if (SCIBusPCMMemoryOffsetWidth == 16) SCIBusPCMTxPayload = new byte[3] { SCIBusPCMReadMemoryCommand, SCIBusPCMCurrentMemoryOffsetBytes[0], SCIBusPCMCurrentMemoryOffsetBytes[1] };
                else SCIBusPCMTxPayload = new byte[4] { SCIBusPCMReadMemoryCommand, SCIBusPCMCurrentMemoryOffsetBytes[0], SCIBusPCMCurrentMemoryOffsetBytes[1], SCIBusPCMCurrentMemoryOffsetBytes[2] };

                if ((SCIBusPCMMemoryOffsetWidth == 24) && (SCIBusPCMReadMemoryCommand == 0x45)) SCIBusPCMTxPayload = new byte[6] { SCIBusPCMReadMemoryCommand, SCIBusPCMCurrentMemoryOffsetBytes[0], SCIBusPCMCurrentMemoryOffsetBytes[1], SCIBusPCMCurrentMemoryOffsetBytes[2], SCIBusPCMIncrementBytes[1], SCIBusPCMIncrementBytes[2] };
            }
            else
            {
                SCIBusPCMReadMemoryInitializeSessionButton.Enabled = true;
                SCIBusPCMReadMemoryStartButton.Enabled = false;
                SCIBusPCMReadMemoryStopButton.Enabled = false;
                UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + "Failed to initialize session.");
            }

            if (SCIBusPCMReadMemoryCommand == 0x45) SCIBusPCMNextRequestTimer.Interval = 10; // ms
            else SCIBusPCMNextRequestTimer.Interval = 25; // ms

            SCIBusPCMReadMemoryHelpButton.Enabled = true;
        }

        private void SCIBusPCMReadMemoryStartButton_Click(object sender, EventArgs e)
        {
            SCIBusPCMReadMemoryCurrentOffsetTextBox.Text = Util.ByteToHexStringSimple(SCIBusPCMCurrentMemoryOffsetBytes);
            SCIBusPCMReadMemoryCurrentOffsetTextBox.SelectionLength = 0;

            if (SCIBusPCMMemoryOffsetWidth == 16) SCIBusPCMTxPayload = new byte[3] { SCIBusPCMReadMemoryCommand, SCIBusPCMCurrentMemoryOffsetBytes[0], SCIBusPCMCurrentMemoryOffsetBytes[1] };
            else SCIBusPCMTxPayload = new byte[4] { SCIBusPCMReadMemoryCommand, SCIBusPCMCurrentMemoryOffsetBytes[0], SCIBusPCMCurrentMemoryOffsetBytes[1], SCIBusPCMCurrentMemoryOffsetBytes[2] };

            if ((SCIBusPCMMemoryOffsetWidth == 24) && (SCIBusPCMReadMemoryCommand == 0x45)) SCIBusPCMTxPayload = new byte[6] { SCIBusPCMReadMemoryCommand, SCIBusPCMCurrentMemoryOffsetBytes[0], SCIBusPCMCurrentMemoryOffsetBytes[1], SCIBusPCMCurrentMemoryOffsetBytes[2], SCIBusPCMIncrementBytes[1], SCIBusPCMIncrementBytes[2] };

            SCIBusPCMReadMemoryInitializeSessionButton.Enabled = false;
            SCIBusPCMReadMemoryStartButton.Enabled = false;
            SCIBusPCMReadMemoryStopButton.Enabled = true;
            SCIBusPCMResponse = false;
            SCIBusPCMReadMemoryFinished = false;
            SCIBusPCMNextRequest = true;
            SCIBusPCMRxRetryCount = 0;
            SCIBusPCMTxRetryCount = 0;
            UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + "Start memory reading session.");
            SCIBusPCMReadMemoryWorker.RunWorkerAsync();
        }

        private void SCIBusPCMReadMemoryStopButton_Click(object sender, EventArgs e)
        {
            if (SCIBusPCMReadMemoryWorker.IsBusy && SCIBusPCMReadMemoryWorker.WorkerSupportsCancellation)
            {
                SCIBusPCMReadMemoryWorker.CancelAsync();
            }

            SCIBusPCMResponse = false;
            SCIBusPCMReadMemoryFinished = true;
        }

        private void SCIBusPCMReadMemoryHelpButton_Click(object sender, EventArgs e)
        {
            if (SCIBusPCMReadMemoryInfoTextBox.Text != "") UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + Environment.NewLine);

            UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, "----------------------HELP---------------------");
            UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + "Command: hex value of the \"read memory\" command recognized by the PCM. For 16-bit address space 15, for 24-bit address space 26 is known to be working. Not all modules may support memory reading.");
            UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Start offset: this value (16/24-bit) signifies the starting address of the memory space. Zero is a good place to start.");
            UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "End offset: this value (16/24-bit) signifies the ending address of the memory space.");
            UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Increment: memory values are read using multiple request messages and this value gets added to the requested memory offset. The default increment value of 1 is appropriate in 99.9% of cases.");
            UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Current offset/Value: these show the current status of the session.");
            UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Initialize session: prepares session, assigns output filename and makes the SCI-bus return to low-speed mode.");
            UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Start: begins to read the PCM's memory using the given instructions. Multiple timeout measures are in place to ensure data integrity and to avoid hiccups. The reading session ends automatically once the ending offset is reached.");
            UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Stop: cancels the current session with the possibility of resuming where the session was left hanging.");
            UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Progress: shows current session status in percentage done and how much memory bytes have been read out of the total bytes count.");
            UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Help: shows this text.");
            UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Remarks: the binary output file and text log file are being updated during the session in real time. PCMs may have different \"read memory\" commands. Memory reading works in low-speed mode only. ROM/RAM/EEPROM areas are usually reachable between a single offset interval. Check the target module's microcontroller datasheet to discover where different memory areas are located. The reader does not check the actual size of the program space, it has to be manually determined using the main GUI window. Check typical file size boundaries:" + Environment.NewLine + "- 7FFF: 32 kB" + Environment.NewLine + "- FFFF: 64 kB" + Environment.NewLine + "- 01FFFF: 128 kB" + Environment.NewLine + "- 03FFFF: 256 kB." + Environment.NewLine + "For a 128 kB program space the \"26 01 FF FF\" request should return a 5-byte response like \"26 01 FF FF AA\", where AA is the last readable memory value, while \"26 02 00 00\" will not return a memory value, just the 4-byte echo only. The command 28 seems to be used specifically to read EEPROM with 16-bit address space (28 00 00 - 28 01 FF).");
            UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + "-----------------------------------------------" + Environment.NewLine);
        }

        private void SCIBusPCMReadMemory_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!SCIBusPCMReadMemoryFinished)
            {
                Thread.Sleep(1);

                if (SCIBusPCMCurrentMemoryOffset > (SCIBusPCMMemoryOffsetEnd - SCIBusPCMIncrement)) SCIBusPCMReadMemoryFinished = true;

                if (SCIBusPCMReadMemoryWorker.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }

                while (!SCIBusPCMNextRequest) // wait for next request message
                {
                    Thread.Sleep(1);

                    if (SCIBusPCMReadMemoryWorker.CancellationPending)
                    {
                        e.Cancel = true;
                        break;
                    }
                }

                SCIBusPCMResponse = false;
                SCIBusPCMNextRequest = false;

                SCIBusPCMReadMemoryWorker.ReportProgress(0); // new messages are sent in the ProgressChanged event handler method

                while (!SCIBusPCMResponse && !SCIBusPCMRxTimeout)
                {
                    Thread.Sleep(1);

                    if (SCIBusPCMReadMemoryWorker.CancellationPending)
                    {
                        e.Cancel = true;
                        break;
                    }
                }

                if (SCIBusPCMRxTimeout)
                {
                    SCIBusPCMRxRetryCount++;

                    if (SCIBusPCMRxRetryCount > 9)
                    {
                        SCIBusPCMRxRetryCount = 0;
                        e.Cancel = true;
                        break;
                    }

                    SCIBusPCMNextRequest = true;
                    SCIBusPCMRxTimeout = false;
                    SCIBusPCMRxTimeoutTimer.Stop();
                    SCIBusPCMRxTimeoutTimer.Start();
                }

                if (SCIBusPCMResponse)
                {
                    SCIBusPCMRxRetryCount = 0;
                }
            }
        }

        private void SCIBusPCMReadMemory_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 0)
            {
                UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + "TX: " + Util.ByteToHexStringSimple(SCIBusPCMTxPayload));

                // Fill Packet class fields with data.
                MainForm.Packet.tx.source = (byte)Packet.Source.device;
                MainForm.Packet.tx.target = (byte)Packet.Target.pcm;
                MainForm.Packet.tx.command = (byte)Packet.Command.msgTx;
                MainForm.Packet.tx.mode = (byte)Packet.MsgTxMode.single;
                MainForm.Packet.tx.payload = SCIBusPCMTxPayload;
                MainForm.Packet.GeneratePacket();

                OriginalForm.TransmitUSBPacket("[<-TX] Send an SCI-bus (PCM) message once:");
                SCIBusPCMRxTimeout = false;
                SCIBusPCMRxTimeoutTimer.Stop();
                SCIBusPCMRxTimeoutTimer.Start();
            }
        }

        private void SCIBusPCMReadMemory_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled == true)
            {
                if (SCIBusPCMRxTimeout) UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + "Memory reading session timeout (RX).");
                if (SCIBusPCMTxTimeout) UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + "Memory reading session timeout (TX).");
                UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + "Memory reading session cancelled.");

                SCIBusPCMReadMemoryInitializeSessionButton.Enabled = true;
                SCIBusPCMReadMemoryStartButton.Enabled = true;
                SCIBusPCMReadMemoryStopButton.Enabled = false;
            }
            else
            {
                UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + "Output: " + SCIBusPCMMemoryBinaryFilename.ToString());
                UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + "Memory reading session finished.");

                SCIBusPCMReadMemoryInitializeSessionButton.Enabled = true;
                SCIBusPCMReadMemoryStartButton.Enabled = false;
                SCIBusPCMReadMemoryStopButton.Enabled = false;
            }

            SCIBusPCMRxTimeout = false;
            SCIBusPCMRxTimeoutTimer.Stop();
            SCIBusPCMTxTimeout = false;
            SCIBusPCMTxTimeoutTimer.Stop();
            //SCIBusPCMNextRequest = false;
            //SCIBusPCMNextRequestTimer.Stop();
        }

        private void SCIBusPCMReadMemoryPresetComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (SCIBusPCMReadMemoryPresetComboBox.SelectedIndex)
            {
                case 0: // ROM/32kB
                    SCIBusPCMReadMemoryCommandTextBox.Text = "15";
                    SCIBusPCMReadMemoryStartOffsetTextBox.Text = "00 00";
                    SCIBusPCMReadMemoryEndOffsetTextBox.Text = "7F FF";
                    SCIBusPCMReadMemoryIncrementTextBox.Text = "00 01";
                    SCIBusPCMReadMemoryCurrentOffsetTextBox.Text = "00 00";
                    SCIBusPCMReadMemoryValueTextBox.Text = "00";
                    break;
                case 1: // ROM/64kB
                    SCIBusPCMReadMemoryCommandTextBox.Text = "15";
                    SCIBusPCMReadMemoryStartOffsetTextBox.Text = "00 00";
                    SCIBusPCMReadMemoryEndOffsetTextBox.Text = "FF FF";
                    SCIBusPCMReadMemoryIncrementTextBox.Text = "00 01";
                    SCIBusPCMReadMemoryCurrentOffsetTextBox.Text = "00 00";
                    SCIBusPCMReadMemoryValueTextBox.Text = "00";
                    break;
                case 2: // ROM/128kB
                    SCIBusPCMReadMemoryCommandTextBox.Text = "26";
                    SCIBusPCMReadMemoryStartOffsetTextBox.Text = "00 00 00";
                    SCIBusPCMReadMemoryEndOffsetTextBox.Text = "01 FF FF";
                    SCIBusPCMReadMemoryIncrementTextBox.Text = "00 00 01";
                    SCIBusPCMReadMemoryCurrentOffsetTextBox.Text = "00 00 00";
                    SCIBusPCMReadMemoryValueTextBox.Text = "00";
                    break;
                case 3: // ROM/256kB
                    SCIBusPCMReadMemoryCommandTextBox.Text = "26";
                    SCIBusPCMReadMemoryStartOffsetTextBox.Text = "00 00 00";
                    SCIBusPCMReadMemoryEndOffsetTextBox.Text = "03 FF FF";
                    SCIBusPCMReadMemoryIncrementTextBox.Text = "00 00 01";
                    SCIBusPCMReadMemoryCurrentOffsetTextBox.Text = "00 00 00";
                    SCIBusPCMReadMemoryValueTextBox.Text = "00";
                    break;
                case 4: // EEPROM (512B)
                    SCIBusPCMReadMemoryCommandTextBox.Text = "28";
                    SCIBusPCMReadMemoryStartOffsetTextBox.Text = "00 00";
                    SCIBusPCMReadMemoryEndOffsetTextBox.Text = "01 FF";
                    SCIBusPCMReadMemoryIncrementTextBox.Text = "00 01";
                    SCIBusPCMReadMemoryCurrentOffsetTextBox.Text = "00 00";
                    SCIBusPCMReadMemoryValueTextBox.Text = "00";
                    break;
                case 5: // RAM (6kB)
                    SCIBusPCMReadMemoryCommandTextBox.Text = "26";
                    SCIBusPCMReadMemoryStartOffsetTextBox.Text = "0F 80 00";
                    SCIBusPCMReadMemoryEndOffsetTextBox.Text = "0F 97 FF";
                    SCIBusPCMReadMemoryIncrementTextBox.Text = "00 00 01";
                    SCIBusPCMReadMemoryCurrentOffsetTextBox.Text = "0F 80 00";
                    SCIBusPCMReadMemoryValueTextBox.Text = "00";
                    break;
            }
        }

        #endregion

        #region Methods

        private void PacketReceivedHandler(object sender, EventArgs e)
        {
            if (MainForm.Packet.rx.payload.Length > 4)
            {
                //ElapsedMillis = TimeSpan.FromMilliseconds((MainForm.Packet.rx.payload[0] << 24) | (MainForm.Packet.rx.payload[1] << 16) | (MainForm.Packet.rx.payload[2] << 8) | MainForm.Packet.rx.payload[3]);
                //Timestamp = DateTime.Today.Add(ElapsedMillis);
                //TimestampString = Timestamp.ToString("HH:mm:ss.fff");
                //UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "T: " + TimestampString);
            }

            if (MainForm.Packet.rx.source == (byte)Packet.Source.device)
            {
                if ((MainForm.Packet.rx.command == (byte)Packet.Command.error) && (MainForm.Packet.rx.mode == (byte)Packet.ErrorMode.errorInternal))
                {
                    UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, " | error: internal error"); // most likely SCI-bus is disconnected
                }
            }

            if (MainForm.Packet.rx.source == (byte)Packet.Source.ccd)
            {
                CCDBusAliveTimer.Stop();
                CCDBusAliveTimer.Start();
                CCDBusAlive = true;

                byte[] CCDBusResponseBytes = MainForm.Packet.rx.payload.Skip(4).ToArray(); // skip 4 timestamp bytes

                if (CCDBusResponseBytes[0] == 0xB2)
                {
                    UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "TX: " + Util.ByteToHexStringSimple(CCDBusResponseBytes));

                    if (CCDBusResponseBytes.SequenceEqual(CCDBusTxPayload))
                    {
                        CCDBusEcho = true;
                        CCDBusResponse = false;
                        UpdateTextBox(CCDBusReadMemoryInfoTextBox, " | echo ok");
                        CCDBusTxTimeoutTimer.Stop();
                        CCDBusRxTimeout = false;
                        CCDBusRxTimeoutTimer.Start();
                    }
                    else
                    {
                        CCDBusEcho = false;
                        CCDBusResponse = false;
                        UpdateTextBox(CCDBusReadMemoryInfoTextBox, " | error: invalid echo");
                    }
                }
                
                if (CCDBusResponseBytes[0] == 0xF2)
                {
                    UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "RX: " + Util.ByteToHexStringSimple(CCDBusResponseBytes));

                    if (CCDBusResponseBytes.Length == 6) // correct response message's length is 6 bytes
                    {
                        if ((CCDBusResponseBytes[1] == CCDBusModule) && (CCDBusResponseBytes[2] == CCDBusReadMemoryCommand) && CCDBusEcho)
                        {
                            CCDBusRxTimeoutTimer.Stop();
                            CCDBusNextRequest = false;
                            CCDBusNextRequestTimer.Stop();
                            CCDBusNextRequestTimer.Start();

                            UpdateTextBox(CCDBusReadMemoryInfoTextBox, " | response ok");

                            CCDBusEcho = false;
                            CCDBusResponse = true;

                            if (CCDBusIncrement == 2) CCDBusMemoryValueBytes = CCDBusResponseBytes.Skip(3).Take(2).ToArray(); // get both payload bytes
                            else CCDBusMemoryValueBytes = CCDBusResponseBytes.Skip(3).Take(1).ToArray(); // get first payload byte only

                            CCDBusReadMemoryCurrentOffsetTextBox.Text = Util.ByteToHexStringSimple(CCDBusCurrentMemoryOffsetBytes);
                            CCDBusReadMemoryCurrentOffsetTextBox.SelectionLength = 0;
                            CCDBusReadMemoryValuesTextBox.Text = Util.ByteToHexStringSimple(CCDBusMemoryValueBytes);
                            CCDBusReadMemoryValuesTextBox.SelectionLength = 0;

                            if (CCDBusIncrement == 2) CCDBusBytesReadCount += 2;
                            else CCDBusBytesReadCount++;

                            CCDBusReadMemoryProgressLabel.Text = "Progress: " + (byte)((double)CCDBusBytesReadCount / (double)CCDBusTotalBytes * 100.0) + "% (" + CCDBusBytesReadCount.ToString() + "/" + CCDBusTotalBytes.ToString() + " bytes)";

                            using (BinaryWriter writer = new BinaryWriter(File.Open(CCDBusMemoryBinaryFilename, FileMode.Append)))
                            {
                                writer.Write(CCDBusMemoryValueBytes); // write byte(s) to file
                                writer.Close();
                            }

                            CCDBusCurrentMemoryOffset += CCDBusIncrement;
                            CCDBusCurrentMemoryOffsetBytes[0] = (byte)(CCDBusCurrentMemoryOffset >> 8);
                            CCDBusCurrentMemoryOffsetBytes[1] = (byte)CCDBusCurrentMemoryOffset;

                            CCDBusTxPayload = new byte[6] { 0xB2, CCDBusModule, CCDBusReadMemoryCommand, CCDBusCurrentMemoryOffsetBytes[0], CCDBusCurrentMemoryOffsetBytes[1], 0x00 };
                            byte checksum = 0;
                            for (int i = 0; i < 5; i++) checksum += CCDBusTxPayload[i];
                            CCDBusTxPayload[5] = checksum;

                            if (CCDBusCurrentMemoryOffset > (CCDBusMemoryOffsetEnd - CCDBusIncrement)) CCDBusReadMemoryFinished = true;
                        }
                        else if (CCDBusResponseBytes[2] == 0xFF)
                        {
                            UpdateTextBox(CCDBusReadMemoryInfoTextBox, " | error: unknown command");
                            CCDBusReadMemoryFinished = true;

                            if (CCDBusReadMemoryWorker.IsBusy && CCDBusReadMemoryWorker.WorkerSupportsCancellation)
                            {
                                CCDBusReadMemoryWorker.CancelAsync();
                            }
                        }
                        else
                        {
                            CCDBusEcho = false;
                            CCDBusResponse = false;
                            UpdateTextBox(CCDBusReadMemoryInfoTextBox, " | error: skip response");
                        }
                    }
                    else
                    {
                        UpdateTextBox(CCDBusReadMemoryInfoTextBox, " | error: invalid length");
                    }
                }
            }

            if (MainForm.Packet.rx.source == (byte)Packet.Source.pcm) 
            {
                byte[] SCIBusPCMResponseBytes = MainForm.Packet.rx.payload.Skip(4).ToArray(); // skip 4 timestamp bytes

                if (SCIBusPCMReadMemoryCommand == 0x45) UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + "RX: " + Environment.NewLine + Util.ByteToHexStringSimple(SCIBusPCMResponseBytes));
                else UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + "RX: " + Util.ByteToHexStringSimple(SCIBusPCMResponseBytes));

                if (SCIBusPCMResponseBytes[0] == 0xFE)
                {
                    SCIBusPCMRxTimeoutTimer.Stop();
                }

                if (SCIBusPCMResponseBytes[0] == 0x46)
                {
                    if ((SCIBusPCMResponseBytes[1] == SCIBusPCMCurrentMemoryOffsetBytes[0]) && (SCIBusPCMResponseBytes[2] == SCIBusPCMCurrentMemoryOffsetBytes[1]) && (SCIBusPCMResponseBytes[3] == SCIBusPCMCurrentMemoryOffsetBytes[2])) // check if response has the offset we are currently waiting for
                    {
                        SCIBusPCMResponse = true;
                        SCIBusPCMRxTimeoutTimer.Stop();

                        UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, " | ok");
                    }
                    else
                    {
                        UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, " | error: invalid offset");
                        return;
                    }

                    ushort length = (ushort)((SCIBusPCMResponseBytes[4] << 8) | SCIBusPCMResponseBytes[5]);
                    SCIBusPCMMemoryArray = new byte[length];
                    Array.Copy(SCIBusPCMResponseBytes, 6, SCIBusPCMMemoryArray, 0, length);

                    SCIBusPCMReadMemoryCurrentOffsetTextBox.Text = Util.ByteToHexStringSimple(SCIBusPCMCurrentMemoryOffsetBytes);
                    SCIBusPCMReadMemoryCurrentOffsetTextBox.SelectionLength = 0;
                    SCIBusPCMReadMemoryValueTextBox.Text = Util.ByteToHexStringSimple(SCIBusPCMMemoryArray);
                    SCIBusPCMReadMemoryValueTextBox.SelectionLength = 0;

                    SCIBusPCMBytesReadCount += SCIBusPCMIncrement;
                    SCIBusPCMReadMemoryProgressLabel.Text = "Progress: " + (byte)((double)SCIBusPCMBytesReadCount / (double)SCIBusPCMTotalBytes * 100.0) + "% (" + SCIBusPCMBytesReadCount.ToString() + "/" + SCIBusPCMTotalBytes.ToString() + " bytes)";

                    using (BinaryWriter writer = new BinaryWriter(File.Open(SCIBusPCMMemoryBinaryFilename, FileMode.Append)))
                    {
                        writer.Write(SCIBusPCMMemoryArray); // write bytes to file
                        writer.Close();
                    }

                    SCIBusPCMCurrentMemoryOffset += SCIBusPCMIncrement;

                    SCIBusPCMCurrentMemoryOffsetBytes[0] = (byte)(SCIBusPCMCurrentMemoryOffset >> 16);
                    SCIBusPCMCurrentMemoryOffsetBytes[1] = (byte)(SCIBusPCMCurrentMemoryOffset >> 8);
                    SCIBusPCMCurrentMemoryOffsetBytes[2] = (byte)SCIBusPCMCurrentMemoryOffset;
                    SCIBusPCMTxPayload = new byte[6] { SCIBusPCMReadMemoryCommand, SCIBusPCMCurrentMemoryOffsetBytes[0], SCIBusPCMCurrentMemoryOffsetBytes[1], SCIBusPCMCurrentMemoryOffsetBytes[2], SCIBusPCMIncrementBytes[1], SCIBusPCMIncrementBytes[2] };

                    SCIBusPCMNextRequest = false;
                    SCIBusPCMNextRequestTimer.Stop();
                    SCIBusPCMNextRequestTimer.Start();
                }
                else if (SCIBusPCMResponseBytes[0] == SCIBusPCMReadMemoryCommand)
                {
                    if (SCIBusPCMResponseBytes.Length == (SCIBusPCMTxPayload.Length + 1))
                    {
                        if ((SCIBusPCMResponseBytes[1] == SCIBusPCMCurrentMemoryOffsetBytes[0]) && (SCIBusPCMResponseBytes[2] == SCIBusPCMCurrentMemoryOffsetBytes[1])) // check if response has the offset we are currently waiting for
                        {
                            if ((SCIBusPCMMemoryOffsetWidth == 16) || ((SCIBusPCMMemoryOffsetWidth == 24) && (SCIBusPCMResponseBytes[3] == SCIBusPCMCurrentMemoryOffsetBytes[2])))
                            {
                                SCIBusPCMResponse = true;
                                SCIBusPCMRxTimeoutTimer.Stop();

                                UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, " | ok");
                            }
                            else
                            {
                                UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, " | error: invalid offset");
                                return; // try again next time
                            }
                        }
                        else
                        {
                            UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, " | error: invalid offset");
                            return;
                        }

                        if (SCIBusPCMMemoryOffsetWidth == 16) SCIBusPCMMemoryValue = SCIBusPCMResponseBytes[3];
                        if (SCIBusPCMMemoryOffsetWidth == 24) SCIBusPCMMemoryValue = SCIBusPCMResponseBytes[4];

                        SCIBusPCMReadMemoryCurrentOffsetTextBox.Text = Util.ByteToHexStringSimple(SCIBusPCMCurrentMemoryOffsetBytes);
                        SCIBusPCMReadMemoryCurrentOffsetTextBox.SelectionLength = 0;
                        SCIBusPCMReadMemoryValueTextBox.Text = Util.ByteToHexStringSimple(new byte[1] { SCIBusPCMMemoryValue });
                        SCIBusPCMReadMemoryValueTextBox.SelectionLength = 0;

                        SCIBusPCMBytesReadCount++;
                        SCIBusPCMReadMemoryProgressLabel.Text = "Progress: " + (byte)((double)SCIBusPCMBytesReadCount / (double)SCIBusPCMTotalBytes * 100.0) + "% (" + SCIBusPCMBytesReadCount.ToString() + "/" + SCIBusPCMTotalBytes.ToString() + " bytes)";

                        using (BinaryWriter writer = new BinaryWriter(File.Open(SCIBusPCMMemoryBinaryFilename, FileMode.Append)))
                        {
                            writer.Write(SCIBusPCMMemoryValue); // write byte to file
                            writer.Close();
                        }

                        SCIBusPCMCurrentMemoryOffset += SCIBusPCMIncrement;

                        if (SCIBusPCMMemoryOffsetWidth == 16)
                        {
                            SCIBusPCMCurrentMemoryOffsetBytes[0] = (byte)(SCIBusPCMCurrentMemoryOffset >> 8);
                            SCIBusPCMCurrentMemoryOffsetBytes[1] = (byte)SCIBusPCMCurrentMemoryOffset;
                            SCIBusPCMTxPayload = new byte[3] { SCIBusPCMReadMemoryCommand, SCIBusPCMCurrentMemoryOffsetBytes[0], SCIBusPCMCurrentMemoryOffsetBytes[1] };
                        }

                        if (SCIBusPCMMemoryOffsetWidth == 24)
                        {
                            SCIBusPCMCurrentMemoryOffsetBytes[0] = (byte)(SCIBusPCMCurrentMemoryOffset >> 16);
                            SCIBusPCMCurrentMemoryOffsetBytes[1] = (byte)(SCIBusPCMCurrentMemoryOffset >> 8);
                            SCIBusPCMCurrentMemoryOffsetBytes[2] = (byte)SCIBusPCMCurrentMemoryOffset;
                            SCIBusPCMTxPayload = new byte[4] { SCIBusPCMReadMemoryCommand, SCIBusPCMCurrentMemoryOffsetBytes[0], SCIBusPCMCurrentMemoryOffsetBytes[1], SCIBusPCMCurrentMemoryOffsetBytes[2] };
                        }

                        SCIBusPCMNextRequest = false;
                        SCIBusPCMNextRequestTimer.Stop();
                        SCIBusPCMNextRequestTimer.Start();
                    }
                    else
                    {
                        UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, " | error: no response");
                    }
                }
                else
                {
                    UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, " | error: invalid command");
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

                if ((textBox.Name == "CCDBusReadMemoryInfoTextBox") && (CCDBusMemoryTextFilename != null)) File.AppendAllText(CCDBusMemoryTextFilename, text);
                if ((textBox.Name == "SCIBusPCMReadMemoryInfoTextBox") && (SCIBusPCMMemoryTextFilename != null)) File.AppendAllText(SCIBusPCMMemoryTextFilename, text);
            }
        }

        private void ReadMemoryForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (CCDBusReadMemoryWorker.IsBusy) CCDBusReadMemoryStopButton_Click(this, EventArgs.Empty);
            if (SCIBusPCMReadMemoryWorker.IsBusy) SCIBusPCMReadMemoryStopButton_Click(this, EventArgs.Empty);

            MainForm.Packet.PacketReceived -= PacketReceivedHandler; // unsubscribe from the OnPacketReceived event
        }

        #endregion
    }
}
