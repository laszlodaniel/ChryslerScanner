using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChryslerScanner.Helpers
{
    public static class UnlockAlgorithm
    {
        public enum Controllers
        {
            SBEC = 1,
            EATX = 2,
            JTEC = 3
        }

        public enum SecurityLevels
        {
            Level1 = 1,
            Level2 = 2,
            Level3 = 3
        }

        public static byte[] GetSecurityKey(Controllers controller, SecurityLevels level, byte[] seed)
        {
            if (seed.Length != 2)
                return null;

            if (seed.All(s => s == 0)) // if all bytes are zero
                return null;

            ushort seedword = (ushort)((seed[0] << 8) + seed[1]);
            ushort keyword = 0;

            switch (controller)
            {
                case Controllers.SBEC:
                case Controllers.EATX:
                case Controllers.JTEC:
                {
                    switch (level)
                    {
                        case SecurityLevels.Level1: // EEPROM/RAM write
                        {
                            keyword = (ushort)((seedword << 2) + 0x9018);
                            break;
                        }
                        case SecurityLevels.Level2: // Special functions (SKIM reset, reversed by Piton)
                        {
                            keyword = (ushort)(seedword & 0xFF00);
                            keyword |= (ushort)(keyword >> 8);
                            ushort mask = (ushort)(seedword & 0xFF);
                            mask |= (ushort)(mask << 8);
                            keyword ^= 0x9340; // polinom
                            keyword += 0x1010;
                            keyword ^= mask;
                            keyword += 0x1911;
                            uint tmp = (uint)((keyword << 16) | keyword);
                            keyword += (ushort)(tmp >> 3);
                            break;
                        }
                        case SecurityLevels.Level3: // Bootstrap flash write
                        {
                            keyword = (ushort)((seedword + 0x247C) | 5); // add magic word and set minimum number of bit rotations
                            byte tmp = (byte)(keyword & 0x0F); // determine number of bit rotations
                            keyword = (ushort)((keyword >> tmp) | (keyword << (16 - tmp))); // rotate bits to the right
                            keyword |= 0x247C; // apply same magic word again
                            break;
                        }
                    }
                    break;
                }
            }

            byte[] key = BitConverter.GetBytes(keyword);

            if (key.Length != 2)
                return null;

            if (BitConverter.IsLittleEndian)
                Array.Reverse(key);

            return key;
        }

        public static byte[] GetSKIMUnlockKey(byte[] seed, string VIN)
        {
            if (VIN.Length != 17)
                return null;

            if (seed.All(s => s == 0)) // if all bytes are zero
                return null;

            byte[] key = new byte[3]; // key length is always 3 bytes

            switch (seed.Length) // seed length may be 2 or 4 bytes
            {
                case 2: // CCD
                {
                    key[0] = seed[1];
                    key[1] = seed[1];
                    key[2] = seed[0];
                    break;
                }
                case 4: // PCI
                {
                    key[0] = seed[1];
                    key[1] = seed[2];
                    key[2] = seed[3];
                    break;
                }
                default: // invalid seed length
                {
                    return null;
                }
            }

            byte[] VINChars = Encoding.ASCII.GetBytes(VIN);
            List<byte> cycles = new List<byte>() { 16, 15, 13, 8 }; // VIN characters to use

            foreach (byte index in cycles)
            {
                byte tmp = VINChars[index];

                key[2] += tmp;
                key[2] ^= tmp;
                key[1] += key[2];
                key[0] += key[1];
                key[0] ^= tmp;
            }

            return key;
        }
    }
}
