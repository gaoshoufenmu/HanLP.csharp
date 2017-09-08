using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using HanLP.csharp.collection;
using HanLP.csharp.collection.trie;
using HanLP.csharp.corpus.tag;
using HanLP.csharp.corpus.io;
using HanLP.csharp.utility;
using static HanLP.csharp.Constants;
using WordAttr = HanLP.csharp.corpus.model.Attribute;

namespace HanLP.csharp.dictionary
{
    public class CoreDictionary
    {
        public static DoubleArrayTrie<WordAttr> _trie = new DoubleArrayTrie<WordAttr>();
        //public static readonly string Path = Config.Core_Dict_Path;
        public const int TotalFrequency = 221894;

        /// <summary>
        /// 静态构造函数：加载词典
        /// </summary>
        static CoreDictionary()
        {
            Load();
        }

        #region Load & Save
        private static bool Load()
        {
            if (LoadDat(Config.Core_Dict_Path)) return true;

            var dict = new SortedDictionary<string, WordAttr>(StrComparer.Default);
            try
            {
                int max_freq = 0;
                foreach (var line in File.ReadLines(Config.Core_Dict_Path))
                {
                    var segs = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);  // Regex.Split(line, @"\s");
                    var natCount = (segs.Length - 1) / 2;
                    var attr = new WordAttr(natCount);
                    for(int i = 0; i < natCount; i++)
                    {
                        attr.natures[i] = (Nature)Enum.Parse(typeof(Nature), segs[1 + (i << 1)]);
                        attr.freqs[i] = int.Parse(segs[(i + 1) << 1]);
                        attr.totalFreq += attr.freqs[i];
                    }
                    dict[segs[0]] = attr;
                    max_freq += attr.totalFreq;
                }
                _trie.Build(dict);

                SaveDat(Config.Core_Dict_Path, dict);
                return true;
            }
            catch(FileNotFoundException e)
            {
                // log warning "core dictionary file does not exist"
                return false;
            }
            catch (IOException e)
            {
                // log warning "core dictionary file read error"
                return false;
            }
        }

        private static bool LoadDat(string path)
        {
            try
            {
                var ba = ByteArray.Create(path + Predefine.BIN_EXT);
                if (ba == null) return false;

                // 先读取values
                var size = ba.NextInt();
                var attrs = new WordAttr[size];

                for(int i = 0; i < size; i++)
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

                return _trie.Load(ba, attrs) && !ba.HasMore();
            }
            catch(Exception e)
            {
                // log warning "dat file reading failed"
                return false;
            }
        }

        /// <summary>
        /// 保存二进制数据到文件
        /// 先保存values，然后保存keys
        /// </summary>
        /// <param name="path"></param>
        /// <param name="dict"></param>
        /// <returns></returns>
        private static bool SaveDat(string path, SortedDictionary<string, WordAttr> dict)
        {
            var fs = new FileStream(path + Predefine.BIN_EXT, FileMode.Create, FileAccess.Write);
            try
            {
                var bytes = BitConverter.GetBytes(dict.Count);
                fs.Write(bytes, 0, 4);

                // -------------- save values ---------------
                foreach(var p in dict)
                {
                    var attr = p.Value;
                    bytes = BitConverter.GetBytes(attr.totalFreq);
                    fs.Write(bytes, 0, 4);
                    bytes = BitConverter.GetBytes(attr.natures.Length);
                    for(int i = 0; i < attr.natures.Length; i++)
                    {
                        bytes = BitConverter.GetBytes((int)attr.natures[i]);
                        fs.Write(bytes, 0, 4);
                        bytes = BitConverter.GetBytes(attr.freqs[i]);
                        fs.Write(bytes, 0, 4);
                    }
                    
                }
                // ------------- value saving finish ------------------

                // -------------- save keys --------------------------
                return _trie.Save(fs);
            }
            catch(Exception e)
            {
                // log warning "save failed"
                return false;
            }
            finally
            {
                fs.Close();
            }
        }
        #endregion

        public static WordAttr GetAttr(string key) => _trie.GetOrDefault(key);

        public static WordAttr GetAttr(int wordId) => _trie.GetByIndex(wordId);

        /// <summary>
        /// 获取单词在核心词典中的频次
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public static int GetFreq(string word)
        {
            var attr = GetAttr(word);
            if (attr == null) return 0;
            return attr.totalFreq;
        }

        public static int GetTotalFreq(string term)
        {
            var attr = _trie.GetOrDefault(term);
            if (attr == null) return 0;
            return attr.totalFreq;
        }

        public static bool Contains(string key) => _trie.ContainsKey(key);

        public static int GetWordId(string key) => _trie.ExactMatch(key);

        //! --------------------------  一些特殊的WORD_ID ------------------------
        private static int _begin_word_id = -1;
        /// <summary>
        /// 开始的ID
        /// </summary>
        public static int BEGIN_WORD_ID
        {
            get
            {
                if (_begin_word_id < 0)
                    _begin_word_id = GetWordId(TAG_BEGIN);
                return _begin_word_id;
            }
        }
        private static int _end_word_id = -1;
        /// <summary>
        /// 结束的ID
        /// </summary>
        public static int END_WORD_ID
        {
            get
            {
                if (_end_word_id < 0)
                    _end_word_id = GetWordId(TAG_END);
                return _end_word_id;
            }
        }
        private static int _nr_word_id = -1;
        /// <summary>
        /// 未知人名词的ID
        /// </summary>
        public static int NR_WORD_ID
        {
            get
            {
                if(_nr_word_id < 0)
                    _nr_word_id = GetWordId(TAG_PEOPLE);
                return _nr_word_id;
            }
        }
        private static int _ns_word_id = -1;
        /// <summary>
        /// 未知地名词的ID
        /// </summary>
        public static int NS_WORD_ID
        {
            get
            {
                if(_ns_word_id < 0)
                    _ns_word_id = GetWordId(TAG_PLACE);
                return _ns_word_id;
            }
        }
        private static int _nt_word_id = -1;
        /// <summary>
        /// 未知团体词的ID
        /// </summary>
        public static int NT_WORD_ID
        {
            get
            {
                if(_nt_word_id < 0)
                    _nt_word_id = GetWordId(TAG_GROUP);
                return _nt_word_id;
            }
        }
        private static int _t_word_id = -1;
        /// <summary>
        /// 未知时间词的ID
        /// </summary>
        public static int T_WORD_ID
        {
            get
            {
                if(_t_word_id < 0)
                    _t_word_id = GetWordId(TAG_TIME);
                return _t_word_id;
            }
        }
        private static int _x_word_id = -1;
        /// <summary>
        /// 未知字符串词的ID
        /// </summary>
        public static int X_WORD_ID
        {
            get
            {
                if(_x_word_id < 0)
                    _x_word_id = GetWordId(TAG_CLUSTER);
                return _x_word_id;
            }
        }
        private static int _m_word_id;
        /// <summary>
        /// 未知数词的ID
        /// </summary>
        public static int M_WORD_ID
        {
            get
            {
                if(_m_word_id < 0)
                    _m_word_id = GetWordId(TAG_NUMBER);
                return _m_word_id;
            }
        }
        private static int _nx_word_id = -1;
        /// <summary>
        /// 未知专有名词的ID
        /// </summary>
        public static int NX_WORD_ID
        {
            get
            {
                if(_nx_word_id < 0)
                    _nx_word_id = GetWordId(TAG_PROPER);
                return _nx_word_id;
            }
        }

    }
}
