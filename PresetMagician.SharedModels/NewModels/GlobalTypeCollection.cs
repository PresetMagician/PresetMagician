using System.Data;
using SharedModels.Collections;

namespace SharedModels.NewModels
{
    public class GlobalTypeCollection: EditableCollection<Type>
    {
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