using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ChryslerCCDSCIScanner
{
    public class CCDDiagnosticsTable
    {
        public delegate void TableUpdatedEventHandler(object sender, EventArgs e);
        public event TableUpdatedEventHandler TableUpdated;
        public List<string> Table = new List<string>();
        public List<ushort> IDByteList = new List<ushort>();
        public List<byte> UniqueIDByteList = new List<byte>();
        public List<byte> B2F2IDByteList = new List<byte>(2);

        public int B2Row = 5;
        public int F2Row = 6;
        public const int listStart = 8;

        public CCDDiagnosticsTable()
        {
            InitCCDTable();
            OnTableUpdated(EventArgs.Empty);
        }

        public void InitCCDTable()
        {
            Table.Clear();
            Table.Add("┌─────────────────────────┐                                                                                              ");
            Table.Add("│ CCD-BUS MODULES         │ STATE: N/A                                                                                   ");
            Table.Add("├─────────────────────────┼─────────────────────────────────────────────────────┬─────────────────────────┬─────────────┐");
            Table.Add("│ MESSAGE [HEX]           │ DESCRIPTION                                         │ VALUE                   │ UNIT        │");
            Table.Add("╞═════════════════════════╪═════════════════════════════════════════════════════╪═════════════════════════╪═════════════╡");
            Table.Add("│ B2 -- -- -- -- --       │ REQUEST  |                                          │                         │             │");
            Table.Add("│ F2 -- -- -- -- --       │ RESPONSE |                                          │                         │             │");
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

            if (!IDByteList.Contains(modifiedID) && ((modifiedID >> 8) != 0xB2) && ((modifiedID >> 8) != 0xF2))
            {
                byte uniqueID = (byte)((modifiedID >> 8) & 0xFF);
                if (!UniqueIDByteList.Contains(uniqueID)) UniqueIDByteList.Add(uniqueID);

                IDByteList.Add(modifiedID);
                IDByteList.Sort();
                location = IDByteList.FindIndex(x => x == modifiedID);

                if (IDByteList.Count == 1)
                {
                    Table.RemoveAt(listStart);
                    Table.Insert(listStart, row);
                }
                else
                {
                    Table.Insert(listStart + location, row);
                }
            }
            else if (IDByteList.Contains(modifiedID) && ((modifiedID >> 8) != 0xB2) && ((modifiedID >>8) != 0xF2)) // if it's not diagnostic request or response message
            {
                location = IDByteList.FindIndex(x => x == modifiedID);
                Table.RemoveAt(listStart + location);
                Table.Insert(listStart + location, row);
            }

            switch (modifiedID >> 8)
            {
                case 0xB2:
                    if (!B2F2IDByteList.Contains(0xB2)) B2F2IDByteList.Add(0xB2);
                    B2F2IDByteList.Sort();
                    Table.RemoveAt(B2Row);
                    Table.Insert(B2Row, row);
                    //Table.RemoveAt(F2Row);
                    //Table.Insert(F2Row, "│ F2 -- -- -- -- --       │ RESPONSE |                                          │                         │             │");
                    break;
                case 0xF2:
                    if (!B2F2IDByteList.Contains(0xF2)) B2F2IDByteList.Add(0xF2);
                    B2F2IDByteList.Sort();
                    Table.RemoveAt(F2Row);
                    Table.Insert(F2Row, row);
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
