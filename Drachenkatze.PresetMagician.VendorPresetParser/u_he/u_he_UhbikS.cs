using System.Collections.Generic;

namespace Drachenkatze.PresetMagician.VendorPresetParser.u_he
{
    public class u_he_UhbikS : u_he, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1432572721};

        public void ScanBanks()
        {
            H2PScanBanks("Uhbik.data", "Uhbik-S", false);
            H2PScanBanks("Uhbik.data", "Uhbik-S", true);
        }
    }
}