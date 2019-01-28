using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using Anotar.Catel;
using Catel.Data;
using Catel.Logging;
using Drachenkatze.PresetMagician.NKSF.NKSF;
using Newtonsoft.Json;
using PresetMagician.Collections;
using PresetMagician.Models;
using PresetMagician.Models.NativeInstrumentsResources;
using PresetMagician.SharedModels;
using Path = Catel.IO.Path;

namespace SharedModels
{
    public class Plugin : ObservableObject, IPlugin
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
            Logger = new PluginLogger();
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


        public void Invalidate()
        {
            IsAnalyzed = false;
        }

        public override string ToString()
        {
            return IsLoaded ? $"{PluginVendor} {PluginName} ({PluginId})" : $"{DllPath}";
        }

        public void OnLoadError(Exception e)
        {
            LoadError = true;
            Error(e.ToString());
            Debug(e.StackTrace);
            LoadErrorMessage = e.ToString();
        }

        public void Log(string messageFormat, params object[] args)
        {
            Logger.Debug(messageFormat);
        }

        public void Debug(string messageFormat, params object[] args)
        {
            Logger.Debug(messageFormat);
        }

        public void Error(string messageFormat, params object[] args)
        {
            Logger.Debug(messageFormat);
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
                    Log(e.Message);
                }
            }
        }

        #region PresetBanks property

        /// <summary>
        /// Gets or sets the PresetBanks value.
        /// </summary>
        [NotMapped]
        public PresetBank RootBank { get; } = new PresetBank();


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

        /// <summary>
        /// Defines if the plugin had a load error
        /// </summary>
        [NotMapped]
        public bool LoadError { get; private set; }

        [NotMapped] public MemoryStream ChunkPresetMemoryStream { get; } = new MemoryStream();
        [NotMapped] public MemoryStream ChunkBankMemoryStream { get; } = new MemoryStream();

        [NotMapped] public VstPluginInfoSurrogate PluginInfo { get; set; }

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
                    Log(e.Message);
                }
            }
        }

        public Dictionary<(int, string), Preset> PresetCache = new Dictionary<(int, string), Preset>();


        /// <summary>
        /// Defines the full path to the plugin DLL
        /// </summary>
        public string DllPath => PluginLocation?.DllPath;

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
        public string DllDirectory => string.IsNullOrEmpty(DllPath) ? null : Path.GetDirectoryName(DllPath);

        /// <summary>
        /// Returns the Dll Filename without the path
        /// </summary>
        public string DllFilename => string.IsNullOrEmpty(DllPath) ? null : Path.GetFileName(DllPath);

        public string CanonicalDllFilename =>
            string.IsNullOrEmpty(DllPath) ? "Plugin DLL is missing. Last known dll path: " + LastKnownGoodDllPath: Path.GetFileName(DllPath);

        /// <summary>
        /// Defines if the current plugin is enabled
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Defines if the current plugin is being scanned.
        /// This flag is used to detect crashes
        /// </summary>
        public bool IsAnalyzing { get; set; }

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
        public ObservableCollection<BankFile> AdditionalBankFiles { get; } = new ObservableCollection<BankFile>();


        public ICollection<Type> DefaultTypes { get; set; } = new HashSet<Type>();

        public ICollection<Mode> DefaultModes { get; set; } = new HashSet<Mode>();

        public string Logs
        {
            get { return string.Join(Environment.NewLine, Logger.LogList); }
        }

        public PluginTypes PluginType { get; set; } = PluginTypes.Unknown;

        public string PluginTypeDescription => PluginType.ToString();

        public int PluginId { get; set; }

        [NotMapped] public int NumPresets => Presets.Count;

        public string PluginName { get; set; } = "";

        public int PresetParserAudioPreviewPreDelay => PresetParser?.AudioPreviewPreDelay ?? 0;

        public string PluginVendor { get; set; }

        [NotMapped] public IVendorPresetParser PresetParser { get; set; }

        /// <summary>
        /// Defines if the plugin is or was scanned
        /// </summary>
        public bool IsAnalyzed { get; set; }

        public bool HasMetadata { get; set; }

        [NotMapped] public bool IsLoaded { get; set; }


        /// <summary>
        /// Defines if the plugin is supported
        /// </summary>
        public bool IsSupported { get; set; }

        public PluginLogger Logger { get; }

        [NotMapped]
        public NativeInstrumentsResource NativeInstrumentsResource { get; set; } = new NativeInstrumentsResource();

        #endregion
    }
}