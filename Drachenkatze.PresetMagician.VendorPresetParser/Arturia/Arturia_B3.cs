using System.Collections.Generic;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia
{
    public class Arturia_B3: Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1416588887 };

        public void ScanBanks()
        {
            List<string> instruments = new List<string>();
            instruments.Add("B-3");
            ScanPresets(instruments);
        }
    }
}