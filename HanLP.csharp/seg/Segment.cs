using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HanLP.csharp.seg.common;
using HanLP.csharp.dictionary.other;
using HanLP.csharp.dictionary;
using HanLP.csharp.corpus.tag;
using HanLP.csharp.utility;
using static HanLP.csharp.Constants;
using WordAttr = HanLP.csharp.corpus.model.Attribute;

namespace HanLP.csharp.seg
{
    /// <summary>
    /// 分词器基类
    /// </summary>
    public abstract class Segment
    {
        /// <summary>
        /// 分词配置
        /// </summary>
        protected SegConfig config = new SegConfig();

        /// <summary>
        /// 原子分词
        /// 仅对数字以及单字节字符连接处理为一个AtomNode，其他每个字符自成一个AtomNode
        /// 考虑了浮点数
        /// </summary>
        /// <param name="chars">待分词字符数组</param>
        /// <param name="start">起始点 inclusive</param>
        /// <param name="end">截止点 exclusive</param>
        /// <returns></returns>
        protected static List<AtomNode> AtomSegment(char[] chars, int start, int end)
        {
            var atoms = new List<AtomNode>();
            var sb = new StringBuilder();

            char c;
            var charTypeArr = new int[end - start];     // 创建用于保存字符类型的数组
            for(int i = 0; i < charTypeArr.Length; i++)
            {
                c = chars[i + start];                   // 当前字符
                charTypeArr[i] = CharType.Get(c);       // 当前字符的类型

                //! 对几种特殊情况作类型调整
                if (c == '.' && i + start < (chars.Length - 1) && CharType.Get(chars[i + start + 1]) == CT_NUM)   // 如果当前是 dot，并且后面跟的字符是数字类型，则标记当前位置类型为数字类型
                    charTypeArr[i] = CT_NUM;
                //else if (c == '.' && i + start < (chars.Length - 1) && chars[i + start + 1] >= '0' && chars[i + start + 1] <= '9')      //todo: 这个条件不是跟上面一样吗？
                //    charTypeArr[i] = CT_SINGLE;
                else if (charTypeArr[i] == CT_LETTER)   // 如果为字母类型，则先统一改成 “单字节” 字符类型
                    charTypeArr[i] = CT_SINGLE;
            }

            int cursor = start;
            int curType, nextType;

            // 对每个字符以及字符类型，构建一个原子节点对象并添加到列表。只有遇到连续数字类型时，将连续的表示数字（integer or float）的字符串作为一个整体原子节点
            while (cursor < end)                 
            {
                curType = charTypeArr[cursor - start];      // 当前字符类型

                if(curType == CT_CHINESE || curType == CT_INDEX || curType == CT_DELIMITER || curType == CT_OTHER)
                {
                    atoms.Add(new AtomNode(chars[cursor], curType));
                    cursor++;
                }
                //! 遇到连续的单字节字符，或者连续的“数字” 类型的字符，则将这些连续的相同类型字符的字符串作为一个整体，构建原子节点对象
                else if(cursor < end - 1 && (curType == CT_SINGLE || curType == CT_NUM))   
                {
                    sb.Clear();
                    sb.Append(chars[cursor]);       // 将当前数字/单字节 字符添加到缓冲区

                    bool used = true;           // 当前指针指向的字符是否被使用
                    while(cursor < end - 1)     // 尚未达到最后一个字符
                    {
                        nextType = charTypeArr[++cursor - start];   // 下一个字符类型，并将指针指向下一个字符
                        if (nextType == curType)
                            sb.Append(chars[cursor]);
                        else
                        {
                            used = false;
                            break;
                        }
                    }
                    atoms.Add(new AtomNode(sb.ToString(), curType));
                    if (used)   // 如果当前指针指向字符 已经被使用，cursor 需要 增 1
                        cursor++;   // 
                }
                else
                {
                    atoms.Add(new AtomNode(chars[cursor], curType));
                    cursor++;
                }
            }
            return atoms;
        }

        /// <summary>
        /// 简易原子分词，所有字符连在一起作为一个AtomNode，其类型为英文字母
        /// </summary>
        /// <param name="chars"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        protected static List<AtomNode> SimpleAtomSegment(char[] chars, int start, int end)
        {
            var atoms = new List<AtomNode>()
            {
                new AtomNode(new string(chars, start, end - start), CT_LETTER)
            };
            return atoms;
        }

        /// <summary>
        /// 快速原子分词，将连续的同类型的字符作为一个AtomNode
        /// 对浮点数做了特殊处理
        /// </summary>
        /// <param name="chars"></param>
        /// <param name="start">起始位置，inclusive</param>
        /// <param name="end">截止位置，exclusive</param>
        /// <returns></returns>
        protected static List<AtomNode> QuickAtomSegment(char[] chars, int start, int end)
        {
            var atoms = new List<AtomNode>();
            var cursor = start;
            int preType = CharType.Get(chars[cursor++]);            // 获取首个字符类型
            int curType;

            while(cursor < end)                                     // 从第二个字符开始，将连续类型的字符组成的字符串作为一个整体，构建一个原子节点对象
            {
                curType = CharType.Get(chars[cursor]);
                if(curType != preType)                              // 如果收到前后相邻字符的类型不一致
                {
                    if(chars[cursor] == '.' && preType == CT_NUM)       // 浮点数识别
                    {
                        cursor++;                                   // 指向 dot 符号的下一个符号
                        while(cursor < end)
                        {
                            curType = CharType.Get(chars[cursor]);  // 获取符号类型
                            if (curType != CT_NUM) break;           // 类型不一致，退出循环
                            cursor++;
                        }
                    }
                    atoms.Add(new AtomNode(new string(chars, start, cursor - start), preType));     // 
                    start = cursor;
                }
                preType = curType;                                  // 更新当前字符的类型
                cursor++;                                           // 更新指针指向下一个字符
            }
            if (cursor == end)  // 还有一种可能就是cursor= end + 1，这种情况时最后末尾为一个浮点数，已经加入列表
                atoms.Add(new AtomNode(new string(chars, start, end - start), preType));
            return atoms;
        }

        /// <summary>
        /// 使用用户词典合并粗分结果，即，最长匹配
        /// </summary>
        /// <param name="vertices">粗分结果</param>
        /// <returns>合并后的结果</returns>
        protected static List<Vertex> CombineByCustomDict(List<Vertex> vertices)
        {
            var arr = vertices.ToArray();

            // 首先使用自定义词条文件加载得到的词典（动态添加的词典比文件词典优先级高，所以后使用动态词典，这样可以覆盖前面的合并结果）
            var dat = CustomDictionary.dat;
            for(int i = 0; i < arr.Length; i++)
            {
                int state = dat.Transition(arr[i].realWord, 1);        // 根据 realWord 路径转移，得到base数组的下标
                if (state > 0)       // 转移成功，表示存在这个词
                {
                    int to = i + 1;     // 以当前位置 i 处的 词条开始，向后获取词条（的属性）
                    int end = to;       // 可以合并的词条的结束位置（exclusive）

                    var attr = dat.GetValueOrDefault(state);    // 获取当前i 位置的词条的属性

                    // 从当前词条的后面开始，依次获取词条和其属性
                    for(; to < arr.Length; to++)
                    {
                        state = dat.Transition(arr[to].realWord, state);    // 接着上一个匹配成功的词条后继续匹配，比如匹配到“勾股”后，继续匹配，万一存在“勾股定理”这个词呢？
                        if (state < 0) break;       // 转移失败，说明没有这样的词条，退出

                        var attrVal = dat.GetValueOrDefault(state);     // 获取当前匹配到的词条的属性
                        if(attrVal != null)     // 词条属性存在
                        {
                            attr = attrVal;     // 更新当前词条的属性，比如前面获取到“勾股”这个词条的属性后，现在可以更新为“勾股定理”这个词条的属性了
                            end = to + 1;
                        }
                    }
                    if(attr != null)        // 匹配到自定义的词，且这个词有属性，则合并
                    {
                        CombineWords(arr, i, end, attr);
                        i = end - 1;    // 更新当前处理的词条位置，为当前合并的一系列连续词条中最后一个词条的位置（下一轮合并则从此词条的下一个词条开始合并）
                    }
                }
            }

            // 再次使用动态词典进行合并
            if(CustomDictionary.binTrie != null)
            {
                var trie = CustomDictionary.binTrie;
                for(int i = 0; i < arr.Length; i++)
                {
                    if (arr[i] == null) continue;

                    var state = trie.Transition(arr[i].realWord, 0);
                    if(state != null)
                    {
                        int to = i + 1;
                        int end = to;
                        var attr = state.Value;
                        for(; to < arr.Length; to++)
                        {
                            if (arr[to] == null) continue;
                            state = state.Transition(arr[to].realWord, 0);
                            if (state == null) break;

                            if(state.Value != null)
                            {
                                attr = state.Value;
                                end = to + 1;
                            }
                        }
                        if(attr != null)
                        {
                            CombineWords(arr, i, end, attr);
                            i = end - 1;
                        }
                    }
                }
            }

            vertices.Clear();
            for(int i = 0; i < arr.Length; i++)
            {
                if (arr[i] != null)
                    vertices.Add(arr[i]);
            }
            return vertices;
        }

        /// <summary>
        /// 使用用户词典合并粗分结果，并将结果收集到全词图中，然后返回合并后的结果
        /// In place modify
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="wordNet"></param>
        /// <returns></returns>
        protected static List<Vertex> CombineByCustomDict(List<Vertex> vertices, WordNet wordNet)
        {
            var list = CombineByCustomDict(vertices);       // 合并，产生 最长匹配
            int line = 0;                                   // 行号

            //! 索引
            for(int i = 0; i < list.Count; i++)
            {
                var vertex = list[i];                       // 当前词条
                var parentLen = vertex.realWord.Length;     // 当前词条的字符串长度
                int currLine = line;                        // 获取当前行号
                if(parentLen >= 3)                          // 长词条，
                {
                    Action<int, int, WordAttr> action = (begin, end, value) =>
                    {
                        if (end - begin == parentLen) return;
                        wordNet.Add(currLine + begin, new Vertex(vertex.realWord.Substring(begin, end), value));
                    };
                    CustomDictionary.Parse(vertex.realWord, action);
                }
                line += parentLen;
            }
            return list;
        }

        /// <summary>
        /// 合并连续的词条形成一个更长的词语并更新到第一个位置，其余原位置上的词条全部重置为null
        /// </summary>
        /// <param name="vertices">词条列表</param>
        /// <param name="start">词条起始位置 inclusive</param>
        /// <param name="end">词条结束位置 exclusive</param>
        /// <param name="attr">目标词语对应的属性</param>
        private static void CombineWords(Vertex[] vertices, int start, int end, WordAttr attr)
        {
            if(start + 1 == end)    // 要合并的词列表中只有一个词
            {
                vertices[start].attr = attr;
            }
            else
            {
                var sb = new StringBuilder();
                for(int i = start; i < end; i++)
                {
                    if (vertices[i] == null) continue;      // 跳过

                    var realWord = vertices[i].realWord;
                    sb.Append(realWord);                    // 合并词语（形成一个更长的词语）
                    vertices[i] = null;
                }
                vertices[start] = new Vertex(sb.ToString(), attr);
            }
        }

        /// <summary>
        /// 合并数量词
        /// </summary>
        /// <param name="list"></param>
        /// <param name="wordNet"></param>
        /// <param name="config"></param>
        protected void CombineNumQuant(List<Vertex> list, WordNet wordNet, SegConfig config)
        {
            if (list.Count < 4) return;     // 除去首尾辅助节点2个，还需要至少2个有效节点才能合并

            var sb = new StringBuilder();
            int line = 1;                   // 初始化为1，跳过起始辅助节点
            for(int i = 0; i < list.Count; i++)
            {
                var pre = list[i];
                if(pre.HasNature(Nature.m))     // 第一个数词出现
                {
                    sb.Append(pre.realWord);
                    Vertex cur = null;
                    i++;
                    while (i < list.Count)      // 遍历数词之后的词
                    {
                        cur = list[i];
                        if (cur.HasNature(Nature.m))        // 连续数词
                        {
                            sb.Append(cur.realWord);
                            list[i] = null;
                            RemoveFromWordNet(cur, wordNet, line, sb.Length);
                            i++;
                        }
                        else
                            break;
                    }
                    if(cur != null)     // 合并连续的数词后，遇到的第一个非数词
                    {
                        if (cur.HasNature(Nature.q) || cur.HasNature(Nature.qv) || cur.HasNature(Nature.qt))     // cur 是量词
                        {
                            if (config.indexMode)           // 如果开启索引（最小）分词模式，则先将连续数词作为一个节点添加进词网
                                wordNet.Add(line, new Vertex(sb.ToString(), new WordAttr(Nature.m)));

                            sb.Append(cur.realWord);        // 合数量词
                            list[i] = null;
                            RemoveFromWordNet(cur, wordNet, line, sb.Length);
                        }
                        else                                // 遇到第一个非数词，也非量词
                            line += cur.realWord.Length;    // 更新行号，表示跳过这个（非数非量）词
                    }

                    if(sb.Length != pre.realWord.Length)    // 长度不等，意味着合并到连续数词或者数量词
                    {
                        foreach (var vertex in wordNet.GetRow(line + pre.realWord.Length))  // 遍历第一个数词之后的行号上的所有节点列表，将这些节点的前驱节点置为空
                            vertex.from = null;
                        pre.realWord = sb.ToString();       // 首个数词修改为连续数词 或者 数量词
                        pre.word = TAG_NUMBER;              // 实际上，本项目将数量词等同数词处理了
                        pre.attr = new WordAttr(Nature.mq);
                        pre.wordId = CoreDictionary.M_WORD_ID;
                        sb.Clear();
                    }
                }

                sb.Clear();
                line += pre.realWord.Length;            // 更新行号，跳过合并后的数量词
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cur"></param>
        /// <param name="wordNet">词网</param>
        /// <param name="line">起始位置的行号，数量词的首字符所在行号</param>
        /// <param name="length">数量词（连续数词）合并后的整体长度</param>
        private static void RemoveFromWordNet(Vertex cur, WordNet wordNet, int line, int length)
        {
            var vertexes = wordNet.Vertices[line + length];         // 紧邻数量词（连续数词）之后的行号
            for(int i = 0; i < vertexes.Count; i++)                 // 遍历这一行上的所有节点
            {   
                if (vertexes[i].from == cur)                        // 如果某个节点的前驱节点是原量词（原数词），现在由于量词或数词被合并进数量词（连续数词），所以将前驱节点置空
                    vertexes[i].from = null;
            }

            var vertexes1 = wordNet.Vertices[line + length - cur.realWord.Length];      // 获取原始量词所在的行号上的所有节点列表
            var removeIdx = -1;
            for(int i = 0; i < vertexes1.Count; i++)                                    //
            {
                if(vertexes1[i] == cur)                                                 // 如果发现原始量词，由于这个量词被合并进入数量词，所以需要移除这个量词
                {
                    removeIdx = i;
                    break;
                }
            }

            if (removeIdx >= 0)                                     // 执行移除操作
                vertexes1.RemoveAt(removeIdx);
        }

        /// <summary>
        /// 可能会多线程分词
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public List<Term> Seg(string text)
        {
            var charArr = text.ToCharArray();
            if (Config.NormalizeChar)
                CharTable.Normallize(charArr);

            if(config.threadNum > 1 && charArr.Length > 10000)      // 对长文本才使用多线程
            {
                var sentences = SentenceUtil.ToSentenceList(charArr);
                var sentenceArr = sentences.ToArray();

                var termListArr = new List<Term>[sentenceArr.Length];
                var per = sentenceArr.Length / config.threadNum;        // 每个线程至少要处理的句子数量

                var threads = new Thread[config.threadNum];
                var lastThreadIdx = config.threadNum - 1;
                var sac = new SegAuxClass(sentenceArr, termListArr);
                for (int i = 0; i < lastThreadIdx; i++)
                {
                    int from = i * per;
                    sac.from = from;
                    sac.to = from + per;
                    threads[i] = new Thread(ThreadSeg);
                    threads[i].Start(sac);
                }
                sac.from = lastThreadIdx * per;
                sac.to = sentenceArr.Length;
                threads[lastThreadIdx] = new Thread(ThreadSeg);
                threads[lastThreadIdx].Start(sac);

                try
                {
                    foreach (var thread in threads)
                        thread.Join();
                }
                catch(ThreadInterruptedException e)
                {
                    // log warning "thread sync exception"
                    return new List<Term>();
                }

                var termList = new List<Term>();
                if(config.offset || config.indexMode)
                {
                    int sentenceOffset = 0;
                    for(int i = 0; i < sentenceArr.Length; i++)
                    {
                        foreach(var term in termListArr[i])
                        {
                            term.offset += sentenceOffset;
                            termList.Add(term);
                        }
                        sentenceOffset += sentenceArr[i].Length;
                    }
                }
                else
                {
                    foreach (var list in termListArr)
                        termList.AddRange(list);
                }
                return termList;
            }

            // 单线程分词
            return SegSentence(charArr);
        }

        /// <summary>
        /// 单线程分词
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public List<Term> Seg(char[] text)
        {
            if (Config.NormalizeChar)
                CharTable.Normallize(text);

            return SegSentence(text);
        }

        /// <summary>
        /// 先将文本分成多个句子，然后对每个句子分词成一个词列表
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public List<List<Term>> Seg2Sentence(string text)
        {
            var termsList = new List<List<Term>>();
            foreach (var sentence in SentenceUtil.ToSentenceList(text))
                termsList.Add(SegSentence(sentence.ToCharArray()));

            return termsList;
        }

        private void ThreadSeg(object obj)
        {
            var sac = (SegAuxClass)obj;

            for(int i = sac.from; i < sac.to; i++)
            {
                sac.termListArr[i] = SegSentence(sac.sentenceArr[i].ToCharArray());
            }
        }

        /// <summary>
        /// 将给定句子分词
        /// </summary>
        /// <param name="sentence"></param>
        /// <returns></returns>
        public abstract List<Term> SegSentence(char[] sentence);

        #region Set SegConfig
        public Segment SetIndexMode(bool enable)
        {
            config.indexMode = enable;
            return this;
        }

        public Segment SetNatureTag(bool enable)
        {
            config.natureTagging = enable;
            return this;
        }
        public Segment SetChsNameRecognize(bool enable)
        {
            config.chsNameRecognize = enable;
            config.UpdateNERConfig();
            return this;
        }

        public Segment SetPlaceRecognize(bool enable)
        {
            config.placeRecognize = enable;
            config.UpdateNERConfig();
            return this;
        }
        public Segment SetOrganizationRecognize(bool enable)
        {
            config.orgRecognize = enable;
            config.UpdateNERConfig();
            return this;
        }

        public Segment SetTranslatedNameRecognize(bool enable)
        {
            config.translatedNameRecognize = enable;
            config.UpdateNERConfig();
            return this;
        }

        public Segment SetJapaneseNameRecognize(bool enable)
        {
            config.jpNameRecognize = enable;
            config.UpdateNERConfig();
            return this;
        }

        public Segment SetCustomDictionary(bool enable)
        {
            config.useCustomDict = enable;
            return this;
        }
        public Segment SetOffset(bool enable)
        {
            config.offset = enable;
            return this;
        }
        public Segment SetNumQuantRecoginize(bool enable)
        {
            config.numQuantRecognize = enable;
            return this;
        }
        public Segment SetAllNameEntityRecognize(bool enable)
        {
            config.chsNameRecognize = enable;
            config.jpNameRecognize = enable;
            config.translatedNameRecognize = enable;
            config.placeRecognize = enable;
            config.orgRecognize = enable;
            config.UpdateNERConfig();
            return this;
        }

        public Segment SetMultiThread(bool enable)
        {
            if (enable) config.threadNum = 4;
            else
                config.threadNum = 1;

            return this;
        }

        public Segment SetMultiThread(int threadNum)
        {
            config.threadNum = threadNum;
            return this;
        }
        #endregion



        /// <summary>
        /// 分词辅助类
        /// </summary>
        class SegAuxClass
        {
            public int from;
            public int to;
            public string[] sentenceArr;
            public List<Term>[] termListArr;

            public SegAuxClass(string[] sentenceArr, List<Term>[] termListArr)
            {
                this.sentenceArr = sentenceArr;
                this.termListArr = termListArr;
                //this.from = from;
                //this.to = to;
            }
        }
    }
}
