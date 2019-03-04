using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Catel.Collections;
using Catel.Data;
using TrackableEntities;
using TrackableEntities.Client;

namespace SharedModels.Collections
{
    public interface ITrackableCollection : INotifyCollectionChanged
    {
        event EventHandler<PropertyChangedEventArgs> ItemPropertyChanged;
    }

    public interface ITrackableCollection<T> where T : class, ITrackable, INotifyPropertyChanged, IIdentifiable
    {
        ITrackingCollection<T> GetChanges();
    }

    public class TrackableCollection<T> : FastObservableCollection<T>, ITrackableCollection, ITrackableCollection<T>,
        IUserEditable where T : class, ITrackable, IUserEditable, INotifyPropertyChanged, IIdentifiable
    {
        private readonly ChangeTrackingCollection<T> _backingTrackingCollection = new ChangeTrackingCollection<T>(true);

        private int _initialCount;
        private List<T> _backupValues;

        /// <summary>
        /// Defines if this model is in editing mode and causes IsUserModified to change
        /// </summary>
        public bool IsEditing { get; private set; }

        private bool _isCollectionItemUserModified;

        public bool IsUserModified { get; private set; }

        public TrackableCollection()
        {
            AutomaticallyDispatchChangeNotifications = false;
        }

        public TrackableCollection(IEnumerable<T> collection) : base(collection)
        {
            AutomaticallyDispatchChangeNotifications = false;
            _backingTrackingCollection = new ChangeTrackingCollection<T>(collection);
        }


        private void ChangeNotificationWrapperOnCollectionItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var adv = e as AdvancedPropertyChangedEventArgs;


            if (e.PropertyName == nameof(TrackableModelBase.IsUserModified) && adv.OldValue != adv.NewValue)
            {
                _isCollectionItemUserModified = (from i in Items where i.IsUserModified select i).Any();
                UpdateIsUserModifiedFlag();
            }

           
            ItemPropertyChanged?.Invoke(sender, e);
        }

        public ChangeTrackingCollection<T> GetTrackingList()
        {
            return _backingTrackingCollection;
        }

        protected override void ClearItems()
        {
            foreach (var item in Items)
            {
                item.CancelEdit();
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
            item.PropertyChanged += ChangeNotificationWrapperOnCollectionItemPropertyChanged;

            if (IsEditing)
            {
                item.BeginEdit();
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
                oldItem.PropertyChanged -= ChangeNotificationWrapperOnCollectionItemPropertyChanged;

                if (IsEditing)
                {
                    oldItem.CancelEdit();
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
                oldItem.PropertyChanged -= ChangeNotificationWrapperOnCollectionItemPropertyChanged;
                if (IsEditing)
                {
                    oldItem.CancelEdit();
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

            item.PropertyChanged += ChangeNotificationWrapperOnCollectionItemPropertyChanged;

            if (IsEditing)
            {
                item.BeginEdit();
            }


            if (!Contains(oldItem))
            {
                oldItem.PropertyChanged -= ChangeNotificationWrapperOnCollectionItemPropertyChanged;
                if (IsEditing)
                {
                    oldItem.CancelEdit();
                }
            }
        }

        public void MergeChanges()
        {
            _backingTrackingCollection.MergeChanges();
        }

        public ITrackingCollection<T> GetChanges()
        {
            _backingTrackingCollection.SynchronizeCollection(this);
            return (ITrackingCollection<T>) (_backingTrackingCollection as ITrackingCollection).GetChanges();
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
            _backupValues = new List<T>(this);
            IsUserModified = false;
            IsEditing = true;

            foreach (var i in this)
            {
                i.BeginEdit();
            }
        }

        public void EndEdit()
        {
            if (!IsEditing) return;

            foreach (var i in this)
            {
                i.EndEdit();
            }

            IsUserModified = false;
            IsEditing = false;
        }

        public void CancelEdit()
        {
            if (!IsEditing) return;


            using (SuspendChangeNotifications())
            {
                this.SynchronizeCollection(_backupValues);
            }

            foreach (var i in this)
            {
                i.CancelEdit();
            }

            EndEdit();
        }

        /// <summary>
        /// Occurs when the <see cref="CollectionChanged"/> event occurs on the target object.
        /// </summary>
        public event EventHandler<PropertyChangedEventArgs> ItemPropertyChanged;
    }
}