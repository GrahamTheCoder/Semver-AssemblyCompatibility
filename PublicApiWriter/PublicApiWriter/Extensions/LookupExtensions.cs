using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssemblyApi.Extensions
{
    internal static class LookupExtensions
    {
        public static Dictionary<TKey, List<TValue>> ToDictionary<TKey, TValue>(this ILookup<TKey, TValue> lookup)
        {
            return lookup.ToDictionary(x => x.Key, x => x.ToList());
        }
    }
}
