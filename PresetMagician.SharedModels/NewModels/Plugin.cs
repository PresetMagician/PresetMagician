using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Catel.Data;
using Catel.IO;
using Ceras;
using Drachenkatze.PresetMagician.NKSF.NKSF;
using Drachenkatze.PresetMagician.Utils;
using PresetMagician.Models.NativeInstrumentsResources;
using SharedModels.Collections;
using ModelBase = SharedModels.Data.ModelBase;

namespace SharedModels.NewModels
{
    public class Plugin : ModelBase
    {
        private int _collectionChangedCounter;
        public readonly Dictionary<string, Preset> PresetCache = new Dictionary<string, Preset>();

        public readonly Dictionary<(string hash, string sourceFile), Preset> PresetHashCache =
            new Dictionary<(string, string), Preset>();

        public override ICollection<string> EditableProperties { get; } = new List<string>
        {
            nameof(Presets),
            nameof(AdditionalBankFiles),
            nameof(IsEnabled),
            nameof(DefaultControllerAssignments), //todo potential issue maybe?
            nameof(DefaultCharacteristics),
            nameof(DefaultTypes),
            nameof(DontReport),
            nameof(PluginLocation),
            nameof(RootBank)
        };

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
            RootBank.BankName = "Hidden Virtual Root";
            RootBank.IsVirtualBank = true;
            RootBank.PresetBanks.Add(new PresetBank {IsVirtualBank = true});

            Logger = new MiniMemoryLogger();
        }

        private void PresetsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_collectionChangedCounter != Presets.Count)
            {
                var oldValue = _collectionChangedCounter;
                _collectionChangedCounter = Presets.Count;
                OnPropertyChanged(nameof(NumPresets), oldValue, _collectionChangedCounter);
            }

            foreach (var item in e.NewItems)
            {
                ((Preset) item).Plugin = this;
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

        public override string ToString()
        {
            return HasMetadata ? $"{PluginVendor} {PluginName} ({VstPluginId})" : $"{DllPath}";
        }

        public void OnLoadError(Exception e)
        {
            LoadError = true;
            Logger.Error($"Error loading plugin because of {e.GetType().FullName}: {e.Message}");
            Logger.Debug(e.StackTrace);
            LoadErrorMessage = e.Message;
        }

        private void PluginLocationOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var adv = e as AdvancedPropertyChangedEventArgs;
            OnPropertyChanged(nameof(PluginLocation), adv.OldValue, adv.NewValue);
        }

        #endregion


        #region Properties

        #region Basic Properties

        /// <summary>
        /// The plugin ID for storage
        /// </summary>
        [Include]
        public string PluginId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Defines if the current plugin is enabled
        /// </summary>
        [Include]
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// The Vst Plugin Id, which is used when matching against PluginLocations
        /// </summary>
        [Include]
        public int VstPluginId { get; set; }

        /// <summary>
        /// The type of the plugin
        /// </summary>
        [Include]
        public PluginTypes PluginType { get; set; } = PluginTypes.Unknown;

        #endregion

        #region Plugin Errors

        // todo refactor this
         public string LoadErrorMessage { get; private set; }

        /// <summary>
        /// Defines if the plugin had a load error
        /// </summary>
        
        public bool LoadError { get; private set; }

        #endregion

        #region Property PluginCapabilities

        /// <summary>
        /// The plugin capabilities
        /// </summary>
        [Include]
        public List<PluginInfoItem> PluginCapabilities { get; private set; } = new List<PluginInfoItem>();

        #endregion

        #region Property Plugin Location

        private PluginLocation _pluginLocation;

        /// <summary>
        /// The plugin location to use. Attach event listeners if being set
        /// </summary>
        [Include]
        public PluginLocation PluginLocation
        {
            get => _pluginLocation;
            set
            {
                if (_pluginLocation == value)
                {
                    return;
                }

                if (_pluginLocation != null)
                {
                    _pluginLocation.PropertyChanged -= PluginLocationOnPropertyChanged;
                }

                var oldValue = _pluginLocation;
                _pluginLocation = value;
                if (_pluginLocation != null)
                {
                    _pluginLocation.PropertyChanged += PluginLocationOnPropertyChanged;
                }

                OnPropertyChanged(nameof(PluginLocation), oldValue, _pluginLocation);
            }
        }

        #endregion

        #region Plugin Info

        [Include] public VstPluginInfoSurrogate PluginInfo { get; set; }

        #endregion

        /// <summary>
        /// Gets or sets the PresetBanks value.
        /// </summary>
        
        public PresetBank RootBank { get; set; } = new PresetBank();

        [Include]
        public EditableCollection<Preset> Presets { get; set; } =
            new EditableCollection<Preset>();

        /// <summary>
        /// Defines the full path to the plugin DLL
        /// </summary>
        
        public string DllPath
        {
            get
            {
                if (PluginLocation == null)
                {
                    return "";
                }

                return PluginLocation.DllPath;
            }
        }

        private string _lastKnownGoodDllPath;

        [Include]
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
                ? "Plugin DLL is missing."
                : Path.GetFileName(DllPath);

        
        public string CanonicalDllDirectory => string.IsNullOrEmpty(DllPath)
            ? "Last known dll path: " + LastKnownGoodDllPath
            : Path.GetDirectoryName(DllPath);


        /// <summary>
        /// Defines if the plugin DLL is present.
        /// A plugin is present if it's DLL Path exists and it is contained within the configured paths
        /// </summary>
        
        public bool IsPresent => PluginLocation != null && PluginLocation.IsPresent;

        /// <summary>
        /// Defines the audio preview pre-delay
        /// </summary>
        [Include]
        public int AudioPreviewPreDelay { get; set; }

        [Include]
        public ControllerAssignments DefaultControllerAssignments { get; set; }

        [Include]
        public bool IsReported { get; set; }
        
        [Include]
        public bool DontReport { get; set; }

        [Include]
        public EditableCollection<BankFile> AdditionalBankFiles { get; set; } = new EditableCollection<BankFile>();

        [Include]
        public TypeCollection DefaultTypes { get; set; } = new TypeCollection();

        [Include]
        public CharacteristicCollection DefaultCharacteristics { get; set; } = new CharacteristicCollection();

        
        public string Logs
        {
            get { return string.Join(Environment.NewLine, Logger.LogList); }
        }


         public string PluginTypeDescription => PluginType.ToString();


         public int NumPresets => Presets?.Count ?? 0;

        [Include] public string PluginName { get; set; } = "<unknown>";

         public int PresetParserAudioPreviewPreDelay => PresetParser?.AudioPreviewPreDelay ?? 0;

         [Include]
        public string PluginVendor { get; set; }

         public IVendorPresetParser PresetParser { get; set; }

        /// <summary>
        /// Defines if the plugin is or was scanned
        /// </summary>
        [Include]
        public bool IsAnalyzed { get; set; }
        [Include]
        public bool HasMetadata { get; set; }

        /// <summary>
        /// Defines the PresetMagician version in which the analysis failed.
        /// </summary>
        [Include]
        public string LastFailedAnalysisVersion { get; set; }
        
        /// <summary>
        /// Defines if the plugin is supported
        /// </summary>
        [Include]
        public bool IsSupported { get; set; }

         public MiniMemoryLogger Logger { get; }

        
        public NativeInstrumentsResource NativeInstrumentsResource { get; set; } = new NativeInstrumentsResource();

        #endregion
    }
}