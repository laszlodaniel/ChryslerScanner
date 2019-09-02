using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Xml;

namespace ChryslerCCDSCIScanner
{
    public class TextTable : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public List<string> Table = new List<string>();
        private List<string> CCDMsgList = new List<string>(256);
        private List<string> SCIPCMMsgList = new List<string>(256);
        private List<byte> SCIPCMReqMsgList = new List<byte>(256);
        private List<string> SCITCMMsgList = new List<string>(256);
        private List<byte> SCITCMReqMsgList = new List<byte>(256);
        private List<byte> CCDIDList = new List<byte>(256);
        private List<byte> SCIPCMIDList = new List<byte>(256);
        private List<byte> SCITCMIDList = new List<byte>(256);
        private List<string> DetailList = new List<string>();


        private string EmptyLine                  = "│                         │                                                    │                          │              │";
        private string CCDBusStateLineNA          = "│ CCD-BUS MODULES         │ STATE: N/A                                                                                    ";
        private string CCDBusStateLineDisabled    = "│ CCD-BUS MODULES         │ STATE: DISABLED | ID BYTES:  | # OF MESSAGES: RX= TX=                                         ";
        private string CCDBusStateLineEnabled     = "│ CCD-BUS MODULES         │ STATE: ENABLED @ 7812.5 BAUD | ID BYTES:  | # OF MESSAGES: RX= TX=                            ";
        private string SCIBusPCMStateLineNA       = "│ SCI-BUS ENGINE          │ STATE: N/A                                                                                    ";
        private string SCIBusPCMStateLineDisabled = "│ SCI-BUS ENGINE          │ STATE: DISABLED | CONFIGURATION:  | # OF MESSAGES: RX= TX=                                    ";
        private string SCIBusPCMStateLineEnabled  = "│ SCI-BUS ENGINE          │ STATE: ENABLED @  BAUD | CONFIGURATION:  | # OF MESSAGES: RX= TX=                             ";
        private string SCIBusTCMStateLineNA       = "│ SCI-BUS TRANSMISSION    │ STATE: N/A                                                                                    ";
        private string SCIBusTCMStateLineDisabled = "│ SCI-BUS TRANSMISSION    │ STATE: DISABLED | CONFIGURATION:  | # OF MESSAGES: RX= TX=                                    ";
        private string SCIBusTCMStateLineEnabled  = "│ SCI-BUS TRANSMISSION    │ STATE: ENABLED @  BAUD | CONFIGURATION:  | # OF MESSAGES: RX= TX=                             ";

        private string VIN = "-----------------"; // 17 characters

        private int CCDListStart = 5; // row number
        private int CCDListEnd = 5;
        private int CCDB2Start = 7;
        private int CCDF2Start = 8;
        private int SCIPCMListStart = 17;
        private int SCIPCMListEnd = 17;
        //private int SCIPCMTableStart = 21;
        private int SCITCMListStart = 26;
        private int SCITCMListEnd = 26;

        private int CCDBusMsgRxCount = 0;
        public static int CCDBusMsgTxCount = 0;
        private int SCIBusPCMMsgRxCount = 0;
        public static int SCIBusPCMMsgTxCount = 0;
        private int SCIBusTCMMsgRxCount = 0;
        public static int SCIBusTCMMsgTxCount = 0;

        private bool CCDBusEnabled = false;
        private bool SCIBusPCMEnabled = false;
        private bool SCIBusTCMEnabled = false;

        private bool IDSorting = true; // Sort messages by ID-byte (CCD-bus and SCI-bus too)
        private bool CCDBusB2MsgPresent = false;
        private bool CCDBusF2MsgPresent = false;
        private int CCDBusIDByteNum = 0;
        private int SCIBusPCMIDByteNum = 0;
        private int SCIBusTCMIDByteNum = 0;

        private double ImperialSlope = 0;
        private double MetricSlope = 0;

        private XmlDocument VehicleProfiles = new XmlDocument();
        LookupTable LT = new LookupTable();

        public TextTable(string VehicleProfilesFileName)
        {
            //try
            //{
            //    VehicleProfiles.Load(VehicleProfilesFileName);
            //    foreach (XmlNode node in VehicleProfiles.DocumentElement.ChildNodes)
            //    {
            //        string text = node.InnerText;
            //    }
            //}
            //catch
            //{
                
            //}

            Table.Add("┌─────────────────────────┐                                                                                               ");
            Table.Add("│ CCD-BUS MODULES         │ STATE: N/A                                                                                    ");
            Table.Add("├─────────────────────────┼────────────────────────────────────────────────────┬──────────────────────────┬──────────────┐");
            Table.Add("│ MESSAGE [HEX]           │ DESCRIPTION                                        │ VALUE                    │ UNIT         │");
            Table.Add("╞═════════════════════════╪════════════════════════════════════════════════════╪══════════════════════════╪══════════════╡");
            Table.Add("│                         │                                                    │                          │              │");
            Table.Add("├─────────────────────────┼────────────────────────────────────────────────────┼──────────────────────────┼──────────────┤");
            Table.Add("│ B2 -- -- -- -- --       │ DRB REQUEST                                        │                          │              │");
            Table.Add("│ F2 -- -- -- -- --       │ DRB RESPONSE                                       │                          │              │");
            Table.Add("└─────────────────────────┴────────────────────────────────────────────────────┴──────────────────────────┴──────────────┘");
            Table.Add("                                                                                                                          ");
            Table.Add("                                                                                                                          ");
            Table.Add("┌─────────────────────────┐                                                                                               ");
            Table.Add("│ SCI-BUS ENGINE          │ STATE: N/A                                                                                    ");
            Table.Add("├─────────────────────────┼────────────────────────────────────────────────────┬──────────────────────────┬──────────────┐");
            Table.Add("│ MESSAGE [HEX]           │ DESCRIPTION                                        │ VALUE                    │ UNIT         │");
            Table.Add("╞═════════════════════════╪════════════════════════════════════════════════════╪══════════════════════════╪══════════════╡");
            Table.Add("│                         │                                                    │                          │              │");
            Table.Add("└─────────────────────────┴────────────────────────────────────────────────────┴──────────────────────────┴──────────────┘");
            Table.Add("                                                                                                                          ");
            //Table.Add("   ┌────┬─────────────────────────────────────────────────┐    ┌────┬─────────────────────────────────────────────────┐   ");
            //Table.Add("   │ -- │ 00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F │    │ -- │ 00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F │   ");
            //Table.Add("   ├────┼─────────────────────────────────────────────────┤    ├────┼─────────────────────────────────────────────────┤   ");
            //Table.Add("   │ 00 │                                                 │    │ 00 │                                                 │   ");
            //Table.Add("   │ 10 │                                                 │    │ 10 │                                                 │   ");
            //Table.Add("   │ 20 │                                                 │    │ 20 │                                                 │   ");
            //Table.Add("   │ 30 │                                                 │    │ 30 │                                                 │   ");
            //Table.Add("   │ 40 │                                                 │    │ 40 │                                                 │   ");
            //Table.Add("   │ 50 │                                                 │    │ 50 │                                                 │   ");
            //Table.Add("   │ 60 │                                                 │    │ 60 │                                                 │   ");
            //Table.Add("   │ 70 │                                                 │    │ 70 │                                                 │   ");
            //Table.Add("   │ 80 │                                                 │    │ 80 │                                                 │   ");
            //Table.Add("   │ 90 │                                                 │    │ 90 │                                                 │   ");
            //Table.Add("   │ A0 │                                                 │    │ A0 │                                                 │   ");
            //Table.Add("   │ B0 │                                                 │    │ B0 │                                                 │   ");
            //Table.Add("   │ C0 │                                                 │    │ C0 │                                                 │   ");
            //Table.Add("   │ D0 │                                                 │    │ D0 │                                                 │   ");
            //Table.Add("   │ E0 │                                                 │    │ E0 │                                                 │   ");
            //Table.Add("   │ F0 │ -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- │    │ F0 │ -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- │   ");
            //Table.Add("   ├────┼────────┬───────────────────┬────────────────────┤    ├────┼────────┬───────────────────┬────────────────────┤   ");
            //Table.Add("   │INFO│ MEM:-- │ SPEED: ----- BAUD │ TIMESTAMP:-------- │    │INFO│ MEM:-- │ SPEED: ----- BAUD │ TIMESTAMP:-------- │   ");
            //Table.Add("   └────┴────────┴───────────────────┴────────────────────┘    └────┴────────┴───────────────────┴────────────────────┘   ");
            Table.Add("                                                                                                                          ");
            Table.Add("┌─────────────────────────┐                                                                                               ");
            Table.Add("│ SCI-BUS TRANSMISSION    │ STATE: N/A                                                                                    ");
            Table.Add("├─────────────────────────┼────────────────────────────────────────────────────┬──────────────────────────┬──────────────┐");
            Table.Add("│ MESSAGE [HEX]           │ DESCRIPTION                                        │ VALUE                    │ UNIT         │");
            Table.Add("╞═════════════════════════╪════════════════════════════════════════════════════╪══════════════════════════╪══════════════╡");
            Table.Add("│                         │                                                    │                          │              │");
            Table.Add("└─────────────────────────┴────────────────────────────────────────────────────┴──────────────────────────┴──────────────┘");
            Table.Add("                                                                                                                          ");
            //Table.Add("   ┌────┬─────────────────────────────────────────────────┐    ┌────┬─────────────────────────────────────────────────┐   ");
            //Table.Add("   │ -- │  0  1  2  3  4  5  6  7  8  9  A  B  C  D  E  F │    │ FX │ 00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F │   ");
            //Table.Add("   ├────┼─────────────────────────────────────────────────┤    ├────┼─────────────────────────────────────────────────┤   ");
            //Table.Add("   │ 00 │ FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF │    │ 00 │ FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF │   ");
            //Table.Add("   │ 10 │ FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF │    │ 10 │ FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF │   ");
            //Table.Add("   │ 20 │ FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF │    │ 20 │ FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF │   ");
            //Table.Add("   │ 30 │ FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF │    │ 30 │ FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF │   ");
            //Table.Add("   │ 40 │ FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF │    │ 40 │ FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF │   ");
            //Table.Add("   │ 50 │ FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF │    │ 50 │ FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF │   ");
            //Table.Add("   │ 60 │ FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF │    │ 60 │ FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF │   ");
            //Table.Add("   │ 70 │ FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF │    │ 70 │ FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF │   ");
            //Table.Add("   │ 80 │ FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF │    │ 80 │ FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF │   ");
            //Table.Add("   │ 90 │ FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF │    │ 90 │ FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF │   ");
            //Table.Add("   │ A0 │ FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF │    │ A0 │ FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF │   ");
            //Table.Add("   │ B0 │ FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF │    │ B0 │ FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF │   ");
            //Table.Add("   │ C0 │ FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF │    │ C0 │ FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF │   ");
            //Table.Add("   │ D0 │ FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF │    │ D0 │ FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF │   ");
            //Table.Add("   │ E0 │ FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF │    │ E0 │ FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF │   ");
            //Table.Add("   │ F0 │ FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF │    │ F0 │ FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF │   ");
            //Table.Add("   ├────┼────────┬───────────────────┬────────────────────┤    ├────┼────────┬───────────────────┬────────────────────┤   ");
            //Table.Add("   │INFO│ MEM:-- │ SPEED:7812.5 BAUD │ TIMESTAMP:00A2FC68 │    │INFO│ MEM:FX │ SPEED: 62500 BAUD │ TIMESTAMP:00A2FC68 │   ");
            //Table.Add("   └────┴────────┴───────────────────┴────────────────────┘    └────┴────────┴───────────────────┴────────────────────┘   ");
            Table.Add("                                                                                                                          ");
        }

        public void UpdateBusState(byte bus, bool enabled, byte speed)
        {
            switch (bus)
            {
                case 0x01: // CCD-bus
                    if (!enabled)
                    {
                        string CCDBusHeaderText = CCDBusStateLineDisabled;
                        CCDBusHeaderText = CCDBusHeaderText.Replace("ID BYTES: ", "ID BYTES: " + CCDBusIDByteNum.ToString()).
                                                            Replace("# OF MESSAGES: RX=", "# OF MESSAGES: RX=" + CCDBusMsgRxCount.ToString()).
                                                            Replace(" TX=", " TX=" + CCDBusMsgTxCount.ToString());
                        CCDBusHeaderText = Util.Truncate(CCDBusHeaderText, EmptyLine.Length); // Replacing strings causes the base string to grow so cut it back to stay in line
                        Table.RemoveAt(CCDListStart - 4); // Remove old CCD-bus header text
                        Table.Insert(CCDListStart - 4, CCDBusHeaderText); // Insert new CCD-bus header text
                    }
                    break;
                default:
                    break;
            }

            SendPropertyChanged("TableUpdated");
        }

        public void UpdateTextTable(byte source, byte subdatacode, byte[] payload)
        {
            int location = 0;
            byte IDByte = payload[4]; // payload still contains the 4 timestamp bytes
            switch (source)
            {
                case 0x01: // CCD-bus message
                    CCDBusEnabled = true;
                    StringBuilder ccdlistitem = new StringBuilder(EmptyLine); // start with a pre-defined empty line
                    string ccdmsgtoinsert = String.Empty;
                    string ccddescriptiontoinsert = String.Empty;
                    string ccdvaluetoinsert = String.Empty;
                    string ccdunittoinsert = String.Empty;
                    byte[] ccdtimestamp = new byte[4];
                    byte[] ccdmessage = new byte[payload.Length - 4];
                    Array.Copy(payload, 0, ccdtimestamp, 0, 4); // copy timestamp only
                    Array.Copy(payload, 4, ccdmessage, 0, payload.Length - 4); // copy message only

                    if (ccdmessage.Length < 9) // max 8 byte fits the message column
                    {
                        ccdmsgtoinsert = Util.ByteToHexString(ccdmessage, 0, ccdmessage.Length) + " ";
                    }
                    else // Trim message (display 7 bytes only) and insert two dots at the end indicating there's more to it
                    {
                        ccdmsgtoinsert = Util.ByteToHexString(ccdmessage, 0, 7) + " .. ";
                    }

                    // Insert message hex bytes in the line
                    ccdlistitem.Remove(2, ccdmsgtoinsert.Length); // remove as much whitespaces as the length of the message
                    ccdlistitem.Insert(2, ccdmsgtoinsert); // insert message where whitespaces were

                    // Insert description in the line
                    ccddescriptiontoinsert = LT.GetCCDBusMessageDescription(IDByte);
                    if (ccddescriptiontoinsert != String.Empty)
                    {
                        ccdlistitem.Remove(28, ccddescriptiontoinsert.Length);
                        ccdlistitem.Insert(28, ccddescriptiontoinsert);
                    }

                    // Insert calculated value in the line
                    switch (IDByte)
                    {
                        case 0x24:
                            if (MainForm.Units == 1) // Imperial
                            {
                                ccdvaluetoinsert = (ccdmessage[1]).ToString();
                            }
                            if (MainForm.Units == 0) // Metric
                            {
                                ccdvaluetoinsert = (ccdmessage[2]).ToString();
                            }
                            break;
                        case 0x6D:
                            // TODO:  replace characters in the VIN string
                            ccdvaluetoinsert = VIN;
                            break;
                        case 0xCE:
                            if (MainForm.Units == 1) // Imperial
                            {
                                ImperialSlope = LT.GetCCDBusMessageSlope(0xCE)[0];
                                ccdvaluetoinsert = ((ccdmessage[1] << 24 | ccdmessage[2] << 16 | ccdmessage[3] << 8 | ccdmessage[4]) * ImperialSlope).ToString("0.000").Replace(",", ".");
                            }
                            if (MainForm.Units == 0) // Metric
                            {
                                MetricSlope = (LT.GetCCDBusMessageSlope(0xCE)[0] * LT.GetCCDBusMessageSlConv(0xCE)[0]);
                                ccdvaluetoinsert = ((ccdmessage[1] << 24 | ccdmessage[2] << 16 | ccdmessage[3] << 8 | ccdmessage[4]) * MetricSlope).ToString("0.000").Replace(",", ".");
                            }
                            break;
                        case 0xEE:
                            if (MainForm.Units == 1) // Imperial
                            {
                                ImperialSlope = LT.GetCCDBusMessageSlope(0xEE)[0];
                                ccdvaluetoinsert = ((ccdmessage[1] << 16 | ccdmessage[2] << 8 | ccdmessage[3]) * ImperialSlope).ToString("0.000").Replace(",", ".");
                            }
                            if (MainForm.Units == 0) // Metric
                            {
                                MetricSlope = (LT.GetCCDBusMessageSlope(0xEE)[0] * LT.GetCCDBusMessageSlConv(0xEE)[0]);
                                ccdvaluetoinsert = ((ccdmessage[1] << 16 | ccdmessage[2] << 8 | ccdmessage[3]) * MetricSlope).ToString("0.000").Replace(",", ".");
                            }
                            break;
                    }
                    ccdlistitem.Remove(81, ccdvaluetoinsert.Length);
                    ccdlistitem.Insert(81, ccdvaluetoinsert);

                    // Insert unit in the line
                    switch (IDByte)
                    {
                        case 0x24:
                            if (MainForm.Units == 1) // Imperial
                            {
                                ccdunittoinsert = LT.GetCCDBusMessageUnitImperial(0x24)[0];
                            }
                            if (MainForm.Units == 0) // Metric
                            {
                                ccdunittoinsert = LT.GetCCDBusMessageUnitMetric(0x24)[1];
                            }
                            break;
                        case 0xCE:
                            if (MainForm.Units == 1) // Imperial
                            {
                                ccdunittoinsert = LT.GetCCDBusMessageUnitImperial(0xCE)[0];
                            }
                            if (MainForm.Units == 0) // Metric
                            {
                                ccdunittoinsert = LT.GetCCDBusMessageUnitMetric(0xCE)[0];
                            }
                            break;
                        case 0xEE:
                            if (MainForm.Units == 1) // Imperial
                            {
                                ccdunittoinsert = LT.GetCCDBusMessageUnitImperial(0xEE)[0];
                            }
                            if (MainForm.Units == 0) // Metric
                            {
                                ccdunittoinsert = LT.GetCCDBusMessageUnitMetric(0xEE)[0];
                            }
                            break;
                    }
                    ccdlistitem.Remove(108, ccdunittoinsert.Length);
                    ccdlistitem.Insert(108, ccdunittoinsert);

                    // Now decide where to put this message in the table itself
                    if (!CCDIDList.Contains(IDByte)) // if this ID-byte is not on the list insert it into a new line
                    {
                        CCDBusIDByteNum++;

                        // Put B2 and F2 messages at the bottom of the table
                        if ((IDByte != 0xB2) && (IDByte != 0xF2)) // if it's not diagnostic request or response message
                        {
                            CCDIDList.Add(IDByte); // add ID-byte to the list
                            if (IDSorting)
                            {
                                CCDIDList.Sort(); // sort ID-bytes by ascending order
                            }
                            location = CCDIDList.FindIndex(x => x == IDByte); // now see where this new ID-byte ends up after sorting

                            if (CCDIDList.Count == 1)
                            {
                                Table.RemoveAt(CCDListStart);
                            }
                            else
                            {
                                CCDListEnd++; // increment start/end row-numbers (new data causes the table to grow)
                                CCDB2Start++;
                                CCDF2Start++;
                                SCIPCMListStart++;
                                SCIPCMListEnd++;
                                SCITCMListStart++;
                                SCITCMListEnd++;
                            }
                            Table.Insert(CCDListStart + location, ccdlistitem.ToString());
                        }
                    }
                    else
                    {
                        if ((IDByte != 0xB2) && (IDByte != 0xF2)) // if it's not diagnostic request or response message
                        {
                            location = CCDIDList.FindIndex(x => x == IDByte);
                            Table.RemoveAt(CCDListStart + location);
                            Table.Insert(CCDListStart + location, ccdlistitem.ToString());
                        }
                    }

                    if (IDByte == 0xB2)
                    {
                        if (!CCDBusB2MsgPresent)
                        {
                            CCDBusB2MsgPresent = true;
                        }
                        else
                        {
                            CCDBusIDByteNum--; // workaround: B2 is not added to the ID-byte list so everytime it appears it increases this number
                        }
                        Table.RemoveAt(CCDB2Start);
                        Table.Insert(CCDB2Start, ccdlistitem.ToString());
                    }
                    else if (IDByte == 0xF2)
                    {
                        if (!CCDBusF2MsgPresent)
                        {
                            CCDBusF2MsgPresent = true;
                        }
                        else
                        {
                            CCDBusIDByteNum--; // workaround: F2 is not added to the ID-byte list so everytime it appears it increases this number
                        }
                        Table.RemoveAt(CCDF2Start);
                        Table.Insert(CCDF2Start, ccdlistitem.ToString());
                    }

                    CCDBusMsgRxCount++;
                    string CCDBusHeaderText = CCDBusStateLineEnabled;
                    CCDBusHeaderText = CCDBusHeaderText.Replace("ID BYTES: ", "ID BYTES: " + CCDBusIDByteNum.ToString()).
                                                        Replace("# OF MESSAGES: RX=", "# OF MESSAGES: RX=" + CCDBusMsgRxCount.ToString()).
                                                        Replace(" TX=", " TX=" + CCDBusMsgTxCount.ToString());
                    CCDBusHeaderText = Util.Truncate(CCDBusHeaderText, EmptyLine.Length); // Replacing strings causes the base string to grow so cut it back to stay in line
                    Table.RemoveAt(CCDListStart - 4); // Remove old CCD-bus header text
                    Table.Insert(CCDListStart - 4, CCDBusHeaderText); // Insert new CCD-bus header text
                    break;
                case 0x02: // SCI-bus message (PCM)
                    SCIBusPCMEnabled = true;
                    StringBuilder scipcmlistitem = new StringBuilder(EmptyLine); // start with a pre-defined empty line
                    string scipcmmsgtoinsert = String.Empty;
                    byte[] pcmtimestamp = new byte[4];
                    byte[] pcmmessage = new byte[payload.Length - 4];
                    Array.Copy(payload, 0, pcmtimestamp, 0, 4); // copy timestamp only
                    Array.Copy(payload, 4, pcmmessage, 0, payload.Length - 4); // copy message only

                    // In case of high speed mode the request and response bytes are sent in separate packets.
                    // First packet is always the request bytes list, save them here and do nothing else.
                    // Second packet contains the response bytes, mix it with the request bytes and update the table.
                    if (subdatacode == 0x00) // low speed bytes
                    {
                        if (pcmmessage.Length < 9) // max 8 byte fits the message column
                        {
                            scipcmmsgtoinsert = Util.ByteToHexString(pcmmessage, 0, pcmmessage.Length) + " ";
                        }
                        else // Trim message and insert two dots at the end indicating there's more to it
                        {
                            scipcmmsgtoinsert = Util.ByteToHexString(pcmmessage, 0, 7) + " .. ";
                        }

                        scipcmlistitem.Remove(2, scipcmmsgtoinsert.Length); // remove as much whitespaces as the length of the message
                        scipcmlistitem.Insert(2, scipcmmsgtoinsert); // insert message where whitespaces were

                        // Now decide where to put this message in reality
                        if (!SCIPCMIDList.Contains(IDByte)) // if this ID-byte is not on the list
                        {
                            SCIBusPCMIDByteNum++;
                            SCIPCMIDList.Add(IDByte); // add ID-byte to the list
                            if (IDSorting)
                            {
                                SCIPCMIDList.Sort(); // sort ID-bytes by ascending order
                            }
                            location = SCIPCMIDList.FindIndex(x => x == IDByte); // now see where this new ID-byte ends up after sorting
                            if (SCIPCMIDList.Count == 1)
                            {
                                Table.RemoveAt(SCIPCMListStart);
                            }
                            else
                            {
                                SCIPCMListEnd++; // increment start/end row-numbers (new data causes the table to grow)
                                SCITCMListStart++;
                                SCITCMListEnd++;
                            }
                            Table.Insert(SCIPCMListStart + location, scipcmlistitem.ToString());
                        }
                        else // if this ID-byte is already displayed
                        {
                            location = SCIPCMIDList.FindIndex(x => x == IDByte);
                            Table.RemoveAt(SCIPCMListStart + location);
                            Table.Insert(SCIPCMListStart + location, scipcmlistitem.ToString());
                        }
                        SCIBusPCMMsgRxCount++;
                        // TODO: update header
                    }
                    if (subdatacode == 0x01) // high speed request bytes, just save them
                    {
                        SCIPCMReqMsgList.Clear(); // first clear previous bytes
                        for (int i = 0; i < pcmmessage.Length; i++)
                        {
                            SCIPCMReqMsgList.Add(pcmmessage[i]);
                        }
                    }
                    else if (subdatacode == 0x02) // high speed response bytes, mix them with saved request bytes
                    {
                        byte[] requestbytes = SCIPCMReqMsgList.ToArray();
                        List<byte> temp = new List<byte>(512);
                        byte[] pcmmessagemix;
                        temp.Add(pcmmessage[0]); // first byte is always the memory area byte
                        for (int i = 1; i < pcmmessage.Length; i++)
                        {
                            temp.Add(requestbytes[i]); // Add request byte first
                            temp.Add(pcmmessage[i]); // Add response byte next

                        }

                        pcmmessagemix = temp.ToArray();
                        File.AppendAllText(MainForm.PCMLogFilename, Util.ByteToHexString(pcmmessagemix, 0, pcmmessagemix.Length) + Environment.NewLine);

                        if (pcmmessagemix.Length < 9) // max 8 byte fits the message column
                        {
                            scipcmmsgtoinsert = Util.ByteToHexString(pcmmessagemix, 0, pcmmessagemix.Length) + " ";
                        }
                        else // Trim message and insert two dots at the end indicating there's more to it
                        {
                            scipcmmsgtoinsert = Util.ByteToHexString(pcmmessagemix, 0, 7) + " .. ";
                        }

                        scipcmlistitem.Remove(2, scipcmmsgtoinsert.Length); // remove as much whitespaces as the length of the message
                        scipcmlistitem.Insert(2, scipcmmsgtoinsert); // insert message where whitespaces were

                        // Now decide where to put this message in reality
                        if (!SCIPCMIDList.Contains(IDByte)) // if this ID-byte is not on the list
                        {
                            SCIBusPCMIDByteNum++;
                            SCIPCMIDList.Add(IDByte); // add ID-byte to the list
                            if (IDSorting)
                            {
                                SCIPCMIDList.Sort(); // sort ID-bytes by ascending order
                            }
                            location = SCIPCMIDList.FindIndex(x => x == IDByte); // now see where this new ID-byte ends up after sorting
                            if (SCIPCMIDList.Count == 1)
                            {
                                Table.RemoveAt(SCIPCMListStart);
                            }
                            else
                            {
                                SCIPCMListEnd++; // increment start/end row-numbers (new data causes the table to grow)
                                SCITCMListStart++;
                                SCITCMListEnd++;
                            }
                            Table.Insert(SCIPCMListStart + location, scipcmlistitem.ToString());
                        }
                        else // if this ID-byte is already displayed
                        {
                            location = SCIPCMIDList.FindIndex(x => x == IDByte);
                            Table.RemoveAt(SCIPCMListStart + location);
                            Table.Insert(SCIPCMListStart + location, scipcmlistitem.ToString());
                        }
                        SCIBusPCMMsgRxCount++;
                        // TODO: update header
                    }
                    break;
                case 0x03: // SCI-bus message (TCM)
                    SCIBusTCMEnabled = true;
                    StringBuilder scitcmlistitem = new StringBuilder(EmptyLine); // start with a pre-defined empty line
                    string scitcmmsgtoinsert = String.Empty;
                    byte[] tcmtimestamp = new byte[4];
                    byte[] tcmmessage = new byte[payload.Length - 4];
                    Array.Copy(payload, 0, tcmtimestamp, 0, 4); // copy timestamp only
                    Array.Copy(payload, 4, tcmmessage, 0, payload.Length - 4); // copy message only

                    // In case of high speed mode the request and response bytes are sent in separate packets.
                    // First packet is always the request bytes list (subdatacode 0x01), save them here and do nothing else.
                    // Second packet contains the response bytes (subdatacode 0x02), mix it with the request bytes and update the table.
                    if (subdatacode == 0x00) // low speed bytes
                    {
                        if (tcmmessage.Length < 9) // max 8 byte fits the message column
                        {
                            scitcmmsgtoinsert = Util.ByteToHexString(tcmmessage, 0, tcmmessage.Length) + " ";
                        }
                        else // Trim message and insert two dots at the end indicating there's more to it
                        {
                            scitcmmsgtoinsert = Util.ByteToHexString(tcmmessage, 0, 7) + " .. ";
                        }

                        scitcmlistitem.Remove(2, scitcmmsgtoinsert.Length); // remove as much whitespaces as the length of the message
                        scitcmlistitem.Insert(2, scitcmmsgtoinsert); // insert message where whitespaces were

                        // Now decide where to put this message in reality
                        if (!SCITCMIDList.Contains(IDByte)) // if this ID-byte is not on the list
                        {
                            SCIBusTCMIDByteNum++;
                            SCITCMIDList.Add(IDByte); // add ID-byte to the list
                            if (IDSorting)
                            {
                                SCITCMIDList.Sort(); // sort ID-bytes by ascending order
                            }
                            location = SCITCMIDList.FindIndex(x => x == IDByte); // now see where this new ID-byte ends up after sorting
                            if (SCITCMIDList.Count == 1)
                            {
                                Table.RemoveAt(SCITCMListStart);
                            }
                            else
                            {
                                SCITCMListEnd++; // increment start/end row-numbers (new data causes the table to grow)
                            }
                            Table.Insert(SCITCMListStart + location, scitcmlistitem.ToString());
                        }
                        else // if this ID-byte is already displayed
                        {
                            location = SCITCMIDList.FindIndex(x => x == IDByte);
                            Table.RemoveAt(SCITCMListStart + location);
                            Table.Insert(SCITCMListStart + location, scitcmlistitem.ToString());
                        }
                        SCIBusTCMMsgRxCount++;
                        // TODO: update header
                    }
                    if (subdatacode == 0x01) // high speed request bytes, just save them
                    {
                        SCITCMReqMsgList.Clear(); // first clear previous bytes
                        for (int i = 0; i < tcmmessage.Length; i++)
                        {
                            SCITCMReqMsgList.Add(tcmmessage[i]);
                        }
                    }
                    else if (subdatacode == 0x02) // high speed response bytes, mix them with saved request bytes
                    {
                        byte[] requestbytes = SCITCMReqMsgList.ToArray();
                        List<byte> temp = new List<byte>(512);
                        byte[] tcmmessagemix;
                        temp.Add(tcmmessage[0]); // first byte is always the memory area byte
                        for (int i = 1; i < tcmmessage.Length; i++)
                        {
                            temp.Add(requestbytes[i]); // Add request byte first
                            temp.Add(tcmmessage[i]); // Add response byte next
                        }

                        tcmmessagemix = temp.ToArray();
                        File.AppendAllText(MainForm.TCMLogFilename, Util.ByteToHexString(tcmmessagemix, 0, tcmmessagemix.Length) + Environment.NewLine);

                        if (tcmmessagemix.Length < 9) // max 8 byte fits the message column
                        {
                            scitcmmsgtoinsert = Util.ByteToHexString(tcmmessagemix, 0, tcmmessagemix.Length) + " ";
                        }
                        else // Trim message and insert two dots at the end indicating there's more to it
                        {
                            scitcmmsgtoinsert = Util.ByteToHexString(tcmmessagemix, 0, 7) + " .. ";
                        }

                        scitcmlistitem.Remove(2, scitcmmsgtoinsert.Length); // remove as much whitespaces as the length of the message
                        scitcmlistitem.Insert(2, scitcmmsgtoinsert); // insert message where whitespaces were

                        // Now decide where to put this message in reality
                        if (!SCITCMIDList.Contains(IDByte)) // if this ID-byte is not on the list
                        {
                            SCIBusTCMIDByteNum++;
                            SCITCMIDList.Add(IDByte); // add ID-byte to the list
                            if (IDSorting)
                            {
                                SCITCMIDList.Sort(); // sort ID-bytes by ascending order
                            }
                            location = SCITCMIDList.FindIndex(x => x == IDByte); // now see where this new ID-byte ends up after sorting
                            if (SCITCMIDList.Count == 1)
                            {
                                Table.RemoveAt(SCITCMListStart);
                            }
                            else
                            {
                                SCITCMListEnd++; // increment start/end row-numbers (new data causes the table to grow)
                            }
                            Table.Insert(SCITCMListStart + location, scitcmlistitem.ToString());
                        }
                        else // if this ID-byte is already displayed
                        {
                            location = SCITCMIDList.FindIndex(x => x == IDByte);
                            Table.RemoveAt(SCITCMListStart + location);
                            Table.Insert(SCITCMListStart + location, scitcmlistitem.ToString());
                        }
                        SCIBusTCMMsgRxCount++;
                        // TODO: update header
                    }
                    break;
                default:
                    break;
            }

            SendPropertyChanged("TableUpdated");
        }

        private void SendPropertyChanged(string property)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
    }
}