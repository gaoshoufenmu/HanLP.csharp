using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HanLP.csharp.Constants;

namespace HanLP.csharp.utility
{
    public class TextUtil
    {
        // 全角字符  GBK编码   Unicode编码
        //   ；      A3BB        FF1B                ＝       A3BD        FF1D
        //   ！      A3A1        FF01                ＠       A3C0        FF20
        //   【      A1BE        3010                ［       A3DB        FF3B 
        //    】     A1BF        3011                ］       A3DD        FF3D
        //    ，     A3AC        FF0C                ＾       A3DE        FF3E
        //    。     A1A3        3002                ＿       A3DF        FF3F
        //    “      A1B0       201C                ｀       A3E0        FF40
        //    ”      A1B1       201D                 ．       A3AE       FF0E
        //    ？     A3BF        FF1F                ∶       A1C3        2236   (3∶2的比，或表示时分秒的分隔符，与英文的:不同)
        //    （     A3A8        FF08                ·       A1A4        B7      （比如专利号中的点号）
        //     ）    A3A9        FF09
        //    ：     A3BA        FF1A
        //    、     A1A2        3001
        //  ／       A3AF        FF0F
        //  ＼       A3DC        FF3C
        //  ＜       A3BC        FF1C
        //  ＞       A3BE        FF1E
        //

        //      序号        GBK范围        Unicode范围
        //   ⒈ ~ ⒛           [A2B1, A2C4] [2488, 249B]
        //   ⑴ ~ ⒇          [A2C5, A2D8]   [2474, 2487]
        //     ① ~ ⑩     [A2D9, A2E2]    [2460,2469]
        //  ㈠ ~ ㈩       [A2E5, A2EE]    [3220, 3229]
        //  Ⅰ ~  Ⅹ ~ Ⅻ     A2F1, A2FC        2160, 216B
        //

        //      全角字符                    GBK编码        Unicode编码
        //  ０１２３４５６７８９           A3B0, A3B9        FF10, FF19
        // ＡＢＣＤＥＦＧＨＩＪＫＬＭＮＯＰＱＲＳＴＵＶＷＸＹＺ   [A3C1, A3DA]        [FF21, FF3A]  
        // ａｂｃｄｅｆｇｈｉｊｋｌｍｎｏｐｑｒｓｔｕｖｗｘｙｚ   [A3E1, A3FA]        [FF41, FF5A]
        //
        // (　) 全角空格，( )半角空格

        /// <summary>
        /// 获取指定字符的类型
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static int CharType(char c) => CharType(c.ToString());

        /// <summary>
        /// 获取字符类型
        /// </summary>
        /// <param name="str">单字符构成的字符串</param>
        /// <returns></returns>
        public static int CharType(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return CT_OTHER;    // 各种类型的空白符号，类型均为 “其他”

            if (ALL_CHINESE_NUMs.Contains(str)) return CT_NUM;      // 全角阿拉伯数字和汉字数，类型为 “数字”
            byte[] bytes;
            try
            {
                bytes = Encoding.GetEncoding("GBK").GetBytes(str);  // 将字符串转为 “GBK”编码
            }
            catch
            {
                bytes = Encoding.Unicode.GetBytes(str);             // 转为 unicode 编码
            }

            var b1 = bytes[0];
            byte b2 = bytes.Length > 1 ? bytes[1] : (byte)0;
            if(b1 < 128)                                            // ascii，单字节字符
            {
                if (' ' == b1) return CT_OTHER;                     // 空白符号，类型为 “其他”
                if ("*\"!,.?()[]{ }+=/\\;:|\n".Contains((char)b1)) return CT_DELIMITER;     // “分隔符” 类型
                if ("0123456789".Contains((char)b1)) return CT_NUM;     // 半角阿拉伯“数字”类型
                return CT_SINGLE;                                   // 所有剩余的单字节字符归类为 “单字节” 类型
            }
            if (b1 == 162) return CT_INDEX;             // 序号
            if (b1 == 163 && b2 > 175 && b2 < 186) return CT_NUM;       // 全角数字字符
            if (b1 == 163 && (b2 >= 193 && b2 <= 218 || b2 >= 225 && b2 <= 250)) return CT_LETTER;  // 全角英文字母大小写
            if (b1 == 161 || b1 == 163) return CT_DELIMITER;    // 全角分隔符  A1, A3
            if (b1 >= 176 && b1 <= 247) return CT_CHINESE;      // 中文字符     GBK: [B040, F7FE] , [F840, F8A0], 
                                                                //             Unicode: [7645, 9F44]+ [~] ,   [9CE3, 9D42] => [0E00, 9FA5]
            return CT_OTHER;
        }

        /// <summary>
        /// 是否全部是中文字符（不包含全角字符）
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsAllChinese(string input) => input.All(c => c >= '\u4e00' && c <= '\u9fa5');
        /// <summary>
        /// 是否全部是单字节字符
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsAllSingleByte(string input) => input.All(c => c <= 128);

        /// <summary>
        /// 将字符串转为整型
        /// </summary>
        /// <param name="input"></param>
        /// <param name="default"></param>
        /// <returns></returns>
        public static int Str2Int(string input, int @default = -1)
        {
            if(int.TryParse(input, out int res))
            {
                return res;
            }
            return @default;
        }

        /// <summary>
        /// 是否全部是数字字符
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsAllNum(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;

            int i = 0;
            if ("±+—-＋".Contains(input[0]))     // 如果包含符号，则起始位置移动一位
                i++;

            while (i < input.Length && "０１２３４５６７８９".Contains(input[i]))     // 向右扫描，如果是全角数字，则位置移动一位
                i++;

            if(i < input.Length)    // i 位置遇到第一个非全角数字字符
            {
                if("∶·．／".Contains(input[i]) || input[i] == '.' || input[i] == '/')
                {
                    i++;
                    while (i < input.Length && "０１２３４５６７８９".Contains(input[i]))     // 原java代码中为 i + 1 < input.Length，不解为什么要 + 1？
                        i++;
                }
            }
            if (i >= input.Length)
                return true;

            while (i < input.Length && "0123456789".Contains(input[i]))
                i++;

            if (i < input.Length)
            {
                if("∶·．／".Contains(input[i]) || input[i] == '.' || input[i] == '/')
                {
                    i++;
                    while (i < input.Length && "0123456789".Contains(input[i]))
                        i++;
                }
            }
            if(i < input.Length)            // 这个 if 语句是多余的，如果 i < input.Length，那么最终返回 false, 所以是否进入这个 if 语句并执行 i-- 并不会改变返回结果
            {
                if (!"百千万亿佰仟％‰".Contains(input[i]) && input[i] != '%')
                    i--;
            }
            if (i >= input.Length)
                return true;
            return false;
        }

        public static bool IsAllIndex(string input) => IsAllIndex(Encoding.GetEncoding("GBK").GetBytes(input));
        /// <summary>
        /// 是否全是序号字符
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static bool IsAllIndex(byte[] bytes)
        {
            int i = 0;
            while(i < bytes.Length - 1 && bytes[i] == 162)  // A2
            {
                i += 2;
            }
            if (i >= bytes.Length) return true;

            while (i < bytes.Length && (bytes[i] >= 'A' && bytes[i] <= 'Z' || bytes[i] >= 'a' && bytes[i] <= 'z'))  // 英文字母也算一种序号字符
                i++;

            return i >= bytes.Length;
        }
        /// <summary>
        /// 是否全部为英文字母字符
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsAllLetter(string input) => input.All(c => c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z');
        /// <summary>
        /// 是否全部为英文字母或数字字符
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsAllLetterOrNum(string input) =>
            input.All(c => c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z' || c >= '0' && c <= '9');

        public static bool IsAllDelimiter(string input) => IsAllDelimiter(Encoding.GetEncoding("GBK").GetBytes(input));
        /// <summary>
        /// 是否全部为分隔符
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static bool IsAllDelimiter(byte[] bytes)
        {
            int i = 0;
            while (i < bytes.Length - 1 && (bytes[i] == 161 || bytes[i] == 163))
                i += 2;

            return i >= bytes.Length;
        }

        /// <summary>
        /// 是否全部是中文数字
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsAllChineseNum(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;

            for(int i = 0; i < input.Length; i++)
            {
                if(i < input.Length - 1 && input[i] == '分' && input[i+1] == '之')    // e.g. 百分之六
                {
                    i += 1;
                    continue;
                }

                if(!ALL_CHINESE_NUMs.Contains(input[i]))        // 如果不是中文数字字符
                {
                    if (NUM_DELIMITER.Contains(input[i]))    // 如果是数字分隔符
                    {
                        // 分隔符前后必须是中文数字，否则返回false
                        if (i == 0 && (!ALL_CHINESE_NUMs.Contains(input[i + 1])) || i == input.Length - 1 && (!ALL_CHINESE_NUMs.Contains(input[i - 1]))) return false;
                    }
                    else if (i != 0 || !CHINEST_NUM_PREFIX.Contains(input[i]))
                        return false;
                }
            }
            return true;
        }
        /// <summary>
        /// 获取字符集上的字符在给定字符串中出现次数
        /// </summary>
        /// <param name="charSet"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public static int GetCharCount(string charSet, string input)
        {
            if (string.IsNullOrEmpty(input)) return 0;

            int count = 0;
            for(int i = 0; i < input.Length; i++)
            {
                if (charSet.Contains(input[i]))
                    count++;
            }
            return count;
        }

        /// <summary>
        /// 是否是年份（不含月日），注意input字符串之后的字符必须为'年'才有意义
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsYear(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;

            input = input.Trim();
            //if (input.StartsWith("公元"))                   // 这里的input 是经过分词后的单词，所以不需要考虑“公元前”，“公元”等前缀
            //{
            //    if (input.Length > 2 && input[2] == '前')
            //        input = input.Substring(3);
            //    else
            //        input = input.Substring(2);

            //    if (input.Length == 0) return false;
            //}

            if (input.Length <= 4 && input.All(c => ALL_NUMs.Contains(c)))
                return true;

            if (input.Length == 2 && TIANGAN.Contains(input[0]) && DIZHI.Contains(input[1]))
                return true;
            return false;
        }

        /// <summary>
        /// 判断给定输入字符串的字符是全部在字符集中
        /// </summary>
        /// <param name="charSet"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsAllContainedIn(string charSet, string input)
        {
            if (string.IsNullOrEmpty(input)) return false;

            return input.All(c => charSet.Contains(c));
        }

        /// <summary>
        /// 是否全部为半角字符
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsDBCCase(string input)
        {
            if (string.IsNullOrEmpty(input)) return false;
            var chars = input.ToCharArray();
            for(int i = 0; i < chars.Length; i++)
            {
                if (Encoding.GetEncoding("GBK").GetByteCount(chars, i, 1) != 1) return false;
            }
            return true;
        }

        /// <summary>
        /// 是否全部为全角字符
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsSBCCase(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;
            var chars = input.ToCharArray();
            for(int i = 0; i < chars.Length; i++)
            {
                if (Encoding.GetEncoding("GBK").GetByteCount(chars, i, 1) != 2) return false;
            }
            return true;
        }

        /// <summary>
        /// 是否为连字符
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsConnectChar(char c) => c == '-' || c == '—';
        public static bool IsUnderlineChar(char c) => c == '＿' || c == '_';
        /// <summary>
        /// 是否为未知类型字符串
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsUnknown(string input) => input != null && input.Contains("未##");

        public static char[] Long2Chars(long x)
        {
            var chars = new char[4];
            chars[0] = (char)(x >> 48);
            chars[1] = (char)(x >> 32);
            chars[2] = (char)(x >> 16);
            chars[3] = (char)x;
            return chars;
        }

        public static string Long2Str(long x) => new string(Long2Chars(x));

        public static bool IsChineseChar(char c) => c >= '\u4e00' && c <= '\u9fa5';

        /// <summary>
        /// 统计子串在原字符串中出现的次数
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="substr"></param>
        /// <returns></returns>
        public static int GetCount(string origin, string substr)
        {
            var list = new List<string>();
            if (string.IsNullOrEmpty(substr)) return 0;

            var count = 0;
            var j = 0;
            for(int i = 0; i < origin.Length - substr.Length + 1; i++)
            {
                if(substr[j] == origin[i])
                {
                    j++;
                    if(j == substr.Length)
                    {
                        count++;
                        j = 0;
                    }
                }
                else
                {
                    i -= j;
                    j = 0;
                }
            }
            return count;
        }

        public static string Join2Str(IEnumerable<char> chars)
        {
            var sb = new StringBuilder();
            foreach (var c in chars)
                sb.Append(c);
            return sb.ToString();
        }

        public static string Join2Str(string delimiter, IEnumerable<string> strs)
        {
            var sb = new StringBuilder(delimiter.Length + 10);
            foreach(var s in strs)
            {
                if (sb.Length == 0)
                    sb.Append(s);
                else
                    sb.Append(delimiter).Append(s);
            }
            return sb.ToString();
        }
    }
}
