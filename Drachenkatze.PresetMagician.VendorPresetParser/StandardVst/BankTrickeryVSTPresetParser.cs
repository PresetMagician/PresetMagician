using Anotar.Catel;
using Drachenkatze.PresetMagician.Utils;
using Drachenkatze.PresetMagician.VSTHost.VST;

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

        public void ScanBanks()
        {
            RootBank.PresetBanks.Add(GetFactoryPresets());
            ParseAdditionalBanks();
        }

        private PresetBank GetFactoryPresets()
        {
            PresetBank factoryBank = new PresetBank
            {
                BankName = BankNameFactory
            };

            GetPresets(factoryBank, 0, VstPlugin.PluginContext.PluginInfo.ProgramCount);
            
            return factoryBank;
        }

        protected override void GetPresets(IPresetBank bank, int start, int numPresets)
        {
            if (start < 0)
            {
                LogTo.Error("GetPresets start index is less than 0, ignoring.");
                return;
            }

            var endIndex = start + numPresets;
            
            if (endIndex > VstPlugin.PluginContext.PluginInfo.ProgramCount)
            {
                LogTo.Error($"GetPresets between {start} and {endIndex} would exceed maximum program count of {VstPlugin.PluginContext.PluginInfo.ProgramCount}, ignoring.");
                return;
            }
            
            for (int index = start; index < endIndex; index++)
            {
                var vstPreset = new Preset();

                VstPlugin.PluginContext.PluginCommandStub.SetProgram(0);
                byte[] programBackup = VstPlugin.PluginContext.PluginCommandStub.GetChunk(true);

                VstPlugin.PluginContext.PluginCommandStub.SetProgram(index);
                vstPreset.PresetName = VstPlugin.PluginContext.PluginCommandStub.GetProgramName();
                byte[] realProgram = VstPlugin.PluginContext.PluginCommandStub.GetChunk(true);
                VstPlugin.PluginContext.PluginCommandStub.SetProgram(0);
                VstPlugin.PluginContext.PluginCommandStub.SetChunk(realProgram, true);
                vstPreset.PresetData = VstPlugin.PluginContext.PluginCommandStub.GetChunk(false);
                VstPlugin.PluginContext.PluginCommandStub.SetChunk(programBackup, true);

                vstPreset.ProgramNumber = VstPlugin.PluginContext.PluginCommandStub.GetProgram();
                vstPreset.PresetBank = bank;

                var hash = HashUtils.getFormattedSHA256Hash(realProgram);

                if (PresetHashes.Contains(hash))
                {
                    LogTo.Debug($"Skipping program {index} because the preset already seem to exist");
                }
                else
                {
                    vstPreset.SetPlugin(VstPlugin);

                    Presets.Add(vstPreset);
                }
            }

        }
    }
}