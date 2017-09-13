using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HanLP.csharp.collection.trie;
using HanLP.csharp.collection.trie.bintrie;
using HanLP.csharp.corpus.io;

namespace HanLP.csharp.model.CRF
{
    public class CRFDependencyModel : CRFModel
    {
        public CRFDependencyModel(ITrie<FeatureFunction> ffTrie) : base(ffTrie) { }

        /// <summary>
        /// 注意与id2tag的区别
        /// </summary>
        public DTag[] id2dtag;

        public override bool LoadFromBin(ByteArray ba)
        {
            if (!base.LoadFromBin(ba)) return false;
            Init();
            return true;
        }

        protected override void OnLoadTxtFinished()
        {
            base.OnLoadTxtFinished();
            Init();
        }

        private void Init()
        {
            id2dtag = new DTag[id2tag.Length];
            for (int i = 0; i < id2tag.Length; i++)
                id2dtag[i] = new DTag(id2tag[i]);
        }

        public bool IsLegal(int tagId, int current, Table table)
        {
            var dtag = id2dtag[tagId];
            if ("ROOT" == dtag.pos)                 //? 如果是虚根root节点？
            {
                for (int i = 0; i < current; i++)
                    if (table.v[i][3].EndsWith("ROOT")) return false;

                return true;
            }
            else
            {
                int posCount = 0;
                if(dtag.offset > 0)
                {
                    for(int i = current + 1; i < table.Size; i++)
                    {
                        if (table.v[i][1] == dtag.pos) posCount++;
                        if (posCount == dtag.offset) return true;
                    }
                    return false;
                }
                else
                {
                    for(int i = current - 1; i >= 0; i--)
                    {
                        if (table.v[i][1] == dtag.pos) posCount++;
                        if (posCount == -dtag.offset) return true;
                    }
                    return false;
                }
            }
        }

        /// <summary>
        /// 重写Tag函数，因为这里不是所有的标签都是合法的，比如最后一个词的标签不可能是+nPos，必须是负数，
        /// 而且任何词的[+/-]nPos都得保证后面（或前面，当符号为负的时候）有n个词语的标签是Pos
        /// </summary>
        /// <param name="table"></param>
        public override void Tag(Table table)
        {
            var size = table.Size;
            double bestScore = double.MinValue;
            int bestTag = 0;
            int tagSize = id2tag.Length;
            var scores = CalcScores(table, 0);      // 0位置命中的特征函数

            for(int i = 0; i < tagSize; i++)
            {
                for(int j = 0; j < tagSize; j++)
                {
                    if (!IsLegal(j, 0, table)) continue;
                    double score = CalcScore(scores, j);
                    if (_matrix != null)
                        score = +_matrix[i][j];
                    
                    if(score > bestScore)
                    {
                        bestScore = score;
                        bestTag = j;
                    }
                }
            }
            table.SetLast(0, id2tag[bestTag]);

            int preTag = bestTag;
            for(int i = 1; i < size; i++)           // 跳过起始虚根节点 ROOT
            {
                scores = CalcScores(table, i);
                bestScore = double.MinValue;
                for(int j = 0; j < tagSize; j++)
                {
                    if (!IsLegal(j, i, table)) continue;

                    double score = CalcScore(scores, j);
                    if (_matrix != null)
                        score += _matrix[preTag][j];
                    if(score > bestScore)
                    {
                        bestScore = score;
                        bestTag = j;
                    }
                }
                table.SetLast(i, id2tag[bestTag]);
                preTag = bestTag;
            }
        }
    }

    public class DTag
    {
        public int offset;      // 偏移量
        public string pos;      // 词性

        public DTag(string tag)
        {
            var args = tag.Split(new[] { '_' }, 2);
            if (args[0][0] == '+') args[0] = args[0].Substring(1);      // 去掉正号"+"
            offset = int.Parse(args[0]);
            pos = args[1];
        }

        public override string ToString() => (offset > 0 ? "+" : "") + offset + "_" + pos;
    }
}
