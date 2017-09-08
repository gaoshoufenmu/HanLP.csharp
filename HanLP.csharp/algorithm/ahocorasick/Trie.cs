using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanLP.csharp.algorithm.ahocorasick
{
    public class Trie
    {
        /// <summary>
        /// 允许重叠
        /// </summary>
        private bool _allowOverlap = true;

        private bool _remainLonest = false;

        public bool AllowOverlap { get => _allowOverlap; set => _allowOverlap = value; }
        public bool RemainLongest { get => _remainLonest; set => _remainLonest = value; }

        private State _root;
        /// <summary>
        /// 是否建立了failure表
        /// </summary>
        private bool _failureStateConstructed;

        public Trie() { }

        public Trie(bool allowOverlap, bool remainLongest)
        {
            _allowOverlap = allowOverlap;
            _remainLonest = remainLongest;
        }

        /// <summary>
        /// 添加一个模式串
        /// </summary>
        /// <param name="keyword"></param>
        public void AddKeyword(string keyword)
        {
            if (string.IsNullOrEmpty(keyword)) return;

            var currState = this._root;
            foreach (var c in keyword)
                currState = currState.AddState(c);

            currState.AddEmit(keyword);
        }

        public void AddKeywords(ICollection<string> keywords)
        {
            foreach (var key in keywords)
                AddKeyword(key);
        }

        /// <summary>
        /// 最长分词
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public List<Token> Tokenize(string text)
        {
            var tokens = new List<Token>();
            var emits = Parse(text);
            if(_allowOverlap)       // 如果允许重叠，那么这里将重叠区间去掉
            {
                var intervals = emits.Select(e => (IIntervalable)e).ToList();
                var intervalTree = new IntervalTree(intervals);
                intervalTree.RemoveOverlaps(intervals);
            }

            var lastPos = -1;
            foreach(var emit in emits)
            {
                if (emit.Start - lastPos > 1)       // 如果上面的分词出现两个词不是无缝连接，那么将这两个词中的内容作为一个碎片token添加进列表
                {
                    var s = lastPos + 1;
                    tokens.Add(new FragmentToken(text.Substring(s, emit.Start - s)));
                }
                tokens.Add(new MatchToken(text.Substring(emit.Start, emit.Size), emit));
                lastPos = emit.End;
            }
            if(text.Length - lastPos > 1)       // 检测最后一个分词是否到达原文本末，若非，则最后剩下的作为一个碎片token
            {
                tokens.Add(new FragmentToken(text.Substring(lastPos + 1)));
            }
            return tokens;
        }

        public List<Emit> Parse(string text)
        {
            Check4ConstructedFailureStates();   // 是否建立失败转移表，如尚未，则建立之

            var currState = _root;
            var emits = new List<Emit>();

            for(int i = 0; i < text.Length; i++)                // 依次遍历每个字符，
            {
                currState = GetState(currState, text[i]);       // 根据当前状态进行转移，并尝试将当前状态的输出（如果有的话）存储到 emit 列表中
                StoreEmits(i, currState, emits);
            }

            if(!_allowOverlap)      // 如果不允许匹配的模式串在原文本中出现重叠，则需要移除重叠的模式串
            {
                var intervals = emits.Select(e => (IIntervalable)e).ToList();
                var intervalTree = new IntervalTree(intervals);
                intervalTree.RemoveOverlaps(intervals);
            }
            if(_remainLonest)
            {
                ReserveLongest(emits);
            }
            return emits;
        }

        private void Check4ConstructedFailureStates()
        {
            if (_failureStateConstructed) return;

            // 建立failure 表
            var queue = new Queue<State>();
            foreach(var depth1State in _root.TransitionStates)  // 首先建立根节点的直接子节点的失败转移状态，然后对这些深度为1的节点向下递归建立失败转移状态
            {
                depth1State.Failure = _root;
                queue.Enqueue(depth1State);
            }
            _failureStateConstructed = true;


            while(queue.Count > 0)      // bfs search
            {
                var state = queue.Dequeue();

                foreach(var key in state.TransitionKeys)
                {
                    var nextState = state.NextState(key, false);    // 这里的key都是state状态的成功转移所用的字符，所以转移状态nextState一定是成功的
                    queue.Enqueue(nextState);

                    // 要获取nextState的失败转移状态，向上获取祖先状态节点的失败转移状态
                    var ancestorFailure = state.Failure;
                    while (ancestorFailure.NextState(key, false) == null)   // 祖先状态的失败转移状态对此字符是否能成功转移，如果不能成功，继续向上获取祖先失败转移，直接根节点
                        ancestorFailure = ancestorFailure.Failure;

                    var nextFailure = ancestorFailure.NextState(key, false);
                    nextState.Failure = nextFailure;
                    nextState.AddEmits(nextFailure.Emits);
                }
            }
        }

        /// <summary>
        /// 根据给定字符获取下一个状态
        /// 如果失败，则按失败转移表转移
        /// </summary>
        /// <param name="curState"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        private static State GetState(State curState, char c)
        {
            var nextState = curState.NextState(c);
            while(nextState == null)
            {
                curState = curState.Failure;
                nextState = curState.NextState(c);
            }
            return nextState;
        }

        /// <summary>
        /// 将匹配成功的当前状态的匹配模式串添加到指定列表中
        /// </summary>
        /// <param name="end">匹配的模式串终点在原文本的位置 inclusive</param>
        /// <param name="currState">匹配成功的当前状态</param>
        /// <param name="list"></param>
        private static void StoreEmits(int end, State currState, List<Emit> list)
        {
            var emits = currState.Emits;
            if(emits != null && emits.Count > 0)
            {
                foreach (var e in emits)
                    list.Add(new Emit(end - e.Length + 1, end, e));
            }
        }

        private static void ReserveLongest(List<Emit> emits)
        {
            if (emits.Count < 2) return;

            var dict = new SortedDictionary<int, Emit>();   // 保存，在同一起点位置开始的最长词条
            foreach(var emit in emits)
            {
                if (dict.TryGetValue(emit.Start, out var pre))
                {
                    if (pre.Size < emit.Size)
                        dict[emit.Start] = emit;
                }
                else
                    dict[emit.Start] = emit;
            }

            if(dict.Count < 2)
            {
                emits.Clear();
                emits.AddRange(dict.Values);
                return;
            }

            var end_dict = new SortedDictionary<int, Emit>();
            foreach(var emit in dict.Values)
            {
                if (end_dict.TryGetValue(emit.End, out var pre))
                {
                    if (pre.Size < emit.Size)
                        end_dict[emit.End] = emit;
                }
                else
                    end_dict[emit.End] = emit;
            }
            emits.Clear();
            emits.AddRange(end_dict.Values);
        }
    }


    
}
