using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanLP.csharp.utility
{
    public class GlobalCache
    {
        public static Dictionary<string, WeakReference> _cache = new Dictionary<string, WeakReference>();
        private static object _lock = new object();
        public static object Get(string id)
        {
            if(_cache.TryGetValue(id, out WeakReference @ref))
            {
                return @ref.Target;
            }
            return null;
        }

        public static object Put(string id, object t)
        {
            if(!_cache.ContainsKey(id))
            {
                lock(_lock)
                {
                    if(!_cache.ContainsKey(id))
                    {
                        _cache.Add(id, new WeakReference(t));
                        return t;
                    }
                }
            }
            return _cache[id].Target;
        }

        public static void Clear()
        {
            lock(_lock)
            {
                _cache.Clear();
            }
        }
    }
}
