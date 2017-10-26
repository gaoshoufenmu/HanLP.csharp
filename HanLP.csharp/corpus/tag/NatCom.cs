using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanLP.csharp.corpus.tag
{
    public enum NatCom
    {
        /// <summary>
        /// 未知
        /// </summary>
        U,
        /// <summary>
        /// 主公司地区
        /// </summary>
        MA,
        /// <summary>
        /// 公司字号
        /// </summary>
        C,
        /// <summary>
        /// 集团所在地区
        /// </summary>
        GD,
        /// <summary>
        /// 行业，经营特点
        /// </summary>
        T,
        OF,
        /// <summary>
        /// 主公司形式，如：集团
        /// </summary>
        OF_JT,
        /// <summary>
        /// 股份
        /// </summary>
        OF_GF,
        /// <summary>
        /// 责任
        /// </summary>
        OF_ZR,
        /// <summary>
        /// 有限
        /// </summary>
        OF_YX,
        /// <summary>
        /// 公司
        /// </summary>
        OF_GS,
        /// <summary>
        /// 符号
        /// </summary>
        W,
        /// <summary>
        /// 英文
        /// </summary>
        E,
        /// <summary>
        /// 主公司组织类型，如：中心，招待所，公司
        /// </summary>
        MO,
        ///// <summary>
        ///// 错误的地区名，会引起混淆的，实际中可能会忽略
        ///// </summary>
        //FA,
        /// <summary>
        /// 分公司地区
        /// </summary>
        BA,
        /// <summary>
        /// 主公司组织类型
        /// </summary>
        BO
    }
}
