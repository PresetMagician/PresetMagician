using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using Catel.Data;
using Catel.Fody;
using Catel.Logging;
using Drachenkatze.PresetMagician.NKSF.NKSF;
using Drachenkatze.PresetMagician.Utils;
using Newtonsoft.Json;
using PresetMagician.Collections;
using PresetMagician.Models;
using PresetMagician.Models.NativeInstrumentsResources;
using PresetMagician.SharedModels;
using Path = Catel.IO.Path;

namespace SharedModels
{
    public class Plugin : ModelBase
    {
        private int _collectionChangedCounter;

        public enum PluginTypes
        {
            Effect,
            Instrument,
            Unknown
        }

        #region Methods

        public Plugin()
        {
            Presets.CollectionChanged += PresetsOnCollectionChanged;
            RootBank.PresetBanks.Add(new PresetBank());
            Logger = new MiniMemoryLogger();
        }

        private void PresetsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_collectionChangedCounter != Presets.Count)
            {
                _collectionChangedCounter = Presets.Count;
                RaisePropertyChanged(nameof(NumPresets));
            }
        }

        public int GetAudioPreviewDelay()
        {
            if (AudioPreviewPreDelay != 0)
            {
                return AudioPreviewPreDelay;
            }

            return PresetParserAudioPreviewPreDelay;
        }

        public bool HasPreset(string sourceFile, string hash)
        {
            return PresetHashCache.ContainsKey((hash, sourceFile));
        }

        public void Invalidate()
        {
            IsAnalyzed = false;
        }

        public override string ToString()
        {
            return IsLoaded ? $"{PluginVendor} {PluginName} ({VstPluginId})" : $"{DllPath}";
        }

        public void OnLoadError(Exception e)
        {
            LoadError = true;
            Logger.Error($"Error loading plugin because of {e.GetType().FullName}: {e.Message}");
            Logger.Debug(e.StackTrace);
            LoadErrorMessage = e.Message;
        }


        #endregion

        [NotMapped] public string LoadErrorMessage { get; private set; }

        /// <summary>
        /// Gets or sets the table collection.
        /// </summary>
        [NotMapped]
        public List<PluginInfoItem> PluginCapabilities { get; } = new List<PluginInfoItem>();

        [Column("PluginCapabilities")]
        public string SerializedPluginCapabilities
        {
            get => JsonConvert.SerializeObject(PluginCapabilities);
            set
            {
                try
                {
                    var capabilities = JsonConvert.DeserializeObject<List<PluginInfoItem>>(value);
                    PluginCapabilities.Clear();
                    PluginCapabilities.AddRange(capabilities);
                }
                catch (JsonReaderException e)
                {
                    Logger.Error($"Please report this as a bug: Unable to deserialize SerializedPluginCapabilities because of {e.GetType().FullName}: {e.Message}");
                    Logger.Debug(e.StackTrace);
                }
            }
        }

        #region PresetBanks property

        /// <summary>
        /// Gets or sets the PresetBanks value.
        /// </summary>
        [NotMapped]
        public PresetBank RootBank { get; } = new PresetBank();


        [ExcludeFromBackup]
        public ProgressFastObservableCollection<Preset> Presets { get; set; } =
            new ProgressFastObservableCollection<Preset>();

        #endregion


        #region Properties

        /// <summary>
        /// The plugin ID for storage in the database
        /// </summary>
        [Key]
        public int Id { get; set; }

        private PluginLocation _pluginLocation;

        public PluginLocation PluginLocation
        {
            get { return _pluginLocation; }
            set
            {
                if (_pluginLocation != null)
                {
                    _pluginLocation.PropertyChanged -= PluginLocationOnPropertyChanged;
                }

                _pluginLocation = value;
                if (_pluginLocation != null)
                {
                    _pluginLocation.PropertyChanged += PluginLocationOnPropertyChanged;
                }

                RaisePropertyChanged(nameof(PluginLocation));
            }
        }

        private void PluginLocationOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(PluginLocation));
        }
        
        [NotMapped]
        public bool HasPresets
        {
            get
            {
                if (Presets != null)
                {
                    return Presets.Count > 0;
                }

                return false;
            }
        }

        /// <summary>
        /// Defines if the plugin had a load error
        /// </summary>
        [NotMapped]
        public bool LoadError { get; private set; }

        [NotMapped] public MemoryStream ChunkPresetMemoryStream { get; } = new MemoryStream();
        [NotMapped] public MemoryStream ChunkBankMemoryStream { get; } = new MemoryStream();

        [ExcludeFromBackup] [NotMapped] public VstPluginInfoSurrogate PluginInfo { get; set; }

        [Column("PluginInfo")]
        public string SerializedPluginInfo
        {
            get => JsonConvert.SerializeObject(PluginInfo);
            set
            {
                try
                {
                    PluginInfo = JsonConvert.DeserializeObject<VstPluginInfoSurrogate>(value);
                }
                catch (JsonReaderException e)
                {
                    Logger.Error($"Please report this as a bug: Unable to deserialize SerializedPluginInfo because of {e.GetType().FullName}: {e.Message}");
                    Logger.Debug(e.StackTrace);
                }
            }
        }

        public Dictionary<string, Preset> PresetCache = new Dictionary<string, Preset>();
        public Dictionary<(string hash, string sourceFile), Preset> PresetHashCache = new Dictionary<(string, string), Preset>();


        /// <summary>
        /// Defines the full path to the plugin DLL
        /// </summary>
        public string DllPath
        {
            get {
                if (PluginLocation == null)
                {
                    return "";
                }

                return PluginLocation.DllPath;

            } 
        }

        private string _lastKnownGoodDllPath;

        public string LastKnownGoodDllPath
        {
            get
            {
                if (PluginLocation != null && PluginLocation.IsPresent)
                {
                    _lastKnownGoodDllPath = PluginLocation.DllPath;
                }

                return _lastKnownGoodDllPath;
            }
            set => _lastKnownGoodDllPath = value;
        }


        /// <summary>
        /// Returns the DLL directory in which the DLL is located
        /// </summary>
        public string DllDirectory => string.IsNullOrEmpty(DllPath) ? "" : Path.GetDirectoryName(DllPath);

        /// <summary>
        /// Returns the Dll Filename without the path
        /// </summary>
        public string DllFilename => string.IsNullOrEmpty(DllPath) ? "" : Path.GetFileName(DllPath);

        public string CanonicalDllFilename =>
            string.IsNullOrEmpty(DllPath)
                ? "Plugin DLL is missing. Last known dll path: " + LastKnownGoodDllPath
                : Path.GetFileName(DllPath);

        /// <summary>
        /// Defines if the current plugin is enabled
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Defines if the plugin DLL is present.
        /// A plugin is present if it's DLL Path exists and it is contained within the configured paths
        /// </summary>
        [NotMapped]
        public bool IsPresent => PluginLocation != null && PluginLocation.IsPresent;


        public int AudioPreviewPreDelay { get; set; }


        [NotMapped] public ControllerAssignments DefaultControllerAssignments { get; set; }

        [Column("DefaultControllerAssignments")]
        // ReSharper disable once UnusedMember.Global
        public string SerializedDefaultControllerAssignments
        {
            get => JsonConvert.SerializeObject(DefaultControllerAssignments);
            set => DefaultControllerAssignments = JsonConvert.DeserializeObject<ControllerAssignments>(value);
        }

        public bool IsReported { get; set; }
        public bool DontReport { get; set; }
        public ProgressFastObservableCollection<BankFile> AdditionalBankFiles { get; set; } = new ProgressFastObservableCollection<BankFile>();


        public ICollection<Type> DefaultTypes { get; set; } = new HashSet<Type>();

        public ICollection<Mode> DefaultModes { get; set; } = new HashSet<Mode>();

        public string Logs
        {
            get { return string.Join(Environment.NewLine, Logger.LogList); }
        }

        public PluginTypes PluginType { get; set; } = PluginTypes.Unknown;

        public string PluginTypeDescription => PluginType.ToString();

        public int VstPluginId { get; set; }

        [NotMapped] public int NumPresets => Presets?.Count ?? 0;

        public string PluginName { get; set; } = "<unknown>";

        public int PresetParserAudioPreviewPreDelay => PresetParser?.AudioPreviewPreDelay ?? 0;

        public string PluginVendor { get; set; }

        [ExcludeFromBackup] [NotMapped] public IVendorPresetParser PresetParser { get; set; }

        /// <summary>
        /// Defines if the plugin is or was scanned
        /// </summary>
        public bool IsAnalyzed { get; set; }

        public bool HasMetadata { get; set; }
        
        /// <summary>
        /// Defines if the metadata scan did not yield any result in the current sessions
        /// </summary>
        [NotMapped] public bool MetadataUnavailableInCurrentSession { get; set; }

        [NotMapped] public bool IsLoaded { get; set; }


        /// <summary>
        /// Defines if the plugin is supported
        /// </summary>
        public bool IsSupported { get; set; }

        public MiniMemoryLogger Logger { get; }

        [NotMapped]
        public NativeInstrumentsResource NativeInstrumentsResource { get; set; } = new NativeInstrumentsResource();

        #endregion
    }
}