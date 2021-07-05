using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChryslerCCDSCIScanner
{
    public partial class WriteMemoryForm : Form
    {
        public MainForm originalForm;

        public TimeSpan ElapsedMillis;
        public DateTime Timestamp;
        public string TimestampString;

        public WriteMemoryForm(MainForm incomingForm)
        {
            originalForm = incomingForm;
            InitializeComponent();
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            MainForm.Packet.PacketReceived += PacketReceivedHandler; // subscribe to the OnPacketReceived event
        }

        public void PacketReceivedHandler(object sender, EventArgs e)
        {
            if (MainForm.Packet.rx.payload.Length > 4)
            {
                ElapsedMillis = TimeSpan.FromMilliseconds((MainForm.Packet.rx.payload[0] << 24) | (MainForm.Packet.rx.payload[1] << 16) | (MainForm.Packet.rx.payload[2] << 8) | MainForm.Packet.rx.payload[3]);
                Timestamp = DateTime.Today.Add(ElapsedMillis);
                TimestampString = Timestamp.ToString("HH:mm:ss.fff");
            }

            if (MainForm.Packet.rx.source == 0x01) // CCD-bus
            {
                // TODO
            }

            if (MainForm.Packet.rx.source == 0x02) // SCI-bus (PCM)
            {
                byte[] SCIBusPCMResponseBytes = MainForm.Packet.rx.payload.Skip(4).ToArray(); // skip 4 timestamp bytes

                // TODO
            }
        }
    }
}
