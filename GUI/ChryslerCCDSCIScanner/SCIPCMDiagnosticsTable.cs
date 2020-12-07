using System;
using System.Collections.Generic;

namespace ChryslerCCDSCIScanner
{
    public class SCIPCMDiagnosticsTable
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

        public SCIPCMDiagnosticsTable()
        {
            InitSCIPCMTable();
            InitRAMDumpTable();
            OnTableUpdated(EventArgs.Empty);
        }

        public void InitSCIPCMTable()
        {
            Table.Clear();
            Table.Add("┌─────────────────────────┐                                                                                              ");
            Table.Add("│ SCI-BUS ENGINE          │ STATE: N/A                                                                                   ");
            Table.Add("├─────────────────────────┼─────────────────────────────────────────────────────┬─────────────────────────┬─────────────┐");
            Table.Add("│ MESSAGE [HEX]           │ DESCRIPTION                                         │ VALUE                   │ UNIT        │");
            Table.Add("╞═════════════════════════╪═════════════════════════════════════════════════════╪═════════════════════════╪═════════════╡");
            Table.Add("│                         │                                                     │                         │             │");
            Table.Add("└─────────────────────────┴─────────────────────────────────────────────────────┴─────────────────────────┴─────────────┘");
        }

        public void InitRAMDumpTable()
        {
            RAMDumpTable.Clear();
            RAMDumpTable.Add("                                                        ");
            RAMDumpTable.Add("┌────┬─────────────────────────────────────────────────┐");
            RAMDumpTable.Add("│ FX │ 00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F │");
            RAMDumpTable.Add("├────┼─────────────────────────────────────────────────┤");
            RAMDumpTable.Add("│ 00 │                                                 │");
            RAMDumpTable.Add("│ 10 │                                                 │");
            RAMDumpTable.Add("│ 20 │                                                 │");
            RAMDumpTable.Add("│ 30 │                                                 │");
            RAMDumpTable.Add("│ 40 │                                                 │");
            RAMDumpTable.Add("│ 50 │                                                 │");
            RAMDumpTable.Add("│ 60 │                                                 │");
            RAMDumpTable.Add("│ 70 │                                                 │");
            RAMDumpTable.Add("│ 80 │                                                 │");
            RAMDumpTable.Add("│ 90 │                                                 │");
            RAMDumpTable.Add("│ A0 │                                                 │");
            RAMDumpTable.Add("│ B0 │                                                 │");
            RAMDumpTable.Add("│ C0 │                                                 │");
            RAMDumpTable.Add("│ D0 │                                                 │");
            RAMDumpTable.Add("│ E0 │                                                 │");
            RAMDumpTable.Add("├────┴─────────────────────────────────────────────────┤");
            RAMDumpTable.Add("│                              TIMESTAMP: 00:00:00.000 │");
            RAMDumpTable.Add("└──────────────────────────────────────────────────────┘");
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

        public void AddRAMTableDump(byte[] data)
        {
            // Don't update table if speed change byte is received.
            if (data[4] == 0xFE) return;

            // Clear table on RAM table change.
            if (data[4] != RAMTableAddress) InitRAMDumpTable();
            RAMTableAddress = data[4];

            // Replace RAM table byte in the header.
            RAMDumpTable[2] = RAMDumpTable[2].Remove(2, 2).Insert(2, Util.ByteToHexString(data, 4, 1));

            // Replace timestamp in the footer.
            TimeSpan timestampMillis = TimeSpan.FromMilliseconds((data[0] << 24) + (data[1] << 16) + (data[2] << 8) + data[3]);
            DateTime timestampDate = DateTime.Today.Add(timestampMillis);
            string timestampString = timestampDate.ToString("HH:mm:ss.fff");
            RAMDumpTable[20] = RAMDumpTable[20].Remove(42, 12).Insert(42, timestampString);

            // Replace RAM values.
            byte[] values = new byte[data.Length - 5];
            Array.Copy(data, 5, values, 0, values.Length);

            for (int i = 0; i < (values.Length / 2); i++)
            {
                int currentRow = 4 + (values[i * 2] >> 4);
                int currentColumn = 7 + 3 * (values[i * 2] & 0x0F);
                RAMDumpTable[currentRow] = RAMDumpTable[currentRow].Remove(currentColumn, 2).Insert(currentColumn, Util.ByteToHexString(values, i * 2 + 1, 1));
            }

            // Add this table to the main table or replace the current one.
            if (RAMDumpTableVisible) Table.RemoveRange(Table.Count - 22, 22);
            Table.AddRange(RAMDumpTable);
            RAMDumpTableVisible = true;
        }

        public virtual void OnTableUpdated(EventArgs e)
        {
            TableUpdated?.Invoke(this, e);
        }
    }
}
