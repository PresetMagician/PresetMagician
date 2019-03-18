using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Catel.Logging;
using Drachenkatze.PresetMagician.Utils;
using Jacobi.Vst.Core;
using PresetMagician.Core.Interfaces;

namespace Drachenkatze.PresetMagician.VendorPresetParser.StandardVST
{
    public abstract class AbstractStandardVstPresetParser : AbstractVendorPresetParser
    {
        public override bool RequiresRescanWithEachRelease { get; } = true;
        
        public override int GetNumPresets()
        {
            PluginInstance.LoadPlugin().Wait();
            return base.GetNumPresets() + PluginInstance.Plugin.PluginInfo.ProgramCount;
        }

        public override async Task DoScan()
        {
            await PluginInstance.LoadPlugin();
            DeterminateVstPresetSaveMode();
            await GetFactoryPresets();
            await base.DoScan();
        }

        protected abstract Task GetFactoryPresets();
    
    }
}