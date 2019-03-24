using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Catel.Collections;
using Catel.IoC;
using Catel.MVVM;
using Catel.Reflection;
using Drachenkatze.PresetMagician.NKSF.NKSF;
using FluentAssertions;
using Jacobi.Vst.Core;
using PresetMagician.Core.ApplicationTask;
using PresetMagician.Core.Commands.Plugin;
using PresetMagician.Core.Extensions;
using PresetMagician.Core.Models;
using PresetMagician.Core.Services;
using PresetMagician.Services;
using PresetMagician.Services.Interfaces;
using PresetMagician.Utils.Logger;
using PresetMagician.Utils.Progress;
using Xunit;
using Xunit.Abstractions;
using Type = PresetMagician.Core.Models.Type;

namespace PresetMagician.Tests.ModelTests
{
    public class PluginTest : BaseTest
    {
        public PluginTest(ITestOutputHelper output, DataFixture fixture) : base(output, fixture)
        {
        }

        private Dictionary<string, object> PropertiesWhichShouldBePersisted =
            new Dictionary<string, object>
            {
                {nameof(Plugin.PluginId), Guid.NewGuid().ToString()},
                {nameof(Plugin.IsEnabled), false},
                {nameof(Plugin.VstPluginId), 12345},
                {nameof(Plugin.PluginType), Plugin.PluginTypes.Instrument},
                {nameof(Plugin.LastKnownGoodDllPath), "lkgdllpath"},
                {nameof(Plugin.AudioPreviewPreDelay), 111},
                {nameof(Plugin.IsReported), true},
                {nameof(Plugin.DontReport), true},
                {nameof(Plugin.PluginVendor), "im da vendor"},
                {nameof(Plugin.IsSupported), true},
            };

        private List<string> PropertiesWhichShouldBeIgnored =
            new List<string>
            {
                nameof(Plugin.UserModifiedProperties),
                nameof(Plugin.LoadErrorMessage),
                nameof(Plugin.LoadError),
                nameof(Plugin.Logs),
                nameof(Plugin.RootBank),
                nameof(Plugin.DllPath),
                nameof(Plugin.DllFilename),
                nameof(Plugin.DllDirectory),
                nameof(Plugin.CanonicalDllFilename),
                nameof(Plugin.CanonicalDllDirectory),
                nameof(Plugin.IsPresent),
                nameof(Plugin.Logger),
                nameof(Plugin.IsUserModified),
                nameof(Plugin.IsEditing),
                nameof(Plugin.NativeInstrumentsResource),
                nameof(Plugin.PresetParserAudioPreviewPreDelay),
                nameof(Plugin.PluginTypeDescription),
                nameof(Plugin.NumPresets),
                nameof(Plugin.PresetParser),
                nameof(Plugin.RequiresMetadataScan),
                nameof(Plugin.PluginName),
                nameof(Plugin.HasMetadata)
            };

        private Dictionary<string, object> PluginLocationPropertiesWhichShouldBePersisted =
            new Dictionary<string, object>
            {
                {nameof(PluginLocation.PluginName), "yo im da plugin"},
                {nameof(PluginLocation.VstPluginId), 1234},
                {nameof(PluginLocation.DllHash), "hash"},
                {nameof(PluginLocation.PluginVendor), "plugin vendor"},
                {nameof(PluginLocation.PluginProduct), "plugin product"},
                {nameof(PluginLocation.LastModifiedDateTime), DateTime.Now},
                {nameof(PluginLocation.VendorVersion), "vendor version"},
                {nameof(PluginLocation.DllPath), "some path"},
                {nameof(PluginLocation.HasMetadata), true},
                {nameof(PluginLocation.PresetParserClassName), "NullPresetParser"},
                {nameof(PluginLocation.LastFailedAnalysisVersion), "0.5.9"}
            };

        private List<string> PluginLocationPropertiesWhichShouldBeIgnored =
            new List<string>
            {
                nameof(PluginLocation.IsPresent),
                nameof(PluginLocation.ShortTextRepresentation),
                nameof(PluginLocation.FullTextRepresentation),
                nameof(PluginLocation.IsUserModified),
                nameof(PluginLocation.IsEditing),
                nameof(PluginLocation.UserModifiedProperties),
                nameof(PluginLocation.PresetParser)
            };

        private Dictionary<string, object> PluginInfoPropertiesWhichShouldBePersisted =
            new Dictionary<string, object>
            {
                {nameof(VstPluginInfoSurrogate.Flags), VstPluginFlags.HasEditor},
                {nameof(VstPluginInfoSurrogate.InitialDelay), 1234},
                {nameof(VstPluginInfoSurrogate.ProgramCount), 100},
                {nameof(VstPluginInfoSurrogate.ParameterCount), 200},
                {nameof(VstPluginInfoSurrogate.AudioInputCount), 1},
                {nameof(VstPluginInfoSurrogate.AudioOutputCount), 2},
                {nameof(VstPluginInfoSurrogate.PluginVersion), 12},
                {nameof(VstPluginInfoSurrogate.PluginID), 2222}
            };

        private List<string> PluginInfoPropertiesWhichShouldBeIgnored =
            new List<string>
            {
                nameof(VstPluginInfoSurrogate.StringFlags)
            };

        private Dictionary<string, object> PresetPropertiesWhichShouldBePersisted =
            new Dictionary<string, object>
            {
                {nameof(Preset.PresetId), "im your friendly id"},
                {nameof(Preset.IsIgnored), true},
                {nameof(Preset.PresetSize), 1234},
                {nameof(Preset.PresetHash), "hash"},
                {nameof(Preset.PresetCompressedSize), 1222},
                {nameof(Preset.LastExported), DateTime.Now},
                {nameof(Preset.IsMetadataModified), true}
            };

        private Dictionary<string, object> PresetMetadataPropertiesWhichShouldBePersisted =
            new Dictionary<string, object>
            {
                {nameof(PresetMetadata.PresetName), "im your friendly preset"},
                {nameof(PresetMetadata.Author), "author"},
                {nameof(PresetMetadata.Comment), "comment"},
                {nameof(PresetMetadata.BankPath), "testbank/testbank1"},
            };

        private Dictionary<string, object> OriginalPresetMetadataPropertiesWhichShouldBePersisted =
            new Dictionary<string, object>
            {
                {nameof(PresetParserMetadata.PresetName), "im your friendly preset"},
                {nameof(PresetParserMetadata.Author), "author"},
                {nameof(PresetParserMetadata.Comment), "comment"},
                {nameof(PresetParserMetadata.BankPath), "testbank/testbank1"},
                {nameof(PresetParserMetadata.SourceFile), "this is my source file"}
            };

        private Dictionary<string, object> ExportedPresetMetadataPropertiesWhichShouldBePersisted =
            new Dictionary<string, object>
            {
                {nameof(ExportedPresetMetadata.PresetName), "im your friendly preset"},
                {nameof(ExportedPresetMetadata.Author), "author"},
                {nameof(ExportedPresetMetadata.Comment), "comment"},
                {nameof(ExportedPresetMetadata.BankPath), "testbank/testbank1"},
            };

        private List<string> PresetPropertiesWhichShouldBeIgnored =
            new List<string>
            {
                nameof(Preset.IsEditing),
                nameof(Preset.IsUserModified),
                nameof(Preset.PresetMetadataModifiedProperties),
                nameof(Preset.UserModifiedProperties),
                nameof(Preset.IsMetadataUserModified)
            };

        private Plugin InitializePluginToBeSaved()
        {
            var plugin = new Plugin();

            // Initialize the plugin with all properties
            foreach (var x in PropertiesWhichShouldBePersisted)
            {
                PropertyHelper.SetPropertyValue(plugin, x.Key, x.Value);
            }

            plugin.PluginCapabilities.Add(new PluginInfoItem("foo", "bar", "test"));

            plugin.PluginLocation = new PluginLocation();

            foreach (var x in PluginLocationPropertiesWhichShouldBePersisted)
            {
                PropertyHelper.SetPropertyValue(plugin.PluginLocation, x.Key, x.Value);
            }

            plugin.PluginInfo = new VstPluginInfoSurrogate();
            foreach (var x in PluginInfoPropertiesWhichShouldBePersisted)
            {
                PropertyHelper.SetPropertyValue(plugin.PluginInfo, x.Key, x.Value);
            }

            var preset = new Preset();
            foreach (var x in PresetPropertiesWhichShouldBePersisted)
            {
                PropertyHelper.SetPropertyValue(preset, x.Key, x.Value);
            }

            foreach (var x in PresetMetadataPropertiesWhichShouldBePersisted)
            {
                PropertyHelper.SetPropertyValue(preset.Metadata, x.Key, x.Value);
            }

            foreach (var x in OriginalPresetMetadataPropertiesWhichShouldBePersisted)
            {
                PropertyHelper.SetPropertyValue(preset.OriginalMetadata, x.Key, x.Value);
            }
            
            

            foreach (var x in ExportedPresetMetadataPropertiesWhichShouldBePersisted)
            {
                PropertyHelper.SetPropertyValue(preset.LastExportedMetadata, x.Key, x.Value);
            }

            preset.Metadata.UserOverwrittenProperties.Add("test");
            preset.PreviewNotePlayer = new PreviewNotePlayer();
            PreviewNotePlayer.PreviewNotePlayers.Add(preset.PreviewNotePlayer);
            preset.Plugin = plugin;
            preset.Metadata.Types.Add(new Type {TypeName = "bla"});
            preset.Metadata.Characteristics.Add(new Characteristic {CharacteristicName = "foo"});

            preset.LastExportedMetadata.Types.Add(new Type {TypeName = "ding"});
            preset.LastExportedMetadata.Characteristics.Add(new Characteristic {CharacteristicName = "ding2"});
            preset.LastExportedMetadata.PreviewNotePlayer = new PreviewNotePlayer();

            preset.OriginalMetadata.Types.Add(new Type {TypeName = "dingdong"});
            preset.OriginalMetadata.Characteristics.Add(new Characteristic {CharacteristicName = "dingdong22"});

            plugin.Presets.Add(preset);
            plugin.DefaultControllerAssignments = new ControllerAssignments();
            plugin.DefaultControllerAssignments.controllerAssignments.Add(new List<ControllerAssignment>
                {new ControllerAssignment {name = "test", id = 5, vflag = true, section = "test", autoname = true}});

            plugin.AdditionalBankFiles.Add(new BankFile {Path = "bla", BankName = "foo", ProgramRange = "test"});

            plugin.DefaultTypes.Add(new Type {TypeName = "kicks", SubTypeName = "hard"});
            plugin.DefaultCharacteristics.Add(new Characteristic {CharacteristicName = "char"});
            return plugin;
        }

        private void TestPluginLocation(PluginLocation originalLocation, PluginLocation savedLocation)
        {
            var testedProperties = new HashSet<string>();

            foreach (var x in PluginLocationPropertiesWhichShouldBePersisted)
            {
                var loadedValue = PropertyHelper.GetPropertyValue(savedLocation, x.Key);
                var originalValue = PropertyHelper.GetPropertyValue(originalLocation, x.Key);

                loadedValue.Should().BeEquivalentTo(originalValue,
                    $"Loading the property {x.Key} should be the same as the saved one");
                testedProperties.Add(x.Key);
            }

            testedProperties.AddRange(PluginLocationPropertiesWhichShouldBeIgnored);

            var allProperties =
                (from prop in typeof(PluginLocation).GetProperties() where !prop.GetType().IsEnum select prop.Name)
                .ToList();

            allProperties.Except(testedProperties).Should().BeEmpty("We want to test ALL TEH PROPERTIEZ");
        }

        private void TestPluginInfo(VstPluginInfoSurrogate originalInfo, VstPluginInfoSurrogate savedInfo)
        {
            var testedProperties = new HashSet<string>();

            foreach (var x in PluginInfoPropertiesWhichShouldBePersisted)
            {
                var loadedValue = PropertyHelper.GetPropertyValue(savedInfo, x.Key);
                var originalValue = PropertyHelper.GetPropertyValue(originalInfo, x.Key);

                loadedValue.Should().BeEquivalentTo(originalValue,
                    $"Loading the property {x.Key} should be the same as the saved one");
                testedProperties.Add(x.Key);
            }

            testedProperties.AddRange(PluginInfoPropertiesWhichShouldBeIgnored);

            var allProperties =
                (from prop in typeof(VstPluginInfoSurrogate).GetProperties() select prop.Name).ToList();

            allProperties.Except(testedProperties).Should().BeEmpty("We want to test ALL TEH PROPERTIEZ");
        }

        private void TestPluginLocations(Plugin originalPlugin, Plugin savedPlugin)
        {
            savedPlugin.PluginLocations.Count.Should().Be(originalPlugin.PluginLocations.Count);
        }

        private void TestPreset(Plugin originalPlugin, Plugin savedPlugin)
        {
            savedPlugin.Presets.Count.Should().Be(originalPlugin.Presets.Count);

            var testedProperties = new HashSet<string>();

            var originalPreset = originalPlugin.Presets.First();
            var savedPreset = savedPlugin.Presets.First();
            foreach (var x in PresetPropertiesWhichShouldBePersisted)
            {
                var loadedValue = PropertyHelper.GetPropertyValue(savedPreset, x.Key);
                var originalValue = PropertyHelper.GetPropertyValue(originalPreset, x.Key);

                loadedValue.Should().BeEquivalentTo(originalValue,
                    $"Loading the property {x.Key} should be the same as the saved one");
                testedProperties.Add(x.Key);
            }

            savedPreset.Metadata.UserOverwrittenProperties.Should()
                .BeEquivalentTo(originalPreset.Metadata.UserOverwrittenProperties);
            testedProperties.Add(nameof(Preset.Metadata.UserOverwrittenProperties));

            savedPreset.Plugin.Should().BeSameAs(savedPlugin);
            testedProperties.Add(nameof(Preset.Plugin));

            savedPreset.PresetBank.BankPath.Should().Be(savedPreset.Metadata.BankPath);
            testedProperties.Add(nameof(Preset.PresetBank));

            savedPreset.PreviewNotePlayer.PreviewNotePlayerId.Should()
                .Be(originalPreset.PreviewNotePlayer.PreviewNotePlayerId);
            testedProperties.Add(nameof(Preset.PreviewNotePlayer));
            testedProperties.Add(nameof(Preset.PreviewNotePlayerId));

            TestMetadata(originalPreset.Metadata, savedPreset.Metadata);
            testedProperties.Add(nameof(Preset.Metadata));
            TestOriginalMetadata(originalPreset.OriginalMetadata, savedPreset.OriginalMetadata);
            testedProperties.Add(nameof(Preset.OriginalMetadata));
            TestExportedMetadata(originalPreset.LastExportedMetadata, savedPreset.LastExportedMetadata);
            testedProperties.Add(nameof(Preset.LastExportedMetadata));

            testedProperties.AddRange(PresetPropertiesWhichShouldBeIgnored);

            var allProperties =
                (from prop in typeof(Preset).GetProperties() select prop.Name).ToList();

            allProperties.Except(testedProperties).Should().BeEmpty("We want to test ALL TEH PROPERTIEZ");
        }

        private void TestExportedMetadata(ExportedPresetMetadata originalPreset, ExportedPresetMetadata savedPreset)
        {
            var testedProperties = new HashSet<string>();

            foreach (var x in ExportedPresetMetadataPropertiesWhichShouldBePersisted)
            {
                var loadedValue = PropertyHelper.GetPropertyValue(savedPreset, x.Key);
                var originalValue = PropertyHelper.GetPropertyValue(originalPreset, x.Key);

                loadedValue.Should().BeEquivalentTo(originalValue,
                    $"Loading the property {x.Key} should be the same as the saved one");
                testedProperties.Add(x.Key);
            }

            savedPreset.Characteristics.IsEqualTo(originalPreset.Characteristics).Should().BeTrue();
            testedProperties.Add(nameof(ExportedPresetMetadata.Characteristics));

            savedPreset.Types.IsEqualTo(originalPreset.Types).Should().BeTrue();
            testedProperties.Add(nameof(ExportedPresetMetadata.Types));
            
            testedProperties.Add(nameof(ExportedPresetMetadata.SerializedCharacteristics));
            testedProperties.Add(nameof(ExportedPresetMetadata.SerializedTypes));

            var allProperties =
                (from prop in typeof(PresetMetadata).GetProperties() select prop.Name).ToList();

            allProperties.Except(testedProperties).Should().BeEmpty("We want to test ALL TEH PROPERTIEZ");
        }

        private void TestOriginalMetadata(PresetParserMetadata originalPreset, PresetParserMetadata savedPreset)
        {
            var testedProperties = new HashSet<string>();

            foreach (var x in OriginalPresetMetadataPropertiesWhichShouldBePersisted)
            {
                var loadedValue = PropertyHelper.GetPropertyValue(savedPreset, x.Key);
                var originalValue = PropertyHelper.GetPropertyValue(originalPreset, x.Key);

                loadedValue.Should().BeEquivalentTo(originalValue,
                    $"Loading the property {x.Key} should be the same as the saved one");
                testedProperties.Add(x.Key);
            }

            savedPreset.Characteristics.IsEqualTo(originalPreset.Characteristics).Should().BeTrue();
            testedProperties.Add(nameof(PresetParserMetadata.Characteristics));

            savedPreset.Types.IsEqualTo(originalPreset.Types).Should().BeTrue();
            testedProperties.Add(nameof(PresetParserMetadata.Types));
            
            testedProperties.Add(nameof(PresetParserMetadata.SerializedCharacteristics));
            testedProperties.Add(nameof(PresetParserMetadata.SerializedTypes));

            
            var allProperties =
                (from prop in typeof(PresetMetadata).GetProperties() select prop.Name).ToList();

            allProperties.Except(testedProperties).Should().BeEmpty("We want to test ALL TEH PROPERTIEZ");
        }

        private void TestMetadata(EditablePresetMetadata originalPreset, EditablePresetMetadata savedPreset)
        {
            var testedProperties = new HashSet<string>();

            foreach (var x in PresetMetadataPropertiesWhichShouldBePersisted)
            {
                var loadedValue = PropertyHelper.GetPropertyValue(savedPreset, x.Key);
                var originalValue = PropertyHelper.GetPropertyValue(originalPreset, x.Key);

                loadedValue.Should().BeEquivalentTo(originalValue,
                    $"Loading the property {x.Key} should be the same as the saved one");
                testedProperties.Add(x.Key);
            }

            savedPreset.Characteristics.IsEqualTo(originalPreset.Characteristics).Should().BeTrue();
            testedProperties.Add(nameof(PresetMetadata.Characteristics));

            savedPreset.Types.IsEqualTo(originalPreset.Types).Should().BeTrue();
            testedProperties.Add(nameof(PresetMetadata.Types));
            
            testedProperties.Add(nameof(PresetMetadata.SerializedCharacteristics));
            testedProperties.Add(nameof(PresetMetadata.SerializedTypes));

            var allProperties =
                (from prop in typeof(PresetMetadata).GetProperties() select prop.Name).ToList();

            allProperties.Except(testedProperties).Should().BeEmpty("We want to test ALL TEH PROPERTIEZ");
        }

        private void TestPluginCapabilities(Plugin originalPlugin, Plugin savedPlugin)
        {
            originalPlugin.PluginCapabilities.Count.Should().Be(savedPlugin.PluginCapabilities.Count);
            originalPlugin.PluginCapabilities.First().Should().BeEquivalentTo(savedPlugin.PluginCapabilities.First());
        }

        [Fact]
        public void TestPersistence()
        {
            var testedProperties = new HashSet<string>();
            var persister = Fixture.GetServiceLocator().ResolveType<DataPersisterService>();
            var plugin = InitializePluginToBeSaved();


            persister.SavePlugin(plugin);
            persister.SavePresetsForPlugin(plugin);
            var loadedPlugin = persister.LoadPlugin(persister.GetPluginStorageFile(plugin));
            persister.LoadPresetsForPlugin(loadedPlugin).Wait();

            foreach (var x in PropertiesWhichShouldBePersisted)
            {
                var loadedValue = PropertyHelper.GetPropertyValue(loadedPlugin, x.Key);
                var originalValue = PropertyHelper.GetPropertyValue(plugin, x.Key);

                loadedValue.Should().BeEquivalentTo(originalValue,
                    $"Loading the property {x.Key} should be the same as the saved one");
                testedProperties.Add(x.Key);
            }

            testedProperties.AddRange(PropertiesWhichShouldBeIgnored);

            TestPluginCapabilities(plugin, loadedPlugin);
            testedProperties.Add(nameof(Plugin.PluginCapabilities));

            TestPluginLocation(plugin.PluginLocation, loadedPlugin.PluginLocation);
            testedProperties.Add(nameof(Plugin.PluginLocation));

            TestPluginInfo(plugin.PluginInfo, loadedPlugin.PluginInfo);
            testedProperties.Add(nameof(Plugin.PluginInfo));

            TestPreset(plugin, loadedPlugin);
            testedProperties.Add(nameof(Plugin.Presets));

            loadedPlugin.DefaultControllerAssignments.Should().BeEquivalentTo(plugin.DefaultControllerAssignments);
            testedProperties.Add(nameof(Plugin.DefaultControllerAssignments));

            loadedPlugin.AdditionalBankFiles.Should().BeEquivalentTo(plugin.AdditionalBankFiles);
            testedProperties.Add(nameof(Plugin.AdditionalBankFiles));

            loadedPlugin.DefaultCharacteristics.Should().BeEquivalentTo(plugin.DefaultCharacteristics);
            testedProperties.Add(nameof(Plugin.DefaultCharacteristics));

            loadedPlugin.DefaultTypes.Should().BeEquivalentTo(plugin.DefaultTypes);
            testedProperties.Add(nameof(Plugin.DefaultTypes));

            TestPluginLocations(plugin, loadedPlugin);
            testedProperties.Add(nameof(Plugin.PluginLocations));

            var allProperties =
                (from prop in typeof(Plugin).GetProperties() where !prop.GetType().IsEnum select prop.Name).ToList();

            allProperties.Except(testedProperties).Should().BeEmpty("We want to test ALL TEH PROPERTIEZ");
        }

        [Fact]
        public void TestSetFromPresetParser()
        {
            var plugin = InitializePluginToBeSaved();
            var presetData = new PresetParserMetadata();
            presetData.Plugin = plugin;
            presetData.SourceFile = "foobar";
            presetData.BankPath = "foo/bar";

            var ps = Fixture.GetServiceLocator().ResolveType<PresetDataPersisterService>();
            ps.OpenDatabase().Wait();

            ps.PersistPreset(presetData, new byte[1]).Wait();

            var lastPreset = plugin.Presets.Last();
            lastPreset.OriginalMetadata.SourceFile.Should().Be("foobar");
            lastPreset.Plugin.Should().Be(plugin);
        }

        private ApplicationProgress CreateProgress()
        {
            var cts = new CancellationTokenSource();

            return new ApplicationProgress
            {
                Progress = new Progress<CountProgress>(),
                LogReporter = new LogReporter(new MiniMemoryLogger()),
                CancellationToken = cts.Token
            };
        }

        [WpfFact]
        public async Task TestPluginMoves()
        {
            var pluginTestFilename = "synister64.dll";
            var pluginTestDirectory = Path.Combine(Directory.GetCurrentDirectory(),
                @"TestData\VstPlugins");
            var pluginSourceDirectory = Path.Combine(Directory.GetCurrentDirectory(),
                @"Resources");
            var pluginSourcePath = Path.Combine(pluginSourceDirectory, pluginTestFilename);


            var pluginTestPath = Path.Combine(pluginTestDirectory, pluginTestFilename);

            Directory.CreateDirectory(pluginTestDirectory);
            File.Delete(pluginTestPath);
            File.Copy(pluginSourcePath, pluginTestPath);

            var pluginService = Fixture.GetServiceLocator().ResolveType<PluginService>();
            var globalService = Fixture.GetServiceLocator().ResolveType<GlobalService>();
            var applicationService = Fixture.GetServiceLocator().ResolveType<IApplicationService>();
            Fixture.StartPool();
            globalService.RuntimeConfiguration.VstDirectories.Add(new VstDirectory
                {Path = pluginTestDirectory, Active = true});


            await Fixture.GetServiceLocator().ResolveType<RefreshPluginsCommand>().ExecuteAsync();
            Fixture.StopPool();


            applicationService.GetApplicationOperationErrors().Should().BeEmpty();
            globalService.Plugins.Count.Should().Be(1);
        }
    }
}