using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace HanLP.csharp.corpus.io
{
    public class IOUtil
    {
        public static string ReadTxt(string path)
        {
            if (!File.Exists(path)) return null;

            var bytes = File.ReadAllBytes(path);
            if (bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
                return Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
