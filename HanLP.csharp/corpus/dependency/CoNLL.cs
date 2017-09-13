using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanLP.csharp.corpus.dependency
{
    public class CoNLLFixer
    {
    }

    /// <summary>
    /// CoNLL语料中的一行
    /// </summary>
    public class CoNllLine
    {
        /// <summary>
        /// 十个值
        /// </summary>
        public string[] value = new string[10];

        public int id;

        public CoNllLine(params string[] args)
        {
            var len = Math.Min(args.Length, value.Length);
            for (int i = 0; i < len; i++)
                value[i] = args[i];

            id = int.Parse(value[0]);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            for(int i = 0; i < 10; i++)
            {
                if (i != 0)
                    sb.Append(' ');
                sb.Append(value[i]);
            }
            return sb.ToString();
        }
    }

    public class CoNLLWord
    {
        /// <summary>
        /// 当前词在句子中的序号
        /// </summary>
        public int ID;
        /// <summary>
        /// 当前词语（或标点）的原型或词干，在中文范畴下，其与FROM相同
        /// </summary>
        public string LEMMA;
        /// <summary>
        /// 当前词语的词性（粗粒度的）
        /// </summary>
        public string CPOSTAG;
        /// <summary>
        /// 当前词语的词性（细粒度）
        /// </summary>
        public string POSTAG;
        /// <summary>
        /// 当前词语的中心词
        /// </summary>
        public CoNLLWord HEAD;
        /// <summary>
        /// 当前词语与中心词的依存关系
        /// depencency-relation
        /// </summary>
        public string DEPREL;
        /// <summary>
        /// 等效字符串
        /// </summary>
        public string NAME;

        public static readonly CoNLLWord ROOT = new CoNLLWord(0, "##核心##", "ROOT", "root");

        public static readonly CoNLLWord NULL = new CoNLLWord(-1, "##空白##", "NULL", "null");

        public CoNLLWord(int id, string lemma, string postag) : this(id, lemma, postag[0].ToString(), postag) { }

        public CoNLLWord(int id, string lemma, string cpostag, string postag)
        {
            ID = id;
            LEMMA = lemma;
            CPOSTAG = cpostag;
            POSTAG = postag;
            NAME = CoNLLUtil.Compile(postag, lemma);
        }

        public CoNLLWord(CoNllLine line): this(line.id, line.value[2], line.value[3], line.value[4])
        {
            DEPREL = line.value[7];
        }

        public override string ToString() => $"{ID} {LEMMA} {POSTAG} {HEAD.ID}";
    }

    public class CoNLLSentence
    {
        public CoNLLWord[] word;

        /// <summary>
        /// 构造一个句子
        /// </summary>
        /// <param name="lines">行列表，每行存储一个Word的相关信息</param>
        public CoNLLSentence(List<CoNllLine> lines)
        {
            var size = lines.Count;
            word = new CoNLLWord[size];
            for (int i = 0; i < lines.Count; i++)
            {
                word[i] = new CoNLLWord(lines[i]);
                var head = int.Parse(lines[word[i].ID - 1].value[6]) - 1;
                if (head != -1)
                    word[i].HEAD = word[head];
                else
                    word[i].HEAD = CoNLLWord.ROOT;
            }
        }

        public CoNLLSentence(CoNLLWord[] word) { this.word = word; }

        public override string ToString()
        {
            var sb = new StringBuilder(word.Length * 50);
            for(int i = 0; i < word.Length; i++)
            {
                if (i != 0)
                    sb.Append(" / ");
                sb.Append(word[i]);
            }
            return base.ToString();
        }

        /// <summary>
        /// 获取边的列表，边 edges[i,j]表示id为i的词语与id为j的词语存在一条依存关系，否则为null
        /// </summary>
        /// <returns></returns>
        public string[,] GetEdgeArr()
        {
            var size = word.Length + 1;                 // 加1 是因为有一个辅助虚根节点ROOT
            var edges = new string[size, size];
            for (int i = 0; i < word.Length; i++)
                edges[word[i].ID, word[i].HEAD.ID] = word[i].DEPREL;
            return edges;
        }

        /// <summary>
        /// 获取包含根节点在内的单词数组
        /// </summary>
        /// <returns></returns>
        public CoNLLWord[] GetWordArrWithRoot()
        {
            var words = new CoNLLWord[word.Length + 1];
            words[0] = CoNLLWord.ROOT;
            Array.Copy(word, 0, words, 1, word.Length);
            return words;
        }


    }

}
