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

        public string HeaderUnknown  = "│ PCI-BUS MODULES         │ STATE: N/A                                                                                   ";
        public string HeaderDisabled = "│ PCI-BUS MODULES         │ STATE: DISABLED                                                                              ";
        public string HeaderEnabled  = "│ PCI-BUS MODULES         │ STATE: ENABLED @ BAUD | LOGIC: | ID BYTES:                                                   ";
        public string EmptyLine =      "│                         │                                                     │                         │             │";
        public string HeaderModified = string.Empty;

        public string VIN = "-----------------"; // 17 characters

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

            if (ID == 0xFF)
            {
                if (payload.Length > 1) modifiedID = (ushort)(((ID << 8) & 0xFF00) + (payload[1] & 0x0F)); // add gauge number after ID (94 00, 94 01, 94 02...)
                else modifiedID = (ushort)((ID << 8) & 0xFF00); // keep ID byte only (94 00)
            }
            else
            {
                modifiedID = (ushort)((ID << 8) & 0xFF00); // keep ID byte only (XX 00)
            }

            string DescriptionToInsert;
            string ValueToInsert = string.Empty;
            string UnitToInsert = string.Empty;

            switch (ID)
            {
                case 0x10:
                    DescriptionToInsert = "ENGINE SPEED | VEHICLE SPEED | MAP";

                    if (message.Length >= 7)
                    {
                        double EngineSpeed = ((payload[0] << 8) + payload[1]) * 0.25;
                        double VehicleSpeedMPH = ((payload[2] << 8) + payload[3]) * 0.0049;
                        double VehicleSpeedKMH = ((payload[2] << 8) + payload[3]) * 0.0049 * 1.609344;
                        byte MAPKPA = payload[4];
                        double MAPPSI = payload[4] * 0.14504;

                        if (Properties.Settings.Default.Units == "imperial")
                        {
                            DescriptionToInsert = "ENGINE: " + Math.Round(EngineSpeed, 1).ToString("0.0").Replace(",", ".") + " RPM | VEHICLE: " + Math.Round(VehicleSpeedMPH, 1).ToString("0.0").Replace(",", ".") + " MPH";
                            ValueToInsert = "MAP: " + Math.Round(MAPPSI, 1).ToString("0.0").Replace(",", ".") + " PSI";
                        }
                        else if (Properties.Settings.Default.Units == "metric")
                        {
                            DescriptionToInsert = "ENGINE: " + Math.Round(EngineSpeed, 1).ToString("0.0").Replace(",", ".") + " RPM | VEHICLE: " + Math.Round(VehicleSpeedKMH, 1).ToString("0.0").Replace(",", ".") + " KM/H";
                            ValueToInsert = "MAP: " + MAPKPA.ToString("0") + " KPA";
                        }
                    }
                    break;
                case 0x14:
                    DescriptionToInsert = "VEHICLE SPEED SENSOR";

                    if (message.Length >= 4)
                    {
                        ushort DistancePulse = (ushort)((payload[0] << 8) + payload[1]);
                        double VehicleSpeedMPH = 28800.0 / DistancePulse;
                        double VehicleSpeedKMH = 28800.0 / DistancePulse * 1.609344;

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
                    }
                    break;
                case 0x1A:
                    DescriptionToInsert = "TPS | CRUISE SPEED | N/A | TARGET IDLE";

                    if (message.Length >= 6)
                    {
                        double TPSVolts = payload[0] * 0.0191;
                        byte CruiseSpeedKMH = payload[1];
                        double CruiseSpeedMPH = (byte)(payload[1] * 1.609344);
                        double TargetIdle = payload[3] * 0.25 * 32.0;

                        if (Properties.Settings.Default.Units == "imperial")
                        {
                            DescriptionToInsert = "TPS: " + Math.Round(TPSVolts, 3).ToString("0.000").Replace(",", ".") + " V | CRUISE SPEED: " + Math.Round(CruiseSpeedMPH).ToString("0") + " MPH | " + "N/A: " + Util.ByteToHexString(payload, 2, 1);
                            ValueToInsert = "TARGET IDLE: " + Math.Round(TargetIdle).ToString("0") + " RPM";
                        }
                        else if (Properties.Settings.Default.Units == "metric")
                        {
                            DescriptionToInsert = "TPS: " + Math.Round(TPSVolts, 3).ToString("0.000").Replace(",", ".") + " V | CRUISE SPEED: " + CruiseSpeedKMH.ToString("0") + " KM/H | " + "N/A: " + Util.ByteToHexString(payload, 2, 1);
                            ValueToInsert = "TARGET IDLE: " + Math.Round(TargetIdle).ToString("0") + " RPM";
                        }
                    }
                    break;
                case 0x1F:
                    DescriptionToInsert = "INSTRUMENT CLUSTER STATUS";

                    if (message.Length >= 4)
                    {
                        ValueToInsert = Util.ByteToHexString(payload, 0, 2);
                    }
                    break;
                case 0x2B:
                    DescriptionToInsert = "AUTOMATIC TEMPERATURE CONTROL STATUS";

                    if (message.Length >= 5)
                    {
                        ValueToInsert = Util.ByteToHexString(payload, 0, 3);
                    }
                    break;
                case 0x2D:
                    DescriptionToInsert = "INSTRUMENT CLUSTER LAMP STATUS";

                    if (message.Length >= 4)
                    {
                        ValueToInsert = Util.ByteToHexString(payload, 0, 2);
                    }
                    break;
                case 0x33:
                    DescriptionToInsert = "SEAT BELT SWITCH";

                    if (message.Length >= 3)
                    {
                        if (Util.IsBitSet(payload[0], 0)) ValueToInsert = "CLOSED";
                        else ValueToInsert = "OPEN";
                    }
                    break;
                case 0x35:
                    DescriptionToInsert = "MIC LAMP STATUS";

                    if (message.Length >= 4)
                    {
                        ValueToInsert = Util.ByteToHexString(payload, 0, 2);
                    }
                    break;
                case 0x37:
                    DescriptionToInsert = "SHIFT LEVER POSITION";

                    if (message.Length >= 4)
                    {
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
                                ValueToInsert = "AUTOSHIFT";
                                break;
                            default:
                                ValueToInsert = "UNDEFINED";
                                break;
                        }
                    }
                    break;
                case 0x4F:
                    DescriptionToInsert = "VEHICLE THEFT ALARM STATUS";

                    if (message.Length >= 7)
                    {
                        ValueToInsert = Util.ByteToHexString(payload, 0, 5);
                    }
                    break;
                case 0x52:
                    DescriptionToInsert = "A/C RELAY STATE";

                    if (message.Length >= 3)
                    {
                        ValueToInsert = Convert.ToString(payload[0], 2).PadLeft(8, '0');
                    }
                    break;
                case 0x5A:
                    DescriptionToInsert = "IGNITION SWITCH STATUS";

                    if (message.Length >= 5)
                    {
                        Util.ByteToHexString(payload, 0, 3);
                    }
                    break;
                case 0x5D:
                    DescriptionToInsert = "MILEAGE INCREMENT | N/A";

                    if (message.Length >= 7)
                    {
                        double MileageIncrementMi = payload[0] * 0.000125;
                        double MileageIncrementKm = payload[0] * 0.000125 * 1.609344;

                        if (Properties.Settings.Default.Units == "imperial")
                        {
                            ValueToInsert = Math.Round(MileageIncrementMi, 6).ToString("0.000000").Replace(",", ".") + " | " + Util.ByteToHexStringSimple(new byte[4] { payload[1], payload[2], payload[3], payload[4] });
                            UnitToInsert = "MI | N/A";
                        }
                        else if (Properties.Settings.Default.Units == "metric")
                        {
                            ValueToInsert = Math.Round(MileageIncrementKm, 6).ToString("0.000000").Replace(",", ".") + " | " + Util.ByteToHexStringSimple(new byte[4] { payload[1], payload[2], payload[3], payload[4] });
                            UnitToInsert = "KM | N/A";
                        }
                    }
                    break;
                case 0x60:
                    DescriptionToInsert = "AUTO HEAD LAMP STATUS 1";

                    if (message.Length >= 4)
                    {
                        Util.ByteToHexString(payload, 0, 2);
                    }
                    break;
                case 0x72:
                    DescriptionToInsert = "VEHICLE DISTANCE / ODOMETER";

                    if (message.Length >= 6)
                    {
                        double OdometerMi = (uint)(payload[0] << 24 | payload[1] << 16 | payload[2] << 8 | payload[3]) * 0.000125;
                        double OdometerKm = (uint)(payload[0] << 24 | payload[1] << 16 | payload[2] << 8 | payload[3]) * 0.000125 * 1.609344;

                        if (Properties.Settings.Default.Units == "imperial")
                        {
                            ValueToInsert = Math.Round(OdometerMi, 3).ToString("0.000").Replace(",", ".");
                            UnitToInsert = "MILE";
                        }
                        else if (Properties.Settings.Default.Units == "metric")
                        {
                            ValueToInsert = Math.Round(OdometerKm, 3).ToString("0.000").Replace(",", ".");
                            UnitToInsert = "KILOMETER";
                        }
                    }
                    break;
                case 0x8D:
                    DescriptionToInsert = "RADIO STATUS";

                    if (message.Length >= 4)
                    {
                        Util.ByteToHexString(payload, 0, 2);
                    }
                    break;
                case 0xA0:
                    DescriptionToInsert = "DISTANCE TO EMPTY";

                    if (message.Length >= 4)
                    {
                        double DTEMi = ((payload[0] << 8) + payload[1]) * 0.1;
                        double DTEKm = ((payload[0] << 8) + payload[1]) * 0.1 * 1.609344;

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
                    }
                    break;
                case 0xA4:
                    DescriptionToInsert = "FUEL LEVEL";

                    if (message.Length >= 3)
                    {
                        double FuelLevelPercent = payload[0] * 0.3922;
                        ValueToInsert = Math.Round(FuelLevelPercent, 1).ToString("0.0").Replace(",", ".");
                        UnitToInsert = "PERCENT";
                    }
                    break;
                case 0xA5:
                    DescriptionToInsert = "FUEL LEVEL SENSOR VOLTAGE | FUEL LEVEL";

                    if (message.Length >= 4)
                    {
                        double FuelLevelSensorVolts = payload[0] * 0.0191;
                        double FuelLevelG = payload[1] * 0.125;
                        double FuelLevelL = payload[1] * 0.125 * 3.785412;

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
                    }
                    break;
                case 0xAC:
                    DescriptionToInsert = "RADIO CLOCK DISPLAY";

                    if (message.Length >= 4)
                    {
                        Util.ByteToHexString(payload, 0, 2);
                    }
                    break;
                case 0xB0:
                    DescriptionToInsert = "CHECK ENGINE LAMP STATE";

                    if (message.Length >= 5)
                    {
                        if (Util.IsBitSet(payload[0], 7))
                        {
                            ValueToInsert = "ON";
                        }
                        else
                        {
                            ValueToInsert = "OFF";
                        }
                    }
                    break;
                case 0xB1:
                    DescriptionToInsert = "SKIM STATUS";

                    if (message.Length >= 3)
                    {
                        List<string> WarningList = new List<string>();
                        WarningList.Clear();

                        if (payload[0] == 0)
                        {
                            ValueToInsert = "NO WARNING";
                        }
                        else
                        {
                            if (Util.IsBitSet(payload[0], 0)) WarningList.Add("WARNING");
                            if (Util.IsBitSet(payload[0], 1)) WarningList.Add("FAILURE");

                            foreach (string s in WarningList)
                            {
                                ValueToInsert += s + " | ";
                            }

                            if (ValueToInsert.Length > 2) ValueToInsert = ValueToInsert.Remove(ValueToInsert.Length - 3); // remove last "|" character
                        }
                    }
                    break;
                case 0xB8:
                    DescriptionToInsert = "AIRBAG STATUS";

                    if (message.Length >= 4)
                    {
                        Util.ByteToHexString(payload, 0, 2);
                    }
                    break;
                case 0xC0:
                    DescriptionToInsert = "BATTERY | OIL | COOLANT | AMBIENT";

                    if (message.Length >= 6)
                    {
                        double BatteryVoltage = payload[0] * 0.0625;
                        double OilPressurePSI = payload[1] * 0.5;
                        double OilPressureKPA = payload[1] * 0.5 * 6.894757;
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
                    }
                    break;
                case 0xCC:
                    DescriptionToInsert = "OUTSIDE AIR TEMPERATURE | OAT SENSOR VOLTAGE";

                    if (message.Length >= 4)
                    {
                        double OutsideAirTemperatureC = payload[0] - 70.0;
                        double OutsideAirTemperatureF = 1.8 * OutsideAirTemperatureC + 32.0;
                        double OATSensorVoltage = payload[1] * 0.0196;

                        if (Properties.Settings.Default.Units == "imperial")
                        {
                            ValueToInsert = Math.Round(OutsideAirTemperatureF).ToString("0") + " | " + Math.Round(OATSensorVoltage, 3).ToString("0.000").Replace(",", ".");
                            UnitToInsert = "°F | V";
                        }
                        else if (Properties.Settings.Default.Units == "metric")
                        {
                            ValueToInsert = Math.Round(OutsideAirTemperatureC).ToString("0") + " | " + Math.Round(OATSensorVoltage, 3).ToString("0.000").Replace(",", ".");
                            UnitToInsert = "°C | V";
                        }
                    }
                    break;
                case 0xD0:
                    DescriptionToInsert = "LIMP STATE 1";

                    if (message.Length >= 4)
                    {
                        ValueToInsert = Util.ByteToHexString(payload, 0, 2);
                    }
                    break;
                case 0xD1:
                    DescriptionToInsert = "LIMP STATE 2";

                    if (message.Length >= 7)
                    {
                        ValueToInsert = Util.ByteToHexString(payload, 0, 5);
                    }
                    break;
                case 0xD2:
                    DescriptionToInsert = "BARO | IAT | A/C HSP | ETHANOL PERCENT";

                    if (message.Length >= 6)
                    {
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
                    }
                    break;
                case 0xDF:
                    DescriptionToInsert = "ACCUMULATED MILEAGE";

                    if (message.Length >= 3)
                    {
                        ValueToInsert = Util.ByteToHexString(payload, 0, 1);
                    }
                    break;
                case 0xE4:
                    DescriptionToInsert = "AUTO HEAD LAMP STATUS 2";

                    if (message.Length >= 4)
                    {
                        Util.ByteToHexString(payload, 0, 2);
                    }
                    break;
                case 0xEA:
                    DescriptionToInsert = "TRANSMISSION TEMPERATURE";

                    if (message.Length >= 3)
                    {
                        double TransmissionTemperatureC = payload[0] - 64;
                        double TransmissionTemperatureF = 1.8 * TransmissionTemperatureC + 32.0;

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
                    }

                    break;
                case 0xED:
                    DescriptionToInsert = "CONFIGURATION | CRBFUL ENGDSP CYLVPC SALENG BSTYLE";

                    if (message.Length >= 7)
                    {
                        ValueToInsert = Util.ByteToHexString(payload, 0, 5);
                    }
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

            string hexBytesToInsert;

            if (message.Length < 9) // max 8 message byte fits the message column
            {
                hexBytesToInsert = Util.ByteToHexString(message, 0, message.Length) + " ";
            }
            else // Trim message (display 7 bytes only) and insert two dots at the end indicating there's more to it
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

            UpdateHeader();

            if (Properties.Settings.Default.Timestamp == true)
            {
                TimeSpan ElapsedTime = TimeSpan.FromMilliseconds(timestamp[0] << 24 | timestamp[1] << 16 | timestamp[2] << 8 | timestamp[3]);
                DateTime Timestamp = DateTime.Today.Add(ElapsedTime);
                string TimestampString = Timestamp.ToString("HH:mm:ss.fff") + " ";
                File.AppendAllText(MainForm.PCILogFilename, TimestampString); // no newline is appended!
            }

            File.AppendAllText(MainForm.PCILogFilename, Util.ByteToHexStringSimple(message) + Environment.NewLine);
        }
    }
}
