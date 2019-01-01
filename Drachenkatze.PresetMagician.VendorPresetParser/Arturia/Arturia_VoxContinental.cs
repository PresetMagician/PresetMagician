using System.Collections.Generic;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia
{
    public class Arturia_VoxContinental: Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1450145842 };

        public void ScanBanks()
        {
            List<string> instruments = new List<string>();
            instruments.Add("VOX Continental");
            ScanPresets(instruments);
        }
    }
}