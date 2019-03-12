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
using MethodTimer;
using SharedModels.Collections;
using SharedModels.Data;
using SharedModels.Extensions;
using SharedModels.Interfaces;

namespace SharedModels
{
    public abstract partial class TrackableModelBase: IUserEditable
    {
        private readonly Dictionary<string, object> _backupValues = new Dictionary<string, object>();
        private IUserEditable _originatingEditingObject;
        
        private bool ShouldTrackUserChanges()
        {
            return IsEditing;
        }

        public void BeginEdit()
        {
            BeginEdit(null);
        }
        

        public void BeginEdit(IUserEditable originatingObject)
        {
            if (IsEditing) return;
            IsUserModified = false;
            IsEditing = true;
            _originatingEditingObject = originatingObject;
            
            foreach (var propertyName in from p in GetType().GetProperties() where p.CanRead && p.CanWrite  select p.Name)
            {

                var value = PropertyHelper.GetPropertyValue(this, propertyName);
                
                _backupValues[propertyName] = value;

                if (EditableProperties.Contains(propertyName) && value is IUserEditable editable)
                {
                    editable.BeginEdit(this);
                }
            }
        }

        public void EndEdit()
        {
            EndEdit(null);
        }
        

        public void EndEdit(IUserEditable originatingObject)
        {
            if (!IsEditing || _originatingEditingObject != originatingObject) return;

            foreach (var propertyName in EditableProperties)
            {
                var value = PropertyHelper.GetPropertyValue(this, propertyName);

                if (value is IUserEditable editable)
                {
                    editable.EndEdit(this);
                }
            }
            
            UserModifiedProperties.Clear();
            IsUserModified = false;
            
            IsEditing = false;
            _originatingEditingObject = null;

        }

        public void CancelEdit()
        {
            CancelEdit(null);
        }

        public void CancelEdit(IUserEditable originatingObject)
        {
            if (!IsEditing || _originatingEditingObject != originatingObject) return;
            
            IsEditing = false;
            UserModifiedProperties.Clear();
            IsUserModified = false;

            foreach (var propertyName in from p in GetType().GetProperties() where p.CanRead && p.CanWrite select p.Name)
            {
                var value = _backupValues[propertyName];
                
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
                return _backupValues[propertyName] == null;
            }

            return value.Equals(_backupValues[propertyName]);
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