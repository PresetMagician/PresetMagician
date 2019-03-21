using System.Collections.Generic;
using System.Linq;
using PresetMagician.Core.Models;

namespace PresetMagician.Core.Extensions
{
    public static class TypeCollectionExtensions
    {
        public static bool HasType<T>(this T collection, Type item) where T : ICollection<Type>
        {
            foreach (var item2 in collection)
            {
                if (item.EffectiveTypeName == item2.EffectiveTypeName &&
                    item.EffectiveSubTypeName == item2.EffectiveSubTypeName)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsEqualTo<T>(this T collection, ICollection<Type> target, bool excludeIgnored = false)
            where T : ICollection<Type>
        {
            if (target == null)
            {
                return false;
            }

            ICollection<Type> source = collection;

            if (excludeIgnored)
            {
                source = (from t in collection where !t.IsIgnored select t).ToList();
            }

            if (target.Count != source.Count)
            {
                return false;
            }

            foreach (var item in source)
            {
                if (!target.HasType(item))
                {
                    return false;
                }
            }

            return true;
        }
    }
}