using System.Collections.Generic;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia
{
    public class Arturia_AnalogLab3: Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1097621810 };

        public override string Remarks { get; set; } =
            "Only AnalogLab3 multi presets are currently supported, as the format has not been fully reverse engineered.";
        
        public void ScanBanks()
        {
            List<string> instruments = new List<string>();
            instruments.Add("Analog Lab");
            ScanPresets(instruments);
        }
    }
}