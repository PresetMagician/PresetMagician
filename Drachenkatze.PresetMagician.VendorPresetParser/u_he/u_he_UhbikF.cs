using System.Collections.Generic;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.u_he
{
    public class u_he_UhbikF : u_he, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1432569393};

        public void ScanBanks()
        {
            H2PScanBanks("Uhbik.data", "Uhbik-F", false);
            H2PScanBanks("Uhbik.data", "Uhbik-F", true);
        }
    }
}