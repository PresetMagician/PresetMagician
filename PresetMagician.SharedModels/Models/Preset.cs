using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Anotar.Catel;
using CannedBytes.Midi.Message;
using Catel.Data;
using Catel.Runtime.Serialization;
using Ceras;
using Newtonsoft.Json;
using SharedModels.Collections;
using ZeroFormatter;

namespace SharedModels.Models
{
    public class Preset : TrackableModelBaseFoo
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
            nameof(Modes),
            nameof(PresetCompressedSize),
            nameof(PresetHash),
            nameof(PresetName),
            nameof(PresetSize),
            nameof(PreviewNoteNumber),
            nameof(Types)
        };

        /// <summary>
        /// A list of all preset metadata properties which can be set using a preset parser.
        /// </summary>
        [Exclude] public static readonly List<string> PresetParserMetadataProperties = new List<string>
        {
            nameof(Author),
            nameof(BankPath),
            nameof(Comment),
            nameof(Modes),
            nameof(PresetName),
            nameof(PreviewNoteNumber),
            nameof(Types)
        };

        [Exclude]
        public override ICollection<string> EditableProperties { get; } = new List<string>
        {
            nameof(Author),
            nameof(BankPath),
            nameof(Comment),
            nameof(Modes),
            nameof(PresetName),
            nameof(PreviewNoteNumber),
            nameof(Types)
        };

        /// <summary>
        /// Saves all properties which can be set by a preset parser, but were updated by the user
        /// </summary>
         public List<string> UserOverwrittenProperties = new List<string>();

        private bool _isEditingFromPresetParser;

        #endregion

        public Preset()
        {
            PreviewNote = new MidiNoteName();

            /*_modes.ItemPropertyChanged += WrapperOnModesCollectionItemPropertyChanged;
            _modes.CollectionChanged += WrapperOnModesCollectionChanged;
            _types.ItemPropertyChanged += WrapperOnTypesCollectionItemPropertyChanged;
            _types.CollectionChanged += WrapperOnTypesCollectionChanged;*/
        }

        /* protected override void OnBeginEdit(BeginEditEventArgs e)
         {
             base.OnBeginEdit(e);
 
             if (!e.Cancel)
             {
                 _isEditing = true;
             }
         }
 
         protected override void OnCancelEditCompleted(CancelEditCompletedEventArgs e)
         {
             base.OnCancelEditCompleted(e);
             if (!e.IsCancelOperationCanceled)
             {
                 _isEditing = false;
             }
         }
 
         protected override void OnEndEdit(EditEventArgs e)
         {
             base.OnEndEdit(e);
             if (!e.Cancel)
             {
                 _isEditing = false;
             }
         }*/

        /// <summary>
        /// Sets updated data delivered by the preset parser. Ignores user-modified properties.
        /// </summary>
        /// <param name="preset"></param>
        public void SetFromPresetParser(Preset preset)
        {
            _isEditingFromPresetParser = true;
            foreach (var property in PresetParserMetadataProperties)
            {
                if (!UserOverwrittenProperties.Contains(property))
                {
                    //SetValue(property, preset.GetValue(property));
                }
            }

            _isEditingFromPresetParser = false;
        }

        private bool ShouldTrackChanges()
        {
            return IsEditing || _isEditingFromPresetParser;
        }

        private void WrapperOnTypesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!ShouldTrackChanges())
            {
                return;
            }

            if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Remove ||
                e.Action == NotifyCollectionChangedAction.Replace)
            {
                if (IsEditing)
                {
                    UserOverwrittenProperties.Add("Types");
                }

                IsMetadataModified = true;
            }
        }

        private void WrapperOnTypesCollectionItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!ShouldTrackChanges())
            {
                return;
            }

            if (e.PropertyName == nameof(Type.Name) || e.PropertyName == nameof(Type.SubTypeName))
            {
                if (IsEditing)
                {
                    UserOverwrittenProperties.Add("Types");
                }

                IsMetadataModified = true;
            }
        }

        private void WrapperOnModesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!ShouldTrackChanges())
            {
                return;
            }

            if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Remove ||
                e.Action == NotifyCollectionChangedAction.Replace)
            {
                if (IsEditing)
                {
                    UserOverwrittenProperties.Add("Modes");
                }

                IsMetadataModified = true;
            }
        }

        private void WrapperOnModesCollectionItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!ShouldTrackChanges())
            {
                return;
            }

            if (e.PropertyName == nameof(Mode.Name))
            {
                if (IsEditing)
                {
                    UserOverwrittenProperties.Add("Modes");
                }

                IsMetadataModified = true;
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

            if (ShouldTrackChanges() && e.IsNewValueMeaningful && _propertiesWhichModifyMetadata.Contains(e.PropertyName))
            {
                if (IsEditing && PresetParserMetadataProperties.Contains(e.PropertyName))
                {
                    if (UserModifiedProperties.Contains(e.PropertyName))
                    {
                        UserOverwrittenProperties.Add(e.PropertyName);
                    }
                    else
                    {
                        UserOverwrittenProperties.Remove(e.PropertyName);
                    }
                }

                IsMetadataModified = true;
            }
        }

        private void PreviewNoteOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var adv = (AdvancedPropertyChangedEventArgs) e;

            if (e.PropertyName == nameof(MidiNoteName.NoteNumber) && adv.OldValue != adv.NewValue)
            {
                if (_propertiesWhichModifyMetadata.Contains(nameof(PreviewNoteNumber)) && ShouldTrackChanges())
                {
                    if (IsEditing && PresetParserMetadataProperties.Contains(nameof(PreviewNoteNumber)))
                    {
                        UserOverwrittenProperties.Add(e.PropertyName);
                    }

                    IsMetadataModified = true;
                }

                OnPropertyChanged(nameof(PreviewNoteNumber), adv.OldValue, adv.NewValue);
            }
        }

        #endregion

        #region Properties

        #region Basic Properties

        /// <summary>
        /// The PresetId. Always a new GUID
        /// </summary>
        [System.ComponentModel.DataAnnotations.Key]
        public string PresetId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// The Vst Plugin Id, which is required in case we lose the association with the plugin and might need to
        /// "repair" the database manually.
        /// </summary>
        public int VstPluginId { get; private set; }

        /// <summary>
        /// If the preset is ignored, it will never be updated by a preset parser and will never be exported. Useful
        /// if a plugin reports empty or nonsense presets.
        /// </summary>
        public bool IsIgnored { get; set; }

        #endregion

        #region Plugin

        /// <summary>
        /// The PluginId as foreign key
        /// </summary>
        [ForeignKey("Plugin")]
        [System.ComponentModel.DataAnnotations.Schema.Index("UniquePreset", IsUnique = true)]
        public int PluginId { get; set; }

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

                    VstPluginId = value.VstPluginId;
                }
            }
        }

        private Plugin _plugin;

        #endregion

        #region PresetBank

        /// <summary>
        /// The PresetBank this preset is assigned to
        /// </summary>
        [NotMapped]
        [Exclude]
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
        public string PresetName { get; set; }

        /// <summary>
        /// The author of the preset
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// The preset comment
        /// </summary>
        public string Comment { get; set; }


        /// <summary>
        /// The bank path. Only set via EntityFramework when loading from the database
        /// </summary>
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

        private TrackableCollection<Type> _types = new TrackableCollection<Type>();
        private TrackableCollection<Mode> _modes = new TrackableCollection<Mode>();

        /// <summary>
        /// The Native Instruments types used for this plugin. Note that this is m:n relationship configured using the
        /// fluent API.
        /// </summary>
        public TrackableCollection<Type> Types
        {
            get => _types;
            set
            {
                if (_types.Equals(value))
                {
                    return;
                }

                var oldValue = _types;


                if (_types != null)
                {
                    _types.ItemPropertyChanged -= WrapperOnTypesCollectionItemPropertyChanged;
                    _types.CollectionChanged -= WrapperOnTypesCollectionChanged;
                }

                _types = value;

                if (_types != null)
                {
                    /*_types.ItemPropertyChanged += WrapperOnTypesCollectionItemPropertyChanged;
                    _types.CollectionChanged += WrapperOnTypesCollectionChanged;*/
                }

                OnPropertyChanged(nameof(Types), oldValue, _types);
            }
        }

        /// <summary>
        /// The Native Instruments modes used for this plugin. Note that this is m:n relationship configured using the
        /// fluent API.
        /// </summary>
        public TrackableCollection<Mode> Modes
        {
            get => _modes;
            set
            {
                if (_modes.Equals(value))
                {
                    return;
                }

                var oldValue = _modes;
                if (_modes != null)
                {
                    _modes.ItemPropertyChanged -= WrapperOnModesCollectionItemPropertyChanged;
                    _modes.CollectionChanged -= WrapperOnModesCollectionChanged;
                }

                _modes = value;

                if (_modes != null)
                {
                    /*_modes.ItemPropertyChanged += WrapperOnModesCollectionItemPropertyChanged;
                    _modes.CollectionChanged += WrapperOnModesCollectionChanged;*/
                }

                OnPropertyChanged(nameof(Modes), oldValue, _modes);
            }
        }

        #endregion

        #region Preset Data Properties

        /// <summary>
        /// The SourceFile identifies where the preset came from. Usually a filename, but can be any string, depending
        /// on how the plugin stores it's presets
        /// </summary>
        [System.ComponentModel.DataAnnotations.Schema.Index("UniquePreset", IsUnique = true)]
        public string SourceFile { get; set; }

        /// <summary>
        /// The preset size. Mainly used for statistics
        /// </summary>
        public int PresetSize { get; set; }

        /// <summary>
        /// The compressed preset size. Mainly used for statistics
        /// </summary>
        public int PresetCompressedSize { get; set; }

        /// <summary>
        /// The hash of the preset data. For memory usage reasons, we keep the preset data in a separate table so we
        /// can load all presets for a plugin and have a very low memory footprint. 
        /// </summary>
        public string PresetHash { get; set; }

        /// <summary>
        /// The preset hash we last exported. This is required because the preset data could change, but still be the
        /// same size. 
        /// </summary>
        public string LastExportedPresetHash { get; set; }

        /// <summary>
        /// The date and time when the preset was last exported. Mainly informational.
        /// </summary>
        public DateTime? LastExported { get; set; }

        /// <summary>
        /// If the metadata has been modified. Used in conjunction with the modified preset data to determinate if the
        /// preset should be exported again
        /// </summary>
        public bool IsMetadataModified { get; set; }

        #endregion

        #region Audio Preview Properties    

        /// <summary>
        /// The preview note used for the audio preview. Note that we only store the midi note number in the database.
        /// </summary>
        [NotMapped]
        [Exclude]
        public MidiNoteName PreviewNote

        {
            get => _previewNote;
            set
            {
                if (_previewNote != null)
                {
                    _previewNote.PropertyChanged -= PreviewNoteOnPropertyChanged;
                }

                if (Equals(value, _previewNote)) return;
                var oldValue = _previewNote;
                _previewNote = value;

                _previewNote.PropertyChanged += PreviewNoteOnPropertyChanged;
                OnPropertyChanged(nameof(PreviewNote), oldValue, _previewNote);
                OnPropertyChanged(nameof(PreviewNoteNumber), oldValue?.NoteNumber, _previewNote?.NoteNumber);
            }
        }

        private MidiNoteName _previewNote;

        /// <summary>
        /// The preview note number which gets stored in the database.
        /// </summary>
        [Exclude]
        public int PreviewNoteNumber
        {
            get
            {
                if (PreviewNote != null)
                {
                    return PreviewNote.NoteNumber;
                }
                else
                {
                    return 0;
                }
            }
            set
            {
                if (Equals(value, PreviewNote.NoteNumber)) return;
                var oldValue = _previewNote.NoteNumber;
                PreviewNote.NoteNumber = value;
                OnPropertyChanged(nameof(PreviewNoteNumber), oldValue, _previewNote.NoteNumber);
            }
        }

        #endregion

        #region Change Tracking

        /// <summary>
        /// Stores all properties which the user has manually modified. These properties will never be updated by a preset parser
        /// </summary>
        [Column("UserModifiedMetadata")]
        [Exclude]
        // ReSharper disable once UnusedMember.Global
        public string SerializedUserModifiedMetadata
        {
            get => JsonConvert.SerializeObject(UserOverwrittenProperties);
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    try
                    {
                        UserOverwrittenProperties = JsonConvert.DeserializeObject<List<string>>(value);
                    }
                    catch (JsonReaderException e)
                    {
                        // todo refactor this to use new bug reporter
                        LogTo.Error(
                            $"Please report this as a bug: Unable to deserialize SerializedUserModifiedMetadata because of {e.GetType().FullName}: {e.Message}");
                        LogTo.Debug(e.StackTrace);
                    }
                }
            }
        }

        /// <summary>
        /// Indicates if the preset has been changed since the last export.
        /// </summary>
        [Exclude]
        public bool ChangedSinceLastExport => IsMetadataModified || LastExportedPresetHash == null ||
                                              LastExportedPresetHash != PresetHash;

        #endregion

        #endregion
    }
}