using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FluentAssertions;
using PresetMagician.Core.Collections;
using PresetMagician.Core.Models;
using PresetMagician.Core.Services;
using PresetMagician.Legacy;
using PresetMagician.Legacy.Models;
using PresetMagician.Legacy.Services;
using PresetMagician.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;
using BankFile = PresetMagician.Core.Models.BankFile;
using OldPluginLocation = PresetMagician.Legacy.Models.PluginLocation;
using OldPreset = PresetMagician.Legacy.Models.Preset;
using OldPlugin = PresetMagician.Legacy.Models.Plugin;
using OldBankFile = PresetMagician.Legacy.Models.BankFile;
using Plugin = PresetMagician.Core.Models.Plugin;
using PluginLocation = PresetMagician.Core.Models.PluginLocation;
using Preset = PresetMagician.Core.Models.Preset;
using Type = PresetMagician.Legacy.Models.Type;

namespace PresetMagician.Tests.ModelTests
{
    public class MigrationTest
    {
        private readonly ITestOutputHelper _output;

        public MigrationTest(ITestOutputHelper output)
        {
            _output = output;
        }
        
        [Fact]
        public void TestMigration()
        {
            var testDb = Guid.NewGuid() + ".sqlite3";
            ApplicationDatabaseContext.DefaultDatabasePath = @"TestDatabases\"+testDb;
            DataPersisterService.DefaultPluginStoragePath = @"MigrationData\Plugins";
            PresetDataPersisterService.DefaultDatabasePath = @"MigrationData\PresetData.sqlite3";

            File.Delete(PresetDataPersisterService.DefaultDatabasePath);
            Directory.CreateDirectory(DataPersisterService.DefaultPluginStoragePath);
            File.Copy(@"Resources\PresetMagician.test.sqlite3", ApplicationDatabaseContext.DefaultDatabasePath);

            foreach (var file in Directory.EnumerateFiles(DataPersisterService.DefaultPluginStoragePath))
            {
                File.Delete(file);
            }
            var sw = new Stopwatch();
            sw.Start();
            var presetDataPersisterService = new PresetDataPersisterService();
            presetDataPersisterService.OpenDatabase().Wait();
            var service = new Ef6MigrationService(new DataPersisterService(new GlobalService()), presetDataPersisterService);
            service.LoadData();
            _output.WriteLine($"Loading took {sw.ElapsedMilliseconds}ms");

            foreach (var oldPlugin in service.OldPlugins)
            {
                var newPlugin = service.MigratePlugin(oldPlugin);

                ComparePlugin(oldPlugin, newPlugin);
                
            }
            
            service.MigratePlugins();

            File.Exists(ApplicationDatabaseContext.DefaultDatabasePath).Should().BeFalse();
            File.Exists(ApplicationDatabaseContext.DefaultDatabasePath+".old").Should().BeTrue();
            presetDataPersisterService.CloseDatabase().Wait();
        }
        
        private void ComparePlugin(OldPlugin oldPlugin, Plugin newPlugin)
        {
            var propertiesToCompare = new HashSet<string>
            {
               nameof(Plugin.IsEnabled),
               nameof(Plugin.PluginType),
               nameof(Plugin.LastKnownGoodDllPath),
               nameof(Plugin.AudioPreviewPreDelay),
               nameof(Plugin.IsReported),
               nameof(Plugin.DontReport),
               nameof(Plugin.PluginName),
               nameof(Plugin.PluginVendor),
               nameof(Plugin.IsSupported),
               nameof(Plugin.HasMetadata),
               nameof(Plugin.VstPluginId),
               nameof(Plugin.IsAnalyzed)
                
            };
            var comparer = new PropertyComparisonHelper(oldPlugin, newPlugin);
            comparer.CompareProperties(propertiesToCompare);
            
            ComparePluginLocation(oldPlugin.PluginLocation, newPlugin.PluginLocation);
            comparer.AddVisitedProperty(nameof(oldPlugin.PluginLocation));
            
            newPlugin.Presets.Count.Should().Be(oldPlugin.Presets.Count);

            foreach (var oldPreset in oldPlugin.Presets)
            {
                var newPreset = (from p in newPlugin.Presets where p.PresetId == oldPreset.PresetId select p)
                    .FirstOrDefault();
                ComparePreset(oldPreset, newPreset);
            }
            comparer.AddVisitedProperty(nameof(oldPlugin.Presets));
            
            CompareTypes(oldPlugin.DefaultTypes, newPlugin.DefaultTypes);
            comparer.AddVisitedProperty(nameof(oldPlugin.DefaultTypes));
            
            CompareModes(oldPlugin.DefaultModes, newPlugin.DefaultCharacteristics);

            newPlugin.PluginCapabilities.Should().BeEquivalentTo(oldPlugin.PluginCapabilities);
            comparer.AddVisitedProperty(nameof(oldPlugin.PluginCapabilities));
            
            newPlugin.PluginInfo.Should().BeEquivalentTo(oldPlugin.PluginInfo);
            comparer.AddVisitedProperty(nameof(oldPlugin.PluginInfo));

            CompareBankFiles(oldPlugin.AdditionalBankFiles, newPlugin.AdditionalBankFiles);
            comparer.AddVisitedProperty(nameof(oldPlugin.AdditionalBankFiles));
            
            
            newPlugin.DefaultControllerAssignments.Should().BeEquivalentTo(oldPlugin.DefaultControllerAssignments);
            comparer.AddVisitedProperty(nameof(oldPlugin.DefaultControllerAssignments));
            
            comparer.GetUnvisitedProperties().Should().BeEmpty();
            
            
        }
        
       

        private void ComparePluginLocation(OldPluginLocation oldLocation, PluginLocation newLocation)
        {
            if (oldLocation == null)
            {
                return;
            }

            newLocation.Should().NotBeNull();
            
            var propertiesToCompare = new HashSet<string>
            {
                nameof(OldPluginLocation.DllHash),
                nameof(OldPluginLocation.DllPath),
                nameof(OldPluginLocation.LastModifiedDateTime),
                nameof(OldPluginLocation.VendorVersion),
                nameof(OldPluginLocation.PluginVendor),
                nameof(OldPluginLocation.PluginName),
                nameof(OldPluginLocation.PluginProduct),
                nameof(OldPluginLocation.VstPluginId)
                
            };
            var comparer = new PropertyComparisonHelper(oldLocation, newLocation);
            comparer.CompareProperties(propertiesToCompare);

            comparer.GetUnvisitedProperties().Should().BeEmpty();
        }
        
        private void ComparePreset(OldPreset oldPreset, Preset newPreset)
        {
            newPreset.Should().NotBeNull();
            
            var propertiesToCompare = new HashSet<string>
            {
                nameof(OldPreset.PresetId),
                nameof(OldPreset.IsIgnored),
                nameof(OldPreset.LastExported),
                nameof(OldPreset.PresetSize),
                nameof(OldPreset.PresetCompressedSize),
                nameof(OldPreset.PresetHash),
            };
            
            var metadataPropertiesToCompare = new HashSet<string>
            {
                nameof(OldPreset.PresetName),
                nameof(OldPreset.Author),
                nameof(OldPreset.Comment),
                nameof(OldPreset.BankPath),
            };
            
            var originalMetadataPropertiesToCompare = new HashSet<string>
            {
                nameof(OldPreset.PresetName),
                nameof(OldPreset.Author),
                nameof(OldPreset.Comment),
                nameof(OldPreset.SourceFile),
                nameof(OldPreset.BankPath),
            };
            
            var comparer = new PropertyComparisonHelper(oldPreset, newPreset);
            comparer.CompareProperties(propertiesToCompare);
            comparer.AddVisitedProperty(nameof(OldPreset.Plugin));
            comparer.AddVisitedProperty(nameof(OldPreset.IsMetadataModified));
            comparer.GetUnvisitedProperties().Should().BeEmpty();
            
            comparer = new PropertyComparisonHelper(oldPreset, newPreset.Metadata);
            comparer.CompareProperties(metadataPropertiesToCompare);
            comparer.AddVisitedProperty(nameof(OldPreset.Types));
            comparer.GetUnvisitedProperties().Should().BeEmpty();
            
            comparer = new PropertyComparisonHelper(oldPreset, newPreset.OriginalMetadata);
            comparer.CompareProperties(originalMetadataPropertiesToCompare);
            comparer.AddVisitedProperty(nameof(OldPreset.Types));
            comparer.AddVisitedProperty(nameof(OldPreset.Plugin));
            comparer.GetUnvisitedProperties().Should().BeEmpty();
            
            CompareTypes(oldPreset.Types, newPreset.OriginalMetadata.Types);
            CompareTypes(oldPreset.Types, newPreset.Metadata.Types);
            
            CompareModes(oldPreset.Modes, newPreset.OriginalMetadata.Characteristics);
            CompareModes(oldPreset.Modes, newPreset.Metadata.Characteristics);
            

            
            
            
            
            
        }

        private void CompareTypes(ICollection<Type> oldTypes, ICollection<Core.Models.Type> newTypes)
        {
            newTypes.Count.Should().Be(oldTypes.Count);

            foreach (var oldType in oldTypes)
            {
                var found = (from t in newTypes
                    where t.TypeName == oldType.Name && t.SubTypeName == oldType.SubTypeName
                    select t).Any();

                found.Should().BeTrue();
            }
        }
        
        private void CompareModes(ICollection<Mode> oldModes, ICollection<Core.Models.Characteristic> newModes)
        {
            newModes.Count.Should().Be(oldModes.Count);

            foreach (var oldMode in oldModes)
            {
                var found = (from t in newModes
                    where t.CharacteristicName == oldMode.Name
                    select t).Any();

                found.Should().BeTrue();
            }
        }
        
        private void CompareBankFiles(ICollection<OldBankFile> oldBankFiles, EditableCollection<BankFile> newBankFiles)
        {
            newBankFiles.Count.Should().Be(oldBankFiles.Count);

            foreach (var oldBankFile in oldBankFiles)
            {
                var found = (from t in newBankFiles
                    where t.BankName == oldBankFile.BankName &&
                          t.ProgramRange == oldBankFile.ProgramRange &&
                                           t.Path == oldBankFile.Path
                    select t).Any();

                found.Should().BeTrue();
            }
        }
    }
}