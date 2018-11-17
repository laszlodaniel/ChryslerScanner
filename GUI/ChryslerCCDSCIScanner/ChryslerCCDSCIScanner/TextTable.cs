using System.Collections.Generic;
using System.ComponentModel;
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
        private List<string> SCITCMMsgList = new List<string>(256);
        private List<byte> CCDIDList = new List<byte>(256);
        private List<byte> SCIPCMIDList = new List<byte>(256);
        private List<byte> SCITCMIDList = new List<byte>(256);
        private List<string> DetailList = new List<string>();

        private string EmptyLine  = "│                         │                                                    │                          │              │";
        
        private int CCDListStart = 5; // row number
        private int CCDListEnd = 5;
        private int SCIPCMListStart = 14;
        private int SCIPCMListEnd = 14;
        private int SCITCMListStart = 23;
        private int SCITCMListEnd = 23;

        private XmlDocument VehicleProfiles = new XmlDocument();

        public TextTable(string VehicleProfilesFileName)
        {
            VehicleProfiles.Load(VehicleProfilesFileName);

            foreach (XmlNode node in VehicleProfiles.DocumentElement.ChildNodes)
            {
                string text = node.InnerText;
            }

            Table.Add("┌─────────────────────────┐                                                                                               ");
            Table.Add("│ CCD-BUS MODULES         │                                                                                               ");
            Table.Add("├─────────────────────────┼────────────────────────────────────────────────────┬──────────────────────────┬──────────────┐");
            Table.Add("│ MESSAGE [HEX]           │ DESCRIPTION                                        │ VALUE                    │ UNIT         │");
            Table.Add("╞═════════════════════════╪════════════════════════════════════════════════════╪══════════════════════════╪══════════════╡");
            Table.Add("│                         │                                                    │                          │              │");
            Table.Add("└─────────────────────────┴────────────────────────────────────────────────────┴──────────────────────────┴──────────────┘");
            Table.Add("  DETAILS:                                                                                                                ");
            Table.Add("                                                                                                                          ");
            Table.Add("┌─────────────────────────┐                                                                                               ");
            Table.Add("│ SCI-BUS_A_ENGINE        │                                                                                               ");
            Table.Add("├─────────────────────────┼────────────────────────────────────────────────────┬──────────────────────────┬──────────────┐");
            Table.Add("│ MESSAGE [HEX]           │ DESCRIPTION                                        │ VALUE                    │ UNIT         │");
            Table.Add("╞═════════════════════════╪════════════════════════════════════════════════════╪══════════════════════════╪══════════════╡");
            Table.Add("│                         │                                                    │                          │              │");
            Table.Add("└─────────────────────────┴────────────────────────────────────────────────────┴──────────────────────────┴──────────────┘");
            Table.Add("  DETAILS:                                                                                                                ");
            //Table.Add("   ┌────┬─────────────────────────────────────────────────┐    ┌────┬─────────────────────────────────────────────────┐   ");
            //Table.Add("   │ -- │ 00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F │    │ F4 │ 00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F │   ");
            //Table.Add("   ├────┼─────────────────────────────────────────────────┤    ├────┼─────────────────────────────────────────────────┤   ");
            //Table.Add("   │ 00 │ FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF │    │ 00 │    FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF │   ");
            //Table.Add("   │ 10 │ 10 11 12 FF FF FF FF FF FF FF FF FF FF FF FF FF │    │ 10 │ FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF │   ");
            //Table.Add("   │ 20 │ FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF │    │ 20 │ FF FF    FF FF FF FF FF FF FF FF FF FF       FF │   ");
            //Table.Add("   │ 30 │ FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF │    │ 30 │ FF             FF FF FF       FF FF FF FF FF FF │   ");
            //Table.Add("   │ 40 │ FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF │    │ 40 │ FF FF FF FF FF FF    FF FF FF FF FF FF FF FF FF │   ");
            //Table.Add("   │ 50 │ FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF │    │ 50 │ FF FF FF FF FF FF FF FF                         │   ");
            //Table.Add("   │ 60 │ FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF │    │ 60 │       FF FF FF FF FF FF FF FF FF FF FF FF FF FF │   ");
            //Table.Add("   │ 70 │ FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF │    │ 70 │ FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF │   ");
            //Table.Add("   │ 80 │ FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF │    │ 80 │ FF FF FF FF FF FF FF FF FF FF FF FF FF          │   ");
            //Table.Add("   │ 90 │ FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF │    │ 90 │                                                 │   ");
            //Table.Add("   │ A0 │ FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF │    │ A0 │                   FF FF                         │   ");
            //Table.Add("   │ B0 │ FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF │    │ B0 │                      FF FF FF FF                │   ");
            //Table.Add("   │ C0 │ FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF │    │ C0 │ FF FF FF FF             FF FF FF FF FF FF FF FF │   ");
            //Table.Add("   │ D0 │ FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF │    │ D0 │       FF FF FF FF FF FF FF FF FF FF FF FF FF FF │   ");
            //Table.Add("   │ E0 │ FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF │    │ E0 │    FF                                           │   ");
            //Table.Add("   │ F0 │ FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF │    │ F0 │                                                 │   ");
            //Table.Add("   ├────┼────────┬───────────────────┬────────────────────┤    ├────┼────────┬───────────────────┬────────────────────┤   ");
            //Table.Add("   │INFO│ MEM:-- │ SPEED:7812.5 BAUD │ TIMESTAMP:00A2FC68 │    │INFO│ MEM:F4 │ SPEED: 62500 BAUD │ TIMESTAMP:00A2FC68 │   ");
            //Table.Add("   └────┴────────┴───────────────────┴────────────────────┘    └────┴────────┴───────────────────┴────────────────────┘   ");
            Table.Add("                                                                                                                          ");
            Table.Add("┌─────────────────────────┐                                                                                               ");
            Table.Add("│ SCI-BUS_A_TRANSMISSION  │                                                                                               ");
            Table.Add("├─────────────────────────┼────────────────────────────────────────────────────┬──────────────────────────┬──────────────┐");
            Table.Add("│ MESSAGE [HEX]           │ DESCRIPTION                                        │ VALUE                    │ UNIT         │");
            Table.Add("╞═════════════════════════╪════════════════════════════════════════════════════╪══════════════════════════╪══════════════╡");
            Table.Add("│                         │                                                    │                          │              │");
            Table.Add("└─────────────────────────┴────────────────────────────────────────────────────┴──────────────────────────┴──────────────┘");
            Table.Add("  DETAILS:                                                                                                                ");
            //Table.Add("   ┌────┬─────────────────────────────────────────────────┐    ┌────┬─────────────────────────────────────────────────┐   ");
            //Table.Add("   │ -- │ 00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F │    │ FX │ 00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F │   ");
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

        public void UpdateTextTable(byte source, byte[] data, int index, int count)
        {
            int location = 0;
            switch (source)
            {
                case 0x01: // CCD-bus message
                    StringBuilder ccdlistitem = new StringBuilder(EmptyLine); // start with a pre-defined empty line
                    string ccdmsgtoinsert = Util.ByteToHexString(data, index, count) + " ";
                    ccdlistitem.Remove(2, ccdmsgtoinsert.Length); // remove as much whitespaces as the length of the message
                    ccdlistitem.Insert(2, ccdmsgtoinsert); // insert message where whitespaces were

                    // Now decide where to put this message in reality
                    if (!CCDIDList.Contains(data[index])) // if this ID-byte is not in the list
                    {
                        CCDIDList.Add(data[index]); // add ID-byte to the list
                        CCDIDList.Sort(); // sort ID-bytes by ascending order
                        location = CCDIDList.FindIndex(x => x == data[index]); // now see where this new ID-byte ends up after sorting
                        if (CCDIDList.Count == 1)
                        {
                            Table.RemoveAt(CCDListStart);
                        }
                        else
                        {
                            CCDListEnd++; // increment start/end row-numbers (new data causes the table to grow)
                            SCIPCMListStart++;
                            SCIPCMListEnd++;
                            SCITCMListStart++;
                            SCITCMListEnd++;
                        }
                        Table.Insert(CCDListStart + location, ccdlistitem.ToString());
                    }
                    else // if this ID-byte is already displayed
                    {
                        location = CCDIDList.FindIndex(x => x == data[index]);
                        Table.RemoveAt(CCDListStart + location);
                        Table.Insert(CCDListStart + location, ccdlistitem.ToString());
                    }
                    break;
                case 0x02: // SCI-bus message (PCM)
                    StringBuilder scipcmlistitem = new StringBuilder(EmptyLine); // start with a pre-defined empty line
                    string scipcmmsgtoinsert = Util.ByteToHexString(data, index, count) + " ";
                    scipcmlistitem.Remove(2, scipcmmsgtoinsert.Length); // remove as much whitespaces as the length of the message
                    scipcmlistitem.Insert(2, scipcmmsgtoinsert); // insert message where whitespaces were

                    // Now decide where to put this message in reality
                    if (!SCIPCMIDList.Contains(data[index])) // if this ID-byte is not in the list
                    {
                        SCIPCMIDList.Add(data[index]); // add ID-byte to the list
                        SCIPCMIDList.Sort(); // sort ID-bytes by ascending order
                        location = SCIPCMIDList.FindIndex(x => x == data[index]); // now see where this new ID-byte ends up after sorting
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
                        location = SCIPCMIDList.FindIndex(x => x == data[index]);
                        Table.RemoveAt(SCIPCMListStart + location);
                        Table.Insert(SCIPCMListStart + location, scipcmlistitem.ToString());
                    }
                    break;
                case 0x03: // SCI-bus message (TCM)
                    StringBuilder scitcmlistitem = new StringBuilder(EmptyLine); // start with a pre-defined empty line
                    string scitcmmsgtoinsert = Util.ByteToHexString(data, index, count) + " ";
                    scitcmlistitem.Remove(2, scitcmmsgtoinsert.Length); // remove as much whitespaces as the length of the message
                    scitcmlistitem.Insert(2, scitcmmsgtoinsert); // insert message where whitespaces were

                    // Now decide where to put this message in reality
                    if (!SCITCMIDList.Contains(data[index])) // if this ID-byte is not in the list
                    {
                        SCITCMIDList.Add(data[index]); // add ID-byte to the list
                        SCITCMIDList.Sort(); // sort ID-bytes by ascending order
                        location = SCITCMIDList.FindIndex(x => x == data[index]); // now see where this new ID-byte ends up after sorting
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
                        location = SCITCMIDList.FindIndex(x => x == data[index]);
                        Table.RemoveAt(SCITCMListStart + location);
                        Table.Insert(SCITCMListStart + location, scitcmlistitem.ToString());
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