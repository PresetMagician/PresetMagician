using System.Threading.Tasks;
using PresetMagician.Core.Enums;

namespace PresetMagician.VendorPresetParser.StandardVST
{
    public abstract class AbstractStandardVstPresetParser : AbstractVendorPresetParser
    {
        public override PresetParserPriorityEnum Priority { get; } = PresetParserPriorityEnum.GENERIC_VST_PRIORITY;
        
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