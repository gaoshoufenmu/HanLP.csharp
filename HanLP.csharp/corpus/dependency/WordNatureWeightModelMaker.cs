using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using HanLP.csharp.corpus.dictionary;
using HanLP.csharp.corpus.document;
using HanLP.csharp.collection;
namespace HanLP.csharp.corpus.dependency
{
    /// <summary>
    /// 生成模型打分器模型构建类
    /// </summary>
    public class WordNatureWeightModelMaker
    {
        public static bool MakeModel(string corpusPath, string modelSavePath)
        {
            var posSet = new SortedSet<string>(StrComparer.Default);
            var dm = new DictionaryMaker();

            for (int i = 0; i < 2; i++)                 //! 执行两遍，无非是将label的频率从1增加到2
            {
                foreach (var s in CoNLLUtil.LoadSentences(corpusPath))
                {
                    foreach (var w in s.word)
                    {
                        AddPair(w.NAME, w.HEAD.NAME, w.DEPREL, dm);
                        AddPair(w.NAME, WrapTag(w.HEAD.POSTAG), w.DEPREL, dm);
                        AddPair(WrapTag(w.POSTAG), w.HEAD.NAME, w.DEPREL, dm);
                        AddPair(WrapTag(w.POSTAG), WrapTag(w.HEAD.POSTAG), w.DEPREL, dm);
                        posSet.Add(w.POSTAG);
                    }
                }
            }

            var sb = new StringBuilder();
            foreach(var pos in posSet)
            {
                sb.Append("cases \"" + pos + "\":\n");
            }
            File.WriteAllText(Config.Word_Nat_Weight_Model_Path, sb.ToString());
            return dm.SaveTxtTo(modelSavePath);
        }

        public static string WrapTag(string tag) => $"<{tag}>";

        public static void AddPair(string from, string to, string label, DictionaryMaker dm)
        {
            dm.Add(new Word(from + "@" + to, label));
            dm.Add(new Word(from + "@", "频次"));
        }
    }
}
