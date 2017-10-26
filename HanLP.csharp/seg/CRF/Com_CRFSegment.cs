using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using HanLP.csharp.seg.common;
using HanLP.csharp.dictionary.ns;
using HanLP.csharp.corpus.tag;
using HanLP.csharp.utility;
using HanLP.csharp.dictionary;

namespace HanLP.csharp.seg.CRF
{
    /// <summary>
    /// 对公司名采用CRF分词
    /// </summary>
    public class Com_CRFSegment
    {
        private static Segment _segment = new CRFSegment().SetCustomDictionary(true);

        private static Dictionary<string, Dictionary<string, int>> AreaCas_Map = new Dictionary<string, Dictionary<string, int>>();

        public static List<ComTerm> Segment(string name, string address = null, string area = null, string areaCode = null)
        {
            var areacode = areaCode;                        // 传参地区码
            if (string.IsNullOrWhiteSpace(areacode))
            {
                List<string> ah = null;
                if(area != null)
                {
                    var ar = AreaDictionary.Search(area);
                    if(ar != null)
                    {
                        ah = ar.AccurateCodes;
                    }
                }
                if (ah == null || ah.Count > 1)
                {
                    // 分析 address 获取地区码
                    string fstAreaCode = null;
                    if (address != null)
                    {
                        var addTerms = _segment.Seg(address);

                        foreach (var t in addTerms)
                        {
                            var tar = AreaDictionary.Search(t.word);
                            if (tar != null)
                            {
                                fstAreaCode = tar.AccurateCodes[0];
                                break;
                            }
                        }
                    }
                    if (fstAreaCode != null)
                        areacode = fstAreaCode;
                    else
                        areacode = "";
                }
                else
                    areacode = ah[0];
            }

            var terms = _segment.Seg(name);
            var list = new List<ComTerm>();           //      <-------------后--------          --------------前--------------->
            var index = -1;
            //ComTerm lastAreaTerm = null;

            var count = terms.Count;
            for(int i = 0; i < count; i++)
            {
                var term = terms[i];
                var ct = term.Copy2ComTerm();
                if(term.nature == Nature.w || !TextUtil.IsAllChinese(term.word))    // 英文，字符判断
                {
                    ct.nc = term.nature == Nature.w ? NatCom.W : NatCom.E;
                    ct.Verified = true;
                    list.Add(ct);
                    index++;
                    continue;
                }
                if (term.word == "公司" || term.word == "责任" || term.word == "集团" || term.word == "股份" || term.word == "有限")
                {
                    ct.nc = NatCom.OF;
                    ct.Verified = true;
                    list.Add(ct);
                    index++;
                    continue;
                }
                if (term.word == "中国")
                {
                    ct.nc = NatCom.MA;
                    //lastAreaTerm = ct;
                    list.Add(ct);
                    index++;
                    continue;
                }

                if (term.word.Length == 1)   // 遇到单字词条
                {
                    // 向前结合所有单字
                    int n = i + 1;
                    while (n < count)
                    {
                        var next_t = terms[n];
                        if (next_t.word.Length == 1)     // 只要是单字，就直接合并
                        {
                            ct.word += next_t.word;
                        }
                        else
                        {
                            break;                      // 退出，当前位置n处的词条长度大于1
                        }
                        n++;
                    }
                    i = n - 1;                          // 设置下一个词条位置
                    // 此时分几种情况
                    if (n == count)  // 如果n == count，那么表示最后都是单字，合并后作为一个词，并且分词结束
                    {
                        ct.nc = NatCom.U;
                        list.Add(ct);
                        return list;
                    }
                    else    // 分词尚未结束，n处表示下一个长度大于1的词条
                    {
                        if(ct.word.Length == 1)     // 如果词条还是长度为1
                        {
                            int combine = 0;        // 0 -》 自成一个词条, 1-> 向前结合，2 -》 向后结合
                            if (index < 0 || list[index].Verified)  // 应该向前结合，因为没法向后结合了
                            {
                                if (TextUtil.IsAllChinese(terms[n].word))    // 检测后面的词条是否是全中文，如不是，则不能与后面的词条结合
                                    combine = 1;
                            }
                            else if((index >= 0 && list[index].Freq < 10) || terms[n].nature == Nature.w || !TextUtil.IsAllChinese(terms[n].word))    // 可以向后结合
                                combine = 2;
                            else    // 前后都有词条
                            {
                                // 首先根据公司名高频词条的频率进行决策
                                if(terms[n].ComFreq >= 10000 && list[index].ComFreq >= 10000)       // 单字成一词
                                    combine = 0;
                                else if (list[index].ComFreq / terms[n].ComFreq > 10)     // 向前结合
                                    combine = 1;
                                else if (terms[n].ComFreq / list[index].ComFreq > 10)    // 向后结合
                                    combine = 2;
                                else if(terms[n].ComFreq < 5000 && list[index].ComFreq < 5000)                        // 
                                {
                                    if(terms[n].nature == Nature.nz)        // 向前结合
                                        combine = 1;
                                    else if(list[index].nature == Nature.nz)        // 向后结合
                                        combine = 2;
                                    else if(terms[n].Freq > list[index].Freq)      // 向后结合
                                        combine = 2;
                                    else                                        // 向前结合
                                        combine = 1;
                                }
                                else if(terms[n].ComFreq < 5000)                // 向前结合
                                    combine = 1;
                                else if(list[index].ComFreq < 5000)             // 向后结合
                                    combine = 2;
                            }
                            if(combine == 0)
                            {
                                list.Add(ct);
                                index++;
                            }
                            else if(combine == 1)
                            {
                                terms[n].word = ct.word + terms[n].word;       // 直接与下一个结合
                                terms[n].offset -= 1;
                            }
                            else
                            {
                                var prev_ct = list[index];
                                prev_ct.word += ct.word;
                                if (prev_ct.word.Length >= 4)
                                {
                                    var suffix = prev_ct.word.Substring(prev_ct.word.Length - 2);
                                    var attr = CoreDictionary.GetAttr(suffix);
                                    if (attr != null)
                                    {
                                        var nat = attr.natures[0];
                                        list[index].word = prev_ct.word.Substring(0, prev_ct.word.Length - 2);
                                        list[index].nc = NatCom.C;
                                        list.Add(new ComTerm(suffix, nat) { nc = NatCom.T, offset = prev_ct.word.Length - 2 });
                                        index++;
                                    }
                                    else if (prev_ct.word.Length > 4)
                                    {
                                        attr = CoreDictionary.GetAttr(prev_ct.word.Substring(prev_ct.word.Length - 3));
                                        if (attr != null)
                                        {
                                            var nat = attr.natures[0];
                                            list[index].word = prev_ct.word.Substring(0, prev_ct.word.Length - 2);
                                            list[index].nc = NatCom.C;
                                            list.Add(new ComTerm(suffix, nat) { nc = NatCom.T, offset = prev_ct.word.Length - 3 });
                                            index++;
                                        }
                                    }
                                }
                            }
                            continue;
                        }
                        else
                        {
                            ct.nc = NatCom.C;
                            list.Add(ct);
                            index++;
                        }
                    }
                }
                else
                {
                    var arearesult = AreaDictionary.Search(term.word);
                    // 没有考虑前后
                    if (arearesult == null)      // 没有找到
                    {
                        if(index >= 0 && list[index].nc == NatCom.MA && term.nature == Nature.ns)    // 如果认为是地名，则有可能是旧地名尚未收录
                        {
                            var prev_ct = list[index];
                            if(prev_ct.ext1.Length <= 4 && (term.word.EndsWith("区") || term.word.EndsWith("县")))    // 已经确定，直接认为当前词条是地区
                            {
                                ct.nc = NatCom.MA;
                                //lastAreaTerm = ct;
                                ct.ext1 = "******";
                                ct.Verified = true;
                                list.Add(ct);
                                index++;
                                continue;
                            }
                            else                                                                // 上一个词条确定是地名，则进行级联
                            {
                                Dictionary<string, int> subMap;
                                if (AreaCas_Map.TryGetValue(prev_ct.ext1, out subMap))
                                {
                                    if (subMap.ContainsKey(term.word))
                                    {
                                        subMap[term.word] += 1;
                                    }
                                    else
                                        subMap[term.word] = 1;
                                }
                                else
                                    AreaCas_Map[prev_ct.ext1] = new Dictionary<string, int>() { [term.word] = 1 };
                            }
                        }
                        if (ct.word.Length < 4)
                        {
                            ct.nc = NatCom.C;
                            list.Add(ct);
                            index++;
                            continue;
                        }
                        else if(ct.word.Length >= 4)
                        {
                            var prehalf = ct.word.Substring(0, 2);
                            var nexhalf = ct.word.Substring(2);
                            var nexattr = CoreDictionary.GetAttr(nexhalf);
                            var preattr = CoreDictionary.GetAttr(prehalf);
                            if(nexattr != null || preattr != null)
                            {
                                list.Add(new ComTerm(prehalf, ct.nature) { offset = ct.offset, nc = NatCom.C });
                                list.Add(new ComTerm(nexhalf, ct.nature) { offset = ct.offset + 2, nc = NatCom.T });
                                index++;
                            }
                            else if(ct.word.Length > 4)
                            {
                                prehalf = ct.word.Substring(0, 3);
                                nexhalf = ct.word.Substring(3);
                                nexattr = CoreDictionary.GetAttr(nexhalf);
                                preattr = CoreDictionary.GetAttr(prehalf);
                                if (nexattr != null || preattr != null)
                                {
                                    list.Add(new ComTerm(prehalf, ct.nature) { offset = ct.offset, nc = NatCom.C });
                                    list.Add(new ComTerm(nexhalf, ct.nature) { offset = ct.offset + 3, nc = NatCom.T });
                                    index++;
                                }
                                else
                                {
                                    ct.nc = NatCom.C;
                                    list.Add(ct);
                                    index++;
                                    continue;
                                }
                            }
                            else
                            {
                                ct.nc = NatCom.C;
                                list.Add(ct);
                                index++;
                                continue;
                            }
                        }
                    }
                    else        // 找到了地区名
                    {
                        if (arearesult.AccurateCodes != null)
                        {
                            ct.nc = NatCom.MA;
                            ct.Verified = true;         // 确认是地区
                            //lastAreaTerm = ct;
                            ct.ext1 = arearesult.AccurateCodes[0];
                            list.Add(ct);
                            index++;
                        }
                        else if (arearesult.Name_Codes_Map != null)        // 未指定地区级别的地区名
                        {
                            var aflag = false;
                            if (arearesult.Name_Codes_Map.Count == 1)
                            {
                                foreach (var code in arearesult.Name_Codes_Map.First().Value)
                                {
                                    // 地区一致
                                    if (code.StartsWith(areacode) || areacode.StartsWith(code))      //! 特别地，如果areacode=""，则条件成立
                                    {
                                        ct.nc = NatCom.MA;
                                        //lastAreaTerm = ct;
                                        ct.ext1 = code;
                                        if (code.Length == 3)
                                            ct.Verified = true;
                                        else
                                            ct.Verified = areacode != "";         // 确认是地区
                                        list.Add(ct);
                                        index++;
                                        aflag = true;
                                        break;
                                    }
                                }
                                if (!aflag)
                                {
                                    ct.nc = NatCom.C;           // 疑似公司字号
                                    list.Add(ct);
                                    index++;
                                }
                            }
                            else        // 有重名的地区
                            {

                                foreach (var p in arearesult.Name_Codes_Map)
                                {
                                    foreach (var code in p.Value)
                                    {
                                        // 地区一致
                                        if (code.StartsWith(areacode) || areacode.StartsWith(code))      //! 特别地，如果areacode=""，则条件成立
                                        {
                                            ct.nc = NatCom.MA;
                                            //lastAreaTerm = ct;
                                            ct.ext1 = code;
                                            if (code.Length == 3)
                                                ct.Verified = true;
                                            else
                                                ct.Verified = areacode != "";         // 确认是地区
                                            list.Add(ct);
                                            index++;
                                            aflag = true;
                                            break;
                                        }
                                    }
                                    if (aflag)
                                        break;
                                }
                                if (!aflag)
                                {
                                    ct.nc = NatCom.C;           // 疑似公司字号
                                    list.Add(ct);
                                    index++;
                                }
                            }
                        }
                    }
                }
                
            }
            return list;
        }


        public static void SaveOldAreaNames()
        {
            var list = new List<string>();
            var sb = new StringBuilder(100);
            foreach(var p in AreaCas_Map)
            {
                sb.Append(p.Key).Append(' ');
                foreach(var pp in p.Value)
                {
                    sb.Append(pp.Key).Append(' ').Append(pp.Value).Append(' ');
                }
                list.Add(sb.ToString());
                sb.Clear();
            }
            if(list.Count > 0)
            {
                File.WriteAllLines(AppDomain.CurrentDomain.BaseDirectory + "/create/OldAreaNames.txt", list.ToArray());
            }
        }
    }
}
