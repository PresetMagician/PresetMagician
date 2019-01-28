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
            if (DeterminateVstPresetSaveMode() == PresetSaveModes.BankTrickery)
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

            for (int index = start; index < endIndex; index++)
            {
                PluginInstance.SetProgram(0);
                var programBackup = PluginInstance.GetChunk(true);
                PluginInstance.SetProgram(index);

                var vstPreset = new Preset
                {
                    SourceFile = sourceFile + ":" + index,
                    PresetBank = bank,
                    PresetName = PluginInstance.GetCurrentProgramName(),
                    Plugin = PluginInstance.Plugin
                };


                byte[] realProgram = PluginInstance.GetChunk(true);
                PluginInstance.SetProgram(0);

                PluginInstance.SetChunk(realProgram, true);
                var presetData = PluginInstance.GetChunk(false);
                PluginInstance.SetChunk(programBackup, true);

                var hash = HashUtils.getIxxHash(realProgram);

                if (PresetHashes.Contains(hash))
                {
                    PluginInstance.Plugin.Logger.Debug(
                        $"Skipping program {index} because the preset already seem to exist");
                }
                else
                {
                    PresetHashes.Add(hash);
                    await DataPersistence.PersistPreset(vstPreset, presetData);
                }
            }
        }
    }
}