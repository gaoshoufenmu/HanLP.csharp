using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanLP.csharp
{
    public static class Constants
    {
        //! ----------------------------- CT: Char Type  ------------------------------
        public const byte CT_SENTENCE_BEGIN = 1;
        public const byte CT_SENTENCE_END = 4;

        /// <summary>
        /// 单字节字符类型
        /// </summary>
        public const byte CT_SINGLE = 5;
        /// <summary>
        /// 分隔符类型"!,.?()[]{}+=
        /// </summary>
        public const byte CT_DELIMITER = CT_SINGLE + 1;
        /// <summary>
        /// 中文字符类型
        /// </summary>
        public const byte CT_CHINESE = CT_SINGLE + 2;
        /// <summary>
        /// 字母类型
        /// </summary>
        public const byte CT_LETTER = CT_SINGLE + 3;
        /// <summary>
        /// 数字
        /// </summary>
        public const byte CT_NUM = CT_SINGLE + 4;
        /// <summary>
        /// 序号
        /// </summary>
        public const byte CT_INDEX = CT_SINGLE + 5;
        /// <summary>
        /// 量词
        /// </summary>
        public const byte CT_QUANT = CT_SINGLE + 6;
        /// <summary>
        /// 其他
        /// </summary>
        public const byte CT_OTHER = CT_SINGLE + 12;

        //! -----------------------------  特殊意义的字符集合  ------------------------------
        public const string ALL_NUMs = HALF_ANGLE_NUMS + FULL_ANGLE_NUMS + ALL_CHINESE_NUMs;
        public const string HALF_ANGLE_NUMS = "0123456789";
        public const string FULL_ANGLE_NUMS = "０１２３４５６７８９";
        public const string ALL_CHINESE_NUMs = CHINESE_SINGLE_DIGITS + CHINESE_NUM_UNITS;
        public const string CHINESE_SINGLE_DIGITS = "零○〇一二两三四五六七八九壹贰叁肆伍陆柒捌玖";
        public const string CHINESE_NUM_UNITS = "十廿百千万亿拾佰仟";
        public const string CHINEST_NUM_PREFIX = "几数第上成";
        public const string NUM_DELIMITER = ":：∶·．.／/点";
        public const string ALL_SINGLE_DIGITS = HALF_ANGLE_NUMS + FULL_ANGLE_NUMS + CHINESE_SINGLE_DIGITS;
        public const string TIANGAN = "甲乙丙丁戊己庚辛壬癸";
        public const string DIZHI = "子丑寅卯辰巳午未申酉戌亥";


        //! ------------------------------- 词性对应的未知情况的等效字符串 ---------------------------------
        /// <summary>
        /// 地址ns
        /// </summary>
        public const string TAG_PLACE = "未##地";
        /// <summary>
        /// 句子开始 begin
        /// </summary>
        public const string TAG_BEGIN = "始##始";
        /// <summary>
        /// 其他
        /// </summary>
        public const string TAG_OTHER = "未##它";
        /// <summary>
        /// 团体名词 nt
        /// </summary>
        public const string TAG_GROUP = "未##团";
        /// <summary>
         /// 数词 m
         /// </summary>
        public const string TAG_NUMBER = "未##数";
        /// <summary>
         /// 数量词 mq （现在觉得应该和数词同等处理，比如一个人和一人都是合理的）
         /// </summary>
        public const string TAG_QUANTIFIER = "未##量";
        /// <summary>
         /// 专有名词 nx
         /// </summary>
        public const string TAG_PROPER = "未##专";
        /// <summary>
         /// 时间 t
         /// </summary>
        public const string TAG_TIME = "未##时";
        /// <summary>
         /// 字符串 x
         /// </summary>
        public const string TAG_CLUSTER = "未##串";
        /// <summary>
         /// 结束 end
         /// <summary>
        public const string TAG_END = "末##末";
        /// <summary>
         /// 人名 nr
        /// </summary>
        public const string TAG_PEOPLE = "未##人";

        /// <summary>
        /// 总词频
        /// </summary>
        public const int MAX_FREQUENCY = 25146057;
        /// <summary>
        /// 平滑因子
        /// </summary>
        public const double SMOOTHING_FACTOR = 1.0 / MAX_FREQUENCY + 0.00001;
        /// <summary>
        /// 平滑参数
        /// </summary>
        public const double SMOOTHING_PARAM = 0.1;
    }
}
