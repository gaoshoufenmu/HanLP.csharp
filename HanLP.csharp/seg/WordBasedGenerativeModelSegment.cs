/**
 * 1. GenerateWordNet(WordNet wordNet)方法调用示例：
 *  “这是一个操蛋的、无法理解的世界”，假设其中仅有 “一个”、“理解”和“世界” 为核心词汇，则输出为 ->
 *  line 0: 始##始
 *  line 1: 这是
 *  line 2: 
 *  line 3: 一个
 *  line 4: 
 *  line 5: 操蛋的
 *  line 6:
 *  line 7: 
 *  line 8: 、
 *  line 9: 无法
 *  line 10: 
 *  line 11: 理解
 *  line 12: 
 *  line 13: 的
 *  line 14: 世界
 *  line 15: 
 *  line 16: 末##末
 * */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using HanLP.csharp.seg.common;
using HanLP.csharp.utility;
using HanLP.csharp.dictionary;
using HanLP.csharp.corpus.tag;
using HanLP.csharp.algorithm;

namespace HanLP.csharp.seg
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class WordBasedGenerativeModelSegment : Segment
    {
        protected static void GenerateWord(List<Vertex> vertices, WordNet wordNet)
        {
            FixResultByRule(vertices);

            wordNet.AddAll(vertices);
        }

        /// <summary>
        /// 通过规则修正一些结果
        /// </summary>
        /// <param name="list"></param>
        protected static void FixResultByRule(List<Vertex> list)
        {
            //--------------------------------------------------------------------
            //Merge all seperate continue num into one number
            CombineContinuousNum(list);

            //--------------------------------------------------------------------
            //The delimiter "－－"
            ChangeDelimiterPos(list);

            //--------------------------------------------------------------------
            //如果前一个词是数字，当前词以“－”或“-”开始，并且不止这一个字符，
            //那么将此“－”符号从当前词中分离出来。
            //例如 “3 / -4 / 月”需要拆分成“3 / - / 4 / 月”
            SplitMiddleSlashFromDigitalWords(list);

            //--------------------------------------------------------------------
            //1、如果当前词是数字，下一个词是“月、日、时、分、秒、月份”中的一个，则合并,且当前词词性是时间
            //2、如果当前词是可以作为年份的数字，下一个词是“年”，则合并，词性为时间，否则为数字。
            //3、如果最后一个汉字是"点" ，则认为当前数字是时间
            //4、如果当前串最后一个汉字不是"∶·．／"和半角的'.''/'，那么是数
            //5、当前串最后一个汉字是"∶·．／"和半角的'.''/'，且长度大于1，那么去掉最后一个字符。例如"1."
            CheckDateElements(list);
        }

        private static void CombineContinuousNum(List<Vertex> list)
        {
            if (list.Count < 2) return;     // 数量太少，不需要合并

            var vertices = new List<Vertex>(list.Count) { list[0] };

            Vertex previous = null;
            Vertex next = null;
            for(int i = 1; i < list.Count; i++)
            {
                previous = vertices[vertices.Count - 1];
                next = list[i];
                if((TextUtil.IsAllNum(previous.realWord) || TextUtil.IsAllChineseNum(previous.realWord)) 
                    && (TextUtil.IsAllNum(next.realWord) || TextUtil.IsAllChineseNum(next.realWord)))
                {
                    previous = Vertex.CreateNumInstance(previous.realWord + next.realWord);     // 合并
                    vertices[vertices.Count - 1] = previous;
                }
                else
                {
                    vertices.Add(next);
                }
            }

            list.Clear();
            list.AddRange(vertices);
        }

        private static void ChangeDelimiterPos(List<Vertex> list)
        {
            foreach(var v in list)
            {
                if(v.realWord == "- -" || v.realWord == "-" || v.realWord == "—")
                {
                    v.ConfirmNature(Nature.w);
                }
            }
        }

        //====================================================================
        //如果前一个词是数字，当前词以“－”或“-”开始，并且不止这一个字符，
        //那么将此“－”符号从当前词中分离出来。
        //例如 “3-4 / 月”需要拆分成“3 / - / 4 / 月”
        //====================================================================
        private static void SplitMiddleSlashFromDigitalWords(List<Vertex> list)
        {
            if (list.Count < 2) return;     // 节点数量太少，不需要切分

            var vertices = new List<Vertex>(list.Count + 2);
            Vertex current = list[0];
            Vertex next = null;
            for(int i = 1; i < list.Count; i++)
            {
                var splitted = false;
                next = list[i];
                var currNature = current.GetNature();
                if(currNature == Nature.nx && (next.HasNature(Nature.q) || next.HasNature(Nature.n)))
                {
                    var segs = current.realWord.Split(new[] { '-' }, 2);
                    if(segs.Length == 2)
                    {
                        if(TextUtil.IsAllNum(segs[0]) && TextUtil.IsAllNum(segs[0]))
                        {
                            current = current.Copy();
                            current.realWord = segs[0];
                            vertices.Add(current);
                            vertices.Add(Vertex.CreatePunctuationInstance("-"));
                            vertices.Add(Vertex.CreateNumInstance(segs[1]));
                            splitted = true;
                        }
                    }
                }
                if (!splitted)
                    vertices.Add(current);
                current = next;
            }

            vertices.Add(current);

            list.Clear();
            list.AddRange(vertices);
        }

        //====================================================================
        //1、如果当前词是数字，下一个词是“月、日、时、分、秒、月份”中的一个，则合并且当前词词性是时间
        //2、如果当前词是可以作为年份的数字，下一个词是“年”，则合并，词性为时间，否则为数字。
        //3、如果最后一个汉字是"点" ，则认为当前数字是时间
        //4、如果当前串最后一个汉字不是"∶·．／"和半角的'.''/'，那么是数
        //5、当前串最后一个汉字是"∶·．／"和半角的'.''/'，且长度大于1，那么去掉最后一个字符。例如"1."
        //====================================================================
        private static void CheckDateElements(List<Vertex> list)
        {
            if (list.Count < 2) return;

            var vertices = new List<Vertex>(list.Count + 2) { list[0] };
            Vertex previous = list[0];
            Vertex next = null;

            for(int i = 1; i < list.Count; i++)
            {
                next = list[i];
                if(TextUtil.IsAllNum(previous.realWord) || TextUtil.IsAllChineseNum(previous.realWord))     // 上一个词是数词
                {
                    var nextStr = next.realWord;
                    // 如果下一个词是“月日时分秒”等中的一个，则合并上一个词词性为时间
                    if(nextStr.Length == 1 && "月日时分秒".Contains(nextStr) || nextStr.Length == 2 && nextStr == "月份")
                    {
                        // 合并
                        previous = Vertex.CreateTimeInstance(previous.realWord + next.realWord);
                        vertices[vertices.Count - 1] = previous;
                        // next 节点则丢弃
                    }
                    else if(nextStr == "年")     // 下一个词是“年”，则合并为时间
                    {
                        if(TextUtil.IsYear(previous.realWord))      // 上一个词表示年份数字
                        {
                            previous = Vertex.CreateTimeInstance(previous.realWord + next.realWord);
                            vertices[vertices.Count - 1] = previous;
                            // next 节点则丢弃
                        }
                        else
                        {
                            previous.ConfirmNature(Nature.m);       // 确保上一个词是数词                 
                            vertices.Add(next);                     // 补上下一个词
                        }
                    }
                    else
                    {
                        if (previous.realWord.EndsWith("点"))        // 如果下一个词是“点”，上一个词是数词，则认为是时间（这个其实不一定吧？“点钟”才能确保是时间吧）
                            previous.ConfirmNature(Nature.t, true);
                        else
                        {
                            var length = previous.realWord.Length;
                            if (!"∶·．／./".Contains(previous.realWord[length - 1]))
                                previous.ConfirmNature(Nature.m, true);
                            else if(previous.realWord.Length > 1)   // 当前串最后一个汉字是"∶·．／"和半角的'.''/'，且长度大于1，那么去掉最后一个字符。例如"1."
                            {
                                previous = Vertex.CreateNumInstance(previous.realWord.Substring(0, length - 1));
                                vertices[vertices.Count - 1] = previous;
                                vertices.Add(Vertex.CreatePunctuationInstance(previous.realWord[length - 1].ToString()));
                            }

                            vertices.Add(next);                     // 补上下一个词
                        }
                    }
                }
                previous = next;
            }

            list.Clear();
            list.AddRange(vertices);
        }

        /// <summary>
        /// 将一条路径转为最终结果
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="offsetEnabled"></param>
        /// <returns></returns>
        protected static List<Term> Convert(List<Vertex> vertices, bool offsetEnabled)
        {
            Debug.Assert(vertices != null && vertices.Count >= 2, "路径不应该短于2");

            var len = vertices.Count - 2;       // 有效路径长度（去掉首尾节点）
            var list = new List<Term>();

            if(offsetEnabled)
            {
                int offset = 0;
                for(int i = 1; i <= len; i++)       // 跳过首节点
                {
                    var term = Convert(vertices[i]);
                    term.offset = offset;
                    offset += term.Length;
                    list.Add(term);
                }
            }
            else
            {
                for(int i = 1; i <= len; i++)
                {
                    list.Add(Convert(vertices[i]));
                }
            }
            return list;
        }

        protected static List<Term> Convert(List<Vertex> vertices) => Convert(vertices, false);

        private static Term Convert(Vertex vertex) => new Term(vertex.realWord, vertex.GuessNature());

        /// <summary>
        /// 生成二元词图
        /// </summary>
        /// <param name="wordNet"></param>
        /// <returns></returns>
        protected static Graph ToBiGraph(WordNet wordNet) => wordNet.ToGraph();

        /// <summary>
        /// 生成一元词网
        /// </summary>
        /// <param name="wordNet">初创词网对象</param>
        protected void GenerateWordNet(WordNet wordNet)
        {
            var chars = wordNet.charArr;        // 原始句子的字符数组

            var searcher = CoreDictionary._trie.GetSearcher(chars, 0);      // 获取核心词典词语搜索器：搜索核心词典中的词条
            while(searcher.Next())
            {
                // searcher.begin + 1 -> 由于存在起始辅助节点，所以每个节点的索引向后偏移 1 。
                wordNet.Add(searcher.begin + 1, new Vertex(new string(chars, searcher.begin, searcher.length), searcher.value, searcher.index));
            }

            var vertices = wordNet.Vertices;

            // 上一步中，仅仅是根据核心词典中的词条来划分原始句子，所以还需要对句子中除了核心词汇之外的部分进行分词
            // 比如“这是一个操蛋的世界”，假设核心词汇为“一个”、“世界”，那显然剩余部分——“这是”、“操蛋的”——还需要进行分词处理
            // 然而，我们这里先使用快速原子分词，后面再进一步使用专用分词器分词
            for(int i = 1; i < vertices.Length;)
            {
                if (vertices[i].Count == 0)     // 如果当前行没有顶点
                {
                    int j = i + 1;              // 往后遍历，找到下一个存在节点的行
                    for (; j < vertices.Length - 1; j++)
                    {
                        if (vertices[j].Count != 0) break;
                    }
                    wordNet.Add(i, QuickAtomSegment(chars, i - 1, j - 1));      // i-1,j-1，这里因为有起始辅助节点，导致下标 i,j 均向后偏移一位，所以需要减去 1。
                    i = j;                      // 更新当前位置到下一个有节点的行号位置
                }
                else
                    i += vertices[i].Last().realWord.Length;
            }
        }

        /// <summary>
        /// 为了索引模式而修饰结果
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="wordNet"></param>
        /// <returns></returns>
        protected static List<Term> DecorateResult4IndexMode(List<Vertex> vertices, WordNet wordNet)
        {
            var list = new List<Term>();
            int line = 1;
            var len = vertices.Count - 2;       // 去掉首尾节点
            var buffer = new Queue<Vertex>();
            Vertex vertex = null;
            int count = 0;
            for(int i = 1; i <= len; i++)       // 跳过首节点
            {
                if (buffer.Count > 0)
                    vertex = buffer.Dequeue();
                else
                    vertex = vertices[count++];   // 当前节点

                var termMain = Convert(vertex);
                list.Add(termMain);
                termMain.offset = line - 1;
                if(vertex.realWord.Length > 2)
                {
                    var currLine = line;
                    while(currLine < line + vertex.realWord.Length)
                    {
                        var vertexes = wordNet.Vertices[line];
                        // 需要倒序
                        for(int j = vertexes.Count - 1; j >= 0; j--)
                        {
                            var smallVertex = vertexes[j];      // 这一行的某个短词
                            if(
                                (termMain.nature == Nature.mq && smallVertex.HasNature(Nature.q) || smallVertex.realWord.Length > 1)
                                && smallVertex != vertex        // 防止重复添加
                                && currLine + smallVertex.realWord.Length <= line + vertex.realWord.Length  // 防止超出边界
                            )
                            {
                                buffer.Enqueue(smallVertex);
                                var termSub = Convert(smallVertex);
                                termSub.offset = currLine - 1;
                                list.Add(termSub);
                            }
                        }
                        ++currLine;
                    }
                }
                line += vertex.realWord.Length;
            }
            return list;
        }

        /// <summary>
        /// 词性标注
        /// </summary>
        /// <param name="list"></param>
        public static void SpeechTaggint(List<Vertex> list) => Viterbi.Compute(list, CoreDictTransfromMatrixDictionary.transformMatrixDictionary);
    }
}
