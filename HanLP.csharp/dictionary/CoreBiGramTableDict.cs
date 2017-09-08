using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using HanLP.csharp.utility;
using HanLP.csharp.corpus.io;

namespace HanLP.csharp.dictionary
{
    public class CoreBiGramTableDict
    {
        private static int[] _start;

        private static int[] _pair;

        private static string _datPath = Config.BiGram_Dict_Path + ".table" + Predefine.BIN_EXT;

        static CoreBiGramTableDict()
        {
            if (!Load())
                throw new Exception("BiGram dictionary file loading error");
        }

        private static bool Load()
        {
            if (LoadDat()) return true;

            var dict = new SortedDictionary<int, SortedDictionary<int, int>>();
            int total = 0;
            int maxWordId = CoreDictionary._trie.Size;
            try
            {
                foreach(var line in File.ReadLines(Config.BiGram_Dict_Path))
                {
                    var segs = line.Split(new[] { '@', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (segs.Length != 3) continue;

                    var fst = segs[0];
                    var idA = CoreDictionary._trie.ExactMatch(fst);
                    if (idA == -1) continue;                            // idA 是核心词典_v数组的下标，所以 idA < maxWordId

                    var snd = segs[1];
                    var idB = CoreDictionary._trie.ExactMatch(snd);
                    if (idB == -1) continue;                            // // idB 是核心词典_v数组的下标，所以 idA < maxWordId

                    var freq = int.Parse(segs[2]);
                    SortedDictionary<int, int> biDict;
                    if(!dict.TryGetValue(idA, out biDict))
                    {
                        biDict = new SortedDictionary<int, int>();
                        dict[idA] = biDict;
                    }
                    biDict[idB] = freq;
                    total += 2;             // total表示二元词总数的2倍
                }

                _start = new int[maxWordId + 1];
                _pair = new int[total];
                int offset = 0;

                for(int i = 0; i < maxWordId; i++)
                {
                    if(dict.TryGetValue(i, out var biDict))     // 找到_v的下标为 i 的词作为二元词的第一个部分
                    {
                        foreach(var p in biDict)
                        {
                            int index = offset << 1;
                            _pair[index] = p.Key;               // 二元词的第二个部分
                            _pair[index + 1] = p.Value;         // 二元词的频率
                            offset++;
                        }
                    }
                    _start[i + 1] = offset;     
                    // 二元词的各个第一部分按其在核心词典的_v数组的下标 i 排序存储在_start中，其中 _start[i+1]表示前面（0到i）所有二元词第一个部分出现的词数
                    // 于是 _start[i+1] - _start[i] 表示下标为 i 的二元词第一个部分（在二元词典中）出现的词数，这个二元词的第一部分就是 核心词典的 _v[i] 值
                }

                SaveDat();
                return true;
            }
            catch(Exception e)
            {
                return false;
            }
        }

        private static bool LoadDat()
        {
            try
            {
                var ba = ByteArray.Create(_datPath);
                var size = ba.NextInt();
                if (CoreDictionary._trie.Size != size - 1)      // 检验两个词典缓存大小的一致性
                    return false;

                _start = new int[size];
                for (int i = 0; i < size; i++)
                    _start[i] = ba.NextInt();

                size = ba.NextInt();
                _pair = new int[size];
                for (int i = 0; i < size; i++)
                    _pair[i] = ba.NextInt();

                return true;
            }
            catch(Exception e)
            {
                return false;
            }
        }

        private static bool SaveDat()
        {
            var fs = new FileStream(_datPath, FileMode.Create, FileAccess.Write);
            try
            {
                var bytes = BitConverter.GetBytes(_start.Length);
                fs.Write(bytes, 0, 4);
                for(int i = 0; i < _start.Length; i++)
                {
                    bytes = BitConverter.GetBytes(_start[i]);
                    fs.Write(bytes, 0, 4);
                }
                bytes = BitConverter.GetBytes(_pair.Length);
                fs.Write(bytes, 0, 4);
                for(int i = 0; i < _pair.Length; i++)
                {
                    bytes = BitConverter.GetBytes(_pair[i]);
                    fs.Write(bytes, 0, 4);
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

        /// <summary>
        /// 获取二元词a+b出现的频次
        /// </summary>
        /// <param name="a">二元词的第一部分</param>
        /// <param name="b">二元词的第二部分</param>
        /// <returns></returns>
        public static int GetBiFreq(string a, string b)
        {
            int idA = CoreDictionary._trie.ExactMatch(a);
            if (idA == -1) return 0;

            int idB = CoreDictionary._trie.ExactMatch(b);
            if (idB == -1) return 0;

            int index = BinarySearch(_pair, _start[idA], _start[idA + 1] - _start[idA], idB);
            if (index < 0) return 0;

            index <<= 1;
            return _pair[index + 1];
        }

        private static int BinarySearch(int[] a, int from, int len, int key)
        {
            if (a.Length <= 0) return -1;
            var low = from;
            var high = from + len - 1;
            if (a.Length < from + len)
                high = a.Length - 1;

            while (low <= high)
            {
                var mid = (low + high) >> 1;
                int cmp = a[mid << 1].CompareTo(key);   // 注意这里 mid 乘以 2，使得下标成为偶数，pair数组中奇数位置对应的是频率，所以不用担心id与频率相等带来的问题
                if (cmp < 0)
                    low = mid + 1;
                else if (cmp > 0)
                    high = mid - 1;
                else
                    return mid;
            }
            return -low - 1; // 没有找到目标对象，如果要将目标按顺序插入到数组容器中，则插入位置应该为 low，为了与上面返回-1 一致，作 -low-1 处理
        }

        /// <summary>
        /// 获取二元词a+b出现的频次
        /// </summary>
        /// <param name="idA">二元词第一部分在核心词典存储的值数组下标</param>
        /// <param name="idB">二元词第二部分在核心词典存储的值数组下标</param>
        /// <returns></returns>
        public static int GetBiFreq(int idA, int idB)
        {
            if (idA == -1 || idB == -1)     // -1表示用户词典，返回正值增加其亲和度
                return 1000;

            int index = BinarySearch(_pair, _start[idA], _start[idA + 1] - _start[idA], idB);
            if (index < 0) return 0;
            index <<= 1;
            return _pair[index + 1];
        }


    }
}
