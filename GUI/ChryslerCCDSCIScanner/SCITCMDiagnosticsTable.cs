using System;
using System.Collections.Generic;

namespace ChryslerCCDSCIScanner
{
    public class SCITCMDiagnosticsTable
    {
        public delegate void TableUpdatedEventHandler(object sender, EventArgs e);
        public event TableUpdatedEventHandler TableUpdated;
        public List<string> Table = new List<string>();
        public List<string> RAMDumpTable = new List<string>();
        public bool RAMDumpTableVisible = false;
        public byte RAMTableAddress = 0;
        public List<ushort> IDByteList = new List<ushort>();
        public List<byte> UniqueIDByteList = new List<byte>();

        public const int listStart = 5;
        public int lastUpdatedLine = 1;

        public SCITCMDiagnosticsTable()
        {
            InitSCITCMTable();
            OnTableUpdated(EventArgs.Empty);
        }

        public void InitSCITCMTable()
        {
            Table.Clear();
            Table.Add("┌─────────────────────────┐                                                                                              ");
            Table.Add("│ SCI-BUS TRANSMISSION    │ STATE: N/A                                                                                   ");
            Table.Add("├─────────────────────────┼─────────────────────────────────────────────────────┬─────────────────────────┬─────────────┐");
            Table.Add("│ MESSAGE [HEX]           │ DESCRIPTION                                         │ VALUE                   │ UNIT        │");
            Table.Add("╞═════════════════════════╪═════════════════════════════════════════════════════╪═════════════════════════╪═════════════╡");
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

            if (!IDByteList.Contains(modifiedID))
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

                lastUpdatedLine = listStart + location;
            }
            else
            {
                location = IDByteList.FindIndex(x => x == modifiedID);
                Table.RemoveAt(listStart + location);
                Table.Insert(listStart + location, row);
                lastUpdatedLine = listStart + location;
            }
        }

        public virtual void OnTableUpdated(EventArgs e)
        {
            TableUpdated?.Invoke(this, e);
        }
    }
}
