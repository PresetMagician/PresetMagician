using SharedModels.Collections;

namespace SharedModels.NewModels
{
    public class TypeCollection: EditableCollection<Type>
    {
        protected override void InsertItem(int index, Type item)
        {
            if (!HasType(item))
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

       
        
        public bool HasType(Type item)
        {
            foreach (var item2 in this)
            {
                if (item.TypeName == item2.TypeName && item.SubTypeName == item2.SubTypeName)
                {
                    return true;
                }

            }

            return false;
        }
        
        public bool IsEqualTo(TypeCollection target)
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
                if (!target.HasType(item))
                {
                    return false;}
            }
            return true;
        }
    }
}