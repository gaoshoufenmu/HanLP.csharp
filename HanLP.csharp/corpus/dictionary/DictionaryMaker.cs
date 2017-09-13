using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using HanLP.csharp.collection.trie.bintrie;
using HanLP.csharp.corpus.document;
using HanLP.csharp.collection;
using HanLP.csharp.utility;

namespace HanLP.csharp.corpus.dictionary
{
    public class DictionaryMaker
    {
        private BinTrie<Item> _trie;
        public DictionaryMaker()
        {
            _trie = new BinTrie<Item>();
        }

        /// <summary>
        /// 向词典中添加一个词(语)
        /// 如果词已经存在，则添加一个标签<seealso cref="IWord.Label"/>
        /// </summary>
        /// <param name="word"></param>
        public void Add(IWord word)
        {
            var item = _trie.GetOrDefault(word.Value);
            if (item == null)
            {
                item = new Item(word.Value, word.Label);
                _trie.Put(item.key, item);
            }
            else
                item.AddLabel(word.Label);
        }

        public void Add(string value, string label) => Add(new Word(value, label));
        public void Add(string param)
        {
            var item = Item.Create(param);
            if (item != null)
                Add(item);
        }

        public void Add(Item item)
        {
            var oldItem = _trie.GetOrDefault(item.key);
            if (oldItem == null)
                _trie.Put(item.key, item);
            else
                oldItem.Combine(item);
        }

        public void AddWithNoCombine(Item item)
        {
            var oldItem = _trie.GetOrDefault(item.key);
            if (oldItem == null)
                _trie.Put(item.key, item);
        }

        /// <summary>
        /// 从词典中移除一个词条
        /// </summary>
        /// <param name="value"></param>
        public void Remove(string value) => _trie.Remove(value);

        public Item Get(string key) => _trie.Get(key);
        public Item GetOrDefault(string key) => _trie.GetOrDefault(key);

        public Item GetOrDefault(IWord word) => _trie.GetOrDefault(word.Value);

        public SortedSet<string> LabelSet
        {
            get
            {
                var ss = new SortedSet<string>(StrComparer.Default);
                foreach(var v in _trie.Values)
                {
                    foreach(var p in v.labelMap)
                    {
                        ss.Add(p.Key);
                    }
                }
                return ss;
            }
        }

        public string[] Keys => _trie.Keys;
        public Item[] Values => _trie.Values;

        public void AddAll(List<Item> items)
        {
            foreach (var item in items)
                Add(item);
        }
        public void AddAllWithNoCombine(List<Item> items)
        {
            foreach (var item in items)
                AddWithNoCombine(item);
        }

        public bool SaveTxtTo(string path)
        {
            if (_trie.Size == 0) return true;
            try
            {
                var entries = _trie.GetEntries();
                var lines = new List<string>();
                foreach(var e in _trie.GetEntries())
                {
                    lines.Add(e.Value.ToString());
                }
                File.WriteAllLines(path, lines.ToArray());
            }
            catch(Exception e)
            {
                return false;
            }
            return true;
        }

        public override string ToString() => $"items count: {_trie.Size}";

        /// <summary>
        /// 读取文件中所有条目
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static List<Item> LoadAsItems(string path)
        {
            var items = new List<Item>();
            try
            {
                foreach(var line in File.ReadLines(path))
                {
                    if (string.IsNullOrEmpty(line)) break;

                    var item = Item.Create(line);
                    if (item == null) return null;              // error

                    items.Add(item);
                }
            }
            catch(Exception e)
            {
                //
            }
            return items;
        }

        public static DictionaryMaker Load(string path)
        {
            var dm = new DictionaryMaker();
            dm.AddAll(LoadAsItems(path));
            return dm;
        }

        public static DictionaryMaker CombineTwo(string pathA, string pathB)
        {
            var dm = new DictionaryMaker();
            dm.AddAll(LoadAsItems(pathA));
            dm.AddAll(LoadAsItems(pathB));
            return dm;
        }
        public static DictionaryMaker CombineMany(params string[] paths)
        {
            var dm = new DictionaryMaker();
            foreach (var p in paths)
                dm.AddAll(LoadAsItems(p));
            return dm;
        }

        public static DictionaryMaker CombineWithNormalization(string[] paths)
        {
            var dm = new DictionaryMaker();
            dm.AddAll(LoadAsItems(paths[0]));
            for(int i = 1; i < paths.Length; i++)
            {
                dm.AddAllWithNoCombine(LoadAsItems(paths[i]));
            }
            return dm;
        }

        public static DictionaryMaker CombineWhenNotInclude(string[] paths)
        {
            var dm = new DictionaryMaker();
            dm.AddAll(LoadAsItems(paths[0]));
            for(int i = 1; i< paths.Length;i++)
            {
                dm.AddAllWithNoCombine(NormalizationFrequency(LoadAsItems(paths[i])));
            }
            return dm;
        }

        public static List<Item> NormalizationFrequency(List<Item> items)
        {
            foreach(var item in items)
            {
                var array = item.LabelMapEntries;
                ArrayHelper.Sort(array, (x, y) => x.Item2 <= y.Item2);      // 按label频率从高到低排序

                for (int i = 0; i < array.Length; i++)                      // 从新给各label的频率赋值
                    item.labelMap[array[i].Item1] = i + 1;
            }
            return items;
        }
    }
}
