using System.Collections.Generic;

namespace Drachenkatze.PresetMagician.VendorPresetParser.u_he
{
    public class u_he_FilterscapeVA : u_he, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1179866689};

        public void ScanBanks()
        {
            H2PScanBanks("Filterscape.data", "FilterscapeVA", false);
            H2PScanBanks("Filterscape.data", "FilterscapeVA", true);
        }
    }
}