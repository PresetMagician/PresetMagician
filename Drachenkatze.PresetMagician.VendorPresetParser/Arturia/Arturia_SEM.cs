using System.Collections.Generic;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia
{
    public class Arturia_SEM: Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1329746738 };

        public void ScanBanks()
        {
            List<string> instruments = new List<string>();
            instruments.Add("SEM");
            ScanPresets(instruments);
        }
    }
}