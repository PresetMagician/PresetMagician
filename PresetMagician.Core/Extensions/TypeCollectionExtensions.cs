using System.Collections.Generic;
using PresetMagician.Core.Models;

namespace PresetMagician.Core.Extensions
{
  
    public static class TypeCollectionExtensions
    {
        public static bool HasType<T> (this T collection, Type item) where T: ICollection<Type>
        {
            foreach (var item2 in collection)
            {
                if (item.TypeName == item2.TypeName && item.SubTypeName == item2.SubTypeName)
                {
                    return true;
                }

            }

            return false;
        }
        
        public static bool IsEqualTo<T> (this T collection, ICollection<Type> target) where T: ICollection<Type>
        {
            if (target == null)
            {
                return false;
            }

            if (target.Count != collection.Count)
            {
                return false;
            }

            foreach (var item in collection)
            {
                if (!target.HasType(item))
                {
                    return false;}
            }
            return true;
        }
    }
}