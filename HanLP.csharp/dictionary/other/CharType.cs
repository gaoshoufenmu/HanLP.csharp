using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using HanLP.csharp.corpus.io;
using HanLP.csharp.utility;

namespace HanLP.csharp.dictionary.other
{
    class CharType
    {
        /// <summary>
        /// 每个字符所属类型，下标由字符表示
        /// 由于类型总数量不大，个人觉得也可以使用一个二叉树来表示，节点表示一个区间，每次搜索最大时间复杂度O(log N)，其中N 为分类数
        /// </summary>
        public static byte[] type;

        static CharType()
        {
            type = new byte[65536];
            var byteArray = ByteArray.Create(Config.Char_Type_Path);
            if (byteArray == null)
                byteArray = Generate();
            
            while(byteArray.HasMore())
            {
                int b = byteArray.NextChar();    // begin
                int e = byteArray.NextChar();    // end
                byte t = byteArray.NextByte();  // type

                for (int i = b; i <= e; i++)
                    type[i] = t;
            }
        }

        private static ByteArray Generate()
        {
            int preType = Constants.CT_SINGLE;
            var preChar = 0;

            var list = new List<int[]>();

            // 将char类型按值获取其类型，然后按类型分类，每个类覆盖的范围的起始点，截止点，以及类型自身数据
            for (int i = 0; i <= char.MaxValue; i++)
            {
                int type = TextUtil.CharType((char)i);      // 获取当前字符的类型
                if(type != preType)                         //! 如果与前一字符的类型不同
                {
                    int[] arr = new int[3];                 // 构建一个数组，记录前一组类型相同的字符
                    arr[0] = preChar;                       // 前一组类型相同的字符起始位置，即起始字符值
                    arr[1] = i - 1;                         // 前一组类型相同的字符截止位置，即截止字符值
                    arr[2] = preType;                       // 前一组字符的类型
                    list.Add(arr);                          // 将前一组字符添加到列表
                    preChar = i;                            // 记录当前组类型相同的第一个字符位置，即当前起始字符值
                }
                preType = type;                             //! 更新上一个字符的类型
            }

            // 处理最后一组相同类型的字符组
            var lastArr = new int[3];
            lastArr[0] = preChar;
            lastArr[1] = char.MaxValue;
            lastArr[2] = preType;

            list.Add(lastArr);

            // 保存到文件中

            var fs = new FileStream(Config.Char_Type_Path, FileMode.Create, FileAccess.Write);
            try
            {
                byte[] bytes;
                for(int i = 0; i < list.Count; i++)
                {
                    var arr = list[i];
                    for (int j = 0; j < 2; j++)
                    {
                        bytes = BitConverter.GetBytes((char)arr[i]);
                        fs.Write(bytes, 0, 2);
                    }
                    fs.WriteByte((byte)arr[2]);
                } 
            }
            catch(Exception e) { }
            finally
            {
                fs.Close();
            }
            return ByteArray.Create(Config.Char_Type_Path);
        }

        public static byte Get(char c) => type[c];
    }
}
