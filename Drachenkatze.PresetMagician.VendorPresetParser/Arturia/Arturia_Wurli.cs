using System.Collections.Generic;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia
{
    public class Arturia_Wurli: Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1465209394 };

        public void ScanBanks()
        {
            List<string> instruments = new List<string>();
            instruments.Add("Wurli");
            ScanPresets(instruments);
        }
    }
}