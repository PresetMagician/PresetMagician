using PresetMagician.Core.Collections;

namespace PresetMagician.Core.Models
{
    public class PresetCollection: EditableCollection<Preset>
    {
        protected override void InsertItem(int index, Preset item)
        {
            
            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, Preset item)
        {
            base.SetItem(index, item);
        }
    }
}