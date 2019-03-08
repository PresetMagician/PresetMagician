using System.ComponentModel;

namespace SharedModels.Data
{
    public interface IUserEditable: IEditableObject
    {
        void BeginEdit(IUserEditable originatingObject);
        void CancelEdit(IUserEditable originatingObject);
        void EndEdit(IUserEditable originatingObject);
        bool IsUserModified { get; }
        bool IsEditing { get; }
    }
}