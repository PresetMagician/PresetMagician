using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FluentAssertions;
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
    public class TrackableCollectionTests
    {
        private readonly ITestOutputHelper output;
        private readonly DatabaseFixture _databaseFixture; 
        
        public TrackableCollectionTests(ITestOutputHelper output, DatabaseFixture databaseFixture)
        {
            this.output = output;
            _databaseFixture = databaseFixture;
        }
        
        [Fact]
        public void TestAdd()
        {
            var manager = _databaseFixture.GetEmptyManager();
            TrackableCollection<Plugin> plugins;
            
            using (var dbContext = manager.Create())
            {
                var test = dbContext.Plugins.ToList();

                plugins = new TrackableCollection<Plugin>(test);
            }

            var plugin = new Plugin();

            Assert.True(plugin.TrackingState == TrackingState.Unchanged);
            plugins.Add(plugin);
            //Assert.True(plugin.TrackingState == TrackingState.Added);

            plugins.GetChanges().Count.Should().Be(1);
            using (var dbContext = manager.Create())
            {
                dbContext.Database.Log = delegate(string s) { output.WriteLine(s);};
                dbContext.SyncChanges(plugins);
                dbContext.SaveChangesAsync();
                
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
            TrackableCollection<Plugin> plugins;

            var testPlugin = new Plugin();

            using (var dbContext = manager.Create())
            {
                dbContext.Database.Log = delegate(string s) { output.WriteLine(s);};
                var test = dbContext.Plugins.ToList();
                plugins = new TrackableCollection<Plugin>(test);
                plugins.Add(testPlugin);
                dbContext.SyncChanges(plugins);
                dbContext.SaveChanges();
                plugins.AcceptChanges();
            }


            Assert.True(testPlugin.TrackingState == TrackingState.Unchanged);

            plugins.Remove(testPlugin);

            //Assert.True(testPlugin.TrackingState == TrackingState.Deleted);

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
            TrackableCollection<Plugin> plugins;

            var testPlugin = new Plugin();

            using (var dbContext = manager.Create())
            {
                dbContext.Database.Log = delegate(string s) { output.WriteLine(s);};
                var test = dbContext.Plugins.ToList();
                plugins = new TrackableCollection<Plugin>(test);
                plugins.Add(testPlugin);
                dbContext.SyncChanges(plugins);
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
        
        [Fact]
        public void TestModify2()
        {
            var manager = _databaseFixture.GetEmptyManager();
            TrackableCollection<Plugin> plugins;

            var testPlugin = new Plugin();

            using (var dbContext = manager.Create())
            {
                dbContext.Database.Log = delegate(string s) { output.WriteLine(s);};
                var test = dbContext.Plugins.ToList();
                plugins = new TrackableCollection<Plugin>(test);
                plugins.Add(testPlugin);
                dbContext.SyncChanges(plugins);
                dbContext.SaveChanges();
                plugins.AcceptChanges();
            }


            Assert.True(testPlugin.TrackingState == TrackingState.Unchanged);

            plugins.Remove(testPlugin);
            plugins.Add(testPlugin);

            plugins.GetChanges().Should().BeEmpty("Removing and adding the same plugin should result in an unmodified list");
            
            plugins.Remove(testPlugin);
            testPlugin.PluginName = "test";
            plugins.Add(testPlugin);
            
            plugins.GetChanges().Count.Should().Be(1, "Modifying a plugin when removed from the list and added again should mark the plugin as modified");
            testPlugin.TrackingState.Should().Be(TrackingState.Modified);

            using (var dbContext = manager.Create())
            {
                dbContext.Database.Log = delegate(string s) { output.WriteLine(s);};
                dbContext.SyncChanges(plugins);
                dbContext.SaveChanges();
                plugins.AcceptChanges();
                
                testPlugin.TrackingState.Should().Be(TrackingState.Unchanged);
                Assert.Equal(plugins.Count, dbContext.Plugins.Count());
                
            }
            
           



        }
    }
}