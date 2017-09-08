using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HanLP.csharp.seg.common;
using HanLP.csharp.dictionary;
using HanLP.csharp.recognition;

namespace HanLP.csharp.seg.viterbi
{
    public class ViterbiSegment : WordBasedGenerativeModelSegment
    {
        /// <summary>
        /// 将给定句子分词
        /// </summary>
        /// <param name="sentence"></param>
        /// <returns></returns>
        public override List<Term> SegSentence(char[] sentence)
        {
            var wordNetAll = new WordNet(sentence);
            //-------------------- 生成一元词网 --------------------------
            GenerateWordNet(wordNetAll);

            // 使用Viterbi分词
            var list = Viterbi(wordNetAll);

            if(config.useCustomDict)    // 需要使用自定义词典
            {
                if (config.indexMode)   // 开启索引模式
                    CombineByCustomDict(list, wordNetAll);
                else
                    CombineByCustomDict(list);
            }

            if (config.numQuantRecognize)               // 数量词识别
                CombineNumQuant(list, wordNetAll, config);

            if(config.nameEntityRecognize)      // 命名实体识别
            {
                var wordNetOptimum = new WordNet(sentence, list);   // 
                var preSize = wordNetOptimum.Size;

                if (config.chsNameRecognize)         // 中文人名识别
                    ChsNameRecognition.Recognition(list, wordNetOptimum, wordNetAll);

                if (config.translatedNameRecognize)
                    TranslatedPersonRecognition.Recognition(list, wordNetOptimum, wordNetAll);

                if(config.jpNameRecognize)
                {

                }
                if(config.placeRecognize)
                {

                }

                if(config.orgRecognize)
                {
                    list = Viterbi(wordNetOptimum);
                    wordNetOptimum.Clear();
                    wordNetOptimum.AddAll(list);
                    preSize = wordNetOptimum.Size;
                    OrgRecognition.Recognition(list, wordNetOptimum, wordNetAll);
                }
                if(wordNetOptimum.Size != preSize)
                {
                    list = Viterbi(wordNetOptimum);
                }
            }

            return Convert(list, config.offset);
        }

        /// <summary>
        /// Viterbi
        /// 其实不是分词，而是从词网中选择一条概率最大的路径
        /// </summary>
        /// <param name="wordNet"></param>
        /// <returns></returns>
        private static List<Vertex> Viterbi(WordNet wordNet)
        {
            var lines = wordNet.Vertices;
            var list = new List<Vertex>();
            foreach (var node in lines[1])      // 遍历第一行节点列表
                node.UpdateMyFrom(lines[0].First());  // 第一行每个节点更新，设置第0 行节点（起始辅助节点）为其前驱

            //! 需要注意的是，UpdateFrom方法更新前驱节点的时候使用了Viterbi方法根据联合概率最大来跟新的
            for(int i = 1; i < lines.Length - 1; i++)
            {
                var line = lines[i];
                if (line == null || line.Count == 0) continue;     // 空行跳过
                foreach(var node in line)
                {
                    if (node.from == null) continue;
                    foreach (var to in lines[i + node.realWord.Length])     // 遍历当前节点的下一行的各节点，注意当前行不同长度的节点对应的下一行也不同
                        to.UpdateMyFrom(node);                              // 下一行各节点以当前节点为前驱节点
                }
            }
            var from = lines[lines.Length - 1].First();     // 最后一行，仅有一个辅助节点，其前驱节点为原始句子的最后一个词
            while(from != null)             // 从最后一个辅助节点开始，根据前驱节点倒推得到所有节点
            {
                list.Add(from);     
                from = from.from;
            }
            list.Reverse();         // 反转，得到顺序
            return list;            // 得到词列表，包括首尾辅助节点
        }
    }
}
