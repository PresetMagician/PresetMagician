using System.Collections.Generic;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia
{
    public class Arturia_Solina: Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1399811122 };

        public void ScanBanks()
        {
            List<string> instruments = new List<string>();
            instruments.Add("Solina");
            ScanPresets(instruments);
        }
    }
}