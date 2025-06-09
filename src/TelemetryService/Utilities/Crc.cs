using System;
using System.Globalization;
using System.IO;

namespace Utilities
{
    public class Crc
    {
        public static ushort CalculateCrc(string message, string sharedSecret)
        {
            // Assemble a byte stream, without the characters that are excluded from the calculation
            MemoryStream ms = new MemoryStream();
            foreach (char c in message)
            {
                switch (c)
                {
                    case '\n':
                    case '\r':
                    case ',':
                    case '%':
                        // Not included in calculation
                         break;
                    default:
                        // World's simplest ASCII conversion, as we know we're just using ASCII and therefore only 0-127 are in use
                        ms.WriteByte((byte)c);
                        break;
                }
            }
            // Append the shared secret
            foreach (char c in sharedSecret)
            {
                ms.WriteByte((byte)c);
            }

            // At this point, ms includes all the bytes we need to consider.
            Crc16Ccitt crcCalculator = new Crc16Ccitt(0xffff);
            return crcCalculator.ComputeChecksum(ms.ToArray());
        }

        public static string CalculatePrintableCrc(string message, string sharedSecret)
        {
            return CalculateCrc(message, sharedSecret).ToString("X4", CultureInfo.InvariantCulture).ToUpperInvariant();
        }


        public static bool VerifyCrc(string messageWithCrc, string sharedSecret)
        {
            // Strip the CRC off the message: it's always after the last comma.
            int lastComma = messageWithCrc.LastIndexOf(",", StringComparison.InvariantCulture);
            if (lastComma < 0)
                return false;
            string messageWithoutCrc = messageWithCrc.Substring(0, lastComma);
            string crcInHex = messageWithCrc.Substring(lastComma + 1);
            int statedCrc;
            if (!int.TryParse(crcInHex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out statedCrc))
                return false;
            ushort computedCrc = CalculateCrc(messageWithoutCrc, sharedSecret);
            return statedCrc == computedCrc;
        }

        // A canonical version.
        // Check a message against a crc - supplied separately so we don't rely upon the formatting of the string.
        // Takes raw message, appends sharedSecret, says whether crc of that is as claimed.
        public static bool VerifyCrcCanonical(string message, string suppliedCrc, string sharedSecret)
        {
            int statedCrc;
            if (!int.TryParse(suppliedCrc, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out statedCrc))
                return false;
            ushort computedCrc = CalculateCrc(message, sharedSecret);
            return statedCrc == computedCrc;
        }


    }

    public class Crc16Ccitt
    {
        const ushort poly = 4129;
        readonly ushort[] table = new ushort[256];
        readonly ushort initialValue;

        public ushort ComputeChecksum(byte[] bytes)
        {
            ushort crc = initialValue;
            for (int i = 0; i < bytes.Length; ++i)
            {
                crc = (ushort)((crc << 8) ^ table[(crc >> 8) ^ (0xff & bytes[i])]);
            }
            return crc;
        }

        public byte[] ComputeChecksumBytes(byte[] bytes)
        {
            ushort crc = ComputeChecksum(bytes);
            return BitConverter.GetBytes(crc);
        }

        public Crc16Ccitt(ushort seed)
        {
            initialValue = seed;
            for (int i = 0; i < table.Length; ++i)
            {
                ushort temp = 0;
                ushort a = (ushort)(i << 8);
                for (int j = 0; j < 8; ++j)
                {
                    if (((temp ^ a) & 0x8000) != 0)
                    {
                        temp = (ushort)((temp << 1) ^ poly);
                    }
                    else
                    {
                        temp <<= 1;
                    }
                    a <<= 1;
                }
                table[i] = temp;
            }
        }
    }
}
