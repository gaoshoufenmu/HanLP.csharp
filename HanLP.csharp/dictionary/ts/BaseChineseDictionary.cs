using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using HanLP.csharp.dictionary.other;
using HanLP.csharp.corpus.dictionary;
using HanLP.csharp.utility;
using HanLP.csharp.corpus.io;
using HanLP.csharp.collection;
using HanLP.csharp.collection.AhoCorasick;
using HanLP.csharp.collection.trie;
using HanLP.csharp.collection.trie.bintrie;

namespace HanLP.csharp.dictionary.ts
{
    public class BaseChineseDictionary
    {
        public static void CombineChain(SortedDictionary<string, string> s2t, SortedDictionary<string, string> t2x)
        {
            // s2t中每个KeyValuePair，根据Value去t2x查找新的Value并设置到s2t中
            foreach(var s in s2t.Keys.ToList())
            {
                var t = s2t[s];
                string x;
                if (t2x.TryGetValue(t, out x))
                {
                    s2t[s] = x;
                }
            }

            foreach(var p in t2x)
            {
                var s = CharTable.Convert(p.Key);
                if(s2t.ContainsKey(s))
                {
                    s2t.Add(s, p.Value);
                }
            }
        }

        public static void CombineReverseChain(SortedDictionary<string, string> t2s, SortedDictionary<string, string> tw2t, bool convert)
        {
            foreach(var p in tw2t)
            {
                if (t2s.TryGetValue(p.Value, out var s))
                {
                    if (s != null)
                    {
                        t2s.Add(p.Key, s);
                        continue;
                    }
                }
                t2s.Add(p.Key, convert ? CharTable.Convert(p.Value) : p.Value);
            }
        }

        /// <summary>
        /// 加载指定路径的词典文件，并保存到字典对象中
        /// 词典文件是字符串文件
        /// </summary>
        /// <param name="dict">保存的字典对象</param>
        /// <param name="reverse">是否需要反转</param>
        /// <param name="paths">词典文件路径</param>
        /// <returns></returns>
        public static bool Load(SortedDictionary<string, string> dict, bool reverse, params string[] paths)
        {
            var sd = new StringDictionary();

            foreach(var path in paths)
            {
                if (!sd.Load(path))
                    return false;
            }
            if (reverse)
                sd = sd.Reverse();

            foreach (var entry in sd.GetEntries())
                dict.Add(entry.Key, entry.Value);


            //var arr = new int[dict.Count];
            //int i = 0;
            //foreach(var p in dict)
            //{
            //    arr[i++] = p.Key.First();
            //}

            return true;
        }

        /// <summary>
        /// 加载指定路径的词典文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="trie"></param>
        /// <param name="reverse"></param>
        /// <returns></returns>
        public static bool Load(string path, ACDoubleArrayTrie<string> trie, bool reverse)
        {
            string datPath = path;
            if (reverse)
                datPath += Predefine.REVERSE_EXT;

            if (LoadDat(datPath, trie)) return true;        // 先尝试加载二进制数据文件

            var dict = new SortedDictionary<string, string>(StrComparer.Default);
            if (!Load(dict, reverse, path)) return false;   // 加载字符串文件到指定字典对象中

            trie.Build(dict);                               // 根据字典对象建议trie
            SaveDat(datPath, trie, dict);
            return true;
        }

        /// <summary>
        /// 加载二进制数据文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="trie"></param>
        /// <returns></returns>
        public static bool LoadDat(string path, ACDoubleArrayTrie<string> trie)
        {
            var ba = ByteArray.Create(path + Predefine.BIN_EXT);
            if (ba == null) return false;

            var size = ba.NextInt();
            var strs = new string[size];
            for (int i = 0; i < size; i++)
                strs[i] = ba.NextString();

            trie.Load(ba, strs);
            return true;
        }

        /// <summary>
        /// 保存二进制数据文件
        /// 先保存Values，然后保存Keys
        /// </summary>
        /// <param name="path"></param>
        /// <param name="trie"></param>
        /// <param name="entries"></param>
        /// <returns></returns>
        public static bool SaveDat(string path, ACDoubleArrayTrie<string> trie, SortedDictionary<string, string> entries)
        {
            if(trie.Size != entries.Count)
            {
                // log warning "key value pair is unmatched"
                return false;
            }

            var fs = new FileStream(path + Predefine.BIN_EXT, FileMode.Create, FileAccess.Write);
            try
            {
                var bytes = BitConverter.GetBytes(entries.Count);
                fs.Write(bytes, 0, 4);

                foreach(var entry in entries)
                {
                    bytes = BitConverter.GetBytes(entry.Value.Length);
                    fs.Write(bytes, 0, 4);
                    foreach(var c in entry.Value)
                    {
                        bytes = BitConverter.GetBytes(c);
                        fs.Write(bytes, 0, 2);
                    }
                }
                return trie.Save(fs);
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

        public static string Seg4Longest(char[] chars, DoubleArrayTrie<string> trie)
        {
            var sb = new StringBuilder(chars.Length);
            var searcher = new Searcher(chars, trie);

            int pos = 0;
            int offset;
            Tuple<string, string> t;
            while((t = searcher.Next()) != null)
            {
                offset = searcher.Offset;
                // 补足没有查到的词
                if(pos < offset)
                    sb.Append(chars, pos, offset - pos);

                sb.Append(t.Item2);
                pos = offset + t.Item2.Length;
            }
            if (pos < chars.Length)
                sb.Append(chars, pos, chars.Length - pos);

            return sb.ToString();
        }

        public static string Seg4Longest(char[] chars, ACDoubleArrayTrie<string> trie)
        {
            var wordNet = new string[chars.Length];
            var lengthNet = new int[chars.Length];      // 最长字符串匹配，所以从某个起始点开始的字符串，需要记录其长度，用于长度比较

            Action<int, int, string> action = (begin, end, value) =>
            {
                var len = end - begin;
                if(len > lengthNet[begin])
                {
                    wordNet[begin] = value;
                    lengthNet[begin] = len;
                }
            };

            trie.Match(chars, action);

            var sb = new StringBuilder(chars.Length);
            for(int i = 0; i < wordNet.Length; )
            {
                if(wordNet[i] == null)
                {
                    sb.Append(chars[i]);
                    i++;
                    continue;
                }
                sb.Append(wordNet[i]);
                i += lengthNet[i];
            }
            return sb.ToString();
        }

        public class Searcher : BaseSearcher<string>
        {
            int _begin;

            DoubleArrayTrie<string> _trie;

            public Searcher(string text, DoubleArrayTrie<string> trie) : base(text)
            {
                _trie = trie;
            }

            public Searcher(char[] chars, DoubleArrayTrie<string> trie) : base(chars)
            {
                _trie = trie;
            }

            /// <summary>
            /// 获取下一个（最长）匹配词语
            /// </summary>
            /// <returns></returns>
            public override Tuple<string, string> Next()
            {
                while(_begin < c.Length)
                {
                    var list = _trie.PrefixMatchWithValue(c, _begin, c.Length, 0);
                    if (list.Count == 0)
                        ++_begin;
                    else
                    {
                        var t = list.Last();        // 选取最长匹配
                        offset = _begin;
                        _begin += t.Item1.Length;
                        return t;
                    }
                }
                return null;
            }
        }
    }
}
