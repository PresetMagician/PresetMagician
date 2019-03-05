using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FluentAssertions;
using PresetMagician.SharedModels;
using SharedModels;
using SharedModels.Collections;
using SharedModels.Extensions;
using TrackableEntities;
using TrackableEntities.Client;
using TrackableEntities.Common;
using TrackableEntities.EF6;
using Xunit;
using Xunit.Abstractions;
using  System.Data.Entity.Migrations;
using Catel.Collections;

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
            TrackableCollection<PluginLocation> foo;

            var testPlugin = new Plugin();
            testPlugin.PluginLocation = new PluginLocation();
            testPlugin.PluginLocation.IsPresent = true;
            testPlugin.Presets.Add(new Preset());
           

            using (var dbContext = manager.Create())
            {
                dbContext.Database.Log = delegate(string s) { output.WriteLine(s);};
                var test = dbContext.Plugins.ToList();
                var test2 = dbContext.PluginLocations.ToList();
                plugins = new TrackableCollection<Plugin>(test);
                foo = new TrackableCollection<PluginLocation>(test2);
              
                plugins.Add(testPlugin);
                var changes = plugins.GetChanges();

                foreach (var i in changes)
                {
                    if (!foo.Contains(i.PluginLocation))
                    {
                        foo.Add(i.PluginLocation);
                    }
                }
                
                dbContext.ApplyChanges(changes);
                dbContext.ApplyChanges(foo.GetChanges());
                
                //testPlugin.PluginLocation.TrackingState.Should().Be(TrackingState.Added);
                
                dbContext.SaveChanges();

                plugins.AcceptChanges();
                //testPlugin.AcceptChanges();
                
                Assert.Equal(plugins.Count, dbContext.Plugins.Count());
                Assert.Equal(foo.Count, dbContext.PluginLocations.Count());
            }

            testPlugin.Id.Should().NotBe(0);
            testPlugin.PluginLocation.Id.Should().NotBe(0);

            testPlugin.TrackingState.Should().Be(TrackingState.Unchanged);
            testPlugin.PluginLocation.TrackingState.Should().Be(TrackingState.Unchanged);

            plugins.Remove(testPlugin);
            plugins.Add(testPlugin);

            plugins.Remove(testPlugin);
           
            testPlugin.PluginName = "test";
            
            
            plugins.Add(testPlugin);
            
            testPlugin.TrackingState.Should().Be(TrackingState.Modified);
            testPlugin.PluginLocation = null;
            //testPlugin.ModifiedProperties.Remove("PluginLocation");
            using (var dbContext = manager.Create())
            {
                dbContext.Database.Log = delegate(string s) { output.WriteLine(s);};
                var changes = plugins.GetChanges();
                dbContext.ApplyChanges(foo.GetChanges());
                dbContext.ApplyChanges(changes);
                
                dbContext.SaveChanges();
                plugins.AcceptChanges();
                
                testPlugin.TrackingState.Should().Be(TrackingState.Unchanged);
                Assert.Equal(plugins.Count, dbContext.Plugins.Count());
                Assert.Equal(foo.Count, dbContext.PluginLocations.Count());
            }

            

            using (var dbContext = manager.Create())
            {
                var test = dbContext.Plugins.ToList();
                var test2 = dbContext.PluginLocations.ToList();
                plugins = new TrackableCollection<Plugin>(test);
                foo = new TrackableCollection<PluginLocation>(test2);
                
            }




        }
    }
}