using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HanLP.csharp.dictionary.ts;
using HanLP.csharp.dictionary.py;
using HanLP.csharp.seg.common;
using HanLP.csharp.tokenizer;
using HanLP.csharp.corpus.dependency;

namespace HanLP.csharp
{
    /// <summary>
    /// 总调用入口类
    /// </summary>
    public class HANLP
    {
        /// <summary>
        /// 繁体转简体
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Traditional2Simplified(string input) => TraditionalChineseDictionary.Traditional2Simplified(input);

        /// <summary>
        /// 简体转繁体
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Simplified2Traditional(string input) => SimplifiedChineseDictionary.Simplified2Traditional(input);


        //! ----------------- 获取中文拼音，网上还有几个实现可以参考 ------------------------
        //! 1. https://github.com/promeG/TinyPinyin
        //! 2. https://github.com/toolgood/ToolGood.Words
        //! 3. 搜索上述项目（或直接搜索中文转拼音）时额外发现的实现方案

        /// <summary>
        /// 获取指定文本的拼音
        /// </summary>
        /// <param name="input">文本</param>
        /// <param name="separator">拼音分隔符</param>
        /// <param name="remainNone">对于没有拼音的字符（如符号），是保留None还是使用原字符</param>
        /// <returns></returns>
        public static string GetPinyin(string input, string separator = "/", bool remainNone = false)
        {
            var list = PinyinDictionary.Translate2Pinyin(input, true);
            var sb = new StringBuilder(list.Count * (5 + separator.Length));

            var lastCharIndex = input.Length - 1;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == Pinyin.None && !remainNone)
                    sb.Append(input[i]);
                else
                    sb.Append(list[i].Pinyin_);

                if (i < lastCharIndex)
                    sb.Append(separator);
            }
            return sb.ToString();
        }

        /// <summary>
        /// 获取指定文本的拼音结构类型的列表
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static List<Pinyin> GetPinyins(string text, bool remainNone = false) => PinyinDictionary.Translate2Pinyin(text, remainNone);

        /// <summary>
        /// 获取文本的拼音的首字母，并连接成一个字符串
        /// </summary>
        /// <param name="text">文本</param>
        /// <param name="separator">连接成串的分隔符</param>
        /// <param name="remainNone">对没有拼音的字符（如符号），是保留None还是原字符</param>
        /// <returns></returns>
        public static string GetInitialChars(string text, string separator = "'", bool remainNone = false)
        {
            var list = PinyinDictionary.Translate2Pinyin(text, remainNone);
            var sb = new StringBuilder(list.Count * (1 + separator.Length));

            var lastCharIndex = list.Count - 1;
            for(int i = 0; i < list.Count; i++)
            {
                sb.Append(list[i].FirstChar);
                if (i < lastCharIndex)
                    sb.Append(separator);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Viterbi 分词
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static List<Term> Segment(string text) => StandardTokenizer.Segment(text);

        //public static CoNLLSentence ParseDependency(string sentence)
        //{

        //}
    }
}
