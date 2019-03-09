using SharedModels.Collections;

namespace SharedModels.NewModels
{
    public class CharacteristicCollection: EditableCollection<Characteristic>
    {
        protected override void InsertItem(int index, Characteristic item)
        {
            if (!HasCharacteristic(item))
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
        
       

        public bool HasCharacteristic(Characteristic item)
        {
            foreach (var item2 in this)
            {
                if (item.CharacteristicName == item2.CharacteristicName)
                {
                    return true;
                }

            }

            return false;
        }
        
        public bool IsEqualTo(CharacteristicCollection target)
        {
            if (target == null)
            {
                return false;
            }

            if (target.Count != Count)
            {
                return false;
            }

            foreach (var item in this)
            {
                if (!target.HasCharacteristic(item))
                {
                    return false;}
            }
            return true;
        }
    }
}