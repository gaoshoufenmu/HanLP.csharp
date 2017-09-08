using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HanLP.csharp.corpus.tag;
using HanLP.csharp.utility;

namespace HanLP.csharp.seg.common
{
    /// <summary>
    /// 原子分词节点类
    /// </summary>
    public class AtomNode
    {
        /// <summary>
        /// 词
        /// </summary>
        public string word;
        /// <summary>
        /// 字符类型
        /// </summary>
        public int pos;

        public AtomNode(string word, int pos)
        {
            this.word = word;
            this.pos = pos;
        }

        public AtomNode(char c, int pos)
        {
            word = c.ToString();
            this.pos = pos;
        }

        /// <summary>
        /// 根据字符类型获取词性
        /// </summary>
        /// <returns></returns>
        public Nature GetNature()
        {
            var nat = Nature.nz;

            switch(pos)
            {
                case Constants.CT_CHINESE:
                    break;
                //case var x when x < Constants.CT_NUM:
                //    break;
                case Constants.CT_INDEX:
                case Constants.CT_NUM:
                    nat = Nature.m;
                    word = "未##数";
                    break;
                case Constants.CT_QUANT:
                    nat = Nature.q;
                    word = "未##量";
                    break;
                case Constants.CT_DELIMITER:
                    nat = Nature.w;
                    break;
                case Constants.CT_LETTER:
                    nat = Nature.nx;
                    word = "未##串";
                    break;
                case Constants.CT_SINGLE:
                    if(Predefine.FLOAT_NUM_REG.IsMatch(word))
                    {
                        nat = Nature.m;
                        word = "未##数";
                    }
                    else
                    {
                        nat = Nature.nx;
                        word = "未##串";
                    }
                    break;
                default:
                    break;
            }
            return nat;
        }

        public override string ToString() => $"{word}, {pos}";
    }
}
