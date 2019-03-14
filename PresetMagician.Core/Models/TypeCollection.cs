using PresetMagician.Core.Collections;
using PresetMagician.Core.Extensions;

namespace PresetMagician.Core.Models
{
    public class TypeCollection: EditableCollection<Type>
    {
        protected override void InsertItem(int index, Type item)
        {
            if (!this.HasType(item))
            {
                item = Type.GlobalTypes.GetGlobalType(item);
                base.InsertItem(index, item);
            }
        }

        protected override void SetItem(int index, Type item)
        {
            item = Type.GlobalTypes.GetGlobalType(item);
            
            base.SetItem(index, item);
        }

       
        
       
    }
}