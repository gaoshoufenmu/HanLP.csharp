using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using HanLP.csharp.dictionary.common;
using HanLP.csharp.corpus.dictionary;
using HanLP.csharp.corpus.tag;

namespace HanLP.csharp.dictionary.ns
{
    public class NSDictionary : CommonDictionary<TagFreqItem<NS>>
    {
        public override TagFreqItem<NS>[] OnLoadValue(string path)
        {
            var valueArr = LoadDat(path + ".value.dat");
            if (valueArr != null) return valueArr;

            var valueList = new List<TagFreqItem<NS>>();
            try
            {
                foreach(var line in File.ReadAllLines(path))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var tuple = TagFreqItem<NS>.Create(line);
                    var tfi = new TagFreqItem<NS>();
                    foreach(var p in tuple.Item2)
                    {
                        tfi.labelMap.Add((NS)Enum.Parse(typeof(NS), p.Key), p.Value);
                    }
                    valueList.Add(tfi);
                }
            }
            catch(Exception e)
            {
                // log error
            }
            return valueList.ToArray();
        }

        private TagFreqItem<NS>[] LoadDat(string path)
        {
            if (!File.Exists(path)) return null;
            var bytes = File.ReadAllBytes(path);

            int index = 0;
            int size = BitConverter.ToInt32(bytes, index);
            index += 4;

            var valueArr = new TagFreqItem<NS>[size];
            for(int i = 0; i < size; i++)
            {
                var currSize = BitConverter.ToInt32(bytes, index);
                index += 4;

                var tfi = new TagFreqItem<NS>();
                for(int j = 0; j < currSize; j++)
                {
                    var tag = BitConverter.ToInt32(bytes, index);
                    index += 4;

                    var freq = BitConverter.ToInt32(bytes, index);
                    index += 4;

                    tfi.labelMap.Add((NS)tag, freq);
                }
                valueArr[i] = tfi;
            }
            return valueArr;
        }

        public override bool OnSaveValue(TagFreqItem<NS>[] valueArr, string path) =>
            SaveDat(path + ".value.dat", valueArr);

        private bool SaveDat(string path, TagFreqItem<NS>[] valueArr)
        {
            var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            try
            {
                var bytes = BitConverter.GetBytes(valueArr.Length);
                fs.Write(bytes, 0, 4);

                for(int i = 0; i < valueArr.Length; i++)
                {
                    var tfi = valueArr[i];
                    bytes = BitConverter.GetBytes(tfi.labelMap.Count);
                    fs.Write(bytes, 0, 4);
                    foreach(var p in tfi.labelMap)
                    {
                        bytes = BitConverter.GetBytes((int)p.Key);
                        fs.Write(bytes, 0, 4);

                        bytes = BitConverter.GetBytes(p.Value);
                        fs.Write(bytes, 0, 4);
                    }
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
