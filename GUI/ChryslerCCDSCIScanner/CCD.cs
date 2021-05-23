using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace ChryslerCCDSCIScanner
{
    public class CCD
    {
        public CCDDiagnosticsTable Diagnostics = new CCDDiagnosticsTable();
        public DataTable MessageDatabase = new DataTable("CCDDatabase");
        public ushort[] IDList;
        public DataColumn column;
        public DataRow row;

        public string VIN = "-----------------"; // 17 characters

        private const int hexBytesColumnStart = 2;
        private const int descriptionColumnStart = 28;
        private const int valueColumnStart = 82;
        private const int unitColumnStart = 108;
        private string state = string.Empty;
        private string speed = "7812.5 baud";
        private string logic = "non-inverted";

        public string HeaderUnknown  = "│ CCD-BUS MODULES         │ STATE: N/A                                                                                   ";
        public string HeaderDisabled = "│ CCD-BUS MODULES         │ STATE: DISABLED                                                                              ";
        public string HeaderEnabled  = "│ CCD-BUS MODULES         │ STATE: ENABLED @ BAUD | LOGIC: | ID BYTES:                                                   ";
        public string EmptyLine      = "│                         │                                                     │                         │             │";
        public string HeaderModified = string.Empty;

        public CCD()
        {
            column = new DataColumn();
            column.DataType = typeof(ushort);
            column.ColumnName = "id";
            column.ReadOnly = true;
            column.Unique = true;
            MessageDatabase.Columns.Add(column);

            column = new DataColumn();
            column.DataType = typeof(byte);
            column.ColumnName = "length";
            column.ReadOnly = true;
            column.Unique = false;
            MessageDatabase.Columns.Add(column);

            column = new DataColumn();
            column.DataType = typeof(byte);
            column.ColumnName = "parameterCount";
            column.ReadOnly = true;
            column.Unique = false;
            MessageDatabase.Columns.Add(column);

            column = new DataColumn();
            column.DataType = typeof(string);
            column.ColumnName = "message";
            column.ReadOnly = false;
            column.Unique = false;
            MessageDatabase.Columns.Add(column);

            column = new DataColumn();
            column.DataType = typeof(string);
            column.ColumnName = "description";
            column.ReadOnly = false;
            column.Unique = false;
            MessageDatabase.Columns.Add(column);

            column = new DataColumn();
            column.DataType = typeof(string);
            column.ColumnName = "value";
            column.ReadOnly = false;
            column.Unique = false;
            MessageDatabase.Columns.Add(column);

            column = new DataColumn();
            column.DataType = typeof(string);
            column.ColumnName = "unit";
            column.ReadOnly = false;
            column.Unique = false;
            MessageDatabase.Columns.Add(column);

            DataColumn[] PrimaryKeyColumns = new DataColumn[1];
            PrimaryKeyColumns[0] = MessageDatabase.Columns["id"];
            MessageDatabase.PrimaryKey = PrimaryKeyColumns;

            DataSet dataSet = new DataSet();
            dataSet.Tables.Add(MessageDatabase);

            #region CCD-bus messages

            row = MessageDatabase.NewRow();
            row["id"] = 0x02;
            row["length"] = 3;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "SHIFT LEVER POSITION";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x0A;
            row["length"] = 4;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "SEND DIAGNOSTIC FAILURE DATA";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x0B;
            row["length"] = 4;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "SKIM PRESENT";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x0C;
            row["length"] = 6;
            row["parameterCount"] = 4;
            row["message"] = string.Empty;
            row["description"] = "BATTERY: | OIL: | COOLANT: ";
            row["value"] = "IAT: ";
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x0D;
            row["length"] = 4;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "UNKNOWN FEATURE PRESENT";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x10;
            row["length"] = 4;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "HVAC MESSAGE";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x11;
            row["length"] = 4;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "UNKNOWN FEATURE PRESENT";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x12;
            row["length"] = 6;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "REQUEST EEPROM READ - COMPASS MINI-TRIP";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x16;
            row["length"] = 3;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "VEHICLE THEFT SECURITY STATE";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x1B;
            row["length"] = 4;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "LAST OS TEMPERATURE";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x1C;
            row["length"] = 4;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "FUEL LEVEL COUNTS";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x23;
            row["length"] = 4;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "COUNTRY CODE";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x24;
            row["length"] = 4;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "VEHICLE SPEED";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x25;
            row["length"] = 3;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "FUEL TANK LEVEL";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x29;
            row["length"] = 4;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "LAST ENGINE SHUTDOWN";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x2A;
            row["length"] = 4;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "UNKNOWN FEATURE PRESENT";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x2C;
            row["length"] = 3;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "WIPER";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x34;
            row["length"] = 4;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "BCM TO MIC MESSAGE";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x35;
            row["length"] = 4;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "US/METRIC STATUS | SEAT-BELT: ";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x36;
            row["length"] = 4;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "UNKNOWN FEATURE PRESENT";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x3A;
            row["length"] = 4;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "INSTRUMENT CLUSTER LAMP STATES";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x3B;
            row["length"] = 4;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "SEND COMPENSATION AND CHECKSUM DATA";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x42;
            row["length"] = 4;
            row["parameterCount"] = 2;
            row["message"] = string.Empty;
            row["description"] = "THROTTLE POSITION SENSOR | CRUISE CONTROL";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x44;
            row["length"] = 4;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "FUEL USED";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x46;
            row["length"] = 4;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "REQUEST CALIBRATION DATA";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x4B;
            row["length"] = 4;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "N/S AND E/W A/D";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x4D;
            row["length"] = 4;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "UNKNOWN FEATURE PRESENT";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x50;
            row["length"] = 3;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "MIC LAMP STATUS";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x52;
            row["length"] = 4;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "SHIFT LEVER POSITION";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x54;
            row["length"] = 4;
            row["parameterCount"] = 2;
            row["message"] = string.Empty;
            row["description"] = "TRANSMISSION TEMPERATURE | ENGINE COOLANT TEMPERATURE";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x56;
            row["length"] = 4;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "REQUESTED MIL STATE - TRANSMISSION";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x6B;
            row["length"] = 4;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "COMPASS COMP. AND CHECKSUM DATA RECEIVED";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x6D;
            row["length"] = 4;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "VEHICLE IDENTIFICATION NUMBER (VIN) CHARACTER";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x75;
            row["length"] = 4;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "A/C HIGH SIDE PRESSURE";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x76;
            row["length"] = 4;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "UNKNOWN FEATURE PRESENT";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x7B;
            row["length"] = 4;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "OUTSIDE AIR TEMPERATURE";
            row["value"] = string.Empty; // Fahrenheit only
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x83;
            row["length"] = 4;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "OUTSIDE AIR TEMPERATURE";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x84;
            row["length"] = 4;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "PCM TO BCM MESSAGE: INCREMENT ODOMETER/TRIPMETER";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x89;
            row["length"] = 4;
            row["parameterCount"] = 2;
            row["message"] = string.Empty;
            row["description"] = "FUEL EFFICIENCY";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x8C;
            row["length"] = 4;
            row["parameterCount"] = 2;
            row["message"] = string.Empty;
            row["description"] = "ENGINE COOLANT TEMPERATURE | INTAKE AIR TEMPERATURE";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x8D;
            row["length"] = 4;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "UNKNOWN FEATURE PRESENT";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x8E;
            row["length"] = 4;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "STATUS 21";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x93;
            row["length"] = 4;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "SEND CALIBRATION AND VARIANCE DATA";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x94;
            row["length"] = 4;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "BCM TO MIC MESSAGE | GAUGE POSITION:";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x99;
            row["length"] = 4;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "COMPASS CALIBRATION AND VARIANCE DATA RECEIVED";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0xA4;
            row["length"] = 4;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "MIC LAMP STATE";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0xA9;
            row["length"] = 3;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "LAST ENGINE SHUTDOWN";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0xAA;
            row["length"] = 4;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "VEHICLE THEFT SECURITY STATE";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0xAC;
            row["length"] = 4;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "VEHICLE INFORMATION";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0xB1;
            row["length"] = 3;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "WARNING";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0xB2;
            row["length"] = 6;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "REQUEST  |";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0xB4;
            row["length"] = 4;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "INTERVAL PROPORTIONAL TO VEHICLE SPEED";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0xB6;
            row["length"] = 4;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "UNKNOWN FEATURE PRESENT";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0xBA;
            row["length"] = 4;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "REQUEST COMPASS CALIBRATION OR VARIANCE";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0xBE;
            row["length"] = 3;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "IGNITION SWITCH POSITION";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0xC2;
            row["length"] = 6;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "SKIM SECRET KEY";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0xC4;
            row["length"] = 4;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "DISTANCE PULSES PER 344 MS";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0xCA;
            row["length"] = 6;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "REQUEST EEPROM WRITE";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0xCB;
            row["length"] = 4;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "SEND COMPASS AND LAST OUTSIDE AIR TEMPERATURE DATA";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0xCC;
            row["length"] = 4;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "ACCUMULATED MILEAGE";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0xCD;
            row["length"] = 4;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "UNKNOWN FEATURE PRESENT";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0xCE;
            row["length"] = 6;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "VEHICLE DISTANCE / ODOMETER";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0xD3;
            row["length"] = 4;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "COMPASS DISPLAY";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0xD4;
            row["length"] = 4;
            row["parameterCount"] = 2;
            row["message"] = string.Empty;
            row["description"] = "BATTERY VOLTAGE | CALCULATED CHARGING VOLTAGE";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0xDA;
            row["length"] = 3;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "INSTRUMENT CLUSTER LAMP STATES";
            row["value"] = string.Empty;
            //0x02: TRIP ODO RESET ST: 1-YES, 0-NO
            //0x04: TRIP ODO RESET SWITCH: 1-CLOSED, 0-OPEN
            //0x08: BEEP REQUEST
            //0x10: CLUSTER ALARM
            //0x20: CLUSTER TYPE: 1-METRIC, 0-US
            //0x40: MIL LAMP
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0xDB;
            row["length"] = 4;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "COMPASS CALL DATA A/C CLUTCH ON";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0xDC;
            row["length"] = 3;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "SHIFT LEVER POSITION";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0xE4;
            row["length"] = 4;
            row["parameterCount"] = 2;
            row["message"] = string.Empty;
            row["description"] = "ENGINE SPEED | INTAKE MANIFOLD ABSOLUTE PRESSURE";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0xEC;
            row["length"] = 4;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "VEHICLE INFORMATION";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0xEE;
            row["length"] = 5;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "TRIP DISTANCE / TRIPMETER";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0xF1;
            row["length"] = 3;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "WARNING";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0xF2;
            row["length"] = 6;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "RESPONSE |";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0xF3;
            row["length"] = 4;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "SWITCH MESSAGE";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0xF5;
            row["length"] = 4;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "ENGINE LAMP CTRL";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0xF6;
            row["length"] = 4;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "UNKNOWN FEATURE PRESENT";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0xFD;
            row["length"] = 4;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "COMPASS COMP. AND TEMPERATURE DATA RECEIVED";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0xFE;
            row["length"] = 3;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "INTERIOR LAMP DIMMING";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0xFF;
            row["length"] = 1;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "CCD-BUS WAKE UP";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            #endregion

            IDList = MessageDatabase.AsEnumerable().Select(r => r.Field<ushort>("id")).ToArray();
        }

        public void UpdateHeader(string state = "enabled", string speed = null, string logic = null)
        {
            if (state != null) this.state = state;
            if (speed != null) this.speed = speed;
            if (logic != null) this.logic = logic;

            if (this.state == "enabled")
            {
                HeaderModified = HeaderEnabled.Replace("@ BAUD", "@ " + this.speed.ToUpper()).Replace("LOGIC:", "LOGIC: " + this.logic.ToUpper()).Replace("ID BYTES: ", "ID BYTES: " + (Diagnostics.UniqueIDByteList.Count + Diagnostics.B2F2IDByteList.Count).ToString());
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
            string hexBytesToInsert = string.Empty;
            string descriptionToInsert = string.Empty;
            string valueToInsert = string.Empty;
            string unitToInsert = string.Empty;
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
                payload = new byte[data.Length - 6];
                Array.Copy(data, 5, payload, 0, payload.Length); // copy payload from the input byte array (without ID and checksum)
            }

            byte ID = message[0];
            ushort modifiedID;

            if (ID == 0x94)
            {
                if (payload.Length > 1) modifiedID = (ushort)(((ID << 8) & 0xFF00) + (payload[1] & 0x0F));
                else modifiedID = (ushort)((ID << 8) & 0xFF00);
            }
            else
            {
                modifiedID = (ushort)((ID << 8) & 0xFF00);
            }

            int rowIndex = MessageDatabase.Rows.IndexOf(MessageDatabase.Rows.Find(ID)); // search ID byte among known messages and get row index

            if (rowIndex != -1) // -1 if row not found
            {
                descriptionToInsert = MessageDatabase.Rows[rowIndex]["description"].ToString();

                int minLength = Convert.ToInt32(MessageDatabase.Rows[rowIndex]["length"]);

                switch (ID)
                {
                    case 0x02: // shift lever position
                    case 0x52:
                        if (message.Length >= minLength)
                        {
                            switch (payload[0] & 0xF0) // analyze the upper 4 bits only
                            {
                                case 0x10:
                                    valueToInsert = "OVERDRIVE";
                                    break;
                                case 0x20:
                                    valueToInsert = "DRIVE";
                                    break;
                                case 0x30:
                                    valueToInsert = "REVERSE";
                                    break;
                                case 0x40:
                                    valueToInsert = "NEUTRAL";
                                    break;
                                case 0x60:
                                    valueToInsert = "NEUTRAL";
                                    break;
                                case 0x80:
                                    valueToInsert = "LOW";
                                    break;
                                case 0xC0:
                                    valueToInsert = "PARK";
                                    break;
                                default:
                                    valueToInsert = "UNDEFINED";
                                    break;
                            }

                            unitToInsert = string.Empty;
                        }
                        else
                        {
                            valueToInsert = "ERROR";
                            unitToInsert = string.Empty;
                        }
                        break;
                    case 0x0A: // send diagnostic failure data
                        valueToInsert = string.Empty;
                        unitToInsert = string.Empty;
                        break;
                    case 0x0B: // SKIM present
                        if (message.Length >= minLength)
                        {
                            if ((payload[0] == 0xFF) && (payload[1] == 0xFF)) valueToInsert = "OK";
                            else valueToInsert = "ERROR";

                            unitToInsert = string.Empty;
                        }
                        else
                        {
                            valueToInsert = "ERROR";
                            unitToInsert = string.Empty;
                        }
                        break;
                    case 0x0C: // battery voltage, oil pressure, coolant temperature, intake air temperature
                        if (message.Length >= minLength)
                        {
                            if (MainForm.units == "imperial")
                            {
                                string batteryVoltage = Math.Round(payload[0] * 0.125D, 1).ToString("0.0").Replace(",", ".");
                                string oilPressure = Math.Round(payload[1] * 0.5D, 1).ToString("0.0").Replace(",", ".");
                                string coolantTemperature = Math.Round((payload[2] * 1.8D) - 83.2D).ToString("0");
                                string intakeAirTemperature = Math.Round((payload[3] * 1.8D) - 83.2D).ToString("0");

                                descriptionToInsert = "BATTERY: " + batteryVoltage + " V | " + "OIL: " + oilPressure + " PSI | " + "COOLANT: " + coolantTemperature + " °F";
                                valueToInsert = "IAT: " + intakeAirTemperature + " °F";
                            }
                            else if (MainForm.units == "metric")
                            {
                                string batteryVoltage = Math.Round(payload[0] * 0.125D, 1).ToString("0.0").Replace(",", ".");
                                string oilPressure = Math.Round(payload[1] * 0.5D * 6.894757D, 1).ToString("0.0").Replace(",", ".");
                                string coolantTemperature = (payload[2] - 64).ToString("0");
                                string intakeAirTemperature = (payload[3] - 64).ToString("0");

                                descriptionToInsert = "BATTERY: " + batteryVoltage + " V | " + "OIL: " + oilPressure + " KPA | " + "COOLANT: " + coolantTemperature + " °C";
                                valueToInsert = "IAT: " + intakeAirTemperature + " °C";
                            }
                            unitToInsert = string.Empty;
                        }
                        else
                        {
                            descriptionToInsert = "BATTERY | OIL | COOLANT | IAT";
                            valueToInsert = "ERROR";
                            unitToInsert = string.Empty;
                        }
                        break;
                    case 0x0D: // unknown feature present
                        valueToInsert = string.Empty;
                        unitToInsert = string.Empty;
                        break;
                    case 0x10: // HVAC message
                        valueToInsert = string.Empty;
                        unitToInsert = string.Empty;
                        break;
                    case 0x11: // unknown feature present
                        valueToInsert = string.Empty;
                        unitToInsert = string.Empty;
                        break;
                    case 0x12: // request EEPROM read - compass mini-trip
                        valueToInsert = string.Empty;
                        unitToInsert = string.Empty;
                        break;
                    case 0x16: // vehicle theft security state
                        if (message.Length >= minLength)
                        {
                            switch (payload[0])
                            {
                                case 0x00:
                                    valueToInsert = "DISARMED";
                                    break;
                                case 0x01:
                                    valueToInsert = "TIMING OUT";
                                    break;
                                case 0x02:
                                    valueToInsert = "ARMED";
                                    break;
                                case 0x04:
                                    valueToInsert = "HORN AND LIGHTS";
                                    break;
                                case 0x08:
                                    valueToInsert = "LIGHTS ONLY";
                                    break;
                                case 0x10:
                                    valueToInsert = "TIMED OUT";
                                    break;
                                case 0x20:
                                    valueToInsert = "SELF DIAGS";
                                    break;
                                default:
                                    valueToInsert = "NONE";
                                    break;
                            }

                            unitToInsert = string.Empty;
                        }
                        else
                        {
                            valueToInsert = string.Empty;
                            unitToInsert = string.Empty;
                        }
                        break;
                    case 0x1B: // last OS temperature
                        valueToInsert = string.Empty;
                        unitToInsert = string.Empty;
                        break;
                    case 0x1C: // fuel level counts
                        valueToInsert = string.Empty;
                        unitToInsert = string.Empty;
                        break;
                    case 0x23: // country code
                        if (message.Length >= minLength)
                        {
                            switch (payload[0])
                            {
                                case 0x00:
                                    valueToInsert = "USA";
                                    break;
                                case 0x01:
                                    valueToInsert = "GULF COAST";
                                    break;
                                case 0x02:
                                    valueToInsert = "EUROPE";
                                    break;
                                case 0x03:
                                    valueToInsert = "JAPAN";
                                    break;
                                case 0x04:
                                    valueToInsert = "MALAYSIA";
                                    break;
                                case 0x05:
                                    valueToInsert = "INDONESIA";
                                    break;
                                case 0x06:
                                    valueToInsert = "AUSTRALIA";
                                    break;
                                case 0x07:
                                    valueToInsert = "ENGLAND";
                                    break;
                                case 0x08:
                                    valueToInsert = "VENEZUELA";
                                    break;
                                case 0x09:
                                    valueToInsert = "CANADA";
                                    break;
                                case 0x0A:
                                default:
                                    valueToInsert = "UNKNOWN";
                                    break;
                            }

                            unitToInsert = string.Empty;
                        }
                        else
                        {
                            valueToInsert = "ERROR";
                            unitToInsert = string.Empty;
                        }
                        break;
                    case 0x24: // vehicle speed
                        if (message.Length >= minLength)
                        {
                            if (MainForm.units == "imperial")
                            {
                                valueToInsert = payload[0].ToString("0");
                                unitToInsert = "MPH";
                            }
                            else if (MainForm.units == "metric")
                            {
                                valueToInsert = payload[1].ToString("0");
                                unitToInsert = "KM/H";
                            }
                        }
                        else
                        {
                            valueToInsert = "ERROR";
                            unitToInsert = string.Empty;
                        }
                        break;
                    case 0x25: // fuel tank level
                        if (message.Length >= minLength)
                        {
                            valueToInsert = Math.Round(payload[0] * 0.3922D, 1).ToString("0.0").Replace(",", ".");
                            unitToInsert = "%";
                        }
                        else
                        {
                            valueToInsert = "ERROR";
                            unitToInsert = string.Empty;
                        }
                        break;
                    case 0x29: // last engine shutdown
                        if (message.Length >= minLength)
                        {
                            if (payload[0] < 10) valueToInsert = "0";
                            valueToInsert += payload[0].ToString("0") + ":";
                            if (payload[1] < 10) valueToInsert += "0";
                            valueToInsert += payload[1].ToString("0");
                            unitToInsert = "HOUR:MINUTE";
                        }
                        else
                        {
                            valueToInsert = "ERROR";
                            unitToInsert = string.Empty;
                        }
                        break;
                    case 0x2A: // unknown feature present
                        valueToInsert = string.Empty;
                        unitToInsert = string.Empty;
                        break;
                    case 0x2C: // wiper
                        if (message.Length >= minLength)
                        {
                            switch (payload[0])
                            {
                                case 0x04:
                                    valueToInsert = "WIPERS ON";
                                    break;
                                case 0x08:
                                    valueToInsert = "WASH ON";
                                    break;
                                case 0x02:
                                    valueToInsert = "ARMED";
                                    break;
                                case 0x0C:
                                    valueToInsert = "WIPE / WASH";
                                    break;
                                case 0x20:
                                    valueToInsert = "INT WIPE";
                                    break;
                                case 0x28:
                                    valueToInsert = "INT WIPE / WASH";
                                    break;
                                default:
                                    valueToInsert = "IDLE";
                                    break;
                            }

                            unitToInsert = string.Empty;
                        }
                        else
                        {
                            valueToInsert = string.Empty;
                            unitToInsert = string.Empty;
                        }
                        break;
                    case 0x34: // BCM to MIC message
                        valueToInsert = string.Empty;
                        unitToInsert = string.Empty;
                        break;
                    case 0x35: // US/Metric status
                        if (message.Length >= minLength)
                        {
                            if (Util.IsBitSet(payload[0], 1)) valueToInsert = "METRIC";
                            else valueToInsert = "US";

                            if (Util.IsBitSet(payload[0], 2)) descriptionToInsert = "US/METRIC STATUS | SEATBELT: BUCKLED";
                            else descriptionToInsert = "US/METRIC STATUS | SEATBELT: UNBUCKLED";

                            unitToInsert = string.Empty;
                        }
                        else
                        {
                            valueToInsert = "ERROR";
                            unitToInsert = string.Empty;
                        }
                        break;
                    case 0x36: // unknown feature present
                        valueToInsert = string.Empty;
                        unitToInsert = string.Empty;
                        break;
                    case 0x3A: // instrument cluster lamp states
                        if (message.Length >= minLength)
                        {
                            switch (payload[0])
                            {
                                case 0x08:
                                    valueToInsert = "ACM LAMP";
                                    break;
                                case 0x10:
                                    valueToInsert = "AIRBAG LAMP";
                                    break;
                                case 0x80:
                                    valueToInsert = "SEATBELT LAMP";
                                    break;
                                default:
                                    valueToInsert = "UNKNOWN";
                                    break;
                            }

                            unitToInsert = string.Empty;
                        }
                        else
                        {
                            valueToInsert = string.Empty;
                            unitToInsert = string.Empty;
                        }
                        break;
                    case 0x3B: // send compensation and checksum data
                        valueToInsert = string.Empty;
                        unitToInsert = string.Empty;
                        break;
                    case 0x42: // throttle position sensor, cruise state
                        if (message.Length >= minLength)
                        {
                            valueToInsert = Math.Round(payload[0] * 0.65D).ToString("0");
                            unitToInsert = "%";
                        }
                        else
                        {
                            valueToInsert = "ERROR";
                            unitToInsert = string.Empty;
                        }
                        break;
                    case 0x44: // fuel used
                        if (message.Length >= minLength)
                        {
                            valueToInsert = ((payload[0] << 8) + payload[1]).ToString("0");
                            unitToInsert = string.Empty;
                        }
                        else
                        {
                            valueToInsert = "ERROR";
                            unitToInsert = string.Empty;
                        }
                        break;
                    case 0x46: // request calibration data
                        valueToInsert = string.Empty;
                        unitToInsert = string.Empty;
                        break;
                    case 0x4B: // N/S and E/W A/D
                        valueToInsert = string.Empty;
                        unitToInsert = string.Empty;
                        break;
                    case 0x4D: // unknown feature present
                        valueToInsert = string.Empty;
                        unitToInsert = string.Empty;
                        break;
                    case 0x50: // MIC lamp status
                        if (message.Length >= minLength)
                        {
                            if ((payload[0] & 0x01) == 0x01) valueToInsert = "AIRBAG LAMP ON";
                            else valueToInsert = "AIRBAG LAMP OFF";
                            unitToInsert = string.Empty;
                        }
                        else
                        {
                            valueToInsert = "ERROR";
                            unitToInsert = string.Empty;
                        }
                        break;
                    case 0x54: // Transmission temperature / Engine coolant temperature
                        if (message.Length >= minLength)
                        {
                            if (MainForm.units == "imperial")
                            {
                                valueToInsert = Math.Round((payload[0] * 1.8D) - 198.4D).ToString("0") + " | " + Math.Round((payload[1] * 1.8D) - 198.4D).ToString("0");
                                unitToInsert = "°F | °F";
                            }
                            else if (MainForm.units == "metric")
                            {
                                valueToInsert = (payload[0] - 128).ToString("0") + " | " + (payload[1] - 128).ToString("0");
                                unitToInsert = "°C | °C";
                            }
                        }
                        else
                        {
                            valueToInsert = "ERROR";
                            unitToInsert = string.Empty;
                        }
                        break;
                    case 0x56: // request MIL state - transmission
                        valueToInsert = string.Empty;
                        unitToInsert = string.Empty;
                        break;
                    case 0x6B: // compass compensation and checksum data received
                        valueToInsert = string.Empty;
                        unitToInsert = string.Empty;
                        break;
                    case 0x6D: // vehicle identification number (VIN) character
                        if (message.Length >= minLength)
                        {
                            if ((payload[0] > 0) && (payload[0] < 18) && (payload[1] >= 0x30 ) && (payload[1] <= 0x5A))
                            {
                                // Replace characters in the VIN string (one by one)
                                VIN = VIN.Remove(payload[0] - 1, 1).Insert(payload[0] - 1, ((char)(payload[1])).ToString());
                            }
                            valueToInsert = VIN;
                            unitToInsert = string.Empty;
                        }
                        else
                        {
                            valueToInsert = "ERROR";
                            unitToInsert = string.Empty;
                        }
                        break;
                    case 0x75: // A/C high side pressure
                        if (message.Length >= minLength)
                        {
                            if (MainForm.units == "imperial")
                            {
                                valueToInsert = Math.Round(payload[0] * 1.961D, 1).ToString("0.0").Replace(",", ".");
                                unitToInsert = "PSI";
                            }
                            else if (MainForm.units == "metric")
                            {
                                valueToInsert = Math.Round(payload[0] * 1.961D * 6.894757D, 1).ToString("0.0").Replace(",", ".");
                                unitToInsert = "KPA";
                            }
                        }
                        else
                        {
                            valueToInsert = "ERROR";
                            unitToInsert = string.Empty;
                        }
                        break;
                    case 0x76: // unknown feature present
                        valueToInsert = string.Empty;
                        unitToInsert = string.Empty;
                        break;
                    case 0x7B: // outside air temperature
                    case 0x83:
                        if (message.Length >= minLength)
                        {
                            if (MainForm.units == "imperial") // default unit for this message
                            {
                                valueToInsert = Math.Round(payload[0] - 70.0D).ToString("0");
                                unitToInsert = "°F";
                            }
                            else if (MainForm.units == "metric") // manual conversion
                            {
                                valueToInsert = Math.Round(((payload[0] - 70.0D) - 32) / 1.8D).ToString("0");
                                unitToInsert = "°C";
                            }
                        }
                        else
                        {
                            valueToInsert = "ERROR";
                            unitToInsert = string.Empty;
                        }
                        break;
                    case 0x84: // PCM to BCM message
                        valueToInsert = string.Empty;
                        unitToInsert = string.Empty;
                        break;
                    case 0x89: // Fuel efficiency
                        if (message.Length >= minLength)
                        {
                            valueToInsert = Util.ByteToHexStringSimple(new byte[1] { payload[0] }) + " MPG | " + payload[1].ToString("0") + " L/100KM";
                            unitToInsert = "-";
                        }
                        else
                        {
                            valueToInsert = "ERROR";
                            unitToInsert = string.Empty;
                        }
                        break;
                    case 0x8C:
                        if (message.Length >= minLength)
                        {
                            if (MainForm.units == "imperial")
                            {
                                valueToInsert = Math.Round((payload[0] * 1.8D) - 198.4D).ToString("0") + " | " + Math.Round((payload[1] * 1.8D) - 198.4D).ToString("0");
                                unitToInsert = "°F | °F";
                            }
                            else if (MainForm.units == "metric")
                            {
                                valueToInsert = (payload[0] - 128).ToString("0") + " | " + (payload[1] - 128).ToString("0");
                                unitToInsert = "°C | °C";
                            }
                        }
                        else
                        {
                            valueToInsert = "ERROR";
                            unitToInsert = string.Empty;
                        }
                        break;
                    case 0x8D: // unknown feature present
                        valueToInsert = string.Empty;
                        unitToInsert = string.Empty;
                        break;
                    case 0x8E: // status 21
                        valueToInsert = string.Empty;
                        unitToInsert = string.Empty;
                        break;
                    case 0x93: // send calibration and variance data
                        valueToInsert = string.Empty;
                        unitToInsert = string.Empty;
                        break;
                    case 0x94: // BCM to MIC message: gauge position
                        if (message.Length >= minLength)
                        {
                            switch (payload[1])
                            {
                                case 0x00: // fuel gauge
                                    descriptionToInsert = "MIC GAUGE POSITION: FUEL LEVEL";
                                    valueToInsert = Util.ByteToHexString(payload, 0, 1);
                                    break;
                                case 0x01: // coolant temperature gauge
                                    descriptionToInsert = "MIC GAUGE POSITION: COOLANT TEMPERATURE";
                                    valueToInsert = Util.ByteToHexString(payload, 0, 1);
                                    break;
                                case 0x02: // speedometer
                                case 0x22:
                                case 0x32:
                                    descriptionToInsert = "MIC GAUGE POSITION: SPEEDOMETER";
                                    valueToInsert = Util.ByteToHexString(payload, 0, 1);
                                    break;
                                case 0x03: // tachometer
                                case 0x23:
                                case 0x33:
                                    descriptionToInsert = "MIC GAUGE POSITION: TACHOMETER";
                                    valueToInsert = Util.ByteToHexString(payload, 0, 1);
                                    break;
                                default:
                                    descriptionToInsert = "MIC GAUGE/LAMP STATE";
                                    valueToInsert = Convert.ToString(payload[0], 2).PadLeft(8, '0');
                                    break;
                            }

                            unitToInsert = string.Empty;
                        }
                        else
                        {
                            valueToInsert = "ERROR";
                            unitToInsert = string.Empty;
                        }
                        break;
                    case 0x99: // compass calibration and variance data received
                        valueToInsert = string.Empty;
                        unitToInsert = string.Empty;
                        break;
                    case 0xA4: // status 10
                        if (message.Length >= minLength)
                        {
                            valueToInsert = string.Empty;

                            if (Util.IsBitSet(payload[0], 1)) valueToInsert += "CRUISE ";
                            if (Util.IsBitSet(payload[0], 2)) valueToInsert += "BRAKE ";
                            if (Util.IsBitSet(payload[0], 5)) valueToInsert += "MIL ";

                            unitToInsert = string.Empty;
                        }
                        else
                        {
                            valueToInsert = "ERROR";
                            unitToInsert = string.Empty;
                        }
                        break;
                    case 0xA9: // last engine shutdown
                        if (message.Length >= minLength)
                        {
                            valueToInsert = payload[0].ToString("0");
                            unitToInsert = "MINUTE";
                        }
                        else
                        {
                            valueToInsert = "ERROR";
                            unitToInsert = string.Empty;
                        }
                        break;
                    case 0xAA: // vehicle theft security state
                        if (message.Length >= minLength)
                        {
                            switch (payload[0])
                            {
                                case 0x00:
                                    valueToInsert = "DISARMED";
                                    break;
                                case 0x01:
                                    valueToInsert = "TIMING OUT";
                                    break;
                                case 0x02:
                                    valueToInsert = "ARMED";
                                    break;
                                case 0x04:
                                    valueToInsert = "HORN AND LIGHTS";
                                    break;
                                case 0x08:
                                    valueToInsert = "LIGHTS ONLY";
                                    break;
                                case 0x10:
                                    valueToInsert = "TIMED OUT";
                                    break;
                                case 0x20:
                                    valueToInsert = "SELF DIAGS";
                                    break;
                                default:
                                    valueToInsert = "NONE";
                                    break;
                            }

                            unitToInsert = string.Empty;
                        }
                        else
                        {
                            valueToInsert = string.Empty;
                            unitToInsert = string.Empty;
                        }
                        break;
                    case 0xAC: // vehicle information
                        valueToInsert = string.Empty;
                        unitToInsert = string.Empty;
                        break;
                    case 0xB1: // warning
                        if (message.Length >= minLength)
                        {
                            List<string> warningList = new List<string>();
                            warningList.Clear();

                            if (payload[0] == 0)
                            {
                                descriptionToInsert = "NO WARNING";
                                valueToInsert = string.Empty;
                                unitToInsert = string.Empty;
                            }
                            else
                            {
                                if (Util.IsBitSet(payload[0], 0)) warningList.Add("KEY IN IGNITION");
                                if (Util.IsBitSet(payload[0], 1)) warningList.Add("SEAT BELT");
                                if (Util.IsBitSet(payload[0], 2)) valueToInsert = "EXTERIOR LAMP";
                                if (Util.IsBitSet(payload[0], 4)) warningList.Add("OVERSPEED ");

                                descriptionToInsert = "WARNING: ";

                                foreach (string s in warningList)
                                {
                                    descriptionToInsert += s + " | ";
                                }

                                descriptionToInsert = descriptionToInsert.Remove(descriptionToInsert.Length - 3); // remove last "|" character
                                unitToInsert = string.Empty;
                            }
                        }
                        break;
                    case 0xB2: // request
                        if (message.Length >= minLength)
                        {
                            switch (payload[0]) // module address
                            {
                                case 0x10: // VIC - Vehicle Info Center
                                    switch (payload[1])
                                    {
                                        case 0x00: // reset
                                            descriptionToInsert = "REQUEST  | VEHICLE INFO CENTER | RESET";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                        case 0x10: // actuator tests
                                            descriptionToInsert = "REQUEST  | VIC | ACTUATOR TEST";

                                            switch (payload[2])
                                            {
                                                case 0x10:
                                                    valueToInsert = "DISPLAY";
                                                    break;
                                                default:
                                                    valueToInsert = string.Empty;
                                                    break;
                                            }

                                            unitToInsert = string.Empty;
                                            break;
                                        case 0x12: // read digital parameters
                                            descriptionToInsert = "REQUEST  | VIC | DIGITAL READ";

                                            switch (payload[2])
                                            {
                                                case 0x00:
                                                    valueToInsert = "TRANSFER CASE POSITION";
                                                    break;
                                                default:
                                                    valueToInsert = string.Empty;
                                                    break;
                                            }

                                            unitToInsert = string.Empty;
                                            break;
                                        case 0x14: // read analog parameters
                                            descriptionToInsert = "REQUEST  | VIC | ANALOG READ";

                                            switch (payload[2])
                                            {
                                                case 0x00:
                                                    valueToInsert = "WASHER LEVEL";
                                                    break;
                                                case 0x01:
                                                    valueToInsert = "COOLANT LEVEL";
                                                    break;
                                                case 0x02:
                                                    valueToInsert = "IGNITION VOLTAGE";
                                                    break;
                                                default:
                                                    valueToInsert = string.Empty;
                                                    break;
                                            }

                                            unitToInsert = string.Empty;
                                            break;
                                        case 0x16: // read fault codes
                                            descriptionToInsert = "REQUEST  | VIC | FAULT CODES";
                                            valueToInsert = "PAGE: " + Util.ByteToHexString(payload, 2, 1);
                                            unitToInsert = string.Empty;
                                            break;
                                        case 0x24: // software version
                                            descriptionToInsert = "REQUEST  | VIC | SOFTWARE VERSION";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                        case 0x40: // erase fault codes
                                            descriptionToInsert = "REQUEST  | VIC | ERASE FAULT CODES";
                                            valueToInsert = "PAGE: " + Util.ByteToHexString(payload, 2, 1);
                                            unitToInsert = string.Empty;
                                            break;
                                        default:
                                            descriptionToInsert = "REQUEST  | VIC";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                    }
                                    break;
                                case 0x18: // VTS - Vehicle Theft Security
                                case 0x1B:
                                    descriptionToInsert = "REQUEST  | VTS";
                                    valueToInsert = string.Empty;
                                    unitToInsert = string.Empty;
                                    break;
                                case 0x19: // CMT - Compass Mini-Trip
                                    switch (payload[1]) // command
                                    {
                                        case 0x00: // reset
                                            descriptionToInsert = "REQUEST  | COMPASS MINI-TRIP | RESET";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                        case 0x10: // actuator test
                                            descriptionToInsert = "REQUEST  | CMT | ACTUATOR TEST";

                                            switch (payload[2])
                                            {
                                                case 0x00: // self test
                                                    valueToInsert = "SELF TEST";
                                                    break;
                                                default:
                                                    valueToInsert = string.Empty;
                                                    break;
                                            }

                                            unitToInsert = string.Empty;
                                            break;
                                        case 0x11: // test status
                                            descriptionToInsert = "REQUEST  | CMT | ACTUATOR TEST STATUS";

                                            switch (payload[2])
                                            {
                                                case 0x00: // self test
                                                    valueToInsert = "SELF TEST";
                                                    break;
                                                default:
                                                    valueToInsert = string.Empty;
                                                    break;
                                            }

                                            unitToInsert = string.Empty;
                                            break;
                                        case 0x12: // read digital parameters
                                            descriptionToInsert = "REQUEST  | CMT | DIGITAL READ";

                                            switch (payload[2])
                                            {
                                                case 0x00:
                                                    valueToInsert = "STEP SWITCH";
                                                    break;
                                                default:
                                                    valueToInsert = string.Empty;
                                                    break;
                                            }

                                            unitToInsert = string.Empty;
                                            break;
                                        case 0x16: // read fault codes
                                            descriptionToInsert = "REQUEST  | CMT | FAULT CODES";
                                            valueToInsert = "PAGE: " + Util.ByteToHexString(payload, 2, 1);
                                            unitToInsert = string.Empty;
                                            break;
                                        case 0x20: // diagnostic data
                                            descriptionToInsert = "REQUEST  | CMT | DIAGNOSTIC DATA";

                                            switch (payload[2])
                                            {
                                                case 0x00:
                                                    valueToInsert = "TEMPERATURE";
                                                    break;
                                                default:
                                                    valueToInsert = string.Empty;
                                                    break;
                                            }

                                            unitToInsert = string.Empty;
                                            break;
                                        case 0x22: // read ROM
                                            switch (payload[2])
                                            {
                                                case 0xC2: // MIC and PCM messages received
                                                    descriptionToInsert = "REQUEST  | CMT | MIC AND PCM MESSAGES RECEIVED";
                                                    valueToInsert = string.Empty;
                                                    unitToInsert = string.Empty;
                                                    break;
                                                default:
                                                    descriptionToInsert = "REQUEST  | CMT | ROM DATA";
                                                    valueToInsert = string.Empty;
                                                    unitToInsert = string.Empty;
                                                    break;
                                            }
                                            break;
                                        case 0x24: // software version
                                            switch (payload[2])
                                            {
                                                case 0x00:
                                                    descriptionToInsert = "REQUEST  | CMT | SOFTWARE VERSION";
                                                    valueToInsert = string.Empty;
                                                    unitToInsert = string.Empty;
                                                    break;
                                                case 0x01:
                                                    descriptionToInsert = "REQUEST  | CMT | EEPROM VERSION";
                                                    valueToInsert = string.Empty;
                                                    unitToInsert = string.Empty;
                                                    break;
                                            }
                                            break;
                                        case 0x40: // erase fault codes
                                            descriptionToInsert = "REQUEST  | CMT | ERASE FAULT CODES";
                                            valueToInsert = "PAGE: " + Util.ByteToHexString(payload, 2, 1);
                                            unitToInsert = string.Empty;
                                            break;
                                        default:
                                            descriptionToInsert = "REQUEST  | CMT";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                    }
                                    break;
                                case 0x1E: // ACM - Airbag Control Module
                                    switch (payload[1]) // command
                                    {
                                        case 0x00: // reset
                                            descriptionToInsert = "REQUEST  | AIRBAG CONTROL MODULE | RESET";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                        case 0x16: // read fault codes
                                            descriptionToInsert = "REQUEST  | ACM | FAULT CODES";
                                            valueToInsert = "PAGE: " + Util.ByteToHexString(payload, 2, 1);
                                            unitToInsert = string.Empty;
                                            break;
                                        case 0x24: // software version
                                            descriptionToInsert = "REQUEST  | ACM | SOFTWARE VERSION";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                        case 0x40: // erase fault codes
                                            descriptionToInsert = "REQUEST  | ACM | ERASE FAULT CODES";
                                            valueToInsert = "PAGE: " + Util.ByteToHexString(payload, 2, 1);
                                            unitToInsert = string.Empty;
                                            break;
                                        default:
                                            descriptionToInsert = "REQUEST  | ACM";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                    }
                                    break;
                                case 0x20: // BCM - Body Control Module | BCM JA PREM VTSS; sc: Body; 0x1012
                                    switch (payload[1]) // command
                                    {
                                        case 0x00: // reset
                                            descriptionToInsert = "REQUEST  | BODY CONTROL MODULE | RESET";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                        case 0x10: // actuator tests
                                            descriptionToInsert = "REQUEST  | BCM | ACTUATOR TEST";

                                            switch (payload[2])
                                            {
                                                case 0x00:
                                                    switch (payload[3])
                                                    {
                                                        case 0x08:
                                                            valueToInsert = "CHIME";
                                                            break;
                                                        case 0x20:
                                                            valueToInsert = "COURTESY LAMPS";
                                                            break;
                                                        default:
                                                            valueToInsert = "UNKNOWN";
                                                            break;
                                                    }
                                                    break;
                                                case 0x01:
                                                    switch (payload[3])
                                                    {
                                                        case 0x04:
                                                            valueToInsert = "HEADLAMP RELAY";
                                                            break;
                                                        case 0x08:
                                                            valueToInsert = "HORN RELAY";
                                                            break;
                                                        case 0x10:
                                                            valueToInsert = "DOOR LOCK";
                                                            break;
                                                        case 0x20:
                                                            valueToInsert = "DOOR UNLOCK";
                                                            break;
                                                        case 0x40:
                                                            valueToInsert = "DR DOOR UNLOCK";
                                                            break;
                                                        case 0x80:
                                                            valueToInsert = "EBL RELAY";
                                                            break;
                                                        default:
                                                            valueToInsert = "UNKNOWN";
                                                            break;
                                                    }
                                                    break;
                                                case 0x02:
                                                    switch (payload[3])
                                                    {
                                                        case 0x20:
                                                            valueToInsert = "VTSS LAMP";
                                                            break;
                                                        case 0x40:
                                                            valueToInsert = "WIPERS LOW";
                                                            break;
                                                        case 0xC0:
                                                            valueToInsert = "WIPERS HIGH";
                                                            break;
                                                        default:
                                                            valueToInsert = "UNKNOWN";
                                                            break;
                                                    }
                                                    break;
                                                case 0x03:
                                                    switch (payload[3])
                                                    {
                                                        default:
                                                            valueToInsert = "UNKNOWN";
                                                            break;
                                                    }
                                                    break;
                                                case 0x04:
                                                    switch (payload[3])
                                                    {
                                                        case 0x00:
                                                            valueToInsert = "RECAL ATC";
                                                            break;
                                                        default:
                                                            valueToInsert = "UNKNOWN";
                                                            break;
                                                    }
                                                    break;
                                                case 0x05:
                                                    switch (payload[3])
                                                    {
                                                        default:
                                                            valueToInsert = "UNKNOWN";
                                                            break;
                                                    }
                                                    break;
                                                case 0x06:
                                                    switch (payload[3])
                                                    {
                                                        default:
                                                            valueToInsert = "UNKNOWN";
                                                            break;
                                                    }
                                                    break;
                                                case 0x07:
                                                    switch (payload[3])
                                                    {
                                                        default:
                                                            valueToInsert = "UNKNOWN";
                                                            break;
                                                    }
                                                    break;
                                                case 0x09:
                                                    switch (payload[3])
                                                    {
                                                        default:
                                                            valueToInsert = "UNKNOWN";
                                                            break;
                                                    }
                                                    break;
                                                case 0x0A:
                                                    switch (payload[3])
                                                    {
                                                        case 0x00:
                                                            valueToInsert = "ENABLE VTSS";
                                                            break;
                                                        default:
                                                            valueToInsert = "UNKNOWN";
                                                            break;
                                                    }
                                                    break;
                                                case 0x0B:
                                                    switch (payload[3])
                                                    {
                                                        default:
                                                            valueToInsert = "UNKNOWN";
                                                            break;
                                                    }
                                                    break;
                                                case 0x0C:
                                                    switch (payload[3])
                                                    {
                                                        default:
                                                            valueToInsert = "UNKNOWN";
                                                            break;
                                                    }
                                                    break;
                                                case 0x0D:
                                                    switch (payload[3])
                                                    {
                                                        case 0x10:
                                                            valueToInsert = "ENABLE DOOR LOCK";
                                                            break;
                                                        default:
                                                            valueToInsert = "UNKNOWN";
                                                            break;
                                                    }
                                                    break;
                                                case 0x0E:
                                                    switch (payload[3])
                                                    {
                                                        case 0x10:
                                                            valueToInsert = "DISABLE DOOR LOCK";
                                                            break;
                                                        default:
                                                            valueToInsert = "UNKNOWN";
                                                            break;
                                                    }
                                                    break;
                                                default:
                                                    valueToInsert = "UNKNOWN";
                                                    break;

                                            }

                                            unitToInsert = string.Empty;
                                            break;
                                        case 0x12: // read digital parameters
                                            descriptionToInsert = "REQUEST  | BCM | DIGITAL READ";
                                            valueToInsert = Util.ByteToHexString(payload, 2, 1);
                                            unitToInsert = string.Empty;
                                            break;
                                        case 0x14: // read analog parameters
                                            switch (payload[2])
                                            {
                                                case 0x00:
                                                    descriptionToInsert = "REQUEST  | BCM | ANALOG READ: PASSENGER DOOR DISARM";
                                                    break;
                                                case 0x01:
                                                    descriptionToInsert = "REQUEST  | BCM | ANALOG READ: PANEL LAMPS";
                                                    break;
                                                case 0x02:
                                                    descriptionToInsert = "REQUEST  | BCM | ANALOG READ: DRDOOR DISARM";
                                                    break;
                                                case 0x03:
                                                    descriptionToInsert = "REQUEST  | BCM | ANALOG READ: HVAC CONTROL HEAD VOLTAGE";
                                                    break;
                                                case 0x04:
                                                    descriptionToInsert = "REQUEST  | BCM | ANALOG READ: CONVERT SELECT";
                                                    break;
                                                case 0x05:
                                                    descriptionToInsert = "REQUEST  | BCM | ANALOG READ: MODE DOOR";
                                                    break;
                                                case 0x06:
                                                    descriptionToInsert = "REQUEST  | BCM | ANALOG READ: DOOR STALL";
                                                    break;
                                                case 0x07:
                                                    descriptionToInsert = "REQUEST  | BCM | ANALOG READ: A/C SWITCH";
                                                    break;
                                                case 0x08:
                                                    descriptionToInsert = "REQUEST  | BCM | ANALOG READ: DOOR LOCK SWITCH VOLTAGE";
                                                    break;
                                                case 0x09:
                                                    descriptionToInsert = "REQUEST  | BCM | ANALOG READ: BATTERY VOLTAGE";
                                                    break;
                                                case 0x0B:
                                                    descriptionToInsert = "REQUEST  | BCM | ANALOG READ: FUEL LEVEL";
                                                    break;
                                                case 0x0C:
                                                    descriptionToInsert = "REQUEST  | BCM | ANALOG READ: EVAP TEMP VOLTAGE";
                                                    break;
                                                case 0x0A:
                                                    descriptionToInsert = "REQUEST  | BCM | ANALOG READ: IGNITION VOLTAGE";
                                                    break;
                                                case 0x0D:
                                                    descriptionToInsert = "REQUEST  | BCM | ANALOG READ: INTERMITTENT WIPER VOLTAGE";
                                                    break;
                                                default:
                                                    descriptionToInsert = "REQUEST  | BCM | ANALOG READ";
                                                    break;
                                            }
                                            
                                            valueToInsert = Util.ByteToHexString(payload, 2, 1);
                                            unitToInsert = string.Empty;
                                            break;
                                        case 0x16: // read fault codes
                                            descriptionToInsert = "REQUEST  | BCM | FAULT CODES";
                                            valueToInsert = "PAGE: " + Util.ByteToHexString(payload, 2, 1);
                                            unitToInsert = string.Empty;
                                            break;
                                        case 0x22: // read ROM
                                            descriptionToInsert = "REQUEST  | BCM | ROM DATA";
                                            int address = (payload[2] << 8) + payload[3];
                                            int addressNext = address + 1;
                                            byte[] addressNextArray = { (byte)((addressNext >> 8) & 0xFF), (byte)(addressNext & 0xFF) };
                                            valueToInsert = "OFFSET: " + Util.ByteToHexString(message, 3, 2) + " | " + Util.ByteToHexStringSimple(addressNextArray);
                                            unitToInsert = string.Empty;
                                            break;
                                        case 0x24: // read module id
                                            descriptionToInsert = "REQUEST  | BCM | MODULE ID";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                        case 0x2A: // read vehicle identification number (vin)
                                            descriptionToInsert = "REQUEST  | BCM | READ VIN";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                        case 0x2C: // write vehicle identification number (vin)
                                            descriptionToInsert = "REQUEST  | BCM | WRITE VIN";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                        case 0x40: // erase fault codes
                                            descriptionToInsert = "REQUEST  | BCM | ERASE FAULT CODES";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                        case 0xB0: // write settings
                                            descriptionToInsert = "REQUEST  | BCM | WRITE SETTINGS";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                        case 0xB1: // read settings
                                            descriptionToInsert = "REQUEST  | BCM | READ SETTINGS";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                        default:
                                            break;
                                    }
                                    break;
                                case 0x22: // MIC - Mechanical Instrument Cluster
                                case 0x60:
                                    switch (payload[1]) // command
                                    {
                                        case 0x00: // reset
                                            descriptionToInsert = "REQUEST  | MECHANICAL INSTRUMENT CLUSTER | RESET";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                        case 0x10: // actuator test
                                            descriptionToInsert = "REQUEST  | MIC | ACTUATOR TEST";

                                            switch (payload[2])
                                            {
                                                case 0x00:
                                                    valueToInsert = "ALL GAUGES";
                                                    break;
                                                case 0x01:
                                                    valueToInsert = "ALL LAMPS";
                                                    break;
                                                case 0x02:
                                                    valueToInsert = "ODO/TRIP/PRND3L";
                                                    break;
                                                case 0x03:
                                                    valueToInsert = "PRND3L SEGMENTS";
                                                    break;
                                                default:
                                                    valueToInsert = "UNDEFINED";
                                                    break;
                                            }

                                            unitToInsert = string.Empty;
                                            break;
                                        case 0x12: // read digital parameters
                                            descriptionToInsert = "REQUEST  | MIC | DIGITAL READ";

                                            switch (payload[2])
                                            {
                                                case 0x00:
                                                    valueToInsert = "ALL SWITCHES";
                                                    break;
                                                default:
                                                    valueToInsert = string.Empty;
                                                    break;
                                            }
                                            
                                            unitToInsert = string.Empty;
                                            break;
                                        case 0x16: // read fault codes
                                            descriptionToInsert = "REQUEST  | MIC | FAULT CODES";
                                            valueToInsert = "PAGE: " + Util.ByteToHexString(payload, 2, 1);
                                            unitToInsert = string.Empty;
                                            break;
                                        case 0x24: // software version
                                            descriptionToInsert = "REQUEST  | MIC | SOFTWARE VERSION";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                        case 0x40: // erase fault codes
                                            descriptionToInsert = "REQUEST  | MIC | ERASE FAULT CODES";
                                            valueToInsert = "PAGE: " + Util.ByteToHexString(payload, 2, 1);
                                            unitToInsert = string.Empty;
                                            break;
                                        case 0xE0: // self test
                                            descriptionToInsert = "REQUEST  | MIC | SELF TEST";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                        default:
                                            descriptionToInsert = "REQUEST  | MIC";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                    }
                                    break;
                                case 0x41: // TCM - Transmission Control Module
                                case 0x42:
                                    switch (payload[1])
                                    {
                                        case 0x00: // reset
                                            descriptionToInsert = "REQUEST  | TRANSMISSION CONTROL MODULE | RESET";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                        default:
                                            descriptionToInsert = "REQUEST  | TCM";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                    }
                                    break;
                                case 0x43: // ABS - Antilock Brake System
                                    switch (payload[1])
                                    {
                                        case 0x00: // reset
                                            descriptionToInsert = "REQUEST  | ANTILOCK BRAKE SYSTEM | RESET";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                        default:
                                            descriptionToInsert = "REQUEST  | ABS";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                    }
                                    break;
                                case 0x50: // HVAC - Heat Vent Air Conditioning
                                    switch (payload[1])
                                    {
                                        case 0x00: // reset
                                            descriptionToInsert = "REQUEST  | HVAC | RESET";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                        default:
                                            descriptionToInsert = "REQUEST  | HVAC";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                    }
                                    break;
                                case 0x80: // DDM - Driver Door Module
                                    switch (payload[1])
                                    {
                                        case 0x00: // reset
                                            descriptionToInsert = "REQUEST  | DRIVER DOOR MODULE | RESET";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                        default:
                                            descriptionToInsert = "REQUEST  | DDM";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                    }
                                    break;
                                case 0x81: // PDM - Passenger Door Module
                                    switch (payload[1])
                                    {
                                        case 0x00: // reset
                                            descriptionToInsert = "REQUEST  | PASSENGER DOOR MODULE | RESET";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                        default:
                                            descriptionToInsert = "REQUEST  | PDM";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                    }
                                    break;
                                case 0x82: // MSM - Memory Seat Module
                                    switch (payload[1])
                                    {
                                        case 0x00: // reset
                                            descriptionToInsert = "REQUEST  | MEMORY SEAT MODULE | RESET";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                        default:
                                            descriptionToInsert = "REQUEST  | MSM";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                    }
                                    break;
                                case 0x96: // ASM - Audio System Module
                                    switch (payload[1])
                                    {
                                        case 0x00: // reset
                                            descriptionToInsert = "REQUEST  | AUDIO SYSTEM MODULE | RESET";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                        default:
                                            descriptionToInsert = "REQUEST  | ASM";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                    }
                                    break;
                                case 0xC0: // SKIM - Sentry Key Immobilizer Module
                                    switch (payload[1])
                                    {
                                        case 0x00: // reset
                                            descriptionToInsert = "REQUEST  | SKIM | RESET";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                        default:
                                            descriptionToInsert = "REQUEST  | SKIM";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                    }
                                    break;
                                case 0xFF: // ALL modules present on the CCD-bus
                                    switch (payload[1]) // command
                                    {
                                        case 0x00: // reset
                                            descriptionToInsert = "REQUEST  | RESET ALL CCD-BUS MODULES";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                        default:
                                            descriptionToInsert = "REQUEST  | ALL CCD-BUS MODULES";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                    }
                                    break;
                                default:
                                    descriptionToInsert = "REQUEST  |";
                                    valueToInsert = string.Empty;
                                    unitToInsert = string.Empty;
                                    break;
                            }
                        }
                        else
                        {
                            descriptionToInsert = "REQUEST  |";
                            valueToInsert = "ERROR";
                            unitToInsert = string.Empty;
                        }

                        if (MainForm.includeTimestampInLogFiles)
                        {
                            TimeSpan ElapsedTime = TimeSpan.FromMilliseconds(timestamp[0] << 24 | timestamp[1] << 16 | timestamp[2] << 8 | timestamp[3]);
                            DateTime Timestamp = DateTime.Today.Add(ElapsedTime);
                            string TimestampString = Timestamp.ToString("HH:mm:ss.fff") + " ";
                            File.AppendAllText(MainForm.CCDB2F2LogFilename, TimestampString); // no newline is appended!
                        }

                        File.AppendAllText(MainForm.CCDB2F2LogFilename, Util.ByteToHexStringSimple(message) + Environment.NewLine);
                        break;
                    case 0xB4: // interval proportional to vehicle speed
                        valueToInsert = string.Empty;
                        unitToInsert = string.Empty;
                        break;
                    case 0xB6: // unknown feature present
                        valueToInsert = string.Empty;
                        unitToInsert = string.Empty;
                        break;
                    case 0xBA: // request compass calibration or variance
                        valueToInsert = string.Empty;
                        unitToInsert = string.Empty;
                        break;
                    case 0xBE: // status 26 - ignition switch position
                        if (message.Length >= minLength)
                        {
                            if (Util.IsBitSet(payload[0], 4)) valueToInsert = "ON";
                            else valueToInsert = "OFF";
                            unitToInsert = string.Empty;
                        }
                        else
                        {
                            valueToInsert = "ERROR";
                            unitToInsert = string.Empty;
                        }
                        break;
                    case 0xC2: // SKIM secret key
                        if (message.Length >= minLength)
                        {
                            valueToInsert = Util.ByteToHexStringSimple(payload);
                            unitToInsert = string.Empty;
                        }
                        else
                        {
                            valueToInsert = "ERROR";
                            unitToInsert = string.Empty;
                        }
                        break;
                    case 0xC4: // distance pulses per 344 ms
                        valueToInsert = string.Empty;
                        unitToInsert = string.Empty;
                        break;
                    case 0xCA: // request EEPROM write
                        valueToInsert = string.Empty;
                        unitToInsert = string.Empty;
                        break;
                    case 0xCB: // send compass and last outside air temperature data
                        valueToInsert = string.Empty;
                        unitToInsert = string.Empty;
                        break;
                    case 0xCC:
                        if (message.Length >= minLength)
                        {
                            valueToInsert = Util.ByteToHexStringSimple(payload);
                            unitToInsert = string.Empty;
                        }
                        else
                        {
                            valueToInsert = "ERROR";
                            unitToInsert = string.Empty;
                        }
                        break;
                    case 0xCD: // unknown feature present
                        valueToInsert = string.Empty;
                        unitToInsert = string.Empty;
                        break;
                    case 0xCE: // vehicle distance / odometer
                        if (message.Length >= minLength)
                        {
                            if (MainForm.units == "imperial")
                            {
                                valueToInsert = Math.Round((UInt32)(payload[0] << 24 | payload[1] << 16 | payload[2] << 8 | payload[3]) * 0.000125D, 3).ToString("0.000").Replace(",", ".");
                                unitToInsert = "MILE";
                            }
                            else if (MainForm.units == "metric")
                            {
                                valueToInsert = Math.Round((UInt32)(payload[0] << 24 | payload[1] << 16 | payload[2] << 8 | payload[3]) * 0.000125D * 1.609334138D, 3).ToString("0.000").Replace(",", ".");
                                unitToInsert = "KILOMETER";
                            }
                        }
                        else
                        {
                            valueToInsert = "ERROR";
                            unitToInsert = string.Empty;
                        }
                        break;
                    case 0xD3: // compass display
                        valueToInsert = string.Empty;
                        unitToInsert = string.Empty;
                        break;
                    case 0xD4: // battery voltage / calculated charging voltage
                        if (message.Length >= minLength)
                        {
                            valueToInsert = Math.Round(payload[0] * 0.0592D, 1).ToString("0.0").Replace(",", ".") + " | " + Math.Round(payload[1] * 0.0592D, 1).ToString("0.0").Replace(",", ".");
                            unitToInsert = "V | V";
                        }
                        else
                        {
                            valueToInsert = "ERROR";
                            unitToInsert = string.Empty;
                        }
                        break;
                    case 0xDA: // instrument cluster lamp states
                        if (message.Length >= minLength)
                        {
                            if ((payload[0] & 0x40) == 0x40) valueToInsert = "MIL LAMP ON";
                            else valueToInsert = "MIL LAMP OFF";
                            unitToInsert = string.Empty;
                        }
                        else
                        {
                            valueToInsert = "ERROR";
                            unitToInsert = string.Empty;
                        }
                        break;
                    case 0xDB: // compass calibration data / A/C clutch on
                        valueToInsert = string.Empty;
                        unitToInsert = string.Empty;
                        break;
                    case 0xDC: // shift lever position
                        if (message.Length >= minLength)
                        {
                            valueToInsert = string.Empty;

                            if (Util.IsBitSet(payload[0], 0)) valueToInsert += "NEUTRAL ";
                            else if (Util.IsBitSet(payload[0], 1)) valueToInsert += "REVERSE ";
                            else if(Util.IsBitSet(payload[0], 2)) valueToInsert += "1 ";
                            else if(Util.IsBitSet(payload[0], 3)) valueToInsert += "2 ";
                            else if(Util.IsBitSet(payload[0], 4)) valueToInsert += "3 ";
                            else if(Util.IsBitSet(payload[0], 5)) valueToInsert += "4 ";
                            else valueToInsert += "N/A ";

                            switch ((payload[0] >> 6) & 0x03)
                            {
                                case 1:
                                    valueToInsert += "| LOCK: PART";
                                    break;
                                case 2:
                                    valueToInsert += "| LOCK: FULL";
                                    break;
                            }

                            unitToInsert = string.Empty;
                        }
                        else
                        {
                            valueToInsert = "ERROR";
                            unitToInsert = string.Empty;
                        }
                        break;
                    case 0xE4: // engine speed (RPM) / intake manifold absolute pressure (MAP)
                        if (message.Length >= minLength)
                        {
                            if (MainForm.units == "imperial")
                            {
                                valueToInsert = (payload[0] * 32D).ToString("0") + " | " + Math.Round(payload[1] * 0.059756D, 1).ToString("0.0").Replace(",", ".");
                                unitToInsert = "RPM | PSI";
                            }
                            else if (MainForm.units == "metric")
                            {
                                valueToInsert = (payload[0] * 32D).ToString("0") + " | " + Math.Round(payload[1] * 0.059756D * 6.894757D, 1).ToString("0.0").Replace(",", ".");
                                unitToInsert = "RPM | KPA";
                            }
                        }
                        else
                        {
                            valueToInsert = "ERROR";
                            unitToInsert = string.Empty;
                        }
                        break;
                    case 0xEC: // vehicle information, limp states in first byte, others (fuel type) in second byte
                        if (message.Length >= minLength)
                        {
                            List<string> limpStates = new List<string>();
                            limpStates.Clear();

                            if (Util.IsBitSet(payload[0], 0)) limpStates.Add("ECT");
                            if (Util.IsBitSet(payload[0], 1)) limpStates.Add("TPS");
                            if (Util.IsBitSet(payload[0], 2)) limpStates.Add("CHARGING");
                            if (Util.IsBitSet(payload[0], 4)) limpStates.Add("A/C PRESSURE");
                            if (Util.IsBitSet(payload[0], 6)) limpStates.Add("IAT");

                            if (limpStates.Count > 0)
                            {
                                descriptionToInsert = "LIMP: ";

                                foreach (string s in limpStates)
                                {
                                    descriptionToInsert += s + " | ";
                                }

                                descriptionToInsert = descriptionToInsert.Remove(descriptionToInsert.Length - 3); // remove last "|" character
                            }
                            else
                            {
                                descriptionToInsert = "VEHICLE INFORMATION | NO LIMP STATE";
                            }

                            switch (payload[1] & 0x1F) // analyze the lower 5 bits only
                            {
                                case 0x00:
                                    valueToInsert = "FUEL: CNG";
                                    break;
                                case 0x04:
                                    valueToInsert = "FUEL: NO LEAD";
                                    break;
                                case 0x08:
                                    valueToInsert = "FUEL: LEADED FUEL";
                                    break;
                                case 0x0C:
                                    valueToInsert = "FUEL: FLEX";
                                    break;
                                case 0x10:
                                    valueToInsert = "FUEL: DIESEL";
                                    break;
                                default:
                                    valueToInsert = "FUEL: UNKNOWN";
                                    break;
                            }

                            unitToInsert = string.Empty;
                        }
                        else
                        {
                            valueToInsert = "ERROR";
                            unitToInsert = string.Empty;
                        }
                        break;
                    case 0xEE: // trip distance, tripmeter
                        if (message.Length >= minLength)
                        {
                            if (MainForm.units == "imperial")
                            {
                                valueToInsert = Math.Round((UInt32)(payload[0] << 16 | payload[1] << 8 | payload[2]) * 0.016D, 3).ToString("0.000").Replace(",", ".");
                                unitToInsert = "MILE";
                            }
                            else if (MainForm.units == "metric")
                            {
                                valueToInsert = Math.Round((UInt32)(payload[0] << 16 | payload[1] << 8 | payload[2]) * 0.016D * 1.609334138D, 3).ToString("0.000").Replace(",", ".");
                                unitToInsert = "KILOMETER";
                            }
                        }
                        else
                        {
                            valueToInsert = "ERROR";
                            unitToInsert = string.Empty;
                        }
                        break;
                    case 0xF1: // warning
                        if (message.Length >= minLength)
                        {
                            List<string> warningList = new List<string>();
                            warningList.Clear();

                            if (payload[0] == 0)
                            {
                                descriptionToInsert = "NO WARNING";
                                valueToInsert = string.Empty;
                                unitToInsert = string.Empty;
                            }
                            else
                            {
                                if (Util.IsBitSet(payload[0], 0)) warningList.Add("LOW FUEL");
                                if (Util.IsBitSet(payload[0], 1)) warningList.Add("LOW OIL");
                                if (Util.IsBitSet(payload[0], 2)) warningList.Add("HI TEMP");
                                if (Util.IsBitSet(payload[0], 3)) valueToInsert = "CRITICAL TEMP";
                                if (Util.IsBitSet(payload[0], 4)) warningList.Add("BRAKE PRESS");

                                descriptionToInsert = "WARNING: ";

                                foreach (string s in warningList)
                                {
                                    descriptionToInsert += s + " | ";
                                }

                                descriptionToInsert = descriptionToInsert.Remove(descriptionToInsert.Length - 3); // remove last "|" character
                                unitToInsert = string.Empty;
                            }
                        }
                        else
                        {
                            valueToInsert = "ERROR";
                            unitToInsert = string.Empty;
                        }
                        break;
                    case 0xF2: // response
                        if (message.Length >= minLength)
                        {
                            switch (payload[0]) // module address
                            {
                                case 0x10: // VIC - Vehicle Info Center
                                    switch (payload[1])
                                    {
                                        case 0x00: // reset
                                            descriptionToInsert = "RESPONSE | VEHICLE INFO CENTER | RESET COMPLETE";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                        case 0x10: // actuator tests
                                            switch (payload[2])
                                            {
                                                case 0x10:
                                                    descriptionToInsert = "RESPONSE | VIC | DISPLAY TEST";
                                                    break;
                                                default:
                                                    descriptionToInsert = "RESPONSE | VIC | ACTUATOR TEST";
                                                    break;
                                            }

                                            valueToInsert = "RUNNING";
                                            unitToInsert = string.Empty;
                                            break;
                                        case 0x12: // read digital parameters
                                            switch (payload[2])
                                            {
                                                case 0x00:
                                                    descriptionToInsert = "RESPONSE | VIC | TRANSFER CASE POSITION";

                                                    switch (payload[3] & 0x1F)
                                                    {
                                                        case 0x06:
                                                            unitToInsert = "4WD LO";
                                                            break;
                                                        case 0x07:
                                                            unitToInsert = "ALL 4WD";
                                                            break;
                                                        case 0x0B:
                                                            unitToInsert = "PART 4WD";
                                                            break;
                                                        case 0x0D:
                                                            unitToInsert = "FULL 4WD";
                                                            break;
                                                        case 0x0F:
                                                            unitToInsert = "2WD";
                                                            break;
                                                        case 0x1F:
                                                            unitToInsert = "NEUTRAL";
                                                            break;
                                                        default:
                                                            unitToInsert = "UNDEFINED";
                                                            break;
                                                    }

                                                    List<string> flags = new List<string>();
                                                    flags.Clear();

                                                    if (Util.IsBitClear(payload[3], 5)) flags.Add("OUTAGE");
                                                    if (Util.IsBitClear(payload[3], 6)) flags.Add("TURN");

                                                    if (flags.Count > 0)
                                                    {
                                                        foreach (string s in flags)
                                                        {
                                                            valueToInsert += s + " | ";
                                                        }

                                                        valueToInsert = valueToInsert.Remove(valueToInsert.Length - 3); // remove last "|" character
                                                        valueToInsert += " LAMP";
                                                    }
                                                    else
                                                    {
                                                        valueToInsert = string.Empty;
                                                    }
                                                    
                                                    break;
                                                default:
                                                    descriptionToInsert = "RESPONSE | VIC | DIGITAL READ";
                                                    valueToInsert = string.Empty;
                                                    unitToInsert = string.Empty;
                                                    break;
                                            }
                                            break;
                                        case 0x14: // read analog parameters
                                            switch (payload[2])
                                            {
                                                case 0x00:
                                                    descriptionToInsert = "RESPONSE | VIC | WASHER LEVEL SENSOR VOLTAGE";
                                                    valueToInsert = Math.Round(payload[3] * 0.019607843, 3).ToString("0.000").Replace(",", ".");
                                                    unitToInsert = "V";
                                                    break;
                                                case 0x01:
                                                    descriptionToInsert = "RESPONSE | VIC | COOLANT LEVEL SENSOR VOLTAGE";
                                                    valueToInsert = Math.Round(payload[3] * 0.019607843, 3).ToString("0.000").Replace(",", ".");
                                                    unitToInsert = "V";
                                                    break;
                                                case 0x02:
                                                    descriptionToInsert = "RESPONSE | VIC | IGNITION VOLTAGE";
                                                    valueToInsert = Math.Round(payload[3] * 0.099, 3).ToString("0.000").Replace(",", ".");
                                                    unitToInsert = "V";
                                                    break;
                                                default:
                                                    descriptionToInsert = "RESPONSE | VIC | ANALOG READ";
                                                    valueToInsert = Util.ByteToHexString(payload, 3, 1);
                                                    unitToInsert = "HEX";
                                                    break;
                                            }
                                            break;
                                        case 0x16: // read fault codes
                                            descriptionToInsert = "RESPONSE | VIC | FAULT CODES";
                                            if (payload[3] == 0x00) valueToInsert = "NO FAULT CODE";
                                            else valueToInsert = "CODE: " + Util.ByteToHexString(payload, 3, 1);
                                            unitToInsert = string.Empty;
                                            break;
                                        case 0x24: // software version
                                            descriptionToInsert = "RESPONSE | VIC | SOFTWARE VERSION";
                                            valueToInsert = Util.ByteToHexString(payload, 2, 2);
                                            unitToInsert = string.Empty;
                                            break;
                                        case 0x40: // erase fault codes
                                            descriptionToInsert = "RESPONSE | VIC | ERASE FAULT CODES";
                                            if (payload[3] == 0x00) valueToInsert = "ERASED";
                                            else valueToInsert = "FAILED";
                                            unitToInsert = string.Empty;
                                            break;
                                        case 0xFF: // command error
                                            descriptionToInsert = "RESPONSE | VIC | COMMAND ERROR";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                        default:
                                            descriptionToInsert = "RESPONSE | VIC";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                    }
                                    break;
                                case 0x18: // VTS - Vehicle Theft Security
                                case 0x1B:
                                    descriptionToInsert = "RESPONSE | VTS";
                                    valueToInsert = string.Empty;
                                    unitToInsert = string.Empty;
                                    break;
                                case 0x19: // CMT - Compass Mini-Trip
                                    switch (payload[1]) // command
                                    {
                                        case 0x00: // reset
                                            descriptionToInsert = "RESPONSE | COMPASS MINI-TRIP | RESET COMPLETE";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                        case 0x10: // actuator test
                                            switch (payload[2])
                                            {
                                                case 0x00: // self test
                                                    descriptionToInsert = "RESPONSE | CMT | SELF TEST";
                                                    if (payload[3] == 0x01) valueToInsert = "RUNNING";
                                                    else valueToInsert = "DENIED";
                                                    break;
                                                default:
                                                    descriptionToInsert = "RESPONSE | CMT | ACTUATOR TEST";
                                                    valueToInsert = string.Empty;
                                                    break;
                                            }

                                            unitToInsert = string.Empty;
                                            break;
                                        case 0x11: // test status
                                            descriptionToInsert = "RESPONSE | CMT | ACTUATOR TEST STATUS";

                                            switch (payload[2])
                                            {
                                                case 0x00: // self test
                                                    if (payload[3] == 0x01) valueToInsert = "RUNNING";
                                                    else valueToInsert = "DENIED";
                                                    break;
                                                default:
                                                    valueToInsert = string.Empty;
                                                    break;
                                            }

                                            unitToInsert = string.Empty;
                                            break;
                                        case 0x12: // read digital parameters
                                            switch (payload[2])
                                            {
                                                case 0x00:
                                                    descriptionToInsert = "RESPONSE | CMT | STEP SWITCH";

                                                    if (Util.IsBitSet(payload[3], 4)) valueToInsert = "PRESSED";
                                                    else valueToInsert = "RELEASED";

                                                    unitToInsert = string.Empty;
                                                    break;
                                                default:
                                                    descriptionToInsert = "RESPONSE | CMT | DIGITAL READ";
                                                    valueToInsert = string.Empty;
                                                    unitToInsert = string.Empty;
                                                    break;
                                            }
                                            break;
                                        case 0x16: // read fault codes
                                            descriptionToInsert = "RESPONSE | CMT | FAULT CODES";
                                            if (payload[3] == 0x00) valueToInsert = "NO FAULT CODE";
                                            else valueToInsert = "CODE: " + Util.ByteToHexString(payload, 3, 1);
                                            unitToInsert = string.Empty;
                                            break;
                                        case 0x20: // diagnostic data
                                            switch (payload[2])
                                            {
                                                case 0x00:
                                                    descriptionToInsert = "RESPONSE | CMT | TEMPERATURE";

                                                    string temperature = string.Empty;

                                                    if (MainForm.units == "imperial")
                                                    {
                                                        temperature = (payload[3] - 40).ToString("0");
                                                        unitToInsert = "°F";
                                                    }
                                                    else if (MainForm.units == "metric")
                                                    {
                                                        temperature = Math.Round(((payload[3] - 40) * 0.555556) - 17.77778).ToString("0");
                                                        unitToInsert = "°C";
                                                    }

                                                    valueToInsert = temperature;
                                                    break;
                                                default:
                                                    descriptionToInsert = "RESPONSE | CMT | DIAGNOSTIC DATA";
                                                    valueToInsert = Util.ByteToHexString(payload, 3, 1);
                                                    unitToInsert = string.Empty;
                                                    break;

                                            }
                                            break;
                                        case 0x22: // read ROM
                                            descriptionToInsert = "RESPONSE | CMT | ROM DATA";
                                            valueToInsert = Util.ByteToHexString(payload, 2, 2);
                                            unitToInsert = string.Empty;
                                            break;
                                        case 0x24: // software version
                                            switch (payload[2])
                                            {
                                                case 0x00: // software version
                                                    descriptionToInsert = "RESPONSE | CMT | SOFTWARE VERSION";
                                                    valueToInsert = Util.ByteToHexString(payload, 2, 2);
                                                    unitToInsert = string.Empty;
                                                    break;
                                                case 0x01: // EEPROM version
                                                    descriptionToInsert = "RESPONSE | CMT | EEPROM VERSION";
                                                    valueToInsert = Util.ByteToHexString(payload, 2, 2);
                                                    unitToInsert = string.Empty;
                                                    break;
                                                default:
                                                    descriptionToInsert = "RESPONSE | CMT";
                                                    valueToInsert = Util.ByteToHexString(payload, 2, 2);
                                                    unitToInsert = string.Empty;
                                                    break;
                                            }
                                            break;
                                        case 0x40: // erase fault codes
                                            descriptionToInsert = "RESPONSE | CMT | ERASE FAULT CODES";
                                            if (payload[3] == 0x00) valueToInsert = "ERASED";
                                            else valueToInsert = "FAILED";
                                            unitToInsert = string.Empty;
                                            break;
                                        case 0xFF: // command error
                                            descriptionToInsert = "RESPONSE | CMT | COMMAND ERROR";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                        default:
                                            descriptionToInsert = "RESPONSE | CMT";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                    }
                                    break;
                                case 0x1E: // ACM - Airbag Control Module
                                    switch (payload[1])
                                    {
                                        case 0x00: // reset
                                            descriptionToInsert = "RESPONSE | AIRBAG CONTROL MODULE | RESET COMPLETE";
                                            valueToInsert = string.Empty;
                                            break;
                                        case 0x16: // read fault codes
                                            descriptionToInsert = "RESPONSE | ACM | FAULT CODES";
                                            if (payload[3] == 0x00) valueToInsert = "NO FAULT CODE";
                                            else valueToInsert = "CODE: " + Util.ByteToHexString(payload, 3, 1);
                                            break;
                                        case 0x24: // software version
                                            descriptionToInsert = "RESPONSE | ACM | SOFTWARE VERSION";
                                            valueToInsert = Util.ByteToHexString(payload, 2, 2);
                                            break;
                                        case 0x40: // erase fault codes
                                            descriptionToInsert = "RESPONSE | ACM | ERASE FAULT CODES";
                                            if (payload[3] == 0x00) valueToInsert = "ERASED";
                                            else valueToInsert = "FAILED";
                                            break;
                                        case 0xFF: // command error
                                            descriptionToInsert = "RESPONSE | ACM | COMMAND ERROR";
                                            valueToInsert = string.Empty;
                                            break;
                                        default:
                                            descriptionToInsert = "RESPONSE | ACM";
                                            valueToInsert = string.Empty;
                                            break;
                                    }

                                    unitToInsert = string.Empty;
                                    break;
                                case 0x20: // BCM - Body Control Module
                                    switch (payload[1]) // command
                                    {
                                        case 0x00: // reset
                                            descriptionToInsert = "RESPONSE | BODY CONTROL MODULE | RESET COMPLETE";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                        case 0x10: // actuator tests
                                            descriptionToInsert = "RESPONSE | BCM | ACTUATOR TEST";
                                            valueToInsert = "RUNNING";
                                            unitToInsert = string.Empty;
                                            break;
                                        case 0x12: // read digital parameters
                                            descriptionToInsert = "RESPONSE | BCM | DIGITAL READ";
                                            valueToInsert = Util.ByteToHexString(payload, 2, 2);
                                            unitToInsert = string.Empty;
                                            break;
                                        case 0x14: // read analog parameters
                                            descriptionToInsert = "RESPONSE | BCM | ANALOG READ";
                                            valueToInsert = Util.ByteToHexString(payload, 3, 1);
                                            unitToInsert = "HEX";
                                            break;
                                        case 0x16: // read fault codes
                                            descriptionToInsert = "RESPONSE | BCM | FAULT CODES";
                                            if (payload[3] == 0x00) valueToInsert = "NO FAULT CODE";
                                            else valueToInsert = "CODE: " + Util.ByteToHexString(payload, 3, 1);
                                            unitToInsert = string.Empty;
                                            break;
                                        case 0x22: // read ROM
                                            descriptionToInsert = "RESPONSE | BCM | ROM DATA";
                                            valueToInsert = "VALUE:     " + Util.ByteToHexString(payload, 2, 1) + " |    " + Util.ByteToHexString(payload, 3, 1);
                                            unitToInsert = string.Empty;
                                            break;
                                        case 0x24: // read module id
                                            descriptionToInsert = "RESPONSE | BCM | MODULE ID";
                                            valueToInsert = Util.ByteToHexString(payload, 2, 2);
                                            unitToInsert = string.Empty;
                                            break;
                                        case 0x2A: // read vehicle identification number (vin)
                                            descriptionToInsert = "RESPONSE | BCM | READ VIN";
                                            valueToInsert = Util.ByteToHexString(payload, 2, 2);
                                            unitToInsert = string.Empty;
                                            break;
                                        case 0x2C: // write vehicle identification number (vin)
                                            descriptionToInsert = "RESPONSE | BCM | WRITE VIN";
                                            valueToInsert = Util.ByteToHexString(payload, 2, 2);
                                            unitToInsert = string.Empty;
                                            break;
                                        case 0x40: // erase fault codes
                                            descriptionToInsert = "RESPONSE | BCM | ERASE FAULT CODES";
                                            if (payload[3] == 0x00) valueToInsert = "ERASED";
                                            else valueToInsert = "FAILED";
                                            unitToInsert = string.Empty;
                                            break;
                                        case 0xB0: // write settings
                                            descriptionToInsert = "RESPONSE | BCM | WRITE SETTINGS";
                                            valueToInsert = Util.ByteToHexString(payload, 2, 2);
                                            unitToInsert = string.Empty;
                                            break;
                                        case 0xB1: // read settings
                                            descriptionToInsert = "RESPONSE | BCM | READ SETTINGS";
                                            valueToInsert = Util.ByteToHexString(payload, 2, 2);
                                            unitToInsert = string.Empty;
                                            break;
                                        case 0xFF: // command error
                                            descriptionToInsert = "RESPONSE | BCM | COMMAND ERROR";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                        default:
                                            descriptionToInsert = "RESPONSE | BCM";
                                            valueToInsert = Util.ByteToHexString(payload, 2, 2);
                                            unitToInsert = string.Empty;
                                            break;
                                    }
                                    break;
                                case 0x22: // MIC - Mechanical Instrument Cluster
                                case 0x60:
                                    switch (payload[1]) // command
                                    {
                                        case 0x00: // reset
                                            descriptionToInsert = "RESPONSE | MIC | RESET COMPLETE";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                        case 0x10: // actuator test
                                            switch (payload[2])
                                            {
                                                case 0x00:
                                                    descriptionToInsert = "RESPONSE | MIC | ALL GAUGES TEST";
                                                    break;
                                                case 0x01:
                                                    descriptionToInsert = "RESPONSE | MIC | ALL LAMPS TEST";
                                                    break;
                                                case 0x02:
                                                    descriptionToInsert = "RESPONSE | MIC | ODO/TRIP/PRND3L TEST";
                                                    break;
                                                case 0x03:
                                                    descriptionToInsert = "RESPONSE | MIC | PRND3L SEGMENTS TEST";
                                                    break;
                                                default:
                                                    descriptionToInsert = "RESPONSE | MIC | ACTUATOR TEST";
                                                    break;
                                            }

                                            valueToInsert = "RUNNING";
                                            unitToInsert = string.Empty;
                                            break;
                                        case 0x12: // read digital parameters
                                            descriptionToInsert = "RESPONSE | MIC | DIGITAL READ";
                                            valueToInsert = Util.ByteToHexString(payload, 3, 1);
                                            unitToInsert = string.Empty;
                                            break;
                                        case 0x16: // read fault codes
                                            descriptionToInsert = "RESPONSE | MIC | FAULT CODES";

                                            if (payload[3] == 0x00) valueToInsert = "NO FAULT CODE";
                                            else
                                            {
                                                List<string> faultCodes = new List<string>();
                                                faultCodes.Clear();

                                                if (Util.IsBitSet(payload[3], 1)) faultCodes.Add("NO BCM MSG");
                                                if (Util.IsBitSet(payload[3], 2)) faultCodes.Add("NO PCM MSG");
                                                if (Util.IsBitSet(payload[3], 4)) faultCodes.Add("BCM FAILURE");
                                                if (Util.IsBitSet(payload[3], 6)) faultCodes.Add("RAM FAILURE");
                                                if (Util.IsBitSet(payload[3], 7)) faultCodes.Add("ROM FAILURE");

                                                foreach (string s in faultCodes)
                                                {
                                                    valueToInsert += s + " | ";
                                                }

                                                valueToInsert = valueToInsert.Remove(valueToInsert.Length - 3); // remove last "|" character
                                            }

                                            unitToInsert = string.Empty;
                                            break;
                                        case 0x24: // software version
                                            descriptionToInsert = "RESPONSE | MIC | SOFTWARE VERSION";
                                            valueToInsert = Util.ByteToHexString(payload, 2, 2);
                                            unitToInsert = string.Empty;
                                            break;
                                        case 0x40: // erase fault codes
                                            descriptionToInsert = "RESPONSE | MIC | ERASE FAULT CODES";
                                            if (payload[3] == 0x00) valueToInsert = "ERASED";
                                            else valueToInsert = "FAILED";
                                            break;
                                        case 0xE0: // self test
                                            descriptionToInsert = "RESPONSE | MIC | SELF TEST";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                        case 0xFF: // command error
                                            descriptionToInsert = "RESPONSE | MIC | COMMAND ERROR";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                        default:
                                            descriptionToInsert = "RESPONSE | MIC";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                    }
                                    break;
                                case 0x41: // TCM - Transmission Control Module
                                case 0x42:
                                    switch (payload[1]) // command
                                    {
                                        case 0x00: // reset
                                            descriptionToInsert = "RESPONSE | TCM | RESET COMPLETE";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                        case 0xFF: // command error
                                            descriptionToInsert = "RESPONSE | TCM | COMMAND ERROR";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                        default:
                                            descriptionToInsert = "RESPONSE | TCM";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                    }
                                    break;
                                case 0x43: // ABS - Antilock Brake System
                                    switch (payload[1]) // command
                                    {
                                        case 0x00: // reset
                                            descriptionToInsert = "RESPONSE | ANTILOCK BRAKE SYSTEM | RESET COMPLETE";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                        case 0xFF: // command error
                                            descriptionToInsert = "RESPONSE | ABS | COMMAND ERROR";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                        default:
                                            descriptionToInsert = "RESPONSE | ABS";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                    }
                                    break;
                                case 0x50: // HVAC - Heat Vent Air Conditioning
                                    switch (payload[1]) // command
                                    {
                                        case 0x00: // reset
                                            descriptionToInsert = "RESPONSE | HVAC | RESET COMPLETE";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                        case 0xFF: // command error
                                            descriptionToInsert = "RESPONSE | HVAC | COMMAND ERROR";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                        default:
                                            descriptionToInsert = "RESPONSE | HVAC";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                    }
                                    break;
                                case 0x80: // DDM - Driver Door Module
                                    switch (payload[1]) // command
                                    {
                                        case 0x00: // reset
                                            descriptionToInsert = "RESPONSE | DRIVER DOOR MODULE | RESET COMPLETE";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                        case 0xFF: // command error
                                            descriptionToInsert = "RESPONSE | DDM | COMMAND ERROR";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                        default:
                                            descriptionToInsert = "RESPONSE | DDM";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                    }
                                    break;
                                case 0x81: // PDM - Passenger Door Module
                                    switch (payload[1]) // command
                                    {
                                        case 0x00: // reset
                                            descriptionToInsert = "RESPONSE | PASSENGER DOOR MODULE | RESET COMPLETE";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                        case 0xFF: // command error
                                            descriptionToInsert = "RESPONSE | PDM | COMMAND ERROR";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                        default:
                                            descriptionToInsert = "RESPONSE | PDM";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                    }
                                    break;
                                case 0x82: // MSM - Memory Seat Module
                                    switch (payload[1]) // command
                                    {
                                        case 0x00: // reset
                                            descriptionToInsert = "RESPONSE | MEMORY SEAT MODULE | RESET COMPLETE";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                        case 0xFF: // command error
                                            descriptionToInsert = "RESPONSE | MSM | COMMAND ERROR";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                        default:
                                            descriptionToInsert = "RESPONSE | MSM";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                    }
                                    break;
                                case 0x96: // ASM - Audio System Module
                                    switch (payload[1]) // command
                                    {
                                        case 0x00: // reset
                                            descriptionToInsert = "RESPONSE | AUDIO SYSTEM MODULE | RESET COMPLETE";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                        case 0xFF: // command error
                                            descriptionToInsert = "RESPONSE | ASM | COMMAND ERROR";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                        default:
                                            descriptionToInsert = "RESPONSE | ASM";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                    }
                                    break;
                                case 0xC0: // SKIM - Sentry Key Immobilizer Module
                                    switch (payload[1]) // command
                                    {
                                        case 0x00: // reset
                                            descriptionToInsert = "RESPONSE | SKIM | RESET COMPLETE";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                        case 0xFF: // command error
                                            descriptionToInsert = "RESPONSE | SKIM | COMMAND ERROR";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                        default:
                                            descriptionToInsert = "RESPONSE | SKIM";
                                            valueToInsert = string.Empty;
                                            unitToInsert = string.Empty;
                                            break;
                                    }
                                    break;
                                default:
                                    descriptionToInsert = "RESPONSE |";
                                    valueToInsert = string.Empty;
                                    unitToInsert = string.Empty;
                                    break;
                            }
                        }
                        else
                        {
                            descriptionToInsert = "RESPONSE |";
                            valueToInsert = "ERROR";
                            unitToInsert = string.Empty;
                        }

                        if (MainForm.includeTimestampInLogFiles)
                        {
                            TimeSpan ElapsedTime = TimeSpan.FromMilliseconds(timestamp[0] << 24 | timestamp[1] << 16 | timestamp[2] << 8 | timestamp[3]);
                            DateTime Timestamp = DateTime.Today.Add(ElapsedTime);
                            string TimestampString = Timestamp.ToString("HH:mm:ss.fff") + " ";
                            File.AppendAllText(MainForm.CCDB2F2LogFilename, TimestampString); // no newline is appended!
                        }

                        File.AppendAllText(MainForm.CCDB2F2LogFilename, Util.ByteToHexStringSimple(message) + Environment.NewLine);
                        break;
                    case 0xF3: // switch message
                        valueToInsert = string.Empty;
                        unitToInsert = string.Empty;
                        break;
                    case 0xF5: // MIL control
                        valueToInsert = string.Empty;
                        unitToInsert = string.Empty;
                        break;
                    case 0xF6: // unknown feature present
                        valueToInsert = string.Empty;
                        unitToInsert = string.Empty;
                        break;
                    case 0xFD: // compass compensation amd temperature data received
                        valueToInsert = string.Empty;
                        unitToInsert = string.Empty;
                        break;
                    case 0xFE: // interior lamp dimming
                        if (message.Length >= minLength)
                        {
                            valueToInsert = Math.Round(payload[0] * 0.392D).ToString("0");
                            unitToInsert = "%";
                        }
                        else
                        {
                            valueToInsert = "ERROR";
                            unitToInsert = string.Empty;
                        }
                        break;
                    case 0xFF: // CCD-bus wake up
                        valueToInsert = string.Empty;
                        unitToInsert = string.Empty;
                        break;
                    default:
                        valueToInsert = string.Empty;
                        unitToInsert = string.Empty;
                        break;
                }

                MessageDatabase.Rows[rowIndex]["message"] = hexBytesToInsert; // edit data table
                MessageDatabase.Rows[rowIndex]["description"] = descriptionToInsert; // edit data table
                MessageDatabase.Rows[rowIndex]["value"] = valueToInsert; // edit data table
                MessageDatabase.Rows[rowIndex]["unit"] = unitToInsert; // edit data table
            }

            if (message.Length < 9) // max 8 message byte fits the message column
            {
                hexBytesToInsert = Util.ByteToHexString(message, 0, message.Length) + " ";
            }
            else // Trim message (display 7 bytes only) and insert two dots at the end indicating there's more to it
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

            if (MainForm.includeTimestampInLogFiles)
            {
                TimeSpan ElapsedTime = TimeSpan.FromMilliseconds(timestamp[0] << 24 | timestamp[1] << 16 | timestamp[2] << 8 | timestamp[3]);
                DateTime Timestamp = DateTime.Today.Add(ElapsedTime);
                string TimestampString = Timestamp.ToString("HH:mm:ss.fff") + " ";
                File.AppendAllText(MainForm.CCDLogFilename, TimestampString); // no newline is appended!
            }

            File.AppendAllText(MainForm.CCDLogFilename, Util.ByteToHexStringSimple(message) + Environment.NewLine);
        }
    }
}
