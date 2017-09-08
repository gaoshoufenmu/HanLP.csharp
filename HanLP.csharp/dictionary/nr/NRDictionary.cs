using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using HanLP.csharp.utility;
using HanLP.csharp.corpus.io;
using HanLP.csharp.dictionary.common;
using HanLP.csharp.corpus.dictionary;
using HanLP.csharp.corpus.tag;

namespace HanLP.csharp.dictionary.nr
{
    public class NRDictionary : CommonDictionary<TagFreqItem<NR>>
    {
        public override TagFreqItem<NR>[] OnLoadValue(string path)
        {
            var valueArr = LoadDat(path + ".value.dat");
            if (valueArr != null) return valueArr;

            var valueList = new List<TagFreqItem<NR>>();
            try
            {
                foreach(var line in File.ReadLines(path))
                {
                    var tuple = TagFreqItem<NR>.Create(line);
                    var tfi = new TagFreqItem<NR>();

                    foreach(var p in tuple.Item2)
                    {
                        //tfi.AddLabel((NR)Enum.Parse(typeof(NR), p.Key), p.Value);
                        tfi.labelMap[(NR)Enum.Parse(typeof(NR), p.Key)] = p.Value;
                    }
                    valueList.Add(tfi);
                }
            }
            catch(Exception e)
            {
                return null;
            }

            return valueList.ToArray();
        }

        private TagFreqItem<NR>[] LoadDat(string path)
        {
            //var bytes = File.ReadAllBytes(path);
            var ba = ByteArray.Create(path);
            if (ba == null) return null;

            //if (bytes == null || bytes.Length < 5) return null;
            //int index = 0;
            //int size = ByteUtil.Bytes2Int(bytes, index);
            //index += 4;
            int size = ba.NextInt();

            var valueArr = new TagFreqItem<NR>[size];

            for(int i = 0; i < size; i++)
            {
                //var currSize = ByteUtil.Bytes2Int(bytes, index);
                //index += 4;
                var currSize = ba.NextInt();

                var tfi = new TagFreqItem<NR>();
                for(int j = 0; j < currSize; j++)
                {
                    //var enumVal = ByteUtil.Bytes2Int(bytes, index);
                    //index += 4;

                    //var freq = ByteUtil.Bytes2Int(bytes, index);
                    //index += 4;

                    var enumVal = ba.NextInt();
                    var freq = ba.NextInt();

                    tfi.AddLabel((NR)enumVal, freq);
                }
                valueArr[i] = tfi;
            }
            return valueArr;
        }

        private bool SaveDat(string path , TagFreqItem<NR>[] valueArr)
        {
            var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            try
            {         
                byte[]  bytes = BitConverter.GetBytes(valueArr.Length);
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

                        bytes = BitConverter.GetBytes(p.Value);
                        fs.Write(bytes, 0, bytes.Length);
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
            finally
            {
                fs.Close();
            }
        }

        public override bool OnSaveValue(TagFreqItem<NR>[] valueArr, string path) => SaveDat(path + ".value.dat", valueArr);
    }
}
