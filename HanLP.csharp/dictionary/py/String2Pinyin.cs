using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HanLP.csharp.algorithm.ahocorasick;
using HanLP.csharp.collection;
namespace HanLP.csharp.dictionary.py
{
    public class String2Pinyin
    {
        private static Trie _trie;
        private static IDictionary<string, Pinyin> _map;

        /// <summary>
        /// 音调统一换为轻声，下标为拼音枚举值，值为音调5或最大值
        /// </summary>
        public static PYName[] Tone2tone5 = new[] { PYName.a5, PYName.a5, PYName.a5, PYName.a5, PYName.a5, PYName.ai4, PYName.ai4, PYName.ai4, PYName.ai4,
            PYName.an4, PYName.an4, PYName.an4, PYName.an4, PYName.ang4, PYName.ang4, PYName.ang4, PYName.ang4, PYName.ao4, PYName.ao4, PYName.ao4, PYName.ao4,
            PYName.ba5, PYName.ba5, PYName.ba5, PYName.ba5, PYName.ba5, PYName.bai4, PYName.bai4, PYName.bai4, PYName.bai4, PYName.ban4, PYName.ban4, PYName.ban4,
            PYName.bang4, PYName.bang4, PYName.bang4, PYName.bao4, PYName.bao4, PYName.bao4, PYName.bao4, PYName.bei5, PYName.bei5, PYName.bei5, PYName.bei5,
            PYName.ben4, PYName.ben4, PYName.ben4, PYName.beng4, PYName.beng4, PYName.beng4, PYName.beng4, PYName.bi4, PYName.bi4, PYName.bi4, PYName.bi4, PYName.bian5,
            PYName.bian5, PYName.bian5, PYName.bian5, PYName.biao4, PYName.biao4, PYName.biao4, PYName.biao4, PYName.bie4, PYName.bie4, PYName.bie4, PYName.bie4,
            PYName.bin4, PYName.bin4, PYName.bin4, PYName.bing4, PYName.bing4, PYName.bing4, PYName.bo5, PYName.bo5, PYName.bo5, PYName.bo5, PYName.bo5, PYName.bu4,
            PYName.bu4, PYName.bu4, PYName.bu4, PYName.ca4, PYName.ca4, PYName.ca4, PYName.cai4, PYName.cai4, PYName.cai4, PYName.cai4, PYName.can4, PYName.can4, PYName.can4,
            PYName.can4, PYName.cang4, PYName.cang4, PYName.cang4, PYName.cang4, PYName.cao4, PYName.cao4, PYName.cao4, PYName.cao4, PYName.ce4, PYName.cen2, PYName.cen2,
            PYName.ceng4, PYName.ceng4, PYName.ceng4, PYName.cha5, PYName.cha5, PYName.cha5, PYName.cha5, PYName.cha5, PYName.chai4, PYName.chai4, PYName.chai4, PYName.chai4,
            PYName.chan4, PYName.chan4, PYName.chan4, PYName.chan4, PYName.chang5, PYName.chang5, PYName.chang5, PYName.chang5, PYName.chang5, PYName.chao4, PYName.chao4,
            PYName.chao4, PYName.chao4, PYName.che4, PYName.che4, PYName.che4, PYName.chen5, PYName.chen5, PYName.chen5, PYName.chen5, PYName.chen5, PYName.cheng4,
            PYName.cheng4, PYName.cheng4, PYName.cheng4, PYName.chi5, PYName.chi5, PYName.chi5, PYName.chi5, PYName.chi5, PYName.chong4, PYName.chong4, PYName.chong4,
            PYName.chong4, PYName.chou5, PYName.chou5, PYName.chou5, PYName.chou5, PYName.chou5, PYName.chu5, PYName.chu5, PYName.chu5, PYName.chu5, PYName.chu5, PYName.chua1,
            PYName.chuai4, PYName.chuai4, PYName.chuai4, PYName.chuai4, PYName.chuan4, PYName.chuan4, PYName.chuan4, PYName.chuan4, PYName.chuang4, PYName.chuang4, PYName.chuang4,
            PYName.chuang4, PYName.chui4, PYName.chui4, PYName.chui4, PYName.chun3, PYName.chun3, PYName.chun3, PYName.chuo5, PYName.chuo5, PYName.chuo5, PYName.chuo5, PYName.ci4,
            PYName.ci4, PYName.ci4, PYName.ci4, PYName.cong4, PYName.cong4, PYName.cong4, PYName.cou4, PYName.cou4, PYName.cu4, PYName.cu4, PYName.cu4, PYName.cu4, PYName.cuan4,
            PYName.cuan4, PYName.cuan4, PYName.cui4, PYName.cui4, PYName.cui4, PYName.cui4, PYName.cun4, PYName.cun4, PYName.cun4, PYName.cun4, PYName.cuo4, PYName.cuo4, PYName.cuo4,
            PYName.cuo4, PYName.da5, PYName.da5, PYName.da5, PYName.da5, PYName.da5, PYName.dai4, PYName.dai4, PYName.dai4, PYName.dan4, PYName.dan4, PYName.dan4, PYName.dan4,
            PYName.dang4, PYName.dang4, PYName.dang4, PYName.dao4, PYName.dao4, PYName.dao4, PYName.dao4, PYName.de5, PYName.de5, PYName.de5, PYName.dei3, PYName.dei3, PYName.den4,
            PYName.den4, PYName.deng4, PYName.deng4, PYName.deng4, PYName.di4, PYName.di4, PYName.di4, PYName.di4, PYName.dia3, PYName.dian4, PYName.dian4, PYName.dian4,
            PYName.diao4, PYName.diao4, PYName.diao4, PYName.die4, PYName.die4, PYName.die4, PYName.ding4, PYName.ding4, PYName.ding4, PYName.ding4, PYName.diu1, PYName.dong4,
            PYName.dong4, PYName.dong4, PYName.dou4, PYName.dou4, PYName.dou4, PYName.dou4, PYName.du4, PYName.du4, PYName.du4, PYName.du4, PYName.duan4, PYName.duan4, PYName.duan4,
            PYName.dui4, PYName.dui4, PYName.dui4, PYName.dun4, PYName.dun4, PYName.dun4, PYName.dun4, PYName.duo5, PYName.duo5, PYName.duo5, PYName.duo5, PYName.duo5, PYName.e4,
            PYName.e4, PYName.e4, PYName.e4, PYName.ei4, PYName.ei4, PYName.ei4, PYName.ei4, PYName.en4, PYName.en4, PYName.en4, PYName.eng1, PYName.er5, PYName.er5, PYName.er5,
            PYName.er5, PYName.fa4, PYName.fa4, PYName.fa4, PYName.fa4, PYName.fan4, PYName.fan4, PYName.fan4, PYName.fan4, PYName.fang5, PYName.fang5, PYName.fang5, PYName.fang5,
            PYName.fang5, PYName.fei4, PYName.fei4, PYName.fei4, PYName.fei4, PYName.fen4, PYName.fen4, PYName.fen4, PYName.fen4, PYName.feng4, PYName.feng4, PYName.feng4,
            PYName.feng4, PYName.fiao4, PYName.fo2, PYName.fou4, PYName.fou4, PYName.fou4, PYName.fou4, PYName.fu5, PYName.fu5, PYName.fu5, PYName.fu5, PYName.fu5, PYName.ga4,
            PYName.ga4, PYName.ga4, PYName.ga4, PYName.gai4, PYName.gai4, PYName.gai4, PYName.gan4, PYName.gan4, PYName.gan4, PYName.gan4, PYName.gang4, PYName.gang4, PYName.gang4,
            PYName.gao4, PYName.gao4, PYName.gao4, PYName.ge4, PYName.ge4, PYName.ge4, PYName.ge4, PYName.gei3, PYName.gen4, PYName.gen4, PYName.gen4, PYName.gen4, PYName.geng4,
            PYName.geng4, PYName.geng4, PYName.gong4, PYName.gong4, PYName.gong4, PYName.gou4, PYName.gou4, PYName.gou4, PYName.gu4, PYName.gu4, PYName.gu4, PYName.gu4, PYName.gua4,
            PYName.gua4, PYName.gua4, PYName.guai4, PYName.guai4, PYName.guai4, PYName.guai4, PYName.guan4, PYName.guan4, PYName.guan4, PYName.guang4, PYName.guang4, PYName.guang4,
            PYName.gui4, PYName.gui4, PYName.gui4, PYName.gui4, PYName.gun4, PYName.gun4, PYName.gun4, PYName.guo5, PYName.guo5, PYName.guo5, PYName.guo5, PYName.guo5, PYName.ha4,
            PYName.ha4, PYName.ha4, PYName.ha4, PYName.hai4, PYName.hai4, PYName.hai4, PYName.hai4, PYName.han5, PYName.han5, PYName.han5, PYName.han5, PYName.han5, PYName.hang4,
            PYName.hang4, PYName.hang4, PYName.hang4, PYName.hao4, PYName.hao4, PYName.hao4, PYName.hao4, PYName.he4, PYName.he4, PYName.he4, PYName.hei1, PYName.hen4, PYName.hen4,
            PYName.hen4, PYName.hen4, PYName.heng4, PYName.heng4, PYName.heng4, PYName.hong4, PYName.hong4, PYName.hong4, PYName.hong4, PYName.hou4, PYName.hou4, PYName.hou4,
            PYName.hou4, PYName.hu4, PYName.hu4, PYName.hu4, PYName.hu4, PYName.hua4, PYName.hua4, PYName.hua4, PYName.hua4, PYName.huai4, PYName.huai4, PYName.huai4, PYName.huan4,
            PYName.huan4, PYName.huan4, PYName.huan4, PYName.huang4, PYName.huang4, PYName.huang4, PYName.huang4, PYName.hui4, PYName.hui4, PYName.hui4, PYName.hui4, PYName.hun4,
            PYName.hun4, PYName.hun4, PYName.hun4, PYName.huo5, PYName.huo5, PYName.huo5, PYName.huo5, PYName.huo5, PYName.ja4, PYName.ji5, PYName.ji5, PYName.ji5, PYName.ji5,
            PYName.ji5, PYName.jia5, PYName.jia5, PYName.jia5, PYName.jia5, PYName.jia5, PYName.jian4, PYName.jian4, PYName.jian4, PYName.jiang4, PYName.jiang4, PYName.jiang4,
            PYName.jiao4, PYName.jiao4, PYName.jiao4, PYName.jiao4, PYName.jie5, PYName.jie5, PYName.jie5, PYName.jie5, PYName.jie5, PYName.jin4, PYName.jin4, PYName.jin4,
            PYName.jing4, PYName.jing4, PYName.jing4, PYName.jiong4, PYName.jiong4, PYName.jiong4, PYName.jiu4, PYName.jiu4, PYName.jiu4, PYName.ju5, PYName.ju5, PYName.ju5,
            PYName.ju5, PYName.ju5, PYName.juan4, PYName.juan4, PYName.juan4, PYName.juan4, PYName.jue4, PYName.jue4, PYName.jue4, PYName.jue4, PYName.jun4, PYName.jun4,
            PYName.jun4, PYName.ka4, PYName.ka4, PYName.ka4, PYName.kai4, PYName.kai4, PYName.kai4, PYName.kan4, PYName.kan4, PYName.kan4, PYName.kang4, PYName.kang4,
            PYName.kang4, PYName.kang4, PYName.kao4, PYName.kao4, PYName.kao4, PYName.kao4, PYName.ke5, PYName.ke5, PYName.ke5, PYName.ke5, PYName.ke5, PYName.kei1, PYName.ken4,
            PYName.ken4, PYName.keng3, PYName.keng3, PYName.kong4, PYName.kong4, PYName.kong4, PYName.kou4, PYName.kou4, PYName.kou4, PYName.ku4, PYName.ku4, PYName.ku4,
            PYName.kua4, PYName.kua4, PYName.kua4, PYName.kuai4, PYName.kuai4, PYName.kuai4, PYName.kuan3, PYName.kuan3, PYName.kuang4, PYName.kuang4, PYName.kuang4, PYName.kuang4,
            PYName.kui4, PYName.kui4, PYName.kui4, PYName.kui4, PYName.kun4, PYName.kun4, PYName.kun4, PYName.kuo4, PYName.kuo4, PYName.la5, PYName.la5, PYName.la5, PYName.la5,
            PYName.la5, PYName.lai4, PYName.lai4, PYName.lai4, PYName.lan5, PYName.lan5, PYName.lan5, PYName.lan5, PYName.lang4, PYName.lang4, PYName.lang4, PYName.lang4,
            PYName.lao4, PYName.lao4, PYName.lao4, PYName.lao4, PYName.le5, PYName.le5, PYName.le5, PYName.lei5, PYName.lei5, PYName.lei5, PYName.lei5, PYName.lei5, PYName.leng4,
            PYName.leng4, PYName.leng4, PYName.leng4, PYName.li5, PYName.li5, PYName.li5, PYName.li5, PYName.li5, PYName.lia3, PYName.lian4, PYName.lian4, PYName.lian4,
            PYName.lian4, PYName.liang5, PYName.liang5, PYName.liang5, PYName.liang5, PYName.liao4, PYName.liao4, PYName.liao4, PYName.liao4, PYName.lie5, PYName.lie5,
            PYName.lie5, PYName.lie5, PYName.lie5, PYName.lin4, PYName.lin4, PYName.lin4, PYName.lin4, PYName.ling4, PYName.ling4, PYName.ling4, PYName.ling4, PYName.liu4,
            PYName.liu4, PYName.liu4, PYName.liu4, PYName.lo5, PYName.long4, PYName.long4, PYName.long4, PYName.long4, PYName.lou5, PYName.lou5, PYName.lou5, PYName.lou5,
            PYName.lou5, PYName.lu5, PYName.lu5, PYName.lu5, PYName.lu5, PYName.lu5, PYName.luan4, PYName.luan4, PYName.luan4, PYName.lun4, PYName.lun4, PYName.lun4, PYName.lun4,
            PYName.luo5, PYName.luo5, PYName.luo5, PYName.luo5, PYName.luo5, PYName.lv4, PYName.lv4, PYName.lv4, PYName.lve4, PYName.lve4, PYName.ma5, PYName.ma5, PYName.ma5,
            PYName.ma5, PYName.ma5, PYName.mai4, PYName.mai4, PYName.mai4, PYName.man4, PYName.man4, PYName.man4, PYName.man4, PYName.mang3, PYName.mang3, PYName.mang3,
            PYName.mao4, PYName.mao4, PYName.mao4, PYName.mao4, PYName.me5, PYName.me5, PYName.me5, PYName.mei4, PYName.mei4, PYName.mei4, PYName.men5, PYName.men5, PYName.men5,
            PYName.men5, PYName.men5, PYName.meng4, PYName.meng4, PYName.meng4, PYName.meng4, PYName.mi4, PYName.mi4, PYName.mi4, PYName.mi4, PYName.mian4, PYName.mian4,
            PYName.mian4, PYName.miao4, PYName.miao4, PYName.miao4, PYName.miao4, PYName.mie4, PYName.mie4, PYName.min3, PYName.min3, PYName.ming4, PYName.ming4, PYName.ming4,
            PYName.miu4, PYName.mo5, PYName.mo5, PYName.mo5, PYName.mo5, PYName.mo5, PYName.mou4, PYName.mou4, PYName.mou4, PYName.mou4, PYName.mu4, PYName.mu4, PYName.mu4,
            PYName.na5, PYName.na5, PYName.na5, PYName.na5, PYName.na5, PYName.nai4, PYName.nai4, PYName.nai4, PYName.nan4, PYName.nan4, PYName.nan4, PYName.nan4, PYName.nang4,
            PYName.nang4, PYName.nang4, PYName.nang4, PYName.nao4, PYName.nao4, PYName.nao4, PYName.nao4, PYName.ne5, PYName.ne5, PYName.ne5, PYName.nei4, PYName.nei4, PYName.nen4,
            PYName.nen4, PYName.nen4, PYName.neng3, PYName.neng3, PYName.ni4, PYName.ni4, PYName.ni4, PYName.ni4, PYName.nian4, PYName.nian4, PYName.nian4, PYName.nian4,
            PYName.niang4, PYName.niang4, PYName.niao4, PYName.niao4, PYName.nie4, PYName.nie4, PYName.nie4, PYName.nie4, PYName.nin3, PYName.nin3, PYName.ning4, PYName.ning4,
            PYName.ning4, PYName.niu4, PYName.niu4, PYName.niu4, PYName.niu4, PYName.nong4, PYName.nong4, PYName.nong4, PYName.nou4, PYName.nou4, PYName.nu4, PYName.nu4, PYName.nu4,
            PYName.nuan4, PYName.nuan4, PYName.nuan4, PYName.nun4, PYName.nun4, PYName.nuo4, PYName.nuo4, PYName.nuo4, PYName.nv4, PYName.nv4, PYName.nve4, PYName.o5, PYName.o5, PYName.o5,
            PYName.o5, PYName.o5, PYName.ou5, PYName.ou5, PYName.ou5, PYName.ou5, PYName.ou5, PYName.pa4, PYName.pa4, PYName.pa4, PYName.pai4, PYName.pai4, PYName.pai4, PYName.pai4, PYName.pan4,
            PYName.pan4, PYName.pan4, PYName.pan4, PYName.pang5, PYName.pang5, PYName.pang5, PYName.pang5, PYName.pang5, PYName.pao4, PYName.pao4, PYName.pao4, PYName.pao4, PYName.pei4, PYName.pei4,
            PYName.pei4, PYName.pei4, PYName.pen5, PYName.pen5, PYName.pen5, PYName.pen5, PYName.pen5, PYName.peng4, PYName.peng4, PYName.peng4, PYName.peng4, PYName.pi5, PYName.pi5, PYName.pi5,
            PYName.pi5, PYName.pi5, PYName.pian4, PYName.pian4, PYName.pian4, PYName.pian4, PYName.piao4, PYName.piao4, PYName.piao4, PYName.piao4, PYName.pie4, PYName.pie4, PYName.pie4,
            PYName.pin4, PYName.pin4, PYName.pin4, PYName.pin4, PYName.ping4, PYName.ping4, PYName.ping4, PYName.ping4, PYName.po5, PYName.po5, PYName.po5, PYName.po5, PYName.po5, PYName.pou4,
            PYName.pou4, PYName.pou4, PYName.pou4, PYName.pu4, PYName.pu4, PYName.pu4, PYName.pu4, PYName.qi5, PYName.qi5, PYName.qi5, PYName.qi5, PYName.qi5, PYName.qia4, PYName.qia4,
            PYName.qia4, PYName.qian5, PYName.qian5, PYName.qian5, PYName.qian5, PYName.qian5, PYName.qiang4, PYName.qiang4, PYName.qiang4, PYName.qiang4, PYName.qiao4, PYName.qiao4, PYName.qiao4,
            PYName.qiao4, PYName.qie4, PYName.qie4, PYName.qie4, PYName.qie4, PYName.qin4, PYName.qin4, PYName.qin4, PYName.qin4, PYName.qing4, PYName.qing4, PYName.qing4, PYName.qing4,
            PYName.qiong3, PYName.qiong3, PYName.qiong3, PYName.qiu4, PYName.qiu4, PYName.qiu4, PYName.qiu4, PYName.qu4, PYName.qu4, PYName.qu4, PYName.qu4, PYName.quan4, PYName.quan4,
            PYName.quan4, PYName.quan4, PYName.que4, PYName.que4, PYName.que4, PYName.qun3, PYName.qun3, PYName.qun3, PYName.ran3, PYName.ran3, PYName.rang4, PYName.rang4, PYName.rang4,
            PYName.rang4, PYName.rao4, PYName.rao4, PYName.rao4, PYName.re4, PYName.re4, PYName.re4, PYName.ren4, PYName.ren4, PYName.ren4, PYName.reng4, PYName.reng4, PYName.reng4, PYName.ri4,
            PYName.rong4, PYName.rong4, PYName.rong4, PYName.rou4, PYName.rou4, PYName.rou4, PYName.ru4, PYName.ru4, PYName.ru4, PYName.ru4, PYName.ruan4, PYName.ruan4, PYName.ruan4, PYName.rui4,
            PYName.rui4, PYName.rui4, PYName.run4, PYName.run4, PYName.ruo4,
            PYName.ruo4, PYName.sa4, PYName.sa4, PYName.sa4, PYName.sai5, PYName.sai5, PYName.sai5, PYName.sai5, PYName.san5, PYName.san5, PYName.san5, PYName.san5, PYName.sang5, PYName.sang5,
            PYName.sang5, PYName.sang5, PYName.sao4, PYName.sao4, PYName.sao4, PYName.se4, PYName.se4, PYName.sen3, PYName.sen3, PYName.seng1, PYName.sha4, PYName.sha4, PYName.sha4, PYName.sha4,
            PYName.shai4, PYName.shai4, PYName.shai4, PYName.shan4, PYName.shan4, PYName.shan4, PYName.shan4, PYName.shang5, PYName.shang5, PYName.shang5, PYName.shang5, PYName.shang5, PYName.shao4,
            PYName.shao4, PYName.shao4, PYName.shao4, PYName.she4, PYName.she4, PYName.she4, PYName.she4, PYName.shei2, PYName.shen4, PYName.shen4, PYName.shen4, PYName.shen4,
            PYName.sheng4, PYName.sheng4, PYName.sheng4, PYName.sheng4, PYName.shi5, PYName.shi5, PYName.shi5, PYName.shi5, PYName.shi5, PYName.shou4, PYName.shou4, PYName.shou4,
            PYName.shou4, PYName.shu4, PYName.shu4, PYName.shu4, PYName.shu4, PYName.shua4, PYName.shua4, PYName.shua4, PYName.shuai4, PYName.shuai4, PYName.shuai4, PYName.shuan4, PYName.shuan4,
            PYName.shuang4, PYName.shuang4, PYName.shuang4, PYName.shui4, PYName.shui4, PYName.shui4, PYName.shun4, PYName.shun4, PYName.shun4, PYName.shuo4, PYName.shuo4, PYName.shuo4,
            PYName.si4, PYName.si4, PYName.si4, PYName.song4, PYName.song4, PYName.song4, PYName.sou4, PYName.sou4, PYName.sou4, PYName.su4, PYName.su4, PYName.su4, PYName.suan4,
            PYName.suan4, PYName.suan4, PYName.sui4, PYName.sui4, PYName.sui4, PYName.sui4, PYName.sun4, PYName.sun4, PYName.sun4, PYName.suo4, PYName.suo4, PYName.suo4, PYName.ta5,
            PYName.ta5, PYName.ta5, PYName.ta5, PYName.tai4, PYName.tai4, PYName.tai4, PYName.tai4, PYName.tan4, PYName.tan4, PYName.tan4, PYName.tan4, PYName.tang4, PYName.tang4, PYName.tang4,
            PYName.tang4, PYName.tao4, PYName.tao4, PYName.tao4, PYName.tao4, PYName.te4, PYName.teng4, PYName.teng4, PYName.teng4, PYName.ti4, PYName.ti4, PYName.ti4, PYName.ti4, PYName.tian5,
            PYName.tian5, PYName.tian5, PYName.tian5, PYName.tian5, PYName.tiao4, PYName.tiao4, PYName.tiao4, PYName.tiao4, PYName.tie4, PYName.tie4, PYName.tie4, PYName.tie4, PYName.ting4,
            PYName.ting4, PYName.ting4, PYName.ting4, PYName.tong4, PYName.tong4, PYName.tong4, PYName.tong4, PYName.tou5, PYName.tou5, PYName.tou5, PYName.tou5, PYName.tou5, PYName.tu5,
            PYName.tu5, PYName.tu5, PYName.tu5, PYName.tu5, PYName.tuan4, PYName.tuan4, PYName.tuan4, PYName.tuan4, PYName.tui4, PYName.tui4, PYName.tui4, PYName.tui4, PYName.tun5, PYName.tun5,
            PYName.tun5, PYName.tun5, PYName.tun5, PYName.tuo4, PYName.tuo4, PYName.tuo4, PYName.tuo4, PYName.wa5, PYName.wa5, PYName.wa5, PYName.wa5, PYName.wa5, PYName.wai4, PYName.wai4,
            PYName.wai4, PYName.wan4, PYName.wan4, PYName.wan4, PYName.wan4, PYName.wang4, PYName.wang4, PYName.wang4, PYName.wang4, PYName.wei4, PYName.wei4, PYName.wei4, PYName.wei4,
            PYName.wen4, PYName.wen4, PYName.wen4, PYName.wen4, PYName.weng4, PYName.weng4, PYName.weng4, PYName.wo4, PYName.wo4, PYName.wo4, PYName.wu4, PYName.wu4, PYName.wu4, PYName.wu4,
            PYName.xi4, PYName.xi4, PYName.xi4, PYName.xi4, PYName.xia4, PYName.xia4, PYName.xia4, PYName.xia4, PYName.xian4, PYName.xian4, PYName.xian4, PYName.xian4, PYName.xiang4,
            PYName.xiang4, PYName.xiang4, PYName.xiang4, PYName.xiao4, PYName.xiao4, PYName.xiao4, PYName.xiao4, PYName.xie4, PYName.xie4, PYName.xie4, PYName.xie4, PYName.xin4, PYName.xin4,
            PYName.xin4, PYName.xin4, PYName.xing4, PYName.xing4, PYName.xing4, PYName.xing4, PYName.xiong4, PYName.xiong4, PYName.xiong4, PYName.xiong4, PYName.xiu4, PYName.xiu4, PYName.xiu4,
            PYName.xiu4, PYName.xu5, PYName.xu5, PYName.xu5, PYName.xu5, PYName.xu5, PYName.xuan4, PYName.xuan4, PYName.xuan4, PYName.xuan4, PYName.xue4, PYName.xue4, PYName.xue4, PYName.xue4,
            PYName.xun4, PYName.xun4, PYName.xun4, PYName.ya5, PYName.ya5, PYName.ya5, PYName.ya5, PYName.ya5, PYName.yai2, PYName.yan4, PYName.yan4, PYName.yan4, PYName.yan4, PYName.yang4,
            PYName.yang4, PYName.yang4, PYName.yang4, PYName.yao4, PYName.yao4, PYName.yao4, PYName.yao4, PYName.ye5, PYName.ye5, PYName.ye5, PYName.ye5, PYName.ye5, PYName.yi5, PYName.yi5,
            PYName.yi5, PYName.yi5, PYName.yi5, PYName.yin4, PYName.yin4, PYName.yin4, PYName.yin4, PYName.ying4, PYName.ying4, PYName.ying4, PYName.ying4, PYName.yo5, PYName.yo5,
            PYName.yong4, PYName.yong4, PYName.yong4, PYName.yong4, PYName.you4, PYName.you4, PYName.you4, PYName.you4, PYName.yu4, PYName.yu4, PYName.yu4, PYName.yu4, PYName.yuan4,
            PYName.yuan4, PYName.yuan4, PYName.yuan4, PYName.yue4, PYName.yue4, PYName.yue4, PYName.yun4, PYName.yun4, PYName.yun4, PYName.yun4, PYName.za3, PYName.za3, PYName.za3,
            PYName.zai4, PYName.zai4, PYName.zai4, PYName.zan4, PYName.zan4, PYName.zan4, PYName.zan4, PYName.zang4, PYName.zang4, PYName.zang4, PYName.zang4, PYName.zao4, PYName.zao4,
            PYName.zao4, PYName.zao4, PYName.ze4, PYName.ze4, PYName.zei2, PYName.zen4, PYName.zen4, PYName.zen4, PYName.zeng4, PYName.zeng4, PYName.zha4, PYName.zha4, PYName.zha4,
            PYName.zha4, PYName.zhai4, PYName.zhai4, PYName.zhai4, PYName.zhai4, PYName.zhan4, PYName.zhan4, PYName.zhan4, PYName.zhan4, PYName.zhang4, PYName.zhang4, PYName.zhang4,
            PYName.zhao4, PYName.zhao4, PYName.zhao4, PYName.zhao4, PYName.zhe5, PYName.zhe5, PYName.zhe5, PYName.zhe5, PYName.zhe5, PYName.zhei4, PYName.zhen4, PYName.zhen4,
            PYName.zhen4, PYName.zheng4, PYName.zheng4,
            PYName.zheng4, PYName.zhi4, PYName.zhi4, PYName.zhi4, PYName.zhi4, PYName.zhong4, PYName.zhong4, PYName.zhong4, PYName.zhou4, PYName.zhou4, PYName.zhou4, PYName.zhou4,
            PYName.zhu4, PYName.zhu4, PYName.zhu4, PYName.zhu4, PYName.zhua3, PYName.zhua3, PYName.zhuai4, PYName.zhuai4, PYName.zhuai4, PYName.zhuan4, PYName.zhuan4, PYName.zhuan4,
            PYName.zhuang4, PYName.zhuang4, PYName.zhuang4, PYName.zhui4, PYName.zhui4, PYName.zhui4, PYName.zhun4, PYName.zhun4, PYName.zhun4, PYName.zhuo4, PYName.zhuo4, PYName.zhuo4,
            PYName.zhuo4, PYName.zi5, PYName.zi5, PYName.zi5, PYName.zi5, PYName.zi5, PYName.zong4, PYName.zong4, PYName.zong4, PYName.zou4, PYName.zou4, PYName.zou4, PYName.zu4,
            PYName.zu4, PYName.zu4, PYName.zu4, PYName.zuan4, PYName.zuan4, PYName.zuan4, PYName.zui4, PYName.zui4, PYName.zui4, PYName.zun4, PYName.zun4, PYName.zun4, PYName.zuo5,
            PYName.zuo5, PYName.zuo5, PYName.zuo5, PYName.zuo5, PYName.none5 };

        static String2Pinyin()
        {
            _trie = new Trie() { RemainLongest = true };    // todo: 可以改成双数组
            _map = new SortedDictionary<string, Pinyin>(StrComparer.Default);
            int end = (int)PYName.none5;

            for(int i = 0; i < end; i++)
            {
                var py = Pinyin.PinyinTable[i];
                var pyWithoutTone = py.Pinyin_;
                var fstChar = py.FirstChar.ToString();

                _trie.AddKeyword(pyWithoutTone);
                _trie.AddKeyword(fstChar);
                _map[pyWithoutTone] = py;
                _map[fstChar] = py;
                _map[py.ToString()] = py;
            }
        }

        public static Pinyin Convert2Tone5(Pinyin p)
        {
            var index = (int)Enum.Parse(typeof(PYName), p.ToString());
            var tone5 = Tone2tone5[index];
            return Pinyin.PinyinTable[(int)tone5];
        }

        public static List<Pinyin> Convert(string text, bool removeTone = false)
        {
            var list = new List<Pinyin>();
            var tokens = _trie.Tokenize(text);

            foreach(var t in tokens)
            {
                var frag = t.Fragment;
                if (t.IsMatch)
                {
                    var py = ConvertSingle(frag);
                    if (removeTone)
                        py = Convert2Tone5(py);
                    list.Add(py);
                }
                else
                {
                    var pys = PinyinDictionary.Translate2Pinyin(frag);
                    if (removeTone)
                        for (int i = 0; i < pys.Count; i++)
                            pys[i] = Convert2Tone5(pys[i]);
                    list.AddRange(pys);
                }
            }
            return list;
        }

        public static Tuple<List<Pinyin>, List<bool>> Convert2Tuple(string text, bool removeTone)
        {
            var pinyins = new List<Pinyin>();
            var bools = new List<bool>();

            var tokens = _trie.Tokenize(text);
            foreach(var t in tokens)
            {
                var frag = t.Fragment;
                if(t.IsMatch)
                {
                    var py = ConvertSingle(frag);
                    if (removeTone)
                        py = Convert2Tone5(py);
                    pinyins.Add(py);
                    bools.Add(frag.Length == py.Pinyin_.Length);    // true表示是一个拼音，false表示是一个输入法头
                }
                else
                {
                    var pys = PinyinDictionary.Translate2Pinyin(frag);

                    pinyins.AddRange(pys);
                    for(int i = 0; i < pys.Count; i++)
                    {
                        bools.Add(true);            // 表示这是一个拼音
                        if(removeTone)
                            pys[i] = Convert2Tone5(pys[i]);
                    }
                }
            }
            return new Tuple<List<Pinyin>, List<bool>>(pinyins, bools);
        }

        public static Pinyin ConvertSingle(string str)
        {
            if (_map.TryGetValue(str, out var py)) return py;

            return Pinyin.None;
        }

        public static List<Pinyin> ChangeToneToTheSame(List<Pinyin> pinyins)
        {
            var list = new List<Pinyin>();
            foreach(var py in pinyins)
            {
                list.Add(Convert2Tone5(py));
            }
            return list;
        }

    }
}
