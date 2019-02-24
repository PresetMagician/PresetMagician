using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using Catel.Data;
using FluentAssertions;
using SharedModels;
using SharedModels.Collections;
using TrackableEntities.Common;
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
                dbContext.Migrate();
                dbContext.Database.Log = delegate(string s) { output.WriteLine(s); };
                
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                var modesList = dbContext.Modes.ToList();

                stopWatch.ElapsedMilliseconds.Should().BeLessThan(1000);
                
                stopWatch.Restart();
                var typesList = dbContext.Types.ToList();
                stopWatch.ElapsedMilliseconds.Should().BeLessThan(1000);
                
                stopWatch.Restart();
                var pluginList = dbContext.Plugins.Include(p => p.DefaultModes).Include(p => p.DefaultTypes).ToList();
                stopWatch.ElapsedMilliseconds.Should().BeLessThan(3000);
               
                stopWatch.Restart();
                var presetsList = dbContext.Presets.Include(p => p.Modes).Include(p => p.Types).ToList();
                stopWatch.ElapsedMilliseconds.Should().BeLessThan(6000);
               
                
               
               


                var hive = (from preset in pluginList where preset.Id == 29 select preset).FirstOrDefault();
                output.WriteLine(hive.Presets.Count.ToString());
                stopWatch.Stop();
                output.WriteLine(stopWatch.Elapsed.ToString());
            }
        }
    }
}