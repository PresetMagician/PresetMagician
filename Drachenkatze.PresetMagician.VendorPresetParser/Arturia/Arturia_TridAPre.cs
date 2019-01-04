using System.Collections.Generic;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia
{
    public class Arturia_TridAPre: Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1414676818 };

        public void ScanBanks()
        {
            List<string> instruments = new List<string>();
            instruments.Add("TridA-Pre");
            ScanPresets(instruments);
        }
    }
}