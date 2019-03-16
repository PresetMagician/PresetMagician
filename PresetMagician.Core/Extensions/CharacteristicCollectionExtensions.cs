using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using Catel.Collections;
using PresetMagician.Core.Models;

namespace PresetMagician.Core.Extensions
{
  
    public static class CharacteristicCollectionExtensions
    {
        public static bool HasCharacteristic<T> (this T collection, Characteristic item) where T: ICollection<Characteristic>
        {
            foreach (var item2 in collection)
            {
                if (item.EffectiveCharacteristicName == item2.EffectiveCharacteristicName)
                {
                    return true;
                }

            }

            return false;
        }
        
        
        public static bool IsEqualTo<T> (this T collection, ICollection<Characteristic> target, bool excludeIgnored = false) where T: ICollection<Characteristic>
        {
            if (target == null)
            {
                return false;
            }
            
            ICollection<Characteristic> source = collection;
            
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
                if (!target.HasCharacteristic(item))
                {
                    return false;}
            }
            return true;
        }
    }
}