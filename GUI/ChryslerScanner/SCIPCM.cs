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
        public List<byte> EngineFaultCodeList = new List<byte>();
        public bool EngineFaultCodesSaved = true;
        public List<byte> EngineFaultCode1TList = new List<byte>();
        public bool EngineFaultCodes1TSaved = true;
        public byte[] EngineDTCList;
        public DataColumn column;
        public DataRow row;

        private const int HexBytesColumnStart = 2;
        private const int DescriptionColumnStart = 28;
        private const int ValueColumnStart = 82;
        private const int UnitColumnStart = 108;

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
            row["description"] = "CRUISE CONTROL SOLENOID CIRCUITS";
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
            row["description"] = "KNOCK SENSOR #1 CIRCUIT";
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
            row["description"] = "CRUISE CONTROL POWER RELAY; OR S/C 12V DRIVER CIRCUIT";
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
            row["description"] = "CRUISE CONTROL SWITCH ALWAYS HIGH";
            EngineDTC.Rows.Add(row);

            row = EngineDTC.NewRow();
            row["id"] = 0x57;
            row["description"] = "CRUISE CONTROL SWITCH ALWAYS LOW";
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

            EngineDTCList = EngineDTC.AsEnumerable().Select(r => r.Field<byte>("id")).ToArray();
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

            string DescriptionToInsert = string.Empty;
            string ValueToInsert = string.Empty;
            string UnitToInsert = string.Empty;

            if ((speed == "976.5 baud") || (speed == "7812.5 baud"))
            {
                switch (ID)
                {
                    case 0x00:
                        DescriptionToInsert = "PCM WAKE UP";
                        break;
                    case 0x10:
                        DescriptionToInsert = "ENGINE FAULT CODE LIST";

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
                                EngineFaultCodeList.Clear();
                                EngineFaultCodeList.AddRange(engineFaultCodePayload);
                                EngineFaultCodeList.Remove(0xFD); // not fault code related
                                EngineFaultCodeList.Remove(0xFE); // end of fault code list signifier

                                if (EngineFaultCodeList.Count > 0)
                                {
                                    ValueToInsert = Util.ByteToHexStringSimple(EngineFaultCodeList.ToArray());
                                    EngineFaultCodesSaved = false;
                                }
                                else
                                {
                                    ValueToInsert = "NO FAULT CODES";
                                    EngineFaultCodesSaved = false;
                                }
                            }
                            else
                            {
                                ValueToInsert = "CHECKSUM ERROR";
                                EngineFaultCodesSaved = true;
                            }
                        }
                        break;
                    case 0x11:
                        DescriptionToInsert = "FAULT BIT LIST";

                        if (message.Length >= 2)
                        {
                            ValueToInsert = Util.ByteToHexStringSimple(payload);
                        }
                        break;
                    case 0x12:
                        DescriptionToInsert = "SELECT HIGH-SPEED MODE";
                        break;
                    case 0x13:
                        DescriptionToInsert = "ACTUATOR TEST";

                        if (message.Length >= 3)
                        {
                            switch (payload[0])
                            {
                                case 0x00:
                                    ValueToInsert = "STOPPED";
                                    break;
                                case 0x01:
                                    DescriptionToInsert = "ACTUATOR TEST | IGNITION COIL BANK #1";
                                    break;
                                case 0x02:
                                    DescriptionToInsert = "ACTUATOR TEST | IGNITION COIL BANK #2";
                                    break;
                                case 0x03:
                                    DescriptionToInsert = "ACTUATOR TEST | IGNITION COIL BANK #3";
                                    break;
                                case 0x04:
                                    DescriptionToInsert = "ACTUATOR TEST | FUEL INJECTOR BANK #1";
                                    break;
                                case 0x05:
                                    DescriptionToInsert = "ACTUATOR TEST | FUEL INJECTOR BANK #2";
                                    break;
                                case 0x06:
                                    DescriptionToInsert = "ACTUATOR TEST | FUEL INJECTOR BANK #3";
                                    break;
                                case 0x07:
                                    DescriptionToInsert = "ACTUATOR TEST | IDLE AIR CONTROL MOTOR";
                                    break;
                                case 0x08:
                                    DescriptionToInsert = "ACTUATOR TEST | RADIATOR FAN RELAY";
                                    break;
                                case 0x09:
                                    DescriptionToInsert = "ACTUATOR TEST | A/C CLUTCH RELAY";
                                    break;
                                case 0x0A:
                                    DescriptionToInsert = "ACTUATOR TEST | AUTOMATIC SHUTDOWN RELAY";
                                    break;
                                case 0x0B:
                                    DescriptionToInsert = "ACTUATOR TEST | EVAP PURGE SOLENOID";
                                    break;
                                case 0x0C:
                                    DescriptionToInsert = "ACTUATOR TEST | CRUISE CONTROL SOLENOIDS";
                                    break;
                                case 0x0D:
                                    DescriptionToInsert = "ACTUATOR TEST | ALTERNATOR FIELD";
                                    break;
                                case 0x0E:
                                    DescriptionToInsert = "ACTUATOR TEST | TACHOMETER OUTPUT";
                                    break;
                                case 0x0F:
                                    DescriptionToInsert = "ACTUATOR TEST | TORQUE CONVERTER CLUTCH";
                                    break;
                                case 0x10:
                                    DescriptionToInsert = "ACTUATOR TEST | EGR SOLENOID";
                                    break;
                                case 0x11:
                                    DescriptionToInsert = "ACTUATOR TEST | WASTEGATE SOLENOID";
                                    break;
                                case 0x12:
                                    DescriptionToInsert = "ACTUATOR TEST | BAROMETER SOLENOID";
                                    break;
                                case 0x14:
                                    DescriptionToInsert = "ACTUATOR TEST | ALL SOLENOIDS / RELAYS";
                                    break;
                                case 0x16:
                                    DescriptionToInsert = "ACTUATOR TEST | TRANSMISSION O/D SOLENOID";
                                    break;
                                case 0x17:
                                    DescriptionToInsert = "ACTUATOR TEST | SHIFT INDICATOR LAMP";
                                    break;
                                case 0x19:
                                    DescriptionToInsert = "ACTUATOR TEST | SURGE VALVE SOLENOID";
                                    break;
                                case 0x1A:
                                    DescriptionToInsert = "ACTUATOR TEST | CRUISE CONTROL VENT SOLENOID";
                                    break;
                                case 0x1B:
                                    DescriptionToInsert = "ACTUATOR TEST | CRUISE CONTROL VACUUM SOLENOID";
                                    break;
                                case 0x1C:
                                    DescriptionToInsert = "ACTUATOR TEST | ASD FUEL SYSTEM";
                                    break;
                                case 0x1D:
                                    DescriptionToInsert = "ACTUATOR TEST | FUEL INJECTOR BANK #4";
                                    break;
                                case 0x1E:
                                    DescriptionToInsert = "ACTUATOR TEST | FUEL INJECTOR BANK #5";
                                    break;
                                case 0x1F:
                                    DescriptionToInsert = "ACTUATOR TEST | FUEL INJECTOR BANK #6";
                                    break;
                                case 0x23:
                                    DescriptionToInsert = "ACTUATOR TEST | IGNITION COIL BANK #4";
                                    break;
                                case 0x24:
                                    DescriptionToInsert = "ACTUATOR TEST | IGNITION COIL BANK #5";
                                    break;
                                case 0x25:
                                    DescriptionToInsert = "ACTUATOR TEST | FUEL INJECTOR BANK #7";
                                    break;
                                case 0x26:
                                    DescriptionToInsert = "ACTUATOR TEST | FUEL INJECTOR BANK #8";
                                    break;
                                case 0x28:
                                    DescriptionToInsert = "ACTUATOR TEST | INTAKE HEATER BANK #1";
                                    break;
                                case 0x29:
                                    DescriptionToInsert = "ACTUATOR TEST | INTAKE HEATER BANK #2";
                                    break;
                                case 0x2C:
                                    DescriptionToInsert = "ACTUATOR TEST | CRUISE CONTROL 12V FEED";
                                    break;
                                case 0x2D:
                                    DescriptionToInsert = "ACTUATOR TEST | INTAKE MANIFOLD TUNE VALVE";
                                    break;
                                case 0x2E:
                                    DescriptionToInsert = "ACTUATOR TEST | LOW SPEED RADIATOR FAN RELAY";
                                    break;
                                case 0x2F:
                                    DescriptionToInsert = "ACTUATOR TEST | HIGH SPEED RADIATOR FAN RELAY";
                                    break;
                                case 0x30:
                                    DescriptionToInsert = "ACTUATOR TEST | FUEL INJECTOR BANK #9";
                                    break;
                                case 0x31:
                                    DescriptionToInsert = "ACTUATOR TEST | FUEL INJECTOR BANK #10";
                                    break;
                                case 0x32:
                                    DescriptionToInsert = "ACTUATOR TEST | 2-3 LOCKOUT SOLENOID";
                                    break;
                                case 0x33:
                                    DescriptionToInsert = "ACTUATOR TEST | FUEL PUMP RELAY";
                                    break;
                                case 0x3B:
                                    DescriptionToInsert = "ACTUATOR TEST | IAC MOTOR STEP UP";
                                    break;
                                case 0x3C:
                                    DescriptionToInsert = "ACTUATOR TEST | IAC MOTOR STEP DOWN";
                                    break;
                                case 0x3D:
                                    DescriptionToInsert = "ACTUATOR TEST | LD PUMP SOLENOID";
                                    break;
                                case 0x40:
                                    DescriptionToInsert = "ACTUATOR TEST | O2 SENSOR HEATER RELAY";
                                    break;
                                case 0x41:
                                    DescriptionToInsert = "ACTUATOR TEST | OVERDRIVE LAMP";
                                    break;
                                case 0x43:
                                    DescriptionToInsert = "ACTUATOR TEST | TRANSMISSION 12V RELAY";
                                    break;
                                case 0x44:
                                    DescriptionToInsert = "ACTUATOR TEST | REVERSE LOCKOUT SOLENOID";
                                    break;
                                case 0x46:
                                    DescriptionToInsert = "ACTUATOR TEST | SHORT RUNNER VALVE";
                                    break;
                                case 0x49:
                                    DescriptionToInsert = "ACTUATOR TEST | WAIT TO START LAMP";
                                    break;
                                case 0x52:
                                    DescriptionToInsert = "ACTUATOR TEST | O2 X/1 SENSOR HEATER RELAY";
                                    break;
                                case 0x53:
                                    DescriptionToInsert = "ACTUATOR TEST | O2 X/2 SENSOR HEATER RELAY";
                                    break;
                                case 0x56:
                                    DescriptionToInsert = "ACTUATOR TEST | 1/1 O2 SENSOR HEATER RELAY";
                                    break;
                                case 0x57:
                                    DescriptionToInsert = "ACTUATOR TEST | O2 SENSOR HEATER RELAY";
                                    break;
                                case 0x5A:
                                    DescriptionToInsert = "ACTUATOR TEST | RADIATOR FAN SOLENOID";
                                    break;
                                case 0x5B:
                                    DescriptionToInsert = "ACTUATOR TEST | 1/2 O2 SENSOR HEATER RELAY";
                                    break;
                                case 0x5D:
                                    DescriptionToInsert = "ACTUATOR TEST | EXHAUST BRAKE";
                                    break;
                                case 0x5E:
                                    DescriptionToInsert = "ACTUATOR TEST | FUEL CONTROL";
                                    break;
                                case 0x5F:
                                    DescriptionToInsert = "ACTUATOR TEST | PWM RADIATOR FAN";
                                    break;
                                default:
                                    DescriptionToInsert = "ACTUATOR TEST | MODE: " + Util.ByteToHexString(payload, 0, 1);
                                    break;
                            }

                            if (payload[0] != 0)
                            {
                                if (payload[0] == payload[1])
                                {
                                    ValueToInsert = "RUNNING";
                                }
                                else
                                {
                                    ValueToInsert = "MODE NOT AVAILABLE";
                                }
                            }
                        }
                        break;
                    case 0x14:
                        DescriptionToInsert = "REQUEST DIAGNOSTIC DATA";

                        if (message.Length >= 3)
                        {
                            switch (payload[0])
                            {
                                case 0x01:
                                    double BTSVoltage = payload[1] * 0.0196;
                                    DescriptionToInsert = "BATTERY TEMPERATURE SENSOR VOLTAGE";
                                    ValueToInsert = Math.Round(BTSVoltage, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0x02:
                                    double O2S11Voltage = payload[1] * 0.0196;
                                    DescriptionToInsert = "UPSTREAM O2 1/1 SENSOR VOLTAGE";
                                    ValueToInsert = Math.Round(O2S11Voltage, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0x05:
                                    DescriptionToInsert = "ENGINE COOLANT TEMPERATURE";

                                    double ECTC = payload[1] - 128;
                                    double ECTF = 1.8 * ECTC + 32.0;

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        ValueToInsert = Math.Round(ECTF).ToString("0");
                                        UnitToInsert = "°F";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        ValueToInsert = ECTC.ToString("0");
                                        UnitToInsert = "°C";
                                    }
                                    break;
                                case 0x06:
                                    double ECTSVoltage = payload[1] * 0.0196;
                                    DescriptionToInsert = "ENGINE COOLANT TEMPERATURE SENSOR VOLTAGE";
                                    ValueToInsert = Math.Round(ECTSVoltage, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0x07:
                                    double TPSVoltage = payload[1] * 0.0196;
                                    DescriptionToInsert = "THROTTLE POSITION SENSOR VOLTAGE";
                                    ValueToInsert = Math.Round(TPSVoltage, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0x08:
                                    double MinTPSVoltage = payload[1] * 0.0196;
                                    DescriptionToInsert = "MINIMUM TPS VOLTAGE";
                                    ValueToInsert = Math.Round(MinTPSVoltage, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0x09:
                                    double KnockSensorVoltage = payload[1] * 0.0196;
                                    DescriptionToInsert = "KNOCK SENSOR 1 VOLTAGE";
                                    ValueToInsert = Math.Round(KnockSensorVoltage, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0x0A:
                                    double BatteryVoltage = payload[1] * 0.0625;
                                    DescriptionToInsert = "BATTERY VOLTAGE";
                                    ValueToInsert = Math.Round(BatteryVoltage, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0x0B:
                                    double MAPPSI = payload[1] * 0.059756;
                                    double MAPKPA = payload[1] * 0.059756 * 6.894757;
                                    DescriptionToInsert = "INTAKE MANIFOLD ABSOLUTE PRESSURE (MAP)";

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        ValueToInsert = Math.Round(MAPPSI, 1).ToString("0.0").Replace(",", ".");
                                        UnitToInsert = "PSI";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        ValueToInsert = Math.Round(MAPKPA, 1).ToString("0.0").Replace(",", ".");
                                        UnitToInsert = "KPA";
                                    }
                                    break;
                                case 0x0C:
                                    DescriptionToInsert = "TARGET IAC STEPPER MOTOR POSITION";
                                    ValueToInsert = payload[1].ToString("0");
                                    break;
                                case 0x0E:
                                    double LTFT1 = payload[1] * 0.196;
                                    DescriptionToInsert = "LONG TERM FUEL TRIM 1";
                                    ValueToInsert = Math.Round(LTFT1, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "PERCENT";
                                    break;
                                case 0x0F:
                                    double BarometricPressurePSI = payload[1] * 0.059756;
                                    double BarometricPressureKPA = payload[1] * 0.059756 * 6.894757;
                                    DescriptionToInsert = "BAROMETRIC PRESSURE";

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        ValueToInsert = Math.Round(BarometricPressurePSI, 1).ToString("0.0").Replace(",", ".");
                                        UnitToInsert = "PSI";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        ValueToInsert = Math.Round(BarometricPressureKPA, 1).ToString("0.0").Replace(",", ".");
                                        UnitToInsert = "KPA";
                                    }
                                    break;
                                case 0x10:
                                    DescriptionToInsert = "MINIMUM AIR FLOW TEST";
                                    if (payload[1] == 0) ValueToInsert = "STOPPED";
                                    else ValueToInsert = "RUNNING";
                                    break;
                                case 0x11:
                                    double EngineSpeed = payload[1] * 32.0;
                                    DescriptionToInsert = "ENGINE SPEED";
                                    ValueToInsert = EngineSpeed.ToString("0");
                                    UnitToInsert = "RPM";
                                    break;
                                case 0x12:
                                    DescriptionToInsert = "CAM/CRANK SYNC SENSE";

                                    if (Util.IsBitSet(payload[1], 4)) ValueToInsert = "IN-SYNC";
                                    else ValueToInsert = "ENGINE STOPPED";
                                    break;
                                case 0x13:
                                    DescriptionToInsert = "KEY-ON CYCLES ERROR 1";
                                    ValueToInsert = payload[1].ToString("0");
                                    break;
                                case 0x15:
                                    double SparkAdvance = payload[1] * 0.5;
                                    DescriptionToInsert = "SPARK ADVANCE";
                                    ValueToInsert = Math.Round(SparkAdvance, 1).ToString("0.0").Replace(",", ".");
                                    UnitToInsert = "DEG";
                                    break;
                                case 0x16:
                                case 0x21:
                                    double Cylinder1Retard = payload[1] * 0.5;
                                    DescriptionToInsert = "CYLINDER 1 RETARD";
                                    ValueToInsert = Math.Round(Cylinder1Retard, 1).ToString("0.0").Replace(",", ".");
                                    UnitToInsert = "DEG";
                                    break;
                                case 0x17:
                                    double Cylinder2Retard = payload[1] * 0.5;
                                    DescriptionToInsert = "CYLINDER 2 RETARD";
                                    ValueToInsert = Math.Round(Cylinder2Retard, 1).ToString("0.0").Replace(",", ".");
                                    UnitToInsert = "DEG";
                                    break;
                                case 0x18:
                                    double Cylinder3Retard = payload[1] * 0.5;
                                    DescriptionToInsert = "CYLINDER 3 RETARD";
                                    ValueToInsert = Math.Round(Cylinder3Retard, 1).ToString("0.0").Replace(",", ".");
                                    UnitToInsert = "DEG";
                                    break;
                                case 0x19:
                                    double Cylinder4Retard = payload[1] * 0.5;
                                    DescriptionToInsert = "CYLINDER 4 RETARD";
                                    ValueToInsert = Math.Round(Cylinder4Retard, 1).ToString("0.0").Replace(",", ".");
                                    UnitToInsert = "DEG";
                                    break;
                                case 0x1A:
                                    double TargetBoostPSI = payload[1] * 0.115294117;
                                    double TargetBoostKPA = payload[1] * 0.115294117 * 6.89475729;
                                    DescriptionToInsert = "TARGET BOOST";

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        ValueToInsert = Math.Round(TargetBoostPSI, 1).ToString("0.0").Replace(",", ".");
                                        UnitToInsert = "PSI";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        ValueToInsert = Math.Round(TargetBoostKPA, 1).ToString("0.0").Replace(",", ".");
                                        UnitToInsert = "KPA";
                                    }
                                    break;
                                case 0x1B:
                                    double IntakeAirTemperatureC = payload[1] - 128;
                                    double IntakeAirTemperatureF = 1.8 * IntakeAirTemperatureC + 32.0;
                                    DescriptionToInsert = "INTAKE AIR TEMPERATURE";

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        ValueToInsert = Math.Round(IntakeAirTemperatureF).ToString("0");
                                        UnitToInsert = "°F";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        ValueToInsert = IntakeAirTemperatureC.ToString("0");
                                        UnitToInsert = "°C";
                                    }
                                    break;
                                case 0x1C:
                                    double IATVoltage = payload[1] * 0.0196;
                                    DescriptionToInsert = "INTAKE AIR TEMPERATURE SENSOR VOLTAGE";
                                    ValueToInsert = Math.Round(IATVoltage, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0x1D:
                                    double CruiseSetSpeedMPH = payload[1] * 0.5;
                                    double CruiseSetSpeedKMH = CruiseSetSpeedMPH * 1.609344;
                                    DescriptionToInsert = "CRUISE SET SPEED";

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        ValueToInsert = CruiseSetSpeedMPH.ToString("0");
                                        UnitToInsert = "MPH";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        ValueToInsert = Math.Round(CruiseSetSpeedKMH).ToString("0");
                                        UnitToInsert = "KM/H";
                                    }
                                    break;
                                case 0x1E:
                                    DescriptionToInsert = "KEY-ON CYCLES ERROR 2";
                                    ValueToInsert = payload[1].ToString("0");
                                    break;
                                case 0x1F:
                                    DescriptionToInsert = "KEY-ON CYCLES ERROR 3";
                                    ValueToInsert = payload[1].ToString("0");
                                    break;
                                case 0x20:
                                    string LastCruiseCutoutReasonA;
                                    string CruiseDeniedReasonA;

                                    switch (payload[1] & 0xF0) // upper 4 bits encode last cutout reason 
                                    {
                                        case 0x00:
                                            LastCruiseCutoutReasonA = "ON/OFF SW";
                                            break;
                                        case 0x10:
                                            LastCruiseCutoutReasonA = "SPEED SEN";
                                            break;
                                        case 0x20:
                                            LastCruiseCutoutReasonA = "RPM LIMIT";
                                            break;
                                        case 0x30:
                                            LastCruiseCutoutReasonA = "BRAKE SW";
                                            break;
                                        case 0x40:
                                            LastCruiseCutoutReasonA = "P/N SW";
                                            break;
                                        case 0x50:
                                            LastCruiseCutoutReasonA = "RPM/SPEED";
                                            break;
                                        case 0x60:
                                            LastCruiseCutoutReasonA = "CLUTCH";
                                            break;
                                        case 0x70:
                                            LastCruiseCutoutReasonA = "S/C DTC";
                                            break;
                                        case 0x80:
                                            LastCruiseCutoutReasonA = "KEY OFF";
                                            break;
                                        case 0x90:
                                            LastCruiseCutoutReasonA = "ACTIVE";
                                            break;
                                        case 0xA0:
                                            LastCruiseCutoutReasonA = "CLUTCH UP";
                                            break;
                                        case 0xB0:
                                            LastCruiseCutoutReasonA = "N/A";
                                            break;
                                        case 0xC0:
                                            LastCruiseCutoutReasonA = "SW DTC";
                                            break;
                                        case 0xD0:
                                            LastCruiseCutoutReasonA = "CANCEL SW";
                                            break;
                                        case 0xE0:
                                            LastCruiseCutoutReasonA = "LIMP-IN";
                                            break;
                                        case 0xF0:
                                            LastCruiseCutoutReasonA = "12V DTC";
                                            break;
                                        default:
                                            LastCruiseCutoutReasonA = "N/A";
                                            break;
                                    }

                                    switch (payload[1] & 0x0F) // lower 4 bits encode denied reason 
                                    {
                                        case 0x00:
                                            CruiseDeniedReasonA = "ON/OFF SW";
                                            break;
                                        case 0x01:
                                            CruiseDeniedReasonA = "SPEED SEN";
                                            break;
                                        case 0x02:
                                            CruiseDeniedReasonA = "RPM LIMIT";
                                            break;
                                        case 0x03:
                                            CruiseDeniedReasonA = "BRAKE SW";
                                            break;
                                        case 0x04:
                                            CruiseDeniedReasonA = "P/N SW";
                                            break;
                                        case 0x05:
                                            CruiseDeniedReasonA = "RPM/SPEED";
                                            break;
                                        case 0x06:
                                            CruiseDeniedReasonA = "CLUTCH";
                                            break;
                                        case 0x07:
                                            CruiseDeniedReasonA = "S/C DTC";
                                            break;
                                        case 0x08:
                                            CruiseDeniedReasonA = "ALLOWED";
                                            break;
                                        case 0x09:
                                            CruiseDeniedReasonA = "ACTIVE";
                                            break;
                                        case 0x0A:
                                            CruiseDeniedReasonA = "CLUTCH UP";
                                            break;
                                        case 0x0B:
                                            CruiseDeniedReasonA = "N/A";
                                            break;
                                        case 0x0C:
                                            CruiseDeniedReasonA = "SW DTC";
                                            break;
                                        case 0x0D:
                                            CruiseDeniedReasonA = "CANCEL SW";
                                            break;
                                        case 0x0E:
                                            CruiseDeniedReasonA = "LIMP-IN";
                                            break;
                                        case 0x0F:
                                            CruiseDeniedReasonA = "12V DTC";
                                            break;
                                        default:
                                            CruiseDeniedReasonA = "N/A";
                                            break;
                                    }

                                    if ((payload[1] & 0x0F) == 0x08)
                                    {
                                        DescriptionToInsert = "CRUISE | LAST CUTOUT: " + LastCruiseCutoutReasonA + " | STATE: " + CruiseDeniedReasonA;
                                        ValueToInsert = "STOPPED";
                                    }
                                    else if ((payload[1] & 0x0F) == 0x09)
                                    {
                                        DescriptionToInsert = "CRUISE | LAST CUTOUT: " + LastCruiseCutoutReasonA + " | STATE: " + CruiseDeniedReasonA;
                                        ValueToInsert = "ENGAGED";
                                    }
                                    else
                                    {
                                        DescriptionToInsert = "CRUISE | LAST CUTOUT: " + LastCruiseCutoutReasonA + " | DENIED: " + CruiseDeniedReasonA;
                                        ValueToInsert = "STOPPED";
                                    }
                                    break;
                                case 0x24:
                                    double BatteryChargingVoltage = payload[1] * 0.0625;
                                    DescriptionToInsert = "BATTERY CHARGING VOLTAGE";
                                    ValueToInsert = Math.Round(BatteryChargingVoltage, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0x25:
                                    DescriptionToInsert = "OVER 5 PSI BOOST TIMER";
                                    ValueToInsert = payload[1].ToString("0");
                                    break;
                                case 0x28:
                                    double WDCPercent = payload[1] * 0.5;
                                    DescriptionToInsert = "WASTEGATE DUTY CYCLE";
                                    ValueToInsert = Math.Round(WDCPercent, 1).ToString("0.0").Replace(",", ".");
                                    UnitToInsert = "PERCENT";
                                    break;
                                case 0x27:
                                    DescriptionToInsert = "VEHICLE THEFT ALARM STATUS";
                                    ValueToInsert = Convert.ToString(payload[1], 2).PadLeft(8, '0');
                                    break;
                                case 0x29:
                                    DescriptionToInsert = "READ FUEL SETTING";
                                    ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                                    break;
                                case 0x2A:
                                    DescriptionToInsert = "READ SET SYNC";
                                    ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                                    break;
                                case 0x2C:
                                    double CSWVoltage = payload[1] * 0.0196;
                                    DescriptionToInsert = "CRUISE SWITCH VOLTAGE SENSE";
                                    ValueToInsert = Math.Round(CSWVoltage, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0x2D:
                                    double AmbientTemperatureC = payload[1] - 128;
                                    double AmbientTemperatureF = 1.8 * AmbientTemperatureC + 32.0;
                                    DescriptionToInsert = "AMBIENT/BATTERY TEMPERATURE";

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        ValueToInsert = Math.Round(AmbientTemperatureF).ToString("0");
                                        UnitToInsert = "°F";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        ValueToInsert = AmbientTemperatureC.ToString("0");
                                        UnitToInsert = "°C";
                                    }
                                    break;
                                case 0x2F:
                                    double O2S21Voltage = payload[1] * 0.0196;
                                    DescriptionToInsert = "UPSTREAM O2 2/1 SENSOR VOLTAGE";
                                    ValueToInsert = Math.Round(O2S21Voltage, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0x30:
                                    double KnockSensor2Voltage = payload[1] * 0.0196;
                                    DescriptionToInsert = "KNOCK SENSOR 2 VOLTAGE";
                                    ValueToInsert = Math.Round(KnockSensor2Voltage, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0x31:
                                    double LTFT2 = payload[1] * 0.196;
                                    DescriptionToInsert = "LONG TERM FUEL TRIM 2";
                                    ValueToInsert = Math.Round(LTFT2, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "PERCENT";
                                    break;
                                case 0x32:
                                    double ACHSPSVoltage = payload[1] * 0.0196;
                                    DescriptionToInsert = "A/C HIGH-SIDE PRESSURE SENSOR VOLTAGE";
                                    ValueToInsert = Math.Round(ACHSPSVoltage, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0x33:
                                    double ACHSPressurePSI = payload[1] * 1.961;
                                    double ACHSPressureKPA = payload[1] * 1.961 * 6.894757;
                                    DescriptionToInsert = "A/C HIGH-SIDE PRESSURE";

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        ValueToInsert = Math.Round(ACHSPressurePSI, 1).ToString("0.0").Replace(",", ".");
                                        UnitToInsert = "PSI";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        ValueToInsert = Math.Round(ACHSPressureKPA, 1).ToString("0.0").Replace(",", ".");
                                        UnitToInsert = "KPA";
                                    }
                                    break;
                                case 0x34:
                                    double FlexFuelSensorVoltage = payload[1] * 0.0196;
                                    DescriptionToInsert = "FLEX FUEL SENSOR VOLTAGE";
                                    ValueToInsert = Math.Round(FlexFuelSensorVoltage, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0x35:
                                    DescriptionToInsert = "FLEX FUEL INFO 1";
                                    ValueToInsert = payload[1].ToString("0");
                                    break;
                                case 0x3B:
                                    DescriptionToInsert = "FUEL SYSTEM STATUS 1";

                                    if (Util.IsBitSet(payload[1], 0)) ValueToInsert = "OPEN LOOP";
                                    if (Util.IsBitSet(payload[1], 1)) ValueToInsert = "CLOSED LOOP";
                                    if (Util.IsBitSet(payload[1], 2)) ValueToInsert = "OPEN LOOP / DRIVE";
                                    if (Util.IsBitSet(payload[1], 3)) ValueToInsert = "OPEN LOOP / DTC";
                                    if (Util.IsBitSet(payload[1], 4)) ValueToInsert = "CLOSED LOOP / DTC";

                                    break;
                                case 0x3E:
                                    double CalPotVoltage = payload[1] * 0.0196;
                                    DescriptionToInsert = "CALPOT VOLTAGE";
                                    ValueToInsert = Math.Round(CalPotVoltage,31).ToString("0.000").Replace(",", ".");
                                    break;
                                case 0x3F:
                                    double O2S12Voltage = payload[1] * 0.0196;
                                    DescriptionToInsert = "DOWNSTREAM O2 1/2 SENSOR VOLTAGE";
                                    ValueToInsert = Math.Round(O2S12Voltage, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0x40:
                                    double MAPSensorVoltage = payload[1] * 0.0196;
                                    DescriptionToInsert = "MAP SENSOR VOLTAGE";
                                    ValueToInsert = Math.Round(MAPSensorVoltage, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0x41:
                                    byte VehicleSpeedMPH = payload[1];
                                    double VehicleSpeedKMH = VehicleSpeedMPH * 1.609344;
                                    DescriptionToInsert = "VEHICLE SPEED";

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        ValueToInsert = VehicleSpeedMPH.ToString("0");
                                        UnitToInsert = "MPH";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        ValueToInsert = Math.Round(VehicleSpeedKMH).ToString("0");
                                        UnitToInsert = "KM/H";
                                    }
                                    break;
                                case 0x42:
                                    DescriptionToInsert = "UPSTREAM O2 1/1 SENSOR LEVEL";

                                    switch (payload[1])
                                    {
                                        case 0xA0:
                                            ValueToInsert = "LEAN";
                                            break;
                                        case 0xB1:
                                            ValueToInsert = "RICH";
                                            break;
                                        case 0xFF:
                                            ValueToInsert = "CENTER";
                                            break;
                                        default:
                                            ValueToInsert = "N/A";
                                            break;
                                    }
                                    break;
                                case 0x45:
                                    double MAPVacuumPSI = payload[1] * 0.059756;
                                    double MAPVacuumKPA = MAPVacuumPSI * 6.894757;
                                    DescriptionToInsert = "MAP VACUUM";

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        ValueToInsert = Math.Round(MAPVacuumPSI, 1).ToString("0.0").Replace(",", ".");
                                        UnitToInsert = "PSI";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        ValueToInsert = Math.Round(MAPVacuumKPA, 1).ToString("0.0").Replace(",", ".");
                                        UnitToInsert = "KPA";
                                    }
                                    break;
                                case 0x46:
                                    double RelativeThrottlePosition = payload[1] * 0.0196;
                                    DescriptionToInsert = "RELATIVE THROTTLE POSITION";
                                    ValueToInsert = Math.Round(RelativeThrottlePosition, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0x47:
                                    double SparkAdvance2 = payload[1] * 0.5;
                                    DescriptionToInsert = "SPARK ADVANCE";
                                    ValueToInsert = Math.Round(SparkAdvance2, 1).ToString("0.0").Replace(",", ".");
                                    UnitToInsert = "DEG";
                                    break;
                                case 0x48:
                                    DescriptionToInsert = "UPSTREAM O2 2/1 SENSOR LEVEL";

                                    switch (payload[1])
                                    {
                                        case 0xA0:
                                            ValueToInsert = "LEAN";
                                            break;
                                        case 0xB1:
                                            ValueToInsert = "RICH";
                                            break;
                                        case 0xFF:
                                            ValueToInsert = "CENTER";
                                            break;
                                        default:
                                            ValueToInsert = "N/A";
                                            break;
                                    }
                                    break;
                                case 0x49:
                                    double O2S22Voltage = payload[1] * 0.0196;
                                    DescriptionToInsert = "DOWNSTREAM O2 2/2 SENSOR VOLTAGE";
                                    ValueToInsert = Math.Round(O2S22Voltage, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0x4A:
                                    DescriptionToInsert = "DOWNSTREAM O2 1/2 SENSOR LEVEL";

                                    switch (payload[1])
                                    {
                                        case 0xA0:
                                            ValueToInsert = "LEAN";
                                            break;
                                        case 0xB1:
                                            ValueToInsert = "RICH";
                                            break;
                                        case 0xFF:
                                            ValueToInsert = "CENTER";
                                            break;
                                        default:
                                            ValueToInsert = "N/A";
                                            break;
                                    }
                                    break;
                                case 0x4B:
                                    DescriptionToInsert = "DOWNSTREAM O2 2/2 SENSOR LEVEL";

                                    switch (payload[1])
                                    {
                                        case 0xA0:
                                            ValueToInsert = "LEAN";
                                            break;
                                        case 0xB1:
                                            ValueToInsert = "RICH";
                                            break;
                                        case 0xFF:
                                            ValueToInsert = "CENTER";
                                            break;
                                        default:
                                            ValueToInsert = "N/A";
                                            break;
                                    }
                                    break;
                                case 0x4E:
                                    double FuelLevelSensorVoltage = payload[1] * 0.0196;
                                    DescriptionToInsert = "FUEL LEVEL SENSOR VOLTAGE";
                                    ValueToInsert = Math.Round(FuelLevelSensorVoltage, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0x4F:
                                    double FuelLevelG = payload[1] * 0.125;
                                    double FuelLevelL = FuelLevelG * 3.785412;
                                    DescriptionToInsert = "FUEL LEVEL";

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        ValueToInsert = Math.Round(FuelLevelG, 1).ToString("0.0").Replace(",", ".");
                                        UnitToInsert = "GALLON";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        ValueToInsert = Math.Round(FuelLevelL, 1).ToString("0.0").Replace(",", ".");
                                        UnitToInsert = "LITER";
                                    }
                                    break;
                                case 0x57:
                                    DescriptionToInsert = "FUEL SYSTEM STATUS 2";

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
                                            ValueToInsert += s + " | ";
                                        }

                                        if (ValueToInsert.Length > 2) ValueToInsert = ValueToInsert.Remove(ValueToInsert.Length - 3); // remove last "|" character
                                    }
                                    else
                                    {
                                        ValueToInsert = "N/A";
                                    }
                                    break;
                                case 0x58:
                                    string LastCruiseCutoutReasonB;
                                    string CruiseDeniedReasonB;

                                    switch (payload[1] & 0xF0) // upper 4 bits encode last cutout reason 
                                    {
                                        case 0x00:
                                            LastCruiseCutoutReasonB = "ON/OFF SW";
                                            break;
                                        case 0x10:
                                            LastCruiseCutoutReasonB = "SPEED SEN";
                                            break;
                                        case 0x20:
                                            LastCruiseCutoutReasonB = "RPM LIMIT";
                                            break;
                                        case 0x30:
                                            LastCruiseCutoutReasonB = "BRAKE SW";
                                            break;
                                        case 0x40:
                                            LastCruiseCutoutReasonB = "P/N SW";
                                            break;
                                        case 0x50:
                                            LastCruiseCutoutReasonB = "RPM/SPEED";
                                            break;
                                        case 0x60:
                                            LastCruiseCutoutReasonB = "CLUTCH";
                                            break;
                                        case 0x70:
                                            LastCruiseCutoutReasonB = "S/C DTC";
                                            break;
                                        case 0x80:
                                            LastCruiseCutoutReasonB = "KEY OFF";
                                            break;
                                        case 0x90:
                                            LastCruiseCutoutReasonB = "ACTIVE";
                                            break;
                                        case 0xA0:
                                            LastCruiseCutoutReasonB = "CLUTCH UP";
                                            break;
                                        case 0xB0:
                                            LastCruiseCutoutReasonB = "N/A";
                                            break;
                                        case 0xC0:
                                            LastCruiseCutoutReasonB = "SW DTC";
                                            break;
                                        case 0xD0:
                                            LastCruiseCutoutReasonB = "CANCEL";
                                            break;
                                        case 0xE0:
                                            LastCruiseCutoutReasonB = "LIMP-IN";
                                            break;
                                        case 0xF0:
                                            LastCruiseCutoutReasonB = "12V DTC";
                                            break;
                                        default:
                                            LastCruiseCutoutReasonB = "N/A";
                                            break;
                                    }

                                    switch (payload[1] & 0x0F) // lower 4 bits encode denied reason 
                                    {
                                        case 0x00:
                                            CruiseDeniedReasonB = "ON/OFF SW";
                                            break;
                                        case 0x01:
                                            CruiseDeniedReasonB = "SPEED SEN";
                                            break;
                                        case 0x02:
                                            CruiseDeniedReasonB = "RPM LIMIT";
                                            break;
                                        case 0x03:
                                            CruiseDeniedReasonB = "BRAKE SW";
                                            break;
                                        case 0x04:
                                            CruiseDeniedReasonB = "P/N SW";
                                            break;
                                        case 0x05:
                                            CruiseDeniedReasonB = "RPM/SPEED";
                                            break;
                                        case 0x06:
                                            CruiseDeniedReasonB = "CLUTCH";
                                            break;
                                        case 0x07:
                                            CruiseDeniedReasonB = "S/C DTC";
                                            break;
                                        case 0x08:
                                            CruiseDeniedReasonB = "ALLOWED";
                                            break;
                                        case 0x09:
                                            CruiseDeniedReasonB = "ACTIVE";
                                            break;
                                        case 0x0A:
                                            CruiseDeniedReasonB = "CLUTCH UP";
                                            break;
                                        case 0x0B:
                                            CruiseDeniedReasonB = "N/A";
                                            break;
                                        case 0x0C:
                                            CruiseDeniedReasonB = "SW DTC";
                                            break;
                                        case 0x0D:
                                            CruiseDeniedReasonB = "CANCEL";
                                            break;
                                        case 0x0E:
                                            CruiseDeniedReasonB = "LIMP-IN";
                                            break;
                                        case 0x0F:
                                            CruiseDeniedReasonB = "12V DTC";
                                            break;
                                        default:
                                            CruiseDeniedReasonB = "N/A";
                                            break;
                                    }

                                    if ((payload[1] & 0x0F) == 0x08)
                                    {
                                        DescriptionToInsert = "CRUISE | LAST CUTOUT: " + LastCruiseCutoutReasonB + " | STATE: " + CruiseDeniedReasonB;
                                        ValueToInsert = "STOPPED";
                                    }
                                    else if ((payload[1] & 0x0F) == 0x09)
                                    {
                                        DescriptionToInsert = "CRUISE | LAST CUTOUT: " + LastCruiseCutoutReasonB + " | STATE: " + CruiseDeniedReasonB;
                                        ValueToInsert = "ENGAGED";
                                    }
                                    else
                                    {
                                        DescriptionToInsert = "CRUISE | LAST CUTOUT: " + LastCruiseCutoutReasonB + " | DENIED: " + CruiseDeniedReasonB;
                                        ValueToInsert = "STOPPED";
                                    }
                                    break;
                                case 0x59:
                                    DescriptionToInsert = "CRUISE CONTROL OPERATING MODE";

                                    switch (payload[1] & 0x0F)
                                    {
                                        case 0x08:
                                            ValueToInsert = "DISENGAGED";
                                            break;
                                        case 0x09:
                                            ValueToInsert = "NORMAL";
                                            break;
                                        case 0x0A:
                                            ValueToInsert = "ACCELERATING";
                                            break;
                                        case 0x0B:
                                            ValueToInsert = "DECELERATING";
                                            break;
                                        default:
                                            ValueToInsert = "N/A";
                                            break;
                                    }
                                    break;
                                case 0x5C:
                                    double EngineLoad = payload[1] * 0.3922;
                                    DescriptionToInsert = "CALCULATED ENGINE LOAD";
                                    ValueToInsert = Math.Round(EngineLoad, 1).ToString("0.0").Replace(",", ".");
                                    UnitToInsert = "PERCENT";
                                    break;
                                case 0x5A:
                                    double OutputShaftSpeed = payload[1] * 20.0;
                                    DescriptionToInsert = "OUTPUT SHAFT SPEED";
                                    ValueToInsert = OutputShaftSpeed.ToString("0");
                                    UnitToInsert = "RPM";
                                    break;
                                case 0x5B:
                                    double GovPDC = payload[1] * 0.3922;
                                    DescriptionToInsert = "GOVERNOR PRESSURE DUTY CYCLE";
                                    ValueToInsert = Math.Round(GovPDC, 1).ToString("0.0").Replace(",", ".");
                                    UnitToInsert = "PERCENT";
                                    break;
                                case 0x5F:
                                    double EGRSensorVoltage = payload[1] * 0.0196;
                                    DescriptionToInsert = "EGR POSITION SENSOR VOLTAGE";
                                    ValueToInsert = Math.Round(EGRSensorVoltage, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0x60:
                                    DescriptionToInsert = "EGR ZREF UPDATE D.C.";
                                    ValueToInsert = payload[1].ToString("0");
                                    break;
                                case 0x64:
                                    double ActualPurgeCurrent = payload[1] * 0.0196;
                                    DescriptionToInsert = "ACTUAL PURGE CURRENT";
                                    ValueToInsert = Math.Round(ActualPurgeCurrent, 1).ToString("0.0").Replace(",", ".");
                                    UnitToInsert = "A";
                                    break;
                                case 0x65:
                                    double CTSVoltage = payload[1] * 0.0196;
                                    DescriptionToInsert = "CATALYST TEMPERATURE SENSOR VOLTAGE";
                                    ValueToInsert = Math.Round(CTSVoltage, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0x66:
                                    double CatalystTemperatureC = payload[1] - 128;
                                    double CatalystTemperatureF = 1.8 * CatalystTemperatureC + 32.0;
                                    DescriptionToInsert = "CATALYST TEMPERATURE";

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        ValueToInsert = Math.Round(CatalystTemperatureF).ToString("0");
                                        UnitToInsert = "°F";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        ValueToInsert = CatalystTemperatureC.ToString("0");
                                        UnitToInsert = "°C";
                                    }
                                    break;
                                case 0x69:
                                    double ATSVoltage = payload[1] * 0.0196;
                                    DescriptionToInsert = "AMBIENT TEMPERATURE SENSOR VOLTAGE";
                                    ValueToInsert = Math.Round(ATSVoltage, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0x6D:
                                    double TCSwVoltage = payload[1] * 0.0196;
                                    DescriptionToInsert = "T-CASE SWITCH VOLTAGE";
                                    ValueToInsert = Math.Round(TCSwVoltage, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0x7A:
                                    double FCACurrent = payload[1] * 0.0196;
                                    DescriptionToInsert = "FCA CURRENT";
                                    ValueToInsert = Math.Round(FCACurrent, 1).ToString("0.0").Replace(",", ".");
                                    UnitToInsert = "A";
                                    break;
                                case 0x7C:
                                    double OTSVoltage = payload[1] * 0.0196;
                                    DescriptionToInsert = "OIL TEMPERATURE SENSOR VOLTAGE";
                                    ValueToInsert = Math.Round(OTSVoltage, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0x7D:
                                    double OilTemperatureC = payload[1] - 64;
                                    double OilTemperatureF = 1.8 * OilTemperatureC + 32.0;
                                    DescriptionToInsert = "OIL TEMPERATURE";

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        ValueToInsert = Math.Round(OilTemperatureF).ToString("0");
                                        UnitToInsert = "°F";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        ValueToInsert = OilTemperatureC.ToString("0");
                                        UnitToInsert = "°C";
                                    }
                                    break;
                                default:
                                    DescriptionToInsert = "REQUEST DIAGNOSTIC DATA | OFFSET: " + Util.ByteToHexString(payload, 0, 1);
                                    ValueToInsert = Util.ByteToHexString(payload, 1, payload.Length - 1);
                                    break;
                            }
                        }
                        break;
                    case 0x15:
                        DescriptionToInsert = "READ FLASH MEMORY";

                        if (message.Length >= 4)
                        {
                            DescriptionToInsert = "READ FLASH MEMORY | OFFSET: " + Util.ByteToHexString(payload, 0, 2);
                            ValueToInsert = Util.ByteToHexString(payload, 2, 1);
                        }
                        break;
                    case 0x16:
                        DescriptionToInsert = "READ FLASH MEMORY CONSTANT";

                        if (message.Length >= 3)
                        {
                            ushort offset = (ushort)(payload[0] + 0x8000);
                            byte[] offsetArray = new byte[2];
                            offsetArray[0] = (byte)((offset >> 8) & 0xFF);
                            offsetArray[1] = (byte)(offset & 0xFF);

                            DescriptionToInsert = "READ FLASH MEMORY CONSTANT | OFFSET: " + Util.ByteToHexStringSimple(offsetArray);
                            ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                        }
                        break;
                    case 0x17:
                        DescriptionToInsert = "ERASE ENGINE FAULT CODES";

                        if (message.Length >= 2)
                        {
                            if (payload[0] == 0xE0) ValueToInsert = "ERASED";
                            else ValueToInsert = "FAILED";
                        }
                        break;
                    case 0x18:
                        DescriptionToInsert = "CONTROL ASD RELAY";

                        if (message.Length >= 3)
                        {
                            ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                        }
                        break;
                    case 0x19:
                        DescriptionToInsert = "SET ENGINE IDLE SPEED";

                        if (message.Length >= 2)
                        {
                            ValueToInsert = Math.Round(payload[0] * 7.85).ToString("0");

                            if (payload[0] < 0x42)
                            {
                                ValueToInsert += " - TOO LOW!";
                            }

                            UnitToInsert = "RPM";
                        }
                        break;
                    case 0x1A:
                        DescriptionToInsert = "SWITCH TEST";

                        if (message.Length >= 3)
                        {
                            List<string> SwitchList = new List<string>();

                            switch (payload[0])
                            {
                                case 0x01:
                                    SwitchList.Clear();

                                    if (Util.IsBitSet(payload[1], 0)) SwitchList.Add("WAIT TO START LAMP");
                                    if (Util.IsBitSet(payload[1], 1)) SwitchList.Add("INTAKE HEATER #1");
                                    if (Util.IsBitSet(payload[1], 2)) SwitchList.Add("INTAKE HEATER #2");
                                    if (Util.IsBitSet(payload[1], 3)) SwitchList.Add("IDLE VALIDATION SW1");
                                    if (Util.IsBitSet(payload[1], 4)) SwitchList.Add("IDLE VALIDATION SW2");
                                    if (Util.IsBitSet(payload[1], 5)) SwitchList.Add("IDLE SELECT");
                                    if (Util.IsBitSet(payload[1], 6)) SwitchList.Add("TRANSFER PMPDR");

                                    if (SwitchList.Count > 0)
                                    {
                                        DescriptionToInsert = "SWITCH TEST | ";

                                        foreach (string s in SwitchList)
                                        {
                                            DescriptionToInsert += s + " | ";
                                        }

                                        if (DescriptionToInsert.Length > 2) DescriptionToInsert = DescriptionToInsert.Remove(DescriptionToInsert.Length - 3); // remove last "|" character
                                    }
                                    else
                                    {
                                        DescriptionToInsert = "SWITCH TEST";
                                    }
                                    break;
                                case 0x02:
                                    SwitchList.Clear();
                                    if (Util.IsBitSet(payload[1], 0)) SwitchList.Add("INJ PUMP");
                                    if (Util.IsBitSet(payload[1], 2)) SwitchList.Add("A/C CLUTCH");
                                    if (Util.IsBitSet(payload[1], 3)) SwitchList.Add("EXHAUST BRAKE");
                                    if (Util.IsBitSet(payload[1], 4)) SwitchList.Add("BRAKE");
                                    if (Util.IsBitSet(payload[1], 5)) SwitchList.Add("EVAP PURGE");
                                    if (Util.IsBitSet(payload[1], 7)) SwitchList.Add("LOW OIL");

                                    if (SwitchList.Count > 0)
                                    {
                                        DescriptionToInsert = "SWITCH TEST | ";

                                        foreach (string s in SwitchList)
                                        {
                                            DescriptionToInsert += s + " | ";
                                        }

                                        DescriptionToInsert = DescriptionToInsert.Remove(DescriptionToInsert.Length - 3); // remove last "|" character
                                    }
                                    else
                                    {
                                        DescriptionToInsert = "SWITCH TEST";
                                    }
                                    break;
                                case 0x03:
                                    SwitchList.Clear();
                                    if (Util.IsBitSet(payload[1], 1)) SwitchList.Add("MIL");
                                    if (Util.IsBitSet(payload[1], 2)) SwitchList.Add("GENERATOR LAMP");
                                    if (Util.IsBitSet(payload[1], 3)) SwitchList.Add("GENERATOR FIELD");
                                    if (Util.IsBitSet(payload[1], 4)) SwitchList.Add("12V FEED");
                                    if (Util.IsBitSet(payload[1], 6)) SwitchList.Add("TRANS O/D");
                                    if (Util.IsBitSet(payload[1], 7)) SwitchList.Add("TRANS TOW MODE");

                                    if (SwitchList.Count > 0)
                                    {
                                        DescriptionToInsert = "SWITCH TEST | ";

                                        foreach (string s in SwitchList)
                                        {
                                            DescriptionToInsert += s + " | ";
                                        }

                                        DescriptionToInsert = DescriptionToInsert.Remove(DescriptionToInsert.Length - 3); // remove last "|" character
                                    }
                                    else
                                    {
                                        DescriptionToInsert = "SWITCH TEST";
                                    }
                                    break;
                                case 0x04:
                                    SwitchList.Clear();
                                    if (Util.IsBitSet(payload[1], 1)) SwitchList.Add("TRD LINK");
                                    if (Util.IsBitSet(payload[1], 4)) SwitchList.Add("ASD");
                                    if (Util.IsBitSet(payload[1], 6)) SwitchList.Add("IGNITION");

                                    if (SwitchList.Count > 0)
                                    {
                                        DescriptionToInsert = "SWITCH TEST | ";

                                        foreach (string s in SwitchList)
                                        {
                                            DescriptionToInsert += s + " | ";
                                        }

                                        DescriptionToInsert = DescriptionToInsert.Remove(DescriptionToInsert.Length - 3); // remove last "|" character
                                    }
                                    else
                                    {
                                        DescriptionToInsert = "SWITCH TEST";
                                    }
                                    break;
                                default:
                                    DescriptionToInsert = "SWITCH TEST | OFFSET: " + Util.ByteToHexString(payload, 0, 1);
                                    break;
                            }
                        }
                        break;
                    case 0x1B:
                        DescriptionToInsert = "INIT BYTE MODE DOWNLOAD";
                        break;
                    case 0x1C:
                        DescriptionToInsert = "WRITE MEMORY";

                        if (message.Length >= 4)
                        {
                            switch (payload[0])
                            {
                                case 0x10:
                                    DescriptionToInsert = "WRITE MEMROY | RESET EMR 1";
                                    if ((payload[1] == 0x00) && (payload[2] == 0xFF)) ValueToInsert = "OK";
                                    else ValueToInsert = "FAILED";
                                    break;
                                case 0x11:
                                    DescriptionToInsert = "WRITE MEMROY | RESET EMR 2";
                                    if ((payload[1] == 0x00) && (payload[2] == 0xFF)) ValueToInsert = "OK";
                                    else ValueToInsert = "FAILED";
                                    break;
                                case 0x1A:
                                    if (payload[1] == 0xFF)
                                    {
                                        DescriptionToInsert = "WRITE MEMROY | ENABLE VAR IDLE";

                                        if (payload[2] == 0xFF)
                                        {
                                            ValueToInsert = "OK";
                                        }
                                        else
                                        {
                                            ValueToInsert = "FAILED";
                                        }
                                    }
                                    else if (payload[1] == 0x00)
                                    {
                                        DescriptionToInsert = "WRITE MEMROY | DISABLE VAR IDLE";

                                        if (payload[2] == 0xFF)
                                        {
                                            ValueToInsert = "OK";
                                        }
                                        else
                                        {
                                            ValueToInsert = "FAILED";
                                        }
                                    }
                                    break;
                                default:
                                    DescriptionToInsert = "WRITE MEMROY | OFFSET: " + Util.ByteToHexString(payload, 0, 1);
                                    ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                                    break;
                            }
                        }
                        break;
                    case 0x1F:
                        DescriptionToInsert = "WRITE RAM WORKER";

                        if (message.Length >= 4)
                        {
                            DescriptionToInsert = "WRITE RAM | OFFSET: 07 " + Util.ByteToHexString(payload, 0, 1);

                            switch (payload[2])
                            {
                                case 0xE5:
                                    ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                                    UnitToInsert = "OK";
                                    break;
                                case 0x00:
                                    ValueToInsert = "DENIED (INVALID OFFSET)";
                                    break;
                                case 0xF1:
                                    ValueToInsert = "DENIED (SECURITY LEVEL)";
                                    break;
                                default:
                                    ValueToInsert = Util.ByteToHexString(payload, 2, 1);
                                    break;
                            }
                        }
                        break;
                    case 0x20:
                        DescriptionToInsert = "RUN RAM WORKER";

                        if (message.Length >= 4)
                        {
                            switch (payload[2])
                            {
                                case 0xE5:
                                    ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                                    UnitToInsert = "OK";
                                    break;
                                case 0x00:
                                    ValueToInsert = "DENIED (NO RTS FOUND)";
                                    break;
                                case 0x01:
                                    ValueToInsert = "DENIED (RTS OFFSET)";
                                    break;
                                case 0x02:
                                    ValueToInsert = "DENIED (INVALID OFFSET)";
                                    break;
                                case 0xF1:
                                    ValueToInsert = "DENIED (SECURITY LEVEL)";
                                    break;
                                default:
                                    ValueToInsert = Util.ByteToHexString(payload, 2, 1);
                                    break;
                            }
                        }
                        break;
                    case 0x21:
                        DescriptionToInsert = "SET ENGINE PARAMETER";

                        if (message.Length >= 3)
                        {
                            switch (payload[0])
                            {
                                case 0x00:
                                    DescriptionToInsert = "UNKILL SPARK SCATTER";

                                    switch (payload[1])
                                    {
                                        case 0x00:
                                            ValueToInsert = "TIMING ABOLISHED";
                                            break;
                                        case 0x01:
                                            ValueToInsert = "TIMING INITIATED";
                                            break;
                                        case 0x02:
                                            ValueToInsert = "REJECTED (OPEN THROTTLE)";
                                            break;
                                        case 0x03:
                                            ValueToInsert = "REJECTED (SLP)";
                                            break;
                                        case 0x10:
                                            ValueToInsert = "SYNC INITIATED";
                                            break;
                                        default:
                                            ValueToInsert = "UNDEFINED";
                                            break;
                                    }

                                    break;
                                case 0x01:
                                    DescriptionToInsert = "KILL SPARK SCATTER";

                                    switch (payload[1])
                                    {
                                        case 0x00:
                                            ValueToInsert = "TIMING ABOLISHED";
                                            break;
                                        case 0x01:
                                            ValueToInsert = "TIMING INITIATED";
                                            break;
                                        case 0x02:
                                            ValueToInsert = "REJECTED (OPEN THROTTLE)";
                                            break;
                                        case 0x03:
                                            ValueToInsert = "REJECTED (SLP)";
                                            break;
                                        case 0x10:
                                            ValueToInsert = "SYNC INITIATED";
                                            break;
                                        default:
                                            ValueToInsert = "UNDEFINED";
                                            break;
                                    }

                                    break;
                                case 0x10:
                                    DescriptionToInsert = "SET SYNC MODE";

                                    switch (payload[1])
                                    {
                                        case 0x00:
                                            ValueToInsert = "TIMING ABOLISHED";
                                            break;
                                        case 0x01:
                                            ValueToInsert = "TIMING INITIATED";
                                            break;
                                        case 0x02:
                                            ValueToInsert = "REJECTED (OPEN THROTTLE)";
                                            break;
                                        case 0x03:
                                            ValueToInsert = "REJECTED (SLP)";
                                            break;
                                        case 0x10:
                                            ValueToInsert = "SYNC INITIATED";
                                            break;
                                        default:
                                            ValueToInsert = "UNDEFINED";
                                            break;
                                    }

                                    break;
                                default:
                                    DescriptionToInsert = "SET ENGINE PARAMETER | OFFSET: " + Util.ByteToHexString(payload, 0, 1);
                                    ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                                    break;
                            }
                        }
                        break;
                    case 0x22:
                        DescriptionToInsert = "SEND ENGINE PARAMETERS";

                        if (message.Length >= 4)
                        {
                            switch (payload[0])
                            {
                                case 0x01:
                                    double EngineSpeed2 = ((payload[1] << 8) + payload[2]) * 0.125;
                                    DescriptionToInsert = "ENGINE SPEED";
                                    ValueToInsert = Math.Round(EngineSpeed2, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "RPM";
                                    break;
                                case 0x02:
                                    double InjectorPulseWidth1 = ((payload[1] << 8) + payload[2]) * 0.00390625;
                                    DescriptionToInsert = "INJECTOR PULSE WIDTH 1";
                                    ValueToInsert = Math.Round(InjectorPulseWidth1, 3).ToString("0.000");
                                    UnitToInsert = "MS"; // milliseconds
                                    break;
                                case 0x03:
                                    double TargetIdleSpeed = ((payload[1] << 8) + payload[2]) * 0.125;
                                    DescriptionToInsert = "TARGET IDLE SPEED";
                                    ValueToInsert = Math.Round(TargetIdleSpeed, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "RPM";
                                    break;
                                case 0x04:
                                    double InjectorPulseWidth2 = ((payload[1] << 8) + payload[2]) * 0.00390625;
                                    DescriptionToInsert = "INJECTOR PULSE WIDTH 2";
                                    ValueToInsert = Math.Round(InjectorPulseWidth2, 3).ToString("0.000");
                                    UnitToInsert = "MS"; // milliseconds
                                    break;
                                default:
                                    DescriptionToInsert = "SEND ENGINE PARAMETER | OFFSET: " + Util.ByteToHexString(payload, 0, 1);
                                    ValueToInsert = Util.ByteToHexString(payload, 1, payload.Length - 1);
                                    break;
                            }
                        }
                        break;
                    case 0x23:
                        DescriptionToInsert = "RESET MEMORY";

                        if (message.Length >= 3)
                        {
                            switch (payload[0])
                            {
                                case 0x01:
                                    DescriptionToInsert = "ERASE ENGINE FAULT CODES";
                                    break;
                                case 0x02:
                                    DescriptionToInsert = "RESET ADAPTIVE FUEL FACTOR (LTFT)";
                                    break;
                                case 0x03:
                                    DescriptionToInsert = "RESET IAC COUNTER";
                                    break;
                                case 0x04:
                                    DescriptionToInsert = "RESET MINIMUM TPS VOLTS";
                                    break;
                                case 0x05:
                                    DescriptionToInsert = "RESET FLEX FUEL PERCENT";
                                    break;
                                case 0x06:
                                    DescriptionToInsert = "RESET CAM/CRANK SYNC";
                                    break;
                                case 0x07:
                                    DescriptionToInsert = "RESET FUEL SHUTOFF";
                                    break;
                                case 0x08:
                                    DescriptionToInsert = "RESET RUNTIME AT STALL";
                                    break;
                                case 0x09:
                                    DescriptionToInsert = "DOOR LOCK ENABLE";
                                    break;
                                case 0x0A:
                                    DescriptionToInsert = "DOOR LOCK DISABLE";
                                    break;
                                case 0x0B:
                                    DescriptionToInsert = "RESET CAM/CRANK TIMING REFERENCE";
                                    break;
                                case 0x0C:
                                    DescriptionToInsert = "A/C FAULT ENABLE";
                                    break;
                                case 0x0D:
                                    DescriptionToInsert = "A/C FAULT DISABLE";
                                    break;
                                case 0x0E:
                                    DescriptionToInsert = "S/C FAULT ENABLE";
                                    break;
                                case 0x0F:
                                    DescriptionToInsert = "S/C FAULT DISABLE";
                                    break;
                                case 0x10:
                                    DescriptionToInsert = "PS FAULT ENABLE";
                                    break;
                                case 0x11:
                                    DescriptionToInsert = "PS FAULT DISABLE";
                                    break;
                                case 0x12:
                                    DescriptionToInsert = "RESET EEPROM / ADAPTIVE NUMERATOR";
                                    break;
                                case 0x13:
                                    DescriptionToInsert = "RESET SKIM F4";
                                    break;
                                case 0x14:
                                    DescriptionToInsert = "RESET DUTY CYCLE MONITOR";
                                    break;
                                case 0x15:
                                    DescriptionToInsert = "RESET TRIP/IDLE/CRUISE/INJ/O/D OFF/WATER IN FUEL";
                                    break;
                                case 0x20:
                                    DescriptionToInsert = "RESET TPS ADAPTATION FOR ETC";
                                    break;
                                case 0x21:
                                    DescriptionToInsert = "RESET MIN PEDAL VALUE";
                                    break;
                                case 0x22:
                                    DescriptionToInsert = "RESET LEARNED KNOCK CORRECTION";
                                    break;
                                case 0x23:
                                    DescriptionToInsert = "RESET LEARNED MISFIRE CORRECTION";
                                    break;
                                case 0x24:
                                    DescriptionToInsert = "RESET IDLE ADAPTATION";
                                    break;
                                default:
                                    DescriptionToInsert = "RESET MEMORY | OFFSET: " + Util.ByteToHexString(payload, 0, 1);
                                    break;
                            }

                            switch (payload[1])
                            {
                                case 0x00:
                                    ValueToInsert = "STOP ENGINE";
                                    break;
                                case 0x01:
                                    ValueToInsert = "MODE NOT SUPPORTED";
                                    break;
                                case 0x02:
                                    ValueToInsert = "DENIED (MODULE BUSY)";
                                    break;
                                case 0x03:
                                    ValueToInsert = "DENIED (SECURITY LEVEL)";
                                    break;
                                case 0xF0:
                                    ValueToInsert = "OK";
                                    break;
                                default:
                                    ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                                    break;
                            }
                        }
                        break;
                    case 0x25:
                        DescriptionToInsert = "WRITE ROM SETTING";

                        if (message.Length >= 4)
                        {
                            switch (payload[0])
                            {
                                case 0x01:
                                    DescriptionToInsert = "WRITE ROM SETTING | CALPOT LEAD";
                                    break;
                                case 0x02:
                                    DescriptionToInsert = "WRITE ROM SETTING | CALPOT MEX";
                                    break;
                                case 0x04:
                                    DescriptionToInsert = "WRITE ROM SETTING | EGR SYSTEM";
                                    break;
                                case 0x05:
                                    DescriptionToInsert = "WRITE ROM SETTING | FUEL INJECTOR #1";
                                    break;
                                case 0x0F:
                                    DescriptionToInsert = "WRITE ROM SETTING | MINIMUM AIR FLOW";
                                    break;
                                case 0x10:
                                    DescriptionToInsert = "WRITE ROM SETTING | CALPOT LHBL";
                                    break;
                                case 0x1C:
                                    DescriptionToInsert = "WRITE ROM SETTING | MISFIRE MONITOR";
                                    break;
                                case 0x21:
                                    DescriptionToInsert = "WRITE ROM SETTING | LINEAR IAC MOTOR";
                                    break;
                                case 0x25:
                                    DescriptionToInsert = "WRITE ROM SETTING | CYLINDER PERFORMANCE TEST";
                                    break;
                                case 0x26:
                                    DescriptionToInsert = "WRITE ROM SETTING | HIGH-PRESSURE SAFETY VALVE TEST";
                                    break;
                                default:
                                    DescriptionToInsert = "WRITE ROM SETTING | OFFSET: " + Util.ByteToHexString(payload, 0, 1);
                                    break;
                            }

                            switch (payload[1])
                            {
                                case 0x00:
                                    ValueToInsert = "RESET";
                                    break;
                                case 0x01:
                                    ValueToInsert = "ENABLED";
                                    break;
                                case 0x02:
                                    ValueToInsert = "DISABLED";
                                    break;
                                default:
                                    ValueToInsert = "UNKNOWN";
                                    break;
                            }

                            switch (payload[2])
                            {
                                case 0x01:
                                    UnitToInsert = "OK";
                                    break;
                                default:
                                    UnitToInsert = "DENIED (" + Util.ByteToHexString(payload, 2, 1) + ")";
                                    break;
                            }
                        }
                        break;
                    case 0x26:
                        DescriptionToInsert = "READ FLASH MEMORY";

                        if (message.Length >= 5)
                        {
                            DescriptionToInsert = "READ FLASH MEMORY | OFFSET: " + Util.ByteToHexString(payload, 0, 3);
                            ValueToInsert = Util.ByteToHexString(payload, 3, 1);
                        }
                        break;
                    case 0x27:
                        DescriptionToInsert = "WRITE EEPROM";

                        if (message.Length >= 5)
                        {
                            DescriptionToInsert = "WRITE EEPROM | OFFSET: " + Util.ByteToHexString(payload, 0, 2);

                            switch (payload[3])
                            {
                                case 0xE2:
                                    ValueToInsert = Util.ByteToHexString(payload, 2, 1);
                                    UnitToInsert = "OK";
                                    break;
                                case 0xE4:
                                    ValueToInsert = "UNKNOWN RESULT";
                                    break;
                                case 0xE5:
                                    ValueToInsert = "UNKNOWN RESULT";
                                    break;
                                case 0xF0:
                                    ValueToInsert = "DENIED (INVALID OFFSET)";
                                    break;
                                case 0xF1:
                                    ValueToInsert = "DENIED (SECURITY LEVEL)";
                                    break;
                                default:
                                    ValueToInsert = Util.ByteToHexString(payload, 3, 1);
                                    break;
                            }
                        }
                        break;
                    case 0x28:
                        DescriptionToInsert = "READ EEPROM";

                        if (message.Length >= 4)
                        {
                            DescriptionToInsert = "READ EEPROM | OFFSET: " + Util.ByteToHexString(payload, 0, 2);
                            ValueToInsert = Util.ByteToHexString(payload, 2, 1);
                        }
                        break;
                    case 0x29:
                        DescriptionToInsert = "WRITE RAM";

                        if (message.Length >= 5)
                        {
                            DescriptionToInsert = "WRITE RAM | OFFSET: " + Util.ByteToHexString(payload, 0, 2);

                            switch (payload[3])
                            {
                                case 0xE5:
                                    ValueToInsert = Util.ByteToHexString(payload, 2, 1);
                                    UnitToInsert = "OK";
                                    break;
                                case 0xF0:
                                    ValueToInsert = "DENIED (INVALID OFFSET)";
                                    break;
                                case 0xF1:
                                    ValueToInsert = "DENIED (SECURITY LEVEL)";
                                    break;
                                default:
                                    ValueToInsert = Util.ByteToHexString(payload, 3, 1);
                                    break;
                            }
                        }
                        break;
                    case 0x2A:
                        DescriptionToInsert = "CONFIGURATION";

                        if (message.Length >= 3)
                        {
                            switch (payload[0])
                            {
                                case 0x01:
                                    DescriptionToInsert = "CONFIGURATION | PART NUMBER 1-2";
                                    ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                                    break;
                                case 0x02:
                                    DescriptionToInsert = "CONFIGURATION | PART NUMBER 3-4";
                                    ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                                    break;
                                case 0x03:
                                    DescriptionToInsert = "CONFIGURATION | PART NUMBER 5-6";
                                    ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                                    break;
                                case 0x04:
                                    DescriptionToInsert = "CONFIGURATION | PART NUMBER 7-8";
                                    ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                                    break;
                                case 0x06:
                                    DescriptionToInsert = "CONFIGURATION | EMISSION STANDARD";

                                    switch (payload[1])
                                    {
                                        case 1:
                                            ValueToInsert = "OBD-II BY CARB";
                                            break;
                                        case 2:
                                            ValueToInsert = "OBD BY EPA";
                                            break;
                                        case 3:
                                            ValueToInsert = "OBD AND OBD-II";
                                            break;
                                        case 4:
                                            ValueToInsert = "OBD-I";
                                            break;
                                        case 5:
                                            ValueToInsert = "NOT OBD COMPLIANT";
                                            break;
                                        case 6:
                                            ValueToInsert = "EOBD (EUROPE)";
                                            break;
                                        case 7:
                                            ValueToInsert = "EOBD AND OBD-II";
                                            break;
                                        case 8:
                                            ValueToInsert = "EOBD AND OBD";
                                            break;
                                        case 9:
                                            ValueToInsert = "EOBD OBD OBD-II";
                                            break;
                                        case 10:
                                            ValueToInsert = "JOBD (JAPAN)";
                                            break;
                                        case 11:
                                            ValueToInsert = "JOBD AND OBD-II";
                                            break;
                                        case 12:
                                            ValueToInsert = "JOBD AND EOBD";
                                            break;
                                        case 13:
                                            ValueToInsert = "JOBD EOBD OBD-II";
                                            break;
                                        case 14:
                                            ValueToInsert = "RESERVED";
                                            break;
                                        case 15:
                                            ValueToInsert = "RESERVED";
                                            break;
                                        case 16:
                                            ValueToInsert = "RESERVED";
                                            break;
                                        case 17:
                                            ValueToInsert = "EMD";
                                            break;
                                        case 18:
                                            ValueToInsert = "EMD+";
                                            break;
                                        case 19:
                                            ValueToInsert = "HD OBD-C";
                                            break;
                                        case 20:
                                            ValueToInsert = "HD OBD";
                                            break;
                                        case 21:
                                            ValueToInsert = "WWH OBD";
                                            break;
                                        case 22:
                                            ValueToInsert = "RESERVED";
                                            break;
                                        case 23:
                                            ValueToInsert = "HD EOBD-I";
                                            break;
                                        case 24:
                                            ValueToInsert = "HD EOBD-I N";
                                            break;
                                        case 25:
                                            ValueToInsert = "HD EOBD-II";
                                            break;
                                        case 26:
                                            ValueToInsert = "HD EOBD-II N";
                                            break;
                                        case 27:
                                            ValueToInsert = "RESERVED";
                                            break;
                                        case 28:
                                            ValueToInsert = "OBDBR-1";
                                            break;
                                        case 29:
                                            ValueToInsert = "OBDBR-2";
                                            break;
                                        case 30:
                                            ValueToInsert = "KOBD";
                                            break;
                                        case 31:
                                            ValueToInsert = "IOBD-I";
                                            break;
                                        case 32:
                                            ValueToInsert = "IOBD-II";
                                            break;
                                        case 33:
                                            ValueToInsert = "HD EOBD-IV";
                                            break;
                                        default:
                                            ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                                            break;
                                    }
                                    break;
                                case 0x07:
                                    DescriptionToInsert = "CONFIGURATION | CHASSIS TYPE";
                                    ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                                    break;
                                case 0x08:
                                    DescriptionToInsert = "CONFIGURATION | ASPIRATION TYPE";

                                    switch (payload[1]) // to be verified!
                                    {
                                        case 1:
                                            ValueToInsert = "NATURAL";
                                            break;
                                        default:
                                            ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                                            break;
                                    }
                                    break;
                                case 0x09:
                                    DescriptionToInsert = "CONFIGURATION | INJECTION TYPE";

                                    switch (payload[1]) // to be verified!
                                    {
                                        case 1:
                                            ValueToInsert = "DIRECT";
                                            break;
                                        case 2:
                                            ValueToInsert = "SEQUENTIAL";
                                            break;
                                        case 3:
                                            ValueToInsert = "SINGLE-POINT";
                                            break;
                                        case 4:
                                            ValueToInsert = "MULTI-POINT";
                                            break;
                                        default:
                                            ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                                            break;
                                    }
                                    break;
                                case 0x0A:
                                    DescriptionToInsert = "CONFIGURATION | FUEL TYPE";

                                    switch (payload[1])
                                    {
                                        case 0x01:
                                            ValueToInsert = "UNLEADED GAS";
                                            break;
                                        case 0x02:
                                            ValueToInsert = "DIESEL";
                                            break;
                                        case 0x03:
                                            ValueToInsert = "PROPANE";
                                            break;
                                        case 0x04:
                                            ValueToInsert = "METHANOL";
                                            break;
                                        case 0x05:
                                            ValueToInsert = "LEADED GAS";
                                            break;
                                        case 0x06:
                                            ValueToInsert = "FLEX";
                                            break;
                                        case 0x07:
                                            ValueToInsert = "CNG";
                                            break;
                                        case 0x08:
                                            ValueToInsert = "ELECTRIC";
                                            break;
                                        default:
                                            ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                                            break;
                                    }
                                    break;
                                case 0x0B:
                                    DescriptionToInsert = "CONFIGURATION | MODEL YEAR";

                                    switch (payload[1])
                                    {
                                        case 0x01:
                                            ValueToInsert = "1991";
                                            break;
                                        case 0x02:
                                            ValueToInsert = "1992";
                                            break;
                                        case 0x03:
                                            ValueToInsert = "1993";
                                            break;
                                        case 0x04:
                                            ValueToInsert = "1994";
                                            break;
                                        case 0x05:
                                            ValueToInsert = "1995";
                                            break;
                                        case 0x06:
                                            ValueToInsert = "1996";
                                            break;
                                        case 0x07:
                                            ValueToInsert = "1997";
                                            break;
                                        case 0x08:
                                            ValueToInsert = "1998";
                                            break;
                                        case 0x09:
                                            ValueToInsert = "1999";
                                            break;
                                        case 0x0A:
                                            ValueToInsert = "2000";
                                            break;
                                        case 0x0B:
                                            ValueToInsert = "2001";
                                            break;
                                        case 0x0C:
                                            ValueToInsert = "2002";
                                            break;
                                        case 0x0D:
                                            ValueToInsert = "2003";
                                            break;
                                        case 0x0E:
                                            ValueToInsert = "2004";
                                            break;
                                        case 0x0F:
                                            ValueToInsert = "2005";
                                            break;
                                        default:
                                            ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                                            break;
                                    }
                                    break;
                                case 0x0C:
                                    DescriptionToInsert = "CONFIGURATION | ENGINE DISPLACEMENT | CONFIG";

                                    switch (payload[1])
                                    {
                                        case 0x01:
                                            ValueToInsert = "2.2L | I4 E-W";
                                            break;
                                        case 0x02:
                                            ValueToInsert = "2.5L | I4 E-W";
                                            break;
                                        case 0x03:
                                            ValueToInsert = "3.0L | V6 E-W";
                                            break;
                                        case 0x04:
                                            ValueToInsert = "3.3L | V6 E-W";
                                            break;
                                        case 0x05:
                                            ValueToInsert = "3.9L | V6 N-S";
                                            break;
                                        case 0x06:
                                            ValueToInsert = "5.2L | V8 N-S";
                                            break;
                                        case 0x07:
                                            ValueToInsert = "5.9L | V8 N-S";
                                            break;
                                        case 0x08:
                                            ValueToInsert = "3.8L | V6 E-W";
                                            break;
                                        case 0x09:
                                            ValueToInsert = "4.0L | I6 N-S";
                                            break;
                                        case 0x0A:
                                            ValueToInsert = "2.0L | I4 E-W SOHC";
                                            break;
                                        case 0x0B:
                                            ValueToInsert = "3.5L | V6 N-S";
                                            break;
                                        case 0x0C:
                                            ValueToInsert = "8.0L | V10 N-S";
                                            break;
                                        case 0x0D:
                                            ValueToInsert = "2.4L | I4 E-W";
                                            break;
                                        case 0x0E:
                                            ValueToInsert = "2.5L | I4 N-S";
                                            break;
                                        case 0x0F:
                                            ValueToInsert = "2.5L | V6 N-S";
                                            break;
                                        case 0x10:
                                            ValueToInsert = "2.0L | I4 E-W DOHC";
                                            break;
                                        case 0x11:
                                            ValueToInsert = "2.5L | V6 E-W";
                                            break;
                                        case 0x12:
                                            ValueToInsert = "5.9L | I6 N-S";
                                            break;
                                        case 0x13:
                                            ValueToInsert = "3.3L | V6 N-S";
                                            break;
                                        case 0x14:
                                            ValueToInsert = "2.7L | V6 N-S";
                                            break;
                                        case 0x15:
                                            ValueToInsert = "3.2L | V6 N-S";
                                            break;
                                        case 0x16:
                                            ValueToInsert = "1.8L | I4 E-W";
                                            break;
                                        case 0x17:
                                            ValueToInsert = "3.7L | V6 N-S";
                                            break;
                                        case 0x18:
                                            ValueToInsert = "4.7L | V8 N-S";
                                            break;
                                        case 0x19:
                                            ValueToInsert = "1.9L | I4 E-W";
                                            break;
                                        case 0x1A:
                                            ValueToInsert = "3.1L | I5 N-S";
                                            break;
                                        case 0x1B:
                                            ValueToInsert = "1.6L | I4 E-W";
                                            break;
                                        case 0x1C:
                                            ValueToInsert = "2.7L | V6 E-W";
                                            break;
                                        default:
                                            ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                                            break;
                                    }
                                    break;
                                case 0x0D:
                                    DescriptionToInsert = "CONFIGURATION | A/C SYSTEM TYPE";
                                    ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                                    break;
                                case 0x0E:
                                    DescriptionToInsert = "CONFIGURATION | ENGINE MANUFACTURER";
                                    ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                                    break;
                                case 0x0F:
                                    DescriptionToInsert = "CONFIGURATION | CONTROLLER HARDWARE TYPE";

                                    switch (payload[1]) // to be verified!
                                    {
                                        case 18:
                                            ValueToInsert = "SBEC3";
                                            break;
                                        default:
                                            ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                                            break;
                                    }
                                    break;
                                case 0x10:
                                    DescriptionToInsert = "CONFIGURATION | BODY STYLE";

                                    switch (payload[1]) // to be verified!
                                    {
                                        case 1:
                                            ValueToInsert = "NS";
                                            break;
                                        case 2:
                                            ValueToInsert = "PL";
                                            break;
                                        case 3:
                                            ValueToInsert = "FJ";
                                            break;
                                        case 4:
                                            ValueToInsert = "LH";
                                            break;
                                        case 5:
                                            ValueToInsert = "PR";
                                            break;
                                        case 6:
                                            ValueToInsert = "JA";
                                            break;
                                        case 7:
                                            ValueToInsert = "JX";
                                            break;
                                        default:
                                            ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                                            break;
                                    }
                                    break;
                                case 0x11:
                                    DescriptionToInsert = "CONFIGURATION | MODULE SOFTWARE PHASE";
                                    ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                                    break;
                                case 0x12:
                                    DescriptionToInsert = "CONFIGURATION | MODULE SOFTWARE VERSION";
                                    ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                                    break;
                                case 0x13:
                                    DescriptionToInsert = "CONFIGURATION | MODULE SOFTWARE FAMILY";
                                    ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                                    break;
                                case 0x14:
                                    DescriptionToInsert = "CONFIGURATION | MODULE SOFTWARE GROUP AND MONTH";
                                    ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                                    break;
                                case 0x15:
                                    DescriptionToInsert = "CONFIGURATION | MODULE SOFTWARE DAY";
                                    ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                                    break;
                                case 0x16:
                                    DescriptionToInsert = "CONFIGURATION | TRANSMISSION TYPE";

                                    switch (payload[1])
                                    {
                                        case 1:
                                            ValueToInsert = "ATX ?-SPEED";
                                            break;
                                        case 2:
                                            ValueToInsert = "MTX";
                                            break;
                                        case 3:
                                            ValueToInsert = "ATX 3-SPEED";
                                            break;
                                        case 4:
                                            ValueToInsert = "ATX ?-SPEED";
                                            break;
                                        default:
                                            ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                                            break;
                                    }
                                    break;
                                case 0x17:
                                    DescriptionToInsert = "CONFIGURATION | PART NUMBER REVISION 1";
                                    ValueToInsert = Encoding.ASCII.GetString(payload, 1, 1);
                                    break;
                                case 0x18:
                                    DescriptionToInsert = "CONFIGURATION | PART NUMBER REVISION 2";
                                    ValueToInsert = Encoding.ASCII.GetString(payload, 1, 1);
                                    break;
                                case 0x19:
                                    DescriptionToInsert = "CONFIGURATION | SOFTWARE REVISION LEVEL";
                                    ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                                    break;
                                case 0x1A:
                                    DescriptionToInsert = "CONFIGURATION | HOMOLOGATION NUMBER 1";
                                    ValueToInsert = Util.ByteToHexString(payload, 1, 1) + " | " + Encoding.ASCII.GetString(payload, 1, 1);
                                    break;
                                case 0x1B:
                                    DescriptionToInsert = "CONFIGURATION | HOMOLOGATION NUMBER 2";
                                    ValueToInsert = Util.ByteToHexString(payload, 1, 1) + " | " + Encoding.ASCII.GetString(payload, 1, 1);
                                    break;
                                case 0x1C:
                                    DescriptionToInsert = "CONFIGURATION | HOMOLOGATION NUMBER 3";
                                    ValueToInsert = Util.ByteToHexString(payload, 1, 1) + " | " + Encoding.ASCII.GetString(payload, 1, 1);
                                    break;
                                case 0x1D:
                                    DescriptionToInsert = "CONFIGURATION | HOMOLOGATION NUMBER 4";
                                    ValueToInsert = Util.ByteToHexString(payload, 1, 1) + " | " + Encoding.ASCII.GetString(payload, 1, 1);
                                    break;
                                case 0x1E:
                                    DescriptionToInsert = "CONFIGURATION | HOMOLOGATION NUMBER 5";
                                    ValueToInsert = Util.ByteToHexString(payload, 1, 1) + " | " + Encoding.ASCII.GetString(payload, 1, 1);
                                    break;
                                case 0x1F:
                                    DescriptionToInsert = "CONFIGURATION | HOMOLOGATION NUMBER 6";
                                    ValueToInsert = Util.ByteToHexString(payload, 1, 1) + " | " + Encoding.ASCII.GetString(payload, 1, 1);
                                    break;
                                default:
                                    DescriptionToInsert = "CONFIGURATION | " + "OFFSET: " + Util.ByteToHexString(payload, 0, 1);
                                    ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                                    break;
                            }
                        }
                        break;
                    case 0x2B:
                        DescriptionToInsert = "GET SECURITY SEED";

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
                                    DescriptionToInsert = "GET SECURITY SEED | KEY: " + Util.ByteToHexStringSimple(keyArray);
                                }
                                else
                                {
                                    DescriptionToInsert = "GET SECURITY SEED | PCM ALREADY UNLOCKED";
                                }

                                ValueToInsert = Util.ByteToHexString(payload, 0, 2);
                            }
                            else
                            {
                                DescriptionToInsert = "GET SECURITY SEED";
                                ValueToInsert = "CHECKSUM ERROR";
                            }
                        }
                        break;
                    case 0x2C:
                        DescriptionToInsert = "SEND SECURITY KEY";

                        if (message.Length >= 5)
                        {
                            switch (payload[3])
                            {
                                case 0x00:
                                    ValueToInsert = "ACCEPTED";
                                    break;
                                case 0x01:
                                    ValueToInsert = "INCORRECT KEY";
                                    break;
                                case 0x02:
                                    ValueToInsert = "CHECKSUM ERROR";
                                    break;
                                case 0x03:
                                    ValueToInsert = "BLOCKED | RESTART PCM";
                                    break;
                                default:
                                    ValueToInsert = Util.ByteToHexString(payload, 3, 1);
                                    break;
                            }
                        }
                        break;
                    case 0x2D:
                        DescriptionToInsert = "READ CONFIGURATION CONSTANT";

                        if (message.Length >= 5)
                        {
                            DescriptionToInsert = "CONFIGURATION | PAGE: " + Util.ByteToHexString(payload, 0, 1) + " | ITEM: " + Util.ByteToHexString(payload, 1, 1);
                            ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                        }
                        break;
                    case 0x2E:
                    case 0x32:
                    case 0x33:
                        DescriptionToInsert = "ONE-TRIP ENGINE FAULT CODES";

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
                                EngineFaultCode1TList.Clear();
                                EngineFaultCode1TList.AddRange(engineFaultCode1TPayload);
                                EngineFaultCode1TList.Remove(0xFD); // not fault code related
                                EngineFaultCode1TList.Remove(0xFE); // end of fault code list signifier

                                if (EngineFaultCode1TList.Count > 0)
                                {
                                    ValueToInsert = Util.ByteToHexStringSimple(EngineFaultCode1TList.ToArray());
                                    EngineFaultCodes1TSaved = false;
                                }
                                else
                                {
                                    ValueToInsert = "NO FAULT CODES";
                                    EngineFaultCodes1TSaved = false;
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
                        DescriptionToInsert = "GET SECURITY SEED";

                        if (message.Length >= 5)
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

                                        DescriptionToInsert = "GET SECURITY SEED #1 | KEY: " + Util.ByteToHexStringSimple(keyArray);
                                    }
                                    else
                                    {
                                        DescriptionToInsert = "GET SECURITY SEED #1 | PCM ALREADY UNLOCKED";
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

                                        DescriptionToInsert = "GET SECURITY SEED #2 | KEY: " + Util.ByteToHexStringSimple(keyArray);
                                    }
                                    else
                                    {
                                        DescriptionToInsert = "GET SECURITY SEED #2 | PCM ALREADY UNLOCKED";
                                    }
                                }

                                ValueToInsert = Util.ByteToHexString(payload, 1, 2);
                            }
                            else
                            {
                                if (payload[0] == 1)
                                {
                                    DescriptionToInsert = "GET SECURITY SEED #1";
                                }
                                else if (payload[0] == 2)
                                {
                                    DescriptionToInsert = "GET SECURITY SEED #2";
                                }

                                ValueToInsert = "CHECKSUM ERROR";
                            }
                        }
                        break;
                    case 0xFE:
                        DescriptionToInsert = "SELECT LOW-SPEED MODE";
                        break;
                    case 0xFF:
                        DescriptionToInsert = "PCM WAKE UP";
                        break;
                    default:
                        DescriptionToInsert = string.Empty;
                        break;
                }
            }
            else if ((speed == "62500 baud") || (speed == "125000 baud"))
            {
                switch (ID)
                {
                    case 0x00:
                        DescriptionToInsert = "PCM WAKE UP";
                        break;
                    case 0x06:
                        DescriptionToInsert = "SET BOOTSTRAP BAUDRATE TO 62500 BAUD";
                        ValueToInsert = "OK";
                        break;
                    case 0x11:
                        DescriptionToInsert = "UPLOAD WORKER FUNCTION";

                        if (message.Length >= 3)
                        {
                            ushort size = (ushort)((payload[0] << 8) + payload[1]);
                            ushort echoCount = (ushort)(payload.Length - 3);

                            DescriptionToInsert = "UPLOAD WORKER FUNCTION | SIZE: " + size.ToString() + " BYTES";
                            ValueToInsert = Util.ByteToHexString(payload, 2, payload.Length - 3);

                            if ((echoCount == size) && (payload[payload.Length - 1] == 0x14))
                            {
                                UnitToInsert = "OK";
                            }
                            else
                            {
                                UnitToInsert = "ERROR";
                            }
                        }
                        break;
                    case 0x21:
                        DescriptionToInsert = "START WORKER FUNCTION";

                        if (message.Length > 1)
                        {
                            DescriptionToInsert += " | RESULT";
                            ValueToInsert = Util.ByteToHexStringSimple(payload.ToArray());
                        }
                        else if ((message.Length == 2) && (payload[0] == 0x22))
                        {
                            ValueToInsert = "FINISHED";
                        }
                        break;
                    case 0x22:
                        DescriptionToInsert = "EXIT WORKER FUNCTION";
                        break;
                    case 0x24:
                        DescriptionToInsert = "REQUEST/SEND BOOTSTRAP SECURITY SEED/KEY";

                        if (message.Length >= 5)
                        {
                            if (message.Length == 5)
                            {
                                byte checksum = (byte)(message[0] + message[1] + message[2] + message[3]);

                                DescriptionToInsert = "REQUEST BOOTSTRAP SECURITY SEED";

                                if (message[4] == checksum)
                                {
                                    if ((message[2] == 0x27) && (message[3] == 0xC1))
                                    {
                                        ValueToInsert = "OK";
                                    }
                                    else
                                    {
                                        ValueToInsert = "ERROR";
                                    }
                                }
                                else
                                {
                                    ValueToInsert = "CHECKSUM ERROR";
                                }
                            }
                            else if (message.Length == 7)
                            {
                                byte checksum = (byte)(message[0] + message[1] + message[2] + message[3] + message[4] + message[5]);

                                DescriptionToInsert = "SEND BOOTSTRAP SECURITY KEY";

                                if (message[6] == checksum)
                                {
                                    if ((message[2] == 0x27) && (message[3] == 0xC2))
                                    {
                                        ValueToInsert = Util.ByteToHexString(message, 4, 2);
                                    }
                                    else
                                    {
                                        ValueToInsert = "ERROR";
                                    }
                                }
                                else
                                {
                                    ValueToInsert = "CHECKSUM ERROR";
                                }
                            }
                            else
                            {
                                DescriptionToInsert = "REQUEST BOOTSTRAP SECURITY SEED";
                            }
                        }
                        break;
                    case 0x26:
                        DescriptionToInsert = "BOOTSTRAP SECURITY STATUS";

                        if (message.Length >= 5)
                        {
                            if (message.Length == 5)
                            {
                                byte checksum = (byte)(message[0] + message[1] + message[2] + message[3]);

                                DescriptionToInsert = "BOOTSTRAP SECURITY STATUS";

                                if (message[4] == checksum)
                                {
                                    if ((message[2] == 0x67) && (message[3] == 0xC2))
                                    {
                                        ValueToInsert = "UNLOCKED";
                                    }
                                    else
                                    {
                                        ValueToInsert = "LOCKED";
                                    }
                                }
                                else
                                {
                                    ValueToInsert = "CHECKSUM ERROR";
                                }
                            }
                            else if (message.Length == 7)
                            {
                                byte checksum = (byte)(message[0] + message[1] + message[2] + message[3] + message[4] + message[5]);

                                DescriptionToInsert = "BOOTSTRAP SECURITY SEED RECEIVED";

                                if (message[6] == checksum)
                                {
                                    if ((message[2] == 0x67) && (message[3] == 0xC1))
                                    {
                                        ValueToInsert = Util.ByteToHexString(payload, 3, 2);
                                    }
                                }
                                else
                                {
                                    ValueToInsert = "CHECKSUM ERROR";
                                }
                            }
                        }
                        break;
                    case 0x31:
                        DescriptionToInsert = "WRITE FLASH BLOCK";

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

                            DescriptionToInsert = "WRITE FLASH BLOCK | OFFSET: " + Util.ByteToHexStringSimple(offset.ToArray()) + " | SIZE: " + Util.ByteToHexStringSimple(length.ToArray());

                            if (echoCount == blockSize)
                            {
                                ValueToInsert = Util.ByteToHexStringSimple(values.ToArray());
                                UnitToInsert = "OK";
                            }
                            else
                            {
                                switch (message[message.Length - 1]) // last payload byte stores error status
                                {
                                    case 0x01:
                                        ValueToInsert = "WRITE ERROR";
                                        break;
                                    case 0x80:
                                        ValueToInsert = "INVALID BLOCK SIZE";
                                        break;
                                    default:
                                        ValueToInsert = "UNKNOWN ERROR";
                                        break;
                                }
                            }
                        }
                        break;
                    case 0x34:
                    case 0x46: // Dino
                        DescriptionToInsert = "READ FLASH BLOCK";

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

                            DescriptionToInsert = "READ FLASH BLOCK | OFFSET: " + Util.ByteToHexStringSimple(offset.ToArray()) + " | SIZE: " + Util.ByteToHexStringSimple(length.ToArray());

                            ushort blockSize = (ushort)((payload[3] << 8) + payload[4]);
                            ushort echoCount = (ushort)(payload.Length - 5);

                            if (echoCount == blockSize)
                            {
                                ValueToInsert = Util.ByteToHexStringSimple(values.ToArray());
                                UnitToInsert = "OK";
                            }
                            else
                            {
                                switch (message[message.Length - 1]) // last payload byte stores error status
                                {
                                    case 0x80:
                                        ValueToInsert = "INVALID BLOCK SIZE";
                                        break;
                                    default:
                                        ValueToInsert = "UNKNOWN ERROR";
                                        break;
                                }
                            }
                        }
                        break;
                    case 0x37:
                        DescriptionToInsert = "WRITE EEPROM BLOCK";

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

                            DescriptionToInsert = "WRITE EEPROM BLOCK | OFFSET: " + Util.ByteToHexStringSimple(offset.ToArray()) + " | SIZE: " + Util.ByteToHexStringSimple(length.ToArray());

                            ushort blockSize = (ushort)((payload[2] << 8) + payload[3]);
                            ushort echoCount = (ushort)(payload.Length - 4);

                            if ((echoCount == blockSize) && (offset[0] < 2))
                            {
                                ValueToInsert = Util.ByteToHexStringSimple(values.ToArray());
                                UnitToInsert = "OK";
                            }
                            else
                            {
                                switch (message[message.Length - 1]) // last payload byte stores error status
                                {
                                    case 0x80:
                                        ValueToInsert = "INVALID BLOCK SIZE";
                                        break;
                                    case 0x83:
                                        ValueToInsert = "INVALID OFFSET";
                                        break;
                                    default:
                                        ValueToInsert = "UNKNOWN ERROR";
                                        break;
                                }
                            }
                        }
                        break;
                    case 0x3A:
                        DescriptionToInsert = "READ EEPROM BLOCK";

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

                            DescriptionToInsert = "READ EEPROM BLOCK | OFFSET: " + Util.ByteToHexStringSimple(offset.ToArray()) + " | SIZE: " + Util.ByteToHexStringSimple(length.ToArray());

                            ushort blockSize = (ushort)((payload[2] << 8) + payload[3]);
                            ushort echoCount = (ushort)(payload.Length - 4);

                            if ((echoCount == blockSize) && (offset[0] < 2))
                            {
                                ValueToInsert = Util.ByteToHexStringSimple(values.ToArray());
                                UnitToInsert = "OK";
                            }
                            else
                            {
                                switch (message[message.Length - 1]) // last payload byte stores error status
                                {
                                    case 0x80:
                                        ValueToInsert = "INVALID BLOCK SIZE";
                                        break;
                                    case 0x83:
                                        ValueToInsert = "INVALID OFFSET";
                                        break;
                                    default:
                                        ValueToInsert = "UNKNOWN ERROR";
                                        break;
                                }
                            }
                        }
                        break;
                    case 0x47:
                        DescriptionToInsert = "START BOOTLOADER";

                        if (message.Length >= 4)
                        {
                            List<byte> offset = new List<byte>();
                            offset.Clear();
                            offset.AddRange(payload.Take(2));
                            DescriptionToInsert = "START BOOTLOADER | OFFSET: " + Util.ByteToHexStringSimple(offset.ToArray());

                            if (payload[2] == 0x22) ValueToInsert = "OK";
                            else ValueToInsert = "ERROR";
                        }
                        else
                        {
                            ValueToInsert = "ERROR";
                        }
                        break;
                    case 0x4C:
                        DescriptionToInsert = "UPLOAD BOOTLOADER";

                        if (message.Length >= 6)
                        {
                            List<byte> OffsetStart = new List<byte>();
                            List<byte> OffsetEnd = new List<byte>();
                            OffsetStart.Clear();
                            OffsetEnd.Clear();
                            OffsetStart.AddRange(payload.Take(2));
                            OffsetEnd.AddRange(payload.Skip(2).Take(2));
                            DescriptionToInsert = "UPLOAD BOOTLOADER | START: " + Util.ByteToHexStringSimple(OffsetStart.ToArray()) + " | END: " + Util.ByteToHexStringSimple(OffsetEnd.ToArray());
                            ValueToInsert = Util.ByteToHexString(payload, 4, payload.Length - 4);

                            ushort start = (ushort)((payload[0] << 8) + payload[1]);
                            ushort end = (ushort)((payload[2] << 8) + payload[3]);

                            if ((end - start + 1) == (payload.Length - 4))
                            {
                                UnitToInsert = "OK";
                            }
                            else
                            {
                                UnitToInsert = "ERROR";
                            }
                        }
                        break;
                    case 0xDB:
                        DescriptionToInsert = string.Empty;

                        if (message.Length >= 5)
                        {
                            if (payload[0] == 0x2F && payload[1] == 0xD8 && payload[2] == 0x3E && payload[3] == 0x23)
                            {
                                DescriptionToInsert = "BOOTSTRAP MODE NOT PROTECTED";
                            }
                            else
                            {
                                DescriptionToInsert = "PING";
                            }
                        }
                        break;
                    case 0xF0:
                        DescriptionToInsert = "F0 RAM TABLE SELECTED";

                        if (message.Length >= 3)
                        {
                            ushort num = (ushort)(payload.Length / 2); // number of byte pairs (offset value)
                            List<byte> offsets = new List<byte>();
                            List<byte> values = new List<byte>();
                            offsets.Clear();
                            values.Clear();

                            for (int i = 0; i < num; i++)
                            {
                                offsets.Add(payload[i * 2]);
                                values.Add(payload[(i * 2) + 1]);
                            }

                            DescriptionToInsert = "F0 RAM TABLE | OFFSET: " + Util.ByteToHexStringSimple(offsets.ToArray());
                            ValueToInsert = Util.ByteToHexStringSimple(values.ToArray());
                        }
                        break;
                    case 0xF1:
                        DescriptionToInsert = "F1 RAM TABLE SELECTED";

                        if (message.Length >= 3)
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

                            DescriptionToInsert = "F1 RAM TABLE | OFFSET: " + Util.ByteToHexStringSimple(offsets.ToArray());
                            ValueToInsert = Util.ByteToHexStringSimple(values.ToArray());
                        }
                        break;
                    case 0xF2:
                        DescriptionToInsert = "F2 RAM TABLE SELECTED";

                        if (message.Length >= 3)
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

                            DescriptionToInsert = "F2 RAM TABLE | OFFSET: " + Util.ByteToHexStringSimple(offsets.ToArray());
                            ValueToInsert = Util.ByteToHexStringSimple(values.ToArray());
                        }
                        break;
                    case 0xF3:
                        DescriptionToInsert = "F3 RAM TABLE SELECTED";

                        if (message.Length >= 3)
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

                            DescriptionToInsert = "F3 RAM TABLE | OFFSET: " + Util.ByteToHexStringSimple(offsets.ToArray());
                            ValueToInsert = Util.ByteToHexStringSimple(values.ToArray());
                        }
                        break;
                    case 0xF4:
                        DescriptionToInsert = "F4 RAM TABLE SELECTED";

                        if (message.Length >= 3)
                        {
                            switch (payload[0])
                            {
                                case 0x01:
                                    DescriptionToInsert = "DTC 1: ";

                                    if (payload[1] == 0) ValueToInsert = "EMPTY DTC SLOT";
                                    else
                                    {
                                        int index = EngineDTC.Rows.IndexOf(EngineDTC.Rows.Find(payload[1]));

                                        if (index > -1) // DTC description found
                                        {
                                            DescriptionToInsert += EngineDTC.Rows[index]["description"];
                                        }
                                        else // no DTC description found
                                        {
                                            DescriptionToInsert += "UNRECOGNIZED DTC";
                                        }
                                    }
                                    break;
                                case 0x02:
                                    DescriptionToInsert = "DTC 8: ";

                                    if (payload[1] == 0) ValueToInsert = "EMPTY DTC SLOT";
                                    else
                                    {
                                        int index = EngineDTC.Rows.IndexOf(EngineDTC.Rows.Find(payload[1]));

                                        if (index > -1) // DTC description found
                                        {
                                            DescriptionToInsert += EngineDTC.Rows[index]["description"];
                                        }
                                        else // no DTC description found
                                        {
                                            DescriptionToInsert += "UNRECOGNIZED DTC";
                                        }
                                    }
                                    break;
                                case 0x03:
                                    DescriptionToInsert = "KEY-ON CYCLES ERROR 1";
                                    ValueToInsert = payload[1].ToString("0");
                                    break;
                                case 0x04:
                                    DescriptionToInsert = "KEY-ON CYCLES ERROR 2";
                                    ValueToInsert = payload[1].ToString("0");
                                    break;
                                case 0x05:
                                    DescriptionToInsert = "KEY-ON CYCLES ERROR 3";
                                    ValueToInsert = payload[1].ToString("0");
                                    break;
                                case 0x06:
                                    DescriptionToInsert = "DTC COUNTER 1";
                                    ValueToInsert = payload[1].ToString("0");
                                    break;
                                case 0x07:
                                    DescriptionToInsert = "DTC COUNTER 2";
                                    ValueToInsert = payload[1].ToString("0");
                                    break;
                                case 0x08:
                                    DescriptionToInsert = "DTC COUNTER 3";
                                    ValueToInsert = payload[1].ToString("0");
                                    break;
                                case 0x09:
                                    DescriptionToInsert = "DTC COUNTER 4";
                                    ValueToInsert = payload[1].ToString("0");
                                    break;
                                case 0x0A:
                                    DescriptionToInsert = "ENGINE SPEED";

                                    if (message.Length >= 5)
                                    {
                                        if (payload[2] == 0x0B)
                                        {
                                            double EngineSpeed = ((payload[1] << 8) + payload[3]) * 0.125;
                                            ValueToInsert = Math.Round(EngineSpeed, 3).ToString("0.000").Replace(",", ".");
                                            UnitToInsert = "RPM";
                                        }
                                        else
                                        {
                                            DescriptionToInsert += " | ERROR: REQUEST F4 0A 0B";
                                        }
                                    }
                                    else
                                    {
                                        DescriptionToInsert += " | HIGH-RESOLUTION: F4 0A 0B";
                                        double EngineSpeed = payload[1] * 32.0;
                                        ValueToInsert = EngineSpeed.ToString("0");
                                        UnitToInsert = "RPM";
                                    }
                                    break;
                                case 0x0B:
                                    DescriptionToInsert = "ENGINE SPEED | ERROR: REQUEST F4 0A 0B";
                                    break;
                                case 0x0C:
                                    DescriptionToInsert = "VEHICLE SPEED";

                                    if (message.Length >= 5)
                                    {
                                        if (payload[2] == 0x0D)
                                        {
                                            double VehicleSpeedMPH = ((payload[1] << 8) + payload[3]) * 0.015625;
                                            double VehicleSpeedKMH = ((payload[1] << 8) + payload[3]) * 0.015625 * 1.609344;

                                            if (Properties.Settings.Default.Units == "imperial")
                                            {
                                                ValueToInsert = Math.Round(VehicleSpeedMPH, 3).ToString("0.000").Replace(",", ".");
                                                UnitToInsert = "MPH";
                                            }
                                            else if (Properties.Settings.Default.Units == "metric")
                                            {
                                                ValueToInsert = Math.Round(VehicleSpeedKMH, 3).ToString("0.000").Replace(",", ".");
                                                UnitToInsert = "KM/H";
                                            }
                                        }
                                        else
                                        {
                                            DescriptionToInsert += " | ERROR: REQUEST F4 0C 0D";
                                        }
                                    }
                                    else
                                    {
                                        DescriptionToInsert += " | HIGH-RESOLUTION: F4 0C 0D";

                                        double VehicleSpeedMPH = payload[1] * 4.0;
                                        double VehicleSpeedKMH = VehicleSpeedMPH * 1.609344;

                                        if (Properties.Settings.Default.Units == "imperial")
                                        {
                                            ValueToInsert = Math.Round(VehicleSpeedMPH).ToString("0").Replace(",", ".");
                                            UnitToInsert = "MPH";
                                        }
                                        else if (Properties.Settings.Default.Units == "metric")
                                        {
                                            ValueToInsert = Math.Round(VehicleSpeedKMH).ToString("0").Replace(",", ".");
                                            UnitToInsert = "KM/H";
                                        }
                                    }
                                    break;
                                case 0x0D:
                                    DescriptionToInsert = "VEHICLE SPEED | ERROR: REQUEST F4 0C 0D";
                                    break;
                                case 0x0E:
                                    DescriptionToInsert = "CRUISE | CONTROL BUTTON PRESSED";

                                    List<string> SwitchList = new List<string>();
                                    SwitchList.Clear();

                                    if (Util.IsBitSet(payload[1], 0)) SwitchList.Add("CANCEL");
                                    if (Util.IsBitSet(payload[1], 1)) SwitchList.Add("COAST");
                                    if (Util.IsBitClear(payload[1], 2)) SwitchList.Add("SET");
                                    if (Util.IsBitSet(payload[1], 3)) SwitchList.Add("ACC/RES");
                                    if (Util.IsBitSet(payload[1], 4)) SwitchList.Add("ON/OFF");
                                    if (Util.IsBitSet(payload[1], 7)) SwitchList.Add("BRAKE");

                                    if (SwitchList.Count > 0)
                                    {
                                        foreach (string s in SwitchList)
                                        {
                                            ValueToInsert += s + " | ";
                                        }

                                        if (ValueToInsert.Length > 2) ValueToInsert = ValueToInsert.Remove(ValueToInsert.Length - 3); // remove last "|" character
                                    }
                                    break;
                                case 0x0F:
                                    double BatteryVoltage = payload[1] * 0.0625;
                                    DescriptionToInsert = "BATTERY VOLTAGE";
                                    ValueToInsert = Math.Round(BatteryVoltage, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0x10:
                                    double ATSVoltage = payload[1] * 0.0196;
                                    DescriptionToInsert = "AMBIENT TEMPERATURE SENSOR VOLTAGE";
                                    ValueToInsert = Math.Round(ATSVoltage, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0x11:
                                    double AmbientTemperatureC = payload[1] - 128;
                                    double AmbientTemperatureF = 1.8 * AmbientTemperatureC + 32.0;

                                    DescriptionToInsert = "AMBIENT TEMPERATURE";

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        ValueToInsert = Math.Round(AmbientTemperatureF).ToString("0");
                                        UnitToInsert = "°F";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        ValueToInsert = AmbientTemperatureC.ToString("0");
                                        UnitToInsert = "°C";
                                    }
                                    break;
                                case 0x12:
                                    double TPSVoltage = payload[1] * 0.0196;
                                    DescriptionToInsert = "THROTTLE POSITION SENSOR (TPS) VOLTAGE";
                                    ValueToInsert = Math.Round(TPSVoltage, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0x13:
                                    double MinTPSVoltage = payload[1] * 0.0196;
                                    DescriptionToInsert = "MINIMUM TPS VOLTAGE";
                                    ValueToInsert = Math.Round(MinTPSVoltage, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0x14:
                                    double CalculatedTPSVoltage = payload[1] * 0.0196;
                                    DescriptionToInsert = "CALCULATED TPS VOLTAGE";
                                    ValueToInsert = Math.Round(CalculatedTPSVoltage, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0x15:
                                    double ECTSensorVoltage = payload[1] * 0.0196;
                                    DescriptionToInsert = "ENGINE COOLANT TEMPERATURE SENSOR VOLTAGE";
                                    ValueToInsert = Math.Round(ECTSensorVoltage, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0x16:
                                    double ECTC = payload[1] - 128;
                                    double ECTF = 1.8 * ECTC + 32.0;
                                    DescriptionToInsert = "ENGINE COOLANT TEMPERATURE";

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        ValueToInsert = Math.Round(ECTF).ToString("0");
                                        UnitToInsert = "°F";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        ValueToInsert = ECTC.ToString("0");
                                        UnitToInsert = "°C";
                                    }
                                    break;
                                case 0x17:
                                    double MAPSensorVoltage = payload[1] * 0.0196;
                                    DescriptionToInsert = "INTAKE MANIFOLD ABSOLUTE PRESSURE SENSOR VOLTAGE";
                                    ValueToInsert = Math.Round(MAPSensorVoltage, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0x18:
                                    double MAPPSI = payload[1] * 0.059756;
                                    double MAPKPA = payload[1] * 0.059756 * 6.894757;
                                    DescriptionToInsert = "INTAKE MANIFOLD ABSOLUTE PRESSURE";

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        ValueToInsert = Math.Round(MAPPSI, 1).ToString("0.0").Replace(",", ".");
                                        UnitToInsert = "PSI";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        ValueToInsert = Math.Round(MAPKPA, 1).ToString("0.0").Replace(",", ".");
                                        UnitToInsert = "KPA";
                                    }
                                    break;
                                case 0x19:
                                    double BarometricPressurePSI = payload[1] * 0.059756;
                                    double BarometricPressureKPA = payload[1] * 0.059756 * 6.894757;
                                    DescriptionToInsert = "BAROMETRIC PRESSURE";

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        ValueToInsert = Math.Round(BarometricPressurePSI, 1).ToString("0.0").Replace(",", ".");
                                        UnitToInsert = "PSI";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        ValueToInsert = Math.Round(BarometricPressureKPA, 1).ToString("0.0").Replace(",", ".");
                                        UnitToInsert = "KPA";
                                    }
                                    break;
                                case 0x1A:
                                    double MAPVacuumPSI = payload[1] * 0.059756;
                                    double MAPVacuumKPA = payload[1] * 0.059756 * 6.894757;
                                    DescriptionToInsert = "MAP VACUUM";

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        ValueToInsert = Math.Round(MAPVacuumPSI, 1).ToString("0.0").Replace(",", ".");
                                        UnitToInsert = "PSI";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        ValueToInsert = Math.Round(MAPVacuumKPA, 1).ToString("0.0").Replace(",", ".");
                                        UnitToInsert = "KPA";
                                    }
                                    break;
                                case 0x1B:
                                    double O2S11Voltage = payload[1] * 0.0196;
                                    DescriptionToInsert = "UPSTREAM O2 1/1 SENSOR VOLTAGE";
                                    ValueToInsert = Math.Round(O2S11Voltage, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0x1C:
                                    double O2S21Voltage = payload[1] * 0.0196;
                                    DescriptionToInsert = "UPSTREAM O2 2/1 SENSOR VOLTAGE";
                                    ValueToInsert = Math.Round(O2S21Voltage, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0x1D:
                                    double IATVoltage = payload[1] * 0.0196;
                                    DescriptionToInsert = "INTAKE AIR TEMPERATURE SENSOR VOLTAGE";
                                    ValueToInsert = Math.Round(IATVoltage, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0x1E:
                                    double IATC = payload[1] - 64;
                                    double IATF = 1.8 * IATC + 32.0;
                                    DescriptionToInsert = "INTAKE AIR TEMPERATURE";

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        ValueToInsert = Math.Round(IATF).ToString("0");
                                        UnitToInsert = "°F";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        ValueToInsert = IATC.ToString("0");
                                        UnitToInsert = "°C";
                                    }
                                    break;
                                case 0x1F:
                                    double KnockSensor1Voltage = payload[1] * 0.0196;
                                    DescriptionToInsert = "KNOCK SENSOR 1 VOLTAGE";
                                    ValueToInsert = Math.Round(KnockSensor1Voltage, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0x20:
                                    double KnockSensor2Voltage = payload[1] * 0.0196;
                                    DescriptionToInsert = "KNOCK SENSOR 2 VOLTAGE";
                                    ValueToInsert = Math.Round(KnockSensor2Voltage, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0x21:
                                    double CruiseSwitchVoltage = payload[1] * 0.0196;
                                    DescriptionToInsert = "CRUISE | SWITCH VOLTAGE";
                                    ValueToInsert = Math.Round(CruiseSwitchVoltage, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0x22:
                                    double BatteryTemperatureVoltage = payload[1] * 0.0196;
                                    DescriptionToInsert = "BATTERY TEMPERATURE SENSOR VOLTAGE";
                                    ValueToInsert = Math.Round(BatteryTemperatureVoltage, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0x23:
                                    double FlexFuelSensorVoltage = payload[1] * 0.0196;
                                    DescriptionToInsert = "FLEX FUEL SENSOR VOLTAGE";
                                    ValueToInsert = Math.Round(FlexFuelSensorVoltage, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0x24:
                                    double FlexFuelEthanolPercent = payload[1] * 0.5;
                                    DescriptionToInsert = "FLEX FUEL ETHANOL PERCENT";
                                    ValueToInsert = Math.Round(FlexFuelEthanolPercent, 1).ToString("0.0").Replace(",", ".");
                                    UnitToInsert = "PERCENT";
                                    break;
                                case 0x25:
                                    double ACHSPressureVoltage = payload[1] * 0.0196;
                                    DescriptionToInsert = "A/C HIGH-SIDE PRESSURE SENSOR VOLTAGE";
                                    ValueToInsert = Math.Round(ACHSPressureVoltage, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0x26:
                                    double ACHSPressurePSI = payload[1] * 1.961;
                                    double ACHSPressureKPA = ACHSPressurePSI * 6.894757;
                                    DescriptionToInsert = "A/C HIGH-SIDE PRESSURE";

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        ValueToInsert = Math.Round(ACHSPressurePSI, 1).ToString("0.0").Replace(",", ".");
                                        UnitToInsert = "PSI";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        ValueToInsert = Math.Round(ACHSPressureKPA, 1).ToString("0.0").Replace(",", ".");
                                        UnitToInsert = "KPA";
                                    }
                                    break;
                                case 0x27:
                                    DescriptionToInsert = "INJECTOR PULSE WIDTH 1";

                                    if (message.Length >= 5)
                                    {
                                        if (payload[2] == 0x28)
                                        {
                                            double InjectorPulseWidth1 = ((payload[1] << 8) + payload[3]) * 0.00390625;
                                            ValueToInsert = Math.Round(InjectorPulseWidth1, 3).ToString("0.000").Replace(",", ".");
                                                
                                        }
                                        else
                                        {
                                            DescriptionToInsert += " | ERROR: REQUEST F4 27 28";
                                        }
                                    }
                                    else
                                    {
                                        byte InjectorPulseWidth1 = payload[1];
                                        DescriptionToInsert += " | HIGH-RESOLUTION: F4 27 28";
                                        ValueToInsert = InjectorPulseWidth1.ToString("0");
                                    }

                                    UnitToInsert = "MS";
                                    break;
                                case 0x28:
                                    DescriptionToInsert = "INJECTOR PULSE WIDTH 1 | ERROR: REQUEST F4 27 28";
                                    break;
                                case 0x29:
                                    DescriptionToInsert = "INJECTOR PULSE WIDTH 2";

                                    if (message.Length >= 5)
                                    {
                                        if (payload[2] == 0x2A)
                                        {
                                            double InjectorPulseWidth2 = ((payload[1] << 8) + payload[3]) * 0.00390625;
                                            ValueToInsert = Math.Round(InjectorPulseWidth2, 3).ToString("0.000").Replace(",", ".");

                                        }
                                        else
                                        {
                                            DescriptionToInsert += " | ERROR: REQUEST F4 29 2A";
                                        }
                                    }
                                    else
                                    {
                                        byte InjectorPulseWidth2 = payload[1];
                                        DescriptionToInsert += " | HIGH-RESOLUTION: F4 29 2A";
                                        ValueToInsert = InjectorPulseWidth2.ToString("0");
                                    }

                                    UnitToInsert = "MS";
                                    break;
                                case 0x2A:
                                    DescriptionToInsert = "INJECTOR PULSE WIDTH 2 | ERROR: REQUEST F4 29 2A";
                                    break;
                                case 0x2B:
                                    double LTFT1 = payload[1] * 0.196;
                                    DescriptionToInsert = "LONG TERM FUEL TRIM 1";
                                    ValueToInsert = Math.Round(LTFT1, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "PERCENT";
                                    break;
                                case 0x2C:
                                    double LTFT2 = payload[1] * 0.196;
                                    DescriptionToInsert = "LONG TERM FUEL TRIM 2";
                                    ValueToInsert = Math.Round(LTFT2, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "PERCENT";
                                    break;
                                case 0x2D:
                                    double ECTC2 = payload[1] - 128;
                                    double ECTF2 = 1.8 * ECTC2 + 32.0;
                                    DescriptionToInsert = "ENGINE COOLANT TEMPERATURE 2";

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        ValueToInsert = Math.Round(ECTF2).ToString("0");
                                        UnitToInsert = "°F";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        ValueToInsert = ECTC2.ToString("0");
                                        UnitToInsert = "°C";
                                    }
                                    break;
                                case 0x2E:
                                    double ECTC3 = payload[1] - 128;
                                    double ECTF3 = 1.8 * ECTC3 + 32.0;
                                    DescriptionToInsert = "ENGINE COOLANT TEMPERATURE 3";

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        ValueToInsert = Math.Round(ECTF3).ToString("0");
                                        UnitToInsert = "°F";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        ValueToInsert = ECTC3.ToString("0");
                                        UnitToInsert = "°C";
                                    }
                                    break;
                                case 0x2F:
                                    double SparkAdvance = payload[1] * 0.5;
                                    DescriptionToInsert = "SPARK ADVANCE";
                                    ValueToInsert = Math.Round(SparkAdvance, 1).ToString("0.0").Replace(",", ".");
                                    UnitToInsert = "DEG";
                                    break;
                                case 0x30:
                                    double TotalKnockRetard = payload[1] * 0.5;
                                    DescriptionToInsert = "TOTAL KNOCK RETARD";
                                    ValueToInsert = Math.Round(TotalKnockRetard, 1).ToString("0.0").Replace(",", ".");
                                    UnitToInsert = "DEG";
                                    break;
                                case 0x31:
                                    double Cylinder1Retard = payload[1] * 0.5;
                                    DescriptionToInsert = "CYLINDER 1 RETARD";
                                    ValueToInsert = Math.Round(Cylinder1Retard, 1).ToString("0.0").Replace(",", ".");
                                    UnitToInsert = "DEG";
                                    break;
                                case 0x32:
                                    double Cylinder2Retard = payload[1] * 0.5;
                                    DescriptionToInsert = "CYLINDER 2 RETARD";
                                    ValueToInsert = Math.Round(Cylinder2Retard, 1).ToString("0.0").Replace(",", ".");
                                    UnitToInsert = "DEG";
                                    break;
                                case 0x33:
                                    double Cylinder3Retard = payload[1] * 0.5;
                                    DescriptionToInsert = "CYLINDER 3 RETARD";
                                    ValueToInsert = Math.Round(Cylinder3Retard, 1).ToString("0.0").Replace(",", ".");
                                    UnitToInsert = "DEG";
                                    break;
                                case 0x34:
                                    double Cylinder4Retard = payload[1] * 0.5;
                                    DescriptionToInsert = "CYLINDER 4 RETARD";
                                    ValueToInsert = Math.Round(Cylinder4Retard, 1).ToString("0.0").Replace(",", ".");
                                    UnitToInsert = "DEG";
                                    break;
                                case 0x35:
                                    DescriptionToInsert = "TARGET IDLE SPEED";

                                    if (message.Length >= 5)
                                    {
                                        if (payload[2] == 0x36)
                                        {
                                            double IdleSpeed = ((payload[1] << 8) + payload[3]) * 0.125;
                                            ValueToInsert = Math.Round(IdleSpeed, 3).ToString("0.000").Replace(",", ".");
                                            UnitToInsert = "RPM";
                                        }
                                        else
                                        {
                                            DescriptionToInsert += " | ERROR: REQUEST F4 35 36";
                                        }
                                    }
                                    else
                                    {
                                        DescriptionToInsert += " | HIGH-RESOLUTION: F4 35 36";
                                        double IdleSpeed = payload[1] * 32.0;
                                        ValueToInsert = IdleSpeed.ToString("0");
                                        UnitToInsert = "RPM";
                                    }
                                    break;
                                case 0x36:
                                    DescriptionToInsert = "TARGET IDLE SPEED | ERROR: REQUEST F4 35 36";
                                    break;
                                case 0x37:
                                    DescriptionToInsert = "TARGET IDLE AIR CONTROL MOTOR STEPS";
                                    ValueToInsert = payload[1].ToString();
                                    break;
                                case 0x3A:
                                    double ChargingVoltage = payload[1] * 0.0625;
                                    DescriptionToInsert = "CHARGING VOLTAGE";
                                    ValueToInsert = Math.Round(ChargingVoltage, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0x3B:
                                    double CruiseSetSpeedMPH = payload[1] * 0.5;
                                    double CruiseSetSpeedKMH = CruiseSetSpeedMPH * 1.609344;
                                    DescriptionToInsert = "CRUISE | SET SPEED";

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        ValueToInsert = Math.Round(CruiseSetSpeedMPH, 1).ToString("0.0").Replace(",", ".");
                                        UnitToInsert = "MPH";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        ValueToInsert = Math.Round(CruiseSetSpeedKMH, 1).ToString("0.0").Replace(",", ".");
                                        UnitToInsert = "KM/H";
                                    }
                                    break;
                                case 0x3C:
                                    DescriptionToInsert = "BIT STATE 5";
                                    ValueToInsert = Convert.ToString(payload[1], 2).PadLeft(8, '0');
                                    break;
                                case 0x3D:
                                    DescriptionToInsert = "BIT STATE 6";
                                    ValueToInsert = Convert.ToString(payload[1], 2).PadLeft(8, '0');
                                    break;
                                case 0x3E:
                                    DescriptionToInsert = "IDLE AIR CONTROL MOTOR STEPS";
                                    ValueToInsert = payload[1].ToString();
                                    break;
                                case 0x3F:
                                case 0xED:
                                    string LastCruiseCutoutReasonD;
                                    string CruiseDeniedReasonD;

                                    switch (payload[1] & 0xF0) // upper 4 bits encode last cutout reason 
                                    {
                                        case 0x00:
                                            LastCruiseCutoutReasonD = "ON/OFF SW";
                                            break;
                                        case 0x10:
                                            LastCruiseCutoutReasonD = "SPEED SEN";
                                            break;
                                        case 0x20:
                                            LastCruiseCutoutReasonD = "RPM LIMIT";
                                            break;
                                        case 0x30:
                                            LastCruiseCutoutReasonD = "BRAKE SW";
                                            break;
                                        case 0x40:
                                            LastCruiseCutoutReasonD = "P/N SW";
                                            break;
                                        case 0x50:
                                            LastCruiseCutoutReasonD = "RPM/SPEED";
                                            break;
                                        case 0x60:
                                            LastCruiseCutoutReasonD = "CLUTCH";
                                            break;
                                        case 0x70:
                                            LastCruiseCutoutReasonD = "S/C DTC";
                                            break;
                                        case 0x80:
                                            LastCruiseCutoutReasonD = "KEY OFF";
                                            break;
                                        case 0x90:
                                            LastCruiseCutoutReasonD = "ACTIVE";
                                            break;
                                        case 0xA0:
                                            LastCruiseCutoutReasonD = "CLUTCH UP";
                                            break;
                                        case 0xB0:
                                            LastCruiseCutoutReasonD = "N/A";
                                            break;
                                        case 0xC0:
                                            LastCruiseCutoutReasonD = "SW DTC";
                                            break;
                                        case 0xD0:
                                            LastCruiseCutoutReasonD = "CANCEL";
                                            break;
                                        case 0xE0:
                                            LastCruiseCutoutReasonD = "LIMP-IN";
                                            break;
                                        case 0xF0:
                                            LastCruiseCutoutReasonD = "12V DTC";
                                            break;
                                        default:
                                            LastCruiseCutoutReasonD = "N/A";
                                            break;
                                    }

                                    switch (payload[1] & 0x0F) // lower 4 bits encode denied reason 
                                    {
                                        case 0x00:
                                            CruiseDeniedReasonD = "ON/OFF SW";
                                            break;
                                        case 0x01:
                                            CruiseDeniedReasonD = "SPEED SEN";
                                            break;
                                        case 0x02:
                                            CruiseDeniedReasonD = "RPM LIMIT";
                                            break;
                                        case 0x03:
                                            CruiseDeniedReasonD = "BRAKE SW";
                                            break;
                                        case 0x04:
                                            CruiseDeniedReasonD = "P/N SW";
                                            break;
                                        case 0x05:
                                            CruiseDeniedReasonD = "RPM/SPEED";
                                            break;
                                        case 0x06:
                                            CruiseDeniedReasonD = "CLUTCH";
                                            break;
                                        case 0x07:
                                            CruiseDeniedReasonD = "S/C DTC";
                                            break;
                                        case 0x08:
                                            CruiseDeniedReasonD = "ALLOWED";
                                            break;
                                        case 0x09:
                                            CruiseDeniedReasonD = "ACTIVE";
                                            break;
                                        case 0x0A:
                                            CruiseDeniedReasonD = "CLUTCH UP";
                                            break;
                                        case 0x0B:
                                            CruiseDeniedReasonD = "N/A";
                                            break;
                                        case 0x0C:
                                            CruiseDeniedReasonD = "SW DTC";
                                            break;
                                        case 0x0D:
                                            CruiseDeniedReasonD = "CANCEL";
                                            break;
                                        case 0x0E:
                                            CruiseDeniedReasonD = "LIMP-IN";
                                            break;
                                        case 0x0F:
                                            CruiseDeniedReasonD = "12V DTC";
                                            break;
                                        default:
                                            CruiseDeniedReasonD = "N/A";
                                            break;
                                    }

                                    if ((payload[1] & 0x0F) == 0x08)
                                    {
                                        DescriptionToInsert = "CRUISE | LAST CUTOUT: " + LastCruiseCutoutReasonD + " | STATE: " + CruiseDeniedReasonD;
                                        ValueToInsert = "STOPPED";
                                    }
                                    else if ((payload[1] & 0x0F) == 0x09)
                                    {
                                        DescriptionToInsert = "CRUISE | LAST CUTOUT: " + LastCruiseCutoutReasonD + " | STATE: " + CruiseDeniedReasonD;
                                        ValueToInsert = "ENGAGED";
                                    }
                                    else
                                    {
                                        DescriptionToInsert = "CRUISE | LAST CUTOUT: " + LastCruiseCutoutReasonD + " | DENIED: " + CruiseDeniedReasonD;
                                        ValueToInsert = "STOPPED";
                                    }
                                    break;
                                case 0x40:
                                    DescriptionToInsert = "VEHICLE THEFT ALARM STATUS";

                                    if (Util.IsBitSet(payload[1], 5))
                                    {
                                        ValueToInsert = "KILL FUEL";
                                    }
                                    else
                                    {
                                        ValueToInsert = "FUEL ON";
                                    }
                                    break;
                                case 0x41:
                                    DescriptionToInsert = "CRANKSHAFT/CAMSHAFT POSITION SENSOR SYNC STATE";

                                    if (Util.IsBitSet(payload[1], 5)) DescriptionToInsert = "CKP: PRESENT | ";
                                    else DescriptionToInsert = "CKP: LOST | ";

                                    if (Util.IsBitSet(payload[1], 6)) DescriptionToInsert += "CMP: PRESENT | ";
                                    else DescriptionToInsert += "CMP: LOST | ";

                                    if (Util.IsBitSet(payload[1], 4)) DescriptionToInsert += "CKP/CMP: IN-SYNC";
                                    else DescriptionToInsert += "CKP/CMP: OUT-OF-SYNC";

                                    if (Util.IsBitSet(payload[1], 0)) ValueToInsert = "HISTORY: IN-SYNC";
                                    else ValueToInsert = "HISTORY: OUT-OF-SYNC";

                                    break;
                                case 0x42:
                                    DescriptionToInsert = "FUEL SYSTEM STATUS 1";

                                    if (Util.IsBitSet(payload[1], 0)) ValueToInsert = "OPEN LOOP";
                                    if (Util.IsBitSet(payload[1], 1)) ValueToInsert = "CLOSED LOOP";
                                    if (Util.IsBitSet(payload[1], 2)) ValueToInsert = "OPEN LOOP / DRIVE";
                                    if (Util.IsBitSet(payload[1], 3)) ValueToInsert = "OPEN LOOP / DTC";
                                    if (Util.IsBitSet(payload[1], 4)) ValueToInsert = "CLOSED LOOP / DTC";

                                    break;
                                case 0x43:
                                    DescriptionToInsert = "CURRENT ADAPTIVE CELL ID";
                                    ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                                    break;
                                case 0x44:
                                    double STFT1 = payload[1] * 0.196;
                                    DescriptionToInsert = "SHORT TERM FUEL TRIM 1";
                                    ValueToInsert = Math.Round(STFT1, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "PERCENT";
                                    break;
                                case 0x45:
                                    double STFT2 = payload[1] * 0.196;
                                    DescriptionToInsert = "SHORT TERM FUEL TRIM 2";
                                    ValueToInsert = Math.Round(STFT2, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "PERCENT";
                                    break;
                                case 0x46:
                                    DescriptionToInsert = "EMISSION SETTINGS 1";
                                    ValueToInsert = Convert.ToString(payload[1], 2).PadLeft(8, '0');
                                    break;
                                case 0x47:
                                    DescriptionToInsert = "EMISSION SETTINGS 2";
                                    ValueToInsert = Convert.ToString(payload[1], 2).PadLeft(8, '0');
                                    break;
                                case 0x48:
                                    double O2S12Voltage = payload[1] * 0.0196;
                                    DescriptionToInsert = "DOWNSTREAM O2 1/2 SENSOR VOLTAGE";
                                    ValueToInsert = Math.Round(O2S12Voltage, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0x49:
                                    double O2S22Voltage = payload[1] * 0.0196;
                                    DescriptionToInsert = "DOWNSTREAM O2 2/2 SENSOR VOLTAGE";
                                    ValueToInsert = Math.Round(O2S22Voltage, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0x4A:
                                    double ClosedLoopTimer = payload[1] * 0.0535;
                                    DescriptionToInsert = "CLOSED LOOP TIMER";
                                    ValueToInsert = Math.Round(ClosedLoopTimer, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "MINUTES";
                                    break;
                                case 0x4B:
                                    DescriptionToInsert = "TIME FROM START/RUN";

                                    if (message.Length >= 5)
                                    {
                                        if (payload[2] == 0x4C)
                                        {
                                            double TimeFromStartRun = ((payload[1] << 8) + payload[3]) * 0.000208984375;
                                            ValueToInsert = Math.Round(TimeFromStartRun, 3).ToString("0.000").Replace(",", ".");
                                            UnitToInsert = "MINUTES";
                                        }
                                        else
                                        {
                                            DescriptionToInsert += " | ERROR: REQUEST F4 4B 4C";
                                        }
                                    }
                                    else
                                    {
                                        DescriptionToInsert += " | HIGH-RESOLUTION: F4 4B 4C";
                                        double TimeFromStartRun = payload[1] * 0.0535;
                                        ValueToInsert = Math.Round(TimeFromStartRun, 3).ToString("0");
                                        UnitToInsert = "MINUTES";
                                    }
                                    break;
                                case 0x4C:
                                    DescriptionToInsert = "TIME FROM START/RUN | ERROR: REQUEST F4 4B 4C";
                                    break;
                                case 0x4D:
                                    double RuntimeAtStall = payload[1] * 0.0535;
                                    DescriptionToInsert = "RUNTIME AT STALL";
                                    ValueToInsert = Math.Round(RuntimeAtStall, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "MINUTES";
                                    break;
                                case 0x4E:
                                    DescriptionToInsert = "CURRENT FUEL SHUTOFF";

                                    byte CFS = (byte)(payload[1] & 0xF0); // keep upper 4 bits

                                    if (CFS == 0)
                                    {
                                        ValueToInsert = "NONE";
                                    }
                                    else
                                    {
                                        if (Util.IsBitSet(CFS, 4)) ValueToInsert = "IN DECEL";
                                        else if (Util.IsBitSet(CFS, 5)) ValueToInsert = "TORQUE MGMT";
                                        else if (Util.IsBitSet(CFS, 6)) ValueToInsert = "REV LIMITER";
                                        else if (Util.IsBitSet(CFS, 7)) ValueToInsert = "ABOVE 112 MPH";
                                    }
                                    break;
                                case 0x4F:
                                    DescriptionToInsert = "HISTORY OF FUEL SHUTOFF";

                                    byte HCFS = (byte)(payload[1] & 0xF0); // keep upper 4 bits

                                    if (HCFS == 0)
                                    {
                                        ValueToInsert = "NONE";
                                    }
                                    else
                                    {
                                        if (Util.IsBitSet(HCFS, 4)) ValueToInsert = "IN DECEL";
                                        else if (Util.IsBitSet(HCFS, 5)) ValueToInsert = "TORQUE MGMT";
                                        else if (Util.IsBitSet(HCFS, 6)) ValueToInsert = "REV LIMITER";
                                        else if (Util.IsBitSet(HCFS, 7)) ValueToInsert = "ABOVE 112 MPH";
                                    }
                                    break;
                                case 0x51:
                                    DescriptionToInsert = "ADAPTIVE NUMERATOR 1";
                                    ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                                    break;
                                case 0x57:
                                    DescriptionToInsert = "RPM/VSS RATIO";
                                    ValueToInsert = payload[1].ToString("0");
                                    break;
                                case 0x58:
                                    DescriptionToInsert = "TRANSMISSION SELECTED GEAR 2";
                                    ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                                    break;
                                case 0x5A:
                                    double DwellCoil1 = payload[1] * 0.008;
                                    DescriptionToInsert = "DWELL COIL 1 (CYL1_4)";
                                    ValueToInsert = Math.Round(DwellCoil1, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "MS";
                                    break;
                                case 0x5B:
                                    double DwellCoil2 = payload[1] * 0.008;
                                    DescriptionToInsert = "DWELL COIL 2 (CYL2_3)";
                                    ValueToInsert = Math.Round(DwellCoil2, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "MS";
                                    break;
                                case 0x5C:
                                    double DwellCoil3 = payload[1] * 0.008;
                                    DescriptionToInsert = "DWELL COIL 3 (CYL3_6)";
                                    ValueToInsert = Math.Round(DwellCoil3, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "MS";
                                    break;
                                case 0x5D:
                                    double FanDutyCycle = payload[1] * 0.3922;
                                    DescriptionToInsert = "FAN DUTY CYCLE";
                                    ValueToInsert = Math.Round(FanDutyCycle, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "PERCENT";
                                    break;
                                case 0x60:
                                    DescriptionToInsert = "A/C RELAY STATE";
                                    ValueToInsert = Convert.ToString(payload[1], 2).PadLeft(8, '0');
                                    break;
                                case 0x73:
                                    List<string> LimpInStates = new List<string>();
                                    LimpInStates.Clear();

                                    if (Util.IsBitSet(payload[1], 0)) LimpInStates.Add("ATS"); // Ambient temperature sensor
                                    if (Util.IsBitSet(payload[1], 3)) LimpInStates.Add("IAT"); // Intake air temperature
                                    if (Util.IsBitSet(payload[1], 4)) LimpInStates.Add("TPS"); // Throttle position sensor
                                    if (Util.IsBitSet(payload[1], 5)) LimpInStates.Add("MAPEL"); // Intake manifold absolute pressure
                                    if (Util.IsBitSet(payload[1], 6)) LimpInStates.Add("MAPVA"); // Intake manifold vacuum
                                    if (Util.IsBitSet(payload[1], 7)) LimpInStates.Add("ECT"); // Engine coolant temperature

                                    if (LimpInStates.Count > 0)
                                    {
                                        DescriptionToInsert = "LIMP-IN: ";

                                        foreach (string s in LimpInStates)
                                        {
                                            DescriptionToInsert += s + " | ";
                                        }

                                        if (DescriptionToInsert.Length > 2) DescriptionToInsert = DescriptionToInsert.Remove(DescriptionToInsert.Length - 3); // remove last "|" character
                                    }
                                    else
                                    {
                                        DescriptionToInsert = "NO LIMP-IN STATE";
                                    }
                                    break;
                                case 0x74:
                                    DescriptionToInsert = "DTC 2: ";

                                    if (payload[1] == 0) ValueToInsert = "EMPTY DTC SLOT";
                                    else
                                    {
                                        int index = EngineDTC.Rows.IndexOf(EngineDTC.Rows.Find(payload[1]));

                                        if (index > -1) // DTC description found
                                        {
                                            DescriptionToInsert += EngineDTC.Rows[index]["description"];
                                        }
                                        else // no DTC description found
                                        {
                                            DescriptionToInsert += "UNRECOGNIZED DTC";
                                        }
                                    }
                                    break;
                                case 0x75:
                                    DescriptionToInsert = "DTC 3: ";

                                    if (payload[1] == 0) ValueToInsert = "EMPTY DTC SLOT";
                                    else
                                    {
                                        int index = EngineDTC.Rows.IndexOf(EngineDTC.Rows.Find(payload[1]));

                                        if (index > -1) // DTC description found
                                        {
                                            DescriptionToInsert += EngineDTC.Rows[index]["description"];
                                        }
                                        else // no DTC description found
                                        {
                                            DescriptionToInsert += "UNRECOGNIZED DTC";
                                        }
                                    }
                                    break;
                                case 0x76:
                                    DescriptionToInsert = "DTC 4: ";

                                    if (payload[1] == 0) ValueToInsert = "EMPTY DTC SLOT";
                                    else
                                    {
                                        int index = EngineDTC.Rows.IndexOf(EngineDTC.Rows.Find(payload[1]));

                                        if (index > -1) // DTC description found
                                        {
                                            DescriptionToInsert += EngineDTC.Rows[index]["description"];
                                        }
                                        else // no DTC description found
                                        {
                                            DescriptionToInsert += "UNRECOGNIZED DTC";
                                        }
                                    }
                                    break;
                                case 0x77:
                                    DescriptionToInsert = "DTC 5: ";

                                    if (payload[1] == 0) ValueToInsert = "EMPTY DTC SLOT";
                                    else
                                    {
                                        int index = EngineDTC.Rows.IndexOf(EngineDTC.Rows.Find(payload[1]));

                                        if (index > -1) // DTC description found
                                        {
                                            DescriptionToInsert += EngineDTC.Rows[index]["description"];
                                        }
                                        else // no DTC description found
                                        {
                                            DescriptionToInsert += "UNRECOGNIZED DTC";
                                        }
                                    }
                                    break;
                                case 0x78:
                                    DescriptionToInsert = "DTC 6: ";

                                    if (payload[1] == 0) ValueToInsert = "EMPTY DTC SLOT";
                                    else
                                    {
                                        int index = EngineDTC.Rows.IndexOf(EngineDTC.Rows.Find(payload[1]));

                                        if (index > -1) // DTC description found
                                        {
                                            DescriptionToInsert += EngineDTC.Rows[index]["description"];
                                        }
                                        else // no DTC description found
                                        {
                                            DescriptionToInsert += "UNRECOGNIZED DTC";
                                        }
                                    }
                                    break;
                                case 0x79:
                                    DescriptionToInsert = "DTC 7: ";

                                    if (payload[1] == 0) ValueToInsert = "EMPTY DTC SLOT";
                                    else
                                    {
                                        int index = EngineDTC.Rows.IndexOf(EngineDTC.Rows.Find(payload[1]));

                                        if (index > -1) // DTC description found
                                        {
                                            DescriptionToInsert += EngineDTC.Rows[index]["description"];
                                        }
                                        else // no DTC description found
                                        {
                                            DescriptionToInsert += "UNRECOGNIZED DTC";
                                        }
                                    }
                                    break;
                                case 0x8E:
                                    DescriptionToInsert = "BIT STATE 7";
                                    ValueToInsert = Convert.ToString(payload[1], 2).PadLeft(8, '0');
                                    break;
                                case 0x94:
                                    DescriptionToInsert = "EGR ZREF UPDATE D.C.";
                                    ValueToInsert = payload[1].ToString("0");
                                    break;
                                case 0x95:
                                    double EGRPosVoltage = payload[1] * 0.0196;
                                    DescriptionToInsert = "EGR POSITION SENSOR VOLTAGE";
                                    ValueToInsert = Math.Round(EGRPosVoltage, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0x96:
                                    double ActualPurgeCurrent = payload[1] * 0.0196;
                                    DescriptionToInsert = "ACTUAL PURGE CURRENT";
                                    ValueToInsert = Math.Round(ActualPurgeCurrent, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "A";
                                    break;
                                case 0x98:
                                    DescriptionToInsert = "TPS INTERMITTENT COUNTER";
                                    ValueToInsert = payload[1].ToString("0");
                                    break;
                                case 0x9B:
                                    double TransmissionTemperatureC = payload[1] - 64;
                                    double TransmissionTemperatureF = 1.8 * TransmissionTemperatureC + 32.0;

                                    DescriptionToInsert = "TRANSMISSION TEMPERATURE";

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        ValueToInsert = Math.Round(TransmissionTemperatureF).ToString("0");
                                        UnitToInsert = "°F";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        ValueToInsert = Math.Round(TransmissionTemperatureC).ToString("0");
                                        UnitToInsert = "°C";
                                    }
                                    break;
                                case 0xA3:
                                    double CamTimingPosition = payload[1] * 0.5;
                                    DescriptionToInsert = "CAM TIMING POSITION";
                                    ValueToInsert = Math.Round(CamTimingPosition, 1).ToString("0.0").Replace(",", ".");
                                    UnitToInsert = "DEG";
                                    break;
                                case 0xA4:
                                    DescriptionToInsert = "ENGINE GOOD TRIP COUNTER";
                                    ValueToInsert = payload[1].ToString("0");
                                    break;
                                case 0xA5:
                                    DescriptionToInsert = "ENGINE WARM-UP CYCLE COUNTER";
                                    ValueToInsert = payload[1].ToString("0");
                                    break;
                                case 0xA6:
                                    DescriptionToInsert = "OBD2 MONITOR TEST RESULTS 1";
                                    ValueToInsert = Convert.ToString(payload[1], 2).PadLeft(8, '0');
                                    break;
                                case 0xA9:
                                    DescriptionToInsert = "FUEL SYSTEM STATUS 1";

                                    if (Util.IsBitSet(payload[1], 0)) ValueToInsert = "OPEN LOOP";
                                    if (Util.IsBitSet(payload[1], 1)) ValueToInsert = "CLOSED LOOP";
                                    if (Util.IsBitSet(payload[1], 2)) ValueToInsert = "OPEN LOOP / DRIVE";
                                    if (Util.IsBitSet(payload[1], 3)) ValueToInsert = "OPEN LOOP / DTC";
                                    if (Util.IsBitSet(payload[1], 4)) ValueToInsert = "CLOSED LOOP / DTC";
                                    break;
                                case 0xAA:
                                    DescriptionToInsert = "FUEL SYSTEM STATUS 2";

                                    if (Util.IsBitSet(payload[1], 0)) ValueToInsert = "OPEN LOOP";
                                    if (Util.IsBitSet(payload[1], 1)) ValueToInsert = "CLOSED LOOP";
                                    if (Util.IsBitSet(payload[1], 2)) ValueToInsert = "OPEN LOOP / DRIVE";
                                    if (Util.IsBitSet(payload[1], 3)) ValueToInsert = "OPEN LOOP / DTC";
                                    if (Util.IsBitSet(payload[1], 4)) ValueToInsert = "CLOSED LOOP / DTC";
                                    break;
                                case 0xAB:
                                    double EngineLoad = payload[1] * 0.3922;
                                    DescriptionToInsert = "ENGINE LOAD";
                                    ValueToInsert = Math.Round(EngineLoad, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "PERCENT";
                                    break;
                                case 0xAD:
                                    double STFT1A = payload[1] * 0.196;
                                    DescriptionToInsert = "SHORT TERM FUEL TRIM 1 A";
                                    ValueToInsert = Math.Round(STFT1A, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "PERCENT";
                                    break;
                                case 0xAE:
                                    double LTFT1A = payload[1] * 0.196;
                                    DescriptionToInsert = "LONG TERM FUEL TRIM 1 A";
                                    ValueToInsert = Math.Round(LTFT1A, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "PERCENT";
                                    break;
                                case 0xAF:
                                    double STFT2A = payload[1] * 0.196;
                                    DescriptionToInsert = "SHORT TERM FUEL TRIM 2 A";
                                    ValueToInsert = Math.Round(STFT2A, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "PERCENT";
                                    break;
                                case 0xB0:
                                    double LTFT2A = payload[1] * 0.196;
                                    DescriptionToInsert = "LONG TERM FUEL TRIM 2 A";
                                    ValueToInsert = Math.Round(LTFT2A, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "PERCENT";
                                    break;
                                case 0xB1:
                                    double MAP6PSI = payload[1] * 0.059756;
                                    double MAP6KPA = payload[1] * 0.059756 * 6.894757;
                                    DescriptionToInsert = "INTAKE MANIFOLD ABSOLUTE PRESSURE (MAP)";

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        ValueToInsert = Math.Round(MAP6PSI, 1).ToString("0.0").Replace(",", ".");
                                        UnitToInsert = "PSI";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        ValueToInsert = Math.Round(MAP6KPA, 1).ToString("0.0").Replace(",", ".");
                                        UnitToInsert = "KPA";
                                    }
                                    break;
                                case 0xB2:
                                    double EngineSpeed2 = payload[1] * 32.0;
                                    DescriptionToInsert = "ENGINE SPEED";
                                    ValueToInsert = EngineSpeed2.ToString("0");
                                    UnitToInsert = "RPM";
                                    break;
                                case 0xB3:
                                    double VehicleSpeed2MPH = payload[1] * 0.5;
                                    double VehicleSpeed2KMH = VehicleSpeed2MPH * 1.609344;
                                    DescriptionToInsert = "VEHICLE SPEED";

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        ValueToInsert = Math.Round(VehicleSpeed2MPH).ToString("0");
                                        UnitToInsert = "MPH";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        ValueToInsert = Math.Round(VehicleSpeed2KMH).ToString("0");
                                        UnitToInsert = "KM/H";
                                    }
                                    break;
                                case 0xB4:
                                    double MAPVacuum2PSI = payload[1] * 0.059756;
                                    double MAPVacuum2KPA = MAPVacuum2PSI * 6.894757;
                                    DescriptionToInsert = "MAP VACUUM";

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        ValueToInsert = Math.Round(MAPVacuum2PSI, 1).ToString("0.0").Replace(",", ".");
                                        UnitToInsert = "PSI";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        ValueToInsert = Math.Round(MAPVacuum2KPA, 1).ToString("0.0").Replace(",", ".");
                                        UnitToInsert = "KPA";
                                    }
                                    break;
                                case 0xB5:
                                    DescriptionToInsert = "DTC E"; // last emission related DTC stored

                                    if (payload[1] == 0) ValueToInsert = "EMPTY DTC SLOT";
                                    else
                                    {
                                        int index = EngineDTC.Rows.IndexOf(EngineDTC.Rows.Find(payload[1]));

                                        if (index > -1) // DTC description found
                                        {
                                            DescriptionToInsert += ": " + EngineDTC.Rows[index]["description"];
                                        }
                                        else // no DTC description found
                                        {
                                            DescriptionToInsert += ": UNRECOGNIZED DTC";
                                        }
                                    }
                                    break;
                                case 0xB6:
                                    double CTSVoltage = payload[1] * 0.0196;
                                    DescriptionToInsert = "CATALYST TEMPERATURE SENSOR VOLTAGE";
                                    ValueToInsert = Math.Round(CTSVoltage, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0xB7:
                                    double GovPDC = payload[1] * 0.3922;
                                    DescriptionToInsert = "PURGE DUTY CYCLE";
                                    ValueToInsert = Math.Round(GovPDC, 1).ToString("0.0").Replace(",", ".");
                                    UnitToInsert = "PERCENT";
                                    break;
                                case 0xC0:
                                    double CatalystTemperatureC = payload[1] - 128;
                                    double CatalystTemperatureF = 1.8 * CatalystTemperatureC + 32.0;
                                    DescriptionToInsert = "CATALYST TEMPERATURE";

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        ValueToInsert = Math.Round(CatalystTemperatureF).ToString("0");
                                        UnitToInsert = "°F";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        ValueToInsert = CatalystTemperatureC.ToString("0");
                                        UnitToInsert = "°C";
                                    }
                                    break;
                                case 0xC1:
                                    DescriptionToInsert = "FUEL LEVEL STATUS 1";
                                    ValueToInsert = Convert.ToString(payload[1], 2).PadLeft(8, '0');
                                    break;
                                case 0xC2:
                                    double FLS3Voltage = payload[1] * 0.0196;
                                    DescriptionToInsert = "FUEL LEVEL SENSOR VOLTAGE 3";
                                    ValueToInsert = Math.Round(FLS3Voltage, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0xDA:
                                    DescriptionToInsert = "P_PCM_NOC_STRDCAMTIME";
                                    ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                                    break;
                                case 0xDB:
                                    double FLS2Voltage = payload[1] * 0.0196;
                                    DescriptionToInsert = "FUEL LEVEL SENSOR VOLTAGE 2";
                                    ValueToInsert = Math.Round(FLS2Voltage, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0xDE:
                                    double FLS1Voltage = payload[1] * 0.0196;
                                    DescriptionToInsert = "FUEL LEVEL SENSOR VOLTAGE 1";
                                    ValueToInsert = Math.Round(FLS1Voltage, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0xDF:
                                    double FuelLevelG = payload[1] * 0.125;
                                    double FuelLevelL = FuelLevelG * 3.785412;
                                    DescriptionToInsert = "FUEL LEVEL";

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        ValueToInsert = Math.Round(FuelLevelG, 1).ToString("0.0").Replace(",", ".");
                                        UnitToInsert = "GALLON";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        ValueToInsert = Math.Round(FuelLevelL, 1).ToString("0.0").Replace(",", ".");
                                        UnitToInsert = "LITER";
                                    }
                                    break;
                                case 0xE3:
                                    double CalculatedEngineLoad = payload[1] * 0.3922;
                                    DescriptionToInsert = "CALCULATED ENGINE LOAD";
                                    ValueToInsert = Math.Round(CalculatedEngineLoad, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "PERCENT";
                                    break;
                                case 0xE4:
                                    DescriptionToInsert = "OBD2 MONITOR TEST RESULTS 2";
                                    ValueToInsert = Convert.ToString(payload[1], 2).PadLeft(8, '0');
                                    break;
                                case 0xE7:
                                    DescriptionToInsert = "CELL #1 - IDLE CELL";
                                    ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                                    break;
                                case 0xE8:
                                    DescriptionToInsert = "CELL #2 - 1ST OFF IDLE CELL";
                                    ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                                    break;
                                case 0xE9:
                                    DescriptionToInsert = "CELL #3 - 2ND OFF IDLE CELL";
                                    ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                                    break;
                                case 0xEC:
                                    DescriptionToInsert = "FUEL SYSTEM STATUS 2";

                                    if (Util.IsBitSet(payload[1], 0)) ValueToInsert = "OPEN LOOP";
                                    if (Util.IsBitSet(payload[1], 1)) ValueToInsert = "CLOSED LOOP";
                                    if (Util.IsBitSet(payload[1], 2)) ValueToInsert = "OPEN LOOP / DRIVE";
                                    if (Util.IsBitSet(payload[1], 3)) ValueToInsert = "OPEN LOOP / DTC";
                                    if (Util.IsBitSet(payload[1], 4)) ValueToInsert = "CLOSED LOOP / DTC";

                                    break;
                                case 0xEE:
                                    DescriptionToInsert = "CRUISE | OPERATING MODE";

                                    switch (payload[1] & 0x0F)
                                    {
                                        case 0x08:
                                            ValueToInsert = "DISENGAGED";
                                            break;
                                        case 0x09:
                                            ValueToInsert = "NORMAL";
                                            break;
                                        case 0x0A:
                                            ValueToInsert = "ACCELERATING";
                                            break;
                                        case 0x0B:
                                            ValueToInsert = "DECELERATING";
                                            break;
                                        default:
                                            ValueToInsert = "N/A";
                                            break;
                                    }
                                    break;
                                case 0xEF:
                                    double TPSCalculatedVoltage = payload[1] * 0.0196;
                                    DescriptionToInsert = "CALCULATED TPS VOLTAGE";
                                    ValueToInsert = Math.Round(TPSCalculatedVoltage, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
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

                                    DescriptionToInsert = "F4 RAM TABLE | OFFSET: " + Util.ByteToHexStringSimple(offsets.ToArray());
                                    ValueToInsert = Util.ByteToHexStringSimple(values.ToArray());
                                    break;
                            }
                        }
                        break;
                    case 0xF5:
                        DescriptionToInsert = "F5 RAM TABLE SELECTED";

                        if (message.Length >= 3)
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

                            DescriptionToInsert = "F5 RAM TABLE | OFFSET: " + Util.ByteToHexStringSimple(offsets.ToArray());
                            ValueToInsert = Util.ByteToHexStringSimple(values.ToArray());
                        }
                        break;
                    case 0xF6:
                        DescriptionToInsert = "F6 RAM TABLE SELECTED";

                        if (message.Length >= 3)
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

                            DescriptionToInsert = "F6 RAM TABLE | OFFSET: " + Util.ByteToHexStringSimple(offsets.ToArray());
                            ValueToInsert = Util.ByteToHexStringSimple(values.ToArray());
                        }
                        break;
                    case 0xF7:
                        DescriptionToInsert = "F7 RAM TABLE SELECTED";

                        if (message.Length >= 3)
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

                            DescriptionToInsert = "F7 RAM TABLE | OFFSET: " + Util.ByteToHexStringSimple(offsets.ToArray());
                            ValueToInsert = Util.ByteToHexStringSimple(values.ToArray());
                        }
                        break;
                    case 0xF8:
                        DescriptionToInsert = "F8 RAM TABLE SELECTED";

                        if (message.Length >= 3)
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

                            DescriptionToInsert = "F8 RAM TABLE | OFFSET: " + Util.ByteToHexStringSimple(offsets.ToArray());
                            ValueToInsert = Util.ByteToHexStringSimple(values.ToArray());
                        }
                        break;
                    case 0xF9:
                        DescriptionToInsert = "F9 RAM TABLE SELECTED";

                        if (message.Length >= 3)
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

                            DescriptionToInsert = "F9 RAM TABLE | OFFSET: " + Util.ByteToHexStringSimple(offsets.ToArray());
                            ValueToInsert = Util.ByteToHexStringSimple(values.ToArray());
                        }
                        break;
                    case 0xFA:
                        DescriptionToInsert = "FA RAM TABLE SELECTED";

                        if (message.Length >= 3)
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

                            DescriptionToInsert = "FA RAM TABLE | OFFSET: " + Util.ByteToHexStringSimple(offsets.ToArray());
                            ValueToInsert = Util.ByteToHexStringSimple(values.ToArray());
                        }
                        break;
                    case 0xFB:
                        DescriptionToInsert = "FB RAM TABLE SELECTED";

                        if (message.Length >= 3)
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

                            DescriptionToInsert = "FB RAM TABLE | OFFSET: " + Util.ByteToHexStringSimple(offsets.ToArray());
                            ValueToInsert = Util.ByteToHexStringSimple(values.ToArray());
                        }
                        break;
                    case 0xFC:
                        DescriptionToInsert = "FC RAM TABLE SELECTED";

                        if (message.Length >= 3)
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

                            DescriptionToInsert = "FC RAM TABLE | OFFSET: " + Util.ByteToHexStringSimple(offsets.ToArray());
                            ValueToInsert = Util.ByteToHexStringSimple(values.ToArray());
                        }
                        break;
                    case 0xFD:
                        DescriptionToInsert = "FD RAM TABLE SELECTED";

                        if (message.Length >= 3)
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

                            DescriptionToInsert = "FD RAM TABLE | OFFSET: " + Util.ByteToHexStringSimple(offsets.ToArray());
                            ValueToInsert = Util.ByteToHexStringSimple(values.ToArray());
                        }
                        break;
                    case 0xFE:
                        DescriptionToInsert = "SELECT LOW-SPEED MODE";
                        break;
                    case 0xFF:
                        DescriptionToInsert = "PCM WAKE UP";
                        break;
                    default:
                        DescriptionToInsert = string.Empty;
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

            if (DescriptionToInsert.Length > 51) DescriptionToInsert = Util.TruncateString(DescriptionToInsert, 48) + "...";
            if (ValueToInsert.Length > 23) ValueToInsert = Util.TruncateString(ValueToInsert, 20) + "...";
            if (UnitToInsert.Length > 11) UnitToInsert = Util.TruncateString(UnitToInsert, 8) + "...";

            StringBuilder rowToAdd = new StringBuilder(EmptyLine); // add empty line first

            rowToAdd.Remove(HexBytesColumnStart, hexBytesToInsert.Length);
            rowToAdd.Insert(HexBytesColumnStart, hexBytesToInsert);

            rowToAdd.Remove(DescriptionColumnStart, DescriptionToInsert.Length);
            rowToAdd.Insert(DescriptionColumnStart, DescriptionToInsert);

            rowToAdd.Remove(ValueColumnStart, ValueToInsert.Length);
            rowToAdd.Insert(ValueColumnStart, ValueToInsert);

            rowToAdd.Remove(UnitColumnStart, UnitToInsert.Length);
            rowToAdd.Insert(UnitColumnStart, UnitToInsert);

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

            File.AppendAllText(MainForm.PCMLogFilename, "PCM: " + Util.ByteToHexStringSimple(message) + Environment.NewLine);

            if (!EngineFaultCodesSaved)
            {
                StringBuilder sb = new StringBuilder();

                if (EngineFaultCodeList.Count > 0)
                {
                    sb.Append("ENGINE FAULT CODE LIST:" + Environment.NewLine);

                    foreach (byte code in EngineFaultCodeList)
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

                EngineFaultCodesSaved = true;
            }

            if (!EngineFaultCodes1TSaved)
            {
                StringBuilder sb = new StringBuilder();

                if (EngineFaultCode1TList.Count > 0)
                {
                    sb.Append("ONE-TRIP ENGINE FAULT CODE LIST:" + Environment.NewLine);

                    foreach (byte code in EngineFaultCode1TList)
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

                EngineFaultCodes1TSaved = true;
            }
        }
    }
}
