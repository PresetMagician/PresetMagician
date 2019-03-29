using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Catel.IoC;
using PresetMagician;
using PresetMagician.Core.Models;
using PresetMagician.Core.Services;
using PresetMagician.VendorPresetParser.AIRMusicTechnology.Tfx;

namespace PresetMagicianScratchPad.Stuff
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public static class AirMusicTech
    {
        public static void TestLoadInPlugin(AirMusicTechTestSetup testSetup)
        {
            var directory = GetTestDirectory(testSetup);
            var files = Directory.EnumerateFiles(directory, "*.airtech.chunk", SearchOption.AllDirectories);
            TestLoadInPlugin(testSetup, files.ToList());
        }

        public static void TestLoadInPlugin(AirMusicTechTestSetup testSetup, List<string> files)
        {
            var serviceLocator = ServiceLocator.Default;

            FrontendInitializer.RegisterTypes(serviceLocator);
            FrontendInitializer.Initialize(serviceLocator);

            var remoteVstService = serviceLocator.ResolveType<RemoteVstService>();

            var plugin = new Plugin {PluginLocation = new PluginLocation {DllPath = testSetup.PluginDll}};

            var pluginInstance = remoteVstService.GetInteractivePluginInstance(plugin, false);

            pluginInstance.LoadPlugin().Wait();

            foreach (var file in files)
            {
                Debug.WriteLine($"Loading {file} into plugin");
                pluginInstance.SetChunk(File.ReadAllBytes(file), false);
            }
        }

        public static string GetTestDirectory(AirMusicTechTestSetup testSetup)
        {
            var pluginName = Path.GetFileNameWithoutExtension(testSetup.PluginDll);
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                $@"PresetMagician\ScratchPad\AirMusicTech\{pluginName}\");
        }

        public static void ConvertFiles(AirMusicTechTestSetup testSetup)
        {
            var outputDirectory = GetTestDirectory(testSetup);

            Directory.CreateDirectory(outputDirectory);


            foreach (var file in Directory.EnumerateFiles(testSetup.PresetDirectory, "*.tfx",
                SearchOption.AllDirectories))
            {
                Debug.WriteLine($"Converting {file}");
                var assemblyObj = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                    where assembly.FullName.StartsWith("PresetMagician.VendorPresetParser")
                    select assembly).First();
                var type = assemblyObj
                    .GetTypes().First(p =>
                        p.FullName == "PresetMagician.VendorPresetParser.AIRMusicTechnology.Tfx." +
                        testSetup.TfxParserType);

                var parser = (Tfx) Activator.CreateInstance(type);
                var fileName = file.Replace(testSetup.PresetDirectory + "\\", "");

                parser.Parse(testSetup.PresetDirectory, fileName);

                var outputFile = Path.Combine(outputDirectory, fileName.Replace(".tfx", ".airtech.chunk"));
                // ReSharper disable once AssignNullToNotNullAttribute
                Directory.CreateDirectory(Path.GetDirectoryName(outputFile));

                var data = parser.GetDataToWrite();
                File.WriteAllBytes(outputFile, data);
            }
        }

        // ReSharper disable once UnusedMember.Global
        public static readonly AirMusicTechTestSetup TESTSETUP_MINIGRAND = new AirMusicTechTestSetup
        {
            PluginDll = @"C:\Program Files\Steinberg\VstPlugins\MiniGrand_x64.dll",
            PresetDirectory = @"C:\Program Files (x86)\AIR Music Technology\Mini Grand\Presets",
            TfxParserType = "TfxMiniGrand"
        };
        
        // ReSharper disable once UnusedMember.Global
        public static readonly AirMusicTechTestSetup TESTSETUP_VELVET = new AirMusicTechTestSetup
        {
            PluginDll = @"C:\Program Files\Steinberg\VstPlugins\Velvet_x64.dll",
            PresetDirectory = @"C:\Program Files (x86)\AIR Music Technology\Velvet\Presets",
            TfxParserType = "TfxVelvet"
        };

        // ReSharper disable once UnusedMember.Global
        public static readonly AirMusicTechTestSetup TESTSETUP_HYBRID = new AirMusicTechTestSetup
        {
            PluginDll = @"C:\Program Files\Steinberg\VstPlugins\Hybrid_x64.dll",
            PresetDirectory = @"C:\Program Files (x86)\AIR Music Technology\Hybrid\Presets",
            TfxParserType = "TfxHybrid3"
        };
    }

    public class AirMusicTechTestSetup
    {
        public string PluginDll { get; set; }
        public string PresetDirectory { get; set; }
        public string TfxParserType { get; set; }
    }
}