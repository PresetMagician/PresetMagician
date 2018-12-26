using System.Collections.Generic;

namespace Drachenkatze.PresetMagician.VendorPresetParser.u_he
{
    internal class u_he_Diva : u_he, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1147754081 };

        public void ScanBanks()
        {
            H2PScanBanks("Diva.data", "Diva", false);
            H2PScanBanks("Diva.data", "Diva", true);
        }
    }
}