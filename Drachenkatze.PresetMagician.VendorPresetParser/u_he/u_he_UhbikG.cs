using System.Collections.Generic;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.u_he
{
    public class u_he_UhbikG : u_he, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1432569649};

        public void ScanBanks()
        {
            H2PScanBanks("Uhbik.data", "Uhbik-G", false);
            H2PScanBanks("Uhbik.data", "Uhbik-G", true);
        }
    }
}