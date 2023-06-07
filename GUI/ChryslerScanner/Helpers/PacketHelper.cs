using System;
using ChryslerScanner.Models;

namespace ChryslerScanner.Helpers
{
    public static class PacketHelper
    {
        public enum Bus : byte
        {
            USB = 0x00,
            CCD = 0x01,
            PCM = 0x02,
            TCM = 0x03,
            PCI = 0x04
        }

        public enum Command : byte
        {
            Reset = 0x00,
            Handshake = 0x01,
            Status = 0x02,
            Settings = 0x03,
            Request = 0x04,
            Response = 0x05,
            MsgTx = 0x06,
            MsgRx = 0x07,
            Debug = 0x0E,
            Error = 0x0F
        }

        public enum ResetMode : byte
        {
            ResetInit = 0x00,
            ResetDone = 0x01
        }

        public enum HandshakeMode : byte
        {
            HandshakeOnly = 0x00,
            HandshakeAndStatus = 0x01
        }

        public enum StatusMode : byte
        {
            None = 0x00
        }

        public enum SettingsMode : byte
        {
            LEDs = 0x01,
            SetCCDBus = 0x02,
            SetSCIBus = 0x03,
            SetRepeatBehavior = 0x04,
            SetLCD = 0x05,
            SetPCIBus = 0x06,
            SetProgVolt = 0x07,
            SetUARTBaudrate = 0x08
        }

        public enum RequestMode : byte
        {
            HardwareFirmwareInfo = 0x01,
            Timestamp = 0x02,
            BatteryVoltage = 0x03,
            ExtEEPROMChecksum = 0x04,
            CCDBusVoltages = 0x05,
            VBBVolts = 0x06,
            VPPVolts = 0x07,
            AllVolts = 0x08
        }

        public enum ResponseMode : byte
        {
            HardwareFirmwareInfo = 0x01,
            Timestamp = 0x02,
            BatteryVoltage = 0x03,
            ExtEEPROMChecksum = 0x04,
            CCDBusVoltages = 0x05,
            VBBVolts = 0x06,
            VPPVolts = 0x07,
            AllVolts = 0x08
        }

        public enum MsgTxMode : byte
        {
            Stop = 0x01,
            Single = 0x02,
            List = 0x03,
            RepeatedSingle = 0x04,
            RepeatedList = 0x05,
            SingleVPP = 0x82
        }

        public enum MsgRxMode : byte
        {
            Stop = 0x01,
            Single = 0x02,
            List = 0x03,
            RepeatedSingle = 0x04,
            RepeatedList = 0x05,
            SingleVPP = 0x82
        }

        public enum DebugMode : byte
        {
            RandomCCDBusMessages = 0x01,
            ReadIntEEPROMbyte = 0x02,
            ReadIntEEPROMblock = 0x03,
            ReadExtEEPROMbyte = 0x04,
            ReadExtEEPROMblock = 0x05,
            WriteIntEEPROMbyte = 0x06,
            WriteIntEEPROMblock = 0x07,
            WriteExtEEPROMbyte = 0x08,
            WriteExtEEPROMblock = 0x09,
            SetArbitraryUARTSpeed = 0x0A,
            InitBootstrapMode = 0x0B,
            UploadWorkerFunction = 0x0C,
            StartWorkerFunction = 0x0D,
            ExitWorkerFunction = 0x0E,
            DefaultSettings = 0xE0,
            GetRandomNumber = 0xE1,
            RestorePCMEEPROM = 0xF0,
            GetAW9523Data = 0xFE,
            Test = 0xFF
        }

        public enum ErrorMode : byte
        {
            Ok = 0x00,
            ErrorLengthInvalidValue = 0x01,
            ErrorDatacodeInvalidCommand = 0x02,
            ErrorSubDatacodeInvalidValue = 0x03,
            ErrorPayloadInvalidValues = 0x04,
            ErrorPacketChecksumInvalidValue = 0x05,
            ErrorPacketTimeoutOccured = 0x06,
            ErrorBufferOverflow = 0x07,
            ErrorInvalidBus = 0x08,
            ErrorSCILsNoResponse = 0xF6,
            ErrorNotEnoughMCURAM = 0xF7,
            ErrorSCIHsMemoryPtrNoResponse = 0xF8,
            ErrorSCIHsInvalidMemoryPtr = 0xF9,
            ErrorSCIHsNoResponse = 0xFA,
            ErrorEEPNotFound = 0xFB,
            ErrorEEPRead = 0xFC,
            ErrorEEPWrite = 0xFD,
            ErrorInternal = 0xFE,
            ErrorFatal = 0xFF
        }

        public enum OnOffMode : byte
        {
            Off = 0x00,
            On = 0x01
        }

        public enum BaudMode : byte
        {
            ExtraLowBaud = 0x01,
            LowBaud = 0x02,
            HighBaud = 0x03,
            ExtraHighBaud = 0x04
        }

        public enum SCISpeedMode : byte
        {
            LowSpeed = 0x01,
            HighSpeed = 0x02
        }

        public const byte PacketSync = 0x3D;

        public static byte[] ExpectedHandshake_V1 = new byte[] { 0x3D, 0x00, 0x17, 0x81, 0x00, 0x43, 0x48, 0x52, 0x59, 0x53, 0x4C, 0x45, 0x52, 0x43, 0x43, 0x44, 0x53, 0x43, 0x49, 0x53, 0x43, 0x41, 0x4E, 0x4E, 0x45, 0x52, 0xF4 };
        public static byte[] ExpectedHandshake_V2 = new byte[] { 0x3D, 0x00, 0x11, 0x81, 0x00, 0x43, 0x48, 0x52, 0x59, 0x53, 0x4C, 0x45, 0x52, 0x53, 0x43, 0x41, 0x4E, 0x4E, 0x45, 0x52, 0x45 };

        public static byte[] Serialize(Packet packet)
        {
            if (packet == null)
                return null;

            packet.Sync = PacketSync;

            if (packet.Payload != null)
            {
                packet.Length = 2 + packet.Payload.Length;
            }
            else
            {
                packet.Length = 2;
            }

            if (packet.Length > 2044)
                return null;

            byte[] bytes = new byte[packet.Length + 4];

            bytes[0] = packet.Sync;
            bytes[1] = (byte)((packet.Length >> 8) & 0xFF);
            bytes[2] = (byte)(packet.Length & 0xFF);
            bytes[3] = (byte)(((packet.Bus << 4) & 0x70) + (packet.Command & 0x0F));

            if (packet.Direction) // true = packet comes from scanner
                bytes[3] += 0x80;

            bytes[4] = packet.Mode;

            if (packet.Length > 2)
            {
                if ((packet.Payload == null) || (packet.Payload.Length < (packet.Length - 2)))
                    return null;

                Array.Copy(packet.Payload, 0, bytes, 5, packet.Payload.Length);
            }

            packet.Checksum = Util.ChecksumCalculator(bytes, 0, bytes.Length - 1);

            bytes[bytes.Length - 1] = packet.Checksum;

            return bytes;
        }

        public static Packet Deserialize(byte[] bytes)
        {
            Packet packet = new Packet();

            packet.Sync = bytes[0];

            if (packet.Sync != PacketSync)
                return null;

            packet.Length = (bytes[1] << 8) + bytes[2];

            if (Util.IsBitSet(bytes[3], 7))
            {
                packet.Direction = true;
            }
            else
            {
                packet.Direction = false;
            }

            packet.Bus = (byte)((bytes[3] >> 4) & 0x07);
            packet.Command = (byte)(bytes[3] & 0x0F);
            packet.Mode = bytes[4];

            if (packet.Length > 2)
            {
                int PayloadLength = packet.Length - 2;
                packet.Payload = new byte[PayloadLength];
                Array.Copy(bytes, 5, packet.Payload, 0, PayloadLength);
            }
            else
            {
                packet.Payload = null;
            }

            packet.Checksum = bytes[bytes.Length - 1];

            if (packet.Checksum != Util.ChecksumCalculator(bytes, 0, bytes.Length - 1))
            {
                return null;
            }

            return packet;
        }
    }
}
