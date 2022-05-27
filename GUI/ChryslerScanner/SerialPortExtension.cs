using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
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
        /// <param name="serialPort">The port to read data from.</param>
        /// <returns>A packet read from the input.</returns>
        public static async Task<byte[]> ReadPacketAsync(this SerialPort serialPort)
        {
            byte[] buffer = new byte[1];
            List<byte> packet = new List<byte>();

            while (true)
            {
                await serialPort.BaseStream.ReadAsync(buffer, 0, 1);
                
                packet.Add(buffer[0]);

                if (packet[0] != 0x3D) packet.Clear(); // make sure that the first byte is always the SYNC byte

                if (Packet.IsPacketComplete(packet.ToArray())) return packet.ToArray();
            }
        }

        /// <summary>
        /// Write a packet to the SerialPort asynchronously.
        /// </summary>
        /// <param name="serialPort">The port to send packet to.</param>
        /// <param name="packet">The packet to send.</param>
        /// <returns></returns>
        public static async Task WritePacketAsync(this SerialPort serialPort, byte[] packet)
        {
            await serialPort.BaseStream.WriteAsync(packet, 0, packet.Length);
            await serialPort.BaseStream.FlushAsync();
        }
    }
}
