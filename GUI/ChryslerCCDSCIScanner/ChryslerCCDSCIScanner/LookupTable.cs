using System;
using System.Collections.Generic;

namespace ChryslerCCDSCIScanner
{
    class LookupTable
    {
        public Dictionary<byte, string> CCDBusMessageDescription { get; }
        public Dictionary<byte, double[]> CCDBusMessageSlope { get; }  // imperial
        public Dictionary<byte, double[]> CCDBusMessageOffset { get; } // imperial
        public Dictionary<byte, double[]> CCDBusMessageSlConv { get; } // metric
        public Dictionary<byte, double[]> CCDBusMessageOfConv { get; } // metric
        public Dictionary<byte, string[]> CCDBusMessageUnitImperial { get; }
        public Dictionary<byte, string[]> CCDBusMessageUnitMetric { get; }

        public LookupTable()
        {
            CCDBusMessageDescription = new Dictionary<byte, string>();
            CCDBusMessageSlope = new Dictionary<byte, double[]>();
            CCDBusMessageOffset = new Dictionary<byte, double[]>();
            CCDBusMessageSlConv = new Dictionary<byte, double[]>();
            CCDBusMessageOfConv = new Dictionary<byte, double[]>();
            CCDBusMessageUnitImperial = new Dictionary<byte, string[]>();
            CCDBusMessageUnitMetric = new Dictionary<byte, string[]>();

            CCDBusMessageDescription.Add(0x24, "VEHICLE SPEED");
            CCDBusMessageDescription.Add(0x29, "LAST ENGINE SHUTDOWN");
            CCDBusMessageDescription.Add(0x3A, "INSTRUMENT CLUSTER LAMP STATES (AIRBAG LAMP)");
            CCDBusMessageDescription.Add(0x42, "THROTTLE POSITION SENSOR | CRUISE CONTROL");
            CCDBusMessageDescription.Add(0x44, "FUEL USED");
            CCDBusMessageDescription.Add(0x50, "AIRBAG LAMP STATE");
            CCDBusMessageDescription.Add(0x6D, "VEHICLE IDENTIFICATION NUMBER (VIN)");
            CCDBusMessageDescription.Add(0x75, "A/C HIGH SIDE PRESSURE");
            CCDBusMessageDescription.Add(0x7B, "OUTSIDE AIR TEMPERATURE");
            CCDBusMessageDescription.Add(0x84, "INCREMENT ODOMETER AND TRIPMETER");
            CCDBusMessageDescription.Add(0x8C, "ENGINE COOLANT TEMPERATURE | INTAKE AIR TEMP.");
            CCDBusMessageDescription.Add(0x94, "INSTRUMENT CLUSTER GAUGE VALUE");
            CCDBusMessageDescription.Add(0xA4, "INSTRUMENT CLUSTER LAMP STATES");
            CCDBusMessageDescription.Add(0xA9, "LAST ENGINE SHUTDOWN");
            CCDBusMessageDescription.Add(0xAA, "VTS STATUS");
            CCDBusMessageDescription.Add(0xAC, "BODY TYPE BROADCAST");
            CCDBusMessageDescription.Add(0xB2, "DRB REQUEST");
            CCDBusMessageDescription.Add(0xBE, "IGNITION SWITCH POSITION");
            CCDBusMessageDescription.Add(0xCC, "ACCUMULATED MILEAGE");
            CCDBusMessageDescription.Add(0xCE, "VEHICLE DISTANCE / ODOMETER");
            CCDBusMessageDescription.Add(0xD4, "BATTERY VOLTAGE | CALCULATED CHARGING VOLTAGE");
            CCDBusMessageDescription.Add(0xDA, "INSTRUMENT CLUSTER LAMP STATES");
            CCDBusMessageDescription.Add(0xE4, "ENGINE SPEED | INTAKE MANIFOLD ABS. PRESSURE");
            CCDBusMessageDescription.Add(0xEC, "VEHICLE INFORMATION / LIMP STATES / FUEL TYPE");
            CCDBusMessageDescription.Add(0xEE, "TRIP DISTANCE / TRIPMETER");
            CCDBusMessageDescription.Add(0xF2, "DRB RESPONSE");
            CCDBusMessageDescription.Add(0xFE, "INTERIOR LAMP DIMMING");
            CCDBusMessageDescription.Add(0xFF, "CCD-BUS WAKE UP");

            CCDBusMessageSlope.Add(0x24, new double[] { 1D });
            CCDBusMessageSlope.Add(0x29, new double[] { 1D });
            CCDBusMessageSlope.Add(0x3A, new double[] { 0 });
            CCDBusMessageSlope.Add(0x42, new double[] { 0.65D, 0 });
            CCDBusMessageSlope.Add(0x44, new double[] { 1D });
            CCDBusMessageSlope.Add(0x50, new double[] { 0 });
            CCDBusMessageSlope.Add(0x6D, new double[] { 1F });
            CCDBusMessageSlope.Add(0x75, new double[] { 1.961D }); // psi
            CCDBusMessageSlope.Add(0x7B, new double[] { 1D }); // F
            CCDBusMessageSlope.Add(0x84, new double[] { 0.000125D });
            CCDBusMessageSlope.Add(0x8C, new double[] { 1.8D, 1.8D }); // F
            CCDBusMessageSlope.Add(0x94, new double[] { 0 });
            CCDBusMessageSlope.Add(0xA4, new double[] { 0 });
            CCDBusMessageSlope.Add(0xA9, new double[] { 1D });
            CCDBusMessageSlope.Add(0xAA, new double[] { 0 });
            CCDBusMessageSlope.Add(0xAC, new double[] { 0 });
            CCDBusMessageSlope.Add(0xB2, new double[] { 0 });
            CCDBusMessageSlope.Add(0xBE, new double[] { 0 });
            CCDBusMessageSlope.Add(0xCC, new double[] { 1D });
            CCDBusMessageSlope.Add(0xCE, new double[] { 0.000125D });
            CCDBusMessageSlope.Add(0xD4, new double[] { 0.0592D, 0.0592D });
            CCDBusMessageSlope.Add(0xDA, new double[] { 0 });
            CCDBusMessageSlope.Add(0xE4, new double[] { 32D, 0.059756D }); // RPM, psi
            CCDBusMessageSlope.Add(0xEC, new double[] { 0 });
            CCDBusMessageSlope.Add(0xEE, new double[] { 0.016D });
            CCDBusMessageSlope.Add(0xF2, new double[] { 0 });
            CCDBusMessageSlope.Add(0xFE, new double[] { 1D });
            CCDBusMessageSlope.Add(0xFF, new double[] { 0 });

            CCDBusMessageOffset.Add(0x24, new double[] { 0 });
            CCDBusMessageOffset.Add(0x29, new double[] { 0 });
            CCDBusMessageOffset.Add(0x3A, new double[] { 0 });
            CCDBusMessageOffset.Add(0x42, new double[] { 0 });
            CCDBusMessageOffset.Add(0x44, new double[] { 0 });
            CCDBusMessageOffset.Add(0x50, new double[] { 0 });
            CCDBusMessageOffset.Add(0x6D, new double[] { 0 });
            CCDBusMessageOffset.Add(0x75, new double[] { 0 }); // psi
            CCDBusMessageOffset.Add(0x7B, new double[] { -70.0D }); // F
            CCDBusMessageOffset.Add(0x84, new double[] { 0 });
            CCDBusMessageOffset.Add(0x8C, new double[] { -198.4D, -198.4D }); // F
            CCDBusMessageOffset.Add(0x94, new double[] { 0 });
            CCDBusMessageOffset.Add(0xA4, new double[] { 0 });
            CCDBusMessageOffset.Add(0xA9, new double[] { 0 });
            CCDBusMessageOffset.Add(0xAA, new double[] { 0 });
            CCDBusMessageOffset.Add(0xAC, new double[] { 0 });
            CCDBusMessageOffset.Add(0xB2, new double[] { 0 });
            CCDBusMessageOffset.Add(0xBE, new double[] { 0 });
            CCDBusMessageOffset.Add(0xCC, new double[] { 0 });
            CCDBusMessageOffset.Add(0xCE, new double[] { 0 });
            CCDBusMessageOffset.Add(0xD4, new double[] { 0, 0 });
            CCDBusMessageOffset.Add(0xDA, new double[] { 0 });
            CCDBusMessageOffset.Add(0xE4, new double[] { 0, 0 }); // RPM, psi
            CCDBusMessageOffset.Add(0xEC, new double[] { 0 });
            CCDBusMessageOffset.Add(0xEE, new double[] { 0 });
            CCDBusMessageOffset.Add(0xF2, new double[] { 0 });
            CCDBusMessageOffset.Add(0xFE, new double[] { 0 });
            CCDBusMessageOffset.Add(0xFF, new double[] { 0 });

            CCDBusMessageSlConv.Add(0x24, new double[] { 1D });
            CCDBusMessageSlConv.Add(0x29, new double[] { 1D });
            CCDBusMessageSlConv.Add(0x3A, new double[] { 0 });
            CCDBusMessageSlConv.Add(0x42, new double[] { 0.65D, 0 });
            CCDBusMessageSlConv.Add(0x44, new double[] { 1D });
            CCDBusMessageSlConv.Add(0x50, new double[] { 0 });
            CCDBusMessageSlConv.Add(0x6D, new double[] { 1D });
            CCDBusMessageSlConv.Add(0x75, new double[] { 6.894757D }); // kPa
            CCDBusMessageSlConv.Add(0x7B, new double[] { 1D }); // °C !!!TODO!!!
            CCDBusMessageSlConv.Add(0x84, new double[] { 0.000125D });
            CCDBusMessageSlConv.Add(0x8C, new double[] { 0.555556D, 0.555556D }); // °C
            CCDBusMessageSlConv.Add(0x94, new double[] { 0 });
            CCDBusMessageSlConv.Add(0xA4, new double[] { 0 });
            CCDBusMessageSlConv.Add(0xA9, new double[] { 1D });
            CCDBusMessageSlConv.Add(0xAA, new double[] { 0 });
            CCDBusMessageSlConv.Add(0xAC, new double[] { 0 });
            CCDBusMessageSlConv.Add(0xB2, new double[] { 0 });
            CCDBusMessageSlConv.Add(0xBE, new double[] { 0 });
            CCDBusMessageSlConv.Add(0xCC, new double[] { 1D });
            CCDBusMessageSlConv.Add(0xCE, new double[] { 1.609334138D });
            CCDBusMessageSlConv.Add(0xD4, new double[] { 0.0592D, 0.0592D });
            CCDBusMessageSlConv.Add(0xDA, new double[] { 0 });
            CCDBusMessageSlConv.Add(0xE4, new double[] { 32D, 6.894757D }); // RPM, kPa
            CCDBusMessageSlConv.Add(0xEC, new double[] { 0 });
            CCDBusMessageSlConv.Add(0xEE, new double[] { 1.609334138D });
            CCDBusMessageSlConv.Add(0xF2, new double[] { 0 });
            CCDBusMessageSlConv.Add(0xFE, new double[] { 1D });
            CCDBusMessageSlConv.Add(0xFF, new double[] { 0 });

            CCDBusMessageOfConv.Add(0x24, new double[] { 0 });
            CCDBusMessageOfConv.Add(0x29, new double[] { 0 });
            CCDBusMessageOfConv.Add(0x3A, new double[] { 0 });
            CCDBusMessageOfConv.Add(0x42, new double[] { 0 });
            CCDBusMessageOfConv.Add(0x44, new double[] { 0 });
            CCDBusMessageOfConv.Add(0x50, new double[] { 0 });
            CCDBusMessageOfConv.Add(0x6D, new double[] { 0 });
            CCDBusMessageOfConv.Add(0x75, new double[] { 0 }); // kPa
            CCDBusMessageOfConv.Add(0x7B, new double[] { 0 }); // °C !!!TODO!!!
            CCDBusMessageOfConv.Add(0x84, new double[] { 0 });
            CCDBusMessageOfConv.Add(0x8C, new double[] { -17.77778D, -17.77778D }); // °C
            CCDBusMessageOfConv.Add(0x94, new double[] { 0 });
            CCDBusMessageOfConv.Add(0xA4, new double[] { 0 });
            CCDBusMessageOfConv.Add(0xA9, new double[] { 0 });
            CCDBusMessageOfConv.Add(0xAA, new double[] { 0 });
            CCDBusMessageOfConv.Add(0xAC, new double[] { 0 });
            CCDBusMessageOfConv.Add(0xB2, new double[] { 0 });
            CCDBusMessageOfConv.Add(0xBE, new double[] { 0 });
            CCDBusMessageOfConv.Add(0xCC, new double[] { 0 });
            CCDBusMessageOfConv.Add(0xCE, new double[] { 0 });
            CCDBusMessageOfConv.Add(0xD4, new double[] { 0, 0 });
            CCDBusMessageOfConv.Add(0xDA, new double[] { 0 });
            CCDBusMessageOfConv.Add(0xE4, new double[] { 0, 0 }); // RPM, kPa
            CCDBusMessageOfConv.Add(0xEC, new double[] { 0 });
            CCDBusMessageOfConv.Add(0xEE, new double[] { 0 });
            CCDBusMessageOfConv.Add(0xF2, new double[] { 0 });
            CCDBusMessageOfConv.Add(0xFE, new double[] { 0 });
            CCDBusMessageOfConv.Add(0xFF, new double[] { 0 });

            CCDBusMessageUnitImperial.Add(0x24, new string[] { "MPH", "KM/H" });
            CCDBusMessageUnitImperial.Add(0x29, new string[] { "HOUR:MINUTE" });
            CCDBusMessageUnitImperial.Add(0x3A, new string[] { String.Empty });
            CCDBusMessageUnitImperial.Add(0x42, new string[] { "%", String.Empty });
            CCDBusMessageUnitImperial.Add(0x44, new string[] { String.Empty });
            CCDBusMessageUnitImperial.Add(0x50, new string[] { String.Empty });
            CCDBusMessageUnitImperial.Add(0x6D, new string[] { String.Empty });
            CCDBusMessageUnitImperial.Add(0x75, new string[] { "PSI" });
            CCDBusMessageUnitImperial.Add(0x7B, new string[] { "F" });
            CCDBusMessageUnitImperial.Add(0x84, new string[] { "MILE" });
            CCDBusMessageUnitImperial.Add(0x8C, new string[] { "F", "F" });
            CCDBusMessageUnitImperial.Add(0x94, new string[] { String.Empty });
            CCDBusMessageUnitImperial.Add(0xA4, new string[] { String.Empty });
            CCDBusMessageUnitImperial.Add(0xA9, new string[] { "MINUTE" });
            CCDBusMessageUnitImperial.Add(0xAA, new string[] { String.Empty });
            CCDBusMessageUnitImperial.Add(0xAC, new string[] { String.Empty });
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
            CCDBusMessageUnitMetric.Add(0x29, new string[] { "HOUR:MINUTE" });
            CCDBusMessageUnitMetric.Add(0x3A, new string[] { String.Empty });
            CCDBusMessageUnitMetric.Add(0x42, new string[] { "%", String.Empty });
            CCDBusMessageUnitMetric.Add(0x44, new string[] { String.Empty });
            CCDBusMessageUnitMetric.Add(0x50, new string[] { String.Empty });
            CCDBusMessageUnitMetric.Add(0x6D, new string[] { String.Empty });
            CCDBusMessageUnitMetric.Add(0x75, new string[] { "KPA" });
            CCDBusMessageUnitMetric.Add(0x7B, new string[] { "°C" });
            CCDBusMessageUnitMetric.Add(0x84, new string[] { "KILOMETER" });
            CCDBusMessageUnitMetric.Add(0x8C, new string[] { "°C", "°C" });
            CCDBusMessageUnitMetric.Add(0x94, new string[] { String.Empty });
            CCDBusMessageUnitMetric.Add(0xA4, new string[] { String.Empty });
            CCDBusMessageUnitMetric.Add(0xA9, new string[] { "MINUTE" });
            CCDBusMessageUnitMetric.Add(0xAA, new string[] { String.Empty });
            CCDBusMessageUnitMetric.Add(0xAC, new string[] { String.Empty });
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

        public double[] GetCCDBusMessageSlope(byte key)
        {
            return CCDBusMessageSlope[key];
        }

        public double[] GetCCDBusMessageOffset(byte key)
        {
            return CCDBusMessageOffset[key];
        }

        public double[] GetCCDBusMessageSlConv(byte key)
        {
            return CCDBusMessageSlConv[key];
        }

        public double[] GetCCDBusMessageOfConv(byte key)
        {
            return CCDBusMessageOfConv[key];
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
