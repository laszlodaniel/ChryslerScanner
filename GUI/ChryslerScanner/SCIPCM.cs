using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ChryslerScanner
{
    public class SCIPCM
    {
        public SCIPCMDiagnosticsTable Diagnostics = new SCIPCMDiagnosticsTable();
        public DataTable EngineDTC = new DataTable("EngineDTC");
        public List<byte> engineFaultCodeList = new List<byte>();
        public bool engineFaultCodesSaved = true;
        public List<byte> engineFaultCode1TList = new List<byte>();
        public bool engineFaultCodes1TSaved = true;
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
            row["description"] = "LEFT BANK O2 SENSOR STAYS ABOVE CENTER (RICH)";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x04;
            row["description"] = "LEFT BANK O2 SENSOR STAYS BELOW CENTER (LEAN)";
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
            row["description"] = "RIGHT BANK O2 SENSOR STAYS ABOVE CENTER (RICH)";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x09;
            row["description"] = "RIGHT BANK O2 SENSOR STAYS BELOW CENTER (LEAN)";
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
            row["description"] = "SYSTEM RICH, L-IDLE ADAPTIVE AT LEAN LIMIT";
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
            row["description"] = "LEFT BANK UPSTREAM O2 SENSOR SHORTED TO VOLTAGE";
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
            row["description"] = "RIGHT BANK UPSTREAM O2 SENSOR STAYS AT CENTER";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x42;
            row["description"] = "RIGHT BANK UPSTREAM O2 SENSOR SHORTED TO VOLTAGE";
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
            row["description"] = "FUEL SYSTEM LEAN, L-IDLE ADAPTIVE AT RICH LIMIT";
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
            row["description"] = "NO CCD/PCI BUS MESSAGES FROM TCM";
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
            row["description"] = "LEFT BANK UPSTREAM O2 SENSOR SLOW RESPONSE";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x67;
            row["description"] = "LEFT BANK UPSTREAM O2 SENSOR HEATER FAILURE";
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
            row["description"] = "MULTIPLE CYLINDER MISFIRE";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x6B;
            row["description"] = "CYLINDER #1 MISFIRE";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x6C;
            row["description"] = "CYLINDER #2 MISFIRE";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x6D;
            row["description"] = "CYLINDER #3 MISFIRE";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x6E;
            row["description"] = "CYLINDER #4 MISFIRE";
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
            row["description"] = "LEFT BANK FUEL SYSTEM RICH";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x77;
            row["description"] = "LEFT BANK FUEL SYSTEM LEAN";
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
            row["description"] = "LEFT BANK DOWNSTREAM O2 SENSOR STAYS AT CENTER";
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
            row["description"] = "BATTERY TEMPERATURE SENSOR VOLTS TOO LOW";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x9A;
            row["description"] = "BATTERY TEMPERATURE SENSOR VOLTS TOO HIGH";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x9B;
            row["description"] = "LEFT BANK UPSTREAM O2 SENSOR SHORTED TO GROUND";
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
            row["id"] = 0xAA;
            row["description"] = "PCM NOT PROGRAMMED";
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
            row["description"] = "MISFIRE ADAPTIVE NUMERATOR AT LIMIT";
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

            row = EngineDTC.NewRow();
            row["id"] = 0xC7;
            row["description"] = "AW4 TRANSMISSION SHIFT SOLENOID B FUNCTIONAL FAILURE";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xC8;
            row["description"] = "RADIATOR TEMPERATURE SENSOR VOLTS TOO LOW";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xC9;
            row["description"] = "RADIATOR TEMPERATURE SENSOR VOLTS TOO HIGH";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xCA;
            row["description"] = "NO I/P CLUSTER CCD/PCI BUS MESSAGES RECEIVED";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xCB;
            row["description"] = "AW4 TRANSMISSION INTERNAL FAILURE (ROM CHECK)";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xCC;
            row["description"] = "UPSTREAM O2 SENSOR SLOW RESPONSE";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xCD;
            row["description"] = "UPSTREAM O2 SENSOR HEATER FAILURE";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xCE;
            row["description"] = "UPSTREAM O2 SENSOR SHORTED TO VOLTAGE";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xCF;
            row["description"] = "UPSTREAM O2 SENSOR SHORTED TO GROUND";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xD0;
            row["description"] = "NO CAM SYNC SIGNAL AT PCM";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xD1;
            row["description"] = "GLOW PLUG RELAY CONTROL CIRCUIT";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xD2;
            row["description"] = "HIGH SPEED CONDENSER FAN CONTROL RELAY CIRCUIT";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xD3;
            row["description"] = "AW4 TRANSMISSION SHIFT SOLENOID B (2-3) SHORTED TO VOLTAGE (12V)";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xD4;
            row["description"] = "EGR POSITION SENSOR VOLTS TOO LOW";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xD5;
            row["description"] = "EGR POSITION SENSOR VOLTS TOO HIGH";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xD6;
            row["description"] = "NO 5 VOLTS TO EGR POSITION SENSOR";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xD7;
            row["description"] = "EGR POSITION SENSOR RATIONALITY FAILURE";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xD8;
            row["description"] = "IGNITION COIL #6 PRIMARY CIRCUIT";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xD9;
            row["description"] = "INTAKE MANIFOLD SHORT RUNNER SOLENOID CIRCUIT";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xDA;
            row["description"] = "AIR ASSIST INJECTION SOLENOID CIRCUIT";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xDB;
            row["description"] = "CATALYST TEMPERATURE SENSOR VOLTS TOO HIGH";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xDC;
            row["description"] = "CATALYST TEMPERATURE SENSOR VOLTS TOO LOW";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xDD;
            row["description"] = "EATX RPM PULSE PERFORMANCE CONDITION";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xDE;
            row["description"] = "NO BUS MESSAGE RECEIVED FROM COMPANION MODULE";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xDF;
            row["description"] = "MIL FAULT IN COMPANION MODULE";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xE0;
            row["description"] = "COOLANT TEMPERATURE SENSOR PERFORMANCE";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xE1;
            row["description"] = "NO MIC BUS MESSAGE";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xE2;
            row["description"] = "NO SKIM BUS MESSAGE RECEIVED";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xE3;
            row["description"] = "IGNITION COIL #7 PRIMARY CIRCUIT";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xE4;
            row["description"] = "IGNITION COIL #8 PRIMARY CIRCUIT";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xE5;
            row["description"] = "PCV SOLENOID CIRCUIT";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xE6;
            row["description"] = "TRANSMISSION FAN RELAY CIRCUIT";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xE7;
            row["description"] = "TCC OR O/D SOLENOID PERFORMANCE";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xE8;
            row["description"] = "WRONG OR INVALID KEY MESSAGE RECEIVED FROM SKIM";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xEA;
            row["description"] = "AW4 TRANSMISSION SOLENOID A 1-2/3-4 OR TCC SOLENOID C FUNCTIONAL FAIL";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xEB;
            row["description"] = "AW4 TRANSMISSION TCC SOLENOID C SHORTED TO GROUND";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xEC;
            row["description"] = "AW4 TRANSMISSION TCC SOLENOID C SHORTED TO VOLTAGE (12V)";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xED;
            row["description"] = "AW4 TRANSMISSION BATTERY VOLTS SENSE TOO LOW";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xEE;
            row["description"] = "AW4 TRANSMISSION BATTERY VOLTS SENSE TOO HIGH";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0xEF;
            row["description"] = "AISIN AW4 TRANSMISSION DTC PRESENT";
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

            if ((this.state == "enabled")&& (this.speed != null) && (this.logic != null) && (this.configuration != null))
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
            byte[] engineFaultCodePayload = new byte[] { }; // stored fault codes
            byte[] engineFaultCode1TPayload = new byte[] { }; // one-trip fault codes

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
                if (message[0] == 0x10)
                {
                    engineFaultCodePayload = new byte[data.Length - 6];
                    Array.Copy(data, 5, engineFaultCodePayload, 0, engineFaultCodePayload.Length); // copy payload from the input byte array (without ID and checksum byte)
                }
                else if ((message[0] == 0x2E) || (message[0] == 0x32) || (message[0] == 0x33))
                {
                    engineFaultCode1TPayload = new byte[data.Length - 6];
                    Array.Copy(data, 5, engineFaultCode1TPayload, 0, engineFaultCode1TPayload.Length); // copy payload from the input byte array (without ID and checksum byte)
                }
            }

            byte ID = message[0];
            ushort modifiedID;

            if ((ID == 0x14) || (ID == 0x22) || (ID == 0x2A) || ((ID >= 0xF0) && (ID < 0xFE)))
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
                    case 0x00:
                        descriptionToInsert = "PCM WAKE UP";
                        break;
                    case 0x10:
                        descriptionToInsert = "ENGINE FAULT CODE LIST";

                        if (message.Length >= 3)
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
                        break;
                    case 0x11:
                        descriptionToInsert = "FAULT BIT LIST";

                        if (message.Length >= 2)
                        {
                            valueToInsert = Util.ByteToHexStringSimple(payload);
                        }
                        break;
                    case 0x12:
                        descriptionToInsert = "SELECT HIGH-SPEED MODE";
                        break;
                    case 0x13:
                        descriptionToInsert = "ACTUATOR TEST";

                        if (message.Length >= 3)
                        {
                            switch (payload[0])
                            {
                                case 0x00:
                                    valueToInsert = "STOPPED";
                                    break;
                                case 0x01:
                                    descriptionToInsert = "ACTUATOR TEST | IGNITION COIL BANK #1";
                                    break;
                                case 0x02:
                                    descriptionToInsert = "ACTUATOR TEST | IGNITION COIL BANK #2";
                                    break;
                                case 0x03:
                                    descriptionToInsert = "ACTUATOR TEST | IGNITION COIL BANK #3";
                                    break;
                                case 0x04:
                                    descriptionToInsert = "ACTUATOR TEST | FUEL INJECTOR BANK #1";
                                    break;
                                case 0x05:
                                    descriptionToInsert = "ACTUATOR TEST | FUEL INJECTOR BANK #2";
                                    break;
                                case 0x06:
                                    descriptionToInsert = "ACTUATOR TEST | FUEL INJECTOR BANK #3";
                                    break;
                                case 0x07:
                                    descriptionToInsert = "ACTUATOR TEST | IDLE AIR CONTROL STEPPER MOTOR";
                                    break;
                                case 0x08:
                                    descriptionToInsert = "ACTUATOR TEST | RADIATOR FAN RELAY";
                                    break;
                                case 0x09:
                                    descriptionToInsert = "ACTUATOR TEST | A/C CLUTCH RELAY";
                                    break;
                                case 0x0A:
                                    descriptionToInsert = "ACTUATOR TEST | AUTOMATIC SHUTDOWN (ASD) RELAY";
                                    break;
                                case 0x0B:
                                    descriptionToInsert = "ACTUATOR TEST | EVAP PURGE SOLENOID";
                                    break;
                                case 0x0C:
                                    descriptionToInsert = "ACTUATOR TEST | SPEED CONTROL SOLENOID";
                                    break;
                                case 0x0D:
                                    descriptionToInsert = "ACTUATOR TEST | ALTERNATOR FIELD";
                                    break;
                                case 0x0E:
                                    descriptionToInsert = "ACTUATOR TEST | TACHOMETER OUTPUT";
                                    break;
                                case 0x0F:
                                    descriptionToInsert = "ACTUATOR TEST | TORQUE CONVERTER CLUTCH";
                                    break;
                                case 0x10:
                                    descriptionToInsert = "ACTUATOR TEST | EGR SOLENOID";
                                    break;
                                case 0x11:
                                    descriptionToInsert = "ACTUATOR TEST | WASTEGATE SOLENOID";
                                    break;
                                case 0x12:
                                    descriptionToInsert = "ACTUATOR TEST | BAROMETER SOLENOID";
                                    break;
                                case 0x14:
                                    descriptionToInsert = "ACTUATOR TEST | ALL SOLENOIDS / RELAYS";
                                    break;
                                case 0x16:
                                    descriptionToInsert = "ACTUATOR TEST | TRANSMISSION O/D SOLENOID";
                                    break;
                                case 0x17:
                                    descriptionToInsert = "ACTUATOR TEST | SHIFT INDICATOR LAMP";
                                    break;
                                case 0x19:
                                    descriptionToInsert = "ACTUATOR TEST | SURGE VALVE SOLENOID";
                                    break;
                                case 0x1A:
                                    descriptionToInsert = "ACTUATOR TEST | SPEED CONTROL VENT SOLENOID";
                                    break;
                                case 0x1B:
                                    descriptionToInsert = "ACTUATOR TEST | SPEED CONTROL VACUUM SOLENOID";
                                    break;
                                case 0x1C:
                                    descriptionToInsert = "ACTUATOR TEST | ASD FUEL SYSTEM";
                                    break;
                                case 0x1D:
                                    descriptionToInsert = "ACTUATOR TEST | FUEL INJECTOR BANK #4";
                                    break;
                                case 0x1E:
                                    descriptionToInsert = "ACTUATOR TEST | FUEL INJECTOR BANK #5";
                                    break;
                                case 0x1F:
                                    descriptionToInsert = "ACTUATOR TEST | FUEL INJECTOR BANK #6";
                                    break;
                                case 0x23:
                                    descriptionToInsert = "ACTUATOR TEST | IGNITION COIL BANK #4";
                                    break;
                                case 0x24:
                                    descriptionToInsert = "ACTUATOR TEST | IGNITION COIL BANK #5";
                                    break;
                                case 0x25:
                                    descriptionToInsert = "ACTUATOR TEST | FUEL INJECTOR BANK #7";
                                    break;
                                case 0x26:
                                    descriptionToInsert = "ACTUATOR TEST | FUEL INJECTOR BANK #8";
                                    break;
                                case 0x28:
                                    descriptionToInsert = "ACTUATOR TEST | INTAKE HEATER BANK #1";
                                    break;
                                case 0x29:
                                    descriptionToInsert = "ACTUATOR TEST | INTAKE HEATER BANK #2";
                                    break;
                                case 0x2C:
                                    descriptionToInsert = "ACTUATOR TEST | SPEED CONTROL 12 VOLT FEED";
                                    break;
                                case 0x2D:
                                    descriptionToInsert = "ACTUATOR TEST | INTAKE MANIFOLD TUNE VALVE";
                                    break;
                                case 0x2E:
                                    descriptionToInsert = "ACTUATOR TEST | LOW SPEED RADIATOR FAN RELAY";
                                    break;
                                case 0x2F:
                                    descriptionToInsert = "ACTUATOR TEST | HIGH SPEED RADIATOR FAN RELAY";
                                    break;
                                case 0x30:
                                    descriptionToInsert = "ACTUATOR TEST | FUEL INJECTOR BANK #9";
                                    break;
                                case 0x31:
                                    descriptionToInsert = "ACTUATOR TEST | FUEL INJECTOR BANK #10";
                                    break;
                                case 0x32:
                                    descriptionToInsert = "ACTUATOR TEST | 2-3 LOCKOUT SOLENOID";
                                    break;
                                case 0x33:
                                    descriptionToInsert = "ACTUATOR TEST | FUEL PUMP RELAY";
                                    break;
                                case 0x3B:
                                    descriptionToInsert = "ACTUATOR TEST | IAC STEPPER MOTOR STEP UP";
                                    break;
                                case 0x3C:
                                    descriptionToInsert = "ACTUATOR TEST | IAC STEPPER MOTOR STEP DOWN";
                                    break;
                                case 0x3D:
                                    descriptionToInsert = "ACTUATOR TEST | LEAK DETECTION PUMP SOLENOID";
                                    break;
                                case 0x40:
                                    descriptionToInsert = "ACTUATOR TEST | O2 SENSOR HEATER RELAY";
                                    break;
                                case 0x41:
                                    descriptionToInsert = "ACTUATOR TEST | OVERDRIVE LAMP";
                                    break;
                                case 0x43:
                                    descriptionToInsert = "ACTUATOR TEST | TRANSMISSION 12 VOLT RELAY";
                                    break;
                                case 0x44:
                                    descriptionToInsert = "ACTUATOR TEST | REVERSE LOCKOUT SOLENOID";
                                    break;
                                case 0x46:
                                    descriptionToInsert = "ACTUATOR TEST | SHORT RUNNER VALVE";
                                    break;
                                case 0x49:
                                    descriptionToInsert = "ACTUATOR TEST | WAIT TO START LAMP";
                                    break;
                                case 0x52:
                                    descriptionToInsert = "ACTUATOR TEST | 1/1 2/1 O2 SENSOR HEATER RELAY";
                                    break;
                                case 0x53:
                                    descriptionToInsert = "ACTUATOR TEST | 1/2 2/2 O2 SENSOR HEATER RELAY";
                                    break;
                                case 0x56:
                                    descriptionToInsert = "ACTUATOR TEST | 1/1 O2 SENSOR HEATER RELAY";
                                    break;
                                case 0x57:
                                    descriptionToInsert = "ACTUATOR TEST | O2 SENSOR HEATER RELAY";
                                    break;
                                case 0x5A:
                                    descriptionToInsert = "ACTUATOR TEST | RADIATOR FAN SOLENOID";
                                    break;
                                case 0x5B:
                                    descriptionToInsert = "ACTUATOR TEST | 1/2 O2 SENSOR HEATER RELAY";
                                    break;
                                case 0x5D:
                                    descriptionToInsert = "ACTUATOR TEST | EXHAUST BRAKE";
                                    break;
                                case 0x5E:
                                    descriptionToInsert = "ACTUATOR TEST | FUEL CONTROL";
                                    break;
                                case 0x5F:
                                    descriptionToInsert = "ACTUATOR TEST | PWM RADIATOR FAN";
                                    break;
                                default:
                                    descriptionToInsert = "ACTUATOR TEST | MODE: " + Util.ByteToHexString(payload, 0, 1);
                                    break;
                            }

                            if (payload[0] != 0)
                            {
                                if (payload[0] == payload[1])
                                {
                                    valueToInsert = "RUNNING";
                                }
                                else
                                {
                                    valueToInsert = "MODE NOT AVAILABLE";
                                }
                            }
                        }
                        break;
                    case 0x14:
                        descriptionToInsert = "REQUEST DIAGNOSTIC DATA";

                        if (message.Length >= 3)
                        {
                            switch (payload[0])
                            {
                                case 0x01:
                                    descriptionToInsert = "BATTERY TEMPERATURE SENSOR VOLTAGE";
                                    valueToInsert = Math.Round(payload[1] * 0.0196, 3).ToString("0.000").Replace(",", ".");
                                    unitToInsert = "V";
                                    break;
                                case 0x02:
                                    descriptionToInsert = "UPSTREAM O2 1/1 SENSOR VOLTAGE";
                                    valueToInsert = Math.Round(payload[1] * 0.0196, 3).ToString("0.000").Replace(",", ".");
                                    unitToInsert = "V";
                                    break;
                                case 0x05:
                                    descriptionToInsert = "ENGINE COOLANT TEMPERATURE";

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        valueToInsert = Math.Round((payload[1] * 1.8) - 198.4).ToString("0");
                                        unitToInsert = "°F";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        valueToInsert = (payload[1] - 128).ToString("0");
                                        unitToInsert = "°C";
                                    }
                                    break;
                                case 0x06:
                                    descriptionToInsert = "ENGINE COOLANT TEMPERATURE SENSOR VOLTAGE";
                                    valueToInsert = Math.Round(payload[1] * 0.0196, 3).ToString("0.000").Replace(",", ".");
                                    unitToInsert = "V";
                                    break;
                                case 0x07:
                                    descriptionToInsert = "THROTTLE POSITION SENSOR VOLTAGE";
                                    valueToInsert = Math.Round(payload[1] * 0.0196, 3).ToString("0.000").Replace(",", ".");
                                    unitToInsert = "V";
                                    break;
                                case 0x08:
                                    descriptionToInsert = "MINIMUM TPS VOLTAGE";
                                    valueToInsert = Math.Round(payload[1] * 0.0196, 3).ToString("0.000").Replace(",", ".");
                                    unitToInsert = "V";
                                    break;
                                case 0x09:
                                    descriptionToInsert = "KNOCK SENSOR VOLTAGE 1";
                                    valueToInsert = Math.Round(payload[1] * 0.0196, 3).ToString("0.000").Replace(",", ".");
                                    unitToInsert = "V";
                                    break;
                                case 0x0A:
                                    descriptionToInsert = "BATTERY VOLTAGE";
                                    valueToInsert = Math.Round(payload[1] * 0.0625, 3).ToString("0.000").Replace(",", ".");
                                    unitToInsert = "V";
                                    break;
                                case 0x0B:
                                    descriptionToInsert = "INTAKE MANIFOLD ABSOLUTE PRESSURE (MAP)";

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        valueToInsert = Math.Round(payload[1] * 0.059756, 1).ToString("0.0").Replace(",", ".");
                                        unitToInsert = "PSI";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        valueToInsert = Math.Round(payload[1] * 0.059756 * 6.894757, 1).ToString("0.0").Replace(",", ".");
                                        unitToInsert = "KPA";
                                    }
                                    break;
                                case 0x0C:
                                    descriptionToInsert = "TARGET IAC STEPPER MOTOR POSITION";
                                    valueToInsert = payload[1].ToString("0");
                                    break;
                                case 0x0E:
                                    descriptionToInsert = "LONG TERM FUEL TRIM 1";
                                    valueToInsert = payload[1].ToString("0");
                                    break;
                                case 0x0F:
                                    descriptionToInsert = "BAROMETRIC PRESSURE";

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        valueToInsert = Math.Round(payload[1] * 0.059756, 1).ToString("0.0").Replace(",", ".");
                                        unitToInsert = "PSI";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        valueToInsert = Math.Round(payload[1] * 0.059756 * 6.894757, 1).ToString("0.0").Replace(",", ".");
                                        unitToInsert = "KPA";
                                    }
                                    break;
                                case 0x10:
                                    descriptionToInsert = "MINIMUM AIR FLOW TEST";
                                    if (payload[1] == 0) valueToInsert = "STOPPED";
                                    else valueToInsert = "RUNNING";
                                    break;
                                case 0x11:
                                    descriptionToInsert = "ENGINE SPEED";
                                    valueToInsert = (payload[1] * 32).ToString("0");
                                    unitToInsert = "RPM";
                                    break;
                                case 0x12:
                                    descriptionToInsert = "CAM/CRANK SYNC SENSE";

                                    if (Util.IsBitSet(payload[1], 4)) valueToInsert = "IN-SYNC";
                                    else valueToInsert = "ENGINE STOPPED";
                                    break;
                                case 0x13:
                                    descriptionToInsert = "KEY-ON CYCLES ERROR 1";
                                    valueToInsert = payload[1].ToString("0");
                                    break;
                                case 0x15:
                                    descriptionToInsert = "SPARK ADVANCE";
                                    valueToInsert = Math.Round((payload[1] * 0.5), 1).ToString("0.0").Replace(",", ".");
                                    unitToInsert = "DEG";
                                    break;
                                case 0x16:
                                case 0x21:
                                    descriptionToInsert = "CYLINDER 1 RETARD";
                                    valueToInsert = Math.Round((payload[1] * 0.5), 1).ToString("0.0").Replace(",", ".");
                                    unitToInsert = "DEG";
                                    break;
                                case 0x17:
                                    descriptionToInsert = "CYLINDER 2 RETARD";
                                    valueToInsert = Math.Round((payload[1] * 0.5), 1).ToString("0.0").Replace(",", ".");
                                    unitToInsert = "DEG";
                                    break;
                                case 0x18:
                                    descriptionToInsert = "CYLINDER 3 RETARD";
                                    valueToInsert = Math.Round((payload[1] * 0.5), 1).ToString("0.0").Replace(",", ".");
                                    unitToInsert = "DEG";
                                    break;
                                case 0x19:
                                    descriptionToInsert = "CYLINDER 4 RETARD";
                                    valueToInsert = Math.Round((payload[1] * 0.5), 1).ToString("0.0").Replace(",", ".");
                                    unitToInsert = "DEG";
                                    break;
                                case 0x1A:
                                    descriptionToInsert = "TARGET BOOST";

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        valueToInsert = Math.Round(payload[1] * 0.115294117, 1).ToString("0.0").Replace(",", ".");
                                        unitToInsert = "PSI";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        valueToInsert = Math.Round(payload[1] * 0.115294117 * 6.89475729, 1).ToString("0.0").Replace(",", ".");
                                        unitToInsert = "KPA";
                                    }

                                    break;
                                case 0x1B:
                                    descriptionToInsert = "INTAKE AIR TEMPERATURE";

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        valueToInsert = Math.Round((payload[1] * 1.8) - 198.4).ToString("0");
                                        unitToInsert = "°F";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        valueToInsert = (payload[1] - 128).ToString("0");
                                        unitToInsert = "°C";
                                    }
                                    break;
                                case 0x1C:
                                    descriptionToInsert = "INTAKE AIR TEMPERATURE SENSOR VOLTAGE";
                                    valueToInsert = Math.Round(payload[1] * 0.0196, 3).ToString("0.000").Replace(",", ".");
                                    unitToInsert = "V";
                                    break;
                                case 0x1D:
                                    descriptionToInsert = "CRUISE SET SPEED";

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        valueToInsert = payload[1].ToString("0");
                                        unitToInsert = "MPH";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        valueToInsert = Math.Round(payload[1] * 1.609344).ToString("0");
                                        unitToInsert = "KM/H";
                                    }
                                    break;
                                case 0x1E:
                                    descriptionToInsert = "KEY-ON CYCLES ERROR 2";
                                    valueToInsert = payload[1].ToString("0");
                                    break;
                                case 0x1F:
                                    descriptionToInsert = "KEY-ON CYCLES ERROR 3";
                                    valueToInsert = payload[1].ToString("0");
                                    break;
                                case 0x20:
                                    string lastCruiseCutoutReason;
                                    string cruiseDeniedReason;

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
                                    break;
                                case 0x24:
                                    descriptionToInsert = "BATTERY CHARGING VOLTAGE";
                                    valueToInsert = Math.Round(payload[1] * 0.0625, 3).ToString("0.000").Replace(",", ".");
                                    unitToInsert = "V";
                                    break;
                                case 0x25:
                                    descriptionToInsert = "OVER 5 PSI BOOST TIMER";
                                    valueToInsert = payload[1].ToString("0");
                                    break;
                                case 0x28:
                                    descriptionToInsert = "WASTEGATE DUTY CYCLE";
                                    valueToInsert = Math.Round(payload[1] * 0.5, 1).ToString("0.0").Replace(",", ".");
                                    unitToInsert = "PERCENT";
                                    break;
                                case 0x27:
                                    descriptionToInsert = "THEFT ALARM STATUS";
                                    valueToInsert = Convert.ToString(payload[1], 2).PadLeft(8, '0');
                                    break;
                                case 0x29:
                                    descriptionToInsert = "READ FUEL SETTING";
                                    valueToInsert = Util.ByteToHexString(payload, 1, 1);
                                    break;
                                case 0x2A:
                                    descriptionToInsert = "READ SET SYNC";
                                    valueToInsert = Util.ByteToHexString(payload, 1, 1);
                                    break;
                                case 0x2C:
                                    descriptionToInsert = "CRUISE SWITCH VOLTAGE SENSE";
                                    valueToInsert = Math.Round(payload[1] * 0.0196, 3).ToString("0.000").Replace(",", ".");
                                    unitToInsert = "V";
                                    break;
                                case 0x2D:
                                    descriptionToInsert = "AMBIENT/BATTERY TEMPERATURE";

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        valueToInsert = Math.Round((payload[1] * 1.8) - 198.4).ToString("0");
                                        unitToInsert = "°F";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        valueToInsert = (payload[1] - 128).ToString("0");
                                        unitToInsert = "°C";
                                    }
                                    break;
                                case 0x2F:
                                    descriptionToInsert = "UPSTREAM O2 2/1 SENSOR VOLTAGE";
                                    valueToInsert = Math.Round(payload[1] * 0.0196, 3).ToString("0.000").Replace(",", ".");
                                    unitToInsert = "V";
                                    break;
                                case 0x30:
                                    descriptionToInsert = "KNOCK SENSOR VOLTAGE 2";
                                    valueToInsert = Math.Round(payload[1] * 0.0196, 3).ToString("0.000").Replace(",", ".");
                                    unitToInsert = "V";
                                    break;
                                case 0x31:
                                    descriptionToInsert = "LONG TERM FUEL TRIM 2";
                                    valueToInsert = payload[1].ToString("0");
                                    break;
                                case 0x32:
                                    descriptionToInsert = "A/C HIGH SIDE PRESSURE SENSOR VOLTAGE";
                                    valueToInsert = Math.Round(payload[1] * 0.0196, 3).ToString("0.000").Replace(",", ".");
                                    unitToInsert = "V";
                                    break;
                                case 0x33:
                                    descriptionToInsert = "A/C HIGH SIDE PRESSURE SENSOR";

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        valueToInsert = Math.Round(payload[1] * 1.961, 1).ToString("0.0").Replace(",", ".");
                                        unitToInsert = "PSI";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        valueToInsert = Math.Round(payload[1] * 1.961 * 6.894757, 1).ToString("0.0").Replace(",", ".");
                                        unitToInsert = "KPA";
                                    }
                                    break;
                                case 0x34:
                                    descriptionToInsert = "FLEX FUEL SENSOR VOLTAGE";
                                    valueToInsert = Math.Round(payload[1] * 0.0196, 3).ToString("0.000").Replace(",", ".");
                                    unitToInsert = "V";
                                    break;
                                case 0x35:
                                    descriptionToInsert = "FLEX FUEL INFO 1";
                                    valueToInsert = payload[1].ToString("0");
                                    break;
                                case 0x3B:
                                    descriptionToInsert = "FUEL SYSTEM STATUS 1";

                                    List<string> FSS1 = new List<string>();
                                    FSS1.Clear();

                                    if (Util.IsBitSet(payload[1], 0)) FSS1.Add("OPEN LOOP");
                                    if (Util.IsBitSet(payload[1], 1)) FSS1.Add("CLOSED LOOP");
                                    if (Util.IsBitSet(payload[1], 2)) FSS1.Add("OPEN LOOP / DRIVE");
                                    if (Util.IsBitSet(payload[1], 3)) FSS1.Add("OPEN LOOP / DTC");
                                    if (Util.IsBitSet(payload[1], 4)) FSS1.Add("CLOSED LOOP / DTC");

                                    if (FSS1.Count > 0)
                                    {
                                        foreach (string s in FSS1)
                                        {
                                            valueToInsert += s + " | ";
                                        }

                                        if (valueToInsert.Length > 2) valueToInsert = valueToInsert.Remove(valueToInsert.Length - 3); // remove last "|" character
                                    }
                                    else
                                    {
                                        valueToInsert = "N/A";
                                    }
                                    break;
                                case 0x3E:
                                    descriptionToInsert = "CALPOT VOLTAGE";
                                    valueToInsert = Util.ByteToHexString(payload, 1, 1);
                                    break;
                                case 0x3F:
                                    descriptionToInsert = "DOWNSTREAM O2 1/2 SENSOR VOLTAGE";
                                    valueToInsert = Math.Round(payload[1] * 0.0196, 3).ToString("0.000").Replace(",", ".");
                                    unitToInsert = "V";
                                    break;
                                case 0x40:
                                    descriptionToInsert = "MAP SENSOR VOLTAGE";
                                    valueToInsert = Math.Round(payload[1] * 0.0196, 3).ToString("0.000").Replace(",", ".");
                                    unitToInsert = "V";
                                    break;
                                case 0x41:
                                    descriptionToInsert = "VEHICLE SPEED";

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        valueToInsert = payload[1].ToString("0");
                                        unitToInsert = "MPH";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        valueToInsert = Math.Round(payload[1] * 1.609344).ToString("0");
                                        unitToInsert = "KM/H";
                                    }
                                    break;
                                case 0x42:
                                    descriptionToInsert = "UPSTREAM O2 1/1 SENSOR LEVEL";

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
                                    break;
                                case 0x45:
                                    descriptionToInsert = "MAP VACUUM";

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        valueToInsert = Math.Round(payload[1] * 0.059756, 1).ToString("0.0").Replace(",", ".");
                                        unitToInsert = "PSI";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        valueToInsert = Math.Round(payload[1] * 0.059756 * 6.894757, 1).ToString("0.0").Replace(",", ".");
                                        unitToInsert = "KPA";
                                    }
                                    break;
                                case 0x46:
                                    descriptionToInsert = "RELATIVE THROTTLE POSITION";
                                    valueToInsert = Math.Round(payload[1] * 0.0196, 3).ToString("0.000").Replace(",", ".");
                                    unitToInsert = "V";
                                    break;
                                case 0x47:
                                    descriptionToInsert = "SPARK ADVANCE";
                                    valueToInsert = Math.Round((payload[1] * 0.5), 1).ToString("0.0").Replace(",", ".");
                                    unitToInsert = "DEG";
                                    break;
                                case 0x48:
                                    descriptionToInsert = "UPSTREAM O2 2/1 SENSOR LEVEL";

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
                                    break;
                                case 0x49:
                                    descriptionToInsert = "DOWNSTREAM O2 2/2 SENSOR VOLTAGE";
                                    valueToInsert = Math.Round(payload[1] * 0.0196, 3).ToString("0.000").Replace(",", ".");
                                    unitToInsert = "V";
                                    break;
                                case 0x4A:
                                    descriptionToInsert = "DOWNSTREAM O2 1/2 SENSOR LEVEL";

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
                                    break;
                                case 0x4B:
                                    descriptionToInsert = "DOWNSTREAM O2 2/2 SENSOR LEVEL";

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
                                    break;
                                case 0x4E:
                                    descriptionToInsert = "FUEL LEVEL SENSOR VOLTAGE";
                                    valueToInsert = Math.Round(payload[1] * 0.0196, 3).ToString("0.000").Replace(",", ".");
                                    unitToInsert = "V";
                                    break;
                                case 0x4F:
                                    descriptionToInsert = "FUEL LEVEL";

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        valueToInsert = Math.Round(payload[1] * 0.125, 1).ToString("0.0").Replace(",", ".");
                                        unitToInsert = "GALLON";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        valueToInsert = Math.Round(payload[1] * 0.125 * 3.785412, 1).ToString("0.0").Replace(",", ".");
                                        unitToInsert = "LITER";
                                    }
                                    break;
                                case 0x57:
                                    descriptionToInsert = "FUEL SYSTEM STATUS 2";

                                    List<string> FSS2 = new List<string>();
                                    FSS2.Clear();

                                    if (Util.IsBitSet(payload[1], 0)) FSS2.Add("OPEN LOOP");
                                    if (Util.IsBitSet(payload[1], 1)) FSS2.Add("CLOSED LOOP");
                                    if (Util.IsBitSet(payload[1], 2)) FSS2.Add("OPEN LOOP / DRIVE");
                                    if (Util.IsBitSet(payload[1], 3)) FSS2.Add("OPEN LOOP / DTC");
                                    if (Util.IsBitSet(payload[1], 4)) FSS2.Add("CLOSED LOOP / DTC");

                                    if (FSS2.Count > 0)
                                    {
                                        foreach (string s in FSS2)
                                        {
                                            valueToInsert += s + " | ";
                                        }

                                        if (valueToInsert.Length > 2) valueToInsert = valueToInsert.Remove(valueToInsert.Length - 3); // remove last "|" character
                                    }
                                    else
                                    {
                                        valueToInsert = "N/A";
                                    }
                                    break;
                                case 0x58:
                                    descriptionToInsert = "CRUISE CUTOUT STATE";

                                    string cutoutReason;

                                    switch (payload[1] & 0xF0) // upper 4 bits encode last cutout reason 
                                    {
                                        case 0x00:
                                            cutoutReason = "ON/OFF SW";
                                            break;
                                        case 0x10:
                                            cutoutReason = "SPEED SEN";
                                            break;
                                        case 0x20:
                                            cutoutReason = "RPM LIMIT";
                                            break;
                                        case 0x30:
                                            cutoutReason = "BRAKE SW";
                                            break;
                                        case 0x40:
                                            cutoutReason = "P/N SW";
                                            break;
                                        case 0x50:
                                            cutoutReason = "RPM/SPEED";
                                            break;
                                        case 0x60:
                                            cutoutReason = "CLUTCH";
                                            break;
                                        case 0x70:
                                            cutoutReason = "S/C DTC";
                                            break;
                                        case 0x80:
                                            cutoutReason = "KEY OFF";
                                            break;
                                        case 0x90:
                                            cutoutReason = "ACTIVE";
                                            break;
                                        case 0xA0:
                                            cutoutReason = "CLUTCH UP";
                                            break;
                                        case 0xB0:
                                            cutoutReason = "N/A";
                                            break;
                                        case 0xC0:
                                            cutoutReason = "SW DTC";
                                            break;
                                        case 0xD0:
                                            cutoutReason = "CANCEL SW";
                                            break;
                                        case 0xE0:
                                            cutoutReason = "LIMP-IN";
                                            break;
                                        case 0xF0:
                                            cutoutReason = "12V DTC";
                                            break;
                                        default:
                                            cutoutReason = "N/A";
                                            break;
                                    }

                                    valueToInsert = cutoutReason;
                                    break;
                                case 0x59:
                                    descriptionToInsert = "CRUISE DENIED STATE";

                                    string deniedReason;

                                    switch (payload[1] & 0x0F) // lower 4 bits encode denied reason 
                                    {
                                        case 0x00:
                                            deniedReason = "ON/OFF SW";
                                            break;
                                        case 0x01:
                                            deniedReason = "SPEED SEN";
                                            break;
                                        case 0x02:
                                            deniedReason = "RPM LIMIT";
                                            break;
                                        case 0x03:
                                            deniedReason = "BRAKE SW";
                                            break;
                                        case 0x04:
                                            deniedReason = "P/N SW";
                                            break;
                                        case 0x05:
                                            deniedReason = "RPM/SPEED";
                                            break;
                                        case 0x06:
                                            deniedReason = "CLUTCH";
                                            break;
                                        case 0x07:
                                            deniedReason = "S/C DTC";
                                            break;
                                        case 0x08:
                                            deniedReason = "ALLOWED";
                                            break;
                                        case 0x09:
                                            deniedReason = "ACTIVE";
                                            break;
                                        case 0x0A:
                                            deniedReason = "CLUTCH UP";
                                            break;
                                        case 0x0B:
                                            deniedReason = "N/A";
                                            break;
                                        case 0x0C:
                                            deniedReason = "SW DTC";
                                            break;
                                        case 0x0D:
                                            deniedReason = "CANCEL SW";
                                            break;
                                        case 0x0E:
                                            deniedReason = "LIMP-IN";
                                            break;
                                        case 0x0F:
                                            deniedReason = "12V DTC";
                                            break;
                                        default:
                                            deniedReason = "N/A";
                                            break;
                                    }

                                    valueToInsert = deniedReason;
                                    break;
                                case 0x5C:
                                    descriptionToInsert = "CALCULATED ENGINE LOAD";
                                    valueToInsert = Math.Round(payload[1] * 0.3922, 1).ToString("0.0").Replace(",", ".");
                                    unitToInsert = "PERCENT";
                                    break;
                                case 0x5A:
                                    descriptionToInsert = "OUTPUT SHAFT SPEED";
                                    valueToInsert = (payload[1] * 20).ToString("0");
                                    unitToInsert = "RPM";
                                    break;
                                case 0x5B:
                                    descriptionToInsert = "GOVERNOR PRESSURE DUTY CYCLE";
                                    valueToInsert = Math.Round(payload[1] * 0.3922, 1).ToString("0.0").Replace(",", ".");
                                    unitToInsert = "PERCENT";
                                    break;
                                case 0x5F:
                                    descriptionToInsert = "EGR POSITION SENSOR VOLTAGE";
                                    valueToInsert = Math.Round(payload[1] * 0.0196, 3).ToString("0.000").Replace(",", ".");
                                    unitToInsert = "V";
                                    break;
                                case 0x60:
                                    descriptionToInsert = "EGR ZREF UPDATE D.C.";
                                    valueToInsert = payload[1].ToString("0");
                                    break;
                                case 0x64:
                                    descriptionToInsert = "ACTUAL PURGE CURRENT";
                                    valueToInsert = Math.Round(payload[1] * 0.0196, 1).ToString("0.0").Replace(",", ".");
                                    unitToInsert = "A";
                                    break;
                                case 0x65:
                                    descriptionToInsert = "CATALYST TEMPERATURE SENSOR VOLTAGE";
                                    valueToInsert = Math.Round(payload[1] * 0.0196, 3).ToString("0.000").Replace(",", ".");
                                    unitToInsert = "V";
                                    break;
                                case 0x66:
                                    descriptionToInsert = "CATALYST TEMPERATURE";

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        valueToInsert = Math.Round((payload[1] * 1.8) - 198.4).ToString("0");
                                        unitToInsert = "°F";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        valueToInsert = (payload[1] - 128).ToString("0");
                                        unitToInsert = "°C";
                                    }
                                    break;
                                case 0x69:
                                    descriptionToInsert = "AMBIENT TEMPERATURE SENSOR VOLTAGE";
                                    valueToInsert = Math.Round(payload[1] * 0.0196, 3).ToString("0.000").Replace(",", ".");
                                    unitToInsert = "V";
                                    break;
                                case 0x6D:
                                    descriptionToInsert = "T-CASE SWITCH VOLTAGE";
                                    valueToInsert = Math.Round(payload[1] * 0.0196, 3).ToString("0.000").Replace(",", ".");
                                    unitToInsert = "V";
                                    break;
                                case 0x7A:
                                    descriptionToInsert = "FCA CURRENT";
                                    valueToInsert = Math.Round(payload[1] * 0.0196, 1).ToString("0.0").Replace(",", ".");
                                    unitToInsert = "A";
                                    break;
                                case 0x7C:
                                    descriptionToInsert = "OIL TEMPERATURE SENSOR VOLTAGE";
                                    valueToInsert = Math.Round(payload[1] * 0.0196, 3).ToString("0.000").Replace(",", ".");
                                    unitToInsert = "V";
                                    break;
                                case 0x7D:
                                    descriptionToInsert = "OIL TEMPERATURE SENSOR";

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        valueToInsert = Math.Round((payload[1] * 1.8) - 83.2).ToString("0");
                                        unitToInsert = "°F";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        valueToInsert = (payload[1] - 64).ToString("0");
                                        unitToInsert = "°C";
                                    }
                                    break;
                                default:
                                    descriptionToInsert = "REQUEST DIAGNOSTIC DATA | OFFSET: " + Util.ByteToHexString(payload, 0, 1);
                                    valueToInsert = Util.ByteToHexString(payload, 1, payload.Length - 1);
                                    break;
                            }
                        }
                        break;
                    case 0x15:
                        descriptionToInsert = "READ FLASH MEMORY";

                        if (message.Length >= 4)
                        {
                            descriptionToInsert = "READ FLASH MEMORY | OFFSET: " + Util.ByteToHexString(payload, 0, 2);
                            valueToInsert = Util.ByteToHexString(payload, 2, 1);
                        }
                        break;
                    case 0x16:
                        descriptionToInsert = "READ FLASH MEMORY CONSTANT";

                        if (message.Length >= 3)
                        {
                            ushort offset = (ushort)(payload[0] + 0x8000);
                            byte[] offsetArray = new byte[2];
                            offsetArray[0] = (byte)((offset >> 8) & 0xFF);
                            offsetArray[1] = (byte)(offset & 0xFF);

                            descriptionToInsert = "READ FLASH MEMORY CONSTANT | OFFSET: " + Util.ByteToHexStringSimple(offsetArray);
                            valueToInsert = Util.ByteToHexString(payload, 1, 1);
                        }
                        break;
                    case 0x17:
                        descriptionToInsert = "ERASE ENGINE FAULT CODES";

                        if (message.Length >= 2)
                        {
                            if (payload[0] == 0xE0) valueToInsert = "ERASED";
                            else valueToInsert = "FAILED";
                        }
                        break;
                    case 0x18:
                        descriptionToInsert = "CONTROL ASD RELAY";

                        if (message.Length >= 3)
                        {
                            valueToInsert = Util.ByteToHexString(payload, 1, 1);
                        }
                        break;
                    case 0x19:
                        descriptionToInsert = "SET ENGINE IDLE SPEED";

                        if (message.Length >= 2)
                        {
                            valueToInsert = Math.Round(payload[0] * 7.85).ToString("0");

                            if (payload[0] < 0x42)
                            {
                                valueToInsert += " - TOO LOW!";
                            }

                            unitToInsert = "RPM";
                        }
                        break;
                    case 0x1A:
                        descriptionToInsert = "SWITCH TEST";

                        if (message.Length >= 3)
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

                                        if (descriptionToInsert.Length > 2) descriptionToInsert = descriptionToInsert.Remove(descriptionToInsert.Length - 3); // remove last "|" character
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
                        }
                        break;
                    case 0x1B:
                        descriptionToInsert = "INIT BYTE MODE DOWNLOAD";
                        break;
                    case 0x1C:
                        descriptionToInsert = "WRITE MEMORY";

                        if (message.Length >= 4)
                        {
                            switch (payload[0])
                            {
                                case 0x10:
                                    descriptionToInsert = "WRITE MEMROY | RESET EMR 1";
                                    if ((payload[1] == 0x00) && (payload[2] == 0xFF)) valueToInsert = "OK";
                                    else valueToInsert = "FAILED";
                                    break;
                                case 0x11:
                                    descriptionToInsert = "WRITE MEMROY | RESET EMR 2";
                                    if ((payload[1] == 0x00) && (payload[2] == 0xFF)) valueToInsert = "OK";
                                    else valueToInsert = "FAILED";
                                    break;
                                case 0x1A:
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
                        break;
                    case 0x1F:
                        descriptionToInsert = "WRITE RAM WORKER";

                        if (message.Length >= 4)
                        {
                            descriptionToInsert = "WRITE RAM | OFFSET: 07 " + Util.ByteToHexString(payload, 0, 1);

                            switch (payload[2])
                            {
                                case 0xE5:
                                    valueToInsert = Util.ByteToHexString(payload, 1, 1);
                                    unitToInsert = "OK";
                                    break;
                                case 0x00:
                                    valueToInsert = "DENIED (INVALID OFFSET)";
                                    break;
                                case 0xF1:
                                    valueToInsert = "DENIED (SECURITY LEVEL)";
                                    break;
                                default:
                                    valueToInsert = Util.ByteToHexString(payload, 2, 1);
                                    break;
                            }
                        }
                        break;
                    case 0x20:
                        descriptionToInsert = "RUN RAM WORKER";

                        if (message.Length >= 4)
                        {
                            switch (payload[2])
                            {
                                case 0xE5:
                                    valueToInsert = Util.ByteToHexString(payload, 1, 1);
                                    unitToInsert = "OK";
                                    break;
                                case 0x00:
                                    valueToInsert = "DENIED (NO RTS FOUND)";
                                    break;
                                case 0x01:
                                    valueToInsert = "DENIED (RTS OFFSET)";
                                    break;
                                case 0x02:
                                    valueToInsert = "DENIED (INVALID OFFSET)";
                                    break;
                                case 0xF1:
                                    valueToInsert = "DENIED (SECURITY LEVEL)";
                                    break;
                                default:
                                    valueToInsert = Util.ByteToHexString(payload, 2, 1);
                                    break;
                            }
                        }
                        break;
                    case 0x21:
                        descriptionToInsert = "SET ENGINE PARAMETER";

                        if (message.Length >= 3)
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
                        break;
                    case 0x22:
                        descriptionToInsert = "SEND ENGINE PARAMETERS";

                        if (message.Length >= 4)
                        {
                            switch (payload[0])
                            {
                                case 0x01:
                                    descriptionToInsert = "ENGINE SPEED";
                                    valueToInsert = (((payload[1] << 8) + payload[2]) * 0.125).ToString("0.000").Replace(",", ".");
                                    unitToInsert = "RPM";
                                    break;
                                case 0x02:
                                    descriptionToInsert = "INJECTOR PULSEWIDTH";
                                    valueToInsert = ((payload[1] << 8) + payload[2]).ToString("0");
                                    unitToInsert = "USEC"; // microseconds
                                    break;
                                case 0x03:
                                    descriptionToInsert = "TARGET IDLE SPEED";
                                    valueToInsert = (((payload[1] << 8) + payload[2]) * 0.125).ToString("0.000").Replace(",", ".");
                                    unitToInsert = "RPM";
                                    break;
                                default:
                                    descriptionToInsert = "SEND ENGINE PARAMETER | OFFSET: " + Util.ByteToHexString(payload, 0, 1);
                                    valueToInsert = Util.ByteToHexString(payload, 1, payload.Length - 1);
                                    break;
                            }
                        }
                        break;
                    case 0x23:
                        descriptionToInsert = "RESET MEMORY";

                        if (message.Length >= 3)
                        {
                            switch (payload[0])
                            {
                                case 0x01:
                                    descriptionToInsert = "ERASE ENGINE FAULT CODES";
                                    break;
                                case 0x02:
                                    descriptionToInsert = "RESET ADAPTIVE FUEL FACTOR (LTFT)";
                                    break;
                                case 0x03:
                                    descriptionToInsert = "RESET IAC COUNTER";
                                    break;
                                case 0x04:
                                    descriptionToInsert = "RESET MINIMUM TPS VOLTS";
                                    break;
                                case 0x05:
                                    descriptionToInsert = "RESET FLEX FUEL PERCENT";
                                    break;
                                case 0x06:
                                    descriptionToInsert = "RESET CAM/CRANK SYNC";
                                    break;
                                case 0x07:
                                    descriptionToInsert = "RESET FUEL SHUTOFF";
                                    break;
                                case 0x08:
                                    descriptionToInsert = "RESET RUNTIME AT STALL";
                                    break;
                                case 0x09:
                                    descriptionToInsert = "DOOR LOCK ENABLE";
                                    break;
                                case 0x0A:
                                    descriptionToInsert = "DOOR LOCK DISABLE";
                                    break;
                                case 0x0B:
                                    descriptionToInsert = "RESET CAM/CRANK TIMING REFERENCE";
                                    break;
                                case 0x0C:
                                    descriptionToInsert = "A/C FAULT ENABLE";
                                    break;
                                case 0x0D:
                                    descriptionToInsert = "A/C FAULT DISABLE";
                                    break;
                                case 0x0E:
                                    descriptionToInsert = "S/C FAULT ENABLE";
                                    break;
                                case 0x0F:
                                    descriptionToInsert = "S/C FAULT DISABLE";
                                    break;
                                case 0x10:
                                    descriptionToInsert = "PS FAULT ENABLE";
                                    break;
                                case 0x11:
                                    descriptionToInsert = "PS FAULT DISABLE";
                                    break;
                                case 0x12:
                                    descriptionToInsert = "RESET EEPROM / ADAPTIVE NUMERATOR";
                                    break;
                                case 0x13:
                                    descriptionToInsert = "RESET SKIM F4";
                                    break;
                                case 0x14:
                                    descriptionToInsert = "RESET DUTY CYCLE MONITOR";
                                    break;
                                case 0x15:
                                    descriptionToInsert = "RESET TRIP/IDLE/CRUISE/INJ/O/D OFF/WATER IN FUEL";
                                    break;
                                case 0x20:
                                    descriptionToInsert = "RESET TPS ADAPTATION FOR ETC";
                                    break;
                                case 0x21:
                                    descriptionToInsert = "RESET MIN PEDAL VALUE";
                                    break;
                                case 0x22:
                                    descriptionToInsert = "RESET LEARNED KNOCK CORRECTION";
                                    break;
                                case 0x23:
                                    descriptionToInsert = "RESET LEARNED MISFIRE CORRECTION";
                                    break;
                                case 0x24:
                                    descriptionToInsert = "RESET IDLE ADAPTATION";
                                    break;
                                default:
                                    descriptionToInsert = "RESET MEMORY | OFFSET: " + Util.ByteToHexString(payload, 0, 1);
                                    break;
                            }

                            switch (payload[1])
                            {
                                case 0x00:
                                    valueToInsert = "STOP ENGINE";
                                    break;
                                case 0x01:
                                    valueToInsert = "MODE NOT SUPPORTED";
                                    break;
                                case 0x02:
                                    valueToInsert = "DENIED (MODULE BUSY)";
                                    break;
                                case 0x03:
                                    valueToInsert = "DENIED (SECURITY LEVEL)";
                                    break;
                                case 0xF0:
                                    valueToInsert = "OK";
                                    break;
                                default:
                                    valueToInsert = Util.ByteToHexString(payload, 1, 1);
                                    break;
                            }
                        }
                        break;
                    case 0x25:
                        descriptionToInsert = "WRITE ROM SETTING";

                        if (message.Length >= 4)
                        {
                            switch (payload[0])
                            {
                                case 0x01:
                                    descriptionToInsert = "WRITE ROM SETTING | CALPOT LEAD";
                                    break;
                                case 0x02:
                                    descriptionToInsert = "WRITE ROM SETTING | CALPOT MEX";
                                    break;
                                case 0x04:
                                    descriptionToInsert = "WRITE ROM SETTING | EGR SYSTEM";
                                    break;
                                case 0x05:
                                    descriptionToInsert = "WRITE ROM SETTING | FUEL INJECTOR #1";
                                    break;
                                case 0x0F:
                                    descriptionToInsert = "WRITE ROM SETTING | MINIMUM AIR FLOW";
                                    break;
                                case 0x10:
                                    descriptionToInsert = "WRITE ROM SETTING | CALPOT LHBL";
                                    break;
                                case 0x1C:
                                    descriptionToInsert = "WRITE ROM SETTING | MISFIRE MONITOR";
                                    break;
                                case 0x21:
                                    descriptionToInsert = "WRITE ROM SETTING | LINEAR IAC MOTOR";
                                    break;
                                case 0x25:
                                    descriptionToInsert = "WRITE ROM SETTING | CYLINDER PERFORMANCE TEST";
                                    break;
                                case 0x26:
                                    descriptionToInsert = "WRITE ROM SETTING | HIGH-PRESSURE SAFETY VALVE TEST";
                                    break;
                                default:
                                    descriptionToInsert = "WRITE ROM SETTING | OFFSET: " + Util.ByteToHexString(payload, 0, 1);
                                    break;
                            }

                            switch (payload[1])
                            {
                                case 0x00:
                                    valueToInsert = "RESET";
                                    break;
                                case 0x01:
                                    valueToInsert = "ENABLED";
                                    break;
                                case 0x02:
                                    valueToInsert = "DISABLED";
                                    break;
                                default:
                                    valueToInsert = "UNKNOWN";
                                    break;
                            }

                            switch (payload[2])
                            {
                                case 0x01:
                                    unitToInsert = "OK";
                                    break;
                                default:
                                    unitToInsert = "DENIED (" + Util.ByteToHexString(payload, 2, 1) + ")";
                                    break;
                            }
                        }
                        break;
                    case 0x26:
                        descriptionToInsert = "READ FLASH MEMORY";

                        if (message.Length >= 5)
                        {
                            descriptionToInsert = "READ FLASH MEMORY | OFFSET: " + Util.ByteToHexString(payload, 0, 3);
                            valueToInsert = Util.ByteToHexString(payload, 3, 1);
                        }
                        break;
                    case 0x27:
                        descriptionToInsert = "WRITE EEPROM";

                        if (message.Length >= 5)
                        {
                            descriptionToInsert = "WRITE EEPROM | OFFSET: " + Util.ByteToHexString(payload, 0, 2);

                            switch (payload[3])
                            {
                                case 0xE2:
                                    valueToInsert = Util.ByteToHexString(payload, 2, 1);
                                    unitToInsert = "OK";
                                    break;
                                case 0xE4:
                                    valueToInsert = "UNKNOWN RESULT";
                                    break;
                                case 0xE5:
                                    valueToInsert = "UNKNOWN RESULT";
                                    break;
                                case 0xF0:
                                    valueToInsert = "DENIED (INVALID OFFSET)";
                                    break;
                                case 0xF1:
                                    valueToInsert = "DENIED (SECURITY LEVEL)";
                                    break;
                                default:
                                    valueToInsert = Util.ByteToHexString(payload, 3, 1);
                                    break;
                            }
                        }
                        break;
                    case 0x28:
                        descriptionToInsert = "READ EEPROM";

                        if (message.Length >= 4)
                        {
                            descriptionToInsert = "READ EEPROM | OFFSET: " + Util.ByteToHexString(payload, 0, 2);
                            valueToInsert = Util.ByteToHexString(payload, 2, 1);
                        }
                        break;
                    case 0x29:
                        descriptionToInsert = "WRITE RAM";

                        if (message.Length >= 5)
                        {
                            descriptionToInsert = "WRITE RAM | OFFSET: " + Util.ByteToHexString(payload, 0, 2);

                            switch (payload[3])
                            {
                                case 0xE5:
                                    valueToInsert = Util.ByteToHexString(payload, 2, 1);
                                    unitToInsert = "OK";
                                    break;
                                case 0xF0:
                                    valueToInsert = "DENIED (INVALID OFFSET)";
                                    break;
                                case 0xF1:
                                    valueToInsert = "DENIED (SECURITY LEVEL)";
                                    break;
                                default:
                                    valueToInsert = Util.ByteToHexString(payload, 3, 1);
                                    break;
                            }
                        }
                        break;
                    case 0x2A:
                        descriptionToInsert = "CONFIGURATION";

                        if (message.Length >= 3)
                        {
                            switch (payload[0])
                            {
                                case 0x01:
                                    descriptionToInsert = "CONFIGURATION | PART NUMBER 1-2";
                                    valueToInsert = Util.ByteToHexString(payload, 1, 1);
                                    break;
                                case 0x02:
                                    descriptionToInsert = "CONFIGURATION | PART NUMBER 3-4";
                                    valueToInsert = Util.ByteToHexString(payload, 1, 1);
                                    break;
                                case 0x03:
                                    descriptionToInsert = "CONFIGURATION | PART NUMBER 5-6";
                                    valueToInsert = Util.ByteToHexString(payload, 1, 1);
                                    break;
                                case 0x04:
                                    descriptionToInsert = "CONFIGURATION | PART NUMBER 7-8";
                                    valueToInsert = Util.ByteToHexString(payload, 1, 1);
                                    break;
                                case 0x06:
                                    descriptionToInsert = "CONFIGURATION | EMISSION STANDARD";

                                    switch (payload[1])
                                    {
                                        case 1:
                                            valueToInsert = "OBD-II BY CARB";
                                            break;
                                        case 2:
                                            valueToInsert = "OBD BY EPA";
                                            break;
                                        case 3:
                                            valueToInsert = "OBD AND OBD-II";
                                            break;
                                        case 4:
                                            valueToInsert = "OBD-I";
                                            break;
                                        case 5:
                                            valueToInsert = "NOT OBD COMPLIANT";
                                            break;
                                        case 6:
                                            valueToInsert = "EOBD (EUROPE)";
                                            break;
                                        case 7:
                                            valueToInsert = "EOBD AND OBD-II";
                                            break;
                                        case 8:
                                            valueToInsert = "EOBD AND OBD";
                                            break;
                                        case 9:
                                            valueToInsert = "EOBD OBD OBD-II";
                                            break;
                                        case 10:
                                            valueToInsert = "JOBD (JAPAN)";
                                            break;
                                        case 11:
                                            valueToInsert = "JOBD AND OBD-II";
                                            break;
                                        case 12:
                                            valueToInsert = "JOBD AND EOBD";
                                            break;
                                        case 13:
                                            valueToInsert = "JOBD EOBD OBD-II";
                                            break;
                                        case 14:
                                            valueToInsert = "RESERVED";
                                            break;
                                        case 15:
                                            valueToInsert = "RESERVED";
                                            break;
                                        case 16:
                                            valueToInsert = "RESERVED";
                                            break;
                                        case 17:
                                            valueToInsert = "EMD";
                                            break;
                                        case 18:
                                            valueToInsert = "EMD+";
                                            break;
                                        case 19:
                                            valueToInsert = "HD OBD-C";
                                            break;
                                        case 20:
                                            valueToInsert = "HD OBD";
                                            break;
                                        case 21:
                                            valueToInsert = "WWH OBD";
                                            break;
                                        case 22:
                                            valueToInsert = "RESERVED";
                                            break;
                                        case 23:
                                            valueToInsert = "HD EOBD-I";
                                            break;
                                        case 24:
                                            valueToInsert = "HD EOBD-I N";
                                            break;
                                        case 25:
                                            valueToInsert = "HD EOBD-II";
                                            break;
                                        case 26:
                                            valueToInsert = "HD EOBD-II N";
                                            break;
                                        case 27:
                                            valueToInsert = "RESERVED";
                                            break;
                                        case 28:
                                            valueToInsert = "OBDBR-1";
                                            break;
                                        case 29:
                                            valueToInsert = "OBDBR-2";
                                            break;
                                        case 30:
                                            valueToInsert = "KOBD";
                                            break;
                                        case 31:
                                            valueToInsert = "IOBD-I";
                                            break;
                                        case 32:
                                            valueToInsert = "IOBD-II";
                                            break;
                                        case 33:
                                            valueToInsert = "HD EOBD-IV";
                                            break;
                                        default:
                                            valueToInsert = Util.ByteToHexString(payload, 1, 1);
                                            break;
                                    }
                                    break;
                                case 0x07:
                                    descriptionToInsert = "CONFIGURATION | CHASSIS TYPE";
                                    valueToInsert = Util.ByteToHexString(payload, 1, 1);
                                    break;
                                case 0x08:
                                    descriptionToInsert = "CONFIGURATION | ASPIRATION TYPE";

                                    switch (payload[1]) // to be verified!
                                    {
                                        case 1:
                                            valueToInsert = "NATURAL";
                                            break;
                                        default:
                                            valueToInsert = Util.ByteToHexString(payload, 1, 1);
                                            break;
                                    }
                                    break;
                                case 0x09:
                                    descriptionToInsert = "CONFIGURATION | INJECTION TYPE";

                                    switch (payload[1]) // to be verified!
                                    {
                                        case 1:
                                            valueToInsert = "DIRECT";
                                            break;
                                        case 2:
                                            valueToInsert = "SEQUENTIAL";
                                            break;
                                        case 3:
                                            valueToInsert = "SINGLE-POINT";
                                            break;
                                        case 4:
                                            valueToInsert = "MULTI-POINT";
                                            break;
                                        default:
                                            valueToInsert = Util.ByteToHexString(payload, 1, 1);
                                            break;
                                    }
                                    break;
                                case 0x0A:
                                    descriptionToInsert = "CONFIGURATION | FUEL TYPE";

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
                                            valueToInsert = Util.ByteToHexString(payload, 1, 1);
                                            break;
                                    }
                                    break;
                                case 0x0B:
                                    descriptionToInsert = "CONFIGURATION | MODEL YEAR";

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
                                            valueToInsert = Util.ByteToHexString(payload, 1, 1);
                                            break;
                                    }
                                    break;
                                case 0x0C:
                                    descriptionToInsert = "CONFIGURATION | ENGINE DISPLACEMENT | CONFIG";

                                    switch (payload[1])
                                    {
                                        case 0x01:
                                            valueToInsert = "2.2L | I4 E-W";
                                            break;
                                        case 0x02:
                                            valueToInsert = "2.5L | I4 E-W";
                                            break;
                                        case 0x03:
                                            valueToInsert = "3.0L | V6 E-W";
                                            break;
                                        case 0x04:
                                            valueToInsert = "3.3L | V6 E-W";
                                            break;
                                        case 0x05:
                                            valueToInsert = "3.9L | V6 N-S";
                                            break;
                                        case 0x06:
                                            valueToInsert = "5.2L | V8 N-S";
                                            break;
                                        case 0x07:
                                            valueToInsert = "5.9L | V8 N-S";
                                            break;
                                        case 0x08:
                                            valueToInsert = "3.8L | V6 E-W";
                                            break;
                                        case 0x09:
                                            valueToInsert = "4.0L | I6 N-S";
                                            break;
                                        case 0x0A:
                                            valueToInsert = "2.0L | I4 E-W SOHC";
                                            break;
                                        case 0x0B:
                                            valueToInsert = "3.5L | V6 N-S";
                                            break;
                                        case 0x0C:
                                            valueToInsert = "8.0L | V10 N-S";
                                            break;
                                        case 0x0D:
                                            valueToInsert = "2.4L | I4 E-W";
                                            break;
                                        case 0x0E:
                                            valueToInsert = "2.5L | I4 N-S";
                                            break;
                                        case 0x0F:
                                            valueToInsert = "2.5L | V6 N-S";
                                            break;
                                        case 0x10:
                                            valueToInsert = "2.0L | I4 E-W DOHC";
                                            break;
                                        case 0x11:
                                            valueToInsert = "2.5L | V6 E-W";
                                            break;
                                        case 0x12:
                                            valueToInsert = "5.9L | I6 N-S";
                                            break;
                                        case 0x13:
                                            valueToInsert = "3.3L | V6 N-S";
                                            break;
                                        case 0x14:
                                            valueToInsert = "2.7L | V6 N-S";
                                            break;
                                        case 0x15:
                                            valueToInsert = "3.2L | V6 N-S";
                                            break;
                                        case 0x16:
                                            valueToInsert = "1.8L | I4 E-W";
                                            break;
                                        case 0x17:
                                            valueToInsert = "3.7L | V6 N-S";
                                            break;
                                        case 0x18:
                                            valueToInsert = "4.7L | V8 N-S";
                                            break;
                                        case 0x19:
                                            valueToInsert = "1.9L | I4 E-W";
                                            break;
                                        case 0x1A:
                                            valueToInsert = "3.1L | I5 N-S";
                                            break;
                                        case 0x1B:
                                            valueToInsert = "1.6L | I4 E-W";
                                            break;
                                        case 0x1C:
                                            valueToInsert = "2.7L | V6 E-W";
                                            break;
                                        default:
                                            valueToInsert = Util.ByteToHexString(payload, 1, 1);
                                            break;
                                    }
                                    break;
                                case 0x0D:
                                    descriptionToInsert = "CONFIGURATION | A/C SYSTEM TYPE";
                                    valueToInsert = Util.ByteToHexString(payload, 1, 1);
                                    break;
                                case 0x0E:
                                    descriptionToInsert = "CONFIGURATION | ENGINE MANUFACTURER";
                                    valueToInsert = Util.ByteToHexString(payload, 1, 1);
                                    break;
                                case 0x0F:
                                    descriptionToInsert = "CONFIGURATION | CONTROLLER HARDWARE TYPE";

                                    switch (payload[1]) // to be verified!
                                    {
                                        case 18:
                                            valueToInsert = "SBEC3";
                                            break;
                                        default:
                                            valueToInsert = Util.ByteToHexString(payload, 1, 1);
                                            break;
                                    }
                                    break;
                                case 0x10:
                                    descriptionToInsert = "CONFIGURATION | BODY STYLE";

                                    switch (payload[1]) // to be verified!
                                    {
                                        case 1:
                                            valueToInsert = "NS";
                                            break;
                                        case 2:
                                            valueToInsert = "PL";
                                            break;
                                        case 3:
                                            valueToInsert = "FJ";
                                            break;
                                        case 4:
                                            valueToInsert = "LH";
                                            break;
                                        case 5:
                                            valueToInsert = "PR";
                                            break;
                                        case 6:
                                            valueToInsert = "JA";
                                            break;
                                        case 7:
                                            valueToInsert = "JX";
                                            break;
                                        default:
                                            valueToInsert = Util.ByteToHexString(payload, 1, 1);
                                            break;
                                    }
                                    break;
                                case 0x11:
                                    descriptionToInsert = "CONFIGURATION | MODULE SOFTWARE PHASE";
                                    valueToInsert = Util.ByteToHexString(payload, 1, 1);
                                    break;
                                case 0x12:
                                    descriptionToInsert = "CONFIGURATION | MODULE SOFTWARE VERSION";
                                    valueToInsert = Util.ByteToHexString(payload, 1, 1);
                                    break;
                                case 0x13:
                                    descriptionToInsert = "CONFIGURATION | MODULE SOFTWARE FAMILY";
                                    valueToInsert = Util.ByteToHexString(payload, 1, 1);
                                    break;
                                case 0x14:
                                    descriptionToInsert = "CONFIGURATION | MODULE SOFTWARE GROUP AND MONTH";
                                    valueToInsert = Util.ByteToHexString(payload, 1, 1);
                                    break;
                                case 0x15:
                                    descriptionToInsert = "CONFIGURATION | MODULE SOFTWARE DAY";
                                    valueToInsert = Util.ByteToHexString(payload, 1, 1);
                                    break;
                                case 0x16:
                                    descriptionToInsert = "CONFIGURATION | TRANSMISSION TYPE";

                                    switch (payload[1])
                                    {
                                        case 1:
                                            valueToInsert = "ATX ?-SPEED";
                                            break;
                                        case 2:
                                            valueToInsert = "MTX";
                                            break;
                                        case 3:
                                            valueToInsert = "ATX 3-SPEED";
                                            break;
                                        case 4:
                                            valueToInsert = "ATX ?-SPEED";
                                            break;
                                        default:
                                            valueToInsert = Util.ByteToHexString(payload, 1, 1);
                                            break;
                                    }
                                    break;
                                case 0x17:
                                    descriptionToInsert = "CONFIGURATION | PART NUMBER REVISION 1";
                                    valueToInsert = Encoding.ASCII.GetString(payload, 1, 1);
                                    break;
                                case 0x18:
                                    descriptionToInsert = "CONFIGURATION | PART NUMBER REVISION 2";
                                    valueToInsert = Encoding.ASCII.GetString(payload, 1, 1);
                                    break;
                                case 0x19:
                                    descriptionToInsert = "CONFIGURATION | SOFTWARE REVISION LEVEL";
                                    valueToInsert = Util.ByteToHexString(payload, 1, 1);
                                    break;
                                case 0x1A:
                                    descriptionToInsert = "CONFIGURATION | HOMOLOGATION NUMBER 1";
                                    valueToInsert = Util.ByteToHexString(payload, 1, 1) + " | " + Encoding.ASCII.GetString(payload, 1, 1);
                                    break;
                                case 0x1B:
                                    descriptionToInsert = "CONFIGURATION | HOMOLOGATION NUMBER 2";
                                    valueToInsert = Util.ByteToHexString(payload, 1, 1) + " | " + Encoding.ASCII.GetString(payload, 1, 1);
                                    break;
                                case 0x1C:
                                    descriptionToInsert = "CONFIGURATION | HOMOLOGATION NUMBER 3";
                                    valueToInsert = Util.ByteToHexString(payload, 1, 1) + " | " + Encoding.ASCII.GetString(payload, 1, 1);
                                    break;
                                case 0x1D:
                                    descriptionToInsert = "CONFIGURATION | HOMOLOGATION NUMBER 4";
                                    valueToInsert = Util.ByteToHexString(payload, 1, 1) + " | " + Encoding.ASCII.GetString(payload, 1, 1);
                                    break;
                                case 0x1E:
                                    descriptionToInsert = "CONFIGURATION | HOMOLOGATION NUMBER 5";
                                    valueToInsert = Util.ByteToHexString(payload, 1, 1) + " | " + Encoding.ASCII.GetString(payload, 1, 1);
                                    break;
                                case 0x1F:
                                    descriptionToInsert = "CONFIGURATION | HOMOLOGATION NUMBER 6";
                                    valueToInsert = Util.ByteToHexString(payload, 1, 1) + " | " + Encoding.ASCII.GetString(payload, 1, 1);
                                    break;
                                default:
                                    descriptionToInsert = "CONFIGURATION | " + "OFFSET: " + Util.ByteToHexString(payload, 0, 1);
                                    valueToInsert = Util.ByteToHexString(payload, 1, 1);
                                    break;
                            }
                        }
                        break;
                    case 0x2B:
                        descriptionToInsert = "GET SECURITY SEED";

                        if (message.Length >= 4)
                        {
                            byte checksum = (byte)(0x2B + payload[0] + payload[1]);

                            if (payload[2] == checksum)
                            {
                                ushort seed = (ushort)((payload[0] << 8) + payload[1]);

                                if (seed != 0)
                                {
                                    ushort key = (ushort)((seed << 2) + 0x9018);
                                    byte keyHB = (byte)(key >> 8);
                                    byte keyLB = (byte)(key);
                                    byte keyChecksum = (byte)(0x2C + keyHB + keyLB);
                                    byte[] keyArray = { 0x2C, keyHB, keyLB, keyChecksum };
                                    descriptionToInsert = "GET SECURITY SEED | KEY: " + Util.ByteToHexStringSimple(keyArray);
                                }
                                else
                                {
                                    descriptionToInsert = "GET SECURITY SEED | PCM ALREADY UNLOCKED";
                                }

                                valueToInsert = Util.ByteToHexString(payload, 0, 2);
                            }
                            else
                            {
                                descriptionToInsert = "GET SECURITY SEED";
                                valueToInsert = "CHECKSUM ERROR";
                            }
                        }
                        break;
                    case 0x2C:
                        descriptionToInsert = "SEND SECURITY KEY";

                        if (message.Length >= 5)
                        {
                            switch (payload[3])
                            {
                                case 0x00:
                                    valueToInsert = "ACCEPTED";
                                    break;
                                case 0x01:
                                    valueToInsert = "INCORRECT KEY";
                                    break;
                                case 0x02:
                                    valueToInsert = "CHECKSUM ERROR";
                                    break;
                                case 0x03:
                                    valueToInsert = "BLOCKED | RESTART PCM";
                                    break;
                                default:
                                    valueToInsert = Util.ByteToHexString(payload, 3, 1);
                                    break;
                            }
                        }
                        break;
                    case 0x2D:
                        descriptionToInsert = "READ CONFIGURATION CONSTANT";

                        if (message.Length >= 5)
                        {
                            descriptionToInsert = "CONFIGURATION | PAGE: " + Util.ByteToHexString(payload, 0, 1) + " | ITEM: " + Util.ByteToHexString(payload, 1, 1);
                            valueToInsert = Util.ByteToHexString(payload, 2, 2);
                        }
                        break;
                    case 0x2E:
                    case 0x32:
                    case 0x33:
                        descriptionToInsert = "ONE-TRIP ENGINE FAULT CODES";

                        if (message.Length >= 3)
                        {
                            //byte checksum = 0;
                            //int checksumLocation = message.Length - 1;

                            //for (int i = 0; i < checksumLocation; i++)
                            //{
                            //    checksum += message[i];
                            //}

                            //if (checksum == message[checksumLocation])
                            //{
                                engineFaultCode1TList.Clear();
                                engineFaultCode1TList.AddRange(engineFaultCode1TPayload);
                                engineFaultCode1TList.Remove(0xFD); // not fault code related
                                engineFaultCode1TList.Remove(0xFE); // end of fault code list signifier

                                if (engineFaultCode1TList.Count > 0)
                                {
                                    valueToInsert = Util.ByteToHexStringSimple(engineFaultCode1TList.ToArray());
                                    engineFaultCodes1TSaved = false;
                                }
                                else
                                {
                                    valueToInsert = "NO FAULT CODES";
                                    engineFaultCodes1TSaved = false;
                                }
                            //}
                            //else
                            //{
                            //    valueToInsert = "CHECKSUM ERROR";
                            //    engineFaultCodes1TSaved = true;
                            //}
                        }
                        break;
                    case 0x35:
                        descriptionToInsert = "GET SECURITY SEED";

                        if (message.Length >= 1)
                        {
                            byte checksum = (byte)(0x35 + payload[0] + payload[1] + payload[2]);

                            if (payload[3] == checksum)
                            {
                                ushort seed = (ushort)((payload[1] << 8) + payload[2]);

                                if (payload[0] == 1)
                                {
                                    if (seed != 0)
                                    {
                                        ushort key = (ushort)((seed << 2) + 0x9018);
                                        byte keyHB = (byte)(key >> 8);
                                        byte keyLB = (byte)(key);
                                        byte keyChecksum = (byte)(0x2C + keyHB + keyLB);
                                        byte[] keyArray = { 0x2C, keyHB, keyLB, keyChecksum };

                                        descriptionToInsert = "GET SECURITY SEED #1 | KEY: " + Util.ByteToHexStringSimple(keyArray);
                                    }
                                    else
                                    {
                                        descriptionToInsert = "GET SECURITY SEED #1 | PCM ALREADY UNLOCKED";
                                    }
                                }
                                else if (payload[0] == 2)
                                {
                                    if (seed != 0)
                                    {
                                        ushort key = (ushort)(seed & 0xFF00);
                                        key |= (ushort)(key >> 8);
                                        ushort mask = (ushort)(seed & 0xFF);
                                        mask |= (ushort)(mask << 8);
                                        key ^= 0x9340; // polinom
                                        key += 0x1010;
                                        key ^= mask;
                                        key += 0x1911;
                                        uint tmp = (uint)((key << 16) | key);
                                        key += (ushort)(tmp >> 3);
                                        byte keyHB = (byte)(key >> 8);
                                        byte keyLB = (byte)(key);
                                        byte keyChecksum = (byte)(0x2C + keyHB + keyLB);
                                        byte[] keyArray = { 0x2C, keyHB, keyLB, keyChecksum };

                                        descriptionToInsert = "GET SECURITY SEED #2 | KEY: " + Util.ByteToHexStringSimple(keyArray);
                                    }
                                    else
                                    {
                                        descriptionToInsert = "GET SECURITY SEED #2 | PCM ALREADY UNLOCKED";
                                    }
                                }

                                valueToInsert = Util.ByteToHexString(payload, 1, 2);
                            }
                            else
                            {
                                if (payload[0] == 1)
                                {
                                    descriptionToInsert = "GET SECURITY SEED #1";
                                }
                                else if (payload[0] == 2)
                                {
                                    descriptionToInsert = "GET SECURITY SEED #2";
                                }

                                valueToInsert = "CHECKSUM ERROR";
                            }
                        }
                        break;
                    case 0xFE:
                        descriptionToInsert = "SELECT LOW-SPEED MODE";
                        break;
                    case 0xFF:
                        descriptionToInsert = "PCM WAKE UP";
                        break;
                    default:
                        descriptionToInsert = string.Empty;
                        break;
                }
            }
            else if ((speed == "62500 baud") || (speed == "125000 baud"))
            {
                switch (ID)
                {
                    case 0x00:
                        descriptionToInsert = "PCM WAKE UP";
                        break;
                    case 0x06:
                        descriptionToInsert = "SET BOOTSTRAP BAUDRATE TO 62500 BAUD";
                        valueToInsert = "OK";
                        break;
                    case 0x11:
                        descriptionToInsert = "UPLOAD WORKER FUNCTION";

                        if (message.Length >= 3)
                        {
                            ushort size = (ushort)((payload[0] << 8) + payload[1]);
                            ushort echoCount = (ushort)(payload.Length - 3);

                            descriptionToInsert = "UPLOAD WORKER FUNCTION | SIZE: " + size.ToString() + " BYTES";
                            valueToInsert = Util.ByteToHexString(payload, 2, payload.Length - 3);

                            if ((echoCount == size) && (payload[payload.Length - 1] == 0x14))
                            {
                                unitToInsert = "OK";
                            }
                            else
                            {
                                unitToInsert = "ERROR";
                            }
                        }
                        break;
                    case 0x21:
                        descriptionToInsert = "START WORKER FUNCTION";

                        if (message.Length > 1)
                        {
                            descriptionToInsert += " | RESULT";
                            valueToInsert = Util.ByteToHexStringSimple(payload.ToArray());
                        }
                        else if ((message.Length == 2) && (payload[0] == 0x22))
                        {
                            valueToInsert = "FINISHED";
                        }
                        break;
                    case 0x22:
                        descriptionToInsert = "EXIT WORKER FUNCTION";
                        break;
                    case 0x24:
                        descriptionToInsert = "REQUEST/SEND BOOTSTRAP SECURITY SEED/KEY";

                        if (message.Length >= 5)
                        {
                            if (message.Length == 5)
                            {
                                byte checksum = (byte)(message[0] + message[1] + message[2] + message[3]);

                                descriptionToInsert = "REQUEST BOOTSTRAP SECURITY SEED";

                                if (message[4] == checksum)
                                {
                                    if ((message[2] == 0x27) && (message[3] == 0xC1))
                                    {
                                        valueToInsert = "OK";
                                    }
                                    else
                                    {
                                        valueToInsert = "ERROR";
                                    }
                                }
                                else
                                {
                                    valueToInsert = "CHECKSUM ERROR";
                                }
                            }
                            else if (message.Length == 7)
                            {
                                byte checksum = (byte)(message[0] + message[1] + message[2] + message[3] + message[4] + message[5]);

                                descriptionToInsert = "SEND BOOTSTRAP SECURITY KEY";

                                if (message[6] == checksum)
                                {
                                    if ((message[2] == 0x27) && (message[3] == 0xC2))
                                    {
                                        valueToInsert = Util.ByteToHexString(message, 4, 2);
                                    }
                                    else
                                    {
                                        valueToInsert = "ERROR";
                                    }
                                }
                                else
                                {
                                    valueToInsert = "CHECKSUM ERROR";
                                }
                            }
                            else
                            {
                                descriptionToInsert = "REQUEST BOOTSTRAP SECURITY SEED";
                            }
                        }
                        break;
                    case 0x26:
                        descriptionToInsert = "BOOTSTRAP SECURITY STATUS";

                        if (message.Length >= 5)
                        {
                            if (message.Length == 5)
                            {
                                byte checksum = (byte)(message[0] + message[1] + message[2] + message[3]);

                                descriptionToInsert = "BOOTSTRAP SECURITY STATUS";

                                if (message[4] == checksum)
                                {
                                    if ((message[2] == 0x67) && (message[3] == 0xC2))
                                    {
                                        valueToInsert = "UNLOCKED";
                                    }
                                    else
                                    {
                                        valueToInsert = "LOCKED";
                                    }
                                }
                                else
                                {
                                    valueToInsert = "CHECKSUM ERROR";
                                }
                            }
                            else if (message.Length == 7)
                            {
                                byte checksum = (byte)(message[0] + message[1] + message[2] + message[3] + message[4] + message[5]);

                                descriptionToInsert = "BOOTSTRAP SECURITY SEED RECEIVED";

                                if (message[6] == checksum)
                                {
                                    if ((message[2] == 0x67) && (message[3] == 0xC1))
                                    {
                                        valueToInsert = Util.ByteToHexString(payload, 3, 2);
                                    }
                                }
                                else
                                {
                                    valueToInsert = "CHECKSUM ERROR";
                                }
                            }
                        }
                        break;
                    case 0x31:
                        descriptionToInsert = "WRITE FLASH BLOCK";

                        if (message.Length >= 7)
                        {
                            List<byte> offset = new List<byte>();
                            List<byte> length = new List<byte>();
                            List<byte> values = new List<byte>();
                            offset.Clear();
                            length.Clear();
                            values.Clear();
                            offset.AddRange(payload.Take(3));
                            length.AddRange(payload.Skip(3).Take(2));
                            values.AddRange(payload.Skip(5));

                            ushort blockSize = (ushort)((payload[3] << 8) + payload[4]);
                            ushort echoCount = (ushort)(payload.Length - 5);

                            descriptionToInsert = "WRITE FLASH BLOCK | OFFSET: " + Util.ByteToHexStringSimple(offset.ToArray()) + " | SIZE: " + Util.ByteToHexStringSimple(length.ToArray());

                            if (echoCount == blockSize)
                            {
                                valueToInsert = Util.ByteToHexStringSimple(values.ToArray());
                                unitToInsert = "OK";
                            }
                            else
                            {
                                switch (message[message.Length - 1]) // last payload byte stores error status
                                {
                                    case 0x01:
                                        valueToInsert = "WRITE ERROR";
                                        break;
                                    case 0x80:
                                        valueToInsert = "INVALID BLOCK SIZE";
                                        break;
                                    default:
                                        valueToInsert = "UNKNOWN ERROR";
                                        break;
                                }
                            }
                        }
                        break;
                    case 0x34:
                    case 0x46: // Dino
                        descriptionToInsert = "READ FLASH BLOCK";

                        if (message.Length >= 7)
                        {
                            List<byte> offset = new List<byte>();
                            List<byte> length = new List<byte>();
                            List<byte> values = new List<byte>();
                            offset.Clear();
                            length.Clear();
                            values.Clear();
                            offset.AddRange(payload.Take(3));
                            length.AddRange(payload.Skip(3).Take(2));
                            values.AddRange(payload.Skip(5));

                            descriptionToInsert = "READ FLASH BLOCK | OFFSET: " + Util.ByteToHexStringSimple(offset.ToArray()) + " | SIZE: " + Util.ByteToHexStringSimple(length.ToArray());

                            ushort blockSize = (ushort)((payload[3] << 8) + payload[4]);
                            ushort echoCount = (ushort)(payload.Length - 5);

                            if (echoCount == blockSize)
                            {
                                valueToInsert = Util.ByteToHexStringSimple(values.ToArray());
                                unitToInsert = "OK";
                            }
                            else
                            {
                                switch (message[message.Length - 1]) // last payload byte stores error status
                                {
                                    case 0x80:
                                        valueToInsert = "INVALID BLOCK SIZE";
                                        break;
                                    default:
                                        valueToInsert = "UNKNOWN ERROR";
                                        break;
                                }
                            }
                        }
                        break;
                    case 0x37:
                        descriptionToInsert = "WRITE EEPROM BLOCK";

                        if (message.Length >= 6)
                        {
                            List<byte> offset = new List<byte>();
                            List<byte> length = new List<byte>();
                            List<byte> values = new List<byte>();
                            offset.Clear();
                            length.Clear();
                            values.Clear();
                            offset.AddRange(payload.Take(2));
                            length.AddRange(payload.Skip(2).Take(2));
                            values.AddRange(payload.Skip(4));

                            descriptionToInsert = "WRITE EEPROM BLOCK | OFFSET: " + Util.ByteToHexStringSimple(offset.ToArray()) + " | SIZE: " + Util.ByteToHexStringSimple(length.ToArray());

                            ushort blockSize = (ushort)((payload[2] << 8) + payload[3]);
                            ushort echoCount = (ushort)(payload.Length - 4);

                            if ((echoCount == blockSize) && (offset[0] < 2))
                            {
                                valueToInsert = Util.ByteToHexStringSimple(values.ToArray());
                                unitToInsert = "OK";
                            }
                            else
                            {
                                switch (message[message.Length - 1]) // last payload byte stores error status
                                {
                                    case 0x80:
                                        valueToInsert = "INVALID BLOCK SIZE";
                                        break;
                                    case 0x83:
                                        valueToInsert = "INVALID OFFSET";
                                        break;
                                    default:
                                        valueToInsert = "UNKNOWN ERROR";
                                        break;
                                }
                            }
                        }
                        break;
                    case 0x3A:
                        descriptionToInsert = "READ EEPROM BLOCK";

                        if (message.Length >= 6)
                        {
                            List<byte> offset = new List<byte>();
                            List<byte> length = new List<byte>();
                            List<byte> values = new List<byte>();
                            offset.Clear();
                            length.Clear();
                            values.Clear();
                            offset.AddRange(payload.Take(2));
                            length.AddRange(payload.Skip(2).Take(2));
                            values.AddRange(payload.Skip(4));

                            descriptionToInsert = "READ EEPROM BLOCK | OFFSET: " + Util.ByteToHexStringSimple(offset.ToArray()) + " | SIZE: " + Util.ByteToHexStringSimple(length.ToArray());

                            ushort blockSize = (ushort)((payload[2] << 8) + payload[3]);
                            ushort echoCount = (ushort)(payload.Length - 4);

                            if ((echoCount == blockSize) && (offset[0] < 2))
                            {
                                valueToInsert = Util.ByteToHexStringSimple(values.ToArray());
                                unitToInsert = "OK";
                            }
                            else
                            {
                                switch (message[message.Length - 1]) // last payload byte stores error status
                                {
                                    case 0x80:
                                        valueToInsert = "INVALID BLOCK SIZE";
                                        break;
                                    case 0x83:
                                        valueToInsert = "INVALID OFFSET";
                                        break;
                                    default:
                                        valueToInsert = "UNKNOWN ERROR";
                                        break;
                                }
                            }
                        }
                        break;
                    case 0x47:
                        descriptionToInsert = "START BOOTLOADER";

                        if (message.Length >= 4)
                        {
                            List<byte> offset = new List<byte>();
                            offset.Clear();
                            offset.AddRange(payload.Take(2));
                            descriptionToInsert = "START BOOTLOADER | OFFSET: " + Util.ByteToHexStringSimple(offset.ToArray());

                            if (payload[2] == 0x22) valueToInsert = "OK";
                            else valueToInsert = "ERROR";
                        }
                        else
                        {
                            valueToInsert = "ERROR";
                        }
                        break;
                    case 0x4C:
                        descriptionToInsert = "UPLOAD BOOTLOADER";

                        if (message.Length >= 6)
                        {
                            List<byte> offsetStart = new List<byte>();
                            List<byte> offsetEnd = new List<byte>();
                            offsetStart.Clear();
                            offsetEnd.Clear();
                            offsetStart.AddRange(payload.Take(2));
                            offsetEnd.AddRange(payload.Skip(2).Take(2));
                            descriptionToInsert = "UPLOAD BOOTLOADER | START: " + Util.ByteToHexStringSimple(offsetStart.ToArray()) + " | END: " + Util.ByteToHexStringSimple(offsetEnd.ToArray());
                            valueToInsert = Util.ByteToHexString(payload, 4, payload.Length - 4);

                            ushort start = (ushort)((payload[0] << 8) + payload[1]);
                            ushort end = (ushort)((payload[2] << 8) + payload[3]);

                            if ((end - start + 1) == (payload.Length - 4))
                            {
                                unitToInsert = "OK";
                            }
                            else
                            {
                                unitToInsert = "ERROR";
                            }
                        }
                        break;
                    case 0xDB:
                        descriptionToInsert = string.Empty;

                        if (message.Length >= 5)
                        {
                            if (payload[0] == 0x2F && payload[1] == 0xD8 && payload[2] == 0x3E && payload[3] == 0x23)
                            {
                                descriptionToInsert = "BOOTSTRAP MODE NOT PROTECTED";
                            }
                            else
                            {
                                descriptionToInsert = "PING";
                            }
                        }
                        break;
                    case 0xF0:
                        descriptionToInsert = "F0 RAM TABLE SELECTED";

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
                        break;
                    case 0xF1:
                        descriptionToInsert = "F1 RAM TABLE SELECTED";

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
                        break;
                    case 0xF2:
                        descriptionToInsert = "F2 RAM TABLE SELECTED";

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
                        break;
                    case 0xF3:
                        descriptionToInsert = "F3 RAM TABLE SELECTED";

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
                        break;
                    case 0xF4:
                        descriptionToInsert = "F4 RAM TABLE SELECTED";

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
                                    break;
                                case 0x0A:
                                    descriptionToInsert = "ENGINE SPEED";

                                    if ((payload.Length >= 4) && (payload[2] == 0x0B))
                                    {
                                        valueToInsert = Math.Round(((payload[1] << 8) + payload[3]) * 0.125, 3).ToString("0.000").Replace(",", ".");
                                        unitToInsert = "RPM";
                                    }
                                    break;
                                case 0x0C:
                                    descriptionToInsert = "VEHICLE SPEED";

                                    if ((payload.Length >= 4) && (payload[2] == 0x0D))
                                    {
                                        if (Properties.Settings.Default.Units == "imperial")
                                        {
                                            valueToInsert = Math.Round(((payload[1] << 8) + payload[3]) * 0.0156, 3).ToString("0.000").Replace(",", ".");
                                            unitToInsert = "MPH";
                                        }
                                        else if (Properties.Settings.Default.Units == "metric")
                                        {
                                            valueToInsert = Math.Round(((payload[1] << 8) + payload[3]) * 0.0156 * 1.609344, 3).ToString("0.000").Replace(",", ".");
                                            unitToInsert = "KM/H";
                                        }
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

                                            if (valueToInsert.Length > 2) valueToInsert = valueToInsert.Remove(valueToInsert.Length - 3); // remove last "|" character
                                        }
                                    }
                                    break;
                                case 0x0F:
                                    descriptionToInsert = "BATTERY VOLTAGE";

                                    if (payload.Length >= 2)
                                    {
                                        valueToInsert = Math.Round(payload[1] * 0.0618, 3).ToString("0.000").Replace(",", ".");
                                        unitToInsert = "V";
                                    }
                                    break;
                                case 0x10:
                                    descriptionToInsert = "AMBIENT TEMPERATURE SENSOR VOLTAGE";

                                    if (payload.Length >= 2)
                                    {
                                        valueToInsert = Math.Round(payload[1] * 0.0196, 3).ToString("0.000").Replace(",", ".");
                                        unitToInsert = "V";
                                    }
                                    break;
                                case 0x11:
                                    descriptionToInsert = "AMBIENT TEMPERATURE";

                                    if (payload.Length >= 2)
                                    {
                                        if (Properties.Settings.Default.Units == "imperial")
                                        {
                                            valueToInsert = Math.Round((payload[1] * 1.8) - 198.4).ToString("0");
                                            unitToInsert = "°F";
                                        }
                                        else if (Properties.Settings.Default.Units == "metric")
                                        {
                                            valueToInsert = (payload[1] - 128).ToString("0");
                                            unitToInsert = "°C";
                                        }
                                    }
                                    break;
                                case 0x12:
                                    descriptionToInsert = "THROTTLE POSITION SENSOR (TPS) VOLTAGE";

                                    if (payload.Length >= 2)
                                    {
                                        valueToInsert = Math.Round(payload[1] * 0.0196, 3).ToString("0.000").Replace(",", ".");
                                        unitToInsert = "V";
                                    }
                                    break;
                                case 0x13:
                                    descriptionToInsert = "MINIMUM TPS VOLTAGE";

                                    if (payload.Length >= 2)
                                    {
                                        valueToInsert = Math.Round(payload[1] * 0.0196, 3).ToString("0.000").Replace(",", ".");
                                        unitToInsert = "V";
                                    }
                                    break;
                                case 0x14:
                                    descriptionToInsert = "CALCULATED TPS VOLTAGE";

                                    if (payload.Length >= 2)
                                    {
                                        valueToInsert = Math.Round(payload[1] * 0.0196, 3).ToString("0.000").Replace(",", ".");
                                        unitToInsert = "V";
                                    }
                                    break;
                                case 0x15:
                                    descriptionToInsert = "ENGINE COOLANT TEMPERATURE SENSOR VOLTAGE";

                                    if (payload.Length >= 2)
                                    {
                                        valueToInsert = Math.Round(payload[1] * 0.0196, 3).ToString("0.000").Replace(",", ".");
                                        unitToInsert = "V";
                                    }
                                    break;
                                case 0x16:
                                    descriptionToInsert = "ENGINE COOLANT TEMPERATURE";

                                    if (payload.Length >= 2)
                                    {
                                        if (Properties.Settings.Default.Units == "imperial")
                                        {
                                            valueToInsert = Math.Round((payload[1] * 1.8) - 198.4).ToString("0");
                                            unitToInsert = "°F";
                                        }
                                        else if (Properties.Settings.Default.Units == "metric")
                                        {
                                            valueToInsert = (payload[1] - 128).ToString("0");
                                            unitToInsert = "°C";
                                        }
                                    }
                                    break;
                                case 0x21:
                                    descriptionToInsert = "CRUISE | SWITCH VOLTAGE";

                                    if (payload.Length >= 2)
                                    {
                                        valueToInsert = Math.Round(payload[1] * 0.0196, 3).ToString("0.000").Replace(",", ".");
                                        unitToInsert = "V";
                                    }
                                    break;
                                case 0x35:
                                    descriptionToInsert = "TARGET IDLE SPEED";

                                    if ((payload.Length >= 4) && (payload[2] == 0x36))
                                    {
                                        valueToInsert = Math.Round(((payload[1] << 8) + payload[3]) * 0.125, 3).ToString("0.000").Replace(",", ".");
                                        unitToInsert = "RPM";
                                    }
                                    break;
                                case 0x37:
                                    descriptionToInsert = "TARGET IDLE AIR CONTROL MOTOR STEPS";

                                    if (payload.Length >= 2)
                                    {
                                        valueToInsert = payload[1].ToString();
                                    }
                                    break;
                                case 0x3A:
                                    descriptionToInsert = "CHARGING VOLTAGE";

                                    if (payload.Length >= 2)
                                    {
                                        valueToInsert = Math.Round(payload[1] * 0.0618, 3).ToString("0.000").Replace(",", ".");
                                        unitToInsert = "V";
                                    }
                                    break;
                                case 0x3B:
                                    descriptionToInsert = "CRUISE | SET SPEED";

                                    if (payload.Length >= 2)
                                    {
                                        if (Properties.Settings.Default.Units == "imperial")
                                        {
                                            valueToInsert = Math.Round(payload[1] / 2.0, 1).ToString("0.0").Replace(",", ".");
                                            unitToInsert = "MPH";
                                        }
                                        else if (Properties.Settings.Default.Units == "metric")
                                        {
                                            valueToInsert = Math.Round((payload[1] / 2.0) * 1.609344, 1).ToString("0.0").Replace(",", ".");
                                            unitToInsert = "KM/H";
                                        }
                                    }
                                    break;
                                case 0x3E:
                                    descriptionToInsert = "IDLE AIR CONTROL MOTOR STEPS";

                                    if (payload.Length >= 2)
                                    {
                                        valueToInsert = payload[1].ToString();
                                    }
                                    else // error
                                    {
                                        valueToInsert = "ERROR";
                                    }
                                    break;
                                case 0x3F:
                                    descriptionToInsert = "CRUISE STATUS";

                                    if (payload.Length >= 2)
                                    {
                                        string lastCruiseCutoutReason;
                                        string cruiseDeniedReason;

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
                                    }
                                    break;
                                case 0x41: // cam/crank sensor state
                                    descriptionToInsert = "CRANKSHAFT/CAMSHAFT POSITION SENSOR STATE";

                                    if (payload.Length >= 2)
                                    {
                                        if (Util.IsBitSet(payload[1], 5)) descriptionToInsert = "CKP: PRESENT | ";
                                        else descriptionToInsert = "CKP: LOST | ";

                                        if (Util.IsBitSet(payload[1], 6)) descriptionToInsert += "CMP: PRESENT | ";
                                        else descriptionToInsert += "CMP: LOST | ";

                                        if (Util.IsBitSet(payload[1], 4)) descriptionToInsert += "CKP/CMP: IN-SYNC";
                                        else descriptionToInsert += "CKP/CMP: OUT-OF-SYNC";

                                        if (Util.IsBitSet(payload[1], 0)) valueToInsert = "HISTORY: IN-SYNC";
                                        else valueToInsert = "HISTORY: OUT-OF-SYNC";
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
                        break;
                    case 0xF5:
                        descriptionToInsert = "F5 RAM TABLE SELECTED";

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
                        break;
                    case 0xF6:
                        descriptionToInsert = "F6 RAM TABLE SELECTED";

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
                        break;
                    case 0xF7:
                        descriptionToInsert = "F7 RAM TABLE SELECTED";

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
                        break;
                    case 0xF8:
                        descriptionToInsert = "F8 RAM TABLE SELECTED";

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
                        break;
                    case 0xF9:
                        descriptionToInsert = "F9 RAM TABLE SELECTED";

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
                        break;
                    case 0xFA:
                        descriptionToInsert = "FA RAM TABLE SELECTED";

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
                        break;
                    case 0xFB:
                        descriptionToInsert = "FB RAM TABLE SELECTED";

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
                        break;
                    case 0xFC:
                        descriptionToInsert = "FC RAM TABLE SELECTED";

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
                        break;
                    case 0xFD:
                        descriptionToInsert = "FD RAM TABLE SELECTED";

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
                        break;
                    case 0xFE:
                        descriptionToInsert = "SELECT LOW-SPEED MODE";
                        break;
                    case 0xFF:
                        descriptionToInsert = "PCM WAKE UP";
                        break;
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

            if (speed == "62500 baud") Diagnostics.AddRAMTableDump(data);

            UpdateHeader();

            if (Properties.Settings.Default.Timestamp == true)
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
                    sb.Append("ENGINE FAULT CODE LIST:" + Environment.NewLine);

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
                    sb.Append("NO ENGINE FAULT CODES");
                    File.AppendAllText(MainForm.PCMLogFilename, Environment.NewLine + sb.ToString() + Environment.NewLine + Environment.NewLine);
                }

                engineFaultCodesSaved = true;
            }

            if (!engineFaultCodes1TSaved)
            {
                StringBuilder sb = new StringBuilder();

                if (engineFaultCode1TList.Count > 0)
                {
                    sb.Append("ONE-TRIP ENGINE FAULT CODE LIST:" + Environment.NewLine);

                    foreach (byte code in engineFaultCode1TList)
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
                    sb.Append("NO ENGINE ONE-TRIP FAULT CODES");
                    File.AppendAllText(MainForm.PCMLogFilename, Environment.NewLine + sb.ToString() + Environment.NewLine + Environment.NewLine);
                }

                engineFaultCodes1TSaved = true;
            }
        }
    }
}
