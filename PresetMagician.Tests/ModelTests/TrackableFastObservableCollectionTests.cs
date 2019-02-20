using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SharedModels;
using SharedModels.Collections;
using SharedModels.Extensions;
using TrackableEntities;
using TrackableEntities.Client;
using TrackableEntities.Common;
using TrackableEntities.EF6;
using Xunit;
using Xunit.Abstractions;

namespace PresetMagician.Tests.ModelTests
{
    [Collection("Database collection")]
    public class TrackableFastObservableCollectionTests
    {
        private readonly ITestOutputHelper output;
        private readonly DatabaseFixture _databaseFixture; 
        
        public TrackableFastObservableCollectionTests(ITestOutputHelper output, DatabaseFixture databaseFixture)
        {
            this.output = output;
            _databaseFixture = databaseFixture;
        }
        
        [Fact]
        public void TestAdd()
        {
            var manager = _databaseFixture.GetEmptyManager();
            TrackableFastObservableCollection<Plugin> plugins;
            
            using (var dbContext = manager.Create())
            {
                var test = dbContext.Plugins.ToList();

                plugins = new TrackableFastObservableCollection<Plugin>(test);
            }

            var plugin = new Plugin();

            Assert.True(plugin.TrackingState == TrackingState.Unchanged);
            plugins.Add(plugin);
            Assert.True(plugin.TrackingState == TrackingState.Added);

            using (var dbContext = manager.Create())
            {
                dbContext.ApplyChanges(plugins);
                Debug.WriteLine(dbContext.SaveChangesAsync().Result);
                plugins.AcceptChanges();
            }

            Assert.True(plugin.TrackingState == TrackingState.Unchanged);

            using (var dbContext = manager.Create())
            {
                Assert.Equal(plugins.Count, dbContext.Plugins.Count());
            }
        }

        [Fact]
        public void TestRemove()
        {
            var manager = _databaseFixture.GetEmptyManager();
            TrackableFastObservableCollection<Plugin> plugins;

            var testPlugin = new Plugin();

            using (var dbContext = manager.Create())
            {
                dbContext.Database.Log = delegate(string s) { output.WriteLine(s);};
                var test = dbContext.Plugins.ToList();
                plugins = new TrackableFastObservableCollection<Plugin>(test);
                plugins.Add(testPlugin);
                dbContext.ApplyChanges(plugins);
                dbContext.SaveChanges();
                plugins.AcceptChanges();
            }


            Assert.True(testPlugin.TrackingState == TrackingState.Unchanged);

            plugins.Remove(testPlugin);

            Assert.True(testPlugin.TrackingState == TrackingState.Deleted);

            using (var dbContext = manager.Create())
            {
                
                dbContext.Database.Log = delegate(string s) { output.WriteLine(s);};
                output.WriteLine(dbContext.Entry(testPlugin).State.ToString());
                dbContext.SyncChanges(plugins);
                output.WriteLine(dbContext.Entry(testPlugin).State.ToString());
                
                dbContext.SaveChanges();
                plugins.MergeChanges();
            }

            Assert.True(testPlugin.TrackingState == TrackingState.Deleted);

            using (var dbContext = manager.Create())
            {
                Assert.Equal(plugins.Count, dbContext.Plugins.Count());
            }
        }
        
        [Fact]
        public void TestModify()
        {
            var manager = _databaseFixture.GetEmptyManager();
            TrackableFastObservableCollection<Plugin> plugins;

            var testPlugin = new Plugin();

            using (var dbContext = manager.Create())
            {
                dbContext.Database.Log = delegate(string s) { output.WriteLine(s);};
                var test = dbContext.Plugins.ToList();
                plugins = new TrackableFastObservableCollection<Plugin>(test);
                plugins.Add(testPlugin);
                dbContext.ApplyChanges(plugins);
                dbContext.SaveChanges();
                plugins.AcceptChanges();
            }


            Assert.True(testPlugin.TrackingState == TrackingState.Unchanged);

            testPlugin.PluginName = "foobar";

            Assert.True(testPlugin.TrackingState == TrackingState.Modified);

            using (var dbContext = manager.Create())
            {
                
                dbContext.Database.Log = delegate(string s) { output.WriteLine(s);};
                output.WriteLine(dbContext.Entry(testPlugin).State.ToString());
                dbContext.SyncChanges(plugins);
                output.WriteLine(dbContext.Entry(testPlugin).State.ToString());
                
                dbContext.SaveChanges();
                plugins.MergeChanges();
            }

          
        }
    }
}