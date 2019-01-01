using System.Collections.Generic;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia
{
    public class Arturia_Piano: Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1349083502 };

        public void ScanBanks()
        {
            List<string> instruments = new List<string>();
            instruments.Add("Piano");
            ScanPresets(instruments);
        }
    }
}