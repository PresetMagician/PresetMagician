
using System.Collections.Generic;

namespace Drachenkatze.PresetMagician.VendorPresetParser.u_he
{
    internal class u_he_BazilleCM : u_he, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1967276877 };

        public void ScanBanks()
        {
            H2PScanBanks("BazilleCM.data", "BazilleCM", false);
            H2PScanBanks("Bazille.data", "Bazille", true);
        }
    }
}