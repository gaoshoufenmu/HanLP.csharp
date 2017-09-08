using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HanLP.csharp.seg.common;
using HanLP.csharp.dictionary;
using HanLP.csharp.corpus.tag;
using HanLP.csharp.corpus.dictionary;

namespace HanLP.csharp.algorithm
{
    public class Viterbi
    {
        /// <summary>
        /// 求解HMM模型问题三：最有可能的状态序列。
        /// 所有概率需提前取对数
        /// </summary>
        /// <param name="obs">观测序列，下标表示时刻，值表示观测值</param>
        /// <param name="states">状态序列，下标表示状态序号，值表示状态值</param>
        /// <param name="start_p">初始概率：初始状态的概率</param>
        /// <param name="trans_p">转移概率：当前状态转移到下一个状态的概率</param>
        /// <param name="emit_p">发射概率：处于某状态下观测到某观测值的概率</param>
        /// <returns></returns>
        public static int[] Compute(int[] obs, int[] states, double[] start_p, double[][] trans_p, double[][] emit_p)
        {
            int max_state_value = 0;        // 最大状态值
            for(int i = 0; i < states.Length; i++)
            {
                if (max_state_value < states[i])
                    max_state_value = states[i];
            }
            max_state_value++;              // 最大状态值 + 1，这样下标可以直接使用状态值
            double[,] v = new double[obs.Length,max_state_value];   // 长度为obs.Length的路径中每个节点的状态（对应的概率）
            int[,] path = new int[max_state_value, obs.Length];     // 最终状态：每个结束状态对应一条路径（长度为 obs.Length）上的状态序列

            for(int i = 0; i < states.Length; i++)
            {
                var y = states[i];      // 状态值，作为下标
                v[0, y] = start_p[y] + emit_p[y][obs[0]];   // 初始状态概率 乘以 处于此状态下观测到第一个观测值的概率
                path[y, 0] = y;
            }

            for(int t = 1; t < obs.Length; t++)             // 依次处理当前每个时刻的观测值
            {
                var newPath = new int[max_state_value, obs.Length];

                for(int i = 0; i < states.Length; i++)      // 依次分析当前每个可能的状态
                {
                    double prob = double.MaxValue;          // (1)
                    var y = states[i];                      // 假设当前状态为 y
                    int state;                              // 记录当前局部最大路径来自上个时刻对应的哪个状态

                    for(int j = 0; j < states.Length; j++)  // 依次讨论上一时刻的每种可能状态
                    {
                        var y0 = states[j];                 // 当上一时刻的状态为 y0 时，
                        double nprob = v[t - 1, y0] + trans_p[y0][y] + emit_p[y][obs[t]];   // 当前时刻处于状态y下观测到obs[t]的概率
                        if(nprob < prob)                    // (2)，结合(1)，不难知道，这里的概率是取了对数并且取了相反数，所以概率值越小越好
                        {
                            prob = nprob;
                            state = y0;             // 上一个时刻状态的状态为state
                            v[t,y] = prob;          // 更新局部最大概率
                            // 复制开始时刻到上一个时刻的路径
                            for(int k = 0; k < t; k++)
                            {
                                newPath[y, k] = path[state, k];
                            }
                            newPath[y, t] = y;      // 最大路径在t时刻的状态为 y
                        }
                    }
                }

                path = newPath;             // 更新了t时刻的经过路径
            }

            // 寻找最优路径
            double best_prob = double.MaxValue;
            int best_state = 0;
            var lastTimeIdx = obs.Length - 1;
            for(int i = 0; i < states.Length; i++)
            {
                var y = states[i];
                if (v[lastTimeIdx, y] < best_prob)
                {
                    best_state = y;
                    best_prob = v[lastTimeIdx, y];
                }
            }
            var best_path = new int[obs.Length];
            for (int i = 0; i < obs.Length; i++)
                best_path[i] = path[best_state, 0];
            return best_path;
        }

        /// <summary>
        /// 特化版的求解HMM模型
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="transMaxtrix">转移矩阵</param>
        public static void Compute(List<Vertex> vertices, TransformMatrixDictionary<Nature> transMaxtrix)
        {
            //int length = vertices.Count - 1;        // 去掉首节点之后的数量（包括了尾节点）
            double[][] cost = new double[2][];      // 滚动数组，用于保存最近两个节点的各状态对应的概率值
            var start = vertices[0];                // vertices包含了首尾节点，故 start 是辅助首节点
            Nature pre = start.attr.natures[0];     // start 节点的nature 肯定是 Nature.begin

            // 第二个节点计算
            Vertex item = vertices[1];
            cost[0] = new double[item.attr.natures.Length];

            for(int i = 0; i < item.attr.natures.Length; i++)           // 遍历第二个节点的所有可能的状态
            {
                var cur = item.attr.natures[i];
                cost[0][i] = transMaxtrix.Trans_Prob[(int)pre][(int)cur] -      // 从首节点状态转移到第二个节点状态的概率 乘以 第二个节点状态下观测到第二个节点值的发射概率（取对数并取相反数）
                    Math.Log((item.attr.freqs[i] + 1e-8) / transMaxtrix.GetFreq(cur));
            }

            Vertex preItem = item;
            Nature[] preTagSet = item.attr.natures;

            for (int i = 2; i < vertices.Count; i++)
            {
                int index_i_1 = i % 2;                  // i = even 时 为 0，表示上一个节点各状态的概率
                int index_i = 1 - index_i_1;            // i = even 时 为 1，表示当前节点各状态的概率

                item = vertices[i];
                cost[index_i] = new double[item.attr.natures.Length];   // 用于保存当前节点在各状态下的概率
                double perfect_cost_line = double.MaxValue;             // 保存 截止到当前时刻 i 为止，已确定的最优路径的概率

                var curTagSet = item.attr.natures;
                for(int k = 0; k < curTagSet.Length; k++)       // 遍历当前节点的所有可能状态（标签）
                {
                    var cur = curTagSet[k];                     // 当前状态
                    for(int n = 0; n < preTagSet.Length; n++)   // 遍历上一个节点的所有可能状态
                    {
                        var p = preTagSet[n];
                        double now = cost[index_i_1][n] + transMaxtrix.Trans_Prob[(int)p][(int)cur] -   // 上一节点状态n的概率 乘以 从上一节点转移到当前节点状态的概率 乘以 当前节点状态的发射概率
                            Math.Log((item.attr.freqs[k] + 1e-8) / transMaxtrix.GetFreq(cur));

                        if(now < cost[index_i][k])
                        {
                            cost[index_i][k] = now;
                            if(now < perfect_cost_line)
                            {
                                perfect_cost_line = now;        
                                pre = p;
                            }
                        }
                    }
                }

                preItem.ConfirmNature(pre);         //! 在当前时刻 i 为止确定的最优路径，并确定这是来自上一节点的哪个状态，从而确定上个节点的状态，然而这种方法确定的每个节点的状态是否属于同一条路径？
                preTagSet = curTagSet;
                preItem = item;
            }
        }

        /// <summary>
        /// 标准版的Viterbi算法，查询准确率高，效率稍低
        /// </summary>
        /// <typeparam name="E"></typeparam>
        /// <param name="roleTagList">观测序列</param>
        /// <param name="transMatrix">转移矩阵</param>
        /// <returns></returns>
        public static List<E> Compute<E>(List<TagFreqItem<E>> roleTagList, TransformMatrixDictionary<E> transMatrix) where E : IConvertible
        {
            var list = new List<E>(roleTagList.Count);      // 标签序列

            var cost = new double[2][];     // 滚动数组，作用与上一个方法类似
            var start = roleTagList[0];     // 首节点，这是一个辅助节点（另外一个辅助节点是尾节点）
            E pre = start.labelMap.First().Key;     // 首节点的 标签是确定的
            list.Add(pre);
            // 第二个节点的标签也是可以很容易算出来的
            var item = roleTagList[1];
            cost[0] = new double[item.labelMap.Count];      // 第二个节点所有可能的标签分别对应的概率
            int j = 0;
            foreach(var p in item.labelMap)
            {
                cost[0][j] = transMatrix.Trans_Prob[Convert.ToInt32(pre)][Convert.ToInt32(p.Key)] -     // transMatrix中所有概率均作了取对数并取相反数处理
                    Math.Log((item.GetFreqOrDefault(p.Key) + 1e-8) / transMatrix.GetFreq(p.Key));       // 状态转移概率乘以发射概率（频次相除，做了为 0 处理）
                j++;
            }
            var preTagSet = item.labelMap.Keys;

            //
            for(int i = 2; i < roleTagList.Count; i++)
            {
                int index_i_1 = i % 2;
                int index_i = 1 - index_i_1;

                item = roleTagList[i];
                cost[index_i] = new double[item.labelMap.Count];
                double perfect_cost_line = double.MaxValue;
                int k = 0;
                var curTagSet = item.labelMap.Keys;

                foreach(var cur in curTagSet)           // 遍历当前节点的所有可能标签
                {
                    cost[index_i][k] = double.MaxValue;
                    j = 0; 
                    foreach(var p in preTagSet)         // 遍历前一节点的所有标签
                    {
                        double now = cost[index_i_1][j]                                             // 上一节点某个状态的概率
                            + transMatrix.Trans_Prob[Convert.ToInt32(p)][Convert.ToInt32(cur)]      // 上一节点那个状态转移到此节点当前状态的概率
                            - Math.Log((item.GetFreqOrDefault(cur) + 1e-8) / transMatrix.GetFreq(cur)); // 此节点当前状态的发射概率
                        j++;
                        if(now < cost[index_i][k])  // 对此节点的当前状态来说，如果发现来自上一节点某个状态的路径对应概率更高（取了相反数，即更小）
                        {
                            cost[index_i][k] = now;     // 记录此节点当前状态的最优路径的概率
                            if(now < perfect_cost_line)
                            {
                                perfect_cost_line = now;    // 记录此节点所有状态中的最优路径的概率
                                pre = p;                    // 记录到达此节点时最优路径中上个节点的标签
                            }
                        }
                    }
                    k++;
                }   
                list.Add(pre);          //! 在当前时刻 i 为止确定的最优路径，并确定这是来自上一节点的哪个状态，从而确定上个节点的状态，然而这种方法确定的每个节点的状态是否属于同一条路径？
                preTagSet = curTagSet;
            }
            list.Add(list[0]);      // 尾节点（##末##）对应的标签
            return list;
        }

        /// <summary>
        /// 仅仅利用了转移矩阵的 Viterbi 算法
        /// </summary>
        /// <typeparam name="E">标签（状态）类型</typeparam>
        /// <param name="roleTagList">观测序列</param>
        /// <param name="transMatrix">转移矩阵</param>
        /// <returns></returns>
        public static List<E> ComputeSimply<E>(List<TagFreqItem<E>> roleTagList, TransformMatrixDictionary<E> transMatrix) where E : IConvertible
        {
            var start = roleTagList[0];             // 首节点
            E pre = start.labelMap.First().Key;     // 首节点标签
            E perfect_tag = pre;
            var list = new List<E>() { pre };

            for(int i = 1; i < roleTagList.Count; i++)
            {
                double perfect_cost = double.MaxValue;
                var item = roleTagList[i];

                foreach(var cur in item.labelMap.Keys)
                {
                    double now = transMatrix.Trans_Prob[Convert.ToInt32(pre)][Convert.ToInt32(cur)]
                        - Math.Log((item.GetFreqOrDefault(cur) + 1e-8) / transMatrix.GetFreq(cur));
                    if(perfect_cost > now)
                    {
                        perfect_cost = now;
                        perfect_tag = cur;
                    }
                }
                pre = perfect_tag;
                list.Add(pre);
            }

            return list;
        }
    }
}
