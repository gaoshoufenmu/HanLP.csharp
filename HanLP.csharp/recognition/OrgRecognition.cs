using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HanLP.csharp.seg.common;
using HanLP.csharp.dictionary.nt;
using HanLP.csharp.corpus.tag;
using HanLP.csharp.algorithm;
using HanLP.csharp.corpus.dictionary;

namespace HanLP.csharp.recognition
{
    public class OrgRecognition
    {
        public static bool Recognition(List<Vertex> vertices, WordNet wordNetOptimum, WordNet wordNetAll)
        {
            var tagItems = RoleTag(vertices, wordNetAll);
            var nts = ViterbiExCompute(tagItems);

            OrgDictionary.PatternMatch(nts, vertices, wordNetOptimum, wordNetAll);
            return true;
        }

        /// <summary>
        /// 找出给定顶点列表中的顶点的关联词性标签，以及对应在机构词典中的《标签，频率》pair。
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="wordNetAll"></param>
        /// <returns></returns>
        public static List<TagFreqItem<NT>> RoleTag(List<Vertex> vertices, WordNet wordNetAll)
        {
            var tagList = new List<TagFreqItem<NT>>();

            for(int i = 0; i < vertices.Count; i++)         // 遍历顶点
            {
                var vertex = vertices[i];                   // 当前顶点

                // 找出当前词条的所有关联词性，并作为
                var nature = vertex.GetNature();            // 当前顶点（词条）的词性
                switch(nature)
                {
                    case Nature.nrf:                        // 音译人名
                        if (vertex.attr.totalFreq <= 1000)
                        {
                            tagList.Add(new TagFreqItem<NT>(NT.F, 1000));
                            continue;
                        }
                        break;
                    case Nature.ni:                         // 机构相关名称
                    case Nature.nic:
                    case Nature.nis:
                    case Nature.nit:
                        var tfi = new TagFreqItem<NT>(NT.K, 1000);          // 
                        tfi.AddLabel(NT.D, 1000);
                        tagList.Add(tfi);
                        continue;
                    case Nature.m:
                        tagList.Add(new TagFreqItem<NT>(NT.M, 1000));
                        continue;
                }

                var tagItem = OrgDictionary.dictionary.Get(vertex.word);        // 此处使用等效词，更加精准
                if (tagItem == null)
                    tagItem = new TagFreqItem<NT>(NT.Z, OrgDictionary.transformMatrixDictionary.GetFreq(NT.Z));

                tagList.Add(tagItem);
            }
            return tagList;
        }

        /// <summary>
        /// 维特比算法求解最优标签
        /// </summary>
        /// <param name="roleTagList"></param>
        /// <returns></returns>
        public static List<NT> ViterbiExCompute(List<TagFreqItem<NT>> roleTagList) => Viterbi.Compute(roleTagList, OrgDictionary.transformMatrixDictionary);
    }
}
 