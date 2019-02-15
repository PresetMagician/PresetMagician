using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using CannedBytes.Midi.Message;
using Catel.Collections;
using Catel.Data;
using Catel.Fody;
using Catel.Runtime.Serialization;

namespace SharedModels
{
    public class Preset: ChildAwareModelBase
    {
        
       [Key] public string PresetId { get; set; } = Guid.NewGuid().ToString();

        [ForeignKey("Plugin")]
        [Index("UniquePreset", IsUnique = true)]
        public int PluginId { get; set; }

        public int VstPluginId { get; set; }

        [ExcludeFromBackup]
        public Plugin Plugin
        {
            get { return _plugin; }
            set
            {
                _plugin = value;

                if (!string.IsNullOrEmpty(_bankPath))
                {
                    PresetBank = _plugin.RootBank.First().CreateRecursive(_bankPath);
                    _bankPath = null;
                }

                VstPluginId = value.VstPluginId;
            }
        }

        private Plugin _plugin;

        public bool IsIgnored { get; set; }
        public bool IsMetadataModified { get; set; }

        public DateTime? LastExported { get; set; }

        public bool ChangedSinceLastExport
        {
            get
            {
                return IsMetadataModified || LastExportedPresetHash == null || LastExportedPresetHash != PresetHash;
            }
        }


        public void SetPlugin(Plugin vst)
        {
            Plugin = vst;
        }

        public Preset()
        {
            PreviewNote = new MidiNoteName("C5");
        }

        private PresetBank _presetBank;

        [NotMapped]
        public PresetBank PresetBank
        {
            get { return _presetBank; }
            set
            {    
                if (_presetBank != null)
                {
                    _presetBank.PropertyChanged -= PresetBankOnPropertyChanged;
                }
                
                _presetBank = value;
                _presetBank.PropertyChanged += PresetBankOnPropertyChanged;
                RaisePropertyChanged(nameof(BankPath));
            }
        }

        private void PresetBankOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PresetBank.BankPath))
            {
                RaisePropertyChanged(nameof(BankPath));
            }
        }

        private string _bankPath;

        public string BankPath
        {
            get
            {
                if (PresetBank != null)
                {
                    return PresetBank.BankPath;
                }

                return "";
            }
            set
            {
                if (Plugin != null)
                {
                    PresetBank = Plugin.RootBank.First().CreateRecursive(value);
                }
                else
                {
                    _bankPath = value;
                }
            }
        }

        [NotMapped]
        public byte[] PresetData
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public int PresetSize { get; set; }
        public int PresetCompressedSize { get; set; }

        public string PresetName { get; set; }

        [NotMapped] [ExcludeFromBackup] public MidiNoteName PreviewNote { get; set; }

        public int PreviewNoteNumber
        {
            get { return PreviewNote.NoteNumber; }
            set { PreviewNote.NoteNumber = value; }
        }

        public string Author { get; set; }
        public string Comment { get; set; }

        [Index("UniquePreset", IsUnique = true)]
        public string SourceFile { get; set; }

        [IncludeInSerialization]
        public FastObservableCollection<Type> Types { get; set; } =
            new FastObservableCollection<Type>();

        [IncludeInSerialization]
        public FastObservableCollection<Mode> Modes { get; set; } = new FastObservableCollection<Mode>();

        public string PresetHash { get; set; }
        public string LastExportedPresetHash { get; set; }
    }
}