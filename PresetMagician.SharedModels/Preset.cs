using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Text;
using Anotar.Catel;
using CannedBytes.Midi.Message;
using Catel.Collections;
using Catel.Data;
using Catel.Fody;
using Catel.Runtime.Serialization;
using Newtonsoft.Json;

namespace SharedModels
{
    public class Preset : ChildAwareModelBase
    {
        #region Fields

        /// <summary>
        /// All properties which should modify the IsMetadataModified flag.
        /// </summary>
        private static readonly HashSet<string> _propertiesWhichModifyMetadata = new HashSet<string>
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
        public static readonly HashSet<string> PresetParserMetadataProperties = new HashSet<string>
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
        public HashSet<string> UserModifiedProperties = new HashSet<string>();

        private bool _isEditing;
        private bool _isEditingFromPresetParser;

        private ChangeNotificationWrapper _modesChangeNotificationWrapper;
        private ChangeNotificationWrapper _typesChangeNotificationWrapper;

        #endregion

        public Preset()
        {
            PreviewNote = new MidiNoteName("C5");

            _modesChangeNotificationWrapper = new ChangeNotificationWrapper(Modes);
            _modesChangeNotificationWrapper.CollectionItemPropertyChanged +=
                WrapperOnModesCollectionItemPropertyChanged;
            _modesChangeNotificationWrapper.CollectionChanged += WrapperOnModesCollectionChanged;

            _typesChangeNotificationWrapper = new ChangeNotificationWrapper(Types);
            _typesChangeNotificationWrapper.CollectionItemPropertyChanged +=
                WrapperOnTypesCollectionItemPropertyChanged;
            _typesChangeNotificationWrapper.CollectionChanged += WrapperOnTypesCollectionChanged;
        }

        protected override void OnBeginEdit(BeginEditEventArgs e)
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
        }

        /// <summary>
        /// Sets updated data delivered by the preset parser. Ignores user-modified properties.
        /// </summary>
        /// <param name="preset"></param>
        public void SetFromPresetParser(Preset preset)
        {
            _isEditingFromPresetParser = true;
            foreach (var property in PresetParserMetadataProperties)
            {
                if (!UserModifiedProperties.Contains(property))
                {
                    SetValue(property, preset.GetValue(property));
                }
            }

            _isEditingFromPresetParser = false;
        }

        private bool ShouldTrackChanges()
        {
            return _isEditing || _isEditingFromPresetParser;
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
                if (_isEditing)
                {
                    UserModifiedProperties.Add("Types");
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
                if (_isEditing)
                {
                    UserModifiedProperties.Add("Types");
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
                if (_isEditing)
                {
                    UserModifiedProperties.Add("Modes");
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
                if (_isEditing)
                {
                    UserModifiedProperties.Add("Modes");
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
            if (e.IsNewValueMeaningful && e.PropertyName == nameof(PresetBank) && PresetBank != null)
            {
                BankPath = PresetBank.BankPath;
            }

            if (e.IsNewValueMeaningful && _propertiesWhichModifyMetadata.Contains(e.PropertyName) &&
                ShouldTrackChanges())
            {
                if (_isEditing && PresetParserMetadataProperties.Contains(e.PropertyName))
                {
                    UserModifiedProperties.Add(e.PropertyName);
                }

                IsMetadataModified = true;
            }

            base.OnPropertyChanged(e);
        }

        private void PreviewNoteOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MidiNoteName.NoteNumber))
            {
                if (_propertiesWhichModifyMetadata.Contains(nameof(PreviewNoteNumber)) && ShouldTrackChanges())
                {
                    if (_isEditing && PresetParserMetadataProperties.Contains(nameof(PreviewNoteNumber)))
                    {
                        UserModifiedProperties.Add(e.PropertyName);
                    }

                    IsMetadataModified = true;
                }

                RaisePropertyChanged(nameof(PreviewNoteNumber));
            }
        }

        #endregion

        #region Properties

        #region Basic Properties

        /// <summary>
        /// The PresetId. Always a new GUID
        /// </summary>
        [Key]
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
        [Index("UniquePreset", IsUnique = true)]
        public int PluginId { get; set; }

        /// <summary>
        /// The plugin this preset belongs to. As soon as the plugin is set, we fill the plugins bank structure
        /// with the string representation of the bank path 
        /// </summary>
        [IncludeInSerialization]
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
        [NotMapped][ExcludeFromBackup]
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
        [IncludeInSerialization]
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
                
                _bankPath = value;
                RaisePropertyChanged(nameof(BankPath));
            }
        }

        private string _bankPath;

        private FastObservableCollection<Type> _types = new FastObservableCollection<Type>();
        private FastObservableCollection<Mode> _modes = new FastObservableCollection<Mode>();

        /// <summary>
        /// The Native Instruments types used for this plugin. Note that this is m:n relationship configured using the
        /// fluent API.
        /// </summary>
        [IncludeInSerialization]
        public FastObservableCollection<Type> Types
        {
            get => _types;
            set
            {
                if (Equals(_types, value))
                {
                    return;
                }

                _typesChangeNotificationWrapper.UnsubscribeFromAllEvents();
                _types = value;
                _typesChangeNotificationWrapper = new ChangeNotificationWrapper(Types);
                _typesChangeNotificationWrapper.CollectionItemPropertyChanged +=
                    WrapperOnTypesCollectionItemPropertyChanged;
                _typesChangeNotificationWrapper.CollectionChanged += WrapperOnTypesCollectionChanged;
                RaisePropertyChanged(nameof(Types));
            }
        }

        /// <summary>
        /// The Native Instruments modes used for this plugin. Note that this is m:n relationship configured using the
        /// fluent API.
        /// </summary>
        [IncludeInSerialization]
        public FastObservableCollection<Mode> Modes
        {
            get => _modes;
            set
            {
                if (Equals(_modes, value))
                {
                    return;
                }

                _modesChangeNotificationWrapper.UnsubscribeFromAllEvents();
                _modes = value;

                _modesChangeNotificationWrapper = new ChangeNotificationWrapper(_modes);
                _modesChangeNotificationWrapper.CollectionItemPropertyChanged +=
                    WrapperOnModesCollectionItemPropertyChanged;
                _modesChangeNotificationWrapper.CollectionChanged += WrapperOnModesCollectionChanged;
                RaisePropertyChanged(nameof(Modes));
            }
        }

        #endregion

        #region Preset Data Properties

        /// <summary>
        /// The SourceFile identifies where the preset came from. Usually a filename, but can be any string, depending
        /// on how the plugin stores it's presets
        /// </summary>
        [Index("UniquePreset", IsUnique = true)]
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
        [ExcludeFromBackup]
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
                _previewNote = value;
                _previewNote.PropertyChanged += PreviewNoteOnPropertyChanged;
                RaisePropertyChanged(nameof(PreviewNote));
                RaisePropertyChanged(nameof(PreviewNoteNumber));
            }
        }

        private MidiNoteName _previewNote;

        /// <summary>
        /// The preview note number which gets stored in the database.
        /// </summary>
        public int PreviewNoteNumber
        {
            get { return PreviewNote.NoteNumber; }
            set
            {
                if (Equals(value, PreviewNote.NoteNumber)) return;
                PreviewNote.NoteNumber = value;
                RaisePropertyChanged(nameof(PreviewNoteNumber));
            }
        }

        #endregion

        #region Change Tracking

        /// <summary>
        /// Stores all properties which the user has manually modified. These properties will never be updated by a preset parser
        /// </summary>
        [Column("UserModifiedMetadata")]
        [ExcludeFromBackup]
        // ReSharper disable once UnusedMember.Global
        public string SerializedUserModifiedMetadata
        {
            get => JsonConvert.SerializeObject(UserModifiedProperties);
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    try
                    {
                        UserModifiedProperties = JsonConvert.DeserializeObject<HashSet<string>>(value);
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
        public bool ChangedSinceLastExport => IsMetadataModified || LastExportedPresetHash == null ||
                                              LastExportedPresetHash != PresetHash;

        #endregion

        #endregion
    }
}