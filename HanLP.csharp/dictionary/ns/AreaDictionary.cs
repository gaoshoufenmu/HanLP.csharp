using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using HanLP.csharp.dictionary.common;
using HanLP.csharp.collection.trie;
using HanLP.csharp.collection;

namespace HanLP.csharp.dictionary.ns
{
    /// <summary>
    /// （中国）行政地区名字典
    /// </summary>
    public class AreaDictionary
    {
        private static DoubleArrayTrie<AreaInfo> _trie;

        private static HashSet<string> Invalids = new HashSet<string>
        {
            "市辖区",
            "城区",
            "郊区",
            "矿区",
            "地区",
            "特区",
            "直辖",
            "保税区",
            "省直辖县级行政区划"
        };

        private static HashSet<string> Nationalities = new HashSet<string>
        {
            "哈萨克",
            "锡伯",
            "蒙古",
            "回族",
            "柯尔克孜",
            "塔吉克",
            "达斡尔",
            "畲族",
            "满族",
            "鄂温克",
            "鄂伦春",
            "朝鲜族",
            "土家",
            "苗族",
            "瑶族",
            "壮族",
            "侗族",
            "布依族"
        };

        static AreaDictionary()
        {
            Load(Config.Area_Dict_Path);
        }

        public static bool Load(string path)
        {
            try
            {
                _trie = new DoubleArrayTrie<AreaInfo>();
                var valueArr = LoadDat(path + ".value.dat");
                if (valueArr != null)
                {
                    if (_trie.Load(path + ".trie.dat", valueArr))
                        return true;
                }
                // 读取txt文件
                var map = new SortedDictionary<string, AreaInfo>(StrComparer.Default);
                foreach (var line in File.ReadLines(path))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var segs = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    var code = segs[0];
                    for (int i = 1; i < segs.Length; i++)
                    {
                        var name = segs[i];
                        if (Invalids.Contains(name)) continue;   // 跳过无效地区名
                        if (name.Length == 2)
                        {
                            AddInMap(name, "", code, map);
                        }
                        else
                        {
                            var lastChar = name[name.Length - 1];
                            if ("市省县区州旗盟".Contains(lastChar))
                            {
                                AddInMap(name.Substring(0, name.Length - 1), lastChar.ToString(), code, map);
                            }
                            else if (name.Length < 9)
                            {
                                AddInMap(name, "", code, map);
                            }
                            var lastTwo = name.Substring(2);
                            var prevs = name.Substring(0, name.Length - 2);
                            if (Invalids.Contains(lastTwo))
                            {
                                AddInMap(prevs, lastTwo, code, map);
                                if (prevs.Length == 3 && "市省".Contains(prevs[2]))
                                {
                                    AddInMap(name.Substring(0, 2), lastTwo, code, map);
                                }
                            }
                            if (lastChar == '旗')
                            {
                                var sublast = name[2];
                                if ("前后左中右特".Contains(sublast))
                                {
                                    AddInMap(prevs, "旗", code, map);
                                }
                            }
                            var subLastTwo = name.Substring(name.Length - 3, 2);
                            if (subLastTwo == "自治")
                            {
                                prevs = name.Substring(0, name.Length - 3);
                                var ends = name.Substring(name.Length - 3);
                                AddInMap(prevs, ends, code, map);
                                if (prevs.Length >= 4)
                                {
                                    for (int k = 2; k < prevs.Length - 1; k++)
                                    {
                                        if (k < prevs.Length - 3)
                                        {
                                            if (Nationalities.Contains(prevs.Substring(k, 4)))
                                            {
                                                AddInMap(prevs.Substring(0, k), ends, code, map);
                                                AddInMap(prevs.Substring(0, k) + "自治", lastChar.ToString(), code, map);
                                                break;
                                            }
                                        }
                                        if (k < prevs.Length - 2)
                                        {
                                            if (Nationalities.Contains(prevs.Substring(k, 3)))
                                            {
                                                AddInMap(prevs.Substring(0, k), ends, code, map);
                                                AddInMap(prevs.Substring(0, k) + "自治", lastChar.ToString(), code, map);
                                                break;
                                            }
                                        }
                                        if (Nationalities.Contains(prevs.Substring(k, 2)))
                                        {
                                            AddInMap(prevs.Substring(0, k), ends, code, map);
                                            AddInMap(prevs.Substring(0, k) + "自治", lastChar.ToString(), code, map);
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                _trie = new DoubleArrayTrie<AreaInfo>();
                _trie.Build(map);
                valueArr = new AreaInfo[map.Count];
                int m = 0;
                foreach (var v in map.Values)
                    valueArr[m++] = v;

                var fs = new FileStream(path + ".trie.dat", FileMode.Create, FileAccess.Write);
                _trie.Save(fs);
                fs.Close();
                SaveDat(path + ".value.dat", valueArr);
                return true;
            }
            catch(Exception e)
            {
                return false;
            }
        }

        private static void AddInMap(string prefix, string tail, string code, SortedDictionary<string, AreaInfo> map)
        {
            AreaInfo ai;
            if(map.TryGetValue(prefix, out ai))
            {
                ai.AddTailCode(tail, code);
            }
            else
            {
                ai = new AreaInfo();
                ai.AddTailCode(tail, code);
                map[prefix] = ai;
            }
        }
        

        private static AreaInfo[] LoadDat(string path)
        {
            if (!File.Exists(path)) return null;
            var bytes = File.ReadAllBytes(path);

            int index = 0;
            int size = BitConverter.ToInt32(bytes, index);
            index += 4;

            var valueArr = new AreaInfo[size];
            for (int i = 0; i < size; i++)
            {
                var areainfo = new AreaInfo();
                var span = areainfo.Deserialize(bytes, index);
                index += span;

                valueArr[i] = areainfo;
            }
            return valueArr;
        }


        private static bool SaveDat(string path, AreaInfo[] valueArr)
        {
            var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            try
            {
                var bytes = BitConverter.GetBytes(valueArr.Length);
                fs.Write(bytes, 0, 4);
                for (int i = 0; i < valueArr.Length; i++)
                {
                    valueArr[i].Serialize(fs);
                }
                return true;
            }
            catch(Exception e)
            {
                return false;
            }
            finally
            {
                fs.Close();
            }
        }


        public static AreaResult Search(string area)
        {
            if (string.IsNullOrWhiteSpace(area)) return null;

            string key = area;
            var lastChar = area[area.Length - 1];

            if (area.Length > 2 && "市省县区州旗盟".Contains(lastChar))
            {
                key = area.Substring(0, area.Length - 1);
                var ai = _trie.GetOrDefault(key);
                if (ai == null)
                {
                    return null;
                }

                var tuple = ai.Match(area.Substring(area.Length - 1));
                var ar = new AreaResult();
                if (tuple != null)
                {
                    ar.AccurateCodes = tuple.Item2;
                    return ar;
                }

                var newMap = new Dictionary<string, List<string>>();
                foreach(var p in ai.MatchAll())
                {
                    var k = key + p.Key;
                    newMap[k] = p.Value;
                }
                ar.Name_Codes_Map = newMap;
                return ar;
            }

            var ai1 = _trie.GetOrDefault(area);
            if (ai1 == null)
            {
                return null;
            }

            var newMap1 = new Dictionary<string, List<string>>();
            foreach (var p in ai1.MatchAll())
            {
                var k = key + p.Key;
                newMap1[k] = p.Value;
            }
            return new AreaResult() { Name_Codes_Map = newMap1 };
        }
    }

    public class AreaInfo
    {
        /// <summary>
        /// 行政地区级别字与对应的行政地区码
        /// </summary>
        public Dictionary<string, HashSet<string>> Tail_Codes_Map;

        public void Serialize(FileStream fs)
        {
            byte[] bs;
            fs.WriteByte((byte)Tail_Codes_Map.Count);
            foreach(var p in Tail_Codes_Map)
            {
                fs.WriteByte((byte)p.Key.Length);
                foreach(var c in p.Key)
                {
                    bs = BitConverter.GetBytes(c);
                    fs.Write(bs, 0, 2);
                }
                fs.WriteByte((byte)p.Value.Count);
                foreach(var code in p.Value)
                {
                    fs.WriteByte((byte)code.Length);
                    foreach(var c in code)
                    {
                        bs = BitConverter.GetBytes(c);
                        fs.Write(bs, 0, 2);
                    }
                }
            }
        }

        public int Deserialize(byte[] bs, int offset)
        {
            var start = offset;
            var count = bs[offset++];
            Tail_Codes_Map = new Dictionary<string, HashSet<string>>();
            for(int i = 0; i < count; i++)
            {
                var keyLength = bs[offset++];
                var sb = new StringBuilder(keyLength);
                for (int j = 0; j < keyLength; j++)
                {
                    sb.Append(BitConverter.ToChar(bs, offset));
                    offset += 2;
                }
                var key = sb.ToString();
                var valLength = bs[offset++];
                var set = new HashSet<string>();
                for(int j = 0; j < valLength; j++)
                {
                    sb.Clear();
                    keyLength = bs[offset++];
                    for (int k = 0; k < keyLength; k++)
                    {
                        sb.Append(BitConverter.ToChar(bs, offset));
                        offset += 2;
                    }
                    set.Add(sb.ToString());
                }
                Tail_Codes_Map[key] = set;
            }
            return offset - start;
        }


        public void AddTailCode(string tail, string code)
        {
            if (Tail_Codes_Map == null)
                Tail_Codes_Map = new Dictionary<string, HashSet<string>>();
            HashSet<string> codes;
            if(!Tail_Codes_Map.TryGetValue(tail, out codes))
            {
                codes = new HashSet<string>();
                Tail_Codes_Map[tail] = codes;
            }

            codes.Add(code);
        }

        /// <summary>
        /// 匹配所有地区码，不指定地区级别尾字
        /// </summary>
        /// <param name="tail"></param>
        /// <param name="min_code_len">地区码最小长度</param>
        /// <returns></returns>
        public Dictionary<string, List<string>> MatchAll(int min_code_len = 2)
        {
            if (Tail_Codes_Map == null) return null;

            var map = new Dictionary<string, List<string>>();
            foreach (var p in Tail_Codes_Map)
            {
                var list = new List<string>();

                foreach (var code in p.Value)
                {
                    if (code.Length >= min_code_len)
                        list.Add(code);
                }
                if (list.Count > 0)
                    map.Add(p.Key, list);
            }
            return map;
        }

        /// <summary>
        /// 根据尾字匹配地区码
        /// </summary>
        /// <param name="tail"></param>
        /// <param name="min_code_len">地区码最小长度</param>
        /// <returns>Item1: 匹配成功的尾字符串，对应的地区码列表</returns>
        public Tuple<string, List<string>> Match(string tail, int min_code_len = 2)
        {
            if (Tail_Codes_Map == null) return null;

            HashSet<string> set;
            if(Tail_Codes_Map.TryGetValue(tail, out set))
            {
                return new Tuple<string, List<string>>(tail, 
                    set.Where(code => code.Length >= min_code_len)
                        .Select(code => code).ToList());
            }
            string str = tail;
            // 没有匹配到，那么----
            // 尝试使用级别别称匹配
            if (tail == "县" || tail == "市" || tail == "区")
            {
                if (tail == "区" || tail == "市")
                    str = "县";
                else
                    str = "市";
                if(Tail_Codes_Map.TryGetValue(str, out set))
                {
                    return new Tuple<string, List<string>>(str,
                        set.Where(code => code.Length >= 6)         // 县为6位字符长度
                            .Select(code => code).ToList());
                }
                else if (tail == "县")
                {
                    if (Tail_Codes_Map.TryGetValue("区", out set))
                    {
                        return new Tuple<string, List<string>>(str,
                            set.Where(code => code.Length >= 6)         // 县为6位字符长度
                                .Select(code => code).ToList());
                    }
                }
                return null;
            }
            if(tail.StartsWith("自治"))
            {
                foreach(var p in Tail_Codes_Map)
                {
                    if(p.Key.EndsWith(tail))
                    {
                        return new Tuple<string, List<string>>(p.Key,
                            p.Value.Where(code => code.Length >= min_code_len)
                                .Select(code => code).ToList());
                    }
                }
                return null;
            }
            else
            {
                foreach (var p in Tail_Codes_Map)
                {
                    if (p.Key.StartsWith(tail))
                    {
                        return new Tuple<string, List<string>>(p.Key,
                            p.Value.Where(code => code.Length >= min_code_len)
                                .Select(code => code).ToList());
                    }
                }
                return null;
            }
        }

    }

    public class AreaResult
    {
        public List<string> AccurateCodes;
        /// <summary>
        /// Key，地区名全称
        /// </summary>
        public Dictionary<string, List<string>> Name_Codes_Map;
    }
}
