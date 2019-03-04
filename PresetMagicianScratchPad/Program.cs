using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using Catel.Collections;
using Catel.Data;
using PresetMagician.Tests;
using PresetMagician.Tests.TestEntities;
using SharedModels;
using SQLite.CodeFirst;
using Type = SharedModels.Type;

namespace PresetMagicianScratchPad
{
    public class Foo : TrackableModelBase
    {
        public override ICollection<string> EditableProperties { get; } = new List<string>
        {
            nameof(Name),
            nameof(Company)
        };

        public string Name { get; set; }
        public Company Company { get; set; }
    }

    public class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var x = new DatabaseFixture();
            var manager = x.GetTestDataManager();

            List<Mode> modesList;
            List<Type> typesList;
            List<Plugin> pluginList;
            Stopwatch stopWatch = new Stopwatch();
            List<Preset> presetsList;
            using (var dbContext = manager.Create())
            {
                dbContext.Migrate();
                //dbContext.Database.Log = delegate(string s) { output.WriteLine(s); };

                TrackableModelBase.IsLoadingFromDatabase = true;
                
                stopWatch.Start();
                modesList = dbContext.Modes.ToList();

                Debug.WriteLine("modesList: " + stopWatch.ElapsedMilliseconds);

                stopWatch.Restart();
                typesList = dbContext.Types.ToList();
                Debug.WriteLine("typesList: " + stopWatch.ElapsedMilliseconds);

                stopWatch.Restart();
                pluginList = dbContext.Plugins.Include(p => p.DefaultModes).Include(p => p.DefaultTypes).ToList();
                Debug.WriteLine("pluginList: " + stopWatch.ElapsedMilliseconds);

                stopWatch.Restart();
                presetsList = dbContext.Presets.Include(p => p.Modes).Include(p => p.Types).ToList();
                Debug.WriteLine("presetList: " + stopWatch.ElapsedMilliseconds);
            }

            GC.Collect(); GC.WaitForPendingFinalizers();



            var hive = (from preset in pluginList where preset.Id == 29 select preset).FirstOrDefault();
                stopWatch.Stop();
                Debug.WriteLine(stopWatch.Elapsed.ToString());
            }
        
    }
}