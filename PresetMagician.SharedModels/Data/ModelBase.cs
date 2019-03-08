using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using Catel.Data;
using Catel.Reflection;
using Ceras;
using FastMember;
using SharedModels.Collections;
using SharedModels.Data;
using SharedModels.Extensions;
using TrackableEntities;
using CatelModelBase = Catel.Data.ModelBase;

namespace SharedModels.Data
{
    public abstract partial class ModelBase : INotifyPropertyChanged
    {
        public virtual event PropertyChangedEventHandler PropertyChanged;

        [Exclude]
        public static bool IsLoadingFromDatabase = false;

        [NotMapped] private Dictionary<string, UserEditableChangeNotificationWrapper> CollectionTrackers =
            new Dictionary<string, UserEditableChangeNotificationWrapper>();

      

        public void OnPropertyChanged(string propertyName, object before, object after)
        {
            var x = new AdvancedPropertyChangedEventArgs(this, propertyName, before, after);
            OnPropertyChanged(x);
            PropertyChanged?.Invoke(this, x);
        }

        public ModelBase()
        {
            /*var type = GetType();

            foreach (var propertyName in (from p in type.GetProperties()
                where p.PropertyType.ImplementsInterfaceEx<ITrackableCollection>() ||
                      p.PropertyType.HasBaseTypeEx(typeof(TrackableModelBaseFoo))
                select p.Name))
            {
                var value = PropertyHelper.GetPropertyValue(this, propertyName);
                ApplyTracker(propertyName, value);
            }*/
        }

        private void ApplyTracker(string propertyName, object value)
        {
            if (value == null)
            {
                return;
            }

            if (value is IEditableCollection collection)
            {
                CollectionTrackers[propertyName] = new UserEditableChangeNotificationWrapper(collection, propertyName);
                CollectionTrackers[propertyName].CollectionChanged += OnWrappedCollectionChanged;
                CollectionTrackers[propertyName].CollectionItemPropertyChanged += OnCollectionItemPropertyChanged;
            }

            if (value is ModelBase value2)
            {
                CollectionTrackers[propertyName] = new UserEditableChangeNotificationWrapper(value2, propertyName);
                CollectionTrackers[propertyName].CollectionItemPropertyChanged += OnCollectionItemPropertyChanged;
            }
        }

        private void RemoveTracker(string propertyName)
        {
            if (CollectionTrackers.ContainsKey(propertyName))
            {
                CollectionTrackers[propertyName].Unsubscribe();
                CollectionTrackers[propertyName].CollectionChanged -= OnWrappedCollectionChanged;
                CollectionTrackers[propertyName].CollectionItemPropertyChanged -=
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