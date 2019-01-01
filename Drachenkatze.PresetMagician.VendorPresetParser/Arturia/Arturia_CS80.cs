using System.Collections.Generic;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia
{
    public class Arturia_CS80: Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1129535027 };

        public void ScanBanks()
        {
            List<string> instruments = new List<string>();
            instruments.Add("CS-80");
            ScanPresets(instruments);
        }
    }
}