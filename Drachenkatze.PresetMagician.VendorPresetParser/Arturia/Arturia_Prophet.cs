using System.Collections.Generic;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia
{
    public class Arturia_Prophet: Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1347571507 };

        public void ScanBanks()
        {
            List<string> instruments = new List<string>();
            instruments.Add("Prophet");
            ScanPresets(instruments);
        }
    }
}