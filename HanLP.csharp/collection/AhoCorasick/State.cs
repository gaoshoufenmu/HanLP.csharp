using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanLP.csharp.collection.AhoCorasick
{
    public class State
    {
        /// <summary>
        /// 模式串长度，也是本状态深度
        /// </summary>
        protected int depth;
        /// <summary>
        /// 失败后转移的
        /// 一个状态只有一个失败转移状态
        /// </summary>
        private State _failure;
        /// <summary>
        /// 本状态匹配成功后的输出
        /// 可能是多个模式串（对应的下标）
        /// </summary>
        private ISet<int> _emits;
        /// <summary>
        /// goto 表
        /// 必须要对key进行排序
        /// </summary>
        private SortedDictionary<char, State> _success = new SortedDictionary<char, State>();
        /// <summary>
        /// 双数组中的对应下标
        /// </summary>
        private int _index;

        public State() { }

        public State(int depth) => this.depth = depth;

        public int Depth { get => depth; }
        public SortedDictionary<char, State> Success { get => _success; }
        public int Index { get => _index; set => _index = value; }
        public State Failure { get => _failure; set => _failure = value; }

        public ISet<int> Emits { get => _emits; }

        public bool IsTerminated() => depth > 0 && _emits != null;

        public int GetFirstEmit() => _emits.FirstOrDefault();

        /// <summary>
        /// 添加一个模式串的下标
        /// </summary>
        /// <param name="key"></param>
        public void AddEmit(int key)
        {
            if (_emits == null)
                _emits = new SortedSet<int>(new IntReverseComparer());
            _emits.Add(key);
        }

        /// <summary>
        /// 添加一组模式串的下标
        /// </summary>
        /// <param name="emits"></param>
        public void AddEmits(IEnumerable<int> emits)
        {
            if (emits == null) return;

            if (_emits == null) _emits = new SortedSet<int>(new IntReverseComparer());

            foreach(var e in emits)
                _emits.Add(e);
        }

        /// <summary>
        /// 转移到下一个状态
        /// </summary>
        /// <param name="ch"></param>
        /// <param name="ignoreRootState">指示转移失败时如何处理：true -> 忽略root，返回null；false -> 不忽略root，如果是root转移失败，返回root，否则返回null</param>
        /// <returns></returns>
        public State NextState(char ch, bool ignoreRootState = false)
        {
            if(_success.TryGetValue(ch, out var next))
                return next;        // 转移成功

            // 转移失败
            if (!ignoreRootState)    // 如果不忽略根节点
            {
                if (depth == 0)     // 如果是root节点
                {
                    return this;
                }
            }
            return null;
        }


        public State AddSuccState(char ch)
        {
            var nextState = NextState(ch, true);
            if(nextState == null)
            {
                nextState = new State(this.depth + 1);
                _success.Add(ch, nextState);
            }
            return nextState;
        }

        public ICollection<State> GetSuccStates() => _success.Values;
        public ICollection<char> GetTransitions() => _success.Keys;
    }
}
