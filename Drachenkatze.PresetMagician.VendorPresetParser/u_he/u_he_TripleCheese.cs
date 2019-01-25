using System.Collections.Generic;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.u_he
{
    internal class u_he_TripleCheese : u_he, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1667388281};

        public void ScanBanks()
        {
            H2PScanBanks("TripleCheese.data", "TripleCheese", false);
            H2PScanBanks("TripleCheese.data", "TripleCheese", true);
        }
    }
}