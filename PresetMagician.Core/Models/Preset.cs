using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using Catel.Data;
using Catel.Reflection;
using Ceras;
using PresetMagician.Core.Data;
using ModelBase = PresetMagician.Core.Data.ModelBase;


namespace PresetMagician.Core.Models
{
    public class Preset : ModelBase
    {
        #region Fields

        /// <summary>
        /// All properties which should modify the IsMetadataModified flag.
        /// </summary>
        private static readonly List<string> _propertiesWhichModifyMetadata = new List<string>
        {
            nameof(Author),
            nameof(BankPath),
            nameof(Comment),
            nameof(Characteristics),
            nameof(PresetCompressedSize),
            nameof(PresetHash),
            nameof(PresetName),
            nameof(PresetSize),
            nameof(PreviewNotePlayer),
            nameof(Types)
        };

        /// <summary>
        /// A list of all preset metadata properties which can be set using a preset parser.
        /// </summary>
        public static readonly List<string> PresetParserMetadataProperties = new List<string>
        {
            nameof(Author),
            nameof(BankPath),
            nameof(Comment),
            nameof(Characteristics),
            nameof(PresetCompressedSize),
            nameof(PresetHash),
            nameof(PresetName),
            nameof(PreviewNotePlayer),
            nameof(PresetSize),
            nameof(Types)
        };

        public override ICollection<string> EditableProperties { get; } = new List<string>
        {
            nameof(Author),
            nameof(BankPath),
            nameof(Comment),
            nameof(Characteristics),
            nameof(PresetName),
            nameof(PreviewNotePlayer),
            nameof(Types)
        };

        /// <summary>
        /// Saves all properties which can be set by a preset parser, but were updated by the user
        /// </summary>
        [Include]
        public HashSet<string> UserOverwrittenProperties { get; set; } = new HashSet<string>();

        #endregion

        public Preset()
        {
        }

        public override void BeginEdit(IUserEditable originatingObject)
        {
            Characteristics.ItemPropertyChanged += WrapperOnModesCollectionItemPropertyChanged;
            Characteristics.CollectionChanged += WrapperOnModesCollectionChanged;
            Types.ItemPropertyChanged += WrapperOnTypesCollectionItemPropertyChanged;
            Types.CollectionChanged += WrapperOnTypesCollectionChanged;
            base.BeginEdit(originatingObject);
        }

        public override void EndEdit(IUserEditable originatingObject)
        {
            Characteristics.ItemPropertyChanged -= WrapperOnModesCollectionItemPropertyChanged;
            Characteristics.CollectionChanged -= WrapperOnModesCollectionChanged;
            Types.ItemPropertyChanged -= WrapperOnTypesCollectionItemPropertyChanged;
            Types.CollectionChanged -= WrapperOnTypesCollectionChanged;

            if (UserModifiedProperties.Count > 0)
            {
                IsMetadataModified = true;
            }

            base.EndEdit(originatingObject);
        }

        public override void CancelEdit(IUserEditable originatingObject)
        {
            Characteristics.ItemPropertyChanged -= WrapperOnModesCollectionItemPropertyChanged;
            Characteristics.CollectionChanged -= WrapperOnModesCollectionChanged;
            Types.ItemPropertyChanged -= WrapperOnTypesCollectionItemPropertyChanged;
            Types.CollectionChanged -= WrapperOnTypesCollectionChanged;
            base.CancelEdit(originatingObject);
        }

        /// <summary>
        /// Sets updated data delivered by the preset parser. Ignores user-modified properties.
        /// </summary>
        /// <param name="preset"></param>
        public void SetFromPresetParser(Preset preset)
        {
            foreach (var property in PresetParserMetadataProperties)
            {
                if (!UserOverwrittenProperties.Contains(property))
                {
                    var value = PropertyHelper.GetPropertyValue(preset, property);

                    var currentValue = PropertyHelper.GetPropertyValue(this, property);

                    if (value != currentValue)
                    {
                        if (value is CharacteristicCollection characteristicCollection)
                        {
                            if (!characteristicCollection.IsEqualTo((CharacteristicCollection) currentValue))
                            {
                                IsMetadataModified = true;
                            }
                        }
                        else if (value is TypeCollection collection)
                        {
                            if (!collection.IsEqualTo((TypeCollection) currentValue))
                            {
                                IsMetadataModified = true;
                            }
                        }
                        else if (value is PreviewNotePlayer previewNotePlayer)
                        {
                            if (!previewNotePlayer.IsEqualTo((PreviewNotePlayer) currentValue))
                            {
                                IsMetadataModified = true;
                            }
                        }
                        else
                        {
                            if (value == null || !value.Equals(currentValue))
                            {
                                PropertyHelper.SetPropertyValue(this, property, value);
                                IsMetadataModified = true;
                            }
                        }
                    }

                    if (IsMetadataModified)
                    {
                        //Debug.WriteLine($"setting IsMetadataModified for preset {PresetName} to true because of {property}");
                    }

                    // todo: check if the property has changed and trigger metadata updates
                    // todo: also iterate over collections
                }
            }
        }

        private void UpdateIsMetadataModifiedInEditMode(string propertyName)
        {
            if (UserModifiedProperties.Contains(propertyName))
            {
                UserOverwrittenProperties.Add(propertyName);
            }
            else
            {
                if (!((HashSet<string>) BackupValues[nameof(UserOverwrittenProperties)]).Contains(propertyName))
                {
                    UserOverwrittenProperties.Remove(propertyName);
                }
            }


            if ((bool) BackupValues[nameof(IsMetadataModified)])
            {
                IsMetadataModified = true;
                return;
            }

            IsMetadataModified = IsUserModified;
        }

        private void WrapperOnTypesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!IsEditing)
            {
                return;
            }

            if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Remove ||
                e.Action == NotifyCollectionChangedAction.Replace)
            {
                UpdateIsMetadataModifiedInEditMode(nameof(Types));
            }
        }

        private void WrapperOnTypesCollectionItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!IsEditing)
            {
                return;
            }

            if (e.PropertyName == nameof(Type.TypeName) || e.PropertyName == nameof(Type.SubTypeName))
            {
                UpdateIsMetadataModifiedInEditMode(nameof(Types));
            }
        }

        private void WrapperOnModesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!IsEditing)
            {
                return;
            }

            if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Remove ||
                e.Action == NotifyCollectionChangedAction.Replace)
            {
                UpdateIsMetadataModifiedInEditMode(nameof(Characteristics));
            }
        }

        private void WrapperOnModesCollectionItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!IsEditing)
            {
                return;
            }

            if (e.PropertyName == nameof(Characteristic.CharacteristicName))
            {
                UpdateIsMetadataModifiedInEditMode(nameof(Characteristics));
            }
        }

        #region Methods

        /// <summary>
        /// If the current PresetBank's BankPath changes, we need to update ours as well. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PresetBankOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PresetBank.BankPath))
            {
                BankPath = PresetBank.BankPath;
            }
        }

        /// <summary>
        /// Updates the IsMetadataModified flag according to the configured list of properties which modify that flag.
        /// Also update the BankPath property if the PresetBank changes.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPropertyChanged(AdvancedPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.IsNewValueMeaningful && e.PropertyName == nameof(PresetBank) && PresetBank != null)
            {
                BankPath = PresetBank.BankPath;
            }

            if (IsEditing && e.IsNewValueMeaningful &&
                _propertiesWhichModifyMetadata.Contains(e.PropertyName))
            {
                if (PresetParserMetadataProperties.Contains(e.PropertyName))
                {
                    Debug.WriteLine($"in OnPropertyChanged for property {e.PropertyName}");
                    UpdateIsMetadataModifiedInEditMode(e.PropertyName);
                }
            }
        }

        #endregion

        #region Properties

        #region Basic Properties

        /// <summary>
        /// The PresetId. Always a new GUID
        /// </summary>
        [Include]
        public string PresetId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// If the preset is ignored, it will never be updated by a preset parser and will never be exported. Useful
        /// if a plugin reports empty or nonsense presets.
        /// </summary>
        [Include]
        public bool IsIgnored { get; set; }

        #endregion

        #region Plugin

        /// <summary>
        /// The plugin this preset belongs to. As soon as the plugin is set, we fill the plugins bank structure
        /// with the string representation of the bank path 
        /// </summary>
        public Plugin Plugin
        {
            get { return _plugin; }
            set
            {
                if (Equals(_plugin, value))
                {
                    return;
                }

                _plugin = value;

                if (_plugin != null)
                {
                    if (!string.IsNullOrEmpty(_bankPath) && PresetBank == null)
                    {
                        PresetBank = _plugin.RootBank.First().CreateRecursive(_bankPath);
                    }
                }
            }
        }

        private Plugin _plugin;

        #endregion

        #region PresetBank

        /// <summary>
        /// The PresetBank this preset is assigned to
        /// </summary>
        public PresetBank PresetBank
        {
            get => _presetBank;
            set
            {
                if (Equals(_presetBank, value))
                {
                    return;
                }

                if (_presetBank != null)
                {
                    _presetBank.PropertyChanged -= PresetBankOnPropertyChanged;
                }

                _presetBank = value;

                if (value != null)
                {
                    BankPath = _presetBank.BankPath;
                    _presetBank.PropertyChanged += PresetBankOnPropertyChanged;
                }
            }
        }

        private PresetBank _presetBank;

        #endregion

        #region Metadata properties

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
        public string BankPath
        {
            get => _bankPath;
            set
            {
                if (Plugin != null && (PresetBank == null || PresetBank.BankPath != value))
                {
                    PresetBank = Plugin.RootBank.First().CreateRecursive(value);
                }

                if (Equals(_bankPath, value))
                {
                    return;
                }

                var oldValue = _bankPath;
                _bankPath = value;
                OnPropertyChanged(nameof(BankPath), (object) oldValue, value);
            }
        }

        private string _bankPath;

        /// <summary>
        /// The Native Instruments types used for this plugin. Note that this is m:n relationship configured using the
        /// fluent API.
        /// </summary>
        [Include]
        public TypeCollection Types { get; set; } = new TypeCollection();


        /// <summary>
        /// The Native Instruments modes used for this plugin. Note that this is m:n relationship configured using the
        /// fluent API.
        /// </summary>
        [Include]
        public CharacteristicCollection Characteristics { get; set; } = new CharacteristicCollection();

        #endregion

        #region Preset Data Properties

        /// <summary>
        /// The SourceFile identifies where the preset came from. Usually a filename, but can be any string, depending
        /// on how the plugin stores it's presets
        /// </summary>
        [Include]
        public string SourceFile { get; set; }

        /// <summary>
        /// The preset size. Mainly used for statistics
        /// </summary>
        [Include]
        public int PresetSize { get; set; }

        /// <summary>
        /// The compressed preset size. Mainly used for statistics
        /// </summary>
        [Include]
        public int PresetCompressedSize { get; set; }

        /// <summary>
        /// The hash of the preset data. For memory usage reasons, we keep the preset data in a separate table so we
        /// can load all presets for a plugin and have a very low memory footprint. 
        /// </summary>
        [Include]
        public string PresetHash { get; set; }

        /// <summary>
        /// The preset hash we last exported. This is required because the preset data could change, but still be the
        /// same size. 
        /// </summary>
        [Include]
        public string LastExportedPresetHash { get; set; }

        /// <summary>
        /// The date and time when the preset was last exported. Mainly informational.
        /// </summary>
        [Include]
        public DateTime? LastExported { get; set; }

        /// <summary>
        /// If the metadata has been modified. Used in conjunction with the modified preset data to determinate if the
        /// preset should be exported again
        /// </summary>
        [Include]
        public bool IsMetadataModified { get; set; }

        #endregion

        #region Audio Preview Properties    

        private PreviewNotePlayer _previewNotePlayer;

        [Include]
        public PreviewNotePlayer PreviewNotePlayer
        {
            get { return _previewNotePlayer; }
            set
            {
                if (value == null)
                {
                    return;
                }

                _previewNotePlayer = PreviewNotePlayer.GetPreviewNotePlayer(value);
            }
        }

        #endregion

        #region Change Tracking

        /// <summary>
        /// Indicates if the preset has been changed since the last export.
        /// </summary>
        public bool ChangedSinceLastExport => IsMetadataModified || LastExportedPresetHash == null ||
                                              LastExportedPresetHash != PresetHash;

        #endregion

        #endregion
    }
}