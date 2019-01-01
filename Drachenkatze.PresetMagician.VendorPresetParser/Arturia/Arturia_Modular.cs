using System.Collections.Generic;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia
{
    public class Arturia_Modular: Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1297040435 };

        public void ScanBanks()
        {
            List<string> instruments = new List<string>();
            instruments.Add("Modular");
            ScanPresets(instruments);
        }
    }
}