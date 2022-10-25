using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChryslerScanner
{
    public class PCIDiagnosticsTable
    {
        public delegate void TableUpdatedEventHandler(object sender, EventArgs e);
        public event TableUpdatedEventHandler TableUpdated;
        public List<string> Table = new List<string>();
        public List<ushort> IDByteList = new List<ushort>();
        public List<byte> UniqueIDByteList = new List<byte>();
        public List<byte> _2426IDByteList = new List<byte>(2);

        public int _24Row = 5; // PCI request
        public int _26Row = 6; // PCI response
        public int _48Row = 7; // OBD2 response
        public int _68Row = 8; // OBD2 request
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
            Table.Add("│ PCI-BUS MODULES         │ STATE: N/A                                                                                   ");
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
                    if (!_2426IDByteList.Contains(0x24)) _2426IDByteList.Add(0x24);
                    _2426IDByteList.Sort();
                    Table.RemoveAt(_24Row);
                    Table.Insert(_24Row, row);
                    LastUpdatedLine = _24Row;
                    break;
                case 0x26:
                    if (!_2426IDByteList.Contains(0x26)) _2426IDByteList.Add(0x26);
                    _2426IDByteList.Sort();
                    Table.RemoveAt(_26Row);
                    Table.Insert(_26Row, row);
                    LastUpdatedLine = _26Row;
                    break;
                case 0x48:
                    if (!_2426IDByteList.Contains(0x48)) _2426IDByteList.Add(0x48);
                    _2426IDByteList.Sort();
                    Table.RemoveAt(_48Row);
                    Table.Insert(_48Row, row);
                    LastUpdatedLine = _48Row;
                    break;
                case 0x68:
                    if (!_2426IDByteList.Contains(0x68)) _2426IDByteList.Add(0x68);
                    _2426IDByteList.Sort();
                    Table.RemoveAt(_68Row);
                    Table.Insert(_68Row, row);
                    LastUpdatedLine = _68Row;
                    break;
                default:
                    break;
            }
        }

        public virtual void OnTableUpdated(EventArgs e)
        {
            TableUpdated?.Invoke(this, e);
        }
    }
}
