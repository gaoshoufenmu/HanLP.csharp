using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HanLP.csharp.corpus.tag;
using HanLP.csharp.dictionary;

namespace HanLP.csharp.seg.common
{
    /// <summary>
    /// 一个单词
    /// </summary>
    public class Term
    {
        /// <summary>
        /// 值
        /// </summary>
        public string word;
        /// <summary>
        /// 词性
        /// </summary>
        public Nature nature;
        /// <summary>
        /// 在文本中的起始位置（需开启分词器的offet选项）
        /// </summary>
        public int offset;

        public int Length => word.Length;

        public Term(string word, Nature nature)
        {
            this.word = word;
            this.nature = nature;
        }

        private int _freq = -1;
        public int Freq
        {
            get
            {
                if (_freq < 0)
                    _freq = CoreDictionary.GetFreq(word);
                return _freq;
            }
        }

        private int _comFreq = -1;
        /// <summary>
        /// 用户自定义词典的频率
        /// </summary>
        public int ComFreq
        {
            get
            {
                if (_comFreq < 0)
                {
                    _comFreq = ComHighFreqDict.GetFreq(word);
                    if (_comFreq == 0)
                        _comFreq = 1;
                }
                return _comFreq;
            }
        }

        public override string ToString() => Config.ShowTermNature ? word + "/" + nature : word;

        public ComTerm Copy2ComTerm()
        {
            var term = new ComTerm(word, nature);
            term.offset = offset;
            return term;
        }
    }

    public class ComTerm : Term
    {
        public NatCom nc;

        public bool Verified;

        public string ext1;  // 扩展字段，用于存储额外信息，具体需要根据NatCom词性来确定存储的什么样的信息

        public ComTerm(string word, Nature nature) : base(word, nature)
        {
        }
    }

    public class ResultTerm<V>
    {
        public string word;
        public V label;
        public int offset;

        public ResultTerm(string word, V label, int offset)
        {
            this.word = word;
            this.label = label;
            this.offset = offset;
        }

        public override string ToString() => word + '/' + label;
    }
}
