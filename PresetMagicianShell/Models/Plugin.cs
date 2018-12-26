using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Catel.Data;
using Drachenkatze.PresetMagician.VendorPresetParser;
using Drachenkatze.PresetMagician.VSTHost.VST;
using Jacobi.Vst.Core;
using Jacobi.Vst.Interop.Host;
using Newtonsoft.Json;
using Path = Catel.IO.Path;

namespace PresetMagicianShell.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Plugin : ModelBase, IVstPlugin, IDisposable
    {
        public Plugin()
        {
            PresetParser = new NullPresetParser();
        }

        public override string ToString()
        {
            
            if (IsLoaded)
            {
                return $"{PluginVendor} {PluginName} ({PluginId})";
            }
            else
            {
                return $"{DllPath}";
            }
        }

        public void OnLoadError(Exception e)
        {
            LoadError = true;
            LoadErrorMessage = e.ToString();
            LoadException = e;
        }

        public string LoadErrorMessage { get; private set; }

        /// <summary>
        /// Gets or sets the table collection.
        /// </summary>
        public List<PluginInfoItem> PluginInfoItems
        {
            get { return _pluginInfoItems.ToList(); }

        }

        private ObservableCollection<PluginInfoItem> _pluginInfoItems = new ObservableCollection<PluginInfoItem>();
      
        #region PresetBanks property

        /// <summary>
        /// Gets or sets the PresetBanks value.
        /// </summary>
        public PresetBank RootBank { get; } = new PresetBank();

        public ObservableCollection<Preset> Presets { get; set; } = new ObservableCollection<Preset>();

        #endregion


        public bool LoadError { get; private set; }
        public Exception LoadException { get; private set; }

        public void OnLoaded()
        {
            PluginName = PluginContext.PluginCommandStub.GetEffectName();

            if (PluginName.Length == 0)
            {
                // Fallback to product name
                PluginName = PluginContext.PluginCommandStub.GetProductString();
            }
            PluginVendor = PluginContext.PluginCommandStub.GetVendorString();
            PluginId = PluginContext.PluginInfo.PluginID;
            NumPresets = PluginContext.PluginInfo.ProgramCount;

            if (PluginContext.PluginInfo.Flags.HasFlag(VstPluginFlags.IsSynth))
            {
                PluginType = VstHost.PluginTypes.Instrument;
            }
            else
            {
                PluginType = VstHost.PluginTypes.Effect;
            }

            PopulatePluginInfoItems();
        }

        public void DeterminatePresetParser()
        {
            PresetParser = VendorPresetParser.GetPresetHandler(this);

                    if (PresetParser == null)
                    {
                        IsSupported = false;

                    }
                    IsSupported = !PresetParser.IsNullParser;
                
        }

        public void Dispose()
        {
            PluginContext?.Dispose();
            PluginContext = null;
        }

        private VstPluginContext _pluginContext = null;

        public VstPluginContext PluginContext
        {
            get { return _pluginContext; }
            set
            {
                _pluginContext = value;
                RaisePropertyChanged(nameof(IsLoaded));
            }
        }

        public MemoryStream ChunkPresetMemoryStream { get; } = new MemoryStream();
        public MemoryStream ChunkBankMemoryStream { get; } = new MemoryStream();

        public static string PluginIdNumberToIdString(int pluginUniqueId)
        {
            byte[] fxIdArray = BitConverter.GetBytes(pluginUniqueId);
            Array.Reverse(fxIdArray);
            string fxIdString = Encoding.Default.GetString(fxIdArray);
            return fxIdString;
        }

        public void GetPresetChunk ()
        {
            var data = PluginContext.PluginCommandStub.GetChunk(true);
            
            if (!(data is null)) {
            ChunkPresetMemoryStream.SetLength(0);
                ChunkPresetMemoryStream.Write(data, 0, data.Length);

            Debug.WriteLine($"Copied {data.Length} bytes to stream");
            
            RaisePropertyChanged(nameof(ChunkPresetMemoryStream));
            }

            data = PluginContext.PluginCommandStub.GetChunk(false);
            
            if (!(data is null)) {
                ChunkBankMemoryStream.SetLength(0);
                ChunkBankMemoryStream.Write(data, 0, data.Length);

                Debug.WriteLine($"Copied {data.Length} bytes to stream");
            
                RaisePropertyChanged(nameof(ChunkBankMemoryStream));
            }
            
        }

        [JsonProperty]
        public bool Enabled { get; set; } = true;

        [JsonProperty]
        public string DllPath { get; set; }

        public string DllDirectory => Path.GetDirectoryName(DllPath);

        public string DllFilename => Path.GetFileName(DllPath);

        [JsonProperty]
        public VstHost.PluginTypes PluginType { get; set; } = VstHost.PluginTypes.Unknown;

        public String PluginTypeDescription => PluginType.ToString();

        [JsonProperty]
        public int PluginId { get; set; }

        public int NumPresets
        {
            get; set;
        }

        [JsonProperty]
        public string PluginName
        {
            get; set;
        }

        [JsonProperty]
        public string PluginVendor
        {
            get; set;
        }


        public IVendorPresetParser PresetParser { get; private set; }

        public bool IsScanned { get; set; } = false;

        public bool IsLoaded
        {
            get
            {
                if (PluginContext != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public void PopulatePluginInfoItems()
        {
            _pluginInfoItems = new ObservableCollection<PluginInfoItem>();

            var pluginContext = PluginContext;

            if (pluginContext != null)
            {
                // plugin product
                _pluginInfoItems.Add(new PluginInfoItem("Base", "Plugin Name", pluginContext.PluginCommandStub.GetEffectName()));
                _pluginInfoItems.Add(new PluginInfoItem("Base", "Product " , pluginContext.PluginCommandStub.GetProductString()));
                _pluginInfoItems.Add(new PluginInfoItem("Base", "Vendor " , pluginContext.PluginCommandStub.GetVendorString()));
                _pluginInfoItems.Add(new PluginInfoItem("Base", "Vendor Version " , pluginContext.PluginCommandStub.GetVendorVersion().ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("Base", "Vst Support " , pluginContext.PluginCommandStub.GetVstVersion().ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("Base", "Plugin Category " , pluginContext.PluginCommandStub.GetCategory().ToString()));

                // plugin info
                _pluginInfoItems.Add(new PluginInfoItem("Base", "Flags " , pluginContext.PluginInfo.Flags.ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("Base", "Plugin ID " , pluginContext.PluginInfo.PluginID.ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("Base", "Plugin ID String" , PluginIdNumberToIdString(pluginContext.PluginInfo.PluginID)));
                
                _pluginInfoItems.Add(new PluginInfoItem("Base", "Plugin Version " , pluginContext.PluginInfo.PluginVersion.ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("Base", "Audio Input Count " , pluginContext.PluginInfo.AudioInputCount.ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("Base", "Audio Output Count " , pluginContext.PluginInfo.AudioOutputCount.ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("Base", "Initial Delay " , pluginContext.PluginInfo.InitialDelay.ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("Base", "Program Count " , pluginContext.PluginInfo.ProgramCount.ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("Base", "Parameter Count " , pluginContext.PluginInfo.ParameterCount.ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("Base", "Tail Size " , pluginContext.PluginCommandStub.GetTailSize().ToString()));

                // can do
                _pluginInfoItems.Add(new PluginInfoItem("CanDo" , nameof(VstPluginCanDo.Bypass) , pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.Bypass)).ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("CanDo" , nameof(VstPluginCanDo.MidiProgramNames) , pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.MidiProgramNames)).ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("CanDo" , nameof(VstPluginCanDo.Offline) , pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.Offline)).ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("CanDo" , nameof(VstPluginCanDo.ReceiveVstEvents) , pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.ReceiveVstEvents)).ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("CanDo" , nameof(VstPluginCanDo.ReceiveVstMidiEvent) , pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.ReceiveVstMidiEvent)).ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("CanDo" , nameof(VstPluginCanDo.ReceiveVstTimeInfo) , pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.ReceiveVstTimeInfo)).ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("CanDo" , nameof(VstPluginCanDo.SendVstEvents) , pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.SendVstEvents)).ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("CanDo" , nameof(VstPluginCanDo.SendVstMidiEvent) , pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.SendVstMidiEvent)).ToString()));

                _pluginInfoItems.Add(new PluginInfoItem("CanDo" , nameof(VstPluginCanDo.ConformsToWindowRules) , pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.ConformsToWindowRules)).ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("CanDo" , nameof(VstPluginCanDo.Metapass) , pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.Metapass)).ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("CanDo" , nameof(VstPluginCanDo.MixDryWet) , pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.MixDryWet)).ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("CanDo" , nameof(VstPluginCanDo.Multipass) , pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.Multipass)).ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("CanDo" , nameof(VstPluginCanDo.NoRealTime) , pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.NoRealTime)).ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("CanDo" , nameof(VstPluginCanDo.PlugAsChannelInsert) , pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.PlugAsChannelInsert)).ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("CanDo" , nameof(VstPluginCanDo.PlugAsSend) , pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.PlugAsSend)).ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("CanDo" , nameof(VstPluginCanDo.SendVstTimeInfo) , pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.SendVstTimeInfo)).ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("CanDo" , nameof(VstPluginCanDo.x1in1out) , pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x1in1out)).ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("CanDo" , nameof(VstPluginCanDo.x1in2out) , pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x1in2out)).ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("CanDo" , nameof(VstPluginCanDo.x2in1out) , pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x2in1out)).ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("CanDo" , nameof(VstPluginCanDo.x2in2out) , pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x2in2out)).ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("CanDo" , nameof(VstPluginCanDo.x2in4out) , pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x2in4out)).ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("CanDo" , nameof(VstPluginCanDo.x4in2out) , pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x4in2out)).ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("CanDo" , nameof(VstPluginCanDo.x4in4out) , pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x4in4out)).ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("CanDo" , nameof(VstPluginCanDo.x4in8out) , pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x4in8out)).ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("CanDo" , nameof(VstPluginCanDo.x8in4out) , pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x8in4out)).ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("CanDo" , nameof(VstPluginCanDo.x8in8out) , pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x8in8out)).ToString()));

                _pluginInfoItems.Add(new PluginInfoItem("Program" , "Current Program Index",pluginContext.PluginCommandStub.GetProgram().ToString()));
                _pluginInfoItems.Add(new PluginInfoItem("Program", "Current Program Name", pluginContext.PluginCommandStub.GetProgramName()));

                for (int i = 0; i < pluginContext.PluginInfo.ParameterCount; i++)
                {
                    string name = pluginContext.PluginCommandStub.GetParameterName(i);
                    string label = pluginContext.PluginCommandStub.GetParameterLabel(i);
                    string display = pluginContext.PluginCommandStub.GetParameterDisplay(i);
                    bool canBeAutomated = pluginContext.PluginCommandStub.CanParameterBeAutomated(i);

                    _pluginInfoItems.Add(new PluginInfoItem("Parameters", i.ToString(),
                        $"Parameter Index: {i} Parameter Name: {name} Display: {display} Label: {label} Can be automated: {canBeAutomated}"));
                }
            }
        }

        public bool IsSupported { get; set; }
    }
}