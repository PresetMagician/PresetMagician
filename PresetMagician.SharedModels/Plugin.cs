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
using Catel.Data;
using Drachenkatze.PresetMagician.NKSF.NKSF;
using Drachenkatze.PresetMagician.VendorPresetParser;
using Jacobi.Vst.Core;
using Jacobi.Vst.Interop.Host;
using Newtonsoft.Json;
using PresetMagician.Models;
using PresetMagician.Models.NativeInstrumentsResources;
using PresetMagician.SharedModels;
using Path = Catel.IO.Path;

namespace SharedModels
{
    public class Plugin : ObservableObject, IDisposable
    {
        private int CollectionChangedCounter;
        
        public enum PluginTypes
        {
            Effect,
            Instrument,
            Unknown
        }

        public Dictionary<(int, string), Preset> PresetCache = new Dictionary<(int, string), Preset>();

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
            
            PluginContext.PluginCommandStub.SetChunk(data, isPreset);
        }
        
        public void GetPresetChunk()
        {
            var data = PluginContext.PluginCommandStub.GetChunk(true);

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
            }
        }
        
        public void OnLoaded()
        {
            PluginName = PluginContext.PluginCommandStub.GetEffectName();

            if (string.IsNullOrEmpty(PluginName))
            {
                PluginName = PluginContext.PluginCommandStub.GetProductString();

                if (string.IsNullOrEmpty(PluginName))
                {
                    // Extreme fallback: Use plugin DLL name
                    PluginName = DllFilename.Replace(".dll", "");
                }
            }
            
            PluginVendor = PluginContext.PluginCommandStub.GetVendorString();
            
            if (string.IsNullOrEmpty(PluginVendor))
            {
                PluginVendor = "Unknown";
            }
            
            PluginId = PluginContext.PluginInfo.PluginID;

            if (PluginContext.PluginInfo.Flags.HasFlag(VstPluginFlags.IsSynth))
            {
                PluginType = PluginTypes.Instrument;
            }
            else
            {
                PluginType = PluginTypes.Effect;
            }

            PopulatePluginInfoItems();
        }

        public void Dispose()
        {
            PluginContext?.Dispose();
            PluginContext = null;
        }

        public static string PluginIdNumberToIdString(int pluginUniqueId)
        {
            byte[] fxIdArray = BitConverter.GetBytes(pluginUniqueId);
            Array.Reverse(fxIdArray);
            string fxIdString = Encoding.Default.GetString(fxIdArray);
            return fxIdString;
        }

        public override string ToString()
        {
            return IsLoaded ? $"{PluginVendor} {PluginName} ({PluginId})" : $"{DllPath}";
        }

        public void OnLoadError(Exception e)
        {
            LoadError = true;
            LoadErrorMessage = e.ToString();
            LoadException = e;
        }

        public void PopulatePluginInfoItems()
        {
            _pluginInfoItems = new ObservableCollection<PluginInfoItem>();

            var pluginContext = PluginContext;

            if (pluginContext != null)
            {
                // plugin product
                _pluginInfoItems.Add(new PluginInfoItem("Base", "Plugin Name",
                    pluginContext.PluginCommandStub.GetEffectName()));
                _pluginInfoItems.Add(new PluginInfoItem("Base", "Product ",
                    pluginContext.PluginCommandStub.GetProductString()));
                _pluginInfoItems.Add(new PluginInfoItem("Base", "Vendor ",
                    pluginContext.PluginCommandStub.GetVendorString()));
                _pluginInfoItems.Add(new PluginInfoItem("Base", "Vendor Version ",
                    pluginContext.PluginCommandStub.GetVendorVersion().ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("Base", "Vst Support ",
                    pluginContext.PluginCommandStub.GetVstVersion().ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("Base", "Plugin Category ",
                    pluginContext.PluginCommandStub.GetCategory().ToString()));

                // plugin info
                _pluginInfoItems.Add(new PluginInfoItem("Base", "Flags ", pluginContext.PluginInfo.Flags.ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("Base", "Plugin ID ",
                    pluginContext.PluginInfo.PluginID.ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("Base", "Plugin ID String",
                    PluginIdNumberToIdString(pluginContext.PluginInfo.PluginID)));

                _pluginInfoItems.Add(new PluginInfoItem("Base", "Plugin Version ",
                    pluginContext.PluginInfo.PluginVersion.ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("Base", "Audio Input Count ",
                    pluginContext.PluginInfo.AudioInputCount.ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("Base", "Audio Output Count ",
                    pluginContext.PluginInfo.AudioOutputCount.ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("Base", "Initial Delay ",
                    pluginContext.PluginInfo.InitialDelay.ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("Base", "Program Count ",
                    pluginContext.PluginInfo.ProgramCount.ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("Base", "Parameter Count ",
                    pluginContext.PluginInfo.ParameterCount.ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("Base", "Tail Size ",
                    pluginContext.PluginCommandStub.GetTailSize().ToString()));

                // can do
                _pluginInfoItems.Add(new PluginInfoItem("CanDo", nameof(VstPluginCanDo.Bypass),
                    pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.Bypass)).ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("CanDo", nameof(VstPluginCanDo.MidiProgramNames),
                    pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.MidiProgramNames))
                        .ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("CanDo", nameof(VstPluginCanDo.Offline),
                    pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.Offline)).ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("CanDo", nameof(VstPluginCanDo.ReceiveVstEvents),
                    pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.ReceiveVstEvents))
                        .ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("CanDo", nameof(VstPluginCanDo.ReceiveVstMidiEvent),
                    pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.ReceiveVstMidiEvent))
                        .ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("CanDo", nameof(VstPluginCanDo.ReceiveVstTimeInfo),
                    pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.ReceiveVstTimeInfo))
                        .ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("CanDo", nameof(VstPluginCanDo.SendVstEvents),
                    pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.SendVstEvents))
                        .ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("CanDo", nameof(VstPluginCanDo.SendVstMidiEvent),
                    pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.SendVstMidiEvent))
                        .ToString()));

                _pluginInfoItems.Add(new PluginInfoItem("CanDo", nameof(VstPluginCanDo.ConformsToWindowRules),
                    pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.ConformsToWindowRules))
                        .ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("CanDo", nameof(VstPluginCanDo.Metapass),
                    pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.Metapass))
                        .ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("CanDo", nameof(VstPluginCanDo.MixDryWet),
                    pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.MixDryWet))
                        .ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("CanDo", nameof(VstPluginCanDo.Multipass),
                    pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.Multipass))
                        .ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("CanDo", nameof(VstPluginCanDo.NoRealTime),
                    pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.NoRealTime))
                        .ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("CanDo", nameof(VstPluginCanDo.PlugAsChannelInsert),
                    pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.PlugAsChannelInsert))
                        .ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("CanDo", nameof(VstPluginCanDo.PlugAsSend),
                    pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.PlugAsSend))
                        .ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("CanDo", nameof(VstPluginCanDo.SendVstTimeInfo),
                    pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.SendVstTimeInfo))
                        .ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("CanDo", nameof(VstPluginCanDo.x1in1out),
                    pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x1in1out))
                        .ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("CanDo", nameof(VstPluginCanDo.x1in2out),
                    pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x1in2out))
                        .ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("CanDo", nameof(VstPluginCanDo.x2in1out),
                    pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x2in1out))
                        .ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("CanDo", nameof(VstPluginCanDo.x2in2out),
                    pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x2in2out))
                        .ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("CanDo", nameof(VstPluginCanDo.x2in4out),
                    pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x2in4out))
                        .ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("CanDo", nameof(VstPluginCanDo.x4in2out),
                    pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x4in2out))
                        .ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("CanDo", nameof(VstPluginCanDo.x4in4out),
                    pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x4in4out))
                        .ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("CanDo", nameof(VstPluginCanDo.x4in8out),
                    pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x4in8out))
                        .ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("CanDo", nameof(VstPluginCanDo.x8in4out),
                    pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x8in4out))
                        .ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("CanDo", nameof(VstPluginCanDo.x8in8out),
                    pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x8in8out))
                        .ToString()));

                _pluginInfoItems.Add(new PluginInfoItem("Program", "Current Program Index",
                    pluginContext.PluginCommandStub.GetProgram().ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("Program", "Current Program Name",
                    pluginContext.PluginCommandStub.GetProgramName()));
            }
        }
        
        #endregion

        [NotMapped]
        public string LoadErrorMessage { get; private set; }

        /// <summary>
        /// Gets or sets the table collection.
        /// </summary>
        [NotMapped]
        public List<PluginInfoItem> PluginInfoItems
        {
            get { return _pluginInfoItems.ToList(); }
        }

        private ObservableCollection<PluginInfoItem> _pluginInfoItems = new ObservableCollection<PluginInfoItem>();

        #region PresetBanks property

        /// <summary>
        /// Gets or sets the PresetBanks value.
        /// </summary>
        [NotMapped]
        public PresetBank RootBank { get; } = new PresetBank();
        
        
        public ObservableCollection<Preset> Presets { get; set; } = new ObservableCollection<Preset>();

        #endregion


        #region Properties

        [NotMapped]
        public bool LoadError { get; private set; }
        
        [NotMapped]
        public Exception LoadException { get; private set; }

        private VstPluginContext _pluginContext;

        [NotMapped]
        public VstPluginContext PluginContext
        {
            get { return _pluginContext; }
            set
            {
                _pluginContext = value;
                RaisePropertyChanged(nameof(IsLoaded));
            }
        }

        [NotMapped]
        public MemoryStream ChunkPresetMemoryStream { get; } = new MemoryStream();
        [NotMapped]
        public MemoryStream ChunkBankMemoryStream { get; } = new MemoryStream();

        
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

        public IVendorPresetParser PresetParser { get; set; }

        [NotMapped]
        public bool IsScanned { get; set; }

        public bool IsLoaded => PluginContext != null;


        [NotMapped]
        public bool IsSupported { get; set; }

        [NotMapped]
        public NativeInstrumentsResource NativeInstrumentsResource { get; set; } = new NativeInstrumentsResource();
        #endregion

        
    }
}