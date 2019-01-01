using System.Collections.Generic;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia
{
    public class Arturia_Matrix12: Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1298232370 };

        public void ScanBanks()
        {
            List<string> instruments = new List<string>();
            instruments.Add("Matrix-12");
            ScanPresets(instruments);
        }
    }
}