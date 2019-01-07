using Anotar.Catel;
using Drachenkatze.PresetMagician.VSTHost.VST;

namespace Drachenkatze.PresetMagician.VendorPresetParser.StandardVST
{
    public class FullBankVstPresetParser : AbstractStandardVstPresetParser, IVendorPresetParser
    {
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
            RootBank.PresetBanks.Add(GetFactoryPresets());
            ParseAdditionalBanks();
        }

        private PresetBank GetFactoryPresets()
        {
            PresetBank factoryBank = new PresetBank
            {
                BankName = BankNameFactory
            };

            
            GetPresets(factoryBank, 0, VstPlugin.NumPresets);
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
                LogTo.Error(
                    $"GetPresets between {start} and {endIndex} would exceed maximum program count of {VstPlugin.PluginContext.PluginInfo.ProgramCount}, ignoring.");
                return;
            }
            
            for (int index = start; index < endIndex; index++)
            {
                var vstPreset = new Preset();

                VstPlugin.PluginContext.PluginCommandStub.SetProgram(index);
                vstPreset.PresetName = VstPlugin.PluginContext.PluginCommandStub.GetProgramName();
                vstPreset.PresetData = VstPlugin.PluginContext.PluginCommandStub.GetChunk(false);
                vstPreset.PresetBank = bank;
                vstPreset.SetPlugin(VstPlugin);
                Presets.Add(vstPreset);
            }
        }
    }
}