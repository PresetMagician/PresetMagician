using System.ComponentModel;
using System.Data.Entity;
using SharedModels.Collections;
using TrackableEntities;
using TrackableEntities.Client;
using TrackableEntities.EF6;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;

namespace SharedModels.Extensions
{
    public static class DbContextExtensions
    {
        public static void SyncChanges<T> (this DbContext context, TrackableCollection<T> items) where T: class, ITrackable, IUserEditable, INotifyPropertyChanged, IIdentifiable 
        {
            context.ApplyChanges(items.GetChanges());
        }
        
        public static IDictionary<string, PropertyInfo> GetTableColumns(this DbContext ctx, System.Type entityType)
        {
            ObjectContext octx = (ctx as IObjectContextAdapter).ObjectContext;
            EntityType storageEntityType = octx.MetadataWorkspace.GetItems(DataSpace.SSpace)
                .Where(x => x.BuiltInTypeKind == BuiltInTypeKind.EntityType).OfType<EntityType>()
                .Single(x => x.Name == entityType.Name);


            var columnNames = storageEntityType.Properties.ToDictionary(x => x.Name,
                y => y.MetadataProperties.FirstOrDefault(x => x.Name == "PreferredName")?.Value as string ?? y.Name);

            return storageEntityType.Properties.Select((elm, index) =>
                    new {elm.Name, Property = entityType.GetProperty(columnNames[elm.Name])})
                .ToDictionary(x => x.Name, x => x.Property);
        }

    }
}