using System.Threading.Tasks;
using PresetMagician.Core.Interfaces;

namespace PresetMagician.VendorPresetParser.StandardVST
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