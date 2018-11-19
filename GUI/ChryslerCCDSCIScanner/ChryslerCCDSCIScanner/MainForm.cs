using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
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
        public static string CCDLogFilename;
        public static string PCMLogFilename;
        public static string TCMLogFilename;
        public static bool USBShowTraffic = true;
        public bool Timeout = false;
        public bool ScannerFound = false;
        public bool IncludeTimestap = false;
        public byte[] buffer = new byte[2048];
        public byte[] HandshakeRequest = new byte[] { 0x3D, 0x00, 0x02, 0x01, 0x00, 0x03 };
        public byte[] MillisRequest = new byte[] { 0x3D, 0x00, 0x02, 0x04, 0x01, 0x07 };

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
            FirmwareDate = 0,
            Timestamp = 1,
            BatteryVoltage = 2
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
        WebClient Client = new WebClient();

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            DiagnosticsGroupBox.Visible = false;
            this.Size = new Size(405, 650);
            this.CenterToScreen();

            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); // Set application icon
            if (!Directory.Exists("LOG")) Directory.CreateDirectory("LOG"); // Create LOG directory if it doesn't exist

            // Set logfile names inside the LOG directory
            DateTimeNow = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            USBLogFilename = @"LOG/usblog_" + DateTimeNow + ".txt";
            CCDLogFilename = @"LOG/ccdlog_" + DateTimeNow + ".txt";
            PCMLogFilename = @"LOG/pcmlog_" + DateTimeNow + ".txt";
            TCMLogFilename = @"LOG/tcmlog_" + DateTimeNow + ".txt";

            TimeoutTimer.Elapsed += new ElapsedEventHandler(TimeoutHandler);
            TimeoutTimer.Interval = 500; // ms
            TimeoutTimer.Enabled = false;

            PM.PropertyChanged += new PropertyChangedEventHandler(DataReceived);
            TT.PropertyChanged += new PropertyChangedEventHandler(TableUpdated);

            DiagnosticsTextBox.Text = String.Join(Environment.NewLine, TT.Table);
            USBCommunicationGroupBox.Enabled = false;
            TargetComboBox.SelectedIndex = 0;
            ModeComboBox.SelectedIndex = 1;
            CommandComboBox.SelectedIndex = 0;

            try // loading saved settings
            {
                if ((string)Properties.Settings.Default["Units"] == "metric") MetricUnitRadioButton.Checked = true;
                else if ((string)Properties.Settings.Default["Units"] == "imperial") ImperialUnitRadioButton.Checked = true;
                if ((string)Properties.Settings.Default["TransmissionMethod"] == "hex") HexCommMethodRadioButton.Checked = true;
                else if ((string)Properties.Settings.Default["TransmissionMethod"] == "ascii") AsciiCommMethodRadioButton.Checked = true;
            }
            catch
            {
                Util.UpdateTextBox(USBTextBox, "[INFO] Application config file is missing (ChryslerCCDSCIScanner.exe.config)", null);
            }

            if (!File.Exists("VehicleProfiles.xml"))
            {
                Util.UpdateTextBox(USBTextBox, "[INFO] Vehicle profiles file is missing (VehicleProfiles.xml)", null);
            }

            ActiveControl = ConnectButton; // put focus on the connect button
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
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
                ConnectButton.Enabled = false;
                byte[] FirstHandshakeRequest = new byte[] { 0x3D, 0x00, 0x02, 0x01, 0x01, 0x04 };
                string[] ports = SerialPort.GetPortNames(); //  get all the available portnames
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
                    Util.UpdateTextBox(USBTextBox, "[<-TX] Handshake request (" + Serial.PortName + ")", FirstHandshakeRequest);
                    SerialDataWriteAsync(FirstHandshakeRequest);

                    Timeout = false; // For the connection procedure we have to manually read response bytes here
                    TimeoutTimer.Enabled = true; // start counting to the set timeout value
                    while ((Serial.BytesToRead < 27) && !Timeout) ; // Wait for expected bytes to arrive
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
                        Util.UpdateTextBox(USBTextBox, "[->RX] Handshake (" + Serial.PortName + ")", msg);
                        Util.UpdateTextBox(USBTextBox, "[INFO] Handshake OK: " + Encoding.ASCII.GetString(msg, 5, 21), null);
                        Util.UpdateTextBox(USBTextBox, "[INFO] Scanner connected (" + Serial.PortName + ")", null);
                        ScannerFound = true; // set flag
                        SerialDataReadAsync(); // start listening to the last opened serial port for incoming data

                        string comport_number_only = Serial.PortName; // update status strip with a nicely formatted label
                        comport_number_only = comport_number_only.Remove(0, 3); // remove "COM" from "COM#"
                        byte comport_number = Convert.ToByte(comport_number_only); // get the numerical value of the comport

                        ConnectButton.Text = "Disconnect";
                        ConnectButton.Enabled = true;
                        MillisButton.Enabled = true;
                        HandshakeButton.Enabled = true;
                        USBCommunicationGroupBox.Enabled = true;
                        if (HexCommMethodRadioButton.Checked)
                        {
                            USBSendComboBox.DropDownStyle = ComboBoxStyle.DropDown;
                            USBSendComboBox.Text = "3D 00 02 01 00 03";
                        }
                        else if (AsciiCommMethodRadioButton.Checked)
                        {
                            USBSendComboBox.DropDownStyle = ComboBoxStyle.DropDown;
                            USBSendComboBox.Text = ">";
                        }
                        this.Text = "Chrysler CCD/SCI Scanner | " + Serial.PortName;
                        return;
                    }
                    else
                    {
                        Util.UpdateTextBox(USBTextBox, "[->RX] Handshake response (" + Serial.PortName + ")", msg);
                        Util.UpdateTextBox(USBTextBox, "[INFO] Handshake ERROR", null);
                        ScannerFound = false;
                    }
                }
                Util.UpdateTextBox(USBTextBox, "[INFO] No scanner available", null);
                ConnectButton.Enabled = true;
            }
            else
            {
                if (Serial.IsOpen && ScannerFound)
                {
                    Serial.Close();
                    ScannerFound = false;
                    ConnectButton.Text = "Connect";
                    MillisButton.Enabled = false;
                    HandshakeButton.Enabled = false;
                    
                    Timeout = false;
                    TimeoutTimer.Enabled = true; // start counting to the set timeout value
                    while ((SerialRxBuffer.ReadLength > 0) && !Timeout) ; // Wait for ringbuffer to get empty
                    TimeoutTimer.Enabled = false; // disable timer

                    SerialRxBuffer.Reset();
                    Util.UpdateTextBox(USBTextBox, "[INFO] Scanner disconnected (" + Serial.PortName + ")", null);
                    USBSendComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                    USBCommunicationGroupBox.Enabled = false;
                    this.Text = "Chrysler CCD/SCI Scanner";
                }
            }
        }

        private void MillisButton_Click(object sender, EventArgs e)
        {
            if (Serial.IsOpen && ScannerFound)
            {
                Util.UpdateTextBox(USBTextBox, "[<-TX] Timestamp request", MillisRequest);
                SerialDataWriteAsync(MillisRequest);
            }
        }

        private void HandshakeButton_Click(object sender, EventArgs e)
        {
            if (Serial.IsOpen && ScannerFound)
            {
                Util.UpdateTextBox(USBTextBox, "[<-TX] Handshake request", HandshakeRequest);
                SerialDataWriteAsync(HandshakeRequest);
            }
        }

        // This method gets called everytime when something arrives on the COM-port
        // It searches for valid packets (even if there are multiple packets in one reception) and discards the garbage bytes
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
                byte checksum = msg[PacketSize - 1];

                byte source = (byte)((datacode >> 6) & 0x03);
                byte target = (byte)((datacode >> 4) & 0x03);
                byte dc_command = (byte)(datacode & 0x0F);

                switch (source)
                {
                    case (byte)Source.USB: // message is coming from the scanner directly, no need to analyze target, it has to be an external computer
                        switch (dc_command)
                        {
                            case (byte)Command.Reset:
                                if (TM == (byte)TransmissionMethod.Hex)
                                {
                                    Util.UpdateTextBox(USBTextBox, "[->RX] Scanner is resetting now", msg);
                                }
                                else if (TM == (byte)TransmissionMethod.Ascii)
                                {
                                    Util.UpdateTextBox(USBTextBox, "> Scanner is resetting now", null);
                                }
                                break;
                            case (byte)Command.Handshake:
                                if (Encoding.ASCII.GetString(payload) == "CHRYSLERCCDSCISCANNER")
                                {
                                    Util.UpdateTextBox(USBTextBox, "[->RX] Handshake response (" + Serial.PortName + ")", msg);
                                    Util.UpdateTextBox(USBTextBox, "[INFO] Handshake OK: " + Encoding.ASCII.GetString(payload), null);
                                    ScannerFound = true;
                                }
                                else
                                {
                                    Util.UpdateTextBox(USBTextBox, "[->RX] Handshake response (" + Serial.PortName + ")", msg);
                                    Util.UpdateTextBox(USBTextBox, "[INFO] Handshake ERROR", null);
                                    ScannerFound = false;
                                }
                                break;
                            case (byte)Command.Response:
                                switch (subdatacode)
                                {
                                    case (byte)Response.FirmwareDate:
                                        DateTime FirmwareDate = Util.UnixTimeStampToDateTime(payload[0] << 56 | payload[1] << 48 | payload[2] << 40 | payload[3] << 32 | payload[4] << 24 | payload[5] << 16 | payload[6] << 8 | payload[7]);
                                        string FirmwareDateString = FirmwareDate.ToString("yyyy.MM.dd HH:mm:ss");
                                        Util.UpdateTextBox(USBTextBox, "[->RX] Firmware date response", msg);
                                        Util.UpdateTextBox(USBTextBox, "[INFO] Firmware date: " + FirmwareDateString, null);
                                        break;
                                    case (byte)Response.Timestamp:
                                        TimeSpan ElapsedTime = TimeSpan.FromMilliseconds(payload[0] << 24 | payload[1] << 16 | payload[2] << 8 | payload[3]);
                                        DateTime Timestamp = DateTime.Today.Add(ElapsedTime);
                                        string TimestampString = Timestamp.ToString("HH:mm:ss.fff");
                                        Util.UpdateTextBox(USBTextBox, "[->RX] Timestamp response", msg);
                                        Util.UpdateTextBox(USBTextBox, "[INFO] Timestamp: " + TimestampString, null);
                                        break;
                                    case (byte)Response.BatteryVoltage:
                                        string BatteryVoltageString = ((payload[0] << 8 | payload[1]) / 100).ToString() + " V";
                                        Util.UpdateTextBox(USBTextBox, "[->RX] Battery voltage response", msg);
                                        Util.UpdateTextBox(USBTextBox, "[INFO] Battery voltage: " + BatteryVoltageString, null);
                                        break;
                                    default:
                                        Util.UpdateTextBox(USBTextBox, "[->RX] Data received", msg);
                                        break;
                                }
                                break;
                            default:
                                Util.UpdateTextBox(USBTextBox, "[->RX] Data received", msg);
                                break;
                        }
                        break;
                    case (byte)Source.CCDBus:
                        Util.UpdateTextBox(USBTextBox, "[->RX] CCD-bus message", msg);
                        TT.UpdateTextTable(source, payload, 4, payload.Length);
                        if (IncludeTimestap) File.AppendAllText(CCDLogFilename, Util.ByteToHexString(payload, 0, payload.Length) + Environment.NewLine);
                        else File.AppendAllText(CCDLogFilename, Util.ByteToHexString(payload, 4, payload.Length) + Environment.NewLine);
                        break;
                    case (byte)Source.SCIBusPCM:
                        Util.UpdateTextBox(USBTextBox, "[->RX] SCI-bus message (PCM)", msg);
                        TT.UpdateTextTable(source, payload, 4, payload.Length);
                        if (IncludeTimestap) File.AppendAllText(PCMLogFilename, Util.ByteToHexString(payload, 0, payload.Length) + Environment.NewLine);
                        else File.AppendAllText(PCMLogFilename, Util.ByteToHexString(payload, 4, payload.Length) + Environment.NewLine);
                        break;
                    case (byte)Source.SCIBusTCM:
                        Util.UpdateTextBox(USBTextBox, "[->RX] SCI-bus message (TCM)", msg);
                        TT.UpdateTextTable(source, payload, 4, payload.Length);
                        if (IncludeTimestap) File.AppendAllText(TCMLogFilename, Util.ByteToHexString(payload, 0, payload.Length) + Environment.NewLine);
                        else File.AppendAllText(TCMLogFilename, Util.ByteToHexString(payload, 4, payload.Length) + Environment.NewLine);
                        break;
                    default:
                        Util.UpdateTextBox(USBTextBox, "[->RX] Data received", msg);
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
                        if ((bytes.Length > 5) && (bytes[0] != 0x00))
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

        private void USBShowTrafficCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (USBShowTrafficCheckBox.Checked)
            {
                USBTextBox.Enabled = true;
                USBShowTraffic = true;
            }
            else
            {
                USBTextBox.Enabled = false;
                USBShowTraffic = false;
            }
        }

        private void TableUpdated(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "TableUpdated")
            {
                DiagnosticsTextBox.Invoke((MethodInvoker)delegate
                {
                    int savedVpos = GetScrollPos(DiagnosticsTextBox.Handle, SB_VERT);
                    DiagnosticsTextBox.Text = String.Join(Environment.NewLine, TT.Table);
                    SetScrollPos(DiagnosticsTextBox.Handle, SB_VERT, savedVpos, true);
                    PostMessageA(DiagnosticsTextBox.Handle, WM_VSCROLL, SB_THUMBPOSITION + 0x10000 * savedVpos, 0);
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

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (DiagnosticsTextBox.Visible) DiagnosticsTextBox.Visible = false;
            else DiagnosticsTextBox.Visible = true;
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
                CommandComboBox.Visible = true;
                ModeLabel.Visible = true;
                ModeComboBox.Visible = true;
                HintTextBox.Visible = true;
                USBSendComboBox.Text = String.Empty;
                USBTextBox.Size = new Size(359, 220);
                TM = (byte)TransmissionMethod.Hex;
                USBTextBox.ScrollToCaret();
                try
                {
                    Properties.Settings.Default["TransmissionMethod"] = "hex";
                    Properties.Settings.Default.Save(); // Saves settings in application configuration file
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
                CommandComboBox.Visible = false;
                ModeLabel.Visible = false;
                ModeComboBox.Visible = false;
                HintTextBox.Visible = false;
                USBSendComboBox.Text = ">";
                USBTextBox.Size = new Size(359, 430);
                TM = (byte)TransmissionMethod.Ascii;
                USBTextBox.ScrollToCaret();
                try
                {
                    Properties.Settings.Default["TransmissionMethod"] = "ascii";
                    Properties.Settings.Default.Save(); // Saves settings in application configuration file
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
                    Properties.Settings.Default.Save(); // Saves settings in application configuration file
                }

                else if (!MetricUnitRadioButton.Checked && ImperialUnitRadioButton.Checked)
                {
                    Properties.Settings.Default["Units"] = "imperial";
                    Properties.Settings.Default.Save(); // Saves settings in application configuration file
                }
            }
            catch
            {

            }
        }
    }
}