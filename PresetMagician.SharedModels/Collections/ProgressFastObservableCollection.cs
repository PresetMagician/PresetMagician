using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Catel;
using Catel.Collections;

namespace PresetMagician.Collections
{
    public class ProgressFastObservableCollection<T> : FastObservableCollection<T>
    {
        private int _initialCount;

        public ProgressFastObservableCollection()
        {
        }

        public ProgressFastObservableCollection(IEnumerable<T> collection) : base(collection)
        {
        }

        public ProgressFastObservableCollection(IEnumerable collection) : base(collection)
        {
        }

        public virtual event EventHandler CollectionCountChanged;

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
            base.OnCollectionChanged(e);

            if (IsDirty && _initialCount != Count)
            {
                CollectionCountChanged.SafeInvoke(this);
            }
        }
    }
}