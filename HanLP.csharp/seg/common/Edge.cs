using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanLP.csharp.seg.common
{
    public class Edge
    {
        public double weight;
        internal string name;

        protected Edge(double weight, string name)
        {
            this.weight = weight;
            this.name = name;
        }
    }

    /// <summary>
    /// 记录了起点的边
    /// </summary>
    public class EdgeFrom : Edge
    {
        public int from;

        public EdgeFrom(int from, double weight, string name) : base(weight, name)
        {
            this.from = from;
        }

        public override string ToString() => $"from: {from}, weight: {weight}, name: {name}";
    }
}
