using System.Collections.Generic;

namespace Drachenkatze.PresetMagician.VendorPresetParser.u_he
{
    internal class u_he_ZebraCM : u_he, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1516593997 };

        public void ScanBanks()
        {
            H2PScanBanks("ZebraCM.data", "ZebraCM", false);
        }
    }
}