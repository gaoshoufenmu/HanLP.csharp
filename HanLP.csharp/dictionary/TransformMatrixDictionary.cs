using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using ExtraConstraints;
using HanLP.csharp.corpus.tag;

namespace HanLP.csharp.dictionary
{
    /// <summary>
    /// 转移矩阵词典
    /// </summary>
    /// <typeparam name="E"></typeparam>
    public class TransformMatrixDictionary<E> where E : IConvertible
    {
        /// <summary>
        /// 内部标签下标最大值
        /// </summary>
        private int _ordinaryMax;
        /// <summary>
        /// 转移矩阵
        /// </summary>
        private int[][] _matrix;
        /// <summary>
        /// 每个标签出现的次数
        /// </summary>
        private int[] _total;
        /// <summary>
        /// 所有标签出现的总次数
        /// </summary>
        private int _totalFreq;
        /// <summary>
        /// 状态
        /// </summary>
        private int[] _states;
        /// <summary>
        /// 初始状态概率
        /// </summary>
        private double[] _start_prob;
        /// <summary>
        /// 状态转移概率
        /// </summary>
        private double[][] _trans_prob;


        public int[] Total => _total;
        public int[] States => _states;
        public double[] Start_Prob => _start_prob;
        public double[][] Trans_Prob => _trans_prob;

        private Type _enumType;

        public TransformMatrixDictionary(Type enumType) => _enumType = enumType;

        public bool Load(string path)
        {
            try
            {
                int[] ordinaryArr = null;                           // 下标为标签序号，值为标签值
                foreach(var line in File.ReadLines(path))
                {
                    if(ordinaryArr == null)
                    {
                        var segs = line.Split(',').Skip(1).ToArray();   // 为了制表方便，第一个segment是废物，所以跳过
                        ordinaryArr = new int[segs.Length];             // 第一行：A-Z，共26个元素

                        for (int i = 0; i < ordinaryArr.Length; i++)
                        {
                            ordinaryArr[i] = (int)Enum.Parse(_enumType, segs[i]);
                            if (_ordinaryMax < ordinaryArr[i])
                                _ordinaryMax = ordinaryArr[i];
                        }

                        _ordinaryMax++;                             // 状态转移矩阵下标为标签值，所以矩阵长度为最大标签值 + 1

                        _matrix = new int[_ordinaryMax][];
                        for (int j = 0; j < _ordinaryMax; j++)
                        {
                            _matrix[j] = new int[_ordinaryMax];
                        }
                    }
                    else
                    {
                        var segs = line.Split(',');
                        var currOrdinary = (int)Enum.Parse(_enumType, segs[0]);     // 当前标签值
                        for (int i = 0; i < ordinaryArr.Length; i++)
                            _matrix[currOrdinary][ordinaryArr[i]] = int.Parse(segs[i + 1]);     // 从当前标签转移到某个标签的频次
                    }
                }

                _total = new int[_ordinaryMax];     // 下标为标签值
                for(int j = 0; j < _ordinaryMax; j++)
                {
                    for (int i = 0; i < _ordinaryMax; i++)
                        _total[j] += _matrix[j][i];          // 按行累加，从状态 j 出发转移到其他任意状态的次数，相当于 状态 j 的出度
                }
                for(int j = 0; j < _ordinaryMax; j++)
                {
                    if (_total[j] == 0)
                    {
                        for (int i = 0; i < _ordinaryMax; i++)
                            _total[j] += _matrix[i][j];     // 对于出度为 0 的节点，则统计其 入度，按列累加
                    }
                }
                for (int j = 0; j < _ordinaryMax; j++)
                    _totalFreq += _total[j];

                _states = ordinaryArr;
                _start_prob = new double[_ordinaryMax];     // 下标为状态值，初始状态概率
                foreach(var s in _states)
                {
                    double freq = _total[s] + 1e-8;         // 为0 校正，状态 s 的频次
                    _start_prob[s] = -Math.Log(freq / _totalFreq);
                }

                _trans_prob = new double[_ordinaryMax][];
                for (int i = 0; i < _ordinaryMax; i++)
                    _trans_prob[i] = new double[_ordinaryMax];

                for(int i = 0; i < _states.Length; i++)
                {
                    var from = _states[i];      // from 状态
                    if (_total[from] > 0)       // from状态节点的出度（或入度）大于 0 ，防止是稀疏矩阵
                    {
                        for (int j = 0; j < _states.Length; j++)
                        {
                            var to = _states[j];    // to 状态
                            double freq = _matrix[from][to] + 1e-8;     // 为 0 校正， 状态from  到 状态 to 的频次
                            _trans_prob[from][to] = -Math.Log(freq / _total[from]);          // 条件概率
                        }
                    }
                }

                return true;
            }
            catch(Exception e)
            {
                // log warning "loading transfrom matrix dictionary file failed"
                return false;
            }
        }
        public int GetFreq(E from, E to) => _matrix[System.Convert.ToInt32(from)][System.Convert.ToInt32(to)];
        public int GetFreq(string from, string to) => GetFreq(Convert(from), Convert(to));

        public int GetFreq(E state) => _total[System.Convert.ToInt32(state)];
        public int GetFreq(string state) => GetFreq(Convert(state));

        public int TotalFreq => _totalFreq;

        /// <summary>
        /// 扩展内部矩阵
        /// 增加一个状态位
        /// </summary>
        public void ExtendSize()
        {
            ++_ordinaryMax;
            var new_trans_prob = new double[_ordinaryMax][];
            for(int i = 0; i < _trans_prob.Length;i++)
            {
                new_trans_prob[i] = new double[_ordinaryMax];
                Array.Copy(_trans_prob[i], new_trans_prob[i], _trans_prob.Length);
            }
            new_trans_prob[_trans_prob.Length] = new double[_ordinaryMax];

            _trans_prob = new_trans_prob;

            var new_total = new int[_ordinaryMax];
            Array.Copy(_total, new_total, _total.Length);
            _total = new_total;

            var new_start_prob = new double[_ordinaryMax];
            Array.Copy(_start_prob, new_start_prob, _start_prob.Length);

            var new_matrix = new int[_ordinaryMax][];
            for(int i = 0; i < _matrix.Length; i++)
            {
                new_matrix[i] = new int[_ordinaryMax];
                Array.Copy(_matrix[i], new_matrix[i], _matrix.Length);
            }
            new_matrix[_matrix.Length] = new int[_ordinaryMax];
        }

        /// <summary>
        /// 标签字符串转为标签枚举类型
        /// </summary>
        /// <param name="label"></param>
        /// <returns></returns>
        private E Convert(string label) => (E)Enum.Parse(_enumType, label);

        public override string ToString() => $"EnumType: {_enumType.Name}, ordinaryMax: {_ordinaryMax}, State Count: {_total.Length}, totalFreq: {_totalFreq}";
    }

    public class CoreDictTransfromMatrixDictionary
    {
        public static TransformMatrixDictionary<Nature> transformMatrixDictionary;

        static CoreDictTransfromMatrixDictionary()
        {
            transformMatrixDictionary = new TransformMatrixDictionary<Nature>(typeof(Nature));
            if (!transformMatrixDictionary.Load(Config.Core_TR_Dict_Path))
                throw new Exception("loading core dictionary transfrom matrix error");
        }
    }

}
