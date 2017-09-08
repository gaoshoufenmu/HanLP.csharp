using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using HanLP.csharp.collection.trie;
using HanLP.csharp.utility;
using HanLP.csharp.collection;

namespace HanLP.csharp.dictionary.nr
{
    /// <summary>
    /// 翻译的人名词典
    /// </summary>
    public class TranslatedPersonDictionary
    {
        public static DoubleArrayTrie<bool> _trie;

        static TranslatedPersonDictionary()
        {
            if(!Load())
            {
                // log load error
            }
        }

        private static bool Load()
        {
            _trie = new DoubleArrayTrie<bool>();
            if (LoadDat()) return true;

            // 从原始字符串编码文件读取词典数据
            try
            {
                var map = new SortedDictionary<string, bool>(StrComparer.Default);      // 翻译人名，存在，则value为true
                var charFreqMap = new SortedDictionary<char, int>();                    // 统计翻译人名中的各字符的频次
                foreach(var line in File.ReadLines(Config.Translated_Person_Dict_Path))
                {
                    map[line] = true;
                    foreach(var c in line)
                    {
                        if ("不赞".IndexOf(c) >= 0) continue;     // 排除一些不常用的字

                        if (charFreqMap.TryGetValue(c, out int f))
                            charFreqMap[c] = f + 1;
                        else
                            charFreqMap[c] = 1;
                    }
                }

                map["·"] = true;

                foreach(var p in charFreqMap)
                {
                    if (p.Value < 10) continue;             // 如果单字符频次小于10，则忽略

                    map[p.Key.ToString()] = true;           // 否则视为一个名称的简称，认为是一个有效名
                }

                _trie.Build(map);
                return true;
            }
            catch(Exception e)
            {
                return false;
            }
        }

        private static bool LoadDat() => _trie.Load(Config.Translated_Person_Dict_Path + Predefine.BIN_EXT);

        private static bool SaveDat(SortedDictionary<string, bool> map)
        {
            var fs = new FileStream(Config.Translated_Person_Dict_Path + Predefine.BIN_EXT, FileMode.Create, FileAccess.Write);
            try
            {
                return _trie.Save(fs);
            }
            catch (Exception e)
            {
                return false;
            }
            finally
            {
                fs.Close();
            }
        }

        public static bool ContainsKey(string key) => _trie.ContainsKey(key);

        public static bool ContainsKey(string key, int length)
        {
            if (!_trie.ContainsKey(key)) return false;

            return key.Length >= length;
        }
    }
}
