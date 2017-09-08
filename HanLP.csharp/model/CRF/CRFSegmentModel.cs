using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HanLP.csharp.collection.trie;
using HanLP.csharp.corpus.io;

namespace HanLP.csharp.model.CRF
{
    /// <summary>
    /// CRF 分词模型，标签取值为 BMES
    /// </summary>
    public class CRFSegmentModel : CRFModel
    {
        private int _idM;
        private int _idE;
        private int _idS;

        public CRFSegmentModel(ITrie<FeatureFunction> ffTrie) : base(ffTrie) { }

        private void InitTagSet()
        {
            _idM = GetTagId("M");
            _idE = GetTagId("E");
            _idS = GetTagId("S");
        }

        public override bool LoadFromBin(ByteArray ba)
        {
            var res = base.LoadFromBin(ba);
            if (res)
                InitTagSet();

            return res;
        }

        protected override void OnLoadTxtFinished()
        {
            base.OnLoadTxtFinished();
            InitTagSet();
        }

        public override void Tag(Table table)
        {
            int size = table.Size;          // 观测序列的长度
            if(size == 1)                   // 单值标注
            {
                table.SetLast(0, "S");
                return;
            }

            var net = new double[size][];
            for(int i = 0; i < size; i++)
            {
                net[i] = new double[4];
                var scores = CalcScores(table, i);
                for(int tag = 0; tag < 4; tag++)
                {
                    net[i][tag] = CalcScore(scores, tag);
                }
            }

            net[0][_idM] = -1000;
            net[0][_idE] = -1000;

            var from = new int[size][];
            for(int i = 1; i < size; i++)
            {
                from[i] = new int[4];
                for(int now = 0; now < 4; now++)
                {
                    double maxScore = -1e10;
                    for(int pre = 0; pre < 4; pre++)
                    {
                        double score = net[i - 1][pre] + _matrix[pre][now] + net[i][now];
                        if(score > maxScore)
                        {
                            maxScore = score;
                            from[i][now] = pre;
                        }
                    }
                    net[i][now] = maxScore;
                }
            }
            // 反向回溯最佳路径
            int bestTag = net[size - 1][_idS] > net[size - 1][_idE] ? _idS : _idE;          // 最后一个观测的标注只可能是 S E
            //table.SetLast(size - 1, id2tag[bestTag]);                                       // 设置最后一个观测的标注
            for(int i = size - 1; i > 0; i--)
            {
                table.SetLast(i, id2tag[bestTag]);
                bestTag = from[i][bestTag];
            }
            table.SetLast(0,id2tag[bestTag]);
        }
    }
}
