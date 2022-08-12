using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace ChryslerScanner
{
    public class PCI
    {
        public PCIDiagnosticsTable Diagnostics = new PCIDiagnosticsTable();
        public DataTable MessageDatabase = new DataTable("PCIDatabase");
        public ushort[] IDList;
        public DataColumn column;
        public DataRow row;

        public string VIN = "-----------------"; // 17 characters

        private const int hexBytesColumnStart = 2;
        private const int descriptionColumnStart = 28;
        private const int valueColumnStart = 82;
        private const int unitColumnStart = 108;
        private string state = string.Empty;
        private string speed = "10400 baud";
        private string logic = "non-inverted";

        public string HeaderUnknown  = "│ PCI-BUS MODULES         │ STATE: N/A                                                                                   ";
        public string HeaderDisabled = "│ PCI-BUS MODULES         │ STATE: DISABLED                                                                              ";
        public string HeaderEnabled  = "│ PCI-BUS MODULES         │ STATE: ENABLED @ BAUD | LOGIC: | ID BYTES:                                                   ";
        public string EmptyLine =      "│                         │                                                     │                         │             │";
        public string HeaderModified = string.Empty;

        public PCI()
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

            #region PCI-bus messages

            // Empty.

            #endregion

            IDList = MessageDatabase.AsEnumerable().Select(r => r.Field<ushort>("id")).ToArray();
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

            switch (ID)
            {
                case 0x10: // engine speed, vehicle speed, MAP sensor value
                    descriptionToInsert = "ENGINE SPEED | VEHICLE SPEED | MAP SENSOR";
                    valueToInsert = string.Empty;
                    unitToInsert = string.Empty;

                    if (message.Length >= 7)
                    {
                        double EngineSpeed = (((payload[0] << 8) + payload[1]) * 0.25D);
                        double VehicleSpeed = 0;

                        if (Properties.Settings.Default.Units == "imperial")
                        {
                            VehicleSpeed = ((payload[2] << 8) + payload[3]) * 0.0049D;
                        }
                        else if (Properties.Settings.Default.Units == "metric")
                        {
                            VehicleSpeed = ((payload[2] << 8) + payload[3]) * 1.609344D * 0.0049D;
                        }

                        double MAPValue = 0;

                        if (Properties.Settings.Default.Units == "imperial")
                        {
                            MAPValue = payload[4] / 0.412D * 0.145037738D;
                        }
                        else if (Properties.Settings.Default.Units == "metric")
                        {
                            MAPValue = payload[4] / 0.412D;
                        }

                        if (Properties.Settings.Default.Units == "imperial")
                        {
                            descriptionToInsert = "ENGINE: " + EngineSpeed.ToString("0.00") + " RPM | VEHICLE: " + VehicleSpeed.ToString("0.00") + " MPH";
                            valueToInsert = "MAP: " + MAPValue.ToString("0.00") + " PSI";
                        }
                        else if (Properties.Settings.Default.Units == "metric")
                        {
                            descriptionToInsert = "ENGINE: " + EngineSpeed.ToString("0.00") + " RPM | VEHICLE: " + VehicleSpeed.ToString("0.00") + " KM/H";
                            valueToInsert = "MAP: " + MAPValue.ToString("0.00") + " KPA";
                        }

                        unitToInsert = string.Empty;
                    }
                    break;
                case 0x14: // vehicle speed sensor, raw distance pulse signal
                    descriptionToInsert = "VEHICLE SPEED SENSOR";
                    valueToInsert = string.Empty;
                    unitToInsert = string.Empty;

                    if (message.Length >= 4)
                    {
                        ushort DistancePulse = (ushort)((payload[0] << 8) + payload[1]);

                        if (Properties.Settings.Default.Units == "imperial")
                        {
                            if (DistancePulse != 0xFFFF)
                            {
                                valueToInsert = Math.Round(28800.0D / DistancePulse, 1).ToString("0.0").Replace(",", ".");
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
                                valueToInsert = Math.Round(28800.0D / DistancePulse * 1.609344D, 1).ToString("0.0").Replace(",", ".");
                            }
                            else
                            {
                                valueToInsert = "0.0";
                            }

                            unitToInsert = "KM/H";
                        }
                    }
                    break;
                case 0x1A: // TPS volts, cruise flags
                    descriptionToInsert = "THROTTLE POSITION SENSOR VOLTAGE | CRUISE FLAGS";
                    valueToInsert = string.Empty;
                    unitToInsert = string.Empty;

                    if (message.Length >= 6)
                    {
                        double TPSVolts = payload[0] * 0.0191D;
                        byte CruiseFlags = payload[2];

                        valueToInsert = TPSVolts.ToString("0.000") + " | " + CruiseFlags;
                        unitToInsert = "V | N/A";
                    }
                    break;
                case 0x35: // MIC lamp status
                    descriptionToInsert = "MIC LAMP STATUS";
                    valueToInsert = string.Empty;
                    unitToInsert = string.Empty;

                    if (message.Length >= 4)
                    {
                        valueToInsert = Util.ByteToHexString(payload, 0, 2);
                    }
                    break;
                case 0x37: // shift lever position
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

                        unitToInsert = string.Empty;
                    }
                    else
                    {
                        valueToInsert = "ERROR";
                        unitToInsert = string.Empty;
                    }
                    break;
                case 0x5D: // mileage increment
                    descriptionToInsert = "MILEAGE INCREMENT";
                    valueToInsert = string.Empty;
                    unitToInsert = string.Empty;

                    if (message.Length >= 7)
                    {
                        if (Properties.Settings.Default.Units == "imperial")
                        {
                            valueToInsert = Math.Round(payload[0] * 0.000125D, 6).ToString("0.000000").Replace(",", ".") + " | " + Util.ByteToHexStringSimple(new byte[1] { payload[1] });
                            unitToInsert = "MI | N/A";
                        }
                        else if (Properties.Settings.Default.Units == "metric")
                        {
                            valueToInsert = Math.Round(payload[0] * 0.000125D * 1.609344D, 6).ToString("0.000000").Replace(",", ".") + " | " + Util.ByteToHexStringSimple(new byte[1] { payload[1] });
                            unitToInsert = "KM | N/A";
                        }
                    }
                    break;
                case 0x72: // odometer
                    descriptionToInsert = "VEHICLE DISTANCE / ODOMETER";
                    valueToInsert = string.Empty;
                    unitToInsert = string.Empty;

                    if (message.Length >= 6)
                    {
                        if (Properties.Settings.Default.Units == "imperial")
                        {
                            valueToInsert = Math.Round((uint)(payload[0] << 24 + payload[1] << 16 + payload[2] << 8 + payload[3]) * 0.000125D, 3).ToString("0.000").Replace(",", ".");
                            unitToInsert = "MILE";
                        }
                        else if (Properties.Settings.Default.Units == "metric")
                        {
                            valueToInsert = Math.Round((uint)(payload[0] << 24 + payload[1] << 16 + payload[2] << 8 + payload[3]) * 0.000125D * 1.609344D, 3).ToString("0.000").Replace(",", ".");
                            unitToInsert = "KILOMETER";
                        }
                    }
                    break;
                case 0xA0: // distance to empty
                    descriptionToInsert = "DISTANCE TO EMPTY";
                    valueToInsert = string.Empty;
                    unitToInsert = string.Empty;

                    if (message.Length >= 4)
                    {
                        if (Properties.Settings.Default.Units == "imperial")
                        {
                            valueToInsert = Math.Round(((payload[0] << 8) + payload[1]) * 0.1D, 1).ToString("0.0").Replace(",", ".");
                            unitToInsert = "MILE";
                        }
                        else if (Properties.Settings.Default.Units == "metric")
                        {
                            valueToInsert = Math.Round(((payload[0] << 8) + payload[1]) * 0.1D * 1.609344D, 1).ToString("0.0").Replace(",", ".");
                            unitToInsert = "KILOMETER";
                        }
                    }
                    break;
                case 0xA4: // fuel tank level percent
                    descriptionToInsert = "FUEL TANK LEVEL";
                    valueToInsert = string.Empty;
                    unitToInsert = string.Empty;

                    if (message.Length >= 3)
                    {
                        valueToInsert = Math.Round((payload[0] * 0.3922D), 1).ToString("0.0").Replace(",", ".");
                        unitToInsert = "%";
                    }
                    break;
                case 0xA5: // fuel tank level
                    descriptionToInsert = "FUEL TANK LEVEL";
                    valueToInsert = string.Empty;
                    unitToInsert = string.Empty;

                    if (message.Length >= 4)
                    {
                        if (Properties.Settings.Default.Units == "imperial")
                        {
                            valueToInsert = Math.Round(payload[1] * 0.125D, 1).ToString("0.0").Replace(",", ".");
                            unitToInsert = "GALLON";
                        }
                        else if (Properties.Settings.Default.Units == "metric")
                        {
                            valueToInsert = Math.Round(payload[1] * 0.125D * 3.785412D, 1).ToString("0.0").Replace(",", ".");
                            unitToInsert = "LITER";
                        }
                    }
                    break;
                case 0xB0: // check engine lamp state
                    descriptionToInsert = "CHECK ENGINE LAMP STATE";
                    valueToInsert = string.Empty;
                    unitToInsert = string.Empty;

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
                case 0xC0: // battery voltage, charging voltage, coolant temperature, intake air temperature
                    descriptionToInsert = "BATTERY | CHARGING | COOLANT | IAT";
                    valueToInsert = string.Empty;
                    unitToInsert = string.Empty;

                    if (message.Length >= 6)
                    {
                        double BatteryVoltage = payload[0] * 0.0625;
                        double ChargingVoltage = payload[1] * 0.0625;
                        byte CoolantTemperature = (byte)(payload[2] - 40); // °C
                        byte IntakeAirTemperature = (byte)(payload[3] - 40); // °C

                        if (Properties.Settings.Default.Units == "imperial")
                        {
                            descriptionToInsert = "BATTERY: " + BatteryVoltage.ToString("0.0") + " V | CHARGING: N/A | COOLANT: " + (1.8 * CoolantTemperature + 32).ToString("0") + " °F";
                            valueToInsert = "IAT: " + (1.8 * IntakeAirTemperature + 32).ToString("0") + " °F";
                        }
                        else if (Properties.Settings.Default.Units == "metric")
                        {
                            descriptionToInsert = "BATTERY: " + BatteryVoltage.ToString("0.0") + " V | CHARGING: N/A | COOLANT: " + CoolantTemperature.ToString("0") + " °C";
                            valueToInsert = "IAT: " + IntakeAirTemperature.ToString("0") + " °C";
                        }

                        unitToInsert = string.Empty;
                    }
                    break;
                case 0xD0: // limp state 1
                    descriptionToInsert = "LIMP STATE 1";
                    valueToInsert = string.Empty;
                    unitToInsert = string.Empty;

                    if (message.Length >= 4)
                    {
                        valueToInsert = Util.ByteToHexString(payload, 0, 2);
                    }
                    break;
                case 0xD1: // limp state 2
                    descriptionToInsert = "LIMP STATE 2";
                    valueToInsert = string.Empty;
                    unitToInsert = string.Empty;

                    if (message.Length >= 7)
                    {
                        valueToInsert = Util.ByteToHexString(payload, 0, 5);
                    }
                    break;
                case 0xD2: // A/C high-side pressure, intake air temperature
                    descriptionToInsert = "A/C HIGH SIDE PRESSURE | IAT";
                    valueToInsert = string.Empty;
                    unitToInsert = string.Empty;

                    if (message.Length >= 6)
                    {
                        double ACHighSidePressurePSI = payload[2] * 2.03; // psi
                        byte IntakeAirTemperature = (byte)(1.8 * payload[1] - 40); // °F

                        if (Properties.Settings.Default.Units == "imperial")
                        {
                            descriptionToInsert = "A/C HIGH SIDE PRESSURE: " + ACHighSidePressurePSI.ToString("0.0") + " PSI";
                            valueToInsert = "IAT: " + IntakeAirTemperature.ToString("0") + " °F";
                        }
                        else if (Properties.Settings.Default.Units == "metric")
                        {
                            descriptionToInsert = "A/C HIGH SIDE PRESSURE: " + (ACHighSidePressurePSI * 6.894757).ToString("0.0") + " KPA";
                            valueToInsert = "IAT: " + (payload[1] - 40).ToString("0") + " °C";
                        }

                        unitToInsert = string.Empty;
                    }
                    break;
                case 0xDF: // accumulated mileage
                    descriptionToInsert = "ACCUMULATED MILEAGE";
                    valueToInsert = string.Empty;
                    unitToInsert = string.Empty;

                    if (message.Length >= 3)
                    {
                        valueToInsert = Util.ByteToHexString(payload, 0, 1);
                    }
                    break;
                case 0xED: // vehicle information
                    descriptionToInsert = "VEHICLE INFORMATION";
                    valueToInsert = string.Empty;
                    unitToInsert = string.Empty;

                    if (message.Length >= 7)
                    {
                        valueToInsert = Util.ByteToHexString(payload, 0, 5);
                    }
                    break;
                case 0xF0: // VIN
                    descriptionToInsert = "VEHICLE IDENTIFICATION NUMBER (VIN) CHARACTER";
                    valueToInsert = string.Empty;
                    unitToInsert = string.Empty;

                    if (((payload[0] == 0x01) && (message.Length >= 4)) || ((payload[0] == 0x02) && (message.Length >= 7)) || ((payload[0] == 0x06) && (message.Length >= 7)) || ((payload[0] == 0x0A) && (message.Length >= 7)) || ((payload[0] == 0x0E) && (message.Length >= 7)))
                    {
                        VIN = VIN.Remove(payload[0] - 1, payload.Length - 1).Insert(payload[0] - 1, Encoding.ASCII.GetString(payload.Skip(1).Take(payload.Length - 1).ToArray()));
                        valueToInsert = VIN;
                        unitToInsert = string.Empty;
                    }
                    break;
                default:
                    descriptionToInsert = string.Empty;
                    valueToInsert = string.Empty;
                    unitToInsert = string.Empty;
                    break;
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
