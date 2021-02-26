using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ChryslerCCDSCIScanner
{
    public static class Util
    {
        /// <summary>
        /// Convert part of a byte array to hexadecimal formatted string.
        /// </summary>
        /// <param name="data">Input byte array.</param>
        /// <param name="offset">First byte in input array to start.</param>
        /// <param name="length">Length of data starting from specified offset.</param>
        /// <param name="maxNumberCount">Number of input bytes after which a new line character is inserted. Zero means no new line.</param>
        /// <returns>A formatted string.</returns>
        public static string ByteToHexString(byte[] data, int offset = 0, int length = 1, int maxNumberCount = 16)
        {
            if (data.Length > 0)
            {
                StringBuilder ret = new StringBuilder();

                for (int i = offset; i < (offset + length); i++)
                {
                    if ((maxNumberCount > 0) && (ret.Length != 0) && ((i % maxNumberCount) == offset)) ret.Append(Environment.NewLine);
                    ret.Append(Convert.ToString(data[i], 16).PadLeft(2, '0').PadRight(3, ' ').ToUpper());
                }

                ret.Remove(ret.Length - 1, 1); // remove last whitespace caused by PadRight(3, ' ')

                return ret.ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Convert byte array to hexadecimal formatted string.
        /// </summary>
        /// <param name="data">Input byte array.</param>
        /// <param name="maxNumberCount">Number of input bytes after which a new line character is inserted. Zero means no new line.</param>
        /// <returns>A formatted string.</returns>
        public static string ByteToHexStringSimple(byte[] data, int maxNumberCount = 16)
        {
            return ByteToHexString(data, 0, data.Length, maxNumberCount);
        }

        /// <summary>
        /// Convert hexadecimal formatted string to byte array.
        /// </summary>
        /// <param name="str">Input string.</param>
        /// <returns>A byte array.</returns>
        public static byte[] HexStringToByte(string str)
        {
            // Remove whitespaces, commas, semi-colons and hex number identifiers.
            string ret = str.Trim().Replace(" ", string.Empty).Replace(",", string.Empty).Replace(";", string.Empty).Replace("$", string.Empty).Replace("0x", string.Empty);
            try
            {
                return Enumerable.Range(0, ret.Length).Where(x => x % 2 == 0).Select(x => Convert.ToByte(ret.Substring(x, 2), 16)).ToArray();
            }
            catch
            {
                return new byte[] { }; // return an empty byte array if something is wrong
            }
        }

        /// <summary>
        /// Update a specific TextBox control with commentary line and a packet in hex-string format.
        /// </summary>
        /// <param name="textBox">Target TextBox.</param>
        /// <param name="text">Commentary line to first appear.</param>
        /// <param name="bytes">Packet to display.</param>
        public static void UpdateTextBox(TextBox textBox, string text, byte[] bytes = null)
        {
            if (!textBox.IsDisposed)
            {
                string ret = string.Empty;

                if (textBox.Text != "") ret += Environment.NewLine + Environment.NewLine;

                ret += text;

                if (bytes != null) ret += Environment.NewLine + ByteToHexString(bytes, 0, bytes.Length);

                if (textBox.TextLength + ret.Length > textBox.MaxLength)
                {
                    textBox.Clear();
                    GC.Collect();
                }

                textBox.AppendText(ret);

                // Save generated text to a logfile.
                if (textBox.Name == "USBTextBox") File.AppendAllText(MainForm.USBTextLogFilename, ret);

                // Save raw USB packet to a binary logfile.
                using (BinaryWriter writer = new BinaryWriter(File.Open(MainForm.USBBinaryLogFilename, FileMode.Append)))
                {
                    if (bytes != null)
                    {
                        writer.Write(bytes);
                        writer.Close();
                    }
                }
            }
        }

        /// <summary>
        /// Compare 2 byte arrays.
        /// </summary>
        /// <param name="first">First byte array.</param>
        /// <param name="second">Second byte array.</param>
        /// <param name="index">Index in the arrays where the comparison begins.</param>
        /// <param name="length">Number of bytes compared.</param>
        /// <returns>True if the two arrays are the same, otherwise returns false.</returns>
        public static bool CompareArrays(byte[] first, byte[] second, int index, int length)
        {
            bool ret = false;

            if ((first.Length < (index + length)) || (second.Length < (index + length))) return false;

            for (int i = index; i < (index + length); i++)
            {
                if (first[i] == second[i]) ret = true;
                else ret = false;
            }

            return ret;
        }

        /// <summary>
        /// Convert a UNIX timestamp to regular date and time.
        /// </summary>
        /// <param name="unixTimeStamp">UNIX timestamp.</param>
        /// <returns>DateTime representation of the UNIX timestamp.</returns>
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        /// <summary>
        /// Chop off input string's end at the specified length.
        /// </summary>
        /// <param name="value">Input string.</param>
        /// <param name="maxLength">Maximum length.</param>
        /// <returns>DateTime representation of the UNIX timestamp.</returns>
        public static string TruncateString(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        /// <summary>
        /// Set bit (0->1) in a byte at a given position.
        /// </summary>
        /// <param name="value">Input byte.</param>
        /// <param name="position">Bit position to modify.</param>
        /// <returns>Modified byte.</returns>
        public static byte SetBit(byte value, byte position)
        {
            return value |= (byte)(1 << position);
        }

        /// <summary>
        /// Clear bit (1->0) in a byte at a given position.
        /// </summary>
        /// <param name="value">Input byte.</param>
        /// <param name="position">Bit position to modify.</param>
        /// <returns>Modified byte.</returns>
        public static byte ClearBit(byte value, byte position)
        {
            return value &= (byte)(~(1 << position));
        }

        /// <summary>
        /// Invert bit (1->0, 0->1) in a byte at a given position.
        /// </summary>
        /// <param name="value">Input byte.</param>
        /// <param name="position">Bit position to modify.</param>
        /// <returns>Modified byte.</returns>
        public static byte InvertBit(byte value, byte position)
        {
            return value ^= (byte)(1 << position);
        }

        /// <summary>
        /// Check if bit is set (1) in a byte at a given position.
        /// </summary>
        /// <param name="value">Input byte.</param>
        /// <param name="position">Bit position to check.</param>
        /// <returns>True if bit is set (1), false if bit is clear (0).</returns>
        public static bool IsBitSet(byte value, byte position)
        {
            return (value & (1 << position)) != 0;
        }

        /// <summary>
        /// Check if bit is clear (0) in a byte at a given position.
        /// </summary>
        /// <param name="value">Input byte.</param>
        /// <param name="position">Bit position to check.</param>
        /// <returns>True if bit is clear (0), false if bit is set (1).</returns>
        public static bool IsBitClear(byte value, byte position)
        {
            return !IsBitSet(value, position);
        }
    }
}
