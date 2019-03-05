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
using FastMember;
using SharedModels.Collections;
using SharedModels.Data;
using SharedModels.Extensions;
using TrackableEntities;
using CatelModelBase = Catel.Data.ModelBase;

namespace SharedModels
{
    public abstract partial class TrackableModelBaseFoo : INotifyPropertyChanged
    {
        public virtual event PropertyChangedEventHandler PropertyChanged;

        public static bool IsLoadingFromDatabase = false;

        public static Dictionary<System.Type, List<string>> DatabaseProperties { get; } =
            new Dictionary<System.Type, List<string>>();

        [NotMapped] private Dictionary<string, CollectionChangeNotificationWrapper> CollectionTrackers =
            new Dictionary<string, CollectionChangeNotificationWrapper>();

        private static HashSet<string> _ignoredPropertiesForModifiedProperties =
            new HashSet<string>
            {
                nameof(ModifiedProperties), nameof(TrackingState), nameof(IsUserModified),
                nameof(IsEditing)
            };

        public virtual void OnPropertyChanged(string propertyName, object before, object after)
        {
            var x = new AdvancedPropertyChangedEventArgs(this, propertyName, before, after);
            this.OnPropertyChanged(x);
            PropertyChanged?.Invoke(this, x);
        }

        public TrackableModelBaseFoo()
        {
            var type = GetType();

            foreach (var propertyName in (from p in type.GetProperties()
                where p.PropertyType.ImplementsInterfaceEx<ITrackableCollection>() ||
                      p.PropertyType.HasBaseTypeEx(typeof(TrackableModelBase))
                select p.Name))
            {
                var value = PropertyHelper.GetPropertyValue(this, propertyName);
                ApplyTracker(propertyName, value);
            }
        }

        private void ApplyTracker(string propertyName, object value)
        {
            if (value == null)
            {
                return;
            }

            if (value is ITrackableCollection collection)
            {
                CollectionTrackers[propertyName] = new CollectionChangeNotificationWrapper(collection, propertyName);
                CollectionTrackers[propertyName].CollectionChanged += OnWrappedCollectionChanged;
                CollectionTrackers[propertyName].CollectionItemPropertyChanged += OnCollectionItemPropertyChanged;
            }

            if (value is TrackableModelBaseFoo value2)
            {
                CollectionTrackers[propertyName] = new CollectionChangeNotificationWrapper(value2, propertyName);
                CollectionTrackers[propertyName].CollectionItemPropertyChanged += OnCollectionItemPropertyChanged;
            }
        }


        protected void OnPropertyChanged(AdvancedPropertyChangedEventArgs e)
        {
            if (e.IsNewValueMeaningful && e.NewValue != e.OldValue)
            {
                // Throw away an old collection tracker
                if (CollectionTrackers.ContainsKey(e.PropertyName))
                {
                    CollectionTrackers[e.PropertyName].Unsubscribe();
                    CollectionTrackers[e.PropertyName].CollectionChanged -= OnWrappedCollectionChanged;
                    CollectionTrackers[e.PropertyName].CollectionItemPropertyChanged -=
                        OnCollectionItemPropertyChanged;
                }

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
            if (IsLoadingFromDatabase)
            {
                return;
            }

            if (ModifiedProperties == null)
            {
                ModifiedProperties = new HashSet<string>();
            }

            if (!ModifiedProperties.Contains(propertyName) &&
                !_ignoredPropertiesForModifiedProperties.Contains(propertyName))
            {
                ModifiedProperties.Add(propertyName);
            }

            if (ShouldTrackUserChanges())
            {
                RecalculateIsUserModified(propertyName);
            }
        }
    }
}