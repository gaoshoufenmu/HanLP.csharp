using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HanLP.csharp.seg.common;
using HanLP.csharp.corpus.tag;
using HanLP.csharp.corpus.dependency;

namespace HanLP.csharp.dependency
{
    /// <summary>
    /// 边
    /// </summary>
    public class Edge
    {
        public int from;
        public int to;
        public float cost;
        public string label;

        public Edge(int from, int to, float cost, string label)
        {
            this.from = from;
            this.to = to;
            this.cost = cost;
            this.label = label;
        }
    }

    public class Node
    {
        public static readonly Node NULL = new Node(new Term(CoNLLWord.NULL.NAME, Nature.n), -1) { label = "null" };

        public string word;
        public string compiledWord;
        public string label;
        public int id;

        public Node(Term term, int id)
        {
            this.id = id;
            switch (term.nature)
            {

                case Nature.bg:
                    label = "b";
                    break;
                case Nature.mg:
                    label = "Mg";
                    break;
                case Nature.nx:
                    label = "x";
                    break;
                case Nature.qg:
                    label = "q";
                    break;
                case Nature.ud:
                    label = "u";
                    break;
                case Nature.uj:
                    label = "u";
                    break;
                case Nature.uz:
                    label = "uzhe";
                    break;
                case Nature.ug:
                    label = "uguo";
                    break;
                case Nature.ul:
                    label = "ulian";
                    break;
                case Nature.uv:
                    label = "u";
                    break;
                case Nature.yg:
                    label = "y";
                    break;
                case Nature.zg:
                    label = "z";
                    break;
                case Nature.ntc:
                case Nature.ntcf:
                case Nature.ntcb:
                case Nature.ntch:
                case Nature.nto:
                case Nature.ntu:
                case Nature.nts:
                case Nature.nth:
                    label = "nt";
                    break;
                case Nature.nh:
                case Nature.nhm:
                case Nature.nhd:
                    label = "nz";
                    break;
                case Nature.nn:
                    label = "n";
                    break;
                case Nature.nnt:
                    label = "n";
                    break;
                case Nature.nnd:
                    label = "n";
                    break;
                case Nature.nf:
                    label = "n";
                    break;
                case Nature.ni:
                case Nature.nit:
                case Nature.nic:
                    label = "nt";
                    break;
                case Nature.nis:
                    label = "n";
                    break;
                case Nature.nm:
                    label = "n";
                    break;
                case Nature.nmc:
                    label = "nz";
                    break;
                case Nature.nb:
                    label = "nz";
                    break;
                case Nature.nba:
                    label = "nz";
                    break;
                case Nature.nbc:
                case Nature.nbp:
                case Nature.nz:
                    label = "nz";
                    break;
                case Nature.g:
                    label = "nz";
                    break;
                case Nature.gm:
                case Nature.gp:
                case Nature.gc:
                case Nature.gb:
                case Nature.gbc:
                case Nature.gg:
                case Nature.gi:
                    label = "nz";
                    break;
                case Nature.j:
                    label = "nz";
                    break;
                case Nature.i:
                    label = "nz";
                    break;
                case Nature.l:
                    label = "nz";
                    break;
                case Nature.rg:
                case Nature.Rg:
                    label = "Rg";
                    break;
                case Nature.udh:
                    label = "u";
                    break;
                case Nature.e:
                    label = "y";
                    break;
                case Nature.xx:
                    label = "x";
                    break;
                case Nature.xu:
                    label = "x";
                    break;
                case Nature.w:
                case Nature.wkz:
                case Nature.wky:
                case Nature.wyz:
                case Nature.wyy:
                case Nature.wj:
                case Nature.ww:
                case Nature.wt:
                case Nature.wd:
                case Nature.wf:
                case Nature.wn:
                case Nature.wm:
                case Nature.ws:
                case Nature.wp:
                case Nature.wb:
                case Nature.wh:
                    label = "x";
                    break;
                case Nature.begin:
                    label = "root";
                    break;
                default:
                    label = term.nature.ToString();
                    break;
            }
            word = term.word;
            compiledWord = CoNLLUtil.Compile(label, word);     // 根据label(tag)的值，选择 TAG_PEOPLE|TAG_PLACE|TAG_GROUP|... 等代替词，如没有对应代替词，则使用word值
        }

        public override string ToString() => $"{word}/{label}";
    }
    public class State : IComparable<State>
    {
        public float cost;
        public int id;
        public Edge edge;

        public State(float cost, int id, Edge edge)
        {
            this.cost = cost;
            this.id = id;
            this.edge = edge;
        }

        public int CompareTo(State other)
        {
            if (cost == other.cost) return 0;
            if (cost < other.cost) return -1;
            return 1;
        }
    }
}
