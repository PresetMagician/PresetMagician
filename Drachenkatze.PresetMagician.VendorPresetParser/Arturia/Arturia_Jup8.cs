using System.Collections.Generic;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia
{
    public class Arturia_Jup8: Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1247105075 };

        public void ScanBanks()
        {
            List<string> instruments = new List<string>();
            instruments.Add("Jup-8");
            ScanPresets(instruments);
        }
    }
}