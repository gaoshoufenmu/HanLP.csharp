using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using HanLP.csharp.interfaces;
using HanLP.csharp.collection.trie.bintrie;

namespace HanLP.csharp.corpus.dictionary
{
    public abstract class SimpleDictionary<V>
    {
        protected BinTrie<V> trie = new BinTrie<V>();

        public BinTrie<V> Trie => trie;

        public bool Load(string path)
        {
            try
            {
                var lines = File.ReadLines(path);
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var entry = OnGenerateEntry(line);
                    if (entry == null) continue;
                    trie.Put(entry.Key, entry.Value);
                }
                return true;
            }
            catch(Exception e)
            {
                return false;
            }
        }

        public abstract TrieEntry<V> OnGenerateEntry(string line);   // 根据一行字符串生成 TrieEntry

        public V GetOrDefault(string key, V v) => trie.GetOrDefault(key, v);

        /// <summary>
        /// 使用其他字典来补充，本质是对 key做 合并运算
        /// </summary>
        /// <param name="other"></param>
        public void UnionWith(SimpleDictionary<V> other)
        {
            if (other == null) return;

            foreach(var p in other.GetEntries())
            {
                if (trie.ContainsKey(p.Key)) continue;
                trie.Put(p.Key, p.Value);
            }
        }

        public List<TrieEntry<V>> GetEntries() => trie.GetEntries();

        public string[] Keys => trie.Keys;
        public V[] Values => trie.Values;

        public int Remove(Predicate<TrieEntry<V>> predicate)
        {
            int size = trie.Size;

            foreach(var p in GetEntries())
            {
                if (predicate(p))
                    trie.Remove(p.Key);
            }
            return size - trie.Size;
        }

        public void Add(string key, V v) => trie.Put(key, v);
        public int Size => trie.Size;
    }
}
