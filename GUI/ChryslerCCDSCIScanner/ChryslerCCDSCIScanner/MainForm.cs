using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows.Forms;

namespace ChryslerCCDSCIScanner
{
    public partial class MainForm : Form
    {
        public bool SerialPortAvailable = false;
        public List<byte> bufferlist = new List<byte>();
        public bool ScannerFound = false;
        public string DateTimeNow;
        public static string USBLogFilename;
        public static string USBBinaryLogFilename;
        public string DiagnosticsSnapshotFilename;
        public string CCDLogFilename;
        public string CCDB2F2LogFilename;
        public string CCDEPROMTextFilename;
        public string CCDEPROMBinaryFilename;
        public string CCDEEPROMTextFilename;
        public string CCDEEPROMBinaryFilename;
        public string PCMLogFilename;
        public string PCMEPROMTextFilename;
        public string PCMEPROMBinaryFilename;
        public string PCMEEPROMTextFilename;
        public string PCMEEPROMBinaryFilename;
        public string TCMLogFilename;
        public string TCMEPROMTextFilename;
        public string TCMEPROMBinaryFilename;
        public string TCMEEPROMTextFilename;
        public string TCMEEPROMBinaryFilename;
        public string UpdateScannerFirmwareLogFilename;
        public static bool USBShowTraffic = true;
        public byte Units = 0;
        public bool Timeout = false;
        public int HeartbeatInterval = 5000;
        public int HeartbeatDuration = 50;
        public int RandomCCDMessageIntervalMin = 20;  // ms
        public int RandomCCDMessageIntervalMax = 100; // ms
        public int RepeatInterval = 100; // ms
        public int RepeatIncrement = 1;
        public bool RepeatIterate = false;
        public byte[] extEEPROMaddress = new byte[] { 0x00, 0x00 };
        public byte[] extEEPROMvalue = new byte[] { 0x00 };
        public bool SetCCDBus = true;
        public byte SetSCIBus = 2;
        public byte[] CCDBusMessageToSendStart = new byte[] { 0xB2, 0x20, 0x22, 0x00, 0x00, 0xF4 };
        public byte[] CCDBusMessageToSendEnd = new byte[] { 0xB2, 0x20, 0x22, 0xFF, 0xFE, 0xF1 };
        public byte[] SCIBusPCMMessageToSendStart = new byte[] { 0x10 };
        public byte[] SCIBusPCMMessageToSendEnd = new byte[] { 0x10 };
        public byte[] SCIBusTCMMessageToSendStart = new byte[] { 0x10 };
        public byte[] SCIBusTCMMessageToSendEnd = new byte[] { 0x10 };
        public List<byte> PacketBytes = new List<byte>();
        public byte PacketLengthHB = 0;
        public byte PacketLengthLB = 0;
        public byte PacketBytesChecksum = 0;

        public List<string> DiagnosticsTable = new List<string>();
        public List<string> OldDiagnosticsTable = new List<string>();
        public List<string> CCDbusMsgList = new List<string>(256);
        public List<byte> CCDBusIDList = new List<byte>(256);
        public List<string> SCIBusPCMMsgList = new List<string>(256);
        public List<byte> SCIBusPCMReqMsgList = new List<byte>(256);
        public List<byte> SCIBusPCMIDList = new List<byte>(256);
        public List<byte> SCIBusPCMSecondaryIDList = new List<byte>(256);
        public List<string> SCIBusTCMMsgList = new List<string>(256);
        public List<byte> SCIBusTCMReqMsgList = new List<byte>(256);
        public List<byte> SCIBusTCMIDList = new List<byte>(256);
        public List<byte> SCIBusTCMSecondaryIDList = new List<byte>(256);

        public string EmptyLine =                   "│                         │                                                     │                         │              │";
        private string CCDBusStateLineNA =          "│ CCD-BUS MODULES         │ STATE: N/A                                                                                    ";
        private string CCDBusStateLineDisabled =    "│ CCD-BUS MODULES         │ STATE: DISABLED                                                                               ";
        private string CCDBusStateLineEnabled =     "│ CCD-BUS MODULES         │ STATE: ENABLED @ 7812.5 BAUD | ID BYTES:  | # OF MESSAGES: RX= TX=                            ";
        private string SCIBusPCMStateLineNA =       "│ SCI-BUS ENGINE          │ STATE: N/A                                                                                    ";
        private string SCIBusPCMStateLineDisabled = "│ SCI-BUS ENGINE          │ STATE: DISABLED                                                                               ";
        private string SCIBusPCMStateLineEnabled =  "│ SCI-BUS ENGINE          │ STATE: ENABLED @  BAUD | CONFIGURATION:  | # OF MESSAGES: RX= TX=                             ";
        private string SCIBusTCMStateLineNA =       "│ SCI-BUS TRANSMISSION    │ STATE: N/A                                                                                    ";
        private string SCIBusTCMStateLineDisabled = "│ SCI-BUS TRANSMISSION    │ STATE: DISABLED                                                                               ";
        private string SCIBusTCMStateLineEnabled =  "│ SCI-BUS TRANSMISSION    │ STATE: ENABLED @  BAUD | CONFIGURATION:  | # OF MESSAGES: RX= TX=                             ";
        public string VIN = "-----------------"; // 17 characters

        public int CCDBusHeaderStart = 1;
        public int CCDBusListStart = 5; // row number
        public int CCDBusListEnd = 5;
        public int CCDBusB2Start = 7;
        public int CCDBusF2Start = 8;
        public int SCIBusPCMHeaderStart = 13;
        public int SCIBusPCMListStart = 17;
        public int SCIBusPCMListEnd = 17;
        public int SCIBusTCMHeaderStart = 22;
        public int SCIBusTCMListStart = 26;
        public int SCIBusTCMListEnd = 26;

        public int CCDBusMsgRxCount = 0;
        public int CCDBusMsgTxCount = 0;
        public int SCIBusPCMMsgRxCount = 0;
        public int SCIBusPCMMsgTxCount = 0;
        public int SCIBusTCMMsgRxCount = 0;
        public int SCIBusTCMMsgTxCount = 0;

        public bool CCDBusEnabled = false;
        public bool SCIBusPCMEnabled = false;
        public bool SCIBusTCMEnabled = false;

        public bool IDSorting = true; // Sort messages by ID-byte (CCD-bus and SCI-bus too)
        public bool CCDBusB2MsgPresent = false;
        public bool CCDBusF2MsgPresent = false;
        public int CCDBusIDByteNum = 0;
        public int SCIBusPCMIDByteNum = 0;
        public int SCIBusTCMIDByteNum = 0;

        public string SCIBusPCMHeaderBaud = String.Empty;
        public string SCIBusPCMHeaderConfiguration = String.Empty;
        public string SCIBusTCMHeaderBaud = String.Empty;
        public string SCIBusTCMHeaderConfiguration = String.Empty;

        public string CCDBusHeaderText = String.Empty;
        public string SCIBusPCMHeaderText = String.Empty;
        public string SCIBusTCMHeaderText = String.Empty;

        public double ImperialSlope = 0;
        public double ImperialOffset = 0;
        public double MetricSlope = 0;
        public double MetricOffset = 0;

        public static string UpdatePort = String.Empty;
        public static UInt64 OldUNIXTime = 0;
        public static UInt64 NewUNIXTime = 0;

        public byte CalculatedProgressBarValue = 0;
        public DateTime CalculatedRemainingTime;
        public bool MemoryReadFinished = false;
        public byte MemoryReadError = 0;

        public byte LastTargetIndex = 0; // Scanner
        public byte LastCommandIndex = 2; // Status
        public byte LastModeIndex = 0; // None

        public byte[] HandshakeRequest = new byte[] { 0x3D, 0x00, 0x02, 0x01, 0x00, 0x03 };
        public byte[] ExpectedHandshake = new byte[] { 0x3D, 0x00, 0x17, 0x01, 0x00, 0x43, 0x48, 0x52, 0x59, 0x53, 0x4C, 0x45, 0x52, 0x43, 0x43, 0x44, 0x53, 0x43, 0x49, 0x53, 0x43, 0x41, 0x4E, 0x4E, 0x45, 0x52, 0x37 };
        public byte[] StatusRequest = new byte[] { 0x3D, 0x00, 0x02, 0x02, 0x00, 0x04 };

        public enum TransmissionMethod
        {
            Hex = 0,
            Ascii = 1
        }
        public byte TM = 0;

        private const int SB_VERT = 0x0001;
        private const int WM_VSCROLL = 0x0115;
        private const int SB_THUMBPOSITION = 0x0004;
        private const int SB_BOTTOM = 0x0007;
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetScrollPos(IntPtr hWnd, int nBar);
        [DllImport("user32.dll")]
        private static extern int SetScrollPos(IntPtr hWnd, int nBar, int nPos, bool bRedraw);
        [DllImport("user32.dll")]
        private static extern bool PostMessageA(IntPtr hWnd, int nBar, int wParam, int lParam);
        [DllImport("user32.dll")]
        public static extern bool LockWindowUpdate(IntPtr hWndLock);

        AboutForm about;
        CheatSheetForm cheatsheet;
        BackgroundWorker bw = new BackgroundWorker();
        SerialPort Serial = new SerialPort();
        System.Timers.Timer TimeoutTimer = new System.Timers.Timer();
        System.Timers.Timer DiagnosticsTableUpdateTimer = new System.Timers.Timer();
        WebClient Downloader = new WebClient();
        Uri FlashFile = new Uri("https://github.com/laszlodaniel/ChryslerCCDSCIScanner/raw/master/Arduino/ChryslerCCDSCIScanner/ChryslerCCDSCIScanner.ino.mega.hex");
        Uri SourceFile = new Uri("https://github.com/laszlodaniel/ChryslerCCDSCIScanner/raw/master/Arduino/ChryslerCCDSCIScanner/main.h");

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            DiagnosticsGroupBox.Visible = false; // hide the expanded view components all at once
            this.Size = new Size(405, 650); // resize form to collapsed view
            this.CenterToScreen(); // put window at the center of the screen

            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); // set application icon
            if (!Directory.Exists("LOG")) Directory.CreateDirectory("LOG"); // create LOG directory if it doesn't exist
            if (!Directory.Exists("LOG/CCD")) Directory.CreateDirectory("LOG/CCD"); // create CCD directory inside LOG if it doesn't exist
            if (!Directory.Exists("LOG/Diagnostics")) Directory.CreateDirectory("LOG/Diagnostics");
            if (!Directory.Exists("LOG/PCM")) Directory.CreateDirectory("LOG/PCM");
            if (!Directory.Exists("LOG/TCM")) Directory.CreateDirectory("LOG/TCM");
            if (!Directory.Exists("LOG/USB")) Directory.CreateDirectory("LOG/USB");
            if (!Directory.Exists("ROMs")) Directory.CreateDirectory("ROMs");

            // Set logfile names inside the LOG directory
            DateTimeNow = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            USBLogFilename = @"LOG/USB/usblog_" + DateTimeNow + ".txt";
            USBBinaryLogFilename = @"LOG/USB/usblog_" + DateTimeNow + ".bin";

            CCDLogFilename = @"LOG/CCD/ccdlog_" + DateTimeNow + ".txt";
            CCDB2F2LogFilename = @"LOG/CCD/ccdb2f2log_" + DateTimeNow + ".txt";
            CCDEPROMTextFilename = @"ROMS/ccd_eprom_" + DateTimeNow + ".txt";
            CCDEPROMBinaryFilename = @"ROMs/ccd_eprom_" + DateTimeNow + ".bin";
            CCDEEPROMTextFilename = @"ROMs/ccd_eeprom_" + DateTimeNow + ".txt";
            CCDEEPROMBinaryFilename = @"ROMs/ccd_eeprom_" + DateTimeNow + ".bin";

            PCMLogFilename = @"LOG/PCM/pcmlog_" + DateTimeNow + ".txt";
            PCMEPROMTextFilename = @"ROMs/pcm_eprom_" + DateTimeNow + ".txt";
            PCMEPROMBinaryFilename = @"ROMs/pcm_eprom_" + DateTimeNow + ".bin";
            PCMEEPROMTextFilename = @"ROMs/pcm_eeprom_" + DateTimeNow + ".txt";
            PCMEEPROMBinaryFilename = @"ROMs/pcm_eeprom_" + DateTimeNow + ".bin";

            TCMLogFilename = @"LOG/TCM/tcmlog_" + DateTimeNow + ".txt";
            TCMEPROMTextFilename = @"ROMs/tcm_eprom_" + DateTimeNow + ".txt";
            TCMEPROMBinaryFilename = @"ROMs/tcm_eprom_" + DateTimeNow + ".bin";
            TCMEEPROMTextFilename = @"ROMs/tcm_eeprom_" + DateTimeNow + ".txt";
            TCMEEPROMBinaryFilename = @"ROMs/tcm_eeprom_" + DateTimeNow + ".bin";

            // Setup timeout timer
            TimeoutTimer.Elapsed += new ElapsedEventHandler(TimeoutHandler);
            TimeoutTimer.Interval = 2000; // ms
            TimeoutTimer.Enabled = false;

            // Setup diagnostics table update timer
            //DiagnosticsTableUpdateTimer.Elapsed += new ElapsedEventHandler(UpdateDiagnosticsListBox);
            //DiagnosticsTableUpdateTimer.Interval = 50; // ms, table refresh rate
            //DiagnosticsTableUpdateTimer.Enabled = true;

            // Fill the table with the default lines
            DiagnosticsTable.Add("┌─────────────────────────┐                                                                                               ");
            DiagnosticsTable.Add("│ CCD-BUS MODULES         │ STATE: N/A                                                                                    ");
            DiagnosticsTable.Add("├─────────────────────────┼─────────────────────────────────────────────────────┬─────────────────────────┬──────────────┐");
            DiagnosticsTable.Add("│ MESSAGE [HEX]           │ DESCRIPTION                                         │ VALUE                   │ UNIT         │");
            DiagnosticsTable.Add("╞═════════════════════════╪═════════════════════════════════════════════════════╪═════════════════════════╪══════════════╡");
            DiagnosticsTable.Add("│                         │                                                     │                         │              │");
            DiagnosticsTable.Add("├─────────────────────────┼─────────────────────────────────────────────────────┼─────────────────────────┼──────────────┤");
            DiagnosticsTable.Add("│ B2 -- -- -- -- --       │ DRB REQUEST                                         │                         │              │");
            DiagnosticsTable.Add("│ F2 -- -- -- -- --       │ DRB RESPONSE                                        │                         │              │");
            DiagnosticsTable.Add("└─────────────────────────┴─────────────────────────────────────────────────────┴─────────────────────────┴──────────────┘");
            DiagnosticsTable.Add("                                                                                                                          ");
            DiagnosticsTable.Add("                                                                                                                          ");
            DiagnosticsTable.Add("┌─────────────────────────┐                                                                                               ");
            DiagnosticsTable.Add("│ SCI-BUS ENGINE          │ STATE: N/A                                                                                    ");
            DiagnosticsTable.Add("├─────────────────────────┼─────────────────────────────────────────────────────┬─────────────────────────┬──────────────┐");
            DiagnosticsTable.Add("│ MESSAGE [HEX]           │ DESCRIPTION                                         │ VALUE                   │ UNIT         │");
            DiagnosticsTable.Add("╞═════════════════════════╪═════════════════════════════════════════════════════╪═════════════════════════╪══════════════╡");
            DiagnosticsTable.Add("│                         │                                                     │                         │              │");
            DiagnosticsTable.Add("└─────────────────────────┴─────────────────────────────────────────────────────┴─────────────────────────┴──────────────┘");
            DiagnosticsTable.Add("                                                                                                                          ");
            DiagnosticsTable.Add("                                                                                                                          ");
            DiagnosticsTable.Add("┌─────────────────────────┐                                                                                               ");
            DiagnosticsTable.Add("│ SCI-BUS TRANSMISSION    │ STATE: N/A                                                                                    ");
            DiagnosticsTable.Add("├─────────────────────────┼─────────────────────────────────────────────────────┬─────────────────────────┬──────────────┐");
            DiagnosticsTable.Add("│ MESSAGE [HEX]           │ DESCRIPTION                                         │ VALUE                   │ UNIT         │");
            DiagnosticsTable.Add("╞═════════════════════════╪═════════════════════════════════════════════════════╪═════════════════════════╪══════════════╡");
            DiagnosticsTable.Add("│                         │                                                     │                         │              │");
            DiagnosticsTable.Add("└─────────────────────────┴─────────────────────────────────────────────────────┴─────────────────────────┴──────────────┘");
            DiagnosticsTable.Add("                                                                                                                          ");
            DiagnosticsListBox.Items.Clear();
            DiagnosticsListBox.Items.AddRange(DiagnosticsTable.ToArray());

            USBCommunicationGroupBox.Enabled = false; // disable by default
            TargetComboBox.SelectedIndex = 0; // set combobox positions
            CommandComboBox.SelectedIndex = 2;
            ModeComboBox.SelectedIndex = 0;

            try // loading saved settings
            {
                MetricToolStripMenuItem.Checked = false;
                ImperialToolStripMenuItem.Checked = false;

                if ((string)Properties.Settings.Default["Units"] == "metric")
                {
                    MetricToolStripMenuItem.Checked = true;
                    Units = 0;
                }
                else if ((string)Properties.Settings.Default["Units"] == "imperial")
                {
                    ImperialToolStripMenuItem.Checked = true;
                    Units = 1;
                }

                if ((string)Properties.Settings.Default["TransmissionMethod"] == "hex") HexCommMethodRadioButton.Checked = true;
                else if ((string)Properties.Settings.Default["TransmissionMethod"] == "ascii") AsciiCommMethodRadioButton.Checked = true;

                if ((bool)Properties.Settings.Default["IncludeTimestamp"] == true) IncludeTimestampInLogFilesToolStripMenuItem.Checked = true;
                else if ((bool)Properties.Settings.Default["IncludeTimestamp"] == false) IncludeTimestampInLogFilesToolStripMenuItem.Checked = false;
            }
            catch
            {
                Util.UpdateTextBox(USBTextBox, "[INFO] Application config file is missing", null);
                Units = 0; // Metric units by default
            }

            //if (!File.Exists("VehicleProfiles.xml"))
            //{
            //    Util.UpdateTextBox(USBTextBox, "[INFO] Vehicle profiles file is missing", null);
            //}

            if (!Directory.Exists("Tools"))
            {
                Util.UpdateTextBox(USBTextBox, "[INFO] AVRDUDE is missing", null);
            }

            UpdateCOMPortList();
            ActiveControl = ConnectButton; // put focus on the connect button
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.DoEvents();
            if (ScannerFound) ConnectButton.PerformClick(); // disconnect first
            if (Serial.IsOpen) Serial.Close();
            
        }

        private void TimeoutHandler(object source, ElapsedEventArgs e)
        {
            Timeout = true;
        }

        private void UpdateCOMPortList()
        {
            COMPortsComboBox.Items.Clear(); // clear combobox
            string[] ports = SerialPort.GetPortNames(); // get available ports

            if (ports.Length > 0)
            {
                COMPortsComboBox.Items.AddRange(ports);
                SerialPortAvailable = true;
                ConnectButton.Enabled = true;
            }
            else
            {
                COMPortsComboBox.Items.Add("N/A");
                SerialPortAvailable = false;
                ConnectButton.Enabled = false;
                Util.UpdateTextBox(USBTextBox, "[INFO] No scanner available", null);
            }

            COMPortsComboBox.SelectedIndex = 0; // select first available item
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            if (!ScannerFound) // connect
            {
                UpdateCOMPortList();

                if (SerialPortAvailable)
                {
                    byte[] buffer = new byte[2048];
                    byte ConnectionCounter = 0;

                    while (ConnectionCounter < 5) // try connecting to the scanner 5 times, then give up
                    {
                        ConnectButton.Enabled = false; // no double-click

                        if (Serial.IsOpen) Serial.Close(); // can't overwrite fields if serial port is open
                        Serial.PortName = COMPortsComboBox.Text;
                        Serial.BaudRate = 250000;
                        Serial.DataBits = 8;
                        Serial.StopBits = StopBits.One;
                        Serial.Parity = Parity.None;
                        Serial.ReadTimeout = 500;
                        Serial.WriteTimeout = 500;

                        Util.UpdateTextBox(USBTextBox, "[INFO] Connecting to " + Serial.PortName, null);

                        try
                        {
                            Serial.Open(); // open current serial port
                        }
                        catch
                        {
                            Util.UpdateTextBox(USBTextBox, "[INFO] " + Serial.PortName + " is opened by another application", null);
                            Util.UpdateTextBox(USBTextBox, "[INFO] Scanner not found at " + Serial.PortName, null);
                            break;
                        }

                        if (Serial.IsOpen)
                        {
                            Serial.DiscardInBuffer();
                            Serial.DiscardOutBuffer();
                            Serial.BaseStream.Flush();

                            Util.UpdateTextBox(USBTextBox, "[<-TX] Handshake request (" + Serial.PortName + ")", HandshakeRequest);
                            Serial.Write(HandshakeRequest, 0, HandshakeRequest.Length);

                            Timeout = false;
                            TimeoutTimer.Enabled = true;

                            while (!Timeout)
                            {
                                if (Serial.BytesToRead > 26)
                                {
                                    Serial.Read(buffer, 0, 27);
                                    break;
                                }
                            }

                            TimeoutTimer.Enabled = false;

                            Serial.DiscardInBuffer();
                            Serial.DiscardOutBuffer();
                            Serial.BaseStream.Flush();

                            if (Timeout)
                            {
                                Util.UpdateTextBox(USBTextBox, "[INFO] Device is not responding at " + Serial.PortName, null);
                                Timeout = false;
                                Serial.Close();
                                ConnectionCounter++; // increase counter value and try again
                            }
                            else
                            {
                                ScannerFound = Util.CompareArrays(buffer, ExpectedHandshake, 0, ExpectedHandshake.Length);

                                if (ScannerFound)
                                {
                                    Util.UpdateTextBox(USBTextBox, "[RX->] Handshake response", ExpectedHandshake);
                                    Util.UpdateTextBox(USBTextBox, "[INFO] Handshake OK: CHRYSLERCCDSCISCANNER", null);
                                    Util.UpdateTextBox(USBTextBox, "[INFO] Scanner connected (" + Serial.PortName + ")", null);
                                    UpdatePort = Serial.PortName;
                                    ConnectButton.Text = "Disconnect";
                                    ConnectButton.Enabled = true;
                                    UpdateScannerFirmwareToolStripMenuItem.Enabled = true;
                                    USBCommunicationGroupBox.Enabled = true;

                                    if (HexCommMethodRadioButton.Checked)
                                    {
                                        USBSendComboBox.DropDownStyle = ComboBoxStyle.DropDown;
                                        CommandComboBox_SelectedIndexChanged(CommandComboBox, EventArgs.Empty); // raise event manually
                                    }
                                    else if (AsciiCommMethodRadioButton.Checked)
                                    {
                                        USBSendComboBox.DropDownStyle = ComboBoxStyle.DropDown;
                                        USBSendComboBox.Text = ">";
                                    }

                                    Serial.DataReceived += new SerialDataReceivedEventHandler(SerialDataReceivedHandler);
                                    Serial.Write(StatusRequest, 0, StatusRequest.Length); // get scanner status immediately
                                    break; // exit while-loop
                                }
                                else
                                {
                                    Util.UpdateTextBox(USBTextBox, "[RX->] Data received", buffer.Take(10).ToArray());
                                    Util.UpdateTextBox(USBTextBox, "[INFO] Handshake ERROR: " + Encoding.ASCII.GetString(buffer, 5, 21), null);
                                    Util.UpdateTextBox(USBTextBox, "[INFO] Scanner not found at " + Serial.PortName, null);
                                    Serial.Close();
                                    ConnectionCounter++; // increase counter value and try again
                                }
                            }
                        }
                    }

                    ConnectButton.Enabled = true;
                }
            }
            else // disconnect
            {
                if (Serial.IsOpen && ScannerFound)
                {
                    Serial.DiscardInBuffer();
                    Serial.DiscardOutBuffer();
                    Serial.BaseStream.Flush();
                    Serial.Close();
                    ScannerFound = false;
                    ConnectButton.Text = "Connect";
                    USBSendComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                    USBCommunicationGroupBox.Enabled = false;
                    UpdateScannerFirmwareToolStripMenuItem.Enabled = false;
                    Util.UpdateTextBox(USBTextBox, "[INFO] Scanner disconnected (" + Serial.PortName + ")", null);
                    Serial.DataReceived -= new SerialDataReceivedEventHandler(SerialDataReceivedHandler);
                }
            }
        }

        private void SerialDataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            int DataLength = sp.BytesToRead;

            // This approach enables reading multiple broken transmissions
            for (int i = 0; i < DataLength; i++)
            {
                try
                {
                    bufferlist.Add((byte)sp.ReadByte());
                }
                catch
                {
                    Util.UpdateTextBox(USBTextBox, "[INFO] Serial read error", null);
                    break;
                }
            }

            // Multiple packets are handled one after another in this while-loop
            while (bufferlist.Count > 0)
            {
                byte PacketID = bufferlist[0];

                switch (PacketID)
                {
                    case 0x3D: // "="
                        if (bufferlist.Count < 3) break; // wait for the length bytes

                        int PacketLength = (bufferlist[1] << 8) + bufferlist[2];
                        int FullPacketLength = PacketLength + 4;

                        if (bufferlist.Count < FullPacketLength) break; // wait for the rest of the bytes to arrive

                        byte[] Packet = new byte[FullPacketLength];
                        int PayloadLength = PacketLength - 2;
                        byte[] Payload = new byte[PayloadLength];
                        int ChecksumLocation = PacketLength + 3;
                        byte DataCode = 0;
                        byte Source = 0;
                        byte Target = 0;
                        byte Command = 0;
                        byte SubDataCode = 0;
                        byte Checksum = 0;
                        byte CalculatedChecksum = 0;

                        Array.Copy(bufferlist.ToArray(), 0, Packet, 0, Packet.Length);

                        Checksum = Packet[ChecksumLocation]; // get packet checksum byte

                        for (int i = 1; i < ChecksumLocation; i++)
                        {
                            CalculatedChecksum += Packet[i]; // calculate checksum
                        }

                        if (CalculatedChecksum == Checksum) // verify checksum
                        {
                            DataCode = Packet[3];
                            Source = (byte)((DataCode >> 6) & 0x03);
                            Target = (byte)((DataCode >> 4) & 0x03);
                            Command = (byte)(DataCode & 0x0F);
                            SubDataCode = Packet[4];

                            if (PayloadLength > 0) // copy Payload bytes if available
                            {
                                Array.Copy(Packet, 5, Payload, 0, Payload.Length);
                            }

                            int location = 0;
                            int secondary_location = 0;
                            byte IDByte = 0;
                            byte SecondaryIDByte = 0;

                            switch (Source)
                            {
                                case 0x00: // USB - message is coming from the scanner directly, no need to analyze target, it has to be an external computer
                                    switch (Command)
                                    {
                                        case 0x00: // Reset
                                            if (Payload.Length > 0)
                                            {
                                                if (Payload[0] == 0) Util.UpdateTextBox(USBTextBox, "[RX->] Scanner is resetting, please wait", Packet);
                                                else if (Payload[0] == 1) Util.UpdateTextBox(USBTextBox, "[RX->] Scanner is ready to accept instructions", Packet);
                                            }
                                            else
                                            {
                                                Util.UpdateTextBox(USBTextBox, "[RX->] Data received", Packet);
                                            }
                                            break;
                                        case 0x01: // Handshake
                                            if (Payload.Length > 0)
                                            {
                                                if (Encoding.ASCII.GetString(Payload) == "CHRYSLERCCDSCISCANNER")
                                                {
                                                    Util.UpdateTextBox(USBTextBox, "[RX->] Handshake response (" + Serial.PortName + ")", Packet);
                                                    Util.UpdateTextBox(USBTextBox, "[INFO] Handshake OK: " + Encoding.ASCII.GetString(Payload), null);
                                                    ScannerFound = true;
                                                }
                                                else
                                                {
                                                    Util.UpdateTextBox(USBTextBox, "[RX->] Handshake response (" + Serial.PortName + ")", Packet);
                                                    Util.UpdateTextBox(USBTextBox, "[INFO] Handshake ERROR", null);
                                                    ScannerFound = false;
                                                }
                                            }
                                            else
                                            {
                                                Util.UpdateTextBox(USBTextBox, "[RX->] Data received", Packet);
                                            }
                                            break;
                                        case 0x02: // Status
                                            if (Payload.Length > 66)
                                            {
                                                string AVRSignature = Util.ByteToHexString(Payload, 0, 3);
                                                if ((Payload[0] == 0x1E) && (Payload[1] == 0x98) && (Payload[2] == 0x01)) AVRSignature += " (ATmega2560)";
                                                else AVRSignature += " (unknown)";
                                                string HardwareVersion = "V" + ((Payload[3] << 8 | Payload[4]) / 100.00).ToString("0.00").Replace(",", ".");
                                                DateTime HardwareDate = Util.UnixTimeStampToDateTime(Payload[5] << 56 | Payload[6] << 48 | Payload[7] << 40 | Payload[8] << 32 | Payload[9] << 24 | Payload[10] << 16 | Payload[11] << 8 | Payload[12]);
                                                DateTime AssemblyDate = Util.UnixTimeStampToDateTime(Payload[13] << 56 | Payload[14] << 48 | Payload[15] << 40 | Payload[16] << 32 | Payload[17] << 24 | Payload[18] << 16 | Payload[19] << 8 | Payload[20]);
                                                DateTime FirmwareDate = Util.UnixTimeStampToDateTime(Payload[21] << 56 | Payload[22] << 48 | Payload[23] << 40 | Payload[24] << 32 | Payload[25] << 24 | Payload[26] << 16 | Payload[27] << 8 | Payload[28]);
                                                OldUNIXTime = (UInt64)(Payload[21] << 56 | Payload[22] << 48 | Payload[23] << 40 | Payload[24] << 32 | Payload[25] << 24 | Payload[26] << 16 | Payload[27] << 8 | Payload[28]);
                                                string HardwareDateString = HardwareDate.ToString("yyyy.MM.dd HH:mm:ss");
                                                string AssemblyDateString = AssemblyDate.ToString("yyyy.MM.dd HH:mm:ss");
                                                string FirmwareDateString = FirmwareDate.ToString("yyyy.MM.dd HH:mm:ss");
                                                string extEEPROMPresent = String.Empty;
                                                if (Payload[29] == 0x01) extEEPROMPresent += "yes";
                                                else extEEPROMPresent += "no";
                                                string extEEPROMChecksum = Util.ByteToHexString(Payload, 30, 31);
                                                if (Payload[30] == Payload[31]) extEEPROMChecksum += " (OK)";
                                                else extEEPROMChecksum += " (ERROR [" + Util.ByteToHexString(Payload, 31, 32) + "])";
                                                TimeSpan ElapsedMillis = TimeSpan.FromMilliseconds(Payload[32] << 24 | Payload[33] << 16 | Payload[34] << 8 | Payload[35]);
                                                DateTime MicrocontrollerTimestamp = DateTime.Today.Add(ElapsedMillis);
                                                string MicrocontrollerTimestampString = MicrocontrollerTimestamp.ToString("HH:mm:ss.fff");
                                                string FreeRAMString = ((Payload[36] << 8) | Payload[37]).ToString() + " free of 8192 bytes";
                                                string ConnectedToVehicle = String.Empty;
                                                if (Payload[38] == 0x01) ConnectedToVehicle = "yes";
                                                else ConnectedToVehicle = "no";
                                                string BatteryVoltageString = ((Payload[39] << 8 | Payload[40]) / 100.00).ToString("0.00").Replace(",", ".") + " V";
                                                string CCDBusStateString = String.Empty;
                                                if (Payload[41] == 0x01)
                                                {
                                                    CCDBusStateString = "enabled";
                                                    CCDBusEnabled = true;
                                                }
                                                else
                                                {
                                                    CCDBusStateString = "disabled";
                                                    CCDBusEnabled = false;
                                                }
                                                CCDBusMsgRxCount = (Payload[42] << 24) | (Payload[43] << 16) | (Payload[44] << 8) | Payload[45];
                                                CCDBusMsgTxCount = (Payload[46] << 24) | (Payload[47] << 16) | (Payload[48] << 8) | Payload[49];
                                                string CCDBusRxMessagesString = CCDBusMsgRxCount.ToString();
                                                string CCDBusTxMessagesString = CCDBusMsgTxCount.ToString();
                                                string SCIBusPCMStateString = String.Empty;
                                                string SCIBusPCMConfigurationString = String.Empty;
                                                string SCIBusPCMSpeedString = String.Empty;
                                                string SCIBusTCMStateString = String.Empty;
                                                string SCIBusTCMConfigurationString = String.Empty;
                                                string SCIBusTCMSpeedString = String.Empty;
                                                if ((Payload[50] & 0x40) != 0)
                                                {
                                                    SCIBusPCMStateString = "enabled";
                                                    SCIBusPCMEnabled = true;
                                                    if ((Payload[50] & 0x20) != 0)
                                                    {
                                                        SCIBusPCMConfigurationString = "B";
                                                        SCIBusPCMHeaderConfiguration = "B";
                                                    }
                                                    else
                                                    {
                                                        SCIBusPCMConfigurationString = "A";
                                                        SCIBusPCMHeaderConfiguration = "A";
                                                    }
                                                    if ((Payload[50] & 0x10) != 0)
                                                    {
                                                        SCIBusPCMSpeedString = "62500 baud";
                                                        SCIBusPCMHeaderBaud = "62500";
                                                    }
                                                    else
                                                    {
                                                        SCIBusPCMSpeedString = "7812.5 baud";
                                                        SCIBusPCMHeaderBaud = "7812.5";
                                                    }
                                                }
                                                else
                                                {
                                                    SCIBusPCMStateString = "disabled";
                                                    SCIBusPCMEnabled = false;
                                                    SCIBusPCMConfigurationString = "-";
                                                    SCIBusPCMHeaderConfiguration = String.Empty;
                                                    SCIBusPCMSpeedString = "-";
                                                    SCIBusPCMHeaderBaud = String.Empty;
                                                }
                                                if ((Payload[50] & 0x04) != 0)
                                                {
                                                    SCIBusTCMStateString = "enabled";
                                                    SCIBusTCMEnabled = true;
                                                    if ((Payload[50] & 0x02) != 0)
                                                    {
                                                        SCIBusTCMConfigurationString = "B";
                                                        SCIBusTCMHeaderConfiguration = "B";
                                                    }
                                                    else
                                                    {
                                                        SCIBusTCMConfigurationString = "A";
                                                        SCIBusTCMHeaderConfiguration = "A";
                                                    }
                                                    if ((Payload[50] & 0x01) != 0)
                                                    {
                                                        SCIBusTCMSpeedString = "62500 baud";
                                                        SCIBusTCMHeaderBaud = "62500";
                                                    }
                                                    else
                                                    {
                                                        SCIBusTCMSpeedString = "7812.5 baud";
                                                        SCIBusTCMHeaderBaud = "7812.5";
                                                    }
                                                }
                                                else
                                                {
                                                    SCIBusTCMStateString = "disabled";
                                                    SCIBusTCMEnabled = false;
                                                    SCIBusTCMConfigurationString = "-";
                                                    SCIBusTCMHeaderConfiguration = String.Empty;
                                                    SCIBusTCMSpeedString = "-";
                                                    SCIBusTCMHeaderBaud = String.Empty;
                                                }
                                                SCIBusPCMMsgRxCount = (Payload[51] << 24) | (Payload[52] << 16) | (Payload[53] << 8) | Payload[54];
                                                SCIBusPCMMsgTxCount = (Payload[55] << 24) | (Payload[56] << 16) | (Payload[57] << 8) | Payload[58];
                                                SCIBusTCMMsgRxCount = (Payload[59] << 24) | (Payload[60] << 16) | (Payload[61] << 8) | Payload[62];
                                                SCIBusTCMMsgTxCount = (Payload[63] << 24) | (Payload[64] << 16) | (Payload[65] << 8) | Payload[66];
                                                string SCIBusPCMRxMessagesString = SCIBusPCMMsgRxCount.ToString();
                                                string SCIBusPCMTxMessagesString = SCIBusPCMMsgTxCount.ToString();
                                                string SCIBusTCMRxMessagesString = SCIBusTCMMsgRxCount.ToString();
                                                string SCIBusTCMTxMessagesString = SCIBusTCMMsgTxCount.ToString();

                                                Util.UpdateTextBox(USBTextBox, "[RX->] Status response", Packet);
                                                Util.UpdateTextBox(USBTextBox, "[INFO] Scanner status:" + Environment.NewLine +
                                                                                "       - AVR signature: " + AVRSignature + Environment.NewLine +
                                                                                "       - hardware ver.: " + HardwareVersion + Environment.NewLine +
                                                                                "       - hardware date: " + HardwareDateString + Environment.NewLine +
                                                                                "       - assembly date: " + AssemblyDateString + Environment.NewLine +
                                                                                "       - firmware date: " + FirmwareDateString + Environment.NewLine +
                                                                                "       - extEEPROM present: " + extEEPROMPresent + Environment.NewLine +
                                                                                "       - extEEPROM checksum: " + extEEPROMChecksum + Environment.NewLine +
                                                                                "       - timestamp: " + MicrocontrollerTimestampString + Environment.NewLine +
                                                                                "       - RAM usage: " + FreeRAMString + Environment.NewLine +
                                                                                "       - connected to vehicle: " + ConnectedToVehicle + Environment.NewLine +
                                                                                "       - battery voltage: " + BatteryVoltageString + Environment.NewLine +
                                                                                "       CCD-bus status:" + Environment.NewLine +
                                                                                "       - state: " + CCDBusStateString + Environment.NewLine +
                                                                                "       - speed: 7812.5 baud" + Environment.NewLine +
                                                                                "       - messages received: " + CCDBusRxMessagesString + Environment.NewLine +
                                                                                "       - messages transmitted: " + CCDBusTxMessagesString + Environment.NewLine +
                                                                                "       SCI-bus status:" + Environment.NewLine +
                                                                                "       - PCM:" + Environment.NewLine +
                                                                                "         - state: " + SCIBusPCMStateString + Environment.NewLine +
                                                                                "         - configuration: " + SCIBusPCMConfigurationString + Environment.NewLine +
                                                                                "         - speed: " + SCIBusPCMSpeedString + Environment.NewLine +
                                                                                "         - messages received: " + SCIBusPCMRxMessagesString + Environment.NewLine +
                                                                                "         - messages transmitted: " + SCIBusPCMTxMessagesString + Environment.NewLine +
                                                                                "       - TCM:" + Environment.NewLine +
                                                                                "         - state: " + SCIBusTCMStateString + Environment.NewLine +
                                                                                "         - configuration: " + SCIBusTCMConfigurationString + Environment.NewLine +
                                                                                "         - speed: " + SCIBusTCMSpeedString + Environment.NewLine +
                                                                                "         - messages received: " + SCIBusTCMRxMessagesString + Environment.NewLine +
                                                                                "         - messages transmitted: " + SCIBusTCMTxMessagesString, null);
                                            }
                                            else
                                            {
                                                Util.UpdateTextBox(USBTextBox, "[RX->] Data received", Packet);
                                            }
                                            break;
                                        case 0x03: // Settings
                                            switch (SubDataCode)
                                            {
                                                case 0x01: // Heartbeat
                                                    if (Payload.Length > 3)
                                                    {
                                                        Util.UpdateTextBox(USBTextBox, "[RX->] Heartbeat settings changed", Packet);
                                                        if ((Payload[0] << 8 | Payload[1]) > 0)
                                                        {
                                                            string interval = (Payload[0] << 8 | Payload[1]).ToString() + " ms";
                                                            string duration = (Payload[2] << 8 | Payload[3]).ToString() + " ms";
                                                            Util.UpdateTextBox(USBTextBox, "[INFO] Heartbeat enabled:" + Environment.NewLine +
                                                                                            "       Interval: " + interval + Environment.NewLine +
                                                                                            "       Duration: " + duration, null);
                                                        }
                                                        else
                                                        {
                                                            Util.UpdateTextBox(USBTextBox, "[INFO] Heartbeat disabled", null);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Util.UpdateTextBox(USBTextBox, "[RX->] Data received", Packet);
                                                    }
                                                    break;
                                                case 0x02: // Set CCD-bus
                                                    if (Payload.Length > 0)
                                                    {
                                                        Util.UpdateTextBox(USBTextBox, "[RX->] CCD-bus settings changed", Packet);
                                                        if (Payload[0] == 0)
                                                        {
                                                            Util.UpdateTextBox(USBTextBox, "[INFO] CCD-bus chip disabled", null);
                                                            CCDBusEnabled = false;
                                                        }
                                                        else if (Payload[0] == 1)
                                                        {
                                                            Util.UpdateTextBox(USBTextBox, "[INFO] CCD-bus chip enabled @ 7812.5 baud", null);
                                                            CCDBusEnabled = true;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Util.UpdateTextBox(USBTextBox, "[RX->] Data received", Packet);
                                                    }
                                                    break;
                                                case 0x03: // Set SCI-bus
                                                    if (Payload.Length > 0)
                                                    {
                                                        string configuration = String.Empty;
                                                        configuration += "[INFO] PCM settings:" + Environment.NewLine;

                                                        if (((Payload[0] >> 6) & 0x01) == 0)
                                                        {
                                                            configuration += "       - state: disabled" + Environment.NewLine;
                                                            SCIBusPCMEnabled = false;
                                                        }
                                                        else
                                                        {
                                                            configuration += "       - state: enabled" + Environment.NewLine;
                                                            SCIBusPCMEnabled = true;
                                                        }

                                                        if (((Payload[0] >> 5) & 0x01) == 0)
                                                        {
                                                            if (((Payload[0] >> 6) & 0x01) == 0)
                                                            {
                                                                configuration += "       - configuration: -" + Environment.NewLine;
                                                                SCIBusPCMHeaderConfiguration = String.Empty;
                                                            }
                                                            else
                                                            {
                                                                configuration += "       - configuration: A" + Environment.NewLine;
                                                                SCIBusPCMHeaderConfiguration = "A";
                                                            }
                                                        }
                                                        else
                                                        {
                                                            if (((Payload[0] >> 6) & 0x01) == 0)
                                                            {
                                                                configuration += "       - configuration: -" + Environment.NewLine;
                                                                SCIBusPCMHeaderConfiguration = String.Empty;
                                                            }
                                                            else
                                                            {
                                                                configuration += "       - configuration: B" + Environment.NewLine;
                                                                SCIBusPCMHeaderConfiguration = "B";
                                                            }
                                                        }

                                                        if (((Payload[0] >> 4) & 0x01) == 0)
                                                        {
                                                            if (((Payload[0] >> 6) & 0x01) == 0)
                                                            {
                                                                configuration += "       - speed: -" + Environment.NewLine;
                                                                SCIBusPCMHeaderBaud = String.Empty;
                                                            }
                                                            else
                                                            {
                                                                configuration += "       - speed: 7812.5 baud" + Environment.NewLine;
                                                                SCIBusPCMHeaderBaud = "7812.5";
                                                            }
                                                        }
                                                        else
                                                        {
                                                            if (((Payload[0] >> 6) & 0x01) == 0)
                                                            {
                                                                configuration += "       - speed: -" + Environment.NewLine;
                                                                SCIBusPCMHeaderBaud = String.Empty;
                                                            }
                                                            else
                                                            {
                                                                configuration += "       - speed: 62500 baud" + Environment.NewLine;
                                                                SCIBusPCMHeaderBaud = "62500";
                                                            }
                                                        }

                                                        configuration += "       TCM settings:" + Environment.NewLine;

                                                        if (((Payload[0] >> 2) & 0x01) == 0)
                                                        {
                                                            configuration += "       - state: disabled" + Environment.NewLine;
                                                            SCIBusTCMEnabled = false;
                                                        }
                                                        else
                                                        {
                                                            configuration += "       - state: enabled" + Environment.NewLine;
                                                            SCIBusTCMEnabled = true;
                                                        }

                                                        if (((Payload[0] >> 1) & 0x01) == 0)
                                                        {
                                                            if (((Payload[0] >> 2) & 0x01) == 0)
                                                            {
                                                                configuration += "       - configuration: -" + Environment.NewLine;
                                                                SCIBusTCMHeaderConfiguration = String.Empty;
                                                            }
                                                            else
                                                            {
                                                                configuration += "       - configuration: A" + Environment.NewLine;
                                                                SCIBusTCMHeaderConfiguration = "A";
                                                            }
                                                        }
                                                        else
                                                        {
                                                            if (((Payload[0] >> 2) & 0x01) == 0)
                                                            {
                                                                configuration += "       - configuration: -" + Environment.NewLine;
                                                                SCIBusTCMHeaderConfiguration = String.Empty;
                                                            }
                                                            else
                                                            {
                                                                configuration += "       - configuration: B" + Environment.NewLine;
                                                                SCIBusTCMHeaderConfiguration = "B";
                                                            }
                                                        }

                                                        if ((Payload[0] & 0x01) == 0)
                                                        {
                                                            if (((Payload[0] >> 2) & 0x01) == 0)
                                                            {
                                                                configuration += "       - speed: -";
                                                                SCIBusTCMHeaderBaud = String.Empty;
                                                            }
                                                            else
                                                            {
                                                                configuration += "       - speed: 7812.5 baud";
                                                                SCIBusTCMHeaderBaud = "7812.5";
                                                            }
                                                        }
                                                        else
                                                        {
                                                            if (((Payload[0] >> 2) & 0x01) == 0)
                                                            {
                                                                configuration += "       - speed: -";
                                                                SCIBusTCMHeaderBaud = String.Empty;
                                                            }
                                                            else
                                                            {
                                                                configuration += "       - speed: 62500 baud";
                                                                SCIBusTCMHeaderBaud = "62500";
                                                            }
                                                        }

                                                        Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus settings changed", Packet);
                                                        Util.UpdateTextBox(USBTextBox, configuration, null);
                                                    }
                                                    else
                                                    {
                                                        Util.UpdateTextBox(USBTextBox, "[RX->] Data received", Packet);
                                                    }
                                                    break;
                                                case 0x04: // Repeated message behavior
                                                    if (Payload.Length > 4)
                                                    {
                                                        string bus = String.Empty;
                                                        switch (Payload[0])
                                                        {
                                                            case 0x01:
                                                                bus = "CCD-bus";
                                                                break;
                                                            case 0x02:
                                                                bus = "SCI-bus (PCM)";
                                                                break;
                                                            case 0x03:
                                                                bus = "SCI-bus (TCM)";
                                                                break;
                                                            default:
                                                                bus = "Unknown";
                                                                break;
                                                        }
                                                        string repeat_interval = (Payload[1] << 8 | Payload[2]).ToString("0") + " ms";
                                                        string increment = (Payload[3] << 8 | Payload[4]).ToString("0");
                                                        Util.UpdateTextBox(USBTextBox, "[RX->] Repeated message behavior changed", Packet);
                                                        Util.UpdateTextBox(USBTextBox, "[INFO] Repeated message behavior settings:" + Environment.NewLine +
                                                                                       "       Bus: " + bus + Environment.NewLine +
                                                                                       "       Interval: " + repeat_interval + Environment.NewLine +
                                                                                       "       Increment: " + increment, null);
                                                    }
                                                    else
                                                    {
                                                        Util.UpdateTextBox(USBTextBox, "[RX->] Data received", Packet);
                                                    }
                                                    break;
                                                case 0x05: // Set LCD
                                                    break;
                                                default:
                                                    break;
                                            }
                                            break;
                                        case 0x05: // Response
                                            switch (SubDataCode)
                                            {
                                                case 0x01: // Hardware/Firmware info
                                                    if (Payload.Length > 25)
                                                    {
                                                        string HardwareVersionString = "V" + ((Payload[0] << 8 | Payload[1]) / 100.00).ToString("0.00").Replace(",", ".");
                                                        DateTime _HardwareDate = Util.UnixTimeStampToDateTime(Payload[2] << 56 | Payload[3] << 48 | Payload[4] << 40 | Payload[5] << 32 | Payload[6] << 24 | Payload[7] << 16 | Payload[8] << 8 | Payload[9]);
                                                        DateTime _AssemblyDate = Util.UnixTimeStampToDateTime(Payload[10] << 56 | Payload[11] << 48 | Payload[12] << 40 | Payload[13] << 32 | Payload[14] << 24 | Payload[15] << 16 | Payload[16] << 8 | Payload[17]);
                                                        DateTime _FirmwareDate = Util.UnixTimeStampToDateTime(Payload[18] << 56 | Payload[19] << 48 | Payload[20] << 40 | Payload[21] << 32 | Payload[22] << 24 | Payload[23] << 16 | Payload[24] << 8 | Payload[25]);
                                                        string _HardwareDateString = _HardwareDate.ToString("yyyy.MM.dd HH:mm:ss");
                                                        string _AssemblyDateString = _AssemblyDate.ToString("yyyy.MM.dd HH:mm:ss");
                                                        string _FirmwareDateString = _FirmwareDate.ToString("yyyy.MM.dd HH:mm:ss");
                                                        Util.UpdateTextBox(USBTextBox, "[RX->] Hardware/Firmware information response", Packet);
                                                        Util.UpdateTextBox(USBTextBox, "[INFO] Hardware ver.: " + HardwareVersionString + Environment.NewLine +
                                                                                        "       Hardware date: " + _HardwareDateString + Environment.NewLine +
                                                                                        "       Assembly date: " + _AssemblyDateString + Environment.NewLine +
                                                                                        "       Firmware date: " + _FirmwareDateString, null);
                                                        OldUNIXTime = (UInt64)(Payload[18] << 56 | Payload[19] << 48 | Payload[20] << 40 | Payload[21] << 32 | Payload[22] << 24 | Payload[23] << 16 | Payload[24] << 8 | Payload[25]);
                                                    }
                                                    else
                                                    {
                                                        Util.UpdateTextBox(USBTextBox, "[RX->] Data received", Packet);
                                                    }
                                                    break;
                                                case 0x02: // Timestamp
                                                    if (Payload.Length > 3)
                                                    {
                                                        TimeSpan ElapsedTime = TimeSpan.FromMilliseconds(Payload[0] << 24 | Payload[1] << 16 | Payload[2] << 8 | Payload[3]);
                                                        DateTime Timestamp = DateTime.Today.Add(ElapsedTime);
                                                        string TimestampString = Timestamp.ToString("HH:mm:ss.fff");
                                                        Util.UpdateTextBox(USBTextBox, "[RX->] Timestamp response", Packet);
                                                        Util.UpdateTextBox(USBTextBox, "[INFO] Timestamp: " + TimestampString, null);
                                                    }
                                                    else
                                                    {
                                                        Util.UpdateTextBox(USBTextBox, "[RX->] Data received", Packet);
                                                    }
                                                    break;
                                                case 0x03: // Battery voltage
                                                    if (Payload.Length > 1)
                                                    {
                                                        string _BatteryVoltageString = ((Payload[0] << 8 | Payload[1]) / 100.00).ToString("0.00").Replace(",", ".") + " V";
                                                        Util.UpdateTextBox(USBTextBox, "[RX->] Battery voltage response", Packet);
                                                        Util.UpdateTextBox(USBTextBox, "[INFO] Battery voltage: " + _BatteryVoltageString, null);
                                                    }
                                                    else
                                                    {
                                                        Util.UpdateTextBox(USBTextBox, "[RX->] Data received", Packet);
                                                    }
                                                    break;
                                                case 0x04: // External EEPROM checksum
                                                    if (Payload.Length > 2)
                                                    {
                                                        Util.UpdateTextBox(USBTextBox, "[RX->] External EEPROM checksum response", Packet);
                                                        if (Payload[0] == 0x01) // External EEPROM present
                                                        {
                                                            string ExternalEEPROMChecksumReading = Util.ByteToHexString(Payload, 1, Payload.Length - 1);
                                                            string ExternalEEPROMChecksumCalculated = Util.ByteToHexString(Payload, 2, Payload.Length);
                                                            if (Payload[1] == Payload[2]) // if checksum reading and checksum calculation is the same
                                                            {
                                                                Util.UpdateTextBox(USBTextBox, "[INFO] External EEPROM checksum: " + ExternalEEPROMChecksumReading + " (OK)", null);
                                                            }
                                                            else
                                                            {
                                                                Util.UpdateTextBox(USBTextBox, "[INFO] External EEPROM checksum ERROR: " + Environment.NewLine +
                                                                                                "       - reads as: " + ExternalEEPROMChecksumReading + Environment.NewLine +
                                                                                                "       - calculated: " + ExternalEEPROMChecksumCalculated, null);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            Util.UpdateTextBox(USBTextBox, "[INFO] No external EEPROM found", null);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Util.UpdateTextBox(USBTextBox, "[RX->] Data received", Packet);
                                                    }
                                                    break;
                                                default:
                                                    Util.UpdateTextBox(USBTextBox, "[RX->] Data received", Packet);
                                                    break;
                                            }
                                            break;
                                        case 0x06: // Send message
                                            switch (SubDataCode)
                                            {
                                                case 0x01:
                                                    if (Payload.Length > 0)
                                                    {
                                                        if (Payload[0] == 0x01) Util.UpdateTextBox(USBTextBox, "[RX->] CCD-bus message TX stopped", Packet);
                                                        else if (Payload[0] == 0x02) Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (PCM) message TX stopped", Packet);
                                                        else if (Payload[0] == 0x03) Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (TCM) message TX stopped", Packet);
                                                    }
                                                    else
                                                    {
                                                        Util.UpdateTextBox(USBTextBox, "[RX->] Data received", Packet);
                                                    }
                                                    break;
                                                case 0x02:
                                                    if (Payload.Length > 0)
                                                    {
                                                        if (Payload[0] == 0x01) Util.UpdateTextBox(USBTextBox, "[RX->] CCD-bus message prepared for TX", Packet);
                                                        else if (Payload[0] == 0x02) Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (PCM) message prepared for TX", Packet);
                                                        else if (Payload[0] == 0x03) Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (TCM) message prepared for TX", Packet);
                                                    }
                                                    else
                                                    {
                                                        Util.UpdateTextBox(USBTextBox, "[RX->] Data received", Packet);
                                                    }
                                                    break;
                                                case 0x03:
                                                    if (Payload.Length > 0)
                                                    {
                                                        if (Payload[0] == 0x01) Util.UpdateTextBox(USBTextBox, "[RX->] CCD-bus message TX started", Packet);
                                                        else if (Payload[0] == 0x02) Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (PCM) message TX started", Packet);
                                                        else if (Payload[0] == 0x03) Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (TCM) message TX started", Packet);
                                                    }
                                                    else
                                                    {
                                                        Util.UpdateTextBox(USBTextBox, "[RX->] Data received", Packet);
                                                    }
                                                    break;
                                                default:
                                                    Util.UpdateTextBox(USBTextBox, "[RX->] Data received", Packet);
                                                    break;
                                            }
                                            break;
                                        case 0x0F: // OK/Error
                                            switch (SubDataCode)
                                            {
                                                case 0x00:
                                                    Util.UpdateTextBox(USBTextBox, "[RX->] OK", Packet);
                                                    break;
                                                case 0x01:
                                                    Util.UpdateTextBox(USBTextBox, "[RX->] Error: invalid length", Packet);
                                                    break;
                                                case 0x02:
                                                    Util.UpdateTextBox(USBTextBox, "[RX->] Error: invalid dc command", Packet);
                                                    break;
                                                case 0x03:
                                                    Util.UpdateTextBox(USBTextBox, "[RX->] Error: invalid sub-data code", Packet);
                                                    break;
                                                case 0x04:
                                                    Util.UpdateTextBox(USBTextBox, "[RX->] Error: invalid Payload value(s)", Packet);
                                                    break;
                                                case 0x05:
                                                    Util.UpdateTextBox(USBTextBox, "[RX->] Error: invalid checksum", Packet);
                                                    break;
                                                case 0x06:
                                                    Util.UpdateTextBox(USBTextBox, "[RX->] Error: packet timeout occured", Packet);
                                                    break;
                                                case 0x07:
                                                    Util.UpdateTextBox(USBTextBox, "[RX->] Error: buffer overflow", Packet);
                                                    break;
                                                case 0xF8:
                                                    Util.UpdateTextBox(USBTextBox, "[RX->] Error: " + Util.ByteToHexString(Payload, 0, 1) + " SCI-bus RAM-table no response", Packet);
                                                    break;
                                                case 0xF9:
                                                    Util.UpdateTextBox(USBTextBox, "[RX->] Error: " + Util.ByteToHexString(Payload, 0, 1) + " SCI-bus RAM-table invalid", Packet);
                                                    break;
                                                case 0xFA:
                                                    Util.UpdateTextBox(USBTextBox, "[RX->] Error: no response from SCI-bus (" + Util.ByteToHexString(Payload, 0, 2) + ")", Packet);
                                                    break;
                                                case 0xFB:
                                                    Util.UpdateTextBox(USBTextBox, "[RX->] Error: external EEPROM not found", Packet);
                                                    break;
                                                case 0xFC:
                                                    Util.UpdateTextBox(USBTextBox, "[RX->] Error: external EEPROM reading not possible", Packet);
                                                    break;
                                                case 0xFD:
                                                    Util.UpdateTextBox(USBTextBox, "[RX->] Error: external EEPROM writing not possible", Packet);
                                                    break;
                                                case 0xFE:
                                                    Util.UpdateTextBox(USBTextBox, "[RX->] Error: internal error", Packet);
                                                    break;
                                                case 0xFF:
                                                    Util.UpdateTextBox(USBTextBox, "[RX->] Error: fatal error", Packet);
                                                    break;
                                            }
                                            break;
                                        default:
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Data received", Packet);
                                            break;
                                    }

                                    if (CCDBusEnabled)
                                    {
                                        CCDBusHeaderText = CCDBusStateLineEnabled;
                                        CCDBusHeaderText = CCDBusHeaderText.Replace("ID BYTES: ", "ID BYTES: " + CCDBusIDByteNum.ToString()).
                                                                            Replace("# OF MESSAGES: RX=", "# OF MESSAGES: RX=" + CCDBusMsgRxCount.ToString()).
                                                                            Replace(" TX=", " TX=" + CCDBusMsgTxCount.ToString());
                                        CCDBusHeaderText = Util.Truncate(CCDBusHeaderText, EmptyLine.Length); // Replacing strings causes the base string to grow so cut it back to stay in line
                                    }
                                    else
                                    {
                                        CCDBusHeaderText = CCDBusStateLineDisabled;
                                    }
                                    DiagnosticsTable.RemoveAt(CCDBusHeaderStart);
                                    DiagnosticsTable.Insert(CCDBusHeaderStart, CCDBusHeaderText);

                                    if (SCIBusPCMEnabled)
                                    {
                                        SCIBusPCMHeaderText = SCIBusPCMStateLineEnabled;
                                        SCIBusPCMHeaderText = SCIBusPCMHeaderText.Replace("@  BAUD", "@ " + SCIBusPCMHeaderBaud + " BAUD").
                                                                            Replace("CONFIGURATION: ", "CONFIGURATION: " + SCIBusPCMHeaderConfiguration).
                                                                            Replace("# OF MESSAGES: RX=", "# OF MESSAGES: RX=" + SCIBusPCMMsgRxCount.ToString()).
                                                                            Replace(" TX=", " TX=" + SCIBusPCMMsgTxCount.ToString());
                                        SCIBusPCMHeaderText = Util.Truncate(SCIBusPCMHeaderText, EmptyLine.Length); // Replacing strings causes the base string to grow so cut it back to stay in line
                                    }
                                    else
                                    {
                                        SCIBusPCMHeaderText = SCIBusPCMStateLineDisabled;
                                    }
                                    DiagnosticsTable.RemoveAt(SCIBusPCMHeaderStart);
                                    DiagnosticsTable.Insert(SCIBusPCMHeaderStart, SCIBusPCMHeaderText);

                                    if (SCIBusTCMEnabled)
                                    {
                                        SCIBusTCMHeaderText = SCIBusTCMStateLineEnabled;
                                        SCIBusTCMHeaderText = SCIBusTCMHeaderText.Replace("@  BAUD", "@ " + SCIBusTCMHeaderBaud + " BAUD").
                                                                            Replace("CONFIGURATION: ", "CONFIGURATION: " + SCIBusTCMHeaderConfiguration).
                                                                            Replace("# OF MESSAGES: RX=", "# OF MESSAGES: RX=" + SCIBusTCMMsgRxCount.ToString()).
                                                                            Replace(" TX=", " TX=" + SCIBusTCMMsgTxCount.ToString());
                                        SCIBusTCMHeaderText = Util.Truncate(SCIBusTCMHeaderText, EmptyLine.Length); // Replacing strings causes the base string to grow so cut it back to stay in line
                                    }
                                    else
                                    {
                                        SCIBusTCMHeaderText = SCIBusTCMStateLineDisabled;
                                    }
                                    DiagnosticsTable.RemoveAt(SCIBusTCMHeaderStart);
                                    DiagnosticsTable.Insert(SCIBusTCMHeaderStart, SCIBusTCMHeaderText);

                                    UpdateDiagnosticsListBox();
                                    break;
                                case 0x01: // CCD-bus
                                    StringBuilder ccdlistitem = new StringBuilder(EmptyLine); // start with a pre-defined empty line
                                    string ccdpackettoinsert = String.Empty;
                                    string ccddescriptiontoinsert = String.Empty;
                                    string ccdvaluetoinsert = String.Empty;
                                    string ccdunittoinsert = String.Empty;
                                    byte[] ccdtimestamp = new byte[4];
                                    byte[] ccdmessage = new byte[Payload.Length - 4];
                                    Array.Copy(Payload, 0, ccdtimestamp, 0, 4); // copy timestamp only
                                    Array.Copy(Payload, 4, ccdmessage, 0, Payload.Length - 4); // copy message only

                                    if (ccdmessage.Length < 9) // max 8 byte fits the message column
                                    {
                                        ccdpackettoinsert = Util.ByteToHexString(ccdmessage, 0, ccdmessage.Length) + " ";
                                    }
                                    else // Trim message (display 7 bytes only) and insert two dots at the end indicating there's more to it
                                    {
                                        ccdpackettoinsert = Util.ByteToHexString(ccdmessage, 0, 7) + " .. ";
                                    }

                                    // Insert message hex bytes in the line
                                    ccdlistitem.Remove(2, ccdpackettoinsert.Length); // remove as much whitespaces as the length of the message
                                    ccdlistitem.Insert(2, ccdpackettoinsert); // insert message where whitespaces were

                                    // First byte in a CCD-bus message is the ID-byte
                                    IDByte = ccdmessage[0];

                                    switch (IDByte)
                                    {
                                        case 0x0C: // battery voltage, oil pressure, coolant temperature, intake temperature
                                            if (ccdmessage.Length > 5)
                                            {
                                                if (Units == 1) // Imperial
                                                {
                                                    string battery_voltage = (ccdmessage[1] * 0.125D).ToString("0.0").Replace(",", ".");
                                                    string oil_pressure = (ccdmessage[2] * 0.5D).ToString("0.0").Replace(",", ".");
                                                    string coolant_temperature = Math.Round((ccdmessage[3] * 1.8D) - 83.2D).ToString("0");
                                                    string intake_air_temperature = Math.Round((ccdmessage[4] * 1.8D) - 83.2D).ToString("0");
                                                    ccddescriptiontoinsert = "BATTERY: " + battery_voltage + " V | " + "OIL: " + oil_pressure + " PSI | " + "COOLANT: " + coolant_temperature + " °F";
                                                    ccdvaluetoinsert = "IAT: " + intake_air_temperature + " °F";
                                                }
                                                if (Units == 0) // Metric
                                                {
                                                    string battery_voltage = (ccdmessage[1] * 0.125D).ToString("0.0").Replace(",", ".");
                                                    string oil_pressure = (ccdmessage[2] * 0.5D * 6.894757D).ToString("0.0").Replace(",", ".");
                                                    string coolant_temperature = Math.Round((((ccdmessage[3] * 1.8D) - 83.2D) * 0.555556D) - 17.77778D).ToString("0");
                                                    string intake_air_temperature = Math.Round((((ccdmessage[4] * 1.8D) - 83.2D) * 0.555556D) - 17.77778D).ToString("0");
                                                    ccddescriptiontoinsert = "BATTERY: " + battery_voltage + " V | " + "OIL: " + oil_pressure + " KPA | " + "COOLANT: " + coolant_temperature + " °C";
                                                    ccdvaluetoinsert = "IAT: " + intake_air_temperature + " °C";
                                                }

                                                ccdunittoinsert = String.Empty;
                                            }
                                            break;
                                        case 0x24:
                                            if (ccdmessage.Length > 3)
                                            {
                                                ccddescriptiontoinsert = "VEHICLE SPEED";

                                                if (Units == 1) // Imperial
                                                {
                                                    ccdvaluetoinsert = ccdmessage[1].ToString("0");
                                                    ccdunittoinsert = "MPH";
                                                }
                                                if (Units == 0) // Metric
                                                {
                                                    ccdvaluetoinsert = ccdmessage[2].ToString("0");
                                                    ccdunittoinsert = "KM/H";
                                                }
                                            }
                                            break;
                                        case 0x25:
                                            if (ccdmessage.Length > 2)
                                            {
                                                ccddescriptiontoinsert = "FUEL TANK LEVEL";

                                                ImperialSlope = 0.3922D;
                                                ccdvaluetoinsert = (ccdmessage[1] * ImperialSlope).ToString("0.0").Replace(",", ".");
                                                ccdunittoinsert = "%";
                                            }
                                            break;
                                        case 0x29:
                                            if (ccdmessage.Length > 3)
                                            {
                                                ccddescriptiontoinsert = "LAST ENGINE SHUTDOWN";

                                                if (ccdmessage[1] < 10) ccdvaluetoinsert = "0";
                                                ccdvaluetoinsert += ccdmessage[1].ToString("0") + ":";
                                                if (ccdmessage[2] < 10) ccdvaluetoinsert += "0";
                                                ccdvaluetoinsert += ccdmessage[2].ToString("0");
                                                ccdunittoinsert = "HOUR:MINUTE";
                                            }
                                            break;
                                        case 0x35:
                                            if (ccdmessage.Length > 3)
                                            {
                                                ccddescriptiontoinsert = "US/METRIC STATUS | SEAT-BELT: ";

                                                if (((ccdmessage[1] & 0x02) >> 1) == 1) ccdvaluetoinsert = "METRIC";
                                                else ccdvaluetoinsert = "US";

                                                if (((ccdmessage[1] & 0x04) >> 2) == 1) ccddescriptiontoinsert += "BUCKLED";
                                                else ccddescriptiontoinsert += "UNBUCKLED";

                                                ccdunittoinsert = String.Empty;
                                            }
                                            break;
                                        case 0x3A:
                                            if (ccdmessage.Length > 3)
                                            {
                                                ccddescriptiontoinsert = "INSTRUMENT CLUSTER LAMP STATES (AIRBAG LAMP)";
                                                ccdvaluetoinsert = Util.ByteToHexString(ccdmessage, 1, 3);
                                                ccdunittoinsert = String.Empty;
                                            }
                                            break;
                                        case 0x42:
                                            if (ccdmessage.Length > 3)
                                            {
                                                ccddescriptiontoinsert = "THROTTLE POSITION SENSOR | CRUISE CONTROL";

                                                ImperialSlope = 0.65D;
                                                ccdvaluetoinsert = (ccdmessage[1] * ImperialSlope).ToString("0");
                                                ccdunittoinsert = "%";
                                            }
                                            break;
                                        case 0x44:
                                            if (ccdmessage.Length > 3)
                                            {
                                                ccddescriptiontoinsert = "FUEL USED";
                                                ccdvaluetoinsert = ((ccdmessage[1] << 8) | ccdmessage[2]).ToString("0");
                                                ccdunittoinsert = String.Empty;
                                            }
                                            break;
                                        case 0x50:
                                            if (ccdmessage.Length > 2)
                                            {
                                                ccddescriptiontoinsert = "AIRBAG LAMP STATE";
                                                if ((ccdmessage[1] & 0x01) == 0x01) ccdvaluetoinsert = "AIRBAG LAMP ON";
                                                else ccdvaluetoinsert = "AIRBAG LAMP OFF";
                                                ccdunittoinsert = String.Empty;
                                            }
                                            break;
                                        case 0x6D:
                                            if (ccdmessage.Length > 3)
                                            {
                                                ccddescriptiontoinsert = "VEHICLE IDENTIFICATION NUMBER (VIN)";
                                                if ((ccdmessage[1] > 0) && (ccdmessage[1] < 18))
                                                {
                                                    // Replace characters in the VIN string (one by one)
                                                    VIN = VIN.Remove(ccdmessage[1] - 1, 1).Insert(ccdmessage[1] - 1, ((char)(ccdmessage[2])).ToString());
                                                }
                                                ccdvaluetoinsert = VIN;
                                                ccdunittoinsert = String.Empty;
                                            }
                                            break;
                                        case 0x75:
                                            if (ccdmessage.Length > 3)
                                            {
                                                ccddescriptiontoinsert = "A/C HIGH SIDE PRESSURE";

                                                if (Units == 1) // Imperial
                                                {
                                                    ImperialSlope = 1.961D;
                                                    ccdvaluetoinsert = (ccdmessage[1] * ImperialSlope).ToString("0.0").Replace(",", ".");
                                                    ccdunittoinsert = "PSI";
                                                }
                                                if (Units == 0) // Metric
                                                {
                                                    MetricSlope = 1.961D * 6.894757D;
                                                    ccdvaluetoinsert = (ccdmessage[1] * MetricSlope).ToString("0.0").Replace(",", ".");
                                                    ccdunittoinsert = "KPA";
                                                }
                                            }
                                            break;
                                        case 0x7B:
                                            if (ccdmessage.Length > 2)
                                            {
                                                ccddescriptiontoinsert = "OUTSIDE AIR TEMPERATURE"; // Fahrenheit only by default

                                                if (Units == 1) // Imperial
                                                {
                                                    ImperialSlope = 1D;
                                                    ImperialOffset = -70.0D;
                                                    ccdvaluetoinsert = ((ccdmessage[1] * ImperialSlope) + ImperialOffset).ToString("0");
                                                    ccdunittoinsert = "°F";
                                                }
                                                if (Units == 0) // Metric (manual conversion)
                                                {
                                                    ImperialSlope = 1D;
                                                    ImperialOffset = -70.0D;
                                                    ccdvaluetoinsert = Math.Round((((ccdmessage[1] * ImperialSlope) + ImperialOffset) - 32) / 1.8D).ToString("0");
                                                    ccdunittoinsert = "°C";
                                                }
                                            }
                                            break;
                                        case 0x8C:
                                            if (ccdmessage.Length > 3)
                                            {
                                                ccddescriptiontoinsert = "ENGINE COOLANT TEMPERATURE | INTAKE AIR TEMPERATURE";

                                                if (Units == 1) // Imperial
                                                {
                                                    ImperialSlope = 1.8D;
                                                    ImperialOffset = -198.4D;
                                                    ccdvaluetoinsert = Math.Round((ccdmessage[1] * ImperialSlope) + ImperialOffset).ToString("0") + " | ";
                                                    ccdvaluetoinsert += Math.Round((ccdmessage[2] * ImperialSlope) + ImperialOffset).ToString("0");
                                                    ccdunittoinsert = "°F | °F";
                                                }
                                                if (Units == 0) // Metric
                                                {
                                                    ImperialSlope = 1.8D;
                                                    ImperialOffset = -198.4D;
                                                    MetricSlope = 0.555556D;
                                                    MetricOffset = -17.77778D;
                                                    ccdvaluetoinsert = Math.Round((((ccdmessage[1] * ImperialSlope) + ImperialOffset) * MetricSlope) + MetricOffset).ToString("0") + " | ";
                                                    ccdvaluetoinsert += Math.Round((((ccdmessage[2] * ImperialSlope) + ImperialOffset) * MetricSlope) + MetricOffset).ToString("0");
                                                    ccdunittoinsert = "°C | °C";
                                                }
                                            }
                                            break;
                                        case 0xA9:
                                            if (ccdmessage.Length > 2)
                                            {
                                                ccddescriptiontoinsert = "LAST ENGINE SHUTDOWN";
                                                ccdvaluetoinsert = ccdmessage[1].ToString("0");
                                                ccdunittoinsert = "MINUTE";
                                            }
                                            break;
                                        case 0xB1:
                                            if (ccdmessage.Length > 2)
                                            {
                                                List<string> warning_list = new List<string>();
                                                warning_list.Clear();

                                                if (ccdmessage[1] == 0)
                                                {
                                                    ccddescriptiontoinsert = "NO WARNING";
                                                    ccdvaluetoinsert = String.Empty;
                                                    ccdunittoinsert = String.Empty;
                                                }
                                                else
                                                {
                                                    if ((ccdmessage[1] & 0x01) == 1) warning_list.Add("KEY IN IGNITION");
                                                    if (((ccdmessage[1] & 0x02) >> 1) == 1) warning_list.Add("SEAT BELT");
                                                    if (((ccdmessage[1] & 0x04) >> 2) == 1) ccdvaluetoinsert = "EXTERIOR LAMP";
                                                    if (((ccdmessage[1] & 0x10) >> 4) == 1) warning_list.Add("OVERSPEED ");

                                                    ccddescriptiontoinsert = "WARNING: ";

                                                    foreach (string s in warning_list)
                                                    {
                                                        ccddescriptiontoinsert += s + " | ";
                                                    }

                                                    if (ccddescriptiontoinsert.Length > 9) ccddescriptiontoinsert = ccddescriptiontoinsert.Remove(ccddescriptiontoinsert.Length - 3); // remove last "|" character
                                                    ccdunittoinsert = String.Empty;
                                                }
                                            }
                                            break;
                                        case 0xB2:
                                            if (ccdmessage.Length > 5)
                                            {
                                                ccddescriptiontoinsert = "DRB REQUEST";
                                                ccdvaluetoinsert = String.Empty;
                                                ccdunittoinsert = String.Empty;
                                            }
                                            break;
                                        case 0xBE:
                                            if (ccdmessage.Length > 2)
                                            {
                                                ccddescriptiontoinsert = "IGNITION SWITCH POSITION";
                                                ccdvaluetoinsert = Util.ByteToHexString(ccdmessage, 1, 2);
                                                ccdunittoinsert = String.Empty;
                                            }
                                            break;
                                        case 0xC2:
                                            if (ccdmessage.Length > 2)
                                            {
                                                ccddescriptiontoinsert = "SKIM SECRET KEY";
                                                ccdvaluetoinsert = Util.ByteToHexString(ccdmessage, 1, ccdmessage.Length - 1);
                                                ccdunittoinsert = String.Empty;
                                            }
                                            break;
                                        case 0xCC:
                                            if (ccdmessage.Length > 3)
                                            {
                                                ccddescriptiontoinsert = "ACCUMULATED MILEAGE";
                                                ccdvaluetoinsert = Util.ByteToHexString(ccdmessage, 1, 3);
                                                ccdunittoinsert = String.Empty;
                                            }
                                            break;
                                        case 0xCE:
                                            if (ccdmessage.Length > 5)
                                            {
                                                ccddescriptiontoinsert = "VEHICLE DISTANCE / ODOMETER";

                                                if (Units == 1) // Imperial
                                                {
                                                    ImperialSlope = 0.000125D;
                                                    ccdvaluetoinsert = ((UInt32)(ccdmessage[1] << 24 | ccdmessage[2] << 16 | ccdmessage[3] << 8 | ccdmessage[4]) * ImperialSlope).ToString("0.000").Replace(",", ".");
                                                    ccdunittoinsert = "MILE";
                                                }
                                                if (Units == 0) // Metric
                                                {
                                                    MetricSlope = 0.000125D * 1.609334138D;
                                                    ccdvaluetoinsert = ((UInt32)(ccdmessage[1] << 24 | ccdmessage[2] << 16 | ccdmessage[3] << 8 | ccdmessage[4]) * MetricSlope).ToString("0.000").Replace(",", ".");
                                                    ccdunittoinsert = "KILOMETER";
                                                }
                                            }
                                            break;
                                        case 0xD4:
                                            if (ccdmessage.Length > 3)
                                            {
                                                ccddescriptiontoinsert = "BATTERY VOLTAGE | CALCULATED CHARGING VOLTAGE";
                                                ImperialSlope = 0.0592D;
                                                ccdvaluetoinsert = (ccdmessage[1] * ImperialSlope).ToString("0.0").Replace(",", ".") + " | " + (ccdmessage[2] * ImperialSlope).ToString("0.0").Replace(",", ".");
                                                ccdunittoinsert = "V | V";
                                            }
                                            break;
                                        case 0xDA:
                                            if (ccdmessage.Length > 2)
                                            {
                                                ccddescriptiontoinsert = "INSTRUMENT CLUSTER LAMP STATES";
                                                if ((ccdmessage[1] & 0x40) == 0x40) ccdvaluetoinsert = "MIL LAMP ON";
                                                else ccdvaluetoinsert = "MIL LAMP OFF";
                                                ccdunittoinsert = String.Empty;
                                            }
                                            break;
                                        case 0xE4:
                                            if (ccdmessage.Length > 3)
                                            {
                                                ccddescriptiontoinsert = "ENGINE SPEED | INTAKE MANIFOLD ABSOLUTE PRESSURE";

                                                if (Units == 1) // Imperial
                                                {
                                                    ImperialSlope = 32D; // RPM
                                                    ccdvaluetoinsert = (ccdmessage[1] * ImperialSlope).ToString("0") + " | ";

                                                    ImperialSlope = 0.059756D; // PSI
                                                    ccdvaluetoinsert += (ccdmessage[2] * ImperialSlope).ToString("0.0").Replace(",", ".");
                                                    ccdunittoinsert = "RPM | PSI";
                                                }
                                                if (Units == 0) // Metric
                                                {
                                                    ImperialSlope = 32D; // RPM
                                                    ccdvaluetoinsert = (ccdmessage[1] * ImperialSlope).ToString("0") + " | ";

                                                    ImperialSlope = 0.059756D; // KPA
                                                    MetricSlope = 6.894757D;
                                                    ccdvaluetoinsert += ((ccdmessage[2] * ImperialSlope) * MetricSlope).ToString("0.0").Replace(",", ".");
                                                    ccdunittoinsert = "RPM | KPA";
                                                }
                                            }
                                            break;
                                        case 0xEE:
                                            if (ccdmessage.Length > 4)
                                            {
                                                ccddescriptiontoinsert = "TRIP DISTANCE / TRIPMETER";

                                                if (Units == 1) // Imperial
                                                {
                                                    ImperialSlope = 0.016D;
                                                    ccdvaluetoinsert = ((UInt32)(ccdmessage[1] << 16 | ccdmessage[2] << 8 | ccdmessage[3]) * ImperialSlope).ToString("0.000").Replace(",", ".");
                                                    ccdunittoinsert = "MILE";
                                                }
                                                if (Units == 0) // Metric
                                                {
                                                    MetricSlope = 0.016D * 1.609334138D;
                                                    ccdvaluetoinsert = ((UInt32)(ccdmessage[1] << 16 | ccdmessage[2] << 8 | ccdmessage[3]) * MetricSlope).ToString("0.000").Replace(",", ".");
                                                    ccdunittoinsert = "KILOMETER";
                                                }
                                            }
                                            break;
                                        case 0xF1:
                                            if (ccdmessage.Length > 2)
                                            {
                                                List<string> warning_list = new List<string>();
                                                warning_list.Clear();

                                                if (ccdmessage[1] == 0)
                                                {
                                                    ccddescriptiontoinsert = "NO WARNING";
                                                    ccdvaluetoinsert = String.Empty;
                                                    ccdunittoinsert = String.Empty;
                                                }
                                                else
                                                {
                                                    if ((ccdmessage[1] & 0x01) == 1) warning_list.Add("LOW FUEL");
                                                    if (((ccdmessage[1] & 0x02) >> 1) == 1) warning_list.Add("LOW OIL");
                                                    if (((ccdmessage[1] & 0x04) >> 2) == 1) warning_list.Add("HI TEMP");
                                                    if (((ccdmessage[1] & 0x08) >> 3) == 1) ccdvaluetoinsert = "CRITICAL TEMP"; // display this in the value columns
                                                    if (((ccdmessage[1] & 0x10) >> 4) == 1) warning_list.Add("BRAKE PRESS");

                                                    ccddescriptiontoinsert = "WARNING: ";

                                                    foreach (string s in warning_list)
                                                    {
                                                        ccddescriptiontoinsert += s + " | ";
                                                    }

                                                    if (ccddescriptiontoinsert.Length > 9) ccddescriptiontoinsert = ccddescriptiontoinsert.Remove(ccddescriptiontoinsert.Length - 3); // remove last "|" character
                                                    ccdunittoinsert = String.Empty;
                                                }
                                            }
                                            break;
                                        case 0xF2:
                                            if (ccdmessage.Length > 5)
                                            {
                                                ccddescriptiontoinsert = "DRB RESPONSE";
                                                ccdvaluetoinsert = String.Empty;
                                                ccdunittoinsert = String.Empty;
                                            }
                                            break;
                                        case 0xFE:
                                            if (ccdmessage.Length > 2)
                                            {
                                                ccddescriptiontoinsert = "INTERIOR LAMP DIMMING";
                                                ccdvaluetoinsert = Util.ByteToHexString(ccdmessage, 1, 2);
                                                ccdunittoinsert = String.Empty;
                                            }
                                            break;
                                        case 0xFF:
                                            ccddescriptiontoinsert = "CCD-BUS WAKE UP";
                                            ccdvaluetoinsert = String.Empty;
                                            ccdunittoinsert = String.Empty;
                                            break;
                                    }

                                    // Insert texts into the line
                                    if (ccddescriptiontoinsert != String.Empty)
                                    {
                                        ccdlistitem.Remove(28, ccddescriptiontoinsert.Length);
                                        ccdlistitem.Insert(28, ccddescriptiontoinsert);
                                    }
                                    if (ccdvaluetoinsert != String.Empty)
                                    {
                                        ccdlistitem.Remove(82, ccdvaluetoinsert.Length);
                                        ccdlistitem.Insert(82, ccdvaluetoinsert);
                                    }
                                    if (ccdunittoinsert != String.Empty)
                                    {
                                        ccdlistitem.Remove(108, ccdunittoinsert.Length);
                                        ccdlistitem.Insert(108, ccdunittoinsert);
                                    }

                                    // Now decide where to put this message in the table itself
                                    if (!CCDBusIDList.Contains(IDByte)) // if this ID-byte is not on the list insert it into a new line
                                    {
                                        CCDBusIDByteNum++;

                                        // Handle B2 and F2 messages elsewhere
                                        if ((IDByte != 0xB2) && (IDByte != 0xF2)) // if it's not diagnostic request or response message
                                        {
                                            CCDBusIDList.Add(IDByte); // add ID-byte to the list
                                            if (IDSorting)
                                            {
                                                CCDBusIDList.Sort(); // sort ID-bytes by ascending order
                                            }
                                            location = CCDBusIDList.FindIndex(x => x == IDByte); // now see where this new ID-byte ends up after sorting

                                            if (CCDBusIDList.Count == 1)
                                            {
                                                DiagnosticsTable.RemoveAt(CCDBusListStart);
                                                DiagnosticsTable.Insert(CCDBusListStart, ccdlistitem.ToString());
                                            }
                                            else
                                            {
                                                DiagnosticsTable.Insert(CCDBusListStart + location, ccdlistitem.ToString());
                                                CCDBusListEnd++; // increment start/end row-numbers (new data causes the table to grow)
                                                CCDBusB2Start++;
                                                CCDBusF2Start++;
                                                SCIBusPCMHeaderStart++;
                                                SCIBusPCMListStart++;
                                                SCIBusPCMListEnd++;
                                                SCIBusTCMHeaderStart++;
                                                SCIBusTCMListStart++;
                                                SCIBusTCMListEnd++;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if ((IDByte != 0xB2) && (IDByte != 0xF2)) // if it's not diagnostic request or response message
                                        {
                                            location = CCDBusIDList.FindIndex(x => x == IDByte);
                                            DiagnosticsTable.RemoveAt(CCDBusListStart + location);
                                            DiagnosticsTable.Insert(CCDBusListStart + location, ccdlistitem.ToString());
                                        }
                                    }

                                    if (IDByte == 0xB2)
                                    {
                                        if (!CCDBusB2MsgPresent)
                                        {
                                            CCDBusB2MsgPresent = true;
                                        }
                                        else
                                        {
                                            CCDBusIDByteNum--; // workaround: B2 is not added to the ID-byte list so everytime it appears it increases this number
                                        }
                                        DiagnosticsTable.RemoveAt(CCDBusB2Start);
                                        DiagnosticsTable.Insert(CCDBusB2Start, ccdlistitem.ToString());

                                        if (IncludeTimestampInLogFilesToolStripMenuItem.Checked)
                                        {
                                            TimeSpan CCDElapsedTime = TimeSpan.FromMilliseconds(ccdtimestamp[0] << 24 | ccdtimestamp[1] << 16 | ccdtimestamp[2] << 8 | ccdtimestamp[3]);
                                            DateTime CCDTimestamp = DateTime.Today.Add(CCDElapsedTime);
                                            string CCDTimestampString = CCDTimestamp.ToString("HH:mm:ss.fff") + " ";
                                            File.AppendAllText(CCDB2F2LogFilename, CCDTimestampString); // no newline is appended!
                                        }
                                        File.AppendAllText(CCDB2F2LogFilename, Util.ByteToHexString(ccdmessage, 0, ccdmessage.Length) + Environment.NewLine); // save B2-messages separately
                                    }
                                    else if (IDByte == 0xF2)
                                    {
                                        if (!CCDBusF2MsgPresent)
                                        {
                                            CCDBusF2MsgPresent = true;
                                        }
                                        else
                                        {
                                            CCDBusIDByteNum--; // workaround: F2 is not added to the ID-byte list so everytime it appears it increases this number
                                        }
                                        DiagnosticsTable.RemoveAt(CCDBusF2Start);
                                        DiagnosticsTable.Insert(CCDBusF2Start, ccdlistitem.ToString());

                                        if (IncludeTimestampInLogFilesToolStripMenuItem.Checked)
                                        {
                                            TimeSpan CCDElapsedTime = TimeSpan.FromMilliseconds(ccdtimestamp[0] << 24 | ccdtimestamp[1] << 16 | ccdtimestamp[2] << 8 | ccdtimestamp[3]);
                                            DateTime CCDTimestamp = DateTime.Today.Add(CCDElapsedTime);
                                            string CCDTimestampString = CCDTimestamp.ToString("HH:mm:ss.fff") + " ";
                                            File.AppendAllText(CCDB2F2LogFilename, CCDTimestampString); // no newline is appended!
                                        }
                                        File.AppendAllText(CCDB2F2LogFilename, Util.ByteToHexString(ccdmessage, 0, ccdmessage.Length) + Environment.NewLine); // save F2-messages separately
                                    }
                                    CCDBusMsgRxCount++;

                                    CCDBusHeaderText = CCDBusStateLineEnabled;
                                    CCDBusHeaderText = CCDBusHeaderText.Replace("ID BYTES: ", "ID BYTES: " + CCDBusIDByteNum.ToString()).
                                                                        Replace("# OF MESSAGES: RX=", "# OF MESSAGES: RX=" + CCDBusMsgRxCount.ToString()).
                                                                        Replace(" TX=", " TX=" + CCDBusMsgTxCount.ToString());
                                    CCDBusHeaderText = Util.Truncate(CCDBusHeaderText, EmptyLine.Length); // Replacing strings causes the base string to grow so cut it back to stay in line
                                    DiagnosticsTable.RemoveAt(CCDBusHeaderStart);
                                    DiagnosticsTable.Insert(CCDBusHeaderStart, CCDBusHeaderText);

                                    UpdateDiagnosticsListBox();

                                    Util.UpdateTextBox(USBTextBox, "[RX->] CCD-bus message", Packet);

                                    if (IncludeTimestampInLogFilesToolStripMenuItem.Checked)
                                    {
                                        TimeSpan CCDElapsedTime = TimeSpan.FromMilliseconds(ccdtimestamp[0] << 24 | ccdtimestamp[1] << 16 | ccdtimestamp[2] << 8 | ccdtimestamp[3]);
                                        DateTime CCDTimestamp = DateTime.Today.Add(CCDElapsedTime);
                                        string CCDTimestampString = CCDTimestamp.ToString("HH:mm:ss.fff") + " ";
                                        File.AppendAllText(CCDLogFilename, CCDTimestampString); // no newline is appended!
                                    }
                                    File.AppendAllText(CCDLogFilename, Util.ByteToHexString(ccdmessage, 0, ccdmessage.Length) + Environment.NewLine);
                                    break;
                                case 0x02: // SCI-bus (PCM)
                                    SCIBusPCMEnabled = true;
                                    StringBuilder scipcmlistitem = new StringBuilder(EmptyLine); // start with a pre-defined empty line
                                    string scipcmmsgtoinsert = String.Empty;
                                    string scipcmdescriptiontoinsert = String.Empty;
                                    string scipcmvaluetoinsert = String.Empty;
                                    string scipcmunittoinsert = String.Empty;
                                    byte[] pcmtimestamp = new byte[4];
                                    byte[] pcmmessage = new byte[Payload.Length - 4];
                                    Array.Copy(Payload, 0, pcmtimestamp, 0, 4); // copy timestamp only
                                    Array.Copy(Payload, 4, pcmmessage, 0, Payload.Length - 4); // copy message only

                                    // In case of high speed mode the request and response bytes are sent in separate packets.
                                    // First packet is always the request bytes list, save them here and do nothing else.
                                    // Second packet contains the response bytes, mix it with the request bytes and update the table.
                                    if (SubDataCode == 0x01) // low speed bytes
                                    {
                                        if (pcmmessage.Length < 9) // max 8 byte fits the message column
                                        {
                                            scipcmmsgtoinsert = Util.ByteToHexString(pcmmessage, 0, pcmmessage.Length) + " ";
                                        }
                                        else // Trim message and insert two dots at the end indicating there's more to it
                                        {
                                            scipcmmsgtoinsert = Util.ByteToHexString(pcmmessage, 0, 7) + " .. ";
                                        }

                                        scipcmlistitem.Remove(2, scipcmmsgtoinsert.Length); // remove as much whitespaces as the length of the message
                                        scipcmlistitem.Insert(2, scipcmmsgtoinsert); // insert message where whitespaces were

                                        IDByte = pcmmessage[0]; // first byte of the message

                                        // Insert description in the line
                                        switch (IDByte)
                                        {
                                            case 0x10:
                                                scipcmdescriptiontoinsert = "ENGINE FAULT CODE LIST";

                                                if (pcmmessage.Length > 2) // minimum 3 bytes are required for a meaningful message
                                                {
                                                    byte dtcchecksum = 0;
                                                    for (int i = 0; i < pcmmessage.Length - 1; i++)
                                                    {
                                                        dtcchecksum += pcmmessage[i];
                                                    }

                                                    if (dtcchecksum == pcmmessage[pcmmessage.Length - 1]) // Checksum OK
                                                    {
                                                        if (((pcmmessage[0] == 0x10) && (pcmmessage[1] == 0xFE) && (pcmmessage[2] == 0x0E)) ||
                                                            ((pcmmessage[0] == 0x10) && (pcmmessage[1] == 0xFD) && (pcmmessage[2] == 0xFE) && (pcmmessage[3] == 0x0B)))
                                                        {
                                                            scipcmvaluetoinsert = "NO FAULT CODES";
                                                        }

                                                        else if (pcmmessage[pcmmessage.Length - 2] == 0xFE)
                                                        {
                                                            if ((pcmmessage.Length > 3) && (pcmmessage[pcmmessage.Length - 3] != 0xFD))
                                                            {
                                                                if (pcmmessage.Length < 12)
                                                                {
                                                                    scipcmvaluetoinsert = Util.ByteToHexString(pcmmessage, 1, pcmmessage.Length - 2) + " ";
                                                                }
                                                                else
                                                                {
                                                                    scipcmvaluetoinsert = Util.ByteToHexString(pcmmessage, 1, 8) + " .. ";
                                                                }
                                                            }
                                                            else if ((pcmmessage.Length > 4) && (pcmmessage[pcmmessage.Length - 3] == 0xFD))
                                                            {
                                                                if (pcmmessage.Length < 13)
                                                                {
                                                                    scipcmvaluetoinsert = Util.ByteToHexString(pcmmessage, 1, pcmmessage.Length - 3) + " ";
                                                                }
                                                                else
                                                                {
                                                                    scipcmvaluetoinsert = Util.ByteToHexString(pcmmessage, 1, 8) + " .. ";
                                                                }
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        scipcmvaluetoinsert = "CHECKSUM ERROR";
                                                    }
                                                }

                                                scipcmunittoinsert = String.Empty;
                                                break;
                                            case 0x11:
                                                scipcmdescriptiontoinsert = "ENGINE FAULT BIT LIST";
                                                scipcmvaluetoinsert = String.Empty;
                                                scipcmunittoinsert = String.Empty;
                                                break;
                                            case 0x12:
                                                scipcmdescriptiontoinsert = "SELECT HIGH-SPEED MODE";
                                                scipcmvaluetoinsert = String.Empty;
                                                scipcmunittoinsert = String.Empty;
                                                break;
                                            case 0x13:
                                                if (pcmmessage.Length > 2)
                                                {
                                                    switch (pcmmessage[2])
                                                    {
                                                        case 0x00:
                                                            scipcmdescriptiontoinsert = "ENGAGE ACTUATOR TEST";
                                                            scipcmvaluetoinsert = "STOPPED";
                                                            scipcmunittoinsert = String.Empty;
                                                            break;
                                                        case 0x07:
                                                            scipcmdescriptiontoinsert = "IAC STEPPER MOTOR TEST";
                                                            scipcmvaluetoinsert = "RUNNING";
                                                            scipcmunittoinsert = String.Empty;
                                                            break;
                                                        default:
                                                            scipcmdescriptiontoinsert = "ENGAGE ACTUATOR TEST";
                                                            scipcmvaluetoinsert = "RUNNING";
                                                            scipcmunittoinsert = String.Empty;
                                                            break;
                                                    }

                                                    scipcmunittoinsert = String.Empty;
                                                }
                                                else
                                                {
                                                    scipcmdescriptiontoinsert = "ENGAGE ACTUATOR TEST";
                                                    scipcmvaluetoinsert = String.Empty;
                                                    scipcmunittoinsert = String.Empty;
                                                }
                                                break;
                                            case 0x14:
                                                if (pcmmessage.Length > 1)
                                                {
                                                    switch (pcmmessage[1])
                                                    {
                                                        case 0x05: // coolant temperature
                                                            scipcmdescriptiontoinsert = "ENGINE COOLANT TEMPERATURE";
                                                            scipcmvaluetoinsert = String.Empty;
                                                            scipcmunittoinsert = String.Empty;
                                                            break;
                                                        default:
                                                            scipcmdescriptiontoinsert = "REQUEST DIAGNOSTIC DATA";
                                                            scipcmvaluetoinsert = String.Empty;
                                                            scipcmunittoinsert = String.Empty;
                                                            break;
                                                    }
                                                }
                                                else
                                                {
                                                    scipcmdescriptiontoinsert = "REQUEST DIAGNOSTIC DATA";
                                                    scipcmvaluetoinsert = String.Empty;
                                                    scipcmunittoinsert = String.Empty;
                                                }
                                                break;
                                            case 0x15:
                                                if (pcmmessage.Length > 3)
                                                {
                                                    scipcmdescriptiontoinsert = "REQUEST MEMORY DATA: ";
                                                    scipcmdescriptiontoinsert += Util.ByteToHexString(pcmmessage, 1, 3);
                                                    scipcmvaluetoinsert = Util.ByteToHexString(pcmmessage, 3, 4);
                                                    scipcmunittoinsert = String.Empty;
                                                }
                                                else
                                                {
                                                    scipcmdescriptiontoinsert = "REQUEST MEMORY DATA";
                                                    scipcmvaluetoinsert = "2 ADDR. BYTES REQUIRED";
                                                    scipcmunittoinsert = String.Empty;
                                                }
                                                break;
                                            case 0x16:
                                                scipcmdescriptiontoinsert = "REQUEST ECU ID";
                                                scipcmvaluetoinsert = String.Empty;
                                                scipcmunittoinsert = String.Empty;
                                                break;
                                            case 0x17:
                                                if (pcmmessage.Length > 1)
                                                {
                                                    switch (pcmmessage[1])
                                                    {
                                                        case 0xE0:
                                                        case 0xFF:
                                                            scipcmdescriptiontoinsert = "ERASE ENGINE FAULT CODES";
                                                            scipcmvaluetoinsert = "ERASED";
                                                            scipcmunittoinsert = String.Empty;
                                                            break;
                                                        default:
                                                            scipcmdescriptiontoinsert = "ERASE ENGINE FAULT CODES";
                                                            scipcmvaluetoinsert = "FAILED";
                                                            scipcmunittoinsert = String.Empty;
                                                            break;
                                                    }
                                                }
                                                else
                                                {
                                                    scipcmdescriptiontoinsert = "ERASE ENGINE FAULT CODES";
                                                    scipcmvaluetoinsert = String.Empty;
                                                    scipcmunittoinsert = String.Empty;
                                                }
                                                break;
                                            case 0x18:
                                                if (pcmmessage.Length > 1)
                                                {
                                                    scipcmdescriptiontoinsert = "CONTROL ASD RELAY";
                                                    scipcmvaluetoinsert = String.Empty;
                                                    scipcmunittoinsert = String.Empty;
                                                }
                                                else
                                                {
                                                    scipcmdescriptiontoinsert = "CONTROL ASD RELAY";
                                                    scipcmvaluetoinsert = String.Empty;
                                                    scipcmunittoinsert = String.Empty;
                                                }
                                                break;
                                            case 0x19:
                                                if (pcmmessage.Length > 1)
                                                {
                                                    scipcmdescriptiontoinsert = "SET MINIMUM IDLE SPEED";

                                                    scipcmvaluetoinsert = (pcmmessage[1] * 7.85D).ToString("0");
                                                    if (pcmmessage[1] < 0x42)
                                                    {
                                                        scipcmvaluetoinsert += " - TOO LOW!";
                                                    }

                                                    scipcmunittoinsert = "RPM";
                                                }
                                                else
                                                {
                                                    scipcmdescriptiontoinsert = "SET MINIMUM IDLE SPEED";
                                                    scipcmvaluetoinsert = "-";
                                                    scipcmunittoinsert = "RPM";
                                                }
                                                break;
                                            case 0x1A:
                                                if (pcmmessage.Length > 1)
                                                {
                                                    scipcmdescriptiontoinsert = "SWITCH TEST";
                                                    scipcmvaluetoinsert = String.Empty;
                                                    scipcmunittoinsert = String.Empty;
                                                }
                                                else
                                                {
                                                    scipcmdescriptiontoinsert = "SWITCH TEST";
                                                    scipcmvaluetoinsert = String.Empty;
                                                    scipcmunittoinsert = String.Empty;
                                                }
                                                break;
                                            case 0x1B:
                                                if (pcmmessage.Length > 1)
                                                {
                                                    switch (pcmmessage[1])
                                                    {
                                                        default:
                                                            scipcmdescriptiontoinsert = "INIT BYTE MODE DOWNLOAD";
                                                            scipcmvaluetoinsert = String.Empty;
                                                            scipcmunittoinsert = String.Empty;
                                                            break;
                                                    }
                                                }
                                                else
                                                {
                                                    scipcmdescriptiontoinsert = "INIT BYTE MODE DOWNLOAD";
                                                    scipcmvaluetoinsert = String.Empty;
                                                    scipcmunittoinsert = String.Empty;
                                                }
                                                break;
                                            case 0x1C:
                                                if (pcmmessage.Length > 3)
                                                {
                                                    switch (pcmmessage[1])
                                                    {
                                                        case 0x10:
                                                        case 0x11:
                                                            scipcmdescriptiontoinsert = "WRITE MEMORY: RESET EMR";
                                                            if ((pcmmessage[2] == 0x00) && (pcmmessage[3] == 0xFF))
                                                            {
                                                                scipcmvaluetoinsert = "OK";
                                                            }
                                                            else
                                                            {
                                                                scipcmvaluetoinsert = "ERROR";
                                                            }
                                                            scipcmunittoinsert = String.Empty;
                                                            break;
                                                        case 0x1A:
                                                            if (pcmmessage[2] == 0xFF)
                                                            {
                                                                scipcmdescriptiontoinsert = "WRITE MEMORY: ENABLE VARIABLE IDLE";

                                                                if (pcmmessage[3] == 0xFF)
                                                                {
                                                                    scipcmvaluetoinsert = "OK";
                                                                }
                                                                else
                                                                {
                                                                    scipcmvaluetoinsert = "ERROR";
                                                                }
                                                            }
                                                            else if (pcmmessage[2] == 0x00)
                                                            {
                                                                scipcmdescriptiontoinsert = "WRITE MEMORY: DISABLE VARIABLE IDLE";

                                                                if (pcmmessage[3] == 0xFF)
                                                                {
                                                                    scipcmvaluetoinsert = "OK";
                                                                }
                                                                else
                                                                {
                                                                    scipcmvaluetoinsert = "ERROR";
                                                                }
                                                            }
                                                            scipcmunittoinsert = String.Empty;
                                                            break;
                                                        default:
                                                            scipcmdescriptiontoinsert = "WRITE MEMORY - " + "ADDRESS: " + Util.ByteToHexString(pcmmessage, 1, 2) + " VALUE: " + Util.ByteToHexString(pcmmessage, 2, 3);
                                                            if (pcmmessage[3] == 0xFF)
                                                            {
                                                                scipcmvaluetoinsert = "OK";
                                                            }
                                                            else
                                                            {
                                                                scipcmvaluetoinsert = "ERROR";
                                                            }
                                                            scipcmunittoinsert = String.Empty;
                                                            break;
                                                    }
                                                }
                                                else
                                                {
                                                    scipcmdescriptiontoinsert = "WRITE MEMORY";
                                                    scipcmvaluetoinsert = String.Empty;
                                                    scipcmunittoinsert = String.Empty;
                                                }
                                                break;
                                            case 0x21:
                                                scipcmdescriptiontoinsert = "SET SYNC / TIMING / SPARK SCATTER";
                                                scipcmvaluetoinsert = String.Empty;
                                                scipcmunittoinsert = String.Empty;
                                                break;
                                            case 0x23:
                                                if (pcmmessage.Length > 2)
                                                {
                                                    switch (pcmmessage[1])
                                                    {
                                                        case 0x01:
                                                            scipcmdescriptiontoinsert = "ERASE ENGINE FAULT CODES";
                                                            if (pcmmessage[2] == 0xFF)
                                                            {
                                                                scipcmvaluetoinsert = "ERASED";
                                                            }
                                                            else
                                                            {
                                                                scipcmvaluetoinsert = "FAILED";
                                                            }
                                                            scipcmunittoinsert = String.Empty;
                                                            break;
                                                        case 0x02:
                                                            scipcmdescriptiontoinsert = "ADAPTIVE";
                                                            if (pcmmessage[2] != 0x00)
                                                            {
                                                                scipcmvaluetoinsert = "ON";
                                                            }
                                                            else
                                                            {
                                                                scipcmvaluetoinsert = "OFF";
                                                            }
                                                            scipcmunittoinsert = String.Empty;
                                                            break;
                                                        case 0x03:
                                                            scipcmdescriptiontoinsert = "IAC (IDLE AIR CONTROL) COUNTER";
                                                            if (pcmmessage[2] != 0x00)
                                                            {
                                                                scipcmvaluetoinsert = "ON";
                                                            }
                                                            else
                                                            {
                                                                scipcmvaluetoinsert = "OFF";
                                                            }
                                                            scipcmunittoinsert = String.Empty;
                                                            break;
                                                        case 0x04:
                                                            scipcmdescriptiontoinsert = "MINIMUM TPS (THROTTLE POSITION SENSOR)";
                                                            if (pcmmessage[2] != 0x00)
                                                            {
                                                                scipcmvaluetoinsert = "ON";
                                                            }
                                                            else
                                                            {
                                                                scipcmvaluetoinsert = "OFF";
                                                            }
                                                            scipcmunittoinsert = String.Empty;
                                                            break;
                                                        case 0x05:
                                                            scipcmdescriptiontoinsert = "FLEX FUEL PERCENT";
                                                            scipcmvaluetoinsert = pcmmessage[2].ToString("0");
                                                            scipcmunittoinsert = "%";
                                                            break;
                                                        case 0x06:
                                                            scipcmdescriptiontoinsert = "CAM/CRANK & SYNC";
                                                            if (pcmmessage[2] != 0x00)
                                                            {
                                                                scipcmvaluetoinsert = "ON (IN SYNC)";
                                                            }
                                                            else
                                                            {
                                                                scipcmvaluetoinsert = "OFF (NOT IN SYNC)";
                                                            }
                                                            scipcmunittoinsert = String.Empty;
                                                            break;
                                                        case 0x07:
                                                            scipcmdescriptiontoinsert = "FUEL SHUTOFF";
                                                            if (pcmmessage[2] != 0x00)
                                                            {
                                                                scipcmvaluetoinsert = "ON";
                                                            }
                                                            else
                                                            {
                                                                scipcmvaluetoinsert = "OFF";
                                                            }
                                                            scipcmunittoinsert = String.Empty;
                                                            break;
                                                        case 0x08:
                                                            scipcmdescriptiontoinsert = "RUNTIME AT STALL";
                                                            if (pcmmessage[2] != 0x00)
                                                            {
                                                                scipcmvaluetoinsert = "ON";
                                                            }
                                                            else
                                                            {
                                                                scipcmvaluetoinsert = "OFF";
                                                            }
                                                            scipcmunittoinsert = String.Empty;
                                                            break;
                                                        case 0x0B:
                                                            scipcmdescriptiontoinsert = "RESET CAM/CRANK";
                                                            switch (pcmmessage[2])
                                                            {
                                                                case 0x00:
                                                                    scipcmvaluetoinsert = "TURN OFF ENGINE";
                                                                    scipcmunittoinsert = "FAILED";
                                                                    break;
                                                                case 0x01:
                                                                    scipcmvaluetoinsert = "COMMAND OOR";
                                                                    scipcmunittoinsert = "FAILED";
                                                                    break;
                                                                case 0x02:
                                                                    scipcmvaluetoinsert = "DENIED WIP";
                                                                    scipcmunittoinsert = "FAILED";
                                                                    break;
                                                                case 0xF0:
                                                                    scipcmvaluetoinsert = "COMPLETED";
                                                                    scipcmunittoinsert = "OK";
                                                                    break;
                                                                default:
                                                                    scipcmvaluetoinsert = "UNRECOGNIZED";
                                                                    scipcmunittoinsert = "FAILED";
                                                                    break;
                                                            }
                                                            break;
                                                        case 0x12:
                                                            scipcmdescriptiontoinsert = "ADAPTIVE NUMERATOR";
                                                            switch (pcmmessage[2])
                                                            {
                                                                case 0x00:
                                                                    scipcmvaluetoinsert = "TURN OFF ENGINE";
                                                                    scipcmunittoinsert = "FAILED";
                                                                    break;
                                                                case 0x01:
                                                                    scipcmvaluetoinsert = "BAD TEST ID";
                                                                    scipcmunittoinsert = "FAILED";
                                                                    break;
                                                                case 0x02:
                                                                    scipcmvaluetoinsert = "MODULE BUSY";
                                                                    scipcmunittoinsert = "FAILED";
                                                                    break;
                                                                case 0x03:
                                                                    scipcmvaluetoinsert = "SECURITY STATUS";
                                                                    scipcmunittoinsert = "FAILED";
                                                                    break;
                                                                case 0xF0:
                                                                    scipcmvaluetoinsert = "EXECUTED";
                                                                    scipcmunittoinsert = "OK";
                                                                    break;
                                                                default:
                                                                    scipcmvaluetoinsert = "UNRECOGNIZED";
                                                                    scipcmunittoinsert = "FAILED";
                                                                    break;
                                                            }
                                                            break;
                                                        case 0x13:
                                                            scipcmdescriptiontoinsert = "RESET SKIM";
                                                            scipcmvaluetoinsert = pcmmessage[2].ToString("0");
                                                            scipcmunittoinsert = String.Empty;
                                                            break;
                                                        case 0x14:
                                                            scipcmdescriptiontoinsert = "RESET DUTY CYCLE MONITOR";
                                                            switch (pcmmessage[2])
                                                            {
                                                                case 0x00:
                                                                    scipcmvaluetoinsert = "TURN OFF ENGINE";
                                                                    scipcmunittoinsert = "FAILED";
                                                                    break;
                                                                case 0x01:
                                                                    scipcmvaluetoinsert = "BAD TEST ID";
                                                                    scipcmunittoinsert = "FAILED";
                                                                    break;
                                                                case 0x02:
                                                                    scipcmvaluetoinsert = "MODULE BUSY";
                                                                    scipcmunittoinsert = "FAILED";
                                                                    break;
                                                                case 0x03:
                                                                    scipcmvaluetoinsert = "SECURITY STATUS";
                                                                    scipcmunittoinsert = "FAILED";
                                                                    break;
                                                                case 0xF0:
                                                                    scipcmvaluetoinsert = "EXECUTED";
                                                                    scipcmunittoinsert = "OK";
                                                                    break;
                                                                default:
                                                                    scipcmvaluetoinsert = "UNRECOGNIZED";
                                                                    scipcmunittoinsert = "FAILED";
                                                                    break;
                                                            }
                                                            break;
                                                        case 0x20:
                                                            scipcmdescriptiontoinsert = "TPS ADAPTATION FOR ETC";
                                                            if (pcmmessage[2] != 0x00)
                                                            {
                                                                scipcmvaluetoinsert = "ON";
                                                            }
                                                            else
                                                            {
                                                                scipcmvaluetoinsert = "OFF";
                                                            }
                                                            scipcmunittoinsert = String.Empty;
                                                            break;
                                                        case 0x21:
                                                            scipcmdescriptiontoinsert = "MIN PEDAL VALUE";
                                                            if (pcmmessage[2] != 0x00)
                                                            {
                                                                scipcmvaluetoinsert = "ON";
                                                            }
                                                            else
                                                            {
                                                                scipcmvaluetoinsert = "OFF";
                                                            }
                                                            scipcmunittoinsert = String.Empty;
                                                            break;
                                                        case 0x22:
                                                            scipcmdescriptiontoinsert = "LEARNED KNOCK CORRECTION";
                                                            if (pcmmessage[2] != 0x00)
                                                            {
                                                                scipcmvaluetoinsert = "ON";
                                                            }
                                                            else
                                                            {
                                                                scipcmvaluetoinsert = "OFF";
                                                            }
                                                            scipcmunittoinsert = String.Empty;
                                                            break;
                                                        case 0x23:
                                                            scipcmdescriptiontoinsert = "LEARNED MISFIRE CORRECTION";
                                                            if (pcmmessage[2] != 0x00)
                                                            {
                                                                scipcmvaluetoinsert = "ON";
                                                            }
                                                            else
                                                            {
                                                                scipcmvaluetoinsert = "OFF";
                                                            }
                                                            scipcmunittoinsert = String.Empty;
                                                            break;
                                                        case 0x24:
                                                            scipcmdescriptiontoinsert = "IDLE ADAPTATION";
                                                            if (pcmmessage[2] != 0x00)
                                                            {
                                                                scipcmvaluetoinsert = "ON";
                                                            }
                                                            else
                                                            {
                                                                scipcmvaluetoinsert = "OFF";
                                                            }
                                                            scipcmunittoinsert = String.Empty;
                                                            break;
                                                        default:
                                                            scipcmdescriptiontoinsert = "ENGINE CONTROLLER REQUEST - " + "ADDRESS: " + Util.ByteToHexString(pcmmessage, 1, 2);
                                                            scipcmvaluetoinsert = Util.ByteToHexString(pcmmessage, 2, 3);
                                                            scipcmunittoinsert = String.Empty;
                                                            break;
                                                    }
                                                }
                                                else
                                                {
                                                    scipcmdescriptiontoinsert = "ENGINE CONTROLLER REQUESTS";
                                                    scipcmvaluetoinsert = String.Empty;
                                                    scipcmunittoinsert = String.Empty;
                                                }
                                                break;
                                            case 0x26: // Read flash memory
                                                if (pcmmessage.Length > 4)
                                                {
                                                    scipcmdescriptiontoinsert = "READ FLASH MEMORY: ";
                                                    scipcmdescriptiontoinsert += Util.ByteToHexString(pcmmessage, 1, 4);
                                                    scipcmvaluetoinsert = Util.ByteToHexString(pcmmessage, 4, 5);
                                                    scipcmunittoinsert = String.Empty;

                                                    if (RepeatIterate)
                                                    {
                                                        File.AppendAllText(PCMEPROMTextFilename, Util.ByteToHexString(pcmmessage, 0, pcmmessage.Length) + Environment.NewLine); // save message to text file

                                                        byte[] CommandOnly = pcmmessage.Take(4).ToArray();
                                                        if (CommandOnly.SequenceEqual(SCIBusPCMMessageToSendEnd))
                                                        {
                                                            RepeatIterate = false;
                                                            ConvertTextToBinaryDump(PCMEPROMTextFilename);
                                                        }

                                                        ProgressBar1.BeginInvoke((MethodInvoker)delegate
                                                        {
                                                        //if (ProgressBar1.Value < ProgressBar1.Maximum) ProgressBar1.Value += 1;
                                                        //else ProgressBar1.Value = 0;
                                                    });

                                                    }
                                                    else
                                                    {
                                                        ProgressBar1.Value = 0;
                                                        PercentageLabel.Text = "0.00%";
                                                        RemainingTimeLabel.Text = "Remaining time: 00:00:00";
                                                    }
                                                }
                                                else
                                                {
                                                    scipcmdescriptiontoinsert = "READ FLASH MEMORY";
                                                    scipcmvaluetoinsert = "3 ADDR. BYTES REQUIRED";
                                                    scipcmunittoinsert = String.Empty;
                                                }
                                                break;
                                            case 0x27: // Write flash memory
                                                if (pcmmessage.Length > 4)
                                                {
                                                    scipcmdescriptiontoinsert = "WRITE FLASH MEMORY: ";
                                                    scipcmdescriptiontoinsert += Util.ByteToHexString(pcmmessage, 1, 4);
                                                    scipcmvaluetoinsert = Util.ByteToHexString(pcmmessage, 4, 5);
                                                    scipcmunittoinsert = String.Empty;
                                                }
                                                else
                                                {
                                                    scipcmdescriptiontoinsert = "WRITE FLASH MEMORY";
                                                    scipcmvaluetoinsert = "3 ADDR. BYTES REQUIRED";
                                                    scipcmunittoinsert = String.Empty;
                                                }
                                                break;
                                            case 0x28: // Read EEPROM
                                                if (pcmmessage.Length > 3)
                                                {
                                                    scipcmdescriptiontoinsert = "READ EEPROM: ";
                                                    scipcmdescriptiontoinsert += Util.ByteToHexString(pcmmessage, 1, 3);
                                                    scipcmvaluetoinsert = Util.ByteToHexString(pcmmessage, 3, 4);
                                                    scipcmunittoinsert = String.Empty;

                                                    if (RepeatIterate)
                                                    {
                                                        File.AppendAllText(PCMEEPROMTextFilename, Util.ByteToHexString(pcmmessage, 0, pcmmessage.Length) + Environment.NewLine); // save message to text file

                                                        byte[] CommandOnly = pcmmessage.Take(3).ToArray();
                                                        if (CommandOnly.SequenceEqual(SCIBusPCMMessageToSendEnd))
                                                        {
                                                            RepeatIterate = false;
                                                            ConvertTextToBinaryDump(PCMEEPROMTextFilename);
                                                        }

                                                        ProgressBar1.BeginInvoke((MethodInvoker)delegate
                                                        {
                                                        //if (ProgressBar1.Value < ProgressBar1.Maximum) ProgressBar1.Value += 1;
                                                        //else ProgressBar1.Value = 0;
                                                    });

                                                    }
                                                    else
                                                    {
                                                        ProgressBar1.Value = 0;
                                                        PercentageLabel.Text = "0.00%";
                                                        RemainingTimeLabel.Text = "Remaining time: 00:00:00";
                                                    }
                                                }
                                                else
                                                {
                                                    scipcmdescriptiontoinsert = "READ EEPROM";
                                                    scipcmvaluetoinsert = "2 ADDR. BYTES REQUIRED";
                                                    scipcmunittoinsert = String.Empty;
                                                }
                                                break;
                                            case 0x2A:
                                                if (pcmmessage.Length > 2)
                                                {
                                                    switch (pcmmessage[1])
                                                    {
                                                        //case 0x00:
                                                        //    if (pcmmessage[2] == 0x00)
                                                        //    {
                                                        //        scipcmdescriptiontoinsert = "SUSPEND REQUEST";
                                                        //        scipcmvaluetoinsert = "OK";
                                                        //        scipcmunittoinsert = String.Empty;
                                                        //    }
                                                        //    else
                                                        //    {
                                                        //        scipcmdescriptiontoinsert = "SUSPEND REQUEST";
                                                        //        scipcmvaluetoinsert = "FAILED";
                                                        //        scipcmunittoinsert = String.Empty;
                                                        //    }
                                                        //    break;
                                                        case 0x01:
                                                            scipcmdescriptiontoinsert = "PCM PART NUMBER 1&2";
                                                            scipcmvaluetoinsert = Util.ByteToHexString(pcmmessage, 2, 3);
                                                            scipcmunittoinsert = String.Empty;
                                                            break;
                                                        case 0x02:
                                                            scipcmdescriptiontoinsert = "PCM PART NUMBER 3&4";
                                                            scipcmvaluetoinsert = Util.ByteToHexString(pcmmessage, 2, 3);
                                                            scipcmunittoinsert = String.Empty;
                                                            break;
                                                        case 0x03:
                                                            scipcmdescriptiontoinsert = "PCM PART NUMBER 5&6";
                                                            scipcmvaluetoinsert = Util.ByteToHexString(pcmmessage, 2, 3);
                                                            scipcmunittoinsert = String.Empty;
                                                            break;
                                                        case 0x04:
                                                            scipcmdescriptiontoinsert = "PCM PART NUMBER 7&8";
                                                            scipcmvaluetoinsert = Util.ByteToHexString(pcmmessage, 2, 3);
                                                            scipcmunittoinsert = String.Empty;
                                                            break;
                                                        case 0x0A:
                                                            scipcmdescriptiontoinsert = "FUEL TYPE";
                                                            switch (pcmmessage[2])
                                                            {
                                                                case 0x01:
                                                                    scipcmvaluetoinsert = "UNLEADED GAS";
                                                                    break;
                                                                case 0x02:
                                                                    scipcmvaluetoinsert = "DIESEL";
                                                                    break;
                                                                case 0x03:
                                                                    scipcmvaluetoinsert = "PROPANE";
                                                                    break;
                                                                case 0x04:
                                                                    scipcmvaluetoinsert = "METHANOL";
                                                                    break;
                                                                case 0x05:
                                                                    scipcmvaluetoinsert = "LEADED GAS";
                                                                    break;
                                                                case 0x06:
                                                                    scipcmvaluetoinsert = "FLEX";
                                                                    break;
                                                                case 0x07:
                                                                    scipcmvaluetoinsert = "CNG";
                                                                    break;
                                                                case 0x08:
                                                                    scipcmvaluetoinsert = "ELECTRIC";
                                                                    break;
                                                                default:
                                                                    scipcmvaluetoinsert = "N/A";
                                                                    break;
                                                            }
                                                            scipcmunittoinsert = String.Empty;
                                                            break;
                                                        case 0x0B:
                                                            scipcmdescriptiontoinsert = "MODEL YEAR";
                                                            switch (pcmmessage[2])
                                                            {
                                                                case 0x01:
                                                                    scipcmvaluetoinsert = "1991";
                                                                    break;
                                                                case 0x02:
                                                                    scipcmvaluetoinsert = "1992";
                                                                    break;
                                                                case 0x03:
                                                                    scipcmvaluetoinsert = "1993";
                                                                    break;
                                                                case 0x04:
                                                                    scipcmvaluetoinsert = "1994";
                                                                    break;
                                                                case 0x05:
                                                                    scipcmvaluetoinsert = "1995";
                                                                    break;
                                                                case 0x06:
                                                                    scipcmvaluetoinsert = "1996";
                                                                    break;
                                                                case 0x07:
                                                                    scipcmvaluetoinsert = "1997";
                                                                    break;
                                                                case 0x08:
                                                                    scipcmvaluetoinsert = "1998";
                                                                    break;
                                                                case 0x09:
                                                                    scipcmvaluetoinsert = "1999";
                                                                    break;
                                                                case 0x0A:
                                                                    scipcmvaluetoinsert = "2000";
                                                                    break;
                                                                case 0x0B:
                                                                    scipcmvaluetoinsert = "2001";
                                                                    break;
                                                                case 0x0C:
                                                                    scipcmvaluetoinsert = "2002";
                                                                    break;
                                                                case 0x0D:
                                                                    scipcmvaluetoinsert = "2003";
                                                                    break;
                                                                case 0x0E:
                                                                    scipcmvaluetoinsert = "2004";
                                                                    break;
                                                                case 0x0F:
                                                                    scipcmvaluetoinsert = "2005";
                                                                    break;
                                                                default:
                                                                    scipcmvaluetoinsert = "N/A";
                                                                    break;
                                                            }
                                                            scipcmunittoinsert = String.Empty;
                                                            break;
                                                        case 0x0C:
                                                            scipcmdescriptiontoinsert = "ENGINE DISPLACEMENT/CONFIGURATION/ORIENTATION";
                                                            switch (pcmmessage[2])
                                                            {
                                                                case 0x01:
                                                                    scipcmvaluetoinsert = "2.2 LITER";
                                                                    scipcmunittoinsert = "I4 E-W";
                                                                    break;
                                                                case 0x02:
                                                                    scipcmvaluetoinsert = "2.5 LITER";
                                                                    scipcmunittoinsert = "I4 E-W";
                                                                    break;
                                                                case 0x03:
                                                                    scipcmvaluetoinsert = "3.0 LITER";
                                                                    scipcmunittoinsert = "V6 E-W";
                                                                    break;
                                                                case 0x04:
                                                                    scipcmvaluetoinsert = "3.3 LITER";
                                                                    scipcmunittoinsert = "V6 E-W";
                                                                    break;
                                                                case 0x05:
                                                                    scipcmvaluetoinsert = "3.9 LITER";
                                                                    scipcmunittoinsert = "V6 N-S";
                                                                    break;
                                                                case 0x06:
                                                                    scipcmvaluetoinsert = "5.2 LITER";
                                                                    scipcmunittoinsert = "V8 N-S";
                                                                    break;
                                                                case 0x07:
                                                                    scipcmvaluetoinsert = "5.9 LITER";
                                                                    scipcmunittoinsert = "V8 N-S";
                                                                    break;
                                                                case 0x08:
                                                                    scipcmvaluetoinsert = "3.8 LITER";
                                                                    scipcmunittoinsert = "V6 E-W";
                                                                    break;
                                                                case 0x09:
                                                                    scipcmvaluetoinsert = "4.0 LITER";
                                                                    scipcmunittoinsert = "I6 N-S";
                                                                    break;
                                                                case 0x0A:
                                                                    scipcmvaluetoinsert = "2.0 LITER";
                                                                    scipcmunittoinsert = "I4 E-W SOHC";
                                                                    break;
                                                                case 0x0B:
                                                                    scipcmvaluetoinsert = "3.5 LITER";
                                                                    scipcmunittoinsert = "V6 N-S";
                                                                    break;
                                                                case 0x0C:
                                                                    scipcmvaluetoinsert = "8.0 LITER";
                                                                    scipcmunittoinsert = "V10 N-S";
                                                                    break;
                                                                case 0x0D:
                                                                    scipcmvaluetoinsert = "2.4 LITER";
                                                                    scipcmunittoinsert = "I4 E-W";
                                                                    break;
                                                                case 0x0E:
                                                                    scipcmvaluetoinsert = "2.5 LITER";
                                                                    scipcmunittoinsert = "I4 N-S";
                                                                    break;
                                                                case 0x0F:
                                                                    scipcmvaluetoinsert = "2.5 LITER";
                                                                    scipcmunittoinsert = "V6 N-S";
                                                                    break;
                                                                case 0x10:
                                                                    scipcmvaluetoinsert = "2.0 LITER";
                                                                    scipcmunittoinsert = "I4 E-W DOHC";
                                                                    break;
                                                                case 0x11:
                                                                    scipcmvaluetoinsert = "2.5 LITER";
                                                                    scipcmunittoinsert = "V6 E-W";
                                                                    break;
                                                                case 0x12:
                                                                    scipcmvaluetoinsert = "5.9 LITER";
                                                                    scipcmunittoinsert = "I6 N-S";
                                                                    break;
                                                                case 0x13:
                                                                    scipcmvaluetoinsert = "3.3 LITER";
                                                                    scipcmunittoinsert = "V6 N-S";
                                                                    break;
                                                                case 0x14:
                                                                    scipcmvaluetoinsert = "2.7 LITER";
                                                                    scipcmunittoinsert = "V6 N-S";
                                                                    break;
                                                                case 0x15:
                                                                    scipcmvaluetoinsert = "3.2 LITER";
                                                                    scipcmunittoinsert = "V6 N-S";
                                                                    break;
                                                                case 0x16:
                                                                    scipcmvaluetoinsert = "1.8 LITER";
                                                                    scipcmunittoinsert = "I4 E-W";
                                                                    break;
                                                                case 0x17:
                                                                    scipcmvaluetoinsert = "3.7 LITER";
                                                                    scipcmunittoinsert = "V6 N-S";
                                                                    break;
                                                                case 0x18:
                                                                    scipcmvaluetoinsert = "4.7 LITER";
                                                                    scipcmunittoinsert = "V8 N-S";
                                                                    break;
                                                                case 0x19:
                                                                    scipcmvaluetoinsert = "1.9 LITER";
                                                                    scipcmunittoinsert = "I4 E-W";
                                                                    break;
                                                                case 0x1A:
                                                                    scipcmvaluetoinsert = "3.1 LITER";
                                                                    scipcmunittoinsert = "I5 N-S";
                                                                    break;
                                                                case 0x1B:
                                                                    scipcmvaluetoinsert = "1.6 LITER";
                                                                    scipcmunittoinsert = "I4 E-W";
                                                                    break;
                                                                case 0x1C:
                                                                    scipcmvaluetoinsert = "2.7 LITER";
                                                                    scipcmunittoinsert = "V6 E-W";
                                                                    break;
                                                                default:
                                                                    scipcmvaluetoinsert = "N/A";
                                                                    scipcmunittoinsert = String.Empty;
                                                                    break;
                                                            }
                                                            break;
                                                        default:
                                                            if (pcmmessage[1] != 0x00)
                                                            {
                                                                scipcmdescriptiontoinsert = "ENGINE CONTROLLER REQUEST - " + "ADDRESS: " + Util.ByteToHexString(pcmmessage, 1, 2);
                                                                scipcmvaluetoinsert = Util.ByteToHexString(pcmmessage, 2, 3);
                                                                scipcmunittoinsert = String.Empty;
                                                            }
                                                            else
                                                            {
                                                                scipcmdescriptiontoinsert = "ENGINE CONTROLLER REQUESTS";
                                                                scipcmvaluetoinsert = "STOPPED";
                                                                scipcmunittoinsert = String.Empty;
                                                            }
                                                            break;
                                                    }
                                                }
                                                else
                                                {
                                                    if (pcmmessage.Length > 1)
                                                    {
                                                        if (pcmmessage[1] == 0x00)
                                                        {
                                                            scipcmdescriptiontoinsert = "ENGINE CONTROLLER REQUESTS";
                                                            scipcmvaluetoinsert = "STOPPED";
                                                            scipcmunittoinsert = String.Empty;
                                                        }
                                                        else
                                                        {
                                                            scipcmdescriptiontoinsert = "ENGINE CONTROLLER REQUESTS";
                                                            scipcmvaluetoinsert = "STOPPED";
                                                            scipcmunittoinsert = String.Empty;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        scipcmdescriptiontoinsert = "ENGINE CONTROLLER REQUESTS";
                                                        scipcmvaluetoinsert = String.Empty;
                                                        scipcmunittoinsert = String.Empty;
                                                    }
                                                }
                                                break;
                                            case 0x2B:
                                            case 0x35:
                                                if (pcmmessage.Length > 3)
                                                {
                                                    scipcmdescriptiontoinsert = "GET SECURITY SEED";
                                                    scipcmvaluetoinsert = Util.ByteToHexString(pcmmessage, 1, 4);
                                                    scipcmunittoinsert = String.Empty;
                                                }
                                                else
                                                {
                                                    scipcmdescriptiontoinsert = "GET SECURITY SEED";
                                                    scipcmvaluetoinsert = String.Empty;
                                                    scipcmunittoinsert = String.Empty;
                                                }
                                                break;
                                            case 0x2C:
                                                if (pcmmessage.Length > 4)
                                                {
                                                    scipcmdescriptiontoinsert = "SEND SECURITY SEED";
                                                    scipcmvaluetoinsert = Util.ByteToHexString(pcmmessage, 1, 4);
                                                    if (pcmmessage[4] != 0x00)
                                                    {
                                                        scipcmunittoinsert = "SUCCEEDED";
                                                    }
                                                    else
                                                    {
                                                        scipcmunittoinsert = "FAILED";
                                                    }
                                                }
                                                else
                                                {
                                                    scipcmdescriptiontoinsert = "SEND SECURITY SEED";
                                                    scipcmvaluetoinsert = String.Empty;
                                                    scipcmunittoinsert = String.Empty;
                                                }
                                                break;
                                            case 0x2D:
                                                if (pcmmessage.Length > 3)
                                                {
                                                    switch (pcmmessage[2])
                                                    {
                                                        default:
                                                            scipcmdescriptiontoinsert = "MIN/MAX ENGINE PARAMETER VALUE: ";
                                                            scipcmdescriptiontoinsert += Util.ByteToHexString(pcmmessage, 1, 3);
                                                            scipcmvaluetoinsert = Util.ByteToHexString(pcmmessage, 3, 4);
                                                            scipcmunittoinsert = String.Empty;
                                                            break;
                                                    }
                                                }
                                                else
                                                {
                                                    scipcmdescriptiontoinsert = "MIN/MAX ENGINE PARAMETER VALUES";
                                                    scipcmvaluetoinsert = String.Empty;
                                                    scipcmunittoinsert = String.Empty;
                                                }
                                                break;
                                            case 0x2E:
                                                if (pcmmessage.Length > 2)
                                                {
                                                    switch (pcmmessage[1])
                                                    {
                                                        default:
                                                            scipcmdescriptiontoinsert = "ENGINE ONE-TRIP FAULT CODES";
                                                            scipcmvaluetoinsert = "N/A";
                                                            scipcmunittoinsert = String.Empty;
                                                            break;
                                                    }
                                                }
                                                else
                                                {
                                                    scipcmdescriptiontoinsert = "ENGINE ONE-TRIP FAULT CODES";
                                                    scipcmvaluetoinsert = String.Empty;
                                                    scipcmunittoinsert = String.Empty;
                                                }
                                                break;
                                            case 0x32:
                                                if (pcmmessage.Length > 3)
                                                {
                                                    scipcmdescriptiontoinsert = "ENGINE FAULT CODES";
                                                    scipcmvaluetoinsert = String.Empty;
                                                    scipcmunittoinsert = String.Empty;

                                                    //switch (pcmmessage[1])
                                                    //{
                                                    //    default:
                                                    //        scipcmdescriptiontoinsert = "ENGINE ONE-TRIP FAULT CODES";
                                                    //        scipcmvaluetoinsert = "N/A";
                                                    //        scipcmunittoinsert = String.Empty;
                                                    //        break;
                                                    //}
                                                }
                                                else
                                                {
                                                    scipcmdescriptiontoinsert = "ENGINE FAULT CODES";
                                                    scipcmvaluetoinsert = String.Empty;
                                                    scipcmunittoinsert = String.Empty;
                                                }
                                                break;
                                            case 0x33:
                                                if (pcmmessage.Length > 2)
                                                {
                                                    switch (pcmmessage[1])
                                                    {
                                                        default:
                                                            scipcmdescriptiontoinsert = "ENGINE ONE-TRIP FAULT CODES";
                                                            scipcmvaluetoinsert = "N/A";
                                                            scipcmunittoinsert = String.Empty;
                                                            break;
                                                    }
                                                }
                                                else
                                                {
                                                    scipcmdescriptiontoinsert = "ENGINE ONE-TRIP FAULT CODES";
                                                    scipcmvaluetoinsert = String.Empty;
                                                    scipcmunittoinsert = String.Empty;
                                                }
                                                break;
                                            case 0xFE:
                                                scipcmdescriptiontoinsert = "ALREADY IN LOW-SPEED MODE";
                                                scipcmunittoinsert = String.Empty;
                                                break;
                                        }

                                        if (scipcmdescriptiontoinsert != String.Empty)
                                        {
                                            scipcmlistitem.Remove(28, scipcmdescriptiontoinsert.Length);
                                            scipcmlistitem.Insert(28, scipcmdescriptiontoinsert);
                                        }
                                        if (scipcmvaluetoinsert != String.Empty)
                                        {
                                            scipcmlistitem.Remove(82, scipcmvaluetoinsert.Length);
                                            scipcmlistitem.Insert(82, scipcmvaluetoinsert);
                                        }
                                        if (scipcmunittoinsert != String.Empty)
                                        {
                                            scipcmlistitem.Remove(108, scipcmunittoinsert.Length);
                                            scipcmlistitem.Insert(108, scipcmunittoinsert);
                                        }

                                        // Now decide where to put this message in reality
                                        if (!SCIBusPCMIDList.Contains(IDByte)) // if this ID-byte is not on the list
                                        {
                                            SCIBusPCMIDByteNum++;
                                            SCIBusPCMIDList.Add(IDByte); // add ID-byte to the list
                                            if (IDSorting)
                                            {
                                                SCIBusPCMIDList.Sort(); // sort ID-bytes by ascending order
                                            }
                                            location = SCIBusPCMIDList.FindIndex(x => x == IDByte); // now see where this new ID-byte ends up after sorting
                                            if (SCIBusPCMIDList.Count == 1)
                                            {
                                                DiagnosticsTable.RemoveAt(SCIBusPCMListStart);
                                                DiagnosticsTable.Insert(SCIBusPCMListStart, scipcmlistitem.ToString());
                                            }
                                            else
                                            {
                                                DiagnosticsTable.Insert(SCIBusPCMListStart + location, scipcmlistitem.ToString());
                                                SCIBusPCMListEnd++; // increment start/end row-numbers (new data causes the table to grow)
                                                SCIBusTCMHeaderStart++;
                                                SCIBusTCMListStart++;
                                                SCIBusTCMListEnd++;
                                            }
                                        }
                                        else // if this ID-byte is already displayed
                                        {
                                            location = SCIBusPCMIDList.FindIndex(x => x == IDByte);
                                            DiagnosticsTable.RemoveAt(SCIBusPCMListStart + location);
                                            DiagnosticsTable.Insert(SCIBusPCMListStart + location, scipcmlistitem.ToString());
                                        }
                                        SCIBusPCMMsgRxCount++;

                                        if (IncludeTimestampInLogFilesToolStripMenuItem.Checked)
                                        {
                                            TimeSpan SCIBusPCMElapsedTime = TimeSpan.FromMilliseconds(pcmtimestamp[0] << 24 | pcmtimestamp[1] << 16 | pcmtimestamp[2] << 8 | pcmtimestamp[3]);
                                            DateTime SCIBusPCMTimestamp = DateTime.Today.Add(SCIBusPCMElapsedTime);
                                            string SCIBusPCMTimestampString = SCIBusPCMTimestamp.ToString("HH:mm:ss.fff") + " ";
                                            File.AppendAllText(PCMLogFilename, SCIBusPCMTimestampString); // no newline is appended!
                                        }
                                        File.AppendAllText(PCMLogFilename, Util.ByteToHexString(pcmmessage, 0, pcmmessage.Length) + Environment.NewLine); // save message to text file
                                    }
                                    if (SubDataCode == 0x02) // high speed request bytes, just save them
                                    {
                                        SCIBusPCMReqMsgList.Clear(); // first clear previous bytes
                                        for (int i = 0; i < pcmmessage.Length; i++)
                                        {
                                            SCIBusPCMReqMsgList.Add(pcmmessage[i]);
                                        }
                                    }
                                    else if (SubDataCode == 0x03) // high speed response bytes, mix them with saved request bytes
                                    {
                                        byte[] requestbytes = SCIBusPCMReqMsgList.ToArray();
                                        List<byte> temp = new List<byte>(512);
                                        byte[] pcmmessagemix;
                                        temp.Add(pcmmessage[0]); // first byte is always the memory area byte
                                        for (int i = 1; i < pcmmessage.Length; i++)
                                        {
                                            temp.Add(requestbytes[i]); // Add request byte first
                                            temp.Add(pcmmessage[i]); // Add response byte next
                                        }

                                        pcmmessagemix = temp.ToArray();

                                        if (pcmmessagemix.Length > 1)
                                        {
                                            SecondaryIDByte = pcmmessagemix[1]; // Secondary ID byte applies to high speed mode only, it's actually the first RAM address that appears in the message
                                        }

                                        if (pcmmessagemix.Length < 9) // max 8 byte fits the message column
                                        {
                                            scipcmmsgtoinsert = Util.ByteToHexString(pcmmessagemix, 0, pcmmessagemix.Length) + " ";
                                        }
                                        else // Trim message and insert two dots at the end indicating there's more to it
                                        {
                                            scipcmmsgtoinsert = Util.ByteToHexString(pcmmessagemix, 0, 7) + " .. ";
                                        }

                                        scipcmlistitem.Remove(2, scipcmmsgtoinsert.Length); // remove as much whitespaces as the length of the message
                                        scipcmlistitem.Insert(2, scipcmmsgtoinsert); // insert message where whitespaces were

                                        // Insert description in the line
                                        switch (IDByte) // RAM table select byte
                                        {
                                            case 0xF4:
                                                switch (SecondaryIDByte) // the second byte acts now as a secondary ID-byte (RAM address)
                                                {
                                                    case 0x0A:
                                                        scipcmdescriptiontoinsert = "ENGINE SPEED";

                                                        if ((pcmmessagemix.Length > 4) && (pcmmessagemix[3] == 0x0B)) // Engine speed
                                                        {
                                                            scipcmvaluetoinsert = (((pcmmessagemix[2] << 8) | pcmmessagemix[4]) * 0.125).ToString("0.000").Replace(",", ".");
                                                            scipcmunittoinsert = "RPM";
                                                        }
                                                        else
                                                        {
                                                            scipcmvaluetoinsert = "ERROR";
                                                            scipcmunittoinsert = String.Empty;
                                                        }
                                                        break;
                                                    case 0x0C:
                                                        scipcmdescriptiontoinsert = "VEHICLE SPEED";

                                                        if ((pcmmessagemix.Length > 4) && (pcmmessagemix[3] == 0x0D)) // Vehicle speed
                                                        {
                                                            if (Units == 1) // Imperial
                                                            {
                                                                scipcmvaluetoinsert = (((pcmmessagemix[2] << 8) | pcmmessagemix[4]) * 0.0156).ToString("0.000").Replace(",", ".");
                                                                scipcmunittoinsert = "MPH";
                                                            }
                                                            if (Units == 0) // Metric
                                                            {
                                                                scipcmvaluetoinsert = (((pcmmessagemix[2] << 8) | pcmmessagemix[4]) * 0.0156 * 1.609344).ToString("0.000").Replace(",", ".");
                                                                scipcmunittoinsert = "KM/H";
                                                            }
                                                        }
                                                        else
                                                        {
                                                            scipcmvaluetoinsert = "ERROR";
                                                            scipcmunittoinsert = String.Empty;
                                                        }
                                                        break;
                                                }
                                                break;
                                            case 0xFE: // Fault code list
                                                scipcmdescriptiontoinsert = "SELECT LOW-SPEED MODE";
                                                scipcmvaluetoinsert = String.Empty;
                                                scipcmunittoinsert = String.Empty;
                                                break;
                                        }
                                        if (scipcmdescriptiontoinsert != String.Empty)
                                        {
                                            scipcmlistitem.Remove(28, scipcmdescriptiontoinsert.Length);
                                            scipcmlistitem.Insert(28, scipcmdescriptiontoinsert);
                                        }
                                        if (scipcmvaluetoinsert != String.Empty)
                                        {
                                            scipcmlistitem.Remove(82, scipcmvaluetoinsert.Length);
                                            scipcmlistitem.Insert(82, scipcmvaluetoinsert);
                                        }
                                        if (scipcmunittoinsert != String.Empty)
                                        {
                                            scipcmlistitem.Remove(108, scipcmunittoinsert.Length);
                                            scipcmlistitem.Insert(108, scipcmunittoinsert);
                                        }

                                        // Now decide where to put this message in reality
                                        if (!SCIBusPCMIDList.Contains(IDByte)) // if this ID-byte is not on the list
                                        {
                                            SCIBusPCMIDByteNum++;
                                            SCIBusPCMIDList.Add(IDByte); // add ID-byte to the list
                                            if (IDSorting)
                                            {
                                                SCIBusPCMIDList.Sort(); // sort ID-bytes by ascending order
                                            }

                                            location = SCIBusPCMIDList.FindIndex(x => x == IDByte); // now see where this new ID-byte ends up after sorting

                                            if (SCIBusPCMIDList.Count == 1)
                                            {
                                                DiagnosticsTable.RemoveAt(SCIBusPCMListStart);
                                                DiagnosticsTable.Insert(SCIBusPCMListStart, scipcmlistitem.ToString());
                                            }
                                            else
                                            {
                                                DiagnosticsTable.Insert(SCIBusPCMListStart + location, scipcmlistitem.ToString());
                                                SCIBusPCMListEnd++; // increment start/end row-numbers (new data causes the table to grow)
                                                SCIBusTCMHeaderStart++;
                                                SCIBusTCMListStart++;
                                                SCIBusTCMListEnd++;
                                            }
                                        }
                                        else // if this ID-byte is already displayed
                                        {
                                            location = SCIBusPCMIDList.FindIndex(x => x == IDByte);
                                            DiagnosticsTable.RemoveAt(SCIBusPCMListStart + location);
                                            DiagnosticsTable.Insert(SCIBusPCMListStart + location, scipcmlistitem.ToString());
                                        }
                                        SCIBusPCMMsgRxCount++;

                                        if (IncludeTimestampInLogFilesToolStripMenuItem.Checked)
                                        {
                                            TimeSpan SCIBusPCMElapsedTime = TimeSpan.FromMilliseconds(pcmtimestamp[0] << 24 | pcmtimestamp[1] << 16 | pcmtimestamp[2] << 8 | pcmtimestamp[3]);
                                            DateTime SCIBusPCMTimestamp = DateTime.Today.Add(SCIBusPCMElapsedTime);
                                            string SCIBusPCMTimestampString = SCIBusPCMTimestamp.ToString("HH:mm:ss.fff") + " ";
                                            File.AppendAllText(PCMLogFilename, SCIBusPCMTimestampString); // no newline is appended!
                                        }
                                        File.AppendAllText(PCMLogFilename, Util.ByteToHexString(pcmmessagemix, 0, pcmmessagemix.Length) + Environment.NewLine); // save message to text file
                                    }

                                    SCIBusPCMHeaderText = SCIBusPCMStateLineEnabled;
                                    SCIBusPCMHeaderText = SCIBusPCMHeaderText.Replace("@  BAUD", "@ " + SCIBusPCMHeaderBaud + " BAUD").
                                                                        Replace("CONFIGURATION: ", "CONFIGURATION: " + SCIBusPCMHeaderConfiguration).
                                                                        Replace("# OF MESSAGES: RX=", "# OF MESSAGES: RX=" + SCIBusPCMMsgRxCount.ToString()).
                                                                        Replace(" TX=", " TX=" + SCIBusPCMMsgTxCount.ToString());
                                    SCIBusPCMHeaderText = Util.Truncate(SCIBusPCMHeaderText, EmptyLine.Length); // Replacing strings causes the base string to grow so cut it back to stay in line
                                    DiagnosticsTable.RemoveAt(SCIBusPCMHeaderStart);
                                    DiagnosticsTable.Insert(SCIBusPCMHeaderStart, SCIBusPCMHeaderText);

                                    UpdateDiagnosticsListBox();

                                    Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus message (PCM)", Packet);

                                    if (MemoryReadFinished)
                                    {
                                        switch (MemoryReadError)
                                        {
                                            case 0: // ok
                                                Util.UpdateTextBox(USBTextBox, "[INFO] EPROM/EEPROM binary export finished", null);
                                                break;
                                            case 1: // missing messages
                                                Util.UpdateTextBox(USBTextBox, "[INFO] EPROM/EEPROM text message missing" + Environment.NewLine +
                                                                               "       Number of lines don't match up with" + Environment.NewLine +
                                                                               "       expected binary size." + Environment.NewLine +
                                                                               "       Restart application and try again.", null);
                                                break;
                                            case 2: // memory value missing
                                                Util.UpdateTextBox(USBTextBox, "[INFO] EPROM/EEPROM text message incomplete" + Environment.NewLine +
                                                                               "       Some messages lack the memory value" + Environment.NewLine +
                                                                               "       that is needed to build the binary file" + Environment.NewLine +
                                                                               "       Restart application and try again.", null);
                                                break;
                                            default:
                                                Util.UpdateTextBox(USBTextBox, "[INFO] EPROM/EEPROM reading error", null);
                                                break;
                                        }

                                        MemoryReadFinished = false;
                                        MemoryReadError = 0;
                                    }
                                    break;
                                case 0x03: // SCI-bus (TCM)
                                    IDByte = Payload[4];
                                    SCIBusTCMEnabled = true;
                                    StringBuilder scitcmlistitem = new StringBuilder(EmptyLine); // start with a pre-defined empty line
                                    string scitcmmsgtoinsert = String.Empty;
                                    byte[] tcmtimestamp = new byte[4];
                                    byte[] tcmmessage = new byte[Payload.Length - 4];
                                    Array.Copy(Payload, 0, tcmtimestamp, 0, 4); // copy timestamp only
                                    Array.Copy(Payload, 4, tcmmessage, 0, Payload.Length - 4); // copy message only

                                    // In case of high speed mode the request and response bytes are sent in separate packets.
                                    // First packet is always the request bytes list (SubDataCode 0x01), save them here and do nothing else.
                                    // Second packet contains the response bytes (SubDataCode 0x02), mix it with the request bytes and update the table.
                                    if (SubDataCode == 0x01) // low speed bytes
                                    {
                                        if (tcmmessage.Length < 9) // max 8 byte fits the message column
                                        {
                                            scitcmmsgtoinsert = Util.ByteToHexString(tcmmessage, 0, tcmmessage.Length) + " ";
                                        }
                                        else // Trim message and insert two dots at the end indicating there's more to it
                                        {
                                            scitcmmsgtoinsert = Util.ByteToHexString(tcmmessage, 0, 7) + " .. ";
                                        }

                                        scitcmlistitem.Remove(2, scitcmmsgtoinsert.Length); // remove as much whitespaces as the length of the message
                                        scitcmlistitem.Insert(2, scitcmmsgtoinsert); // insert message where whitespaces were

                                        // Now decide where to put this message in reality
                                        if (!SCIBusTCMIDList.Contains(IDByte)) // if this ID-byte is not on the list
                                        {
                                            SCIBusTCMIDByteNum++;
                                            SCIBusTCMIDList.Add(IDByte); // add ID-byte to the list
                                            if (IDSorting)
                                            {
                                                SCIBusTCMIDList.Sort(); // sort ID-bytes by ascending order
                                            }
                                            location = SCIBusTCMIDList.FindIndex(x => x == IDByte); // now see where this new ID-byte ends up after sorting
                                            if (SCIBusTCMIDList.Count == 1)
                                            {
                                                DiagnosticsTable.RemoveAt(SCIBusTCMListStart);
                                                DiagnosticsTable.Insert(SCIBusTCMListStart, scitcmlistitem.ToString());
                                            }
                                            else
                                            {
                                                DiagnosticsTable.Insert(SCIBusTCMListStart + location, scitcmlistitem.ToString());
                                                SCIBusTCMListEnd++; // increment start/end row-numbers (new data causes the table to grow)
                                            }
                                        }
                                        else // if this ID-byte is already displayed
                                        {
                                            location = SCIBusTCMIDList.FindIndex(x => x == IDByte);
                                            DiagnosticsTable.RemoveAt(SCIBusTCMListStart + location);
                                            DiagnosticsTable.Insert(SCIBusTCMListStart + location, scitcmlistitem.ToString());
                                        }
                                        SCIBusTCMMsgRxCount++;

                                        if (IncludeTimestampInLogFilesToolStripMenuItem.Checked)
                                        {
                                            TimeSpan SCIBusTCMElapsedTime = TimeSpan.FromMilliseconds(tcmtimestamp[0] << 24 | tcmtimestamp[1] << 16 | tcmtimestamp[2] << 8 | tcmtimestamp[3]);
                                            DateTime SCIBusTCMTimestamp = DateTime.Today.Add(SCIBusTCMElapsedTime);
                                            string SCIBusTCMTimestampString = SCIBusTCMTimestamp.ToString("HH:mm:ss.fff") + " ";
                                            File.AppendAllText(TCMLogFilename, SCIBusTCMTimestampString); // no newline is appended!
                                        }
                                        File.AppendAllText(TCMLogFilename, Util.ByteToHexString(tcmmessage, 0, tcmmessage.Length) + Environment.NewLine); // save message to text file
                                                                                                                                                          // TODO: update header
                                    }
                                    if (SubDataCode == 0x02) // high speed request bytes, just save them
                                    {
                                        SCIBusTCMReqMsgList.Clear(); // first clear previous bytes
                                        for (int i = 0; i < tcmmessage.Length; i++)
                                        {
                                            SCIBusTCMReqMsgList.Add(tcmmessage[i]);
                                        }
                                    }
                                    else if (SubDataCode == 0x03) // high speed response bytes, mix them with saved request bytes
                                    {
                                        byte[] requestbytes = SCIBusTCMReqMsgList.ToArray();
                                        List<byte> temp = new List<byte>(512);
                                        byte[] tcmmessagemix;
                                        temp.Add(tcmmessage[0]); // first byte is always the memory area byte
                                        for (int i = 1; i < tcmmessage.Length; i++)
                                        {
                                            temp.Add(requestbytes[i]); // Add request byte first
                                            temp.Add(tcmmessage[i]); // Add response byte next
                                        }

                                        tcmmessagemix = temp.ToArray();

                                        if (tcmmessagemix.Length < 9) // max 8 byte fits the message column
                                        {
                                            scitcmmsgtoinsert = Util.ByteToHexString(tcmmessagemix, 0, tcmmessagemix.Length) + " ";
                                        }
                                        else // Trim message and insert two dots at the end indicating there's more to it
                                        {
                                            scitcmmsgtoinsert = Util.ByteToHexString(tcmmessagemix, 0, 7) + " .. ";
                                        }

                                        scitcmlistitem.Remove(2, scitcmmsgtoinsert.Length); // remove as much whitespaces as the length of the message
                                        scitcmlistitem.Insert(2, scitcmmsgtoinsert); // insert message where whitespaces were

                                        // Now decide where to put this message in reality
                                        if (!SCIBusTCMIDList.Contains(IDByte)) // if this ID-byte is not on the list
                                        {
                                            SCIBusTCMIDByteNum++;
                                            SCIBusTCMIDList.Add(IDByte); // add ID-byte to the list
                                            if (IDSorting)
                                            {
                                                SCIBusTCMIDList.Sort(); // sort ID-bytes by ascending order
                                            }
                                            location = SCIBusTCMIDList.FindIndex(x => x == IDByte); // now see where this new ID-byte ends up after sorting
                                            if (SCIBusTCMIDList.Count == 1)
                                            {
                                                DiagnosticsTable.RemoveAt(SCIBusTCMListStart);
                                                DiagnosticsTable.Insert(SCIBusTCMListStart, scitcmlistitem.ToString());
                                            }
                                            else
                                            {
                                                DiagnosticsTable.Insert(SCIBusTCMListStart + location, scitcmlistitem.ToString());
                                                SCIBusTCMListEnd++; // increment start/end row-numbers (new data causes the table to grow)
                                            }
                                        }
                                        else // if this ID-byte is already displayed
                                        {
                                            location = SCIBusTCMIDList.FindIndex(x => x == IDByte);
                                            DiagnosticsTable.RemoveAt(SCIBusTCMListStart + location);
                                            DiagnosticsTable.Insert(SCIBusTCMListStart + location, scitcmlistitem.ToString());
                                        }
                                        SCIBusTCMMsgRxCount++;

                                        if (IncludeTimestampInLogFilesToolStripMenuItem.Checked)
                                        {
                                            TimeSpan SCIBusTCMElapsedTime = TimeSpan.FromMilliseconds(tcmtimestamp[0] << 24 | tcmtimestamp[1] << 16 | tcmtimestamp[2] << 8 | tcmtimestamp[3]);
                                            DateTime SCIBusTCMTimestamp = DateTime.Today.Add(SCIBusTCMElapsedTime);
                                            string SCIBusTCMTimestampString = SCIBusTCMTimestamp.ToString("HH:mm:ss.fff") + " ";
                                            File.AppendAllText(TCMLogFilename, SCIBusTCMTimestampString); // no newline is appended!
                                        }
                                        File.AppendAllText(TCMLogFilename, Util.ByteToHexString(tcmmessagemix, 0, tcmmessagemix.Length) + Environment.NewLine); // save message to text file
                                    }

                                    SCIBusTCMHeaderText = SCIBusTCMStateLineEnabled;
                                    SCIBusTCMHeaderText = SCIBusTCMHeaderText.Replace("@  BAUD", "@ " + SCIBusTCMHeaderBaud + " BAUD").
                                                                        Replace("CONFIGURATION: ", "CONFIGURATION: " + SCIBusTCMHeaderConfiguration).
                                                                        Replace("# OF MESSAGES: RX=", "# OF MESSAGES: RX=" + SCIBusTCMMsgRxCount.ToString()).
                                                                        Replace(" TX=", " TX=" + SCIBusTCMMsgTxCount.ToString());
                                    SCIBusTCMHeaderText = Util.Truncate(SCIBusTCMHeaderText, EmptyLine.Length); // Replacing strings causes the base string to grow so cut it back to stay in line
                                    DiagnosticsTable.RemoveAt(SCIBusTCMHeaderStart);
                                    DiagnosticsTable.Insert(SCIBusTCMHeaderStart, SCIBusTCMHeaderText);
                                    UpdateDiagnosticsListBox();
                                    Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus message (TCM)", Packet);
                                    break;
                                default:
                                    Util.UpdateTextBox(USBTextBox, "[RX->] Data received", Packet);
                                    break;
                            }
                        }
                        else
                        {
                            Util.UpdateTextBox(USBTextBox, "[RX->] Data received with checksum error", Packet);
                        }

                        bufferlist.RemoveRange(0, FullPacketLength);
                        break;
                    case 0x3E: // ">"
                        if (bufferlist[bufferlist.Count - 1] == 0x0A) // if last character is EOL (end of line)
                        {
                            string line = Encoding.ASCII.GetString(bufferlist.Take(bufferlist.Count - 1).ToArray()); // convert to ASCII text except the last character (EOL)
                            Util.UpdateTextBox(USBTextBox, line, null);

                            // Save raw USB packet to a binary logfile
                            using (BinaryWriter writer = new BinaryWriter(File.Open(USBBinaryLogFilename, FileMode.Append)))
                            {
                                writer.Write(bufferlist.ToArray());
                                writer.Close();
                            }

                            bufferlist.Clear();
                        }
                        break;
                    default:
                        bufferlist.RemoveAt(0); // remove this byte and see what's next
                        break;
                }
            }
        }

        private void UpdateDiagnosticsListBox()
        {
            if (DiagnosticsListBox.InvokeRequired)
            {
                DiagnosticsListBox.BeginInvoke((MethodInvoker)delegate
                {
                    //DiagnosticsListBox.SuspendLayout();
                    DiagnosticsListBox.BeginUpdate();
                    int savedVpos = GetScrollPos((IntPtr)DiagnosticsListBox.Handle, SB_VERT);
                    DiagnosticsListBox.Items.Clear();

                    try
                    {
                        DiagnosticsListBox.Items.AddRange(DiagnosticsTable.ToArray());
                    }
                    catch
                    {
                        ResetViewButton.PerformClick();
                    }

                    DiagnosticsListBox.Update();
                    DiagnosticsListBox.Refresh();
                    SetScrollPos((IntPtr)DiagnosticsListBox.Handle, SB_VERT, savedVpos, true);
                    PostMessageA((IntPtr)DiagnosticsListBox.Handle, WM_VSCROLL, SB_THUMBPOSITION + 0x10000 * savedVpos, 0);
                    DiagnosticsListBox.EndUpdate();
                    //DiagnosticsListBox.ResumeLayout();
                });
            }
            else
            {
                //DiagnosticsListBox.SuspendLayout();
                DiagnosticsListBox.BeginUpdate();
                int savedVpos = GetScrollPos((IntPtr)DiagnosticsListBox.Handle, SB_VERT);
                DiagnosticsListBox.Items.Clear();

                try
                {
                    DiagnosticsListBox.Items.AddRange(DiagnosticsTable.ToArray());
                }
                catch
                {
                    ResetViewButton.PerformClick();
                }

                DiagnosticsListBox.Update();
                DiagnosticsListBox.Refresh();
                SetScrollPos((IntPtr)DiagnosticsListBox.Handle, SB_VERT, savedVpos, true);
                PostMessageA((IntPtr)DiagnosticsListBox.Handle, WM_VSCROLL, SB_THUMBPOSITION + 0x10000 * savedVpos, 0);
                DiagnosticsListBox.EndUpdate();
                //DiagnosticsListBox.ResumeLayout();
            }
        }

        private void ResetViewButton_Click(object sender, EventArgs e)
        {
            DiagnosticsListBox.Items.Clear();
            DiagnosticsTable.Clear(); // delete everything
            DiagnosticsTable.Add("┌─────────────────────────┐                                                                                               ");
            DiagnosticsTable.Add("│ CCD-BUS MODULES         │ STATE: N/A                                                                                    ");
            DiagnosticsTable.Add("├─────────────────────────┼─────────────────────────────────────────────────────┬─────────────────────────┬──────────────┐");
            DiagnosticsTable.Add("│ MESSAGE [HEX]           │ DESCRIPTION                                         │ VALUE                   │ UNIT         │");
            DiagnosticsTable.Add("╞═════════════════════════╪═════════════════════════════════════════════════════╪═════════════════════════╪══════════════╡");
            DiagnosticsTable.Add("│                         │                                                     │                         │              │");
            DiagnosticsTable.Add("├─────────────────────────┼─────────────────────────────────────────────────────┼─────────────────────────┼──────────────┤");
            DiagnosticsTable.Add("│ B2 -- -- -- -- --       │ DRB REQUEST                                         │                         │              │");
            DiagnosticsTable.Add("│ F2 -- -- -- -- --       │ DRB RESPONSE                                        │                         │              │");
            DiagnosticsTable.Add("└─────────────────────────┴─────────────────────────────────────────────────────┴─────────────────────────┴──────────────┘");
            DiagnosticsTable.Add("                                                                                                                          ");
            DiagnosticsTable.Add("                                                                                                                          ");
            DiagnosticsTable.Add("┌─────────────────────────┐                                                                                               ");
            DiagnosticsTable.Add("│ SCI-BUS ENGINE          │ STATE: N/A                                                                                    ");
            DiagnosticsTable.Add("├─────────────────────────┼─────────────────────────────────────────────────────┬─────────────────────────┬──────────────┐");
            DiagnosticsTable.Add("│ MESSAGE [HEX]           │ DESCRIPTION                                         │ VALUE                   │ UNIT         │");
            DiagnosticsTable.Add("╞═════════════════════════╪═════════════════════════════════════════════════════╪═════════════════════════╪══════════════╡");
            DiagnosticsTable.Add("│                         │                                                     │                         │              │");
            DiagnosticsTable.Add("└─────────────────────────┴─────────────────────────────────────────────────────┴─────────────────────────┴──────────────┘");
            DiagnosticsTable.Add("                                                                                                                          ");
            DiagnosticsTable.Add("                                                                                                                          ");
            DiagnosticsTable.Add("┌─────────────────────────┐                                                                                               ");
            DiagnosticsTable.Add("│ SCI-BUS TRANSMISSION    │ STATE: N/A                                                                                    ");
            DiagnosticsTable.Add("├─────────────────────────┼─────────────────────────────────────────────────────┬─────────────────────────┬──────────────┐");
            DiagnosticsTable.Add("│ MESSAGE [HEX]           │ DESCRIPTION                                         │ VALUE                   │ UNIT         │");
            DiagnosticsTable.Add("╞═════════════════════════╪═════════════════════════════════════════════════════╪═════════════════════════╪══════════════╡");
            DiagnosticsTable.Add("│                         │                                                     │                         │              │");
            DiagnosticsTable.Add("└─────────────────────────┴─────────────────────────────────────────────────────┴─────────────────────────┴──────────────┘");
            DiagnosticsTable.Add("                                                                                                                          ");
            //OldDiagnosticsTable.Clear();
            UpdateDiagnosticsListBox(); // force table update

            CCDBusHeaderStart = 1;
            CCDBusListStart = 5;
            CCDBusListEnd = 5;
            CCDBusB2Start = 7;
            CCDBusF2Start = 8;
            SCIBusPCMHeaderStart = 13;
            SCIBusPCMListStart = 17;
            SCIBusPCMListEnd = 17;
            SCIBusTCMHeaderStart = 22;
            SCIBusTCMListStart = 26;
            SCIBusTCMListEnd = 26;

            CCDBusB2MsgPresent = false;
            CCDBusF2MsgPresent = false;
            CCDBusIDByteNum = 0;
            SCIBusPCMIDByteNum = 0;
            SCIBusTCMIDByteNum = 0;

            CCDbusMsgList.Clear();
            SCIBusPCMMsgList.Clear();
            SCIBusPCMReqMsgList.Clear();
            SCIBusTCMMsgList.Clear();
            SCIBusTCMReqMsgList.Clear();
            CCDBusIDList.Clear();
            SCIBusPCMIDList.Clear();
            SCIBusTCMIDList.Clear();
        }

        private void USBClearAllButton_Click(object sender, EventArgs e)
        {
            USBTextBox.Clear();
            USBSendComboBox.Text = String.Empty;
            USBSendComboBox.Items.Clear();
        }

        private void USBClearButton_Click(object sender, EventArgs e)
        {
            USBSendComboBox.Text = String.Empty;
        }

        private void USBSendButton_Click(object sender, EventArgs e)
        {
            if (ScannerFound)
            {
                if (USBSendComboBox.Text != String.Empty)
                {
                    if (HexCommMethodRadioButton.Checked)
                    {
                        UpdateUSBSendComboBox();

                        byte[] bytes = Util.HexStringToByte(USBSendComboBox.Text);
                        if ((bytes.Length > 5) && (bytes != null))
                        {
                            Util.UpdateTextBox(USBTextBox, "[<-TX] Data transmitted", bytes);
                            Serial.Write(bytes, 0, bytes.Length);

                            if (!USBSendComboBox.Items.Contains(USBSendComboBox.Text)) // only add unique items (no repeat!)
                            {
                                USBSendComboBox.Items.Add(USBSendComboBox.Text); // add command to the list so it can be selected later
                            }

                            if ((TargetComboBox.SelectedIndex == 1) && (CommandComboBox.SelectedIndex == 0) && (ModeComboBox.SelectedIndex == 1) && (Param1ComboBox.Text != String.Empty))
                            {
                                CCDBusMsgTxCount++;
                            }

                            if ((TargetComboBox.SelectedIndex == 2) && (CommandComboBox.SelectedIndex == 0) && (ModeComboBox.SelectedIndex == 1) && (Param1ComboBox.Text != String.Empty))
                            {
                                SCIBusPCMMsgTxCount++;
                            }

                            if ((TargetComboBox.SelectedIndex == 3) && (CommandComboBox.SelectedIndex == 0) && (ModeComboBox.SelectedIndex == 1) && (Param1ComboBox.Text != String.Empty))
                            {
                                SCIBusTCMMsgTxCount++;
                            }
                        }
                    }
                    else if (AsciiCommMethodRadioButton.Checked)
                    {
                        Util.UpdateTextBox(USBTextBox, USBSendComboBox.Text, null);
                        List<byte> Message = new List<byte>();
                        Message.AddRange(Encoding.ASCII.GetBytes(USBSendComboBox.Text));
                        Message.Add(0x0A); // add newline character at the end manually
                        byte[] MessageBytes = Message.ToArray();
                        if ((MessageBytes.Length > 1) && (MessageBytes != null))
                        {
                            Serial.Write(MessageBytes, 0, MessageBytes.Length);

                            // Save raw USB packet to a binary logfile
                            using (BinaryWriter writer = new BinaryWriter(File.Open(USBBinaryLogFilename, FileMode.Append)))
                            {
                                if (MessageBytes != null)
                                {
                                    writer.Write(MessageBytes);
                                    writer.Close();
                                }
                            }

                            if (!USBSendComboBox.Items.Contains(USBSendComboBox.Text)) // only add unique items (no repeat!)
                            {
                                USBSendComboBox.Items.Add(USBSendComboBox.Text); // add command to the list so it can be selected later
                            }
                        }
                    }
                }
            }
        }

        private void ExpandButton_Click(object sender, EventArgs e)
        {
            if (ExpandButton.Text == "Expand >>")
            {
                this.Size = new Size(1300, 650);
                this.CenterToScreen();
                DiagnosticsGroupBox.Visible = true;
                ExpandButton.Text = "<< Collapse";
            }
            else if (ExpandButton.Text == "<< Collapse")
            {
                DiagnosticsGroupBox.Visible = false;
                this.Size = new Size(405, 650);
                this.CenterToScreen();
                ExpandButton.Text = "Expand >>";
            }
        }

        private void CommMethodRadioButtons_CheckedChanged(object sender, EventArgs e)
        {
            if (HexCommMethodRadioButton.Checked && !AsciiCommMethodRadioButton.Checked)
            {
                TargetLabel.Visible = true;
                TargetComboBox.Visible = true;
                CommandLabel.Visible = true;
                ModeComboBox.Visible = true;
                ModeLabel.Visible = true;
                CommandComboBox.Visible = true;
                HintTextBox.Visible = true;
                PreviewLabel.Text = "Preview:";
                TargetComboBox_SelectedIndexChanged(this, EventArgs.Empty);
                CommandComboBox_SelectedIndexChanged(this, EventArgs.Empty);
                ModeComboBox_SelectedIndexChanged(this, EventArgs.Empty); // Workaround
                USBTextBox.Size = new Size(359, 220);
                TM = (byte)TransmissionMethod.Hex;
                USBTextBox.ScrollToCaret();
                try
                {
                    Properties.Settings.Default["TransmissionMethod"] = "hex";
                    Properties.Settings.Default.Save(); // Save settings in application configuration file
                }
                catch
                {

                }
            }
            else if (!HexCommMethodRadioButton.Checked && AsciiCommMethodRadioButton.Checked)
            {
                TargetLabel.Visible = false;
                TargetComboBox.Visible = false;
                CommandLabel.Visible = false;
                ModeComboBox.Visible = false;
                ModeLabel.Visible = false;
                CommandComboBox.Visible = false;
                HintTextBox.Visible = false;
                Param1Label1.Visible = false;
                Param1ComboBox.Visible = false;
                Param1Label2.Visible = false;
                Param2Label1.Visible = false;
                Param2ComboBox.Visible = false;
                Param2Label2.Visible = false;
                Param3Label1.Visible = false;
                Param3ComboBox.Visible = false;
                Param3Label2.Visible = false;
                PreviewLabel.Text = "Message:";
                USBSendComboBox.Text = ">";
                USBTextBox.Size = new Size(359, 430);
                TM = (byte)TransmissionMethod.Ascii;
                USBTextBox.ScrollToCaret();
                try
                {
                    Properties.Settings.Default["TransmissionMethod"] = "ascii";
                    Properties.Settings.Default.Save(); // Save settings in application configuration file
                }
                catch
                {

                }
            }
        }

        private void USBSendComboBox_TextChanged(object sender, EventArgs e)
        {
            if (USBCommunicationGroupBox.Enabled && AsciiCommMethodRadioButton.Checked && ((USBSendComboBox.Text == String.Empty) || (USBSendComboBox.Text.Length == 1)))
            {
                USBSendComboBox.Text = ">";
                SendKeys.Send("{End}");
            }
        }

        private void USBSendComboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                USBSendButton.PerformClick();
                e.Handled = true;
            }
        }

        private void UpdateUSBSendComboBox()
        {
            byte[] USBSendComboBoxValue = new byte[] { 0x00 };
            PacketBytes.Clear();
            PacketBytesChecksum = 0;

            switch (TargetComboBox.SelectedIndex)
            {
                case 0: // Scanner
                    switch (CommandComboBox.SelectedIndex)
                    {
                        case 0: // Reset
                            USBSendComboBoxValue = new byte[] { 0x3D, 0x00, 0x02, 0x00, 0x00, 0x02};
                            break;
                        case 1: // Handshake request
                            switch (ModeComboBox.SelectedIndex)
                            {
                                case 0: // Handshake only
                                    USBSendComboBoxValue = new byte[] { 0x3D, 0x00, 0x02, 0x01, 0x00, 0x03 };
                                    break;
                                case 1: // Handshake + hardware/firmare information
                                    USBSendComboBoxValue = new byte[] { 0x3D, 0x00, 0x02, 0x01, 0x01, 0x04 };
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case 2: // Status request
                            USBSendComboBoxValue = new byte[] { 0x3D, 0x00, 0x02, 0x02, 0x00, 0x04 };
                            break;
                        case 3: // Settings
                            switch (ModeComboBox.SelectedIndex)
                            {
                                case 0: // Heartbeat
                                    byte HeartbeatIntervalHB = (byte)((HeartbeatInterval >> 8) & 0xFF);
                                    byte HeartbeatIntervalLB = (byte)(HeartbeatInterval & 0xFF);
                                    byte HeartbeatDurationHB = (byte)((HeartbeatDuration >> 8) & 0xFF);
                                    byte HeartbeatDurationLB = (byte)(HeartbeatDuration & 0xFF);

                                    PacketBytes.AddRange(new byte[] { 0x3D, 0x00, 0x06, 0x03, 0x01, HeartbeatIntervalHB, HeartbeatIntervalLB, HeartbeatDurationHB, HeartbeatDurationLB });

                                    for (int i = 1; i < PacketBytes.Count; i++)
                                    {
                                        PacketBytesChecksum += PacketBytes[i];
                                    }

                                    PacketBytes.Add(PacketBytesChecksum);
                                    USBSendComboBoxValue = PacketBytes.ToArray();
                                    break;
                                case 1: // Set CCD-bus
                                    PacketBytes.AddRange(new byte[] { 0x3D, 0x00, 0x03, 0x03, 0x02 });
                                    if (SetCCDBus) PacketBytes.Add(0x01);
                                    else PacketBytes.Add(0x00);

                                    for (int i = 1; i < PacketBytes.Count; i++)
                                    {
                                        PacketBytesChecksum += PacketBytes[i];
                                    }

                                    PacketBytes.Add(PacketBytesChecksum);
                                    USBSendComboBoxValue = PacketBytes.ToArray();
                                    break;
                                case 2: // Set SCI-bus
                                    PacketBytes.AddRange(new byte[] { 0x3D, 0x00, 0x03, 0x03, 0x03, SetSCIBus });
                                    for (int i = 1; i < PacketBytes.Count; i++)
                                    {
                                        PacketBytesChecksum += PacketBytes[i];
                                    }
                                    PacketBytes.Add(PacketBytesChecksum);
                                    USBSendComboBoxValue = PacketBytes.ToArray();
                                    break;
                                case 3: // Repeated message behavior
                                    byte bus = (byte)(Param1ComboBox.SelectedIndex + 1);
                                    byte RepeatIntervalHB = (byte)((RepeatInterval >> 8) & 0xFF);
                                    byte RepeatIntervalLB = (byte)(RepeatInterval & 0xFF);
                                    byte RepeatIncrementHB = (byte)((RepeatIncrement >> 8) & 0xFF);
                                    byte RepeatIncrementLB = (byte)(RepeatIncrement & 0xFF);

                                    PacketBytes.AddRange(new byte[] { 0x3D, 0x00, 0x07, 0x03, 0x04, bus, RepeatIntervalHB, RepeatIntervalLB, RepeatIncrementHB, RepeatIncrementLB });

                                    for (int i = 1; i < PacketBytes.Count; i++)
                                    {
                                        PacketBytesChecksum += PacketBytes[i];
                                    }

                                    PacketBytes.Add(PacketBytesChecksum);
                                    USBSendComboBoxValue = PacketBytes.ToArray();
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case 4: // Request
                            switch (ModeComboBox.SelectedIndex)
                            {
                                case 0: // Hardware/Firmware information
                                    USBSendComboBoxValue = new byte[] { 0x3D, 0x00, 0x02, 0x04, 0x01, 0x07 };
                                    break;
                                case 1: // Timestamp
                                    USBSendComboBoxValue = new byte[] { 0x3D, 0x00, 0x02, 0x04, 0x02, 0x08 };
                                    break;
                                case 2: // Battery voltage
                                    USBSendComboBoxValue = new byte[] { 0x3D, 0x00, 0x02, 0x04, 0x03, 0x09 };
                                    break;
                                case 3: // External EEPROM checksum
                                    USBSendComboBoxValue = new byte[] { 0x3D, 0x00, 0x02, 0x04, 0x04, 0x0A };
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case 5: // Debug
                            switch (ModeComboBox.SelectedIndex)
                            {
                                case 0: // Generate random CCD-bus messages
                                    PacketBytes.AddRange(new byte[] { 0x3D, 0x00, 0x07, 0x0E, 0x01 });
                                    if (Param3ComboBox.SelectedIndex == 0) PacketBytes.Add(0x00); // false
                                    else PacketBytes.Add(0x01); // true

                                    byte MinIntervalHB = (byte)((RandomCCDMessageIntervalMin >> 8) & 0xFF);
                                    byte MinIntervalLB = (byte)(RandomCCDMessageIntervalMin & 0xFF);
                                    byte MaxIntervalHB = (byte)((RandomCCDMessageIntervalMax >> 8) & 0xFF);
                                    byte MaxIntervalLB = (byte)(RandomCCDMessageIntervalMax & 0xFF);

                                    PacketBytes.AddRange(new byte[] { MinIntervalHB, MinIntervalLB, MaxIntervalHB, MaxIntervalLB });

                                    for (int i = 1; i < PacketBytes.Count; i++)
                                    {
                                        PacketBytesChecksum += PacketBytes[i];
                                    }

                                    PacketBytes.Add(PacketBytesChecksum);
                                    USBSendComboBoxValue = PacketBytes.ToArray();
                                    break;
                                case 1: // Read external EEPROM
                                    PacketBytes.AddRange(new byte[] { 0x3D, 0x00, 0x04, 0x0E, 0x02 });
                                    PacketBytes.AddRange(extEEPROMaddress);

                                    for (int i = 1; i < PacketBytes.Count; i++)
                                    {
                                        PacketBytesChecksum += PacketBytes[i];
                                    }

                                    PacketBytes.Add(PacketBytesChecksum);
                                    USBSendComboBoxValue = PacketBytes.ToArray();
                                    break;
                                case 2: // Write external EEPROM
                                    PacketBytes.AddRange(new byte[] { 0x3D, 0x00, 0x05, 0x0E, 0x03 });
                                    PacketBytes.AddRange(extEEPROMaddress);
                                    PacketBytes.AddRange(extEEPROMvalue);

                                    for (int i = 1; i < PacketBytes.Count; i++)
                                    {
                                        PacketBytesChecksum += PacketBytes[i]; 
                                    }

                                    PacketBytes.Add(PacketBytesChecksum);
                                    USBSendComboBoxValue = PacketBytes.ToArray();
                                    break;
                                case 3: // Set arbitrary UART speed
                                    break;
                                default:
                                    USBSendComboBoxValue = new byte[] { 0x3D, 0x00, 0x02, 0x0E, 0x00, 0x10 };
                                    break;
                            }   
                            break;
                        default:
                            break;
                    }
                    break;
                case 1: // CCD-bus
                    switch (CommandComboBox.SelectedIndex)
                    {
                        case 0: // Send message
                            switch (ModeComboBox.SelectedIndex)
                            {
                                case 0: // Stop message transmission
                                    USBSendComboBoxValue = new byte[] { 0x3D, 0x00, 0x02, 0x16, 0x01, 0x19 };
                                    RepeatIterate = false;
                                    break;
                                case 1: // Single message
                                    RepeatIterate = false;

                                    PacketLengthHB = (byte)((2 + CCDBusMessageToSendStart.Length) >> 8);
                                    PacketLengthLB = (byte)((2 + CCDBusMessageToSendStart.Length) & 0xFF);

                                    PacketBytes.AddRange(new byte[] { 0x3D, PacketLengthHB, PacketLengthLB, 0x16, 0x02 });
                                    PacketBytes.AddRange(CCDBusMessageToSendStart);

                                    for (int i = 1; i < PacketBytes.Count; i++)
                                    {
                                        PacketBytesChecksum += PacketBytes[i];
                                    }

                                    PacketBytes.Add(PacketBytesChecksum);
                                    USBSendComboBoxValue = PacketBytes.ToArray();
                                    break;
                                case 2: // Repeated message
                                    if (Param3ComboBox.SelectedIndex == 0) // no iteration
                                    {
                                        RepeatIterate = false;

                                        PacketLengthHB = (byte)((4 + CCDBusMessageToSendStart.Length) >> 8);
                                        PacketLengthLB = (byte)((4 + CCDBusMessageToSendStart.Length) & 0xFF);

                                        PacketBytes.AddRange(new byte[] { 0x3D, PacketLengthHB, PacketLengthLB, 0x16, 0x03, 0x00, (byte)CCDBusMessageToSendStart.Length });
                                        PacketBytes.AddRange(CCDBusMessageToSendStart);

                                        for (int i = 1; i < PacketBytes.Count; i++)
                                        {
                                            PacketBytesChecksum += PacketBytes[i];
                                        }

                                        PacketBytes.Add(PacketBytesChecksum);
                                    }
                                    else if (Param3ComboBox.SelectedIndex == 1) // iteration, same message length supposed
                                    {
                                        RepeatIterate = true;
                                        
                                        PacketLengthHB = (byte)((4 + CCDBusMessageToSendStart.Length + CCDBusMessageToSendEnd.Length) >> 8);
                                        PacketLengthLB = (byte)((4 + CCDBusMessageToSendStart.Length + CCDBusMessageToSendEnd.Length) & 0xFF);

                                        PacketBytes.AddRange(new byte[] { 0x3D, PacketLengthHB, PacketLengthLB, 0x16, 0x03, 0x01, (byte)CCDBusMessageToSendStart.Length });
                                        PacketBytes.AddRange(CCDBusMessageToSendStart);
                                        PacketBytes.AddRange(CCDBusMessageToSendEnd);

                                        for (int i = 1; i < PacketBytes.Count; i++)
                                        {
                                            PacketBytesChecksum += PacketBytes[i];
                                        }

                                        PacketBytes.Add(PacketBytesChecksum);
                                    }

                                    USBSendComboBoxValue = PacketBytes.ToArray();
                                    break;
                                default:
                                    break;
                            }
                            break;
                        default:
                            break;
                    }
                    break;
                case 2: // SCI-bus (PCM)
                    switch (CommandComboBox.SelectedIndex)
                    {
                        case 0: // Send message
                            switch (ModeComboBox.SelectedIndex)
                            {
                                case 0: // Stop message transmission
                                    USBSendComboBoxValue = new byte[] { 0x3D, 0x00, 0x02, 0x26, 0x01, 0x29 };
                                    RepeatIterate = false;
                                    break;
                                case 1: // Single message
                                    RepeatIterate = false;

                                    PacketLengthHB = (byte)((2 + SCIBusPCMMessageToSendStart.Length) >> 8);
                                    PacketLengthLB = (byte)((2 + SCIBusPCMMessageToSendStart.Length) & 0xFF);

                                    PacketBytes.AddRange(new byte[] { 0x3D, PacketLengthHB, PacketLengthLB, 0x26, 0x02 });
                                    PacketBytes.AddRange(SCIBusPCMMessageToSendStart);

                                    for (int i = 1; i < PacketBytes.Count; i++)
                                    {
                                        PacketBytesChecksum += PacketBytes[i];
                                    }

                                    PacketBytes.Add(PacketBytesChecksum);
                                    USBSendComboBoxValue = PacketBytes.ToArray();
                                    break;
                                case 2: // Repeated message
                                    if (Param3ComboBox.SelectedIndex == 0) // no iteration
                                    {
                                        RepeatIterate = false;

                                        PacketLengthHB = (byte)((4 + SCIBusPCMMessageToSendStart.Length) >> 8);
                                        PacketLengthLB = (byte)((4 + SCIBusPCMMessageToSendStart.Length) & 0xFF);

                                        PacketBytes.AddRange(new byte[] { 0x3D, PacketLengthHB, PacketLengthLB, 0x26, 0x03, 0x00, (byte)SCIBusPCMMessageToSendStart.Length });
                                        PacketBytes.AddRange(SCIBusPCMMessageToSendStart);

                                        for (int i = 1; i < PacketBytes.Count; i++)
                                        {
                                            PacketBytesChecksum += PacketBytes[i];
                                        }

                                        PacketBytes.Add(PacketBytesChecksum);
                                    }
                                    else if (Param3ComboBox.SelectedIndex == 1) // iteration, same message length supposed
                                    {
                                        RepeatIterate = true;

                                        PacketLengthHB = (byte)((4 + SCIBusPCMMessageToSendStart.Length + SCIBusPCMMessageToSendEnd.Length) >> 8);
                                        PacketLengthLB = (byte)((4 + SCIBusPCMMessageToSendStart.Length + SCIBusPCMMessageToSendEnd.Length) & 0xFF);

                                        PacketBytes.AddRange(new byte[] { 0x3D, PacketLengthHB, PacketLengthLB, 0x26, 0x03, 0x01, (byte)SCIBusPCMMessageToSendStart.Length });
                                        PacketBytes.AddRange(SCIBusPCMMessageToSendStart);
                                        PacketBytes.AddRange(SCIBusPCMMessageToSendEnd);

                                        for (int i = 1; i < PacketBytes.Count; i++)
                                        {
                                            PacketBytesChecksum += PacketBytes[i];
                                        }

                                        PacketBytes.Add(PacketBytesChecksum);
                                    }

                                    USBSendComboBoxValue = PacketBytes.ToArray();
                                    break;
                                default:
                                    break;
                            }
                            break;
                        default:
                            break;
                    }
                    break;
                case 3: // SCI-bus (TCM)
                    switch (CommandComboBox.SelectedIndex)
                    {
                        case 0: // Send message
                            switch (ModeComboBox.SelectedIndex)
                            {
                                case 0: // Stop message transmission
                                    USBSendComboBoxValue = new byte[] { 0x3D, 0x00, 0x02, 0x36, 0x01, 0x39 };
                                    RepeatIterate = false;
                                    break;
                                case 1: // Single message
                                    RepeatIterate = false;

                                    PacketLengthHB = (byte)((2 + SCIBusTCMMessageToSendStart.Length) >> 8);
                                    PacketLengthLB = (byte)((2 + SCIBusTCMMessageToSendStart.Length) & 0xFF);

                                    PacketBytes.AddRange(new byte[] { 0x3D, PacketLengthHB, PacketLengthLB, 0x36, 0x02 });
                                    PacketBytes.AddRange(SCIBusTCMMessageToSendStart);

                                    for (int i = 1; i < PacketBytes.Count; i++)
                                    {
                                        PacketBytesChecksum += PacketBytes[i];
                                    }

                                    PacketBytes.Add(PacketBytesChecksum);
                                    USBSendComboBoxValue = PacketBytes.ToArray();
                                    break;
                                case 2: // Repeated message
                                    if (Param3ComboBox.SelectedIndex == 0) // no iteration
                                    {
                                        RepeatIterate = false;

                                        PacketLengthHB = (byte)((4 + SCIBusTCMMessageToSendStart.Length) >> 8);
                                        PacketLengthLB = (byte)((4 + SCIBusTCMMessageToSendStart.Length) & 0xFF);

                                        PacketBytes.AddRange(new byte[] { 0x3D, PacketLengthHB, PacketLengthLB, 0x36, 0x03, 0x00, (byte)SCIBusTCMMessageToSendStart.Length });
                                        PacketBytes.AddRange(SCIBusTCMMessageToSendStart);

                                        for (int i = 1; i < PacketBytes.Count; i++)
                                        {
                                            PacketBytesChecksum += PacketBytes[i];
                                        }

                                        PacketBytes.Add(PacketBytesChecksum);
                                    }
                                    else if (Param3ComboBox.SelectedIndex == 1) // iteration, same message length supposed
                                    {
                                        RepeatIterate = true;

                                        PacketLengthHB = (byte)((4 + SCIBusTCMMessageToSendStart.Length + SCIBusTCMMessageToSendEnd.Length) >> 8);
                                        PacketLengthLB = (byte)((4 + SCIBusTCMMessageToSendStart.Length + SCIBusTCMMessageToSendEnd.Length) & 0xFF);

                                        PacketBytes.AddRange(new byte[] { 0x3D, PacketLengthHB, PacketLengthLB, 0x36, 0x03, 0x01, (byte)SCIBusTCMMessageToSendStart.Length });
                                        PacketBytes.AddRange(SCIBusTCMMessageToSendStart);
                                        PacketBytes.AddRange(SCIBusTCMMessageToSendEnd);

                                        for (int i = 1; i < PacketBytes.Count; i++)
                                        {
                                            PacketBytesChecksum += PacketBytes[i];
                                        }

                                        PacketBytes.Add(PacketBytesChecksum);
                                    }

                                    USBSendComboBoxValue = PacketBytes.ToArray();
                                    break;
                                default:
                                    break;
                            }
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    USBSendComboBoxValue = new byte[] { 0x3D, 0x00, 0x02, 0x02, 0x00, 0x04 };
                    break;
            }

            USBSendComboBox.Text = Util.ByteToHexStringSimple(USBSendComboBoxValue);
        }

        private void TargetComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (TargetComboBox.SelectedIndex)
            {
                case 0: // Scanner
                    CommandComboBox.Items.Clear();
                    CommandComboBox.Items.AddRange(new string[] { "Reset", "Handshake", "Status", "Settings", "Request", "Debug" });
                    ModeComboBox.Items.Clear();
                    ModeComboBox.Items.AddRange(new string[] { "None" });
                    CommandComboBox.SelectedIndex = 2; // Command: Status
                    ModeComboBox.SelectedIndex = 0; // Mode: None
                    Param1Label1.Visible = false; // Hide all parameter labels and comboboxes
                    Param1Label2.Visible = false;
                    Param1ComboBox.Visible = false;
                    Param2Label1.Visible = false;
                    Param2Label2.Visible = false;
                    Param2ComboBox.Visible = false;
                    Param3Label1.Visible = false;
                    Param3Label2.Visible = false;
                    Param3ComboBox.Visible = false;
                    LastTargetIndex = 0;
                    break;
                case 1: // CCD-bus
                    CommandComboBox.Items.Clear();
                    CommandComboBox.Items.AddRange(new string[] { "Send message" });
                    ModeComboBox.Items.Clear();
                    ModeComboBox.Items.AddRange(new string[] { "Stop message transmission", "Single message", "Repeated message(s)" });
                    CommandComboBox.SelectedIndex = 0; // Send message
                    ModeComboBox.SelectedIndex = 1; // Single message
                    Param1Label1.Visible = true;
                    Param1Label1.Text = "Message:";
                    Param1Label2.Visible = true;
                    Param1Label2.Text = String.Empty; // Rename second label
                    Param1ComboBox.Visible = true;
                    Param1ComboBox.Font = new Font("Courier New", 9F);
                    Param1ComboBox.DropDownStyle = ComboBoxStyle.DropDown;
                    Param1ComboBox.Items.Clear();
                    Param1ComboBox.Text = Util.ByteToHexStringSimple(CCDBusMessageToSendStart); // Load last valid message
                    Param2Label1.Visible = false;
                    Param2Label2.Visible = false;
                    Param2ComboBox.Visible = false;
                    Param2ComboBox.Text = Util.ByteToHexStringSimple(CCDBusMessageToSendEnd); // Load last valid message
                    Param3Label1.Visible = false;
                    Param3Label2.Visible = false;
                    Param3ComboBox.Visible = false;
                    LastTargetIndex = 1;
                    break;
                case 2: // SCI-bus (PCM)
                    CommandComboBox.Items.Clear();
                    CommandComboBox.Items.AddRange(new string[] { "Send message" });
                    ModeComboBox.Items.Clear();
                    ModeComboBox.Items.AddRange(new string[] { "Stop message transmission", "Single message", "Repeated message(s)" });
                    CommandComboBox.SelectedIndex = 0; // Send message
                    ModeComboBox.SelectedIndex = 1; // Single message
                    Param1Label1.Visible = true;
                    Param1Label1.Text = "Message:";
                    Param1Label2.Visible = true;
                    Param1Label2.Text = String.Empty; // Rename second label
                    Param1ComboBox.Visible = true;
                    Param1ComboBox.Font = new Font("Courier New", 9F);
                    Param1ComboBox.DropDownStyle = ComboBoxStyle.DropDown;
                    Param1ComboBox.Items.Clear();
                    Param1ComboBox.Text = Util.ByteToHexStringSimple(SCIBusPCMMessageToSendStart); // Load last valid message
                    Param2Label1.Visible = false;
                    Param2Label2.Visible = false;
                    Param2ComboBox.Visible = false;
                    Param2ComboBox.Text = Util.ByteToHexStringSimple(SCIBusPCMMessageToSendEnd); // Load last valid message
                    Param3Label1.Visible = false;
                    Param3Label2.Visible = false;
                    Param3ComboBox.Visible = false;
                    LastTargetIndex = 2;
                    break;
                case 3: // SCI-bus (TCM)
                    CommandComboBox.Items.Clear();
                    CommandComboBox.Items.AddRange(new string[] { "Send message" });
                    ModeComboBox.Items.Clear();
                    ModeComboBox.Items.AddRange(new string[] { "Stop message transmission", "Single message", "Repeated message(s)" });
                    CommandComboBox.SelectedIndex = 0; // Send message
                    ModeComboBox.SelectedIndex = 1; // Single message
                    Param1Label1.Visible = true;
                    Param1Label1.Text = "Message:";
                    Param1Label2.Visible = true;
                    Param1Label2.Text = String.Empty; // Rename second label
                    Param1ComboBox.Visible = true;
                    Param1ComboBox.Font = new Font("Courier New", 9F);
                    Param1ComboBox.DropDownStyle = ComboBoxStyle.DropDown;
                    Param1ComboBox.Items.Clear();
                    Param1ComboBox.Text = Util.ByteToHexStringSimple(SCIBusTCMMessageToSendStart); // Load last valid message
                    Param2Label1.Visible = false;
                    Param2Label2.Visible = false;
                    Param2ComboBox.Visible = false;
                    Param2ComboBox.Text = Util.ByteToHexStringSimple(SCIBusTCMMessageToSendEnd); // Load last valid message
                    Param3Label1.Visible = false;
                    Param3Label2.Visible = false;
                    Param3ComboBox.Visible = false;
                    LastTargetIndex = 3;
                    break;
                default:
                    break;
            }

            UpdateUSBSendComboBox();
        }

        private void CommandComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (TargetComboBox.SelectedIndex)
            {
                case 0: // Scanner
                    switch (CommandComboBox.SelectedIndex)
                    {
                        case 0: // Reset
                            ModeComboBox.Items.Clear();
                            ModeComboBox.Items.AddRange(new string[] { "None" });
                            ModeComboBox.SelectedIndex = 0; // None
                            Param1Label1.Visible = false;
                            Param1Label2.Visible = false;
                            Param1ComboBox.Visible = false;
                            Param2Label1.Visible = false;
                            Param2Label2.Visible = false;
                            Param2ComboBox.Visible = false;
                            Param3Label1.Visible = false;
                            Param3Label2.Visible = false;
                            Param3ComboBox.Visible = false;
                            break;
                        case 1: // Handshake
                            ModeComboBox.Items.Clear();
                            ModeComboBox.Items.AddRange(new string[] { "None", "Include hardware/firmware information" });
                            ModeComboBox.SelectedIndex = 0; // None
                            Param1Label1.Visible = false;
                            Param1Label2.Visible = false;
                            Param1ComboBox.Visible = false;
                            Param2Label1.Visible = false;
                            Param2Label2.Visible = false;
                            Param2ComboBox.Visible = false;
                            Param3Label1.Visible = false;
                            Param3Label2.Visible = false;
                            Param3ComboBox.Visible = false;
                            break;
                        case 2: // Status
                            ModeComboBox.Items.Clear();
                            ModeComboBox.Items.AddRange(new string[] { "None" });
                            ModeComboBox.SelectedIndex = 0; // None
                            Param1Label1.Visible = false;
                            Param1Label2.Visible = false;
                            Param1ComboBox.Visible = false;
                            Param2Label1.Visible = false;
                            Param2Label2.Visible = false;
                            Param2ComboBox.Visible = false;
                            Param3Label1.Visible = false;
                            Param3Label2.Visible = false;
                            Param3ComboBox.Visible = false;
                            break;
                        case 3: // Settings
                            ModeComboBox.Items.Clear();
                            ModeComboBox.Items.AddRange(new string[] { "Heartbeat", "Set CCD-bus", "Set SCI-bus", "Repeated message behavior" });
                            ModeComboBox.SelectedIndex = 0; // Heartbeat
                            Param1Label1.Visible = true;
                            Param1Label2.Visible = true;
                            Param1ComboBox.Visible = true;
                            Param1ComboBox.Font = new Font("Courier New", 9F);
                            Param1ComboBox.DropDownStyle = ComboBoxStyle.DropDown;
                            Param1Label1.Text = "Interval:";
                            Param1ComboBox.Text = HeartbeatInterval.ToString();
                            Param1Label2.Text = "millisecond";
                            Param2Label1.Visible = true;
                            Param2Label2.Visible = true;
                            Param2ComboBox.Visible = true;
                            Param2ComboBox.Font = new Font("Courier New", 9F);
                            Param2ComboBox.DropDownStyle = ComboBoxStyle.DropDown;
                            Param2Label1.Text = "Duration:";
                            Param2ComboBox.Text = HeartbeatDuration.ToString();
                            Param2Label2.Text = "millisecond";
                            Param3Label1.Visible = false;
                            Param3Label2.Visible = false;
                            Param3ComboBox.Visible = false;
                            break;
                        case 4: // Request
                            ModeComboBox.Items.Clear();
                            ModeComboBox.Items.AddRange(new string[] { "Hardware/Firmware information", "Timestamp", "Battery voltage", "External EEPROM checksum" });
                            ModeComboBox.SelectedIndex = 0; // Firmware date
                            Param1Label1.Visible = false;
                            Param1Label2.Visible = false;
                            Param1ComboBox.Visible = false;
                            Param2Label1.Visible = false;
                            Param2Label2.Visible = false;
                            Param2ComboBox.Visible = false;
                            Param3Label1.Visible = false;
                            Param3Label2.Visible = false;
                            Param3ComboBox.Visible = false;
                            break;
                        case 5: // Debug
                            ModeComboBox.Items.Clear();
                            ModeComboBox.Items.AddRange(new string[] { "Generate random CCD-bus messages", "Read external EEPROM", "Write external EEPROM", "Set arbitrary UART-speed" });
                            ModeComboBox.SelectedIndex = 0; // Generate random CCD-bus messages
                            Param1Label1.Visible = true;
                            Param1Label2.Visible = true;
                            Param1ComboBox.Visible = true;
                            Param1ComboBox.Text = RandomCCDMessageIntervalMin.ToString();
                            Param2Label1.Visible = true;
                            Param2Label2.Visible = true;
                            Param2ComboBox.Visible = true;
                            Param2ComboBox.Text = RandomCCDMessageIntervalMax.ToString();
                            Param3Label1.Visible = true;
                            Param3Label2.Visible = true;
                            Param3ComboBox.Visible = true;

                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }

            UpdateUSBSendComboBox();
        }

        private void ModeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (TargetComboBox.SelectedIndex)
            {
                case 0: // Scanner
                    switch (CommandComboBox.SelectedIndex)
                    {
                        case 0: // Reset
                            Param1Label1.Visible = false;
                            Param1Label2.Visible = false;
                            Param1ComboBox.Visible = false;
                            Param2Label1.Visible = false;
                            Param2Label2.Visible = false;
                            Param2ComboBox.Visible = false;
                            Param3Label1.Visible = false;
                            Param3Label2.Visible = false;
                            Param3ComboBox.Visible = false;
                            HintTextBox.Text = Environment.NewLine 
                                             + "This command makes the scanner reset itself." + Environment.NewLine 
                                             + "Variables will return to their default values" + Environment.NewLine 
                                             + "unless they are saved to internal/external EEPROM.";
                            break;
                        case 1: // Handshake
                            Param1Label1.Visible = false;
                            Param1Label2.Visible = false;
                            Param1ComboBox.Visible = false;
                            Param2Label1.Visible = false;
                            Param2Label2.Visible = false;
                            Param2ComboBox.Visible = false;
                            Param3Label1.Visible = false;
                            Param3Label2.Visible = false;
                            Param3ComboBox.Visible = false;
                            HintTextBox.Text = Environment.NewLine
                                             + "Handshake request is a good way to identify this scanner" + Environment.NewLine
                                             + "among other devices. The scanner will always respond" + Environment.NewLine 
                                             + "with CHRYSLERCCDSCISCANNER to this request.";
                            break;
                        case 2: // Status
                            Param1Label1.Visible = false;
                            Param1Label2.Visible = false;
                            Param1ComboBox.Visible = false;
                            Param2Label1.Visible = false;
                            Param2Label2.Visible = false;
                            Param2ComboBox.Visible = false;
                            Param3Label1.Visible = false;
                            Param3Label2.Visible = false;
                            Param3ComboBox.Visible = false;
                            HintTextBox.Text = Environment.NewLine 
                                             + Environment.NewLine 
                                             + Environment.NewLine 
                                             + "Request status message from the scanner.";
                            break;
                        case 3: // Settings
                            switch (ModeComboBox.SelectedIndex)
                            {
                                case 0: // Hearbeat
                                    Param1Label1.Visible = true;
                                    Param1Label1.Text = "Interval:";
                                    Param1Label2.Visible = true;
                                    Param1Label2.Text = "millisecond";
                                    Param1ComboBox.Visible = true;
                                    Param1ComboBox.Enabled = true;
                                    Param1ComboBox.Font = new Font("Courier New", 9F);
                                    Param1ComboBox.DropDownStyle = ComboBoxStyle.DropDown;
                                    Param1ComboBox.Items.Clear();
                                    Param1ComboBox.Text = HeartbeatInterval.ToString();
                                    Param2Label1.Visible = true;
                                    Param2Label1.Text = "Duration:";
                                    Param2Label2.Visible = true;
                                    Param2Label2.Text = "millisecond";
                                    Param2ComboBox.Visible = true;
                                    Param2ComboBox.Enabled = true;
                                    Param2ComboBox.Font = new Font("Courier New", 9F);
                                    Param2ComboBox.DropDownStyle = ComboBoxStyle.DropDown;
                                    Param2ComboBox.Items.Clear();
                                    Param2ComboBox.Text = HeartbeatDuration.ToString();
                                    Param3Label1.Visible = false;
                                    Param3Label2.Visible = false;
                                    Param3ComboBox.Visible = false;
                                    HintTextBox.Text = Environment.NewLine 
                                                     + "Set LED behavior in the scanner:" + Environment.NewLine 
                                                     + "- interval: blinking period of the blue ACT LED only," + Environment.NewLine 
                                                     + "- duration: maximum on-time of any LEDs (default = 50 ms).";
                                    break;
                                case 1: // Set CCD-bus
                                    Param1Label1.Visible = true;
                                    Param1Label1.Text = "State:";
                                    Param1Label2.Visible = true;
                                    Param1Label2.Text = "";
                                    Param1ComboBox.Visible = true;
                                    Param1ComboBox.Enabled = true;
                                    Param1ComboBox.Font = new Font("Microsoft Sans Serif", 8.25F);
                                    Param1ComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                                    Param1ComboBox.Items.Clear();
                                    Param1ComboBox.Items.AddRange(new string[] { "Off", "On" });
                                    Param1ComboBox.SelectedIndex = 0;
                                    Param2Label1.Visible = false;
                                    Param2Label2.Visible = false;
                                    Param2ComboBox.Visible = false;
                                    Param3Label1.Visible = false;
                                    Param3Label2.Visible = false;
                                    Param3ComboBox.Visible = false;
                                    HintTextBox.Text = Environment.NewLine
                                                     + Environment.NewLine
                                                     + Environment.NewLine
                                                     + "Turn on/off the CCD-bus transceiver chip.";
                                    break;
                                case 2: // Set SCI-bus
                                    Param1Label1.Visible = true;
                                    Param1Label1.Text = "Module:";
                                    Param1Label2.Visible = true;
                                    Param1Label2.Text = "";
                                    Param1ComboBox.Visible = true;
                                    Param1ComboBox.Enabled = true;
                                    Param1ComboBox.Font = new Font("Microsoft Sans Serif", 8.25F);
                                    Param1ComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                                    Param1ComboBox.Items.Clear();
                                    Param1ComboBox.Items.AddRange(new string[] { "Engine (PCM)", "Transmission (TCM)" });
                                    Param1ComboBox.SelectedIndex = 0;
                                    Param2Label1.Visible = true;
                                    Param2Label1.Text = "Speed:";
                                    Param2Label2.Visible = false;
                                    Param2ComboBox.Visible = true;
                                    Param2ComboBox.Enabled = true;
                                    Param2ComboBox.Font = new Font("Microsoft Sans Serif", 8.25F);
                                    Param2ComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                                    Param2ComboBox.Items.Clear();
                                    Param2ComboBox.Items.AddRange(new string[] { "Disable", "Low (7812.5 baud)", "High (62500 baud)" });
                                    Param2ComboBox.SelectedIndex = 1;
                                    Param3Label1.Visible = true;
                                    Param3Label1.Text = "Config.:";
                                    Param3Label2.Visible = false;
                                    Param3ComboBox.Visible = true;
                                    Param3ComboBox.Enabled = true;
                                    Param3ComboBox.Font = new Font("Microsoft Sans Serif", 8.25F);
                                    Param3ComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                                    Param3ComboBox.Items.Clear();
                                    Param3ComboBox.Items.AddRange(new string[] { "A", "B" });
                                    Param3ComboBox.SelectedIndex = 0;
                                    HintTextBox.Text = "Set SCI-bus transceiver circuits:" + Environment.NewLine
                                                     + "- module: engine (PCM) or transmission (TCM)," + Environment.NewLine
                                                     + "- speed: communication speed," + Environment.NewLine
                                                     + "- config: OBD-II pin routing specific to the vehicle.";
                                    break;
                                case 3: // Repeated message behavior
                                    Param1Label1.Visible = true;
                                    Param1Label1.Text = "Bus:";
                                    Param1Label2.Visible = false;
                                    Param1ComboBox.Visible = true;
                                    Param1ComboBox.Enabled = true;
                                    Param1ComboBox.Font = new Font("Microsoft Sans Serif", 8.25F);
                                    Param1ComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                                    Param1ComboBox.Items.Clear();
                                    Param1ComboBox.Items.AddRange(new string[] { "CCD-bus", "SCI-bus (PCM)", "SCI-bus (TCM)" });
                                    Param1ComboBox.SelectedIndex = 0;
                                    Param2Label1.Visible = true;
                                    Param2Label1.Text = "Interval:";
                                    Param2Label2.Visible = true;
                                    Param2Label2.Text = "milliseconds";
                                    Param2ComboBox.Visible = true;
                                    Param2ComboBox.Enabled = true;
                                    Param2ComboBox.Font = new Font("Courier New", 9F);
                                    Param2ComboBox.DropDownStyle = ComboBoxStyle.DropDown;
                                    Param2ComboBox.Items.Clear();
                                    Param2ComboBox.Text = RepeatInterval.ToString();
                                    Param3Label1.Visible = true;
                                    Param3Label1.Text = "Incr.:";
                                    Param3Label2.Visible = false;
                                    Param3ComboBox.Visible = true;
                                    Param3ComboBox.Enabled = true;
                                    Param3ComboBox.Font = new Font("Courier New", 9F);
                                    Param3ComboBox.DropDownStyle = ComboBoxStyle.DropDown;
                                    Param3ComboBox.Items.Clear();
                                    Param3ComboBox.Text = RepeatIncrement.ToString();
                                    HintTextBox.Text = "Set repeated message behavior for CCD/SCI-bus:" + Environment.NewLine
                                                     + "- interval: elapsed time between messages," + Environment.NewLine
                                                     + "- increment: the byte difference between each message," + Environment.NewLine
                                                     + "when iteration is enabled (default = 1).";
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case 4: // Request
                            switch (ModeComboBox.SelectedIndex)
                            {
                                case 0: // Hardware/Firmware information
                                    Param1Label1.Visible = false;
                                    Param1Label2.Visible = false;
                                    Param1ComboBox.Visible = false;
                                    Param2Label1.Visible = false;
                                    Param2Label2.Visible = false;
                                    Param2ComboBox.Visible = false;
                                    Param3Label1.Visible = false;
                                    Param3Label2.Visible = false;
                                    Param3ComboBox.Visible = false;
                                    HintTextBox.Text = Environment.NewLine
                                                     + Environment.NewLine
                                                     + "Request haradware version/date, assembly date," + Environment.NewLine
                                                     + "firmware date and external EEPROM checksum byte.";
                                    break;
                                case 1: // Timestamp
                                    Param1Label1.Visible = false;
                                    Param1Label2.Visible = false;
                                    Param1ComboBox.Visible = false;
                                    Param2Label1.Visible = false;
                                    Param2Label2.Visible = false;
                                    Param2ComboBox.Visible = false;
                                    Param3Label1.Visible = false;
                                    Param3Label2.Visible = false;
                                    Param3ComboBox.Visible = false;
                                    HintTextBox.Text = Environment.NewLine
                                                     + Environment.NewLine
                                                     + "Request timestamp that is the time elapsed since" + Environment.NewLine
                                                     + "the scanner has power or the last reset occured.";
                                    break;
                                case 2: // Battery voltage
                                    Param1Label1.Visible = false;
                                    Param1Label2.Visible = false;
                                    Param1ComboBox.Visible = false;
                                    Param2Label1.Visible = false;
                                    Param2Label2.Visible = false;
                                    Param2ComboBox.Visible = false;
                                    Param3Label1.Visible = false;
                                    Param3Label2.Visible = false;
                                    Param3ComboBox.Visible = false;
                                    HintTextBox.Text = Environment.NewLine
                                                     + Environment.NewLine
                                                     + "Request battery voltage calculated by the scanner" + Environment.NewLine
                                                     + "using a resistor divider circuit.";
                                    break;
                                case 3: // External EEPROM checksum
                                    Param1Label1.Visible = false;
                                    Param1Label2.Visible = false;
                                    Param1ComboBox.Visible = false;
                                    Param2Label1.Visible = false;
                                    Param2Label2.Visible = false;
                                    Param2ComboBox.Visible = false;
                                    Param3Label1.Visible = false;
                                    Param3Label2.Visible = false;
                                    Param3ComboBox.Visible = false;
                                    HintTextBox.Text = Environment.NewLine
                                                     + "First 256 bytes of the external EEPROM is reserved" + Environment.NewLine
                                                     + "for hardware informations with the last byte being" + Environment.NewLine
                                                     + "the checksum byte. Read it and calculate if it is correct.";
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case 5: // Debug
                            switch (ModeComboBox.SelectedIndex)
                            {
                                case 0: // Generate random CCD-bus messages
                                    Param1Label1.Visible = true;
                                    Param1Label1.Text = "Min:";
                                    Param1Label2.Visible = true;
                                    Param1Label2.Text = "millisecond";
                                    Param1ComboBox.Visible = true;
                                    Param1ComboBox.Enabled = true;
                                    Param1ComboBox.Font = new Font("Courier New", 9F);
                                    Param1ComboBox.DropDownStyle = ComboBoxStyle.DropDown;
                                    Param1ComboBox.Items.Clear();
                                    Param1ComboBox.Text = RandomCCDMessageIntervalMin.ToString();
                                    Param2Label1.Visible = true;
                                    Param2Label1.Text = "Max:";
                                    Param2Label2.Visible = true;
                                    Param2Label2.Text = "millisecond";
                                    Param2ComboBox.Visible = true;
                                    Param2ComboBox.Enabled = true;
                                    Param2ComboBox.Font = new Font("Courier New", 9F);
                                    Param2ComboBox.DropDownStyle = ComboBoxStyle.DropDown;
                                    Param2ComboBox.Items.Clear();
                                    Param2ComboBox.Text = RandomCCDMessageIntervalMax.ToString();
                                    Param3Label1.Visible = true;
                                    Param3Label1.Text = "Enable:";
                                    Param3Label2.Visible = false;
                                    Param3ComboBox.Visible = true;
                                    Param3ComboBox.Enabled = true;
                                    Param3ComboBox.Font = new Font("Microsoft Sans Serif", 8.25F);
                                    Param3ComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                                    Param3ComboBox.Items.Clear();
                                    Param3ComboBox.Items.AddRange(new string[] { "false", "true" });
                                    Param3ComboBox.SelectedIndex = 1;
                                    HintTextBox.Text = "Broadcast random CCD-bus messages" + Environment.NewLine
                                                     + "at random times defined by min / max intervals." + Environment.NewLine
                                                     + "Do this only when the scanner is not connected" + Environment.NewLine
                                                     + "to the car and connect both standalone bias jumpers!";
                                    break;
                                case 1: // Read external EEPROM
                                    Param1Label1.Visible = true;
                                    Param1Label1.Text = "Address:";
                                    Param1Label2.Visible = false;
                                    Param1ComboBox.Visible = true;
                                    Param1ComboBox.Enabled = true;
                                    Param1ComboBox.Font = new Font("Courier New", 9F);
                                    Param1ComboBox.DropDownStyle = ComboBoxStyle.DropDown;
                                    Param1ComboBox.Items.Clear();
                                    Param1ComboBox.Text = Util.ByteToHexString(extEEPROMaddress, 0, extEEPROMaddress.Length);
                                    Param2Label1.Visible = false;
                                    Param2Label2.Visible = false;
                                    Param2ComboBox.Visible = false;
                                    Param3Label1.Visible = false;
                                    Param3Label2.Visible = false;
                                    Param3ComboBox.Visible = false;
                                    HintTextBox.Text = Environment.NewLine
                                                     + Environment.NewLine
                                                     + Environment.NewLine
                                                     + "Read 1 byte from the external EEPROM chip.";
                                    break;
                                case 2: // Write external EEPROM
                                    Param1Label1.Visible = true;
                                    Param1Label1.Text = "Address:";
                                    Param1Label2.Visible = false;
                                    Param1ComboBox.Visible = true;
                                    Param1ComboBox.Enabled = true;
                                    Param1ComboBox.Font = new Font("Courier New", 9F);
                                    Param1ComboBox.DropDownStyle = ComboBoxStyle.DropDown;
                                    Param1ComboBox.Items.Clear();
                                    Param1ComboBox.Text = Util.ByteToHexString(extEEPROMaddress, 0, extEEPROMaddress.Length);
                                    Param2Label1.Visible = true;
                                    Param2Label1.Text = "Value:";
                                    Param2Label2.Visible = false;
                                    Param2ComboBox.Visible = true;
                                    Param2ComboBox.Enabled = true;
                                    Param2ComboBox.Font = new Font("Courier New", 9F);
                                    Param2ComboBox.DropDownStyle = ComboBoxStyle.DropDown;
                                    Param2ComboBox.Items.Clear();
                                    Param2ComboBox.Text = Util.ByteToHexString(extEEPROMvalue, 0, extEEPROMvalue.Length);
                                    Param3Label1.Visible = false;
                                    Param3Label2.Visible = false;
                                    Param3ComboBox.Visible = false;
                                    HintTextBox.Text = Environment.NewLine
                                                     + Environment.NewLine
                                                     + Environment.NewLine
                                                     + "Write 1 byte to the external EEPROM chip.";
                                    break;
                                case 3: // Set arbitrary UART-speed
                                    Param1Label1.Visible = true;
                                    Param1Label1.Text = "Bus:";
                                    Param1Label2.Visible = false;
                                    Param1ComboBox.Visible = true;
                                    Param1ComboBox.Enabled = true;
                                    Param1ComboBox.Font = new Font("Microsoft Sans Serif", 8.25F);
                                    Param1ComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                                    Param1ComboBox.Items.Clear();
                                    Param1ComboBox.Items.AddRange(new string[] { "CCD-bus", "SCI-bus (PCM)", "SCI-bus (TCM)" });
                                    Param1ComboBox.SelectedIndex = 0;
                                    Param2Label1.Visible = true;
                                    Param2Label1.Text = "Speed:";
                                    Param2Label2.Visible = false;
                                    Param2ComboBox.Visible = true;
                                    Param2ComboBox.Enabled = true;
                                    Param2ComboBox.Font = new Font("Microsoft Sans Serif", 8.25F);
                                    Param2ComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                                    Param2ComboBox.Items.Clear();
                                    Param2ComboBox.Items.AddRange(new string[] { "976.5 baud", "7812.5 baud", "62500 baud", "125000 baud" });
                                    Param2ComboBox.SelectedIndex = 0;
                                    Param3Label1.Visible = false;
                                    Param3Label2.Visible = false;
                                    Param3ComboBox.Visible = false;
                                    HintTextBox.Text = Environment.NewLine
                                                     + Environment.NewLine
                                                     + Environment.NewLine
                                                     + "Set arbitrary UART-speed.";
                                    break;
                            }
                            break;
                        default:
                            break;
                    }
                    break;
                case 1: // CCD-bus
                    switch (CommandComboBox.SelectedIndex)
                    {
                        case 0: // Send message
                            switch (ModeComboBox.SelectedIndex)
                            {
                                case 0: // Stop message transmission
                                    Param1Label1.Visible = false;
                                    Param1Label2.Visible = false;
                                    Param1ComboBox.Visible = false;
                                    Param2Label1.Visible = false;
                                    Param2Label2.Visible = false;
                                    Param2ComboBox.Visible = false;
                                    Param3Label1.Visible = false;
                                    Param3Label2.Visible = false;
                                    Param3ComboBox.Visible = false;
                                    HintTextBox.Text = Environment.NewLine
                                                     + Environment.NewLine
                                                     + Environment.NewLine
                                                     + "Stop repeated message transmission.";
                                    break;
                                case 1: // Single message
                                    Param1Label1.Visible = true;
                                    Param1Label1.Text = "Message:";
                                    Param1Label2.Visible = false;
                                    Param1ComboBox.Visible = true;
                                    Param1ComboBox.Enabled = true;
                                    Param1ComboBox.Font = new Font("Courier New", 9F);
                                    Param1ComboBox.DropDownStyle = ComboBoxStyle.DropDown;
                                    Param1ComboBox.Items.Clear();
                                    Param2Label1.Visible = false;
                                    Param2Label2.Visible = false;
                                    Param2ComboBox.Visible = false;
                                    Param3Label1.Visible = false;
                                    Param3Label2.Visible = false;
                                    Param3ComboBox.Visible = false;
                                    HintTextBox.Text = Environment.NewLine
                                                     + Environment.NewLine
                                                     + Environment.NewLine
                                                     + "Send a single message to the CCD-bus.";
                                    break;
                                case 2: // Repeated message(s)
                                    Param1Label1.Visible = true;
                                    Param1Label1.Text = "Start:";
                                    Param1Label2.Visible = true;
                                    Param1Label2.Text = "message";
                                    Param1ComboBox.Visible = true;
                                    Param1ComboBox.Enabled = true;
                                    Param1ComboBox.Font = new Font("Courier New", 9F);
                                    Param1ComboBox.DropDownStyle = ComboBoxStyle.DropDown;
                                    Param1ComboBox.Items.Clear();
                                    Param2Label1.Visible = true;
                                    Param2Label1.Text = "End:";
                                    Param2Label2.Visible = true;
                                    Param2Label2.Text = "message";
                                    Param2ComboBox.Visible = true;
                                    Param2ComboBox.Enabled = false;
                                    Param2ComboBox.Font = new Font("Courier New", 9F);
                                    Param2ComboBox.DropDownStyle = ComboBoxStyle.DropDown;
                                    Param2ComboBox.Items.Clear();
                                    Param3Label1.Visible = true;
                                    Param3Label1.Text = "Iterate:";
                                    Param3Label2.Visible = false;
                                    Param3Label2.Text = "";
                                    Param3ComboBox.Visible = true;
                                    Param3ComboBox.Enabled = true;
                                    Param3ComboBox.Font = new Font("Microsoft Sans Serif", 8.25F);
                                    Param3ComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                                    Param3ComboBox.Items.Clear();
                                    Param3ComboBox.Items.AddRange(new string[] { "false", "true" });
                                    Param3ComboBox.SelectedIndex = 0;
                                    HintTextBox.Text = Environment.NewLine
                                                     + "Send a message repeatedly to the CCD-bus." + Environment.NewLine
                                                     + "Iteration increment and repeating interval is adjustable" + Environment.NewLine
                                                     + "in settings (default increment = 1, interval = 100 ms).";
                                    break;
                                default:
                                    break;
                            }
                            break;
                        default:
                            break;
                    }
                    break;
                case 2: // SCI-bus (PCM)
                    switch (CommandComboBox.SelectedIndex)
                    {
                        case 0: // Send message
                            switch (ModeComboBox.SelectedIndex)
                            {
                                case 0: // Stop message transmission
                                    Param1Label1.Visible = false;
                                    Param1Label2.Visible = false;
                                    Param1ComboBox.Visible = false;
                                    Param2Label1.Visible = false;
                                    Param2Label2.Visible = false;
                                    Param2ComboBox.Visible = false;
                                    Param3Label1.Visible = false;
                                    Param3Label2.Visible = false;
                                    Param3ComboBox.Visible = false;
                                    HintTextBox.Text = Environment.NewLine
                                                     + Environment.NewLine
                                                     + Environment.NewLine
                                                     + "Stop repeated message transmission.";
                                    break;
                                case 1: // Single message
                                    Param1Label1.Visible = true;
                                    Param1Label1.Text = "Message:";
                                    Param1Label2.Visible = false;
                                    Param1ComboBox.Visible = true;
                                    Param1ComboBox.Enabled = true;
                                    Param1ComboBox.Font = new Font("Courier New", 9F);
                                    Param1ComboBox.DropDownStyle = ComboBoxStyle.DropDown;
                                    Param1ComboBox.Items.Clear();
                                    Param2Label1.Visible = false;
                                    Param2Label2.Visible = false;
                                    Param2ComboBox.Visible = false;
                                    Param3Label1.Visible = false;
                                    Param3Label2.Visible = false;
                                    Param3ComboBox.Visible = false;
                                    HintTextBox.Text = Environment.NewLine
                                                     + Environment.NewLine
                                                     + Environment.NewLine
                                                     + "Send a single message to the SCI-bus (PCM).";
                                    break;
                                case 2: // Repeated message(s)
                                    Param1Label1.Visible = true;
                                    Param1Label1.Text = "Start:";
                                    Param1Label2.Visible = true;
                                    Param1Label2.Text = "message";
                                    Param1ComboBox.Visible = true;
                                    Param1ComboBox.Enabled = true;
                                    Param1ComboBox.Font = new Font("Courier New", 9F);
                                    Param1ComboBox.DropDownStyle = ComboBoxStyle.DropDown;
                                    Param1ComboBox.Items.Clear();
                                    Param2Label1.Visible = true;
                                    Param2Label1.Text = "End:";
                                    Param2Label2.Visible = true;
                                    Param2Label2.Text = "message";
                                    Param2ComboBox.Visible = true;
                                    Param2ComboBox.Enabled = false;
                                    Param2ComboBox.Font = new Font("Courier New", 9F);
                                    Param2ComboBox.DropDownStyle = ComboBoxStyle.DropDown;
                                    Param2ComboBox.Items.Clear();
                                    Param3Label1.Visible = true;
                                    Param3Label1.Text = "Iterate:";
                                    Param3Label2.Visible = false;
                                    Param3Label2.Text = "";
                                    Param3ComboBox.Visible = true;
                                    Param3ComboBox.Enabled = true;
                                    Param3ComboBox.Font = new Font("Microsoft Sans Serif", 8.25F);
                                    Param3ComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                                    Param3ComboBox.Items.Clear();
                                    Param3ComboBox.Items.AddRange(new string[] { "false", "true" });
                                    Param3ComboBox.SelectedIndex = 0;
                                    HintTextBox.Text = Environment.NewLine
                                                     + Environment.NewLine
                                                     + "Send a message repeatedly to the SCI-bus (PCM)." + Environment.NewLine
                                                     + "Iteration increment is adjustable in settings (default = 1).";
                                    break;
                                default:
                                    break;
                            }
                            break;
                        default:
                            break;
                    }
                    break;
                case 3: // SCI-bus (TCM)
                    switch (CommandComboBox.SelectedIndex)
                    {
                        case 0: // Send message
                            switch (ModeComboBox.SelectedIndex)
                            {
                                case 0: // Stop message transmission
                                    Param1Label1.Visible = false;
                                    Param1Label2.Visible = false;
                                    Param1ComboBox.Visible = false;
                                    Param2Label1.Visible = false;
                                    Param2Label2.Visible = false;
                                    Param2ComboBox.Visible = false;
                                    Param3Label1.Visible = false;
                                    Param3Label2.Visible = false;
                                    Param3ComboBox.Visible = false;
                                    HintTextBox.Text = Environment.NewLine
                                                     + Environment.NewLine
                                                     + Environment.NewLine
                                                     + "Stop repeated message transmission.";
                                    break;
                                case 1: // Single message
                                    Param1Label1.Visible = true;
                                    Param1Label1.Text = "Message:";
                                    Param1Label2.Visible = false;
                                    Param1ComboBox.Visible = true;
                                    Param1ComboBox.Enabled = true;
                                    Param1ComboBox.Font = new Font("Courier New", 9F);
                                    Param1ComboBox.DropDownStyle = ComboBoxStyle.DropDown;
                                    Param1ComboBox.Items.Clear();
                                    Param2Label1.Visible = false;
                                    Param2Label2.Visible = false;
                                    Param2ComboBox.Visible = false;
                                    Param3Label1.Visible = false;
                                    Param3Label2.Visible = false;
                                    Param3ComboBox.Visible = false;
                                    HintTextBox.Text = Environment.NewLine
                                                     + Environment.NewLine
                                                     + Environment.NewLine
                                                     + "Send a single message to the SCI-bus (TCM).";
                                    break;
                                case 2: // Repeated message(s)
                                    Param1Label1.Visible = true;
                                    Param1Label1.Text = "Start:";
                                    Param1Label2.Visible = true;
                                    Param1Label2.Text = "message";
                                    Param1ComboBox.Visible = true;
                                    Param1ComboBox.Enabled = true;
                                    Param1ComboBox.Font = new Font("Courier New", 9F);
                                    Param1ComboBox.DropDownStyle = ComboBoxStyle.DropDown;
                                    Param1ComboBox.Items.Clear();
                                    Param2Label1.Visible = true;
                                    Param2Label1.Text = "End:";
                                    Param2Label2.Visible = true;
                                    Param2Label2.Text = "message";
                                    Param2ComboBox.Visible = true;
                                    Param2ComboBox.Enabled = true;
                                    Param2ComboBox.Font = new Font("Courier New", 9F);
                                    Param2ComboBox.DropDownStyle = ComboBoxStyle.DropDown;
                                    Param2ComboBox.Items.Clear();
                                    Param3Label1.Visible = true;
                                    Param3Label1.Text = "Iterate:";
                                    Param3Label2.Visible = false;
                                    Param3Label2.Text = "";
                                    Param3ComboBox.Visible = true;
                                    Param3ComboBox.Enabled = true;
                                    Param3ComboBox.Font = new Font("Microsoft Sans Serif", 8.25F);
                                    Param3ComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                                    Param3ComboBox.Items.Clear();
                                    Param3ComboBox.Items.AddRange(new string[] { "false", "true" });
                                    Param3ComboBox.SelectedIndex = 0;
                                    HintTextBox.Text = Environment.NewLine
                                                     + Environment.NewLine
                                                     + "Send a message repeatedly to the SCI-bus (TCM)." + Environment.NewLine
                                                     + "Iteration increment is adjustable in settings (default = 1).";
                                    break;
                                default:
                                    break;
                            }
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }

            UpdateUSBSendComboBox();
        }

        private void ParamsComboBox_TextChanged(object sender, EventArgs e)
        {
            switch (TargetComboBox.SelectedIndex)
            {
                case 0: // Scanner
                    switch (CommandComboBox.SelectedIndex)
                    {
                        case 3: // Settings
                            switch (ModeComboBox.SelectedIndex)
                            {
                                case 0: // Heartbeat
                                    try
                                    {
                                        HeartbeatInterval = int.Parse(Param1ComboBox.Text);
                                        HeartbeatDuration = int.Parse(Param2ComboBox.Text);
                                    }
                                    catch
                                    {
                                        
                                    }
                                    break;
                                case 1: // Set CCD-bus
                                    try
                                    {
                                        if (Param1ComboBox.SelectedIndex == 0) SetCCDBus = false;
                                        else if (Param1ComboBox.SelectedIndex == 1) SetCCDBus = true;
                                    }
                                    catch
                                    {

                                    }
                                    break;
                                case 2: // Set SCI-bus
                                    try
                                    {
                                        byte pcm_change = 0;
                                        byte pcm_state = 0;
                                        byte pcm_config = 0;
                                        byte pcm_speed = 0;
                                        byte tcm_change = 0;
                                        byte tcm_state = 0;
                                        byte tcm_config = 0;
                                        byte tcm_speed = 0;

                                        if (Param1ComboBox.SelectedIndex == 0) // PCM
                                        {
                                            pcm_change = 1;
                                            tcm_change = 0;

                                            if (Param2ComboBox.SelectedIndex == 0) // State: disabled
                                            {
                                                pcm_state = 0; // disabled
                                            }
                                            else if (Param2ComboBox.SelectedIndex == 1) // State: enabled & 7812.5 baud
                                            {
                                                pcm_state = 1; // enabled
                                                pcm_speed = 0; // 7812.5 baud
                                            }
                                            else if (Param2ComboBox.SelectedIndex == 2) // State: enabled & 62500 baud
                                            {
                                                pcm_state = 1; // enabled
                                                pcm_speed = 1; // 62500 baud
                                            }

                                            if (Param3ComboBox.SelectedIndex == 0) pcm_config = 0; // "A" configuration
                                            else if (Param3ComboBox.SelectedIndex == 1) pcm_config = 1; // "B" configuration
                                        }
                                        else
                                        {
                                            pcm_change = 0;
                                            tcm_change = 1;

                                            if (Param2ComboBox.SelectedIndex == 0) // State: disabled
                                            {
                                                tcm_state = 0; // disabled
                                            }
                                            else if (Param2ComboBox.SelectedIndex == 1) // State: enabled & 7812.5 baud
                                            {
                                                tcm_state = 1; // enabled
                                                tcm_speed = 0; // 7812.5 baud
                                            }
                                            else if (Param2ComboBox.SelectedIndex == 2) // State: enabled & 62500 baud
                                            {
                                                tcm_state = 1; // enabled
                                                tcm_speed = 1; // 62500 baud
                                            }

                                            if (Param3ComboBox.SelectedIndex == 0) tcm_config = 0; // "A" configuration
                                            else if (Param3ComboBox.SelectedIndex == 1) tcm_config = 1; // "B" configuration
                                        }

                                        if (pcm_change == 1)
                                        {
                                            SetSCIBus = (byte)((pcm_change << 7) | (pcm_state << 6) | (pcm_config << 5) | pcm_speed << 4);
                                            SetSCIBus &= 0xF0;
                                        }
                                        if (tcm_change == 1)
                                        {
                                            SetSCIBus = (byte)((tcm_change << 3) | (tcm_state << 2) | (tcm_config << 1) | tcm_speed);
                                            SetSCIBus &= 0x0F;
                                        }
                                    }
                                    catch
                                    {

                                    }
                                    break;
                                case 3: // Repeated message behavior
                                    try
                                    {
                                        RepeatInterval = int.Parse(Param2ComboBox.Text);
                                        RepeatIncrement = int.Parse(Param3ComboBox.Text);
                                    }
                                    catch
                                    {

                                    }
                                    break;
                                default:
                                    break;
                            }
                            
                            break;
                        case 5: // Debug
                            switch (ModeComboBox.SelectedIndex)
                            {
                                case 0: // Generate random CCD-bus messages
                                    try
                                    {
                                        RandomCCDMessageIntervalMin = int.Parse(Param1ComboBox.Text);
                                        RandomCCDMessageIntervalMax = int.Parse(Param2ComboBox.Text);
                                    }
                                    catch
                                    {

                                    }
                                    break;
                                case 1: // Read external EEPROM
                                    try
                                    {
                                        extEEPROMaddress = Util.HexStringToByte(Param1ComboBox.Text);
                                    }
                                    catch
                                    {

                                    }
                                    break;
                                case 2: // Write external EEPROM
                                    try
                                    {
                                        extEEPROMaddress = Util.HexStringToByte(Param1ComboBox.Text);
                                        extEEPROMvalue = Util.HexStringToByte(Param2ComboBox.Text);
                                    }
                                    catch
                                    {

                                    }
                                    break;
                                case 3: // Set arbitrary UART speed

                                    break;
                                default:
                                    break;
                            }
                            break;
                        default:
                            break;
                    }
                    break;
                case 1: // CCD-bus
                    switch (CommandComboBox.SelectedIndex)
                    {
                        case 0: // Send message
                            switch (ModeComboBox.SelectedIndex)
                            {
                                case 0: // Stop message transmission
                                    break;
                                case 1: // Single message
                                    CCDBusMessageToSendStart = Util.HexStringToByte(Param1ComboBox.Text);
                                    break;
                                case 2: // Repeated message
                                    if (Param3ComboBox.SelectedIndex == 0)
                                    {
                                        Param2ComboBox.Enabled = false;
                                    }
                                    else if (Param3ComboBox.SelectedIndex == 1)
                                    {
                                        Param2ComboBox.Enabled = true;
                                    }
                                    CCDBusMessageToSendStart = Util.HexStringToByte(Param1ComboBox.Text);
                                    CCDBusMessageToSendEnd = Util.HexStringToByte(Param2ComboBox.Text);
                                    break;
                                default:
                                    break;
                            }
                            break;
                        default:
                            break;
                    }
                    break;
                case 2: // SCI-bus (PCM)
                    switch (CommandComboBox.SelectedIndex)
                    {
                        case 0: // Send message
                            switch (ModeComboBox.SelectedIndex)
                            {
                                case 0: // Stop message transmission
                                    break;
                                case 1: // Single message
                                    SCIBusPCMMessageToSendStart = Util.HexStringToByte(Param1ComboBox.Text);
                                    break;
                                case 2: // Repeated message
                                    if (Param3ComboBox.SelectedIndex == 0)
                                    {
                                        Param2ComboBox.Enabled = false;
                                    }
                                    else if (Param3ComboBox.SelectedIndex == 1)
                                    {
                                        Param2ComboBox.Enabled = true;
                                    }
                                    SCIBusPCMMessageToSendStart = Util.HexStringToByte(Param1ComboBox.Text);
                                    SCIBusPCMMessageToSendEnd = Util.HexStringToByte(Param2ComboBox.Text);
                                    break;
                                default:
                                    break;
                            }
                            break;
                        default:
                            break;
                    }
                    break;
                case 3: // SCI-bus (TCM)
                    switch (CommandComboBox.SelectedIndex)
                    {
                        case 0: // Send message
                            switch (ModeComboBox.SelectedIndex)
                            {
                                case 0: // Stop message transmission
                                    break;
                                case 1: // Single message
                                    SCIBusTCMMessageToSendStart = Util.HexStringToByte(Param1ComboBox.Text);
                                    break;
                                case 2: // Repeated message
                                    if (Param3ComboBox.SelectedIndex == 0)
                                    {
                                        Param2ComboBox.Enabled = false;
                                    }
                                    else if (Param3ComboBox.SelectedIndex == 1)
                                    {
                                        Param2ComboBox.Enabled = true;
                                    }
                                    SCIBusTCMMessageToSendStart = Util.HexStringToByte(Param1ComboBox.Text);
                                    SCIBusTCMMessageToSendEnd = Util.HexStringToByte(Param2ComboBox.Text);
                                    break;
                                default:
                                    break;
                            }
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }

            UpdateUSBSendComboBox();
        }

        private void ConvertTextToBinaryDump(string filename)
        {
            // Open file and decide what kind of memory dump is it
            string[] text = File.ReadAllLines(filename);
            List<byte[]> raw_byte_list = new List<byte[]>();
            int start_address = 0;
            int end_address = 0;

            foreach (string t in text)
            {
                raw_byte_list.Add(Util.HexStringToByte(t));
            }

            List<byte[]> byte_list = raw_byte_list.Distinct().ToList(); // remove duplicated lines caused by the scanner when re-trying a memory address

            byte id = byte_list[0].ToArray()[0]; // firs byte of the first message

            switch (id)
            {
                case 0x15: // PCM ROM dump (16-bit, 0-64 kilobytes)
                    // Determine if the text dump is correct and no values are missing
                    start_address = (byte_list[0].ToArray()[1] << 8) | byte_list[0].ToArray()[2];
                    end_address = (byte_list[byte_list.Count - 1].ToArray()[1] << 8) | byte_list[byte_list.Count - 1].ToArray()[2];

                    // Good sign if there are as much messages like the difference of the ending and starting addresses
                    if ((end_address - start_address + 1) == byte_list.Count)
                    {
                        // See if every line has 4 bytes. The 4th byte is the one that need to be saved in a binary file
                        bool ok = true;
                        foreach (byte[] b in byte_list)
                        {
                            if (b.Length != 4)
                            {
                                ok = false;
                                break;
                            }
                        }

                        if (ok)
                        {
                            using (BinaryWriter writer = new BinaryWriter(File.Open(PCMEPROMBinaryFilename, FileMode.Append)))
                            {
                                foreach (byte[] b in byte_list)
                                {
                                    writer.Write(b[3]); // write the 4th byte to the binary file
                                }

                                writer.Close();
                            }
                            
                            MemoryReadError = 0;
                        }
                        else
                        {
                            MemoryReadError = 2; // memory value missing
                        }

                    }
                    else
                    {
                        MemoryReadError = 1; // not enough messages
                    }
                    MemoryReadFinished = true;
                    break;
                case 0x26: // PCM ROM dump (24-bit, 0-16383 kilobytes)
                    // Determine if the text dump is correct and no values are missing
                    start_address = (byte_list[0].ToArray()[1] << 16) | (byte_list[0].ToArray()[2] << 8) | byte_list[0].ToArray()[3];
                    end_address = (byte_list[byte_list.Count - 1].ToArray()[1] << 16) | (byte_list[byte_list.Count - 1].ToArray()[2] << 8) | byte_list[byte_list.Count - 1].ToArray()[3];

                    // Good sign if there are as much messages like the difference of the ending and starting addresses
                    if ((end_address - start_address + 1) == byte_list.Count)
                    {
                        // See if every line has 5 bytes. The 5th byte is the one that need to be saved in a binary file
                        bool ok = true;
                        foreach (byte[] b in byte_list)
                        {
                            if (b.Length != 5)
                            {
                                ok = false;
                                break;
                            }
                        }

                        if (ok)
                        {
                            using (BinaryWriter writer = new BinaryWriter(File.Open(PCMEPROMBinaryFilename, FileMode.Append)))
                            {
                                foreach (byte[] b in byte_list)
                                {
                                    writer.Write(b[4]); // write the 5th byte to the binary file
                                }

                                writer.Close();
                            }

                            MemoryReadError = 0;
                        }
                        else
                        {
                            MemoryReadError = 2; // memory value missing
                        }
                    }
                    else
                    {
                        MemoryReadError = 1; // not enough messages
                    }
                    MemoryReadFinished = true;
                    break;
                case 0x28: // PCM EEPROM dump (16-bit, 0-64 kilobytes)
                    // Determine if the text dump is correct and no values are missing
                    start_address = (byte_list[0].ToArray()[1] << 8) | byte_list[0].ToArray()[2];
                    end_address = (byte_list[byte_list.Count - 1].ToArray()[1] << 8) | byte_list[byte_list.Count - 1].ToArray()[2];

                    // Good sign if there are as much messages like the difference of the ending and starting addresses
                    if ((end_address - start_address + 1) == byte_list.Count)
                    {
                        // See if every line has 4 bytes. The 4th byte is the one that need to be saved in a binary file
                        bool ok = true;
                        foreach (byte[] b in byte_list)
                        {
                            if (b.Length != 4)
                            {
                                ok = false;
                                break;
                            }
                        }

                        if (ok)
                        {
                            using (BinaryWriter writer = new BinaryWriter(File.Open(PCMEEPROMBinaryFilename, FileMode.Append)))
                            {
                                foreach (byte[] b in byte_list)
                                {
                                    writer.Write(b[3]); // write the 4th byte to the binary file
                                }

                                writer.Close();
                            }

                            MemoryReadError = 0;
                        }
                        else
                        {
                            MemoryReadError = 2; // memory value missing
                        }
                    }
                    else
                    {
                        MemoryReadError = 1; // not enough messages
                    }
                    MemoryReadFinished = true;
                    break;
                default:
                    MemoryReadFinished = true;
                    break;
            }
        }

        private void Param1ComboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                USBSendButton.PerformClick();
                e.Handled = true;
            }
        }

        private void Param2ComboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                USBSendButton.PerformClick();
                e.Handled = true;
            }
        }

        private void Param3ComboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                USBSendButton.PerformClick();
                e.Handled = true;
            }
        }

        private void SnapshotButton_Click(object sender, EventArgs e)
        {
            DateTimeNow = DateTime.Now.ToString("yyyyMMdd_HHmmss"); // new time for each snapshot
            DiagnosticsSnapshotFilename = @"LOG/Diagnostics/diagnosticssnapshot_" + DateTimeNow + ".txt";
            File.AppendAllText(DiagnosticsSnapshotFilename, String.Join(Environment.NewLine, DiagnosticsTable));
        }

        private void SortIDBytesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (SortIDBytesCheckBox.Checked)
            {
                IDSorting = true;
            }
            else
            {
                IDSorting = false;
            }
        }

        private void UpdateScannerFirmwareToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //if (true)
            if ((UpdatePort != String.Empty) && (OldUNIXTime != 0) && ScannerFound)
            {
                Util.UpdateTextBox(USBTextBox, "[INFO] Searching for new scanner firmware" + Environment.NewLine +
                                               "       This may take a few seconds...", null);

                // Download latest main.h file from GitHub
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                try
                {
                    Downloader.DownloadFile(SourceFile, @"Tools/main.h");
                }
                catch
                {
                    Util.UpdateTextBox(USBTextBox, "[INFO] Download error", null);
                }

                if (File.Exists(@"Tools/main.h"))
                {
                    // Get new UNIX time value from the downloaded file
                    string line = String.Empty;
                    bool done = false;
                    using (Stream stream = File.Open(@"Tools/main.h", FileMode.Open))
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            while (!done)
                            {
                                line = reader.ReadLine();
                                if (line.Contains("#define FW_DATE"))
                                {
                                    done = true;
                                }
                            }
                        }
                    }

                    string hexline = line.Substring(16, 18);
                    NewUNIXTime = Convert.ToUInt64(hexline, 16);

                    DateTime OldFirmwareDate = Util.UnixTimeStampToDateTime(OldUNIXTime);
                    DateTime NewFirmwareDate = Util.UnixTimeStampToDateTime(NewUNIXTime);
                    string OldFirmwareDateString = OldFirmwareDate.ToString("yyyy.MM.dd HH:mm:ss");
                    string NewFirmwareDateString = NewFirmwareDate.ToString("yyyy.MM.dd HH:mm:ss");

                    Util.UpdateTextBox(USBTextBox, "[INFO] Old firmware date: " + OldFirmwareDateString, null);
                    Util.UpdateTextBox(USBTextBox, "[INFO] New firmware date: " + NewFirmwareDateString, null);

                    if (NewUNIXTime > OldUNIXTime)
                    {
                        Util.UpdateTextBox(USBTextBox, "[INFO] Downloading new firmware", null);
                        Downloader.DownloadFile(FlashFile, @"Tools/ChryslerCCDSCIScanner.ino.mega.hex");
                        Util.UpdateTextBox(USBTextBox, "[INFO] Beginning firmware update", null);
                        ConnectButton.PerformClick(); // disconnect
                        Thread.Sleep(500); // wait until UI updates its controls
                        this.Refresh();
                        Process process = new Process();
                        process.StartInfo.WorkingDirectory = "Tools";
                        process.StartInfo.FileName = "avrdude.exe";
                        process.StartInfo.Arguments = "-C avrdude.conf -p m2560 -c wiring -P " + UpdatePort + " -b 115200 -D -U flash:w:ChryslerCCDSCIScanner.ino.mega.hex:i";
                        process.Start();
                        process.WaitForExit();
                        this.Refresh();
                        Util.UpdateTextBox(USBTextBox, "[INFO] Scanner firmware update finished" + Environment.NewLine + "       Connect again manually", null);
                        File.Delete(@"Tools/main.h");
                        File.Delete(@"Tools/ChryslerCCDSCIScanner.ino.mega.hex");
                    }
                    else
                    {
                        Util.UpdateTextBox(USBTextBox, "[INFO] No scanner firmware update available", null);
                        File.Delete(@"Tools/main.h");
                    }
                }
            }
            else
            {
                Util.UpdateTextBox(USBTextBox, "[INFO] Scanner firmware update error", null);
            }
        }

        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (about == null || !about.Visible)
            {
                about = new AboutForm(this);
                about.Show();
            }
            else
            {
                about.BringToFront();
            }
        }

        private void CheatSheetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (cheatsheet == null || !cheatsheet.Visible)
            {
                cheatsheet = new CheatSheetForm(this);
                cheatsheet.Show();
            }
            else
            {
                cheatsheet.BringToFront();
            }
        }

        private void CCDBusEnabledCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            byte[] EnableCCDBusTransceiver = new byte[] { 0x3D, 0x00, 0x03, 0x03, 0x02, 0x01, 0x09 };
            byte[] DisableCCDBusTransceiver = new byte[] { 0x3D, 0x00, 0x03, 0x03, 0x02, 0x00, 0x08 };

            if (CCDBusEnabledCheckBox.Checked)
            {
                Serial.Write(EnableCCDBusTransceiver, 0, EnableCCDBusTransceiver.Length);
            }
            else
            {
                Serial.Write(DisableCCDBusTransceiver, 0, DisableCCDBusTransceiver.Length);
            }
        }

        private void IncludeTimestampInLogFilesToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (IncludeTimestampInLogFilesToolStripMenuItem.Checked)
            {
                Properties.Settings.Default["IncludeTimestamp"] = true;
                Properties.Settings.Default.Save(); // Save settings in application configuration file
            }
            else
            {
                Properties.Settings.Default["IncludeTimestamp"] = false;
                Properties.Settings.Default.Save(); // Save settings in application configuration file
            }
        }

        private void MetricToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default["Units"] = "metric";
            Properties.Settings.Default.Save(); // Save settings in application configuration file
            Units = 0;
            ImperialToolStripMenuItem.Checked = false;
        }

        private void ImperialToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default["Units"] = "imperial";
            Properties.Settings.Default.Save(); // Save settings in application configuration file
            Units = 1;
            MetricToolStripMenuItem.Checked = false;
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            UpdateCOMPortList();
        }
    }
}