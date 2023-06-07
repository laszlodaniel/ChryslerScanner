using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ChryslerScanner.Helpers;

namespace ChryslerScanner
{
    public class CCD
    {
        public CCDDiagnosticsTable Diagnostics = new CCDDiagnosticsTable();

        private const int HexBytesColumnStart = 2;
        private const int DescriptionColumnStart = 28;
        private const int ValueColumnStart = 82;
        private const int UnitColumnStart = 108;
        private string state = string.Empty;
        private string speed = "7812.5 baud";
        private string logic = "non-inverted";

        public string HeaderUnknown  = "│ CCD-BUS (SAE J1567)     │ STATE: N/A                                                                                   ";
        public string HeaderDisabled = "│ CCD-BUS (SAE J1567)     │ STATE: DISABLED                                                                              ";
        public string HeaderEnabled  = "│ CCD-BUS (SAE J1567)     │ STATE: ENABLED @ BAUD | LOGIC: | ID BYTES:                                                   ";
        public string EmptyLine      = "│                         │                                                     │                         │             │";
        public string HeaderModified = string.Empty;

        public bool TransmissionLRCVIRequested = false;
        public bool Transmission24CVIRequested = false;
        public bool TransmissionODCVIRequested = false;
        public bool TransmissionUDCVIRequested = false;
        public bool TransmissionTemperatureRequested = false;

        public string VIN = "-----------------"; // 17 characters
        public string BeaconNote = "------------------"; // 18 characters from 9 CCD messages

        private byte[] SKIMPayload = new byte[5] { 0, 0, 0, 0, 0 };
        private byte[] SKIMPayloadPCM = new byte[5] { 0, 0, 0, 0, 0 };

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

            byte[] timestamp = new byte[4];
            byte[] message = new byte[] { };
            byte[] payload = new byte[] { };

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
                payload = new byte[data.Length - 6];
                Array.Copy(data, 5, payload, 0, payload.Length); // copy payload from the input byte array (without ID and checksum)
            }

            string DescriptionToInsert;
            string ValueToInsert = string.Empty;
            string UnitToInsert = string.Empty;
            byte ID = message[0];

            switch (ID)
            {
                case 0x02:
                {
                    DescriptionToInsert = "SHIFT LEVER POSITION";

                    if (message.Length < 3) break;

                    switch (payload[0])
                    {
                        case 0x01:
                        {
                            ValueToInsert = "PARK";
                            break;
                        }
                        case 0x02:
                        {
                            ValueToInsert = "REVERSE";
                            break;
                        }
                        case 0x03:
                        {
                            ValueToInsert = "NEUTRAL";
                            break;
                        }
                        case 0x05:
                        {
                            ValueToInsert = "DRIVE";
                            break;
                        }
                        case 0x06:
                        {
                            ValueToInsert = "AUTOSTICK";
                            break;
                        }
                        default:
                        {
                            ValueToInsert = "UNDEFINED";
                            break;
                        }
                    }
                    break;
                }
                case 0x07:
                {
                    DescriptionToInsert = "RESTORE SKIM SECRET KEY FROM PCM EEPROM"; // packet sent to SKIM to write to SKIM EEPROM (DRB3 SKIM replaced menu)

                    // SKIM generates same CCD ID if secret key is transferred to PCM EEPROM?

                    if (message.Length < 4) break;
                    if ((payload[0] < 0x10) || (payload[0] > 0x14)) break;

                    SKIMPayloadPCM[payload[0] - 0x10] = payload[1];
                    ValueToInsert = Util.ByteToHexStringSimple(SKIMPayloadPCM);
                    break;
                }
                case 0x0A:
                {
                    DescriptionToInsert = "SEND DIAGNOSTIC FAILURE DATA";

                    if (message.Length < 4) break;

                    // Empty.

                    break;
                }
                case 0x0B:
                {
                    DescriptionToInsert = "SKIM STATUS";

                    if (message.Length < 3) break;

                    List<string> WarningList = new List<string>();

                    if (payload[0] == 0)
                    {
                        ValueToInsert = "NO WARNING";
                        break;
                    }

                    if (Util.IsBitSet(payload[0], 2)) WarningList.Add("-7-");
                    if (Util.IsBitSet(payload[0], 6)) WarningList.Add("-6-");
                    if (Util.IsBitSet(payload[0], 5)) WarningList.Add("-5-");
                    if (Util.IsBitSet(payload[0], 4)) WarningList.Add("-4-");
                    if (Util.IsBitSet(payload[0], 3)) WarningList.Add("-3-");
                    if (Util.IsBitSet(payload[0], 2)) WarningList.Add("-2-");
                    if (Util.IsBitSet(payload[0], 1)) WarningList.Add("FAILURE");
                    if (Util.IsBitSet(payload[0], 0)) WarningList.Add("WARNING");

                    foreach (string s in WarningList)
                    {
                        ValueToInsert += s + " | ";
                    }

                    if (ValueToInsert.Length > 2) ValueToInsert = ValueToInsert.Remove(ValueToInsert.Length - 3); // remove last "|" character

                    break;
                }
                case 0x0C:
                {
                    DescriptionToInsert = "BATTERY | OIL | COOLANT | AMBIENT";

                    if (message.Length < 6) break;

                    double BatteryVoltageA = payload[0] * 0.125;
                    double OilPressurePSI = payload[1] * 0.5;
                    double OilPressureKPA = OilPressurePSI * 6.894757;
                    double CoolantTemperatureC = payload[2] - 64;
                    double CoolantTemperatureF = 1.8 * CoolantTemperatureC + 32.0;
                    double AmbientTemperatureC = payload[3] - 64;
                    double AmbientTemperatureF = 1.8 * AmbientTemperatureC + 32.0;

                    if (Properties.Settings.Default.Units == "imperial")
                    {
                        DescriptionToInsert = "BATTERY: " + Math.Round(BatteryVoltageA, 1).ToString("0.0").Replace(",", ".") + " V | OIL: " + Math.Round(OilPressurePSI, 1).ToString("0.0").Replace(",", ".") + " PSI | COOLANT: " + Math.Round(CoolantTemperatureF, 1).ToString("0") + " °F";
                        ValueToInsert = "AMBIENT: " + Math.Round(AmbientTemperatureF).ToString("0") + " °F";
                    }
                    else if (Properties.Settings.Default.Units == "metric")
                    {
                        DescriptionToInsert = "BATTERY: " + Math.Round(BatteryVoltageA, 1).ToString("0.0").Replace(",", ".") + " V | OIL: " + Math.Round(OilPressureKPA, 1).ToString("0.0").Replace(",", ".") + " KPA | COOLANT: " + CoolantTemperatureC.ToString("0") + " °C";
                        ValueToInsert = "AMBIENT: " + AmbientTemperatureC.ToString("0") + " °C";
                    }
                    break;
                }
                case 0x0D:
                {
                    DescriptionToInsert = "PCM BEACON 0D";

                    if (message.Length < 4) break;

                    BeaconNote = BeaconNote.Remove(0x08, 2).Insert(0x08, ((char)payload[0]).ToString() + ((char)payload[1]).ToString());

                    if (!BeaconNote.Contains("-") && !BeaconNote.Contains("ÿ"))
                    {
                        ValueToInsert = BeaconNote;
                    }
                    else
                    {
                        ValueToInsert = Util.ByteToHexString(payload, 0, 2);
                    }
                    break;
                }
                case 0x10:
                {
                    DescriptionToInsert = "HVAC MESSAGE";

                    if (message.Length < 4) break;

                    // Empty.

                    break;
                }
                case 0x11:
                {
                    DescriptionToInsert = "PCM BEACON 11";

                    if (message.Length < 4) break;

                    BeaconNote = BeaconNote.Remove(0x10, 2).Insert(0x10, ((char)payload[0]).ToString() + ((char)payload[1]).ToString());
                    ValueToInsert = Util.ByteToHexString(payload, 0, 2);
                    break;
                }
                case 0x12:
                {
                    DescriptionToInsert = "REQUEST EEPROM READ - COMPASS MINI-TRIP";

                    if (message.Length < 6) break;

                    // Empty.

                    break;
                }
                case 0x16:
                {
                    DescriptionToInsert = "SKIM SECURITY STATUS";

                    if (message.Length < 3) break;

                    switch (payload[0])
                    {
                        case 0x00:
                        {
                            ValueToInsert = "DISARMED";
                            break;
                        }
                        case 0x01:
                        {
                            ValueToInsert = "TIMING OUT";
                            break;
                        }
                        case 0x02:
                        {
                            ValueToInsert = "ARMED";
                            break;
                        }
                        case 0x04:
                        {
                            ValueToInsert = "HORN AND LIGHTS";
                            break;
                        }
                        case 0x08:
                        {
                            ValueToInsert = "LIGHTS ONLY";
                            break;
                        }
                        case 0x10:
                        {
                            ValueToInsert = "TIMED OUT";
                            break;
                        }
                        case 0x20:
                        {
                            ValueToInsert = "SELF DIAGS";
                            break;
                        }
                        default:
                        {
                            ValueToInsert = "NONE";
                            break;
                        }
                    }
                    break;
                }
                case 0x1B:
                {
                    DescriptionToInsert = "LAST OS TEMPERATURE";

                    if (message.Length < 4) break;

                    // Empty.

                    break;
                }
                case 0x1C:
                {
                    DescriptionToInsert = "FUEL LEVEL COUNTS";

                    if (message.Length < 4) break;

                    // Empty.

                    break;
                }
                case 0x23:
                {
                    DescriptionToInsert = "COUNTRY CODE";

                    if (message.Length < 4) break;

                    switch (payload[0])
                    {
                        case 0x00:
                        {
                            ValueToInsert = "USA";
                            break;
                        }
                        case 0x01:
                        {
                            ValueToInsert = "GULF COAST";
                            break;
                        }
                        case 0x02:
                        {
                            ValueToInsert = "EUROPE";
                            break;
                        }
                        case 0x03:
                        {
                            ValueToInsert = "JAPAN";
                            break;
                        }
                        case 0x04:
                        {
                            ValueToInsert = "MALAYSIA";
                            break;
                        }
                        case 0x05:
                        {
                            ValueToInsert = "INDONESIA";
                            break;
                        }
                        case 0x06:
                        {
                            ValueToInsert = "AUSTRALIA";
                            break;
                        }
                        case 0x07:
                        {
                            ValueToInsert = "ENGLAND";
                            break;
                        }
                        case 0x08:
                        {
                            ValueToInsert = "VENEZUELA";
                            break;
                        }
                        case 0x09:
                        {
                            ValueToInsert = "CANADA";
                            break;
                        }
                        case 0x0A:
                        default:
                        {
                            ValueToInsert = "UNKNOWN";
                            break;
                        }
                    }
                    break;
                }
                case 0x24:
                {
                    DescriptionToInsert = "VEHICLE SPEED";

                    if (message.Length < 4) break;

                    ValueToInsert = payload[0].ToString("0") + " | " + payload[1].ToString("0");
                    UnitToInsert = "MPH | KM/H";
                    break;
                }
                case 0x25:
                {
                    DescriptionToInsert = "FUEL LEVEL";

                    if (message.Length < 3) break;

                    double FuelLevelPercent = payload[0] * 0.3921568627;

                    ValueToInsert = Math.Round(FuelLevelPercent, 1).ToString("0.0").Replace(",", ".");
                    UnitToInsert = "PERCENT";
                    break;
                }
                case 0x29:
                {
                    DescriptionToInsert = "LAST ENGINE SHUTDOWN";

                    if (message.Length < 4) break;

                    uint TimerMinutes = (uint)(payload[0] * 60 + payload[1]);
                    TimeSpan Timestamp29 = TimeSpan.FromMinutes(TimerMinutes);

                    ValueToInsert = Timestamp29.ToString(@"hh\:mm");
                    UnitToInsert = "HH:MM";
                    break;
                }
                case 0x2C:
                {
                    DescriptionToInsert = "WIPER";

                    if (message.Length < 3) break;

                    switch (payload[0])
                    {
                        case 0x04:
                        {
                            ValueToInsert = "WIPERS ON";
                            break;
                        }
                        case 0x08:
                        {
                            ValueToInsert = "WASH ON";
                            break;
                        }
                        case 0x02:
                        {
                            ValueToInsert = "ARMED";
                            break;
                        }
                        case 0x0C:
                        {
                            ValueToInsert = "WIPE / WASH";
                            break;
                        }
                        case 0x20:
                        {
                            ValueToInsert = "INT WIPE";
                            break;
                        }
                        case 0x28:
                        {
                            ValueToInsert = "INT WIPE / WASH";
                            break;
                        }
                        default:
                        {
                            ValueToInsert = "IDLE";
                            break;
                        }
                    }
                    break;
                }
                case 0x34:
                {
                    DescriptionToInsert = "BCM TO MIC MESSAGE";

                    if (message.Length < 4) break;

                    // Empty.

                    break;
                }
                case 0x35:
                {
                    DescriptionToInsert = "US/METRIC STATUS | SEAT-BELT:";

                    if (message.Length < 4) break;

                    if (Util.IsBitSet(payload[0], 1)) ValueToInsert = "METRIC";
                    else ValueToInsert = "US";

                    if (Util.IsBitSet(payload[0], 2)) DescriptionToInsert = "US/METRIC STATUS | SEATBELT: BUCKLED";
                    else DescriptionToInsert = "US/METRIC STATUS | SEATBELT: UNBUCKLED";

                    break;
                }
                case 0x36:
                {
                    DescriptionToInsert = "PCM BEACON 36";

                    if (message.Length < 4) break;

                    BeaconNote = BeaconNote.Remove(0x00, 2).Insert(0x00, ((char)payload[0]).ToString() + ((char)payload[1]).ToString());
                    ValueToInsert = Util.ByteToHexString(payload, 0, 2);
                    break;
                }
                case 0x3A:
                {
                    DescriptionToInsert = "INSTRUMENT CLUSTER LAMP STATES";

                    if (message.Length < 4) break;

                    List<string> LampListA = new List<string>();

                    if (payload[0] != 0)
                    {
                        if (Util.IsBitSet(payload[0], 7)) LampListA.Add("SBT"); // seat belt
                        if (Util.IsBitSet(payload[0], 6)) LampListA.Add("-6-");
                        if (Util.IsBitSet(payload[0], 5)) LampListA.Add("-5-");
                        if (Util.IsBitSet(payload[0], 4)) LampListA.Add("ABG"); // airbag
                        if (Util.IsBitSet(payload[0], 3)) LampListA.Add("ABG");
                        if (Util.IsBitSet(payload[0], 2)) LampListA.Add("-2-");
                        if (Util.IsBitSet(payload[0], 1)) LampListA.Add("-1-");
                        if (Util.IsBitSet(payload[0], 0)) LampListA.Add("-0-");

                        foreach (string s in LampListA)
                        {
                            ValueToInsert += s + " | ";
                        }

                        if (ValueToInsert.Length > 2) ValueToInsert = ValueToInsert.Remove(ValueToInsert.Length - 3); // remove last "|" character
                    }
                    break;
                }
                case 0x3B:
                {
                    DescriptionToInsert = "SEND COMPENSATION AND CHECKSUM DATA";

                    if (message.Length < 4) break;

                    // Empty.

                    break;
                }
                case 0x42:
                {
                    DescriptionToInsert = "DELTA TPS VOLTS | CRUISE SET SPEED";

                    if (message.Length < 4) break;

                    double DeltaTPSVolts = payload[0] * 0.0196; // current volts - minimum volts
                    double CruiseSetSpeedMPH = payload[1] * 0.5;
                    double CruiseSetSpeedKMH = CruiseSetSpeedMPH * 1.609344;

                    if (Properties.Settings.Default.Units == "imperial")
                    {
                        ValueToInsert = Math.Round(DeltaTPSVolts, 3).ToString("0.000").Replace(",", ".") + " | " + Math.Round(CruiseSetSpeedMPH, 1).ToString("0.0").Replace(",", ".");
                        UnitToInsert = "V | MPH";
                    }
                    else if (Properties.Settings.Default.Units == "metric")
                    {
                        ValueToInsert = Math.Round(DeltaTPSVolts, 3).ToString("0.000").Replace(",", ".") + " | " + Math.Round(CruiseSetSpeedKMH, 1).ToString("0.0").Replace(",", ".");
                        UnitToInsert = "V | KM/H";
                    }
                    break;
                }
                case 0x44:
                {
                    DescriptionToInsert = "FUEL USED";

                    if (message.Length < 4) break;

                    ushort FuelUsed = (ushort)((payload[0] << 8) + payload[1]);
                    ValueToInsert = FuelUsed.ToString("0");
                    break;
                }
                case 0x46:
                {
                    DescriptionToInsert = "REQUEST CALIBRATION DATA";

                    if (message.Length < 4) break;

                    // Empty.

                    break;
                }
                case 0x4B:
                {
                    DescriptionToInsert = "N/S AND E/W A/D";

                    if (message.Length < 4) break;

                    // Empty.

                    break;
                }
                case 0x4D:
                {
                    DescriptionToInsert = "PCM BEACON 4D";

                    if (message.Length < 4) break;

                    BeaconNote = BeaconNote.Remove(0x0C, 2).Insert(0x0C, ((char)payload[0]).ToString() + ((char)payload[1]).ToString());
                    ValueToInsert = Util.ByteToHexString(payload, 0, 2);
                    break;
                }
                case 0x50:
                {
                    DescriptionToInsert = "AIRBAG LAMP REQUEST";

                    if (message.Length < 3) break;

                    List<string> LampListB = new List<string>();

                    if (payload[0] != 0)
                    {
                        if (Util.IsBitSet(payload[0], 7)) LampListB.Add("-7-");
                        if (Util.IsBitSet(payload[0], 6)) LampListB.Add("-6-");
                        if (Util.IsBitSet(payload[0], 5)) LampListB.Add("-5-");
                        if (Util.IsBitSet(payload[0], 4)) LampListB.Add("-4-");
                        if (Util.IsBitSet(payload[0], 3)) LampListB.Add("-3-");
                        if (Util.IsBitSet(payload[0], 2)) LampListB.Add("SBT"); // seat belt
                        if (Util.IsBitSet(payload[0], 1)) LampListB.Add("-1-");
                        if (Util.IsBitSet(payload[0], 0)) LampListB.Add("ABG"); // airbag

                        foreach (string s in LampListB)
                        {
                            ValueToInsert += s + " | ";
                        }

                        if (ValueToInsert.Length > 2) ValueToInsert = ValueToInsert.Remove(ValueToInsert.Length - 3); // remove last "|" character
                    }
                    break;
                }
                case 0x52:
                {
                    DescriptionToInsert = "TRANSMISSION GEAR REQUEST";

                    if (message.Length < 4) break;

                    if (Util.IsBitSet(payload[0], 7) && ((payload[0] & 0x70) != 0)) // Autostick equipped
                    {
                        ValueToInsert += "AUTOSTICK | ";
                    }

                    switch (payload[0] & 0x70)
                    {
                        case 0x10:
                        {
                            ValueToInsert += "1ST";
                            break;
                        }
                        case 0x20:
                        {
                            ValueToInsert += "2ND";
                            break;
                        }
                        case 0x30:
                        {
                            ValueToInsert += "3RD";
                            break;
                        }
                        case 0x40:
                        {
                            ValueToInsert += "4TH";
                            break;
                        }
                    }
                    break;
                }
                case 0x54:
                {
                    DescriptionToInsert = "BAROMETRIC PRESSURE | INTAKE AIR TEMPERATURE";

                    if (message.Length < 4) break;

                    if ((payload[0] == 0xFF) && (payload[1] == 0xFF))
                    {
                        ValueToInsert = "MSG NOT USED";
                        break;
                    }

                    double BarometricPressurePSI = payload[0] * 0.1217 * 0.4911542; // 1 inHg = 0.4911542 psi
                    double BarometricPressureKPA = BarometricPressurePSI * 6.894757;
                    double IntakeAirTemperatureC = payload[1] - 128;
                    double IntakeAirTemperatureF = 1.8 * IntakeAirTemperatureC + 32.0;

                    if (Properties.Settings.Default.Units == "imperial")
                    {
                        ValueToInsert = Math.Round(BarometricPressurePSI, 1).ToString("0.0").Replace(",", ".") + " | " + Math.Round(IntakeAirTemperatureF).ToString("0");
                        UnitToInsert = "PSI | °F";
                    }
                    else if (Properties.Settings.Default.Units == "metric")
                    {
                        ValueToInsert = Math.Round(BarometricPressureKPA, 1).ToString("0.0").Replace(",", ".") + " | " + Math.Round(IntakeAirTemperatureC).ToString("0");
                        UnitToInsert = "KPA | °C";
                    }
                    break;
                }
                case 0x56:
                {
                    DescriptionToInsert = "TCM | FAULT CODE PRESENT";

                    if (message.Length < 6) break;

                    if ((payload[1] == 0) && (payload[2] == 0))
                    {
                        DescriptionToInsert = "TCM | NO FAULT CODE";
                        break;
                    }

                    ValueToInsert = "OBD2 P" + Util.ByteToHexString(payload, 1, 2).Replace(" ", "");
                    break;
                }
                case 0x6B:
                {
                    DescriptionToInsert = "COMPASS COMP. AND CHECKSUM DATA RECEIVED";

                    if (message.Length < 4) break;

                    // Empty.

                    break;
                }
                case 0x6C:
                {
                    DescriptionToInsert = "CRUISE STATUS";

                    if (message.Length < 4) break;

                    // Empty.

                    break;
                }
                case 0x6D:
                {
                    DescriptionToInsert = "VEHICLE IDENTIFICATION NUMBER (VIN) CHARACTER";

                    if (message.Length < 4) break;

                    if ((payload[0] > 0) && (payload[0] < 18) && (payload[1] >= 0x30) && (payload[1] <= 0x5A))
                    {
                        // Replace characters in the VIN string (one by one)
                        VIN = VIN.Remove(payload[0] - 1, 1).Insert(payload[0] - 1, ((char)payload[1]).ToString());
                    }

                    ValueToInsert = VIN;
                    break;
                }
                case 0x75:
                {
                    DescriptionToInsert = "A/C HIGH-SIDE PRESSURE | FLEX FUEL ETHANOL PERCENT";

                    if (message.Length < 4) break;

                    double ACHSPressurePSI = payload[0] * 1.961;
                    double ACHSPressureKPA = ACHSPressurePSI * 6.894757;
                    double EthanolPercent = ((((ushort)(payload[1] * 326.0) << 1) & 0xFF00) >> 8) / 2.0;

                    if (Properties.Settings.Default.Units == "imperial")
                    {
                        ValueToInsert = Math.Round(ACHSPressurePSI, 1).ToString("0.0").Replace(",", ".") + " | " + Math.Round(EthanolPercent, 1).ToString("0.0").Replace(",", ".");
                        UnitToInsert = "PSI | %";
                    }
                    else if (Properties.Settings.Default.Units == "metric")
                    {
                        ValueToInsert = Math.Round(ACHSPressureKPA, 1).ToString("0.0").Replace(",", ".") + " | " + Math.Round(EthanolPercent, 1).ToString("0.0").Replace(",", ".");
                        UnitToInsert = "KPA | %";
                    }
                    break;
                }
                case 0x76:
                {
                    DescriptionToInsert = "PCM BEACON 76";

                    if (message.Length < 4) break;

                    BeaconNote = BeaconNote.Remove(0x04, 2).Insert(0x04, ((char)payload[0]).ToString() + ((char)payload[1]).ToString());
                    ValueToInsert = Util.ByteToHexString(payload, 0, 2);
                    break;
                }
                case 0x7B:
                case 0x83:
                {
                    DescriptionToInsert = "OUTSIDE AIR TEMPERATURE";

                    if (message.Length < 4) break;

                    double OutsideAirTemperatureF = payload[0] - 70.0;
                    double OutsideAirTemperatureC = (OutsideAirTemperatureF - 32.0) / 1.8;

                    if (Properties.Settings.Default.Units == "imperial") // default unit for this message
                    {
                        ValueToInsert = Math.Round(OutsideAirTemperatureF).ToString("0");
                        UnitToInsert = "°F";
                    }
                    else if (Properties.Settings.Default.Units == "metric") // manual conversion
                    {
                        ValueToInsert = Math.Round(OutsideAirTemperatureC).ToString("0");
                        UnitToInsert = "°C";
                    }
                    break;
                }
                case 0x7C:
                {
                    DescriptionToInsert = "TRANSMISSION TEMPERATURE";

                    if (message.Length < 4) break;

                    double TransTempF = payload[0] * 4.0;
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
                }
                case 0x7E:
                {
                    DescriptionToInsert = "A/C RELAY STATES";

                    if (message.Length < 3) break;

                    List<string> RelayList = new List<string>();

                    if (payload[0] != 0)
                    {
                        DescriptionToInsert += " | ";

                        if (Util.IsBitSet(payload[0], 7)) RelayList.Add("-7-");
                        if (Util.IsBitSet(payload[0], 6)) RelayList.Add("-6-");
                        if (Util.IsBitSet(payload[0], 5)) RelayList.Add("-5-");
                        if (Util.IsBitSet(payload[0], 4)) RelayList.Add("DEFRST"); // defrost relay
                        if (Util.IsBitSet(payload[0], 3)) RelayList.Add("-3-");
                        if (Util.IsBitSet(payload[0], 2)) RelayList.Add("BLOWER"); // blower fan relay
                        if (Util.IsBitSet(payload[0], 1)) RelayList.Add("-1-");
                        if (Util.IsBitSet(payload[0], 0)) RelayList.Add("CLUTCH"); // clutch relay

                        foreach (string s in RelayList)
                        {
                            DescriptionToInsert += s + " | ";
                        }

                        if (DescriptionToInsert.Length > 2) DescriptionToInsert = DescriptionToInsert.Remove(DescriptionToInsert.Length - 3); // remove last "|" character
                    }

                    ValueToInsert = Convert.ToString(payload[0], 2).PadLeft(8, '0');
                    break;
                }
                case 0x81:
                {
                    DescriptionToInsert = "RADIO CLOCK DISPLAY";

                    if (message.Length < 4) break;

                    ValueToInsert = Util.ByteToHexString(payload, 0, 1) + ":" + Util.ByteToHexString(payload, 1, 1);
                    UnitToInsert = "HH:MM";
                    break;
                }
                case 0x84:
                {
                    DescriptionToInsert = "INJECTOR PULSE WIDTH | MILEAGE INCREMENT";

                    if (message.Length < 4) break;

                    double InjectorPulseWidth = payload[0];
                    double MileageIncrement = payload[1] * 0.000125;
                    double KilometerageIncrement = MileageIncrement * 1.609344;

                    if (Properties.Settings.Default.Units == "imperial")
                    {
                        ValueToInsert = InjectorPulseWidth.ToString("0") + " | " + Math.Round(MileageIncrement, 6).ToString("0.000000").Replace(",", ".");
                        UnitToInsert = "MS | MI";
                    }
                    else if (Properties.Settings.Default.Units == "metric")
                    {
                        ValueToInsert = InjectorPulseWidth.ToString("0") + " | " + Math.Round(KilometerageIncrement, 6).ToString("0.000000").Replace(",", ".");
                        UnitToInsert = "MS | KM";
                    }
                    break;
                }
                case 0x89:
                {
                    DescriptionToInsert = "FUEL EFFICIENCY";

                    if (message.Length < 4) break;

                    ValueToInsert = Util.ByteToHexStringSimple(new byte[1] { payload[0] }) + " MPG | " + payload[1].ToString("0") + " L/100KM";
                    break;
                }
                case 0x8C:
                {
                    DescriptionToInsert = "ENGINE COOLANT TEMPERATURE | AMBIENT TEMPERATURE";

                    if (message.Length < 4) break;

                    double ECTC = payload[0] - 128;
                    double ECTF = 1.8 * ECTC + 32.0;
                    double ATSC = payload[1] - 128;
                    double ATSF = 1.8 * ATSC + 32.0;

                    if (Properties.Settings.Default.Units == "imperial")
                    {
                        ValueToInsert = Math.Round(ECTF).ToString("0") + " | " + Math.Round(ATSF).ToString("0");
                        UnitToInsert = "°F | °F";
                    }
                    else if (Properties.Settings.Default.Units == "metric")
                    {
                        ValueToInsert = ECTC.ToString("0") + " | " + ATSC.ToString("0");
                        UnitToInsert = "°C | °C";
                    }
                    break;
                }
                case 0x8D:
                {
                    DescriptionToInsert = "PCM BEACON 8D";

                    if (message.Length < 4) break;

                    BeaconNote = BeaconNote.Remove(0x0A, 2).Insert(0x0A, ((char)payload[0]).ToString() + ((char)payload[1]).ToString());
                    ValueToInsert = Util.ByteToHexString(payload, 0, 2);
                    break;
                }
                case 0x8E:
                {
                    DescriptionToInsert = "STATUS 21";

                    if (message.Length < 4) break;

                    // Empty.

                    break;
                }
                case 0x91:
                {
                    DescriptionToInsert = "UPDATE BEACON MESSAGE PAYLOAD IN PCM EEPROM";

                    if (message.Length < 4) break;

                    ValueToInsert = Util.ByteToHexString(payload, 0, 2);
                    break;
                }
                case 0x93:
                {
                    DescriptionToInsert = "SEND CALIBRATION AND VARIANCE DATA";

                    if (message.Length < 4) break;

                    // Empty.

                    break;
                }
                case 0x94:
                {
                    DescriptionToInsert = "MIC GAUGE/LAMP STATE";

                    if (message.Length < 4) break;

                    switch (payload[1])
                    {
                        case 0x00: // fuel gauge
                        {
                            DescriptionToInsert = "MIC GAUGE POSITION | FUEL LEVEL";
                            ValueToInsert = Util.ByteToHexString(payload, 0, 1);
                            break;
                        }
                        case 0x01: // coolant temperature gauge
                        {
                            DescriptionToInsert = "MIC GAUGE POSITION | COOLANT TEMPERATURE";
                            ValueToInsert = Util.ByteToHexString(payload, 0, 1);
                            break;
                        }
                        case 0x02: // speedometer
                        case 0x22:
                        case 0x32:
                        {
                            DescriptionToInsert = "MIC GAUGE POSITION | SPEEDOMETER";
                            ValueToInsert = Util.ByteToHexString(payload, 0, 1);
                            break;
                        }
                        case 0x03: // tachometer
                        case 0x23:
                        case 0x33:
                        case 0x07:
                        case 0x27:
                        case 0x37:
                        {
                            DescriptionToInsert = "MIC GAUGE POSITION | TACHOMETER";
                            ValueToInsert = Util.ByteToHexString(payload, 0, 1);
                            break;
                        }
                        default:
                        {
                            ValueToInsert = Convert.ToString(payload[0], 2).PadLeft(8, '0');
                            break;
                        }
                    }
                    break;
                }
                case 0x95:
                {
                    DescriptionToInsert = "FUEL LEVEL SENSOR VOLTAGE | FUEL LEVEL";

                    if (message.Length < 4) break;

                    double FuelLevelSensorVoltage = payload[0] * 0.0196;
                    double FuelLevelG = payload[1] * 0.125;
                    double FuelLevelL = FuelLevelG * 3.785412;

                    if (Properties.Settings.Default.Units == "imperial")
                    {
                        ValueToInsert = Math.Round(FuelLevelSensorVoltage, 3).ToString("0.000").Replace(",", ".") + " | " + Math.Round(FuelLevelG, 1).ToString("0.0").Replace(",", ".");
                        UnitToInsert = "V | GALLON";
                    }
                    else if (Properties.Settings.Default.Units == "metric")
                    {
                        ValueToInsert = Math.Round(FuelLevelSensorVoltage, 3).ToString("0.000").Replace(",", ".") + " | " + Math.Round(FuelLevelL, 1).ToString("0.0").Replace(",", ".");
                        UnitToInsert = "V | LITER";
                    }
                    break;
                }
                case 0x99:
                {
                    DescriptionToInsert = "COMPASS CALIBRATION AND VARIANCE DATA RECEIVED";

                    if (message.Length < 4) break;

                    // Empty.

                    break;
                }
                case 0xA4:
                {
                    DescriptionToInsert = "STATUS: ";

                    if (message.Length < 4) break;

                    List<string> Status = new List<string>();

                    if ((payload[0] == 0) && (payload[1] == 0))
                    {
                        DescriptionToInsert += "ATX";
                        break;
                    }

                    if (Util.IsBitSet(payload[0], 7)) Status.Add("MTX"); // manual transmission
                    else Status.Add("ATX"); // automatic transmission

                    if (Util.IsBitSet(payload[0], 6)) Status.Add("-6-");
                    if (Util.IsBitSet(payload[0], 5)) Status.Add("CEL"); // chech engine lamp
                    if (Util.IsBitSet(payload[0], 4)) Status.Add("-4-");
                    if (Util.IsBitSet(payload[0], 3)) Status.Add("ACT"); // A/C clutch
                    if (Util.IsBitSet(payload[0], 2)) Status.Add("BPP"); // brake pedal pressed
                    if (Util.IsBitSet(payload[0], 1)) Status.Add("TPP"); // throttle pedal pressed
                    if (Util.IsBitSet(payload[0], 0)) Status.Add("CCE"); // cruise control engaged

                    if (Util.IsBitSet(payload[1], 7)) Status.Add("-7-");
                    if (Util.IsBitSet(payload[1], 6)) Status.Add("-6-");
                    if (Util.IsBitSet(payload[1], 5)) Status.Add("-5-");
                    if (Util.IsBitSet(payload[1], 4)) Status.Add("TFR"); // transmission fan relay
                    if (Util.IsBitSet(payload[1], 3)) Status.Add("-3-");
                    if (Util.IsBitSet(payload[1], 2)) Status.Add("CCL"); // cruise control lamp

                    if (Util.IsBitSet(payload[1], 1) && Util.IsBitSet(payload[1], 0)) Status.Add("TMR"); // torque management response
                    else
                    {
                        if (Util.IsBitSet(payload[1], 1)) Status.Add("TM1"); // torque management response 1
                        if (Util.IsBitSet(payload[1], 0)) Status.Add("TM0"); // torque management response 0
                    }

                    foreach (string s in Status)
                    {
                        DescriptionToInsert += s + " | ";
                    }

                    if (DescriptionToInsert.Length > 2) DescriptionToInsert = DescriptionToInsert.Remove(DescriptionToInsert.Length - 3); // remove last "|" character
                    break;
                }
                case 0xA5:
                {
                    DescriptionToInsert = "PWM FAN DUTY CYCLE";

                    if (message.Length < 4) break;

                    double PWMFANDutyCycle = payload[0] * 0.3921568627;

                    ValueToInsert = Math.Round(PWMFANDutyCycle, 1).ToString("0.0").Replace(",", ".");
                    UnitToInsert = "PERCENT";
                    break;
                }
                case 0xA6:
                {
                    DescriptionToInsert = "PCM SEED FOR SKIM";

                    if (message.Length < 4) break;

                    ValueToInsert = Util.ByteToHexString(payload, 0, 2);

                    if (VIN.Contains("-")) break;

                    byte[] key = UnlockAlgorithm.GetSKIMUnlockKey(payload, VIN);
                    byte[] UnlockRequest = { 0xC2, 0xC0, key[0], key[1], key[2], 0x00 };

                    UnlockRequest[UnlockRequest.Length - 1] = Util.ChecksumCalculator(UnlockRequest, 0, UnlockRequest.Length - 1);
                    DescriptionToInsert += " | KEY: " + Util.ByteToHexStringSimple(UnlockRequest);
                    break;
                }
                case 0xA9:
                {
                    DescriptionToInsert = "LAST ENGINE SHUTDOWN";

                    if (message.Length < 3) break;

                    TimeSpan TimestampA9 = TimeSpan.FromMinutes(payload[0]);

                    ValueToInsert = TimestampA9.ToString(@"hh\:mm");

                    if (payload[0] == 0xFF) ValueToInsert += "+";

                    UnitToInsert = "HH:MM";
                    break;
                }
                case 0xAA:
                {
                    DescriptionToInsert = "VEHICLE THEFT SECURITY STATUS";

                    if (message.Length < 5) break;

                    //DescriptionToInsert += " | VERSION: " + Util.ByteToHexString(payload, 0, 2);

                    switch (payload[2])
                    {
                        case 0x00:
                        {
                            ValueToInsert = "DISARMED";
                            break;
                        }
                        case 0x01:
                        {
                            ValueToInsert = "TIMING OUT";
                            break;
                        }
                        case 0x02:
                        {
                            ValueToInsert = "ARMED";
                            break;
                        }
                        case 0x04:
                        {
                            ValueToInsert = "HORN AND LIGHTS";
                            break;
                        }
                        case 0x08:
                        {
                            ValueToInsert = "LIGHTS ONLY";
                            break;
                        }
                        case 0x10:
                        {
                            ValueToInsert = "TIMED OUT";
                            break;
                        }
                        case 0x20:
                        {
                            ValueToInsert = "SELF DIAGS";
                            break;
                        }
                        default:
                        {
                            ValueToInsert = "INVALID";
                            break;
                        }
                    }
                    break;
                }
                case 0xAC:
                {
                    DescriptionToInsert = "ENGINE TYPE: ";

                    if (message.Length < 4) break;

                    DescriptionToInsert += (payload[0] >> 4).ToString("0");
                    DescriptionToInsert += " | SIZE: " + (payload[0] & 0x0F).ToString("0");
                    DescriptionToInsert += " | STYLE: " + (payload[1] >> 4).ToString("0");
                    DescriptionToInsert += " | MAKE: " + (payload[1] & 0x07).ToString("0");

                    if (Util.IsBitSet(payload[1], 3)) ValueToInsert = "AWD";
                    break;
                }
                case 0xB1:
                {
                    DescriptionToInsert = "WARNING: ";

                    if (message.Length < 3) break;

                    List<string> WarningListB = new List<string>();

                    if (payload[0] == 0)
                    {
                        DescriptionToInsert = "NO WARNING";
                    }
                    else
                    {
                        if (Util.IsBitSet(payload[0], 7)) WarningListB.Add("-7-");
                        if (Util.IsBitSet(payload[0], 6)) WarningListB.Add("-6-");
                        if (Util.IsBitSet(payload[0], 5)) WarningListB.Add("-5-");
                        if (Util.IsBitSet(payload[0], 4)) WarningListB.Add("OVRSPD"); // overspeed
                        if (Util.IsBitSet(payload[0], 3)) WarningListB.Add("-3-");
                        if (Util.IsBitSet(payload[0], 2)) WarningListB.Add("EXTLMP"); // external lights
                        if (Util.IsBitSet(payload[0], 1)) WarningListB.Add("STBELT"); // seat belt
                        if (Util.IsBitSet(payload[0], 0)) WarningListB.Add("KYNIGN"); // key in ignition

                        foreach (string s in WarningListB)
                        {
                            DescriptionToInsert += s + " | ";
                        }

                        if (DescriptionToInsert.Length > 2) DescriptionToInsert = DescriptionToInsert.Remove(DescriptionToInsert.Length - 3); // remove last "|" character
                    }
                    break;
                }
                case 0xB2:
                {
                    DescriptionToInsert = "REQUEST  |";

                    if (message.Length < 6) break;

                    switch (payload[0]) // module address
                    {
                        case 0x10: // VIC - Vehicle Info Center
                        {
                            switch (payload[1])
                            {
                                case 0x00: // reset
                                {
                                    DescriptionToInsert = "REQUEST  | VEHICLE INFO CENTER | RESET";
                                    break;
                                }
                                case 0x10: // actuator tests
                                {
                                    DescriptionToInsert = "REQUEST  | VIC | ACTUATOR TEST";

                                    switch (payload[2])
                                    {
                                        case 0x10:
                                        {
                                            ValueToInsert = "DISPLAY";
                                            break;
                                        }
                                        default:
                                        {
                                            ValueToInsert = Util.ByteToHexString(payload, 2, 1);
                                            break;
                                        }
                                    }

                                    UnitToInsert = Util.ByteToHexString(payload, 3, 1);
                                    break;
                                }
                                case 0x12: // read digital parameters
                                {
                                    DescriptionToInsert = "REQUEST  | VIC | DIGITAL READ";

                                    switch (payload[2])
                                    {
                                        case 0x00:
                                        {
                                            ValueToInsert = "TRANSFER CASE POSITION";
                                            break;
                                        }
                                        default:
                                        {
                                            ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                            break;
                                        }
                                    }
                                    break;
                                }
                                case 0x14: // read analog parameters
                                {
                                    DescriptionToInsert = "REQUEST  | VIC | ANALOG READ";

                                    switch (payload[2])
                                    {
                                        case 0x00:
                                        {
                                            ValueToInsert = "WASHER LEVEL";
                                            break;
                                        }
                                        case 0x01:
                                        {
                                            ValueToInsert = "COOLANT LEVEL";
                                            break;
                                        }
                                        case 0x02:
                                        {
                                            ValueToInsert = "IGNITION VOLTAGE";
                                            break;
                                        }
                                        default:
                                        {
                                            ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                            break;
                                        }
                                    }
                                    break;
                                }
                                case 0x16: // read fault codes
                                {
                                    DescriptionToInsert = "REQUEST  | VIC | FAULT CODES";
                                    ValueToInsert = "PAGE: " + Util.ByteToHexString(payload, 2, 1);
                                    break;
                                }
                                case 0x24: // software version
                                {
                                    DescriptionToInsert = "REQUEST  | VIC | SOFTWARE VERSION";
                                    break;
                                }
                                case 0x40: // erase fault codes
                                {
                                    DescriptionToInsert = "REQUEST  | VIC | ERASE FAULT CODES";
                                    ValueToInsert = "PAGE: " + Util.ByteToHexString(payload, 2, 1);
                                    break;
                                }
                                default:
                                {
                                    DescriptionToInsert = "REQUEST  | VIC | COMMAND: " + Util.ByteToHexString(payload, 1, 1);
                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                    break;
                                }
                            }
                            break;
                        }
                        case 0x18: // VTS - Vehicle Theft Security
                        case 0x1B:
                        {
                            DescriptionToInsert = "REQUEST  | VTS | COMMAND: " + Util.ByteToHexString(payload, 1, 1);
                            ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                            break;
                        }
                        case 0x19: // CMT - Compass Mini-Trip
                        {
                            switch (payload[1]) // command
                            {
                                case 0x00: // reset
                                {
                                    DescriptionToInsert = "REQUEST  | COMPASS MINI-TRIP | RESET";
                                    break;
                                }
                                case 0x10: // actuator test
                                {
                                    DescriptionToInsert = "REQUEST  | CMT | ACTUATOR TEST";

                                    switch (payload[2])
                                    {
                                        case 0x00: // self test
                                        {
                                            ValueToInsert = "SELF TEST";
                                            break;
                                        }
                                        default:
                                        {
                                            ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                            break;
                                        }
                                    }
                                    break;
                                }
                                case 0x11: // test status
                                {
                                    DescriptionToInsert = "REQUEST  | CMT | ACTUATOR TEST STATUS";

                                    switch (payload[2])
                                    {
                                        case 0x00: // self test
                                        {
                                            ValueToInsert = "SELF TEST";
                                            break;
                                        }
                                        default:
                                        {
                                            ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                            break;
                                        }
                                    }
                                    break;
                                }
                                case 0x12: // read digital parameters
                                {
                                    DescriptionToInsert = "REQUEST  | CMT | DIGITAL READ";

                                    switch (payload[2])
                                    {
                                        case 0x00:
                                        {
                                            ValueToInsert = "STEP SWITCH";
                                            break;
                                        }
                                        default:
                                        {
                                            ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                            break;
                                        }
                                    }
                                    break;
                                }
                                case 0x16: // read fault codes
                                {
                                    DescriptionToInsert = "REQUEST  | CMT | FAULT CODES";
                                    ValueToInsert = "PAGE: " + Util.ByteToHexString(payload, 2, 1);
                                    break;
                                }
                                case 0x20: // diagnostic data
                                {
                                    DescriptionToInsert = "REQUEST  | CMT | DIAGNOSTIC DATA";

                                    switch (payload[2])
                                    {
                                        case 0x00:
                                        {
                                            ValueToInsert = "TEMPERATURE";
                                            break;
                                        }
                                        default:
                                        {
                                            ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                            break;
                                        }
                                    }
                                    break;
                                }
                                case 0x22: // read ROM
                                {
                                    switch (payload[2])
                                    {
                                        case 0xC2: // MIC and PCM messages received
                                        {
                                            DescriptionToInsert = "REQUEST  | CMT | MIC AND PCM MESSAGES RECEIVED";
                                            break;
                                        }
                                        default:
                                        {
                                            DescriptionToInsert = "REQUEST  | CMT | ROM DATA";
                                            ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                            break;
                                        }
                                    }
                                    break;
                                }
                                case 0x24: // software version
                                {
                                    switch (payload[2])
                                    {
                                        case 0x00:
                                        {
                                            DescriptionToInsert = "REQUEST  | CMT | SOFTWARE VERSION";
                                            break;
                                        }
                                        case 0x01:
                                        {
                                            DescriptionToInsert = "REQUEST  | CMT | EEPROM VERSION";
                                            break;
                                        }
                                        default:
                                        {
                                            ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                            break;
                                        }
                                    }
                                    break;
                                }
                                case 0x40: // erase fault codes
                                {
                                    DescriptionToInsert = "REQUEST  | CMT | ERASE FAULT CODES";
                                    ValueToInsert = "PAGE: " + Util.ByteToHexString(payload, 2, 1);
                                    break;
                                }
                                default:
                                {
                                    DescriptionToInsert = "REQUEST  | CMT | COMMAND: " + Util.ByteToHexString(payload, 1, 1);
                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                    break;
                                }
                            }
                            break;
                        }
                        case 0x1E: // ACM - Airbag Control Module
                        {
                            switch (payload[1]) // command
                            {
                                case 0x00: // reset
                                {
                                    DescriptionToInsert = "REQUEST  | AIRBAG CONTROL MODULE | RESET";
                                    break;
                                }
                                case 0x16: // read fault codes
                                {
                                    DescriptionToInsert = "REQUEST  | ACM | FAULT CODES";
                                    ValueToInsert = "PAGE: " + Util.ByteToHexString(payload, 2, 1);
                                    break;
                                }
                                case 0x24: // software version
                                {
                                    DescriptionToInsert = "REQUEST  | ACM | SOFTWARE VERSION";
                                    break;
                                }
                                case 0x40: // erase fault codes
                                {
                                    DescriptionToInsert = "REQUEST  | ACM | ERASE FAULT CODES";
                                    ValueToInsert = "PAGE: " + Util.ByteToHexString(payload, 2, 1);
                                    break;
                                }
                                default:
                                {
                                    DescriptionToInsert = "REQUEST  | ACM | COMMAND: " + Util.ByteToHexString(payload, 1, 1);
                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                    break;
                                }
                            }
                            break;
                        }
                        case 0x20: // BCM - Body Control Module | BCM JA PREM VTSS; sc: Body; 0x1012
                        {
                            switch (payload[1]) // command
                            {
                                case 0x00: // reset
                                {
                                    DescriptionToInsert = "REQUEST  | BODY CONTROL MODULE | RESET";
                                    break;
                                }
                                case 0x10: // actuator tests
                                {
                                    DescriptionToInsert = "REQUEST  | BCM | ACTUATOR TEST";

                                    switch (payload[2])
                                    {
                                        case 0x00:
                                        {
                                            switch (payload[3])
                                            {
                                                case 0x08:
                                                {
                                                    ValueToInsert = "CHIME";
                                                    break;
                                                }
                                                case 0x20:
                                                {
                                                    ValueToInsert = "COURTESY LAMPS";
                                                    break;
                                                }
                                                default:
                                                {
                                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                                    break;
                                                }
                                            }
                                            break;
                                        }
                                        case 0x01:
                                        {
                                            switch (payload[3])
                                            {
                                                case 0x04:
                                                {
                                                    ValueToInsert = "HEADLAMP RELAY";
                                                    break;
                                                }
                                                case 0x08:
                                                {
                                                    ValueToInsert = "HORN RELAY";
                                                    break;
                                                }
                                                case 0x10:
                                                {
                                                    ValueToInsert = "DOOR LOCK";
                                                    break;
                                                }
                                                case 0x20:
                                                {
                                                    ValueToInsert = "DOOR UNLOCK";
                                                    break;
                                                }
                                                case 0x40:
                                                {
                                                    ValueToInsert = "DR DOOR UNLOCK";
                                                    break;
                                                }
                                                case 0x80:
                                                {
                                                    ValueToInsert = "EBL RELAY";
                                                    break;
                                                }
                                                default:
                                                {
                                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                                    break;
                                                }
                                            }
                                            break;
                                        }
                                        case 0x02:
                                        {
                                            switch (payload[3])
                                            {
                                                case 0x20:
                                                {
                                                    ValueToInsert = "VTSS LAMP";
                                                    break;
                                                }
                                                case 0x40:
                                                {
                                                    ValueToInsert = "WIPERS LOW";
                                                    break;
                                                }
                                                case 0xC0:
                                                {
                                                    ValueToInsert = "WIPERS HIGH";
                                                    break;
                                                }
                                                default:
                                                {
                                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                                    break;
                                                }
                                            }
                                            break;
                                        }
                                        case 0x03:
                                        {
                                            switch (payload[3])
                                            {
                                                default:
                                                {
                                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                                    break;
                                                }
                                            }
                                            break;
                                        }
                                        case 0x04:
                                        {
                                            switch (payload[3])
                                            {
                                                case 0x00:
                                                {
                                                    ValueToInsert = "RECAL ATC";
                                                    break;
                                                }
                                                default:
                                                {
                                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                                    break;
                                                }
                                            }
                                            break;
                                        }
                                        case 0x05:
                                        {
                                            switch (payload[3])
                                            {
                                                default:
                                                {
                                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                                    break;
                                                }
                                            }
                                            break;
                                        }
                                        case 0x06:
                                        {
                                            switch (payload[3])
                                            {
                                                default:
                                                {
                                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                                    break;
                                                }
                                            }
                                            break;
                                        }
                                        case 0x07:
                                        {
                                            switch (payload[3])
                                            {
                                                default:
                                                {
                                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                                    break;
                                                }
                                            }
                                            break;
                                        }
                                        case 0x09:
                                        {
                                            switch (payload[3])
                                            {
                                                default:
                                                {
                                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                                    break;
                                                }
                                            }
                                            break;
                                        }
                                        case 0x0A:
                                        {
                                            switch (payload[3])
                                            {
                                                case 0x00:
                                                {
                                                    ValueToInsert = "ENABLE VTSS";
                                                    break;
                                                }
                                                default:
                                                {
                                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                                    break;
                                                }
                                            }
                                            break;
                                        }
                                        case 0x0B:
                                        {
                                            switch (payload[3])
                                            {
                                                default:
                                                {
                                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                                    break;
                                                }
                                            }
                                            break;
                                        }
                                        case 0x0C:
                                        {
                                            switch (payload[3])
                                            {
                                                default:
                                                {
                                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                                    break;
                                                }
                                            }
                                            break;
                                        }
                                        case 0x0D:
                                        {
                                            switch (payload[3])
                                            {
                                                case 0x10:
                                                {
                                                    ValueToInsert = "ENABLE DOOR LOCK";
                                                    break;
                                                }
                                                default:
                                                {
                                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                                    break;
                                                }
                                            }
                                            break;
                                        }
                                        case 0x0E:
                                        {
                                            switch (payload[3])
                                            {
                                                case 0x10:
                                                {
                                                    ValueToInsert = "DISABLE DOOR LOCK";
                                                    break;
                                                }
                                                default:
                                                {
                                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                                    break;
                                                }
                                            }
                                            break;
                                        }
                                        default:
                                        {
                                            ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                            break;

                                        }
                                    }
                                    break;
                                }
                                case 0x12: // read digital parameters
                                {
                                    DescriptionToInsert = "REQUEST  | BCM | DIGITAL READ";
                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                    break;
                                }
                                case 0x14: // read analog parameters
                                {
                                    switch (payload[2])
                                    {
                                        case 0x00:
                                        {
                                            DescriptionToInsert = "REQUEST  | BCM | ANALOG READ: PASSENGER DOOR DISARM";
                                            break;
                                        }
                                        case 0x01:
                                        {
                                            DescriptionToInsert = "REQUEST  | BCM | ANALOG READ: PANEL LAMPS";
                                            break;
                                        }
                                        case 0x02:
                                        {
                                            DescriptionToInsert = "REQUEST  | BCM | ANALOG READ: DRDOOR DISARM";
                                            break;
                                        }
                                        case 0x03:
                                        {
                                            DescriptionToInsert = "REQUEST  | BCM | ANALOG READ: HVAC CONTROL HEAD VOLTAGE";
                                            break;
                                        }
                                        case 0x04:
                                        {
                                            DescriptionToInsert = "REQUEST  | BCM | ANALOG READ: CONVERT SELECT";
                                            break;
                                        }
                                        case 0x05:
                                        {
                                            DescriptionToInsert = "REQUEST  | BCM | ANALOG READ: MODE DOOR";
                                            break;
                                        }
                                        case 0x06:
                                        {
                                            DescriptionToInsert = "REQUEST  | BCM | ANALOG READ: DOOR STALL";
                                            break;
                                        }
                                        case 0x07:
                                        {
                                            DescriptionToInsert = "REQUEST  | BCM | ANALOG READ: A/C SWITCH";
                                            break;
                                        }
                                        case 0x08:
                                        {
                                            DescriptionToInsert = "REQUEST  | BCM | ANALOG READ: DOOR LOCK SWITCH VOLTAGE";
                                            break;
                                        }
                                        case 0x09:
                                        {
                                            DescriptionToInsert = "REQUEST  | BCM | ANALOG READ: BATTERY VOLTAGE";
                                            break;
                                        }
                                        case 0x0B:
                                        {
                                            DescriptionToInsert = "REQUEST  | BCM | ANALOG READ: FUEL LEVEL";
                                            break;
                                        }
                                        case 0x0C:
                                        {
                                            DescriptionToInsert = "REQUEST  | BCM | ANALOG READ: EVAP TEMP VOLTAGE";
                                            break;
                                        }
                                        case 0x0A:
                                        {
                                            DescriptionToInsert = "REQUEST  | BCM | ANALOG READ: IGNITION VOLTAGE";
                                            break;
                                        }
                                        case 0x0D:
                                        {
                                            DescriptionToInsert = "REQUEST  | BCM | ANALOG READ: INTERMITTENT WIPER VOLTAGE";
                                            break;
                                        }
                                        default:
                                        {
                                            DescriptionToInsert = "REQUEST  | BCM | ANALOG READ";
                                            break;
                                        }
                                    }

                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                    break;
                                }
                                case 0x16: // read fault codes
                                {
                                    DescriptionToInsert = "REQUEST  | BCM | FAULT CODES";
                                    ValueToInsert = "PAGE: " + Util.ByteToHexString(payload, 2, 1);
                                    break;
                                }
                                case 0x22: // read ROM
                                {
                                    DescriptionToInsert = "REQUEST  | BCM | ROM DATA";
                                    int address = (payload[2] << 8) + payload[3];
                                    int addressNext = address + 1;
                                    byte[] addressNextArray = { (byte)((addressNext >> 8) & 0xFF), (byte)(addressNext & 0xFF) };
                                    ValueToInsert = "OFFSET: " + Util.ByteToHexString(message, 3, 2) + " | " + Util.ByteToHexStringSimple(addressNextArray);
                                    break;
                                }
                                case 0x24: // read module id
                                {
                                    DescriptionToInsert = "REQUEST  | BCM | MODULE ID";
                                    break;
                                }
                                case 0x2A: // read vehicle identification number (vin)
                                {
                                    DescriptionToInsert = "REQUEST  | BCM | READ VIN";
                                    break;
                                }
                                case 0x2C: // write vehicle identification number (vin)
                                {
                                    DescriptionToInsert = "REQUEST  | BCM | WRITE VIN";
                                    break;
                                }
                                case 0x40: // erase fault codes
                                {
                                    DescriptionToInsert = "REQUEST  | BCM | ERASE FAULT CODES";
                                    break;
                                }
                                case 0x60: // write EEPROM
                                {
                                    DescriptionToInsert = "REQUEST  | BCM | WRITE EEPROM | OFFSET: " + Util.ByteToHexStringSimple(new byte[2] { payload[2], payload[3] });
                                    break;
                                }
                                case 0xB0: // write settings
                                {
                                    DescriptionToInsert = "REQUEST  | BCM | WRITE SETTINGS";
                                    break;
                                }
                                case 0xB1: // read settings
                                {
                                    DescriptionToInsert = "REQUEST  | BCM | READ SETTINGS";
                                    break;
                                }
                                default:
                                {
                                    DescriptionToInsert = "REQUEST  | BCM | COMMAND: " + Util.ByteToHexString(payload, 1, 1);
                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                    break;
                                }
                            }
                            break;
                        }
                        case 0x22: // MIC - Mechanical Instrument Cluster
                        case 0x60:
                        {
                            switch (payload[1]) // command
                            {
                                case 0x00: // reset
                                {
                                    DescriptionToInsert = "REQUEST  | MECHANICAL INSTRUMENT CLUSTER | RESET";
                                    break;
                                }
                                case 0x10: // actuator test
                                {
                                    DescriptionToInsert = "REQUEST  | MIC | ACTUATOR TEST";

                                    switch (payload[2])
                                    {
                                        case 0x00:
                                        {
                                            ValueToInsert = "ALL GAUGES";
                                            break;
                                        }
                                        case 0x01:
                                        {
                                            ValueToInsert = "ALL LAMPS";
                                            break;
                                        }
                                        case 0x02:
                                        {
                                            ValueToInsert = "ODO/TRIP/PRND3L";
                                            break;
                                        }
                                        case 0x03:
                                        {
                                            ValueToInsert = "PRND3L SEGMENTS";
                                            break;
                                        }
                                        default:
                                        {
                                            ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                            break;
                                        }
                                    }
                                    break;
                                }
                                case 0x12: // read digital parameters
                                {
                                    DescriptionToInsert = "REQUEST  | MIC | DIGITAL READ";

                                    switch (payload[2])
                                    {
                                        case 0x00:
                                        {
                                            ValueToInsert = "ALL SWITCHES";
                                            break;
                                        }
                                        default:
                                        {
                                            ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                            break;
                                        }
                                    }
                                    break;
                                }
                                case 0x16: // read fault codes
                                {
                                    DescriptionToInsert = "REQUEST  | MIC | FAULT CODES";
                                    ValueToInsert = "PAGE: " + Util.ByteToHexString(payload, 2, 1);
                                    break;
                                }
                                case 0x24: // software version
                                {
                                    DescriptionToInsert = "REQUEST  | MIC | SOFTWARE VERSION";
                                    break;
                                }
                                case 0x40: // erase fault codes
                                {
                                    DescriptionToInsert = "REQUEST  | MIC | ERASE FAULT CODES";
                                    ValueToInsert = "PAGE: " + Util.ByteToHexString(payload, 2, 1);
                                    break;
                                }
                                case 0xE0: // self test
                                {
                                    DescriptionToInsert = "REQUEST  | MIC | SELF TEST";
                                    break;
                                }
                                default:
                                {
                                    DescriptionToInsert = "REQUEST  | MIC | COMMAND: " + Util.ByteToHexString(payload, 1, 1);
                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                    break;
                                }
                            }
                            break;
                        }
                        case 0x41: // TCM - Transmission Control Module
                        case 0x42:
                        {
                            switch (payload[1])
                            {
                                case 0x00: // reset
                                {
                                    DescriptionToInsert = "REQUEST  | TRANSMISSION CONTROL MODULE | RESET";
                                    break;
                                }
                                case 0x24: // read analog parameters
                                {
                                    DescriptionToInsert = "REQUEST  | TCM | READ ANALOG PARAMETER";

                                    switch (payload[2])
                                    {
                                        case 0x0B: // LR Clutch Volume Index (CVI)
                                        {
                                            TransmissionLRCVIRequested = true;
                                            DescriptionToInsert = "REQUEST  | TCM | LR CLUTCH VOLUME INDEX (CVI)";
                                            break;
                                        }
                                        case 0x0C: // 24 CVI
                                        {
                                            Transmission24CVIRequested = true;
                                            DescriptionToInsert = "REQUEST  | TCM | 24 CLUTCH VOLUME INDEX (CVI)";
                                            break;
                                        }
                                        case 0x0D: // OD CVI
                                        {
                                            TransmissionODCVIRequested = true;
                                            DescriptionToInsert = "REQUEST  | TCM | OD CLUTCH VOLUME INDEX (CVI)";
                                            break;
                                        }
                                        case 0x0E: // UD CVI
                                        {
                                            TransmissionUDCVIRequested = true;
                                            DescriptionToInsert = "REQUEST  | TCM | UD CLUTCH VOLUME INDEX (CVI)";
                                            break;
                                        }
                                        case 0x10: // transmission temperature
                                        {
                                            TransmissionTemperatureRequested = true;
                                            DescriptionToInsert = "REQUEST  | TCM | TRANSMISSION TEMPERATURE";
                                            break;
                                        }
                                        default:
                                        {
                                            ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                            break;
                                        }
                                    }
                                    break;
                                }
                                default:
                                {
                                    DescriptionToInsert = "REQUEST  | TCM | COMMAND: " + Util.ByteToHexString(payload, 1, 1);
                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                    break;
                                }
                            }
                            break;
                        }
                        case 0x43: // ABS - Antilock Brake System
                        {
                            switch (payload[1])
                            {
                                case 0x00: // reset
                                {
                                    DescriptionToInsert = "REQUEST  | ANTILOCK BRAKE SYSTEM | RESET";
                                    break;
                                }
                                default:
                                {
                                    DescriptionToInsert = "REQUEST  | ABS | COMMAND: " + Util.ByteToHexString(payload, 1, 1);
                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                    break;
                                }
                            }
                            break;
                        }
                        case 0x50: // HVAC - Heat Vent Air Conditioning
                        {
                            switch (payload[1])
                            {
                                case 0x00: // reset
                                {
                                    DescriptionToInsert = "REQUEST  | HVAC | RESET";
                                    break;
                                }
                                default:
                                {
                                    DescriptionToInsert = "REQUEST  | HVAC | COMMAND: " + Util.ByteToHexString(payload, 1, 1);
                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                    break;
                                }
                            }
                            break;
                        }
                        case 0x80: // DDM - Driver Door Module
                        {
                            switch (payload[1])
                            {
                                case 0x00: // reset
                                {
                                    DescriptionToInsert = "REQUEST  | DRIVER DOOR MODULE | RESET";
                                    break;
                                }
                                default:
                                {
                                    DescriptionToInsert = "REQUEST  | DDM | COMMAND: " + Util.ByteToHexString(payload, 1, 1);
                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                    break;
                                }
                            }
                            break;
                        }
                        case 0x81: // PDM - Passenger Door Module
                        {
                            switch (payload[1])
                            {
                                case 0x00: // reset
                                {
                                    DescriptionToInsert = "REQUEST  | PASSENGER DOOR MODULE | RESET";
                                    break;
                                }
                                default:
                                {
                                    DescriptionToInsert = "REQUEST  | PDM | COMMAND: " + Util.ByteToHexString(payload, 1, 1);
                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                    break;
                                }
                            }
                            break;
                        }
                        case 0x82: // MSM - Memory Seat Module
                        {
                            switch (payload[1])
                            {
                                case 0x00: // reset
                                {
                                    DescriptionToInsert = "REQUEST  | MEMORY SEAT MODULE | RESET";
                                    break;
                                }
                                default:
                                {
                                    DescriptionToInsert = "REQUEST  | MSM | COMMAND: " + Util.ByteToHexString(payload, 1, 1);
                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                    break;
                                }
                            }
                            break;
                        }
                        case 0x96: // ASM - Audio System Module
                        {
                            switch (payload[1])
                            {
                                case 0x00: // reset
                                {
                                    DescriptionToInsert = "REQUEST  | AUDIO SYSTEM MODULE | RESET";
                                    break;
                                }
                                default:
                                {
                                    DescriptionToInsert = "REQUEST  | ASM | COMMAND: " + Util.ByteToHexString(payload, 1, 1);
                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                    break;
                                }
                            }
                            break;
                        }
                        case 0xC0: // SKIM - Sentry Key Immobilizer Module
                        {
                            switch (payload[1])
                            {
                                case 0x00: // reset
                                {
                                    DescriptionToInsert = "REQUEST  | SKIM | RESET";
                                    break;
                                }
                                default:
                                {
                                    DescriptionToInsert = "REQUEST  | SKIM | COMMAND: " + Util.ByteToHexString(payload, 1, 1);
                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                    break;
                                }
                            }
                            break;
                        }
                        case 0xFF: // ALL modules present on the CCD-bus
                        {
                            switch (payload[1]) // command
                            {
                                case 0x00: // reset
                                {
                                    DescriptionToInsert = "REQUEST  | RESET ALL CCD-BUS MODULES";
                                    break;
                                }
                                default:
                                {
                                    DescriptionToInsert = "REQUEST  | ALL CCD-BUS MODULES | COMMAND: " + Util.ByteToHexString(payload, 1, 1);
                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                    break;
                                }
                            }
                            break;
                        }
                        default:
                        {
                            DescriptionToInsert = "REQUEST  | MODULE: " + Util.ByteToHexStringSimple(new byte[1] { payload[0] }) + " | COMMAND: " + Util.ByteToHexStringSimple(new byte[1] { payload[1] }) + " | PARAMS: " + Util.ByteToHexStringSimple(new byte[2] { payload[2], payload[3] });
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

                    File.AppendAllText(MainForm.CCDB2F2LogFilename, "CCD: " + Util.ByteToHexStringSimple(message) + Environment.NewLine);
                    break;
                }
                case 0xB4:
                case 0xC4:
                {
                    DescriptionToInsert = "VEHICLE SPEED SENSOR PULSE INTERVAL";

                    if (message.Length < 4) break;

                    ushort DistancePulse = (ushort)((payload[0] << 8) + payload[1]);

                    if (DistancePulse == 0) break;

                    double VehicleSpeedMPH = 28800.0 / DistancePulse; // 8000 * 3.6 = 28800
                    double VehicleSpeedKMH = VehicleSpeedMPH * 1.609344;
                    //double MileageIncrementMi = 22016.0 / DistancePulse; // 64 * 344 = 22016, already received in CCD ID 84
                    //double MileageIncrementKm = MileageIncrementMi * 1.609344;

                    if (Properties.Settings.Default.Units == "imperial")
                    {
                        if (DistancePulse != 0xFFFF)
                        {
                            ValueToInsert = Math.Round(VehicleSpeedMPH, 1).ToString("0.0").Replace(",", ".");
                        }
                        else
                        {
                            ValueToInsert = "0.0";
                        }

                        UnitToInsert = "MPH";
                    }
                    else if (Properties.Settings.Default.Units == "metric")
                    {
                        if (DistancePulse != 0xFFFF)
                        {
                            ValueToInsert = Math.Round(VehicleSpeedKMH, 1).ToString("0.0").Replace(",", ".");
                        }
                        else
                        {
                            ValueToInsert = "0.0";
                        }

                        UnitToInsert = "KM/H";
                    }
                    break;
                }
                case 0xB6:
                {
                    DescriptionToInsert = "PCM BEACON B6";

                    if (message.Length < 4) break;

                    BeaconNote = BeaconNote.Remove(0x02, 2).Insert(0x02, ((char)payload[0]).ToString() + ((char)payload[1]).ToString());
                    ValueToInsert = Util.ByteToHexString(payload, 0, 2);
                    break;
                }
                case 0xBA:
                {
                    DescriptionToInsert = "REQUEST COMPASS CALIBRATION OR VARIANCE";

                    if (message.Length < 4) break;

                    // Empty.

                    break;
                }
                case 0xBE:
                {
                    DescriptionToInsert = "IGNITION SWITCH POSITION";

                    if (message.Length < 3) break;

                    if (Util.IsBitSet(payload[0], 4)) ValueToInsert = "ON";
                    else ValueToInsert = "OFF";
                    break;
                }
                case 0xC2:
                {
                    DescriptionToInsert = "SKIM | SEED/KEY VALIDATION";

                    if (message.Length < 6) break;

                    switch (payload[0])
                    {
                        case 0xC0:
                        {
                            DescriptionToInsert = "SKIM | KEY RECEIVED";
                            ValueToInsert = Util.ByteToHexString(payload, 1, 3);
                            break;
                        }
                        case 0xC1:
                        {
                            DescriptionToInsert = "SKIM | PAYLOAD #1 TO BE WRITTEN TO PCM EEPROM"; // packet received from SKIM to write to PCM EEPROM (DRB3 PCM replaced menu)
                            Array.Copy(payload, 1, SKIMPayload, 0, 3);
                            ValueToInsert = Util.ByteToHexString(payload, 1, 3);
                            break;
                        }
                        case 0xC2:
                        {
                            DescriptionToInsert = "SKIM | PAYLOAD #2 TO BE WRITTEN TO PCM EEPROM"; // packet received from SKIM to write to PCM EEPROM (DRB3 PCM replaced menu)
                            Array.Copy(payload, 1, SKIMPayload, 3, 2);
                            ValueToInsert = Util.ByteToHexStringSimple(SKIMPayload);
                            UnitToInsert = "EEPROM 01D8";
                            break;
                        }
                        case 0xC8:
                        {
                            DescriptionToInsert = "SKIM | REQUEST SEED FROM PCM";
                            break;
                        }
                        default:
                        {
                            ValueToInsert = "INVALID MSG";
                            break;
                        }
                    }
                    break;
                }
                case 0xCA:
                {
                    DescriptionToInsert = "WRITE EEPROM";

                    if (message.Length < 6) break;

                    switch (payload[0])
                    {
                        case 0x1B: // VTS
                        {
                            DescriptionToInsert = "WRITE EEPROM | VTS | ";
                            break;
                        }
                        case 0x20: // BCM
                        {
                            DescriptionToInsert = "WRITE EEPROM | BCM | ";
                            break;
                        }
                        case 0x43: // ABS
                        {
                            DescriptionToInsert = "WRITE EEPROM | ABS | ";
                            break;
                        }
                        default:
                        {
                            DescriptionToInsert = "WRITE EEPROM | MODULE ID: " + Util.ByteToHexString(payload, 0, 1) + " | ";
                            break;
                        }
                    }

                    DescriptionToInsert += "OFFSET: " + Util.ByteToHexString(payload, 1, 2);
                    ValueToInsert = Util.ByteToHexString(payload, 3, 1);
                    break;
                }
                case 0xCB:
                {
                    DescriptionToInsert = "SEND COMPASS AND LAST OUTSIDE AIR TEMPERATURE DATA";

                    if (message.Length < 4) break;

                    // Empty.

                    break;
                }
                case 0xCC:
                {
                    DescriptionToInsert = "PCM MILEAGE | TARGET ENGINE IDLE SPEED";

                    if (message.Length < 4) break;

                    double Mileage = payload[0] * 256 * 8.192 * 0.25; // miles
                    double Kilometerage = Mileage * 1.609344; // kilometers
                    double TargetIdle = payload[1] * 32.0 * 0.25; // rpm

                    if (payload[1] == 0xFF) TargetIdle = 0; // sometimes this slot is FFd out and not used

                    if (Properties.Settings.Default.Units == "imperial")
                    {
                        ValueToInsert = Math.Round(Mileage).ToString("0") + " | " + Math.Round(TargetIdle).ToString("0");
                        UnitToInsert = "MI | RPM";
                    }
                    else if (Properties.Settings.Default.Units == "metric")
                    {
                        ValueToInsert = Math.Round(Kilometerage).ToString("0") + " | " + Math.Round(TargetIdle).ToString("0");
                        UnitToInsert = "KM | RPM";
                    }
                    break;
                }
                case 0xCD:
                {
                    DescriptionToInsert = "PCM BEACON CD";

                    if (message.Length < 4) break;

                    BeaconNote = BeaconNote.Remove(0x0E, 2).Insert(0x0E, ((char)payload[0]).ToString() + ((char)payload[1]).ToString());
                    ValueToInsert = Util.ByteToHexString(payload, 0, 2);
                    break;
                }
                case 0xCE:
                {
                    DescriptionToInsert = "BCM MILEAGE";

                    if (message.Length < 6) break;

                    double BCMMileage = (uint)(payload[0] << 24 | payload[1] << 16 | payload[2] << 8 | payload[3]) * 0.000125;
                    double BCMKilometerage = BCMMileage * 1.609344;

                    if (Properties.Settings.Default.Units == "imperial")
                    {
                        ValueToInsert = Math.Round(BCMMileage, 3).ToString("0.000").Replace(",", ".");
                        UnitToInsert = "MILE";
                    }
                    else if (Properties.Settings.Default.Units == "metric")
                    {
                        ValueToInsert = Math.Round(BCMKilometerage, 3).ToString("0.000").Replace(",", ".");
                        UnitToInsert = "KILOMETER";
                    }
                    break;
                }
                case 0xD3:
                {
                    DescriptionToInsert = "COMPASS DISPLAY";

                    if (message.Length < 4) break;

                    // Empty.

                    break;
                }
                case 0xD4:
                {
                    DescriptionToInsert = "BATTERY VOLTAGE | CHARGING VOLTAGE";

                    if (message.Length < 4) break;

                    double BatteryVoltageB = payload[0] * 0.0625;
                    double ChargingVoltage = payload[1] * 0.0625;

                    ValueToInsert = Math.Round(BatteryVoltageB, 1).ToString("0.0").Replace(",", ".") + " | " + Math.Round(ChargingVoltage, 1).ToString("0.0").Replace(",", ".");
                    UnitToInsert = "V | V";
                    break;
                }
                case 0xDA:
                {
                    DescriptionToInsert = "MIC SWITCH/LAMP STATE";

                    if (message.Length < 3) break;

                    if (Util.IsBitSet(payload[0], 6)) ValueToInsert = "CEL ON";
                    else ValueToInsert = "CEL OFF";

                    break;
                }
                case 0xDB:
                {
                    DescriptionToInsert = "COMPASS CALL DATA | A/C CLUTCH ON";

                    if (message.Length < 4) break;

                    // Empty.

                    break;
                }
                case 0xDC:
                {
                    DescriptionToInsert = "TRANSMISSION SELECTED GEAR";

                    if (message.Length < 3) break;

                    ValueToInsert = string.Empty;

                    if (Util.IsBitSet(payload[0], 0)) ValueToInsert += "NEUTRAL ";
                    if (Util.IsBitSet(payload[0], 1)) ValueToInsert += "REVERSE ";
                    if (Util.IsBitSet(payload[0], 2)) ValueToInsert += "1ST ";
                    if (Util.IsBitSet(payload[0], 3)) ValueToInsert += "2ND ";
                    if (Util.IsBitSet(payload[0], 4)) ValueToInsert += "3RD ";
                    if (Util.IsBitSet(payload[0], 5)) ValueToInsert += "4TH ";

                    switch ((payload[0] >> 6) & 0x03)
                    {
                        case 1:
                        {
                            ValueToInsert += "| LOCK: PART";
                            break;
                        }
                        case 2:
                        {
                            ValueToInsert += "| LOCK: FULL";
                            break;
                        }
                    }
                    break;
                }
                case 0xE4:
                {
                    DescriptionToInsert = "ENGINE SPEED | INTAKE MANIFOLD ABSOLUTE PRESSURE";

                    if (message.Length < 4) break;

                    double EngineSpeed = payload[0] * 32.0;
                    double MAPPSI = payload[1] * 0.1217 * 0.4911542; // 1 inHg = 0.4911542 psi
                    double MAPKPA = MAPPSI * 6.894757;

                    if (Properties.Settings.Default.Units == "imperial")
                    {
                        ValueToInsert = Math.Round(EngineSpeed).ToString("0") + " | " + Math.Round(MAPPSI, 1).ToString("0.0").Replace(",", ".");
                        UnitToInsert = "RPM | PSI";
                    }
                    else if (Properties.Settings.Default.Units == "metric")
                    {
                        ValueToInsert = Math.Round(EngineSpeed).ToString("0") + " | " + Math.Round(MAPKPA, 1).ToString("0.0").Replace(",", ".");
                        UnitToInsert = "RPM | KPA";
                    }
                    break;
                }
                case 0xEC:
                {
                    DescriptionToInsert = "LIMP-IN STATE";

                    if (message.Length < 4) break;

                    List<string> LimpStates = new List<string>();

                    if (Util.IsBitSet(payload[0], 7)) LimpStates.Add("ATS"); // Ambient temperature sensor
                    if (Util.IsBitSet(payload[0], 6)) LimpStates.Add("IAT"); // Intake air temperature
                    if (Util.IsBitSet(payload[0], 5)) LimpStates.Add("FSM"); // Fuel system?
                    if (Util.IsBitSet(payload[0], 4)) LimpStates.Add("ACP"); // A/C high-side pressure
                    if (Util.IsBitSet(payload[0], 3)) LimpStates.Add("CHG"); // Charging system
                    if (Util.IsBitSet(payload[0], 2)) LimpStates.Add("CHB"); // Battery charging voltage
                    if (Util.IsBitSet(payload[0], 1)) LimpStates.Add("TPS"); // Throttle position sensor
                    if (Util.IsBitSet(payload[0], 0)) LimpStates.Add("ECT"); // Engine coolant temperature

                    if (LimpStates.Count > 0)
                    {
                        DescriptionToInsert = "LIMP: ";

                        foreach (string s in LimpStates)
                        {
                            DescriptionToInsert += s + " | ";
                        }

                        if (DescriptionToInsert.Length > 2) DescriptionToInsert = DescriptionToInsert.Remove(DescriptionToInsert.Length - 3); // remove last "|" character
                    }
                    else
                    {
                        DescriptionToInsert = "NO LIMP-IN STATE";
                    }

                    switch (payload[1] & 0x1C) // analyze relevant bits only
                    {
                        case 0x00:
                        {
                            ValueToInsert = "FUEL: CNG";
                            break;
                        }
                        case 0x04:
                        {
                            ValueToInsert = "FUEL: UNLEADED GAS";
                            break;
                        }
                        case 0x08:
                        {
                            ValueToInsert = "FUEL: LEADED GAS";
                            break;
                        }
                        case 0x0C:
                        {
                            ValueToInsert = "FUEL: FLEX";
                            break;
                        }
                        case 0x10:
                        {
                            ValueToInsert = "FUEL: DIESEL";
                            break;
                        }
                        default:
                        {
                            ValueToInsert = "FUEL: UNKNOWN";
                            break;
                        }
                    }

                    if (Util.IsBitSet(payload[1], 1) || Util.IsBitSet(payload[1], 0))
                    {
                        UnitToInsert = "SKIM: " + Convert.ToString(payload[1] & 0x03, 2).PadLeft(2, '0');
                    }
                    break;
                }
                case 0xEE:
                {
                    DescriptionToInsert = "BCM TRIP DISTANCE";

                    if (message.Length < 5) break;

                    double BCMTripDistanceMi = (uint)(payload[0] << 16 | payload[1] << 8 | payload[2]) * 0.016;
                    double BCMTripDistanceKm = BCMTripDistanceMi * 1.609344;

                    if (Properties.Settings.Default.Units == "imperial")
                    {
                        ValueToInsert = Math.Round(BCMTripDistanceMi, 3).ToString("0.000").Replace(",", ".");
                        UnitToInsert = "MILE";
                    }
                    else if (Properties.Settings.Default.Units == "metric")
                    {
                        ValueToInsert = Math.Round(BCMTripDistanceKm, 3).ToString("0.000").Replace(",", ".");
                        UnitToInsert = "KILOMETER";
                    }
                    break;
                }
                case 0xF1:
                {
                    DescriptionToInsert = "WARNING: ";

                    if (message.Length < 3) break;

                    List<string> WarningListA = new List<string>();

                    if (payload[0] == 0)
                    {
                        DescriptionToInsert = "NO WARNING";
                    }
                    else
                    {
                        if (Util.IsBitSet(payload[0], 7)) WarningListA.Add("-7-");
                        if (Util.IsBitSet(payload[0], 6)) WarningListA.Add("-6-");
                        if (Util.IsBitSet(payload[0], 5)) WarningListA.Add("-5-");
                        if (Util.IsBitSet(payload[0], 4)) WarningListA.Add("BPP"); // brake pedal pressed
                        if (Util.IsBitSet(payload[0], 3)) WarningListA.Add("CRT"); // critical temperature
                        if (Util.IsBitSet(payload[0], 2)) WarningListA.Add("HIT"); // high temperature
                        if (Util.IsBitSet(payload[0], 1)) WarningListA.Add("LWO"); // low oil
                        if (Util.IsBitSet(payload[0], 0)) WarningListA.Add("LWF"); // low fuel

                        foreach (string s in WarningListA)
                        {
                            DescriptionToInsert += s + " | ";
                        }

                        if (DescriptionToInsert.Length > 2) DescriptionToInsert = DescriptionToInsert.Remove(DescriptionToInsert.Length - 3); // remove last "|" character
                    }
                    break;
                }
                case 0xF2:
                {
                    DescriptionToInsert = "RESPONSE |";

                    if (message.Length < 6) break;

                    switch (payload[0]) // module address
                    {
                        case 0x10: // VIC - Vehicle Info Center
                        {
                            switch (payload[1])
                            {
                                case 0x00: // reset
                                {
                                    DescriptionToInsert = "RESPONSE | VEHICLE INFO CENTER | RESET COMPLETE";
                                    break;
                                }
                                case 0x10: // actuator tests
                                {
                                    switch (payload[2])
                                    {
                                        case 0x10:
                                        {
                                            DescriptionToInsert = "RESPONSE | VIC | DISPLAY TEST";
                                            break;
                                        }
                                        default:
                                        {
                                            DescriptionToInsert = "RESPONSE | VIC | ACTUATOR TEST";
                                            ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                            break;
                                        }
                                    }
                                    break;
                                }
                                case 0x12: // read digital parameters
                                {
                                    switch (payload[2])
                                    {
                                        case 0x00:
                                        {
                                            DescriptionToInsert = "RESPONSE | VIC | TRANSFER CASE POSITION";

                                            switch (payload[3] & 0x1F)
                                            {
                                                case 0x06:
                                                {
                                                    UnitToInsert = "4WD LO";
                                                    break;
                                                }
                                                case 0x07:
                                                {
                                                    UnitToInsert = "ALL 4WD";
                                                    break;
                                                }
                                                case 0x0B:
                                                {
                                                    UnitToInsert = "PART 4WD";
                                                    break;
                                                }
                                                case 0x0D:
                                                {
                                                    UnitToInsert = "FULL 4WD";
                                                    break;
                                                }
                                                case 0x0F:
                                                {
                                                    UnitToInsert = "2WD";
                                                    break;
                                                }
                                                case 0x1F:
                                                {
                                                    UnitToInsert = "NEUTRAL";
                                                    break;
                                                }
                                                default:
                                                {
                                                    UnitToInsert = "UNDEFINED";
                                                    break;
                                                }
                                            }

                                            List<string> flags = new List<string>();

                                            if (Util.IsBitClear(payload[3], 5)) flags.Add("OUTAGE");
                                            if (Util.IsBitClear(payload[3], 6)) flags.Add("TURN");

                                            if (flags.Count > 0)
                                            {
                                                foreach (string s in flags)
                                                {
                                                    ValueToInsert += s + " | ";
                                                }

                                                if (ValueToInsert.Length > 2) ValueToInsert = ValueToInsert.Remove(ValueToInsert.Length - 3); // remove last "|" character
                                                ValueToInsert += " LAMP";
                                            }

                                            break;
                                        }
                                        default:
                                        {
                                            DescriptionToInsert = "RESPONSE | VIC | DIGITAL READ";
                                            ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                            break;
                                        }
                                    }
                                    break;
                                }
                                case 0x14: // read analog parameters
                                {
                                    switch (payload[2])
                                    {
                                        case 0x00:
                                        {
                                            double WasherLevelSensorVoltage = payload[3] * 0.0196;
                                            DescriptionToInsert = "RESPONSE | VIC | WASHER LEVEL SENSOR VOLTAGE";
                                            ValueToInsert = Math.Round(WasherLevelSensorVoltage, 3).ToString("0.000").Replace(",", ".");
                                            UnitToInsert = "V";
                                            break;
                                        }
                                        case 0x01:
                                        {
                                            double CoolantLevelSensorVoltage = payload[3] * 0.0196;
                                            DescriptionToInsert = "RESPONSE | VIC | COOLANT LEVEL SENSOR VOLTAGE";
                                            ValueToInsert = Math.Round(CoolantLevelSensorVoltage, 3).ToString("0.000").Replace(",", ".");
                                            UnitToInsert = "V";
                                            break;
                                        }
                                        case 0x02:
                                        {
                                            double IgnitionVoltage = payload[3] * 0.099;
                                            DescriptionToInsert = "RESPONSE | VIC | IGNITION VOLTAGE";
                                            ValueToInsert = Math.Round(IgnitionVoltage, 3).ToString("0.000").Replace(",", ".");
                                            UnitToInsert = "V";
                                            break;
                                        }
                                        default:
                                        {
                                            DescriptionToInsert = "RESPONSE | VIC | ANALOG READ";
                                            ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                            break;
                                        }
                                    }
                                    break;
                                }
                                case 0x16: // read fault codes
                                {
                                    DescriptionToInsert = "RESPONSE | VIC | FAULT CODES";
                                    if (payload[3] == 0x00) ValueToInsert = "NO FAULT CODE";
                                    else ValueToInsert = "CODE: " + Util.ByteToHexString(payload, 2, 2);
                                    break;
                                }
                                case 0x24: // software version
                                {
                                    DescriptionToInsert = "RESPONSE | VIC | SOFTWARE VERSION";
                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                    break;
                                }
                                case 0x40: // erase fault codes
                                {
                                    DescriptionToInsert = "RESPONSE | VIC | ERASE FAULT CODES";
                                    if (payload[3] == 0x00) ValueToInsert = "ERASED";
                                    else ValueToInsert = "FAILED";
                                    break;
                                }
                                case 0xFF: // command error
                                {
                                    DescriptionToInsert = "RESPONSE | VIC | COMMAND ERROR";
                                    Util.ByteToHexString(payload, 2, 2);
                                    break;
                                }
                                default:
                                {
                                    DescriptionToInsert = "RESPONSE | VIC";
                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                    break;
                                }
                            }
                            break;
                        }
                        case 0x18: // VTS - Vehicle Theft Security
                        case 0x1B:
                        {
                            switch (payload[1])
                            {
                                case 0x00:
                                {
                                    DescriptionToInsert = "RESPONSE | VTS | RESET COMPLETE";
                                    break;
                                }
                                default:
                                {
                                    DescriptionToInsert = "RESPONSE | VTS";
                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                    break;
                                }
                            }
                            break;
                        }
                        case 0x19: // CMT - Compass Mini-Trip
                        {
                            switch (payload[1]) // command
                            {
                                case 0x00: // reset
                                {
                                    DescriptionToInsert = "RESPONSE | COMPASS MINI-TRIP | RESET COMPLETE";
                                    break;
                                }
                                case 0x10: // actuator test
                                {
                                    switch (payload[2])
                                    {
                                        case 0x00: // self test
                                        {
                                            DescriptionToInsert = "RESPONSE | CMT | SELF TEST";
                                            if (payload[3] == 0x01) ValueToInsert = "RUNNING";
                                            else ValueToInsert = "DENIED";
                                            break;
                                        }
                                        default:
                                        {
                                            DescriptionToInsert = "RESPONSE | CMT | ACTUATOR TEST";
                                            ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                            break;
                                        }
                                    }
                                    break;
                                }
                                case 0x11: // test status
                                {
                                    DescriptionToInsert = "RESPONSE | CMT | ACTUATOR TEST STATUS";

                                    switch (payload[2])
                                    {
                                        case 0x00: // self test
                                        {
                                            if (payload[3] == 0x01) ValueToInsert = "RUNNING";
                                            else ValueToInsert = "DENIED";
                                            break;
                                        }
                                        default:
                                        {
                                            ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                            break;
                                        }
                                    }

                                    break;
                                }
                                case 0x12: // read digital parameters
                                {
                                    switch (payload[2])
                                    {
                                        case 0x00:
                                        {
                                            DescriptionToInsert = "RESPONSE | CMT | STEP SWITCH";

                                            if (Util.IsBitSet(payload[3], 4)) ValueToInsert = "PRESSED";
                                            else ValueToInsert = "RELEASED";

                                            break;
                                        }
                                        default:
                                        {
                                            DescriptionToInsert = "RESPONSE | CMT | DIGITAL READ";
                                            ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                            break;
                                        }
                                    }
                                    break;
                                }
                                case 0x16: // read fault codes
                                {
                                    DescriptionToInsert = "RESPONSE | CMT | FAULT CODES";

                                    if (payload[3] == 0x00) ValueToInsert = "NO FAULT CODE";
                                    else ValueToInsert = "CODE: " + Util.ByteToHexString(payload, 2, 2);

                                    break;
                                }
                                case 0x20: // diagnostic data
                                {
                                    switch (payload[2])
                                    {
                                        case 0x00:
                                            double TemperatureF = payload[3] - 40;
                                            double TemperatureC = TemperatureF * 0.555556 - 17.77778;

                                            DescriptionToInsert = "RESPONSE | CMT | TEMPERATURE";

                                            if (Properties.Settings.Default.Units == "imperial")
                                            {
                                                ValueToInsert = TemperatureF.ToString("0");
                                                UnitToInsert = "°F";
                                            }
                                            else if (Properties.Settings.Default.Units == "metric")
                                            {
                                                ValueToInsert = Math.Round(TemperatureC).ToString("0");
                                                UnitToInsert = "°C";
                                            }
                                            break;
                                        default:
                                            DescriptionToInsert = "RESPONSE | CMT | DIAGNOSTIC DATA";
                                            ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                            break;

                                    }
                                    break;
                                }
                                case 0x22: // read ROM
                                {
                                    DescriptionToInsert = "RESPONSE | CMT | ROM DATA";
                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                    break;
                                }
                                case 0x24: // software version
                                {
                                    switch (payload[2])
                                    {
                                        case 0x00: // software version
                                        {
                                            DescriptionToInsert = "RESPONSE | CMT | SOFTWARE VERSION";
                                            ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                            break;
                                        }
                                        case 0x01: // EEPROM version
                                        {
                                            DescriptionToInsert = "RESPONSE | CMT | EEPROM VERSION";
                                            ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                            break;
                                        }
                                        default:
                                        {
                                            DescriptionToInsert = "RESPONSE | CMT";
                                            ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                            break;
                                        }
                                    }
                                    break;
                                }
                                case 0x40: // erase fault codes
                                {
                                    DescriptionToInsert = "RESPONSE | CMT | ERASE FAULT CODES";
                                    if (payload[3] == 0x00) ValueToInsert = "ERASED";
                                    else ValueToInsert = "FAILED";
                                    break;
                                }
                                case 0xFF: // command error
                                {
                                    DescriptionToInsert = "RESPONSE | CMT | COMMAND ERROR";
                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                    break;
                                }
                                default:
                                {
                                    DescriptionToInsert = "RESPONSE | CMT | COMMAND: " + Util.ByteToHexString(payload, 1, 1);
                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                    break;
                                }
                            }
                            break;
                        }
                        case 0x1E: // ACM - Airbag Control Module
                        {
                            switch (payload[1])
                            {
                                case 0x00: // reset
                                {
                                    DescriptionToInsert = "RESPONSE | AIRBAG CONTROL MODULE | RESET COMPLETE";
                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                    break;
                                }
                                case 0x16: // read fault codes
                                {
                                    DescriptionToInsert = "RESPONSE | ACM | FAULT CODES";
                                    if (payload[3] == 0x00) ValueToInsert = "NO FAULT CODE";
                                    else ValueToInsert = "CODE: " + Util.ByteToHexString(payload, 2, 2);
                                    break;
                                }
                                case 0x24: // software version
                                {
                                    DescriptionToInsert = "RESPONSE | ACM | SOFTWARE VERSION";
                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                    break;
                                }
                                case 0x40: // erase fault codes
                                {
                                    DescriptionToInsert = "RESPONSE | ACM | ERASE FAULT CODES";
                                    if (payload[3] == 0x00) ValueToInsert = "ERASED";
                                    else ValueToInsert = "FAILED";
                                    break;
                                }
                                case 0xFF: // command error
                                {
                                    DescriptionToInsert = "RESPONSE | ACM | COMMAND ERROR";
                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                    break;
                                }
                                default:
                                {
                                    DescriptionToInsert = "RESPONSE | ACM | COMMAND: " + Util.ByteToHexString(payload, 1, 1);
                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                    break;
                                }
                            }
                            break;
                        }
                        case 0x20: // BCM - Body Control Module
                        {
                            switch (payload[1]) // command
                            {
                                case 0x00: // reset
                                {
                                    DescriptionToInsert = "RESPONSE | BODY CONTROL MODULE | RESET COMPLETE";
                                    break;
                                }
                                case 0x10: // actuator tests
                                {
                                    DescriptionToInsert = "RESPONSE | BCM | ACTUATOR TEST";
                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                    break;
                                }
                                case 0x12: // read digital parameters
                                {
                                    DescriptionToInsert = "RESPONSE | BCM | DIGITAL READ";
                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                    break;
                                }
                                case 0x14: // read analog parameters
                                {
                                    DescriptionToInsert = "RESPONSE | BCM | ANALOG READ";
                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                    break;
                                }
                                case 0x16: // read fault codes
                                {
                                    DescriptionToInsert = "RESPONSE | BCM | FAULT CODES";
                                    if (payload[3] == 0x00) ValueToInsert = "NO FAULT CODE";
                                    else ValueToInsert = "CODE: " + Util.ByteToHexString(payload, 3, 1);
                                    break;
                                }
                                case 0x22: // read ROM
                                {
                                    DescriptionToInsert = "RESPONSE | BCM | ROM DATA";
                                    ValueToInsert = "VALUE:     " + Util.ByteToHexString(payload, 2, 1) + " |    " + Util.ByteToHexString(payload, 3, 1);
                                    break;
                                }
                                case 0x24: // read module id
                                {
                                    DescriptionToInsert = "RESPONSE | BCM | MODULE ID";
                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                    break;
                                }
                                case 0x2A: // read vehicle identification number (vin)
                                {
                                    DescriptionToInsert = "RESPONSE | BCM | READ VIN";
                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                    break;
                                }
                                case 0x2C: // write vehicle identification number (vin)
                                {
                                    DescriptionToInsert = "RESPONSE | BCM | WRITE VIN";
                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                    break;
                                }
                                case 0x40: // erase fault codes
                                {
                                    DescriptionToInsert = "RESPONSE | BCM | ERASE FAULT CODES";
                                    if (payload[3] == 0x00) ValueToInsert = "ERASED";
                                    else ValueToInsert = "FAILED";
                                    break;
                                }
                                case 0x60: // write EEPROM
                                {
                                    DescriptionToInsert = "RESPONSE | BCM | WRITE EEPROM OFFSET";
                                    ValueToInsert = Util.ByteToHexStringSimple(new byte[2] { payload[2], payload[3] });
                                    UnitToInsert = "OK";
                                    break;
                                }
                                case 0xB0: // write settings
                                {
                                    DescriptionToInsert = "RESPONSE | BCM | WRITE SETTINGS";
                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                    break;
                                }
                                case 0xB1: // read settings
                                {
                                    DescriptionToInsert = "RESPONSE | BCM | READ SETTINGS";
                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                    break;
                                }
                                case 0xFF: // command error
                                {
                                    DescriptionToInsert = "RESPONSE | BCM | COMMAND ERROR";
                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                    break;
                                }
                                default:
                                {
                                    DescriptionToInsert = "RESPONSE | BCM | COMMAND: " + Util.ByteToHexString(payload, 1, 1);
                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                    break;
                                }
                            }
                            break;
                        }
                        case 0x22: // MIC - Mechanical Instrument Cluster
                        case 0x60:
                            switch (payload[1]) // command
                            {
                                case 0x00: // reset
                                {
                                    DescriptionToInsert = "RESPONSE | MIC | RESET COMPLETE";
                                    break;
                                }
                                case 0x10: // actuator test
                                {
                                    switch (payload[2])
                                    {
                                        case 0x00:
                                        {
                                            DescriptionToInsert = "RESPONSE | MIC | ALL GAUGES TEST";
                                            break;
                                        }
                                        case 0x01:
                                        {
                                            DescriptionToInsert = "RESPONSE | MIC | ALL LAMPS TEST";
                                            break;
                                        }
                                        case 0x02:
                                        {
                                            DescriptionToInsert = "RESPONSE | MIC | ODO/TRIP/PRND3L TEST";
                                            break;
                                        }
                                        case 0x03:
                                        {
                                            DescriptionToInsert = "RESPONSE | MIC | PRND3L SEGMENTS TEST";
                                            break;
                                        }
                                        default:
                                        {
                                            DescriptionToInsert = "RESPONSE | MIC | ACTUATOR TEST";
                                            break;
                                        }
                                    }

                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2); // running or stopped
                                    break;
                                }
                                case 0x12: // read digital parameters
                                {
                                    DescriptionToInsert = "RESPONSE | MIC | DIGITAL READ";
                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                    break;
                                }
                                case 0x16: // read fault codes
                                {
                                    DescriptionToInsert = "RESPONSE | MIC | FAULT CODES";

                                    if (payload[3] == 0x00) ValueToInsert = "NO FAULT CODE";
                                    else
                                    {
                                        List<string> FaultCodes = new List<string>();

                                        if (Util.IsBitSet(payload[3], 1)) FaultCodes.Add("NO BCM MSG");
                                        if (Util.IsBitSet(payload[3], 2)) FaultCodes.Add("NO PCM MSG");
                                        if (Util.IsBitSet(payload[3], 4)) FaultCodes.Add("BCM FAILURE");
                                        if (Util.IsBitSet(payload[3], 6)) FaultCodes.Add("RAM FAILURE");
                                        if (Util.IsBitSet(payload[3], 7)) FaultCodes.Add("ROM FAILURE");

                                        foreach (string s in FaultCodes)
                                        {
                                            ValueToInsert += s + " | ";
                                        }

                                        if (ValueToInsert.Length > 2) ValueToInsert = ValueToInsert.Remove(ValueToInsert.Length - 3); // remove last "|" character
                                    }

                                    break;
                                }
                                case 0x24: // software version
                                {
                                    DescriptionToInsert = "RESPONSE | MIC | SOFTWARE VERSION";
                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                    break;
                                }
                                case 0x40: // erase fault codes
                                {
                                    DescriptionToInsert = "RESPONSE | MIC | ERASE FAULT CODES";
                                    if (payload[3] == 0x00) ValueToInsert = "ERASED";
                                    else ValueToInsert = "FAILED";
                                    break;
                                }
                                case 0xE0: // self test
                                {
                                    DescriptionToInsert = "RESPONSE | MIC | SELF TEST";
                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                    break;
                                }
                                case 0xFF: // command error
                                {
                                    DescriptionToInsert = "RESPONSE | MIC | COMMAND ERROR";
                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                    break;
                                }
                                default:
                                {
                                    DescriptionToInsert = "RESPONSE | MIC | COMMAND: " + Util.ByteToHexString(payload, 1, 1);
                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                    break;
                                }
                            }
                            break;
                        case 0x41: // TCM - Transmission Control Module
                        case 0x42:
                        {
                            switch (payload[1]) // command
                            {
                                case 0x00: // reset
                                {
                                    DescriptionToInsert = "RESPONSE | TCM | RESET COMPLETE";
                                    break;
                                }
                                case 0x24: // read analog parameter
                                {
                                    DescriptionToInsert = "RESPONSE | TCM | READ ANALOG PARAMETER";

                                    if (TransmissionLRCVIRequested)
                                    {
                                        TransmissionLRCVIRequested = false;
                                        double CVI_LR_I = payload[2] / 64.0;
                                        double CVI_LR_M = payload[2] / 64.0 * 16.387064;
                                        DescriptionToInsert = "RESPONSE | TCM | LR CLUTCH VOLUME INDEX (CVI)";

                                        if (payload[2] != 0xFF)
                                        {
                                            if (Properties.Settings.Default.Units == "imperial")
                                            {
                                                ValueToInsert = payload[2].ToString("0") + Math.Round(CVI_LR_I, 3).ToString("0.000").Replace(",", ".");
                                                UnitToInsert = "IN^3";
                                            }
                                            else if (Properties.Settings.Default.Units == "metric")
                                            {
                                                ValueToInsert = payload[2].ToString("0") + " = " + Math.Round(CVI_LR_M, 3).ToString("0.000").Replace(",", ".");
                                                UnitToInsert = "CM^3";
                                            }
                                        }
                                        else
                                        {
                                            ValueToInsert = "ERROR";
                                        }
                                    }

                                    if (Transmission24CVIRequested)
                                    {
                                        Transmission24CVIRequested = false;
                                        double CVI_24_I = payload[2] / 64.0;
                                        double CVI_24_M = payload[2] / 64.0 * 16.387064;
                                        DescriptionToInsert = "RESPONSE | TCM | 24 CLUTCH VOLUME INDEX (CVI)";

                                        if (payload[2] != 0xFF)
                                        {
                                            if (Properties.Settings.Default.Units == "imperial")
                                            {
                                                ValueToInsert = payload[2].ToString("0") + " = " + Math.Round(CVI_24_I, 3).ToString("0.000").Replace(",", ".");
                                                UnitToInsert = "IN^3";
                                            }
                                            else if (Properties.Settings.Default.Units == "metric")
                                            {
                                                ValueToInsert = payload[2].ToString("0") + " = " + Math.Round(CVI_24_M, 3).ToString("0.000").Replace(",", ".");
                                                UnitToInsert = "CM^3";
                                            }
                                        }
                                        else
                                        {
                                            ValueToInsert = "ERROR";
                                        }
                                    }

                                    if (TransmissionODCVIRequested)
                                    {
                                        TransmissionODCVIRequested = false;
                                        double CVI_OD_I = payload[2] / 64.0;
                                        double CVI_OD_M = payload[2] / 64.0 * 16.387064;
                                        DescriptionToInsert = "RESPONSE | TCM | OD CLUTCH VOLUME INDEX (CVI)";

                                        if (payload[2] != 0xFF)
                                        {
                                            if (Properties.Settings.Default.Units == "imperial")
                                            {
                                                ValueToInsert = payload[2].ToString("0") + " = " + Math.Round(CVI_OD_I, 3).ToString("0.000").Replace(",", ".");
                                                UnitToInsert = "IN^3";
                                            }
                                            else if (Properties.Settings.Default.Units == "metric")
                                            {
                                                ValueToInsert = payload[2].ToString("0") + " = " + Math.Round(CVI_OD_M, 3).ToString("0.000").Replace(",", ".");
                                                UnitToInsert = "CM^3";
                                            }
                                        }
                                        else
                                        {
                                            ValueToInsert = "ERROR";
                                        }
                                    }

                                    if (TransmissionUDCVIRequested)
                                    {
                                        TransmissionUDCVIRequested = false;
                                        double CVI_UD_I = payload[2] / 64.0;
                                        double CVI_UD_M = payload[2] / 64.0 * 16.387064;
                                        DescriptionToInsert = "RESPONSE | TCM | UD CLUTCH VOLUME INDEX (CVI)";

                                        if (payload[2] != 0xFF)
                                        {
                                            if (Properties.Settings.Default.Units == "imperial")
                                            {
                                                ValueToInsert = payload[2].ToString("0") + " = " + Math.Round(CVI_UD_I, 3).ToString("0.000").Replace(",", ".");
                                                UnitToInsert = "IN^3";
                                            }
                                            else if (Properties.Settings.Default.Units == "metric")
                                            {
                                                ValueToInsert = payload[2].ToString("0") + " = " + Math.Round(CVI_UD_M, 3).ToString("0.000").Replace(",", ".");
                                                UnitToInsert = "CM^3";
                                            }
                                        }
                                        else
                                        {
                                            ValueToInsert = "ERROR";
                                        }
                                    }

                                    if (TransmissionTemperatureRequested)
                                    {
                                        TransmissionTemperatureRequested = false;
                                        DescriptionToInsert = "RESPONSE | TCM | TRANSMISSION TEMPERATURE";

                                        if (payload[2] != 0xFF)
                                        {
                                            double TransmissionTemperatureF = ((payload[2] << 8) + payload[3]) * 0.0156;
                                            double TransmissionTemperatureC = (((payload[2] << 8) + payload[3]) * 0.0156 * 0.555556) - 17.77778;

                                            if (Properties.Settings.Default.Units == "imperial")
                                            {
                                                ValueToInsert = Math.Round(TransmissionTemperatureF, 1).ToString("0.0").Replace(",", ".");
                                                UnitToInsert = "°F";
                                            }
                                            else if (Properties.Settings.Default.Units == "metric")
                                            {
                                                ValueToInsert = Math.Round(TransmissionTemperatureC, 1).ToString("0.0").Replace(",", ".");
                                                UnitToInsert = "°C";
                                            }
                                        }
                                        else
                                        {
                                            ValueToInsert = "ERROR";
                                        }
                                    }

                                    break;
                                }
                                case 0xFF: // command error
                                {
                                    DescriptionToInsert = "RESPONSE | TCM | COMMAND ERROR";
                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                    break;
                                }
                                default:
                                {
                                    DescriptionToInsert = "RESPONSE | TCM | COMMAND: " + Util.ByteToHexString(payload, 1, 1);
                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                    break;
                                }
                            }
                            break;
                        }
                        case 0x43: // ABS - Antilock Brake System
                        {
                            switch (payload[1]) // command
                            {
                                case 0x00: // reset
                                {
                                    DescriptionToInsert = "RESPONSE | ANTILOCK BRAKE SYSTEM | RESET COMPLETE";
                                    break;
                                }
                                case 0xFF: // command error
                                {
                                    DescriptionToInsert = "RESPONSE | ABS | COMMAND ERROR";
                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                    break;
                                }
                                default:
                                {
                                    DescriptionToInsert = "RESPONSE | ABS | COMMAND: " + Util.ByteToHexString(payload, 1, 1);
                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                    break;
                                }
                            }
                            break;
                        }
                        case 0x50: // HVAC - Heat Vent Air Conditioning
                        {
                            switch (payload[1]) // command
                            {
                                case 0x00: // reset
                                {
                                    DescriptionToInsert = "RESPONSE | HVAC | RESET COMPLETE";
                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                    break;
                                }
                                case 0xFF: // command error
                                {
                                    DescriptionToInsert = "RESPONSE | HVAC | COMMAND ERROR";
                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                    break;
                                }
                                default:
                                {
                                    DescriptionToInsert = "RESPONSE | HVAC | COMMAND: " + Util.ByteToHexString(payload, 1, 1);
                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                    break;
                                }
                            }
                            break;
                        }
                        case 0x80: // DDM - Driver Door Module
                        {
                            switch (payload[1]) // command
                            {
                                case 0x00: // reset
                                {
                                    DescriptionToInsert = "RESPONSE | DRIVER DOOR MODULE | RESET COMPLETE";
                                    break;
                                }
                                case 0xFF: // command error
                                {
                                    DescriptionToInsert = "RESPONSE | DDM | COMMAND ERROR";
                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                    break;
                                }
                                default:
                                {
                                    DescriptionToInsert = "RESPONSE | DDM | COMMAND: " + Util.ByteToHexString(payload, 1, 1);
                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                    break;
                                }
                            }
                            break;
                        }
                        case 0x81: // PDM - Passenger Door Module
                        {
                            switch (payload[1]) // command
                            {
                                case 0x00: // reset
                                {
                                    DescriptionToInsert = "RESPONSE | PASSENGER DOOR MODULE | RESET COMPLETE";
                                    break;
                                }
                                case 0xFF: // command error
                                {
                                    DescriptionToInsert = "RESPONSE | PDM | COMMAND ERROR";
                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                    break;
                                }
                                default:
                                {
                                    DescriptionToInsert = "RESPONSE | PDM | COMMAND " + Util.ByteToHexString(payload, 1, 1);
                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                    break;
                                }
                            }
                            break;
                        }
                        case 0x82: // MSM - Memory Seat Module
                        {
                            switch (payload[1]) // command
                            {
                                case 0x00: // reset
                                {
                                    DescriptionToInsert = "RESPONSE | MEMORY SEAT MODULE | RESET COMPLETE";
                                    break;
                                }
                                case 0xFF: // command error
                                {
                                    DescriptionToInsert = "RESPONSE | MSM | COMMAND ERROR";
                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                    break;
                                }
                                default:
                                {
                                    DescriptionToInsert = "RESPONSE | MSM | COMMAND: " + Util.ByteToHexString(payload, 1, 1);
                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                    break;
                                }
                            }
                            break;
                        }
                        case 0x96: // ASM - Audio System Module
                        {
                            switch (payload[1]) // command
                            {
                                case 0x00: // reset
                                {
                                    DescriptionToInsert = "RESPONSE | AUDIO SYSTEM MODULE | RESET COMPLETE";
                                    break;
                                }
                                case 0xFF: // command error
                                {
                                    DescriptionToInsert = "RESPONSE | ASM | COMMAND ERROR";
                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                    break;
                                }
                                default:
                                {
                                    DescriptionToInsert = "RESPONSE | ASM | COMMAND: " + Util.ByteToHexString(payload, 1, 1);
                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                    break;
                                }
                            }
                            break;
                        }
                        case 0xC0: // SKIM - Sentry Key Immobilizer Module
                        {
                            switch (payload[1]) // command
                            {
                                case 0x00: // reset
                                {
                                    DescriptionToInsert = "RESPONSE | SKIM | RESET COMPLETE";
                                    break;
                                }
                                case 0xFF: // command error
                                {
                                    DescriptionToInsert = "RESPONSE | SKIM | COMMAND ERROR";
                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                    break;
                                }
                                default:
                                {
                                    DescriptionToInsert = "RESPONSE | SKIM | COMMAND: " + Util.ByteToHexString(payload, 1, 1);
                                    ValueToInsert = Util.ByteToHexString(payload, 2, 2);
                                    break;
                                }
                            }
                            break;
                        }
                        default:
                        {
                            DescriptionToInsert = "RESPONSE | MODULE: " + Util.ByteToHexStringSimple(new byte[1] { payload[0] }) + " | COMMAND: " + Util.ByteToHexStringSimple(new byte[1] { payload[1] }) + " | PARAMS: " + Util.ByteToHexStringSimple(new byte[2] { payload[2], payload[3] });
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

                    File.AppendAllText(MainForm.CCDB2F2LogFilename, "CCD: " + Util.ByteToHexStringSimple(message) + Environment.NewLine);
                    break;
                }
                case 0xF3:
                {
                    DescriptionToInsert = "SWITCH MESSAGE";

                    if (message.Length < 4) break;

                    // Empty.

                    break;
                }
                case 0xF5:
                {
                    DescriptionToInsert = "ENGINE LAMP CTRL";

                    if (message.Length < 4) break;

                    if (Util.IsBitSet(payload[0], 0))
                    {
                        ValueToInsert = "CEL ON"; // check engine lamp on
                    }

                    break;
                }
                case 0xF6:
                {
                    DescriptionToInsert = "PCM BEACON F6";

                    if (message.Length < 4) break;

                    BeaconNote = BeaconNote.Remove(0x06, 2).Insert(0x06, ((char)payload[0]).ToString() + ((char)payload[1]).ToString());
                    ValueToInsert = Util.ByteToHexString(payload, 0, 2);
                    break;
                }
                case 0xFD:
                {
                    DescriptionToInsert = "COMPASS COMP. AND TEMPERATURE DATA RECEIVED";

                    if (message.Length < 4) break;

                    // Empty.

                    break;
                }
                case 0xFE:
                {
                    DescriptionToInsert = "INTERIOR LAMP DIMMING";

                    if (message.Length < 3) break;

                    double DimmingPercentage = payload[0] * 0.3921568627;

                    ValueToInsert = Math.Round(DimmingPercentage, 1).ToString("0.0").Replace(",", ".");
                    UnitToInsert = "PERCENT";
                    break;
                }
                case 0xFF:
                {
                    DescriptionToInsert = "CCD-BUS WAKE UP";
                    break;
                }
                default:
                {
                    DescriptionToInsert = string.Empty;
                    break;
                }
            }

            string HexBytesToInsert;

            if (message.Length < 9) // max 8 message byte fits the message column
            {
                HexBytesToInsert = Util.ByteToHexString(message, 0, message.Length) + " ";
            }
            else // Trim message (display 7 bytes only) and insert two dots at the end indicating there's more to it
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

            ushort modifiedID; // extends ordinary ID with additional index for sorting

            if (ID == 0x94) // a single gauge position message holds many gauge positions
            {
                if (payload.Length > 1) modifiedID = (ushort)(((ID << 8) & 0xFF00) + (payload[1] & 0x0F)); // add gauge number after ID (94 00, 94 01, 94 02...)
                else modifiedID = (ushort)((ID << 8) & 0xFF00); // keep ID byte only (94 00)
            }
            else if (ID == 0xC2) // SKIM modes
            {
                if (payload.Length > 0) modifiedID = (ushort)(((ID << 8) & 0xFF00) + payload[0]);
                else modifiedID = (ushort)((ID << 8) & 0xFF00);
            }
            else
            {
                modifiedID = (ushort)((ID << 8) & 0xFF00); // keep ID byte only (XX 00)
            }

            Diagnostics.AddRow(modifiedID, RowToAdd.ToString());

            UpdateHeader();

            if (Properties.Settings.Default.Timestamp == true)
            {
                TimeSpan ElapsedTime = TimeSpan.FromMilliseconds(timestamp[0] << 24 | timestamp[1] << 16 | timestamp[2] << 8 | timestamp[3]);
                DateTime Timestamp = DateTime.Today.Add(ElapsedTime);
                string TimestampString = Timestamp.ToString("HH:mm:ss.fff") + ",";
                File.AppendAllText(MainForm.CCDLogFilename, TimestampString); // no newline is appended!
            }

            File.AppendAllText(MainForm.CCDLogFilename, "CCD," + Util.ByteToHexStringSimple(message) + Environment.NewLine);
        }
    }
}
