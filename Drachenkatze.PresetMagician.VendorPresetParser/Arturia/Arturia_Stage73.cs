using System.Collections.Generic;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia
{
    public class Arturia_Stage73: Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1400136039 };

        public void ScanBanks()
        {
            List<string> instruments = new List<string>();
            instruments.Add("Stage-73");
            ScanPresets(instruments);
        }
    }
}