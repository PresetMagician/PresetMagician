using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using Catel.Data;
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

            ObservableCollection<Plugin> plugins; 
            
            using (var dbContext = manager.Create())
            {
                dbContext.Migrate();
                ModelBase.DisablePropertyChangeNotifications = true;
                dbContext.Database.Log = delegate(string s) { output.WriteLine(s);};
                dbContext.Plugins.Include(plugin => plugin.AdditionalBankFiles).Include(plugin => plugin.PluginLocation).Load();
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                dbContext.Plugins.Include(plugin => plugin.AdditionalBankFiles).Include(plugin => plugin.PluginLocation).Include(plugin => plugin.Presets).Load();
                plugins = dbContext.Plugins.Local;
                
                stopWatch.Stop();
                output.WriteLine(stopWatch.Elapsed.ToString());
            }
            
            output.WriteLine(plugins.Count.ToString());
        }
    }
}