using System.Collections.Generic;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia
{
    public class Arturia_CMI: Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1129138550 };

        public override string Remarks { get; set; } =
            "Some audio previews are exported empty; however, that should be less than 1% of all presets.";
        
        public override int AudioPreviewPreDelay { get; set; } = 4096;
        
        public void ScanBanks()
        {
            List<string> instruments = new List<string>();
            instruments.Add("CMI");
            ScanPresets(instruments);
        }
    }
}