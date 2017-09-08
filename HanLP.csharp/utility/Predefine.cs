using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HanLP.csharp.utility
{
    /// <summary>
    /// 预定义一些约定好的范围
    /// </summary>
    class Predefine
    {
        /// <summary>
        /// 二进制文件后缀
        /// </summary>
        public const string BIN_EXT = ".bin";
        /// <summary>
        /// 保存反转数据的文件后缀
        /// </summary>
        public const string REVERSE_EXT = ".reverse";
        /// <summary>
        /// 浮点数正则
        /// 必须有整数部分，负号可有可无，可以没有小数部分，但如果有小数部分，则必须有小数点，且后跟至少一个数字
        /// </summary>
        public static readonly Regex FLOAT_NUM_REG = new Regex(@"^(-?\d+)(\.\d+)?$", RegexOptions.Compiled);
    }
}
