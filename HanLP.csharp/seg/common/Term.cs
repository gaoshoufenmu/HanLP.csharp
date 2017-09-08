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
        public override string ToString() => Config.ShowTermNature ? word + "/" + nature : word;
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
