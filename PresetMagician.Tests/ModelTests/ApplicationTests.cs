using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using Catel.Data;
using FluentAssertions;
using SharedModels;
using SharedModels.Collections;
using Xunit;
using Xunit.Abstractions;

namespace PresetMagician.Tests.ModelTests
{
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

            using (var dbContext = manager.Create())
            {
                //dbContext.Database.Log = delegate(string s) { output.WriteLine(s); };
                TrackableModelBase.IsLoadingFromDatabase = true;
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                var modesList = dbContext.Modes.ToList();

                output.WriteLine("modesList: "+stopWatch.ElapsedMilliseconds);
                stopWatch.ElapsedMilliseconds.Should().BeLessThan(1000);
                
                stopWatch.Restart();
                var typesList = dbContext.Types.ToList();
                output.WriteLine("typesList: "+stopWatch.ElapsedMilliseconds);
                
                stopWatch.Restart();
                var pluginList = dbContext.Plugins.Include(p => p.DefaultModes).Include(p => p.DefaultTypes).ToList();
                output.WriteLine("pluginList: "+stopWatch.ElapsedMilliseconds);
               
                stopWatch.Restart();
                var presetsList = dbContext.Presets.ToList();
                output.WriteLine("presetList: "+stopWatch.ElapsedMilliseconds);
                stopWatch.Restart();
                TrackableModelBase.IsLoadingFromDatabase = false;
            }
        }
    }
}