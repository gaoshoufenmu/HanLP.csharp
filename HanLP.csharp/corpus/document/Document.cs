using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HanLP.csharp.corpus.document
{
    /// <summary>
    /// 文档，由多个句子（<seealso cref="Sentence"/>）组成
    /// </summary>
    public class Document
    {
        public List<Sentence> Sentences;
        public Document(List<Sentence> sentences) => Sentences = sentences;

        // 句子的正则表达式（从文档中匹配句子）
        // 注意句子的结尾，除了"。！？"，还需要后跟"/w"或者"/w "。或者接换行符，或者美元符号'$'
        private static Regex _pattern = new Regex(@".+?((。/w)|(！/w )|(？/w )|\n|$)", RegexOptions.Compiled);

        public static Document Create(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;

            var matches = _pattern.Matches(input);
            var sentences = new List<Sentence>();
            foreach(Match m in matches)
            {
                var s = Sentence.Create(m.Value);
                if (s == null) return null;

                sentences.Add(s);
            }
            return new Document(sentences);
        }

        /// <summary>
        /// 获取文档中的所有词语
        /// 包含单词或者短语两种形式
        /// </summary>
        /// <returns></returns>
        public List<IWord> GetAllWords()
        {
            var list = new List<IWord>();
            for(int i = 0; i < Sentences.Count; i++)
            {
                var s = Sentences[i];
                list.AddRange(s.Words);
            }
            return list;
        }

        /// <summary>
        /// 获取简单的单词
        /// 对于短语，则将其拆分为一个个单词
        /// </summary>
        /// <returns></returns>
        public List<Word> GetAllSimpleWords()
        {
            var words = new List<Word>();
            foreach(var word in GetAllWords())
            {
                var cw = word as CompoundWord;
                if (cw == null)
                    words.Add((Word)word);
                else
                    words.AddRange(cw.Words);
            }
            return words;
        }

        /// <summary>
        /// 获取文档的所有单词，以二维列表形式表示，第一维表示各句子，第二维表示句子中各单词，短语被拆分为简单单词
        /// </summary>
        /// <returns></returns>
        public List<List<Word>> GetSimpleWordsAsSentences()
        {
            var lists = new List<List<Word>>();
            for(int i = 0; i < Sentences.Count; i++)
            {
                var list = new List<Word>();                // 分别对每个句子，创建一个单词列表存储句子单词
                var words = Sentences[i].Words;
                for (int j = 0; j < words.Count; j++)
                {
                    var word = words[j];
                    var cw = word as CompoundWord;
                    if (cw == null)
                        list.Add((Word)word);
                    else
                        list.AddRange(cw.Words);
                }
                lists.Add(list);
            }
            return lists;
        }

        /// <summary>
        /// 获取文档的所有词语，以二维列表形式表示，第一维表示各句子，第二维表示句子中各单词，短语不被拆分
        /// </summary>
        /// <returns></returns>
        public List<List<IWord>> GetWordsAsSentences()
        {
            var lists = new List<List<IWord>>();
            for(int i = 0; i < Sentences.Count; i++)
            {
                var list = new List<IWord>(Sentences[i].Words);     //! 这里重新创建了一个新的列表，防止原句子的Word列表被修改
                lists.Add(list);
            }
            return lists;
        }

        /// <summary>
        /// 获取文档的所有单词，以二维列表形式表示，第一维表示各句子，第二维表示句子中各单词
        /// </summary>
        /// <param name="collapse">短语是否坍缩为一个单词</param>
        /// <returns></returns>
        public List<List<Word>> GetSimpleWordsAsSentences(bool collapse)
        {
            var lists = new List<List<Word>>();
            for(int i = 0; i < Sentences.Count; i++)
            {
                var list = new List<Word>();
                var words = Sentences[i].Words;
                for(int j = 0; j < words.Count; j++)
                {
                    var word = words[j];
                    var cw = word as CompoundWord;
                    if (cw == null)
                    {
                        list.Add((Word)word);
                    }
                    else if (collapse)
                    {
                        list.Add(cw.ToWord());
                    }
                    else
                        list.AddRange(cw.Words);
                }
                lists.Add(list);
            }
            return lists;
        }

        /// <summary>
        /// 获取文档的所有单词，以二维列表形式表示，第一维表示各句子，第二维表示句子中各单词
        /// </summary>
        /// <param name="labelSet">标签集合，如果短语的标签存在于这个集合中，则短语需要被拆分，否则短语坍缩为一个单词</param>
        /// <returns></returns>
        public List<List<Word>> GetSimpleWordsAsSentences(ISet<string> labelSet)
        {
            var lists = new List<List<Word>>();
            for(int i = 0; i < Sentences.Count; i++)
            {
                var list = new List<Word>();
                var words = Sentences[i].Words;
                for(int j = 0; j < words.Count; j++)
                {
                    var word = words[j];
                    var cw = word as CompoundWord;
                    if (cw == null)
                        words.Add((Word)word);
                    else if (labelSet.Contains(cw.Label))
                        words.AddRange(cw.Words);
                    else
                        words.Add(cw.ToWord());
                }
                lists.Add(list);
            }
            return lists;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            for(int i = 0; i < Sentences.Count; i++)
            {
                if (i != 0) sb.Append(' ');

                sb.Append(Sentences[i]);
            }
            return sb.ToString();
        }
    }
}
