using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using HanLP.csharp.corpus.dictionary;
using HanLP.csharp.collection;

namespace HanLP.csharp.dictionary.py
{
    /// <summary>
    /// 声母韵母音调词典
    /// </summary>
    public class SYTDictionary
    {
        /// <summary>
        /// sheng mu, 声母
        /// </summary>
        private static SortedSet<string> _smSet = new SortedSet<string>(StrComparer.Default);
        /// <summary>
        /// yun mu, 韵母
        /// </summary>
        private static SortedSet<string> _ymSet = new SortedSet<string>(StrComparer.Default);
        /// <summary>
        /// sound tone, 音调
        /// </summary>
        private static SortedSet<string> _stSet = new SortedSet<string>(StrComparer.Default);

        private static SortedDictionary<string, string[]> _map = new SortedDictionary<string, string[]>(StrComparer.Default);
        static SYTDictionary()
        {
            var sd = new StringDictionary();
            if(sd.Load(Config.SYT_Dict_Path))
            {
                foreach(var p in sd.GetEntries())
                {
                    var segs = p.Value.Split(',');
                    if (segs[0].Length == 0) segs[0] = "none";
                    if (!string.IsNullOrWhiteSpace(segs[0]))
                        _smSet.Add(segs[0]);
                    if (!string.IsNullOrWhiteSpace(segs[1]))
                        _ymSet.Add(segs[1]);
                    if (!string.IsNullOrWhiteSpace(segs[2]))
                        _stSet.Add(segs[2]);

                    var va = new string[4];
                    Array.Copy(segs, va, 3);
                    va[3] = PinyinUtil.Pinyin_ToneNum2ToneMark(p.Key);
                    _map.Add(p.Key, va);
                }
            }
        }

        /// <summary>
        /// Dump 到文件
        /// </summary>
        /// <param name="path"></param>
        public static void Dump(string dir)
        {
            Dump(_smSet, dir + "sm.txt");
            Dump(_smSet, dir + "ym.txt");
            Dump(_smSet, dir + "st.txt");

            var hdSet = new SortedSet<string>(StrComparer.Default);
            for (int i = 0; i < Pinyin.PinyinTable.Length; i++)
                hdSet.Add(Pinyin.PinyinTable[i].Head.ToString());
            Dump(hdSet, dir + "head.txt");

            var sb = new StringBuilder(_map.Count * 10);
            foreach(var p in _map)
            {
                var vals = p.Value;
                var py = Pinyin.PinyinTable[(int)Enum.Parse(typeof(PYName), p.Key)];
                sb.Append($"{p.Key}(Shengmu.{vals[0]}, Yunmu.{vals[1]}, {vals[2]}, \"{vals[3]}\", \"{p.Key.Substring(0, p.Key.Length - 1)}\", Head.{py.Head}, '{py.FirstChar}')\n");
            }
            File.WriteAllText(dir + "py.txt", sb.ToString());
        }

        private static void Dump(ISet<string> set, string path)
        {
            var sb = new StringBuilder(set.Count * 3);
            foreach(var s in set)
            {
                if (sb.Length == 0)
                    sb.Append(s);
                else
                    sb.Append(",\n").Append(s);
            }
            File.WriteAllText(path, sb.ToString());
        }
    }
}
