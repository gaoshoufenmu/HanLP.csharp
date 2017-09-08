using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanLP.csharp.seg.common
{
    public class Graph
    {
        /// <summary>
        /// 所有顶点数组
        /// </summary>
        public Vertex[] vertices;
        /// <summary>
        /// 边，下标表示到达的顶点在<seealso cref="vertices"/>的下标，值是到达这个顶点的来自顶点集合
        /// </summary>
        public List<EdgeFrom>[] edgesTo;

        public Graph(Vertex[] vertexes)
        {
            int size = vertexes.Length;
            vertices = vertexes;
            edgesTo = new List<EdgeFrom>[size];
            for (int i = 0; i < size; i++)
                edgesTo[i] = new List<EdgeFrom>();
        }

        /// <summary>
        /// 连接两个节点
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="weight"></param>
        public void Connect(int from, int to, double weight) =>
            edgesTo[to].Add(new EdgeFrom(from, weight, vertices[from].word + "->" + vertices[to].word));

        /// <summary>
        /// 获取的到达指定节点的出发节点列表
        /// </summary>
        /// <param name="to"></param>
        /// <returns></returns>
        public List<EdgeFrom> GetEdgeFroms(int to) => edgesTo[to];

        /// <summary>
        /// 获取路径上的节点
        /// </summary>
        /// <param name="path">下标所表示的路径</param>
        /// <returns></returns>
        public List<Vertex> Parse(int[] path)
        {
            var list = new List<Vertex>();
            for (int i = 0; i < path.Length; i++)
                list.Add(vertices[path[i]]);

            return list;
        }

        /// <summary>
        /// 将路径上的节点的realWord连接起来
        /// </summary>
        /// <param name="path"></param>
        /// <param name="sep">连接分隔符</param>
        /// <returns></returns>
        public static string Parse(List<Vertex> path, string sep = " ")
        {
            if (path.Count < 2) throw new ArgumentException("路径上节点数量不足2");

            var sb = new StringBuilder();
            for(int i = 1; i < path.Count - 1; i++)     // 去掉首尾（空白辅助）节点
            {
                var v = path[i];
                sb.Append(v.realWord).Append(sep);
            }
            return sb.ToString();
        }

        public string PrintByTo()
        {
            var sb = new StringBuilder("========== 按终点打印 ===========\n");
            for(int to = 0; to < edgesTo.Length; to++)
            {
                var froms = edgesTo[to];
                foreach(var f in froms)
                {
                    sb.Append(string.Format("to:%3d, from:%3d, weight:%05.2f, word:%s\n", to, f.from, f.weight, f.name));
                }
            }
            return sb.ToString();
        }

        public override string ToString() => $"vertices count: {vertices.Length}";
    }
}
