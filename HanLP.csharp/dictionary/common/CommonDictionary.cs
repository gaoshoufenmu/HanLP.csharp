using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using HanLP.csharp.collection;
using HanLP.csharp.collection.trie;

namespace HanLP.csharp.dictionary.common
{
    /// <summary>
    /// 通用词典
    /// </summary>
    /// <typeparam name="V"></typeparam>
    public abstract class CommonDictionary<V>
    {
        private DoubleArrayTrie<V> _trie;

        public bool Load(string path)
        {
            _trie = new DoubleArrayTrie<V>();
            var valueArr = OnLoadValue(path);
            if(valueArr == null)
            {
                // log info ""
                return false;
            }
            if(LoadDat(path + ".trie.dat", valueArr))
            {
                // log info ""
                return true;
            }

            var keys = new List<string>(valueArr.Length);

            try
            {
                foreach(var line in File.ReadLines(path))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var segs = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    keys.Add(segs[0]);
                }
            }
            catch(Exception e) { }

            var error = _trie.Build(keys, valueArr);
            if(error != 0)              // 出错
            {
                var map = new SortedDictionary<string, V>(StrComparer.Default);
                for(int i = 0; i < valueArr.Length; i++)
                {
                    map[keys[i]] = valueArr[i];
                }
                _trie = new DoubleArrayTrie<V>();
                _trie.Build(map);
                int j = 0;
                foreach (var v in map.Values)
                    valueArr[j++] = v;
            }

            var fs = new FileStream(path + ".trie.dat", FileMode.Create, FileAccess.Write);
            _trie.Save(fs);
            fs.Close();
            OnSaveValue(valueArr, path);
            return true;
        }
        private bool LoadDat(string path, V[] valueArr) => _trie.Load(path, valueArr);

        /// <summary>
        /// 排序词典
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool Sort(string path)
        {
            var map = new SortedDictionary<string, string>(StrComparer.Default);        // 通过这个排序字典进行排序
            try
            {
                foreach(var line in File.ReadLines(path))
                {
                    var segs = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    map[segs[0]] = line;
                }
                File.WriteAllLines(path, map.Select(p => p.Value).ToArray());
                return true;
            }
            catch(Exception e)
            {
                // log warning: "sorting dict file failed" + e.Message
                return false;
            }
        }
        /// <summary>
        /// 查询一个单词
        /// </summary>
        /// <param name="key"></param>
        /// <returns>单词对应条目</returns>
        public V Get(string key) => _trie.GetOrDefault(key);

        /// <summary>
        /// 是否包含键
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Contains(string key) => _trie.ContainsKey(key);

        /// <summary>
        /// 词条数量
        /// </summary>
        public int Size => _trie.Size;
        public abstract V[] OnLoadValue(string path);
        public abstract bool OnSaveValue(V[] valueArr, string path);
        public BaseSearcher<V> GetSearcher(string text) => new Searcher(text.ToCharArray(), _trie);

        public class Searcher : BaseSearcher<V>
        {
            private int _begin;

            private List<Tuple<string, V>> _entries;
            private DoubleArrayTrie<V> _trie;

            public Searcher(char[] cs, DoubleArrayTrie<V> trie): base(cs)
            {
                _trie = trie;
            }
            public Searcher(string text, DoubleArrayTrie<V> trie) : base(text.ToCharArray())
            {
                _entries = new List<Tuple<string, V>>();
                _trie = trie;
            }

            public override Tuple<string, V> Next()
            {
                while(_entries.Count == 0 && _begin < c.Length)
                {
                    _entries = _trie.PrefixMatchWithValue(c, _begin, c.Length, 0);
                    _begin++;
                }

                if(_entries.Count == 0 && _begin < c.Length)            // 总觉得这是多余的
                {
                    _entries = _trie.PrefixMatchWithValue(c, _begin, c.Length, 0);
                    _begin++;
                }
                if (_entries.Count == 0)
                    return null;

                var fst = _entries[0];
                _entries.RemoveAt(0);
                offset = _begin - 1;
                return fst;
            }
        }
    }
}
