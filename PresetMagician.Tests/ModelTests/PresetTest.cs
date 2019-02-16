using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Catel.Collections;
using Catel.Data;
using FluentAssertions;
using PresetMagician.Tests.Extensions;
using SharedModels;
using SQLitePCL;
using Xunit;
using Xunit.Abstractions;
using Type = SharedModels.Type;

namespace PresetMagician.Tests.ModelTests
{
    public class PresetTest
    {
        private List<string> ExpectedDatabaseProperties = new List<string>
        {
            "PresetId",
            "PluginId", "VstPluginId", "IsMetadataModified", "LastExported", "BankPath", "PresetSize",
            "PresetCompressedSize", "PresetName", "PreviewNoteNumber", "Author", "Comment",
            "SourceFile", "PresetHash", "LastExportedPresetHash", "IsIgnored", "UserModifiedMetadata"
        };

        private Dictionary<string, object> PropertiesWhichShouldNotModifyIsMetadataModified =
            new Dictionary<string, object>
            {
                {"LastExportedPresetHash", "bla"},
                {"LastExported", DateTime.Now}
            };

        private Dictionary<string, object> PropertiesWhichShouldModifyIsMetadataModified =
            new Dictionary<string, object>
            {
                {"PresetName", "bla"},
                {"PreviewNoteNumber", 15},
                {"Author", "bla"},
                {"Comment", "bla"},
                {"BankPath", "bla/foo"},
                {"PresetHash", "dingdong"}
            };

        private HashSet<string> PropertiesWhichAreNotRelevantToMetadataModified =
            new HashSet<string>
            {
                "HasErrors",
                "HasWarnings",
                "IsDirty",
                "IsReadOnly",
                "SourceFile",
                "IsIgnored",
                "PluginId",
                "Plugin",
                "SerializedUserModifiedMetadata",
                "ChangedSinceLastExport",
                "PresetId",
                "VstPluginId",
                "IsMetadataModified"
            };

        private readonly ITestOutputHelper output;

        public PresetTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        private Preset GetFreshPresetTestSubject()
        {
            var mode = new Mode();
            var type = new Type();
            var preset = new Preset();
            preset.BankPath = "foo/bar";
            var plugin = new Plugin();
            preset.Plugin = plugin;
            plugin.Presets.Add(preset);
            preset.Types.Add(type);
            preset.Modes.Add(mode);
            preset.LastExportedPresetHash = "foobar";
            preset.PresetHash = "foobar";
            preset.PresetName = "my preset";
            preset.IsMetadataModified = false;
            plugin.ClearIsDirtyOnAllChilds();
            plugin.ClearDirtyFlag();

            return preset;
        }

        [Fact]
        public void TestPresetDatabaseFields()
        {
            using (var context = ApplicationDatabaseContext.Create())
            {
                var actualDbProperties =
                    (from prop in context.GetTableColumns(typeof(Preset)) select prop.Key).ToList();

                var missingTestProperties = actualDbProperties.Except(ExpectedDatabaseProperties);
                missingTestProperties.Should().BeEmpty("Missing in unit test");

                var missingModelProperties = ExpectedDatabaseProperties.Except(actualDbProperties);
                missingModelProperties.Should().BeEmpty("Missing in model");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Fact]
        public void TestPresetModifiedField()
        {
            var preset = GetFreshPresetTestSubject();

            preset.ChangedSinceLastExport.Should().BeFalse("It is unmodified");
            preset.IsMetadataModified.Should().BeFalse("It is unmodified");
        }

        [Fact]
        public void TestIsMetadataModified()
        {
            var testedProperties = new HashSet<string>();

            foreach (var notModifyProperty in PropertiesWhichShouldNotModifyIsMetadataModified)
            {
                var preset2 = GetFreshPresetTestSubject();
                (preset2 as IEditableObject).BeginEdit();

                preset2.GetType().GetProperty(notModifyProperty.Key).SetValue(preset2, notModifyProperty.Value);

                preset2.IsMetadataModified.Should()
                    .BeFalse($"{notModifyProperty.Key} should not modify IsMetadataModified");

                testedProperties.Add(notModifyProperty.Key);
            }

            foreach (var modifyProperty in PropertiesWhichShouldModifyIsMetadataModified)
            {
                var preset2 = GetFreshPresetTestSubject();
                (preset2 as IEditableObject).BeginEdit();
                preset2.GetType().GetProperty(modifyProperty.Key).SetValue(preset2, modifyProperty.Value);

                preset2.IsMetadataModified.Should()
                    .BeTrue($"{modifyProperty.Key} should modify IsMetadataModified when edit mode is on");

                if (Preset.PresetParserMetadataProperties.Contains(modifyProperty.Key))
                {
                    preset2.UserModifiedProperties.Should().Contain(modifyProperty.Key,
                        $"UserModifiedProperties should contain {modifyProperty.Key} because it's a PresetParserMetadataProperties which is now user modified");
                }

                testedProperties.Add(modifyProperty.Key);
            }

            foreach (var modifyProperty in PropertiesWhichShouldModifyIsMetadataModified)
            {
                var preset2 = GetFreshPresetTestSubject();
                preset2.GetType().GetProperty(modifyProperty.Key).SetValue(preset2, modifyProperty.Value);

                preset2.IsMetadataModified.Should()
                    .BeFalse($"{modifyProperty.Key} should not modify IsMetadataModified if edit mode is off");

                testedProperties.Add(modifyProperty.Key);
            }

            TestModesCollection(testedProperties);
            TestTypesCollection(testedProperties);
            
            var preset = GetFreshPresetTestSubject();
            (preset as IEditableObject).BeginEdit();
            preset.PresetSize = 1234;
            preset.IsMetadataModified.Should()
                .BeTrue($"Changing the preset size should modify IsMetadataModified when edit mode is on");
            preset.UserModifiedProperties.Should().NotContain("PresetSize");
            testedProperties.Add("PresetSize");
            
            preset = GetFreshPresetTestSubject();
            (preset as IEditableObject).BeginEdit();
            preset.PresetCompressedSize = 1234;
            preset.IsMetadataModified.Should()
                .BeTrue($"Changing the preset compressed size should modify IsMetadataModified when edit mode is on");
            preset.UserModifiedProperties.Should().NotContain("PresetCompressedSize");
            testedProperties.Add("PresetCompressedSize");
            
            preset = GetFreshPresetTestSubject();
            (preset as IEditableObject).BeginEdit();
            preset.PreviewNote.NoteNumber = 121;
            preset.IsMetadataModified.Should()
                .BeTrue($"Changing the previewnote object should modify IsMetadataModified when edit mode is on");
            preset.UserModifiedProperties.Should().NotContain("PreviewNote");
            preset.UserModifiedProperties.Should().Contain("PreviewNoteNumber");
            testedProperties.Add("PreviewNote");

            TestPresetBank(testedProperties);
            testedProperties.AddRange(PropertiesWhichAreNotRelevantToMetadataModified);


            var allProperties =
                (from prop in typeof(Preset).GetProperties() where !prop.GetType().IsEnum select prop.Name).ToList();

            allProperties.Except(testedProperties).Should().BeEmpty("We want to test ALL TEH PROPERTIEZ");
        }

        private void TestPresetBank(HashSet<string> testedProperties)
        {
            var preset = GetFreshPresetTestSubject();
            (preset as IEditableObject).BeginEdit();
            
            preset.Plugin.RootBank.PresetBanks.First().PresetBanks.Add(new PresetBank() { BankName = "yo mama"});
            preset.IsMetadataModified.Should().BeFalse("a bank was added but did not change our bank path");
            
            preset = GetFreshPresetTestSubject();
            (preset as IEditableObject).BeginEdit();
            var pp = new PresetBank() {BankName = "yo mama"};
            preset.Plugin.RootBank.PresetBanks.First().PresetBanks.Add(pp);
            preset.PresetBank = pp;
            preset.IsMetadataModified.Should().BeTrue("the bank was changed and should trigger a change");
            
            preset = GetFreshPresetTestSubject();
            (preset as IEditableObject).BeginEdit();
            preset.Plugin.RootBank.PresetBanks.First().First().BankName = "diz changed";
            preset.IsMetadataModified.Should().BeTrue("the bank name was changed and should trigger a change");

            testedProperties.Add("PresetBank");
        }

        private void TestModesCollection(HashSet<string> testedProperties)
        {
            var preset = GetFreshPresetTestSubject();
            (preset as IEditableObject).BeginEdit();
            preset.Modes.First().Name = "test";
            preset.IsMetadataModified.Should()
                .BeTrue($"Changing a mode name should modify IsMetadataModified when edit mode is on");
            preset.UserModifiedProperties.Should().Contain("Modes");

            preset = GetFreshPresetTestSubject();
            (preset as IEditableObject).BeginEdit();
            preset.Modes.RemoveFirst();
            preset.IsMetadataModified.Should()
                .BeTrue($"Removing a mode should modify IsMetadataModified when edit mode is on");
            preset.UserModifiedProperties.Should().Contain("Modes");

            preset = GetFreshPresetTestSubject();
            (preset as IEditableObject).BeginEdit();
            preset.Modes.Add(new Mode {Name = "bla"});
            preset.IsMetadataModified.Should()
                .BeTrue($"Adding a mode name should modify IsMetadataModified when edit mode is on");
            preset.UserModifiedProperties.Should().Contain("Modes");

            preset = GetFreshPresetTestSubject();
            (preset as IEditableObject).BeginEdit();
            preset.Modes = new FastObservableCollection<Mode>();
            preset.IsMetadataModified.Should()
                .BeTrue($"Replacing the modes collection should modify IsMetadataModified when edit mode is on");
            preset.UserModifiedProperties.Should().Contain("Modes");

            preset = GetFreshPresetTestSubject();
            var oldModes = preset.Modes;
            preset.Modes = new FastObservableCollection<Mode>();

            (preset as IEditableObject).BeginEdit();
            oldModes.Clear();
            preset.IsMetadataModified.Should()
                .BeFalse($"Modifying a detached modes collection should not modify IsMetadataModified when edit mode is on");
            preset.UserModifiedProperties.Should().NotContain("Modes");

            testedProperties.Add("Modes");
        }

        private void TestTypesCollection(HashSet<string> testedProperties)
        {
            var preset = GetFreshPresetTestSubject();
            (preset as IEditableObject).BeginEdit();
            preset.Types.First().Name = "test";
            preset.IsMetadataModified.Should()
                .BeTrue($"Changing a type name should modify IsMetadataModified when edit mode is on");
            preset.UserModifiedProperties.Should().Contain("Types");
            
            preset = GetFreshPresetTestSubject();
            (preset as IEditableObject).BeginEdit();
            preset.Types.First().SubTypeName = "test";
            preset.IsMetadataModified.Should()
                .BeTrue($"Changing a type name should modify IsMetadataModified when edit mode is on");
            preset.UserModifiedProperties.Should().Contain("Types");

            preset = GetFreshPresetTestSubject();
            (preset as IEditableObject).BeginEdit();
            preset.Types.RemoveFirst();
            preset.IsMetadataModified.Should()
                .BeTrue($"Removing a type should modify IsMetadataModified when edit mode is on");
            preset.UserModifiedProperties.Should().Contain("Types");

            preset = GetFreshPresetTestSubject();
            (preset as IEditableObject).BeginEdit();
            preset.Types.Add(new Type{Name = "bla"});
            preset.IsMetadataModified.Should()
                .BeTrue($"Adding a type name should modify IsMetadataModified when edit mode is on");
            preset.UserModifiedProperties.Should().Contain("Types");

            preset = GetFreshPresetTestSubject();
            (preset as IEditableObject).BeginEdit();
            preset.Types = new FastObservableCollection<Type>();
            preset.IsMetadataModified.Should()
                .BeTrue($"Replacing the type collection should modify IsMetadataModified when edit mode is on");
            preset.UserModifiedProperties.Should().Contain("Types");

            preset = GetFreshPresetTestSubject();
            var oldTypes = preset.Types;
            preset.Types = new FastObservableCollection<Type>();

            (preset as IEditableObject).BeginEdit();
            oldTypes.Clear();
            preset.IsMetadataModified.Should()
                .BeFalse($"Modifying a detached types collection should not modify IsMetadataModified when edit mode is on");
            preset.UserModifiedProperties.Should().NotContain("Types");

            testedProperties.Add("Types");
        }
    }
}