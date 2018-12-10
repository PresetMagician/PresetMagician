using System;
using Catel.Data;
using Catel.IO;
using Catel.Runtime.Serialization;
using Drachenkatze.PresetMagician.VendorPresetParser;
using Drachenkatze.PresetMagician.VSTHost.VST;
using Jacobi.Vst.Core;
using Jacobi.Vst.Interop.Host;

namespace PresetMagicianShell.Models
{
    public class Plugin : ModelBase, IVstPlugin, IDisposable
    {
        public Plugin()
        {
            PresetParser = new NullPresetParser();
        }
        public void OnLoadError(string errorMessage)
        {
            LoadError = true;
            LoadErrorMessage = errorMessage;
        }

        [ExcludeFromSerialization]
        public string LoadErrorMessage { get; private set; }

        [ExcludeFromSerialization]
        public bool LoadError { get; private set; }
        public void OnLoaded()
        {
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

        }

        public void DeterminatePresetParser()
        {
            PresetParser = VendorPresetParser.GetPresetHandler(this);
        }

        public void Dispose()
        {
            if (PluginContext != null)
            {
                PluginContext.Dispose();
            }
        }
        
        public VstPluginContext PluginContext { get; set; } = null;

        [IncludeInSerialization]
        public bool Enabled { get; set; } = true;

        [IncludeInSerialization]
        public string DllPath { get; set; }

        public string DllDirectory => Path.GetDirectoryName(DllPath);

        public string DllFilename => Path.GetFileName(DllPath);

        
        public VstHost.PluginTypes PluginType { get; set; } = VstHost.PluginTypes.Unknown;

        [IncludeInSerialization]
        public String PluginTypeDescription => PluginType.ToString();

        [IncludeInSerialization]
        public int PluginId { get; set; }

        public int NumPresets
        {
            get; set;
        }

        [IncludeInSerialization]
        public string PluginName
        {
            get; set;
        }

        [IncludeInSerialization]
        public string PluginVendor
        {
            get; set;
        }


        [ExcludeFromSerialization]
        public IVendorPresetParser PresetParser { get; private set; }

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

       public bool IsSupported
        {
            get
            {
                if (PresetParser == null)
                {
                    return false;

                }
                return !PresetParser.IsNullParser;
            }
        }
    }
}