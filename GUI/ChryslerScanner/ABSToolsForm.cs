using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace ChryslerScanner
{
    public partial class ABSToolsForm : Form
    {
        private MainForm OriginalForm;

        private bool CCDBusAlive = false;
        private bool CCDBusEcho = false;
        private bool CCDBusResponse = false;
        private bool CCDBusNextRequest = false;
        private byte CCDBusRxRetryCount = 0;
        private byte CCDBusTxRetryCount = 0;
        private bool ABSToolsFinished = false;
        private bool CCDBusRxTimeout = false;
        private bool CCDBusTxTimeout = false;
        private byte[] CCDBusTxPayload;

        private bool ABSEnterMK20DiagModeRequested = false;
        private bool ABSExitMK20DiagModeRequested = false;
        private bool ABSSoftwareIDRequested = false;
        private byte ABSSoftwareID = 0;
        private bool ABSMK20Type = false;
        private byte ABSSoftwareVersion = 0;
        private bool ABSHardwareIDRequested = false;
        private byte DrivenWheels = 0;
        private string DrivenWheelsString = string.Empty;
        private byte PumpValveCount = 0;
        private bool ABSSoftwareChecksumRequested = false;
        private byte ABSSoftwareChecksum = 0;
        private bool ABSTattleTaleRequested = false;
        private string ABSTattleTaleStatus = string.Empty;
        private bool ABSPartNumberRequested = false;
        private byte[] ABSPartNumber;
        private byte ABSPartNumberStart = 0;
        private byte ABSPartNumberLength = 0;
        private byte ABSPartNumberPtr = 0;
        private bool ABSFaultCodesRequested = false;
        private byte ABSFaultCodePage = 0;
        private byte[] ABSFaultCodes = new byte[6];
        private bool ABSEraseFaultCodesRequested = false;

        private byte[] ABSEnterMK20DiagMode = new byte[6] { 0xB2, 0x43, 0x68, 0x02, 0x00, 0x00 };
        private byte[] ABSExitMK20DiagMode = new byte[6] { 0xB2, 0x43, 0x60, 0x00, 0x00, 0x00 };

        private Task CurrentTask = Task.None;

        private string ABSToolsLogFilename;

        private System.Timers.Timer CCDBusAliveTimer = new System.Timers.Timer();
        private System.Timers.Timer CCDBusNextRequestTimer = new System.Timers.Timer();
        private System.Timers.Timer CCDBusRxTimeoutTimer = new System.Timers.Timer();
        private System.Timers.Timer CCDBusTxTimeoutTimer = new System.Timers.Timer();
        private BackgroundWorker ABSToolsWorker = new BackgroundWorker();

        private enum Task
        {
            None,
            ReadID,
            ReadFaultCodes,
            EraseFaultCodes
        }

        public ABSToolsForm(MainForm IncomingForm)
        {
            OriginalForm = IncomingForm;
            InitializeComponent();
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            MainForm.Packet.PacketReceived += PacketReceivedHandler; // subscribe to the OnPacketReceived event
            OriginalForm.ChangeLanguage();

            ABSToolsLogFilename = @"LOG/ABS/abslog_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt";

            CCDBusAliveTimer.Elapsed += new ElapsedEventHandler(CCDBusAliveHandler);
            CCDBusAliveTimer.Interval = 1000; // ms
            CCDBusAliveTimer.AutoReset = true;
            CCDBusAliveTimer.Enabled = true;
            CCDBusAliveTimer.Start();

            CCDBusNextRequestTimer.Elapsed += new ElapsedEventHandler(CCDBusNextRequestHandler);
            CCDBusNextRequestTimer.Interval = 25; // ms
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

            ABSToolsWorker.WorkerReportsProgress = true;
            ABSToolsWorker.WorkerSupportsCancellation = true;
            ABSToolsWorker.DoWork += new DoWorkEventHandler(ABSTools_DoWork);
            ABSToolsWorker.ProgressChanged += new ProgressChangedEventHandler(ABSTools_ProgressChanged);
            ABSToolsWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(ABSTools_RunWorkerCompleted);

            UpdateTextBox(ABSToolsInfoTextBox, "Begin by identifying ABS module.");

            ActiveControl = ABSModuleIDReadButton;
        }

        private void CCDBusAliveHandler(object source, ElapsedEventArgs e) => CCDBusAlive = false;

        private void CCDBusNextRequestHandler(object source, ElapsedEventArgs e) => CCDBusNextRequest = true;

        private void CCDBusRxTimeoutHandler(object source, ElapsedEventArgs e) => CCDBusRxTimeout = true;

        private void CCDBusTxTimeoutHandler(object source, ElapsedEventArgs e) => CCDBusTxTimeout = true;

        private void ABSTools_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!ABSToolsFinished)
            {
                Thread.Sleep(1);

                if (ABSToolsWorker.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }

                while (!CCDBusNextRequest) // wait for next request message
                {
                    Thread.Sleep(1);

                    if (ABSToolsWorker.CancellationPending)
                    {
                        e.Cancel = true;
                        break;
                    }
                }

                CCDBusEcho = false;
                CCDBusResponse = false;
                CCDBusNextRequest = false;

                ABSToolsWorker.ReportProgress(0); // new messages are sent in the ProgressChanged event handler method

                while (!CCDBusEcho && !CCDBusTxTimeout)
                {
                    Thread.Sleep(1);

                    if (ABSToolsWorker.CancellationPending)
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

                        if (ABSToolsWorker.CancellationPending)
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

        private void ABSTools_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 0)
            {
                // Fill Packet class fields with data.
                MainForm.Packet.tx.bus = (byte)Packet.Bus.ccd;
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

        private void ABSTools_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled == true)
            {
                if (CCDBusRxTimeout) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "Timeout (RX).");
                if (CCDBusTxTimeout) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "Timeout (TX).");

                UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + Environment.NewLine + "Task cancelled.");

                if (!CCDBusAlive) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + Environment.NewLine + "CCD-bus communication lost.");
            }
            else
            {
                UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + Environment.NewLine + "Task finished.");
            }

            CurrentTask = Task.None;
            CCDBusRxTimeout = false;
            CCDBusRxTimeoutTimer.Stop();
            CCDBusTxTimeout = false;
            CCDBusTxTimeoutTimer.Stop();
            //CCDBusNextRequest = false;
            //CCDBusNextRequestTimer.Stop();
        }

        private void ABSModuleIDReadButton_Click(object sender, EventArgs e)
        {
            if (!ABSToolsWorker.IsBusy)
            {
                UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + Environment.NewLine + "Read software and hardware ID.");

                CCDBusEcho = false;
                CCDBusResponse = false;
                ABSToolsFinished = false;
                CCDBusNextRequest = true;
                CCDBusRxRetryCount = 0;
                CCDBusTxRetryCount = 0;
                CCDBusTxPayload = ABSEnterMK20DiagMode; // enter MK20 diagnostic mode, just in case

                CurrentTask = Task.ReadID;
                ABSToolsWorker.RunWorkerAsync();
            }
        }

        private void ReadFaultCodesButton_Click(object sender, EventArgs e)
        {
            if (!ABSToolsWorker.IsBusy)
            {
                UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + Environment.NewLine + "Read fault codes.");

                CCDBusEcho = false;
                CCDBusResponse = false;
                ABSToolsFinished = false;
                CCDBusNextRequest = true;
                CCDBusRxRetryCount = 0;
                CCDBusTxRetryCount = 0;
                ABSFaultCodePage = 0;
                CCDBusTxPayload = ABSEnterMK20DiagMode; // enter MK20 diagnostic mode, just in case

                CurrentTask = Task.ReadFaultCodes;
                ABSToolsWorker.RunWorkerAsync();
            }
        }

        private void EraseFaultCodesButton_Click(object sender, EventArgs e)
        {
            if (!ABSToolsWorker.IsBusy)
            {
                UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + Environment.NewLine + "Erase fault codes.");

                CCDBusEcho = false;
                CCDBusResponse = false;
                ABSToolsFinished = false;
                CCDBusNextRequest = true;
                CCDBusRxRetryCount = 0;
                CCDBusTxRetryCount = 0;
                CCDBusTxPayload = ABSEnterMK20DiagMode; // enter MK20 diagnostic mode, just in case

                CurrentTask = Task.EraseFaultCodes;
                ABSToolsWorker.RunWorkerAsync();
            }
        }

        private void PacketReceivedHandler(object sender, EventArgs e)
        {
            if (MainForm.Packet.rx.payload.Length > 4)
            {
                //ElapsedMillis = TimeSpan.FromMilliseconds((MainForm.Packet.rx.payload[0] << 24) | (MainForm.Packet.rx.payload[1] << 16) | (MainForm.Packet.rx.payload[2] << 8) | MainForm.Packet.rx.payload[3]);
                //Timestamp = DateTime.Today.Add(ElapsedMillis);
                //TimestampString = Timestamp.ToString("HH:mm:ss.fff");
                //UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "T: " + TimestampString);
            }

            if (MainForm.Packet.rx.bus == (byte)Packet.Bus.usb)
            {
                switch (MainForm.Packet.rx.command)
                {
                    default:
                        break;
                }
            }

            if (MainForm.Packet.rx.bus == (byte)Packet.Bus.ccd)
            {
                CCDBusAliveTimer.Stop();
                CCDBusAliveTimer.Start();
                CCDBusAlive = true;

                byte[] CCDBusResponseBytes = MainForm.Packet.rx.payload.Skip(4).ToArray(); // skip 4 timestamp bytes

                switch (CCDBusResponseBytes[0])
                {
                    case 0xB2: // request echoed back
                        if (CCDBusResponseBytes.Length < 6) break;

                        CCDBusEcho = true;
                        CCDBusTxTimeoutTimer.Stop();
                        CCDBusRxTimeout = false;
                        CCDBusRxTimeoutTimer.Start();

                        if ((CCDBusResponseBytes[1] == 0x43) && (CCDBusResponseBytes[2] == 0x68) && (CCDBusResponseBytes[3] == 0x02) && (CCDBusResponseBytes[4] == 0x00))
                        {
                            UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + Environment.NewLine + "Enter diagnostic mode.");

                            // Prepare next step.
                                
                            switch (CurrentTask)
                            {
                                case Task.ReadID:
                                    CCDBusTxPayload = new byte[6] { 0xB2, 0x43, 0x24, 0x00, 0x00, 0x19 }; // read software ID next
                                    break;
                                case Task.ReadFaultCodes:
                                    CCDBusTxPayload = new byte[6] { 0xB2, 0x43, 0x16, ABSFaultCodePage, 0x00, 0x00 }; // read fault code page 0
                                    break;
                                case Task.EraseFaultCodes:
                                    CCDBusTxPayload = new byte[6] { 0xB2, 0x43, 0x40, 0x00, 0x00, 0x00 }; // erase fault codes
                                    break;
                                default:
                                    break;
                            }

                            ABSEnterMK20DiagModeRequested = true;
                        }

                        if ((CCDBusResponseBytes[1] == 0x43) && (CCDBusResponseBytes[2] == 0x60) && (CCDBusResponseBytes[3] == 0x00) && (CCDBusResponseBytes[4] == 0x00))
                        {
                            UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + Environment.NewLine + "Exit diagnostic mode.");

                            ABSExitMK20DiagModeRequested = true;
                            CCDBusResponse = true;
                            ABSToolsFinished = true;
                        }

                        if ((CCDBusResponseBytes[1] == 0x43) && (CCDBusResponseBytes[2] == 0x24) && (CCDBusResponseBytes[3] == 0x00) && (CCDBusResponseBytes[4] == 0x00))
                        {
                            ABSSoftwareIDRequested = true;
                        }

                        if ((CCDBusResponseBytes[1] == 0x43) && (CCDBusResponseBytes[2] == 0x20) && (CCDBusResponseBytes[3] == (byte)(ABSPartNumberStart + ABSPartNumberPtr)) && (CCDBusResponseBytes[4] == 0x00))
                        {
                            ABSPartNumberRequested = true;
                        }

                        if ((CCDBusResponseBytes[1] == 0x43) && (CCDBusResponseBytes[2] == 0x24) && (CCDBusResponseBytes[3] == 0x02) && (CCDBusResponseBytes[4] == 0x00))
                        {
                            ABSSoftwareChecksumRequested = true;
                        }

                        if ((CCDBusResponseBytes[1] == 0x43) && (CCDBusResponseBytes[2] == 0x24) && (CCDBusResponseBytes[3] == 0x01) && (CCDBusResponseBytes[4] == 0x00))
                        {
                            ABSHardwareIDRequested = true;
                        }

                        if ((CCDBusResponseBytes[1] == 0x43) && (CCDBusResponseBytes[2] == 0x24) && (CCDBusResponseBytes[3] == 0x03) && (CCDBusResponseBytes[4] == 0x00))
                        {
                            ABSTattleTaleRequested = true;
                        }

                        if ((CCDBusResponseBytes[1] == 0x43) && (CCDBusResponseBytes[2] == 0x16) && (CCDBusResponseBytes[3] == ABSFaultCodePage) && (CCDBusResponseBytes[4] == 0x00))
                        {
                            ABSFaultCodesRequested = true;
                        }

                        if ((CCDBusResponseBytes[1] == 0x43) && (CCDBusResponseBytes[2] == 0x40) && (CCDBusResponseBytes[3] == 0x00) && (CCDBusResponseBytes[4] == 0x00))
                        {
                            ABSEraseFaultCodesRequested = true;
                        }
                        break;
                    case 0xF2: // response
                        if (CCDBusResponseBytes.Length < 6) break;

                        CCDBusRxTimeoutTimer.Stop();
                        CCDBusNextRequest = false;
                        CCDBusNextRequestTimer.Stop();
                        CCDBusNextRequestTimer.Start();

                        if (ABSEnterMK20DiagModeRequested && (CCDBusResponseBytes[1] == 0x43) && (CCDBusResponseBytes[2] == 0x68))
                        {
                            ABSEnterMK20DiagModeRequested = false;
                            UpdateTextBox(ABSToolsInfoTextBox, " OK."); // enter MK20 diagnostic mode
                        }

                        if (ABSExitMK20DiagModeRequested && (CCDBusResponseBytes[1] == 0x43) && (CCDBusResponseBytes[2] == 0x60))
                        {
                            ABSExitMK20DiagModeRequested = false;
                            UpdateTextBox(ABSToolsInfoTextBox, " OK."); // exit MK20 diagnostic mode
                            ABSToolsFinished = true;
                        }

                        if (ABSSoftwareIDRequested && (CCDBusResponseBytes[1] == 0x43) && (CCDBusResponseBytes[2] == 0x24))
                        {
                            ABSSoftwareIDRequested = false;
                            ABSSoftwareID = CCDBusResponseBytes[3];
                            ABSSoftwareVersion = CCDBusResponseBytes[4];

                            UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + Environment.NewLine + "Type: ");

                            switch (ABSSoftwareID)
                            {
                                case 0x80:
                                case 0x85:
                                    UpdateTextBox(ABSToolsInfoTextBox, "Teves/ATE Jeep MK4-G ABS");
                                    break;
                                case 0x19:
                                case 0x84:
                                    UpdateTextBox(ABSToolsInfoTextBox, "Teves/ATE LH MK4-G ABS+LTCS");
                                    break;
                                case 0x09:
                                case 0x82:
                                    UpdateTextBox(ABSToolsInfoTextBox, "Teves/ATE LH MK4-G ABS");
                                    break;
                                case 0x0A:
                                case 0x83:
                                    UpdateTextBox(ABSToolsInfoTextBox, "Teves/ATE NS MK4-G ABS");
                                    break;
                                case 0xBB:
                                    UpdateTextBox(ABSToolsInfoTextBox, "Teves/ATE Jeep MK20 ABS");
                                    break;
                                case 0xBE:
                                    UpdateTextBox(ABSToolsInfoTextBox, "Teves/ATE NS MK20 ABS+LTCS");
                                    break;
                                case 0xBC:
                                    UpdateTextBox(ABSToolsInfoTextBox, "Teves/ATE NS MK20 ABS");
                                    break;
                                case 0xBF:
                                    UpdateTextBox(ABSToolsInfoTextBox, "Teves/ATE JA/JX MK20 ABS+LTCS");
                                    break;
                                case 0xBD:
                                    UpdateTextBox(ABSToolsInfoTextBox, "Teves/ATE JA/JX MK20 ABS");
                                    break;
                                case 0xB1:
                                    UpdateTextBox(ABSToolsInfoTextBox, "Teves/ATE PL MK20 ABS");
                                    break;
                                default:
                                    UpdateTextBox(ABSToolsInfoTextBox, "unknown");
                                    break;
                            }

                            UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "Version: " + Util.ByteToHexString(new byte[1] { ABSSoftwareVersion }).Insert(1, "."));

                            // Prepare part number reading.
                                
                            switch (ABSSoftwareID)
                            {
                                case 0x80:
                                case 0x85:
                                case 0x19:
                                case 0x84:
                                case 0x09:
                                case 0x82:
                                case 0x0A:
                                case 0x83: // MK4-G
                                    ABSMK20Type = false;
                                    ABSPartNumber = new byte[8];
                                    ABSPartNumberStart = 1; // start offset
                                    ABSPartNumberLength = 8; // part number length
                                    ABSPartNumberPtr = 0;
                                    break;
                                case 0xBB:
                                case 0xBE:
                                case 0xBC:
                                case 0xBF:
                                case 0xBD:
                                case 0xB1: // MK20
                                default:
                                    ABSMK20Type = true;
                                    ABSPartNumber = new byte[10];
                                    ABSPartNumberStart = 0x79; // start offset
                                    ABSPartNumberLength = 10; // part number length
                                    ABSPartNumberPtr = 0;
                                    break;
                            }

                            CCDBusTxPayload = new byte[6] { 0xB2, 0x43, 0x20, (byte)(ABSPartNumberStart + ABSPartNumberPtr), 0x00, 0x00 }; // read part number next
                        }

                        if (ABSPartNumberRequested && (CCDBusResponseBytes[1] == 0x43) && (CCDBusResponseBytes[2] == 0x20))
                        {
                            ABSPartNumberRequested = false;

                            byte character = 0;

                            if (ABSMK20Type) character = CCDBusResponseBytes[3]; // first payload byte
                            else character = CCDBusResponseBytes[4]; // second payload byte

                            ABSPartNumber[ABSPartNumberPtr] = character; // save current character
                            ABSPartNumberPtr++; // next character

                            CCDBusTxPayload = new byte[6] { 0xB2, 0x43, 0x20, (byte)(ABSPartNumberStart + ABSPartNumberPtr), 0x00, 0x00 }; // read next part number character

                            if (ABSPartNumberPtr >= ABSPartNumberLength) // all part number characters have been read
                            {
                                UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "Part number: " + Encoding.ASCII.GetString(ABSPartNumber));

                                // Prepare software checksum reading.
                                    
                                CCDBusTxPayload = new byte[6] { 0xB2, 0x43, 0x24, 0x02, 0x00, 0x1B }; // read software checksum next
                            }
                        }

                        if (ABSSoftwareChecksumRequested && (CCDBusResponseBytes[1] == 0x43) && (CCDBusResponseBytes[2] == 0x24))
                        {
                            ABSSoftwareChecksumRequested = false;
                            ABSSoftwareChecksum = CCDBusResponseBytes[3];

                            UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "Checksum: " + Util.ByteToHexString(new byte[1] { ABSSoftwareChecksum }));

                            // Prepare hardware ID reading.
                                
                            CCDBusTxPayload = new byte[6] { 0xB2, 0x43, 0x24, 0x01, 0x00, 0x1A }; // read hardware ID next
                        }

                        if (ABSHardwareIDRequested && (CCDBusResponseBytes[1] == 0x43) && (CCDBusResponseBytes[2] == 0x24))
                        {
                            ABSHardwareIDRequested = false;
                            DrivenWheels = (byte)((CCDBusResponseBytes[3] & 0xC0 ) >> 6);
                            PumpValveCount = (byte)(CCDBusResponseBytes[4] & 0x0F);

                            switch (DrivenWheels)
                            {
                                case 1:
                                    DrivenWheelsString = "RWD";
                                    break;
                                case 2:
                                    DrivenWheelsString = "FWD";
                                    break;
                                case 3:
                                    DrivenWheelsString = "4WD";
                                    break;
                                default:
                                    DrivenWheelsString = "?WD (unknown)";
                                    break;
                            }

                            UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + Environment.NewLine + "Driven wheels: " + DrivenWheelsString);
                            UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "Pump valve count: " + PumpValveCount.ToString("0"));

                            // Prepare tattle tale reading.
                                
                            CCDBusTxPayload = new byte[6] { 0xB2, 0x43, 0x24, 0x03, 0x00, 0x1C }; // read tattle tale status next
                        }

                        if (ABSTattleTaleRequested && (CCDBusResponseBytes[1] == 0x43) && (CCDBusResponseBytes[2] == 0x24))
                        {
                            ABSTattleTaleRequested = false;
                                
                            if ((CCDBusResponseBytes[3] & 0x01) == 0x01)
                            {
                                ABSTattleTaleStatus = "were read.";
                            }
                            else
                            {
                                ABSTattleTaleStatus = "were not read.";
                            }

                            UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "Fault codes " + ABSTattleTaleStatus);

                            // Exit MK20 diagnostic mode.
                            CCDBusTxPayload = ABSExitMK20DiagMode;
                        }

                        if (ABSFaultCodesRequested && (CCDBusResponseBytes[1] == 0x43) && (CCDBusResponseBytes[2] == 0x16) && (CCDBusResponseBytes[3] == ABSFaultCodePage))
                        {
                            ABSFaultCodesRequested = false;

                            ABSFaultCodes[ABSFaultCodePage] = CCDBusResponseBytes[4]; // save fault code
                            ABSFaultCodePage++; // next fault code page

                            CCDBusTxPayload = new byte[6] { 0xB2, 0x43, 0x16, ABSFaultCodePage, 0x00, 0x00 }; // read next fault code page
                                
                            if (((ABSSoftwareID == 0x80) || (ABSSoftwareID == 0x85)) && (ABSFaultCodePage >= 2)) // Jeep MK4-G has 2 fault code pages
                            {
                                if ((ABSFaultCodes[0] == 0) && (ABSFaultCodes[1] == 0))
                                {
                                    UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + Environment.NewLine + "No fault code.");
                                }
                                else
                                {
                                    UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + Environment.NewLine + "Fault codes:" + Environment.NewLine);

                                    if (Util.IsBitSet(ABSFaultCodes[0], 7)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "RR WHEEL SPD / SIGNAL");
                                    if (Util.IsBitSet(ABSFaultCodes[0], 6)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "RR SENSOR / SIGNAL");
                                    if (Util.IsBitSet(ABSFaultCodes[0], 5)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "RL WHEEL SPD / SIGNAL");
                                    if (Util.IsBitSet(ABSFaultCodes[0], 4)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "RL SENSOR / SIGNAL");
                                    if (Util.IsBitSet(ABSFaultCodes[0], 3)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "FR WHEEL SPD / SIGNAL");
                                    if (Util.IsBitSet(ABSFaultCodes[0], 2)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "FR SENSOR / SIGNAL");
                                    if (Util.IsBitSet(ABSFaultCodes[0], 1)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "FL WHEEL SPD / SIGNAL");
                                    if (Util.IsBitSet(ABSFaultCodes[0], 0)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "FL SENSOR / SIGNAL");

                                    if (Util.IsBitSet(ABSFaultCodes[1], 7)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "OVERVOLTAGE");
                                    if (Util.IsBitSet(ABSFaultCodes[1], 6)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "BUS COMMUNICATION");
                                    if (Util.IsBitSet(ABSFaultCodes[1], 5)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "DISTURBANCE DETECT");
                                    if (Util.IsBitSet(ABSFaultCodes[1], 4)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "UNDERVOLTAGE");
                                    if (Util.IsBitSet(ABSFaultCodes[1], 3)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "G-SWITCH INPUT"); // only difference between Jeep MK4-G and MK20
                                    if (Util.IsBitSet(ABSFaultCodes[1], 2)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "ECU INTERNAL");
                                    if (Util.IsBitSet(ABSFaultCodes[1], 1)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "MAIN RELAY");
                                    if (Util.IsBitSet(ABSFaultCodes[1], 0)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "HYDRO PUMP CIRCUIT");
                                }

                                // Exit MK20 diagnostic mode.
                                CCDBusTxPayload = ABSExitMK20DiagMode;
                            }
                            else if (ABSMK20Type && (ABSFaultCodePage >= 2)) // MK20 types have 2 fault code pages
                            {
                                if ((ABSFaultCodes[0] == 0) && (ABSFaultCodes[1] == 0))
                                {
                                    UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + Environment.NewLine + "No fault code.");
                                }
                                else
                                {
                                    UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + Environment.NewLine + "Fault codes:" + Environment.NewLine);

                                    if (Util.IsBitSet(ABSFaultCodes[0], 7)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "RR WHEEL SPD / SIGNAL");
                                    if (Util.IsBitSet(ABSFaultCodes[0], 6)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "RR SENSOR / SIGNAL");
                                    if (Util.IsBitSet(ABSFaultCodes[0], 5)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "RL WHEEL SPD / SIGNAL");
                                    if (Util.IsBitSet(ABSFaultCodes[0], 4)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "RL SENSOR / SIGNAL");
                                    if (Util.IsBitSet(ABSFaultCodes[0], 3)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "FR WHEEL SPD / SIGNAL");
                                    if (Util.IsBitSet(ABSFaultCodes[0], 2)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "FR SENSOR / SIGNAL");
                                    if (Util.IsBitSet(ABSFaultCodes[0], 1)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "FL WHEEL SPD / SIGNAL");
                                    if (Util.IsBitSet(ABSFaultCodes[0], 0)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "FL SENSOR / SIGNAL");

                                    if (Util.IsBitSet(ABSFaultCodes[1], 7)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "OVERVOLTAGE");
                                    if (Util.IsBitSet(ABSFaultCodes[1], 6)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "BUS COMMUNICATION");
                                    if (Util.IsBitSet(ABSFaultCodes[1], 5)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "DISTURBANCE DETECT");
                                    if (Util.IsBitSet(ABSFaultCodes[1], 4)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "UNDERVOLTAGE");
                                    if (Util.IsBitSet(ABSFaultCodes[1], 3)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "WARNING LAMP CIRCUIT");  // only difference between Jeep MK4-G and MK20
                                    if (Util.IsBitSet(ABSFaultCodes[1], 2)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "ECU INTERNAL");
                                    if (Util.IsBitSet(ABSFaultCodes[1], 1)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "MAIN RELAY");
                                    if (Util.IsBitSet(ABSFaultCodes[1], 0)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "HYDRO PUMP CIRCUIT");
                                }

                                // Exit MK20 diagnostic mode.
                                CCDBusTxPayload = ABSExitMK20DiagMode;
                            }
                            else if (ABSFaultCodePage >= 6) // other MK4-G types have 5 fault code pages and 1 fault counter slot
                            {
                                if ((ABSFaultCodes[0] == 0) && (ABSFaultCodes[1] == 0) && (ABSFaultCodes[2] == 0) && (ABSFaultCodes[3] == 0) && (ABSFaultCodes[4] == 0))
                                {
                                    UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + Environment.NewLine + "No fault code.");
                                }
                                else
                                {
                                    UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + Environment.NewLine + "Fault codes:" + Environment.NewLine);

                                    if (Util.IsBitSet(ABSFaultCodes[0], 7)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "RR OUTLET VALVE");
                                    if (Util.IsBitSet(ABSFaultCodes[0], 6)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "RR INLET VALVE");
                                    if (Util.IsBitSet(ABSFaultCodes[0], 5)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "RL OUTLET VALVE");
                                    if (Util.IsBitSet(ABSFaultCodes[0], 4)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "RL INLET VALVE");
                                    if (Util.IsBitSet(ABSFaultCodes[0], 3)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "FR OUTLET VALVE");
                                    if (Util.IsBitSet(ABSFaultCodes[0], 2)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "FR INLET VALVE");
                                    if (Util.IsBitSet(ABSFaultCodes[0], 1)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "FL OUTLET VALVE");
                                    if (Util.IsBitSet(ABSFaultCodes[0], 0)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "FL INLET VALVE");

                                    if (Util.IsBitSet(ABSFaultCodes[1], 7)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "TRAC CNTL VALVE 1");
                                    if (Util.IsBitSet(ABSFaultCodes[1], 6)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "TRAC CNTL VALVE 2");
                                    if (Util.IsBitSet(ABSFaultCodes[1], 5)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "MAIN RELAY");
                                    if (Util.IsBitSet(ABSFaultCodes[1], 4)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "DIAGNOSTIC COMPARE");
                                    if (Util.IsBitSet(ABSFaultCodes[1], 3)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "VALVE BLOCK FEED");
                                    if (Util.IsBitSet(ABSFaultCodes[1], 2)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "PUMP MOTOR CIRCUIT");
                                    if (Util.IsBitSet(ABSFaultCodes[1], 1)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "CONTROLLER CONFIG");
                                    if (Util.IsBitSet(ABSFaultCodes[1], 0)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "PUMP NOT RUNNING");

                                    if (Util.IsBitSet(ABSFaultCodes[2], 7)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "RR CONTINUITY > 25");
                                    if (Util.IsBitSet(ABSFaultCodes[2], 6)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "RL CONTINUITY > 25");
                                    if (Util.IsBitSet(ABSFaultCodes[2], 5)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "FR CONTINUITY > 25");
                                    if (Util.IsBitSet(ABSFaultCodes[2], 4)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "FL CONTINUITY > 25");
                                    if (Util.IsBitSet(ABSFaultCodes[2], 3)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "RR TRIGGER MONITOR");
                                    if (Util.IsBitSet(ABSFaultCodes[2], 2)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "RL TRIGGER MONITOR");
                                    if (Util.IsBitSet(ABSFaultCodes[2], 1)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "FR TRIGGER MONITOR");
                                    if (Util.IsBitSet(ABSFaultCodes[2], 0)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "FL TRIGGER MONITOR");

                                    if (Util.IsBitSet(ABSFaultCodes[3], 7)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "OVERVOLTAGE");
                                    if (Util.IsBitSet(ABSFaultCodes[3], 6)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "UNDERVOLTAGE");
                                    if (Util.IsBitSet(ABSFaultCodes[3], 5)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "WARNING LAMP CIRCUIT");
                                    if (Util.IsBitSet(ABSFaultCodes[3], 4)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "G-SWITCH INPUT");
                                    if (Util.IsBitSet(ABSFaultCodes[3], 3)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "RR SPEED COMPARISON");
                                    if (Util.IsBitSet(ABSFaultCodes[3], 2)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "RL SPEED COMPARISON");
                                    if (Util.IsBitSet(ABSFaultCodes[3], 1)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "FR SPEED COMPARISON");
                                    if (Util.IsBitSet(ABSFaultCodes[3], 0)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "FL SPEED COMPARISON");

                                    if (Util.IsBitSet(ABSFaultCodes[4], 7)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "RR MISSING SIGNAL");
                                    if (Util.IsBitSet(ABSFaultCodes[4], 6)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "RL MISSING SIGNAL");
                                    if (Util.IsBitSet(ABSFaultCodes[4], 5)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "FR MISSING SIGNAL");
                                    if (Util.IsBitSet(ABSFaultCodes[4], 4)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "FL MISSING SIGNAL");
                                    if (Util.IsBitSet(ABSFaultCodes[4], 3)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "RR CONTINUITY < 25");
                                    if (Util.IsBitSet(ABSFaultCodes[4], 2)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "RL CONTINUITY < 25");
                                    if (Util.IsBitSet(ABSFaultCodes[4], 1)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "FR CONTINUITY < 25");
                                    if (Util.IsBitSet(ABSFaultCodes[4], 0)) UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "FL CONTINUITY < 25");

                                    byte NewFaultCodes = (byte)(32 - ABSFaultCodes[5]);

                                    UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + Environment.NewLine + NewFaultCodes + " since last fault.");
                                }

                                // Exit MK20 diagnostic mode.
                                CCDBusTxPayload = ABSExitMK20DiagMode;
                            }
                        }

                        if (ABSEraseFaultCodesRequested && (CCDBusResponseBytes[1] == 0x43) && (CCDBusResponseBytes[2] == 0x40))
                        {
                            ABSEraseFaultCodesRequested = false;

                            if ((CCDBusResponseBytes[3] == 0x00) && (CCDBusResponseBytes[4] == 0x00))
                            {
                                UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + Environment.NewLine + "Fault codes erased successfully.");
                            }
                            else
                            {
                                UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + Environment.NewLine + "Fault codes were not erased, try again.");
                            }

                            // Exit MK20 diagnostic mode.
                            CCDBusTxPayload = ABSExitMK20DiagMode;
                        }

                        if ((CCDBusResponseBytes[1] == 0x43) && (CCDBusResponseBytes[2] == 0xFF))
                        {
                            // Invalid command.

                            ABSEnterMK20DiagModeRequested = false;
                            ABSExitMK20DiagModeRequested = false;
                            ABSSoftwareIDRequested = false;
                            ABSPartNumberRequested = false;
                            ABSSoftwareChecksumRequested = false;
                            ABSHardwareIDRequested = false;
                            ABSTattleTaleRequested = false;
                            ABSFaultCodesRequested = false;
                            ABSEraseFaultCodesRequested = false;
                        }

                        CCDBusResponse = true;
                        break;
                    default:
                        break;
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

                File.AppendAllText(ABSToolsLogFilename, text);
            }
        }

        private void ABSToolsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (ABSToolsWorker.IsBusy) ABSToolsWorker.CancelAsync();
            MainForm.Packet.PacketReceived -= PacketReceivedHandler; // unsubscribe from the OnPacketReceived event
        }
    }
}
