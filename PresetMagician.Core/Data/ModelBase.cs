using System.Collections.Generic;
using System.ComponentModel;
using Catel.Data;
using Ceras;
using PresetMagician.Core.Collections;

namespace PresetMagician.Core.Data
{
    public abstract partial class ModelBase : INotifyPropertyChanged
    {
        public virtual event PropertyChangedEventHandler PropertyChanged;

        private readonly Dictionary<string, UserEditableChangeNotificationWrapper> _collectionTrackers =
            new Dictionary<string, UserEditableChangeNotificationWrapper>();

        public void OnPropertyChanged(string propertyName, object before, object after)
        {
            var x = new AdvancedPropertyChangedEventArgs(this, propertyName, before, after);
            OnPropertyChanged(x);
            PropertyChanged?.Invoke(this, x);
        }

        public ModelBase()
        {
          
        }

        private void ApplyTracker(string propertyName, object value)
        {
            if (value == null)
            {
                return;
            }

            if (value is IEditableCollection collection)
            {
                _collectionTrackers[propertyName] = new UserEditableChangeNotificationWrapper(collection, propertyName);
                _collectionTrackers[propertyName].CollectionChanged += OnWrappedCollectionChanged;
                _collectionTrackers[propertyName].CollectionItemPropertyChanged += OnCollectionItemPropertyChanged;
            }

            if (value is ModelBase value2)
            {
                _collectionTrackers[propertyName] = new UserEditableChangeNotificationWrapper(value2, propertyName);
                _collectionTrackers[propertyName].CollectionItemPropertyChanged += OnCollectionItemPropertyChanged;
            }
        }

        private void RemoveTracker(string propertyName)
        {
            if (_collectionTrackers.ContainsKey(propertyName))
            {
                _collectionTrackers[propertyName].Unsubscribe();
                _collectionTrackers[propertyName].CollectionChanged -= OnWrappedCollectionChanged;
                _collectionTrackers[propertyName].CollectionItemPropertyChanged -=
                    OnCollectionItemPropertyChanged;
            }
        }

        protected virtual void OnPropertyChanged(AdvancedPropertyChangedEventArgs e)
        {
            if (IsEditing && e.IsNewValueMeaningful && e.NewValue != e.OldValue)
            {
                // Throw away an old collection tracker
                RemoveTracker(e.PropertyName);
                ApplyTracker(e.PropertyName, e.NewValue);

                MarkPropertyModified(e.PropertyName);
            }
        }

        private void OnCollectionItemPropertyChanged(object sender, WrappedCollectionItemPropertyChangedEventArgs e)
        {
            MarkPropertyModified(e.SourceProperty);
        }

        private void OnWrappedCollectionChanged(object sender, WrappedCollectionChangedEventArgs e)
        {
            MarkPropertyModified(e.SourceProperty);
        }

        private void MarkPropertyModified(string propertyName)
        {
            if (ShouldTrackUserChanges())
            {
                RecalculateIsUserModified(propertyName);
            }
        }
    }
}