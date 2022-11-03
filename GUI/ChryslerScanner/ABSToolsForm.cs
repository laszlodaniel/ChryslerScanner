using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace ChryslerScanner
{
    public partial class ABSToolsForm : Form
    {
        private MainForm OriginalForm;

        private string ABSToolsLogFilename;

        private System.Timers.Timer ABSToolsNextRequestTimer = new System.Timers.Timer();
        private System.Timers.Timer ABSToolsRxTimeoutTimer = new System.Timers.Timer();
        private System.Timers.Timer ABSToolsTxTimeoutTimer = new System.Timers.Timer();
        private BackgroundWorker ABSToolsWorker = new BackgroundWorker();

        private bool ABSToolsResponse = false;
        private bool ABSToolsNextRequest = false;
        private byte ABSToolsRxRetryCount = 0;
        private byte ABSToolsTxRetryCount = 0;
        private bool ABSToolsFinished = false;
        private bool ABSToolsRxTimeout = false;
        private bool ABSToolsTxTimeout = false;
        private byte[] ABSToolsTxPayload = new byte[4];

        private enum Task
        {
            None,
            ReadID,
            ActuatorTest,
            ActuatorTestMonitor,
            ReadFaults,
            EraseFaults,
            ReadAnalog,
            ReadDigital,
            ReadEEPROM,
            WriteEEPROM
        }

        public ABSToolsForm(MainForm IncomingForm)
        {
            OriginalForm = IncomingForm;
            InitializeComponent();
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            MainForm.Packet.PacketReceived += PacketReceivedHandler; // subscribe to the OnPacketReceived event

            ABSToolsLogFilename = @"LOG/ABS/abslog_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt";

            ABSToolsNextRequestTimer.Elapsed += new ElapsedEventHandler(ABSToolsNextRequestHandler);
            ABSToolsNextRequestTimer.Interval = 25; // ms
            ABSToolsNextRequestTimer.AutoReset = false;
            ABSToolsNextRequestTimer.Enabled = true;
            ABSToolsNextRequestTimer.Start();

            ABSToolsRxTimeoutTimer.Elapsed += new ElapsedEventHandler(ABSToolsRxTimeoutHandler);
            ABSToolsRxTimeoutTimer.Interval = 2000; // ms
            ABSToolsRxTimeoutTimer.AutoReset = false;
            ABSToolsRxTimeoutTimer.Enabled = true;
            ABSToolsRxTimeoutTimer.Stop();

            ABSToolsTxTimeoutTimer.Elapsed += new ElapsedEventHandler(ABSToolsTxTimeoutHandler);
            ABSToolsTxTimeoutTimer.Interval = 2000; // ms
            ABSToolsTxTimeoutTimer.AutoReset = false;
            ABSToolsTxTimeoutTimer.Enabled = true;
            ABSToolsTxTimeoutTimer.Stop();

            ABSModuleInformationComboBox.SelectedIndex = 0;
        }

        private void ABSToolsNextRequestHandler(object source, ElapsedEventArgs e) => ABSToolsNextRequest = true;

        private void ABSToolsRxTimeoutHandler(object source, ElapsedEventArgs e) => ABSToolsRxTimeout = true;

        private void ABSToolsTxTimeoutHandler(object source, ElapsedEventArgs e) => ABSToolsTxTimeout = true;

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
                byte[] CCDBusResponseBytes = MainForm.Packet.rx.payload.Skip(4).ToArray(); // skip 4 timestamp bytes

                switch (CCDBusResponseBytes[0])
                {
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
