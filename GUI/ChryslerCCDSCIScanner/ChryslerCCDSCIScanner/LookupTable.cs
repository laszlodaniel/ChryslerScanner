using System;
using System.Collections.Generic;

namespace ChryslerCCDSCIScanner
{
    class LookupTable
    {
        public Dictionary<byte, string> CCDBusMessageDescription { get; }
        public Dictionary<byte, double[]> CCDBusMessageValueScalingSlopeImperial { get; }
        public Dictionary<byte, double[]> CCDBusMessageValueScalingOffsetImperial { get; }
        public Dictionary<byte, double[]> CCDBusMessageValueScalingSlopeMetric { get; }
        public Dictionary<byte, double[]> CCDBusMessageValueScalingOffsetMetric { get; }
        public Dictionary<byte, string[]> CCDBusMessageUnitImperial { get; }
        public Dictionary<byte, string[]> CCDBusMessageUnitMetric { get; }

        public LookupTable()
        {
            CCDBusMessageDescription = new Dictionary<byte, string>();
            CCDBusMessageValueScalingSlopeImperial = new Dictionary<byte, double[]>();
            CCDBusMessageValueScalingOffsetImperial = new Dictionary<byte, double[]>();
            CCDBusMessageValueScalingSlopeMetric = new Dictionary<byte, double[]>();
            CCDBusMessageValueScalingOffsetMetric = new Dictionary<byte, double[]>();
            CCDBusMessageUnitImperial = new Dictionary<byte, string[]>();
            CCDBusMessageUnitMetric = new Dictionary<byte, string[]>();

            CCDBusMessageDescription.Add(0x24, "VEHICLE SPEED");
            CCDBusMessageDescription.Add(0x29, "LAST ENGINE SHUTDOWN");
            CCDBusMessageDescription.Add(0x3A, "INSTRUMENT CLUSTER LAMP STATES (AIRBAG LAMP)");
            CCDBusMessageDescription.Add(0x42, "THROTTLE POSITION SENSOR | CRUISE CONTROL");
            CCDBusMessageDescription.Add(0x50, "AIRBAG LAMP STATE");
            CCDBusMessageDescription.Add(0x6D, "VEHICLE IDENTIFICATION NUMBER (VIN)");
            CCDBusMessageDescription.Add(0x75, "A/C HIGH SIDE PRESSURE");
            CCDBusMessageDescription.Add(0x84, "INCREMENT ODOMETER AND TRIPMETER");
            CCDBusMessageDescription.Add(0x8C, "ENGINE COOLANT TEMPERATURE | AMBIENT TEMPERATURE");
            CCDBusMessageDescription.Add(0x94, "INSTRUMENT CLUSTER GAUGE VALUE");
            CCDBusMessageDescription.Add(0xA4, "INSTRUMENT CLUSTER LAMP STATES");
            CCDBusMessageDescription.Add(0xA9, "LAST ENGINE SHUTDOWN");
            CCDBusMessageDescription.Add(0xB2, "DRB REQUEST");
            CCDBusMessageDescription.Add(0xBE, "IGNITION SWITCH POSITION");
            CCDBusMessageDescription.Add(0xCC, "ACCUMULATED MILEAGE");
            CCDBusMessageDescription.Add(0xCE, "VEHICLE DISTANCE / ODOMETER");
            CCDBusMessageDescription.Add(0xD4, "BATTERY VOLTAGE | CALCULATED CHARGING VOLTAGE");
            CCDBusMessageDescription.Add(0xDA, "INSTRUMENT CLUSTER LAMP STATES (CHECK ENGINE)");
            CCDBusMessageDescription.Add(0xE4, "ENGINE SPEED | INTAKE MANIFOLD ABS. PRESSURE");
            CCDBusMessageDescription.Add(0xEC, "VEHICLE INFORMATION / LIMP STATES / FUEL TYPE");
            CCDBusMessageDescription.Add(0xEE, "TRIP DISTANCE / TRIPMETER");
            CCDBusMessageDescription.Add(0xF2, "DRB RESPONSE");
            CCDBusMessageDescription.Add(0xFE, "INTERIOR LAMP DIMMING");
            CCDBusMessageDescription.Add(0xFF, "CCD-BUS WAKE UP");

            CCDBusMessageValueScalingSlopeImperial.Add(0x24, new double[] { 1D });
            CCDBusMessageValueScalingSlopeImperial.Add(0x29, new double[] { 1D });
            CCDBusMessageValueScalingSlopeImperial.Add(0x3A, new double[] { 0 });
            CCDBusMessageValueScalingSlopeImperial.Add(0x42, new double[] { 0.65D, 0 });
            CCDBusMessageValueScalingSlopeImperial.Add(0x50, new double[] { 0 });
            CCDBusMessageValueScalingSlopeImperial.Add(0x6D, new double[] { 1F });
            CCDBusMessageValueScalingSlopeImperial.Add(0x75, new double[] { 1.961D }); // psi
            CCDBusMessageValueScalingSlopeImperial.Add(0x84, new double[] { 0.000125D });
            CCDBusMessageValueScalingSlopeImperial.Add(0x8C, new double[] { 1.8D, 1.8D }); // F
            CCDBusMessageValueScalingSlopeImperial.Add(0x94, new double[] { 0 });
            CCDBusMessageValueScalingSlopeImperial.Add(0xA4, new double[] { 0 });
            CCDBusMessageValueScalingSlopeImperial.Add(0xA9, new double[] { 1D });
            CCDBusMessageValueScalingSlopeImperial.Add(0xB2, new double[] { 0 });
            CCDBusMessageValueScalingSlopeImperial.Add(0xBE, new double[] { 0 });
            CCDBusMessageValueScalingSlopeImperial.Add(0xCC, new double[] { 1D });
            CCDBusMessageValueScalingSlopeImperial.Add(0xCE, new double[] { 0.000125D });
            CCDBusMessageValueScalingSlopeImperial.Add(0xD4, new double[] { 0.0592D, 0.0592D });
            CCDBusMessageValueScalingSlopeImperial.Add(0xDA, new double[] { 0 });
            CCDBusMessageValueScalingSlopeImperial.Add(0xE4, new double[] { 32D, 0.059756D }); // RPM, psi
            CCDBusMessageValueScalingSlopeImperial.Add(0xEC, new double[] { 0 });
            CCDBusMessageValueScalingSlopeImperial.Add(0xEE, new double[] { 0.016D });
            CCDBusMessageValueScalingSlopeImperial.Add(0xF2, new double[] { 0 });
            CCDBusMessageValueScalingSlopeImperial.Add(0xFE, new double[] { 1D });
            CCDBusMessageValueScalingSlopeImperial.Add(0xFF, new double[] { 0 });

            CCDBusMessageValueScalingOffsetImperial.Add(0x24, new double[] { 0 });
            CCDBusMessageValueScalingOffsetImperial.Add(0x29, new double[] { 0 });
            CCDBusMessageValueScalingOffsetImperial.Add(0x3A, new double[] { 0 });
            CCDBusMessageValueScalingOffsetImperial.Add(0x42, new double[] { 0 });
            CCDBusMessageValueScalingOffsetImperial.Add(0x50, new double[] { 0 });
            CCDBusMessageValueScalingOffsetImperial.Add(0x6D, new double[] { 0 });
            CCDBusMessageValueScalingOffsetImperial.Add(0x75, new double[] { 0 }); // psi
            CCDBusMessageValueScalingOffsetImperial.Add(0x84, new double[] { 0 });
            CCDBusMessageValueScalingOffsetImperial.Add(0x8C, new double[] { -83.2D, -83.2D }); // F
            CCDBusMessageValueScalingOffsetImperial.Add(0x94, new double[] { 0 });
            CCDBusMessageValueScalingOffsetImperial.Add(0xA4, new double[] { 0 });
            CCDBusMessageValueScalingOffsetImperial.Add(0xA9, new double[] { 0 });
            CCDBusMessageValueScalingOffsetImperial.Add(0xB2, new double[] { 0 });
            CCDBusMessageValueScalingOffsetImperial.Add(0xBE, new double[] { 0 });
            CCDBusMessageValueScalingOffsetImperial.Add(0xCC, new double[] { 0 });
            CCDBusMessageValueScalingOffsetImperial.Add(0xCE, new double[] { 0 });
            CCDBusMessageValueScalingOffsetImperial.Add(0xD4, new double[] { 0, 0 });
            CCDBusMessageValueScalingOffsetImperial.Add(0xDA, new double[] { 0 });
            CCDBusMessageValueScalingOffsetImperial.Add(0xE4, new double[] { 0, 0 }); // RPM, psi
            CCDBusMessageValueScalingOffsetImperial.Add(0xEC, new double[] { 0 });
            CCDBusMessageValueScalingOffsetImperial.Add(0xEE, new double[] { 0 });
            CCDBusMessageValueScalingOffsetImperial.Add(0xF2, new double[] { 0 });
            CCDBusMessageValueScalingOffsetImperial.Add(0xFE, new double[] { 0 });
            CCDBusMessageValueScalingOffsetImperial.Add(0xFF, new double[] { 0 });

            CCDBusMessageValueScalingSlopeMetric.Add(0x24, new double[] { 1D });
            CCDBusMessageValueScalingSlopeMetric.Add(0x29, new double[] { 1D });
            CCDBusMessageValueScalingSlopeMetric.Add(0x3A, new double[] { 0 });
            CCDBusMessageValueScalingSlopeMetric.Add(0x42, new double[] { 0.65D, 0 });
            CCDBusMessageValueScalingSlopeMetric.Add(0x50, new double[] { 0 });
            CCDBusMessageValueScalingSlopeMetric.Add(0x6D, new double[] { 1D });
            CCDBusMessageValueScalingSlopeMetric.Add(0x75, new double[] { 6.894757D }); // kPa
            CCDBusMessageValueScalingSlopeMetric.Add(0x84, new double[] { 0.000125D });
            CCDBusMessageValueScalingSlopeMetric.Add(0x8C, new double[] { 0.555556D, 0.555556D }); // °C
            CCDBusMessageValueScalingSlopeMetric.Add(0x94, new double[] { 0 });
            CCDBusMessageValueScalingSlopeMetric.Add(0xA4, new double[] { 0 });
            CCDBusMessageValueScalingSlopeMetric.Add(0xA9, new double[] { 1D });
            CCDBusMessageValueScalingSlopeMetric.Add(0xB2, new double[] { 0 });
            CCDBusMessageValueScalingSlopeMetric.Add(0xBE, new double[] { 0 });
            CCDBusMessageValueScalingSlopeMetric.Add(0xCC, new double[] { 1D });
            CCDBusMessageValueScalingSlopeMetric.Add(0xCE, new double[] { 1.609334138D });
            CCDBusMessageValueScalingSlopeMetric.Add(0xD4, new double[] { 0.0592D, 0.0592D });
            CCDBusMessageValueScalingSlopeMetric.Add(0xDA, new double[] { 0 });
            CCDBusMessageValueScalingSlopeMetric.Add(0xE4, new double[] { 32D, 6.894757D }); // RPM, kPa
            CCDBusMessageValueScalingSlopeMetric.Add(0xEC, new double[] { 0 });
            CCDBusMessageValueScalingSlopeMetric.Add(0xEE, new double[] { 1.609334138D });
            CCDBusMessageValueScalingSlopeMetric.Add(0xF2, new double[] { 0 });
            CCDBusMessageValueScalingSlopeMetric.Add(0xFE, new double[] { 1D });
            CCDBusMessageValueScalingSlopeMetric.Add(0xFF, new double[] { 0 });

            CCDBusMessageValueScalingOffsetMetric.Add(0x24, new double[] { 0 });
            CCDBusMessageValueScalingOffsetMetric.Add(0x29, new double[] { 0 });
            CCDBusMessageValueScalingOffsetMetric.Add(0x3A, new double[] { 0 });
            CCDBusMessageValueScalingOffsetMetric.Add(0x42, new double[] { 0 });
            CCDBusMessageValueScalingOffsetMetric.Add(0x50, new double[] { 0 });
            CCDBusMessageValueScalingOffsetMetric.Add(0x6D, new double[] { 0 });
            CCDBusMessageValueScalingOffsetMetric.Add(0x75, new double[] { 0 }); // kPa
            CCDBusMessageValueScalingOffsetMetric.Add(0x84, new double[] { 0 });
            CCDBusMessageValueScalingOffsetMetric.Add(0x8C, new double[] { -17.77778D, -17.77778D }); // °C
            CCDBusMessageValueScalingOffsetMetric.Add(0x94, new double[] { 0 });
            CCDBusMessageValueScalingOffsetMetric.Add(0xA4, new double[] { 0 });
            CCDBusMessageValueScalingOffsetMetric.Add(0xA9, new double[] { 0 });
            CCDBusMessageValueScalingOffsetMetric.Add(0xB2, new double[] { 0 });
            CCDBusMessageValueScalingOffsetMetric.Add(0xBE, new double[] { 0 });
            CCDBusMessageValueScalingOffsetMetric.Add(0xCC, new double[] { 0 });
            CCDBusMessageValueScalingOffsetMetric.Add(0xCE, new double[] { 0 });
            CCDBusMessageValueScalingOffsetMetric.Add(0xD4, new double[] { 0, 0 });
            CCDBusMessageValueScalingOffsetMetric.Add(0xDA, new double[] { 0 });
            CCDBusMessageValueScalingOffsetMetric.Add(0xE4, new double[] { 0, 0 }); // RPM, kPa
            CCDBusMessageValueScalingOffsetMetric.Add(0xEC, new double[] { 0 });
            CCDBusMessageValueScalingOffsetMetric.Add(0xEE, new double[] { 0 });
            CCDBusMessageValueScalingOffsetMetric.Add(0xF2, new double[] { 0 });
            CCDBusMessageValueScalingOffsetMetric.Add(0xFE, new double[] { 0 });
            CCDBusMessageValueScalingOffsetMetric.Add(0xFF, new double[] { 0 });

            CCDBusMessageUnitImperial.Add(0x24, new string[] { "MPH", "KM/H" });
            CCDBusMessageUnitImperial.Add(0x29, new string[] { "MINUTE" });
            CCDBusMessageUnitImperial.Add(0x3A, new string[] { String.Empty });
            CCDBusMessageUnitImperial.Add(0x42, new string[] { "%", String.Empty });
            CCDBusMessageUnitImperial.Add(0x50, new string[] { String.Empty });
            CCDBusMessageUnitImperial.Add(0x6D, new string[] { String.Empty });
            CCDBusMessageUnitImperial.Add(0x75, new string[] { "PSI" });
            CCDBusMessageUnitImperial.Add(0x84, new string[] { "MILE" });
            CCDBusMessageUnitImperial.Add(0x8C, new string[] { "°F", "°F" });
            CCDBusMessageUnitImperial.Add(0x94, new string[] { String.Empty });
            CCDBusMessageUnitImperial.Add(0xA4, new string[] { String.Empty });
            CCDBusMessageUnitImperial.Add(0xA9, new string[] { "MINUTE" });
            CCDBusMessageUnitImperial.Add(0xB2, new string[] { String.Empty });
            CCDBusMessageUnitImperial.Add(0xBE, new string[] { String.Empty });
            CCDBusMessageUnitImperial.Add(0xCC, new string[] { String.Empty });
            CCDBusMessageUnitImperial.Add(0xCE, new string[] { "MILE" });
            CCDBusMessageUnitImperial.Add(0xD4, new string[] { "V", "V" });
            CCDBusMessageUnitImperial.Add(0xDA, new string[] { String.Empty });
            CCDBusMessageUnitImperial.Add(0xE4, new string[] { "RPM", "PSI" });
            CCDBusMessageUnitImperial.Add(0xEC, new string[] { String.Empty });
            CCDBusMessageUnitImperial.Add(0xEE, new string[] { "MILE" });
            CCDBusMessageUnitImperial.Add(0xF2, new string[] { String.Empty });
            CCDBusMessageUnitImperial.Add(0xFE, new string[] { String.Empty });
            CCDBusMessageUnitImperial.Add(0xFF, new string[] { String.Empty });

            CCDBusMessageUnitMetric.Add(0x24, new string[] { "MPH", "KM/H" });
            CCDBusMessageUnitMetric.Add(0x29, new string[] { "MINUTE" });
            CCDBusMessageUnitMetric.Add(0x3A, new string[] { String.Empty });
            CCDBusMessageUnitMetric.Add(0x42, new string[] { "%", String.Empty });
            CCDBusMessageUnitMetric.Add(0x50, new string[] { String.Empty });
            CCDBusMessageUnitMetric.Add(0x6D, new string[] { String.Empty });
            CCDBusMessageUnitMetric.Add(0x75, new string[] { "KPA" });
            CCDBusMessageUnitMetric.Add(0x84, new string[] { "KILOMETER" });
            CCDBusMessageUnitMetric.Add(0x8C, new string[] { "°C", "°C" });
            CCDBusMessageUnitMetric.Add(0x94, new string[] { String.Empty });
            CCDBusMessageUnitMetric.Add(0xA4, new string[] { String.Empty });
            CCDBusMessageUnitMetric.Add(0xA9, new string[] { "MINUTE" });
            CCDBusMessageUnitMetric.Add(0xB2, new string[] { String.Empty });
            CCDBusMessageUnitMetric.Add(0xBE, new string[] { String.Empty });
            CCDBusMessageUnitMetric.Add(0xCC, new string[] { String.Empty });
            CCDBusMessageUnitMetric.Add(0xCE, new string[] { "KILOMETER" });
            CCDBusMessageUnitMetric.Add(0xD4, new string[] { "V", "V" });
            CCDBusMessageUnitMetric.Add(0xDA, new string[] { String.Empty });
            CCDBusMessageUnitMetric.Add(0xE4, new string[] { "RPM", "KPA" });
            CCDBusMessageUnitMetric.Add(0xEC, new string[] { String.Empty });
            CCDBusMessageUnitMetric.Add(0xEE, new string[] { "KILOMETER" });
            CCDBusMessageUnitMetric.Add(0xF2, new string[] { String.Empty });
            CCDBusMessageUnitMetric.Add(0xFE, new string[] { String.Empty });
            CCDBusMessageUnitMetric.Add(0xFF, new string[] { String.Empty });
        }

        public string GetCCDBusMessageDescription(byte key)
        {
            if (CCDBusMessageDescription.ContainsKey(key)) return CCDBusMessageDescription[key];
            else return String.Empty;
        }

        public double[] GetCCDBusMessageValueScalingSlopeImperial(byte key)
        {
            return CCDBusMessageValueScalingSlopeImperial[key];
        }

        public double[] GetCCDBusMessageValueScalingOffsetImperial(byte key)
        {
            return CCDBusMessageValueScalingOffsetImperial[key];
        }

        public double[] GetCCDBusMessageValueScalingSlopeMetric(byte key)
        {
            return CCDBusMessageValueScalingSlopeMetric[key];
        }

        public double[] GetCCDBusMessageValueScalingOffsetMetric(byte key)
        {
            return CCDBusMessageValueScalingOffsetMetric[key];
        }

        public string[] GetCCDBusMessageUnitImperial(byte key)
        {
            return CCDBusMessageUnitImperial[key];
        }

        public string[] GetCCDBusMessageUnitMetric(byte key)
        {
            return CCDBusMessageUnitMetric[key];
        }
    }
}
