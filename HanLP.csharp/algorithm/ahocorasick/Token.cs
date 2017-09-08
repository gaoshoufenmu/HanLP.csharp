using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanLP.csharp.algorithm.ahocorasick
{
    public class Emit : Interval
    {
        private string _keyword;
        public string Keyword => _keyword;

        public Emit(int start, int end, string keyword) : base(start, end)
        {
            _keyword = keyword;
        }
        public override string ToString() => $"{base.ToString()}={_keyword}";
    }

    public abstract class Token
    {
        private string _fragment;
        public string Fragment => _fragment;
        public Token(string fragment) => _fragment = fragment;

        public abstract bool IsMatch { get; }
        public abstract Emit Emit { get; }
    }

    public class FragmentToken : Token
    {
        public FragmentToken(string fragment) : base(fragment) { }
        public override bool IsMatch => false;
        public override Emit Emit => null;
    }

    public class MatchToken : Token
    {
        private Emit _emit;

        public MatchToken(string fragment, Emit emit) : base(fragment)
        {
            _emit = emit;
        }
        public override bool IsMatch => true;
        public override Emit Emit => _emit;
    }
}
