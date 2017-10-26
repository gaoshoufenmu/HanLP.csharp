using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using HanLP.csharp;
using HanLP.csharp.utility;

namespace HanLP.Test.csharp
{
    [TestFixture]
    public class HanLP_Test
    {
        #region simplified<->traditional
        [Test]
        public void Traditional2Simplified()
        {
            var origin = "以後等妳當上皇后，就能買士多啤梨慶祝了";
            var expect = "以后等你当上皇后，就能买草莓庆祝了";
            Assert.AreEqual(expect, HANLP.Traditional2Simplified(origin));
        }

        [Test]
        public void Simplified2Traditional()
        {
            var origin = "以后等你当上皇后，就能买草莓庆祝了";
            var expect = "以後等你當上皇后，就能買草莓慶祝了";   // 简转繁词典中没有“你”，“草莓”词条
            Assert.AreEqual(expect, HANLP.Simplified2Traditional(origin));
        }
        #endregion

        #region pinyin
        [Test]
        public void GetPinyin()
        {
            var origin = "重载不是重任！说客拗口但却执拗地说到";
            var expect1 = "chong/zai/bu/shi/zhong/ren/！/shui/ke/ao/kou/dan/que/zhi/niu/di/shuo/dao";
            Assert.AreEqual(expect1, HANLP.GetPinyin(origin));

            var expect2 = "chong/zai/bu/shi/zhong/ren/none/shui/ke/ao/kou/dan/que/zhi/niu/di/shuo/dao";
            Assert.AreEqual(expect2, HANLP.GetPinyin(origin, remainNone: true));
        }

        [Test]
        public void GetPinyins()
        {
            var origin = "重载不是重任！";
            var list = HANLP.GetPinyins(origin);

            var pinyinWithToneNum = "chong2,zai3,bu2,shi4,zhong4,ren4";       // 数字音调
            Assert.AreEqual(pinyinWithToneNum, TextUtil.Join2Str(",", list.Select(l => l.ToString())));

            var pinyinWithToneMark = "chóng,zǎi,bú,shì,zhòng,rèn";              // 符号音调
            Assert.AreEqual(pinyinWithToneMark, TextUtil.Join2Str(",", list.Select(l => l.PinyinWithTone)));
        }

        [Test]
        public void GetInitialChars()
        {
            var origin = "拗口但又执拗地来自西藏的人物传记中有阿胶~";
            var expect = "a'k'd'y'z'n'd'l'z'x'z'd'r'w'z'j'z'y'e'j";

            Assert.AreEqual(expect, HANLP.GetInitialChars(origin));
        }
        #endregion

        #region NER
        [Test]
        public void PeopleNameRecognition()
        {
            var testCases = new[]
            {
                "签约仪式前，秦光荣、李纪恒、仇和等一同会见了参加签约的企业家。",
                "区长庄木弟新年致辞",
                "朱立伦：两岸都希望共创双赢 习朱历史会晤在即",
                "陕西首富吴一坚被带走 与令计划妻子有交集",
                "据美国之音电台网站4月28日报道，8岁的凯瑟琳·克罗尔（凤甫娟）和很多华裔美国小朋友一样，小小年纪就开始学小提琴了。她的妈妈是位虎妈么？",
                "凯瑟琳和露西（庐瑞媛），跟她们的哥哥们有一些不同。",
                "王国强、高峰、汪洋、张朝阳光着头、韩寒、小四",
                "张浩和胡健康复员回家了",
                "王总和小丽结婚了",
                "编剧邵钧林和稽道青说",
                "这里有关天培的有关事迹",
                "龚学平等领导说,邓颖超生前杜绝超生",
                "甄子丹和刘德华为这部电影付出了非常多的努力"
            };

            var segTerms = new[]
            {
                "签约/vi, 仪式/n, 前/f, ，/w, 秦光荣/nr, 、/w, 李纪恒/nr, 、/w, 仇和/nr, 等/udeng, 一同/d, 会见/v, 了/ule, 参加/v, 签约/vi, 的/ude1, 企业家/nnt, 。/w, ",
                "区长/nnt, 庄木弟/nr, 新年/t, 致辞/vi, ",
                "朱立伦/nr, ：/w, 两岸/n, 都/d, 希望/v, 共创/v, 双赢/n,  /n, 习/v, 朱/ag, 历史/n, 会晤/vn, 在即/vi, ",
                "陕西/ns, 首富/n, 吴一坚/nr, 被/pbei, 带走/v,  /n, 与/cc, 令计划/nr, 妻子/n, 有/vyou, 交集/v, ",
                "据/p, 美国之音/n, 电台/nis, 网站/n, 4/n, 月/n, 28/n, 日/b, 报道/v, ，/w, 8/n, 岁/qt, 的/ude1, 凯瑟琳·克罗尔/nrf, （/w, 凤甫娟/nr, ）/w, 和/cc, 很多/m, 华裔/n, 美国/nsf, 小朋友/n, 一样/uyy, "
                    + "，/w, 小小/z, 年纪/n, 就/d, 开始/v, 学/v, 小提琴/n, 了/ule, 。/w, 她/rr, 的/ude1, 妈妈/n, 是/vshi, 位/q, 虎妈/nz, 么/y, ？/w, ",
                "凯瑟琳/nrf, 和/cc, 露西/nrf, （/w, 庐瑞媛/nr, ）/w, ，/w, 跟/p, 她们/rr, 的/ude1, 哥哥/n, 们/k, 有/vyou, 一些/m, 不同/a, 。/w, ",
                "王国强/nr, 、/w, 高峰/n, 、/w, 汪洋/n, 、/w, 张朝阳/nr, 光/n, 着/uzhe, 头/n, 、/w, 韩寒/nr, 、/w, 小四/nr, ",
                "张浩/nr, 和/cc, 胡健康/nr, 复员/v, 回家/vi, 了/ule, ",
                "王总/nr, 和/cc, 小丽/nr, 结婚/vi, 了/ule, ",
                "编剧/nnt, 邵钧林/nr, 和/cc, 稽道青/nr, 说/v, ",
                "这里/rzs, 有/vyou, 关天培/nr, 的/ude1, 有关/vn, 事迹/n, ",
                "龚学平/nr, 等/udeng, 领导/n, 说/v, ,/n, 邓颖超/nr, 生前/t, 杜绝/v, 超生/vi, ",
                "甄子丹/nr, 和/cc, 刘德华/nr, 为/p, 这部/r, 电影/n, 付出/v, 了/ule, 非常/d, 多/a, 的/ude1, 努力/ad, "
            };
            for(int i = 0; i < testCases.Length; i++)
            {
                var s = testCases[i];
                var terms = HANLP.Segment(s);
                var sb = new StringBuilder(s.Length * 4);
                foreach(var t in terms)
                {
                    sb.Append($"{t}, ");
                }
                Assert.AreEqual(segTerms[i], sb.ToString());
            }
        }

        [Test]
        public void OrgRecognition()
        {
            var testcases = new[]
            {
                "南昌市公安局西湖分局筷子巷派出所",
                "济南杨铭宇餐饮管理有限公司是由杨先生创办的餐饮企业",
                "我在上海林原科技有限公司兼职工作，",
                "我经常在台川喜宴餐厅吃饭，",
                "偶尔去开元地中海影城看电影。"
            };
            var expects = new[]
            {
                "南昌市公安局/nt, 西湖分局筷子巷派出所/nt, ",
                "济南杨铭宇餐饮管理有限公司/nt, 是/vshi, 由/p, 杨先生/nr, 创办/v, 的/ude1, 餐饮企业/nt, ",
                "我/rr, 在/p, 上海/ns, 林原科技有限公司/nt, 兼职/vn, 工作/vn, ，/w, ",
                "我/rr, 经常/d, 在/p, 台川喜宴餐厅/nt, 吃饭/vi, ，/w, ",
                "偶尔/d, 去/vf, 开元地中海影城/nt, 看/v, 电影/n, 。/w, "
            };
            for (int i = 0; i < testcases.Length; i++)
            {
                var s = testcases[i];
                var terms = HANLP.Segment(s);
                var sb = new StringBuilder(s.Length * 4);
                foreach (var t in terms)
                {
                    sb.Append($"{t}, ");
                }
                Assert.AreEqual(expects[i], sb.ToString());
            }
        }
        #endregion
    }
}
