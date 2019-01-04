using System.Collections.Generic;

namespace Drachenkatze.PresetMagician.VendorPresetParser.u_he
{
    public class u_he_Protoverb : u_he, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1969770582 };

        public void ScanBanks()
        {
            H2PScanBanks("Protoverb.data", "Protoverb", false);
            H2PScanBanks("Protoverb.data", "Protoverb", true);
        }
    }
}