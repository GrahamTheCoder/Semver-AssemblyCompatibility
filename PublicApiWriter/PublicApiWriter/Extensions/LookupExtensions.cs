using System.Collections.Generic;
using System.Linq;

namespace Gtc.AssemblyApi.Extensions
{
    internal static class LookupExtensions
    {
        public static Dictionary<TKey, List<TValue>> ToDictionary<TKey, TValue>(this ILookup<TKey, TValue> lookup)
        {
            return lookup.ToDictionary(x => x.Key, x => x.ToList());
        }
    }
}
