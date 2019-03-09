using SharedModels.Collections;

namespace SharedModels.NewModels
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