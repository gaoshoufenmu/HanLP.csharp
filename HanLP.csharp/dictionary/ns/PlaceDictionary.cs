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

namespace HanLP.csharp.dictionary.ns
{
    /// <summary>
    /// 地名识别
    /// </summary>
    public class PlaceDictionary
    {
        public static NSDictionary dict;

        public static TransformMatrixDictionary<NS> trans_tr_dict;

        public static ACDoubleArrayTrie<string> trie;

        public static readonly WordAttr ATTRIBUTE = CoreDictionary.GetAttr(CoreDictionary.NS_WORD_ID);

        static PlaceDictionary()
        {
            dict = new NSDictionary();
            dict.Load(Config.Place_Dict_Path);
            trans_tr_dict = new TransformMatrixDictionary<NS>(typeof(NS));
            trans_tr_dict.Load(Config.Place_TR_Dict_Path);
            trie = new ACDoubleArrayTrie<string>();

            var patternMap = new SortedDictionary<string, string>(StrComparer.Default);
            patternMap.Add("CH", null);
            patternMap.Add("CDH", null);
            patternMap.Add("CDEH", null);
            patternMap.Add("GH", null);
            trie.Build(patternMap);
        }

        public static void PatternMatch(List<NS> nss, List<Vertex> vertices, WordNet wordnet_op, WordNet wordnet_all)
        {
            var sb = new StringBuilder(nss.Count);
            for (int i = 0; i < nss.Count; i++)
                sb.Append(nss[i].ToString());

            var patternStr = sb.ToString();
            var vertexArr = vertices.ToArray();
            trie.Match(patternStr, (begin, end, keyword) =>
            {
                var sbName = new StringBuilder();
                for (int i = begin; i < end; i++)
                {
                    sbName.Append(vertexArr[i].realWord);
                }
                var name = sbName.ToString();
                if (IsBadCase(name)) return;
                int offset = 0;
                for (int i = 0; i < begin; i++)
                    offset += vertexArr[i].realWord.Length;

                wordnet_op.Insert(offset, new Vertex(TAG_PLACE, name, ATTRIBUTE, CoreDictionary.NS_WORD_ID), wordnet_all);
            });
        }

        private static bool IsBadCase(string name)
        {
            var tfi = dict.Get(name);
            if (tfi == null) return false;
            return tfi.ContainsLabel(NS.Z);
        }
    }
}
