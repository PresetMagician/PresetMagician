using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using Catel.Data;
using Ceras;
using PresetMagician.Core.Extensions;
using ModelBase = PresetMagician.Core.Data.ModelBase;

namespace PresetMagician.Core.Models
{
    public class Preset : ModelBase
    {
        #region Fields

        public override HashSet<string> GetEditableProperties()
        {
            return _editableProperties;
        }

        private static HashSet<string> _editableProperties { get; } = new HashSet<string>
        {
            nameof(PreviewNotePlayer),
            nameof(IsIgnored),
            nameof(Metadata)
        };

        private string _bankPath;

        private void SetBankPath(string value)
        {
            if (Plugin != null && (PresetBank == null || PresetBank.BankPath != value))
            {
                PresetBank = Plugin.RootBank.First().CreateRecursive(value);
            }
            else
            {
                _bankPath = value;
            }
        }

        #endregion

        public Preset()
        {
            _metadata.PropertyChanged += MetadataOnPropertyChanged;
        }

        /// <summary>
        /// Sets updated data delivered by the preset parser. Ignores user-modified properties.
        /// </summary>
        /// <param name="preset"></param>
        public void SetFromPresetParser(PresetParserMetadata presetMetadata)
        {
            OriginalMetadata = presetMetadata;
            Metadata.ApplyFrom(OriginalMetadata);
            UpdateIsMetadataModified();
        }

        /// <summary>
        /// Sets updated data delivered by the preset parser. Ignores user-modified properties.
        /// </summary>
        /// <param name="preset"></param>
        public void UpdateLastExportedMetadata()
        {
            LastExportedMetadata.BankPath = Metadata.BankPath;
            LastExportedMetadata.Author = Metadata.Author;
            LastExportedMetadata.Comment = Metadata.Comment;
            LastExportedMetadata.PresetName = Metadata.PresetName;

            LastExportedMetadata.Types.Clear();

            foreach (var type in Metadata.Types)
            {
                if (!type.IsIgnored)
                {
                    LastExportedMetadata.Types.Add(new Type
                        {TypeName = type.EffectiveTypeName, SubTypeName = type.EffectiveSubTypeName});
                }
            }

            LastExportedMetadata.Characteristics.Clear();

            foreach (var characteristic in Metadata.Characteristics)
            {
                if (!characteristic.IsIgnored)
                {
                    LastExportedMetadata.Characteristics.Add(new Characteristic
                        {CharacteristicName = characteristic.EffectiveCharacteristicName});
                }
            }

            LastExportedMetadata.PreviewNotePlayer = PreviewNotePlayer;
            LastExportedMetadata.PresetHash = PresetHash;
            LastExported = DateTime.Now;

            UpdateIsMetadataModified();
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
                Metadata.BankPath = PresetBank.BankPath;
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

            if (e.PropertyName == nameof(PreviewNotePlayer) || e.PropertyName == nameof(PresetHash))
            {
                UpdateIsMetadataModified();
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
                if (ReferenceEquals(_plugin, value))
                {
                    return;
                }

                if (_plugin != null && value == null)
                {
                    Debug.WriteLine("FOO");
                }

                _plugin = value;

                if (_plugin != null)
                {
                    if (!string.IsNullOrEmpty(_bankPath) && (PresetBank == null || PresetBank.BankPath != _bankPath))
                    {
                        PresetBank = _plugin.RootBank.First().CreateRecursive(_bankPath);
                        _bankPath = null;
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
                if (ReferenceEquals(_presetBank, value))
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
                    Metadata.BankPath = _presetBank.BankPath;
                    _presetBank.PropertyChanged += PresetBankOnPropertyChanged;
                }
            }
        }

        private PresetBank _presetBank;

        #endregion

        #region Preset Data Properties

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
        
        [Include]
        public bool IsMetadataUserModified { get; set; }
        
        public PresetMetadataModifiedProperties PresetMetadataModifiedProperties { get; } = new PresetMetadataModifiedProperties();

        /// <summary>
        /// The metadata from the preset parser 
        /// </summary>
        [Include]
        public PresetParserMetadata OriginalMetadata { get; set; } = new PresetParserMetadata();

        private EditablePresetMetadata _metadata = new EditablePresetMetadata();

        /// <summary>
        /// The current metadata
        /// </summary>
        [Include]
        public EditablePresetMetadata Metadata
        {
            get { return _metadata; }
            set
            {
                if (ReferenceEquals(_metadata, value))
                {
                    return;
                }

                _metadata.PropertyChanged -= MetadataOnPropertyChanged;
                _metadata = value;
                SetBankPath(_metadata.BankPath);
                _metadata.PropertyChanged += MetadataOnPropertyChanged;
            }
        }

        private void MetadataOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PresetMetadata.BankPath))
            {
                SetBankPath(_metadata.BankPath);
            }

            if (IsEditing && Metadata.GetEditableProperties()
                    .Contains(e.PropertyName))
            {
                if (Metadata.IsEqualTo(OriginalMetadata, e.PropertyName))
                {
                    Metadata.UserOverwrittenProperties.Remove(e.PropertyName);
                }
                else
                {
                    Metadata.UserOverwrittenProperties.Add(e.PropertyName);
                }
            }

            UpdateIsMetadataModified();
        }

        private void UpdateIsMetadataModified()
        {
            IsMetadataModified = !(_metadata.IsEqualTo(LastExportedMetadata) &&
                                   PresetHash == LastExportedMetadata.PresetHash &&
                                   LastExportedMetadata.PreviewNotePlayer.PreviewNotePlayerId ==
                                   PreviewNotePlayer.PreviewNotePlayerId);
            _metadata.UpdateModified(OriginalMetadata, PresetMetadataModifiedProperties);
            IsMetadataUserModified = PresetMetadataModifiedProperties.IsModified();
        }

        /// <summary>
        /// The metadata last exported 
        /// </summary>
        [Include]
        public ExportedPresetMetadata LastExportedMetadata { get; set; } = new ExportedPresetMetadata();

        #endregion

        #region Audio Preview Properties    

        private PreviewNotePlayer _previewNotePlayer = PreviewNotePlayer.Default;

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

        #endregion

        #endregion
    }
}