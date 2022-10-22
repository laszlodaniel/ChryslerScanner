using System;
using System.IO;
using System.Linq;
using System.Text;

namespace ChryslerScanner
{
    public class PCI
    {
        public PCIDiagnosticsTable Diagnostics = new PCIDiagnosticsTable();

        private const int hexBytesColumnStart = 2;
        private const int descriptionColumnStart = 28;
        private const int valueColumnStart = 82;
        private const int unitColumnStart = 108;
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

            if (ID == 0xFF)
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
                case 0x10:
                    descriptionToInsert = "ENGINE SPEED | VEHICLE SPEED | MAP SENSOR";

                    if (message.Length >= 7)
                    {
                        double EngineSpeed = (((payload[0] << 8) + payload[1]) * 0.25);
                        double VehicleSpeedMPH = ((payload[2] << 8) + payload[3]) * 0.0049;
                        double VehicleSpeedKMH = ((payload[2] << 8) + payload[3]) * 0.0049 * 1.609344;
                        byte MAPKPA = payload[4];
                        double MAPPSI = payload[4] * 0.14504;

                        if (Properties.Settings.Default.Units == "imperial")
                        {
                            descriptionToInsert = "ENGINE: " + EngineSpeed.ToString("0.00").Replace(",", ".") + " RPM | VEHICLE: " + VehicleSpeedMPH.ToString("0.00").Replace(",", ".") + " MPH";
                            valueToInsert = "MAP: " + MAPPSI.ToString("0.00").Replace(",", ".") + " PSI";
                        }
                        else if (Properties.Settings.Default.Units == "metric")
                        {
                            descriptionToInsert = "ENGINE: " + EngineSpeed.ToString("0.00").Replace(",", ".") + " RPM | VEHICLE: " + VehicleSpeedKMH.ToString("0.00").Replace(",", ".") + " KM/H";
                            valueToInsert = "MAP: " + MAPKPA.ToString("0") + " KPA";
                        }
                    }
                    break;
                case 0x14:
                    descriptionToInsert = "VEHICLE SPEED SENSOR";

                    if (message.Length >= 4)
                    {
                        ushort DistancePulse = (ushort)((payload[0] << 8) + payload[1]);
                        double VehicleSpeedMPH = 28800.0 / DistancePulse;
                        double VehicleSpeedKMH = 28800.0 / DistancePulse * 1.609344;

                        if (Properties.Settings.Default.Units == "imperial")
                        {
                            if (DistancePulse != 0xFFFF)
                            {
                                valueToInsert = Math.Round(VehicleSpeedMPH, 1).ToString("0.0").Replace(",", ".");
                            }
                            else
                            {
                                valueToInsert = "0.0";
                            }

                            unitToInsert = "MPH";
                        }
                        else if (Properties.Settings.Default.Units == "metric")
                        {
                            if (DistancePulse != 0xFFFF)
                            {
                                valueToInsert = Math.Round(VehicleSpeedKMH, 1).ToString("0.0").Replace(",", ".");
                            }
                            else
                            {
                                valueToInsert = "0.0";
                            }

                            unitToInsert = "KM/H";
                        }
                    }
                    break;
                case 0x1A:
                    descriptionToInsert = "TPS | CRUISE SPEED | N/A | TARGET IDLE";

                    if (message.Length >= 6)
                    {
                        double TPSVolts = payload[0] * 0.0191;
                        byte CruiseSpeedKMH = payload[1];
                        byte CruiseSpeedMPH = (byte)(payload[1] * 1.609344);
                        double TargetIdle = payload[3] * 0.25 * 32.0;

                        if (Properties.Settings.Default.Units == "imperial")
                        {
                            descriptionToInsert = "TPS: " + TPSVolts.ToString("0.000").Replace(",", ".") + " V | CRUISE SPEED: " + CruiseSpeedMPH.ToString("0") + " MPH | " + "N/A: " + Util.ByteToHexString(payload, 2, 1);
                            valueToInsert = "TARGET IDLE: " + TargetIdle.ToString("0") + " RPM";
                        }
                        else if (Properties.Settings.Default.Units == "metric")
                        {
                            descriptionToInsert = "TPS: " + TPSVolts.ToString("0.000").Replace(",", ".") + " V | CRUISE SPEED: " + CruiseSpeedKMH.ToString("0") + " KM/H | " + "N/A: " + Util.ByteToHexString(payload, 2, 1);
                            valueToInsert = "TARGET IDLE: " + TargetIdle.ToString("0") + " RPM";
                        }
                    }
                    break;
                case 0x35:
                    descriptionToInsert = "MIC LAMP STATUS";

                    if (message.Length >= 4)
                    {
                        valueToInsert = Util.ByteToHexString(payload, 0, 2);
                    }
                    break;
                case 0x37:
                    descriptionToInsert = "SHIFT LEVER POSITION";

                    if (message.Length >= 4)
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
                case 0x5D:
                    descriptionToInsert = "MILEAGE INCREMENT | N/A";

                    if (message.Length >= 7)
                    {
                        double MileageIncrementMi = payload[0] * 0.000125;
                        double MileageIncrementKm = payload[0] * 0.000125 * 1.609344;

                        if (Properties.Settings.Default.Units == "imperial")
                        {
                            valueToInsert = Math.Round(MileageIncrementMi, 6).ToString("0.000000").Replace(",", ".") + " | " + Util.ByteToHexStringSimple(new byte[4] { payload[1], payload[2], payload[3], payload[4] });
                            unitToInsert = "MI | N/A";
                        }
                        else if (Properties.Settings.Default.Units == "metric")
                        {
                            valueToInsert = Math.Round(MileageIncrementKm, 6).ToString("0.000000").Replace(",", ".") + " | " + Util.ByteToHexStringSimple(new byte[4] { payload[1], payload[2], payload[3], payload[4] });
                            unitToInsert = "KM | N/A";
                        }
                    }
                    break;
                case 0x72:
                    descriptionToInsert = "VEHICLE DISTANCE / ODOMETER";

                    if (message.Length >= 6)
                    {
                        double OdometerMi = (uint)(payload[0] << 24 | payload[1] << 16 | payload[2] << 8 | payload[3]) * 0.000125;
                        double OdometerKm = (uint)(payload[0] << 24 | payload[1] << 16 | payload[2] << 8 | payload[3]) * 0.000125 * 1.609344;

                        if (Properties.Settings.Default.Units == "imperial")
                        {
                            valueToInsert = Math.Round(OdometerMi, 3).ToString("0.000").Replace(",", ".");
                            unitToInsert = "MILE";
                        }
                        else if (Properties.Settings.Default.Units == "metric")
                        {
                            valueToInsert = Math.Round(OdometerKm, 3).ToString("0.000").Replace(",", ".");
                            unitToInsert = "KILOMETER";
                        }
                    }
                    break;
                case 0xA0:
                    descriptionToInsert = "DISTANCE TO EMPTY";

                    if (message.Length >= 4)
                    {
                        double DTEMi = ((payload[0] << 8) + payload[1]) * 0.1;
                        double DTEKm = ((payload[0] << 8) + payload[1]) * 0.1 * 1.609344;

                        if (Properties.Settings.Default.Units == "imperial")
                        {
                            valueToInsert = Math.Round(DTEMi, 1).ToString("0.0").Replace(",", ".");
                            unitToInsert = "MILE";
                        }
                        else if (Properties.Settings.Default.Units == "metric")
                        {
                            valueToInsert = Math.Round(DTEKm, 1).ToString("0.0").Replace(",", ".");
                            unitToInsert = "KILOMETER";
                        }
                    }
                    break;
                case 0xA4:
                    descriptionToInsert = "FUEL LEVEL";

                    if (message.Length >= 3)
                    {
                        double FuelLevelPercent = payload[0] * 0.3922;
                        valueToInsert = Math.Round(FuelLevelPercent, 1).ToString("0.0").Replace(",", ".");
                        unitToInsert = "PERCENT";
                    }
                    break;
                case 0xA5:
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
                case 0xB0:
                    descriptionToInsert = "CHECK ENGINE LAMP STATE";

                    if (message.Length >= 5)
                    {
                        if (Util.IsBitSet(payload[0], 7))
                        {
                            valueToInsert = "ON";
                        }
                        else
                        {
                            valueToInsert = "OFF";
                        }
                    }
                    break;
                case 0xC0:
                    descriptionToInsert = "BATTERY | OIL | COOLANT | IAT";

                    if (message.Length >= 6)
                    {
                        double BatteryVoltage = payload[0] * 0.0625;
                        double OilPressurePSI = payload[1] * 0.5;
                        double OilPressureKPA = payload[1] * 0.5 * 6.894757;
                        double CoolantTemperatureC = payload[2] - 40;
                        double CoolantTemperatureF = 1.8 * CoolantTemperatureC + 32.0;
                        double IntakeAirTemperatureC = payload[3] - 40;
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
                case 0xD0:
                    descriptionToInsert = "LIMP STATE 1";

                    if (message.Length >= 4)
                    {
                        valueToInsert = Util.ByteToHexString(payload, 0, 2);
                    }
                    break;
                case 0xD1:
                    descriptionToInsert = "LIMP STATE 2";

                    if (message.Length >= 7)
                    {
                        valueToInsert = Util.ByteToHexString(payload, 0, 5);
                    }
                    break;
                case 0xD2:
                    descriptionToInsert = "BARO | IAT | A/C HSP | N/A";

                    if (message.Length >= 6)
                    {
                        byte BarometricPressureKPA = payload[0];
                        double BarometricPressurePSI = payload[0] * 0.14504;
                        double IntakeAirTemperatureC = payload[1] - 40; // °C
                        double IntakeAirTemperatureF = 1.8 * IntakeAirTemperatureC + 32; // °F
                        double ACHighSidePressurePSI = payload[2] * 2.03; // psi
                        double ACHighSidePressureKPA = payload[2] * 2.03 * 6.894757; // kPa

                        if (Properties.Settings.Default.Units == "imperial")
                        {
                            descriptionToInsert = "BARO: " + BarometricPressurePSI.ToString("0.0").Replace(",", ".") + " PSI | IAT: " + IntakeAirTemperatureF.ToString("0") + " °F | A/C HSP: " + ACHighSidePressurePSI.ToString("0.0").Replace(",", ".") + " PSI";
                            valueToInsert = "N/A: " + Util.ByteToHexString(payload, 3, 1);
                        }
                        else if (Properties.Settings.Default.Units == "metric")
                        {
                            descriptionToInsert = "BARO: " + BarometricPressureKPA.ToString("0.0").Replace(",", ".") + " KPA | IAT: " + IntakeAirTemperatureC.ToString("0") + " °C | A/C HSP: " + ACHighSidePressureKPA.ToString("0.0").Replace(",", ".") + " KPA";
                            valueToInsert = "N/A: " + Util.ByteToHexString(payload, 3, 1);
                        }
                    }
                    break;
                case 0xDF:
                    descriptionToInsert = "ACCUMULATED MILEAGE";

                    if (message.Length >= 3)
                    {
                        valueToInsert = Util.ByteToHexString(payload, 0, 1);
                    }
                    break;
                case 0xED:
                    descriptionToInsert = "CONFIGURATION | CRBFUL ENGDSP CYLVPC SALENG BSTYLE";

                    if (message.Length >= 7)
                    {
                        valueToInsert = Util.ByteToHexString(payload, 0, 5);
                    }
                    break;
                case 0xF0:
                    descriptionToInsert = "VEHICLE IDENTIFICATION NUMBER (VIN) CHARACTER";

                    if (((payload[0] == 0x01) && (message.Length >= 4)) || ((payload[0] == 0x02) && (message.Length >= 7)) || ((payload[0] == 0x06) && (message.Length >= 7)) || ((payload[0] == 0x0A) && (message.Length >= 7)) || ((payload[0] == 0x0E) && (message.Length >= 7)))
                    {
                        VIN = VIN.Remove(payload[0] - 1, payload.Length - 1).Insert(payload[0] - 1, Encoding.ASCII.GetString(payload.Skip(1).Take(payload.Length - 1).ToArray()));
                        valueToInsert = VIN;
                    }
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
                File.AppendAllText(MainForm.PCILogFilename, TimestampString); // no newline is appended!
            }

            File.AppendAllText(MainForm.PCILogFilename, Util.ByteToHexStringSimple(message) + Environment.NewLine);
        }
    }
}
