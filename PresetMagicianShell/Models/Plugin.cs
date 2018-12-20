using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Catel.Data;
using Catel.IO;
using Catel.Runtime.Serialization;
using Drachenkatze.PresetMagician.VendorPresetParser;
using Drachenkatze.PresetMagician.VSTHost.VST;
using Jacobi.Vst.Core;
using Jacobi.Vst.Interop.Host;
using Newtonsoft.Json;

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
        public ObservableCollection<PluginInfoItem> PluginInfoItems { get; private set; } = new ObservableCollection<PluginInfoItem>();
      
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
            PluginInfoItems = new ObservableCollection<PluginInfoItem>();
            PluginName = PluginContext.PluginCommandStub.GetEffectName();
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
        }
        
        public VstPluginContext PluginContext { get; set; } = null;

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
            PluginInfoItems = new ObservableCollection<PluginInfoItem>();

            var pluginContext = PluginContext;

            if (pluginContext != null)
            {
                // plugin product
                PluginInfoItems.Add(new PluginInfoItem("Plugin Name " + pluginContext.PluginCommandStub.GetEffectName()));
                PluginInfoItems.Add(new PluginInfoItem("Product " + pluginContext.PluginCommandStub.GetProductString()));
                PluginInfoItems.Add(new PluginInfoItem("Vendor " + pluginContext.PluginCommandStub.GetVendorString()));
                PluginInfoItems.Add(new PluginInfoItem("Vendor Version " + pluginContext.PluginCommandStub.GetVendorVersion().ToString()));
                PluginInfoItems.Add(new PluginInfoItem("Vst Support " + pluginContext.PluginCommandStub.GetVstVersion().ToString()));
                PluginInfoItems.Add(new PluginInfoItem("Plugin Category " + pluginContext.PluginCommandStub.GetCategory().ToString()));

                // plugin info
                PluginInfoItems.Add(new PluginInfoItem("Flags " + pluginContext.PluginInfo.Flags.ToString()));
                PluginInfoItems.Add(new PluginInfoItem("Plugin ID " + pluginContext.PluginInfo.PluginID.ToString()));
                PluginInfoItems.Add(new PluginInfoItem("Plugin Version " + pluginContext.PluginInfo.PluginVersion.ToString()));
                PluginInfoItems.Add(new PluginInfoItem("Audio Input Count " + pluginContext.PluginInfo.AudioInputCount.ToString()));
                PluginInfoItems.Add(new PluginInfoItem("Audio Output Count " + pluginContext.PluginInfo.AudioOutputCount.ToString()));
                PluginInfoItems.Add(new PluginInfoItem("Initial Delay " + pluginContext.PluginInfo.InitialDelay.ToString()));
                PluginInfoItems.Add(new PluginInfoItem("Program Count " + pluginContext.PluginInfo.ProgramCount.ToString()));
                PluginInfoItems.Add(new PluginInfoItem("Parameter Count " + pluginContext.PluginInfo.ParameterCount.ToString()));
                PluginInfoItems.Add(new PluginInfoItem("Tail Size " + pluginContext.PluginCommandStub.GetTailSize().ToString()));

                // can do
                PluginInfoItems.Add(new PluginInfoItem("CanDo: " + VstPluginCanDo.Bypass + pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.Bypass)).ToString()));
                PluginInfoItems.Add(new PluginInfoItem("CanDo: " + VstPluginCanDo.MidiProgramNames + pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.MidiProgramNames)).ToString()));
                PluginInfoItems.Add(new PluginInfoItem("CanDo: " + VstPluginCanDo.Offline + pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.Offline)).ToString()));
                PluginInfoItems.Add(new PluginInfoItem("CanDo: " + VstPluginCanDo.ReceiveVstEvents + pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.ReceiveVstEvents)).ToString()));
                PluginInfoItems.Add(new PluginInfoItem("CanDo: " + VstPluginCanDo.ReceiveVstMidiEvent + pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.ReceiveVstMidiEvent)).ToString()));
                PluginInfoItems.Add(new PluginInfoItem("CanDo: " + VstPluginCanDo.ReceiveVstTimeInfo + pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.ReceiveVstTimeInfo)).ToString()));
                PluginInfoItems.Add(new PluginInfoItem("CanDo: " + VstPluginCanDo.SendVstEvents + pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.SendVstEvents)).ToString()));
                PluginInfoItems.Add(new PluginInfoItem("CanDo: " + VstPluginCanDo.SendVstMidiEvent + pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.SendVstMidiEvent)).ToString()));

                PluginInfoItems.Add(new PluginInfoItem("CanDo: " + VstPluginCanDo.ConformsToWindowRules + pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.ConformsToWindowRules)).ToString()));
                PluginInfoItems.Add(new PluginInfoItem("CanDo: " + VstPluginCanDo.Metapass + pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.Metapass)).ToString()));
                PluginInfoItems.Add(new PluginInfoItem("CanDo: " + VstPluginCanDo.MixDryWet + pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.MixDryWet)).ToString()));
                PluginInfoItems.Add(new PluginInfoItem("CanDo: " + VstPluginCanDo.Multipass + pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.Multipass)).ToString()));
                PluginInfoItems.Add(new PluginInfoItem("CanDo: " + VstPluginCanDo.NoRealTime + pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.NoRealTime)).ToString()));
                PluginInfoItems.Add(new PluginInfoItem("CanDo: " + VstPluginCanDo.PlugAsChannelInsert + pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.PlugAsChannelInsert)).ToString()));
                PluginInfoItems.Add(new PluginInfoItem("CanDo: " + VstPluginCanDo.PlugAsSend + pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.PlugAsSend)).ToString()));
                PluginInfoItems.Add(new PluginInfoItem("CanDo: " + VstPluginCanDo.SendVstTimeInfo + pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.SendVstTimeInfo)).ToString()));
                PluginInfoItems.Add(new PluginInfoItem("CanDo: " + VstPluginCanDo.x1in1out + pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x1in1out)).ToString()));
                PluginInfoItems.Add(new PluginInfoItem("CanDo: " + VstPluginCanDo.x1in2out + pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x1in2out)).ToString()));
                PluginInfoItems.Add(new PluginInfoItem("CanDo: " + VstPluginCanDo.x2in1out + pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x2in1out)).ToString()));
                PluginInfoItems.Add(new PluginInfoItem("CanDo: " + VstPluginCanDo.x2in2out + pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x2in2out)).ToString()));
                PluginInfoItems.Add(new PluginInfoItem("CanDo: " + VstPluginCanDo.x2in4out + pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x2in4out)).ToString()));
                PluginInfoItems.Add(new PluginInfoItem("CanDo: " + VstPluginCanDo.x4in2out + pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x4in2out)).ToString()));
                PluginInfoItems.Add(new PluginInfoItem("CanDo: " + VstPluginCanDo.x4in4out + pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x4in4out)).ToString()));
                PluginInfoItems.Add(new PluginInfoItem("CanDo: " + VstPluginCanDo.x4in8out + pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x4in8out)).ToString()));
                PluginInfoItems.Add(new PluginInfoItem("CanDo: " + VstPluginCanDo.x8in4out + pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x8in4out)).ToString()));
                PluginInfoItems.Add(new PluginInfoItem("CanDo: " + VstPluginCanDo.x8in8out + pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x8in8out)).ToString()));

                PluginInfoItems.Add(new PluginInfoItem("Program: " + pluginContext.PluginCommandStub.GetProgram()));
                PluginInfoItems.Add(new PluginInfoItem("Program Name: " + pluginContext.PluginCommandStub.GetProgramName()));

                for (int i = 0; i < pluginContext.PluginInfo.ParameterCount; i++)
                {
                    string name = pluginContext.PluginCommandStub.GetParameterName(i);
                    string label = pluginContext.PluginCommandStub.GetParameterLabel(i);
                    string display = pluginContext.PluginCommandStub.GetParameterDisplay(i);
                    bool canBeAutomated = pluginContext.PluginCommandStub.CanParameterBeAutomated(i);

                    PluginInfoItems.Add(new PluginInfoItem(
                        $"Parameter Index: {i} Parameter Name: {name} Display: {display} Label: {label} Can be automated: {canBeAutomated}"));
                }
            }
        }

        public bool IsSupported { get; set; }
    }
}