using System;
using System.Collections.Generic;
using System.Linq;

namespace PresetMagician.Extensions
{
    public static class CollectionExtensions
    {
        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            foreach (T obj in items)
                collection.Add(obj);
        }

        public static int RemoveAll<T>(this ICollection<T> collection, Func<T, bool> match)
        {
            IList<T> list = (IList<T>) collection.Where<T>(match).ToList<T>();
            foreach (T obj in (IEnumerable<T>) list)
                collection.Remove(obj);
            return list.Count;
        }
    }
}
