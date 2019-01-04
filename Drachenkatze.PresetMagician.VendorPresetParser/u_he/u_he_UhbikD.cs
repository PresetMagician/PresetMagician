using System.Collections.Generic;

namespace Drachenkatze.PresetMagician.VendorPresetParser.u_he
{
    public class u_he_UhbikD : u_he, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1432568881 };

        public void ScanBanks()
        {
            H2PScanBanks("Uhbik.data", "Uhbik-D", false);
            H2PScanBanks("Uhbik.data", "Uhbik-D", true);
        }
    }
}