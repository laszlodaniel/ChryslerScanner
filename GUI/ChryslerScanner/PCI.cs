using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ChryslerScanner
{
    public class PCI
    {
        public PCIDiagnosticsTable Diagnostics = new PCIDiagnosticsTable();

        private const int HexBytesColumnStart = 2;
        private const int DescriptionColumnStart = 28;
        private const int ValueColumnStart = 82;
        private const int UnitColumnStart = 108;
        private string state = string.Empty;
        private string speed = "10416 baud";
        private string logic = "non-inverted";

        public string HeaderUnknown  = "│ PCI-BUS (SAE J1850 VPW) │ STATE: N/A                                                                                   ";
        public string HeaderDisabled = "│ PCI-BUS (SAE J1850 VPW) │ STATE: DISABLED                                                                              ";
        public string HeaderEnabled  = "│ PCI-BUS (SAE J1850 VPW) │ STATE: ENABLED @ BAUD | LOGIC: | ID BYTES:                                                   ";
        public string EmptyLine =      "│                         │                                                     │                         │             │";
        public string HeaderModified = string.Empty;

        public string VIN = "-----------------"; // 17 characters

        private byte[] SKIMPayload = new byte[5] { 0, 0, 0, 0, 0 };
        private byte[] SKIMPayloadPCM = new byte[5] { 0, 0, 0, 0, 0 };

        public PCI()
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
                HeaderModified = HeaderEnabled.Replace("@ BAUD", "@ " + this.speed.ToUpper()).Replace("LOGIC:", "LOGIC: " + this.logic.ToUpper()).Replace("ID BYTES: ", "ID BYTES: " + (Diagnostics.UniqueIDByteList.Count + Diagnostics._2426IDByteList.Count).ToString());
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

            List<string> Status = new List<string>();
            string StatusString = string.Empty;

            string DescriptionToInsert;
            string ValueToInsert = string.Empty;
            string UnitToInsert = string.Empty;
            byte ID = message[0];

            switch (ID)
            {
                case 0x0A:
                    DescriptionToInsert = "AIRBAG LAMP REQUEST";

                    if (message.Length < 4) break;

                    break;
                case 0x10:
                    DescriptionToInsert = "ENGINE SPEED | VEHICLE SPEED | MAP";

                    if (message.Length < 7) break;

                    double EngineSpeed = ((payload[0] << 8) + payload[1]) * 0.25;
                    double VehicleSpeedMPHA = ((payload[2] << 8) + payload[3]) * 0.0049;
                    double VehicleSpeedKMHA = VehicleSpeedMPHA * 1.609344;
                    byte MAPKPA = payload[4];
                    double MAPPSI = MAPKPA * 0.14504;

                    if (Properties.Settings.Default.Units == "imperial")
                    {
                        DescriptionToInsert = "ENGINE: " + Math.Round(EngineSpeed, 1).ToString("0.0").Replace(",", ".") + " RPM | VEHICLE: " + Math.Round(VehicleSpeedMPHA, 1).ToString("0.0").Replace(",", ".") + " MPH";
                        ValueToInsert = "MAP: " + Math.Round(MAPPSI, 1).ToString("0.0").Replace(",", ".") + " PSI";
                    }
                    else if (Properties.Settings.Default.Units == "metric")
                    {
                        DescriptionToInsert = "ENGINE: " + Math.Round(EngineSpeed, 1).ToString("0.0").Replace(",", ".") + " RPM | VEHICLE: " + Math.Round(VehicleSpeedKMHA, 1).ToString("0.0").Replace(",", ".") + " KM/H";
                        ValueToInsert = "MAP: " + MAPKPA.ToString("0") + " KPA";
                    }
                    break;
                case 0x14:
                    DescriptionToInsert = "VEHICLE SPEED SENSOR";

                    if (message.Length < 4) break;

                    ushort DistancePulse = (ushort)((payload[0] << 8) + payload[1]);
                    double VehicleSpeedMPHB = 28800.0 / DistancePulse;
                    double VehicleSpeedKMHB = VehicleSpeedMPHB * 1.609344;

                    if (Properties.Settings.Default.Units == "imperial")
                    {
                        if (DistancePulse != 0xFFFF)
                        {
                            ValueToInsert = Math.Round(VehicleSpeedMPHB, 1).ToString("0.0").Replace(",", ".");
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
                            ValueToInsert = Math.Round(VehicleSpeedKMHB, 1).ToString("0.0").Replace(",", ".");
                        }
                        else
                        {
                            ValueToInsert = "0.0";
                        }

                        UnitToInsert = "KM/H";
                    }
                    break;
                case 0x16:
                    DescriptionToInsert = "STATUS: ";

                    if (message.Length < 5) break;

                    if ((payload[0] == 0) && (payload[1] == 0))
                    {
                        DescriptionToInsert = "STATUS EMPTY";
                    }
                    else
                    {
                        if (Util.IsBitSet(payload[0], 7)) Status.Add("-7-");
                        if (Util.IsBitSet(payload[0], 6)) Status.Add("-6-");
                        if (Util.IsBitSet(payload[0], 5)) Status.Add("-5-");
                        if (Util.IsBitSet(payload[0], 4)) Status.Add("TFR"); // transmission fan relay
                        if (Util.IsBitSet(payload[0], 3)) Status.Add("-3-");
                        if (Util.IsBitSet(payload[0], 2)) Status.Add("-2-");

                        if (Util.IsBitSet(payload[0], 1) && Util.IsBitSet(payload[0], 1)) Status.Add("TMR"); // torque management response
                        else
                        {
                            if (Util.IsBitSet(payload[0], 1)) Status.Add("TM1"); // torque management response 1
                            if (Util.IsBitSet(payload[0], 0)) Status.Add("TM0"); // torque management response 0
                        }

                        foreach (string s in Status)
                        {
                            StatusString += s + " | ";
                        }

                        if (StatusString.Length > 2) StatusString = StatusString.Remove(StatusString.Length - 3); // remove last "|" character

                        DescriptionToInsert += StatusString;
                    }
                    break;
                case 0x1A:
                    DescriptionToInsert = "TPS | CRUISE SET SPEED | CRUISE STATE | TARGET IDLE";

                    if (message.Length < 6) break;

                    double TPSVolts = payload[0] * 0.0196; // current volts - minimum volts
                    double CruiseSetSpeedKMH = payload[1];
                    double CruiseSetSpeedMPH = CruiseSetSpeedKMH / 1.609344;
                    byte CruiseState = payload[2];
                    double TargetIdle = payload[3] * 32.0 * 0.25;

                    string CruiseStateString = string.Empty;

                    if (CruiseState == 0)
                    {
                        CruiseStateString = Convert.ToString(CruiseState, 2).PadLeft(8, '0');
                    }
                    else
                    {
                        if (Util.IsBitSet(CruiseState, 7)) Status.Add("-7-");
                        if (Util.IsBitSet(CruiseState, 6)) Status.Add("-6-");
                        if (Util.IsBitSet(CruiseState, 5)) Status.Add("-5-");
                        if (Util.IsBitSet(CruiseState, 4)) Status.Add("-4-");
                        if (Util.IsBitSet(CruiseState, 3)) Status.Add("-3-");
                        if (Util.IsBitSet(CruiseState, 2)) Status.Add("BPP"); // brake pedal pressed
                        if (Util.IsBitSet(CruiseState, 1)) Status.Add("CCL"); // cruise control lamp
                        if (Util.IsBitSet(CruiseState, 0)) Status.Add("CCE"); // cruise control engaged

                        foreach (string s in Status)
                        {
                            StatusString += s + " | ";
                        }

                        if (StatusString.Length > 2) StatusString = StatusString.Remove(StatusString.Length - 3); // remove last "|" character
                    }

                    if (Properties.Settings.Default.Units == "imperial")
                    {
                        DescriptionToInsert = "CRUISE SET SPD: " + Math.Round(CruiseSetSpeedMPH, 1).ToString("0.0").Replace(",", ".") + " MPH | STATE: " + StatusString;
                        ValueToInsert = "TARGET IDLE: " + Math.Round(TargetIdle).ToString("0") + " RPM";
                            
                    }
                    else if (Properties.Settings.Default.Units == "metric")
                    {
                        DescriptionToInsert = "CRUISE SET SPD: " + Math.Round(CruiseSetSpeedKMH, 1).ToString("0.0").Replace(",", ".") + " KM/H | STATE: " + CruiseStateString;
                        ValueToInsert = "TARGET IDLE: " + Math.Round(TargetIdle).ToString("0") + " RPM";
                    }

                    UnitToInsert = "TPS: " + Math.Round(TPSVolts, 3).ToString("0.000").Replace(",", ".") + "V";
                    break;
                case 0x1F:
                    DescriptionToInsert = "INSTRUMENT CLUSTER STATUS";

                    if (message.Length < 4) break;

                    ValueToInsert = Util.ByteToHexString(payload, 0, 2);
                    break;
                case 0x24:
                    DescriptionToInsert = "REQUEST  |";

                    if (message.Length < 7) break;

                    // TODO

                    break;
                case 0x25:
                    DescriptionToInsert = "FRONT DOOR AJAR SWITCH";

                    if (message.Length < 4) break;

                    break;
                case 0x26:
                    DescriptionToInsert = "RESPONSE |";

                    if (message.Length < 7) break;

                    // TODO

                    break;
                case 0x2B:
                    DescriptionToInsert = "AUTOMATIC TEMPERATURE CONTROL STATUS";

                    if (message.Length < 5) break;

                    ValueToInsert = Util.ByteToHexString(payload, 0, 3);
                    break;
                case 0x2D:
                    DescriptionToInsert = "INSTRUMENT CLUSTER LAMP STATUS";

                    if (message.Length < 4) break;

                    ValueToInsert = Util.ByteToHexString(payload, 0, 2);
                    break;
                case 0x33:
                    DescriptionToInsert = "SEAT BELT SWITCH";

                    if (message.Length < 3) break;

                    if (Util.IsBitSet(payload[0], 0)) ValueToInsert = "CLOSED";
                    else ValueToInsert = "OPEN";

                    break;
                case 0x35:
                    DescriptionToInsert = "STATUS: ";

                    if (message.Length < 4) break;
                    
                    if ((payload[0] == 0) && (payload[1] == 0))
                    {
                        DescriptionToInsert += "ATX";
                        break;
                    }

                    if (Util.IsBitSet(payload[1], 7)) Status.Add("MTX"); // manual transmission
                    else Status.Add("ATX"); // automatic transmission

                    if (Util.IsBitSet(payload[1], 6)) Status.Add("-6-");
                    if (Util.IsBitSet(payload[1], 5)) Status.Add("-5-");
                    if (Util.IsBitSet(payload[1], 4)) Status.Add("-4-");
                    if (Util.IsBitSet(payload[1], 3)) Status.Add("ACT"); // A/C clutch
                    if (Util.IsBitSet(payload[1], 2)) Status.Add("BPP"); // break pedal pressed
                    if (Util.IsBitSet(payload[1], 1)) Status.Add("TPP"); // throttle pedal pressed
                    if (Util.IsBitSet(payload[1], 0)) Status.Add("CCE"); // cruise control engaged

                    if (Util.IsBitSet(payload[0], 7)) Status.Add("-7-");
                    if (Util.IsBitSet(payload[0], 6)) Status.Add("-6-");
                    if (Util.IsBitSet(payload[0], 5)) Status.Add("-5-");
                    if (Util.IsBitSet(payload[0], 4)) Status.Add("-4-");
                    if (Util.IsBitSet(payload[0], 3)) Status.Add("-3-");
                    if (Util.IsBitSet(payload[0], 2)) Status.Add("CRL"); // cruise lamp
                    if (Util.IsBitSet(payload[0], 1)) Status.Add("-1-");
                    if (Util.IsBitSet(payload[0], 0)) Status.Add("SKF"); // SKIM found

                    foreach (string s in Status)
                    {
                        DescriptionToInsert += s + " | ";
                    }

                    if (DescriptionToInsert.Length > 2) DescriptionToInsert = DescriptionToInsert.Remove(DescriptionToInsert.Length - 3); // remove last "|" character
                    break;
                case 0x37:
                    DescriptionToInsert = "SHIFT LEVER POSITION";

                    if (message.Length < 4) break;

                    switch (payload[0])
                    {
                        case 0x01:
                            ValueToInsert = "PARK";
                            break;
                        case 0x02:
                            ValueToInsert = "REVERSE";
                            break;
                        case 0x03:
                            ValueToInsert = "NEUTRAL";
                            break;
                        case 0x05:
                            ValueToInsert = "DRIVE";
                            break;
                        case 0x06:
                            ValueToInsert = "AUTOSTICK";
                            break;
                        default:
                            ValueToInsert = "UNDEFINED";
                            break;
                    }

                    if ((payload[0] == 0x06) && Util.IsBitSet(payload[1], 7)) // Autostick equipped
                    {
                        switch (payload[1] & 0xF0)
                        {
                            case 0x90:
                                ValueToInsert += " | 1ST";
                                break;
                            case 0xA0:
                                ValueToInsert += " | 2ND";
                                break;
                            case 0xB0:
                                ValueToInsert += " | 3RD";
                                break;
                            case 0xC0:
                                ValueToInsert += " | 4TH";
                                break;
                            default:
                                break;
                        }
                    }
                    break;
                case 0x3A:
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
                            ValueToInsert += "| LOCK: PART";
                            break;
                        case 2:
                            ValueToInsert += "| LOCK: FULL";
                            break;
                        default:
                            break;
                    }
                    break;
                case 0x3F:
                    DescriptionToInsert = "PCM SEED FOR SKIM";

                    if (message.Length < 6) break;

                    ValueToInsert = Util.ByteToHexString(payload, 0, 4);

                    if (VIN.Contains("-")) break;

                    byte[] key = Util.GetSKIMUnlockKey(payload, VIN);
                    byte[] KeyArray = { 0xC2, 0xC0, key[0], key[1], key[2], 0x00 };

                    KeyArray[KeyArray.Length - 1] = Util.ChecksumCalculator(KeyArray, 0, KeyArray.Length - 1);
                    DescriptionToInsert += " | KEY: " + Util.ByteToHexStringSimple(KeyArray);
                    break;
                case 0x42:
                    DescriptionToInsert = "LAST ENGINE SHUTDOWN";

                    if (message.Length < 4) break;

                    uint TimerMinutes = (uint)(payload[0] * 60 + payload[1]);
                    TimeSpan Timestamp = TimeSpan.FromMinutes(TimerMinutes);

                    ValueToInsert = Timestamp.ToString(@"hh\:mm");
                    UnitToInsert = "HH:MM";
                    break;
                case 0x48:
                    DescriptionToInsert = "REQUEST  |"; // OBD2

                    if (message.Length < 7) break;

                    // TODO

                    break;
                case 0x4F:
                    DescriptionToInsert = "SKIM | SEED/KEY VALIDATION";

                    if (message.Length < 7) break;

                    if ((payload[0] != 0xC0) && (payload[0] != 0x40) && (payload[0] != 0x10)) break;

                    if (payload[0] == 0xC0) // seed / key
                    {
                        if (payload[1] != 0)
                        {
                            DescriptionToInsert = "SKIM | REQUEST SEED FROM PCM";
                        }
                        else
                        {
                            DescriptionToInsert = "SKIM | KEY RECEIVED";
                            ValueToInsert = Util.ByteToHexString(payload, 2, 3);
                        }
                    }

                    if (payload[0] == 0x40) // packet received from SKIM to write to PCM EEPROM (DRB3 PCM replaced menu)
                    {
                        switch (payload[1] & 0x03)
                        {
                            case 0x01:
                                DescriptionToInsert = "SKIM | PAYLOAD #1 FROM EEPROM";
                                Array.Copy(payload, 2, SKIMPayload, 0, 3);
                                ValueToInsert = Util.ByteToHexString(payload, 2, 3);
                                break;
                            case 0x02:
                                DescriptionToInsert = "SKIM | PAYLOAD #2 FROM EEPROM";
                                Array.Copy(payload, 2, SKIMPayload, 3, 2);
                                ValueToInsert = Util.ByteToHexStringSimple(SKIMPayload);
                                UnitToInsert = "EEPROM 01D8";
                                break;
                            default:
                                DescriptionToInsert = "SKIM | PAYLOAD TO BE WRITTEN TO PCM EEPROM";
                                ValueToInsert = "INVALID MSG";
                                break;
                        }
                    }

                    if (payload[0] == 0x10) // packet sent to SKIM to write to SKIM EEPROM (DRB3 SKIM replaced menu)
                    {
                        switch (payload[1] & 0x03)
                        {
                            case 0x01:
                                DescriptionToInsert = "SKIM PAYLOAD #1 FROM PCM EEPROM";
                                Array.Copy(payload, 2, SKIMPayloadPCM, 0, 3);
                                ValueToInsert = Util.ByteToHexString(payload, 2, 3);
                                break;
                            case 0x02:
                                DescriptionToInsert = "SKIM PAYLOAD #2 FROM PCM EEPROM";
                                Array.Copy(payload, 2, SKIMPayloadPCM, 3, 2);
                                ValueToInsert = Util.ByteToHexStringSimple(SKIMPayloadPCM);
                                UnitToInsert = "EEPROM 01D8";
                                break;
                            default:
                                DescriptionToInsert = "SKIM PAYLOAD FROM PCM EEPROM";
                                ValueToInsert = "INVALID MSG";
                                break;
                        }
                    }
                    break;
                case 0x52:
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
                case 0x5A:
                    DescriptionToInsert = "IGNITION SWITCH STATUS";

                    if (message.Length < 5) break;

                    ValueToInsert = Util.ByteToHexString(payload, 0, 3);

                    break;
                case 0x5B:
                    DescriptionToInsert = "RUN RELAY";

                    if (message.Length < 4) break;

                    break;
                case 0x5D:
                    DescriptionToInsert = "MILEAGE INCREMENT | INJECTOR PULSE WIDTH";

                    if (message.Length < 7) break;

                    double MileageIncrement = payload[0] * 0.000125;
                    double KilometerageIncrement = MileageIncrement * 1.609344;
                    double InjectorPulseWidth = ((payload[1] << 8) + payload[2]) * 0.00390625;

                    if (Properties.Settings.Default.Units == "imperial")
                    {
                        ValueToInsert = Math.Round(MileageIncrement, 6).ToString("0.000000").Replace(",", ".") + " | " + InjectorPulseWidth.ToString("0.000").Replace(",", ".");
                        UnitToInsert = "MI | MS";
                    }
                    else if (Properties.Settings.Default.Units == "metric")
                    {
                        ValueToInsert = Math.Round(KilometerageIncrement, 6).ToString("0.000000").Replace(",", ".") + " | " + InjectorPulseWidth.ToString("0.000").Replace(",", ".");
                        UnitToInsert = "KM | MS";
                    }
                    break;
                case 0x60:
                    DescriptionToInsert = "AUTO HEAD LAMP STATUS 1";

                    if (message.Length < 4) break;

                    ValueToInsert = Util.ByteToHexString(payload, 0, 2);
                    break;
                case 0x68:
                    DescriptionToInsert = "RESPONSE |"; // OBD2

                    if (message.Length < 6) break;

                    // TODO

                    break;
                case 0x6C:
                    DescriptionToInsert = "TCM | FAULT CODE PRESENT";

                    if (message.Length < 7) break;

                    if ((payload[1] == 0) && (payload[2] == 0))
                    {
                        DescriptionToInsert = "TCM | NO FAULT CODE";
                        break;
                    }

                    ValueToInsert = "OBD2 P" + Util.ByteToHexString(payload, 1, 2).Replace(" ", "");
                    break;
                case 0x6E:
                    DescriptionToInsert = "PCM BEACON PAYLOAD #1";

                    if (message.Length < 7) break;

                    ValueToInsert = Util.ByteToHexString(payload, 0, 5);
                    break;
                case 0x6F:
                    DescriptionToInsert = "PCM BEACON PAYLOAD #2";

                    if (message.Length < 7) break;

                    ValueToInsert = Util.ByteToHexString(payload, 0, 5);
                    break;
                case 0x72:
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
                case 0x87:
                    DescriptionToInsert = "UPDATE BEACON MESSAGE PAYLOAD IN PCM EEPROM";

                    if (message.Length < 4) break;

                    ValueToInsert = Util.ByteToHexString(payload, 0, 2);
                    break;
                case 0x8D:
                    DescriptionToInsert = "RADIO STATUS";

                    if (message.Length < 4) break;

                    ValueToInsert = Util.ByteToHexString(payload, 0, 2);
                    break;
                case 0xA0:
                    DescriptionToInsert = "DISTANCE TO EMPTY";

                    if (message.Length < 4) break;

                    double DTEMi = ((payload[0] << 8) + payload[1]) * 0.1;
                    double DTEKm = DTEMi * 1.609344;

                    if (Properties.Settings.Default.Units == "imperial")
                    {
                        ValueToInsert = Math.Round(DTEMi, 1).ToString("0.0").Replace(",", ".");
                        UnitToInsert = "MILE";
                    }
                    else if (Properties.Settings.Default.Units == "metric")
                    {
                        ValueToInsert = Math.Round(DTEKm, 1).ToString("0.0").Replace(",", ".");
                        UnitToInsert = "KILOMETER";
                    }
                    break;
                case 0xA3:
                    DescriptionToInsert = "AMBIENT TEMPERATURE SENSOR VOLTAGE";

                    if (message.Length < 4) break;

                    double ATSVolts = ((((payload[0] << 8) + payload[1]) >> 2) & 0xFF) * 0.0196;

                    ValueToInsert = Math.Round(ATSVolts, 3).ToString("0.000").Replace(",", ".");
                    UnitToInsert = "V";
                    break;
                case 0xA4:
                    DescriptionToInsert = "FUEL LEVEL";

                    if (message.Length < 3) break;

                    double FuelLevelPercent = payload[0] * 0.3921568627;

                    ValueToInsert = Math.Round(FuelLevelPercent, 1).ToString("0.0").Replace(",", ".");
                    UnitToInsert = "PERCENT";
                    break;
                case 0xA5:
                    DescriptionToInsert = "FUEL LEVEL SENSOR VOLTAGE | FUEL LEVEL";

                    if (message.Length < 4) break;

                    double FuelLevelSensorVolts = payload[0] * 0.0196;
                    double FuelLevelG = payload[1] * 0.125;
                    double FuelLevelL = FuelLevelG * 3.785412;

                    if (Properties.Settings.Default.Units == "imperial")
                    {
                        ValueToInsert = Math.Round(FuelLevelSensorVolts, 3).ToString("0.000").Replace(",", ".") + " | " + Math.Round(FuelLevelG, 1).ToString("0.0").Replace(",", ".");
                        UnitToInsert = "V | GALLON";
                    }
                    else if (Properties.Settings.Default.Units == "metric")
                    {
                        ValueToInsert = Math.Round(FuelLevelSensorVolts, 3).ToString("0.000").Replace(",", ".") + " | " + Math.Round(FuelLevelL, 1).ToString("0.0").Replace(",", ".");
                        UnitToInsert = "V | LITER";
                    }
                    break;
                case 0xA7:
                    DescriptionToInsert = "FOB NUMBER/BUTTON";

                    if (message.Length < 4) break;

                    break;
                case 0xAC:
                    DescriptionToInsert = "RADIO CLOCK DISPLAY";

                    if (message.Length < 4) break;

                    ValueToInsert = Util.ByteToHexString(payload, 0, 1) + ":" + Util.ByteToHexString(payload, 1, 1);
                    UnitToInsert = "HH:MM";
                    break;
                case 0xB0:
                    DescriptionToInsert = "CHECK ENGINE LAMP STATE";

                    if (message.Length < 5) break;

                    if (Util.IsBitSet(payload[0], 7))
                    {
                        ValueToInsert = "ON";
                    }
                    else
                    {
                        ValueToInsert = "OFF";
                    }
                    break;
                case 0xB1:
                    DescriptionToInsert = "SKIM STATUS";

                    if (message.Length < 3) break;

                    if (payload[0] == 0)
                    {
                        ValueToInsert = "NO WARNING";
                    }
                    else
                    {
                        List<string> WarningList = new List<string>();

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
                    }
                    break;
                case 0xB8:
                    DescriptionToInsert = "AIRBAG STATUS";

                    if (message.Length < 4) break;

                    ValueToInsert = Util.ByteToHexString(payload, 0, 2);
                    break;
                case 0xC0:
                    DescriptionToInsert = "BATTERY | OIL | COOLANT | AMBIENT";

                    if (message.Length < 6) break;

                    double BatteryVoltage = payload[0] * 0.0625;
                    double OilPressurePSI = payload[1] * 0.5;
                    double OilPressureKPA = OilPressurePSI * 6.894757;
                    double CoolantTemperatureC = payload[2] - 40;
                    double CoolantTemperatureF = 1.8 * CoolantTemperatureC + 32.0;
                    double AmbientTemperatureC = payload[3] - 40;
                    double AmbientTemperatureF = 1.8 * AmbientTemperatureC + 32.0;

                    if (Properties.Settings.Default.Units == "imperial")
                    {
                        DescriptionToInsert = "BATTERY: " + Math.Round(BatteryVoltage, 1).ToString("0.0").Replace(",", ".") + " V | OIL: " + Math.Round(OilPressurePSI, 1).ToString("0.0").Replace(",", ".") + " PSI | COOLANT: " + Math.Round(CoolantTemperatureF).ToString("0") + " °F";
                        ValueToInsert = "AMBIENT: " + Math.Round(AmbientTemperatureF).ToString("0") + " °F";
                    }
                    else if (Properties.Settings.Default.Units == "metric")
                    {
                        DescriptionToInsert = "BATTERY: " + Math.Round(BatteryVoltage, 1).ToString("0.0").Replace(",", ".") + " V | OIL: " + Math.Round(OilPressureKPA, 1).ToString("0.0").Replace(",", ".") + " KPA | COOLANT: " + CoolantTemperatureC.ToString("0") + " °C";
                        ValueToInsert = "AMBIENT: " + AmbientTemperatureC.ToString("0") + " °C";
                    }
                    break;
                case 0xCC:
                    DescriptionToInsert = "OUTSIDE AIR TEMPERATURE";

                    if (message.Length < 4) break;

                    double OutsideAirTemperatureC = payload[0] - 70.0;
                    double OutsideAirTemperatureF = 1.8 * OutsideAirTemperatureC + 32.0;

                    if (Properties.Settings.Default.Units == "imperial")
                    {
                        ValueToInsert = Math.Round(OutsideAirTemperatureF).ToString("0");
                        UnitToInsert = "°F";
                    }
                    else if (Properties.Settings.Default.Units == "metric")
                    {
                        ValueToInsert = Math.Round(OutsideAirTemperatureC).ToString("0");
                        UnitToInsert = "°C";
                    }
                    break;
                case 0xD0:
                    DescriptionToInsert = "LIMP-IN STATE";

                    if (message.Length < 4) break;

                    List<string> LimpStateA = new List<string>();

                    if (Util.IsBitSet(payload[1], 7)) LimpStateA.Add("ATS"); // Ambient temperature sensor
                    if (Util.IsBitSet(payload[1], 6)) LimpStateA.Add("IAT"); // Intake air temperature
                    if (Util.IsBitSet(payload[1], 5)) LimpStateA.Add("FSM"); // Fuel system?
                    if (Util.IsBitSet(payload[1], 4)) LimpStateA.Add("ACP"); // A/C high-side pressure
                    if (Util.IsBitSet(payload[1], 3)) LimpStateA.Add("CHG"); // Charging system
                    if (Util.IsBitSet(payload[1], 2)) LimpStateA.Add("CHB"); // Battery charging voltage
                    if (Util.IsBitSet(payload[1], 1)) LimpStateA.Add("TPS"); // Throttle position sensor
                    if (Util.IsBitSet(payload[1], 0)) LimpStateA.Add("ECT"); // Engine coolant temperature

                    if (LimpStateA.Count > 0)
                    {
                        DescriptionToInsert = "LIMP: ";

                        foreach (string s in LimpStateA)
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
                case 0xD1:
                    DescriptionToInsert = "LIMP-IN STATE | PWM FAN DUTY CYCLE";

                    if (message.Length < 7) break;

                    List<string> LimpStateB = new List<string>();

                    if (Util.IsBitSet(payload[2], 7)) LimpStateB.Add("ATS"); // Ambient temperature sensor
                    if (Util.IsBitSet(payload[2], 6)) LimpStateB.Add("IAT"); // Intake air temperature
                    if (Util.IsBitSet(payload[2], 5)) LimpStateB.Add("FSM"); // Fuel system?
                    if (Util.IsBitSet(payload[2], 4)) LimpStateB.Add("ACP"); // A/C high-side pressure
                    if (Util.IsBitSet(payload[2], 3)) LimpStateB.Add("CHG"); // Charging system
                    if (Util.IsBitSet(payload[2], 2)) LimpStateB.Add("CHB"); // Battery charging voltage
                    if (Util.IsBitSet(payload[2], 1)) LimpStateB.Add("TPS"); // Throttle position sensor
                    if (Util.IsBitSet(payload[2], 0)) LimpStateB.Add("ECT"); // Engine coolant temperature

                    if (LimpStateB.Count > 0)
                    {
                        DescriptionToInsert = "LIMP: ";

                        foreach (string s in LimpStateB)
                        {
                            DescriptionToInsert += s + " | ";
                        }

                        if (DescriptionToInsert.Length > 2) DescriptionToInsert = DescriptionToInsert.Remove(DescriptionToInsert.Length - 3); // remove last "|" character
                    }
                    else
                    {
                        DescriptionToInsert = "NO LIMP-IN STATE";
                    }

                    double PWMFANDutyCycle = payload[3] * 0.3921568627;

                    ValueToInsert = "PWM FAN DUTY: " + Math.Round(PWMFANDutyCycle, 1).ToString("0.0").Replace(",", ".");
                    UnitToInsert = "PERCENT";
                    break;
                case 0xD2:
                    DescriptionToInsert = "BARO | IAT | A/C HSP | ETHANOL PERCENT";

                    if (message.Length < 6) break;

                    byte BarometricPressureKPA = payload[0];
                    double BarometricPressurePSI = BarometricPressureKPA * 0.14504;
                    double IntakeAirTemperatureC = payload[1] - 40;
                    double IntakeAirTemperatureF = 1.8 * IntakeAirTemperatureC + 32;
                    double ACHighSidePressurePSI = payload[2] * 2.03;
                    double ACHighSidePressureKPA = ACHighSidePressurePSI * 6.894757;
                    double EthanolPercent = payload[3] * 0.5;

                    if (Properties.Settings.Default.Units == "imperial")
                    {
                        DescriptionToInsert = "BARO: " + Math.Round(BarometricPressurePSI, 1).ToString("0.0").Replace(",", ".") + " PSI | IAT: " + Math.Round(IntakeAirTemperatureF).ToString("0") + " °F | A/C HSP: " + Math.Round(ACHighSidePressurePSI, 1).ToString("0.0").Replace(",", ".") + " PSI";
                        ValueToInsert = "ETHANOL: " + Math.Round(EthanolPercent, 1).ToString("0.0").Replace(",", ".") + " PERCENT";
                    }
                    else if (Properties.Settings.Default.Units == "metric")
                    {
                        DescriptionToInsert = "BARO: " + BarometricPressureKPA.ToString("0.0").Replace(",", ".") + " KPA | IAT: " + Math.Round(IntakeAirTemperatureC).ToString("0") + " °C | A/C HSP: " + Math.Round(ACHighSidePressureKPA, 1).ToString("0.0").Replace(",", ".") + " KPA";
                        ValueToInsert = "ETHANOL: " + Math.Round(EthanolPercent, 1).ToString("0.0").Replace(",", ".") + " PERCENT";
                    }
                    break;
                case 0xDF:
                    DescriptionToInsert = "PCM MILEAGE";

                    if (message.Length < 3) break;

                    double Mileage = payload[0] * 256 * 8.192 * 0.25; // miles
                    double Kilometerage = Mileage * 1.609344; // kilometers

                    if (Properties.Settings.Default.Units == "imperial")
                    {
                        ValueToInsert = Math.Round(Mileage).ToString("0");
                        UnitToInsert = "MILE";
                    }
                    else if (Properties.Settings.Default.Units == "metric")
                    {
                        ValueToInsert = Math.Round(Kilometerage).ToString("0");
                        UnitToInsert = "KILOMETER";
                    }
                    break;
                case 0xE4:
                    DescriptionToInsert = "AUTO HEAD LAMP STATUS 2";

                    if (message.Length < 4) break;

                    ValueToInsert = Util.ByteToHexString(payload, 0, 2);
                    break;
                case 0xEA:
                    DescriptionToInsert = "TRANSMISSION TEMPERATURE";

                    if (message.Length < 3) break;

                    double TransTempC = payload[0] - 40;
                    double TransTempF = 1.8 * TransTempC + 32.0;

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
                case 0xED:
                    DescriptionToInsert = "CONFIGURATION | CRBFUL ENGDSP CYLVPC SALENG BSTYLE";

                    if (message.Length < 7) break;

                    ValueToInsert = Util.ByteToHexString(payload, 0, 5);
                    break;
                case 0xF0:
                    DescriptionToInsert = "VEHICLE IDENTIFICATION NUMBER (VIN) CHARACTER";

                    if (((payload[0] == 0x01) && (message.Length >= 4)) || ((payload[0] == 0x02) && (message.Length >= 7)) || ((payload[0] == 0x06) && (message.Length >= 7)) || ((payload[0] == 0x0A) && (message.Length >= 7)) || ((payload[0] == 0x0E) && (message.Length >= 7)))
                    {
                        VIN = VIN.Remove(payload[0] - 1, payload.Length - 1).Insert(payload[0] - 1, Encoding.ASCII.GetString(payload.Skip(1).Take(payload.Length - 1).ToArray()));
                        ValueToInsert = VIN;
                    }
                    break;
                default:
                    DescriptionToInsert = string.Empty;
                    break;
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

            if (ID == 0x4F)
            {
                if (payload.Length > 1) modifiedID = (ushort)(((ID << 8) & 0xFF00) + payload[0]);
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
                string TimestampString = Timestamp.ToString("HH:mm:ss.fff") + " ";
                File.AppendAllText(MainForm.PCILogFilename, TimestampString); // no newline is appended!
            }

            File.AppendAllText(MainForm.PCILogFilename, "PCI: " + Util.ByteToHexStringSimple(message) + Environment.NewLine);
        }
    }
}
