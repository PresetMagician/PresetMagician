using PresetMagician.Core.Collections;

namespace PresetMagician.Core.Models
{
    public class CharacteristicCollection : EditableCollection<Characteristic>
    {
        public CharacteristicCollection() : base(true)
        {
        }

        protected override void InsertItem(int index, Characteristic item)
        {
            item = Characteristic.GlobalCharacteristics.GetGlobalCharacteristic(item);
            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, Characteristic item)
        {
            item = Characteristic.GlobalCharacteristics.GetGlobalCharacteristic(item);
            base.SetItem(index, item);
        }
    }
}