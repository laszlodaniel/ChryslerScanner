using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace ChryslerScanner
{
    public class SCITCM
    {
        public SCITCMDiagnosticsTable Diagnostics = new SCITCMDiagnosticsTable();
        public DataTable TransmissionDTC = new DataTable("TransmissionDTC");
        public List<byte> transmissionFaultCodeList = new List<byte>();
        public bool transmissionFaultCodesSaved = true;
        public byte[] transmissionDTCList;
        public DataColumn column;
        public DataRow row;

        private const int hexBytesColumnStart = 2;
        private const int descriptionColumnStart = 28;
        private const int valueColumnStart = 82;
        private const int unitColumnStart = 108;

        public string state = null;
        public string speed = null;
        public string logic = null;
        public string configuration = null;

        public string HeaderUnknown  = "│ SCI-BUS TRANSMISSION    │ STATE: N/A                                                                                   ";
        public string HeaderDisabled = "│ SCI-BUS TRANSMISSION    │ STATE: DISABLED                                                                              ";
        public string HeaderEnabled  = "│ SCI-BUS TRANSMISSION    │ STATE: ENABLED @ BAUD | LOGIC: | CONFIGURATION:                                              ";
        public string EmptyLine      = "│                         │                                                     │                         │             │";
        public string HeaderModified = string.Empty;

        public SCITCM()
        {
            column = new DataColumn();
            column.DataType = typeof(byte);
            column.ColumnName = "id";
            column.ReadOnly = true;
            column.Unique = true;
            TransmissionDTC.Columns.Add(column);

            column = new DataColumn();
            column.DataType = typeof(string);
            column.ColumnName = "description";
            column.ReadOnly = true;
            column.Unique = false;
            TransmissionDTC.Columns.Add(column);

            DataColumn[] PrimaryKeyColumnsDTC = new DataColumn[1];
            PrimaryKeyColumnsDTC[0] = TransmissionDTC.Columns["id"];
            TransmissionDTC.PrimaryKey = PrimaryKeyColumnsDTC;

            DataSet dataSetDTC = new DataSet();
            dataSetDTC.Tables.Add(TransmissionDTC);

            #region SCI-bus (TCM) fault codes

            row = TransmissionDTC.NewRow();
            row["id"] = 0x00;
            row["description"] = "UNRECOGNIZED DTC";
            TransmissionDTC.Rows.Add(row);

            #endregion

            transmissionDTCList = TransmissionDTC.AsEnumerable().Select(r => r.Field<byte>("id")).ToArray();
        }

        public void UpdateHeader(string state = "enabled", string speed = null, string logic = null, string configuration = null)
        {
            if (state != null) this.state = state;
            if (speed != null) this.speed = speed;
            if (logic != null) this.logic = logic;
            if (configuration != null) this.configuration = configuration;

            if ((this.state == "enabled") && (this.speed != null) && (this.logic != null) && (this.configuration != null))
            {
                HeaderModified = HeaderEnabled.Replace("@ BAUD", "@ " + this.speed.ToUpper()).Replace("LOGIC:", "LOGIC: " + this.logic.ToUpper()).Replace("CONFIGURATION: ", "CONFIGURATION: " + this.configuration);
                HeaderModified = Util.TruncateString(HeaderModified, EmptyLine.Length);
                Diagnostics.UpdateHeader(HeaderModified);
            }
            else if (this.state == "disabled")
            {
                Diagnostics.UpdateHeader(HeaderDisabled);
            }
            else
            {
                Diagnostics.UpdateHeader(HeaderUnknown);
            }
        }

        public void AddMessage(byte[] data)
        {
            if ((data == null) || (data.Length < 5)) return;
            
            StringBuilder rowToAdd = new StringBuilder(EmptyLine); // add empty line first

            byte[] timestamp = new byte[4];
            byte[] message = new byte[] { };
            byte[] payload = new byte[] { };

            if (data.Length > 3)
            {
                Array.Copy(data, 0, timestamp, 0, 4);
            }

            if (data.Length > 4)
            {
                message = new byte[data.Length - 4];
                Array.Copy(data, 4, message, 0, message.Length); // copy message from the input byte array
            }

            if (data.Length > 5)
            {
                payload = new byte[data.Length - 5];
                Array.Copy(data, 5, payload, 0, payload.Length); // copy payload from the input byte array (without ID)
            }

            byte ID = message[0];
            ushort modifiedID;

            if (ID == 0x14)
            {
                if (payload.Length > 0) modifiedID = (ushort)(((ID << 8) & 0xFF00) + payload[0]);
                else modifiedID = (ushort)((ID << 8) & 0xFF00);
            }
            else
            {
                modifiedID = (ushort)((ID << 8) & 0xFF00);
            }

            string descriptionToInsert = string.Empty;
            string valueToInsert = string.Empty;
            string unitToInsert = string.Empty;

            if ((speed == "976.5 baud") || (speed == "7812.5 baud"))
            {
                switch (ID)
                {
                    default:
                        descriptionToInsert = string.Empty;
                        break;
                }
            }
            else if ((speed == "62500 baud") || (speed == "125000 baud"))
            {
                switch (ID)
                {
                    default:
                        descriptionToInsert = string.Empty;
                        break;
                }
            }

            string hexBytesToInsert;

            if (message.Length < 9) // max 8 message byte fits the message column
            {
                hexBytesToInsert = Util.ByteToHexString(message, 0, message.Length) + " ";
            }
            else // trim message (display 7 bytes only) and insert two dots at the end indicating there's more to it
            {
                hexBytesToInsert = Util.ByteToHexString(message, 0, 7) + " .. ";
            }

            if (descriptionToInsert.Length > 51) descriptionToInsert = Util.TruncateString(descriptionToInsert, 48) + "...";
            if (valueToInsert.Length > 23) valueToInsert = Util.TruncateString(valueToInsert, 20) + "...";
            if (unitToInsert.Length > 11) unitToInsert = Util.TruncateString(unitToInsert, 8) + "...";

            rowToAdd.Remove(hexBytesColumnStart, hexBytesToInsert.Length);
            rowToAdd.Insert(hexBytesColumnStart, hexBytesToInsert);

            rowToAdd.Remove(descriptionColumnStart, descriptionToInsert.Length);
            rowToAdd.Insert(descriptionColumnStart, descriptionToInsert);

            rowToAdd.Remove(valueColumnStart, valueToInsert.Length);
            rowToAdd.Insert(valueColumnStart, valueToInsert);

            rowToAdd.Remove(unitColumnStart, unitToInsert.Length);
            rowToAdd.Insert(unitColumnStart, unitToInsert);

            Diagnostics.AddRow(modifiedID, rowToAdd.ToString());

            UpdateHeader();

            if (Properties.Settings.Default.Timestamp == true)
            {
                TimeSpan ElapsedTime = TimeSpan.FromMilliseconds(timestamp[0] << 24 | timestamp[1] << 16 | timestamp[2] << 8 | timestamp[3]);
                DateTime Timestamp = DateTime.Today.Add(ElapsedTime);
                string TimestampString = Timestamp.ToString("HH:mm:ss.fff") + " ";
                File.AppendAllText(MainForm.TCMLogFilename, TimestampString); // no newline is appended!
            }

            File.AppendAllText(MainForm.TCMLogFilename, Util.ByteToHexStringSimple(message) + Environment.NewLine);
        }
    }
}
