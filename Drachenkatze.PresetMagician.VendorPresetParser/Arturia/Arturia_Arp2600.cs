using System.Collections.Generic;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia
{
    public class Arturia_Arp2600: Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1095913523 };

        public void ScanBanks()
        {
            List<string> instruments = new List<string>();
            instruments.Add("ARP 2600");
            ScanPresets(instruments);
        }
    }
}