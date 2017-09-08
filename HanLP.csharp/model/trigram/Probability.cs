using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using HanLP.csharp.collection.trie.bintrie;
using HanLP.csharp.corpus.io;

namespace HanLP.csharp.model.trigram
{
    /// <summary>
    /// 频率统计
    /// </summary>
    public class Probability
    {
        public BinTrie<int> d;
        /// <summary>
        /// 所有词的总频率
        /// </summary>
        private int _total;
        public int Total => _total;

        public Probability()
        {
            d = new BinTrie<int>();
        }

        public bool Exists(string key) => d.ContainsKey(key);
        public int Get(string key) => d.Get(key);
        public int GetOrDefault(string key, int freq = 0) => d.GetOrDefault(key, freq);
        //public int GetOrDefault(int freq, params char[][] keyArr) => d.GetOrDefault(Convert(keyArr), freq);
        public int Get(params char[] key) => d.Get(new string(key));
        public int GetOrDefault(int freq, params char[] key) => d.GetOrDefault(new string(key), freq);
        public int Get(params char[][] keyArr) => d.Get(Convert(keyArr));
        public int GetOrDefault(int freq, params char[][] keyArr) => d.GetOrDefault(Convert(keyArr), freq);

        /// <summary>
        /// 转换为词
        /// 合并所有char-tag pair的char字符形成一个子串
        /// </summary>
        /// <param name="keyArr">char-tag pair 数组</param>
        /// <returns></returns>
        private static string Convert(params char[][] keyArr)
        {
            var sb = new StringBuilder(keyArr.GetLength(0) * 2);
            foreach(var key in keyArr)
            {
                sb.Append(key[0]).Append(key[1]);
            }
            return sb.ToString();
        }

        private string Convert(ICollection<char[]> keyArr)
        {
            var sb = new StringBuilder(keyArr.Count * 2);
            foreach (var key in keyArr)
                sb.Append(key[0]).Append(key[1]);
            return sb.ToString();
        }

        /// <summary>
        /// 增加一个词频对
        /// 如果词已经存在，则增加其频率
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(string key, int value)
        {
            var freq = d.GetOrDefault(key, 0);
            freq += value;
            d.Put(key, freq);
            _total += value;
        }

        /// <summary>
        /// 增加一个词频对
        /// 如果词已经存在，则增加其频率
        /// </summary>
        /// <param name="value"></param>
        /// <param name="key"></param>
        public void Add(int value, params char[] key)
        {
            var k = new string(key);
            var freq = d.GetOrDefault(k);
            freq += value;
            d.Put(k, freq);
            _total += freq;
        }

        public void Add(int value, params char[][] keys) => Add(Convert(keys), value);

        /// <summary>
        /// 获取指定词的概率
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public double Prob(params char[] key)
        {
            var freq = d.GetOrDefault(new string(key));
            return freq / (double)_total;
        }

        /// <summary>
        /// 获取指定词的概率
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public double Prob(string key)
        {
            var freq = d.GetOrDefault(key);
            return freq / (double)_total;
        }

        public double Prob(params char[][] keyArr) => Prob(Convert(keyArr));

        public void Save(FileStream fs)
        {
            var bytes = BitConverter.GetBytes(_total);
            fs.Write(bytes, 0, 4);
            var valueArr = d.Values;
            bytes = BitConverter.GetBytes(valueArr.Length);
            fs.Write(bytes, 0, 4);
            foreach(var value in valueArr)
            {
                bytes = BitConverter.GetBytes(value);
                fs.Write(bytes, 0, 4);
            }
            d.Save(fs);
        }

        public bool Load(ByteArray ba, bool compatible = false)
        {
            _total = ba.NextInt(compatible);
            int size = ba.NextInt(compatible);
            var valueArr = new int[size];
            for (int i = 0; i < size; i++)
                valueArr[i] = ba.NextInt(compatible);         // valueArr[i] = ba.NextInt();


            var nChild = new BaseNode<int>[d.Child.Length - 1];
            Array.Copy(d.Child, nChild, nChild.Length);
            d.Child = nChild;
            return d.Load(ba, new ValueArray<int>().SetValues(valueArr), compatible);
            // return Load2BinTrie(ba, valueArr);
        }

        private bool Load2BinTrie(ByteArray ba, int[] valueArr, bool compatible = false)
        {
            var nChild = new BaseNode<int>[d.Child.Length - 1];
            Array.Copy(d.Child, nChild, nChild.Length);
            d.Child = nChild;
            return d.Load(ba, new ValueArray<int>().SetValues(valueArr), compatible);
        }
    }
}
