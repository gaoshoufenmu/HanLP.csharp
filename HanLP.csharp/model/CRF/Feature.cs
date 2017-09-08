using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HanLP.csharp.corpus.io;
using HanLP.csharp.utility;

namespace HanLP.csharp.model.CRF
{
    /// <summary>
    /// 特征函数
    /// </summary>
    public class FeatureFunction
    {
        private bool _highFirst = true;
        /// <summary>
        /// 是否是高位字节在前
        /// Big-Endian|Little-Endian
        /// </summary>
        public bool HighFirst { get => _highFirst; set => _highFirst = value; }
        /// <summary>
        /// 环境参数
        /// </summary>
        internal char[] o;
        /// <summary>
        /// 特征函数对应的权值
        /// </summary>
        internal double[] w;
        public FeatureFunction(char[] o, int tagSize)
        {
            this.o = o;
            w = new double[tagSize];
        }
        public FeatureFunction() { }

        /// <summary>
        /// 注意兼容性，没有使用高位字节在前
        /// </summary>
        /// <param name="fs"></param>
        public void Save(FileStream fs)
        {
            if (_highFirst)
            {
                var bytes = ByteUtil.Int2Byte_HighFirst(o.Length);
                fs.Write(bytes, 0, 4);
                for (int i = 0; i < o.Length; i++)
                {
                    bytes = ByteUtil.Char2Byte_HighFirst(o[i]);
                    fs.Write(bytes, 0, 2);
                }
                bytes = ByteUtil.Int2Byte_HighFirst(w.Length);
                fs.Write(bytes, 0, 4);
                for (int i = 0; i < w.Length; i++)
                {
                    bytes = ByteUtil.Double2Byte_HighFirst(w[i]);
                    fs.Write(bytes, 0, 8);
                }
            }
            else
            {
                var bytes = BitConverter.GetBytes(o.Length);
                fs.Write(bytes, 0, 4);
                for(int i = 0; i < o.Length; i++)
                {
                    bytes = BitConverter.GetBytes(o[i]);
                    fs.Write(bytes, 0, 2);
                }
                bytes = BitConverter.GetBytes(w.Length);
                fs.Write(bytes, 0, 4);
                for(int i = 0; i < w.Length; i++)
                {
                    bytes = BitConverter.GetBytes(w[i]);
                    fs.Write(bytes, 0, 8);
                }
            }
        }

        public void Load(ByteArray ba)
        {
            int size = ba.NextInt(_highFirst);
            o = new char[size];
            for(int i = 0; i < size; i++)
            {
                o[i] = ba.NextChar(_highFirst);
            }
            size = ba.NextInt(_highFirst);
            w = new double[size];

            for (int i = 0; i < size; i++)
                w[i] = ba.NextDouble(_highFirst);
        }
    }

    /// <summary>
    /// 特征模板
    /// </summary>
    public class FeatureTemplate
    {
        private bool _highFirst = true;
        /// <summary>
        /// 是否是高位字节在前
        /// </summary>
        public bool HighFirst { get => _highFirst; set => _highFirst = value; }
        private string _template;

        /// <summary>
        /// 以"%x["开头，"]"结尾，中间是有符号的用一个comma分隔的数字
        /// </summary>
        private static Regex _pattern = new Regex(@"%x\[(-?\d*),(\d*)]");
        private List<string> _delimiters;

        /// <summary>
        /// 每个部分%x[-2,0]的位移，其中int[0]存储第一个数(-2)， int[1]存储第二个数（0）
        /// </summary>
        private List<int[]> _offsets;


        public static FeatureTemplate Create(string template)
        {
            var ft = new FeatureTemplate();
            ft._delimiters = new List<string>();
            ft._offsets = new List<int[]>(3);
            ft._template = template;

            var matches = _pattern.Matches(template);
            int start = 0;
            foreach(Match m in matches)
            {
                ft._delimiters.Add(template.Substring(start, m.Index));
                start = m.Index + m.Length;
                ft._offsets.Add(new[] { int.Parse(m.Groups[1].Value), int.Parse(m.Groups[2].Value)});       // m.Groups是否从0 开始？
            }
            return ft;
        }

        public char[] GenerateParam(Table table, int curr)
        {
            var sb = new StringBuilder(_delimiters.Count * 10);
            for(int i = 0; i < _delimiters.Count; i++)
            {
                sb.Append(_delimiters[i]);
                var coord = _offsets[i];
                sb.Append(table.Get(curr + coord[0], coord[1]));
            }
            var o = new char[sb.Length];
            sb.CopyTo(0, o, 0, sb.Length);
            return o;
        }

        public string GenerateParam2Str(Table table, int curr)
        {
            var sb = new StringBuilder(_delimiters.Count * 10);
            for (int i = 0; i < _delimiters.Count; i++)
            {
                sb.Append(_delimiters[i]);
                var coord = _offsets[i];
                sb.Append(table.Get(curr + coord[0], coord[1]));
            }
            return sb.ToString();
        }

        public override string ToString()
        {
            var sb = new StringBuilder(20 + _template.Length);
            sb.Append("template:'").Append(_template).Append("'");
            return sb.ToString();
        }

        public void Load(ByteArray ba)
        {
            _template = ba.NextUTFStr(_highFirst);
            var size = ba.NextInt(_highFirst);
            _offsets = new List<int[]>(size);
            for (int i = 0; i < size; i++)
                _offsets.Add(new[] { ba.NextInt(_highFirst), ba.NextInt(_highFirst) });

            size = ba.NextInt(_highFirst);
            _delimiters = new List<string>(size);
            for (int i = 0; i < size; i++)
                _delimiters.Add(ba.NextUTFStr(_highFirst));
        }

        /// <summary>
        /// 为了与原数据文件兼容，为了简单，只考虑了高位字节优先的情况
        /// </summary>
        /// <param name="fs"></param>
        public void Save(FileStream fs)
        {
            var bs = ByteUtil.UTF2Byte_HighFirst(_template);
            fs.Write(bs, 0, bs.Length);

            bs = ByteUtil.Int2Byte_HighFirst(_offsets.Count);
            fs.Write(bs, 0, bs.Length);
            for(int i = 0; i < _offsets.Count; i++)
            {
                bs = ByteUtil.Int2Byte_HighFirst(_offsets[i][0]);
                fs.Write(bs, 0, bs.Length);
                bs = ByteUtil.Int2Byte_HighFirst(_offsets[i][1]);
                fs.Write(bs, 0, bs.Length);
            }
            bs = ByteUtil.Int2Byte_HighFirst(_delimiters.Count);
            fs.Write(bs, 0, bs.Length);
            for(int i = 0; i < _delimiters.Count; i++)
            {
                bs = ByteUtil.UTF2Byte_HighFirst(_delimiters[i]);
                fs.Write(bs, 0, bs.Length);
            }
        }
    }

    /// <summary>
    /// 给一个实例生成一个元素表
    /// </summary>
    public class Table
    {
        public const string HEAD = "_B";

        public string[][] v;

        private int _size = -1;
        public int Size
        {
            get
            {
                if (_size < 0)
                    _size = v.GetLength(0);
                return _size;
            }
        }
        /// <summary>
        /// 获取指定坐标的元素
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public string Get(int x, int y)
        {
            if (x < 0) return HEAD + x;
            if (x >= v.Length) return HEAD + "+" + (x - v.Length + 1);

            return v[x][y];
        }

        /// <summary>
        /// 设置给定横坐标那一行的最后一个值
        /// </summary>
        /// <param name="x"></param>
        /// <param name="t"></param>
        public void SetLast(int x, string t) => v[x][v[x].Length - 1] = t;
    }
}
