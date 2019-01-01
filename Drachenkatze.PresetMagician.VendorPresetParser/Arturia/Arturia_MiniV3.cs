using System.Collections.Generic;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia
{
    public class Arturia_MiniV3: Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1296649779 };

        public void ScanBanks()
        {
            List<string> instruments = new List<string>();
            instruments.Add("Mini");
            ScanPresets(instruments);
        }
    }
}