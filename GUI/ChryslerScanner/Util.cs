using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ChryslerScanner
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
                StringBuilder ret = new StringBuilder();

                if (textBox.Text != "") ret.Append(Environment.NewLine + Environment.NewLine);

                ret.Append(text);

                if (bytes != null) ret.Append(Environment.NewLine + ByteToHexString(bytes, 0, bytes.Length));

                if (textBox.TextLength + ret.Length > textBox.MaxLength)
                {
                    textBox.Clear();
                    GC.Collect();
                }

                textBox.AppendText(ret.ToString());

                // Save generated text to a logfile.
                if (textBox.Name == "USBTextBox") File.AppendAllText(MainForm.USBTextLogFilename, ret.ToString());

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

        /// <summary>
        /// Calculate checksum value of a byte array.
        /// </summary>
        /// <param name="data">Input byte array.</param>
        /// <param name="index">Start offset in byte array<./param>
        /// <param name="length">Number of bytes to take into calculation.</param>
        /// <returns>Checksum byte.</returns>
        public static byte ChecksumCalculator(byte[] data, int index, int length)
        {
            byte checksum = 0;

            for (int i = index; i < length; i++)
            {
                checksum += data[i];
            }

            return checksum;
        }

        /// <summary>
        /// Calculate CRC-8 J1850 value of a byte array.
        /// </summary>
        /// <param name="data">Input byte array.</param>
        /// <param name="index">Start offset in byte array<./param>
        /// <param name="length">Number of bytes to take into calculation.</param>
        /// <returns>CRC-8 J1850 byte.</returns>
        public static byte CRCCalculator(byte[] data, int index, int length)
        {
            byte crc = 0xFF, poly, bit_count;
            int byte_count;
            int byte_point = 0;
            byte bit_point;

            for (byte_count = index; byte_count < length; ++byte_count, ++byte_point)
            {
                for (bit_count = 0, bit_point = 0x80; bit_count < 8; ++bit_count, bit_point >>= 1)
                {
                    if ((bit_point & data[byte_point]) > 0) // case for new bit = 1
                    {
                        poly = (byte)(IsBitSet(crc, 7) ? 1 : 0x1C);
                        crc = (byte)(((crc << 1) | 1) ^ poly);
                    }
                    else // case for new bit = 0
                    {
                        poly = (byte)(IsBitSet(crc, 7) ? 0x1D : 0);
                        crc = (byte)((crc << 1) ^ poly);
                    }
                }
            }

            return (byte)~crc;
        }

        public static byte[] GetSKIMUnlockKey(byte[] seed, string VIN)
        {
            byte[] result = new byte[3];

            if (seed.Length == 2) // CCD
            {
                result[0] = seed[1];
                result[1] = seed[1];
                result[2] = seed[0];
            }
            else if (seed.Length == 4) // PCI
            {
                result[0] = seed[1];
                result[1] = seed[2];
                result[2] = seed[3];
            }
            else
            {
                return null;
            }

            byte temp = Encoding.ASCII.GetBytes(VIN)[16]; // start with 17th VIN character

            result[2] += temp;
            result[2] ^= temp;
            result[1] += result[2];
            result[0] += result[1];
            result[0] ^= temp;

            temp = Encoding.ASCII.GetBytes(VIN)[15]; // continue with 16th VIN character

            result[2] += temp;
            result[2] ^= temp;
            result[1] += result[2];
            result[0] += result[1];
            result[0] ^= temp;

            temp = Encoding.ASCII.GetBytes(VIN)[13]; // continue with 14th VIN character

            result[2] += temp;
            result[2] ^= temp;
            result[1] += result[2];
            result[0] += result[1];
            result[0] ^= temp;

            temp = Encoding.ASCII.GetBytes(VIN)[8]; // finish with 9th VIN character (also known as security field)

            result[2] += temp;
            result[2] ^= temp;
            result[1] += result[2];
            result[0] += result[1];
            result[0] ^= temp;

            return result;
        }
    }
}
