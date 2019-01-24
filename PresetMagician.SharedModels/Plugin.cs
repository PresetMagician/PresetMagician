using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Catel.Collections;
using Catel.Data;
using Drachenkatze.PresetMagician.NKSF.NKSF;
using Drachenkatze.PresetMagician.VendorPresetParser;
using Jacobi.Vst.Core;
using Jacobi.Vst.Core.Plugin;
using Jacobi.Vst.Interop.Host;
using Newtonsoft.Json;
using PresetMagician.Collections;
using PresetMagician.Models;
using PresetMagician.Models.NativeInstrumentsResources;
using PresetMagician.SharedModels;
using Path = Catel.IO.Path;

namespace SharedModels
{
    public class Plugin : ObservableObject
    {
        private int CollectionChangedCounter;
        
        public enum PluginTypes
        {
            Effect,
            Instrument,
            Unknown
        }

        public Dictionary<(int, string), Preset> PresetCache = new Dictionary<(int, string), Preset>();

        [NotMapped]
        public IRemoteVstService RemoteVstService { get; set; }
        [NotMapped]
        public bool PooledRemoteVstService { get; set; }
        #region Methods
        
        public Plugin()
        {
            Presets.CollectionChanged += PresetsOnCollectionChanged;
            RootBank.PresetBanks.Add(new PresetBank());
        }

        private void PresetsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChangedCounter != Presets.Count)
            {
                CollectionChangedCounter = Presets.Count;
                RaisePropertyChanged(nameof(NumPresets));
            }
        }
        
        public void SetPresetChunk(byte [] data, bool isPreset)
        {
            throw new Exception("Obsolete");
            // todo obsolete
            //PluginContext.PluginCommandStub.SetChunk(data, isPreset);
        }
        
        public void GetPresetChunk()
        {
            throw new Exception("Obsolete");
            // todo obsolete
            /*var data = PluginContext.PluginCommandStub.GetChunk(true);

            if (!(data is null))
            {
                ChunkPresetMemoryStream.SetLength(0);
                ChunkPresetMemoryStream.Write(data, 0, data.Length);

                RaisePropertyChanged(nameof(ChunkPresetMemoryStream));
            }

            data = PluginContext.PluginCommandStub.GetChunk(false);

            if (!(data is null))
            {
                ChunkBankMemoryStream.SetLength(0);
                ChunkBankMemoryStream.Write(data, 0, data.Length);

                RaisePropertyChanged(nameof(ChunkBankMemoryStream));
            }*/
        }
        
        public void OnLoaded()
        {
            throw new Exception("Should not be used anymore!");
        }

      

        public override string ToString()
        {
            return IsLoaded ? $"{PluginVendor} {PluginName} ({PluginId})" : $"{DllPath}";
        }

        public void OnLoadError(Exception e)
        {
            LoadError = true;
            Debug(e.ToString());
            LoadErrorMessage = e.ToString();
            LoadException = e;
        }

        #endregion

        [NotMapped]
        public string LoadErrorMessage { get; private set; }

        /// <summary>
        /// Gets or sets the table collection.
        /// </summary>
        [NotMapped]
        public List<PluginInfoItem> PluginInfoItems { get; } = new List<PluginInfoItem>();
       
        #region PresetBanks property

        /// <summary>
        /// Gets or sets the PresetBanks value.
        /// </summary>
        [NotMapped]
        public PresetBank RootBank { get; } = new PresetBank();
        
        
        public ProgressFastObservableCollection<Preset> Presets { get; set; } = new ProgressFastObservableCollection<Preset>();

        #endregion


        #region Properties
        
        [NotMapped]
        public Guid Guid { get; set; }

        [NotMapped]
        public bool LoadError { get; private set; }
        
        [NotMapped]
        public Exception LoadException { get; private set; }

        [NotMapped]
        public MemoryStream ChunkPresetMemoryStream { get; } = new MemoryStream();
        [NotMapped]
        public MemoryStream ChunkBankMemoryStream { get; } = new MemoryStream();

        [NotMapped]
        public VstPluginInfoSurrogate PluginInfo { get;set; }
        
        [Key]
        public int Id { get; set; }
        
        /// <summary>
        /// Defines the full path to the plugin DLL
        /// </summary>
        [Index(IsUnique = true)]
        public string DllPath { get; set; }
        
        /// <summary>
        /// Returns the DLL directory in which the DLL is located
        /// </summary>
        public string DllDirectory => string.IsNullOrEmpty(DllPath) ? null : Path.GetDirectoryName(DllPath);

        /// <summary>
        /// Returns the Dll Filename without the path
        /// </summary>
        public string DllFilename => string.IsNullOrEmpty(DllPath) ? null : Path.GetFileName(DllPath);
        
        /// <summary>
        /// Defines if the current plugin is enabled
        /// </summary>
        public bool IsEnabled { get; set; } = true;
        
        /// <summary>
        /// Defines if the current plugin is being scanned.
        /// This flag is used to detect crashes
        /// </summary>
        public bool IsScanning { get; set; }
        
        /// <summary>
        /// Defines if the plugin DLL is present.
        /// A plugin is present if it's DLL Path exists and it is contained within the configured paths
        /// </summary>
        public bool IsPresent { get; set; } = true;
        
        
        public int AudioPreviewPreDelay { get; set; }

        public int GetAudioPreviewDelay()
        {
            if (AudioPreviewPreDelay != 0)
            {
                return AudioPreviewPreDelay;
            }

            return PresetParserAudioPreviewPreDelay;
        }
        
        [NotMapped]
        public ControllerAssignments DefaultControllerAssignments { get; set; }

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
        
        public PluginTypes PluginType { get; set; } = PluginTypes.Unknown;

        public string PluginTypeDescription => PluginType.ToString();

        public int PluginId { get; set; }

        [NotMapped]
        public int NumPresets => Presets.Count;

        public string PluginName { get; set; } = "";

        public int PresetParserAudioPreviewPreDelay => PresetParser?.AudioPreviewPreDelay ?? 0;

        public string PluginVendor { get; set; }

        [NotMapped]
        public IVendorPresetParser PresetParser { get; set; }

        [NotMapped]
        public bool IsScanned { get; set; }

        [NotMapped]
        public bool IsLoaded { get; set; }
      


        [NotMapped]
        public bool IsSupported { get; set; }

        [NotMapped]
        public NativeInstrumentsResource NativeInstrumentsResource { get; set; } = new NativeInstrumentsResource();
        #endregion

        public string Logs
        {
            get { return string.Join(Environment.NewLine, LogList); }
        }
        
        public List<string> LogList = new List<string>();

        public void Log(string messageFormat, params object[] args)
        {
            LogList.Add( string.Format(messageFormat, args));
        }
        
        public void Debug(string messageFormat, params object[] args)
        {
            LogList.Add( string.Format(messageFormat, args));
        }
        
        public void Error(string messageFormat, params object[] args)
        {
            LogList.Add( string.Format(messageFormat, args));
        }
    }
}