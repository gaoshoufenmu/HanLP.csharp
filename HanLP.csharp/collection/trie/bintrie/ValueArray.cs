using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanLP.csharp.collection.trie.bintrie
{
    public class ValueArray<V>
    {
        internal V[] values;
        private int _offset;

        public ValueArray() { }

        public ValueArray(V[] values) => this.values = values;
        public V NextValue() => values == null ? default(V) : values[_offset++];

        public void ResetPos() => _offset = 0;
        public bool HasMore() => values != null && _offset < values.Length - 1;

        public ValueArray<V> SetValues(V[] vs)
        {
            values = vs;
            return this;
        }

        public static readonly ValueArray<V> NullCreater = new ValueArray<V>();
    }
}
