using System;
using System.Collections.Generic;
using System.Diagnostics;
using Catel.Collections;
using Catel.IoC;
using PresetMagician;
using PresetMagician.Core.Models;
using PresetMagician.Core.Services;
using Type = PresetMagician.Core.Models.Type;

namespace PresetMagicianScratchPad
{
    public class Program
    {
        private static Preset GetFreshPresetTestSubject()
        {
            
            var characteristic = new Characteristic();
            var type = new Type();
            var preset = new Preset();
            
            var originalMetadata = new PresetParserMetadata();
            originalMetadata.BankPath = "foo/bar";
            originalMetadata.Author = "horst";
            originalMetadata.Comment = "kein kommentar";
            originalMetadata.Types.Add(type);
            originalMetadata.Characteristics.Add(characteristic);
            originalMetadata.PresetName = "my preset";
            

            var plugin = new Plugin();
            preset.Plugin = plugin;
            preset.SetFromPresetParser(originalMetadata);

            plugin.Presets.Add(preset);

            preset.PresetHash = "foobar";

            preset.PresetSize = 1234;
            preset.PresetCompressedSize = 4567;

            preset.IsMetadataModified = false;
            preset.UpdateLastExportedMetadata();
            return preset;
        }
        [STAThread]
        static void Main(string[] args)
        {
            FrontendInitializer.RegisterTypes(ServiceLocator.Default);
            FrontendInitializer.Initialize(ServiceLocator.Default);
            var sw = new Stopwatch();

            GetFreshPresetTestSubject();

            var ps = ServiceLocator.Default.ResolveType<DataPersisterService>();
            var gs = ServiceLocator.Default.ResolveType<GlobalService>();

           

            sw.Start();

            ps.Load();
            Console.WriteLine("Plugin Load: " + sw.ElapsedMilliseconds + "ms");
            sw.Restart();

            foreach (var plugin in gs.Plugins)
            {
                ps.LoadPresetsForPlugin(plugin).Wait();
            }
            
            
            
            Console.WriteLine("Plugin Preset Load: " + sw.ElapsedMilliseconds + "ms");

        }
    }
}