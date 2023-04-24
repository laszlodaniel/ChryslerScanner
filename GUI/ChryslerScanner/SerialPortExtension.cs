using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Threading.Tasks;

namespace ChryslerScanner
{
    public static class SerialPortExtension
    {
        /// <summary>
        /// Read a packet from the SerialPort asynchronously.
        /// </summary>
        /// <remarks>
        /// Read the input one byte at a time, add byte to the overall response byte array, once the packet ends then wait for another packet to start.
        /// </remarks>
        /// <param name="_stream">Underlying BaseStream of Serial port.</param>
        /// <returns>A packet read from the input.</returns>
        public static async Task<byte[]> ReadPacketAsync(Stream _stream)
        {
            byte[] buffer = new byte[1];
            List<byte> packet = new List<byte>();

            while (MainForm.Packet.SP.IsOpen)
            {
                try
                {
                    await _stream.ReadAsync(buffer, 0, 1);
                }
                catch
                {
                    return null;
                }
                
                packet.Add(buffer[0]);

                if (packet[0] != 0x3D)
                {
                    packet.Clear(); // make sure that the first byte is always the SYNC byte
                }

                if (packet.Count >= 3) // check length
                {
                    if ((packet[1] << 8) + packet[2] >= 1024)
                    {
                        packet.Clear(); // invalid packet
                    }
                }

                byte status = Packet.PacketStatus(packet.ToArray());

                if (status == 0) return packet.ToArray();
            }

            return null;
        }

        /// <summary>
        /// Write a packet to the SerialPort asynchronously.
        /// </summary>
        /// <param name="_stream">Underlying BaseStream of Serial port.</param>
        /// <param name="packet">The packet to send.</param>
        /// <returns></returns>
        public static async Task WritePacketAsync(Stream _stream, byte[] packet)
        {
            try
            {
                await _stream.WriteAsync(packet, 0, packet.Length);
                //await _stream.FlushAsync();
            }
            catch
            {
                // TODO
            }
            finally
            {
                // TODO
            }
        }
    }
}
