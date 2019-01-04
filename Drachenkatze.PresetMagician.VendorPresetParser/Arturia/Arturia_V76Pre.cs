using System.Collections.Generic;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia
{
    public class Arturia_V76Pre: Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1413827653 };

        public void ScanBanks()
        {
            List<string> instruments = new List<string>();
            instruments.Add("V76-Pre");
            ScanPresets(instruments);
        }
    }
}