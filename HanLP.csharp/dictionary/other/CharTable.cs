using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using HanLP.csharp.utility;
using HanLP.csharp.corpus.io;

namespace HanLP.csharp.dictionary.other
{
    /// <summary>
    /// 正规化字符：全角->半角，大写->小写，繁体->简体
    /// </summary>
    public class CharTable
    {
        /// <summary>
        /// 正规化字符所使用的表
        /// </summary>
        public static char[] CONVERT;

        static CharTable()
        {
            if(!Load())
            {
                // log error
                throw new Exception("Loading char normalization table error");
            }
        }

        private static bool Load()
        {
            var datPath = Config.Char_Table_Path + Predefine.BIN_EXT;
            if (LoadDat(datPath)) return true;

            CONVERT = new char[char.MaxValue + 1];
            for (int i = 0; i < CONVERT.Length; i++)
                CONVERT[i] = (char)i;

            var lines = File.ReadAllLines(Config.Char_Table_Path);
            for(int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (line.Length != 3) continue;
                CONVERT[line[0]] = CONVERT[line[2]];
            }

            return SaveDat(datPath);
        }

        private static bool LoadDat(string path)
        {
            var ba = ByteArray.Create(path);
            if (ba == null) return false;

            CONVERT = new char[char.MaxValue + 1];

            int i = 0;
            while(ba.HasMore())
            {
                CONVERT[i++] = ba.NextChar();
            }
            return true;
        }

        private static bool SaveDat(string path)
        {
            var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            for(int i = 0; i < CONVERT.Length; i++)
            {
                //var bytes = BitConverter.GetBytes((char)i);
                //fs.Write(bytes, 0, 2);
                var bytes = BitConverter.GetBytes(CONVERT[i]);
                fs.Write(bytes, 0, 2);
            }
            fs.Close();
            return true;
        }

        public static char Convert(char c) => CONVERT[c];

        public static char[] Convert(char[] chars)
        {
            var res = new char[chars.Length];
            for (int i = 0; i < chars.Length; i++)
                res[i] = CONVERT[chars[i]];

            return res;
        }

        public static string Convert(string str)
        {
            if (str == null) return null;
            var res = new char[str.Length];
            for (int i = 0; i < str.Length; i++)
                res[i] = CONVERT[str[i]];

            return new string(res);
        }

        public static void Normallize(char[] chars)
        {
            for (int i = 0; i < chars.Length; i++)
                chars[i] = CONVERT[chars[i]];
        }
    }
}
