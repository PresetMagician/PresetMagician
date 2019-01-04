using System.Collections.Generic;

namespace Drachenkatze.PresetMagician.VendorPresetParser.u_he
{
    internal class u_he_Bazille : u_he, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1433421876 };

        public void ScanBanks()
        {
            H2PScanBanks("Bazille.data", "Bazille", false);
            H2PScanBanks("Bazille.data", "Bazille", true);
        }
    }
}