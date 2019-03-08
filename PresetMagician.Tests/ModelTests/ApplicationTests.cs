using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Catel.Collections;
using Ceras;
using FluentAssertions;
using MessagePack;
using SharedModels;
using SharedModels.Models;
using Xunit;
using Xunit.Abstractions;
using ZeroFormatter;
using Type = SharedModels.Type;

namespace PresetMagician.Tests.ModelTests
{
    public class Root
    {
        public List<Plugin> Plugins { get; set; }
        
        public List<Mode> Modes { get; set; }
        
        public List<Type> Types { get; set; }
        
        public List<PluginLocation> PluginLocations { get; set; }
        
    }
    [Collection("Database collection")]
    public class ApplicationTests
    {
        private readonly ITestOutputHelper output;
        private readonly DatabaseFixture _databaseFixture;

        public ApplicationTests(ITestOutputHelper output, DatabaseFixture databaseFixture)
        {
            this.output = output;
            _databaseFixture = databaseFixture;
        }

        [Fact]
        public void TestFoo()
        { 
            var manager = _databaseFixture.GetTestDataManager();

            var root = new Root();
            byte[] data;
            
            Stopwatch stopWatch = new Stopwatch();
            
            using (var dbContext = manager.Create())
            {
                //dbContext.Database.Log = delegate(string s) { output.WriteLine(s); };
                TrackableModelBase.IsLoadingFromDatabase = true;
                
                stopWatch.Start();
                root.Modes = dbContext.Modes.ToList();

                output.WriteLine("modesList: "+stopWatch.ElapsedMilliseconds);
                stopWatch.ElapsedMilliseconds.Should().BeLessThan(1000);
                
                stopWatch.Restart();
                root.Types = dbContext.Types.ToList();
                output.WriteLine("typesList: "+stopWatch.ElapsedMilliseconds);
                
                stopWatch.Restart();
                root.Plugins = dbContext.Plugins.Include(p => p.DefaultModes).Include(p => p.DefaultTypes).ToList();
                output.WriteLine("pluginList: "+stopWatch.ElapsedMilliseconds);
               
                stopWatch.Restart();
                var xjj = dbContext.Presets.ToList();
                output.WriteLine("presetList: "+stopWatch.ElapsedMilliseconds);
                stopWatch.Restart();
                TrackableModelBase.IsLoadingFromDatabase = false;
                
               
                var ceras = new CerasSerializer();

                data = ceras.Serialize(root);
                output.WriteLine("serializing: "+stopWatch.ElapsedMilliseconds);
                output.WriteLine("serializing bytes: "+data.Length);
                
            }
            
            GC.Collect(); GC.WaitForPendingFinalizers();
            
            output.WriteLine("mem usage now: "+Process.GetCurrentProcess().PrivateMemorySize64);
            
            var ceras2 = new CerasSerializer();
            stopWatch.Restart();
            TrackableModelBaseFoo.IsLoadingFromDatabase = true;
            var x = ceras2.Deserialize<Root>(data);
                GC.Collect(); GC.WaitForPendingFinalizers();
            output.WriteLine("deserializing: "+stopWatch.ElapsedMilliseconds);
                
            GC.Collect(); GC.WaitForPendingFinalizers();
            Process.GetCurrentProcess().Refresh();
            output.WriteLine("mem usage after: "+Process.GetCurrentProcess().PrivateMemorySize64);
            Thread.Sleep(100000);
        }
    }
}