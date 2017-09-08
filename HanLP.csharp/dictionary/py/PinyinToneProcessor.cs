using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HanLP.csharp.collection;
using HanLP.csharp.algorithm.ahocorasick;

namespace HanLP.csharp.dictionary.py
{
    /// <summary>
    /// 带音标的拼音处理器
    /// 将待处理字符串转换为拼音对象
    /// </summary>
    public class PinyinToneProcessor
    {
        /// <summary>
        /// 带音调的字母到Pinyin的map
        /// </summary>
        private static SortedDictionary<string, Pinyin> _mapKey = new SortedDictionary<string, Pinyin>(StrComparer.Default);
        /// <summary>
        /// 带数字音调的字母到Pinyin的map
        /// </summary>
        private static SortedDictionary<string, Pinyin> _mapNumberKey = new SortedDictionary<string, Pinyin>(StrComparer.Default);

        private static Trie _trie;

        static PinyinToneProcessor()
        {
            foreach(var py in Pinyin.PinyinTable)
            {
                _mapNumberKey.Add(py.ToString(), py);
                _mapKey[py.PinyinWithTone] = py;
                _mapKey[py.Pinyin_] = String2Pinyin.Convert2Tone5(py);
            }
            _trie = new Trie() { RemainLongest = true };
            _trie.AddKeywords(_mapKey.Keys);
        }

        public static bool IsValid(string pinyin) => _mapNumberKey.ContainsKey(pinyin);

        public static bool IsAllValid(string[] pinyins) => pinyins.All(py => _mapNumberKey.ContainsKey(py));

        public static Pinyin ConvertFromToneNum(string pinyin)
        {
            if (_mapNumberKey.TryGetValue(pinyin, out var py))
                return py;

            return Pinyin.None;
        }
        public static List<Pinyin> ConvertFromToneNum(string[] pinyins)
        {
            var list = new List<Pinyin>();
            for(int i = 0; i < pinyins.Length; i++)
            {
                var py = pinyins[i];
                list.Add(Convert(py));
            }
            return list;
        }

        public static Pinyin Convert(string pinyin)
        {
            if (_mapKey.TryGetValue(pinyin, out var py))
                return py;

            return Pinyin.None;
        }

        public static List<Pinyin> Convert(string tonePinyin, bool removeNone)
        {
            var list = new List<Pinyin>();
            var tokens = _trie.Tokenize(tonePinyin);
            foreach(var t in tokens)
            {
                if (_mapKey.TryGetValue(t.Fragment, out var py))
                {
                    list.Add(py);
                }
                else if (!removeNone)
                    list.Add(Pinyin.None);
            }
            return list;
        }

        public static List<Pinyin> Convert(string[] pinyins)
        {
            var list = new List<Pinyin>();
            for (int i = 0; i < pinyins.Length; i++)
                list.Add(Convert(pinyins[i]));

            return list;
        }
    }
}
