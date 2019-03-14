using System.Collections.Generic;
using PresetMagician.Core.Models;

namespace PresetMagician.Core.Extensions
{
  
    public static class CharacteristicCollectionExtensions
    {
        public static bool HasCharacteristic<T> (this T collection, Characteristic item) where T: ICollection<Characteristic>
        {
            foreach (var item2 in collection)
            {
                if (item.CharacteristicName == item2.CharacteristicName)
                {
                    return true;
                }

            }

            return false;
        }
        
        public static bool IsEqualTo<T> (this T collection, ICollection<Characteristic> target) where T: ICollection<Characteristic>
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
                if (!target.HasCharacteristic(item))
                {
                    return false;}
            }
            return true;
        }
    }
}