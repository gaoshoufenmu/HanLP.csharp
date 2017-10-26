using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HanLP.csharp.seg.common;
using HanLP.csharp.corpus.dictionary;
using HanLP.csharp.corpus.tag;
using HanLP.csharp.algorithm;
using HanLP.csharp.dictionary.ns;
namespace HanLP.csharp.recognition
{
    public class PlaceRecognition
    {
        public static bool Recognition(List<Vertex> vertices, WordNet wordnet_op, WordNet wordnet_all)
        {
            var tagItems = RoleTag(vertices, wordnet_all);
            var nss = ViterbiExCompute(tagItems);
            PlaceDictionary.PatternMatch(nss, vertices, wordnet_op, wordnet_all);
            return true;
        }

        private static List<TagFreqItem<NS>> RoleTag(List<Vertex> vertices, WordNet wordnet_all)
        {
            var tagList = new List<TagFreqItem<NS>>();
            for (int i = 0; i < vertices.Count; i++)
            {
                var vertex = vertices[i];

                var nature = vertex.GetNature();
                if (Nature.ns == nature && vertex.attr.totalFreq <= 1000)
                {
                    if (vertex.realWord.Length < 3)     // 二字地名
                        tagList.Add(new TagFreqItem<NS>(NS.H, NS.G));
                    else
                        tagList.Add(new TagFreqItem<NS>(NS.G));
                    continue;
                }
                var tfi = PlaceDictionary.dict.Get(vertex.word);        // 使用等效词
                if (tfi == null)
                    tfi = new TagFreqItem<NS>(NS.Z, PlaceDictionary.trans_tr_dict.GetFreq(NS.Z));
                tagList.Add(tfi);
            }
            return tagList;
        }

        public static List<NS> ViterbiExCompute(List<TagFreqItem<NS>> roleTagList) =>
            Viterbi.Compute(roleTagList, PlaceDictionary.trans_tr_dict);
    }
}
