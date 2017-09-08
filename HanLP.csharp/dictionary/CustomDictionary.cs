using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using HanLP.csharp.collection.trie;
using HanLP.csharp.corpus.io;
using HanLP.csharp.corpus.tag;
using HanLP.csharp.dictionary.other;
using HanLP.csharp.utility;
using HanLP.csharp.collection;
using HanLP.csharp.collection.trie.bintrie;
using WordAttr = HanLP.csharp.corpus.model.Attribute;
using HanLP.csharp.collection.AhoCorasick;

namespace HanLP.csharp.dictionary
{
    public class CustomDictionary
    {
        /// <summary>
        /// 动态增加新的词条
        /// </summary>
        public static BinTrie<WordAttr> binTrie;
        /// <summary>
        /// 从文件中加载词典
        /// </summary>
        public static DoubleArrayTrie<WordAttr> dat = new DoubleArrayTrie<WordAttr>();

        static CustomDictionary()
        {
            Load();
        }
        private static bool Load()
        {
            if (LoadDat(Config.Custom_Dict_Path[0])) return true;

            dat = new DoubleArrayTrie<WordAttr>();

            var dict = new SortedDictionary<string, WordAttr>(StrComparer.Default);

            try
            {
                for(var i = 0; i < Config.Custom_Dict_Path.Length; i++)
                {
                    var p = Config.Custom_Dict_Path[i];         // 当前自定义词典文件路径
                    var defNat = Nature.n;
                    int spaceIdx = p.IndexOf(' ');
                    if(spaceIdx > 0)
                    {
                        // 有默认词性
                        var nat = p.Substring(spaceIdx + 1);    // 空格之后为词性
                        p = p.Substring(0, spaceIdx);           // 
                        defNat = NatureHelper.GetOrCreate(nat);
                    }
                    Load(p, defNat, dict);
                    //bool success = 
                    //if(!success)
                        // log warning "loading file failed: " + p 
                }
                if(dict.Count == 0)
                {
                    // log warning "no items loaded"
                    dict[Constants.TAG_OTHER] = null;   // 当作空白占位符
                }

                dat.Build(dict);

                SaveDat(Config.Custom_Dict_Path[0], dict);
                return true;
            }
            catch(Exception e)
            {
                return false;
            }
        }

        private static bool LoadDat(string path)
        {
            try
            {
                var ba = ByteArray.Create(path + Predefine.BIN_EXT);
                if (ba == null) return false;

                int size = ba.NextInt();
                if(size < 0)    // 一种兼容措施，当Size小于零表示文件头存储了-Size个用户词性
                {
                    while(size < 0)
                    {
                        var customNat = ba.NextString();
                        NatureHelper.GetOrCreate(customNat);    // register user-defined nature
                        size++;
                    }
                    size = ba.NextInt();
                }
                var attrs = new WordAttr[size];

                for(int i = 0; i < size; i++)       // 加载values
                {
                    var totalFreq = ba.NextInt();
                    var len = ba.NextInt();
                    attrs[i] = new WordAttr(len);
                    attrs[i].totalFreq = totalFreq;
                    for(int j = 0; j < len; j++)
                    {
                        attrs[i].natures[j] = (Nature)ba.NextInt();
                        attrs[i].freqs[j] = ba.NextInt();
                    }
                }
                return dat.Load(ba, attrs);     // 加载keys
            }
            catch(Exception e)
            {
                return false;
            }
        }
        private static bool SaveDat(string path, SortedDictionary<string, WordAttr> dict)
        {
            var fs = new FileStream(path + Predefine.BIN_EXT, FileMode.Create, FileAccess.Write);
            byte[] bytes;
            try
            {
                if (NatureHelper.CustomNatures.Count > 0)        // save custom natures
                {
                    bytes = BitConverter.GetBytes(-NatureHelper.CustomNatures.Count);
                    fs.Write(bytes, 0, 4);
                    foreach (var p in NatureHelper.CustomNatures)
                    {
                        var nat = p.Key;
                        bytes = BitConverter.GetBytes(nat.Length);
                        fs.Write(bytes, 0, 4);
                        foreach (var c in nat)
                        {
                            bytes = BitConverter.GetBytes(c);
                            fs.Write(bytes, 0, 2);
                        }
                    }
                }

                bytes = BitConverter.GetBytes(dict.Count);      // save attributes
                fs.Write(bytes, 0, 4);
                foreach (var p in dict)
                {
                    p.Value.Save(fs);
                }

                dat.Save(fs);       // save keys
                return true;
            }
            catch(Exception e)
            {
                // log
                return false;
            }
            finally
            {
                fs.Close();
            }
        }

        private static bool Load(string path, Nature defNat, SortedDictionary<string, WordAttr> dict)
        {
            try
            {
                var splitter = new[] { ' ', '\t'};
                if (path.EndsWith(".csv"))
                    splitter = new[] { ',' };

                foreach(var line in File.ReadLines(path))
                {
                    var segs = line.Split(splitter);
                    if (segs.Length == 0)
                        continue;

                    if (Config.NormalizeChar)
                        segs[0] = CharTable.Convert(segs[0]);

                    var natCount = (segs.Length - 1) / 2;
                    WordAttr attr;
                    if (natCount == 0)
                        attr = new WordAttr(defNat);
                    else
                    {
                        attr = new WordAttr(natCount);
                        for(int i = 0; i < natCount; i++)
                        {
                            attr.natures[i] = NatureHelper.GetOrCreate(segs[1 + (i << 1)]);
                            attr.freqs[i] = int.Parse(segs[(i + 1) << 1]);
                            attr.totalFreq += attr.freqs[i];
                        }
                    }
                    dict[segs[0]] = attr;
                }
                return true;
            }
            catch(Exception e)
            {
                return false;
            }
        }

        /// <summary>
        /// 如果词条存在，更新词条的属性
        /// </summary>
        /// <param name="key">词条</param>
        /// <param name="attr">词条属性</param>
        /// <param name="dict">加载期间的词条字典</param>
        /// <param name="rewriteDict">核心词典被更新的记录字典</param>
        /// <returns>更新是否成功</returns>
        private static bool UpdateAttrIfExist(string key, WordAttr attr, SortedDictionary<string, WordAttr> dict, SortedDictionary<int, WordAttr> rewriteDict)
        {
            var wordID = CoreDictionary.GetWordId(key);
            WordAttr attrExisted;

            if(wordID != -1)
            {
                attrExisted = CoreDictionary.GetAttr(wordID);
                attrExisted.natures = attr.natures;
                attrExisted.freqs = attr.freqs;
                attrExisted.totalFreq = attr.totalFreq;
                rewriteDict[wordID] = attr;
                return true;
            }

            if(dict.TryGetValue(key, out attrExisted))
            {
                attrExisted.natures = attr.natures;
                attrExisted.freqs = attr.freqs;
                attrExisted.totalFreq = attr.totalFreq;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 往自定义词典中插入一个新词（覆盖模式）
        /// 动态增删不会持久化到词典文件
        /// </summary>
        /// <param name="word">新词，如“裸婚”</param>
        /// <param name="natWithFreq">词性以及词频，默认为"nz 1"</param>
        /// <returns></returns>
        public static bool Insert(string word, string natWithFreq = null)
        {
            if (string.IsNullOrEmpty(word)) return false;
            if (Config.NormalizeChar) word = CharTable.Convert(word);

            var attr = string.IsNullOrEmpty(natWithFreq)
                ? new WordAttr(Nature.nz, 1)
                : WordAttr.Create(natWithFreq);
            if (attr == null) return false;

            if (dat.Set(word, attr)) return true;
            if (binTrie == null) binTrie = new BinTrie<WordAttr>();
            binTrie.Put(word, attr);
            return true;
        }

        /// <summary>
        /// 非覆盖模式，往自定义词典中插入一个新词
        /// 动态增删不会持久化到词典文件
        /// </summary>
        /// <param name="word">新词，如“裸婚”</param>
        /// <param name="natWithFreq">词性以及词频，默认为"nz 1"</param>
        /// <returns></returns>
        public static bool Add(string word, string natWithFreq = null)
        {
            if (Config.NormalizeChar) word = CharTable.Convert(word);

            if (ContainsKey(word)) return false;
            return Insert(word, natWithFreq);
        }

        /// <summary>
        /// 是否包含指定词条
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool ContainsKey(string key)
        {
            if (dat.ContainsKey(key)) return true;

            return binTrie != null && binTrie.ContainsKey(key);
        }
        /// <summary>
        /// 获取指定词条
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static WordAttr GetAttr(string key)
        {
            if (Config.NormalizeChar) key = CharTable.Convert(key);

            var attr = dat.GetOrDefault(key);
            if(attr == null && binTrie != null)
                attr = binTrie.GetOrDefault(key);

            return attr;
        }
        /// <summary>
        /// 删除指定词条
        /// 动态增删不会持久化到词典文件
        /// </summary>
        /// <param name="key"></param>
        public static void Remove(string key)
        {
            if (Config.NormalizeChar) key = CharTable.Convert(key);
            if(binTrie != null)
            {
                binTrie.Remove(key);
            }
        }

        /// <summary>
        /// 前缀查询
        /// 为什么只在binTrie中查询？
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public static List<TrieEntry<WordAttr>> PrefixMatch(string prefix)
        {
            if (binTrie == null) return null;
            return binTrie.PrefixMatch(prefix);
        }

        /// <summary>
        /// 解析文本，获取词条的属性
        /// </summary>
        /// <param name="chars"></param>
        /// <param name="action">begin(inclusive), end(exclusive)</param>
        public static void Parse(char[] chars, Action<int, int, WordAttr> action)
        {
            // 先尝试用动态添加的词典来匹配一遍
            if(binTrie != null)
            {
                var searcher = GetSearcher(chars);
                int offset;
                Tuple<string, WordAttr> tuple;
                while((tuple = searcher.Next()) != null)
                {
                    offset = searcher.Offset;
                    action(offset, offset + tuple.Item1.Length, tuple.Item2);
                }
            }

            // 用文件加载的词典再次匹配一遍，需要注意的是，这两次匹配到的结果顺序自然是错乱的，所以需要在action中将其安装起始位置排序
            var searcher2 = dat.GetSearcher(chars, 0);
            while(searcher2.Next())
            {
                action(searcher2.begin, searcher2.begin + searcher2.length, searcher2.value);
            }
        }
        /// <summary>
        /// 解析文本，获取词条的属性
        /// </summary>
        /// <param name="text"></param>
        /// <param name="action"></param>
        public static void Parse(string text, Action<int, int, WordAttr> action) => Parse(text.ToCharArray(), action);

        public static BaseSearcher<WordAttr> GetSearcher(string text) => new Searcher(text);

        public static BaseSearcher<WordAttr> GetSearcher(char[] chars) => new Searcher(chars);

        public override string ToString() => $"DAT({dat.Size})";

        public class Searcher : BaseSearcher<WordAttr>
        {
            private int _begin;
            private List<TrieEntry<WordAttr>> _tuples;

            public Searcher(char[] cs) : base(cs)
            {
                _tuples = new List<TrieEntry<WordAttr>>();
            }

            public Searcher(string input) : this(input.ToCharArray()) { }

            public override Tuple<string, WordAttr> Next()
            {
                while(_tuples.Count == 0 && _begin < c.Length)
                {
                    _tuples = binTrie.PrefixMatch(c, _begin);
                    ++_begin;
                }

                //
                if(_tuples.Count == 0 && _begin < c.Length) // 不会进入这个if语句块中吧？
                {
                    _tuples = binTrie.PrefixMatch(c, _begin);
                    ++_begin;
                }

                if (_tuples.Count == 0)
                    return null;

                var fst = _tuples[0];
                _tuples.RemoveAt(0);
                offset = _begin - 1;
                return new Tuple<string, WordAttr>(fst.Key, fst.Value);
            }
        }

    }
}
