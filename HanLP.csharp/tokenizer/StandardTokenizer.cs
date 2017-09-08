using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HanLP.csharp.seg;
using HanLP.csharp.seg.viterbi;
using HanLP.csharp.seg.common;

namespace HanLP.csharp.tokenizer
{
    /// <summary>
    /// 标准分词器，Viterbi分词
    /// </summary>
    public class StandardTokenizer
    {
        public static readonly Segment SEGMENT = new ViterbiSegment();

        public static List<Term> Segment(string text) => SEGMENT.Seg(text.ToCharArray());

        public static List<Term> Segment(char[] chars) => SEGMENT.Seg(chars);

        public static List<List<Term>> Seg2Sentence(string text) => SEGMENT.Seg2Sentence(text);
    }
}
