using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using HanLP.csharp.utility;

namespace HanLP.csharp.corpus.dependency
{
    public class CoNLLUtil
    {
        public static string Compile(string tag, string name)
        {
            if (tag.StartsWith("m")) return Constants.TAG_NUMBER;
            if (tag.StartsWith("nr")) return Constants.TAG_PEOPLE;
            if (tag.StartsWith("ns")) return Constants.TAG_PLACE;
            if (tag.StartsWith("nt")) return Constants.TAG_GROUP;
            if (tag.StartsWith("t")) return Constants.TAG_TIME;
            if (tag.StartsWith("x")) return Constants.TAG_CLUSTER;
            if (tag.StartsWith("nx")) return Constants.TAG_PROPER;
            if (tag.StartsWith("xx")) return Constants.TAG_OTHER;

            return name;
        }

        public static void Fix(string path)
        {
            var sb = new StringBuilder();
            foreach(var line in File.ReadLines(path))
            {
                if (string.IsNullOrWhiteSpace(line))
                    sb.Append(line).Append('\n');
                else
                {
                    var args = line.Split('\t');
                    sb.Append(line);
                    for (int i = 10 - args.Length; i > 0; i--)
                        sb.Append("\t_");
                    sb.Append('\n');
                }
            }
            File.WriteAllText(path + ".fixed.txt", sb.ToString());
        }

        public static List<CoNLLSentence> LoadSentences(string path)
        {
            var sentences = new List<CoNLLSentence>();
            var lines = new List<CoNllLine>();
            foreach(var line in File.ReadLines(path))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    sentences.Add(new CoNLLSentence(lines));
                    lines = new List<CoNllLine>();
                }
                else
                    lines.Add(new CoNllLine(line.Split('\t')));
            }
            return sentences;
        }
    }
}
