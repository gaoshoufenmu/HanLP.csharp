using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using HanLP.csharp.corpus.document;
using HanLP.csharp.corpus.io;
namespace HanLP.csharp.model.trigram
{
    /// <summary>
    /// 基于字符的生成模型，本质是一个TriGram文法模型，2阶隐马尔科夫模型
    /// </summary>
    public class CharBasedGenerativeModel
    {
        private bool _binDataCompatible = true;
        public bool BinDataCompatible { get => _binDataCompatible; set => _binDataCompatible = value; }
        /// <summary>
        /// 标注标签
        /// </summary>
        public static readonly char[] id2tag = new[] { 'b', 'm', 'e', 's' };
        /// <summary>
        /// 视野范围外事件，看不见的一个字符'\b'，其标注tag 为'x'，这个字符是辅助字符
        /// 对于一个句子来说，第一和第二个字符均没有完整的前两个状态，形成不了TriGram窗口
        /// </summary>
        public static readonly char[] bos = { '\b', 'x' };
        /// <summary>
        /// 无穷小（负无穷）
        /// </summary>
        public const double INF_M = -1e10;
        /// <summary>
        /// 频次统计
        /// </summary>
        private Probability _tf;
        /// <summary>
        /// 二阶隐马尔科夫三个参数
        /// </summary>
        private double _l1, _l2, _l3;

        public CharBasedGenerativeModel() => _tf = new Probability();

        /// <summary>
        /// 学习，观测一个句子
        /// </summary>
        /// <param name="words"></param>
        public void Learn(List<Word> words)
        {
            var sentence = new List<char[]>();
            // 将句子中的词依次排开，标记其中所有的字符
            for(int i = 0; i < words.Count; i++)
            {
                var w = words[i].Value;
                if(w.Length == 1)           // 单字符词
                {
                    sentence.Add(new[] { w[0], 's' });
                }
                else
                {
                    sentence.Add(new[] { w[0], 'b' });
                    for(int j = 1; j < w.Length -1; j++)
                    {
                        sentence.Add(new[] { w[j], 'm' });
                    }
                    sentence.Add(new[] { w[w.Length - 1], 'e' });
                }
            }

            // 当前的TriGram 窗口
            var now = new char[3][];        // 二阶HMM， TriGram
            now[1] = bos;
            now[2] = bos;
            _tf.Add(1, bos, bos);   // 添加一个词频对： "\bx\bx" -> 1
            _tf.Add(2, bos);        // 添加一个词频对： "\bx" -> 2

            for(int i = 0; i < sentence.Count; i++)
            {
                Array.Copy(now, 1, now, 0, 2);      // 将now矩阵第 1，2 行数据移到 第 0，1 行上
                now[2] = sentence[i];               // 设置now矩阵第三行，current
                _tf.Add(1, sentence[i]);            // uni -> current
                _tf.Add(1, now[1], now[2]);         // bi  -> current - 1, current
                _tf.Add(1, now);                    // tri -> current - 2, current - 1, current
            }
        }

        /// <summary>
        /// 观测完毕后训练
        /// </summary>
        public void Train()
        {
            double tl1 = 0;
            double tl2 = 0;
            double tl3 = 0;

            foreach(var key in _tf.d.Keys)
            {
                if (key.Length != 6) continue;      // tri samples

                var now = new char[][]
                {
                    new [] {key[0], key[1] },       // char-tag pair
                    new [] {key[2], key[3] },
                    new [] {key[4], key[5] }
                };

                //? 减1是为了去掉隐性字符'\b'的影响，任何一个字符或者两个相邻字符也可以与'\b'形成 Bi Tri 窗口，故也需要减 1 处理
                // Uni, Bi, Tri 窗口的（条件）概率
                double c3 = Div_S(_tf.Get(now) - 1, _tf.Get(now[0], now[1]) - 1);
                double c2 = Div_S(_tf.Get(now[1], now[2]) - 1, _tf.Get(now[1]) - 1);
                double c1 = Div_S(_tf.Get(now[2]) - 1, _tf.Total - 1);

                // Uni, Bi, Tri，找出（条件）概率最大值，然后更新（增加）其对应的权重值 \lambda
                if (c3 >= c1 && c3 >= c2)
                    tl3 += _tf.Get(now);
                else if (c2 >= c1 && c2 >= c3)
                    tl2 += _tf.Get(now);
                else if (c1 >= c2 && c1 >= c3)
                    tl1 += _tf.Get(now);
            }

            // 归一化各权重值
            var l = tl1 + tl2 + tl3;
            _l1 = Div_S(tl1, l);
            _l2 = Div_S(tl2, l);
            _l3 = Div_S(tl3, l);
        }

        public char[] Tag(char[] charArray)
        {
            if (charArray.Length == 0) return new char[0];
            if (charArray.Length == 1) return new[] { 's' };

            var tags = new char[charArray.Length];
            var now = new double[4,4];
            var first = new double[4];      // 首字符的状态概率

            // link[i,s,t] := 第i 个字符的当前tag为t，第i -1 个字符的tag为 s -> 得到第i - 2 个字符的tag值
            var link = new int[charArray.Length,4,4];

            // 首字符的tag只可能是b|s，所以是其余tag(m|e)的概率为0，对数为负无穷
            // 于是计算tag为b|s的概率对数即可，计算中使用两个辅助隐性字符'\b'
            for(int s = 0; s < 4; s++)      // s: tag index
            {
                double p = (s == 1 || s == 2) ? INF_M : LogProb(bos, bos, new[] { charArray[0], id2tag[s] });
                first[s] = p;
            }

            // 第二个字符需要一个辅助隐性字符来计算概率
            for(int f = 0; f < 4; f++)
            {
                for(int s = 0; s < 4; s++)
                {
                    double p = first[f] + LogProb(bos, new[] { charArray[0], id2tag[f] }, new[] { charArray[1], id2tag[s] });
                    now[f,s] = p;
                    link[1, f, s] = f;
                }
            }

            // 从第三个字符开始，使用TriGram
            var pre = new double[4, 4];
            for(int i = 2; i < charArray.Length; i++)
            {
                var temp = pre;     // 交换引用，避免每次新建二维数组
                pre = now;
                now = temp;

                for(int s = 0; s < 4; s++)              // 遍历前1 字符的tag
                {
                    for(int t = 0; t < 4; t++)          // 遍历当前字符的tag
                    {
                        now[s, t] = -1e20;      // 设置初值为最小值，负无穷
                        for(int f = 0; f < 4; f++)      // 遍历前2 字符的tag
                        {
                            double p = pre[f, s] + LogProb(new[] { charArray[i - 2], id2tag[f] },
                                                          new[] { charArray[i - 1], id2tag[s] },
                                                          new[] { charArray[i], id2tag[t] });

                            // 找出概率最大时对应的前2 字符的tag
                            if(p > now[s,t])
                            {
                                now[s, t] = p;          // 更新概率
                                link[i, s, t] = f;      // 更新链接前2 字符的tag
                            }
                        }
                    }
                }
            }

            // 找出使得整个序列联合概率最大对应的tag，由于最后一个字符对应的概率就是全局序列整体的概率，
            // 所以可以得到最后一个TriGram的tag，然后通过link倒推得到所有字符的tag
            // 此时now就表示最后一个TriGram 窗口
            double score = INF_M;
            int ss = 0;                         // 概率最大对应的倒数第二字符的tag
            int tt = 0;                         // 概率最大对应的末字符的tag
            for(int i = 0; i < 4; i++)          // 遍历倒数第二字符的tag 
            {
                for(int j = 0; j < 4; j++)      // 遍历末字符的tag
                {
                    if(now[i,j] > score)
                    {
                        score = now[i, j];
                        ss = i;
                        tt = j;
                    }
                }
            }

            var linkFstDim = link.GetLength(0);
            for(int i = linkFstDim - 1; i >= 0; i--)       // 通过link倒推
            {
                tags[i] = id2tag[tt];
                int f = link[i, ss, tt];                    // i - 2 处字符的tag
                tt = ss;                                    //
                ss = f;
            }
            return tags;
        }

        public static double Div_S(int denum, int num)
        {
            if (num == 0) return 0;
            return denum / (double)num;
        }
        public static double Div_S(double denum, double num)
        {
            if (num == 0) return 0;
            return denum / num;
        }

        /// <summary>
        /// 计算观测序列概率的对数
        /// </summary>
        /// <param name="s1">前2个观测</param>
        /// <param name="s2">前1个观测</param>
        /// <param name="s3">当前观测</param>
        /// <returns></returns>
        public double LogProb(char[] s1, char[] s2, char[] s3)
        {
            double uni = _l1 * _tf.Prob(s3);
            double bi = Div_S(_l2 * _tf.GetOrDefault(0, s2, s3), _tf.GetOrDefault(0, s2));
            double tri = Div_S(_l3 * _tf.GetOrDefault(0, s1, s2, s3), _tf.GetOrDefault(0, s1, s2));

            var prob = uni + bi + tri;      // 当前观测序列的概率
            if (prob == 0) return INF_M;

            return Math.Log(prob);      // 计算概率对数
        }


        public void Load(ByteArray ba)
        {
            _l1 = ba.NextDouble(_binDataCompatible);
            _l2 = ba.NextDouble(_binDataCompatible);
            _l3 = ba.NextDouble(_binDataCompatible);

            _tf.Load(ba, _binDataCompatible);
        }



        /// <summary>
        /// todo: 需要考虑是否兼容模式
        /// </summary>
        /// <param name="fs"></param>
        public void Save(FileStream fs)
        {
            var bytes = BitConverter.GetBytes(_l1);
            fs.Write(bytes, 0, 8);
            bytes = BitConverter.GetBytes(_l2);
            fs.Write(bytes, 0, 8);
            bytes = BitConverter.GetBytes(_l3);
            fs.Write(bytes, 0, 8);
            _tf.Save(fs);
        }
    }
}
