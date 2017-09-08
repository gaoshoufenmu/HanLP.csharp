using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using HanLP.csharp.corpus.tag;
using HanLP.csharp.utility;
using HanLP.csharp.corpus.io;

namespace HanLP.csharp.corpus.model
{
    /// <summary>
    /// 词属性，包括词性数组，词频数组（与词性数组一一对应，一个词可能有多个词性，如“研究”可以是名词或动词），以及总词频
    /// </summary>
    public class Attribute
    {
        /// <summary>
        /// 词性数组
        /// </summary>
        public Nature[] natures;
        /// <summary>
        /// 词性对应的词频
        /// </summary>
        public int[] freqs;
        /// <summary>
        /// 总词频
        /// </summary>
        public int totalFreq;

        /// <summary>
        /// 构建词属性对象
        /// </summary>
        /// <param name="size">词性数组大小</param>
        public Attribute(int size)
        {
            natures = new Nature[size];
            freqs = new int[size];
        }

        /// <summary>
        /// 构建词属性对象
        /// </summary>
        /// <param name="nats">指定词性数组</param>
        /// <param name="fs">指定词频数组</param>
        public Attribute(Nature[] nats, int[] fs)
        {
            natures = nats;
            freqs = fs;
        }

        public Attribute(Nature nat, int freq)
        {
            natures = new[] { nat };
            freqs = new[] { freq };
            totalFreq = freq;
        }

        public Attribute(Nature[] nats, int[] fs, int totalFreq)
        {
            natures = nats;
            freqs = fs;
            this.totalFreq = totalFreq;
        }

        /// <summary>
        /// 使用单个词性，默认词频1000，构造对象
        /// </summary>
        /// <param name="nat"></param>
        public Attribute(Nature nat) : this(nat, 1000)
        { }

        /// <summary>
        /// 获取指定词性的词频
        /// </summary>
        /// <param name="nature"></param>
        /// <returns></returns>
        public int GetFreq(string nature)
        {
            if (Enum.TryParse<Nature>(nature, out var nat))
                return GetFreq(nat);
            return 0;
        }
        /// <summary>
        /// 当前属性是否包含指定词频
        /// </summary>
        /// <param name="nat"></param>
        /// <returns></returns>
        public bool HashNature(Nature nat) => GetFreq(nat) > 0;

        /// <summary>
        /// 是否存在指定前缀的词性
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public bool PrefixMatch(string prefix)
        {
            for(int i = 0; i < natures.Length; i++)
                if (NatureHelper.StartsWith(natures[i], prefix)) return true;
            return false;
        }

        public override string ToString()
        {
            var sb = new StringBuilder(natures.Length * 4);
            for(int i = 0; i < natures.Length; i++)
            {
                if (i == 0)
                    sb.Append(natures[i]).Append(' ').Append(freqs[i]);
                else
                    sb.Append(' ').Append(natures[i]).Append(' ').Append(freqs[i]);
            }
            return sb.ToString();
        }

        public void Save(FileStream fs)
        {
            var bytes = BitConverter.GetBytes(totalFreq);
            fs.Write(bytes, 0, 4);
            bytes = BitConverter.GetBytes(natures.Length);
            fs.Write(bytes, 0, 4);
            for(int i = 0; i < natures.Length; i++)
            {
                bytes = BitConverter.GetBytes((int)natures[i]);
                fs.Write(bytes, 0, 4);
                bytes = BitConverter.GetBytes(freqs[i]);
                fs.Write(bytes, 0, 4);
            }
        }

        /// <summary>
        /// 获取指定词性的词频
        /// </summary>
        /// <param name="nature"></param>
        /// <returns></returns>
        public int GetFreq(Nature nature)
        {
            for(int i = 0; i < natures.Length; i++)
            {
                if (nature == natures[i])
                    return freqs[i];
            }
            return 0;
        }

        public static Attribute Create(string natWithFreq)
        {
            try
            {
                var segs = natWithFreq.Split(' ');
                int natCount = segs.Length / 2;

                var attr = new Attribute(natCount);
                for(int i = 0; i < natCount; i++)
                {
                    attr.natures[i] = NatureHelper.GetOrCreate(segs[i << 1]);  //LexiconUtil.Str2Nat(segs[2 * i], null);
                    attr.freqs[i] = int.Parse(segs[2 * i + 1]);
                    attr.totalFreq += attr.freqs[i];
                }
                return attr;
            }
            catch(Exception e)
            {
                // log warning "creating Attribute failed";
                return null;
            }
        }

        public static Attribute Create(ByteArray ba, Nature[] nats)
        {
            var totalFreq = ba.NextInt();
            var len = ba.NextInt();
            var attr = new Attribute(len);
            attr.totalFreq = totalFreq;

            for(int i = 0; i < len; i++)
            {
                attr.natures[i] = nats[ba.NextInt()];
                attr.freqs[i] = ba.NextInt();
            }
            return attr;
        }

    }
}
