/*using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using PresetMagician.SharedModels.Collections;
using SharedModels;
using TrackableEntities;
using TrackableEntities.Client;
using TrackableEntities.Common;
using TrackableEntities.EF6;
using Xunit;
using Xunit.Abstractions;

namespace PresetMagician.Tests.ModelTests
{
   
    public class TrackableFastObservableCollectionTests
    {
        private readonly ITestOutputHelper output;

        public TrackableFastObservableCollectionTests(ITestOutputHelper output)
        {
            this.output = output;
        }
        
        [Fact]
        public void TestAdd()
        {
            TrackableFastObservableCollection<Plugin> plugins;
            ApplicationDatabaseContext.OverrideDbPath = @"Resources\PresetMagician.test.sqlite3";

            using (var dbContext = new ApplicationDatabaseContext())
            {
                var test = dbContext.Plugins.ToList();

                plugins = new TrackableFastObservableCollection<Plugin>(test);
            }

            Assert.True(plugins.First().TrackingState == TrackingState.Unchanged);

            var plugin = new Plugin();

            Assert.True(plugin.TrackingState == TrackingState.Unchanged);
            plugins.Add(plugin);
            Assert.True(plugin.TrackingState == TrackingState.Added);

            using (var dbContext = new ApplicationDatabaseContext())
            {
                dbContext.ApplyChanges(plugins);
                Debug.WriteLine(dbContext.SaveChangesAsync().Result);
                plugins.AcceptChanges();
            }

            Assert.True(plugin.TrackingState == TrackingState.Unchanged);

            using (var dbContext = new ApplicationDatabaseContext())
            {
                Assert.Equal(plugins.Count, dbContext.Plugins.Count());
            }
        }

        [Fact]
        public void TestRemove()
        {
            ChangeTrackingCollection<Plugin> plugins;
            //ApplicationDatabaseContext.OverrideDbPath = @"Resources\PresetMagician.test.sqlite3";
            ApplicationDatabaseContext.OverrideDbPath = @"Resources\PresetMagician.test.empty.sqlite3";
            if (File.Exists(ApplicationDatabaseContext.OverrideDbPath))
            {
                File.Delete(ApplicationDatabaseContext.OverrideDbPath);
            }

            var testPlugin = new Plugin();

            using (var dbContext = new ApplicationDatabaseContext())
            {
                dbContext.Database.Log = delegate(string s) { output.WriteLine(s);};
                var test = dbContext.Plugins.ToList();
                plugins = new ChangeTrackingCollection<Plugin>(test);
                plugins.Add(testPlugin);
                dbContext.ApplyChanges(plugins);
                dbContext.SaveChanges();
                plugins.AcceptChanges();
            }


            Assert.True(testPlugin.TrackingState == TrackingState.Unchanged);

            plugins.Remove(testPlugin);

            Assert.True(testPlugin.TrackingState == TrackingState.Deleted);

            using (var dbContext = new ApplicationDatabaseContext())
            {
                
                dbContext.Database.Log = delegate(string s) { output.WriteLine(s);};
                Debug.WriteLine(dbContext.Entry(testPlugin).State.ToString());
                dbContext.ApplyChanges(plugins);
                dbContext.ApplyChanges(testPlugin);
                Debug.WriteLine(dbContext.Entry(testPlugin).State.ToString());
                
                dbContext.SaveChanges();
                plugins.MergeChanges();
            }

            Assert.True(testPlugin.TrackingState == TrackingState.Deleted);

            using (var dbContext = new ApplicationDatabaseContext())
            {
                Assert.Equal(plugins.Count, dbContext.Plugins.Count());
            }
        }
    }
}*/