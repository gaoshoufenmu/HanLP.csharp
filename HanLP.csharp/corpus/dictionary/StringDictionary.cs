using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using HanLP.csharp.collection.trie.bintrie;

namespace HanLP.csharp.corpus.dictionary
{
    public class StringDictionary : SimpleDictionary<string>
    {
        private string _separator;

        public StringDictionary(string separator) => _separator = separator;

        public StringDictionary() => _separator = "=";

        public override TrieEntry<string> OnGenerateEntry(string line)
        {
            var segs = line.Split(_separator.ToCharArray());
            if(segs.Length != 2)
            {
                // log warning "read error"
                return null;
            }
            
            return new TrieEntry<string>(segs[0], segs[1]);
        }

        public bool Save(string path)
        {
            var fs = new FileStream(path, FileMode.Create, FileAccess.Write);

            byte[] bytes;
            var encoding = Encoding.UTF8;
            try
            {
                foreach (var entry in trie.GetEntries())
                {
                    bytes = encoding.GetBytes(entry.Key);
                    fs.Write(bytes, 0, bytes.Length);

                    bytes = encoding.GetBytes(_separator);
                    fs.Write(bytes, 0, bytes.Length);

                    bytes = encoding.GetBytes(entry.Value);
                    fs.Write(bytes, 0, bytes.Length);
                }
                return true;
            }
            catch(Exception e)
            {
                // log warning
                return false;
            }
            finally
            {
                fs.Close();
            }
        }

        /// <summary>
        /// 将key 和 value 反转
        /// </summary>
        /// <returns></returns>
        public StringDictionary Reverse()
        {
            var dict = new StringDictionary(_separator);
            foreach (var entry in GetEntries())
                dict.trie.Put(entry.Value, entry.Key);

            return dict;
        }
    }
}
