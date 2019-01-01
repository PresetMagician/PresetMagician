using System.Collections.Generic;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia
{
    public class Arturia_DX7: Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1148729137 };

        public void ScanBanks()
        {
            List<string> instruments = new List<string>();
            instruments.Add("DX7");
            ScanPresets(instruments);
        }
    }
}