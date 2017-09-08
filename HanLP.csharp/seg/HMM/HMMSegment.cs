using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HanLP.csharp.model.trigram;
using HanLP.csharp.corpus.io;
using HanLP.csharp.utility;
using HanLP.csharp.seg.common;
using HanLP.csharp.corpus.tag;

namespace HanLP.csharp.seg.HMM
{
    /// <summary>
    /// based on 2-kind HMM segment model
    /// </summary>
    public class HMMSegment : CharBasedGenerativeModelSegment
    {
        private CharBasedGenerativeModel _model;
        public HMMSegment(string modelPath)
        {
            _model = GlobalCache.Get(modelPath) as CharBasedGenerativeModel;
            if (_model != null) return;

            _model = new CharBasedGenerativeModel();
            var ba = ByteArray.Create(modelPath);
            if (ba == null) throw new ArgumentException($"HMM segment model file does not exist {modelPath}");
            _model.Load(ba);

            GlobalCache.Put(modelPath, _model);
        }
        public HMMSegment() : this(Config.HMM_Segment_Model_Path) { }

        public override List<Term> SegSentence(char[] sentence)
        {
            var tags = _model.Tag(sentence);
            var terms = new List<Term>();
            int offset = 0;

            for(int i = 0; i < tags.Length; i++, offset++)
            {
                switch(tags[i])
                {
                    case 'b':
                        var begin = offset;         // 记录起始位置
                        while(tags[i] != 'e')       // 继续下一个位置，直到找到结束位置'e'
                        {
                            offset++;
                            i++;
                            if (i == tags.Length) break;        // 防止标注错误，未能将结束'e'标注出来
                        }
                        if (i == tags.Length)                   // 已经分词完毕
                            terms.Add(new Term(new string(sentence, begin, offset - begin), Nature.none));      //! 不显示词性
                        else
                            terms.Add(new Term(new string(sentence, begin, offset - begin + 1), Nature.none));

                        break;
                    default:
                        terms.Add(new Term(new string(sentence, offset, 1), Nature.none));
                        break;
                }
            }
            return terms;
        }
    }
}
