using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using HanLP.csharp.interfaces;
using HanLP.csharp.corpus.io;
using HanLP.csharp.utility;

namespace HanLP.csharp.collection.trie.bintrie
{
    public abstract class BaseNode<V> : IComparable<BaseNode<V>>
    {
        /// <summary>
        /// 所有状态
        /// </summary>
        public static readonly Status[] ALL_STATUS = new[] { Status.UNDEFINED, Status.NOT_WORD, Status.WORD_MIDDLE, Status.WORD_END };
        /// <summary>
        /// 子节点数组
        /// </summary>
        protected BaseNode<V>[] child;
        internal BaseNode<V>[] Child
        {
            get => child;
            set => child = value;
        }

        /// <summary>
        /// 节点状态
        /// </summary>
        protected Status status;
        public Status Status { get => status; set => status = value; }
        /// <summary>
        /// 节点字符
        /// </summary>
        protected char c;
        public char C { get => c; }
        /// <summary>
        /// 节点关联值
        /// </summary>
        protected V v;
        public V Value { get => v; set => v = value; }

        /// <summary>
        /// 根据指定路径进行状态节点的转移
        /// </summary>
        /// <param name="path"></param>
        /// <param name="begin">路径起始点</param>
        /// <returns></returns>
        public BaseNode<V> Transition(string path, int begin)
        {
            var cur = this;
            for(int i = begin; i < path.Length; i++)
            {
                cur = cur.GetChild(path[i]);
                if (cur == null || cur.status == Status.UNDEFINED) return null;
            }
            return cur;
        }
        /// <summary>
        /// 获取子节点
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public abstract BaseNode<V> GetChild(char c);
        /// <summary>
        /// 是否存在指定子节点
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public bool HasChild(char c) => GetChild(c) != null;
        /// <summary>
        /// 添加子节点
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public abstract bool AddChild(BaseNode<V> node);

        public int CompareTo(BaseNode<V> other)
        {
            if (this.c > other.c) return 1;
            if (this.c < other.c) return -1;
            return 0;
        }

        public void Walk(StringBuilder sb, List<TrieEntry<V>> list)
        {
            sb.Append(c);
            if (status == Status.WORD_MIDDLE || status == Status.WORD_END)
                list.Add(new TrieEntry<V>(sb.ToString(), Value));

            if (child == null) return;

            foreach(var ch in child)
            {
                if (ch == null) continue;
                ch.Walk(new StringBuilder(sb.ToString()), list);
            }
        }

        public void Walk2Save(FileStream fs)
        {
            fs.Write(BitConverter.GetBytes(c), 0, 2);
            fs.WriteByte((byte)status);
            int childSize = 0;
            if (child != null)
                childSize = child.Length;
            fs.Write(BitConverter.GetBytes(childSize), 0, 4);
            if (child == null) return;

            for (int i = 0; i < childSize; i++)
                child[i].Walk2Save(fs);
        }

        public void Walk2SaveWithValue(FileStream fs)
        {
            fs.Write(BitConverter.GetBytes(c), 0, 2);
            fs.WriteByte((byte)status);
            if (status == Status.WORD_MIDDLE || status == Status.WORD_END)
            {
                var valBytes = Serializer.Serialize(v);
                fs.Write(BitConverter.GetBytes(valBytes.Length), 0, 4);
                fs.Write(valBytes, 0, valBytes.Length);
            }

            int childSize = 0;
            if (child != null) childSize = child.Length;

            fs.Write(BitConverter.GetBytes(childSize), 0, 4);
            if (child == null) return;

            for(int i = 0; i < childSize; i++)
            {
                child[i].Walk2SaveWithValue(fs);
            }
        }

        public void Walk2Load(ByteArray ba, ValueArray<V> va, bool compatible = false)
        {
            c = ba.NextChar(compatible);
            status = (Status)ba.NextInt(compatible);
            if(status == Status.WORD_MIDDLE || status == Status.WORD_END)
            {
                v = va.NextValue();
            }

            var childSize = ba.NextInt(compatible);
            child = new BaseNode<V>[childSize];
            for(int i = 0; i < childSize; i++)
            {
                child[i] = new Node<V>();
                child[i].Walk2Load(ba, va, compatible);
            }
        }

        public void Walk2LoadWithValue(FileStream fs)
        {
            var bytes = new byte[2];
            fs.Read(bytes, 0, 2);
            c = BitConverter.ToChar(bytes, 0);

            status = (Status)fs.ReadByte();
            if(status == Status.WORD_MIDDLE || status == Status.WORD_END)
            {
                bytes = new byte[4];
                fs.Read(bytes, 0, 4);
                var valLen = BitConverter.ToInt32(bytes, 0);
                bytes = new byte[valLen];
                fs.Read(bytes, 0, valLen);
                v = Serializer.Deserialize<V>(bytes);
            }
            bytes = new byte[4];
            fs.Read(bytes, 0, 4);
            var childSize = BitConverter.ToInt32(bytes, 0);
            child = new BaseNode<V>[childSize];
            for (int i = 0; i < childSize; i++)
            {
                child[i] = new Node<V>();
                child[i].Walk2LoadWithValue(fs);
            }
        }

        

        public override string ToString() => $"c={c}, value={v}, status={status}";

        public override int GetHashCode() => c;
    }

    public class TrieEntry<V> : IComparable<TrieEntry<V>>
    {
        private string _key;
        private V _v;

        public string Key => _key;
        public V Value => _v;
        public TrieEntry(string item1, V item2)
        {
            _key = item1;
            _v = item2;
        }

        public int CompareTo(TrieEntry<V> other) => this._key.CompareTo(other._key);
    }

    public enum Status
    {
        /// <summary>
        /// 未定义
        /// </summary>
        UNDEFINED,
        /// <summary>
        /// 不是词语结尾
        /// </summary>
        NOT_WORD,
        /// <summary>
        /// 是词语结尾，并且还可以继续
        /// </summary>
        WORD_MIDDLE,
        /// <summary>
        /// 是词语结尾，但没有继续
        /// </summary>
        WORD_END
    }
}
