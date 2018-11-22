using Drachenkatze.PresetMagician.VendorPresetParser.StandardVST;

namespace Drachenkatze.PresetMagician.VendorPresetParser.StandardVst
{
    public class FallbackVstPresetParser : BankTrickeryVstPresetParser
    {
        private const string RemarkText = "Due to the way NI saves preset data in the NKSF files, this plugin might not be compatible with that mechanism. " +
                                         "We'll try anyways; double check by loading different generated NKSF presets (don't rely on the audio previews!)";

        public override string Remarks { get; set; } =
            RemarkText;

        public override bool CanHandle()
        {
            if (DeterminateVSTPresetSaveMode() == PresetSaveModes.Fallback)
            {
                return true;
            }

            return false;
        }
    }
}