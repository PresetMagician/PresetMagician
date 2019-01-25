using System.Collections.Generic;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.u_he
{
    public class u_he_UhbikT : u_he, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1432572977};

        public void ScanBanks()
        {
            H2PScanBanks("Uhbik.data", "Uhbik-T", false);
            H2PScanBanks("Uhbik.data", "Uhbik-T", true);
        }
    }
}