using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanLP.csharp.utility
{
    public class CharUtil
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsSpaceLetter(char c) =>
            c == 8 || c == 9 || c == 10 || c == 13 || c == 32 || c == 160;

        public static bool IsEnglishChar(char c) =>
            (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
    }
}
