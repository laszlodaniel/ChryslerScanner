using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace ChryslerCCDSCIScanner
{
    public class SCIPCM
    {
        public SCIPCMDiagnosticsTable Diagnostics = new SCIPCMDiagnosticsTable();
        public DataTable MessageDatabase = new DataTable("PCMDatabase");
        public DataTable EngineDTC = new DataTable("EngineDTC");
        public List<byte> engineFaultCodeList = new List<byte>();
        public bool engineFaultCodesSaved = true;
        public ushort[] IDList;
        public byte[] engineDTCList;
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

        public string HeaderUnknown  = "│ SCI-BUS ENGINE          │ STATE: N/A                                                                                   ";
        public string HeaderDisabled = "│ SCI-BUS ENGINE          │ STATE: DISABLED                                                                              ";
        public string HeaderEnabled  = "│ SCI-BUS ENGINE          │ STATE: ENABLED @ BAUD | LOGIC: | CONFIGURATION:                                              ";
        public string EmptyLine      = "│                         │                                                     │                         │             │";
        public string HeaderModified = string.Empty;

        public SCIPCM()
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

            #region SCI-bus (PCM) messages

            row = MessageDatabase.NewRow();
            row["id"] = 0x10;
            row["length"] = 3;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "ENGINE FAULT CODE LIST";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x11;
            row["length"] = 2;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "FAULT BIT LIST";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x12;
            row["length"] = 1;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "SELECT HIGH-SPEED MODE";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x13;
            row["length"] = 2;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "ACTUATOR TEST";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x14;
            row["length"] = 3;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "REQUEST DIAGNOSTIC DATA";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x15;
            row["length"] = 4;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "ROM VALUE";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x16;
            row["length"] = 3;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "ROM CONSTANT VALUE";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x17;
            row["length"] = 2;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "ERASE FAULT CODES";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x18;
            row["length"] = 2;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "CONTROL ASD RELAY";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x19;
            row["length"] = 2;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "SET MINIMUM IDLE SPEED";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x1A;
            row["length"] = 3;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "SWITCH TEST";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x1B;
            row["length"] = 1;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "INIT BYTE MODE DOWNLOAD";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x1C;
            row["length"] = 4;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "WRITE MEMORY";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x21;
            row["length"] = 3;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "SET SYNC / TIMING / SPARK SCATTER";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x22;
            row["length"] = 3;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "SEND ENGINE PARAMETERS";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x23;
            row["length"] = 3;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "ENGINE CONTROLLER REQUEST";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x26;
            row["length"] = 5;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "READ FLASH MEMORY";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x27;
            row["length"] = 5;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "WRITE FLASH MEMORY";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x28;
            row["length"] = 4;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "READ EEPROM";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x2A;
            row["length"] = 2;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "INFORMATION REQUEST";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x2B;
            row["length"] = 4;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "GET SECURITY SEED";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x2C;
            row["length"] = 5;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "SEND SECURITY SEED";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x2D;
            row["length"] = 3;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "MIN/MAX ENGINE PARAMETER VALUE";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x2E;
            row["length"] = 2;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "ONE-TRIP FAULT CODES";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0x32;
            row["length"] = 2;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "ENGINE FAULT CODES";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0xF0;
            row["length"] = 3;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "F0 RAM TABLE VALUE(S)";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0xF1;
            row["length"] = 3;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "F1 RAM TABLE TABLE VALUE(S)";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0xF2;
            row["length"] = 3;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "F2 RAM TABLE VALUE(S)";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0xF3;
            row["length"] = 3;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "F3 RAM TABLE VALUE(S)";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0xF4;
            row["length"] = 3;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "F4 RAM TABLE VALUE(S)";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0xF5;
            row["length"] = 3;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "F5 RAM TABLE VALUE(S)";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0xF6;
            row["length"] = 3;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "F6 RAM TABLE VALUE(S)";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0xF7;
            row["length"] = 3;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "F7 RAM TABLE VALUE(S)";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0xF8;
            row["length"] = 3;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "F8 RAM TABLE VALUE(S)";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0xF9;
            row["length"] = 3;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "F9 RAM TABLE VALUE(S)";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0xFA;
            row["length"] = 3;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "FA RAM TABLE VALUE(S)";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0xFB;
            row["length"] = 3;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "FB RAM TABLE VALUE(S)";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0xFC;
            row["length"] = 3;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "FC RAM TABLE VALUE(S)";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0xFD;
            row["length"] = 3;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "FD RAM TABLE VALUE(S)";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            row = MessageDatabase.NewRow();
            row["id"] = 0xFE;
            row["length"] = 1;
            row["parameterCount"] = 1;
            row["message"] = string.Empty;
            row["description"] = "SELECT LOW-SPEED MODE";
            row["value"] = string.Empty;
            row["unit"] = string.Empty;
            MessageDatabase.Rows.Add(row);

            #endregion

            IDList = MessageDatabase.AsEnumerable().Select(r => r.Field<ushort>("id")).ToArray();

            column = new DataColumn();
            column.DataType = typeof(byte);
            column.ColumnName = "id";
            column.ReadOnly = true;
            column.Unique = true;
            EngineDTC.Columns.Add(column);

            column = new DataColumn();
            column.DataType = typeof(string);
            column.ColumnName = "description";
            column.ReadOnly = true;
            column.Unique = false;
            EngineDTC.Columns.Add(column);

            DataColumn[] PrimaryKeyColumnsDTC = new DataColumn[1];
            PrimaryKeyColumnsDTC[0] = EngineDTC.Columns["id"];
            EngineDTC.PrimaryKey = PrimaryKeyColumnsDTC;

            DataSet dataSetDTC = new DataSet();
            dataSetDTC.Tables.Add(EngineDTC);

            #region SCI-bus (PCM) fault codes

            row = EngineDTC.NewRow();
            row["id"] = 0x00;
            row["description"] = "UNRECOGNIZED DTC";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x01;
            row["description"] = "NO CAM SIGNAL AT PCM";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x02;
            row["description"] = "INTERNAL CONTROLLER FAILURE";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x03;
            row["description"] = "O2 SENSOR STAYS ABOVE CENTER (RICH)";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x04;
            row["description"] = "O2 SENSOR STAYS BELOW CENTER (LEAN)";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x05;
            row["description"] = "CHARGING SYSTEM VOLTAGE TOO LOW";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x06;
            row["description"] = "CHARGING SYSTEM VOLTAGE TOO HIGH";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x07;
            row["description"] = "TURBO BOOST LIMIT EXCEEDED";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x08;
            row["description"] = "RIGHT O2 SENSOR STAYS ABOVE CENTER (RICH)";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x09;
            row["description"] = "RIGHT O2 SENSOR STAYS BELOW CENTER (LEAN)";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x0A;
            row["description"] = "AUTO SHUTDOWN RELAY CONTROL CIRCUIT";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x0B;
            row["description"] = "GENERATOR FIELD NOT SWITCHING PROPERLY";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x0C;
            row["description"] = "TORQUE CONVERTER CLUTCH SOLENOID / TRANS RELAY CIRCUITS";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x0D;
            row["description"] = "TURBOCHARGER WASTEGATE SOLENOID CIRCUIT";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x0E;
            row["description"] = "LOW SPEED FAN CONTROL RELAY CIRCUIT";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x0F;
            row["description"] = "SPEED CONTROL SOLENOID CIRCUITS";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x10;
            row["description"] = "A/C CLUTCH RELAY CIRCUIT";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x11;
            row["description"] = "EGR SOLENOID CIRCUIT";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x12;
            row["description"] = "EVAP PURGE SOLENOID CIRCUIT";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x13;
            row["description"] = "INJECTOR #3 CONTROL CIRCUIT";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x14;
            row["description"] = "INJECTOR #2 CONTROL CIRCUIT";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x15;
            row["description"] = "INJECTOR #1 CONTROL CIRCUIT";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x16;
            row["description"] = "INJECTOR #3 PEAK CURRENT NOT REACHED";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x17;
            row["description"] = "INJECTOR #2 PEAK CURRENT NOT REACHED";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x18;
            row["description"] = "INJECTOR #1 PEAK CURRENT NOT REACHED";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x19;
            row["description"] = "IDLE AIR CONTROL MOTOR CIRCUITS";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x1A;
            row["description"] = "THROTTLE POSITION SENSOR VOLTAGE LOW";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x1B;
            row["description"] = "THROTTLE POSITION SENSOR VOLTAGE HIGH";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x1C;
            row["description"] = "THROTTLE BODY TEMP SENSOR VOLTAGE LOW";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x1D;
            row["description"] = "THROTTLE BODY TEMP SENSOR VOLTAGE HIGH";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x1E;
            row["description"] = "COOLANT TEMPERATURE SENSOR VOLTAGE TOO LOW";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x1F;
            row["description"] = "COOLANT TEMPERATURE SENSOR VOLTAGE TOO HIGH";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x20;
            row["description"] = "UPSTREAM O2 SENSOR STAYS AT CENTER";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x21;
            row["description"] = "ENGINE IS COLD TOO LONG";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x22;
            row["description"] = "SKIP SHIFT SOLENOID CIRCUIT";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x23;
            row["description"] = "NO VEHICLE SPEED SENSOR SIGNAL";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x24;
            row["description"] = "MAP SENSOR VOLTAGE TOO LOW";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x25;
            row["description"] = "MAP SENSOR VOLTAGE TOO HIGH";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x26;
            row["description"] = "SLOW CHANGE IN IDLE MAP SENSOR SIGNAL";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x27;
            row["description"] = "NO CHANGE IN MAP FROM START TO RUN";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x28;
            row["description"] = "NO CRANKSHAFT REFERENCE SIGNAL AT PCM";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x29;
            row["description"] = "IGNITION COIL #3 PRIMARY CIRCUIT";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x2A;
            row["description"] = "IGNITION COIL #2 PRIMARY CIRCUIT";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x2B;
            row["description"] = "IGNITION COIL #1 PRIMARY CIRCUIT";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x2C;
            row["description"] = "NO ASD RELAY OUTPUT VOLTAGE AT PCM";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x2D;
            row["description"] = "SYSTEM RICH, IDLE ADAPTIVE AT LEAN LIMIT";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x2E;
            row["description"] = "EGR SYSTEM FAILURE";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x2F;
            row["description"] = "BAROMETRIC READ SOLENOID CIRCUIT";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x30;
            row["description"] = "PCM FAILURE SRI MILE NOT STORED";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x31;
            row["description"] = "PCM FAILURE EEPROM WRITE DENIED";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x32;
            row["description"] = "TRANSMISSION 3-4 SHIFT SOLENOID / TRANSMISSION RELAY CIRCUITS";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x33;
            row["description"] = "SECONDARY AIR SOLENOID CIRCUIT";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x34;
            row["description"] = "IDLE SWITCH SHORTED TO GROUND";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x35;
            row["description"] = "IDLE SWITCH OPEN CIRCUIT";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x36;
            row["description"] = "SURGE VALVE SOLENOID CIRCUIT";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x37;
            row["description"] = "INJECTOR #9 CONTROL CIRCUIT";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x38;
            row["description"] = "INJECTOR #10 CONTROL CIRCUIT";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x39;
            row["description"] = "INTAKE AIR TEMPERATURE SENSOR VOLTAGE LOW";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x3A;
            row["description"] = "INTAKE AIR TEMPERATURE SENSOR VOLTAGE HIGH";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x3B;
            row["description"] = "KNOCK SENSOR CIRCUIT";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x3C;
            row["description"] = "BAROMETRIC PRESSURE OUT OF RANGE";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x3D;
            row["description"] = "INJECTOR #4 CONTROL CIRCUIT";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x3E;
            row["description"] = "UPSTREAM O2 SENSOR SHORTED TO VOLTAGE";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x3F;
            row["description"] = "FUEL SYSTEM RICH, R-IDLE ADAPTIVE AT LEAN LIMIT";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x40;
            row["description"] = "WASTEGATE #2 CIRCUIT";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x41;
            row["description"] = "RIGHT O2 SENSOR STAYS AT CENTER";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x42;
            row["description"] = "RIGHT O2 SENSOR SHORTED TO VOLTAGE";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x43;
            row["description"] = "FUEL SYSTEM LEAN, R-IDLE ADAPTIVE AT RICH LIMIT";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x44;
            row["description"] = "PCM FAILURE SPI COMMUNICATIONS";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x45;
            row["description"] = "INJECTOR #5 CONTROL CIRCUIT";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x46;
            row["description"] = "INJECTOR #6 CONTROL CIRCUIT";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x47;
            row["description"] = "BATTERY TEMPERATURE SENSOR VOLTS OUT OF LIMIT";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x48;
            row["description"] = "NO CMP AT IGNITION / INJ DRIVER MODULE";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x49;
            row["description"] = "NO CKP AT IGNITION/ INJ DRIVER MODULE";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x4A;
            row["description"] = "TRANSMISSION TEMPERATURE SENSOR VOLTAGE TOO LOW";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x4B;
            row["description"] = "TRANSMISSION TEMPERATURE SENSOR VOLTAGE TOO HIGH";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x4C;
            row["description"] = "IGNITION COIL #4 PRIMARY CIRCUIT";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x4D;
            row["description"] = "IGNITION COIL #5 PRIMARY CIRCUIT";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x4E;
            row["description"] = "FUEL SYSTEM LEAN, IDLE ADAPTIVE AT RICH LIMIT";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x4F;
            row["description"] = "INJECTOR #7 CONTROL CIRCUIT";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x50;
            row["description"] = "INJECTOR #8 CONTROL CIRCUIT";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x51;
            row["description"] = "FUEL PUMP RESISTOR BYPASS RELAY CIRCUIT";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x52;
            row["description"] = "SPEED CONTROL POWER RELAY; OR S/C 12V DRIVER CIRCUIT";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x53;
            row["description"] = "KNOCK SENSOR #2 CIRCUIT";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x54;
            row["description"] = "FLEX FUEL SENSOR VOLTS TOO HIGH";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x55;
            row["description"] = "FLEX FUEL SENSOR VOLTS TOO LOW";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x56;
            row["description"] = "SPEED CONTROL SWITCH ALWAYS HIGH";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x57;
            row["description"] = "SPEED CONTROL SWITCH ALWAYS LOW";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x58;
            row["description"] = "MANIFOLD TUNE VALVE SOLENOID CIRCUIT";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x59;
            row["description"] = "NO BUS MESSAGES";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x5A;
            row["description"] = "A/C PRESSURE SENSOR VOLTS TOO HIGH";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x5B;
            row["description"] = "A/C PRESSURE SENSOR VOLTS TOO LOW";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x5C;
            row["description"] = "LOW SPEED FAN CONTROL RELAY CIRCUIT";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x5D;
            row["description"] = "HIGH SPEED CONDENSER FAN CTRL RELAY CIRCUIT";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x5E;
            row["description"] = "CNG TEMPERATURE SENSOR VOLTAGE TOO LOW";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x5F;
            row["description"] = "CNG TEMPERATURE SENSOR VOLTAGE TOO HIGH";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x60;
            row["description"] = "NO CCD/J1850 MESSAGES FROM TCM";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x61;
            row["description"] = "NO CCD/PCI BUS MESSAGE FROM BCM";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x62;
            row["description"] = "CNG PRESSURE SENSOR VOLTAGE TOO HIGH";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x63;
            row["description"] = "CNG PRESSURE SENSOR VOLTAGE TOO LOW";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x64;
            row["description"] = "LOSS OF FLEX FUEL CALIBRATION SIGNAL";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x65;
            row["description"] = "FUEL PUMP RELAY CONTROL CIRCUIT";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x66;
            row["description"] = "UPSTREAM O2 SENSOR SLOW RESPONSE";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x67;
            row["description"] = "UPSTREAM O2 SENSOR HEATER FAILURE";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x68;
            row["description"] = "DOWNSTREAM O2 SENSOR UNABLE TO SWITCH RICH/LEAN";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x69;
            row["description"] = "DOWNSTREAM O2 SENSOR HEATER FAILURE";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x6A;
            row["description"] = "MULTIPLE CYLINDER MIS-FIRE";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x6B;
            row["description"] = "CYLINDER #1 MIS-FIRE";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x6C;
            row["description"] = "CYLINDER #2 MIS-FIRE";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x6D;
            row["description"] = "CYLINDER #3 MIS-FIRE";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x6E;
            row["description"] = "CYLINDER #4 MIS-FIRE";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x6F;
            row["description"] = "TOO LITTLE SECONDARY AIR";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x70;
            row["description"] = "CATALYTIC CONVERTER EFFICIENCY FAILURE";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x71;
            row["description"] = "EVAP PURGE FLOW MONITOR FAILURE";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x72;
            row["description"] = "P/N SWITCH STUCK IN PARK OR IN GEAR";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x73;
            row["description"] = "POWER STEERING SWITCH FAILURE";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x74;
            row["description"] = "DESIRED FUEL TIMING ADVANCE NOT REACHED";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x75;
            row["description"] = "LOST FUEL INJECTION TIMING SIGNAL";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x76;
            row["description"] = "FUEL SYSTEM RICH";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x77;
            row["description"] = "FUEL SYSTEM LEAN";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x78;
            row["description"] = "RIGHT BANK FUEL SYSTEM RICH";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x79;
            row["description"] = "RIGHT BANK FUEL SYSTEM LEAN";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x7A;
            row["description"] = "RIGHT BANK UPSTREAM O2 SENSOR SLOW RESPONSE";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x7B;
            row["description"] = "RIGHT BANK DOWNSTREAM O2 SENSOR SLOW RESPONSE";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x7C;
            row["description"] = "RIGHT BANK UPSTREAM O2 SENSOR HEATER FAILURE";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x7D;
            row["description"] = "RIGHT BANK DOWNSTREAM O2 SENSOR HEATER FAILURE";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x7E;
            row["description"] = "DOWNSTREAM O2 SENSOR SHORTED TO VOLTAGE";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x7F;
            row["description"] = "RIGHT BANK DOWNSTREAM O2 SENSOR SHORTED TO VOLTAGE";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x80;
            row["description"] = "CLOSED LOOP TEMPERATURE NOT REACHED";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x81;
            row["description"] = "DOWNSTREAM O2 SENSOR STAYS AT CENTER";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x82;
            row["description"] = "RIGHT BANK DOWNSTREAM O2 SENSOR STAYS AT CENTER";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x83;
            row["description"] = "LEAN OPERATION AT WIDE OPEN THROTTLE";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x84;
            row["description"] = "TPS VOLTAGE DOES NOT AGREE WITH MAP";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x85;
            row["description"] = "TIMING BELT SKIPPED 1 TOOTH OR MORE";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x86;
            row["description"] = "NO 5 VOLTS TO A/C PRESSURE SENSOR";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x87;
            row["description"] = "NO 5 VOLTS TO MAP SENSOR";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x88;
            row["description"] = "NO 5 VOLTS TO TPS";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x89;
            row["description"] = "EATX CONTROLLER DTC PRESENT";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x8A;
            row["description"] = "TARGET IDLE NOT REACHED";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x8B;
            row["description"] = "HIGH SPEED RADIATOR FAN CONTROL RELAY CIRCUIT";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x8C;
            row["description"] = "DIESEL EGR SYSTEM FAILURE";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x8D;
            row["description"] = "GOVERNOR PRESSURE NOT EQUAL TO TARGET @ 15 - 20 PSI";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x8E;
            row["description"] = "GOVERNOR PRESSURE ABOVE 3 PSI IN GEAR WITH 0 MPH";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x8F;
            row["description"] = "STARTER RELAY CONTROL CIRCUIT";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x90;
            row["description"] = "DOWNSTREAM O2 SENSOR SHORTED TO GROUND";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x91;
            row["description"] = "VACUUM LEAK FOUND (IAC FULLY SEATED)";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x92;
            row["description"] = "5 VOLT SUPPLY, OUTPUT TOO LOW";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x93;
            row["description"] = "DOWNSTREAM O2 SENSOR SHORTED TO VOLTAGE";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x94;
            row["description"] = "TORQUE CONVERTER CLUTCH, NO RPM DROP AT LOCKUP";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x95;
            row["description"] = "FUEL LEVEL SENDING UNIT VOLTS TOO LOW";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x96;
            row["description"] = "FUEL LEVEL SENDING UNIT VOLTS TOO HIGH";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x97;
            row["description"] = "FUEL LEVEL UNIT NO CHANGE OVER MILES";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x98;
            row["description"] = "BRAKE SWITCH STUCK PRESSED OR RELEASED";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x99;
            row["description"] = "AMBIENT / BATTERY TEMPERATURE SENSOR VOLTS TOO LOW";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x9A;
            row["description"] = "AMBIENT / BATTERY TEMPERATURE SENSOR VOLTS TOO HIGH";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x9B;
            row["description"] = "UPSTREAM O2 SENSOR SHORTED TO GROUND";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x9C;
            row["description"] = "DOWNSTREAM O2 SENSOR SHORTED TO GROUND";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x9D;
            row["description"] = "INTERMITTENT LOSS OF CMP OR CKP";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x9E;
            row["description"] = "TOO MUCH SECONDARY AIR";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x9F;
            row["description"] = "DOWNSTREAM O2 SENSOR SLOW RESPONSE";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xA0;
            row["description"] = "EVAP LEAK MONITOR SMALL LEAK DETECTED";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xA1;
            row["description"] = "EVAP LEAK MONITOR LARGE LEAK DETECTED";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xA2;
            row["description"] = "NO TEMPERATURE RISE SEEN FROM INTAKE HEATERS";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xA3;
            row["description"] = "WAIT TO START LAMP CIRCUIT";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xA4;
            row["description"] = "TRANSMISSION TEMPERATURE SENSOR, NO TEMPERATURE RISE AFTR START";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xA5;
            row["description"] = "3-4 SHIFT SOLENOID, NO RPM DROP @ 3-4 SHIFT";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xA6;
            row["description"] = "LOW OUTPUT SPEED SENSOR RPM, ABOVE 15 MPH";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xA7;
            row["description"] = "GOVERNOR PRESSURE SENSOR VOLTS TOO LOW";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xA8;
            row["description"] = "GOVERNOR PRESSURE SENSOR VOLTS TOO HIGH";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xA9;
            row["description"] = "GOVERNOR PRESSURE SENSOR OFFSET VOLTS TOO LOW OR HIGH";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xAB;
            row["description"] = "GOVERNOR PRESSURE SOLENOID CONTROL / TRANSMISSION RELAY CIRCUITS";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xAC;
            row["description"] = "DOWNSTREAM O2 SENSOR STUCK AT CENTER";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xAD;
            row["description"] = "TRANSMISSION 12 VOLT SUPPLY RELAY CONTROL CIRCUIT";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xAE;
            row["description"] = "CYLINDER #5 MIS-FIRE";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xAF;
            row["description"] = "CYLINDER #6 MIS-FIRE";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xB0;
            row["description"] = "CYLINDER #7 MIS-FIRE";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xB1;
            row["description"] = "CYLINDER #8 MIS-FIRE";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xB2;
            row["description"] = "CYLINDER #9 MIS-FIRE";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xB3;
            row["description"] = "CYLINDER #10 MIS-FIRE";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xB4;
            row["description"] = "RIGHT BANK CATALYST EFFICIENCY FAILURE";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xB5;
            row["description"] = "REAR BANK UPSTREAM O2 SENSOR SHORTED TO GROUND";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xB6;
            row["description"] = "REAR BANK DOWNSTREAM O2 SENSOR SHORTED TO GROUND";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xB7;
            row["description"] = "LEAK DETECTION PUMP SOLENOID CIRCUIT";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xB8;
            row["description"] = "LEAK DETECT PUMP SWITCH OR MECHANICAL FAULT";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xB9;
            row["description"] = "AUXILIARY 5 VOLT SUPPLY OUTPUT TOO LOW";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xBA;
            row["description"] = "MIS-FIRE ADAPTIVE NUMERATOR AT LIMIT";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xBB;
            row["description"] = "EVAP LEAK MONITOR PINCHED HOSE FOUND";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xBC;
            row["description"] = "O/D SWITCH PRESSED (LOW) MORE THAN 5 MIN";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xBD;
            row["description"] = "DOWNSTREAM O2 SENSOR HEATER FAILURE";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xC5;
            row["description"] = "HIGH SPEED RADIATOR FAN GROUND CONTROL RELAY CIRCUIT";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xC6;
            row["description"] = "ONE OF THE IGNITION COILS DRAWS TOO MUCH CURRENT";
            EngineDTC.Rows.Add(row);

            #endregion

            engineDTCList = EngineDTC.AsEnumerable().Select(r => r.Field<byte>("id")).ToArray();
        }

        public void UpdateHeader(string state = "enabled", string speed = null, string logic = null, string configuration = null)
        {
            if (state != null) this.state = state;
            if (speed != null) this.speed = speed;
            if (logic != null) this.logic = logic;
            if (configuration != null) this.configuration = configuration;

            if (this.state == "enabled")
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
            string hexBytesToInsert = string.Empty;
            string descriptionToInsert = string.Empty;
            string valueToInsert = string.Empty;
            string unitToInsert = string.Empty;
            byte[] timestamp = new byte[4];
            byte[] message = new byte[] { };
            byte[] payload = new byte[] { };
            byte[] engineFaultCodePayload = new byte[] { };

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

            if (message.Length > 2)
            {
                engineFaultCodePayload = new byte[data.Length - 6];
                Array.Copy(data, 5, engineFaultCodePayload, 0, engineFaultCodePayload.Length); // copy payload from the input byte array (without ID and checksum byte)
            }

            byte ID = message[0];
            ushort modifiedID;

            if ((ID == 0x14)  || (ID == 0x22) || ((ID >= 0xF0) && (ID < 0xFE)))
            {
                if (payload.Length > 0) modifiedID = (ushort)(((ID << 8) & 0xFF00) + payload[0]);
                else modifiedID = (ushort)((ID << 8) & 0xFF00);
            }
            else 
            {
                modifiedID = (ushort)((ID << 8) & 0xFF00);
            }

            int rowIndex = MessageDatabase.Rows.IndexOf(MessageDatabase.Rows.Find(ID)); // search ID byte among known messages and get row index

            if (rowIndex != -1) // row found
            {
                descriptionToInsert = MessageDatabase.Rows[rowIndex]["description"].ToString();

                int minLength = Convert.ToInt32(MessageDatabase.Rows[rowIndex]["length"]);

                if ((speed == "976.5 baud") || (speed == "7812.5 baud"))
                {
                    switch (ID)
                    {
                        case 0x10: // engine fault code list
                            if (message.Length >= minLength)
                            {
                                byte checksum = 0;
                                int checksumLocation = message.Length - 1;

                                for (int i = 0; i < checksumLocation; i++)
                                {
                                    checksum += message[i];
                                }

                                if (checksum == message[checksumLocation])
                                {
                                    engineFaultCodeList.Clear();
                                    engineFaultCodeList.AddRange(engineFaultCodePayload);
                                    engineFaultCodeList.Remove(0xFD); // not fault code related
                                    engineFaultCodeList.Remove(0xFE); // end of fault code list signifier

                                    if (engineFaultCodeList.Count > 0)
                                    {
                                        valueToInsert = Util.ByteToHexStringSimple(engineFaultCodeList.ToArray());
                                        engineFaultCodesSaved = false;
                                    }
                                    else
                                    {
                                        valueToInsert = "NO FAULT CODES";
                                        engineFaultCodesSaved = false;
                                    }
                                }
                                else
                                {
                                    valueToInsert = "CHECKSUM ERROR";
                                    engineFaultCodesSaved = true;
                                }
                            }
                            else // error
                            {
                                valueToInsert = "ERROR";
                                engineFaultCodesSaved = true;
                            }
                            unitToInsert = string.Empty;
                            break;
                        case 0x11: // fault bit list
                            if (message.Length >= minLength)
                            {
                                valueToInsert = Util.ByteToHexStringSimple(payload);
                            }
                            else // error
                            {
                                valueToInsert = "ERROR";
                            }
                            unitToInsert = string.Empty;
                            break;
                        case 0x12: // select high-speed mode
                            if (message.Length >= minLength)
                            {
                                valueToInsert = string.Empty;
                            }
                            else // error
                            {
                                valueToInsert = "ERROR";
                            }
                            unitToInsert = string.Empty;
                            break;
                        case 0x13: // actuator test
                            if (message.Length >= minLength)
                            {
                                switch (payload[0])
                                {
                                    case 0x00:
                                        if (payload.Length > 1)
                                        {
                                            if (payload[1] == 0x00)
                                            {
                                                descriptionToInsert = "ACTUATOR TEST";
                                                valueToInsert = "STOPPED";
                                            }
                                        }
                                        break;
                                    case 0x01:
                                        descriptionToInsert = "ACTUATOR TEST | IGNITION COIL BANK #1";
                                        valueToInsert = "RUNNING";
                                        break;
                                    case 0x02:
                                        descriptionToInsert = "ACTUATOR TEST | IGNITION COIL BANK #2";
                                        valueToInsert = "RUNNING";
                                        break;
                                    case 0x03:
                                        descriptionToInsert = "ACTUATOR TEST | IGNITION COIL BANK #3";
                                        valueToInsert = "RUNNING";
                                        break;
                                    case 0x04:
                                        descriptionToInsert = "ACTUATOR TEST | FUEL INJECTOR BANK #1";
                                        valueToInsert = "RUNNING";
                                        break;
                                    case 0x05:
                                        descriptionToInsert = "ACTUATOR TEST | FUEL INJECTOR BANK #2";
                                        valueToInsert = "RUNNING";
                                        break;
                                    case 0x06:
                                        descriptionToInsert = "ACTUATOR TEST | FUEL INJECTOR BANK #3";
                                        valueToInsert = "RUNNING";
                                        break;
                                    case 0x07:
                                        descriptionToInsert = "ACTUATOR TEST | IDLE AIR CONTROL (IAC) STEPPER MOTOR";
                                        valueToInsert = "RUNNING";
                                        break;
                                    case 0x08:
                                        descriptionToInsert = "ACTUATOR TEST | RADIATOR FAN RELAY";
                                        valueToInsert = "RUNNING";
                                        break;
                                    case 0x09:
                                        descriptionToInsert = "ACTUATOR TEST | A/C CLUTCH RELAY";
                                        valueToInsert = "RUNNING";
                                        break;
                                    case 0x0A:
                                        descriptionToInsert = "ACTUATOR TEST | AUTOMATIC SHUTDOWN (ASD) RELAY";
                                        valueToInsert = "RUNNING";
                                        break;
                                    case 0x0B:
                                        descriptionToInsert = "ACTUATOR TEST | EVAP PURGE SOLENOID";
                                        valueToInsert = "RUNNING";
                                        break;
                                    case 0x0C:
                                        descriptionToInsert = "ACTUATOR TEST | SPEED CONTROL SOLENOID";
                                        valueToInsert = "RUNNING";
                                        break;
                                    case 0x0D:
                                        descriptionToInsert = "ACTUATOR TEST | GENERATOR / ALTERNATOR FIELD";
                                        valueToInsert = "RUNNING";
                                        break;
                                    case 0x0E:
                                        descriptionToInsert = "ACTUATOR TEST | TACHOMETER OUTPUT";
                                        valueToInsert = "RUNNING";
                                        break;
                                    case 0x0F:
                                        descriptionToInsert = "ACTUATOR TEST | TORQUE CONVERTER CLUTCH";
                                        valueToInsert = "RUNNING";
                                        break;
                                    case 0x10:
                                        descriptionToInsert = "ACTUATOR TEST | EGR SOLENOID";
                                        valueToInsert = "RUNNING";
                                        break;
                                    case 0x11:
                                        descriptionToInsert = "ACTUATOR TEST | WASTEGATE SOLENOID";
                                        valueToInsert = "RUNNING";
                                        break;
                                    case 0x12:
                                        descriptionToInsert = "ACTUATOR TEST | BAROMETER SOLENOID";
                                        valueToInsert = "RUNNING";
                                        break;
                                    case 0x14:
                                        descriptionToInsert = "ACTUATOR TEST | ALL SOLENOIDS / RELAYS";
                                        valueToInsert = "RUNNING";
                                        break;
                                    case 0x16:
                                        descriptionToInsert = "ACTUATOR TEST | TRANSMISSION O/D SOLENOID";
                                        valueToInsert = "RUNNING";
                                        break;
                                    case 0x17:
                                        descriptionToInsert = "ACTUATOR TEST | SHIFT INDICATOR LAMP";
                                        valueToInsert = "RUNNING";
                                        break;
                                    case 0x19:
                                        descriptionToInsert = "ACTUATOR TEST | SURGE VALVE SOLENOID";
                                        valueToInsert = "RUNNING";
                                        break;
                                    case 0x1A:
                                        descriptionToInsert = "ACTUATOR TEST | SPEED CONTROL VENT SOLENOID";
                                        valueToInsert = "RUNNING";
                                        break;
                                    case 0x1B:
                                        descriptionToInsert = "ACTUATOR TEST | SPEED CONTROL VACUUM SOLENOID";
                                        valueToInsert = "RUNNING";
                                        break;
                                    case 0x1C:
                                        descriptionToInsert = "ACTUATOR TEST | ASD FUEL SYSTEM";
                                        valueToInsert = "RUNNING";
                                        break;
                                    case 0x1D:
                                        descriptionToInsert = "ACTUATOR TEST | FUEL INJECTOR BANK #4";
                                        valueToInsert = "RUNNING";
                                        break;
                                    case 0x1E:
                                        descriptionToInsert = "ACTUATOR TEST | FUEL INJECTOR BANK #5";
                                        valueToInsert = "RUNNING";
                                        break;
                                    case 0x1F:
                                        descriptionToInsert = "ACTUATOR TEST | FUEL INJECTOR BANK #6";
                                        valueToInsert = "RUNNING";
                                        break;
                                    case 0x23:
                                        descriptionToInsert = "ACTUATOR TEST | IGNITION COIL BANK #4";
                                        valueToInsert = "RUNNING";
                                        break;
                                    case 0x24:
                                        descriptionToInsert = "ACTUATOR TEST | IGNITION COIL BANK #5";
                                        valueToInsert = "RUNNING";
                                        break;
                                    case 0x25:
                                        descriptionToInsert = "ACTUATOR TEST | FUEL INJECTOR BANK #7";
                                        valueToInsert = "RUNNING";
                                        break;
                                    case 0x26:
                                        descriptionToInsert = "ACTUATOR TEST | FUEL INJECTOR BANK #8";
                                        valueToInsert = "RUNNING";
                                        break;
                                    case 0x28:
                                        descriptionToInsert = "ACTUATOR TEST | INTAKE HEATER BANK #1";
                                        valueToInsert = "RUNNING";
                                        break;
                                    case 0x29:
                                        descriptionToInsert = "ACTUATOR TEST | INTAKE HEATER BANK #2";
                                        valueToInsert = "RUNNING";
                                        break;
                                    case 0x2C:
                                        descriptionToInsert = "ACTUATOR TEST | SPEED CONTROL 12 VOLT FEED";
                                        valueToInsert = "RUNNING";
                                        break;
                                    case 0x2D:
                                        descriptionToInsert = "ACTUATOR TEST | INTAKE MANIFOLD TUNE VALVE";
                                        valueToInsert = "RUNNING";
                                        break;
                                    case 0x2E:
                                        descriptionToInsert = "ACTUATOR TEST | LOW SPEED RADIATOR FAN RELAY";
                                        valueToInsert = "RUNNING";
                                        break;
                                    case 0x2F:
                                        descriptionToInsert = "ACTUATOR TEST | HIGH SPEED RADIATOR FAN RELAY";
                                        valueToInsert = "RUNNING";
                                        break;
                                    case 0x30:
                                        descriptionToInsert = "ACTUATOR TEST | FUEL INJECTOR BANK #9";
                                        valueToInsert = "RUNNING";
                                        break;
                                    case 0x31:
                                        descriptionToInsert = "ACTUATOR TEST | FUEL INJECTOR BANK #10";
                                        valueToInsert = "RUNNING";
                                        break;
                                    case 0x32:
                                        descriptionToInsert = "ACTUATOR TEST | 2-3 LOCKOUT SOLENOID";
                                        valueToInsert = "RUNNING";
                                        break;
                                    case 0x33:
                                        descriptionToInsert = "ACTUATOR TEST | FUEL PUMP RELAY";
                                        valueToInsert = "RUNNING";
                                        break;
                                    case 0x3B:
                                        descriptionToInsert = "ACTUATOR TEST | IAC STEPPER MOTOR STEP UP";
                                        valueToInsert = "RUNNING";
                                        break;
                                    case 0x3C:
                                        descriptionToInsert = "ACTUATOR TEST | IAC STEPPER MOTOR STEP DOWN";
                                        valueToInsert = "RUNNING";
                                        break;
                                    case 0x3D:
                                        descriptionToInsert = "ACTUATOR TEST | LEAK DETECTION PUMP SOLENOID";
                                        valueToInsert = "RUNNING";
                                        break;
                                    case 0x40:
                                        descriptionToInsert = "ACTUATOR TEST | O2 SENSOR HEATER RELAY";
                                        valueToInsert = "RUNNING";
                                        break;
                                    case 0x41:
                                        descriptionToInsert = "ACTUATOR TEST | OVERDRIVE LAMP";
                                        valueToInsert = "RUNNING";
                                        break;
                                    case 0x43:
                                        descriptionToInsert = "ACTUATOR TEST | TRANSMISSION 12 VOLT RELAY";
                                        valueToInsert = "RUNNING";
                                        break;
                                    case 0x44:
                                        descriptionToInsert = "ACTUATOR TEST | REVERSE LOCKOUT SOLENOID";
                                        valueToInsert = "RUNNING";
                                        break;
                                    case 0x46:
                                        descriptionToInsert = "ACTUATOR TEST | SHORT RUNNER VALVE";
                                        valueToInsert = "RUNNING";
                                        break;
                                    case 0x49:
                                        descriptionToInsert = "ACTUATOR TEST | WAIT TO START LAMP";
                                        valueToInsert = "RUNNING";
                                        break;
                                    case 0x52:
                                        descriptionToInsert = "ACTUATOR TEST | 1/1 2/1 O2 SENSOR HEATER RELAY";
                                        valueToInsert = "RUNNING";
                                        break;
                                    case 0x53:
                                        descriptionToInsert = "ACTUATOR TEST | 1/2 2/2 O2 SENSOR HEATER RELAY";
                                        valueToInsert = "RUNNING";
                                        break;
                                    case 0x56:
                                        descriptionToInsert = "ACTUATOR TEST | 1/1 O2 SENSOR HEATER RELAY";
                                        valueToInsert = "RUNNING";
                                        break;
                                    case 0x57:
                                        descriptionToInsert = "ACTUATOR TEST | O2 SENSOR HEATER RELAY";
                                        valueToInsert = "RUNNING";
                                        break;
                                    case 0x5A:
                                        descriptionToInsert = "ACTUATOR TEST | RADIATOR FAN SOLENOID";
                                        valueToInsert = "RUNNING";
                                        break;
                                    case 0x5B:
                                        descriptionToInsert = "ACTUATOR TEST | 1/2 O2 SENSOR HEATER RELAY";
                                        valueToInsert = "RUNNING";
                                        break;
                                    case 0x5D:
                                        descriptionToInsert = "ACTUATOR TEST | EXHAUST BRAKE";
                                        valueToInsert = "RUNNING";
                                        break;
                                    case 0x5E:
                                        descriptionToInsert = "ACTUATOR TEST | FUEL CONTROL";
                                        valueToInsert = "RUNNING";
                                        break;
                                    case 0x5F:
                                        descriptionToInsert = "ACTUATOR TEST | PWM RADIATOR FAN";
                                        valueToInsert = "RUNNING";
                                        break;
                                    default:
                                        descriptionToInsert = "ACTUATOR TEST | OFFSET: " + Util.ByteToHexString(payload, 0, 1);
                                        valueToInsert = string.Empty;
                                        break;
                                }
                            }
                            else // error
                            {
                                descriptionToInsert = "ACTUATOR TEST";
                                valueToInsert = "ERROR";
                            }
                            unitToInsert = string.Empty;
                            break;
                        case 0x14: // request diagnostic data
                            if (message.Length >= minLength)
                            {
                                switch (payload[0])
                                {
                                    case 0x01: // ambient air temperature sensor
                                        descriptionToInsert = "AMBIENT AIR TEMPERATURE";

                                        if (MainForm.units == "imperial")
                                        {
                                            valueToInsert = payload[1].ToString("0");
                                            unitToInsert = "°F";
                                        }
                                        else if (MainForm.units == "metric")
                                        {
                                            valueToInsert = Math.Round((payload[1] * 0.555556D) - 17.77778D).ToString("0");
                                            unitToInsert = "°C";
                                        }

                                        break;
                                    case 0x02: // upstream (pre-cat) o2 sensor voltage
                                        descriptionToInsert = "UPSTREAM O2 SENSOR VOLTAGE (PRE-CATALISATOR)";
                                        valueToInsert = Math.Round(payload[1] * 0.019607843D, 3).ToString("0.000").Replace(",", ".");
                                        unitToInsert = "V";
                                        break;
                                    case 0x05: // engine coolant temperature
                                        descriptionToInsert = "ENGINE COOLANT TEMPERATURE";

                                        if (MainForm.units == "imperial")
                                        {
                                            valueToInsert = Math.Round((payload[1] * 1.8D) - 198.4D).ToString("0");
                                            unitToInsert = "°F";
                                        }
                                        else if (MainForm.units == "metric")
                                        {
                                            valueToInsert = (payload[1] - 128).ToString("0");
                                            unitToInsert = "°C";
                                        }
                                        break;
                                    case 0x06: // engine coolant temperature sensor voltage
                                        descriptionToInsert = "ENGINE COOLANT TEMPERATURE SENSOR VOLTAGE";
                                        valueToInsert = Math.Round(payload[1] * 0.019607843D, 3).ToString("0.000").Replace(",", ".");
                                        unitToInsert = "V";
                                        break;
                                    case 0x07: // throttle position sensor voltage
                                        descriptionToInsert = "THROTTLE POSITION SENSOR VOLTAGE";
                                        valueToInsert = Math.Round(payload[1] * 0.019607843D, 3).ToString("0.000").Replace(",", ".");
                                        unitToInsert = "V";
                                        break;
                                    case 0x08: // minimum throttle position sensor voltage
                                        descriptionToInsert = "MINIMUM TPS VOLTAGE";
                                        valueToInsert = Math.Round(payload[1] * 0.019607843D, 3).ToString("0.000").Replace(",", ".");
                                        unitToInsert = "V";
                                        break;
                                    case 0x09: // knock sensor voltage
                                        descriptionToInsert = "KNOCK SENSOR VOLTAGE";
                                        valueToInsert = Math.Round(payload[1] * 0.019607843D, 3).ToString("0.000").Replace(",", ".");
                                        unitToInsert = "V";
                                        break;
                                    case 0x0A: // battery voltage
                                        descriptionToInsert = "BATTERY VOLTAGE";
                                        valueToInsert = Math.Round(payload[1] * 0.0592D, 1).ToString("0.0").Replace(",", ".");
                                        unitToInsert = "V";
                                        break;
                                    case 0x0B: // map value
                                        descriptionToInsert = "INTAKE MANIFOLD ABSOLUTE PRESSURE (MAP)";

                                        if (MainForm.units == "imperial")
                                        {
                                            valueToInsert = Math.Round(payload[1] * 0.059756D, 1).ToString("0.0").Replace(",", ".");
                                            unitToInsert = "PSI";
                                        }
                                        else if (MainForm.units == "metric")
                                        {
                                            valueToInsert = Math.Round(payload[1] * 0.059756D * 6.894757D, 1).ToString("0.0").Replace(",", ".");
                                            unitToInsert = "KPA";
                                        }

                                        break;
                                    case 0x0C: // target iac stepper motor position
                                        descriptionToInsert = "TARGET IAC STEPPER MOTOR POSITION";
                                        valueToInsert = payload[1].ToString("0");
                                        unitToInsert = string.Empty;
                                        break;
                                    case 0x0E: // adaptive fuel factor
                                        descriptionToInsert = "ADAPTIVE FUEL FACTOR";
                                        valueToInsert = payload[1].ToString("0");
                                        unitToInsert = string.Empty;
                                        break;
                                    case 0x0F: // barometric pressure sensor
                                        descriptionToInsert = "BAROMETRIC PRESSURE";

                                        if (MainForm.units == "imperial")
                                        {
                                            valueToInsert = Math.Round(payload[1] * 0.059756D, 1).ToString("0.0").Replace(",", ".");
                                            unitToInsert = "PSI";
                                        }
                                        else if (MainForm.units == "metric")
                                        {
                                            valueToInsert = Math.Round(payload[1] * 0.059756D * 6.894757D, 1).ToString("0.0").Replace(",", ".");
                                            unitToInsert = "KPA";
                                        }

                                        break;
                                    case 0x10: // minimum engine speed
                                        descriptionToInsert = "MINIMUM ENGINE SPEED (IDLE)";
                                        valueToInsert = (payload[1] * 32).ToString("0");
                                        unitToInsert = "RPM";
                                        break;
                                    case 0x11: // engine speed
                                        descriptionToInsert = "ENGINE SPEED";
                                        valueToInsert = (payload[1] * 32).ToString("0");
                                        unitToInsert = "RPM";
                                        break;
                                    case 0x12: // sync sense
                                        descriptionToInsert = "SYNC SENSE";

                                        if ((payload[1] & 0x10) == 0x10) valueToInsert = "IN SYNC";
                                        else if ((payload[1] & 0x10) != 0x10) valueToInsert = "ENGINE STOPPED";
                                        unitToInsert = string.Empty;

                                        break;
                                    case 0x13: // key-on cycles error 1
                                        descriptionToInsert = "KEY-ON CYCLES ERROR 1";
                                        valueToInsert = payload[1].ToString("0");
                                        unitToInsert = string.Empty;
                                        break;
                                    case 0x15: // spark advance
                                        descriptionToInsert = "SPARK ADVANCE";
                                        valueToInsert = Math.Round((payload[1] * 0.5D) - 64, 1).ToString("0.0").Replace(",", ".");
                                        unitToInsert = "DEG";
                                        break;
                                    case 0x16: // cylinder 1 retard
                                        descriptionToInsert = "CYLINDER 1 RETARD";
                                        valueToInsert = payload[1].ToString("0");
                                        unitToInsert = string.Empty;
                                        break;
                                    case 0x17: // cylinder 2 retard
                                        descriptionToInsert = "CYLINDER 2 RETARD";
                                        valueToInsert = payload[1].ToString("0");
                                        unitToInsert = string.Empty;
                                        break;
                                    case 0x18: // cylinder 3 retard
                                        descriptionToInsert = "CYLINDER 3 RETARD";
                                        valueToInsert = payload[1].ToString("0");
                                        unitToInsert = string.Empty;
                                        break;
                                    case 0x19: // cylinder 4 retard
                                        descriptionToInsert = "CYLINDER 4 RETARD";
                                        valueToInsert = payload[1].ToString("0");
                                        unitToInsert = string.Empty;
                                        break;
                                    case 0x1A: // target boost
                                        descriptionToInsert = "TARGET BOOST";

                                        if (MainForm.units == "imperial")
                                        {
                                            valueToInsert = Math.Round(payload[1] * 0.115294117D, 1).ToString("0.0").Replace(",", ".");
                                            unitToInsert = "PSI";
                                        }
                                        else if (MainForm.units == "metric")
                                        {
                                            valueToInsert = Math.Round((payload[1] * 0.115294117D) * 6.89475729D, 1).ToString("0.0").Replace(",", ".");
                                            unitToInsert = "KPA";
                                        }

                                        break;
                                    case 0x1D: // cruise target speed
                                        descriptionToInsert = "CRUISE TARGET SPEED";
                                        valueToInsert = payload[1].ToString("0");
                                        unitToInsert = string.Empty;
                                        break;
                                    case 0x1E: // key-on cycles error 2
                                        descriptionToInsert = "KEY-ON CYCLES ERROR 2";
                                        valueToInsert = payload[1].ToString("0");
                                        unitToInsert = string.Empty;
                                        break;
                                    case 0x1F: // key-on cycles error 3
                                        descriptionToInsert = "KEY-ON CYCLES ERROR 3";
                                        valueToInsert = payload[1].ToString("0");
                                        unitToInsert = string.Empty;
                                        break;
                                    case 0x20: // speed control status
                                        string lastCruiseCutoutReason = string.Empty;
                                        string cruiseDeniedReason = string.Empty;

                                        switch (payload[1] & 0xF0) // upper 4 bits encode last cutout reason 
                                        {
                                            case 0x00:
                                                lastCruiseCutoutReason = "ON/OFF SW";
                                                break;
                                            case 0x10:
                                                lastCruiseCutoutReason = "SPEED SEN";
                                                break;
                                            case 0x20:
                                                lastCruiseCutoutReason = "RPM LIMIT";
                                                break;
                                            case 0x30:
                                                lastCruiseCutoutReason = "BRAKE SW";
                                                break;
                                            case 0x40:
                                                lastCruiseCutoutReason = "P/N SW";
                                                break;
                                            case 0x50:
                                                lastCruiseCutoutReason = "RPM/SPEED";
                                                break;
                                            case 0x60:
                                                lastCruiseCutoutReason = "CLUTCH";
                                                break;
                                            case 0x70:
                                                lastCruiseCutoutReason = "S/C DTC";
                                                break;
                                            case 0x80:
                                                lastCruiseCutoutReason = "KEY OFF";
                                                break;
                                            case 0x90:
                                                lastCruiseCutoutReason = "ACTIVE";
                                                break;
                                            case 0xA0:
                                                lastCruiseCutoutReason = "CLUTCH UP";
                                                break;
                                            case 0xB0:
                                                lastCruiseCutoutReason = "N/A";
                                                break;
                                            case 0xC0:
                                                lastCruiseCutoutReason = "SW DTC";
                                                break;
                                            case 0xD0:
                                                lastCruiseCutoutReason = "CANCEL SW";
                                                break;
                                            case 0xE0:
                                                lastCruiseCutoutReason = "LIMP-IN";
                                                break;
                                            case 0xF0:
                                                lastCruiseCutoutReason = "12V DTC";
                                                break;
                                            default:
                                                lastCruiseCutoutReason = "N/A";
                                                break;
                                        }

                                        switch (payload[1] & 0x0F) // lower 4 bits encode denied reason 
                                        {
                                            case 0x00:
                                                cruiseDeniedReason = "ON/OFF SW";
                                                break;
                                            case 0x01:
                                                cruiseDeniedReason = "SPEED SEN";
                                                break;
                                            case 0x02:
                                                cruiseDeniedReason = "RPM LIMIT";
                                                break;
                                            case 0x03:
                                                cruiseDeniedReason = "BRAKE SW";
                                                break;
                                            case 0x04:
                                                cruiseDeniedReason = "P/N SW";
                                                break;
                                            case 0x05:
                                                cruiseDeniedReason = "RPM/SPEED";
                                                break;
                                            case 0x06:
                                                cruiseDeniedReason = "CLUTCH";
                                                break;
                                            case 0x07:
                                                cruiseDeniedReason = "S/C DTC";
                                                break;
                                            case 0x08:
                                                cruiseDeniedReason = "ALLOWED";
                                                break;
                                            case 0x09:
                                                cruiseDeniedReason = "ACTIVE";
                                                break;
                                            case 0x0A:
                                                cruiseDeniedReason = "CLUTCH UP";
                                                break;
                                            case 0x0B:
                                                cruiseDeniedReason = "N/A";
                                                break;
                                            case 0x0C:
                                                cruiseDeniedReason = "SW DTC";
                                                break;
                                            case 0x0D:
                                                cruiseDeniedReason = "CANCEL SW";
                                                break;
                                            case 0x0E:
                                                cruiseDeniedReason = "LIMP-IN";
                                                break;
                                            case 0x0F:
                                                cruiseDeniedReason = "12V DTC";
                                                break;
                                            default:
                                                cruiseDeniedReason = "N/A";
                                                break;
                                        }

                                        if ((payload[1] & 0x0F) == 0x08)
                                        {
                                            descriptionToInsert = "CRUISE | LAST CUTOUT: " + lastCruiseCutoutReason + " | STATE: " + cruiseDeniedReason;
                                            valueToInsert = "STOPPED";
                                        }
                                        else if ((payload[1] & 0x0F) == 0x09)
                                        {
                                            descriptionToInsert = "CRUISE | LAST CUTOUT: " + lastCruiseCutoutReason + " | STATE: " + cruiseDeniedReason;
                                            valueToInsert = "ENGAGED";
                                        }
                                        else
                                        {
                                            descriptionToInsert = "CRUISE | LAST CUTOUT: " + lastCruiseCutoutReason + " | DENIED: " + cruiseDeniedReason;
                                            valueToInsert = "STOPPED";
                                        }
                                        unitToInsert = string.Empty;
                                        break;
                                    case 0x24: // target battery charging voltage
                                        descriptionToInsert = "TARGET BATTERY CHARGING VOLTAGE";
                                        valueToInsert = Math.Round(payload[1] * 0.0592D, 1).ToString("0.0").Replace(",", ".");
                                        unitToInsert = "V";
                                        break;
                                    case 0x25: // over 5 psi boost timer
                                        descriptionToInsert = "OVER 5 PSI BOOST TIMER";
                                        valueToInsert = payload[1].ToString("0");
                                        unitToInsert = string.Empty;
                                        break;
                                    case 0x26:
                                    case 0x28: // wastegate duty cycle
                                        descriptionToInsert = "WASTEGATE DUTY CYCLE";
                                        valueToInsert = Math.Round(payload[1] * 0.5D, 1).ToString("0.0").Replace(",", ".");
                                        unitToInsert = "%";
                                        break;
                                    case 0x27: // theft alarm status
                                        descriptionToInsert = "THEFT ALARM STATUS";
                                        valueToInsert = Convert.ToString(payload[1], 2).PadLeft(8, '0');
                                        unitToInsert = string.Empty;
                                        break;
                                    case 0x32: // A/C high side pressure sensor voltage
                                        descriptionToInsert = "A/C HIGH SIDE PRESSURE SENSOR VOLTAGE";
                                        valueToInsert = Math.Round(payload[1] * 0.019607843D, 3).ToString("0.000").Replace(",", ".");
                                        unitToInsert = "V";
                                        break;
                                    case 0x33: // A/C high side pressure
                                        descriptionToInsert = "A/C HIGH SIDE PRESSURE SENSOR";

                                        if (MainForm.units == "imperial")
                                        {
                                            valueToInsert = Math.Round(payload[1] * 1.961D, 1).ToString("0.0").Replace(",", ".");
                                            unitToInsert = "PSI";
                                        }
                                        else if (MainForm.units == "metric")
                                        {
                                            valueToInsert = Math.Round(payload[1] * 1.961D * 6.894757D, 1).ToString("0.0").Replace(",", ".");
                                            unitToInsert = "KPA";
                                        }

                                        break;
                                    case 0x40: // intake map sensor volts
                                        descriptionToInsert = "INTAKE MAP SENSOR VOLTAGE";
                                        valueToInsert = Math.Round(payload[1] * 0.019607843D, 3).ToString("0.000").Replace(",", ".");
                                        unitToInsert = "V";
                                        break;
                                    case 0x41: // vehicle speed
                                        descriptionToInsert = "VEHICLE SPEED";

                                        if (MainForm.units == "imperial")
                                        {
                                            valueToInsert = Math.Round(payload[1] / 2.0D).ToString("0");
                                            unitToInsert = "MPH";
                                        }
                                        else if (MainForm.units == "metric")
                                        {
                                            valueToInsert = Math.Round(payload[1] / 2.0D * 1.609344D).ToString("0");
                                            unitToInsert = "KM/H";
                                        }

                                        break;
                                    case 0x42: // upstream (pre-cat) o2 sensor level
                                        descriptionToInsert = "UPSTREAM O2 SENSOR LEVEL (PRE-CATALISATOR)";

                                        switch (payload[1])
                                        {
                                            case 0xA0:
                                                valueToInsert = "LEAN";
                                                break;
                                            case 0xB1:
                                                valueToInsert = "RICH";
                                                break;
                                            case 0xFF:
                                                valueToInsert = "CENTER";
                                                break;
                                            default:
                                                valueToInsert = "N/A";
                                                break;
                                        }

                                        unitToInsert = string.Empty;
                                        break;
                                    case 0x46: // throttle position sensor
                                        descriptionToInsert = "THROTTLE POSITION SENSOR";
                                        valueToInsert = Math.Round(payload[1] * 0.3922D, 1).ToString("0.0").Replace(",", ".");
                                        unitToInsert = "%";
                                        break;
                                    case 0x48: // downstream (post-cat) o2 sensor level
                                        descriptionToInsert = "DOWNSTREAM O2 SENSOR LEVEL (POST-CATALISATOR)";

                                        switch (payload[1])
                                        {
                                            case 0xA0:
                                                valueToInsert = "LEAN";
                                                break;
                                            case 0xB1:
                                                valueToInsert = "RICH";
                                                break;
                                            case 0xFF:
                                                valueToInsert = "CENTER";
                                                break;
                                            default:
                                                valueToInsert = "N/A";
                                                break;
                                        }

                                        unitToInsert = string.Empty;
                                        break;
                                    case 0x4E: // fuel level sensor voltage
                                        descriptionToInsert = "FUEL LEVEL SENSOR VOLTAGE";
                                        valueToInsert = Math.Round(payload[1] * 0.019607843D, 3).ToString("0.000").Replace(",", ".");
                                        unitToInsert = "V";
                                        break;
                                    case 0x4F: // fuel level
                                        descriptionToInsert = "FUEL LEVEL";

                                        if (MainForm.units == "imperial")
                                        {
                                            valueToInsert = Math.Round(payload[1] * 0.125D, 1).ToString("0.0").Replace(",", ".");
                                            unitToInsert = "GALLON";
                                        }
                                        else if (MainForm.units == "metric")
                                        {
                                            valueToInsert = Math.Round(payload[1] * 0.125D * 3.785412D, 1).ToString("0.0").Replace(",", ".");
                                            unitToInsert = "LITER";
                                        }

                                        break;
                                    case 0x5C: // calculated engine load
                                        descriptionToInsert = "CALCULATED ENGINE LOAD";
                                        valueToInsert = Math.Round(payload[1] * 0.3922D, 1).ToString("0.0").Replace(",", ".");
                                        unitToInsert = "%";
                                        break;
                                    case 0x5A: // output shaft speed
                                        descriptionToInsert = "OUTPUT SHAFT SPEED";
                                        valueToInsert = (payload[1] * 20).ToString("0.0").Replace(",", ".");
                                        unitToInsert = "RPM";
                                        break;
                                    case 0x5B: // governor pressure duty cycle
                                        descriptionToInsert = "GOVERNOR PRESSURE DUTY CYCLE";
                                        valueToInsert = Math.Round(payload[1] * 0.3922D, 1).ToString("0.0").Replace(",", ".");
                                        unitToInsert = "%";
                                        break;
                                    case 0x6D: // T-case switch voltage
                                        descriptionToInsert = "T-CASE SWITCH VOLTAGE";
                                        valueToInsert = Math.Round(payload[1] * 0.019607843D, 3).ToString("0.000").Replace(",", ".");
                                        unitToInsert = "V";
                                        break;
                                    case 0x7A: // FCA current
                                        descriptionToInsert = "FCA CURRENT";
                                        valueToInsert = Math.Round(payload[1] * 0.019607843D, 1).ToString("0.0").Replace(",", ".");
                                        unitToInsert = "A";
                                        break;
                                    case 0x7C: // oil temperature sensor voltage
                                        descriptionToInsert = "OIL TEMPERATURE SENSOR VOLTAGE";
                                        valueToInsert = Math.Round(payload[1] * 0.019607843D, 3).ToString("0.000").Replace(",", ".");
                                        unitToInsert = "V";
                                        break;
                                    case 0x7D: // oil temperature sensor
                                        descriptionToInsert = "OIL TEMPERATURE SENSOR";

                                        if (MainForm.units == "imperial")
                                        {
                                            valueToInsert = Math.Round((payload[1] * 1.8D) - 83.2D).ToString("0");
                                            unitToInsert = "°F";
                                        }
                                        else if (MainForm.units == "metric")
                                        {
                                            valueToInsert = (payload[1] - 64).ToString("0");
                                            unitToInsert = "°C";
                                        }

                                        break;
                                    default:
                                        descriptionToInsert = "REQUEST DIAGNOSTIC DATA | OFFSET: " + Util.ByteToHexString(payload, 0, 1);
                                        valueToInsert = Util.ByteToHexString(payload, 1, payload.Length - 1);
                                        unitToInsert = string.Empty;
                                        break;
                                }
                            }
                            else // error
                            {
                                descriptionToInsert = "REQUEST DIAGNOSTIC DATA";
                                valueToInsert = "ERROR";
                                unitToInsert = string.Empty;
                            }
                            break;
                        case 0x15: // ROM value
                            if (message.Length >= minLength)
                            {
                                descriptionToInsert = "ROM VALUE | OFFSET: " + Util.ByteToHexString(payload, 0, 2);
                                valueToInsert = Util.ByteToHexString(payload, 2, 1);
                            }
                            else // error
                            {
                                descriptionToInsert = "ROM VALUE";
                                valueToInsert = "ERROR";
                            }
                            unitToInsert = string.Empty;
                            break;
                        case 0x16: // ROM constant value
                            if (message.Length >= minLength)
                            {
                                ushort offset = (ushort)(payload[0] + 0x8000);
                                byte[] offsetArray = new byte[2];
                                offsetArray[0] = (byte)((offset >> 8) & 0xFF);
                                offsetArray[1] = (byte)(offset & 0xFF);

                                descriptionToInsert = "ROM CONSTANT VALUE | OFFSET: " + Util.ByteToHexStringSimple(offsetArray);
                                valueToInsert = Util.ByteToHexString(payload, 1, 1);
                            }
                            else // error
                            {
                                descriptionToInsert = "ROM CONSTANT VALUE";
                                valueToInsert = "ERROR";
                            }
                            unitToInsert = string.Empty;
                            break;
                        case 0x17: // erase fault codes
                            if (message.Length >= minLength)
                            {
                                if (payload[0] == 0xE0) valueToInsert = "ERASED";
                                else valueToInsert = "FAILED";
                            }
                            else // error
                            {
                                valueToInsert = "ERROR";
                            }
                            unitToInsert = string.Empty;
                            break;
                        case 0x18: // control ASD relay
                            if (message.Length >= minLength)
                            {
                                // TODO
                            }
                            else // error
                            {
                                valueToInsert = "ERROR";
                            }
                            unitToInsert = string.Empty;
                            break;
                        case 0x19: // set minimum idle speed
                            if (message.Length >= minLength)
                            {
                                valueToInsert = Math.Round(payload[0] * 7.85D).ToString("0");

                                if (payload[0] < 0x42)
                                {
                                    valueToInsert += " - TOO LOW!";
                                }

                                unitToInsert = "RPM";
                            }
                            else // error
                            {
                                valueToInsert = "ERROR";
                                unitToInsert = string.Empty;
                            }
                            break;
                        case 0x1A: // switch test
                            if (message.Length >= minLength)
                            {
                                List<string> switchList = new List<string>();

                                switch (payload[0])
                                {
                                    case 0x01:
                                        switchList.Clear();
                                        if (Util.IsBitSet(payload[1], 0)) switchList.Add("WAIT TO START LAMP");
                                        if (Util.IsBitSet(payload[1], 1)) switchList.Add("INTAKE HEATER #1");
                                        if (Util.IsBitSet(payload[1], 2)) switchList.Add("INTAKE HEATER #2");
                                        if (Util.IsBitSet(payload[1], 3)) switchList.Add("IDLE VALIDATION SW1");
                                        if (Util.IsBitSet(payload[1], 4)) switchList.Add("IDLE VALIDATION SW2");
                                        if (Util.IsBitSet(payload[1], 5)) switchList.Add("IDLE SELECT");
                                        if (Util.IsBitSet(payload[1], 6)) switchList.Add("TRANSFER PMPDR");

                                        
                                        if (switchList.Count > 0)
                                        {
                                            descriptionToInsert = "SWITCH TEST | ";

                                            foreach (string s in switchList)
                                            {
                                                descriptionToInsert += s + " | ";
                                            }

                                            descriptionToInsert = descriptionToInsert.Remove(descriptionToInsert.Length - 3); // remove last "|" character
                                        }
                                        else
                                        {
                                            descriptionToInsert = "SWITCH TEST";
                                        }
                                        break;
                                    case 0x02:
                                        switchList.Clear();
                                        if (Util.IsBitSet(payload[1], 0)) switchList.Add("INJ PUMP");
                                        if (Util.IsBitSet(payload[1], 2)) switchList.Add("A/C CLUTCH");
                                        if (Util.IsBitSet(payload[1], 3)) switchList.Add("EXHAUST BRAKE");
                                        if (Util.IsBitSet(payload[1], 4)) switchList.Add("BRAKE");
                                        if (Util.IsBitSet(payload[1], 5)) switchList.Add("EVAP PURGE");
                                        if (Util.IsBitSet(payload[1], 7)) switchList.Add("LOW OIL");

                                        if (switchList.Count > 0)
                                        {
                                            descriptionToInsert = "SWITCH TEST | ";

                                            foreach (string s in switchList)
                                            {
                                                descriptionToInsert += s + " | ";
                                            }

                                            descriptionToInsert = descriptionToInsert.Remove(descriptionToInsert.Length - 3); // remove last "|" character
                                        }
                                        else
                                        {
                                            descriptionToInsert = "SWITCH TEST";
                                        }
                                        break;
                                    case 0x03:
                                        switchList.Clear();
                                        if (Util.IsBitSet(payload[1], 1)) switchList.Add("MIL");
                                        if (Util.IsBitSet(payload[1], 2)) switchList.Add("GENERATOR LAMP");
                                        if (Util.IsBitSet(payload[1], 3)) switchList.Add("GENERATOR FIELD");
                                        if (Util.IsBitSet(payload[1], 4)) switchList.Add("12V FEED");
                                        if (Util.IsBitSet(payload[1], 6)) switchList.Add("TRANS O/D");
                                        if (Util.IsBitSet(payload[1], 7)) switchList.Add("TRANS TOW MODE");

                                        if (switchList.Count > 0)
                                        {
                                            descriptionToInsert = "SWITCH TEST | ";

                                            foreach (string s in switchList)
                                            {
                                                descriptionToInsert += s + " | ";
                                            }

                                            descriptionToInsert = descriptionToInsert.Remove(descriptionToInsert.Length - 3); // remove last "|" character
                                        }
                                        else
                                        {
                                            descriptionToInsert = "SWITCH TEST";
                                        }
                                        break;
                                    case 0x04:
                                        switchList.Clear();
                                        if (Util.IsBitSet(payload[1], 1)) switchList.Add("TRD LINK");
                                        if (Util.IsBitSet(payload[1], 4)) switchList.Add("ASD");
                                        if (Util.IsBitSet(payload[1], 6)) switchList.Add("IGNITION");

                                        if (switchList.Count > 0)
                                        {
                                            descriptionToInsert = "SWITCH TEST | ";

                                            foreach (string s in switchList)
                                            {
                                                descriptionToInsert += s + " | ";
                                            }

                                            descriptionToInsert = descriptionToInsert.Remove(descriptionToInsert.Length - 3); // remove last "|" character
                                        }
                                        else
                                        {
                                            descriptionToInsert = "SWITCH TEST";
                                        }
                                        break;
                                    default:
                                        descriptionToInsert = "SWITCH TEST | OFFSET: " + Util.ByteToHexString(payload, 0, 1);
                                        break;
                                }

                                valueToInsert = string.Empty;
                            }
                            else // error
                            {
                                valueToInsert = "ERROR";
                            }
                            unitToInsert = string.Empty;
                            break;
                        case 0x1B: // init byte mode download
                            valueToInsert = string.Empty;
                            unitToInsert = string.Empty;
                            break;
                        case 0x1C: // write memory
                            if (message.Length >= minLength)
                            {
                                switch (payload[0])
                                {
                                    case 0x10: // reset emr1
                                        descriptionToInsert = "WRITE MEMROY | RESET EMR 1";
                                        if ((payload[1] == 0x00) && (payload[2] == 0xFF)) valueToInsert = "OK";
                                        else valueToInsert = "FAILED";
                                        break;
                                    case 0x11: // reset emr2
                                        descriptionToInsert = "WRITE MEMROY | RESET EMR 2";
                                        if ((payload[1] == 0x00) && (payload[2] == 0xFF)) valueToInsert = "OK";
                                        else valueToInsert = "FAILED";
                                        break;
                                    case 0x1A: // var idle
                                        if (payload[1] == 0xFF)
                                        {
                                            descriptionToInsert = "WRITE MEMROY | ENABLE VAR IDLE";

                                            if (payload[2] == 0xFF)
                                            {
                                                valueToInsert = "OK";
                                            }
                                            else
                                            {
                                                valueToInsert = "FAILED";
                                            }
                                        }
                                        else if (payload[1] == 0x00)
                                        {
                                            descriptionToInsert = "WRITE MEMROY | DISABLE VAR IDLE";

                                            if (payload[2] == 0xFF)
                                            {
                                                valueToInsert = "OK";
                                            }
                                            else
                                            {
                                                valueToInsert = "FAILED";
                                            }
                                        }
                                        break;
                                    default:
                                        descriptionToInsert = "WRITE MEMROY | OFFSET: " + Util.ByteToHexString(payload, 0, 1);
                                        valueToInsert = Util.ByteToHexString(payload, 1, 1);
                                        break;
                                }
                            }
                            else // error
                            {
                                descriptionToInsert = "WRITE MEMROY";
                                valueToInsert = "ERROR";
                            }
                            unitToInsert = string.Empty;
                            break;
                        case 0x21: // set sync / timing / spark scatter
                            if (message.Length >= minLength)
                            {
                                switch (payload[0])
                                {
                                    case 0x00:
                                        descriptionToInsert = "UNKILL SPARK SCATTER";

                                        switch (payload[1])
                                        {
                                            case 0x00:
                                                valueToInsert = "TIMING ABOLISHED";
                                                break;
                                            case 0x01:
                                                valueToInsert = "TIMING INITIATED";
                                                break;
                                            case 0x02:
                                                valueToInsert = "REJECTED (OPEN THROTTLE)";
                                                break;
                                            case 0x03:
                                                valueToInsert = "REJECTED (SLP)";
                                                break;
                                            case 0x10:
                                                valueToInsert = "SYNC INITIATED";
                                                break;
                                            default:
                                                valueToInsert = "UNDEFINED";
                                                break;
                                        }

                                        break;
                                    case 0x01:
                                        descriptionToInsert = "KILL SPARK SCATTER";

                                        switch (payload[1])
                                        {
                                            case 0x00:
                                                valueToInsert = "TIMING ABOLISHED";
                                                break;
                                            case 0x01:
                                                valueToInsert = "TIMING INITIATED";
                                                break;
                                            case 0x02:
                                                valueToInsert = "REJECTED (OPEN THROTTLE)";
                                                break;
                                            case 0x03:
                                                valueToInsert = "REJECTED (SLP)";
                                                break;
                                            case 0x10:
                                                valueToInsert = "SYNC INITIATED";
                                                break;
                                            default:
                                                valueToInsert = "UNDEFINED";
                                                break;
                                        }

                                        break;
                                    case 0x10:
                                        descriptionToInsert = "SET SYNC MODE";

                                        switch (payload[1])
                                        {
                                            case 0x00:
                                                valueToInsert = "TIMING ABOLISHED";
                                                break;
                                            case 0x01:
                                                valueToInsert = "TIMING INITIATED";
                                                break;
                                            case 0x02:
                                                valueToInsert = "REJECTED (OPEN THROTTLE)";
                                                break;
                                            case 0x03:
                                                valueToInsert = "REJECTED (SLP)";
                                                break;
                                            case 0x10:
                                                valueToInsert = "SYNC INITIATED";
                                                break;
                                            default:
                                                valueToInsert = "UNDEFINED";
                                                break;
                                        }

                                        break;
                                    default:
                                        descriptionToInsert = "SET ENGINE PARAMETER | OFFSET: " + Util.ByteToHexString(payload, 0, 1);
                                        valueToInsert = Util.ByteToHexString(payload, 1, 1);
                                        break;
                                }
                            }
                            else // error
                            {
                                descriptionToInsert = "SET ENGINE PARAMETER";
                                valueToInsert = "ERROR";
                            }
                            unitToInsert = string.Empty;
                            break;
                        case 0x22: // send engine parameters
                            if (message.Length >= minLength)
                            {
                                switch (payload[0])
                                {
                                    case 0x01: // RPM
                                        descriptionToInsert = "ENGINE SPEED";
                                        valueToInsert = (payload[1] * 32).ToString("0");
                                        unitToInsert = "RPM";
                                        break;
                                    case 0x02: // injector pulsewidth
                                        descriptionToInsert = "INJECTOR PULSEWIDTH";
                                        valueToInsert = payload[1].ToString("0");
                                        unitToInsert = string.Empty;
                                        break;
                                    case 0x03: // target idle speed
                                        descriptionToInsert = "TARGET IDLE SPEED";
                                        valueToInsert = (payload[1] * 32).ToString("0");
                                        unitToInsert = "RPM";
                                        break;
                                    default:
                                        descriptionToInsert = "SEND ENGINE PARAMETER | OFFSET: " + Util.ByteToHexString(payload, 0, 1);
                                        valueToInsert = payload[1].ToString("0");
                                        unitToInsert = string.Empty;
                                        break;
                                }
                            }
                            else // error
                            {
                                descriptionToInsert = "SEND ENGINE PARAMETER";
                                valueToInsert = "ERROR";
                                unitToInsert = string.Empty;
                            }
                            break;
                        case 0x23: // reset adaptive memory
                            if (message.Length >= minLength)
                            {
                                switch (payload[0])
                                {
                                    case 0x01:
                                        descriptionToInsert = "ERASE ENGINE FAULT CODES / EEPROM";
                                        if (payload[1] == 0xFF)
                                        {
                                            valueToInsert = "SUCCESS";
                                        }
                                        else
                                        {
                                            valueToInsert = "FAILED";
                                        }
                                        unitToInsert = string.Empty;
                                        break;
                                    case 0x02:
                                        descriptionToInsert = "ADAPTIVE / RAM";
                                        if (payload[1] != 0x00)
                                        {
                                            valueToInsert = "ON";
                                        }
                                        else
                                        {
                                            valueToInsert = "OFF";
                                        }
                                        unitToInsert = string.Empty;
                                        break;
                                    case 0x03:
                                        descriptionToInsert = "IAC (IDLE AIR CONTROL) COUNTER";
                                        if (payload[1] != 0x00)
                                        {
                                            valueToInsert = "ON";
                                        }
                                        else
                                        {
                                            valueToInsert = "OFF";
                                        }
                                        unitToInsert = string.Empty;
                                        break;
                                    case 0x04:
                                        descriptionToInsert = "MINIMUM TPS (THROTTLE POSITION SENSOR)";
                                        if (payload[1] != 0x00)
                                        {
                                            valueToInsert = "ON";
                                        }
                                        else
                                        {
                                            valueToInsert = "OFF";
                                        }
                                        unitToInsert = string.Empty;
                                        break;
                                    case 0x05:
                                        descriptionToInsert = "FLEX FUEL PERCENT";
                                        valueToInsert = payload[1].ToString("0");
                                        unitToInsert = "%";
                                        break;
                                    case 0x06:
                                        descriptionToInsert = "CAM/CRANK & SYNC";
                                        if (payload[1] != 0x00)
                                        {
                                            valueToInsert = "ON (IN SYNC)";
                                        }
                                        else
                                        {
                                            valueToInsert = "OFF (NOT IN SYNC)";
                                        }
                                        unitToInsert = string.Empty;
                                        break;
                                    case 0x07:
                                        descriptionToInsert = "FUEL SHUTOFF";
                                        if (payload[1] != 0x00)
                                        {
                                            valueToInsert = "ON";
                                        }
                                        else
                                        {
                                            valueToInsert = "OFF";
                                        }
                                        unitToInsert = string.Empty;
                                        break;
                                    case 0x08:
                                        descriptionToInsert = "RUNTIME AT STALL";
                                        if (payload[1] != 0x00)
                                        {
                                            valueToInsert = "ON";
                                        }
                                        else
                                        {
                                            valueToInsert = "OFF";
                                        }
                                        unitToInsert = string.Empty;
                                        break;
                                    case 0x0B:
                                        descriptionToInsert = "RESET CAM/CRANK";
                                        switch (payload[1])
                                        {
                                            case 0x00:
                                                valueToInsert = "TURN OFF ENGINE";
                                                unitToInsert = "FAILED";
                                                break;
                                            case 0x01:
                                                valueToInsert = "COMMAND OOR";
                                                unitToInsert = "FAILED";
                                                break;
                                            case 0x02:
                                                valueToInsert = "DENIED WIP";
                                                unitToInsert = "FAILED";
                                                break;
                                            case 0xF0:
                                                valueToInsert = "COMPLETED";
                                                unitToInsert = "OK";
                                                break;
                                            default:
                                                valueToInsert = "UNRECOGNIZED";
                                                unitToInsert = "FAILED";
                                                break;
                                        }
                                        break;
                                    case 0x12:
                                        descriptionToInsert = "RESET ADAPTIVE NUMERATOR";
                                        switch (payload[1])
                                        {
                                            case 0x00:
                                                valueToInsert = "TURN OFF ENGINE";
                                                unitToInsert = "FAILED";
                                                break;
                                            case 0x01:
                                                valueToInsert = "BAD TEST ID";
                                                unitToInsert = "FAILED";
                                                break;
                                            case 0x02:
                                                valueToInsert = "MODULE BUSY";
                                                unitToInsert = "FAILED";
                                                break;
                                            case 0x03:
                                                valueToInsert = "SECURITY STATUS";
                                                unitToInsert = "FAILED";
                                                break;
                                            case 0xF0:
                                                valueToInsert = "EXECUTED";
                                                unitToInsert = "OK";
                                                break;
                                            default:
                                                valueToInsert = "UNRECOGNIZED";
                                                unitToInsert = "FAILED";
                                                break;
                                        }
                                        break;
                                    case 0x13:
                                        descriptionToInsert = "RESET SKIM";
                                        valueToInsert = payload[1].ToString("0");
                                        unitToInsert = string.Empty;
                                        break;
                                    case 0x14:
                                        descriptionToInsert = "RESET DUTY CYCLE MONITOR";
                                        switch (payload[1])
                                        {
                                            case 0x00:
                                                valueToInsert = "TURN OFF ENGINE";
                                                unitToInsert = "FAILED";
                                                break;
                                            case 0x01:
                                                valueToInsert = "BAD TEST ID";
                                                unitToInsert = "FAILED";
                                                break;
                                            case 0x02:
                                                valueToInsert = "MODULE BUSY";
                                                unitToInsert = "FAILED";
                                                break;
                                            case 0x03:
                                                valueToInsert = "SECURITY STATUS";
                                                unitToInsert = "FAILED";
                                                break;
                                            case 0xF0:
                                                valueToInsert = "EXECUTED";
                                                unitToInsert = "OK";
                                                break;
                                            default:
                                                valueToInsert = "UNRECOGNIZED";
                                                unitToInsert = "FAILED";
                                                break;
                                        }
                                        break;
                                    case 0x20:
                                        descriptionToInsert = "TPS ADAPTATION FOR ETC";
                                        if (payload[1] != 0x00)
                                        {
                                            valueToInsert = "ON";
                                        }
                                        else
                                        {
                                            valueToInsert = "OFF";
                                        }
                                        unitToInsert = string.Empty;
                                        break;
                                    case 0x21:
                                        descriptionToInsert = "MIN PEDAL VALUE";
                                        if (payload[1] != 0x00)
                                        {
                                            valueToInsert = "ON";
                                        }
                                        else
                                        {
                                            valueToInsert = "OFF";
                                        }
                                        unitToInsert = string.Empty;
                                        break;
                                    case 0x22:
                                        descriptionToInsert = "LEARNED KNOCK CORRECTION";
                                        if (payload[1] != 0x00)
                                        {
                                            valueToInsert = "ON";
                                        }
                                        else
                                        {
                                            valueToInsert = "OFF";
                                        }
                                        unitToInsert = string.Empty;
                                        break;
                                    case 0x23:
                                        descriptionToInsert = "LEARNED MISFIRE CORRECTION";
                                        if (payload[1] != 0x00)
                                        {
                                            valueToInsert = "ON";
                                        }
                                        else
                                        {
                                            valueToInsert = "OFF";
                                        }
                                        unitToInsert = string.Empty;
                                        break;
                                    case 0x24:
                                        descriptionToInsert = "IDLE ADAPTATION";
                                        if (payload[1] != 0x00)
                                        {
                                            valueToInsert = "ON";
                                        }
                                        else
                                        {
                                            valueToInsert = "OFF";
                                        }
                                        unitToInsert = string.Empty;
                                        break;
                                    default:
                                        descriptionToInsert = "ENGINE CONTROLLER REQUEST | OFFSET: " + Util.ByteToHexString(payload, 0, 1);
                                        valueToInsert = Util.ByteToHexString(payload, 1, 1);
                                        unitToInsert = string.Empty;
                                        break;
                                }
                            }
                            else // error
                            {
                                descriptionToInsert = "ENGINE CONTROLLER REQUEST";
                                valueToInsert = "ERROR";
                                unitToInsert = string.Empty;
                            }
                            break;
                        case 0x26: // read flash memory
                            if (message.Length >= minLength)
                            {
                                descriptionToInsert = "READ FLASH MEMORY | OFFSET: " + Util.ByteToHexString(payload, 0, 3);
                                valueToInsert = Util.ByteToHexString(payload, 3, 1);
                            }
                            else // error
                            {
                                descriptionToInsert = "READ FLASH MEMORY";
                                valueToInsert = "ERROR";
                            }
                            unitToInsert = string.Empty;
                            break;
                        case 0x27: // write flash memory
                            if (message.Length >= minLength)
                            {
                                valueToInsert = "NOT SUPPORTED";
                            }
                            else // error
                            {
                                valueToInsert = "ERROR";
                            }
                            unitToInsert = string.Empty;
                            break;
                        case 0x28: // read EEPROM
                            if (message.Length >= minLength)
                            {
                                descriptionToInsert = "READ EEPROM | OFFSET: ";
                                descriptionToInsert += Util.ByteToHexString(payload, 0, 2);
                                valueToInsert = Util.ByteToHexString(payload, 2, 1);
                            }
                            else // error
                            {
                                descriptionToInsert = "READ EEPROM";
                                valueToInsert = "ERROR";
                            }
                            unitToInsert = string.Empty;
                            break;
                        case 0x2A: // information request
                            if (message.Length >= minLength)
                            {
                                switch (payload[0])
                                {
                                    case 0x01:
                                        descriptionToInsert = "INFORMATION REQUEST | PCM PART NUMBER 1-2";
                                        valueToInsert = Util.ByteToHexString(payload, 1, 1);
                                        unitToInsert = string.Empty;
                                        break;
                                    case 0x02:
                                        descriptionToInsert = "INFORMATION REQUEST | PCM PART NUMBER 3-4";
                                        valueToInsert = Util.ByteToHexString(payload, 1, 1);
                                        unitToInsert = string.Empty;
                                        break;
                                    case 0x03:
                                        descriptionToInsert = "INFORMATION REQUEST | PCM PART NUMBER 5-6";
                                        valueToInsert = Util.ByteToHexString(payload, 1, 1);
                                        unitToInsert = string.Empty;
                                        break;
                                    case 0x04:
                                        descriptionToInsert = "INFORMATION REQUEST | PCM PART NUMBER 7-8";
                                        valueToInsert = Util.ByteToHexString(payload, 1, 1);
                                        unitToInsert = string.Empty;
                                        break;
                                    case 0x0A:
                                        descriptionToInsert = "INFORMATION REQUEST | FUEL TYPE";
                                        switch (payload[1])
                                        {
                                            case 0x01:
                                                valueToInsert = "UNLEADED GAS";
                                                break;
                                            case 0x02:
                                                valueToInsert = "DIESEL";
                                                break;
                                            case 0x03:
                                                valueToInsert = "PROPANE";
                                                break;
                                            case 0x04:
                                                valueToInsert = "METHANOL";
                                                break;
                                            case 0x05:
                                                valueToInsert = "LEADED GAS";
                                                break;
                                            case 0x06:
                                                valueToInsert = "FLEX";
                                                break;
                                            case 0x07:
                                                valueToInsert = "CNG";
                                                break;
                                            case 0x08:
                                                valueToInsert = "ELECTRIC";
                                                break;
                                            default:
                                                valueToInsert = "N/A";
                                                break;
                                        }
                                        unitToInsert = string.Empty;
                                        break;
                                    case 0x0B:
                                        descriptionToInsert = "INFORMATION REQUEST | MODEL YEAR";
                                        switch (payload[1])
                                        {
                                            case 0x01:
                                                valueToInsert = "1991";
                                                break;
                                            case 0x02:
                                                valueToInsert = "1992";
                                                break;
                                            case 0x03:
                                                valueToInsert = "1993";
                                                break;
                                            case 0x04:
                                                valueToInsert = "1994";
                                                break;
                                            case 0x05:
                                                valueToInsert = "1995";
                                                break;
                                            case 0x06:
                                                valueToInsert = "1996";
                                                break;
                                            case 0x07:
                                                valueToInsert = "1997";
                                                break;
                                            case 0x08:
                                                valueToInsert = "1998";
                                                break;
                                            case 0x09:
                                                valueToInsert = "1999";
                                                break;
                                            case 0x0A:
                                                valueToInsert = "2000";
                                                break;
                                            case 0x0B:
                                                valueToInsert = "2001";
                                                break;
                                            case 0x0C:
                                                valueToInsert = "2002";
                                                break;
                                            case 0x0D:
                                                valueToInsert = "2003";
                                                break;
                                            case 0x0E:
                                                valueToInsert = "2004";
                                                break;
                                            case 0x0F:
                                                valueToInsert = "2005";
                                                break;
                                            default:
                                                valueToInsert = "N/A";
                                                break;
                                        }
                                        unitToInsert = string.Empty;
                                        break;
                                    case 0x0C:
                                        descriptionToInsert = "INFORMATION REQUEST | ENGINE DISPLACEMENT/CONFIGURATION/ORIENTATION";
                                        switch (payload[1])
                                        {
                                            case 0x01:
                                                valueToInsert = "2.2 LITER";
                                                unitToInsert = "I4 E-W";
                                                break;
                                            case 0x02:
                                                valueToInsert = "2.5 LITER";
                                                unitToInsert = "I4 E-W";
                                                break;
                                            case 0x03:
                                                valueToInsert = "3.0 LITER";
                                                unitToInsert = "V6 E-W";
                                                break;
                                            case 0x04:
                                                valueToInsert = "3.3 LITER";
                                                unitToInsert = "V6 E-W";
                                                break;
                                            case 0x05:
                                                valueToInsert = "3.9 LITER";
                                                unitToInsert = "V6 N-S";
                                                break;
                                            case 0x06:
                                                valueToInsert = "5.2 LITER";
                                                unitToInsert = "V8 N-S";
                                                break;
                                            case 0x07:
                                                valueToInsert = "5.9 LITER";
                                                unitToInsert = "V8 N-S";
                                                break;
                                            case 0x08:
                                                valueToInsert = "3.8 LITER";
                                                unitToInsert = "V6 E-W";
                                                break;
                                            case 0x09:
                                                valueToInsert = "4.0 LITER";
                                                unitToInsert = "I6 N-S";
                                                break;
                                            case 0x0A:
                                                valueToInsert = "2.0 LITER";
                                                unitToInsert = "I4 E-W SOHC";
                                                break;
                                            case 0x0B:
                                                valueToInsert = "3.5 LITER";
                                                unitToInsert = "V6 N-S";
                                                break;
                                            case 0x0C:
                                                valueToInsert = "8.0 LITER";
                                                unitToInsert = "V10 N-S";
                                                break;
                                            case 0x0D:
                                                valueToInsert = "2.4 LITER";
                                                unitToInsert = "I4 E-W";
                                                break;
                                            case 0x0E:
                                                valueToInsert = "2.5 LITER";
                                                unitToInsert = "I4 N-S";
                                                break;
                                            case 0x0F:
                                                valueToInsert = "2.5 LITER";
                                                unitToInsert = "V6 N-S";
                                                break;
                                            case 0x10:
                                                valueToInsert = "2.0 LITER";
                                                unitToInsert = "I4 E-W DOHC";
                                                break;
                                            case 0x11:
                                                valueToInsert = "2.5 LITER";
                                                unitToInsert = "V6 E-W";
                                                break;
                                            case 0x12:
                                                valueToInsert = "5.9 LITER";
                                                unitToInsert = "I6 N-S";
                                                break;
                                            case 0x13:
                                                valueToInsert = "3.3 LITER";
                                                unitToInsert = "V6 N-S";
                                                break;
                                            case 0x14:
                                                valueToInsert = "2.7 LITER";
                                                unitToInsert = "V6 N-S";
                                                break;
                                            case 0x15:
                                                valueToInsert = "3.2 LITER";
                                                unitToInsert = "V6 N-S";
                                                break;
                                            case 0x16:
                                                valueToInsert = "1.8 LITER";
                                                unitToInsert = "I4 E-W";
                                                break;
                                            case 0x17:
                                                valueToInsert = "3.7 LITER";
                                                unitToInsert = "V6 N-S";
                                                break;
                                            case 0x18:
                                                valueToInsert = "4.7 LITER";
                                                unitToInsert = "V8 N-S";
                                                break;
                                            case 0x19:
                                                valueToInsert = "1.9 LITER";
                                                unitToInsert = "I4 E-W";
                                                break;
                                            case 0x1A:
                                                valueToInsert = "3.1 LITER";
                                                unitToInsert = "I5 N-S";
                                                break;
                                            case 0x1B:
                                                valueToInsert = "1.6 LITER";
                                                unitToInsert = "I4 E-W";
                                                break;
                                            case 0x1C:
                                                valueToInsert = "2.7 LITER";
                                                unitToInsert = "V6 E-W";
                                                break;
                                            default:
                                                valueToInsert = "N/A";
                                                unitToInsert = string.Empty;
                                                break;
                                        }
                                        break;
                                    default:
                                        if (payload[0] != 0x00)
                                        {
                                            descriptionToInsert = "INFORMATION REQUEST | " + "OFFSET: " + Util.ByteToHexString(payload, 0, 1);
                                            valueToInsert = Util.ByteToHexString(payload, 1, 1);
                                            unitToInsert = string.Empty;
                                        }
                                        else
                                        {
                                            descriptionToInsert = "INFORMATION REQUEST";
                                            valueToInsert = "STOPPED";
                                            unitToInsert = string.Empty;
                                        }
                                        break;
                                }
                            }
                            else // error
                            {
                                descriptionToInsert = "INFORMATION REQUEST";
                                valueToInsert = "ERROR";
                                unitToInsert = string.Empty;
                            }
                            break;
                        case 0x2B: // get security seed
                            if (message.Length >= minLength)
                            {
                                valueToInsert = Util.ByteToHexStringSimple(payload);
                            }
                            else // error
                            {
                                valueToInsert = "ERROR";
                            }
                            unitToInsert = string.Empty;
                            break;
                        case 0x2C: // send security seed
                            if (message.Length >= minLength)
                            {
                                switch (payload[3])
                                {
                                    case 0x02:
                                        valueToInsert = "FAILED";
                                        break;
                                    default:
                                        valueToInsert = "RESULT=" + Util.ByteToHexString(payload, 3, 1);
                                        break;
                                }
                            }
                            else // error
                            {
                                valueToInsert = "ERROR";
                            }
                            unitToInsert = string.Empty;
                            break;
                        case 0x2D: // min-max engine parameter value
                            if (message.Length >= minLength)
                            {
                                descriptionToInsert = "MIN/MAX ENGINE PARAMETER VALUE | OFFSET: " + Util.ByteToHexString(payload, 0, 1);
                                valueToInsert = Util.ByteToHexString(payload, 1, 1);
                            }
                            else // error
                            {
                                descriptionToInsert = "MIN/MAX ENGINE PARAMETER VALUE";
                                valueToInsert = "ERROR";
                            }
                            unitToInsert = string.Empty;
                            break;
                        case 0x2E: // engine fault codes
                        case 0x32:
                        case 0x33:
                            if (message.Length >= minLength)
                            {
                                valueToInsert = Util.ByteToHexStringSimple(payload);
                            }
                            else // error
                            {
                                valueToInsert = "ERROR";
                            }
                            unitToInsert = string.Empty;
                            break;
                        case 0xFE: // select low-speed mode
                            if (message.Length >= minLength)
                            {
                                valueToInsert = string.Empty;
                            }
                            else // error
                            {
                                valueToInsert = "ERROR";
                            }
                            unitToInsert = string.Empty;
                            break;
                        default:
                            descriptionToInsert = string.Empty;
                            valueToInsert = string.Empty;
                            unitToInsert = string.Empty;
                            break;
                    }
                }
                else if ((speed == "62500 baud") || (speed == "125000 baud"))
                {
                    switch (ID)
                    {
                        case 0xF0: // high-speed mode RAM table
                            if (payload.Length > 1)
                            {
                                ushort num = (ushort)(payload.Length / 2);
                                List<byte> offsets = new List<byte>();
                                List<byte> values = new List<byte>();
                                offsets.Clear();
                                values.Clear();

                                for (int i = 0; i < num; i++)
                                {
                                    offsets.Add(payload[i * 2]);
                                    values.Add(payload[(i * 2) + 1]);
                                }

                                descriptionToInsert = "F0 RAM TABLE VALUE | OFFSET: " + Util.ByteToHexStringSimple(offsets.ToArray());
                                valueToInsert = Util.ByteToHexStringSimple(values.ToArray());
                            }
                            else
                            {
                                descriptionToInsert = "F0 RAM TABLE SELECTED";
                                valueToInsert = string.Empty;
                            }
                            unitToInsert = string.Empty;
                            break;
                        case 0xF1: // high-speed mode RAM table
                            if (payload.Length > 1)
                            {
                                ushort num = (ushort)(payload.Length / 2);
                                List<byte> offsets = new List<byte>();
                                List<byte> values = new List<byte>();
                                offsets.Clear();
                                values.Clear();

                                for (int i = 0; i < num; i++)
                                {
                                    offsets.Add(payload[i * 2]);
                                    values.Add(payload[(i * 2) + 1]);
                                }

                                descriptionToInsert = "F1 RAM TABLE VALUE | OFFSET: " + Util.ByteToHexStringSimple(offsets.ToArray());
                                valueToInsert = Util.ByteToHexStringSimple(values.ToArray());
                            }
                            else
                            {
                                descriptionToInsert = "F1 RAM TABLE SELECTED";
                                valueToInsert = string.Empty;
                            }
                            unitToInsert = string.Empty;
                            break;
                        case 0xF2: // high-speed mode RAM table
                            if (payload.Length > 1)
                            {
                                ushort num = (ushort)(payload.Length / 2);
                                List<byte> offsets = new List<byte>();
                                List<byte> values = new List<byte>();
                                offsets.Clear();
                                values.Clear();

                                for (int i = 0; i < num; i++)
                                {
                                    offsets.Add(payload[i * 2]);
                                    values.Add(payload[(i * 2) + 1]);
                                }

                                descriptionToInsert = "F2 RAM TABLE VALUE | OFFSET: " + Util.ByteToHexStringSimple(offsets.ToArray());
                                valueToInsert = Util.ByteToHexStringSimple(values.ToArray());
                            }
                            else
                            {
                                descriptionToInsert = "F2 RAM TABLE SELECTED";
                                valueToInsert = string.Empty;
                            }
                            unitToInsert = string.Empty;
                            break;
                        case 0xF3: // high-speed mode RAM table
                            if (payload.Length > 1)
                            {
                                ushort num = (ushort)(payload.Length / 2);
                                List<byte> offsets = new List<byte>();
                                List<byte> values = new List<byte>();
                                offsets.Clear();
                                values.Clear();

                                for (int i = 0; i < num; i++)
                                {
                                    offsets.Add(payload[i * 2]);
                                    values.Add(payload[(i * 2) + 1]);
                                }

                                descriptionToInsert = "F3 RAM TABLE VALUE | OFFSET: " + Util.ByteToHexStringSimple(offsets.ToArray());
                                valueToInsert = Util.ByteToHexStringSimple(values.ToArray());
                            }
                            else
                            {
                                descriptionToInsert = "F3 RAM TABLE SELECTED";
                                valueToInsert = string.Empty;
                            }
                            unitToInsert = string.Empty;
                            break;
                        case 0xF4: // high-speed mode RAM table
                            if (payload.Length > 1)
                            {
                                switch (payload[0]) // RAM offset
                                {
                                    case 0x01:
                                        descriptionToInsert = "DIAGNOSTIC TROUBLE CODE";

                                        if (payload.Length >= 2)
                                        {
                                            valueToInsert = Util.ByteToHexString(payload, 1, 1);
                                        }
                                        else // error
                                        {
                                            valueToInsert = "ERROR";
                                        }

                                        unitToInsert = string.Empty;
                                        break;
                                    case 0x0A:
                                        descriptionToInsert = "ENGINE SPEED";

                                        if ((payload.Length >= 4) && (payload[2] == 0x0B))
                                        {
                                            valueToInsert = Math.Round(((payload[1] << 8) + payload[3]) * 0.125D, 3).ToString("0.000").Replace(",", ".");
                                            unitToInsert = "RPM";
                                        }
                                        else // error
                                        {
                                            valueToInsert = "ERROR";
                                            unitToInsert = string.Empty;
                                        }
                                        break;
                                    case 0x0C:
                                        descriptionToInsert = "VEHICLE SPEED";

                                        if ((payload.Length >= 4) && (payload[2] == 0x0D))
                                        {
                                            if (MainForm.units == "imperial")
                                            {
                                                valueToInsert = Math.Round(((payload[1] << 8) + payload[3]) * 0.0156D, 3).ToString("0.000").Replace(",", ".");
                                                unitToInsert = "MPH";
                                            }
                                            else if (MainForm.units == "metric")
                                            {
                                                valueToInsert = Math.Round(((payload[1] << 8) + payload[3]) * 0.0156D * 1.609344D, 3).ToString("0.000").Replace(",", ".");
                                                unitToInsert = "KM/H";
                                            }
                                        }
                                        else // error
                                        {
                                            valueToInsert = "ERROR";
                                            unitToInsert = string.Empty;
                                        }
                                        break;
                                    case 0x0E:
                                        descriptionToInsert = "CRUISE | CONTROL SWITCH PRESSED";

                                        if (payload.Length >= 2)
                                        {
                                            List<string> switchList = new List<string>();
                                            switchList.Clear();

                                            if (Util.IsBitSet(payload[1], 0)) switchList.Add("CANCEL");
                                            if (Util.IsBitSet(payload[1], 1)) switchList.Add("COAST");
                                            if (Util.IsBitClear(payload[1], 2)) switchList.Add("SET");
                                            if (Util.IsBitSet(payload[1], 3)) switchList.Add("ACC/RES");
                                            if (Util.IsBitSet(payload[1], 4)) switchList.Add("ON/OFF");
                                            if (Util.IsBitSet(payload[1], 7)) switchList.Add("BRAKE");

                                            if (switchList.Count > 0)
                                            {
                                                foreach (string s in switchList)
                                                {
                                                    valueToInsert += s + " | ";
                                                }

                                                valueToInsert = valueToInsert.Remove(valueToInsert.Length - 3); // remove last "|" character
                                            }
                                            else
                                            {
                                                valueToInsert = string.Empty;
                                            }
                                        }
                                        else // error
                                        {
                                            valueToInsert = "ERROR";
                                        }

                                        unitToInsert = string.Empty;
                                        break;
                                    case 0x0F:
                                        descriptionToInsert = "BATTERY VOLTAGE";

                                        if (payload.Length >= 2)
                                        {
                                            valueToInsert = Math.Round(payload[1] * 0.0618D, 3).ToString("0.000").Replace(",", ".");
                                            unitToInsert = "V";
                                        }
                                        else // error
                                        {
                                            valueToInsert = "ERROR";
                                            unitToInsert = string.Empty;
                                        }
                                        break;
                                    case 0x10:
                                        descriptionToInsert = "AMBIENT TEMPERATURE SENSOR VOLTAGE";

                                        if (payload.Length >= 2)
                                        {
                                            valueToInsert = Math.Round(payload[1] * 0.0196D, 3).ToString("0.000").Replace(",", ".");
                                            unitToInsert = "V";
                                        }
                                        else // error
                                        {
                                            valueToInsert = "ERROR";
                                            unitToInsert = string.Empty;
                                        }
                                        break;
                                    case 0x11:
                                        descriptionToInsert = "AMBIENT TEMPERATURE";

                                        if (payload.Length >= 2)
                                        {
                                            if (MainForm.units == "imperial")
                                            {
                                                valueToInsert = Math.Round((payload[1] * 1.8D) - 198.4D).ToString("0");
                                                unitToInsert = "°F";
                                            }
                                            else if (MainForm.units == "metric")
                                            {
                                                valueToInsert = (payload[1] - 128).ToString("0");
                                                unitToInsert = "°C";
                                            }
                                        }
                                        else // error
                                        {
                                            valueToInsert = "ERROR";
                                            unitToInsert = string.Empty;
                                        }
                                        break;
                                    case 0x12:
                                        descriptionToInsert = "THROTTLE POSITION SENSOR (TPS) VOLTAGE";

                                        if (payload.Length >= 2)
                                        {
                                            valueToInsert = Math.Round(payload[1] * 0.0196D, 3).ToString("0.000").Replace(",", ".");
                                            unitToInsert = "V";
                                        }
                                        else // error
                                        {
                                            valueToInsert = "ERROR";
                                            unitToInsert = string.Empty;
                                        }
                                        break;
                                    case 0x13:
                                        descriptionToInsert = "MINIMUM TPS VOLTAGE";

                                        if (payload.Length >= 2)
                                        {
                                            valueToInsert = Math.Round(payload[1] * 0.0196D, 3).ToString("0.000").Replace(",", ".");
                                            unitToInsert = "V";
                                        }
                                        else // error
                                        {
                                            valueToInsert = "ERROR";
                                            unitToInsert = string.Empty;
                                        }
                                        break;
                                    case 0x14:
                                        descriptionToInsert = "CALCULATED TPS VOLTAGE";

                                        if (payload.Length >= 2)
                                        {
                                            valueToInsert = Math.Round(payload[1] * 0.0196D, 3).ToString("0.000").Replace(",", ".");
                                            unitToInsert = "V";
                                        }
                                        else // error
                                        {
                                            valueToInsert = "ERROR";
                                            unitToInsert = string.Empty;
                                        }
                                        break;
                                    case 0x15:
                                        descriptionToInsert = "ENGINE COOLANT TEMPERATURE SENSOR VOLTAGE";

                                        if (payload.Length >= 2)
                                        {
                                            valueToInsert = Math.Round(payload[1] * 0.0196D, 3).ToString("0.000").Replace(",", ".");
                                            unitToInsert = "V";
                                        }
                                        else // error
                                        {
                                            valueToInsert = "ERROR";
                                            unitToInsert = string.Empty;
                                        }
                                        break;
                                    case 0x16:
                                        descriptionToInsert = "ENGINE COOLANT TEMPERATURE";

                                        if (payload.Length >= 2)
                                        {
                                            if (MainForm.units == "imperial")
                                            {
                                                valueToInsert = Math.Round((payload[1] * 1.8D) - 198.4D).ToString("0");
                                                unitToInsert = "°F";
                                            }
                                            else if (MainForm.units == "metric")
                                            {
                                                valueToInsert = (payload[1] - 128).ToString("0");
                                                unitToInsert = "°C";
                                            }
                                        }
                                        else // error
                                        {
                                            valueToInsert = "ERROR";
                                            unitToInsert = string.Empty;
                                        }
                                        break;
                                    case 0x21:
                                        descriptionToInsert = "CRUISE | SWITCH VOLTAGE";

                                        if (payload.Length >= 2)
                                        {
                                            valueToInsert = Math.Round(payload[1] * 0.0196D, 3).ToString("0.000").Replace(",", ".");
                                            unitToInsert = "V";
                                        }
                                        else // error
                                        {
                                            valueToInsert = "ERROR";
                                            unitToInsert = string.Empty;
                                        }
                                        break;
                                    case 0x3B:
                                        descriptionToInsert = "CRUISE | SET SPEED";

                                        if (payload.Length >= 2)
                                        {
                                            if (MainForm.units == "imperial")
                                            {
                                                valueToInsert = Math.Round(payload[1] / 2.0D, 1).ToString("0.0").Replace(",", ".");
                                                unitToInsert = "MPH";
                                            }
                                            else if (MainForm.units == "metric")
                                            {
                                                valueToInsert = Math.Round((payload[1] / 2.0D) * 1.609344D, 1).ToString("0.0").Replace(",", ".");
                                                unitToInsert = "KM/H";
                                            }
                                        }
                                        else // error
                                        {
                                            valueToInsert = "ERROR";
                                            unitToInsert = string.Empty;
                                        }
                                        break;
                                    case 0x3F:
                                        if (payload.Length >= 2)
                                        {
                                            string lastCruiseCutoutReason = string.Empty;
                                            string cruiseDeniedReason = string.Empty;

                                            switch (payload[1] & 0xF0) // upper 4 bits encode last cutout reason 
                                            {
                                                case 0x00:
                                                    lastCruiseCutoutReason = "ON/OFF SW";
                                                    break;
                                                case 0x10:
                                                    lastCruiseCutoutReason = "SPEED SEN";
                                                    break;
                                                case 0x20:
                                                    lastCruiseCutoutReason = "RPM LIMIT";
                                                    break;
                                                case 0x30:
                                                    lastCruiseCutoutReason = "BRAKE SW";
                                                    break;
                                                case 0x40:
                                                    lastCruiseCutoutReason = "P/N SW";
                                                    break;
                                                case 0x50:
                                                    lastCruiseCutoutReason = "RPM/SPEED";
                                                    break;
                                                case 0x60:
                                                    lastCruiseCutoutReason = "CLUTCH";
                                                    break;
                                                case 0x70:
                                                    lastCruiseCutoutReason = "S/C DTC";
                                                    break;
                                                case 0x80:
                                                    lastCruiseCutoutReason = "KEY OFF";
                                                    break;
                                                case 0x90:
                                                    lastCruiseCutoutReason = "ACTIVE";
                                                    break;
                                                case 0xA0:
                                                    lastCruiseCutoutReason = "CLUTCH UP";
                                                    break;
                                                case 0xB0:
                                                    lastCruiseCutoutReason = "N/A";
                                                    break;
                                                case 0xC0:
                                                    lastCruiseCutoutReason = "SW DTC";
                                                    break;
                                                case 0xD0:
                                                    lastCruiseCutoutReason = "CANCEL";
                                                    break;
                                                case 0xE0:
                                                    lastCruiseCutoutReason = "LIMP-IN";
                                                    break;
                                                case 0xF0:
                                                    lastCruiseCutoutReason = "12V DTC";
                                                    break;
                                                default:
                                                    lastCruiseCutoutReason = "N/A";
                                                    break;
                                            }

                                            switch (payload[1] & 0x0F) // lower 4 bits encode denied reason 
                                            {
                                                case 0x00:
                                                    cruiseDeniedReason = "ON/OFF SW";
                                                    break;
                                                case 0x01:
                                                    cruiseDeniedReason = "SPEED SEN";
                                                    break;
                                                case 0x02:
                                                    cruiseDeniedReason = "RPM LIMIT";
                                                    break;
                                                case 0x03:
                                                    cruiseDeniedReason = "BRAKE SW";
                                                    break;
                                                case 0x04:
                                                    cruiseDeniedReason = "P/N SW";
                                                    break;
                                                case 0x05:
                                                    cruiseDeniedReason = "RPM/SPEED";
                                                    break;
                                                case 0x06:
                                                    cruiseDeniedReason = "CLUTCH";
                                                    break;
                                                case 0x07:
                                                    cruiseDeniedReason = "S/C DTC";
                                                    break;
                                                case 0x08:
                                                    cruiseDeniedReason = "ALLOWED";
                                                    break;
                                                case 0x09:
                                                    cruiseDeniedReason = "ACTIVE";
                                                    break;
                                                case 0x0A:
                                                    cruiseDeniedReason = "CLUTCH UP";
                                                    break;
                                                case 0x0B:
                                                    cruiseDeniedReason = "N/A";
                                                    break;
                                                case 0x0C:
                                                    cruiseDeniedReason = "SW DTC";
                                                    break;
                                                case 0x0D:
                                                    cruiseDeniedReason = "CANCEL";
                                                    break;
                                                case 0x0E:
                                                    cruiseDeniedReason = "LIMP-IN";
                                                    break;
                                                case 0x0F:
                                                    cruiseDeniedReason = "12V DTC";
                                                    break;
                                                default:
                                                    cruiseDeniedReason = "N/A";
                                                    break;
                                            }

                                            if ((payload[1] & 0x0F) == 0x08)
                                            {
                                                descriptionToInsert = "CRUISE | LAST CUTOUT: " + lastCruiseCutoutReason + " | STATE: " + cruiseDeniedReason;
                                                valueToInsert = "STOPPED";
                                            }
                                            else if ((payload[1] & 0x0F) == 0x09)
                                            {
                                                descriptionToInsert = "CRUISE | LAST CUTOUT: " + lastCruiseCutoutReason + " | STATE: " + cruiseDeniedReason;
                                                valueToInsert = "ENGAGED";
                                            }
                                            else
                                            {
                                                descriptionToInsert = "CRUISE | LAST CUTOUT: " + lastCruiseCutoutReason + " | DENIED: " + cruiseDeniedReason;
                                                valueToInsert = "STOPPED";
                                            }

                                            unitToInsert = string.Empty;
                                        }
                                        else // error
                                        {
                                            descriptionToInsert = "CRUISE STATE";
                                            valueToInsert = "ERROR";
                                            unitToInsert = string.Empty;
                                        }
                                        break;
                                    default:
                                        ushort num = (ushort)(payload.Length / 2);
                                        List<byte> offsets = new List<byte>();
                                        List<byte> values = new List<byte>();
                                        offsets.Clear();
                                        values.Clear();

                                        for (int i = 0; i < num; i++)
                                        {
                                            offsets.Add(payload[i * 2]);
                                            values.Add(payload[(i * 2) + 1]);
                                        }

                                        descriptionToInsert = "F4 RAM TABLE VALUE | OFFSET: " + Util.ByteToHexStringSimple(offsets.ToArray());
                                        valueToInsert = Util.ByteToHexStringSimple(values.ToArray());
                                        break;
                                }
                            }
                            else // RAM table only
                            {
                                descriptionToInsert = "F4 RAM TABLE SELECTED";
                                valueToInsert = string.Empty;
                                unitToInsert = string.Empty;
                            }
                            break;
                        case 0xF5: // high-speed mode RAM table
                            if (payload.Length > 1)
                            {
                                ushort num = (ushort)(payload.Length / 2);
                                List<byte> offsets = new List<byte>();
                                List<byte> values = new List<byte>();
                                offsets.Clear();
                                values.Clear();

                                for (int i = 0; i < num; i++)
                                {
                                    offsets.Add(payload[i * 2]);
                                    values.Add(payload[(i * 2) + 1]);
                                }

                                descriptionToInsert = "F5 RAM TABLE VALUE | OFFSET: " + Util.ByteToHexStringSimple(offsets.ToArray());
                                valueToInsert = Util.ByteToHexStringSimple(values.ToArray());
                            }
                            else
                            {
                                descriptionToInsert = "F5 RAM TABLE SELECTED";
                                valueToInsert = string.Empty;
                            }
                            unitToInsert = string.Empty;
                            break;
                        case 0xF6: // high-speed mode RAM table
                            if (payload.Length > 1)
                            {
                                ushort num = (ushort)(payload.Length / 2);
                                List<byte> offsets = new List<byte>();
                                List<byte> values = new List<byte>();
                                offsets.Clear();
                                values.Clear();

                                for (int i = 0; i < num; i++)
                                {
                                    offsets.Add(payload[i * 2]);
                                    values.Add(payload[(i * 2) + 1]);
                                }

                                descriptionToInsert = "F6 RAM TABLE VALUE | OFFSET: " + Util.ByteToHexStringSimple(offsets.ToArray());
                                valueToInsert = Util.ByteToHexStringSimple(values.ToArray());
                            }
                            else
                            {
                                descriptionToInsert = "F6 RAM TABLE SELECTED";
                                valueToInsert = string.Empty;
                            }
                            unitToInsert = string.Empty;
                            break;
                        case 0xF7: // high-speed mode RAM table
                            if (payload.Length > 1)
                            {
                                ushort num = (ushort)(payload.Length / 2);
                                List<byte> offsets = new List<byte>();
                                List<byte> values = new List<byte>();
                                offsets.Clear();
                                values.Clear();

                                for (int i = 0; i < num; i++)
                                {
                                    offsets.Add(payload[i * 2]);
                                    values.Add(payload[(i * 2) + 1]);
                                }

                                descriptionToInsert = "F7 RAM TABLE VALUE | OFFSET: " + Util.ByteToHexStringSimple(offsets.ToArray());
                                valueToInsert = Util.ByteToHexStringSimple(values.ToArray());
                            }
                            else
                            {
                                descriptionToInsert = "F7 RAM TABLE SELECTED";
                                valueToInsert = string.Empty;
                            }
                            unitToInsert = string.Empty;
                            break;
                        case 0xF8: // high-speed mode RAM table
                            if (payload.Length > 1)
                            {
                                ushort num = (ushort)(payload.Length / 2);
                                List<byte> offsets = new List<byte>();
                                List<byte> values = new List<byte>();
                                offsets.Clear();
                                values.Clear();

                                for (int i = 0; i < num; i++)
                                {
                                    offsets.Add(payload[i * 2]);
                                    values.Add(payload[(i * 2) + 1]);
                                }

                                descriptionToInsert = "F8 RAM TABLE VALUE | OFFSET: " + Util.ByteToHexStringSimple(offsets.ToArray());
                                valueToInsert = Util.ByteToHexStringSimple(values.ToArray());
                            }
                            else
                            {
                                descriptionToInsert = "F8 RAM TABLE SELECTED";
                                valueToInsert = string.Empty;
                            }
                            unitToInsert = string.Empty;
                            break;
                        case 0xF9: // high-speed mode RAM table
                            if (payload.Length > 1)
                            {
                                ushort num = (ushort)(payload.Length / 2);
                                List<byte> offsets = new List<byte>();
                                List<byte> values = new List<byte>();
                                offsets.Clear();
                                values.Clear();

                                for (int i = 0; i < num; i++)
                                {
                                    offsets.Add(payload[i * 2]);
                                    values.Add(payload[(i * 2) + 1]);
                                }

                                descriptionToInsert = "F9 RAM TABLE VALUE | OFFSET: " + Util.ByteToHexStringSimple(offsets.ToArray());
                                valueToInsert = Util.ByteToHexStringSimple(values.ToArray());
                            }
                            else
                            {
                                descriptionToInsert = "F9 RAM TABLE SELECTED";
                                valueToInsert = string.Empty;
                            }
                            unitToInsert = string.Empty;
                            break;
                        case 0xFA: // high-speed mode RAM table
                            if (payload.Length > 1)
                            {
                                ushort num = (ushort)(payload.Length / 2);
                                List<byte> offsets = new List<byte>();
                                List<byte> values = new List<byte>();
                                offsets.Clear();
                                values.Clear();

                                for (int i = 0; i < num; i++)
                                {
                                    offsets.Add(payload[i * 2]);
                                    values.Add(payload[(i * 2) + 1]);
                                }

                                descriptionToInsert = "FA RAM TABLE VALUE | OFFSET: " + Util.ByteToHexStringSimple(offsets.ToArray());
                                valueToInsert = Util.ByteToHexStringSimple(values.ToArray());
                            }
                            else
                            {
                                descriptionToInsert = "FA RAM TABLE SELECTED";
                                valueToInsert = string.Empty;
                            }
                            unitToInsert = string.Empty;
                            break;
                        case 0xFB: // high-speed mode RAM table
                            if (payload.Length > 1)
                            {
                                ushort num = (ushort)(payload.Length / 2);
                                List<byte> offsets = new List<byte>();
                                List<byte> values = new List<byte>();
                                offsets.Clear();
                                values.Clear();

                                for (int i = 0; i < num; i++)
                                {
                                    offsets.Add(payload[i * 2]);
                                    values.Add(payload[(i * 2) + 1]);
                                }

                                descriptionToInsert = "FB RAM TABLE VALUE | OFFSET: " + Util.ByteToHexStringSimple(offsets.ToArray());
                                valueToInsert = Util.ByteToHexStringSimple(values.ToArray());
                            }
                            else
                            {
                                descriptionToInsert = "FB RAM TABLE SELECTED";
                                valueToInsert = string.Empty;
                            }
                            unitToInsert = string.Empty;
                            break;
                        case 0xFC: // high-speed mode RAM table
                            if (payload.Length > 1)
                            {
                                ushort num = (ushort)(payload.Length / 2);
                                List<byte> offsets = new List<byte>();
                                List<byte> values = new List<byte>();
                                offsets.Clear();
                                values.Clear();

                                for (int i = 0; i < num; i++)
                                {
                                    offsets.Add(payload[i * 2]);
                                    values.Add(payload[(i * 2) + 1]);
                                }

                                descriptionToInsert = "FC RAM TABLE VALUE | OFFSET: " + Util.ByteToHexStringSimple(offsets.ToArray());
                                valueToInsert = Util.ByteToHexStringSimple(values.ToArray());
                            }
                            else
                            {
                                descriptionToInsert = "FC RAM TABLE SELECTED";
                                valueToInsert = string.Empty;
                            }
                            unitToInsert = string.Empty;
                            break;
                        case 0xFD: // high-speed mode RAM table
                            if (payload.Length > 1)
                            {
                                ushort num = (ushort)(payload.Length / 2);
                                List<byte> offsets = new List<byte>();
                                List<byte> values = new List<byte>();
                                offsets.Clear();
                                values.Clear();

                                for (int i = 0; i < num; i++)
                                {
                                    offsets.Add(payload[i * 2]);
                                    values.Add(payload[(i * 2) + 1]);
                                }

                                descriptionToInsert = "FD RAM TABLE VALUE | OFFSET: " + Util.ByteToHexStringSimple(offsets.ToArray());
                                valueToInsert = Util.ByteToHexStringSimple(values.ToArray());
                            }
                            else
                            {
                                descriptionToInsert = "FD RAM TABLE SELECTED";
                                valueToInsert = string.Empty;
                            }
                            unitToInsert = string.Empty;
                            break;
                        case 0xFE: // select low-speed mode
                            if (message.Length >= minLength)
                            {
                                valueToInsert = string.Empty;
                            }
                            else // error
                            {
                                valueToInsert = "ERROR";
                            }
                            unitToInsert = string.Empty;
                            break;
                        case 0xFF: // high-speed mode RAM table
                            if (payload.Length > 1)
                            {
                                ushort num = (ushort)(payload.Length / 2);
                                List<byte> offsets = new List<byte>();
                                List<byte> values = new List<byte>();
                                offsets.Clear();
                                values.Clear();

                                for (int i = 0; i < num; i++)
                                {
                                    offsets.Add(payload[i * 2]);
                                    values.Add(payload[(i * 2) + 1]);
                                }

                                descriptionToInsert = "FF RAM TABLE VALUE | OFFSET: " + Util.ByteToHexStringSimple(offsets.ToArray());
                                valueToInsert = Util.ByteToHexStringSimple(values.ToArray());
                            }
                            else
                            {
                                descriptionToInsert = "FF RAM TABLE SELECTED";
                                valueToInsert = string.Empty;
                            }
                            unitToInsert = string.Empty;
                            break;
                        default:
                            descriptionToInsert = string.Empty;
                            valueToInsert = string.Empty;
                            unitToInsert = string.Empty;
                            break;
                    }
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

            //if (speed == "62500 baud") Diagnostics.AddRAMTableDump(data);

            UpdateHeader();

            // Save message in the log file.
            if (MainForm.includeTimestampInLogFiles)
            {
                TimeSpan ElapsedTime = TimeSpan.FromMilliseconds(timestamp[0] << 24 | timestamp[1] << 16 | timestamp[2] << 8 | timestamp[3]);
                DateTime Timestamp = DateTime.Today.Add(ElapsedTime);
                string TimestampString = Timestamp.ToString("HH:mm:ss.fff") + " ";
                File.AppendAllText(MainForm.PCMLogFilename, TimestampString); // no newline is appended!
            }

            File.AppendAllText(MainForm.PCMLogFilename, Util.ByteToHexStringSimple(message) + Environment.NewLine);

            if (!engineFaultCodesSaved)
            {
                StringBuilder sb = new StringBuilder();

                if (engineFaultCodeList.Count > 0)
                {
                    sb.Append("FAULT CODE LIST:" + Environment.NewLine);

                    foreach (byte code in engineFaultCodeList)
                    {
                        int index = EngineDTC.Rows.IndexOf(EngineDTC.Rows.Find(code));
                        byte[] temp = new byte[1] { code };

                        if (index > -1) // DTC description found
                        {
                            sb.Append(Util.ByteToHexStringSimple(temp) + ": " + EngineDTC.Rows[index]["description"] + Environment.NewLine);
                        }
                        else // no DTC description found
                        {
                            sb.Append(Util.ByteToHexStringSimple(temp) + ": -" + Environment.NewLine);
                        }
                    }

                    sb.Remove(sb.Length - 1, 1); // remove last newline character

                    File.AppendAllText(MainForm.PCMLogFilename, Environment.NewLine + sb.ToString() + Environment.NewLine);
                }
                else
                {
                    sb.Append("NO FAULT CODES");
                    File.AppendAllText(MainForm.PCMLogFilename, Environment.NewLine + sb.ToString() + Environment.NewLine + Environment.NewLine);
                }

                engineFaultCodesSaved = true;
            }
        }
    }
}
