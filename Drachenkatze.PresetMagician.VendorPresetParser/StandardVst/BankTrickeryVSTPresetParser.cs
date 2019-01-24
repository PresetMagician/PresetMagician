using System.Threading.Tasks;
using Anotar.Catel;
using Drachenkatze.PresetMagician.Utils;
using Drachenkatze.PresetMagician.VSTHost.VST;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.StandardVST
{
    public class BankTrickeryVstPresetParser : AbstractStandardVstPresetParser, IVendorPresetParser
    {
        public override bool CanHandle()
        {
            if (DeterminateVSTPresetSaveMode() == PresetSaveModes.BankTrickery)
            {
                return true;
            }

            return false;
        }

        protected override async Task GetFactoryPresets()
        {
            var factoryBank = FindOrCreateBank(BankNameFactory);

            await GetPresets(factoryBank, 0, Plugin.PluginInfo.ProgramCount, "Builtin");
        }

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

            for (int index = start; index < endIndex; index++)
            {
                RemoteVstService.SetProgram(Plugin.Guid, 0);
                var programBackup = RemoteVstService.GetChunk(Plugin.Guid, true);
                RemoteVstService.SetProgram(Plugin.Guid, index);

                var vstPreset = new Preset
                {
                    SourceFile = sourceFile + ":" + index,
                    PresetBank = bank,
                    PresetName = RemoteVstService.GetCurrentProgramName(Plugin.Guid),
                    Plugin = Plugin
                };


                byte[] realProgram = RemoteVstService.GetChunk(Plugin.Guid, true);
                RemoteVstService.SetProgram(Plugin.Guid, 0);

                RemoteVstService.SetChunk(Plugin.Guid, realProgram, true);
                var presetData = RemoteVstService.GetChunk(Plugin.Guid, false);
                RemoteVstService.SetChunk(Plugin.Guid, programBackup, true);

                var hash = HashUtils.getIxxHash(realProgram);

                if (PresetHashes.Contains(hash))
                {
                    Plugin.Debug($"Skipping program {index} because the preset already seem to exist");
                }
                else
                {
                    PresetHashes.Add(hash);
                    await PresetDataStorer.PersistPreset(vstPreset, presetData);
                }
            }
        }
    }
}