﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using HanLP.csharp.seg.HMM;
using HanLP.csharp;
using HanLP.csharp.seg.CRF;

namespace HanLP.Test.csharp
{
    [TestFixture]
    public class Seg_Test
    {
        [Test]
        public void HMM_Segment()
        {
            var testCases = new[]
            {
                "HanLP是由一系列模型与算法组成的Java工具包，目标是普及自然语言处理在生产环境中的应用。",
                "高锰酸钾，强氧化剂，紫红色晶体，可溶于水，遇乙醇即被还原。常用作消毒剂、水净化剂、氧化剂、漂白剂、毒气吸收剂、二氧化碳精制剂等。", // 专业名词有一定辨识能力
                "《夜晚的骰子》通过描述浅草的舞女在暗夜中扔骰子的情景,寄托了作者对庶民生活区的情感",    // 非新闻语料
                "这个像是真的[委屈]前面那个打扮太江户了，一点不上品...@hankcs",                       // 微博
                "鼎泰丰的小笼一点味道也没有...每样都淡淡的...淡淡的，哪有食堂2A的好次",
                "克里斯蒂娜·克罗尔说：不，我不是虎妈。我全家都热爱音乐，我也鼓励他们这么做。",
                "今日APPS：Sago Mini Toolbox培养孩子动手能力",
                "财政部副部长王保安调任国家统计局党组书记",
                "2.34米男子娶1.53米女粉丝 称夫妻生活没问题",
                "你看过穆赫兰道吗",
                "乐视超级手机能否承载贾布斯的生态梦"
            };
            var expects = new[]
            {
                "HanLP是由一系列/ 模型/ 与/ 算法/ 组成/ 的/ Java工具/ 包/ ，/ 目标/ 是/ 普及/ 自然/ 语言/ 处理/ 在/ 生产/ 环境/ 中/ 的/ 应用/ 。",
                "高锰酸钾/ ，/ 强氧化剂/ ，/ 紫红色/ 晶体/ ，/ 可/ 溶于/ 水/ ，/ 遇乙醇/ 即/ 被/ 还原/ 。/ 常/ 用作/ 消毒/ 剂/ 、/ 水/ 净化剂/ 、/ 氧化剂/ 、/ 漂白剂/ 、/ 毒气/ 吸收/ 剂/ 、/ 二氧化碳/ 精制剂/ 等/ 。",
                "《/ 夜晚/ 的/ 骰子/ 》/ 通过/ 描述/ 浅/ 草/ 的/ 舞女/ 在/ 暗夜/ 中/ 扔骰子/ 的/ 情景/ ,/ 寄托/ 了/ 作者/ 对庶民/ 生活区/ 的/ 情感",
                "这个/ 像/ 是/ 真的/ [/ 委屈/ ]/ 前面/ 那个/ 打扮/ 太/ 江/ 户/ 了/ ，/ 一点/ 不/ 上品/ ./ ./ ./ @/ hankcs",
                "鼎泰丰/ 的/ 小笼/ 一点/ 味道/ 也/ 没有/ ./ ./ ./ 每样/ 都/ 淡淡/ 的/ ./ ./ ./ 淡淡的/ ，/ 哪/ 有/ 食堂/ 2A/ 的/ 好次",
                "克里斯蒂娜·克罗尔/ 说/ ：/ 不/ ，/ 我/ 不是/ 虎妈/ 。/ 我/ 全家/ 都/ 热爱/ 音乐/ ，/ 我/ 也/ 鼓励/ 他们/ 这么/ 做/ 。",
                "今日/ APPS/ ：/ Sago Mini Toolbox培养/ 孩子/ 动手/ 能力",
                "财政部/ 副部长/ 王保安/ 调任/ 国家/ 统计局/ 党组/ 书记",
                "2/ ./ 34/ 米/ 男子/ 娶/ 1/ ./ 53/ 米/ 女/ 粉丝/  称夫妻/ 生活/ 没问题",
                "你/ 看过/ 穆赫/ 兰/ 道/ 吗",
                "乐/ 视/ 超级/ 手机/ 能否/ 承载/ 贾布斯/ 的/ 生态/ 梦"
            };
            var segment = new HMMSegment();

            for(int j = 0; j < testCases.Length; j++)
            {
                var terms = segment.Seg(testCases[j]);

                var sb = new StringBuilder();
                for(int i = 0; i < terms.Count; i++)
                {
                    if (i != 0)
                        sb.Append("/ ");
                    sb.Append(terms[i].word);
                }
                Assert.AreEqual(expects[j], sb.ToString());
            }
        }

        [Test]
        public void CRF_Segment()
        {
            Config.ShowTermNature = false;      // 关闭词性显示
            var segment = new CRFSegment().SetCustomDictionary(false);      // 不使用自定义词典
            var testCases = new[]
            {
                "HanLP是由一系列模型与算法组成的Java工具包，目标是普及自然语言处理在生产环境中的应用。",
                "鐵桿部隊憤怒情緒集結 馬英九腹背受敵",           // 繁体无压力
                "馬英九回應連勝文“丐幫說”：稱黨內同志談話應謹慎",
                "高锰酸钾，强氧化剂，紫红色晶体，可溶于水，遇乙醇即被还原。常用作消毒剂、水净化剂、氧化剂、漂白剂、毒气吸收剂、二氧化碳精制剂等。", // 专业名词有一定辨识能力
                "《夜晚的骰子》通过描述浅草的舞女在暗夜中扔骰子的情景,寄托了作者对庶民生活区的情感",    // 非新闻语料
                "这个像是真的[委屈]前面那个打扮太江户了，一点不上品...@hankcs",                       // 微博
                "鼎泰丰的小笼一点味道也没有...每样都淡淡的...淡淡的，哪有食堂2A的好次",
                "克里斯蒂娜·克罗尔说：不，我不是虎妈。我全家都热爱音乐，我也鼓励他们这么做。",
                "今日APPS：Sago Mini Toolbox培养孩子动手能力",
                "财政部副部长王保安调任国家统计局党组书记",
                "2.34米男子娶1.53米女粉丝 称夫妻生活没问题",
                "你看过穆赫兰道吗",
                "国办发布网络提速降费十四条指导意见 鼓励流量不清零",
                "乐视超级手机能否承载贾布斯的生态梦"
            };

            var expects = new[]
            {
                "HanLP/ 是/ 由/ 一系列/ 模型/ 与/ 算法/ 组成/ 的/ Java/ 工具包/ ，/ 目标/ 是/ 普及/ 自然/ 语言/ 处理/ 在/ 生产/ 环境/ 中/ 的/ 应用/ 。",
                "鐵桿/ 部隊/ 憤怒/ 情緒/ 集結/  / 馬英九/ 腹背受敵",
                "馬英九/ 回應/ 連勝文/ “/ 丐幫/ 說”/ ：/ 稱/ 黨內/ 同志/ 談話/ 應/ 謹慎",
                "高锰酸钾/ ，/ 强/ 氧化剂/ ，/ 紫红色/ 晶体/ ，/ 可/ 溶于/ 水/ ，/ 遇/ 乙醇/ 即/ 被/ 还原/ 。/ 常用/ 作/ 消毒剂/ 、/ 水/ 净化剂/ 、/ 氧化剂/ 、/ 漂白剂/ 、/ 毒气/ 吸收剂/ 、/ 二氧化碳/ 精制剂/ 等/ 。",
                "《/ 夜晚/ 的/ 骰子/ 》/ 通过/ 描述/ 浅草/ 的/ 舞女/ 在/ 暗夜/ 中/ 扔/ 骰子/ 的/ 情景/ ,/ 寄托/ 了/ 作者/ 对/ 庶民/ 生活区/ 的/ 情感",
                "这个/ 像/ 是/ 真的/ [/ 委屈/ ]/ 前面/ 那个/ 打扮/ 太江/ 户/ 了/ ，/ 一点/ 不/ 上品/ ./ ./ ./ @/ hankcs",
                "鼎泰丰/ 的/ 小笼/ 一点/ 味道/ 也/ 没有/ ./ ./ ./ 每样/ 都/ 淡淡的/ ./ ./ ./ 淡淡的/ ，/ 哪/ 有/ 食堂/ 2/ A/ 的/ 好次",
                "克里斯蒂娜·克罗尔/ 说/ ：/ 不/ ，/ 我/ 不是/ 虎妈/ 。/ 我/ 全家/ 都/ 热爱/ 音乐/ ，/ 我/ 也/ 鼓励/ 他们/ 这么/ 做/ 。",
                "今日/ APPS/ ：/ Sago Mini Toolbox/ 培养/ 孩子/ 动手/ 能力",
                "财政部/ 副部长/ 王保安/ 调任/ 国家/ 统计局/ 党组/ 书记",
                "2.34/ 米/ 男子/ 娶/ 1.53/ 米/ 女/ 粉丝/  / 称/ 夫妻/ 生活/ 没问题",
                "你/ 看过/ 穆赫兰道/ 吗",
                "国办/ 发布/ 网络/ 提速/ 降费/ 十四条/ 指导/ 意见/  / 鼓励/ 流量/ 不/ 清零",
                "乐/ 视/ 超级/ 手机/ 能否/ 承载/ 贾布斯/ 的/ 生态/ 梦"
            };
            for(int j = 0; j < testCases.Length; j++)
            {
                var terms = segment.Seg(testCases[j]);
                var sb = new StringBuilder();
                for (int i = 0; i < terms.Count; i++)
                {
                    if (i != 0)
                        sb.Append("/ ");
                    sb.Append(terms[i].word);
                }
                Assert.AreEqual(expects[j], sb.ToString());
            }
        }

        [Test]
        public void CRF_Segment_Com()
        {
            //var segment = new CRFSegment().SetCustomDictionary(false);      // 不使用自定义词典
            var testCases = new[]
            {
                "山西织袜厂",
                "富拉尔基纺织印染厂进出口公司",
"太原市绿园绿化工程队",
"太原市晋腾达物资贸易经销部",
"太原南城缘园春饼屋",
"黑龙江省农垦总局粮油食品进出口公司",
"太原南城翔龙大酒店",
"黑龙江龙涤集团有限公司",
"太原电力电缆厂经销处",
"黑龙江省漠河县对外经济贸易公司",
"潮阳市达成实业有限公司太原经销部",
"太原市南城区银盘歌苑",
"黑龙江省同江市对外贸易公司",
"太原钟德诚商贸有限责任公司",
"太原市五一广场园林综合服务部",
"太原市南城区银剑招待所",
"黑龙江省嘉荫县对外贸易公司",
"中国外运黑龙江物流公司",
"太原市南城亿亨保健品销售中心",
"山西大华通煤矿机电设备厂",
"东宁国际经济技术合作公司",
"太原南城钟百服饰中心",
"太原市南城区天河桑娜中心",
"逊克县对外贸易公司",
"太原迎泽天虹花业服务中心",
"太原地毯厂销售部",
"黑龙江省萝北县对外经济贸易公司",
"太原东方雕塑艺术发展中心",
"黑龙江省密山市对外贸易公司",
"饶河县对外贸易公司",
"太原市琛茂机电物资供应站",
"大庆市华盛进出口公司",
"太原市鼎鑫进口汽车修配厂",
"太原嘉宝实业公司",
"黑龙江省外国投资咨询公司",
"太原市南城区先河娱乐城",
"太原利普包装制品厂",
"山西省邮电工业公司公话设备经销部",
"山西太原海源邮电物资经销处",
"黑龙江省虎林县对外贸易公司",
"内蒙古第二毛纺织总厂太原销售处",
"太原市南城大福油店",
"武汉阀门厂太原销售处",
"太原市南城圣大保罗专卖店",
"中国黑龙江省呼玛县边境贸易公司",
"中国重型汽车销售公司太原汽运特约服务站",
"太原市国际旅贸公司",
"太原市新雅潮内衣总汇",
"黑龙江省宁安市对外经济贸易公司",
"黑龙江金巢典当有限责任公司",
"太原鸿信路路通汽车配件维修部",
"太原市亨久源建材销售中心",
"黑龙江省东亚储运销售公司",
"上海永生联合销售公司太原经营部",
"太原南城虹发电器商行",
"黑龙江省机械进出口公司",
"长沙锅炉厂太原经营部",
"太原南城晋阳电器开发研究所",
"太原市迎泽废旧物资回收公司",
"太原市源盛泵业销售维修中心",
"太原南城顺天翔贸易中心",
"黑龙江进出口商品检验公司",
"太原北华自动化设备厂",
"太原市迎泽菜园土产日杂批零商店",
"齐齐哈尔纺织厂进出口公司",
"太原市汾水制镜厂特种玻璃经销部",
"黑龙江省大兴安岭对外经济贸易公司",
"黑龙江通达国际贸易有限公司",
"太原南城王牌农用车经营部",
"太原南城清尘日用化工厂",
"太原如磐工程机械加工厂",
"中国外运黑龙江齐齐哈尔公司",
"山西省物资储运经销公司劳动服务公司",
"安阳市大桥化工厂驻太原经营处",
"黑龙江省轻工业品进出口分公司松花江地区支公司",
"太原市南城金城五金机电批发部",
"北钢集团进出口公司",
"《神州印象》杂志社",
"中国鹤岗对外贸易运输公司",
"太原物华宝工业品轴承贸易部",
"太原市南城区鹏程汽车修理厂",
"太原南城燕云楼酒店",
"无锡市远东电缆厂太原经营部",
"太原南城哈尔滨新世界大酒店",
"太原市对外经济贸易委员会劳服公司四方家电经营中心",
"太原市森隆工矿物资供应站",
"江苏湖风集团公司太原经销处",
"新疆石河子八一毛纺织厂太原经营部",
"山西省水工机械厂销售中心",
"山西针纺配件厂",
"山西前进机器厂职工商店",
"山西奥达纬编厂",
"沈阳松辽工程机械厂太原销售处",
"常州煤矿机械厂太原服务部",
"常州煤矿电器厂太原服务部",
"太原市雄洲物资贸易中心",
"太原市凯园物资经销中心",
"太原供电局宾馆",
"太原南城区金昌盛影视厅",
"太原南城宏达家俱部",
"汉江油泵油咀厂太原销售服务中心",
"太原南城开源酒楼",
"太原雅慧商贸有限公司",
"山西省金大陆贸易发展中心",
"太原市南城宣工工程机械销售中心",
"太原市南城川都名食店",
"太原市红旗橡胶制品厂供应站",
"太原市迎泽区梦迪莎婚纱写真店",
"太原华生兴电子发展中心",
"太原市迎泽区物资总公司物资再生利用开发经营部",
"太原昌景物资贸易中心",
"太原南城美瑛贸易中心",
"东方集团股份有限公司",
"山西富林房地产开发有限公司",
"太原南城西诺刹车蹄片专卖中心",
"太原市南城区广告人书屋",
"哈尔滨刃具厂太原销售处",
"太原晋辉达建材石膏站",
"宝鸡叉车厂太原销售部",
"太原超力钢电杆制造厂",
"太原工业锅炉厂",
"太原市南城区好邻居商店",
"太原市伤残人康复器材供应站",
"太原南城金轮制鞋厂",
"太原市三新环保技术咨询部",
"山西汇瑞祥商贸中心",
"西北煤矿机械二厂太原经销部",
"太原市迎泽区晶星日用化学品厂",
"太原化学工业集团公司有机化工厂物资经营部",
"太原南城红火客栈",
"山西省人民政府第一招待所多种经营部",
"太原南城锦江大酒店",
"太原南城大富豪酒店",
"山西财经大学劳动服务公司",
"太原江都贸易商行",
"太原金纬复合材料厂",
"太原南城盛合酒店用品总汇",
"山西省陶行知研究会生活教育书社",
"太原市海迅电话交换机维护中心",
"大同中药厂驻太原经销部",
"太原市南城清诚洗衣店",
"太原茂深达物业公司",
"太原山威贸易公司",
"太原面粉三厂天粟粮油经销四部",
"山西省糖酒副食公司三江商行",
"山西电机厂太原销售处",
"太原市南城区众心贸易中心",
"浙江帅康集团有限公司太原销售中心",
"太原易日综合贸易部",
"鞍山电磁阀有限责任公司太原销售处",
"太原尤合力礼仪服务培训中心",
"太原南城健民综合商店",
"太原市南城花原美容院",
"山西清徐来福老陈醋有限公司太原专卖店",
"天水长城控制电器厂太原销售服务部",
"自贡高压阀门总厂太原销售处",
"太原南城明声机电供应站",
"太原市南城区新兴公用五金电器厂",
"太原市迎泽区永达体育用品供应站",
"太原市南城永华干洗店",
"山西省综合测绘院技术开发部",
"山西盛华贸易公司",
"广西化轻建材南宁公司太原销售中心",
"太原金座大酒店",
"太原隆信液压设备修造中心",
"太原市南城区重阳药店",
"太原沧新电子技术服务中心（普通合伙）",
"太原南城智达通讯经销中心",
"太原市小店区太行红宇建筑配件厂",
"浙江省东阳市二轻建筑工程公司太原工程处",
"太原三莺物资贸易中心",
"太原市南城乐新物业管理中心",
"太原汾源水利技术服务部",
"太原市同发重型机械备件中心",
"太原市振达物资机电供应站",
"太原市华贸物资机电供应站",
"太原市万发金工汽车修理厂",
"山西经济技术服务公司合作部",
"太原市康信纺织贸易中心",
"太原市杏花岭区祥岳化玻供应站",
"太原市山达物资中心",
"黑龙江省电子工贸总公司",
"黑龙江省轻工家用电器维修服务中心",
"哈尔滨卫检科技开发公司",
"黑龙江省新星生物制品经销站",
"黑龙江省保安服务总公司",
"黑龙江省粮油购储经营公司",
"黑龙江辰能投资集团有限责任公司",
"黑龙江省粮食工业公司",
"黑龙江省粮油食品经销公司",
"黑龙江省石油大厦",
"黑龙江省油脂公司",
"黑龙江省宏达高新技术开发公司",
"黑龙江雪龙饮品集团",
"五大连池中国国际旅行社",
"哈尔滨铁路局工程总承包公司",
"黑龙江省设备租赁公司",
"黑龙江斯博建筑工程有限责任公司",
"黑龙江省医药经贸公司",
"黑龙江省粮油物资公司大连开发区公司",
"黑龙江省茶叶公司",
"黑龙江省旅游贸易公司",
"黑龙江省中医药开发有限公司",
"黑龙江省龙金经济贸易公司",
"黑龙江省隆阜贸易总公司",
"黑龙江农垦贸易集团公司",
"黑龙江省供销合作企业总公司",
"黑龙江省农垦物资贸易总公司",
"黑龙江省公路勘察设计院",
"黑龙江省振龙经贸实业(集团)股份公司",
"黑龙江省昌华经济贸易公司",
"黑龙江省航运物资供应公司",
"黑龙江省北华贸易公司",
"黑龙江省天龙建筑装修工程公司",
"黑龙江省粮食经济贸易公司",
"黑龙江省中信石油化工贸易公司",
"黑龙江省轻工建设管理有限公司",
"黑龙江省制冷工程公司",
"黑龙江省龙丹乳品贸易公司",
"黑龙江省育兴经济贸易公司",
"黑龙江省科海贸易公司",
"黑龙江省商业对外供应公司",
"黑龙江省北方经济信息公司",
"黑龙江省机械工业贸易公司",
"黑龙江省卫达实业开发公司",
"黑龙江省实利贸易公司",
"黑龙江省工业贸易总公司",
"黑龙江煤矿机械有限公司",
"中航资本控股股份有限公司",
"黑龙江省农垦贸易公司",
"黑龙江省建设经济技术开发中心",
"黑龙江省制糖工业原料公司",
"黑龙江省制糖工业技术开发公司",
"黑龙江省龙华房地产开发公司龙华家俱城",
"黑龙江省地方铁路多种经营总公司",
"黑龙江省新颖装璜材料公司",
"黑龙江省隆福测绘技术开发总公司",
"黑龙江省昌隆贸易公司",
"黑龙江省龙轻贸易公司",
"国网黑龙江省电力有限公司",
"黑龙江省麒麟工贸公司",
"黑龙江省龙源电力燃料有限公司",
"东北金城哈尔滨经贸中心",
"黑龙江省物资经济协作开发总公司",
"黑龙江省专利技术开发公司",
"黑龙江省龙储物资经销公司",
"黑龙江省华夏经济技术开发公司",
"黑龙江省四方工贸有限公司",
"黑龙江省联开贸易公司",
"黑龙江中北房地产开发集团有限公司",
"黑龙江省甜菜糖业总公司",
"黑龙江省盛通贸易公司",
"黑龙江省北斗经济贸易公司",
"黑龙江海外开发总公司",
"黑龙江省畜牧兽医药械公司",
"黑龙江省东亚安装工程公司",
"黑龙江省华宇房地产开发公司",
"黑龙江农垦物资再生利用总公司",
"黑龙江省龙艺装饰工程公司",
"黑龙江省振安实业集团总公司",
"黑龙江省供销房地产开发有限责任公司",
"黑龙江省出国旅行咨询服务公司",
"黑龙江省四达资源开发公司",
"黑龙江金龙实业股份有限公司",
"黑龙江省松花江林业物资公司",
"黑龙江省糖业多种经营开发公司",
"黑龙江省食糖交易中心",
"黑龙江粮食经纪公司",
"黑龙江省天宇药品科技开发公司",
"黑龙江省麒麟山贸易公司",
"黑龙江省龙煤房地产开发公司",
"黑龙江省民建会计师事务所",
"黑龙江省饲料企业集团总公司",
"黑龙江省天星经济发展公司",
"黑龙江龙华石油化工(集团)股份有限公司",
"黑龙江省东环实业总公司",
"黑龙江省隆华经贸公司",
"黑龙江亚地旅行社",
"黑龙江省帼华经贸公司",
"哈尔滨铁路房建置业集团有限公司",
"黑龙江省微通经贸总公司",
"黑龙江大阳房地产开发股份有限公司",
"黑龙江省隆福装饰公司",
"黑龙江中建工程公司",
"黑龙江省联友经济贸易有限公司",
"黑龙江省华乡贸易股份有限公司",
"黑龙江省大远房地产开发公司",
"黑龙江省龙腾房地产综合开发公司",
"黑龙江省宏达房地产综合开发公司",
"黑龙江银建工程监理公司",
"黑龙江群智科技开发公司",
"黑龙江省外国企业服务公司",
"黑龙江华龙信息公司",
"黑龙江省地质矿产物资供销总公司",
"黑龙江省北方粮油经销公司",
"黑龙江省友联经济贸易公司",
"黑龙江省商技贸易公司",
"黑龙江省隆泰贸易公司",
"黑龙江省包装食品机械公司",
"黑龙江省洪发房地产综合开发公司",
"黑龙江腾达食品保健有限公司",
"黑龙江省昌达粮油股份有限公司",
"黑龙江大学经济贸易公司",
"黑龙江省二轻集体企业联社",
"黑龙江省物资信息公司",
"黑龙江福龙能源开发公司",
"黑龙江省松花江林业商业公司",
"黑龙江省国营农场总局烟酒公司",
"黑龙江省广源房地产综合开发公司",
"黑龙江省电力房地产综合开发公司",
"黑龙江省农垦产品交易中心",
"黑龙江省水利系统干部培训中心秀水宾馆",
"黑龙江省龙海工贸有限公司",
"黑龙江农电科技开发公司",
"黑龙江电力经营公司",
"黑龙江省合合房地产开发公司",
"黑龙江省宏渤经济贸易公司",
"黑龙江省有线电视开发公司",
"黑龙江省法律咨询顾问服务公司",
"黑龙江省环际经贸公司",
"黑龙江省龙宁经济贸易总公司",
"黑龙江省龙机经济技术开发公司",
"黑龙江农牧渔业经济技术开发总公司",
"黑龙江省达华经济贸易公司",
"黑龙江省德利经济贸易发展公司",
"黑龙江省华富达经济贸易公司",
"黑龙江中煤房地产综合开发公司",
"黑龙江省北兴贸易总公司",
"黑龙江省供销合作星宇贸易公司",
"黑龙江省电力物资总公司",
"黑龙江省龙建装饰公司",
"黑龙江兴业经济贸易公司",
"黑龙江省通信线路工程公司",
"哈尔滨铁路局工务科技开发公司",
"哈尔滨铁路局车辆技术开发公司",
"黑龙江省世达机械设备公司",
"黑龙江省华裕经济贸易公司",
"黑龙江省粮油运销公司",
"黑龙江银信股份有限公司",
"黑龙江省泥炭开发公司",
"黑龙江利达经济贸易总公司",
"黑龙江省迅通实业公司",
"黑龙江省信源贸易公司",
"黑龙江省医健科技开发公司",
"黑龙江省本原土地综合技术开发中心",
"黑龙江联友经济贸易公司",
"哈尔滨师范大学团山实业公司",
"黑龙江农垦石油燃料总公司",
"黑龙江农垦房地产综合开发总公司",
"黑龙江省欧亚经济贸易公司",
"黑龙江省商业经济技术开发公司",
"黑龙江省八达路桥建设有限公司",
"黑龙江省华苑贸易公司",
"黑龙江省北渔科技开发公司",
"黑龙江省乳品销售公司",
"黑龙江长通经贸总公司",
"黑龙江省龙威经济贸易公司",
"黑龙江省工交经济技术发展公司",
"黑龙江省北亚经贸有限公司",
"黑龙江省经纬工贸公司",
"黑龙江省森工物资储运公司",
"黑龙江省烟草房地产综合开发公司",
"黑龙江鑫北经济贸易公司",
"黑龙江省华威经济贸易公司",
"黑龙江省新业经济贸易公司",
"黑龙江省南北贸易公司",
"黑龙江省银联房地产发展有限公司",
"黑龙江省隆庆房地产发展有限公司",
"华电能源股份有限公司",
"黑龙江省有色金属矿业开发公司",
"黑龙江省林业煤炭运销公司",
"黑龙江省天华经济贸易公司",
"黑龙江华煤实业有限公司",
"黑龙江省国信房地产开发有限责任公司",
"黑龙江省泰丰经济贸易公司",
"黑龙江友迪信息开发公司",
"黑龙江东北煤炭矿山设备租赁公司",
"黑龙江省中轻贸易公司",
"黑龙江省龙滨房地产开发公司",
"黑龙江省边胜实业总公司",
"黑龙江国土资源开发实业总公司",
"黑龙江省贫困地区经济开发服务中心",
"黑龙江省宏远贸易公司",
"黑龙江省联达经济贸易公司",
"黑龙江省欧罗巴实业开发公司",
"黑龙江省机电产品市场",
"黑龙江省农发经济贸易有限公司",
"黑龙江省华发保健用品公司",
"黑龙江省国龙贸易有限公司",
"黑龙江高维企业集团有限公司",
"黑龙江省农垦建筑设计院",
"黑龙江省医药物资贸易公司",
"黑龙江省东亚房地产开发公司",
"黑龙江省有色金属物资供销总公司储运供销公司",
"黑龙江省草业开发公司",
"黑龙江省华昌经济技术发展总公司",
"黑龙江省军转房屋综合开发公司",
"黑龙江省纺织工贸有限公司",
"黑龙江省体育国际旅行社",
"黑龙江省建筑材料开发公司",
"黑龙江省商业贸易总公司",
"黑龙江省育达药业公司",
"黑龙江省农垦煤炭运销公司",
"黑龙江志诚房地产开发股份有限公司",
"哈尔滨铁路局建筑装饰工程公司",
"黑龙江省食品医药设计院有限责任公司",
"黑龙江省科工实业发展总公司装饰分公司",
"黑龙江省中兴经济贸易公司",
"黑龙江省粮食工程建设综合开发公司",
"黑龙江工大房地产综合开发公司",
"黑龙江省企联经济技术开发公司",
"黑龙江天择实业有限公司",
"黑龙江北大荒粮油经销总公司",
"黑龙江省宏盛经贸公司",
"黑龙江省通亚路桥工程公司",
"黑龙江新华经济发展公司",
"黑龙江省机电设备更新有限公司",
"黑龙江省信达经济事务所",
"黑龙江省极光经济贸易公司",
"黑龙江省正兴信息贸易公司",
"黑龙江省华夏建筑设计院",
"黑龙江省长宏经贸公司",
"黑龙江省旅游汽车公司",
"黑龙江省美乐制冷技术开发公司",
"黑龙江省福客来广告装饰公司",
"黑龙江省宏泰经济贸易公司",
"黑龙江省粮油实业开发公司",
"黑龙江农垦科技咨询开发公司",
"黑龙江省泰昌经济贸易公司",
"黑龙江省宝兴实业发展公司",
"黑龙江省科智经济贸易总公司",
"黑龙江省科达粮食干燥技术开发公司",
"黑龙江中村建筑设计所",
"黑龙江北极广告公司",
"黑龙江省水文地质工程地质勘察工程公司",
"黑龙江省天威(集团)公司",
"黑龙江农垦粮油经贸公司",
"黑龙江省江大房地产开发有限公司",
"黑龙江龙华会计师事务所有限公司",
"黑龙江电视广告公司",
"黑龙江省龙商经济贸易公司",
"黑龙江绿色食品公司",
"哈尔滨铁路局纪元经贸公司",
"黑龙江福日家电经销公司",
"黑龙江省祥通经济贸易公司",
"黑龙江同发房地产综合开发公司",
"黑龙江省亨通建筑工程公司",
"黑龙江省国际贸易经济发展公司",
"黑龙江省化学药业经贸公司",
"黑龙江巨龙房地产开发公司",
"黑龙江省恒利达技术开发公司",
"黑龙江省机械科技工贸公司",
"黑龙江省乳品批发交易中心",
"黑龙江省兴联物资贸易公司",
"黑龙江招商国际旅游公司",
"伊春中国国际旅行社有限责任公司",
"黑龙江省粮食建设监理公司",
"黑龙江省乳品设备成套安装工程公司",
"黑龙江省金利物资经济贸易公司",
"黑龙江省商物贸易公司",
"黑龙江省发电设备检修工程总公司",
"黑龙江外商企业咨询服务有限责任公司",
"黑龙江省蓝莓生物药品研究所有限公司",
"黑龙江海佩克工贸实业发展公司",
"黑龙江省欣佳经济贸易公司",
"黑龙江省水利水电实业公司",
"黑龙江省亚地实业公司",
"哈尔滨金京房地产开发有限公司",
"黑龙江省民福经济贸易公司",
"黑龙江省人闻建筑装饰工程有限公司",
"黑龙江水路运输代理服务公司",
"黑龙江省方圆经济贸易公司",
"黑龙江省新技建筑装饰工程公司",
"黑龙江信海房地产实业公司",
"黑龙江省宝通经贸发展公司",
"中国第一汽车集团贸易公司黑龙江联合公司",
"黑龙江省机械设备成套局",
"黑龙江新型墙体材料开发有限公司",
"黑龙江省龙微通信技术服务公司",
"黑龙江跨国采购中心有限公司",
"黑龙江省利都贸易公司",
"黑龙江省远大公司",
"黑龙江帝威房地产综合开发公司",
"黑龙江新宇投资发展有限公司",
"黑龙江省医用氧气总公司",
"黑龙江新生活经济贸易服务公司",
"黑龙江华源电力(集团)股份有限公司",
"黑龙江海资经贸公司",
"黑龙江省应用食品研究发展公司",
"黑龙江省口岸经贸公司",
"黑龙江省龙建水泥公司",
"黑龙江省建兴工程咨询公司",
"黑龙江省金亚房地产综合开发有限公司",
"黑龙江汇雅装饰有限公司",
"黑龙江省北方大厦",
"黑龙江天马房地产综合开发(集团)有限公司",
"黑龙江省开明经贸公司",
"黑龙江省机场管理集团有限公司",
"黑龙江中亚经贸公司",
"黑龙江省农垦商业企业总公司",
"黑龙江易达信息开发公司",
"新华房地产开发黑龙江公司",
"黑龙江省天成贸易公司",
"东北林业大学东亚电子仪器开发公司",
"黑龙江多维经济贸易公司",
"黑龙江省中国绘画艺术研究所",
"黑龙江省林区供销合作社联合社农业生产资料公司",
"牡丹江中国国际旅行社有限责任公司",
"黑龙江立信会计师事务所有限责任公司",
"黑龙江省物资房地产综合开发公司",
"黑龙江省龙安警用装备调配服务中心",
"黑龙江益龙会计师事务所有限公司",
"黑龙江创威经济贸易公司",
"黑龙江五环智力开发公司",
"东北林业大学工程咨询设计研究院有限公司",
"黑龙江省棉麻公司",
"黑龙江省中苑贸易公司",
"黑龙江东煤设备制造多种经营总公司",
"东北林业大学经济技术开发总公司",
"黑龙江省佳联经济贸易公司",
"黑龙江省地质矿产局测绘院",
"黑龙江省油田开发股份有限公司",
"黑龙江物产(集团)总公司",
"黑龙江省松花江地区商业经济贸易总公司",
"黑龙江省高速公路集团公司",
"黑龙江省大远电子技术开发公司",
"黑龙江省龙机包装食品设备公司",
"大庆头台油田开发有限责任公司",
"黑龙江乳业集团总公司",
"黑龙江省煤炭设计研究院(哈尔滨煤炭设计研究院)",
"黑龙江省三江农业物资贸易公司",
"哈尔滨铁路局工程咨询公司",
"黑龙江商华技术贸易有限公司",
"黑龙江省地发综合服务公司",
"京蓝科技股份有限公司",
"黑龙江外贸华润实业总公司",
"黑龙江华兴会计师事务所有限公司",
"黑龙江省粮油食品进出口(集团)利达贸易公司",
"黑龙江交通实业总公司",
"黑龙江省军转建筑安装公司",
"黑龙江省商业设计院",
"黑龙江省利农植保公司",
"黑龙江省电子工业开发公司",
"黑龙江省广通公路工程公司",
"黑龙江省乡镇企业生产资料公司",
"中煤东北设备成套公司",
"黑龙江正方会计师事务所有限责任公司",
"黑龙江省公路工程监理咨询公司",
"黑龙江省工商行政管理局劳动服务公司",
"黑龙江省工业锅炉成套安装公司",
"黑龙江省远东宾馆",
"黑龙江省国联资产评估事务所",
"黑龙江明嘉透平实业股份有限公司",
"黑龙江省纪元工贸实业发展公司",
"黑龙江省东亚商贸公司",
"黑龙江省文化用品公司",
"黑龙江省土产畜产进出口公司新香坊土畜产加工厂",
"黑龙江广电广告公司",
"黑龙江省永固桩基础工程公司",
"黑龙江省科星医药研究所",
"黑龙江省旅游产品生产供应公司",
"黑龙江省物资再生利用二公司",
"黑龙江省物资再生利用一公司",
"黑龙江海格智能化设施安装有限责任公司",
"黑龙江飞天建筑设计公司",
"黑龙江省新源保健食品开发公司",
"黑龙江雨辰工程地质勘察公司",
"黑龙江中煤建筑装饰工程公司",
"黑龙江省机动车辆资产评估事务所",
"黑龙江龙涤股份有限公司",
"黑龙江省宏信经济贸易公司",
"哈尔滨铁路局直属工程公司",
"黑龙江省农机经济技术开发公司",
"黑龙江经闻贸易公司",
"黑龙江美术图书发行公司",
"黑龙江省经济技术联合发展总公司",
"黑龙江省建筑材科技开发有限责任公司",
"黑龙江省中艺装饰公司",
"黑龙江广播发展经济贸易公司",
"黑龙江省龙烟经贸商行",
"黑龙江新生活文化传播中心",
"黑龙江省恒兴道桥工程有限责任公司",
"黑龙江海外经济贸易有限责任公司",
"黑龙江远大经济技术发展公司",
"黑龙江省精良民用技术开发公司",
"黑龙江省光远房地产开发有限公司",
"黑龙江华龙公路工程咨询监理公司",
"黑龙江省龙机刀具技术开发有限责任公司",
"哈尔滨太平洋期货交易服务有限公司",
"黑龙江中亚煤炭建设监理中心",
"大庆新华企业集团股份有限公司",
"黑龙江省科宏地质勘查开发有限责任公司",
"黑龙江天泰煤矿有限责任公司",
"黑龙江中铁建设监理有限责任公司",
"黑龙江天问文化实业有限公司.",
"黑龙江省石天房地产开发有限公司",
"黑龙江省饮食服务贸易(集团)总公司",
"黑龙江龙发建筑工程有限公司",
"黑龙江省商通有限责任公司",
"黑龙江精化机械技术服务中心",
"黑龙江三吉利广告有限公司",
"黑龙江良信汽车电器科技开发有限公司",
"黑龙江省纺联物资有限公司",
"黑龙江省工程农机交易市场",
"黑龙江中汽汽车自选市场",
"黑龙江省水利工程建设监理公司",
"黑龙江江龙建设监理所",
"黑龙江省九星机房设施工程有限公司",
"黑龙江成功房地产开发有限责任公司",
"黑龙江省烟叶物资公司",
"黑龙江省宏图有限责任公司",
"黑龙江省龙铁哈尔滨市产权交易中心",
"黑龙江省安通咨询服务有限公司",
"黑龙江省双龙兽药厂",
"黑龙江北满特钢联营销售中心",
"黑龙江达通建设发展有限责任公司",
"黑龙江省万利食品有限公司",
"黑龙江省电信技术服务中心",
"黑龙江省通贸有限责任公司",
"黑龙江省国际货物运输代理总公司",
"黑龙江龙华设备安装有限公司",
"黑龙江中大实业开发总公司",
"黑龙江天正实业有限公司",
"黑龙江省北鹏经贸有限公司",
"哈尔滨铁路局齐齐哈尔铁路分局",
"黑龙江省银发石油销售发展有限公司",
"黑龙江电力建设监理有限责任公司",
"黑龙江省昌泰经贸有限公司",
"黑龙江省光通电信工程有限公司",
"哈尔滨集成医药有限公司",
"黑龙江省恒运房地产（集团）股份有限公司",
"龙电集团有限公司",
"黑龙江省龙腾房地产综合开发有限公司",
"黑龙江省科学物资发展有限责任公司",
"黑龙江省地方煤炭工业（集团）总公司",
"北京吉普汽车黑龙江省特约销售有限责任公司",
"黑龙江国煤商务服务有限公司",
"黑龙江省气象广告有限公司",
"黑龙江省华滨石油化工有限公司",
"哈尔滨新兴进出口有限责任公司",
"黑龙江省振中经济贸易有限公司",
"黑龙江天马经贸有限公司",
"黑龙江黑航工程监理咨询有限公司",
"黑龙江省福达医药经销有限公司",
"黑龙江海员对外技术服务有限责任公司",
"黑龙江省通信线路工程设计所",
"黑龙江省正达养殖有限责任公司",
"葆祥黑龙江进出口有限责任公司",
"黑龙江省兴凯湖兴海米业有限公司",
"黑龙江省本原国土资源勘测规划技术服务中心",
"北京华审金信会计师事务所有限责任公司",
"黑龙江三峡经济发展公司",
"黑龙江宏业工程建设监理有限责任公司",
"黑龙江北方油脂技术开发有限责任公司",
"黑龙江省建兴资产评估有限公司",
"黑龙江鹏龙实业发展有限公司",
"黑龙江凌武物资经贸处",
"黑龙江省煤炭开发公司",
"黑龙江宏明经贸有限公司",
"黑龙江东地经济技术合作有限责任公司",
"黑龙江省旺达经贸有限责任公司",
"黑龙江省新华书店批销中心有限公司",
"轩辕集团实业开发有限责任公司",
"黑龙江省计量器具制造销售中心",
"大庆榆树林油田开发有限责任公司",
"黑龙江省警隆有限公司",
"黑龙江海外房地产开发集团有限公司",
"黑龙江牡丹电子有限责任公司",
"黑龙江省华福经贸有限公司",
"黑龙江省保安电子工程有限公司",
"黑龙江省龙纺建设工程承包有限公司",
"黑龙江省茗茶有限责任公司",
"黑龙江民航大厦",
"黑龙江省三宝养殖有限责任公司",
"黑龙江龙运报关有限公司",
"黑龙江省直属粮库管理有限公司",
"黑龙江省北大荒集团哈尔滨销售中心",
"黑龙江北联贸易有限责任公司",
"黑龙江省瑞华股份有限公司",
"黑龙江外贸国运报关行",
"黑龙江省畜产公司",
"黑龙江现代文化艺术发展中心",
"黑龙江金城建筑安装工程有限公司",
"哈尔滨铁路局资产评估事务所",
"黑龙江省新兴粮油有限公司",
"黑龙江省华宇高科技开发有限公司",
"黑龙江省中远电子有限责任公司",
"黑龙江省粮丰实业发展有限公司",
"黑龙江省迈克能源技术有限公司",
"黑龙江省祥泰建筑工程有限公司",
"哈尔滨铁路局集装箱运输中心",
"黑龙江省双亚经贸有限责任公司",
"黑龙江恒升经贸有限公司",
"黑龙江富利达建筑工程股份有限公司",
"黑龙江省中天实业发展有限公司",
"黑龙江太平洋纯水技术有限责任公司",
"哈尔滨国际旅行社",
"鸡东县边境经济贸易公司",
"黑龙江劳动职业信息服务中心",
"黑龙江省华业轻纺工业有限公司",
"黑龙江省澳利达药业股份有限公司",
"黑龙江省环宇贸易有限责任公司",
"黑龙江省冶金工程建设监理有限责任公司",
"黑龙江省太阳岛垂钓娱乐有限责任公司",
"黑龙江鞍达实业集团股份有限公司",
"黑龙江省中科贸易有限责任公司",
"黑龙江省松花江化工经贸有限公司",
"黑龙江华大抽纱有限责任公司",
"黑龙江省奋斗广告装璜有限责任公司",
"中国黑龙江外轮代理公司",
"黑龙江龙门大厦酒店有限责任公司",
"黑龙江省北大荒绿色食品销售中心",
"黑龙江大成建筑涂料厂",
"黑龙江省三合实业有限公司",
"黑龙江省华江经济贸易有限责任公司",
"黑龙江省金林钢铁经销有限责任公司",
"黑龙江省华远建筑安装工程有限公司",
"黑龙江省国龙医药有限公司",
"黑龙江国惠会计师事务所有限公司",
"黑龙江省龙宁房地产综合开发有限责任公司",
"黑龙江中广信会计师事务所有限责任公司",
"黑龙江省瑞达药业有限公司",
"黑龙江省泰都物资经贸有限公司",
"黑龙江省华赢科工贸有限公司",
"黑龙江省阿里特贸易有限责任公司",
"黑龙江盘古会计师事务所有限公司",
"黑龙江三乐经济贸易有限责任公司",
"黑龙江现代会计师事务所有限责任公司",
"黑龙江省供销仓储有限公司",
"黑龙江中益经济发展有限责任公司",
"黑龙江省新华书店有限公司",
"黑龙江宏运报关有限公司",
"黑龙江省家用电子产品维修中心",
"黑龙江省天成装饰有限责任公司",
"黑龙江省瀚林伟业建筑装饰工程有限公司",
"黑龙江省东方女神鲜啤开发有限责任公司",
"黑龙江龙贸房地产综合开发有限责任公司",
"黑龙江星海建设工程发展有限公司",
"黑龙江金谷科技开发有限责任公司",
"黑龙江省金兴森工木片有限责任公司",
"东北金城实业黑龙江公司",
"黑龙江省华远房地产综合开发有限公司",
"黑龙江利源经贸有限公司",
"黑龙江诺亚经济技术开发有限公司",
"黑龙江铁海储运有限责任公司",
"黑龙江隆昇昌建筑工程有限公司",
"黑龙江省天鸿经济贸易有限公司",
"黑龙江省人防工程建设监理有限责任公司",
"黑龙江省求实工程经济咨询服务有限责任公司",
"黑龙江省建筑标准设计研究院有限公司",
"黑龙江金世纪房地产开发有限责任公司",
"黑龙江省亚星工贸发展有限责任公司",
"黑龙江海外进出口有限责任公司",
"黑龙江民航广告有限责任公司",
"黑龙江万弘建筑工程有限责任公司",
"黑龙江省供销再生资源开发有限公司",
"黑龙江省龙煤装饰有限责任公司",
"黑龙江现代经济技术开发（集团）有限公司",
"黑龙江省华信药业有限公司",
"黑龙江省康信医药有限责任公司",
"哈尔滨冷冻厂",
"黑龙江省中贝技术有限公司",
"黑龙江省宏力装饰工程有限公司",
"黑龙江省炎黄文化经济技术发展公司",
"黑龙江安防监控工程有限责任公司",
"黑龙江省金博医学科技开发推广中心",
"黑龙江省冠达经济贸易有限责任公司",
"黑龙江省博达建筑工程设计有限公司",
"黑龙江铁龙经贸有限责任公司",
"黑龙江省永信经济发展有限责任公司",
"黑龙江省同力装饰有限责任公司",
"中国能源建设集团黑龙江省火电第一工程有限公司",
"黑龙江省旭龙经贸有限责任公司",
"黑龙江万龙茶叶饮品有限公司",
"黑龙江益龙贸易有限责任公司",
"黑龙江远通公路桥梁工程有限责任公司",
"大庆兴谊股份有限公司",
"黑龙江省天元汽车维修有限公司",
"黑龙江省天元实业有限责任公司",
"黑龙江省中实锅炉安装工程有限公司",
"黑龙江东煤煤炭销售有限公司",
"黑龙江省恒润装饰有限公司",
"黑龙江省兴盐粮油贸易有限责任公司",
"黑龙江瑞盛贸易有限公司",
"黑龙江三辰贸易有限公司",
"黑龙江省明鉴经贸有限责任公司",
"黑龙江省金建计算机系统技术有限公司",
"黑龙江省万达酒店设备有限公司",
"黑龙江中地贸易有限公司",
"黑龙江资材贸易有限公司",
"黑龙江同致贸易有限公司",
"黑龙江新艺贸易有限公司",
"黑龙江佳信贸易有限公司",
"黑龙江省龙经技术开发有限公司",
"黑龙江双鼎贸易有限公司",
"黑龙江省开元旅游贸易有限公司",
"黑龙江众业贸易有限公司",
"黑龙江省金泉自动化办公机具有限公司",
"黑龙江亨通建筑经济咨询服务有限公司",
"黑龙江高维投资顾问有限公司",
"黑龙江和信经贸有限公司",
"黑龙江省龙腾集团股份有限公司",
"黑龙江天马物业管理有限公司",
"黑龙江省日月广告有限公司",
"黑龙江绿色食品集团有限公司",
"黑龙江省晓龙房地产开发有限公司",
"黑龙江大众广告有限责任公司",
"黑龙江省新特药业有限公司",
"黑龙江天胤经贸发展有限责任公司",
"黑龙江省建兴房地产中介有限公司",
"黑龙江金谷大厦",
"黑龙江鑫海实业(集团)股份有限公司",
"黑龙江省中地水处理技术开发有限责任公司",
"黑龙江钱塘物业有限责任公司",
"黑龙江省华融集团股份有限公司",
"黑龙江省华瑞电子技术有限公司",
"黑龙江省永威经贸有限公司",
"黑龙江省天宇经贸有限责任公司",
"黑龙江省海外实业有限公司",
"黑龙江省天鹅土地资源开发有限公司",
"黑龙江国大保健药业有限公司",
"黑龙江省龙昊实业发展有限公司",
"黑龙江省巨龙经济贸易有限公司",
"黑龙江省省通汽车救助维修服务有限公司",
"黑龙江复康保健药业有限公司",
"黑龙江农垦农业生产资料总公司",
"黑龙江省双福节能有限公司",
"黑龙江友邦建筑技术工程有限公司",
"黑龙江新洲实业发展集团有限公司",
"黑龙江省青岩实业有限责任公司",
"黑龙江蕙通房地产开发有限公司",
"黑龙江和信经济文化传播有限责任公司",
"黑龙江捷利监管货场服务有限公司",
"黑龙江金远经济贸易有限公司",
"黑龙江省机电设备招标局",
"黑龙江省金辰机电工程安装有限责任公司",
"黑龙江省哈丰贸易有限责任公司",
"黑龙江省特力农业发展有限责任公司",
"黑龙江省天虹通信技术发展有限责任公司",
"黑龙江兴中企业委托经营有限公司",
"黑龙江省美达实业有限责任公司",
"黑龙江省瑞驰建设集团有限责任公司",
"黑龙江省黎明金属回收有限责任公司",
"黑龙江达利影印事业有限公司",
"黑龙江金麟经贸有限公司",
"黑龙江省物资拍卖行",
"黑龙江天楹公共关系服务有限责任公司",
"黑龙江省圣基电声有限责任公司",
"黑龙江中桥广告装饰有限公司",
"黑龙江省东大工矿设备有限公司",
"黑龙江省融达生产资料有限责任公司",
"黑龙江三维秸杆饲料有限责任公司",
"黑龙江宇远商品混凝土有限公司",
"黑龙江省庆华建筑工程有限公司",
"黑龙江省北方木业有限公司",
"黑龙江鑫隆达实业发展有限责任公司",
"黑龙江京安农业综合开发有限责任公司",
"黑龙江星海建筑装饰工程有限公司",
"黑龙江隆基经济贸易有限责任公司",
"大庆石油国际工程公司",
"黑龙江省天鑫饲料有限责任公司",
"黑龙江省天一建筑有限责任公司",
"黑龙江精业包装有限责任公司",
"黑龙江北大荒酿酒（集团）有限责任公司",
"黑龙江红星参茸制品有限公司",
"黑龙江北国投资顾问有限责任公司",
"黑龙江省骏达经贸有限责任公司",
"黑龙江天赐实业有限公司",
"黑龙江华正农业经济发展有限责任公司",
"黑龙江奥联影视文化传播有限公司",
"奉化市日用机械厂",
"黑龙江省天人建筑装饰有限公司",
"黑龙江省林区供销合作社联合社",
"黑龙江省新新新药特药有限责任公司",
"黑龙江焰圣液化石油气开发有限责任公司",
"黑龙江蓝星化学清洗有限责任公司",
"黑龙江惠德纺织有限责任公司",
"黑龙江省龙飞建设开发有限公司",
"黑龙江省龙贸拍卖有限公司",
"黑龙江博远汽车维修有限公司",
"黑龙江盛昌纺织品有限责任公司",
"黑龙江华夏建筑装饰有限责任公司",
"黑龙江省凤吉经贸有限责任公司",
"黑龙江省四维岩土工程有限责任公司",
"黑龙江省振安消防设施安装工程有限公司",
"新华社黑龙江音像中心",
"黑龙江省鑫大动物养殖有限责任公司",
"黑龙江北鸽拍卖有限责任公司",
"黑龙江中海医药有限责任公司",
"黑龙江行星农机有限责任公司",
"黑龙江省老区经济开发有限公司",
"黑龙江省正创建筑工程有限责任公司",
"黑龙江省远东能源发有限责任公司",
"黑龙江龙电电力工程咨询有限责任公司",
"黑龙江省秋宝工程建设监理有限责任公司",
"黑龙江达深岩土工程有限责任公司",
"黑龙江东风经济贸易有限责任公司",
"大庆南垣集团股份有限公司",
"黑龙江省火电第一工程公司多种经营总公司",
"黑龙江省振江农机有限公司",
"黑龙江省东北包装食品机械有限公司",
"黑龙江省东大有色金属材料有限公司",
"慧谷盛志投资股份有限公司",
"黑龙江省神宇经贸有限公司",
"中国外运黑龙江饶河公司",
"中国外运黑龙江富锦公司",
"中国外运黑龙江嘉荫公司",
"黑龙江省龙烟贸易有限责任公司",
"黑龙江省运隆贸易有限责任公司",
"黑龙江省松达消防设施安装有限责任公司",
"黑龙江欧凯商贸有限公司",
"齐齐哈尔龙铁建筑安装股份有限公司",
"黑龙江省泰格药业有限公司",
"黑龙江省欧罗巴机电有限公司",
"黑龙江省松林石油有限公司",
"黑龙江正鑫化工厂",
"黑龙江省建华煤炭经销有限责任公司",
"黑龙江省恒泰医药有限责任公司",
"黑龙江聚兴(集团)股份有限公司",
"天津汽车工业销售黑龙江有限公司",
"黑龙江省康宏医药有限责任公司",
"黑龙江省大吉汽车销售有限公司",
"黑龙江省龙腾建设监理有限公司",
"黑龙江省中贸经济发展有限责任公司",
"黑龙江太平洋医药有限责任公司",
"黑龙江省祥和资产评估事务所",
"大庆市北方通讯股份有限公司",
"黑龙江中发物业管理有限公司",
"黑龙江省发达经贸有限责任公司",
"黑龙江全方工程建设监理有限责任公司",
"黑龙江利达电缆材料股份有限公司",
"黑龙江龙育会计师事务所",
"黑龙江大成空调自动化工程有限公司",
"黑龙江省中龙农业综合开发有限公司",
"黑龙江海威物资贸易有限责任公司",
"黑龙江省国商粮油贸易有限责任公司",
"黑龙江省嘉实经贸有限公司",
"黑龙江省龙骧农副产品经销有限公司",
"黑龙江电力实业投资有限公司",
"光明集团股份有限公司",
"黑龙江地铁发展有限公司",
"黑龙江现代建筑工程有限公司",
"黑龙江现代房地产开发有限公司",
"黑龙江绿丰生物有机肥料有限责任公司",
"黑龙江省丰华经贸有限公司",
"黑龙江省嘉隆经济贸易有限公司",
"大庆恒致电缆材料股份有限公司",
"黑龙江省科利达化工有限公司",
"黑龙江华新会计师事务所有限公司",
"黑龙江金信经济贸易有限责任公司",
"黑龙江恒洋氨基酸技术开发有限公司",
"黑龙江省金波煤炭经销有限公司",
"黑龙江永威纸业有限公司",
"黑龙江省瑞泽医药有限公司",
"黑龙江鑫合贸易有限责任公司",
"黑龙江省广盛达贸易有限公司",
"黑龙江鑫汇经贸有限责任公司",
"黑龙江省维义外贸商品销售有限公司",
"黑龙江国利工程管理有限公司",
"黑龙江国投实业发展有限责任公司",
"黑龙江直立电梯实业有限公司",
"黑龙江省圣大生物工程有限责任公司",
"黑龙江省新纪元经贸有限责任公司",
"黑龙江省新兴龙经贸有限责任公司",
"黑龙江省格瑞谷物进出口有限公司",
"黑龙江省金厦建设开发有限公司",
"黑龙江现代绿色工程技术有限公司",
"黑龙江墨龙经贸有限责任公司",
"黑龙江省百吉装饰设计工程有限公司",
"黑龙江路通电器有限责任公司",
"黑龙江环宇和平卫星应用有限公司",
"黑龙江兴达实业股份有限公司",
"黑龙江网联经济信息有限公司",
"黑龙江新时代广告有限责任公司",
"黑龙江省隆旺经贸有限公司",
"黑龙江省东南石化经贸有限公司",
"黑龙江省喜盈门建筑装饰工程有限公司",
"黑龙江夕阳红医药保健品有限公司",
"黑龙江飞天建设工程总承包有限责任公司",
"哈尔滨市查家粮食仓库",
"黑龙江嘉泰贸易发展有限责任公司",
"黑龙江瑞全制药厂",
"黑龙江联帮工程股份有限公司",
"黑龙江椰风食品贸易有限公司",
"黑龙江省江铁贸易发展有限责任公司",
"黑龙江友谊货运代理有限公司",
"黑龙江省高路华电器经销有限责任公司",
"黑龙江饲料原料中转储备中心",
"黑龙江仙池矿泉资源开发有限责任公司",
"黑龙江圣泰[集团]股份有限公司",
"黑龙江省丰庆种粮有限责任公司",
"黑龙江省国方工业供销有限责任公司",
"黑龙江信诚租赁经纪有限公司",
"黑龙江长瑞计算机系统有限公司",
"黑龙江鸿联信息技术有限公司",
"黑龙江友联房地产综合开发有限公司",
"黑龙江省时代电气有限公司",
"黑龙江超越会计师事务所有限公司",
"黑龙江省天地广告有限责任公司",
"黑龙江丰瑞会计师事务所有限公司",

            };
            var res = new List<string>(1000);
            var sb = new StringBuilder();
            for (int j = 0; j < testCases.Length; j++)
            {
                var terms = Com_CRFSegment.Segment(testCases[j]);
                
                for (int i = 0; i < terms.Count; i++)
                {
                    if (i != 0)
                        sb.Append("/ ");
                    sb.Append(terms[i]);
                }
                res.Add(sb.ToString());
                sb.Clear();
            }
        }
    }
}
