using System;
using System.Diagnostics;
using System.IO;
using FluentAssertions;
using PresetMagician.Core.Exceptions;
using PresetMagician.Core.Models;
using PresetMagician.NKS;
using PresetMagician.VstHost.VST;
using Xunit;
using Xunit.Abstractions;

namespace PresetMagician.Tests.ModelTests
{
    public class ExportTest : BaseTest
    {
        public ExportTest(ITestOutputHelper output, DataFixture fixture) : base(output, fixture)
        {
        }

        [Fact]
        public void TestExport()
        {
            TestExportInternal(Plugin.PluginTypes.Instrument, ".nksf");
            TestExportInternal(Plugin.PluginTypes.Effect, ".nksfx");
        }

        private Plugin GetTestPlugin(Plugin.PluginTypes pluginType)
        {
            var plugin = new Plugin();
            plugin.VstPluginId = 1222222;
            plugin.PluginName = "SuperDuperPlugin";
            plugin.PluginVendor = "Drachenkatze";
            plugin.PluginType = pluginType;

            return plugin;
        }
        
        private void TestBankPath4Levels(string outputDir, Plugin.PluginTypes pluginType, string expectedExtension)
        {
            var plugin = GetTestPlugin(pluginType);
            
            var preset = new Preset();
            preset.Plugin = plugin;
            preset.PresetId = "a6d4d311-6856-4fe7-863b-d90b3240b085";
            preset.Metadata.PresetName = "Preset$!#";
            preset.Metadata.BankPath = "Factory/Default/Crazy/Felicia";
            
            var presetExportInfo = new PresetExportInfo(preset);
            presetExportInfo.UserContentDirectory = outputDir;
            
            presetExportInfo.FolderMode = PresetExportInfo.FolderExportMode.SINGLE_FOLDER;

            MakeRelative(presetExportInfo).Should().Be(@"SuperDuperPlugin\Preset$!#"+expectedExtension);
            MakePreviewRelative(presetExportInfo, true).Should().Be(@"SuperDuperPlugin\.previews\Preset$!#"+expectedExtension+".wav");
            MakePreviewRelative(presetExportInfo).Should().Be(@"SuperDuperPlugin\.previews\Preset$!#"+expectedExtension+".ogg");
            
            presetExportInfo.FolderMode = PresetExportInfo.FolderExportMode.ONE_LEVEL_FIRST_BANK;

            MakeRelative(presetExportInfo).Should().Be(@"SuperDuperPlugin\Factory\Preset$!#"+expectedExtension);
            MakePreviewRelative(presetExportInfo, true).Should().Be(@"SuperDuperPlugin\Factory\.previews\Preset$!#"+expectedExtension+".wav");
            MakePreviewRelative(presetExportInfo).Should().Be(@"SuperDuperPlugin\Factory\.previews\Preset$!#"+expectedExtension+".ogg");
            
            presetExportInfo.FolderMode = PresetExportInfo.FolderExportMode.ONE_LEVEL_LAST_BANK;

            MakeRelative(presetExportInfo).Should().Be(@"SuperDuperPlugin\Felicia\Preset$!#"+expectedExtension);
            MakePreviewRelative(presetExportInfo, true).Should().Be(@"SuperDuperPlugin\Felicia\.previews\Preset$!#"+expectedExtension+".wav");
            MakePreviewRelative(presetExportInfo).Should().Be(@"SuperDuperPlugin\Felicia\.previews\Preset$!#"+expectedExtension+".ogg");
            
            presetExportInfo.FolderMode = PresetExportInfo.FolderExportMode.SUBFOLDERS_TRIMMED;

            MakeRelative(presetExportInfo).Should().Be(@"SuperDuperPlugin\Factory\Default - Crazy - Felicia\Preset$!#"+expectedExtension);
            MakePreviewRelative(presetExportInfo, true).Should().Be(@"SuperDuperPlugin\Factory\Default - Crazy - Felicia\.previews\Preset$!#"+expectedExtension+".wav");
            MakePreviewRelative(presetExportInfo).Should().Be(@"SuperDuperPlugin\Factory\Default - Crazy - Felicia\.previews\Preset$!#"+expectedExtension+".ogg");
            
            presetExportInfo.FolderMode = PresetExportInfo.FolderExportMode.SUBFOLDERS;

            MakeRelative(presetExportInfo).Should().Be(@"SuperDuperPlugin\Factory\Default\Crazy\Felicia\Preset$!#"+expectedExtension);
            MakePreviewRelative(presetExportInfo, true).Should().Be(@"SuperDuperPlugin\Factory\Default\Crazy\Felicia\.previews\Preset$!#"+expectedExtension+".wav");
            MakePreviewRelative(presetExportInfo).Should().Be(@"SuperDuperPlugin\Factory\Default\Crazy\Felicia\.previews\Preset$!#"+expectedExtension+".ogg");
            
            

        }
        

        private void TestBankPath3Levels(string outputDir, Plugin.PluginTypes pluginType, string expectedExtension)
        {
            var plugin = GetTestPlugin(pluginType);
            
            var preset = new Preset();
            preset.Plugin = plugin;
            preset.PresetId = "a6d4d311-6856-4fe7-863b-d90b3240b085";
            preset.Metadata.PresetName = "Preset$!#";
            preset.Metadata.BankPath = "Factory/Default/Crazy";
            
            var presetExportInfo = new PresetExportInfo(preset);
            presetExportInfo.UserContentDirectory = outputDir;
            
            presetExportInfo.FolderMode = PresetExportInfo.FolderExportMode.SINGLE_FOLDER;

            MakeRelative(presetExportInfo).Should().Be(@"SuperDuperPlugin\Preset$!#"+expectedExtension);
            MakePreviewRelative(presetExportInfo, true).Should().Be(@"SuperDuperPlugin\.previews\Preset$!#"+expectedExtension+".wav");
            MakePreviewRelative(presetExportInfo).Should().Be(@"SuperDuperPlugin\.previews\Preset$!#"+expectedExtension+".ogg");
            
            presetExportInfo.FolderMode = PresetExportInfo.FolderExportMode.ONE_LEVEL_FIRST_BANK;

            MakeRelative(presetExportInfo).Should().Be(@"SuperDuperPlugin\Factory\Preset$!#"+expectedExtension);
            MakePreviewRelative(presetExportInfo, true).Should().Be(@"SuperDuperPlugin\Factory\.previews\Preset$!#"+expectedExtension+".wav");
            MakePreviewRelative(presetExportInfo).Should().Be(@"SuperDuperPlugin\Factory\.previews\Preset$!#"+expectedExtension+".ogg");
            
            presetExportInfo.FolderMode = PresetExportInfo.FolderExportMode.ONE_LEVEL_LAST_BANK;

            MakeRelative(presetExportInfo).Should().Be(@"SuperDuperPlugin\Crazy\Preset$!#"+expectedExtension);
            MakePreviewRelative(presetExportInfo, true).Should().Be(@"SuperDuperPlugin\Crazy\.previews\Preset$!#"+expectedExtension+".wav");
            MakePreviewRelative(presetExportInfo).Should().Be(@"SuperDuperPlugin\Crazy\.previews\Preset$!#"+expectedExtension+".ogg");
            
            presetExportInfo.FolderMode = PresetExportInfo.FolderExportMode.SUBFOLDERS_TRIMMED;

            MakeRelative(presetExportInfo).Should().Be(@"SuperDuperPlugin\Factory\Default - Crazy\Preset$!#"+expectedExtension);
            MakePreviewRelative(presetExportInfo, true).Should().Be(@"SuperDuperPlugin\Factory\Default - Crazy\.previews\Preset$!#"+expectedExtension+".wav");
            MakePreviewRelative(presetExportInfo).Should().Be(@"SuperDuperPlugin\Factory\Default - Crazy\.previews\Preset$!#"+expectedExtension+".ogg");
            
            presetExportInfo.FolderMode = PresetExportInfo.FolderExportMode.SUBFOLDERS;

            MakeRelative(presetExportInfo).Should().Be(@"SuperDuperPlugin\Factory\Default\Crazy\Preset$!#"+expectedExtension);
            MakePreviewRelative(presetExportInfo, true).Should().Be(@"SuperDuperPlugin\Factory\Default\Crazy\.previews\Preset$!#"+expectedExtension+".wav");
            MakePreviewRelative(presetExportInfo).Should().Be(@"SuperDuperPlugin\Factory\Default\Crazy\.previews\Preset$!#"+expectedExtension+".ogg");

        }
        
        private void TestBankPath2Levels(string outputDir, Plugin.PluginTypes pluginType, string expectedExtension)
        {
            var plugin = GetTestPlugin(pluginType);
            
            var preset = new Preset();
            preset.Plugin = plugin;
            preset.PresetId = "a6d4d311-6856-4fe7-863b-d90b3240b085";
            preset.Metadata.PresetName = "Preset$!#";
            preset.Metadata.BankPath = "Factory/Default";
            
            var presetExportInfo = new PresetExportInfo(preset);
            presetExportInfo.UserContentDirectory = outputDir;
            
            presetExportInfo.FolderMode = PresetExportInfo.FolderExportMode.SINGLE_FOLDER;

            MakeRelative(presetExportInfo).Should().Be(@"SuperDuperPlugin\Preset$!#"+expectedExtension);
            MakePreviewRelative(presetExportInfo, true).Should().Be(@"SuperDuperPlugin\.previews\Preset$!#"+expectedExtension+".wav");
            MakePreviewRelative(presetExportInfo).Should().Be(@"SuperDuperPlugin\.previews\Preset$!#"+expectedExtension+".ogg");
            
            presetExportInfo.FolderMode = PresetExportInfo.FolderExportMode.ONE_LEVEL_FIRST_BANK;

            MakeRelative(presetExportInfo).Should().Be(@"SuperDuperPlugin\Factory\Preset$!#"+expectedExtension);
            MakePreviewRelative(presetExportInfo, true).Should().Be(@"SuperDuperPlugin\Factory\.previews\Preset$!#"+expectedExtension+".wav");
            MakePreviewRelative(presetExportInfo).Should().Be(@"SuperDuperPlugin\Factory\.previews\Preset$!#"+expectedExtension+".ogg");
            
            presetExportInfo.FolderMode = PresetExportInfo.FolderExportMode.ONE_LEVEL_LAST_BANK;

            MakeRelative(presetExportInfo).Should().Be(@"SuperDuperPlugin\Default\Preset$!#"+expectedExtension);
            MakePreviewRelative(presetExportInfo, true).Should().Be(@"SuperDuperPlugin\Default\.previews\Preset$!#"+expectedExtension+".wav");
            MakePreviewRelative(presetExportInfo).Should().Be(@"SuperDuperPlugin\Default\.previews\Preset$!#"+expectedExtension+".ogg");
            
            presetExportInfo.FolderMode = PresetExportInfo.FolderExportMode.SUBFOLDERS_TRIMMED;

            MakeRelative(presetExportInfo).Should().Be(@"SuperDuperPlugin\Factory\Default\Preset$!#"+expectedExtension);
            MakePreviewRelative(presetExportInfo, true).Should().Be(@"SuperDuperPlugin\Factory\Default\.previews\Preset$!#"+expectedExtension+".wav");
            MakePreviewRelative(presetExportInfo).Should().Be(@"SuperDuperPlugin\Factory\Default\.previews\Preset$!#"+expectedExtension+".ogg");
            
            presetExportInfo.FolderMode = PresetExportInfo.FolderExportMode.SUBFOLDERS;

            MakeRelative(presetExportInfo).Should().Be(@"SuperDuperPlugin\Factory\Default\Preset$!#"+expectedExtension);
            MakePreviewRelative(presetExportInfo, true).Should().Be(@"SuperDuperPlugin\Factory\Default\.previews\Preset$!#"+expectedExtension+".wav");
            MakePreviewRelative(presetExportInfo).Should().Be(@"SuperDuperPlugin\Factory\Default\.previews\Preset$!#"+expectedExtension+".ogg");

        }
        
        private void TestBankPath1Levels(string outputDir, Plugin.PluginTypes pluginType, string expectedExtension)
        {
            var plugin = GetTestPlugin(pluginType);
            
            var preset = new Preset();
            preset.Plugin = plugin;
            preset.PresetId = "a6d4d311-6856-4fe7-863b-d90b3240b085";
            preset.Metadata.PresetName = "Preset$!#";
            preset.Metadata.BankPath = "Factory";
            
            var presetExportInfo = new PresetExportInfo(preset);
            presetExportInfo.UserContentDirectory = outputDir;
            
            presetExportInfo.FolderMode = PresetExportInfo.FolderExportMode.SINGLE_FOLDER;

            MakeRelative(presetExportInfo).Should().Be(@"SuperDuperPlugin\Preset$!#"+expectedExtension);
            MakePreviewRelative(presetExportInfo, true).Should().Be(@"SuperDuperPlugin\.previews\Preset$!#"+expectedExtension+".wav");
            MakePreviewRelative(presetExportInfo).Should().Be(@"SuperDuperPlugin\.previews\Preset$!#"+expectedExtension+".ogg");
            
            presetExportInfo.FolderMode = PresetExportInfo.FolderExportMode.ONE_LEVEL_FIRST_BANK;

            MakeRelative(presetExportInfo).Should().Be(@"SuperDuperPlugin\Factory\Preset$!#"+expectedExtension);
            MakePreviewRelative(presetExportInfo, true).Should().Be(@"SuperDuperPlugin\Factory\.previews\Preset$!#"+expectedExtension+".wav");
            MakePreviewRelative(presetExportInfo).Should().Be(@"SuperDuperPlugin\Factory\.previews\Preset$!#"+expectedExtension+".ogg");
            
            presetExportInfo.FolderMode = PresetExportInfo.FolderExportMode.ONE_LEVEL_LAST_BANK;

            MakeRelative(presetExportInfo).Should().Be(@"SuperDuperPlugin\Factory\Preset$!#"+expectedExtension);
            MakePreviewRelative(presetExportInfo, true).Should().Be(@"SuperDuperPlugin\Factory\.previews\Preset$!#"+expectedExtension+".wav");
            MakePreviewRelative(presetExportInfo).Should().Be(@"SuperDuperPlugin\Factory\.previews\Preset$!#"+expectedExtension+".ogg");
            
            presetExportInfo.FolderMode = PresetExportInfo.FolderExportMode.SUBFOLDERS_TRIMMED;

            MakeRelative(presetExportInfo).Should().Be(@"SuperDuperPlugin\Factory\Preset$!#"+expectedExtension);
            MakePreviewRelative(presetExportInfo, true).Should().Be(@"SuperDuperPlugin\Factory\.previews\Preset$!#"+expectedExtension+".wav");
            MakePreviewRelative(presetExportInfo).Should().Be(@"SuperDuperPlugin\Factory\.previews\Preset$!#"+expectedExtension+".ogg");
            
            presetExportInfo.FolderMode = PresetExportInfo.FolderExportMode.SUBFOLDERS;

            MakeRelative(presetExportInfo).Should().Be(@"SuperDuperPlugin\Factory\Preset$!#"+expectedExtension);
            MakePreviewRelative(presetExportInfo, true).Should().Be(@"SuperDuperPlugin\Factory\.previews\Preset$!#"+expectedExtension+".wav");
            MakePreviewRelative(presetExportInfo).Should().Be(@"SuperDuperPlugin\Factory\.previews\Preset$!#"+expectedExtension+".ogg");

        }
        private void TestExportInternal(Plugin.PluginTypes pluginType, string expectedExtension)
        {
            var outputDir = Path.Combine(Directory.GetCurrentDirectory(), @"TestData\PresetExport");
            Directory.CreateDirectory(outputDir);

            foreach (var file in Directory.EnumerateFiles(outputDir, "*", SearchOption.AllDirectories))
            {
                File.Delete(file);
            }

            var plugin = GetTestPlugin(pluginType);

            TestBankPath1Levels(outputDir, pluginType, expectedExtension);
            TestBankPath2Levels(outputDir, pluginType, expectedExtension);
            TestBankPath3Levels(outputDir, pluginType, expectedExtension);
            TestBankPath4Levels(outputDir, pluginType, expectedExtension);
            
            var existingDefaultPreset = new Preset();
            existingDefaultPreset.Plugin = plugin;
            existingDefaultPreset.PresetId = "a6d4d311-6856-ffff-863b-d90b3240b085";
            existingDefaultPreset.Metadata.PresetName = "Default";
            existingDefaultPreset.Metadata.BankPath = "Factory/Default/Crazy";

            var exporter = new NKSExport(null);
            var existingPresetExportInfo = new PresetExportInfo(existingDefaultPreset);
            existingPresetExportInfo.UserContentDirectory = outputDir;
            existingPresetExportInfo.FolderMode = PresetExportInfo.FolderExportMode.ONE_LEVEL_LAST_BANK;
            
            exporter.ExportNKSPreset(existingPresetExportInfo, new byte[] { 0xFF});

            existingDefaultPreset.PresetId ="ffffff11-6856-ffff-863b-d90b3240b085";
            
            existingPresetExportInfo = new PresetExportInfo(existingDefaultPreset);
            existingPresetExportInfo.UserContentDirectory = outputDir;
            existingPresetExportInfo.FolderMode = PresetExportInfo.FolderExportMode.ONE_LEVEL_LAST_BANK;

            exporter.Invoking(p => p.ExportNKSPreset(existingPresetExportInfo, new byte[] {0xFF})).Should()
                .Throw<NksExportException>();

            existingPresetExportInfo.OverwriteMode = PresetExportInfo.FileOverwriteMode.APPEND_GUID;

            MakeRelative(existingPresetExportInfo).Should()
                .Be(@"SuperDuperPlugin\Crazy\Default.ffffff11-6856-ffff-863b-d90b3240b085"+expectedExtension);
            
            existingPresetExportInfo.OverwriteMode = PresetExportInfo.FileOverwriteMode.FORCE_OVERWRITE;

            MakeRelative(existingPresetExportInfo).Should()
                .Be(@"SuperDuperPlugin\Crazy\Default"+expectedExtension);





        }

        private string MakeRelative(PresetExportInfo presetExportInfo)
        {
            return presetExportInfo.GetFullOutputPath().Replace(presetExportInfo.UserContentDirectory+@"\", "");
        }
        
        private string MakePreviewRelative(PresetExportInfo presetExportInfo, bool wav = false)
        {
            return presetExportInfo.GetPreviewFilename(wav).Replace(presetExportInfo.UserContentDirectory+@"\", "");
        }
    }


}