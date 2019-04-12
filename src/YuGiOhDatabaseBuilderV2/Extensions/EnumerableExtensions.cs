using System;
using System.Collections.Generic;
using System.Linq;

namespace YuGiOhDatabaseBuilderV2.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> source, int nSize = 1000)
        {
            var list = source.ToList();
            for (var i = 0; i < list.Count; i += nSize)
            {
                yield return list.GetRange(i, Math.Min(nSize, list.Count - i));
            }
        }
    }
}
