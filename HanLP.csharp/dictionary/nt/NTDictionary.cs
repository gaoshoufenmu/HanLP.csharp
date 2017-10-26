using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using HanLP.csharp.dictionary.common;
using HanLP.csharp.corpus.dictionary;
using HanLP.csharp.corpus.tag;
namespace HanLP.csharp.dictionary.nt
{
    public class NTDictionary : CommonDictionary<TagFreqItem<NT>>
    {
        public override TagFreqItem<NT>[] OnLoadValue(string path)
        {
            var valueArr = LoadDat(path + ".value.dat");
            if (valueArr != null) return valueArr;

            var valueList = new List<TagFreqItem<NT>>();
            try
            {
                foreach(var line in File.ReadLines(path))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var tuple = TagFreqItem<NT>.Create(line);
                    var tfi = new TagFreqItem<NT>();

                    foreach(var p in tuple.Item2)
                    {
                        tfi.labelMap.Add((NT)Enum.Parse(typeof(NT), p.Key), p.Value);
                    }
                    valueList.Add(tfi);
                }
            }
            catch(Exception e)
            {
                // log load error
            }
            return valueList.ToArray();
        }

        public override bool OnSaveValue(TagFreqItem<NT>[] valueArr, string path) => SaveDat(path + ".value.dat", valueArr);

        private bool SaveDat(string path, TagFreqItem<NT>[] valueArr)
        {
            var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            try
            {
                var bytes = BitConverter.GetBytes(valueArr.Length);
                fs.Write(bytes, 0, bytes.Length);

                for(int i = 0; i < valueArr.Length; i++)
                {
                    var tfi = valueArr[i];
                    bytes = BitConverter.GetBytes(tfi.labelMap.Count);
                    fs.Write(bytes, 0, bytes.Length);
                    foreach(var p in tfi.labelMap)
                    {
                        bytes = BitConverter.GetBytes((int)p.Key);
                        fs.Write(bytes, 0, bytes.Length);

                        bytes = BitConverter.GetBytes((int)p.Value);
                        fs.Write(bytes, 0, bytes.Length);
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

        private TagFreqItem<NT>[] LoadDat(string path)
        {
            if (!File.Exists(path)) return null;
            var bytes = File.ReadAllBytes(path);

            int index = 0;
            int size = BitConverter.ToInt32(bytes, index);
            index += 4;

            var valueArr = new TagFreqItem<NT>[size];

            for(int i = 0; i < size; i++)
            {
                var currSize = BitConverter.ToInt32(bytes, index);
                index += 4;

                var tfi = new TagFreqItem<NT>();
                for(int j = 0; j < currSize; j++)
                {
                    var tagVal = BitConverter.ToInt32(bytes, index);
                    index += 4;

                    var freq = BitConverter.ToInt32(bytes, index);
                    index += 4;

                    tfi.labelMap.Add((NT)tagVal, freq);
                }

                valueArr[i] = tfi;
            }
            return valueArr;
        }
    }
}
