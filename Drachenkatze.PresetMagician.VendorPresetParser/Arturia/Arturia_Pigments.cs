using System.Collections.Generic;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia
{
    public class Arturia_Pigments: Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1264677937 };

        public override string Remarks { get; set; } =
            "Exported audio previews are randomly empty; this could be a pigments bug.";
        
        public void ScanBanks()
        {
            List<string> instruments = new List<string>();
            instruments.Add("Pigments");
            ScanPresets(instruments);
        }
    }
}