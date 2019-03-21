using PresetMagician.Core.Collections;

namespace PresetMagician.Core.Models
{
    public class TypeCollection : EditableCollection<Type>
    {
        public TypeCollection() : base(true)
        {
        }

        protected override void InsertItem(int index, Type item)
        {
            item = Type.GlobalTypes.GetGlobalType(item);
            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, Type item)
        {
            item = Type.GlobalTypes.GetGlobalType(item);

            base.SetItem(index, item);
        }
    }
}