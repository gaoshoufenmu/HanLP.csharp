using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HanLP.csharp.seg.common;
using HanLP.csharp.dictionary.nr;
using HanLP.csharp.corpus.tag;
using HanLP.csharp.dictionary;
using static HanLP.csharp.Constants;
using WordAttr = HanLP.csharp.corpus.model.Attribute;

namespace HanLP.csharp.recognition
{
    public class TranslatedPersonRecognition
    {
        public static void Recognition(List<Vertex> vertices, WordNet wordNetOptimum, WordNet wordNetAll)
        {
            var sb = new StringBuilder();
            int appendTimes = 0;                        // stringbuilder 附加次数

            int line = 1;                               // 行号，跳过起始辅助节点
            int activeLine = 1;                         //
            for(int i = 1; i < vertices.Count; i++)     // 遍历节点，跳过起始辅助节点
            {
                var vertex = vertices[i];
                if(appendTimes > 0)                     // 已经附加过
                {
                    // 如果顶点词性为音译人名，或者音译人名词典包含顶点字符串值
                    if(vertex.GuessNature() == Nature.nrf || TranslatedPersonDictionary.ContainsKey(vertex.realWord))
                    {
                        sb.Append(vertex.realWord);
                        ++appendTimes;
                    }
                    else
                    {
                        // 识别结束
                        if(appendTimes > 1)             // 附加两次才算是一个完整的音译人名吗？
                        {
                            wordNetOptimum.Insert(activeLine, new Vertex(TAG_PEOPLE, sb.ToString(), new WordAttr(Nature.nrf), CoreDictionary.NR_WORD_ID), wordNetAll);
                        }
                        sb.Clear();
                        appendTimes = 0;
                    }
                }
                else                                    // 尚未附加过
                {
                    if(vertex.GuessNature() == Nature.nrf || TranslatedPersonDictionary.ContainsKey(vertex.realWord))
                    {
                        sb.Append(vertex.realWord);
                        ++appendTimes;
                        activeLine = line;              // 第一次附加，记录活跃行号
                    }
                }

                line += vertex.realWord.Length;         // 更新下一个顶点的行号
            }
        }
    }
}
