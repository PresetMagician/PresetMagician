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
        
        [Include]
        public HashSet<string> UserOverwrittenProperties { get; set; } = new HashSet<string>();

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
        public FastObservableCollection<Type> Types { get; set; }= new TypeCollection();
        
        /// <summary>
        /// The Native Instruments characteristics used for this preset.
        /// </summary>
        [Include]
        public FastObservableCollection<Characteristic> Characteristics { get; set; }= new CharacteristicCollection();

        protected override void OnCollectionItemPropertyChanged(object sender, WrappedCollectionItemPropertyChangedEventArgs e)
        {
            if (e.SourceProperty == nameof(Characteristics))
            {
                OnPropertyChanged(e.SourceProperty, Characteristics, Characteristics);
            }
            
            if (e.SourceProperty == nameof(Types))
            {
                OnPropertyChanged(e.SourceProperty, Types, Types);
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
                    Types.Add(new Type { TypeName = type.TypeName, SubTypeName = type.SubTypeName});
                }
            }
            
            if (!UserOverwrittenProperties.Contains(nameof(Characteristics)))
            {
                Characteristics.Clear();

                foreach (var characteristic in presetMetadata.Characteristics)
                {
                    Characteristics.Add(new Characteristic { CharacteristicName= characteristic.CharacteristicName});
                }
            }
        }
    }
}