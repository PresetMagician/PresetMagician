using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Catel;
using Catel.Collections;
using Catel.Data;
using Catel.Reflection;
using FluentAssertions;
using PresetMagician.Core.Models;
using Xunit;
using Xunit.Abstractions;
using Type = PresetMagician.Core.Models.Type;

// ReSharper disable PossibleNullReferenceException

namespace PresetMagician.Tests.ModelTests
{
    public class PresetTests 
    {
        private Dictionary<string, object> PropertiesWhichShouldNotModifyIsMetadataModified =
            new Dictionary<string, object>
            {
                {nameof(Preset.LastExportedPresetHash), "bla"},
                {nameof(Preset.LastExported), DateTime.Now}
            };

        private Dictionary<string, object> PropertiesWhichShouldModifyIsMetadataModified =
            new Dictionary<string, object>
            {
                {nameof(Preset.PresetName), "bla"},
                {nameof(Preset.PreviewNotePlayer), new PreviewNotePlayer()},
                {nameof(Preset.Author), "bla"},
                {nameof(Preset.Comment), "bla"},
                {nameof(Preset.BankPath), "bla/foo"},
                {nameof(Preset.PresetHash), "dingdong"},
                {nameof(Preset.PresetSize), 109991},
                {nameof(Preset.PresetCompressedSize), 109991}
            };

        private Dictionary<string, object> PropertiesWhichShouldModifyIsMetadataModifiedInEditMode =
            new Dictionary<string, object>
            {
                {nameof(Preset.PresetName), "bla"},
                {nameof(Preset.PreviewNotePlayer), new PreviewNotePlayer()},
                {nameof(Preset.Author), "bla"},
                {nameof(Preset.Comment), "bla"},
                {nameof(Preset.BankPath), "bla/foo"},
            };

        private HashSet<string> PropertiesWhichAreNotRelevantToMetadataModifiedInEditMode =
            new HashSet<string>
            {
                nameof(Preset.PresetSize),
                nameof(Preset.PresetCompressedSize),
                nameof(Preset.PresetHash)
                
            };

    private HashSet<string> PropertiesWhichAreNotRelevantToMetadataModified =
            new HashSet<string>
            {
                nameof(Preset.EditableProperties),
                nameof(Preset.IsUserModified),
                nameof(Preset.IsEditing),
                nameof(Preset.UserModifiedProperties),
                nameof(Preset.SourceFile),
                nameof(Preset.IsIgnored),
                nameof(Preset.Plugin),
                nameof(Preset.IsMetadataModified),
                nameof(Preset.ChangedSinceLastExport),
                nameof(Preset.PresetId),
                nameof(Preset.UserOverwrittenProperties)
            };

        private readonly ITestOutputHelper _output;

        public PresetTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private static Preset GetFreshPresetTestSubject()
        {
            var characteristic = new Characteristic();
            var type = new Type();
            var preset = new Preset();
            preset.BankPath = "foo/bar";
            var plugin = new Plugin();
            preset.Plugin = plugin;
            plugin.Presets.Add(preset);
            preset.Types.Add(type);
            preset.Characteristics.Add(characteristic);
            preset.LastExportedPresetHash = "foobar";
            preset.PresetHash = "foobar";
            preset.PresetName = "my preset";
            preset.PresetSize = 1234;
            preset.PresetCompressedSize = 4567;
            preset.Author = "horst";
            preset.Comment = "kein kommentar";
            preset.IsMetadataModified = false;

            return preset;
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
        public void TestBackupPerformance()
        {
            for (var i = 0; i < 100; i++)
            {
                var preset2 = GetFreshPresetTestSubject();
                ((IEditableObject) preset2).BeginEdit();
            }
        }

        [Fact]
        public void TestBackupAndRestorePerformance()
        {
            for (var i = 0; i < 100; i++)
            {
                var preset2 = GetFreshPresetTestSubject();
                ((IEditableObject) preset2).BeginEdit();
                ((IEditableObject) preset2).CancelEdit();
            }
        }

        [Fact]
        public void TestIsMetadataUserModified()
        {
            var testedProperties = new HashSet<string>();

            foreach (var notModifyProperty in PropertiesWhichShouldNotModifyIsMetadataModified)
            {
                var preset2 = GetFreshPresetTestSubject();
                ((IEditableObject) preset2).BeginEdit();

                preset2.GetType().GetProperty(notModifyProperty.Key).SetValue(preset2, notModifyProperty.Value);
                preset2.IsMetadataModified.Should()
                    .BeFalse($"{notModifyProperty.Key} should not modify IsMetadataModified");

                testedProperties.Add(notModifyProperty.Key);
            }

            foreach (var modifyProperty in PropertiesWhichShouldModifyIsMetadataModifiedInEditMode)
            {
                var preset2 = GetFreshPresetTestSubject();
                (preset2 as IEditableObject).BeginEdit();
                preset2.GetType().GetProperty(modifyProperty.Key).SetValue(preset2, modifyProperty.Value);

                preset2.IsMetadataModified.Should()
                    .BeTrue($"{modifyProperty.Key} should modify IsMetadataModified when edit mode is on");

                if (Preset.PresetParserMetadataProperties.Contains(modifyProperty.Key))
                {
                    preset2.UserOverwrittenProperties.Should().Contain(modifyProperty.Key,
                        $"UserOverwrittenProperties should contain {modifyProperty.Key} because it's a PresetParserMetadataProperties which is now user modified");
                }

                testedProperties.Add(modifyProperty.Key);
            }

            foreach (var modifyProperty in PropertiesWhichShouldModifyIsMetadataModifiedInEditMode)
            {
                var preset2 = GetFreshPresetTestSubject();
                preset2.GetType().GetProperty(modifyProperty.Key).SetValue(preset2, modifyProperty.Value);

                preset2.IsMetadataModified.Should()
                    .BeFalse($"{modifyProperty.Key} should not modify IsMetadataModified if edit mode is off");

                testedProperties.Add(modifyProperty.Key);
            }

            TestCharacteristicsCollection(testedProperties);
            TestTypesCollection(testedProperties);

            var preset = GetFreshPresetTestSubject();
            (preset as IEditableObject).BeginEdit();


            TestPresetBank(testedProperties);
            testedProperties.AddRange(PropertiesWhichAreNotRelevantToMetadataModified);
            testedProperties.AddRange(PropertiesWhichAreNotRelevantToMetadataModifiedInEditMode);


            var allProperties =
                (from prop in typeof(Preset).GetProperties() where !prop.GetType().IsEnum select prop.Name).ToList();

            allProperties.Except(testedProperties).Should().BeEmpty("We want to test ALL TEH PROPERTIEZ");
        }


        [Fact]
        public void TestIsMetadataPresetParserModified()
        {
            var testedProperties = new HashSet<string>();

            foreach (var notModifyProperty in PropertiesWhichShouldNotModifyIsMetadataModified)
            {
                var preset2 = GetFreshPresetTestSubject();
                var preset3 = GetFreshPresetTestSubject();
                preset3.IsMetadataModified = false;
                preset2.GetType().GetProperty(notModifyProperty.Key).SetValue(preset2, notModifyProperty.Value);

                preset3.SetFromPresetParser(preset2);
                preset3.IsMetadataModified.Should()
                    .BeFalse($"{notModifyProperty.Key} should not modify IsMetadataModified");

                testedProperties.Add(notModifyProperty.Key);
            }

            foreach (var modifyProperty in PropertiesWhichShouldModifyIsMetadataModified)
            {
                var preset2 = GetFreshPresetTestSubject();

                var preset3 = GetFreshPresetTestSubject();
                PropertyHelper.SetPropertyValue(preset2, modifyProperty.Key, modifyProperty.Value);

                preset3.IsMetadataModified = false;

                preset3.SetFromPresetParser(preset2);

                preset3.IsMetadataModified.Should()
                    .BeTrue($"{modifyProperty.Key} should modify IsMetadataModified when edit mode is on");


                testedProperties.Add(modifyProperty.Key);
            }

            TestPresetBankPresetParser(testedProperties);
            TestCharacteristicsCollectionPresetParser(testedProperties);
            TestTypesCollectionPresetParser(testedProperties);
            testedProperties.AddRange(PropertiesWhichAreNotRelevantToMetadataModified);


            var allProperties =
                (from prop in typeof(Preset).GetProperties() where !prop.GetType().IsEnum select prop.Name).ToList();

            allProperties.Except(testedProperties).Should().BeEmpty("We want to test ALL TEH PROPERTIEZ");
        }

        private void TestPresetBankPresetParser(HashSet<string> testedProperties)
        {
            var preset = GetFreshPresetTestSubject();
            var preset2 = GetFreshPresetTestSubject();
            preset.Plugin.RootBank.PresetBanks.First().PresetBanks.Add(new PresetBank() {BankName = "yo mama"});
            preset2.SetFromPresetParser(preset);
            preset2.IsMetadataModified.Should().BeFalse("a bank was added but did not change our bank path");


            preset = GetFreshPresetTestSubject();
            preset2 = GetFreshPresetTestSubject();
            var pp = new PresetBank() {BankName = "yo mama"};
            preset.Plugin.RootBank.PresetBanks.First().PresetBanks.Add(pp);
            preset.PresetBank = pp;
            preset2.SetFromPresetParser(preset);
            preset2.IsMetadataModified.Should().BeTrue("the bank was changed and should trigger a change");

            preset = GetFreshPresetTestSubject();
            preset2 = GetFreshPresetTestSubject();
            preset.Plugin.RootBank.PresetBanks.First().First().BankName = "diz changed";
            preset2.SetFromPresetParser(preset);
            preset2.IsMetadataModified.Should().BeTrue("the bank name was changed and should trigger a change");

            testedProperties.Add(nameof(Preset.PresetBank));
        }

        private void TestPresetBank(HashSet<string> testedProperties)
        {
            var preset = GetFreshPresetTestSubject();
            ((IEditableObject) preset).BeginEdit();

            preset.Plugin.RootBank.PresetBanks.First().PresetBanks.Add(new PresetBank() {BankName = "yo mama"});
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

        private void TestCharacteristicsCollection(HashSet<string> testedProperties)
        {
            var preset = GetFreshPresetTestSubject();
           
            (preset as IEditableObject).BeginEdit();
            preset.Characteristics.First().CharacteristicName = "dingblafoo";
            preset.IsMetadataModified.Should()
                .BeTrue("renaming a characteristic should modify IsMetadataModified when edit mode is on");
            preset.UserOverwrittenProperties.Should().Contain(nameof(Preset.Characteristics));
            
            preset = GetFreshPresetTestSubject();
            (preset as IEditableObject).BeginEdit();
            preset.Characteristics.RemoveFirst();
            preset.IsMetadataModified.Should()
                .BeTrue("Removing a mode should modify IsMetadataModified when edit mode is on");
            preset.UserOverwrittenProperties.Should().Contain(nameof(Preset.Characteristics));

            preset = GetFreshPresetTestSubject();
            (preset as IEditableObject).BeginEdit();
            preset.Characteristics.Add(new Characteristic() {CharacteristicName = "bla"});
            preset.IsMetadataModified.Should()
                .BeTrue("Adding a mode name should modify IsMetadataModified when edit mode is on");
            preset.UserOverwrittenProperties.Should().Contain(nameof(Preset.Characteristics));
            
            preset = GetFreshPresetTestSubject();
            var preset2 = GetFreshPresetTestSubject();
            (preset as IEditableObject).BeginEdit();
            (preset2 as IEditableObject).BeginEdit();
            
            preset.Characteristics.First().Should().Be(preset2.Characteristics.First());
            preset.Characteristics.First().CharacteristicName = "dingblafoo";
            preset.IsMetadataModified.Should()
                .BeTrue("renaming a characteristic should modify IsMetadataModified when edit mode is on");
            preset.UserOverwrittenProperties.Should().Contain(nameof(Preset.Characteristics));
            preset2.IsMetadataModified.Should()
                .BeTrue("renaming a characteristic should modify IsMetadataModified when edit mode is on");
            preset2.UserOverwrittenProperties.Should().Contain(nameof(Preset.Characteristics));
            
            testedProperties.Add(nameof(Preset.Characteristics));
        }

        private void TestTypesCollection(HashSet<string> testedProperties)
        {
            var preset = GetFreshPresetTestSubject();
          
            (preset as IEditableObject).BeginEdit();
            preset.Types.First().TypeName = "blatype";
            preset.IsMetadataModified.Should()
                .BeTrue("renaming a type should modify IsMetadataModified when edit mode is on");
            preset.UserOverwrittenProperties.Should().Contain(nameof(Preset.Types));
            
            preset = GetFreshPresetTestSubject();
            (preset as IEditableObject).BeginEdit();
            preset.Types.First().SubTypeName = "blatype";
            preset.IsMetadataModified.Should()
                .BeTrue("renaming a type should modify IsMetadataModified when edit mode is on");
            preset.UserOverwrittenProperties.Should().Contain(nameof(Preset.Types));
            
            preset = GetFreshPresetTestSubject();
            (preset as IEditableObject).BeginEdit();
            preset.Types.RemoveFirst();
            preset.IsMetadataModified.Should()
                .BeTrue("Removing a type should modify IsMetadataModified when edit mode is on");
            preset.UserOverwrittenProperties.Should().Contain(nameof(Preset.Types));

            preset = GetFreshPresetTestSubject();
            (preset as IEditableObject).BeginEdit();
            preset.Types.Add(new Type {TypeName = "bla"});
            preset.IsMetadataModified.Should()
                .BeTrue("Adding a type name should modify IsMetadataModified when edit mode is on");
            preset.UserOverwrittenProperties.Should().Contain(nameof(Preset.Types));
            
            preset = GetFreshPresetTestSubject();
            var preset2 = GetFreshPresetTestSubject();
            (preset as IEditableObject).BeginEdit();
            (preset2 as IEditableObject).BeginEdit();
            
            preset.Types.First().Should().Be(preset2.Types.First());
            preset.Types.First().TypeName = "dingblafoo";
            preset.IsMetadataModified.Should()
                .BeTrue("renaming a type should modify IsMetadataModified when edit mode is on");
            preset.UserOverwrittenProperties.Should().Contain(nameof(Preset.Types));
            preset2.IsMetadataModified.Should()
                .BeTrue("renaming a type should modify IsMetadataModified when edit mode is on");
            preset2.UserOverwrittenProperties.Should().Contain(nameof(Preset.Types));
            
            preset = GetFreshPresetTestSubject();
            preset2 = GetFreshPresetTestSubject();
            (preset as IEditableObject).BeginEdit();
            (preset2 as IEditableObject).BeginEdit();
            
            preset.Types.First().Should().Be(preset2.Types.First());
            preset.Types.First().SubTypeName = "dingblafoo";
            preset.IsMetadataModified.Should()
                .BeTrue("renaming a type should modify IsMetadataModified when edit mode is on");
            preset.UserOverwrittenProperties.Should().Contain(nameof(Preset.Types));
            preset2.IsMetadataModified.Should()
                .BeTrue("renaming a type should modify IsMetadataModified when edit mode is on");
            preset2.UserOverwrittenProperties.Should().Contain(nameof(Preset.Types));


            testedProperties.Add(nameof(Preset.Types));
        }

        private void TestCharacteristicsCollectionPresetParser(HashSet<string> testedProperties)
        {
            var preset = GetFreshPresetTestSubject();
            var preset2 = GetFreshPresetTestSubject();

       
            preset.Characteristics.RemoveFirst();
            preset2.SetFromPresetParser(preset);
            preset2.IsMetadataModified.Should()
                .BeTrue("Removing a mode should modify IsMetadataModified when edit mode is on");
            preset2.UserOverwrittenProperties.Should().NotContain(nameof(Preset.Characteristics));

            preset = GetFreshPresetTestSubject();
            preset2 = GetFreshPresetTestSubject();
            preset.Characteristics.Add(new Characteristic() {CharacteristicName = "bla"});
            preset2.SetFromPresetParser(preset);
            preset2.IsMetadataModified.Should()
                .BeTrue("Adding a mode name should modify IsMetadataModified when edit mode is on");
            preset2.UserOverwrittenProperties.Should().NotContain(nameof(Preset.Characteristics));

            testedProperties.Add(nameof(Preset.Characteristics));
        }

        private void TestTypesCollectionPresetParser(HashSet<string> testedProperties)
        {
            var preset = GetFreshPresetTestSubject();
            var preset2 = GetFreshPresetTestSubject();
            
         
            
            preset.Types.RemoveFirst();
            preset2.SetFromPresetParser(preset);
            preset2.IsMetadataModified.Should()
                .BeTrue("Removing a type should modify IsMetadataModified when edit mode is on");
            preset2.UserOverwrittenProperties.Should().NotContain(nameof(Preset.Types));

            preset = GetFreshPresetTestSubject();
            preset2 = GetFreshPresetTestSubject();
            preset2.SetFromPresetParser(preset);
            preset.Types.Add(new Type {TypeName = "bla"});
            preset2.SetFromPresetParser(preset);
            preset2.IsMetadataModified.Should()
                .BeTrue("Adding a type name should modify IsMetadataModified when edit mode is on");
            preset2.UserOverwrittenProperties.Should().NotContain(nameof(Preset.Types));


            testedProperties.Add(nameof(Preset.Types));
        }
    }
}