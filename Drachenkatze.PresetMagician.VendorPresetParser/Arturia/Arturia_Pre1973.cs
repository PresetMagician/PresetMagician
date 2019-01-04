using System.Collections.Generic;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia
{
    public class Arturia_Pre1973: Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1347565363 };

        public void ScanBanks()
        {
            List<string> instruments = new List<string>();
            instruments.Add("1973-Pre");
            ScanPresets(instruments);
        }
    }
}