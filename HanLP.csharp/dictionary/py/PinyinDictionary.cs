using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using HanLP.csharp.collection.trie;
using HanLP.csharp.collection.AhoCorasick;
using HanLP.csharp.corpus.io;
using HanLP.csharp.corpus.dictionary;
using HanLP.csharp.utility;
using HanLP.csharp.collection;

namespace HanLP.csharp.dictionary.py
{
    public class PinyinDictionary
    {
        public static ACDoubleArrayTrie<Pinyin[]> _trie = new ACDoubleArrayTrie<Pinyin[]>();

        static PinyinDictionary()
        {
            if (!Load(Config.Pinyin_Dict_Path))
                throw new ArgumentException("Pinyin dict file loading error");
        }

        private static bool Load(string path)
        {
            if (LoadDat(path)) return true;     // 尝试加载二进制数据文件

            var sd = new StringDictionary();
            if (!sd.Load(path)) return false;   // 加载字符串文件到字典对象中

            // 将数据放入排序字典中，以便建立Trie
            var dict = new SortedDictionary<string, Pinyin[]>(StrComparer.Default);
            foreach(var p in sd.GetEntries())
            {
                var segs = p.Value.Split(',');      // 词语的每个字的拼音使用逗号分隔
                var pys = new Pinyin[segs.Length];
                for(int i = 0; i < pys.Length; i++)
                {
                    var index = (int)Enum.Parse(typeof(PYName), segs[i]);
                    pys[i] = Pinyin.PinyinTable[index];
                }

                dict.Add(p.Key, pys);           // 添加中文词条与其对应的拼音
            }
            _trie.Build(dict);
            SaveDat(path, _trie, dict);
            return true;
        }

        private static bool LoadDat(string path)
        {
            var ba = ByteArray.Create(path + Predefine.BIN_EXT);
            if (ba == null) return false;

            var size = ba.NextInt();
            var va = new Pinyin[size][];

            for(int i = 0; i < size; i++)
            {
                int len = ba.NextInt();
                va[i] = new Pinyin[len];
                for (int j = 0; j < len; j++)
                    va[i][j] = Pinyin.PinyinTable[ba.NextInt()];
            }

            return _trie.Load(ba, va);
        }

        private static bool SaveDat(string path, ACDoubleArrayTrie<Pinyin[]> trie, SortedDictionary<string, Pinyin[]> dict)
        {
            var fs = new FileStream(path + Predefine.BIN_EXT, FileMode.Create, FileAccess.Write);
            try
            {
                var bytes = BitConverter.GetBytes(dict.Count);
                fs.Write(bytes, 0, 4);
                foreach(var p in dict)
                {
                    var values = p.Value;
                    bytes = BitConverter.GetBytes(values.Length);
                    fs.Write(bytes, 0, 4);
                    for (int j = 0; j < values.Length; j++)
                    {
                        var py = values[j];
                        fs.Write(BitConverter.GetBytes(py.Index), 0, 4);
                    }
                }

                return trie.Save(fs);
            }
            catch(Exception e)
            {
                // log warning "save data file error"
                return false;
            }
            finally { fs.Close(); }
        }


        /// <summary>
        /// 获取给定词语的拼音
        /// 可能是多音词，所以返回是拼音数组
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static Pinyin[] Get(string key) => _trie.GetOrDefault(key);

        /// <summary>
        /// 将中文句子翻译成拼音
        /// </summary>
        /// <param name="sentence"></param>
        /// <param name="remainNone"></param>
        /// <returns></returns>
        public static List<Pinyin> Translate2Pinyin(string sentence, bool remainNone = true) => Seg4Longest(sentence.ToCharArray(), _trie, remainNone);



        /// <summary>
        /// 最长词语搜索匹配拼音
        /// </summary>
        /// <param name="chars"></param>
        /// <param name="trie"></param>
        /// <param name="remainNone">对于没有匹配到拼音的子串，是否使用pinyin.none 代替</param>
        /// <returns></returns>
        private static List<Pinyin> Seg4Longest(char[] chars, ACDoubleArrayTrie<Pinyin[]> trie, bool remainNone)
        {
            var wordNet = new Pinyin[chars.Length][];   // 第一维表示以每个字符下标开始的子串的拼音，第二维表示子串中各字符（拼音）的个数
            Action<int, int, Pinyin[]> action = (begin, end, value) =>
            {
                var len = end - begin;          // 本次匹配到的子串长度
                if (wordNet[begin] == null || len > wordNet[begin].Length)     // 如果当前下标位置没有匹配到子串，或者当前匹配子串长度大于上一次该位置的匹配子串长度，则重置该下标位置的匹配子串拼音
                    wordNet[begin] = len == 1 ? new[] { value[0] } : value;
            };

            trie.Match(chars, action);

            var list = new List<Pinyin>();
            for(int i = 0; i < wordNet.Length; )
            {
                if(wordNet[i] == null)          // chars 中 i 位置开始的子串没有匹配到拼音
                {
                    if (remainNone)             // 是否使用 none 代替没有匹配到的子串
                        list.Add(Pinyin.PinyinTable[(int)PYName.none5]);

                    i++;
                    continue;
                }

                for (var j = 0; j < wordNet[i].Length; j++)     // 从位置i处开始匹配到子串拼音
                    list.Add(wordNet[i][j]);                    // 依次将各字符的拼音加入列表

                i += wordNet[i].Length;
            }
            return list;
        }

        /// <summary>
        /// 提供待转换成拼音的文本以及拼音Trie结构，依次返回文本中各词语的拼音
        /// 使用<seealso cref="Next"/> 方法
        /// </summary>
        public class Searcher : BaseSearcher<Pinyin[]>
        {
            private int _begin;

            private DoubleArrayTrie<Pinyin[]> _trie;

            public Searcher(string text, DoubleArrayTrie<Pinyin[]> trie) : base(text)
            {
                _trie = trie;
            }
            public Searcher(char[] chars, DoubleArrayTrie<Pinyin[]> trie) : base(chars)
            {
                _trie = trie;
            }

            public override Tuple<string, Pinyin[]> Next()
            {
                while(_begin < c.Length)
                {
                    var list = _trie.PrefixMatchWithValue(c, _begin, c.Length, 0);
                    if (list.Count == 0)
                        ++_begin;
                    else
                    {
                        var last = list[list.Count - 1];
                        offset = _begin;
                        _begin += last.Item1.Length;
                        return last;
                    }
                }
                return null;
            }
        }
    }
}
