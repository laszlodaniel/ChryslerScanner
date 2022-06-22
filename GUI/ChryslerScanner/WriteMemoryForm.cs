using System;
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
    public partial class WriteMemoryForm : Form
    {
        private MainForm OriginalForm;

        private const ushort PCMUnlockKey = 0x9018;
        private bool PCMUnlocked = false;

        private const ushort EEPROMSize = 512; // bytes
        private const ushort RAMSize = 6144; // bytes

        private const ushort SRIMileageOffsetStart = 0x0000;
        private const ushort SRIMileageOffsetEnd = 0x0007;
        private const ushort SKIMVTSSOffset = 0x0008;
        private const ushort VINOffsetStart = 0x0062;
        private const ushort VINOffsetEnd = 0x0072;
        private const ushort VINLength = 17;
        private const ushort PartNumberOffset1Start = 0x01E2;
        private const ushort PartNumberOffset1End = 0x01EA;
        private const ushort PartNumberOffset2Start = 0x01F0;
        private const ushort PartNumberOffset2End = 0x01F3;

        private const ushort CCDBCMMileageMsgLength = 6;
        private byte[] CCDBCMMileage = new byte[CCDBCMMileageMsgLength];
        private double CCDBCMMileageMi = 0;
        private double CCDBCMMileageKm = 0;
        private bool CCDBCMMileageReceived = false;
        private const ushort SRIMileageLength = 2;
        private byte[] SRIMileage = new byte[SRIMileageLength];
        private uint SRIMileageRaw = 0;
        private double SRIMileageMi = 0;
        private double SRIMileageKm = 0;
        private byte[] SRIMileageNew = new byte[4 * SRIMileageLength];
        private uint SRIMileageNewRaw = 0;
        private double SRIMileageNewMi = 0;
        private double SRIMileageNewKm = 0;
        private byte SKIMVTSS = 0;
        private byte[] VIN = new byte[VINLength];
        private string VINString;
        private byte[] PartNumberBuffer = new byte[18];
        private byte[] PartNumber = new byte[6];
        private ushort PartNumberLocation = 0;

        private byte[] EEPROMBuffer;
        private byte[] RAMBuffer;

        private string SCIBusPCMWriteMemoryLogFilename;
        private string SCIBusPCMMemoryEEPROMBackupFilename;
        private string SCIBusPCMMemoryRAMBackupFilename;

        private System.Timers.Timer SCIBusPCMNextRequestTimer = new System.Timers.Timer();
        private System.Timers.Timer SCIBusPCMRxTimeoutTimer = new System.Timers.Timer();
        private System.Timers.Timer SCIBusPCMTxTimeoutTimer = new System.Timers.Timer();
        private BackgroundWorker SCIBusPCMReadMemoryWorker = new BackgroundWorker();
        private BackgroundWorker SCIBusPCMWriteMemoryWorker = new BackgroundWorker();

        private Task CurrentTask = Task.None;
        private bool SCIBusPCMResponse = false;
        private bool SCIBusPCMNextRequest = false;
        private byte SCIBusPCMRxRetryCount = 0;
        private byte SCIBusPCMTxRetryCount = 0;
        private bool SCIBusPCMReadMemoryFinished = false;
        private bool SCIBusPCMWriteMemoryFinished = false;
        private bool SCIBusPCMRxTimeout = false;
        private bool SCIBusPCMTxTimeout = false;
        private byte[] SCIBusPCMTxPayload = new byte[4];

        private byte[] SCIBusPCMMemoryOffsetStartBytes = new byte[2];
        private uint SCIBusPCMMemoryOffsetStart = 0;
        private byte[] SCIBusPCMMemoryOffsetEndBytes = new byte[2];
        private uint SCIBusPCMMemoryOffsetEnd = 0;
        private byte[] SCIBusPCMCurrentMemoryOffsetBytes = new byte[2];
        private uint SCIBusPCMCurrentMemoryOffset = 0;
        private byte SCIBusPCMMemoryValue = 0;

        private TimeSpan ElapsedMillis;
        private DateTime Timestamp;
        private string TimestampString;

        private enum Task
        {
            None,
            ReadSRIMileage,
            ReadSKIMVTSS,
            ReadVIN,
            ReadPartNumber,
            ReadEEPROM,
            ReadRAM,
            BackupEEPROM,
            BackupRAM,
            WriteSRIMileage,
            WriteSKIMVTSS,
            WriteVIN,
            WritePartNumber,
            WriteEEPROM,
            WriteRAM,
            RestoreEEPROM
        }

        private enum CCD_ID
        {
            BCMMileage = 0xCE
        }

        private enum SCI_ID
        {
            ReadROMRAM = 0x26,
            WriteEEPROM = 0x27,
            ReadEEPROM = 0x28,
            WriteRAM = 0x29,
            PCMInfo = 0x2A,
            GetSecuritySeed = 0x2B,
            SendSecuritySeed = 0x2C
        }

        public WriteMemoryForm(MainForm IncomingForm)
        {
            OriginalForm = IncomingForm;
            InitializeComponent();
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            MainForm.Packet.PacketReceived += PacketReceivedHandler; // subscribe to the OnPacketReceived event
            WriteMemoryTabControl.SelectedTab = SCIBusPCMTabPage;
            UpdateMileageUnit();

            SCIBusPCMWriteMemoryLogFilename = @"LOG/PCM/pcmlog_write_memory_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt";

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

            SCIBusPCMWriteMemoryWorker.WorkerReportsProgress = true;
            SCIBusPCMWriteMemoryWorker.WorkerSupportsCancellation = true;
            SCIBusPCMWriteMemoryWorker.DoWork += new DoWorkEventHandler(SCIBusPCMWriteMemory_DoWork);
            SCIBusPCMWriteMemoryWorker.ProgressChanged += new ProgressChangedEventHandler(SCIBusPCMWriteMemory_ProgressChanged);
            SCIBusPCMWriteMemoryWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(SCIBusPCMWriteMemory_RunWorkerCompleted);

            UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, "Before modifying memory content, make sure to backup first with the \"BAK\" button.");
            UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Cycle ignition key for changes in EEPROM to take effect. RAM changes are immediate.");

            ActiveControl = SCIBusPCMWriteMemorySRIMileageReadButton; // put focus on this button
        }

        #region CCD-bus

        // TODO

        #endregion

        #region SCI-bus (PCM)

        private void SCIBusPCMNextRequestHandler(object source, ElapsedEventArgs e) => SCIBusPCMNextRequest = true;

        private void SCIBusPCMRxTimeoutHandler(object source, ElapsedEventArgs e) => SCIBusPCMRxTimeout = true;

        private void SCIBusPCMTxTimeoutHandler(object source, ElapsedEventArgs e) => SCIBusPCMTxTimeout = true;

        private void SCIBusPCMWriteMemorySRIMileageTextBox_TextChanged(object sender, EventArgs e)
        {
            if (SCIBusPCMWriteMemorySRIMileageTextBox.Text.Length > 0)
            {
                if (SCIBusPCMWriteMemorySRIMileageTextBox.Text.Length > 8)
                {
                    string NewText = Util.TruncateString(SCIBusPCMWriteMemorySRIMileageTextBox.Text, 8);
                    SCIBusPCMWriteMemorySRIMileageTextBox.Text = NewText;
                    SCIBusPCMWriteMemorySRIMileageTextBox.SelectionStart = SCIBusPCMWriteMemorySRIMileageTextBox.Text.Length;
                    SCIBusPCMWriteMemorySRIMileageTextBox.ScrollToCaret();
                }

                if (!SCIBusPCMWriteMemorySRIMileageTextBox.Text.IsNumeric())
                {
                    SCIBusPCMWriteMemorySRIMileageTextBox.Clear();
                }
                else
                {
                    SCIBusPCMWriteMemorySRIMileageWriteButton.Enabled = true;
                }
            }
            else
            {
                SCIBusPCMWriteMemorySRIMileageWriteButton.Enabled = false;
            }
        }

        private void SCIBusPCMWriteMemorySRIMileageReadButton_Click(object sender, EventArgs e)
        {
            if (!SCIBusPCMReadMemoryWorker.IsBusy && !SCIBusPCMWriteMemoryWorker.IsBusy)
            {
                UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Read SRI Mileage.");

                SCIBusPCMMemoryOffsetStart = (uint)SRIMileageOffsetStart;
                SCIBusPCMMemoryOffsetStartBytes[0] = (byte)(SRIMileageOffsetStart >> 8);
                SCIBusPCMMemoryOffsetStartBytes[1] = (byte)SRIMileageOffsetStart;

                SCIBusPCMMemoryOffsetEnd = SCIBusPCMMemoryOffsetStart + 1; // read first instance only
                SCIBusPCMMemoryOffsetEndBytes[0] = (byte)(SRIMileageOffsetEnd >> 8);
                SCIBusPCMMemoryOffsetEndBytes[1] = (byte)SRIMileageOffsetEnd;

                SCIBusPCMCurrentMemoryOffset = SCIBusPCMMemoryOffsetStart;
                SCIBusPCMCurrentMemoryOffsetBytes[0] = SCIBusPCMMemoryOffsetStartBytes[0];
                SCIBusPCMCurrentMemoryOffsetBytes[1] = SCIBusPCMMemoryOffsetStartBytes[1];

                SCIBusPCMTxPayload = new byte[3] { (byte)SCI_ID.ReadEEPROM, SCIBusPCMMemoryOffsetStartBytes[0], SCIBusPCMMemoryOffsetStartBytes[1] };

                SCIBusPCMReadMemoryFinished = false;
                SCIBusPCMNextRequest = true;
                CurrentTask = Task.ReadSRIMileage;
                SCIBusPCMReadMemoryWorker.RunWorkerAsync();
                SCIBusPCMWriteMemorySRIMileageReadButton.Enabled = false;
            }
        }

        private void SCIBusPCMWriteMemorySRIMileageWriteButton_Click(object sender, EventArgs e)
        {
            if (!SCIBusPCMReadMemoryWorker.IsBusy && !SCIBusPCMWriteMemoryWorker.IsBusy)
            {
                if (Properties.Settings.Default.Units == "imperial")
                {
                    double.TryParse(SCIBusPCMWriteMemorySRIMileageTextBox.Text, out SRIMileageNewMi);
                    SRIMileageNewKm = SRIMileageNewMi * 1.609344D;
                }
                else if (Properties.Settings.Default.Units == "metric")
                {
                    double.TryParse(SCIBusPCMWriteMemorySRIMileageTextBox.Text, out SRIMileageNewKm);
                    SRIMileageNewMi = SRIMileageNewKm / 1.609344D;
                }

                SRIMileageNewRaw = (uint)(Math.Round(SRIMileageNewMi / 8.192D)); // first instance is the real mileage
                SRIMileageNew[0] = (byte)(SRIMileageNewRaw >> 8);
                SRIMileageNew[1] = (byte)SRIMileageNewRaw;

                SRIMileageNewRaw++; // second instance is incremented by 1 mile
                SRIMileageNew[2] = (byte)(SRIMileageNewRaw >> 8);
                SRIMileageNew[3] = (byte)SRIMileageNewRaw;
                SRIMileageNewRaw--;

                SRIMileageNew[4] = 0xFF; // 3rd and 4th instance can be FF'd out
                SRIMileageNew[5] = 0xFF;
                SRIMileageNew[6] = 0xFF;
                SRIMileageNew[7] = 0xFF;

                SRIMileageMi = SRIMileageNewRaw * 8.192D;
                SRIMileageKm = SRIMileageMi * 1.609344D;

                SCIBusPCMMemoryOffsetStart = SRIMileageOffsetStart;
                SCIBusPCMMemoryOffsetStartBytes[0] = (byte)(SCIBusPCMMemoryOffsetStart >> 8);
                SCIBusPCMMemoryOffsetStartBytes[1] = (byte)SCIBusPCMMemoryOffsetStart;

                SCIBusPCMMemoryOffsetEnd = SRIMileageOffsetEnd;
                SCIBusPCMMemoryOffsetEndBytes[0] = (byte)(SCIBusPCMMemoryOffsetEnd >> 8);
                SCIBusPCMMemoryOffsetEndBytes[1] = (byte)SCIBusPCMMemoryOffsetEnd;

                SCIBusPCMCurrentMemoryOffset = SCIBusPCMMemoryOffsetStart;
                SCIBusPCMCurrentMemoryOffsetBytes[0] = SCIBusPCMMemoryOffsetStartBytes[0];
                SCIBusPCMCurrentMemoryOffsetBytes[1] = SCIBusPCMMemoryOffsetStartBytes[1];

                SCIBusPCMTxPayload = new byte[4] { (byte)SCI_ID.WriteEEPROM, SCIBusPCMMemoryOffsetStartBytes[0], SCIBusPCMMemoryOffsetStartBytes[1], SRIMileageNew[0] };

                if (Properties.Settings.Default.Units == "imperial")
                {
                    SCIBusPCMWriteMemorySRIMileageTextBox.Text = Math.Round(SRIMileageMi, 1).ToString("0.0").Replace(",", ".");
                    SCIBusPCMWriteMemorySRIMileageTextBox.SelectionStart = SCIBusPCMWriteMemorySRIMileageTextBox.Text.Length;
                    SCIBusPCMWriteMemorySRIMileageTextBox.ScrollToCaret();

                }
                else if (Properties.Settings.Default.Units == "metric")
                {
                    SCIBusPCMWriteMemorySRIMileageTextBox.Text = Math.Round(SRIMileageKm, 1).ToString("0.0").Replace(",", ".");
                    SCIBusPCMWriteMemorySRIMileageTextBox.SelectionStart = SCIBusPCMWriteMemorySRIMileageTextBox.Text.Length;
                    SCIBusPCMWriteMemorySRIMileageTextBox.ScrollToCaret();
                }

                SCIBusPCMWriteMemoryFinished = false;
                SCIBusPCMNextRequest = true;
                CurrentTask = Task.WriteSRIMileage;
                SCIBusPCMWriteMemoryWorker.RunWorkerAsync();
                SCIBusPCMWriteMemorySRIMileageWriteButton.Enabled = false;
            }
            
        }

        private void SCIBusPCMWriteMemorySRIMileageTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                e.Handled = true;

                if (SCIBusPCMWriteMemorySRIMileageWriteButton.Enabled) SCIBusPCMWriteMemorySRIMileageWriteButton_Click(this, EventArgs.Empty);
            }
        }

        private void SCIBusPCMWriteMemorySKIMVTSSComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SCIBusPCMWriteMemorySKIMVTSSComboBox.SelectedIndex < 2)
            {
                SCIBusPCMWriteMemorySKIMVTSSWriteButton.Enabled = true;
            }
            else
            {
                SCIBusPCMWriteMemorySKIMVTSSWriteButton.Enabled = false;
            }
        }

        private void SCIBusPCMWriteMemorySKIMVTSSReadButton_Click(object sender, EventArgs e)
        {
            if (!SCIBusPCMReadMemoryWorker.IsBusy && !SCIBusPCMWriteMemoryWorker.IsBusy)
            {
                UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Read SKIM/VTSS Status.");

                SCIBusPCMMemoryOffsetStart = (uint)SKIMVTSSOffset;
                SCIBusPCMMemoryOffsetStartBytes[0] = (byte)(SKIMVTSSOffset >> 8);
                SCIBusPCMMemoryOffsetStartBytes[1] = (byte)SKIMVTSSOffset;

                SCIBusPCMMemoryOffsetEnd = (uint)(SKIMVTSSOffset);
                SCIBusPCMMemoryOffsetEndBytes[0] = (byte)(SKIMVTSSOffset >> 8);
                SCIBusPCMMemoryOffsetEndBytes[1] = (byte)SKIMVTSSOffset;

                SCIBusPCMCurrentMemoryOffset = SCIBusPCMMemoryOffsetStart;
                SCIBusPCMCurrentMemoryOffsetBytes[0] = SCIBusPCMMemoryOffsetStartBytes[0];
                SCIBusPCMCurrentMemoryOffsetBytes[1] = SCIBusPCMMemoryOffsetStartBytes[1];

                SCIBusPCMTxPayload = new byte[3] { (byte)SCI_ID.ReadEEPROM, SCIBusPCMMemoryOffsetStartBytes[0], SCIBusPCMMemoryOffsetStartBytes[1] };

                SCIBusPCMReadMemoryFinished = false;
                SCIBusPCMNextRequest = true;
                CurrentTask = Task.ReadSKIMVTSS;
                SCIBusPCMReadMemoryWorker.RunWorkerAsync();
                SCIBusPCMWriteMemorySKIMVTSSReadButton.Enabled = false;
            }
        }

        private void SCIBusPCMWriteMemorySKIMVTSSWriteButton_Click(object sender, EventArgs e)
        {
            if (!SCIBusPCMReadMemoryWorker.IsBusy && !SCIBusPCMWriteMemoryWorker.IsBusy)
            {
                UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "SKIM/VTSS write is not yet supported. Please write desired byte at 00 08 EEPROM offset manually.");
            }
        }

        private void SCIBusPCMWriteMemoryVINTextBox_TextChanged(object sender, EventArgs e)
        {
            if (SCIBusPCMWriteMemoryVINTextBox.Text.Length >= 17)
            {
                string NewText = Util.TruncateString(SCIBusPCMWriteMemoryVINTextBox.Text.ToUpper(), 17);
                SCIBusPCMWriteMemoryVINTextBox.Text = NewText;
                SCIBusPCMWriteMemoryVINTextBox.SelectionStart = SCIBusPCMWriteMemoryVINTextBox.Text.Length;
                SCIBusPCMWriteMemoryVINTextBox.ScrollToCaret();
                SCIBusPCMWriteMemoryVINWriteButton.Enabled = true;
            }
            else
            {
                SCIBusPCMWriteMemoryVINWriteButton.Enabled = false;
            }
        }

        private void SCIBusPCMWriteMemoryVINReadButton_Click(object sender, EventArgs e)
        {
            if (!SCIBusPCMReadMemoryWorker.IsBusy && !SCIBusPCMWriteMemoryWorker.IsBusy)
            {
                UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Read VIN.");

                SCIBusPCMMemoryOffsetStart = (uint)VINOffsetStart;
                SCIBusPCMMemoryOffsetStartBytes[0] = (byte)(VINOffsetStart >> 8);
                SCIBusPCMMemoryOffsetStartBytes[1] = (byte)VINOffsetStart;

                SCIBusPCMMemoryOffsetEnd = (uint)(VINOffsetEnd);
                SCIBusPCMMemoryOffsetEndBytes[0] = (byte)(VINOffsetEnd >> 8);
                SCIBusPCMMemoryOffsetEndBytes[1] = (byte)VINOffsetEnd;

                SCIBusPCMCurrentMemoryOffset = SCIBusPCMMemoryOffsetStart;
                SCIBusPCMCurrentMemoryOffsetBytes[0] = SCIBusPCMMemoryOffsetStartBytes[0];
                SCIBusPCMCurrentMemoryOffsetBytes[1] = SCIBusPCMMemoryOffsetStartBytes[1];

                SCIBusPCMTxPayload = new byte[3] { (byte)SCI_ID.ReadEEPROM, SCIBusPCMMemoryOffsetStartBytes[0], SCIBusPCMMemoryOffsetStartBytes[1] };

                SCIBusPCMReadMemoryFinished = false;
                SCIBusPCMNextRequest = true;
                CurrentTask = Task.ReadVIN;
                SCIBusPCMReadMemoryWorker.RunWorkerAsync();
                SCIBusPCMWriteMemoryVINReadButton.Enabled = false;
            }
        }

        private void SCIBusPCMWriteMemoryVINWriteButton_Click(object sender, EventArgs e)
        {
            if (!SCIBusPCMReadMemoryWorker.IsBusy && !SCIBusPCMWriteMemoryWorker.IsBusy && (SCIBusPCMWriteMemoryVINTextBox.Text.Length == VINLength))
            {
                VINString = SCIBusPCMWriteMemoryVINTextBox.Text.ToUpper();
                VIN = Encoding.ASCII.GetBytes(VINString);

                SCIBusPCMMemoryOffsetStart = VINOffsetStart;
                SCIBusPCMMemoryOffsetStartBytes[0] = (byte)(SCIBusPCMMemoryOffsetStart >> 8);
                SCIBusPCMMemoryOffsetStartBytes[1] = (byte)SCIBusPCMMemoryOffsetStart;

                SCIBusPCMMemoryOffsetEnd = VINOffsetEnd;
                SCIBusPCMMemoryOffsetEndBytes[0] = (byte)(SCIBusPCMMemoryOffsetEnd >> 8);
                SCIBusPCMMemoryOffsetEndBytes[1] = (byte)SCIBusPCMMemoryOffsetEnd;

                SCIBusPCMCurrentMemoryOffset = SCIBusPCMMemoryOffsetStart;
                SCIBusPCMCurrentMemoryOffsetBytes[0] = SCIBusPCMMemoryOffsetStartBytes[0];
                SCIBusPCMCurrentMemoryOffsetBytes[1] = SCIBusPCMMemoryOffsetStartBytes[1];

                SCIBusPCMTxPayload = new byte[4] { (byte)SCI_ID.WriteEEPROM, SCIBusPCMMemoryOffsetStartBytes[0], SCIBusPCMMemoryOffsetStartBytes[1], VIN[0] };

                SCIBusPCMWriteMemoryFinished = false;
                SCIBusPCMNextRequest = true;
                CurrentTask = Task.WriteVIN;
                SCIBusPCMWriteMemoryWorker.RunWorkerAsync();
                SCIBusPCMWriteMemoryVINWriteButton.Enabled = false;
            }
        }

        private void SCIBusPCMWriteMemoryVINTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                e.Handled = true;

                if (SCIBusPCMWriteMemoryVINWriteButton.Enabled) SCIBusPCMWriteMemoryVINWriteButton_Click(this, EventArgs.Empty);
            }
        }

        private void SCIBusPCMWriteMemoryPartNumberTextBox_TextChanged(object sender, EventArgs e)
        {
            if (!Util.TruncateString(SCIBusPCMWriteMemoryPartNumberTextBox.Text, 8).ToString().IsNumeric())
            {
                SCIBusPCMWriteMemoryPartNumberTextBox.Clear();
            }

            if (SCIBusPCMWriteMemoryPartNumberTextBox.Text.Length >= 10)
            {
                string NewText = Util.TruncateString(SCIBusPCMWriteMemoryPartNumberTextBox.Text, 10).ToUpper();
                SCIBusPCMWriteMemoryPartNumberTextBox.Text = NewText;
                SCIBusPCMWriteMemoryPartNumberTextBox.SelectionStart = SCIBusPCMWriteMemoryPartNumberTextBox.Text.Length;
                SCIBusPCMWriteMemoryPartNumberTextBox.ScrollToCaret();
            }

            if ((SCIBusPCMWriteMemoryPartNumberTextBox.Text.Length == 8) || (SCIBusPCMWriteMemoryPartNumberTextBox.Text.Length == 10))
            {
                SCIBusPCMWriteMemoryPartNumberWriteButton.Enabled = true;
            }
            else
            {
                SCIBusPCMWriteMemoryPartNumberWriteButton.Enabled = false;
            }
        }

        private void SCIBusPCMWriteMemoryPartNumberReadButton_Click(object sender, EventArgs e)
        {
            if (!SCIBusPCMReadMemoryWorker.IsBusy && !SCIBusPCMWriteMemoryWorker.IsBusy)
            {
                UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Read Part Number.");

                SCIBusPCMMemoryOffsetStart = (uint)PartNumberOffset1Start;
                SCIBusPCMMemoryOffsetStartBytes[0] = (byte)(PartNumberOffset1Start >> 8);
                SCIBusPCMMemoryOffsetStartBytes[1] = unchecked((byte)PartNumberOffset1Start);

                SCIBusPCMMemoryOffsetEnd = (uint)(PartNumberOffset2End);
                SCIBusPCMMemoryOffsetEndBytes[0] = (PartNumberOffset2End >> 8);
                SCIBusPCMMemoryOffsetEndBytes[1] = unchecked((byte)PartNumberOffset2End);

                SCIBusPCMCurrentMemoryOffset = SCIBusPCMMemoryOffsetStart;
                SCIBusPCMCurrentMemoryOffsetBytes[0] = SCIBusPCMMemoryOffsetStartBytes[0];
                SCIBusPCMCurrentMemoryOffsetBytes[1] = SCIBusPCMMemoryOffsetStartBytes[1];

                SCIBusPCMTxPayload = new byte[3] { (byte)SCI_ID.ReadEEPROM, SCIBusPCMMemoryOffsetStartBytes[0], SCIBusPCMMemoryOffsetStartBytes[1] };

                SCIBusPCMReadMemoryFinished = false;
                SCIBusPCMNextRequest = true;
                CurrentTask = Task.ReadPartNumber;
                SCIBusPCMReadMemoryWorker.RunWorkerAsync();
                SCIBusPCMWriteMemoryPartNumberReadButton.Enabled = false;
            }
        }

        private void SCIBusPCMWriteMemoryPartNumberWriteButton_Click(object sender, EventArgs e)
        {
            if (!SCIBusPCMReadMemoryWorker.IsBusy && !SCIBusPCMWriteMemoryWorker.IsBusy && PartNumberLocation != 0)
            {
                byte[] a;

                switch (SCIBusPCMWriteMemoryPartNumberTextBox.Text.Length)
                {
                    case 8:
                        a = Util.HexStringToByte(SCIBusPCMWriteMemoryPartNumberTextBox.Text);
                        Array.Copy(a, PartNumberBuffer, a.Length);

                        PartNumber[0] = PartNumberBuffer[0];
                        PartNumber[1] = PartNumberBuffer[1];
                        PartNumber[2] = PartNumberBuffer[2];
                        PartNumber[3] = PartNumberBuffer[3];
                        PartNumber[4] = 0;
                        PartNumber[5] = 0;

                        break;
                    case 10:
                        string EditedPartNumber = Util.TruncateString(SCIBusPCMWriteMemoryPartNumberTextBox.Text, 8);
                        a = Util.HexStringToByte(EditedPartNumber);
                        Array.Copy(a, PartNumberBuffer, a.Length);

                        PartNumber[0] = PartNumberBuffer[0];
                        PartNumber[1] = PartNumberBuffer[1];
                        PartNumber[2] = PartNumberBuffer[2];
                        PartNumber[3] = PartNumberBuffer[3];
                        PartNumber[4] = Encoding.ASCII.GetBytes(SCIBusPCMWriteMemoryPartNumberTextBox.Text)[8];
                        PartNumber[5] = Encoding.ASCII.GetBytes(SCIBusPCMWriteMemoryPartNumberTextBox.Text)[9];

                        break;
                    default:
                        UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Invalid Part Number length.");
                        break;
                }

                switch (PartNumberLocation)
                {
                    case PartNumberOffset1Start:
                        if ((PartNumber[4] != 0) && (PartNumber[5] != 0)) // 2 revision letters are present
                        {
                            SCIBusPCMMemoryOffsetStart = PartNumberOffset1Start;
                            SCIBusPCMMemoryOffsetStartBytes[0] = (byte)(SCIBusPCMMemoryOffsetStart >> 8);
                            SCIBusPCMMemoryOffsetStartBytes[1] = (byte)SCIBusPCMMemoryOffsetStart;

                            SCIBusPCMMemoryOffsetEnd = PartNumberOffset1End;
                            SCIBusPCMMemoryOffsetEndBytes[0] = (byte)(SCIBusPCMMemoryOffsetEnd >> 8);
                            SCIBusPCMMemoryOffsetEndBytes[1] = (byte)SCIBusPCMMemoryOffsetEnd;
                        }
                        else // no revision letters are present, write first 8 characters (4 bytes) only
                        {
                            SCIBusPCMMemoryOffsetStart = PartNumberOffset1Start;
                            SCIBusPCMMemoryOffsetStartBytes[0] = (byte)(SCIBusPCMMemoryOffsetStart >> 8);
                            SCIBusPCMMemoryOffsetStartBytes[1] = (byte)SCIBusPCMMemoryOffsetStart;

                            SCIBusPCMMemoryOffsetEnd = SCIBusPCMMemoryOffsetStart + 3;
                            SCIBusPCMMemoryOffsetEndBytes[0] = (byte)(SCIBusPCMMemoryOffsetEnd >> 8);
                            SCIBusPCMMemoryOffsetEndBytes[1] = (byte)SCIBusPCMMemoryOffsetEnd;
                        }
                        break;
                    case PartNumberOffset2Start: // 2 revision letters are not written if present in input field 
                        SCIBusPCMMemoryOffsetStart = PartNumberOffset2Start;
                        SCIBusPCMMemoryOffsetStartBytes[0] = (byte)(SCIBusPCMMemoryOffsetStart >> 8);
                        SCIBusPCMMemoryOffsetStartBytes[1] = (byte)SCIBusPCMMemoryOffsetStart;

                        SCIBusPCMMemoryOffsetEnd = PartNumberOffset2End;
                        SCIBusPCMMemoryOffsetEndBytes[0] = (byte)(SCIBusPCMMemoryOffsetEnd >> 8);
                        SCIBusPCMMemoryOffsetEndBytes[1] = (byte)SCIBusPCMMemoryOffsetEnd;

                        break;
                }

                SCIBusPCMCurrentMemoryOffset = SCIBusPCMMemoryOffsetStart;
                SCIBusPCMCurrentMemoryOffsetBytes[0] = SCIBusPCMMemoryOffsetStartBytes[0];
                SCIBusPCMCurrentMemoryOffsetBytes[1] = SCIBusPCMMemoryOffsetStartBytes[1];

                SCIBusPCMTxPayload = new byte[4] { (byte)SCI_ID.WriteEEPROM, SCIBusPCMMemoryOffsetStartBytes[0], SCIBusPCMMemoryOffsetStartBytes[1], PartNumber[0] };

                SCIBusPCMWriteMemoryFinished = false;
                SCIBusPCMNextRequest = true;
                CurrentTask = Task.WritePartNumber;
                SCIBusPCMWriteMemoryWorker.RunWorkerAsync();
                SCIBusPCMWriteMemoryPartNumberWriteButton.Enabled = false;
            }
            else if (PartNumberLocation == 0)
            {
                UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Part Number needs to be read first before writing a new one!");
            }
        }

        private void SCIBusPCMWriteMemoryPartNumberTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                e.Handled = true;

                if (SCIBusPCMWriteMemoryPartNumberWriteButton.Enabled) SCIBusPCMWriteMemoryPartNumberWriteButton_Click(this, EventArgs.Empty);
            }
        }

        private void SCIBusPCMWriteMemoryEEPROMOffsetAndCountAndValueTextBox_TextChanged(object sender, EventArgs e)
        {
            bool OffsetValid;
            bool CountValid;
            bool ValueValid;
            
            if (SCIBusPCMWriteMemoryEEPROMOffsetTextBox.Text != string.Empty)
            {
                byte[] bytes = Util.HexStringToByte(SCIBusPCMWriteMemoryEEPROMOffsetTextBox.Text);

                if ((bytes != null) && (bytes.Length == 2))
                {
                    OffsetValid = true;
                }
                else
                {
                    OffsetValid = false;
                }
            }
            else
            {
                OffsetValid = false;
            }

            uint.TryParse(SCIBusPCMWriteMemoryEEPROMValueCountTextBox.Text, out uint count);

            if (SCIBusPCMWriteMemoryEEPROMValueCountTextBox.Text.IsNumeric() && (count > 0))
            {
                CountValid = true;
            }
            else
            {
                CountValid = false;
            }

            if (SCIBusPCMWriteMemoryEEPROMValueTextBox.Text != string.Empty)
            {
                byte[] bytes = Util.HexStringToByte(SCIBusPCMWriteMemoryEEPROMValueTextBox.Text);

                if ((bytes != null) && (bytes.Length > 0))
                {
                    ValueValid = true;
                }
                else
                {
                    ValueValid = false;
                }
            }
            else
            {
                ValueValid = false;
            }

            if (OffsetValid && CountValid)
            {
                SCIBusPCMWriteMemoryEEPROMReadButton.Enabled = true;
            }
            else
            {
                SCIBusPCMWriteMemoryEEPROMReadButton.Enabled = false;
            }

            if (OffsetValid && ValueValid)
            {
                SCIBusPCMWriteMemoryEEPROMWriteButton.Enabled = true;
            }
            else
            {
                SCIBusPCMWriteMemoryEEPROMWriteButton.Enabled = false;
            }
        }

        private void SCIBusPCMWriteMemoryEEPROMReadButton_Click(object sender, EventArgs e)
        {
            if (SCIBusPCMWriteMemoryEEPROMReadButton.Text == "Stop")
            {
                SCIBusPCMReadMemoryWorker.CancelAsync();
                SCIBusPCMWriteMemoryEEPROMReadButton.Text = "Read";
            }

            if (!SCIBusPCMReadMemoryWorker.IsBusy && !SCIBusPCMWriteMemoryWorker.IsBusy)
            {
                UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Read EEPROM.");

                uint.TryParse(SCIBusPCMWriteMemoryEEPROMValueCountTextBox.Text, out uint count);

                SCIBusPCMMemoryOffsetStartBytes = Util.HexStringToByte(SCIBusPCMWriteMemoryEEPROMOffsetTextBox.Text);
                SCIBusPCMMemoryOffsetStart = (uint)((SCIBusPCMMemoryOffsetStartBytes[0] << 8) | SCIBusPCMMemoryOffsetStartBytes[1]);
                SCIBusPCMMemoryOffsetEnd = SCIBusPCMMemoryOffsetStart + count - 1;
                SCIBusPCMMemoryOffsetEndBytes[0] = (byte)(SCIBusPCMMemoryOffsetEnd >> 8);
                SCIBusPCMMemoryOffsetEndBytes[1] = (byte)SCIBusPCMMemoryOffsetEnd;

                if (SCIBusPCMMemoryOffsetStart > (EEPROMSize - 1))
                {
                    SCIBusPCMMemoryOffsetStart = (EEPROMSize - 1);
                    SCIBusPCMMemoryOffsetStartBytes[0] = (byte)(SCIBusPCMMemoryOffsetStart >> 8);
                    SCIBusPCMMemoryOffsetStartBytes[1] = (byte)(SCIBusPCMMemoryOffsetStart);
                    SCIBusPCMWriteMemoryEEPROMOffsetTextBox.Text = Util.ByteToHexString(SCIBusPCMMemoryOffsetStartBytes, 0, SCIBusPCMMemoryOffsetStartBytes.Length);
                    count = 1;
                    SCIBusPCMWriteMemoryEEPROMValueCountTextBox.Text = count.ToString();
                    SCIBusPCMMemoryOffsetEnd = (EEPROMSize - 1);
                    SCIBusPCMMemoryOffsetEndBytes[0] = (byte)(SCIBusPCMMemoryOffsetEnd >> 8);
                    SCIBusPCMMemoryOffsetEndBytes[1] = (byte)(SCIBusPCMMemoryOffsetEnd);
                }

                if (SCIBusPCMMemoryOffsetEnd > (EEPROMSize - 1))
                {
                    count = (EEPROMSize - 1) - SCIBusPCMMemoryOffsetStart + 1;
                    SCIBusPCMWriteMemoryEEPROMValueCountTextBox.Text = count.ToString();
                    SCIBusPCMMemoryOffsetEnd = (EEPROMSize - 1);
                    SCIBusPCMMemoryOffsetEndBytes[0] = (byte)(SCIBusPCMMemoryOffsetEnd >> 8);
                    SCIBusPCMMemoryOffsetEndBytes[1] = (byte)(SCIBusPCMMemoryOffsetEnd);
                }

                EEPROMBuffer = new byte[count];

                SCIBusPCMCurrentMemoryOffset = SCIBusPCMMemoryOffsetStart;
                SCIBusPCMCurrentMemoryOffsetBytes[0] = SCIBusPCMMemoryOffsetStartBytes[0];
                SCIBusPCMCurrentMemoryOffsetBytes[1] = SCIBusPCMMemoryOffsetStartBytes[1];

                SCIBusPCMTxPayload = new byte[3] { (byte)SCI_ID.ReadEEPROM, SCIBusPCMMemoryOffsetStartBytes[0], SCIBusPCMMemoryOffsetStartBytes[1] };

                SCIBusPCMWriteMemoryEEPROMValueTextBox.Clear();

                SCIBusPCMWriteMemoryEEPROMReadButton.Text = "Stop";

                SCIBusPCMReadMemoryFinished = false;
                SCIBusPCMNextRequest = true;
                CurrentTask = Task.ReadEEPROM;
                SCIBusPCMReadMemoryWorker.RunWorkerAsync();
            }
        }

        private void SCIBusPCMWriteMemoryEEPROMWriteButton_Click(object sender, EventArgs e)
        {
            if (SCIBusPCMWriteMemoryEEPROMWriteButton.Text == "Stop")
            {
                SCIBusPCMWriteMemoryWorker.CancelAsync();
                SCIBusPCMWriteMemoryEEPROMWriteButton.Text = "Write";
            }

            if (!SCIBusPCMReadMemoryWorker.IsBusy && !SCIBusPCMWriteMemoryWorker.IsBusy)
            {
                byte[] a = Util.HexStringToByte(SCIBusPCMWriteMemoryEEPROMValueTextBox.Text);
                uint count = (uint)a.Length;
                
                SCIBusPCMMemoryOffsetStartBytes = Util.HexStringToByte(SCIBusPCMWriteMemoryEEPROMOffsetTextBox.Text);
                SCIBusPCMMemoryOffsetStart = (uint)((SCIBusPCMMemoryOffsetStartBytes[0] << 8) | SCIBusPCMMemoryOffsetStartBytes[1]);
                SCIBusPCMMemoryOffsetEnd = SCIBusPCMMemoryOffsetStart + count - 1;
                SCIBusPCMMemoryOffsetEndBytes[0] = (byte)(SCIBusPCMMemoryOffsetEnd >> 8);
                SCIBusPCMMemoryOffsetEndBytes[1] = (byte)SCIBusPCMMemoryOffsetEnd;

                if (SCIBusPCMMemoryOffsetStart > (EEPROMSize - 1))
                {
                    SCIBusPCMMemoryOffsetStart = (EEPROMSize - 1);
                    SCIBusPCMMemoryOffsetStartBytes[0] = (byte)(SCIBusPCMMemoryOffsetStart >> 8);
                    SCIBusPCMMemoryOffsetStartBytes[1] = (byte)(SCIBusPCMMemoryOffsetStart);
                    SCIBusPCMWriteMemoryEEPROMOffsetTextBox.Text = Util.ByteToHexString(SCIBusPCMMemoryOffsetStartBytes, 0, SCIBusPCMMemoryOffsetStartBytes.Length);
                    count = 1;
                    SCIBusPCMWriteMemoryEEPROMValueCountTextBox.Text = count.ToString();
                    SCIBusPCMMemoryOffsetEnd = (EEPROMSize - 1);
                    SCIBusPCMMemoryOffsetEndBytes[0] = (byte)(SCIBusPCMMemoryOffsetEnd >> 8);
                    SCIBusPCMMemoryOffsetEndBytes[1] = (byte)(SCIBusPCMMemoryOffsetEnd);
                    SCIBusPCMWriteMemoryEEPROMValueTextBox.Text = Util.ByteToHexString(a, 0, (int)count);
                }

                if (SCIBusPCMMemoryOffsetEnd > (EEPROMSize - 1))
                {
                    count = (EEPROMSize - 1) - SCIBusPCMMemoryOffsetStart + 1;
                    SCIBusPCMWriteMemoryEEPROMValueCountTextBox.Text = count.ToString();
                    SCIBusPCMMemoryOffsetEnd = (EEPROMSize - 1);
                    SCIBusPCMMemoryOffsetEndBytes[0] = (byte)(SCIBusPCMMemoryOffsetEnd >> 8);
                    SCIBusPCMMemoryOffsetEndBytes[1] = (byte)(SCIBusPCMMemoryOffsetEnd);
                    SCIBusPCMWriteMemoryEEPROMValueTextBox.Text = Util.ByteToHexString(a, 0, (int)count);
                }

                EEPROMBuffer = new byte[count];
                Array.Copy(a, EEPROMBuffer, count);

                SCIBusPCMCurrentMemoryOffset = SCIBusPCMMemoryOffsetStart;
                SCIBusPCMCurrentMemoryOffsetBytes[0] = SCIBusPCMMemoryOffsetStartBytes[0];
                SCIBusPCMCurrentMemoryOffsetBytes[1] = SCIBusPCMMemoryOffsetStartBytes[1];

                SCIBusPCMTxPayload = new byte[4] { (byte)SCI_ID.WriteEEPROM, SCIBusPCMMemoryOffsetStartBytes[0], SCIBusPCMMemoryOffsetStartBytes[1], EEPROMBuffer[0] };

                SCIBusPCMWriteMemoryEEPROMValueCountTextBox.Text = count.ToString();
                SCIBusPCMWriteMemoryEEPROMWriteButton.Text = "Stop";

                SCIBusPCMWriteMemoryFinished = false;
                SCIBusPCMNextRequest = true;
                CurrentTask = Task.WriteEEPROM;
                SCIBusPCMWriteMemoryWorker.RunWorkerAsync();
            }
        }

        private void SCIBusPCMWriteMemoryEEPROMBackupButton_Click(object sender, EventArgs e)
        {
            if (SCIBusPCMWriteMemoryEEPROMBackupButton.Text == "Stop")
            {
                SCIBusPCMReadMemoryWorker.CancelAsync();
                SCIBusPCMWriteMemoryEEPROMBackupButton.Text = "BAK";
            }

            if (!SCIBusPCMReadMemoryWorker.IsBusy && !SCIBusPCMWriteMemoryWorker.IsBusy)
            {
                SCIBusPCMMemoryEEPROMBackupFilename = @"ROMs/PCM/pcm_eeprom_backup_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".bin";

                UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Backup EEPROM to \"" + SCIBusPCMMemoryEEPROMBackupFilename + "\" is in progress.");

                SCIBusPCMMemoryOffsetStart = 0;
                SCIBusPCMMemoryOffsetStartBytes[0] = 0;
                SCIBusPCMMemoryOffsetStartBytes[1] = 0;

                SCIBusPCMMemoryOffsetEnd = EEPROMSize - 1;
                SCIBusPCMMemoryOffsetEndBytes[0] = (byte)(SCIBusPCMMemoryOffsetEnd >> 8);
                SCIBusPCMMemoryOffsetEndBytes[1] = (byte)SCIBusPCMMemoryOffsetEnd;

                SCIBusPCMWriteMemoryEEPROMValueCountTextBox.Text = EEPROMSize.ToString();

                SCIBusPCMCurrentMemoryOffset = SCIBusPCMMemoryOffsetStart;
                SCIBusPCMCurrentMemoryOffsetBytes[0] = SCIBusPCMMemoryOffsetStartBytes[0];
                SCIBusPCMCurrentMemoryOffsetBytes[1] = SCIBusPCMMemoryOffsetStartBytes[1];

                SCIBusPCMTxPayload = new byte[3] { (byte)SCI_ID.ReadEEPROM, SCIBusPCMMemoryOffsetStartBytes[0], SCIBusPCMMemoryOffsetStartBytes[1] };

                SCIBusPCMWriteMemoryEEPROMBackupButton.Text = "Stop";

                SCIBusPCMReadMemoryFinished = false;
                SCIBusPCMNextRequest = true;
                CurrentTask = Task.BackupEEPROM;
                SCIBusPCMReadMemoryWorker.RunWorkerAsync();
            }
        }

        private void SCIBusPCMWriteMemoryEEPROMRestoreButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = Path.Combine(Application.StartupPath, @"ROMs\PCM");
                openFileDialog.Filter = "Binary files (*.bin)|*.bin|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    using (var stream = File.Open(openFileDialog.FileName, FileMode.Open))
                    {
                        using (var reader = new BinaryReader(stream, Encoding.UTF8, false))
                        {
                            if (stream.Length == 512)
                            {
                                MainForm.Packet.tx.bus = (byte)Packet.Bus.usb;
                                MainForm.Packet.tx.command = (byte)Packet.Command.debug;
                                MainForm.Packet.tx.mode = (byte)Packet.DebugMode.restorePCMEEPROM;
                                MainForm.Packet.tx.payload = reader.ReadBytes(512);
                                MainForm.Packet.GeneratePacket();

                                OriginalForm.TransmitUSBPacket("[<-TX] Restore PCM EEPROM from backup:");
                                UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Restore EEPROM. In progress.");
                            }
                            else
                            {
                                MessageBox.Show("Incorrect file size (" + stream.Length.ToString() + " bytes)!" + Environment.NewLine +
                                                "EEPROM backup file size should be 512 bytes.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                    }
                }
            }
        }

        private void SCIBusPCMWriteMemoryEEPROMOffsetTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                e.Handled = true;

                if (SCIBusPCMWriteMemoryEEPROMReadButton.Enabled) SCIBusPCMWriteMemoryEEPROMReadButton_Click(this, EventArgs.Empty);
            }
        }

        private void SCIBusPCMWriteMemoryEEPROMValueCountTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                e.Handled = true;

                if (SCIBusPCMWriteMemoryEEPROMReadButton.Enabled) SCIBusPCMWriteMemoryEEPROMReadButton_Click(this, EventArgs.Empty);
            }
        }

        private void SCIBusPCMWriteMemoryEEPROMValueTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                e.Handled = true;

                if (SCIBusPCMWriteMemoryEEPROMWriteButton.Enabled) SCIBusPCMWriteMemoryEEPROMWriteButton_Click(this, EventArgs.Empty);
            }
        }

        private void SCIBusPCMWriteMemoryRAMOffsetAndCountAndValueTextBox_TextChanged(object sender, EventArgs e)
        {
            bool OffsetValid;
            bool CountValid;
            bool ValueValid;

            if (SCIBusPCMWriteMemoryRAMOffsetTextBox.Text != string.Empty)
            {
                byte[] bytes = Util.HexStringToByte(SCIBusPCMWriteMemoryRAMOffsetTextBox.Text);

                if ((bytes != null) && (bytes.Length == 2))
                {
                    OffsetValid = true;
                }
                else
                {
                    OffsetValid = false;
                }
            }
            else
            {
                OffsetValid = false;
            }

            uint.TryParse(SCIBusPCMWriteMemoryRAMValueCountTextBox.Text, out uint count);

            if (SCIBusPCMWriteMemoryRAMValueCountTextBox.Text.IsNumeric() && (count > 0))
            {
                CountValid = true;
            }
            else
            {
                CountValid = false;
            }

            if (SCIBusPCMWriteMemoryRAMValueTextBox.Text != string.Empty)
            {
                byte[] bytes = Util.HexStringToByte(SCIBusPCMWriteMemoryRAMValueTextBox.Text);

                if ((bytes != null) && (bytes.Length > 0))
                {
                    ValueValid = true;
                }
                else
                {
                    ValueValid = false;
                }
            }
            else
            {
                ValueValid = false;
            }

            if (OffsetValid && CountValid)
            {
                SCIBusPCMWriteMemoryRAMReadButton.Enabled = true;
            }
            else
            {
                SCIBusPCMWriteMemoryRAMReadButton.Enabled = false;
            }

            if (OffsetValid && ValueValid)
            {
                SCIBusPCMWriteMemoryRAMWriteButton.Enabled = true;
            }
            else
            {
                SCIBusPCMWriteMemoryRAMWriteButton.Enabled = false;
            }
        }

        private void SCIBusPCMWriteMemoryRAMReadButton_Click(object sender, EventArgs e)
        {
            if (SCIBusPCMWriteMemoryRAMReadButton.Text == "Stop")
            {
                SCIBusPCMReadMemoryWorker.CancelAsync();
                SCIBusPCMWriteMemoryRAMReadButton.Text = "Read";
            }

            if (!SCIBusPCMReadMemoryWorker.IsBusy && !SCIBusPCMWriteMemoryWorker.IsBusy)
            {
                uint.TryParse(SCIBusPCMWriteMemoryRAMValueCountTextBox.Text, out uint count);

                UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Read RAM.");

                SCIBusPCMMemoryOffsetStartBytes = Util.HexStringToByte(SCIBusPCMWriteMemoryRAMOffsetTextBox.Text);
                SCIBusPCMMemoryOffsetStart = (uint)((SCIBusPCMMemoryOffsetStartBytes[0] << 8) | SCIBusPCMMemoryOffsetStartBytes[1]);
                SCIBusPCMMemoryOffsetEnd = SCIBusPCMMemoryOffsetStart + count - 1;
                SCIBusPCMMemoryOffsetEndBytes[0] = (byte)(SCIBusPCMMemoryOffsetEnd >> 8);
                SCIBusPCMMemoryOffsetEndBytes[1] = (byte)SCIBusPCMMemoryOffsetEnd;

                if (SCIBusPCMMemoryOffsetStart > (RAMSize - 1))
                {
                    SCIBusPCMMemoryOffsetStart = (RAMSize - 1);
                    SCIBusPCMMemoryOffsetStartBytes[0] = (byte)(SCIBusPCMMemoryOffsetStart >> 8);
                    SCIBusPCMMemoryOffsetStartBytes[1] = (byte)(SCIBusPCMMemoryOffsetStart);
                    SCIBusPCMWriteMemoryRAMOffsetTextBox.Text = Util.ByteToHexString(SCIBusPCMMemoryOffsetStartBytes, 0, SCIBusPCMMemoryOffsetStartBytes.Length);
                    count = 1;
                    SCIBusPCMWriteMemoryRAMValueCountTextBox.Text = count.ToString();
                    SCIBusPCMMemoryOffsetEnd = (RAMSize - 1);
                    SCIBusPCMMemoryOffsetEndBytes[0] = (byte)(SCIBusPCMMemoryOffsetEnd >> 8);
                    SCIBusPCMMemoryOffsetEndBytes[1] = (byte)(SCIBusPCMMemoryOffsetEnd);
                }

                if (SCIBusPCMMemoryOffsetEnd > (RAMSize - 1))
                {
                    count = (RAMSize - 1) - SCIBusPCMMemoryOffsetStart + 1;
                    SCIBusPCMWriteMemoryRAMValueCountTextBox.Text = count.ToString();
                    SCIBusPCMMemoryOffsetEnd = (RAMSize - 1);
                    SCIBusPCMMemoryOffsetEndBytes[0] = (byte)(SCIBusPCMMemoryOffsetEnd >> 8);
                    SCIBusPCMMemoryOffsetEndBytes[1] = (byte)(SCIBusPCMMemoryOffsetEnd);
                }

                RAMBuffer = new byte[count];

                SCIBusPCMCurrentMemoryOffset = SCIBusPCMMemoryOffsetStart;
                SCIBusPCMCurrentMemoryOffsetBytes[0] = SCIBusPCMMemoryOffsetStartBytes[0];
                SCIBusPCMCurrentMemoryOffsetBytes[1] = SCIBusPCMMemoryOffsetStartBytes[1];

                SCIBusPCMTxPayload = new byte[4] { (byte)SCI_ID.ReadROMRAM, 0x0F, (byte)(SCIBusPCMMemoryOffsetStartBytes[0] + 0x80), SCIBusPCMMemoryOffsetStartBytes[1] };

                SCIBusPCMWriteMemoryRAMValueTextBox.Clear();

                SCIBusPCMWriteMemoryRAMReadButton.Text = "Stop";

                SCIBusPCMReadMemoryFinished = false;
                SCIBusPCMNextRequest = true;
                CurrentTask = Task.ReadRAM;
                SCIBusPCMReadMemoryWorker.RunWorkerAsync();
            }
        }

        private void SCIBusPCMWriteMemoryRAMWriteButton_Click(object sender, EventArgs e)
        {
            if (SCIBusPCMWriteMemoryRAMWriteButton.Text == "Stop")
            {
                SCIBusPCMWriteMemoryWorker.CancelAsync();
                SCIBusPCMWriteMemoryRAMWriteButton.Text = "Write";
            }

            if (!SCIBusPCMReadMemoryWorker.IsBusy && !SCIBusPCMWriteMemoryWorker.IsBusy)
            {
                byte[] a = Util.HexStringToByte(SCIBusPCMWriteMemoryRAMValueTextBox.Text);
                uint count = (uint)a.Length;

                SCIBusPCMMemoryOffsetStartBytes = Util.HexStringToByte(SCIBusPCMWriteMemoryRAMOffsetTextBox.Text);
                SCIBusPCMMemoryOffsetStart = (uint)((SCIBusPCMMemoryOffsetStartBytes[0] << 8) | SCIBusPCMMemoryOffsetStartBytes[1]);
                SCIBusPCMMemoryOffsetEnd = SCIBusPCMMemoryOffsetStart + count - 1;
                SCIBusPCMMemoryOffsetEndBytes[0] = (byte)(SCIBusPCMMemoryOffsetEnd >> 8);
                SCIBusPCMMemoryOffsetEndBytes[1] = (byte)SCIBusPCMMemoryOffsetEnd;

                if (SCIBusPCMMemoryOffsetStart > (RAMSize - 1))
                {
                    SCIBusPCMMemoryOffsetStart = (RAMSize - 1);
                    SCIBusPCMMemoryOffsetStartBytes[0] = (byte)(SCIBusPCMMemoryOffsetStart >> 8);
                    SCIBusPCMMemoryOffsetStartBytes[1] = (byte)(SCIBusPCMMemoryOffsetStart);
                    SCIBusPCMWriteMemoryRAMOffsetTextBox.Text = Util.ByteToHexString(SCIBusPCMMemoryOffsetStartBytes, 0, SCIBusPCMMemoryOffsetStartBytes.Length);
                    count = 1;
                    SCIBusPCMWriteMemoryRAMValueCountTextBox.Text = count.ToString();
                    SCIBusPCMMemoryOffsetEnd = (RAMSize - 1);
                    SCIBusPCMMemoryOffsetEndBytes[0] = (byte)(SCIBusPCMMemoryOffsetEnd >> 8);
                    SCIBusPCMMemoryOffsetEndBytes[1] = (byte)(SCIBusPCMMemoryOffsetEnd);
                    SCIBusPCMWriteMemoryRAMValueTextBox.Text = Util.ByteToHexString(a, 0, (int)count);
                }

                if (SCIBusPCMMemoryOffsetEnd > (RAMSize - 1))
                {
                    count = (RAMSize - 1) - SCIBusPCMMemoryOffsetStart + 1;
                    SCIBusPCMWriteMemoryRAMValueCountTextBox.Text = count.ToString();
                    SCIBusPCMMemoryOffsetEnd = (RAMSize - 1);
                    SCIBusPCMMemoryOffsetEndBytes[0] = (byte)(SCIBusPCMMemoryOffsetEnd >> 8);
                    SCIBusPCMMemoryOffsetEndBytes[1] = (byte)(SCIBusPCMMemoryOffsetEnd);
                    SCIBusPCMWriteMemoryRAMValueTextBox.Text = Util.ByteToHexString(a, 0, (int)count);
                }

                RAMBuffer = new byte[count];
                Array.Copy(a, RAMBuffer, count);

                SCIBusPCMCurrentMemoryOffset = SCIBusPCMMemoryOffsetStart;
                SCIBusPCMCurrentMemoryOffsetBytes[0] = SCIBusPCMMemoryOffsetStartBytes[0];
                SCIBusPCMCurrentMemoryOffsetBytes[1] = SCIBusPCMMemoryOffsetStartBytes[1];

                SCIBusPCMTxPayload = new byte[4] { (byte)SCI_ID.WriteRAM, SCIBusPCMMemoryOffsetStartBytes[0], SCIBusPCMMemoryOffsetStartBytes[1], RAMBuffer[0] };

                SCIBusPCMWriteMemoryRAMValueCountTextBox.Text = count.ToString();
                SCIBusPCMWriteMemoryRAMWriteButton.Text = "Stop";

                SCIBusPCMWriteMemoryFinished = false;
                SCIBusPCMNextRequest = true;
                CurrentTask = Task.WriteRAM;
                SCIBusPCMWriteMemoryWorker.RunWorkerAsync();
            }
        }

        private void SCIBusPCMWriteMemoryRAMBackupButton_Click(object sender, EventArgs e)
        {
            if (SCIBusPCMWriteMemoryRAMBackupButton.Text == "Stop")
            {
                SCIBusPCMReadMemoryWorker.CancelAsync();
                SCIBusPCMWriteMemoryRAMBackupButton.Text = "BAK";
            }

            if (!SCIBusPCMReadMemoryWorker.IsBusy && !SCIBusPCMWriteMemoryWorker.IsBusy)
            {
                SCIBusPCMMemoryRAMBackupFilename = @"ROMs/PCM/pcm_ram_backup_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".bin";

                UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Backup RAM to \"" + SCIBusPCMMemoryRAMBackupFilename + "\" is in progress.");

                SCIBusPCMMemoryOffsetStart = 0;
                SCIBusPCMMemoryOffsetStartBytes[0] = 0;
                SCIBusPCMMemoryOffsetStartBytes[1] = 0;

                SCIBusPCMMemoryOffsetEnd = RAMSize - 1;
                SCIBusPCMMemoryOffsetEndBytes[0] = (byte)(SCIBusPCMMemoryOffsetEnd >> 8);
                SCIBusPCMMemoryOffsetEndBytes[1] = (byte)SCIBusPCMMemoryOffsetEnd;

                SCIBusPCMWriteMemoryRAMValueCountTextBox.Text = RAMSize.ToString();

                SCIBusPCMCurrentMemoryOffset = SCIBusPCMMemoryOffsetStart;
                SCIBusPCMCurrentMemoryOffsetBytes[0] = SCIBusPCMMemoryOffsetStartBytes[0];
                SCIBusPCMCurrentMemoryOffsetBytes[1] = SCIBusPCMMemoryOffsetStartBytes[1];

                SCIBusPCMTxPayload = new byte[4] { (byte)SCI_ID.ReadROMRAM, 0x0F, (byte)(SCIBusPCMMemoryOffsetStartBytes[0] + 0x80), SCIBusPCMMemoryOffsetStartBytes[1] };

                SCIBusPCMWriteMemoryRAMBackupButton.Text = "Stop";

                SCIBusPCMReadMemoryFinished = false;
                SCIBusPCMNextRequest = true;
                CurrentTask = Task.BackupRAM;
                SCIBusPCMReadMemoryWorker.RunWorkerAsync();
            }
        }

        private void SCIBusPCMWriteMemoryRAMRestoreButton_Click(object sender, EventArgs e)
        {

        }

        private void SCIBusPCMWriteMemoryRAMOffsetTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                e.Handled = true;

                if (SCIBusPCMWriteMemoryRAMReadButton.Enabled) SCIBusPCMWriteMemoryRAMReadButton_Click(this, EventArgs.Empty);
            }
        }

        private void SCIBusPCMWriteMemoryRAMValueCountTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                e.Handled = true;

                if (SCIBusPCMWriteMemoryRAMReadButton.Enabled) SCIBusPCMWriteMemoryRAMReadButton_Click(this, EventArgs.Empty);
            }
        }

        private void SCIBusPCMWriteMemoryRAMValueTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                e.Handled = true;

                if (SCIBusPCMWriteMemoryRAMWriteButton.Enabled) SCIBusPCMWriteMemoryRAMWriteButton_Click(this, EventArgs.Empty);
            }
        }

        private void SCIBusPCMWriteMemoryCopyBCMMileageButton_Click(object sender, EventArgs e)
        {
            SRIMileageMi = CCDBCMMileageMi;
            SRIMileageKm = CCDBCMMileageKm;

            if (Properties.Settings.Default.Units == "imperial")
            {
                SCIBusPCMWriteMemorySRIMileageTextBox.Text = Math.Round(SRIMileageMi, 1).ToString("0.0").Replace(",", ".");
                SCIBusPCMWriteMemorySRIMileageTextBox.SelectionStart = SCIBusPCMWriteMemorySRIMileageTextBox.Text.Length;
                SCIBusPCMWriteMemorySRIMileageTextBox.ScrollToCaret();

            }
            else if (Properties.Settings.Default.Units == "metric")
            {
                SCIBusPCMWriteMemorySRIMileageTextBox.Text = Math.Round(SRIMileageKm, 1).ToString("0.0").Replace(",", ".");
                SCIBusPCMWriteMemorySRIMileageTextBox.SelectionStart = SCIBusPCMWriteMemorySRIMileageTextBox.Text.Length;
                SCIBusPCMWriteMemorySRIMileageTextBox.ScrollToCaret();
            }

            UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Rounded BCM Mileage copied.");

            CCDBCMMileageReceived = false;
            SCIBusPCMWriteMemoryCopyBCMMileageButton.Enabled = false;
        }

        private void SCIBusPCMWriteMemoryHelpButton_Click(object sender, EventArgs e)
        {
            UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "------------HELP------------");
            UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + "Welcome to the SBEC3 PCM memory manipulator.");
            UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "SRI Mileage:" + Environment.NewLine + "Low-resolution mileage. When entered manually it is rounded to the nearest multiple of 8.192 miles. If the scanner receives mileage from BCM via CCD-bus then the GUI offers an option to copy it.");
            UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "SKIM/VTSS:"  + Environment.NewLine + "Security related setting currently not understood.");
            UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "VIN:" + Environment.NewLine + "Vehicle identification number.");
            UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Part Number:" + Environment.NewLine + "Redundant flash part number. Its value does not seem to affect the PCM.");
            UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "EEPROM Offset/n/Value(s):" + Environment.NewLine + "Advanced option to read and write at arbitrary positions in the EEPROM. Be sure to backup EEPROM with the \"BAK\" button to avoid damage. The binary backup file is saved to the \"ROMs\" folder. Cycle ignition key for changes in EEPROM to take effect.");
            UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "RAM Offset/n/Value(s):" + Environment.NewLine + "Advanced option to read and write at arbitrary positions in the RAM. The \"BAK\" button dumps its content in a binary file to the \"ROMs\" folder. Be cautious changing RAM values because they are directly responsible for PCM behavior. Changes made in RAM are taking effect immediately. RAM reading may not be possible for all SBEC3 PCMs due to limitations (lack of backdoor in firmware).");
            UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Pressing the Enter key at various textboxes causes the related read/write function to be executed. Multiple error-checking measures are implemented to avoid writing bogus data.");
            UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + "----------------------------");
        }

        private void SCIBusPCMReadMemory_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!SCIBusPCMReadMemoryFinished)
            {
                Thread.Sleep(1);

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

                SCIBusPCMReadMemoryWorker.ReportProgress(0); // request message is sent in the ProgressChanged event handler method

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

                if (SCIBusPCMCurrentMemoryOffset > SCIBusPCMMemoryOffsetEnd) SCIBusPCMReadMemoryFinished = true;
            }
        }

        private void SCIBusPCMReadMemory_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 0)
            {
                MainForm.Packet.tx.bus = (byte)Packet.Bus.pcm;
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
                UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, " Cancelled.");

                switch (CurrentTask)
                {
                    case Task.ReadSRIMileage:
                        SCIBusPCMWriteMemorySRIMileageReadButton.Enabled = true;
                        break;
                    case Task.ReadSKIMVTSS:
                        SCIBusPCMWriteMemorySKIMVTSSReadButton.Enabled = true;
                        break;
                    case Task.ReadVIN:
                        SCIBusPCMWriteMemoryVINReadButton.Enabled = true;
                        break;
                    case Task.ReadPartNumber:
                        SCIBusPCMWriteMemoryPartNumberReadButton.Enabled = true;
                        break;
                    case Task.ReadEEPROM:
                        SCIBusPCMWriteMemoryEEPROMReadButton.Enabled = true;
                        break;
                    case Task.ReadRAM:
                        SCIBusPCMWriteMemoryRAMReadButton.Enabled = true;
                        break;
                    case Task.BackupEEPROM:
                        SCIBusPCMWriteMemoryEEPROMBackupButton.Text = "BAK";
                        break;
                    case Task.BackupRAM:
                        SCIBusPCMWriteMemoryRAMBackupButton.Text = "BAK";
                        break;
                }

                if (SCIBusPCMRxTimeout) UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + "Memory read timeout (RX).");
                if (SCIBusPCMTxTimeout) UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + "Memory read timeout (TX).");
            }
            else
            {
                switch (CurrentTask)
                {
                    case Task.ReadSRIMileage:
                        SRIMileageRaw = (uint)((SRIMileage[0] << 8) | SRIMileage[1]);
                        SRIMileageMi = SRIMileageRaw * 8.192D;
                        SRIMileageKm = SRIMileageMi * 1.609344D;

                        if (Properties.Settings.Default.Units == "imperial")
                        {
                            UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, " Done." + Environment.NewLine + "SRI Mileage: " + Math.Round(SRIMileageMi, 1).ToString("0.0").Replace(",", ".") + " mi");
                            SCIBusPCMWriteMemorySRIMileageTextBox.Text = Math.Round(SRIMileageMi, 1).ToString("0.0").Replace(",", ".");
                            SCIBusPCMWriteMemorySRIMileageTextBox.SelectionStart = SCIBusPCMWriteMemorySRIMileageTextBox.Text.Length;
                            SCIBusPCMWriteMemorySRIMileageTextBox.ScrollToCaret();

                        }
                        else if (Properties.Settings.Default.Units == "metric")
                        {
                            UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, " Done." + Environment.NewLine + "SRI Mileage: " + Math.Round(SRIMileageKm, 1).ToString("0.0").Replace(",", ".") + " km");
                            SCIBusPCMWriteMemorySRIMileageTextBox.Text = Math.Round(SRIMileageKm, 1).ToString("0.0").Replace(",", ".");
                            SCIBusPCMWriteMemorySRIMileageTextBox.SelectionStart = SCIBusPCMWriteMemorySRIMileageTextBox.Text.Length;
                            SCIBusPCMWriteMemorySRIMileageTextBox.ScrollToCaret();
                        }

                        SCIBusPCMWriteMemorySRIMileageReadButton.Enabled = true;
                        break;
                    case Task.ReadSKIMVTSS:
                        switch (SKIMVTSS)
                        {
                            case 0x00:
                                SCIBusPCMWriteMemorySKIMVTSSComboBox.SelectedIndex = 1; // enabled
                                break;
                            case 0xFF:
                                SCIBusPCMWriteMemorySKIMVTSSComboBox.SelectedIndex = 0; // disabled
                                break;
                            default:
                                SCIBusPCMWriteMemorySKIMVTSSComboBox.SelectedIndex = 2; // N/A
                                break;
                        }

                        UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, " Done." + Environment.NewLine + "SKIM/VTSS Status: " + Util.ByteToHexStringSimple(new byte[1] { SKIMVTSS }));
                        SCIBusPCMWriteMemorySKIMVTSSReadButton.Enabled = true;
                        break;
                    case Task.ReadVIN:
                        UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, " Done." + Environment.NewLine + "VIN: " + Encoding.ASCII.GetString(VIN, 0, VIN.Length));
                        SCIBusPCMWriteMemoryVINTextBox.Text = Encoding.ASCII.GetString(VIN, 0, VIN.Length);
                        SCIBusPCMWriteMemoryVINTextBox.SelectionStart = SCIBusPCMWriteMemoryVINTextBox.Text.Length;
                        SCIBusPCMWriteMemoryVINTextBox.ScrollToCaret();
                        SCIBusPCMWriteMemoryVINReadButton.Enabled = true;
                        break;
                    case Task.ReadPartNumber:
                        if (PartNumberBuffer[0] != 0xFF)
                        {
                            PartNumberLocation = PartNumberOffset1Start;
                            PartNumber[0] = PartNumberBuffer[0];
                            PartNumber[1] = PartNumberBuffer[1];
                            PartNumber[2] = PartNumberBuffer[2];
                            PartNumber[3] = PartNumberBuffer[3];
                            PartNumber[4] = PartNumberBuffer[7];
                            PartNumber[5] = PartNumberBuffer[8];
                        }
                        else if (PartNumberBuffer[14] != 0xFF)
                        {
                            PartNumberLocation = PartNumberOffset2Start;
                            PartNumber[0] = PartNumberBuffer[14];
                            PartNumber[1] = PartNumberBuffer[15];
                            PartNumber[2] = PartNumberBuffer[16];
                            PartNumber[3] = PartNumberBuffer[17];
                            PartNumber[4] = 0;
                            PartNumber[5] = 0;
                        }
                        else
                        {
                            PartNumberLocation = 0;
                            PartNumber[0] = 0;
                            PartNumber[1] = 0;
                            PartNumber[2] = 0;
                            PartNumber[3] = 0;
                            PartNumber[4] = 0;
                            PartNumber[5] = 0;
                        }

                        if (PartNumber[0] != 0)
                        {
                            string PartNumberString = Util.ByteToHexString(PartNumber, 0, 4).Replace(" ", "");

                            if ((PartNumber[4] != 0) && (PartNumber[5] != 0))
                            {
                                PartNumberString += Encoding.ASCII.GetString(PartNumber, 4, 2);
                            }

                            UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, " Done." + Environment.NewLine + "Part Number: " + PartNumberString);
                            SCIBusPCMWriteMemoryPartNumberTextBox.Text = PartNumberString;
                            SCIBusPCMWriteMemoryPartNumberTextBox.SelectionStart = SCIBusPCMWriteMemoryPartNumberTextBox.Text.Length;
                            SCIBusPCMWriteMemoryPartNumberTextBox.ScrollToCaret();
                            SCIBusPCMWriteMemoryPartNumberReadButton.Enabled = true;
                        }
                        else
                        {
                            UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + "Cannot find Part Number.");
                        }
                        break;
                    case Task.ReadEEPROM:
                        UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, " Done.");
                        UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + "Offset: " + Util.ByteToHexStringSimple(SCIBusPCMMemoryOffsetStartBytes) + " | Count: " + EEPROMBuffer.Length.ToString());
                        UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + "Result:");
                        UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Util.ByteToHexStringSimple(EEPROMBuffer, 8));
                        SCIBusPCMWriteMemoryEEPROMValueTextBox.Text = Util.ByteToHexString(EEPROMBuffer, 0, EEPROMBuffer.Length, EEPROMBuffer.Length);
                        SCIBusPCMWriteMemoryEEPROMValueTextBox.SelectionStart = 0;
                        SCIBusPCMWriteMemoryEEPROMValueTextBox.ScrollToCaret();
                        SCIBusPCMWriteMemoryEEPROMReadButton.Text = "Read";
                        break;
                    case Task.ReadRAM:
                        UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, " Done.");
                        UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + "Offset: " + Util.ByteToHexStringSimple(SCIBusPCMMemoryOffsetStartBytes) + " | Count: " + RAMBuffer.Length.ToString());
                        UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + "Result:");
                        UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Util.ByteToHexStringSimple(RAMBuffer, 8));
                        SCIBusPCMWriteMemoryRAMValueTextBox.Text = Util.ByteToHexString(RAMBuffer, 0, RAMBuffer.Length, RAMBuffer.Length);
                        SCIBusPCMWriteMemoryRAMValueTextBox.SelectionStart = 0;
                        SCIBusPCMWriteMemoryRAMValueTextBox.ScrollToCaret();
                        SCIBusPCMWriteMemoryRAMReadButton.Text = "Read";
                        break;
                    case Task.BackupEEPROM:
                        UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, " Done.");
                        SCIBusPCMWriteMemoryEEPROMBackupButton.Text = "BAK";

                        using (BinaryWriter writer = new BinaryWriter(File.Open(SCIBusPCMMemoryEEPROMBackupFilename, FileMode.Open)))
                        {
                            if (writer.BaseStream.Length != EEPROMSize)
                            {
                                UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + "Incorrect file size!");
                            }
                        }

                        break;
                    case Task.BackupRAM:
                        UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, " Done.");
                        SCIBusPCMWriteMemoryRAMBackupButton.Text = "BAK";

                        using (BinaryWriter writer = new BinaryWriter(File.Open(SCIBusPCMMemoryRAMBackupFilename, FileMode.Open)))
                        {
                            if (writer.BaseStream.Length != RAMSize)
                            {
                                UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + "Incorrect file size!");
                            }
                        }

                        break;
                }
            }

            SCIBusPCMRxTimeout = false;
            SCIBusPCMRxTimeoutTimer.Stop();
            SCIBusPCMTxTimeout = false;
            SCIBusPCMTxTimeoutTimer.Stop();
            CurrentTask = Task.None;
            SCIBusPCMTxPayload = null;
        }

        private void SCIBusPCMWriteMemory_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!SCIBusPCMWriteMemoryFinished)
            {
                Thread.Sleep(1);

                if (SCIBusPCMWriteMemoryWorker.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }

                while (!SCIBusPCMNextRequest) // wait for next request message
                {
                    Thread.Sleep(1);

                    if (SCIBusPCMWriteMemoryWorker.CancellationPending)
                    {
                        e.Cancel = true;
                        break;
                    }
                }

                SCIBusPCMResponse = false;
                SCIBusPCMNextRequest = false;

                SCIBusPCMWriteMemoryWorker.ReportProgress(0); // request message is sent in the ProgressChanged event handler method

                while ((!PCMUnlocked && !SCIBusPCMRxTimeout) || (!SCIBusPCMResponse && !SCIBusPCMRxTimeout))
                {
                    Thread.Sleep(1);

                    if (SCIBusPCMWriteMemoryWorker.CancellationPending)
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

                if (SCIBusPCMCurrentMemoryOffset > SCIBusPCMMemoryOffsetEnd) SCIBusPCMWriteMemoryFinished = true;
            }
        }

        private void SCIBusPCMWriteMemory_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 0)
            {
                if (!PCMUnlocked)
                {
                    MainForm.Packet.tx.bus = (byte)Packet.Bus.pcm;
                    MainForm.Packet.tx.command = (byte)Packet.Command.msgTx;
                    MainForm.Packet.tx.mode = (byte)Packet.MsgTxMode.single;
                    MainForm.Packet.tx.payload = new byte[1] { (byte)SCI_ID.GetSecuritySeed };
                }
                else
                {
                    MainForm.Packet.tx.bus = (byte)Packet.Bus.pcm;
                    MainForm.Packet.tx.command = (byte)Packet.Command.msgTx;
                    MainForm.Packet.tx.mode = (byte)Packet.MsgTxMode.single;
                    MainForm.Packet.tx.payload = SCIBusPCMTxPayload;
                }

                MainForm.Packet.GeneratePacket();
                OriginalForm.TransmitUSBPacket("[<-TX] Send an SCI-bus (PCM) message once:");

                SCIBusPCMRxTimeout = false;
                SCIBusPCMRxTimeoutTimer.Stop();
                SCIBusPCMRxTimeoutTimer.Start();
            }
        }

        private void SCIBusPCMWriteMemory_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled == true)
            {
                switch (CurrentTask)
                {
                    case Task.WriteSRIMileage:
                        UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Write SRI Mileage. Cancelled.");
                        SCIBusPCMWriteMemorySRIMileageTextBox_TextChanged(this, EventArgs.Empty);
                        break;
                    case Task.WriteSKIMVTSS:
                        SCIBusPCMWriteMemorySKIMVTSSComboBox_SelectedIndexChanged(this, EventArgs.Empty);
                        break;
                    case Task.WriteVIN:
                        UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Write VIN. Cancelled.");
                        SCIBusPCMWriteMemoryVINTextBox_TextChanged(this, EventArgs.Empty);
                        break;
                    case Task.WritePartNumber:
                        UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Write Part Number. Cancelled.");
                        SCIBusPCMWriteMemoryPartNumberTextBox_TextChanged(this, EventArgs.Empty);
                        break;
                    case Task.WriteEEPROM:
                        UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Write EEPROM. Cancelled.");
                        SCIBusPCMWriteMemoryEEPROMOffsetAndCountAndValueTextBox_TextChanged(this, EventArgs.Empty);
                        break;
                    case Task.WriteRAM:
                        UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Write RAM. Cancelled.");
                        SCIBusPCMWriteMemoryRAMOffsetAndCountAndValueTextBox_TextChanged(this, EventArgs.Empty);
                        break;
                    case Task.RestoreEEPROM:
                        break;
                }

                if (SCIBusPCMRxTimeout) UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + "Memory write timeout (RX).");
                if (SCIBusPCMTxTimeout) UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + "Memory write timeout (TX).");
            }
            else
            {
                switch (CurrentTask)
                {
                    case Task.WriteSRIMileage:
                        UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Write SRI Mileage. Done.");

                        if (Properties.Settings.Default.Units == "imperial")
                        {
                            UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + "New SRI Mileage: " + Math.Round(SRIMileageMi, 1).ToString("0.0").Replace(",", ".") + " mi");
                        }
                        else if (Properties.Settings.Default.Units == "metric")
                        {
                            UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + "New SRI Mileage: " + Math.Round(SRIMileageKm, 1).ToString("0.0").Replace(",", ".") + " km");
                        }

                        SCIBusPCMWriteMemorySRIMileageTextBox_TextChanged(this, EventArgs.Empty);
                        break;
                    case Task.WriteSKIMVTSS:
                        SCIBusPCMWriteMemorySKIMVTSSComboBox_SelectedIndexChanged(this, EventArgs.Empty);
                        break;
                    case Task.WriteVIN:
                        UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Write VIN. Done.");
                        UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + "New VIN: " + VINString);
                        SCIBusPCMWriteMemoryVINTextBox_TextChanged(this, EventArgs.Empty);
                        break;
                    case Task.WritePartNumber:
                        UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Write Part Number. Done.");

                        string PartNumberString = Util.ByteToHexString(PartNumber, 0, 4).Replace(" ", "");

                        if ((PartNumberLocation == PartNumberOffset1Start) && (PartNumber[4] != 0) && (PartNumber[5] != 0))
                        {
                            PartNumberString += Encoding.ASCII.GetString(PartNumber, 4, 2);
                        }

                        UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + "New Part Number: " + PartNumberString);
                        SCIBusPCMWriteMemoryPartNumberTextBox.Text = PartNumberString;
                        SCIBusPCMWriteMemoryPartNumberTextBox.SelectionStart = SCIBusPCMWriteMemoryPartNumberTextBox.Text.Length;
                        SCIBusPCMWriteMemoryPartNumberTextBox.ScrollToCaret();
                        SCIBusPCMWriteMemoryPartNumberTextBox_TextChanged(this, EventArgs.Empty);
                        break;
                    case Task.WriteEEPROM:
                        UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Write EEPROM. Done.");
                        UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + "Offset: " + Util.ByteToHexStringSimple(SCIBusPCMMemoryOffsetStartBytes) + " | Count: " + EEPROMBuffer.Length.ToString());
                        UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + "Result:");
                        UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Util.ByteToHexStringSimple(EEPROMBuffer, 8));
                        SCIBusPCMWriteMemoryEEPROMWriteButton.Text = "Write";
                        SCIBusPCMWriteMemoryEEPROMOffsetAndCountAndValueTextBox_TextChanged(this, EventArgs.Empty);
                        break;
                    case Task.WriteRAM:
                        UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Write RAM. Done.");
                        UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + "Offset: " + Util.ByteToHexStringSimple(SCIBusPCMMemoryOffsetStartBytes) + " | Count: " + RAMBuffer.Length.ToString());
                        UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + "Result:");
                        UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Util.ByteToHexStringSimple(RAMBuffer, 8));
                        SCIBusPCMWriteMemoryRAMWriteButton.Text = "Write";
                        SCIBusPCMWriteMemoryRAMOffsetAndCountAndValueTextBox_TextChanged(this, EventArgs.Empty);
                        break;
                    case Task.RestoreEEPROM:
                        break;
                }
            }

            SCIBusPCMRxTimeout = false;
            SCIBusPCMRxTimeoutTimer.Stop();
            SCIBusPCMTxTimeout = false;
            SCIBusPCMTxTimeoutTimer.Stop();
            CurrentTask = Task.None;
            SCIBusPCMTxPayload = null;
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

            if (MainForm.Packet.rx.bus == (byte)Packet.Bus.usb)
            {
                switch (MainForm.Packet.rx.command)
                {
                    case (byte)Packet.Command.debug:
                        switch (MainForm.Packet.rx.mode)
                        {
                            case (byte)Packet.DebugMode.restorePCMEEPROM:
                                switch (MainForm.Packet.rx.payload[0])
                                {
                                    case 0:
                                        UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Restore EEPROM. Done.");
                                        break;
                                    default:
                                        UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Restore EEPROM. Error.");
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
            }

            if (MainForm.Packet.rx.bus == (byte)Packet.Bus.ccd)
            {
                byte[] CCDBusResponseBytes = MainForm.Packet.rx.payload.Skip(4).ToArray(); // skip 4 timestamp bytes

                switch (CCDBusResponseBytes[0])
                {
                    case (byte)CCD_ID.BCMMileage:
                        if ((CCDBusResponseBytes.Length == CCDBCMMileageMsgLength) && !CCDBCMMileageReceived)
                        {
                            CCDBCMMileage = CCDBusResponseBytes.Skip(1).Take(4).ToArray(); // skip ID copy 4-bytes long mileage only, ignore checksum (5th byte)
                            CCDBCMMileageMi = Math.Round((uint)(CCDBusResponseBytes[1] << 24 | CCDBusResponseBytes[2] << 16 | CCDBusResponseBytes[3] << 8 | CCDBusResponseBytes[4]) * 0.000125D, 1);
                            CCDBCMMileageKm = Math.Round(CCDBCMMileageMi * 1.609344D, 1);

                            if (Properties.Settings.Default.Units == "imperial")
                            {
                                UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "BCM Mileage: " + CCDBCMMileageMi.ToString("0.0").Replace(",", ".") + " mi");
                            }
                            else if (Properties.Settings.Default.Units == "metric")
                            {
                                UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "BCM Mileage: " + CCDBCMMileageKm.ToString("0.0").Replace(",", ".") + " km");
                            }

                            CCDBCMMileageReceived = true;
                            SCIBusPCMWriteMemoryCopyBCMMileageButton.Enabled = true;
                        }
                        break;
                    default:
                        break;
                }
            }

            if (MainForm.Packet.rx.bus == (byte)Packet.Bus.pcm)
            {
                byte[] SCIBusPCMResponseBytes = MainForm.Packet.rx.payload.Skip(4).ToArray(); // skip 4 timestamp bytes

                switch (SCIBusPCMResponseBytes[0])
                {
                    case (byte)SCI_ID.ReadROMRAM:
                        if (SCIBusPCMResponseBytes.Length > 4)
                        {
                            uint Index = SCIBusPCMCurrentMemoryOffset - SCIBusPCMMemoryOffsetStart;

                            switch (CurrentTask)
                            {
                                case Task.ReadRAM:
                                    RAMBuffer[Index] = SCIBusPCMResponseBytes[4];
                                    break;
                                case Task.BackupRAM:
                                    using (BinaryWriter writer = new BinaryWriter(File.Open(SCIBusPCMMemoryRAMBackupFilename, FileMode.Append)))
                                    {
                                        writer.Write(SCIBusPCMResponseBytes[4]);
                                        writer.Close();
                                    }

                                    SCIBusPCMWriteMemoryRAMOffsetTextBox.Text = Util.ByteToHexString(SCIBusPCMCurrentMemoryOffsetBytes, 0, 2);
                                    SCIBusPCMWriteMemoryRAMValueTextBox.Text = Util.ByteToHexString(SCIBusPCMResponseBytes, 4, 1);
                                    break;
                            }
                            
                            SCIBusPCMResponse = true;
                            SCIBusPCMCurrentMemoryOffset++;
                            SCIBusPCMCurrentMemoryOffsetBytes[0] = (byte)(SCIBusPCMCurrentMemoryOffset >> 8);
                            SCIBusPCMCurrentMemoryOffsetBytes[1] = (byte)SCIBusPCMCurrentMemoryOffset;
                            SCIBusPCMTxPayload = new byte[4] { (byte)SCI_ID.ReadROMRAM, 0x0F, (byte)(SCIBusPCMCurrentMemoryOffsetBytes[0] + 0x80), SCIBusPCMCurrentMemoryOffsetBytes[1] };
                        }
                        break;
                    case (byte)SCI_ID.WriteEEPROM:
                        if (SCIBusPCMResponseBytes.Length > 4)
                        {
                            if (SCIBusPCMResponseBytes[4] == 0xE2) // write ok
                            {
                                SCIBusPCMResponse = true;
                                SCIBusPCMCurrentMemoryOffset++;
                                SCIBusPCMCurrentMemoryOffsetBytes[0] = (byte)(SCIBusPCMCurrentMemoryOffset >> 8);
                                SCIBusPCMCurrentMemoryOffsetBytes[1] = (byte)SCIBusPCMCurrentMemoryOffset;

                                uint Index = SCIBusPCMCurrentMemoryOffset - SCIBusPCMMemoryOffsetStart;
                                byte Payload = 0;

                                switch (CurrentTask)
                                {
                                    case Task.WriteSRIMileage:
                                        if (Index >= SRIMileageNew.Length) Index = (uint)(SRIMileageNew.Length - 1);
                                        Payload = SRIMileageNew[Index];
                                        break;
                                    case Task.WriteSKIMVTSS:
                                        break;
                                    case Task.WriteVIN:
                                        if (Index >= VIN.Length) Index = (uint)(VIN.Length - 1);
                                        Payload = VIN[Index];
                                        break;
                                    case Task.WritePartNumber:
                                        if (Index >= PartNumber.Length) Index = (uint)(PartNumber.Length - 1);

                                        if ((SCIBusPCMCurrentMemoryOffset == (PartNumberOffset1Start + 4)) && (PartNumberLocation == PartNumberOffset1Start) && (PartNumber[4] != 0) && (PartNumber[5] != 0))
                                        {
                                            // Skip gap between numbers and last 2 revision letters.
                                            
                                            SCIBusPCMMemoryOffsetStart += 3; // move start offset to the first revision letter
                                            SCIBusPCMMemoryOffsetStartBytes[0] = (byte)(SCIBusPCMMemoryOffsetStart >> 8);
                                            SCIBusPCMMemoryOffsetStartBytes[1] = (byte)SCIBusPCMMemoryOffsetStart;

                                            SCIBusPCMCurrentMemoryOffset += 3; // move current memory offset to the first revision letter
                                            SCIBusPCMCurrentMemoryOffsetBytes[0] = (byte)(SCIBusPCMCurrentMemoryOffset >> 8);
                                            SCIBusPCMCurrentMemoryOffsetBytes[1] = (byte)SCIBusPCMCurrentMemoryOffset;

                                            Index = SCIBusPCMCurrentMemoryOffset - SCIBusPCMMemoryOffsetStart; // calculate index of the first revision letter
                                        }

                                        Payload = PartNumber[Index];
                                        break;
                                    case Task.WriteEEPROM:
                                        if (Index >= EEPROMBuffer.Length) Index = (uint)(EEPROMBuffer.Length - 1);
                                        Payload = EEPROMBuffer[Index];
                                        break;
                                    case Task.RestoreEEPROM:
                                        break;
                                }

                                SCIBusPCMTxPayload = new byte[4] { (byte)SCI_ID.WriteEEPROM, SCIBusPCMCurrentMemoryOffsetBytes[0], SCIBusPCMCurrentMemoryOffsetBytes[1], Payload };
                            }
                            else if (SCIBusPCMResponseBytes[4] == 0xF1) // no security clearance
                            {
                                PCMUnlocked = false;
                            }
                            else if (SCIBusPCMResponseBytes[4] == 0xF0) // offset out of range
                            {

                            }
                        }
                        break;
                    case (byte)SCI_ID.ReadEEPROM:
                        if (SCIBusPCMResponseBytes.Length > 3)
                        {
                            uint Index = SCIBusPCMCurrentMemoryOffset - SCIBusPCMMemoryOffsetStart;

                            switch (CurrentTask)
                            {
                                case Task.ReadSRIMileage:
                                    SRIMileage[Index] = SCIBusPCMResponseBytes[3];
                                    break;
                                case Task.ReadSKIMVTSS:
                                    SKIMVTSS = SCIBusPCMResponseBytes[3];
                                    break;
                                case Task.ReadVIN:
                                    VIN[Index] = SCIBusPCMResponseBytes[3];
                                    break;
                                case Task.ReadPartNumber:
                                    PartNumberBuffer[Index] = SCIBusPCMResponseBytes[3];
                                    break;
                                case Task.ReadEEPROM:
                                    EEPROMBuffer[Index] = SCIBusPCMResponseBytes[3];
                                    break;
                                case Task.BackupEEPROM:
                                    using (BinaryWriter writer = new BinaryWriter(File.Open(SCIBusPCMMemoryEEPROMBackupFilename, FileMode.Append)))
                                    {
                                        writer.Write(SCIBusPCMResponseBytes[3]);
                                        writer.Close();
                                    }

                                    SCIBusPCMWriteMemoryEEPROMOffsetTextBox.Text = Util.ByteToHexString(SCIBusPCMResponseBytes, 1, 2);
                                    SCIBusPCMWriteMemoryEEPROMValueTextBox.Text = Util.ByteToHexString(SCIBusPCMResponseBytes, 3, 1);
                                    break;
                            }

                            SCIBusPCMResponse = true;
                            SCIBusPCMCurrentMemoryOffset++;
                            SCIBusPCMCurrentMemoryOffsetBytes[0] = (byte)(SCIBusPCMCurrentMemoryOffset >> 8);
                            SCIBusPCMCurrentMemoryOffsetBytes[1] = (byte)SCIBusPCMCurrentMemoryOffset;
                            SCIBusPCMTxPayload = new byte[3] { (byte)SCI_ID.ReadEEPROM, SCIBusPCMCurrentMemoryOffsetBytes[0], SCIBusPCMCurrentMemoryOffsetBytes[1] };
                        }
                        break;
                    case (byte)SCI_ID.WriteRAM:
                        if (SCIBusPCMResponseBytes.Length > 4)
                        {
                            if (SCIBusPCMResponseBytes[4] == 0xE5) // write ok
                            {
                                SCIBusPCMResponse = true;
                                SCIBusPCMCurrentMemoryOffset++;
                                SCIBusPCMCurrentMemoryOffsetBytes[0] = (byte)(SCIBusPCMCurrentMemoryOffset >> 8);
                                SCIBusPCMCurrentMemoryOffsetBytes[1] = (byte)SCIBusPCMCurrentMemoryOffset;

                                uint Index = SCIBusPCMCurrentMemoryOffset - SCIBusPCMMemoryOffsetStart;
                                byte Payload = 0;

                                switch (CurrentTask)
                                {
                                    case Task.WriteRAM:
                                        if (Index >= RAMBuffer.Length) Index = (uint)(RAMBuffer.Length - 1);
                                        Payload = RAMBuffer[Index];
                                        break;
                                }

                                SCIBusPCMTxPayload = new byte[4] { (byte)SCI_ID.WriteRAM, SCIBusPCMCurrentMemoryOffsetBytes[0], SCIBusPCMCurrentMemoryOffsetBytes[1], Payload };
                            }
                            else if (SCIBusPCMResponseBytes[4] == 0xF1) // no security clearance
                            {
                                PCMUnlocked = false;
                            }
                            else if (SCIBusPCMResponseBytes[4] == 0xF0) // offset out of range
                            {

                            }
                        }
                        break;
                    case (byte)SCI_ID.GetSecuritySeed:
                        if (SCIBusPCMResponseBytes.Length > 3)
                        {
                            if ((SCIBusPCMResponseBytes[1] == 0) && (SCIBusPCMResponseBytes[2] == 0))
                            {
                                if (SCIBusPCMResponseBytes[3] == (byte)SCI_ID.GetSecuritySeed) // checksum ok
                                {
                                    PCMUnlocked = true;
                                    SCIBusPCMResponse = true;
                                    UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Check PCM status: unlocked.");
                                }
                                else // checksum error
                                {
                                    PCMUnlocked = false;
                                    UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Check PCM status: error [1].");
                                }
                            }
                            else
                            {
                                PCMUnlocked = false;

                                byte checksum = (byte)((byte)SCI_ID.GetSecuritySeed + SCIBusPCMResponseBytes[1] + SCIBusPCMResponseBytes[2]);

                                if (checksum == SCIBusPCMResponseBytes[3]) // checksum ok
                                {
                                    SCIBusPCMResponse = true;
                                    UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Check PCM status: locked.");

                                    ushort seed = (ushort)((SCIBusPCMResponseBytes[1] << 8) | SCIBusPCMResponseBytes[2]);
                                    ushort solution = (ushort)((seed << 2) + PCMUnlockKey);
                                    byte solutionHB = (byte)(solution >> 8);
                                    byte solutionLB = (byte)(solution);
                                    byte solutionChecksum = (byte)((byte)SCI_ID.SendSecuritySeed + solutionHB + solutionLB);
                                    byte[] solutionArray = { (byte)SCI_ID.SendSecuritySeed, solutionHB, solutionLB, solutionChecksum };

                                    UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + "Attempting to unlock PCM.");

                                    MainForm.Packet.tx.bus = (byte)Packet.Bus.pcm;
                                    MainForm.Packet.tx.command = (byte)Packet.Command.msgTx;
                                    MainForm.Packet.tx.mode = (byte)Packet.MsgTxMode.single;
                                    MainForm.Packet.tx.payload = solutionArray;
                                    MainForm.Packet.GeneratePacket();

                                    OriginalForm.TransmitUSBPacket("[<-TX] Send a SCI-bus (PCM) message once:");
                                }
                                else // checksum error
                                {
                                    UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Check PCM status: error [2].");
                                }
                            }
                        }
                        else
                        {
                            UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Check PCM status: error [3].");
                        }
                        break;
                    case (byte)SCI_ID.SendSecuritySeed:
                        if (SCIBusPCMResponseBytes.Length > 4)
                        {
                            switch (SCIBusPCMResponseBytes[4])
                            {
                                case 0x00:
                                    PCMUnlocked = true;
                                    SCIBusPCMResponse = true;
                                    UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + "PCM unlocked.");
                                    break;
                                default:
                                    PCMUnlocked = false;
                                    SCIBusPCMResponse = true;
                                    UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + "PCM unlock failed [" + Util.ByteToHexString(SCIBusPCMResponseBytes, 4, 1) + "].");
                                    break;
                            }
                        }
                        else
                        {
                            PCMUnlocked = false;
                            UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + "PCM unlock error [1].");
                        }
                        break;
                    default:
                        break;
                }

                SCIBusPCMNextRequest = false;
                SCIBusPCMNextRequestTimer.Stop();
                SCIBusPCMNextRequestTimer.Start();
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

                if ((textBox.Name == "SCIBusPCMWriteMemoryInfoTextBox") && (SCIBusPCMWriteMemoryLogFilename != null)) File.AppendAllText(SCIBusPCMWriteMemoryLogFilename, text);
            }
        }

        public void UpdateMileageUnit()
        {
            if (Properties.Settings.Default.Units == "imperial")
            {
                SCIBusPCMWriteMemorySRIMileageUnitLabel.Text = "mi";
                if (SRIMileageMi != 0) SCIBusPCMWriteMemorySRIMileageTextBox.Text = SRIMileageMi.ToString("0.0").Replace(",", ".");
            }
            else if (Properties.Settings.Default.Units == "metric")
            {
                SCIBusPCMWriteMemorySRIMileageUnitLabel.Text = "km";
                if (SRIMileageKm != 0) SCIBusPCMWriteMemorySRIMileageTextBox.Text = SRIMileageKm.ToString("0.0").Replace(",", ".");
            }
        }

        private void WriteMemoryForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (SCIBusPCMReadMemoryWorker.IsBusy) SCIBusPCMReadMemoryWorker.CancelAsync();
            if (SCIBusPCMWriteMemoryWorker.IsBusy) SCIBusPCMWriteMemoryWorker.CancelAsync();
            MainForm.Packet.PacketReceived -= PacketReceivedHandler; // unsubscribe from the OnPacketReceived event
        }

        #endregion
    }
}
