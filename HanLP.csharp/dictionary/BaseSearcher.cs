using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HanLP.csharp.collection.trie.bintrie;

namespace HanLP.csharp.dictionary
{
    /// <summary>
    /// 查询者基类
    /// </summary>
    public abstract class BaseSearcher<V>
    {
        /// <summary>
        /// 待分词文件的字符列表
        /// </summary>
        protected char[] c;

        /// <summary>
        /// 当前处理的开始位置，在此位置之前的分词已经确定
        /// 注意不是当前读取头所指位置
        /// </summary>
        protected int offset;

        public BaseSearcher(char[] c) => this.c = c;

        public BaseSearcher(string text) => this.c = text.ToCharArray();

        /// <summary>
        /// 获取下一个分词
        /// </summary>
        /// <returns></returns>
        public abstract Tuple<string, V> Next();

        public int Offset => offset;
    }
}
