using System.Collections.Generic;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia
{
    public class Arturia_CMI: Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1129138550 };

        public override string Remarks { get; set; } =
            "Audio previews are not working yet";
        
        public void ScanBanks()
        {
            List<string> instruments = new List<string>();
            instruments.Add("CMI");
            ScanPresets(instruments);
        }
    }
}