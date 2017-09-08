using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HanLP.csharp.seg.common;
using HanLP.csharp.corpus.tag;

namespace HanLP.csharp.utility
{
    public class SentenceUtil
    {
        /// <summary>
        /// 根据一些标点符号将文本分成多个句子
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static List<string> ToSentenceList(string text) => ToSentenceList(text.ToCharArray());

        /// <summary>
        /// 根据一些标点符号将文本分成多个句子
        /// </summary>
        /// <param name="chars"></param>
        /// <returns></returns>
        public static List<string>  ToSentenceList(char[] chars)
        {
            var sb = new StringBuilder();
            var list = new List<string>();

            for(int i = 0; i < chars.Length; i++)
            {
                if(sb.Length == 0 &&(char.IsWhiteSpace(chars[i]) || chars[i] == '	' || chars[i] == '　')) //
                {
                    continue;       // 忽略开头的空白符
                }

                sb.Append(chars[i]);
                switch(chars[i])
                {
                    case '.':
                        if (i < chars.Length - 1 && chars[i + 1] > 128) // 当前是 点号，且不是最后一个字符，且后跟一个非ascii字符
                        {
                            InsertIntoList(sb, list);                   // 则 点号作为分隔符，连同前面的字符组成一个句子
                            sb.Clear();
                        }                                               // 否则准备处理下一个字符
                        break;
                    case '…':
                        if (i < chars.Length - 1 && chars[i + 1] == '…')    // 遇到省略号，则连同前面的字符组成一个句子
                        {
                            sb.Append('…');
                            ++i;
                            InsertIntoList(sb, list);
                            sb.Clear();
                        }
                        break;
                    case ' ':
                    case '　':
                    case '	':
                    case ' ':
                    case '。':
                    case '，':
                    case ',':
                        InsertIntoList(sb, list);
                        sb.Clear();
                        break;
                    case ';':
                    case '；':
                        InsertIntoList(sb, list);
                        sb.Clear();
                        break;
                    case '!':
                    case '！':
                        InsertIntoList(sb, list);
                        sb.Clear();
                        break;
                    case '?':
                    case '？':
                        InsertIntoList(sb, list);
                        sb.Clear();
                        break;
                    case '\n':
                    case '\r':
                        InsertIntoList(sb, list);
                        sb.Clear();
                        break;
                }
            }

            if (sb.Length > 0)
                InsertIntoList(sb, list);

            return list;
        }

        private static void InsertIntoList(StringBuilder sb, List<string> list)
        {
            var content = sb.ToString().Trim();
            if (content.Length > 0)
                list.Add(content);
        }

        public static bool HasNature(List<Term> list, Nature nature)
        {
            foreach(var term in list)
            {
                if (term.nature == nature)
                    return true;
            }
            return false;
        }
    }
}
