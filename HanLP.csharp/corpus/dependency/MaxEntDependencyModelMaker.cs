using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace HanLP.csharp.corpus.dependency
{
    /// <summary>
    /// 最大熵模型构建类，训练时不使用自己的代码，借用opennlp训练
    /// 本maker只生成训练文件
    /// </summary>
    public class MaxEntDependencyModelMaker
    {
        public static void MakeModel(string corpusPath, string modelSavePath)
        {
            var sentences = CoNLLUtil.LoadSentences(corpusPath);
            var lines = new List<string>(sentences.Count * 30);
            var sb = new StringBuilder();
            //int id = 1;
            for(int k = 0; k < sentences.Count; k++)
            {
                var s = sentences[k];

                var edges = s.GetEdgeArr();             // 获取一句话中各词之间的依存关系（边）
                var word = s.GetWordArrWithRoot();      // 获取一句话中的所有单词，包括虚根节点
                var size = edges.GetLength(0);
                for(int i = 0; i < size; i++)
                {
                    for(int j = 0; j < size; j++)
                    {
                        if (i == j) continue;           // 词条与自身之间需要不考虑依存关系

                        var contexts = new List<string>();
                        // 从i出发到达j的边，可能存在这样的依存关系，也可能不存在，此时使用NULL代替
                        contexts.AddRange(GenerateSingleWordContext(word, i, "i"));
                        contexts.AddRange(GenerateSingleWordContext(word, j, "j"));
                        contexts.AddRange(GenerateUniContext(word, i, j));

                        foreach(var c in contexts)
                        {
                            sb.Append(c).Append(' ');
                        }
                        sb.Append(edges[i, j]);
                        lines.Add(sb.ToString());
                        sb.Clear();
                    }
                }
            }
            File.WriteAllLines(modelSavePath, lines.ToArray());
        }

        /// <summary>
        /// 获取一组单词中指定目标位置处单词的上下文
        /// 从当前单词的前2单词处（如果不存在，则使用NULL代替）开始，上下文为 活动单词名称/词性 + 目标位置mark + 活动单词位置与目标位置的差
        /// </summary>
        /// <param name="word"></param>
        /// <param name="index"></param>
        /// <param name="mark"></param>
        /// <returns></returns>
        public static IEnumerable<string> GenerateSingleWordContext(CoNLLWord[] word, int index, string mark)
        {
            var context = new List<string>();
            CoNLLWord w = null;
            for(int i = index - 2; i < index + 2 + 1; i++)
            {
                if (i >= 0 && i < word.Length)
                    w = word[i];
                else
                    w = CoNLLWord.NULL;

                context.Add(w.NAME + mark + (i - index));
                context.Add(w.POSTAG + mark + (i - index));
            }
            return context;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="word"></param>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <returns></returns>
        public static IEnumerable<string> GenerateUniContext(CoNLLWord[] word, int i, int j)
        {
            var context = new List<string>();
            context.Add(word[i].NAME + '→' + word[j].NAME);
            context.Add(word[i].POSTAG + '→' + word[j].POSTAG);
            context.Add(word[i].NAME + '→' + word[j].NAME + (i - j));
            context.Add(word[i].POSTAG + '→' + word[j].POSTAG + (i - j));
            CoNLLWord wordBeforeI = i - 1 >= 0 ? word[i - 1] : CoNLLWord.NULL;
            CoNLLWord wordBeforeJ = j - 1 >= 0 ? word[j - 1] : CoNLLWord.NULL;
            context.Add(wordBeforeI.NAME + '@' + word[i].NAME + '→' + word[j].NAME);
            context.Add(word[i].NAME + '→' + wordBeforeJ.NAME + '@' + word[j].NAME);
            context.Add(wordBeforeI.POSTAG + '@' + word[i].POSTAG + '→' + word[j].POSTAG);
            context.Add(word[i].POSTAG + '→' + wordBeforeJ.POSTAG + '@' + word[j].POSTAG);
            return context;
        }
    }
}
