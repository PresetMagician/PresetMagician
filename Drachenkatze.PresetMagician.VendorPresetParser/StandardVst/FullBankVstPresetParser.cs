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
            return DeterminateVstPresetSaveMode() == PresetSaveModes.FullBank;
        }


        protected override async Task GetFactoryPresets()
        {
            var factoryBank = FindOrCreateBank(BankNameFactory);

            await GetPresets(factoryBank, 0, PluginInstance.Plugin.PluginInfo.ProgramCount, "Builtin");
        }

        [Time]
        protected override async Task GetPresets(PresetBank bank, int start, int numPresets, string sourceFile)
        {
            if (start < 0)
            {
                PluginInstance.Plugin.Logger.Error("GetPresets start index is less than 0, ignoring.");
                return;
            }

            var endIndex = start + numPresets;

            if (endIndex > PluginInstance.Plugin.PluginInfo.ProgramCount)
            {
                PluginInstance.Plugin.Logger.Error(
                    $"GetPresets between {start} and {endIndex} would exceed maximum program count of {PluginInstance.Plugin.PluginInfo.ProgramCount}, ignoring.");
                return;
            }

            for (var index = start; index < endIndex; index++)
            {
                PluginInstance.SetProgram(index);

                var preset = new Preset
                {
                    PresetName = PluginInstance.GetCurrentProgramName(),
                    PresetBank = bank,
                    SourceFile = sourceFile + ":" + index,
                    Plugin = PluginInstance.Plugin
                };

                await DataPersistence.PersistPreset(preset, PluginInstance.GetChunk(false));
            }
        }
    }
}