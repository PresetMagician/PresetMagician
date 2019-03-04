using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.CompilerServices;
using Catel.Collections;
using Catel.Data;
using Catel.Reflection;
using Force.DeepCloner;
using SharedModels.Collections;
using SharedModels.Extensions;

namespace SharedModels
{
    public interface IUserEditable: IEditableObject
    {
        bool IsUserModified { get; }
        bool IsEditing { get; }
    }

    public abstract partial class TrackableModelBase: IUserEditable
    {
        private readonly Dictionary<string, object> _backupValues = new Dictionary<string, object>();
        
        private bool ShouldTrackUserChanges()
        {
            return IsEditing;
        }

        public void BeginEdit()
        {
            if (IsEditing) return;
            IsEditing = true;
            foreach (var propertyName in from p in GetType().GetProperties() where p.CanRead && p.CanWrite  select p.Name)
            {

                var value = PropertyHelper.GetPropertyValue(this, propertyName);
                
                _backupValues[propertyName] = value;

                if (EditableProperties.Contains(propertyName) && value is IUserEditable editable)
                {
                    editable.BeginEdit();
                }
            }
        }

        public void EndEdit()
        {
            if (!IsEditing) return;

            foreach (var propertyName in EditableProperties)
            {
                var value = PropertyHelper.GetPropertyValue(this, propertyName);

                if (value is IUserEditable editable)
                {
                    editable.EndEdit();
                }
            }
            
            UserModifiedProperties.Clear();
            IsUserModified = false;
            
            IsEditing = false;
             
        }

        public void CancelEdit()
        {
            if (!IsEditing) return;
            
            IsEditing = false;
            UserModifiedProperties.Clear();
            IsUserModified = false;

            foreach (var propertyName in from p in GetType().GetProperties() where p.CanRead && p.CanWrite select p.Name)
            {

                var value = _backupValues[propertyName];
               
                PropertyHelper.SetPropertyValue(this, propertyName, value);
              

                if (EditableProperties.Contains(propertyName) && value is IUserEditable editable)
                {
                    editable.CancelEdit();
                }
            }
            
          
        }

        protected void RecalculateIsUserModified(string propertyName)
        {
            if (!EditableProperties.Contains(propertyName))
            {
                return;
            }
            CheckUserModified(propertyName);
            IsUserModified = UserModifiedProperties.Count > 0;
        }

        protected void CheckUserModified(string propertyName)
        {
            var value = PropertyHelper.GetPropertyValue(this, propertyName);

            if (value != _backupValues[propertyName])
            {
                UserModifiedProperties.Add(propertyName);
            }
            else
            {
                if (value is IUserEditable editable)
                {
                    if (editable.IsUserModified)
                    {
                        UserModifiedProperties.Add(propertyName);
                    }
                    else
                    {
                        UserModifiedProperties.Remove(propertyName);
                    }
                }
                else
                {
                    UserModifiedProperties.Remove(propertyName);
                }
            }
        }
        
        #region Properties
        
        /// <summary>
        /// Defines if one or more properties were changed by the user during the current edit session. Will be reset
        /// after editing.
        /// </summary>
        [NotMapped]
        public bool IsUserModified { get; private set; }

        /// <summary>
        /// Defines all properties which are editable by the user. If defined here, the property will cause the
        /// IsUserModified flag to be changed if in edit mode
        /// </summary>
        [NotMapped]
        public virtual ICollection<string> EditableProperties { get; }= new HashSet<string>();
        
        /// <summary>
        /// Holds all properties which the user actually modified during an edit session. This set will be cleared after
        /// editing.
        /// </summary>
        [NotMapped]
        public HashSet<string> UserModifiedProperties { get; } = new HashSet<string>();
        
        /// <summary>
        /// Defines if this model is in editing mode and causes IsUserModified to change
        /// </summary>
        [NotMapped]
        public bool IsEditing { get; private set; }
        
        #endregion
    }
}