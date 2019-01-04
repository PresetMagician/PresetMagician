using System.Collections.Generic;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia
{
    public class Arturia_MiniFilter: Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1298744937 };

        public void ScanBanks()
        {
            List<string> instruments = new List<string>();
            instruments.Add("Mini-Filter");
            ScanPresets(instruments);
        }
    }
}