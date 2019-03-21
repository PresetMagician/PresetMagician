using System.ComponentModel;

namespace PresetMagician.Core.Data
{
    public interface IUserEditable : IEditableObject
    {
        void BeginEdit(IUserEditable originatingObject);
        void CancelEdit(IUserEditable originatingObject);
        void EndEdit(IUserEditable originatingObject);
        bool IsUserModified { get; }
    }
}