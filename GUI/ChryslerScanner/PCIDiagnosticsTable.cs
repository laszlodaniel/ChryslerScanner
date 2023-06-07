using System;
using System.Collections.Generic;

namespace ChryslerScanner
{
    public class PCIDiagnosticsTable
    {
        public delegate void TableUpdatedEventHandler(object sender, EventArgs e);
        public event TableUpdatedEventHandler TableUpdated;
        public List<string> Table = new List<string>();
        public List<ushort> IDByteList = new List<ushort>();
        public List<byte> UniqueIDByteList = new List<byte>();
        public List<byte> IDByte2426List = new List<byte>(2);

        public int Row24 = 5; // PCI request
        public int Row26 = 6; // PCI response
        public int Row48 = 7; // OBD2 response
        public int Row68 = 8; // OBD2 request
        public const int ListStart = 10;
        public int LastUpdatedLine = 1;

        public PCIDiagnosticsTable()
        {
            InitPCITable();
            OnTableUpdated(EventArgs.Empty);
        }

        public void InitPCITable()
        {
            Table.Clear();
            Table.Add("┌─────────────────────────┐                                                                                              ");
            Table.Add("│ PCI-BUS (SAE J1850 VPW) │ STATE: N/A                                                                                   ");
            Table.Add("├─────────────────────────┼─────────────────────────────────────────────────────┬─────────────────────────┬─────────────┐");
            Table.Add("│ MESSAGE [HEX]           │ DESCRIPTION                                         │ VALUE                   │ UNIT        │");
            Table.Add("╞═════════════════════════╪═════════════════════════════════════════════════════╪═════════════════════════╪═════════════╡");
            Table.Add("│ 24 -- -- -- -- -- --    │ REQUEST  |                                          │                         │             │");
            Table.Add("│ 26 -- -- -- -- -- --    │ RESPONSE |                                          │                         │             │");
            Table.Add("│ 48 -- -- -- -- -- --    │ REQUEST  |                                          │                         │             │");
            Table.Add("│ 68 -- -- -- -- --       │ RESPONSE |                                          │                         │             │");
            Table.Add("├─────────────────────────┼─────────────────────────────────────────────────────┼─────────────────────────┼─────────────┤");
            Table.Add("│                         │                                                     │                         │             │");
            Table.Add("└─────────────────────────┴─────────────────────────────────────────────────────┴─────────────────────────┴─────────────┘");
        }

        public void UpdateHeader(string row)
        {
            Table.RemoveAt(1);
            Table.Insert(1, row);
            OnTableUpdated(EventArgs.Empty); // raise event so that MainForm can update the listbox
        }

        public void AddRow(ushort modifiedID, string row)
        {
            int location = 0;

            if (!IDByteList.Contains(modifiedID) && ((modifiedID >> 8) != 0x24) && ((modifiedID >> 8) != 0x26) && ((modifiedID >> 8) != 0x48) && ((modifiedID >> 8) != 0x68))
            {
                byte uniqueID = (byte)((modifiedID >> 8) & 0xFF);
                if (!UniqueIDByteList.Contains(uniqueID)) UniqueIDByteList.Add(uniqueID);

                IDByteList.Add(modifiedID);

                if (Properties.Settings.Default.SortByID == true) IDByteList.Sort();

                location = IDByteList.FindIndex(x => x == modifiedID);

                if (IDByteList.Count == 1)
                {
                    Table.RemoveAt(ListStart);
                    Table.Insert(ListStart, row);
                }
                else
                {
                    Table.Insert(ListStart + location, row);
                }

                LastUpdatedLine = ListStart + location;
            }
            else if (IDByteList.Contains(modifiedID) && ((modifiedID >> 8) != 0x24) && ((modifiedID >> 8) != 0x26) && ((modifiedID >> 8) != 0x48) && ((modifiedID >> 8) != 0x68)) // if it's not diagnostic request or response message
            {
                location = IDByteList.FindIndex(x => x == modifiedID);
                Table.RemoveAt(ListStart + location);
                Table.Insert(ListStart + location, row);
                LastUpdatedLine = ListStart + location;
            }

            switch (modifiedID >> 8)
            {
                case 0x24:
                {
                    if (!IDByte2426List.Contains(0x24)) IDByte2426List.Add(0x24);
                    IDByte2426List.Sort();
                    Table.RemoveAt(Row24);
                    Table.Insert(Row24, row);
                    LastUpdatedLine = Row24;
                    break;
                }
                case 0x26:
                {
                    if (!IDByte2426List.Contains(0x26)) IDByte2426List.Add(0x26);
                    IDByte2426List.Sort();
                    Table.RemoveAt(Row26);
                    Table.Insert(Row26, row);
                    LastUpdatedLine = Row26;
                    break;
                }
                case 0x48:
                {
                    if (!IDByte2426List.Contains(0x48)) IDByte2426List.Add(0x48);
                    IDByte2426List.Sort();
                    Table.RemoveAt(Row48);
                    Table.Insert(Row48, row);
                    LastUpdatedLine = Row48;
                    break;
                }
                case 0x68:
                {
                    if (!IDByte2426List.Contains(0x68)) IDByte2426List.Add(0x68);
                    IDByte2426List.Sort();
                    Table.RemoveAt(Row68);
                    Table.Insert(Row68, row);
                    LastUpdatedLine = Row68;
                    break;
                }
            }
        }

        public virtual void OnTableUpdated(EventArgs e)
        {
            TableUpdated?.Invoke(this, e);
        }
    }
}
