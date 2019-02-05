using System.Threading.Tasks;
using Catel.Logging;
using JetBrains.Annotations;
using MethodTimer;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.StandardVST
{
    [UsedImplicitly]
    public class FullBankVstPresetParser : AbstractStandardVstPresetParser, IVendorPresetParser
    {
        public override bool CanHandle()
        {
            return PresetSaveMode == PresetSaveModes.FullBank;
        }


        protected override async Task GetFactoryPresets()
        {
            var factoryBank = RootBank.CreateRecursive(BankNameFactory);

            await GetPresets(factoryBank, 0, PluginInstance.Plugin.PluginInfo.ProgramCount, "Builtin");
        }

        
    }
}