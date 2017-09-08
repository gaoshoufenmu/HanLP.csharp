using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HanLP.csharp.corpus.tag;
using HanLP.csharp.collection;
using HanLP.csharp.seg.common;
using HanLP.csharp.collection.AhoCorasick;
using static HanLP.csharp.Constants;
using WordAttr = HanLP.csharp.corpus.model.Attribute;

namespace HanLP.csharp.dictionary.nr
{
    /// <summary>
    /// 中文人名识别词典
    /// </summary>
    public class ChsPersonNameDict
    {
        public static NRDictionary dictionary;

        public static TransformMatrixDictionary<NR> transformMatrixDictionary;

        /// <summary>
        /// 人名识别模式
        /// </summary>
        public static ACDoubleArrayTrie<NRPattern> _trie;

        public static readonly WordAttr ATTRIBUTE = new WordAttr(Nature.nr, 100);

        static ChsPersonNameDict()
        {
            dictionary = new NRDictionary();
            if(!dictionary.Load(Config.Person_Dict_Path))
            {
                // log: loading error
                return;
            }

            transformMatrixDictionary = new TransformMatrixDictionary<NR>(typeof(NR));
            transformMatrixDictionary.Load(Config.Person_TR_Dict_Path);

            _trie = new ACDoubleArrayTrie<NRPattern>();
            var map = new SortedDictionary<string, NRPattern>(StrComparer.Default);

            var nrPatMax = (int)NRPattern.XD + 1;
            for(int i = 0; i < nrPatMax; i++)
            {
                var nrPat = (NRPattern)i;
                map.Add(nrPat.ToString(), nrPat);
            }

            _trie.Build(map);
        }

        /// <summary>
        /// 模式匹配
        /// </summary>
        /// <param name="nrs">确定的标注序列</param>
        /// <param name="vertexs">原始的未加角色标注的序列</param>
        /// <param name="wordNetOptimum"></param>
        /// <param name="wordNetAll"></param>
        public static void PatternMatch(List<NR> nrs, List<Vertex> vertexs, WordNet wordNetOptimum, WordNet wordNetAll)
        {
            var sb = new StringBuilder(nrs.Count);  // 存储 NR 的枚举模式串
            var preNR = NR.A;
            bool backUp = false;
            int index = 0;
            for(int i = 0; i < nrs.Count; i++, index++)
            {
                var cur = vertexs[index];
                switch (nrs[i])
                {
                    case NR.U:      // 人名上文和姓成词， 比如： 这里【有关】天培的壮烈
                        if(!backUp)                 // 如果尚未备份，则备份一下
                        {
                            vertexs = new List<Vertex>(vertexs);
                            backUp = true;
                        }
                        sb.Append(NR.K.ToString());
                        sb.Append(NR.B.ToString());
                        preNR = NR.B;
                        var nowK = cur.realWord.Substring(0, cur.realWord.Length - 1);      // 人名的上文，参见上面 “有关”的“有”
                        var nowB = cur.realWord.Substring(cur.realWord.Length - 1);         // 最后一个字表示姓氏，单独提取出来，参见上面 “有关”的“关”
                        // 因为匹配到人名前缀与人名合在一个节点里面，将当前节点拆分
                        vertexs[index] = new Vertex(nowK);
                        vertexs.Insert(++index, new Vertex(nowB));
                        continue;
                    case NR.V:      // 人名末与下文成词，比如：“龚学平等领导”中的“龚学平等”，“邓颖超生前”中的“邓颖超生”
                        if (!backUp)
                        {
                            vertexs = new List<Vertex>(vertexs);
                            backUp = true;
                        }
                        if (preNR == NR.B)
                            sb.Append(NR.E.ToString());     // BE
                        else
                            sb.Append(NR.D.ToString());     // CD

                        sb.Append(NR.L.ToString());

                        var nowED = cur.realWord.Substring(cur.realWord.Length - 1);        // 提取最后一个字
                        var nowL = cur.realWord.Substring(0, cur.realWord.Length - 1);      // 
                        vertexs[index] = new Vertex(nowL);                                  // 1.
                        vertexs.Insert(++index, new Vertex(nowED));                         // 2. 这两行与原java代码执行顺序相反
                        continue;
                    default:
                        sb.Append(nrs[i].ToString());
                        break;
                }
                preNR = nrs[i];

                
            }

            var patternStr = sb.ToString();     // 所有节点连接起来形成的模式串
            var wordArr = vertexs.ToArray();

            var offsetArr = new int[wordArr.Length];
            offsetArr[0] = 0;
            for (int i = 1; i < wordArr.Length; i++)
                offsetArr[i] = offsetArr[i - 1] + wordArr[i - 1].realWord.Length;

            _trie.Match(patternStr, (begin, end, value) =>
            {
                var sbName = new StringBuilder();
                for (int i = begin; i < end; i++)
                    sbName.Append(wordArr[i].realWord);

                var name = sbName.ToString();
                switch(value)
                {
                    case NRPattern.BCD:
                        if (name[0] == name[2]) return;     // 认为姓和最后一个名不可能相等
                        break;
                }
                if (IsBadCase(name)) return;

                wordNetOptimum.Insert(offsetArr[begin], new Vertex(TAG_PEOPLE, name, ATTRIBUTE, CoreDictionary.NR_WORD_ID), wordNetAll);
            });
        }

        /// <summary>
        /// 因为任何算法都无法解决100%的问题，总是有一些bad case，这些bad case会以“盖公章 A 1”的形式加入词典中
        /// 这个方法返回人名是否是bad case
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static bool IsBadCase(string name)
        {
            var tfi = dictionary.Get(name);
            if (tfi == null) return false;
            return tfi.ContainsLabel(NR.A);
        }
    }
}
