using System.Linq;
using System.Threading.Tasks;
using Anotar.Catel;
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
            return DeterminateVSTPresetSaveMode() == PresetSaveModes.FullBank;
        }


        protected override async Task GetFactoryPresets()
        {
            var factoryBank = FindOrCreateBank(BankNameFactory);

            await GetPresets(factoryBank, 0, Plugin.PluginInfo.ProgramCount, "Builtin");
        }

        [Time]
        protected override async Task GetPresets(PresetBank bank, int start, int numPresets, string sourceFile)
        {
            if (start < 0)
            {
                Plugin.Error("GetPresets start index is less than 0, ignoring.");
                return;
            }

            var endIndex = start + numPresets;

            if (endIndex > Plugin.PluginInfo.ProgramCount)
            {
                Plugin.Error(
                    $"GetPresets between {start} and {endIndex} would exceed maximum program count of {Plugin.PluginInfo.ProgramCount}, ignoring.");
                return;
            }

            for (var index = start; index < endIndex; index++)
            {
                RemoteVstService.SetProgram(Plugin.Guid, index);

                var preset = new Preset
                {
                    PresetName = RemoteVstService.GetCurrentProgramName(Plugin.Guid),
                    PresetBank = bank,
                    SourceFile = sourceFile + ":" + index,
                    Plugin = Plugin
                };

                await PresetDataStorer.PersistPreset(preset, RemoteVstService.GetChunk(Plugin.Guid, false));
            }
        }
    }
}