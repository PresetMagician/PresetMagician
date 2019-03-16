using PresetMagician.Core.Collections;

namespace PresetMagician.Core.Models
{
    public class GlobalCharacteristicCollection: EditableCollection<Characteristic>
    {
        public GlobalCharacteristicCollection(): base(true)
        {
        }

        public Characteristic GetGlobalCharacteristic(Characteristic item)
        {
            foreach (var item2 in this)
            {
                if (item.CharacteristicName == item2.CharacteristicName)
                {
                    return item2;
                }
            }
            
            Characteristic.GlobalCharacteristics.Add(item);

            return item;
        }
    }
}