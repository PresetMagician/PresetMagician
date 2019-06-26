using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Catel.Data;
using Catel.IO;
using Ceras;
using PresetMagician.Core.Collections;
using PresetMagician.Core.Interfaces;
using PresetMagician.Core.Models.NativeInstrumentsResources;
using PresetMagician.NKS;
using PresetMagician.Utils.Logger;
using ModelBase = PresetMagician.Core.Data.ModelBase;

namespace PresetMagician.Core.Models
{
    public class Plugin : ModelBase
    {
        private int _collectionChangedCounter;

        public readonly Dictionary<(string hash, string sourceFile), Preset> PresetHashCache =
            new Dictionary<(string, string), Preset>();

        public override HashSet<string> GetEditableProperties()
        {
            return _editableProperties;
        }

        private static HashSet<string> _editableProperties { get; } = new HashSet<string>
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

        /// <summary>
        /// Ensures that if the current plugin location isn't present, but another plugin location is,
        /// that the present plugin location is set.
        /// </summary>
        public void EnsurePluginLocationIsPresent()
        {
            if (IsPresent)
            {
                return;
            }

            PluginLocation =
                (from p in PluginLocations
                    where p.IsPresent && p.HasMetadata && p.PresetParser != null
                    orderby p.PresetParser.Priority descending
                    select p).FirstOrDefault();
        }

        private void PresetsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_collectionChangedCounter != Presets.Count)
            {
                var oldValue = _collectionChangedCounter;
                _collectionChangedCounter = Presets.Count;
                OnPropertyChanged(nameof(NumPresets), oldValue, _collectionChangedCounter);
            }

            if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Replace)
            {
                foreach (var item in e.NewItems)
                {
                    ((Preset) item).Plugin = this;
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                foreach (var item in Presets)
                {
                    item.Plugin = this;
                }
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

        public override string ToString()
        {
            return HasMetadata ? $"{PluginVendor} {PluginName} ({VstPluginId})" : $"{DllPath}";
        }

        public void LogPluginError(string action, Exception e)
        {
            LoadError = true;
            Logger.Error($"Error while {action}: ({e.GetType().FullName}) {e.Message}");
            Logger.LogException(e);
            LoadErrorMessage = e.Message;
        }

        private void PluginLocationOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var adv = e as AdvancedPropertyChangedEventArgs;

            if (e.PropertyName == nameof(PluginLocation.IsPresent))
            {
                OnPropertyChanged(nameof(IsPresent), adv.OldValue, adv.NewValue);
            }

            if (e.PropertyName == nameof(PluginLocation.PresetParser))
            {
                OnPropertyChanged(nameof(PresetParser), adv.OldValue, adv.NewValue);
            }

            if (e.PropertyName == nameof(PluginLocation.HasMetadata))
            {
                OnPropertyChanged(nameof(HasMetadata), adv.OldValue, adv.NewValue);
            }

            SyncMetadataFromPluginLocation();
        }

        private void SyncMetadataFromPluginLocation()
        {
            if (PluginLocation != null)
            {
                PluginName = PluginLocation.PluginName;
                PluginVendor = PluginLocation.PluginVendor;
            }
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
        public List<PluginInfoItem> PluginCapabilities { get; set; } = new List<PluginInfoItem>();

        #endregion

        #region Property Plugin Location

        [Include] public HashSet<PluginLocation> PluginLocations { get; set; } = new HashSet<PluginLocation>();

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

                var oldIsPreset = IsPresent;
                var oldValue = _pluginLocation;
                var oldPresetParser = PresetParser;
                _pluginLocation = value;
                if (_pluginLocation != null)
                {
                    _pluginLocation.PropertyChanged += PluginLocationOnPropertyChanged;
                    PluginLocations.Add(_pluginLocation);
                }

                SyncMetadataFromPluginLocation();

                OnPropertyChanged(nameof(PluginLocation), oldValue, _pluginLocation);
                OnPropertyChanged(nameof(IsPresent), oldIsPreset, IsPresent);
                OnPropertyChanged(nameof(PresetParser), oldPresetParser, PresetParser);
            }
        }

        #endregion

        #region Plugin Info

        [Include] public VstPluginInfoSurrogate PluginInfo { get; set; }

        #endregion

        /// <summary>
        /// Gets or sets the PresetBanks value.
        /// </summary>

        public PresetBank RootBank { get; } = new PresetBank();

        private EditableCollection<Preset> _presets = new EditableCollection<Preset>();

        public EditableCollection<Preset> Presets
        {
            get { return _presets; }
            set
            {
                if (ReferenceEquals(_presets, value))
                {
                    return;
                }

                if (_presets != null)
                {
                    _presets.CollectionChanged -= PresetsOnCollectionChanged;
                }

                _presets = value;
                foreach (var preset in _presets)
                {
                    preset.Plugin = this;
                }

                _presets.CollectionChanged += PresetsOnCollectionChanged;
            }
        }


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
                ? "Plugin DLL missing or changed"
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

        [Include] public ControllerAssignments DefaultControllerAssignments { get; set; }

        [Include] public bool IsReported { get; set; }

        [Include] public bool DontReport { get; set; }

        [Include]
        public EditableCollection<BankFile> AdditionalBankFiles { get; set; } = new EditableCollection<BankFile>();

        [Include] public TypeCollection DefaultTypes { get; set; } = new TypeCollection();

        [Include] public CharacteristicCollection DefaultCharacteristics { get; set; } = new CharacteristicCollection();


        public string Logs
        {
            get { return string.Join(Environment.NewLine, Logger.LogList); }
        }


        public string PluginTypeDescription => PluginType.ToString();


        public int NumPresets => Presets?.Count ?? 0;

        [Include] public string PluginName { get; set; } = "<unknown>";
        [Include] public string OverriddenPluginName { get; set; } = "";
        [Include] public bool OverridePluginName { get; set; }


        public int PresetParserAudioPreviewPreDelay => PresetParser?.AudioPreviewPreDelay ?? 0;

        [Include] public string PluginVendor { get; set; }

        public IVendorPresetParser PresetParser => PluginLocation?.PresetParser;

        public bool HasMetadata => PluginLocation != null && PluginLocation.HasMetadata;


        public bool RequiresMetadataScan { get; private set; }

        public void UpdateRequiresMetadataScanFlag(string currentVersion)
        {
            RequiresMetadataScan = false;
            if (!IsEnabled)
            {
                return;
            }


            foreach (var pluginLocation in PluginLocations)
            {
                if (!pluginLocation.IsPresent)
                {
                    continue;
                }

                // Unsure if a missing preset parser should trigger a metadata rescan
                //  || pluginLocation.PresetParser == null
                if (pluginLocation.LastMetadataAnalysisVersion != currentVersion)
                {
                    RequiresMetadataScan = true;
                    return;
                }
            }
        }

        /// <summary>
        /// Defines if the plugin is supported
        /// </summary>
        [Include]
        public bool IsSupported { get; set; }

        public MiniMemoryLogger Logger { get; }


        public NativeInstrumentsResource NativeInstrumentsResource { get; set; } = new NativeInstrumentsResource();

        public void OnBeforeCerasSerialize()
        {
            foreach (var preset in Presets)
            {
                preset.OnBeforeCerasSerialize();
            }
        }

        public void OnAfterCerasDeserialize()
        {
            foreach (var preset in Presets)
            {
                preset.OnAfterCerasDeserialize();
            }
        }

        #endregion
    }
}