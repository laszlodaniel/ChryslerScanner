using System;
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
            ushort seedword = (ushort)((seed[0] << 8) + seed[1]);

            //ushort seedword = 0;

            //if (BitConverter.IsLittleEndian)
            //{
            //    byte[] reverse = seed;
            //    Array.Reverse(reverse);
            //    seedword = BitConverter.ToUInt16(reverse, 0);
            //}
            //else
            //{
            //    seedword = BitConverter.ToUInt16(seed, 0);
            //}

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
                            byte i = (byte)(keyword & 0x0F); // determine number of bit rotations

                            while (i > 0) // rotate bits to the right
                            {
                                i--;
                                if ((keyword & 1) == 1) keyword = ((ushort)((keyword >> 1) | 0x8000)); // rightmost bit is transferred to the leftmost position
                                else keyword >>= 1;
                            }

                            keyword |= 0x247C; // apply same magic word again
                            break;
                        }
                    }
                    break;
                }
            }

            byte[] key = BitConverter.GetBytes(keyword);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(key);

            return key;
        }

        public static byte[] GetSKIMUnlockKey(byte[] seed, string VIN)
        {
            byte[] key = new byte[3];

            switch (seed.Length)
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
                default:
                {
                    return null;
                }
            }

            byte temp = Encoding.ASCII.GetBytes(VIN)[16]; // start with 17th VIN character

            key[2] += temp;
            key[2] ^= temp;
            key[1] += key[2];
            key[0] += key[1];
            key[0] ^= temp;

            temp = Encoding.ASCII.GetBytes(VIN)[15]; // continue with 16th VIN character

            key[2] += temp;
            key[2] ^= temp;
            key[1] += key[2];
            key[0] += key[1];
            key[0] ^= temp;

            temp = Encoding.ASCII.GetBytes(VIN)[13]; // continue with 14th VIN character

            key[2] += temp;
            key[2] ^= temp;
            key[1] += key[2];
            key[0] += key[1];
            key[0] ^= temp;

            temp = Encoding.ASCII.GetBytes(VIN)[8]; // finish with 9th VIN character (also known as security field)

            key[2] += temp;
            key[2] ^= temp;
            key[1] += key[2];
            key[0] += key[1];
            key[0] ^= temp;

            return key;
        }
    }
}
