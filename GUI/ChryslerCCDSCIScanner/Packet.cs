using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Threading;

namespace ChryslerCCDSCIScanner
{
    public class Packet : IDisposable
    {
        public SerialPort Serial = new SerialPort();
        public delegate void PacketReceivedEventHandler(object sender, EventArgs e);
        public event PacketReceivedEventHandler PacketReceived;

        private bool isDisposed = false;

        public struct PacketRx
        {
            public byte[] buffer;
            public int length;
            public byte bus;
            public byte command;
            public byte mode;
            public byte[] payload;
            public byte checksum;
        }

        public struct PacketTx
        {
            public byte[] buffer;
            public int length;
            public byte bus;
            public byte command;
            public byte mode;
            public byte[] payload;
            public byte checksum;
        }

        public PacketRx rx = new PacketRx();
        public PacketTx tx = new PacketTx();

        public enum Bus : byte
        {
            usb = 0x00,
            ccd = 0x01,
            pcm = 0x02,
            tcm = 0x03,
            pci = 0x04
        }

        public enum Command : byte
        {
            reset = 0x00,
            handshake = 0x01,
            status = 0x02,
            settings = 0x03,
            request = 0x04,
            response = 0x05,
            msgTx = 0x06,
            msgRx = 0x07,
            debug = 0x0E,
            error = 0x0F
        }

        public enum ResetMode : byte
        {
            resetInit = 0x00,
            resetDone = 0x01
        }

        public enum HandshakeMode : byte
        {
            handshakeOnly = 0x00,
            handshakeAndStatus = 0x01
        }

        public enum StatusMode : byte
        {
            none = 0x00
        }

        public enum SettingsMode : byte
        {
            leds = 0x01,
            setCCDBus = 0x02,
            setSCIBus = 0x03,
            setRepeatBehavior = 0x04,
            setLCD = 0x05
        }

        public enum RequestMode : byte
        {
            hardwareFirmwareInfo = 0x01,
            timestamp = 0x02,
            batteryVoltage = 0x03,
            extEEPROMChecksum = 0x04,
            CCDBusVoltages = 0x05
        }

        public enum ResponseMode : byte
        {
            hardwareFirmwareInfo = 0x01,
            timestamp = 0x02,
            batteryVoltage = 0x03,
            extEEPROMChecksum = 0x04,
            CCDBusVoltages = 0x05
        }

        public enum MsgTxMode : byte
        {
            stop = 0x01,
            single = 0x02,
            list = 0x03,
            repeatedSingle = 0x04,
            repeatedList = 0x05
        }

        public enum MsgRxMode : byte
        {
            stop = 0x01,
            single = 0x02,
            list = 0x03,
            repeatedSingle = 0x04,
            repeatedList = 0x05
        }

        public enum DebugMode : byte
        {
            randomCCDBusMessages = 0x01,
            readIntEEPROMbyte = 0x02,
            readIntEEPROMblock = 0x03,
            readExtEEPROMbyte = 0x04,
            readExtEEPROMblock = 0x05,
            writeIntEEPROMbyte = 0x06,
            writeIntEEPROMblock = 0x07,
            writeExtEEPROMbyte = 0x08,
            writeExtEEPROMblock = 0x09,
            setArbitraryUARTSpeed = 0x0A,
            initBootstrapMode = 0x0B,
            writeWorkerFunction = 0x0C,
            restorePCMEEPROM = 0xF0
        }

        public enum ErrorMode : byte
        {
            ok = 0x00,
            errorLengthInvalidValue = 0x01,
            errorDatacodeInvalidCommand = 0x02,
            errorSubDatacodeInvalidValue = 0x03,
            errorPayloadInvalidValues = 0x04,
            errorPacketChecksumInvalidValue = 0x05,
            errorPacketTimeoutOccured = 0x06,
            errorBufferOverflow = 0x07,
            errorDatacodeInvalidBus = 0x08,
            errorSCILsNoResponse = 0xF6,
            errorNotEnoughMCURAM = 0xF7,
            errorSCIHsMemoryPtrNoResponse = 0xF8,
            errorSCIHsInvalidMemoryPtr = 0xF9,
            errorSCIHsNoResponse = 0xFA,
            errorEEPNotFound = 0xFB,
            errorEEPRead = 0xFC,
            errorEEPWrite = 0xFD,
            errorInternal = 0xFE,
            errorFatal = 0xFF
        }

        public enum OnOffMode : byte
        {
            off = 0x00,
            on = 0x01
        }

        public enum BaudMode : byte
        {
            extraLowBaud = 0x01,
            lowBaud = 0x02,
            highBaud = 0x03,
            extraHighBaud = 0x04
        }

        public enum SCISpeedMode : byte
        {
            lowSpeed = 0x01,
            highSpeed = 0x02
        }

        public byte[] expectedHandshake_V1 = new byte[] { 0x3D, 0x00, 0x17, 0x81, 0x00, 0x43, 0x48, 0x52, 0x59, 0x53, 0x4C, 0x45, 0x52, 0x43, 0x43, 0x44, 0x53, 0x43, 0x49, 0x53, 0x43, 0x41, 0x4E, 0x4E, 0x45, 0x52, 0xF4 };
        public byte[] expectedHandshake_V2 = new byte[] { 0x3D, 0x00, 0x11, 0x81, 0x00, 0x43, 0x48, 0x52, 0x59, 0x53, 0x4C, 0x45, 0x52, 0x53, 0x43, 0x41, 0x4E, 0x4E, 0x45, 0x52, 0x45 };

        public Packet()
        {
            rx.buffer = new byte[] { };
            tx.buffer = new byte[] { };
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing && (Serial != null))
                {
                    Serial.Dispose();
                }

                isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void OnPacketReceived(EventArgs e)
        {
            PacketReceived?.Invoke(this, e);
        }

        /// <summary>
        /// Read the current opened serial port asynchronously in an infinte loop and raise an event if a valid communication packet is received.
        /// </summary>
        /// <returns>None.</returns>
        public async void MonitorSerialPort()
        {
            while (true)
            {
                try
                {
                    rx.buffer = await SerialPortExtension.ReadPacketAsync(Serial);
                    
                    // Verify checksum.
                    byte checksum = 0;
                    int checksumLocation = rx.buffer.Length - 1;

                    for (int i = 0; i < checksumLocation; i++) checksum += rx.buffer[i];

                    if (checksum == rx.buffer[checksumLocation])
                    {
                        ParsePacket();
                        OnPacketReceived(EventArgs.Empty); // raise OnPacketReceived event and start analyzing the received packet where subscribed
                    }
                }
                catch (IOException)
                {
                    return;
                }
                catch (InvalidOperationException)
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Check if full communication packet is received.
        /// </summary>
        /// <param name="packet">Input byte array.</param>
        /// <returns>True if input byte array seems to be a full communication packet, otherwise returns false.</returns>
        public static bool IsPacketComplete(byte[] packet)
        {
            if ((packet.Length > 5) && (packet[0] == 0x3D))
            {
                int length = (packet[1] << 8) + packet[2] + 4;

                if (packet.Length < length) return false;
                else return true;
            }
            else return false;
        }

        /// <summary>
        /// Interpret a valid communication packet and update class fields with its values.
        /// </summary>
        /// <returns>None.</returns>
        public void ParsePacket()
        {
            rx.length = (rx.buffer[1] << 8) + rx.buffer[2];
            Array.Resize(ref rx.buffer, rx.length + 4);
            rx.bus = (byte)((rx.buffer[3] >> 4) & 0x07);
            rx.command = (byte)(rx.buffer[3] & 0x0F);
            rx.mode = rx.buffer[4];
            if (rx.length > 2)
            {
                int payloadLength = rx.length - 2;
                rx.payload = new byte[payloadLength];
                Array.Copy(rx.buffer, 5, rx.payload, 0, payloadLength);
            }
            else rx.payload = null;
            rx.checksum = rx.buffer[rx.buffer.Length - 1];
        }

        /// <summary>
        /// Generate communication packet from the current class fields and update the buffer field.
        /// </summary>
        /// <returns>None.</returns>
        public void GeneratePacket()
        {
            ushort packetLength;
            byte checksum = 0;

            if (tx.payload == null) packetLength = 6;
            else packetLength = (ushort)(tx.payload.Length + 6);

            List<byte> packet = new List<byte>(packetLength);

            byte lengthHB = (byte)(((packetLength - 4) >> 8) & 0xFF);
            byte lengthLB = (byte)((packetLength - 4) & 0xFF);
            byte datacode = (byte)(((tx.bus << 4) & 0x70) + (tx.command & 0x0F));
            byte subdatacode = tx.mode;

            packet.AddRange(new byte[] { 0x3D, lengthHB, lengthLB, datacode, subdatacode});
            if (tx.payload != null)  packet.AddRange(tx.payload);

            for (int i = 0; i < packet.Count; i++)
            {
                checksum += packet[i];
            }

            packet.Add(checksum);

            tx.buffer = packet.ToArray();
        }
    }
}
