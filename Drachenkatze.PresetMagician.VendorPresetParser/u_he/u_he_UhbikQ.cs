using System.Collections.Generic;

namespace Drachenkatze.PresetMagician.VendorPresetParser.u_he
{
    public class u_he_UhbikQ : u_he, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1432572209};

        public void ScanBanks()
        {
            H2PScanBanks("Uhbik.data", "Uhbik-Q", false);
            H2PScanBanks("Uhbik.data", "Uhbik-Q", true);
        }
    }
}