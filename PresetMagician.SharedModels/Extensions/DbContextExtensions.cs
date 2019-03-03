using System.Collections.Generic;
using System.Data.Entity;
using SharedModels.Collections;
using TrackableEntities;
using TrackableEntities.Common;
using TrackableEntities.EF6;
using TrackableEntities;
using TrackableEntities.Client;
using TrackableEntities.Common;
using System.ComponentModel;
using TrackableEntities.EF6;

namespace SharedModels.Extensions
{
    public static class DbContextExtensions
    {
        public static void SyncChanges<T> (this DbContext context, TrackableCollection<T> items) where T: class, ITrackable, IUserEditable, INotifyPropertyChanged, IIdentifiable 
        {
            context.ApplyChanges(items.GetChanges());
            
        }

    }
}