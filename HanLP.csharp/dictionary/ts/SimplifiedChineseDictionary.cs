using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HanLP.csharp.collection.AhoCorasick;

namespace HanLP.csharp.dictionary.ts
{
    public class SimplifiedChineseDictionary : BaseChineseDictionary
    {
        private static ACDoubleArrayTrie<string> _trie = new ACDoubleArrayTrie<string>();

        static SimplifiedChineseDictionary()
        {
            if (!Load(Config.Simple2Traditial_Dict_Dir + "s2t.txt", _trie, false))
                throw new ArgumentException("Simplified to Traditional dict file loading error.");
        }

        /// <summary>
        /// 将简体句子转为繁体句子
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Simplified2Traditional(string input) => Seg4Longest(input.ToCharArray(), _trie);

        /// <summary>
        /// 将简体句子转为繁体句子
        /// </summary>
        /// <param name="chars"></param>
        /// <returns></returns>
        public static string Simplified2Traditional(char[] chars) => Seg4Longest(chars, _trie);

        /// <summary>
        /// 将简体短语转换为繁体短语
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Simplified2Traditional_Phrase(string input) => _trie.GetOrDefault(input);
    }
}
