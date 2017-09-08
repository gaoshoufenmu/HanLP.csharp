/*         +-
 * --Start --> sign                            %
 *    |          |                          |------> percent_terminal
 *    |          |                          |
 *    |          |012...                    |  /             012...
 *    | 012...   |        012..             | -----> frac   --------------->|
 *    |          | <-------------|          |                               |
 *    |         \|/              |          |                               | 
 *    |-------> num_terminal --->|------------                              |
 *              |                |           |   ·                          |
 *              |                |           | -----> cdot_terminal         |
 *            . |                |           |           \|/                |
 *              |                |           |   012...   |                 |
 *              |                | <---------| <--------- | <-------------- |
 *             \|/       012..   |           |    :      /|\
 *       dec_terminal ---------->|           | -------> colon
 *       
 *  Notice: （1）上面这个状态转移图，会出现两种非num_terminal终止态混合的情况，比如字符串"0.1%"，或者"0.1/0.5",这些还算属于正常情况，
 *              但是"0.1/0.5%"，"0.1:5%"或"1.2·3.5%"等就属于异常情况，不过我们这里忽略异常情况
 *          （2）数字字符串的类型NumType，以所遇到的最大值为准，NumType.signed为特殊类型，可以与其他类型进行按位或操作
 *              
 * */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HanLP.csharp.Constants;

namespace HanLP.csharp.model
{

    public class NumDFA
    {
        private static DFAState _start;

        public static void Init()
        {
            var percent_terminal = new DFAState() { IsTerminal = true, Type = NumType.percent };
            var dec_terminal = new DFAState() { IsTerminal = true, NextDict = new Dictionary<StateType, DFAState>(), Type = NumType.dec };

            var cdot_terminal = new DFAState() { IsTerminal = true, NextDict = new Dictionary<StateType, DFAState>(), Type = NumType.cdot };

            var frac = new DFAState() { NextDict = new Dictionary<StateType, DFAState>(), Type = NumType.frac };

            var colon = new DFAState() { NextDict = new Dictionary<StateType, DFAState>(), Type = NumType.colon };

            var num_terminal = new DFAState() { IsTerminal = true, NextDict = new Dictionary<StateType, DFAState>(), Type = NumType.num };

            num_terminal.NextDict.Add(StateType.percent, percent_terminal);
            num_terminal.NextDict.Add(StateType.num, num_terminal);
            num_terminal.NextDict.Add(StateType.dec, dec_terminal);
            num_terminal.NextDict.Add(StateType.cdot, cdot_terminal);
            num_terminal.NextDict.Add(StateType.frac, frac);
            num_terminal.NextDict.Add(StateType.colon, colon);

            dec_terminal.NextDict.Add(StateType.num, num_terminal);
            frac.NextDict.Add(StateType.num, num_terminal);

            cdot_terminal.NextDict.Add(StateType.num, num_terminal);

            colon.NextDict.Add(StateType.num, num_terminal);

            var sign = new DFAState() { NextDict = new Dictionary<StateType, DFAState>(), Type = NumType.signed };
            sign.NextDict.Add(StateType.num, num_terminal);

            _start = new DFAState() { NextDict = new Dictionary<StateType, DFAState>() };
            _start.NextDict.Add(StateType.num, num_terminal);
            _start.NextDict.Add(StateType.signed, sign);
        }

        public static NumMatch Match(string input, int start)
        {
            var nm = new NumMatch();
            var typeCache = NumType.unknown;
            if (string.IsNullOrEmpty(input) || start < 0) return nm;

            DFAState cur = _start;
            DFAState pre = null;
            for(int i = start; i < input.Length; i++)
            {
                pre = cur;
                cur = cur.Process(input[i]);
                if (cur == null)
                {
                    if (!pre.IsTerminal)    // 上一个状态非终止状态
                        nm.Length--;
                    return nm;
                }

                if (cur.Type == NumType.signed)
                    nm.IsSigned = true;
                nm.Length++;
                if(cur.IsTerminal)
                {
                    if (typeCache > nm.Type)
                        nm.Type = typeCache;
                }
                if (cur.Type > nm.Type)
                {
                    if (cur.IsTerminal)
                        nm.Type = cur.Type;
                    else
                        typeCache = cur.Type;
                }
            }
            return nm;
        }

        

        public class DFAState
        {
            public NumType Type;
            public bool IsTerminal;
            public Dictionary<StateType, DFAState> NextDict;

            
            public DFAState Process(char c)
            {
                if (NextDict != null)
                {
                    var stateType = GetNextState(c);
                    if (stateType == StateType.unknown) return null;

                    if (NextDict.TryGetValue(stateType, out var state))
                    {
                        return state;
                    }
                }
                return null;
            }

            public static StateType GetNextState(char c)
            {
                if (ALL_NUMs.Contains(c)) return StateType.num;
                if ("∶：:比".Contains(c)) return StateType.colon;
                if ("／/".Contains(c)) return StateType.frac;
                if ("±+—-＋".Contains(c)) return StateType.signed;
                if ("．.".Contains(c)) return StateType.dec;
                if ('·' == c) return StateType.cdot;
                if ("%％‰".Contains(c)) return StateType.percent;
                return StateType.unknown;
            }
        }

        public enum StateType
        {
            unknown,
            signed,
            colon,
            cdot,
            dec,
            frac,
            percent,
            num,
        }
    }

    public enum NumType
    {
        unknown = 0x00,
        /// <summary>
        /// 带符号的
        /// </summary>
        signed = 0x01,
        /// <summary>
        /// 普通数值
        /// </summary>
        num = 0x02,
        /// <summary>
        /// 小数
        /// </summary>
        dec = 0x04,
        /// <summary>
        /// 含cdot的数字字符串
        /// </summary>
        cdot = 0x06,
        /// <summary>
        /// 分数
        /// </summary>
        frac = 0x08,
        /// <summary>
        /// 含colon的数字字符串
        /// </summary>
        colon = 0x0a,
        /// <summary>
        /// 百(千)分比
        /// </summary>
        percent = 0x0c,
    }

    /// <summary>
    /// 数值匹配结果
    /// </summary>
    public class NumMatch
    {
        /// <summary>
        /// 匹配长度
        /// </summary>
        public int Length;
        /// <summary>
        /// 匹配的数字字符串的类型
        /// </summary>
        public NumType Type;
        /// <summary>
        /// 是否含有符号
        /// </summary>
        public bool IsSigned;
    }
}
