using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace HanLP.csharp.corpus.document
{
    /// <summary>
    /// 句子，以"。，：！"结尾的认为是句子
    /// </summary>
    public class Sentence
    {
        public List<IWord> Words;
        public Sentence(List<IWord> words) => Words = words;

        public override string ToString()
        {
            var sb = new StringBuilder(Words.Count * 4);
            for(int i = 0; i < Words.Count; i++)
            {
                if (i != 0)
                    sb.Append(' ');
                sb.Append(Words[i].ToString());
            }
            return sb.ToString();
        }

        // 词语的正则表达式（从句子中匹配出词语）
        // 1. 以'['开始，后面接
        //  1-1. 一个或多个“非空白”字符，然后接分隔符'/'，然后接一个或多个“阿拉伯数字或英文字母”，然后接一个或多个“空白”符。整个1-1部分可以是零个或多个
        //  1-2. 一个或多个“非空白”字符，然后接分隔符'/'，然后接一个或多个“阿拉伯数字或英文字母”，然后接一个"]/"，然后接一个或多个“阿拉伯数字或英文字母”
        // 2. 一个或多个“非空白”字符，然后接分隔符'/'，然后接一个或多个“阿拉伯数字或英文字母”
        private static Regex _pattern = new Regex(@"(\[(([^\s]+/[0-9a-zA-Z]+)\s+)+?([^\s]+/[0-9a-zA-Z]+)]/[0-9a-zA-Z]+)|([^\\s]+/[0-9a-zA-Z]+)", RegexOptions.Compiled);

        public static Sentence Create(string input)
        {
            var matches = _pattern.Matches(input);
            var words = new List<IWord>();
            foreach(Match m in matches)
            {
                var str = m.Value;
                var word = WordFactory.Create(str);
                if (word == null) return null;

                words.Add(word);
            }
            return new Sentence(words);
        }
    }
}
