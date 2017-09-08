using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanLP.csharp.interfaces
{
    public interface ITextHit<V>
    {
        /// <summary>
        /// 命中一个文本匹配的模式串
        /// </summary>
        /// <param name="begin">模式串在文本中的起始位置inclusive</param>
        /// <param name="end">模式串在文本中的结束位置exclusive</param>
        /// <param name="v">模式串对应的值</param>
        void Hit(int begin, int end, V v);
    }

    public interface ITextHitFull<V>
    {
        /// <summary>
        /// 命中一个文本匹配的模式串
        /// </summary>
        /// <param name="begin">模式串在文本中的起始位置inclusive</param>
        /// <param name="end">模式串在文本中的结束位置exclusive</param>
        /// <param name="v">模式串对应的值</param>
        /// <param name="index">模式串在模式串列表中的下标</param>
        void Hit(int begin, int end, V v, int index);
    }
}
