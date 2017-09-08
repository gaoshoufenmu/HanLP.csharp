using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanLP.csharp.corpus.tag
{
    public enum NT
    {
        /// <summary>
        /// 上文	[参与]亚太经合组织的活动
        /// </summary>
        A,
        /// <summary>
        /// 下文	中央电视台[报道]
        /// </summary>
        B,
        /// <summary>
        /// 连接词	北京电视台[和]天津电视台
        /// </summary>
        X,
        /// <summary>
        /// 特征词的一般性前缀	 北京[电影]学院
        /// </summary>
        C,
        /// <summary>
        /// 特征词的译名性前缀 	美国[摩托罗拉]公司
        /// </summary>
        F,
        /// <summary>
        /// 特征词的地名性前缀 	交通银行[北京]分行
        /// </summary>
        G,
        /// <summary>
        /// 特征词的机构名前缀	  [中共中央]顾问委员会
        /// </summary>
        H,
        /// <summary>
        /// 特征词的特殊性前缀	 [华谊]医院
        /// </summary>
        I,
        /// <summary>
        /// 特征词的简称性前缀 	[巴]政府
        /// </summary>
        J,
        /// <summary>
        /// 整个机构 [麦当劳]
        /// </summary>
        K,
        /// <summary>
        /// 方位词
        /// </summary>
        L,
        /// <summary>
        /// 数词 公交集团[五]分公司
        /// </summary>
        M,
        /// <summary>
        /// 单字碎片
        /// </summary>
        P,
        /// <summary>
        /// 符号
        /// </summary>
        W,
        /// <summary>
        /// 机构名的特征词	国务院侨务[办公室]
        /// </summary>
        D,
        /// <summary>
        /// 非机构名成份
        /// </summary>
        Z,

        /// <summary>
        /// 句子的开头
        /// </summary>
        S
    }
}
