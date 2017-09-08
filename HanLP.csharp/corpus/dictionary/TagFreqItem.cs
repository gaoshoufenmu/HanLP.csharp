using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HanLP.csharp.collection;
namespace HanLP.csharp.corpus.dictionary
{
    /// <summary>
    /// 标签和频次的简单条目类
    /// </summary>
    public class SimpleItem
    {
        public IDictionary<string, int> labelMap = new SortedDictionary<string, int>(StrComparer.Default);
        public void AddLabel(string label)
        {
            if (labelMap.ContainsKey(label))
            {
                labelMap[label] += 1;
            }
            else
                labelMap[label] = 1;
        }

        public void AddLabel(string label, int freq)
        {
            if (labelMap.ContainsKey(label))
                labelMap[label] += freq;
            else
                labelMap[label] = freq;
        }

        public void RemoveLabel(string label) => labelMap.Remove(label);

        public bool ContainsLabel(string label) => labelMap.ContainsKey(label);

        public int GetFreqOrDefault(string label, int f = 0)
        {
            int freq;
            if (labelMap.TryGetValue(label, out freq))
                return freq;

            return f;
        }

        public static SimpleItem Create(string param)
        {
            if (param == null) return null;
            return Create(param.Split(' '));
        }

        public static SimpleItem Create(string[] param_s)
        {
            if (param_s.Length % 2 == 1) return null;

            var item = new SimpleItem();
            for(int i = 0; i < param_s.Length / 2; i++)
            {
                item.labelMap[param_s[i << 1]] = int.Parse(param_s[1 + i * 2]);
            }
            return item;
        }

        public void Combine(SimpleItem other)
        {
            foreach(var p in other.labelMap)
            {
                AddLabel(p.Key, p.Value);
            }
        }
        public int GetTotalFreq()
        {
            var freq = 0;
            foreach (var p in labelMap)
                freq += p.Value;
            return freq;
        }

    }

    public class Item : SimpleItem
    {
        /// <summary>
        /// 该条目的索引，比如“啊”
        /// </summary>
        public string key;
        public Item(string key, string label)
        {
            this.key = key;
            labelMap[label] = 1;
        }
        public Item(string key)
        {
            this.key = key;
        }

        public override string ToString() => key;

        public static Item Create(string param)
        {
            if (param == null) return null;
            return Create(param.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries));
        }
        public static Item Create(string[] param_s)
        {
            if (param_s.Length % 2 == 0) return null;
            Item item = new Item(param_s[0]);
            int count = (param_s.Length - 1) / 2;
            for (int i = 0; i < count; i++)
                item.labelMap[param_s[1 + 2 * i]] = int.Parse(param_s[2 + 2 * i]);

            return item;
        }
    }

    /// <summary>
    /// 对标签-频次的封装
    /// </summary>
    /// <typeparam name="E"></typeparam>
    public class TagFreqItem<E> where E : IConvertible
    {
        public IDictionary<E, int> labelMap = new SortedDictionary<E, int>();
        public TagFreqItem() { }
        public TagFreqItem(E label, int freq)
        {
            labelMap[label] = freq;
        }

        public TagFreqItem(params E[] labels)
        {
            for (int i = 0; i < labels.Length; i++)
                labelMap[labels[i]] = 1;
        }

        public void AddLabel(E label)
        {
            if (labelMap.ContainsKey(label))
                labelMap[label] += 1;
            else
                labelMap[label] = 1;
        }

        public void AddLabel(E label, int freq)
        {
            if (labelMap.ContainsKey(label))
                labelMap[label] += freq;
            else
                labelMap[label] = freq;
        }

        public bool ContainsLabel(E label) => labelMap.ContainsKey(label);
        public int GetFreqOrDefault(E label, int f = 0)
        {
            if (labelMap.TryGetValue(label, out int freq))
                return freq;
            return f;
        }

        public static Tuple<string, KeyValuePair<string, int>[]> Create(string param)
        {
            if (param == null) return null;
            return Create(param.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries));
        }

        public static Tuple<string, KeyValuePair<string, int>[]> Create(string[] param_s)
        {
            if (param_s.Length % 2 == 0) return null;
            var count = param_s.Length / 2;

            var entries = new KeyValuePair<string, int>[count];
            for(int i = 0; i < count; i++)
            {
                entries[i] = new KeyValuePair<string, int>(param_s[1 + 2 * i], int.Parse(param_s[2 + 2 * i]));
            }
            return new Tuple<string, KeyValuePair<string, int>[]>(param_s[0], entries);
        }

        public override string ToString()
        {
            var sb = new StringBuilder(labelMap.Count * 4);
            foreach(var p in labelMap)
            {
                sb.Append(p.Key).Append(' ').Append(p.Value).Append(' ');
            }
            return sb.ToString();
        }
    }
}
