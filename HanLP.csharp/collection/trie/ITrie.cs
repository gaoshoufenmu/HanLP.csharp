using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using HanLP.csharp.corpus.io;

namespace HanLP.csharp.collection.trie
{
    public interface ITrie<V>
    {
        int Build(SortedDictionary<string, V> dict);
        V GetOrDefault(string key, V v = default(V));
        V this[string key] { get; }
        bool ContainsKey(string key);
        int Size { get; }
        bool Load(ByteArray ba, V[] vs, bool highFirst = false);

        V[] Values { get; }

        void Save(FileStream fs);
    }
}
