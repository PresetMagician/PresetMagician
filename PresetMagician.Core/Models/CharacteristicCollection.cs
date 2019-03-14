using System.Collections.Generic;
using PresetMagician.Core.Collections;
using PresetMagician.Core.Extensions;

namespace PresetMagician.Core.Models
{
    

    public class CharacteristicCollection: EditableCollection<Characteristic>
    {
        protected override void InsertItem(int index, Characteristic item)
        {
            if (!this.HasCharacteristic(item))
            {
                item = Characteristic.GlobalCharacteristics.GetGlobalCharacteristic(item);
                base.InsertItem(index, item);
            }
        }

        protected override void SetItem(int index, Characteristic item)
        {
            item = Characteristic.GlobalCharacteristics.GetGlobalCharacteristic(item);
            base.SetItem(index, item);
        }
        
       

    }
}