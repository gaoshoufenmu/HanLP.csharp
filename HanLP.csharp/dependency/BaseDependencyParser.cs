using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HanLP.csharp.corpus.dependency;
using HanLP.csharp.corpus.document;
using HanLP.csharp.seg.common;
using HanLP.csharp.seg;
using HanLP.csharp.seg.viterbi;

namespace HanLP.csharp.dependency
{
    public interface IDependencyParser
    {
        /// <summary>
        /// 分析句子的依存句法
        /// </summary>
        /// <param name="terms">表示句子的所有单词，可以是任何具有词性标注功能的分词器的分词结果</param>
        /// <returns>CoNLL格式的依存句法树</returns>
        CoNLLSentence Parse(List<Term> terms);

        /// <summary>
        /// 分析句子的依存句法
        /// </summary>
        /// <param name="sentence"></param>
        /// <returns></returns>
        CoNLLSentence Parse(string sentence);
        /// <summary>
        /// Parser使用的分词器
        /// </summary>
        /// <returns></returns>
        Segment Segment { get; set; }
        ///// <summary>
        ///// 设置Parser使用的分词器
        ///// </summary>
        ///// <param name="seg"></param>
        ///// <returns></returns>
        //IDependencyParser SetSegment(Segment seg);
        /// <summary>
        /// 依存关系映射表
        /// </summary>
        /// <returns></returns>
        IDictionary<string, string> DepRelTranslater { get; set; }
        ///// <summary>
        ///// 设置依存关系映射表
        ///// </summary>
        ///// <param name="depRelTranslater"></param>
        ///// <returns></returns>
        //IDependencyParser SetDepRelTranslator(IDictionary<string, string> depRelTranslater);
        /// <summary>
        /// 依存关系自动转换开关
        /// </summary>
        /// <param name="enable"></param>
        /// <returns></returns>
        IDependencyParser EnableDepRelTranslater(bool enable);
    }


    public abstract class BaseDependencyParser : IDependencyParser
    {
        /// <summary>
        /// 开启词性标注
        /// </summary>
        private Segment _segment = new ViterbiSegment().SetNatureTag(true);

        public Segment Segment { get => _segment; set => _segment = value; }


        private IDictionary<string, string> _depRelTranslater;

        public IDictionary<string, string> DepRelTranslater { get => _depRelTranslater; set => _depRelTranslater = value; }

        private bool _enableDepRelTranslater;


        public CoNLLSentence Parse(string sentence)
        {
            if (string.IsNullOrWhiteSpace(sentence)) return null;
            var output = Parse(_segment.Seg(sentence.ToCharArray()));
            if(_enableDepRelTranslater && _depRelTranslater != null)
            {
                for(int i = 0; i < output.word.Length; i++)        // 遍历句子中的各单词
                {
                    var word = output.word[i];
                    if (_depRelTranslater.TryGetValue(word.DEPREL, out var translatedDeprel))
                        word.DEPREL = translatedDeprel;
                }
            }
            return output;
        }
        public IDependencyParser EnableDepRelTranslater(bool enable)
        {
            _enableDepRelTranslater = enable;
            return this;
        }

        public virtual CoNLLSentence Parse(List<Term> terms)
        {
            throw new NotImplementedException();
        }
    }
}
