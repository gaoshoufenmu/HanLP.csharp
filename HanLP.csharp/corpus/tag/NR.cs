using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanLP.csharp.corpus.tag
{
    /// <summary>
    /// 人名标签
    /// </summary>
    public enum NR
    {
        /// <summary>
        /// 	Pf	姓氏	【张】华平先生
        /// </summary>
        B,

        /// <summary>
        /// 	Pm	双名的首字	张【华】平先生
        /// </summary>
        C,

        /// <summary>
        /// 	Pt	双名的末字	张华【平】先生
        /// </summary>
        D,

        /// <summary>
        /// 	Ps	单名	张【浩】说：“我是一个好人”
        /// </summary>
        E,

        /// <summary>
        /// 	Ppf	前缀	【老】刘、【小】李
        /// </summary>
        F,

        /// <summary>
        /// 	Plf	后缀	王【总】、刘【老】、肖【氏】、吴【妈】、叶【帅】
        /// </summary>
        G,

        /// <summary>
        /// 	Pp	人名的上文	又【来到】于洪洋的家。
        /// </summary>
        K,

        /// <summary>
        /// 	Pn	人名的下文	新华社记者黄文【摄】
        /// </summary>
        L,

        /// <summary>
        /// 	Ppn	两个中国人名之间的成分	编剧邵钧林【和】稽道青说
        /// </summary>
        M,

        /// <summary>
        /// 	Ppf	人名的上文和姓成词	这里【有关】天培的壮烈
        /// </summary>
        U,

        /// <summary>
        /// 	Pnw	三字人名的末字和下文成词	龚学平等领导, 邓颖【超生】前
        /// </summary>
        V,

        /// <summary>
        /// 	Pfm	姓与双名的首字成词	【王国】维、
        /// </summary>
        X,

        /// <summary>
        /// 	Pfs	姓与单名成词	【高峰】、【汪洋】
        /// </summary>
        Y,

        /// <summary>
        /// 	Pmt	双名本身成词	张【朝阳】
        /// </summary>
        Z,

        /// <summary>
        /// 	Po	以上之外其他的角色
        /// </summary>
        A,

        /// <summary>
        /// 句子的开头
        /// </summary>
        S,
    }
}
