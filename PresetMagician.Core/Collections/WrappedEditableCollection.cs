using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Catel.Collections;
using PresetMagician.Core.Data;

namespace PresetMagician.Core.Collections
{
    public class WrappedEditableCollection<TWrapped, TOriginal> : FastObservableCollection<TWrapped>
        where TWrapped : WrappedModel<TOriginal>, new()
        where TOriginal : class, IUserEditable, INotifyPropertyChanged
    {
        private EditableCollection<TOriginal> _wrappedList;
        private bool _internalSync;
        private readonly object _lock = new object();

        public WrappedEditableCollection(EditableCollection<TOriginal> wrappedList)
        {
            _wrappedList = wrappedList;
            _wrappedList.CollectionChanged += WrappedListOnCollectionChanged;

            SyncFromWrappedInternal();
        }

        private void SyncFromWrappedInternal()
        {
            lock (_lock)
            {
                _internalSync = true;
                var wrappedItemsCount = _wrappedList.Count;

                var obsoleteItems = Count - wrappedItemsCount;

                for (var i = 0; i < obsoleteItems; i++)
                {
                    RemoveAt(wrappedItemsCount);
                }

                for (var i = 0; i < wrappedItemsCount; i++)
                {
                    if (Count < i + 1)
                    {
                        var newItem = new TWrapped();
                        newItem.OriginalItem = _wrappedList[i];
                        Add(newItem);
                    }
                    else
                    {
                        if (this[i].OriginalItem != _wrappedList[i])
                        {
                            var newItem = new TWrapped();
                            newItem.OriginalItem = _wrappedList[i];
                            SetItem(i, newItem);
                        }
                    }
                }


                _internalSync = false;
            }
        }

        public TWrapped GetFromOriginal(TOriginal item)
        {
            return (from i in this where i.OriginalItem == item select i).FirstOrDefault();
        }

        private void WrappedListOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SyncFromWrappedInternal();
        }

        protected override void ClearItems()
        {
            _wrappedList.Clear();
            base.ClearItems();
        }

        /// <summary>
        /// Inserts an item into the collection at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
        /// <param name="item">The object to insert.</param>
        protected override void InsertItem(int index, TWrapped item)
        {
            if (!_internalSync)
            {
                _wrappedList.Insert(index, item.OriginalItem);
            }
            else
            {
                base.InsertItem(index, item);
            }
        }

        /// <summary>
        /// Moves the item at the specified index to a new location in the collection.
        /// </summary>
        /// <param name="oldIndex">The zero-based index specifying the location of the item to be moved.</param>
        /// <param name="newIndex">The zero-based index specifying the new location of the item.</param>
        protected override void MoveItem(int oldIndex, int newIndex)
        {
            if (!_internalSync)
            {
                _wrappedList.Move(oldIndex, newIndex);
            }

            throw new Exception("We should never arrive here");
        }


        /// <summary>
        /// Removes the item at the specified index of the collection.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        protected override void RemoveItem(int index)
        {
            if (!_internalSync)
            {
                _wrappedList.RemoveAt(index);
            }
            else
            {
                base.RemoveItem(index);
            }
        }

        /// <summary>Replaces the element at the specified index.</summary>
        /// <param name="index">The zero-based index of the element to replace.</param>
        /// <param name="item">The new value for the element at the specified index.</param>
        protected override void SetItem(int index, TWrapped item)
        {
            if (!_internalSync)
            {
                _wrappedList[index] = item.OriginalItem;
            }
            else
            {
                base.SetItem(index, item);
            }
        }
    }
}