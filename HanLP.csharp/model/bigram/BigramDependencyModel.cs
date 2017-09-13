using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using HanLP.csharp.collection.trie;
using HanLP.csharp.corpus.io;
using HanLP.csharp.utility;
using HanLP.csharp.collection;
using HanLP.csharp.corpus.dependency;

namespace HanLP.csharp.model.bigram
{
    public class BigramDependencyModel
    {
        private static DoubleArrayTrie<string> _trie;

        static BigramDependencyModel()
        {
            if(!Load(Config.Word_Nature_Model_Path))
            {
                // log error
            }
        }
        private static bool Load(string path)
        {
            _trie = new DoubleArrayTrie<string>();
            if (LoadDat(path + ".bi" + Predefine.BIN_EXT)) return true;

            var map = new SortedDictionary<string, string>(StrComparer.Default);
            foreach(var line in File.ReadLines(path))
            {
                var param = line.Split(' ');
                if (param[0].EndsWith("@")) continue;

                var dependency = param[1];
                map[param[0]] = dependency;
            }

            if (map.Count == 0) return false;
            _trie.Build(map);
            if(!SaveDat(path, map))
            {
                // log error
            }
            return true;
        }

        private static bool LoadDat(string path)
        {
            var ba = ByteArray.Create(path);
            if (ba == null) return false;
            var size = ba.NextInt_HighFirst();          // 直接读取原始二进制文件，需要兼容模式
            var arr = new string[size];
            for (int i = 0; i < arr.Length; i++)
                arr[i] = ba.NextUTFStr(true);

            return _trie.Load(ba, arr, true);
        }


        /// <summary>
        /// High First
        /// </summary>
        /// <param name="path"></param>
        /// <param name="map"></param>
        /// <returns></returns>
        public static bool SaveDat(string path, SortedDictionary<string, string> map)
        {
            var fs = new FileStream(path + ".bi" + Predefine.BIN_EXT, FileMode.Create, FileAccess.Write);
            try
            {
                var values = map.Values;
                var bytes = ByteUtil.Int2Byte_HighFirst(values.Count);
                fs.Write(bytes, 0, 4);

                foreach(var v in values)
                {
                    bytes = ByteUtil.UTF2Byte_HighFirst(v);
                    fs.Write(bytes, 0, bytes.Length);
                }
                return _trie.Save(fs);
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

        public static string Get(string key) => _trie.GetOrDefault(key);

        public static string Get(string fromWord, string fromPos, string toWord, string toPos)
        {
            var dependency = Get(fromWord + "@" + toWord);
            if (dependency == null) dependency = Get(fromWord + "@" + WordNatureWeightModelMaker.WrapTag(toPos));
            if (dependency == null) dependency = Get(WordNatureWeightModelMaker.WrapTag(fromPos) + "@" + toWord);
            if (dependency == null) dependency = Get(WordNatureWeightModelMaker.WrapTag(fromPos) + "@" + WordNatureWeightModelMaker.WrapTag(toPos));
            if (dependency == null) return "未知";
            return dependency;
        }
    }
}
