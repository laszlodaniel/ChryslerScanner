using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Net;
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
        public static string CCDLogFilename;
        public static string PCMLogFilename;
        public static string TCMLogFilename;
        public static string UpdateScannerFirmwareLogFilename;
        public static bool USBShowTraffic = true;
        public static byte Units = 0;
        public bool Timeout = false;
        public bool ScannerFound = false;
        public bool IncludeTimestap = false;
        public byte[] buffer = new byte[2048];
        public int HeartbeatInterval = 5000;
        public int HeartbeatDuration = 50;
        public bool SetCCDBus = true;
        public byte SetSCIBus = 2;
        public byte[] CCDBusMessageToSend = new byte[] { 0xB2, 0x20, 0x22, 0x00, 0x00, 0xF4 };
        public byte[] SCIBusPCMMessageToSend = new byte[] { 0x10 };
        public byte[] SCIBusTCMMessageToSend = new byte[] { 0x10 };
        public List<byte> PacketBytes = new List<byte>();
        public byte PacketBytesChecksum = 0;

        public static string UpdatePort = String.Empty;
        public static Int64 OldUNIXTime = 0;
        public static Int64 NewUNIXTime = 0;


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

        SerialPort Serial = new SerialPort();
        PacketManager PM = new PacketManager();
        CircularBuffer<byte> SerialRxBuffer = new CircularBuffer<byte>(2048);
        TextTable TT = new TextTable(@"VehicleProfiles.xml");
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

            // Set logfile names inside the LOG directory
            DateTimeNow = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            USBLogFilename = @"LOG/usblog_" + DateTimeNow + ".txt";
            USBBinaryLogFilename = @"LOG/usblog_" + DateTimeNow + ".bin";
            CCDLogFilename = @"LOG/ccdlog_" + DateTimeNow + ".txt";
            PCMLogFilename = @"LOG/pcmlog_" + DateTimeNow + ".txt";
            TCMLogFilename = @"LOG/tcmlog_" + DateTimeNow + ".txt";

            // Setup timeout timer
            TimeoutTimer.Elapsed += new ElapsedEventHandler(TimeoutHandler);
            TimeoutTimer.Interval = 500; // ms
            TimeoutTimer.Enabled = false;

            // Associate event handler methods to specific events
            PM.PropertyChanged += new PropertyChangedEventHandler(DataReceived); // packet manager
            TT.PropertyChanged += new PropertyChangedEventHandler(TableUpdated); // text table

            DiagnosticsTextBox.Text = String.Join(Environment.NewLine, TT.Table);
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
                Util.UpdateTextBox(USBTextBox, "[INFO] Application config file is missing (ChryslerCCDSCIScanner.exe.config)", null);
                Units = 0; // Metric units by default
            }

            if (!File.Exists("VehicleProfiles.xml"))
            {
                Util.UpdateTextBox(USBTextBox, "[INFO] Vehicle profiles file is missing (VehicleProfiles.xml)", null);
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
                byte[] HandshakeHwFwRequest = new byte[] { 0x3D, 0x00, 0x02, 0x01, 0x01, 0x04 };
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
                    Util.UpdateTextBox(USBTextBox, "[<-TX] Handshake + HW/FW info request (" + Serial.PortName + ")", HandshakeHwFwRequest);
                    SerialDataWriteAsync(HandshakeHwFwRequest);

                    Timeout = false; // for the connection procedure we have to manually read response bytes here
                    TimeoutTimer.Enabled = true; // start counting to the set timeout value
                    while ((Serial.BytesToRead < 27) && !Timeout) ; // wait for expected bytes to arrive
                    TimeoutTimer.Enabled = false; // disable timer
                    if (Timeout)
                    {
                        if (Serial.BytesToRead > 0) Util.UpdateTextBox(USBTextBox, "[INFO] " + Serial.PortName + " answer is invalid (" + Serial.BytesToRead + " bytes)", null);
                        else Util.UpdateTextBox(USBTextBox, "[INFO] " + Serial.PortName + " is not answering", null);
                        continue; // skip current iteration and jump to next comport (if any)
                    }

                    byte[] msg = new byte[27]; // handshake response must be 27 bytes long
                    Serial.Read(msg, 0, 27); // read exactly that much bytes from the serial input buffer

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
                    }
                }
                Util.UpdateTextBox(USBTextBox, "[INFO] No scanner available", null);
                ConnectButton.Enabled = true;
            }
            else // disconnect
            {
                if (Serial.IsOpen && ScannerFound)
                {
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

        // This method gets called everytime when something arrives on the COM-port
        // It searches for valid packets (even if there are multiple packets in one reception) and discards garbage bytes
        private void DataReceived(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "DataReceived")
            {
                int PacketSize = 0;

                // Find the first byte with value 0x3D
                Here: // This is a goto label, never mind now, might want to get rid of it with another while-loop
                while ((SerialRxBuffer.Array[SerialRxBuffer.Start] != 0x3D) && (SerialRxBuffer.ReadLength > 0))
                {
                    SerialRxBuffer.Pop();
                }

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
                            case (byte)Command.Settings:
                                switch (subdatacode)
                                {
                                    case 0x00: // Heartbeat
                                        Util.UpdateTextBox(USBTextBox, "[RX->] Heartbeat settings changed" , msg);
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
                                        if (payload[0] == 0) Util.UpdateTextBox(USBTextBox, "[INFO] CCD-bus chip disabled", null);
                                        else if (payload[0] == 1) Util.UpdateTextBox(USBTextBox, "[INFO] CCD-bus chip enabled @ 7812.5 baud", null);
                                        break;
                                    case 0x02: // Set SCI-bus
                                        string configuration = String.Empty;

                                        if (((payload[0] >> 2) & 0x01) == 0) configuration += "SCI-BUS_A";
                                        else if (((payload[0] >> 2) & 0x01) == 1) configuration += "SCI-BUS_B";

                                        if ((payload[0] & 0x01) == 0) configuration += "_PCM";
                                        else if ((payload[0] & 0x01) == 1) configuration += "_TCM";

                                        if (((payload[0] >> 1) & 0x01) == 0) configuration += " disabled";
                                        else if (((payload[0] >> 1) & 0x01) == 1)
                                        {
                                            if (((payload[0] >> 3) & 0x01) == 0) configuration += " enabled @ 7812.5 baud";
                                            else if (((payload[0] >> 3) & 0x01) == 1) configuration += " enabled @ 62500 baud";
                                        }

                                        Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus settings changed", msg);
                                        Util.UpdateTextBox(USBTextBox, "[INFO] " + configuration, null);
                                        break;
                                }
                                break;
                            case (byte)Command.Response:
                                switch (subdatacode)
                                {
                                    case (byte)Response.HwFwInfo:
                                        string HardwareVersionString = "V" + ((payload[0] << 8 | payload[1]) / 100.00).ToString("0.00").Replace(",", ".");
                                        DateTime HardwareDate = Util.UnixTimeStampToDateTime(payload[2] << 56 | payload[3] << 48 | payload[4] << 40 | payload[5] << 32 | payload[6] << 24 | payload[7] << 16 | payload[8] << 8 | payload[9]);
                                        DateTime AssemblyDate = Util.UnixTimeStampToDateTime(payload[10] << 56 | payload[11] << 48 | payload[12] << 40 | payload[13] << 32 | payload[14] << 24 | payload[15] << 16 | payload[16] << 8 | payload[17]);
                                        DateTime FirmwareDate = Util.UnixTimeStampToDateTime(payload[18] << 56 | payload[19] << 48 | payload[20] << 40 | payload[21] << 32 | payload[22] << 24 | payload[23] << 16 | payload[24] << 8 | payload[25]);
                                        string HardwareDateString = HardwareDate.ToString("yyyy.MM.dd HH:mm:ss");
                                        string AssemblyDateString = AssemblyDate.ToString("yyyy.MM.dd HH:mm:ss");
                                        string FirmwareDateString = FirmwareDate.ToString("yyyy.MM.dd HH:mm:ss");
                                        Util.UpdateTextBox(USBTextBox, "[RX->] Hardware/Firmware information response", msg);
                                        Util.UpdateTextBox(USBTextBox, "[INFO] Hardware ver.: " + HardwareVersionString + Environment.NewLine + 
                                                                       "       Hardware date: " + HardwareDateString + Environment.NewLine + 
                                                                       "       Assembly date: " + AssemblyDateString + Environment.NewLine + 
                                                                       "       Firmware date: " + FirmwareDateString, null);
                                        OldUNIXTime = payload[18] << 56 | payload[19] << 48 | payload[20] << 40 | payload[21] << 32 | payload[22] << 24 | payload[23] << 16 | payload[24] << 8 | payload[25];
                                        break;
                                    case (byte)Response.Timestamp:
                                        TimeSpan ElapsedTime = TimeSpan.FromMilliseconds(payload[0] << 24 | payload[1] << 16 | payload[2] << 8 | payload[3]);
                                        DateTime Timestamp = DateTime.Today.Add(ElapsedTime);
                                        string TimestampString = Timestamp.ToString("HH:mm:ss.fff");
                                        Util.UpdateTextBox(USBTextBox, "[RX->] Timestamp response", msg);
                                        Util.UpdateTextBox(USBTextBox, "[INFO] Timestamp: " + TimestampString, null);
                                        break;
                                    case (byte)Response.BatteryVoltage:
                                        string BatteryVoltageString = ((payload[0] << 8 | payload[1]) / 100.00).ToString("0.00").Replace(",", ".") + " V";
                                        Util.UpdateTextBox(USBTextBox, "[RX->] Battery voltage response", msg);
                                        Util.UpdateTextBox(USBTextBox, "[INFO] Battery voltage: " + BatteryVoltageString, null);
                                        break;
                                    case (byte)Response.ExternalEEPROMChecksum:
                                        if (payload[0] == 0x00) // OK
                                        {
                                            string ExternalEEPROMChecksumString = Util.ByteToHexString(payload, 1, payload.Length);
                                            Util.UpdateTextBox(USBTextBox, "[RX->] External EEPROM checksum response", msg);
                                            Util.UpdateTextBox(USBTextBox, "[INFO] External EEPROM checksum OK: " + ExternalEEPROMChecksumString, null);
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
                                    case 0xFA:
                                        Util.UpdateTextBox(USBTextBox, "[RX->] Error: external EEPROM not found", msg);
                                        break;
                                    case 0xFB:
                                        Util.UpdateTextBox(USBTextBox, "[RX->] Error: external EEPROM checksum wrong", msg);
                                        Util.UpdateTextBox(USBTextBox, "[INFO] External EEPROM checksum: " + Environment.NewLine + "       - calculated: " + Util.ByteToHexString(payload, 1, 2) + Environment.NewLine + "       - reads as: " + Util.ByteToHexString(payload, 0, 1), null);
                                        break;
                                    case 0xFC:
                                        Util.UpdateTextBox(USBTextBox, "[RX->] Error: external EEPROM read not possible", msg);
                                        break;
                                    case 0xFD:
                                        Util.UpdateTextBox(USBTextBox, "[RX->] Error: external EEPROM write not possible", msg);
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
                        break;
                    case (byte)Source.CCDBus:
                        Util.UpdateTextBox(USBTextBox, "[RX->] CCD-bus message", msg);
                        TT.UpdateTextTable(source, payload, 4, payload.Length);
                        if (IncludeTimestap) File.AppendAllText(CCDLogFilename, Util.ByteToHexString(payload, 0, payload.Length) + Environment.NewLine);
                        else File.AppendAllText(CCDLogFilename, Util.ByteToHexString(payload, 4, payload.Length) + Environment.NewLine);
                        break;
                    case (byte)Source.SCIBusPCM:
                        Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus message (PCM)", msg);
                        TT.UpdateTextTable(source, payload, 4, payload.Length);
                        if (IncludeTimestap) File.AppendAllText(PCMLogFilename, Util.ByteToHexString(payload, 0, payload.Length) + Environment.NewLine);
                        else File.AppendAllText(PCMLogFilename, Util.ByteToHexString(payload, 4, payload.Length) + Environment.NewLine);
                        break;
                    case (byte)Source.SCIBusTCM:
                        Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus message (TCM)", msg);
                        TT.UpdateTextTable(source, payload, 4, payload.Length);
                        if (IncludeTimestap) File.AppendAllText(TCMLogFilename, Util.ByteToHexString(payload, 0, payload.Length) + Environment.NewLine);
                        else File.AppendAllText(TCMLogFilename, Util.ByteToHexString(payload, 4, payload.Length) + Environment.NewLine);
                        break;
                    default:
                        Util.UpdateTextBox(USBTextBox, "[RX->] Data received", msg);
                        break;
                }
                if (SerialRxBuffer.ReadLength > 0) goto Here;
                else SerialRxBuffer.Reset(); // said unsafety is handled by resetting the ringbuffer whenever it's empty so the head and tail variable points to zero
            }
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
                        }
                    }
                    else if (AsciiCommMethodRadioButton.Checked)
                    {
                        Util.UpdateTextBox(USBTextBox, USBSendComboBox.Text, null);
                    }
                }
            }
        }

        private void TableUpdated(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "TableUpdated")
            {
                DiagnosticsTextBox.BeginInvoke((MethodInvoker)delegate
                {
                    DiagnosticsTextBox.SuspendLayout();
                    int savedVpos = GetScrollPos(DiagnosticsTextBox.Handle, SB_VERT);
                    DiagnosticsTextBox.Text = String.Join(Environment.NewLine, TT.Table);
                    SetScrollPos(DiagnosticsTextBox.Handle, SB_VERT, savedVpos, true);
                    PostMessageA(DiagnosticsTextBox.Handle, WM_VSCROLL, SB_THUMBPOSITION + 0x10000 * savedVpos, 0);
                    Application.DoEvents();
                    DiagnosticsTextBox.ResumeLayout();
                });
            }
        }

        private async Task SerialDataReadAsyncTask()
        {
            while (ScannerFound)
            {
                Task<int> readByteTask = Serial.BaseStream.ReadAsync(buffer, 0, buffer.Length);
                int bytesRead = await readByteTask;
                SerialRxBuffer.Append(buffer, 0, bytesRead);
                PM.DataReceived = true; // fire datareceived event and analyze received data
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
                USBSendComboBox.Text = String.Empty;
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
                            USBSendComboBoxValue = new byte[] { 0x3D, 0x00, 0x02, 0x0E, 0x00, 0x10 };
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
                                    Param1ComboBox.Font = new Font("Courier New", 9F);
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
                            HintTextBox.Text = Environment.NewLine
                                             + Environment.NewLine
                                             + "Experimental commands are first tested here" + Environment.NewLine
                                             + "before they are moved to their right place.";
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
                                        byte pcm_tcm = 0;
                                        byte enable = 0;
                                        byte bus_config = 0;
                                        byte bus_speed = 0;

                                        if (Param1ComboBox.SelectedIndex == 0) pcm_tcm = 0; // PCM
                                        else if (Param1ComboBox.SelectedIndex == 1) pcm_tcm = 1; // TCM

                                        if (Param2ComboBox.SelectedIndex == 0)
                                        {
                                            enable = 0;
                                            bus_speed = 0; // low speed but it's ignored
                                        }
                                        else if (Param2ComboBox.SelectedIndex == 1)
                                        {
                                            enable = 1;
                                            bus_speed = 0; // low speed
                                        }
                                        else if (Param2ComboBox.SelectedIndex == 2)
                                        {
                                            enable = 1;
                                            bus_speed = 1; // high speed
                                        }

                                        if (Param3ComboBox.SelectedIndex == 0) bus_config = 0; // "A" configuration
                                        else if (Param3ComboBox.SelectedIndex == 1) bus_config = 1; // "B" configuration

                                        SetSCIBus = (byte)((bus_speed << 3) | (bus_config << 2) | (enable << 1) | pcm_tcm);
                                    }
                                    catch
                                    {

                                    }
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

        private void UpdateScannerFirmwareToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //if (true)
            if ((UpdatePort != String.Empty) && (OldUNIXTime != 0) && ScannerFound)
            {
                Util.UpdateTextBox(USBTextBox, "[INFO] Searching for new scanner firmware" + Environment.NewLine + "       This may take a few seconds...", null);

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
                NewUNIXTime = Convert.ToInt64(hexline, 16);

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
    }
}