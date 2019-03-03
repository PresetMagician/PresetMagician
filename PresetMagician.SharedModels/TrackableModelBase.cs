using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Catel.Data;
using SharedModels.Data;
using SharedModels.Extensions;
using CatelModelBase = Catel.Data.ModelBase;

namespace SharedModels
{
    public abstract partial class TrackableModelBase : ChildAwareModelBase, IEditableObject
    {
        
        [NotMapped]
        private Dictionary<string, CollectionChangeNotificationWrapper> CollectionTrackers = new Dictionary<string, CollectionChangeNotificationWrapper>();
        
        private static HashSet<string> _ignoredPropertiesForModifiedProperties =
            new HashSet<string> {nameof(ModifiedProperties), nameof(TrackingState), nameof(IsDirty), nameof(IsUserModified), nameof(IsEditing)};

        protected override void OnPropertyChanged(AdvancedPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            // Throw away an old collection tracker
            if (CollectionTrackers.ContainsKey(e.PropertyName) && !e.NewValue.Equals(e.OldValue))
            {
                if (e.OldValue.GetType().IsTrackableCollection() && CollectionTrackers.ContainsKey(e.PropertyName))
                {
                    CollectionTrackers[e.PropertyName].Unsubscribe();
                    CollectionTrackers[e.PropertyName].CollectionChanged -= OnWrappedCollectionChanged;
                    CollectionTrackers[e.PropertyName].CollectionItemPropertyChanged -= OnCollectionItemPropertyChanged;
                }
            }
            
            if (e.NewValue != null && e.NewValue.GetType().IsTrackableCollection())
            {
                CollectionTrackers[e.PropertyName] = new CollectionChangeNotificationWrapper(e.NewValue, e.PropertyName);
                CollectionTrackers[e.PropertyName].CollectionChanged += OnWrappedCollectionChanged;
                CollectionTrackers[e.PropertyName].CollectionItemPropertyChanged += OnCollectionItemPropertyChanged;
            }

            if (e.IsNewValueMeaningful && e.OldValue != e.NewValue)
            {
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
            if (ModifiedProperties == null)
            {
                ModifiedProperties = new List<string>();
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