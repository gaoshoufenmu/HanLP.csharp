using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanLP.csharp.algorithm.ahocorasick
{
    public interface IIntervalable : IComparable
    {
        /// <summary>
        /// 起点 inclusive
        /// </summary>
        int Start { get; }
        /// <summary>
        /// 终点 inclusive
        /// </summary>
        int End { get; }
        int Size { get; }
    }
    public class Interval : IIntervalable
    {
        private int _start;
        private int _end;
        public int Start => _start;
        public int End => _end;
        public int Size => _end - _start + 1;

        public Interval(int start, int end)
        {
            _start = start;
            _end = end;
        }

        /// <summary>
        /// 是否与指定区间有（部分）重叠
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool IsOverlappedWith(Interval other) => _start <= other._end || _end >= other._start;
        /// <summary>
        /// 是否被指定区间包含
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool IsContainedBy(Interval other) => _start >= other._start && _end <= other._start;
        /// <summary>
        /// 是否包含指定一点
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool ContainsPoint(int point) => _start <= point && point <= _end;

        public override int GetHashCode() => _start % 100 + _end % 100;

        public int CompareTo(object obj)
        {
            if (obj is IIntervalable)
            {
                var other = (IIntervalable)obj;
                int cmp = _start - other.Start;
                return cmp != 0 ? cmp : _end - other.End;       // compare start and if start is equal then compare end
            }
            return -1;
        }
        public override string ToString() => $"{_start}:{_end}";
    }
}
