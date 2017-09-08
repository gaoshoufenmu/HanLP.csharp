using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using HanLP.csharp.corpus.io;

namespace HanLP.csharp.corpus.document
{
    public class CorpusLoader
    {
        public static List<Document> Convert2Docs(string dir)
        {
            var list = new List<Document>();
            foreach(var path in Directory.GetFiles(dir))
            {
                list.Add(Document.Create(IOUtil.ReadTxt(path)));
            }
            return list;
        }

        public static List<List<Word>> GetSimpleWordsAsSentences(string dir)
        {
            var docs = Convert2Docs(dir);
            var lists = new List<List<Word>>();
            for(int i = 0; i < docs.Count; i++)
            {
                lists.AddRange(docs[i].GetSimpleWordsAsSentences());
            }
            return lists;
        }

        public static List<List<IWord>> GetWordsAsSentences(string dir)
        {
            var docs = Convert2Docs(dir);
            var lists = new List<List<IWord>>();
            for(int i = 0; i < docs.Count; i++)
            {
                foreach (var s in docs[i].Sentences)
                    lists.Add(s.Words);
            }
            return lists;
        }

        public static Document Convert2Doc(string path) => Document.Create(IOUtil.ReadTxt(path));

    }
}
