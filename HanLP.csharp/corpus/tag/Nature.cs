/*
 * 名词 n, 时间词 t, 处所词 s, 方位词 f, 数词 m, 量词 q, 区别词 b， 代词 r， 动词 v， 形容词 a， 状态词 z， 副词 d，介词 p，
 * 连词 c，助词 u， 语气词 y， 叹词 e， 拟声词 o， 成语 i， 习用语 l， 简称 j， 前接成分 h， 后接成分 k， 语素 g， 非语素字 x， 标点符号 w
 * （如与下面枚举冲突，以枚举意义为准）
 * 
 * */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using HanLP.csharp.collection;

namespace HanLP.csharp.corpus.tag
{
    /// <summary>
    /// 词性枚举
    /// </summary>
    public enum Nature
    {
        /// <summary>
        /// 区别语素
        /// </summary>
        bg,

        /// <summary>
        /// 数语素
        /// </summary>
        mg,

        /// <summary>
        /// 名词性惯用语
        /// </summary>
        nl,

        /// <summary>
        /// 字母专名
        /// </summary>
        nx,

        /// <summary>
        /// 量词语素
        /// </summary>
        qg,

        /// <summary>
        /// 助词
        /// </summary>
        ud,

        /// <summary>
        /// 助词
        /// </summary>
        uj,

        /// <summary>
        /// 着
        /// </summary>
        uz,

        /// <summary>
        /// 过
        /// </summary>
        ug,

        /// <summary>
        /// 连词
        /// </summary>
        ul,

        /// <summary>
        /// 连词
        /// </summary>
        uv,

        /// <summary>
        /// 语气语素
        /// </summary>
        yg,

        /// <summary>
        /// 状态词
        /// </summary>
        zg,

        // 以上标签来自ICT，以下标签来自北大

        /// <summary>
        /// 名词
        /// </summary>
        n,

        /// <summary>
        /// 人名
        /// </summary>
        nr,

        /// <summary>
        /// 日语人名
        /// </summary>
        nrj,

        /// <summary>
        /// 音译人名
        /// </summary>
        nrf,

        /// <summary>
        /// 复姓
        /// </summary>
        nr1,

        /// <summary>
        /// 蒙古姓名
        /// </summary>
        nr2,

        /// <summary>
        /// 地名
        /// </summary>
        ns,

        /// <summary>
        /// 音译地名
        /// </summary>
        nsf,

        /// <summary>
        /// 机构团体名
        /// </summary>
        nt,

        /// <summary>
        /// 公司名
        /// </summary>
        ntc,

        /// <summary>
        /// 工厂
        /// </summary>
        ntcf,

        /// <summary>
        /// 银行
        /// </summary>
        ntcb,

        /// <summary>
        /// 酒店宾馆
        /// </summary>
        ntch,

        /// <summary>
        /// 政府机构
        /// </summary>
        nto,

        /// <summary>
        /// 大学
        /// </summary>
        ntu,

        /// <summary>
        /// 中小学
        /// </summary>
        nts,

        /// <summary>
        /// 医院
        /// </summary>
        nth,

        /// <summary>
        /// 医药疾病等健康相关名词
        /// </summary>
        nh,

        /// <summary>
        /// 药品
        /// </summary>
        nhm,

        /// <summary>
        /// 疾病
        /// </summary>
        nhd,

        /// <summary>
        /// 工作相关名词
        /// </summary>
        nn,

        /// <summary>
        /// 职务职称
        /// </summary>
        nnt,

        /// <summary>
        /// 职业
        /// </summary>
        nnd,

        /// <summary>
        /// 名词性语素
        /// </summary>
        ng,

        /// <summary>
        /// 食品，比如“薯片”
        /// </summary>
        nf,

        /// <summary>
        /// 机构相关（不是独立机构名）
        /// </summary>
        ni,

        /// <summary>
        /// 教育相关机构
        /// </summary>
        nit,

        /// <summary>
        /// 下属机构
        /// </summary>
        nic,

        /// <summary>
        /// 机构后缀
        /// </summary>
        nis,

        /// <summary>
        /// 物品名
        /// </summary>
        nm,

        /// <summary>
        /// 化学品名
        /// </summary>
        nmc,

        /// <summary>
        /// 生物名
        /// </summary>
        nb,

        /// <summary>
        /// 动物名
        /// </summary>
        nba,

        /// <summary>
        /// 动物纲目
        /// </summary>
        nbc,

        /// <summary>
        /// 植物名
        /// </summary>
        nbp,

        /// <summary>
        /// 其他专名
        /// </summary>
        nz,

        /// <summary>
        /// 学术词汇
        /// </summary>
        g,

        /// <summary>
        /// 数学相关词汇
        /// </summary>
        gm,

        /// <summary>
        /// 物理相关词汇
        /// </summary>
        gp,

        /// <summary>
        /// 化学相关词汇
        /// </summary>
        gc,

        /// <summary>
        /// 生物相关词汇
        /// </summary>
        gb,

        /// <summary>
        /// 生物类别
        /// </summary>
        gbc,

        /// <summary>
        /// 地理地质相关词汇
        /// </summary>
        gg,

        /// <summary>
        /// 计算机相关词汇
        /// </summary>
        gi,

        /// <summary>
        /// 简称略语
        /// </summary>
        j,

        /// <summary>
        /// 成语
        /// </summary>
        i,

        /// <summary>
        /// 习用语
        /// </summary>
        l,

        /// <summary>
        /// 时间词
        /// </summary>
        t,

        /// <summary>
        /// 时间词性语素
        /// </summary>
        tg,

        /// <summary>
        /// 处所词
        /// </summary>
        s,

        /// <summary>
        /// 方位词
        /// </summary>
        f,

        /// <summary>
        /// 动词
        /// </summary>
        v,

        /// <summary>
        /// 副动词
        /// </summary>
        vd,

        /// <summary>
        /// 名动词
        /// </summary>
        vn,

        /// <summary>
        /// 动词“是”
        /// </summary>
        vshi,

        /// <summary>
        /// 动词“有”
        /// </summary>
        vyou,

        /// <summary>
        /// 趋向动词
        /// </summary>
        vf,

        /// <summary>
        /// 形式动词
        /// </summary>
        vx,

        /// <summary>
        /// 不及物动词（内动词）
        /// </summary>
        vi,

        /// <summary>
        /// 动词性惯用语
        /// </summary>
        vl,

        /// <summary>
        /// 动词性语素
        /// </summary>
        vg,

        /// <summary>
        /// 形容词
        /// </summary>
        a,

        /// <summary>
        /// 副形词
        /// </summary>
        ad,

        /// <summary>
        /// 名形词
        /// </summary>
        an,

        /// <summary>
        /// 形容词性语素
        /// </summary>
        ag,

        /// <summary>
        /// 形容词性惯用语
        /// </summary>
        al,

        /// <summary>
        /// 区别词
        /// </summary>
        b,

        /// <summary>
        /// 区别词性惯用语
        /// </summary>
        bl,

        /// <summary>
        /// 状态词
        /// </summary>
        z,

        /// <summary>
        /// 代词
        /// </summary>
        r,

        /// <summary>
        /// 人称代词
        /// </summary>
        rr,

        /// <summary>
        /// 指示代词
        /// </summary>
        rz,

        /// <summary>
        /// 时间指示代词
        /// </summary>
        rzt,

        /// <summary>
        /// 处所指示代词
        /// </summary>
        rzs,

        /// <summary>
        /// 谓词性指示代词
        /// </summary>
        rzv,

        /// <summary>
        /// 疑问代词
        /// </summary>
        ry,

        /// <summary>
        /// 时间疑问代词
        /// </summary>
        ryt,

        /// <summary>
        /// 处所疑问代词
        /// </summary>
        rys,

        /// <summary>
        /// 谓词性疑问代词
        /// </summary>
        ryv,

        /// <summary>
        /// 代词性语素
        /// </summary>
        rg,

        /// <summary>
        /// 古汉语代词性语素
        /// </summary>
        Rg,

        /// <summary>
        /// 数词
        /// </summary>
        m,

        /// <summary>
        /// 数量词
        /// </summary>
        mq,

        /// <summary>
        /// 甲乙丙丁之类的数词
        /// </summary>
        Mg,

        /// <summary>
        /// 量词
        /// </summary>
        q,

        /// <summary>
        /// 动量词
        /// </summary>
        qv,

        /// <summary>
        /// 时量词
        /// </summary>
        qt,

        /// <summary>
        /// 副词
        /// </summary>
        d,

        /// <summary>
        /// 辄,俱,复之类的副词
        /// </summary>
        dg,

        /// <summary>
        /// 连语
        /// </summary>
        dl,

        /// <summary>
        /// 介词
        /// </summary>
        p,

        /// <summary>
        /// 介词“把”
        /// </summary>
        pba,

        /// <summary>
        /// 介词“被”
        /// </summary>
        pbei,

        /// <summary>
        /// 连词
        /// </summary>
        c,

        /// <summary>
        /// 并列连词
        /// </summary>
        cc,

        /// <summary>
        /// 助词
        /// </summary>
        u,

        /// <summary>
        /// 着
        /// </summary>
        uzhe,

        /// <summary>
        /// 了 喽
        /// </summary>
        ule,

        /// <summary>
        /// 过
        /// </summary>
        uguo,

        /// <summary>
        /// 的 底
        /// </summary>
        ude1,

        /// <summary>
        /// 地
        /// </summary>
        ude2,

        /// <summary>
        /// 得
        /// </summary>
        ude3,

        /// <summary>
        /// 所
        /// </summary>
        usuo,

        /// <summary>
        /// 等 等等 云云
        /// </summary>
        udeng,

        /// <summary>
        /// 一样 一般 似的 般
        /// </summary>
        uyy,

        /// <summary>
        /// 的话
        /// </summary>
        udh,

        /// <summary>
        /// 来讲 来说 而言 说来
        /// </summary>
        uls,

        /// <summary>
        /// 之
        /// </summary>
        uzhi,

        /// <summary>
        /// 连 （“连小学生都会”）
        /// </summary>
        ulian,

        /// <summary>
        /// 叹词
        /// </summary>
        e,

        /// <summary>
        /// 语气词(delete yg)
        /// </summary>
        y,

        /// <summary>
        /// 拟声词
        /// </summary>
        o,

        /// <summary>
        /// 前缀
        /// </summary>
        h,

        /// <summary>
        /// 后缀
        /// </summary>
        k,

        /// <summary>
        /// 字符串
        /// </summary>
        x,

        /// <summary>
        /// 非语素字
        /// </summary>
        xx,

        /// <summary>
        /// 网址URL
        /// </summary>
        xu,

        /// <summary>
        /// 标点符号
        /// </summary>
        w,

        /// <summary>
        /// 左括号，全角：（ 〔  ［  ｛  《 【  〖 〈   半角：( [ { <
        /// </summary>
        wkz,

        /// <summary>
        /// 右括号，全角：） 〕  ］ ｝ 》  】 〗 〉 半角： ) ] { >
        /// </summary>
        wky,

        /// <summary>
        /// 左引号，全角：“ ‘ 『
        /// </summary>
        wyz,

        /// <summary>
        /// 右引号，全角：” ’ 』
        /// </summary>
        wyy,

        /// <summary>
        /// 句号，全角：。
        /// </summary>
        wj,

        /// <summary>
        /// 问号，全角：？ 半角：?
        /// </summary>
        ww,

        /// <summary>
        /// 叹号，全角：！ 半角：!
        /// </summary>
        wt,

        /// <summary>
        /// 逗号，全角：， 半角：,
        /// </summary>
        wd,

        /// <summary>
        /// 分号，全角：； 半角： ;
        /// </summary>
        wf,

        /// <summary>
        /// 顿号，全角：、
        /// </summary>
        wn,

        /// <summary>
        /// 冒号，全角：： 半角： :
        /// </summary>
        wm,

        /// <summary>
        /// 省略号，全角：……  …
        /// </summary>
        ws,

        /// <summary>
        /// 破折号，全角：——   －－   ——－   半角：---  ----
        /// </summary>
        wp,

        /// <summary>
        /// 百分号千分号，全角：％ ‰   半角：%
        /// </summary>
        wb,

        /// <summary>
        /// 单位符号，全角：￥ ＄ ￡  °  ℃  半角：$
        /// </summary>
        wh,

        /// <summary>
        /// 仅用于终##终，不会出现在分词结果中
        /// </summary>
        end,

        /// <summary>
        /// 仅用于始##始，不会出现在分词结果中
        /// </summary>
        begin,

        none,
    }

    public class NatureHelper
    {
        /// <summary>
        /// 词性是否以该前缀开头<br>
        ///    词性根据开头的几个字母可以判断大的类别
        /// @param prefix 前缀
        /// @return 是否以该前缀开头
        /// <summary>
        public static bool StartsWith(Nature nat, string prefix) => nat.ToString().StartsWith(prefix);

        /// <summary>
        /// 词性是否以该前缀开头<br>
        ///     词性根据开头的几个字母可以判断大的类别
        /// @param prefix 前缀
        /// @return 是否以该前缀开头
        /// <summary>
        public static bool StartsWith(Nature nat, char initial) => nat.ToString()[0] == initial;

        /// <summary>
        /// 词性的首字母<br>
        ///     词性根据开头的几个字母可以判断大的类别
        /// @return
        /// <summary>
        public static char FirstChar(Nature nat) => nat.ToString()[0];

        /// <summary>
        /// 安全地将字符串类型的词性转为Enum类型，如果未定义该词性，则返回null
        /// @param name 字符串词性
        /// @return Enum词性
        /// <summary>
        public static Nature FromString(string name)
        {
            if (Enum.TryParse<Nature>(name, out var nat))
                return nat;

            return Nature.none;
        }

        /// <summary>
        /// 创建自定义词性,如果已有该对应词性,则直接返回已有的词性
        /// @param name 字符串词性
        /// @return Enum词性
        /// <summary>
        public static Nature GetOrCreate(string name)
        {
            if (Enum.TryParse<Nature>(name, out var nat))
                return nat;

            //todo: dynamic create enum     C# EnumBuilder
            int val;
            if(_customNatures.TryGetValue(name, out val))
            {
                return (Nature)val;
            }
            else
            {
                var v = 500 + _customNatures.Count;
                _customNatures.Add(name, v);
                return (Nature)v;
            }
        }

        private static SortedDictionary<string, int> _customNatures = new SortedDictionary<string, int>(StrComparer.Default);
        public static SortedDictionary<string, int> CustomNatures => _customNatures;
    }

}
