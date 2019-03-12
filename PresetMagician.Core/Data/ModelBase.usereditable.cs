using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Catel.Reflection;

namespace PresetMagician.Core.Data
{
    public abstract partial class ModelBase: IUserEditable
    {
        protected readonly Dictionary<string, object> BackupValues = new Dictionary<string, object>();
        private IUserEditable _originatingEditingObject;
        
        private bool ShouldTrackUserChanges()
        {
            return IsEditing;
        }

        public virtual void BeginEdit()
        {
            BeginEdit(null);
        }

        private List<string> GetBackupProperties()
        {
            return (from p in GetType().GetProperties() where p.CanRead && p.CanWrite select p.Name).ToList();
        }

        public virtual void BeginEdit(IUserEditable originatingObject)
        {
            if (IsEditing) return;
            IsUserModified = false;
            IsEditing = true;
            _originatingEditingObject = originatingObject;
            
            foreach (var propertyName in GetBackupProperties())
            {
                var value = PropertyHelper.GetPropertyValue(this, propertyName);
                BackupValues[propertyName] = value;

                if (EditableProperties.Contains(propertyName) && value is IUserEditable editable)
                {
                    ApplyTracker(propertyName, value);
                    editable.BeginEdit(this);
                }
            }
        }

        public virtual void EndEdit()
        {
            EndEdit(null);
        }
        

        public virtual void EndEdit(IUserEditable originatingObject)
        {
            if (!IsEditing || _originatingEditingObject != originatingObject) return;

            foreach (var propertyName in GetBackupProperties())
            {
                var value = PropertyHelper.GetPropertyValue(this, propertyName);
                
                if (EditableProperties.Contains(propertyName) && value is IUserEditable editable)
                {
                    RemoveTracker(propertyName);
                    editable.EndEdit(this);
                }
            }
            
            UserModifiedProperties.Clear();
            IsUserModified = false;
            
            IsEditing = false;
            _originatingEditingObject = null;

        }

        public virtual void CancelEdit()
        {
            CancelEdit(null);
        }

        public virtual void CancelEdit(IUserEditable originatingObject)
        {
            if (!IsEditing || _originatingEditingObject != originatingObject) return;
            
            IsEditing = false;
            UserModifiedProperties.Clear();
            IsUserModified = false;
            
            foreach (var propertyName in GetBackupProperties())
            {
                var value = BackupValues[propertyName];
                RemoveTracker(propertyName);
                
                if (!IsEqualToBackup(PropertyHelper.GetPropertyValue(this, propertyName), propertyName))
                {
                    PropertyHelper.SetPropertyValue(this, propertyName, value);
                }

                if (EditableProperties.Contains(propertyName) && value is IUserEditable editable)
                {
                    editable.CancelEdit(this);
                }
            }
            _originatingEditingObject = null;
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

        private bool IsEqualToBackup(object value, string propertyName)
        {
            if (value == null)
            {
                return BackupValues[propertyName] == null;
            }

            return value.Equals(BackupValues[propertyName]);
        }

        protected void CheckUserModified(string propertyName)
        {
            var value = PropertyHelper.GetPropertyValue(this, propertyName);

            if (!IsEqualToBackup(value, propertyName))
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
        public bool IsUserModified { get; private set; }

        /// <summary>
        /// Defines all properties which are editable by the user. If defined here, the property will cause the
        /// IsUserModified flag to be changed if in edit mode
        /// </summary>
        public virtual ICollection<string> EditableProperties { get; } = new HashSet<string>();
        
        /// <summary>
        /// Holds all properties which the user actually modified during an edit session. This set will be cleared after
        /// editing.
        /// </summary>
        public HashSet<string> UserModifiedProperties { get; } = new HashSet<string>();
        
        /// <summary>
        /// Defines if this model is in editing mode and causes IsUserModified to change
        /// </summary>
        public bool IsEditing { get; private set; }
        
        #endregion
    }
}