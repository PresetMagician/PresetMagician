using Drachenkatze.PresetMagician.VSTHost.VST;
using System.Collections.Generic;

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
        }

        private PresetBank GetFactoryPresets()
        {
            PresetBank factoryBank = new PresetBank
            {
                BankName = BankNameFactory
            };

            for (int index = 0; index < VstPlugin.NumPresets; index++)
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
                vstPreset.PresetBank = factoryBank;

                vstPreset.SetPlugin(VstPlugin);

                Presets.Add(vstPreset);
            }

            return factoryBank;
        }
    }
}