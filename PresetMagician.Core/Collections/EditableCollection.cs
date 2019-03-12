using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Catel.Collections;
using Catel.Data;
using PresetMagician.Core.Data;
using ModelBase = PresetMagician.Core.Data.ModelBase;

namespace PresetMagician.Core.Collections
{
    public interface IEditableCollection : INotifyPropertyChanged, INotifyCollectionChanged
    {
        event EventHandler<PropertyChangedEventArgs> ItemPropertyChanged;
    }

    public interface IEditableCollection<T>:  IList<T>, IEditableCollection, IUserEditable where T : class, INotifyPropertyChanged
    {

    }

    public class EditableCollection<T> : FastObservableCollection<T>, IEditableCollection<T> where T : class, IUserEditable, INotifyPropertyChanged
    {
        private IUserEditable _originatingEditingObject;

        private int _initialCount;
        private List<T> _backupValues;

        /// <summary>
        /// Defines if this model is in editing mode and causes IsUserModified to change
        /// </summary>
        public bool IsEditing { get; private set; }

        private bool _isCollectionItemUserModified;

        public bool IsUserModified { get; private set; }

        public EditableCollection()
        {
        }

        public EditableCollection(IEnumerable<T> collection) : base(collection)
        {
           
        }


        private void ChangeNotificationWrapperOnCollectionItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var adv = e as AdvancedPropertyChangedEventArgs;

            if (IsEditing && e.PropertyName == nameof(ModelBase.IsUserModified) && adv.OldValue != adv.NewValue)
            {
                _isCollectionItemUserModified = (from i in Items where i.IsUserModified select i).Any();
                UpdateIsUserModifiedFlag();
            }
           
            ItemPropertyChanged?.Invoke(sender, e);
        }

        protected override void ClearItems()
        {
            foreach (var item in Items)
            {
                item.CancelEdit(this);
            }

            base.ClearItems();
        }

        /// <summary>
        /// Inserts an item into the collection at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
        /// <param name="item">The object to insert.</param>
        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);
            
            if (IsEditing)
            {
                item.PropertyChanged += ChangeNotificationWrapperOnCollectionItemPropertyChanged;
                item.BeginEdit(this);
            }
        }

        /// <summary>
        /// Moves the item at the specified index to a new location in the collection.
        /// </summary>
        /// <param name="oldIndex">The zero-based index specifying the location of the item to be moved.</param>
        /// <param name="newIndex">The zero-based index specifying the new location of the item.</param>
        protected override void MoveItem(int oldIndex, int newIndex)
        {
            var oldItem = this[newIndex];
            base.MoveItem(oldIndex, newIndex);
            if (!Contains(oldItem))
            {
                

                if (IsEditing)
                {
                    oldItem.PropertyChanged -= ChangeNotificationWrapperOnCollectionItemPropertyChanged;
                    oldItem.CancelEdit(this);
                }
            }
        }
        
        

        /// <summary>
        /// Removes the item at the specified index of the collection.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        protected override void RemoveItem(int index)
        {
            var oldItem = this[index];
            base.RemoveItem(index);
            if (!Contains(oldItem))
            {
                
                if (IsEditing)
                {
                    oldItem.PropertyChanged -= ChangeNotificationWrapperOnCollectionItemPropertyChanged;
                    oldItem.CancelEdit(this);
                }
            }
        }

        /// <summary>Replaces the element at the specified index.</summary>
        /// <param name="index">The zero-based index of the element to replace.</param>
        /// <param name="item">The new value for the element at the specified index.</param>
        protected override void SetItem(int index, T item)
        {
            var oldItem = this[index];
            base.SetItem(index, item);

            

            if (IsEditing)
            {
                item.PropertyChanged += ChangeNotificationWrapperOnCollectionItemPropertyChanged;
                item.BeginEdit(this);
            }


            if (!Contains(oldItem))
            {
                
                if (IsEditing)
                {
                    oldItem.PropertyChanged -= ChangeNotificationWrapperOnCollectionItemPropertyChanged;
                    oldItem.CancelEdit(this);
                }
            }
        }

        public event EventHandler CollectionCountChanged;

        public new IDisposable SuspendChangeNotifications()
        {
            return SuspendChangeNotifications(SuspensionMode.None);
        }

        public new IDisposable SuspendChangeNotifications(SuspensionMode mode)
        {
            _initialCount = Count;
            return base.SuspendChangeNotifications(mode);
        }


        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (IsDirty && _initialCount != Count)
            {
                CollectionCountChanged?.Invoke(this, EventArgs.Empty);
            }

            if (IsEditing)
            {
                UpdateIsUserModifiedFlag();
            }

            base.OnCollectionChanged(e);
        }

        protected void UpdateIsUserModifiedFlag()
        {
            if (!IsEditing)
            {
                return;}
            IsUserModified = _isCollectionItemUserModified ||
                             _backupValues.Count != Count ||
                             _backupValues.Except(this).Any() || this.Except(_backupValues).Any();
        }


        public void BeginEdit()
        {
            BeginEdit(null);
        }
        
        public void BeginEdit(IUserEditable originatingObject)
        {
            _backupValues = new List<T>(this);
            IsUserModified = false;
            IsEditing = true;
            _originatingEditingObject = originatingObject;

            foreach (var i in this)
            {
                i.PropertyChanged += ChangeNotificationWrapperOnCollectionItemPropertyChanged;
                i.BeginEdit(this);
            }
        }

        public void EndEdit()
        {
            EndEdit(null);
        }
        public void EndEdit(IUserEditable originatingObject)
        {
            if (!IsEditing || _originatingEditingObject != originatingObject) return;

            foreach (var i in this)
            {
                i.PropertyChanged -= ChangeNotificationWrapperOnCollectionItemPropertyChanged;
                i.EndEdit(this);
            }

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

            foreach (var i in this)
            {
                i.PropertyChanged += ChangeNotificationWrapperOnCollectionItemPropertyChanged;
            }

            using (SuspendChangeNotifications())
            {
                this.SynchronizeCollection(_backupValues);
            }

            foreach (var i in this)
            {
                i.PropertyChanged += ChangeNotificationWrapperOnCollectionItemPropertyChanged;
                i.CancelEdit(this);
            }
            
            IsUserModified = false;
            IsEditing = false;
            _originatingEditingObject = null;

        }

        /// <summary>
        /// Occurs when the <see cref="CollectionChanged"/> event occurs on the target object.
        /// </summary>
        public event EventHandler<PropertyChangedEventArgs> ItemPropertyChanged;
    }
}