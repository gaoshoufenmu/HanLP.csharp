using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HanLP.csharp.collection;
using HanLP.csharp.utility;

namespace HanLP.csharp.algorithm.ahocorasick
{
    public class State
    {
        /// <summary>
        /// 模式串的长度，也是当前状态节点的深度（根节点深度为0）
        /// </summary>
        private int _depth;
        /// <summary>
        /// fail后的转移状态
        /// 每个状态有且仅有一个失败转移状态
        /// </summary>
        private State _failure = null;
        /// <summary>
        /// 包含的模式串
        /// </summary>
        private SortedSet<string> _emits;
        /// <summary>
        /// 成功转移表
        /// </summary>
        private SortedDictionary<char, State> _success;

        public int Depth => _depth;
        /// <summary>
        /// fail后的转移状态
        /// 每个状态有且仅有一个失败转移状态
        /// </summary>
        public State Failure { get => _failure; set => _failure = value; }
        public SortedDictionary<char, State> Success => _success;
        public SortedSet<string> Emits => _emits;
        /// <summary>
        /// 可成功转移的key集合
        /// </summary>
        public ICollection<char> TransitionKeys => _success.Keys;
        /// <summary>
        /// 可成功转移的状态集合
        /// </summary>
        public ICollection<State> TransitionStates => _success.Values;


        public State() { }

        public State(int depth)=>_depth = depth;

        public void AddEmit(string emit)
        {
            if (_emits == null)
                _emits = new SortedSet<string>(StrComparer.Default);
            _emits.Add(emit);
        }

        public void AddEmits(IEnumerable<string> emits)
        {
            if (emits == null) return;
            if (_emits == null)
                _emits = new SortedSet<string>(StrComparer.Default);

            foreach (var e in emits)
                _emits.Add(e);
        }

        /// <summary>
        /// 根据给定输入字符增加一个成功转移状态
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public State AddState(char c)
        {
            var nextState = NextState(c);
            if(nextState == null)
            {
                nextState = new State(_depth + 1);
                _success.Add(c, nextState);
            }
            return nextState;
        }

        /// <summary>
        /// 获取转移状态
        /// 默认忽略根节点的自转移行为
        /// </summary>
        /// <param name="c"></param>
        /// <param name="ignoreRootState">是否忽略根节点的自转移，默认忽略</param>
        /// <returns></returns>
        public State NextState(char c, bool ignoreRootState = true)
        {
            if(_success.TryGetValue(c, out var nextState))
                return nextState;       // 转移成功，直接返回转移状态

            if (!ignoreRootState && _depth == 0)    // 如果当前处在根节点，并且不忽略根节点自转移，则返回根节点自身
                return this;

            return null;        // 转移失败
        }


        public override string ToString() =>
            $"depth={_depth},emits=({TextUtil.Join2Str("/", _emits)}),success=({TextUtil.Join2Str(_success.Keys)})";
    }
}
