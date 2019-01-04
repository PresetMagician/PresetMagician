using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia
{
    [UsedImplicitly]
    public class Arturia_Arp2600: Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1095913523 };

        public void ScanBanks()
        {
            var instruments = new List<string> {"ARP 2600"};
            ScanPresets(instruments);
        }
    }
}