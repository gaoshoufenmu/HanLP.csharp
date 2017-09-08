using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanLP.csharp.utility
{
    public class Serializer
    {
        public static byte[] Serialize<V>(V v)
        {
            var type = typeof(V);
            switch(type.Name)
            {
                case "String":
                    return Encoding.Default.GetBytes(v as string);
                case "Int32":
                case "Int16":
                case "Double":
                case "Byte":
                case "Boolean":
                    dynamic i = v;
                    return BitConverter.GetBytes(i);
                default:
                    throw new ArgumentException("unknown type");
            }
        }

        public static V Deserialize<V>(byte[] bytes)
        {
            var type = typeof(V);
            switch (type.Name)
            {
                case "String":
                    dynamic str = BitConverter.ToString(bytes);
                    return str;
                case "Int32":
                    dynamic i = BitConverter.ToInt32(bytes, 0);
                    return i;
                case "Byte":
                    dynamic bt = bytes[0];
                    return bt;
                case "Int16":
                    dynamic us = BitConverter.ToInt16(bytes, 0);
                    return us;
                case "Double":
                    dynamic d = BitConverter.ToDouble(bytes, 0);
                    return d;
                case "Boolean":
                    dynamic b = BitConverter.ToBoolean(bytes, 0);
                    return b;
                default:
                    throw new ArgumentException("unknown type");
            }
        }
    }
}
