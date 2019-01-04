using System.Collections.Generic;

namespace Drachenkatze.PresetMagician.VendorPresetParser.u_he
{
    public class u_he_Filterscape : u_he, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1095583057 };

        public void ScanBanks()
        {
            H2PScanBanks("Filterscape.data", "Filterscape", false);
            H2PScanBanks("Filterscape.data", "Filterscape", true);
        }
    }
}