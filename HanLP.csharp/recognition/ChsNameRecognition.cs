using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HanLP.csharp.seg.common;
using HanLP.csharp.corpus.tag;
using HanLP.csharp.corpus.dictionary;
using HanLP.csharp.dictionary.nr;
using HanLP.csharp.algorithm;

namespace HanLP.csharp.recognition
{
    /// <summary>
    /// 中文人名识别
    /// </summary>
    public class ChsNameRecognition
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="wordSegs">句子分词后的词条列表</param>
        /// <param name="wordNetOptimum"></param>
        /// <param name="wordNetAll"></param>
        /// <returns></returns>
        public static bool Recognition(List<Vertex> wordSegs, WordNet wordNetOptimum, WordNet wordNetAll)
        {
            var tags = RoleObserve(wordSegs);
            var nrs = ViterbiComputeSimply(tags);

            ChsPersonNameDict.PatternMatch(nrs, wordSegs, wordNetOptimum, wordNetAll);
            return true;
        }

        /// <summary>
        /// 角色观察
        /// </summary>
        /// <param name="wordSegs"></param>
        /// <returns></returns>
        public static List<TagFreqItem<NR>> RoleObserve(List<Vertex> wordSegs)
        {
            var tagList = new List<TagFreqItem<NR>>() { new TagFreqItem<NR>(NR.A, NR.K) };      // 始 ## 始 A K
            var dict = ChsPersonNameDict.dictionary;
            for(int i = 1; i < wordSegs.Count; i++)         // 跳过起始辅助节点
            {
                var vertex = wordSegs[i];
                var nritem = dict.Get(vertex.realWord);         // 获取词条（节点）的字符串值对应的《标签，频率》pair
                if(nritem == null)                      // 如果没有字符串对应的TagFreqItem，那就由顶点对应的词性来帮助分析
                {
                    switch(vertex.GuessNature())        
                    {
                        case Nature.nr:         // 如果词性是人名，
                            if (vertex.attr.totalFreq <= 1000 && vertex.realWord.Length == 2)
                                nritem = new TagFreqItem<NR>(NR.X, NR.G);
                            else
                                nritem = new TagFreqItem<NR>(NR.A, ChsPersonNameDict.transformMatrixDictionary.GetFreq(NR.A));
                            break;
                        case Nature.nnt:        // 职务职称
                            nritem = new TagFreqItem<NR>(NR.G, NR.K);
                            break;
                        default:
                            nritem = new TagFreqItem<NR>(NR.A, ChsPersonNameDict.transformMatrixDictionary.GetFreq(NR.A));
                            break;
                    }
                }       // 如果人名词典中存在当前顶点这样的词条，那直接添加对应的TagFreqItem到列表中
                tagList.Add(nritem);
            }

            return tagList;
        }

        public static List<NR> ViterbiCompute(List<TagFreqItem<NR>> tags) => Viterbi.Compute(tags, ChsPersonNameDict.transformMatrixDictionary);

        public static List<NR> ViterbiComputeSimply(List<TagFreqItem<NR>> tags) => Viterbi.ComputeSimply(tags, ChsPersonNameDict.transformMatrixDictionary);
    }
}
