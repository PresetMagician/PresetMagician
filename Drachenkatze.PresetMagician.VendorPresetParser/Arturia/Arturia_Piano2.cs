using System.Collections.Generic;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia
{
    public class Arturia_Piano2: Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1349083442 };

        public void ScanBanks()
        {
            List<string> instruments = new List<string>();
            instruments.Add("Piano V2");
            ScanPresets(instruments);
        }
    }
}