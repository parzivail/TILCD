using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPiTiLcd
{
    /// <summary>
    /// From http://www.codeproject.com/Tips/1080310/Csharp-Binary-Literal-Helper-Class
    /// </summary>
    public static class BinaryLiteral
    {
        public static byte BinaryLiteralToByte(this string str)
        {
            return ToByte(str);
        }

        public static short BinaryLiteralToInt16(this string str)
        {
            return ToInt16(str);
        }

        public static int BinaryLiteralToInt32(this string str)
        {
            return ToInt32(str);
        }

        public static long BinaryLiteralToInt64(this string str)
        {
            return ToInt64(str);
        }

        public static byte ToByte(string str)
        {
            return (byte)ToInt64(str, sizeof(byte));
        }

        public static short ToInt16(string str)
        {
            return (short)ToInt64(str, sizeof(short));
        }

        public static int ToInt32(string str)
        {
            return (int)ToInt64(str, sizeof(int));
        }

        public static long ToInt64(string str)
        {
            return ToInt64(str, sizeof(long));
        }

        private static long ToInt64(string str, int sizeInBytes)
        {
            int sizeInBits = sizeInBytes * 8;
            int bitIndex = 0;
            long result = 0;

            for (int i = str.Length - 1; i >= 0; i--)
            {
                char c = str[i];

                if (c != ' ')
                {
                    if (bitIndex == sizeInBits)
                    {
                        throw new OverflowException("binary literal too long: " + str);
                    }

                    if (c == '1')
                    {
                        result |= 1L << bitIndex;
                    }
                    else if (c != '0')
                    {
                        throw new InvalidCastException(String.Format("invalid character '{0}' in binary literal: {1}", c, str));
                    }

                    bitIndex++;
                }
            }

            return result;
        }
    }
}
