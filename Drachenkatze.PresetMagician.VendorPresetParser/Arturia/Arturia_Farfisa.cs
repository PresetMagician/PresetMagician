using System.Collections.Generic;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia
{
    public class Arturia_Farfisa: Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1180791398 };

        public void ScanBanks()
        {
            List<string> instruments = new List<string>();
            instruments.Add("Farfisa");
            ScanPresets(instruments);
        }
    }
}