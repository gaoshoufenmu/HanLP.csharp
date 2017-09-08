using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanLP.csharp.algorithm.ahocorasick
{
    public class IntervalTree
    {
        private IntervalNode _root;

        public IntervalTree(IEnumerable<IIntervalable> intervals) => _root = new IntervalNode(intervals);

        /// <summary>
        /// 移除与指定区间集合有重叠的部分
        /// </summary>
        /// <param name="intervals"></param>
        /// <returns></returns>
        public List<IIntervalable> RemoveOverlaps(List<IIntervalable> intervals)
        {
            intervals.Sort(new IntervalableComparatorBySize());
            var removeIntervals = new SortedSet<IIntervalable>(new IntervalableComparatorBySize());

            for(int i = 0; i < intervals.Count; i++)
            {
                var inter = intervals[i];
                if (removeIntervals.Contains(inter)) continue;

                foreach (var it in FindOverlaps(inter))
                    removeIntervals.Add(it);
            }

            foreach(var ri in removeIntervals)
                intervals.Remove(ri);

            intervals.Sort(new IntervalableComparatorByPos());
            return intervals;
        }

        public List<IIntervalable> FindOverlaps(IIntervalable interval) =>
            _root.GetOverlap(interval);
    }

    public class IntervalNode
    {
        /// <summary>
        /// 区间集合的最左端
        /// </summary>
        private IntervalNode _left = null;
        /// <summary>
        /// 区间集合的最右端
        /// </summary>
        private IntervalNode _right = null;
        /// <summary>
        /// 中点
        /// </summary>
        private int _point;
        /// <summary>
        /// 区间集合，区间包含了中点
        /// </summary>
        private List<IIntervalable> _intervals = new List<IIntervalable>();

        public IntervalNode(IEnumerable<IIntervalable> intervals)
        {
            _point = DetermineMedian(intervals);
            var lefts = new List<IIntervalable>();
            var rights = new List<IIntervalable>();

            foreach(var inter in intervals)
            {
                if (inter.End < _point)
                    lefts.Add(inter);
                else if (inter.Start > _point)
                    rights.Add(inter);
                else
                    _intervals.Add(inter);
            }
            if (lefts.Count > 0)
                _left = new IntervalNode(lefts);
            if (rights.Count > 0)
                _right = new IntervalNode(rights);
        }

        /// <summary>
        /// 计算中点：所有点最左边 与 所有点最右边 的平均值
        /// </summary>
        /// <param name="intervals"></param>
        /// <returns></returns>
        private int DetermineMedian(IEnumerable<IIntervalable> intervals)
        {
            int start = int.MaxValue;
            int end = int.MinValue;
            foreach(var inter in intervals)
            {
                var currStart = inter.Start;
                var currEnd = inter.End;
                if (currStart < start)
                    start = currStart;
                if (currEnd > end)
                    end = currEnd;
            }
            return (start + end) / 2;
        }

        /// <summary>
        /// 寻找与指定区间有重叠的区间
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        public List<IIntervalable> GetOverlap(IIntervalable interval)
        {
            var overlaps = new List<IIntervalable>();
            if(_point < interval.Start)
            {
                // 右边找找
                AddOverlaps(interval, overlaps, FindOverlapRange(_right, interval));
                AddOverlaps(interval, overlaps, Check4Overlap(interval, Direction.RIGHT));
            }
            else if(_point > interval.End)  // 目标区间在当前节点的中点的左边
            {
                AddOverlaps(interval, overlaps, FindOverlapRange(_left, interval));
                AddOverlaps(interval, overlaps, Check4Overlap(interval, Direction.LEFT));
            }
            else
            {
                AddOverlaps(interval, overlaps, _intervals);
                AddOverlaps(interval, overlaps, FindOverlapRange(_left, interval));
                AddOverlaps(interval, overlaps, FindOverlapRange(_right, interval));
            }
            return overlaps;
        }

        /// <summary>
        /// 将备选区间添加到指定列表中
        /// </summary>
        /// <param name="interval">备选区间不能与指定区间相等</param>
        /// <param name="list"></param>
        /// <param name="candidates"></param>
        private void AddOverlaps(IIntervalable interval, List<IIntervalable> list, List<IIntervalable> candidates)
        {
            for(int i = 0; i < candidates.Count; i++)
            {
                var inter = candidates[i];
                if (!inter.Equals(interval))
                    list.Add(inter);
            }
        }

        /// <summary>
        /// 在当前节点的区间集合中寻找与指定区间重叠的
        /// </summary>
        /// <param name="interval">一个区间，与该区间重叠</param>
        /// <param name="direction">方向，表示是左边还是右边</param>
        /// <returns></returns>
        private List<IIntervalable> Check4Overlap(IIntervalable interval, Direction direction)
        {
            var overlaps = new List<IIntervalable>();
            foreach(var inter in _intervals)
            {
                if(direction == Direction.LEFT)     // 检测与指定区间是否右边重叠:    interval  ________________   |_point|
                {                                   //                                                 ______________________  inter  (_point 处于 inter区间里面)
                    if (inter.Start <= interval.End)
                        overlaps.Add(inter);
                }
                else                                // 检测与指定区间是否左边重叠:    |_point|      ________________  interval
                {                                   //                      ___________________________   inter    (_point 处于 inter区间里面)
                    if (inter.End >= interval.Start)
                        overlaps.Add(inter);
                }
            }
            return overlaps;
        }
        private static List<IIntervalable> FindOverlapRange(IntervalNode node, IIntervalable interval)
        {
            if (node != null)
                return node.GetOverlap(interval);

            return new List<IIntervalable>();
        }

        private enum Direction : byte
        {
            LEFT, RIGHT
        }
    }

    public class IntervalableComparatorBySize : IComparer<IIntervalable>
    {
        public int Compare(IIntervalable x, IIntervalable y)
        {
            int cmp = x.Size - y.Size;
            if(cmp == 0)
            {
                cmp = x.Start - y.Start;
            }
            return cmp;
        }
    }

    public class IntervalableComparatorByPos : IComparer<IIntervalable>
    {
        public int Compare(IIntervalable x, IIntervalable y) => x.Start - y.Start;
    }
}
