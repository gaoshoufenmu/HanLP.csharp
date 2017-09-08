using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanLP.csharp.corpus.tag
{
    /// <summary>
    /// 地名角色标签
    /// </summary>
    public enum NS
    {
        /// <summary>
        /// 地名的上文 我【来到】中关园
        /// </summary>
        A,
        /// <summary>
        /// 地名的下文刘家村/和/下岸村/相邻
        /// </summary>
        B,
        /// <summary>
        /// 中国地名的第一个字
        /// </summary>
        C,
        /// <summary>
        /// 中国地名的第二个字
        /// </summary>
        D,
        /// <summary>
        /// 中国地名的第三个字
        /// </summary>
        E,
        /// <summary>
        /// 其他整个的地名
        /// </summary>
        G,
        /// <summary>
        /// 中国地名的后缀海/淀区
        /// </summary>
        H,
        /// <summary>
        /// 连接词刘家村/和/下岸村/相邻
        /// </summary>
        X,
        /// <summary>
        /// 其它非地名成分
        /// </summary>
        Z,

        /// <summary>
        /// 句子的开头
        /// </summary>
        S,
    }
}
