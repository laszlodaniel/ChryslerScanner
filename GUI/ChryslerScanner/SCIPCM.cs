using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Windows.Forms;

namespace ChryslerScanner
{
    public class SCIPCM
    {
        public SCIPCMDiagnosticsTable Diagnostics = new SCIPCMDiagnosticsTable();
        public DataTable SBEC3EngineDTC = new DataTable("SBEC3EngineDTC");
        public List<byte> StoredFaultCodeList = new List<byte>();
        public bool StoredFaultCodesSaved = true;
        public List<byte> PendingFaultCodeList = new List<byte>();
        public bool PendingFaultCodesSaved = true;
        public List<byte> FaultCode1TList = new List<byte>();
        public bool FaultCodes1TSaved = true;
        public byte[] SBEC3EngineDTCList;
        public DataColumn Column;
        public DataRow Row;

        private const int HexBytesColumnStart = 2;
        private const int DescriptionColumnStart = 28;
        private const int ValueColumnStart = 82;
        private const int UnitColumnStart = 108;

        public string state = null;
        public string speed = null;
        public string logic = null;
        public string configuration = null;

        public string HeaderUnknown  = "│ SCI-BUS (SAE J2610) PCM │ STATE: N/A                                                                                   ";
        public string HeaderDisabled = "│ SCI-BUS (SAE J2610) PCM │ STATE: DISABLED                                                                              ";
        public string HeaderEnabled  = "│ SCI-BUS (SAE J2610) PCM │ STATE: ENABLED @ BAUD | LOGIC: | CONFIGURATION:                                              ";
        public string EmptyLine      = "│                         │                                                     │                         │             │";
        public string HeaderModified = string.Empty;

        public byte ControllerHardwareType = 0;
        public byte[] PartNumberChars = new byte[6] { 0, 0, 0, 0, 0, 0 };
        public string[] EngineToolsStatusBarTextItems = new string[12];
        public int Year = 2003;

        public SCIPCM()
        {
            Column = new DataColumn
            {
                DataType = typeof(byte),
                ColumnName = "id",
                ReadOnly = true,
                Unique = true
            };
            SBEC3EngineDTC.Columns.Add(Column);

            Column = new DataColumn
            {
                DataType = typeof(string),
                ColumnName = "description",
                ReadOnly = true,
                Unique = false
            };
            SBEC3EngineDTC.Columns.Add(Column);

            DataColumn[] PrimaryKeyColumnsDTC = new DataColumn[1];
            PrimaryKeyColumnsDTC[0] = SBEC3EngineDTC.Columns["id"];
            SBEC3EngineDTC.PrimaryKey = PrimaryKeyColumnsDTC;

            #region SBEC3 engine fault codes

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x00;
            Row["description"] = "UNRECOGNIZED DTC";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x01;
            Row["description"] = "NO CAM SIGNAL AT PCM";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x02;
            Row["description"] = "INTERNAL CONTROLLER FAILURE";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x03;
            Row["description"] = "LEFT BANK O2 SENSOR STAYS ABOVE CENTER (RICH)";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x04;
            Row["description"] = "LEFT BANK O2 SENSOR STAYS BELOW CENTER (LEAN)";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x05;
            Row["description"] = "CHARGING SYSTEM VOLTAGE TOO LOW";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x06;
            Row["description"] = "CHARGING SYSTEM VOLTAGE TOO HIGH";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x07;
            Row["description"] = "TURBO BOOST LIMIT EXCEEDED";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x08;
            Row["description"] = "RIGHT BANK O2 SENSOR STAYS ABOVE CENTER (RICH)";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x09;
            Row["description"] = "RIGHT BANK O2 SENSOR STAYS BELOW CENTER (LEAN)";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x0A;
            Row["description"] = "AUTO SHUTDOWN RELAY CONTROL CIRCUIT";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x0B;
            Row["description"] = "GENERATOR FIELD NOT SWITCHING PROPERLY";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x0C;
            Row["description"] = "TORQUE CONVERTER CLUTCH SOLENOID / TRANS RELAY CIRCUITS";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x0D;
            Row["description"] = "TURBOCHARGER WASTEGATE SOLENOID CIRCUIT";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x0E;
            Row["description"] = "LOW SPEED FAN CONTROL RELAY CIRCUIT";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x0F;
            Row["description"] = "CRUISE CONTROL SOLENOID CIRCUITS";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x10;
            Row["description"] = "A/C CLUTCH RELAY CIRCUIT";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x11;
            Row["description"] = "EGR SOLENOID CIRCUIT";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x12;
            Row["description"] = "EVAP PURGE SOLENOID CIRCUIT";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x13;
            Row["description"] = "INJECTOR #3 CONTROL CIRCUIT";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x14;
            Row["description"] = "INJECTOR #2 CONTROL CIRCUIT";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x15;
            Row["description"] = "INJECTOR #1 CONTROL CIRCUIT";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x16;
            Row["description"] = "INJECTOR #3 PEAK CURRENT NOT REACHED";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x17;
            Row["description"] = "INJECTOR #2 PEAK CURRENT NOT REACHED";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x18;
            Row["description"] = "INJECTOR #1 PEAK CURRENT NOT REACHED";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x19;
            Row["description"] = "IDLE AIR CONTROL MOTOR CIRCUITS";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x1A;
            Row["description"] = "THROTTLE POSITION SENSOR VOLTAGE LOW";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x1B;
            Row["description"] = "THROTTLE POSITION SENSOR VOLTAGE HIGH";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x1C;
            Row["description"] = "THROTTLE BODY TEMP SENSOR VOLTAGE LOW";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x1D;
            Row["description"] = "THROTTLE BODY TEMP SENSOR VOLTAGE HIGH";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x1E;
            Row["description"] = "COOLANT TEMPERATURE SENSOR VOLTAGE TOO LOW";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x1F;
            Row["description"] = "COOLANT TEMPERATURE SENSOR VOLTAGE TOO HIGH";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x20;
            Row["description"] = "UPSTREAM O2 SENSOR STAYS AT CENTER";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x21;
            Row["description"] = "ENGINE IS COLD TOO LONG";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x22;
            Row["description"] = "SKIP SHIFT SOLENOID CIRCUIT";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x23;
            Row["description"] = "NO VEHICLE SPEED SENSOR SIGNAL";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x24;
            Row["description"] = "MAP SENSOR VOLTAGE TOO LOW";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x25;
            Row["description"] = "MAP SENSOR VOLTAGE TOO HIGH";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x26;
            Row["description"] = "SLOW CHANGE IN IDLE MAP SENSOR SIGNAL";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x27;
            Row["description"] = "NO CHANGE IN MAP FROM START TO RUN";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x28;
            Row["description"] = "NO CRANKSHAFT REFERENCE SIGNAL AT PCM";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x29;
            Row["description"] = "IGNITION COIL #3 PRIMARY CIRCUIT";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x2A;
            Row["description"] = "IGNITION COIL #2 PRIMARY CIRCUIT";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x2B;
            Row["description"] = "IGNITION COIL #1 PRIMARY CIRCUIT";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x2C;
            Row["description"] = "NO ASD RELAY OUTPUT VOLTAGE AT PCM";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x2D;
            Row["description"] = "SYSTEM RICH, L-IDLE ADAPTIVE AT LEAN LIMIT";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x2E;
            Row["description"] = "EGR SYSTEM FAILURE";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x2F;
            Row["description"] = "BAROMETRIC READ SOLENOID CIRCUIT";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x30;
            Row["description"] = "PCM FAILURE SRI MILE NOT STORED";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x31;
            Row["description"] = "PCM FAILURE EEPROM WRITE DENIED";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x32;
            Row["description"] = "TRANSMISSION 3-4 SHIFT SOLENOID / TRANSMISSION RELAY CIRCUITS";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x33;
            Row["description"] = "SECONDARY AIR SOLENOID CIRCUIT";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x34;
            Row["description"] = "IDLE SWITCH SHORTED TO GROUND";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x35;
            Row["description"] = "IDLE SWITCH OPEN CIRCUIT";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x36;
            Row["description"] = "SURGE VALVE SOLENOID CIRCUIT";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x37;
            Row["description"] = "INJECTOR #9 CONTROL CIRCUIT";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x38;
            Row["description"] = "INJECTOR #10 CONTROL CIRCUIT";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x39;
            Row["description"] = "INTAKE AIR TEMPERATURE SENSOR VOLTAGE LOW";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x3A;
            Row["description"] = "INTAKE AIR TEMPERATURE SENSOR VOLTAGE HIGH";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x3B;
            Row["description"] = "KNOCK SENSOR #1 CIRCUIT";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x3C;
            Row["description"] = "BAROMETRIC PRESSURE OUT OF RANGE";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x3D;
            Row["description"] = "INJECTOR #4 CONTROL CIRCUIT";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x3E;
            Row["description"] = "LEFT BANK UPSTREAM O2 SENSOR SHORTED TO VOLTAGE";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x3F;
            Row["description"] = "FUEL SYSTEM RICH, R-IDLE ADAPTIVE AT LEAN LIMIT";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x40;
            Row["description"] = "WASTEGATE #2 CIRCUIT";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x41;
            Row["description"] = "RIGHT BANK UPSTREAM O2 SENSOR STAYS AT CENTER";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x42;
            Row["description"] = "RIGHT BANK UPSTREAM O2 SENSOR SHORTED TO VOLTAGE";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x43;
            Row["description"] = "FUEL SYSTEM LEAN, R-IDLE ADAPTIVE AT RICH LIMIT";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x44;
            Row["description"] = "PCM FAILURE SPI COMMUNICATIONS";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x45;
            Row["description"] = "INJECTOR #5 CONTROL CIRCUIT";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x46;
            Row["description"] = "INJECTOR #6 CONTROL CIRCUIT";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x47;
            Row["description"] = "BATTERY TEMPERATURE SENSOR VOLTS OUT OF RNG";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x48;
            Row["description"] = "NO CMP AT IGNITION / INJ DRIVER MODULE";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x49;
            Row["description"] = "NO CKP AT IGNITION/ INJ DRIVER MODULE";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x4A;
            Row["description"] = "TRANSMISSION TEMPERATURE SENSOR VOLTAGE TOO LOW";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x4B;
            Row["description"] = "TRANSMISSION TEMPERATURE SENSOR VOLTAGE TOO HIGH";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x4C;
            Row["description"] = "IGNITION COIL #4 PRIMARY CIRCUIT";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x4D;
            Row["description"] = "IGNITION COIL #5 PRIMARY CIRCUIT";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x4E;
            Row["description"] = "FUEL SYSTEM LEAN, L-IDLE ADAPTIVE AT RICH LIMIT";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x4F;
            Row["description"] = "INJECTOR #7 CONTROL CIRCUIT";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x50;
            Row["description"] = "INJECTOR #8 CONTROL CIRCUIT";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x51;
            Row["description"] = "FUEL PUMP RESISTOR BYPASS RELAY CIRCUIT";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x52;
            Row["description"] = "CRUISE CONTROL POWER RELAY OR 12V DRIVER CIRCUIT";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x53;
            Row["description"] = "KNOCK SENSOR #2 CIRCUIT";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x54;
            Row["description"] = "FLEX FUEL SENSOR VOLTS TOO HIGH";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x55;
            Row["description"] = "FLEX FUEL SENSOR VOLTS TOO LOW";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x56;
            Row["description"] = "CRUISE CONTROL SWITCH ALWAYS HIGH";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x57;
            Row["description"] = "CRUISE CONTROL SWITCH ALWAYS LOW";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x58;
            Row["description"] = "MANIFOLD TUNE VALVE SOLENOID CIRCUIT";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x59;
            Row["description"] = "NO BUS MESSAGES";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x5A;
            Row["description"] = "A/C PRESSURE SENSOR VOLTS TOO HIGH";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x5B;
            Row["description"] = "A/C PRESSURE SENSOR VOLTS TOO LOW";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x5C;
            Row["description"] = "LOW SPEED FAN CONTROL RELAY CIRCUIT";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x5D;
            Row["description"] = "HIGH SPEED CONDENSER FAN CTRL RELAY CIRCUIT";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x5E;
            Row["description"] = "CNG TEMPERATURE SENSOR VOLTAGE TOO LOW";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x5F;
            Row["description"] = "CNG TEMPERATURE SENSOR VOLTAGE TOO HIGH";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x60;
            Row["description"] = "NO CCD/PCI BUS MESSAGES FROM TCM";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x61;
            Row["description"] = "NO CCD/PCI BUS MESSAGE FROM BCM";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x62;
            Row["description"] = "CNG PRESSURE SENSOR VOLTAGE TOO HIGH";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x63;
            Row["description"] = "CNG PRESSURE SENSOR VOLTAGE TOO LOW";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x64;
            Row["description"] = "LOSS OF FLEX FUEL CALIBRATION SIGNAL";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x65;
            Row["description"] = "FUEL PUMP RELAY CONTROL CIRCUIT";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x66;
            Row["description"] = "LEFT BANK UPSTREAM O2 SENSOR SLOW RESPONSE";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x67;
            Row["description"] = "LEFT BANK UPSTREAM O2 SENSOR HEATER FAILURE";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x68;
            Row["description"] = "DOWNSTREAM O2 SENSOR UNABLE TO SWITCH RICH/LEAN";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x69;
            Row["description"] = "DOWNSTREAM O2 SENSOR HEATER FAILURE";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x6A;
            Row["description"] = "MULTIPLE CYLINDER MISFIRE";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x6B;
            Row["description"] = "CYLINDER #1 MISFIRE";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x6C;
            Row["description"] = "CYLINDER #2 MISFIRE";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x6D;
            Row["description"] = "CYLINDER #3 MISFIRE";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x6E;
            Row["description"] = "CYLINDER #4 MISFIRE";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x6F;
            Row["description"] = "TOO LITTLE SECONDARY AIR";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x70;
            Row["description"] = "CATALYTIC CONVERTER EFFICIENCY FAILURE";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x71;
            Row["description"] = "EVAP PURGE FLOW MONITOR FAILURE";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x72;
            Row["description"] = "P/N SWITCH STUCK IN PARK OR IN GEAR";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x73;
            Row["description"] = "POWER STEERING SWITCH FAILURE";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x74;
            Row["description"] = "DESIRED FUEL TIMING ADVANCE NOT REACHED";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x75;
            Row["description"] = "LOST FUEL INJECTION TIMING SIGNAL";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x76;
            Row["description"] = "LEFT BANK FUEL SYSTEM RICH";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x77;
            Row["description"] = "LEFT BANK FUEL SYSTEM LEAN";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x78;
            Row["description"] = "RIGHT BANK FUEL SYSTEM RICH";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x79;
            Row["description"] = "RIGHT BANK FUEL SYSTEM LEAN";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x7A;
            Row["description"] = "RIGHT BANK UPSTREAM O2 SENSOR SLOW RESPONSE";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x7B;
            Row["description"] = "RIGHT BANK DOWNSTREAM O2 SENSOR SLOW RESPONSE";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x7C;
            Row["description"] = "RIGHT BANK UPSTREAM O2 SENSOR HEATER FAILURE";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x7D;
            Row["description"] = "RIGHT BANK DOWNSTREAM O2 SENSOR HEATER FAILURE";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x7E;
            Row["description"] = "DOWNSTREAM O2 SENSOR SHORTED TO VOLTAGE";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x7F;
            Row["description"] = "RIGHT BANK DOWNSTREAM O2 SENSOR SHORTED TO VOLTAGE";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x80;
            Row["description"] = "CLOSED LOOP TEMPERATURE NOT REACHED";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x81;
            Row["description"] = "LEFT BANK DOWNSTREAM O2 SENSOR STAYS AT CENTER";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x82;
            Row["description"] = "RIGHT BANK DOWNSTREAM O2 SENSOR STAYS AT CENTER";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x83;
            Row["description"] = "LEAN OPERATION AT WIDE OPEN THROTTLE";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x84;
            Row["description"] = "TPS VOLTAGE DOES NOT AGREE WITH MAP";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x85;
            Row["description"] = "TIMING BELT SKIPPED 1 TOOTH OR MORE";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x86;
            Row["description"] = "NO 5 VOLTS TO A/C PRESSURE SENSOR";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x87;
            Row["description"] = "NO 5 VOLTS TO MAP SENSOR";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x88;
            Row["description"] = "NO 5 VOLTS TO TPS";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x89;
            Row["description"] = "EATX CONTROLLER DTC PRESENT";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x8A;
            Row["description"] = "TARGET IDLE NOT REACHED";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x8B;
            Row["description"] = "HIGH SPEED RADIATOR FAN CONTROL RELAY CIRCUIT";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x8C;
            Row["description"] = "DIESEL EGR SYSTEM FAILURE";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x8D;
            Row["description"] = "GOVERNOR PRESSURE NOT EQUAL TO TARGET @ 15 - 20 PSI";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x8E;
            Row["description"] = "GOVERNOR PRESSURE ABOVE 3 PSI IN GEAR WITH 0 MPH";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x8F;
            Row["description"] = "STARTER RELAY CONTROL CIRCUIT";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x90;
            Row["description"] = "DOWNSTREAM O2 SENSOR SHORTED TO GROUND";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x91;
            Row["description"] = "VACUUM LEAK FOUND (IAC FULLY SEATED)";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x92;
            Row["description"] = "5 VOLT SUPPLY, OUTPUT TOO LOW";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x93;
            Row["description"] = "DOWNSTREAM O2 SENSOR SHORTED TO VOLTAGE";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x94;
            Row["description"] = "TORQUE CONVERTER CLUTCH, NO RPM DROP AT LOCKUP";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x95;
            Row["description"] = "FUEL LEVEL SENDING UNIT VOLTS TOO LOW";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x96;
            Row["description"] = "FUEL LEVEL SENDING UNIT VOLTS TOO HIGH";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x97;
            Row["description"] = "FUEL LEVEL UNIT NO CHANGE OVER MILES";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x98;
            Row["description"] = "BRAKE SWITCH STUCK PRESSED OR RELEASED";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x99;
            Row["description"] = "BATTERY TEMPERATURE SENSOR VOLTS TOO LOW";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x9A;
            Row["description"] = "BATTERY TEMPERATURE SENSOR VOLTS TOO HIGH";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x9B;
            Row["description"] = "LEFT BANK UPSTREAM O2 SENSOR SHORTED TO GROUND";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x9C;
            Row["description"] = "DOWNSTREAM O2 SENSOR SHORTED TO GROUND";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x9D;
            Row["description"] = "INTERMITTENT LOSS OF CMP OR CKP";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x9E;
            Row["description"] = "TOO MUCH SECONDARY AIR";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0x9F;
            Row["description"] = "DOWNSTREAM O2 SENSOR SLOW RESPONSE";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xA0;
            Row["description"] = "EVAP LEAK MONITOR SMALL LEAK DETECTED";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xA1;
            Row["description"] = "EVAP LEAK MONITOR LARGE LEAK DETECTED";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xA2;
            Row["description"] = "NO TEMPERATURE RISE SEEN FROM INTAKE HEATERS";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xA3;
            Row["description"] = "WAIT TO START LAMP CIRCUIT";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xA4;
            Row["description"] = "TRANSMISSION TEMPERATURE SENSOR, NO TEMPERATURE RISE AFTR START";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xA5;
            Row["description"] = "3-4 SHIFT SOLENOID, NO RPM DROP @ 3-4 SHIFT";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xA6;
            Row["description"] = "LOW OUTPUT SPEED SENSOR RPM, ABOVE 15 MPH";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xA7;
            Row["description"] = "GOVERNOR PRESSURE SENSOR VOLTS TOO LOW";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xA8;
            Row["description"] = "GOVERNOR PRESSURE SENSOR VOLTS TOO HIGH";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xA9;
            Row["description"] = "GOVERNOR PRESSURE SENSOR OFFSET VOLTS TOO LOW OR HIGH";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xAA;
            Row["description"] = "PCM NOT PROGRAMMED";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xAB;
            Row["description"] = "GOVERNOR PRESSURE SOLENOID CONTROL / TRANSMISSION RELAY CIRCUITS";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xAC;
            Row["description"] = "DOWNSTREAM O2 SENSOR STUCK AT CENTER";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xAD;
            Row["description"] = "TRANSMISSION 12 VOLT SUPPLY RELAY CONTROL CIRCUIT";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xAE;
            Row["description"] = "CYLINDER #5 MIS-FIRE";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xAF;
            Row["description"] = "CYLINDER #6 MIS-FIRE";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xB0;
            Row["description"] = "CYLINDER #7 MIS-FIRE";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xB1;
            Row["description"] = "CYLINDER #8 MIS-FIRE";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xB2;
            Row["description"] = "CYLINDER #9 MIS-FIRE";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xB3;
            Row["description"] = "CYLINDER #10 MIS-FIRE";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xB4;
            Row["description"] = "RIGHT BANK CATALYST EFFICIENCY FAILURE";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xB5;
            Row["description"] = "REAR BANK UPSTREAM O2 SENSOR SHORTED TO GROUND";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xB6;
            Row["description"] = "REAR BANK DOWNSTREAM O2 SENSOR SHORTED TO GROUND";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xB7;
            Row["description"] = "LEAK DETECTION PUMP SOLENOID CIRCUIT";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xB8;
            Row["description"] = "LEAK DETECT PUMP SWITCH OR MECHANICAL FAULT";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xB9;
            Row["description"] = "AUXILIARY 5 VOLT SUPPLY OUTPUT TOO LOW";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xBA;
            Row["description"] = "MISFIRE ADAPTIVE NUMERATOR AT LIMIT";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xBB;
            Row["description"] = "EVAP LEAK MONITOR PINCHED HOSE FOUND";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xBC;
            Row["description"] = "O/D SWITCH PRESSED (LOW) MORE THAN 5 MIN";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xBD;
            Row["description"] = "DOWNSTREAM O2 SENSOR HEATER FAILURE";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xC5;
            Row["description"] = "HIGH SPEED RADIATOR FAN GROUND CONTROL RELAY CIRCUIT";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xC6;
            Row["description"] = "ONE OF THE IGNITION COILS DRAWS TOO MUCH CURRENT";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xC7;
            Row["description"] = "AW4 TRANSMISSION SHIFT SOLENOID B FUNCTIONAL FAILURE";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xC8;
            Row["description"] = "RADIATOR TEMPERATURE SENSOR VOLTS TOO LOW";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xC9;
            Row["description"] = "RADIATOR TEMPERATURE SENSOR VOLTS TOO HIGH";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xCA;
            Row["description"] = "NO I/P CLUSTER CCD/PCI BUS MESSAGES RECEIVED";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xCB;
            Row["description"] = "AW4 TRANSMISSION INTERNAL FAILURE (ROM CHECK)";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xCC;
            Row["description"] = "UPSTREAM O2 SENSOR SLOW RESPONSE";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xCD;
            Row["description"] = "UPSTREAM O2 SENSOR HEATER FAILURE";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xCE;
            Row["description"] = "UPSTREAM O2 SENSOR SHORTED TO VOLTAGE";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xCF;
            Row["description"] = "UPSTREAM O2 SENSOR SHORTED TO GROUND";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xD0;
            Row["description"] = "NO CAM SYNC SIGNAL AT PCM";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xD1;
            Row["description"] = "GLOW PLUG RELAY CONTROL CIRCUIT";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xD2;
            Row["description"] = "HIGH SPEED CONDENSER FAN CONTROL RELAY CIRCUIT";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xD3;
            Row["description"] = "AW4 TRANSMISSION SHIFT SOLENOID B (2-3) SHORTED TO VOLTAGE (12V)";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xD4;
            Row["description"] = "EGR POSITION SENSOR VOLTS TOO LOW";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xD5;
            Row["description"] = "EGR POSITION SENSOR VOLTS TOO HIGH";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xD6;
            Row["description"] = "NO 5 VOLTS TO EGR POSITION SENSOR";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xD7;
            Row["description"] = "EGR POSITION SENSOR RATIONALITY FAILURE";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xD8;
            Row["description"] = "IGNITION COIL #6 PRIMARY CIRCUIT";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xD9;
            Row["description"] = "INTAKE MANIFOLD SHORT RUNNER SOLENOID CIRCUIT";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xDA;
            Row["description"] = "AIR ASSIST INJECTION SOLENOID CIRCUIT";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xDB;
            Row["description"] = "CATALYST TEMPERATURE SENSOR VOLTS TOO HIGH";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xDC;
            Row["description"] = "CATALYST TEMPERATURE SENSOR VOLTS TOO LOW";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xDD;
            Row["description"] = "EATX RPM PULSE PERFORMANCE CONDITION";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xDE;
            Row["description"] = "NO BUS MESSAGE RECEIVED FROM COMPANION MODULE";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xDF;
            Row["description"] = "MIL FAULT IN COMPANION MODULE";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xE0;
            Row["description"] = "COOLANT TEMPERATURE SENSOR PERFORMANCE";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xE1;
            Row["description"] = "NO MIC BUS MESSAGE";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xE2;
            Row["description"] = "NO SKIM BUS MESSAGE RECEIVED";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xE3;
            Row["description"] = "IGNITION COIL #7 PRIMARY CIRCUIT";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xE4;
            Row["description"] = "IGNITION COIL #8 PRIMARY CIRCUIT";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xE5;
            Row["description"] = "PCV SOLENOID CIRCUIT";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xE6;
            Row["description"] = "TRANSMISSION FAN RELAY CIRCUIT";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xE7;
            Row["description"] = "TCC OR O/D SOLENOID PERFORMANCE";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xE8;
            Row["description"] = "WRONG OR INVALID KEY MESSAGE RECEIVED FROM SKIM";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xEA;
            Row["description"] = "AW4 TRANSMISSION SOLENOID A 1-2/3-4 OR TCC SOLENOID C FUNCTIONAL FAIL";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xEB;
            Row["description"] = "AW4 TRANSMISSION TCC SOLENOID C SHORTED TO GROUND";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xEC;
            Row["description"] = "AW4 TRANSMISSION TCC SOLENOID C SHORTED TO VOLTAGE (12V)";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xED;
            Row["description"] = "AW4 TRANSMISSION BATTERY VOLTS SENSE TOO LOW";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xEE;
            Row["description"] = "AW4 TRANSMISSION BATTERY VOLTS SENSE TOO HIGH";
            SBEC3EngineDTC.Rows.Add(Row);

            Row = SBEC3EngineDTC.NewRow();
            Row["id"] = 0xEF;
            Row["description"] = "AISIN AW4 TRANSMISSION DTC PRESENT";
            SBEC3EngineDTC.Rows.Add(Row);

            #endregion

            SBEC3EngineDTCList = SBEC3EngineDTC.AsEnumerable().Select(r => r.Field<byte>("id")).ToArray();
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

            byte[] timestamp = new byte[4];
            byte[] message = new byte[] { };
            byte[] payload = new byte[] { };
            byte[] StoredFaultCodePayload = new byte[] { }; // stored fault codes
            byte[] PendingFaultCodePayload = new byte[] { }; // pending fault codes
            byte[] FaultCode1TPayload = new byte[] { }; // one-trip fault codes

            if (data.Length >= 4)
            {
                Array.Copy(data, 0, timestamp, 0, 4);
            }

            if (data.Length >= 5)
            {
                message = new byte[data.Length - 4];
                Array.Copy(data, 4, message, 0, message.Length); // copy message from the input byte array
            }

            if (data.Length >= 6)
            {
                payload = new byte[data.Length - 5];
                Array.Copy(data, 5, payload, 0, payload.Length); // copy payload from the input byte array (without ID)
            }

            if (message.Length >= 3)
            {
                if ((message[0] == 0x10) || (message[0] == 0x32)) // stored fault codes, SBEC or CUMMINS, respectively
                {
                    StoredFaultCodePayload = new byte[message.Length - 2]; // skip ID and checksum bytes
                    Array.Copy(message, 1, StoredFaultCodePayload, 0, StoredFaultCodePayload.Length); // copy payload from the input byte array
                }
                else if (message[0] == 0x11) // pending fault codes
                {
                    PendingFaultCodePayload = new byte[2]; // always 2 bytes long
                    Array.Copy(message, 1, PendingFaultCodePayload, 0, PendingFaultCodePayload.Length); // copy payload from the input byte array (without ID)
                }
                else if (message[0] == 0x2E) // one-trip fault codes
                {
                    FaultCode1TPayload = new byte[message.Length - 2];
                    Array.Copy(message, 1, FaultCode1TPayload, 0, FaultCode1TPayload.Length); // copy payload from the input byte array (without ID and checksum byte)
                }
            }

            string DescriptionToInsert = string.Empty;
            string ValueToInsert = string.Empty;
            string UnitToInsert = string.Empty;
            byte ID = message[0];

            if ((speed == "976.5 baud") || (speed == "7812.5 baud"))
            {
                switch (ID)
                {
                    case 0x00:
                        DescriptionToInsert = "PCM WAKE UP";
                        break;
                    case 0x10: // SBEC
                    case 0x32: // CUMMINS
                        DescriptionToInsert = "STORED FAULT CODE LIST";

                        if (message.Length < 3) break;

                        byte ChecksumA = 0;
                        byte ChecksumALocation = (byte)(message.Length - 1);

                        for (int i = 0; i < ChecksumALocation; i++)
                        {
                            ChecksumA += message[i];
                        }

                        if (ChecksumA != message[ChecksumALocation])
                        {
                            StoredFaultCodeList.Clear();
                            ValueToInsert = "CHECKSUM ERROR";
                            StoredFaultCodesSaved = true;
                            break;
                        }

                        StoredFaultCodeList.Clear();
                        StoredFaultCodeList.AddRange(StoredFaultCodePayload);
                        StoredFaultCodeList.Remove(0xFD); // not fault code related
                        StoredFaultCodeList.Remove(0xFE); // end of fault code list signifier

                        if (StoredFaultCodeList.Count == 0)
                        {
                            StoredFaultCodeList.Clear();
                            ValueToInsert = "NO FAULT CODE";
                            StoredFaultCodesSaved = false;
                            break;
                        }

                        ValueToInsert = Util.ByteToHexStringSimple(StoredFaultCodeList.ToArray());
                        StoredFaultCodesSaved = false;
                        break;
                    case 0x11:
                        DescriptionToInsert = "PENDING FAULT CODE LIST";

                        if (message.Length < 3) break;

                        if (((payload[0] == 0) || (payload[0] == 0xFD)) && (payload[1] == 0))
                        {
                            PendingFaultCodeList.Clear();
                            ValueToInsert = "NO FAULT CODE";
                            PendingFaultCodesSaved = false;
                            break;
                        }

                        PendingFaultCodeList.AddRange(PendingFaultCodePayload);
                        ValueToInsert = Util.ByteToHexString(payload, 0, 2);
                        PendingFaultCodesSaved = false;
                        break;
                    case 0x12:
                        DescriptionToInsert = "SELECT HIGH-SPEED MODE";
                        break;
                    case 0x13:
                        DescriptionToInsert = "ACTUATOR TEST";

                        if (message.Length < 3) break;

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
                                DescriptionToInsert = "ACTUATOR TEST | TORQUE CONVERTER CLUTCH RELAY";
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
                            case 0x3E:
                                DescriptionToInsert = "ACTUATOR TEST | ALL RADIATOR FAN RELAYS";
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
                            case 0x50:
                                DescriptionToInsert = "ACTUATOR TEST | TRANSMISSION FAN RELAY";
                                break;
                            case 0x51:
                                DescriptionToInsert = "ACTUATOR TEST | TRANSMISSION PTU SOLENOID";
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
                        break;
                    case 0x14:
                        DescriptionToInsert = "REQUEST DIAGNOSTIC DATA";

                        if (message.Length < 3) break;

                        switch (payload[0])
                        {
                            case 0x01:
                                DescriptionToInsert = "BATTERY TEMPERATURE SENSOR VOLTAGE";

                                double BTSVoltage = payload[1] * 0.0196;
                                
                                ValueToInsert = Math.Round(BTSVoltage, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "V";
                                break;
                            case 0x02:
                                DescriptionToInsert = "UPSTREAM O2 1/1 SENSOR VOLTAGE";

                                double O2S11Voltage = payload[1] * 0.0196;

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
                                DescriptionToInsert = "ENGINE COOLANT TEMPERATURE SENSOR VOLTAGE";

                                double ECTSVoltage = payload[1] * 0.0196;

                                ValueToInsert = Math.Round(ECTSVoltage, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "V";
                                break;
                            case 0x07:
                                DescriptionToInsert = "THROTTLE POSITION SENSOR VOLTAGE";

                                double TPSVoltage = payload[1] * 0.0196;

                                ValueToInsert = Math.Round(TPSVoltage, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "V";
                                break;
                            case 0x08:
                                DescriptionToInsert = "MINIMUM TPS VOLTAGE";

                                double MinTPSVoltage = payload[1] * 0.0196;

                                ValueToInsert = Math.Round(MinTPSVoltage, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "V";
                                break;
                            case 0x09:
                                DescriptionToInsert = "KNOCK SENSOR 1 VOLTAGE";

                                double KnockSensorVoltage = payload[1] * 0.0196;

                                ValueToInsert = Math.Round(KnockSensorVoltage, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "V";
                                break;
                            case 0x0A:
                                DescriptionToInsert = "BATTERY VOLTAGE";

                                double BatteryVoltage = payload[1] * 0.0625;

                                ValueToInsert = Math.Round(BatteryVoltage, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "V";
                                break;
                            case 0x0B:
                                DescriptionToInsert = "INTAKE MANIFOLD ABSOLUTE PRESSURE (MAP)";

                                double MAPPSI = payload[1] * 0.059756;
                                double MAPKPA = MAPPSI * 6.894757;

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
                                DescriptionToInsert = "LONG TERM FUEL TRIM 1";

                                double LTFT1 = payload[1] * 0.196;

                                if (payload[1] >= 0x80) LTFT1 -= 50.0;

                                ValueToInsert = Math.Round(LTFT1, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "PERCENT";
                                break;
                            case 0x0F:
                                DescriptionToInsert = "BAROMETRIC PRESSURE";

                                double BarometricPressurePSI = payload[1] * 0.059756;
                                double BarometricPressureKPA = BarometricPressurePSI * 6.894757;

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
                                DescriptionToInsert = "ENGINE SPEED";

                                double EngineSpeed = payload[1] * 32.0;
                                
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
                                DescriptionToInsert = "SPARK ADVANCE";

                                double SparkAdvance = payload[1] * 0.5;
                                
                                ValueToInsert = Math.Round(SparkAdvance, 1).ToString("0.0").Replace(",", ".");
                                UnitToInsert = "DEG";
                                break;
                            case 0x16:
                            case 0x21:
                                DescriptionToInsert = "CYLINDER 1 RETARD";

                                double Cylinder1Retard = payload[1] * 0.5;
                                
                                ValueToInsert = Math.Round(Cylinder1Retard, 1).ToString("0.0").Replace(",", ".");
                                UnitToInsert = "DEG";
                                break;
                            case 0x17:
                                DescriptionToInsert = "CYLINDER 2 RETARD";

                                double Cylinder2Retard = payload[1] * 0.5;
                                
                                ValueToInsert = Math.Round(Cylinder2Retard, 1).ToString("0.0").Replace(",", ".");
                                UnitToInsert = "DEG";
                                break;
                            case 0x18:
                                DescriptionToInsert = "CYLINDER 3 RETARD";

                                double Cylinder3Retard = payload[1] * 0.5;
                                
                                ValueToInsert = Math.Round(Cylinder3Retard, 1).ToString("0.0").Replace(",", ".");
                                UnitToInsert = "DEG";
                                break;
                            case 0x19:
                                DescriptionToInsert = "CYLINDER 4 RETARD";

                                double Cylinder4Retard = payload[1] * 0.5;
                                
                                ValueToInsert = Math.Round(Cylinder4Retard, 1).ToString("0.0").Replace(",", ".");
                                UnitToInsert = "DEG";
                                break;
                            case 0x1A:
                                DescriptionToInsert = "TARGET BOOST";

                                double TargetBoostPSI = payload[1] * 0.115294117;
                                double TargetBoostKPA = TargetBoostPSI * 6.89475729;
                                
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
                                DescriptionToInsert = "INTAKE AIR TEMPERATURE";

                                double IntakeAirTemperatureC = payload[1] - 128;
                                double IntakeAirTemperatureF = 1.8 * IntakeAirTemperatureC + 32.0;
                                
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
                                DescriptionToInsert = "INTAKE AIR TEMPERATURE SENSOR VOLTAGE";

                                double IATVoltage = payload[1] * 0.0196;
                                
                                ValueToInsert = Math.Round(IATVoltage, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "V";
                                break;
                            case 0x1D:
                                DescriptionToInsert = "CRUISE SET SPEED";

                                double CruiseSetSpeedMPH = payload[1] * 0.5;
                                double CruiseSetSpeedKMH = CruiseSetSpeedMPH * 1.609344;

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
                                        LastCruiseCutoutReasonA = "DTC PRESENT";
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
                                        LastCruiseCutoutReasonA = "TPS LIMP-IN";
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
                                        CruiseDeniedReasonA = "DTC PRESENT";
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
                                        CruiseDeniedReasonA = "TPS LIMP-IN";
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
                                DescriptionToInsert = "BATTERY CHARGING VOLTAGE";

                                double BatteryChargingVoltage = payload[1] * 0.0625;
                                
                                ValueToInsert = Math.Round(BatteryChargingVoltage, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "V";
                                break;
                            case 0x25:
                                DescriptionToInsert = "OVER 5 PSI BOOST TIMER";
                                ValueToInsert = payload[1].ToString("0");
                                break;
                            case 0x28:
                                DescriptionToInsert = "WASTEGATE DUTY CYCLE";

                                double WDCPercent = payload[1] * 0.5;
                                
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
                                DescriptionToInsert = "CRUISE SWITCH VOLTAGE SENSE";

                                double CSWVoltage = payload[1] * 0.0196;
                                
                                ValueToInsert = Math.Round(CSWVoltage, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "V";
                                break;
                            case 0x2D:
                                DescriptionToInsert = "AMBIENT/BATTERY TEMPERATURE";

                                double AmbientTemperatureC = payload[1] - 128;
                                double AmbientTemperatureF = 1.8 * AmbientTemperatureC + 32.0;

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
                            case 0x2E:
                                DescriptionToInsert = "FUEL FACTOR (NOT LH)";
                                ValueToInsert = payload[1].ToString("0");
                                break;
                            case 0x2F:
                                DescriptionToInsert = "UPSTREAM O2 2/1 SENSOR VOLTAGE";

                                double O2S21Voltage = payload[1] * 0.0196;
                                
                                ValueToInsert = Math.Round(O2S21Voltage, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "V";
                                break;
                            case 0x30:
                                DescriptionToInsert = "KNOCK SENSOR 2 VOLTAGE";

                                double KnockSensor2Voltage = payload[1] * 0.0196;
                                
                                ValueToInsert = Math.Round(KnockSensor2Voltage, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "V";
                                break;
                            case 0x31:
                                DescriptionToInsert = "LONG TERM FUEL TRIM 2";

                                double LTFT2 = payload[1] * 0.196;

                                if (payload[1] >= 0x80) LTFT2 -= 50.0;

                                ValueToInsert = Math.Round(LTFT2, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "PERCENT";
                                break;
                            case 0x32:
                                DescriptionToInsert = "A/C HIGH-SIDE PRESSURE SENSOR VOLTAGE";

                                double ACHSPSVoltage = payload[1] * 0.0196;
                                
                                ValueToInsert = Math.Round(ACHSPSVoltage, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "V";
                                break;
                            case 0x33:
                                DescriptionToInsert = "A/C HIGH-SIDE PRESSURE";

                                double ACHSPressurePSI = payload[1] * 1.961;
                                double ACHSPressureKPA = ACHSPressurePSI * 6.894757;
                                
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
                                DescriptionToInsert = "FLEX FUEL SENSOR VOLTAGE";

                                double FlexFuelSensorVoltage = payload[1] * 0.0196;
                                
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
                                DescriptionToInsert = "CALPOT VOLTAGE";

                                double CalPotVoltage = payload[1] * 0.0196;
                                
                                ValueToInsert = Math.Round(CalPotVoltage, 31).ToString("0.000").Replace(",", ".");
                                break;
                            case 0x3F:
                                DescriptionToInsert = "DOWNSTREAM O2 1/2 SENSOR VOLTAGE";

                                double O2S12Voltage = payload[1] * 0.0196;
                                
                                ValueToInsert = Math.Round(O2S12Voltage, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "V";
                                break;
                            case 0x40:
                                DescriptionToInsert = "MAP SENSOR VOLTAGE";

                                double MAPSensorVoltage = payload[1] * 0.0196;
                                
                                ValueToInsert = Math.Round(MAPSensorVoltage, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "V";
                                break;
                            case 0x41:
                                DescriptionToInsert = "VEHICLE SPEED";

                                byte VehicleSpeedMPH = payload[1];
                                double VehicleSpeedKMH = VehicleSpeedMPH * 1.609344;
                                
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
                                DescriptionToInsert = "MAP VACUUM";

                                double MAPVacuumPSI = payload[1] * 0.059756;
                                double MAPVacuumKPA = MAPVacuumPSI * 6.894757;
                                
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
                                DescriptionToInsert = "DELTA THROTTLE POSITION";

                                double DeltaThrottlePosition = payload[1] * 0.0196;
                                
                                ValueToInsert = Math.Round(DeltaThrottlePosition, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "V";
                                break;
                            case 0x47:
                                DescriptionToInsert = "SPARK ADVANCE";

                                double SparkAdvance2 = payload[1] * 0.5;
                                
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
                                DescriptionToInsert = "DOWNSTREAM O2 2/2 SENSOR VOLTAGE";

                                double O2S22Voltage = payload[1] * 0.0196;
                                
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
                                DescriptionToInsert = "FUEL LEVEL SENSOR VOLTAGE";

                                double FuelLevelSensorVoltage = payload[1] * 0.0196;
                                
                                ValueToInsert = Math.Round(FuelLevelSensorVoltage, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "V";
                                break;
                            case 0x4F:
                                DescriptionToInsert = "FUEL LEVEL";

                                double FuelLevelG = payload[1] * 0.125;
                                double FuelLevelL = FuelLevelG * 3.785412;
                                
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

                                if (Util.IsBitSet(payload[1], 0)) FSS2.Add("OPEN LOOP");
                                if (Util.IsBitSet(payload[1], 1)) FSS2.Add("CLOSED LOOP");
                                if (Util.IsBitSet(payload[1], 2)) FSS2.Add("OPEN LOOP / DRIVE");
                                if (Util.IsBitSet(payload[1], 3)) FSS2.Add("OPEN LOOP / DTC");
                                if (Util.IsBitSet(payload[1], 4)) FSS2.Add("CLOSED LOOP / DTC");

                                if (FSS2.Count == 0)
                                {
                                    ValueToInsert = "N/A";
                                    break;
                                }

                                foreach (string s in FSS2)
                                {
                                    ValueToInsert += s + " | ";
                                }

                                if (ValueToInsert.Length > 2) ValueToInsert = ValueToInsert.Remove(ValueToInsert.Length - 3); // remove last "|" character
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
                                        LastCruiseCutoutReasonB = "DTC PRESENT";
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
                                        LastCruiseCutoutReasonB = "TPS LIMP-IN";
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
                                        CruiseDeniedReasonB = "DTC PRESENT";
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
                                        CruiseDeniedReasonB = "TPS LIMP-IN";
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
                            case 0x5A:
                                DescriptionToInsert = "OUTPUT SHAFT SPEED";

                                double OutputShaftSpeed = payload[1] * 20.0;
                                
                                ValueToInsert = OutputShaftSpeed.ToString("0");
                                UnitToInsert = "RPM";
                                break;
                            case 0x5B:
                                DescriptionToInsert = "GOVERNOR PRESSURE DUTY CYCLE";

                                double GovPDC = payload[1] * 0.3921568627;
                                
                                ValueToInsert = Math.Round(GovPDC, 1).ToString("0.0").Replace(",", ".");
                                UnitToInsert = "PERCENT";
                                break;
                            case 0x5C:
                                DescriptionToInsert = "ENGINE LOAD";

                                double EngineLoadB = payload[1] * 0.3921568627;
                                
                                ValueToInsert = Math.Round(EngineLoadB, 1).ToString("0.0").Replace(",", ".");
                                UnitToInsert = "PERCENT";
                                break;
                            case 0x5F:
                                DescriptionToInsert = "EGR POSITION SENSOR VOLTAGE";

                                double EGRSensorVoltage = payload[1] * 0.0196;
                                
                                ValueToInsert = Math.Round(EGRSensorVoltage, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "V";
                                break;
                            case 0x60:
                                DescriptionToInsert = "EGR ZREF UPDATE D.C.";
                                ValueToInsert = payload[1].ToString("0");
                                break;
                            case 0x64:
                                DescriptionToInsert = "ACTUAL PURGE CURRENT";

                                double ActualPurgeCurrent = payload[1] * 0.0196;
                                
                                ValueToInsert = Math.Round(ActualPurgeCurrent, 1).ToString("0.0").Replace(",", ".");
                                UnitToInsert = "A";
                                break;
                            case 0x65:
                                DescriptionToInsert = "CATALYST TEMPERATURE SENSOR VOLTAGE";

                                double CTSVoltage = payload[1] * 0.0196;
                                
                                ValueToInsert = Math.Round(CTSVoltage, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "V";
                                break;
                            case 0x66:
                                DescriptionToInsert = "CATALYST TEMPERATURE";

                                double CatalystTemperatureC = payload[1] - 128;
                                double CatalystTemperatureF = 1.8 * CatalystTemperatureC + 32.0;

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
                                DescriptionToInsert = "AMBIENT TEMPERATURE SENSOR VOLTAGE";

                                double ATSVoltage = payload[1] * 0.0196;
                                
                                ValueToInsert = Math.Round(ATSVoltage, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "V";
                                break;
                            case 0x6D:
                                DescriptionToInsert = "T-CASE SWITCH VOLTAGE";

                                double TCSwVoltage = payload[1] * 0.0196;
                                
                                ValueToInsert = Math.Round(TCSwVoltage, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "V";
                                break;
                            case 0x7A:
                                DescriptionToInsert = "FCA CURRENT";

                                double FCACurrent = payload[1] * 0.0196;
                                
                                ValueToInsert = Math.Round(FCACurrent, 1).ToString("0.0").Replace(",", ".");
                                UnitToInsert = "A";
                                break;
                            case 0x7C:
                                DescriptionToInsert = "OIL TEMPERATURE SENSOR VOLTAGE";

                                double OTSVoltage = payload[1] * 0.0196;
                                
                                ValueToInsert = Math.Round(OTSVoltage, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "V";
                                break;
                            case 0x7D:
                                DescriptionToInsert = "OIL TEMPERATURE";

                                double OilTemperatureC = payload[1] - 64;
                                double OilTemperatureF = 1.8 * OilTemperatureC + 32.0;
                                

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
                        break;
                    case 0x15:
                        DescriptionToInsert = "READ FLASH MEMORY";

                        if (message.Length < 4) break;

                        DescriptionToInsert = "READ FLASH MEMORY | OFFSET: " + Util.ByteToHexString(payload, 0, 2);
                        ValueToInsert = Util.ByteToHexString(payload, 2, 1);
                        break;
                    case 0x16:
                        DescriptionToInsert = "READ FLASH MEMORY CONSTANT";

                        if (message.Length < 3) break;

                        ushort offset = (ushort)(payload[0] + 0x8000);
                        byte[] offsetArray = new byte[2];
                        offsetArray[0] = (byte)((offset >> 8) & 0xFF);
                        offsetArray[1] = (byte)(offset & 0xFF);

                        DescriptionToInsert = "READ FLASH MEMORY CONSTANT | OFFSET: " + Util.ByteToHexStringSimple(offsetArray);
                        ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                        break;
                    case 0x17:
                        DescriptionToInsert = "ERASE ENGINE FAULT CODES";

                        if (message.Length < 2) break;

                        if (payload[0] == 0xE0) ValueToInsert = "ERASED";
                        else ValueToInsert = "FAILED";

                        break;
                    case 0x18:
                        DescriptionToInsert = "CONTROL ASD RELAY";

                        if (message.Length < 3) break;

                        ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                        break;
                    case 0x19:
                        DescriptionToInsert = "SET ENGINE IDLE SPEED";

                        if (message.Length < 2) break;

                        ValueToInsert = Math.Round(payload[0] * 7.85).ToString("0");

                        if (payload[0] < 0x42)
                        {
                            ValueToInsert += " - TOO LOW!";
                        }

                        UnitToInsert = "RPM";
                        break;
                    case 0x1A:
                        DescriptionToInsert = "SWITCH TEST";

                        if (message.Length < 3) break;

                        List<string> SwitchList = new List<string>();

                        switch (payload[0])
                        {
                            case 0x01:
                                if (Util.IsBitSet(payload[1], 0)) SwitchList.Add("WAIT TO START LAMP");
                                if (Util.IsBitSet(payload[1], 1)) SwitchList.Add("INTAKE HEATER #1");
                                if (Util.IsBitSet(payload[1], 2)) SwitchList.Add("INTAKE HEATER #2");
                                if (Util.IsBitSet(payload[1], 3)) SwitchList.Add("IDLE VALIDATION SW1");
                                if (Util.IsBitSet(payload[1], 4)) SwitchList.Add("IDLE VALIDATION SW2");
                                if (Util.IsBitSet(payload[1], 5)) SwitchList.Add("IDLE SELECT");
                                if (Util.IsBitSet(payload[1], 6)) SwitchList.Add("TRANSFER PMPDR");
                                break;
                            case 0x02:
                                if (Util.IsBitSet(payload[1], 0)) SwitchList.Add("INJ PUMP");
                                if (Util.IsBitSet(payload[1], 2)) SwitchList.Add("A/C CLUTCH");
                                if (Util.IsBitSet(payload[1], 3)) SwitchList.Add("EXHAUST BRAKE");
                                if (Util.IsBitSet(payload[1], 4)) SwitchList.Add("BRAKE");
                                if (Util.IsBitSet(payload[1], 5)) SwitchList.Add("EVAP PURGE");
                                if (Util.IsBitSet(payload[1], 7)) SwitchList.Add("LOW OIL");
                                break;
                            case 0x03:
                                if (Util.IsBitSet(payload[1], 1)) SwitchList.Add("MIL");
                                if (Util.IsBitSet(payload[1], 2)) SwitchList.Add("GENERATOR LAMP");
                                if (Util.IsBitSet(payload[1], 3)) SwitchList.Add("GENERATOR FIELD");
                                if (Util.IsBitSet(payload[1], 4)) SwitchList.Add("12V FEED");
                                if (Util.IsBitSet(payload[1], 6)) SwitchList.Add("TRANS O/D");
                                if (Util.IsBitSet(payload[1], 7)) SwitchList.Add("TRANS TOW MODE");
                                break;
                            case 0x04:
                                if (Util.IsBitSet(payload[1], 1)) SwitchList.Add("TRD LINK");
                                if (Util.IsBitSet(payload[1], 4)) SwitchList.Add("ASD");
                                if (Util.IsBitSet(payload[1], 6)) SwitchList.Add("IGNITION");
                                break;
                            default:
                                DescriptionToInsert = "SWITCH TEST | OFFSET: " + Util.ByteToHexString(payload, 0, 1);
                                break;
                        }

                        if (SwitchList.Count == 0) break;

                        DescriptionToInsert += " | ";

                        foreach (string s in SwitchList)
                        {
                            DescriptionToInsert += s + " | ";
                        }

                        if (DescriptionToInsert.Length > 2) DescriptionToInsert = DescriptionToInsert.Remove(DescriptionToInsert.Length - 3); // remove last "|" character

                        break;
                    case 0x1B:
                        DescriptionToInsert = "INIT BYTE MODE DOWNLOAD";
                        break;
                    case 0x1C:
                        DescriptionToInsert = "WRITE MEMORY";

                        if (message.Length < 4) break;

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
                        break;
                    case 0x1F:
                        DescriptionToInsert = "WRITE RAM WORKER";

                        if (message.Length < 4) break;

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
                        break;
                    case 0x20:
                        DescriptionToInsert = "RUN RAM WORKER";

                        if (message.Length < 4) break;

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
                        break;
                    case 0x21:
                        DescriptionToInsert = "IGNITION TIMING";

                        if (message.Length < 3) break;

                        switch (payload[0])
                        {
                            case 0x00:
                                DescriptionToInsert += " | UNKILL SPARK SCATTER";
                                break;
                            case 0x01:
                                DescriptionToInsert += " | KILL SPARK SCATTER";
                                break;
                            default:
                                DescriptionToInsert += " | MODE: " + Util.ByteToHexString(payload, 0, 1);
                                break;
                        }

                        switch (payload[1])
                        {
                            case 0x00:
                                ValueToInsert = "BASIC TIMING ABOLISHED";
                                break;
                            case 0x01:
                                ValueToInsert = "BASIC TIMING INITIATED";
                                break;
                            case 0x02:
                                ValueToInsert = "REJECTED (OPEN THR)";
                                break;
                            case 0x03:
                                ValueToInsert = "REJECTED (IN DRIVE)";
                                break;
                            default:
                                ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                                UnitToInsert = "UNDEFINED";
                                break;
                        }
                        break;
                    case 0x22:
                        DescriptionToInsert = "READ ENGINE PARAMETER";

                        if (message.Length < 4) break;

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
                        break;
                    case 0x23:
                        DescriptionToInsert = "RESET MEMORY";

                        if (message.Length < 3) break;

                        switch (payload[0])
                        {
                            case 0x01:
                                DescriptionToInsert = "ERASE ALL FAULT DATA";
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
                                DescriptionToInsert = "CRUISE FAULT ENABLE";
                                break;
                            case 0x0F:
                                DescriptionToInsert = "CRUISE FAULT DISABLE";
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
                                DescriptionToInsert = "RESET SKIM";
                                break;
                            case 0x14:
                                DescriptionToInsert = "RESET DUTY CYCLE MONITOR";
                                break;
                            case 0x15:
                                DescriptionToInsert = "RESET TRIP/IDLE/CRUISE/INJ";
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
                                ValueToInsert = "MODE NOT AVAILABLE";
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
                        break;
                    case 0x25:
                        DescriptionToInsert = "OVERRIDE"; // similar to SCI 13 actuator test, except here the PCM lets us torture the running engine for real

                        if (message.Length < 4) break;

                        if ((payload[0] == 0x00) && (payload[1] == 0x00) && (payload[2] == 0x00))
                        {
                            DescriptionToInsert += " | ALL SUSPENDED";
                            break;
                        }

                        switch (payload[0])
                        {
                            case 0x01:
                                DescriptionToInsert += " | PPS DUTY CYCLE";
                                break;
                            case 0x02:
                                DescriptionToInsert += " | ";
                                break;
                            case 0x03:
                                DescriptionToInsert += " | ";
                                break;
                            case 0x04:
                                DescriptionToInsert += " | LINEAR EGR STEPS";
                                break;
                            case 0x05:
                                DescriptionToInsert += " | FUEL INJECTOR #1";
                                break;
                            case 0x06:
                                DescriptionToInsert += " | FUEL INJECTOR #2";
                                break;
                            case 0x07:
                                DescriptionToInsert += " | FUEL INJECTOR #3";
                                break;
                            case 0x08:
                                DescriptionToInsert += " | FUEL INJECTOR #4";
                                break;
                            case 0x09:
                                DescriptionToInsert += " | FUEL INJECTOR #5";
                                break;
                            case 0x0A:
                                DescriptionToInsert += " | FUEL INJECTOR #6";
                                break;
                            case 0x0B:
                                DescriptionToInsert += " | ";
                                break;
                            case 0x0C:
                                DescriptionToInsert += " | ";
                                break;
                            case 0x0D:
                                DescriptionToInsert += " | ";
                                break;
                            case 0x0E:
                                DescriptionToInsert += " | ";
                                break;
                            case 0x0F:
                                DescriptionToInsert += " | MINIMUM AIR FLOW";
                                break;
                            case 0x10:
                                DescriptionToInsert += " | CALPOT LHBL";
                                break;
                            case 0x11:
                                DescriptionToInsert += " | ALTERNATOR FIELD";
                                break;
                            case 0x12:
                                DescriptionToInsert += " | ";
                                break;
                            case 0x13:
                                DescriptionToInsert += " | LEAK DETECTION PUMP SYSTEM";
                                break;
                            case 0x1C:
                                DescriptionToInsert += " | MISFIRE MONITOR";
                                break;
                            case 0x1D:
                                DescriptionToInsert += " | EVAPORATIVE EMISSION CONTROL SYSTEM";
                                break;
                            case 0x21:
                                DescriptionToInsert += " | LINEAR IAC MOTOR";
                                break;
                            case 0x25:
                                DescriptionToInsert += " | CYLINDER PERFORMANCE TEST";
                                break;
                            case 0x26:
                                DescriptionToInsert += " | HIGH-PRESSURE SAFETY VALVE TEST";
                                break;
                            default:
                                DescriptionToInsert += " | SETTING: " + Util.ByteToHexString(payload, 0, 1);
                                break;
                        }

                        switch (payload[1])
                        {
                            case 0x00:
                                ValueToInsert = "RESET";
                                break;
                            case 0x01:
                                ValueToInsert = "ENABLE";
                                break;
                            case 0x02:
                                ValueToInsert = "DISABLE";
                                break;
                            default:
                                if (payload[1] >= 0x80)
                                {
                                    ValueToInsert = payload[1].ToString("0");
                                }
                                break;
                        }

                        if (payload[2] == payload[1])
                        {
                            UnitToInsert = "OK";
                            break;
                        }

                        switch (payload[2])
                        {
                            default:
                                ValueToInsert = "ERROR";
                                UnitToInsert = Util.ByteToHexString(payload, 2, 1);
                                break;
                        }
                        break;
                    case 0x26:
                        DescriptionToInsert = "READ FLASH MEMORY";

                        if (message.Length < 5) break;

                        DescriptionToInsert = "READ FLASH MEMORY | OFFSET: " + Util.ByteToHexString(payload, 0, 3);
                        ValueToInsert = Util.ByteToHexString(payload, 3, 1);
                        break;
                    case 0x27:
                        DescriptionToInsert = "WRITE EEPROM";

                        if (message.Length < 5) break;

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
                        break;
                    case 0x28:
                        DescriptionToInsert = "READ EEPROM";

                        if (message.Length < 4) break;

                        DescriptionToInsert = "READ EEPROM | OFFSET: " + Util.ByteToHexString(payload, 0, 2);
                        ValueToInsert = Util.ByteToHexString(payload, 2, 1);
                        break;
                    case 0x29:
                        DescriptionToInsert = "WRITE RAM";

                        if (message.Length < 5) break;

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
                        break;
                    case 0x2A:
                        DescriptionToInsert = "CONFIGURATION";

                        if (message.Length < 3) break;

                        switch (payload[0])
                        {
                            case 0x01:
                                DescriptionToInsert += " | PART NUMBER 1-2";
                                ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                                PartNumberChars[0] = payload[1];
                                break;
                            case 0x02:
                                DescriptionToInsert += " | PART NUMBER 3-4";
                                ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                                PartNumberChars[1] = payload[1];
                                break;
                            case 0x03:
                                DescriptionToInsert += " | PART NUMBER 5-6";
                                ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                                PartNumberChars[2] = payload[1];
                                break;
                            case 0x04:
                                DescriptionToInsert += " | PART NUMBER 7-8 | CHECKSUM";
                                ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                                PartNumberChars[3] = payload[1];
                                break;
                            //case 0x05:
                            //    DescriptionToInsert += " | EMISSION STANDARD SUPPLEMENT";

                            //    switch (payload[1])
                            //    {
                            //        case 0x01:
                            //            ValueToInsert = "CA/NY/MA STATE TLEV MOD";
                            //            break;
                            //        case 0x02:
                            //            ValueToInsert = "CA/NY/MA STATE MODULE";
                            //            break;
                            //        case 0x03:
                            //            ValueToInsert = "CA/MA/FED HIGH ALT MOD";
                            //            break;
                            //        case 0x04:
                            //            ValueToInsert = "CA/MA STATE MODULE";
                            //            break;
                            //        case 0x05:
                            //            ValueToInsert = "CA/NY/FED HIGH ALT MOD";
                            //            break;
                            //        case 0x06:
                            //            ValueToInsert = "CA/NY MODULE";
                            //            break;
                            //        case 0x07:
                            //            ValueToInsert = "FEDERAL LOW ALT MODULE";
                            //            break;
                            //        case 0x08:
                            //            ValueToInsert = "FED LOW ALT/TAIWAN MOD";
                            //            break;
                            //        case 0x09:
                            //            ValueToInsert = "CA/NY/MA STATE LEV MOD";
                            //            break;
                            //        case 0x0A:
                            //            ValueToInsert = "CA/NY/MA/CT STATE MOD";
                            //            break;
                            //        case 0x0B:
                            //            ValueToInsert = "CA/NY/MA/CT TLEV MOD";
                            //            break;
                            //        case 0x0C:
                            //            ValueToInsert = "CA/NY/MA/CT LEV MOD";
                            //            break;
                            //        default:
                            //            ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                            //            break;
                            //    }
                            //    break;
                            case 0x06:
                                DescriptionToInsert += " | EMISSION STANDARD";

                                switch (payload[1])
                                {
                                    case 0x00:
                                        ValueToInsert = "FEDERAL HIGH ALTITUDE";
                                        break;
                                    case 0x01:
                                        ValueToInsert = "TRUCK MODULE";
                                        break;
                                    case 0x02:
                                        ValueToInsert = "MEXICAN MODULE";
                                        break;
                                    case 0x03:
                                        ValueToInsert = "CA/NY/MA/CT STATE";
                                        break;
                                    case 0x04:
                                        ValueToInsert = "FEDERAL/CANADIAN";
                                        break;
                                    case 0x05:
                                        ValueToInsert = "BUX/ECE";
                                        break;
                                    case 0x06:
                                        ValueToInsert = "GULF STATES";
                                        break;
                                    case 0x07:
                                        ValueToInsert = "50 STATE/CANADIAN";
                                        break;
                                    case 0x08:
                                        ValueToInsert = "TRANSITORY LOW-EM (NBT)";
                                        break;
                                    case 0x09:
                                        ValueToInsert = "LOW EMISSION VEH (NBV)";
                                        break;
                                    case 0x0A:
                                        ValueToInsert = "CARB OBD2 TRUCK MODULE";
                                        break;
                                    case 0x0B:
                                        ValueToInsert = "EPA FEDERAL OBD TRUCK M";
                                        break;
                                    case 0x0C:
                                        ValueToInsert = "HEAVY DUTY TRUCK MODULE";
                                        break;
                                    case 0x0D:
                                        ValueToInsert = "CANADIAN ONLY";
                                        break;
                                    case 0x0E:
                                        ValueToInsert = "FEDERAL ONLY";
                                        break;
                                    case 0x0F:
                                        ValueToInsert = "50 STATE ONLY";
                                        break;
                                    case 0x10:
                                        ValueToInsert = "ZERO EMISSION VEH (NBZ)";
                                        break;
                                    case 0x11:
                                        ValueToInsert = "ULTRA LOW-EM VEH (NBU)";
                                        break;
                                    case 0x12:
                                        ValueToInsert = "JAPAN EMISSIONS (NGJ)";
                                        break;
                                    case 0x13:
                                        ValueToInsert = "EURO STAGE 3 OBD (NB3)";
                                        break;
                                    case 0x14:
                                        ValueToInsert = "NATIONAL LOW EMS (NLEV)";
                                        break;
                                    case 0x15:
                                        ValueToInsert = "EURO STAGE 2 (NB2)";
                                        break;
                                    case 0x16:
                                        ValueToInsert = "EURO STAGE 4 (NB4)";
                                        break;
                                    case 0x17:
                                        ValueToInsert = "SUPER ULTRA LO-EM (NBS)";
                                        break;
                                    case 0x18:
                                        ValueToInsert = "ENHARENTLY LOW-EM (NBI)";
                                        break;
                                    case 0x19:
                                        ValueToInsert = "PARTIAL ZERO-EM (NBP)";
                                        break;
                                    default:
                                        ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                                        break;
                                }

                                EngineToolsStatusBarTextItems[6] = ValueToInsert;
                                break;
                            case 0x07:
                                DescriptionToInsert += " | CHASSIS TYPE";

                                switch (payload[1])
                                {
                                    case 0x01:
                                        ValueToInsert = "MTX RWD HEAVY DUTY";
                                        break;
                                    case 0x02:
                                        ValueToInsert = "ATX RWD HEAVY DUTY";
                                        break;
                                    case 0x03:
                                        ValueToInsert = "MTX RWD";
                                        break;
                                    case 0x04:
                                        ValueToInsert = "ATX RWD";
                                        break;
                                    case 0x05:
                                        ValueToInsert = "MTX FWD HEAVY DUTY";
                                        break;
                                    case 0x06:
                                        ValueToInsert = "ATX FWD HEAVY DUTY";
                                        break;
                                    case 0x07:
                                        ValueToInsert = "MTX FWD";
                                        break;
                                    case 0x08:
                                        ValueToInsert = "ATX FWD";
                                        break;
                                    case 0x09:
                                        ValueToInsert = "MTX AWD HEAVY DUTY";
                                        break;
                                    case 0x0A:
                                        ValueToInsert = "ATX AWD HEAVY DUTY";
                                        break;
                                    case 0x0B:
                                        ValueToInsert = "MTX AWD";
                                        break;
                                    case 0x0C:
                                        ValueToInsert = "ATX AWD";
                                        break;
                                    case 0x0D:
                                        ValueToInsert = "4X2 MTX";
                                        break;
                                    case 0x0E:
                                        ValueToInsert = "4X2 ATX";
                                        break;
                                    case 0x0F:
                                        ValueToInsert = "4X4 MTX";
                                        break;
                                    case 0x10:
                                        ValueToInsert = "4X4 ATX";
                                        break;
                                    default:
                                        ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                                        break;
                                }

                                EngineToolsStatusBarTextItems[9] = ValueToInsert;
                                break;
                            case 0x08:
                                DescriptionToInsert += " | ASPIRATION TYPE";

                                switch (payload[1])
                                {
                                    case 0x01:
                                        ValueToInsert = "NATURAL";
                                        break;
                                    case 0x02:
                                        ValueToInsert = "TURBO I";
                                        break;
                                    case 0x03:
                                        ValueToInsert = "TURBO II";
                                        break;
                                    case 0x04:
                                        ValueToInsert = "TURBO III";
                                        break;
                                    case 0x05:
                                        ValueToInsert = "TURBO IV";
                                        break;
                                    case 0x06:
                                        ValueToInsert = "TURBO DIESEL";
                                        break;
                                    case 0x07:
                                        ValueToInsert = "TWO-STROKE";
                                        break;
                                    default:
                                        ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                                        break;
                                }

                                EngineToolsStatusBarTextItems[7] = ValueToInsert;

                                if (EngineToolsStatusBarTextItems[7] == "NATURAL") EngineToolsStatusBarTextItems[7] = "NATURAL ASPIRATION";

                                break;
                            case 0x09:
                                DescriptionToInsert += " | INJECTION TYPE";

                                switch (payload[1])
                                {
                                    case 0x01:
                                        ValueToInsert = "TBI SINGLE";
                                        break;
                                    case 0x02:
                                        ValueToInsert = "TBI DOUBLE";
                                        break;
                                    case 0x03:
                                        ValueToInsert = "MPI BANKED";
                                        break;
                                    case 0x04:
                                        ValueToInsert = "SFI";
                                        break;
                                    case 0x05:
                                        ValueToInsert = "DIRECT";
                                        break;
                                    default:
                                        ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                                        break;
                                }

                                EngineToolsStatusBarTextItems[5] = ValueToInsert;

                                if (EngineToolsStatusBarTextItems[5] == "DIRECT") EngineToolsStatusBarTextItems[5] = "DIRECT INJ";

                                break;
                            case 0x0A:
                                DescriptionToInsert += " | FUEL TYPE";

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
                                        ValueToInsert = "SENSORLESS FLEX";
                                        break;
                                    case 0x07:
                                        ValueToInsert = "CNG";
                                        break;
                                    case 0x08:
                                        ValueToInsert = "LEAD-ACID ELECTRIC";
                                        break;
                                    case 0x09:
                                        ValueToInsert = "NIMH ELECTRIC";
                                        break;
                                    default:
                                        ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                                        break;
                                }

                                EngineToolsStatusBarTextItems[4] = ValueToInsert;
                                break;
                            case 0x0B:
                                DescriptionToInsert += " | MODEL YEAR";

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
                                        ValueToInsert = "1994 1/2";
                                        break;
                                    case 0x0F:
                                        ValueToInsert = "1991-1995";
                                        break;
                                    default:
                                        ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                                        break;
                                }

                                EngineToolsStatusBarTextItems[0] = ValueToInsert;
                                Year = 1990 + payload[1];
                                break;
                            case 0x0C:
                                DescriptionToInsert += " | ENGINE DISPLACEMENT AND CYL ORIENT.";

                                switch (payload[1])
                                {
                                    case 0x01:
                                        ValueToInsert = "2.2L I4 E-W";
                                        break;
                                    case 0x02:
                                        ValueToInsert = "2.5L I4 E-W";
                                        break;
                                    case 0x03:
                                        ValueToInsert = "3.0L V6 E-W";
                                        break;
                                    case 0x04:
                                        ValueToInsert = "3.3L V6 E-W";
                                        break;
                                    case 0x05:
                                        ValueToInsert = "3.9L V6 N-S";
                                        break;
                                    case 0x06:
                                        ValueToInsert = "5.2L V8 N-S";
                                        break;
                                    case 0x07:
                                        ValueToInsert = "5.9L V8 N-S";
                                        break;
                                    case 0x08:
                                        ValueToInsert = "3.8L V6 E-W";
                                        break;
                                    case 0x09:
                                        ValueToInsert = "4.0L I6 N-S";
                                        break;
                                    case 0x0A:
                                        ValueToInsert = "2.0L I4 E-W SOHC";
                                        break;
                                    case 0x0B:
                                        ValueToInsert = "3.5L V6 N-S";
                                        break;
                                    case 0x0C:
                                        ValueToInsert = "8.0L V10 N-S";
                                        break;
                                    case 0x0D:
                                        ValueToInsert = "2.4L I4 E-W";
                                        break;
                                    case 0x0E:
                                        ValueToInsert = "2.5L I4 N-S";
                                        break;
                                    case 0x0F:
                                        ValueToInsert = "2.5L V6 N-S";
                                        break;
                                    case 0x10:
                                        ValueToInsert = "2.0L I4 E-W DOHC";
                                        break;
                                    case 0x11:
                                        ValueToInsert = "2.5L V6 E-W";
                                        break;
                                    case 0x12:
                                        ValueToInsert = "5.9L I6 N-S";
                                        break;
                                    case 0x13:
                                        ValueToInsert = "3.3L V6 N-S";
                                        break;
                                    case 0x14:
                                        ValueToInsert = "2.7L V6 N-S";
                                        break;
                                    case 0x15:
                                        ValueToInsert = "3.2L V6 N-S";
                                        break;
                                    case 0x16:
                                        ValueToInsert = "1.8L I4 E-W";
                                        break;
                                    case 0x17:
                                        ValueToInsert = "3.7L V6 N-S";
                                        break;
                                    case 0x18:
                                        ValueToInsert = "4.7L V8 N-S";
                                        break;
                                    case 0x19:
                                        ValueToInsert = "1.9L I4 E-W";
                                        break;
                                    case 0x1A:
                                        ValueToInsert = "3.1L I5 N-S";
                                        break;
                                    case 0x1B:
                                        ValueToInsert = "1.6L I4 E-W";
                                        break;
                                    case 0x1C:
                                        ValueToInsert = "2.7L V6 E-W";
                                        break;
                                    default:
                                        ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                                        break;
                                }

                                EngineToolsStatusBarTextItems[3] = ValueToInsert;
                                break;
                            case 0x0D:
                                DescriptionToInsert += " | COOLING FAN";

                                switch (payload[1])
                                {
                                    case 0x01:
                                        ValueToInsert = "NO ELECTRIC FAN";
                                        break;
                                    case 0x02:
                                        ValueToInsert = "SINGLE FAN SINGLE SPEED";
                                        break;
                                    case 0x03:
                                        ValueToInsert = "SINGLE FAN TWO SPEED";
                                        break;
                                    case 0x04:
                                        ValueToInsert = "SINGLE FAN VAR SPEED";
                                        break;
                                    case 0x05:
                                        ValueToInsert = "TWO FANS SINGLE SPEED";
                                        break;
                                    case 0x06:
                                        ValueToInsert = "TWO FANS TWO SPEED";
                                        break;
                                    case 0x07:
                                        ValueToInsert = "TWO FANS VAR SPEED";
                                        break;
                                    case 0x08:
                                        ValueToInsert = "THREE FANS SINGLE SPD";
                                        break;
                                    case 0x09:
                                        ValueToInsert = "THREE FANS TWO SPEED";
                                        break;
                                    case 0x0A:
                                        ValueToInsert = "THREE FANS VAR SPEED";
                                        break;
                                    case 0x0B:
                                        ValueToInsert = "AUX COOLING FAN";
                                        break;
                                    case 0x0C:
                                        ValueToInsert = "SINGLE FAN VAR HYDR";
                                        break;
                                    default:
                                        ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                                        break;
                                }

                                EngineToolsStatusBarTextItems[8] = ValueToInsert;
                                break;
                            case 0x0E:
                                DescriptionToInsert += " | ENGINE MANUFACTURER";

                                switch (payload[1])
                                {
                                    case 0x01:
                                        ValueToInsert = "CHRYSLER";
                                        break;
                                    case 0x02:
                                        ValueToInsert = "JEEP";
                                        break;
                                    case 0x03:
                                        ValueToInsert = "MITSUBISHI";
                                        break;
                                    case 0x04:
                                        ValueToInsert = "LOTUS";
                                        break;
                                    case 0x05:
                                        ValueToInsert = "DITOMASO";
                                        break;
                                    case 0x06:
                                        ValueToInsert = "PRV";
                                        break;
                                    case 0x07:
                                        ValueToInsert = "CUMMINS";
                                        break;
                                    case 0x08:
                                        ValueToInsert = "NORTHRUP/GRUMMAN";
                                        break;
                                    case 0x09:
                                        ValueToInsert = "VM MOTORI";
                                        break;
                                    case 0x0A:
                                        ValueToInsert = "MERCEDES DIESEL";
                                        break;
                                    default:
                                        ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                                        break;
                                }

                                EngineToolsStatusBarTextItems[2] = ValueToInsert;
                                break;
                            case 0x0F:
                                DescriptionToInsert += " | CONTROLLER HARDWARE TYPE";

                                switch (payload[1])
                                {
                                    case 0x01:
                                        ValueToInsert = "FCC";
                                        break;
                                    case 0x02:
                                        ValueToInsert = "SBEC1";
                                        break;
                                    case 0x03:
                                        ValueToInsert = "SBEC2";
                                        break;
                                    case 0x04:
                                        ValueToInsert = "SBEC2A";
                                        break;
                                    case 0x05:
                                        ValueToInsert = "SBEC3";
                                        break;
                                    case 0x06:
                                        ValueToInsert = "JTEC";
                                        break;
                                    case 0x07:
                                        ValueToInsert = "SBEC3A";
                                        break;
                                    case 0x08:
                                        ValueToInsert = "SBEC3+";
                                        break;
                                    case 0x09:
                                        ValueToInsert = "CUMMINS";
                                        break;
                                    case 0x0A:
                                        ValueToInsert = "BOSCH";
                                        break;
                                    case 0x0B:
                                        ValueToInsert = "NORTHROP EV SCU";
                                        break;
                                    case 0x0C:
                                        ValueToInsert = "JTEC+";
                                        break;
                                    case 0x0D:
                                        ValueToInsert = "JTEC (TCM ONLY)";
                                        break;
                                    case 0x0E:
                                        ValueToInsert = "JTEC+ (TCM ONLY)";
                                        break;
                                    case 0x0F:
                                        ValueToInsert = "BOSCH EDC15-V";
                                        break;
                                    case 0x10:
                                        ValueToInsert = "BOSCH EDC15-C5";
                                        break;
                                    case 0x11:
                                        ValueToInsert = "SIEMENS SIM-70";
                                        break;
                                    case 0x12:
                                        ValueToInsert = "SBEC3A+";
                                        break;
                                    case 0x13:
                                        ValueToInsert = "SBEC3B";
                                        break;
                                    case 0x14:
                                        ValueToInsert = "GENERIC JTEC";
                                        break;
                                    case 0x15:
                                        ValueToInsert = "CUMMINS 845";
                                        break;
                                    case 0x16:
                                        ValueToInsert = "CUMMINS 846";
                                        break;
                                    case 0x17:
                                        ValueToInsert = "GENERIC CUMMINS";
                                        break;
                                    case 0x18:
                                        ValueToInsert = "CUMMINS 848";
                                        break;
                                    case 0x19:
                                        ValueToInsert = "EDC16-C2";
                                        break;
                                    case 0x1A:
                                        ValueToInsert = "";
                                        break;
                                    case 0x1B:
                                        ValueToInsert = "NGC";
                                        break;
                                    case 0x1C:
                                        ValueToInsert = "EDC16-C2";
                                        break;
                                    case 0x1D:
                                        ValueToInsert = "CUMMINS 2";
                                        break;
                                    default:
                                        ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                                        break;
                                }

                                ControllerHardwareType = payload[1];
                                break;
                            case 0x10:
                                DescriptionToInsert += " | BODY STYLE";

                                switch (payload[1])
                                {
                                    case 0x01:
                                        ValueToInsert = "YJ";
                                        break;
                                    case 0x02:
                                        ValueToInsert = "XJ";
                                        break;
                                    case 0x03:
                                        ValueToInsert = "ZJ/ZG";
                                        break;
                                    case 0x04:
                                        ValueToInsert = "FJ";
                                        break;
                                    case 0x05:
                                        ValueToInsert = "PL";
                                        break;
                                    case 0x06:
                                        ValueToInsert = "JA";
                                        break;
                                    case 0x07:
                                        ValueToInsert = "AA/AG/AJ/AP";
                                        break;
                                    case 0x08:
                                        ValueToInsert = "AC/AY";
                                        break;
                                    case 0x09:
                                        ValueToInsert = "AS/ES";
                                        break;
                                    case 0x0A:
                                        ValueToInsert = "LH";
                                        break;
                                    case 0x0B:
                                        ValueToInsert = "NS/GS";
                                        break;
                                    case 0x0C:
                                        ValueToInsert = "AB";
                                        break;
                                    case 0x0D:
                                        ValueToInsert = "AN";
                                        break;
                                    case 0x0E:
                                        ValueToInsert = "BR";
                                        break;
                                    case 0x0F:
                                        ValueToInsert = "SR";
                                        break;
                                    case 0x10:
                                        ValueToInsert = "AN/BR";
                                        break;
                                    case 0x11:
                                        ValueToInsert = "AN/AB";
                                        break;
                                    case 0x12:
                                        ValueToInsert = "JX";
                                        break;
                                    case 0x13:
                                        ValueToInsert = "PR";
                                        break;
                                    case 0x14:
                                        ValueToInsert = "TJ";
                                        break;
                                    case 0x15:
                                        ValueToInsert = "DN";
                                        break;
                                    case 0x16:
                                        ValueToInsert = "WJ/WG";
                                        break;
                                    case 0x17:
                                        ValueToInsert = "SJ";
                                        break;
                                    case 0x18:
                                        ValueToInsert = "JR";
                                        break;
                                    case 0x19:
                                        ValueToInsert = "PT";
                                        break;
                                    case 0x1A:
                                        ValueToInsert = "RS/RG";
                                        break;
                                    case 0x1B:
                                        ValueToInsert = "KJ";
                                        break;
                                    case 0x1C:
                                        ValueToInsert = "DR";
                                        break;
                                    case 0x1D:
                                        ValueToInsert = "F24S/FJ22";
                                        break;
                                    case 0x1E:
                                        ValueToInsert = "FJ22";
                                        break;
                                    case 0x1F:
                                        ValueToInsert = "AA/AJ/AP";
                                        break;
                                    default:
                                        ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                                        break;
                                }

                                EngineToolsStatusBarTextItems[1] = ValueToInsert;
                                break;
                            case 0x11:
                                DescriptionToInsert += " | MODULE SOFTWARE PHASE";
                                ValueToInsert = payload[1].ToString("0");
                                break;
                            case 0x12:
                                DescriptionToInsert += " | MODULE SOFTWARE VERSION";
                                ValueToInsert = payload[1].ToString("0");
                                break;
                            case 0x13:
                                DescriptionToInsert += " | MODULE SOFTWARE FAMILY";
                                ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                                break;
                            case 0x14:
                                DescriptionToInsert += " | MODULE SOFTWARE GROUP AND MONTH";

                                switch (payload[1] & 0x0F)
                                {
                                    case 0x01:
                                        ValueToInsert = "JANUARY";
                                        break;
                                    case 0x02:
                                        ValueToInsert = "FEBRUARY";
                                        break;
                                    case 0x03:
                                        ValueToInsert = "MARCH";
                                        break;
                                    case 0x04:
                                        ValueToInsert = "APRIL";
                                        break;
                                    case 0x05:
                                        ValueToInsert = "MAY";
                                        break;
                                    case 0x06:
                                        ValueToInsert = "JUNE";
                                        break;
                                    case 0x07:
                                        ValueToInsert = "JULY";
                                        break;
                                    case 0x08:
                                        ValueToInsert = "AUGUST";
                                        break;
                                    case 0x09:
                                        ValueToInsert = "SEPTEMBER";
                                        break;
                                    case 0x0A:
                                        ValueToInsert = "OCTOBER";
                                        break;
                                    case 0x0B:
                                        ValueToInsert = "NOVEMBER";
                                        break;
                                    case 0x0C:
                                        ValueToInsert = "DECEMBER";
                                        break;
                                    default:
                                        ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                                        break;
                                }
                                break;
                            case 0x15:
                                DescriptionToInsert += " | MODULE SOFTWARE DAY";
                                ValueToInsert = payload[1].ToString("0");
                                break;
                            case 0x16:
                                DescriptionToInsert += " | TRANSMISSION TYPE";

                                switch (payload[1])
                                {
                                    case 0x01:
                                        ValueToInsert = "ATX ?-SPEED (PTU)";
                                        break;
                                    case 0x02:
                                        ValueToInsert = "MTX";
                                        break;
                                    case 0x03:
                                        ValueToInsert = "ATX 3-SPEED";
                                        break;
                                    case 0x04:
                                        ValueToInsert = "ATX 4-SPEED";
                                        break;
                                    default:
                                        ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                                        break;
                                }

                                EngineToolsStatusBarTextItems[10] = ValueToInsert;
                                break;
                            case 0x17:
                                DescriptionToInsert += " | PART NUMBER REVISION 1";
                                ValueToInsert = Encoding.ASCII.GetString(payload, 1, 1);
                                PartNumberChars[4] = payload[1];
                                break;
                            case 0x18:
                                DescriptionToInsert += " | PART NUMBER REVISION 2";
                                ValueToInsert = Encoding.ASCII.GetString(payload, 1, 1);
                                PartNumberChars[5] = payload[1];
                                break;
                            case 0x19:
                                DescriptionToInsert += " | SOFTWARE REVISION LEVEL";
                                ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                                break;
                            case 0x1A:
                                DescriptionToInsert += " | HOMOLOGATION ID 1";
                                ValueToInsert = Util.ByteToHexString(payload, 1, 1);

                                if ((payload[1] < 0x20) || (payload[1] > 0x7E)) break;

                                ValueToInsert += " | " + Encoding.ASCII.GetString(payload, 1, 1);
                                break;
                            case 0x1B:
                                DescriptionToInsert += " | HOMOLOGATION ID 2";
                                ValueToInsert = Util.ByteToHexString(payload, 1, 1);

                                if ((payload[1] < 0x20) || (payload[1] > 0x7E)) break;

                                ValueToInsert += " | " + Encoding.ASCII.GetString(payload, 1, 1);
                                break;
                            case 0x1C:
                                DescriptionToInsert += " | HOMOLOGATION ID 3";
                                ValueToInsert = Util.ByteToHexString(payload, 1, 1);

                                if ((payload[1] < 0x20) || (payload[1] > 0x7E)) break;

                                ValueToInsert += " | " + Encoding.ASCII.GetString(payload, 1, 1);
                                break;
                            case 0x1D:
                                DescriptionToInsert += " | HOMOLOGATION ID 4";
                                ValueToInsert = Util.ByteToHexString(payload, 1, 1);

                                if ((payload[1] < 0x20) || (payload[1] > 0x7E)) break;

                                ValueToInsert += " | " + Encoding.ASCII.GetString(payload, 1, 1);
                                break;
                            case 0x1E:
                                DescriptionToInsert += " | HOMOLOGATION ID 5";
                                ValueToInsert = Util.ByteToHexString(payload, 1, 1);

                                if ((payload[1] < 0x20) || (payload[1] > 0x7E)) break;

                                ValueToInsert += " | " + Encoding.ASCII.GetString(payload, 1, 1);
                                break;
                            case 0x1F:
                                DescriptionToInsert += " | HOMOLOGATION ID 6";
                                ValueToInsert = Util.ByteToHexString(payload, 1, 1);

                                if ((payload[1] < 0x20) || (payload[1] > 0x7E)) break;

                                ValueToInsert += " | " + Encoding.ASCII.GetString(payload, 1, 1);
                                break;
                            default:
                                DescriptionToInsert += " | " + "OFFSET: " + Util.ByteToHexString(payload, 0, 1);
                                ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                                break;
                        }
                        break;
                    case 0x2B:
                        DescriptionToInsert = "GET SECURITY SEED"; // Legacy, Level 1 only! Level 2 can only be unlocked with 35 02.

                        if (message.Length < 4) break;

                        byte ChecksumB = (byte)(0x2B + payload[0] + payload[1]);

                        if (payload[2] != ChecksumB)
                        {
                            ValueToInsert = "CHECKSUM ERROR";
                            break;
                        }

                        ushort SeedA = (ushort)((payload[0] << 8) + payload[1]);

                        if (SeedA == 0)
                        {
                            DescriptionToInsert += " | PCM ALREADY UNLOCKED";
                            break;
                        }

                        ushort KeyA = (ushort)((SeedA << 2) + 0x9018);
                        byte KeyAHB = (byte)(KeyA >> 8);
                        byte KeyALB = (byte)(KeyA);
                        byte KeyAChecksum = (byte)(0x2C + KeyAHB + KeyALB);
                        byte[] KeyAArray = new byte[4] { 0x2C, KeyAHB, KeyALB, KeyAChecksum };
                        DescriptionToInsert += " | KEY: " + Util.ByteToHexStringSimple(KeyAArray);
                        ValueToInsert = Util.ByteToHexString(payload, 0, 2);
                        break;
                    case 0x2C:
                        DescriptionToInsert = "SEND SECURITY KEY";

                        if (message.Length < 5) break;

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
                        break;
                    case 0x2D:
                        DescriptionToInsert = "READ CONFIGURATION CONSTANT";

                        if (message.Length < 5) break;

                        DescriptionToInsert = "CONFIGURATION | PAGE: " + Util.ByteToHexString(payload, 0, 1) + " | ITEM: " + Util.ByteToHexString(payload, 1, 1);
                        ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                        break;
                    case 0x2E: // SBEC
                    case 0x33: // CUMMINS
                        DescriptionToInsert = "ONE-TRIP FAULT CODE LIST";

                        if (message.Length < 3) break;

                        byte ChecksumD = 0;
                        byte ChecksumDLocation = (byte)(message.Length - 1);

                        for (int i = 0; i < ChecksumDLocation; i++)
                        {
                            ChecksumD += message[i];
                        }

                        if ((ChecksumD != message[ChecksumDLocation]) && ((ChecksumD - 0x1E) != message[ChecksumDLocation]))
                        {
                            FaultCode1TList.Clear();
                            ValueToInsert = "CHECKSUM ERROR";
                            FaultCodes1TSaved = true;
                            break;
                        }

                        if ((ChecksumD - 0x1E) == message[ChecksumDLocation])
                        {
                            DescriptionToInsert += " | CHECKSUM BUG";
                        }

                        FaultCode1TList.Clear();
                        FaultCode1TList.AddRange(FaultCode1TPayload);
                        FaultCode1TList.Remove(0xFD); // not fault code related
                        FaultCode1TList.Remove(0xFE); // end of fault code list signifier

                        if (FaultCode1TList.Count == 0)
                        {
                            FaultCode1TList.Clear();
                            ValueToInsert = "NO FAULT CODE";
                            FaultCodes1TSaved = false;
                            break;
                        }

                        ValueToInsert = Util.ByteToHexStringSimple(FaultCode1TList.ToArray());
                        FaultCodes1TSaved = false;
                        break;
                    case 0x35:
                        DescriptionToInsert = "GET SECURITY SEED";

                        if (message.Length < 5) break;

                        byte ChecksumE = (byte)(0x35 + payload[0] + payload[1] + payload[2]);

                        if (payload[3] != ChecksumE)
                        {
                            if (payload[0] == 1)
                            {
                                DescriptionToInsert += " #1";
                            }
                            else if (payload[0] == 2)
                            {
                                DescriptionToInsert += " #2";
                            }

                            ValueToInsert = "CHECKSUM ERROR";
                            break;
                        }

                        ushort SeedB = (ushort)((payload[1] << 8) + payload[2]);

                        if (payload[0] == 1)
                        {
                            if (SeedB == 0)
                            {
                                DescriptionToInsert += " #1 | PCM ALREADY UNLOCKED";
                                break;
                            }

                            ushort KeyB = (ushort)((SeedB << 2) + 0x9018);
                            byte KeyBHB = (byte)(KeyB >> 8);
                            byte KeyBLB = (byte)(KeyB);
                            byte KeyBChecksum = (byte)(0x2C + KeyBHB + KeyBLB);
                            byte[] KeyBArray = new byte[4] { 0x2C, KeyBHB, KeyBLB, KeyBChecksum };

                            DescriptionToInsert += " #1 | KEY: " + Util.ByteToHexStringSimple(KeyBArray);
                        }
                        else if (payload[0] == 2)
                        {
                            if (SeedB == 0)
                            {
                                DescriptionToInsert += " #2 | PCM ALREADY UNLOCKED";
                                break;
                            }

                            ushort KeyC = (ushort)(SeedB & 0xFF00);
                            KeyC |= (ushort)(KeyC >> 8);
                            ushort mask = (ushort)(SeedB & 0xFF);
                            mask |= (ushort)(mask << 8);
                            KeyC ^= 0x9340; // polinom
                            KeyC += 0x1010;
                            KeyC ^= mask;
                            KeyC += 0x1911;
                            uint tmp = (uint)((KeyC << 16) | KeyC);
                            KeyC += (ushort)(tmp >> 3);
                            byte KeyVHB = (byte)(KeyC >> 8);
                            byte KeyVLB = (byte)(KeyC);
                            byte KeyVChecksum = (byte)(0x2C + KeyVHB + KeyVLB);
                            byte[] KeyCArray = new byte[4] { 0x2C, KeyVHB, KeyVLB, KeyVChecksum };

                            DescriptionToInsert += " #2 | KEY: " + Util.ByteToHexStringSimple(KeyCArray);
                        }

                        ValueToInsert = Util.ByteToHexString(payload, 1, 2);
                        break;
                    case 0x36:
                        // TODO: OBD2 diagnostics
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
                List<byte> HSOffset = new List<byte>();
                List<byte> HSValues = new List<byte>();
                List<byte> BootstrapOffset = new List<byte>();
                List<byte> BootstrapLength = new List<byte>();
                List<byte> BootstrapValues = new List<byte>();
                List<byte> OffsetStart = new List<byte>();
                List<byte> OffsetEnd = new List<byte>();

                ushort HSBPNum = (ushort)(payload.Length / 2); // number of byte pairs (offset value)
                ushort BlockSize;
                ushort EchoCount;

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

                        if (message.Length < 3) break;

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

                        if (message.Length < 5) break;

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
                        break;
                    case 0x26:
                        DescriptionToInsert = "BOOTSTRAP SECURITY STATUS";

                        if (message.Length < 5) break;

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
                        break;
                    case 0x31:
                        DescriptionToInsert = "WRITE FLASH BLOCK";

                        if (message.Length < 7) break;

                        BootstrapOffset.AddRange(payload.Take(3));
                        BootstrapLength.AddRange(payload.Skip(3).Take(2));
                        BootstrapValues.AddRange(payload.Skip(5));

                        BlockSize = (ushort)((payload[3] << 8) + payload[4]);
                        EchoCount = (ushort)(payload.Length - 5);

                        DescriptionToInsert = "WRITE FLASH BLOCK | OFFSET: " + Util.ByteToHexStringSimple(BootstrapOffset.ToArray()) + " | SIZE: " + Util.ByteToHexStringSimple(BootstrapLength.ToArray());

                        if (EchoCount == BlockSize)
                        {
                            ValueToInsert = Util.ByteToHexStringSimple(BootstrapValues.ToArray());
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
                        break;
                    case 0x34:
                    case 0x46: // Dino
                        DescriptionToInsert = "READ FLASH BLOCK";

                        if (message.Length < 7) break;

                        BootstrapOffset.AddRange(payload.Take(3));
                        BootstrapLength.AddRange(payload.Skip(3).Take(2));
                        BootstrapValues.AddRange(payload.Skip(5));

                        DescriptionToInsert = "READ FLASH BLOCK | OFFSET: " + Util.ByteToHexStringSimple(BootstrapOffset.ToArray()) + " | SIZE: " + Util.ByteToHexStringSimple(BootstrapLength.ToArray());

                        BlockSize = (ushort)((payload[3] << 8) + payload[4]);
                        EchoCount = (ushort)(payload.Length - 5);

                        if (EchoCount == BlockSize)
                        {
                            ValueToInsert = Util.ByteToHexStringSimple(BootstrapValues.ToArray());
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
                        break;
                    case 0x37:
                        DescriptionToInsert = "WRITE EEPROM BLOCK";

                        if (message.Length < 6) break;

                        BootstrapOffset.AddRange(payload.Take(2));
                        BootstrapLength.AddRange(payload.Skip(2).Take(2));
                        BootstrapValues.AddRange(payload.Skip(4));

                        DescriptionToInsert = "WRITE EEPROM BLOCK | OFFSET: " + Util.ByteToHexStringSimple(BootstrapOffset.ToArray()) + " | SIZE: " + Util.ByteToHexStringSimple(BootstrapLength.ToArray());

                        BlockSize = (ushort)((payload[2] << 8) + payload[3]);
                        EchoCount = (ushort)(payload.Length - 4);

                        if ((EchoCount == BlockSize) && (BootstrapOffset[0] < 2))
                        {
                            ValueToInsert = Util.ByteToHexStringSimple(BootstrapValues.ToArray());
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
                        break;
                    case 0x3A:
                        DescriptionToInsert = "READ EEPROM BLOCK";

                        if (message.Length < 6) break;

                        BootstrapOffset.AddRange(payload.Take(2));
                        BootstrapLength.AddRange(payload.Skip(2).Take(2));
                        BootstrapValues.AddRange(payload.Skip(4));

                        DescriptionToInsert = "READ EEPROM BLOCK | OFFSET: " + Util.ByteToHexStringSimple(BootstrapOffset.ToArray()) + " | SIZE: " + Util.ByteToHexStringSimple(BootstrapLength.ToArray());

                        BlockSize = (ushort)((payload[2] << 8) + payload[3]);
                        EchoCount = (ushort)(payload.Length - 4);

                        if ((EchoCount == BlockSize) && (BootstrapOffset[0] < 2))
                        {
                            ValueToInsert = Util.ByteToHexStringSimple(BootstrapValues.ToArray());
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
                        break;
                    case 0x47:
                        DescriptionToInsert = "START BOOTLOADER";

                        if (message.Length >= 4)
                        {
                            BootstrapOffset.AddRange(payload.Take(2));
                            DescriptionToInsert = "START BOOTLOADER | OFFSET: " + Util.ByteToHexStringSimple(BootstrapOffset.ToArray());

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

                        if (message.Length < 6) break;

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
                        break;
                    case 0xDB:
                        DescriptionToInsert = string.Empty;

                        if (message.Length < 5) break;

                        if (payload[0] == 0x2F && payload[1] == 0xD8 && payload[2] == 0x3E && payload[3] == 0x23)
                        {
                            DescriptionToInsert = "BOOTSTRAP MODE NOT PROTECTED";
                        }
                        else
                        {
                            DescriptionToInsert = "PING";
                        }
                        break;
                    case 0xF0:
                        DescriptionToInsert = "F0 RAM TABLE SELECTED";

                        if (message.Length < 3) break;

                        switch (payload[0])
                        {
                            default:
                                for (int i = 0; i < HSBPNum; i++)
                                {
                                    HSOffset.Add(payload[i * 2]);
                                    HSValues.Add(payload[(i * 2) + 1]);
                                }

                                DescriptionToInsert = "F0 RAM TABLE | OFFSET: " + Util.ByteToHexStringSimple(HSOffset.ToArray());
                                ValueToInsert = Util.ByteToHexStringSimple(HSValues.ToArray());
                                break;
                        }
                        break;
                    case 0xF1:
                        DescriptionToInsert = "F1 RAM TABLE SELECTED";

                        if (message.Length < 3) break;

                        switch (payload[0])
                        {
                            default:
                                for (int i = 0; i < HSBPNum; i++)
                                {
                                    HSOffset.Add(payload[i * 2]);
                                    HSValues.Add(payload[(i * 2) + 1]);
                                }

                                DescriptionToInsert = "F1 RAM TABLE | OFFSET: " + Util.ByteToHexStringSimple(HSOffset.ToArray());
                                ValueToInsert = Util.ByteToHexStringSimple(HSValues.ToArray());
                                break;
                        }
                        break;
                    case 0xF2:
                        DescriptionToInsert = "F2 RAM TABLE SELECTED";

                        if (message.Length < 3) break;

                        switch (payload[0])
                        {
                            default:
                                for (int i = 0; i < HSBPNum; i++)
                                {
                                    HSOffset.Add(payload[i * 2]);
                                    HSValues.Add(payload[(i * 2) + 1]);
                                }

                                DescriptionToInsert = "F2 RAM TABLE | OFFSET: " + Util.ByteToHexStringSimple(HSOffset.ToArray());
                                ValueToInsert = Util.ByteToHexStringSimple(HSValues.ToArray());
                                break;
                        }
                        break;
                    case 0xF3:
                        DescriptionToInsert = "F3 RAM TABLE SELECTED";

                        if (message.Length < 3) break;

                        switch (payload[0])
                        {
                            default:
                                for (int i = 0; i < HSBPNum; i++)
                                {
                                    HSOffset.Add(payload[i * 2]);
                                    HSValues.Add(payload[(i * 2) + 1]);
                                }

                                DescriptionToInsert = "F3 RAM TABLE | OFFSET: " + Util.ByteToHexStringSimple(HSOffset.ToArray());
                                ValueToInsert = Util.ByteToHexStringSimple(HSValues.ToArray());
                                break;
                        }
                        break;
                    case 0xF4:
                        DescriptionToInsert = "F4 RAM TABLE SELECTED";

                        if (message.Length < 3) break;

                        switch (payload[0])
                        {
                            case 0x01:
                                DescriptionToInsert = "DTC 1: ";

                                if (payload[1] == 0) ValueToInsert = "EMPTY SLOT";
                                else
                                {
                                    int index = SBEC3EngineDTC.Rows.IndexOf(SBEC3EngineDTC.Rows.Find(payload[1]));

                                    if (index > -1) // DTC description found
                                    {
                                        DescriptionToInsert += SBEC3EngineDTC.Rows[index]["description"];
                                    }
                                    else // no DTC description found
                                    {
                                        DescriptionToInsert += "UNRECOGNIZED DTC";
                                    }
                                }
                                break;
                            case 0x02:
                                DescriptionToInsert = "DTC 8: ";

                                if (payload[1] == 0) ValueToInsert = "EMPTY SLOT";
                                else
                                {
                                    int index = SBEC3EngineDTC.Rows.IndexOf(SBEC3EngineDTC.Rows.Find(payload[1]));

                                    if (index > -1) // DTC description found
                                    {
                                        DescriptionToInsert += SBEC3EngineDTC.Rows[index]["description"];
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

                                if (message.Length < 5) break;

                                if (payload[2] != (payload[0] + 1))
                                {
                                    DescriptionToInsert += " | ERROR: REQUEST F4 0A 0B";
                                    break;
                                }

                                double EngineSpeed = ((payload[1] << 8) + payload[3]) * 0.125;

                                ValueToInsert = Math.Round(EngineSpeed, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "RPM";
                                break;
                            case 0x0B:
                                DescriptionToInsert = "ENGINE SPEED | ERROR: REQUEST F4 0A 0B";
                                break;
                            case 0x0C:
                                DescriptionToInsert = "VEHICLE SPEED";

                                if (message.Length < 5) break;

                                if (payload[2] != (payload[0] + 1))
                                {
                                    DescriptionToInsert += " | ERROR: REQUEST F4 0C 0D";
                                    break;
                                }

                                double VehicleSpeedMPH = ((payload[1] << 8) + payload[3]) * 0.015625;
                                double VehicleSpeedKMH = VehicleSpeedMPH * 1.609344;

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
                                break;
                            case 0x0D:
                                DescriptionToInsert = "VEHICLE SPEED | ERROR: REQUEST F4 0C 0D";
                                break;
                            case 0x0E:
                                DescriptionToInsert = "CRUISE | BUTTON PRESSED";

                                List<string> SwitchList = new List<string>();

                                if (Util.IsBitSet(payload[1], 7)) SwitchList.Add("BRAKE");
                                if (Util.IsBitSet(payload[1], 6)) SwitchList.Add("-6-");
                                if (Util.IsBitSet(payload[1], 5)) SwitchList.Add("-5-");
                                if (Util.IsBitSet(payload[1], 4)) SwitchList.Add("ON/OFF");
                                if (Util.IsBitSet(payload[1], 3)) SwitchList.Add("ACC/RES");
                                if (Util.IsBitClear(payload[1], 2)) SwitchList.Add("SET");
                                if (Util.IsBitSet(payload[1], 1)) SwitchList.Add("COAST");
                                if (Util.IsBitSet(payload[1], 0)) SwitchList.Add("CANCEL");

                                if (SwitchList.Count == 0) break;

                                foreach (string s in SwitchList)
                                {
                                    ValueToInsert += s + " | ";
                                }

                                if (ValueToInsert.Length > 2) ValueToInsert = ValueToInsert.Remove(ValueToInsert.Length - 3); // remove last "|" character
                                break;
                            case 0x0F:
                                DescriptionToInsert = "BATTERY VOLTAGE";

                                double BatteryVoltage = payload[1] * 0.0625;
                                
                                ValueToInsert = Math.Round(BatteryVoltage, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "V";
                                break;
                            case 0x10:
                                DescriptionToInsert = "AMBIENT TEMPERATURE SENSOR VOLTAGE";

                                double ATSVoltage = payload[1] * 0.0196;
                                
                                ValueToInsert = Math.Round(ATSVoltage, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "V";
                                break;
                            case 0x11:
                                DescriptionToInsert = "AMBIENT TEMPERATURE";

                                double AmbientTemperatureC = payload[1] - 128;
                                double AmbientTemperatureF = 1.8 * AmbientTemperatureC + 32.0;

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
                                DescriptionToInsert = "THROTTLE POSITION SENSOR (TPS) VOLTAGE";

                                double TPSVoltage = payload[1] * 0.0196;
                                
                                ValueToInsert = Math.Round(TPSVoltage, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "V";
                                break;
                            case 0x13:
                                DescriptionToInsert = "MINIMUM TPS VOLTAGE";

                                double MinTPSVoltage = payload[1] * 0.0196;
                                
                                ValueToInsert = Math.Round(MinTPSVoltage, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "V";
                                break;
                            case 0x14:
                                DescriptionToInsert = "CALCULATED TPS VOLTAGE";

                                double CalculatedTPSVoltageA = payload[1] * 0.0196;
                                
                                ValueToInsert = Math.Round(CalculatedTPSVoltageA, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "V";
                                break;
                            case 0x15:
                                DescriptionToInsert = "ENGINE COOLANT TEMPERATURE SENSOR VOLTAGE";

                                double ECTSensorVoltage = payload[1] * 0.0196;
                                
                                ValueToInsert = Math.Round(ECTSensorVoltage, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "V";
                                break;
                            case 0x16:
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
                            case 0x17:
                                DescriptionToInsert = "INTAKE MANIFOLD ABSOLUTE PRESSURE SENSOR VOLTAGE";

                                double MAPSensorVoltage = payload[1] * 0.0196;
                                
                                ValueToInsert = Math.Round(MAPSensorVoltage, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "V";
                                break;
                            case 0x18:
                                DescriptionToInsert = "INTAKE MANIFOLD ABSOLUTE PRESSURE";

                                double MAPPSI = payload[1] * 0.059756;
                                double MAPKPA = MAPPSI * 6.894757;

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
                                DescriptionToInsert = "BAROMETRIC PRESSURE";

                                double BarometricPressurePSI = payload[1] * 0.059756;
                                double BarometricPressureKPA = BarometricPressurePSI * 6.894757;

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
                                DescriptionToInsert = "MAP VACUUM";

                                double MAPVacuumPSI = payload[1] * 0.059756;
                                double MAPVacuumKPA = MAPVacuumPSI * 6.894757;
                                
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
                                DescriptionToInsert = "UPSTREAM O2 1/1 SENSOR VOLTAGE";

                                double O2S11Voltage = payload[1] * 0.0196;
                                
                                ValueToInsert = Math.Round(O2S11Voltage, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "V";
                                break;
                            case 0x1C:
                                DescriptionToInsert = "UPSTREAM O2 2/1 SENSOR VOLTAGE";

                                double O2S21Voltage = payload[1] * 0.0196;
                                
                                ValueToInsert = Math.Round(O2S21Voltage, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "V";
                                break;
                            case 0x1D:
                                DescriptionToInsert = "INTAKE AIR TEMPERATURE SENSOR VOLTAGE";

                                double IATVoltage = payload[1] * 0.0196;
                                
                                ValueToInsert = Math.Round(IATVoltage, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "V";
                                break;
                            case 0x1E:
                                DescriptionToInsert = "INTAKE AIR TEMPERATURE";

                                double IATC = payload[1] - 64;
                                double IATF = 1.8 * IATC + 32.0;
                                
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
                                DescriptionToInsert = "KNOCK SENSOR 1 VOLTAGE";

                                double KnockSensor1Voltage = payload[1] * 0.0196;
                                
                                ValueToInsert = Math.Round(KnockSensor1Voltage, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "V";
                                break;
                            case 0x20:
                                DescriptionToInsert = "KNOCK SENSOR 2 VOLTAGE";

                                double KnockSensor2Voltage = payload[1] * 0.0196;
                                
                                ValueToInsert = Math.Round(KnockSensor2Voltage, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "V";
                                break;
                            case 0x21:
                                DescriptionToInsert = "CRUISE | SWITCH VOLTAGE";

                                double CruiseSwitchVoltage = payload[1] * 0.0196;
                                
                                ValueToInsert = Math.Round(CruiseSwitchVoltage, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "V";
                                break;
                            case 0x22:
                                DescriptionToInsert = "BATTERY TEMPERATURE SENSOR VOLTAGE";

                                double BatteryTemperatureVoltage = payload[1] * 0.0196;
                                
                                ValueToInsert = Math.Round(BatteryTemperatureVoltage, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "V";
                                break;
                            case 0x23:
                                DescriptionToInsert = "FLEX FUEL SENSOR VOLTAGE";

                                double FlexFuelSensorVoltage = payload[1] * 0.0196;
                                
                                ValueToInsert = Math.Round(FlexFuelSensorVoltage, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "V";
                                break;
                            case 0x24:
                                DescriptionToInsert = "FLEX FUEL ETHANOL PERCENT";

                                double FlexFuelEthanolPercent = payload[1] * 0.5;
                                
                                ValueToInsert = Math.Round(FlexFuelEthanolPercent, 1).ToString("0.0").Replace(",", ".");
                                UnitToInsert = "PERCENT";
                                break;
                            case 0x25:
                                DescriptionToInsert = "A/C HIGH-SIDE PRESSURE SENSOR VOLTAGE";

                                double ACHSPressureVoltage = payload[1] * 0.0196;
                                
                                ValueToInsert = Math.Round(ACHSPressureVoltage, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "V";
                                break;
                            case 0x26:
                                DescriptionToInsert = "A/C HIGH-SIDE PRESSURE";

                                double ACHSPressurePSI = payload[1] * 1.961;
                                double ACHSPressureKPA = ACHSPressurePSI * 6.894757;
                                
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

                                if (message.Length < 5) break;

                                if (payload[2] != (payload[0] + 1))
                                {
                                    DescriptionToInsert += " | ERROR: REQUEST F4 27 28";
                                    break;
                                }

                                double InjectorPulseWidth1 = ((payload[1] << 8) + payload[3]) * 0.00390625;

                                ValueToInsert = Math.Round(InjectorPulseWidth1, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "MS";
                                break;
                            case 0x28:
                                DescriptionToInsert = "INJECTOR PULSE WIDTH 1 | ERROR: REQUEST F4 27 28";
                                break;
                            case 0x29:
                                DescriptionToInsert = "INJECTOR PULSE WIDTH 2";

                                if (message.Length < 5) break;

                                if (payload[2] != (payload[0] + 1))
                                {
                                    DescriptionToInsert += " | ERROR: REQUEST F4 29 2A";
                                    break;
                                }

                                double InjectorPulseWidth2 = ((payload[1] << 8) + payload[3]) * 0.00390625;

                                ValueToInsert = Math.Round(InjectorPulseWidth2, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "MS";
                                break;
                            case 0x2A:
                                DescriptionToInsert = "INJECTOR PULSE WIDTH 2 | ERROR: REQUEST F4 29 2A";
                                break;
                            case 0x2B:
                                DescriptionToInsert = "LONG TERM FUEL TRIM 1";

                                double LTFT1 = payload[1] * 0.196;

                                if (payload[1] >= 0x80) LTFT1 -= 50.0;

                                ValueToInsert = Math.Round(LTFT1, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "PERCENT";
                                break;
                            case 0x2C:
                                DescriptionToInsert = "LONG TERM FUEL TRIM 2";

                                double LTFT2 = payload[1] * 0.196;

                                if (payload[1] >= 0x80) LTFT2 -= 50.0;

                                ValueToInsert = Math.Round(LTFT2, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "PERCENT";
                                break;
                            case 0x2D:
                                DescriptionToInsert = "ENGINE COOLANT TEMPERATURE 2";

                                double ECTC2 = payload[1] - 128;
                                double ECTF2 = 1.8 * ECTC2 + 32.0;
                                
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
                                DescriptionToInsert = "ENGINE COOLANT TEMPERATURE 3";

                                double ECTC3 = payload[1] - 128;
                                double ECTF3 = 1.8 * ECTC3 + 32.0;
                                
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
                                DescriptionToInsert = "SPARK ADVANCE";

                                double SparkAdvance = payload[1] * 0.5;
                                
                                ValueToInsert = Math.Round(SparkAdvance, 1).ToString("0.0").Replace(",", ".");
                                UnitToInsert = "DEG";
                                break;
                            case 0x30:
                                DescriptionToInsert = "TOTAL KNOCK RETARD";

                                double TotalKnockRetard = payload[1] * 0.5;
                                
                                ValueToInsert = Math.Round(TotalKnockRetard, 1).ToString("0.0").Replace(",", ".");
                                UnitToInsert = "DEG";
                                break;
                            case 0x31:
                                DescriptionToInsert = "CYLINDER 1 RETARD";

                                double Cylinder1Retard = payload[1] * 0.5;
                                
                                ValueToInsert = Math.Round(Cylinder1Retard, 1).ToString("0.0").Replace(",", ".");
                                UnitToInsert = "DEG";
                                break;
                            case 0x32:
                                DescriptionToInsert = "CYLINDER 2 RETARD";

                                double Cylinder2Retard = payload[1] * 0.5;
                                
                                ValueToInsert = Math.Round(Cylinder2Retard, 1).ToString("0.0").Replace(",", ".");
                                UnitToInsert = "DEG";
                                break;
                            case 0x33:
                                DescriptionToInsert = "CYLINDER 3 RETARD";

                                double Cylinder3Retard = payload[1] * 0.5;
                                
                                ValueToInsert = Math.Round(Cylinder3Retard, 1).ToString("0.0").Replace(",", ".");
                                UnitToInsert = "DEG";
                                break;
                            case 0x34:
                                DescriptionToInsert = "CYLINDER 4 RETARD";

                                double Cylinder4Retard = payload[1] * 0.5;
                                
                                ValueToInsert = Math.Round(Cylinder4Retard, 1).ToString("0.0").Replace(",", ".");
                                UnitToInsert = "DEG";
                                break;
                            case 0x35:
                                DescriptionToInsert = "TARGET IDLE SPEED";

                                if (message.Length < 5) break;

                                if (payload[2] != (payload[0] + 1))
                                {
                                    DescriptionToInsert += " | ERROR: REQUEST F4 35 36";
                                    break;
                                }

                                double IdleSpeed = ((payload[1] << 8) + payload[3]) * 0.125;

                                ValueToInsert = Math.Round(IdleSpeed, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "RPM";
                                break;
                            case 0x36:
                                DescriptionToInsert = "TARGET IDLE SPEED | ERROR: REQUEST F4 35 36";
                                break;
                            case 0x37:
                                DescriptionToInsert = "TARGET IDLE AIR CONTROL MOTOR STEPS";
                                ValueToInsert = payload[1].ToString();
                                break;
                            case 0x3A:
                                DescriptionToInsert = "CHARGING VOLTAGE";

                                double ChargingVoltage = payload[1] * 0.0625;
                                
                                ValueToInsert = Math.Round(ChargingVoltage, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "V";
                                break;
                            case 0x3B:
                                DescriptionToInsert = "CRUISE | SET SPEED";

                                double CruiseSetSpeedMPH = payload[1] * 0.5;
                                double CruiseSetSpeedKMH = CruiseSetSpeedMPH * 1.609344;
                                
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

                                if (message.Length < 5) break;

                                if (payload[2] != (payload[0] + 1))
                                {
                                    DescriptionToInsert += " | ERROR: REQUEST F4 3C 3D";
                                    break;
                                }

                                ValueToInsert = Convert.ToString(payload[1], 2).PadLeft(8, '0') + " " + Convert.ToString(payload[3], 2).PadLeft(8, '0');
                                break;
                            case 0x3D:
                                DescriptionToInsert = "BIT STATE 5 | ERROR: REQUEST F4 3C 3D";
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
                                        LastCruiseCutoutReasonD = "DTC PRESENT";
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
                                        LastCruiseCutoutReasonD = "TPS LIMP-IN";
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
                                        CruiseDeniedReasonD = "DTC PRESENT";
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
                                        CruiseDeniedReasonD = "TPS LIMP-IN";
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
                                //DescriptionToInsert = "CRANKSHAFT/CAMSHAFT POSITION SENSOR SYNC STATE";

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
                                DescriptionToInsert = "SHORT TERM FUEL TRIM 1";

                                double STFT1 = payload[1] * 0.196;

                                if (payload[1] >= 0x80) STFT1 -= 50.0;

                                ValueToInsert = Math.Round(STFT1, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "PERCENT";
                                break;
                            case 0x45:
                                DescriptionToInsert = "SHORT TERM FUEL TRIM 2";

                                double STFT2 = payload[1] * 0.196;

                                if (payload[1] >= 0x80) STFT2 -= 50.0;

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
                                DescriptionToInsert = "DOWNSTREAM O2 1/2 SENSOR VOLTAGE";

                                double O2S12Voltage = payload[1] * 0.0196;
                                
                                ValueToInsert = Math.Round(O2S12Voltage, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "V";
                                break;
                            case 0x49:
                                DescriptionToInsert = "DOWNSTREAM O2 2/2 SENSOR VOLTAGE";

                                double O2S22Voltage = payload[1] * 0.0196;
                                
                                ValueToInsert = Math.Round(O2S22Voltage, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "V";
                                break;
                            case 0x4A:
                                DescriptionToInsert = "CLOSED LOOP TIMER";

                                double ClosedLoopTimer = payload[1] * 0.0535;
                                
                                ValueToInsert = Math.Round(ClosedLoopTimer, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "MINUTES";
                                break;
                            case 0x4B:
                                DescriptionToInsert = "TIME FROM START/RUN";

                                if (message.Length < 5) break;

                                if (payload[2] != (payload[0] + 1))
                                {
                                    DescriptionToInsert += " | ERROR: REQUEST F4 4B 4C";
                                    break;
                                }

                                double TimeFromStartRun = ((payload[1] << 8) + payload[3]) * 0.000208984375;

                                ValueToInsert = Math.Round(TimeFromStartRun, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "MINUTES";
                                break;
                            case 0x4C:
                                DescriptionToInsert = "TIME FROM START/RUN | ERROR: REQUEST F4 4B 4C";
                                break;
                            case 0x4D:
                                DescriptionToInsert = "RUNTIME AT STALL";

                                double RuntimeAtStall = payload[1] * 0.0535;
                                
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
                                DescriptionToInsert = "DWELL COIL 1 (CYL1_4)";

                                double DwellCoil1 = payload[1] * 0.008;
                                
                                ValueToInsert = Math.Round(DwellCoil1, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "MS";
                                break;
                            case 0x5B:
                                DescriptionToInsert = "DWELL COIL 2 (CYL2_3)";

                                double DwellCoil2 = payload[1] * 0.008;
                                
                                ValueToInsert = Math.Round(DwellCoil2, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "MS";
                                break;
                            case 0x5C:
                                DescriptionToInsert = "DWELL COIL 3 (CYL3_6)";

                                double DwellCoil3 = payload[1] * 0.008;
                                
                                ValueToInsert = Math.Round(DwellCoil3, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "MS";
                                break;
                            case 0x5D:
                                DescriptionToInsert = "FAN DUTY CYCLE";

                                double FanDutyCycle = payload[1] * 0.3921568627;
                                
                                ValueToInsert = Math.Round(FanDutyCycle, 1).ToString("0.0").Replace(",", ".");
                                UnitToInsert = "PERCENT";
                                break;
                            case 0x60:
                                DescriptionToInsert = "A/C RELAY STATE";
                                ValueToInsert = Convert.ToString(payload[1], 2).PadLeft(8, '0');
                                break;
                            case 0x61:
                                DescriptionToInsert = "DISTANCE TRAVELED UP TO 4.2 MILES";

                                double DistanceMi = payload[1] * 0.032;
                                double DistanceKm = DistanceMi * 1.609344;

                                if (Properties.Settings.Default.Units == "imperial")
                                {
                                    ValueToInsert = Math.Round(DistanceMi, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "MILE";
                                }
                                else if (Properties.Settings.Default.Units == "metric")
                                {
                                    ValueToInsert = Math.Round(DistanceKm, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "KILOMETER";
                                }
                                break;
                            case 0x73:
                                DescriptionToInsert = "LIMP-IN: ";

                                List<string> LimpInStates = new List<string>();

                                if (Util.IsBitSet(payload[1], 0)) LimpInStates.Add("ATS"); // Ambient temperature sensor
                                if (Util.IsBitSet(payload[1], 3)) LimpInStates.Add("IAT"); // Intake air temperature
                                if (Util.IsBitSet(payload[1], 4)) LimpInStates.Add("TPS"); // Throttle position sensor
                                if (Util.IsBitSet(payload[1], 5)) LimpInStates.Add("MPE"); // Intake manifold absolute pressure
                                if (Util.IsBitSet(payload[1], 6)) LimpInStates.Add("MPV"); // Intake manifold vacuum
                                if (Util.IsBitSet(payload[1], 7)) LimpInStates.Add("ECT"); // Engine coolant temperature

                                if (LimpInStates.Count == 0)
                                {
                                    DescriptionToInsert = "NO LIMP-IN STATE";
                                    break;
                                }

                                foreach (string s in LimpInStates)
                                {
                                    DescriptionToInsert += s + " | ";
                                }

                                if (DescriptionToInsert.Length > 2) DescriptionToInsert = DescriptionToInsert.Remove(DescriptionToInsert.Length - 3); // remove last "|" character
                                break;
                            case 0x74:
                                DescriptionToInsert = "DTC 2: ";

                                if (payload[1] == 0) ValueToInsert = "EMPTY SLOT";
                                else
                                {
                                    int index = SBEC3EngineDTC.Rows.IndexOf(SBEC3EngineDTC.Rows.Find(payload[1]));

                                    if (index > -1) // DTC description found
                                    {
                                        DescriptionToInsert += SBEC3EngineDTC.Rows[index]["description"];
                                    }
                                    else // no DTC description found
                                    {
                                        DescriptionToInsert += "UNRECOGNIZED DTC";
                                    }
                                }
                                break;
                            case 0x75:
                                DescriptionToInsert = "DTC 3: ";

                                if (payload[1] == 0) ValueToInsert = "EMPTY SLOT";
                                else
                                {
                                    int index = SBEC3EngineDTC.Rows.IndexOf(SBEC3EngineDTC.Rows.Find(payload[1]));

                                    if (index > -1) // DTC description found
                                    {
                                        DescriptionToInsert += SBEC3EngineDTC.Rows[index]["description"];
                                    }
                                    else // no DTC description found
                                    {
                                        DescriptionToInsert += "UNRECOGNIZED DTC";
                                    }
                                }
                                break;
                            case 0x76:
                                DescriptionToInsert = "DTC 4: ";

                                if (payload[1] == 0) ValueToInsert = "EMPTY SLOT";
                                else
                                {
                                    int index = SBEC3EngineDTC.Rows.IndexOf(SBEC3EngineDTC.Rows.Find(payload[1]));

                                    if (index > -1) // DTC description found
                                    {
                                        DescriptionToInsert += SBEC3EngineDTC.Rows[index]["description"];
                                    }
                                    else // no DTC description found
                                    {
                                        DescriptionToInsert += "UNRECOGNIZED DTC";
                                    }
                                }
                                break;
                            case 0x77:
                                DescriptionToInsert = "DTC 5: ";

                                if (payload[1] == 0) ValueToInsert = "EMPTY SLOT";
                                else
                                {
                                    int index = SBEC3EngineDTC.Rows.IndexOf(SBEC3EngineDTC.Rows.Find(payload[1]));

                                    if (index > -1) // DTC description found
                                    {
                                        DescriptionToInsert += SBEC3EngineDTC.Rows[index]["description"];
                                    }
                                    else // no DTC description found
                                    {
                                        DescriptionToInsert += "UNRECOGNIZED DTC";
                                    }
                                }
                                break;
                            case 0x78:
                                DescriptionToInsert = "DTC 6: ";

                                if (payload[1] == 0) ValueToInsert = "EMPTY SLOT";
                                else
                                {
                                    int index = SBEC3EngineDTC.Rows.IndexOf(SBEC3EngineDTC.Rows.Find(payload[1]));

                                    if (index > -1) // DTC description found
                                    {
                                        DescriptionToInsert += SBEC3EngineDTC.Rows[index]["description"];
                                    }
                                    else // no DTC description found
                                    {
                                        DescriptionToInsert += "UNRECOGNIZED DTC";
                                    }
                                }
                                break;
                            case 0x79:
                                DescriptionToInsert = "DTC 7: ";

                                if (payload[1] == 0) ValueToInsert = "EMPTY SLOT";
                                else
                                {
                                    int index = SBEC3EngineDTC.Rows.IndexOf(SBEC3EngineDTC.Rows.Find(payload[1]));

                                    if (index > -1) // DTC description found
                                    {
                                        DescriptionToInsert += SBEC3EngineDTC.Rows[index]["description"];
                                    }
                                    else // no DTC description found
                                    {
                                        DescriptionToInsert += "UNRECOGNIZED DTC";
                                    }
                                }
                                break;
                            case 0x7A:
                                DescriptionToInsert = "SPI TRANSFER RESULT";

                                if (message.Length < 5) break;

                                if (payload[2] != (payload[0] + 1))
                                {
                                    DescriptionToInsert += " | ERROR: REQUEST F4 7A 7B";
                                    break;
                                }

                                ValueToInsert = Convert.ToString(payload[1], 2).PadLeft(8, '0') + " " + Convert.ToString(payload[3], 2).PadLeft(8, '0');
                                break;
                            case 0x7B:
                                DescriptionToInsert = "SPI TRANSFER RESULT | ERROR: REQUEST F4 7A 7B";
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
                                DescriptionToInsert = "EGR POSITION SENSOR VOLTAGE";

                                double EGRPosVoltage = payload[1] * 0.0196;
                                
                                ValueToInsert = Math.Round(EGRPosVoltage, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "V";
                                break;
                            case 0x96:
                                DescriptionToInsert = "ACTUAL PURGE CURRENT";

                                double ActualPurgeCurrent = payload[1] * 0.0196;
                                
                                ValueToInsert = Math.Round(ActualPurgeCurrent, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "A";
                                break;
                            case 0x98:
                                DescriptionToInsert = "TPS INTERMITTENT COUNTER";
                                ValueToInsert = payload[1].ToString("0");
                                break;
                            case 0x9B:
                                DescriptionToInsert = "TRANSMISSION TEMPERATURE";

                                double TransTempF = payload[1] * 4.0;
                                double TransTempC = (TransTempF - 32.0) / 1.8;

                                if (Properties.Settings.Default.Units == "imperial")
                                {
                                    ValueToInsert = TransTempF.ToString("0");
                                    UnitToInsert = "°F";
                                }
                                else if (Properties.Settings.Default.Units == "metric")
                                {
                                    ValueToInsert = TransTempC.ToString("0");
                                    UnitToInsert = "°C";
                                }
                                break;
                            case 0xA3:
                                DescriptionToInsert = "CAM TIMING POSITION";

                                double CamTimingPosition = payload[1] * 0.5;
                                
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
                            case 0xA7:
                                DescriptionToInsert = "FREEZE FRAME PRIORITY LEVEL";
                                ValueToInsert = payload[1].ToString("0");
                                UnitToInsert = "0=LO 7=HI";
                                break;
                            case 0xA8:
                                DescriptionToInsert = "FREEZE FRAME DTC: ";
                                ValueToInsert = Util.ByteToHexString(payload, 1, 1);

                                if (payload[1] == 0x00)
                                {
                                    DescriptionToInsert += "EMPTY SLOT";
                                    break;
                                }
                                
                                int frzindex = SBEC3EngineDTC.Rows.IndexOf(SBEC3EngineDTC.Rows.Find(payload[1]));

                                if (frzindex > -1) // DTC description found
                                {
                                    DescriptionToInsert += SBEC3EngineDTC.Rows[frzindex]["description"];
                                }
                                else // no DTC description found
                                {
                                    DescriptionToInsert += "UNRECOGNIZED DTC";
                                }
                                break;
                            case 0xA9:
                                DescriptionToInsert = "FUEL SYSTEM STATUS 1";

                                if (payload[1] == 0)
                                {
                                    ValueToInsert = "N/A";
                                    break;
                                }

                                if (Util.IsBitSet(payload[1], 0)) ValueToInsert = "OPEN LOOP";
                                if (Util.IsBitSet(payload[1], 1)) ValueToInsert = "CLOSED LOOP";
                                if (Util.IsBitSet(payload[1], 2)) ValueToInsert = "OPEN LOOP / DRIVE";
                                if (Util.IsBitSet(payload[1], 3)) ValueToInsert = "OPEN LOOP / DTC";
                                if (Util.IsBitSet(payload[1], 4)) ValueToInsert = "CLOSED LOOP / DTC";
                                break;
                            case 0xAA:
                                DescriptionToInsert = "FUEL SYSTEM STATUS 2";

                                if (payload[1] == 0)
                                {
                                    ValueToInsert = "N/A";
                                    break;
                                }

                                if (Util.IsBitSet(payload[1], 0)) ValueToInsert = "OPEN LOOP";
                                if (Util.IsBitSet(payload[1], 1)) ValueToInsert = "CLOSED LOOP";
                                if (Util.IsBitSet(payload[1], 2)) ValueToInsert = "OPEN LOOP / DRIVE";
                                if (Util.IsBitSet(payload[1], 3)) ValueToInsert = "OPEN LOOP / DTC";
                                if (Util.IsBitSet(payload[1], 4)) ValueToInsert = "CLOSED LOOP / DTC";
                                break;
                            case 0xAB:
                                DescriptionToInsert = "ENGINE LOAD";

                                double EngineLoadC = payload[1] * 0.3921568627;
                                
                                ValueToInsert = Math.Round(EngineLoadC, 1).ToString("0.0").Replace(",", ".");
                                UnitToInsert = "PERCENT";
                                break;
                            case 0xAC:
                                DescriptionToInsert = "ENGINE COOLANT TEMPERATURE";

                                double ECTCF = payload[1] - 128;
                                double ECTFF = 1.8 * ECTCF + 32.0;

                                if (Properties.Settings.Default.Units == "imperial")
                                {
                                    ValueToInsert = Math.Round(ECTFF).ToString("0");
                                    UnitToInsert = "°F";
                                }
                                else if (Properties.Settings.Default.Units == "metric")
                                {
                                    ValueToInsert = ECTCF.ToString("0");
                                    UnitToInsert = "°C";
                                }
                                break;
                            case 0xAD:
                                DescriptionToInsert = "SHORT TERM FUEL TRIM 1";

                                double STFT1F = payload[1] * 0.196;

                                if (payload[1] >= 0x80) STFT1F -= 50.0;

                                ValueToInsert = Math.Round(STFT1F, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "PERCENT";
                                break;
                            case 0xAE:
                                DescriptionToInsert = "LONG TERM FUEL TRIM 1";

                                double LTFT1F = payload[1] * 0.196;

                                if (payload[1] >= 0x80) LTFT1F -= 50.0;

                                ValueToInsert = Math.Round(LTFT1F, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "PERCENT";
                                break;
                            case 0xAF:
                                DescriptionToInsert = "SHORT TERM FUEL TRIM 2";

                                double STFT2F = payload[1] * 0.196;

                                if (payload[1] >= 0x80) STFT2F -= 50.0;

                                ValueToInsert = Math.Round(STFT2F, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "PERCENT";
                                break;
                            case 0xB0:
                                DescriptionToInsert = "LONG TERM FUEL TRIM 2";

                                double LTFT2F = payload[1] * 0.196;

                                if (payload[1] >= 0x80) LTFT2F -= 50.0;

                                ValueToInsert = Math.Round(LTFT2F, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "PERCENT";
                                break;
                            case 0xB1:
                                DescriptionToInsert = "INTAKE MANIFOLD ABSOLUTE PRESSURE (MAP)";

                                double MAPFPSI = payload[1] * 0.059756;
                                double MAPFKPA = MAPFPSI * 6.894757;

                                if (Properties.Settings.Default.Units == "imperial")
                                {
                                    ValueToInsert = Math.Round(MAPFPSI, 1).ToString("0.0").Replace(",", ".");
                                    UnitToInsert = "PSI";
                                }
                                else if (Properties.Settings.Default.Units == "metric")
                                {
                                    ValueToInsert = Math.Round(MAPFKPA, 1).ToString("0.0").Replace(",", ".");
                                    UnitToInsert = "KPA";
                                }
                                break;
                            case 0xB2:
                                DescriptionToInsert = "ENGINE SPEED";

                                double EngineSpeedF = payload[1] * 32.0;
                                
                                ValueToInsert = EngineSpeedF.ToString("0");
                                UnitToInsert = "RPM";
                                break;
                            case 0xB3:
                                DescriptionToInsert = "VEHICLE SPEED";

                                double VehicleSpeedFMPH = payload[1] * 0.5;
                                double VehicleSpeedFKMH = VehicleSpeedFMPH * 1.609344;
                                
                                if (Properties.Settings.Default.Units == "imperial")
                                {
                                    ValueToInsert = Math.Round(VehicleSpeedFMPH).ToString("0");
                                    UnitToInsert = "MPH";
                                }
                                else if (Properties.Settings.Default.Units == "metric")
                                {
                                    ValueToInsert = Math.Round(VehicleSpeedFKMH).ToString("0");
                                    UnitToInsert = "KM/H";
                                }
                                break;
                            case 0xB4:
                                DescriptionToInsert = "MAP VACUUM";

                                double MAPVacuumFPSI = payload[1] * 0.059756;
                                double MAPVacuumFKPA = MAPVacuumFPSI * 6.894757;

                                if (Properties.Settings.Default.Units == "imperial")
                                {
                                    ValueToInsert = Math.Round(MAPVacuumFPSI, 1).ToString("0.0").Replace(",", ".");
                                    UnitToInsert = "PSI";
                                }
                                else if (Properties.Settings.Default.Units == "metric")
                                {
                                    ValueToInsert = Math.Round(MAPVacuumFKPA, 1).ToString("0.0").Replace(",", ".");
                                    UnitToInsert = "KPA";
                                }
                                break;
                            case 0xB5:
                                DescriptionToInsert = "DTC E"; // last emission related DTC stored

                                if (payload[1] == 0) ValueToInsert = "EMPTY SLOT";
                                else
                                {
                                    int index = SBEC3EngineDTC.Rows.IndexOf(SBEC3EngineDTC.Rows.Find(payload[1]));

                                    if (index > -1) // DTC description found
                                    {
                                        DescriptionToInsert += ": " + SBEC3EngineDTC.Rows[index]["description"];
                                    }
                                    else // no DTC description found
                                    {
                                        DescriptionToInsert += ": UNRECOGNIZED DTC";
                                    }
                                }
                                break;
                            case 0xB6:
                                DescriptionToInsert = "CATALYST TEMPERATURE SENSOR VOLTAGE";

                                double CTSVoltage = payload[1] * 0.0196;
                                
                                ValueToInsert = Math.Round(CTSVoltage, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "V";
                                break;
                            case 0xB7:
                                DescriptionToInsert = "PURGE DUTY CYCLE";

                                double GovPDC = payload[1] * 0.3921568627;
                                
                                ValueToInsert = Math.Round(GovPDC, 1).ToString("0.0").Replace(",", ".");
                                UnitToInsert = "PERCENT";
                                break;
                            case 0xBD:
                                DescriptionToInsert = "Brake switch monitor result 0";
                                ValueToInsert = Convert.ToString(payload[1], 2).PadLeft(8, '0');
                                break;
                            case 0xBE:
                                DescriptionToInsert = "Brake switch monitor result 1";
                                ValueToInsert = Convert.ToString(payload[1], 2).PadLeft(8, '0');
                                break;
                            case 0xBF:
                                DescriptionToInsert = "Brake switch monitor result 2";
                                ValueToInsert = Convert.ToString(payload[1], 2).PadLeft(8, '0');
                                break;
                            case 0xC0:
                                DescriptionToInsert = "CATALYST TEMPERATURE";

                                double CatalystTemperatureC = payload[1] - 128;
                                double CatalystTemperatureF = 1.8 * CatalystTemperatureC + 32.0;

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
                                DescriptionToInsert = "FUEL LEVEL SENSOR VOLTAGE 3";

                                double FLS3Voltage = payload[1] * 0.0196;
                                
                                ValueToInsert = Math.Round(FLS3Voltage, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "V";
                                break;
                            case 0xD2:
                                DescriptionToInsert = "SENSOR RATIONALITY RESULT 0";
                                ValueToInsert = Convert.ToString(payload[1], 2).PadLeft(8, '0');
                                break;
                            case 0xD3:
                                DescriptionToInsert = "SENSOR RATIONALITY RESULT 1";
                                ValueToInsert = Convert.ToString(payload[1], 2).PadLeft(8, '0');
                                break;
                            case 0xD4:
                                DescriptionToInsert = "O2 SENSOR RATIONALITY RESULT 0";
                                ValueToInsert = Convert.ToString(payload[1], 2).PadLeft(8, '0');
                                break;
                            case 0xD5:
                                DescriptionToInsert = "O2 SENSOR RATIONALITY RESULT 1";
                                ValueToInsert = Convert.ToString(payload[1], 2).PadLeft(8, '0');
                                break;
                            case 0xD6:
                                DescriptionToInsert = "TORQUE CONVERTER CLUTCH MONITOR RESULT";

                                if (Util.IsBitSet(payload[1], 7))
                                {
                                    ValueToInsert = "PTU SOL RAT";
                                }
                                else
                                {
                                    ValueToInsert = "OK";
                                }
                                break;
                            case 0xD7:
                                DescriptionToInsert = "SENSOR RATIONALITY RESULT 2";
                                ValueToInsert = Convert.ToString(payload[1], 2).PadLeft(8, '0');
                                break;
                            case 0xD8:
                                DescriptionToInsert = "SENSOR RATIONALITY RESULT 3";
                                ValueToInsert = Convert.ToString(payload[1], 2).PadLeft(8, '0');
                                break;
                            case 0xDA:
                                DescriptionToInsert = "P_PCM_NOC_STRDCAMTIME";
                                ValueToInsert = Util.ByteToHexString(payload, 1, 1);
                                break;
                            case 0xDB:
                                DescriptionToInsert = "FUEL LEVEL SENSOR VOLTAGE 2";

                                double FLS2Voltage = payload[1] * 0.0196;
                                
                                ValueToInsert = Math.Round(FLS2Voltage, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "V";
                                break;
                            case 0xDD:
                                DescriptionToInsert = "CONFIGURATION 1";
                                ValueToInsert = Convert.ToString(payload[1], 2).PadLeft(8, '0');
                                break;
                            case 0xDE:
                                DescriptionToInsert = "FUEL LEVEL SENSOR VOLTAGE 1";

                                double FLS1Voltage = payload[1] * 0.0196;
                                
                                ValueToInsert = Math.Round(FLS1Voltage, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "V";
                                break;
                            case 0xDF:
                                DescriptionToInsert = "FUEL LEVEL";

                                double FuelLevelG = payload[1] * 0.125;
                                double FuelLevelL = FuelLevelG * 3.785412;

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
                            case 0xE0:
                                DescriptionToInsert = "FUEL USED";

                                double FuelUsedG = payload[1] * 0.125;
                                double FuelUsedL = FuelUsedG * 3.785412;
                                
                                if (Properties.Settings.Default.Units == "imperial")
                                {
                                    ValueToInsert = Math.Round(FuelUsedG, 1).ToString("0.0").Replace(",", ".");
                                    UnitToInsert = "GALLON";
                                }
                                else if (Properties.Settings.Default.Units == "metric")
                                {
                                    ValueToInsert = Math.Round(FuelUsedL, 1).ToString("0.0").Replace(",", ".");
                                    UnitToInsert = "LITER";
                                }
                                break;
                            case 0xE3:
                                DescriptionToInsert = "ENGINE LOAD";

                                double EngineLoadD = payload[1] * 0.3921568627;
                                
                                ValueToInsert = Math.Round(EngineLoadD, 1).ToString("0.0").Replace(",", ".");
                                UnitToInsert = "PERCENT";
                                break;
                            case 0xE4:
                                DescriptionToInsert = "OBD2 MONITOR TEST RESULTS 2";
                                ValueToInsert = Convert.ToString(payload[1], 2).PadLeft(8, '0');
                                break;
                            case 0xE5:
                                DescriptionToInsert = "CONFIGURATION 2";
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
                                DescriptionToInsert = "CALCULATED TPS VOLTAGE";

                                double CalculatedTPSVoltageB = payload[1] * 0.0196;
                                
                                ValueToInsert = Math.Round(CalculatedTPSVoltageB, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "V";
                                break;
                            default:
                                for (int i = 0; i < HSBPNum; i++)
                                {
                                    HSOffset.Add(payload[i * 2]);
                                    HSValues.Add(payload[(i * 2) + 1]);
                                }

                                DescriptionToInsert = "F4 RAM TABLE | OFFSET: " + Util.ByteToHexStringSimple(HSOffset.ToArray());
                                ValueToInsert = Util.ByteToHexStringSimple(HSValues.ToArray());
                                break;
                        }
                        break;
                    case 0xF5: // SBEC3 and CUMMINS
                        DescriptionToInsert = "F5 RAM TABLE SELECTED";

                        if (message.Length < 3) break;

                        switch (payload[0])
                        {
                            case 0x1E:
                                DescriptionToInsert = "ACTUAL GOVERNOR PRESSURE";

                                if (message.Length < 5) break;

                                if (payload[2] != (payload[0] + 1))
                                {
                                    DescriptionToInsert += " | ERROR: REQUEST F5 1E 1F";
                                    break;
                                }

                                double ActGovernorPressurePSI = ((payload[1] << 8) + payload[3]) * 0.0078125;
                                double ActGovernorPressureKPA = ActGovernorPressurePSI * 6.894757;

                                if (Properties.Settings.Default.Units == "imperial")
                                {
                                    ValueToInsert = Math.Round(ActGovernorPressurePSI, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "PSI";
                                }
                                else if (Properties.Settings.Default.Units == "metric")
                                {
                                    ValueToInsert = Math.Round(ActGovernorPressureKPA, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "KPA";
                                }
                                break;
                            case 0x1F:
                                DescriptionToInsert = "ACTUAL GOVERNOR PRESSURE | ERROR: REQUEST F5 1E 1F";
                                break;
                            case 0x47:
                                DescriptionToInsert = "TTVA ADJUSTED POSITION"; // Transmission Throttle Valve Actuator

                                if (message.Length < 5) break;

                                if (payload[2] != (payload[0] + 1))
                                {
                                    DescriptionToInsert += " | ERROR: REQUEST F5 47 48";
                                    break;
                                }

                                double TTVAAdjustedPosition = ((payload[1] << 8) + payload[3]) * 0.0078125;

                                ValueToInsert = Math.Round(TTVAAdjustedPosition, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "DEGREE";
                                break;
                            case 0x48:
                                DescriptionToInsert = "TTVA ADJUSTED POSITION | ERROR: REQUEST F5 47 48";
                                break;
                            case 0x49:
                                DescriptionToInsert = "TTVA TARGET POSITION";

                                if (message.Length < 5) break;

                                if (payload[2] != (payload[0] + 1))
                                {
                                    DescriptionToInsert += " | ERROR: REQUEST F5 49 4A";
                                    break;
                                }

                                double TTVATargetPosition = ((payload[1] << 8) + payload[3]) * 0.0078125;

                                ValueToInsert = Math.Round(TTVATargetPosition, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "DEGREE";
                                break;
                            case 0x4A:
                                DescriptionToInsert = "TTVA TARGET POSITION | ERROR: REQUEST F5 49 4A";
                                break;
                            case 0x4B:
                                DescriptionToInsert = "TTVA ACTUAL POSITION";

                                if (message.Length < 5) break;

                                if (payload[2] != (payload[0] + 1))
                                {
                                    DescriptionToInsert += " | ERROR: REQUEST F5 4B 4C";
                                    break;
                                }

                                double TTVAActualPosition = ((payload[1] << 8) + payload[3]) * 0.0078125;

                                ValueToInsert = Math.Round(TTVAActualPosition, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "DEGREE";
                                break;
                            case 0x4C:
                                DescriptionToInsert = "TTVA ACTUAL POSITION | ERROR: REQUEST F5 4B 4C";
                                break;
                            case 0x4D:
                                DescriptionToInsert = "TTVA DUTY CYCLE";

                                if (message.Length < 5) break;

                                if (payload[2] != (payload[0] + 1))
                                {
                                    DescriptionToInsert += " | ERROR: REQUEST F5 4D 4E";
                                    break;
                                }

                                double TTVADutyCycle = ((payload[1] << 8) + payload[3]) * 0.0078125;

                                ValueToInsert = Math.Round(TTVADutyCycle, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "PERCENT";
                                break;
                            case 0x4E:
                                DescriptionToInsert = "TTVA DUTY CYCLE | ERROR: REQUEST F5 4D 4E";
                                break;
                            case 0xCB:
                                DescriptionToInsert = "TCM | FAULT CODE PRESENT";

                                if (message.Length < 5) break;

                                if (payload[2] != (payload[0] + 1))
                                {
                                    DescriptionToInsert += " | ERROR: REQUEST F5 CB CC";
                                    break;
                                }

                                if ((payload[1] == 0) && (payload[3] == 0))
                                {
                                    DescriptionToInsert = "TCM | NO FAULT CODE";
                                    break;
                                }

                                ValueToInsert = "OBD2 P" + Util.ByteToHexStringSimple(new byte[2] { payload[1], payload[3] }).Replace(" ", "");
                                break;
                            case 0xCC:
                                DescriptionToInsert = "TCM | FAULT CODE PRESENT | ERROR: REQUEST F5 CB CC";
                                break;
                            default:
                                for (int i = 0; i < HSBPNum; i++)
                                {
                                    HSOffset.Add(payload[i * 2]);
                                    HSValues.Add(payload[(i * 2) + 1]);
                                }

                                DescriptionToInsert = "F5 RAM TABLE | OFFSET: " + Util.ByteToHexStringSimple(HSOffset.ToArray());
                                ValueToInsert = Util.ByteToHexStringSimple(HSValues.ToArray());
                                break;
                        }
                        break;
                    case 0xF6:
                        DescriptionToInsert = "F6 RAM TABLE SELECTED";

                        if (message.Length < 3) break;

                        switch (payload[0])
                        {
                            default:
                                for (int i = 0; i < HSBPNum; i++)
                                {
                                    HSOffset.Add(payload[i * 2]);
                                    HSValues.Add(payload[(i * 2) + 1]);
                                }

                                DescriptionToInsert = "F6 RAM TABLE | OFFSET: " + Util.ByteToHexStringSimple(HSOffset.ToArray());
                                ValueToInsert = Util.ByteToHexStringSimple(HSValues.ToArray());
                                break;
                        }
                        break;
                    case 0xF7:
                        DescriptionToInsert = "F7 RAM TABLE SELECTED";

                        if (message.Length < 3) break;

                        switch (payload[0])
                        {
                            default:
                                for (int i = 0; i < HSBPNum; i++)
                                {
                                    HSOffset.Add(payload[i * 2]);
                                    HSValues.Add(payload[(i * 2) + 1]);
                                }

                                DescriptionToInsert = "F7 RAM TABLE | OFFSET: " + Util.ByteToHexStringSimple(HSOffset.ToArray());
                                ValueToInsert = Util.ByteToHexStringSimple(HSValues.ToArray());
                                break;
                        }
                        break;
                    case 0xF8: // CUMMINS DTC FREEZE FRAMES
                        DescriptionToInsert = "F8 RAM TABLE SELECTED";

                        if (message.Length < 3) break;

                        if ((Year < 2003) && (ControllerHardwareType == 9)) // CUMMINS
                        {
                            switch (payload[0])
                            {
                                case 0x07:
                                    DescriptionToInsert = "VEHICLE SPEED";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST F8 07 08";
                                        break;
                                    }

                                    double VehicleSpeedAMPH = ((payload[1] << 8) + payload[3]) * 0.00390625;
                                    double VehicleSpeedAKMH = VehicleSpeedAMPH * 1.609344;

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        ValueToInsert = Math.Round(VehicleSpeedAMPH, 3).ToString("0.000").Replace(",", ".");
                                        UnitToInsert = "MPH";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        ValueToInsert = Math.Round(VehicleSpeedAKMH, 3).ToString("0.000").Replace(",", ".");
                                        UnitToInsert = "KM/H";
                                    }
                                    break;
                                case 0x08:
                                    DescriptionToInsert = "VEHICLE SPEED | ERROR: REQUEST F8 07 08";
                                    break;
                                case 0x09:
                                    DescriptionToInsert = "ENGINE SPEED";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST F8 09 0A";
                                        break;
                                    }

                                    double EngineSpeedA = ((payload[1] << 8) + payload[3]) * 0.125;

                                    ValueToInsert = Math.Round(EngineSpeedA, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "RPM";
                                    break;
                                case 0x0A:
                                    DescriptionToInsert = "ENGINE SPEED | ERROR: REQUEST F8 09 0A";
                                    break;
                                case 0x0B:
                                    DescriptionToInsert = "SWITCH STATUS 1";

                                    List<string> SwitchListA = new List<string>();

                                    if (Util.IsBitSet(payload[1], 7)) SwitchListA.Add("-7-");
                                    if (Util.IsBitSet(payload[1], 6)) SwitchListA.Add("BRAKE");
                                    if (Util.IsBitSet(payload[1], 5)) SwitchListA.Add("IDLE");
                                    if (Util.IsBitSet(payload[1], 4)) SwitchListA.Add("-4-");
                                    if (Util.IsBitSet(payload[1], 3)) SwitchListA.Add("-3-");
                                    if (Util.IsBitSet(payload[1], 2)) SwitchListA.Add("NOT-IDLE");
                                    if (Util.IsBitSet(payload[1], 1)) SwitchListA.Add("-1-");
                                    if (Util.IsBitSet(payload[1], 0)) SwitchListA.Add("-0-");

                                    if (SwitchListA.Count == 0) break;

                                    DescriptionToInsert += ": ";

                                    foreach (string s in SwitchListA)
                                    {
                                        DescriptionToInsert += s + " | ";
                                    }

                                    if (DescriptionToInsert.Length > 2) DescriptionToInsert = DescriptionToInsert.Remove(DescriptionToInsert.Length - 3); // remove last "|" character
                                    break;
                                case 0x0D:
                                    DescriptionToInsert = "ENGINE LOAD";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST F8 0D 0E";
                                        break;
                                    }

                                    double EngineLoadA = ((payload[1] << 8) + payload[3]) * 0.00390625;

                                    ValueToInsert = Math.Round(EngineLoadA, 1).ToString("0.0").Replace(",", ".");
                                    UnitToInsert = "PERCENT";
                                    break;
                                case 0x0E:
                                    DescriptionToInsert = "ENGINE LOAD | ERROR: REQUEST F8 0D 0E";
                                    break;
                                case 0x0F:
                                    DescriptionToInsert = "APP SENSOR PERCENT";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST F8 0F 10";
                                        break;
                                    }

                                    double APPSPercentA = ((payload[1] << 8) + payload[3]) * 0.25;

                                    ValueToInsert = Math.Round(APPSPercentA, 1).ToString("0.0").Replace(",", ".");
                                    UnitToInsert = "PERCENT";
                                    break;
                                case 0x10:
                                    DescriptionToInsert = "APP SENSOR PERCENT | ERROR: REQUEST F8 0F 10";
                                    break;
                                case 0x11:
                                    DescriptionToInsert = "BOOST PRESSURE";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST F8 11 12";
                                        break;
                                    }

                                    double BoostPressureAPSI = ((payload[1] << 8) + payload[3]) * 0.0078125;
                                    double BoostPressureAKPA = BoostPressureAPSI * 6.894757;

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        ValueToInsert = Math.Round(BoostPressureAPSI, 3).ToString("0.000").Replace(",", ".");
                                        UnitToInsert = "PSI";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        ValueToInsert = Math.Round(BoostPressureAKPA, 3).ToString("0.000").Replace(",", ".");
                                        UnitToInsert = "KPA";
                                    }
                                    break;
                                case 0x12:
                                    DescriptionToInsert = "BOOST PRESSURE | ERROR: REQUEST F8 11 12";
                                    break;
                                case 0x13:
                                    DescriptionToInsert = "ENGINE COOLANT TEMPERATURE";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST F8 13 14";
                                        break;
                                    }

                                    double ECT1F = ((payload[1] << 8) + payload[3]) * 0.015625;
                                    double ECT1C = (ECT1F - 32.0) / 1.8;

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        ValueToInsert = Math.Round(ECT1F, 1).ToString("0.0").Replace(",", ".");
                                        UnitToInsert = "°F";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        ValueToInsert = Math.Round(ECT1C, 1).ToString("0.0").Replace(",", ".");
                                        UnitToInsert = "°C";
                                    }
                                    break;
                                case 0x14:
                                    DescriptionToInsert = "ENGINE COOLANT TEMPERATURE | ERROR: REQUEST F8 13 14";
                                    break;
                                case 0x15:
                                    DescriptionToInsert = "INTAKE AIR TEMPERATURE";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST F8 15 16";
                                        break;
                                    }

                                    double IAT1F = ((payload[1] << 8) + payload[3]) * 0.015625;
                                    double IAT1C = (IAT1F - 32.0) / 1.8;

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        ValueToInsert = Math.Round(IAT1F, 1).ToString("0.0").Replace(",", ".");
                                        UnitToInsert = "°F";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        ValueToInsert = Math.Round(IAT1C, 1).ToString("0.0").Replace(",", ".");
                                        UnitToInsert = "°C";
                                    }
                                    break;
                                case 0x16:
                                    DescriptionToInsert = "INTAKE AIR TEMPERATURE | ERROR: REQUEST F8 15 16";
                                    break;
                                case 0x17:
                                    DescriptionToInsert = "OIL PRESSURE";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST F8 17 18";
                                        break;
                                    }

                                    double OilPressureAPSI = ((payload[1] << 8) + payload[3]) * 0.0078125;
                                    double OilPressureAKPA = OilPressureAPSI * 6.894757;

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        ValueToInsert = Math.Round(OilPressureAPSI, 3).ToString("0.000").Replace(",", ".");
                                        UnitToInsert = "PSI";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        ValueToInsert = Math.Round(OilPressureAKPA, 3).ToString("0.000").Replace(",", ".");
                                        UnitToInsert = "KPA";
                                    }
                                    break;
                                case 0x18:
                                    DescriptionToInsert = "OIL PRESSURE | ERROR: REQUEST F8 17 18";
                                    break;
                                case 0x19:
                                    DescriptionToInsert = "SWITCH STATUS 2";

                                    List<string> SwitchListB = new List<string>();

                                    if (Util.IsBitSet(payload[1], 7)) SwitchListB.Add("-7-");
                                    if (Util.IsBitSet(payload[1], 6)) SwitchListB.Add("-6-");
                                    if (Util.IsBitSet(payload[1], 5)) SwitchListB.Add("-5-");
                                    if (Util.IsBitSet(payload[1], 4)) SwitchListB.Add("-4-");
                                    if (Util.IsBitSet(payload[1], 3)) SwitchListB.Add("INTHEAT2"); // intake heater 2
                                    if (Util.IsBitSet(payload[1], 2)) SwitchListB.Add("INTHEAT1"); // intake heater 1
                                    if (Util.IsBitSet(payload[1], 1)) SwitchListB.Add("TRFPMPDR"); // transfer pump driver
                                    if (Util.IsBitSet(payload[1], 0)) SwitchListB.Add("-0-");

                                    if (SwitchListB.Count == 0) break;

                                    DescriptionToInsert += ": ";

                                    foreach (string s in SwitchListB)
                                    {
                                        DescriptionToInsert += s + " | ";
                                    }

                                    if (DescriptionToInsert.Length > 2) DescriptionToInsert = DescriptionToInsert.Remove(DescriptionToInsert.Length - 3); // remove last "|" character
                                    break;
                                case 0x20:
                                    DescriptionToInsert = "FINAL FUEL STATE";

                                    switch (payload[1])
                                    {
                                        case 0x00:
                                            ValueToInsert = "NOT SET";
                                            break;
                                        case 0x01:
                                            ValueToInsert = "JCOM TORQUE";
                                            break;
                                        case 0x02:
                                            ValueToInsert = "JCOM SPEED";
                                            break;
                                        case 0x03:
                                            ValueToInsert = "PROGRSV SHIFT";
                                            break;
                                        case 0x04:
                                            ValueToInsert = "PTO";
                                            break;
                                        case 0x05:
                                            ValueToInsert = "USER COMMAND";
                                            break;
                                        case 0x06:
                                            ValueToInsert = "LIMP HOME";
                                            break;
                                        case 0x07:
                                            ValueToInsert = "ASG THROTTLE";
                                            break;
                                        case 0x08:
                                            ValueToInsert = "4-D FUELING";
                                            break;
                                        case 0x09:
                                            ValueToInsert = "CRUISE CONTROL";
                                            break;
                                        case 0x0A:
                                            ValueToInsert = "ROAD SPEED GOV";
                                            break;
                                        case 0x0B:
                                            ValueToInsert = "LOW SPEED GOV";
                                            break;
                                        case 0x0C:
                                            ValueToInsert = "HIGH SPEED GOV";
                                            break;
                                        case 0x0D:
                                            ValueToInsert = "TORQUE DERATE OVERRIDE";
                                            break;
                                        case 0x0E:
                                            ValueToInsert = "LOW GEAR";
                                            break;
                                        case 0x0F:
                                            ValueToInsert = "ALTITUDE DERATE";
                                            break;
                                        case 0x10:
                                            ValueToInsert = "AFC DERATE";
                                            break;
                                        case 0x11:
                                            ValueToInsert = "ANC DERATE";
                                            break;
                                        case 0x12:
                                            ValueToInsert = "ENGINE PROTECT";
                                            break;
                                        case 0x13:
                                            ValueToInsert = "TORQUE CRV LIMIT";
                                            break;
                                        case 0x14:
                                            ValueToInsert = "JCOM TORQUE DERATE";
                                            break;
                                        case 0x15:
                                            ValueToInsert = "OUT OF GEAR";
                                            break;
                                        case 0x16:
                                            ValueToInsert = "CRANKING";
                                            break;
                                        case 0x17:
                                            ValueToInsert = "USER OVERRIDE";
                                            break;
                                        case 0x18:
                                            ValueToInsert = "ENGINE BRAKE";
                                            break;
                                        case 0x19:
                                            ValueToInsert = "ENGINE OVERSPEED";
                                            break;
                                        case 0x1A:
                                            ValueToInsert = "ENGINE STOPPED";
                                            break;
                                        case 0x1B:
                                            ValueToInsert = "SHUTDOWN";
                                            break;
                                        case 0x1C:
                                            ValueToInsert = "FUEL DTC DERATE";
                                            break;
                                        case 0x1D:
                                            ValueToInsert = "ENGINE PROTECT";
                                            break;
                                        case 0x1E:
                                            ValueToInsert = "ALL SPD GOV APP";
                                            break;
                                        case 0x1F:
                                            ValueToInsert = "ALT TORQUE";
                                            break;
                                        case 0x20:
                                            ValueToInsert = "MASTER/SLAVE OVERRIDE";
                                            break;
                                        case 0x21:
                                            ValueToInsert = "STARTUP OIL LIMIT";
                                            break;
                                        case 0x22:
                                            ValueToInsert = "PTO DERATE";
                                            break;
                                        case 0x23:
                                            ValueToInsert = "TORQUE CONTROL";
                                            break;
                                        case 0x24:
                                            ValueToInsert = "POWERTRAIN PROTECT";
                                            break;
                                        case 0x25:
                                            ValueToInsert = "T2 SPEED";
                                            break;
                                        case 0x26:
                                            ValueToInsert = "T2 TORQUE DERATE";
                                            break;
                                        case 0x27:
                                            ValueToInsert = "T2 DERATE";
                                            break;
                                        case 0x28:
                                            ValueToInsert = "NO DERATE";
                                            break;
                                        case 0x29:
                                            ValueToInsert = "ANTI THEFT DERATE";
                                            break;
                                        case 0x2A:
                                            ValueToInsert = "PART THROTTLE LIMIT";
                                            break;
                                        case 0x2B:
                                            ValueToInsert = "STEADY-STATE AMB DERATE";
                                            break;
                                        case 0x2C:
                                            ValueToInsert = "TRSNT COOLANT DERATE";
                                            break;
                                    }
                                    break;
                                case 0x21:
                                    DescriptionToInsert = "BATTERY VOLTAGE";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST F8 21 22";
                                        break;
                                    }

                                    double BatteryVoltsA = ((payload[1] << 8) + payload[3]) * 0.0625;

                                    ValueToInsert = Math.Round(BatteryVoltsA, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0x22:
                                    DescriptionToInsert = "BATTERY VOLTAGE | ERROR: REQUEST F8 21 22";
                                    break;
                                case 0x23:
                                    DescriptionToInsert = "CALCULATED FUEL";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST F8 23 24";
                                        break;
                                    }

                                    double CalculatedFuelA = ((payload[1] << 8) + payload[3]) * 0.001953155;

                                    ValueToInsert = Math.Round(CalculatedFuelA, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "PERCENT";
                                    break;
                                case 0x24:
                                    DescriptionToInsert = "CALCULATED FUEL | ERROR: REQUEST F8 23 24";
                                    break;
                                case 0x25:
                                    DescriptionToInsert = "CALCULATED TIMING";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST F8 25 26";
                                        break;
                                    }

                                    double CalculatedTimingA = (((payload[1] << 8) + payload[3]) * 0.1176475) - 20.0;

                                    ValueToInsert = Math.Round(CalculatedTimingA, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "DEGREE";
                                    break;
                                case 0x26:
                                    DescriptionToInsert = "CALCULATED TIMING | ERROR: REQUEST F8 25 26";
                                    break;
                                case 0x27:
                                    DescriptionToInsert = "REGULATOR VALVE CURRENT";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST F8 27 28";
                                        break;
                                    }

                                    double RVCA = ((payload[1] << 8) + payload[3]) * 1.220721752;

                                    ValueToInsert = Math.Round(RVCA, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "MILLIAMPS";
                                    break;
                                case 0x28:
                                    DescriptionToInsert = "REGULATOR VALVE CURRENT | ERROR: REQUEST F8 27 28";
                                    break;
                                case 0x29:
                                    DescriptionToInsert = "INJECTOR PUMP FUEL TEMPERATURE";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST F8 29 2A";
                                        break;
                                    }

                                    double InjPumpFuelTempAF = ((payload[1] << 8) + payload[3]) * 0.0625;
                                    double InjPumpFuelTempAC = (InjPumpFuelTempAF - 32.0) / 1.8;

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        ValueToInsert = Math.Round(InjPumpFuelTempAF, 1).ToString("0.0").Replace(",", ".");
                                        UnitToInsert = "°F";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        ValueToInsert = Math.Round(InjPumpFuelTempAC, 1).ToString("0.0").Replace(",", ".");
                                        UnitToInsert = "°C";
                                    }
                                    break;
                                case 0x2A:
                                    DescriptionToInsert = "INJECTOR PUMP FUEL TEMPERATURE | ERROR: REQUEST F8 29 2A";
                                    break;
                                case 0x2B:
                                    DescriptionToInsert = "INJECTOR PUMP ENGINE SPEED";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST F8 2B 2C";
                                        break;
                                    }

                                    double InjPumpEngineSpeedA = ((payload[1] << 8) + payload[3]) * 0.125;

                                    ValueToInsert = Math.Round(InjPumpEngineSpeedA, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "RPM";
                                    break;
                                case 0x2C:
                                    DescriptionToInsert = "INJECTOR PUMP ENGINE SPEED | ERROR: REQUEST F8 2B 2C";
                                    break;
                                case 0x2F:
                                    DescriptionToInsert = "FREEZE FRM DTC 1:";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST F8 2F 30";
                                        break;
                                    }

                                    if ((payload[1] == 0x00) && (payload[3] == 0x00))
                                    {
                                        DescriptionToInsert += "EMPTY SLOT";
                                        break;
                                    }

                                    ValueToInsert = "OBD2 P" + Util.ByteToHexString(new byte[2] { payload[1], payload[3] }, 0, 2).Replace(" ", "");
                                    break;
                                case 0x30:
                                    DescriptionToInsert = "FREEZE FRAME DTC 1: | ERROR: REQUEST F8 2F 30";
                                    break;
                                case 0x37:
                                    DescriptionToInsert = "VEHICLE SPEED";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST F8 37 38";
                                        break;
                                    }

                                    double VehicleSpeedBMPH = ((payload[1] << 8) + payload[3]) * 0.00390625;
                                    double VehicleSpeedBKMH = VehicleSpeedBMPH * 1.609344;

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        ValueToInsert = Math.Round(VehicleSpeedBMPH, 3).ToString("0.000").Replace(",", ".");
                                        UnitToInsert = "MPH";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        ValueToInsert = Math.Round(VehicleSpeedBKMH, 3).ToString("0.000").Replace(",", ".");
                                        UnitToInsert = "KM/H";
                                    }
                                    break;
                                case 0x38:
                                    DescriptionToInsert = "VEHICLE SPEED | ERROR: REQUEST F8 37 38";
                                    break;
                                case 0x39:
                                    DescriptionToInsert = "ENGINE SPEED";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST F8 39 3A";
                                        break;
                                    }

                                    double EngineSpeedB = ((payload[1] << 8) + payload[3]) * 0.125;

                                    ValueToInsert = Math.Round(EngineSpeedB, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "RPM";
                                    break;
                                case 0x3A:
                                    DescriptionToInsert = "ENGINE SPEED | ERROR: REQUEST F8 39 3A";
                                    break;
                                case 0x3B:
                                    DescriptionToInsert = "SWITCH STATUS 1";

                                    List<string> SwitchListC = new List<string>();

                                    if (Util.IsBitSet(payload[1], 7)) SwitchListC.Add("-7-");
                                    if (Util.IsBitSet(payload[1], 6)) SwitchListC.Add("BRAKE");
                                    if (Util.IsBitSet(payload[1], 5)) SwitchListC.Add("IDLE");
                                    if (Util.IsBitSet(payload[1], 4)) SwitchListC.Add("-4-");
                                    if (Util.IsBitSet(payload[1], 3)) SwitchListC.Add("-3-");
                                    if (Util.IsBitSet(payload[1], 2)) SwitchListC.Add("NOT-IDLE");
                                    if (Util.IsBitSet(payload[1], 1)) SwitchListC.Add("-1-");
                                    if (Util.IsBitSet(payload[1], 0)) SwitchListC.Add("-0-");

                                    if (SwitchListC.Count == 0) break;

                                    DescriptionToInsert += ": ";

                                    foreach (string s in SwitchListC)
                                    {
                                        DescriptionToInsert += s + " | ";
                                    }

                                    if (DescriptionToInsert.Length > 2) DescriptionToInsert = DescriptionToInsert.Remove(DescriptionToInsert.Length - 3); // remove last "|" character
                                    break;
                                case 0x3D:
                                    DescriptionToInsert = "ENGINE LOAD";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST F8 3D 3E";
                                        break;
                                    }

                                    double EngineLoadB = ((payload[1] << 8) + payload[3]) * 0.00390625;

                                    ValueToInsert = Math.Round(EngineLoadB, 1).ToString("0.0").Replace(",", ".");
                                    UnitToInsert = "PERCENT";
                                    break;
                                case 0x3E:
                                    DescriptionToInsert = "ENGINE LOAD | ERROR: REQUEST F8 3D 3E";
                                    break;
                                case 0x3F:
                                    DescriptionToInsert = "APP SENSOR PERCENT";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST F8 3F 40";
                                        break;
                                    }

                                    double APPSPercentB = ((payload[1] << 8) + payload[3]) * 0.25;

                                    ValueToInsert = Math.Round(APPSPercentB, 1).ToString("0.0").Replace(",", ".");
                                    UnitToInsert = "PERCENT";
                                    break;
                                case 0x40:
                                    DescriptionToInsert = "APP SENSOR PERCENT | ERROR: REQUEST F8 3F 40";
                                    break;
                                case 0x41:
                                    DescriptionToInsert = "BOOST PRESSURE";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST F8 42 43";
                                        break;
                                    }

                                    double BoostPressureBPSI = ((payload[1] << 8) + payload[3]) * 0.0078125;
                                    double BoostPressureBKPA = BoostPressureBPSI * 6.894757;

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        ValueToInsert = Math.Round(BoostPressureBPSI, 3).ToString("0.000").Replace(",", ".");
                                        UnitToInsert = "PSI";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        ValueToInsert = Math.Round(BoostPressureBKPA, 3).ToString("0.000").Replace(",", ".");
                                        UnitToInsert = "KPA";
                                    }
                                    break;
                                case 0x42:
                                    DescriptionToInsert = "BOOST PRESSURE | ERROR: REQUEST F8 42 43";
                                    break;
                                case 0x43:
                                    DescriptionToInsert = "ENGINE COOLANT TEMPERATURE";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST F8 44 45";
                                        break;
                                    }

                                    double ECT2F = ((payload[1] << 8) + payload[3]) * 0.015625;
                                    double ECT2C = (ECT2F - 32.0) / 1.8;

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        ValueToInsert = Math.Round(ECT2F, 1).ToString("0.0").Replace(",", ".");
                                        UnitToInsert = "°F";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        ValueToInsert = Math.Round(ECT2C, 1).ToString("0.0").Replace(",", ".");
                                        UnitToInsert = "°C";
                                    }
                                    break;
                                case 0x44:
                                    DescriptionToInsert = "ENGINE COOLANT TEMPERATURE | ERROR: REQUEST F8 44 45";
                                    break;
                                case 0x45:
                                    DescriptionToInsert = "INTAKE AIR TEMPERATURE";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST F8 46 47";
                                        break;
                                    }

                                    double IAT2F = ((payload[1] << 8) + payload[3]) * 0.015625;
                                    double IAT2C = (IAT2F - 32.0) / 1.8;

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        ValueToInsert = Math.Round(IAT2F, 1).ToString("0.0").Replace(",", ".");
                                        UnitToInsert = "°F";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        ValueToInsert = Math.Round(IAT2C, 1).ToString("0.0").Replace(",", ".");
                                        UnitToInsert = "°C";
                                    }
                                    break;
                                case 0x46:
                                    DescriptionToInsert = "INTAKE AIR TEMPERATURE | ERROR: REQUEST F8 46 47";
                                    break;
                                case 0x47:
                                    DescriptionToInsert = "OIL PRESSURE";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST F8 48 49";
                                        break;
                                    }

                                    double OilPressureBPSI = ((payload[1] << 8) + payload[3]) * 0.0078125;
                                    double OilPressureBKPA = OilPressureBPSI * 6.894757;

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        ValueToInsert = Math.Round(OilPressureBPSI, 3).ToString("0.000").Replace(",", ".");
                                        UnitToInsert = "PSI";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        ValueToInsert = Math.Round(OilPressureBKPA, 3).ToString("0.000").Replace(",", ".");
                                        UnitToInsert = "KPA";
                                    }
                                    break;
                                case 0x48:
                                    DescriptionToInsert = "OIL PRESSURE | ERROR: REQUEST F8 48 49";
                                    break;
                                case 0x49:
                                    DescriptionToInsert = "SWITCH STATUS 2";

                                    List<string> SwitchListD = new List<string>();

                                    if (Util.IsBitSet(payload[1], 7)) SwitchListD.Add("-7-");
                                    if (Util.IsBitSet(payload[1], 6)) SwitchListD.Add("-6-");
                                    if (Util.IsBitSet(payload[1], 5)) SwitchListD.Add("-5-");
                                    if (Util.IsBitSet(payload[1], 4)) SwitchListD.Add("-4-");
                                    if (Util.IsBitSet(payload[1], 3)) SwitchListD.Add("INTHEAT2"); // intake heater 2
                                    if (Util.IsBitSet(payload[1], 2)) SwitchListD.Add("INTHEAT1"); // intake heater 1
                                    if (Util.IsBitSet(payload[1], 1)) SwitchListD.Add("TRFPMPDR"); // transfer pump driver
                                    if (Util.IsBitSet(payload[1], 0)) SwitchListD.Add("-0-");

                                    if (SwitchListD.Count == 0) break;

                                    DescriptionToInsert += ": ";

                                    foreach (string s in SwitchListD)
                                    {
                                        DescriptionToInsert += s + " | ";
                                    }

                                    if (DescriptionToInsert.Length > 2) DescriptionToInsert = DescriptionToInsert.Remove(DescriptionToInsert.Length - 3); // remove last "|" character
                                    break;
                                case 0x50:
                                    DescriptionToInsert = "FINAL FUEL STATE";

                                    switch (payload[1])
                                    {
                                        case 0x00:
                                            ValueToInsert = "NOT SET";
                                            break;
                                        case 0x01:
                                            ValueToInsert = "JCOM TORQUE";
                                            break;
                                        case 0x02:
                                            ValueToInsert = "JCOM SPEED";
                                            break;
                                        case 0x03:
                                            ValueToInsert = "PROGRSV SHIFT";
                                            break;
                                        case 0x04:
                                            ValueToInsert = "PTO";
                                            break;
                                        case 0x05:
                                            ValueToInsert = "USER COMMAND";
                                            break;
                                        case 0x06:
                                            ValueToInsert = "LIMP HOME";
                                            break;
                                        case 0x07:
                                            ValueToInsert = "ASG THROTTLE";
                                            break;
                                        case 0x08:
                                            ValueToInsert = "4-D FUELING";
                                            break;
                                        case 0x09:
                                            ValueToInsert = "CRUISE CONTROL";
                                            break;
                                        case 0x0A:
                                            ValueToInsert = "ROAD SPEED GOV";
                                            break;
                                        case 0x0B:
                                            ValueToInsert = "LOW SPEED GOV";
                                            break;
                                        case 0x0C:
                                            ValueToInsert = "HIGH SPEED GOV";
                                            break;
                                        case 0x0D:
                                            ValueToInsert = "TORQUE DERATE OVERRIDE";
                                            break;
                                        case 0x0E:
                                            ValueToInsert = "LOW GEAR";
                                            break;
                                        case 0x0F:
                                            ValueToInsert = "ALTITUDE DERATE";
                                            break;
                                        case 0x10:
                                            ValueToInsert = "AFC DERATE";
                                            break;
                                        case 0x11:
                                            ValueToInsert = "ANC DERATE";
                                            break;
                                        case 0x12:
                                            ValueToInsert = "ENGINE PROTECT";
                                            break;
                                        case 0x13:
                                            ValueToInsert = "TORQUE CRV LIMIT";
                                            break;
                                        case 0x14:
                                            ValueToInsert = "JCOM TORQUE DERATE";
                                            break;
                                        case 0x15:
                                            ValueToInsert = "OUT OF GEAR";
                                            break;
                                        case 0x16:
                                            ValueToInsert = "CRANKING";
                                            break;
                                        case 0x17:
                                            ValueToInsert = "USER OVERRIDE";
                                            break;
                                        case 0x18:
                                            ValueToInsert = "ENGINE BRAKE";
                                            break;
                                        case 0x19:
                                            ValueToInsert = "ENGINE OVERSPEED";
                                            break;
                                        case 0x1A:
                                            ValueToInsert = "ENGINE STOPPED";
                                            break;
                                        case 0x1B:
                                            ValueToInsert = "SHUTDOWN";
                                            break;
                                        case 0x1C:
                                            ValueToInsert = "FUEL DTC DERATE";
                                            break;
                                        case 0x1D:
                                            ValueToInsert = "ENGINE PROTECT";
                                            break;
                                        case 0x1E:
                                            ValueToInsert = "ALL SPD GOV APP";
                                            break;
                                        case 0x1F:
                                            ValueToInsert = "ALT TORQUE";
                                            break;
                                        case 0x20:
                                            ValueToInsert = "MASTER/SLAVE OVERRIDE";
                                            break;
                                        case 0x21:
                                            ValueToInsert = "STARTUP OIL LIMIT";
                                            break;
                                        case 0x22:
                                            ValueToInsert = "PTO DERATE";
                                            break;
                                        case 0x23:
                                            ValueToInsert = "TORQUE CONTROL";
                                            break;
                                        case 0x24:
                                            ValueToInsert = "POWERTRAIN PROTECT";
                                            break;
                                        case 0x25:
                                            ValueToInsert = "T2 SPEED";
                                            break;
                                        case 0x26:
                                            ValueToInsert = "T2 TORQUE DERATE";
                                            break;
                                        case 0x27:
                                            ValueToInsert = "T2 DERATE";
                                            break;
                                        case 0x28:
                                            ValueToInsert = "NO DERATE";
                                            break;
                                        case 0x29:
                                            ValueToInsert = "ANTI THEFT DERATE";
                                            break;
                                        case 0x2A:
                                            ValueToInsert = "PART THROTTLE LIMIT";
                                            break;
                                        case 0x2B:
                                            ValueToInsert = "STEADY-STATE AMB DERATE";
                                            break;
                                        case 0x2C:
                                            ValueToInsert = "TRSNT COOLANT DERATE";
                                            break;
                                    }
                                    break;
                                case 0x51:
                                    DescriptionToInsert = "BATTERY VOLTAGE";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST F8 51 52";
                                        break;
                                    }

                                    double BatteryVoltsB = ((payload[1] << 8) + payload[3]) * 0.0625;

                                    ValueToInsert = Math.Round(BatteryVoltsB, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0x52:
                                    DescriptionToInsert = "BATTERY VOLTAGE | ERROR: REQUEST F8 51 52";
                                    break;
                                case 0x53:
                                    DescriptionToInsert = "CALCULATED FUEL";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST F8 53 54";
                                        break;
                                    }

                                    double CalculatedFuelB = ((payload[1] << 8) + payload[3]) * 0.001953155;

                                    ValueToInsert = Math.Round(CalculatedFuelB, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "PERCENT";
                                    break;
                                case 0x54:
                                    DescriptionToInsert = "CALCULATED FUEL | ERROR: REQUEST F8 53 54";
                                    break;
                                case 0x55:
                                    DescriptionToInsert = "CALCULATED TIMING";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST F8 55 56";
                                        break;
                                    }

                                    double CalculatedTimingB = (((payload[1] << 8) + payload[3]) * 0.1176475) - 20.0;

                                    ValueToInsert = Math.Round(CalculatedTimingB, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "DEGREE";
                                    break;
                                case 0x56:
                                    DescriptionToInsert = "CALCULATED TIMING | ERROR: REQUEST F8 55 56";
                                    break;
                                case 0x57:
                                    DescriptionToInsert = "REGULATOR VALVE CURRENT";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST F8 57 58";
                                        break;
                                    }

                                    double RVCB = ((payload[1] << 8) + payload[3]) * 1.220721752;

                                    ValueToInsert = Math.Round(RVCB, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "MILLIAMPS";
                                    break;
                                case 0x58:
                                    DescriptionToInsert = "REGULATOR VALVE CURRENT | ERROR: REQUEST F8 57 58";
                                    break;
                                case 0x59:
                                    DescriptionToInsert = "INJECTOR PUMP FUEL TEMPERATURE";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST F8 59 5A";
                                        break;
                                    }

                                    double InjPumpFuelTempBF = ((payload[1] << 8) + payload[3]) * 0.0625;
                                    double InjPumpFuelTempBC = (InjPumpFuelTempBF - 32.0) / 1.8;

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        ValueToInsert = Math.Round(InjPumpFuelTempBF, 1).ToString("0.0").Replace(",", ".");
                                        UnitToInsert = "°F";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        ValueToInsert = Math.Round(InjPumpFuelTempBC, 1).ToString("0.0").Replace(",", ".");
                                        UnitToInsert = "°C";
                                    }
                                    break;
                                case 0x5A:
                                    DescriptionToInsert = "INJECTOR PUMP FUEL TEMPERATURE | ERROR: REQUEST F8 59 5A";
                                    break;
                                case 0x5B:
                                    DescriptionToInsert = "INJECTOR PUMP ENGINE SPEED";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST F8 5B 5C";
                                        break;
                                    }

                                    double InjPumpEngineSpeedB = ((payload[1] << 8) + payload[3]) * 0.125;

                                    ValueToInsert = Math.Round(InjPumpEngineSpeedB, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "RPM";
                                    break;
                                case 0x5C:
                                    DescriptionToInsert = "INJECTOR PUMP ENGINE SPEED | ERROR: REQUEST F8 5B 5C";
                                    break;
                                case 0x5F:
                                    DescriptionToInsert = "FREEZE FRM DTC 2:";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST F8 5F 60";
                                        break;
                                    }

                                    if ((payload[1] == 0x00) && (payload[3] == 0x00))
                                    {
                                        DescriptionToInsert += "EMPTY SLOT";
                                        break;
                                    }

                                    ValueToInsert = "OBD2 P" + Util.ByteToHexString(new byte[2] { payload[1], payload[3] }, 0, 2).Replace(" ", "");
                                    break;
                                case 0x60:
                                    DescriptionToInsert = "FREEZE FRAME DTC 2: | ERROR: REQUEST F8 5F 60";
                                    break;
                                default:
                                    for (int i = 0; i < HSBPNum; i++)
                                    {
                                        HSOffset.Add(payload[i * 2]);
                                        HSValues.Add(payload[(i * 2) + 1]);
                                    }

                                    DescriptionToInsert = "F8 RAM TABLE | OFFSET: " + Util.ByteToHexStringSimple(HSOffset.ToArray());
                                    ValueToInsert = Util.ByteToHexStringSimple(HSValues.ToArray());
                                    break;
                            }
                            break;
                        }
                        else if ((Year >= 2003) && (ControllerHardwareType == 9)) // CUMMINS
                        {
                            switch (payload[0])
                            {
                                case 0x06:
                                    DescriptionToInsert = "VEHICLE SPEED";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST F8 06 07";
                                        break;
                                    }

                                    double VehicleSpeedAMPH = ((payload[1] << 8) + payload[3]) * 0.00390625;
                                    double VehicleSpeedAKMH = VehicleSpeedAMPH * 1.609344;

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        ValueToInsert = Math.Round(VehicleSpeedAMPH, 3).ToString("0.000").Replace(",", ".");
                                        UnitToInsert = "MPH";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        ValueToInsert = Math.Round(VehicleSpeedAKMH, 3).ToString("0.000").Replace(",", ".");
                                        UnitToInsert = "KM/H";
                                    }
                                    break;
                                case 0x07:
                                    DescriptionToInsert = "VEHICLE SPEED | ERROR: REQUEST F8 06 07";
                                    break;
                                case 0x08:
                                    DescriptionToInsert = "ENGINE SPEED";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST F8 08 09";
                                        break;
                                    }

                                    double EngineSpeedA = ((payload[1] << 8) + payload[3]) * 0.125;

                                    ValueToInsert = Math.Round(EngineSpeedA, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "RPM";
                                    break;
                                case 0x09:
                                    DescriptionToInsert = "ENGINE SPEED | ERROR: REQUEST F8 08 09";
                                    break;
                                case 0x0A:
                                    DescriptionToInsert = "SWITCH STATUS 1";

                                    List<string> SwitchListA = new List<string>();

                                    if (Util.IsBitSet(payload[1], 7)) SwitchListA.Add("-7-");
                                    if (Util.IsBitSet(payload[1], 6)) SwitchListA.Add("BRAKE");
                                    if (Util.IsBitSet(payload[1], 5)) SwitchListA.Add("-5-");
                                    if (Util.IsBitSet(payload[1], 4)) SwitchListA.Add("-4-");
                                    if (Util.IsBitSet(payload[1], 3)) SwitchListA.Add("-3-");
                                    if (Util.IsBitSet(payload[1], 2)) SwitchListA.Add("-2-");
                                    if (Util.IsBitSet(payload[1], 1)) SwitchListA.Add("-1-");
                                    if (Util.IsBitSet(payload[1], 0)) SwitchListA.Add("-0-");

                                    if (SwitchListA.Count == 0) break;

                                    DescriptionToInsert += ": ";

                                    foreach (string s in SwitchListA)
                                    {
                                        DescriptionToInsert += s + " | ";
                                    }

                                    if (DescriptionToInsert.Length > 2) DescriptionToInsert = DescriptionToInsert.Remove(DescriptionToInsert.Length - 3); // remove last "|" character
                                    break;
                                case 0x0B:
                                    DescriptionToInsert = "SWITCH STATUS 2";

                                    List<string> SwitchListB = new List<string>();

                                    if (Util.IsBitSet(payload[1], 7)) SwitchListB.Add("-7-");
                                    if (Util.IsBitSet(payload[1], 6)) SwitchListB.Add("BRAKE");
                                    if (Util.IsBitSet(payload[1], 5)) SwitchListB.Add("IDLE");
                                    if (Util.IsBitSet(payload[1], 4)) SwitchListB.Add("-4-");
                                    if (Util.IsBitSet(payload[1], 3)) SwitchListB.Add("-3-");
                                    if (Util.IsBitSet(payload[1], 2)) SwitchListB.Add("NOT-IDLE");
                                    if (Util.IsBitSet(payload[1], 1)) SwitchListB.Add("-1-");
                                    if (Util.IsBitSet(payload[1], 0)) SwitchListB.Add("-0-");

                                    if (SwitchListB.Count == 0) break;

                                    DescriptionToInsert += ": ";

                                    foreach (string s in SwitchListB)
                                    {
                                        DescriptionToInsert += s + " | ";
                                    }

                                    if (DescriptionToInsert.Length > 2) DescriptionToInsert = DescriptionToInsert.Remove(DescriptionToInsert.Length - 3); // remove last "|" character
                                    break;
                                case 0x0C:
                                    DescriptionToInsert = "ENGINE LOAD";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST F8 0C 0D";
                                        break;
                                    }

                                    double EngineLoadA = ((payload[1] << 8) + payload[3]) * 0.00390625;

                                    ValueToInsert = Math.Round(EngineLoadA, 1).ToString("0.0").Replace(",", ".");
                                    UnitToInsert = "PERCENT";
                                    break;
                                case 0x0D:
                                    DescriptionToInsert = "ENGINE LOAD | ERROR: REQUEST F8 0C 0D";
                                    break;
                                case 0x0E:
                                    DescriptionToInsert = "APP SENSOR PERCENT";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST F8 0E 0F";
                                        break;
                                    }

                                    double APPSPercentA = ((payload[1] << 8) + payload[3]) * 0.25;

                                    ValueToInsert = Math.Round(APPSPercentA, 1).ToString("0.0").Replace(",", ".");
                                    UnitToInsert = "PERCENT";
                                    break;
                                case 0x0F:
                                    DescriptionToInsert = "APP SENSOR PERCENT | ERROR: REQUEST F8 0E 0F";
                                    break;
                                case 0x10:
                                    DescriptionToInsert = "BOOST PRESSURE";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST F8 10 11";
                                        break;
                                    }

                                    double BoostPressureAPSI = ((payload[1] << 8) + payload[3]) * 0.0078125;
                                    double BoostPressureAKPA = BoostPressureAPSI * 6.894757;

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        ValueToInsert = Math.Round(BoostPressureAPSI, 3).ToString("0.000").Replace(",", ".");
                                        UnitToInsert = "PSI";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        ValueToInsert = Math.Round(BoostPressureAKPA, 3).ToString("0.000").Replace(",", ".");
                                        UnitToInsert = "KPA";
                                    }
                                    break;
                                case 0x11:
                                    DescriptionToInsert = "BOOST PRESSURE | ERROR: REQUEST F8 10 11";
                                    break;
                                case 0x12:
                                    DescriptionToInsert = "ENGINE COOLANT TEMPERATURE";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST F8 12 13";
                                        break;
                                    }

                                    double ECT1F = ((payload[1] << 8) + payload[3]) * 0.015625;
                                    double ECT1C = (ECT1F - 32.0) / 1.8;

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        ValueToInsert = Math.Round(ECT1F, 1).ToString("0.0").Replace(",", ".");
                                        UnitToInsert = "°F";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        ValueToInsert = Math.Round(ECT1C, 1).ToString("0.0").Replace(",", ".");
                                        UnitToInsert = "°C";
                                    }
                                    break;
                                case 0x13:
                                    DescriptionToInsert = "ENGINE COOLANT TEMPERATURE | ERROR: REQUEST F8 12 13";
                                    break;
                                case 0x14:
                                    DescriptionToInsert = "INTAKE AIR TEMPERATURE";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST F8 14 15";
                                        break;
                                    }

                                    double IAT1F = ((payload[1] << 8) + payload[3]) * 0.015625;
                                    double IAT1C = (IAT1F - 32.0) / 1.8;

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        ValueToInsert = Math.Round(IAT1F, 1).ToString("0.0").Replace(",", ".");
                                        UnitToInsert = "°F";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        ValueToInsert = Math.Round(IAT1C, 1).ToString("0.0").Replace(",", ".");
                                        UnitToInsert = "°C";
                                    }
                                    break;
                                case 0x15:
                                    DescriptionToInsert = "INTAKE AIR TEMPERATURE | ERROR: REQUEST F8 14 15";
                                    break;
                                case 0x19:
                                    DescriptionToInsert = "SWITCH STATUS 3";

                                    List<string> SwitchListC = new List<string>();

                                    if (Util.IsBitSet(payload[1], 7)) SwitchListC.Add("-7-");
                                    if (Util.IsBitSet(payload[1], 6)) SwitchListC.Add("-6-");
                                    if (Util.IsBitSet(payload[1], 5)) SwitchListC.Add("-5-");
                                    if (Util.IsBitSet(payload[1], 4)) SwitchListC.Add("-4-");
                                    if (Util.IsBitSet(payload[1], 3)) SwitchListC.Add("INTHEAT2"); // intake heater 2
                                    if (Util.IsBitSet(payload[1], 2)) SwitchListC.Add("INTHEAT1"); // intake heater 1
                                    if (Util.IsBitSet(payload[1], 1)) SwitchListC.Add("TRFPMPDR"); // transfer pump driver
                                    if (Util.IsBitSet(payload[1], 0)) SwitchListC.Add("-0-");

                                    if (SwitchListC.Count == 0) break;

                                    DescriptionToInsert += ": ";

                                    foreach (string s in SwitchListC)
                                    {
                                        DescriptionToInsert += s + " | ";
                                    }

                                    if (DescriptionToInsert.Length > 2) DescriptionToInsert = DescriptionToInsert.Remove(DescriptionToInsert.Length - 3); // remove last "|" character
                                    break;
                                case 0x1E:
                                    DescriptionToInsert = "FINAL FUEL STATE";

                                    switch (payload[1])
                                    {
                                        case 0x00:
                                            ValueToInsert = "NOT SET";
                                            break;
                                        case 0x01:
                                            ValueToInsert = "JCOM TORQUE";
                                            break;
                                        case 0x02:
                                            ValueToInsert = "JCOM SPEED";
                                            break;
                                        case 0x03:
                                            ValueToInsert = "PROGRSV SHIFT";
                                            break;
                                        case 0x04:
                                            ValueToInsert = "PTO";
                                            break;
                                        case 0x05:
                                            ValueToInsert = "USER COMMAND";
                                            break;
                                        case 0x06:
                                            ValueToInsert = "LIMP HOME";
                                            break;
                                        case 0x07:
                                            ValueToInsert = "ASG THROTTLE";
                                            break;
                                        case 0x08:
                                            ValueToInsert = "4-D FUELING";
                                            break;
                                        case 0x09:
                                            ValueToInsert = "CRUISE CONTROL";
                                            break;
                                        case 0x0A:
                                            ValueToInsert = "ROAD SPEED GOV";
                                            break;
                                        case 0x0B:
                                            ValueToInsert = "LOW SPEED GOV";
                                            break;
                                        case 0x0C:
                                            ValueToInsert = "HIGH SPEED GOV";
                                            break;
                                        case 0x0D:
                                            ValueToInsert = "TORQUE DERATE OVERRIDE";
                                            break;
                                        case 0x0E:
                                            ValueToInsert = "LOW GEAR";
                                            break;
                                        case 0x0F:
                                            ValueToInsert = "ALTITUDE DERATE";
                                            break;
                                        case 0x10:
                                            ValueToInsert = "AFC DERATE";
                                            break;
                                        case 0x11:
                                            ValueToInsert = "ANC DERATE";
                                            break;
                                        case 0x12:
                                            ValueToInsert = "ENGINE PROTECT";
                                            break;
                                        case 0x13:
                                            ValueToInsert = "TORQUE CRV LIMIT";
                                            break;
                                        case 0x14:
                                            ValueToInsert = "JCOM TORQUE DERATE";
                                            break;
                                        case 0x15:
                                            ValueToInsert = "OUT OF GEAR";
                                            break;
                                        case 0x16:
                                            ValueToInsert = "CRANKING";
                                            break;
                                        case 0x17:
                                            ValueToInsert = "USER OVERRIDE";
                                            break;
                                        case 0x18:
                                            ValueToInsert = "ENGINE BRAKE";
                                            break;
                                        case 0x19:
                                            ValueToInsert = "ENGINE OVERSPEED";
                                            break;
                                        case 0x1A:
                                            ValueToInsert = "ENGINE STOPPED";
                                            break;
                                        case 0x1B:
                                            ValueToInsert = "SHUTDOWN";
                                            break;
                                        case 0x1C:
                                            ValueToInsert = "FUEL DTC DERATE";
                                            break;
                                        case 0x1D:
                                            ValueToInsert = "ENGINE PROTECT";
                                            break;
                                        case 0x1E:
                                            ValueToInsert = "ALL SPD GOV APP";
                                            break;
                                        case 0x1F:
                                            ValueToInsert = "ALT TORQUE";
                                            break;
                                        case 0x20:
                                            ValueToInsert = "MASTER/SLAVE OVERRIDE";
                                            break;
                                        case 0x21:
                                            ValueToInsert = "STARTUP OIL LIMIT";
                                            break;
                                        case 0x22:
                                            ValueToInsert = "PTO DERATE";
                                            break;
                                        case 0x23:
                                            ValueToInsert = "TORQUE CONTROL";
                                            break;
                                        case 0x24:
                                            ValueToInsert = "POWERTRAIN PROTECT";
                                            break;
                                        case 0x25:
                                            ValueToInsert = "T2 SPEED";
                                            break;
                                        case 0x26:
                                            ValueToInsert = "T2 TORQUE DERATE";
                                            break;
                                        case 0x27:
                                            ValueToInsert = "T2 DERATE";
                                            break;
                                        case 0x28:
                                            ValueToInsert = "NO DERATE";
                                            break;
                                        case 0x29:
                                            ValueToInsert = "ANTI THEFT DERATE";
                                            break;
                                        case 0x2A:
                                            ValueToInsert = "PART THROTTLE LIMIT";
                                            break;
                                        case 0x2B:
                                            ValueToInsert = "STEADY-STATE AMB DERATE";
                                            break;
                                        case 0x2C:
                                            ValueToInsert = "TRSNT COOLANT DERATE";
                                            break;
                                    }
                                    break;
                                case 0x1F:
                                    DescriptionToInsert = "BATTERY VOLTAGE";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST F8 1F 20";
                                        break;
                                    }

                                    double BatteryVoltsA = ((payload[1] << 8) + payload[3]) * 0.0625;

                                    ValueToInsert = Math.Round(BatteryVoltsA, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0x20:
                                    DescriptionToInsert = "BATTERY VOLTAGE | ERROR: REQUEST F8 1F 20";
                                    break;
                                case 0x21:
                                    DescriptionToInsert = "CALCULATED FUEL";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST F8 21 22";
                                        break;
                                    }

                                    double CalculatedFuelA = ((payload[1] << 8) + payload[3]) * 0.001953155;

                                    ValueToInsert = Math.Round(CalculatedFuelA, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "PERCENT";
                                    break;
                                case 0x22:
                                    DescriptionToInsert = "CALCULATED FUEL | ERROR: REQUEST F8 21 22";
                                    break;
                                case 0x23:
                                    DescriptionToInsert = "CALCULATED TIMING";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST F8 23 24";
                                        break;
                                    }

                                    double CalculatedTimingA = (((payload[1] << 8) + payload[3]) * 0.1176475) - 20.0;

                                    ValueToInsert = Math.Round(CalculatedTimingA, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "DEGREE";
                                    break;
                                case 0x24:
                                    DescriptionToInsert = "CALCULATED TIMING | ERROR: REQUEST F8 23 24";
                                    break;
                                case 0x25:
                                    DescriptionToInsert = "REGULATOR VALVE CURRENT";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST F8 25 26";
                                        break;
                                    }

                                    double RVCA = ((payload[1] << 8) + payload[3]) * 1.220721752;

                                    ValueToInsert = Math.Round(RVCA, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "MILLIAMPS";
                                    break;
                                case 0x26:
                                    DescriptionToInsert = "REGULATOR VALVE CURRENT | ERROR: REQUEST F8 25 26";
                                    break;
                                case 0x27:
                                    DescriptionToInsert = "DEFECT STATUS";

                                    switch (payload[1])
                                    {
                                        case 0x00:
                                            ValueToInsert = "OK";
                                            break;
                                        case 0x01:
                                            ValueToInsert = "CURRENT HIGH";
                                            break;
                                        case 0x02:
                                            ValueToInsert = "CURRENT LOW";
                                            break;
                                        default:
                                            break;
                                    }
                                    break;
                                case 0x28:
                                    DescriptionToInsert = "FUEL PRESSURE STATUS";

                                    switch (payload[1])
                                    {
                                        case 0x00:
                                            ValueToInsert = "OK";
                                            break;
                                        case 0x01:
                                            ValueToInsert = "TOO HIGH";
                                            break;
                                        case 0x02:
                                            ValueToInsert = "LIMIT";
                                            break;
                                        case 0x04:
                                            ValueToInsert = "TOO LOW";
                                            break;
                                        case 0x08:
                                            ValueToInsert = "NEG DEV";
                                            break;
                                        case 0x10:
                                            ValueToInsert = "POS DEV";
                                            break;
                                        case 0x20:
                                            ValueToInsert = "LK MON";
                                            break;
                                        case 0x40:
                                            ValueToInsert = "LK IDLE";
                                            break;
                                        default:
                                            break;
                                    }
                                    break;
                                case 0x29:
                                    DescriptionToInsert = "FUEL PRESSURE VOLTS";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST F8 29 2A";
                                        break;
                                    }

                                    double FuelPressureVoltsA = ((payload[1] << 8) + payload[3]) * 0.0049;

                                    ValueToInsert = Math.Round(FuelPressureVoltsA, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0x2A:
                                    DescriptionToInsert = "FUEL PRESSURE VOLTS | ERROR: REQUEST F8 29 2A";
                                    break;
                                case 0x2D:
                                    DescriptionToInsert = "FUEL LEVEL PERCENT";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST F8 2D 2E";
                                        break;
                                    }

                                    double FuelLevelPercentA = ((payload[1] << 8) + payload[3]) * 0.3921568627;

                                    ValueToInsert = Math.Round(FuelLevelPercentA, 1).ToString("0.0").Replace(",", ".");
                                    UnitToInsert = "PERCENT";
                                    break;
                                case 0x2E:
                                    DescriptionToInsert = "FUEL LEVEL PERCENT | ERROR: REQUEST F8 2D 2E";
                                    break;
                                case 0x31:
                                    DescriptionToInsert = "FREEZE FRM DTC 1:";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST F8 31 32";
                                        break;
                                    }

                                    if ((payload[1] == 0x00) && (payload[3] == 0x00))
                                    {
                                        DescriptionToInsert += "EMPTY SLOT";
                                        break;
                                    }

                                    ValueToInsert = "OBD2 P" + Util.ByteToHexString(new byte[2] { payload[1], payload[3] }, 0, 2).Replace(" ", "");
                                    break;
                                case 0x32:
                                    DescriptionToInsert = "FREEZE FRAME DTC 1: | ERROR: REQUEST F8 31 32";
                                    break;
                                case 0x38:
                                    DescriptionToInsert = "VEHICLE SPEED";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST F8 38 39";
                                        break;
                                    }

                                    double VehicleSpeedBMPH = ((payload[1] << 8) + payload[3]) * 0.00390625;
                                    double VehicleSpeedBKMH = VehicleSpeedBMPH * 1.609344;

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        ValueToInsert = Math.Round(VehicleSpeedBMPH, 3).ToString("0.000").Replace(",", ".");
                                        UnitToInsert = "MPH";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        ValueToInsert = Math.Round(VehicleSpeedBKMH, 3).ToString("0.000").Replace(",", ".");
                                        UnitToInsert = "KM/H";
                                    }
                                    break;
                                case 0x39:
                                    DescriptionToInsert = "VEHICLE SPEED | ERROR: REQUEST F8 38 39";
                                    break;
                                case 0x3A:
                                    DescriptionToInsert = "ENGINE SPEED";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST F8 3A 3B";
                                        break;
                                    }

                                    double EngineSpeedB = ((payload[1] << 8) + payload[3]) * 0.125;

                                    ValueToInsert = Math.Round(EngineSpeedB, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "RPM";
                                    break;
                                case 0x3B:
                                    DescriptionToInsert = "ENGINE SPEED | ERROR: REQUEST F8 3A 3B";
                                    break;
                                case 0x3C:
                                    DescriptionToInsert = "SWITCH STATUS 1";

                                    List<string> SwitchListD = new List<string>();

                                    if (Util.IsBitSet(payload[1], 7)) SwitchListD.Add("-7-");
                                    if (Util.IsBitSet(payload[1], 6)) SwitchListD.Add("BRAKE");
                                    if (Util.IsBitSet(payload[1], 5)) SwitchListD.Add("-5-");
                                    if (Util.IsBitSet(payload[1], 4)) SwitchListD.Add("-4-");
                                    if (Util.IsBitSet(payload[1], 3)) SwitchListD.Add("-3-");
                                    if (Util.IsBitSet(payload[1], 2)) SwitchListD.Add("-2-");
                                    if (Util.IsBitSet(payload[1], 1)) SwitchListD.Add("-1-");
                                    if (Util.IsBitSet(payload[1], 0)) SwitchListD.Add("-0-");

                                    if (SwitchListD.Count == 0) break;

                                    DescriptionToInsert += ": ";

                                    foreach (string s in SwitchListD)
                                    {
                                        DescriptionToInsert += s + " | ";
                                    }

                                    if (DescriptionToInsert.Length > 2) DescriptionToInsert = DescriptionToInsert.Remove(DescriptionToInsert.Length - 3); // remove last "|" character
                                    break;
                                case 0x3D:
                                    DescriptionToInsert = "SWITCH STATUS 2";

                                    List<string> SwitchListE = new List<string>();

                                    if (Util.IsBitSet(payload[1], 7)) SwitchListE.Add("-7-");
                                    if (Util.IsBitSet(payload[1], 6)) SwitchListE.Add("BRAKE");
                                    if (Util.IsBitSet(payload[1], 5)) SwitchListE.Add("IDLE");
                                    if (Util.IsBitSet(payload[1], 4)) SwitchListE.Add("-4-");
                                    if (Util.IsBitSet(payload[1], 3)) SwitchListE.Add("-3-");
                                    if (Util.IsBitSet(payload[1], 2)) SwitchListE.Add("NOT-IDLE");
                                    if (Util.IsBitSet(payload[1], 1)) SwitchListE.Add("-1-");
                                    if (Util.IsBitSet(payload[1], 0)) SwitchListE.Add("-0-");

                                    if (SwitchListE.Count == 0) break;

                                    DescriptionToInsert += ": ";

                                    foreach (string s in SwitchListE)
                                    {
                                        DescriptionToInsert += s + " | ";
                                    }

                                    if (DescriptionToInsert.Length > 2) DescriptionToInsert = DescriptionToInsert.Remove(DescriptionToInsert.Length - 3); // remove last "|" character
                                    break;
                                case 0x3E:
                                    DescriptionToInsert = "ENGINE LOAD";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST F8 3E 3F";
                                        break;
                                    }

                                    double EngineLoadB = ((payload[1] << 8) + payload[3]) * 0.00390625;

                                    ValueToInsert = Math.Round(EngineLoadB, 1).ToString("0.0").Replace(",", ".");
                                    UnitToInsert = "PERCENT";
                                    break;
                                case 0x3F:
                                    DescriptionToInsert = "ENGINE LOAD | ERROR: REQUEST F8 3E 3F";
                                    break;
                                case 0x40:
                                    DescriptionToInsert = "APP SENSOR PERCENT";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST F8 40 41";
                                        break;
                                    }

                                    double APPSPercentB = ((payload[1] << 8) + payload[3]) * 0.25;

                                    ValueToInsert = Math.Round(APPSPercentB, 1).ToString("0.0").Replace(",", ".");
                                    UnitToInsert = "PERCENT";
                                    break;
                                case 0x41:
                                    DescriptionToInsert = "APP SENSOR PERCENT | ERROR: REQUEST F8 40 41";
                                    break;
                                case 0x42:
                                    DescriptionToInsert = "BOOST PRESSURE";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST F8 42 43";
                                        break;
                                    }

                                    double BoostPressureBPSI = ((payload[1] << 8) + payload[3]) * 0.0078125;
                                    double BoostPressureBKPA = BoostPressureBPSI * 6.894757;

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        ValueToInsert = Math.Round(BoostPressureBPSI, 3).ToString("0.000").Replace(",", ".");
                                        UnitToInsert = "PSI";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        ValueToInsert = Math.Round(BoostPressureBKPA, 3).ToString("0.000").Replace(",", ".");
                                        UnitToInsert = "KPA";
                                    }
                                    break;
                                case 0x43:
                                    DescriptionToInsert = "BOOST PRESSURE | ERROR: REQUEST F8 42 43";
                                    break;
                                case 0x44:
                                    DescriptionToInsert = "ENGINE COOLANT TEMPERATURE";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST F8 44 45";
                                        break;
                                    }

                                    double ECT2F = ((payload[1] << 8) + payload[3]) * 0.015625;
                                    double ECT2C = (ECT2F - 32.0) / 1.8;

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        ValueToInsert = Math.Round(ECT2F, 1).ToString("0.0").Replace(",", ".");
                                        UnitToInsert = "°F";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        ValueToInsert = Math.Round(ECT2C, 1).ToString("0.0").Replace(",", ".");
                                        UnitToInsert = "°C";
                                    }
                                    break;
                                case 0x45:
                                    DescriptionToInsert = "ENGINE COOLANT TEMPERATURE | ERROR: REQUEST F8 44 45";
                                    break;
                                case 0x46:
                                    DescriptionToInsert = "INTAKE AIR TEMPERATURE";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST F8 46 47";
                                        break;
                                    }

                                    double IAT2F = ((payload[1] << 8) + payload[3]) * 0.015625;
                                    double IAT2C = (IAT2F - 32.0) / 1.8;

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        ValueToInsert = Math.Round(IAT2F, 1).ToString("0.0").Replace(",", ".");
                                        UnitToInsert = "°F";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        ValueToInsert = Math.Round(IAT2C, 1).ToString("0.0").Replace(",", ".");
                                        UnitToInsert = "°C";
                                    }
                                    break;
                                case 0x47:
                                    DescriptionToInsert = "INTAKE AIR TEMPERATURE | ERROR: REQUEST F8 46 47";
                                    break;
                                case 0x4B:
                                    DescriptionToInsert = "SWITCH STATUS 3";

                                    List<string> SwitchListF = new List<string>();

                                    if (Util.IsBitSet(payload[1], 7)) SwitchListF.Add("-7-");
                                    if (Util.IsBitSet(payload[1], 6)) SwitchListF.Add("-6-");
                                    if (Util.IsBitSet(payload[1], 5)) SwitchListF.Add("-5-");
                                    if (Util.IsBitSet(payload[1], 4)) SwitchListF.Add("-4-");
                                    if (Util.IsBitSet(payload[1], 3)) SwitchListF.Add("INTHEAT2"); // intake heater 2
                                    if (Util.IsBitSet(payload[1], 2)) SwitchListF.Add("INTHEAT1"); // intake heater 1
                                    if (Util.IsBitSet(payload[1], 1)) SwitchListF.Add("TRFPMPDR"); // transfer pump driver
                                    if (Util.IsBitSet(payload[1], 0)) SwitchListF.Add("-0-");

                                    if (SwitchListF.Count == 0) break;

                                    DescriptionToInsert += ": ";

                                    foreach (string s in SwitchListF)
                                    {
                                        DescriptionToInsert += s + " | ";
                                    }

                                    if (DescriptionToInsert.Length > 2) DescriptionToInsert = DescriptionToInsert.Remove(DescriptionToInsert.Length - 3); // remove last "|" character
                                    break;
                                case 0x50:
                                    DescriptionToInsert = "FINAL FUEL STATE";

                                    switch (payload[1])
                                    {
                                        case 0x00:
                                            ValueToInsert = "NOT SET";
                                            break;
                                        case 0x01:
                                            ValueToInsert = "JCOM TORQUE";
                                            break;
                                        case 0x02:
                                            ValueToInsert = "JCOM SPEED";
                                            break;
                                        case 0x03:
                                            ValueToInsert = "PROGRSV SHIFT";
                                            break;
                                        case 0x04:
                                            ValueToInsert = "PTO";
                                            break;
                                        case 0x05:
                                            ValueToInsert = "USER COMMAND";
                                            break;
                                        case 0x06:
                                            ValueToInsert = "LIMP HOME";
                                            break;
                                        case 0x07:
                                            ValueToInsert = "ASG THROTTLE";
                                            break;
                                        case 0x08:
                                            ValueToInsert = "4-D FUELING";
                                            break;
                                        case 0x09:
                                            ValueToInsert = "CRUISE CONTROL";
                                            break;
                                        case 0x0A:
                                            ValueToInsert = "ROAD SPEED GOV";
                                            break;
                                        case 0x0B:
                                            ValueToInsert = "LOW SPEED GOV";
                                            break;
                                        case 0x0C:
                                            ValueToInsert = "HIGH SPEED GOV";
                                            break;
                                        case 0x0D:
                                            ValueToInsert = "TORQUE DERATE OVERRIDE";
                                            break;
                                        case 0x0E:
                                            ValueToInsert = "LOW GEAR";
                                            break;
                                        case 0x0F:
                                            ValueToInsert = "ALTITUDE DERATE";
                                            break;
                                        case 0x10:
                                            ValueToInsert = "AFC DERATE";
                                            break;
                                        case 0x11:
                                            ValueToInsert = "ANC DERATE";
                                            break;
                                        case 0x12:
                                            ValueToInsert = "ENGINE PROTECT";
                                            break;
                                        case 0x13:
                                            ValueToInsert = "TORQUE CRV LIMIT";
                                            break;
                                        case 0x14:
                                            ValueToInsert = "JCOM TORQUE DERATE";
                                            break;
                                        case 0x15:
                                            ValueToInsert = "OUT OF GEAR";
                                            break;
                                        case 0x16:
                                            ValueToInsert = "CRANKING";
                                            break;
                                        case 0x17:
                                            ValueToInsert = "USER OVERRIDE";
                                            break;
                                        case 0x18:
                                            ValueToInsert = "ENGINE BRAKE";
                                            break;
                                        case 0x19:
                                            ValueToInsert = "ENGINE OVERSPEED";
                                            break;
                                        case 0x1A:
                                            ValueToInsert = "ENGINE STOPPED";
                                            break;
                                        case 0x1B:
                                            ValueToInsert = "SHUTDOWN";
                                            break;
                                        case 0x1C:
                                            ValueToInsert = "FUEL DTC DERATE";
                                            break;
                                        case 0x1D:
                                            ValueToInsert = "ENGINE PROTECT";
                                            break;
                                        case 0x1E:
                                            ValueToInsert = "ALL SPD GOV APP";
                                            break;
                                        case 0x1F:
                                            ValueToInsert = "ALT TORQUE";
                                            break;
                                        case 0x20:
                                            ValueToInsert = "MASTER/SLAVE OVERRIDE";
                                            break;
                                        case 0x21:
                                            ValueToInsert = "STARTUP OIL LIMIT";
                                            break;
                                        case 0x22:
                                            ValueToInsert = "PTO DERATE";
                                            break;
                                        case 0x23:
                                            ValueToInsert = "TORQUE CONTROL";
                                            break;
                                        case 0x24:
                                            ValueToInsert = "POWERTRAIN PROTECT";
                                            break;
                                        case 0x25:
                                            ValueToInsert = "T2 SPEED";
                                            break;
                                        case 0x26:
                                            ValueToInsert = "T2 TORQUE DERATE";
                                            break;
                                        case 0x27:
                                            ValueToInsert = "T2 DERATE";
                                            break;
                                        case 0x28:
                                            ValueToInsert = "NO DERATE";
                                            break;
                                        case 0x29:
                                            ValueToInsert = "ANTI THEFT DERATE";
                                            break;
                                        case 0x2A:
                                            ValueToInsert = "PART THROTTLE LIMIT";
                                            break;
                                        case 0x2B:
                                            ValueToInsert = "STEADY-STATE AMB DERATE";
                                            break;
                                        case 0x2C:
                                            ValueToInsert = "TRSNT COOLANT DERATE";
                                            break;
                                    }
                                    break;
                                case 0x51:
                                    DescriptionToInsert = "BATTERY VOLTAGE";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST F8 51 52";
                                        break;
                                    }

                                    double BatteryVoltsB = ((payload[1] << 8) + payload[3]) * 0.0625;

                                    ValueToInsert = Math.Round(BatteryVoltsB, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0x52:
                                    DescriptionToInsert = "BATTERY VOLTAGE | ERROR: REQUEST F8 51 52";
                                    break;
                                case 0x53:
                                    DescriptionToInsert = "CALCULATED FUEL";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST F8 53 54";
                                        break;
                                    }

                                    double CalculatedFuelB = ((payload[1] << 8) + payload[3]) * 0.001953155;

                                    ValueToInsert = Math.Round(CalculatedFuelB, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "PERCENT";
                                    break;
                                case 0x54:
                                    DescriptionToInsert = "CALCULATED FUEL | ERROR: REQUEST F8 53 54";
                                    break;
                                case 0x55:
                                    DescriptionToInsert = "CALCULATED TIMING";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST F8 55 56";
                                        break;
                                    }

                                    double CalculatedTimingB = (((payload[1] << 8) + payload[3]) * 0.1176475) - 20.0;

                                    ValueToInsert = Math.Round(CalculatedTimingB, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "DEGREE";
                                    break;
                                case 0x56:
                                    DescriptionToInsert = "CALCULATED TIMING | ERROR: REQUEST F8 55 56";
                                    break;
                                case 0x57:
                                    DescriptionToInsert = "REGULATOR VALVE CURRENT";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST F8 57 58";
                                        break;
                                    }

                                    double RVCB = ((payload[1] << 8) + payload[3]) * 1.220721752;

                                    ValueToInsert = Math.Round(RVCB, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "MILLIAMPS";
                                    break;
                                case 0x58:
                                    DescriptionToInsert = "REGULATOR VALVE CURRENT | ERROR: REQUEST F8 57 58";
                                    break;
                                case 0x59:
                                    DescriptionToInsert = "DEFECT STATUS";

                                    switch (payload[1])
                                    {
                                        case 0x00:
                                            ValueToInsert = "OK";
                                            break;
                                        case 0x01:
                                            ValueToInsert = "CURRENT HIGH";
                                            break;
                                        case 0x02:
                                            ValueToInsert = "CURRENT LOW";
                                            break;
                                        default:
                                            break;
                                    }
                                    break;
                                case 0x5A:
                                    DescriptionToInsert = "FUEL PRESSURE STATUS";

                                    switch (payload[1])
                                    {
                                        case 0x00:
                                            ValueToInsert = "OK";
                                            break;
                                        case 0x01:
                                            ValueToInsert = "TOO HIGH";
                                            break;
                                        case 0x02:
                                            ValueToInsert = "LIMIT";
                                            break;
                                        case 0x04:
                                            ValueToInsert = "TOO LOW";
                                            break;
                                        case 0x08:
                                            ValueToInsert = "NEG DEV";
                                            break;
                                        case 0x10:
                                            ValueToInsert = "POS DEV";
                                            break;
                                        case 0x20:
                                            ValueToInsert = "LK MON";
                                            break;
                                        case 0x40:
                                            ValueToInsert = "LK IDLE";
                                            break;
                                        default:
                                            break;
                                    }
                                    break;
                                case 0x5B:
                                    DescriptionToInsert = "FUEL PRESSURE VOLTS";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST F8 5B 5C";
                                        break;
                                    }

                                    double FuelPressureVoltsB = ((payload[1] << 8) + payload[3]) * 0.0049;

                                    ValueToInsert = Math.Round(FuelPressureVoltsB, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0x5C:
                                    DescriptionToInsert = "FUEL PRESSURE VOLTS | ERROR: REQUEST F8 5B 5C";
                                    break;
                                case 0x5F:
                                    DescriptionToInsert = "FUEL LEVEL PERCENT";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST F8 5F 60";
                                        break;
                                    }

                                    double FuelLevelPercentB = ((payload[1] << 8) + payload[3]) * 0.3921568627;

                                    ValueToInsert = Math.Round(FuelLevelPercentB, 1).ToString("0.0").Replace(",", ".");
                                    UnitToInsert = "PERCENT";
                                    break;
                                case 0x60:
                                    DescriptionToInsert = "FUEL LEVEL PERCENT | ERROR: REQUEST F8 5F 60";
                                    break;
                                case 0x63:
                                    DescriptionToInsert = "FREEZE FRM DTC 2:";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST F8 63 64";
                                        break;
                                    }

                                    if ((payload[1] == 0x00) && (payload[3] == 0x00))
                                    {
                                        DescriptionToInsert += "EMPTY SLOT";
                                        break;
                                    }

                                    ValueToInsert = "OBD2 P" + Util.ByteToHexString(new byte[2] { payload[1], payload[3] }, 0, 2).Replace(" ", "");
                                    break;
                                case 0x64:
                                    DescriptionToInsert = "FREEZE FRAME DTC 2: | ERROR: REQUEST F8 63 64";
                                    break;
                                default:
                                    for (int i = 0; i < HSBPNum; i++)
                                    {
                                        HSOffset.Add(payload[i * 2]);
                                        HSValues.Add(payload[(i * 2) + 1]);
                                    }

                                    DescriptionToInsert = "F8 RAM TABLE | OFFSET: " + Util.ByteToHexStringSimple(HSOffset.ToArray());
                                    ValueToInsert = Util.ByteToHexStringSimple(HSValues.ToArray());
                                    break;
                            }
                        }
                        else
                        {
                            switch (payload[0])
                            {
                                default:
                                    for (int i = 0; i < HSBPNum; i++)
                                    {
                                        HSOffset.Add(payload[i * 2]);
                                        HSValues.Add(payload[(i * 2) + 1]);
                                    }

                                    DescriptionToInsert = "F8 RAM TABLE | OFFSET: " + Util.ByteToHexStringSimple(HSOffset.ToArray());
                                    ValueToInsert = Util.ByteToHexStringSimple(HSValues.ToArray());
                                    break;
                            }
                        }
                        break;
                    case 0xF9:
                        DescriptionToInsert = "F9 RAM TABLE SELECTED";

                        if (message.Length < 3) break;

                        switch (payload[0])
                        {
                            default:
                                for (int i = 0; i < HSBPNum; i++)
                                {
                                    HSOffset.Add(payload[i * 2]);
                                    HSValues.Add(payload[(i * 2) + 1]);
                                }

                                DescriptionToInsert = "F9 RAM TABLE | OFFSET: " + Util.ByteToHexStringSimple(HSOffset.ToArray());
                                ValueToInsert = Util.ByteToHexStringSimple(HSValues.ToArray());
                                break;
                        }
                        break;
                    case 0xFA:
                        DescriptionToInsert = "FA RAM TABLE SELECTED";

                        if (message.Length < 3) break;

                        switch (payload[0])
                        {
                            default:
                                for (int i = 0; i < HSBPNum; i++)
                                {
                                    HSOffset.Add(payload[i * 2]);
                                    HSValues.Add(payload[(i * 2) + 1]);
                                }

                                DescriptionToInsert = "FA RAM TABLE | OFFSET: " + Util.ByteToHexStringSimple(HSOffset.ToArray());
                                ValueToInsert = Util.ByteToHexStringSimple(HSValues.ToArray());
                                break;
                        }
                        break;
                    case 0xFB: // CUMMINS SENSORS
                        DescriptionToInsert = "FB RAM TABLE SELECTED";

                        if (message.Length < 3) break;

                        switch (payload[0])
                        {
                            case 0x01:
                                DescriptionToInsert = "ENGINE SPEED";

                                if (message.Length < 5) break;

                                if (payload[2] != (payload[0] + 1))
                                {
                                    DescriptionToInsert += " | ERROR: REQUEST FB 01 02";
                                    break;
                                }

                                double EngineSpeed = ((payload[1] << 8) + payload[3]) * 0.125;

                                ValueToInsert = Math.Round(EngineSpeed, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "RPM";
                                break;
                            case 0x02:
                                DescriptionToInsert = "ENGINE SPEED | ERROR: REQUEST FB 01 02";
                                break;
                            case 0x03:
                                DescriptionToInsert = "TRANSMISSION TEMPERATURE";

                                if (message.Length < 5) break;

                                if (payload[2] != (payload[0] + 1))
                                {
                                    DescriptionToInsert += " | ERROR: REQUEST FB 03 04";
                                    break;
                                }

                                double TransTempF = ((payload[1] << 8) + payload[3]) * 0.015625;
                                double TransTempC = (TransTempF - 32.0) / 1.8;

                                if (Properties.Settings.Default.Units == "imperial")
                                {
                                    ValueToInsert = Math.Round(TransTempF, 1).ToString("0.0").Replace(",", ".");
                                    UnitToInsert = "°F";
                                }
                                else if (Properties.Settings.Default.Units == "metric")
                                {
                                    ValueToInsert = Math.Round(TransTempC, 1).ToString("0.0").Replace(",", ".");
                                    UnitToInsert = "°C";
                                }
                                break;
                            case 0x04:
                                DescriptionToInsert = "TRANSMISSION TEMPERATURE | ERROR: REQUEST FB 03 04";
                                break;
                            case 0x05:
                                DescriptionToInsert = "VEHICLE SPEED";

                                if (message.Length < 5) break;

                                if (payload[2] != (payload[0] + 1))
                                {
                                    DescriptionToInsert += " | ERROR: REQUEST FB 05 06";
                                    break;
                                }

                                double VehicleSpeedMPH = ((payload[1] << 8) + payload[3]) * 0.00390625;
                                double VehicleSpeedKMH = VehicleSpeedMPH * 1.609344;

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
                                break;
                            case 0x06:
                                DescriptionToInsert = "VEHICLE SPEED | ERROR: REQUEST FB 05 06";
                                break;
                            case 0x07:
                                DescriptionToInsert = "APP SENSOR PERCENT";

                                if (message.Length < 5) break;

                                if (payload[2] != (payload[0] + 1))
                                {
                                    DescriptionToInsert += " | ERROR: REQUEST FB 07 08";
                                    break;
                                }

                                double APPSPercent = ((payload[1] << 8) + payload[3]) * 0.25;

                                ValueToInsert = Math.Round(APPSPercent, 1).ToString("0.0").Replace(",", ".");
                                UnitToInsert = "PERCENT";
                                break;
                            case 0x08:
                                DescriptionToInsert = "APP SENSOR PERCENT | ERROR: REQUEST FB 07 08";
                                break;
                            case 0x0B:
                                DescriptionToInsert = "TRANSMISSION TEMPERATURE SENSOR VOLTS";

                                if (message.Length < 5) break;

                                if (payload[2] != (payload[0] + 1))
                                {
                                    DescriptionToInsert += " | ERROR: REQUEST FB 0B 0C";
                                    break;
                                }

                                double TransTempSensorVolts = ((payload[1] << 8) + payload[3]) * 0.0049;

                                ValueToInsert = Math.Round(TransTempSensorVolts, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "V";
                                break;
                            case 0x0C:
                                DescriptionToInsert = "TRANSMISSION TEMP SENSOR VOLTS | ERROR: REQUEST FB 0B 0C";
                                break;
                            case 0x0F:
                                DescriptionToInsert = "ENGINE COOLANT TEMPERATURE";

                                if (message.Length < 5) break;

                                if (payload[2] != (payload[0] + 1))
                                {
                                    DescriptionToInsert += " | ERROR: REQUEST FB 0F 10";
                                    break;
                                }

                                double ECTF = ((payload[1] << 8) + payload[3]) * 0.015625;
                                double ECTC = (ECTF - 32.0) / 1.8;

                                if (Properties.Settings.Default.Units == "imperial")
                                {
                                    ValueToInsert = Math.Round(ECTF, 1).ToString("0.0").Replace(",", ".");
                                    UnitToInsert = "°F";
                                }
                                else if (Properties.Settings.Default.Units == "metric")
                                {
                                    ValueToInsert = Math.Round(ECTC, 1).ToString("0.0").Replace(",", ".");
                                    UnitToInsert = "°C";
                                }
                                break;
                            case 0x10:
                                DescriptionToInsert = "ENGINE COOLANT TEMPERATURE | ERROR: REQUEST FB 0F 10";
                                break;
                            case 0x11:
                                DescriptionToInsert = "ENGINE COOLANT TEMPERATURE SENSOR VOLTS";

                                if (message.Length < 5) break;

                                if (payload[2] != (payload[0] + 1))
                                {
                                    DescriptionToInsert += " | ERROR: REQUEST FB 11 12";
                                    break;
                                }

                                double ECTVolts = ((payload[1] << 8) + payload[3]) * 0.0049;

                                ValueToInsert = Math.Round(ECTVolts, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "V";
                                break;
                            case 0x12:
                                DescriptionToInsert = "ECT SENSOR VOLTS | ERROR: REQUEST FB 11 12";
                                break;
                            case 0x13:
                                DescriptionToInsert = "BOOST PRESSURE";

                                if (message.Length < 5) break;

                                if (payload[2] != (payload[0] + 1))
                                {
                                    DescriptionToInsert += " | ERROR: REQUEST FB 13 14";
                                    break;
                                }

                                double BoostPressureAPSI = ((payload[1] << 8) + payload[3]) * 0.0078125;
                                double BoostPressureAKPA = BoostPressureAPSI * 6.894757;

                                if (Properties.Settings.Default.Units == "imperial")
                                {
                                    ValueToInsert = Math.Round(BoostPressureAPSI, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "PSI";
                                }
                                else if (Properties.Settings.Default.Units == "metric")
                                {
                                    ValueToInsert = Math.Round(BoostPressureAKPA, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "KPA";
                                }
                                break;
                            case 0x14:
                                DescriptionToInsert = "BOOST PRESSURE | ERROR: REQUEST FB 13 14";
                                break;
                            case 0x15:
                                DescriptionToInsert = "INTAKE AIR TEMPERATURE";

                                if (message.Length < 5) break;

                                if (payload[2] != (payload[0] + 1))
                                {
                                    DescriptionToInsert += " | ERROR: REQUEST FB 15 16";
                                    break;
                                }

                                double IATF = ((payload[1] << 8) + payload[3]) * 0.015625;
                                double IATC = (IATF - 32.0) / 1.8;

                                if (Properties.Settings.Default.Units == "imperial")
                                {
                                    ValueToInsert = Math.Round(IATF, 1).ToString("0.0").Replace(",", ".");
                                    UnitToInsert = "°F";
                                }
                                else if (Properties.Settings.Default.Units == "metric")
                                {
                                    ValueToInsert = Math.Round(IATC, 1).ToString("0.0").Replace(",", ".");
                                    UnitToInsert = "°C";
                                }
                                break;
                            case 0x16:
                                DescriptionToInsert = "INTAKE AIR TEMPERATURE | ERROR: REQUEST FB 15 16";
                                break;
                            case 0x17:
                                DescriptionToInsert = "INTAKE AIR TEMPERATURE SENSOR VOLTS";

                                if (message.Length < 5) break;

                                if (payload[2] != (payload[0] + 1))
                                {
                                    DescriptionToInsert += " | ERROR: REQUEST FB 17 18";
                                    break;
                                }

                                double IATVolts = ((payload[1] << 8) + payload[3]) * 0.0049;

                                ValueToInsert = Math.Round(IATVolts, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "V";
                                break;
                            case 0x18:
                                DescriptionToInsert = "IAT SENSOR VOLTS | ERROR: REQUEST FB 17 18";
                                break;
                            case 0x19:
                                DescriptionToInsert = "BATTERY VOLTAGE";

                                if (message.Length < 5) break;

                                if (payload[2] != (payload[0] + 1))
                                {
                                    DescriptionToInsert += " | ERROR: REQUEST FB 19 1A";
                                    break;
                                }

                                double BatteryVolts = ((payload[1] << 8) + payload[3]) * 0.0625;

                                ValueToInsert = Math.Round(BatteryVolts, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "V";
                                break;
                            case 0x1A:
                                DescriptionToInsert = "BATTERY VOLTAGE | ERROR: REQUEST FB 19 1A";
                                break;
                        }

                        if ((Year < 2003) && (ControllerHardwareType == 9)) // CUMMINS
                        {
                            switch (payload[1])
                            {
                                case 0x1B:
                                    DescriptionToInsert = "INJECTOR PUMP BATTERY VOLTAGE";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST FB 1B 1C";
                                        break;
                                    }

                                    double InjPumpBatteryVolts = ((payload[1] << 8) + payload[3]) * 0.0183;

                                    ValueToInsert = Math.Round(InjPumpBatteryVolts, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0x1C:
                                    DescriptionToInsert = "INJECTOR PUMP BATTERY VOLTAGE | ERROR: REQUEST FB 1B 1C";
                                    break;
                                case 0x1F:
                                    DescriptionToInsert = "INJECTOR PUMP FUEL TEMPERATURE";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST FB 1F 20";
                                        break;
                                    }

                                    double InjPumpFuelTempF = ((payload[1] << 8) + payload[3]) * 0.0625;
                                    double InjPumpFuelTempC = (InjPumpFuelTempF - 32.0) / 1.8;

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        ValueToInsert = Math.Round(InjPumpFuelTempF, 1).ToString("0.0").Replace(",", ".");
                                        UnitToInsert = "°F";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        ValueToInsert = Math.Round(InjPumpFuelTempC, 1).ToString("0.0").Replace(",", ".");
                                        UnitToInsert = "°C";
                                    }
                                    break;
                                case 0x20:
                                    DescriptionToInsert = "INJECTOR PUMP FUEL TEMPERATURE | ERROR: REQUEST FB 1F 20";
                                    break;
                                case 0x47:
                                    DescriptionToInsert = "SWITCH STATUS";

                                    List<string> SwitchListA = new List<string>();

                                    if (Util.IsBitSet(payload[1], 7)) SwitchListA.Add("OPSCLSD"); // 1=oil pressure switch closed, 0=open
                                    else SwitchListA.Add("OPSOPEN");
                                    if (Util.IsBitSet(payload[1], 6)) SwitchListA.Add("-6-");
                                    if (Util.IsBitSet(payload[1], 5)) SwitchListA.Add("ODRLSD"); // 1=overdrive switch released, 0=pressed
                                    else SwitchListA.Add("ODPRSD");
                                    if (Util.IsBitSet(payload[1], 4)) SwitchListA.Add("P/N"); // 1=P/N, 0=D/R
                                    else SwitchListA.Add("D/R");
                                    if (Util.IsBitSet(payload[1], 3)) SwitchListA.Add("-3-");
                                    if (Util.IsBitSet(payload[1], 2)) SwitchListA.Add("-2-");
                                    if (Util.IsBitSet(payload[1], 1)) SwitchListA.Add("-1-");
                                    if (Util.IsBitSet(payload[1], 0)) SwitchListA.Add("0-0");

                                    if (SwitchListA.Count == 0) break;

                                    foreach (string s in SwitchListA)
                                    {
                                        ValueToInsert += s + " | ";
                                    }

                                    if (ValueToInsert.Length > 2) ValueToInsert = ValueToInsert.Remove(ValueToInsert.Length - 3); // remove last "|" character
                                    break;
                                case 0x4A:
                                    DescriptionToInsert = "FINAL FUEL STATE";

                                    switch (payload[1])
                                    {
                                        case 0x00:
                                            ValueToInsert = "NOT SET";
                                            break;
                                        case 0x01:
                                            ValueToInsert = "JCOM TORQUE";
                                            break;
                                        case 0x02:
                                            ValueToInsert = "JCOM SPEED";
                                            break;
                                        case 0x03:
                                            ValueToInsert = "PROGRSV SHIFT";
                                            break;
                                        case 0x04:
                                            ValueToInsert = "PTO";
                                            break;
                                        case 0x05:
                                            ValueToInsert = "USER COMMAND";
                                            break;
                                        case 0x06:
                                            ValueToInsert = "LIMP HOME";
                                            break;
                                        case 0x07:
                                            ValueToInsert = "ASG THROTTLE";
                                            break;
                                        case 0x08:
                                            ValueToInsert = "4-D FUELING";
                                            break;
                                        case 0x09:
                                            ValueToInsert = "CRUISE CONTROL";
                                            break;
                                        case 0x0A:
                                            ValueToInsert = "ROAD SPEED GOV";
                                            break;
                                        case 0x0B:
                                            ValueToInsert = "LOW SPEED GOV";
                                            break;
                                        case 0x0C:
                                            ValueToInsert = "HIGH SPEED GOV";
                                            break;
                                        case 0x0D:
                                            ValueToInsert = "TORQUE DERATE OVERRIDE";
                                            break;
                                        case 0x0E:
                                            ValueToInsert = "LOW GEAR";
                                            break;
                                        case 0x0F:
                                            ValueToInsert = "ALTITUDE DERATE";
                                            break;
                                        case 0x10:
                                            ValueToInsert = "AFC DERATE";
                                            break;
                                        case 0x11:
                                            ValueToInsert = "ANC DERATE";
                                            break;
                                        case 0x12:
                                            ValueToInsert = "ENGINE PROTECT";
                                            break;
                                        case 0x13:
                                            ValueToInsert = "TORQUE CRV LIMIT";
                                            break;
                                        case 0x14:
                                            ValueToInsert = "JCOM TORQUE DERATE";
                                            break;
                                        case 0x15:
                                            ValueToInsert = "OUT OF GEAR";
                                            break;
                                        case 0x16:
                                            ValueToInsert = "CRANKING";
                                            break;
                                        case 0x17:
                                            ValueToInsert = "USER OVERRIDE";
                                            break;
                                        case 0x18:
                                            ValueToInsert = "ENGINE BRAKE";
                                            break;
                                        case 0x19:
                                            ValueToInsert = "ENGINE OVERSPEED";
                                            break;
                                        case 0x1A:
                                            ValueToInsert = "ENGINE STOPPED";
                                            break;
                                        case 0x1B:
                                            ValueToInsert = "SHUTDOWN";
                                            break;
                                        case 0x1C:
                                            ValueToInsert = "FUEL DTC DERATE";
                                            break;
                                        case 0x1D:
                                            ValueToInsert = "ENGINE PROTECT";
                                            break;
                                        case 0x1E:
                                            ValueToInsert = "ALL SPD GOV APP";
                                            break;
                                        case 0x1F:
                                            ValueToInsert = "ALT TORQUE";
                                            break;
                                        case 0x20:
                                            ValueToInsert = "MASTER/SLAVE OVERRIDE";
                                            break;
                                        case 0x21:
                                            ValueToInsert = "STARTUP OIL LIMIT";
                                            break;
                                        case 0x22:
                                            ValueToInsert = "PTO DERATE";
                                            break;
                                        case 0x23:
                                            ValueToInsert = "TORQUE CONTROL";
                                            break;
                                        case 0x24:
                                            ValueToInsert = "POWERTRAIN PROTECT";
                                            break;
                                        case 0x25:
                                            ValueToInsert = "T2 SPEED";
                                            break;
                                        case 0x26:
                                            ValueToInsert = "T2 TORQUE DERATE";
                                            break;
                                        case 0x27:
                                            ValueToInsert = "T2 DERATE";
                                            break;
                                        case 0x28:
                                            ValueToInsert = "NO DERATE";
                                            break;
                                        case 0x29:
                                            ValueToInsert = "ANTI THEFT DERATE";
                                            break;
                                        case 0x2A:
                                            ValueToInsert = "PART THROTTLE LIMIT";
                                            break;
                                        case 0x2B:
                                            ValueToInsert = "STEADY-STATE AMB DERATE";
                                            break;
                                        case 0x2C:
                                            ValueToInsert = "TRSNT COOLANT DERATE";
                                            break;
                                    }
                                    break;
                                case 0x51:
                                    DescriptionToInsert = "BOOST VOLTS";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST FB 51 52";
                                        break;
                                    }

                                    double BoostVolts = ((payload[1] << 8) + payload[3]) * 0.0049;

                                    ValueToInsert = Math.Round(BoostVolts, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0x52:
                                    DescriptionToInsert = "BOOST VOLTS | ERROR: REQUEST FB 51 52";
                                    break;
                                case 0x55:
                                    DescriptionToInsert = "WATER IN FUEL VOLTS";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST FB 55 56";
                                        break;
                                    }

                                    double WIFVoltsB = ((payload[1] << 8) + payload[3]) * 0.0049;

                                    ValueToInsert = Math.Round(WIFVoltsB, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0x56:
                                    DescriptionToInsert = "WATER IN FUEL VOLTS | ERROR: REQUEST FB 55 56";
                                    break;
                                case 0x57:
                                    DescriptionToInsert = "ENGINE LOAD";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST FB 57 58";
                                        break;
                                    }

                                    double EngineLoadA = ((payload[1] << 8) + payload[3]) * 0.00390625;

                                    ValueToInsert = Math.Round(EngineLoadA, 1).ToString("0.0").Replace(",", ".");
                                    UnitToInsert = "PERCENT";
                                    break;
                                case 0x58:
                                    DescriptionToInsert = "ENGINE LOAD | ERROR: REQUEST FB 57 58";
                                    break;
                                case 0xB9:
                                    DescriptionToInsert = "BATTERY TEMPERATURE";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST FB B9 BA";
                                        break;
                                    }

                                    double BatteryTempteratureF = ((payload[1] << 8) + payload[3]) * 0.0048;
                                    double BatteryTemperatureC = (BatteryTempteratureF - 32.0) / 1.8;

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        ValueToInsert = Math.Round(BatteryTempteratureF, 1).ToString("0.0").Replace(",", ".");
                                        UnitToInsert = "°F";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        ValueToInsert = Math.Round(BatteryTemperatureC, 1).ToString("0.0").Replace(",", ".");
                                        UnitToInsert = "°C";
                                    }
                                    break;
                                case 0xBA:
                                    DescriptionToInsert = "BATTERY TEMPERATURE | ERROR: REQUEST FB B9 BA";
                                    break;
                                case 0xCB:
                                    DescriptionToInsert = "KEY-ON COUNTER";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST FB CB CC";
                                        break;
                                    }

                                    ushort KeyOnCounter = (ushort)((payload[1] << 8) + payload[3]);

                                    ValueToInsert = KeyOnCounter.ToString("0");
                                    UnitToInsert = "COUNTS";
                                    break;
                                case 0xCC:
                                    DescriptionToInsert = "KEY-ON COUNTER | ERROR: REQUEST FB CB CC";
                                    break;
                                case 0xCD:
                                    DescriptionToInsert = "ENGINE SPEED CKD SENSOR";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST FB CD CE";
                                        break;
                                    }

                                    double EngineSpeedCKD = ((payload[1] << 8) + payload[3]) * 0.125;

                                    ValueToInsert = Math.Round(EngineSpeedCKD, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "RPM";
                                    break;
                                case 0xCE:
                                    DescriptionToInsert = "ENGINE SPEED CKD SENSOR | ERROR: REQUEST FB CF D0";
                                    break;
                                case 0xCF:
                                    DescriptionToInsert = "ENGINE SPEED CMP SENSOR";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST FB CD CE";
                                        break;
                                    }

                                    double EngineSpeedCMP = ((payload[1] << 8) + payload[3]) * 0.125;

                                    ValueToInsert = Math.Round(EngineSpeedCMP, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "RPM";
                                    break;
                                case 0xD0:
                                    DescriptionToInsert = "ENGINE SPEED CMP SENSOR | ERROR: REQUEST FB CF D0";
                                    break;
                                case 0xD7:
                                    DescriptionToInsert = "APP SENSOR VOLTS";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST FB D7 D8";
                                        break;
                                    }

                                    double APPSVolts = ((payload[1] << 8) + payload[3]) * 0.0049;

                                    ValueToInsert = Math.Round(APPSVolts, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0xD8:
                                    DescriptionToInsert = "APP SENSOR VOLTS | ERROR: REQUEST FB D7 D8";
                                    break;
                                case 0xDC:
                                    DescriptionToInsert = "CRUISE CONTROL | DENIED REASON";

                                    switch (payload[1])
                                    {
                                        case 0x02:
                                            ValueToInsert = "CRUISE SWITCH DTC";
                                            break;
                                        case 0x03:
                                            ValueToInsert = "VSS RATIONALITY";
                                            break;
                                        case 0x04:
                                            ValueToInsert = "BRAKE RATIONALITY";
                                            break;
                                        case 0x05:
                                            ValueToInsert = "ON/OFF SWITCH";
                                            break;
                                        case 0x06:
                                            ValueToInsert = "BRAKE SWITCH";
                                            break;
                                        case 0x07:
                                            ValueToInsert = "CANCEL SWITCH";
                                            break;
                                        case 0x08:
                                            ValueToInsert = "SPEED SENSOR";
                                            break;
                                        case 0x09:
                                            ValueToInsert = "RPM LIMIT";
                                            break;
                                        case 0x0A:
                                            ValueToInsert = "RPM/VSS RATIO";
                                            break;
                                        case 0x0B:
                                            ValueToInsert = "CLUTCH SWITCH";
                                            break;
                                        case 0x0C:
                                            ValueToInsert = "P/N SWITCH";
                                            break;
                                        default:
                                            ValueToInsert = "N/A";
                                            break;
                                    }
                                    break;
                                case 0xDE:
                                    DescriptionToInsert = "CRUISE CONTROL | LAST CUTOUT REASON";

                                    switch (payload[1])
                                    {
                                        case 0x02:
                                            ValueToInsert = "CRUISE SWITCH DTC";
                                            break;
                                        case 0x03:
                                            ValueToInsert = "VSS RATIONALITY";
                                            break;
                                        case 0x04:
                                            ValueToInsert = "BRAKE RATIONALITY";
                                            break;
                                        case 0x05:
                                            ValueToInsert = "ON/OFF SWITCH";
                                            break;
                                        case 0x06:
                                            ValueToInsert = "BRAKE SWITCH";
                                            break;
                                        case 0x07:
                                            ValueToInsert = "CANCEL SWITCH";
                                            break;
                                        case 0x08:
                                            ValueToInsert = "SPEED SENSOR";
                                            break;
                                        case 0x09:
                                            ValueToInsert = "RPM LIMIT";
                                            break;
                                        case 0x0A:
                                            ValueToInsert = "RPM/VSS RATIO";
                                            break;
                                        case 0x0B:
                                            ValueToInsert = "CLUTCH SWITCH";
                                            break;
                                        case 0x0C:
                                            ValueToInsert = "P/N SWITCH";
                                            break;
                                        default:
                                            ValueToInsert = "N/A";
                                            break;
                                    }
                                    break;
                                case 0xE2:
                                    DescriptionToInsert = "CRUISE INDICATOR LAMP";

                                    if (Util.IsBitSet(payload[1], 0)) ValueToInsert = "ON";
                                    else ValueToInsert = "OFF";

                                    break;
                                case 0xE4:
                                    DescriptionToInsert = "CRUISE | BUTTON PRESSED";

                                    List<string> SwitchListB = new List<string>();

                                    if (payload[1] == 0) SwitchListB.Add("ON/OFF");
                                    if (Util.IsBitSet(payload[1], 1) && Util.IsBitSet(payload[1], 0)) SwitchListB.Add("SET");

                                    if (Util.IsBitSet(payload[1], 7)) SwitchListB.Add("-7-");
                                    if (Util.IsBitSet(payload[1], 6)) SwitchListB.Add("-6-");
                                    if (Util.IsBitSet(payload[1], 5)) SwitchListB.Add("-5-");
                                    if (Util.IsBitSet(payload[1], 4)) SwitchListB.Add("-4-");
                                    if (Util.IsBitSet(payload[1], 3)) SwitchListB.Add("-3-");
                                    if (Util.IsBitSet(payload[1], 2)) SwitchListB.Add("ACC/RES");
                                    if (Util.IsBitSet(payload[1], 1)) SwitchListB.Add("COAST");
                                    if (Util.IsBitSet(payload[1], 0)) SwitchListB.Add("CANCEL");

                                    if (SwitchListB.Count == 0) break;

                                    foreach (string s in SwitchListB)
                                    {
                                        ValueToInsert += s + " | ";
                                    }

                                    if (ValueToInsert.Length > 2) ValueToInsert = ValueToInsert.Remove(ValueToInsert.Length - 3); // remove last "|" character
                                    break;
                                case 0xE5:
                                    DescriptionToInsert = "CRUISE SET SPEED";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST FB E5 E6";
                                        break;
                                    }

                                    double CruiseSetSpeedMPH = ((payload[1] << 8) + payload[3]) * 0.00390625;
                                    double CruiseSetSpeedKMH = CruiseSetSpeedMPH * 1.609344;

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        ValueToInsert = Math.Round(CruiseSetSpeedMPH, 3).ToString("0.000").Replace(",", ".");
                                        UnitToInsert = "MPH";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        ValueToInsert = Math.Round(CruiseSetSpeedKMH, 3).ToString("0.000").Replace(",", ".");
                                        UnitToInsert = "KM/H";
                                    }
                                    break;
                                case 0xE6:
                                    DescriptionToInsert = "CRUISE SET SPEED | ERROR: REQUEST FB E5 E6";
                                    break;
                                case 0xE7:
                                    DescriptionToInsert = "CRUISE SWITCH VOLTS";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST FB E7 E8";
                                        break;
                                    }

                                    double CruiseSwitchVolts = ((payload[1] << 8) + payload[3]) * 0.0049;

                                    ValueToInsert = Math.Round(CruiseSwitchVolts, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0xE8:
                                    DescriptionToInsert = "CRUISE SWITCH VOLTS | ERROR: REQUEST FB E7 E8";
                                    break;
                                default:
                                    for (int i = 0; i < HSBPNum; i++)
                                    {
                                        HSOffset.Add(payload[i * 2]);
                                        HSValues.Add(payload[(i * 2) + 1]);
                                    }

                                    DescriptionToInsert = "FB RAM TABLE | OFFSET: " + Util.ByteToHexStringSimple(HSOffset.ToArray());
                                    ValueToInsert = Util.ByteToHexStringSimple(HSValues.ToArray());
                                    break;
                            }
                        }
                        else if ((Year >= 2003) && (ControllerHardwareType == 9)) // CUMMINS
                        {
                            switch (payload[1])
                            {
                                case 0x1B:
                                    DescriptionToInsert = "OUTPUT SHAFT SPEED";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST FB 1B 1C";
                                        break;
                                    }

                                    double OutputShaftSpeed = ((payload[1] << 8) + payload[3]) * 0.125;

                                    ValueToInsert = Math.Round(OutputShaftSpeed, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "RPM";
                                    break;
                                case 0x1C:
                                    DescriptionToInsert = "OUTPUT SHAFT SPEED | ERROR: REQUEST FB 1B 1C";
                                    break;
                                case 0x1D:
                                    DescriptionToInsert = "WATER IN FUEL";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST FB 1D 1E";
                                        break;
                                    }

                                    double WIFCounts = ((payload[1] << 8) + payload[3]);

                                    ValueToInsert = WIFCounts.ToString("0");
                                    UnitToInsert = "COUNTS";
                                    break;
                                case 0x1E:
                                    DescriptionToInsert = "WATER IN FUEL | ERROR: REQUEST FB 1D 1E";
                                    break;
                                case 0x1F:
                                    DescriptionToInsert = "TRANSMISSION PWM DUTY CYCLE";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST FB 1F 20";
                                        break;
                                    }

                                    double TransPWMDutyCycle = ((payload[1] << 8) + payload[3]) * 0.00390625;

                                    ValueToInsert = Math.Round(TransPWMDutyCycle, 1).ToString("0.0").Replace(",", ".");
                                    UnitToInsert = "PERCENT";
                                    break;
                                case 0x20:
                                    DescriptionToInsert = "TRANSMISSION PWM DUTY CYCLE | ERROR: REQUEST FB 1F 20";
                                    break;
                                case 0x23:
                                    DescriptionToInsert = "PRESENT DRIVE GEAR";

                                    switch (payload[1])
                                    {
                                        case 0x00:
                                            ValueToInsert = "NEUTRAL";
                                            break;
                                        case 0x01:
                                            ValueToInsert = "1ST";
                                            break;
                                        case 0x02:
                                            ValueToInsert = "2ND";
                                            break;
                                        case 0x03:
                                            ValueToInsert = "3RD";
                                            break;
                                        case 0x04:
                                            ValueToInsert = "4TH";
                                            break;
                                        case 0x05:
                                            ValueToInsert = "5TH";
                                            break;
                                        case 0x06:
                                            ValueToInsert = "6TH";
                                            break;
                                        case 0x10:
                                            ValueToInsert = "REVERSE";
                                            break;
                                        default:
                                            break;
                                    }
                                    break;
                                case 0x2F:
                                    DescriptionToInsert = "TARGET GOVERNOR PRESSURE";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST FB 2F 30";
                                        break;
                                    }

                                    double TargetGovPressurePSI = ((payload[1] << 8) + payload[3]) * 0.0078125;
                                    double TargetGovPressureKPA = TargetGovPressurePSI * 6.894757;

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        ValueToInsert = Math.Round(TargetGovPressurePSI, 3).ToString("0.000").Replace(",", ".");
                                        UnitToInsert = "PSI";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        ValueToInsert = Math.Round(TargetGovPressureKPA, 3).ToString("0.000").Replace(",", ".");
                                        UnitToInsert = "KPA";
                                    }
                                    break;
                                case 0x30:
                                    DescriptionToInsert = "TARGET GOVERNOR PRESSURE | ERROR: REQUEST FB 2F 30";
                                    break;
                                case 0x31:
                                    DescriptionToInsert = "PPS 1 SENSOR PERCENT";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST FB 31 32";
                                        break;
                                    }

                                    double PPS1SensorPercent = ((payload[1] << 8) + payload[3]) * 0.25;

                                    ValueToInsert = Math.Round(PPS1SensorPercent, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "PERCENT";
                                    break;
                                case 0x32:
                                    DescriptionToInsert = "PPS 1 SENSOR PERCENT | ERROR: REQUEST FB 31 32";
                                    break;
                                case 0x33:
                                    DescriptionToInsert = "PPS 1 SENSOR VOLTS";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST FB 33 34";
                                        break;
                                    }

                                    double PPS1SensorVolts = ((payload[1] << 8) + payload[3]) * 0.0049;

                                    ValueToInsert = Math.Round(PPS1SensorVolts, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0x34:
                                    DescriptionToInsert = "PPS 1 SENSOR VOLTS | ERROR: REQUEST FB 33 34";
                                    break;
                                case 0x3F:
                                    DescriptionToInsert = "PPS 2 SENSOR PERCENT";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST FB 3F 40";
                                        break;
                                    }

                                    double PPS2SensorPercent = ((payload[1] << 8) + payload[3]) * 0.25;

                                    ValueToInsert = Math.Round(PPS2SensorPercent, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "PERCENT";
                                    break;
                                case 0x40:
                                    DescriptionToInsert = "PPS 2 SENSOR PERCENT | ERROR: REQUEST FB 3F 40";
                                    break;
                                case 0x41:
                                    DescriptionToInsert = "PPS 2 SENSOR VOLTS";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST FB 41 42";
                                        break;
                                    }

                                    double PPS2SensorVolts = ((payload[1] << 8) + payload[3]) * 0.0049;

                                    ValueToInsert = Math.Round(PPS2SensorVolts, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0x42:
                                    DescriptionToInsert = "PPS 2 SENSOR VOLTS | ERROR: REQUEST FB 41 42";
                                    break;
                                case 0x45:
                                    DescriptionToInsert = "IDLE SWITCH STATUS";

                                    if (Util.IsBitSet(payload[1], 1) && Util.IsBitSet(payload[1], 0)) ValueToInsert = "PRESSED";
                                    else ValueToInsert = "RELEASED";
                                    break;
                                case 0x46:
                                    DescriptionToInsert = "BRAKE SWITCH PRESSED";

                                    double BSPPercentage = payload[1] * 0.25;

                                    ValueToInsert = Math.Round(BSPPercentage, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "PERCENT";
                                    break;
                                case 0x47:
                                    DescriptionToInsert = "SWITCH STATUS";

                                    List<string> SwitchListA = new List<string>();

                                    if (Util.IsBitSet(payload[1], 7)) SwitchListA.Add("OPSCLSD"); // 1=oil pressure switch closed, 0=open
                                    else SwitchListA.Add("OPSOPEN");
                                    if (Util.IsBitSet(payload[1], 6)) SwitchListA.Add("-6-");
                                    if (Util.IsBitSet(payload[1], 5)) SwitchListA.Add("ODRLSD"); // 1=overdrive switch released, 0=pressed
                                    else SwitchListA.Add("ODPRSD");
                                    if (Util.IsBitSet(payload[1], 4)) SwitchListA.Add("P/N"); // 1=P/N, 0=D/R
                                    else SwitchListA.Add("D/R");
                                    if (Util.IsBitSet(payload[1], 3)) SwitchListA.Add("-3-");
                                    if (Util.IsBitSet(payload[1], 2)) SwitchListA.Add("-2-");
                                    if (Util.IsBitSet(payload[1], 1)) SwitchListA.Add("-1-");
                                    if (Util.IsBitSet(payload[1], 0)) SwitchListA.Add("-0-");

                                    if (SwitchListA.Count == 0) break;

                                    foreach (string s in SwitchListA)
                                    {
                                        ValueToInsert += s + " | ";
                                    }

                                    if (ValueToInsert.Length > 2) ValueToInsert = ValueToInsert.Remove(ValueToInsert.Length - 3); // remove last "|" character
                                    break;
                                case 0x48:
                                    DescriptionToInsert = "DESIRED TORQUE CONVERTER CLUTCH STATUS";

                                    if (Util.IsBitSet(payload[1], 0)) ValueToInsert = "LOCKED";
                                    else ValueToInsert = "UNLOCKED";

                                    break;
                                case 0x4A:
                                    DescriptionToInsert = "FINAL FUEL STATE";

                                    switch (payload[1])
                                    {
                                        case 0x00:
                                            ValueToInsert = "NOT SET";
                                            break;
                                        case 0x01:
                                            ValueToInsert = "JCOM TORQUE";
                                            break;
                                        case 0x02:
                                            ValueToInsert = "JCOM SPEED";
                                            break;
                                        case 0x03:
                                            ValueToInsert = "PROGRSV SHIFT";
                                            break;
                                        case 0x04:
                                            ValueToInsert = "PTO";
                                            break;
                                        case 0x05:
                                            ValueToInsert = "USER COMMAND";
                                            break;
                                        case 0x06:
                                            ValueToInsert = "LIMP HOME";
                                            break;
                                        case 0x07:
                                            ValueToInsert = "ASG THROTTLE";
                                            break;
                                        case 0x08:
                                            ValueToInsert = "4-D FUELING";
                                            break;
                                        case 0x09:
                                            ValueToInsert = "CRUISE CONTROL";
                                            break;
                                        case 0x0A:
                                            ValueToInsert = "ROAD SPEED GOV";
                                            break;
                                        case 0x0B:
                                            ValueToInsert = "LOW SPEED GOV";
                                            break;
                                        case 0x0C:
                                            ValueToInsert = "HIGH SPEED GOV";
                                            break;
                                        case 0x0D:
                                            ValueToInsert = "TORQUE DERATE OVERRIDE";
                                            break;
                                        case 0x0E:
                                            ValueToInsert = "LOW GEAR";
                                            break;
                                        case 0x0F:
                                            ValueToInsert = "ALTITUDE DERATE";
                                            break;
                                        case 0x10:
                                            ValueToInsert = "AFC DERATE";
                                            break;
                                        case 0x11:
                                            ValueToInsert = "ANC DERATE";
                                            break;
                                        case 0x12:
                                            ValueToInsert = "ENGINE PROTECT";
                                            break;
                                        case 0x13:
                                            ValueToInsert = "TORQUE CRV LIMIT";
                                            break;
                                        case 0x14:
                                            ValueToInsert = "JCOM TORQUE DERATE";
                                            break;
                                        case 0x15:
                                            ValueToInsert = "OUT OF GEAR";
                                            break;
                                        case 0x16:
                                            ValueToInsert = "CRANKING";
                                            break;
                                        case 0x17:
                                            ValueToInsert = "USER OVERRIDE";
                                            break;
                                        case 0x18:
                                            ValueToInsert = "ENGINE BRAKE";
                                            break;
                                        case 0x19:
                                            ValueToInsert = "ENGINE OVERSPEED";
                                            break;
                                        case 0x1A:
                                            ValueToInsert = "ENGINE STOPPED";
                                            break;
                                        case 0x1B:
                                            ValueToInsert = "SHUTDOWN";
                                            break;
                                        case 0x1C:
                                            ValueToInsert = "FUEL DTC DERATE";
                                            break;
                                        case 0x1D:
                                            ValueToInsert = "ENGINE PROTECT";
                                            break;
                                        case 0x1E:
                                            ValueToInsert = "ALL SPD GOV APP";
                                            break;
                                        case 0x1F:
                                            ValueToInsert = "ALT TORQUE";
                                            break;
                                        case 0x20:
                                            ValueToInsert = "MASTER/SLAVE OVERRIDE";
                                            break;
                                        case 0x21:
                                            ValueToInsert = "STARTUP OIL LIMIT";
                                            break;
                                        case 0x22:
                                            ValueToInsert = "PTO DERATE";
                                            break;
                                        case 0x23:
                                            ValueToInsert = "TORQUE CONTROL";
                                            break;
                                        case 0x24:
                                            ValueToInsert = "POWERTRAIN PROTECT";
                                            break;
                                        case 0x25:
                                            ValueToInsert = "T2 SPEED";
                                            break;
                                        case 0x26:
                                            ValueToInsert = "T2 TORQUE DERATE";
                                            break;
                                        case 0x27:
                                            ValueToInsert = "T2 DERATE";
                                            break;
                                        case 0x28:
                                            ValueToInsert = "NO DERATE";
                                            break;
                                        case 0x29:
                                            ValueToInsert = "ANTI THEFT DERATE";
                                            break;
                                        case 0x2A:
                                            ValueToInsert = "PART THROTTLE LIMIT";
                                            break;
                                        case 0x2B:
                                            ValueToInsert = "STEADY-STATE AMB DERATE";
                                            break;
                                        case 0x2C:
                                            ValueToInsert = "TRSNT COOLANT DERATE";
                                            break;
                                    }
                                    break;
                                case 0x4F:
                                    DescriptionToInsert = "WASTEGATE DUTY CYCLE";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST FB 4F 50";
                                        break;
                                    }

                                    double WasteGateDutyCycle = ((payload[1] << 8) + payload[3]) * 0.00390625;

                                    ValueToInsert = Math.Round(WasteGateDutyCycle, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "PERCENT";
                                    break;
                                case 0x50:
                                    DescriptionToInsert = "WASTEGATE DUTY CYCLE | ERROR: REQUEST FB 4F 50";
                                    break;
                                case 0x51:
                                    DescriptionToInsert = "BOOST VOLTS";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST FB 51 52";
                                        break;
                                    }

                                    double BoostVolts = ((payload[1] << 8) + payload[3]) * 0.0049;

                                    ValueToInsert = Math.Round(BoostVolts, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0x52:
                                    DescriptionToInsert = "BOOST VOLTS | ERROR: REQUEST FB 51 52";
                                    break;
                                case 0x55:
                                    DescriptionToInsert = "WATER IN FUEL VOLTS";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST FB 55 56";
                                        break;
                                    }

                                    double WIFVoltsB = ((payload[1] << 8) + payload[3]) * 0.0049;

                                    ValueToInsert = Math.Round(WIFVoltsB, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0x56:
                                    DescriptionToInsert = "WATER IN FUEL VOLTS | ERROR: REQUEST FB 55 56";
                                    break;
                                case 0x57:
                                    DescriptionToInsert = "ENGINE LOAD";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST FB 57 58";
                                        break;
                                    }

                                    double EngineLoadA = ((payload[1] << 8) + payload[3]) * 0.00390625;

                                    ValueToInsert = Math.Round(EngineLoadA, 1).ToString("0.0").Replace(",", ".");
                                    UnitToInsert = "PERCENT";
                                    break;
                                case 0x58:
                                    DescriptionToInsert = "ENGINE LOAD | ERROR: REQUEST FB 57 58";
                                    break;
                                case 0xB9:
                                    DescriptionToInsert = "BATTERY TEMPERATURE";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST FB B9 BA";
                                        break;
                                    }

                                    double BatteryTempteratureF = ((payload[1] << 8) + payload[3]) * 0.015625;
                                    double BatteryTemperatureC = (BatteryTempteratureF - 32.0) / 1.8;

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        ValueToInsert = Math.Round(BatteryTempteratureF, 1).ToString("0.0").Replace(",", ".");
                                        UnitToInsert = "°F";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        ValueToInsert = Math.Round(BatteryTemperatureC, 1).ToString("0.0").Replace(",", ".");
                                        UnitToInsert = "°C";
                                    }
                                    break;
                                case 0xBA:
                                    DescriptionToInsert = "BATTERY TEMPERATURE | ERROR: REQUEST FB B9 BA";
                                    break;
                                case 0xCB:
                                    DescriptionToInsert = "KEY-ON COUNTER";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST FB CB CC";
                                        break;
                                    }

                                    ushort KeyOnCounter = (ushort)((payload[1] << 8) + payload[3]);

                                    ValueToInsert = KeyOnCounter.ToString("0");
                                    UnitToInsert = "COUNTS";
                                    break;
                                case 0xCC:
                                    DescriptionToInsert = "KEY-ON COUNTER | ERROR: REQUEST FB CB CC";
                                    break;
                                case 0xCD:
                                    DescriptionToInsert = "ENGINE SPEED CKD SENSOR";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST FB CD CE";
                                        break;
                                    }

                                    double EngineSpeedCKD = ((payload[1] << 8) + payload[3]) * 0.125;

                                    ValueToInsert = Math.Round(EngineSpeedCKD, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "RPM";
                                    break;
                                case 0xCE:
                                    DescriptionToInsert = "ENGINE SPEED CKD SENSOR | ERROR: REQUEST FB CF D0";
                                    break;
                                case 0xCF:
                                    DescriptionToInsert = "ENGINE SPEED CMP SENSOR";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST FB CD CE";
                                        break;
                                    }

                                    double EngineSpeedCMP = ((payload[1] << 8) + payload[3]) * 0.125;

                                    ValueToInsert = Math.Round(EngineSpeedCMP, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "RPM";
                                    break;
                                case 0xD0:
                                    DescriptionToInsert = "ENGINE SPEED CMP SENSOR | ERROR: REQUEST FB CF D0";
                                    break;
                                case 0xD2:
                                    DescriptionToInsert = "RELAY STATUS";

                                    List<string> RelayList = new List<string>();

                                    if (Util.IsBitSet(payload[1], 7)) RelayList.Add("-7-");
                                    if (Util.IsBitSet(payload[1], 6)) RelayList.Add("-6-");
                                    if (Util.IsBitSet(payload[1], 5)) RelayList.Add("CRS12V");
                                    if (Util.IsBitSet(payload[1], 4)) RelayList.Add("CVACBLOCK"); // cruise vacuum solenoid block
                                    else RelayList.Add("CVACAPPLY"); // cruise vacuum solenoid apply
                                    if (Util.IsBitSet(payload[1], 3)) RelayList.Add("CVNTBLOCK"); // cruise vent solenoid block
                                    else RelayList.Add("CVNTBLEED"); // cruise vent solenoid bleed
                                    if (Util.IsBitSet(payload[1], 2)) RelayList.Add("-2-");
                                    if (Util.IsBitSet(payload[1], 1)) RelayList.Add("ODSOLON"); // overdrive solenoid on
                                    else RelayList.Add("ODSOLOFF"); // overdrive solenoid off
                                    if (Util.IsBitSet(payload[1], 0)) RelayList.Add("TCM12V");

                                    if (RelayList.Count == 0) break;

                                    DescriptionToInsert += ": ";

                                    foreach (string s in RelayList)
                                    {
                                        DescriptionToInsert += s + " | ";
                                    }

                                    if (DescriptionToInsert.Length > 2) DescriptionToInsert = DescriptionToInsert.Remove(DescriptionToInsert.Length - 3); // remove last "|" character
                                    break;
                                case 0xD7:
                                    DescriptionToInsert = "APP SENSOR VOLTS";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST FB D7 D8";
                                        break;
                                    }

                                    double APPSVolts = ((payload[1] << 8) + payload[3]) * 0.0049;

                                    ValueToInsert = Math.Round(APPSVolts, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0xD8:
                                    DescriptionToInsert = "APP SENSOR VOLTS | ERROR: REQUEST FB D7 D8";
                                    break;
                                case 0xDC:
                                    DescriptionToInsert = "CRUISE CONTROL | DENIED REASON";

                                    switch (payload[1])
                                    {
                                        case 0x02:
                                            ValueToInsert = "CRUISE SWITCH DTC";
                                            break;
                                        case 0x03:
                                            ValueToInsert = "VSS RATIONALITY";
                                            break;
                                        case 0x04:
                                            ValueToInsert = "BRAKE RATIONALITY";
                                            break;
                                        case 0x05:
                                            ValueToInsert = "ON/OFF SWITCH";
                                            break;
                                        case 0x06:
                                            ValueToInsert = "BRAKE SWITCH";
                                            break;
                                        case 0x07:
                                            ValueToInsert = "CANCEL SWITCH";
                                            break;
                                        case 0x08:
                                            ValueToInsert = "SPEED SENSOR";
                                            break;
                                        case 0x09:
                                            ValueToInsert = "RPM LIMIT";
                                            break;
                                        case 0x0A:
                                            ValueToInsert = "RPM/VSS RATIO";
                                            break;
                                        case 0x0B:
                                            ValueToInsert = "CLUTCH SWITCH";
                                            break;
                                        case 0x0C:
                                            ValueToInsert = "P/N SWITCH";
                                            break;
                                        default:
                                            ValueToInsert = "N/A";
                                            break;
                                    }
                                    break;
                                case 0xDE:
                                    DescriptionToInsert = "CRUISE CONTROL | LAST CUTOUT REASON";

                                    switch (payload[1])
                                    {
                                        case 0x02:
                                            ValueToInsert = "CRUISE SWITCH DTC";
                                            break;
                                        case 0x03:
                                            ValueToInsert = "VSS RATIONALITY";
                                            break;
                                        case 0x04:
                                            ValueToInsert = "BRAKE RATIONALITY";
                                            break;
                                        case 0x05:
                                            ValueToInsert = "ON/OFF SWITCH";
                                            break;
                                        case 0x06:
                                            ValueToInsert = "BRAKE SWITCH";
                                            break;
                                        case 0x07:
                                            ValueToInsert = "CANCEL SWITCH";
                                            break;
                                        case 0x08:
                                            ValueToInsert = "SPEED SENSOR";
                                            break;
                                        case 0x09:
                                            ValueToInsert = "RPM LIMIT";
                                            break;
                                        case 0x0A:
                                            ValueToInsert = "RPM/VSS RATIO";
                                            break;
                                        case 0x0B:
                                            ValueToInsert = "CLUTCH SWITCH";
                                            break;
                                        case 0x0C:
                                            ValueToInsert = "P/N SWITCH";
                                            break;
                                        default:
                                            ValueToInsert = "N/A";
                                            break;
                                    }
                                    break;
                                case 0xE2:
                                    DescriptionToInsert = "CRUISE INDICATOR LAMP";

                                    if (Util.IsBitSet(payload[1], 0)) ValueToInsert = "ON";
                                    else ValueToInsert = "OFF";

                                    break;
                                case 0xE4:
                                    DescriptionToInsert = "CRUISE | BUTTON PRESSED";

                                    List<string> SwitchListB = new List<string>();

                                    if (payload[1] == 0) SwitchListB.Add("ON/OFF");
                                    if (Util.IsBitSet(payload[1], 1) && Util.IsBitSet(payload[1], 0)) SwitchListB.Add("SET");

                                    if (Util.IsBitSet(payload[1], 7)) SwitchListB.Add("-7-");
                                    if (Util.IsBitSet(payload[1], 6)) SwitchListB.Add("-6-");
                                    if (Util.IsBitSet(payload[1], 5)) SwitchListB.Add("-5-");
                                    if (Util.IsBitSet(payload[1], 4)) SwitchListB.Add("-4-");
                                    if (Util.IsBitSet(payload[1], 3)) SwitchListB.Add("-3-");
                                    if (Util.IsBitSet(payload[1], 2)) SwitchListB.Add("ACC/RES");
                                    if (Util.IsBitSet(payload[1], 1)) SwitchListB.Add("COAST");
                                    if (Util.IsBitSet(payload[1], 0)) SwitchListB.Add("CANCEL");

                                    if (SwitchListB.Count == 0) break;

                                    foreach (string s in SwitchListB)
                                    {
                                        ValueToInsert += s + " | ";
                                    }

                                    if (ValueToInsert.Length > 2) ValueToInsert = ValueToInsert.Remove(ValueToInsert.Length - 3); // remove last "|" character
                                    break;
                                case 0xE5:
                                    DescriptionToInsert = "CRUISE SET SPEED";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST FB E5 E6";
                                        break;
                                    }

                                    double CruiseSetSpeedMPH = ((payload[1] << 8) + payload[3]) * 0.00390625;
                                    double CruiseSetSpeedKMH = CruiseSetSpeedMPH * 1.609344;

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        ValueToInsert = Math.Round(CruiseSetSpeedMPH, 3).ToString("0.000").Replace(",", ".");
                                        UnitToInsert = "MPH";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        ValueToInsert = Math.Round(CruiseSetSpeedKMH, 3).ToString("0.000").Replace(",", ".");
                                        UnitToInsert = "KM/H";
                                    }
                                    break;
                                case 0xE6:
                                    DescriptionToInsert = "CRUISE SET SPEED | ERROR: REQUEST FB E5 E6";
                                    break;
                                case 0xE7:
                                    DescriptionToInsert = "CRUISE SWITCH VOLTS";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST FB E7 E8";
                                        break;
                                    }

                                    double CruiseSwitchVolts = ((payload[1] << 8) + payload[3]) * 0.0049;

                                    ValueToInsert = Math.Round(CruiseSwitchVolts, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "V";
                                    break;
                                case 0xE8:
                                    DescriptionToInsert = "CRUISE SWITCH VOLTS | ERROR: REQUEST FB E7 E8";
                                    break;
                                case 0xEB:
                                    DescriptionToInsert = "INJECTORS DISABLED VEHICLE SPEED";

                                    if (message.Length < 5) break;

                                    if (payload[2] != (payload[0] + 1))
                                    {
                                        DescriptionToInsert += " | ERROR: REQUEST F8 EB EC";
                                        break;
                                    }

                                    double InjDisVehicleSpeedMPH = ((payload[1] << 8) + payload[3]) * 0.00390625;
                                    double InjDisVehicleSpeedKMH = InjDisVehicleSpeedMPH * 1.609344;

                                    if (Properties.Settings.Default.Units == "imperial")
                                    {
                                        ValueToInsert = Math.Round(InjDisVehicleSpeedMPH, 3).ToString("0.000").Replace(",", ".");
                                        UnitToInsert = "MPH";
                                    }
                                    else if (Properties.Settings.Default.Units == "metric")
                                    {
                                        ValueToInsert = Math.Round(InjDisVehicleSpeedKMH, 3).ToString("0.000").Replace(",", ".");
                                        UnitToInsert = "KM/H";
                                    }
                                    break;
                                case 0xEC:
                                    DescriptionToInsert = "INJECTORS DISABLED VEHICLE SPEED | ERROR: REQUEST FB EB EC";
                                    break;
                                default:
                                    for (int i = 0; i < HSBPNum; i++)
                                    {
                                        HSOffset.Add(payload[i * 2]);
                                        HSValues.Add(payload[(i * 2) + 1]);
                                    }

                                    DescriptionToInsert = "FB RAM TABLE | OFFSET: " + Util.ByteToHexStringSimple(HSOffset.ToArray());
                                    ValueToInsert = Util.ByteToHexStringSimple(HSValues.ToArray());
                                    break;
                            }
                        }
                        else
                        {
                            switch (payload[0])
                            {
                                default:
                                    for (int i = 0; i < HSBPNum; i++)
                                    {
                                        HSOffset.Add(payload[i * 2]);
                                        HSValues.Add(payload[(i * 2) + 1]);
                                    }

                                    DescriptionToInsert = "FB RAM TABLE | OFFSET: " + Util.ByteToHexStringSimple(HSOffset.ToArray());
                                    ValueToInsert = Util.ByteToHexStringSimple(HSValues.ToArray());
                                    break;
                            }
                        }
                        break;
                    case 0xFC: // CUMMINS STATISTICS 1
                        DescriptionToInsert = "FC RAM TABLE SELECTED";

                        if (message.Length < 3) break;

                        switch (payload[0])
                        {
                            case 0x01:
                                DescriptionToInsert = "TOTAL FUEL USED";

                                if (message.Length < 9) break;

                                if ((payload[2] != (payload[0] + 1)) || (payload[4] != (payload[0] + 2)) || (payload[6] != (payload[0] + 3)))
                                {
                                    DescriptionToInsert += " | ERROR: REQUEST FC 01 02 03 04";
                                    break;
                                }

                                double TotalFuelUsedG = (uint)(payload[1] << 24 | payload[3] << 16 | payload[5] << 8 | payload[7]) * 0.000076;
                                double TotalFuelUsedL = TotalFuelUsedG * 3.785412;

                                if (Properties.Settings.Default.Units == "imperial")
                                {
                                    ValueToInsert = Math.Round(TotalFuelUsedG, 1).ToString("0.0").Replace(",", ".");
                                    UnitToInsert = "GALLON";
                                }
                                else if (Properties.Settings.Default.Units == "metric")
                                {
                                    ValueToInsert = Math.Round(TotalFuelUsedL, 1).ToString("0.0").Replace(",", ".");
                                    UnitToInsert = "LITER";
                                }
                                break;
                            case 0x02:
                            case 0x03:
                            case 0x04:
                                DescriptionToInsert = "TOTAL FUEL USED | ERROR: REQUEST FC 01 02 03 04";
                                break;
                            case 0x05:
                                DescriptionToInsert = "TRIP FUEL USED";

                                if (message.Length < 9) break;

                                if ((payload[2] != (payload[0] + 1)) || (payload[4] != (payload[0] + 2)) || (payload[6] != (payload[0] + 3)))
                                {
                                    DescriptionToInsert += " | ERROR: REQUEST FC 05 06 07 08";
                                    break;
                                }

                                double TripFuelUsedG = (uint)(payload[1] << 24 | payload[3] << 16 | payload[5] << 8 | payload[7]) * 0.000076;
                                double TripFuelUsedL = TripFuelUsedG * 3.785412;

                                if (Properties.Settings.Default.Units == "imperial")
                                {
                                    ValueToInsert = Math.Round(TripFuelUsedG, 1).ToString("0.0").Replace(",", ".");
                                    UnitToInsert = "GALLON";
                                }
                                else if (Properties.Settings.Default.Units == "metric")
                                {
                                    ValueToInsert = Math.Round(TripFuelUsedL, 1).ToString("0.0").Replace(",", ".");
                                    UnitToInsert = "LITER";
                                }
                                break;
                            case 0x06:
                            case 0x07:
                            case 0x08:
                                DescriptionToInsert = "TRIP FUEL USED | ERROR: REQUEST FC 05 06 07 08";
                                break;
                            case 0x09:
                                DescriptionToInsert = "TOTAL TIME";

                                if (message.Length < 9) break;

                                if ((payload[2] != (payload[0] + 1)) || (payload[4] != (payload[0] + 2)) || (payload[6] != (payload[0] + 3)))
                                {
                                    DescriptionToInsert += " | ERROR: REQUEST FC 09 0A 0B 0C";
                                    break;
                                }

                                double TotalTimeHours = (uint)(payload[1] << 24 | payload[3] << 16 | payload[5] << 8 | payload[7]) * 0.000028;
                                double TotalTimeSeconds = TotalTimeHours * 3600.0;
                                TimeSpan TimestampA = TimeSpan.FromSeconds(TotalTimeSeconds);

                                ValueToInsert = TimestampA.ToString(@"hh\:mm\:ss");
                                UnitToInsert = "HH:MM:SS";
                                break;
                            case 0x0A:
                            case 0x0B:
                            case 0x0C:
                                DescriptionToInsert = "TOTAL TIME | ERROR: REQUEST FC 09 0A 0B 0C";
                                break;
                            case 0x0D:
                                DescriptionToInsert = "TRIP TIME";

                                if (message.Length < 9) break;

                                if ((payload[2] != (payload[0] + 1)) || (payload[4] != (payload[0] + 2)) || (payload[6] != (payload[0] + 3)))
                                {
                                    DescriptionToInsert += " | ERROR: REQUEST FC 0D 0E 0F 10";
                                    break;
                                }

                                double TripTimeHours = (uint)(payload[1] << 24 | payload[3] << 16 | payload[5] << 8 | payload[7]) * 0.000028;
                                double TripTimeSeconds = TripTimeHours * 3600.0;
                                TimeSpan TimestampB = TimeSpan.FromSeconds(TripTimeSeconds);

                                ValueToInsert = TimestampB.ToString(@"hh\:mm\:ss");
                                UnitToInsert = "HH:MM:SS";
                                break;
                            case 0x0E:
                            case 0x0F:
                            case 0x10:
                                DescriptionToInsert = "TRIP TIME | ERROR: REQUEST FC 0D 0E 0F 10";
                                break;
                            case 0x11:
                                DescriptionToInsert = "TOTAL IDLE FUEL";

                                if (message.Length < 9) break;

                                if ((payload[2] != (payload[0] + 1)) || (payload[4] != (payload[0] + 2)) || (payload[6] != (payload[0] + 3)))
                                {
                                    DescriptionToInsert += " | ERROR: REQUEST FC 11 12 13 14";
                                    break;
                                }

                                double TotalIdleFuelG = (uint)(payload[1] << 24 | payload[3] << 16 | payload[5] << 8 | payload[7]) * 0.000076;
                                double TotalIdleFuelL = TotalIdleFuelG * 3.785412;

                                if (Properties.Settings.Default.Units == "imperial")
                                {
                                    ValueToInsert = Math.Round(TotalIdleFuelG, 1).ToString("0.0").Replace(",", ".");
                                    UnitToInsert = "GALLON";
                                }
                                else if (Properties.Settings.Default.Units == "metric")
                                {
                                    ValueToInsert = Math.Round(TotalIdleFuelL, 1).ToString("0.0").Replace(",", ".");
                                    UnitToInsert = "LITER";
                                }
                                break;
                            case 0x12:
                            case 0x13:
                            case 0x14:
                                DescriptionToInsert = "TOTAL IDLE FUEL | ERROR: REQUEST FC 11 12 13 14";
                                break;
                            case 0x15:
                                DescriptionToInsert = "TRIP IDLE FUEL";

                                if (message.Length < 9) break;

                                if ((payload[2] != (payload[0] + 1)) || (payload[4] != (payload[0] + 2)) || (payload[6] != (payload[0] + 3)))
                                {
                                    DescriptionToInsert += " | ERROR: REQUEST FC 15 16 17 18";
                                    break;
                                }

                                double TripIdleFuelG = (uint)(payload[1] << 24 | payload[3] << 16 | payload[5] << 8 | payload[7]) * 0.000076;
                                double TripIdleFuelL = TripIdleFuelG * 3.785412;

                                if (Properties.Settings.Default.Units == "imperial")
                                {
                                    ValueToInsert = Math.Round(TripIdleFuelG, 1).ToString("0.0").Replace(",", ".");
                                    UnitToInsert = "GALLON";
                                }
                                else if (Properties.Settings.Default.Units == "metric")
                                {
                                    ValueToInsert = Math.Round(TripIdleFuelL, 1).ToString("0.0").Replace(",", ".");
                                    UnitToInsert = "LITER";
                                }
                                break;
                            case 0x16:
                            case 0x17:
                            case 0x18:
                                DescriptionToInsert = "TRIP IDLE FUEL | ERROR: REQUEST FC 15 16 17 18";
                                break;
                            case 0x19:
                                DescriptionToInsert = "TOTAL IDLE TIME";

                                if (message.Length < 9) break;

                                if ((payload[2] != (payload[0] + 1)) || (payload[4] != (payload[0] + 2)) || (payload[6] != (payload[0] + 3)))
                                {
                                    DescriptionToInsert += " | ERROR: REQUEST FC 19 1A 1B 1C";
                                    break;
                                }

                                double TotalIdleTimeHours = (uint)(payload[1] << 24 | payload[3] << 16 | payload[5] << 8 | payload[7]) * 0.000028;
                                double TotalIdleTimeSeconds = TotalIdleTimeHours * 3600.0;
                                TimeSpan TimestampC = TimeSpan.FromSeconds(TotalIdleTimeSeconds);

                                ValueToInsert = TimestampC.ToString(@"hh\:mm\:ss");
                                UnitToInsert = "HH:MM:SS";
                                break;
                            case 0x1A:
                            case 0x1B:
                            case 0x1C:
                                DescriptionToInsert = "TOTAL IDLE TIME | ERROR: REQUEST FC 19 1A 1B 1C";
                                break;
                            case 0x1D:
                                DescriptionToInsert = "TRIP IDLE TIME";

                                if (message.Length < 9) break;

                                if ((payload[2] != (payload[0] + 1)) || (payload[4] != (payload[0] + 2)) || (payload[6] != (payload[0] + 3)))
                                {
                                    DescriptionToInsert += " | ERROR: REQUEST FC 1D 1E 1F 20";
                                    break;
                                }

                                double TripIdleTimeHours = (uint)(payload[1] << 24 | payload[3] << 16 | payload[5] << 8 | payload[7]) * 0.000028;
                                double TripIdleTimeSeconds = TripIdleTimeHours * 3600.0;
                                TimeSpan TimestampD = TimeSpan.FromSeconds(TripIdleTimeSeconds);

                                ValueToInsert = TimestampD.ToString(@"hh\:mm\:ss");
                                UnitToInsert = "HH:MM:SS";
                                break;
                            case 0x1E:
                            case 0x1F:
                            case 0x20:
                                DescriptionToInsert = "TRIP IDLE TIME | ERROR: REQUEST FC 1D 1E 1F 20";
                                break;
                            case 0x21:
                                DescriptionToInsert = "TOTAL DISTANCE";

                                if (message.Length < 9) break;

                                if ((payload[2] != (payload[0] + 1)) || (payload[4] != (payload[0] + 2)) || (payload[6] != (payload[0] + 3)))
                                {
                                    DescriptionToInsert += " | ERROR: REQUEST FC 21 22 23 24";
                                    break;
                                }

                                double TotalDistanceMi = (uint)(payload[1] << 24 | payload[3] << 16 | payload[5] << 8 | payload[7]) * 0.000125;
                                double TotalDistanceKm = TotalDistanceMi * 1.609344;

                                if (Properties.Settings.Default.Units == "imperial")
                                {
                                    ValueToInsert = Math.Round(TotalDistanceMi, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "MILE";
                                }
                                else if (Properties.Settings.Default.Units == "metric")
                                {
                                    ValueToInsert = Math.Round(TotalDistanceKm, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "KILOMETER";
                                }
                                break;
                            case 0x22:
                            case 0x23:
                            case 0x24:
                                DescriptionToInsert = "TOTAL DISTANCE | ERROR: REQUEST FC 21 22 23 24";
                                break;
                            case 0x25:
                                DescriptionToInsert = "TRIP DISTANCE";

                                if (message.Length < 9) break;

                                if ((payload[2] != (payload[0] + 1)) || (payload[4] != (payload[0] + 2)) || (payload[6] != (payload[0] + 3)))
                                {
                                    DescriptionToInsert += " | ERROR: REQUEST FC 25 26 27 28";
                                    break;
                                }

                                double TripDistanceMi = (uint)(payload[1] << 24 | payload[3] << 16 | payload[5] << 8 | payload[7]) * 0.000125;
                                double TripDistanceKm = TripDistanceMi * 1.609344;

                                if (Properties.Settings.Default.Units == "imperial")
                                {
                                    ValueToInsert = Math.Round(TripDistanceMi, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "MILE";
                                }
                                else if (Properties.Settings.Default.Units == "metric")
                                {
                                    ValueToInsert = Math.Round(TripDistanceKm, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "KILOMETER";
                                }
                                break;
                            case 0x26:
                            case 0x27:
                            case 0x28:
                                DescriptionToInsert = "TRIP DISTANCE | ERROR: REQUEST FC 25 26 27 28";
                                break;
                            case 0x29:
                                DescriptionToInsert = "TRIP AVERAGE FUEL";

                                if (message.Length < 5) break;

                                if (payload[2] != (payload[0] + 1))
                                {
                                    DescriptionToInsert += " | ERROR: REQUEST FC 29 2A";
                                    break;
                                }

                                double TripAverageFuelMPG = ((payload[1] << 8) + payload[3]) * 0.125;
                                double TripAverageFuelLP100KM = 235.214583 / TripAverageFuelMPG;

                                if (Properties.Settings.Default.Units == "imperial")
                                {
                                    ValueToInsert = Math.Round(TripAverageFuelMPG, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "MPG";
                                }
                                else if (Properties.Settings.Default.Units == "metric")
                                {
                                    ValueToInsert = Math.Round(TripAverageFuelLP100KM, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "L/100KM";
                                }
                                break;
                            case 0x2A:
                                DescriptionToInsert = "TRIP AVERAGE FUEL | ERROR: REQUEST FC 29 2A";
                                break;
                            case 0x2B:
                                DescriptionToInsert = "ECM RUN TIME";

                                if (message.Length < 9) break;

                                if ((payload[2] != (payload[0] + 1)) || (payload[4] != (payload[0] + 2)) || (payload[6] != (payload[0] + 3)))
                                {
                                    DescriptionToInsert += " | ERROR: REQUEST FC 2B 2C 2D 2E";
                                    break;
                                }

                                double ECMRunTimeSeconds = (uint)(payload[1] << 24 | payload[3] << 16 | payload[5] << 8 | payload[7]) * 0.2;
                                TimeSpan TimestampE = TimeSpan.FromSeconds(ECMRunTimeSeconds);

                                ValueToInsert = TimestampE.ToString(@"hh\:mm\:ss");
                                UnitToInsert = "HH:MM:SS";
                                break;
                            case 0x2C:
                            case 0x2D:
                            case 0x2E:
                                DescriptionToInsert = "ECM RUN TIME | ERROR: REQUEST FC 2B 2C 2D 2E";
                                break;
                            case 0x2F:
                                DescriptionToInsert = "ENGINE RUN TIME";

                                if (message.Length < 9) break;

                                if ((payload[2] != (payload[0] + 1)) || (payload[4] != (payload[0] + 2)) || (payload[6] != (payload[0] + 3)))
                                {
                                    DescriptionToInsert += " | ERROR: REQUEST FC 2F 30 31 32";
                                    break;
                                }

                                double EngineRunTimeSeconds = (uint)(payload[1] << 24 | payload[3] << 16 | payload[5] << 8 | payload[7]) * 0.2;
                                TimeSpan TimestampF = TimeSpan.FromSeconds(EngineRunTimeSeconds);

                                ValueToInsert = TimestampF.ToString(@"hh\:mm\:ss");
                                UnitToInsert = "HH:MM:SS";
                                break;
                            case 0x30:
                            case 0x31:
                            case 0x32:
                                DescriptionToInsert = "ENGINE RUN TIME | ERROR: REQUEST FC 2F 30 31 32";
                                break;
                            default:
                                for (int i = 0; i < HSBPNum; i++)
                                {
                                    HSOffset.Add(payload[i * 2]);
                                    HSValues.Add(payload[(i * 2) + 1]);
                                }

                                DescriptionToInsert = "FC RAM TABLE | OFFSET: " + Util.ByteToHexStringSimple(HSOffset.ToArray());
                                ValueToInsert = Util.ByteToHexStringSimple(HSValues.ToArray());
                                break;
                        }
                        break;
                    case 0xFD: // CUMMINS STATISTICS 2
                        DescriptionToInsert = "FD RAM TABLE SELECTED";

                        if (message.Length < 3) break;

                        switch (payload[0])
                        {
                            case 0x21:
                                DescriptionToInsert = "FUEL PRESSURE REGULATOR OUTPUT";

                                if (message.Length < 5) break;

                                if (payload[2] != (payload[0] + 1))
                                {
                                    DescriptionToInsert += " | ERROR: REQUEST FD 21 22";
                                    break;
                                }

                                double FuelPressureRegOutput = ((payload[1] << 8) + payload[3]) * 0.025;

                                ValueToInsert = Math.Round(FuelPressureRegOutput, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "PERCENT";
                                break;
                            case 0x22:
                                DescriptionToInsert = "FUEL PRESSURE REGULATOR OUTPUT | ERROR: REQUEST FD 21 22";
                                break;
                            case 0x23:
                                DescriptionToInsert = "FUEL PRESSURE";

                                if (message.Length < 5) break;

                                if (payload[2] != (payload[0] + 1))
                                {
                                    DescriptionToInsert += " | ERROR: REQUEST FD 23 24";
                                    break;
                                }

                                double FuelPressurePSI = ((payload[1] << 8) + payload[3]) * 7.078;
                                double FuelPressureKPA = FuelPressurePSI * 6.894757;

                                if (Properties.Settings.Default.Units == "imperial")
                                {
                                    ValueToInsert = Math.Round(FuelPressurePSI, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "PSI";
                                }
                                else if (Properties.Settings.Default.Units == "metric")
                                {
                                    ValueToInsert = Math.Round(FuelPressureKPA, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "KPA";
                                }
                                break;
                            case 0x24:
                                DescriptionToInsert = "FUEL PRESSURE | ERROR: REQUEST FD 23 24";
                                break;
                            case 0x27:
                                DescriptionToInsert = "FUEL PRESSURE SETPOINT";

                                if (message.Length < 5) break;

                                if (payload[2] != (payload[0] + 1))
                                {
                                    DescriptionToInsert += " | ERROR: REQUEST FD 27 28";
                                    break;
                                }

                                double FuelPressureSetpointPSI = ((payload[1] << 8) + payload[3]) * 7.078;
                                double FuelPressureSetpointKPA = FuelPressureSetpointPSI * 6.894757;

                                if (Properties.Settings.Default.Units == "imperial")
                                {
                                    ValueToInsert = Math.Round(FuelPressureSetpointPSI, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "PSI";
                                }
                                else if (Properties.Settings.Default.Units == "metric")
                                {
                                    ValueToInsert = Math.Round(FuelPressureSetpointKPA, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "KPA";
                                }
                                break;
                            case 0x28:
                                DescriptionToInsert = "FUEL PRESSURE SETPOINT | ERROR: REQUEST FD 27 28";
                                break;
                            case 0x29:
                                DescriptionToInsert = "FUEL PRESSURE SENSOR VOLTS";

                                if (message.Length < 5) break;

                                if (payload[2] != (payload[0] + 1))
                                {
                                    DescriptionToInsert += " | ERROR: REQUEST FD 29 2A";
                                    break;
                                }

                                double FuelPressureSensorVolts = ((payload[1] << 8) + payload[3]) * 0.0012;

                                ValueToInsert = Math.Round(FuelPressureSensorVolts, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "V";
                                break;
                            case 0x2A:
                                DescriptionToInsert = "FUEL PRESSURE SENSOR VOLTS | ERROR: REQUEST FD 29 2A";
                                break;
                            case 0x7C:
                                DescriptionToInsert = "CVN";

                                if (message.Length < 9) break;

                                if ((payload[2] != (payload[0] + 1)) || (payload[4] != (payload[0] + 2)) || (payload[6] != (payload[0] + 3)))
                                {
                                    DescriptionToInsert += " | ERROR: REQUEST FD 7C 7D 7E 7F";
                                    break;
                                }

                                double CVNVolts = (uint)(payload[1] << 24 | payload[3] << 16 | payload[5] << 8 | payload[7]) * 0.0049;

                                ValueToInsert = Math.Round(CVNVolts, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "V";
                                break;
                            case 0x7D:
                            case 0x7E:
                            case 0x7F:
                                DescriptionToInsert = "CVN | ERROR: REQUEST FD 7C 7D 7E 7F";
                                break;
                            case 0x80:
                                DescriptionToInsert = "RADIATOR FAN SPEED";

                                if (message.Length < 5) break;

                                if (payload[2] != (payload[0] + 1))
                                {
                                    DescriptionToInsert += " | ERROR: REQUEST FD 80 81";
                                    break;
                                }

                                double RadiatorFanSpeed = ((payload[1] << 8) + payload[3]) * 0.125;

                                ValueToInsert = Math.Round(RadiatorFanSpeed, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "RPM";
                                break;
                            case 0x81:
                                DescriptionToInsert = "RADIATOR FAN SPEED | ERROR: REQUEST FD 80 81";
                                break;
                            case 0x82:
                                DescriptionToInsert = "DESIRED RADIATOR FAN PWM";

                                if (message.Length < 5) break;

                                if (payload[2] != (payload[0] + 1))
                                {
                                    DescriptionToInsert += " | ERROR: REQUEST FD 82 83";
                                    break;
                                }

                                double DesRadFanPWM = ((payload[1] << 8) + payload[3]) * 0.00390625;

                                ValueToInsert = Math.Round(DesRadFanPWM, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "PERCENT";
                                break;
                            case 0x83:
                                DescriptionToInsert = "DESIRED RADIATOR FAN PWM | ERROR: REQUEST FD 82 83";
                                break;
                            case 0x84:
                                DescriptionToInsert = "% OF TIME @  0-10% LOAD";

                                if (message.Length < 5) break;

                                if (payload[2] != (payload[0] + 1))
                                {
                                    DescriptionToInsert += " | ERROR: REQUEST FD 84 85";
                                    break;
                                }

                                double TimeAt10Load = ((payload[1] << 8) + payload[3]) * 0.00390625;

                                ValueToInsert = Math.Round(TimeAt10Load, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "PERCENT";
                                break;
                            case 0x85:
                                DescriptionToInsert = "% OF TIME @  0-10% LOAD | ERROR: REQUEST FD 84 85";
                                break;
                            case 0x86:
                                DescriptionToInsert = "% OF TIME @ 11-20% LOAD";

                                if (message.Length < 5) break;

                                if (payload[2] != (payload[0] + 1))
                                {
                                    DescriptionToInsert += " | ERROR: REQUEST FD 86 87";
                                    break;
                                }

                                double TimeAt20Load = ((payload[1] << 8) + payload[3]) * 0.00390625;

                                ValueToInsert = Math.Round(TimeAt20Load, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "PERCENT";
                                break;
                            case 0x87:
                                DescriptionToInsert = "% OF TIME @ 11-20% LOAD | ERROR: REQUEST FD 86 87";
                                break;
                            case 0x88:
                                DescriptionToInsert = "% OF TIME @ 21-30% LOAD";

                                if (message.Length < 5) break;

                                if (payload[2] != (payload[0] + 1))
                                {
                                    DescriptionToInsert += " | ERROR: REQUEST FD 88 89";
                                    break;
                                }

                                double TimeAt30Load = ((payload[1] << 8) + payload[3]) * 0.00390625;

                                ValueToInsert = Math.Round(TimeAt30Load, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "PERCENT";
                                break;
                            case 0x89:
                                DescriptionToInsert = "% OF TIME @ 21-30% LOAD | ERROR: REQUEST FD 88 89";
                                break;
                            case 0x8A:
                                DescriptionToInsert = "% OF TIME @ 31-40% LOAD";

                                if (message.Length < 5) break;

                                if (payload[2] != (payload[0] + 1))
                                {
                                    DescriptionToInsert += " | ERROR: REQUEST FD 8A 8B";
                                    break;
                                }

                                double TimeAt40Load = ((payload[1] << 8) + payload[3]) * 0.00390625;

                                ValueToInsert = Math.Round(TimeAt40Load, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "PERCENT";
                                break;
                            case 0x8B:
                                DescriptionToInsert = "% OF TIME @ 31-40% LOAD | ERROR: REQUEST FD 8A 8B";
                                break;
                            case 0x8C:
                                DescriptionToInsert = "% OF TIME @ 41-50% LOAD";

                                if (message.Length < 5) break;

                                if (payload[2] != (payload[0] + 1))
                                {
                                    DescriptionToInsert += " | ERROR: REQUEST FD 8C 8D";
                                    break;
                                }

                                double TimeAt50Load = ((payload[1] << 8) + payload[3]) * 0.00390625;

                                ValueToInsert = Math.Round(TimeAt50Load, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "PERCENT";
                                break;
                            case 0x8D:
                                DescriptionToInsert = "% OF TIME @ 41-50% LOAD | ERROR: REQUEST FD 8C 8D";
                                break;
                            case 0x8E:
                                DescriptionToInsert = "% OF TIME @ 51-60% LOAD";

                                if (message.Length < 5) break;

                                if (payload[2] != (payload[0] + 1))
                                {
                                    DescriptionToInsert += " | ERROR: REQUEST FD 8E 8F";
                                    break;
                                }

                                double TimeAt60Load = ((payload[1] << 8) + payload[3]) * 0.00390625;

                                ValueToInsert = Math.Round(TimeAt60Load, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "PERCENT";
                                break;
                            case 0x8F:
                                DescriptionToInsert = "% OF TIME @ 51-60% LOAD | ERROR: REQUEST FD 8E 8F";
                                break;
                            case 0x90:
                                DescriptionToInsert = "% OF TIME @ 61-70% LOAD";

                                if (message.Length < 5) break;

                                if (payload[2] != (payload[0] + 1))
                                {
                                    DescriptionToInsert += " | ERROR: REQUEST FD 90 91";
                                    break;
                                }

                                double TimeAt70Load = ((payload[1] << 8) + payload[3]) * 0.00390625;

                                ValueToInsert = Math.Round(TimeAt70Load, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "PERCENT";
                                break;
                            case 0x91:
                                DescriptionToInsert = "% OF TIME @ 61-70% LOAD | ERROR: REQUEST FD 90 91";
                                break;
                            case 0x92:
                                DescriptionToInsert = "% OF TIME @ 71-80% LOAD";

                                if (message.Length < 5) break;

                                if (payload[2] != (payload[0] + 1))
                                {
                                    DescriptionToInsert += " | ERROR: REQUEST FD 92 93";
                                    break;
                                }

                                double TimeAt80Load = ((payload[1] << 8) + payload[3]) * 0.00390625;

                                ValueToInsert = Math.Round(TimeAt80Load, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "PERCENT";
                                break;
                            case 0x93:
                                DescriptionToInsert = "% OF TIME @ 71-80% LOAD | ERROR: REQUEST FD 92 93";
                                break;
                            case 0x94:
                                DescriptionToInsert = "% OF TIME @ 81-90% LOAD";

                                if (message.Length < 5) break;

                                if (payload[2] != (payload[0] + 1))
                                {
                                    DescriptionToInsert += " | ERROR: REQUEST FD 94 95";
                                    break;
                                }

                                double TimeAt90Load = ((payload[1] << 8) + payload[3]) * 0.00390625;

                                ValueToInsert = Math.Round(TimeAt90Load, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "PERCENT";
                                break;
                            case 0x95:
                                DescriptionToInsert = "% OF TIME @ 81-90% LOAD | ERROR: REQUEST FD 94 95";
                                break;
                            case 0x96:
                                DescriptionToInsert = "% OF TIME @ 91-100% LOAD";

                                if (message.Length < 5) break;

                                if (payload[2] != (payload[0] + 1))
                                {
                                    DescriptionToInsert += " | ERROR: REQUEST FD 96 97";
                                    break;
                                }

                                double TimeAt100Load = ((payload[1] << 8) + payload[3]) * 0.00390625;

                                ValueToInsert = Math.Round(TimeAt100Load, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "PERCENT";
                                break;
                            case 0x97:
                                DescriptionToInsert = "% OF TIME @ 91-100% LOAD | ERROR: REQUEST FD 96 97";
                                break;
                            case 0xA0:
                                DescriptionToInsert = "BAROMETRIC PRESSURE";

                                if (message.Length < 5) break;

                                if (payload[2] != (payload[0] + 1))
                                {
                                    DescriptionToInsert += " | ERROR: REQUEST FD A0 A1";
                                    break;
                                }

                                double BarometricPressurePSI = ((payload[1] << 8) + payload[3]) * 0.0159 * 0.4911542; // 1 inHg = 0.4911542 psi
                                double BarometricPressureKPA = BarometricPressurePSI * 6.894757;

                                if (Properties.Settings.Default.Units == "imperial")
                                {
                                    ValueToInsert = Math.Round(BarometricPressurePSI, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "PSI";
                                }
                                else if (Properties.Settings.Default.Units == "metric")
                                {
                                    ValueToInsert = Math.Round(BarometricPressureKPA, 3).ToString("0.000").Replace(",", ".");
                                    UnitToInsert = "KPA";
                                }
                                break;
                            case 0xA1:
                                DescriptionToInsert = "BAROMETRIC PRESSURE | ERROR: REQUEST FD A0 A1";
                                break;
                            case 0xA4:
                                DescriptionToInsert = "AMBIENT AIR TEMPERATURE";

                                if (message.Length < 5) break;

                                if (payload[2] != (payload[0] + 1))
                                {
                                    DescriptionToInsert += " | ERROR: REQUEST FD A4 A5";
                                    break;
                                }

                                double AmbientAirTempF = ((payload[1] << 8) + payload[3]) * 0.015625;
                                double AmbientAirTempC = (AmbientAirTempF - 32.0) / 1.8;

                                if (Properties.Settings.Default.Units == "imperial")
                                {
                                    ValueToInsert = Math.Round(AmbientAirTempF, 1).ToString("0.0").Replace(",", ".");
                                    UnitToInsert = "°F";
                                }
                                else if (Properties.Settings.Default.Units == "metric")
                                {
                                    ValueToInsert = Math.Round(AmbientAirTempC, 1).ToString("0.0").Replace(",", ".");
                                    UnitToInsert = "°C";
                                }
                                break;
                            case 0xA5:
                                DescriptionToInsert = "AMBIENT AIR TEMPERATURE | ERROR: REQUEST FD A4 A5";
                                break;
                            case 0xA6:
                                DescriptionToInsert = "AMBIENT AIR TEMPERATURE SENSOR VOLTS";

                                if (message.Length < 5) break;

                                if (payload[2] != (payload[0] + 1))
                                {
                                    DescriptionToInsert += " | ERROR: REQUEST FD A6 A7";
                                    break;
                                }

                                double AmbientTempSensorVolts = ((payload[1] << 8) + payload[3]) * 0.0049;

                                ValueToInsert = Math.Round(AmbientTempSensorVolts, 3).ToString("0.000").Replace(",", ".");
                                UnitToInsert = "V";
                                break;
                            case 0xA7:
                                DescriptionToInsert = "AMBIENT TEMP SENSOR VOLTS | ERROR: REQUEST FD A6 A7";
                                break;
                            case 0xE1:
                                DescriptionToInsert = "CYLINDER 1 CONTRIBUTION";

                                int Cylinder1Contribution = payload[1];

                                ValueToInsert = Cylinder1Contribution.ToString("0");
                                UnitToInsert = "PERCENT";
                                break;
                            case 0xE2:
                                DescriptionToInsert = "CYLINDER 5 CONTRIBUTION";

                                int Cylinder5Contribution = payload[1];

                                ValueToInsert = Cylinder5Contribution.ToString("0");
                                UnitToInsert = "PERCENT";
                                break;
                            case 0xE3:
                                DescriptionToInsert = "CYLINDER 3 CONTRIBUTION";

                                int Cylinder3Contribution = payload[1];

                                ValueToInsert = Cylinder3Contribution.ToString("0");
                                UnitToInsert = "PERCENT";
                                break;
                            case 0xE4:
                                DescriptionToInsert = "CYLINDER 6 CONTRIBUTION";

                                int Cylinder6Contribution = payload[1];

                                ValueToInsert = Cylinder6Contribution.ToString("0");
                                UnitToInsert = "PERCENT";
                                break;
                            case 0xE5:
                                DescriptionToInsert = "CYLINDER 2 CONTRIBUTION";

                                int Cylinder2Contribution = payload[1];

                                ValueToInsert = Cylinder2Contribution.ToString("0");
                                UnitToInsert = "PERCENT";
                                break;
                            case 0xE6:
                                DescriptionToInsert = "CYLINDER 4 CONTRIBUTION";

                                int Cylinder4Contribution = payload[1];

                                ValueToInsert = Cylinder4Contribution.ToString("0");
                                UnitToInsert = "PERCENT";
                                break;
                            case 0xE7:
                                DescriptionToInsert = "CYLINDER 1-3 CONTRIBUTION";

                                int Cylinder13Contribution = payload[1];

                                ValueToInsert = Cylinder13Contribution.ToString("0");
                                UnitToInsert = "PERCENT";
                                break;
                            case 0xE8:
                                DescriptionToInsert = "CYLINDER 4-6 CONTRIBUTION";

                                int Cylinder46Contribution = payload[1];

                                ValueToInsert = Cylinder46Contribution.ToString("0");
                                UnitToInsert = "PERCENT";
                                break;
                            case 0xEA:
                                DescriptionToInsert = "ENGINE SPEED";

                                int EnginSpeedLowRes = payload[1] * 32;

                                ValueToInsert = EnginSpeedLowRes.ToString("0");
                                UnitToInsert = "RPM";
                                break;
                            case 0xEB:
                                DescriptionToInsert = "CYLINDER TEST STATUS";

                                switch (payload[1])
                                {
                                    case 0x00:
                                        ValueToInsert = "NOT RUNNING";
                                        break;
                                    case 0x01:
                                        ValueToInsert = "RUNNING";
                                        break;
                                    case 0x02:
                                        ValueToInsert = "ABORTED";
                                        break;
                                    default:
                                        break;
                                }
                                break;
                            case 0xEC:
                                DescriptionToInsert = "FPO TEST STATUS";

                                switch (payload[1])
                                {
                                    case 0x00:
                                        ValueToInsert = "NOT RUNNING";
                                        break;
                                    case 0x01:
                                        ValueToInsert = "RUNNING";
                                        break;
                                    case 0x02:
                                        ValueToInsert = "ABORTED";
                                        break;
                                    default:
                                        break;
                                }
                                break;
                            default:
                                for (int i = 0; i < HSBPNum; i++)
                                {
                                    HSOffset.Add(payload[i * 2]);
                                    HSValues.Add(payload[(i * 2) + 1]);
                                }

                                DescriptionToInsert = "FD RAM TABLE | OFFSET: " + Util.ByteToHexStringSimple(HSOffset.ToArray());
                                ValueToInsert = Util.ByteToHexStringSimple(HSValues.ToArray());
                                break;
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

            string HexBytesToInsert;

            if (message.Length < 9) // max 8 message byte fits the message column
            {
                HexBytesToInsert = Util.ByteToHexString(message, 0, message.Length) + " ";
            }
            else // trim message (display 7 bytes only) and insert two dots at the end indicating there's more to it
            {
                HexBytesToInsert = Util.ByteToHexString(message, 0, 7) + " .. ";
            }

            if (DescriptionToInsert.Length > 51) DescriptionToInsert = Util.TruncateString(DescriptionToInsert, 48) + "...";
            if (ValueToInsert.Length > 23) ValueToInsert = Util.TruncateString(ValueToInsert, 20) + "...";
            if (UnitToInsert.Length > 11) UnitToInsert = Util.TruncateString(UnitToInsert, 8) + "...";

            StringBuilder RowToAdd = new StringBuilder(EmptyLine); // add empty line first

            RowToAdd.Remove(HexBytesColumnStart, HexBytesToInsert.Length);
            RowToAdd.Insert(HexBytesColumnStart, HexBytesToInsert);

            RowToAdd.Remove(DescriptionColumnStart, DescriptionToInsert.Length);
            RowToAdd.Insert(DescriptionColumnStart, DescriptionToInsert);

            RowToAdd.Remove(ValueColumnStart, ValueToInsert.Length);
            RowToAdd.Insert(ValueColumnStart, ValueToInsert);

            RowToAdd.Remove(UnitColumnStart, UnitToInsert.Length);
            RowToAdd.Insert(UnitColumnStart, UnitToInsert);

            ushort modifiedID;

            if ((ID == 0x14) || (ID == 0x22) || (ID == 0x25) || (ID == 0x2A) || ((ID >= 0xF0) && (ID < 0xFE)))
            {
                if (payload.Length > 0) modifiedID = (ushort)(((ID << 8) & 0xFF00) + payload[0]);
                else modifiedID = (ushort)((ID << 8) & 0xFF00);
            }
            else
            {
                modifiedID = (ushort)((ID << 8) & 0xFF00);
            }

            Diagnostics.AddRow(modifiedID, RowToAdd.ToString());

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

            if (!StoredFaultCodesSaved)
            {
                StringBuilder sb = new StringBuilder();

                if (StoredFaultCodeList.Count > 0)
                {
                    sb.Append("STORED FAULT CODE LIST:" + Environment.NewLine);

                    foreach (byte code in StoredFaultCodeList)
                    {
                        if (code == 0x00) continue; // skip this iteration
                        
                        int index = SBEC3EngineDTC.Rows.IndexOf(SBEC3EngineDTC.Rows.Find(code));
                        byte[] temp = new byte[1] { code };

                        if (index > -1) // DTC description found
                        {
                            sb.Append(Util.ByteToHexStringSimple(temp) + ": " + SBEC3EngineDTC.Rows[index]["description"] + Environment.NewLine);
                        }
                        else // no DTC description found
                        {
                            sb.Append(Util.ByteToHexStringSimple(temp) + ": UNRECOGNIZED DTC" + Environment.NewLine);
                        }
                    }

                    sb.Remove(sb.Length - 1, 1); // remove last newline character

                    File.AppendAllText(MainForm.PCMLogFilename, Environment.NewLine + sb.ToString() + Environment.NewLine);
                }
                else
                {
                    sb.Append("NO STORED FAULT CODE");
                    File.AppendAllText(MainForm.PCMLogFilename, Environment.NewLine + sb.ToString() + Environment.NewLine + Environment.NewLine);
                }

                StoredFaultCodesSaved = true;
            }

            if (!PendingFaultCodesSaved)
            {
                StringBuilder sb = new StringBuilder();

                if (PendingFaultCodeList.Count > 0)
                {
                    sb.Append("PENDING FAULT CODE LIST:" + Environment.NewLine);

                    foreach (byte code in PendingFaultCodeList)
                    {
                        if (code == 0x00) continue; // skip this iteration

                        int index = SBEC3EngineDTC.Rows.IndexOf(SBEC3EngineDTC.Rows.Find(code));
                        byte[] temp = new byte[1] { code };

                        if (index > -1) // DTC description found
                        {
                            sb.Append(Util.ByteToHexStringSimple(temp) + ": " + SBEC3EngineDTC.Rows[index]["description"] + Environment.NewLine);
                        }
                        else // no DTC description found
                        {
                            sb.Append(Util.ByteToHexStringSimple(temp) + ": UNRECOGNIZED DTC" + Environment.NewLine);
                        }
                    }

                    sb.Remove(sb.Length - 1, 1); // remove last newline character

                    File.AppendAllText(MainForm.PCMLogFilename, Environment.NewLine + sb.ToString() + Environment.NewLine);
                }
                else
                {
                    sb.Append("NO PENDING FAULT CODE");
                    File.AppendAllText(MainForm.PCMLogFilename, Environment.NewLine + sb.ToString() + Environment.NewLine + Environment.NewLine);
                }

                PendingFaultCodesSaved = true;
            }

            if (!FaultCodes1TSaved) // one-trip fault codes
            {
                StringBuilder sb = new StringBuilder();

                if (FaultCode1TList.Count > 0)
                {
                    sb.Append("ONE-TRIP FAULT CODE LIST:" + Environment.NewLine);

                    foreach (byte code in FaultCode1TList)
                    {
                        if (code == 0x00) continue; // skip this iteration

                        int index = SBEC3EngineDTC.Rows.IndexOf(SBEC3EngineDTC.Rows.Find(code));
                        byte[] temp = new byte[1] { code };

                        if (index > -1) // DTC description found
                        {
                            sb.Append(Util.ByteToHexStringSimple(temp) + ": " + SBEC3EngineDTC.Rows[index]["description"] + Environment.NewLine);
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
                    sb.Append("NO ONE-TRIP FAULT CODE");
                    File.AppendAllText(MainForm.PCMLogFilename, Environment.NewLine + sb.ToString() + Environment.NewLine + Environment.NewLine);
                }

                FaultCodes1TSaved = true;
            }
        }
    }
}
