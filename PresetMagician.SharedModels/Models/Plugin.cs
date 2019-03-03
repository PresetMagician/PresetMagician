using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Catel.IO;
using Catel.Runtime.Serialization;
using Drachenkatze.PresetMagician.NKSF.NKSF;
using Drachenkatze.PresetMagician.Utils;
using Newtonsoft.Json;
using PresetMagician.Models;
using PresetMagician.Models.NativeInstrumentsResources;
using PresetMagician.SharedModels;
using SharedModels.Collections;

namespace SharedModels
{
    public class Plugin : TrackableModelBase 
    {
        private int _collectionChangedCounter;
        public readonly Dictionary<string, Preset> PresetCache = new Dictionary<string, Preset>();
        public readonly Dictionary<(string hash, string sourceFile), Preset> PresetHashCache = new Dictionary<(string, string), Preset>();

        public override ICollection<string> EditableProperties { get; } = new List<string>
        {
            nameof(Presets),
            nameof(AdditionalBankFiles),
            nameof(IsEnabled),
            nameof(DefaultControllerAssignments), //todo potential issue maybe?
            nameof(DefaultModes),
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
            RootBank.PresetBanks.Add(new PresetBank { IsVirtualBank = true});
            
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
        
        private void PluginLocationOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(PluginLocation));
        }

        #endregion

        
        #region Properties
        
        #region Basic Properties
        
        /// <summary>
        /// The plugin ID for storage in the database
        /// </summary>
        [Key]
        public int Id { get; set; }
        
        /// <summary>
        /// Defines if the current plugin is enabled
        /// </summary>
        public bool IsEnabled { get; set; } = true;
        
        /// <summary>
        /// The Vst Plugin Id, which is used when matching against PluginLocations
        /// </summary>
        public int VstPluginId { get; set; }
        
        /// <summary>
        /// The type of the plugin
        /// </summary>
        public PluginTypes PluginType { get; set; } = PluginTypes.Unknown;
        
        #endregion
        
        #region Plugin Errors
        // todo refactor this
        [NotMapped]  public string LoadErrorMessage { get; private set; }
        
        /// <summary>
        /// Defines if the plugin had a load error
        /// </summary>
        [NotMapped] 
        public bool LoadError { get; private set; }
        #endregion
        
        #region Property PluginCapabilities
        
        /// <summary>
        /// The plugin capabilities
        /// </summary>
        [NotMapped]
        public List<PluginInfoItem> PluginCapabilities { get; } = new List<PluginInfoItem>();
        
        /// <summary>
        /// These are the plugin capabilities which will get serialized into the database
        /// todo check if this can be refactored to use EF6 builtin serializer only and get rid of that serializer stuff
        /// </summary>
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
                    // todo refactor this to use new bug reporter
                    Logger.Error($"Please report this as a bug: Unable to deserialize SerializedPluginCapabilities because of {e.GetType().FullName}: {e.Message}");
                    Logger.Debug(e.StackTrace);
                }
            }
        }
        #endregion
        
        #region Property Plugin Location
        
        private PluginLocation _pluginLocation;

        /// <summary>
        /// The plugin location to use. Attach event listeners if being set
        /// </summary>
        public PluginLocation PluginLocation
        {
            get => _pluginLocation;
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
        #endregion
        
        #region Plugin Info
         [NotMapped] public VstPluginInfoSurrogate PluginInfo { get; set; }

        /// <summary>
        /// These are the plugin infos which will get serialized into the database
        /// todo check if this can be refactored to use EF6 builtin serializer only and get rid of that serializer stuff
        /// </summary>
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
        #endregion

        /// <summary>
        /// Gets or sets the PresetBanks value.
        /// </summary>
        [NotMapped]
        public PresetBank RootBank { get; set; } = new PresetBank();

        
        [IncludeInSerialization]
        public TrackableCollection<Preset> Presets { get; set; } =
            new TrackableCollection<Preset>();

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
                ? "Plugin DLL is missing."
                : Path.GetFileName(DllPath);

        public string CanonicalDllDirectory =>string.IsNullOrEmpty(DllPath)
            ? "Last known dll path: " + LastKnownGoodDllPath
            : Path.GetDirectoryName(DllPath);
        
       

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
        public TrackableCollection<BankFile> AdditionalBankFiles { get; set; } = new TrackableCollection<BankFile>();


        public ICollection<Type> DefaultTypes { get; set; } = new HashSet<Type>();

        public ICollection<Mode> DefaultModes { get; set; } = new HashSet<Mode>();

        public string Logs
        {
            get { return string.Join(Environment.NewLine, Logger.LogList); }
        }

        

        public string PluginTypeDescription => PluginType.ToString();

        

        [NotMapped] public int NumPresets => Presets?.Count ?? 0;

        public string PluginName { get; set; } = "<unknown>";

        
        public int PresetParserAudioPreviewPreDelay => PresetParser?.AudioPreviewPreDelay ?? 0;

        public string PluginVendor { get; set; }

         [NotMapped] public IVendorPresetParser PresetParser { get; set; }

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

        // todo remove
        public void ClearDirtyFlag()
        {
            //IsDirty = false;
        }
        
        [NotMapped] 
        public NativeInstrumentsResource NativeInstrumentsResource { get; set; } = new NativeInstrumentsResource();

        #endregion
    }
}