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
                {nameof(Preset.LastExported), DateTime.Now}
            };

        private Dictionary<string, object> PropertiesWhichShouldModifyIsMetadataModified =
            new Dictionary<string, object>
            {
                {nameof(Preset.PresetHash), "dingdong"},
                {nameof(Preset.PreviewNotePlayer), new PreviewNotePlayer()},
                
            };
        
        private Dictionary<string, object> MetadataPropertiesWhichShouldModifyIsMetadataModified =
            new Dictionary<string, object>
            {
                {nameof(PresetMetadata.PresetName), "bla"},
                {nameof(PresetMetadata.Author), "bla"},
                {nameof(PresetMetadata.Comment), "bla"},
                {nameof(PresetMetadata.BankPath), "bla/foo"},
            };

        private Dictionary<string, object> PropertiesWhichShouldModifyIsMetadataModifiedInEditMode =
            new Dictionary<string, object>
            {
                {nameof(Preset.PreviewNotePlayer), new PreviewNotePlayer()},
            };
        
        private Dictionary<string, object> MetadataPropertiesWhichShouldModifyIsMetadataModifiedInEditMode =
            new Dictionary<string, object>
            {
                {nameof(PresetMetadata.PresetName), "bla"},
                {nameof(PresetMetadata.Author), "bla"},
                {nameof(PresetMetadata.Comment), "bla"},
                {nameof(PresetMetadata.BankPath), "bla/foo"},
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
                nameof(Preset.IsUserModified),
                nameof(Preset.IsEditing),
                nameof(Preset.UserModifiedProperties),
                nameof(Preset.IsIgnored),
                nameof(Preset.Plugin),
                nameof(Preset.IsMetadataModified),
                nameof(Preset.LastExported),
                nameof(Preset.PresetId),
                nameof(Preset.PresetSize),
                nameof(Preset.PresetCompressedSize),
                nameof(Preset.OriginalMetadata),
                nameof(Preset.LastExportedMetadata)
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


        /// <summary>
        /// 
        /// </summary>
        [Fact]
        public void TestPresetModifiedField()
        {
            var preset = GetFreshPresetTestSubject();

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

                testedProperties.Add(modifyProperty.Key);
            }
            
            
           

            TestMetadata(testedProperties);
           

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

          
            foreach (var modifyProperty in MetadataPropertiesWhichShouldModifyIsMetadataModified)
            {
                var preset2 = GetFreshPresetTestSubject();

                var preset3 = GetFreshPresetTestSubject();
                PropertyHelper.SetPropertyValue(preset2.OriginalMetadata, modifyProperty.Key, modifyProperty.Value);

                preset3.IsMetadataModified = false;

                preset3.SetFromPresetParser(preset2.OriginalMetadata);

                preset3.IsMetadataModified.Should()
                    .BeTrue($"{modifyProperty.Key} should modify IsMetadataModified");


                testedProperties.Add(modifyProperty.Key);
            }
            
            foreach (var modifyProperty in PropertiesWhichShouldModifyIsMetadataModified)
            {
                var preset2 = GetFreshPresetTestSubject();

                preset2.IsMetadataModified = false;
                PropertyHelper.SetPropertyValue(preset2, modifyProperty.Key, modifyProperty.Value);

                

                preset2.IsMetadataModified.Should()
                    .BeTrue($"{modifyProperty.Key} should modify IsMetadataModified");


                testedProperties.Add(modifyProperty.Key);
            }
            
            

            TestPresetParserMetadata(testedProperties);
            TestPresetBankPresetParser(testedProperties);
   
            testedProperties.AddRange(PropertiesWhichAreNotRelevantToMetadataModified);


            var allProperties =
                (from prop in typeof(Preset).GetProperties() where !prop.GetType().IsEnum select prop.Name).ToList();

            allProperties.Except(testedProperties).Should().BeEmpty("We want to test ALL TEH PROPERTIEZ");
        }

        private void TestPresetBankPresetParser(HashSet<string> testedProperties)
        {
           
           var preset = GetFreshPresetTestSubject();
           var preset2 = GetFreshPresetTestSubject();
           preset.OriginalMetadata.BankPath = "sldjflsdkj";
            preset2.SetFromPresetParser(preset.OriginalMetadata);
            preset2.IsMetadataModified.Should().BeTrue("the bank name was changed and should trigger a change");

            testedProperties.Add(nameof(Preset.PresetBank));
        }

        private void TestMetadata(HashSet<string> testedProperties)
        {
            var metadataTestedProperties = new HashSet<string>();
            foreach (var modifyProperty in MetadataPropertiesWhichShouldModifyIsMetadataModifiedInEditMode)
            {
                var preset2 = GetFreshPresetTestSubject();
                (preset2 as IEditableObject).BeginEdit();
                preset2.Metadata.GetType().GetProperty(modifyProperty.Key).SetValue(preset2.Metadata, modifyProperty.Value);

                preset2.IsMetadataModified.Should()
                    .BeTrue($"{modifyProperty.Key} should modify IsMetadataModified when edit mode is on");

                preset2.Metadata.UserOverwrittenProperties.Should().Contain(modifyProperty.Key);
                metadataTestedProperties.Add(modifyProperty.Key);
            }
            
            TestTypesCollection(metadataTestedProperties);
            TestCharacteristicsCollection(metadataTestedProperties);
            
            var allProperties =
                (from prop in typeof(PresetMetadata).GetProperties() select prop.Name).ToList();

            allProperties.Except(metadataTestedProperties).Should().BeEmpty("We want to test ALL TEH PROPERTIEZ");
            
            testedProperties.Add(nameof(Preset.Metadata));

        }
        
        private void TestPresetParserMetadata(HashSet<string> testedProperties)
        {
            var metadataTestedProperties = new HashSet<string>();
            foreach (var modifyProperty in MetadataPropertiesWhichShouldModifyIsMetadataModified)
            {
                var preset2 = GetFreshPresetTestSubject();
                var preset = GetFreshPresetTestSubject();
                preset.OriginalMetadata.GetType().GetProperty(modifyProperty.Key).SetValue(preset.OriginalMetadata, modifyProperty.Value);

                preset2.SetFromPresetParser(preset.OriginalMetadata);
                preset2.IsMetadataModified.Should()
                    .BeTrue($"{modifyProperty.Key} should modify IsMetadataModified");

                metadataTestedProperties.Add(modifyProperty.Key);
            }
            
            TestTypesCollectionPresetParser(metadataTestedProperties);
            TestCharacteristicsCollectionPresetParser(metadataTestedProperties);
            
            var allProperties =
                (from prop in typeof(PresetMetadata).GetProperties() select prop.Name).ToList();

            allProperties.Except(metadataTestedProperties).Should().BeEmpty("We want to test ALL TEH PROPERTIEZ");
            
            testedProperties.Add(nameof(Preset.Metadata));

            

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
            preset.Metadata.Characteristics.First().CharacteristicName = "dingblafoo";
            preset.IsMetadataModified.Should()
                .BeTrue("renaming a characteristic should modify IsMetadataModified when edit mode is on");
            preset.Metadata.UserOverwrittenProperties.Should().Contain(nameof(PresetMetadata.Characteristics));
            
            preset = GetFreshPresetTestSubject();
            (preset as IEditableObject).BeginEdit();
            preset.Metadata.Characteristics.RemoveFirst();
            preset.IsMetadataModified.Should()
                .BeTrue("Removing a mode should modify IsMetadataModified when edit mode is on");
            preset.Metadata.UserOverwrittenProperties.Should().Contain(nameof(PresetMetadata.Characteristics));

            preset = GetFreshPresetTestSubject();
            (preset as IEditableObject).BeginEdit();
            preset.Metadata.Characteristics.Add(new Characteristic() {CharacteristicName = "bla"});
            preset.IsMetadataModified.Should()
                .BeTrue("Adding a mode name should modify IsMetadataModified when edit mode is on");
            preset.Metadata.UserOverwrittenProperties.Should().Contain(nameof(PresetMetadata.Characteristics));
            
            preset = GetFreshPresetTestSubject();
            var preset2 = GetFreshPresetTestSubject();
            (preset as IEditableObject).BeginEdit();
            (preset2 as IEditableObject).BeginEdit();
            
            preset.Metadata.Characteristics.First().Should().Be(preset2.Metadata.Characteristics.First());
            preset.Metadata.Characteristics.First().CharacteristicName = "dingblafoo";
            preset.IsMetadataModified.Should()
                .BeTrue("renaming a characteristic should modify IsMetadataModified when edit mode is on");
            preset.Metadata.UserOverwrittenProperties.Should().Contain(nameof(PresetMetadata.Characteristics));
            preset2.IsMetadataModified.Should()
                .BeTrue("renaming a characteristic should modify IsMetadataModified when edit mode is on");
            preset2.Metadata.UserOverwrittenProperties.Should().Contain(nameof(PresetMetadata.Characteristics));
            
            testedProperties.Add(nameof(PresetMetadata.Characteristics));
        }

        private void TestTypesCollection(HashSet<string> testedProperties)
        {
            var preset = GetFreshPresetTestSubject();
          
            (preset as IEditableObject).BeginEdit();
            preset.Metadata.Types.First().TypeName = "blatype";
            preset.IsMetadataModified.Should()
                .BeTrue("renaming a type should modify IsMetadataModified when edit mode is on");
            preset.Metadata.UserOverwrittenProperties.Should().Contain(nameof(PresetMetadata.Types));
            
            preset = GetFreshPresetTestSubject();
            (preset as IEditableObject).BeginEdit();
            preset.Metadata.Types.First().SubTypeName = "blatype";
            preset.IsMetadataModified.Should()
                .BeTrue("renaming a type should modify IsMetadataModified when edit mode is on");
            preset.Metadata.UserOverwrittenProperties.Should().Contain(nameof(PresetMetadata.Types));
            
            preset = GetFreshPresetTestSubject();
            (preset as IEditableObject).BeginEdit();
            preset.Metadata.Types.RemoveFirst();
            preset.IsMetadataModified.Should()
                .BeTrue("Removing a type should modify IsMetadataModified when edit mode is on");
            preset.Metadata.UserOverwrittenProperties.Should().Contain(nameof(PresetMetadata.Types));

            preset = GetFreshPresetTestSubject();
            (preset as IEditableObject).BeginEdit();
            preset.Metadata.Types.Add(new Type {TypeName = "bla"});
            preset.IsMetadataModified.Should()
                .BeTrue("Adding a type name should modify IsMetadataModified when edit mode is on");
            preset.Metadata.UserOverwrittenProperties.Should().Contain(nameof(PresetMetadata.Types));
            
            preset = GetFreshPresetTestSubject();
            var preset2 = GetFreshPresetTestSubject();
            (preset as IEditableObject).BeginEdit();
            (preset2 as IEditableObject).BeginEdit();
            
            preset.Metadata.Types.First().Should().Be(preset2.Metadata.Types.First());
            preset.Metadata.Types.First().TypeName = "dingblafoo";
            preset.IsMetadataModified.Should()
                .BeTrue("renaming a type should modify IsMetadataModified when edit mode is on");
            preset.Metadata.UserOverwrittenProperties.Should().Contain(nameof(PresetMetadata.Types));
            preset2.IsMetadataModified.Should()
                .BeTrue("renaming a type should modify IsMetadataModified when edit mode is on");
            preset2.Metadata.UserOverwrittenProperties.Should().Contain(nameof(PresetMetadata.Types));
            
            preset = GetFreshPresetTestSubject();
            preset2 = GetFreshPresetTestSubject();
            (preset as IEditableObject).BeginEdit();
            (preset2 as IEditableObject).BeginEdit();
            
            preset.Metadata.Types.First().Should().Be(preset2.Metadata.Types.First());
            preset.Metadata.Types.First().SubTypeName = "dingblafoo";
            preset.IsMetadataModified.Should()
                .BeTrue("renaming a type should modify IsMetadataModified when edit mode is on");
            preset.Metadata.UserOverwrittenProperties.Should().Contain(nameof(PresetMetadata.Types));
            preset2.IsMetadataModified.Should()
                .BeTrue("renaming a type should modify IsMetadataModified when edit mode is on");
            preset2.Metadata.UserOverwrittenProperties.Should().Contain(nameof(PresetMetadata.Types));


            testedProperties.Add(nameof(PresetMetadata.Types));
        }

        private void TestCharacteristicsCollectionPresetParser(HashSet<string> testedProperties)
        {
            var preset = GetFreshPresetTestSubject();
            var preset2 = GetFreshPresetTestSubject();

       
            preset.OriginalMetadata.Characteristics.RemoveFirst();
            preset2.SetFromPresetParser(preset.OriginalMetadata);
            preset2.IsMetadataModified.Should()
                .BeTrue("Removing a mode should modify IsMetadataModified when edit mode is on");
            preset2.Metadata.UserOverwrittenProperties.Should().NotContain(nameof(PresetMetadata.Characteristics));

            preset = GetFreshPresetTestSubject();
            preset2 = GetFreshPresetTestSubject();
            preset.OriginalMetadata.Characteristics.Add(new Characteristic() {CharacteristicName = "bla"});
            preset2.SetFromPresetParser(preset.OriginalMetadata);
            preset2.IsMetadataModified.Should()
                .BeTrue("Adding a mode name should modify IsMetadataModified when edit mode is on");
            preset2.Metadata.UserOverwrittenProperties.Should().NotContain(nameof(PresetMetadata.Characteristics));

            testedProperties.Add(nameof(PresetMetadata.Characteristics));
        }

        private void TestTypesCollectionPresetParser(HashSet<string> testedProperties)
        {
            var preset = GetFreshPresetTestSubject();
            var preset2 = GetFreshPresetTestSubject();
            
         
            
            preset.OriginalMetadata.Types.RemoveFirst();
            preset2.SetFromPresetParser(preset.OriginalMetadata);
            preset2.IsMetadataModified.Should()
                .BeTrue("Removing a type should modify IsMetadataModified when edit mode is on");
            preset2.Metadata.UserOverwrittenProperties.Should().NotContain(nameof(PresetMetadata.Types));

            preset = GetFreshPresetTestSubject();
            preset2 = GetFreshPresetTestSubject();
            preset2.SetFromPresetParser(preset.OriginalMetadata);
            preset.OriginalMetadata.Types.Add(new Type {TypeName = "bla"});
            preset2.SetFromPresetParser(preset.OriginalMetadata);
            preset2.IsMetadataModified.Should()
                .BeTrue("Adding a type name should modify IsMetadataModified when edit mode is on");
            preset2.Metadata.UserOverwrittenProperties.Should().NotContain(nameof(PresetMetadata.Types));


            testedProperties.Add(nameof(PresetMetadata.Types));
        }
    }
}