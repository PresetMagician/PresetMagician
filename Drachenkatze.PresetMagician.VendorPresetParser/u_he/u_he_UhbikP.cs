using System.Collections.Generic;

namespace Drachenkatze.PresetMagician.VendorPresetParser.u_he
{
    public class u_he_UhbikP : u_he, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1432571953};

        public void ScanBanks()
        {
            H2PScanBanks("Uhbik.data", "Uhbik-P", false);
            H2PScanBanks("Uhbik.data", "Uhbik-P", true);
        }
    }
}