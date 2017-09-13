using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HanLP.csharp.model.CRF;
using HanLP.csharp.utility;
using HanLP.csharp.corpus.io;
using HanLP.csharp.collection.trie;
using HanLP.csharp.corpus.dependency;
using HanLP.csharp.seg.common;
using HanLP.csharp.model.bigram;

namespace HanLP.csharp.dependency
{
    public class CRFDependencyParser : BaseDependencyParser
    {
        private CRFModel _crfModel;

        public CRFDependencyParser() : this(Config.CRF_Dependency_Model_Path) { }

        public CRFDependencyParser(string modelPath)
        {
            _crfModel = GlobalCache.Get(modelPath) as CRFDependencyModel;
            if (_crfModel != null) return;
            if(Load(modelPath))
            {
                GlobalCache.Put(modelPath, _crfModel);
            }
        }

        private bool Load(string path)
        {
            if (LoadDat(path + Predefine.BIN_EXT)) return true;
            _crfModel = new CRFDependencyModel(new DoubleArrayTrie<FeatureFunction>());
            try
            {
                _crfModel.LoadFromTxt(path);
                return true;
            }
            catch(Exception e)
            {
                return false;
            }
        }

        private bool LoadDat(string path)
        {
            var ba = ByteArray.Create(path);
            if (ba == null) return false;

            _crfModel = new CRFDependencyModel(new DoubleArrayTrie<FeatureFunction>());
            return _crfModel.LoadFromBin(ba);
        }

        public override CoNLLSentence Parse(List<Term> terms)
        {
            var table = new Table();
            table.v = new string[terms.Count][];
            for(int i = 0; i < terms.Count; i++)
            {
                var term = terms[i];
                var line = new string[4];
                table.v[i] = line;
                line[0] = term.word;
                line[2] = DependencyUtil.compilePOS(term.nature);
                line[1] = line[2].Substring(0, 1);
            }
            _crfModel.Tag(table);

            var words = new CoNLLWord[table.Size];
            for(int i = 0; i < words.Length; i++)
            {
                words[i] = new CoNLLWord(i + 1, table.v[i][0], table.v[i][2], table.v[i][1]);
            }
            
            for(int i = 0; i < table.Size; i++)
            {
                var line = table.v[i];
                var dtag = new DTag(line[3]);
                if(dtag.pos.EndsWith("ROOT"))
                {
                    words[i].HEAD = CoNLLWord.ROOT;
                }
                else
                {
                    var index = ConvertOffset2Index(dtag, table, i);
                    if (index == -1)
                        words[i].HEAD = CoNLLWord.NULL;
                    else
                        words[i].HEAD = words[index];
                }
            }
            
            for(int i = 0; i < words.Length; i++)
            {
                words[i].DEPREL = BigramDependencyModel.Get(words[i].NAME, words[i].POSTAG, words[i].HEAD.NAME, words[i].HEAD.POSTAG);
            }

            return new CoNLLSentence(words);
        }

        public static int ConvertOffset2Index(DTag dtag, Table table, int current)
        {
            int posCount = 0;
            if(dtag.offset > 0)
            {
                for(int i = current + 1; i < table.Size; i++)
                {
                    if (table.v[i][1] == dtag.pos) ++posCount;
                    if (posCount == dtag.offset) return i;
                }
            }
            else
            {
                for(int i = current - 1; i >= 0; i--)
                {
                    if (table.v[i][1] == dtag.pos) ++posCount;
                    if (posCount == -dtag.offset) return i;
                }
            }

            return -1;
        }
    }
}
