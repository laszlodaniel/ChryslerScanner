using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace ChryslerScanner
{
    public class CCD
    {
        public CCDDiagnosticsTable Diagnostics = new CCDDiagnosticsTable();

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

        public bool transmissionLRCVIRequested = false;
        public bool transmission24CVIRequested = false;
        public bool transmissionODCVIRequested = false;
        public bool transmissionUDCVIRequested = false;
        public bool transmissionTemperatureRequested = false;

        public string VIN = "-----------------"; // 17 characters

        public CCD()
        {
            // Empty.
        }

        public void UpdateHeader(string state = "enabled", string speed = null, string logic = null)
        {
            if (state != null) this.state = state;
            if (speed != null) this.speed = speed;
            if (logic != null) this.logic = logic;

            if ((this.state == "enabled") && (this.speed != null) && (this.logic != null))
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
            ushort modifiedID; // extends ordinary ID with additional index for sorting

            if (ID == 0x94) // a single gauge position message holds many gauge positions
            {
                if (payload.Length > 1) modifiedID = (ushort)(((ID << 8) & 0xFF00) + (payload[1] & 0x0F)); // add gauge number after ID (94 00, 94 01, 94 02...)
                else modifiedID = (ushort)((ID << 8) & 0xFF00); // keep ID byte only (94 00)
            }
            else
            {
                modifiedID = (ushort)((ID << 8) & 0xFF00); // keep ID byte only (XX 00)
            }

            string descriptionToInsert;
            string valueToInsert = string.Empty;
            string unitToInsert = string.Empty;

            switch (ID)
            {
                case 0x02:
                    descriptionToInsert = "SHIFT LEVER POSITION";

                    if (message.Length >= 3)
                    {
                        switch (payload[0])
                        {
                            case 0x01:
                                valueToInsert = "PARK";
                                break;
                            case 0x02:
                                valueToInsert = "REVERSE";
                                break;
                            case 0x03:
                                valueToInsert = "NEUTRAL";
                                break;
                            case 0x05:
                                valueToInsert = "DRIVE";
                                break;
                            case 0x06:
                                valueToInsert = "AUTOSHIFT";
                                break;
                            default:
                                valueToInsert = "UNDEFINED";
                                break;
                        }
                    }
                    break;
                case 0x0A:
                    descriptionToInsert = "SEND DIAGNOSTIC FAILURE DATA";

                    if (message.Length >= 4)
                    {
                        // Empty.
                    }
                    break;
                case 0x0B:
                    descriptionToInsert = "SKIM STATUS";

                    if (message.Length >= 3)
                    {
                        valueToInsert = Convert.ToString(payload[0], 2).PadLeft(8, '0');
                    }
                    break;
                case 0x0C:
                    descriptionToInsert = "BATTERY | OIL | COOLANT | IAT";

                    if (message.Length >= 6)
                    {
                        double BatteryVoltage = payload[0] * 0.125;
                        double OilPressurePSI = payload[1] * 0.5;
                        double OilPressureKPA = payload[1] * 0.5 * 6.894757;
                        double CoolantTemperatureC = payload[2] - 64;
                        double CoolantTemperatureF = 1.8 * CoolantTemperatureC + 32.0;
                        double IntakeAirTemperatureC = payload[3] - 64;
                        double IntakeAirTemperatureF = 1.8 * IntakeAirTemperatureC + 32.0;

                        if (Properties.Settings.Default.Units == "imperial")
                        {
                            descriptionToInsert = "BATTERY: " + BatteryVoltage.ToString("0.0").Replace(",", ".") + " V | OIL: " + OilPressurePSI.ToString("0.0").Replace(",", ".") + " PSI | COOLANT: " + CoolantTemperatureF.ToString("0") + " °F";
                            valueToInsert = "IAT: " + IntakeAirTemperatureF.ToString("0") + " °F";
                        }
                        else if (Properties.Settings.Default.Units == "metric")
                        {
                            descriptionToInsert = "BATTERY: " + BatteryVoltage.ToString("0.0").Replace(",", ".") + " V | OIL: " + OilPressureKPA.ToString("0.0").Replace(",", ".") + " KPA | COOLANT: " + CoolantTemperatureC.ToString("0") + " °C";
                            valueToInsert = "IAT: " + IntakeAirTemperatureC.ToString("0") + " °C";
                        }
                    }
                    break;
                case 0x0D:
                    descriptionToInsert = "UNKNOWN FEATURE PRESENT";

                    if (message.Length >= 4)
                    {
                        // Empty.
                    }
                    break;
                case 0x10:
                    descriptionToInsert = "HVAC MESSAGE";

                    if (message.Length >= 4)
                    {
                        // Empty.
                    }
                    break;
                case 0x11:
                    descriptionToInsert = "UNKNOWN FEATURE PRESENT";

                    if (message.Length >= 4)
                    {
                        // Empty.
                    }
                    break;
                case 0x12:
                    descriptionToInsert = "REQUEST EEPROM READ - COMPASS MINI-TRIP";

                    if (message.Length >= 6)
                    {
                        // Empty.
                    }
                    break;
                case 0x16:
                    descriptionToInsert = "VEHICLE THEFT SECURITY STATE";

                    if (message.Length >= 3)
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
                    }
                    break;
                case 0x1B:
                    descriptionToInsert = "LAST OS TEMPERATURE";

                    if (message.Length >= 4)
                    {
                        // Empty.
                    }
                    break;
                case 0x1C:
                    descriptionToInsert = "FUEL LEVEL COUNTS";

                    if (message.Length >= 4)
                    {
                        // Empty.
                    }
                    break;
                case 0x23:
                    descriptionToInsert = "COUNTRY CODE";

                    if (message.Length >= 4)
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
                    }
                    break;
                case 0x24:
                    descriptionToInsert = "VEHICLE SPEED";

                    if (message.Length >= 4)
                    {
                        if (Properties.Settings.Default.Units == "imperial")
                        {
                            valueToInsert = payload[0].ToString("0");
                            unitToInsert = "MPH";
                        }
                        else if (Properties.Settings.Default.Units == "metric")
                        {
                            valueToInsert = payload[1].ToString("0");
                            unitToInsert = "KM/H";
                        }
                    }
                    break;
                case 0x25:
                    descriptionToInsert = "FUEL LEVEL";

                    if (message.Length >= 3)
                    {
                        valueToInsert = Math.Round(payload[0] * 0.3922, 1).ToString("0.0").Replace(",", ".");
                        unitToInsert = "PERCENT";
                    }
                    break;
                case 0x29:
                    descriptionToInsert = "LAST ENGINE SHUTDOWN";

                    if (message.Length >= 4)
                    {
                        if (payload[0] < 10) valueToInsert = "0";
                        valueToInsert += payload[0].ToString("0") + ":";
                        if (payload[1] < 10) valueToInsert += "0";
                        valueToInsert += payload[1].ToString("0");
                        unitToInsert = "HOUR:MINUTE";
                    }
                    break;
                case 0x2A:
                    descriptionToInsert = "UNKNOWN FEATURE PRESENT";

                    if (message.Length >= 4)
                    {
                        // Empty.
                    }
                    break;
                case 0x2C:
                    descriptionToInsert = "WIPER";

                    if (message.Length >= 3)
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
                    }
                    break;
                case 0x34:
                    descriptionToInsert = "BCM TO MIC MESSAGE";

                    if (message.Length >= 4)
                    {
                        // Empty.
                    }
                    break;
                case 0x35:
                    descriptionToInsert = "US/METRIC STATUS | SEAT-BELT:";

                    if (message.Length >= 4)
                    {
                        if (Util.IsBitSet(payload[0], 1)) valueToInsert = "METRIC";
                        else valueToInsert = "US";

                        if (Util.IsBitSet(payload[0], 2)) descriptionToInsert = "US/METRIC STATUS | SEATBELT: BUCKLED";
                        else descriptionToInsert = "US/METRIC STATUS | SEATBELT: UNBUCKLED";
                    }
                    break;
                case 0x36:
                    descriptionToInsert = "UNKNOWN FEATURE PRESENT";

                    if (message.Length >= 4)
                    {
                        // Empty.
                    }
                    break;
                case 0x3A:
                    descriptionToInsert = "INSTRUMENT CLUSTER LAMP STATES";

                    if (message.Length >= 4)
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
                    }
                    break;
                case 0x3B:
                    descriptionToInsert = "SEND COMPENSATION AND CHECKSUM DATA";

                    if (message.Length >= 4)
                    {
                        // Empty.
                    }
                    break;
                case 0x42:
                    descriptionToInsert = "THROTTLE POSITION SENSOR | CRUISE SET SPEED";

                    if (message.Length >= 4)
                    {
                        if (Properties.Settings.Default.Units == "imperial")
                        {
                            valueToInsert = Math.Round(payload[0] * 0.65).ToString("0") + " | " + payload[1].ToString("0");
                            unitToInsert = "% | MPH";
                        }
                        else if (Properties.Settings.Default.Units == "metric")
                        {
                            valueToInsert = Math.Round(payload[0] * 0.65).ToString("0") + " | " + (payload[1] * 1.609344).ToString("0");
                            unitToInsert = "% | KM/H";
                        }
                    }
                    break;
                case 0x44:
                    descriptionToInsert = "FUEL USED";

                    if (message.Length >= 4)
                    {
                        valueToInsert = ((payload[0] << 8) + payload[1]).ToString("0");
                    }
                    break;
                case 0x46:
                    descriptionToInsert = "REQUEST CALIBRATION DATA";

                    if (message.Length >= 4)
                    {
                        // Empty.
                    }
                    break;
                case 0x4B:
                    descriptionToInsert = "N/S AND E/W A/D";

                    if (message.Length >= 4)
                    {
                        // Empty.
                    }
                    break;
                case 0x4D:
                    descriptionToInsert = "UNKNOWN FEATURE PRESENT";

                    if (message.Length >= 4)
                    {
                        // Empty.
                    }
                    break;
                case 0x50:
                    descriptionToInsert = "MIC LAMP STATE (AIRBAG | SEATBELT)";

                    if (message.Length >= 3)
                    {
                        List<string> lampList = new List<string>();
                        lampList.Clear();

                        if (payload[0] == 0)
                        {
                            valueToInsert = string.Empty;
                        }
                        else
                        {
                            if (Util.IsBitSet(payload[0], 0)) lampList.Add("AIRBAG");
                            if (Util.IsBitSet(payload[0], 2)) lampList.Add("SEATBELT");
                            valueToInsert = string.Empty;

                            foreach (string s in lampList)
                            {
                                valueToInsert += s + " | ";
                            }

                            if (valueToInsert.Length > 2) valueToInsert = valueToInsert.Remove(valueToInsert.Length - 3); // remove last "|" character
                        }
                    }
                    break;
                case 0x52:
                    descriptionToInsert = "TRANSMISSION STATUS / SELECTED GEAR";

                    if (message.Length >= 4)
                    {
                        switch (payload[0])
                        {
                            case 0x10:
                                valueToInsert = "1ST";
                                break;
                            case 0x20:
                                valueToInsert = "2ND";
                                break;
                            case 0x30:
                                valueToInsert = "3RD";
                                break;
                            case 0x40:
                                valueToInsert = "4TH";
                                break;
                            default:
                                valueToInsert = "UNDEFINED";
                                break;
                        }
                    }
                    break;
                case 0x54:
                    descriptionToInsert = "BAROMETRIC PRESSURE | TEMPERATURE";

                    if (message.Length >= 4)
                    {
                        if (Properties.Settings.Default.Units == "imperial")
                        {
                            valueToInsert = Math.Round(payload[0] * 0.1217 * 0.49109778, 1).ToString("0.0").Replace(",", ".") + " | " + Math.Round((payload[1] * 1.8) - 198.4).ToString("0");
                            unitToInsert = "PSI | °F";
                        }
                        else if (Properties.Settings.Default.Units == "metric")
                        {
                            valueToInsert = Math.Round(payload[0] * 0.1217 * 25.4 * 0.133322368, 1).ToString("0.0").Replace(",", ".") + " | " + (payload[1] - 128).ToString("0");
                            unitToInsert = "KPA | °C";
                        }
                    }
                    break;
                case 0x56:
                    descriptionToInsert = "REQUESTED MIL STATE - TRANSMISSION";

                    if (message.Length >= 4)
                    {
                        // Empty.
                    }
                    break;
                case 0x6B:
                    descriptionToInsert = "COMPASS COMP. AND CHECKSUM DATA RECEIVED";

                    if (message.Length >= 4)
                    {
                        // Empty.
                    }
                    break;
                case 0x6D:
                    descriptionToInsert = "VEHICLE IDENTIFICATION NUMBER (VIN) CHARACTER";

                    if (message.Length >= 4)
                    {
                        if ((payload[0] > 0) && (payload[0] < 18) && (payload[1] >= 0x30) && (payload[1] <= 0x5A))
                        {
                            // Replace characters in the VIN string (one by one)
                            VIN = VIN.Remove(payload[0] - 1, 1).Insert(payload[0] - 1, ((char)(payload[1])).ToString());
                        }
                        valueToInsert = VIN;
                    }
                    break;
                case 0x75:
                    descriptionToInsert = "A/C HIGH SIDE PRESSURE";

                    if (message.Length >= 4)
                    {
                        if (Properties.Settings.Default.Units == "imperial")
                        {
                            valueToInsert = Math.Round(payload[0] * 1.961, 1).ToString("0.0").Replace(",", ".");
                            unitToInsert = "PSI";
                        }
                        else if (Properties.Settings.Default.Units == "metric")
                        {
                            valueToInsert = Math.Round(payload[0] * 1.961 * 6.894757, 1).ToString("0.0").Replace(",", ".");
                            unitToInsert = "KPA";
                        }
                    }
                    break;
                case 0x76:
                    descriptionToInsert = "UNKNOWN FEATURE PRESENT";

                    if (message.Length >= 4)
                    {
                        // Empty.
                    }
                    break;
                case 0x7B:
                case 0x83:
                    descriptionToInsert = "OUTSIDE AIR TEMPERATURE";

                    if (message.Length >= 4)
                    {
                        if (Properties.Settings.Default.Units == "imperial") // default unit for this message
                        {
                            valueToInsert = Math.Round(payload[0] - 70.0).ToString("0");
                            unitToInsert = "°F";
                        }
                        else if (Properties.Settings.Default.Units == "metric") // manual conversion
                        {
                            valueToInsert = Math.Round(((payload[0] - 70.0) - 32.0) / 1.8).ToString("0");
                            unitToInsert = "°C";
                        }
                    }
                    break;
                case 0x7C:
                    descriptionToInsert = "TRANSMISSION TEMPERATURE";

                    if (message.Length >= 4)
                    {
                        double TemperatureF = Math.Round(1.8 * payload[0] - 83.2);
                        double TemperatureC = payload[0] - 64.0;
                        
                        if (Properties.Settings.Default.Units == "imperial")
                        {
                            valueToInsert = TemperatureF.ToString("0");
                            unitToInsert = "°F";
                        }
                        else if (Properties.Settings.Default.Units == "metric")
                        {
                            valueToInsert = TemperatureC.ToString("0");
                            unitToInsert = "°C";
                        }
                    }
                    break;
                case 0x7E:
                    descriptionToInsert = "A/C CLUTCH RELAY STATE";

                    if (message.Length >= 3)
                    {
                        if (Util.IsBitSet(payload[0], 0)) valueToInsert = "ON";
                        else valueToInsert = "OFF";
                    }
                    break;
                case 0x84:
                    descriptionToInsert = "PCM TO BCM MESSAGE | MILEAGE INCREMENT";

                    if (message.Length >= 4)
                    {
                        if (Properties.Settings.Default.Units == "imperial")
                        {
                            valueToInsert = Util.ByteToHexStringSimple(new byte[1] { payload[0] }) + " | " + Math.Round(payload[1] * 0.000125, 6).ToString("0.000000").Replace(",", ".");
                            unitToInsert = "N/A | MI";
                        }
                        else if (Properties.Settings.Default.Units == "metric")
                        {
                            valueToInsert = Util.ByteToHexStringSimple(new byte[1] { payload[0] }) + " | " + Math.Round(payload[1] * 0.000125 * 1.609344, 6).ToString("0.000000").Replace(",", ".");
                            unitToInsert = "N/A | KM";
                        }
                    }
                    break;
                case 0x89:
                    descriptionToInsert = "FUEL EFFICIENCY";

                    if (message.Length >= 4)
                    {
                        valueToInsert = Util.ByteToHexStringSimple(new byte[1] { payload[0] }) + " MPG | " + payload[1].ToString("0") + " L/100KM";
                    }
                    break;
                case 0x8C:
                    descriptionToInsert = "ENGINE COOLANT TEMPERATURE | AMB/BAT TEMPERATURE";

                    if (message.Length >= 4)
                    {
                        if (Properties.Settings.Default.Units == "imperial")
                        {
                            valueToInsert = Math.Round((payload[0] * 1.8) - 198.4).ToString("0") + " | " + Math.Round((payload[1] * 1.8) - 198.4).ToString("0");
                            unitToInsert = "°F | °F";
                        }
                        else if (Properties.Settings.Default.Units == "metric")
                        {
                            valueToInsert = (payload[0] - 128).ToString("0") + " | " + (payload[1] - 128).ToString("0");
                            unitToInsert = "°C | °C";
                        }
                    }
                    break;
                case 0x8D:
                    descriptionToInsert = "UNKNOWN FEATURE PRESENT";

                    if (message.Length >= 4)
                    {
                        // Empty.
                    }
                    break;
                case 0x8E:
                    descriptionToInsert = "STATUS 21";

                    if (message.Length >= 4)
                    {
                        // Empty.
                    }
                    break;
                case 0x93:
                    descriptionToInsert = "SEND CALIBRATION AND VARIANCE DATA";

                    if (message.Length >= 4)
                    {
                        // Empty.
                    }
                    break;
                case 0x94:
                    descriptionToInsert = "MIC GAUGE/LAMP STATE";

                    if (message.Length >= 4)
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
                            case 0x07:
                            case 0x27:
                            case 0x37:
                                descriptionToInsert = "MIC GAUGE POSITION: TACHOMETER";
                                valueToInsert = Util.ByteToHexString(payload, 0, 1);
                                break;
                            default:
                                descriptionToInsert = "MIC GAUGE/LAMP STATE";
                                valueToInsert = Convert.ToString(payload[0], 2).PadLeft(8, '0');
                                break;
                        }
                    }
                    break;
                case 0x95:
                    descriptionToInsert = "FUEL LEVEL SENSOR VOLTAGE | FUEL LEVEL";

                    if (message.Length >= 4)
                    {
                        double FuelLevelSensorVolts = payload[0] * 0.0191;
                        double FuelLevelG = payload[1] * 0.125;
                        double FuelLevelL = payload[1] * 0.125 * 3.785412;

                        if (Properties.Settings.Default.Units == "imperial")
                        {
                            valueToInsert = Math.Round(FuelLevelSensorVolts, 3).ToString("0.000").Replace(",", ".") + " | " + Math.Round(FuelLevelG, 1).ToString("0.0").Replace(",", ".");
                            unitToInsert = "V | GALLON";
                        }
                        else if (Properties.Settings.Default.Units == "metric")
                        {
                            valueToInsert = Math.Round(FuelLevelSensorVolts, 3).ToString("0.000").Replace(",", ".") + " | " + Math.Round(FuelLevelL, 1).ToString("0.0").Replace(",", ".");
                            unitToInsert = "V | LITER";
                        }
                    }
                    break;
                case 0x99:
                    descriptionToInsert = "COMPASS CALIBRATION AND VARIANCE DATA RECEIVED";

                    if (message.Length >= 4)
                    {
                        // Empty.
                    }
                    break;
                case 0xA4:
                    descriptionToInsert = "MIC LAMP STATE";

                    if (message.Length >= 4)
                    {
                        valueToInsert = string.Empty;

                        if (Util.IsBitSet(payload[0], 1)) valueToInsert += "CRUISE ";
                        if (Util.IsBitSet(payload[0], 2)) valueToInsert += "BRAKE ";
                        if (Util.IsBitSet(payload[0], 5)) valueToInsert += "MIL ";
                    }
                    break;
                case 0xA9:
                    descriptionToInsert = "LAST ENGINE SHUTDOWN";

                    if (message.Length >= 3)
                    {
                        valueToInsert = payload[0].ToString("0");
                        unitToInsert = "MINUTE";
                    }
                    break;
                case 0xAA:
                    descriptionToInsert = "VEHICLE THEFT SECURITY STATE";

                    if (message.Length >= 4)
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
                    }
                    break;
                case 0xAC:
                    descriptionToInsert = "VEHICLE INFORMATION";

                    if (message.Length >= 4)
                    {
                        // Empty.
                    }
                    break;
                case 0xB1:
                    descriptionToInsert = "WARNING";

                    if (message.Length >= 3)
                    {
                        List<string> warningList = new List<string>();
                        warningList.Clear();

                        if (payload[0] == 0)
                        {
                            descriptionToInsert = "NO WARNING";
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

                            if (descriptionToInsert.Length > 2) descriptionToInsert = descriptionToInsert.Remove(descriptionToInsert.Length - 3); // remove last "|" character
                        }
                    }
                    break;
                case 0xB2:
                    descriptionToInsert = "REQUEST  |";

                    if (message.Length >= 6)
                    {
                        switch (payload[0]) // module address
                        {
                            case 0x10: // VIC - Vehicle Info Center
                                switch (payload[1])
                                {
                                    case 0x00: // reset
                                        descriptionToInsert = "REQUEST  | VEHICLE INFO CENTER | RESET";
                                        break;
                                    case 0x10: // actuator tests
                                        descriptionToInsert = "REQUEST  | VIC | ACTUATOR TEST";

                                        switch (payload[2])
                                        {
                                            case 0x10:
                                                valueToInsert = "DISPLAY";
                                                break;
                                        }

                                        break;
                                    case 0x12: // read digital parameters
                                        descriptionToInsert = "REQUEST  | VIC | DIGITAL READ";

                                        switch (payload[2])
                                        {
                                            case 0x00:
                                                valueToInsert = "TRANSFER CASE POSITION";
                                                break;
                                        }

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
                                        }

                                        break;
                                    case 0x16: // read fault codes
                                        descriptionToInsert = "REQUEST  | VIC | FAULT CODES";
                                        valueToInsert = "PAGE: " + Util.ByteToHexString(payload, 2, 1);
                                        break;
                                    case 0x24: // software version
                                        descriptionToInsert = "REQUEST  | VIC | SOFTWARE VERSION";
                                        break;
                                    case 0x40: // erase fault codes
                                        descriptionToInsert = "REQUEST  | VIC | ERASE FAULT CODES";
                                        valueToInsert = "PAGE: " + Util.ByteToHexString(payload, 2, 1);
                                        break;
                                    default:
                                        descriptionToInsert = "REQUEST  | VIC";
                                        break;
                                }
                                break;
                            case 0x18: // VTS - Vehicle Theft Security
                            case 0x1B:
                                descriptionToInsert = "REQUEST  | VTS";
                                break;
                            case 0x19: // CMT - Compass Mini-Trip
                                switch (payload[1]) // command
                                {
                                    case 0x00: // reset
                                        descriptionToInsert = "REQUEST  | COMPASS MINI-TRIP | RESET";
                                        break;
                                    case 0x10: // actuator test
                                        descriptionToInsert = "REQUEST  | CMT | ACTUATOR TEST";

                                        switch (payload[2])
                                        {
                                            case 0x00: // self test
                                                valueToInsert = "SELF TEST";
                                                break;
                                        }

                                        break;
                                    case 0x11: // test status
                                        descriptionToInsert = "REQUEST  | CMT | ACTUATOR TEST STATUS";

                                        switch (payload[2])
                                        {
                                            case 0x00: // self test
                                                valueToInsert = "SELF TEST";
                                                break;
                                        }

                                        break;
                                    case 0x12: // read digital parameters
                                        descriptionToInsert = "REQUEST  | CMT | DIGITAL READ";

                                        switch (payload[2])
                                        {
                                            case 0x00:
                                                valueToInsert = "STEP SWITCH";
                                                break;
                                        }

                                        break;
                                    case 0x16: // read fault codes
                                        descriptionToInsert = "REQUEST  | CMT | FAULT CODES";
                                        valueToInsert = "PAGE: " + Util.ByteToHexString(payload, 2, 1);
                                        break;
                                    case 0x20: // diagnostic data
                                        descriptionToInsert = "REQUEST  | CMT | DIAGNOSTIC DATA";

                                        switch (payload[2])
                                        {
                                            case 0x00:
                                                valueToInsert = "TEMPERATURE";
                                                break;
                                        }

                                        break;
                                    case 0x22: // read ROM
                                        switch (payload[2])
                                        {
                                            case 0xC2: // MIC and PCM messages received
                                                descriptionToInsert = "REQUEST  | CMT | MIC AND PCM MESSAGES RECEIVED";
                                                break;
                                            default:
                                                descriptionToInsert = "REQUEST  | CMT | ROM DATA";
                                                break;
                                        }
                                        break;
                                    case 0x24: // software version
                                        switch (payload[2])
                                        {
                                            case 0x00:
                                                descriptionToInsert = "REQUEST  | CMT | SOFTWARE VERSION";
                                                break;
                                            case 0x01:
                                                descriptionToInsert = "REQUEST  | CMT | EEPROM VERSION";
                                                break;
                                        }
                                        break;
                                    case 0x40: // erase fault codes
                                        descriptionToInsert = "REQUEST  | CMT | ERASE FAULT CODES";
                                        valueToInsert = "PAGE: " + Util.ByteToHexString(payload, 2, 1);
                                        break;
                                    default:
                                        descriptionToInsert = "REQUEST  | CMT";
                                        break;
                                }
                                break;
                            case 0x1E: // ACM - Airbag Control Module
                                switch (payload[1]) // command
                                {
                                    case 0x00: // reset
                                        descriptionToInsert = "REQUEST  | AIRBAG CONTROL MODULE | RESET";
                                        break;
                                    case 0x16: // read fault codes
                                        descriptionToInsert = "REQUEST  | ACM | FAULT CODES";
                                        valueToInsert = "PAGE: " + Util.ByteToHexString(payload, 2, 1);
                                        break;
                                    case 0x24: // software version
                                        descriptionToInsert = "REQUEST  | ACM | SOFTWARE VERSION";
                                        break;
                                    case 0x40: // erase fault codes
                                        descriptionToInsert = "REQUEST  | ACM | ERASE FAULT CODES";
                                        valueToInsert = "PAGE: " + Util.ByteToHexString(payload, 2, 1);
                                        break;
                                    default:
                                        descriptionToInsert = "REQUEST  | ACM";
                                        break;
                                }
                                break;
                            case 0x20: // BCM - Body Control Module | BCM JA PREM VTSS; sc: Body; 0x1012
                                switch (payload[1]) // command
                                {
                                    case 0x00: // reset
                                        descriptionToInsert = "REQUEST  | BODY CONTROL MODULE | RESET";
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

                                        break;
                                    case 0x12: // read digital parameters
                                        descriptionToInsert = "REQUEST  | BCM | DIGITAL READ";
                                        valueToInsert = Util.ByteToHexString(payload, 2, 1);
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
                                        break;
                                    case 0x16: // read fault codes
                                        descriptionToInsert = "REQUEST  | BCM | FAULT CODES";
                                        valueToInsert = "PAGE: " + Util.ByteToHexString(payload, 2, 1);
                                        break;
                                    case 0x22: // read ROM
                                        descriptionToInsert = "REQUEST  | BCM | ROM DATA";
                                        int address = (payload[2] << 8) + payload[3];
                                        int addressNext = address + 1;
                                        byte[] addressNextArray = { (byte)((addressNext >> 8) & 0xFF), (byte)(addressNext & 0xFF) };
                                        valueToInsert = "OFFSET: " + Util.ByteToHexString(message, 3, 2) + " | " + Util.ByteToHexStringSimple(addressNextArray);
                                        break;
                                    case 0x24: // read module id
                                        descriptionToInsert = "REQUEST  | BCM | MODULE ID";
                                        break;
                                    case 0x2A: // read vehicle identification number (vin)
                                        descriptionToInsert = "REQUEST  | BCM | READ VIN";
                                        break;
                                    case 0x2C: // write vehicle identification number (vin)
                                        descriptionToInsert = "REQUEST  | BCM | WRITE VIN";
                                        break;
                                    case 0x40: // erase fault codes
                                        descriptionToInsert = "REQUEST  | BCM | ERASE FAULT CODES";
                                        break;
                                    case 0x60: // write EEPROM
                                        descriptionToInsert = "REQUEST  | BCM | WRITE EEPROM | OFFSET: " + Util.ByteToHexStringSimple(new byte[2] { payload[2], payload[3] });
                                        break;
                                    case 0xB0: // write settings
                                        descriptionToInsert = "REQUEST  | BCM | WRITE SETTINGS";
                                        break;
                                    case 0xB1: // read settings
                                        descriptionToInsert = "REQUEST  | BCM | READ SETTINGS";
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

                                        break;
                                    case 0x12: // read digital parameters
                                        descriptionToInsert = "REQUEST  | MIC | DIGITAL READ";

                                        switch (payload[2])
                                        {
                                            case 0x00:
                                                valueToInsert = "ALL SWITCHES";
                                                break;
                                        }

                                        break;
                                    case 0x16: // read fault codes
                                        descriptionToInsert = "REQUEST  | MIC | FAULT CODES";
                                        valueToInsert = "PAGE: " + Util.ByteToHexString(payload, 2, 1);
                                        break;
                                    case 0x24: // software version
                                        descriptionToInsert = "REQUEST  | MIC | SOFTWARE VERSION";
                                        break;
                                    case 0x40: // erase fault codes
                                        descriptionToInsert = "REQUEST  | MIC | ERASE FAULT CODES";
                                        valueToInsert = "PAGE: " + Util.ByteToHexString(payload, 2, 1);
                                        break;
                                    case 0xE0: // self test
                                        descriptionToInsert = "REQUEST  | MIC | SELF TEST";
                                        break;
                                    default:
                                        descriptionToInsert = "REQUEST  | MIC";
                                        break;
                                }
                                break;
                            case 0x41: // TCM - Transmission Control Module
                            case 0x42:
                                switch (payload[1])
                                {
                                    case 0x00: // reset
                                        descriptionToInsert = "REQUEST  | TRANSMISSION CONTROL MODULE | RESET";
                                        break;
                                    case 0x24: // read analog parameters
                                        descriptionToInsert = "REQUEST  | TCM | READ ANALOG PARAMETER";

                                        switch (payload[2])
                                        {
                                            case 0x0B: // LR Clutch Volume Index (CVI)
                                                transmissionLRCVIRequested = true;
                                                descriptionToInsert = "REQUEST  | TCM | LR CLUTCH VOLUME INDEX (CVI)";
                                                break;
                                            case 0x0C: // 24 CVI
                                                transmission24CVIRequested = true;
                                                descriptionToInsert = "REQUEST  | TCM | 24 CLUTCH VOLUME INDEX (CVI)";
                                                break;
                                            case 0x0D: // OD CVI
                                                transmissionODCVIRequested = true;
                                                descriptionToInsert = "REQUEST  | TCM | OD CLUTCH VOLUME INDEX (CVI)";
                                                break;
                                            case 0x0E: // UD CVI
                                                transmissionUDCVIRequested = true;
                                                descriptionToInsert = "REQUEST  | TCM | UD CLUTCH VOLUME INDEX (CVI)";
                                                break;
                                            case 0x10: // transmission temperature
                                                transmissionTemperatureRequested = true;
                                                descriptionToInsert = "REQUEST  | TCM | TRANSMISSION TEMPERATURE";
                                                break;
                                        }

                                        break;
                                    default:
                                        descriptionToInsert = "REQUEST  | TCM";
                                        break;
                                }
                                break;
                            case 0x43: // ABS - Antilock Brake System
                                switch (payload[1])
                                {
                                    case 0x00: // reset
                                        descriptionToInsert = "REQUEST  | ANTILOCK BRAKE SYSTEM | RESET";
                                        break;
                                    default:
                                        descriptionToInsert = "REQUEST  | ABS";
                                        break;
                                }
                                break;
                            case 0x50: // HVAC - Heat Vent Air Conditioning
                                switch (payload[1])
                                {
                                    case 0x00: // reset
                                        descriptionToInsert = "REQUEST  | HVAC | RESET";
                                        break;
                                    default:
                                        descriptionToInsert = "REQUEST  | HVAC";
                                        break;
                                }
                                break;
                            case 0x80: // DDM - Driver Door Module
                                switch (payload[1])
                                {
                                    case 0x00: // reset
                                        descriptionToInsert = "REQUEST  | DRIVER DOOR MODULE | RESET";
                                        break;
                                    default:
                                        descriptionToInsert = "REQUEST  | DDM";
                                        break;
                                }
                                break;
                            case 0x81: // PDM - Passenger Door Module
                                switch (payload[1])
                                {
                                    case 0x00: // reset
                                        descriptionToInsert = "REQUEST  | PASSENGER DOOR MODULE | RESET";
                                        break;
                                    default:
                                        descriptionToInsert = "REQUEST  | PDM";
                                        break;
                                }
                                break;
                            case 0x82: // MSM - Memory Seat Module
                                switch (payload[1])
                                {
                                    case 0x00: // reset
                                        descriptionToInsert = "REQUEST  | MEMORY SEAT MODULE | RESET";
                                        break;
                                    default:
                                        descriptionToInsert = "REQUEST  | MSM";
                                        break;
                                }
                                break;
                            case 0x96: // ASM - Audio System Module
                                switch (payload[1])
                                {
                                    case 0x00: // reset
                                        descriptionToInsert = "REQUEST  | AUDIO SYSTEM MODULE | RESET";
                                        break;
                                    default:
                                        descriptionToInsert = "REQUEST  | ASM";
                                        break;
                                }
                                break;
                            case 0xC0: // SKIM - Sentry Key Immobilizer Module
                                switch (payload[1])
                                {
                                    case 0x00: // reset
                                        descriptionToInsert = "REQUEST  | SKIM | RESET";
                                        break;
                                    default:
                                        descriptionToInsert = "REQUEST  | SKIM";
                                        break;
                                }
                                break;
                            case 0xFF: // ALL modules present on the CCD-bus
                                switch (payload[1]) // command
                                {
                                    case 0x00: // reset
                                        descriptionToInsert = "REQUEST  | RESET ALL CCD-BUS MODULES";
                                        break;
                                    default:
                                        descriptionToInsert = "REQUEST  | ALL CCD-BUS MODULES";
                                        break;
                                }
                                break;
                            default:
                                descriptionToInsert = "REQUEST  | MODULE: " + Util.ByteToHexStringSimple(new byte[1] { payload[0] }) + " | COMMAND: " + Util.ByteToHexStringSimple(new byte[1] { payload[1] }) + " | PARAMS: " + Util.ByteToHexStringSimple(new byte[2] { payload[2], payload[3] });
                                break;
                        }
                    }

                    if (Properties.Settings.Default.Timestamp == true)
                    {
                        TimeSpan ElapsedTime = TimeSpan.FromMilliseconds(timestamp[0] << 24 | timestamp[1] << 16 | timestamp[2] << 8 | timestamp[3]);
                        DateTime Timestamp = DateTime.Today.Add(ElapsedTime);
                        string TimestampString = Timestamp.ToString("HH:mm:ss.fff") + " ";
                        File.AppendAllText(MainForm.CCDB2F2LogFilename, TimestampString); // no newline is appended!
                    }

                    File.AppendAllText(MainForm.CCDB2F2LogFilename, Util.ByteToHexStringSimple(message) + Environment.NewLine);
                    break;
                case 0xB4:
                case 0xC4:
                    descriptionToInsert = "VEHICLE SPEED SENSOR";

                    if (message.Length >= 4)
                    {
                        if (Properties.Settings.Default.Units == "imperial")
                        {
                            if ((payload[0] != 0xFF) && (payload[1] != 0xFF))
                            {
                                valueToInsert = Math.Round(28800.0 / ((payload[0] << 8) | payload[1]), 1).ToString("0.0").Replace(",", ".");
                            }
                            else
                            {
                                valueToInsert = "0.0";
                            }

                            unitToInsert = "MPH";
                        }
                        else if (Properties.Settings.Default.Units == "metric")
                        {
                            if ((payload[0] != 0xFF) && (payload[1] != 0xFF))
                            {
                                valueToInsert = Math.Round(28800.0 / ((payload[0] << 8) | payload[1]) * 1.609344, 1).ToString("0.0").Replace(",", ".");
                            }
                            else
                            {
                                valueToInsert = "0.0";
                            }

                            unitToInsert = "KM/H";
                        }
                    }
                    break;
                case 0xB6:
                    descriptionToInsert = "UNKNOWN FEATURE PRESENT";

                    if (message.Length >= 4)
                    {
                        // Empty.
                    }
                    break;
                case 0xBA:
                    descriptionToInsert = "REQUEST COMPASS CALIBRATION OR VARIANCE";

                    if (message.Length >= 4)
                    {
                        // Empty.
                    }
                    break;
                case 0xBE:
                    descriptionToInsert = "IGNITION SWITCH POSITION";

                    if (message.Length >= 3)
                    {
                        if (Util.IsBitSet(payload[0], 4)) valueToInsert = "ON";
                        else valueToInsert = "OFF";
                    }
                    break;
                case 0xC2:
                    descriptionToInsert = "SKIM SECRET KEY";

                    if (message.Length >= 6)
                    {
                        valueToInsert = Util.ByteToHexStringSimple(payload);
                    }
                    break;
                case 0xCA:
                    descriptionToInsert = "WRITE EEPROM";

                    if (message.Length >= 6)
                    {
                        switch (payload[0])
                        {
                            case 0x1B: // VTS
                                descriptionToInsert = "WRITE EEPROM | VTS | ";
                                break;
                            case 0x20: // BCM
                                descriptionToInsert = "WRITE EEPROM | BCM | ";
                                break;
                            case 0x43: // ABS
                                descriptionToInsert = "WRITE EEPROM | ABS | ";
                                break;
                            default:
                                descriptionToInsert = "WRITE EEPROM | MODULE ID: " + Util.ByteToHexStringSimple(new byte[1] { payload[0] }) + " | ";
                                break;
                        }

                        descriptionToInsert += "OFFSET: " + Util.ByteToHexStringSimple(new byte[2] { payload[1], payload[2] });
                        valueToInsert = Util.ByteToHexStringSimple(new byte[1] { payload[3] });
                    }
                    break;
                case 0xCB:
                    descriptionToInsert = "SEND COMPASS AND LAST OUTSIDE AIR TEMPERATURE DATA";

                    if (message.Length >= 4)
                    {
                        // Empty.
                    }
                    break;
                case 0xCC:
                    descriptionToInsert = "ACCUMULATED MILEAGE";

                    if (message.Length >= 4)
                    {
                        valueToInsert = Util.ByteToHexStringSimple(payload);
                    }
                    break;
                case 0xCD:
                    descriptionToInsert = "UNKNOWN FEATURE PRESENT";

                    if (message.Length >= 4)
                    {
                        // Empty.
                    }
                    break;
                case 0xCE:
                    descriptionToInsert = "VEHICLE DISTANCE / ODOMETER";

                    if (message.Length >= 6)
                    {
                        if (Properties.Settings.Default.Units == "imperial")
                        {
                            valueToInsert = Math.Round((uint)(payload[0] << 24 | payload[1] << 16 | payload[2] << 8 | payload[3]) * 0.000125, 3).ToString("0.000").Replace(",", ".");
                            unitToInsert = "MILE";
                        }
                        else if (Properties.Settings.Default.Units == "metric")
                        {
                            valueToInsert = Math.Round((uint)(payload[0] << 24 | payload[1] << 16 | payload[2] << 8 | payload[3]) * 0.000125 * 1.609344, 3).ToString("0.000").Replace(",", ".");
                            unitToInsert = "KILOMETER";
                        }
                    }
                    break;
                case 0xD3:
                    descriptionToInsert = "COMPASS DISPLAY";

                    if (message.Length >= 4)
                    {
                        // Empty.
                    }
                    break;
                case 0xD4:
                    descriptionToInsert = "BATTERY VOLTAGE | CHARGING VOLTAGE";

                    if (message.Length >= 4)
                    {
                        valueToInsert = Math.Round(payload[0] * 0.0625, 1).ToString("0.0").Replace(",", ".") + " | " + Math.Round(payload[1] * 0.0625, 1).ToString("0.0").Replace(",", ".");
                        unitToInsert = "V | V";
                    }
                    break;
                case 0xDA:
                    descriptionToInsert = "MIC SWITCH/LAMP STATE";

                    if (message.Length >= 3)
                    {
                        if (Util.IsBitSet(payload[0], 6)) valueToInsert = "MIL LAMP ON";
                        else valueToInsert = "MIL LAMP OFF";
                    }
                    break;
                case 0xDB:
                    descriptionToInsert = "COMPASS CALL DATA | A/C CLUTCH ON";

                    if (message.Length >= 4)
                    {
                        // Empty.
                    }
                    break;
                case 0xDC:
                    descriptionToInsert = "TRANSMISSION STATUS / SELECTED GEAR";

                    if (message.Length >= 3)
                    {
                        valueToInsert = string.Empty;

                        if (Util.IsBitSet(payload[0], 0)) valueToInsert += "NEUTRAL ";
                        else if (Util.IsBitSet(payload[0], 1)) valueToInsert += "REVERSE ";
                        else if (Util.IsBitSet(payload[0], 2)) valueToInsert += "1 ";
                        else if (Util.IsBitSet(payload[0], 3)) valueToInsert += "2 ";
                        else if (Util.IsBitSet(payload[0], 4)) valueToInsert += "3 ";
                        else if (Util.IsBitSet(payload[0], 5)) valueToInsert += "4 ";
                        else valueToInsert += "N/A ";

                        switch ((payload[0] >> 6) & 0x03)
                        {
                            case 1:
                                valueToInsert += "| LOCK: PART";
                                break;
                            case 2:
                                valueToInsert += "| LOCK: FULL";
                                break;
                            default:
                                valueToInsert += "| LOCK: N/A";
                                break;
                        }
                    }
                    break;
                case 0xE4:
                    descriptionToInsert = "ENGINE SPEED | INTAKE MANIFOLD ABSOLUTE PRESSURE";

                    if (message.Length >= 4)
                    {
                        if (Properties.Settings.Default.Units == "imperial")
                        {
                            valueToInsert = (payload[0] * 32).ToString("0") + " | " + Math.Round(payload[1] * 0.1217 * 0.49109778, 1).ToString("0.0").Replace(",", ".");
                            unitToInsert = "RPM | PSI";
                        }
                        else if (Properties.Settings.Default.Units == "metric")
                        {
                            valueToInsert = (payload[0] * 32).ToString("0") + " | " + Math.Round(payload[1] * 0.1217 * 25.4 * 0.133322368, 1).ToString("0.0").Replace(",", ".");
                            unitToInsert = "RPM | KPA";
                        }
                    }
                    break;
                case 0xEC:
                    descriptionToInsert = "VEHICLE INFORMATION";

                    if (message.Length >= 4)
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

                            if (descriptionToInsert.Length > 2) descriptionToInsert = descriptionToInsert.Remove(descriptionToInsert.Length - 3); // remove last "|" character
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
                    }
                    break;
                case 0xEE:
                    descriptionToInsert = "TRIP DISTANCE / TRIPMETER";

                    if (message.Length >= 5)
                    {
                        if (Properties.Settings.Default.Units == "imperial")
                        {
                            valueToInsert = Math.Round((uint)(payload[0] << 16 | payload[1] << 8 | payload[2]) * 0.016, 3).ToString("0.000").Replace(",", ".");
                            unitToInsert = "MILE";
                        }
                        else if (Properties.Settings.Default.Units == "metric")
                        {
                            valueToInsert = Math.Round((uint)(payload[0] << 16 | payload[1] << 8 | payload[2]) * 0.016 * 1.609344, 3).ToString("0.000").Replace(",", ".");
                            unitToInsert = "KILOMETER";
                        }
                    }
                    break;
                case 0xF1:
                    descriptionToInsert = "WARNING";

                    if (message.Length >= 3)
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

                            if (descriptionToInsert.Length > 2) descriptionToInsert = descriptionToInsert.Remove(descriptionToInsert.Length - 3); // remove last "|" character
                            unitToInsert = string.Empty;
                        }
                    }
                    break;
                case 0xF2:
                    descriptionToInsert = "RESPONSE |";

                    if (message.Length >= 6)
                    {
                        switch (payload[0]) // module address
                        {
                            case 0x10: // VIC - Vehicle Info Center
                                switch (payload[1])
                                {
                                    case 0x00: // reset
                                        descriptionToInsert = "RESPONSE | VEHICLE INFO CENTER | RESET COMPLETE";
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

                                                    if (valueToInsert.Length > 2) valueToInsert = valueToInsert.Remove(valueToInsert.Length - 3); // remove last "|" character
                                                    valueToInsert += " LAMP";
                                                }

                                                break;
                                            default:
                                                descriptionToInsert = "RESPONSE | VIC | DIGITAL READ";
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
                                        break;
                                    case 0x24: // software version
                                        descriptionToInsert = "RESPONSE | VIC | SOFTWARE VERSION";
                                        valueToInsert = Util.ByteToHexString(payload, 2, 2);
                                        break;
                                    case 0x40: // erase fault codes
                                        descriptionToInsert = "RESPONSE | VIC | ERASE FAULT CODES";
                                        if (payload[3] == 0x00) valueToInsert = "ERASED";
                                        else valueToInsert = "FAILED";
                                        break;
                                    case 0xFF: // command error
                                        descriptionToInsert = "RESPONSE | VIC | COMMAND ERROR";
                                        break;
                                    default:
                                        descriptionToInsert = "RESPONSE | VIC";
                                        break;
                                }
                                break;
                            case 0x18: // VTS - Vehicle Theft Security
                            case 0x1B:
                                descriptionToInsert = "RESPONSE | VTS";
                                break;
                            case 0x19: // CMT - Compass Mini-Trip
                                switch (payload[1]) // command
                                {
                                    case 0x00: // reset
                                        descriptionToInsert = "RESPONSE | COMPASS MINI-TRIP | RESET COMPLETE";
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
                                                break;
                                        }

                                        break;
                                    case 0x11: // test status
                                        descriptionToInsert = "RESPONSE | CMT | ACTUATOR TEST STATUS";

                                        switch (payload[2])
                                        {
                                            case 0x00: // self test
                                                if (payload[3] == 0x01) valueToInsert = "RUNNING";
                                                else valueToInsert = "DENIED";
                                                break;
                                        }

                                        break;
                                    case 0x12: // read digital parameters
                                        switch (payload[2])
                                        {
                                            case 0x00:
                                                descriptionToInsert = "RESPONSE | CMT | STEP SWITCH";

                                                if (Util.IsBitSet(payload[3], 4)) valueToInsert = "PRESSED";
                                                else valueToInsert = "RELEASED";

                                                break;
                                            default:
                                                descriptionToInsert = "RESPONSE | CMT | DIGITAL READ";
                                                break;
                                        }
                                        break;
                                    case 0x16: // read fault codes
                                        descriptionToInsert = "RESPONSE | CMT | FAULT CODES";

                                        if (payload[3] == 0x00) valueToInsert = "NO FAULT CODE";
                                        else valueToInsert = "CODE: " + Util.ByteToHexString(payload, 3, 1);

                                        break;
                                    case 0x20: // diagnostic data
                                        switch (payload[2])
                                        {
                                            case 0x00:
                                                descriptionToInsert = "RESPONSE | CMT | TEMPERATURE";

                                                string temperature = string.Empty;

                                                if (Properties.Settings.Default.Units == "imperial")
                                                {
                                                    temperature = (payload[3] - 40).ToString("0");
                                                    unitToInsert = "°F";
                                                }
                                                else if (Properties.Settings.Default.Units == "metric")
                                                {
                                                    temperature = Math.Round(((payload[3] - 40) * 0.555556) - 17.77778).ToString("0");
                                                    unitToInsert = "°C";
                                                }

                                                valueToInsert = temperature;
                                                break;
                                            default:
                                                descriptionToInsert = "RESPONSE | CMT | DIAGNOSTIC DATA";
                                                valueToInsert = Util.ByteToHexString(payload, 3, 1);
                                                break;

                                        }
                                        break;
                                    case 0x22: // read ROM
                                        descriptionToInsert = "RESPONSE | CMT | ROM DATA";
                                        valueToInsert = Util.ByteToHexString(payload, 2, 2);
                                        break;
                                    case 0x24: // software version
                                        switch (payload[2])
                                        {
                                            case 0x00: // software version
                                                descriptionToInsert = "RESPONSE | CMT | SOFTWARE VERSION";
                                                valueToInsert = Util.ByteToHexString(payload, 2, 2);
                                                break;
                                            case 0x01: // EEPROM version
                                                descriptionToInsert = "RESPONSE | CMT | EEPROM VERSION";
                                                valueToInsert = Util.ByteToHexString(payload, 2, 2);
                                                break;
                                            default:
                                                descriptionToInsert = "RESPONSE | CMT";
                                                valueToInsert = Util.ByteToHexString(payload, 2, 2);
                                                break;
                                        }
                                        break;
                                    case 0x40: // erase fault codes
                                        descriptionToInsert = "RESPONSE | CMT | ERASE FAULT CODES";
                                        if (payload[3] == 0x00) valueToInsert = "ERASED";
                                        else valueToInsert = "FAILED";
                                        break;
                                    case 0xFF: // command error
                                        descriptionToInsert = "RESPONSE | CMT | COMMAND ERROR";
                                        break;
                                    default:
                                        descriptionToInsert = "RESPONSE | CMT";
                                        break;
                                }
                                break;
                            case 0x1E: // ACM - Airbag Control Module
                                switch (payload[1])
                                {
                                    case 0x00: // reset
                                        descriptionToInsert = "RESPONSE | AIRBAG CONTROL MODULE | RESET COMPLETE";
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
                                        break;
                                    default:
                                        descriptionToInsert = "RESPONSE | ACM";
                                        break;
                                }

                                unitToInsert = string.Empty;
                                break;
                            case 0x20: // BCM - Body Control Module
                                switch (payload[1]) // command
                                {
                                    case 0x00: // reset
                                        descriptionToInsert = "RESPONSE | BODY CONTROL MODULE | RESET COMPLETE";
                                        break;
                                    case 0x10: // actuator tests
                                        descriptionToInsert = "RESPONSE | BCM | ACTUATOR TEST";
                                        valueToInsert = "RUNNING";
                                        break;
                                    case 0x12: // read digital parameters
                                        descriptionToInsert = "RESPONSE | BCM | DIGITAL READ";
                                        valueToInsert = Util.ByteToHexString(payload, 2, 2);
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
                                        break;
                                    case 0x22: // read ROM
                                        descriptionToInsert = "RESPONSE | BCM | ROM DATA";
                                        valueToInsert = "VALUE:     " + Util.ByteToHexString(payload, 2, 1) + " |    " + Util.ByteToHexString(payload, 3, 1);
                                        break;
                                    case 0x24: // read module id
                                        descriptionToInsert = "RESPONSE | BCM | MODULE ID";
                                        valueToInsert = Util.ByteToHexString(payload, 2, 2);
                                        break;
                                    case 0x2A: // read vehicle identification number (vin)
                                        descriptionToInsert = "RESPONSE | BCM | READ VIN";
                                        valueToInsert = Util.ByteToHexString(payload, 2, 2);
                                        break;
                                    case 0x2C: // write vehicle identification number (vin)
                                        descriptionToInsert = "RESPONSE | BCM | WRITE VIN";
                                        valueToInsert = Util.ByteToHexString(payload, 2, 2);
                                        break;
                                    case 0x40: // erase fault codes
                                        descriptionToInsert = "RESPONSE | BCM | ERASE FAULT CODES";
                                        if (payload[3] == 0x00) valueToInsert = "ERASED";
                                        else valueToInsert = "FAILED";
                                        break;
                                    case 0x60: // write EEPROM
                                        descriptionToInsert = "RESPONSE | BCM | WRITE EEPROM OFFSET";
                                        valueToInsert = Util.ByteToHexStringSimple(new byte[2] { payload[2], payload[3] });
                                        unitToInsert = "OK";
                                        break;
                                    case 0xB0: // write settings
                                        descriptionToInsert = "RESPONSE | BCM | WRITE SETTINGS";
                                        valueToInsert = Util.ByteToHexString(payload, 2, 2);
                                        break;
                                    case 0xB1: // read settings
                                        descriptionToInsert = "RESPONSE | BCM | READ SETTINGS";
                                        valueToInsert = Util.ByteToHexString(payload, 2, 2);
                                        break;
                                    case 0xFF: // command error
                                        descriptionToInsert = "RESPONSE | BCM | COMMAND ERROR";
                                        break;
                                    default:
                                        descriptionToInsert = "RESPONSE | BCM";
                                        valueToInsert = Util.ByteToHexString(payload, 2, 2);
                                        break;
                                }
                                break;
                            case 0x22: // MIC - Mechanical Instrument Cluster
                            case 0x60:
                                switch (payload[1]) // command
                                {
                                    case 0x00: // reset
                                        descriptionToInsert = "RESPONSE | MIC | RESET COMPLETE";
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
                                        break;
                                    case 0x12: // read digital parameters
                                        descriptionToInsert = "RESPONSE | MIC | DIGITAL READ";
                                        valueToInsert = Util.ByteToHexString(payload, 3, 1);
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

                                            if (valueToInsert.Length > 2) valueToInsert = valueToInsert.Remove(valueToInsert.Length - 3); // remove last "|" character
                                        }

                                        break;
                                    case 0x24: // software version
                                        descriptionToInsert = "RESPONSE | MIC | SOFTWARE VERSION";
                                        valueToInsert = Util.ByteToHexString(payload, 2, 2);
                                        break;
                                    case 0x40: // erase fault codes
                                        descriptionToInsert = "RESPONSE | MIC | ERASE FAULT CODES";
                                        if (payload[3] == 0x00) valueToInsert = "ERASED";
                                        else valueToInsert = "FAILED";
                                        break;
                                    case 0xE0: // self test
                                        descriptionToInsert = "RESPONSE | MIC | SELF TEST";
                                        break;
                                    case 0xFF: // command error
                                        descriptionToInsert = "RESPONSE | MIC | COMMAND ERROR";
                                        break;
                                    default:
                                        descriptionToInsert = "RESPONSE | MIC";
                                        break;
                                }
                                break;
                            case 0x41: // TCM - Transmission Control Module
                            case 0x42:
                                switch (payload[1]) // command
                                {
                                    case 0x00: // reset
                                        descriptionToInsert = "RESPONSE | TCM | RESET COMPLETE";
                                        break;
                                    case 0x24: // read analog parameter
                                        descriptionToInsert = "RESPONSE | TCM | READ ANALOG PARAMETER";

                                        if (transmissionLRCVIRequested)
                                        {
                                            transmissionLRCVIRequested = false;
                                            descriptionToInsert = "RESPONSE | TCM | LR CLUTCH VOLUME INDEX (CVI)";

                                            if (payload[2] != 0xFF)
                                            {
                                                if (Properties.Settings.Default.Units == "imperial")
                                                {
                                                    valueToInsert = payload[2].ToString("0") + " = " + Math.Round(payload[2] / 64.0, 3).ToString("0.000").Replace(",", ".");
                                                    unitToInsert = "IN^3";
                                                }
                                                else if (Properties.Settings.Default.Units == "metric")
                                                {
                                                    valueToInsert = payload[2].ToString("0") + " = " + Math.Round(payload[2] / 64.0 * 16.387064, 3).ToString("0.000").Replace(",", ".");
                                                    unitToInsert = "CM^3";
                                                }
                                            }
                                            else
                                            {
                                                valueToInsert = "ERROR";
                                            }
                                        }

                                        if (transmission24CVIRequested)
                                        {
                                            transmission24CVIRequested = false;
                                            descriptionToInsert = "RESPONSE | TCM | 24 CLUTCH VOLUME INDEX (CVI)";

                                            if (payload[2] != 0xFF)
                                            {
                                                if (Properties.Settings.Default.Units == "imperial")
                                                {
                                                    valueToInsert = payload[2].ToString("0") + " = " + Math.Round(payload[2] / 64.0, 3).ToString("0.000").Replace(",", ".");
                                                    unitToInsert = "IN^3";
                                                }
                                                else if (Properties.Settings.Default.Units == "metric")
                                                {
                                                    valueToInsert = payload[2].ToString("0") + " = " + Math.Round(payload[2] / 64.0 * 16.387064, 3).ToString("0.000").Replace(",", ".");
                                                    unitToInsert = "CM^3";
                                                }
                                            }
                                            else
                                            {
                                                valueToInsert = "ERROR";
                                            }
                                        }

                                        if (transmissionODCVIRequested)
                                        {
                                            transmissionODCVIRequested = false;
                                            descriptionToInsert = "RESPONSE | TCM | OD CLUTCH VOLUME INDEX (CVI)";

                                            if (payload[2] != 0xFF)
                                            {
                                                if (Properties.Settings.Default.Units == "imperial")
                                                {
                                                    valueToInsert = payload[2].ToString("0") + " = " + Math.Round(payload[2] / 64.0, 3).ToString("0.000").Replace(",", ".");
                                                    unitToInsert = "IN^3";
                                                }
                                                else if (Properties.Settings.Default.Units == "metric")
                                                {
                                                    valueToInsert = payload[2].ToString("0") + " = " + Math.Round(payload[2] / 64.0 * 16.387064, 3).ToString("0.000").Replace(",", ".");
                                                    unitToInsert = "CM^3";
                                                }
                                            }
                                            else
                                            {
                                                valueToInsert = "ERROR";
                                            }
                                        }

                                        if (transmissionUDCVIRequested)
                                        {
                                            transmissionUDCVIRequested = false;
                                            descriptionToInsert = "RESPONSE | TCM | UD CLUTCH VOLUME INDEX (CVI)";

                                            if (payload[2] != 0xFF)
                                            {
                                                if (Properties.Settings.Default.Units == "imperial")
                                                {
                                                    valueToInsert = payload[2].ToString("0") + " = " + Math.Round(payload[2] / 64.0, 3).ToString("0.000").Replace(",", ".");
                                                    unitToInsert = "IN^3";
                                                }
                                                else if (Properties.Settings.Default.Units == "metric")
                                                {
                                                    valueToInsert = payload[2].ToString("0") + " = " + Math.Round(payload[2] / 64.0 * 16.387064, 3).ToString("0.000").Replace(",", ".");
                                                    unitToInsert = "CM^3";
                                                }
                                            }
                                            else
                                            {
                                                valueToInsert = "ERROR";
                                            }
                                        }

                                        if (transmissionTemperatureRequested)
                                        {
                                            transmissionTemperatureRequested = false;
                                            descriptionToInsert = "RESPONSE | TCM | TRANSMISSION TEMPERATURE";

                                            if (payload[2] != 0xFF)
                                            {
                                                if (Properties.Settings.Default.Units == "imperial")
                                                {
                                                    valueToInsert = Math.Round(((payload[2] << 8) + payload[3]) * 0.0156, 1).ToString("0.0").Replace(",", ".");
                                                    unitToInsert = "°F";
                                                }
                                                else if (Properties.Settings.Default.Units == "metric")
                                                {
                                                    valueToInsert = Math.Round((((payload[2] << 8) + payload[3]) * 0.0156 * 0.555556) - 17.77778, 1).ToString("0.0").Replace(",", ".");
                                                    unitToInsert = "°C";
                                                }
                                            }
                                            else
                                            {
                                                valueToInsert = "ERROR";
                                            }
                                        }

                                        break;
                                    case 0xFF: // command error
                                        descriptionToInsert = "RESPONSE | TCM | COMMAND ERROR";
                                        break;
                                    default:
                                        descriptionToInsert = "RESPONSE | TCM";
                                        break;
                                }
                                break;
                            case 0x43: // ABS - Antilock Brake System
                                switch (payload[1]) // command
                                {
                                    case 0x00: // reset
                                        descriptionToInsert = "RESPONSE | ANTILOCK BRAKE SYSTEM | RESET COMPLETE";
                                        break;
                                    case 0xFF: // command error
                                        descriptionToInsert = "RESPONSE | ABS | COMMAND ERROR";
                                        break;
                                    default:
                                        descriptionToInsert = "RESPONSE | ABS";
                                        break;
                                }
                                break;
                            case 0x50: // HVAC - Heat Vent Air Conditioning
                                switch (payload[1]) // command
                                {
                                    case 0x00: // reset
                                        descriptionToInsert = "RESPONSE | HVAC | RESET COMPLETE";
                                        break;
                                    case 0xFF: // command error
                                        descriptionToInsert = "RESPONSE | HVAC | COMMAND ERROR";
                                        break;
                                    default:
                                        descriptionToInsert = "RESPONSE | HVAC";
                                        break;
                                }
                                break;
                            case 0x80: // DDM - Driver Door Module
                                switch (payload[1]) // command
                                {
                                    case 0x00: // reset
                                        descriptionToInsert = "RESPONSE | DRIVER DOOR MODULE | RESET COMPLETE";
                                        break;
                                    case 0xFF: // command error
                                        descriptionToInsert = "RESPONSE | DDM | COMMAND ERROR";
                                        break;
                                    default:
                                        descriptionToInsert = "RESPONSE | DDM";
                                        break;
                                }
                                break;
                            case 0x81: // PDM - Passenger Door Module
                                switch (payload[1]) // command
                                {
                                    case 0x00: // reset
                                        descriptionToInsert = "RESPONSE | PASSENGER DOOR MODULE | RESET COMPLETE";
                                        break;
                                    case 0xFF: // command error
                                        descriptionToInsert = "RESPONSE | PDM | COMMAND ERROR";
                                        break;
                                    default:
                                        descriptionToInsert = "RESPONSE | PDM";
                                        break;
                                }
                                break;
                            case 0x82: // MSM - Memory Seat Module
                                switch (payload[1]) // command
                                {
                                    case 0x00: // reset
                                        descriptionToInsert = "RESPONSE | MEMORY SEAT MODULE | RESET COMPLETE";
                                        break;
                                    case 0xFF: // command error
                                        descriptionToInsert = "RESPONSE | MSM | COMMAND ERROR";
                                        break;
                                    default:
                                        descriptionToInsert = "RESPONSE | MSM";
                                        break;
                                }
                                break;
                            case 0x96: // ASM - Audio System Module
                                switch (payload[1]) // command
                                {
                                    case 0x00: // reset
                                        descriptionToInsert = "RESPONSE | AUDIO SYSTEM MODULE | RESET COMPLETE";
                                        break;
                                    case 0xFF: // command error
                                        descriptionToInsert = "RESPONSE | ASM | COMMAND ERROR";
                                        break;
                                    default:
                                        descriptionToInsert = "RESPONSE | ASM";
                                        break;
                                }
                                break;
                            case 0xC0: // SKIM - Sentry Key Immobilizer Module
                                switch (payload[1]) // command
                                {
                                    case 0x00: // reset
                                        descriptionToInsert = "RESPONSE | SKIM | RESET COMPLETE";
                                        break;
                                    case 0xFF: // command error
                                        descriptionToInsert = "RESPONSE | SKIM | COMMAND ERROR";
                                        break;
                                    default:
                                        descriptionToInsert = "RESPONSE | SKIM";
                                        break;
                                }
                                break;
                            default:
                                descriptionToInsert = "RESPONSE | MODULE: " + Util.ByteToHexStringSimple(new byte[1] { payload[0] }) + " | COMMAND: " + Util.ByteToHexStringSimple(new byte[1] { payload[1] }) + " | PARAMS: " + Util.ByteToHexStringSimple(new byte[2] { payload[2], payload[3] });
 
                                break;
                        }
                    }

                    if (Properties.Settings.Default.Timestamp == true)
                    {
                        TimeSpan ElapsedTime = TimeSpan.FromMilliseconds(timestamp[0] << 24 | timestamp[1] << 16 | timestamp[2] << 8 | timestamp[3]);
                        DateTime Timestamp = DateTime.Today.Add(ElapsedTime);
                        string TimestampString = Timestamp.ToString("HH:mm:ss.fff") + " ";
                        File.AppendAllText(MainForm.CCDB2F2LogFilename, TimestampString); // no newline is appended!
                    }

                    File.AppendAllText(MainForm.CCDB2F2LogFilename, Util.ByteToHexStringSimple(message) + Environment.NewLine);
                    break;
                case 0xF3:
                    descriptionToInsert = "SWITCH MESSAGE";

                    if (message.Length >= 4)
                    {
                        // Empty.
                    }
                    break;
                case 0xF5:
                    descriptionToInsert = "ENGINE LAMP CTRL";

                    if (message.Length >= 4)
                    {
                        // Empty.
                    }
                    break;
                case 0xF6:
                    descriptionToInsert = "UNKNOWN FEATURE PRESENT";

                    if (message.Length >= 4)
                    {
                        // Empty.
                    }
                    break;
                case 0xFD:
                    descriptionToInsert = "COMPASS COMP. AND TEMPERATURE DATA RECEIVED";

                    if (message.Length >= 4)
                    {
                        // Empty.
                    }
                    break;
                case 0xFE:
                    descriptionToInsert = "INTERIOR LAMP DIMMING";

                    if (message.Length >= 3)
                    {
                        valueToInsert = Math.Round(payload[0] * 0.392).ToString("0");
                        unitToInsert = "PERCENT";
                    }
                    break;
                case 0xFF:
                    descriptionToInsert = "CCD-BUS WAKE UP";
                    break;
                default:
                    descriptionToInsert = string.Empty;
                    break;
            }

            string hexBytesToInsert;

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

            if (Properties.Settings.Default.Timestamp == true)
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
