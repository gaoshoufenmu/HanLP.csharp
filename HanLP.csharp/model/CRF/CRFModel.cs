using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using HanLP.csharp.collection.trie;
using HanLP.csharp.corpus.io;
using HanLP.csharp.utility;
using HanLP.csharp.collection;

namespace HanLP.csharp.model.CRF
{
    public class CRFModel
    {
        protected Dictionary<string, int> tag2id;
        protected string[] id2tag;

        /// <summary>
        /// 特征函数Trie
        /// </summary>
        protected ITrie<FeatureFunction> _ffTrie;
        /// <summary>
        /// 特征模板列表
        /// </summary>
        protected List<FeatureTemplate> _ftList;

        /// <summary>
        /// tag的二元转移矩阵，适用于BiGram Feature
        /// </summary>
        protected double[][] _matrix;
        public CRFModel()
        {
            _ffTrie = new DoubleArrayTrie<FeatureFunction>();
        }

        public CRFModel(ITrie<FeatureFunction> trie)
        {
            _ffTrie = trie;
        }

        public virtual void Load(string path)
        {
            if (LoadFromBin(ByteArray.Create(path + Predefine.BIN_EXT))) return;
            LoadFromTxt(path);
        }

        public virtual void LoadFromTxt(string path)
        {
            //if (Load(ByteArray.Create(path + Predefine.BIN_EXT))) return;

            var lines = File.ReadAllLines(path);
            // print lines[0];          // version
            // print lines[1];          // cost-factor
            // print lines[2];          // "maxid:xx"
            // print lines[3];          // xsize
            // lines[4]: blank line

            tag2id = new Dictionary<string, int>();
            int i = 5;
            string line = null;
            for(; i < lines.Length; i++)
            {
                line = lines[i];
                if (string.IsNullOrEmpty(line)) break;
                tag2id[line] = i - 5;
            }
            int size = tag2id.Count;
            id2tag = new string[size];      //! id2tag的元素赋值为什么不放入上一个循环中？ tag2id.Count必然比keyvaluepair.value最大值大1，否则下面给id2tag元素赋值会出错
            foreach (var p in tag2id)
                id2tag[p.Value] = p.Key;

            var ffMap = new SortedDictionary<string, FeatureFunction>(StrComparer.Default);
            var ffList = new List<FeatureFunction>();

            _ftList = new List<FeatureTemplate>();
            i++;                                    // 跳过当前的空白行
            for(;i < lines.Length; i++)
            {
                line = lines[i];
                if (string.IsNullOrEmpty(line)) break;
                if ("B" != line)
                {
                    _ftList.Add(FeatureTemplate.Create(line));
                }
                else
                    _matrix = new double[size][];               //? 第一次遇到"B"行，表示接来下是_matrix数据信息，于是，初始化_matrix方阵
            }
            i++;                                    // 跳过当前空白行
            if (_matrix != null)
                i++;                                // 如果提供了_matrix数据信息，则接下来一行为 0 B，跳过
            for(; i < lines.Length; i++)
            {
                line = lines[i];
                if (string.IsNullOrEmpty(line)) break;
                var args = line.Split(new[] { ' ' }, 2);
                var chars = args[1].ToCharArray();
                var ff = new FeatureFunction(chars, size);
                ffMap[args[1]] = ff;
                ffList.Add(ff);
            }
            i++;                                    // 跳过当前空白行
            if(_matrix != null)
            {
                for(int k = 0; k < size; k++)
                {
                    _matrix[k] = new double[size];
                    for (int j = 0; j < size; j++)
                        _matrix[k][j] = double.Parse(lines[i++]);
                }
            }

            for(int k = 0; k < ffList.Count; k++)
            {
                var ff = ffList[k];
                for (int j = 0; j < size; j++)
                    ff.w[j] = double.Parse(lines[i++]);
            }
            if(i < lines.Length)
            {
                // log  "文本读取有残留，可能出现问题"
            }
            _ffTrie.Build(ffMap);

            // 缓存bin数据文件
            var fs = new FileStream(path + Predefine.BIN_EXT, FileMode.Create, FileAccess.Write);
            try
            {
                Save(fs);
                fs.Close();
            }
            catch(Exception e)
            {
                // log e
                fs.Close();
                File.Delete(path + Predefine.BIN_EXT);
            }
            OnLoadTxtFinished();
        }

        protected virtual void OnLoadTxtFinished()
        {

        }

        /// <summary>
        /// 维特比算法标注
        /// </summary>
        /// <param name="table"></param>
        public virtual void Tag(Table table)
        {
            var size = table.Size;
            if (size == 0) return;

            int tagSize = id2tag.Length;
            var net = new double[size][];
            for (int i = 0; i < size; i++)                  // 遍历观测序列
            {
                var scores = CalcScores(table, i);
                net[i] = new double[tagSize];
                for(int tag = 0; tag < tagSize; tag++)      // 遍历标签的所有可能值
                {
                    net[i][tag] = CalcScore(scores, tag);
                }
            }

            if(size == 1)                                   // 单值标注
            {
                var maxScore = -1e10;                       // 记录最佳标注得分
                int bestTag = 0;                            // 记录最佳标注
                for(int tag = 0; tag < tagSize; tag++)
                {
                    if(net[0][tag] > maxScore)
                    {
                        maxScore = net[0][tag];
                        bestTag = tag;
                    }
                }
                table.SetLast(0, id2tag[bestTag]);
                return;
            }

            // 序列标注
            var from = new int[size][];                     // 记录所有可能的局部最佳路径，用于最后回溯
            //from[0] = new int[tagSize];
            for(int i = 1; i < size; i++)
            {
                from[i] = new int[tagSize];
                for(int now = 0; now < tagSize; now++)
                {
                    double maxScore = -1e10;
                    for(int pre = 0; pre < tagSize; pre++)
                    {
                        double score = net[i - 1][pre] + _matrix[pre][now] + net[i][now];
                        if(score > maxScore)
                        {
                            maxScore = score;
                            from[i][now] = pre;             // 记录位置 i 处标签为now的条件下，当前最佳路径对应的前一个位置i-1处的标签
                        }
                    }
                    net[i][now] = maxScore;                 // 更新位置i 处标签为now 的条件下，最佳得分
                }
            }

            // 反向回溯全局最佳路径
            double globalScore = -1e10;
            int currTag = 0;
            // 寻找最后一个观测的最佳标注，以此回溯
            for(int tag = 0; tag < net[size -1].Length; tag++)
            {
                if(net[size - 1][tag] > globalScore)
                {
                    globalScore = net[size - 1][tag];
                    currTag = tag;
                }
            }
            //table.SetLast(size - 1, id2tag[currTag]);
            //currTag = from[size - 1][currTag];          // 获取倒数第二个观测的标注
            for(int i = size - 1; i > 0; i--)
            {
                table.SetLast(i, id2tag[currTag]);
                currTag = from[i][currTag];
            }
            table.SetLast(0, id2tag[currTag]);
        }

        protected List<double[]> CalcScores(Table table, int curr)
        {
            var scores = new List<double[]>();
            for(int i = 0; i < _ftList.Count; i++)
            {
                var o = _ftList[i].GenerateParam2Str(table, curr);
                var ff = _ffTrie.GetOrDefault(o);
                if (ff == null) continue;
                scores.Add(ff.w);
            }
            return scores;
        }

        public static double CalcScore(List<double[]> scores, int tag)
        {
            double score = 0;
            for(int i = 0; i < scores.Count; i++)
            {
                score += scores[i][tag];
            }
            return score;
        }

        /// <summary>
        /// 根据给定的CRF模型txt数据文件创建CRF模型类实例
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static CRFModel CreateFromTxt(string path)
        {
            var crf = new CRFModel();
            crf.LoadFromTxt(path);
            return crf;
        }

        /// <summary>
        /// 根据给定模型数据文件路径创建CRF模型类实例
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static CRFModel Create(string path)
        {
            var crf = new CRFModel();
            crf.Load(path);
            return crf;
        }


        /// <summary>
        /// 根据标签获取对应的下标id
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public int GetTagId(string tag)
        {
            if (tag2id.TryGetValue(tag, out var id))
                return id;

            return -1;
        }

        public virtual bool LoadFromBin(ByteArray ba)
        {
            if (ba == null) return false;
            try
            {
                var size = ba.NextInt_HighFirst();          // 使用Model目标的数据文件均与相应的原数据文件兼容，所以这里只讨论高位在前的情况
                id2tag = new string[size];
                tag2id = new Dictionary<string, int>(size);

                for(int i = 0; i < size; i++)
                {
                    id2tag[i] = ba.NextUTFStr(true);
                    tag2id[id2tag[i]] = i;
                }

                var ffs = new FeatureFunction[ba.NextInt_HighFirst()];
                for(int i = 0; i < ffs.Length; i++)
                {
                    ffs[i] = new FeatureFunction();
                    ffs[i].Load(ba);
                }

                _ffTrie.Load(ba, ffs, true);
                size = ba.NextInt_HighFirst();
                _ftList = new List<FeatureTemplate>(size);
                for(int i = 0; i < size; i++)
                {
                    var ft = new FeatureTemplate();
                    ft.Load(ba);
                    _ftList.Add(ft);
                }
                size = ba.NextInt_HighFirst();
                if (size == 0) return true;

                _matrix = new double[size][];
                for(int i = 0; i < size; i++)
                {
                    _matrix[i] = new double[size];
                    for(int j = 0; j < size; j++)
                    {
                        _matrix[i][j] = ba.NextDouble_HighFirst();
                    }
                }
                return true;
            }
            catch(Exception e)
            {
                return false;
            }
        }

        /// <summary>
        /// 只考虑兼容原数据文件的情况，即采用高位在前
        /// </summary>
        /// <param name="fs"></param>
        public void Save(FileStream fs)
        {
            var bs = ByteUtil.Int2Byte_HighFirst(id2tag.Length);
            fs.Write(bs, 0, bs.Length);
            for(int i = 0; i < id2tag.Length; i++)
            {
                bs = ByteUtil.UTF2Byte_HighFirst(id2tag[i]);
                fs.Write(bs, 0, bs.Length);
            }

            var ffs = _ffTrie.Values;
            bs = ByteUtil.Int2Byte_HighFirst(ffs.Length);
            fs.Write(bs, 0, bs.Length);
            for (int i = 0; i < ffs.Length; i++)
                ffs[i].Save(fs);

            _ffTrie.Save(fs);

            bs = ByteUtil.Int2Byte_HighFirst(_ftList.Count);
            fs.Write(bs, 0, bs.Length);
            for (int i = 0; i < _ftList.Count; i++)
                _ftList[i].Save(fs);

            if(_matrix != null)
            {
                var fstDimLen = _matrix.GetLength(0);
                bs = ByteUtil.Int2Byte_HighFirst(fstDimLen);
                fs.Write(bs, 0, bs.Length);
                for(int i = 0; i < fstDimLen; i++)
                {
                    for(int j = 0; j < fstDimLen; j++)      // 方阵
                    {
                        bs = ByteUtil.Double2Byte_HighFirst(_matrix[i][j]);
                        fs.Write(bs, 0, bs.Length);
                    }
                }
            }
            else
            {
                fs.Write(new byte[4], 0, 4);
            }
        }
    }
}
