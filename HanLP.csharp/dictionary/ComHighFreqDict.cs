using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using HanLP.csharp.collection;
using HanLP.csharp.collection.trie;

namespace HanLP.csharp.dictionary
{
    public class ComHighFreqDict
    {
        public static DoubleArrayTrie<int> _trie;

        static ComHighFreqDict()
        {
            Load(Config.Com_HighFreq_Dict_Path);
        }

        public static void Load(string path)
        {
            _trie = new DoubleArrayTrie<int>();
            var valueArr = LoadDat(path + ".value.dat");
            if(valueArr != null)
            {
                if (_trie.Load(path + ".trie.dat", valueArr))
                    return;
            }
            var map = new SortedDictionary<string, int>(StrComparer.Default);
            foreach(var line in File.ReadLines(path))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var segs = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                map[segs[0]] = int.Parse(segs[1]);
            }
            _trie = new DoubleArrayTrie<int>();
            _trie.Build(map);
            valueArr = new int[map.Count];
            int m = 0;
            foreach (var v in map.Values)
                valueArr[m++] = v;

            var fs = new FileStream(path + ".trie.dat", FileMode.Create, FileAccess.Write);
            _trie.Save(fs);
            fs.Close();
            SaveDat(path + ".value.dat", valueArr);
        }

        public static int GetFreq(string key) => _trie.GetOrDefault(key);

        private static int[] LoadDat(string path)
        {
            if (!File.Exists(path)) return null;
            var bytes = File.ReadAllBytes(path);

            int index = 0;
            int size = BitConverter.ToInt32(bytes, index);
            index += 4;

            var values = new int[size];
            for(int i = 0; i < size; i++)
            {
                values[i] = BitConverter.ToInt32(bytes, index);
                index += 4;
            }
            return values;
        }

        private static bool SaveDat(string path, int[] values)
        {
            var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            try
            {
                var bytes = BitConverter.GetBytes(values.Length);
                fs.Write(bytes, 0, 4);
                for (int i = 0; i < values.Length; i++)
                {
                    bytes = BitConverter.GetBytes(values[i]);
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
    }
}
