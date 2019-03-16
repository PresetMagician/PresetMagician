using System.Collections;
using System.Collections.Generic;
using Ceras;
using PresetMagician.Core.Data;
using PresetMagician.Core.Extensions;
using PresetMagician.Core.Interfaces;
using Catel.Collections;

namespace PresetMagician.Core.Models
{
    public class EditablePresetMetadata : ModelBase, IPresetMetadata
    {
        public override HashSet<string> GetEditableProperties()
        {
            return EditableProperties;
        }

        private static HashSet<string> EditableProperties { get; } = new HashSet<string>
        {
            nameof(Author),
            nameof(BankPath),
            nameof(Comment),
            nameof(Characteristics),
            nameof(PresetName),
            nameof(Types)
        };

        private bool _lastTypesUserModifiedValue;
        private bool _lastCharacteristicsUserModifiedValue;

        [Include] public HashSet<string> UserOverwrittenProperties { get; set; } = new HashSet<string>();

        /// <summary>
        /// The name of the preset
        /// </summary>
        [Include]
        public string PresetName { get; set; }

        /// <summary>
        /// The author of the preset
        /// </summary>
        [Include]
        public string Author { get; set; }

        /// <summary>
        /// The preset comment
        /// </summary>
        [Include]
        public string Comment { get; set; }

        /// <summary>
        /// The bank path. Only set via EntityFramework when loading from the database
        /// </summary>
        [Include]
        public string BankPath { get; set; }

        /// <summary>
        /// The Native Instruments types used for this preset.
        /// </summary>
        [Include]
        public FastObservableCollection<Type> Types { get; set; } = new TypeCollection();

        /// <summary>
        /// The Native Instruments characteristics used for this preset.
        /// </summary>
        [Include]
        public FastObservableCollection<Characteristic> Characteristics { get; set; } = new CharacteristicCollection();

        public override void BeginEdit(IUserEditable originatingObject)
        {
            if (!IsEditing)
            {
                _lastCharacteristicsUserModifiedValue = false;
                _lastTypesUserModifiedValue = false;
            }
            
            
            base.BeginEdit(originatingObject);
        }

        protected override void OnCollectionItemPropertyChanged(object sender,
            WrappedCollectionItemPropertyChangedEventArgs e)
        {
            if (e.SourceProperty == nameof(Characteristics) &&
                e.OriginalPropertyChangedEventArgs.PropertyName == nameof(IsUserModified) &&
                _lastCharacteristicsUserModifiedValue != ((CharacteristicCollection)Characteristics).IsUserModified)
            {
                OnPropertyChanged(e.SourceProperty, null, Characteristics);
                _lastCharacteristicsUserModifiedValue = ((CharacteristicCollection) Characteristics).IsUserModified;
            }

            if (e.SourceProperty == nameof(Types) &&
                e.OriginalPropertyChangedEventArgs.PropertyName == nameof(IsUserModified) &&
                _lastTypesUserModifiedValue != ((TypeCollection)Types).IsUserModified)
            {
                OnPropertyChanged(e.SourceProperty, null, Types);
                _lastTypesUserModifiedValue = ((TypeCollection) Types).IsUserModified;
            }

            base.OnCollectionItemPropertyChanged(sender, e);
        }

        public void ApplyFrom(IPresetMetadata presetMetadata)
        {
            if (!UserOverwrittenProperties.Contains(nameof(Author)))
            {
                Author = presetMetadata.Author;
            }

            if (!UserOverwrittenProperties.Contains(nameof(Comment)))
            {
                Comment = presetMetadata.Comment;
            }

            if (!UserOverwrittenProperties.Contains(nameof(PresetName)))
            {
                PresetName = presetMetadata.PresetName;
            }

            if (!UserOverwrittenProperties.Contains(nameof(BankPath)))
            {
                BankPath = presetMetadata.BankPath;
            }

            if (!UserOverwrittenProperties.Contains(nameof(Types)))
            {
                Types.Clear();

                foreach (var type in presetMetadata.Types)
                {
                    if (!type.IsIgnored)
                    {
                        Types.Add(new Type {TypeName = type.TypeName, SubTypeName = type.SubTypeName});
                    }
                }
            }

            if (!UserOverwrittenProperties.Contains(nameof(Characteristics)))
            {
                Characteristics.Clear();

                foreach (var characteristic in presetMetadata.Characteristics)
                {
                    if (!characteristic.IsIgnored)
                    {
                        Characteristics.Add(new Characteristic
                            {CharacteristicName = characteristic.CharacteristicName});
                    }
                }
            }
        }
    }
}