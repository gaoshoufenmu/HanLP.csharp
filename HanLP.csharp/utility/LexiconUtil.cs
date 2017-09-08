using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HanLP.csharp.corpus.tag;

namespace HanLP.csharp.utility
{
    /// <summary>
    /// 词典和词性相关的工具类
    /// </summary>
    public class LexiconUtil
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="customNats">自定义词性集合</param>
        /// <returns></returns>
        public static Nature Str2Nat(string name, HashSet<Nature> customNats)
        {
            if (Enum.TryParse<Nature>(name, out var nat)) return nat;

            //TODO: handle custom Nature enums
            return Nature.nz;
        }
    }
}
