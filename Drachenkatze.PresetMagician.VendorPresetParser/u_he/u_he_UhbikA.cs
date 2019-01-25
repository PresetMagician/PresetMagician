using System.Collections.Generic;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.u_he
{
    public class u_he_UhbikA : u_he, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1432568113};

        public void ScanBanks()
        {
            H2PScanBanks("Uhbik.data", "Uhbik-A", false);
            H2PScanBanks("Uhbik.data", "Uhbik-A", true);
        }
    }
}