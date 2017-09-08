/**
 * 构造词网对象，假设输入字符串长度为 n，每个字符对应一个节点，首尾各加一个辅助节点，于是长度变为 (n+2)，每个节点处将对应一个列表，表示一个上下文相关的子串
 * 
 * 
 * 
 * 
 * */

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
    /// 词网
    /// </summary>
    public class WordNet
    {
        /// <summary>
        /// 节点，每一行都是前缀词，跟图的表示方式不同
        /// </summary>
        private List<Vertex>[] _vertices;
        /// <summary>
        /// 共有多少个节点
        /// </summary>
        private int _size;
        /// <summary>
        /// 原始句子
        /// </summary>
        [Obsolete]
        public string sentence;
        /// <summary>
        /// 原始句子对应的数组
        /// </summary>
        public char[] charArr;

        public List<Vertex>[] Vertices => _vertices;

        /// <summary>
        /// 句子中实际分词后的节点数，包含首尾节点
        /// 一个词条表示一个节点
        /// </summary>
        public int Size => _size;

        /// <summary>
        /// 根据给定字符数组表示的句子创建词网对象
        /// </summary>
        /// <param name="charArr"></param>
        public WordNet(char[] charArr)
        {
            this.charArr = charArr;
            _vertices = new List<Vertex>[charArr.Length + 2];   // 句子首尾各增加一个节点
            for (int i = 0; i < _vertices.Length; i++)
                _vertices[i] = new List<Vertex>();

            _vertices[0].Add(Vertex.CreateB());
            _vertices[_vertices.Length - 1].Add(Vertex.CreateE());
            _size = 2;      // 当前仅有首尾节点
        }

        public WordNet(string sentence) : this(sentence.ToCharArray()) { }

        /// <summary>
        /// 根据给定字符数组以及对应的顶点列表创建词网
        /// </summary>
        /// <param name="chars"></param>
        /// <param name="vertexes"></param>
        public WordNet(char[] chars, List<Vertex> vertexes)
        {
            this.charArr = chars;
            _vertices = new List<Vertex>[chars.Length + 2];

            for (int j = 0; j < _vertices.Length; j++)
                _vertices[j] = new List<Vertex>();

            int i = 0;
            // 将列表中的各个顶点插入到其所表示词的首字符在原句子中的位置
            // 比如句子，“这个操蛋的世界”，对应顶点列表所表示的词分别为“这个”、“操蛋”、“的”和“世界”，顶点依次插入位置（行号）为：1, 3, 5, 6
            // 首尾各有一个辅助节点，其realWord = " "，首辅助节点插入行号为 0，
            foreach (var v in vertexes)      
            {
                _vertices[i].Add(v);
                _size++;
                i += v.realWord.Length;
            }
        }

        /// <summary>
        /// 增加节点
        /// </summary>
        /// <param name="line">行号</param>
        /// <param name="vertex">节点</param>
        public void Add(int line, Vertex vertex)
        {
            foreach(var v in _vertices[line])
            {
                // 保证唯一性
                if (v.realWord.Length == vertex.realWord.Length) return;
            }
            _vertices[line].Add(vertex);
            _size++;
        }

        /// <summary>
        /// 强行增加，替换已存在节点
        /// </summary>
        /// <param name="line"></param>
        /// <param name="vertex"></param>
        public void Push(int line, Vertex vertex)
        {
            var list = _vertices[line];
            bool replaced = false;
            for(int i = 0; i < list.Count; i++)
            {
                if(list[i].realWord.Length == vertex.realWord.Length)
                {
                    list[i] = vertex;
                    replaced = true;
                    break;
                }
            }
            if(!replaced)
            {
                list.Add(vertex);
                _size++;
            }
        }

        /// <summary>
        /// 添加节点列表
        /// </summary>
        /// <param name="vertexes"></param>
        public void AddAll(List<Vertex> vertexes)
        {
            int i = 0; 
            foreach(var v in vertexes)
            {
                Add(i, v);
                i += v.realWord.Length;
            }
        }

        /// <summary>
        /// 获取指定行上的节点列表
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public List<Vertex> GetRow(int line) => _vertices[line];

        /// <summary>
        /// 获取指定行的第一个节点
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public Vertex GetFirstCell(int line) => _vertices[line].FirstOrDefault();

        /// <summary>
        /// 获取指定行上长度为指定值的节点
        /// </summary>
        /// <param name="line"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public Vertex Get(int line, int len)
        {
            foreach(var v in _vertices[line])
            {
                if (v.realWord.Length == len)
                    return v;
            }
            return null;
        }

        public void Insert(int line, Vertex vertex, WordNet wordNet)
        {
            foreach (var oldVertex in _vertices[line])                          // 首先检测这个行号上面是否有相同的词存在（同起始位置同长度的词就是相同词）
                if (oldVertex.realWord.Length == vertex.realWord.Length)
                    return;

            _vertices[line].Add(vertex);            // 添加顶点
            _size++;

            // 保证连接
            for(int i = line - 1; i > 1; i--)
            {
                if (Get(i, 1) == null)
                {
                    var fst = wordNet.GetFirstCell(i);      // 使用参数WordNet的相应节点来填充
                    if (fst == null) break;

                    _vertices[i].Add(fst);
                    _size++;
                    if (_vertices[i].Count > 1) break;      // i 行上原来就有节点，能保证连接性，所以这里可以直接退出循环
                }
                else
                    break;
            }

            // 保证这个词语直达
            int l = line + vertex.realWord.Length;          // 当前节点到达的行号
            if(GetRow(l).Count == 0)                        // 到达的行上没有节点，则需要使用WordNet的相应行上所有节点来填充
            {
                var list = wordNet.GetRow(l);
                if (list == null || list.Count == 0) return;    // 没法补救

                _vertices[l].AddRange(list);
                _size += list.Count;
            }

            // 直达之后一直往后
            for(++l; l < _vertices.Length; l++)     //
            {
                if (GetRow(l).Count == 0)
                {
                    var fst = wordNet.GetFirstCell(l);
                    if (fst == null) break;

                    _vertices[l].Add(fst);
                    _size++;
                    if (_vertices[l].Count > 1) break;
                }
                else
                    break;
            }
        }

        /// <summary>
        /// 将原子分词的节点列表添加进来
        /// </summary>
        /// <param name="line">起始行号</param>
        /// <param name="atomSegment"></param>
        public void Add(int line, List<AtomNode> atomSegment)
        {
            int offset = 0;     // 偏移值，随着原子节点的迭代而更新
            foreach(var atom in atomSegment)
            {
                var word = atom.word;
                var nature = Nature.n;
                int id = -1;
                switch(atom.pos)    // 根据字符类型确定词性
                {
                    case CT_CHINESE:
                        break;
                    case CT_INDEX:
                    case CT_NUM:
                        nature = Nature.m;
                        word = "未##数";
                        id = CoreDictionary.M_WORD_ID;
                        break;
                    case CT_DELIMITER:
                    case CT_OTHER:
                        nature = Nature.w;
                        break;
                    case CT_SINGLE:
                        nature = Nature.nx;
                        word = "未##串";
                        id = CoreDictionary.X_WORD_ID;
                        break;
                    default:
                        break;
                }
                Add(line + offset, new Vertex(word, atom.word, new WordAttr(nature, 10000), id));
                offset += atom.word.Length;
            }
        }

        /// <summary>
        /// 获取所有顶点的数组形式
        /// </summary>
        /// <returns></returns>
        private Vertex[] GetVertexArr()
        {
            var arr = new Vertex[_size];
            int c = 0;
            foreach(var list in _vertices)
            {
                for(int i = 0; i < list.Count; i++)
                {
                    list[i].index = c;
                    arr[c++] = list[i];
                }
            }
            return arr;
        }

        /// <summary>
        /// 将词网转换为词图
        /// </summary>
        /// <returns></returns>
        public Graph ToGraph()
        {
            var graph = new Graph(GetVertexArr());

            for(int row = 0; row < _vertices.Length - 1; ++row)
            {
                var froms = _vertices[row];     // 获取某一行上节点列表
                foreach(var from in froms)
                {
                    Debug.Assert(from.realWord.Length > 0, "空节点会导致死循环");
                    int toIndex = row + from.realWord.Length;
                    foreach (var to in _vertices[toIndex])
                        graph.Connect(from.index, to.index, Vertex.CalcWeight(from, to));
                }
            }
            return graph;
        }

        /// <summary>
        /// 合并连续的NS节点
        /// </summary>
        public void CombineContinuousNS()
        {
            for(int row = 0; row < _vertices.Length - 1; row++)
            {
                var froms = _vertices[row];     // 获取某一行上的节点列表
                for(int i = 0; i < froms.Count; i++)
                {
                    var from = froms[i];
                    if(from.GetNature() == Nature.ns)
                    {
                        var toIndex = row + from.realWord.Length;
                        var tos = _vertices[toIndex];
                        for(int j = 0; j < tos.Count; j++)
                        {
                            var to = tos[j];
                            if(to.GetNature() == Nature.ns)
                            {
                                froms[i] = Vertex.CreateAddrInstance(from.realWord + to.realWord);
                                break;
                            }
                        }
                    }
                }
            }
        }

        public void Clear()
        {
            for (int i = 0; i < _vertices.Length; i++)
                _vertices[i].Clear();

            _size = 0;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            for(int i = 0; i < _vertices.Length; i++)
            {
                sb.Append(i).Append(" line vertex count:").Append(_vertices[i].Count).Append(',');
            }
            return sb.ToString();
        }
    }
}
