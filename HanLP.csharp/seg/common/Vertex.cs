using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using HanLP.csharp.corpus.tag;
using HanLP.csharp.dictionary;
using static HanLP.csharp.Constants;
using WordAttr = HanLP.csharp.corpus.model.Attribute;

namespace HanLP.csharp.seg.common
{
    /// <summary>
    /// 顶点，存储了一个词的所有信息，包括原始字符串值，等效字符串值，在核心词典中存储id，顶点数组中的唯一性id，词属性，权重和前驱顶点等
    /// </summary>
    public class Vertex
    {
        /// <summary>
        /// 节点对应的词或等效词（如 未##数）
        /// </summary>
        public string word;
        /// <summary>
        /// 节点对应的真实词，绝对不含“##”
        /// </summary>
        public string realWord;
        /// <summary>
        /// 等效词的Id，存储在核心词典<seealso cref="CoreDictionary"/>中的值数组的下标
        /// </summary>
        public int wordId;
        /// <summary>
        /// 在一维顶点数组中的下标，可以视作这个顶点的id
        /// </summary>
        public int index;
        /// <summary>
        /// 词的属性
        /// </summary>
        public WordAttr attr;
        /// <summary>
        /// 最短路径对应的权重
        /// </summary>
        public double weight;
        /// <summary>
        /// 到该节点的最短路径的前驱节点
        /// </summary>
        public Vertex from;

        /// <summary>
        /// 始##始
        /// </summary>
        public static readonly Vertex B = new Vertex(TAG_BEGIN, " ", new WordAttr(Nature.begin, MAX_FREQUENCY / 10), CoreDictionary.BEGIN_WORD_ID);
        /// <summary>
        /// 末##末
        /// </summary>
        public static readonly Vertex E = new Vertex(TAG_END, " ", new WordAttr(Nature.end, MAX_FREQUENCY / 10), CoreDictionary.END_WORD_ID);

        public Vertex(string realWord) : this(null, realWord, CoreDictionary.GetAttr(realWord), -1) { }
        public Vertex(char realWord, WordAttr attr) : this(realWord.ToString(), attr) { }
        public Vertex(string realWord, WordAttr attr) : this(null, realWord, attr, -1) { }

        public Vertex(string realWord, WordAttr attr, int wordId) : this(null, realWord, attr, wordId) { }


        public Vertex(string word, string realWord, WordAttr attr, int wordId)
        {
            if (attr == null)
                this.attr = new WordAttr(Nature.n, 1);
            else
                this.attr = attr;

            this.wordId = wordId;
            if (word == null)
                this.word = CompileRealWord(realWord, this.attr);
            else
                this.word = word;

            //Debug.Assert(realWord.Length > 0, "构造空白节点会导致死循环");
            if (realWord.Length <= 0)
                throw new Exception("构造空白节点会导致死循环");
            this.realWord = realWord;
            
        }

        /// <summary>
        /// 将原词转为等效词
        /// </summary>
        /// <param name="realWord">原来的词</param>
        /// <param name="attr">等效词串</param>
        /// <returns></returns>
        private string CompileRealWord(string realWord, WordAttr attr)
        {
            if(attr.natures.Length == 1)
            {
                switch(attr.natures[0])
                {
                    case var x when x >= Nature.nr && x <= Nature.nr2:      // 人名
                        wordId = CoreDictionary.NR_WORD_ID;
                        return TAG_PEOPLE;
                    case Nature.ns:                                         // 地名
                    case Nature.nsf:
                        wordId = CoreDictionary.NS_WORD_ID;
                        return TAG_PLACE;
                    case Nature.nx:                                         // 专有名词
                        wordId = CoreDictionary.NX_WORD_ID;
                        this.attr = CoreDictionary.GetAttr(CoreDictionary.NX_WORD_ID);
                        return TAG_PROPER;
                    case var x when x >= Nature.nt && x <= Nature.nth || x == Nature.nit:
                        wordId = CoreDictionary.NT_WORD_ID;
                        return TAG_GROUP;
                    case Nature.m:
                    case Nature.mq:
                        wordId = CoreDictionary.M_WORD_ID;
                        this.attr = CoreDictionary.GetAttr(CoreDictionary.M_WORD_ID);
                        return TAG_NUMBER;
                    case Nature.x:
                        wordId = CoreDictionary.X_WORD_ID;
                        this.attr = CoreDictionary.GetAttr(CoreDictionary.X_WORD_ID);
                        return TAG_CLUSTER;
                    case Nature.t:
                        wordId = CoreDictionary.T_WORD_ID;
                        this.attr = CoreDictionary.GetAttr(CoreDictionary.T_WORD_ID);
                        return TAG_TIME;
                }
            }
            return realWord;
        }

        public void UpdateMyFrom(Vertex from)
        {
            var weight = from.weight + CalcWeight(from, this);
            if(this.from == null || this.weight > weight)       // 这里weight本应是负的，取了相反数，所以 weight大的反而表示概率小
            {
                // 更新from
                this.from = from;       
                this.weight = weight;
            }
        }

        public static double CalcWeight(Vertex from, Vertex to)
        {
            // from 的频次
            int freq = from.attr.totalFreq;
            if (freq == 0) freq = 1;            // 0 校正

            // 联合频次
            var biGramWordFreq = CoreBiGramTableDict.GetBiFreq(from.wordId, to.wordId);

            // F(AB)/F(A) 
            // 为了防止 biGramWordFreq为0，所以做了处理如下：F(AB)/F(A) = (1 - SMOOTHING_FACTOR) * biGramWordFreq / freq + SMOOTHING_FACTOR
            // 继续用 F(A)/TOTAL 以小一阶的形式（乘以SMOOTHING_PARAM）做平滑处理
            var value = -Math.Log(SMOOTHING_PARAM * freq / MAX_FREQUENCY + (1 - SMOOTHING_PARAM) * ((1 - SMOOTHING_FACTOR) * biGramWordFreq / freq + SMOOTHING_FACTOR));
            if (value < 0)
                value = -value;
            return value;
        }

        /// <summary>
        /// 将词性锁定为指定参数nature
        /// </summary>
        /// <param name="nature">词性</param>
        /// <returns>如果锁定词典在词性列表中，返回true，否则返回false</returns>
        public bool ConfirmNature(Nature nature)
        {
            // 如果只有一个词性且为参数指定词性，则返回true
            if (attr.natures.Length == 0 && attr.natures[0] == nature) return true;

            // 否则，需要重新设置属性，使得词性数组中只有一个锁定的词性
            bool res = true;
            var freq = attr.GetFreq(nature);
            if(freq == 0)
            {
                freq = 1000;
                res = false;
            }
            attr = new WordAttr(nature, freq);
            return res;
        }

        public bool ConfirmNature(Nature nature, bool updateWord)
        {
            if (updateWord)
            {
                switch (nature)
                {
                    case Nature.m:
                        word = TAG_NUMBER;
                        break;
                    case Nature.t:
                        word = TAG_TIME;
                        break;
                    default:
                        break;
                }
            }
            return ConfirmNature(nature);
        }

        /// <summary>
        /// 获取该节点的词性，如果词性尚未确定，则返回none
        /// </summary>
        /// <returns></returns>
        public Nature GetNature()
        {
            if (attr.natures.Length > 0) return attr.natures[0];

            // 如果词性尚未确定（有多个）
            return Nature.none;
        }

        /// <summary>
        /// 猜测词性，返回出现频率最大的那个一个词性
        /// </summary>
        /// <returns></returns>
        public Nature GuessNature() => attr.natures[0];
        public bool HasNature(Nature nature) => attr.GetFreq(nature) > 0;

        public Vertex Copy() => new Vertex(word, realWord, attr, -1);

        /// <summary>
        /// 创建一个数词实例
        /// </summary>
        /// <param name="realWord"></param>
        /// <returns></returns>
        public static Vertex CreateNumInstance(string realWord) => new Vertex(TAG_NUMBER, realWord, new WordAttr(Nature.m, 1000), -1);
        /// <summary>
        /// 创建一个地名实例
        /// </summary>
        /// <param name="realWord"></param>
        /// <returns></returns>
        public static Vertex CreateAddrInstance(string realWord) => new Vertex(TAG_PLACE, realWord, new WordAttr(Nature.ns, 1000), -1);
        /// <summary>
        /// 创建一个标点符号实例
        /// </summary>
        /// <param name="realWord"></param>
        /// <returns></returns>
        public static Vertex CreatePunctuationInstance(string realWord) => new Vertex(null, realWord, new WordAttr(Nature.w, 1000), -1);

        public static Vertex CreatePersonInstance(string realWord) => CreatePersonInstance(realWord, 1000);
        public static Vertex CreateTranslatedPersonInstance(string realWord, int freq) => new Vertex(TAG_PEOPLE, realWord, new WordAttr(Nature.nrf, freq), -1);
        public static Vertex CreateJapanesePersonInstance(string realWord, int freq) => new Vertex(TAG_PEOPLE, realWord, new WordAttr(Nature.nrj, freq), -1);
        public static Vertex CreatePersonInstance(string realWord, int freq) => new Vertex(TAG_PEOPLE, realWord, new WordAttr(Nature.nr, freq), -1);
        public static Vertex CreatePlaceInstance(string realWord, int freq) => new Vertex(TAG_PLACE, realWord, new WordAttr(Nature.ns, freq), -1);
        public static Vertex CreateOrganizationInstance(string realWord, int freq) => new Vertex(TAG_GROUP, realWord, new WordAttr(Nature.nt, freq), -1);
        public static Vertex CreateTimeInstance(string realWord, int freq) => new Vertex(TAG_TIME, realWord, new WordAttr(Nature.t, freq), -1);
        public static Vertex CreateTimeInstance(string realWord) => new Vertex(TAG_TIME, realWord, new WordAttr(Nature.t, 1000), -1);
        /// <summary>
        /// 创建起始辅助节点
        /// </summary>
        /// <returns></returns>
        public static Vertex CreateB() => new Vertex(TAG_BEGIN, " ", new WordAttr(Nature.begin, MAX_FREQUENCY / 10), CoreDictionary.BEGIN_WORD_ID);
        /// <summary>
        /// 创建结束辅助节点
        /// </summary>
        /// <returns></returns>
        public static Vertex CreateE() => new Vertex(TAG_END, " ", new WordAttr(Nature.end, MAX_FREQUENCY / 10), CoreDictionary.END_WORD_ID);

        public override string ToString() => $"realWord: {realWord}";
    }  
}
