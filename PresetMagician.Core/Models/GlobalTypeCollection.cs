using PresetMagician.Core.Collections;

namespace PresetMagician.Core.Models
{
    public class GlobalTypeCollection: EditableCollection<Type>
    {
        public GlobalTypeCollection() : base(true)
        {
        }

        public Type GetGlobalType(Type item)
        {
            foreach (var item2 in this)
            {
                if (item.TypeName == item2.TypeName && item.SubTypeName == item2.SubTypeName)
                {
                    return item2;
                }
            }

            Type.GlobalTypes.Add(item);
            return item;
        }
    }
}