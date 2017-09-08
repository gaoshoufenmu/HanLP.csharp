using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HanLP.csharp.collection;
using HanLP.csharp.corpus.tag;
using HanLP.csharp.seg.common;
using HanLP.csharp.collection.AhoCorasick;
using static HanLP.csharp.Constants;
using WordAttr = HanLP.csharp.corpus.model.Attribute;

namespace HanLP.csharp.dictionary.nt
{
    public class OrgDictionary
    {
        public static NTDictionary dictionary;

        public static TransformMatrixDictionary<NT> transformMatrixDictionary;

        public static ACDoubleArrayTrie<string> _trie;

        public static readonly WordAttr ATTRIBUTE = CoreDictionary.GetAttr(CoreDictionary.NT_WORD_ID);

        static OrgDictionary()
        {
            dictionary = new NTDictionary();
            dictionary.Load(Config.Org_Dict_Path);
            transformMatrixDictionary = new TransformMatrixDictionary<NT>(typeof(NT));
            transformMatrixDictionary.Load(Config.Org_TR_Dict_Path);
            _trie = new ACDoubleArrayTrie<string>();

            var patternMap = new SortedDictionary<string, string>(StrComparer.Default);
            for(int i = 0; i <= (int)NTPattern.WWIWWCWD; i++)
            {
                var enumStr = ((NTPattern)i).ToString();
                patternMap.Add(enumStr, enumStr);
            }
            _trie.Build(patternMap);
        }

        public static void PatternMatch(List<NT> nts, List<Vertex> vertices, WordNet wordNetOptimum, WordNet wordNetAll)
        {
            var sb = new StringBuilder(nts.Count);
            for(int i = 0; i < nts.Count; i++)
            {
                sb.Append(nts[i].ToString());
            }

            var patternStr = sb.ToString();
            var vertexArr = vertices.ToArray();

            _trie.Match(patternStr, (begin, end, keyword) =>
            {
                var sbName = new StringBuilder();
                for(int i = begin; i < end; i++)
                {
                    sbName.Append(vertexArr[i].realWord);
                }

                var name = sbName.ToString();
                if (IsBadCase(name)) return;        // 对一些basecase 做出调整

                int offset = 0;
                for(int i = 0; i < begin; i++)
                {
                    offset += vertexArr[i].realWord.Length;
                }

                wordNetOptimum.Insert(offset, new Vertex(TAG_GROUP, name, ATTRIBUTE, CoreDictionary.NT_WORD_ID), wordNetAll);
            });
        }

        /// <summary>
        /// 因为任何算法都无法解决100%的问题，总是有一些bad case，这些bad case会以“单词 Z 1”的形式加入词典中<BR>
        /// 这个方法返回是否是bad case
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static bool IsBadCase(string name)
        {
            var tagFreqItem = dictionary.Get(name);
            if (tagFreqItem == null) return false;
            return tagFreqItem.ContainsLabel(NT.Z);
        }
    }
}
