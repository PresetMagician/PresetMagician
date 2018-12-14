using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Drachenkatze.PresetMagician.VSTHost.VST;

namespace Drachenkatze.PresetMagician.VendorPresetParser.StandardVST
{
    public class FullBankVstPresetParser : AbstractStandardVstPresetParser, IVendorPresetParser
    {
        private const string BankNameFactory = "Factory";

        public override bool CanHandle()
        {
            if (DeterminateVSTPresetSaveMode() == PresetSaveModes.FullBank)
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

                VstPlugin.PluginContext.PluginCommandStub.SetProgram(index);
                vstPreset.PresetName = VstPlugin.PluginContext.PluginCommandStub.GetProgramName();
                vstPreset.PresetData = VstPlugin.PluginContext.PluginCommandStub.GetChunk(false);
                vstPreset.SetPlugin(VstPlugin);
                factoryBank.Presets.Add(vstPreset);
            }

            return factoryBank;
        }
    }
}