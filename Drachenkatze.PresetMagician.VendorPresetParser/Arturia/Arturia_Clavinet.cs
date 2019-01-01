using System.Collections.Generic;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia
{
    public class Arturia_Clavinet: Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1131176310 };

        public void ScanBanks()
        {
            List<string> instruments = new List<string>();
            instruments.Add("Clavinet");
            ScanPresets(instruments);
        }
    }
}