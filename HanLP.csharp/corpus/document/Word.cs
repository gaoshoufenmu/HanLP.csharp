using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanLP.csharp.corpus.document
{
    public interface IWord
    {
        string Value { get; set; }
        string Label { get; set; }
    }

    /// <summary>
    /// 单词
    /// </summary>
    public class Word : IWord
    {
        /// <summary>
        /// 单词的真实值
        /// </summary>
        private string _value;
        /// <summary>
        /// 单词的词性标签，比如“n”
        /// </summary>
        private string _label;
        public string Value
        {
            get => _value;
            set => _value = value;
        }
        public string Label
        {
            get => _label;
            set => _label = value;
        }

        public Word(string value, string label)
        {
            _value = value;
            _label = label;
        }

        public override string ToString() => _value + "/" + _label;

        public static Word Create(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;

            int cutIndex = input.LastIndexOf('/');
            if (cutIndex <= 0 || cutIndex == input.Length - 1) return null;     // 分隔符位于边缘


            return new Word(input.Substring(0, cutIndex), input.Substring(cutIndex + 1));
        }
    }

    /// <summary>
    /// 短语，复合词，由多个单词组成
    /// </summary>
    public class CompoundWord : IWord
    {
        /// <summary>
        /// 复合词包含的多个词列表
        /// </summary>
        public List<Word> Words;

        private string _label;
        public string Label
        {
            get => _label;
            set => _label = value;
        }
        private string _value;
        public string Value
        {
            get
            {
                if(_value == null)
                {
                    var sb = new StringBuilder();
                    for (int i = 0; i < Words.Count; i++)
                        sb.Append(Words[i].Value);
                    _value = sb.ToString();
                }
                return _value;
            }
            set
            {
                Words.Clear();
                Words.Add(new Word(value, _label));
                _value = value;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder(Words.Count * 4);
            sb.Append('[');
            for(int i = 0; i < Words.Count; i++)
            {
                if (i == 0)
                    sb.Append(Words[i].ToString());
                else
                    sb.Append(' ').Append(Words[i].ToString());
            }
            sb.Append("]/").Append(_label);
            return sb.ToString();
        }

        public Word ToWord() => new Word(Value, Label);

        public CompoundWord(List<Word> words, string label)
        {
            this.Words = words;
            this._label = label;
        }

        public static CompoundWord Create(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;

            int cutIndex = input.LastIndexOf('/');
            // 最后一个'/'之前的部分用于构造Word列表，至少要构造一个Word对象，而构造Word，需要使用'/'来分隔成两部分，而首字符为'['，
            if (cutIndex <= 2 || cutIndex == input.Length - 1) return null;

            var words = new List<Word>();
            var wordParam = input.Substring(1, cutIndex - 1);       // 跳过首字符'['
            //? 约定首字符为"["的且不以"[/"开头的输入字符串用于构造复合词对象，否则用于构造单词对象。是否能构造成功还需检查是否满足指定类型词的条件
            foreach (var seg in wordParam.Split(' '))       // Word 之间使用' '分隔
            {
                if (seg.Length == 0) continue;
                var word = Word.Create(seg);
                if (word == null) return null;

                words.Add(word);
            }
            var labelParam = input.Substring(cutIndex + 1);
            return new CompoundWord(words, labelParam);
        }
    }
    public class WordFactory
    {
        public static IWord Create(string input)
        {
            if (input == null) return null;
            if (input.StartsWith("[") && !input.StartsWith("[/"))
                return CompoundWord.Create(input);

            return Word.Create(input);
        }
    }
}
