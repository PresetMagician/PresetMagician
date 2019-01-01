using System.Collections.Generic;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia
{
    public class Arturia_Synclavier: Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1400464236 };

        public void ScanBanks()
        {
            List<string> instruments = new List<string>();
            instruments.Add("Synclavier");
            ScanPresets(instruments);
        }
    }
}