using System.Collections.Generic;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.u_he
{
    public class u_he_FilterscapeQ6 : u_he, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1179865398};

        public void ScanBanks()
        {
            H2PScanBanks("Filterscape.data", "FilterscapeQ6", false);
            H2PScanBanks("Filterscape.data", "FilterscapeQ6", true);
        }
    }
}