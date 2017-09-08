using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using HanLP.csharp.interfaces;
using HanLP.csharp.corpus.io;

namespace HanLP.csharp.collection.AhoCorasick
{
    /// <summary>
    /// Aho-Corasick 双数组 Trie 自动机
    /// 借助State类实现。goto表和failure表和输出表均使用数组存储
    /// </summary>
    public class ACDoubleArrayTrie<V>
    {
        /// <summary>
        /// 双数组值
        /// </summary>
        protected int[] _check;
        /// <summary>
        /// 双数组之base
        /// </summary>
        protected int[] _base;
        /// <summary>
        /// fail 表
        /// </summary>
        int[] _fail;
        /// <summary>
        /// 输出表
        /// </summary>
        int[][] _output;
        /// <summary>
        /// 保存value
        /// </summary>
        V[] _v;
        /// <summary>
        /// 每个key长度
        /// 由于key字符串被拆成字符并以节点形式存储在Trie中，所以需要一个数组来记录每个原始key字符串的长度
        /// </summary>
        int[] _l;
        /// <summary>
        /// base和check数组大小（具有有效值的最大下标 +1）
        /// </summary>
        int _size;
        /// <summary>
        /// 存储的词条数量
        /// </summary>
        public int Size => _v == null ? 0 : _v.Length;

        /// <summary>
        /// 用指定文本匹配（多模式串）
        /// </summary>
        /// <param name="input"></param>
        /// <returns>匹配结果列表</returns>
        public List<AC_Hit<V>> Match(string input)
        {
            int position = 1;       // 匹配文本下一个读取位置（初始化为 1）
            int currState = 0;      // 初始状态 root -> 0
            var emits = new List<AC_Hit<V>>();
            for (var i = 0; i < input.Length; i++)
            {
                currState = GetState(currState, input[i]);
                StoreEmit(position, currState, emits);
                position++;
            }
            return emits;
        }

        /// <summary>
        /// 多模式串匹配
        /// </summary>
        /// <param name="chars"></param>
        /// <param name="action"></param>
        public void Match(char[] chars, Action<int, int, V> action)
        {
            int position = 1;
            int currState = 0;
            for (int i = 0; i < chars.Length; i++)
            {
                currState = GetState(currState, chars[i]);
                var arr = _output[currState];
                if (arr != null)
                {
                    foreach (var a in arr)
                        action(position - _l[a], position, _v[a]);
                }
                position++;
            }
        }

        /// <summary>
        /// 用指定文本匹配（多模式串），并在每次匹配成功时调用一次命中方法，以通知外部关于命中的信息
        /// </summary>
        /// <param name="input"></param>
        /// <param name="processor"></param>
        public void Match(string input, Action<int, int, V> action) => Match(input.ToCharArray(), action);

        /// <summary>
        /// 精确匹配（完全匹配）
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int ExactMatch(string key) => ExactMatch(key, 0, key.Length, 0);
        /// <summary>
        /// 精确匹配（完全匹配）
        /// </summary>
        /// <param name="key"></param>
        /// <param name="start"></param>
        /// <param name="len"></param>
        /// <param name="nodePos"></param>
        /// <returns></returns>
        private int ExactMatch(string key, int start, int len, int nodePos)
        {
            if (start < 0 || start > key.Length) start = 0;
            if (nodePos < 0 || nodePos >= _size) nodePos = 0;
            var end = start + len;
            if (end > key.Length)
                end = key.Length;

            int b = _base[nodePos];
            int p;
            for (int i = start; i < end; i++)
            {
                p = b + key[i] + 1;
                if (p < _size && b == _check[p])
                    b = _base[p];
                else
                    return -1;
            }

            p = b;
            int n = _base[p];
            if (b == _check[p] && n < 0)
                return -n - 1;
            return -1;
        }

        public V GetOrDefault(string key, V v = default(V))
        {
            var index = ExactMatch(key);
            if (index >= 0)
                return _v[index];
            return v;
        }
        public V this[string key]
        {
            get
            {
                var index = ExactMatch(key);
                if (index >= 0)
                    return _v[index];
                throw new KeyNotFoundException("key doesn't exist: " + key);
            }
        }

        public V Get(int index)
        {
            if (index < 0 || index > _v.Length) throw new IndexOutOfRangeException("index is out of range: " + index);

            return _v[index];
        }

        public bool Set(string key, V value)
        {
            var index = ExactMatch(key);
            if (index >= 0)
            {
                _v[index] = value;
                return true;
            }
            return false;
        }

        private int GetState(int currState, char ch)
        {
            var newState = Transition(ch, currState);
            while (newState == -1)
            {
                currState = _fail[currState];       // 失败状态转移
                newState = Transition(ch, currState);
            }
            // 最终newState要么是成功转移状态，要么是回到root状态（0）
            return newState;
        }

        protected int Transition(char c, int nodePos)
        {
            var b = _base[nodePos];
            int p = b + c + 1;
            if (p < _size && b == _check[p])
                return p;       // success

            if (nodePos == 0) return 0;     // 如果是root状态，则依然保存root状态
            return -1;      // failure
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position">匹配文本下一个读取位置</param>
        /// <param name="currState">当前匹配状态</param>
        /// <param name="emits"></param>
        private void StoreEmit(int position, int currState, List<AC_Hit<V>> emits)
        {
            var hits = _output[currState];      // 当前状态的输出
            if (hits != null)                    // 对应输出是否存在（多模式串）
            {
                foreach (var hit in hits)       // 遍历各模式串（的下标）
                    emits.Add(new AC_Hit<V>(position - _l[hit], position, _v[hit]));
            }
        }

        private AC_DAT_Helper _helper;

        public void Build(SortedDictionary<string, V> dict)
        {
            _v = new V[dict.Count];
            _l = new int[dict.Count];
            _helper = new AC_DAT_Helper(dict.Count);

            var keys = new string[dict.Count];


            int idx = 0;
            foreach (var p in dict)
            {
                _v[idx] = p.Value;
                keys[idx] = p.Key;
                _l[idx] = p.Key.Length;
                idx++;
            }

            for (int i = 0; i < keys.Length; i++)
            {
                var currState = _helper.root;
                var key = keys[i];
                for (int j = 0; j < key.Length; j++)
                    currState = currState.AddSuccState(key[j]);

                currState.AddEmit(i);
            }

            BuildDoubleArrayTrie(keys);
            ConstructFailStates();
            _helper = null;
            ShrinkMem();
        }



        private void BuildDoubleArrayTrie(ICollection<string> keys)
        {
            Resize(65536 * 32);
            _base[0] = 1;

            var siblings = FetchSiblings(_helper.root);
            Insert(siblings);
        }

        private int Resize(int newSize)
        {
            var base2 = new int[newSize];
            var check2 = new int[newSize];
            var used2 = new bool[newSize];

            if (_helper.allocSize > 0)
            {
                Array.Copy(_base, base2, _helper.allocSize);
                Array.Copy(_check, check2, _helper.allocSize);
                Array.Copy(_helper.used, used2, _helper.allocSize);
            }
            _base = base2;
            _check = check2;
            _helper.used = used2;
            return _helper.allocSize = newSize;
        }

        private List<KeyValuePair<int, State>> FetchSiblings(State parent)
        {
            var siblings = new List<KeyValuePair<int, State>>(parent.Success.Count + 1);
            if (parent.IsTerminated())     // 父状态本身也表示一个终结词
            {
                var synonym = new State(-parent.Depth - 1);     // 此节点是一个特殊的子节点，表示父节点本身所代表的终结词，注意这个节点的depth值为负，那么其IsTerminated方法返回false，要注意！！！
                synonym.AddEmit(parent.GetFirstEmit());
                siblings.Add(new KeyValuePair<int, State>(0, synonym));
            }

            foreach (var p in parent.Success)
                siblings.Add(new KeyValuePair<int, State>(p.Key + 1, p.Value));       // p.Key + 1 保证了即使 '\0' 的子节点也不与上面的 synonym 节点冲突

            return siblings;
        }


        private int Insert(List<KeyValuePair<int, State>> siblings)
        {
            int begin = 0;
            int pos = Math.Max(siblings[0].Key + 1, _helper.nextCheckPos) - 1;
            int nonzero_num = 0;
            int first = 0;

            if (_helper.allocSize <= pos + 1)
                Resize(pos + 2);

            var lastCode = siblings[siblings.Count - 1].Key;
            outer:
            while (true)
            {
                pos++;
                if (_helper.allocSize <= pos)
                    Resize(pos + 1);

                if (_check[pos] != 0)
                {
                    nonzero_num++;
                    continue;
                }
                else if (first == 0)
                {
                    _helper.nextCheckPos = pos;     // 找到下一个
                    first = 1;
                }

                begin = pos - siblings[0].Key;      // 
                if (_helper.allocSize <= begin + lastCode)
                {
                    var finishPercent_Reciprocal = 1.0 * _helper.keySize / (_helper.progress + 1);     // 完成百分比的倒数， + 1 是为了防止除 0 异常
                    double l = 1.05 > finishPercent_Reciprocal ? 1.05 : finishPercent_Reciprocal;
                    Resize((int)(_helper.allocSize * l));
                }

                if (_helper.used[begin]) continue;

                for (int i = 0; i < siblings.Count; i++)
                    if (_check[begin + siblings[i].Key] != 0)       // 数组位置已占用
                        goto outer;

                break;
            }

            if (1.0 * nonzero_num / (pos - _helper.nextCheckPos + 1) >= 0.95)    // 占用超过95%，则 nextCheckPos 到 pos 之间的数组不再使用
                _helper.nextCheckPos = pos;

            _helper.used[begin] = true;

            // 数组实际占用空间
            if (_size < begin + lastCode + 1)
                _size = begin + lastCode + 1;

            foreach (var p in siblings)
                _check[begin + p.Key] = begin;

            for (var i = 0; i < siblings.Count; i++)
            {
                var new_siblings = FetchSiblings(siblings[i].Value);
                if (new_siblings.Count == 0)     // 是叶节点
                {
                    _base[begin + siblings[i].Key] = (-siblings[i].Value.GetFirstEmit() - 1);
                    _helper.progress++;
                }
                else
                {
                    int h = Insert(new_siblings);
                    _base[begin + siblings[i].Key] = h;
                }
                siblings[i].Value.Index = begin + siblings[i].Key;      // 设置当前节点在数组中的位置
            }
            return begin;
        }

        private void ConstructFailStates()
        {
            _fail = new int[_size + 1];
            _fail[1] = _base[0];
            _output = new int[_size + 1][];

            var queue = new Queue<State>();
            // 深度为1 的状态节点，failure状态均为根节点
            foreach (var depth1State in _helper.root.GetSuccStates())
            {
                depth1State.Failure = _helper.root;
                _fail[depth1State.Index] = _helper.root.Index;
                queue.Enqueue(depth1State);
                ConstructOutput(depth1State);
            }

            //
            while (queue.Count > 0)
            {
                var state = queue.Dequeue();

                foreach (var key in state.GetTransitions())
                {
                    var nextState = state.NextState(key);   // 这里nextState不可能为null，因为key来自字典词条的字符值，而不是匹配时任意给的
                    queue.Enqueue(nextState);

                    // 想要获取nextState的失败状态，递归向上获取祖先节点的失败状态，并且使得这个状态对当前 key 字符的转换状态不为 null
                    var ancestorFailure = state.Failure;
                    while (ancestorFailure.NextState(key) == null)  // 根节点转移失败时返回根节点自身
                        ancestorFailure = ancestorFailure.Failure;

                    // 此时failure.NextState(key) != null 是成立的，否则不可能跳出while循环
                    // 此即 nextState 失败状态
                    var nextFailure = ancestorFailure.NextState(key);

                    //// 上面这段求 nextState 的失败状态可以修改为如下
                    //var nextFailure = failure.NextState(key);     // 先求父节点的失败状态对当前 key 字符的转换状态，如果成功，此即要求的 nextState 的 失败状态 
                    //while(nextFailure == null)                    // 否则，继续向上求更上一层的失败状态对当前key 字符的转移状态，直到这个转移状态不为null
                    //{
                    //    failure = failure.Failure;
                    //    nextFailure = failure.NextState(key);
                    //}

                    nextState.Failure = nextFailure;
                    _fail[nextState.Index] = nextFailure.Index;
                    nextState.AddEmits(nextFailure.Emits);
                    ConstructOutput(nextState);
                }
            }
        }

        private void ConstructOutput(State state)
        {
            var emits = state.Emits;
            if (emits != null && emits.Count > 0)
            {
                var output = new int[emits.Count];
                int i = 0;
                foreach (var e in emits)
                {
                    output[i++] = e;
                }
                _output[state.Index] = output;
            }
        }

        private void ShrinkMem()
        {
            var nbase = new int[_size + 65535];
            Array.Copy(_base, nbase, _size);
            _base = nbase;
            var ncheck = new int[_size + 65535];
            Array.Copy(_check, ncheck, _size);
            _check = ncheck;
        }

        public bool Save(string path)
        {
            var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            try
            {
                return Save(fs);
            }
            catch (Exception e)
            {
                return false;
            }
            finally
            {
                fs.Close();
            }
        }

        /// <summary>
        /// 保存Trie中与Key相关的信息数据
        /// </summary>
        /// <param name="fs"></param>
        /// <returns></returns>
        public bool Save(FileStream fs)
        {
            var bytes = BitConverter.GetBytes(_size);
            fs.Write(bytes, 0, 4);

            for (int i = 0; i < _size; i++)
            {
                bytes = BitConverter.GetBytes(_base[i]);
                fs.Write(bytes, 0, 4);
                bytes = BitConverter.GetBytes(_check[i]);
                fs.Write(bytes, 0, 4);
                bytes = BitConverter.GetBytes(_fail[i]);
                fs.Write(bytes, 0, 4);
                var output = _output[i];
                if (output == null)
                    fs.Write(new byte[4], 0, 4);
                else
                {
                    bytes = BitConverter.GetBytes(output.Length);
                    fs.Write(bytes, 0, 4);
                    for (int j = 0; j < output.Length; j++)
                        fs.Write(BitConverter.GetBytes(output[j]), 0, 4);
                }
            }

            fs.Write(BitConverter.GetBytes(_l.Length), 0, 4);
            for (int i = 0; i < _l.Length; i++)
            {
                fs.Write(BitConverter.GetBytes(_l[i]), 0, 4);
            }
            return true;
        }

        public bool Load(string path, V[] values) => Load(ByteArray.Create(path), values);

        public bool Load(ByteArray ba, V[] values)
        {
            if (ba == null) return false;

            _size = ba.NextInt();
            _base = new int[_size + 65535];     // 多预留一些空间，防止越界
            _check = new int[_size + 65535];
            _fail = new int[_size + 65535];
            _output = new int[_size + 65535][];

            int len;
            for (int i = 0; i < _size; i++)
            {
                _base[i] = ba.NextInt();
                _check[i] = ba.NextInt();
                _fail[i] = ba.NextInt();

                len = ba.NextInt();
                _output[i] = new int[len];
                for (int j = 0; j < len; j++)
                    _output[i][j] = ba.NextInt();
            }
            len = ba.NextInt();
            _l = new int[len];
            for (int i = 0; i < len; i++)
                _l[i] = ba.NextInt();

            _v = values;
            return true;
        }
    }

    public class AC_DAT_Helper
    {
        public State root = new State();
        public bool[] used;

        public int allocSize;
        public int progress;
        public int nextCheckPos;
        public int keySize;

        public AC_DAT_Helper(int keySize) => this.keySize = keySize;
    }


    public class AC_Hit<V>
    {
        /// <summary>
        /// 模式串在原始文本中的起始位置
        /// </summary>
        public int Begin { get; private set; }
        /// <summary>
        /// 模式串在原始文本中的截止位置
        /// </summary>
        public int End { get; private set; }
        /// <summary>
        /// 模式串值
        /// </summary>
        public V Value { get; private set; }

        public AC_Hit(int begin, int end, V value)
        {
            Begin = begin;
            End = end;
            Value = value;
        }

        public override string ToString() => $"{Begin}:{End}={Value}";
    }
            
}
