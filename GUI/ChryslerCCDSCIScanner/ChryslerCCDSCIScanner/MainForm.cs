using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using RJCP.Datastructures;

namespace ChryslerCCDSCIScanner
{
    public partial class MainForm : Form
    {
        public string DateTimeNow;
        public static string USBLogFilename;
        public static string USBBinaryLogFilename;
        public static string DiagnosticsSnapshotFilename;
        public static string CCDLogFilename;
        public static string CCDB2F2LogFilename;
        public static string PCMLogFilename;
        public static string TCMLogFilename;
        public static string UpdateScannerFirmwareLogFilename;
        public static bool USBShowTraffic = true;
        public static byte Units = 0;
        public bool Timeout = false;
        public bool ScannerFound = false;
        public byte[] buffer = new byte[2048];
        public int HeartbeatInterval = 5000;
        public int HeartbeatDuration = 50;
        public int RandomCCDMessageIntervalMin = 20;
        public int RandomCCDMessageIntervalMax = 100;
        public bool SetCCDBus = true;
        public byte SetSCIBus = 2;
        public byte[] CCDBusMessageToSend = new byte[] { 0xB2, 0x20, 0x22, 0x00, 0x00, 0xF4 };
        public byte[] SCIBusPCMMessageToSend = new byte[] { 0x10 };
        public byte[] SCIBusTCMMessageToSend = new byte[] { 0x10 };
        public List<byte> PacketBytes = new List<byte>();
        public byte PacketBytesChecksum = 0;

        public BindingList<string> DiagnosticsTable = new BindingList<string>();
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


        byte LastTargetIndex = 0; // Scanner
        byte LastCommandIndex = 2; // Status
        byte LastModeIndex = 0; // None

        public enum TransmissionMethod
        {
            Hex = 0,
            Ascii = 1
        }
        public byte TM = 0;

        public enum Source
        {
            USB = 0,
            CCDBus = 1,
            SCIBusPCM = 2,
            SCIBusTCM = 3
        }

        public enum Target
        {
            USB = 0,
            CCDBus = 1,
            SCIBusPCM = 2,
            SCIBusTCM = 3
        }

        public enum Command
        {
            Reset = 0,
            Handshake = 1,
            Status = 2,
            Settings = 3,
            Request = 4,
            Response = 5,
            SendMessage = 6,
            MessageReceived = 7,
            Debug = 14,
            OkError = 15
        }

        public enum Response
        {
            HwFwInfo = 0,
            Timestamp = 1,
            BatteryVoltage = 2,
            ExternalEEPROMChecksum = 3
        }

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
        BackgroundWorker bw = new BackgroundWorker();
        SerialPort Serial = new SerialPort();
        CircularBuffer<byte> SerialRxBuffer = new CircularBuffer<byte>(2048);
        LookupTable LT = new LookupTable();
        System.Timers.Timer TimeoutTimer = new System.Timers.Timer();
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

            // Set logfile names inside the LOG directory
            DateTimeNow = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            USBLogFilename = @"LOG/USB/usblog_" + DateTimeNow + ".txt";
            USBBinaryLogFilename = @"LOG/USB/usblog_" + DateTimeNow + ".bin";
            CCDLogFilename = @"LOG/CCD/ccdlog_" + DateTimeNow + ".txt";
            CCDB2F2LogFilename = @"LOG/CCD/ccdb2f2log_" + DateTimeNow + ".txt";
            PCMLogFilename = @"LOG/PCM/pcmlog_" + DateTimeNow + ".txt";
            TCMLogFilename = @"LOG/TCM/tcmlog_" + DateTimeNow + ".txt";

            // Setup timeout timer
            TimeoutTimer.Elapsed += new ElapsedEventHandler(TimeoutHandler);
            TimeoutTimer.Interval = 2000; // ms
            TimeoutTimer.Enabled = false;

            // Setup backgroundworker
            //bw.DoWork += (o, args) => DataReceived();
            //bw.RunWorkerCompleted += (o, args) => UpdateDiagnosticsListBox();

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
                if ((string)Properties.Settings.Default["Units"] == "metric")
                {
                    MetricUnitRadioButton.Checked = true;
                    Units = 0;
                }
                else if ((string)Properties.Settings.Default["Units"] == "imperial")
                {
                    ImperialUnitRadioButton.Checked = true;
                    Units = 1;
                }
                if ((string)Properties.Settings.Default["TransmissionMethod"] == "hex") HexCommMethodRadioButton.Checked = true;
                else if ((string)Properties.Settings.Default["TransmissionMethod"] == "ascii") AsciiCommMethodRadioButton.Checked = true;
            }
            catch
            {
                Util.UpdateTextBox(USBTextBox, "[INFO] Application config file is missing", null);
                Units = 0; // Metric units by default
            }

            if (!File.Exists("VehicleProfiles.xml"))
            {
                Util.UpdateTextBox(USBTextBox, "[INFO] Vehicle profiles file is missing", null);
            }

            if (!Directory.Exists("Tools"))
            {
                Util.UpdateTextBox(USBTextBox, "[INFO] AVRDUDE is missing", null);
            }

            ActiveControl = ConnectButton; // put focus on the connect button
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            ScannerFound = false;
            if (Serial.IsOpen) Serial.Close();
        }

        private void TimeoutHandler(object source, ElapsedEventArgs e)
        {
            Timeout = true;
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            if (!ScannerFound) // only let connect once when there's no scanner found yet
            {
                ConnectButton.Enabled = false; // no double-click
                byte[] HandshakeRequest = new byte[] { 0x3D, 0x00, 0x02, 0x01, 0x01, 0x04 }; // with hardware/firmware request
                bool HandshakeFound = false;
                string[] ports = SerialPort.GetPortNames(); // get all available portnames
                if (ports.Length == 0) // if there's none, do nothing
                {
                    Util.UpdateTextBox(USBTextBox, "[INFO] No scanner available", null);
                    ConnectButton.Enabled = true;
                    return;
                }

                Util.UpdateTextBox(USBTextBox, "[INFO] Searching scanner", null);
                foreach (string port in ports) // iterate through all available serial ports
                {
                    if (Serial.IsOpen) Serial.Close(); // can't overwrite fields if serial port is open
                    Serial.PortName = port;
                    Serial.BaudRate = 250000;
                    Serial.DataBits = 8;
                    Serial.StopBits = StopBits.One;
                    Serial.Parity = Parity.None;
                    Serial.ReadTimeout = 500;
                    Serial.WriteTimeout = 500;

                    try
                    {
                        Serial.Open(); // open current serial port
                    }
                    catch
                    {
                        Util.UpdateTextBox(USBTextBox, "[INFO] " + Serial.PortName + " is opened by another application", null);
                        continue; // skip current iteration and jump to next comport (if any)
                    }

                    Serial.DiscardInBuffer();
                    Serial.DiscardOutBuffer();
                    Serial.BaseStream.Flush();
                    Util.UpdateTextBox(USBTextBox, "[<-TX] Handshake request (" + Serial.PortName + ")", HandshakeRequest);
                    SerialDataWriteAsync(HandshakeRequest);

                    Timeout = false; // for the connection procedure we have to manually read response bytes here
                    TimeoutTimer.Enabled = true; // start counting to the set timeout value

                    while (!HandshakeFound && !Timeout)
                    {
                        if (Serial.BytesToRead > 0)
                        {
                            byte Byte01 = (byte)Serial.ReadByte();
                            
                            if (Byte01 == 0x3D) // look for packet sync byte
                            {
                                byte Byte02 = (byte)Serial.ReadByte();
                                byte Byte03 = (byte)Serial.ReadByte();
                                byte Byte04 = (byte)Serial.ReadByte();

                                if ((Byte02 == 0x00) && (Byte03 == 0x17) && (Byte04 == 0x01)) // look for handshake packet identifiers (length, datacode)
                                {
                                    HandshakeFound = true;
                                    Serial.ReadByte(); // read the subdatacode into oblivion
                                }
                            }
                        }
                    }

                    TimeoutTimer.Enabled = false; // disable timer
                    if (Timeout)
                    {
                        if (Serial.BytesToRead > 0)
                        {
                            Util.UpdateTextBox(USBTextBox, "[INFO] " + Serial.PortName + " answer is invalid", null);
                        }
                        else Util.UpdateTextBox(USBTextBox, "[INFO] " + Serial.PortName + " is not answering", null);
                        Serial.DiscardInBuffer();
                        Serial.DiscardOutBuffer();
                        Serial.BaseStream.Flush();
                        continue; // skip current iteration and jump to next comport (if any)
                    }

                    if (HandshakeFound)
                    {
                        byte[] msg = new byte[27];
                        msg[0] = 0x3D;
                        msg[1] = 0x00;
                        msg[2] = 0x17;
                        msg[3] = 0x01;
                        msg[4] = 0x00;
                        Serial.Read(msg, 5, 22);

                        if (Encoding.ASCII.GetString(msg, 5, 21) == "CHRYSLERCCDSCISCANNER") // compare payload to the expected handshake response (ascii text)
                        {
                            Util.UpdateTextBox(USBTextBox, "[RX->] Handshake response (" + Serial.PortName + ")", msg);
                            Util.UpdateTextBox(USBTextBox, "[INFO] Handshake OK: " + Encoding.ASCII.GetString(msg, 5, 21), null);
                            Util.UpdateTextBox(USBTextBox, "[INFO] Scanner connected (" + Serial.PortName + ")", null);
                            ScannerFound = true; // set flag
                            SerialDataReadAsync(); // start listening to the last opened serial port for incoming data

                            ConnectButton.Text = "Disconnect";
                            ConnectButton.Enabled = true;
                            USBCommunicationGroupBox.Enabled = true;
                            UpdateScannerFirmwareToolStripMenuItem.Enabled = true;

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
                            USBCommunicationGroupBox.Text = "USB communication (" + Serial.PortName + ")";
                            UpdatePort = Serial.PortName;
                            return;
                        }
                        else
                        {
                            Util.UpdateTextBox(USBTextBox, "[RX->] Handshake response (" + Serial.PortName + ")", msg);
                            Util.UpdateTextBox(USBTextBox, "[INFO] Handshake ERROR: " + Encoding.ASCII.GetString(msg, 5, 21), null);
                            ScannerFound = false;
                            Serial.DiscardInBuffer();
                            Serial.DiscardOutBuffer();
                            Serial.BaseStream.Flush();
                        }
                    }
                }
                Util.UpdateTextBox(USBTextBox, "[INFO] No scanner available", null);
                ConnectButton.Enabled = true;
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

                    Timeout = false;
                    TimeoutTimer.Enabled = true; // start counting to the set timeout value
                    while ((SerialRxBuffer.ReadLength > 0) && !Timeout) ; // Wait for ringbuffer to be empty
                    TimeoutTimer.Enabled = false; // disable timer

                    SerialRxBuffer.Reset();
                    Util.UpdateTextBox(USBTextBox, "[INFO] Scanner disconnected (" + Serial.PortName + ")", null);
                    USBSendComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                    USBCommunicationGroupBox.Enabled = false;
                    USBCommunicationGroupBox.Text = "USB communication";
                    UpdateScannerFirmwareToolStripMenuItem.Enabled = false;
                }
            }
        }

        private async Task SerialDataReadAsyncTask()
        {
            while (ScannerFound)
            {
                Task<int> readByteTask = Serial.BaseStream.ReadAsync(buffer, 0, buffer.Length);
                int bytesRead = await readByteTask;
                SerialRxBuffer.Append(buffer, 0, bytesRead);
                DataReceived();
            }
        }

        private async void SerialDataReadAsync()
        {
            try
            {
                await Task.Run(() => SerialDataReadAsyncTask());
            }
            catch
            {
                if (!USBTextBox.IsDisposed && ScannerFound)
                {
                    Util.UpdateTextBox(USBTextBox, "[INFO] Can't listen to " + Serial.PortName, null);
                    try
                    {
                        Serial.DiscardInBuffer();
                        Serial.DiscardOutBuffer();
                        Serial.BaseStream.Flush();
                    }
                    catch
                    {
                        Util.UpdateTextBox(USBTextBox, "[INFO] USB disconnected", null);
                    }
                }
            }
        }

        private async void SerialDataWriteAsync(byte[] message)
        {
            try
            {
                await Serial.BaseStream.WriteAsync(message, 0, message.Length);
            }
            catch
            {
                if (!USBTextBox.IsDisposed)
                {
                    Util.UpdateTextBox(USBTextBox, "[INFO] Can't write to " + Serial.PortName, null);
                    try
                    {
                        Serial.DiscardInBuffer();
                        Serial.DiscardOutBuffer();
                        Serial.BaseStream.Flush();
                    }
                    catch
                    {
                        Util.UpdateTextBox(USBTextBox, "[INFO] USB disconnected", null);
                    }
                }
            }
        }

        private void DataReceived()
        {
            // Find the first byte with value 0x3D ("=") or 0x3E (">")
            Here: // This is a goto label, never mind now, might want to get rid of it with another while-loop
            while (((SerialRxBuffer.Array[SerialRxBuffer.Start] != 0x3D) && (SerialRxBuffer.Array[SerialRxBuffer.Start] != 0x3E)) && (SerialRxBuffer.ReadLength > 0))
            {
                SerialRxBuffer.Pop();
            }

            switch (SerialRxBuffer.Array[SerialRxBuffer.Start])
            {
                case 0x3D:
                    int PacketSize = 0;

                    // Get the length of the packet
                    // This becomes unsafe when the ringbuffer reaches the last positions and overflows to the first one!
                    if (SerialRxBuffer.ReadLength > 2)
                    {
                        PacketSize = ((SerialRxBuffer.Array[SerialRxBuffer.Start + 1] << 8) | SerialRxBuffer.Array[SerialRxBuffer.Start + 2]) + 4;
                    }
                    else { return; } // If there are clearly not enough bytes then don't do anything

                    // If the size is bigger than 1018 bytes (payload only, + 6 bytes = 1024) then it's most likely garbage data 
                    if (PacketSize > 1018)
                    {
                        SerialRxBuffer.Pop(); // Pop this byte that lead us here so we can search for another start sign
                        goto Here; // Jump back to the while loop to repeat
                    }

                    Timeout = false;
                    TimeoutTimer.Enabled = true; // start counting to the set timeout value
                    while ((SerialRxBuffer.ReadLength < PacketSize) && !Timeout) ; // Wait for expected bytes to arrive
                    TimeoutTimer.Enabled = false; // disable timer
                    if (Timeout)
                    {
                        SerialRxBuffer.Reset();
                        return;
                    }

                    byte[] msg = new byte[PacketSize];
                    SerialRxBuffer.MoveTo(msg, 0, PacketSize);

                    // Extract information from the received bytes
                    byte sync = msg[0];
                    byte length_hb = msg[1];
                    byte length_lb = msg[2];
                    byte datacode = msg[3];
                    byte subdatacode = msg[4];
                    byte[] payload = new byte[PacketSize - 6];
                    Array.Copy(msg, 5, payload, 0, PacketSize - 6);
                    byte checksum = msg[PacketSize - 1]; // -1 because zero-indexing

                    byte source = (byte)((datacode >> 6) & 0x03);
                    byte target = (byte)((datacode >> 4) & 0x03);
                    byte command = (byte)(datacode & 0x0F);

                    int location = 0;
                    int secondary_location = 0;
                    byte IDByte = 0;
                    byte SecondaryIDByte = 0;

                    switch (source)
                    {
                        case (byte)Source.USB: // message is coming from the scanner directly, no need to analyze target, it has to be an external computer
                            switch (command)
                            {
                                case (byte)Command.Reset:
                                    if (payload[0] == 0) Util.UpdateTextBox(USBTextBox, "[RX->] Scanner is resetting, please wait", msg);
                                    else if (payload[0] == 1) Util.UpdateTextBox(USBTextBox, "[RX->] Scanner is ready to accept instructions", msg);
                                    break;
                                case (byte)Command.Handshake:
                                    if (Encoding.ASCII.GetString(payload) == "CHRYSLERCCDSCISCANNER")
                                    {
                                        Util.UpdateTextBox(USBTextBox, "[RX->] Handshake response (" + Serial.PortName + ")", msg);
                                        Util.UpdateTextBox(USBTextBox, "[INFO] Handshake OK: " + Encoding.ASCII.GetString(payload), null);
                                        ScannerFound = true;
                                    }
                                    else
                                    {
                                        Util.UpdateTextBox(USBTextBox, "[RX->] Handshake response (" + Serial.PortName + ")", msg);
                                        Util.UpdateTextBox(USBTextBox, "[INFO] Handshake ERROR", null);
                                        ScannerFound = false;
                                    }
                                    break;
                                case (byte)Command.Status:
                                    string AVRSignature = Util.ByteToHexString(payload, 0, 3);
                                    if ((payload[0] == 0x1E) && (payload[1] == 0x98) && (payload[2] == 0x01)) AVRSignature += " (ATmega2560)";
                                    else AVRSignature += " (unknown)";
                                    string HardwareVersion = "V" + ((payload[3] << 8 | payload[4]) / 100.00).ToString("0.00").Replace(",", ".");
                                    DateTime HardwareDate = Util.UnixTimeStampToDateTime(payload[5] << 56 | payload[6] << 48 | payload[7] << 40 | payload[8] << 32 | payload[9] << 24 | payload[10] << 16 | payload[11] << 8 | payload[12]);
                                    DateTime AssemblyDate = Util.UnixTimeStampToDateTime(payload[13] << 56 | payload[14] << 48 | payload[15] << 40 | payload[16] << 32 | payload[17] << 24 | payload[18] << 16 | payload[19] << 8 | payload[20]);
                                    DateTime FirmwareDate = Util.UnixTimeStampToDateTime(payload[21] << 56 | payload[22] << 48 | payload[23] << 40 | payload[24] << 32 | payload[25] << 24 | payload[26] << 16 | payload[27] << 8 | payload[28]);
                                    OldUNIXTime = (UInt64)(payload[21] << 56 | payload[22] << 48 | payload[23] << 40 | payload[24] << 32 | payload[25] << 24 | payload[26] << 16 | payload[27] << 8 | payload[28]);
                                    string HardwareDateString = HardwareDate.ToString("yyyy.MM.dd HH:mm:ss");
                                    string AssemblyDateString = AssemblyDate.ToString("yyyy.MM.dd HH:mm:ss");
                                    string FirmwareDateString = FirmwareDate.ToString("yyyy.MM.dd HH:mm:ss");
                                    string extEEPROMPresent = String.Empty;
                                    if (payload[29] == 0x01) extEEPROMPresent += "yes";
                                    else extEEPROMPresent += "no";
                                    string extEEPROMChecksum = Util.ByteToHexString(payload, 30, 31);
                                    if (payload[30] == payload[31]) extEEPROMChecksum += " (OK)";
                                    else extEEPROMChecksum += " (ERROR [" + Util.ByteToHexString(payload, 31, 32) + "])";
                                    TimeSpan ElapsedMillis = TimeSpan.FromMilliseconds(payload[32] << 24 | payload[33] << 16 | payload[34] << 8 | payload[35]);
                                    DateTime MicrocontrollerTimestamp = DateTime.Today.Add(ElapsedMillis);
                                    string MicrocontrollerTimestampString = MicrocontrollerTimestamp.ToString("HH:mm:ss.fff");
                                    string FreeRAMString = ((payload[36] << 8) | payload[37]).ToString() + " free of 8192 bytes";
                                    string ConnectedToVehicle = String.Empty;
                                    if (payload[38] == 0x01) ConnectedToVehicle = "yes";
                                    else ConnectedToVehicle = "no";
                                    string BatteryVoltageString = ((payload[39] << 8 | payload[40]) / 100.00).ToString("0.00").Replace(",", ".") + " V";
                                    string CCDBusStateString = String.Empty;
                                    if (payload[41] == 0x01)
                                    {
                                        CCDBusStateString = "enabled";
                                        CCDBusEnabled = true;
                                    }
                                    else
                                    {
                                        CCDBusStateString = "disabled";
                                        CCDBusEnabled = false;
                                    }
                                    CCDBusMsgRxCount = (payload[42] << 24) | (payload[43] << 16) | (payload[44] << 8) | payload[45];
                                    CCDBusMsgTxCount = (payload[46] << 24) | (payload[47] << 16) | (payload[48] << 8) | payload[49];
                                    string CCDBusRxMessagesString = CCDBusMsgRxCount.ToString();
                                    string CCDBusTxMessagesString = CCDBusMsgTxCount.ToString();
                                    string SCIBusPCMStateString = String.Empty;
                                    string SCIBusPCMConfigurationString = String.Empty;
                                    string SCIBusPCMSpeedString = String.Empty;
                                    string SCIBusTCMStateString = String.Empty;
                                    string SCIBusTCMConfigurationString = String.Empty;
                                    string SCIBusTCMSpeedString = String.Empty;
                                    if ((payload[50] & 0x40) != 0)
                                    {
                                        SCIBusPCMStateString = "enabled";
                                        SCIBusPCMEnabled = true;
                                        if ((payload[50] & 0x20) != 0)
                                        {
                                            SCIBusPCMConfigurationString = "B";
                                            SCIBusPCMHeaderConfiguration = "B";
                                        }
                                        else
                                        {
                                            SCIBusPCMConfigurationString = "A";
                                            SCIBusPCMHeaderConfiguration = "A";
                                        }
                                        if ((payload[50] & 0x10) != 0)
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
                                    if ((payload[50] & 0x04) != 0)
                                    {
                                        SCIBusTCMStateString = "enabled";
                                        SCIBusTCMEnabled = true;
                                        if ((payload[50] & 0x02) != 0)
                                        {
                                            SCIBusTCMConfigurationString = "B";
                                            SCIBusTCMHeaderConfiguration = "B";
                                        }
                                        else
                                        {
                                            SCIBusTCMConfigurationString = "A";
                                            SCIBusTCMHeaderConfiguration = "A";
                                        }
                                        if ((payload[50] & 0x01) != 0)
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
                                    SCIBusPCMMsgRxCount = (payload[51] << 24) | (payload[52] << 16) | (payload[53] << 8) | payload[54];
                                    SCIBusPCMMsgTxCount = (payload[55] << 24) | (payload[56] << 16) | (payload[57] << 8) | payload[58];
                                    SCIBusTCMMsgRxCount = (payload[59] << 24) | (payload[60] << 16) | (payload[61] << 8) | payload[62];
                                    SCIBusTCMMsgTxCount = (payload[63] << 24) | (payload[64] << 16) | (payload[65] << 8) | payload[66];
                                    string SCIBusPCMRxMessagesString = SCIBusPCMMsgRxCount.ToString();
                                    string SCIBusPCMTxMessagesString = SCIBusPCMMsgTxCount.ToString();
                                    string SCIBusTCMRxMessagesString = SCIBusTCMMsgRxCount.ToString();
                                    string SCIBusTCMTxMessagesString = SCIBusTCMMsgTxCount.ToString();

                                    Util.UpdateTextBox(USBTextBox, "[RX->] Status response", msg);
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
                                    break;
                                case (byte)Command.Settings:
                                    switch (subdatacode)
                                    {
                                        case 0x00: // Heartbeat
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Heartbeat settings changed", msg);
                                            if ((payload[0] << 8 | payload[1]) > 0)
                                            {
                                                string interval = (payload[0] << 8 | payload[1]).ToString() + " ms";
                                                string duration = (payload[2] << 8 | payload[3]).ToString() + " ms";
                                                Util.UpdateTextBox(USBTextBox, "[INFO] Heartbeat enabled:" + Environment.NewLine +
                                                                                "       Interval: " + interval + Environment.NewLine +
                                                                                "       Duration: " + duration, null);
                                            }
                                            else
                                            {
                                                Util.UpdateTextBox(USBTextBox, "[INFO] Heartbeat disabled", null);
                                            }
                                            break;
                                        case 0x01: // Set CCD-bus
                                            Util.UpdateTextBox(USBTextBox, "[RX->] CCD-bus settings changed", msg);
                                            if (payload[0] == 0)
                                            {
                                                Util.UpdateTextBox(USBTextBox, "[INFO] CCD-bus chip disabled", null);
                                                CCDBusEnabled = false;
                                            }
                                            else if (payload[0] == 1)
                                            {
                                                Util.UpdateTextBox(USBTextBox, "[INFO] CCD-bus chip enabled @ 7812.5 baud", null);
                                                CCDBusEnabled = true;
                                            }
                                            break;
                                        case 0x02: // Set SCI-bus
                                            string configuration = String.Empty;

                                            configuration += "[INFO] PCM settings:" + Environment.NewLine;

                                            if (((payload[0] >> 6) & 0x01) == 0)
                                            {
                                                configuration += "       - state: disabled" + Environment.NewLine;
                                                SCIBusPCMEnabled = false;
                                            }
                                            else
                                            {
                                                configuration += "       - state: enabled" + Environment.NewLine;
                                                SCIBusPCMEnabled = true;
                                            }

                                            if (((payload[0] >> 5) & 0x01) == 0)
                                            {
                                                if (((payload[0] >> 6) & 0x01) == 0)
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
                                                if (((payload[0] >> 6) & 0x01) == 0)
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

                                            if (((payload[0] >> 4) & 0x01) == 0)
                                            {
                                                if (((payload[0] >> 6) & 0x01) == 0)
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
                                                if (((payload[0] >> 6) & 0x01) == 0)
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

                                            if (((payload[0] >> 2) & 0x01) == 0)
                                            {
                                                configuration += "       - state: disabled" + Environment.NewLine;
                                                SCIBusTCMEnabled = false;
                                            }
                                            else
                                            {
                                                configuration += "       - state: enabled" + Environment.NewLine;
                                                SCIBusTCMEnabled = true;
                                            }

                                            if (((payload[0] >> 1) & 0x01) == 0)
                                            {
                                                if (((payload[0] >> 2) & 0x01) == 0)
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
                                                if (((payload[0] >> 2) & 0x01) == 0)
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

                                            if ((payload[0] & 0x01) == 0)
                                            {
                                                if (((payload[0] >> 2) & 0x01) == 0)
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
                                                if (((payload[0] >> 2) & 0x01) == 0)
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

                                            Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus settings changed", msg);
                                            Util.UpdateTextBox(USBTextBox, configuration, null);
                                            break;
                                    }
                                    break;
                                case (byte)Command.Response:
                                    switch (subdatacode)
                                    {
                                        case (byte)Response.HwFwInfo:
                                            string HardwareVersionString = "V" + ((payload[0] << 8 | payload[1]) / 100.00).ToString("0.00").Replace(",", ".");
                                            DateTime _HardwareDate = Util.UnixTimeStampToDateTime(payload[2] << 56 | payload[3] << 48 | payload[4] << 40 | payload[5] << 32 | payload[6] << 24 | payload[7] << 16 | payload[8] << 8 | payload[9]);
                                            DateTime _AssemblyDate = Util.UnixTimeStampToDateTime(payload[10] << 56 | payload[11] << 48 | payload[12] << 40 | payload[13] << 32 | payload[14] << 24 | payload[15] << 16 | payload[16] << 8 | payload[17]);
                                            DateTime _FirmwareDate = Util.UnixTimeStampToDateTime(payload[18] << 56 | payload[19] << 48 | payload[20] << 40 | payload[21] << 32 | payload[22] << 24 | payload[23] << 16 | payload[24] << 8 | payload[25]);
                                            string _HardwareDateString = _HardwareDate.ToString("yyyy.MM.dd HH:mm:ss");
                                            string _AssemblyDateString = _AssemblyDate.ToString("yyyy.MM.dd HH:mm:ss");
                                            string _FirmwareDateString = _FirmwareDate.ToString("yyyy.MM.dd HH:mm:ss");
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Hardware/Firmware information response", msg);
                                            Util.UpdateTextBox(USBTextBox, "[INFO] Hardware ver.: " + HardwareVersionString + Environment.NewLine +
                                                                            "       Hardware date: " + _HardwareDateString + Environment.NewLine +
                                                                            "       Assembly date: " + _AssemblyDateString + Environment.NewLine +
                                                                            "       Firmware date: " + _FirmwareDateString, null);
                                            OldUNIXTime = (UInt64)(payload[18] << 56 | payload[19] << 48 | payload[20] << 40 | payload[21] << 32 | payload[22] << 24 | payload[23] << 16 | payload[24] << 8 | payload[25]);
                                            break;
                                        case (byte)Response.Timestamp:
                                            TimeSpan ElapsedTime = TimeSpan.FromMilliseconds(payload[0] << 24 | payload[1] << 16 | payload[2] << 8 | payload[3]);
                                            DateTime Timestamp = DateTime.Today.Add(ElapsedTime);
                                            string TimestampString = Timestamp.ToString("HH:mm:ss.fff");
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Timestamp response", msg);
                                            Util.UpdateTextBox(USBTextBox, "[INFO] Timestamp: " + TimestampString, null);
                                            break;
                                        case (byte)Response.BatteryVoltage:
                                            string _BatteryVoltageString = ((payload[0] << 8 | payload[1]) / 100.00).ToString("0.00").Replace(",", ".") + " V";
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Battery voltage response", msg);
                                            Util.UpdateTextBox(USBTextBox, "[INFO] Battery voltage: " + _BatteryVoltageString, null);
                                            break;
                                        case (byte)Response.ExternalEEPROMChecksum:
                                            Util.UpdateTextBox(USBTextBox, "[RX->] External EEPROM checksum response", msg);
                                            if (payload[0] == 0x01) // External EEPROM present
                                            {
                                                string ExternalEEPROMChecksumReading = Util.ByteToHexString(payload, 1, payload.Length - 1);
                                                string ExternalEEPROMChecksumCalculated = Util.ByteToHexString(payload, 2, payload.Length);
                                                if (payload[1] == payload[2]) // if checksum reading and checksum calculation is the same
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
                                            break;
                                        default:
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Data received", msg);
                                            break;
                                    }
                                    break;
                                case (byte)Command.SendMessage:
                                    switch (subdatacode)
                                    {
                                        case 0x00:
                                            if (payload[0] == 0x01) Util.UpdateTextBox(USBTextBox, "[RX->] CCD-bus message TX stopped", msg);
                                            else if (payload[0] == 0x02) Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (PCM) message TX stopped", msg);
                                            else if (payload[0] == 0x03) Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (TCM) message TX stopped", msg);
                                            break;
                                        case 0x01:
                                            if (payload[0] == 0x01) Util.UpdateTextBox(USBTextBox, "[RX->] CCD-bus message prepared for TX", msg);
                                            else if (payload[0] == 0x02) Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (PCM) message prepared for TX", msg);
                                            else if (payload[0] == 0x03) Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (TCM) message prepared for TX", msg);
                                            break;
                                        case 0x02:
                                            if (payload[0] == 0x01) Util.UpdateTextBox(USBTextBox, "[RX->] CCD-bus message TX started", msg);
                                            else if (payload[0] == 0x02) Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (PCM) message TX started", msg);
                                            else if (payload[0] == 0x03) Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (TCM) message TX started", msg);
                                            break;
                                        default:
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Data received", msg);
                                            break;
                                    }
                                    break;
                                case (byte)Command.OkError:
                                    switch (subdatacode)
                                    {
                                        case 0x00:
                                            Util.UpdateTextBox(USBTextBox, "[RX->] OK", msg);
                                            break;
                                        case 0x01:
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Error: invalid length", msg);
                                            break;
                                        case 0x02:
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Error: invalid dc command", msg);
                                            break;
                                        case 0x03:
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Error: invalid sub-data code", msg);
                                            break;
                                        case 0x04:
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Error: invalid payload value(s)", msg);
                                            break;
                                        case 0x05:
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Error: invalid checksum", msg);
                                            break;
                                        case 0x06:
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Error: packet timeout occured", msg);
                                            break;
                                        case 0x07:
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Error: buffer overflow", msg);
                                            break;
                                        case 0xFB:
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Error: external EEPROM not found", msg);
                                            break;
                                        case 0xFC:
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Error: external EEPROM reading not possible", msg);
                                            break;
                                        case 0xFD:
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Error: external EEPROM writing not possible", msg);
                                            break;
                                        case 0xFE:
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Error: internal error", msg);
                                            break;
                                        case 0xFF:
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Error: fatal error", msg);
                                            break;
                                    }
                                    break;
                                default:
                                    Util.UpdateTextBox(USBTextBox, "[RX->] Data received", msg);
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
                        case (byte)Source.CCDBus:
                            StringBuilder ccdlistitem = new StringBuilder(EmptyLine); // start with a pre-defined empty line
                            string ccdmsgtoinsert = String.Empty;
                            string ccddescriptiontoinsert = String.Empty;
                            string ccdvaluetoinsert = String.Empty;
                            string ccdunittoinsert = String.Empty;
                            byte[] ccdtimestamp = new byte[4];
                            byte[] ccdmessage = new byte[payload.Length - 4];
                            Array.Copy(payload, 0, ccdtimestamp, 0, 4); // copy timestamp only
                            Array.Copy(payload, 4, ccdmessage, 0, payload.Length - 4); // copy message only

                            IDByte = ccdmessage[0];

                            if (ccdmessage.Length < 9) // max 8 byte fits the message column
                            {
                                ccdmsgtoinsert = Util.ByteToHexString(ccdmessage, 0, ccdmessage.Length) + " ";
                            }
                            else // Trim message (display 7 bytes only) and insert two dots at the end indicating there's more to it
                            {
                                ccdmsgtoinsert = Util.ByteToHexString(ccdmessage, 0, 7) + " .. ";
                            }

                            // Insert message hex bytes in the line
                            ccdlistitem.Remove(2, ccdmsgtoinsert.Length); // remove as much whitespaces as the length of the message
                            ccdlistitem.Insert(2, ccdmsgtoinsert); // insert message where whitespaces were

                            // Determine description, value and unit texts
                            switch (IDByte)
                            {
                                case 0x24:
                                    if (ccdmessage.Length > 3)
                                    {
                                        ccddescriptiontoinsert = LT.GetCCDBusMessageDescription(0x24);

                                        if (Units == 1) // Imperial
                                        {
                                            ccdvaluetoinsert = ccdmessage[1].ToString("0");
                                            ccdunittoinsert = LT.GetCCDBusMessageUnitImperial(0x24)[0];
                                        }
                                        if (Units == 0) // Metric
                                        {
                                            ccdvaluetoinsert = ccdmessage[2].ToString("0");
                                            ccdunittoinsert = LT.GetCCDBusMessageUnitMetric(0x24)[1];
                                        }
                                    }
                                    break;
                                case 0x29:
                                    if (ccdmessage.Length > 3)
                                    {
                                        ccddescriptiontoinsert = LT.GetCCDBusMessageDescription(0x29);

                                        if (ccdmessage[1] < 10) ccdvaluetoinsert = "0";
                                        ccdvaluetoinsert += ccdmessage[1].ToString("0") + ":";
                                        if (ccdmessage[2] < 10) ccdvaluetoinsert += "0";
                                        ccdvaluetoinsert += ccdmessage[2].ToString("0");

                                        ccdunittoinsert = LT.GetCCDBusMessageUnitMetric(0x29)[0];
                                    }
                                    break;
                                case 0x42:
                                    if (ccdmessage.Length > 3)
                                    {
                                        ccddescriptiontoinsert = LT.GetCCDBusMessageDescription(0x42);

                                        ImperialSlope = LT.GetCCDBusMessageSlope(0x42)[0];
                                        ccdvaluetoinsert = (ccdmessage[1] * ImperialSlope).ToString("0");

                                        ccdunittoinsert = LT.GetCCDBusMessageUnitMetric(0x42)[0];
                                    }
                                    break;
                                case 0x44:
                                    if (ccdmessage.Length > 2)
                                    {
                                        ccddescriptiontoinsert = LT.GetCCDBusMessageDescription(0x44);

                                        ccdvaluetoinsert = ccdmessage[1].ToString("0");

                                        ccdunittoinsert = LT.GetCCDBusMessageUnitMetric(0x44)[0];
                                    }
                                    break;
                                case 0x50:
                                    if (ccdmessage.Length > 2)
                                    {
                                        ccddescriptiontoinsert = LT.GetCCDBusMessageDescription(0x50);
                                        if ((ccdmessage[1] & 0x01) == 0x01) ccdvaluetoinsert = "AIRBAG LAMP ON";
                                        else ccdvaluetoinsert = "AIRBAG LAMP OFF";

                                        ccdunittoinsert = LT.GetCCDBusMessageUnitMetric(0x50)[0];
                                    }
                                    break;
                                case 0x6D:
                                    if (ccdmessage.Length > 3)
                                    {
                                        ccddescriptiontoinsert = LT.GetCCDBusMessageDescription(0x6D);
                                        if ((ccdmessage[1] > 0) && (ccdmessage[1] < 18))
                                        {
                                            // Replace characters in the VIN string (one by one)
                                            VIN = VIN.Remove(ccdmessage[1] - 1, 1).Insert(ccdmessage[1] - 1, ((char)(ccdmessage[2])).ToString());
                                        }

                                        ccdunittoinsert = LT.GetCCDBusMessageUnitMetric(0x6D)[0];
                                    }
                                    ccdvaluetoinsert = VIN;
                                    break;
                                case 0x75:
                                    if (ccdmessage.Length > 3)
                                    {
                                        ccddescriptiontoinsert = LT.GetCCDBusMessageDescription(0x75);

                                        if (Units == 1) // Imperial
                                        {
                                            ImperialSlope = LT.GetCCDBusMessageSlope(0x75)[0];
                                            ccdvaluetoinsert = (ccdmessage[1] * ImperialSlope).ToString("0.0").Replace(",", ".");

                                            ccdunittoinsert = LT.GetCCDBusMessageUnitImperial(0x75)[0];
                                        }
                                        if (Units == 0) // Metric
                                        {
                                            MetricSlope = (LT.GetCCDBusMessageSlope(0x75)[0] * LT.GetCCDBusMessageSlConv(0x75)[0]);
                                            ccdvaluetoinsert = (ccdmessage[1] * MetricSlope).ToString("0.0").Replace(",", ".");

                                            ccdunittoinsert = LT.GetCCDBusMessageUnitMetric(0x75)[0];
                                        }
                                    }
                                    break;
                                case 0x7B:
                                    if (ccdmessage.Length > 2)
                                    {
                                        ccddescriptiontoinsert = LT.GetCCDBusMessageDescription(0x7B);
                                        ImperialSlope = LT.GetCCDBusMessageSlope(0x7B)[0];
                                        ImperialOffset = LT.GetCCDBusMessageOffset(0x7B)[0];
                                        ccdvaluetoinsert = ((ccdmessage[1] * ImperialSlope) + ImperialOffset).ToString("0.0").Replace(",", ".");

                                        ccdunittoinsert = LT.GetCCDBusMessageUnitImperial(0x7B)[0];
                                    }
                                    break;
                                case 0x8C:
                                    if (ccdmessage.Length > 3)
                                    {
                                        ccddescriptiontoinsert = LT.GetCCDBusMessageDescription(0x8C);

                                        if (Units == 1) // Imperial
                                        {
                                            ImperialSlope = LT.GetCCDBusMessageSlope(0x8C)[0];
                                            ImperialOffset = LT.GetCCDBusMessageOffset(0x8C)[0];
                                            ccdvaluetoinsert = ((ccdmessage[1] * ImperialSlope) + ImperialOffset).ToString("0.0").Replace(",", ".") + " | ";
                                            ccdvaluetoinsert += ((ccdmessage[2] * ImperialSlope) + ImperialOffset).ToString("0.0").Replace(",", ".");

                                            ccdunittoinsert = LT.GetCCDBusMessageUnitImperial(0x8C)[0];
                                        }
                                        if (Units == 0) // Metric
                                        {
                                            ImperialSlope = LT.GetCCDBusMessageSlope(0x8C)[0];
                                            ImperialOffset = LT.GetCCDBusMessageOffset(0x8C)[0];
                                            MetricSlope = LT.GetCCDBusMessageSlConv(0x8C)[0];
                                            MetricOffset = LT.GetCCDBusMessageOfConv(0x8C)[0];
                                            ccdvaluetoinsert = ((((ccdmessage[1] * ImperialSlope) + ImperialOffset) * MetricSlope) + MetricOffset).ToString("0.0").Replace(",", ".") + " | ";
                                            ccdvaluetoinsert += ((((ccdmessage[2] * ImperialSlope) + ImperialOffset) * MetricSlope) + MetricOffset).ToString("0.0").Replace(",", ".");

                                            ccdunittoinsert = LT.GetCCDBusMessageUnitMetric(0x8C)[0];
                                        }
                                    }
                                    break;
                                case 0xA9:
                                    if (ccdmessage.Length > 2)
                                    {
                                        ccddescriptiontoinsert = LT.GetCCDBusMessageDescription(0xA9);
                                        ccdvaluetoinsert = ccdmessage[1].ToString("0");

                                        ccdunittoinsert = LT.GetCCDBusMessageUnitMetric(0xA9)[0];
                                    }
                                    break;
                                case 0xB2:
                                    if (ccdmessage.Length > 5)
                                    {
                                        ccddescriptiontoinsert = LT.GetCCDBusMessageDescription(0xB2);
                                        ccdvaluetoinsert = String.Empty;
                                        ccdunittoinsert = String.Empty;
                                    }
                                    else
                                    {
                                        ccddescriptiontoinsert = LT.GetCCDBusMessageDescription(0xB2);
                                        ccdvaluetoinsert = "INVALID REQUEST";
                                        ccdunittoinsert = String.Empty;
                                    }
                                    break;
                                case 0xCE:
                                    if (ccdmessage.Length > 5)
                                    {
                                        ccddescriptiontoinsert = LT.GetCCDBusMessageDescription(0xCE);

                                        if (Units == 1) // Imperial
                                        {
                                            ImperialSlope = LT.GetCCDBusMessageSlope(0xCE)[0];
                                            ccdvaluetoinsert = ((UInt32)(ccdmessage[1] << 24 | ccdmessage[2] << 16 | ccdmessage[3] << 8 | ccdmessage[4]) * ImperialSlope).ToString("0.000").Replace(",", ".");

                                            ccdunittoinsert = LT.GetCCDBusMessageUnitImperial(0xCE)[0];
                                        }
                                        if (Units == 0) // Metric
                                        {
                                            MetricSlope = (LT.GetCCDBusMessageSlope(0xCE)[0] * LT.GetCCDBusMessageSlConv(0xCE)[0]);
                                            ccdvaluetoinsert = ((UInt32)(ccdmessage[1] << 24 | ccdmessage[2] << 16 | ccdmessage[3] << 8 | ccdmessage[4]) * MetricSlope).ToString("0.000").Replace(",", ".");

                                            ccdunittoinsert = LT.GetCCDBusMessageUnitMetric(0xCE)[0];
                                        }
                                    }
                                    break;
                                case 0xD4:
                                    if (ccdmessage.Length > 3)
                                    {
                                        ccddescriptiontoinsert = LT.GetCCDBusMessageDescription(0xD4);
                                        ImperialSlope = LT.GetCCDBusMessageSlope(0xD4)[0];
                                        ccdvaluetoinsert = (ccdmessage[1] * ImperialSlope).ToString("0.0").Replace(",", ".") + " | " + (ccdmessage[2] * ImperialSlope).ToString("0.0").Replace(",", ".");

                                        ccdunittoinsert = LT.GetCCDBusMessageUnitMetric(0xD4)[0];
                                    }
                                    break;
                                case 0xDA:
                                    if (ccdmessage.Length > 2)
                                    {
                                        ccddescriptiontoinsert = LT.GetCCDBusMessageDescription(0xDA);
                                        if ((ccdmessage[1] & 0x40) == 0x40) ccdvaluetoinsert = "MIL LAMP ON";
                                        else ccdvaluetoinsert = "MIL LAMP OFF";

                                        ccdunittoinsert = LT.GetCCDBusMessageUnitMetric(0xDA)[0];
                                    }
                                    break;
                                case 0xE4:
                                    if (ccdmessage.Length > 3)
                                    {
                                        ccddescriptiontoinsert = LT.GetCCDBusMessageDescription(0xE4);

                                        if (Units == 1) // Imperial
                                        {
                                            ImperialSlope = LT.GetCCDBusMessageSlope(0xE4)[0]; // RPM
                                            ccdvaluetoinsert = (ccdmessage[1] * ImperialSlope).ToString("0") + " | ";

                                            ImperialSlope = LT.GetCCDBusMessageSlope(0xE4)[1]; // PSI
                                            ccdvaluetoinsert += (ccdmessage[2] * ImperialSlope).ToString("0.0").Replace(",", ".");

                                            ccdunittoinsert = LT.GetCCDBusMessageUnitImperial(0xE4)[0];
                                        }
                                        if (Units == 0) // Metric
                                        {
                                            ImperialSlope = LT.GetCCDBusMessageSlope(0xE4)[0]; // RPM
                                            ccdvaluetoinsert = (ccdmessage[1] * ImperialSlope).ToString("0") + " | ";

                                            ImperialSlope = LT.GetCCDBusMessageSlope(0xE4)[1]; // KPA
                                            MetricSlope = LT.GetCCDBusMessageSlConv(0xE4)[1];
                                            ccdvaluetoinsert += ((ccdmessage[2] * ImperialSlope) * MetricSlope).ToString("0.0").Replace(",", ".");

                                            ccdunittoinsert = LT.GetCCDBusMessageUnitMetric(0xE4)[0];
                                        }
                                    }
                                    break;
                                case 0xEE:
                                    if (ccdmessage.Length > 4)
                                    {
                                        ccddescriptiontoinsert = LT.GetCCDBusMessageDescription(0xEE);

                                        if (Units == 1) // Imperial
                                        {
                                            ImperialSlope = LT.GetCCDBusMessageSlope(0xEE)[0];
                                            ccdvaluetoinsert = ((UInt32)(ccdmessage[1] << 16 | ccdmessage[2] << 8 | ccdmessage[3]) * ImperialSlope).ToString("0.000").Replace(",", ".");

                                            ccdunittoinsert = LT.GetCCDBusMessageUnitImperial(0xEE)[0];
                                        }
                                        if (Units == 0) // Metric
                                        {
                                            MetricSlope = (LT.GetCCDBusMessageSlope(0xEE)[0] * LT.GetCCDBusMessageSlConv(0xEE)[0]);
                                            ccdvaluetoinsert = ((UInt32)(ccdmessage[1] << 16 | ccdmessage[2] << 8 | ccdmessage[3]) * MetricSlope).ToString("0.000").Replace(",", ".");

                                            ccdunittoinsert = LT.GetCCDBusMessageUnitMetric(0xEE)[0];
                                        }
                                    }
                                    break;
                                case 0xF2:
                                    if (ccdmessage.Length > 5)
                                    {
                                        ccddescriptiontoinsert = LT.GetCCDBusMessageDescription(0xF2);
                                        ccdvaluetoinsert = String.Empty;
                                        ccdunittoinsert = String.Empty;
                                    }
                                    else
                                    {
                                        ccddescriptiontoinsert = LT.GetCCDBusMessageDescription(0xF2);
                                        ccdvaluetoinsert = "INVALID RESPONSE";
                                        ccdunittoinsert = String.Empty;
                                    }
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

                                // Put B2 and F2 messages at the bottom of the table
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
                                File.AppendAllText(CCDB2F2LogFilename, Util.ByteToHexString(payload, 4, payload.Length) + Environment.NewLine); // save B2-messages separately
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
                                File.AppendAllText(CCDB2F2LogFilename, Util.ByteToHexString(payload, 4, payload.Length) + Environment.NewLine); // save F2-messages separately
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
                            Util.UpdateTextBox(USBTextBox, "[RX->] CCD-bus message", msg);
                            File.AppendAllText(CCDLogFilename, Util.ByteToHexString(payload, 4, payload.Length) + Environment.NewLine);
                            break;
                        case (byte)Source.SCIBusPCM:
                            SCIBusPCMEnabled = true;
                            StringBuilder scipcmlistitem = new StringBuilder(EmptyLine); // start with a pre-defined empty line
                            string scipcmmsgtoinsert = String.Empty;
                            string scipcmdescriptiontoinsert = String.Empty;
                            string scipcmvaluetoinsert = String.Empty;
                            string scipcmunittoinsert = String.Empty;
                            byte[] pcmtimestamp = new byte[4];
                            byte[] pcmmessage = new byte[payload.Length - 4];
                            Array.Copy(payload, 0, pcmtimestamp, 0, 4); // copy timestamp only
                            Array.Copy(payload, 4, pcmmessage, 0, payload.Length - 4); // copy message only

                            IDByte = pcmmessage[0];

                            // In case of high speed mode the request and response bytes are sent in separate packets.
                            // First packet is always the request bytes list, save them here and do nothing else.
                            // Second packet contains the response bytes, mix it with the request bytes and update the table.
                            if (subdatacode == 0x00) // low speed bytes
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
                                        }
                                        else
                                        {
                                            scipcmdescriptiontoinsert = "READ FLASH MEMORY";
                                            scipcmvaluetoinsert = "3 ADDR. BYTES REQUIRED";
                                            scipcmunittoinsert = String.Empty;
                                        }
                                        break;
                                    case 0x27: // Read flash memory
                                        if (pcmmessage.Length > 4)
                                        {
                                            scipcmdescriptiontoinsert = "WRITE FLASH MEMORY: ";
                                            scipcmdescriptiontoinsert += Util.ByteToHexString(pcmmessage, 1, 4);
                                            scipcmvaluetoinsert = Util.ByteToHexString(pcmmessage, 4, 5);
                                            scipcmunittoinsert = String.Empty;
                                        }
                                        else
                                        {
                                            scipcmdescriptiontoinsert = "READ FLASH MEMORY";
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
                                            scipcmdescriptiontoinsert = "ENGINE CONTROLLER REQUESTS";
                                            scipcmvaluetoinsert = "STOPPED";
                                            scipcmunittoinsert = String.Empty;
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
                                        scipcmdescriptiontoinsert = String.Empty;
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
                                File.AppendAllText(PCMLogFilename, Util.ByteToHexString(payload, 4, payload.Length) + Environment.NewLine); // save message to text file
                            }
                            if (subdatacode == 0x01) // high speed request bytes, just save them
                            {
                                SCIBusPCMReqMsgList.Clear(); // first clear previous bytes
                                for (int i = 0; i < pcmmessage.Length; i++)
                                {
                                    SCIBusPCMReqMsgList.Add(pcmmessage[i]);
                                }
                            }
                            else if (subdatacode == 0x02) // high speed response bytes, mix them with saved request bytes
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

                                if (pcmmessagemix.Length > 2)
                                {
                                    SecondaryIDByte = pcmmessagemix[2]; // Secondary ID byte applies to high speed mode only, it's actually the first RAM address that appears in the message
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
                            Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus message (PCM)", msg);
                            break;
                        case (byte)Source.SCIBusTCM:
                            IDByte = payload[4];
                            SCIBusTCMEnabled = true;
                            StringBuilder scitcmlistitem = new StringBuilder(EmptyLine); // start with a pre-defined empty line
                            string scitcmmsgtoinsert = String.Empty;
                            byte[] tcmtimestamp = new byte[4];
                            byte[] tcmmessage = new byte[payload.Length - 4];
                            Array.Copy(payload, 0, tcmtimestamp, 0, 4); // copy timestamp only
                            Array.Copy(payload, 4, tcmmessage, 0, payload.Length - 4); // copy message only

                            // In case of high speed mode the request and response bytes are sent in separate packets.
                            // First packet is always the request bytes list (subdatacode 0x01), save them here and do nothing else.
                            // Second packet contains the response bytes (subdatacode 0x02), mix it with the request bytes and update the table.
                            if (subdatacode == 0x00) // low speed bytes
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
                                File.AppendAllText(TCMLogFilename, Util.ByteToHexString(payload, 4, payload.Length) + Environment.NewLine); // save message to text file
                                // TODO: update header
                            }
                            if (subdatacode == 0x01) // high speed request bytes, just save them
                            {
                                SCIBusTCMReqMsgList.Clear(); // first clear previous bytes
                                for (int i = 0; i < tcmmessage.Length; i++)
                                {
                                    SCIBusTCMReqMsgList.Add(tcmmessage[i]);
                                }
                            }
                            else if (subdatacode == 0x02) // high speed response bytes, mix them with saved request bytes
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
                            Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus message (TCM)", msg);
                            break;
                        default:
                            Util.UpdateTextBox(USBTextBox, "[RX->] Data received", msg);
                            break;
                    }

                    if (SerialRxBuffer.ReadLength > 0) goto Here;
                    else SerialRxBuffer.Reset();
                    break;
                case 0x3E: // ">"
                    if (SerialRxBuffer.ReadLength > 1) // wait for characters to arrive
                    {
                        List<byte> raw_bytes = new List<byte>();
                        while (SerialRxBuffer.Array[SerialRxBuffer.Start] != 0x0A)
                        {
                            raw_bytes.Add(SerialRxBuffer.Pop());
                        }
                        SerialRxBuffer.Pop(); // remove newline character from the buffer
                        string line = Encoding.ASCII.GetString(raw_bytes.ToArray());
                        Util.UpdateTextBox(USBTextBox, line, null);

                        if (SerialRxBuffer.ReadLength > 0) goto Here;
                        else SerialRxBuffer.Reset();
                    }
                    break;
                default:
                    break;
            }
        }

        private void UpdateDiagnosticsListBox()
        {
            if (DiagnosticsListBox.InvokeRequired)
            {
                DiagnosticsListBox.BeginInvoke((MethodInvoker)delegate
                {
                    DiagnosticsListBox.SuspendLayout();
                    DiagnosticsListBox.BeginUpdate();
                    int savedVpos = GetScrollPos((IntPtr)DiagnosticsListBox.Handle, SB_VERT);
                    DiagnosticsListBox.Items.Clear();
                    DiagnosticsListBox.Items.AddRange(DiagnosticsTable.ToArray());
                    DiagnosticsListBox.Update();
                    DiagnosticsListBox.Refresh();
                    SetScrollPos((IntPtr)DiagnosticsListBox.Handle, SB_VERT, savedVpos, true);
                    PostMessageA((IntPtr)DiagnosticsListBox.Handle, WM_VSCROLL, SB_THUMBPOSITION + 0x10000 * savedVpos, 0);
                    DiagnosticsListBox.EndUpdate();
                    DiagnosticsListBox.ResumeLayout();
                });
            }
            else
            {
                DiagnosticsListBox.SuspendLayout();
                DiagnosticsListBox.BeginUpdate();
                int savedVpos = GetScrollPos((IntPtr)DiagnosticsListBox.Handle, SB_VERT);
                DiagnosticsListBox.Items.Clear();
                DiagnosticsListBox.Items.AddRange(DiagnosticsTable.ToArray());
                DiagnosticsListBox.Update();
                DiagnosticsListBox.Refresh();
                SetScrollPos((IntPtr)DiagnosticsListBox.Handle, SB_VERT, savedVpos, true);
                PostMessageA((IntPtr)DiagnosticsListBox.Handle, WM_VSCROLL, SB_THUMBPOSITION + 0x10000 * savedVpos, 0);
                DiagnosticsListBox.EndUpdate();
                DiagnosticsListBox.ResumeLayout();
            }
        }

        private void ResetViewButton_Click(object sender, EventArgs e)
        {
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
            UpdateDiagnosticsListBox();

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
                        byte[] bytes = Util.HexStringToByte(USBSendComboBox.Text);
                        if ((bytes.Length > 5) && (bytes != null))
                        {
                            Util.UpdateTextBox(USBTextBox, "[<-TX] Data transmitted", bytes);
                            SerialDataWriteAsync(bytes);

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
                        List<byte> raw_bytes = new List<byte>();
                        raw_bytes.AddRange(Encoding.ASCII.GetBytes(USBSendComboBox.Text));
                        raw_bytes.Add(0x0A); // add newline character at the end manually
                        byte[] bytes = raw_bytes.ToArray();
                        if ((bytes.Length > 1) && (bytes != null))
                        {
                            //Util.UpdateTextBox(USBTextBox, USBSendComboBox.Text, null);
                            SerialDataWriteAsync(bytes);
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

        private void UnitsRadioButtons_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (MetricUnitRadioButton.Checked && !ImperialUnitRadioButton.Checked)
                {
                    Properties.Settings.Default["Units"] = "metric";
                    Properties.Settings.Default.Save(); // Save settings in application configuration file
                    Units = 0;
                }

                else if (!MetricUnitRadioButton.Checked && ImperialUnitRadioButton.Checked)
                {
                    Properties.Settings.Default["Units"] = "imperial";
                    Properties.Settings.Default.Save(); // Save settings in application configuration file
                    Units = 1;
                }
            }
            catch
            {

            }
        }

        private void UpdateUSBSendComboBox()
        {
            byte[] USBSendComboBoxValue = new byte[] { 0x00 };

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

                                    PacketBytes.Clear();
                                    PacketBytes.AddRange(new byte[] { 0x3D, 0x00, 0x06, 0x03, 0x00, HeartbeatIntervalHB, HeartbeatIntervalLB, HeartbeatDurationHB, HeartbeatDurationLB });

                                    PacketBytesChecksum = 0;
                                    for (byte i = 1; i < PacketBytes.Count; i++)
                                    {
                                        PacketBytesChecksum += PacketBytes[i];
                                    }

                                    PacketBytes.Add(PacketBytesChecksum);
                                    USBSendComboBoxValue = PacketBytes.ToArray();
                                    break;
                                case 1: // Set CCD-bus
                                    PacketBytes.Clear();
                                    PacketBytes.AddRange(new byte[] { 0x3D, 0x00, 0x03, 0x03, 0x01 });
                                    if (SetCCDBus) PacketBytes.AddRange(new byte[] { 0x01 });
                                    else PacketBytes.AddRange(new byte[] { 0x00 });

                                    PacketBytesChecksum = 0;
                                    for (byte i = 1; i < 6; i++)
                                    {
                                        PacketBytesChecksum += PacketBytes[i];
                                    }
                                    PacketBytes.Add(PacketBytesChecksum);
                                    USBSendComboBoxValue = PacketBytes.ToArray();
                                    break;
                                case 2: // Set SCI-bus
                                    PacketBytes.Clear();
                                    PacketBytes.AddRange(new byte[] { 0x3D, 0x00, 0x03, 0x03, 0x02, SetSCIBus });

                                    PacketBytesChecksum = 0;
                                    for (byte i = 1; i < 6; i++)
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
                                    USBSendComboBoxValue = new byte[] { 0x3D, 0x00, 0x02, 0x04, 0x00, 0x06 };
                                    break;
                                case 1: // Timestamp
                                    USBSendComboBoxValue = new byte[] { 0x3D, 0x00, 0x02, 0x04, 0x01, 0x07 };
                                    break;
                                case 2: // Battery voltage
                                    USBSendComboBoxValue = new byte[] { 0x3D, 0x00, 0x02, 0x04, 0x02, 0x08 };
                                    break;
                                case 3: // External EEPROM checksum
                                    USBSendComboBoxValue = new byte[] { 0x3D, 0x00, 0x02, 0x04, 0x03, 0x09 };
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case 5: // Debug
                            switch (ModeComboBox.SelectedIndex)
                            {
                                case 0: // Generate random CCD-bus messages
                                    PacketBytes.Clear();
                                    PacketBytes.AddRange(new byte[] { 0x3D, 0x00, 0x07, 0x0E, 0x01 });

                                    if (Param3ComboBox.SelectedIndex == 0) PacketBytes.AddRange(new byte[] { 0x00 }); // false
                                    else PacketBytes.AddRange(new byte[] { 0x01 }); // true

                                    byte MinIntervalHB = (byte)((RandomCCDMessageIntervalMin >> 8) & 0xFF);
                                    byte MinIntervalLB = (byte)(RandomCCDMessageIntervalMin & 0xFF);
                                    byte MaxIntervalHB = (byte)((RandomCCDMessageIntervalMax >> 8) & 0xFF);
                                    byte MaxIntervalLB = (byte)(RandomCCDMessageIntervalMax & 0xFF);

                                    PacketBytes.AddRange(new byte[] { MinIntervalHB, MinIntervalLB, MaxIntervalHB, MaxIntervalLB });

                                    PacketBytesChecksum = 0;
                                    for (byte i = 1; i < 10; i++)
                                    {
                                        PacketBytesChecksum += PacketBytes[i];
                                    }
                                    PacketBytes.Add(PacketBytesChecksum);
                                    USBSendComboBoxValue = PacketBytes.ToArray();
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
                                    break;
                                case 1: // Single message
                                    PacketBytes.Clear();
                                    int FullPacketLength = 6 + CCDBusMessageToSend.Length; // including sync and checksum
                                    byte PacketLengthHB = (byte)((2 + CCDBusMessageToSend.Length) >> 8);
                                    byte PacketLengthLB = (byte)((2 + CCDBusMessageToSend.Length) & 0xFF);

                                    PacketBytes.AddRange(new byte[] { 0x3D, PacketLengthHB, PacketLengthLB, 0x16, 0x01 });
                                    PacketBytes.AddRange(CCDBusMessageToSend);

                                    byte PacketBytesChecksum = 0;
                                    for (byte i = 1; i < FullPacketLength - 1; i++)
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
                                    PacketBytes.Clear();
                                    int FullPacketLength = 6 + SCIBusPCMMessageToSend.Length; // including sync and checksum
                                    byte PacketLengthHB = (byte)((2 + SCIBusPCMMessageToSend.Length) >> 8);
                                    byte PacketLengthLB = (byte)((2 + SCIBusPCMMessageToSend.Length) & 0xFF);

                                    PacketBytes.AddRange(new byte[] { 0x3D, PacketLengthHB, PacketLengthLB, 0x26, 0x01 });
                                    PacketBytes.AddRange(SCIBusPCMMessageToSend);

                                    byte PacketBytesChecksum = 0;
                                    for (byte i = 1; i < FullPacketLength - 1; i++)
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
                                    PacketBytes.Clear();
                                    int FullPacketLength = 6 + SCIBusTCMMessageToSend.Length; // including sync and checksum
                                    byte PacketLengthHB = (byte)((2 + SCIBusTCMMessageToSend.Length) >> 8);
                                    byte PacketLengthLB = (byte)((2 + SCIBusTCMMessageToSend.Length) & 0xFF);

                                    PacketBytes.AddRange(new byte[] { 0x3D, PacketLengthHB, PacketLengthLB, 0x36, 0x01 });
                                    PacketBytes.AddRange(SCIBusTCMMessageToSend);

                                    byte PacketBytesChecksum = 0;
                                    for (byte i = 1; i < FullPacketLength - 1; i++)
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
                    Param1ComboBox.Text = Util.ByteToHexStringSimple(CCDBusMessageToSend); // Load last valid message
                    Param2Label1.Visible = false;
                    Param2Label2.Visible = false;
                    Param2ComboBox.Visible = false;
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
                    Param1ComboBox.Text = Util.ByteToHexStringSimple(SCIBusPCMMessageToSend); // Load last valid message
                    Param2Label1.Visible = false;
                    Param2Label2.Visible = false;
                    Param2ComboBox.Visible = false;
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
                    Param1ComboBox.Text = Util.ByteToHexStringSimple(SCIBusTCMMessageToSend); // Load last valid message
                    Param2Label1.Visible = false;
                    Param2Label2.Visible = false;
                    Param2ComboBox.Visible = false;
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
                            ModeComboBox.Items.AddRange(new string[] { "Heartbeat", "Set CCD-bus", "Set SCI-bus" });
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
                            ModeComboBox.Items.AddRange(new string[] { "Generate random CCD-bus messages" });
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
                                    Param1ComboBox.Font = new Font("Courier New", 9F);
                                    Param1ComboBox.DropDownStyle = ComboBoxStyle.DropDown;
                                    Param1ComboBox.Items.Clear();
                                    Param2Label1.Visible = true;
                                    Param2Label1.Text = "Duration:";
                                    Param2Label2.Visible = true;
                                    Param2Label2.Text = "millisecond";
                                    Param2ComboBox.Visible = true;
                                    Param2ComboBox.Font = new Font("Courier New", 9F);
                                    Param2ComboBox.DropDownStyle = ComboBoxStyle.DropDown;
                                    Param2ComboBox.Items.Clear();
                                    Param3Label1.Visible = false;
                                    Param3Label2.Visible = false;
                                    Param3ComboBox.Visible = false;
                                    Param1ComboBox.Text = HeartbeatInterval.ToString();
                                    Param2ComboBox.Text = HeartbeatDuration.ToString();
                                    HintTextBox.Text = Environment.NewLine 
                                                     + "Set LED behavior in the scanner:" + Environment.NewLine 
                                                     + "- interval: blinking period of the blue ACT LED only," + Environment.NewLine 
                                                     + "- duration: maximum on-time of any LEDs.";
                                    break;
                                case 1: // Set CCD-bus
                                    Param1Label1.Visible = true;
                                    Param1Label1.Text = "State:";
                                    Param1Label2.Visible = true;
                                    Param1Label2.Text = "";
                                    Param1ComboBox.Visible = true;
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
                                                     + "Turn on/off the CCD-bus transceiver chip";
                                    break;
                                case 2: // Set SCI-bus
                                    Param1Label1.Visible = true;
                                    Param1Label1.Text = "Module:";
                                    Param1Label2.Visible = true;
                                    Param1Label2.Text = "";
                                    Param1ComboBox.Visible = true;
                                    Param1ComboBox.Font = new Font("Microsoft Sans Serif", 8.25F);
                                    Param1ComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                                    Param1ComboBox.Items.Clear();
                                    Param1ComboBox.Items.AddRange(new string[] { "Engine (PCM)", "Transmission (TCM)" });
                                    Param1ComboBox.SelectedIndex = 0;
                                    Param2Label1.Visible = true;
                                    Param2Label1.Text = "Speed:";
                                    Param2Label2.Visible = true;
                                    Param2Label2.Text = "";
                                    Param2ComboBox.Visible = true;
                                    Param2ComboBox.Font = new Font("Microsoft Sans Serif", 8.25F);
                                    Param2ComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                                    Param2ComboBox.Items.Clear();
                                    Param2ComboBox.Items.AddRange(new string[] { "Disable", "Low (7812.5 baud)", "High (62500 baud)" });
                                    Param2ComboBox.SelectedIndex = 1;
                                    Param3Label1.Visible = true;
                                    Param3Label1.Text = "Config.:";
                                    Param3Label2.Visible = true;
                                    Param3Label2.Text = "";
                                    Param3ComboBox.Visible = true;
                                    Param3ComboBox.Font = new Font("Microsoft Sans Serif", 8.25F);
                                    Param3ComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                                    Param3ComboBox.Items.Clear();
                                    Param3ComboBox.Items.AddRange(new string[] { "A", "B" });
                                    Param3ComboBox.SelectedIndex = 0;
                                    HintTextBox.Text = "Set SCI-bus transceiver circuits" + Environment.NewLine
                                                     + "- module: engine (PCM) or transmission (TCM)" + Environment.NewLine
                                                     + "- speed: communication speed" + Environment.NewLine
                                                     + "- config: OBD-II pin routing specific to the vehicle";
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
                                    Param1ComboBox.Font = new Font("Courier New", 9F);
                                    Param1ComboBox.DropDownStyle = ComboBoxStyle.DropDown;
                                    Param1ComboBox.Items.Clear();
                                    Param1ComboBox.Text = RandomCCDMessageIntervalMin.ToString();
                                    Param2Label1.Visible = true;
                                    Param2Label1.Text = "Max:";
                                    Param2Label2.Visible = true;
                                    Param2Label2.Text = "millisecond";
                                    Param2ComboBox.Visible = true;
                                    Param2ComboBox.Font = new Font("Courier New", 9F);
                                    Param2ComboBox.DropDownStyle = ComboBoxStyle.DropDown;
                                    Param2ComboBox.Items.Clear();
                                    Param2ComboBox.Text = RandomCCDMessageIntervalMax.ToString();
                                    Param3Label1.Visible = true;
                                    Param3Label1.Text = "Enable:";
                                    Param3Label2.Visible = false;
                                    Param3Label2.Text = "";
                                    Param3ComboBox.Visible = true;
                                    Param3ComboBox.Font = new Font("Microsoft Sans Serif", 8.25F);
                                    Param3ComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                                    Param3ComboBox.Items.Clear();
                                    Param3ComboBox.Items.AddRange(new string[] { "false", "true" });
                                    Param3ComboBox.SelectedIndex = 1;
                                    //Param1ComboBox.Text = "20";
                                    //Param2ComboBox.Text = "100";
                                    HintTextBox.Text = "Broadcast random CCD-bus messages" + Environment.NewLine
                                                     + "at random times defined by min / max intervals." + Environment.NewLine
                                                     + "Do this only when the scanner is not connected" + Environment.NewLine
                                                     + "to the car and connect both standalone bias jumpers!";
                                    break;
                            }

                            //HintTextBox.Text = Environment.NewLine
                            //            + Environment.NewLine
                            //            + "Experimental commands are first tested here" + Environment.NewLine
                            //            + "before they are moved to their right place.";
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
                                    HintTextBox.Text = Environment.NewLine
                                                     + Environment.NewLine
                                                     + Environment.NewLine
                                                     + "Stop repeated message transmission.";
                                    break;
                                case 1: // Single message
                                    HintTextBox.Text = Environment.NewLine
                                                     + Environment.NewLine
                                                     + Environment.NewLine
                                                     + "Send a single message to the CCD-bus.";
                                    break;
                                case 2: // Repeated message(s)
                                    HintTextBox.Text = Environment.NewLine
                                                     + Environment.NewLine
                                                     + Environment.NewLine
                                                     + "Send a message or multiple messages to the CCD-bus.";
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
                                    HintTextBox.Text = Environment.NewLine
                                                     + Environment.NewLine
                                                     + Environment.NewLine
                                                     + "Stop repeated message transmission.";
                                    break;
                                case 1: // Single message
                                    HintTextBox.Text = Environment.NewLine
                                                     + Environment.NewLine
                                                     + Environment.NewLine
                                                     + "Send a single message to the SCI-bus (PCM).";
                                    break;
                                case 2: // Repeated message(s)
                                    HintTextBox.Text = Environment.NewLine
                                                     + Environment.NewLine
                                                     + Environment.NewLine
                                                     + "Send a message or multiple messages to the SCI-bus (PCM).";
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
                                    HintTextBox.Text = Environment.NewLine
                                                     + Environment.NewLine
                                                     + Environment.NewLine
                                                     + "Stop repeated message transmission.";
                                    break;
                                case 1: // Single message
                                    HintTextBox.Text = Environment.NewLine
                                                     + Environment.NewLine
                                                     + Environment.NewLine
                                                     + "Send a single message to the SCI-bus (TCM).";
                                    break;
                                case 2: // Repeated message(s)
                                    HintTextBox.Text = Environment.NewLine
                                                     + Environment.NewLine
                                                     + Environment.NewLine
                                                     + "Send a message or multiple messages to the SCI-bus (TCM).";
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
                                    CCDBusMessageToSend = Util.HexStringToByte(Param1ComboBox.Text);
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
                                    SCIBusPCMMessageToSend = Util.HexStringToByte(Param1ComboBox.Text);
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
                                    SCIBusTCMMessageToSend = Util.HexStringToByte(Param1ComboBox.Text);
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
                Util.UpdateTextBox(USBTextBox, "[INFO] Searching for new scanner firmware" + Environment.NewLine 
                                             + "       This may take a few seconds...", null);

                // Download latest main.h file from GitHub
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
                Downloader.DownloadFile(SourceFile, @"Tools/main.h");
                
                // Get new UNIX time value from the downloaded file
                string line = String.Empty;
                bool done = false;
                using (Stream stream = File.Open(@"Tools/main.h", FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        while(!done)
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
                    process.StartInfo.Arguments = "-C avrdude.conf -v -p atmega2560 -c wiring -P " + UpdatePort + " -b 115200 -D -U flash:w:ChryslerCCDSCIScanner.ino.mega.hex:i";
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
    }
}