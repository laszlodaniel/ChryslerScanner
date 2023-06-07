using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using DRBDBReader.DB;
using DRBDBReader.DB.Records;
using ChryslerScanner.Helpers;
using ChryslerScanner.Models;
using ChryslerScanner.Services;

namespace ChryslerScanner
{
    public partial class MainForm : Form
    {
        private readonly SynchronizationContext UIContext;

        private bool GUIUpdateAvailable = false;
        private bool FWUpdateAvailabile = false;
        private bool ResetFromUpdate = false;
        public string GUIVersion = string.Empty;
        public string FWVersion = string.Empty;
        public string HWVersion = string.Empty;
        private UInt64 DeviceFirmwareTimestamp;
        private string SelectedPort = string.Empty;
        private bool timeout = false;
        private bool DeviceFound = false;
        private const uint IntEEPROMsize = 4096;
        private const uint ExtEEPROMsize = 4096;
        private static bool PCMSelected = true;
        private static bool TCMSelected = false;

        public static string USBTextLogFilename;
        public static string USBBinaryLogFilename;
        public static string CCDLogFilename;
        public static string CCDB2F2LogFilename;
        public static string CCDEPROMTextFilename;
        public static string CCDEPROMBinaryFilename;
        public static string CCDEEPROMTextFilename;
        public static string CCDEEPROMBinaryFilename;
        public static string PCILogFilename;
        public static string PCMLogFilename;
        public static string PCMFlashTextFilename;
        public static string PCMEPROMBinaryFilename;
        public static string PCMEEPROMTextFilename;
        public static string PCMEEPROMBinaryFilename;
        public static string TCMLogFilename;

        private int LastCCDScrollBarPosition = 0;
        private int LastPCIScrollBarPosition = 0;
        private int LastPCMScrollBarPosition = 0;
        private int LastTCMScrollBarPosition = 0;

        private static ushort TableRefreshRate = 0;

        private List<string> CCDTableBuffer = new List<string>();
        private List<int> CCDTableBufferLocation = new List<int>();
        private List<int> CCDTableRowCountHistory = new List<int>();

        private List<string> PCITableBuffer = new List<string>();
        private List<int> PCITableBufferLocation = new List<int>();
        private List<int> PCITableRowCountHistory = new List<int>();

        private List<string> PCMTableBuffer = new List<string>();
        private List<int> PCMTableBufferLocation = new List<int>();
        private List<int> PCMTableRowCountHistory = new List<int>();

        private List<string> TCMTableBuffer = new List<string>();
        private List<int> TCMTableBufferLocation = new List<int>();
        private List<int> TCMTableRowCountHistory = new List<int>();

        private readonly SerialService SerialService;

        private ReadMemoryForm ReadMemory;
        private ReadWriteMemoryForm ReadWriteMemory;
        private BootstrapToolsForm BootstrapTools;
        private EngineToolsForm EngineTools;
        private ABSToolsForm ABSTools;
        private AboutForm About;
        public CCD CCD = new CCD();
        public PCI PCI = new PCI();
        public SCIPCM PCM = new SCIPCM();
        public SCITCM TCM = new SCITCM();
        private System.Timers.Timer TimeoutTimer = new System.Timers.Timer();
        private System.Timers.Timer CCDTableRefreshTimer = new System.Timers.Timer();
        private System.Timers.Timer PCITableRefreshTimer = new System.Timers.Timer();
        private System.Timers.Timer PCMTableRefreshTimer = new System.Timers.Timer();
        private System.Timers.Timer TCMTableRefreshTimer = new System.Timers.Timer();
        private WebClient Downloader = new WebClient();
        private FileInfo fi = new FileInfo(@"DRBDBReader/database.mem");
        private Database db;

        public MainForm(SerialService service)
        {
            InitializeComponent();
            UIContext = SynchronizationContext.Current;
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); // set application icon
            DiagnosticsGroupBox.Visible = false; // hide the expanded view components all at once
            Size = new Size(405, 650); // resize form to collapsed view
            CenterToScreen(); // put window at the center of the screen

            SerialService = service;

            if (fi.Exists) db = new Database(fi); // load DRB3 database

            GUIVersion = "v" + Assembly.GetExecutingAssembly().GetName().Version.ToString(3);
            Text += "  |  GUI " + GUIVersion;

            if (!Directory.Exists("LOG")) Directory.CreateDirectory("LOG");
            if (!Directory.Exists("LOG/ABS")) Directory.CreateDirectory("LOG/ABS");
            if (!Directory.Exists("LOG/CCD")) Directory.CreateDirectory("LOG/CCD");
            if (!Directory.Exists("LOG/PCI")) Directory.CreateDirectory("LOG/PCI");
            if (!Directory.Exists("LOG/SCI")) Directory.CreateDirectory("LOG/SCI");
            if (!Directory.Exists("LOG/PCM")) Directory.CreateDirectory("LOG/PCM");
            if (!Directory.Exists("LOG/TCM")) Directory.CreateDirectory("LOG/TCM");
            if (!Directory.Exists("LOG/USB")) Directory.CreateDirectory("LOG/USB");
            if (!Directory.Exists("ROMs")) Directory.CreateDirectory("ROMs");
            if (!Directory.Exists("ROMs/CCD")) Directory.CreateDirectory("ROMs/CCD");
            if (!Directory.Exists("ROMs/PCM")) Directory.CreateDirectory("ROMs/PCM");

            string DateTimeNow = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            USBTextLogFilename = @"LOG/USB/usblog_" + DateTimeNow + ".txt";
            USBBinaryLogFilename = @"LOG/USB/usblog_" + DateTimeNow + ".bin";

            CCDLogFilename = @"LOG/CCD/ccdlog_" + DateTimeNow + ".txt";
            CCDB2F2LogFilename = @"LOG/CCD/ccdb2f2log_" + DateTimeNow + ".txt";
            //CCDEPROMTextFilename = @"ROMS/CCD/ccd_eprom_" + DateTimeNow + ".txt";
            //CCDEPROMBinaryFilename = @"ROMs/CCD/ccd_eprom_" + DateTimeNow + ".bin";
            //CCDEEPROMTextFilename = @"ROMs/CCD/ccd_eeprom_" + DateTimeNow + ".txt";
            //CCDEEPROMBinaryFilename = @"ROMs/CCD/ccd_eeprom_" + DateTimeNow + ".bin";

            PCILogFilename = @"LOG/PCI/pcilog_" + DateTimeNow + ".txt";

            PCMLogFilename = @"LOG/PCM/pcmlog_" + DateTimeNow + ".txt";
            //PCMFlashTextFilename = @"ROMs/PCM/pcm_flash_" + DateTimeNow + ".txt";
            //PCMEPROMBinaryFilename = @"ROMs/PCM/pcm_eprom_" + DateTimeNow + ".bin";
            //PCMEEPROMTextFilename = @"ROMs/PCM/pcm_eeprom_" + DateTimeNow + ".txt";
            //PCMEEPROMBinaryFilename = @"ROMs/PCM/pcm_eeprom_" + DateTimeNow + ".bin";

            TCMLogFilename = @"LOG/TCM/tcmlog_" + DateTimeNow + ".txt";

            TimeoutTimer.Elapsed += new ElapsedEventHandler(TimeoutHandler);
            TimeoutTimer.Interval = 2000; // ms
            TimeoutTimer.AutoReset = false;
            TimeoutTimer.Enabled = true;
            TimeoutTimer.Stop();
            timeout = false;

            CCDTableRefreshTimer.Elapsed += new ElapsedEventHandler(CCDTableRefreshHandler);
            CCDTableRefreshTimer.Interval = 10; // ms
            CCDTableRefreshTimer.AutoReset = true;
            CCDTableRefreshTimer.Enabled = true;

            PCITableRefreshTimer.Elapsed += new ElapsedEventHandler(PCITableRefreshHandler);
            PCITableRefreshTimer.Interval = 10; // ms
            PCITableRefreshTimer.AutoReset = true;
            PCITableRefreshTimer.Enabled = true;

            PCMTableRefreshTimer.Elapsed += new ElapsedEventHandler(PCMTableRefreshHandler);
            PCMTableRefreshTimer.Interval = 10; // ms
            PCMTableRefreshTimer.AutoReset = true;
            PCMTableRefreshTimer.Enabled = true;

            TCMTableRefreshTimer.Elapsed += new ElapsedEventHandler(TCMTableRefreshHandler);
            TCMTableRefreshTimer.Interval = 10; // ms
            TCMTableRefreshTimer.AutoReset = true;
            TCMTableRefreshTimer.Enabled = true;

            UpdateCOMPortList();

            CCDBusDiagnosticsListBox.Items.AddRange(CCD.Diagnostics.Table.ToArray());
            PCIBusDiagnosticsListBox.Items.AddRange(PCI.Diagnostics.Table.ToArray());
            SCIBusPCMDiagnosticsListBox.Items.AddRange(PCM.Diagnostics.Table.ToArray());
            SCIBusTCMDiagnosticsListBox.Items.AddRange(TCM.Diagnostics.Table.ToArray());

            SCIBusModuleComboBox.SelectedIndex = 0;
            SCIBusOBDConfigurationComboBox.SelectedIndex = 0;
            SCIBusSpeedComboBox.SelectedIndex = 2;
            SCIBusLogicComboBox.SelectedIndex = 2;
            LCDStateComboBox.SelectedIndex = 0;
            LCDDataSourceComboBox.SelectedIndex = 0;

            // Load saved settings.
            if (Properties.Settings.Default.Units == "metric")
            {
                MetricUnitsToolStripMenuItem.Checked = true;
                ImperialUnitsToolStripMenuItem.Checked = false;
            }
            else if (Properties.Settings.Default.Units == "imperial")
            {
                MetricUnitsToolStripMenuItem.Checked = false;
                ImperialUnitsToolStripMenuItem.Checked = true;
            }

            if (Properties.Settings.Default.Language == "English")
            {
                EnglishLangToolStripMenuItem.Checked = true;
                SpanishLangToolStripMenuItem.Checked = false;
            }
            else if (Properties.Settings.Default.Language == "Spanish")
            {
                EnglishLangToolStripMenuItem.Checked = false;
                SpanishLangToolStripMenuItem.Checked = true;
            }

            ChangeLanguage();

            if (Properties.Settings.Default.UARTBaudrate == 250000)
            {
                Baudrate250000ToolStripMenuItem.Checked = true;
                Baudrate115200ToolStripMenuItem.Checked = false;
            }
            else if (Properties.Settings.Default.UARTBaudrate == 115200)
            {
                Baudrate250000ToolStripMenuItem.Checked = false;
                Baudrate115200ToolStripMenuItem.Checked = true;
            }

            if (Properties.Settings.Default.Timestamp == true)
            {
                IncludeTimestampInLogFilesToolStripMenuItem.Checked = true;
            }
            else
            {
                IncludeTimestampInLogFilesToolStripMenuItem.Checked = false;
            }

            if (Properties.Settings.Default.CCDBusOnDemand == true)
            {
                CCDBusOnDemandToolStripMenuItem.Checked = true;
            }
            else
            {
                CCDBusOnDemandToolStripMenuItem.Checked = false;
            }

            if (Properties.Settings.Default.PCIBusOnDemand == true)
            {
                PCIBusOnDemandToolStripMenuItem.Checked = true;
            }
            else
            {
                PCIBusOnDemandToolStripMenuItem.Checked = false;
            }

            if (Properties.Settings.Default.SortByID == true)
            {
                SortMessagesByIDByteToolStripMenuItem.Checked = true;
            }
            else
            {
                SortMessagesByIDByteToolStripMenuItem.Checked = false;
            }

            if (Properties.Settings.Default.DisplayRawBusPackets == true)
            {
                DisplayRawBusPacketsToolStripMenuItem.Checked = true;
            }
            else
            {
                DisplayRawBusPacketsToolStripMenuItem.Checked = false;
            }

            ActiveControl = ConnectButton; // put focus on the connect button
        }

        #region Methods

        private void TimeoutHandler(object source, ElapsedEventArgs e) => timeout = true;

        private void CCDTableRefreshHandler(object source, ElapsedEventArgs e)
        {
            if (CCDTableBuffer.Count == 0)
                return;

            CCDBusDiagnosticsListBox.BeginInvoke((MethodInvoker)delegate
            {
                CCDBusDiagnosticsListBox.BeginUpdate();

                LastCCDScrollBarPosition = CCDBusDiagnosticsListBox.GetVerticalScrollPosition();

                // Update header line.
                CCDBusDiagnosticsListBox.Items.RemoveAt(1);
                CCDBusDiagnosticsListBox.Items.Insert(1, CCD.Diagnostics.Table[1]);

                // Update lines from buffer.
                for (int i = 0; i < CCDTableBuffer.Count; i++)
                {
                    if (CCDBusDiagnosticsListBox.Items.Count == CCDTableRowCountHistory[i])
                    {
                        if (CCDTableBufferLocation[i] < CCDBusDiagnosticsListBox.Items.Count) // check if buffer location is within the ListBox's item count
                        {
                            CCDBusDiagnosticsListBox.Items.RemoveAt(CCDTableBufferLocation[i]);
                        }
                    }

                    if (CCDTableBufferLocation[i] <= CCDBusDiagnosticsListBox.Items.Count) // check if buffer location is within the ListBox's item count
                    {
                        CCDBusDiagnosticsListBox.Items.Insert(CCDTableBufferLocation[i], CCDTableBuffer[i]);
                    }
                }

                CCDBusDiagnosticsListBox.SetVerticalScrollPosition(LastCCDScrollBarPosition);

                CCDBusDiagnosticsListBox.EndUpdate();

                CCDTableBuffer.Clear();
                CCDTableBufferLocation.Clear();
                CCDTableRowCountHistory.Clear();
            });
        }

        private void PCITableRefreshHandler(object source, ElapsedEventArgs e)
        {
            if (PCITableBuffer.Count == 0)
                return;

            PCIBusDiagnosticsListBox.BeginInvoke((MethodInvoker)delegate
            {
                PCIBusDiagnosticsListBox.BeginUpdate();

                LastPCIScrollBarPosition = PCIBusDiagnosticsListBox.GetVerticalScrollPosition();

                // Update header line.
                PCIBusDiagnosticsListBox.Items.RemoveAt(1);
                PCIBusDiagnosticsListBox.Items.Insert(1, PCI.Diagnostics.Table[1]);

                // Update lines from buffer.
                for (int i = 0; i < PCITableBuffer.Count; i++)
                {
                    if (PCIBusDiagnosticsListBox.Items.Count == PCITableRowCountHistory[i])
                    {
                        if (PCITableBufferLocation[i] < PCIBusDiagnosticsListBox.Items.Count) // check if buffer location is within the ListBox's item count
                        {
                            PCIBusDiagnosticsListBox.Items.RemoveAt(PCITableBufferLocation[i]);
                        }
                    }

                    if (PCITableBufferLocation[i] <= PCIBusDiagnosticsListBox.Items.Count) // check if buffer location is within the ListBox's item count
                    {
                        PCIBusDiagnosticsListBox.Items.Insert(PCITableBufferLocation[i], PCITableBuffer[i]);
                    }
                }

                PCIBusDiagnosticsListBox.SetVerticalScrollPosition(LastPCIScrollBarPosition);

                PCIBusDiagnosticsListBox.EndUpdate();

                PCITableBuffer.Clear();
                PCITableBufferLocation.Clear();
                PCITableRowCountHistory.Clear();
            });
        }

        private void PCMTableRefreshHandler(object source, ElapsedEventArgs e)
        {
            if (PCMTableBuffer.Count == 0)
                return;

            SCIBusPCMDiagnosticsListBox.BeginInvoke((MethodInvoker)delegate
            {
                SCIBusPCMDiagnosticsListBox.BeginUpdate();

                LastPCMScrollBarPosition = SCIBusPCMDiagnosticsListBox.GetVerticalScrollPosition();

                // Update header line.
                SCIBusPCMDiagnosticsListBox.Items.RemoveAt(1);
                SCIBusPCMDiagnosticsListBox.Items.Insert(1, PCM.Diagnostics.Table[1]);

                // Update lines from buffer.
                for (int i = 0; i < PCMTableBuffer.Count; i++)
                {
                    if (SCIBusPCMDiagnosticsListBox.Items.Count == PCMTableRowCountHistory[i])
                    {
                        if (PCMTableBufferLocation[i] < SCIBusPCMDiagnosticsListBox.Items.Count) // check if buffer location is within the ListBox's item count
                        {
                            SCIBusPCMDiagnosticsListBox.Items.RemoveAt(PCMTableBufferLocation[i]);
                        }
                    }

                    if (PCMTableBufferLocation[i] <= SCIBusPCMDiagnosticsListBox.Items.Count) // check if buffer location is within the ListBox's item count
                    {
                        SCIBusPCMDiagnosticsListBox.Items.Insert(PCMTableBufferLocation[i], PCMTableBuffer[i]);
                    }
                }

                SCIBusPCMDiagnosticsListBox.SetVerticalScrollPosition(LastPCMScrollBarPosition);

                SCIBusPCMDiagnosticsListBox.EndUpdate();

                PCMTableBuffer.Clear();
                PCMTableBufferLocation.Clear();
                PCMTableRowCountHistory.Clear();
            });
        }

        private void TCMTableRefreshHandler(object source, ElapsedEventArgs e)
        {
            if (TCMTableBuffer.Count == 0)
                return;

            SCIBusTCMDiagnosticsListBox.BeginInvoke((MethodInvoker)delegate
            {
                SCIBusTCMDiagnosticsListBox.BeginUpdate();

                LastTCMScrollBarPosition = SCIBusTCMDiagnosticsListBox.GetVerticalScrollPosition();

                // Update header line.
                SCIBusTCMDiagnosticsListBox.Items.RemoveAt(1);
                SCIBusTCMDiagnosticsListBox.Items.Insert(1, TCM.Diagnostics.Table[1]);

                // Update lines from buffer.
                for (int i = 0; i < TCMTableBuffer.Count; i++)
                {
                    if (SCIBusTCMDiagnosticsListBox.Items.Count == TCMTableRowCountHistory[i])
                    {
                        SCIBusTCMDiagnosticsListBox.Items.RemoveAt(TCMTableBufferLocation[i]);
                    }

                    if (TCMTableBufferLocation[i] <= SCIBusTCMDiagnosticsListBox.Items.Count) // check if buffer location is within the ListBox's item count
                    {
                        SCIBusTCMDiagnosticsListBox.Items.Insert(TCMTableBufferLocation[i], TCMTableBuffer[i]);
                    }
                }

                SCIBusTCMDiagnosticsListBox.SetVerticalScrollPosition(LastTCMScrollBarPosition);

                SCIBusTCMDiagnosticsListBox.EndUpdate();

                TCMTableBuffer.Clear();
                TCMTableBufferLocation.Clear();
                TCMTableRowCountHistory.Clear();
            });
        }

        private void UpdateCOMPortList()
        {
            COMPortsComboBox.Items.Clear(); // clear combobox

            string[] ports = SerialPort.GetPortNames(); // get available ports

            if (ports.Length > 0)
            {
                COMPortsComboBox.Items.AddRange(ports);
                ConnectButton.Enabled = true;

                if (SelectedPort == string.Empty) // if no port has been selected
                {
                    COMPortsComboBox.SelectedIndex = 0; // select first available port
                    SelectedPort = COMPortsComboBox.Text;
                }
                else
                {
                    try
                    {
                        COMPortsComboBox.SelectedIndex = COMPortsComboBox.Items.IndexOf(SelectedPort); // try to find previously selected port
                    }
                    catch
                    {
                        COMPortsComboBox.SelectedIndex = 0; // if previously selected port is not found then select first available port
                    }
                }
            }
            else
            {
                COMPortsComboBox.Items.Add("N/A");
                ConnectButton.Enabled = false;
                COMPortsComboBox.SelectedIndex = 0; // select "N/A"
                SelectedPort = string.Empty;
                Util.UpdateTextBox(USBTextBox, "[INFO] No device available.");
            }
        }

        private void SetRepeatedMessageBehavior(PacketHelper.Bus bus)
        {
            List<byte> payloadList = new List<byte>();
            bool success = false;
            int repeatInterval = 0;
            byte[] repeatIntervalArray = new byte[2];

            switch (bus)
            {
                case PacketHelper.Bus.CCD:
                {
                    success = int.TryParse(CCDBusTxMessageRepeatIntervalTextBox.Text, out repeatInterval);
                    break;
                }
                case PacketHelper.Bus.PCI:
                {
                    success = int.TryParse(PCIBusTxMessageRepeatIntervalTextBox.Text, out repeatInterval);
                    break;
                }
                case PacketHelper.Bus.PCM:
                {
                    success = int.TryParse(SCIBusTxMessageRepeatIntervalTextBox.Text, out repeatInterval);
                    break;
                }
                case PacketHelper.Bus.TCM:
                {
                    success = int.TryParse(SCIBusTxMessageRepeatIntervalTextBox.Text, out repeatInterval);
                    break;
                }
            }

            if (!success || (repeatInterval == 0))
            {
                repeatInterval = 100;

                switch (bus)
                {
                    case PacketHelper.Bus.CCD:
                    {
                        CCDBusTxMessageRepeatIntervalTextBox.Text = "100";
                        break;
                    }
                    case PacketHelper.Bus.PCI:
                    {
                        PCIBusTxMessageRepeatIntervalTextBox.Text = "100";
                        break;
                    }
                    case PacketHelper.Bus.PCM:
                    {
                        SCIBusTxMessageRepeatIntervalTextBox.Text = "100";
                        break;
                    }
                    case PacketHelper.Bus.TCM:
                    {
                        SCIBusTxMessageRepeatIntervalTextBox.Text = "100";
                        break;
                    }
                }
            }

            repeatIntervalArray[0] = (byte)((repeatInterval >> 8) & 0xFF);
            repeatIntervalArray[1] = (byte)(repeatInterval & 0xFF);

            payloadList.Add((byte)bus);
            payloadList.AddRange(repeatIntervalArray);

            Packet packet = new Packet();

            packet.Bus = (byte)PacketHelper.Bus.USB;
            packet.Command = (byte)PacketHelper.Command.Settings;
            packet.Mode = (byte)PacketHelper.SettingsMode.SetRepeatBehavior;
            packet.Payload = payloadList.ToArray();

            switch (bus)
            {
                case PacketHelper.Bus.CCD:
                {
                    Util.UpdateTextBox(USBTextBox, "[<-TX] Set CCD-bus message repeat:", PacketHelper.Serialize(packet));
                    break;
                }
                case PacketHelper.Bus.PCI:
                {
                    Util.UpdateTextBox(USBTextBox, "[<-TX] Set PCI-bus message repeat:", PacketHelper.Serialize(packet));
                    break;
                }
                case PacketHelper.Bus.PCM:
                {
                    Util.UpdateTextBox(USBTextBox, "[<-TX] Set SCI-bus (PCM) message repeat:", PacketHelper.Serialize(packet));
                    break;
                }
                case PacketHelper.Bus.TCM:
                {
                    Util.UpdateTextBox(USBTextBox, "[<-TX] Set SCI-bus (TCM) message repeat:", PacketHelper.Serialize(packet));
                    break;
                }
            }

            SerialService.WritePacket(packet);
        }

        private void PacketReceivedHandler(object sender, Packet packet)
        {
            UIContext.Post(state =>
            {
                switch (packet.Bus)
                {
                    case (byte)PacketHelper.Bus.USB:
                    {
                        switch (packet.Command)
                        {
                            case (byte)PacketHelper.Command.Reset:
                            {
                                if ((packet.Payload == null) || (packet.Payload.Length == 0))
                                {
                                    Util.UpdateTextBox(USBTextBox, "[RX->] Invalid reset packet:", PacketHelper.Serialize(packet));
                                    break;
                                }

                                switch (packet.Mode)
                                {
                                    case (byte)PacketHelper.ResetMode.ResetInit:
                                    {
                                        Util.UpdateTextBox(USBTextBox, "[RX->] Reset in progress:", PacketHelper.Serialize(packet));
                                        Util.UpdateTextBox(USBTextBox, "[INFO] Device is resetting, please wait.");
                                        break;
                                    }
                                    case (byte)PacketHelper.ResetMode.ResetDone:
                                    {
                                        Util.UpdateTextBox(USBTextBox, "[RX->] Reset done:", PacketHelper.Serialize(packet));

                                        switch (packet.Payload[0])
                                        {
                                            case 0:
                                            {
                                                Util.UpdateTextBox(USBTextBox, "[INFO] Device is ready to accept instructions.");
                                                break;
                                            }
                                            case 1:
                                            {
                                                Util.UpdateTextBox(USBTextBox, "[INFO] Reset reason: ESP_RST_POWERON");
                                                break;
                                            }
                                            case 2:
                                            {
                                                Util.UpdateTextBox(USBTextBox, "[INFO] Reset reason: ESP_RST_EXT");
                                                break;
                                            }
                                            case 3:
                                            {
                                                Util.UpdateTextBox(USBTextBox, "[INFO] Reset reason: ESP_RST_SW");
                                                break;
                                            }
                                            case 4:
                                            {
                                                Util.UpdateTextBox(USBTextBox, "[INFO] Reset reason: ESP_RST_PANIC");
                                                break;
                                            }
                                            case 5:
                                            {
                                                Util.UpdateTextBox(USBTextBox, "[INFO] Reset reason: ESP_RST_INT_WDT");
                                                break;
                                            }
                                            case 6:
                                            {
                                                Util.UpdateTextBox(USBTextBox, "[INFO] Reset reason: ESP_RST_TASK_WDT");
                                                break;
                                            }
                                            case 7:
                                            {
                                                Util.UpdateTextBox(USBTextBox, "[INFO] Reset reason: ESP_RST_WDT");
                                                break;
                                            }
                                            case 8:
                                            {
                                                Util.UpdateTextBox(USBTextBox, "[INFO] Reset reason: ESP_RST_DEEPSLEEP");
                                                break;
                                            }
                                            case 9:
                                            {
                                                Util.UpdateTextBox(USBTextBox, "[INFO] Reset reason: ESP_RST_BROWNOUT");
                                                break;
                                            }
                                            case 10:
                                            {
                                                Util.UpdateTextBox(USBTextBox, "[INFO] Reset reason: ESP_RST_SDIO");
                                                break;
                                            }
                                            default:
                                            {
                                                Util.UpdateTextBox(USBTextBox, "[INFO] Reset reason: unknown.");
                                                break;
                                            }
                                        }

                                        if (ResetFromUpdate)
                                        {
                                            ResetFromUpdate = false;
                                            VersionInfoButton_Click(this, EventArgs.Empty);
                                            Util.UpdateTextBox(USBTextBox, "[INFO] Device firmware updated.");
                                        }
                                        break;
                                    }
                                    default:
                                    {
                                        Util.UpdateTextBox(USBTextBox, "[INFO] Unknown reset PacketHelper.");
                                        break;
                                    }
                                }
                                break;
                            }
                            case (byte)PacketHelper.Command.Handshake:
                            {
                                if (packet.Payload == null)
                                {
                                    Util.UpdateTextBox(USBTextBox, "[RX->] Invalid handshake packet:", PacketHelper.Serialize(packet));
                                    break;
                                }

                                byte[] serialized = PacketHelper.Serialize(packet);

                                Util.UpdateTextBox(USBTextBox, "[RX->] Handshake response:", serialized);

                                if (Util.CompareArrays(serialized, PacketHelper.ExpectedHandshake_V1, 0, PacketHelper.ExpectedHandshake_V1.Length) || Util.CompareArrays(serialized, PacketHelper.ExpectedHandshake_V2, 0, PacketHelper.ExpectedHandshake_V2.Length))
                                {
                                    Util.UpdateTextBox(USBTextBox, "[INFO] Handshake OK: " + Encoding.ASCII.GetString(packet.Payload, 0, packet.Payload.Length));
                                }
                                else
                                {
                                    Util.UpdateTextBox(USBTextBox, "[INFO] Handshake ERROR: " + Encoding.ASCII.GetString(packet.Payload, 0, packet.Payload.Length));
                                }
                                break;
                            }
                            case (byte)PacketHelper.Command.Status:
                            {
                                if (packet.Payload == null)
                                {
                                    Util.UpdateTextBox(USBTextBox, "[RX->] Invalid status packet:", PacketHelper.Serialize(packet));
                                    break;
                                }

                                if (HWVersion.Contains("v1.") && (packet.Payload.Length >= 53))
                                {
                                    string AVRSignature = Util.ByteToHexString(packet.Payload, 0, 3);
                                    if ((packet.Payload[0] == 0x1E) && (packet.Payload[1] == 0x98) && (packet.Payload[2] == 0x01)) AVRSignature += " (ATmega2560)";
                                    else AVRSignature += " (unknown)";

                                    string extEEPROMPresent = string.Empty;
                                    if (packet.Payload[3] == 0x01) extEEPROMPresent += "yes";
                                    else extEEPROMPresent += "no";
                                    string extEEPROMChecksum = Util.ByteToHexString(packet.Payload, 4, 1);
                                    if (packet.Payload[4] == packet.Payload[5]) extEEPROMChecksum += "=OK";
                                    else extEEPROMChecksum += "!=" + Util.ByteToHexString(packet.Payload, 5, 1) + ", ERROR";

                                    TimeSpan elapsedMillis = TimeSpan.FromMilliseconds((packet.Payload[6] << 24) + (packet.Payload[7] << 16) + (packet.Payload[8] << 8) + packet.Payload[9]);
                                    DateTime microcontrollerTimestamp = DateTime.Today.Add(elapsedMillis);
                                    string microcontrollerTimestampString = microcontrollerTimestamp.ToString("HH:mm:ss.fff");

                                    int freeRAM = (packet.Payload[10] << 8) + packet.Payload[11];
                                    string freeRAMString = (100.0 * ((8192.0 - freeRAM) / 8192.0)).ToString("0.0") + "% (" + (8192.0 - freeRAM).ToString("0") + "/8192 bytes)";

                                    string connectedToVehicle = string.Empty;
                                    if (packet.Payload[12] == 0x01) connectedToVehicle = "yes";
                                    else connectedToVehicle = "no";

                                    string batteryVoltageString = (((packet.Payload[13] << 8) + packet.Payload[14]) / 1000.00).ToString("0.000").Replace(",", ".") + " V";

                                    string CCDBusStateString = string.Empty;
                                    string CCDBusLogicString = string.Empty;
                                    string CCDTerminationBiasEnabledString = string.Empty;
                                    string CCDBusSpeedString = string.Empty;
                                    string CCDBusMsgRxCountString = string.Empty;
                                    string CCDBusMsgTxCountString = string.Empty;

                                    if (Util.IsBitClear(packet.Payload[15], 7))
                                    {
                                        CCDBusTransceiverOnOffCheckBox.CheckedChanged -= new EventHandler(CCDBusSettingsCheckBox_CheckedChanged);
                                        CCDBusTransceiverOnOffCheckBox.Checked = false;
                                        CCDBusTransceiverOnOffCheckBox.Text = "CCD-bus transceiver OFF";
                                        CCDBusTransceiverOnOffCheckBox.CheckedChanged += new EventHandler(CCDBusSettingsCheckBox_CheckedChanged);
                                        CCDBusStateString = "disabled";
                                    }
                                    else
                                    {
                                        CCDBusTransceiverOnOffCheckBox.CheckedChanged -= new EventHandler(CCDBusSettingsCheckBox_CheckedChanged); // unsubscribe from this event to set checkbox without raising said event
                                        CCDBusTransceiverOnOffCheckBox.Checked = true;
                                        CCDBusTransceiverOnOffCheckBox.Text = "CCD-bus transceiver ON";
                                        CCDBusTransceiverOnOffCheckBox.CheckedChanged += new EventHandler(CCDBusSettingsCheckBox_CheckedChanged); // re-subscribe to event
                                        CCDBusStateString = "enabled";
                                    }

                                    if (Util.IsBitClear(packet.Payload[15], 3)) CCDBusLogicString = "non-inverted";
                                    else CCDBusLogicString = "inverted";

                                    if (Util.IsBitClear(packet.Payload[15], 6))
                                    {
                                        CCDBusTerminationBiasOnOffCheckBox.CheckedChanged -= new EventHandler(this.CCDBusSettingsCheckBox_CheckedChanged);
                                        CCDBusTerminationBiasOnOffCheckBox.Checked = false;
                                        CCDBusTerminationBiasOnOffCheckBox.Text = "CCD-bus termination / bias OFF";
                                        CCDBusTerminationBiasOnOffCheckBox.CheckedChanged += new EventHandler(this.CCDBusSettingsCheckBox_CheckedChanged);
                                        CCDTerminationBiasEnabledString = "disabled";
                                    }
                                    else
                                    {
                                        CCDBusTerminationBiasOnOffCheckBox.CheckedChanged -= new EventHandler(this.CCDBusSettingsCheckBox_CheckedChanged);
                                        CCDBusTerminationBiasOnOffCheckBox.Checked = true;
                                        CCDBusTerminationBiasOnOffCheckBox.Text = "CCD-bus termination / bias ON";
                                        CCDBusTerminationBiasOnOffCheckBox.CheckedChanged += new EventHandler(this.CCDBusSettingsCheckBox_CheckedChanged);
                                        CCDTerminationBiasEnabledString = "enabled";
                                    }

                                    switch (packet.Payload[15] & 0x03)
                                    {
                                        case 0x00:
                                        {
                                            CCDBusSpeedString = "976.5 baud";
                                            break;
                                        }
                                        case 0x01:
                                        {
                                            CCDBusSpeedString = "7812.5 baud";
                                            break;
                                        }
                                        case 0x02:
                                        {
                                            CCDBusSpeedString = "62500 baud";
                                            break;
                                        }
                                        case 0x03:
                                        {
                                            CCDBusSpeedString = "125000 baud";
                                            break;
                                        }
                                        default:
                                        {
                                            CCDBusSpeedString = "unknown";
                                            break;
                                        }
                                    }

                                    CCD.UpdateHeader(CCDBusStateString, CCDBusSpeedString, CCDBusLogicString);

                                    CCDBusMsgRxCountString = ((packet.Payload[16] << 24) + (packet.Payload[17] << 16) + (packet.Payload[18] << 8) + packet.Payload[19]).ToString();
                                    CCDBusMsgTxCountString = ((packet.Payload[20] << 24) + (packet.Payload[21] << 16) + (packet.Payload[22] << 8) + packet.Payload[23]).ToString();

                                    string SCIBusPCMStateString = string.Empty;
                                    string SCIBusPCMLogicString = string.Empty;
                                    string SCIBusPCMNGCModeString = string.Empty;
                                    string SCIBusPCMSBEC2ModeString = string.Empty;
                                    string SCIBusPCMOBDConfigurationString = string.Empty;
                                    string SCIBusPCMSpeedString = string.Empty;
                                    string SCIBusPCMMsgRxCountString = string.Empty;
                                    string SCIBusPCMMsgTxCountString = string.Empty;

                                    if (Util.IsBitSet(packet.Payload[24], 7))
                                    {
                                        SCIBusPCMStateString = "enabled";

                                        if (Util.IsBitClear(packet.Payload[24], 4) &&
                                            Util.IsBitClear(packet.Payload[24], 3) &&
                                            Util.IsBitClear(packet.Payload[24], 6))
                                        {
                                            SCIBusLogicComboBox.SelectedIndex = 2; // OBD2

                                            SCIBusPCMNGCModeString = "disabled";
                                            SCIBusPCMLogicString += "non-inverted";
                                        }
                                        else
                                        {
                                            if (Util.IsBitSet(packet.Payload[24], 4))
                                            {
                                                SCIBusPCMNGCModeString = "enabled";
                                                SCIBusLogicComboBox.SelectedIndex = 3; // OBD2 NGC
                                            }
                                            else
                                            {
                                                SCIBusPCMNGCModeString = "disabled";
                                            }

                                            if (Util.IsBitSet(packet.Payload[24], 3))
                                            {
                                                SCIBusPCMLogicString += "inverted";
                                                SCIBusLogicComboBox.SelectedIndex = 1; // OBD1 JTEC
                                            }
                                            else
                                            {
                                                SCIBusPCMLogicString += "non-inverted";
                                            }

                                            if (Util.IsBitSet(packet.Payload[24], 6))
                                            {
                                                SCIBusPCMSBEC2ModeString += "(SBEC)";
                                                SCIBusLogicComboBox.SelectedIndex = 0; // OBD1 SBEC
                                            }
                                        }

                                        if (Util.IsBitClear(packet.Payload[24], 2))
                                        {
                                            SCIBusPCMOBDConfigurationString = "A";
                                            SCIBusOBDConfigurationComboBox.SelectedIndex = 0;
                                        }
                                        else
                                        {
                                            SCIBusPCMOBDConfigurationString = "B";
                                            SCIBusOBDConfigurationComboBox.SelectedIndex = 1;
                                        }

                                        switch (packet.Payload[24] & 0x03)
                                        {
                                            case 0x00:
                                            {
                                                SCIBusPCMSpeedString = "976.5 baud";
                                                SCIBusSpeedComboBox.SelectedIndex = 1;
                                                break;
                                            }
                                            case 0x01:
                                            {
                                                SCIBusPCMSpeedString = "7812.5 baud";
                                                SCIBusSpeedComboBox.SelectedIndex = 2;
                                                break;
                                            }
                                            case 0x02:
                                            {
                                                SCIBusPCMSpeedString = "62500 baud";
                                                SCIBusSpeedComboBox.SelectedIndex = 3;
                                                break;
                                            }
                                            case 0x03:
                                            {
                                                SCIBusPCMSpeedString = "125000 baud";
                                                SCIBusSpeedComboBox.SelectedIndex = 4;
                                                break;
                                            }
                                            default:
                                            {
                                                SCIBusPCMSpeedString = "unknown";
                                                SCIBusSpeedComboBox.SelectedIndex = 0;
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        SCIBusPCMStateString = "disabled";
                                        SCIBusPCMLogicString = "-";
                                        SCIBusPCMOBDConfigurationString = "-";
                                        SCIBusPCMSpeedString = "-";
                                        SCIBusSpeedComboBox.SelectedIndex = 0;
                                    }

                                    PCM.UpdateHeader(SCIBusPCMStateString, SCIBusPCMSpeedString, SCIBusPCMLogicString, SCIBusPCMOBDConfigurationString);

                                    SCIBusPCMMsgRxCountString = ((packet.Payload[25] << 24) + (packet.Payload[26] << 16) + (packet.Payload[27] << 8) + packet.Payload[28]).ToString();
                                    SCIBusPCMMsgTxCountString = ((packet.Payload[29] << 24) + (packet.Payload[30] << 16) + (packet.Payload[31] << 8) + packet.Payload[32]).ToString();

                                    string SCIBusTCMStateString = string.Empty;
                                    string SCIBusTCMLogicString = string.Empty;
                                    string SCIBusTCMNGCModeString = string.Empty;
                                    string SCIBusTCMSBEC2ModeString = string.Empty;
                                    string SCIBusTCMOBDConfigurationString = string.Empty;
                                    string SCIBusTCMSpeedString = string.Empty;
                                    string SCIBusTCMMsgRxCountString = string.Empty;
                                    string SCIBusTCMMsgTxCountString = string.Empty;

                                    if (Util.IsBitSet(packet.Payload[33], 7))
                                    {
                                        SCIBusTCMStateString = "enabled";

                                        if (Util.IsBitClear(packet.Payload[33], 4) &&
                                            Util.IsBitClear(packet.Payload[33], 3) &&
                                            Util.IsBitClear(packet.Payload[33], 6))
                                        {
                                            SCIBusLogicComboBox.SelectedIndex = 2; // OBD2

                                            SCIBusTCMNGCModeString = "disabled";
                                            SCIBusTCMLogicString += "non-inverted";
                                        }
                                        else
                                        {
                                            if (Util.IsBitSet(packet.Payload[33], 4))
                                            {
                                                SCIBusPCMNGCModeString = "enabled";
                                                SCIBusLogicComboBox.SelectedIndex = 3; // OBD2 NGC
                                            }
                                            else
                                            {
                                                SCIBusPCMNGCModeString = "disabled";
                                            }

                                            if (Util.IsBitSet(packet.Payload[33], 3))
                                            {
                                                SCIBusPCMLogicString += "inverted";
                                                SCIBusLogicComboBox.SelectedIndex = 1; // OBD1 JTEC
                                            }
                                            else
                                            {
                                                SCIBusPCMLogicString += "non-inverted";
                                            }

                                            if (Util.IsBitSet(packet.Payload[33], 6))
                                            {
                                                SCIBusPCMSBEC2ModeString += "(SBEC)";
                                                SCIBusLogicComboBox.SelectedIndex = 0; // OBD1 SBEC
                                            }
                                        }

                                        if (Util.IsBitClear(packet.Payload[33], 2))
                                        {
                                            SCIBusTCMOBDConfigurationString = "A";
                                            SCIBusOBDConfigurationComboBox.SelectedIndex = 0;
                                        }
                                        else
                                        {
                                            SCIBusTCMOBDConfigurationString = "B";
                                            SCIBusOBDConfigurationComboBox.SelectedIndex = 1;
                                        }

                                        switch (packet.Payload[33] & 0x03)
                                        {
                                            case 0x00:
                                            {
                                                SCIBusTCMSpeedString = "976.5 baud";
                                                SCIBusSpeedComboBox.SelectedIndex = 1;
                                                break;
                                            }
                                            case 0x01:
                                            {
                                                SCIBusTCMSpeedString = "7812.5 baud";
                                                SCIBusSpeedComboBox.SelectedIndex = 2;
                                                break;
                                            }
                                            case 0x02:
                                            {
                                                SCIBusTCMSpeedString = "62500 baud";
                                                SCIBusSpeedComboBox.SelectedIndex = 3;
                                                break;
                                            }
                                            case 0x03:
                                            {
                                                SCIBusTCMSpeedString = "125000 baud";
                                                SCIBusSpeedComboBox.SelectedIndex = 4;
                                                break;
                                            }
                                            default:
                                            {
                                                SCIBusTCMSpeedString = "unknown";
                                                SCIBusSpeedComboBox.SelectedIndex = 0;
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        SCIBusTCMStateString = "disabled";
                                        SCIBusTCMLogicString = "-";
                                        SCIBusTCMOBDConfigurationString = "-";
                                        SCIBusTCMSpeedString = "-";
                                    }

                                    TCM.UpdateHeader(SCIBusTCMStateString, SCIBusTCMSpeedString, SCIBusTCMLogicString, SCIBusTCMOBDConfigurationString);

                                    SCIBusTCMMsgRxCountString = ((packet.Payload[34] << 24) + (packet.Payload[35] << 16) + (packet.Payload[36] << 8) + packet.Payload[37]).ToString();
                                    SCIBusTCMMsgTxCountString = ((packet.Payload[38] << 24) + (packet.Payload[39] << 16) + (packet.Payload[40] << 8) + packet.Payload[41]).ToString();

                                    string LCDStatusString = string.Empty;
                                    string LCDI2CAddressString = string.Empty;
                                    string LCDSizeString = string.Empty;
                                    string LCDRefreshRateString = string.Empty;
                                    string LCDUnitsString = string.Empty;
                                    string LCDDataSourceString = string.Empty;

                                    if (packet.Payload[42] == 0x00)
                                    {
                                        LCDStateComboBox.SelectedIndex = 0;
                                        LCDStatusString = "disabled";
                                    }
                                    else
                                    {
                                        LCDStateComboBox.SelectedIndex = 1;
                                        LCDStatusString = "enabled";
                                    }

                                    LCDI2CAddressTextBox.Text = Util.ByteToHexString(packet.Payload, 43, 1);
                                    LCDI2CAddressString = Util.ByteToHexString(packet.Payload, 43, 1) + " (hex)";

                                    LCDWidthTextBox.Text = packet.Payload[44].ToString("0");
                                    LCDHeightTextBox.Text = packet.Payload[45].ToString("0");
                                    LCDSizeString = packet.Payload[44].ToString("0") + "x" + packet.Payload[45].ToString("0") + " characters";

                                    LCDRefreshRateTextBox.Text = packet.Payload[46].ToString("0");
                                    LCDRefreshRateString = packet.Payload[46].ToString("0") + " Hz";

                                    if (packet.Payload[47] == 0x00)
                                    {
                                        LCDUnitsString = "imperial";
                                    }
                                    else
                                    {
                                        LCDUnitsString = "metric";
                                    }

                                    switch (packet.Payload[48])
                                    {
                                        case 0x01:
                                        {
                                            LCDDataSourceString = "CCD-bus";
                                            LCDDataSourceComboBox.SelectedIndex = 0;
                                            break;
                                        }
                                        case 0x02:
                                        {
                                            LCDDataSourceString = "SCI-bus (PCM)";
                                            LCDDataSourceComboBox.SelectedIndex = 1;
                                            break;
                                        }
                                        case 0x03:
                                        {
                                            LCDDataSourceString = "SCI-bus (TCM)";
                                            LCDDataSourceComboBox.SelectedIndex = 2;
                                            break;
                                        }
                                        default:
                                        {
                                            LCDDataSourceString = "CCD-bus";
                                            LCDDataSourceComboBox.SelectedIndex = 0;
                                            break;
                                        }
                                    }

                                    ushort LEDHeartbeatInterval = (ushort)((packet.Payload[49] << 8) + packet.Payload[50]);
                                    ushort LEDBlinkDuration = (ushort)((packet.Payload[51] << 8) + packet.Payload[52]);

                                    string LEDHeartbeatStateString = string.Empty;
                                    if (LEDHeartbeatInterval > 0) LEDHeartbeatStateString = "enabled";
                                    else LEDHeartbeatStateString = "disabled";

                                    string LEDBlinkDurationString = LEDBlinkDuration.ToString() + " ms";
                                    string LEDHeartbeatIntervalString = LEDHeartbeatInterval.ToString() + " ms";
                                    HeartbeatIntervalTextBox.Text = LEDHeartbeatInterval.ToString();
                                    LEDBlinkDurationTextBox.Text = LEDBlinkDuration.ToString();

                                    Util.UpdateTextBox(USBTextBox, "[RX->] Status response:", PacketHelper.Serialize(packet));
                                    Util.UpdateTextBox(USBTextBox, "[INFO] -------------Device status--------------" + Environment.NewLine +
                                                                    "       AVR signature: " + AVRSignature + Environment.NewLine +
                                                                    "       External EEPROM present: " + extEEPROMPresent + Environment.NewLine +
                                                                    "       External EEPROM checksum: " + extEEPROMChecksum + Environment.NewLine +
                                                                    "       Timestamp: " + microcontrollerTimestampString + Environment.NewLine +
                                                                    "       RAM usage: " + freeRAMString + Environment.NewLine +
                                                                    "       Connected to vehicle: " + connectedToVehicle + Environment.NewLine +
                                                                    "       Battery voltage: " + batteryVoltageString + Environment.NewLine +
                                                                    "       -------------CCD-bus status-------------" + Environment.NewLine +
                                                                    "       State: " + CCDBusStateString + Environment.NewLine +
                                                                    "       Logic: " + CCDBusLogicString + Environment.NewLine +
                                                                    "       Termination and bias: " + CCDTerminationBiasEnabledString + Environment.NewLine +
                                                                    "       Speed: " + CCDBusSpeedString + Environment.NewLine +
                                                                    "       Messages received: " + CCDBusMsgRxCountString + Environment.NewLine +
                                                                    "       Messages sent: " + CCDBusMsgTxCountString + Environment.NewLine +
                                                                    "       ----------SCI-bus (PCM) status----------" + Environment.NewLine +
                                                                    "       State: " + SCIBusPCMStateString + Environment.NewLine +
                                                                    "       Logic: " + SCIBusPCMLogicString + " " + SCIBusPCMSBEC2ModeString + Environment.NewLine +
                                                                    "       NGC mode: " + SCIBusPCMNGCModeString + Environment.NewLine +
                                                                    "       OBD config.: " + SCIBusPCMOBDConfigurationString + Environment.NewLine +
                                                                    "       Speed: " + SCIBusPCMSpeedString + Environment.NewLine +
                                                                    "       Messages received: " + SCIBusPCMMsgRxCountString + Environment.NewLine +
                                                                    "       Messages sent: " + SCIBusPCMMsgTxCountString + Environment.NewLine +
                                                                    "       ----------SCI-bus (TCM) status----------" + Environment.NewLine +
                                                                    "       State: " + SCIBusTCMStateString + Environment.NewLine +
                                                                    "       Logic: " + SCIBusTCMLogicString + " " + SCIBusTCMSBEC2ModeString + Environment.NewLine +
                                                                    "       NGC mode: " + SCIBusTCMNGCModeString + Environment.NewLine +
                                                                    "       OBD config.: " + SCIBusTCMOBDConfigurationString + Environment.NewLine +
                                                                    "       Speed: " + SCIBusTCMSpeedString + Environment.NewLine +
                                                                    "       Messages received: " + SCIBusTCMMsgRxCountString + Environment.NewLine +
                                                                    "       Messages sent: " + SCIBusTCMMsgTxCountString + Environment.NewLine +
                                                                    "       ---------------LCD status---------------" + Environment.NewLine +
                                                                    "       State: " + LCDStatusString + Environment.NewLine +
                                                                    "       I2C address: " + LCDI2CAddressString + Environment.NewLine +
                                                                    "       Size: " + LCDSizeString + Environment.NewLine +
                                                                    "       Refresh rate: " + LCDRefreshRateString + Environment.NewLine +
                                                                    "       Units: " + LCDUnitsString + Environment.NewLine +
                                                                    "       Data source: " + LCDDataSourceString + Environment.NewLine +
                                                                    "       ---------------LED status---------------" + Environment.NewLine +
                                                                    "       Heartbeat state: " + LEDHeartbeatStateString + Environment.NewLine +
                                                                    "       Heartbeat interval: " + LEDHeartbeatIntervalString + Environment.NewLine +
                                                                    "       Blink duration: " + LEDBlinkDurationString);

                                    //UpdateLCDPreviewTextBox();
                                }
                                else if (HWVersion.Contains("v2.") && (packet.Payload.Length >= 45))
                                {
                                    TimeSpan elapsedMillis = TimeSpan.FromMilliseconds((packet.Payload[0] << 24) + (packet.Payload[1] << 16) + (packet.Payload[2] << 8) + packet.Payload[3]);
                                    DateTime microcontrollerTimestamp = DateTime.Today.Add(elapsedMillis);
                                    string microcontrollerTimestampString = microcontrollerTimestamp.ToString("HH:mm:ss.fff");

                                    int freeRAM = (packet.Payload[4] << 24) + (packet.Payload[5] << 16) + (packet.Payload[6] << 8) + packet.Payload[7];
                                    string freeRAMString = (100.0 * (freeRAM / 532480.0)).ToString("0.0") + "% (" + freeRAM.ToString("0") + "/532480 bytes)";
                                    string batteryVoltageString = (((packet.Payload[8] << 8) + packet.Payload[9]) / 1000.0).ToString("0.000").Replace(",", ".") + " V";
                                    string bootstrapVoltageString = (((packet.Payload[10] << 8) + packet.Payload[11]) / 1000.0).ToString("0.000").Replace(",", ".") + " V";
                                    string programmingVoltageString = (((packet.Payload[12] << 8) + packet.Payload[13]) / 1000.0).ToString("0.000").Replace(",", ".") + " V";

                                    string CCDBusStateString = string.Empty;
                                    string CCDBusLogicString = string.Empty;
                                    string CCDTerminationBiasEnabledString = string.Empty;
                                    string CCDBusSpeedString = string.Empty;
                                    string CCDBusMsgRxCountString = string.Empty;
                                    string CCDBusMsgTxCountString = string.Empty;

                                    if (Util.IsBitClear(packet.Payload[14], 7))
                                    {
                                        CCDBusTransceiverOnOffCheckBox.CheckedChanged -= new EventHandler(CCDBusSettingsCheckBox_CheckedChanged);
                                        CCDBusTransceiverOnOffCheckBox.Checked = false;
                                        CCDBusTransceiverOnOffCheckBox.Text = "CCD-bus transceiver OFF";
                                        CCDBusTransceiverOnOffCheckBox.CheckedChanged += new EventHandler(CCDBusSettingsCheckBox_CheckedChanged);
                                        CCDBusStateString = "disabled";
                                    }
                                    else
                                    {
                                        CCDBusTransceiverOnOffCheckBox.CheckedChanged -= new EventHandler(CCDBusSettingsCheckBox_CheckedChanged); // unsubscribe from this event to set checkbox without raising said event
                                        CCDBusTransceiverOnOffCheckBox.Checked = true;
                                        CCDBusTransceiverOnOffCheckBox.Text = "CCD-bus transceiver ON";
                                        CCDBusTransceiverOnOffCheckBox.CheckedChanged += new EventHandler(CCDBusSettingsCheckBox_CheckedChanged); // re-subscribe to event
                                        CCDBusStateString = "enabled";
                                    }

                                    if (Util.IsBitClear(packet.Payload[14], 3)) CCDBusLogicString = "non-inverted";
                                    else CCDBusLogicString = "inverted";

                                    if (Util.IsBitClear(packet.Payload[14], 6))
                                    {
                                        CCDBusTerminationBiasOnOffCheckBox.CheckedChanged -= new EventHandler(this.CCDBusSettingsCheckBox_CheckedChanged);
                                        CCDBusTerminationBiasOnOffCheckBox.Checked = false;
                                        CCDBusTerminationBiasOnOffCheckBox.Text = "CCD-bus termination / bias OFF";
                                        CCDBusTerminationBiasOnOffCheckBox.CheckedChanged += new EventHandler(this.CCDBusSettingsCheckBox_CheckedChanged);
                                        CCDTerminationBiasEnabledString = "disabled";
                                    }
                                    else
                                    {
                                        CCDBusTerminationBiasOnOffCheckBox.CheckedChanged -= new EventHandler(this.CCDBusSettingsCheckBox_CheckedChanged);
                                        CCDBusTerminationBiasOnOffCheckBox.Checked = true;
                                        CCDBusTerminationBiasOnOffCheckBox.Text = "CCD-bus termination / bias ON";
                                        CCDBusTerminationBiasOnOffCheckBox.CheckedChanged += new EventHandler(this.CCDBusSettingsCheckBox_CheckedChanged);
                                        CCDTerminationBiasEnabledString = "enabled";
                                    }

                                    switch (packet.Payload[14] & 0x03)
                                    {
                                        case 0x00:
                                        {
                                            CCDBusSpeedString = "976.5 baud";
                                            break;
                                        }
                                        case 0x01:
                                        {
                                            CCDBusSpeedString = "7812.5 baud";
                                            break;
                                        }
                                        case 0x02:
                                        {
                                            CCDBusSpeedString = "62500 baud";
                                            break;
                                        }
                                        case 0x03:
                                        {
                                            CCDBusSpeedString = "125000 baud";
                                            break;
                                        }
                                        default:
                                        {
                                            CCDBusSpeedString = "unknown";
                                            break;
                                        }
                                    }

                                    CCD.UpdateHeader(CCDBusStateString, CCDBusSpeedString, CCDBusLogicString);

                                    CCDBusMsgRxCountString = ((packet.Payload[15] << 24) + (packet.Payload[16] << 16) + (packet.Payload[17] << 8) + packet.Payload[18]).ToString();
                                    CCDBusMsgTxCountString = ((packet.Payload[19] << 24) + (packet.Payload[20] << 16) + (packet.Payload[21] << 8) + packet.Payload[22]).ToString();

                                    string PCIBusStateString = string.Empty;
                                    string PCIBusLogicString = string.Empty;
                                    string PCIBusSpeedString = string.Empty;
                                    string PCIBusMsgRxCountString = string.Empty;
                                    string PCIBusMsgTxCountString = string.Empty;

                                    if (Util.IsBitClear(packet.Payload[23], 7))
                                    {
                                        PCIBusTransceiverOnOffCheckBox.CheckedChanged -= new EventHandler(PCIBusSettingsCheckBox_CheckedChanged);
                                        PCIBusTransceiverOnOffCheckBox.Checked = false;
                                        PCIBusTransceiverOnOffCheckBox.Text = "PCI-bus transceiver OFF";
                                        PCIBusTransceiverOnOffCheckBox.CheckedChanged += new EventHandler(PCIBusSettingsCheckBox_CheckedChanged);
                                        PCIBusStateString = "disabled";
                                    }
                                    else
                                    {
                                        PCIBusTransceiverOnOffCheckBox.CheckedChanged -= new EventHandler(PCIBusSettingsCheckBox_CheckedChanged);
                                        PCIBusTransceiverOnOffCheckBox.Checked = true;
                                        PCIBusTransceiverOnOffCheckBox.Text = "PCI-bus transceiver ON";
                                        PCIBusTransceiverOnOffCheckBox.CheckedChanged += new EventHandler(PCIBusSettingsCheckBox_CheckedChanged);
                                        PCIBusStateString = "enabled";
                                    }

                                    if (Util.IsBitClear(packet.Payload[23], 6)) PCIBusLogicString = "active-low";
                                    else PCIBusLogicString = "active-high";

                                    PCIBusSpeedString = "10416 baud";

                                    PCI.UpdateHeader(PCIBusStateString, PCIBusSpeedString, PCIBusLogicString);

                                    PCIBusMsgRxCountString = ((packet.Payload[24] << 24) + (packet.Payload[25] << 16) + (packet.Payload[26] << 8) + packet.Payload[27]).ToString();
                                    PCIBusMsgTxCountString = ((packet.Payload[28] << 24) + (packet.Payload[29] << 16) + (packet.Payload[30] << 8) + packet.Payload[31]).ToString();

                                    string SCIBusStateString = string.Empty;
                                    string SCIBusModuleString = string.Empty;
                                    string SCIBusLogicString = string.Empty;
                                    string SCIBusNGCModeString = string.Empty;
                                    string SCIBusSBEC2ModeString = string.Empty;
                                    string SCIBusOBDConfigurationString = string.Empty;
                                    string SCIBusSpeedString = string.Empty;
                                    string SCIBusMsgRxCountString = string.Empty;
                                    string SCIBusMsgTxCountString = string.Empty;

                                    if (Util.IsBitSet(packet.Payload[32], 7))
                                    {
                                        SCIBusStateString = "enabled";

                                        if (Util.IsBitClear(packet.Payload[32], 4) &&
                                            Util.IsBitClear(packet.Payload[32], 3) &&
                                            Util.IsBitClear(packet.Payload[32], 6))
                                        {
                                            SCIBusLogicComboBox.SelectedIndex = 2; // OBD2

                                            SCIBusNGCModeString = "disabled";
                                            SCIBusLogicString += "non-inverted";
                                        }
                                        else
                                        {
                                            if (Util.IsBitSet(packet.Payload[32], 4))
                                            {
                                                SCIBusNGCModeString = "enabled";
                                                SCIBusLogicComboBox.SelectedIndex = 3; // OBD2 NGC
                                            }
                                            else
                                            {
                                                SCIBusNGCModeString = "disabled";
                                            }

                                            if (Util.IsBitSet(packet.Payload[32], 3))
                                            {
                                                SCIBusLogicString += "inverted";
                                                SCIBusLogicComboBox.SelectedIndex = 1; // OBD1 JTEC
                                            }
                                            else
                                            {
                                                SCIBusLogicString += "non-inverted";
                                            }

                                            if (Util.IsBitSet(packet.Payload[32], 6))
                                            {
                                                SCIBusSBEC2ModeString += "(SBEC)";
                                                SCIBusLogicComboBox.SelectedIndex = 0; // OBD1 SBEC
                                            }
                                        }

                                        if (Util.IsBitClear(packet.Payload[32], 2))
                                        {
                                            SCIBusOBDConfigurationString = "A";
                                            SCIBusOBDConfigurationComboBox.SelectedIndex = 0;
                                        }
                                        else
                                        {
                                            SCIBusOBDConfigurationString = "B";
                                            SCIBusOBDConfigurationComboBox.SelectedIndex = 1;
                                        }

                                        switch (packet.Payload[32] & 0x03)
                                        {
                                            case 0x00:
                                            {
                                                SCIBusSpeedString = "976.5 baud";
                                                SCIBusSpeedComboBox.SelectedIndex = 1;
                                                break;
                                            }
                                            case 0x01:
                                            {
                                                SCIBusSpeedString = "7812.5 baud";
                                                SCIBusSpeedComboBox.SelectedIndex = 2;
                                                break;
                                            }
                                            case 0x02:
                                            {
                                                SCIBusSpeedString = "62500 baud";
                                                SCIBusSpeedComboBox.SelectedIndex = 3;
                                                break;
                                            }
                                            case 0x03:
                                            {
                                                SCIBusSpeedString = "125000 baud";
                                                SCIBusSpeedComboBox.SelectedIndex = 4;
                                                break;
                                            }
                                            default:
                                            {
                                                SCIBusSpeedString = "unknown";
                                                SCIBusSpeedComboBox.SelectedIndex = 0;
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        SCIBusStateString = "disabled";
                                        SCIBusLogicString = "-";
                                        SCIBusOBDConfigurationString = "-";
                                        SCIBusSpeedString = "-";
                                        SCIBusSpeedComboBox.SelectedIndex = 0;
                                    }

                                    SCIBusMsgRxCountString = ((packet.Payload[33] << 24) + (packet.Payload[34] << 16) + (packet.Payload[35] << 8) + packet.Payload[36]).ToString();
                                    SCIBusMsgTxCountString = ((packet.Payload[37] << 24) + (packet.Payload[38] << 16) + (packet.Payload[39] << 8) + packet.Payload[40]).ToString();

                                    if (Util.IsBitClear(packet.Payload[32], 5)) // PCM
                                    {
                                        SCIBusModuleString = "PCM (Engine)";
                                        SCIBusModuleComboBox.SelectedIndex = 0;
                                        PCM.UpdateHeader(SCIBusStateString, SCIBusSpeedString, SCIBusLogicString, SCIBusOBDConfigurationString);
                                        TCM.UpdateHeader("disabled", "-", "-", "-");
                                    }
                                    else // TCM
                                    {
                                        SCIBusModuleString = "TCM (Transmission)";
                                        SCIBusModuleComboBox.SelectedIndex = 1;
                                        TCM.UpdateHeader(SCIBusStateString, SCIBusSpeedString, SCIBusLogicString, SCIBusOBDConfigurationString);
                                        PCM.UpdateHeader("disabled", "-", "-", "-");
                                    }

                                    ushort LEDHeartbeatInterval = (ushort)((packet.Payload[41] << 8) + packet.Payload[42]);
                                    ushort LEDBlinkDuration = (ushort)((packet.Payload[43] << 8) + packet.Payload[44]);

                                    string LEDHeartbeatStateString = string.Empty;
                                    if (LEDHeartbeatInterval > 0) LEDHeartbeatStateString = "enabled";
                                    else LEDHeartbeatStateString = "disabled";

                                    string LEDBlinkDurationString = LEDBlinkDuration.ToString() + " ms";
                                    string LEDHeartbeatIntervalString = LEDHeartbeatInterval.ToString() + " ms";
                                    HeartbeatIntervalTextBox.Text = LEDHeartbeatInterval.ToString();
                                    LEDBlinkDurationTextBox.Text = LEDBlinkDuration.ToString();

                                    Util.UpdateTextBox(USBTextBox, "[RX->] Status response:", PacketHelper.Serialize(packet));
                                    Util.UpdateTextBox(USBTextBox, "[INFO] -------------Device status--------------" + Environment.NewLine +
                                                                    "       Timestamp: " + microcontrollerTimestampString + Environment.NewLine +
                                                                    "       RAM usage: " + freeRAMString + Environment.NewLine +
                                                                    "       Battery voltage: " + batteryVoltageString + Environment.NewLine +
                                                                    "       Bootstrap voltage: " + bootstrapVoltageString + Environment.NewLine +
                                                                    "       Programming voltage: " + programmingVoltageString + Environment.NewLine +
                                                                    "       -------------CCD-bus status-------------" + Environment.NewLine +
                                                                    "       State: " + CCDBusStateString + Environment.NewLine +
                                                                    "       Logic: " + CCDBusLogicString + Environment.NewLine +
                                                                    "       Termination and bias: " + CCDTerminationBiasEnabledString + Environment.NewLine +
                                                                    "       Speed: " + CCDBusSpeedString + Environment.NewLine +
                                                                    "       Messages received: " + CCDBusMsgRxCountString + Environment.NewLine +
                                                                    "       Messages sent: " + CCDBusMsgTxCountString + Environment.NewLine +
                                                                    "       -------------PCI-bus status-------------" + Environment.NewLine +
                                                                    "       State: " + PCIBusStateString + Environment.NewLine +
                                                                    "       Logic: " + PCIBusLogicString + Environment.NewLine +
                                                                    "       Speed: " + PCIBusSpeedString + Environment.NewLine +
                                                                    "       Messages received: " + PCIBusMsgRxCountString + Environment.NewLine +
                                                                    "       Messages sent: " + PCIBusMsgTxCountString + Environment.NewLine +
                                                                    "       -------------SCI-bus status-------------" + Environment.NewLine +
                                                                    "       Module: " + SCIBusModuleString + Environment.NewLine +
                                                                    "       State: " + SCIBusStateString + Environment.NewLine +
                                                                    "       Logic: " + SCIBusLogicString + " " + SCIBusSBEC2ModeString + Environment.NewLine +
                                                                    "       NGC mode: " + SCIBusNGCModeString + Environment.NewLine +
                                                                    "       OBD config.: " + SCIBusOBDConfigurationString + Environment.NewLine +
                                                                    "       Speed: " + SCIBusSpeedString + Environment.NewLine +
                                                                    "       Messages received: " + SCIBusMsgRxCountString + Environment.NewLine +
                                                                    "       Messages sent: " + SCIBusMsgTxCountString + Environment.NewLine +
                                                                    "       ---------------LED status---------------" + Environment.NewLine +
                                                                    "       Heartbeat state: " + LEDHeartbeatStateString + Environment.NewLine +
                                                                    "       Heartbeat interval: " + LEDHeartbeatIntervalString + Environment.NewLine +
                                                                    "       Blink duration: " + LEDBlinkDurationString);

                                    BeginInvoke((MethodInvoker)delegate
                                    {
                                        EEPROMChecksumButton.Visible = false;
                                        InternalEEPROMRadioButton.Visible = false;
                                        ExternalEEPROMRadioButton.Visible = false;
                                        DebugLabel.Visible = false;
                                        ReadEEPROMButton.Visible = false;
                                        EEPROMReadAddressLabel.Visible = false;
                                        EEPROMReadAddressTextBox.Visible = false;
                                        EEPROMReadCountLabel.Visible = false;
                                        EEPROMReadCountTextBox.Visible = false;
                                        WriteEEPROMButton.Visible = false;
                                        EEPROMWriteAddressLabel.Visible = false;
                                        EEPROMWriteAddressTextBox.Visible = false;
                                        EEPROMWriteEnableCheckBox.Visible = false;
                                        EEPROMWriteValuesLabel.Visible = false;
                                        EEPROMWriteValuesTextBox.Visible = false;
                                        MeasureCCDBusVoltagesButton.Visible = false;
                                        CCDBusTerminationBiasOnOffCheckBox.Enabled = false;
                                        LCDApplySettingsButton.Enabled = false;
                                    });
                                }
                                break;
                            }
                            case (byte)PacketHelper.Command.Settings:
                            {
                                switch (packet.Mode)
                                {
                                    case (byte)PacketHelper.SettingsMode.LEDs:
                                    {
                                        if ((packet.Payload != null) && (packet.Payload.Length >= 4))
                                        {
                                            Util.UpdateTextBox(USBTextBox, "[RX->] LED settings changed:", PacketHelper.Serialize(packet));

                                            int heartbeatInterval = (packet.Payload[0] << 8) + packet.Payload[1];
                                            int blinkDuration = (packet.Payload[2] << 8) + packet.Payload[3];
                                            string heartbeatIntervalString = heartbeatInterval.ToString() + " ms";
                                            string blinkDurationString = blinkDuration.ToString() + " ms";

                                            string heartbeatStateString = string.Empty;
                                            if (heartbeatInterval > 0) heartbeatStateString = "enabled";
                                            else heartbeatStateString = "disabled";

                                            Util.UpdateTextBox(USBTextBox, "[INFO] LED settings:" + Environment.NewLine +
                                                                           "       Heartbeat state: " + heartbeatStateString + Environment.NewLine +
                                                                           "       Heartbeat interval: " + heartbeatIntervalString + Environment.NewLine +
                                                                           "       LED blink duration: " + blinkDurationString);
                                        }
                                        else
                                        {
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Invalid settings packet:", PacketHelper.Serialize(packet));
                                        }
                                        break;
                                    }
                                    case (byte)PacketHelper.SettingsMode.SetCCDBus:
                                    {
                                        if ((packet.Payload != null) && (packet.Payload.Length > 0))
                                        {
                                            string CCDBusStateString = string.Empty;
                                            string CCDBusTerminationBiasString = string.Empty;

                                            if (Util.IsBitClear(packet.Payload[0], 7))
                                            {
                                                CCDBusStateString = "disabled";
                                            }
                                            else
                                            {
                                                CCDBusStateString = "enabled";
                                            }

                                            if (Util.IsBitClear(packet.Payload[0], 6))
                                            {
                                                CCDBusTerminationBiasString = "disabled";
                                            }
                                            else
                                            {
                                                CCDBusTerminationBiasString = "enabled";
                                            }

                                            Util.UpdateTextBox(USBTextBox, "[RX->] CCD-bus settings changed:", PacketHelper.Serialize(packet));
                                            Util.UpdateTextBox(USBTextBox, "[INFO] CCD-bus settings: " + Environment.NewLine +
                                                                           "       - state: " + CCDBusStateString + Environment.NewLine +
                                                                           "       - termination and bias: " + CCDBusTerminationBiasString);

                                            CCD.UpdateHeader(CCDBusStateString, null, null);
                                        }
                                        else
                                        {
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Invalid settings packet:", PacketHelper.Serialize(packet));
                                        }
                                        break;
                                    }
                                    case (byte)PacketHelper.SettingsMode.SetSCIBus:
                                    {
                                        if ((packet.Payload != null) && (packet.Payload.Length > 0))
                                        {
                                            string SCIBusPCMStateString = string.Empty;
                                            string SCIBusPCMLogicString = string.Empty;
                                            string SCIBusPCMNGCModeString = string.Empty;
                                            string SCIBusPCMSBEC2ModeString = string.Empty;
                                            string SCIBusPCMOBDConfigurationString = string.Empty;
                                            string SCIBusPCMSpeedString = string.Empty;
                                            string SCIBusTCMStateString = string.Empty;
                                            string SCIBusTCMLogicString = string.Empty;
                                            string SCIBusTCMNGCModeString = string.Empty;
                                            string SCIBusTCMSBEC2ModeString = string.Empty;
                                            string SCIBusTCMOBDConfigurationString = string.Empty;
                                            string SCIBusTCMSpeedString = string.Empty;

                                            Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus settings changed:", PacketHelper.Serialize(packet));

                                            if (Util.IsBitSet(packet.Payload[0], 5)) // TCM settings received
                                            {
                                                if (Util.IsBitClear(packet.Payload[0], 7))
                                                {
                                                    SCIBusTCMStateString = "disabled";
                                                }
                                                else
                                                {
                                                    PCMSelected = false;
                                                    TCMSelected = true;
                                                    SCIBusTCMStateString = "enabled";
                                                    SCIBusPCMStateString = "disabled";

                                                    if (Util.IsBitClear(packet.Payload[0], 4) &&
                                                        Util.IsBitClear(packet.Payload[0], 3) &&
                                                        Util.IsBitClear(packet.Payload[0], 6))
                                                    {
                                                        BeginInvoke((MethodInvoker)delegate
                                                        {
                                                            SCIBusLogicComboBox.SelectedIndex = 2; // OBD2
                                                        });

                                                        SCIBusTCMNGCModeString = "disabled";
                                                        SCIBusTCMLogicString += "non-inverted";
                                                    }
                                                    else
                                                    {
                                                        if (Util.IsBitSet(packet.Payload[0], 4))
                                                        {
                                                            SCIBusTCMNGCModeString = "enabled";

                                                            BeginInvoke((MethodInvoker)delegate
                                                            {
                                                                SCIBusLogicComboBox.SelectedIndex = 3; // OBD2 NGC
                                                            });
                                                        }
                                                        else
                                                        {
                                                            SCIBusTCMNGCModeString = "disabled";
                                                        }

                                                        if (Util.IsBitSet(packet.Payload[0], 3))
                                                        {
                                                            SCIBusTCMLogicString += "inverted";

                                                            BeginInvoke((MethodInvoker)delegate
                                                            {
                                                                SCIBusLogicComboBox.SelectedIndex = 1; // OBD1 JTEC
                                                            });
                                                        }
                                                        else
                                                        {
                                                            SCIBusTCMLogicString += "non-inverted";
                                                        }

                                                        if (Util.IsBitSet(packet.Payload[0], 6))
                                                        {
                                                            SCIBusTCMSBEC2ModeString += "(SBEC)";

                                                            BeginInvoke((MethodInvoker)delegate
                                                            {
                                                                SCIBusLogicComboBox.SelectedIndex = 0; // OBD1 SBEC
                                                            });
                                                        }
                                                    }

                                                    if (Util.IsBitClear(packet.Payload[0], 2))
                                                    {
                                                        SCIBusTCMOBDConfigurationString = "A";
                                                    }
                                                    else
                                                    {
                                                        SCIBusTCMOBDConfigurationString = "B";
                                                    }

                                                    switch (packet.Payload[0] & 0x03)
                                                    {
                                                        case 0x00:
                                                        {
                                                            SCIBusTCMSpeedString = "976.5 baud";

                                                            BeginInvoke((MethodInvoker)delegate
                                                            {
                                                                SCIBusSpeedComboBox.SelectedIndex = 1;
                                                            });
                                                            break;
                                                        }
                                                        case 0x01:
                                                        {
                                                            SCIBusTCMSpeedString = "7812.5 baud";

                                                            BeginInvoke((MethodInvoker)delegate
                                                            {
                                                                SCIBusSpeedComboBox.SelectedIndex = 2;
                                                            });
                                                            break;
                                                        }
                                                        case 0x02:
                                                        {
                                                            SCIBusTCMSpeedString = "62500 baud";

                                                            BeginInvoke((MethodInvoker)delegate
                                                            {
                                                                SCIBusSpeedComboBox.SelectedIndex = 3;
                                                            });
                                                            break;
                                                        }
                                                        case 0x03:
                                                        {
                                                            SCIBusTCMSpeedString = "125000 baud";

                                                            BeginInvoke((MethodInvoker)delegate
                                                            {
                                                                SCIBusSpeedComboBox.SelectedIndex = 4;
                                                            });
                                                            break;
                                                        }
                                                    }
                                                }

                                                if (SCIBusTCMStateString == "enabled")
                                                {
                                                    Util.UpdateTextBox(USBTextBox, "[INFO] TCM settings: " + Environment.NewLine +
                                                                                   "       - state: " + SCIBusTCMStateString + Environment.NewLine +
                                                                                   "       - logic: " + SCIBusTCMLogicString + " " + SCIBusTCMSBEC2ModeString + Environment.NewLine +
                                                                                   "       - ngc mode: " + SCIBusTCMNGCModeString + Environment.NewLine +
                                                                                   "       - obd config.: " + SCIBusTCMOBDConfigurationString + Environment.NewLine +
                                                                                   "       - speed: " + SCIBusTCMSpeedString + Environment.NewLine +
                                                                                   "       PCM settings: " + Environment.NewLine +
                                                                                   "       - state: disabled");
                                                }
                                                else
                                                {
                                                    Util.UpdateTextBox(USBTextBox, "[INFO] TCM settings: " + Environment.NewLine +
                                                                                    "       - state: " + SCIBusTCMStateString);
                                                }

                                                TCM.UpdateHeader(SCIBusTCMStateString, SCIBusTCMSpeedString, SCIBusTCMLogicString, SCIBusTCMOBDConfigurationString);
                                                PCM.UpdateHeader(SCIBusPCMStateString, SCIBusPCMSpeedString, SCIBusPCMLogicString, SCIBusPCMOBDConfigurationString);
                                            }
                                            else // PCM settings received
                                            {
                                                if (Util.IsBitClear(packet.Payload[0], 7))
                                                {
                                                    SCIBusPCMStateString = "disabled";
                                                }
                                                else
                                                {
                                                    PCMSelected = true;
                                                    TCMSelected = false;
                                                    SCIBusPCMStateString = "enabled";
                                                    SCIBusTCMStateString = "disabled";

                                                    if (Util.IsBitClear(packet.Payload[0], 4) &&
                                                        Util.IsBitClear(packet.Payload[0], 3) &&
                                                        Util.IsBitClear(packet.Payload[0], 6))
                                                    {
                                                        BeginInvoke((MethodInvoker)delegate
                                                        {
                                                            SCIBusLogicComboBox.SelectedIndex = 2; // OBD2
                                                        });

                                                        SCIBusPCMNGCModeString = "disabled";
                                                        SCIBusPCMLogicString += "non-inverted";
                                                    }
                                                    else
                                                    {
                                                        if (Util.IsBitSet(packet.Payload[0], 4))
                                                        {
                                                            SCIBusPCMNGCModeString = "enabled";

                                                            BeginInvoke((MethodInvoker)delegate
                                                            {
                                                                SCIBusLogicComboBox.SelectedIndex = 3; // OBD2 NGC
                                                            });
                                                        }
                                                        else
                                                        {
                                                            SCIBusPCMNGCModeString = "disabled";
                                                        }

                                                        if (Util.IsBitSet(packet.Payload[0], 3))
                                                        {
                                                            SCIBusPCMLogicString += "inverted";

                                                            BeginInvoke((MethodInvoker)delegate
                                                            {
                                                                SCIBusLogicComboBox.SelectedIndex = 1; // OBD1 JTEC
                                                            });
                                                        }
                                                        else
                                                        {
                                                            SCIBusPCMLogicString += "non-inverted";
                                                        }

                                                        if (Util.IsBitSet(packet.Payload[0], 6))
                                                        {
                                                            SCIBusPCMSBEC2ModeString += "(SBEC)";

                                                            BeginInvoke((MethodInvoker)delegate
                                                            {
                                                                SCIBusLogicComboBox.SelectedIndex = 0; // OBD1 SBEC
                                                            });
                                                        }
                                                    }

                                                    if (Util.IsBitClear(packet.Payload[0], 2))
                                                    {
                                                        SCIBusPCMOBDConfigurationString = "A";
                                                    }
                                                    else
                                                    {
                                                        SCIBusPCMOBDConfigurationString = "B";
                                                    }

                                                    switch (packet.Payload[0] & 0x03)
                                                    {
                                                        case 0x00:
                                                        {
                                                            SCIBusPCMSpeedString = "976.5 baud";

                                                            BeginInvoke((MethodInvoker)delegate
                                                            {
                                                                SCIBusSpeedComboBox.SelectedIndex = 1;
                                                            });
                                                            break;
                                                        }
                                                        case 0x01:
                                                        {
                                                            SCIBusPCMSpeedString = "7812.5 baud";

                                                            BeginInvoke((MethodInvoker)delegate
                                                            {
                                                                SCIBusSpeedComboBox.SelectedIndex = 2;
                                                            });
                                                            break;
                                                        }
                                                        case 0x02:
                                                        {
                                                            SCIBusPCMSpeedString = "62500 baud";

                                                            BeginInvoke((MethodInvoker)delegate
                                                            {
                                                                SCIBusSpeedComboBox.SelectedIndex = 3;
                                                            });
                                                            break;
                                                        }
                                                        case 0x03:
                                                        {
                                                            SCIBusPCMSpeedString = "125000 baud";

                                                            BeginInvoke((MethodInvoker)delegate
                                                            {
                                                                SCIBusSpeedComboBox.SelectedIndex = 4;
                                                            });
                                                            break;
                                                        }
                                                    }
                                                }

                                                if (SCIBusPCMStateString == "enabled")
                                                {
                                                    Util.UpdateTextBox(USBTextBox, "[INFO] PCM settings: " + Environment.NewLine +
                                                                                   "       - state: " + SCIBusPCMStateString + Environment.NewLine +
                                                                                   "       - logic: " + SCIBusPCMLogicString + " " + SCIBusPCMSBEC2ModeString + Environment.NewLine +
                                                                                   "       - ngc mode: " + SCIBusPCMNGCModeString + Environment.NewLine +
                                                                                   "       - obd config.: " + SCIBusPCMOBDConfigurationString + Environment.NewLine +
                                                                                   "       - speed: " + SCIBusPCMSpeedString + Environment.NewLine +
                                                                                   "       TCM settings: " + Environment.NewLine +
                                                                                   "       - state: disabled");
                                                }
                                                else
                                                {
                                                    Util.UpdateTextBox(USBTextBox, "[INFO] PCM settings: " + Environment.NewLine +
                                                                                    "       - state: " + SCIBusPCMStateString);
                                                }

                                                PCM.UpdateHeader(SCIBusPCMStateString, SCIBusPCMSpeedString, SCIBusPCMLogicString, SCIBusPCMOBDConfigurationString);
                                                TCM.UpdateHeader(SCIBusTCMStateString, SCIBusTCMSpeedString, SCIBusTCMLogicString, SCIBusTCMOBDConfigurationString);
                                            }

                                            //SCIBusModuleComboBox_SelectedIndexChanged(this, EventArgs.Empty);
                                        }
                                        else
                                        {
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Invalid settings packet:", PacketHelper.Serialize(packet));
                                        }
                                        break;
                                    }
                                    case (byte)PacketHelper.SettingsMode.SetRepeatBehavior:
                                    {
                                        if ((packet.Payload != null) && (packet.Payload.Length >= 3))
                                        {
                                            string bus = string.Empty;

                                            switch (packet.Payload[0])
                                            {
                                                case (byte)PacketHelper.Bus.CCD:
                                                {
                                                    bus = "CCD-bus";
                                                    break;
                                                }
                                                case (byte)PacketHelper.Bus.PCM:
                                                {
                                                    bus = "SCI-bus (PCM)";
                                                    break;
                                                }
                                                case (byte)PacketHelper.Bus.TCM:
                                                {
                                                    bus = "SCI-bus (TCM)";
                                                    break;
                                                }
                                                case (byte)PacketHelper.Bus.PCI:
                                                {
                                                    bus = "PCI-bus";
                                                    break;
                                                }
                                                default:
                                                {
                                                    bus = "Unknown";
                                                    break;
                                                }
                                            }

                                            string repeat_interval = ((packet.Payload[1] << 8) + packet.Payload[2]).ToString("0") + " ms";

                                            Util.UpdateTextBox(USBTextBox, "[RX->] Repeated message behavior changed:", PacketHelper.Serialize(packet));
                                            Util.UpdateTextBox(USBTextBox, "[INFO] Repeated message behavior settings:" + Environment.NewLine +
                                                                           "       Bus: " + bus + Environment.NewLine +
                                                                           "       Interval: " + repeat_interval);
                                        }
                                        else
                                        {
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Invalid settings packet:", PacketHelper.Serialize(packet));
                                        }
                                        break;
                                    }
                                    case (byte)PacketHelper.SettingsMode.SetLCD:
                                    {
                                        if ((packet.Payload != null) && (packet.Payload.Length >= 7))
                                        {
                                            string LCDState = string.Empty;

                                            switch (packet.Payload[0])
                                            {
                                                case 0x00:
                                                {
                                                    LCDState = "disabled";
                                                    break;
                                                }
                                                case 0x01:
                                                {
                                                    LCDState = "enabled";
                                                    break;
                                                }
                                                default:
                                                {
                                                    LCDState = "unknown";
                                                    break;
                                                }
                                            }

                                            string LCDI2CAddress = Util.ByteToHexString(packet.Payload, 1, 1) + " (hex)";
                                            string LCDSize = packet.Payload[2].ToString("0") + "x" + packet.Payload[3].ToString("0") + " characters";
                                            string LCDRefreshRate = packet.Payload[4].ToString("0") + " Hz";
                                            string LCDUnits = string.Empty;

                                            if (packet.Payload[5] == 0) LCDUnits = "imperial";
                                            else if (packet.Payload[5] == 1) LCDUnits = "metric";
                                            else LCDUnits = "imperial";

                                            string LCDDataSource = string.Empty;
                                            switch (packet.Payload[6])
                                            {
                                                case 0x01:
                                                {
                                                    LCDDataSource = "CCD-bus";
                                                    break;
                                                }
                                                case 0x02:
                                                {
                                                    LCDDataSource = "SCI-bus (PCM)";
                                                    break;
                                                }
                                                case 0x03:
                                                {
                                                    LCDDataSource = "SCI-bus (TCM)";
                                                    break;
                                                }
                                                case 0x04:
                                                {
                                                    LCDDataSource = "PCI-bus";
                                                    break;
                                                }
                                                default:
                                                {
                                                    LCDDataSource = "unknown";
                                                    break;
                                                }
                                            }

                                            Util.UpdateTextBox(USBTextBox, "[RX->] LCD settings changed:", PacketHelper.Serialize(packet));
                                            Util.UpdateTextBox(USBTextBox, "[INFO] LCD information:" + Environment.NewLine +
                                                                            "       State: " + LCDState + Environment.NewLine +
                                                                            "       I2C address: " + LCDI2CAddress + Environment.NewLine +
                                                                            "       Size: " + LCDSize + Environment.NewLine +
                                                                            "       Refresh rate: " + LCDRefreshRate + Environment.NewLine +
                                                                            "       Units: " + LCDUnits + Environment.NewLine +
                                                                            "       Data source: " + LCDDataSource);

                                            UpdateLCDPreviewTextBox();
                                        }
                                        else
                                        {
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Invalid settings packet:", PacketHelper.Serialize(packet));
                                        }
                                        break;
                                    }
                                    case (byte)PacketHelper.SettingsMode.SetPCIBus:
                                    {
                                        if ((packet.Payload != null) && (packet.Payload.Length > 0))
                                        {
                                            string PCIBusStateString = string.Empty;
                                            string PCIBusLogicLevelString = string.Empty;

                                            if (Util.IsBitClear(packet.Payload[0], 7))
                                            {
                                                PCIBusStateString = "disabled";
                                            }
                                            else
                                            {
                                                PCIBusStateString = "enabled";
                                            }

                                            if (Util.IsBitClear(packet.Payload[0], 6))
                                            {
                                                PCIBusLogicLevelString = "active-low";
                                            }
                                            else
                                            {
                                                PCIBusLogicLevelString = "active-high";
                                            }

                                            Util.UpdateTextBox(USBTextBox, "[RX->] PCI-bus settings changed:", PacketHelper.Serialize(packet));
                                            Util.UpdateTextBox(USBTextBox, "[INFO] PCI-bus settings: " + Environment.NewLine +
                                                                           "       - state: " + PCIBusStateString + Environment.NewLine +
                                                                           "       - logic: " + PCIBusLogicLevelString);

                                            PCI.UpdateHeader(PCIBusStateString, null, PCIBusLogicLevelString);
                                        }
                                        else
                                        {
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Invalid settings packet:", PacketHelper.Serialize(packet));
                                        }
                                        break;
                                    }
                                    case (byte)PacketHelper.SettingsMode.SetProgVolt:
                                    {
                                        if ((packet.Payload != null) && (packet.Payload.Length > 0))
                                        {
                                            Util.UpdateTextBox(USBTextBox, "[RX->] ProgVolt settings changed:", PacketHelper.Serialize(packet));

                                            if (Util.IsBitSet(packet.Payload[0], 7))
                                            {
                                                Util.UpdateTextBox(USBTextBox, "[INFO] VBB (12V) applied to SCI-RX pin.");
                                            }
                                            else if (Util.IsBitSet(packet.Payload[0], 6))
                                            {
                                                Util.UpdateTextBox(USBTextBox, "[INFO] VPP (20V) applied to SCI-RX pin.");
                                            }
                                            else if ((packet.Payload[0] & 0xC0) == 0)
                                            {
                                                Util.UpdateTextBox(USBTextBox, "[INFO] VBB/VPP removed from SCI-RX pin.");
                                            }
                                        }
                                        else
                                        {
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Invalid ProgVolt settings packet:", PacketHelper.Serialize(packet));
                                        }
                                        break;
                                    }
                                    case (byte)(PacketHelper.SettingsMode.SetUARTBaudrate):
                                    {
                                        if ((packet.Payload != null) && (packet.Payload.Length >= 4))
                                        {
                                            if (!SerialService.Connect(SelectedPort))
                                            {
                                                Util.UpdateTextBox(USBTextBox, "[INFO] Device not found on " + SelectedPort + ".");
                                                return;
                                            }

                                            uint baudrate = (uint)((packet.Payload[0] << 24) | (packet.Payload[1] << 16) | (packet.Payload[2] << 8) | packet.Payload[3]);

                                            Util.UpdateTextBox(USBTextBox, "[INFO] Scanner baudrate = " + baudrate);
                                        }
                                        else
                                        {
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Invalid UART baudrate packet:", PacketHelper.Serialize(packet));
                                        }
                                        break;
                                    }
                                    default:
                                    {
                                        Util.UpdateTextBox(USBTextBox, "[RX->] Packet received:", PacketHelper.Serialize(packet));
                                        break;
                                    }
                                }
                                break;
                            }
                            case (byte)PacketHelper.Command.Request:
                            {
                                break;
                            }
                            case (byte)PacketHelper.Command.Response:
                            {
                                switch (packet.Mode)
                                {
                                    case (byte)PacketHelper.ResponseMode.HardwareFirmwareInfo:
                                    {
                                        if (packet.Payload != null)
                                        {
                                            if ((packet.Payload[0] == 0) && (packet.Payload.Length >= 30)) // V1.5.0 and below
                                            {
                                                double HardwareVersion = ((packet.Payload[0] << 8) + packet.Payload[1]) / 100.00;
                                                string HardwareVersionString = "v" + (HardwareVersion).ToString("0.00").Replace(",", ".").Insert(3, ".");
                                                DateTime HardwareDate = Util.UnixTimeStampToDateTime((packet.Payload[6] << 24) + (packet.Payload[7] << 16) + (packet.Payload[8] << 8) + packet.Payload[9]);
                                                DateTime AssemblyDate = Util.UnixTimeStampToDateTime((packet.Payload[14] << 24) + (packet.Payload[15] << 16) + (packet.Payload[16] << 8) + packet.Payload[17]);
                                                DateTime FirmwareDate = Util.UnixTimeStampToDateTime((packet.Payload[22] << 24) + (packet.Payload[23] << 16) + (packet.Payload[24] << 8) + packet.Payload[25]);
                                                DeviceFirmwareTimestamp = (UInt64)((packet.Payload[22] << 24) + (packet.Payload[23] << 16) + (packet.Payload[24] << 8) + packet.Payload[25]);
                                                string HardwareDateString = HardwareDate.ToString("yyyy.MM.dd HH:mm:ss");
                                                string AssemblyDateString = AssemblyDate.ToString("yyyy.MM.dd HH:mm:ss");
                                                string FirmwareDateString = FirmwareDate.ToString("yyyy.MM.dd HH:mm:ss");
                                                string FirmwareVersionString = "v" + packet.Payload[26].ToString("0") + "." + packet.Payload[27].ToString("0") + "." + packet.Payload[28].ToString("0");
                                                HWVersion = HardwareVersionString;
                                                FWVersion = FirmwareVersionString;

                                                Util.UpdateTextBox(USBTextBox, "[RX->] Hardware/Firmware information response:", PacketHelper.Serialize(packet));
                                                Util.UpdateTextBox(USBTextBox, "[INFO] Hardware ver.: " + HardwareVersionString + Environment.NewLine +
                                                                               "       Firmware ver.: " + FirmwareVersionString + Environment.NewLine +
                                                                               "       Hardware date: " + HardwareDateString + Environment.NewLine +
                                                                               "       Assembly date: " + AssemblyDateString + Environment.NewLine +
                                                                               "       Firmware date: " + FirmwareDateString);
                                                BeginInvoke((MethodInvoker)delegate
                                                {
                                                    if (!Text.Contains("  |  FW v"))
                                                    {
                                                        Text += "  |  FW " + FirmwareVersionString + "  |  HW " + HardwareVersionString;
                                                    }
                                                    else
                                                    {
                                                        Text = Text.Remove(Text.Length - (HardwareVersionString.Length + FirmwareVersionString.Length + 8));
                                                        Text += FirmwareVersionString + "  |  HW " + HardwareVersionString;
                                                    }
                                                });

                                                if (Math.Round(HardwareVersion * 100) < 144) // below V1.44 the hardware doesn't support these features
                                                {
                                                    MeasureCCDBusVoltagesButton.Enabled = false;
                                                    CCDBusTerminationBiasOnOffCheckBox.Enabled = false;
                                                }
                                                else
                                                {
                                                    MeasureCCDBusVoltagesButton.Enabled = true;
                                                    CCDBusTerminationBiasOnOffCheckBox.Enabled = true;
                                                }
                                            }
                                            else if (packet.Payload.Length >= 22) // V2.0.0 and above
                                            {
                                                string HardwareVersionString = "v" + (packet.Payload[0]).ToString() + "." + (packet.Payload[1]).ToString() + "." + (packet.Payload[2]).ToString();
                                                string FirmwareVersionString = "v" + packet.Payload[4].ToString() + "." + packet.Payload[5].ToString() + "." + packet.Payload[6].ToString();
                                                HWVersion = HardwareVersionString;
                                                FWVersion = FirmwareVersionString;
                                                string ChipModelString;

                                                switch (packet.Payload[8])
                                                {
                                                    case 1:
                                                    {
                                                        ChipModelString = "ESP32";
                                                        break;
                                                    }
                                                    case 2:
                                                    {
                                                        ChipModelString = "ESP32-S2";
                                                        break;
                                                    }
                                                    case 5:
                                                    {
                                                        ChipModelString = "ESP32-C3";
                                                        break;
                                                    }
                                                    case 6:
                                                    {
                                                        ChipModelString = "ESP32-H2";
                                                        break;
                                                    }
                                                    case 9:
                                                    {
                                                        ChipModelString = "ESP32-S3";
                                                        break;
                                                    }
                                                    case 12:
                                                    {
                                                        ChipModelString = "ESP32-C2";
                                                        break;
                                                    }
                                                    default:
                                                    {
                                                        ChipModelString = "unknown";
                                                        break;
                                                    }
                                                }

                                                string ChipSizeString = "" + packet.Payload[15].ToString() + "MB";
                                                string ChipFeaturesString = string.Empty;

                                                List<string> features = new List<string>();
                                                features.Clear();

                                                if (Util.IsBitSet(packet.Payload[14], 0)) features.Add(ChipSizeString + " Flash");
                                                if (Util.IsBitSet(packet.Payload[14], 1)) features.Add("WiFi");
                                                if (Util.IsBitSet(packet.Payload[14], 5)) features.Add("BT");
                                                if (Util.IsBitSet(packet.Payload[14], 4)) features.Add("BLE");
                                                if (Util.IsBitSet(packet.Payload[14], 6)) features.Add("IEEE 802.15.4");
                                                if (Util.IsBitSet(packet.Payload[14], 7)) features.Add("Embedded PSRAM");

                                                if (features.Count > 0)
                                                {
                                                    foreach (string s in features)
                                                    {
                                                        ChipFeaturesString += s + "/";
                                                    }

                                                    if (ChipFeaturesString.Length > 2) ChipFeaturesString = ChipFeaturesString.Remove(ChipFeaturesString.Length - 1); // remove last "/" character
                                                }

                                                Util.UpdateTextBox(USBTextBox, "[RX->] Hardware/Firmware information response:", PacketHelper.Serialize(packet));
                                                Util.UpdateTextBox(USBTextBox, "[INFO] Hardware ver.: " + HardwareVersionString + Environment.NewLine +
                                                                               "       Firmware ver.: " + FirmwareVersionString + Environment.NewLine +
                                                                               "       CPU Model    : " + ChipModelString + Environment.NewLine +
                                                                               "         - Revision : " + packet.Payload[9].ToString() + Environment.NewLine +
                                                                               "         - Cores    : " + packet.Payload[10].ToString() + Environment.NewLine +
                                                                               "         - Features : " + ChipFeaturesString + Environment.NewLine +
                                                                               "         - MAC addr.: " + Util.ByteToHexString(packet.Payload, 16) + ":" + Util.ByteToHexString(packet.Payload, 17) + ":" + Util.ByteToHexString(packet.Payload, 18) + ":" + Util.ByteToHexString(packet.Payload, 19) + ":" + Util.ByteToHexString(packet.Payload, 20) + ":" + Util.ByteToHexString(packet.Payload, 21));

                                                BeginInvoke((MethodInvoker)delegate
                                                {
                                                    if (!Text.Contains("  |  FW v"))
                                                    {
                                                        Text += "  |  FW " + FirmwareVersionString + "  |  HW " + HardwareVersionString;
                                                    }
                                                    else
                                                    {
                                                        Text = Text.Remove(Text.Length - (HardwareVersionString.Length + FirmwareVersionString.Length + 8));
                                                        Text += FirmwareVersionString + "  |  HW " + HardwareVersionString;
                                                    }
                                                });
                                            }
                                        }
                                        else
                                        {
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Invalid HW/FW info packet:", PacketHelper.Serialize(packet));
                                        }
                                        break;
                                    }
                                    case (byte)PacketHelper.ResponseMode.Timestamp:
                                    {
                                        if ((packet.Payload != null) && (packet.Payload.Length > 3))
                                        {
                                            TimeSpan elapsedTime = TimeSpan.FromMilliseconds((packet.Payload[0] << 24) + (packet.Payload[1] << 16) + (packet.Payload[2] << 8) + packet.Payload[3]);
                                            DateTime timestamp = DateTime.Today.Add(elapsedTime);
                                            string timestampString = timestamp.ToString("HH:mm:ss.fff");

                                            Util.UpdateTextBox(USBTextBox, "[RX->] Timestamp response:", PacketHelper.Serialize(packet));
                                            Util.UpdateTextBox(USBTextBox, "[INFO] Device timestamp: " + timestampString);
                                        }
                                        else
                                        {
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Invalid timestamp packet:", PacketHelper.Serialize(packet));
                                        }
                                        break;
                                    }
                                    case (byte)PacketHelper.ResponseMode.BatteryVoltage:
                                    {
                                        if ((packet.Payload != null) && (packet.Payload.Length > 1))
                                        {
                                            string BatteryVoltageString = (((packet.Payload[0] << 8) + packet.Payload[1]) / 1000.00).ToString("0.000").Replace(",", ".") + " V";
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Battery voltage response:", PacketHelper.Serialize(packet));
                                            Util.UpdateTextBox(USBTextBox, "[INFO] Battery voltage: " + BatteryVoltageString);
                                        }
                                        else
                                        {
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Invalid battery voltage packet:", PacketHelper.Serialize(packet));
                                        }
                                        break;
                                    }
                                    case (byte)PacketHelper.ResponseMode.ExtEEPROMChecksum:
                                    {
                                        if ((packet.Payload != null) && (packet.Payload.Length > 2))
                                        {
                                            Util.UpdateTextBox(USBTextBox, "[RX->] External EEPROM checksum response:", PacketHelper.Serialize(packet));
                                            if (packet.Payload[0] == 0x01) // External EEPROM present
                                            {
                                                string ExternalEEPROMChecksumReading = Util.ByteToHexString(packet.Payload, 1, 1);
                                                string ExternalEEPROMChecksumCalculated = Util.ByteToHexString(packet.Payload, 2, 1);
                                                if (packet.Payload[1] == packet.Payload[2]) // if checksum reading and checksum calculation is the same
                                                {
                                                    Util.UpdateTextBox(USBTextBox, "[INFO] External EEPROM checksum: " + ExternalEEPROMChecksumReading + "=OK.");
                                                }
                                                else
                                                {
                                                    Util.UpdateTextBox(USBTextBox, "[INFO] External EEPROM checksum ERROR: " + Environment.NewLine +
                                                                                   "       - reads as: " + ExternalEEPROMChecksumReading + Environment.NewLine +
                                                                                   "       - calculated: " + ExternalEEPROMChecksumCalculated);
                                                }
                                            }
                                            else
                                            {
                                                Util.UpdateTextBox(USBTextBox, "[INFO] No external EEPROM found.");
                                            }
                                        }
                                        else
                                        {
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Invalid extEEPROM checksum packet:", PacketHelper.Serialize(packet));
                                        }
                                        break;
                                    }
                                    case (byte)PacketHelper.ResponseMode.CCDBusVoltages:
                                    {
                                        if ((packet.Payload != null) && (packet.Payload.Length > 3))
                                        {
                                            string CCDPositiveVoltage = (((packet.Payload[0] << 8) + packet.Payload[1]) / 1000.00).ToString("0.000").Replace(",", ".") + " V";
                                            string CCDNegativeVoltage = (((packet.Payload[2] << 8) + packet.Payload[3]) / 1000.00).ToString("0.000").Replace(",", ".") + " V";

                                            Util.UpdateTextBox(USBTextBox, "[RX->] CCD-bus voltage measurements response:", PacketHelper.Serialize(packet));
                                            Util.UpdateTextBox(USBTextBox, "[INFO] CCD-bus wire voltages:" + Environment.NewLine +
                                                                           "       CCD+: " + CCDPositiveVoltage + Environment.NewLine +
                                                                           "       CCD-: " + CCDNegativeVoltage);
                                        }
                                        else
                                        {
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Invalid CCD-bus voltage measurements packet:", PacketHelper.Serialize(packet));
                                        }
                                        break;
                                    }
                                    case (byte)PacketHelper.ResponseMode.VBBVolts:
                                    {
                                        if ((packet.Payload != null) && (packet.Payload.Length > 1))
                                        {
                                            string BootstrapVoltageString = (((packet.Payload[0] << 8) + packet.Payload[1]) / 1000.00).ToString("0.000").Replace(",", ".") + " V";
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Bootstrap voltage response:", PacketHelper.Serialize(packet));
                                            Util.UpdateTextBox(USBTextBox, "[INFO] Bootstrap voltage: " + BootstrapVoltageString);
                                        }
                                        else
                                        {
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Invalid bootstrap voltage packet:", PacketHelper.Serialize(packet));
                                        }
                                        break;
                                    }
                                    case (byte)PacketHelper.ResponseMode.VPPVolts:
                                    {
                                        if ((packet.Payload != null) && (packet.Payload.Length > 1))
                                        {
                                            string ProgrammingVoltageString = (((packet.Payload[0] << 8) + packet.Payload[1]) / 1000.00).ToString("0.000").Replace(",", ".") + " V";
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Programming voltage response:", PacketHelper.Serialize(packet));
                                            Util.UpdateTextBox(USBTextBox, "[INFO] Programming voltage: " + ProgrammingVoltageString);
                                        }
                                        else
                                        {
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Invalid programming voltage packet:", PacketHelper.Serialize(packet));
                                        }
                                        break;
                                    }
                                    case (byte)PacketHelper.ResponseMode.AllVolts:
                                    {
                                        if ((packet.Payload != null) && (packet.Payload.Length > 5))
                                        {
                                            string BatteryVoltageString = (((packet.Payload[0] << 8) + packet.Payload[1]) / 1000.00).ToString("0.000").Replace(",", ".") + " V";
                                            string BootstrapVoltageString = (((packet.Payload[2] << 8) + packet.Payload[3]) / 1000.00).ToString("0.000").Replace(",", ".") + " V";
                                            string ProgrammingVoltageString = (((packet.Payload[4] << 8) + packet.Payload[5]) / 1000.00).ToString("0.000").Replace(",", ".") + " V";
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Voltage measurements response:", PacketHelper.Serialize(packet));
                                            Util.UpdateTextBox(USBTextBox, "[INFO] Battery voltage: " + BatteryVoltageString + Environment.NewLine +
                                                                           "       Bootstrap voltage: " + BootstrapVoltageString + Environment.NewLine +
                                                                           "       Programming voltage: " + ProgrammingVoltageString);
                                        }
                                        else
                                        {
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Invalid voltage measurements packet:", PacketHelper.Serialize(packet));
                                        }
                                        break;
                                    }
                                    default:
                                    {
                                        Util.UpdateTextBox(USBTextBox, "[RX->] Packet received:", PacketHelper.Serialize(packet));
                                        break;
                                    }
                                }
                                break;
                            }
                            case (byte)PacketHelper.Command.MsgTx:
                            {
                                switch (packet.Mode)
                                {
                                    case (byte)PacketHelper.MsgTxMode.Stop:
                                    {
                                        if ((packet.Payload != null) && (packet.Payload.Length > 0))
                                        {
                                            switch (packet.Payload[0])
                                            {
                                                case (byte)PacketHelper.Bus.CCD:
                                                {
                                                    Util.UpdateTextBox(USBTextBox, "[RX->] CCD-bus repeated Tx stopped:", PacketHelper.Serialize(packet));
                                                    break;
                                                }
                                                case (byte)PacketHelper.Bus.PCM:
                                                {
                                                    Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (PCM) repeated Tx stopped:", PacketHelper.Serialize(packet));
                                                    break;
                                                }
                                                case (byte)PacketHelper.Bus.TCM:
                                                {
                                                    Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (TCM) repeated Tx stopped:", PacketHelper.Serialize(packet));
                                                    break;
                                                }
                                                case (byte)PacketHelper.Bus.PCI:
                                                {
                                                    Util.UpdateTextBox(USBTextBox, "[RX->] PCI-bus repeated Tx stopped:", PacketHelper.Serialize(packet));
                                                    break;
                                                }
                                                default:
                                                {
                                                    Util.UpdateTextBox(USBTextBox, "[RX->] Unknown communication bus action:", PacketHelper.Serialize(packet));
                                                    break;
                                                }
                                            }
                                        }
                                        else // error
                                        {
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Invalid communication bus action:", PacketHelper.Serialize(packet));
                                        }
                                        break;
                                    }
                                    case (byte)PacketHelper.MsgTxMode.Single:
                                    case (byte)PacketHelper.MsgTxMode.SingleVPP:
                                    {
                                        if ((packet.Payload != null) && (packet.Payload.Length > 0))
                                        {
                                            switch (packet.Payload[0])
                                            {
                                                case (byte)PacketHelper.Bus.CCD:
                                                {
                                                    Util.UpdateTextBox(USBTextBox, "[RX->] CCD-bus message prepared for Tx:", PacketHelper.Serialize(packet));
                                                    break;
                                                }
                                                case (byte)PacketHelper.Bus.PCM:
                                                {
                                                    Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (PCM) message prepared for Tx:", PacketHelper.Serialize(packet));
                                                    break;
                                                }
                                                case (byte)PacketHelper.Bus.TCM:
                                                {
                                                    Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (TCM) message prepared for Tx:", PacketHelper.Serialize(packet));
                                                    break;
                                                }
                                                case (byte)PacketHelper.Bus.PCI:
                                                {
                                                    Util.UpdateTextBox(USBTextBox, "[RX->] PCI-bus message prepared for Tx:", PacketHelper.Serialize(packet));
                                                    break;
                                                }
                                                default:
                                                {
                                                    Util.UpdateTextBox(USBTextBox, "[RX->] Unknown communication bus action:", PacketHelper.Serialize(packet));
                                                    break;
                                                }
                                            }
                                        }
                                        else // error
                                        {
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Invalid communication bus action:", PacketHelper.Serialize(packet));
                                        }
                                        break;
                                    }
                                    case (byte)PacketHelper.MsgTxMode.List:
                                    {
                                        if ((packet.Payload != null) && (packet.Payload.Length > 0))
                                        {
                                            switch (packet.Payload[0])
                                            {
                                                case (byte)PacketHelper.Bus.CCD:
                                                {
                                                    Util.UpdateTextBox(USBTextBox, "[RX->] CCD-bus message list prepared for Tx:", PacketHelper.Serialize(packet));
                                                    break;
                                                }
                                                case (byte)PacketHelper.Bus.PCM:
                                                {
                                                    Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (PCM) messages list prepared for Tx:", PacketHelper.Serialize(packet));
                                                    break;
                                                }
                                                case (byte)PacketHelper.Bus.TCM:
                                                {
                                                    Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (TCM) messages list prepared for Tx:", PacketHelper.Serialize(packet));
                                                    break;
                                                }
                                                case (byte)PacketHelper.Bus.PCI:
                                                {
                                                    Util.UpdateTextBox(USBTextBox, "[RX->] PCI-bus message list prepared for Tx:", PacketHelper.Serialize(packet));
                                                    break;
                                                }
                                                default:
                                                {
                                                    Util.UpdateTextBox(USBTextBox, "[RX->] Unknown communication bus action:", PacketHelper.Serialize(packet));
                                                    break;
                                                }
                                            }
                                        }
                                        else // error
                                        {
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Invalid communication bus action:", PacketHelper.Serialize(packet));
                                        }
                                        break;
                                    }
                                    case (byte)PacketHelper.MsgTxMode.RepeatedSingle:
                                    {
                                        if ((packet.Payload != null) && (packet.Payload.Length > 0))
                                        {
                                            switch (packet.Payload[0])
                                            {
                                                case (byte)PacketHelper.Bus.CCD:
                                                {
                                                    Util.UpdateTextBox(USBTextBox, "[RX->] CCD-bus repeated message Tx started:", PacketHelper.Serialize(packet));
                                                    break;
                                                }
                                                case (byte)PacketHelper.Bus.PCM:
                                                {
                                                    Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (PCM) repeated message Tx started:", PacketHelper.Serialize(packet));
                                                    break;
                                                }
                                                case (byte)PacketHelper.Bus.TCM:
                                                {
                                                    Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (TCM) repeated message Tx started:", PacketHelper.Serialize(packet));
                                                    break;
                                                }
                                                case (byte)PacketHelper.Bus.PCI:
                                                {
                                                    Util.UpdateTextBox(USBTextBox, "[RX->] PCI-bus repeated message Tx started:", PacketHelper.Serialize(packet));
                                                    break;
                                                }
                                                default:
                                                {
                                                    Util.UpdateTextBox(USBTextBox, "[RX->] Unknown communication bus action:", PacketHelper.Serialize(packet));
                                                    break;
                                                }
                                            }
                                        }
                                        else // error
                                        {
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Invalid communication bus action:", PacketHelper.Serialize(packet));
                                        }
                                        break;
                                    }
                                    case (byte)PacketHelper.MsgTxMode.RepeatedList:
                                    {
                                        if ((packet.Payload != null) && (packet.Payload.Length > 0))
                                        {
                                            switch (packet.Payload[0])
                                            {
                                                case (byte)PacketHelper.Bus.CCD:
                                                {
                                                    Util.UpdateTextBox(USBTextBox, "[RX->] CCD-bus repeated message list Tx started:", PacketHelper.Serialize(packet));
                                                    break;
                                                }
                                                case (byte)PacketHelper.Bus.PCM:
                                                {
                                                    Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (PCM) repeated message list Tx started:", PacketHelper.Serialize(packet));
                                                    break;
                                                }
                                                case (byte)PacketHelper.Bus.TCM:
                                                {
                                                    Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (TCM) repeated message list Tx started:", PacketHelper.Serialize(packet));
                                                    break;
                                                }
                                                case (byte)PacketHelper.Bus.PCI:
                                                {
                                                    Util.UpdateTextBox(USBTextBox, "[RX->] PCI-bus repeated message list Tx started:", PacketHelper.Serialize(packet));
                                                    break;
                                                }
                                                default:
                                                {
                                                    Util.UpdateTextBox(USBTextBox, "[RX->] Unknown communication bus action:", PacketHelper.Serialize(packet));
                                                    break;
                                                }
                                            }
                                        }
                                        else // error
                                        {
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Invalid communication bus action:", PacketHelper.Serialize(packet));
                                        }
                                        break;
                                    }
                                    default:
                                        Util.UpdateTextBox(USBTextBox, "[RX->] Packet received:", PacketHelper.Serialize(packet));
                                        break;
                                }
                                break;
                            }
                            case (byte)PacketHelper.Command.MsgRx:
                            {
                                switch (packet.Mode)
                                {
                                    case (byte)PacketHelper.MsgRxMode.Stop:
                                    {
                                        break;
                                    }
                                    case (byte)PacketHelper.MsgRxMode.Single:
                                    {
                                        break;
                                    }
                                    case (byte)PacketHelper.MsgRxMode.List:
                                    {
                                        break;
                                    }
                                    case (byte)PacketHelper.MsgRxMode.RepeatedSingle:
                                    {
                                        break;
                                    }
                                    case (byte)PacketHelper.MsgRxMode.RepeatedList:
                                    {
                                        break;
                                    }
                                    case (byte)PacketHelper.MsgTxMode.SingleVPP:
                                    {
                                        break;
                                    }
                                    default:
                                    {
                                        Util.UpdateTextBox(USBTextBox, "[RX->] Packet received:", PacketHelper.Serialize(packet));
                                        break;
                                    }
                                }
                                break;
                            }
                            case (byte)PacketHelper.Command.Debug:
                            {
                                switch (packet.Mode)
                                {
                                    case (byte)PacketHelper.DebugMode.RandomCCDBusMessages:
                                    {
                                        if ((packet.Payload != null) && (packet.Payload.Length > 0))
                                        {
                                            switch (packet.Payload[0])
                                            {
                                                case (byte)PacketHelper.OnOffMode.On:
                                                {
                                                    Util.UpdateTextBox(USBTextBox, "[RX->] Random CCD-bus messages started:", PacketHelper.Serialize(packet));
                                                    break;
                                                }
                                                case (byte)PacketHelper.OnOffMode.Off:
                                                {
                                                    Util.UpdateTextBox(USBTextBox, "[RX->] Random CCD-bus messages stopped:", PacketHelper.Serialize(packet));
                                                    break;
                                                }
                                                default:
                                                {
                                                    Util.UpdateTextBox(USBTextBox, "[RX->] Unknown debug packet:", PacketHelper.Serialize(packet));
                                                    break;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Invalid debug packet:", PacketHelper.Serialize(packet));
                                        }
                                        break;
                                    }
                                    case (byte)PacketHelper.DebugMode.ReadIntEEPROMbyte:
                                    {
                                        if ((packet.Payload != null) && (packet.Payload.Length > 3))
                                        {
                                            if (packet.Payload[0] == 0x00) // OK
                                            {
                                                string offset = Util.ByteToHexString(packet.Payload, 1, 2);
                                                string value = Util.ByteToHexString(packet.Payload, 3, 1);

                                                Util.UpdateTextBox(USBTextBox, "[RX->] Internal EEPROM byte read response:", PacketHelper.Serialize(packet));
                                                Util.UpdateTextBox(USBTextBox, "[INFO] Internal EEPROM byte information:" + Environment.NewLine +
                                                                               "       Offset: " + offset + " | Value: " + value);
                                            }
                                            else // error
                                            {
                                                Util.UpdateTextBox(USBTextBox, "[RX->] Internal EEPROM byte read error:", PacketHelper.Serialize(packet));
                                            }
                                        }
                                        else
                                        {
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Invalid debug packet:", PacketHelper.Serialize(packet));
                                        }
                                        break;
                                    }
                                    case (byte)PacketHelper.DebugMode.ReadIntEEPROMblock:
                                    {
                                        if ((packet.Payload != null) && (packet.Payload.Length > 4))
                                        {
                                            if (packet.Payload[0] == 0x00) // OK
                                            {
                                                int start = (packet.Payload[1] << 8) + packet.Payload[2];
                                                int length = packet.Payload.Length - 3;
                                                string offset = Util.ByteToHexString(packet.Payload, 1, 2);
                                                string values = Util.ByteToHexString(packet.Payload, 3, packet.Payload.Length - 3);
                                                string count = length.ToString();

                                                Util.UpdateTextBox(USBTextBox, "[RX->] Internal EEPROM block read response:", PacketHelper.Serialize(packet));
                                                Util.UpdateTextBox(USBTextBox, "[INFO] Internal EEPROM block information:" + Environment.NewLine +
                                                                               "       Offset: " + offset + " | Count: " + count + Environment.NewLine + values);

                                                if ((start == 0) && (length == 256))
                                                {
                                                    double HardwareVersion = ((packet.Payload[3] << 8) + packet.Payload[4]) / 100.00;
                                                    string HardwareVersionString = "v" + (HardwareVersion).ToString("0.00").Replace(",", ".").Insert(3, ".");
                                                    DateTime HardwareDate = Util.UnixTimeStampToDateTime((packet.Payload[9] << 24) + (packet.Payload[10] << 16) + (packet.Payload[11] << 8) + packet.Payload[12]);
                                                    DateTime AssemblyDate = Util.UnixTimeStampToDateTime((packet.Payload[17] << 24) + (packet.Payload[18] << 16) + (packet.Payload[19] << 8) + packet.Payload[20]);
                                                    string HardwareDateString = HardwareDate.ToString("yyyy.MM.dd HH:mm:ss");
                                                    string AssemblyDateString = AssemblyDate.ToString("yyyy.MM.dd HH:mm:ss");
                                                    double ADCVoltage = ((packet.Payload[21] << 8) + packet.Payload[22]) / 100.00;
                                                    string ADCVoltageString = ADCVoltage.ToString("0.00").Replace(",", ".") + " V";
                                                    double RDHighResistance = ((packet.Payload[23] << 8) + packet.Payload[24]) / 1000.0;
                                                    double RDLowResistance = ((packet.Payload[25] << 8) + packet.Payload[26]) / 1000.0;
                                                    string RDHighResistanceString = RDHighResistance.ToString("0.000").Replace(",", ".") + " kΩ";
                                                    string RDLowResistanceString = RDLowResistance.ToString("0.000").Replace(",", ".") + " kΩ";
                                                    string LCDStateString = string.Empty;

                                                    if (Util.IsBitSet(packet.Payload[27], 0)) LCDStateString = "enabled";
                                                    else LCDStateString = "disabled";

                                                    string LCDI2CAddressString = Util.ByteToHexString(packet.Payload, 28, 1) + " (hex)";
                                                    string LCDWidthString = packet.Payload[29].ToString("0") + " characters";
                                                    string LCDHeightString = packet.Payload[30].ToString("0") + " characters";
                                                    string LCDRefreshRateString = packet.Payload[31].ToString("0") + " Hz";
                                                    string LCDUnitsString = string.Empty;
                                                    string LCDDataSourceString = string.Empty;

                                                    if (packet.Payload[32] == 0x00) LCDUnitsString = "imperial";
                                                    else if (packet.Payload[32] == 0x01) LCDUnitsString = "metric";
                                                    else LCDUnitsString = "imperial";

                                                    switch (packet.Payload[33])
                                                    {
                                                        case 0x01:
                                                        {
                                                            LCDDataSourceString = "CCD-bus";
                                                            break;
                                                        }
                                                        case 0x02:
                                                        {
                                                            LCDDataSourceString = "SCI-bus (PCM)";
                                                            break;
                                                        }
                                                        case 0x03:
                                                        {
                                                            LCDDataSourceString = "SCI-bus (TCM)";
                                                            break;
                                                        }
                                                        default:
                                                        {
                                                            LCDDataSourceString = "CCD-bus";
                                                            break;
                                                        }
                                                    }

                                                    string LEDHeartbeatInterval = ((packet.Payload[34] << 8) + packet.Payload[35]).ToString() + " ms";
                                                    string LEDBlinkDuration = ((packet.Payload[36] << 8) + packet.Payload[37]).ToString() + " ms";

                                                    Util.UpdateTextBox(USBTextBox, "[INFO] External EEPROM settings:" + Environment.NewLine +
                                                                                   "       Hardware ver.: " + Util.ByteToHexString(packet.Payload, 3, 2) + " | " + HardwareVersionString + Environment.NewLine +
                                                                                   "       Hardware date: " + Util.ByteToHexString(packet.Payload, 5, 8) + " | " + Environment.NewLine +
                                                                                   "                      " + HardwareDateString + Environment.NewLine +
                                                                                   "       Assembly date: " + Util.ByteToHexString(packet.Payload, 13, 8) + " | " + Environment.NewLine +
                                                                                   "                      " + AssemblyDateString + Environment.NewLine +
                                                                                   "       ADC supply:    " + Util.ByteToHexString(packet.Payload, 21, 2) + " | " + ADCVoltageString + Environment.NewLine +
                                                                                   "       RDH resistor:  " + Util.ByteToHexString(packet.Payload, 23, 2) + " | " + RDHighResistanceString + Environment.NewLine +
                                                                                   "       RDL resistor:  " + Util.ByteToHexString(packet.Payload, 25, 2) + " | " + RDLowResistanceString + Environment.NewLine +
                                                                                   "       LCD state:        " + Util.ByteToHexString(packet.Payload, 27, 1) + " | " + LCDStateString + Environment.NewLine +
                                                                                   "       LCD I2C addr.:    " + Util.ByteToHexString(packet.Payload, 28, 1) + " | " + LCDI2CAddressString + Environment.NewLine +
                                                                                   "       LCD width:        " + Util.ByteToHexString(packet.Payload, 29, 1) + " | " + LCDWidthString + Environment.NewLine +
                                                                                   "       LCD height:       " + Util.ByteToHexString(packet.Payload, 30, 1) + " | " + LCDHeightString + Environment.NewLine +
                                                                                   "       LCD refresh:      " + Util.ByteToHexString(packet.Payload, 31, 1) + " | " + LCDRefreshRateString + Environment.NewLine +
                                                                                   "       LCD units:        " + Util.ByteToHexString(packet.Payload, 32, 1) + " | " + LCDUnitsString + Environment.NewLine +
                                                                                   "       LCD data src:     " + Util.ByteToHexString(packet.Payload, 33, 1) + " | " + LCDDataSourceString + Environment.NewLine +
                                                                                   "       LED heartbeat: " + Util.ByteToHexString(packet.Payload, 34, 2) + " | " + LEDHeartbeatInterval + Environment.NewLine +
                                                                                   "       LED blink:     " + Util.ByteToHexString(packet.Payload, 36, 2) + " | " + LEDBlinkDuration + Environment.NewLine +
                                                                                   "       CCD settings:     " + Util.ByteToHexString(packet.Payload, 38, 1) + Environment.NewLine +
                                                                                   "       PCM settings:     " + Util.ByteToHexString(packet.Payload, 39, 1) + Environment.NewLine +
                                                                                   "       TCM settings:     " + Util.ByteToHexString(packet.Payload, 40, 1));
                                                }
                                            }
                                            else // error
                                            {
                                                Util.UpdateTextBox(USBTextBox, "[RX->] Internal EEPROM block read error:", PacketHelper.Serialize(packet));
                                            }
                                        }
                                        else
                                        {
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Invalid debug packet:", PacketHelper.Serialize(packet));
                                        }
                                        break;
                                    }
                                    case (byte)PacketHelper.DebugMode.ReadExtEEPROMbyte:
                                    {
                                        if ((packet.Payload != null) && (packet.Payload.Length > 3))
                                        {
                                            if (packet.Payload[0] == 0x00) // OK
                                            {
                                                string offset = Util.ByteToHexString(packet.Payload, 1, 2);
                                                string value = Util.ByteToHexString(packet.Payload, 3, 1);

                                                Util.UpdateTextBox(USBTextBox, "[RX->] External EEPROM byte read response:", PacketHelper.Serialize(packet));
                                                Util.UpdateTextBox(USBTextBox, "[INFO] External EEPROM byte information:" + Environment.NewLine +
                                                                               "       Offset: " + offset + " | Value: " + value);
                                            }
                                            else // error
                                            {
                                                Util.UpdateTextBox(USBTextBox, "[RX->] External EEPROM byte read error:", PacketHelper.Serialize(packet));
                                            }
                                        }
                                        else
                                        {
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Invalid debug packet:", PacketHelper.Serialize(packet));
                                        }
                                        break;
                                    }
                                    case (byte)PacketHelper.DebugMode.ReadExtEEPROMblock:
                                    {
                                        if ((packet.Payload != null) && (packet.Payload.Length > 4))
                                        {
                                            if (packet.Payload[0] == 0x00) // OK
                                            {
                                                int start = (packet.Payload[1] << 8) + packet.Payload[2];
                                                int length = packet.Payload.Length - 3;
                                                string offset = Util.ByteToHexString(packet.Payload, 1, 2);
                                                string values = Util.ByteToHexString(packet.Payload, 3, packet.Payload.Length - 3);
                                                string count = length.ToString();

                                                Util.UpdateTextBox(USBTextBox, "[RX->] External EEPROM block read response:", PacketHelper.Serialize(packet));
                                                Util.UpdateTextBox(USBTextBox, "[INFO] External EEPROM block information:" + Environment.NewLine +
                                                                               "       Offset: " + offset + " | Count: " + count + Environment.NewLine + values);
                                            }
                                            else // error
                                            {
                                                Util.UpdateTextBox(USBTextBox, "[RX->] External EEPROM block read error:", PacketHelper.Serialize(packet));
                                            }
                                        }
                                        else
                                        {
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Invalid debug packet:", PacketHelper.Serialize(packet));
                                        }
                                        break;
                                    }
                                    case (byte)PacketHelper.DebugMode.WriteIntEEPROMbyte:
                                    {
                                        if ((packet.Payload != null) && (packet.Payload.Length > 3))
                                        {
                                            if (packet.Payload[0] == 0x00) // OK
                                            {
                                                string offset = Util.ByteToHexString(packet.Payload, 1, 2);
                                                string value = Util.ByteToHexString(packet.Payload, 3, 1);

                                                Util.UpdateTextBox(USBTextBox, "[RX->] Internal EEPROM byte write response:", PacketHelper.Serialize(packet));
                                                Util.UpdateTextBox(USBTextBox, "[INFO] Internal EEPROM byte information:" + Environment.NewLine +
                                                                               "       Offset: " + offset + " | Value: " + value);

                                                byte writtenByte = Util.HexStringToByte(EEPROMWriteValuesTextBox.Text)[0];
                                                byte readByte = packet.Payload[3];

                                                if (readByte == writtenByte)
                                                {
                                                    MessageBox.Show("Internal EEPROM byte write successful!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                                }
                                                else
                                                {
                                                    MessageBox.Show("Internal EEPROM byte write failed!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                                }
                                            }
                                            else // error
                                            {
                                                Util.UpdateTextBox(USBTextBox, "[RX->] Internal EEPROM byte write error:", PacketHelper.Serialize(packet));
                                            }
                                        }
                                        else
                                        {
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Invalid debug packet:", PacketHelper.Serialize(packet));
                                        }
                                        break;
                                    }
                                    case (byte)PacketHelper.DebugMode.WriteIntEEPROMblock:
                                    {
                                        if ((packet.Payload != null) && (packet.Payload.Length > 4))
                                        {
                                            if (packet.Payload[0] == 0x00) // OK
                                            {
                                                string offset = Util.ByteToHexString(packet.Payload, 1, 2);
                                                string values = Util.ByteToHexString(packet.Payload, 3, packet.Payload.Length - 3);
                                                string count = (packet.Payload.Length - 3).ToString();

                                                Util.UpdateTextBox(USBTextBox, "[RX->] Internal EEPROM block write response:", PacketHelper.Serialize(packet));
                                                Util.UpdateTextBox(USBTextBox, "[INFO] Internal EEPROM block information:" + Environment.NewLine +
                                                                               "       Offset: " + offset + " | Count: " + count + Environment.NewLine + values);

                                                byte[] writtenBytes = Util.HexStringToByte(EEPROMWriteValuesTextBox.Text);
                                                byte[] readBytes = new byte[writtenBytes.Length];
                                                Array.Copy(packet.Payload, 3, readBytes, 0, packet.Payload.Length - 3);

                                                if (Util.CompareArrays(writtenBytes, readBytes, 0, writtenBytes.Length))
                                                {
                                                    MessageBox.Show("Internal EEPROM block write successful!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                                }
                                                else
                                                {
                                                    MessageBox.Show("Internal EEPROM block write failed!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                                }
                                            }
                                            else // error
                                            {
                                                Util.UpdateTextBox(USBTextBox, "[RX->] Internal EEPROM block write error:", PacketHelper.Serialize(packet));
                                            }
                                        }
                                        else
                                        {
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Invalid debug packet:", PacketHelper.Serialize(packet));
                                        }
                                        break;
                                    }
                                    case (byte)PacketHelper.DebugMode.WriteExtEEPROMbyte:
                                    {
                                        if ((packet.Payload != null) && (packet.Payload.Length > 3))
                                        {
                                            if (packet.Payload[0] == 0x00) // OK
                                            {
                                                string offset = Util.ByteToHexString(packet.Payload, 1, 2);
                                                string value = Util.ByteToHexString(packet.Payload, 3, 1);

                                                Util.UpdateTextBox(USBTextBox, "[RX->] External EEPROM byte write response:", PacketHelper.Serialize(packet));
                                                Util.UpdateTextBox(USBTextBox, "[INFO] External EEPROM byte information:" + Environment.NewLine +
                                                                               "       Offset: " + offset + " | Value: " + value);

                                                byte writtenByte = Util.HexStringToByte(EEPROMWriteValuesTextBox.Text)[0];
                                                byte readByte = packet.Payload[3];

                                                if (readByte == writtenByte)
                                                {
                                                    MessageBox.Show("External EEPROM byte write successful!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                                }
                                                else
                                                {
                                                    MessageBox.Show("External EEPROM byte write failed!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                                }
                                            }
                                            else // error
                                            {
                                                Util.UpdateTextBox(USBTextBox, "[RX->] External EEPROM byte write error:", PacketHelper.Serialize(packet));
                                            }
                                        }
                                        else
                                        {
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Invalid debug packet:", PacketHelper.Serialize(packet));
                                        }
                                        break;
                                    }
                                    case (byte)PacketHelper.DebugMode.WriteExtEEPROMblock:
                                    {
                                        if ((packet.Payload != null) && (packet.Payload.Length > 4))
                                        {
                                            if (packet.Payload[0] == 0x00) // OK
                                            {
                                                string offset = Util.ByteToHexString(packet.Payload, 1, 2);
                                                string values = Util.ByteToHexString(packet.Payload, 3, packet.Payload.Length - 3);
                                                string count = (packet.Payload.Length - 3).ToString();

                                                Util.UpdateTextBox(USBTextBox, "[RX->] External EEPROM block write response:", PacketHelper.Serialize(packet));
                                                Util.UpdateTextBox(USBTextBox, "[INFO] External EEPROM block information:" + Environment.NewLine +
                                                                               "       Offset: " + offset + " | Count: " + count + Environment.NewLine + values);

                                                byte[] writtenBytes = Util.HexStringToByte(EEPROMWriteValuesTextBox.Text);
                                                byte[] readBytes = new byte[writtenBytes.Length];
                                                Array.Copy(packet.Payload, 3, readBytes, 0, packet.Payload.Length - 3);

                                                if (Util.CompareArrays(writtenBytes, readBytes, 0, writtenBytes.Length))
                                                {
                                                    MessageBox.Show("External EEPROM block write successful!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                                }
                                                else
                                                {
                                                    MessageBox.Show("External EEPROM block write failed!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                                }
                                            }
                                            else // error
                                            {
                                                Util.UpdateTextBox(USBTextBox, "[RX->] External EEPROM block write error:", PacketHelper.Serialize(packet));
                                            }
                                        }
                                        else
                                        {
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Invalid debug packet:", PacketHelper.Serialize(packet));
                                        }
                                        break;
                                    }
                                    case (byte)PacketHelper.DebugMode.SetArbitraryUARTSpeed:
                                    {
                                        if ((packet.Payload != null) && (packet.Payload.Length > 1))
                                        {
                                            switch (packet.Payload[0])
                                            {
                                                case (byte)PacketHelper.Bus.CCD:
                                                {
                                                    switch (packet.Payload[1])
                                                    {
                                                        case (byte)PacketHelper.BaudMode.ExtraLowBaud:
                                                        {
                                                            Util.UpdateTextBox(USBTextBox, "[RX->] CCD-bus speed changed:", PacketHelper.Serialize(packet));
                                                            Util.UpdateTextBox(USBTextBox, "[INFO] CCD-bus speed: 976.5 baud");
                                                            break;
                                                        }
                                                        case (byte)PacketHelper.BaudMode.LowBaud:
                                                        {
                                                            Util.UpdateTextBox(USBTextBox, "[RX->] CCD-bus speed changed:", PacketHelper.Serialize(packet));
                                                            Util.UpdateTextBox(USBTextBox, "[INFO] CCD-bus speed: 7812.5 baud");
                                                            break;
                                                        }
                                                        case (byte)PacketHelper.BaudMode.HighBaud:
                                                        {
                                                            Util.UpdateTextBox(USBTextBox, "[RX->] CCD-bus speed changed:", PacketHelper.Serialize(packet));
                                                            Util.UpdateTextBox(USBTextBox, "[INFO] CCD-bus speed: 62500 baud");
                                                            break;
                                                        }
                                                        case (byte)PacketHelper.BaudMode.ExtraHighBaud:
                                                        {
                                                            Util.UpdateTextBox(USBTextBox, "[RX->] CCD-bus speed changed:", PacketHelper.Serialize(packet));
                                                            Util.UpdateTextBox(USBTextBox, "[INFO] CCD-bus speed: 125000 baud");
                                                            break;
                                                        }
                                                        default:
                                                        {
                                                            Util.UpdateTextBox(USBTextBox, "[RX->] CCD-bus speed unchanged:", PacketHelper.Serialize(packet));
                                                            break;
                                                        }
                                                    }
                                                    break;
                                                }
                                                case (byte)PacketHelper.Bus.PCM:
                                                {
                                                    switch (packet.Payload[1])
                                                    {
                                                        case (byte)PacketHelper.BaudMode.ExtraLowBaud:
                                                        {
                                                            Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (PCM) speed changed:", PacketHelper.Serialize(packet));
                                                            Util.UpdateTextBox(USBTextBox, "[INFO] SCI-bus (PCM) speed: 976.5 baud");
                                                            break;
                                                        }
                                                        case (byte)PacketHelper.BaudMode.LowBaud:
                                                        {
                                                            Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (PCM) speed changed:", PacketHelper.Serialize(packet));
                                                            Util.UpdateTextBox(USBTextBox, "[INFO] SCI-bus (PCM) speed: 7812.5 baud");
                                                            break;
                                                        }
                                                        case (byte)PacketHelper.BaudMode.HighBaud:
                                                        {
                                                            Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (PCM) speed changed:", PacketHelper.Serialize(packet));
                                                            Util.UpdateTextBox(USBTextBox, "[INFO] SCI-bus (PCM) speed: 62500 baud");
                                                            break;
                                                        }
                                                        case (byte)PacketHelper.BaudMode.ExtraHighBaud:
                                                        {
                                                            Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (PCM) speed changed:", PacketHelper.Serialize(packet));
                                                            Util.UpdateTextBox(USBTextBox, "[INFO] SCI-bus (PCM) speed: 125000 baud");
                                                            break;
                                                        }
                                                        default:
                                                        {
                                                            Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (PCM) speed unchanged:", PacketHelper.Serialize(packet));
                                                            break;
                                                        }
                                                    }
                                                    break;
                                                }
                                                case (byte)PacketHelper.Bus.TCM:
                                                {
                                                    switch (packet.Payload[1])
                                                    {
                                                        case (byte)PacketHelper.BaudMode.ExtraLowBaud:
                                                        {
                                                            Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (TCM) speed changed:", PacketHelper.Serialize(packet));
                                                            Util.UpdateTextBox(USBTextBox, "[INFO] SCI-bus (TCM) speed: 976.5 baud");
                                                            break;
                                                        }
                                                        case (byte)PacketHelper.BaudMode.LowBaud:
                                                        {
                                                            Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (TCM) speed changed:", PacketHelper.Serialize(packet));
                                                            Util.UpdateTextBox(USBTextBox, "[INFO] SCI-bus (TCM) speed: 7812.5 baud");
                                                            break;
                                                        }
                                                        case (byte)PacketHelper.BaudMode.HighBaud:
                                                        {
                                                            Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (TCM) speed changed:", PacketHelper.Serialize(packet));
                                                            Util.UpdateTextBox(USBTextBox, "[INFO] SCI-bus (TCM) speed: 62500 baud");
                                                            break;
                                                        }
                                                        case (byte)PacketHelper.BaudMode.ExtraHighBaud:
                                                        {
                                                            Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (TCM) speed changed:", PacketHelper.Serialize(packet));
                                                            Util.UpdateTextBox(USBTextBox, "[INFO] SCI-bus (TCM) speed: 125000 baud");
                                                            break;
                                                        }
                                                        default:
                                                        {
                                                            Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (TCM) speed unchanged:", PacketHelper.Serialize(packet));
                                                            break;
                                                        }
                                                    }
                                                    break;
                                                }
                                                default:
                                                {
                                                    Util.UpdateTextBox(USBTextBox, "[RX->] Unknown baudrate change request:", PacketHelper.Serialize(packet));
                                                    break;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Invalid debug packet:", PacketHelper.Serialize(packet));
                                        }
                                        break;
                                    }
                                    case (byte)PacketHelper.DebugMode.InitBootstrapMode:
                                    {
                                        if ((packet.Payload != null) && (packet.Payload.Length > 0))
                                        {
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Init bootstrap mode result:", PacketHelper.Serialize(packet));

                                            switch (packet.Payload[0])
                                            {
                                                case 0:
                                                {
                                                    Util.UpdateTextBox(USBTextBox, "[INFO] Bootstrap init success.");
                                                    break;
                                                }
                                                case 1:
                                                {
                                                    Util.UpdateTextBox(USBTextBox, "[INFO] Error: no response to magic byte.");
                                                    break;
                                                }
                                                case 2:
                                                {
                                                    Util.UpdateTextBox(USBTextBox, "[INFO] Error: unexpected response to magic byte.");
                                                    break;
                                                }
                                                case 3:
                                                {
                                                    Util.UpdateTextBox(USBTextBox, "[INFO] Error: security seed response timeout.");
                                                    break;
                                                }
                                                case 4:
                                                {
                                                    Util.UpdateTextBox(USBTextBox, "[INFO] Error: security seed response checksum.");
                                                    break;
                                                }
                                                case 5:
                                                {
                                                    Util.UpdateTextBox(USBTextBox, "[INFO] Error: security key status timeout.");
                                                    break;
                                                }
                                                case 6:
                                                {
                                                    Util.UpdateTextBox(USBTextBox, "[INFO] Error: security key not accepted.");
                                                    break;
                                                }
                                                case 7:
                                                {
                                                    Util.UpdateTextBox(USBTextBox, "[INFO] Error: start bootloader timeout.");
                                                    break;
                                                }
                                                case 8:
                                                {
                                                    Util.UpdateTextBox(USBTextBox, "[INFO] Error: unexpected bootloader status byte.");
                                                    break;
                                                }
                                                default:
                                                {
                                                    Util.UpdateTextBox(USBTextBox, "[INFO] Error: unknown result.");
                                                    break;
                                                }
                                            }
                                        }
                                        break;
                                    }
                                    case (byte)PacketHelper.DebugMode.UploadWorkerFunction:
                                    {
                                        if ((packet.Payload != null) && (packet.Payload.Length > 0))
                                        {
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Upload worker function result:", PacketHelper.Serialize(packet));

                                            switch (packet.Payload[0])
                                            {
                                                case 0:
                                                {
                                                    Util.UpdateTextBox(USBTextBox, "[INFO] Worker function upload success.");
                                                    break;
                                                }
                                                case 1:
                                                {
                                                    Util.UpdateTextBox(USBTextBox, "[INFO] Error: no response to ping.");
                                                    break;
                                                }
                                                case 2:
                                                {
                                                    Util.UpdateTextBox(USBTextBox, "[INFO] Error: upload finished status byte not received.");
                                                    break;
                                                }
                                                case 3:
                                                {
                                                    Util.UpdateTextBox(USBTextBox, "[INFO] Error: unexpected upload finished status.");
                                                    break;
                                                }
                                                default:
                                                {
                                                    Util.UpdateTextBox(USBTextBox, "[INFO] Error: unknown result.");
                                                    break;
                                                }
                                            }
                                        }
                                        break;
                                    }
                                    case (byte)PacketHelper.DebugMode.StartWorkerFunction:
                                    case (byte)PacketHelper.DebugMode.DefaultSettings:
                                    case (byte)PacketHelper.DebugMode.GetRandomNumber:
                                    case (byte)PacketHelper.DebugMode.RestorePCMEEPROM:
                                    case (byte)PacketHelper.DebugMode.GetAW9523Data:
                                    default:
                                    {
                                        Util.UpdateTextBox(USBTextBox, "[RX->] Packet received:", PacketHelper.Serialize(packet));
                                        break;
                                    }
                                }
                                break;
                            }
                            case (byte)PacketHelper.Command.Error:
                            {
                                switch (packet.Mode)
                                {
                                    case (byte)PacketHelper.ErrorMode.Ok:
                                    {
                                        Util.UpdateTextBox(USBTextBox, "[RX->] OK:", PacketHelper.Serialize(packet));
                                        break;
                                    }
                                    case (byte)PacketHelper.ErrorMode.ErrorLengthInvalidValue:
                                    {
                                        Util.UpdateTextBox(USBTextBox, "[RX->] Error, invalid packet length:", PacketHelper.Serialize(packet));
                                        break;
                                    }
                                    case (byte)PacketHelper.ErrorMode.ErrorDatacodeInvalidCommand:
                                    {
                                        Util.UpdateTextBox(USBTextBox, "[RX->] Error, invalid dc command:", PacketHelper.Serialize(packet));
                                        break;
                                    }
                                    case (byte)PacketHelper.ErrorMode.ErrorSubDatacodeInvalidValue:
                                    {
                                        Util.UpdateTextBox(USBTextBox, "[RX->] Error, invalid sub-data code:", PacketHelper.Serialize(packet));
                                        break;
                                    }
                                    case (byte)PacketHelper.ErrorMode.ErrorPayloadInvalidValues:
                                    {
                                        Util.UpdateTextBox(USBTextBox, "[RX->] Error, invalid payload value(s):", PacketHelper.Serialize(packet));
                                        break;
                                    }
                                    case (byte)PacketHelper.ErrorMode.ErrorPacketChecksumInvalidValue:
                                    {
                                        Util.UpdateTextBox(USBTextBox, "[RX->] Error, invalid checksum:", PacketHelper.Serialize(packet));
                                        break;
                                    }
                                    case (byte)PacketHelper.ErrorMode.ErrorPacketTimeoutOccured:
                                    {
                                        Util.UpdateTextBox(USBTextBox, "[RX->] Error, packet timeout occured:", PacketHelper.Serialize(packet));
                                        break;
                                    }
                                    case (byte)PacketHelper.ErrorMode.ErrorBufferOverflow:
                                    {
                                        Util.UpdateTextBox(USBTextBox, "[RX->] Error, buffer overflow:", PacketHelper.Serialize(packet));
                                        break;
                                    }
                                    case (byte)PacketHelper.ErrorMode.ErrorInvalidBus:
                                    {
                                        Util.UpdateTextBox(USBTextBox, "[RX->] Error, invalid bus:", PacketHelper.Serialize(packet));
                                        break;
                                    }
                                    case (byte)PacketHelper.ErrorMode.ErrorSCILsNoResponse:
                                    {
                                        Util.UpdateTextBox(USBTextBox, "[RX->] Error, no response from SCI-bus:", PacketHelper.Serialize(packet));
                                        break;
                                    }
                                    case (byte)PacketHelper.ErrorMode.ErrorNotEnoughMCURAM:
                                    {
                                        Util.UpdateTextBox(USBTextBox, "[RX->] Error, not enough MCU RAM:", PacketHelper.Serialize(packet));
                                        break;
                                    }
                                    case (byte)PacketHelper.ErrorMode.ErrorSCIHsMemoryPtrNoResponse:
                                    {
                                        if ((packet.Payload != null) && (packet.Payload.Length > 0))
                                        {
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Error, SCI-bus RAM-table no response (" + Util.ByteToHexString(packet.Payload, 0, 1) + "):", PacketHelper.Serialize(packet));
                                        }
                                        else
                                        {
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Error, SCI-bus RAM-table no response:", PacketHelper.Serialize(packet));
                                        }
                                        break;
                                    }
                                    case (byte)PacketHelper.ErrorMode.ErrorSCIHsInvalidMemoryPtr:
                                    {
                                        if ((packet.Payload != null) && (packet.Payload.Length > 0))
                                        {
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Error, SCI-bus RAM-table invalid (" + Util.ByteToHexString(packet.Payload, 0, 1) + "):", PacketHelper.Serialize(packet));
                                        }
                                        else
                                        {
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Error, SCI-bus RAM-table invalid:", PacketHelper.Serialize(packet));
                                        }
                                        break;
                                    }
                                    case (byte)PacketHelper.ErrorMode.ErrorSCIHsNoResponse:
                                    {
                                        if ((packet.Payload != null) && (packet.Payload.Length > 1))
                                        {
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Error, no response from SCI-bus (" + Util.ByteToHexString(packet.Payload, 0, 2) + "):", PacketHelper.Serialize(packet));
                                        }
                                        else
                                        {
                                            Util.UpdateTextBox(USBTextBox, "[RX->] Error, no response from SCI-bus:", PacketHelper.Serialize(packet));
                                        }
                                        break;
                                    }
                                    case (byte)PacketHelper.ErrorMode.ErrorEEPNotFound:
                                    {
                                        Util.UpdateTextBox(USBTextBox, "[RX->] Error, external EEPROM not found:", PacketHelper.Serialize(packet));
                                        break;
                                    }
                                    case (byte)PacketHelper.ErrorMode.ErrorEEPRead:
                                    {
                                        Util.UpdateTextBox(USBTextBox, "[RX->] Error, external EEPROM read failure:", PacketHelper.Serialize(packet));
                                        break;
                                    }
                                    case (byte)PacketHelper.ErrorMode.ErrorEEPWrite:
                                    {
                                        Util.UpdateTextBox(USBTextBox, "[RX->] Error, external EEPROM write failure:", PacketHelper.Serialize(packet));
                                        break;
                                    }
                                    case (byte)PacketHelper.ErrorMode.ErrorInternal:
                                    {
                                        Util.UpdateTextBox(USBTextBox, "[RX->] Error, internal error:", PacketHelper.Serialize(packet));
                                        break;
                                    }
                                    case (byte)PacketHelper.ErrorMode.ErrorFatal:
                                    {
                                        Util.UpdateTextBox(USBTextBox, "[RX->] Error, fatal error:", PacketHelper.Serialize(packet));
                                        break;
                                    }
                                    default:
                                    {
                                        Util.UpdateTextBox(USBTextBox, "[RX->] Error packet received:", PacketHelper.Serialize(packet));
                                        break;
                                    }
                                }
                                break;
                            }
                            default:
                            {
                                Util.UpdateTextBox(USBTextBox, "[RX->] Packet received:", PacketHelper.Serialize(packet));
                                break;
                            }
                        }
                        break;
                    }
                    case (byte)PacketHelper.Bus.CCD:
                    {
                        if (CCDBusOnDemandToolStripMenuItem.Checked && (ScannerTabControl.SelectedTab.Name == "CCDBusControlTabPage") || !CCDBusOnDemandToolStripMenuItem.Checked)
                        {
                            if (Properties.Settings.Default.DisplayRawBusPackets)
                            {
                                Util.UpdateTextBox(USBTextBox, "[RX->] CCD-bus message:", PacketHelper.Serialize(packet));
                            }

                            CCD.AddMessage(packet.Payload.ToArray());
                        }
                        break;
                    }
                    case (byte)PacketHelper.Bus.PCI:
                    {
                        if (PCIBusOnDemandToolStripMenuItem.Checked && (ScannerTabControl.SelectedTab.Name == "PCIBusControlTabPage") || !PCIBusOnDemandToolStripMenuItem.Checked)
                        {
                            if (Properties.Settings.Default.DisplayRawBusPackets)
                            {
                                Util.UpdateTextBox(USBTextBox, "[RX->] PCI-bus message:", PacketHelper.Serialize(packet));
                            }

                            PCI.AddMessage(packet.Payload.ToArray());
                        }
                        break;
                    }
                    case (byte)PacketHelper.Bus.PCM:
                    {
                        switch (packet.Mode)
                        {
                            case (byte)PacketHelper.SCISpeedMode.LowSpeed:
                            {
                                if (packet.Payload.Length > 4)
                                {
                                    switch (packet.Payload[4]) // ID byte
                                    {
                                        case 0x10:
                                        case 0x32:
                                        {
                                            if (packet.Payload.Length < 7)
                                            {
                                                Util.UpdateTextBox(USBTextBox, "[RX->] Invalid SCI-bus (PCM) stored fault code list:", PacketHelper.Serialize(packet));
                                                break;
                                            }

                                            int ChecksumALocation = packet.Payload.Length - 1;

                                            if (packet.Payload[ChecksumALocation] != Util.ChecksumCalculator(packet.Payload, 4, ChecksumALocation))
                                            {
                                                Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (PCM) fault code checksum error:", PacketHelper.Serialize(packet));
                                                break;
                                            }

                                            Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (PCM) stored fault code list:", PacketHelper.Serialize(packet));

                                            List<byte> StoredFaultCodeList = new List<byte>();
                                            StoredFaultCodeList.AddRange(packet.Payload.Skip(5).Take(packet.Payload.Length - 6)); // skip first 5 bytes (timestamp and ID)
                                            StoredFaultCodeList.Remove(0xFD); // not fault code related
                                            StoredFaultCodeList.Remove(0xFE); // end of fault code list signifier

                                            if (StoredFaultCodeList.Count > 0)
                                            {
                                                StringBuilder sb = new StringBuilder();

                                                foreach (byte code in StoredFaultCodeList)
                                                {
                                                    int index = PCM.SBEC3EngineDTC.Rows.IndexOf(PCM.SBEC3EngineDTC.Rows.Find(code));

                                                    if (index > -1) // DTC description found
                                                    {
                                                        sb.Append(Util.ByteToHexStringSimple(new byte[1] { code }) + ": " + PCM.SBEC3EngineDTC.Rows[index]["description"] + Environment.NewLine);
                                                    }
                                                    else // no DTC description found
                                                    {
                                                        sb.Append(Util.ByteToHexStringSimple(new byte[1] { code }) + ": UNRECOGNIZED DTC" + Environment.NewLine);
                                                    }
                                                }

                                                sb.Remove(sb.Length - 2, 2); // remove last newline character

                                                Util.UpdateTextBox(USBTextBox, "[INFO] Stored PCM fault codes found:" + Environment.NewLine + sb.ToString());
                                            }
                                            else
                                            {
                                                Util.UpdateTextBox(USBTextBox, "[INFO] No stored PCM fault code found.");
                                            }
                                            break;
                                        }
                                        case 0x11: // pending fault code list request
                                        {
                                            if (packet.Payload.Length < 3)
                                            {
                                                Util.UpdateTextBox(USBTextBox, "[RX->] Invalid SCI-bus (PCM) pending fault code list:", PacketHelper.Serialize(packet));
                                                break;
                                            }

                                            Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (PCM) pending fault code list:", PacketHelper.Serialize(packet));

                                            List<byte> PendingFaultCodeList = new List<byte>();
                                            PendingFaultCodeList.AddRange(packet.Payload.Skip(5).Take(packet.Payload.Length - 2)); // skip first 5 bytes (timestamp and ID)

                                            if ((PendingFaultCodeList[0] == 0) && (PendingFaultCodeList[1] == 0))
                                            {
                                                Util.UpdateTextBox(USBTextBox, "[INFO] No pending PCM fault code found.");
                                            }
                                            else
                                            {
                                                StringBuilder sb = new StringBuilder();

                                                foreach (byte code in PendingFaultCodeList)
                                                {
                                                    if (code == 0) continue; // skip zero code, empty slot

                                                    int index = PCM.SBEC3EngineDTC.Rows.IndexOf(PCM.SBEC3EngineDTC.Rows.Find(code));

                                                    if (index > -1) // DTC description found
                                                    {
                                                        sb.Append(Util.ByteToHexStringSimple(new byte[1] { code }) + ": " + PCM.SBEC3EngineDTC.Rows[index]["description"] + Environment.NewLine);
                                                    }
                                                    else // no DTC description found
                                                    {
                                                        sb.Append(Util.ByteToHexStringSimple(new byte[1] { code }) + ": EMPTY DTC SLOT" + Environment.NewLine);
                                                    }
                                                }

                                                sb.Remove(sb.Length - 2, 2); // remove last newline character

                                                Util.UpdateTextBox(USBTextBox, "[INFO] Pending PCM fault codes found:" + Environment.NewLine + sb.ToString());
                                            }
                                            break;
                                        }
                                        case 0x2E:
                                        case 0x33:
                                        {
                                            if (packet.Payload.Length > 6)
                                            {
                                                int ChecksumLocation = packet.Payload.Length - 1;

                                                byte checksum = Util.ChecksumCalculator(packet.Payload, 4, ChecksumLocation);

                                                if ((packet.Payload[ChecksumLocation] == checksum) || (packet.Payload[ChecksumLocation] == (checksum - 0x1E)))
                                                {
                                                    Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (PCM) one-trip fault code list:", PacketHelper.Serialize(packet));

                                                    List<byte> FaultCode1TList = new List<byte>();
                                                    FaultCode1TList.AddRange(packet.Payload.Skip(5).Take(packet.Payload.Length - 6)); // skip first 5 bytes (timestamp and ID)
                                                    FaultCode1TList.Remove(0xFD); // not fault code related
                                                    FaultCode1TList.Remove(0xFE); // end of fault code list signifier

                                                    if (FaultCode1TList.Count > 0)
                                                    {
                                                        StringBuilder sb = new StringBuilder();

                                                        foreach (byte code in FaultCode1TList)
                                                        {
                                                            int index = PCM.SBEC3EngineDTC.Rows.IndexOf(PCM.SBEC3EngineDTC.Rows.Find(code));

                                                            if (index > -1) // DTC description found
                                                            {
                                                                sb.Append(Util.ByteToHexStringSimple(new byte[1] { code }) + ": " + PCM.SBEC3EngineDTC.Rows[index]["description"] + Environment.NewLine);
                                                            }
                                                            else // no DTC description found
                                                            {
                                                                sb.Append(Util.ByteToHexStringSimple(new byte[1] { code }) + ": UNRECOGNIZED DTC" + Environment.NewLine);
                                                            }
                                                        }

                                                        sb.Remove(sb.Length - 2, 2); // remove last newline character

                                                        Util.UpdateTextBox(USBTextBox, "[INFO] One-trip PCM fault codes found:" + Environment.NewLine + sb.ToString());
                                                    }
                                                    else
                                                    {
                                                        Util.UpdateTextBox(USBTextBox, "[INFO] No one-trip PCM fault code found.");
                                                    }
                                                }
                                                else
                                                {
                                                    Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (PCM) fault code checksum error:", PacketHelper.Serialize(packet));
                                                }
                                            }
                                            else // error
                                            {
                                                Util.UpdateTextBox(USBTextBox, "[RX->] Invalid SCI-bus (PCM) one-trip fault code list:", PacketHelper.Serialize(packet));
                                            }
                                            break;
                                        }
                                        case 0x17: // erase fault codes
                                        {
                                            if (packet.Payload.Length > 5)
                                            {
                                                Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (PCM) erase fault code list:", PacketHelper.Serialize(packet));

                                                if (packet.Payload[5] == 0xE0) Util.UpdateTextBox(USBTextBox, "[INFO] SCI-bus (PCM) fault code list erased.");
                                                else Util.UpdateTextBox(USBTextBox, "[INFO] SCI-bus (PCM) erase fault code list error.");
                                            }
                                            else // error
                                            {
                                                Util.UpdateTextBox(USBTextBox, "[RX->] Invalid SCI-bus (PCM) erase fault code list response:", PacketHelper.Serialize(packet));
                                            }
                                            break;
                                        }
                                        default:
                                        {
                                            if (Properties.Settings.Default.DisplayRawBusPackets)
                                            {
                                                Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (PCM) low-speed message:", PacketHelper.Serialize(packet));
                                            }
                                            break;
                                        }
                                    }
                                }
                                break;
                            }
                            case (byte)PacketHelper.SCISpeedMode.HighSpeed:
                            {
                                if (Properties.Settings.Default.DisplayRawBusPackets)
                                {
                                    Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (PCM) high-speed message:", PacketHelper.Serialize(packet));
                                }
                                break;
                            }
                            default:
                            {
                                if (Properties.Settings.Default.DisplayRawBusPackets)
                                {
                                    Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (PCM) message:", PacketHelper.Serialize(packet));
                                }
                                break;
                            }
                        }

                        PCM.AddMessage(packet.Payload.ToArray());
                        break;
                    }
                    case (byte)PacketHelper.Bus.TCM:
                    {
                        if (Properties.Settings.Default.DisplayRawBusPackets)
                        {
                            Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (TCM) message:", PacketHelper.Serialize(packet));
                        }

                        TCM.AddMessage(packet.Payload.ToArray());
                        break;
                    }
                    default:
                    {
                        Util.UpdateTextBox(USBTextBox, "[RX->] Packet received:", PacketHelper.Serialize(packet));
                        break;
                    }
                }
            }, null);
        }

        private void UpdateCCDTable(object sender, EventArgs e)
        {
            if (CCDBusDiagnosticsListBox.Items.Count == 0)
            {
                CCDBusDiagnosticsListBox.Items.AddRange(CCD.Diagnostics.Table.ToArray());
                return;
            }

            // Add current line to the buffer.
            CCDTableBuffer.Add(CCD.Diagnostics.Table[CCD.Diagnostics.LastUpdatedLine]);
            CCDTableBufferLocation.Add(CCD.Diagnostics.LastUpdatedLine);
            CCDTableRowCountHistory.Add(CCD.Diagnostics.Table.Count);

            //if (CCD.Diagnostics.lastUpdatedLine == CCD.Diagnostics.B2Row)
            //{
            //    CCDTableBuffer.Add(CCD.Diagnostics.Table[CCD.Diagnostics.F2Row]);
            //    CCDTableBufferLocation.Add(CCD.Diagnostics.F2Row);
            //    CCDTableRowCountHistory.Add(CCD.Diagnostics.Table.Count);
            //}
        }

        private void UpdatePCITable(object sender, EventArgs e)
        {
            if (PCIBusDiagnosticsListBox.Items.Count == 0)
            {
                PCIBusDiagnosticsListBox.Items.AddRange(PCI.Diagnostics.Table.ToArray());
                return;
            }

            // Add current line to the buffer.
            PCITableBuffer.Add(PCI.Diagnostics.Table[PCI.Diagnostics.LastUpdatedLine]);
            PCITableBufferLocation.Add(PCI.Diagnostics.LastUpdatedLine);
            PCITableRowCountHistory.Add(PCI.Diagnostics.Table.Count);
        }

        private void UpdateSCIPCMTable(object sender, EventArgs e)
        {
            if (SCIBusPCMDiagnosticsListBox.Items.Count == 0)
            {
                SCIBusPCMDiagnosticsListBox.Items.AddRange(PCM.Diagnostics.Table.ToArray());
                return;
            }

            // Add current line to the buffer.
            PCMTableBuffer.Add(PCM.Diagnostics.Table[PCM.Diagnostics.LastUpdatedLine]);
            PCMTableBufferLocation.Add(PCM.Diagnostics.LastUpdatedLine);
            PCMTableRowCountHistory.Add(PCM.Diagnostics.Table.Count);

            // Add visible RAM-table to the buffer.
            if ((PCM.speed == "62500 baud") && PCM.Diagnostics.RAMDumpTableVisible)
            {
                for (int i = 0; i < 23; i++) // RAM-table has 22 lines + 1 empty line below main table
                {
                    PCMTableBuffer.Add(PCM.Diagnostics.Table[PCM.Diagnostics.Table.Count - 23 + i]);
                    PCMTableBufferLocation.Add(PCM.Diagnostics.Table.Count - 23 + i);
                    PCMTableRowCountHistory.Add(PCM.Diagnostics.Table.Count);
                }
            }
        }

        private void UpdateSCITCMTable(object sender, EventArgs e)
        {
            if (SCIBusTCMDiagnosticsListBox.Items.Count == 0)
            {
                SCIBusTCMDiagnosticsListBox.Items.AddRange(TCM.Diagnostics.Table.ToArray());
                return;
            }

            // Add current line to the buffer.
            TCMTableBuffer.Add(TCM.Diagnostics.Table[TCM.Diagnostics.LastUpdatedLine]);
            TCMTableBufferLocation.Add(TCM.Diagnostics.LastUpdatedLine);
            TCMTableRowCountHistory.Add(TCM.Diagnostics.Table.Count);
        }

        public void TransmitUSBPacket(string description, Packet packet)
        {
            Invoke((MethodInvoker)delegate
            {
                if (!USBSendPacketComboBox.Items.Contains(USBSendPacketComboBox.Text)) // only add unique items (no repeat!)
                {
                    USBSendPacketComboBox.Items.Add(USBSendPacketComboBox.Text); // add command to the list so it can be selected later
                }
            });

            Util.UpdateTextBox(USBTextBox, description, PacketHelper.Serialize(packet));
        }

        public void UpdateUSBTextBox(string description)
        {
            Util.UpdateTextBox(USBTextBox, description);
        }

        public void SelectSCIBusHSMode()
        {
            SCIBusSpeedComboBox.SelectedIndex = 3; // 62500 baud
            SCIBusModuleConfigSpeedApplyButton_Click(this, EventArgs.Empty);
        }

        public void SelectSCIBusLSMode()
        {
            SCIBusSpeedComboBox.SelectedIndex = 2; // 7812.5 baud
            SCIBusModuleConfigSpeedApplyButton_Click(this, EventArgs.Empty);
        }

        #endregion

        #region USB communication

        private void USBSendPacketButton_Click(object sender, EventArgs e)
        {
            if (USBSendPacketComboBox.Text == string.Empty)
                return;

            byte[] bytes = Util.HexStringToByte(USBSendPacketComboBox.Text);

            if ((bytes == null) || (bytes.Length < 6))
            {
                MessageBox.Show("Minimum packet length = 6 bytes.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Packet packet = PacketHelper.Deserialize(bytes);

            if (!USBSendPacketComboBox.Items.Contains(USBSendPacketComboBox.Text)) // only add unique items (no repeat!)
            {
                USBSendPacketComboBox.Items.Add(USBSendPacketComboBox.Text); // add command to the list so it can be selected later
            }

            Util.UpdateTextBox(USBTextBox, "[<-TX] Data transmitted:", PacketHelper.Serialize(packet));

            SerialService.WritePacket(packet);
        }

        private void USBSendPacketComboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                e.Handled = true;
                USBSendPacketButton_Click(this, EventArgs.Empty);
            }
        }

        #endregion

        #region Control panel

        private void COMPortsRefreshButton_Click(object sender, EventArgs e)
        {
            UpdateCOMPortList();
        }

        private void DemoButton_Click(object sender, EventArgs e)
        {
            Util.UpdateTextBox(USBTextBox, "[INFO] GUI is now running in demo mode." + Environment.NewLine + "       Explore features without scanner.");
            USBCommunicationGroupBox.Enabled = true;
            ScannerTabControl.Enabled = true;
            DiagnosticsGroupBox.Enabled = true;
            ReadMemoryToolStripMenuItem.Enabled = true;
            ReadWriteMemoryToolStripMenuItem.Enabled = true;
            BootstrapToolsToolStripMenuItem.Enabled = true;
            EngineToolsToolStripMenuItem.Enabled = true;
            ABSToolsToolStripMenuItem.Enabled = true;
        }

        private void COMPortsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedPort = COMPortsComboBox.Text;
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            UpdateCOMPortList();

            if ((SelectedPort == "N/A") || (SelectedPort == string.Empty))
            {
                return;
            }

            if (ConnectButton.Text == Languages.strings.Connect)
            {
                Util.UpdateTextBox(USBTextBox, "[INFO] Connecting to " + SelectedPort + ".");

                if (!SerialService.Connect(SelectedPort))
                {
                    Util.UpdateTextBox(USBTextBox, "[INFO] Device not found on " + SelectedPort + ".");
                    return;
                }

                Util.UpdateTextBox(USBTextBox, "[INFO] Device connected to " + SelectedPort + ".");

                DeviceFound = true;
                ConnectButton.Text = Languages.strings.Disconnect;
                COMPortsComboBox.Enabled = false;
                COMPortsRefreshButton.Enabled = false;
                USBCommunicationGroupBox.Enabled = true;
                ScannerTabControl.Enabled = true;
                DiagnosticsGroupBox.Enabled = true;
                ReadMemoryToolStripMenuItem.Enabled = true;
                ReadWriteMemoryToolStripMenuItem.Enabled = true;
                BootstrapToolsToolStripMenuItem.Enabled = true;
                EngineToolsToolStripMenuItem.Enabled = true;
                ABSToolsToolStripMenuItem.Enabled = true;
                SerialService.PacketReceived += PacketReceivedHandler; // subscribe to the PacketReceived event
                CCD.Diagnostics.TableUpdated += UpdateCCDTable; // subscribe to the CCD-bus OnTableUpdated event
                PCI.Diagnostics.TableUpdated += UpdatePCITable; // subscribe to the PCI-bus OnTableUpdated event
                PCM.Diagnostics.TableUpdated += UpdateSCIPCMTable; // subscribe to the SCI-bus (PCM) OnTableUpdated event
                TCM.Diagnostics.TableUpdated += UpdateSCITCMTable; // subscribe to the SCI-bus (TCM) OnTableUpdated event
                ActiveControl = ExpandButton;
                VersionInfoButton_Click(this, EventArgs.Empty);
                StatusButton_Click(this, EventArgs.Empty);
            }
            else if (ConnectButton.Text == Languages.strings.Disconnect)
            {
                SerialService.Disconnect();
                SerialService.PacketReceived -= PacketReceivedHandler; // unsubscribe from the PacketReceived event
                CCD.Diagnostics.TableUpdated -= UpdateCCDTable; // unsubscribe from the CCD-bus OnTableUpdated event
                PCI.Diagnostics.TableUpdated -= UpdatePCITable; // unsubscribe from the CCD-bus OnTableUpdated event
                PCM.Diagnostics.TableUpdated -= UpdateSCIPCMTable; // unsubscribe from the SCI-bus (PCM) OnTableUpdated event
                TCM.Diagnostics.TableUpdated -= UpdateSCITCMTable; // unsubscribe from the SCI-bus (PCM) OnTableUpdated event
                ConnectButton.Text = Languages.strings.Connect;
                COMPortsComboBox.Enabled = true;
                COMPortsRefreshButton.Enabled = true;
                USBCommunicationGroupBox.Enabled = false;
                ScannerTabControl.Enabled = false;
                DiagnosticsGroupBox.Enabled = false;
                ReadMemoryToolStripMenuItem.Enabled = false;
                ReadWriteMemoryToolStripMenuItem.Enabled = false;
                BootstrapToolsToolStripMenuItem.Enabled = false;
                EngineToolsToolStripMenuItem.Enabled = false;
                ABSToolsToolStripMenuItem.Enabled = false;
                DeviceFound = false;
                timeout = false;
                Util.UpdateTextBox(USBTextBox, "[INFO] Device disconnected (" + SelectedPort + ").");
                Text = "Chrysler Scanner  |  GUI " + GUIVersion;
            }
        }

        private void ExpandButton_Click(object sender, EventArgs e)
        {
            if (ExpandButton.Text == Languages.strings.Expand)
            {
                DiagnosticsGroupBox.Visible = true;
                Size = new Size(1300, 650);
                CenterToScreen();
                ExpandButton.Text = Languages.strings.Collapse;
            }
            else if (ExpandButton.Text == Languages.strings.Collapse)
            {
                Size = new Size(405, 650);
                CenterToScreen();
                DiagnosticsGroupBox.Visible = false;
                ExpandButton.Text = Languages.strings.Expand;
            }
        }

        #endregion

        #region Device tab 

        private void ResetButton_Click(object sender, EventArgs e)
        {
            Packet packet = new Packet();

            packet.Bus = (byte)PacketHelper.Bus.USB;
            packet.Command = (byte)PacketHelper.Command.Reset;
            packet.Mode = (byte)PacketHelper.ResetMode.ResetInit;
            packet.Payload = null;

            Util.UpdateTextBox(USBTextBox, "[<-TX] Reset device:", PacketHelper.Serialize(packet));

            SerialService.WritePacket(packet);
        }

        private void HandshakeButton_Click(object sender, EventArgs e)
        {
            Packet packet = new Packet();

            packet.Bus = (byte)PacketHelper.Bus.USB;
            packet.Command = (byte)PacketHelper.Command.Handshake;
            packet.Mode = (byte)PacketHelper.HandshakeMode.HandshakeOnly;
            packet.Payload = null;

            Util.UpdateTextBox(USBTextBox, "[<-TX] Handshake request:", PacketHelper.Serialize(packet));

            SerialService.WritePacket(packet);
        }

        private void StatusButton_Click(object sender, EventArgs e)
        {
            Packet packet = new Packet();

            packet.Bus = (byte)PacketHelper.Bus.USB;
            packet.Command = (byte)PacketHelper.Command.Status;
            packet.Mode = (byte)PacketHelper.StatusMode.None;
            packet.Payload = null;

            Util.UpdateTextBox(USBTextBox, "[<-TX] Status request:", PacketHelper.Serialize(packet));

            SerialService.WritePacket(packet);
        }

        private void VersionInfoButton_Click(object sender, EventArgs e)
        {
            Packet packet = new Packet();

            packet.Bus = (byte)PacketHelper.Bus.USB;
            packet.Command = (byte)PacketHelper.Command.Request;
            packet.Mode = (byte)PacketHelper.RequestMode.HardwareFirmwareInfo;
            packet.Payload = null;

            Util.UpdateTextBox(USBTextBox, "[<-TX] Hardware/Firmware information request:", PacketHelper.Serialize(packet));

            SerialService.WritePacket(packet);
        }

        private void TimestampButton_Click(object sender, EventArgs e)
        {
            Packet packet = new Packet();

            packet.Bus = (byte)PacketHelper.Bus.USB;
            packet.Command = (byte)PacketHelper.Command.Request;
            packet.Mode = (byte)PacketHelper.RequestMode.Timestamp;
            packet.Payload = null;

            Util.UpdateTextBox(USBTextBox, "[<-TX] Timestamp request:", PacketHelper.Serialize(packet));

            SerialService.WritePacket(packet);
        }

        private void BatteryVoltageButton_Click(object sender, EventArgs e)
        {
            Packet packet = new Packet();

            packet.Bus = (byte)PacketHelper.Bus.USB;
            packet.Command = (byte)PacketHelper.Command.Request;
            packet.Mode = (byte)PacketHelper.RequestMode.BatteryVoltage;
            packet.Payload = null;

            Util.UpdateTextBox(USBTextBox, "[<-TX] Battery voltage request:", PacketHelper.Serialize(packet));

            SerialService.WritePacket(packet);
        }

        private void EEPROMChecksumButton_Click(object sender, EventArgs e)
        {
            if (InternalEEPROMRadioButton.Checked)
            {
                MessageBox.Show("The internal EEPROM has no assigned checksum!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Packet packet = new Packet();

            packet.Bus = (byte)PacketHelper.Bus.USB;
            packet.Command = (byte)PacketHelper.Command.Request;
            packet.Mode = (byte)PacketHelper.RequestMode.ExtEEPROMChecksum;
            packet.Payload = null;

            Util.UpdateTextBox(USBTextBox, "[<-TX] External EEPROM checksum request:", PacketHelper.Serialize(packet));

            SerialService.WritePacket(packet);
        }

        private void ReadEEPROMButton_Click(object sender, EventArgs e)
        {
            byte[] address = Util.HexStringToByte(EEPROMReadAddressTextBox.Text);

            if (address.Length != 2)
            {
                MessageBox.Show("Read address needs to be 2 bytes long!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (address.Length == 0) return;

            int offset = (address[0] << 8) + address[1];
            bool success = int.TryParse(EEPROMReadCountTextBox.Text, out int readCount);

            if (!success)
            {
                EEPROMReadCountTextBox.Text = "1";
                readCount = 1;
            }

            byte[] readCountBytes = new byte[] { (byte)(readCount >> 8), (byte)(readCount & 0xFF) };

            if (InternalEEPROMRadioButton.Checked)
            {
                if (readCount == 1)
                {
                    Packet packet = new Packet();

                    packet.Bus = (byte)PacketHelper.Bus.USB;
                    packet.Command = (byte)PacketHelper.Command.Debug;
                    packet.Mode = (byte)PacketHelper.DebugMode.ReadIntEEPROMbyte;
                    packet.Payload = address;

                    Util.UpdateTextBox(USBTextBox, "[<-TX] Internal EEPROM byte read request:", PacketHelper.Serialize(packet));

                    SerialService.WritePacket(packet);

                    if (offset > (IntEEPROMsize - 1))
                    {
                        MessageBox.Show("Internal EEPROM size exceeded (4096 bytes)!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else if (readCount > 1)
                {
                    List<byte> payloadList = new List<byte>();
                    payloadList.AddRange(address);
                    payloadList.AddRange(readCountBytes);

                    Packet packet = new Packet();

                    packet.Bus = (byte)PacketHelper.Bus.USB;
                    packet.Command = (byte)PacketHelper.Command.Debug;
                    packet.Mode = (byte)PacketHelper.DebugMode.ReadIntEEPROMblock;
                    packet.Payload = payloadList.ToArray();

                    Util.UpdateTextBox(USBTextBox, "[<-TX] Internal EEPROM block read request:", PacketHelper.Serialize(packet));

                    SerialService.WritePacket(packet);

                    if ((offset + (readCount - 1)) > (IntEEPROMsize - 1))
                    {
                        MessageBox.Show("Internal EEPROM size exceeded (4096 bytes)!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("Count value cannot be 0!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else if (ExternalEEPROMRadioButton.Checked)
            {
                if (readCount == 1)
                {
                    Packet packet = new Packet();

                    packet.Bus = (byte)PacketHelper.Bus.USB;
                    packet.Command = (byte)PacketHelper.Command.Debug;
                    packet.Mode = (byte)PacketHelper.DebugMode.ReadExtEEPROMbyte;
                    packet.Payload = address;

                    Util.UpdateTextBox(USBTextBox, "[<-TX] External EEPROM byte read request:", PacketHelper.Serialize(packet));

                    SerialService.WritePacket(packet);

                    if (offset > (ExtEEPROMsize - 1))
                    {
                        MessageBox.Show("External EEPROM size exceeded (4096 bytes)!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else if (readCount > 1)
                {
                    List<byte> payloadList = new List<byte>();
                    payloadList.AddRange(address);
                    payloadList.AddRange(readCountBytes);

                    Packet packet = new Packet();

                    packet.Bus = (byte)PacketHelper.Bus.USB;
                    packet.Command = (byte)PacketHelper.Command.Debug;
                    packet.Mode = (byte)PacketHelper.DebugMode.ReadExtEEPROMblock;
                    packet.Payload = payloadList.ToArray();

                    Util.UpdateTextBox(USBTextBox, "[<-TX] External EEPROM block read request:", PacketHelper.Serialize(packet));

                    SerialService.WritePacket(packet);

                    if ((offset + (readCount - 1)) > (ExtEEPROMsize - 1))
                    {
                        MessageBox.Show("External EEPROM size exceeded (4096 bytes)!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("Count value cannot be 0!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void WriteEEPROMButton_Click(object sender, EventArgs e)
        {
            byte[] address = Util.HexStringToByte(EEPROMWriteAddressTextBox.Text);

            if (address.Length != 2)
            {
                MessageBox.Show("Write address needs to be 2 bytes long!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (address.Length == 0) return;

            int offset = (address[0] << 8) + address[1];
            byte[] values = Util.HexStringToByte(EEPROMWriteValuesTextBox.Text);
            if (values.Length == 0) return;
            int writeCount = values.Length;

            if (InternalEEPROMRadioButton.Checked)
            {
                if (writeCount == 1)
                {
                    List<byte> payloadList = new List<byte>();
                    payloadList.AddRange(address);
                    payloadList.Add(values[0]);

                    Packet packet = new Packet();

                    packet.Bus = (byte)PacketHelper.Bus.USB;
                    packet.Command = (byte)PacketHelper.Command.Debug;
                    packet.Mode = (byte)PacketHelper.DebugMode.WriteIntEEPROMbyte;
                    packet.Payload = payloadList.ToArray();

                    Util.UpdateTextBox(USBTextBox, "[<-TX] Internal EEPROM byte write request:", PacketHelper.Serialize(packet));

                    SerialService.WritePacket(packet);

                    if (offset > (IntEEPROMsize - 1))
                    {
                        MessageBox.Show("Internal EEPROM size exceeded (4096 bytes)!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else if (writeCount > 1)
                {
                    List<byte> payloadList = new List<byte>();
                    payloadList.AddRange(address);
                    payloadList.AddRange(values);

                    Packet packet = new Packet();

                    packet.Bus = (byte)PacketHelper.Bus.USB;
                    packet.Command = (byte)PacketHelper.Command.Debug;
                    packet.Mode = (byte)PacketHelper.DebugMode.WriteIntEEPROMblock;
                    packet.Payload = payloadList.ToArray();

                    Util.UpdateTextBox(USBTextBox, "[<-TX] Internal EEPROM block write request:", PacketHelper.Serialize(packet));

                    SerialService.WritePacket(packet);

                    if ((offset + (writeCount - 1)) > (IntEEPROMsize - 1))
                    {
                        MessageBox.Show("Internal EEPROM size exceeded (4096 bytes)!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("Count value cannot be 0!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else if (ExternalEEPROMRadioButton.Checked)
            {
                if (writeCount == 1)
                {
                    List<byte> payloadList = new List<byte>();
                    payloadList.AddRange(address);
                    payloadList.Add(values[0]);

                    Packet packet = new Packet();

                    packet.Bus = (byte)PacketHelper.Bus.USB;
                    packet.Command = (byte)PacketHelper.Command.Debug;
                    packet.Mode = (byte)PacketHelper.DebugMode.WriteExtEEPROMbyte;
                    packet.Payload = payloadList.ToArray();

                    Util.UpdateTextBox(USBTextBox, "[<-TX] External EEPROM byte write request:", PacketHelper.Serialize(packet));

                    SerialService.WritePacket(packet);

                    if (offset > (ExtEEPROMsize - 1))
                    {
                        MessageBox.Show("External EEPROM size exceeded (4096 bytes)!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else if (writeCount > 1)
                {
                    List<byte> payloadList = new List<byte>();
                    payloadList.AddRange(address);
                    payloadList.AddRange(values);

                    Packet packet = new Packet();

                    packet.Bus = (byte)PacketHelper.Bus.USB;
                    packet.Command = (byte)PacketHelper.Command.Debug;
                    packet.Mode = (byte)PacketHelper.DebugMode.WriteExtEEPROMblock;
                    packet.Payload = payloadList.ToArray();

                    Util.UpdateTextBox(USBTextBox, "[<-TX] External EEPROM block write request:", PacketHelper.Serialize(packet));

                    SerialService.WritePacket(packet);

                    if ((offset + (writeCount - 1)) > (ExtEEPROMsize - 1))
                    {
                        MessageBox.Show("External EEPROM size exceeded (4096 bytes)!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("Count value cannot be 0!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void SetLEDsButton_Click(object sender, EventArgs e)
        {
            bool success;
            List<byte> payloadList = new List<byte>();
            byte heartbeatInterval_HB, heartbeatInterval_LB, LEDBlinkDuration_HB, LEDBlinkDuration_LB;

            success = int.TryParse(HeartbeatIntervalTextBox.Text, out int heartbeatInterval);
            if (!success)
            {
                HeartbeatIntervalTextBox.Text = "5000";
                heartbeatInterval = 5000;
            }

            success = int.TryParse(LEDBlinkDurationTextBox.Text, out int LEDBlinkDuration);
            if (!success)
            {
                LEDBlinkDurationTextBox.Text = "50";
                LEDBlinkDuration = 50;
            }

            heartbeatInterval_HB = (byte)((heartbeatInterval >> 8) & 0xFF);
            heartbeatInterval_LB = (byte)(heartbeatInterval & 0xFF);
            LEDBlinkDuration_HB = (byte)((LEDBlinkDuration >> 8) & 0xFF);
            LEDBlinkDuration_LB = (byte)(LEDBlinkDuration & 0xFF);

            payloadList.AddRange(new byte[] { heartbeatInterval_HB, heartbeatInterval_LB, LEDBlinkDuration_HB, LEDBlinkDuration_LB });

            Packet packet = new Packet();

            packet.Bus = (byte)PacketHelper.Bus.USB;
            packet.Command = (byte)PacketHelper.Command.Settings;
            packet.Mode = (byte)PacketHelper.SettingsMode.LEDs;
            packet.Payload = payloadList.ToArray();

            Util.UpdateTextBox(USBTextBox, "[<-TX] Change LED settings:", PacketHelper.Serialize(packet));

            SerialService.WritePacket(packet);
        }

        private void EEPROMWriteEnableCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (EEPROMWriteEnableCheckBox.Checked)
            {
                if (MessageBox.Show("Modifying EEPROM values can cause the device to behave unpredictably!" + Environment.NewLine + 
                                    "Are you sure you want to continue?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                {
                    WriteEEPROMButton.Enabled = true;
                    EEPROMWriteAddressLabel.Enabled = true;
                    EEPROMWriteAddressTextBox.Enabled = true;
                    EEPROMWriteValuesLabel.Enabled = true;
                    EEPROMWriteValuesTextBox.Enabled = true;
                }
                else
                {
                    EEPROMWriteEnableCheckBox.Checked = false;
                }
            }
            else
            {
                WriteEEPROMButton.Enabled = false;
                EEPROMWriteAddressLabel.Enabled = false;
                EEPROMWriteAddressTextBox.Enabled = false;
                EEPROMWriteValuesLabel.Enabled = false;
                EEPROMWriteValuesTextBox.Enabled = false;
            }
        }

        private void EEPROMReadAddressTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                e.Handled = true;
                ReadEEPROMButton_Click(this, EventArgs.Empty);
            }
        }

        private void EEPROMReadCountTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                e.Handled = true;
                ReadEEPROMButton_Click(this, EventArgs.Empty);
            }
        }

        private void EEPROMWriteAddressTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                e.Handled = true;
                WriteEEPROMButton_Click(this, EventArgs.Empty);
            }
        }

        private void EEPROMWriteValuesTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                e.Handled = true;
                WriteEEPROMButton_Click(this, EventArgs.Empty);
            }
        }

        private void HeartbeatIntervalTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                e.Handled = true;
                SetLEDsButton_Click(this, EventArgs.Empty);
            }
        }

        private void LEDBlinkDurationTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                e.Handled = true;
                SetLEDsButton_Click(this, EventArgs.Empty);
            }
        }

        #endregion

        #region CCD-bus tab 

        private void CCDBusTxMessageAddButton_Click(object sender, EventArgs e)
        {
            string[] messageStrings = CCDBusTxMessageComboBox.Text.Split(','); // multiple messages can be separated by commas

            foreach (string element in messageStrings)
            {
                byte[] message = Util.HexStringToByte(element);

                if (message.Length > 0)
                {
                    byte messageID = message[0];
                    bool match = false;
                    int rowNumber = 0;

                    if (CCDBusTxMessageChecksumCheckBox.Checked && (message.Length > 1))
                    {
                        message[message.Length - 1] = Util.ChecksumCalculator(message, 0, message.Length - 1);

                        CCDBusTxMessageComboBox.Text = Util.ByteToHexStringSimple(message);
                        CCDBusTxMessageComboBox.SelectionStart = CCDBusTxMessageComboBox.Text.Length;
                        CCDBusTxMessageComboBox.SelectionLength = 0;
                    }

                    for (int i = 0; i < CCDBusTxMessagesListBox.Items.Count; i++)
                    {
                        byte[] item = Util.HexStringToByte(CCDBusTxMessagesListBox.Items[i].ToString());
                        if (item[0] == messageID)
                        {
                            match = true;
                            rowNumber = i;
                            break;
                        }
                    }

                    string itemToInsert = Util.ByteToHexString(message, 0, message.Length);

                    if (!match)
                    {
                        CCDBusTxMessagesListBox.Items.Add(itemToInsert);
                    }
                    else
                    {
                        if (CCDBusOverwriteDuplicateIDCheckBox.Checked && !CCDBusTxMessageRepeatIntervalCheckBox.Checked)
                        {
                            CCDBusTxMessagesListBox.Items.RemoveAt(rowNumber);
                            CCDBusTxMessagesListBox.Items.Insert(rowNumber, itemToInsert);
                        }
                        else
                        {
                            CCDBusTxMessagesListBox.Items.Add(itemToInsert);
                        }
                    }

                    if (CCDBusTxMessagesListBox.Items.Count > 0)
                    {
                        CCDBusTxMessageRemoveItemButton.Enabled = true;
                        CCDBusTxMessageClearListButton.Enabled = true;
                        CCDBusSendMessagesButton.Enabled = true;
                        CCDBusStopRepeatedMessagesButton.Enabled = true;
                    }
                    else
                    {
                        CCDBusTxMessageRemoveItemButton.Enabled = false;
                        CCDBusTxMessageClearListButton.Enabled = false;
                        CCDBusSendMessagesButton.Enabled = false;
                        CCDBusStopRepeatedMessagesButton.Enabled = false;
                    }

                    if (!CCDBusTxMessageComboBox.Items.Contains(CCDBusTxMessageComboBox.Text)) // only add unique items (no repeat!)
                    {
                        CCDBusTxMessageComboBox.Items.Add(CCDBusTxMessageComboBox.Text); // add message to the list so it can be selected later
                    }

                    if (CCDBusTxMessageAddButton.Text == "Edit") CCDBusTxMessageAddButton.Text = "Add";
                }
            }

            CCDBusTxMessagesListBox.TopIndex = CCDBusTxMessagesListBox.Items.Count - 1; // scroll down
        }

        private void CCDBusTxMessageRemoveItemButton_Click(object sender, EventArgs e)
        {
            if (CCDBusTxMessagesListBox.SelectedIndex > -1)
            {
                for (int i = CCDBusTxMessagesListBox.SelectedIndices.Count - 1; i >= 0; i--)
                {
                    int index = CCDBusTxMessagesListBox.SelectedIndices[i];
                    CCDBusTxMessagesListBox.Items.RemoveAt(index);
                }

                if (CCDBusTxMessagesListBox.Items.Count > 0)
                {
                    CCDBusTxMessageRemoveItemButton.Enabled = true;
                    CCDBusTxMessageClearListButton.Enabled = true;
                    CCDBusSendMessagesButton.Enabled = true;
                    CCDBusStopRepeatedMessagesButton.Enabled = true;
                }
                else
                {
                    CCDBusTxMessageRemoveItemButton.Enabled = false;
                    CCDBusTxMessageClearListButton.Enabled = false;
                    CCDBusSendMessagesButton.Enabled = false;
                    CCDBusStopRepeatedMessagesButton.Enabled = false;
                }
            }
        }

        private void CCDBusTxMessageClearListButton_Click(object sender, EventArgs e)
        {
            CCDBusTxMessagesListBox.Items.Clear();
            CCDBusTxMessageRemoveItemButton.Enabled = false;
            CCDBusTxMessageClearListButton.Enabled = false;
            CCDBusSendMessagesButton.Enabled = false;
            CCDBusStopRepeatedMessagesButton.Enabled = false;
        }

        private void CCDBusSendMessagesButton_Click(object sender, EventArgs e)
        {
            if (DebugRandomCCDBusMessagesButton.Text == "Stop random messages")
            {
                DebugRandomCCDBusMessagesButton_Click(this, EventArgs.Empty);
            }

            if (!CCDBusTxMessageRepeatIntervalCheckBox.Checked) // no repeat
            {
                if (CCDBusTxMessagesListBox.Items.Count == 1) // single message once
                {
                    byte[] message = Util.HexStringToByte(CCDBusTxMessagesListBox.Items[0].ToString());

                    Packet packet = new Packet();

                    packet.Bus = (byte)PacketHelper.Bus.CCD;
                    packet.Command = (byte)PacketHelper.Command.MsgTx;
                    packet.Mode = (byte)PacketHelper.MsgTxMode.Single;
                    packet.Payload = message;

                    Util.UpdateTextBox(USBTextBox, "[<-TX] Send a CCD-bus message once:", PacketHelper.Serialize(packet));

                    if (message.Length > 0)
                    {
                        Util.UpdateTextBox(USBTextBox, "[INFO] CCD-bus message Tx list:" + Environment.NewLine +
                                                       "       " + Util.ByteToHexStringSimple(message));
                    }

                    SerialService.WritePacket(packet);
                }
                else // list of messages once
                {
                    List<byte[]> messages = new List<byte[]>();
                    List<byte> payloadList = new List<byte>();
                    byte messageCount = (byte)CCDBusTxMessagesListBox.Items.Count;

                    for (int i = 0; i < messageCount; i++)
                    {
                        messages.Add(Util.HexStringToByte(CCDBusTxMessagesListBox.Items[i].ToString()));
                    }

                    payloadList.Add(messageCount); // number of messages

                    for (int i = 0; i < messageCount; i++)
                    {
                        payloadList.Add((byte)messages[i].Length); // message length
                        payloadList.AddRange(messages[i]); // message
                    }


                    Packet packet = new Packet();

                    packet.Bus = (byte)PacketHelper.Bus.CCD;
                    packet.Command = (byte)PacketHelper.Command.MsgTx;
                    packet.Mode = (byte)PacketHelper.MsgTxMode.List;
                    packet.Payload = payloadList.ToArray();

                    Util.UpdateTextBox(USBTextBox, "[<-TX] Send a CCD-bus message list once:", PacketHelper.Serialize(packet));

                    StringBuilder messageList = new StringBuilder();

                    foreach (byte[] item in messages)
                    {
                        messageList.Append("       " + Util.ByteToHexStringSimple(item) + Environment.NewLine);
                    }

                    messageList.Replace(Environment.NewLine, string.Empty, messageList.Length - 2, 2); // remove last newline character

                    if (messages.Count > 0)
                    {
                        Util.UpdateTextBox(USBTextBox, "[INFO] CCD-bus message Tx list:" + Environment.NewLine + messageList.ToString());
                    }

                    SerialService.WritePacket(packet);
                }
            }
            else // repeat message(s) forever
            {
                if (CCDBusTxMessagesListBox.Items.Count == 1) // repeated single message
                {
                    // First send a settings packet to configure repeat behavior
                    SetRepeatedMessageBehavior(PacketHelper.Bus.CCD);

                    byte[] message = Util.HexStringToByte(CCDBusTxMessagesListBox.Items[0].ToString());
                    List<byte> payloadList = new List<byte>();

                    payloadList.AddRange(message);

                    Packet packet = new Packet();

                    packet.Bus = (byte)PacketHelper.Bus.CCD;
                    packet.Command = (byte)PacketHelper.Command.MsgTx;
                    packet.Mode = (byte)PacketHelper.MsgTxMode.RepeatedSingle;
                    packet.Payload = payloadList.ToArray();

                    Util.UpdateTextBox(USBTextBox, "[<-TX] Send a repeated CCD-bus message:", PacketHelper.Serialize(packet));

                    if (message.Length > 0)
                    {
                        Util.UpdateTextBox(USBTextBox, "[INFO] CCD-bus message Tx list:" + Environment.NewLine +
                                                       "       " + Util.ByteToHexStringSimple(message));
                    }

                    SerialService.WritePacket(packet);
                }
                else // repeated list of messages
                {
                    // First send a settings packet to configure repeat behavior
                    SetRepeatedMessageBehavior(PacketHelper.Bus.CCD);

                    List<byte[]> messages = new List<byte[]>();
                    List<byte> payloadList = new List<byte>();
                    byte messageCount = (byte)CCDBusTxMessagesListBox.Items.Count;

                    for (int i = 0; i < messageCount; i++)
                    {
                        messages.Add(Util.HexStringToByte(CCDBusTxMessagesListBox.Items[i].ToString()));
                    }

                    payloadList.Add(messageCount); // number of messages

                    for (int i = 0; i < messageCount; i++)
                    {
                        payloadList.Add((byte)messages[i].Length); // message length
                        payloadList.AddRange(messages[i]); // message
                    }

                    Packet packet = new Packet();

                    packet.Bus = (byte)PacketHelper.Bus.CCD;
                    packet.Command = (byte)PacketHelper.Command.MsgTx;
                    packet.Mode = (byte)PacketHelper.MsgTxMode.RepeatedList;
                    packet.Payload = payloadList.ToArray();

                    Util.UpdateTextBox(USBTextBox, "[<-TX] Send repeated CCD-bus message list:", PacketHelper.Serialize(packet));

                    StringBuilder messageList = new StringBuilder();

                    foreach (byte[] item in messages)
                    {
                        messageList.Append("       " + Util.ByteToHexStringSimple(item) + Environment.NewLine);
                    }
                    messageList.Replace(Environment.NewLine, string.Empty, messageList.Length - 2, 2); // remove last newline character

                    if (messages.Count > 0) Util.UpdateTextBox(USBTextBox, "[INFO] CCD-bus message Tx list:" + Environment.NewLine + messageList.ToString());

                    SerialService.WritePacket(packet);
                }
            }
        }

        private void CCDBusStopRepeatedMessagesButton_Click(object sender, EventArgs e)
        {
            Packet packet = new Packet();

            packet.Bus = (byte)PacketHelper.Bus.CCD;
            packet.Command = (byte)PacketHelper.Command.MsgTx;
            packet.Mode = (byte)PacketHelper.MsgTxMode.Stop;
            packet.Payload = null;

            Util.UpdateTextBox(USBTextBox, "[<-TX] Stop repeated Tx on CCD-bus:", PacketHelper.Serialize(packet));

            SerialService.WritePacket(packet);
        }

        private void DebugRandomCCDBusMessagesButton_Click(object sender, EventArgs e)
        {
            if (DebugRandomCCDBusMessagesButton.Text == "Send random messages")
            {
                if (MessageBox.Show("Use this debug mode with caution!" + Environment.NewLine + "The random generated messages may confuse modules on a live CCD-bus network.", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.OK)
                {
                    bool success;
                    byte minIntervalHB, minIntervalLB, maxIntervalHB, maxIntervalLB;

                    success = int.TryParse(CCDBusRandomMessageIntervalMinTextBox.Text, out int minInterval);
                    if (!success || (minInterval == 0))
                    {
                        minInterval = 200;
                        CCDBusRandomMessageIntervalMinTextBox.Text = "200";
                    }

                    success = int.TryParse(CCDBusRandomMessageIntervalMaxTextBox.Text, out int maxInterval);
                    if (!success || (maxInterval == 0))
                    {
                        maxInterval = 1000;
                        CCDBusRandomMessageIntervalMaxTextBox.Text = "1000";
                    }

                    minIntervalHB = (byte)((minInterval >> 8) & 0xFF);
                    minIntervalLB = (byte)(minInterval & 0xFF);
                    maxIntervalHB = (byte)((maxInterval >> 8) & 0xFF);
                    maxIntervalLB = (byte)(maxInterval & 0xFF);

                    Packet packet = new Packet();

                    packet.Bus = (byte)PacketHelper.Bus.USB;
                    packet.Command = (byte)PacketHelper.Command.Debug;
                    packet.Mode = (byte)PacketHelper.DebugMode.RandomCCDBusMessages;
                    packet.Payload = new byte[5] { 0x01, minIntervalHB, minIntervalLB, maxIntervalHB, maxIntervalLB };

                    Util.UpdateTextBox(USBTextBox, "[<-TX] Send random CCD-bus messages:", PacketHelper.Serialize(packet));

                    DebugRandomCCDBusMessagesButton.Text = "Stop random messages";

                    SerialService.WritePacket(packet);
                }
            }
            else if (DebugRandomCCDBusMessagesButton.Text == "Stop random messages")
            {
                Packet packet = new Packet();

                packet.Bus = (byte)PacketHelper.Bus.USB;
                packet.Command = (byte)PacketHelper.Command.Debug;
                packet.Mode = (byte)PacketHelper.DebugMode.RandomCCDBusMessages;
                packet.Payload = new byte[5] { 0x00, 0x00, 0x00, 0x00, 0x00 };

                Util.UpdateTextBox(USBTextBox, "[<-TX] Stop random CCD-bus messages:", PacketHelper.Serialize(packet));

                DebugRandomCCDBusMessagesButton.Text = "Send random messages";

                SerialService.WritePacket(packet);
            }
        }

        private void MeasureCCDBusVoltagesButton_Click(object sender, EventArgs e)
        {
            Packet packet = new Packet();

            packet.Bus = (byte)PacketHelper.Bus.USB;
            packet.Command = (byte)PacketHelper.Command.Request;
            packet.Mode = (byte)PacketHelper.RequestMode.CCDBusVoltages;
            packet.Payload = null;

            Util.UpdateTextBox(USBTextBox, "[<-TX] Measure CCD-bus voltages request:", PacketHelper.Serialize(packet));

            SerialService.WritePacket(packet);
        }

        private void CCDBusTxMessagesListBox_DoubleClick(object sender, EventArgs e)
        {
            if ((CCDBusTxMessagesListBox.Items.Count > 0) && (CCDBusTxMessagesListBox.SelectedIndex > -1))
            {
                string selectedItem = CCDBusTxMessagesListBox.SelectedItem.ToString();
                CCDBusTxMessageComboBox.Text = selectedItem;
                CCDBusTxMessageComboBox.Focus();
                CCDBusTxMessageComboBox.SelectionStart = CCDBusTxMessageComboBox.Text.Length;
                CCDBusTxMessageAddButton.Text = "Edit";
            }
        }

        private void CCDBusTxMessageRepeatIntervalCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (CCDBusTxMessageRepeatIntervalCheckBox.Checked)
            {
                CCDBusTxMessageRepeatIntervalTextBox.Enabled = true;
                MillisecondsLabel03.Enabled = true;
            }
            else
            {
                CCDBusTxMessageRepeatIntervalTextBox.Enabled = false;
                MillisecondsLabel03.Enabled = false;
            }
        }

        private void CCDBusSettingsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            byte config = 0x01; // set lowest bit to indicate 7812.5 baud speed

            if (CCDBusTransceiverOnOffCheckBox.Checked)
            {
                config = Util.SetBit(config, 7);
                CCDBusTransceiverOnOffCheckBox.Text = "CCD-bus transceiver ON";
                CCD.UpdateHeader("enabled", null, null);
            }
            else
            {
                config = Util.ClearBit(config, 7);
                CCDBusTransceiverOnOffCheckBox.Text = "CCD-bus transceiver OFF";
                CCD.UpdateHeader("disabled", null, null);
            }

            if (CCDBusTerminationBiasOnOffCheckBox.Checked)
            {
                config = Util.SetBit(config, 6);
                CCDBusTerminationBiasOnOffCheckBox.Text = "CCD-bus termination / bias ON";
            }
            else
            {
                config = Util.ClearBit(config, 6);
                CCDBusTerminationBiasOnOffCheckBox.Text = "CCD-bus termination / bias OFF";
            }

            Packet packet = new Packet();

            packet.Bus = (byte)PacketHelper.Bus.USB;
            packet.Command = (byte)PacketHelper.Command.Settings;
            packet.Mode = (byte)PacketHelper.SettingsMode.SetCCDBus;
            packet.Payload = new byte[1] { config };

            Util.UpdateTextBox(USBTextBox, "[<-TX] Change CCD-bus settings:", PacketHelper.Serialize(packet));

            SerialService.WritePacket(packet);
        }

        private void CCDBusTxMessageRepeatIntervalTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                e.Handled = true;
                SetRepeatedMessageBehavior(PacketHelper.Bus.CCD);
            }
        }

        private void CCDBusRandomMessageIntervalMinTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                e.Handled = true;
                DebugRandomCCDBusMessagesButton_Click(this, EventArgs.Empty);
            }
        }

        private void CCDBusRandomMessageIntervalMaxTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                e.Handled = true;
                DebugRandomCCDBusMessagesButton_Click(this, EventArgs.Empty);
            }
        }

        private void CCDBusTxMessageComboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                e.Handled = true;

                if (CCDBusTxMessageComboBox.Text.Length < 2) return;

                if (CCDBusTxMessageComboBox.Text.Contains(","))
                {
                    CCDBusTxMessageAddButton_Click(this, EventArgs.Empty);

                    if (!CCDBusTxMessageComboBox.Items.Contains(CCDBusTxMessageComboBox.Text)) // only add unique items (no repeat!)
                    {
                        CCDBusTxMessageComboBox.Items.Add(CCDBusTxMessageComboBox.Text); // add message to the list so it can be selected later
                    }

                    CCDBusTxMessageComboBox.Text = string.Empty;
                    return;
                }

                byte[] message = Util.HexStringToByte(CCDBusTxMessageComboBox.Text);

                if (CCDBusTxMessageAddButton.Text != "Edit")
                {
                    if (CCDBusTxMessageChecksumCheckBox.Checked && (message.Length > 1))
                    {
                        message[message.Length - 1] = Util.ChecksumCalculator(message, 0, message.Length - 1);

                        CCDBusTxMessageComboBox.Text = Util.ByteToHexStringSimple(message);
                        CCDBusTxMessageComboBox.SelectionStart = CCDBusTxMessageComboBox.Text.Length;
                        CCDBusTxMessageComboBox.SelectionLength = 0;
                    }

                    Packet packet = new Packet();

                    packet.Bus = (byte)PacketHelper.Bus.CCD;
                    packet.Command = (byte)PacketHelper.Command.MsgTx;
                    packet.Mode = (byte)PacketHelper.MsgTxMode.Single;
                    packet.Payload = message;

                    Util.UpdateTextBox(USBTextBox, "[<-TX] Send a CCD-bus message once:", PacketHelper.Serialize(packet));

                    if (message.Length > 0)
                    {
                        Util.UpdateTextBox(USBTextBox, "[INFO] CCD-bus message Tx list:" + Environment.NewLine +
                                                       "       " + Util.ByteToHexStringSimple(message));
                    }

                    SerialService.WritePacket(packet);
                }
                else
                {
                    if (CCDBusTxMessageChecksumCheckBox.Checked && (message.Length > 1))
                    {
                        message[message.Length - 1] = Util.ChecksumCalculator(message, 0, message.Length - 1);

                        CCDBusTxMessageComboBox.Text = Util.ByteToHexStringSimple(message);
                        CCDBusTxMessageComboBox.SelectionStart = CCDBusTxMessageComboBox.Text.Length;
                        CCDBusTxMessageComboBox.SelectionLength = 0;
                    }

                    CCDBusTxMessageAddButton_Click(this, EventArgs.Empty);
                }

                if (!CCDBusTxMessageComboBox.Items.Contains(CCDBusTxMessageComboBox.Text)) // only add unique items (no repeat!)
                {
                    CCDBusTxMessageComboBox.Items.Add(CCDBusTxMessageComboBox.Text); // add message to the list so it can be selected later
                }
            }
        }

        #endregion

        #region SCI-bus tab 

        private void SCIBusTxMessageAddButton_Click(object sender, EventArgs e)
        {
            string[] messageStrings = SCIBusTxMessageComboBox.Text.Split(','); // multiple messages can be separated by commas

            foreach (string element in messageStrings)
            {
                byte[] message = Util.HexStringToByte(element);

                if (message.Length > 0)
                {
                    byte messageID = message[0];
                    bool match = false;
                    int rowNumber = 0;

                    if (SCIBusTxMessageChecksumCheckBox.Checked && (message.Length > 1))
                    {
                        message[message.Length - 1] = Util.ChecksumCalculator(message, 0, message.Length - 1);

                        SCIBusTxMessageComboBox.Text = Util.ByteToHexStringSimple(message);
                        SCIBusTxMessageComboBox.SelectionStart = CCDBusTxMessageComboBox.Text.Length;
                        SCIBusTxMessageComboBox.SelectionLength = 0;
                    }

                    for (int i = 0; i < SCIBusTxMessagesListBox.Items.Count; i++)
                    {
                        byte[] item = Util.HexStringToByte(SCIBusTxMessagesListBox.Items[i].ToString());

                        if (item[0] == messageID)
                        {
                            match = true;
                            rowNumber = i;
                            break;
                        }
                    }

                    string itemToInsert = Util.ByteToHexString(message, 0, message.Length);

                    if (!match)
                    {
                        SCIBusTxMessagesListBox.Items.Add(itemToInsert);
                    }
                    else
                    {
                        if (SCIBusOverwriteDuplicateIDCheckBox.Checked && !SCIBusTxMessageRepeatIntervalCheckBox.Checked)
                        {
                            SCIBusTxMessagesListBox.Items.RemoveAt(rowNumber);
                            SCIBusTxMessagesListBox.Items.Insert(rowNumber, itemToInsert);
                        }
                        else
                        {
                            SCIBusTxMessagesListBox.Items.Add(itemToInsert);
                        }
                    }

                    if (SCIBusTxMessagesListBox.Items.Count > 0)
                    {
                        SCIBusTxMessageRemoveItemButton.Enabled = true;
                        SCIBusTxMessageClearListButton.Enabled = true;
                        SCIBusSendMessagesButton.Enabled = true;
                        SCIBusStopRepeatedMessagesButton.Enabled = true;
                    }
                    else
                    {
                        SCIBusTxMessageRemoveItemButton.Enabled = false;
                        SCIBusTxMessageClearListButton.Enabled = false;
                        SCIBusSendMessagesButton.Enabled = false;
                        SCIBusStopRepeatedMessagesButton.Enabled = false;
                    }

                    if (!SCIBusTxMessageComboBox.Items.Contains(SCIBusTxMessageComboBox.Text)) // only add unique items (no repeat!)
                    {
                        SCIBusTxMessageComboBox.Items.Add(SCIBusTxMessageComboBox.Text); // add message to the list so it can be selected later
                    }

                    if (SCIBusTxMessageAddButton.Text == "Edit") SCIBusTxMessageAddButton.Text = "Add";
                }
            }

            SCIBusTxMessagesListBox.TopIndex = SCIBusTxMessagesListBox.Items.Count - 1; // scroll down
        }

        private void SCIBusTxMessageRemoveItemButton_Click(object sender, EventArgs e)
        {
            if (SCIBusTxMessagesListBox.SelectedIndex > -1)
            {
                for (int i = SCIBusTxMessagesListBox.SelectedIndices.Count - 1; i >= 0; i--)
                {
                    int index = SCIBusTxMessagesListBox.SelectedIndices[i];
                    SCIBusTxMessagesListBox.Items.RemoveAt(index);
                }

                if (SCIBusTxMessagesListBox.Items.Count > 0)
                {
                    SCIBusTxMessageRemoveItemButton.Enabled = true;
                    SCIBusTxMessageClearListButton.Enabled = true;
                    SCIBusSendMessagesButton.Enabled = true;
                    SCIBusStopRepeatedMessagesButton.Enabled = true;
                }
                else
                {
                    SCIBusTxMessageRemoveItemButton.Enabled = false;
                    SCIBusTxMessageClearListButton.Enabled = false;
                    SCIBusSendMessagesButton.Enabled = false;
                    SCIBusStopRepeatedMessagesButton.Enabled = false;
                }
            }
        }

        private void SCIBusTxMessageClearListButton_Click(object sender, EventArgs e)
        {
            SCIBusTxMessagesListBox.Items.Clear();
            SCIBusTxMessageRemoveItemButton.Enabled = false;
            SCIBusTxMessageClearListButton.Enabled = false;
            SCIBusSendMessagesButton.Enabled = false;
            SCIBusStopRepeatedMessagesButton.Enabled = false;
        }

        private void SCIBusSendMessagesButton_Click(object sender, EventArgs e)
        {
            if (!SCIBusTxMessageRepeatIntervalCheckBox.Checked) // no repeat
            {
                if (SCIBusTxMessagesListBox.Items.Count == 1) // single message once
                {
                    byte[] message = Util.HexStringToByte(SCIBusTxMessagesListBox.Items[0].ToString());

                    switch (SCIBusModuleComboBox.SelectedIndex)
                    {
                        case 0: // engine
                        {
                            Packet packet = new Packet();

                            packet.Bus = (byte)PacketHelper.Bus.PCM;
                            packet.Command = (byte)PacketHelper.Command.MsgTx;
                            packet.Mode = (byte)PacketHelper.MsgTxMode.Single;
                            packet.Payload = message;

                            Util.UpdateTextBox(USBTextBox, "[<-TX] Send an SCI-bus (PCM) message once::", PacketHelper.Serialize(packet));

                            if (message.Length > 0)
                            {
                                Util.UpdateTextBox(USBTextBox, "[INFO] SCI-bus (PCM) message Tx list:" + Environment.NewLine +
                                                               "       " + Util.ByteToHexStringSimple(message));
                            }

                            SerialService.WritePacket(packet);
                            break;
                        }
                        case 1: // transmission
                        {
                            Packet packet = new Packet();

                            packet.Bus = (byte)PacketHelper.Bus.TCM;
                            packet.Command = (byte)PacketHelper.Command.MsgTx;
                            packet.Mode = (byte)PacketHelper.MsgTxMode.Single;
                            packet.Payload = message;

                            Util.UpdateTextBox(USBTextBox, "[<-TX] Send an SCI-bus (TCM) message once:", PacketHelper.Serialize(packet));

                            if (message.Length > 0)
                            {
                                Util.UpdateTextBox(USBTextBox, "[INFO] SCI-bus (TCM) message Tx list:" + Environment.NewLine +
                                                               "       " + Util.ByteToHexStringSimple(message));
                            }

                            SerialService.WritePacket(packet);
                            break;
                        }
                    }
                }
                else // list of messages once
                {
                    List<byte[]> messages = new List<byte[]>();
                    List<byte> payloadList = new List<byte>();
                    byte messageCount = (byte)SCIBusTxMessagesListBox.Items.Count;

                    for (int i = 0; i < messageCount; i++)
                    {
                        messages.Add(Util.HexStringToByte(SCIBusTxMessagesListBox.Items[i].ToString()));
                    }

                    payloadList.Add(messageCount); // number of messages

                    for (int i = 0; i < messageCount; i++)
                    {
                        payloadList.Add((byte)messages[i].Length); // message length
                        payloadList.AddRange(messages[i]); // message
                    }

                    StringBuilder messageList = new StringBuilder();

                    switch (SCIBusModuleComboBox.SelectedIndex)
                    {
                        case 0: // engine
                        {
                            Packet packet = new Packet();

                            packet.Bus = (byte)PacketHelper.Bus.PCM;
                            packet.Command = (byte)PacketHelper.Command.MsgTx;
                            packet.Mode = (byte)PacketHelper.MsgTxMode.List;
                            packet.Payload = payloadList.ToArray();

                            Util.UpdateTextBox(USBTextBox, "[<-TX] Send SCI-bus (PCM) message list once:", PacketHelper.Serialize(packet));

                            foreach (byte[] item in messages)
                            {
                                messageList.Append("       " + Util.ByteToHexStringSimple(item) + Environment.NewLine);
                            }

                            messageList.Replace(Environment.NewLine, string.Empty, messageList.Length - 2, 2); // remove last newline character

                            if (messages.Count > 0)
                            {
                                Util.UpdateTextBox(USBTextBox, "[INFO] SCI-bus (PCM) message Tx list:" + Environment.NewLine + messageList.ToString());
                            }

                            SerialService.WritePacket(packet);
                            break;
                        }
                        case 1: // transmission
                        {
                            Packet packet = new Packet();

                            packet.Bus = (byte)PacketHelper.Bus.TCM;
                            packet.Command = (byte)PacketHelper.Command.MsgTx;
                            packet.Mode = (byte)PacketHelper.MsgTxMode.List;
                            packet.Payload = payloadList.ToArray();

                            Util.UpdateTextBox(USBTextBox, "[<-TX] Send SCI-bus (TCM) message list once:", PacketHelper.Serialize(packet));

                            foreach (byte[] item in messages)
                            {
                                messageList.Append("       " + Util.ByteToHexStringSimple(item) + Environment.NewLine);
                            }

                            messageList.Replace(Environment.NewLine, string.Empty, messageList.Length - 2, 2); // remove last newline character

                            if (messages.Count > 0)
                            {
                                Util.UpdateTextBox(USBTextBox, "[INFO] SCI-bus (TCM) message Tx list:" + Environment.NewLine + messageList.ToString());
                            }

                            SerialService.WritePacket(packet);
                            break;
                        }
                    }
                }
            }
            else // repeat message(s) forever
            {
                if (SCIBusTxMessagesListBox.Items.Count == 1) // repeated single message
                {
                    byte[] message = Util.HexStringToByte(SCIBusTxMessagesListBox.Items[0].ToString());
                    List<byte> payloadList = new List<byte>();

                    payloadList.AddRange(message);

                    switch (SCIBusModuleComboBox.SelectedIndex)
                    {
                        case 0: // engine 
                        {
                            SetRepeatedMessageBehavior(PacketHelper.Bus.PCM); // first send a settings packet to configure repeat behavior

                            Packet packet = new Packet();

                            packet.Bus = (byte)PacketHelper.Bus.PCM;
                            packet.Command = (byte)PacketHelper.Command.MsgTx;
                            packet.Mode = (byte)PacketHelper.MsgTxMode.RepeatedSingle;
                            packet.Payload = payloadList.ToArray();

                            Util.UpdateTextBox(USBTextBox, "[<-TX] Send a repeated SCI-bus (PCM) message:", PacketHelper.Serialize(packet));

                            if (message.Length > 0)
                            {
                                Util.UpdateTextBox(USBTextBox, "[INFO] SCI-bus (PCM) message Tx list:" + Environment.NewLine +
                                                               "       " + Util.ByteToHexStringSimple(message));
                            }

                            SerialService.WritePacket(packet);
                            break;
                        }
                        case 1: // transmission
                        {
                            SetRepeatedMessageBehavior(PacketHelper.Bus.TCM); // first send a settings packet to configure repeat behavior

                            Packet packet = new Packet();

                            packet.Bus = (byte)PacketHelper.Bus.TCM;
                            packet.Command = (byte)PacketHelper.Command.MsgTx;
                            packet.Mode = (byte)PacketHelper.MsgTxMode.RepeatedSingle;
                            packet.Payload = payloadList.ToArray();

                            Util.UpdateTextBox(USBTextBox, "[<-TX] Send a repeated SCI-bus (TCM) message:", PacketHelper.Serialize(packet));

                            if (message.Length > 0)
                            {
                                Util.UpdateTextBox(USBTextBox, "[INFO] SCI-bus (TCM) message Tx list:" + Environment.NewLine +
                                                               "       " + Util.ByteToHexStringSimple(message));
                            }

                            SerialService.WritePacket(packet);
                            break;
                        }
                    }
                }
                else // repeated list of messages
                {
                    List<byte[]> messages = new List<byte[]>();
                    List<byte> payloadList = new List<byte>();
                    byte messageCount = (byte)SCIBusTxMessagesListBox.Items.Count;

                    for (int i = 0; i < messageCount; i++)
                    {
                        messages.Add(Util.HexStringToByte(SCIBusTxMessagesListBox.Items[i].ToString()));
                    }

                    payloadList.Add(messageCount); // number of messages

                    for (int i = 0; i < messageCount; i++)
                    {
                        payloadList.Add((byte)messages[i].Length); // message length
                        payloadList.AddRange(messages[i]); // message
                    }

                    StringBuilder messageList = new StringBuilder();

                    switch (SCIBusModuleComboBox.SelectedIndex)
                    {
                        case 0: // engine
                        {
                            SetRepeatedMessageBehavior(PacketHelper.Bus.PCM); // first send a settings packet to configure repeat behavior

                            Packet packet = new Packet();

                            packet.Bus = (byte)PacketHelper.Bus.PCM;
                            packet.Command = (byte)PacketHelper.Command.MsgTx;
                            packet.Mode = (byte)PacketHelper.MsgTxMode.RepeatedList;
                            packet.Payload = payloadList.ToArray();

                            Util.UpdateTextBox(USBTextBox, "[<-TX] Send repeated SCI-bus (PCM) message list:", PacketHelper.Serialize(packet));

                            foreach (byte[] item in messages)
                            {
                                messageList.Append("       " + Util.ByteToHexStringSimple(item) + Environment.NewLine);
                            }

                            messageList.Replace(Environment.NewLine, string.Empty, messageList.Length - 2, 2); // remove last newline character

                            if (messages.Count > 0) Util.UpdateTextBox(USBTextBox, "[INFO] SCI-bus (PCM) message Tx list:" + Environment.NewLine + messageList.ToString());

                            SerialService.WritePacket(packet);
                            break;
                        }
                        case 1: // transmission
                        {
                            SetRepeatedMessageBehavior(PacketHelper.Bus.TCM); // first send a settings packet to configure repeat behavior

                            Packet packet = new Packet();

                            packet.Bus = (byte)PacketHelper.Bus.TCM;
                            packet.Command = (byte)PacketHelper.Command.MsgTx;
                            packet.Mode = (byte)PacketHelper.MsgTxMode.RepeatedList;
                            packet.Payload = payloadList.ToArray();

                            Util.UpdateTextBox(USBTextBox, "[<-TX] Send repeated SCI-bus (TCM) message list:", PacketHelper.Serialize(packet));

                            foreach (byte[] item in messages)
                            {
                                messageList.Append("       " + Util.ByteToHexStringSimple(item) + Environment.NewLine);
                            }

                            messageList.Replace(Environment.NewLine, string.Empty, messageList.Length - 2, 2); // remove last newline character

                            if (messages.Count > 0) Util.UpdateTextBox(USBTextBox, "[INFO] SCI-bus (TCM) message Tx list:" + Environment.NewLine + messageList.ToString());

                            SerialService.WritePacket(packet);
                            break;
                        }
                    }
                }
            }
        }

        private void SCIBusStopRepeatedMessagesButton_Click(object sender, EventArgs e)
        {
            Packet packet = new Packet();

            packet.Bus = (byte)PacketHelper.Bus.PCM;
            packet.Command = (byte)PacketHelper.Command.MsgTx;
            packet.Mode = (byte)PacketHelper.MsgTxMode.Stop;
            packet.Payload = null;

            Util.UpdateTextBox(USBTextBox, "[<-TX] Stop repeated Tx on SCI-bus (PCM):", PacketHelper.Serialize(packet));

            SerialService.WritePacket(packet);
        }

        private void SCIBusModuleConfigSpeedApplyButton_Click(object sender, EventArgs e)
        {
            byte config = 0x00;

            switch (SCIBusModuleComboBox.SelectedIndex)
            {
                case 0: // engine
                {
                    config = Util.ClearBit(config, 5); // PCM
                    break;
                }
                case 1: // transmission
                {
                    config = Util.SetBit(config, 5); // TCM
                    break;
                }
            }

            switch (SCIBusLogicComboBox.SelectedIndex)
            {
                case 0: // OBD1 SBEC
                {
                    config = Util.SetBit(config, 6); // SBEC bit
                    config = Util.SetBit(config, 3); // inverted logic bit
                    break;
                }
                case 1: // OBD1 JTEC
                {
                    config = Util.SetBit(config, 3); // inverted logic bit
                    break;
                }
                case 3: // OBD2 NGC
                {
                    config = Util.SetBit(config, 4); // ngc bit
                    break;
                }
            }

            if (SCIBusOBDConfigurationComboBox.SelectedIndex == 0)
            {
                config = Util.ClearBit(config, 2);
            }
            else
            {
                config = Util.SetBit(config, 2);
            }

            switch (SCIBusSpeedComboBox.SelectedIndex)
            {
                case 0: // off
                {
                    config = Util.ClearBit(config, 7); // clear state bit (disabled)
                    config = Util.ClearBit(config, 1);
                    config = Util.SetBit(config, 0);
                    break;
                }
                case 1: // 976.5 baud
                {
                    config = Util.SetBit(config, 7); // set state bit (enabled)
                    config = Util.ClearBit(config, 1);
                    config = Util.ClearBit(config, 0);
                    break;
                }
                case 2: // 7812.5 baud
                {
                    config = Util.SetBit(config, 7); // set state bit (enabled)
                    config = Util.ClearBit(config, 1);
                    config = Util.SetBit(config, 0);
                    break;
                }
                case 3: // 62500 baud
                {
                    config = Util.SetBit(config, 7); // set state bit (enabled)
                    config = Util.SetBit(config, 1);
                    config = Util.ClearBit(config, 0);
                    break;
                }
                case 4: // 125000 baud
                {
                    config = Util.SetBit(config, 7); // set state bit (enabled)
                    config = Util.SetBit(config, 1);
                    config = Util.SetBit(config, 0);
                    break;
                }
                default: // 7812.5 baud
                {
                    config = Util.SetBit(config, 7); // set state bit (enabled)
                    config = Util.ClearBit(config, 1);
                    config = Util.SetBit(config, 0);
                    break;
                }
            }

            Packet packet = new Packet();

            packet.Bus = (byte)PacketHelper.Bus.USB;
            packet.Command = (byte)PacketHelper.Command.Settings;
            packet.Mode = (byte)PacketHelper.SettingsMode.SetSCIBus;
            packet.Payload = new byte[1] { config };

            Util.UpdateTextBox(USBTextBox, "[<-TX] Change SCI-bus settings:", PacketHelper.Serialize(packet));

            SerialService.WritePacket(packet);
        }

        private void SCIBusTxMessagesListBox_DoubleClick(object sender, EventArgs e)
        {
            if ((SCIBusTxMessagesListBox.Items.Count > 0) && (SCIBusTxMessagesListBox.SelectedIndex > -1))
            {
                string selectedItem = SCIBusTxMessagesListBox.SelectedItem.ToString();
                SCIBusTxMessageComboBox.Text = selectedItem;
                SCIBusTxMessageComboBox.Focus();
                SCIBusTxMessageComboBox.SelectionStart = SCIBusTxMessageComboBox.Text.Length;
                SCIBusTxMessageAddButton.Text = "Edit";
            }
        }

        private void SCIBusTxMessageRepeatIntervalCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (SCIBusTxMessageRepeatIntervalCheckBox.Checked)
            {
                SCIBusTxMessageRepeatIntervalTextBox.Enabled = true;
                MillisecondsLabel05.Enabled = true;
            }
            else
            {
                SCIBusTxMessageRepeatIntervalTextBox.Enabled = false;
                MillisecondsLabel05.Enabled = false;
            }
        }

        private void SCIBusModuleComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            BeginInvoke((MethodInvoker)delegate
            {
                switch (SCIBusModuleComboBox.SelectedIndex)
                {
                    case 0: // PCM
                    {
                        if (PCM.state == "enabled")
                        {
                            if (PCM.speed == "976.5 baud") SCIBusSpeedComboBox.SelectedIndex = 1;
                            else if (PCM.speed == "7812.5 baud") SCIBusSpeedComboBox.SelectedIndex = 2;
                            else if (PCM.speed == "62500 baud") SCIBusSpeedComboBox.SelectedIndex = 3;
                            else if (PCM.speed == "125000 baud") SCIBusSpeedComboBox.SelectedIndex = 4;
                        }
                        else if (PCM.state == "disabled")
                        {
                            SCIBusSpeedComboBox.SelectedIndex = 0; // off
                        }

                        if (PCM.logic == "non-inverted") SCIBusLogicComboBox.SelectedIndex = 2; // OBD2
                        else if (PCM.logic == "inverted") SCIBusLogicComboBox.SelectedIndex = 1; // OBD1 JTEC

                        // TODO: decide SBEC 4-bit exchange business

                        if (PCM.configuration == "A") SCIBusOBDConfigurationComboBox.SelectedIndex = 0;
                        else if (PCM.configuration == "B") SCIBusOBDConfigurationComboBox.SelectedIndex = 1;

                        PCMSelected = true;
                        TCMSelected = false;
                        break;
                    }
                    case 1: // TCM
                    {
                        if (TCM.state == "enabled")
                        {
                            if (TCM.speed == "976.5 baud") SCIBusSpeedComboBox.SelectedIndex = 1;
                            else if (TCM.speed == "7812.5 baud") SCIBusSpeedComboBox.SelectedIndex = 2;
                            else if (TCM.speed == "62500 baud") SCIBusSpeedComboBox.SelectedIndex = 3;
                            else if (TCM.speed == "125000 baud") SCIBusSpeedComboBox.SelectedIndex = 4;
                        }
                        else if (TCM.state == "disabled")
                        {
                            SCIBusSpeedComboBox.SelectedIndex = 0; // off
                        }

                        if (TCM.logic == "non-inverted") SCIBusLogicComboBox.SelectedIndex = 2; // OBD2
                        else if (TCM.logic == "inverted") SCIBusLogicComboBox.SelectedIndex = 1; // OBD1 JTEC

                        // TODO: decide SBEC 4-bit exchange business

                        if (TCM.configuration == "A") SCIBusOBDConfigurationComboBox.SelectedIndex = 0;
                        else if (TCM.configuration == "B") SCIBusOBDConfigurationComboBox.SelectedIndex = 1;

                        PCMSelected = false;
                        TCMSelected = true;
                        break;
                    }
                }
            });
        }

        private void SCIBusTxMessageRepeatIntervalTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                e.Handled = true;

                switch (SCIBusModuleComboBox.SelectedIndex)
                {
                    case 0: // engine
                    {
                        SetRepeatedMessageBehavior(PacketHelper.Bus.PCM);
                        break;
                    }
                    case 1: // transmission
                    {
                        SetRepeatedMessageBehavior(PacketHelper.Bus.TCM);
                        break;
                    }
                }
            }
        }

        private void SCIBusTxMessageComboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                e.Handled = true;

                if (SCIBusTxMessageComboBox.Text.Length < 2) return;

                if (SCIBusTxMessageComboBox.Text.Contains(","))
                {
                    SCIBusTxMessageAddButton_Click(this, EventArgs.Empty);

                    if (!SCIBusTxMessageComboBox.Items.Contains(SCIBusTxMessageComboBox.Text)) // only add unique items (no repeat!)
                    {
                        SCIBusTxMessageComboBox.Items.Add(SCIBusTxMessageComboBox.Text); // add message to the list so it can be selected later
                    }

                    SCIBusTxMessageComboBox.Text = string.Empty;
                    return;
                }
                
                byte[] message = Util.HexStringToByte(SCIBusTxMessageComboBox.Text);

                if (SCIBusTxMessageAddButton.Text != "Edit")
                {
                    if (SCIBusTxMessageChecksumCheckBox.Checked && (message.Length > 1))
                    {
                        message[message.Length - 1] = Util.ChecksumCalculator(message, 0, message.Length - 1);

                        SCIBusTxMessageComboBox.Text = Util.ByteToHexStringSimple(message);
                        SCIBusTxMessageComboBox.SelectionStart = SCIBusTxMessageComboBox.Text.Length;
                        SCIBusTxMessageComboBox.SelectionLength = 0;
                    }

                    switch (SCIBusModuleComboBox.SelectedIndex)
                    {
                        case 0: // engine
                        {
                            Packet packet = new Packet();

                            packet.Bus = (byte)PacketHelper.Bus.PCM;
                            packet.Command = (byte)PacketHelper.Command.MsgTx;
                            packet.Mode = (byte)PacketHelper.MsgTxMode.Single;
                            packet.Payload = message;

                            Util.UpdateTextBox(USBTextBox, "[<-TX] Send an SCI-bus (PCM) message once:", PacketHelper.Serialize(packet));

                            if (message.Length > 0)
                            {
                                Util.UpdateTextBox(USBTextBox, "[INFO] SCI-bus (PCM) message Tx list:" + Environment.NewLine +
                                                               "       " + Util.ByteToHexStringSimple(message));
                            }

                            SerialService.WritePacket(packet);
                            break;
                        }
                        case 1: // transmission
                        {
                            Packet packet = new Packet();

                            packet.Bus = (byte)PacketHelper.Bus.TCM;
                            packet.Command = (byte)PacketHelper.Command.MsgTx;
                            packet.Mode = (byte)PacketHelper.MsgTxMode.Single;
                            packet.Payload = message;

                            Util.UpdateTextBox(USBTextBox, "[<-TX] Send an SCI-bus (TCM) message once:", PacketHelper.Serialize(packet));

                            if (message.Length > 0)
                            {
                                Util.UpdateTextBox(USBTextBox, "[INFO] SCI-bus (TCM) message Tx list:" + Environment.NewLine +
                                                               "       " + Util.ByteToHexStringSimple(message));
                            }

                            SerialService.WritePacket(packet);
                            break;
                        }
                    }
                }
                else
                {
                    if (SCIBusTxMessageChecksumCheckBox.Checked && (message.Length > 1))
                    {
                        message[message.Length - 1] = Util.ChecksumCalculator(message, 0, message.Length - 1);

                        SCIBusTxMessageComboBox.Text = Util.ByteToHexStringSimple(message);
                        SCIBusTxMessageComboBox.SelectionStart = SCIBusTxMessageComboBox.Text.Length;
                        SCIBusTxMessageComboBox.SelectionLength = 0;
                    }

                    SCIBusTxMessageAddButton_Click(this, EventArgs.Empty);
                }

                if (!SCIBusTxMessageComboBox.Items.Contains(SCIBusTxMessageComboBox.Text)) // only add unique items (no repeat!)
                {
                    SCIBusTxMessageComboBox.Items.Add(SCIBusTxMessageComboBox.Text); // add message to the list so it can be selected later
                }
            }
        }

        #endregion

        #region PCI-bus tab

        private void PCIBusTxMessageAddButton_Click(object sender, EventArgs e)
        {
            string[] messageStrings = PCIBusTxMessageComboBox.Text.Split(','); // multiple messages can be separated by commas

            foreach (string element in messageStrings)
            {
                byte[] message = Util.HexStringToByte(element);
                if (message.Length > 0)
                {
                    byte messageID = message[0];
                    bool match = false;
                    int rowNumber = 0;

                    if (PCIBusTxMessageCRCCheckBox.Checked && (message.Length > 1))
                    {
                        message[message.Length - 1] = Util.CRCCalculator(message, 0, message.Length - 1);

                        PCIBusTxMessageComboBox.Text = Util.ByteToHexStringSimple(message);
                        PCIBusTxMessageComboBox.SelectionStart = PCIBusTxMessageComboBox.Text.Length;
                        PCIBusTxMessageComboBox.SelectionLength = 0;
                    }

                    for (int i = 0; i < PCIBusTxMessagesListBox.Items.Count; i++)
                    {
                        byte[] item = Util.HexStringToByte(PCIBusTxMessagesListBox.Items[i].ToString());
                        if (item[0] == messageID)
                        {
                            match = true;
                            rowNumber = i;
                            break;
                        }
                    }

                    string itemToInsert = Util.ByteToHexString(message, 0, message.Length);

                    if (!match)
                    {
                        PCIBusTxMessagesListBox.Items.Add(itemToInsert);
                    }
                    else
                    {
                        if (PCIBusOverwriteDuplicateIDCheckBox.Checked && !PCIBusTxMessageRepeatIntervalCheckBox.Checked)
                        {
                            PCIBusTxMessagesListBox.Items.RemoveAt(rowNumber);
                            PCIBusTxMessagesListBox.Items.Insert(rowNumber, itemToInsert);
                        }
                        else
                        {
                            PCIBusTxMessagesListBox.Items.Add(itemToInsert);
                        }
                    }

                    if (PCIBusTxMessagesListBox.Items.Count > 0)
                    {
                        PCIBusTxMessageRemoveItemButton.Enabled = true;
                        PCIBusTxMessageClearListButton.Enabled = true;
                        PCIBusSendMessagesButton.Enabled = true;
                        PCIBusStopRepeatedMessagesButton.Enabled = true;
                    }
                    else
                    {
                        PCIBusTxMessageRemoveItemButton.Enabled = false;
                        PCIBusTxMessageClearListButton.Enabled = false;
                        PCIBusSendMessagesButton.Enabled = false;
                        PCIBusStopRepeatedMessagesButton.Enabled = false;
                    }

                    if (!PCIBusTxMessageComboBox.Items.Contains(PCIBusTxMessageComboBox.Text)) // only add unique items (no repeat!)
                    {
                        PCIBusTxMessageComboBox.Items.Add(PCIBusTxMessageComboBox.Text); // add message to the list so it can be selected later
                    }

                    if (PCIBusTxMessageAddButton.Text == "Edit") PCIBusTxMessageAddButton.Text = "Add";
                }
            }

            CCDBusTxMessagesListBox.TopIndex = CCDBusTxMessagesListBox.Items.Count - 1; // scroll down
        }

        private void PCIBusTxMessageRemoveItemButton_Click(object sender, EventArgs e)
        {
            if (PCIBusTxMessagesListBox.SelectedIndex > -1)
            {
                for (int i = PCIBusTxMessagesListBox.SelectedIndices.Count - 1; i >= 0; i--)
                {
                    int index = PCIBusTxMessagesListBox.SelectedIndices[i];
                    PCIBusTxMessagesListBox.Items.RemoveAt(index);
                }

                if (PCIBusTxMessagesListBox.Items.Count > 0)
                {
                    PCIBusTxMessageRemoveItemButton.Enabled = true;
                    PCIBusTxMessageClearListButton.Enabled = true;
                    PCIBusSendMessagesButton.Enabled = true;
                    PCIBusStopRepeatedMessagesButton.Enabled = true;
                }
                else
                {
                    PCIBusTxMessageRemoveItemButton.Enabled = false;
                    PCIBusTxMessageClearListButton.Enabled = false;
                    PCIBusSendMessagesButton.Enabled = false;
                    PCIBusStopRepeatedMessagesButton.Enabled = false;
                }
            }
        }

        private void PCIBusTxMessageClearListButton_Click(object sender, EventArgs e)
        {
            PCIBusTxMessagesListBox.Items.Clear();
            PCIBusTxMessageRemoveItemButton.Enabled = false;
            PCIBusTxMessageClearListButton.Enabled = false;
            PCIBusSendMessagesButton.Enabled = false;
            PCIBusStopRepeatedMessagesButton.Enabled = false;
        }

        private void PCIBusSendMessagesButton_Click(object sender, EventArgs e)
        {
            if (!PCIBusTxMessageRepeatIntervalCheckBox.Checked) // no repeat
            {
                if (PCIBusTxMessagesListBox.Items.Count == 1) // single message once
                {
                    byte[] message = Util.HexStringToByte(PCIBusTxMessagesListBox.Items[0].ToString());

                    Packet packet = new Packet();

                    packet.Bus = (byte)PacketHelper.Bus.PCI;
                    packet.Command = (byte)PacketHelper.Command.MsgTx;
                    packet.Mode = (byte)PacketHelper.MsgTxMode.Single;
                    packet.Payload = message;

                    Util.UpdateTextBox(USBTextBox, "[<-TX] Send a PCI-bus message once:", PacketHelper.Serialize(packet));

                    if (message.Length > 0)
                    {
                        Util.UpdateTextBox(USBTextBox, "[INFO] PCI-bus message Tx list:" + Environment.NewLine +
                                                       "       " + Util.ByteToHexStringSimple(message));
                    }

                    SerialService.WritePacket(packet);
                }
                else // list of messages once
                {
                    List<byte[]> messages = new List<byte[]>();
                    List<byte> payloadList = new List<byte>();
                    byte messageCount = (byte)PCIBusTxMessagesListBox.Items.Count;

                    for (int i = 0; i < messageCount; i++)
                    {
                        messages.Add(Util.HexStringToByte(PCIBusTxMessagesListBox.Items[i].ToString()));
                    }

                    payloadList.Add(messageCount); // number of messages

                    for (int i = 0; i < messageCount; i++)
                    {
                        payloadList.Add((byte)messages[i].Length); // message length
                        payloadList.AddRange(messages[i]); // message
                    }

                    Packet packet = new Packet();

                    packet.Bus = (byte)PacketHelper.Bus.PCI;
                    packet.Command = (byte)PacketHelper.Command.MsgTx;
                    packet.Mode = (byte)PacketHelper.MsgTxMode.List;
                    packet.Payload = payloadList.ToArray();

                    Util.UpdateTextBox(USBTextBox, "[<-TX] Send PCI-bus message list once:", PacketHelper.Serialize(packet));

                    StringBuilder messageList = new StringBuilder();

                    foreach (byte[] item in messages)
                    {
                        messageList.Append("       " + Util.ByteToHexStringSimple(item) + Environment.NewLine);
                    }

                    messageList.Replace(Environment.NewLine, string.Empty, messageList.Length - 2, 2); // remove last newline character

                    if (messages.Count > 0) Util.UpdateTextBox(USBTextBox, "[INFO] PCI-bus message Tx list:" + Environment.NewLine + messageList.ToString());

                    SerialService.WritePacket(packet);
                }
            }
            else // repeat message(s) forever
            {
                if (PCIBusTxMessagesListBox.Items.Count == 1) // repeated single message
                {
                    // First send a settings packet to configure repeat behavior
                    SetRepeatedMessageBehavior(PacketHelper.Bus.PCI);

                    byte[] message = Util.HexStringToByte(PCIBusTxMessagesListBox.Items[0].ToString());
                    List<byte> payloadList = new List<byte>();

                    payloadList.AddRange(message);

                    Packet packet = new Packet();

                    packet.Bus = (byte)PacketHelper.Bus.PCI;
                    packet.Command = (byte)PacketHelper.Command.MsgTx;
                    packet.Mode = (byte)PacketHelper.MsgTxMode.RepeatedSingle;
                    packet.Payload = payloadList.ToArray();

                    Util.UpdateTextBox(USBTextBox, "[<-TX] Send a repeated PCI-bus message:", PacketHelper.Serialize(packet));

                    if (message.Length > 0)
                    {
                        Util.UpdateTextBox(USBTextBox, "[INFO] PCI-bus message Tx list:" + Environment.NewLine +
                                                       "       " + Util.ByteToHexStringSimple(message));
                    }

                    SerialService.WritePacket(packet);
                }
                else // repeated list of messages
                {
                    // First send a settings packet to configure repeat behavior
                    SetRepeatedMessageBehavior(PacketHelper.Bus.PCI);

                    List<byte[]> messages = new List<byte[]>();
                    List<byte> payloadList = new List<byte>();
                    byte messageCount = (byte)PCIBusTxMessagesListBox.Items.Count;

                    for (int i = 0; i < messageCount; i++)
                    {
                        messages.Add(Util.HexStringToByte(PCIBusTxMessagesListBox.Items[i].ToString()));
                    }

                    payloadList.Add(messageCount); // number of messages

                    for (int i = 0; i < messageCount; i++)
                    {
                        payloadList.Add((byte)messages[i].Length); // message length
                        payloadList.AddRange(messages[i]); // message
                    }

                    Packet packet = new Packet();

                    packet.Bus = (byte)PacketHelper.Bus.PCI;
                    packet.Command = (byte)PacketHelper.Command.MsgTx;
                    packet.Mode = (byte)PacketHelper.MsgTxMode.RepeatedList;
                    packet.Payload = payloadList.ToArray();

                    Util.UpdateTextBox(USBTextBox, "[<-TX] Send repeated PCI-bus message list:", PacketHelper.Serialize(packet));

                    StringBuilder messageList = new StringBuilder();

                    foreach (byte[] item in messages)
                    {
                        messageList.Append("       " + Util.ByteToHexStringSimple(item) + Environment.NewLine);
                    }

                    messageList.Replace(Environment.NewLine, string.Empty, messageList.Length - 2, 2); // remove last newline character

                    if (messages.Count > 0) Util.UpdateTextBox(USBTextBox, "[INFO] PCI-bus message Tx list:" + Environment.NewLine + messageList.ToString());

                    SerialService.WritePacket(packet);
                }
            }
        }

        private void PCIBusStopRepeatedMessagesButton_Click(object sender, EventArgs e)
        {
            Packet packet = new Packet();

            packet.Bus = (byte)PacketHelper.Bus.PCI;
            packet.Command = (byte)PacketHelper.Command.MsgTx;
            packet.Mode = (byte)PacketHelper.MsgTxMode.Stop;
            packet.Payload = null;

            Util.UpdateTextBox(USBTextBox, "[<-TX] Stop repeated Tx on PCI-bus:", PacketHelper.Serialize(packet));

            SerialService.WritePacket(packet);
        }

        private void PCIBusTxMessagesListBox_DoubleClick(object sender, EventArgs e)
        {
            if ((PCIBusTxMessagesListBox.Items.Count > 0) && (PCIBusTxMessagesListBox.SelectedIndex > -1))
            {
                string selectedItem = PCIBusTxMessagesListBox.SelectedItem.ToString();
                PCIBusTxMessageComboBox.Text = selectedItem;
                PCIBusTxMessageComboBox.Focus();
                PCIBusTxMessageComboBox.SelectionStart = PCIBusTxMessageComboBox.Text.Length;
                PCIBusTxMessageAddButton.Text = "Edit";
            }
        }

        private void PCIBusTxMessageRepeatIntervalCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (PCIBusTxMessageRepeatIntervalCheckBox.Checked)
            {
                PCIBusTxMessageRepeatIntervalTextBox.Enabled = true;
                MillisecondsLabel06.Enabled = true;
            }
            else
            {
                PCIBusTxMessageRepeatIntervalTextBox.Enabled = false;
                MillisecondsLabel06.Enabled = false;
            }
        }

        private void PCIBusSettingsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            byte config = 0x40; // keep logic bit high (active-high)

            if (PCIBusTransceiverOnOffCheckBox.Checked)
            {
                config = Util.SetBit(config, 7);
                PCIBusTransceiverOnOffCheckBox.Text = "PCI-bus transceiver ON";
                PCI.UpdateHeader("enabled", null, null);
            }
            else
            {
                config = Util.ClearBit(config, 7);
                PCIBusTransceiverOnOffCheckBox.Text = "PCI-bus transceiver OFF";
                PCI.UpdateHeader("disabled", null, null);
            }

            Packet packet = new Packet();

            packet.Bus = (byte)PacketHelper.Bus.USB;
            packet.Command = (byte)PacketHelper.Command.Settings;
            packet.Mode = (byte)PacketHelper.SettingsMode.SetPCIBus;
            packet.Payload = new byte[1] { config };

            Util.UpdateTextBox(USBTextBox, "[<-TX] Change PCI-bus settings:", PacketHelper.Serialize(packet));

            SerialService.WritePacket(packet);
        }

        private void PCIBusTxMessageRepeatIntervalTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                e.Handled = true;
                SetRepeatedMessageBehavior(PacketHelper.Bus.PCI);
            }
        }

        private void PCIBusTxMessageComboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                e.Handled = true;

                if (PCIBusTxMessageComboBox.Text.Length < 2) return;

                if (PCIBusTxMessageComboBox.Text.Contains(","))
                {
                    PCIBusTxMessageAddButton_Click(this, EventArgs.Empty);

                    if (!PCIBusTxMessageComboBox.Items.Contains(PCIBusTxMessageComboBox.Text)) // only add unique items (no repeat!)
                    {
                        PCIBusTxMessageComboBox.Items.Add(PCIBusTxMessageComboBox.Text); // add message to the list so it can be selected later
                    }

                    PCIBusTxMessageComboBox.Text = string.Empty;
                    return;
                }

                byte[] message = Util.HexStringToByte(PCIBusTxMessageComboBox.Text);

                if (PCIBusTxMessageAddButton.Text != "Edit")
                {
                    if (PCIBusTxMessageCRCCheckBox.Checked && (message.Length > 1))
                    {
                        message[message.Length - 1] = Util.CRCCalculator(message, 0, message.Length - 1);

                        PCIBusTxMessageComboBox.Text = Util.ByteToHexStringSimple(message);
                        PCIBusTxMessageComboBox.SelectionStart = PCIBusTxMessageComboBox.Text.Length;
                        PCIBusTxMessageComboBox.SelectionLength = 0;
                    }

                    Packet packet = new Packet();

                    packet.Bus = (byte)PacketHelper.Bus.PCI;
                    packet.Command = (byte)PacketHelper.Command.MsgTx;
                    packet.Mode = (byte)PacketHelper.MsgTxMode.Single;
                    packet.Payload = message;

                    Util.UpdateTextBox(USBTextBox, "[<-TX] Send a PCI-bus message once:", PacketHelper.Serialize(packet));

                    if (message.Length > 0)
                    {
                        Util.UpdateTextBox(USBTextBox, "[INFO] PCI-bus message Tx list:" + Environment.NewLine +
                                                       "       " + Util.ByteToHexStringSimple(message));
                    }

                    SerialService.WritePacket(packet);
                }
                else
                {
                    if (PCIBusTxMessageCRCCheckBox.Checked && (message.Length > 1))
                    {
                        message[message.Length - 1] = Util.CRCCalculator(message, 0, message.Length - 1);

                        PCIBusTxMessageComboBox.Text = Util.ByteToHexStringSimple(message);
                        PCIBusTxMessageComboBox.SelectionStart = PCIBusTxMessageComboBox.Text.Length;
                        PCIBusTxMessageComboBox.SelectionLength = 0;
                    }

                    PCIBusTxMessageAddButton_Click(this, EventArgs.Empty);
                }

                if (!PCIBusTxMessageComboBox.Items.Contains(PCIBusTxMessageComboBox.Text)) // only add unique items (no repeat!)
                {
                    PCIBusTxMessageComboBox.Items.Add(PCIBusTxMessageComboBox.Text); // add message to the list so it can be selected later
                }
            }
        }

        #endregion

        #region Diagnostics tab

        private void DiagnosticsRefreshButton_Click(object sender, EventArgs e)
        {
            switch (DiagnosticsTabControl.SelectedTab.Name)
            {
                case "CCDBusDiagnosticsTabPage":
                {
                    CCDBusDiagnosticsListBox.Items.Clear();
                    CCDBusDiagnosticsListBox.Items.AddRange(CCD.Diagnostics.Table.ToArray());
                    break;
                }
                case "PCIBusDiagnosticsTabPage":
                {
                    PCIBusDiagnosticsListBox.Items.Clear();
                    PCIBusDiagnosticsListBox.Items.AddRange(PCI.Diagnostics.Table.ToArray());
                    break;
                }
                case "SCIBusPCMDiagnosticsTabPage":
                {
                    SCIBusPCMDiagnosticsListBox.Items.Clear();
                    SCIBusPCMDiagnosticsListBox.Items.AddRange(PCM.Diagnostics.Table.ToArray());
                    break;
                }
                case "SCIBusTCMDiagnosticsTabPage":
                {
                    SCIBusTCMDiagnosticsListBox.Items.Clear();
                    SCIBusTCMDiagnosticsListBox.Items.AddRange(TCM.Diagnostics.Table.ToArray());
                    break;
                }
            }
        }

        private void DiagnosticsResetViewButton_Click(object sender, EventArgs e)
        {
            switch (DiagnosticsTabControl.SelectedTab.Name)
            {
                case "CCDBusDiagnosticsTabPage":
                {
                    CCD.Diagnostics.IDByteList.Clear();
                    CCD.Diagnostics.UniqueIDByteList.Clear();
                    CCD.Diagnostics.B2F2IDByteList.Clear();
                    CCDTableBuffer.Clear();
                    CCDTableBufferLocation.Clear();
                    CCDTableRowCountHistory.Clear();
                    CCD.Diagnostics.LastUpdatedLine = 1;
                    CCD.Diagnostics.InitCCDTable();
                    CCDBusDiagnosticsListBox.Items.Clear();
                    CCDBusDiagnosticsListBox.Items.AddRange(CCD.Diagnostics.Table.ToArray());
                    break;
                }
                case "PCIBusDiagnosticsTabPage":
                {
                    PCI.Diagnostics.IDByteList.Clear();
                    PCI.Diagnostics.UniqueIDByteList.Clear();
                    PCI.Diagnostics.IDByte2426List.Clear();
                    PCITableBuffer.Clear();
                    PCITableBufferLocation.Clear();
                    PCITableRowCountHistory.Clear();
                    PCI.Diagnostics.LastUpdatedLine = 1;
                    PCI.Diagnostics.InitPCITable();
                    PCIBusDiagnosticsListBox.Items.Clear();
                    PCIBusDiagnosticsListBox.Items.AddRange(PCI.Diagnostics.Table.ToArray());
                    break;
                }
                case "SCIBusPCMDiagnosticsTabPage":
                {
                    PCM.Diagnostics.IDByteList.Clear();
                    PCM.Diagnostics.UniqueIDByteList.Clear();
                    PCMTableBuffer.Clear();
                    PCMTableBufferLocation.Clear();
                    PCMTableRowCountHistory.Clear();
                    PCM.Diagnostics.LastUpdatedLine = 1;
                    PCM.Diagnostics.InitSCIPCMTable();
                    PCM.Diagnostics.InitRAMDumpTable();
                    PCM.Diagnostics.RAMDumpTableVisible = false;
                    PCM.Diagnostics.RAMTableAddress = 0;
                    SCIBusPCMDiagnosticsListBox.Items.Clear();
                    SCIBusPCMDiagnosticsListBox.Items.AddRange(PCM.Diagnostics.Table.ToArray());
                    break;
                }
                case "SCIBusTCMDiagnosticsTabPage":
                {
                    TCM.Diagnostics.IDByteList.Clear();
                    TCM.Diagnostics.UniqueIDByteList.Clear();
                    TCMTableBuffer.Clear();
                    TCMTableBufferLocation.Clear();
                    TCMTableRowCountHistory.Clear();
                    TCM.Diagnostics.LastUpdatedLine = 1;
                    TCM.Diagnostics.InitSCITCMTable();
                    //TCM.Diagnostics.InitRAMDumpTable();
                    //TCM.Diagnostics.RAMDumpTableVisible = false;
                    //TCM.Diagnostics.RAMTableAddress = 0;
                    SCIBusTCMDiagnosticsListBox.Items.Clear();
                    SCIBusTCMDiagnosticsListBox.Items.AddRange(TCM.Diagnostics.Table.ToArray());
                    break;
                }
            }
        }

        private void DiagnosticsCopyToClipboardButton_Click(object sender, EventArgs e)
        {
            Clipboard.Clear();

            switch (DiagnosticsTabControl.SelectedTab.Name)
            {
                case "CCDBusDiagnosticsTabPage":
                {
                    Clipboard.SetText(string.Join(Environment.NewLine, CCD.Diagnostics.Table.ToArray()) + Environment.NewLine);
                    break;
                }
                case "PCIBusDiagnosticsTabPage":
                {
                    Clipboard.SetText(string.Join(Environment.NewLine, PCI.Diagnostics.Table.ToArray()) + Environment.NewLine);
                    break;
                }
                case "SCIBusPCMDiagnosticsTabPage":
                {
                    Clipboard.SetText(string.Join(Environment.NewLine, PCM.Diagnostics.Table.ToArray()) + Environment.NewLine);
                    break;
                }
                case "SCIBusTCMDiagnosticsTabPage":
                {
                    Clipboard.SetText(string.Join(Environment.NewLine, TCM.Diagnostics.Table.ToArray()) + Environment.NewLine);
                    break;
                }
            }
        }

        private void DiagnosticsSnapshotButton_Click(object sender, EventArgs e)
        {
            string DateTimeNow;
            int counter;

            switch (DiagnosticsTabControl.SelectedTab.Name)
            {
                case "CCDBusDiagnosticsTabPage":
                {
                    DateTimeNow = DateTime.Now.ToString("yyyyMMdd_HHmmss"); // new time for each snapshot
                    string CCDDiagnosticsSnapshotFilename = @"LOG/CCD/ccdsnapshot_" + DateTimeNow;
                    counter = 1;

                    while (File.Exists(CCDDiagnosticsSnapshotFilename + ".txt"))
                    {
                        if (CCDDiagnosticsSnapshotFilename.Length > 35) CCDDiagnosticsSnapshotFilename = CCDDiagnosticsSnapshotFilename.Remove(CCDDiagnosticsSnapshotFilename.Length - 3, 3); // remove last 3 characters
                        CCDDiagnosticsSnapshotFilename += "_";
                        if (counter < 10) CCDDiagnosticsSnapshotFilename += "0";
                        CCDDiagnosticsSnapshotFilename += counter.ToString();
                        counter++;
                    }

                    CCDDiagnosticsSnapshotFilename += ".txt";
                    File.AppendAllText(CCDDiagnosticsSnapshotFilename, string.Join(Environment.NewLine, CCD.Diagnostics.Table.ToArray()) + Environment.NewLine);
                    break;
                }
                case "PCIBusDiagnosticsTabPage":
                {
                    DateTimeNow = DateTime.Now.ToString("yyyyMMdd_HHmmss"); // new time for each snapshot
                    string PCIDiagnosticsSnapshotFilename = @"LOG/PCI/pcisnapshot_" + DateTimeNow;
                    counter = 1;

                    while (File.Exists(PCIDiagnosticsSnapshotFilename + ".txt"))
                    {
                        if (PCIDiagnosticsSnapshotFilename.Length > 35) PCIDiagnosticsSnapshotFilename = PCIDiagnosticsSnapshotFilename.Remove(PCIDiagnosticsSnapshotFilename.Length - 3, 3); // remove last 3 characters
                        PCIDiagnosticsSnapshotFilename += "_";
                        if (counter < 10) PCIDiagnosticsSnapshotFilename += "0";
                        PCIDiagnosticsSnapshotFilename += counter.ToString();
                        counter++;
                    }

                    PCIDiagnosticsSnapshotFilename += ".txt";
                    File.AppendAllText(PCIDiagnosticsSnapshotFilename, string.Join(Environment.NewLine, PCI.Diagnostics.Table.ToArray()) + Environment.NewLine);
                    break;
                }
                case "SCIBusPCMDiagnosticsTabPage":
                {
                    DateTimeNow = DateTime.Now.ToString("yyyyMMdd_HHmmss"); // new time for each snapshot
                    string PCMDiagnosticsSnapshotFilename = @"LOG/PCM/pcmsnapshot_" + DateTimeNow;
                    counter = 1;

                    while (File.Exists(PCMDiagnosticsSnapshotFilename + ".txt"))
                    {
                        if (PCMDiagnosticsSnapshotFilename.Length > 35) PCMDiagnosticsSnapshotFilename = PCMDiagnosticsSnapshotFilename.Remove(PCMDiagnosticsSnapshotFilename.Length - 3, 3); // remove last 3 characters
                        PCMDiagnosticsSnapshotFilename += "_";
                        if (counter < 10) PCMDiagnosticsSnapshotFilename += "0";
                        PCMDiagnosticsSnapshotFilename += counter.ToString();
                        counter++;
                    }

                    PCMDiagnosticsSnapshotFilename += ".txt";
                    File.AppendAllText(PCMDiagnosticsSnapshotFilename, string.Join(Environment.NewLine, PCM.Diagnostics.Table.ToArray()) + Environment.NewLine);
                    break;
                }
                case "SCIBusTCMDiagnosticsTabPage":
                {
                    DateTimeNow = DateTime.Now.ToString("yyyyMMdd_HHmmss"); // new time for each snapshot
                    string TCMDiagnosticsSnapshotFilename = @"LOG/TCM/tcmsnapshot_" + DateTimeNow;
                    counter = 1;

                    while (File.Exists(TCMDiagnosticsSnapshotFilename + ".txt"))
                    {
                        if (TCMDiagnosticsSnapshotFilename.Length > 35) TCMDiagnosticsSnapshotFilename = TCMDiagnosticsSnapshotFilename.Remove(TCMDiagnosticsSnapshotFilename.Length - 3, 3); // remove last 3 characters
                        TCMDiagnosticsSnapshotFilename += "_";
                        if (counter < 10) TCMDiagnosticsSnapshotFilename += "0";
                        TCMDiagnosticsSnapshotFilename += counter.ToString();
                        counter++;
                    }

                    TCMDiagnosticsSnapshotFilename += ".txt";
                    File.AppendAllText(TCMDiagnosticsSnapshotFilename, string.Join(Environment.NewLine, TCM.Diagnostics.Table.ToArray()) + Environment.NewLine);
                    break;
                }
            }
        }

        #endregion

        #region LCD tab

        private void LCDApplySettingsButton_Click(object sender, EventArgs e)
        {
            byte LCDState = (byte)LCDStateComboBox.SelectedIndex;

            bool success = byte.TryParse(Util.HexStringToByte(LCDI2CAddressTextBox.Text)[0].ToString(), out byte LCDI2CAddress);
            if (!success)
            {
                LCDI2CAddress = 0x27;
                LCDI2CAddressTextBox.Text = "27";
            }

            success = byte.TryParse(LCDWidthTextBox.Text, out byte LCDWidth);
            if (!success)
            {
                LCDWidth = 20;
                LCDWidthTextBox.Text = "20";
            }

            success = byte.TryParse(LCDHeightTextBox.Text, out byte LCDHeight);
            if (!success)
            {
                LCDHeight = 4;
                LCDHeightTextBox.Text = "4";
            }

            success = byte.TryParse(LCDRefreshRateTextBox.Text, out byte LCDRefreshRate);
            if (!success)
            {
                LCDRefreshRate = 20;
                LCDRefreshRateTextBox.Text = "20";
            }

            byte LCDUnits;
            if (Properties.Settings.Default.Units == "imperial") LCDUnits = 0;
            else if (Properties.Settings.Default.Units == "metric") LCDUnits = 1;
            else LCDUnits = 0;

            byte LCDDataSource = (byte)(LCDDataSourceComboBox.SelectedIndex + 1);

            Packet packet = new Packet();

            packet.Bus = (byte)PacketHelper.Bus.USB;
            packet.Command = (byte)PacketHelper.Command.Settings;
            packet.Mode = (byte)PacketHelper.SettingsMode.SetLCD;
            packet.Payload = new byte[7] { LCDState, LCDI2CAddress, LCDWidth, LCDHeight, LCDRefreshRate, LCDUnits, LCDDataSource };

            Util.UpdateTextBox(USBTextBox, "[<-TX] Change LCD settings:", PacketHelper.Serialize(packet));

            SerialService.WritePacket(packet);

            UpdateLCDPreviewTextBox();
        }

        private void LCDDataSourceComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (LCDDataSourceComboBox.SelectedIndex == 2)
            {
                MessageBox.Show("Currently the transmission controller cannot be selected as LCD data source.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LCDDataSourceComboBox.SelectedIndex = 1;
            }
        }

        private void UpdateLCDPreviewTextBox()
        {
            switch (LCDStateComboBox.SelectedIndex)
            {
                case 0: // disabled
                {
                    if ((LCDWidthTextBox.Text == "20") && (LCDHeightTextBox.Text == "4"))
                    {
                        LCDPreviewTextBox.Clear();
                        LCDPreviewTextBox.AppendText("--------------------" + Environment.NewLine);
                        LCDPreviewTextBox.AppendText("  CHRYSLER CCD/SCI  " + Environment.NewLine);
                        LCDPreviewTextBox.AppendText("   SCANNER VX.XX    " + Environment.NewLine);
                        LCDPreviewTextBox.AppendText("--------------------");
                    }
                    else if ((LCDWidthTextBox.Text == "16") && (LCDHeightTextBox.Text == "2"))
                    {
                        LCDPreviewTextBox.Clear();
                        LCDPreviewTextBox.AppendText("CHRYSLER CCD/SCI" + Environment.NewLine);
                        LCDPreviewTextBox.AppendText(" SCANNER VX.XX  ");
                    }
                    else
                    {
                        LCDPreviewTextBox.Clear();
                        LCDPreviewTextBox.AppendText("CCD/SCI");
                    }
                    break;
                }
                case 1: // enabled
                {
                    if ((LCDWidthTextBox.Text == "20") && (LCDHeightTextBox.Text == "4"))
                    {
                        if (Properties.Settings.Default.Units == "imperial")
                        {
                            LCDPreviewTextBox.Clear();
                            LCDPreviewTextBox.AppendText("  0mph     0rpm   0%" + Environment.NewLine);
                            LCDPreviewTextBox.AppendText("  0/  0°F     0.0psi" + Environment.NewLine);
                            LCDPreviewTextBox.AppendText(" 0.0/ 0.0V          " + Environment.NewLine);
                            LCDPreviewTextBox.AppendText("     0.000mi        ");
                        }
                        else if (Properties.Settings.Default.Units == "metric")
                        {
                            LCDPreviewTextBox.Clear();
                            LCDPreviewTextBox.AppendText("  0km/h    0rpm   0%" + Environment.NewLine);
                            LCDPreviewTextBox.AppendText("  0/  0°C     0.0kPa" + Environment.NewLine);
                            LCDPreviewTextBox.AppendText(" 0.0/ 0.0V          " + Environment.NewLine);
                            LCDPreviewTextBox.AppendText("     0.000km        ");
                        }
                    }
                    else if ((LCDWidthTextBox.Text == "16") && (LCDHeightTextBox.Text == "2"))
                    {
                        if (Properties.Settings.Default.Units == "imperial")
                        {
                            LCDPreviewTextBox.Clear();
                            LCDPreviewTextBox.AppendText("  0mph      0rpm" + Environment.NewLine);
                            LCDPreviewTextBox.AppendText("  0°F     0.0psi");
                        }
                        else if (Properties.Settings.Default.Units == "metric")
                        {
                            LCDPreviewTextBox.Clear();
                            LCDPreviewTextBox.AppendText("  0km/h     0rpm" + Environment.NewLine);
                            LCDPreviewTextBox.AppendText("  0°C     0.0kPa");
                        }
                    }
                    else
                    {
                        LCDPreviewTextBox.Clear();
                        LCDPreviewTextBox.AppendText("    0rpm");
                    }
                    break;
                }
            }
        }

        #endregion

        #region Menu strip

        private void UpdateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Uri GUIAssemblyInfoFile = new Uri("https://raw.githubusercontent.com/laszlodaniel/ChryslerScanner/master/GUI/ChryslerScanner/Properties/AssemblyInfo.cs");
            Uri GUIZIPDownload = new Uri("https://github.com/laszlodaniel/ChryslerScanner/raw/master/GUI/ChryslerScanner/bin/Debug/ChryslerScanner_GUI.zip");

            // First check if GUI update is available.
            // Download the latest AssemblyInfo.cs file from GitHub and compare version numbers.
            if (!Directory.Exists("Update")) Directory.CreateDirectory("Update"); // create Update folder first if it doesn't exist

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            try
            {
                Downloader.DownloadFile(GUIAssemblyInfoFile, @"Update/AssemblyInfo.cs");
            }
            catch
            {
                MessageBox.Show("GUI update availability cannot be checked.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            if (File.Exists(@"Update/AssemblyInfo.cs"))
            {
                // Get latest version number from the downloaded file.
                string line = string.Empty;
                bool done = false;
                using (Stream stream = File.Open(@"Update/AssemblyInfo.cs", FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        while (!done)
                        {
                            Thread.Sleep(1);
                            
                            line = reader.ReadLine();

                            if (line != null)
                            {
                                if (line.StartsWith("[assembly: AssemblyVersion"))
                                {
                                    done = true;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }

                if (line != null)
                {
                    string latestGUIVersionString = "v" + line.Substring(28, line.IndexOf(")") - 29);

                    if (latestGUIVersionString == GUIVersion)
                    {
                        MessageBox.Show("You are using the latest GUI version.", "No GUI update available", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        if (MessageBox.Show("Latest GUI version: " + latestGUIVersionString + Environment.NewLine +
                                            "Current GUI version: " + GUIVersion + Environment.NewLine +
                                            "There is a new GUI version available." + Environment.NewLine +
                                            "Do you want to download it?", "GUI update available", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                        {
                            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                            try
                            {
                                Downloader.DownloadFile(GUIZIPDownload, @"Update/ChryslerScanner_GUI.zip");
                                MessageBox.Show("Updated GUI download finished." + Environment.NewLine +
                                                "Close this application and unpack the .zip-file from the Update folder!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            catch
                            {
                                MessageBox.Show("GUI download error.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                        else
                        {
                            MessageBox.Show("GUI update cancelled.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("GUI download error." + Environment.NewLine + "Please download the latest GUI .zip file from GitHub and overwrite the old executable file.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                File.Delete(@"Update/AssemblyInfo.cs");
            }

            // Proceed by selecting the correct update tools.
            if (HWVersion.Contains("v1."))
            {
                Uri FWSourceFile = new Uri("https://raw.githubusercontent.com/laszlodaniel/ChryslerScanner/master/Arduino/ChryslerCCDSCIScanner/ChryslerCCDSCIScanner.ino");
                Uri FWFlashFile = new Uri("https://raw.githubusercontent.com/laszlodaniel/ChryslerScanner/master/Arduino/ChryslerCCDSCIScanner/ChryslerCCDSCIScanner.ino.mega.hex");

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                try
                {
                    Downloader.DownloadFile(FWSourceFile, @"Update/ChryslerCCDSCIScanner.ino");
                }
                catch
                {
                    MessageBox.Show("Firmware update availability cannot be checked.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                if (File.Exists(@"Update/ChryslerCCDSCIScanner.ino") && DeviceFound)
                {
                    // Get new version/UNIX time value from the downloaded file
                    string line = string.Empty;
                    bool done = false;
                    using (Stream stream = File.Open(@"Update/ChryslerCCDSCIScanner.ino", FileMode.Open))
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            while (!done)
                            {
                                Thread.Sleep(1);

                                line = reader.ReadLine();

                                if (line != null)
                                {
                                    if (line.Contains("#define FW_VERSION") || line.Contains("#define FW_DATE"))
                                    {
                                        done = true;
                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }

                    if (line != null)
                    {
                        uint ver = 0;

                        if (line.StartsWith("#define FW_DATE"))
                        {
                            string hexline = line.Substring(16, 10);
                            ver = Convert.ToUInt32(hexline, 16);
                        }
                        else if (line.StartsWith("#define FW_VERSION"))
                        {
                            string hexline = line.Substring(19, 10);
                            ver = Convert.ToUInt32(hexline, 16);
                        }

                        byte major = (byte)(ver >> 24);
                        byte minor = (byte)(ver >> 16);
                        byte patch = (byte)(ver >> 8);
                        string latestFWVersionString = "v" + major.ToString("0") + "." + minor.ToString("0") + "." + patch.ToString("0");

                        if (latestFWVersionString == FWVersion)
                        {
                            MessageBox.Show("The diagnostic scanner uses the latest firmware version.", "No firmware update available", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            if (File.Exists(@"Tools/avrdude.exe") && File.Exists(@"Tools/avrdude.conf") && File.Exists(@"Tools/libusb0.dll"))
                            {
                                if (MessageBox.Show("Latest firmware version: " + latestFWVersionString + Environment.NewLine +
                                                    "Current firmware version: " + FWVersion + Environment.NewLine +
                                                    "There is a new device firmware version available." + Environment.NewLine +
                                                    "Do you want to update the device?", "Device firmware update available", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                                {
                                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                                    try
                                    {
                                        Downloader.DownloadFile(FWFlashFile, @"Tools/ChryslerCCDSCIScanner.ino.mega.hex");
                                    }
                                    catch
                                    {
                                        MessageBox.Show("Firmware download error.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    }

                                    if (File.Exists(@"Tools/ChryslerCCDSCIScanner.ino.mega.hex"))
                                    {
                                        SerialService.Disconnect();

                                        this.Refresh();
                                        Process process = new Process();
                                        process.StartInfo.WorkingDirectory = "Tools";
                                        process.StartInfo.FileName = "avrdude.exe";
                                        process.StartInfo.Arguments = "-C avrdude.conf -p m2560 -c wiring -P " + SelectedPort + " -b 115200 -D -U flash:w:ChryslerCCDSCIScanner.ino.mega.hex:i";
                                        process.Start();
                                        process.WaitForExit();
                                        this.Refresh();

                                        File.Delete(@"Tools/ChryslerCCDSCIScanner.ino.mega.hex");
                                        FWVersion = latestFWVersionString;

                                        if (!SerialService.Connect(SelectedPort))
                                        {
                                            Util.UpdateTextBox(USBTextBox, "[INFO] Device not found on " + SelectedPort + ".");
                                        }

                                        ResetButton_Click(this, EventArgs.Empty);
                                        ResetFromUpdate = true;
                                    }
                                    else
                                    {
                                        MessageBox.Show("Flash file is missing.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("Device firmware update cancelled.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                            }
                            else
                            {
                                MessageBox.Show("V1 update tools are missing.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Firmware download error." + Environment.NewLine + "Please download the latest .hex flash file from GitHub and perform a manual update." + Environment.NewLine + "For more information see \"Tools\\readme.txt\".", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                    File.Delete(@"Update/ChryslerCCDSCIScanner.ino");
                }
                else if (!DeviceFound)
                {
                    File.Delete(@"Update/ChryslerCCDSCIScanner.ino");
                    MessageBox.Show("Device firmware update cannot be checked." + Environment.NewLine +
                                    "Connect to the device and try again!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else if (HWVersion.Contains("v2."))
            {
                Uri FWSourceFile = new Uri("https://raw.githubusercontent.com/laszlodaniel/ChryslerScanner/master/PlatformIO/ChryslerScanner/CMakeLists.txt");
                Uri FWFlashFile = new Uri("https://raw.githubusercontent.com/laszlodaniel/ChryslerScanner/master/PlatformIO/ChryslerScanner/src/ChryslerScanner.bin");

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                try
                {
                    Downloader.DownloadFile(FWSourceFile, @"Update/CMakeLists.txt");
                }
                catch
                {
                    MessageBox.Show("Firmware update availability cannot be checked.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                if (File.Exists(@"Update/CMakeLists.txt") && DeviceFound)
                {
                    string line = string.Empty;
                    bool done = false;
                    using (Stream stream = File.Open(@"Update/CMakeLists.txt", FileMode.Open))
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            while (!done)
                            {
                                Thread.Sleep(1);

                                line = reader.ReadLine();

                                if (line != null)
                                {
                                    if (line.Contains("set(PROJECT_VER"))
                                    {
                                        done = true;
                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }

                    if (line != null)
                    {
                        string latestFWVersionString = "v" + line.Substring(17, 5);

                        if (latestFWVersionString == FWVersion)
                        {
                            MessageBox.Show("The diagnostic scanner uses the latest firmware version.", "No firmware update available", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            if (File.Exists(@"Tools/esptool.exe") && File.Exists(@"Tools/bootloader.bin") && File.Exists(@"Tools/partition_custom_table.bin") && File.Exists(@"Tools/ota_data_initial.bin"))
                            {
                                if (MessageBox.Show("Latest firmware version: " + latestFWVersionString + Environment.NewLine +
                                                    "Current firmware version: " + FWVersion + Environment.NewLine +
                                                    "There is a new device firmware version available." + Environment.NewLine +
                                                    "Do you want to update the device?", "Device firmware update available", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                                {
                                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                                    try
                                    {
                                        Downloader.DownloadFile(FWFlashFile, @"Tools/ChryslerScanner.bin");
                                    }
                                    catch
                                    {
                                        MessageBox.Show("Firmware download error.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    }

                                    if (File.Exists(@"Tools/ChryslerScanner.bin"))
                                    {
                                        SerialService.Disconnect();

                                        this.Refresh();
                                        Process process = new Process();
                                        process.StartInfo.WorkingDirectory = "Tools";
                                        process.StartInfo.FileName = "esptool.exe";
                                        process.StartInfo.Arguments = "--chip esp32 --port " + SelectedPort + " --baud 921600 --before default_reset --after hard_reset write_flash -z --flash_mode dio --flash_freq 40m --flash_size detect 0x1000 bootloader.bin 0x8000 partition_custom_table.bin 0xe000 ota_data_initial.bin 0x10000 ChryslerScanner.bin";
                                        process.Start();
                                        process.WaitForExit();
                                        this.Refresh();

                                        File.Delete(@"Tools/ChryslerScanner.bin");
                                        FWVersion = latestFWVersionString;

                                        if (!SerialService.Connect(SelectedPort))
                                        {
                                            Util.UpdateTextBox(USBTextBox, "[INFO] Device not found on " + SelectedPort + ".");
                                        }

                                        ResetButton_Click(this, EventArgs.Empty);
                                        ResetFromUpdate = true;
                                    }
                                    else
                                    {
                                        MessageBox.Show("Flash file is missing.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("Device firmware update cancelled.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                            }
                            else
                            {
                                MessageBox.Show("V2 update tools are missing.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Firmware download error." + Environment.NewLine + "Please download the latest .bin flash file from GitHub and perform a manual update." + Environment.NewLine + "For more information see \"Tools\\readme.txt\".", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                    File.Delete(@"Update/CMakeLists.txt");
                }
                else if (!DeviceFound)
                {
                    File.Delete(@"Update/CMakeLists.txt");
                    MessageBox.Show("Device firmware update cannot be checked." + Environment.NewLine +
                                    "Connect to the device and try again!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                MessageBox.Show("Firmware update availability cannot be checked.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void Downloader_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            // TODO
            return;
        }

        private void Downloader_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            // TODO
            return;
        }

        private void ReadMemoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ReadMemory == null)
            {
                ReadMemory = new ReadMemoryForm(this, ContainerManager.Instance.GetInstance<SerialService>())
                {
                    StartPosition = FormStartPosition.CenterParent
                };

                ReadMemory.FormClosed += delegate { ReadMemory = null; };
                ReadMemory.Show(this);

                if (ReadMemory.StartPosition == FormStartPosition.CenterParent)
                {
                    var x = Location.X + (Width - ReadMemory.Width) / 2;
                    var y = Location.Y + (Height - ReadMemory.Height) / 2;
                    ReadMemory.Location = new Point(Math.Max(x, 0), Math.Max(y, 0));
                }
            }
            else
            {
                ReadMemory.WindowState = FormWindowState.Normal;
                ReadMemory.Focus();
            }
        }

        private void ReadWriteMemoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ReadWriteMemory == null)
            {
                ReadWriteMemory = new ReadWriteMemoryForm(this, ContainerManager.Instance.GetInstance<SerialService>())
                {
                    StartPosition = FormStartPosition.CenterParent
                };

                ReadWriteMemory.FormClosed += delegate { ReadWriteMemory = null; };
                ReadWriteMemory.Show(this);

                if (ReadWriteMemory.StartPosition == FormStartPosition.CenterParent)
                {
                    var x = Location.X + (Width - ReadWriteMemory.Width) / 2;
                    var y = Location.Y + (Height - ReadWriteMemory.Height) / 2;
                    ReadWriteMemory.Location = new Point(Math.Max(x, 0), Math.Max(y, 0));
                }
            }
            else
            {
                ReadWriteMemory.WindowState = FormWindowState.Normal;
                ReadWriteMemory.Focus();
            }
        }

        private void BootstrapToolsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (BootstrapTools == null)
            {
                ScannerTabControl.SelectedTab = SCIBusControlTabPage;

                if (SCIBusModuleComboBox.SelectedIndex == 0) DiagnosticsTabControl.SelectedTab = SCIBusPCMDiagnosticsTabPage;
                else if (SCIBusModuleComboBox.SelectedIndex == 1) DiagnosticsTabControl.SelectedTab = SCIBusTCMDiagnosticsTabPage;

                BootstrapTools = new BootstrapToolsForm(this, ContainerManager.Instance.GetInstance<SerialService>())
                {
                    StartPosition = FormStartPosition.CenterParent
                };

                BootstrapTools.FormClosed += delegate { BootstrapTools = null; };
                BootstrapTools.Show(this);

                if (BootstrapTools.StartPosition == FormStartPosition.CenterParent)
                {
                    var x = Location.X + (Width - BootstrapTools.Width) / 2;
                    var y = Location.Y + (Height - BootstrapTools.Height) / 2;
                    BootstrapTools.Location = new Point(Math.Max(x, 0), Math.Max(y, 0));
                }
            }
            else
            {
                BootstrapTools.WindowState = FormWindowState.Normal;
                BootstrapTools.Focus();
            }
        }

        private void EngineToolsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (EngineTools == null)
            {
                ScannerTabControl.SelectedTab = SCIBusControlTabPage;
                DiagnosticsTabControl.SelectedTab = SCIBusPCMDiagnosticsTabPage;

                EngineTools = new EngineToolsForm(this, ContainerManager.Instance.GetInstance<SerialService>())
                {
                    StartPosition = FormStartPosition.CenterParent
                };

                EngineTools.FormClosed += delegate { EngineTools = null; };
                EngineTools.Show(this);

                if (EngineTools.StartPosition == FormStartPosition.CenterParent)
                {
                    var x = Location.X + (Width - EngineTools.Width) / 2;
                    var y = Location.Y + (Height - EngineTools.Height) / 2;
                    EngineTools.Location = new Point(Math.Max(x, 0), Math.Max(y, 0));
                }
            }
            else
            {
                EngineTools.WindowState = FormWindowState.Normal;
                EngineTools.Focus();
            }
        }

        private void ABSToolsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ABSTools == null)
            {
                ScannerTabControl.SelectedTab = CCDBusControlTabPage;
                DiagnosticsTabControl.SelectedTab = CCDBusDiagnosticsTabPage;

                ABSTools = new ABSToolsForm(this, ContainerManager.Instance.GetInstance<SerialService>())
                {
                    StartPosition = FormStartPosition.CenterParent
                };

                ABSTools.FormClosed += delegate { ABSTools = null; };
                ABSTools.Show(this);

                if (ABSTools.StartPosition == FormStartPosition.CenterParent)
                {
                    var x = Location.X + (Width - ABSTools.Width) / 2;
                    var y = Location.Y + (Height - ABSTools.Height) / 2;
                    ABSTools.Location = new Point(Math.Max(x, 0), Math.Max(y, 0));
                }
            }
            else
            {
                ABSTools.WindowState = FormWindowState.Normal;
                ABSTools.Focus();
            }
        }

        private void MetricUnitsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImperialUnitsToolStripMenuItem.Checked = false;
            MetricUnitsToolStripMenuItem.Checked = true;

            Properties.Settings.Default.Units = "metric";
            Properties.Settings.Default.Save(); // save setting in application configuration file

            ReadWriteMemory?.UpdateMileageUnit();
        }

        private void ImperialUnitsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImperialUnitsToolStripMenuItem.Checked = true;
            MetricUnitsToolStripMenuItem.Checked = false;

            Properties.Settings.Default.Units = "imperial";
            Properties.Settings.Default.Save(); // save setting in application configuration file

            ReadWriteMemory?.UpdateMileageUnit();
        }

        private void EnglishLangToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EnglishLangToolStripMenuItem.Checked = true;
            SpanishLangToolStripMenuItem.Checked = false;

            Properties.Settings.Default.Language = "English";
            Properties.Settings.Default.Save(); // save setting in application configuration file

            ChangeLanguage();
        }

        private void SpanishLangToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EnglishLangToolStripMenuItem.Checked = false;
            SpanishLangToolStripMenuItem.Checked = true;

            Properties.Settings.Default.Language = "Spanish";
            Properties.Settings.Default.Save(); // save setting in application configuration file

            ChangeLanguage();
        }

        public void ChangeLanguage()
        {
            if (Properties.Settings.Default.Language == "English")
            {
                CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("en");
            }
            else if (Properties.Settings.Default.Language == "Spanish")
            {
                CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("es");
            }

            // To add more translations create/edit "Languages/strings.xx.resx" file.
            // Do NOT edit lines below.

            ToolsToolStripMenuItem.Text = Languages.strings.Tools;
            SettingsToolStripMenuItem.Text = Languages.strings.Settings;
            AboutToolStripMenuItem.Text = Languages.strings.About;
            UpdateToolStripMenuItem.Text = Languages.strings.Update;
            ReadMemoryToolStripMenuItem.Text = Languages.strings.ReadMemory;
            ReadWriteMemoryToolStripMenuItem.Text = Languages.strings.ReadWriteMemory;
            BootstrapToolsToolStripMenuItem.Text = Languages.strings.BootstrapTools;
            EngineToolsToolStripMenuItem.Text = Languages.strings.EngineTools;
            ABSToolsToolStripMenuItem.Text = Languages.strings.ABSTools;
            UnitToolStripMenuItem.Text = Languages.strings.Unit;
            LanguageToolStripMenuItem.Text = Languages.strings.Language;
            IncludeTimestampInLogFilesToolStripMenuItem.Text = Languages.strings.IncludeTimestampInLogFiles;
            CCDBusOnDemandToolStripMenuItem.Text = Languages.strings.CCDBusOnDemand;
            PCIBusOnDemandToolStripMenuItem.Text = Languages.strings.PCIBusOnDemand;
            SortMessagesByIDByteToolStripMenuItem.Text = Languages.strings.SortMessagesByIDByte;
            MetricUnitsToolStripMenuItem.Text = Languages.strings.Metric;
            ImperialUnitsToolStripMenuItem.Text = Languages.strings.Imperial;
            EnglishLangToolStripMenuItem.Text = Languages.strings.English;
            SpanishLangToolStripMenuItem.Text = Languages.strings.Spanish;

            USBCommunicationGroupBox.Text = Languages.strings.USBCommunication;
            ControlPanelGroupBox.Text = Languages.strings.ControlPanel;
            DiagnosticsGroupBox.Text = Languages.strings.Diagnostics;

            USBSendPacketButton.Text = Languages.strings.SendPacket;

            if (DeviceFound)
            {
                ConnectButton.Text = Languages.strings.Disconnect;
            }
            else
            {
                ConnectButton.Text = Languages.strings.Connect;
            }

            COMPortsRefreshButton.Text = Languages.strings.Refresh;
            DemoButton.Text = Languages.strings.Demo;

            if (Size == new Size(405, 650))
            {
                ExpandButton.Text = Languages.strings.Expand;
            }
            else if (Size == new Size(1300, 650))
            {
                ExpandButton.Text = Languages.strings.Collapse;
            }

            MainLabel.Text = Languages.strings.Main;
            ResetButton.Text = Languages.strings.Reset;
            HandshakeButton.Text = Languages.strings.Handshake;
            StatusButton.Text = Languages.strings.Status;

            RequestLabel.Text = Languages.strings.Request;
            VersionInfoButton.Text = Languages.strings.VersionInfo;
            TimestampButton.Text = Languages.strings.Timestamp;
            BatteryVoltageButton.Text = Languages.strings.BatteryVoltage;

            SettingsLabel.Text = Languages.strings.SettingsLabel;
            SetLEDsButton.Text = Languages.strings.SetLEDs;
            HeartbeatIntervalLabel.Text = Languages.strings.HeartbeatInterval;
            LEDBlinkDurationLabel.Text = Languages.strings.BlinkDuration;

            SCIBusPCMDiagnosticsTabPage.Text = Languages.strings.SCIBusEngine;
            SCIBusTCMDiagnosticsTabPage.Text = Languages.strings.SCIBusTransmission;

            DiagnosticsRefreshButton.Text = Languages.strings.Refresh;
            DiagnosticsResetViewButton.Text = Languages.strings.ResetView;
            DiagnosticsCopyToClipboardButton.Text = Languages.strings.CopyTableToClipboard;
            DiagnosticsSnapshotButton.Text = Languages.strings.Snapshot;

            if (About != null)
            {
                About.Text = Languages.strings.About;
                // TODO
            }

            if (ReadMemory != null)
            {
                ReadMemory.Text = Languages.strings.ReadMemory;
                // TODO
            }

            if (ReadWriteMemory != null)
            {
                ReadWriteMemory.Text = Languages.strings.ReadWriteMemory;
                // TODO
            }

            if (BootstrapTools != null)
            {
                BootstrapTools.Text = Languages.strings.BootstrapTools;
                // TODO
            }

            if (EngineTools != null)
            {
                EngineTools.Text = Languages.strings.EngineTools;
                // TODO
            }

            if (ABSTools != null)
            {
                ABSTools.Text = Languages.strings.ABSTools;
                // TODO
            }
        }

        private void UpdateUARTBaudrate(int baudrate)
        {
            if (!DeviceFound)
                return;
            
            byte[] BaudratePayload = new byte[4];

            BaudratePayload[0] = (byte)((baudrate >> 24) & 0xFF);
            BaudratePayload[1] = (byte)((baudrate >> 16) & 0xFF);
            BaudratePayload[2] = (byte)((baudrate >> 8) & 0xFF);
            BaudratePayload[3] = (byte)(baudrate & 0xFF);

            Packet packet = new Packet();

            packet.Bus = (byte)PacketHelper.Bus.USB;
            packet.Command = (byte)PacketHelper.Command.Settings;
            packet.Mode = (byte)PacketHelper.SettingsMode.SetUARTBaudrate;
            packet.Payload = BaudratePayload;

            Util.UpdateTextBox(USBTextBox, "[<-TX] Set UART baudrate:", PacketHelper.Serialize(packet));
            Util.UpdateTextBox(USBTextBox, "[INFO] GUI baudrate = " + baudrate);

            SerialService.WritePacket(packet);
        }

        private void Baudrate250000ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Baudrate250000ToolStripMenuItem.Checked = true;
            Baudrate115200ToolStripMenuItem.Checked = false;
            
            Properties.Settings.Default.UARTBaudrate = 250000;
            Properties.Settings.Default.Save(); // save setting in application configuration file

            UpdateUARTBaudrate(Properties.Settings.Default.UARTBaudrate);

            MessageBox.Show("Restart GUI!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Application.Exit();
        }

        private void Baudrate115200ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Baudrate250000ToolStripMenuItem.Checked = false;
            Baudrate115200ToolStripMenuItem.Checked = true;

            Properties.Settings.Default.UARTBaudrate = 115200;
            Properties.Settings.Default.Save(); // save setting in application configuration file

            UpdateUARTBaudrate(Properties.Settings.Default.UARTBaudrate);

            MessageBox.Show("Restart GUI!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Application.Exit();
        }

        private void IncludeTimestampInLogFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (IncludeTimestampInLogFilesToolStripMenuItem.Checked)
            {
                Properties.Settings.Default.Timestamp = true;
            }
            else
            {
                Properties.Settings.Default.Timestamp = false;
            }

            Properties.Settings.Default.Save(); // save setting in application configuration file
        }

        private void CCDBusOnDemandToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CCDBusOnDemandToolStripMenuItem.Checked)
            {
                Properties.Settings.Default.CCDBusOnDemand = true;
            }
            else
            {
                Properties.Settings.Default.CCDBusOnDemand = false;
            }

            Properties.Settings.Default.Save(); // save setting in application configuration file
        }

        private void PCIBusOnDemandToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (PCIBusOnDemandToolStripMenuItem.Checked)
            {
                Properties.Settings.Default.PCIBusOnDemand = true;
            }
            else
            {
                Properties.Settings.Default.PCIBusOnDemand = false;
            }

            Properties.Settings.Default.Save(); // save setting in application configuration file
        }

        private void SortMessagesByIDByteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SortMessagesByIDByteToolStripMenuItem.Checked)
            {
                Properties.Settings.Default.SortByID = true;
            }
            else
            {
                Properties.Settings.Default.SortByID = false;
            }

            Properties.Settings.Default.Save(); // save setting in application configuration file
        }

        private void DisplayRawBusPacketsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DisplayRawBusPacketsToolStripMenuItem.Checked)
            {
                Properties.Settings.Default.DisplayRawBusPackets = true;
            }
            else
            {
                Properties.Settings.Default.DisplayRawBusPackets = false;
            }

            Properties.Settings.Default.Save(); // save setting in application configuration file
        }

        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            About = new AboutForm(this)
            {
                StartPosition = FormStartPosition.CenterParent
            };

            About.FormClosed += delegate { About = null; };
            About.ShowDialog(this);

            ChangeLanguage();
        }

        #endregion

        private void MainForm_Load(object sender, EventArgs e)
        {
            Util.UpdateTextBox(USBTextBox, "[INFO] GUI started (" + GUIVersion + ")");
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (!DeviceFound)
                return;

            if (e.Control && e.KeyCode == Keys.B) // Ctrl+B shortcut
            {
                BootstrapToolsToolStripMenuItem_Click(this, EventArgs.Empty);
            }

            if (e.Control && e.KeyCode == Keys.E) // Ctrl+E shortcut
            {
                EngineToolsToolStripMenuItem_Click(this, EventArgs.Empty);
            }

            if (e.Control && e.KeyCode == Keys.W) // Ctrl+A shortcut
            {
                ABSToolsToolStripMenuItem_Click(this, EventArgs.Empty);
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // TODO
        }
    }
}
