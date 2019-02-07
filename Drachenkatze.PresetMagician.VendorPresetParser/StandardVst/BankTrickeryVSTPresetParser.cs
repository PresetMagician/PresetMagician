using System.Threading.Tasks;
using Catel.Logging;
using Drachenkatze.PresetMagician.Utils;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.StandardVST
{
    public class BankTrickeryVstPresetParser : AbstractStandardVstPresetParser, IVendorPresetParser
    {
        public override bool CanHandle()
        {
            DeterminateVstPresetSaveMode();
            if (PresetSaveMode == PresetSaveModes.BankTrickery)
            {
                return true;
            }

            return false;
        }

        protected override async Task GetFactoryPresets()
        {
            var factoryBank = RootBank.CreateRecursive(BankNameFactory);

            await GetPresets(factoryBank, 0, PluginInstance.Plugin.PluginInfo.ProgramCount, "Builtin");
        }

        
    }
}