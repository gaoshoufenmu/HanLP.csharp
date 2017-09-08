using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HanLP.csharp.model.CRF;
using HanLP.csharp.utility;
using HanLP.csharp.seg.common;
using HanLP.csharp.dictionary.other;
using HanLP.csharp.dictionary;
using HanLP.csharp.corpus.tag;
using HanLP.csharp.collection.trie.bintrie;
using HanLP.csharp.algorithm;
using WordAttr = HanLP.csharp.corpus.model.Attribute;

namespace HanLP.csharp.seg.CRF
{
    public class CRFSegment : CharBasedGenerativeModelSegment
    {
        private CRFModel _crfModel;

        public CRFSegment(CRFSegmentModel crfModel) { this._crfModel = crfModel; }

        public CRFSegment() : this(Config.CRF_Segment_Model_Path) { }

        public CRFSegment(string modelPath)
        {
            _crfModel = GlobalCache.Get(modelPath) as CRFSegmentModel;
            if(_crfModel == null)
            {
                //_crfModel = CRFSegmentModel.Create(modelPath);

                _crfModel = new CRFSegmentModel(new BinTrie<FeatureFunction>());
                _crfModel.Load(modelPath);
                GlobalCache.Put(modelPath, _crfModel);
            }
        }

        public override List<Term> SegSentence(char[] sentence)
        {
            var list = new List<Term>();
            if (sentence.Length == 0) return list;

            var convertedChars = CharTable.Convert(sentence);
            var table = new Table();
            table.v = AtomSeg2Table(convertedChars);
            _crfModel.Tag(table);
            int offset = 0;

            for (int i = 0; i < table.Size; offset += table.v[i][1].Length, i++)
            {
                var line = table.v[i];
                switch(line[2][0])
                {
                    case 'B':
                        int begin = offset;
                        while(table.v[i][2][0] != 'E')          // 寻找结束标签'E'
                        {
                            offset += table.v[i][1].Length;
                            i++;
                            if (i == table.Size) break;     // 达到最后一个字符
                        }
                        // 退出while循环
                        if (i == table.Size)        // 肯定是由while loop的break退出的，offset已经包含了最后一格词的长度
                        {
                            list.Add(new Term(new string(sentence, begin, offset - begin), Nature.none));
                        }
                        else                        // 由while loop正常退出，当前词标注为'E',offset尚未包含这个词的长度
                            list.Add(new Term(new string(sentence, begin, offset - begin + table.v[i][1].Length), Nature.none));

                        break;
                    default:        // 理论来说，只可能是标注为'S'，所以单独成词
                        list.Add(new Term(new string(sentence, offset, table.v[i][1].Length), Nature.none));
                        break;
                }
            }

            if(config.natureTagging)
            {
                var vertices = ToVertexList(list, true);
                Viterbi.Compute(vertices, CoreDictTransfromMatrixDictionary.transformMatrixDictionary);
                for(int i = 0; i < list.Count; i++)
                {
                    var term = list[i];
                    if (term.nature != Nature.none)
                        term.nature = vertices[i + 1].GuessNature();            // vertices[i+1] -> 附加了辅助起始节点
                }
            }
            if(config.useCustomDict)
            {
                var vertices = ToVertexList(list, false);       //? 会不会覆盖上面的词性标注值
                CombineByCustomDict(vertices);
                list = ToTermList(vertices, config.offset);
            }
            return list;
        }

        /// <summary>
        /// 原子分词，连续的数字或者连续的英文空格作为一个整体词，其他单个字符自成一词
        /// 第一列为词性，第二列为原子分词后的词
        /// </summary>
        /// <param name="sentence"></param>
        /// <returns></returns>
        public static string[][] AtomSeg2Table(char[] sentence)
        {
            var table = new string[sentence.Length][];

            // 是否可以table类型从交叉数组改为二维数组
            for(int i = 0; i < sentence.Length; i++)
                table[i] = new string[3];               //! 设置长度为 3，fst 为词性，snd为词本身，thd用于存储使用CRFModel Tag标注后的标签

            int size = 0;                               // 原子分词后的词数量，由于连续的数字被认为是一个整体，所以 size <= sentence.Length
            int lastIdx = sentence.Length - 1;          // 最后的位置下标
            var sb = new StringBuilder();               // 缓存连续的（阿拉伯）数字
            for(int i = 0; i < sentence.Length; i++)
            {
                if(sentence[i] >= '0' && sentence[i] <= '9')
                {
                    sb.Append(sentence[i]);
                    if(i == lastIdx)                    // 如果当前已经是最后一个字符
                    {
                        table[size][0] = "M";           //? 词性？
                        table[size][1] = sb.ToString();
                        ++size;
                        sb.Clear();                     // 缓存结束
                        break;                          // 到达最后一个字符后退出for循环
                    }

                    char c = sentence[++i];             // 到这里，说明当前字符是数字且尚未达到最后一个字符，则需要继续查看下一个字符是否是数字
                    while(c == '.' || c == '%' || (c >= '0' && c <= '9'))       // 满足条件，表示连续的数值
                    {
                        sb.Append(c);
                        if(i == lastIdx)                    // 检测是否是最后一个字符
                        {
                            table[size][0] = "M";           //? 词性？
                            table[size][1] = sb.ToString();
                            ++size;
                            sb.Clear();                     // 缓存结束
                            goto FINISH;                          // 到达最后一个字符后退出for循环
                        }

                        c = sentence[++i];                 // 当前不是最后一个字符，则需要继续向后获取字符，以查看是否是数字
                    }

                    // 当前字符 c 已经不是数字字符了，此时需要处理缓存的连续数字字符串
                    table[size][0] = "M";
                    table[size][1] = sb.ToString();
                    ++size;
                    sb.Clear();
                    --i;                                    // 当前字符c 不是数字字符，进入下一个for循环处理，于是，需要将 i 回退一格
                }
                // 当前字符不是数字字符
                else if(CharUtil.IsEnglishChar(sentence[i]) || sentence[i] == ' ')      // 与数字字符处理类似，连续的英文or空格作为一个整体
                {
                    sb.Append(sentence[i]);
                    if(i == lastIdx)
                    {
                        table[size][0] = "W";
                        table[size][1] = sb.ToString();
                        ++size;
                        sb.Clear();
                        break;
                    }
                    char c = sentence[++i];
                    while(CharUtil.IsEnglishChar(c) || c == ' ')
                    {
                        sb.Append(sentence[i]);
                        if(i == lastIdx)
                        {
                            table[size][0] = "W";
                            table[size][1] = sb.ToString();
                            ++size;
                            sb.Clear();
                            goto FINISH;
                        }
                        c = sentence[++i];
                    }
                    table[size][0] = "W";
                    table[size][1] = sb.ToString();
                    ++size;
                    sb.Clear();
                    i--;
                }
                else
                {
                    table[size][0] = table[size][1] = sentence[i].ToString();
                    size++;
                }
            }
            FINISH:
            if (size < sentence.Length)
                return ResizeArray(table, size);
            return table;
        }

        /// <summary>
        /// 原子分词，连续的数字或连续的英文字母整体作为一格词，其他单个字符自成一词
        /// 没有标记词性
        /// </summary>
        /// <param name="sentence"></param>
        /// <returns></returns>
        public static List<string> AtomSeg(char[] sentence)
        {
            var list = new List<string>(sentence.Length);
            var lastIdx = sentence.Length - 1;                  // 最后一个字符的下标
            var sb = new StringBuilder();                       // 缓存连续的数字，或连续的英文字母

            char c;
            for(int i = 0; i < sentence.Length; i++)
            {
                c = sentence[i];
                if(c >= '0' && c <= '9')
                {
                    sb.Append(c);
                    if(i == lastIdx)
                    {
                        list.Add(sb.ToString());
                        sb.Clear();
                        break;
                    }
                    c = sentence[++i];
                    while(c == '.' || c == '%' || (c >= '0' && c <= '9'))       // 检查是否有连续的数字
                    {
                        sb.Append(c);
                        if(i == lastIdx)
                        {
                            list.Add(sb.ToString());
                            sb.Clear();
                            goto FINISH;
                        }
                        c = sentence[++i];
                    }
                    list.Add(sb.ToString());
                    sb.Clear();
                    i--;                            // 遇到非数字字符，需要回退一格，准备进入下一个for loop
                }
                else if(CharUtil.IsEnglishChar(c))      // 为啥这里不将空格并入英文字符里面呢？马萨卡... 用空格来分隔英文单词？
                {
                    sb.Append(c);
                    if(i == lastIdx)
                    {
                        list.Add(sb.ToString());
                        sb.Clear();
                        break;
                    }
                    c = sentence[++i];
                    while(CharUtil.IsEnglishChar(c))
                    {
                        sb.Append(c);
                        if(i == lastIdx)
                        {
                            list.Add(sb.ToString());
                            sb.Clear();
                            goto FINISH;
                        }
                        c = sentence[++i];
                    }
                    list.Add(sb.ToString());
                    sb.Clear();
                    i--;                        // 遇到非英文字母，需要回退一格，进入下一个for loop
                }
                else
                {
                    list.Add(c.ToString());
                }
            }
            FINISH:
            return list;
        }

        public static Term Vertex2Term(Vertex vertex) => new Term(vertex.realWord, vertex.GuessNature());

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="offsetEnabled">是否标记offset属性</param>
        /// <returns></returns>
        public static List<Term> ToTermList(List<Vertex> vertices, bool offsetEnabled)
        {
            var size = vertices.Count;
            var terms = new List<Term>(size);

            if(offsetEnabled)
            {
                int offset = 0;
                for(int i = 0; i < size; i++)
                {
                    var term = Vertex2Term(vertices[i]);
                    term.offset = offset;
                    offset += term.Length;
                    terms.Add(term);
                }
            }
            else
            {
                for(int i = 0; i < size; i++)
                {
                    terms.Add(Vertex2Term(vertices[i]));
                }
            }
            return terms;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="terms"></param>
        /// <param name="appendStart">是否附加起始辅助节点</param>
        /// <returns></returns>
        public static List<Vertex> ToVertexList(List<Term> terms, bool appendStart)
        {
            var vertices = new List<Vertex>(terms.Count + 1);
            if (appendStart) vertices.Add(Vertex.B);
            for(int i = 0; i < terms.Count; i++)
            {
                var term = terms[i];
                var attr = CoreDictionary.GetAttr(term.word);
                if (attr == null)
                {
                    if (string.IsNullOrWhiteSpace(term.word))
                        attr = new WordAttr(Nature.x);              // 普通字符串
                    else
                        attr = new WordAttr(Nature.nz);             // 其他专名
                }
                else
                    term.nature = attr.natures[0];                  //! 修改原始Term词条的词性

                vertices.Add(new Vertex(term.word, attr));
            }
            return vertices;
        }

        /// <summary>
        /// 截取靠前的Size大小的数组元素
        /// </summary>
        /// <param name="array"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        private static string[][] ResizeArray(string[][] array, int size)
        {
            var nArray = new string[size][];
            Array.Copy(array, 0, nArray, 0, size);
            return nArray;
        }
    }
}
