using System.Collections.Generic;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.u_he
{
    public class u_he_Presswerk : u_he, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1969770583};

        public void ScanBanks()
        {
            H2PScanBanks("Presswerk.data", "Presswerk", false);
            H2PScanBanks("Presswerk.data", "Presswerk", true);
        }
    }
}