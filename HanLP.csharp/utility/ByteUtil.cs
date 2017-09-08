using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanLP.csharp.utility
{
    public class ByteUtil
    {
        public static char Bytes2Char(byte[] bs) => (char)((bs[0] << 8) | bs[1]);

        public static int Bytes2Int(byte[] bs, int start)
        {
            int num = bs[start + 3];
            num |= bs[start + 2] << 8;
            num |= bs[start + 1] << 16;
            num |= bs[start] << 24;
            return num;
        }

        /// <summary>
        /// 字节转双精度浮点数，高位字节在前
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public static double Byte2Double_HighFirst(byte[] bytes, int start)
        {
            long l = (long)(((ulong)bytes[start] << 56) & 0xFF00000000000000L);
            l |= (long)(((ulong)bytes[start + 1] << 48) & 0xFF000000000000L);
            l |= (long)(((ulong)bytes[start + 2] << 40) & 0xFF0000000000L);
            l |= (long)(((ulong)bytes[start + 3] << 32) & 0xFF00000000L);
            l |= (long)(((ulong)bytes[start + 4] << 24) & 0xFF000000L);
            l |= (long)(((ulong)bytes[start + 5] << 16) & 0xFF0000L);
            l |= (long)(((ulong)bytes[start + 6] << 8) & 0xFF00L);
            l |= (long)(((ulong)bytes[start + 7]) & 0xFFL);
            return BitConverter.Int64BitsToDouble(l);
        }

        public static int Byte2Int_HighFirst(byte[] bytes, int start)
        {
            int num = (int)(((uint)bytes[start] << 24) & 0xFF000000);
            //num |= (bytes[start + 1] << 16) & 0xFF0000;
            //num |= (bytes[start + 2] << 8) & 0xFF00;
            //num |= bytes[start + 3] & 0xFF;

            num |= bytes[start + 2] << 8;
            num |= bytes[start + 1] << 16;
            num |= bytes[start + 3];
            return num;
        }

        public static byte[] Int2Byte_HighFirst(int i)
        {
            var bs = new byte[4];
            bs[0] = (byte)(i >> 24);
            bs[1] = (byte)((i >> 16) & 0xFF);
            bs[2] = (byte)((i >> 8) & 0xFF);
            bs[3] = (byte)(i & 0xFF);
            return bs;
        }

        public static byte[] Char2Byte_HighFirst(char c)
        {
            var bs = new byte[2];
            bs[0] = (byte)(c >> 8);
            bs[1] = (byte)c;
            return bs;
        }

        public static byte[] Long2Byte_HighFirst(long l)
        {
            var bs = new byte[8];
            bs[0] = (byte)(l >> 56);
            bs[1] = (byte)(l >> 48);
            bs[2] = (byte)(l >> 40);
            bs[3] = (byte)(l >> 32);
            bs[4] = (byte)(l >> 24);
            bs[5] = (byte)(l >> 16);
            bs[6] = (byte)(l >> 8);
            bs[7] = (byte)l;
            return bs;
        }

        public static byte[] Double2Byte_HighFirst(double d) => Long2Byte_HighFirst(BitConverter.DoubleToInt64Bits(d));

        /// <summary>
        /// 将utf字符转为字节数组
        /// 高位字节在先
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static byte[] UTF2Byte_HighFirst(string str)
        {
            var len = str.Length;
            int utfLen = 0;
            int c = 0;
            int count = 0;

            for(int j = 0; j < len; j++)
            {
                c = str[j];
                if ((c >= 0x0001) && (c <= 0x007F))     // 单字节字符
                    utfLen++;
                else if (c > 0x07FF)                    // 三字节字符
                    utfLen += 3;
                else                                    // 双字节字符
                    utfLen += 2;
            }
            if (utfLen > 65535) throw new Exception("encoded string too long(bytes): " + utfLen);

            var bs = new byte[utfLen + 2];
            bs[count++] = (byte)((utfLen >> 8) & 0xFF);     // 写入长度，高位在前
            bs[count++] = (byte)utfLen;

            int i = 0; 
            for(;i < len; i++)                              //
            {
                c = str[i];
                if (!(c >= 0x0001 && c <= 0x007F)) break;       // 持续写入单字节字符，如遇到非单字节字符，则退出
                bs[count++] = (byte)c;
            }

            for(; i < len; i++)
            {
                c = str[i];
                if (c >= 0x0001 && c <= 0x007F) bs[count++] = (byte)c;      // 如果是单字节字符，则直接强转然后写入
                else if(c > 0x07FF)                     // 处理三字节字符
                {
                    bs[count++] = (byte)(0xE0 | ((c >> 12) & 0x0F));
                    bs[count++] = (byte)(0x80 | ((c >> 6) & 0x3F));
                    bs[count++] = (byte)(0x80 | (c & 0x3F));
                }
                else                                    // 处理双字节字符
                {
                    bs[count++] = (byte)(0xC0 | ((c >> 6) & 0x1F));
                    bs[count++] = (byte)(0x80 | (c & 0x3F));
                }
            }
            return bs;
        }

    }
}
