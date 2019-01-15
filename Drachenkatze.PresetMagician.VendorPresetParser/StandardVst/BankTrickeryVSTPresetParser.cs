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

            await GetPresets(factoryBank, 0, Plugin.PluginContext.PluginInfo.ProgramCount, "Builtin");
            
        }

        protected override async Task GetPresets(PresetBank bank, int start, int numPresets, string sourceFile)
        {
            if (start < 0)
            {
                LogTo.Error("GetPresets start index is less than 0, ignoring.");
                return;
            }

            var endIndex = start + numPresets;
            
            if (endIndex > Plugin.PluginContext.PluginInfo.ProgramCount)
            {
                LogTo.Error($"GetPresets between {start} and {endIndex} would exceed maximum program count of {Plugin.PluginContext.PluginInfo.ProgramCount}, ignoring.");
                return;
            }
            
            for (int index = start; index < endIndex; index++)
            {
                Plugin.PluginContext.PluginCommandStub.SetProgram(0);
                var programBackup = Plugin.PluginContext.PluginCommandStub.GetChunk(true);
                Plugin.PluginContext.PluginCommandStub.SetProgram(index);

                var vstPreset = new Preset
                {
                    SourceFile = sourceFile + ":" + index,
                    PresetBank = bank,
                    PresetName = Plugin.PluginContext.PluginCommandStub.GetProgramName(),
                    Plugin = Plugin
                };


                byte[] realProgram = Plugin.PluginContext.PluginCommandStub.GetChunk(true);
                Plugin.PluginContext.PluginCommandStub.SetProgram(0);
                Plugin.PluginContext.PluginCommandStub.SetChunk(realProgram, true);
                var presetData = Plugin.PluginContext.PluginCommandStub.GetChunk(false);
                Plugin.PluginContext.PluginCommandStub.SetChunk(programBackup, true);

                var hash = HashUtils.getIxxHash(realProgram);

                if (PresetHashes.Contains(hash))
                {
                    LogTo.Debug($"Skipping program {index} because the preset already seem to exist");
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