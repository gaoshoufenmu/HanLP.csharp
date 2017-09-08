using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HanLP.csharp.collection.AhoCorasick;

namespace HanLP.csharp.dictionary.ts
{
    class TraditionalChineseDictionary : BaseChineseDictionary
    {
        public static ACDoubleArrayTrie<string> trie = new ACDoubleArrayTrie<string>();

        static TraditionalChineseDictionary()
        {
            if (!Load(Config.Simple2Traditial_Dict_Dir + "t2s.txt", trie, false))
                throw new ArgumentException("Simplified & Traditional Chinese dict files: load error");
        }

        /// <summary>
        /// 繁体句子转简体句子
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Traditional2Simplified(string input) => Seg4Longest(input.ToCharArray(), trie);
        /// <summary>
        /// 繁体句子转简体句子
        /// </summary>
        /// <param name="chars"></param>
        /// <returns></returns>
        public static string Traditional2Simplified(char[] chars) => Seg4Longest(chars, trie);
    }
}
