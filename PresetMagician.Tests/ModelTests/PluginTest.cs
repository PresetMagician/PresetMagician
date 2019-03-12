using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Catel.Collections;
using Catel.Reflection;
using Drachenkatze.PresetMagician.NKSF.NKSF;
using FluentAssertions;
using Jacobi.Vst.Core;
using PresetMagician.Core.Models;
using PresetMagician.Core.Services;
using Xunit;
using PluginInfoItem = PresetMagician.Core.Models.PluginInfoItem;
using Type = PresetMagician.Core.Models.Type;
using VstPluginInfoSurrogate = PresetMagician.Core.Models.VstPluginInfoSurrogate;

namespace PresetMagician.Tests.ModelTests
{
    public class PluginTest
    {
        private Dictionary<string, object> PropertiesWhichShouldBePersisted =
            new Dictionary<string, object>
            {
                {nameof(Plugin.PluginName), "bla"},
                {nameof(Plugin.PluginId), Guid.NewGuid().ToString()},
                {nameof(Plugin.IsEnabled), false},
                {nameof(Plugin.VstPluginId), 12345},
                {nameof(Plugin.PluginType), Plugin.PluginTypes.Instrument},
                {nameof(Plugin.LastKnownGoodDllPath), "lkgdllpath"},
                {nameof(Plugin.AudioPreviewPreDelay), 111},
                {nameof(Plugin.IsReported), true},
                {nameof(Plugin.DontReport), true},
                {nameof(Plugin.PluginVendor), "im da vendor"},
                {nameof(Plugin.IsAnalyzed), true},
                {nameof(Plugin.HasMetadata), true},
                {nameof(Plugin.IsSupported), true},
                {nameof(Plugin.LastFailedAnalysisVersion), "0.5.9"}
            };

        private List<string> PropertiesWhichShouldBeIgnored =
            new List<string>
            {
                nameof(Plugin.EditableProperties),
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
                nameof(Plugin.RequiresMetadataScan)
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
                {nameof(PluginLocation.DllPath), "some path"}
            };

        private List<string> PluginLocationPropertiesWhichShouldBeIgnored =
            new List<string>
            {
                nameof(PluginLocation.IsPresent),
                nameof(PluginLocation.ShortTextRepresentation),
                nameof(PluginLocation.FullTextRepresentation),
                nameof(PluginLocation.IsUserModified),
                nameof(PluginLocation.EditableProperties),
                nameof(PluginLocation.IsEditing),
                nameof(PluginLocation.UserModifiedProperties)
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
                {nameof(Preset.PresetName), "im your friendly preset"},
                {nameof(Preset.PresetId), "im your friendly id"},
                {nameof(Preset.IsIgnored), true},
                {nameof(Preset.Author), "author"},
                {nameof(Preset.Comment), "comment"},
                {nameof(Preset.BankPath), "testbank/testbank1"},
                {nameof(Preset.SourceFile), "sourcefile"},
                {nameof(Preset.PresetSize), 1234},
                {nameof(Preset.PresetHash), "hash"},
                {nameof(Preset.LastExportedPresetHash), "lasthash"},
                {nameof(Preset.PresetCompressedSize), 1222},
                {nameof(Preset.LastExported), DateTime.Now},
                {nameof(Preset.IsMetadataModified), true}
            };

        private List<string> PresetPropertiesWhichShouldBeIgnored =
            new List<string>
            {
                nameof(Preset.IsEditing),
                nameof(Preset.IsUserModified),
                nameof(Preset.EditableProperties),
                nameof(Preset.UserModifiedProperties),
                nameof(Preset.ChangedSinceLastExport)
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

            preset.UserOverwrittenProperties.Add("test");
            preset.PreviewNotePlayer = new PreviewNotePlayer();
            preset.Plugin = plugin;
            preset.Types.Add(new Type {TypeName = "bla"});
            preset.Characteristics.Add(new Characteristic {CharacteristicName = "foo"});

            plugin.Presets.Add(preset);
            plugin.DefaultControllerAssignments = new ControllerAssignments();
            plugin.DefaultControllerAssignments.controllerAssignments.Add(new List<ControllerAssignment>
                {new ControllerAssignment {name = "test", id = 5, vflag = true, section = "test", autoname = true}});

            plugin.AdditionalBankFiles.Add(new BankFile { Path = "bla", BankName = "foo", ProgramRange = "test"});

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

            savedPreset.UserOverwrittenProperties.Should().BeEquivalentTo(originalPreset.UserOverwrittenProperties);
            testedProperties.Add(nameof(Preset.UserOverwrittenProperties));

            savedPreset.Plugin.Should().BeSameAs(savedPlugin);
            testedProperties.Add(nameof(Preset.Plugin));

            savedPreset.PresetBank.BankPath.Should().Be(savedPreset.BankPath);
            testedProperties.Add(nameof(Preset.PresetBank));

            savedPreset.PreviewNotePlayer.PreviewNotePlayerId.Should()
                .Be(originalPreset.PreviewNotePlayer.PreviewNotePlayerId);
            testedProperties.Add(nameof(Preset.PreviewNotePlayer));

            savedPreset.Characteristics.IsEqualTo(originalPreset.Characteristics).Should().BeTrue();
            testedProperties.Add(nameof(Preset.Characteristics));

            savedPreset.Types.IsEqualTo(originalPreset.Types).Should().BeTrue();
            testedProperties.Add(nameof(Preset.Types));

            testedProperties.AddRange(PresetPropertiesWhichShouldBeIgnored);

            var allProperties =
                (from prop in typeof(Preset).GetProperties() select prop.Name).ToList();

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
            DataPersisterService.DefaultPluginStoragePath = Directory.GetCurrentDirectory();

            var testedProperties = new HashSet<string>();
            var persister = new DataPersisterService();
            var plugin = InitializePluginToBeSaved();


            persister.SavePlugin(plugin);
            var loadedPlugin = persister.LoadPlugin(persister.GetPluginStorageFile(plugin));

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
        public void TestPluginMoves()
        {
        }
    }
}