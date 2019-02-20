using System.Collections.Generic;
using System.ComponentModel;
using Catel.Collections;
using TrackableEntities;
using TrackableEntities.Client;

namespace SharedModels.Collections
{
    public class TrackableFastObservableCollection<T> : FastObservableCollection<T>
        where T : class, ITrackable, INotifyPropertyChanged, IIdentifiable
    {
        private ChangeTrackingCollection<T> _backingTrackingCollection = new ChangeTrackingCollection<T>(true);
        private bool _beginTracking;
        
        public TrackableFastObservableCollection()
        {
        }

        public TrackableFastObservableCollection(IEnumerable<T> collection) : base(collection)
        {
            _backingTrackingCollection = new ChangeTrackingCollection<T>(collection);
            _beginTracking = true;
        }

        public ChangeTrackingCollection<T> GetTrackingList()
        {
            return _backingTrackingCollection;
        }

        protected override void ClearItems()
        {
            base.ClearItems();
            _backingTrackingCollection.Clear();
        }

        /// <summary>
        /// Inserts an item into the collection at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
        /// <param name="item">The object to insert.</param>
        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);
            if (_beginTracking)
            {
                _backingTrackingCollection.Insert(index, item);
            }
        }

        /// <summary>
        /// Moves the item at the specified index to a new location in the collection.
        /// </summary>
        /// <param name="oldIndex">The zero-based index specifying the location of the item to be moved.</param>
        /// <param name="newIndex">The zero-based index specifying the new location of the item.</param>
        protected override void MoveItem(int oldIndex, int newIndex)
        {
            base.MoveItem(oldIndex, newIndex);
            _backingTrackingCollection.Move(oldIndex, newIndex);
        }

        /// <summary>
        /// Removes the item at the specified index of the collection.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);
            _backingTrackingCollection.RemoveAt(index);
        }

        /// <summary>Replaces the element at the specified index.</summary>
        /// <param name="index">The zero-based index of the element to replace.</param>
        /// <param name="item">The new value for the element at the specified index.</param>
        protected override void SetItem(int index, T item)
        {
            base.SetItem(index, item);
            _backingTrackingCollection[index] = item;
        }

        public void MergeChanges()
        {
            _backingTrackingCollection.MergeChanges();
        }

        public ITrackingCollection<T> GetChanges()
        {
            return (ITrackingCollection<T>) (_backingTrackingCollection as ITrackingCollection).GetChanges();
        }
    }
}