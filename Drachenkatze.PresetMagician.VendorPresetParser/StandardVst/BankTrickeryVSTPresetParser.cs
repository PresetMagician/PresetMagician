using Drachenkatze.PresetMagician.VSTHost.VST;
using System.Collections.Generic;

namespace Drachenkatze.PresetMagician.VendorPresetParser.StandardVST
{
    public class BankTrickeryVstPresetParser : AbstractStandardVstPresetParser, IVendorPresetParser
    {
        private const string BankNameFactory = "Factory";

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
            Banks = new List<PresetBank> { GetFactoryPresets() };
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
                vstPreset.BankName = VstPlugin.PluginName + " Factory";

                vstPreset.SetPlugin(VstPlugin);

                factoryBank.Presets.Add(vstPreset);
            }

            return factoryBank;
        }
    }
}