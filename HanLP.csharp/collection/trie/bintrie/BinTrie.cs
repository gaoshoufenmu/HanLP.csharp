using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using HanLP.csharp.interfaces;
using HanLP.csharp.corpus.io;

namespace HanLP.csharp.collection.trie.bintrie
{
    public class BinTrie<V> : BaseNode<V>, ITrie<V>
    {
        private int _size;

        public BinTrie()
        {
            child = new BaseNode<V>[65536];
            status = Status.NOT_WORD;
        }

        /// <summary>
        /// 插入一个词
        /// </summary>
        /// <param name="key"></param>
        /// <param name="v"></param>
        public void Put(string key, V v)
        {
            if (string.IsNullOrEmpty(key)) return;

            BaseNode<V> branch = this;
            for(int i = 0; i < key.Length - 1; i++)
            {
                // 除最后一个字符外，都是“继续”
                branch.AddChild(new Node<V>(key[i], Status.NOT_WORD, default(V)));
                branch = branch.GetChild(key[i]);
            }

            if (branch.AddChild(new Node<V>(key[key.Length - 1], Status.WORD_END, v)))
                _size++;
        }

        public void Remove(string key)
        {
            BaseNode<V> branch = this;
            for(int i = 0; i < key.Length - 1; i++)
            {
                if (branch == null) return;
                branch = branch.GetChild(key[i]);
            }
            if (branch == null) return;

            if (branch.AddChild(new Node<V>(key[key.Length - 1], Status.UNDEFINED, v)))
                _size--;
        }

        public V Get(string key)
        {
            BaseNode<V> branch = this;
            for(int i = 0; i < key.Length; i++)
            {
                if (branch == null) throw new KeyNotFoundException();
                branch = branch.GetChild(key[i]);
            }
            if (branch == null) throw new KeyNotFoundException();
            if (branch.Status < Status.WORD_MIDDLE) throw new KeyNotFoundException();

            return branch.Value;
        }

        public bool ContainsKey(string key)
        {
            BaseNode<V> branch = this;
            for(int i = 0; i < key.Length; i++)
            {
                if (branch == null) return false;
                branch = branch.GetChild(key[i]);
            }
            return branch != null && branch.Status >= Status.WORD_MIDDLE;
        }


        public List<TrieEntry<V>> GetEntries()
        {
            var list = new List<TrieEntry<V>>();
            var sb = new StringBuilder();

            for(int i = 0; i < child.Length; i++)
            {
                var node = child[i];
                if (node == null) continue;

                node.Walk(new StringBuilder(sb.ToString()), list);
            }
            return list;
        }

        public string[] Keys
        {
            get
            {
                var list = GetEntries();
                var keys = new string[list.Count];
                for (int i = 0; i < list.Count; i++)
                    keys[i] = list[i].Key;

                return keys;
            }
        }

        public V[] Values
        {
            get
            {
                var list = GetEntries();
                var values = new V[list.Count];
                for (int i = 0; i < list.Count; i++)
                    values[i] = list[i].Value;

                return values;
            }
        }

        public int Size => _size;

        public V this[string key] => Get(key);


        // 以下两个PrefixMatch 方法是有区别的，分别简介如下：
        // 1. 给定前缀串 a1a2...ak, 查找字符串 x 满足： x = a1...ak...an，其中 n >= k
        // 2. 给定关键词 a1a2...ak, 查找字符串 x 满足： x = a1...ai，其中 1 <= i <= k

        /// <summary>
        /// 根据提供的前缀串进行（前缀）匹配
        /// 必须前缀包含前缀串
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public List<TrieEntry<V>> PrefixMatch(string prefix)
        {
            var list = new List<TrieEntry<V>>();
            var sb = new StringBuilder(prefix.Substring(0, prefix.Length - 1));
            BaseNode<V> branch = this;

            // 按照prefix路径向下匹配匹配到最后一个字符
            for(int i = 0; i < prefix.Length; i++)
            {
                if (branch == null) return list;
                branch = branch.GetChild(prefix[i]);
            }

            if (branch == null) return list;
            branch.Walk(sb, list);          // 从最后一个字符开始匹配下去，作为前缀匹配
            return list;
        }

        public List<TrieEntry<V>> PrefixMatch(char[] chars, int begin)
        {
            var list = new List<TrieEntry<V>>();
            var sb = new StringBuilder();
            BaseNode<V> branch = this;

            for (int i = begin; i < chars.Length; i++)
            {
                branch = branch.GetChild(chars[i]);
                if (branch == null || branch.Status == Status.UNDEFINED) return list;

                sb.Append(chars[i]);
                if (branch.Status >= Status.WORD_MIDDLE)
                    list.Add(new TrieEntry<V>(sb.ToString(), branch.Value));
            }
            return list;
        }

        /// <summary>
        /// 对给定关键词前缀匹配
        /// 关键词的前缀
        /// </summary>
        /// <param name="key"></param>
        /// <param name="begin"></param>
        /// <returns></returns>
        public List<TrieEntry<V>> PrefixMatch(string key, int begin) => PrefixMatch(key.ToCharArray(), begin);

        /// <summary>
        /// 添加子节点
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public override bool AddChild(BaseNode<V> node)
        {
            var add = false;
            var target = GetChild(node.C);
            if(target == null)      // 如果子节点不存在，则直接添加
            {
                child[node.C] = node;
                add = true;
            }
            else
            {
                switch(node.Status)
                {
                    case Status.UNDEFINED:      // 可以作为一种软删除方式
                        if(target.Status != Status.NOT_WORD)
                        {
                            target.Status = Status.NOT_WORD;
                            add = true;
                        }
                        break;
                    case Status.NOT_WORD:
                        if (target.Status == Status.WORD_END)
                            target.Status = Status.WORD_MIDDLE;
                        break;
                    case Status.WORD_END:
                        if (target.Status == Status.NOT_WORD)
                            target.Status = Status.WORD_MIDDLE;
                        add = target.Value == null;         // 原先节点若无值，则表示添加，否则只是修改，不算添加
                        target.Value = node.Value;      // 重置value值
                        break;
                }
            }
            return add;
        }

        /// <summary>
        /// 获取字符对应的子节点
        /// 首字符直接分配内存，以后字符则通过二分搜索合适的位置并插入，详情参考
        /// <seealso cref="Node{V}.GetChild(char)"/>
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public override BaseNode<V> GetChild(char c) => child[c];

        public bool Load(string path)
        {
            var bytes = File.ReadAllBytes(path);
            var ba = ByteArray.Create(path);

            for(int i = 0; i < child.Length; i++)
            {
                var flag = ba.NextInt();
                if(flag == 1)               // 子节点有值
                {
                    child[i] = new Node<V>();
                    child[i].Walk2Load(ba, ValueArray<V>.NullCreater);
                }
            }
            _size = -1; // 不知道有多少，因为有些节点 Status值为 Status.NOT_WORD，是来自软删除
            return true;
        }

        public bool Load(string path, ValueArray<V> va) => Load(ByteArray.Create(path), va, false);

        public bool Load(string path, V[] values, bool highFirst = false) => Load(ByteArray.Create(path), new ValueArray<V>(values), highFirst);

        public bool Load(ByteArray ba, ValueArray<V> va, bool compatibal = false)
        {
            for (int i = 0; i < child.Length; ++i)
            {
                if (ba.NextInt(compatibal) == 1)
                {
                    child[i] = new Node<V>();
                    child[i].Walk2Load(ba, va, compatibal);
                }
            }
            _size = va.values.Length;
            return true;
        }

        public bool Save(FileStream fs)
        {
            try
            {
                for(var i = 0; i < child.Length; i++)
                {
                    var node = child[i];
                    if(node == null)    // 子节点不存在，保存 0 作为标记
                        fs.Write(new byte[4], 0, 4);
                    else
                    {
                        fs.Write(BitConverter.GetBytes(1), 0, 4);   // 子节点存在，写入标记1
                        node.Walk2Save(fs);
                    }
                }
            }
            catch(Exception e)
            {
                return false;
            }
            return true;
        }

        public bool Save(string path)
        {
            var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            var res = Save(fs);
            fs.Close();
            return res;
        }

        public int Build(SortedDictionary<string, V> dict)
        {
            foreach (var p in dict)
                Put(p.Key, p.Value);
            return 0;
        }

        public V GetOrDefault(string key, V v = default(V))
        {
            BaseNode<V> branch = this;
            for (int i = 0; i < key.Length; i++)
            {
                if (branch == null) return v;
                branch = branch.GetChild(key[i]);
            }
            if (branch == null) return v;
            if (branch.Status < Status.WORD_MIDDLE) return v;

            return branch.Value;
        }

        public bool Load(ByteArray ba, V[] vs, bool highFirst = false) => Load(ba, new ValueArray<V>(vs), highFirst);

        void ITrie<V>.Save(FileStream fs) => Save(fs);
    }
}
